# Optik — Peta Kemampuan Existing

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Revision | `1` — audit penuh pertama |
| Status | `draft` |
| Jenis audit | Audit penuh, **read-only**. Nol baris source diubah, nol akses basis data, nol server dijalankan |
| Masukan keputusan | [00-interview-decisions.md](00-interview-decisions.md) revision 1 (`OPT-DEC-001`..`OPT-DEC-026`) |
| Backend SHA | `2a5094ca41817a7f8422576d055ab18dd186d3c4` (branch `yoga`). *Working tree* hanya memuat folder `docs/module-blueprints/optik/` yang belum di-commit |
| Frontend v2 SHA | `b6dd6c304043fb904c664d7751b1ce9d9c532715` (branch `YogaV2`). *Working tree* bersih |
| Frontend v1 SHA | `7210f5f4db3a0c1f51446249e5c35a57eedb081c` (`C:\Users\Admin\QuilvianSystemFrontendDev`), referensi tampilan saja |
| Kontrak yang berlaku | Belum ada kontrak Optik |
| Tanggal audit | 2026-11-03 |

**Singkatan bukti.** `BE` = `NewQuilvianSystemBackend`, `FE` = `QuilvianSystemFrontendDev` (v2),
`FE1` = frontend v1. Bila SHA tidak ditulis, yang berlaku adalah SHA pada tabel di atas.

> **Cara membaca dokumen ini.** Dokumen ini menjawab *apa yang sudah ada di sistem hari ini*,
> bukan *bagaimana Optik nanti dibangun*. Setiap baris membawa lokasi bukti supaya bisa
> diperiksa ulang. Status *Ready to reuse* hanya diberikan bila kode, hak akses, dan pemakainya
> terlihat. Isi basis data (misalnya apakah unit layanan "Optik" sudah diinput) **tidak**
> diperiksa, sehingga butir yang bergantung pada data berstatus *Unknown*.

---

## 1. Ringkasan untuk pembaca umum

| No | Temuan | Artinya bagi Optik |
| ---: | --- | --- |
| 1 | Backend **belum punya apa pun** tentang Optik: tidak ada tabel, layanan, endpoint, atau prefix modul | Seluruh inti Optik (pemeriksaan refraksi, resep kacamata, pesanan, garansi) dibangun baru. Modelnya tertahan aturan `QBE-MOD-002` sampai prefix didaftarkan |
| 2 | Registrasi sudah punya **jalur resmi pendaftaran kunjungan untuk unit penunjang** (dipakai Lab, Radiologi, MCU). Kunjungan dibuat Registrasi, bukan modul peminta | `OPT-DEC-006` bisa memakai jalur ini. Belum ada jenis unit layanan "Optik" di daftar jenis unit |
| 3 | Master barang satu-satunya adalah **master obat** (`MstDrug`). Kamar Operasi sudah memakai master yang sama untuk bahan dan implan | Frame, lensa, dan cairan lensa kontak bisa dicatat sebagai barang di master ini, tetapi **tidak ada** isian khusus frame/lensa (warna, ukuran, indeks lensa, jenis lapisan) |
| 4 | Stok dipegang Farmasi lewat **satu pintu penulis kartu stok** (`DrugStockService`), dan Kamar Operasi sudah memotong stok lewat pintu itu | `OPT-DEC-004` dan `OPT-DEC-019` bisa memakai pola yang sama. Depo Optik cukup menjadi satu lokasi penyimpanan baru |
| 5 | Harga jual barang sudah diambil dari **master tarif** yang menautkan tarif ke barang, per klinik dan kelas pasien, lengkap dengan masa berlaku | Menjawab sebagian `OPT-OQ-012`: sumber harga = master tarif (milik Master Data/Billing), bukan isian petugas |
| 6 | Billing punya dua jalur penagihan: **daftar sumber resmi** (Optik belum terdaftar) dan **tagihan dari katalog tarif** (dipakai MCU) | Optik bisa menagih tanpa mengubah kontrak Billing, lewat jalur katalog seperti MCU |
| 7 | Billing punya **pesanan uang muka (deposit)** per kunjungan, tetapi minimumnya berupa **nominal**, bukan persentase. Konsep **uang muka hangus juga belum ada** | `OPT-DEC-008`/`009` (minimal 50%) dan `OPT-DEC-010` (uang muka hangus) butuh keputusan pemilik Billing |
| 8 | **Diskon dikelola Billing** di tingkat tagihan, lengkap dengan kebijakan batas dan persetujuan | Bertentangan dengan `OPT-DEC-021` yang menempatkan pemberian diskon pada petugas penjualan Optik. Perlu keputusan siapa yang mencatat diskon |
| 9 | Data **STR/SIP sudah ada** di HR dan akun pengguna sudah tertaut ke data pegawai. Pengecekan masa berlaku masih ditulis ulang di dua tempat, salah satunya membandingkan jam UTC | `OPT-DEC-012` bisa dipenuhi. Optik harus membandingkan tanggal kalender WIB, bukan meniru perbandingan jam UTC |
| 10 | Jenis penjamin **BPJS ada di daftar, tetapi tidak dipakai di mana pun**. Pilihan cara bayar kunjungan bahkan tidak memuat BPJS, dan modul Insurance berstatus `PARTIAL` | Jalur kacamata BPJS (`OPT-DEC-014`) **tidak punya fondasi** di sistem saat ini. Inilah risiko terbesar MVP |
| 11 | Rekam Medis sudah punya **pintu baca standar** yang dipasang 11 modul (Lab, Radiologi, MCU, Farmasi, dan lain-lain) | `OPT-DEC-024` cukup memasang satu pembaca baru, dengan izin pemilik Rekam Medis |
| 12 | Menu Optik v2 hanya **sisa**: berkas `config.jsx` yang tidak dipakai, satu kunci menu, dan aturan route lama. Menu sidebar yang hidup **tidak** memuat Optik | Tidak ada layar v2 untuk dipakai ulang |
| 13 | Layar Optik v1 berisi **data contoh tertulis langsung di kode** (pasien, resep, pesanan), dan form pendaftarannya memanggil `/PendaftaranPasienOptik` yang **tidak ada** di backend v2 | v1 hanya acuan tampilan (bentuk isian resep OD/OS dan contoh status pesanan) |
| 14 | "Optik" sebagai **tenant penyewa** di Finance hanya berasal dari **data contoh v1** yang tertulis di kode, bukan keputusan bisnis | `OPT-CONFLICT-001` hampir pasti bukan pertentangan nyata (pertanyaan `OPT-Q-08`) |

