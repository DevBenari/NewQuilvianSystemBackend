# Laporan Perubahan Backend — `BE-MCU-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-034` |
| Judul | Persetujuan pelepasan dan penyerahan |
| Slice | `MVP-3` — `EPIC MCU-06` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-034`* |
| Trace | `FR-MCU-051`..`053`; `MCU-DEC-010`, `011`, `019`, `027`; `MCU-AC-012`, `013`, `015`, `021`; `AT-MCU-41`, `42`, `43` (penuh), `44` |
| Contract version | `MCU-API-v1` grup Mcu Delivery (`GET/POST deliveries`, `PATCH deliveries/{deliveryId}/correction-notice`, `GET/POST release-consents`, `PATCH release-consents/{consentId}/revoke`); `MCU-VAL-v1` `VAL-MCU-31`..`33`, `40`, `46`; `MCU-STATE-v1` 1.1 (*Terverifikasi → Diserahkan*, *Diserahkan → Diserahkan*); `MCU-PERM-v1` (`McuDelivery : Read/Create/ManageConsent`) — `approved` 2026-10-21 |
| Dependency | `BE-MCU-031` ✅, `BE-MCU-033` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang sudah ada (1), persistence yang ada (1), privasi data kesehatan ke pihak ketiga (2), workflow beberapa layar (2) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuDeliveryService.cs`, `McuDeliveryController.cs`, `McuDeliveryDtos.cs`, `McuLabels.cs`, `McuOperationResult.cs`. Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `cee59643` (branch `yoga`) — tidak bergeser sejak `BE-MCU-033` |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-41`, `42`, `43` (penuh), `44` terbukti di harness integrasi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (service/controller/DTO MCU; tanpa entity baru — `McuResultDelivery`, `McuReleaseConsent` sejak `BE-MCU-002`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuDelivery`), `QBE-LOG-001` (tujuan penyerahan dan alasan pencabutan tidak masuk log), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (pencabutan beraktor, tidak menghapus) |
| Checker QBE | `PASS` — 5 berkas task nol temuan; kontrol negatif `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus. Pemanggilan tool pertama sesudah `dotnet test` (tiga run di dalamnya) tidak menangkap kontrol; pemanggilan kedua menangkapnya — pola yang sama dengan `030`–`033`, sebab belum diketahui |

---

## 1. Masalah yang diperbaiki

Hasil yang sudah terverifikasi belum bisa dicatat penyerahannya, persetujuan peserta untuk melepas hasil lengkap ke
perusahaan belum bisa dicatat, dan penerima versi lama sesudah buka ulang belum bisa ditandai *sudah diberi tahu*.
Tanpa itu tidak ada jejak siapa menerima apa, dan tidak ada penjaga yang mencegah diagnosis peserta sampai ke
perusahaannya.

**Contoh.** Dewi mencoba menyerahkan hasil lengkap Siti ke PT Contoh tanpa persetujuan Siti → `409` *"Peserta belum
menyetujui pelepasan hasil lengkap kepada PT Contoh. Serahkan ringkasan kelayakan."* Dewi menyerahkan ringkasan ke
*hrd@ptcontoh.co.id* → tersimpan dengan penerima *Perusahaan*, isi *Ringkasan kelayakan*, saluran *Email*, tujuan,
versi resume 1, petugas Dewi, dan waktunya; order menjadi *Diserahkan*.

---

## 2. Proses bisnis

**Penyerahan (manual, rilis pertama — `MCU-DEC-027`).**
1. Petugas mencetak/mengunduh hasil (`BE-MCU-033`), menyerahkannya di luar sistem, lalu mencatat: penerima (peserta /
   perusahaan), isi (lengkap / ringkasan kelayakan), saluran (ambil langsung, WhatsApp, email, telepon), tujuan, waktu
   (boleh lampau, tidak di masa depan), dan catatan.
2. Syarat: resume berlaku *Terverifikasi* (`VAL-MCU-32`); WhatsApp/email/telepon wajib bertujuan yang sah
   (`VAL-MCU-33`); isi lengkap ke perusahaan butuh persetujuan pelepasan yang berlaku (`VAL-MCU-31`); penerima perusahaan
   hanya untuk peserta perusahaan.
3. Penyerahan pertama versi itu memindah order *Terverifikasi → Diserahkan*; penyerahan berikutnya hanya menambah
   catatan.
4. Tombol **Kirim** otomatis belum ada (`MCU-COORD-005`).

**Persetujuan pelepasan (`MCU-DEC-010`).**
1. Petugas mencatat persetujuan peserta perusahaan: bentuk bukti (formulir kertas bernomor, atau persetujuan
   elektronik), waktu, dan perusahaannya (otomatis = perusahaan penjamin order). Satu persetujuan berlaku per order.
2. Persetujuan dapat dicabut dengan alasan; sesudahnya isi lengkap ke perusahaan kembali ditolak. Persetujuan baru boleh
   dicatat lagi; riwayat semuanya tetap terbaca.

**Pemberitahuan koreksi (`MCU-DEC-011`).** Sesudah buka ulang (`BE-MCU-031`), setiap penyerahan versi lama bertanda
*perlu diberi tahu*. Petugas mencatat waktu pemberitahuan, sekali per penyerahan. Versi baru wajib diverifikasi lagi
sebelum diserahkan ulang.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Belum terverifikasi (termasuk versi baru sesudah buka ulang) | `409` `VAL-MCU-32` *"Hasil belum diverifikasi."* |
| Isi lengkap ke perusahaan tanpa persetujuan berlaku | `409` `VAL-MCU-31` *"Peserta belum menyetujui pelepasan hasil lengkap kepada PT Contoh. Serahkan ringkasan kelayakan."* |
| WhatsApp/telepon/email tanpa tujuan; nomor atau email tidak sah | `400` `VAL-MCU-33` *"Isi nomor WhatsApp tujuan."* / *"Nomor tujuan tidak valid."* / *"Format email tujuan tidak valid."* |
| Waktu penyerahan/persetujuan/pemberitahuan di masa depan | `400` |
| Penerima atau persetujuan perusahaan pada peserta perorangan | `400` *"Order ini bukan peserta perusahaan."* |
| Formulir kertas tanpa nomor | `400` *"Isi nomor formulir persetujuan."* |
| Persetujuan berlaku sudah ada / sudah dicabut | `409` |
| Cabut tanpa alasan | `400` `VAL-MCU-40` |
| Pemberitahuan koreksi untuk penyerahan yang tidak ditandai / sudah dicatat | `409` |
| Versi order berbeda | `409` `VAL-MCU-46` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-034`), `contracts/api-contract.md` (grup Mcu Delivery), `validation-matrix.md`
(`VAL-MCU-31`..`33`), `state-transition-matrix.md` (1.1), `permission-audit-matrix.md`, `testing/acceptance-test-matrix.md`
(`AT-MCU-41`..`45`), `00-interview-decisions.md` (`MCU-DEC-010`, `011`, `019`, `027`, `MCU-OQ-003`), `McuResultDelivery`,
`McuReleaseConsent` dan konfigurasinya (index unik persetujuan berlaku), enum penyerahan/bukti, `McuDeliveryService`
(`BE-MCU-033`), `McuResumeService.ReopenAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuDeliveryService.cs` | `GetDeliveriesAsync`, `CreateDeliveryAsync`, `RecordCorrectionNoticeAsync`, `GetConsentsAsync`, `CreateConsentAsync`, `RevokeConsentAsync`; validasi tujuan; perpindahan *Diserahkan* |
| `MedicalCheckupManagement/Controllers/McuDeliveryController.cs` | Enam endpoint baru; log tanpa tujuan dan alasan |
| `MedicalCheckupManagement/DTOs/McuDeliveryDtos.cs` | `CreateMcuResultDeliveryRequest`, `RecordCorrectionNoticeRequest`, `McuResultDeliveryResponse`, `CreateMcuReleaseConsentRequest`, `RevokeMcuReleaseConsentRequest`, `McuReleaseConsentResponse` |
| `MedicalCheckupManagement/Services/McuLabels.cs` | Label penerima, isi, saluran, bukti persetujuan |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-31`, `32`, `33` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Enam endpoint baru sesuai `MCU-API-v1` |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Privasi | Hasil lengkap ke perusahaan hanya dengan persetujuan berlaku. Tujuan penyerahan (nomor, email) disimpan tetapi tidak masuk log aplikasi; alasan pencabutan tidak masuk log. Izin `McuDelivery : Create/ManageConsent` per departemen-posisi — pemberiannya di devYoga belum diperiksa |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Bentuk DTO | Hanya nama | Ruas sesuai kolom model + label | Kontrak tidak merinci |
| Bukti persetujuan | `MCU-OQ-003` *draft* | Kedua bentuk dalam enum (formulir kertas bernomor wajib nomor; elektronik tanpa nomor); tanpa unggah berkas | Roadmap: `MCU-OQ-003` menahan go-live, bukan task ini |
| Validasi nomor | Hanya *tujuan wajib* + *format email* | Nomor WhatsApp/telepon 8–15 digit (boleh `+`, spasi, `-`) | Mencegah isian asal seperti *nomor siti* |
| Isi untuk peserta | Tidak disebut | Peserta boleh menerima lengkap maupun ringkasan | `MCU-DEC-010` hanya membatasi perusahaan |
| `RecordCorrectionNoticeRequest` | Hanya nama | `NotifiedAt` (kosong = sekarang) + `Version` | Model hanya punya `CorrectionNoticeRecordedAt`; pelaku tercatat di `UpdateBy` |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Delivery

Base URL: `/api/v1/health-services/medical-checkup-management/orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/deliveries` | Riwayat penyerahan, terbaru di atas | `McuDelivery : Read` | - | `List<McuResultDeliveryResponse>` |
| `POST` | `/deliveries` | Catat penyerahan manual | `McuDelivery : Create` | `CreateMcuResultDeliveryRequest` | `McuResultDeliveryResponse` |
| `PATCH` | `/deliveries/{deliveryId}/correction-notice` | Catat pemberitahuan koreksi kepada penerima versi lama | `McuDelivery : Create` | `RecordCorrectionNoticeRequest` | `McuResultDeliveryResponse` |
| `GET` | `/release-consents` | Riwayat persetujuan pelepasan | `McuDelivery : Read` | - | `List<McuReleaseConsentResponse>` |
| `POST` | `/release-consents` | Catat persetujuan pelepasan hasil lengkap | `McuDelivery : ManageConsent` | `CreateMcuReleaseConsentRequest` | `McuReleaseConsentResponse` |
| `PATCH` | `/release-consents/{consentId}/revoke` | Cabut persetujuan | `McuDelivery : ManageConsent` | `RevokeMcuReleaseConsentRequest` (reason, version) | `McuReleaseConsentResponse` |

Kode status: `200`, `400`, `404`, `409`. Log aplikasi: nomor order, versi, label isi/penerima/saluran, nama perusahaan —
tanpa tujuan, alasan, nama peserta, atau NIK.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-033` (SQLite, service produksi MCU/Lab/Rad/Billing/Registrasi).
Order Siti (PT Contoh) melewati alur sungguhan: walk-in → mulai → hasil → resume → approve → verifikasi. Harness
**dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 48 lulus (7 penyerahan + 41 regresi) | `PASS` | `Passed! - Failed: 0, Passed: 48` |
| `AT-MCU-41` | Disetujui belum terverifikasi → `409` `VAL-MCU-32`; nol penyerahan | `PASS` | `AT_MCU_41_…` |
| `AT-MCU-42` penyerahan | Isi lengkap ke PT Contoh tanpa persetujuan → `409` `VAL-MCU-31` pesan persis; ringkasan lewat email tersimpan lengkap (penerima, isi, saluran, tujuan, versi 1, Dewi, waktu); order *Diserahkan* dengan satu riwayat *Terverifikasi → Diserahkan*; penyerahan kedua tidak menambah riwayat | `PASS` | `AT_MCU_42_isi_lengkap_…` |
| Persetujuan pelepasan | Formulir tanpa nomor `400`; dengan nomor → berlaku; ganda `409`; isi lengkap ke perusahaan berhasil; cabut tanpa alasan `VAL-MCU-40`; cabut → tidak berlaku, cabut ulang `409`; isi lengkap kembali `VAL-MCU-31`; persetujuan baru sesudahnya; riwayat 2, satu berlaku | `PASS` | `Persetujuan_pelepasan_…` |
| Peserta perorangan | Persetujuan → `400` *"Order ini bukan peserta perusahaan."* | `PASS` | `Persetujuan_dan_penyerahan_…` |
| `AT-MCU-44` | WhatsApp tanpa tujuan `400` `VAL-MCU-33`; email `siti@` ditolak; telepon tanpa tujuan; nomor tidak sah; waktu masa depan; versi lama `VAL-MCU-46`; WhatsApp `0812 3456 7890` 30 menit lalu tersimpan dengan Dewi dan waktunya | `PASS` | `AT_MCU_44_…` |
| `AT-MCU-43` penuh | Penyerahan ke HRD dan Siti → catat koreksi sebelum buka ulang `409`; buka ulang → kedua penyerahan *perlu diberi tahu*, versi 1; serahkan versi 2 sebelum verifikasi `409`; catat koreksi HRD → tercatat, kedua kali `409`; approve + verifikasi versi 2 → penyerahan versi 2, order *Diserahkan* | `PASS` | `AT_MCU_43_penuh_…` |
| Kontrak endpoint | Enam template dan izin (`Read`, `Create`, `ManageConsent`) | `PASS` | `Endpoint_penyerahan_…` |
| Uji mutasi | `VAL-MCU-31`, `32`, `33`, perpindahan *Diserahkan*, dan syarat *perlu diberi tahu* dilepas → 5 uji **gagal**; dipulihkan identik (`cmp`), kembali 48/48 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (5 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan (pemanggilan kedua) | `PASS` | Keluaran checker |
| `AT-MCU-45` (log tanpa NIK, nilai, isi, tujuan) | Tidak diuji otomatis; ditinjau di kode — kalimat log endpoint ini tidak memuat tujuan, alasan, nama, maupun NIK | `NOT RUN` | Review diff |
| PostgreSQL (index unik persetujuan berlaku), `403` lewat HTTP | Tidak dijalankan | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` di sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-41` — serahkan sebelum verifikasi `409` | Terpenuhi | §5 |
| `AT-MCU-42` — isi lengkap ke perusahaan tanpa persetujuan `409`; ringkasan tersimpan dengan penerima, isi, saluran, tujuan, versi, petugas, waktu | Terpenuhi (bagian refleksi DTO di `BE-MCU-033`) | §5 |
| `AT-MCU-43` penuh — versi lama *Dikoreksi*, penyerahan lama *perlu diberi tahu*, isian terbuka, revisi tercatat (`BE-MCU-031`), pemberitahuan dicatat, penyerahan ulang versi 2 | Terpenuhi | §5 |
| `AT-MCU-44` — WhatsApp tanpa tujuan `400`; dengan tujuan tersimpan dengan petugas dan waktu | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Checker QBE: pemanggilan tool pertama sesudah `dotnet test` meleset seluruhnya (lima task berturut-turut); memori diperbarui dengan pola ini |
| Masalah yang diketahui | Bentuk bukti persetujuan final (`MCU-OQ-003`, *draft*) — **menahan go-live** penyerahan ke perusahaan; unggah berkas formulir belum ada. Tombol Kirim otomatis menunggu `MCU-COORD-005` |
| Risiko tersisa | Index unik persetujuan berlaku hanya terbukti lewat penjaga kode, bukan PostgreSQL. Hak akses `McuDelivery` di devYoga belum diperiksa |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); kontrol negatif QBE dibuat lalu dihapus |
| Interupsi | `NONE` |
| Status Git | ` M` `McuLabels.cs`, `McuOperationResult.cs`, `Program.cs` (sejak `033`), laporan `030`–`032` (koreksi); `??` tiga berkas `McuDelivery*` (`033` + `034`), laporan `033` dan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-035` (uji menyeluruh privasi log `AT-MCU-45` dan registri izin MCU) — task `MVP-3` terakhir |
