# Laporan Perubahan Backend — `BE-MCU-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-014` |
| Judul | *Walk-in* admisi |
| Slice | `MVP-1` — `EPIC MCU-03` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-014`* |
| Trace | `FR-MCU-025`; `MCU-DEC-017`, `MCU-DEC-037` |
| Contract version | `MCU-API-v1` grup Mcu Order (`POST /walk-ins`); `MCU-VAL-v1` `VAL-MCU-10`, `11`, `16`, `18`, `20`, `21`, `25`..`29`; `MCU-STATE-v1` (*— → Hadir*); `MCU-PERM-v1` (`McuOrder : Create` + `CheckIn`) — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-013` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), kontrak API baru (2), persistence/transaksi (1), keamanan berkaitan — izin kedua (1), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `McuBookingService.cs`, `McuCheckInService.cs`, `McuOrderController.cs`, `McuOrderDtos.cs`; laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — kedua kriteria terbukti di harness dengan Registrasi dan Billing sungguhan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (tanpa perubahan modul mitra di task ini) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuOrder : Create` pada endpoint + `McuOrder : CheckIn` di service), `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` (order + kuota + check-in lintas modul dalam satu transaksi), `QBE-CODE-001`..`006`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 4 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Peserta yang datang tanpa booking belum dapat dilayani: admisi harus membooking lalu check-in dalam dua
langkah terpisah, dan kegagalan di tengah dapat meninggalkan order *Terjadwal* tanpa kunjungan.

**Contoh.** Senin berkuota 30 sudah penuh; Siti datang tanpa booking → *"Kuota MCU tanggal … sudah penuh
(30/30). Minta persetujuan kepala unit untuk melampaui."* Kepala unit menyetujui pelampauan 5; admisi
mendaftarkan Siti → satu order langsung *Hadir*, satu kunjungan, satu baris paket, kuota 31/35.

---

## 2. Proses bisnis

1. Admisi memilih pasien terdaftar (peserta baru didaftarkan lebih dulu lewat Patient Management), paket,
   penjamin, dokter MCU, dan menandai persyaratan — satu halaman (`05-screen-brief.md` 5.3).
2. Pemeriksaan sebelum nomor terbit: pasien, paket (`VAL-MCU-20`), perusahaan (`21`), order ganda hari ini
   (`18`), hak check-in, dokter (`26`), persyaratan (`27`, `28`), tarif (`29`), kartu penjamin, sisa kuota
   (`10`, `11`).
3. Nomor order terbit; lalu **satu transaksi**: kunci peserta, periksa ulang order ganda, pakai kursi hari ini,
   simpan order dan potret komponen, lalu check-in `BE-MCU-013` (kunjungan, baris paket, dokter, persyaratan,
   riwayat *— → Hadir*). Commit, lalu notifikasi antrean Registrasi.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Kuota hari ini penuh | `409` `VAL-MCU-11` menyebut (30/30); tidak ada yang tersimpan; sesudah pelampauan berhasil |
| Pasien belum terdaftar | `400` `VAL-MCU-16` *"Pilih pasien terdaftar atau daftarkan pasien baru lebih dulu."* |
| Peserta sudah punya order aktif hari ini (mis. booking) | `409` `VAL-MCU-18` — check-in order booking itu, bukan *walk-in* |
| Pengguna tanpa `McuOrder : CheckIn` | `403` |
| Dokter/persyaratan/tarif/kartu tidak sah | `400`/`409` sesuai matriks; nomor tidak terbit |
| Registrasi atau Billing menolak di tengah | Order, komponen, kursi, kunjungan, antrean, baris tagihan batal bersama; nomor yang sudah terbit hangus |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-014`), `contracts/api-contract.md`, `permission-audit-matrix.md`,
`validation-matrix.md`, `state-transition-matrix.md`, `05-screen-brief.md` 5.3, `00-interview-decisions.md`
(`MCU-DEC-017`, `037`), `AccessPermissionAttribute.cs` (tanpa `AllowMultiple`),
`EncounterIntakeService.HasRegistrationAuthorityAsync` (pola seam kewenangan), `AccessPermissionService.cs`,
`McuBookingService.cs`, `McuCheckInService.cs`, `McuOrderController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuBookingService.cs` | `CreateWalkInAsync`; potret komponen paket diekstrak menjadi `LoadPackageComponentsAsync`/`AddOrderComponents` yang dipakai booking dan *walk-in*; dependensi `McuCheckInService` |
| `MedicalCheckupManagement/Services/McuCheckInService.cs` | Inti check-in menjadi `CheckInCoreAsync` yang ikut transaksi pemanggil bila ada (tanpa commit/rollback/notifikasi sendiri) dan mencatat riwayat *— → Hadir* untuk *walk-in*; `ValidateWalkInAsync`; seam `HasCheckInAuthorityAsync`; `PublishDeferredAsync`; penolong `PackageRequirementsAsync`, `CompanyCardMissingAsync`. `CheckInAsync` publik tidak berubah perilakunya |
| `MedicalCheckupManagement/Controllers/McuOrderController.cs` | `POST walk-ins` (`McuOrder : Create`) |
| `MedicalCheckupManagement/DTOs/McuOrderDtos.cs` | `CreateMcuWalkInRequest` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST orders/walk-ins` baru; endpoint lain tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Endpoint memegang `McuOrder : Create`; izin kedua `McuOrder : CheckIn` diperiksa service lewat `AccessPermissionService` — satu action hanya boleh punya satu `[AccessPermission]`. Kewenangan pendaftaran Registrasi tetap diperiksa intake |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Dua izin | `McuOrder : Create` + `McuOrder : CheckIn` | `Create` lewat atribut; `CheckIn` lewat service | `AccessPermissionAttribute` tidak `AllowMultiple`; matriks bagian 2 mencantumkan endpoint ini di bawah `Create` |
| Peserta *walk-in* | Sama dengan Booking bagian 1–3 (boleh peserta baru) | Wajib pasien terdaftar | Check-in wajib pasien tertaut (`BE-MCU-013`); MCU tidak membuat pasien — peserta baru didaftarkan lebih dulu lewat Patient Management, sesuai catatan kontrak grup Mcu Check In |
| Cara pencocokan | — | *Pasien terdaftar saat booking* (`ExistingAtBooking`) — pendaftaran *walk-in* adalah saat booking-nya | Tidak ada nilai enum khusus *walk-in* |
| Pemeriksaan sebelum nomor | Tidak disebut | Seluruh pemeriksaan yang tidak butuh order dijalankan sebelum nomor terbit | Konsisten dengan booking `BE-MCU-011` |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Order

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/walk-ins` | Pendaftaran *walk-in* di admisi (langsung check-in) | `McuOrder : Create` (+ `McuOrder : CheckIn` diperiksa service) | `CreateMcuWalkInRequest` (packageId, patientId, participantPhone?, companyGuarantorId?, mcuDoctorId, patientCompanyGuarantorId?, requirementOutcomes[]) | `McuOrderDetailResponse` | Tersedia |