---

## 2. Batas audit

| Klaster | Yang diperiksa | Cara |
| --- | --- | --- |
| Identity/Master | Pasien, unit layanan, klinik, barang, depo, supplier, tarif | Baca model `MstServiceUnit`/`MstClinic`/`MstDrug`/`MstDrugStorageLocation`/`MstSupplier`/`MstTariff` |
| Episode/Transaction | Kunjungan unit penunjang | `EncounterIntakeService` |
| Actor/Workforce | STR/SIP, profesi, tautan akun ke pegawai | `WfpCredentialLicense`, `MstProfession`, `ApplicationUser` |
| Workflow/Status | Pola versi/revisi, nomor dokumen | Model `*Revision`/`*Version` Farmasi, `NumberSeriesAllocator` |
| Documentation/Record | Pintu baca Rekam Medis | `IMedicalRecordDocumentSource` dan pemasangnya |
| Order/Result | Resep kacamata, pesanan Optik | `rg` `optik/optic/kacamata/refraksi/eyeglass/spectacle` |
| Financial | Penagihan, uang muka, diskon, status bayar, refund, penjamin | `BillingSourceContract`, `BillingInvoiceService`, `BillingSourceTariffResolver`, `BilDepositPaymentOrder`, `MstDepositPolicy`, `MstDiscountPolicy`, `BilDiscountApplication`, `BillingCoverageAdapter` |
| Authorization/Audit | Pola izin dan registry prefix | `[AccessPermission]` MCU, `SysAccessPolicy`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| External Integration | BPJS | `rg` `bpjs/vclaim/sep`, status modul Insurance |
| Frontend | Menu, route, layar, input desimal | `menu-items.jsx`, `config.jsx`, `route-guard-link.js`, layar Optik FE1 |

**Tidak diaudit:** isi basis data (unit layanan, profesi, kategori barang, dan depo yang sudah
diinput), perilaku saat berjalan, laporan (`OPT-DEC-025`, semuanya *Missing* karena Optik belum
ada), dan aturan BPJS di luar sistem (`OPT-OQ-009`).

