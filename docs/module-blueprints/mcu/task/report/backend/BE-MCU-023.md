# Laporan Perubahan Backend — `BE-MCU-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-023` |
| Judul | Isi Pemeriksaan MCU |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-023`* |
| Trace | `FR-MCU-036`; `MCU-DEC-007`, `018`, `035`, `041` |
| Contract version | `MCU-API-v1` grup Mcu Examination (`GET/PUT components/{componentId}/examination-result`); `MCU-VAL-v1` `VAL-MCU-36`, `42`, `43`, `44`, `46`; `MCU-STATE-v1` 1.1 (komponen terakhir tuntas), 2.1–2.2; `MCU-PERM-v1` (`McuExamination : Read/Update`); `MCU-INT-v1` `INT-MCU-11` — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-022` ✅, `BE-MCU-001` ✅ (bukti devYoga `MCU-UNK-05` sudah ditutup di sana: `AT-MCU-RUN-02`) |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), dua endpoint baru (2), data sensitif (1), konkurensi (1), lintas modul baca Registrasi (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `McuExaminationService.cs`, `McuExaminationController.cs`, `McuExaminationDtos.cs`, `McuOperationResult.cs`; laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-33`, `34`, `35` terbukti |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (Registrasi hanya dibaca: `RegQueue.ScreeningCompletedAt`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuExamination : Read/Update`), `QBE-LOG-001` (tanpa nilai/kesimpulan), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CODE-001`..`006`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 4 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Komponen *Pemeriksaan MCU* (Pemeriksaan Fisik, EKG, Visus, …) sudah *Siap diisi* sejak `BE-MCU-021`, tetapi
belum ada tempat mengisi hasilnya, sehingga order tidak pernah bisa tuntas menuju resume.

**Contoh.** Perawat belum menyelesaikan skrining Budi → dr. Rina membuka Pemeriksaan Fisik: isian tampil baca-saja
dengan *"Skrining awal peserta belum selesai."*, simpan ditolak `409`. Sesudah skrining, dr. Rina mengisi berat
`64,5` → tersimpan `64.5`, bukan `645`. EKG *irama sinus, 78 x/menit, kesan normal* diselesaikan tanpa lampiran;
karena itu komponen terakhir, order menjadi *Menunggu Resume*.

---

## 2. Proses bisnis

1. Petugas/dokter MCU membuka komponen *Pemeriksaan MCU*: isian menurut nomor butir (jenis, satuan, pilihan,
   wajib), nilai tersimpan, kesimpulan, status skrining, dan alasan baca-saja bila ada.
2. **Simpan Draf** (`complete = false`): nilai yang dikirim disimpan; isian wajib belum diperiksa.
3. **Simpan & Selesaikan** (`complete = true`): semua isian wajib harus terisi; komponen *Selesai*, dokter
   pemeriksa tercatat dari akun penyimpan. Bila semua komponen order tuntas, order *Menunggu Resume*.
4. Sesudah resume disetujui, isian terkunci. Sesudah dokter membuka ulang resume, isian dapat diubah dan setiap
   nilai yang berubah mencatat nilai lamanya lebih dulu.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Skrining kunjungan belum selesai | `409` `VAL-MCU-36` *"Skrining awal peserta belum selesai."* |
| Isian wajib kosong saat diselesaikan | `400` `VAL-MCU-42` *"Isi Tekanan darah sistolik."* (butir terkecil yang kosong) |
| Angka bukan desimal (`enam`, `1.234,5`, `1e3`) | `400` `VAL-MCU-43` *"Nilai harus berupa angka, contoh 6.4."* |
| Resume berlaku *Disetujui*/*Terverifikasi* | `409` `VAL-MCU-44` *"Hasil terkunci. Minta dokter membuka ulang resume."* |
| Komponen Lab/Rad | `400` *"Hasil Laboratorium/Radiologi diisi di modulnya."* (`MCU-STATE-v1` 2.2) |
| Komponen belum siap / dijadwal ulang / dibatalkan; order belum dimulai | `409` dengan alasannya |
| Pilihan di luar daftar; Ya/Tidak lain; isian bukan milik pemeriksaan ini | `400` dengan alasannya |
| Simpan bersamaan | `409` `VAL-MCU-46`; tidak ada hasil setengah tersimpan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-023`), `contracts/api-contract.md`, `validation-matrix.md`,
`state-transition-matrix.md`, `permission-audit-matrix.md` (audit revisi tanpa nilai), `02-backend-architecture.md`
(model hasil, kolom sensitif), `05-screen-brief.md` 5.6, `00-interview-decisions.md` (`MCU-DEC-018`, `028`, `035`),
`01-existing-capability-map.md` (`MCU-CAP-29`, `MCU-UNK-05`), laporan `BE-MCU-001` (`AT-MCU-RUN-02`), model
`McuExaminationResult*`, `McuExaminationField`, `McuResume`, `RegQueue`, `McuReferenceDataService` (format pilihan),
konfigurasi index unik hasil.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` | `GetResultAsync`, `SaveResultAsync`; penolong `BuildResultAsync`, `ReadOnlyReasonAsync` (urutan: kunci resume → status → skrining), `IsScreeningCompletedAsync`, `NormalizeValue`, `TryNormalizeNumber`, `IsUniqueViolation` (pola `PostgresErrorCodes.UniqueViolation` modul) |
| `MedicalCheckupManagement/Controllers/McuExaminationController.cs` | `GET`/`PUT components/{componentId}/examination-result`; log tanpa nilai maupun kesimpulan |
| `MedicalCheckupManagement/DTOs/McuExaminationDtos.cs` | `SaveMcuExaminationResultRequest`, `McuExaminationResultValueRequest`, `McuExaminationResultResponse`, `McuExaminationResultFieldResponse` |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-36`, `42`, `43`, `44` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru sesuai kontrak |
| Database | Tanpa migration (tabel hasil sejak `BE-MCU-002`). **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | `McuExamination : Read` untuk baca, `Update` untuk simpan. Nilai dan kesimpulan **tidak** ditulis ke log aplikasi; revisi menyimpan nilai lama di tabelnya sendiri |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Angka berkoma | `VAL-MCU-43` *"desimal titik"*; `AT-MCU-34` *`6,4` dan `6.4`* | Server menerima koma atau titik dan menyimpan bentuk titik; lebih dari satu pemisah ditolak | Memenuhi `AT-MCU-34` tanpa bergantung pada FE; tidak pernah membuang koma |
| Simpan ulang komponen *Selesai* | Tidak disebut | Diperiksa seperti diselesaikan (isian wajib tidak dapat dikosongkan); komponen tetap *Selesai* | Mencegah komponen tuntas kembali bolong tanpa disadari order |
| Isi `SaveMcuExaminationResultRequest` | `values[]`, `conclusion`, `complete` | Sama, tanpa `version` | Kontrak tidak memuatnya; konkurensi dijaga token `Version` order yang dinaikkan setiap simpan |
| Kunci resume | `VAL-MCU-44`: resume berlaku `Approved`/`Verified` | Juga bila order *Disetujui Dokter*/*Terverifikasi*/*Diserahkan* | Status order dan resume selalu sejalan; dua sumber membuat kunci tetap berlaku walau salah satunya tertinggal |
| Revisi nilai | *Nilai lama bila diubah sesudah resume pernah disetujui* | Revisi hanya untuk nilai yang **berubah** pada isian yang sudah punya nilai; `ResumeVersionNumber` = versi resume berlaku | Isian baru tidak punya nilai lama |
| Isian yang dihapus admin | — | Tidak ditampilkan dan tidak dapat diisi; nilainya tetap tersimpan | `McuReferenceDataService` menghapus isian secara lunak |
| Ya/Tidak | — | Disimpan `Ya`/`Tidak` (tanpa beda huruf besar) | Bentuk tetap untuk resume dan cetak |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Examination (`.../orders/{id}`)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/components/{componentId}/examination-result` | Isian + hasil Pemeriksaan MCU, status skrining, alasan baca-saja | `McuExamination : Read` | - | `McuExaminationResultResponse` | Tersedia |
| `PUT` | `/components/{componentId}/examination-result` | Isi/ubah hasil; `complete = true` menyelesaikan komponen | `McuExamination : Update` | `SaveMcuExaminationResultRequest` (values[{examinationFieldId, value}], conclusion, complete) | `McuExaminationResultResponse` | Tersedia |

Kode status: `200`, `400`, `404`, `409`. Log aplikasi: nama pemeriksaan, nomor order, status komponen — tanpa
nilai isian maupun kesimpulan.

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`: SQLite; `McuExaminationService`
produksi dengan `InpatientClinicalContextService` sungguhan; akun dr. Rina tertaut `ApplicationUser.DoctorId`;
antrean `RegQueue` dengan/tanpa `ScreeningCompletedAt`), dijalankan, lalu **dihapus** sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 17 lulus | `PASS` | `Passed! - Failed: 0, Passed: 17` |
| `AT-MCU-33` | Tanpa skrining → `409` `VAL-MCU-36` pesan persis, nol hasil; GET: `isScreeningCompleted = false`, alasan baca-saja sama, isian terurut 1–5 (isian terhapus tidak tampil). Sesudah `ScreeningCompletedAt` → *Selesai*, dokter pemeriksa dr. Rina, pengisi Rina, kesimpulan tersimpan | `PASS` | `AT_MCU_33_…` |
| Bukti devYoga `MCU-UNK-05` | Sudah ditutup di `BE-MCU-001` (`AT-MCU-RUN-02`: `ScreeningCompletedAt` terisi lewat nurse station) — sinyal yang sama dibaca task ini | `PASS` | [`BE-MCU-001`](BE-MCU-001.md) |
| `AT-MCU-34` | `6,4` → `6.4`; `6.4` → `6.4`; ` 64 ` → `64`; `-0,5` → `-0.5` (respons dan baris basis data); simpan draf tetap *Siap diisi*. `enam`, `1.234,5`, `6,4,1`, `1e3` → `400` `VAL-MCU-43`, nol nilai tersimpan | `PASS` | `AT_MCU_34_…` (8 kasus) |
| `AT-MCU-35` | EKG (teks + angka) diselesaikan tanpa lampiran → *Selesai*; sebagai komponen terakhir, order *Dalam Pemeriksaan* → *Menunggu Resume* dengan riwayat sistem | `PASS` | `AT_MCU_35_…` |
| `VAL-MCU-42` dan bentuk isian | Selesaikan tanpa sistolik → *"Isi Tekanan darah sistolik."*, nol hasil; pilihan `Hijau` → daftar pilihan; isian asing ditolak; `normal`/`ya` → `Normal`/`Ya`; komponen *Selesai* tidak dapat dikosongkan lewat draf | `PASS` | `Selesaikan_tanpa_isian_wajib_…` |
| `VAL-MCU-44` | Resume berlaku *Approved* → `409` pesan persis; GET menampilkan alasan yang sama; nilai lama tetap `120` | `PASS` | `Hasil_terkunci_…` |
| Revisi sesudah buka ulang | Resume v1 *Corrected* (pernah disetujui) + v2 *Draft* → ubah sistolik 120 → 130: satu revisi `120`, versi resume 2, pelaku Rina; nilai baru 130. Sebelum pernah disetujui → nol revisi | `PASS` | `Ubah_sesudah_buka_ulang_…`, `Sebelum_resume_disetujui_…` |
| Status dan jenis | Komponen Lab → `400` (simpan dan baca); komponen *Menunggu* → `409`; order *Hadir* → *"Pemeriksaan belum dimulai."*; komponen/order tak dikenal → `404`; nol hasil | `PASS` | `Komponen_lab_belum_siap_…` |
| Simpan bersamaan | Versi order naik tepat sebelum simpan → `409` `VAL-MCU-46`; nol hasil dan nol nilai | `PASS` | `Simpan_bersamaan_…` |
| Endpoint | `GET`/`PUT components/{componentId:guid}/examination-result` = `Read`/`Update`; `PermissionRegistryValidator` nol temuan MCU | `PASS` | `Endpoint_hasil_…` |
| Uji mutasi | Koma dibuang (`6,4` → `64`, cacat `BaseTextField`), kunci skrining dan kunci resume dilepas → 4 uji **gagal**; dipulihkan identik (`cmp`), kembali 17/17 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (4 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| PostgreSQL (index unik hasil saat simpan pertama serentak), `403` lewat HTTP, Swagger | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-33` — `409` sebelum skrining; berhasil sesudahnya (bukti `MCU-UNK-05`) | Terpenuhi | §5 |
| `AT-MCU-34` — `6,4` dan `6.4` tersimpan `6.4`, bukan `64` | Terpenuhi | §5 |
| `AT-MCU-35` — EKG *Selesai* tanpa lampiran | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Skrining dianggap selesai bila **ada** antrean kunjungan dengan `ScreeningCompletedAt`; kunjungan MCU tanpa antrean (klinik MCU tanpa wajib antrean) tidak pernah dapat diisi — master `CLN-MCU-001` di devYoga mewajibkan antrean |
| Masalah yang diketahui | Risiko dari `BE-MCU-001` tetap: sesudah skrining kunjungan MCU *Menunggu Dokter* dan dapat diselesaikan dari layar dokter rawat jalan di luar alur MCU (`MCU-COORD-004`) — tidak memengaruhi isi hasil |
| Risiko tersisa | Tabrakan index unik hasil hanya diterjemahkan ke `409` di PostgreSQL (tidak teruji di SQLite); hak akses `McuExamination` belum diberikan ke jabatan mana pun; startup backend ke devYoga masih menunggu migration MCU |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Tidak ada |
| Status Git | Perubahan task di folder modul MCU (`??`); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-025` (batal penuh) dan `BE-MCU-026` (perluasan refund Billing) siap; `BE-MCU-024` (batal komponen + refund) menunggu `BE-MCU-026` |
