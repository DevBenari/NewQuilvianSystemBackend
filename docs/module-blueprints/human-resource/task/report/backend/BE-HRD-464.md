# Laporan Perubahan Backend — `BE-HRD-464`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-464` |
| Judul | Pengecualian konversi tidak memblokir tutup dan diselesaikan saat tutup |
| Slice | `A52-B` / `MVP-77` gelombang 1 · `EPIC HRD-132` (`MUST HAVE`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 32, kartu `BE-HRD-464` |
| Trace | `FR-PK-034`; `HRD-DEC-249`; fakta `HRD-TF-093` |
| Contract version | `v31` — API 37.3; Validation `VR-PK-31`; State 35.2; Integration `INT-HR-120`; desain `02` 39.4, 38.9.2, 38.10, 38.11 — approved 24 November 2026 |
| Dependency | Kontrak `v31` ✅ |
| Klasifikasi | `STANDARD` — aturan service dalam transaksi yang ada, perluasan dua DTO respons, satu teks log; tanpa perubahan skema |
| Task mode | `BACKEND` (instruksi pengguna 25 November 2026: "lanjutkan BE-HRD-464") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `4d2c0162` (branch `yoga`). Working tree sebelum task memuat `BE-HRD-463` (belum di-commit) dan dokumen amandemen `A52-B`; keduanya tidak disentuh task ini |
| Tanggal | 25 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-468` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `AttendanceManagement` (entity `Hrd*` yang sudah ada; task tidak menambah entity sehingga tidak butuh keputusan prefix) |
| Keberlakuan | `TOUCHED LEGACY` — `AttendancePeriodService` (pratinjau, `CloseAsync`, satu helper kueri), `AttendancePeriodDtos` (dua DTO respons), `AttendancePeriodController` (teks log aksi tutup) |
| QBE yang berlaku | `QBE-SVC-001` (aturan pemblokir dan penutupan pengecualian berada di service; controller hanya menulis log), `QBE-API-001` (respons `ApiResponse`, kode status tidak berubah), `QBE-PERM-001` (hak `AttendancePeriod : Read/Close` tidak berubah). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. `<summary>` hanya pada properti DTO (schema), tidak ada pada action controller |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: periode presensi ICU Februari 2027 (`ATP-2027-02`). Rina lupa scan pulang pada 10 Februari. Rekap hari itu `IncompletePunch` dan pemrosesan presensi membuat pengecualian `MissingCheckOut` yang memblokir payroll.

| Langkah | Yang terjadi |
| --- | --- |
| Admin HRD membuka pratinjau tutup (`GET periods/{id}/close-preview`) | Pengecualian Rina tidak lagi dihitung di `openPayrollBlockingException`, dan isu `PAYROLL_BLOCKING_EXCEPTION` tidak muncul. `conversionExceptionCount = 1`. Hari Rina tetap tercantum di daftar calon konversi (peringatan `INCOMPLETE_PUNCH_ATTENDANCE`), dan `canClose = true` |
| Admin HRD menutup periode (`POST periods/{id}/close`) | Dalam satu transaksi `Serializable`, rekap Rina menjadi `Absent`. Pengecualian `MissingCheckOut` menjadi `Closed` dengan `resolvedByUserId` = penutup, `resolvedAt` = jam tutup, dan catatan "Diselesaikan oleh penutupan periode ATP-2027-02 (HRD-DEC-249): hari dikonversi menjadi Absent". Periode menjadi `Closed`. Respons memuat `closedConversionExceptionCount = 1`, dan pesan menyebut jumlahnya |
| Log aplikasi | `AttendancePeriod.Close`: "Menutup attendance period ATP-2027-02: 1 hari dikonversi menjadi Absent, 1 pengecualian konversi ditutup (HRD-DEC-249)." |
| Budi punya `ScheduleConflict` terbuka pada hari `IncompletePunch` | Tetap memblokir: pratinjau `canClose = false`, tutup `409` "Masih terdapat attendance exception yang memblokir payroll." Tidak ada yang berubah |
| Dodi punya `MissingCheckOut` terbuka pada hari `Present` (bukan calon konversi) | Tetap memblokir, `409` |
| Database gagal di tengah tutup | Seluruhnya batal: periode tetap `Open`, rekap tetap `IncompletePunch`, pengecualian tetap `Open` |
| Periode dibuka kembali | Pengecualian tetap `Closed` dan rekap tetap `Absent`. Koreksi dilakukan lewat koreksi presensi |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendancePeriodService.cs` | Konstanta `ConversionExceptionTypes` (`MissingCheckIn`, `MissingCheckOut`, `Absent`). Helper baru `ConversionExceptions(scopedDailies)`: pengecualian belum dihapus, `Open`/`UnderReview`, bertipe konversi, pada rekap calon konversi (`CloseConversionStatuses`) dalam cakupan periode. Pratinjau: hitungan `PAYROLL_BLOCKING_EXCEPTION` mengecualikan pengecualian bertipe konversi pada calon konversi, dan `ConversionExceptionCount` diisi dari helper. `CloseAsync`: pengecualian konversi dibaca sebelum rekap diubah, lalu ditutup pada `SaveChanges` kedua yang sama dengan konversi dan status `Closed` (`ExceptionStatus`, `ResolvedByUserId`, `ResolvedAt`, `ResolutionNote`, `UpdateBy`, `UpdateDateTime`). Respons mengisi `ClosedConversionExceptionCount`, dan pesan sukses menyebut jumlahnya |
| `…/DTOs/AttendancePeriodDtos.cs` | `AttendancePeriodClosePreviewResponse.ConversionExceptionCount`, `AttendancePeriodActionResponse.ClosedConversionExceptionCount` (keduanya `int`, ber-`<summary>`) |
| `…/Controllers/AttendancePeriodController.cs` | Teks log `AttendancePeriod.Close` memuat kode periode, jumlah hari yang dikonversi, dan jumlah pengecualian yang ditutup |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AttendancePeriodCloseConversionExceptionTests.cs` (baru), `AcceptanceTestIndex.md` | 7 uji (4 fakta + 1 teori 3 kasus) lewat `AttendancePeriodService` nyata; baris `AT-HRD-PK-48` dan baris kelas di indeks |

`ReopenAsync` tidak diubah. Perilaku yang ada sudah memenuhi AC4 (hanya membuka kunci rekap), dan task ini membuktikannya.

### Delta terhadap kartu roadmap

| Hal | Kartu | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| `AttendancePeriodController.cs` | Tidak termasuk cakupan | Satu pemanggilan log diubah | Outcome meminta log `AttendancePeriod.Close` memuat jumlahnya. `LoggerService` tidak menulis objek payload, hanya teks pesan, jadi jumlahnya harus masuk ke teks |
| Pengecualian konversi yang tidak memblokir payroll (`IsPayrollBlocking = false`) | Tidak disebut | Ikut dihitung di `ConversionExceptionCount` dan ikut ditutup | Desain 38.9.2 mendefinisikan pengecualian konversi dari tipe, status, dan hari, bukan dari penanda payroll. Hari itu menjadi `Absent`, jadi pengecualiannya tidak lagi bermakna |
| Pesan sukses tutup | Tidak disebut | Menyebut jumlah pengecualian yang ditutup | Supaya hasil di layar sama dengan isi log; ruas `closedConversionExceptionCount` tetap sumber angka untuk frontend |
| Status kontrak API 37.3 | Tabel masih "Sudah ada — perubahan **Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `AttendancePeriodCloseConversionExceptionTests` + `ClosePeriodTests` (`BE-HRD-344`) | 13/13 lulus | `PASS` (AC1–AC4, regresi tutup periode) | Keluaran `dotnet test` |
| Kontrol negatif: 10 mutasi — (1) filter tipe pada hitungan pemblokir dibuang, kontrol yang diminta kartu; (2) syarat hari calon konversi pada hitungan pemblokir dibuang; (3) pengecualian tidak ditutup, kontrol yang diminta kartu; (4) helper tanpa filter tipe; (5) helper tanpa filter status; (6) helper menyertakan yang dihapus; (7) catatan penyelesaian kosong; (8) `closedConversionExceptionCount` selalu 0; (9) penutup tidak dicatat; (10) `conversionExceptionCount` selalu 0 | 10/10 tertangkap: (1) AC2 `ScheduleConflict` gagal; (2) AC2 hari bukan calon konversi, 2 kasus; (3) AC1, ketiga tipe, dan AC4; (4) AC2 `ScheduleConflict` (hitungan konversi); (5) dan (6) uji ketiga tipe (yang sudah selesai/dihapus ikut ditutup); (7) AC1; (8) dan (9) AC1 + ketiga tipe; (10) 5 uji. Tanpa galat kompilasi. Source dipulihkan dengan `writeFileSync` (mtime baru), hash SHA-256 `e6525df7…` sama dengan salinan asli | `PASS` | `mut451.cjs`, `mut451-hasil.log` di scratchpad sesi |
| Regresi penuh proyek uji HR | 430/430 lulus (12 menit 49 detik) sesudah build ulang dari source asli, dengan env `QUILVIAN_BACKEND_ROOT`. Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`; salinan selain itu identik dengan folder uji repo | `PASS` | `test451-full.log` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | `build451-final.log` |
| QBE `Strict` atas 4 berkas | 0 violation, 0 review. Probe `TrxZzzQbeProbe451` (ber-`[Table]`) terdeteksi 4 violation (`QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`) lalu dihapus | `PASS` | Keluaran checker |
| Bukti API data asli devYoga (`AT-HRD-PK-48`, ulang `UAT-PK-09`) | Tidak dijalankan di task ini | `NOT RUN` | Kartu menempatkannya di `BE-HRD-468` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Periode dengan satu `IncompletePunch` + `MissingCheckOut`: pratinjau tanpa isu pemblokir, `conversionExceptionCount = 1`; tutup `200`, rekap `Absent`, pengecualian `Closed` dengan catatan, `closedConversionExceptionCount = 1` | Terpenuhi | `Belum_Lengkap_dengan_MissingCheckOut_tidak_memblokir_dan_ditutup_saat_tutup`; ketiga tipe dan kedua status: `Ketiga_tipe_pada_calon_konversi_ditutup_dan_yang_lain_tidak_disentuh` |
| 2. `ScheduleConflict` terbuka atau `MissingCheckOut` pada hari bukan calon konversi → tetap `409` | Terpenuhi | `Pengecualian_lain_tetap_memblokir_tutup` (3 kasus: `ScheduleConflict` pada `IncompletePunch`, `MissingCheckOut` pada `Present`, `MissingCheckIn` pada `Late`) |
| 3. Galat di tengah transaksi → periode, rekap, dan pengecualian tidak berubah | Terpenuhi | `Galat_di_tengah_transaksi_tidak_mengubah_pengecualian` (interceptor menggagalkan `SaveChanges` kedua) |
| 4. Buka kembali periode tidak membuka ulang pengecualian | Terpenuhi | `Buka_kembali_tidak_membuka_ulang_pengecualian` |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: setiap pengecualian terbuka yang memblokir payroll menghalangi tutup, termasuk `MissingCheckOut` pada hari yang pasti dikonversi menjadi `Absent` saat tutup (`HRD-TF-093`) | Periode dengan hari Belum Lengkap tidak bisa ditutup tanpa menyelesaikan pengecualiannya satu per satu, padahal tutup sendiri yang memutuskan hari itu | Diperbaiki (inti task) |
| As-is: log `AttendancePeriod.Close` hanya berisi "Menutup attendance period." karena payload diabaikan `LoggerService` | Jejak tutup tidak memuat kode periode maupun jumlah | Diperbaiki untuk aksi tutup; aksi lain di controller ini tidak disentuh |
| Pengecualian tertutup otomatis bisa menyembunyikan masalah data | Risiko kartu | Dijaga catatan penyelesaian yang menyebut periode dan keputusan, pelaku penutup, jam tutup, serta log jumlah |
| Buka kembali tidak membatalkan konversi maupun membuka ulang pengecualian | Perilaku yang disengaja (desain 38.9.2 butir 5) | Koreksi lewat koreksi presensi |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `AttendancePeriodCloseConversionExceptionTests.cs` (cukup `git add`) |
| Dampak frontend | Dua ruas baru tanpa menghapus ruas lama. Pratinjau dapat menyebut `conversionExceptionCount` pada konfirmasi, dan hasil tutup menyebut `closedConversionExceptionCount` (`FE-HRD-412`) |
| Bukti data asli | `AT-HRD-PK-48` / ulang `UAT-PK-09` di devYoga dikerjakan di `BE-HRD-468` |
| Task berikutnya | `BE-HRD-465` (cakupan unit Perubahan Shift dan Tukar Dinas sisi HR); `FE-HRD-412` dapat mulai memakai ruas baru ini |