---

## 3. Tabel kemampuan

### 3.1 Kunjungan dan pasien

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-01` | Pasien Optik terhubung ke data pasien induk | Patient Management | `BE/Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs` ringkasan kelas: "tidak ada satu pun jalur di sini yang membuat atau mengubah data induk pasien. Pasien wajib sudah terdaftar" | `Ready to reuse` | — | Pasien baru harus didaftarkan dulu di Patient Management |
| `OPT-CAP-02` | Kunjungan resmi unit Optik (`OPT-DEC-006`) | Registrasi | `EncounterIntakeService.cs#ResolveVisitProfileAsync` (`:395`): `EncounterType.Outpatient` = kunjungan penunjang **tanpa klinik, dokter, dan antrean**; unit cukup `MstServiceUnit` aktif (`#LoadServiceUnitAsync` `:375`). Pemakai: Lab, Radiologi, MCU | `Reuse with adapter` | Optik memanggil service ini dari service Optik sendiri (tanpa endpoint publik baru di Registrasi), sama seperti Lab | Kunjungan tercatat `Outpatient`. Laporan kunjungan yang memilah jenis kunjungan tidak bisa membedakan Optik kecuali dari unit layanannya |
| `OPT-CAP-03` | Jenis unit layanan "Optik" | Master Data | `BE/Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`: tidak ada nilai Optik (ada `Other = 99`); Hemodialisa menambah nilai baru di ujung (`Hemodialysis = 10`) | `Extend` | Tambah nilai enum di ujung, atau pakai `Other` + kode unit (`OPT-Q-07`) | Memakai `Other` membuat unit Optik tak bisa dicari menurut jenis |
| `OPT-CAP-04` | Baris unit layanan dan depo Optik di basis data | Master Data / Farmasi | Tidak diperiksa (nol akses basis data) | `Unknown` | Cek data dev/produksi; seeder seperti `HemodialysisMasterDataSeeder` bisa dijadikan pola | — |
| `OPT-CAP-05` | Rujukan dari Poli Mata | Registrasi | `EncounterIntakeService.cs:479-536` menerima `ReferralInstitutionId`/`ReferralDoctorId` dan memeriksa dokter perujuk | `Reuse with adapter` | Rujukan internal tercatat sebagai dokter perujuk. Tautan ke kunjungan Poli Mata asal **tidak** tersedia | Riwayat "dirujuk dari kunjungan mana" tidak bisa ditelusuri tanpa tambahan |

### 3.2 Aktor, kewenangan, dan izin

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-06` | STR/SIP pemeriksa berlaku pada tanggal periksa (`OPT-DEC-012`) | Human Resource | `BE/Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpCredentialLicense.cs` (`LicenseType`, `ExpiredDate`, `IsVerified`, `IsRevoked`, `BlocksClinicalServiceWhenInvalid`); `BE/Models/ApplicationUser.cs:16` `WorkforceProfileId` | `Reuse with adapter` | Belum ada service bersama. Logikanya ditulis ulang di `RegistrationManagement/Controllers/DoctorQueueController.cs:1646` (STR/SIP, `AppDateTimeHelper.OperationalDate()`) dan `PharmacyManagement/Services/PrescriptionCopyService.cs:449` (`ExpiredDate >= DateTime.UtcNow`) | Meniru perbandingan UTC menimbulkan jebakan 17 jam. **Contoh:** SIP berakhir 31-10-2026 (tersimpan 31-10 00.00 UTC) dianggap kedaluwarsa sejak 31-10 pukul 07.00 WIB, padahal seharusnya masih berlaku sepanjang 31-10 |
| `OPT-CAP-07` | Profesi Refraksionis Optisien dan jenis izinnya | Human Resource | `CompetencyAndCredential/Models/MstProfession.cs` (`RequiresLicense`, `LicenseTypes`); data profesi tidak diperiksa | `Unknown` | Pastikan profesi RO dan jenis izin STR-RO/SIP-RO sudah diinput | Bila jenis izin RO ditulis bebas, penyaring `LicenseType == "STR" \|\| "SIP"` meleset |
| `OPT-CAP-08` | Hak akses per aksi (resep, pesanan, batal, diskon) | Platform Authorization | Pola `[AccessPermission(P.<Resource>.Resource, P.<Aksi>)]` di controller MCU; kebijakan per departemen-jabatan `BE/Models/SysAccessPolicy.cs` | `Ready to reuse` (pola) | Resource dan aksi Optik belum ada | `OPT-DEC-005` (petugas penjualan tidak boleh membuat resep) bergantung pada pemetaan jabatan RO/Sp.M/penjualan/Kepala Optik yang benar |
| `OPT-CAP-09` | Prefix modul Optik di registry | Tata kelola backend | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:50-76`: ada `Mcu`, `Hmd`, `Lab`, `Rad`; tidak ada prefix Optik | `Missing` | Daftarkan prefix (misalnya `Opt`) beserta pemilik sebelum model pertama dibuat (`QBE-MOD-002`, `QBE-MOD-003`) | **Blocker implementasi** seluruh model Optik |

