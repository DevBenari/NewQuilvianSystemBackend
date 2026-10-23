# Laporan Perubahan Backend — `BE-MCU-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-013` |
| Judul | Check-in admisi |
| Slice | `MVP-1` — `EPIC MCU-03` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-013`* |
| Trace | `FR-MCU-020`..`024`; `MCU-DEC-003`, `013`, `020`, `025`, `026`, `032`, `033`, `039`, `042`; `MCU-COORD-009`; `NFR-001`, `NFR-003` |
| Contract version | `MCU-API-v1` grup Mcu Check In; `MCU-VAL-v1` `VAL-MCU-24`..`30`; `MCU-STATE-v1` (*Terjadwal → Hadir*, bagian 1.2, komponen bagian 2.1); `MCU-INT-v1` `INT-MCU-01`..`03`; `02-backend-architecture.md` §2.1 — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-001` ✅, `BE-MCU-011` ✅ |
| Keputusan tambahan | 2026-10-22, Yoga Aji Pratama: (1) `EncounterIntakeService` dan `BillingInvoiceService` **ikut transaksi pemanggil**; (2) intake **menerima penjamin perusahaan khusus kunjungan MCU**. Roadmap semula menyebut Billing *dipakai apa adanya*; persetujuan tertulis pemilik Billing (Yasmina) dan pemilik Registrasi atas dua perubahan kecil ini **belum tercatat** |
| Klasifikasi | `HEAVY` — skor 11: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika kompleks lintas tiga modul (2), kontrak API baru (2), persistence/transaksi (1), keamanan berkaitan — kewenangan Registrasi, data sensitif (1), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/{Controllers,DTOs,Services}/`; `RegistrationManagement/{DTOS,Services}/EncounterIntake*`; `BillingInvoiceService.cs` (satu blok); `Program.cs`; laporan ini; tanda roadmap/traceability; catatan penutupan di laporan `BE-MCU-001` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-15`..`24` terbukti di harness dengan Registrasi dan Billing sungguhan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | MCU: `HealthServices` / `MedicalCheckupManagement`, prefix `Mcu`, registry `ACTIVE`. Mitra: `RegistrationManagement` (`Reg`), `BillingManagement` (`Bil`) |
| Keberlakuan | `NEW CODE` (MCU); `TOUCHED LEGACY` untuk `EncounterIntakeService`, `EncounterIntakeDtos`, `BillingInvoiceService.UpsertChargeAsync`, `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuOrder : CheckIn`), `QBE-LOG-001` (riwayat status + penetapan dokter + log aplikasi tanpa NIK), `QBE-VAL-001` (`VAL-MCU-24`..`30`), `QBE-TXN-001` (satu transaksi lintas tiga modul — inti task), `QBE-DTO-001`, `QBE-CFG-002`/`TOUCHED LEGACY` (perubahan mitra terbatas, perilaku pemanggil lama dipertahankan) |
| Checker QBE | `PASS` — 8 berkas (MCU + dua berkas mitra + `Program.cs`) nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Peserta yang sudah dibooking belum dapat di-check-in: belum ada kunjungan MCU, tagihan paket, dokter MCU,
maupun pencatatan persyaratan. Hambatan teknisnya, `EncounterIntakeService` dan `BillingInvoiceService`
masing-masing membuka transaksi sendiri — sehingga "satu transaksi" yang diminta kontrak tidak mungkin —
dan jalur intake menolak penjamin perusahaan.

**Contoh.** Budi check-in 27-10-2026: order **yang sama** menjadi *Hadir*, satu kunjungan Medical Check-Up
berantrean skrining perawat, satu baris *Paket Basic* Rp 350.000, tanpa biaya administrasi. Siti (PT Contoh)
check-in saat harga perusahaan berlaku → Rp 300.000 atas tanggungan kartu PT Contoh miliknya; bila harga itu
sudah berakhir → Rp 350.000. Bila tarif paket dinonaktifkan admin sebelum check-in → *"Tarif paket tidak
aktif. Hubungi admin MCU."*, order tetap *Terjadwal*, tanpa kunjungan. Bila Billing menolak **sesudah**
kunjungan dibuat, kunjungan itu ikut batal — tidak ada kunjungan yatim.

---

## 2. Proses bisnis

