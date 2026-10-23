# Laporan Perubahan Backend — `BE-MCU-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-011` |
| Judul | Booking, jadwal ulang, batal sebelum datang |
| Slice | `MVP-1` — `EPIC MCU-02` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-011`* |
| Trace | `FR-MCU-010`..`014`; `MCU-DEC-003`, `012`, `014`, `033`, `038`, `041` |
| Contract version | `MCU-API-v1` grup Mcu Order; `MCU-VAL-v1` `VAL-MCU-10`, `11`, `15`..`18`, `20`, `21`, `23`, `46`; `MCU-STATE-v1` bagian 1; `MCU-PERM-v1` (`McuOrder : Create/Update/Cancel`) — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-004` ✅, `005` ✅, `007` ✅, `010` ✅ |
| Klasifikasi | `HEAVY` — skor 11: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika kompleks — kuota, order ganda, nomor, status (2), kontrak API baru (2), persistence yang sudah ada (1), keamanan berkaitan — data sensitif peserta (1), workflow luas (2) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/{Controllers,DTOs,Services}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — seluruh acceptance test task terbukti di harness; log terbukti tanpa NIK |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — satu baris registrasi |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`Create`, `Update`, `Cancel`), `QBE-LOG-001` (pelaku + nomor order; tanpa data sensitif), `QBE-VAL-001`, `QBE-TXN-001` (pakai/lepas kuota + order + riwayat dalam satu transaksi), `QBE-CODE-001`..`006` (nomor lewat `McuOrderNumberService`), `QBE-DTO-001`, `QBE-DEL-001` (batal = status + `IsCancel`/`CancelBy`, bukan hapus) |
| Checker QBE | `PASS` — 6 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Admisi belum dapat mencatat booking MCU dari telepon/WhatsApp, memindahkan tanggalnya, atau membatalkannya.

**Contoh.** Siti Aminah (belum pasien) menelepon untuk Paket Basic pada 27-10-2026. Admisi mengisi nama,
NIK, dan tanggal lahirnya → order `MCU-RSMMC-000001` *Terjadwal*, kuota 27-10 menjadi 1/30, belum ada
kunjungan, tagihan, maupun No. RM. Rekan admisi yang tanpa sadar membooking Siti lagi ditolak *"Peserta ini
sudah terjadwal MCU pada 27-10-2026 (MCU-RSMMC-000001)."* Siti lalu minta pindah ke 30-10 → kursi 27-10
dilepas, kursi 30-10 dipakai, riwayat mencatat *"Dijadwal ulang dari 27-10-2026 ke 30-10-2026: Peserta
dinas luar kota"*. Akhirnya Siti membatalkan → *Dibatalkan* beralasan, kursi dilepas, tanpa refund.

---

## 2. Proses bisnis

**Booking** (`POST orders/bookings`):

1. Admisi memilih pasien terdaftar **atau** mengisi identitas sementara (nama, NIK 16 digit, tanggal lahir,
   HP), penjamin, paket, dan tanggal.
2. Sistem memeriksa tanggal (`VAL-MCU-15`), peserta (`16`, `17`), paket (`20`), perusahaan (`21`), order
   ganda (`18`), dan sisa kuota (`10`, `11`) — seluruhnya **sebelum** nomor terbit.
3. Nomor order diterbitkan alokator Platform.
4. Dalam satu transaksi: kunci peserta+tanggal, periksa ulang order ganda, pakai kursi (`12` bila kalah
   balapan), simpan order *Terjadwal*, potret komponen paket (*Menunggu*), dan riwayat *— → Terjadwal*.

**Jadwal ulang** (`PATCH orders/{id}/reschedule`): order *Terjadwal* atau *Tidak Hadir* pindah ke tanggal
baru; kursi lama dilepas (hanya bila masih *Terjadwal*), kursi baru dipakai, tanggal komponen ikut pindah,
riwayat mencatat kedua tanggal.