### 3.3 Pemeriksaan, resep, dan rekam medis

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-10` | Pemeriksaan refraksi dan resep kacamata/lensa kontak (OD/OS SPH, CYL, AXIS, ADD, PD; BC, DIA) | Optik | `rg optik/optic/kacamata/refraksi/eyeglass/spectacle` di `BE/Areas`: hanya teks skala risiko jatuh ("memakai kacamata") di `ClinicalInstrumentDraftSeeder.cs:351`. `PhmPrescription` khusus obat | `Missing` | Dibangun baru di modul Optik | — |
| `OPT-CAP-11` | Revisi resep berversi (`OPT-DEC-007`, `011`) | Optik | Pola versi di Farmasi: `PhmMedicationAdministrationRevision`, `PhmSlidingScaleOrderVersion` | `Missing` (pola tersedia) | Pola revisi Farmasi dijadikan acuan | — |
| `OPT-CAP-12` | Hasil optik tampil di Rekam Medis (`OPT-DEC-024`) | Rekam Medis | `BE/Areas/HealthServices/MedicalRecordManagement/Services/Sources/IMedicalRecordDocumentSource.cs:28`; 11 pemasang, misalnya `McuMedicalRecordDocumentSource`, `PhmMedicalRecordDocumentSource` (ditulis di modul sumber dengan persetujuan pemilik, `RM-DEC-075`) | `Reuse with adapter` | Pasang `…MedicalRecordDocumentSource` milik Optik. Isinya hanya visus, refraksi, dan resep versi terakhir | Butuh persetujuan pemilik Rekam Medis |
| `OPT-CAP-13` | Nomor dokumen resep/pesanan | Platform | `BE/Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs`, dipakai Finance, HR, dan Bank Darah | `Ready to reuse` | Daftarkan kunci nomor Optik | — |

### 3.4 Barang, depo, dan stok

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-14` | Master barang frame, lensa, lensa kontak, cairan, aksesoris (`OPT-DEC-004`) | Master Data (dipakai Farmasi) | `BE/Areas/HealthServices/MasterData/Models/MstDrug.cs`: satu-satunya master barang. Ada `DrugCategoryId`, `BrandName`, `IsConsumable`, `IsStockManaged`, `IsBatchTracked`, `IsNeedPrescription`; **tidak ada** atribut warna, ukuran frame, indeks/jenis/lapisan lensa. Kamar Operasi memakai master ini untuk bahan dan implan (`OperatingRoomInventoryDispatchService.cs`) | `Extend` | Kategori barang Optik + aturan penamaan, atau atribut terstruktur baru (`OPT-Q-03`) | Barang Optik akan muncul di daftar pilihan obat bila penyaringnya tidak memilah kategori |
| `OPT-CAP-15` | Depo Optik | Farmasi | `MasterData/Models/MstDrugStorageLocation.cs` (`ServiceUnitId`, `ClinicId`, `IsAllowDispensing`, `IsAllowReceiving`, `IsAllowTransferIn`) | `Reuse with adapter` | Satu baris lokasi untuk unit Optik; keberadaan datanya `Unknown` | Pemilik Farmasi harus setuju (`OPT-OQ-003`) |
| `OPT-CAP-16` | Potong stok saat `Dalam Pengerjaan` (`OPT-DEC-019`) dan saat penjualan langsung diserahkan (`OPT-DEC-020`) | Farmasi | `PharmacyManagement/Services/DrugStockService.cs#IssueAsync` (`:635`), `#ReserveAsync` (`:570`), `#ReleaseReservationAsync` (`:611`); `IssueStockRequest` membawa `SourceDocumentType`/`SourceDocumentId`; jenis dokumen sumber di `Enums/DrugStockEnums.cs:75` | `Reuse with adapter` | Tambah jenis dokumen sumber Optik. Pemanggilan lewat service seperti Kamar Operasi, bukan menulis saldo sendiri | `IssueAsync` mewajibkan *Idempotency-Key* dan memeriksa mutasi sebelumnya dengan kunci yang sama (`EnsureIdempotencyKey`, `FindMutationsByKeyPrefixAsync`). `OPT-AC-008` terpenuhi **asal** Optik memakai kunci tetap per pesanan, bukan kunci baru per klik. **Contoh:** kunci `pesanan-ORD-0001-potong` dikirim dua kali sehingga stok hanya berkurang sekali |
| `OPT-CAP-17` | Frame kembali ke stok saat batal (`OPT-DEC-010`) | Farmasi | `DrugStockService.cs#ReceiveBatchAsync` (`:750`); mutasi punya `CorrectionOfMutationId` | `Reuse with adapter` | Tentukan jenis mutasi yang benar (koreksi atau penerimaan kembali) | Salah jenis mutasi mengacaukan laporan persediaan |
| `OPT-CAP-18` | Lab lensa luar sebagai tujuan pengerjaan (`OPT-DEC-013`) | Administrator Master Data | `BE/Areas/Administrator/MasterData/Models/MstSupplier.cs` | `Reuse with adapter` | Lab lensa dicatat sebagai supplier | — |

