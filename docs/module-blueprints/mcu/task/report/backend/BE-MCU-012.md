# Laporan Perubahan Backend — `BE-MCU-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-012` |
| Judul | Tutup hari: *Tidak Hadir* |
| Slice | `MVP-1` — `EPIC MCU-02` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-012`* |
| Trace | `FR-MCU-015`; `MCU-DEC-014`, `MCU-DEC-030`; `NFR-007` |
| Contract version | `MCU-STATE-v1` (*Terjadwal → Tidak Hadir*, sistem); `02-backend-architecture.md` §5.5 dan §13; `MCU-INT-v1` (tutup hari gagal sebagian) — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-011` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), persistence yang sudah ada (1), workflow terbatas (1). Risiko utamanya — pekerjaan latar yang menulis basis data bersama saat start — ditangani lewat saklar |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/Services/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability; koreksi urutan komponen di `McuBookingService.cs` dan `McuPackageService.cs` (§3.4) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-13` dan seluruh kriteria terbukti dengan jam tiruan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — registrasi service dan hosted service di blok `runBackgroundJobs` |
| QBE yang berlaku | `QBE-SVC-001` (logika di `McuNoShowService`; hosted service hanya penjadwal), `QBE-TXN-001` (satu transaksi per order: lepas kursi + status + riwayat), `QBE-LOG-001` (riwayat `McuOrderStatusHistory` dengan pelaku sistem; log aplikasi ringkasan jumlah, tanpa data peserta), `QBE-DEL-001` (tidak ada hapus) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Peserta yang dibooking tetapi tidak datang tetap *Terjadwal* selamanya dan tetap memakan kuota, sehingga
kursi yang sebenarnya kosong tidak dapat dipakai dan Daftar Pasien MCU menampilkan order yang tidak akan
pernah dilayani.

**Contoh.** Budi booking 27-10-2026 dan tidak datang. Pukul 23.59 WIB 27 Oktober ordernya menjadi *Tidak
Hadir*, kuota 27 Oktober kembali 29/30, dan riwayat mencatat *"Tutup hari: peserta tidak check-in."* oleh
sistem. Siti yang H-1 sudah membalas *YA* lewat WhatsApp tetapi tetap tidak datang juga menjadi *Tidak
Hadir* — konfirmasi datang bukan check-in (`MCU-DEC-030`). Server yang mati dari 27 sampai 29 pagi
mengejar seluruh tanggal yang terlewat begitu menyala.

---

## 2. Proses bisnis

1. Saat aplikasi menyala dan setiap hari pukul `MedicalCheckup:NoShowCutoffLocalTime` (bawaan 23.59 WIB),
   `McuNoShowHostedService` memanggil `McuNoShowService.CloseOverdueAsync`.
2. Tanggal yang sudah ditutup = hari ini bila sudah lewat jam tutup, selain itu kemarin.
3. Setiap order *Terjadwal* bertanggal ≤ tanggal itu diproses dalam transaksinya sendiri: kursi dilepas,
   status *Tidak Hadir*, `NoShowAt` diisi, `Version` naik, satu riwayat oleh sistem.
4. Petugas kemudian dapat menjadwal ulang order yang sama (`BE-MCU-011`).

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Order sudah *Hadir*, *Dibatalkan*, atau bertanggal sesudahnya | Tidak disentuh |
| Konfirmasi datang WhatsApp tanpa check-in | Tetap menjadi *Tidak Hadir* |
| Dijalankan dua kali | Jalan kedua tidak mengubah apa pun |
| Sebelum jam tutup | Hari itu belum ditutup; hanya tanggal sebelumnya |
| Server mati beberapa hari | Seluruh tanggal terlewat dikejar pada jalan berikutnya |
| Satu order gagal disimpan (mis. sedang di-check-in pada detik yang sama) | Dilewati, dihitung gagal, dicoba siklus berikutnya; order lain tetap diproses |
| Order sudah berubah status sejak daftar dibaca | Dilewati tanpa perubahan |
| `MedicalCheckup:NoShowEnabled = false` | Hosted service berhenti tanpa menyentuh basis data |
| Proses `Web` saja (tanpa background job) | Hosted service tidak didaftarkan — mengikuti blok `runBackgroundJobs` |
| Basis data belum menerima migration MCU | Kesalahan ditangkap dan dicatat; aplikasi tidak berhenti; dicoba lagi pada jam tutup berikutnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`02-backend-architecture.md` §5.5 dan §13, `contracts/state-transition-matrix.md`,
`contracts/integration-contract.md`, `testing/acceptance-test-matrix.md` (`AT-MCU-13`),
`00-interview-decisions.md` (`MCU-DEC-014`, `030`), `PatientMembershipLifecycleHostedService.cs` (pola
acuan), `Program.cs` (blok `runBackgroundJobs`), `AppDateTimeHelper.cs`, `NumberSeriesAllocator.cs` (zona
waktu bisnis), `McuScheduleService.cs`, `McuBookingService.cs`, `McuPackageService.cs`, `McuOrderQueryService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuNoShowService.cs` (baru) | `McuNoShowOptions`, `McuNoShowResult`, `ClosedThroughDate`, `CloseOverdueAsync` per order |
| `MedicalCheckupManagement/Services/McuNoShowHostedService.cs` (baru) | Jalan saat start lalu tiap jam tutup; saklar; `DelayUntilNextRun` |
| `Program.cs` | `AddScoped<McuNoShowService>()`; `Configure<McuNoShowOptions>` + `AddHostedService<McuNoShowHostedService>()` di blok `runBackgroundJobs` |
| `MedicalCheckupManagement/Services/McuBookingService.cs` | Koreksi urutan potret komponen order (§3.4) |
| `MedicalCheckupManagement/Services/McuPackageService.cs` | Koreksi urutan komponen paket (§3.4) |
| `docs/.../task/report/backend/BE-MCU-005.md`, `BE-MCU-011.md` | Catatan koreksi pada baris *Masalah yang diketahui* |
| `Tests/QuilvianSystemBackend.McuTests/McuNoShowServiceTests.cs` (baru) | 9 uji dengan jam tiruan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tanpa endpoint |
| Database | Tanpa migration. Hosted service **menulis** ke basis data saat aplikasi menyala bila saklar menyala; dalam task ini tidak dijalankan terhadap basis data mana pun |
| Keamanan/Auth | Tidak ada izin baru. Riwayat bertanda pelaku sistem (`ChangedByUserId` kosong). Log aplikasi hanya tanggal dan jumlah |
| Konfigurasi | `MedicalCheckup:NoShowEnabled` (bawaan `true`) dan `MedicalCheckup:NoShowCutoffLocalTime` (bawaan `23:59:00`). Tidak ditambahkan ke `appsettings.json`; bawaan di kode, sama dengan `PatientMembership:Lifecycle` |

### 3.4 Delta terhadap kontrak dan koreksi task sebelumnya

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Nama saklar | *Ada saklar konfigurasi* | `MedicalCheckup:NoShowEnabled` | Sejajar dengan kunci jam tutup `MedicalCheckup:NoShowCutoffLocalTime` dari arsitektur §13 |
| Pelaku riwayat | *Oleh sistem* | `ChangedByUserId` kosong + alasan tetap | Tab Riwayat menampilkan pelaku kosong sebagai sistem |
| **Koreksi `BE-MCU-011`** | — | Potret komponen order diurutkan pasti (jenis, waktu dibuat di paket, nama) dan diberi `CreateDateTime` berurutan | Sebelumnya semua komponen mendapat cap waktu kembar; urutan di detail order dapat tertukar. Ditemukan karena uji `AT_MCU_08_…` gagal sekali pada putaran ini |
| **Koreksi `BE-MCU-005`** | — | `CreateDateTime` komponen paket dinaikkan per komponen | Cacat yang sama pada detail paket |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — pekerjaan latar tanpa endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama), putaran pertama | 136 lulus, **1 gagal**: `AT_MCU_08_…` (`BE-MCU-011`) — urutan komponen tertukar | `NEW ERROR` (cacat laten `BE-MCU-011`, bukan disebabkan task ini) | Keluaran uji |
| Sesudah koreksi urutan | 137 lulus (128 lama + 9 baru) | `PASS` | `Passed! - Failed: 0, Passed: 137` |
| Kelas uji booking + paket diulang 3× tanpa build | 42/42 lulus ketiganya | `PASS` | Keluaran uji |
| `AT-MCU-13` — 27-10 23.59 WIB | Order dengan dan tanpa konfirmasi WhatsApp → *Tidak Hadir* (`NoShowAt`, `Version` 1); order *Hadir*, *Dibatalkan*, dan besok tidak berubah; kuota 27-10 3 → 1; riwayat *Terjadwal → Tidak Hadir* oleh sistem beralasan; nol kunjungan | `PASS` | `AT_MCU_13_…` |
| Idempoten | Jalan kedua 0 order; kuota, `Version`, jumlah riwayat tetap | `PASS` | `Dijalankan_dua_kali_…` |
| Jam tutup | 23.58 → ditutup sampai kemarin saja | `PASS` | `Sebelum_jam_tutup_…` |
| Mengejar | 29-10 08.00 → 27 dan 28 ditutup | `PASS` | `Server_yang_mati_…` |
| Jeda ke jam tutup berikutnya | 09.00 → 14 j 59 m; 23.59 → 24 j; 23.58 → 1 m | `PASS` | `Jeda_sampai_…` (3 kasus) |
| Saklar mati | Hosted service selesai tanpa membuka scope basis data | `PASS` | `Saklar_mati_…` |
| Saklar menyala | Hosted service menutup hari saat start (jam tiruan 28-10 00.30) lalu berhenti bersih | `PASS` | `Saklar_menyala_…` |
| Checker QBE `-Path` (5 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Kegagalan sebagian per order | Jalur `DbUpdateException` → dihitung gagal dan dilewati; tidak disimulasikan | `NOT RUN` | Butuh balapan check-in sungguhan di PostgreSQL |
| Hosted service pada aplikasi berjalan terhadap devYoga | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-13` — contoh Budi: 23.59 WIB 27 Okt → *Tidak Hadir*, kuota dilepas, riwayat oleh sistem | Terpenuhi | §5 |
| Dijalankan dua kali tidak mengubah apa pun lagi | Terpenuhi | §5 |
| Ada saklar konfigurasi untuk mematikannya saat uji modul lain | Terpenuhi — `MedicalCheckup:NoShowEnabled` | §5 |
| Verifikasi: uji dengan jam tiruan; uji idempoten | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Saklar bawaan menyala.** Saat migration MCU diterapkan ke devYoga dan backend lokal dinyalakan dengan background job, order *Terjadwal* bertanggal lampau di devYoga langsung menjadi *Tidak Hadir*. Untuk menguji modul lain tanpa efek ini, jalankan dengan `MedicalCheckup__NoShowEnabled=false` |
| Masalah yang diketahui | Tidak ada (cacat urutan komponen `BE-MCU-005`/`011` sudah dikoreksi, §3.4) |
| Risiko tersisa | Kegagalan sebagian per order belum disimulasikan; startup backend ke devYoga masih menunggu migration `BE-MCU-002`/`003` |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` |
| Status Git | Perubahan task: ` M Program.cs`; `??` dua berkas service baru; perubahan pada `McuBookingService.cs` dan `McuPackageService.cs` (berkas task sebelumnya, belum di-commit); laporan ini; catatan koreksi di laporan `BE-MCU-005`/`011`; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-013` (check-in admisi) dan `BE-MCU-020` siap |