**Batal sebelum datang** (`PATCH orders/{id}/cancel`): order *Terjadwal* → *Dibatalkan* dengan alasan,
kursi dilepas, tanpa refund (`MCU-DEC-038`).

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Tanpa pasien dan identitas tidak lengkap | `400` `VAL-MCU-16` |
| NIK bukan 16 digit angka | `400` `VAL-MCU-17` |
| Peserta (pasien atau NIK — termasuk NIK pasien) sudah punya order aktif pada tanggal itu | `409` `VAL-MCU-18` menyebut nomor order pertama |
| Tanggal lampau / tutup / belum diatur / penuh | `400` `VAL-MCU-15` / `409` `VAL-MCU-10` / `10` / `11`; **nomor tidak terbit** |
| Dua booking merebut kursi terakhir | Satu berhasil; yang kalah `409` `VAL-MCU-12`; nomornya hangus (`INV-PLT-002`) |
| Paket nonaktif; perusahaan nonaktif atau kerja sama berakhir sebelum tanggal pemeriksaan | `400` `VAL-MCU-20`; `400` `VAL-MCU-21` |
| `Version` dari layar berbeda | `409` `VAL-MCU-46` |
| Jadwal ulang ke tanggal sama / ke tanggal penuh | `400` / `409`; order dan kursi lama tidak berubah |
| Batal tanpa alasan | `400` `VAL-MCU-23` |
| Batal/jadwal ulang order *Dibatalkan* | `409` *"Order sudah dibatalkan."* |
| Batal order yang sudah check-in | `409` — batal penuh adalah `BE-MCU-025` |
| Sesudah batal, peserta dibooking lagi pada tanggal sama | Diterima (order batal tidak dihitung ganda) |
| Tanpa hak akses | `403` dari `AccessPermissionFilter` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md`, `validation-matrix.md` bagian 2–3 dan `VAL-MCU-46`, `state-transition-matrix.md`,
`permission-audit-matrix.md`, `testing/acceptance-test-matrix.md`, `00-interview-decisions.md`
(`MCU-DEC-003`, `014`, `033`, `037`, `038`), `05-screen-brief.md` 5.2, model `McuOrder`,
`McuOrderComponent`, `McuOrderStatusHistory`, `McuPackage*`, `MstPatient`, `MstCompanyGuarantor`,
`McuOrderConfiguration.cs`, `LoggerService.cs`, `NumberSeriesAllocator.cs`, komponen MCU `BE-MCU-004`..`010`,
`Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuBookingService.cs` (baru) | Booking, jadwal ulang, batal sebelum datang |
| `MedicalCheckupManagement/Services/McuScheduleService.cs` | Tambah `CheckAvailabilityAsync` — pemeriksaan kuota baca saja sebelum nomor terbit |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Tambah kode `VAL-MCU-16`, `17`, `18`, `20`, `21`, `23`, `46`, `MCU-INVALID-TRANSITION`, `MCU-NUMBER-ALLOCATION-FAILED` |
| `MedicalCheckupManagement/DTOs/McuOrderDtos.cs` | Tambah `CreateMcuBookingRequest`, `RescheduleMcuOrderRequest`, `CancelMcuOrderRequest` |
| `MedicalCheckupManagement/Controllers/McuOrderController.cs` | Tambah 3 action tulis dan log aplikasi |
| `Program.cs` | `AddScoped<McuBookingService>()` |
| `Tests/QuilvianSystemBackend.McuTests/McuBookingServiceTests.cs` (baru) | 16 uji, termasuk balapan jalur penuh dan uji log |
| `Tests/QuilvianSystemBackend.McuTests/McuOrderControllerContractTests.cs` | Diperbarui ke 8 endpoint |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint baru sesuai `MCU-API-v1`; bagian baca `BE-MCU-010` tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Action `McuOrder : Create`, `Update`, `Cancel` mulai terdaftar. Identitas sementara (nama, NIK, tanggal lahir, HP) disimpan di order; **tidak satu pun** masuk log — dibuktikan uji yang menangkap seluruh keluaran `LoggerService` |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Urutan booking | Tidak disebut | Validasi baca + cek kuota → nomor → transaksi (kunci, cek ulang, pakai kursi, simpan) | Booking yang pasti ditolak tidak menghanguskan nomor; alokator wajib di luar transaksi pemanggil (`DEC-PLT-008`) |
| Kunci order ganda | `VAL-MCU-18` | `pg_advisory_xact_lock` per NIK (atau pasien) + tanggal di dalam transaksi | Dua booking serentak untuk peserta sama tidak lolos bersamaan; tidak ada index unik untuk aturan ini |
| Pesan `VAL-MCU-21` | *"Kerja sama PT Contoh berakhir 31-12-2026."* | Juga *"Kerja sama {nama} tidak aktif."* untuk perusahaan nonaktif | Matriks menyebut kondisi nonaktif tanpa kalimat |
| `reason` pada jadwal ulang | Request `newDate, version` | `reason` opsional, tercatat di riwayat bersama kedua tanggal | `MCU-DEC-014` meminta riwayat mencatat tanggal lama |
| Batal order *Tidak Hadir* | Matriks hanya *Terjadwal → Dibatalkan* (sebelum datang) | Ditolak `409` | Tidak ada perpindahan *Tidak Hadir → Dibatalkan* di `MCU-STATE-v1`; **perlu keputusan pemilik** bila diinginkan |
| Batal sesudah check-in | Endpoint sama, jalur batal penuh | `409` *"…pembatalan penuh sesudah check-in belum tersedia."* | Jalur itu milik `BE-MCU-025` |
| NIK peserta terdaftar | Kamus: NIK wajib bila belum pasien | Order pasien terdaftar tidak menyimpan NIK; pemeriksaan ganda memakai `MstPatient.IdentityNumber` | Menghindari salinan NIK kedua |
| Tarif terpakai | Ditetapkan saat check-in | `AppliedTariffId` kosong saat booking | `MCU-STATE-v1`: *tarif berlaku* adalah syarat check-in |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Order

Base URL: `api/v1/health-services/medical-checkup-management/orders` — bagian tulis `BE-MCU-011` (bagian
baca lihat [`BE-MCU-010`](BE-MCU-010.md)).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/bookings` | Booking (telepon/WhatsApp) | `McuOrder : Create` | `CreateMcuBookingRequest` (packageId, examinationDate, patientId **atau** participantName + participantNik + participantBirthDate, participantPhone?, companyGuarantorId?) | `McuOrderDetailResponse` | Tersedia |
| `PATCH` | `/{id}/reschedule` | Jadwal ulang order *Terjadwal*/*Tidak Hadir* | `McuOrder : Update` | `RescheduleMcuOrderRequest` (newDate, version, reason?) | `McuOrderDetailResponse` | Tersedia |
| `PATCH` | `/{id}/cancel` | Batal sebelum datang | `McuOrder : Cancel` | `CancelMcuOrderRequest` (reason, version) | `McuOrderDetailResponse` | Tersedia — jalur batal penuh menyusul `BE-MCU-025` |
| `POST` | `/recommendations`, `/walk-ins`; `PATCH` `/{id}/arrival-confirmation` | — | — | — | — | Belum tersedia (task lain) |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi memuat nomor order, tanggal, sumber, dan
pelaku; tidak memuat nama, NIK, HP, maupun alasan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama) | 128 lulus (112 lama + 16 baru), 0 error kompilasi | `PASS` | `Passed! - Failed: 0, Passed: 128` |
| `AT-MCU-08` | `MCU-RSMMC-000001` *Terjadwal*; `PatientId` kosong; kuota 1/30; 0 kunjungan; 0 baris tagihan; 0 pasien baru; 3 komponen *Menunggu*; riwayat *— → Terjadwal* oleh admisi | `PASS` | `AT_MCU_08_…` |
| Pasien terdaftar | Tertaut pasien; nama dan RM dari pasien; NIK tidak disalin ke order | `PASS` | `Booking_pasien_terdaftar_…` |
| `AT-MCU-09` | Tanpa NIK `VAL-MCU-16`; 15 digit dan berhuruf `VAL-MCU-17`; nol order, kuota tetap | `PASS` | `AT_MCU_09_…` (3 kasus) |
| `AT-MCU-10` | Booking kedua `409` menyebut `MCU-RSMMC-000001`; kuota tetap 1; tanggal lain diterima | `PASS` | `AT_MCU_10_…` |
| NIK identitas sementara = NIK pasien terdaftar | Dianggap peserta sama → `VAL-MCU-18` | `PASS` | `Pasien_dan_identitas_sementara_…` |
| `AT-MCU-04` | Ke-31 `409` (30/30); kuota tetap 30; nol order; booking berikutnya tetap `000001` (nomor tidak hangus) | `PASS` | `AT_MCU_04_…` |
| `AT-MCU-05` (jalur service penuh) | Booking pesaing disisipkan sesudah pemeriksaan kuota petugas kedua: satu berhasil, yang kalah `VAL-MCU-12`; `UsedQuota` = kuota = 30; tepat satu order dan satu riwayat (transaksi yang kalah dibatalkan utuh) | `PASS` | `AT_MCU_05_…` |
| `VAL-MCU-15`, `10`, `20`, `21` | Lampau, tutup, belum diatur, paket nonaktif, kontrak berakhir (pesan persis), perusahaan nonaktif; nol order | `PASS` | `Tanggal_lampau_…` |
| Jadwal ulang | Kursi pindah 27→30 (lama 0, baru 1); komponen ikut; `Version` 1; riwayat memuat kedua tanggal + alasan | `PASS` | `Jadwal_ulang_memindahkan_…` |
| Jadwal ulang *Tidak Hadir* | Tidak melepas kursi lama lagi; kursi baru dipakai | `PASS` | `Jadwal_ulang_order_Tidak_Hadir_…` |
| Jadwal ulang tidak sah | Tanggal sama `400`; `Version` basi `VAL-MCU-46`; tanggal penuh `VAL-MCU-11`; order dan kursi lama tidak berubah | `PASS` | `Jadwal_ulang_ditolak_…` |
| `AT-MCU-14` | Tanpa alasan `VAL-MCU-23`; dengan alasan → *Dibatalkan*, `IsCancel`/`CancelBy` terisi, kuota 0, nol refund, riwayat beralasan; batal ulang `409`; booking ulang tanggal sama diterima | `PASS` | `AT_MCU_14_…` |
| Batal sesudah check-in | `409`; kursi tetap | `PASS` | `Order_yang_sudah_check_in_…` |
| DoD — log tanpa NIK | Booking, jadwal ulang, batal lewat controller: 3 log; tidak satu pun (termasuk scope) memuat NIK, nama, HP, atau alasan | `PASS` | `Log_booking_jadwal_ulang_dan_batal_…` |
| Grup Swagger, 8 endpoint, string hak akses, `PermissionRegistryValidator` | Sesuai; nol temuan | `PASS` | `McuOrderControllerContractTests` |
| Checker QBE `-Path` (6 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Kunci advisory peserta dan balapan di PostgreSQL | Tidak berlaku di SQLite | `NOT RUN` | Butuh basis data dengan migration MCU |
| `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-04` | Terpenuhi | §5 |
| `AT-MCU-05` (jalur API/service) | Terpenuhi | §5 |
| `AT-MCU-08` — contoh Budi/Siti identitas sementara: *Terjadwal*, `PatientId` kosong, kuota 1/30, nol kunjungan/tagihan/pasien baru | Terpenuhi | §5 |
| `AT-MCU-09` | Terpenuhi | §5 |
| `AT-MCU-10` — booking kedua menyebut `MCU-RSMMC-000001` | Terpenuhi | §5 |
| `AT-MCU-14` | Terpenuhi | §5 |
| DoD: kriteria terbukti; log tidak memuat NIK | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Booking yang kalah balapan kursi menghanguskan satu nomor order (perilaku alokator yang disengaja). Booking yang ditolak karena tutup/penuh/ganda **tidak** menghanguskan nomor |
| Masalah yang diketahui | **Dikoreksi 2026-10-22 lewat `BE-MCU-012`:** potret komponen order mendapat `CreateDateTime` kembar sehingga urutannya di detail order bisa tertukar (uji `AT_MCU_08_…` sempat gagal sekali); kini berurutan pasti. Lihat [`BE-MCU-012`](BE-MCU-012.md) §3.4 |
| Keputusan terbuka | Apakah order *Tidak Hadir* boleh dibatalkan (saat ini `409`, mengikuti `MCU-STATE-v1`) — pemilik: Yoga |
| Risiko tersisa | Kunci advisory dan balapan di PostgreSQL belum diuji; startup backend ke devYoga masih menunggu migration `BE-MCU-002`/`003` |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` (satu hitungan baris log di uji diperbaiki sebelum dijalankan) |
| Status Git | Perubahan task: ` M Program.cs`; `??` `McuBookingService.cs`; perubahan pada berkas MCU sebelumnya yang belum di-commit (`McuScheduleService.cs`, `McuOperationResult.cs`, `McuOrderDtos.cs`, `McuOrderController.cs`); laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-012` (tutup hari *Tidak Hadir*) kini terbuka; `BE-MCU-013` (check-in) menunggu `BE-MCU-001` ✅ dan `011` ✅ — juga terbuka; `BE-MCU-020` tetap siap |
