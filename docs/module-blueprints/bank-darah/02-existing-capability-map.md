# Bank Darah — Existing Capability Map

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Blueprint revision | `3` |
| Capability map revision | `3` |
| Status | `DRAFT` |
| Sumber keputusan | `00-interview-decisions.md` revisi 2, `SCOPE-BD-001` sampai `DEC-BD-024` |
| Backend SHA audit penuh | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Backend SHA impact scan terakhir | `4205d18a6d656555eedd781f14e8a18fb5ea20d1` cabang `sukmagp` — 3 September 2026 |
| Status kesegaran | **`CURRENT`** — penanda `STALE` dicabut oleh impact scan di bawah |
| Frontend SHA yang diaudit | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Tanggal audit | `2026-09-02` |
| Mode | Read-only. Tidak ada satu pun berkas source aplikasi yang diubah. |

Dokumen ini mencatat **apa yang sudah ada di sistem hari ini**, bukan arsitektur yang diinginkan.
Setiap baris memakai tepat satu status dari taksonomi baku: `Ready to reuse`, `Reuse with adapter`,
`Extend`, `Repair`, `Missing`, `Conflict`, atau `Unknown`.

Rujukan bukti per baris tetap menyebut `@9522caa`, yaitu SHA saat audit penuh dijalankan. Itu
disengaja: audit penuh memang dilakukan di sana, dan impact scan **tidak** menggantikannya. Bagian
berikut mencatat apa yang diperiksa ulang pada `4205d18` beserta hasilnya.

---

## Impact scan terbatas — 3 September 2026

**Pemicu.** Backend bergerak dari `a9bc9fd` ke `4205d18` lewat merge `QuilvianIntegrationBackend` ke
`sukmagp`. Berbeda dengan seluruh pergerakan SHA sebelumnya pada modul ini — yang seluruhnya dokumen
blueprint — merge ini membawa perubahan source aplikasi nyata, sehingga peta ditandai `STALE`.

**Batas scan.** Terbatas pada `BD-CAP-014`, `BD-CAP-003`, dan dampak snapshot migration, ditambah dua
baris yang areanya ikut tersentuh (`BD-CAP-015`, `BD-CAP-006`). **Bukan** audit ulang 24 kemampuan.

### Cara batasnya dipertanggungjawabkan

Membatasi scan pada beberapa baris hanya sah bila baris lain memang tidak tersentuh. Itu diperiksa
secara menyeluruh, bukan diasumsikan: seluruh nama berkas `.cs` yang dikutip peta ini diadu dengan
daftar berkas yang berubah antara `9522caa` dan `4205d18`.

| Pemeriksaan | Hasil |
| --- | --- |
| Berkas `.cs` yang dikutip peta | 24 |
| Berkas `.cs` yang berubah karena merge | 28 |
| **Irisan keduanya** | **1 berkas — `LabOrder.cs`** |

Hanya satu berkas bukti yang tersentuh. Dua puluh tiga berkas bukti lainnya tidak berubah sama sekali,
sehingga baris yang bergantung padanya tetap sahih tanpa perlu ditelusuri ulang.

### Hasil per baris yang diperiksa

