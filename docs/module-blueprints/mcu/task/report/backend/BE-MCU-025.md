# Laporan Perubahan Backend — `BE-MCU-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-025` |
| Judul | Batal penuh |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-025`* |
| Trace | `FR-MCU-035`; `MCU-DEC-021`; `MCU-AC-014` |
| Contract version | `MCU-API-v1` grup Mcu Order (`PATCH {id}/cancel`, `CancelMcuOrderRequest`); `MCU-VAL-v1` `VAL-MCU-23`, `41`, `46`; `MCU-STATE-v1` 1.1 (*Hadir/Dalam Pemeriksaan → Dibatalkan*), 2.3 (arti *dimulai*); `MCU-PERM-v1` (`McuOrder : Cancel`); `MCU-INT-v1` `INT-MCU-08`, `09` — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-021` ✅ |
| Klasifikasi | `HIGH` — skor 12: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika tinggi (2), lintas empat modul Lab/Rad/Billing/Registrasi (2), perubahan transaksi modul mitra (2), uang — refund/void (2), konkurensi (1) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuExaminationService.cs`, `McuOrderController.cs`, `McuBookingService.cs`, `McuOperationResult.cs`. Mitra: `BillingRefundService.cs`, `BillingInvoiceService.cs` (Billing), `EncounterIntakeService.cs` (Registrasi). Laporan ini; tanda roadmap/traceability |
| Keputusan user (2026-10-22) | (1) Billing dan Registrasi **ikut transaksi pemanggil**; (2) kuota hari itu **tidak** dikembalikan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-32` terbukti dengan Lab, Billing, dan Registrasi sungguhan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE`; modul mitra `BillingManagement` (`Bil`) dan `RegistrationManagement` (`Reg`) |
| Keberlakuan | `NEW CODE` (MCU, jalur baru Registrasi) + `TOUCHED CODE` (Billing: syarat pembukaan transaksi) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuOrder : Cancel`), `QBE-LOG-001` (alasan batal tidak dicatat log), `QBE-VAL-001`, `QBE-TXN-001` (satu transaksi lintas empat modul), `QBE-CODE-001`..`006`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 7 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Order yang sudah check-in tidak dapat dibatalkan: `PATCH orders/{id}/cancel` menjawab *"pembatalan penuh sesudah
check-in belum tersedia"*. Peserta yang batal sesudah membayar tidak punya jalan pengembalian uang, dan kunjungan,
pesanan Lab/Rad, serta baris paket tertinggal terbuka.

**Contoh.** Budi membayar Rp 350.000 pukul 07.10, lalu pukul 07.20 harus pulang; darahnya belum diambil. Dewi
membatalkan dengan alasan *permintaan peserta* → order *Dibatalkan*, pesanan Lab batal, refund Rp 350.000 diajukan,
kunjungan dan antreannya batal. Bila darah Budi sudah diambil pukul 07.15 → `409` *"Sudah ada pemeriksaan yang
dimulai. Gunakan pembatalan per komponen."*

---

## 2. Proses bisnis