### 3.5 Harga, tagihan, uang muka, dan diskon

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-19` | Harga jual dari master, tidak diketik petugas (`OPT-DEC-021`) | Master Data / Billing | `MasterData/Models/MstTariff.cs` (`DrugId`, `ClinicId`, `PatientClassId`, `NormalPrice`, `EffectiveStartDate/EndDate`); `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs#FindDrugTariffAsync` (`:528`) memilih tarif paling spesifik yang berlaku | `Ready to reuse` | Tarif per barang Optik harus diinput | Barang tanpa tarif ditolak, tidak pernah Rp 0 (perilaku resolver) |
| `OPT-CAP-20` | Menerbitkan tagihan pesanan Optik | Billing | Jalur 1: `Operational/Constants/BillingSourceContract.cs`, daftar sumber resmi (Lab, Radiologi, Bank Darah, Kamar Operasi, Resep, dan lain-lain); sumber tak terdaftar ditolak `BIL_SOURCE_INVALID`. Jalur 2: `Billing/Services/BillingInvoiceService.cs#AddCatalogChargeAsync` (`:1356`), harga dari `MstTariff`, wajib *Idempotency-Key*, dipakai MCU (`McuCheckInService.cs:32`) | `Reuse with adapter` | Pilih jalur (`OPT-Q-04`) | `AddCatalogChargeAsync` menggabungkan baris dengan nama tarif sama dalam satu kunjungan, sehingga dua pesanan berframe sama bisa menyatu menjadi satu baris berjumlah 2 |
| `OPT-CAP-21` | Uang muka minimal 50% sebelum `Dalam Pengerjaan` (`OPT-DEC-008`, `009`) | Billing | `Billing/Models/BilDepositPaymentOrder.cs` (`SourceType` termasuk `OTHER`, `SourceReferenceId`, `RequestedAmount`, `PaidAmount`, `Status`); `BilDepositAccount` satu per kunjungan; `MasterData/Models/MstDepositPolicy.cs` `MinimumAmount` **nominal**; controller `Health Services / Billing Management / Billing / Deposit Orders` (`BillingDeposit : Create/Read`) | `Extend` | Minimum persentase per pesanan belum ada. Optik bisa menghitung 50% lalu meminta pesanan uang muka bernominal (`OPT-Q-02`) | Pemakaian uang muka untuk melunasi tagihan Optik belum diperiksa dari ujung ke ujung |
| `OPT-CAP-22` | Uang muka hangus saat batal atau tidak diambil (`OPT-DEC-010`, `018`) | Billing | Tidak ditemukan konsep penghangusan di `BillingDepositOrderService.cs`; yang ada pembatalan pesanan uang muka (`POST {id}/cancel`) dan refund (`BilRefundCase`) | `Missing` | Butuh keputusan pemilik Billing: uang muka hangus diakui sebagai pendapatan apa dan oleh siapa | Tanpa itu, uang muka tertahan di saldo deposit kunjungan |
| `OPT-CAP-23` | Diskon berbatas + persetujuan Kepala Optik (`OPT-DEC-021`) | Billing | `MasterData/Models/MstDiscountPolicy.cs` (`ValueType`, `Value`, `Limit`, `RequiresApproval`, `ApproverRole`); `Billing/Models/BilDiscountApplication.cs` (`InvoiceItemId`, `ApprovalStatus`, `RequestedBy`, `ApprovedBy`, `Reason`) | `Conflict` | Diskon tercatat di **tagihan** (wilayah Billing/kasir). `OPT-DEC-021` menempatkannya pada petugas penjualan di pesanan Optik (`OPT-Q-01`) | Dua tempat diskon menimbulkan potongan ganda |
| `OPT-CAP-24` | Optik membaca status lunas (penyerahan wajib lunas) | Billing | `BillingInvoiceService.cs#GetChargeSummaryByEncounterAsync` (`:1226`); pola gerbang lunas Lab `LabPaymentClearanceRules` | `Reuse with adapter` | Gerbang lunas Optik per pesanan | Status lunas dibaca per kunjungan, bukan per pesanan. Satu kunjungan dengan dua pesanan perlu dipilah |