| Baris | Bukti yang dikutip | Berubah? | Putusan |
| --- | --- | --- | --- |
| `BD-CAP-014` pola route & grup Swagger | `LabOrderController.cs` | **Tidak** | **Tetap `Ready to reuse`.** `[Route]`, `[Tags]`, `[Authorize]`, `[ApiController]`, `[AccessController]`, dan pembungkus `ApiResponse<T>` seluruhnya identik. Pola yang dicontoh `api-contract.md` `v4` tidak bergeser |
| `BD-CAP-003` sinyal penutupan kunjungan | `InpEpisode.cs`, `EncounterStatus.cs` | **Tidak** | **Tetap `Reuse with adapter`.** Kelima field yang dipakai `DEC-BD-014` — `EpisodeStatus`, `DischargeDecidedAt`, `PhysicallyLeftAt`, `ClosedAt`, `DischargeType` — masih ada dan tidak berubah. Yang berubah hanya **controller** InPatient, yang tidak dipanggil Bank Darah |
| `BD-CAP-007` pola pesanan terikat kunjungan | `LabOrder.cs` | **Ya, aditif** | **Tetap `Reuse with adapter`.** Yang bertambah satu kolom `Discipline`. Seluruh field yang dikutip — `EncounterId`, `ProcedureId`, `StatusBeforeHold`, `RequestedAt`, `RequestedByUserId`, `CompletedAt` — utuh |
| `BD-CAP-010` token konkurensi | `LabOrder.cs#Version` | **Tidak** | **Tetap `Ready to reuse`.** `public int Version` tidak tersentuh sama sekali |
| `BD-CAP-015` penyerahan fakta biaya | `BillingSourceContract.cs` | **Tidak** | **Tetap `Extend`.** Daftar sumber tertutup tidak berubah; Bank Darah tetap belum ada di dalamnya, sehingga `DEC-BD-016` tetap dibutuhkan |
| `BD-CAP-006` klinik, ruangan, kelas pasien | `MstClinic.cs`, `MstRoom.cs`, `MstPatientClass.cs`, `MstServiceUnit.cs` | **Tidak** | **Tetap `Ready to reuse`.** Yang berubah di MasterData hanya `BedController` dan `InpatientClearanceItem*`, keduanya di luar pemakaian Bank Darah |

### Dampak migration

| Pemeriksaan | Hasil |
| --- | --- |
| Migration baru sejak `9522caa` | `20260901082243_AddBilTenderKwitansiNumber`, `20260902042242_AddLabOrderDiscipline` |
| Basis migration Bank Darah | Kini `20260902042242_AddLabOrderDiscipline`, bukan lagi migration per `9522caa` |
| Entity Bank Darah di `ApplicationDbContextModelSnapshot.cs` | **Nihil** — tidak ada `Bbk*` maupun `MstBlood*`, sesuai harapan karena belum ada implementasi |
| `MstServiceUnit` di snapshot | Ada, dengan `IsAvailableForDisplay`, `IsAvailableForRegistrationQueue`, `IsAvailableForScreening`, dan **satu index gabungan** atas ketiganya bersama `IsActive`, `IsDelete` |
| `MstDrugStorageLocation` di snapshot | **Ada** — menguatkan `DEC-BD-035` yang menolak memakainya ulang, dan menegaskan celah cakupan audit pada `BD-CAP-006` |

Rencana migration Bank Darah (`02-backend-architecture.md` §I) **tidak perlu berubah**: ketiga
langkahnya tetap sah, dan seluruhnya membuat objek baru yang tidak bersinggungan dengan kedua migration
baru itu. Yang bergeser hanya titik basisnya.

### Dua temuan yang menguatkan blueprint, bukan membatalkannya

**1. Laboratory menyatakan Bank Darah di luar scope-nya, dengan kata-katanya sendiri.**
Enum `LabDiscipline` yang baru memuat keterangan:

> *"Ketiganya berjalan sejajar dengan daftar pasien dan alur hasilnya masing-masing: Patologi Klinik,
> Patologi Anatomi, dan Mikrobiologi. Bank Darah sengaja tidak ada di sini karena tetap berada di luar
> scope modul."*

Ini menguatkan `DEC-BD-015` dan `DEC-BD-018` — pemeriksaan golongan darah dan sampel Bank Darah berada
di Bank Darah, bukan di Laboratorium — serta batas `BD-CTX-09` yang menyatakan keduanya "jalan
sendiri-sendiri". Batas itu kini **berbukti dua arah**: bukan hanya keputusan Bank Darah, tetapi juga
tercatat pada kode modul tetangganya.

**2. Pemecahan butir hak akses ternyata pola rumah, bukan penemuan Bank Darah.**
Tim InPatient memecah `AccessAction` menjadi butir tersendiri pada merge ini — `Sign`, `SetIsolation`,
`Reopen`, `MarkFinancialClearance`, dan `ReadFinancialClearance` — dengan alasan yang dinyatakan
langsung di kode: supaya kasir dapat diberi kemampuan menandai **tanpa** ikut memperoleh akses baca isi
resume pulang.