Kode status: `200`, `400`, `403`, `409`. Log aplikasi: nomor order, nomor kunjungan, dokter MCU — tanpa NIK.

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`: SQLite, `EncounterIntakeService`
produksi dengan seam kewenangan, `BillingInvoiceService` produksi lewat `AddBillingManagement()`, fungsi tiruan
`hashtext`/`pg_advisory_xact_lock`, alokator nomor Platform), dijalankan, lalu **dihapus** sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 11 lulus | `PASS` | `Passed! - Failed: 0, Passed: 11` |
| Kriteria (1) | Hari penuh → `409` `VAL-MCU-11` (30/30), nol order, nol kunjungan; sesudah pelampauan 5 → *Hadir*, sumber *Walk-in Admisi*, kuota 31 | `PASS` | `Kriteria_1_…` |
| Kriteria (2) | `MCU-RSMMC-000001`; 1 kunjungan (kunci idempotensi check-in), **1 baris** Rp 350.000, 2 komponen, 1 penandaan persyaratan, 1 penetapan dokter, **1 riwayat *— → Hadir***; kuota 1 | `PASS` | `Kriteria_2_…` |
| *Walk-in* perusahaan | Rp 300.000; sumber bayar kartu PT Contoh | `PASS` | `Walk_in_peserta_perusahaan_…` |
| Billing menolak di tengah | Nol order, komponen, riwayat, kunjungan, antrean, baris tagihan; kuota tetap 0 | `PASS` | `Walk_in_yang_gagal_di_Billing_…` |
| Penolakan awal | Tanpa dokter `VAL-MCU-26`, tanpa tanda persyaratan `VAL-MCU-27` (pesan persis), pasien tak dikenal `VAL-MCU-16`, tanpa hak check-in `403`; nol order, kuota 0; nomor pertama yang terbit tetap `000001` | `PASS` | `Penolakan_sebelum_order_…` |
| Peserta yang sudah dibooking hari ini | `409` `VAL-MCU-18` | `PASS` | `Peserta_yang_sudah_dibooking_…` |
| Endpoint | `POST walk-ins` = `McuOrder : Create`; `PermissionRegistryValidator` nol temuan untuk MCU | `PASS` | `Endpoint_walk_in_…` |
| Regresi `BE-MCU-013` | Check-in booking: *Terjadwal → Hadir*, kirim ulang tanpa kunjungan/baris kedua; Billing gagal → nol kunjungan; tarif nonaktif `VAL-MCU-29` | `PASS` | 3 uji `Regresi_check_in_…` / `Regresi_tarif_…` |
| Regresi `BE-MCU-011` | Booking memotret komponen berurutan (Darah Lengkap, Gula Darah Puasa), *Menunggu*, nol kunjungan | `PASS` | `Regresi_booking_…` |
| Checker QBE `-Path` (4 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Balapan dan kunci di PostgreSQL; `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) *Walk-in* pada hari berkuota penuh → `409` menyebut kuota; sesudah pelampauan kepala unit → berhasil | Terpenuhi | §5 |
| (2) Satu kunjungan dan satu baris paket, sama seperti check-in | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kegagalan modul mitra sesudah nomor terbit menghanguskan satu nomor order (perilaku alokator yang disengaja) |
| Masalah yang diketahui | Persetujuan tertulis pemilik Billing/Registrasi atas perubahan `BE-MCU-013` masih belum tercatat; kunjungan MCU tercatat *walk-in* Registrasi (catatan (c) `BE-MCU-001`) |
| Risiko tersisa | Balapan dua *walk-in* untuk kursi terakhir dan kunci peserta hanya terbukti logikanya di SQLite; startup backend ke devYoga masih menunggu migration MCU |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Sesi terputus sesudah controller ditulis; dilanjutkan dari build — tidak ada suntingan ganda |
| Status Git | Perubahan task: berkas MCU di folder modul (`??`); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-020` (Lab/Rad bertanda paket dan penahanan tagihan) siap; sesudahnya `BE-MCU-021` (mulai pemeriksaan) |