### 3.6 Penjamin dan BPJS

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-25` | Kunjungan berpenjamin BPJS | Registrasi / Insurance | `RegistrationManagement/Enums/PatientEncounterGuarantorType.cs`: `BPJS = 4` ada, tetapi **nol pemakai** di `BE/Areas`. `EncounterPaymentType.cs` hanya `Cash`, `Insurance`, `CompanyGuarantor` | `Missing` | Fondasi BPJS (penjamin, SEP, kelas hak) belum ada | **Risiko terbesar MVP** — `OPT-DEC-014` tidak punya landasan |
| `OPT-CAP-26` | Plafon tanggungan per kelas | Insurance / Billing | `MstInsuranceCoverageRule` (`DrugCategoryId`, `PatientClassId`, `MaxAmountPerVisit`) dihitung `BillingCoverageAdapter.cs` | `Unknown` | Bisa dipakai bila BPJS dimodelkan sebagai penjamin asuransi. Belum terbukti untuk BPJS | Modul Insurance `PARTIAL`; produksi belum diotorisasi (`insurance-management/MODULE-STATUS.md:8`, `:28`) |
| `OPT-CAP-27` | Jeda minimal kacamata BPJS (misalnya 2 tahun) | Optik | Tidak ada | `Missing` | Optik membaca riwayat pesanan BPJS miliknya sendiri | Riwayat dari RS lain tidak terlihat |

### 3.7 Finance (titik sentuh tenant)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-28` | Menjelaskan `OPT-CONFLICT-001` (optik sebagai tenant) | Finance | `finance-management/00-interview-decisions.md:1102-1110`: "Dicatat sebagai **petunjuk**, bukan sebagai aturan yang sudah sah — sumbernya data contoh yang di-hardcode". Penagihan sewa tenant masih terbuka (`FIN-OQ-043`) | `Conflict` (dangkal) | Kemungkinan besar contoh v1, bukan tenant nyata (`OPT-Q-08`) | Bila ternyata ada optik swasta penyewa, laporan omzet Optik RS tidak boleh mencampurnya |