1. Admisi menekan **Batal** pada order. Order *Terjadwal* tetap memakai batal sebelum datang (`BE-MCU-011`, kuota
   dilepas); order *Hadir*/*Dalam Pemeriksaan* memakai batal penuh.
2. Syarat: alasan diisi, versi order sama, dan **belum ada komponen dimulai** (`MCU-STATE-v1` 2.3).
3. Satu transaksi:
   - pesanan Lab dan Rad turunan dibatalkan di modulnya;
   - baris paket dikembalikan utuh: **refund diajukan** bila invoice sudah dibayar, **baris dibatalkan (void)** bila
     belum (mis. peserta perusahaan);
   - kunjungan MCU beserta antreannya dibatalkan;
   - semua komponen dan order *Dibatalkan*, riwayat mencatat alasan.
4. Sesudah commit, notifikasi antrean batal dikirim. Kuota hari itu tidak dikembalikan.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Komponen sudah dimulai (wadah diambil, pesanan Lab melewati *Requested/Confirmed*, Rad dikerjakan/akuisisi, hasil Pemeriksaan MCU tersimpan, komponen *Selesai*) | `409` `VAL-MCU-41` |
| Alasan kosong | `400` `VAL-MCU-23` *"Alasan pembatalan wajib diisi."* |
| Versi order berbeda / tabrakan simpan | `409` `VAL-MCU-46` |
| Lab, Rad, Billing, atau Registrasi menolak di tengah (mis. kunjungan sudah selesai) | `409` `MCU-PARTNER-REJECTED`; pesanan Lab, refund, kunjungan, komponen, order — **semuanya tetap seperti semula** |
| Baris paket/kunjungan tidak ditemukan | `409` *"Kunjungan atau baris paket order ini tidak ditemukan. Hubungi kasir."* |
| Batal ulang | `409` *"Order sudah dibatalkan."*; tanpa refund kedua |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-025`), `contracts/api-contract.md`, `state-transition-matrix.md` (1.1, 2.3),
`validation-matrix.md`, `integration-contract.md` (`INT-MCU-08`, `09`), `00-interview-decisions.md` (`MCU-DEC-021`),
`McuBookingService.CancelBeforeArrivalAsync`, `McuOrderController`, `BillingRefundService.CreateAsync`,
`BillingInvoiceService.VoidItemAsync`, `BillingCalculationService` (transaksi pemanggil sudah didukung; kunjungan
batal tidak dihitung), `ContractBillingChargeSourceAdapter` (`ADHOC_CATALOG`/`ADDED` dapat di-void),
`OutpatientEncounterListService.CancelAsync`/`GetCancelBlockedReason`, `EncounterIntakeService`, `LabOrderService.CancelAsync`,
`RadOrderService.CancelAsync`/`TransitionAsync`, `LabSpecimenStatus`, `BillingManagementServiceCollectionExtensions`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` | `CancelOrderAsync` (batal penuh, meneruskan status lain ke batal sebelum datang), `AnyComponentStartedAsync`, `PaidAmountAsync`, `CancelPartnerFailure`; dependensi `McuBookingService`, `BillingRefundService`, `BillingInvoiceService`, `EncounterIntakeService` |
| `MedicalCheckupManagement/Controllers/McuOrderController.cs` | `PATCH {id}/cancel` memanggil `CancelOrderAsync`; log membedakan batal sebelum datang dan batal penuh (tanpa alasan) |
| `MedicalCheckupManagement/Services/McuBookingService.cs` | Pesan dan dokumentasi jalur sebelum datang disesuaikan (tidak lagi *"belum tersedia"*) |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-41` |
| `BillingManagement/Billing/Services/BillingRefundService.cs` | `CreateAsync`: transaksi Serializable hanya bila belum ada transaksi pemanggil; kunci penasihat tetap dipegang |
| `BillingManagement/Billing/Services/BillingInvoiceService.cs` | `VoidItemAsync`: idem |
| `RegistrationManagement/Services/EncounterIntakeService.cs` | `CancelMedicalCheckupAsync`: membatalkan kunjungan `MedicalCheckup` + antrean dengan kolom dan aturan boleh-batal yang sama dengan Daftar Pasien Rawat Jalan; ikut transaksi pemanggil; notifikasi/audit ditunda ke `PublishDeferredAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `PATCH orders/{id}/cancel` kini juga melayani order *Hadir*/*Dalam Pemeriksaan*, sesuai kontrak (*"Batal sebelum datang **atau** batal penuh sesudah check-in"*); request/response tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | `McuOrder : Cancel` tetap satu-satunya izin. Jalur pembatalan kunjungan MCU di Registrasi tidak memakai lingkup klinis rawat jalan (pemanggilnya MCU yang sudah memeriksa izin) dan hanya menerima kunjungan bertipe `MedicalCheckup`. Alasan batal tidak masuk log aplikasi |
| Modul mitra | Perilaku Billing tanpa transaksi pemanggil **tidak berubah** (terbukti uji regresi) |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Roadmap/kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Refund Billing *apa adanya* | Reuse `CreateAsync`/`VoidItemAsync` tanpa perubahan | Keduanya diubah agar ikut transaksi pemanggil | Keduanya selalu membuka transaksi Serializable sendiri sehingga mustahil satu transaksi; keputusan user 2026-10-22 |
| Pembatalan kunjungan | *Pembatalan kunjungan Registrasi* | Jalur baru `CancelMedicalCheckupAsync` di Registrasi | Jalur yang ada membuka transaksi sendiri dan mensyaratkan lingkup klinis rawat jalan |
| Arti *Lab dimulai* | *Lebih lanjut dari `Requested`* | Lebih lanjut dari `Requested`/`Confirmed`, **atau** ada wadah yang sudah diambil | Contoh `MCU-DEC-021` (*darah sudah diambil*); konfirmasi dokter pemeriksa Lab bukan pengambilan bahan |
| Arti *Rad dimulai* | *Sudah `start`* | Pesanan `InProgress`/`Completed`, atau study berakuisisi | Termasuk Form Hasil langsung (`RAD-DEC-023`) yang memindah pesanan ke `InProgress` tanpa akuisisi |
| Sudah dibayar | — | Ada tender `SUCCEEDED` pada settlement `INVOICE_PAYMENT` invoice itu → refund; selain itu → void | Rumus yang sama dengan `LabPaymentClearanceRules` |
| Kuota | Tidak disebut | Tidak dikembalikan | Keputusan user 2026-10-22 |
| Komponen dibatalkan | — | `CancelReason` = alasan batal penuh; `CancelledByDoctorId` kosong | Batal penuh oleh admisi, bukan dokter |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Order

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/cancel` | Batal sebelum datang (*Terjadwal*) **atau** batal penuh sesudah check-in (*Hadir*/*Dalam Pemeriksaan*) | `McuOrder : Cancel` | `CancelMcuOrderRequest` (reason, version) | `McuOrderDetailResponse` | Tersedia (kedua jalur) |

Kode status: `200`, `400`, `404`, `409`. Log aplikasi batal penuh: nomor order dan nomor kunjungan — tanpa alasan,
nama, atau NIK.

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`): SQLite, rakitan DI seperti
`Program.cs` — `AddBillingManagement()` (refund, invoice, kalkulasi sungguhan), `EncounterIntakeService` produksi
(seam kewenangan saja yang diganti), `LabOrderService`/`RadOrderService` produksi, `McuBookingService`/`McuCheckInService`
produksi. Order lahir lewat *walk-in* sungguhan dan dimulai lewat `StartAsync`. Model uji menyimpan `DateTimeOffset`
sebagai bilangan (keterbatasan SQLite pada kueri pajak Billing). Harness **dihapus** sesudahnya sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 9 lulus | `PASS` | `Passed! - Failed: 0, Passed: 9` |
| `AT-MCU-32` sebelum | Bayar 350.000 → mulai → batal: order *Dibatalkan* (alasan tersimpan, riwayat *Dalam Pemeriksaan → Dibatalkan*), komponen *Dibatalkan*, pesanan Lab *Cancelled*, **refund Rp 350.000** *Submitted* atas baris paket (correlation = order), kunjungan dan antrean batal, kuota tetap 1 | `PASS` | `AT_MCU_32_sebelum_…` |
| `AT-MCU-32` sesudah | Wadah *Collected* → `409` `VAL-MCU-41` pesan persis; order, pesanan Lab, refund (0), kunjungan tidak berubah | `PASS` | `AT_MCU_32_sesudah_…` |
| Peserta perusahaan belum bayar | Batal langsung sesudah check-in → baris Rp 300.000 **Voided**, nol refund, kunjungan batal, riwayat *Hadir → Dibatalkan* | `PASS` | `Peserta_perusahaan_…` |
| Registrasi menolak di akhir | Kunjungan sudah selesai → `409` `MCU-PARTNER-REJECTED`; pesanan Lab kembali *Requested*, nol refund, kunjungan tidak batal, order dan komponen tidak berubah | `PASS` | `Registrasi_menolak_…` |
| Validasi | Alasan kosong `VAL-MCU-23`; versi lama `VAL-MCU-46`; hasil Pemeriksaan MCU tersimpan → `VAL-MCU-41` | `PASS` | `Alasan_wajib_…` |
| Order *Terjadwal* | Tetap lewat batal sebelum datang; kuota dilepas (1 → 0) | `PASS` | `Order_terjadwal_…` |
| Batal ulang | `409` *"Order sudah dibatalkan."*; tetap 1 refund | `PASS` | `Kirim_ulang_…` |
| Regresi Billing | `CreateAsync` dan `VoidItemAsync` tanpa transaksi pemanggil berhasil dan tidak meninggalkan transaksi terbuka | `PASS` | `Regresi_Billing_…` (2 uji) |
| Uji mutasi | Penjaga `VAL-MCU-41` dilepas dan refund dikembalikan ke *selalu membuka transaksi* → 5 uji **gagal**; dipulihkan identik (`cmp`), kembali 9/9 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (7 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| `PharmacyTests` (memuat uji Billing) | Gagal kompilasi karena perubahan lama — `Billing.Constants` dan `EncounterStatus.InProgress` tidak ada lagi | `EXISTING` | Keluaran `dotnet build`; tidak terkait diff ini |
| PostgreSQL (kunci `FOR UPDATE`, kunci penasihat, Serializable), notifikasi antrean realtime, `403` lewat HTTP | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-32` — sebelum: *Dibatalkan*, refund utuh, kunjungan batal | Terpenuhi | §5 |
| `AT-MCU-32` — sesudah spesimen diambil: `409` | Terpenuhi | §5 |
| Satu transaksi; pesanan turunan dibatalkan; refund bila dibayar, void bila belum | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Di dalam transaksi MCU, audit Billing (refund/void) ditulis sebelum transaksi MCU commit; bila langkah sesudahnya gagal, log audit Billing bisa menyebut refund/void yang akhirnya batal. Notifikasi antrean dan audit Registrasi sudah ditunda sesudah commit |
| Masalah yang diketahui | Persetujuan tertulis pemilik Billing (Yasmina) dan Registrasi atas perubahan transaksi `BE-MCU-013` dan `BE-MCU-025` belum tercatat. Refund utuh memakai nilai baris paket penuh — bila kelak ada refund komponen sebelumnya (`BE-MCU-024`) atau pembayaran sebagian, nominalnya perlu dibatasi lewat perluasan Billing `BE-MCU-026`; refund case aktif lain pada invoice yang sama membuat batal penuh ditolak `409` |
| Risiko tersisa | Kunci baris dan isolasi hanya terbukti di SQLite; hak akses `McuOrder : Cancel` mengikuti `SysAccessPolicies`; startup backend ke devYoga masih menunggu migration MCU |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Tidak ada; satu sisipan shell gagal di-parse sebelum menulis apa pun lalu diulang lewat berkas skrip |
| Status Git | MCU di folder modul (`??`); `BillingRefundService.cs`, `BillingInvoiceService.cs`, `EncounterIntakeService.cs`, `McuOrderController.cs`, `McuBookingService.cs` (`M`/`??` sesuai status awal); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-026` (perluasan refund Billing) siap; sesudahnya `BE-MCU-024` (batal komponen + refund) |