1. **Pencocokan pasien** (`GET patient-matches`) — order identitas sementara dicocokkan menurut NIK dan
   tanggal lahir: *cocok penuh*, *NIK sama tanggal lahir beda*, atau *belum terdaftar* (petugas
   mendaftarkan pasien baru lewat Patient Management dengan isian awal dari identitas sementara).
2. **Pratinjau** (`GET check-in/preview`) — persyaratan yang wajib ditandai, harga yang akan dipakai dan
   sumbernya, dokter jaga sebagai isian bawaan dokter MCU, kartu penjamin perusahaan pasien.
3. **Check-in** (`POST check-in`) — pemeriksaan: tanggal hari ini, status, `Version`, dokter, pencocokan,
   persyaratan, tarif berlaku, kartu penjamin, klinik MCU. Lalu **satu transaksi**: kunjungan
   (`EncounterIntakeService`, kunci idempotensi `MCU-CHECKIN:{orderId}`) → baris paket
   (`AddCatalogChargeAsync`, kunci idempotensi Billing = `McuOrder.Id`) → order *Hadir* (pasien, cara
   pencocokan, kunjungan, dokter, tarif, baris tagihan) + penetapan dokter + penandaan persyaratan + komponen
   dijadwal ulang + riwayat. Sesudah commit, notifikasi antrean Registrasi dikirim.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Tanggal pemeriksaan bukan hari ini | `409` `VAL-MCU-24` |