### 3.8 Frontend

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `OPT-CAP-29` | Menu Optik di sidebar v2 | FE Optik | Menu hidup `FE/src/utils/menu-sidebar/menu-items.jsx`: ada "Medical Check-Up" (`:2548`), tidak ada Optik. `FE/src/utils/config.jsx:111` "Pendaftaran Pasien Optik" **tidak diimpor berkas mana pun**. `left-sidebar-menu-handle.jsx:20` kunci `menuOptik`. `src/utils/auth/route-guard-link.js:2-3` route `/Optik` untuk peran Dokter/Perawat | `Missing` | Menu dan layar baru menurut screen brief (`OPT-FE-001`) | Sisa route-guard berbasis peran bisa membingungkan saat menu baru dibuat |
| `OPT-CAP-30` | Layar v1 sebagai acuan | — | `FE1/src/components/view/Optik/dashboard-optik.jsx:38-236`: array contoh `appointmentData`, `productsData`, `prescriptionsData` (OD/OS sphere/cylinder/axis/add, PD), `ordersData` (status `Processing`, `Manufacturing`, `Ready for Pickup`). `FE1/.../pasien-optik.jsx:140` → `POST /PendaftaranPasienOptik` (`PendaftaranPasienOptikSlices.jsx:91`), tidak ada di BE v2 | `Reuse with adapter` (acuan tampilan saja) | — | Perjanjian Optik v1 hanya memuat ulang layar perjanjian Radiologi (`FE1/src/app/Optik/Beranda/perjanjian-optik/page.jsx`) |
| `OPT-CAP-31` | Isian desimal resep tanpa kehilangan koma (`OPT-FE-003`) | FE bersama | `DECIMAL_FIELD_PROPS` didefinisikan ulang di `company-guarantor-coverage-rule-constants.jsx:46` dan `insurance-coverage-rule-constants.jsx:95`; `parseDecimal` dipakai di layar Mikrobiologi dan MCU | `Reuse with adapter` | Belum ada satu helper bersama; tanda minus (-1,50) dan kelipatan 0,25 harus diuji | `type: "number"` membuang desimal (-1,50 → -150) |

---

## 4. Kontrak as-is yang akan disentuh Optik

### Health Services / Billing Management / Billing / Deposit Orders

