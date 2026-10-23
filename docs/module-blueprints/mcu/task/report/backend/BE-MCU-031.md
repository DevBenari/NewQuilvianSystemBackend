# Laporan Perubahan Backend — `BE-MCU-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-031` |
| Judul | Buka ulang dan versi resume |
| Slice | `MVP-3` — `EPIC MCU-05` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-031`* |
| Trace | `FR-MCU-044`; `MCU-DEC-011`; `MCU-AC-013`; `AT-MCU-43` (tanpa alur penyerahan) |
| Contract version | `MCU-API-v1` grup Mcu Resume (`POST /reopen`, `ReopenMcuResumeRequest`); `MCU-STATE-v1` 1.1 (*Disetujui Dokter/Terverifikasi/Diserahkan → Menunggu Resume*) dan 3 (*Approved/Verified → Corrected* + versi baru *Draft*); `MCU-VAL-v1` `VAL-MCU-38`, `40`, `44`, `46`; `MCU-PERM-v1` (`McuResume : Reopen`) — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-030` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang sudah ada (1), perilaku persistence yang ada — index unik versi berlaku (1), keamanan berkaitan (1), workflow terbatas (1), revisi nilai klinis (1) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuResumeService.cs`, `McuResumeController.cs`, `McuResumeDtos.cs`, `McuExaminationService.cs` (versi revisi). Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-43` (tanpa alur penyerahan) terbukti di harness integrasi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (method dan endpoint baru; perbaikan kecil di service MCU baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuResume : Reopen`), `QBE-LOG-001` (alasan koreksi tidak masuk log), `QBE-VAL-001`, `QBE-TXN-001` (versi lama, versi baru, penyerahan, order satu transaksi), `QBE-DTO-001` |
| Checker QBE | `PASS` — 4 berkas task nol temuan; kontrol negatif `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus. Run pertama berjalan bersamaan dengan `dotnet test` dan tidak menangkap kontrol (sama dengan `BE-MCU-030`); dua run sendirian berikutnya menangkapnya |

---

## 1. Masalah yang diperbaiki

Resume yang sudah disetujui, diverifikasi, atau diserahkan tidak dapat dikoreksi. Kesalahan klinis yang baru
ketahuan sesudah hasil dikirim tidak punya jalan perbaikan yang tercatat.

**Contoh.** Resume Budi versi 1 (*Fit*, Audiometri 20 dB) sudah diserahkan ke Budi dan PT Contoh. dr. Rina
mendapati kesalahan dan membuka ulang dengan alasan *salah baca nilai tekanan darah*:
- versi 1 *Dikoreksi* dan tetap terbaca lengkap (isi, pemberi approve, pemverifikasi, alasan koreksi);
- versi 2 *Draf* berisi salinan isi versi 1;
- kedua penyerahan versi 1 ditandai *perlu diberi tahu*;
- order kembali *Menunggu Resume*, sehingga Audiometri dapat diubah ke 35 dB dan nilai lama 20 dB tercatat sebagai
  revisi milik versi 1;
- versi 2 wajib disetujui dr. Rina dan diverifikasi Dewi lagi.

---

## 2. Proses bisnis

1. Dokter MCU yang **ditetapkan** menekan **Buka ulang** dan mengisi alasan.
2. Syarat: order *Disetujui Dokter*, *Terverifikasi*, atau *Diserahkan*, dengan resume berlaku *Disetujui Dokter* atau
   *Terverifikasi*; versi order sama.
3. Satu transaksi:
   - versi berlaku → *Dikoreksi*, tidak lagi berlaku, mencatat alasan, dokter, dan waktu;
   - versi baru (nomor + 1) *Draf* berisi salinan isi, ditulis dokter yang membuka;
   - setiap penyerahan versi lama → *perlu diberi tahu*;
   - order → *Menunggu Resume*, riwayat mencatat alasan.
4. Dokter memperbaiki isian Pemeriksaan MCU dan/atau draf, lalu alur `BE-MCU-030` berlaku lagi: approve → verifikasi.
5. Nilai isian yang diubah mencatat nilai lama beserta **versi resume yang disetujui dengan nilai itu**.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Bukan dokter yang ditetapkan (termasuk petugas) | `403` `VAL-MCU-38` |
| Alasan kosong | `400` `VAL-MCU-40` *"Alasan wajib diisi."* |
| Versi order berbeda | `409` `VAL-MCU-46` |
| Resume masih *Draf* (belum disetujui / baru dibuka ulang) | `409` *"Resume belum disetujui dokter; ubah drafnya langsung."* |
| Order dibatalkan | `409` *"Order sudah dibatalkan."* |
| Mengubah isian sebelum dibuka ulang | `409` `VAL-MCU-44` *"Hasil terkunci. Minta dokter membuka ulang resume."* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-031`), `contracts/api-contract.md`, `state-transition-matrix.md` (1.1, 2.2, 3),
`validation-matrix.md`, `permission-audit-matrix.md`, `testing/acceptance-test-matrix.md` (`AT-MCU-16`, `43`),
`00-interview-decisions.md` (`MCU-DEC-011`, `MCU-AC-013`), `erd/data-dictionary.md` (`ResumeVersionNumber`),
`McuResume`, `McuResumeConfiguration` (index unik versi berlaku), `McuResultDelivery`, `McuExaminationResultValueRevision`,
`McuResumeService`, `McuExaminationService.SaveResultAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuResumeService.cs` | `ReopenAsync`: penjaga, versi lama *Dikoreksi*, versi baru *Draf* bersalinan isi, penanda penyerahan, order *Menunggu Resume*; dua `SaveChanges` dalam satu transaksi |
| `MedicalCheckupManagement/Controllers/McuResumeController.cs` | `POST reopen`, izin `McuResume : Reopen`; log tanpa alasan |
| `MedicalCheckupManagement/DTOs/McuResumeDtos.cs` | `ReopenMcuResumeRequest` (reason, version) |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` | Revisi nilai mencatat versi resume **terakhir yang disetujui**, bukan versi berlaku |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /orders/{id}/resumes/reopen` baru, sesuai `MCU-API-v1`. Endpoint lain tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | `McuResume : Reopen` **dan** akun tertaut ke `McuDoctorId` order. Alasan koreksi dan isi resume tidak masuk log aplikasi |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Roadmap/kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| `AT-MCU-16` | Disebut sebagai *versi ulang* | Tidak diuji di task ini | `AT-MCU-16` adalah uji **check-in** (`MCU-AC-001`), tidak berhubungan dengan buka ulang; kemungkinan salah rujuk di roadmap |
| Penanda penyerahan | *Diisi bila `BE-MCU-034` sudah ada* | Sudah diisi sekarang atas semua `McuResultDelivery` versi lama | Tabelnya sudah ada sejak `BE-MCU-002`; diuji dengan baris penyerahan yang disemai langsung. Uji penuh `AT-MCU-43` tetap diulang di `BE-MCU-034` |
| Isi versi baru | Tidak disebut | Salinan isi versi lama; `ApprovedAt` kosong | Dokter mengoreksi, bukan menulis dari nol |
| `ResumeVersionNumber` revisi | Data dictionary: *versi resume yang disetujui dengan nilai lama* | Diperbaiki — sebelumnya `BE-MCU-023` mencatat versi **berlaku** (sesudah buka ulang = draf versi 2) | Termasuk cakupan roadmap (*McuExaminationService — revisi nilai*) |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Resume

Base URL: `/api/v1/health-services/medical-checkup-management/orders/{id}/resumes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/reopen` | Buka ulang resume menjadi versi baru; versi lama tetap terbaca | `McuResume : Reopen` + dokter yang ditetapkan | `ReopenMcuResumeRequest` (reason, version) | `McuResumeResponse` (versi baru) |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi: nomor order, nomor versi baru, nama dokter.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-030` (SQLite, service produksi MCU/Lab/Rad/Billing/Registrasi).
Audiometri diisi lewat `SaveResultAsync` sungguhan dengan isian angka *Ambang dengar kanan*. Penyerahan versi lama
disemai langsung (alurnya milik `BE-MCU-034`). Harness **dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 29 lulus (5 buka ulang + 11 resume + 13 batal komponen) | `PASS` | `Passed! - Failed: 0, Passed: 29` |
| `AT-MCU-43` versi dan penyerahan | Order *Diserahkan* → buka ulang: versi 2 *Draf* berlaku berisi salinan isi, `ApprovedAt` kosong; versi 1 *Dikoreksi*, tidak berlaku, alasan + dr. Rina + pemverifikasi Dewi tetap terbaca; `GET` urut [2, 1]; kedua penyerahan versi 1 `RequiresCorrectionNotice = true`; riwayat *Diserahkan → Menunggu Resume* beralasan | `PASS` | `AT_MCU_43_versi_lama_…` |
| `AT-MCU-43` isian dan revisi | Sebelum buka ulang `VAL-MCU-44`; sesudahnya 20 → 35 dB berhasil; satu revisi `20`, `ResumeVersionNumber = 1`, pelaku dr. Rina; versi 2 approve lalu verifikasi → order *Terverifikasi* | `PASS` | `AT_MCU_43_isian_terbuka_…` |
| Buka ulang kedua | Versi 3; status [Dikoreksi, Dikoreksi, Draf]; tepat satu berlaku | `PASS` | `Buka_ulang_kedua_kali_…` |
| Penjaga | dr. Bima dan Dewi `403` `VAL-MCU-38`; alasan kosong `400` `VAL-MCU-40`; versi lama `VAL-MCU-46`; tetap satu versi sesudah semua penolakan; buka ulang saat *Draf* `409` | `PASS` | `Buka_ulang_ditolak_…` |
| Kontrak endpoint | `POST reopen`, izin `McuResume : Reopen` | `PASS` | `Endpoint_POST_reopen_…` |
| Uji mutasi | Versi revisi dikembalikan ke *versi berlaku*, penanda penyerahan dilepas, penjaga dokter buka ulang dilepas, status lama tidak *Dikoreksi* → 4 uji **gagal**; dipulihkan identik (`cmp`), kembali 29/29 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (4 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan | `PASS` | Keluaran checker |
| PostgreSQL — index unik versi berlaku (model uji SQLite melepas index unik berfilter) | Urutan dua `SaveChanges` belum terbukti terhadap index sungguhan | `NOT RUN` | Startup devYoga menunggu dua migration |

Uji manual: `NOT FEASIBLE` di sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-43` tanpa bagian penyerahan — versi 1 *Dikoreksi* terbaca, versi 2 *Draf*, isian terbuka, revisi nilai tercatat | Terpenuhi (penanda penyerahan juga terbukti dengan data semai) | §5 |
| Hanya dokter yang ditetapkan, beralasan | Terpenuhi | §5 |
| `AT-MCU-16` *versi ulang* | Tidak berlaku — lihat §3.4 | — |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Checker QBE yang berjalan bersamaan dengan `dotnet test` tidak menangkap kontrol negatif (dua task berturut-turut); jalankan sendirian sesudah build selesai |
| Masalah yang diketahui | Pencatatan *sudah diberi tahu* (`CorrectionNoticeRecordedAt`) milik `BE-MCU-034` |
| Risiko tersisa | Index unik berfilter `IsCurrent` hanya teruji lewat urutan kode, bukan PostgreSQL. Revisi nilai yang tercatat sebelum perbaikan ini tidak ada (fitur baru, belum dipakai di devYoga) |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); kontrol negatif QBE dibuat lalu dihapus |
| Interupsi | `NONE` |
| Status Git | Perubahan task ini seluruhnya di folder `MedicalCheckupManagement/` (`??`); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-032` (ganti dokter MCU) siap; sesudahnya bagian ketiga `AT-MCU-39` |