| Order *Tidak Hadir* / *Dibatalkan* / status lain | `409` (`AT-MCU-15`: *"Order sudah Tidak Hadir. Jadwalkan ulang lebih dulu."*) |
| NIK sama, tanggal lahir beda (atau NIK beda) tanpa konfirmasi | `409` `VAL-MCU-25` menyebut RM; dengan konfirmasi → *Dikonfirmasi petugas* + petugas tercatat |
| Tanpa dokter MCU | `400` `VAL-MCU-26` |
| Persyaratan wajib tidak ditandai / lanjut tanpa catatan | `400` `VAL-MCU-27` / `28` |
| Persyaratan tidak terpenuhi → jadwal ulang komponen | Komponen *Dijadwal ulang* ke tanggal susulan (wajib buka, tanpa kuota); komponen lain tetap berjalan |
| Tarif paket/perusahaan tidak aktif | `409` `VAL-MCU-29`; tidak ada yang tersimpan |
| Peserta perusahaan tanpa kartu penjamin berlaku | `409` `MCU-COMPANY-CARD-MISSING` |
| Registrasi atau Billing menolak di tengah transaksi | Seluruh check-in batal; pesan mitra diteruskan (`MCU-PARTNER-REJECTED`) |
| Kirim ulang / dua klik bersamaan | `200` hasil check-in yang sama (`VAL-MCU-30`); tanpa kunjungan atau baris kedua |
| `Version` basi | `409` `VAL-MCU-46` |
| Pengguna tanpa kewenangan Registrasi membuat kunjungan | `403` dari Registrasi |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md`, `validation-matrix.md` bagian 4, `state-transition-matrix.md`,
`integration-contract.md` (`INT-MCU-01`..`03`), `permission-audit-matrix.md`,
`testing/acceptance-test-matrix.md` (`AT-MCU-15`..`24`), `02-backend-architecture.md` §2.1/§5.5/§5.6,
laporan `BE-MCU-001` (catatan untuk task ini), `EncounterIntakeService.cs`, `EncounterIntakeDtos.cs`,
`EncounterPaymentSourceService.cs` (aturan kartu penjamin loket), `RegPatientEncounterGuarantor.cs` +
configuration, `MstPatientCompanyGuarantor.cs`, `BillingInvoiceService.cs` (`AddCatalogChargeAsync`,
`UpsertChargeAsync`, `MapServiceType`), `AdministrationFeeCalculationService.cs`,
`BillingManagementServiceCollectionExtensions.cs`, `AccessMenuSeeder.cs`, `Program.cs`, harness uji Farmasi
(fungsi tiruan PostgreSQL) dan BE-MCU-001, komponen MCU sebelumnya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuCheckInService.cs` (baru) | Pencocokan, pratinjau, check-in satu transaksi |
| `MedicalCheckupManagement/Controllers/McuCheckInController.cs` (baru) | 3 endpoint `McuOrder : CheckIn`; tag *Mcu Check In* |
| `MedicalCheckupManagement/DTOs/McuCheckInDtos.cs` (baru) | Request/response check-in |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-24`..`29`, `MCU-COMPANY-CARD-MISSING`, `MCU-CLINIC-MISSING`, `MCU-PARTNER-REJECTED` |
| `RegistrationManagement/Services/EncounterIntakeService.cs` | **Ikut transaksi pemanggil** (savepoint untuk percobaan ulang nomor; notifikasi antrean + audit ditunda ke `PublishDeferredAsync`); **penjamin perusahaan** khusus `MedicalCheckup` dengan pemeriksaan kartu sama seperti loket dan snapshot sumber bayar sama seperti loket. Tanpa transaksi pemanggil dan untuk jenis kunjungan lain, perilaku lama tidak berubah |
| `RegistrationManagement/DTOS/EncounterIntakeDtos.cs` | Ruas `PatientCompanyGuarantorId` |
| `BillingManagement/Billing/Services/BillingInvoiceService.cs` | `UpsertChargeAsync` tidak membuka transaksi Serializable bila pemanggil sudah membuka transaksi; kunci penasihat tetap diambil. Tanpa transaksi pemanggil, tidak berubah |
| `Program.cs` | `AddScoped<McuCheckInService>()` |
| `docs/.../BE-MCU-001.md` | Catatan penutupan butir (a) dan (b) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint baru grup *Mcu Check In*. `PATCH mcu-doctor` di grup yang sama menyusul `BE-MCU-032`. Request intake (kontrak internal Registrasi) bertambah satu ruas opsional |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Action `McuOrder : CheckIn`. Pembuatan kunjungan tetap memeriksa kewenangan Registrasi (`HasRegistrationAuthorityAsync`) — akun admisi MCU wajib memegangnya. Penjamin perusahaan lewat jalur ini hanya untuk kunjungan MCU; penjamin benefit karyawan internal tetap hanya lewat loket. Log aplikasi tanpa NIK |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Transaksi mitra | §2.1 satu transaksi; roadmap: Billing *dipakai apa adanya* | Registrasi dan Billing ikut transaksi pemanggil | Keputusan pemilik MCU 2026-10-22; tanpa itu §2.1 mustahil. Isolasi Billing di jalur ini mengikuti transaksi MCU (*read committed*) + kunci penasihat Billing; kunjungan yang ditagih baru dibuat di transaksi yang sama sehingga tak ada penulis lain |
| Penjamin perusahaan | `INT-MCU-02`: dari `MstPatientCompanyGuarantor` | Kartu milik pasien untuk perusahaan order, berlaku hari itu; dipilih otomatis (kartu utama dulu) atau lewat `patientCompanyGuarantorId` | Kartu wajib menurut model sumber bayar. Peserta tanpa kartu → `409` dengan arahan melengkapi data penjamin |
| `NewPatientRegistered` | Cara pencocokan | NIK + tanggal lahir sama **dan** pasien terdaftar sesudah booking | MCU tidak membuat pasien, jadi pasien baru dikenali dari waktu daftarnya |
| NIK berbeda | Matriks hanya menyebut tanggal lahir beda | NIK berbeda juga butuh konfirmasi petugas (`VAL-MCU-25`) | Petugas memilih pasien lain secara manual |
| Klinik MCU | Tidak disebut | Klinik aktif bertipe *Medical Check-Up* tersedia registrasi, kode terkecil | Satu klinik MCU per rumah sakit (§9) |
| Jadwal ulang komponen saat check-in | Wajib tanggal buka | `rescheduledDate` > hari ini, tanggal buka, tanpa kuota | `MCU-STATE-v1` bagian 2.1 |
| Kode penolakan mitra | Tidak disebut | `MCU-PARTNER-REJECTED` dengan pesan mitra apa adanya (`400`/`403`/`409`) | Pesan Registrasi/Billing sudah ditulis untuk petugas |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Check In

Base URL: `api/v1/health-services/medical-checkup-management/orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/patient-matches` | Calon pasien cocok (NIK + tanggal lahir) | `McuOrder : CheckIn` | - | `McuPatientMatchResponse` (`Linked` / `FullMatch` / `NikMatchBirthDateDiffers` / `NoMatch` + kandidat) | Tersedia |
| `GET` | `/check-in/preview` | Persyaratan, harga yang akan dipakai, dokter jaga bawaan, kartu penjamin | `McuOrder : CheckIn` | - | `McuCheckInPreviewResponse` | Tersedia |
| `POST` | `/check-in` | Check-in | `McuOrder : CheckIn` | `McuCheckInRequest` (patientId?, matchConfirmation, mcuDoctorId, patientCompanyGuarantorId?, requirementOutcomes[], version) | `McuOrderDetailResponse` | Tersedia |
| `PATCH` | `/mcu-doctor` | Ganti dokter MCU | `McuOrder : ReassignDoctor` | — | — | Belum tersedia (`BE-MCU-032`) |

Kode status: `200` (termasuk kirim ulang), `400`, `403`, `404`, `409`. Log aplikasi: nomor order, nomor
kunjungan, dokter MCU, cara pencocokan.

---

## 5. Verifikasi

Harness: basis data SQLite di memori; Registrasi = `EncounterIntakeService` produksi (seam kewenangan saja);
Billing = `BillingInvoiceService` produksi dirangkai `AddBillingManagement()`; fungsi tiruan `hashtext` dan
`pg_advisory_xact_lock` seperti harness Farmasi. Kegagalan mitra disimulasikan lewat data (kategori tarif
nonaktif membuat Billing menolak **sesudah** kunjungan dibuat), bukan interceptor.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| Uji MCU penuh | 153 lulus (137 lama + 16 baru); termasuk uji intake `BE-MCU-001` (jalur `Outpatient` tidak berubah) | `PASS` | `Passed! - Failed: 0, Passed: 153` |
| Satu uji kontrak lama (`McuOrderControllerContractTests`) | Sempat gagal: menghitung semua rute `/orders` (8 → 11 karena rute check-in). Uji diperbaiki agar menghitung rute milik `McuOrderController` | `PASS` sesudah perbaikan uji | Keluaran uji |
| `AT-MCU-16` + `23` | Order sama *Hadir*; 1 kunjungan MCU (klinik, dokter, Cash, kunci idempotensi), 1 antrean; **tepat 1 baris** tagihan Rp 350.000 (tanpa biaya administrasi); `PackageInvoiceItemId`, tarif, `Version` 1; 1 penetapan dokter, 1 riwayat, 1 penandaan persyaratan | `PASS` | `AT_MCU_16_dan_23_…` |
| `AT-MCU-17` | Kirim ulang dengan versi lama dan baru → `200`; tetap 1 kunjungan, 1 baris, `Version` 1 | `PASS` | `AT_MCU_17_…` |
| `AT-MCU-18` | `FullMatch`; tertaut pasien lama; cara *Cocok NIK dan tanggal lahir*; nol pasien baru | `PASS` | `AT_MCU_18_…` |
| `AT-MCU-19` | `NikMatchBirthDateDiffers`; tanpa konfirmasi `409` pesan persis; dengan konfirmasi *Dikonfirmasi petugas* + petugas tercatat | `PASS` | `AT_MCU_19_…` |
| `AT-MCU-20` | Tanpa tanda `400` pesan persis; tanpa catatan `VAL-MCU-28`; ke tanggal tutup `409`; jadwal ulang → GDP *Dijadwal ulang* ke besok, DL tetap *Menunggu* hari ini; kuota besok tidak terpakai | `PASS` | `AT_MCU_20_…` |
| `AT-MCU-21` | `400` *"Pilih dokter MCU pemeriksa."* | `PASS` | `AT_MCU_21_…` |
| `AT-MCU-22` | Harga perusahaan berlaku → Rp 300.000, kunjungan `CompanyGuarantor` dengan snapshot kartu (perusahaan, nomor pegawai); harga berakhir → Rp 350.000 | `PASS` | `AT_MCU_22_…` (2 uji) |
| Peserta perusahaan tanpa kartu | `409` `MCU-COMPANY-CARD-MISSING`; nol kunjungan | `PASS` | `Peserta_perusahaan_tanpa_kartu_…` |
| `AT-MCU-24` | Tarif nonaktif → `409` `VAL-MCU-29`; order *Terjadwal*; nol kunjungan; nol baris | `PASS` | `AT_MCU_24_…` |
| **Satu transaksi** — Billing menolak sesudah kunjungan dibuat | `409` `MCU-PARTNER-REJECTED`; **nol** kunjungan, sumber bayar, antrean, baris tagihan, riwayat; order *Terjadwal* `Version` 0. Sesudah data dibenahi, check-in yang sama berhasil utuh | `PASS` | `Billing_gagal_sesudah_kunjungan_dibuat_…` |
| `AT-MCU-15` + `VAL-MCU-24`/`46` | Tanggal lain, *Tidak Hadir*, versi basi ditolak; nol kunjungan | `PASS` | `AT_MCU_15_…` |
| Pratinjau | Tanpa penghalang; dokter jaga dr. Rina; Rp 300.000 *Company*; tarif aktif; 1 persyaratan GDP; 1 kartu penjamin | `PASS` | `Pratinjau_…` |
| Kontrak controller | Tag, route, 3 endpoint `McuOrder : CheckIn`; identitas `[AccessController]` sama dengan `McuOrderController`; `PermissionRegistryValidator` nol temuan | `PASS` | `McuCheckInControllerContractTests` |
| Uji Radiologi | 106 lulus | `PASS` | Keluaran uji |
| Uji Patient Management dan Farmasi | Gagal **kompilasi**: konstruktor `PatientMembershipController` (commit `252fb56a`, 19-10), namespace `Billing.Constants`, `EncounterStatus.InProgress` | `EXISTING / ENVIRONMENT ISSUE` | Tidak satu pun berkas terkait ada di diff task ini (`git diff --name-only`); folder milik orang lain, dibiarkan atas instruksi user |
| Checker QBE `-Path` (8 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Kunci penasihat, savepoint, dan balapan di PostgreSQL | Tidak dijalankan | `NOT RUN` | Butuh basis data dengan migration MCU |
| `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