Base URL: `api/v1/health-services/billing-management/billing/deposit-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat pesanan uang muka untuk satu kunjungan | `BillingDeposit : Create` | body pesanan uang muka (`OrderType`, `SourceType`, `SourceReferenceId`, nominal) | detail pesanan uang muka |
| `GET` | `/` | Daftar pesanan uang muka | `BillingDeposit : Read` | query | `PagedResult` pesanan uang muka |
| `GET` | `/{id}` | Detail pesanan uang muka | `BillingDeposit : Read` | — | detail pesanan uang muka |
| `POST` | `/{id}/payments` | Kasir mencatat pembayaran uang muka | `BillingDeposit : Create` | body pembayaran | detail pembayaran |
| `POST` | `/{id}/cancel` | Membatalkan pesanan uang muka | `BillingDeposit : Create` | body alasan | detail pesanan uang muka |
| `GET` | `/payments/{paymentId}/receipt` | Kuitansi pembayaran uang muka | `BillingDeposit : Read` | — | berkas kuitansi |

Bukti: `BE/Areas/HealthServices/BillingManagement/Billing/Controllers/BillingDepositOrdersController.cs:14-187`.
Bentuk *payload* rinci tidak diaudit, dan pemakaian dari modul lain dibahas di desain.

**Service internal tanpa endpoint publik** (dipanggil dari service modul lain, bukan lewat HTTP):

| Service | Method | Dipakai untuk Optik | Pemakai sekarang |
| --- | --- | --- | --- |
| `EncounterIntakeService` | pendaftaran kunjungan penunjang | Kunjungan Optik | Lab, Radiologi, MCU |
| `DrugStockService` | `IssueAsync`, `ReserveAsync`, `ReleaseReservationAsync`, `ReceiveBatchAsync` | Potong dan kembalikan stok | Farmasi, Kamar Operasi |
| `BillingInvoiceService` | `AddCatalogChargeAsync` | Tagihan barang Optik | MCU |
| `NumberSeriesAllocator` | alokasi nomor | Nomor resep dan pesanan | Finance, HR, Bank Darah |

---

## 5. Pertentangan dan butir tidak diketahui

| ID | Jenis | Uraian | Memblokir |
| --- | --- | --- | --- |
| `OPT-CONFLICT-002` | Conflict | Diskon dicatat Billing di tagihan (`OPT-CAP-23`), sedangkan `OPT-DEC-021` menempatkannya pada petugas penjualan Optik | `DESIGN` (harga dan diskon) |
| `OPT-CONFLICT-003` | Conflict | `OPT-DEC-014` memasukkan BPJS ke MVP, padahal fondasi BPJS belum ada (`OPT-CAP-25`) dan modul Insurance `PARTIAL` | `DESIGN` (jalur BPJS) |
| `OPT-CONFLICT-004` | Conflict | Minimum uang muka persentase (`OPT-DEC-009`) dan uang muka hangus (`OPT-DEC-010`) tidak dikenal Billing (`OPT-CAP-21`, `22`) | `DESIGN` (uang muka) |
| `OPT-CONFLICT-001` | Conflict (dangkal) | Optik sebagai tenant hanya berasal dari data contoh v1 (`OPT-CAP-28`) | Tidak memblokir bila `OPT-Q-08` disetujui |
| `OPT-UNK-001` | Unknown | Unit layanan, depo, kategori barang, dan profesi RO di basis data (`OPT-CAP-04`, `07`, `15`) | `IMPLEMENTATION` |
| `OPT-UNK-002` | Unknown | Apakah aturan tanggungan asuransi mampu memodelkan plafon BPJS per kelas (`OPT-CAP-26`) | `DESIGN` (jalur BPJS) |

---

## 6. Pertanyaan penutup untuk closure pass

| ID | Pertanyaan | Pemilik | Dasar |
| --- | --- | --- | --- |
| `OPT-Q-01` | Di mana diskon dicatat: di pesanan Optik (Optik mengirim harga bersih), atau lewat kebijakan diskon Billing yang diajukan petugas Optik dan disetujui Kepala Optik sebagai `ApproverRole`? | Yoga Aji Pratama + pemilik Billing | `OPT-CONFLICT-002` |
| `OPT-Q-02` | Uang muka: apakah Optik menghitung 50% lalu meminta pesanan uang muka bernominal ke Billing? Siapa yang mengakui uang muka hangus dan bagaimana caranya? | Yoga Aji Pratama + pemilik Billing | `OPT-CONFLICT-004` |
| `OPT-Q-03` | Barang Optik di master obat: cukup kategori "Optik" dengan nama/kode deskriptif, atau perlu atribut terstruktur (warna, ukuran, indeks lensa) untuk pencarian dan laporan? | Yoga Aji Pratama + pemilik Master Data/Farmasi | `OPT-CAP-14` |
| `OPT-Q-04` | Penagihan Optik lewat jalur katalog tarif (seperti MCU, tanpa ubah kontrak Billing) atau didaftarkan sebagai sumber resmi baru di `BillingSourceContract`? | Yoga Aji Pratama + pemilik Billing | `OPT-CAP-20` |
| `OPT-Q-05` | BPJS: tetap di MVP dengan menunggu fondasi BPJS dibangun pemiliknya, atau dipindah ke slice berikutnya (amandemen `OPT-DEC-014`)? | Yoga Aji Pratama | `OPT-CONFLICT-003` |
| `OPT-Q-06` | Mendaftarkan prefix modul Optik (usulan `Opt`) dengan Yoga Aji Pratama sebagai pemegang modul | Yoga Aji Pratama | `OPT-CAP-09` |
| `OPT-Q-07` | Unit Optik: tambah nilai jenis unit layanan baru, atau pakai `Other` dengan kode unit khusus? | Yoga Aji Pratama + pemilik Master Data | `OPT-CAP-03` |
| `OPT-Q-08` | Menutup `OPT-CONFLICT-001`: "optik" di Finance = data contoh v1, bukan tenant nyata? | Yoga Aji Pratama | `OPT-CAP-28` |
| `OPT-Q-09` | Rujukan Poli Mata: cukup dokter perujuk, atau harus tertaut ke kunjungan Poli Mata asal? | Yoga Aji Pratama | `OPT-CAP-05` |

---

## 7. Pemicu impact scan

Peta ini dianggap basi dan perlu dipindai ulang secara terbatas bila salah satu berikut berubah
sejak SHA di atas:

- `EncounterIntakeService.cs`, `ServiceUnitType.cs`, `PatientEncounterGuarantorType.cs`, `EncounterPaymentType.cs`;
- `DrugStockService.cs`, `DrugStockEnums.cs`, `MstDrug.cs`, `MstDrugStorageLocation.cs`;
- `BillingSourceContract.cs`, `BillingInvoiceService.cs#AddCatalogChargeAsync`, `BilDepositPaymentOrder.cs`, `MstDepositPolicy.cs`, `MstDiscountPolicy.cs`;
- `WfpCredentialLicense.cs`, `IMedicalRecordDocumentSource.cs`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
- status modul Insurance berubah dari `PARTIAL`;
- `FE/src/utils/menu-sidebar/menu-items.jsx`.