Itu persis alasan `DEC-BD-043` memecah `BloodUnit : Resolve` menjadi tiga dan `DEC-BD-044` memisahkan
`BloodOrder : Cancel` dari `Update`. Rancangan hak akses `v4` karena itu **mengikuti konvensi yang
sedang berlaku**, bukan menciptakan pola baru.

### Satu catatan kecil untuk task migration, bukan cacat blueprint

`MstServiceUnit` memasangkan ketiga penanda `IsAvailableFor*` yang sudah ada dengan **satu index
gabungan**. Blueprint `BE-BD-002` menambahkan `IsAvailableForBloodOrder` tanpa menyebut index.

Itu **bukan** kekeliruan: jalur akses utamanya adalah pemeriksaan satu unit berdasarkan `Id` saat order
dibuat, yang tidak menuntut index. Index baru relevan hanya bila kelak ada layar yang menyaring daftar
unit berdasarkan penanda ini. Dicatat sebagai bahan pertimbangan saat `BE-BD-002` dieksekusi, bukan
sebagai perubahan kontrak.

### Putusan impact scan

**Blueprint Bank Darah tidak perlu diubah.** Tidak ada satu pun baris kemampuan yang berpindah status,
tidak ada kontrak `v4` yang menjadi salah, dan tidak ada keputusan yang perlu ditinjau ulang. Penanda
`STALE` **dicabut**.

Yang berubah hanya dua hal administratif: basis migration bergeser ke
`20260902042242_AddLabOrderDiscipline`, dan SHA impact scan tercatat `4205d18`.

---

## Batas audit

Audit dibatasi pada klaster kemampuan yang benar-benar disentuh Bank Darah menurut batas scope yang
sudah dikunci pada `SCOPE-BD-001`:

| Klaster | Yang dicari |
| --- | --- |
| Identity/Master Owner | Pasien dan golongan darahnya |
| Episode/Transaction Owner | Kunjungan pasien beserta sinyal penutupannya |
| Actor/Workforce | Dokter dan pengguna pelaku tindakan |
| Location/Resource | Unit pelayanan, klinik, ruangan, kelas pasien |
| Workflow/Status | Pola order klinis, perpindahan status, kunci konkurensi |
| Documentation/Record | Riwayat perpindahan status yang hanya bisa ditambah |
| Order/Result | Pola pesanan Laboratorium dan Radiologi sebagai pembanding terdekat |
| Financial | Kontrak penyerahan fakta biaya ke Billing |
| Authorization/Audit | Atribut hak akses, jejak audit dasar, kontrak response |
| External Integration | Jejak integrasi PMI dan HCLAB |

Yang **tidak** diaudit: aturan internal modul tetangga, seluruh daftar di luar scope pada BRD §9,
dan kemampuan yang tidak disentuh Bank Darah.

---

## Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap / adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `BD-CAP-001` | Data pasien sebagai rujukan order darah | PatientManagement | `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#MstPatient@9522caa` | `Ready to reuse` | Bank Darah cukup menyimpan `PatientId` | Rendah |
| `BD-CAP-002` | Kunjungan pasien sebagai konteks order | RegistrationManagement | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#TrxPatientEncounter@9522caa` — memuat `PatientId`, `ServiceUnitId`, `ClinicId`, `RoomId`, `DoctorId`, `PatientClassId`, `EncounterType`, `EncounterStatus`, `EncounterNumber` | `Ready to reuse` | Seluruh rujukan yang dibutuhkan `DEC-BD-003` dan `DEC-BD-004` sudah tersedia pada satu entity | Rendah |
| `BD-CAP-003` | Sinyal "kunjungan sudah ditutup" untuk menentukan order kedaluwarsa | RegistrationManagement dan InPatientManagement | `Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs@9522caa` — nilai akhirnya `Completed`, `Cancelled`, `NoShow`; tidak ada nilai `Closed`. `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs#InpEpisode@9522caa` — punya `EpisodeStatus`, `DischargeDecidedAt`, `PhysicallyLeftAt`, `ClosedAt`, `DischargeType` tersendiri | `Reuse with adapter` — semula `Conflict`, ditutup `DEC-BD-014` | Dipakai lewat dua penyesuai berbeda: status kunjungan untuk rawat jalan dan IGD, waktu pasien meninggalkan rumah sakit untuk rawat inap. Bank Darah hanya membaca, tidak mengubah | Sedang |
| `BD-CAP-004` | Data dokter peminta dan dokter perujuk | HR — Master Data Workforce | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs#MstDoctor@9522caa`, dirujuk `TrxPatientEncounter.DoctorId` | `Ready to reuse` | Cukup menyimpan `DoctorId` | Rendah |
| `BD-CAP-005` | Kewenangan unit pelayanan memesan darah, tanpa dikunci di kode | HealthServices — Master Data | `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs#MstServiceUnit@9522caa` — sudah memakai pola tanda kemampuan per unit: `IsAvailableForRegistration`, `IsAvailableForKiosk`, `IsAvailableForAppointment`, `IsQueueRequired`, `IsDoctorRequired`, `IsScreeningRequired` | **`Extend`** | Tambahkan satu tanda kemampuan baru bergaya sama, misalnya penanda "unit ini boleh memesan darah". `DEC-BD-012` terpenuhi tanpa membuat mekanisme konfigurasi baru | Rendah |
| `BD-CAP-006` | Klinik, ruangan, dan kelas pasien | HealthServices — Master Data | `MstClinic.cs`, `MstRoom.cs`, `MstPatientClass.cs`, `MstServiceUnit.cs@9522caa` | `Ready to reuse` | Cukup menyimpan rujukannya | Rendah |
| `BD-CAP-007` | Pola pesanan klinis yang terikat kunjungan | LaboratoryManagement | `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs#LabOrder@9522caa` — `EncounterId`, `ProcedureId`, status pesanan, `StatusBeforeHold`, `RequestedAt`, `RequestedByUserId`, `CompletedAt`, `Version` | `Reuse with adapter` | Dipakai sebagai **pola**, bukan sebagai entity bersama. Bank Darah membuat entity sendiri dengan bentuk yang sama | Rendah |
| `BD-CAP-008` | Pola baris rincian di bawah satu pesanan, lengkap dengan salinan tarif | LaboratoryManagement | `Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs#TrxLabSpecimen@9522caa` — `ProcedureId`, `ProcedureCodeSnapshot`, `ProcedureNameSnapshot`, `TariffId`, `TariffCodeSnapshot`, `SupersededSpecimenId` | `Reuse with adapter` | Pola salinan tarif dipakai ulang agar pengiriman ke Billing tetap dapat diulang tanpa berubah. Perbedaannya, nomor kantong darah datang dari PMI dan bukan dibuat server — lihat `ASM-BD-003` | Sedang |
| `BD-CAP-009` | Riwayat perpindahan status yang hanya bisa ditambah | LaboratoryManagement | `Areas/HealthServices/LaboratoryManagement/Models/TrxLabTransitionHistory.cs#TrxLabTransitionHistory@9522caa` — `Scope`, `Action`, `FromStatus`, `ToStatus`, `ReasonCode`, `ReasonNote`, `ActorUserId`, `OccurredAt`, `CorrelationId`; tidak ada jalur update di service | `Reuse with adapter` | Pola langsung untuk pencatatan pergerakan kantong darah yang diminta BG-BD-004 dan `DEC-BD-007` | Rendah |
| `BD-CAP-010` | Pengaman agar satu kantong tidak terpakai dua kali | LaboratoryManagement | `LabOrder.cs#Version@9522caa` — token konkurensi bertipe `int` | `Ready to reuse` | Pola yang sama dipakai untuk alokasi kantong. Aturan unik alokasi aktif tetap harus dirancang tersendiri | Sedang |
| `BD-CAP-011` | Jejak audit dasar dan penghapusan lunak | Platform backend | `Models/IdentityModel.cs#IdentityModel@9522caa` — `CreateBy`, `UpdateBy`, `DeleteBy`, `CancelBy`, `IsCancel`, `IsDelete` beserta waktunya | `Ready to reuse` | Memenuhi larangan hapus keras pada BR-BD-010 | Rendah |
| `BD-CAP-012` | Bentuk response dan daftar bertingkat | Platform backend | `Responses/ApiResponse.cs`, `Responses/PagedResult.cs@9522caa` | `Ready to reuse` | Dipakai apa adanya | Rendah |
| `BD-CAP-013` | Hak akses tingkat controller dan tindakan | Platform backend | `Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs@9522caa`; contoh pemakaian pada `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs@9522caa` dengan `[AccessPermission("LabOrder", "Read")]`, `"Create"`, `"Process"`, `"Hold"`, `"Update"` | `Ready to reuse` | Kelompok kewenangan BRD §14 dipetakan ke pola ini. Tidak ada model keamanan baru | Rendah |
| `BD-CAP-014` | Konvensi route dan penamaan grup Swagger | Platform backend | `LabOrderController.cs@9522caa` — `[Route("api/v1/health-services/laboratory-management/lab-orders")]`, `[Tags("Health Services / Laboratory Management / Lab Order")]`, `[Authorize]` | `Ready to reuse` | Route Bank Darah mengikuti bentuk yang sama. Jangan menyimpulkan route dari URL frontend | Rendah |
| `BD-CAP-015` | Penyerahan fakta biaya ke Billing secara idempotent | BillingManagement dan ClinicalManagement | `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs@9522caa` — daftar sumber tertutup berisi `InternalTest`, `Prescription`, `Procedure`, `Laboratory`, `Radiology`; `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs#EmitChargeEligibilityAsync@9522caa`; contoh pemakaian pada `LabSpecimenService.cs@9522caa` | **`Extend`** | Bank Darah belum ada di daftar sumber yang diizinkan. Perlu penambahan satu konteks sumber dan satu jenis efek biaya. Sampai itu ada, BR-BD-004 tidak dapat mengirim biaya ke Billing | Sedang |
| `BD-CAP-016` | Nilai golongan darah dan Rhesus | Platform backend | `Enums/BloodType.cs#BloodType@9522caa` — `APositive` sampai `ONegative`, ditambah `Unknown` dan `NotDisclosed`; dipakai `MstPatient.BloodType@9522caa` | `Ready to reuse` untuk golongan darah **yang diminta** | ABO dan Rhesus digabung dalam satu nilai. Cukup untuk `DEC-BD-011` | Rendah |
| `BD-CAP-017` | Sumber sah golongan darah pasien | belum ditetapkan | `MstPatient.BloodType@9522caa` adalah data induk pasien, **bukan** hasil pemeriksaan laboratorium yang tervalidasi. Tidak ditemukan entity hasil pemeriksaan golongan darah di `LaboratoryManagement` | `Missing` — semula `Conflict`, ditutup `DEC-BD-015` | Sumber sah ditetapkan sebagai hasil pemeriksaan tersendiri milik Bank Darah, dan `MstPatient.BloodType` dikunci sebagai data administratif saja. Kemampuannya sendiri belum ada dan dibangun baru | Sedang |
| `BD-CAP-018` | Katalog komponen darah — PRC, TC, FFP, dan lainnya | belum ada | Tidak ditemukan katalog komponen darah di `Areas/HealthServices/MasterData/Models/@9522caa`. `MstProcedure` adalah katalog tindakan, bukan komponen darah | `Missing` | `DEC-BD-005` memakai komponen darah sebagai salah satu penanda order ganda, sehingga komponen tidak boleh berupa ketikan bebas. Katalog ini menjadi kebutuhan baru | Sedang |
| `BD-CAP-019` | Seluruh kapabilitas inti Bank Darah — order darah, permintaan ke PMI, penerimaan kantong, kantong operasional, alokasi, pemberian | belum ada | Tidak ada folder Blood Bank di `Areas/HealthServices/@9522caa`. Pencarian kata kunci `blood`, `bank darah`, `transfusi`, `BDRS` pada seluruh `Areas/`, `Models/`, dan `Repositories/` tidak menemukan satu pun entity, controller, atau service Bank Darah | `Missing` | Seluruh BR-BD-001 sampai BR-BD-019 dibangun baru | — |
| `BD-CAP-020` | Halaman Bank Darah pada frontend | belum ada | Tidak ada route Bank Darah di `V2QuilvianSystemFrontendDev/src/app/health-services/@afbb8ab`. Folder `app/administrator/master-data/bank` adalah data induk bank keuangan, bukan Bank Darah. Kemunculan kata "golongan darah" lain hanyalah field pada data induk pasien, pegawai, dan dokter | `Missing` | Seluruh layar Bank Darah dibangun baru | — |
| `BD-CAP-021` | Komponen dasar frontend yang dapat dipakai ulang | Frontend V2 | `V2QuilvianSystemFrontendDev/src/components/features/base-features/@afbb8ab` — `hero.jsx`, `data-table.jsx`, `data-filter.jsx`, `filter-select.jsx`, `filter-date-picker.jsx`, `base-button.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx`, `base-detail-view.jsx`, `base-editor-form.jsx` | `Ready to reuse` | Seluruh komponen yang disebut PRD §8 memang tersedia. Dilarang membuat komponen dasar tandingan | Rendah |
| `BD-CAP-022` | Pemisahan data per fasilitas kesehatan | Platform backend | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs@9522caa` ada, tetapi pencarian `HospitalSite` pada seluruh `Areas/HealthServices/@9522caa` tidak menghasilkan satu pun rujukan. `TrxPatientEncounter` tidak memiliki kolom fasilitas | `Ready to reuse` | Layanan kesehatan Quilvian saat ini tidak memisahkan data per fasilitas. Bank Darah mengikuti pola yang sama dan tidak menambahkan pemisahan baru | Rendah |
| `BD-CAP-023` | Integrasi PMI | belum ada | Tidak ditemukan satu pun kode, konfigurasi, atau konstanta yang menyebut PMI pada kedua repository | `Missing` | Sesuai `DEC-BD-002`, memang tidak dibutuhkan pada MVP. Permintaan ke PMI dicatat, pengirimannya manual | — |
| `BD-CAP-024` | Integrasi HCLAB — workstation `BANK DARAH`, kode `BBW`, Lab Sec `GL` | belum ada | Tidak ditemukan rujukan HCLAB, `BBW`, maupun `GL` pada `Areas/HealthServices/LaboratoryManagement/@9522caa` | `Unknown` | BR-BD-014 hanya menuntut dokumen penelusuran integrasi, bukan implementasi. Perlu bukti dari luar repository | Sedang |

## Ringkasan status

| Status | Jumlah | ID |
| --- | --- | --- |
| `Ready to reuse` | 12 | `BD-CAP-001`, `002`, `004`, `006`, `010`, `011`, `012`, `013`, `014`, `016`, `021`, `022` |
| `Reuse with adapter` | 4 | `BD-CAP-003`, `007`, `008`, `009` |
| `Extend` | 2 | `BD-CAP-005`, `015` |
| `Missing` | 5 | `BD-CAP-017`, `018`, `019`, `020`, `023` |
| `Conflict` | 0 | keduanya ditutup pada closure pass 2026-09-02 |
| `Unknown` | 1 | `BD-CAP-024` |
| `Repair` | 0 | — |

---

## Kontrak as-is yang akan diikuti Bank Darah

### Health Services / Laboratory Management / Lab Order

Grup ini bukan milik Bank Darah. Ia dicantumkan sebagai **contoh kontrak yang sudah berjalan**,
supaya kontrak Bank Darah nanti mengikuti bentuk yang sama dan bukan bentuk baru.

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat daftar order laboratorium | `LabOrder : Read` | query | `ApiResponse<PagedResult<T>>` |
| `GET` | `/{id}` | Melihat detail satu order | `LabOrder : Read` | — | `ApiResponse<T>` |
| `POST` | `/` | Membuat order pemeriksaan | `LabOrder : Create` | body DTO | `ApiResponse<T>` |
| `POST` | `/{id}` proses | Menandai order mulai dikerjakan | `LabOrder : Process` | body DTO | `ApiResponse<T>` |
| `POST` | `/{id}` tahan | Menahan dan melanjutkan order | `LabOrder : Hold` | body DTO | `ApiResponse<T>` |
| `POST` | `/{id}` batal | Membatalkan order | `LabOrder : Update` | body DTO | `ApiResponse<T>` |

Kode status yang perlu dijelaskan artinya bagi pengguna: `200` permintaan berhasil; `400` isian yang
dikirim tidak lengkap atau formatnya salah; `401` pengguna belum masuk; `403` pengguna tidak punya
hak akses untuk tindakan ini; `404` data yang dicari tidak ditemukan; `409` dua petugas mengubah
data yang sama pada waktu hampir bersamaan, atau status data sudah berubah.

Kontrak endpoint Bank Darah sendiri **belum ada** dan tidak dibuat pada tahap ini.

### Kontrak penyerahan biaya ke Billing

Modul klinis tidak menghitung tagihan sendiri. Ia mengirim fakta kelayakan biaya ke Billing melalui
`ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync`, dengan konteks sumber dan jenis efek yang
harus terdaftar pada `BillingSourceContract`. Daftar itu tertutup, dan Bank Darah belum ada di
dalamnya.

**Contoh cara kerja yang sudah berjalan di Laboratorium.** Satu pesanan berisi tiga pemeriksaan
senilai Rp200.000, Rp150.000, dan Rp100.000. Dua sampel dinyatakan layak dan satu ditolak, sehingga
yang ditagihkan Rp350.000. Tarif disalin ke baris sampel saat direncanakan, bukan dibaca ulang saat
pengiriman, supaya pengiriman ulang menghasilkan isi yang sama persis dan Billing mengenalinya
sebagai pengiriman ulang — bukan tagihan baru.

---

## Temuan yang perlu keputusan manusia

### `BD-CAP-003` — Dua sinyal penutupan kunjungan yang berbeda — **SELESAI**

> Ditutup `DEC-BD-014` pada closure pass 2026-09-02. Rawat jalan dan IGD memakai status akhir
> kunjungan; rawat inap memakai waktu pasien benar-benar meninggalkan rumah sakit, bukan penutupan
> administratif episode. Uraian di bawah dipertahankan sebagai catatan temuan.


`DEC-BD-006` menyatakan order darah kedaluwarsa ketika kunjungan asalnya ditutup. Source
memperlihatkan keadaan yang lebih rumit dari itu.

Untuk rawat jalan dan IGD, kunjungan berakhir lewat `EncounterStatus` dengan nilai akhir `Completed`,
`Cancelled`, atau `NoShow`. Tidak ada nilai bernama `Closed`.

Untuk rawat inap, ada lapisan tersendiri: `InpEpisode` punya `EpisodeStatus`, `DischargeDecidedAt`,
`PhysicallyLeftAt`, `ClosedAt`, dan `DischargeType`. Pasien rawat inap yang sudah pulang belum tentu
tercermin pada `EncounterStatus`.

**Yang harus diputuskan:** untuk tiap jenis kunjungan, sinyal mana yang menandai kunjungan sudah
ditutup bagi Bank Darah. Contoh pertanyaan yang harus dijawab: apakah pasien rawat inap yang sudah
`PhysicallyLeftAt` tetapi episodenya belum `ClosedAt` sudah membuat order darahnya kedaluwarsa?

Sampai ini diputuskan, `DEC-BD-006` dan kriteria uji `AC-BD-004` serta `AC-BD-007` belum dapat
dibangun.

### `BD-CAP-017` — Golongan darah pasien mudah disalahpahami sebagai hasil pemeriksaan — **SELESAI**

> Ditutup `DEC-BD-015` pada closure pass 2026-09-02. Sumber sah adalah hasil pemeriksaan tersendiri
> milik Bank Darah; `MstPatient.BloodType` dikunci sebagai data administratif saja. Uraian di bawah
> dipertahankan sebagai catatan temuan.


`MstPatient.BloodType` sudah ada dan terisi lewat pendaftaran pasien. Ia adalah data induk
administratif, bukan hasil pemeriksaan laboratorium yang divalidasi. Tidak ditemukan entity hasil
pemeriksaan golongan darah di modul Laboratorium.

`INV-BD-011` melarang memakai golongan darah yang belum tervalidasi sebagai dasar keputusan
kesesuaian darah. Risikonya nyata: field yang sudah tersedia dan terlihat rapi sangat mudah dipakai
sebagai jalan pintas.

**Yang harus diputuskan:** BR-BD-011 — siapa sumber yang sah untuk golongan darah dan Rhesus, siapa
yang memvalidasinya, dan kapan label boleh dicetak.

---

## Peluang pemakaian ulang yang ditemukan

1. **Jangan membuat model keamanan baru.** `BD-CAP-013` sudah menyediakan pola hak akses tingkat
   tindakan yang persis cocok dengan kelompok kewenangan BRD §14.
2. **Jangan membuat mekanisme konfigurasi unit baru.** `BD-CAP-005` memperlihatkan `MstServiceUnit`
   sudah memakai pola tanda kemampuan per unit. `DEC-BD-012` terpenuhi hanya dengan menambah satu
   tanda bergaya sama.
3. **Jangan merancang pencatatan riwayat dari nol.** `BD-CAP-009` sudah membuktikan pola riwayat
   yang hanya bisa ditambah, lengkap dengan penyalinan kode alasan sebagai teks supaya riwayat lama
   tidak berubah makna ketika alasan dinonaktifkan.
4. **Jangan membuat perhitungan tarif sendiri.** `BD-CAP-015` memperlihatkan modul klinis hanya
   mengirim fakta kelayakan biaya, dan Billing yang menentukan akibat finansialnya.
5. **Jangan membuat komponen tampilan dasar tandingan.** `BD-CAP-021` membuktikan seluruh komponen
   yang disebut PRD §8 sudah ada.

---

## Pemicu peta menjadi usang

Peta ini terikat pada backend `9522caa` dan frontend `afbb8ab`. Bila salah satu SHA berubah, tandai
peta ini `STALE` lalu jalankan pemindaian dampak terbatas pada berkas berikut sebelum peta dipakai
lagi:

`TrxPatientEncounter.cs` · `EncounterStatus.cs` · `InpEpisode.cs` · `MstServiceUnit.cs` ·
`MstPatient.cs` · `Enums/BloodType.cs` · `LabOrder.cs` · `TrxLabSpecimen.cs` ·
`TrxLabTransitionHistory.cs` · `BillingSourceContract.cs` · `ClinicalMilestoneFactProducer.cs` ·
`Attributes/Access*.cs` · `Responses/ApiResponse.cs` · `Responses/PagedResult.cs` ·
`src/components/features/base-features/`.

## Pertanyaan penutup

| Pertanyaan | Keadaan |
| --- | --- |
| Sinyal mana yang menandai kunjungan ditutup, per jenis kunjungan? | **Terjawab** `DEC-BD-014` |
| Siapa sumber sah golongan darah dan Rhesus? | **Terjawab** `DEC-BD-015`. Siapa validatornya masih `DEF-BD-004` |
| Katalog komponen darah dibuat sebagai data induk baru? | **Terjawab** `DEC-BD-024` — menjadi isi Setup Bank Darah |
| Apakah penambahan konteks sumber Bank Darah pada kontrak Billing disetujui pemiliknya? | **Belum** — `DEC-BD-016` masih terbuka. Pemicunya kini jelas: satu tindakan Bank Darah yang selesai (`DEC-BD-021`) |
| Adakah bukti integrasi HCLAB di luar repository? | **Belum** — `DEC-BD-022` menempatkan HCLAB di luar MVP dan hanya sebagai temuan penelusuran |