**Folder uji dihapus.** Atas instruksi user, `Tests/QuilvianSystemBackend.McuTests` dan
`Tests/QuilvianSystemBackend.RadiologyTests` dihapus sesudah seluruh uji di atas lulus (keduanya diabaikan
git, sehingga penghapusan permanen). Bukti di laporan ini dan laporan `BE-MCU-002`..`012` berasal dari
jalannya uji sebelum penghapusan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-16`..`AT-MCU-24` lulus | Terpenuhi | §5 |
| Contoh Budi: order sama *Hadir*, satu kunjungan, satu baris Rp 350.000, tanpa biaya administrasi | Terpenuhi | §5 |
| Siti (PT Contoh): Rp 300.000 saat berlaku; Rp 350.000 sesudah berakhir | Terpenuhi (tanggal relatif terhadap hari uji) | §5 |
| Tarif dinonaktifkan sebelum check-in → `409`, order *Terjadwal*, nol kunjungan | Terpenuhi | §5 |
| `preview` mengisi dokter jaga sebagai pilihan bawaan | Terpenuhi | §5 |
| Verifikasi: uji integrasi lintas modul; kegagalan disimulasikan | Terpenuhi — lewat data, bukan interceptor | §5 |
| DoD: kirim ulang check-in tidak menggandakan apa pun | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Persetujuan tertulis pemilik Billing (Yasmina) dan Registrasi** atas perubahan "ikut transaksi pemanggil" dan "penjamin perusahaan untuk kunjungan MCU" belum tercatat di bukti; perubahan dibuat atas keputusan pemilik MCU 2026-10-22 dan menjaga perilaku pemanggil lama |
| Masalah yang diketahui | (c) dari laporan `BE-MCU-001` tetap terbuka: kunjungan MCU tercatat `RegistrationSource = WalkIn`/`IsWalkIn = true` dan kelas pasien *RAWAT JALAN* untuk peserta booking. Audit Billing di jalur ini ditulis sebelum commit MCU — bila check-in batal sesudahnya, log Billing sempat mencatat charge yang tidak jadi |
| Risiko tersisa | Perilaku savepoint dan kunci penasihat di PostgreSQL belum diuji; akun admisi MCU wajib memegang kewenangan pendaftaran Registrasi; kategori tarif paket wajib aktif; peserta perusahaan wajib punya kartu penjamin |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus; folder uji MCU dan Radiologi dihapus atas instruksi user |
| Interupsi | `NONE` (satu proyeksi `DateOnly.FromDateTime` dipindah ke memori; registrasi DI uji dilengkapi) |
| Status Git | Perubahan task: ` M` `EncounterIntakeService.cs`, `EncounterIntakeDtos.cs`, `BillingInvoiceService.cs`, `Program.cs`; `??` tiga berkas MCU baru; perubahan `McuOperationResult.cs`; laporan ini; catatan di `BE-MCU-001.md`; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-014` (*walk-in* admisi) kini terbuka; `BE-MCU-020` tetap siap; `BE-MCU-021` menunggu `020` |
