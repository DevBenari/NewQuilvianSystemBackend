# Rawat Inap — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Capability-map revision | **`1.7`** — ditambah **bagian 20: Audit kemampuan Workspace PPRI `PRD-RWI-ADMISI-001` (7 Oktober 2026)**, 54 kemampuan `PPRI-CAP-01` s.d. `PPRI-CAP-54`. Bagian 19 dinyatakan `STALE` terhadap `BE@671191e` untuk butir yang terbukti berubah (bagian 20.1). Sebelumnya `1.6` — ditambah **bagian 19: Impact scan terfokus Finishing Rawat Inap `PRD-RWI-FINISHING-001` (1 Oktober 2026)**, 33 kemampuan `FIN-CAP-01` s.d. `FIN-CAP-33`. Bagian 18 dinyatakan `STALE` untuk butir yang ditimpa bagian 19. Sebelumnya `1.5` — ditambah **bagian 18: Audit Kemampuan Integrasi Rawat Inap ↔ Billing (Pass A — Muhammad Hamzah, 17 September 2026)**. Sebelumnya `1.4` — ditambah bagian 16 dan bagian 17 (penyelarasan `PRD-RWI-V2-001` fase `RLN-PH-03` 15 September 2026). Bagian 1–14 baseline historis; bagian 15, 16, 17 historis untuk ruang kerja dokter dan keperawatan |
| Status | `source-audited / focused-admission-workspace-audit`. **Per 7 Oktober 2026, bagian 20 current untuk scope `PRD-RWI-ADMISI-001` (Workspace PPRI) terhadap `BE@671191e` (branch `MHamzah`) dan `FE@2788966` (branch `HamzahV2`).** Sebelumnya `source-audited / focused-finishing-audit`. **Per 1 Oktober 2026, bagian 19 current untuk scope `PRD-RWI-FINISHING-001` terhadap `BE@c8e99ce5` (branch `MHamzah`) dan `FE@22ad6733` (branch `HamzahV2`).** Sebelumnya: **per 17 September 2026, bagian 18 current untuk integrasi Rawat Inap ↔ Billing terhadap `BE@fe7e60d4` (branch `MHamzah`) dan `FE@2c007588` (branch `HamzahV2`).** Bagian 17 current untuk ruang kerja dokter dan keperawatan terhadap `BE@df3679c0` dan `FE@147355f5`; bagian 1–16 historis. Dokumen ini belum menyatakan sub-modul siap dibangun, siap dirilis, atau siap produksi |
| Tanggal audit | Audit Workspace PPRI 7 Oktober 2026; impact scan Finishing 1 Oktober 2026; audit integrasi Rawat Inap ↔ Billing 17 September 2026; impact scan penyelarasan V2 15 September 2026; impact scan V2 11 September 2026; baseline 21 Agustus 2026; impact scan Dokter Rawat Inap 2 September 2026 (`Asia/Jakarta`) |
| Masukan bisnis | [`00-interview-decisions.md`](./00-interview-decisions.md), **revision `35`** untuk bagian 20, bersama `PRD-RWI-ADMISI-001` v`0.2` (SHA-256 `f1fd336f…dc1192`); revision `27` untuk bagian 19 (sebelumnya revision `25` untuk bagian 18); `PRD-RWI-FINISHING-001` v`0.1`, SHA-256 `047a43aa…fddb4c29` |
| Daftar periksa audit | `RWI-TRC-001` sampai `RWI-TRC-009` pada dokumen keputusan |
| Decision ID yang dirujuk | `RWI-DEC-001` s.d. `RWI-DEC-035`, `RWI-DEC-075` s.d. `RWI-DEC-079`, `RWI-DEC-093` s.d. `RWI-DEC-096`, `RWI-DEC-106` s.d. `RWI-DEC-149`, serta **`RWI-DEC-156` s.d. `RWI-DEC-161`**, dan **`RWI-DEC-163` s.d. `RWI-DEC-183`** untuk bagian 19, serta **`RWI-DEC-225` s.d. `RWI-DEC-248`** dan `RWI-OQ-116` s.d. `RWI-OQ-121` untuk bagian 20 |
| Backend snapshot | **Bagian 20: `671191eb1aff3f618456cb6c909863bb0eca82f4`** (branch `MHamzah`; enam berkas source belum di-commit, bukti dibaca dari `HEAD`); bagian 19: `c8e99ce54a677f512d2a7ae80432545e1b650248` (branch `MHamzah`); bagian 18: `fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344` (branch `MHamzah`); bagian 17: `df3679c0d5b2f08106702153eb242d3a6cb2929b` (branch `MHamzah`); bagian 16: `201de753`; baseline `5afb54bd75281648010e50ef14f43ca1f80d8efd`; impact scan slice dokter `93b3227c431401d8f586dec4e1fb25fbf41766e3` (branch `MHamzah`) |
| Frontend snapshot | **Bagian 20: `27889662ab1e6149bd7cd291b480796905e52594`** (branch `HamzahV2`; audit dimulai pada `1f889d67c`, lalu commit `27889662a` masuk selama audit dan seluruh bukti diperiksa ulang padanya); bagian 19: `22ad6733096b9eb257fd9ae2af17df5ad25d8967` (branch `HamzahV2`); bagian 18: `2c00758832f834cff0288bef4f0d2fcf1161fb52` (branch `HamzahV2`); bagian 17: `147355f505e875148b8416866ada6cf8b2f1ad99` (branch `HamzahV2`), ditambah pembanding V1 `13c3a96b`; bagian 16: `7f6b9356`; baseline `dec4fdeff07c3c96ad9f07f41f184c54cf771371`; impact scan slice dokter `863f24b0d1617069310c04e5770b47fd1b518b5b` (branch `HamzahV2`) |
| Contract version | **Bagian 20:** belum ada kontrak target Workspace PPRI; amandemen kontrak `episode-rawat-inap` (usulan `0.11.0`, `RWI-DEC-227`) belum ditulis. Sebelumnya: Target sub-modul `dokter-rawat-inap`: API, integration, state, validation, permission, dan acceptance test `0.1.0`, seluruhnya `draft`. Kontrak as-is aktual dicatat pada bagian 15 |
| Cara audit | Pembacaan statis: model/entity, konfigurasi Entity Framework, migration, `DbSet`, route controller, atribut hak akses, registrasi *dependency injection* (DI), seeder, service/state frontend, menu, dan inventaris test |
| Batas tulis | Hanya dokumen ini. Tidak ada satu baris source aplikasi yang diubah, tidak ada build, tidak ada migration, tidak ada eksekusi database |

> **Cara membaca dokumen ini.** Dokumen ini menjawab satu pertanyaan saja: **apa yang sudah ada di
> dalam sistem hari ini, dan sejauh mana hal itu dapat dipakai ulang untuk Rawat Inap.** Dokumen
> ini tidak merancang tabel baru, tidak menetapkan API baru, dan tidak memutuskan aturan bisnis.
> Semua pertanyaan yang muncul dikumpulkan pada bagian 12 untuk dibawa ke `/grill-me`.

**Singkatan bukti.** Supaya tabel tidak terlalu panjang, dua penanda berikut dipakai di seluruh dokumen:

- `BE@5afb54b` berarti repository `NewQuilvianSystemBackend` pada commit `5afb54bd75281648010e50ef14f43ca1f80d8efd`.
- `FE@dec4fdef` berarti repository `QuilvianSystemFrontendDev` pada commit `dec4fdeff07c3c96ad9f07f41f184c54cf771371`.

Contoh cara membaca satu baris bukti:

> `BE@5afb54b Areas/HealthServices/MasterData/Models/MstBed.cs:41 IsReservable`

Artinya: pada repository backend, commit `5afb54b`, berkas `MstBed.cs`, baris 41, terdapat kolom
bernama `IsReservable`.

---

## 1. Batas audit

### 1.1 Yang diperiksa

Audit ini menelusuri sepuluh klaster kemampuan yang relevan dengan perjalanan pasien rawat inap:

| Klaster | Isi yang ditelusuri |
| --- | --- |
| Identity/Master Owner | Pasien, dokter, perawat, kelas pasien |
| Episode/Transaction Owner | Kunjungan (`TrxPatientEncounter`), episode rawat inap |
| Actor/Workforce | Dokter penanggung jawab, penugasan perawat |
| Location/Resource | Unit layanan, kamar, tempat tidur, boks bayi |
| Workflow/Status | Status episode, status tempat tidur, perpindahan, penutupan |
| Documentation/Record | Pengkajian, tanda vital, diagnosis, tindakan, CPPT, resume, persetujuan |
| Order/Result | Resep dan penyerahan obat |
| Financial | Kelayakan keuangan, kelas tagihan, tarif kamar |
| Authorization/Audit | Hak akses per peran, jejak perubahan |
| External Integration | Jalur masuk dari IGD, jalur ke Farmasi, jalur ke Billing |

Untuk masing-masing klaster diperiksa: entity dan relasinya, konfigurasi Entity Framework,
migration, `DbSet`, endpoint, hak akses, registrasi DI, seeder, konsumen frontend (route, menu,
service, Redux), serta test yang ada.

### 1.2 Yang tidak termasuk audit ini

- Merancang schema, API, atau alur layar target. Itu pekerjaan `/qv-design`.
- Menjalankan aplikasi, membuka database, atau memeriksa isi data produksi.
- Menetapkan pemilik baru, mengubah registry modul, atau menaikkan status `PLANNED` menjadi `ACTIVE`.
- Memperbaiki cacat yang ditemukan. Semua temuan hanya dicatat, tidak dikerjakan.

### 1.3 Istilah status yang dipakai

Hanya tujuh nilai berikut yang dipakai, tidak boleh ada nilai lain:

| Status | Arti sederhana |
| --- | --- |
| `Ready to reuse` | Sudah ada, sudah bekerja, dan dapat langsung dipakai apa adanya |
| `Reuse with adapter` | Sudah ada, tetapi perlu lapisan penyesuai karena bentuknya dibuat untuk keperluan lain |
| `Extend` | Sudah ada fondasinya, tetapi kolom, aturan, atau perilaku yang dibutuhkan Rawat Inap belum ada |
| `Repair` | Sudah ada, tetapi perilakunya salah atau menyesatkan sehingga tidak aman dipakai apa adanya |
| `Missing` | Tidak ditemukan sama sekali di dalam source |
| `Conflict` | Ada dua sumber yang saling bertentangan, misalnya frontend memanggil sesuatu yang tidak ada di backend |
| `Unknown` | Tidak dapat dijawab dari source; butuh akses lingkungan atau keputusan manusia |

---

## 2. Impact scan sejak dokumen keputusan ditulis

Dokumen keputusan `00-interview-decisions.md` mencatat backend SHA `45dcfa1`. Audit ini memakai
SHA yang lebih baru, yaitu `5afb54b`. Karena SHA berubah, impact scan wajib dilakukan lebih dulu.

**Hasil pemeriksaan rentang `45dcfa1` sampai `5afb54b`:**

| Yang berubah | Rincian |
| --- | --- |
| Dokumen blueprint | 30 berkas baru pada `docs/module-blueprints/` (billing-kasir, pharmacy, rawat-inap). Tidak memengaruhi source aplikasi |
| Tata letak berkas aturan | `.codex/rules/*.md` dipindahkan menjadi `.codex/*.md`. Isi tidak berubah |
| Billing | Penambahan `BillingManagementServiceCollectionExtensions.cs` (+16 baris) dan `BillingModuleService.cs` (+16 baris), keduanya masih berupa kerangka kosong |
| `Program.cs` | +3 baris, yaitu pendaftaran DI untuk kerangka Billing di atas |
| Test project | `QuilvianSystemBackend.Tests` ditambahkan dengan satu berkas test fondasi Billing (+51 baris) |
| `ApplicationDbContext.cs` | 32 baris berubah, berupa perapian, tanpa penambahan `DbSet` baru |

**Kesimpulan impact scan:** tidak ada satu pun entity, endpoint, migration, atau registrasi DI yang
berkaitan dengan tempat tidur, kunjungan, dokumentasi klinis, atau rawat inap yang berubah pada
rentang ini. Karena itu seluruh temuan pada dokumen keputusan yang menyangkut source masih berlaku,
dan audit ini dapat langsung memakai SHA terbaru.

Frontend SHA `dec4fdeff` sama persis dengan yang tercatat pada dokumen keputusan, sehingga tidak
ada rentang yang perlu dipindai di sisi frontend.

---

## 3. Kesimpulan eksekutif

### 3.1 Kalimat pendek

**Modul Rawat Inap belum ada sama sekali di dalam source.** Yang sudah ada adalah bahan-bahannya:
master tempat tidur yang lengkap, kunjungan pasien, dan berkas dokumentasi klinis. Ketiganya
dibangun untuk pasien rawat jalan, sehingga sebagian besar tidak dapat dipakai apa adanya untuk
pasien menginap.

Tidak ditemukan satu pun berkas dengan awalan `Inp`, tidak ada folder
`Areas/HealthServices/InPatientManagement/`, dan tidak ada satu pun dari 446 `DbSet` yang berkaitan
dengan episode rawat inap, penempatan tempat tidur, atau daftar pasien dirawat.

Bukti: `BE@5afb54b Areas/HealthServices/` memuat delapan folder — BillingManagement,
ClinicalManagement, EmergencyInstallationManagement, LaboratoryManagement, MasterData,
PatientManagement, PharmacyManagement, RegistrationManagement — dan tidak ada InPatientManagement.
`BE@5afb54b Repositories/ApplicationDbContext.cs` memuat 446 deklarasi `public DbSet`, tidak ada
yang memuat kata Inpatient, Admission, BedAssignment, maupun Census.

### 3.2 Tiga hambatan terbesar yang ditemukan

**Hambatan pertama — dokumentasi klinis terkunci pada antrean poliklinik.**

Untuk menulis pengkajian, catatan dokter, diagnosis, tindakan, dan resep, sistem sekarang
mewajibkan adanya **antrean** (`QueueId`) dan **konsultasi** (`ConsultationId`). Pasien rawat inap
tidak mengambil nomor antrean dan tidak duduk di depan poli. Akibatnya, tanpa penyesuaian, dokter
dan perawat tidak bisa menulis apa pun untuk pasien yang sedang menginap.

Contoh konkret: perawat ingin menulis pengkajian awal untuk Ny. Sari yang baru masuk ke bangsal
Melati kamar 3B. Endpoint `POST /api/v1/health-services/clinical-management/patient-assessments`
akan mencari baris antrean dengan `QueueId` yang dikirim dan mencocokkannya dengan kunjungan
Ny. Sari. Karena bangsal rawat inap tidak membuat antrean, `QueueId` tidak ada, dan permintaan
gagal.

Bukti: `BE@5afb54b Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs:21-24`
menandai `EncounterId` dan `QueueId` keduanya `[Required]`;
`BE@5afb54b Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs:265-267`
memanggil `FirstAsync` pada `TrxQueue` sehingga baris antrean **wajib benar-benar ada**;
`BE@5afb54b Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:499`
dan `:589` menunjukkan antrean hanya dibuat bila `IsQueueRequired` bernilai benar.

**Hambatan kedua — status tempat tidur hanyalah catatan master, bukan catatan penghunian.**

Kolom `MstBed.BedStatus` sudah punya nilai `Reserved` dan `Occupied`, tetapi tidak ada satu pun
tempat di dalam sistem yang mengubahnya secara otomatis ketika pasien ditempatkan. Satu-satunya
cara mengubahnya adalah lewat endpoint master data, dan endpoint itu tidak menanyakan pasien mana
yang menempati, sejak kapan, dan sampai kapan.

Contoh konkret: petugas admisi menempatkan Tn. Budi di bed `BD-RSMMC-00042`. Hari ini sistem tidak
punya tempat untuk menyimpan fakta "Tn. Budi menempati bed 42 sejak 21 Agustus 2026 pukul 10.00".
Yang bisa dilakukan hanyalah mengubah kolom status bed menjadi `Occupied` lewat menu master data,
tanpa jejak siapa yang menempatinya. Bila kemudian bed itu lupa dikembalikan ke `Available`, kamar
akan terlihat penuh selamanya padahal kosong.

Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/BedController.cs:514-548`, yaitu
`PATCH /{id}/availability` yang hanya menyalin `request.BedStatus` ke entity, tanpa pasien, tanpa
waktu mulai, tanpa penguncian, dan tanpa pemeriksaan tabrakan. Pencarian menyeluruh atas `MstBed`,
`BedStatus`, dan `BedId` di seluruh `Areas/`, `Services/`, dan `Repositories/` hanya menemukan
`BedController`, konfigurasi EF, satu pemeriksaan pemakaian pada `RoomController.cs:630`, dan kolom
lepas `FromBedId`/`ToBedId` milik IGD.

**Hambatan ketiga — kelayakan keuangan belum punya sumber apa pun.**

`RWI-RULE-009` menetapkan penutupan episode diblokir sampai status keuangan bernilai `Cleared`. Di
dalam source, modul `BillingManagement` hanya berisi dua tabel master dan satu kelas service yang
isinya kosong. Tidak ada faktur, tidak ada tagihan berjalan, dan tidak ada satu pun nilai `Pending`,
`Cleared`, atau `Blocked` yang bisa dibaca.

Bukti: `BE@5afb54b Areas/HealthServices/BillingManagement/` hanya memuat
`MasterData/Models/MstBillingItemCategory.cs`, `MasterData/Models/MstPaymentMethod.cs`, dua
controller master, dan `Billing/Services/BillingModuleService.cs:10-15` yang konstruktornya hanya
memeriksa bahwa `dbContext` tidak null lalu tidak menyimpannya sama sekali.

### 3.3 Kabar baik yang perlu dicatat

Tiga hal berikut jauh lebih siap daripada yang diperkirakan dokumen keputusan:

1. **Master tempat tidur sudah sangat lengkap.** Semua penanda yang dibutuhkan `RWI-RULE-012`
   (jenis kelamin, isolasi), `RWI-RULE-014` (boks bayi, intensif), dan `RWI-RULE-001` (dapat
   dipesan) sudah ada sebagai kolom, sudah bisa disaring, dan sudah punya ringkasan jumlah.
2. **Persetujuan pasien sudah siap pakai.** Jenis persetujuan `Admission`, `GeneralTreatment`, dan
   `ReleaseOfMedicalInformation` sudah ada, sudah menempel ke kunjungan, dan tidak mewajibkan
   antrean maupun konsultasi. Ini menjawab kebutuhan `RWI-RULE-025` hampir seluruhnya.
3. **CPPT sudah bebas dari antrean.** Berbeda dari pengkajian dan konsultasi, catatan CPPT boleh
   ditulis hanya dengan `PatientId`, sedangkan `EncounterId`, `QueueId`, dan `ConsultationId`
   semuanya boleh kosong. Karena `RWI-RULE-017` menetapkan visite dibaca dari catatan CPPT dokter,
   fondasinya sudah tersedia.

---

## 4. Jawaban langsung atas sembilan pertanyaan `RWI-TRC`

Bagian ini menjawab daftar periksa yang ditulis pemilik kebutuhan pada dokumen keputusan. Setiap
jawaban berupa fakta source, bukan pendapat.

### `RWI-TRC-001` — Apakah `MstBed` sudah punya `BedStatus`, `IsReservable`, penyaring, dan ringkasan?

**Jawaban: Ya, seluruhnya benar. Klaim EPIC RI-02 terbukti.**

| Yang diklaim | Terbukti | Bukti |
| --- | --- | --- |
| Kolom `BedStatus` | Ya | `BE@5afb54b Areas/HealthServices/MasterData/Models/MstBed.cs:27` |
| Nilai `Reserved` tersedia | Ya | `BE@5afb54b Areas/HealthServices/MasterData/Enums/BedStatus.cs:8` |
| Kolom `IsReservable` | Ya | `BE@5afb54b Areas/HealthServices/MasterData/Models/MstBed.cs:41` |
| Penyaring kamar | Ya | `BE@5afb54b Areas/HealthServices/MasterData/Controllers/BedController.cs:143` parameter `roomId` |
| Penyaring unit layanan | Ya | `BE@5afb54b .../BedController.cs:144` parameter `serviceUnitId` |
| Penyaring kelas pasien | Ya | `BE@5afb54b .../BedController.cs:145` parameter `patientClassId` |
| Ringkasan Available/Occupied | Ya | `BE@5afb54b .../BedController.cs:104-133` |

Nilai lengkap `BedStatus` adalah `Unknown`, `Available`, `Occupied`, `Reserved`, `Cleaning`,
`Maintenance`, `Blocked`, dan `Inactive`.

**Catatan penting yang tidak ditanyakan tetapi wajib diketahui:** kolom-kolom itu ada, tetapi tidak
ada mesin yang menggerakkannya. Lihat hambatan kedua pada bagian 3.2.

### `RWI-TRC-002` — Apakah `PatientEncounterController` memaksa kelas pasien `"RAWAT JALAN"`?

**Jawaban: Benar, tetapi hanya untuk kunjungan bertipe rawat jalan. Untuk tipe lain, kelas pasien
yang dikirim pemanggil dipakai apa adanya.**

Bukti: `BE@5afb54b Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:1417`
berbunyi `if (request.EncounterType == EncounterType.Outpatient)`. Baru di dalam blok itulah sistem
memaksa mencari master kelas bernama `"RAWAT JALAN"`, yang nilainya didefinisikan pada `.cs:55`
sebagai `DefaultOutpatientPatientClassName = "RAWAT JALAN"`. Untuk kunjungan selain rawat jalan,
jalur yang dipakai adalah `.cs:1483` dan seterusnya, yaitu memakai `request.PatientClassId` apa
adanya, dan nilainya boleh kosong.

**Akibat bagi `RWI-DEC-011`:** pemaksaan `"RAWAT JALAN"` **bukan** penghalang. Kunjungan bertipe
rawat inap dapat dibuat dengan kelas pasien pilihan petugas.

Contoh konkret: petugas admisi mendaftarkan Tn. Budi untuk rawat inap kelas 2. Bila `EncounterType`
diisi `Inpatient` (nilai 3) dan `PatientClassId` diisi identitas kelas 2, sistem menerimanya tanpa
mengubahnya menjadi `"RAWAT JALAN"`.

### `RWI-TRC-003` — Bentuk nyata `TrxPatientEncounter`: status, relasi lokasi, riwayat lokasi

**Jawaban: Kunjungan sudah mengenal tipe rawat inap, punya satu kolom kamar, tetapi statusnya
seluruhnya bercorak rawat jalan dan riwayat lokasi tidak ada.**

| Hal | Temuan | Bukti |
| --- | --- | --- |
| Tipe rawat inap | Ada, `EncounterType.Inpatient = 3` | `BE@5afb54b Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs:8` |
| Kolom lokasi | Ada satu, `RoomId` yang boleh kosong. Tidak ada `BedId` | `BE@5afb54b .../Models/TrxPatientEncounter.cs:38` |
| Riwayat lokasi | **Tidak ada.** `RoomId` hanya menyimpan nilai terakhir | Pencarian entity riwayat lokasi pada `Areas/` tidak menemukan apa pun |
| Status | 12 nilai, seluruhnya alur poliklinik | `BE@5afb54b .../Enums/EncounterStatus.cs` |

Daftar lengkap `EncounterStatus`: `Draft`, `Registered`, `Queued`, `WaitingForNurse`,
`InNurseScreening`, `WaitingForDoctor`, `InConsultation`, `ConsultationCompleted`, `Billing`,
`Completed`, `Cancelled`, `NoShow`.

Tidak ada `Admitted`, tidak ada `DischargePending`, dan tidak ada `Closed` dalam pengertian
`RWI-RULE-003`. Nilai `Completed` dipakai untuk konsultasi poliklinik yang selesai, bukan untuk
episode menginap yang ditutup.

**Temuan tambahan yang penting.** Perubahan status kunjungan tidak dijaga aturan perpindahan apa
pun. Endpoint `PATCH /patient-encounters/{id}/status` hanya memeriksa dua hal: nilainya terdaftar
di dalam enum, dan kunjungan belum batal atau selesai. Setelah itu nilai baru langsung ditimpa.
Tidak ada pemeriksaan "dari status apa boleh ke status apa", dan alasan perubahan menimpa kolom
`Notes` yang sama sehingga alasan sebelumnya hilang.

Bukti: `BE@5afb54b .../Controllers/PatientEncounterController.cs:864-894`.

Contoh konkret: pengguna dengan hak `PatientEncounter : Update` dapat mengubah status kunjungan
langsung dari `Registered` menjadi `Billing` tanpa pernah melewati konsultasi, dan sistem
menerimanya.

### `RWI-TRC-004` — Apakah dokumen klinis sudah terhubung ke `EncounterId`?

**Jawaban: Semua sudah terhubung ke kunjungan. Tetapi lima di antaranya juga mewajibkan antrean
atau konsultasi, dan itulah yang menjadi penghalang.**

| Entity | `EncounterId` | Ketergantungan lain yang wajib | Bukti |
| --- | --- | --- | --- |
| `TrxPatientAssessment` | Wajib | **`QueueId` wajib** | `.../Models/TrxPatientAssessment.cs:21-24` |
| `TrxDoctorConsultation` | Wajib | **`QueueId` wajib** | `.../Models/TrxDoctorConsultation.cs:22-25` |
| `TrxPatientDiagnosis` | Wajib | **`ConsultationId` wajib** | `.../Models/TrxPatientDiagnosis.cs:18-21` |
| `TrxPatientProcedure` | Wajib | **`ConsultationId` wajib** | `.../Models/TrxPatientProcedure.cs:18-21` |
| `TrxPrescription` | Wajib | **`ConsultationId` wajib** | `.../Models/TrxPrescription.cs:25-28` |
| `TrxPatientVitalSign` | Boleh kosong | Tidak ada | `.../Models/TrxPatientVitalSign.cs:28-34` |
| `TrxPatientIntegratedProgressNote` (CPPT) | Boleh kosong | Tidak ada | `.../Models/TrxPatientIntegratedProgressNote.cs:28-34` |
| `TrxPatientConsent` | Boleh kosong | Tidak ada | `.../Models/TrxPatientConsent.cs:28-36` |
| `TrxMedicalCertificate` | Boleh kosong | Tidak ada | `.../Models/TrxMedicalCertificate.cs:28-38` |
| `TrxPatientClinicalDocument` | Boleh kosong | Tidak ada | `.../Models/TrxPatientClinicalDocument.cs:28-38` |
| `TrxClinicalNoteAttachment` | Boleh kosong | Tidak ada | `.../Models/TrxClinicalNoteAttachment.cs:28-38` |
| `TrxPatientAllergy` | Boleh kosong | Tidak ada | `.../Models/TrxPatientAllergy.cs:28-32` |

Ketergantungan itu bukan sekadar kolom, tetapi ditegakkan saat penyimpanan:

- `BE@5afb54b .../Controllers/PatientAssessmentController.cs:265-267` mencari baris `TrxQueue` yang
  cocok dengan `request.QueueId` **dan** `request.EncounterId`, memakai `FirstAsync` yang melempar
  kesalahan bila tidak ketemu.
- `BE@5afb54b .../Controllers/DoctorConsultationController.cs:206` menolak permintaan dengan pesan
  `"QueueId wajib diisi."`, lalu `.cs:255-258` mencari baris antrean yang harus benar-benar ada.
- `BE@5afb54b Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs:278-281`
  mencari `TrxDoctorConsultation` yang cocok dengan `request.ConsultationId` **dan**
  `request.EncounterId`, lalu seluruh isi resep — pasien, dokter, unit layanan, klinik — disalin
  dari baris konsultasi itu, bukan dari isian pemanggil (`.cs:292-306`).

**Temuan tambahan yang jauh lebih menghambat daripada keharusan antrean itu sendiri.** Selain
mewajibkan antrean, modul Klinis dan Farmasi juga mengunci **jumlahnya menjadi satu**:

| Aturan yang ditegakkan | Bunyi penolakan | Bukti |
| --- | --- | --- |
| Satu kunjungan hanya boleh punya **satu konsultasi dokter**, selamanya | "Konsultasi dokter untuk encounter ini sudah ada." | `BE@5afb54b .../Controllers/DoctorConsultationController.cs:809-815` |
| Satu konsultasi hanya boleh punya **satu resep aktif** | "Konsultasi ini sudah memiliki resep aktif." | `BE@5afb54b .../PharmacyManagement/Controllers/PrescriptionController.cs:578-581` |
| Konsultasi yang sudah `Completed` **tidak boleh ditambah resep** | "Konsultasi yang sudah completed tidak dapat ditambahkan resep." | `BE@5afb54b .../PrescriptionController.cs:575` |
| Antrean harus berstatus `WaitingForDoctor`, `CalledByDoctor`, atau `InConsultation` saat konsultasi dibuat | "Status antrean tidak valid untuk konsultasi dokter." | `BE@5afb54b .../DoctorConsultationController.cs:803-808` |
| Antrean harus bertanda `IsScreeningRequired` saat pengkajian dibuat | "Antrean ini tidak membutuhkan screening." | `BE@5afb54b .../PatientAssessmentController.cs:645-646` |

Contoh konkret akibatnya: Tn. Budi dirawat lima hari dan diperiksa dokter setiap hari. Dengan
aturan yang berlaku hari ini, seluruh lima hari itu hanya boleh punya **satu** baris konsultasi
dan **satu** resep aktif. Begitu resep hari pertama diserahkan dan konsultasi ditandai selesai,
dokter tidak dapat lagi membuat resep hari kedua pada kunjungan yang sama.

Akibat penting bagi pilihan desain: gagasan "membuat antrean semu untuk pasien rawat inap" tidak
menyelesaikan masalah. Antrean semu memang membuat konsultasi pertama bisa dibuat, tetapi
konsultasi kedua tetap ditolak karena penjaganya memeriksa `EncounterId`, bukan `QueueId`.

**Bagian yang menggembirakan untuk `RWI-DEC-025` (visite dibaca dari CPPT).** CPPT sudah menyimpan
`ProfessionType`, `ProviderUserId`, `DoctorId`, `NoteDateTime`, `SourceModule`, dan
`ProviderDisplayNameSnapshot`. Semua bahan untuk menghitung "satu visite per dokter per tanggal"
sudah berada di satu tabel.

Bukti: `BE@5afb54b .../Models/TrxPatientIntegratedProgressNote.cs:38`, `:48`, `:52`, `:59`, `:62`, `:78`.

**Bagian yang belum ada untuk `RWI-RULE-021` (verifikasi CPPT oleh DPJP).** Tidak ada satu pun
kolom verifikasi pada CPPT, dan tidak ada endpoint verifikasi. Pencarian kata `Verif` pada model
dan controller CPPT tidak menghasilkan apa pun.

### `RWI-TRC-005` — Apakah `BillingManagement` baru punya `MasterData` saja?

**Jawaban: Ya, benar, dan bahkan lebih sedikit dari yang diperkirakan.**

Isi seluruh modul Billing hari ini:

| Berkas | Isi |
| --- | --- |
| `MasterData/Models/MstBillingItemCategory.cs` | Master kategori item tagihan |
| `MasterData/Models/MstPaymentMethod.cs` | Master metode pembayaran |
| `MasterData/Controllers/BillingItemCategoryController.cs` | CRUD kategori |
| `MasterData/Controllers/PaymentMethodController.cs` | CRUD metode pembayaran |
| `Billing/Services/BillingModuleService.cs` | Kelas kosong. Konstruktornya menerima `ApplicationDbContext`, memeriksa nilainya tidak null, lalu **tidak menyimpannya**. Tidak ada satu pun method bisnis |

Bukti: `BE@5afb54b Areas/HealthServices/BillingManagement/Billing/Services/BillingModuleService.cs:10-15`.

**Akibat bagi `RWI-RULE-009`:** gerbang keuangan tidak punya sumber data. Nilai `Pending`,
`Cleared`, dan `Blocked` tidak ada di mana pun. Bila modul Rawat Inap dibangun hari ini, gerbang
itu akan selalu menahan penutupan episode, sehingga setiap penutupan harus lewat jalan keluar
supervisor. Jalan keluar itu justru dirancang sebagai pengecualian, bukan jalur utama.

### `RWI-TRC-006` — Apakah master bed, room, service unit, dan kelas pasien sudah terisi data?

**Jawaban: Tidak dapat dipastikan dari source. Statusnya `Unknown`. Yang pasti: tidak ada seeder
yang mengisinya secara otomatis.**

Seluruh seeder yang terdaftar di dalam repository:

| Seeder | Yang diisi |
| --- | --- |
| `Seeders/AccessMenuSeeder.cs` | Modul, controller, dan action hak akses |
| `Seeders/AppVersionSeeder.cs` | Versi aplikasi |
| `Seeders/DefaultWorkScheduleSeeder.cs` | Jadwal kerja bawaan |
| `Seeders/Icd10DiagnosisSeeder.cs` | Diagnosis ICD-10 |
| `Seeders/SuperAdminSeeder.cs` | Pengguna super admin |
| `Areas/HealthServices/MasterData/Seeders/EmergencyMasterDataSeeder.cs` | Master IGD: triase, jenis kasus, cara datang, jenis disposisi, pengaturan IGD |

**Tidak ada satu pun seeder untuk tempat tidur, kamar, unit layanan, maupun kelas pasien.** Bahkan
seeder IGD pun tidak membuat kamar dan tempat tidur; ia justru **membaca** unit layanan IGD yang
harus sudah ada lebih dulu (`EmergencyMasterDataSeeder.cs:349`).

Tabelnya sendiri sudah ada sejak lama:
`BE@5afb54b Migrations/20260526045352_initializeMstBed.cs` bertanggal 26 Mei 2026, dan `MstBed`
masih tercatat pada `Migrations/ApplicationDbContextModelSnapshot.cs`.

**Kesimpulan:** tabel siap, isi tidak dijamin. Apakah data bed dan kamar sudah dimasukkan admin
hanya dapat dipastikan dengan membuka database, dan itu di luar batas audit read-only ini.

**Tentang boks bayi (`RWI-DEC-020`):** kolomnya sudah tersedia, yaitu `MstBed.IsForNewborn`
(`.../MstBed.cs:33`), `MstRoom.IsForNewborn` (`.../MstRoom.cs:43`), `RoomType.BabyRoom`
(`.../Enums/RoomType.cs`), dan `MstPatientClass.IsForNewborn` (`.../MstPatientClass.cs:39`). Jadi
mendaftarkan boks bayi sebagai tempat tidur tersendiri di kamar ibu **tidak** memerlukan tabel baru.

### `RWI-TRC-007` — Pola permission yang dipakai repository

**Jawaban: Pola berbasis peran dengan butir hak akses per pasangan controller dan action, dan
daftarnya dibuat otomatis dari atribut di dalam kode.**

Cara kerjanya, urut:

1. Setiap controller diberi atribut `[AccessController(...)]` yang menyebut kode modul, nama modul,
   dan nama controller. Contoh nyata: `BE@5afb54b .../Controllers/BedController.cs:25-33` memakai
   `moduleCode: "HEALTH_SERVICE_MASTER_DATA"` dan `ControllerName = "Bed"`.
2. Setiap endpoint diberi dua atribut. `[AccessAction("Read", "Read Bed", ...)]` mendaftarkan nama
   tindakan, dan `[AccessPermission("Bed", "Read")]` memasang penjaga saat permintaan masuk.
   Contoh: `BE@5afb54b .../Controllers/BedController.cs:55`.
3. Saat aplikasi dinyalakan, `Seeders/AccessMenuSeeder.cs:22-60` menyisir seluruh endpoint, membaca
   kedua atribut itu, lalu membuat baris modul, controller, dan action di database bila belum ada.
4. Saat permintaan masuk, `Filters/AccessPermissionFilter.cs:28-77` memeriksa apakah pengguna sudah
   login, lalu bertanya ke `Services/Security/AccessPermissionService.cs:26` apakah peran pengguna
   itu punya akses ke pasangan controller dan action tersebut.
5. Bila tidak punya, permintaan ditolak dengan kode 403 dan pesan berbahasa Indonesia
   "Anda tidak memiliki akses ke menu atau fitur ini."

**Akibat bagi Rawat Inap:** NFR-004 dapat dipenuhi tanpa membangun mesin hak akses baru. Cukup
memberi atribut yang sama pada controller `Inp` yang akan dibuat, dan butir haknya muncul sendiri.

**Yang tidak tersedia dan ini penting untuk `RWI-RULE-016`.** Pola ini hanya mengenal "peran ini
boleh melakukan tindakan ini", dan sama sekali tidak mengenal "orang ini boleh melakukan tindakan
ini **terhadap pasien ini**". Aturan `RWI-DEC-023` dan `RWI-DEC-024` menuntut sistem menolak dokter
yang bukan DPJP episode tersebut. Penjaga semacam itu harus ditulis sendiri di dalam service modul,
karena mesin hak akses yang ada tidak dapat melakukannya.

### `RWI-TRC-008` — Apa yang dihasilkan IGD saat disposisi "rawat inap"?

**Jawaban: Hanya satu baris keputusan berisi jenis disposisi dan unit tujuan. Tidak ada admisi,
tidak ada tempat tidur, dan tidak ada apa pun yang diteruskan ke rawat inap.**

Jenis disposisi `RANAP` memang sudah ada dan sudah diisi otomatis oleh seeder:

> `new DispositionTypeDefinition("RANAP", "Rawat inap", true, false, 20)`

Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Seeders/EmergencyMasterDataSeeder.cs:284`. Nilai
`true` pertama berarti `RequiresDestinationServiceUnit`, sehingga petugas IGD **wajib** memilih
unit layanan tujuan ketika memilih disposisi rawat inap.

Yang tersimpan saat disposisi dibuat:

| Kolom | Isi | Bukti |
| --- | --- | --- |
| `EmergencyVisitId` | Kunjungan IGD asal | `.../Models/TrxEmergencyDisposition.cs:17` |
| `DispositionTypeId` | Menunjuk baris `RANAP` | `.../Models/TrxEmergencyDisposition.cs:20` |
| `DestinationServiceUnitId` | Unit layanan tujuan, misalnya bangsal Melati | `.../Models/TrxEmergencyDisposition.cs:35` |
| `DecidedByDoctorId`, `DecidedAt` | Dokter IGD dan waktu keputusan | `.../Models/TrxEmergencyDisposition.cs:25-27` |
| `ConfirmedByUserId`, `ExecutedAt` | Konfirmasi dan waktu pelaksanaan | `.../Models/TrxEmergencyDisposition.cs:29-33` |

Yang **tidak** tersimpan dan tidak dikerjakan: kamar tujuan, tempat tidur tujuan, permintaan tempat
tidur, kelas perawatan, DPJP rawat inap, dan pembuatan episode.

**Satu penanda kontrak yang tidak pernah dijalankan.** Master jenis disposisi punya kolom
`ClosesEmergencyVisit`, dan seeder mengisinya `true` untuk semua jenis termasuk `RANAP`
(`EmergencyMasterDataSeeder.cs:310`). Namun pencarian menyeluruh menunjukkan kolom itu hanya dibaca
dan ditulis oleh CRUD master dan konfigurasi EF, dan **tidak dipakai satu pun alur kerja** untuk
benar-benar menutup kunjungan IGD.

Bukti: pencarian `ClosesEmergencyVisit` di seluruh source hanya menemukan
`.../MasterData/Controllers/EmergencyDispositionTypeController.cs:164`, `:221`, `:299`,
`.../MasterData/DTOs/EmergencyDispositionTypeDtos.cs:12`, `:34`,
`.../MasterData/Models/MstEmergencyDispositionType.cs:25`,
`.../MasterData/Seeders/EmergencyMasterDataSeeder.cs:310`, dan
`Repositories/Configurations/HealthServices/MasterData/EmergencyInstallationManagement/MstEmergencyDispositionTypeConfiguration.cs:31`.

**Akibat bagi `RWI-RULE-005`.** Apakah kunjungan IGD tetap terbuka atau ditutup saat pasien naik ke
bangsal, hari ini ditentukan oleh perilaku yang belum ditulis. Ini menjadi pertanyaan penutup.

**Ada satu pola yang sangat berguna di IGD.** `TrxEmergencyTransfer` sudah merekam perpindahan
lengkap dengan asal dan tujuan pada tiga tingkat — unit layanan, kamar, dan tempat tidur — ditambah
status, peminta, penerima, alasan, dan ringkasan serah terima.

Bukti: `BE@5afb54b Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTransfer.cs:15-63`.

Namun tabel itu menempel pada `EmergencyVisitId`, sehingga tidak dapat dipakai langsung untuk
episode rawat inap. Nilainya adalah sebagai **contoh bentuk yang sudah disetujui repository**.

Perlu dicatat: kolom `FromBedId` dan `ToBedId` pada tabel itu hanya diberi indeks, **tanpa relasi ke
`MstBed`**, dan perpindahan IGD tidak pernah mengubah status tempat tidur mana pun.

Bukti: `BE@5afb54b Repositories/Configurations/HealthServices/EmergencyInstallationManagement/TrxEmergencyTransferConfiguration.cs:31-32`
hanya memakai `HasIndex`, dibandingkan `.cs:34-58` yang memakai `HasOne` untuk relasi yang
benar-benar ada.

### `RWI-TRC-009` — Apakah sudah ada mekanisme audit perubahan status yang bisa dipakai ulang?

**Jawaban: Ada tiga lapis, dan tidak satu pun cukup untuk NFR-003 apa adanya.**

**Lapis pertama — kolom jejak pada setiap tabel.** Semua entity mewarisi `IdentityModel` yang
menyimpan `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`,
`CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`.

Bukti: `BE@5afb54b Models/IdentityModel.cs:5-23`.

Keterbatasannya: yang tersimpan hanya perubahan **terakhir**. Bila status episode berubah lima
kali, hanya perubahan kelima yang terlihat; empat sebelumnya hilang.

**Lapis kedua — catatan aktivitas.** `Services/Logging/LoggerService.cs` menyediakan `InfoAsync`,
`WarningAsync`, `ErrorAsync`, dan `AuditAsync`. Isinya lengkap: siapa penggunanya, alamat IP,
perangkat, jalur permintaan, dan pesan.

Keterbatasannya: keluarannya berupa berkas log yang dibaca lewat Grafana Loki, bukan tabel
database. Catatan ini tidak dapat ditampilkan di layar sebagai riwayat pasien, tidak dapat disaring
per episode, dan tidak terikat transaksi database.

Bukti: `BE@5afb54b Services/Logging/LoggerService.cs:36` untuk `AuditAsync`, dan `:88-108` untuk
penyusunan baris teks log beserta komentar yang menyebut Grafana Loki.

**Lapis ketiga — tabel riwayat status yang sudah terbukti.** Modul Workflow milik HR punya
`TrxWorkflowStatusHistory` dengan kolom `FromWorkflowStatus`, `ToWorkflowStatus`, `ActionType`,
`ChangedByUserId`, `ChangedAt`, `SequenceNumber`, `Comment`, `IsSystemGenerated`, dan
`StatusSnapshotJson`.

Bukti: `BE@5afb54b Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowStatusHistory.cs:12-41`.

Ini adalah bentuk yang tepat untuk kebutuhan Rawat Inap. Keterbatasannya: tabel itu menempel pada
`WorkflowInstanceId` milik HR, jadi yang dapat dipakai ulang adalah **polanya**, bukan tabelnya.

---

## 5. Capability evidence map

Tabel berikut adalah inti dokumen ini. Kolom Kebutuhan merujuk kode kemampuan PRD (`CAP-xxx`) dan
aturan bisnis (`RWI-RULE-xxx`) pada dokumen keputusan.

| ID | Kebutuhan | Pemilik | Bukti as-is | Status | Gap atau adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `RWI-CAP-001` | Memilih pasien terdaftar (CAP-002) | PatientManagement | `BE@5afb54b Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:42 Gender`; halaman `FE@dec4fdef src/app/health-services/patient-management/master-data/patients/` | `Ready to reuse` | Tidak ada | Rendah |
| `RWI-CAP-002` | Penjamin dan cara bayar saat masuk (CAP-003) | RegistrationManagement | `BE@5afb54b Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:102 PaymentType`, `:206 PaymentSource`; `Models/TrxPatientEncounterGuarantor.cs` | `Reuse with adapter` | Snapshot penjamin dibuat saat kunjungan dibuat, bukan saat admisi. Perubahan penjamin di tengah rawat inap belum punya jalur | Sedang |
| `RWI-CAP-003` | Menentukan DPJP (CAP-004) | RegistrationManagement | `BE@5afb54b .../Models/TrxPatientEncounter.cs:40 DoctorId`, `:183 Doctor`; `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs` | `Reuse with adapter` | Hanya satu kolom dokter tanpa peran DPJP, tanpa masa berlaku, dan tanpa pengalihan tanggung jawab yang dituntut `RWI-RULE-016` | Tinggi |
| `RWI-CAP-004` | Mencari tempat tidur tersedia (CAP-005) | MasterData | `BE@5afb54b Areas/HealthServices/MasterData/Controllers/BedController.cs:135-220` daftar bertingkat dengan 13 penyaring; `:221-285` daftar pilihan; `:104-133` ringkasan | `Ready to reuse` | Tidak ada untuk sisi baca | Rendah |
| `RWI-CAP-005` | Atribut dan status tempat tidur | MasterData | `BE@5afb54b .../Models/MstBed.cs:27 BedStatus`, `:29-41` tujuh penanda peruntukan; `Enums/BedStatus.cs:3-13` | `Ready to reuse` | Tidak ada sebagai master | Rendah |
| `RWI-CAP-006` | Pemesanan tempat tidur 2 jam dan gugur otomatis (`RWI-RULE-001`, `RWI-RULE-002`) | Belum ada | Tidak ditemukan entity, `DbSet`, service, atau endpoint pemesanan | `Missing` | Perlu catatan pemesanan berisi calon pasien, waktu mulai, batas waktu, dan perhitungan kedaluwarsa saat dibaca | Tinggi |
| `RWI-CAP-007` | Penempatan pasien pada tempat tidur (CAP-006) | Belum ada | Tidak ditemukan entity penempatan. `MstBed.BedStatus` hanya dapat diubah lewat `BedController.cs:514-548` tanpa pasien dan tanpa waktu | `Missing` | Perlu catatan penempatan satu pasien pada satu tempat tidur beserta waktu mulai dan berakhir | Tinggi |
| `RWI-CAP-008` | Episode rawat inap dengan status `Draft`, `Admitted`, `DischargePending`, `Closed`, `Cancelled` (`RWI-RULE-003`) | Belum ada | Tidak ada entity episode. `EncounterStatus` hanya memuat 12 status poliklinik | `Missing` | Seluruh model status episode belum ada | Tinggi |
| `RWI-CAP-009` | Kunjungan sebagai jangkar episode (`RWI-RULE-005`) | RegistrationManagement | `BE@5afb54b .../Models/TrxPatientEncounter.cs:15-19`, `:83 EncounterType`; `Enums/EncounterType.cs:8 Inpatient = 3` | `Reuse with adapter` | Perlu aturan satu episode menempel tepat satu kunjungan, dan penentuan nasib kunjungan IGD saat pasien naik ke bangsal | Sedang |
| `RWI-CAP-010` | Kunjungan rawat inap dibuat otomatis untuk pasien datang langsung (`RWI-DEC-011`) | RegistrationManagement | `BE@5afb54b .../Controllers/PatientEncounterController.cs:389-398` endpoint admin; `:1417` pemaksaan kelas hanya untuk rawat jalan; `:499` dan `:589` pembuatan antrean bergantung `IsQueueRequired` | `Extend` | Jalur pembuatan masih bercorak poliklinik: nomor antrean, jadwal dokter, klinik. Perlu jalur khusus tanpa antrean | Sedang |
| `RWI-CAP-011` | Kelas perawatan dan kelas tagihan (`RWI-RULE-007`) | MasterData | `BE@5afb54b .../Models/MstPatientClass.cs:29 ClassLevel`, `:33 IsForInpatient`, `:37 IsForIntensiveCare`, `:39 IsForNewborn`, `:41 IsForRoomCharge`, `:47 DefaultDailyRoomRate` | `Ready to reuse` | Master siap. Riwayat perubahan kelas selama episode belum ada, lihat `RWI-CAP-017` | Rendah |
| `RWI-CAP-012` | Daftar pasien dirawat beserta lokasinya atau census (CAP-008) | Belum ada | Tidak ditemukan endpoint, view, atau query yang menggabungkan pasien, episode aktif, dan lokasi | `Missing` | Seluruh census belum ada | Tinggi |
| `RWI-CAP-013` | Perhitungan lama dirawat (`RWI-RULE-019`) | Belum ada | Tidak ditemukan perhitungan selisih tanggal untuk lama rawat | `Missing` | Perlu perhitungan selisih tanggal dengan hasil paling sedikit 1 hari | Sedang |
| `RWI-CAP-014` | Penugasan perawat penanggung jawab per pasien (CAP-011) | Belum ada | Yang ada hanya penugasan perawat ke klaster nurse station: `BE@5afb54b Areas/Administrator/MasterData/Models/MstNurseStationClusterStaff.cs:14-31`, dan itu untuk memanggil antrean poliklinik | `Missing` | Perlu penugasan perawat pada satu episode atau satu giliran jaga, bukan pada klaster | Sedang |
| `RWI-CAP-015` | Pengkajian awal perawat (CAP-012) | ClinicalManagement | `BE@5afb54b .../Models/TrxPatientAssessment.cs:21-24`; `Controllers/PatientAssessmentController.cs:245-407` pembuatan, `:265-267` keharusan antrean, `:168-209` pembacaan per kunjungan | `Reuse with adapter` | Isi formulirnya siap. Keharusan `QueueId` harus diatasi lebih dulu | Tinggi |
| `RWI-CAP-016` | Tanda vital dan catatan keperawatan dasar (CAP-014) | ClinicalManagement | `BE@5afb54b .../Models/TrxPatientVitalSign.cs:28-34` seluruh pengait boleh kosong | `Ready to reuse` | Tidak ada penghalang teknis | Rendah |
| `RWI-CAP-017` | Pindah kamar, pindah tempat tidur, dan pindah kelas (CAP-017, `RWI-RULE-006`, `RWI-RULE-007`) | Belum ada untuk rawat inap | Pola tersedia di IGD: `BE@5afb54b .../Models/TrxEmergencyTransfer.cs:15-63`, tetapi menempel pada `EmergencyVisitId` dan tidak mengubah status bed | `Missing` | Perlu perpindahan milik episode rawat inap yang bersifat satu tindakan utuh sesuai `RWI-RULE-008` | Tinggi |
| `RWI-CAP-018` | Dokumentasi dokter bentuk SOAP dan kajian dokter (CAP-020, CAP-022) | ClinicalManagement | `BE@5afb54b .../Models/TrxDoctorConsultation.cs:22-25`; `Controllers/DoctorConsultationController.cs:206`, `:255-258` | `Reuse with adapter` | Konsultasi mewajibkan antrean. Untuk rawat inap perlu jalur lain atau pelonggaran | Tinggi |
| `RWI-CAP-019` | CPPT (CAP-021) | ClinicalManagement | `BE@5afb54b .../Models/TrxPatientIntegratedProgressNote.cs:28-42` seluruh pengait boleh kosong; `Controllers/PatientIntegratedProgressNoteController.cs:260` pembuatan bebas, `:165-230` timeline | `Ready to reuse` | Tidak ada penghalang untuk menulis dan membaca | Rendah |
| `RWI-CAP-020` | Verifikasi CPPT oleh DPJP (`RWI-RULE-021`) | Belum ada | Tidak ada kolom maupun endpoint verifikasi pada CPPT | `Missing` | Perlu penanda verifikasi beserta pelaku dan waktunya | Sedang |
| `RWI-CAP-021` | Resep pasien rawat inap (CAP-023) | PharmacyManagement | `BE@5afb54b .../Models/TrxPrescription.cs:25-28`; `Controllers/PrescriptionController.cs:262-340`, khususnya `:278-281` dan `:292-306` | `Reuse with adapter` | Resep mewarisi seluruh konteks dari konsultasi. Tanpa konsultasi, resep tidak dapat dibuat | Tinggi |
| `RWI-CAP-022` | Obat pulang sebagai jenis resep (`RWI-RULE-024`) | PharmacyManagement | Tidak ada kolom jenis resep. Enum yang tersedia hanya `PrescriptionStatus`, `PrescriptionPaymentStatus`, dan `PrescriptionFulfillmentStatus` pada `BE@5afb54b Areas/HealthServices/PharmacyManagement/Enums/` | `Extend` | Perlu penanda jenis resep atau penanda obat pulang, dan status penyerahannya dibaca balik | Sedang |
| `RWI-CAP-023` | Tindakan dokter (CAP-024) | ClinicalManagement | `BE@5afb54b .../Models/TrxPatientProcedure.cs:18-21` mewajibkan `ConsultationId`; `Controllers/PatientProcedureController.cs` | `Reuse with adapter` | Sama seperti resep, terikat pada konsultasi | Tinggi |
| `RWI-CAP-024` | Pencatatan visite dokter (CAP-025, `RWI-RULE-017`, `RWI-DEC-031`) | ClinicalManagement | Bahan lengkap ada di CPPT: `.../TrxPatientIntegratedProgressNote.cs:38 DoctorId`, `:48 NoteDateTime`, `:52 ProfessionType`, `:59 ProviderUserId` | `Extend` | Perlu perhitungan satu visite per dokter per tanggal beserta laporannya. Datanya sudah ada, agregasinya belum | Rendah |
| `RWI-CAP-025` | Resume medis atau resume pulang (CAP-026) | ClinicalManagement | `BE@5afb54b .../Enums/MedicalCertificateType.cs:16 MedicalResumeLetter`, `:10 InpatientStatement`, `:11 DeathCertificate`; `.../Models/TrxMedicalCertificate.cs:156 AdmissionDate`, `:158 DischargeDate`, `:160 DeathDateTime`, `:163 CauseOfDeath`; `.../Enums/PatientClinicalDocumentType.cs:9 DischargeSummary` | `Reuse with adapter` | Bentuknya surat keterangan, bukan catatan resmi episode. Belum ada pengait ke episode rawat inap dan belum menjadi syarat penutupan | Sedang |
| `RWI-CAP-026` | Lima cara pulang (`RWI-RULE-011`) | Belum ada | Sebagian data ada pada surat keterangan dan pada disposisi IGD (`.../TrxEmergencyDisposition.cs:55 IsPatientDeceased`), tetapi tidak ada model cara pulang milik episode rawat inap | `Missing` | Perlu jenis cara pulang beserta syarat penutupan yang berbeda-beda | Tinggi |
| `RWI-CAP-027` | Gerbang kelayakan keuangan (`RWI-RULE-009`) | BillingManagement | `BE@5afb54b Areas/HealthServices/BillingManagement/Billing/Services/BillingModuleService.cs:10-15` masih kosong; hanya dua master tersedia | `Missing` | Tidak ada sumber nilai `Pending`, `Cleared`, `Blocked`. Perlu keputusan bentuk sementara | Tinggi |
| `RWI-CAP-028` | Daftar periksa administrasi sebelum penutupan (`RWI-RULE-018`) | Belum ada | Tidak ditemukan master butir daftar periksa maupun catatan penandaannya | `Missing` | Perlu master butir yang dapat diatur admin, dan catatan penandaan per episode | Sedang |
| `RWI-CAP-029` | Penutupan episode dan pelepasan tempat tidur (CAP-028) | Belum ada | Tidak ada endpoint penutupan. Pelepasan bed hanya mungkin lewat CRUD master | `Missing` | Perlu satu tindakan utuh yang menutup episode dan mengosongkan tempat tidur bersamaan | Tinggi |
| `RWI-CAP-030` | Membuka kembali episode yang sudah ditutup (`RWI-RULE-020`) | Belum ada | Tidak ada mekanisme reopen di mana pun | `Missing` | Perlu jalur reopen khusus supervisor yang tidak mengembalikan tempat tidur | Rendah |
| `RWI-CAP-031` | Persetujuan umum rawat inap (`RWI-RULE-025`) | ClinicalManagement | `BE@5afb54b .../Enums/PatientConsentType.cs:6 GeneralTreatment`, `:14 Admission`, `:15 ReleaseOfMedicalInformation`; `.../Models/TrxPatientConsent.cs:28-36` pengait boleh kosong, `:137-161` data penanda tangan, `:167-197` penjelas dan saksi; `Controllers/PatientConsentController.cs:295`, `:595 sign`, `:641 verify` | `Ready to reuse` | Siap dipakai. Yang perlu ditambahkan hanya penunjukan penerima informasi bila tidak diwakili jenis `ReleaseOfMedicalInformation` | Rendah |
| `RWI-CAP-032` | Bayi baru lahir dan boks bayi (`RWI-RULE-014`) | MasterData | `BE@5afb54b .../MstBed.cs:33 IsForNewborn`; `.../MstRoom.cs:43 IsForNewborn`; `Enums/RoomType.cs BabyRoom`; `.../MstPatientClass.cs:39 IsForNewborn` | `Ready to reuse` untuk masternya | Episode bayi tersendiri bergantung pada `RWI-CAP-008` yang belum ada | Rendah |
| `RWI-CAP-033` | Penyaring jenis kelamin dan isolasi (`RWI-RULE-012`) | MasterData | `BE@5afb54b .../MstBed.cs:29-35`; `Controllers/BedController.cs:146-151` penyaring `isForMale`, `isForFemale`, `isForNewborn`, `isIsolationBed` | `Ready to reuse` sebagai penyaring | Sesuai `RWI-DEC-018` memang hanya penyaring. Belum ada aturan yang menolak penempatan | Tinggi, karena `RWI-DEC-018` masih `draft` dan menjadi gerbang keras sebelum produksi |
| `RWI-CAP-034` | Parameter yang dapat diubah admin: 2 jam, 24 jam, 1 hari, ambang daftar pantau | MasterData | Pola tersedia: `BE@5afb54b Areas/HealthServices/MasterData/Models/MstEmergencySetting.cs:16 Code`, `:34 ImmediateCareLevelThreshold`, `:36 RequireRegistrationBeforeTreatmentFromLevel` | `Reuse with adapter` | Polanya terbukti dipakai IGD. Rawat Inap perlu tabel pengaturannya sendiri dengan awalan `Inp` | Rendah |
| `RWI-CAP-035` | Hak akses per peran (NFR-004) | Platform | `BE@5afb54b Attributes/AccessPermissionAttribute.cs:9-17`; `Filters/AccessPermissionFilter.cs:28-77`; `Services/Security/AccessPermissionService.cs:26`; `Seeders/AccessMenuSeeder.cs:22-60` | `Ready to reuse` | Tidak ada | Rendah |
| `RWI-CAP-036` | Kewenangan per pasien, yaitu hanya DPJP episode itu (`RWI-RULE-016`, `RWI-DEC-023`, `RWI-DEC-024`) | Belum ada | Mesin hak akses hanya mengenal peran terhadap endpoint, tidak mengenal hubungan pengguna dengan satu pasien | `Missing` | Perlu penjaga tambahan di dalam service modul | Tinggi |
| `RWI-CAP-037` | Jejak audit perubahan status (NFR-003) | Platform | `BE@5afb54b Models/IdentityModel.cs:5-23` hanya nilai terakhir; `Services/Logging/LoggerService.cs:36` menulis ke berkas log; pola tabel riwayat pada `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowStatusHistory.cs:12-41` | `Reuse with adapter` | Untuk riwayat status episode yang dapat ditampilkan dan disaring, perlu tabel riwayat sendiri mengikuti pola HR | Sedang |
| `RWI-CAP-038` | Jalur masuk dari IGD (`RWI-TRC-008`) | EmergencyInstallationManagement | `BE@5afb54b .../Models/TrxEmergencyDisposition.cs:17-35`; `.../Seeders/EmergencyMasterDataSeeder.cs:284` jenis `RANAP`; `Controllers/EmergencyDispositionController.cs:151` dan `:283` | `Reuse with adapter` | Disposisi hanya menyimpan keputusan. Serah terima ke rawat inap belum ada, dan `ClosesEmergencyVisit` tidak pernah dijalankan | Tinggi |
| `RWI-CAP-039` | Tiga daftar pantau kepatuhan (`RWI-RULE-023`) | Belum ada | Tidak ditemukan laporan pantau kepatuhan apa pun | `Missing` | Bergantung pada `RWI-CAP-008`, `RWI-CAP-015`, dan `RWI-CAP-020` | Rendah |
| `RWI-CAP-040` | Episode `Draft` yang ditinggalkan menjadi batal (`RWI-RULE-022`) | Belum ada | Tidak ada mekanisme kedaluwarsa berbasis pembacaan di mana pun | `Missing` | Bergantung pada `RWI-CAP-008` | Rendah |
| `RWI-CAP-041` | Route dan menu Rawat Inap di frontend | Belum ada | `FE@dec4fdef src/app/health-services/` hanya memuat emergency-installation-management, master-data, patient-management, pharmacy-management, registration-management, dan select-demo. `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:894` hanya memuat menu "Rawat Jalan" | `Missing` | Seluruh layar Rawat Inap belum ada | Tinggi |
| `RWI-CAP-042` | Halaman master tempat tidur di frontend | Frontend master data | Halaman `FE@dec4fdef src/app/health-services/master-data/bed/bed-client.jsx`; menu `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:683-686`; state `FE@dec4fdef src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx` | `Conflict` | Tombol aktif dan nonaktif memanggil endpoint yang tidak ada di backend. Rincian pada bagian 9 | Sedang |
| `RWI-CAP-043` | Isi data master bed, kamar, unit layanan, dan kelas pasien (`RWI-TRC-006`) | MasterData | Tabel ada sejak `BE@5afb54b Migrations/20260526045352_initializeMstBed.cs`. Tidak ada seeder yang mengisinya | `Unknown` | Tidak dapat dipastikan tanpa membuka database | Tinggi, karena Definition of Done melarang manipulasi database manual |
| `RWI-CAP-044` | Bukti pengujian untuk kemampuan yang dipakai ulang | Platform | Backend hanya punya satu berkas test: `BE@5afb54b QuilvianSystemBackend.Tests/BillingManagement/BillingModuleFoundationTests.cs`. Frontend punya empat berkas: `FE@dec4fdef tests/unit/auth-security.test.mjs`, `tests/unit/base-components-regression.test.mjs`, `tests/e2e/auth-security.spec.mjs`, `tests/e2e/route-smoke.spec.mjs` | `Missing` | Tidak ada satu pun test yang menyentuh tempat tidur, kunjungan, atau dokumentasi klinis | Sedang |

**Rekapitulasi status:**

| Status | Jumlah | Nomor `RWI-CAP-` |
| --- | ---: | --- |
| `Ready to reuse` | 10 | 001, 004, 005, 011, 016, 019, 031, 032, 033, 035 |
| `Reuse with adapter` | 11 | 002, 003, 009, 015, 018, 021, 023, 025, 034, 037, 038 |
| `Extend` | 3 | 010, 022, 024 |
| `Repair` | 0 | — |
| `Missing` | 18 | 006, 007, 008, 012, 013, 014, 017, 020, 026, 027, 028, 029, 030, 036, 039, 040, 041, 044 |
| `Conflict` | 1 | 042 |
| `Unknown` | 1 | 043 |
| **Total** | **44** | — |

Dua baris `Ready to reuse` bersifat bersyarat dan tidak boleh dibaca sebagai "selesai":
`RWI-CAP-032` siap hanya pada bagian masternya, karena episode bayi tersendiri bergantung pada
`RWI-CAP-008` yang belum ada; dan `RWI-CAP-033` siap hanya sebagai penyaring pencarian, sesuai
`RWI-DEC-018` yang masih berstatus `draft` dan menjadi gerbang keras sebelum produksi.

Status `Repair` tidak dipakai satu kali pun. Cacat perilaku yang ditemukan pada bagian 9 melekat
pada frontend yang memanggil endpoint tidak ada, sehingga digolongkan `Conflict`, bukan `Repair`.

---

## 6. Kontrak backend as-is

Bagian ini menyalin kontrak yang **benar-benar berlaku hari ini**, supaya `/qv-design` tidak
menebak. Judul tiap bagian memakai nilai atribut `[Tags(...)]` apa adanya, sehingga dapat dicocokkan
langsung dengan halaman Swagger.

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`
Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/BedController.cs:24` dan `:34`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Mengambil pilihan penyaring beserta nilai bawaannya untuk layar pencarian bed | `Bed : Read` | – | `ApiResponse<BedFilterMetadataResponse>` |
| GET | `/summary` | Menghitung jumlah bed per keadaan: total, aktif, tersedia, terisi, perbaikan, dapat dipesan, isolasi, intensif, ODC, bayi, laki-laki, perempuan | `Bed : Read` | – | `ApiResponse<BedSummaryResponse>` |
| GET | `/` | Daftar bed bertingkat dengan 13 penyaring: `roomId`, `serviceUnitId`, `patientClassId`, `isActive`, `bedStatus`, `isForMale`, `isForFemale`, `isForNewborn`, `isIsolationBed`, `isIntensiveCareBed`, `isOdcBed`, `isReservable`, `search` | `Bed : Read` | Query | `ApiResponse<ResponseBedPagedResult>` |
| GET | `/options` | Daftar ringkas bed untuk isian pilihan pada formulir | `Bed : Read` | Query | `ApiResponse<BedOptionPagedResponse>` |
| GET | `/{id}` | Detail satu bed | `Bed : Read` | – | `ApiResponse<BedDetailResponse>` |
| POST | `/` | Menambah bed baru. Kode bed dibuat otomatis dengan awalan `BD-RSMMC-` dan lima digit | `Bed : Create` | `CreateBedRequest` | `ApiResponse<BedCreateResponse>` |
| PUT | `/{id}` | Mengubah seluruh data bed | `Bed : Update` | `UpdateBedRequest` | `ApiResponse<BedUpdateResponse>` |
| PATCH | `/{id}/status` | Mengubah **status aktif atau nonaktif** bed. Perhatikan: walaupun namanya `status`, yang diubah adalah `IsActive`, bukan `BedStatus` | `Bed : Update` | `UpdateBedStatusRequest` | `ApiResponse<BedUpdateResponse>` |
| PATCH | `/{id}/availability` | Mengubah `BedStatus`, misalnya dari `Available` menjadi `Occupied` | `Bed : Update` | `UpdateBedAvailabilityRequest` | `ApiResponse<BedUpdateResponse>` |
| DELETE | `/{id}` | Menandai bed terhapus disertai alasan | `Bed : Delete` | `DeleteBedRequest` | `ApiResponse<BedDeleteResponse>` |

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| 200 | Permintaan berhasil |
| 400 | Isian tidak lengkap atau formatnya salah, misalnya rentang tanggal penyaring tidak masuk akal |
| 401 | Pengguna belum login atau sesi sudah berakhir |
| 403 | Pengguna tidak punya hak akses untuk tindakan ini |
| 404 | Bed yang dimaksud tidak ditemukan |

**Dua catatan yang memengaruhi desain Rawat Inap:**

1. Penamaan endpoint membingungkan. `/status` mengubah aktif atau nonaktif, sedangkan `/availability`
   mengubah `BedStatus`. Bukti: `.cs:498-500` mengisi `entity.IsActive = request.IsActive`,
   sementara `.cs:534` mengisi `entity.BedStatus = request.BedStatus`.
2. `/availability` tidak punya pengaman apa pun. Tidak ada pemeriksaan bahwa bed sedang kosong, tidak
   ada penguncian baris, dan tidak ada catatan pasien. Dua petugas yang menekan tombol pada waktu
   hampir bersamaan sama-sama berhasil, dan yang terakhir menimpa yang pertama.

### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`
Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/RoomController.cs:25` dan `:35`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Pilihan penyaring kamar | `Room : Read` | – | `ApiResponse<...>` |
| GET | `/summary` | Ringkasan jumlah kamar | `Room : Read` | – | `ApiResponse<...>` |
| GET | `/` | Daftar kamar bertingkat | `Room : Read` | Query | `ApiResponse<...>` |
| GET | `/options` | Daftar ringkas kamar untuk isian pilihan | `Room : Read` | Query | `ApiResponse<...>` |
| GET | `/{id}` | Detail satu kamar | `Room : Read` | – | `ApiResponse<...>` |
| POST | `/` | Menambah kamar | `Room : Create` | Body | `ApiResponse<...>` |
| PUT | `/{id}` | Mengubah kamar | `Room : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/status` | Mengubah status aktif kamar | `Room : Update` | Body | `ApiResponse<...>` |
| DELETE | `/{id}` | Menandai kamar terhapus | `Room : Delete` | Body | `ApiResponse<...>` |

Kolom `MstRoom` yang relevan untuk Rawat Inap:
`ServiceUnitId` (`.../MstRoom.cs:14`), `PatientClassId` (`:16`), `RoomType` (`:26`), `Capacity`
(`:37`), `IsForMale`, `IsForFemale`, `IsForNewborn`, `IsIsolationRoom`, `IsIntensiveCare`,
`IsOdcRoom` (`:39-49`), dan `IsAvailableForAdmission` (`:51`).

Nilai `RoomType` yang tersedia: `Unknown`, `OutpatientRoom`, `InpatientRoom`, `EmergencyRoom`,
`IntensiveCareRoom`, `IsolationRoom`, `BabyRoom`, `DeliveryRoom`, `OperatingRoom`, `OdcRoom`,
`ProcedureRoom`, `ObservationRoom`, `Other`.

### Health Services / Master Data / Patient Class

Base URL: `api/v1/health-services/master-data/patient-classes`
Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/PatientClassController.cs:24` dan `:34`.

Bentuk endpointnya sama persis dengan Room: `filters/metadata`, `summary`, daftar, `options`,
detail, tambah, ubah, `PATCH /{id}/status`, dan hapus.

Nilai `PatientClassType` yang tersedia: `Unknown`, `General`, `Class3`, `Class2`, `Class1`, `VIP`,
`VVIP`, `ICU`, `HCU`, `NICU`, `Isolation`, `Executive`, `Baby`, `Labor`, `Odc`, `Perinatology`,
`Suite`, `Luxury`, `Other`.

### Health Services / Master Data / Service Unit

Base URL: `api/v1/health-services/master-data/service-units`
Bukti: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs:25` dan `:35`.

Bentuk endpointnya sama dengan Room dan Patient Class.

Nilai `ServiceUnitType`: `Unknown`, `Outpatient`, `Inpatient`, `Emergency`, `Laboratory`,
`Radiology`, `Pharmacy`, `MedicalCheckup`, `OperatingRoom`, `DeliveryRoom`, `Other`.

Kolom yang menentukan perilaku antrean: `IsQueueRequired` (`.../MstServiceUnit.cs:38`),
`IsDoctorRequired` (`:40`), dan `IsScreeningRequired` (`:42`). Ketiganya menjadi kunci hambatan
pertama pada bagian 3.2.

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`
Bukti: `BE@5afb54b Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:33` dan `:43`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/admin/filters/metadata` | Pilihan penyaring untuk petugas | `PatientEncounter : Read` | – | `ApiResponse<...>` |
| GET | `/filters/metadata` dan `/kiosk/filters/metadata` | Pilihan penyaring untuk kiosk | Tanpa penjaga hak akses | – | `ApiResponse<...>` |
| GET | `/summary` dan `/admin/summary` | Ringkasan kunjungan | `PatientEncounter : Read` | Query | `ApiResponse<...>` |
| GET | `/admin` | Daftar kunjungan bertingkat, dapat disaring `encounterType` | `PatientEncounter : Read` | Query | `ApiResponse<...>` |
| GET | `/admin/options`, `/options`, `/kiosk/options` | Daftar ringkas kunjungan | Beragam | Query | `ApiResponse<...>` |
| GET | `/{id}` dan `/admin/{id}` | Detail satu kunjungan | `PatientEncounter : Read` | – | `ApiResponse<...>` |
| POST | `/admin` | Membuat kunjungan oleh petugas | `PatientEncounter : Create` | `PatientEncounterCreateRequest` | `ApiResponse<...>` |
| POST | `/kiosk` | Membuat kunjungan dari kiosk | Tanpa penjaga hak akses | `PatientEncounterCreateRequest` | `ApiResponse<...>` |
| PATCH | `/{id}/status` | Mengubah status kunjungan | `PatientEncounter : Update` | `PatientEncounterStatusRequest` | `ApiResponse<object>` |
| PATCH | `/{id}/check-in` | Menandai pasien sudah hadir | `PatientEncounter : Update` | – | `ApiResponse<object>` |
| PATCH | `/{id}/cancel` | Membatalkan kunjungan beserta antreannya | `PatientEncounter : Update` | `PatientEncounterCancelRequest` | `ApiResponse<object>` |
| DELETE | `/{id}` | Menandai kunjungan terhapus | `PatientEncounter : Delete` | Body | `ApiResponse<...>` |

Alur pembuatan kunjungan yang berlaku hari ini, urut:

1. Sistem menentukan tanggal kunjungan yang dituju (`.cs:412-422`).
2. Bila tipe kunjungan adalah rawat jalan, kelas pasien dipaksa `"RAWAT JALAN"`. Bila bukan, kelas
   yang dikirim dipakai apa adanya (`.cs:428-440` dan `:1417`).
3. Sistem menentukan apakah antrean diperlukan, diambil dari klinik bila ada, kalau tidak dari unit
   layanan (`.cs:499`).
4. Baris kunjungan dan baris penjamin dibuat bersamaan (`.cs:584-585`).
5. Bila antrean diperlukan, nomor antrean dibuat dan baris antrean disimpan (`.cs:589-620`).

**Yang perlu diperhatikan untuk Rawat Inap.** Bila unit layanan rawat inap disetel
`IsQueueRequired = false`, langkah 5 dilewati sehingga tidak ada antrean sama sekali. Itu memang
benar secara proses, tetapi justru membuat pengkajian, konsultasi, resep, diagnosis, dan tindakan
tidak dapat dibuat, karena semuanya menuntut antrean atau konsultasi.

### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`
Bukti: `BE@5afb54b Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs:24` dan `:34`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | Daftar pengkajian, dapat disaring per antrean | `PatientAssessment : Read` | Query | `ApiResponse<...>` |
| GET | `/{id}` | Detail satu pengkajian | `PatientAssessment : Read` | – | `ApiResponse<...>` |
| GET | `/active-by-encounter/{encounterId}` | Pengkajian aktif milik satu kunjungan | `PatientAssessment : Read` | – | `ApiResponse<...>` |
| GET | `/active-by-queue/{queueId}` | Pengkajian aktif milik satu antrean | `PatientAssessment : Read` | – | `ApiResponse<...>` |
| POST | `/` | Membuat pengkajian. **Wajib menyertakan `QueueId` dan `EncounterId` yang cocok** | `PatientAssessment : Create` | Body | `ApiResponse<...>` |
| PUT | `/{id}` | Mengubah isi pengkajian | `PatientAssessment : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/complete` | Menyelesaikan pengkajian | `PatientAssessment : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/cancel` | Membatalkan pengkajian | `PatientAssessment : Update` | Body | `ApiResponse<...>` |

Kabar baiknya, pembacaan sudah tersedia per kunjungan lewat `/active-by-encounter/{encounterId}`,
sehingga sisi baca tidak bergantung antrean. Yang bergantung antrean hanya sisi tulis.

### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`
Bukti: `BE@5afb54b .../Controllers/DoctorConsultationController.cs:27` dan `:37`.

Kontrak yang mengikat: `QueueId` wajib diisi (`.cs:206`), dan baris antrean yang ditunjuk harus
benar-benar ada dan cocok dengan kunjungan (`.cs:255-258`).

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`
Bukti: `BE@5afb54b .../Controllers/PatientIntegratedProgressNoteController.cs:25` dan `:35`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Pilihan penyaring CPPT | `PatientIntegratedProgressNote : Read` | – | `ApiResponse<...>` |
| GET | `/` | Daftar CPPT bertingkat | `PatientIntegratedProgressNote : Read` | Query | `ApiResponse<...>` |
| GET | `/timeline` | CPPT tersusun urut waktu | `PatientIntegratedProgressNote : Read` | Query | `ApiResponse<...>` |
| GET | `/{id}` | Detail satu catatan | `PatientIntegratedProgressNote : Read` | – | `ApiResponse<...>` |
| GET | `/draft-from-consultation/{consultationId}` | Rancangan catatan dari satu konsultasi | `PatientIntegratedProgressNote : Read` | – | `ApiResponse<...>` |
| POST | `/` | Membuat catatan. `EncounterId`, `QueueId`, dan `ConsultationId` semuanya boleh kosong | `PatientIntegratedProgressNote : Create` | Body | `ApiResponse<...>` |
| POST | `/from-consultation/{consultationId}` | Membuat catatan dari satu konsultasi | `PatientIntegratedProgressNote : Create` | Body | `ApiResponse<...>` |
| PUT | `/{id}` | Mengubah catatan | `PatientIntegratedProgressNote : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/cancel` | Membatalkan catatan | `PatientIntegratedProgressNote : Update` | Body | `ApiResponse<...>` |
| DELETE | `/{id}` | Menandai catatan terhapus | `PatientIntegratedProgressNote : Delete` | Body | `ApiResponse<...>` |

**Tidak ada endpoint verifikasi.** Ini yang membuat `RWI-CAP-020` berstatus `Missing`.

### Health Services / Clinical Management / Patient Consent

Base URL: `api/v1/health-services/clinical-management/patient-consents`
Bukti: `BE@5afb54b .../Controllers/PatientConsentController.cs:25` dan `:35`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Pilihan penyaring persetujuan | `PatientConsent : Read` | – | `ApiResponse<...>` |
| GET | `/` | Daftar persetujuan bertingkat | `PatientConsent : Read` | Query | `ApiResponse<...>` |
| GET | `/options` | Daftar ringkas persetujuan | `PatientConsent : Read` | Query | `ApiResponse<...>` |
| GET | `/{id}` | Detail satu persetujuan | `PatientConsent : Read` | – | `ApiResponse<...>` |
| POST | `/` | Membuat persetujuan | `PatientConsent : Create` | Body | `ApiResponse<...>` |
| PUT | `/{id}` | Mengubah persetujuan | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/sign` | Menandatangani | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/verify` | Memverifikasi | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/approve` | Menyetujui | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/reject` | Menolak | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/withdraw` | Menarik kembali persetujuan | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/cancel` | Membatalkan | `PatientConsent : Update` | Body | `ApiResponse<...>` |
| DELETE | `/{id}` | Menandai terhapus | `PatientConsent : Delete` | Body | `ApiResponse<...>` |

Alur status persetujuan yang tersedia: `Draft`, `PendingSignature`, `Signed`, `Verified`,
`Approved`, `Rejected`, `Withdrawn`, `Expired`, `Cancelled`, `EnteredInError`.

### Health Services / Pharmacy Management / Prescription

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`
Bukti: `BE@5afb54b Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs:28` dan `:38`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Pilihan penyaring resep | `Prescription : Read` | – | `ApiResponse<...>` |
| GET | `/` | Daftar resep bertingkat | `Prescription : Read` | Query | `ApiResponse<...>` |
| GET | `/options` | Daftar ringkas resep | `Prescription : Read` | Query | `ApiResponse<...>` |
| GET | `/active-by-consultation/{consultationId}` | Resep aktif milik satu konsultasi | `Prescription : Read` | – | `ApiResponse<PrescriptionDetailResponse>` |
| GET | `/{id}` | Detail satu resep | `Prescription : Read` | – | `ApiResponse<PrescriptionDetailResponse>` |
| POST | `/` | Membuat kepala resep. **Wajib `ConsultationId` dan `EncounterId` yang cocok** | `Prescription : Create` | `CreatePrescriptionRequest` | `ApiResponse<PrescriptionCreateResponse>` |
| PUT | `/{id}` | Mengubah kepala resep | `Prescription : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/billing-generated` | Menandai tagihan sudah dibuat | `Prescription : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/payment-paid` | Menandai sudah dibayar | `Prescription : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/insurance-approved` | Menandai disetujui penjamin | `Prescription : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/payment-waived` | Menandai pembayaran dibebaskan | `Prescription : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/cancel` | Membatalkan resep | `Prescription : Update` | Body | `ApiResponse<...>` |
| DELETE | `/{id}` | Menandai terhapus | `Prescription : Delete` | Body | `ApiResponse<...>` |

**Tidak ada kolom jenis resep.** Untuk `RWI-RULE-024` obat pulang, tidak ada tempat menyimpan
penanda "ini resep obat pulang". Inilah dasar status `Extend` pada `RWI-CAP-022`.

### Health Services / Emergency Installation Management / Emergency Disposition

Base URL: `api/v1/health-services/emergency-installation-management/emergency-dispositions`
Bukti: `BE@5afb54b .../Controllers/EmergencyDispositionController.cs:23` dan `:33`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | Daftar keputusan disposisi | `EmergencyDisposition : Read` | Query | `ApiResponse<...>` |
| GET | `/{id}` | Detail satu keputusan | `EmergencyDisposition : Read` | – | `ApiResponse<...>` |
| POST | `/` | Membuat keputusan disposisi. Unit tujuan wajib bila jenis disposisi menuntutnya | `EmergencyDisposition : Create` | Body | `ApiResponse<...>` |
| PUT | `/{id}` | Mengubah keputusan | `EmergencyDisposition : Update` | Body | `ApiResponse<...>` |
| PATCH | `/{id}/disposition-status` | Mengubah status keputusan | `EmergencyDisposition : Update` | Body | `ApiResponse<...>` |
| DELETE | `/{id}` | Menandai terhapus | `EmergencyDisposition : Delete` | Body | `ApiResponse<...>` |

Pemeriksaan yang berlaku saat membuat keputusan: bila `RequiresDestinationServiceUnit` bernilai
benar, `DestinationServiceUnitId` wajib diisi dan unit layanannya harus benar-benar ada.

Bukti: `BE@5afb54b .../Services/EmergencyDispositionService.cs:60-72`.

### Health Services / Emergency Installation Management / Emergency Transfer

Base URL: `api/v1/health-services/emergency-installation-management/emergency-transfers`
Bukti: `BE@5afb54b .../Controllers/EmergencyTransferController.cs:21` dan `:31`.

Tabel ini disebutkan bukan karena akan dipakai, melainkan karena bentuknya adalah **contoh
perpindahan yang sudah disetujui repository**: `FromServiceUnitId`, `ToServiceUnitId`, `FromRoomId`,
`ToRoomId`, `FromBedId`, `ToBedId`, `TransferStatus`, `RequestedByUserId`, `AcceptedByUserId`,
`SendingNurseUserId`, `ReceivingNurseUserId`, `TransferReason`, `HandoverSummary`, dan
`RejectionReason`.

---

## 7. Kontrak frontend as-is

### 7.1 Yang dapat dijangkau pengguna hari ini

| Layar | Route | Menu | Bukti |
| --- | --- | --- | --- |
| Master tempat tidur | `/health-services/master-data/bed` | "Tempat Tidur" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:683-686` |
| Master kamar | `/health-services/master-data/room` | "Ruangan" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:676-680` |
| Master kelas pasien | `/health-services/master-data/patient-class` | "Kelas Pasien" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:692-696` |
| Master unit layanan | `/health-services/master-data/service-unit` | Ada di kelompok master data | `FE@dec4fdef src/app/health-services/master-data/service-unit/` |
| Antrean dokter poliklinik | `/health-services/registration-management/doctor-queues` | "Dokter → Rawat Jalan" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:860-870` |
| Skrining perawat | `/health-services/registration-management/nurse-station-queue` | "Rawat Jalan → Skrining Pasien" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:900-908` |
| Pendaftaran IGD | `/health-services/registration-management/emergency-registration` | "Instalasi Gawat Darurat" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:876-882` |
| Triase IGD | `/health-services/emergency-installation-management/emergency-triage` | "Triage Pasien" | `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:883-890` |
| Resep farmasi | `/health-services/pharmacy-management/prescriptions/[consultationId]` | Diakses dari alur dokter | `FE@dec4fdef src/app/health-services/pharmacy-management/prescriptions/[consultationId]/` |

### 7.2 Tidak ada satu pun layar Rawat Inap

Folder `FE@dec4fdef src/app/health-services/` hanya memuat enam anak folder:
`emergency-installation-management`, `master-data`, `patient-management`, `pharmacy-management`,
`registration-management`, dan `select-demo`. Tidak ada `inpatient-management`, tidak ada
`rawat-inap`, dan tidak ada halaman census, admisi, maupun penempatan bed.

Menu sisi kiri juga hanya mengenal "Rawat Jalan" dan "Instalasi Gawat Darurat".
Bukti: `FE@dec4fdef src/utils/menu-sidebar/menu-items.jsx:894`.

### 7.3 Seluruh dokumentasi klinis dibungkus di dalam ruang kerja antrean dokter

Ini adalah cerminan hambatan pertama di sisi frontend. Semua tab dokumentasi klinis berada di dalam
halaman antrean dokter, bukan sebagai halaman tersendiri per pasien:

| Tab | Berkas |
| --- | --- |
| SOAP | `FE@dec4fdef src/components/view/health-services/registration-management/doctor-queues/tabs/soap/doctor-soap-tab.jsx` |
| CPPT | `FE@dec4fdef .../doctor-queues/tabs/cppt/doctor-cppt-tab.jsx` |
| Tindakan | `FE@dec4fdef .../doctor-queues/tabs/procedure/doctor-procedure-tab.jsx` |
| Resep | `FE@dec4fdef src/lib/hooks/health-services/pharmacy-management/use-doctor-prescription.js` |

Ruang kerjanya sendiri berputar pada `queueId`. Hook `useDoctorConsultationWorkspace` menyimpan
peta `consultationIdByQueueId`, dan bila konsultasi belum ada, ia memanggil
`getActiveDoctorConsultationByQueue(queueId, ...)`.

Bukti: `FE@dec4fdef src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorConsultationWorkspace.js:48`,
`:69-83`, dan `:123-145`.

**Akibatnya:** untuk pasien rawat inap yang tidak punya antrean, seluruh tab itu tidak dapat dibuka
sama sekali, bukan hanya kosong isinya.

### 7.4 Layanan API frontend yang sudah ada dan dapat dipakai ulang

| Berkas | Base URL yang dipanggil |
| --- | --- |
| `FE@dec4fdef src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js:3` | `/v1/health-services/clinical-management/patient-integrated-progress-notes` |
| `FE@dec4fdef src/lib/services/health-services/clinical-management/doctor-consultation.service.js:3` | `/v1/health-services/clinical-management/doctor-consultations` |
| `FE@dec4fdef src/lib/services/health-services/clinical-management/patient-diagnosis.service.js` | `/v1/health-services/clinical-management/patient-diagnoses` |
| `FE@dec4fdef src/lib/services/health-services/clinical-management/patient-procedure.service.js` | `/v1/health-services/clinical-management/patient-procedures` |
| `FE@dec4fdef src/lib/services/health-services/clinical-management/prescribing-drug.service.js` | Peresepan dari sisi dokter |
| `FE@dec4fdef src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx:5` | `/v1/health-services/master-data/beds` |
| `FE@dec4fdef src/lib/hooks/select/health-service/health-service-select-resources.js:14` | `/health-services/master-data/beds/options` untuk isian pilihan |

Kedua layanan CPPT dan konsultasi sudah menangani pembungkus jawaban `ApiResponse` dan sudah
memaklumi kode 404 sebagai "belum ada", sehingga polanya dapat dipakai ulang.

Bukti: `FE@dec4fdef .../doctor-consultation.service.js:15-23`.

---

## 8. Perjalanan ujung-ke-ujung yang ditelusuri

Perjalanan yang dituntut `RWI-DEC-004` adalah:

> Admisi → penempatan bed → census → penugasan perawat → pengkajian awal → dokumentasi → resep →
> pindah bed → keputusan pulang → resume → clearance → penutupan → bed kembali kosong.

Hasil penelusuran langkah demi langkah:

| No | Langkah | Dapat dijalankan hari ini | Yang menghentikannya |
| ---: | --- | --- | --- |
| 1 | Memilih pasien yang sudah terdaftar | Ya | – |
| 2 | Menentukan penjamin | Ya, saat kunjungan dibuat | – |
| 3 | Menentukan DPJP | Sebagian | Hanya satu kolom dokter tanpa peran DPJP |
| 4 | Mencari bed kosong | Ya | – |
| 5 | Memesan bed selama 2 jam | **Tidak** | Tidak ada catatan pemesanan (`RWI-CAP-006`) |
| 6 | Menempatkan pasien di bed | **Tidak** | Tidak ada catatan penempatan (`RWI-CAP-007`) |
| 7 | Mengaktifkan episode | **Tidak** | Tidak ada episode (`RWI-CAP-008`) |
| 8 | Menampilkan census | **Tidak** | Tidak ada census (`RWI-CAP-012`) |
| 9 | Menugaskan perawat | **Tidak** | Hanya ada penugasan ke klaster antrean (`RWI-CAP-014`) |
| 10 | Menulis pengkajian awal | **Tidak** | Butuh antrean (`RWI-CAP-015`) |
| 11 | Menulis SOAP dan kajian dokter | **Tidak** | Butuh antrean (`RWI-CAP-018`) |
| 12 | Menulis CPPT | Ya | – |
| 13 | Mencatat tanda vital | Ya | – |
| 14 | Membuat resep | **Tidak** | Butuh konsultasi (`RWI-CAP-021`) |
| 15 | Mencatat tindakan | **Tidak** | Butuh konsultasi (`RWI-CAP-023`) |
| 16 | Menghitung visite | Sebagian | Datanya ada di CPPT, agregasinya belum (`RWI-CAP-024`) |
| 17 | Memindahkan pasien | **Tidak** | Tidak ada perpindahan milik episode (`RWI-CAP-017`) |
| 18 | Memutuskan pasien boleh pulang | **Tidak** | Tidak ada status `DischargePending` |
| 19 | Membuat resume pulang | Sebagian | Ada surat keterangan, belum menjadi bagian episode (`RWI-CAP-025`) |
| 20 | Memeriksa kelayakan keuangan | **Tidak** | Billing hanya master (`RWI-CAP-027`) |
| 21 | Memeriksa kelengkapan administrasi | **Tidak** | Tidak ada daftar periksa (`RWI-CAP-028`) |
| 22 | Menutup episode | **Tidak** | Tidak ada penutupan (`RWI-CAP-029`) |
| 23 | Mengosongkan bed kembali | Hanya manual | Lewat menu master data, tanpa kaitan pasien |

Dari 23 langkah: **5 dapat dijalankan penuh** (nomor 1, 2, 4, 12, 13), **3 sebagian** (nomor 3, 16,
19), **14 tidak dapat dijalankan sama sekali**, dan **1 hanya bisa dikerjakan manual lewat menu
master data** (nomor 23).

Perjalanan jalur masuk dari IGD juga ditelusuri terpisah:

| No | Langkah | Dapat dijalankan | Bukti |
| ---: | --- | --- | --- |
| 1 | Dokter IGD memilih disposisi "Rawat inap" | Ya | Jenis `RANAP` sudah diisi seeder |
| 2 | Petugas memilih unit layanan tujuan | Ya | `RequiresDestinationServiceUnit = true` |
| 3 | Sistem memesan tempat tidur di unit tujuan | **Tidak** | Tidak ada pemesanan |
| 4 | Sistem membuat episode rawat inap | **Tidak** | Tidak ada episode |
| 5 | Kunjungan IGD ditutup | **Tidak** | `ClosesEmergencyVisit` tidak pernah dijalankan |
| 6 | Bangsal menerima pasien | **Tidak** | Tidak ada serah terima |

---

## 9. Ketidakcocokan dan konflik antara frontend dan backend

### 9.1 `RWI-CON-TRC-001` — Tombol aktif dan nonaktif tempat tidur memanggil endpoint yang tidak ada

**Tingkat: Confirmed conflict.**

Halaman detail tempat tidur punya tombol untuk mengaktifkan dan menonaktifkan bed. Tombol itu
memanggil dua thunk Redux:

- `deactivateBed` memanggil `PATCH /v1/health-services/master-data/beds/{id}/deactivate`
- `activateBed` memanggil `PATCH /v1/health-services/master-data/beds/{id}/activate`

Bukti frontend: `FE@dec4fdef src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx:315-322`
dan `:334-341`. Pemakaiannya: `FE@dec4fdef src/lib/hooks/health-services/master-data/bed/use-master-data-bed-detail.jsx:184`
dan `:191`.

Di sisi backend, `BedController` **tidak punya** route `/activate` maupun `/deactivate`. Seluruh
route yang ada hanya sepuluh: `filters/metadata`, `summary`, daftar, `options`, detail, tambah,
ubah, `/{id}/status`, `/{id}/availability`, dan hapus.

Bukti backend: `BE@5afb54b Areas/HealthServices/MasterData/Controllers/BedController.cs:52`, `:104`,
`:135`, `:221`, `:286`, `:318`, `:403`, `:478`, `:514`, `:551`. Pencarian kata `activate` pada
berkas itu tidak menghasilkan satu baris pun.

**Akibat bagi pengguna:** ketika petugas menekan tombol "Nonaktifkan" pada halaman detail tempat
tidur, permintaan akan dijawab 404 dan muncul pesan gagal, padahal pesan sukses sudah disiapkan di
kode. Bed tidak pernah benar-benar berubah statusnya.

**Ini bukan salah tulis pada frontend semata.** Pola `/activate` dan `/deactivate` memang dipakai di
tempat lain dan di sana backendnya ada:

| Master | Frontend memanggil | Backend menyediakan |
| --- | --- | --- |
| Jadwal dokter | `/activate`, `/deactivate` | Ada, `BE@5afb54b .../DoctorScheduleController.cs:924` dan `:935` |
| Aturan penjaminan | `/activate`, `/deactivate` | Ada, `BE@5afb54b .../InsuranceCoverageRuleController.cs:605` dan `:615` |
| Tempat tidur | `/activate`, `/deactivate` | **Tidak ada** |
| Master lain (usia, obat, pemasok, dan seterusnya) | `/status` | Ada |

Jadi ada tiga gaya yang hidup berdampingan, dan tempat tidur memakai gaya yang backendnya belum
menyusul.

**Kenapa ini penting bagi Rawat Inap:** modul Rawat Inap akan sangat bergantung pada master tempat
tidur. Bila menonaktifkan bed saja tidak berfungsi, admin tidak dapat menutup bed yang sedang
diperbaiki, dan pencarian bed kosong akan menampilkan bed yang seharusnya tidak boleh dipakai.

### 9.2 `RWI-CON-TRC-002` — Penamaan `/status` pada tempat tidur bermakna ganda

**Tingkat: Confirmed conflict penamaan.**

Pada hampir semua master, `PATCH /{id}/status` berarti mengubah aktif atau nonaktif. Pada tempat
tidur, `PATCH /{id}/status` juga mengubah aktif atau nonaktif, tetapi ada endpoint kedua
`/availability` yang mengubah `BedStatus`. Nama `status` di sini menunjuk hal yang berbeda dari
`BedStatus` yang juga bernama status.

Bukti: `BE@5afb54b .../BedController.cs:498-500` mengisi `entity.IsActive`, sedangkan `.cs:534`
mengisi `entity.BedStatus`.

**Akibat:** desain Rawat Inap harus menyebut keduanya secara eksplisit agar tidak tertukar. Contoh
kalimat yang aman: "bed nonaktif" untuk `IsActive = false`, dan "bed terisi" untuk
`BedStatus = Occupied`.

### 9.3 `RWI-CON-TRC-003` — Penanda kontrak IGD yang tidak pernah dijalankan

**Tingkat: Confirmed conflict antara kontrak tertulis dan perilaku nyata.**

Master jenis disposisi menjanjikan `ClosesEmergencyVisit`, tetapi tidak ada satu pun alur kerja yang
membacanya untuk menutup kunjungan IGD. Rinciannya sudah dipaparkan pada `RWI-TRC-008`.

**Akibat bagi Rawat Inap:** `RWI-RULE-005` menyatakan kunjungan IGD yang sudah ada dipakai apa
adanya. Bila kelak `ClosesEmergencyVisit` benar-benar dijalankan, kunjungan IGD akan tertutup dan
episode rawat inap kehilangan jangkarnya. Perlu keputusan sebelum desain dikunci.

### 9.4 Ketidakcocokan bentuk yang bukan cacat, tetapi wajib dicatat

| Hal | Bentuk backend | Bentuk yang dibutuhkan Rawat Inap |
| --- | --- | --- |
| Pengkajian | Satu pengkajian per antrean poliklinik | Satu pengkajian awal per episode menginap |
| Konsultasi | Satu konsultasi per antrean, per kunjungan poli | Banyak catatan dokter selama berhari-hari |
| Resep | Satu resep per konsultasi | Banyak resep harian ditambah satu resep obat pulang |
| Kunjungan | Satu kunjungan berumur satu hari | Satu kunjungan berumur beberapa hari |
| Status kunjungan | Berputar pada antrean dan konsultasi | Berputar pada admisi, rencana pulang, dan penutupan |

---

## 10. Fakta, inferensi, dan rekomendasi

Bagian ini sengaja dipisahkan supaya pembaca tahu mana yang terbukti dan mana yang merupakan
penilaian.

### 10.1 Fakta — terbukti langsung dari source

| ID | Fakta | Bukti |
| --- | --- | --- |
| `RWI-TF-001` | Tidak ada folder, entity, `DbSet`, endpoint, atau berkas apa pun berawalan `Inp` | `BE@5afb54b Areas/HealthServices/` dan `Repositories/ApplicationDbContext.cs` |
| `RWI-TF-002` | `MstBed` sudah punya `BedStatus` dengan nilai `Reserved`, `IsReservable`, dan tujuh penanda peruntukan | `BE@5afb54b .../MstBed.cs:27-41`; `Enums/BedStatus.cs:3-13` |
| `RWI-TF-003` | Satu-satunya penulis `MstBed.BedStatus` adalah CRUD master data | Pencarian menyeluruh `MstBed`, `BedStatus`, `BedId` pada `Areas/`, `Services/`, `Repositories/` |
| `RWI-TF-004` | Pemaksaan kelas `"RAWAT JALAN"` hanya berlaku untuk `EncounterType.Outpatient` | `BE@5afb54b .../PatientEncounterController.cs:1417` |
| `RWI-TF-005` | `EncounterType.Inpatient` sudah ada dengan nilai 3 | `BE@5afb54b .../Enums/EncounterType.cs:8` |
| `RWI-TF-006` | `EncounterStatus` tidak memuat `Admitted`, `DischargePending`, maupun `Closed` | `BE@5afb54b .../Enums/EncounterStatus.cs` |
| `RWI-TF-007` | Perubahan status kunjungan tidak dijaga aturan perpindahan apa pun | `BE@5afb54b .../PatientEncounterController.cs:864-894` |
| `RWI-TF-008` | Pengkajian dan konsultasi mewajibkan baris antrean yang benar-benar ada | `BE@5afb54b .../PatientAssessmentController.cs:265-267`; `.../DoctorConsultationController.cs:206`, `:255-258` |
| `RWI-TF-009` | Resep mewarisi seluruh konteks dari konsultasi | `BE@5afb54b .../PrescriptionController.cs:278-281`, `:292-306` |
| `RWI-TF-026` | Satu kunjungan hanya boleh punya satu konsultasi dokter. Penjaganya memeriksa `EncounterId`, bukan `QueueId`, sehingga antrean semu tidak dapat melewatinya | `BE@5afb54b .../DoctorConsultationController.cs:809-815` |
| `RWI-TF-027` | Satu konsultasi hanya boleh punya satu resep aktif, dan konsultasi yang sudah `Completed` tidak boleh ditambah resep | `BE@5afb54b .../PrescriptionController.cs:575`, `:578-581` |
| `RWI-TF-028` | Pembuatan pengkajian dan konsultasi juga memeriksa status antrean dan penanda `IsScreeningRequired`/`IsDoctorRequired` pada baris antrean | `BE@5afb54b .../PatientAssessmentController.cs:645-661`; `.../DoctorConsultationController.cs:797-808` |
| `RWI-TF-010` | CPPT, tanda vital, persetujuan, surat keterangan, dan dokumen klinis tidak mewajibkan antrean maupun konsultasi | Model masing-masing, seluruh pengait bertipe `Guid?` |
| `RWI-TF-011` | CPPT tidak punya kolom maupun endpoint verifikasi | `BE@5afb54b .../TrxPatientIntegratedProgressNote.cs`; `.../PatientIntegratedProgressNoteController.cs` |
| `RWI-TF-012` | `BillingManagement` hanya berisi dua master dan satu service kosong | `BE@5afb54b .../BillingModuleService.cs:10-15` |
| `RWI-TF-013` | Tidak ada seeder untuk bed, kamar, unit layanan, dan kelas pasien | Daftar lengkap `Seeders/` dan `Areas/HealthServices/MasterData/Seeders/` |
| `RWI-TF-014` | Hak akses hanya mengenal peran terhadap pasangan controller dan action | `BE@5afb54b Filters/AccessPermissionFilter.cs:28-77`; `Services/Security/AccessPermissionService.cs:26` |
| `RWI-TF-015` | Butir hak akses dibuat otomatis dari atribut saat aplikasi dinyalakan | `BE@5afb54b Seeders/AccessMenuSeeder.cs:22-60` |
| `RWI-TF-016` | Jenis disposisi `RANAP` sudah diisi seeder dan mewajibkan unit layanan tujuan | `BE@5afb54b .../EmergencyMasterDataSeeder.cs:284` |
| `RWI-TF-017` | `ClosesEmergencyVisit` tidak pernah dibaca satu pun alur kerja | Pencarian menyeluruh kata itu di seluruh source |
| `RWI-TF-018` | `TrxEmergencyTransfer.FromBedId` dan `ToBedId` hanya diindeks, tanpa relasi ke `MstBed` | `BE@5afb54b Repositories/Configurations/.../TrxEmergencyTransferConfiguration.cs:31-32` |
| `RWI-TF-019` | Catatan audit berupa berkas log, bukan tabel database | `BE@5afb54b Services/Logging/LoggerService.cs:88-108` |
| `RWI-TF-020` | Pola tabel riwayat status sudah ada dan terbukti di modul Workflow HR | `BE@5afb54b .../TrxWorkflowStatusHistory.cs:12-41` |
| `RWI-TF-021` | Frontend tidak punya satu pun route atau menu Rawat Inap | `FE@dec4fdef src/app/health-services/`; `src/utils/menu-sidebar/menu-items.jsx:894` |
| `RWI-TF-022` | Frontend memanggil `/beds/{id}/activate` dan `/deactivate` yang tidak ada di backend | `FE@dec4fdef .../master-data-bed-slice.jsx:320`, `:339` versus `BE@5afb54b .../BedController.cs` |
| `RWI-TF-023` | Seluruh dokumentasi klinis di frontend berputar pada `queueId` | `FE@dec4fdef .../useDoctorConsultationWorkspace.js:123-145` |
| `RWI-TF-024` | Tidak ada test yang menyentuh bed, kunjungan, atau dokumentasi klinis | Satu berkas test backend, empat berkas test frontend |
| `RWI-TF-025` | Registry masih mencatat `InPatientManagement / Inpatient`, prefix `Inp`, lifecycle `PLANNED` | `BE@5afb54b docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:20` |

### 10.2 Inferensi — penilaian agent, bukan fakta

| ID | Inferensi | Dasar |
| --- | --- | --- |
| `RWI-TI-001` | Keharusan `QueueId` dan `ConsultationId` adalah hambatan tunggal terbesar. Enam kemampuan MUST bergantung padanya | `RWI-TF-008`, `RWI-TF-009`, `RWI-TF-023` |
| `RWI-TI-002` | Master tempat tidur dirancang dengan rawat inap dalam pikiran, tetapi mesinnya tidak pernah dibangun. Kolom `IsReservable`, `IsForNewborn`, dan `IsIsolationBed` tidak punya konsumen apa pun hari ini | `RWI-TF-002`, `RWI-TF-003` |
| `RWI-TI-003` | Gerbang keuangan `RWI-RULE-009` akan selalu menahan bila dibangun sekarang, sehingga jalan keluar supervisor berubah dari pengecualian menjadi jalur normal | `RWI-TF-012` |
| `RWI-TI-004` | Menaruh riwayat lokasi pada `TrxPatientEncounter` bukan pilihan yang wajar, karena kunjungan hari ini hanya punya satu `RoomId` dan dimiliki modul Registrasi | `RWI-TF-006`, registry |
| `RWI-TI-005` | Aturan "hanya DPJP episode ini" tidak dapat dititipkan ke mesin hak akses. Penjaganya harus ditulis di dalam service modul | `RWI-TF-014` |
| `RWI-TI-006` | Cacat tombol aktif dan nonaktif bed kemungkinan besar tidak pernah diuji, karena tidak ada test yang menyentuh bed sama sekali | `RWI-TF-022`, `RWI-TF-024` |
| `RWI-TI-007` | Pemakaian `TrxMedicalCertificate` sebagai resume pulang akan mencampur dua hal: surat untuk pasien, dan catatan resmi episode | `RWI-TF-010` |

### 10.3 Rekomendasi — untuk dipertimbangkan, bukan untuk dijalankan tanpa persetujuan

Rekomendasi berikut tidak mengikat dan tidak boleh dianggap keputusan.

| ID | Rekomendasi | Alasan |
| --- | --- | --- |
| `RWI-TR-001` | Jadikan penyelesaian ketergantungan antrean sebagai keputusan pertama sebelum desain apa pun disusun. Tanpa itu, enam kemampuan MUST tidak dapat direncanakan | `RWI-TI-001` |
| `RWI-TR-002` | Pertimbangkan catatan penempatan tempat tidur sebagai satu-satunya sumber kebenaran penghunian, dan perlakukan `MstBed.BedStatus` sebagai bayangan yang dihitung, bukan sebagai sumber | `RWI-TI-002` |
| `RWI-TR-003` | Pertimbangkan mengunci daftar bed saat penempatan supaya dua petugas tidak menempatkan dua pasien pada bed yang sama, karena endpoint yang ada sekarang tidak punya pengaman itu | `RWI-TF-003` |
| `RWI-TR-004` | Pertimbangkan tabel riwayat status episode yang mengikuti pola `TrxWorkflowStatusHistory`, karena `IdentityModel` hanya menyimpan perubahan terakhir | `RWI-TF-019`, `RWI-TF-020` |
| `RWI-TR-005` | Pertimbangkan satu tabel pengaturan Rawat Inap yang mengikuti pola `MstEmergencySetting` untuk menampung batas 2 jam, 24 jam, 1 hari, dan ambang daftar pantau | `RWI-CAP-034` |
| `RWI-TR-006` | Perbaikan tombol aktif dan nonaktif bed sebaiknya ditangani sebagai pekerjaan tersendiri milik pemilik master data, bukan diselipkan ke dalam Rawat Inap | `RWI-CON-TRC-001` |
| `RWI-TR-007` | Sebelum implementasi dimulai, pastikan data master bed, kamar, unit layanan, dan kelas pasien sudah terisi lewat aplikasi, karena Definition of Done melarang manipulasi database manual | `RWI-CAP-043` |

---

## 11. Bukti verifikasi dan keterbatasan audit

### 11.1 Apa yang benar-benar diperiksa

| Jenis pemeriksaan | Cakupan |
| --- | --- |
| Struktur modul backend | Seluruh `Areas/`, kedalaman tiga tingkat |
| Model dan entity | 13 model ClinicalManagement, 4 model RegistrationManagement, 9 model EmergencyInstallationManagement, 15 model PharmacyManagement, 32 model MasterData HealthServices, 18 model Administrator MasterData |
| Persistence | `Repositories/ApplicationDbContext.cs` dengan 446 `DbSet`, konfigurasi EF terkait bed dan transfer |
| Migration | Daftar seluruh migration dan `ApplicationDbContextModelSnapshot.cs` |
| Endpoint | Seluruh route pada 11 controller yang relevan, beserta atribut hak aksesnya |
| Hak akses | Atribut, filter, service, dan seeder hak akses |
| Seeder | Enam seeder yang terdaftar |
| Registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Frontend | Struktur `src/app/`, menu, 16 berkas service dan hook klinis, slice Redux tempat tidur, hook detail bed |
| Test | Seluruh isi `QuilvianSystemBackend.Tests/` dan `tests/` |

### 11.2 Keterbatasan yang harus diketahui pembaca

1. **Audit ini tidak menjalankan apa pun.** Tidak ada build, tidak ada aplikasi yang dinyalakan,
   tidak ada permintaan HTTP yang benar-benar dikirim. Kesimpulan bahwa `/beds/{id}/activate`
   menghasilkan 404 diambil dari ketiadaan route pada source, bukan dari percobaan.
2. **Isi database tidak diperiksa.** Karena itu `RWI-CAP-043` berstatus `Unknown` dan bukan
   `Missing` maupun `Ready to reuse`.
3. **Modul di luar sepuluh klaster tidak disisir.** Modul HR, Laboratorium, dan SelfServices hanya
   disentuh sejauh berkaitan, misalnya `MstDoctor` dan pola `TrxWorkflowStatusHistory`.
4. **Aturan bisnis yang masih `draft` tidak dinilai kelayakannya.** `RWI-DEC-018`, `RWI-DEC-029`,
   dan `RWI-DEC-035` tetap menunggu pemilik klinis dan pemilik privasi. Audit ini hanya melaporkan
   apakah source mendukungnya.
5. **Tidak ada bukti pengujian untuk kemampuan yang dinyatakan `Ready to reuse`.** Status itu
   berdasarkan pembacaan source, bukan berdasarkan test yang lulus. Ini pembatas nyata: sembilan
   kemampuan `Ready to reuse` sama sekali tidak punya test.

---

## 12. Unknown dan pertanyaan penutup untuk `/grill-me`

### 12.1 Daftar Unknown

| ID | Yang tidak dapat dijawab dari source | Kenapa |
| --- | --- | --- |
| `RWI-UNK-001` | Apakah master bed, kamar, unit layanan, dan kelas pasien sudah terisi data di lingkungan yang akan dipakai | Butuh akses database, di luar batas audit |
| `RWI-UNK-002` | Berapa banyak kamar rawat inap dan tempat tidur yang sebenarnya ada di rumah sakit | Keputusan dan data organisasi |
| `RWI-UNK-003` | Apakah unit layanan rawat inap sudah disetel `IsQueueRequired = false` | Butuh akses database |
| `RWI-UNK-004` | Apakah cacat tombol aktif dan nonaktif bed sudah diketahui pemilik master data | Butuh keterangan manusia |
| `RWI-UNK-005` | Kapan `BillingManagement` akan punya kemampuan transaksi yang dapat memberi status kelayakan keuangan | Bergantung roadmap modul lain |
| `RWI-UNK-006` | Apakah blueprint IGD yang sudah ada merencanakan serah terima ke rawat inap | Perlu pembacaan dan penyelarasan lintas blueprint, di luar batas audit source ini |

### 12.2 Pertanyaan penutup

> **Status per 21 Agustus 2026: ketujuh belas pertanyaan sudah tertutup** pada Closure Pass
> `/grill-me`. Keputusannya tercatat pada [`00-interview-decisions.md`](./00-interview-decisions.md)
> revision `2` sebagai `RWI-DEC-038` sampai `RWI-DEC-051`, dan aturannya pada `RWI-RULE-026`
> sampai `RWI-RULE-034`. Ringkasan penutupannya:
>
> | Pertanyaan | Ditutup oleh | Inti keputusan |
> |---|---|---|
> | `RWI-TRQ-001` s.d. `003` | `RWI-DEC-038`, `RWI-RULE-026` | Mesin klinis yang ada dilonggarkan untuk kunjungan rawat inap; tidak ada entity tandingan dan tidak ada antrean semu |
> | `RWI-TRQ-004`, `005` | `RWI-DEC-039`, `RWI-RULE-027` | Catatan penempatan jadi sumber kebenaran; `BedStatus` turun jadi salinan satu transaksi; `Reserved`/`Occupied` dicabut dari wewenang admin |
> | `RWI-TRQ-006` | `RWI-DEC-040`, `RWI-RULE-028` | Kelayakan keuangan disimpan di episode dan ditandai manual kasir sampai Billing siap; `RWI-DEC-015` tetap utuh |
> | `RWI-TRQ-007`, `008` | `RWI-DEC-041`, `RWI-RULE-029` | Kunjungan IGD ditutup, kunjungan rawat inap baru dibuat sebagai jangkar; keduanya dihubungkan sebagai satu rangkaian |
> | `RWI-TRQ-009` | `RWI-DEC-042`, `RWI-RULE-030` | Episode punya catatan DPJP berriwayat; penjaga kewenangan ditulis di service, bukan di mesin hak akses |
> | `RWI-TRQ-010` | `RWI-DEC-043`, `RWI-RULE-031` | Tabel riwayat status milik Rawat Inap, meniru `TrxWorkflowStatusHistory` tanpa menumpang padanya |
> | `RWI-TRQ-011` | `RWI-DEC-045`, `RWI-RULE-032` | Resume pulang jadi catatan resmi milik episode; surat keterangan tetap milik modul Klinis |
> | `RWI-TRQ-012` | `RWI-DEC-046` | Penanda obat pulang disimpan di tabel resep milik Farmasi |
> | `RWI-TRQ-013` | `RWI-DEC-047`, `RWI-RULE-033` | Perawat penanggung jawab per episode dengan riwayat; jadwal jaga tidak ditarik ke scope |
> | `RWI-TRQ-014` | `RWI-DEC-048` | Data master diisi lewat layar aplikasi; seeder hanya untuk pengembangan. Penanggung jawabnya tetap terbuka pada `RWI-OQ-036` |
> | `RWI-TRQ-015` | `RWI-DEC-049` | Cacat tombol bed diperbaiki di frontend dengan memanggil `PATCH /status` yang sudah ada; jadi prasyarat |
> | `RWI-TRQ-016` | `RWI-DEC-050`, `RWI-RULE-034` | Satu tabel pengaturan Rawat Inap meniru pola `MstEmergencySetting` |
> | `RWI-TRQ-017` | `RWI-DEC-051` | Test jadi bagian pekerjaan Rawat Inap; test regresi wajib pada setiap task yang menyentuh modul tetangga |

Pertanyaan berikut **tidak dijawab oleh dokumen ini**. Semuanya dibawa ke `/grill-me` untuk
diputuskan pemilik kebutuhan. Setiap pertanyaan disertai temuan yang memunculkannya, supaya
pemilik kebutuhan tidak perlu membaca ulang seluruh dokumen.

| ID | Pertanyaan | Temuan pemicu | Kenapa harus diputus manusia |
| --- | --- | --- | --- |
| `RWI-TRQ-001` | Bagaimana pasien rawat inap boleh menulis pengkajian, catatan dokter, diagnosis, tindakan, dan resep, mengingat semuanya hari ini menuntut antrean atau konsultasi? | `RWI-TF-008`, `RWI-TF-009` | Menyangkut kepemilikan data lintas modul dan risiko merusak alur poliklinik yang sudah berjalan |
| `RWI-TRQ-002` | Apakah modul Rawat Inap boleh membuat baris antrean semu untuk pasien menginap, atau justru dilarang karena akan mengotori laporan antrean poliklinik? | `RWI-TF-008`, `RWI-TF-023` | Keputusan proses bisnis, bukan keputusan teknis |
| `RWI-TRQ-003` | Bila antrean semu ditolak, apakah pemilik modul Klinis bersedia melonggarkan keharusan `QueueId` dan `ConsultationId`, dan siapa yang mengerjakannya? | `RWI-TF-008`, `RWI-TF-009` | Perubahan pada modul milik pihak lain memerlukan persetujuan pemiliknya |
| `RWI-TRQ-004` | Setelah modul Rawat Inap ada, mana yang menjadi sumber kebenaran penghunian tempat tidur: kolom `MstBed.BedStatus` atau catatan penempatan milik Rawat Inap? | `RWI-TF-002`, `RWI-TF-003` | Menentukan siapa pemilik data dan siapa yang boleh mengubahnya |
| `RWI-TRQ-005` | Siapa yang boleh mengubah `BedStatus` lewat menu master data setelah Rawat Inap berjalan, dan apakah hak itu perlu dicabut agar tidak bertabrakan? | `RWI-TF-003` | Keputusan kewenangan |
| `RWI-TRQ-006` | Karena Billing belum punya kemampuan transaksi, apa bentuk sementara gerbang kelayakan keuangan pada MVP: menahan penuh, memperingatkan saja, atau ditandai manual petugas? | `RWI-TF-012`, `RWI-TI-003` | `RWI-DEC-015` mengunci "memblokir", tetapi sumber datanya belum ada. Ini perubahan keputusan, bukan penafsiran |
| `RWI-TRQ-007` | Saat pasien IGD naik ke bangsal, apakah kunjungan IGD ditutup dan kunjungan rawat inap baru dibuat, atau kunjungan IGD dipakai terus sebagai jangkar episode? | `RWI-TF-016`, `RWI-TF-017` | `RWI-RULE-005` menyatakan dipakai apa adanya, tetapi master IGD menandai jenis `RANAP` sebagai penutup kunjungan |
| `RWI-TRQ-008` | Siapa yang berwenang memperbaiki penanda `ClosesEmergencyVisit` yang tidak pernah dijalankan, dan apakah itu prasyarat sebelum Rawat Inap dibangun? | `RWI-TF-017` | Menyentuh modul IGD milik pihak lain |
| `RWI-TRQ-009` | Bagaimana aturan "hanya DPJP episode ini yang boleh memindahkan pasien" ditegakkan, mengingat mesin hak akses hanya mengenal peran? | `RWI-TF-014`, `RWI-TI-005` | Menentukan besar pekerjaan dan letak penjaganya |
| `RWI-TRQ-010` | Untuk riwayat perubahan status episode, apakah dibuat tabel riwayat sendiri mengikuti pola `TrxWorkflowStatusHistory`, atau cukup mengandalkan catatan log yang ada? | `RWI-TF-019`, `RWI-TF-020` | Menentukan apakah riwayat dapat ditampilkan di layar dan diaudit |
| `RWI-TRQ-011` | Resume pulang dibuat sebagai catatan resmi milik episode rawat inap, atau memakai `TrxMedicalCertificate` yang sudah ada? | `RWI-CAP-025`, `RWI-TI-007` | Menentukan pemilik data rekam medis dan bentuk cetakannya |
| `RWI-TRQ-012` | Penanda obat pulang ditaruh di modul Farmasi sebagai jenis resep, atau di modul Rawat Inap sebagai penanda tersendiri? | `RWI-CAP-022` | Menyentuh modul Farmasi yang berstatus `ACTIVE` |
| `RWI-TRQ-013` | Penugasan perawat penanggung jawab dibuat per episode, per giliran jaga, atau memakai klaster nurse station yang sudah ada? | `RWI-CAP-014` | Keputusan proses kerja bangsal |
| `RWI-TRQ-014` | Siapa yang bertanggung jawab mengisi data master tempat tidur dan kamar sebelum modul dipakai, dan kapan batas waktunya? | `RWI-CAP-043`, `RWI-UNK-001` | Definition of Done melarang manipulasi database manual, jadi pengisian harus lewat aplikasi oleh orang yang berwenang |
| `RWI-TRQ-015` | Perbaikan tombol aktif dan nonaktif tempat tidur menjadi pekerjaan siapa, dan apakah menjadi prasyarat sebelum Rawat Inap dibangun? | `RWI-CON-TRC-001` | Menentukan urutan pekerjaan dan pemiliknya |
| `RWI-TRQ-016` | Semua parameter yang dapat diubah admin — 2 jam, 24 jam, 1 hari, dan tiga ambang daftar pantau — disatukan dalam satu tabel pengaturan Rawat Inap, atau disebar per kemampuan? | `RWI-CAP-034` | Menentukan bentuk layar pengaturan dan kewenangan admin |
| `RWI-TRQ-017` | Karena sembilan kemampuan berstatus `Ready to reuse` sama sekali belum punya test, apakah pembuatan test menjadi bagian pekerjaan Rawat Inap atau pekerjaan terpisah? | `RWI-TF-024`, keterbatasan 11.2 butir 5 | Menentukan cakupan dan besar pekerjaan |

---

## 13. Staleness dan pemicu impact scan

### 13.1 Kapan peta ini menjadi kedaluwarsa

Peta ini terikat pada dua SHA:

- Backend `5afb54bd75281648010e50ef14f43ca1f80d8efd`
- Frontend `dec4fdeff07c3c96ad9f07f41f184c54cf771371`

Begitu salah satu berubah, peta ini **ditandai stale** dan wajib melewati impact scan terbatas
sebelum dipakai lagi.

### 13.2 Perubahan yang mewajibkan impact scan penuh

| Bila berubah | Yang harus dipindai ulang |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstBed.cs`, `MstRoom.cs`, `MstPatientClass.cs`, `MstServiceUnit.cs` | `RWI-CAP-004`, `RWI-CAP-005`, `RWI-CAP-011`, `RWI-CAP-032`, `RWI-CAP-033` |
| `Areas/HealthServices/MasterData/Controllers/BedController.cs` | `RWI-CAP-004`, `RWI-CAP-042`, `RWI-CON-TRC-001`, `RWI-CON-TRC-002` |
| `Areas/HealthServices/RegistrationManagement/**` | `RWI-CAP-002`, `RWI-CAP-003`, `RWI-CAP-009`, `RWI-CAP-010` |
| `Areas/HealthServices/ClinicalManagement/**` | `RWI-CAP-015`, `RWI-CAP-016`, `RWI-CAP-018`, `RWI-CAP-019`, `RWI-CAP-020`, `RWI-CAP-023`, `RWI-CAP-024`, `RWI-CAP-025`, `RWI-CAP-031` |
| `Areas/HealthServices/PharmacyManagement/**` | `RWI-CAP-021`, `RWI-CAP-022` |
| `Areas/HealthServices/BillingManagement/**` | `RWI-CAP-027`, dan pertanyaan `RWI-TRQ-006` |
| `Areas/HealthServices/EmergencyInstallationManagement/**` | `RWI-CAP-038`, `RWI-CON-TRC-003`, dan pertanyaan `RWI-TRQ-007` |
| `Attributes/`, `Filters/AccessPermissionFilter.cs`, `Services/Security/`, `Seeders/AccessMenuSeeder.cs` | `RWI-CAP-035`, `RWI-CAP-036` |
| `Models/IdentityModel.cs`, `Services/Logging/LoggerService.cs` | `RWI-CAP-037` |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Status `PLANNED` modul, `RWI-FACT-001` sampai `RWI-FACT-003` |
| Munculnya berkas berawalan `Inp` di mana pun | Seluruh baris `Missing` wajib dinilai ulang |
| `FE src/app/health-services/**`, `src/utils/menu-sidebar/menu-items.jsx` | `RWI-CAP-041`, bagian 7 |
| `FE src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx` | `RWI-CAP-042`, `RWI-CON-TRC-001` |
| `FE src/lib/hooks/health-services/registration-management/doctor-queue/**` | Bagian 7.3, dan pertanyaan `RWI-TRQ-001` |

### 13.3 Perubahan yang tidak mewajibkan impact scan

Perubahan pada `docs/`, laporan gaya visual, berkas konfigurasi build, dan modul HR yang tidak
menyentuh `TrxWorkflowStatusHistory` tidak mengubah kesimpulan peta ini.

---

## 14. Handoff

### 14.1 Yang sudah selesai

- Sembilan pertanyaan `RWI-TRC-001` sampai `RWI-TRC-009` terjawab seluruhnya dengan bukti source.
- 44 kemampuan diklasifikasi memakai tujuh status yang diizinkan.
- Tiga konflik antara frontend dan backend dikonfirmasi.
- Enam butir `Unknown` dan 17 pertanyaan penutup dikumpulkan.
- Kontrak as-is backend dan frontend dicatat, termasuk tabel endpoint bergaya Swagger.

### 14.2 Yang belum boleh dikerjakan

- Implementasi, migration, dan pekerjaan database. Modul `InPatientManagement` masih berstatus
  `PLANNED` pada registry, dan menurut `RWI-FACT-002` status itu hanya memberi hak penamaan.
- Perbaikan cacat yang ditemukan pada bagian 9. Semuanya hanya dicatat.
- Perancangan schema, API, dan layar target.

### 14.3 Langkah berikutnya yang disarankan

1. Bawa 17 pertanyaan pada bagian 12.2 ke `/grill-me`. Empat di antaranya — `RWI-TRQ-001`,
   `RWI-TRQ-004`, `RWI-TRQ-006`, dan `RWI-TRQ-007` — memblokir desain, karena jawabannya mengubah
   bentuk tabel dan kontrak, bukan sekadar rinciannya.
2. Setelah pertanyaan penutup dijawab, jalankan `/qv-design` untuk menyusun blueprint target.
3. Peta ini tetap berlaku selama kedua SHA tidak berubah. Bila berubah, jalankan
   `/qv-trace impact-scan` lebih dulu memakai tabel pemicu pada bagian 13.2.

### 14.4 Catatan penutup tentang `RWI-DEC-001`

Dokumen keputusan mencatat pada `RWI-DEC-001` bahwa modul ini dikerjakan sebagai Scope Pass tanpa
capability map, sehingga risiko duplikasi dengan modul existing belum diperiksa. **Dengan
terbitnya dokumen ini, catatan itu sudah dapat diperbarui.** Hasil pemeriksaan duplikasi:

| Kemampuan yang berpotensi duplikat | Hasil pemeriksaan |
| --- | --- |
| Pasien | Tidak duplikat. `MstPatient` dipakai ulang |
| Dokter dan pegawai | Tidak duplikat. `MstDoctor` dan `MstWorkforceProfile` dipakai ulang |
| Kunjungan | Tidak duplikat. `TrxPatientEncounter` dipakai ulang sebagai jangkar |
| Penjamin | Tidak duplikat. `TrxPatientEncounterGuarantor` dipakai ulang |
| Tindakan | Tidak duplikat. `TrxPatientProcedure` dipakai ulang |
| Resep | Tidak duplikat. `TrxPrescription` dipakai ulang |
| Kamar dan tempat tidur | Tidak duplikat. `MstRoom` dan `MstBed` dipakai ulang |
| Kelas pasien | Tidak duplikat. `MstPatientClass` dipakai ulang |
| Persetujuan | Tidak duplikat. `TrxPatientConsent` dipakai ulang |
| Perpindahan pasien | **Berpotensi mirip** dengan `TrxEmergencyTransfer`, tetapi tidak duplikat karena keduanya menempel pada episode yang berbeda. Yang dipakai ulang adalah polanya |
| Pengaturan parameter | **Berpotensi mirip** dengan `MstEmergencySetting`, tetapi tidak duplikat karena isinya berbeda. Yang dipakai ulang adalah polanya |
| Riwayat status | **Berpotensi mirip** dengan `TrxWorkflowStatusHistory`, tetapi tidak duplikat karena tabel itu milik Workflow HR. Yang dipakai ulang adalah polanya |

Tidak ditemukan satu pun rencana tabel baru pada dokumen keputusan yang benar-benar menduplikasi
tabel yang sudah ada.

---

## 15. Impact scan terfokus — Dokter Rawat Inap — 2 September 2026

### 15.1 Identitas, input, dan batas audit

Bagian ini **menggantikan temuan lama hanya untuk** `CAP-015`, `CAP-020` s.d. `CAP-025`,
`INT-DOK-01`, `INT-DOK-02`, serta consumer frontend Dokter Rawat Inap. Baris lain pada bagian 1–14
belum dipindai ulang terhadap SHA terbaru dan tetap dianggap historis/stale.

| Field | Nilai |
|---|---|
| Blueprint | `RWI-BP-001`, revision `5`, bentuk `COMPOSITE` |
| Sub-modul | `dokter-rawat-inap`, status `draft`, belum di-approve |
| Keputusan yang mengikat | `RWI-DEC-038`, `RWI-DEC-062`, `RWI-DEC-070`, `RWI-DEC-080` s.d. `RWI-DEC-083`; decision log revision `7`, SHA-256 `e9f2c957dfc68d609c426d7c91018f01223b4d163c85498be525804387724d9c` |
| Manifest sub-modul | SHA-256 `0c9cc8a0d87c156fd302695466c0d805887968501c96c946c40a0beec332d003` |
| Kontrak target | API dan integration `0.1.0`, keduanya `draft`; SHA-256 API `6096aa168580c8c2f9e3f6b39ad17b4e885c78f1dd602b9695a725284574974e`; integration `a9d9d8ab8e2395c950f01aa3d82e9e178c925e88779c89afa1e3c9f217bc3553` |
| Acceptance target | `0.1.0`, `draft`, SHA-256 `bb60db8796a905568b3e14713c60d6fde3c1b3ff04278881e5581e8d2d5c18e4` |
| Backend | `BE@93b3227` = `93b3227c431401d8f586dec4e1fb25fbf41766e3`, branch `MHamzah`, working tree bersih saat snapshot dikunci |
| Frontend | `FE@863f24b` = `863f24b0d1617069310c04e5770b47fd1b518b5b`, branch `HamzahV2`; perubahan working tree pada empat berkas admisi tidak diaudit dan tidak diubah |
| Batas klaster | Episode/DPJP, konsultasi/SOAP, kajian medis, CPPT, resep, tindakan, visite, lab/radiologi, permission, consumer frontend, dan test terkait |
| Cara audit | Pembacaan statis model, `DbSet`, migration, controller, service, DI, permission, route/menu/service/hook/component frontend, inventaris test, dan test terarah |
| Batas tulis | Hanya dokumen capability map ini. Source backend dan frontend tidak diubah; database dan migration tidak dijalankan |

### 15.2 Ringkasan perubahan sejak baseline

1. Fondasi episode rawat inap, penugasan DPJP berperiode, census, discharge, dan permission Rawat
   Inap kini benar-benar ada. Ini membatalkan temuan baseline yang menyatakan episode/DPJP belum ada.
2. Modul Radiologi kini memiliki order, study, migration, permission, DI, lifecycle, dan penyaring
   `EncounterId`. Pernyataan blueprint dokter bahwa “modul radiologi belum ada” sudah stale.
3. Jalur klinis dokter belum menjadi rawat inap. Konsultasi dan pengkajian tanpa antrean hanya
   mengenali `EmgVisit`; satu konsultasi per encounter dan satu resep aktif per konsultasi masih
   dipaksakan.
4. Route dan menu frontend Dokter Rawat Inap kini ter-commit, tetapi consumer-nya memakai hook,
   service, status, aksi panggil, dan `queueId` milik antrean dokter rawat jalan. Ia tidak membaca
   episode, census, atau penugasan DPJP.
5. Tidak ditemukan `TrxPhysicianVisit`, endpoint visite, resource/action visite, test klinis rawat
   inap, test radiologi, atau test frontend Dokter Rawat Inap.

### 15.3 Capability evidence map

Setiap baris memakai tepat satu status dari kontrak audit. Status ini menilai **kemampuan target
Dokter Rawat Inap**, bukan sekadar keberadaan nama file.

| ID | Need | Owner | Evidence repo/path#symbol@SHA | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `DOK-TRC-CTX-01` | Episode aktif, encounter, pasien, lokasi, dan DPJP sebagai konteks klinis | `InPatientManagement` | `BE@93b3227 Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs#InpEpisode`; `.../Models/InpDoctorAssignment.cs#InpDoctorAssignment`; `.../Controllers/InpatientCensusController.cs#Get`; `.../Services/InpCensusQueryService.cs#BuildFilteredQuery`; `Migrations/20260824095353_CreateInpatientTransactionTables.cs#Up`; `Program.cs#InpEpisodeService`; test `InpEpisodeOpenAdmissionTests` dan `InpDoctorAndNurseAssignmentTests` | `Ready to reuse` | Census sudah dapat disaring `DoctorId`; detail episode dan riwayat DPJP tersedia. Consumer klinis tetap harus memakai kontrak ini | Rendah pada fondasi; tinggi bila dilewati consumer |
| `DOK-TRC-INT-01` | Shared inpatient clinical context resolver untuk konsultasi dan pengkajian tanpa antrean | `ClinicalManagement` | `BE@93b3227 Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs#ValidateRequestAsync`; `.../PatientAssessmentController.cs#ValidateRequestAsync`; pencarian `InpEpisode`/`EpisodeId` pada kedua controller tidak menemukan cabang rawat inap | `Missing` | Tambahkan resolusi episode aktif, kecocokan patient/encounter, dan kewenangan DPJP tanpa membuat antrean semu | Sangat tinggi; menahan `CAP-020`, `CAP-022`, lalu resep dan tindakan |
| `DOK-TRC-DEF-01` | Jalur konsultasi tanpa antrean tidak boleh meledak saat `queue == null` | `ClinicalManagement` | `BE@93b3227 .../DoctorConsultationController.cs#Create` mengambil queue nullable pada sekitar baris 258–265 tetapi selalu menulis `queue.QueueStatus` dan waktu konsultasi pada baris 361–385 | `Repair` | Lindungi seluruh mutasi queue pada cabang tanpa antrean dan tambah test regresi IGD serta rawat inap | Sangat tinggi; inferensi statis menunjukkan `NullReferenceException` pada jalur no-queue yang dinyatakan valid |
| `DOK-TRC-INT-02` | Banyak konsultasi sepanjang episode dan banyak resep aktif sesuai `RWI-RULE-026` | `ClinicalManagement` + `PharmacyManagement` | `BE@93b3227 .../DoctorConsultationController.cs#ValidateRequestAsync` menolak konsultasi kedua per `EncounterId` sekitar baris 844–850 dan 916–923; `.../PharmacyManagement/Controllers/PrescriptionController.cs#ValidateCreateRequestAsync` menolak resep aktif kedua pada baris 555–563 | `Extend` | Scope pelonggaran hanya `Inpatient` dan `Emergency`; alur Outpatient/MCU harus tetap sama | Sangat tinggi; menahan dokumentasi dan order berulang selama rawat inap |
| `DOK-TRC-CAP015` | Order dan pembacaan hasil laboratorium/radiologi per episode | `LaboratoryManagement` + `RadiologyManagement` | `BE@93b3227 .../LaboratoryManagement/Models/LabOrder.cs#LabOrder`; `.../LabOrderController.cs#GetList` tanpa filter encounter; `.../RadiologyManagement/Models/RadOrder.cs#RadOrder`; `.../Models/RadStudy.cs#RadStudy`; `.../Controllers/RadOrderController.cs#GetList`; `Migrations/20260828093000_AddRadiologyManagement.cs#Up`; `Program.cs#RadOrderService` | `Extend` | Keduanya punya jangkar `EncounterId`, tetapi belum punya pembuktian kepemilikan episode A/B; daftar lab tidak dapat difilter encounter; hasil final terverifikasi untuk dibaca workspace belum ditemukan. Radiologi perlu diserap ulang ke kontrak target | Tinggi; risiko data lintas episode dan blueprint menganggap Radiologi tidak ada |
| `DOK-TRC-CAP020` | SOAP dokter rawat inap dengan waktu klinis dan amendment | `ClinicalManagement` + `MedicalRecordManagement` | `BE@93b3227 .../DoctorConsultationController.cs#SaveSoap`; `...#Create`; `.../MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs#Create`; `.../ClinicalDocumentIntegrityController.cs#Sign` | `Repair` | SOAP dan infrastruktur integritas/addendum dapat dipakai, tetapi create rawat inap ditolak dan cabang no-queue cacat; episode tertutup dan otoritas DPJP belum dijaga | Sangat tinggi |
| `DOK-TRC-CAP021` | CPPT lintas profesi, waktu klinis, verifikasi DPJP, overdue, dan amendment | `ClinicalManagement` + `MedicalRecordManagement` | `BE@93b3227 .../Models/TrxPatientIntegratedProgressNote.cs#TrxPatientIntegratedProgressNote` memiliki encounter, profession, provider, dan SOAP summary; `.../PatientIntegratedProgressNoteController.cs#RegisterIntegrityAsync`; tidak ada `VerifiedBy`, status verifikasi, atau endpoint verifikasi pada model/controller | `Extend` | Reuse CPPT dan integrity/addendum; tambahkan konteks episode, penjaga DPJP, lifecycle verifikasi, waktu verifikasi, dan query overdue | Tinggi; catatan bisa lahir tanpa pembuktian episode dan tidak dapat memenuhi verifikasi DPJP |
| `DOK-TRC-CAP022` | Kajian medis awal | `ClinicalManagement` | `BE@93b3227 .../PatientAssessmentController.cs#Create`, `#GetActiveByEncounter`, `#ValidateRequestAsync`; model/DTO assessment sudah menyimpan isi kajian, tetapi cabang tanpa antrean hanya memeriksa `EmgVisit` | `Reuse with adapter` | Blueprint memilih reuse `TrxPatientAssessment`; perlukan resolver episode, discriminator/aturan kajian medis, SLA, dan otoritas. Keputusan struktur tetap sebaiknya ditutup sebelum approval | Tinggi |
| `DOK-TRC-CAP023` | Resep rutin/harian dan obat pulang, lebih dari satu selama episode | `PharmacyManagement` | `BE@93b3227 .../Models/TrxPrescription.cs#TrxPrescription` memiliki encounter, consultation, status pembayaran/pemenuhan; `.../PrescriptionController.cs#Create`, `#GetList`; tidak ditemukan `PrescriptionOrderType`/`Discharge`; gate resep aktif kedua masih ada | `Extend` | Tambah jenis resep, pelonggaran scoped, idempotency, konteks episode, dan consumer status pemenuhan read-only | Sangat tinggi; resep kedua dan obat pulang belum dapat direpresentasikan |
| `DOK-TRC-CAP024` | Tindakan dokter planned/performed dengan handoff billing yang aman | `ClinicalManagement` | `BE@93b3227 .../Models/TrxPatientProcedure.cs#TrxPatientProcedure` memiliki encounter, consultation, `IsExecuted`, `ExecutedAt`, `PerformedAt`; `.../PatientProcedureController.cs#Execute` menyimpan klinis lalu mengirim `ClinicalMilestoneFact` | `Extend` | Fondasi kuat, tetapi masih wajib consultation, belum terikat episode/visite, belum ada idempotency target, otoritas DPJP, atau test rawat inap | Tinggi |
| `DOK-TRC-CAP025` | Pencatatan visite sebagai event mandiri | `ClinicalManagement` | Pencarian `TrxPhysicianVisit` dan `PhysicianVisit` pada `Areas`, `Migrations`, `Repositories`, serta test di `BE@93b3227` menghasilkan `NO_MATCH` | `Missing` | Buat kemampuan milik ClinicalManagement sesuai kontrak target; jangan menurunkan visite dari SOAP/CPPT | Tinggi; satu dari enam kemampuan MUST belum memiliki persistence, API, permission, consumer, atau test |
| `DOK-TRC-FE-BASE` | Komponen shell klinis reusable | Frontend shared UI | `FE@863f24b src/components/ui/doctor-clinical-base/#index`; commit asal `415f4ad`; komponen page header, summary, patient card, context, tab, table, panel, badge, dan empty state tersedia | `Reuse with adapter` | Dapat dipakai setelah sumber data diganti ke episode/census dan state klinis target; tidak ditemukan test komponen | Sedang |
| `DOK-TRC-FE-01` | Workspace Dokter Rawat Inap menampilkan pasien episode aktif milik DPJP | Frontend Inpatient + Clinical consumer | `FE@863f24b src/.../doctor-inpatient/doctor-inpatient-view.jsx#DoctorInpatientView` mengimpor `useDoctorQueue`, `useDoctorQueueBoard`, `useDoctorConsultationWorkspace`, tab rawat jalan, serta aksi panggil/skip/no-show; `.../use-doctor-queue.js#buildInitialFilters` meminta antrean tanggal hari ini; `menu-items.jsx#healthServicesDoctorQueueInpatient` membuat menu tingkat dua | `Conflict` | Ganti sumber daftar dengan census/episode `DoctorId`, hilangkan semantik antrean, dan patuhi keputusan blueprint bahwa layar dokter menjadi anak konteks episode—bukan antrean rawat jalan yang diberi label rawat inap | Sangat tinggi; secara inferensi dapat menampilkan pasien rawat jalan sebagai “pasien rawat inap” dan mengirim aksi antrean yang salah |
| `DOK-TRC-AUTH-01` | Permission resource/action dan penjaga DPJP per episode | Platform + pemilik capability | `BE@93b3227` controller existing memakai `[Authorize]` dan `[AccessPermission]`; `InpEpisodeService.IsActiveDoctorAsync` tersedia; tidak ditemukan resource/action `PhysicianVisit` atau penggunaan episode/DPJP pada controller klinis dokter | `Extend` | Reuse permission engine dan penjaga DPJP; sambungkan pada setiap command/query klinis serta tambahkan resource/action visite | Sangat tinggi; permission endpoint generik belum membuktikan dokter berwenang atas pasien tertentu |
| `DOK-TRC-VER-01` | Bukti otomatis jalur dokter rawat inap | Backend + Frontend | `BE@93b3227`: test episode/DPJP, DI, dan lab ada; tidak ditemukan test konsultasi/assessment/CPPT/procedure/prescription/radiology rawat inap. `FE@863f24b`: tidak ditemukan test `doctor-inpatient` atau `doctor-clinical-base` | `Missing` | Tambahkan integration, authorization, regression Outpatient/IGD, concurrency/idempotency PostgreSQL, dan frontend state tests sesuai acceptance matrix | Sangat tinggi; UI dan kontrak klinis belum mempunyai jaring pengaman |

### 15.4 Kontrak dan perjalanan as-is yang benar-benar ditelusuri

| Perjalanan | Jejak as-is | Hasil |
|---|---|---|
| Membuka daftar pasien DPJP | Backend menyediakan `GET /api/v1/health-services/inpatient-management/census?doctorId=...`; frontend dokter justru memanggil `GET /v1/health-services/registration-management/doctor-queues` dengan tanggal hari ini | `Conflict` — sumber data yang benar ada tetapi tidak dipakai |
| Membuka/melanjutkan SOAP | Frontend memilih baris `queueId`, lalu workspace konsultasi mencari konsultasi aktif berdasarkan antrean; backend konsultasi rawat inap belum ada | `Repair` — tidak boleh dipakai untuk episode sebelum resolver dan null-queue defect selesai |
| Menulis CPPT | CPPT dapat dibuat dengan `EncounterId` dan punya integrity/addendum; belum ada verifikasi DPJP serta penjaga episode | `Extend` |
| Membuat resep kedua/obat pulang | Resep pertama memakai konsultasi; resep aktif kedua ditolak dan jenis obat pulang tidak ada | `Extend` |
| Mencatat tindakan | Tindakan planned/executed dan handoff billing tersedia, tetapi wajib consultation dan tidak membuktikan episode/DPJP | `Extend` |
| Memesan lab/radiologi | Order menempel pada encounter; radiologi dapat difilter encounter, lab list tidak. Pembacaan hasil final terverifikasi belum ditemukan | `Extend` |
| Mencatat visite | Tidak ada entity, migration, endpoint, permission, consumer, atau test | `Missing` |

### 15.5 Fakta, inferensi, dan rekomendasi

#### Fakta

- `InpEpisode`, DPJP berperiode, census dengan filter `DoctorId`, migration, DI, permission, dan test
  fondasinya tersedia.
- Validasi konsultasi/pengkajian tanpa antrean hanya mengakui encounter yang memiliki `EmgVisit`.
- Konsultasi kedua per encounter dan resep aktif kedua per konsultasi masih ditolak.
- Radiologi kini ada di source; seluruh pernyataan target yang mengatakan sebaliknya sudah stale.
- Frontend Dokter Rawat Inap ter-commit dan reachable dari menu, tetapi memakai kontrak antrean
  rawat jalan serta tidak membaca `InpEpisode`/census.

#### Inferensi audit

- Mutasi `queue` tanpa pemeriksaan null pada jalur no-queue sangat mungkin menghasilkan HTTP 500.
  Ini inferensi dari kontrol alur statis; runtime API tidak dijalankan.
- Workspace frontend saat ini berisiko memberi label “Rawat Inap” pada data antrean rawat jalan
  dan mengeksekusi call/skip/no-show terhadap antrean tersebut. Ini inferensi dari import, filter,
  dan route service; browser tidak dijalankan.
- `EncounterId` saja belum cukup membuktikan dokumen/order milik episode yang sedang dibuka bila
  kontrak target secara eksplisit menguji episode A versus episode B.

#### Rekomendasi berurutan

1. **Jangan sign-off atau rilis workspace frontend saat ini.** Ubah entry point menjadi census/
   episode berfilter dokter aktif sebelum menghubungkan tab klinis.
2. Kerjakan `INT-DOK-01` bersama `INT-KEP-01`, sekaligus repair null-queue pada konsultasi dan
   regression test IGD.
3. Kerjakan `INT-DOK-02` dengan pembatas eksplisit `Inpatient`/`Emergency`; pertahankan perilaku
   Outpatient/MCU lewat regression test.
4. Setelah resolver stabil, lanjutkan extension CPPT, resep, tindakan, lab/radiologi, lalu buat
   `PhysicianVisit`. Jangan membuat tabel dokumentasi `Inp*` tandingan.
5. Jalankan amendment desain untuk menyerap keberadaan Radiologi dan membetulkan kontrak,
   frontend architecture, PRD-to-MVP, serta acceptance matrix yang masih mengatakan modulnya tidak ada.

### 15.6 Bukti verifikasi dan keterbatasan

- Perintah terarah terhadap binary Debug yang sudah terbangun:
  `dotnet test ... --no-build --no-restore --filter ...` menghasilkan **26 passed, 0 failed,
  0 skipped** dalam 20 detik. Cakupannya: `InpDoctorAndNurseAssignmentTests`,
  `InpEpisodeOpenAdmissionTests`, `InpatientServiceRegistrationTests`, dan
  `LabOrderDisciplineTests`.
- Percobaan build-test pertama dihentikan tanpa hasil karena beberapa proses .NET lain sedang
  berjalan bersamaan. Karena run sukses memakai `--no-build`, audit ini **tidak mengklaim build
  bersih dari source pada snapshot ini**.
- Database, migration, API runtime, browser, dan environment role assignment tidak dijalankan.
- Tidak diketahui apakah migration terbaru sudah diterapkan pada environment target, apakah role
  non-SuperAdmin sudah menerima permission, dan apakah data master/episode nyata tersedia. Ketiganya
  tetap `Unknown` sampai verifikasi environment dilakukan.
- Tidak ada source aplikasi yang diubah oleh audit ini.

### 15.7 Pertanyaan penutup dan handoff

| ID | Pertanyaan/keputusan yang masih perlu ditutup | Sifat |
|---|---|---|
| `RWI-DOK-TRQ-001` | Apakah pilihan reuse `TrxPatientAssessment` untuk kajian medis disetujui sebelum blueprint naik dari `draft`? | Keputusan struktur; tidak menghalangi fakta audit, tetapi dapat mengubah desain dan roadmap |
| `RWI-DOK-TRQ-002` | Siapa pemilik Clinical Governance untuk menetapkan nilai SLA kajian medis dan verifikasi CPPT? | Keputusan organisasi/klinis; mekanisme dapat dibangun dengan policy kosong |
| `RWI-DOK-TRQ-003` | Apakah commit frontend `b15d211bb` akan dirework langsung atau dikarantina dari rilis sampai kontrak episode tersedia? | Keputusan delivery; rekomendasi audit adalah jangan dirilis dalam bentuk sekarang |

Handoff berikutnya adalah kembali ke `manage-module-blueprint` untuk menandai artefak dokter yang
terdampak sebagai stale dan menjadwalkan amendment. Setelah target dan kontrak diperbarui serta
disetujui, barulah `plan-module-delivery` boleh memecah pekerjaan. Audit ini sendiri **tidak** memberi
wewenang implementasi.

### 15.8 Staleness impact scan ini

Bagian 15 terikat pada `BE@93b3227` dan `FE@863f24b`. Perubahan setelah salah satu SHA tersebut
membuat bagian ini stale. Impact scan berikutnya minimal wajib memeriksa:

- `DoctorConsultationController`, `PatientAssessmentController`, CPPT, procedure, prescription,
  LabOrder, Radiology, `InpEpisode`/census, dan seluruh test terkait di backend;
- route/menu Dokter Rawat Inap, `useDoctorQueue`, `useDoctorConsultationWorkspace`, service klinis,
  komponen `doctor-clinical-base`, serta test terkait di frontend;
- perubahan kontrak `dokter-rawat-inap` atau keputusan `RWI-DEC-038`, `062`, `070`, `080`–`083`.

---

## 16. Impact scan terfokus — Rawat Inap V2 — 11 September 2026

### 16.1 Batas audit

| Atribut | Nilai |
|---|---|
| Pemicu | `RWI-DEC-097` — `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` diterima sebagai masukan hulu |
| `blueprint_id` / `revision` | `RWI-BP-001` / `5` |
| `backend_source_sha` | `201de7535d4ca00fa9ede2395d4fb769f023b9e6`, branch `MHamzah`, 10 September 2026 |
| `frontend_source_sha` | `7f6b9356f6349d516570d6d603ca026f2c7f4ec2`, branch `HamzahV2`, 10 September 2026 |
| `contract_versions` saat audit | `episode-rawat-inap` `0.6.1`, `keperawatan` `0.3.0`, `dokter-rawat-inap` `0.4.0` |
| Cakupan | Empat penyimpangan `P0` PRD V2, ditambah pemeriksaan keberadaan sembilan kemampuan baru |
| Yang **tidak** dinilai | Kinerja, migration yang sudah terpasang di environment, dan hasil test. Test **tidak dijalankan** |
| Sifat | **Read-only** terhadap source kedua repository. Nol baris source disentuh |

Kedua SHA di atas sama persis dengan snapshot audit PRD V2, sehingga temuan dokumen itu dapat
diperiksa ulang baris demi baris. Hasilnya: **keempat temuan `P0` terbukti**, tetapi **tiga di
antaranya lebih luas** daripada yang tertulis, dan **dua kemampuan yang dikira belum ada ternyata
sudah tersedia sebagian**.

### 16.2 Capability evidence map

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `V2-CAP-01` | Kelayakan bed tidak dipengaruhi jenis kelamin penghuni kamar lain | `InPatientManagement` | `InpBedOccupancyService.cs#EvaluatePlacementEligibilityAsync@201de753` baris 1359 s.d. 1410 | **Conflict** | Aturan 6 `ROOM_GENDER_MIXED` baris 1386 s.d. 1397 **wajib dihapus**. Aturan 5 `PATIENT_GENDER_UNKNOWN` baris 1401 memuat klausa `countedOccupants.Count > 0` yang **juga** membaca penghuni, sehingga ikut dicabut. `LoadRoomOccupantsAsync` baris 1639 menjadi kode mati bila keduanya lepas | Aturan 4 `BED_GENDER_MISMATCH`, aturan 7 `ISOLATION_REQUIRED`, dan aturan 8 `ISOLATION_BED_RESERVED` **tidak boleh ikut tercabut** |
| `V2-CAP-02` | Catatan klinis final tidak dapat disembunyikan | `ClinicalManagement` dan `MedicalRecordManagement` | `ClinicalDocumentIntegrityService.cs#JenisYangDitegakkan@201de753` baris 75 s.d. 81; `PatientIntegratedProgressNoteController.cs@201de753` baris 586 dan 865 | **Repair** | Mesin keutuhan **sudah ada dan sudah terpasang pada controller yang sama**, tetapi `EnsureMutableAsync` hanya dipanggil pada `Update`, **tidak** pada `Delete`. Hanya **4 dari 13** `ClinicalDocumentKind` ditegakkan | **Sepuluh** controller punya `HttpDelete` tanpa satu pun state guard, bukan dua seperti tertulis pada PRD |
| `V2-CAP-03` | Penulis klinis berasal dari identitas terautentikasi | `ClinicalManagement` | `InpatientClinicalContextService.cs#ResolveAsync@201de753` baris 304 s.d. 319; `DoctorConsultationController.cs@201de753` baris 452 | **Repair** | `isDoctorAuthorized` bernilai `true` secara bawaan dan hanya diuji bila `doctorId` dikirim. Dari sembilan titik panggil, **hanya `PhysicianVisitController` baris 298** yang mengirimnya | `ApplicationUser.DoctorId` **sudah ada**, tetapi dipakai **satu** service saja di seluruh repository, yaitu `BillingDiscountService` |
| `V2-CAP-04` | Kelayakan keuangan berasal dari fakta Billing | `BillingManagement` dan `InPatientManagement` | `InpFinancialClearance.cs@201de753`; `InpDischargeService.Closure.cs@201de753` baris 277 s.d. 312 dan 641 s.d. 687 | **Extend** | Closure **nol** membaca Billing. `IsManualMarking` bernilai `true` bawaan dan `ClearanceStatus` hanya bertiga nilai. Butuh read model ringkasan finansial per episode | Keterangan baris 284 yang menyatakan Billing belum punya kemampuan transaksi **sudah basi**; enam belas service Billing tersedia, termasuk settlement, refund, dan finalisasi |
| `V2-CAP-05` | Bukti consent tersimpan per episode | `ClinicalManagement` | `TrxPatientConsent.cs@201de753` baris 28, 200 s.d. 212, 261 s.d. 268 | **Extend** | **Lebih lengkap dari dugaan PRD.** `ConsentFileHash`, `ConsentFilePath`, `ConsentFileName`, `ConsentFileSizeBytes`, `SignerRelationship`, `ConsentMethod`, dan riwayat pencabutan **sudah ada**. Yang kurang: versi template, dan penegakan keutuhan karena `Consent` termasuk sembilan jenis yang belum ditegakkan | `PatientConsentController` baris 822 dapat menghapus consent tanpa guard apa pun |
| `V2-CAP-06` | Lima cara keluar | `InPatientManagement` | `InpDischargeType.cs@201de753` | **Extend** | Hanya **tiga** nilai tersedia, yaitu `DoctorApproved`, `AgainstMedicalAdvice`, dan `Referred`. `Death` dan `Absconded` belum ada | Menunggu `MVP-RWI-D-011`, yang **tidak** dibuka `RWI-DEC-097` |
| `V2-CAP-07` | Selector episode ibu operasional | `InPatientManagement` dan frontend | `InpEpisode.cs@201de753` baris 44 dan 86; `use-inpatient-admission-flow.jsx@7f6b9356` baris 23, 257, 266 | **Repair** | Backend `MotherEpisodeId` **sudah ada**. Frontend menyimpan `motherEpisodeId` sebagai state teks dan **mengunci langkah** dengannya, tetapi **nol** endpoint pencarian episode ibu dan **nol** komponen selector | Langkah admisi bayi baru lahir tidak dapat diselesaikan pengguna hari ini |
| `V2-CAP-08` | Medication Administration Record | `ClinicalManagement` dan `PharmacyManagement` | Penelusuran `MedicationAdministration`, `MedicationDose`, `DoseEvent` pada seluruh `Areas/**` `@201de753` | **Missing** | Nol model, nol service, nol endpoint | `RWI-DEC-097` membalikkan statusnya dari luar-MVP menjadi `P0` dan `P1`; belum ada satu pun fondasi |
| `V2-CAP-09` | Observasi transfusi dan reaksi | `ClinicalManagement` | Penelusuran `Transfusion` `@201de753` | **Missing** | Yang ada hanya `PatientConsentType` dan `MstBloodBankReason`, keduanya bukan workflow | — |
| `V2-CAP-10` | Sliding scale | `ClinicalManagement` | Penelusuran `SlidingScale` `@201de753` | **Missing** | Nol jejak | Menuntut protokol berversi yang juga belum ada |
| `V2-CAP-11` | Intake/output, drain, dan WSD | `ClinicalManagement` | `EmgObservationDetail.cs@201de753` | **Reuse with adapter** | Tidak ada di Rawat Inap, tetapi **pola yang sama sudah berjalan di IGD** lewat `EmergencyObservationDetailController`. Layak menjadi rujukan bentuk, bukan disalin | Menyalin tabel IGD ke Rawat Inap melanggar `RWI-DEC-081` |
| `V2-CAP-12` | Handover antar shift keperawatan | `ClinicalManagement` | `BilCashierShiftHandover.cs@201de753` | **Missing** | Serah terima shift **hanya ada untuk kasir**, bukan klinis. Polanya dapat menjadi rujukan | — |
| `V2-CAP-13` | Clinical handover transfer antarunit | `InPatientManagement` dan `ClinicalManagement` | Penelusuran `ClinicalHandover` dan `TransferHandover` `@201de753` | **Missing** | Transfer bed atomik sudah ada; artefak serah terima klinisnya belum | — |
| `V2-CAP-14` | Adverse drug reaction dari MAR | `ClinicalManagement` | Penelusuran `AdverseDrugReaction` `@201de753` | **Missing** | Bergantung pada `V2-CAP-08` | — |

### 16.3 Fakta, inferensi, dan rekomendasi

**Fakta.** Seluruh baris pada bagian 16.2 dibaca langsung dari source pada kedua SHA yang
tercatat. Tidak satu pun berasal dari ingatan sesi sebelumnya atau dari klaim dokumen.

**Inferensi.** `LoadRoomOccupantsAsync` menjadi kode mati setelah aturan 5 dan 6 dicabut. Ini
kesimpulan dari pembacaan pemakaian, bukan fakta yang diuji compiler; pemeriksaannya diserahkan
ke task implementasi.

**Rekomendasi.** Tiga koreksi `P0` berstatus `Repair` dan satu berstatus `Conflict`. Keempatnya
dapat dikerjakan **tanpa keputusan bisnis baru**, karena mesinnya sudah ada dan `RWI-DEC-097`
sudah membuka arah perbaikannya. Sembilan kemampuan `Missing` dan `Extend` **tidak** boleh masuk
gelombang yang sama.

### 16.4 Ketidakcocokan frontend dan backend

| Temuan | Backend `@201de753` | Frontend `@7f6b9356` |
|---|---|---|
| Kode `ROOM_GENDER_MIXED` | Diterbitkan `InpBedOccupancyService` aturan 6 | Dipetakan `inpatient-placement-utils.jsx` baris 12, dan dikunci **tiga** berkas test, yaitu `inpatient-placement.test.mjs` baris 57, 82, dan 129, serta `inpatient-episode-detail.spec.mjs` baris 605 |
| Selector episode ibu | `MotherEpisodeId` tersedia | State ada, pencarian dan komponen selector **tidak ada** |

Mencabut aturan 6 di backend **akan** menggagalkan test frontend yang mengunci kodenya. Keduanya
wajib berada pada satu gelombang, dan itu menjadikan koreksi ini pekerjaan lintas repository.

### 16.5 Unknown dan pertanyaan penutup

| ID | Pertanyaan | Kenapa tidak dapat dijawab source |
|---|---|---|
| `V2-UNK-01` | Apakah sembilan `ClinicalDocumentKind` yang belum ditegakkan memang disengaja, dan mana yang wajib naik untuk MVP V2? | Keterangan source menyebut keadaan itu wajib dinyatakan terbuka di layar lewat `RM-FE-009`, tetapi tidak menyebut rencana kenaikannya. Pemilik `MedicalRecordManagement` |
| `V2-UNK-02` | Bentuk ringkasan finansial episode yang disediakan Billing, dan apakah dibaca langsung atau lewat snapshot bertanda `calculatedAt` | `OPEN-MVP-006`, pemilik `BillingManagement` |
| `V2-UNK-03` | Apakah sepuluh jalur `HttpDelete` ditutup seluruhnya, atau hanya yang menyentuh dokumen klinis final | Keputusan pemilik `ClinicalManagement`; berdampak ke Rawat Jalan |

### 16.6 Staleness dan pemicu impact scan

Peta bagian 16 ini menjadi `STALE` ketika salah satu berikut berubah:

- `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`;
- `Areas/HealthServices/ClinicalManagement/Controllers/` mana pun yang memuat `HttpDelete`;
- `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs`;
- `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs`;
- `Areas/HealthServices/InPatientManagement/Enums/InpDischargeType.cs`;
- `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` pada frontend;
- keputusan `RWI-DEC-097`, `RWI-OQ-047`, atau `RWI-OQ-054`.

---

## 17. Impact scan terfokus — Penyelarasan `PRD-RWI-V2-001`, fase `RLN-PH-03` — 15 September 2026

### 17.1 Batas audit

| Atribut | Nilai |
|---|---|
| Pemicu | Fase `RLN-PH-03` pada `blueprint-manifest.md` bagian 0-B.4, ditambah tiga pemeriksaan titipan Amendment Pass `PRD-RWI-V2-001` yang tuntas 15 September 2026 |
| `blueprint_id` / `revision` | `RWI-BP-001` / `6` |
| Masukan hulu | `PRD-RWI-V2-001` v`2.0`, `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md`, SHA-256 `2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f` — **diperiksa ulang, tidak berubah** |
| Decision log | `00-interview-decisions.md` revision `19`, SHA-256 `c5c5105cb0fc2585edc5168916d613918a30820ee5cd59c1e62c9ed47c565696`, keputusan terakhir `RWI-DEC-137` |
| `backend_source_sha` | `df3679c0d5b2f08106702153eb242d3a6cb2929b`, branch `MHamzah`, 14 September 2026 |
| `frontend_source_sha` | `147355f505e875148b8416866ada6cf8b2f1ad99`, branch `HamzahV2`, 12 September 2026, working tree bersih |
| Pembanding V1 | Frontend `QuilvianSystemFrontendDev@13c3a96b`, commit lokal. Backend V1 `QuilvianSystemBackendDev@QuilvianSta` **tidak tersedia** di workspace |
| `contract_versions` saat audit | `episode-rawat-inap` `0.8.0`, `dokter-rawat-inap` `0.5.0`, `keperawatan` `0.4.0`, sesuai `RWI-DEC-105` |
| Cakupan | (a) selisih SHA, `RLN-14`; (b) kesamaan layout dokter dengan Dokter Rawat Jalan, `RLN-10`; (c) tujuh isi Pengkajian Pasien dan enam sub-isi Asuhan Keperawatan V1 terhadap V2, bagian V1 dari `RLN-07`; (d) tujuh belas butir MVP-0 PRD bagian 56, `RLN-09`; (e) jalur ubah dan penyelesaian kajian medis, tindakan dokter, dan catatan terpadu, titipan `RWI-DEC-128`; (f) jalur pakai template resep terhadap `RWI-DEC-122` butir (3) s.d. (6), titipan `RWI-DEC-135` |
| Yang **tidak** dinilai | Kinerja, data di database, migration yang terpasang, dan perilaku saat aplikasi berjalan. Aplikasi, build, dan test **tidak dijalankan** |
| Sifat | **Read-only** terhadap source kedua repository. Satu-satunya berkas yang ditulis adalah dokumen ini |

**Singkatan bukti bagian ini:** `BE@df3679c0` untuk backend, `FE@147355f5` untuk frontend V2, dan
`FE-V1@13c3a96b` untuk frontend V1. Semua path backend relatif terhadap `Areas/HealthServices/`
kecuali disebut lain.

### 17.2 Ringkasan untuk pembaca umum

Audit ini menjawab satu pertanyaan: **seberapa jauh sistem V2 hari ini sudah memenuhi ruang kerja
dokter dan keperawatan versi PRD baru, dan di mana jalurnya justru bertentangan dengan keputusan
yang sudah disetujui.**

Tiga temuan paling penting:

1. **Layar pengkajian keperawatan V2 dapat mencatat risiko klinis secara terbalik.** Contoh:
   perawat memilih "Risiko Tinggi (Skor ≥ 45 Morse)", mengetik skor 55, dan tidak mencentang
   ataksia maupun instabilitas postur. Backend menyimpan **tidak berisiko** dengan skor kosong, lalu
   saat dibuka ulang layar menampilkan **"Risiko Rendah"**. Penyebabnya tiga sekaligus: angka enum di
   frontend bergeser satu dari backend, layar tidak punya kontrol `hasFallRisk`, dan backend
   menghitung ulang kategori dari dua centang saja. Pola geser satu yang sama terjadi pada status
   gizi dan nafsu makan, sedangkan status fungsional mengirim angka yang tidak dikenal backend.
   Rinciannya pada 17.4.
2. **Jalur ubah dan penyelesaian catatan dokter belum dijaga penulis maupun penugasan** untuk SOAP,
   kajian medis, dan konsep catatan terpadu. Tindakan dokter bahkan menerima pembuatan baru pada
   episode yang sudah ditutup. Verifikasi CPPT menerima dokter dengan **peran apa pun**, padahal
   `RWI-DEC-125` hanya mengizinkan DPJP.
3. **Ruang kerja dokter rawat inap V2 tersusun dua halaman**, yaitu daftar pasien lalu pindah ke
   halaman ruang kerja, memakai pustaka komponen yang **berbeda** dari Dokter Rawat Jalan. PRD
   menuntut susunan satu halaman terbelah yang identik dengan Rawat Jalan.

Satu temuan **mengoreksi** pencatatan pass sebelumnya: daftar "catatan saya yang belum
ditandatangani" **sudah ada** di modul Rekam Medis, sehingga `RWI-FACT-028` butir (2) terlalu luas.
Lihat `RLN3-CAP-37`.

### 17.3 Capability evidence map

#### 17.3.1 Selisih SHA dan ketersediaan pembanding — `RLN-14`

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-01` | Bukti source penyelarasan V2 masih sama dengan snapshot handoff | Semua modul | `git rev-list --count 4f79e998..df3679c0` = **2** commit, keduanya hanya berkas `docs/` (`34f368d4`, `df3679c0`). `FE` `HEAD` = `147355f5`, sama dengan handoff, working tree bersih | **Ready to reuse** | Fakta `RWI-FACT-028` s.d. `RWI-FACT-036` dibaca pada SHA yang sama, sehingga berlaku utuh | Sejak scan V2 `201de753` ada 121 commit dan 85 berkas berubah di `ClinicalManagement`, `InPatientManagement`, `PharmacyManagement`, dan `MedicalRecordManagement`; baris 16.2 diperlakukan basi |
| `RLN3-CAP-02` | Pembanding backend V1 | — | `QuilvianSystemBackendDev@QuilvianSta` tidak ada di workspace | **Unknown** | Isi V1 hanya dapat dibuktikan dari frontend `FE-V1@13c3a96b` | Klaim PRD tentang isi V1 yang tidak tampak di frontend V1 tidak dapat dibuktikan maupun dibantah |

#### 17.3.2 Kesamaan layout Dokter Rawat Inap dengan Dokter Rawat Jalan — `RLN-10`

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-03` | Kerangka halaman Dokter Rawat Jalan sebagai acuan layout | Frontend `registration-management` | `FE@147355f5 src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx#DoctorQueueView` baris 196–292: `topBar` + `SummaryBar`, `workspaceGrid` berisi `leftPanel` (daftar `QueuePatientCard`) dan `rightPanel` (`ConsultationTabs`, `DoctorPatientContext`, isi, `FinalizeConsultationPanel`). `src/style/health-services/registration-management/doctor-queues/doctor-queue-view.module.css` baris 169–174: `grid-template-columns: minmax(280px, 340px) minmax(0, 1fr)` | **Reuse with adapter** | Yang umum dan dapat dipakai langsung: kelas tata letak pada berkas CSS dan `EmptyState` (hanya `title`, `description`, `icon`). Yang **terikat data antrean**: `SummaryBar` (menghitung Total Antrean, Menunggu, Dipanggil lewat `buildDoctorQueueVisibleSummary`), `QueuePatientCard` (`QUEUE_STATUS`, `getQueueCode`), `ConsultationTabs` (daftar tab tetap `DOCTOR_QUEUE_TABS`), dan `DoctorPatientContext` (util antrean). Keempatnya butuh versi berbasis props atau adapter data episode | Komponen itu milik Rawat Jalan. Mengubahnya menjadi komponen bersama berada di luar scope blueprint ini dan butuh pemilik `rawat-jalan`; menyalinnya melahirkan dua salinan yang dapat menyimpang |
| `RLN3-CAP-04` | Permukaan Dokter Rawat Inap V2 hari ini | Frontend `inpatient-management` | `FE@147355f5 src/app/health-services/inpatient-management/doctor-inpatient/page.jsx` → `doctor-inpatient-view.jsx` baris 45–115: `ClinicalPageHeader`, `ClinicalSummaryBar`, filter, daftar berpaginasi yang **menautkan ke rute lain**. `src/app/health-services/inpatient-management/episodes/[id]/physician/page.jsx` → `physician-workspace-view.jsx` baris 125–211. Tab pada `src/lib/constants/health-services/inpatient-management/inpatient-physician-constants.jsx` baris 21–50 | **Conflict** | Susunan dua halaman, bukan satu halaman terbelah. Komponen dasar dari `src/components/ui/doctor-clinical-base`, yang **tidak dipakai** Rawat Jalan sama sekali. Tab yang ada: Kajian Medis, Catatan Perkembangan, Catatan Terpadu, Visite, **Resep & Tindakan dalam satu tab**, dan Penunjang yang tidak punya folder isi | Bertentangan dengan PRD bagian 8, 9, dan 61 (`UI-AC-DOK-001` s.d. `012`) yang diikuti `RWI-DEC-107`. Ruang kerja keperawatan memakai pustaka ketiga, `src/components/ui/clinical-workspace`, sehingga ada **tiga** keluarga komponen klinis paralel |

#### 17.3.3 Pengkajian Pasien dan Asuhan Keperawatan V1 terhadap V2 — bagian V1 dari `RLN-07`

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-05` | Kajian Umum | `ClinicalManagement` | V1: `FE-V1@13c3a96b src/components/view/Rawat-Inap/perawat/perawatan-pasien/pengkajian-pasien/form-kajian-umum/kajian-pasien-umum.jsx` memuat **7** bagian: Sumber Data Pasien, Pernapasan, Integritas Kulit, Skrining Nutrisi, Eliminasi, Ketergantungan, Status Fungsional. V2: `BE@df3679c0 ClinicalManagement/Models/TrxPatientAssessment.cs` baris 117–326, satu tabel datar; frontend `nursing-workspace/sections/assessment/groups/*` | **Extend** | V2 sudah punya keluhan, tanda vital, oksigen, kesadaran, skrining gizi MST, status fungsional, dan catatan psikososial/edukasi/perawat. V2 **belum** punya sumber data pasien, pernapasan rinci, integritas kulit, eliminasi, dan ketergantungan ADL terstruktur | **PRD bagian 28 tidak cocok dengan V1.** PRD menyebut sepuluh bagian V1 termasuk Psikososial, Alat Bantu, dan Catatan Relevan. Frontend V1 hanya punya tujuh; "Alat Bantu" hanya berupa isian di Eliminasi dan Ketergantungan; "Psikososial" dan "Catatan Relevan" tidak ditemukan |
| `RLN3-CAP-06` | Resiko Jatuh | `ClinicalManagement` | V1: `pengkajian-pasien/resiko-jatuh/{Anak-Anak,Dewasa,Lansia}` (`RWI-FACT-027`). V2: `FE@147355f5 .../groups/fall-risk-group.jsx` baris 13–15; `BE@df3679c0 ClinicalManagement/Controllers/PatientAssessmentController.cs#CalculateFallRiskScore` baris 2321 dan `#CalculateFallRiskStatus` baris 2363 | **Conflict** | Lihat `RLN3-CON-01`. Konfigurasi berversi `RWI-DEC-124` dan `RWI-DEC-136` belum ada sama sekali | **Keselamatan klinis:** pasien berisiko tinggi dapat tersimpan tidak berisiko |
| `RLN3-CAP-07` | Monitoring Nyeri sebagai rangkaian | `ClinicalManagement` | V1: `pengkajian-pasien/assement-nyeri/add-assesment-nyeri.jsx` — skor nyeri, skor sedasi, pola napas, tanda vital, intervensi farmakologi (obat dari resep atau master, dosis, rute, waktu), intervensi non-farmakologi, waktu kajian ulang, perawat monitoring dan intervensi, tanda tangan. V2: `TrxPatientAssessment` baris 222–242 | **Missing** | V2 hanya punya **satu** penilaian nyeri per dokumen pengkajian. Rangkaian monitoring beserta kajian ulang setelah intervensi tidak ada | — |
| `RLN3-CAP-08` | Assesment Edukasi | `ClinicalManagement` | V1: `pengkajian-pasien/assestmen-edukasi/add-assesment-edukasi.jsx` — kebutuhan, hambatan, bahasa, penerjemah, pendidikan, nilai kepercayaan, tipe pembelajaran, topik, metode, durasi, evaluasi Baik/Cukup/Kurang, nama dan tanda tangan wali, cetak. V2: `TrxPatientAssessment.EducationNote` baris 323 teks bebas; `education-group.jsx` dua isian teks | **Missing** | Hanya teks bebas | `MasterData/Models/MstDiagnosisEducationRecommendation.cs` dapat menjadi sumber topik, perlu dibuktikan saat desain |
| `RLN3-CAP-09` | Pengawasan Harian Pasien, termasuk intake, output, dan balance cairan | `ClinicalManagement` | V1: `pengkajian-pasien/pengawasan-harian/section/*` — tanda vital, kesadaran AVPU dan agitasi, nyeri dan non-farmakologi, intake infus/oral/NGT, output urin/feses/NGT/WL, gula darah, lingkar perut, asupan makanan, diet, mobilisasi, resep. V2: nol model; pola IGD `EmgObservationDetail` (`V2-CAP-11`) | **Missing** | — | `RWI-DEC-081` melarang menyalin tabel IGD |
| `RLN3-CAP-10` | Evaluasi Awal MPP | `ClinicalManagement` | V1: `pengkajian-pasien/evaluasi-awal/form/page.jsx` memanggil `createEvaluasiAwal` dengan isian dinamis dari master `/ChecklistItem`, plus halaman cetak. V2: nol model; nol penanda MPP (`RWI-FACT-031`) | **Missing** | Arah sudah diputuskan `RWI-DEC-118` dan `RWI-DEC-131` | — |
| `RLN3-CAP-11` | Perencanaan Pulang | `ClinicalManagement` | V1: `pengkajian-pasien/rencana-pulang/*` — formulir, riwayat, tanda tangan, cetak. V2: `PatientAssessmentType.DischargePlanning = 3`; `discharge-planning-group.jsx` hanya satu isian teks "Catatan Rencana Pemulangan (Discharge Planning) & Catatan Perawat" | **Extend** | Jenis dokumen ada, isi terstruktur tidak. `InpDischargeSummary` adalah resume pulang milik episode, **bukan** perencanaan pulang | `BR-RWI-016` |
| `RLN3-CAP-12` | Tanda vital keperawatan sebagai rangkaian | `ClinicalManagement` | `BE@df3679c0 ClinicalManagement/Models/TrxPatientVitalSign.cs` baris 26–38 tanpa `InpEpisodeId`; `PatientVitalSignController.cs` endpoint `active-by-encounter` dan `active-by-queue`; konsumen frontend hanya antrean dokter, nurse station, dan IGD | **Extend** | Polanya satu tanda vital aktif per kunjungan atau antrean, bukan rangkaian per episode | — |
| `RLN3-CAP-13` | SOAP Keperawatan dan Catatan Terintegrasi | `ClinicalManagement` | `RWI-DEC-115`; `BE@df3679c0 ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` penjaga unit perawat `EnsureNursingUnitAuthorityAsync` baris 91; frontend keperawatan hanya punya seksi Pengkajian, Rencana Asuhan, Tindakan Keperawatan, dan Lini Masa (`inpatient-nursing-constants.js` baris 15–34) | **Reuse with adapter** | Backend CPPT berprofesi Perawat sudah ada; ruang kerja keperawatan V2 belum punya bagian CPPT/SOAP | — |
| `RLN3-CAP-14` | Tindakan Harian keperawatan | `ClinicalManagement` | `NursingInterventionController.cs`, `CliNursingIntervention.cs`, seksi frontend Tindakan Keperawatan; kunci idempotency ada | **Ready to reuse** | — | — |
| `RLN3-CAP-15` | Obat & Alkes: MAR dan rekonsiliasi obat | `PharmacyManagement` | `V2-CAP-08`; `RWI-FACT-032` | **Missing** | Nol model | `P0` sesuai `RWI-DEC-116` dan `RWI-DEC-133` |
| `RLN3-CAP-16` | Catatan Keperawatan | `ClinicalManagement` | V1: `asuhan-keperawatan/catatan-keperawatan/*` berisi catatan pra/intra/pasca-operatif, diet medis, observasi cairan, observasi cairan WSD, pemberian obat, dan sliding scale. PRD bagian 41 hanya menggambar **catatan kartu per waktu dan perawat**. V2: `TrxPatientAssessment.NurseNote` baris 326 saja | **Unknown** | Belum jelas apakah Catatan Keperawatan PRD adalah CPPT berprofesi Perawat `RWI-DEC-115` atau dokumen tersendiri; isi V1-nya juga jauh lebih luas dari PRD | Lihat `RLN3-UNK-02` |

#### 17.3.4 Butir MVP-0 PRD bagian 56 — `RLN-09`

| ID | Butir | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-17` | Keperawatan — assessment pagination | FE + `ClinicalManagement` | `BE@df3679c0 PatientAssessmentController.cs#GetByEpisode` baris 350–407 menjawab objek berpaginasi `{ pageNumber, pageSize, totalData, totalPage, items }`. `FE@147355f5 .../nursing-workspace/sections/assessment/assessment-section.jsx` baris 229–233 dan `src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-workspace.jsx` baris 238–245 memakai `Array.isArray(payload) ? payload : []` | **Conflict** | Objek berpaginasi bukan array, sehingga menurut pembacaan statis daftar pengkajian keperawatan **selalu kosong** dan layar selalu memulai pengkajian baru. Hook dokter `use-inpatient-medical-assessment.jsx` sudah menormalkan lewat `toAssessmentList` | Konsep yang sudah tersimpan tidak termuat; berpotensi ditolak sebagai pengkajian awal kedua. **Belum dibuktikan saat berjalan** |
| `RLN3-CAP-18` | Keperawatan — assessment status | FE + `ClinicalManagement` | `BE@df3679c0 ClinicalManagement/Enums/PatientAssessmentStatus.cs` `Draft = 0`, `InProgress = 1`, `Completed = 2`, `Cancelled = 3`. `FE@147355f5 assessment-section.jsx` baris 245, 286, 314, dan 643 menganggap `assessmentStatus === 1` sebagai selesai | **Conflict** | Frontend memuat addendum untuk dokumen `InProgress` dan tidak mengenali `Completed` | — |
| `RLN3-CAP-19` | Keperawatan — fall-risk enum | FE + `ClinicalManagement` | `RLN3-CON-01` | **Conflict** | — | Keselamatan klinis |
| `RLN3-CAP-20` | Keperawatan — fall-risk scoring | FE + `ClinicalManagement` | `RLN3-CON-01`; `RWI-FACT-036` | **Conflict** | Skor dan batas tertanam di backend, label batas Morse tertanam di frontend; keduanya melanggar `RWI-DEC-136` | Keselamatan klinis |
| `RLN3-CAP-21` | Keperawatan — detail-before-edit | FE | `FE@147355f5 assessment-section.jsx` baris 239–242 mengisi form dari **baris daftar**; `patient-assessment.service.js#getPatientAssessmentById` tersedia tetapi tidak dipakai keperawatan | **Repair** | Ambil detail sebelum form diisi | Isian yang tidak ada di tanggapan daftar dapat tertimpa kosong saat disimpan |
| `RLN3-CAP-22` | Keperawatan — unknown vs false | FE + `ClinicalManagement` | Backend menolak penyelesaian bila risiko jatuh atau gizi masih `Unknown`, `PatientAssessmentController.cs` baris 2083–2086. Frontend `assessment-section.jsx` baris 104, 121, dan 127 mengirim `consciousnessStatus \|\| 1`, `appetiteStatus \|\| 1`, dan `functionalStatus \|\| 1`. Pada backend nilai `1` berarti `ComposMentis`, `Normal`, dan `Independent` | **Repair** | Isian yang belum dikaji terkirim sebagai keadaan normal | Melanggar `BR-RWI-007` |
| `RLN3-CAP-23` | Keperawatan — permission fallback | FE | Penanganan `401`/`403` khusus hanya ada di `use-inpatient-financial-clearance.jsx` baris 111; nol di ruang kerja dokter dan keperawatan | **Missing** | Layar menampilkan galat umum saat hak ditolak | — |
| `RLN3-CAP-24` | Dokter — SOAP finalization semantics | `ClinicalManagement` + `PharmacyManagement` | Finalisasi mendaftar tanda tangan pada mesin keutuhan (`BE-RWI-038`), tetapi tanpa pemeriksaan penulis (`RWI-FACT-029`, dibaca ulang pada SHA yang sama) | **Repair** | Lihat `RLN3-CAP-30` | — |
| `RLN3-CAP-25` | Dokter — CPPT DPJP verification | `ClinicalManagement` | `BE@df3679c0 ClinicalManagement/Services/CpptVerificationService.cs` baris 221 memanggil `InpatientClinicalContextService.IsDoctorAssignedAsync`, yang menerima penugasan **peran apa pun** yang aktif saat ini. Verifikasi catatan sendiri ditolak baris 231–235 | **Conflict** | Konsulen dan dokter jaga dapat memverifikasi, bertentangan dengan `RWI-DEC-125` dan `AC-RWI-011`; pesan penolakan tetap berbunyi "hanya DPJP". Pengecualian DPJP terakhir pada episode tertutup `RWI-DEC-126` **belum ada**: setelah penutupan tidak ada penugasan aktif, sehingga tidak ada yang dapat memverifikasi | — |
| `RLN3-CAP-26` | Dokter — idempotency per command | Beberapa modul | Kunci permintaan ada pada `NursingInterventionController`, `PatientProcedureController` (buat), `PhysicianVisitController`, dan `PharmacyManagement/Controllers/PrescriptionController`. **Tidak ada** pada `DoctorConsultationController`, `PatientAssessmentController`, dan `PatientIntegratedProgressNoteController` | **Repair** | Tiga perintah tulis utama belum tahan kiriman ulang | `BR-RWI-011` |
| `RLN3-CAP-27` | Dokter — prescription header-item workflow | `PharmacyManagement` | `BE@df3679c0 PharmacyManagement/Services/PrescriptionWorkflowService.cs` baris 32–33 menolak finalisasi klinis resep tanpa item; `CanDelete` baris 112–117 hanya untuk draft kosong | **Ready to reuse** | — | Jalur pakai template dapat mengisi item tanpa pemeriksaan lengkap, lihat 17.3.6 |
| `RLN3-CAP-28` | Dokter — unsaved draft guard | FE | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-exit-guard.jsx` baris 48–56 (`beforeunload`) hanya dipakai admisi; nol di ruang kerja dokter dan keperawatan | **Reuse with adapter** | Pola sudah ada, belum dipasang | `AC-RWI-017` |
| `RLN3-CAP-29` | Shared — enum contract | FE + `ClinicalManagement` | Selain `RLN3-CAP-18` dan `RLN3-CON-01`: status gizi frontend 1/2/3 terhadap backend `NoRisk`/`LowRisk`/`MediumRisk`/`HighRisk` (`nutrition-group.jsx` baris 13–15); nafsu makan frontend 3 "Sangat Buruk" terhadap backend `3 = Increased`, `4 = Poor` (baris 19–21); status fungsional frontend 1–5 terhadap backend `Independent`, `NeedPartialAssistance`, `FullyDependent` (`functional-group.jsx` baris 11–15). DTO `PatientAssessmentDtos.cs` tidak memvalidasi nilai enum | **Conflict** | Pilihan "Ketergantungan Berat" dan "Total" tersimpan sebagai angka 4 dan 5 yang tidak dikenal backend | Data klinis tersimpan dengan arti berbeda dari yang dipilih perawat |
| `RLN3-CAP-30` | Shared — episode guard dan actor resolution pada jalur ubah/selesai | `ClinicalManagement` | Jalur **buat** dijaga `InpatientClinicalContextService.ResolveForDoctorWriteAsync` (`BE-RWI-076`). Jalur ubah dan selesai lihat 17.3.5 | **Repair** | — | — |
| `RLN3-CAP-31` | Shared — error state | FE | `ClinicalStateBoundary` dipakai `doctor-inpatient-view.jsx` baris 76, `physician-workspace-view.jsx` baris 156, dan seksi keperawatan (`src/components/ui/clinical-workspace`) | **Ready to reuse** | — | Dua implementasi `ClinicalStateBoundary` pada dua pustaka berbeda |
| `RLN3-CAP-32` | Shared — permission contract | `ClinicalManagement` | `AccessPermission` terpasang pada seluruh endpoint yang diperiksa; hubungan klinis ditegakkan pada jalur buat dan pada unit perawat, tetapi tidak pada jalur ubah dokter maupun peran verifikasi | **Repair** | — | `BR-RWI-009` |

#### 17.3.5 Jalur ubah dan penyelesaian catatan dokter — titipan `RWI-DEC-128`

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-33` | SOAP: ubah, simpan otomatis, selesai hanya oleh penulis | `ClinicalManagement` + `PharmacyManagement` | `RWI-FACT-029`, dibaca ulang pada `BE@df3679c0`: `DoctorConsultationController.cs` baris 584–700 dan 769; `PharmacyManagement/Services/ConsultationFinalizationService.cs` baris 55–94 dan 178 | **Repair** | Tanpa pemeriksaan penulis, penugasan, maupun status episode | Konsep dapat diselesaikan orang lain dan tercatat bertanda tangan atas nama penulis |
| `RLN3-CAP-34` | Kajian medis: ubah dan selesai hanya oleh penulis | `ClinicalManagement` | `BE@df3679c0 PatientAssessmentController.cs#UpdateAssessment` baris 706–760 dan `#CompleteAssessment` baris 850–1000: penjaga `EnsureNursingUnitAuthorityAsync` **langsung meloloskan** jenis kajian medis. Tanda tangan memakai `AssessmentByUserId ?? CreateBy`, bukan penekan tombol | **Repair** | Tanpa pemeriksaan penulis, penugasan dokter, maupun status episode | Sama dengan SOAP |
| `RLN3-CAP-35` | Tindakan dokter: buat, ubah, laksanakan | `ClinicalManagement` | `BE@df3679c0 PatientProcedureController.cs#CreateProcedure` baris 462–520 memanggil `ResolveForDoctorWriteAsync` dengan **`forNewDocument: false`**, sehingga episode tertutup **tidak** ditolak untuk tindakan baru. `#UpdateProcedure` baris 737 dst. hanya memeriksa hak `PatientProcedure : Update` dan status konsultasi. `#ExecuteProcedure` baris 1037–1120 menulis pelaksana dari akun login dan mendaftar tanda tangan atas nama pelaksana | **Repair** | Tindakan baru pada episode tertutup bertentangan dengan `RWI-DEC-086` dan `RWI-DEC-129` jalur tidak normal (b). Jalur ubah tanpa penulis dan penugasan | Siapa "penulis" tindakan untuk `RWI-DEC-128` belum jelas, `RLN3-UNK-01` |
| `RLN3-CAP-36` | Catatan terpadu: ubah dan tanda tangan konsep hanya oleh penulis | `ClinicalManagement` + `MedicalRecordManagement` | `BE@df3679c0 PatientIntegratedProgressNoteController.cs#UpdateProgressNote` baris 698–790: pemeriksaan unit hanya untuk profesi Perawat; profesi Dokter tanpa pemeriksaan penulis; `EnsureMutableAsync` meloloskan `Draft`. Tanda tangan lewat `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs#SignAsync` baris 260–292 **mewajibkan** penulis sama dengan penandatangan | **Repair** | Konsep CPPT dokter lain dapat diubah, tetapi tidak dapat ditandatangani orang lain | — |
| `RLN3-CAP-37` | Daftar konsep milik sendiri untuk "Catatan Saya" `RWI-DEC-127` dan `RWI-DEC-129` | `MedicalRecordManagement` | `BE@df3679c0 MedicalRecordManagement/Controllers/ClinicalDocumentIntegrityController.cs#GetMyUnsigned` baris 240–290: menyaring `AuthorUserId` = pengguna login dan status `Draft`, lintas jenis dokumen dan lintas layanan, berpaginasi. Frontend `FE@147355f5 src/app/health-services/medical-record-management/my-unsigned-notes/page.jsx` | **Reuse with adapter** | Hanya konsep. Catatan **final** milik sendiri untuk addendum belum punya daftar. Tidak menyaring rawat inap | **Mengoreksi `RWI-FACT-028` butir (2)**, yang menyatakan belum ada daftar catatan berdasarkan penulis. Pernyataan itu hanya benar untuk catatan final |
| `RLN3-CAP-38` | Penguncian otomatis konsep saat kunjungan selesai | `MedicalRecordManagement` | `BE@df3679c0 ClinicalDocumentIntegrityService.cs#LockOpenDocumentsForEncounterAsync` baris 325–360 mengubah semua konsep satu kunjungan menjadi `LockedUnsigned`. Dipanggil `RegistrationManagement/Controllers/DoctorQueueController.cs` baris 503, `NurseStationQueueController.cs` baris 343, dan `PatientEncounterController.cs` baris 955 saat status menjadi `Completed`. `InPatientManagement` **tidak** memanggilnya | **Conflict** | Kebijakan `RM-DEC-003` mengunci konsep saat kunjungan selesai, sedangkan `RWI-DEC-129` membiarkan konsep diselesaikan setelah episode ditutup. Hari ini penutupan episode tidak memicu penguncian | Bila status kunjungan rawat inap diubah menjadi `Completed` lewat `PatientEncounterController`, seluruh konsep terkunci dan `RWI-DEC-129` tidak dapat dijalankan |

#### 17.3.6 Jalur pakai template resep — titipan `RWI-DEC-135`

| ID | Kebutuhan `RWI-DEC-122` | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RLN3-CAP-39` | Hanya pemilik yang memakai templatenya | `PharmacyManagement` | `BE@df3679c0 PharmacyManagement/Services/PrescriptionTemplateService.cs#ApplyAsync` baris 172–186 tidak memeriksa `OwnerDoctorId`; endpoint `POST {id}/apply` memakai hak `PrescriptionTemplate : Create` (`PrescriptionTemplateController.cs` baris 178–182) | **Repair** | — | Template pribadi dokter lain dapat dipakai lewat permintaan langsung |
| `RLN3-CAP-40` | Butir (3): perawat tidak memakai template | `PharmacyManagement` | Sama; tidak ada pemeriksaan bahwa pemakai adalah dokter | **Repair** | Hanya bergantung pada pembagian hak akses | — |
| `RLN3-CAP-41` | Butir (4): hanya mengisi draft | `PharmacyManagement` | `ApplyAsync` baris 178 `EnsureEditableAsync(request.PrescriptionId)` | **Ready to reuse** | — | — |
| `RLN3-CAP-42` | Butir (4): pemeriksaan ulang alergi saat dipakai | `PharmacyManagement` | Nol pemeriksaan alergi otomatis di `PharmacyManagement`; satu-satunya jejak adalah butir telaah manual apoteker `CLI_ALLERGY` pada `PharmacyManagement/Seeders/PrescriptionReviewCriterionSeeder.cs` baris 23 | **Missing** | Belum ada sumber alergi yang dibaca saat meresepkan | Keselamatan obat |
| `RLN3-CAP-43` | Butir (4) dan (5): ketersediaan diperiksa, obat tidak tersedia ditandai dan tidak tersimpan tanpa diganti | `PharmacyManagement` | `ApplyAsync` menyalin obat tanpa memeriksa `IsActive` atau `IsPrescribable` saat dipakai; kegagalan coverage satu obat melempar galat sehingga **seluruh** pemakaian batal | **Repair** | Butuh penandaan per butir, bukan gagal total | — |
| `RLN3-CAP-44` | Butir (6): template kosong ditolak | `PharmacyManagement` | `ValidateTemplateContentAsync` baris 277–282 hanya menolak racikan tanpa bahan; template tanpa obat **dan** tanpa racikan lolos | **Repair** | — | `BR-RWI-015` |

### 17.4 Contoh perjalanan yang benar-benar ditelusuri: risiko jatuh keperawatan — `RLN3-CON-01`

**Tujuan proses:** perawat menilai risiko jatuh pasien rawat inap supaya pencegahan jatuh dapat
dimulai. **Pelaku:** perawat unit tempat pasien dirawat. **Pemicu:** perawat membuka Pengkajian pada
ruang kerja keperawatan. **Prasyarat:** perawat bertugas di unit itu (`RWI-DEC-100`).

**Langkah as-is menurut source:**

1. Frontend menampilkan tiga pilihan kategori dengan nilai tetap, `fall-risk-group.jsx` baris 13–15:
   `1` "Risiko Rendah (Skor 0 - 24 Morse)", `2` "Risiko Sedang (Skor 25 - 44 Morse)", `3` "Risiko
   Tinggi (Skor ≥ 45 Morse)".
2. Perawat memilih kategori, mengetik skor, dan boleh mencentang "Gaya Berjalan / Ataksia" serta
   "Ketidakstabilan Postur". Layar **tidak** punya kontrol untuk `hasFallRisk`, sehingga nilainya
   selalu `false` (`assessment-section.jsx` baris 68 dan 115).
3. Frontend mengirim `hasFallRisk`, `fallRiskStatus`, `fallRiskScore`, `hasAtaxia`, dan
   `hasPosturalInstability` (baris 115–119).
4. Backend menetapkan `hasFallRisk = HasFallRisk || HasAtaxia || HasPosturalInstability`, lalu
   **membuang** skor ketikan dan menghitung skor sendiri: ataksia +1, instabilitas +1
   (`PatientAssessmentController.cs` baris 2119–2120 dan 2321–2331).
5. Backend menetapkan kategori (`CalculateFallRiskStatus`, baris 2363–2383): bila `hasFallRisk`
   salah dan kategori yang dikirim bukan `Unknown`, hasilnya `NoRisk`; bila benar, skor ≥ 2 `HighRisk`,
   skor 1 `MediumRisk`, skor 0 `LowRisk`.
6. Enum backend `FallRiskStatus`: `Unknown = 0`, `NoRisk = 1`, `LowRisk = 2`, `MediumRisk = 3`,
   `HighRisk = 4`. Saat dibuka ulang, frontend membaca angka itu memakai tabel langkah 1.

**Hasil untuk tiga kasus nyata** (data samaran):

| Kasus | Yang dipilih perawat | Yang tersimpan backend | Yang tampil saat dibuka ulang |
|---|---|---|---|
| Tn. A tanpa centang | Tinggi (`3`), skor 55 | `NoRisk` (`1`), skor kosong | "Risiko Rendah" |
| Ny. B mencentang ataksia | Tinggi (`3`), skor 55 | `MediumRisk` (`3`), skor 1 | "Risiko Tinggi" — kebetulan sama labelnya, beda artinya |
| An. C mencentang keduanya | Sedang (`2`), skor 30 | `HighRisk` (`4`), skor 2 | Tidak ada label untuk nilai `4` |

**Aturan yang dilanggar:** `RWI-DEC-124` (skor dan kategori dari konfigurasi berversi yang disahkan),
`RWI-DEC-136` (angka batas dilarang ditanam di source backend maupun frontend), dan `BR-RWI-007`.
**Hasil akhir hari ini:** data risiko jatuh keperawatan rawat inap tidak dapat dipercaya sebagai dasar
pencegahan jatuh. Perbaikannya **tidak** memerlukan keputusan bisnis baru, karena arahnya sudah
dikunci `RWI-DEC-124` dan `RWI-DEC-136`; isi klinisnya tetap menunggu `RWI-OQ-056`.

### 17.5 Kontrak as-is yang ditelusuri

Tabel berikut hanya memuat endpoint yang diperiksa pada audit ini. Semua berada di dalam
`ApiResponse<T>`. Tidak ada endpoint baru yang diusulkan.

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Daftar pengkajian satu episode, berpaginasi | `PatientAssessment : Read` | Query `assessmentType`, `pageNumber` (bawaan 1), `pageSize` (bawaan 25) | `ResponsePatientAssessmentPagedResult` |
| `GET` | `/{id}` | Detail satu pengkajian | `PatientAssessment : Read` | - | `PatientAssessmentDetailResponse` |
| `PUT` | `/{id}` | Mengubah pengkajian yang belum selesai | `PatientAssessment : Update` | Body `UpdatePatientAssessmentRequest` | `object` |
| `PATCH` | `/{id}/complete` | Menyelesaikan pengkajian dan mendaftarkan tanda tangan | `PatientAssessment : Update` | Body `CompletePatientAssessmentRequest` | `PatientAssessmentCompleteResponse` |

Kode status: `400` isian wajib kosong atau dokumen sudah selesai; `403` perawat tidak bertugas di unit
pasien (tidak berlaku untuk kajian medis); `404` pengkajian tidak ditemukan.

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `PUT` | `/{id}` | Mengubah konsep catatan terpadu | `PatientIntegratedProgressNote : Update` | Body `UpdatePatientIntegratedProgressNoteRequest` | `PatientIntegratedProgressNoteUpdateResponse` |
| `PATCH` | `/{id}/verify` | Verifikasi catatan oleh dokter | `PatientIntegratedProgressNote : Verify` | Body permintaan verifikasi | `PatientIntegratedProgressNoteResponse` |

Kode status: `400` catatan dibatalkan, catatan hasil generate, atau sudah terkunci; `403` perawat unit
lain, dokter tanpa penugasan aktif, atau memverifikasi catatan sendiri; `409` sudah diverifikasi;
`422` catatan tidak berada di bawah perawatan rawat inap.

#### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `PUT` | `/{id}` | Mengubah konsep SOAP | `DoctorConsultation : Update` | Body `UpdateDoctorConsultationRequest` | `DoctorConsultationUpdateResponse` |
| `PATCH` | `/{id}/soap` | Simpan otomatis bagian SOAP | `DoctorConsultation : Update` | Body `UpdateDoctorConsultationSoapRequest` | `DoctorConsultationSoapUpdateResponse` |
| `PATCH` | `/{id}/complete` | Menyelesaikan konsultasi, sama dengan tanda tangan penulis | `DoctorConsultation : Update` | Body permintaan finalisasi | `ConsultationFinalizationResponse` |

Kode status: `400` konsultasi sudah selesai atau dibatalkan; `404` tidak ditemukan; `409` data sudah
berubah sejak dimuat.

#### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Mencatat tindakan baru, tahan kiriman ulang | `PatientProcedure : Create` | Body `CreatePatientProcedureRequest` | `PatientProcedureCreateResponse` |
| `PUT` | `/{id}` | Mengubah tindakan | `PatientProcedure : Update` | Body `UpdatePatientProcedureRequest` | `PatientProcedureUpdateResponse` |
| `PATCH` | `/{id}/execute` | Menandai tindakan sudah dilaksanakan | `PatientProcedure : Update` | Body `ExecutePatientProcedureRequest` | `object` |

Kode status: `400` isian tidak valid, butuh persetujuan, atau sudah dibatalkan; `403` dokter tanpa
penugasan pada waktu tindakan. Kiriman ulang dengan kunci permintaan yang sama dijawab `200` beserta
tindakan yang sudah tercatat, bukan membuat tindakan kedua.

#### Health Services / Pharmacy Management / Prescription Template

Base URL: `api/v1/health-services/pharmacy-management/prescription-templates`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/{id}/apply` | Menyalin isi template ke draft resep | `PrescriptionTemplate : Create` | Body `ApplyPrescriptionTemplateRequest` | `ApplyPrescriptionTemplateResponse` |

Kode status: `400` resep bukan draft, template tidak aktif, atau coverage salah satu obat gagal.

#### Health Services / Medical Record Management / Clinical Document Integrity

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/by-document/{documentKind}/{documentId}/sign` | Penulis menandatangani konsepnya | `ClinicalDocumentIntegrity : Update` | - | `ClinicalDocumentIntegrityResponse` |
| `GET` | `/my-unsigned` | Daftar konsep milik pengguna login yang belum ditandatangani | `ClinicalDocumentIntegrity : Read` | Query `pageNumber`, `pageSize` | `ResponseUnsignedDocumentPagedResult` |

Kode status: `403` bukan penulis catatan; `404` catatan tidak terdaftar pada mesin keutuhan; `400`
catatan sudah terkunci.

### 17.6 Ketidakcocokan frontend dan backend

| ID | Temuan | Backend `@df3679c0` | Frontend `@147355f5` |
|---|---|---|---|
| `RLN3-CON-01` | Risiko jatuh keperawatan | Enum 0–4, skor dari dua centang, kategori dihitung ulang | Nilai 1–3 berlabel Morse, skor diketik, tanpa kontrol `hasFallRisk` |
| `RLN3-CON-02` | Daftar pengkajian per episode | Objek berpaginasi | Mengharapkan array pada jalur keperawatan |
| `RLN3-CON-03` | Status pengkajian | `Completed = 2` | Menganggap `1` selesai |
| `RLN3-CON-04` | Status gizi, nafsu makan, status fungsional | Lihat `RLN3-CAP-29` | Lihat `RLN3-CAP-29` |
| `RLN3-CON-05` | Susunan ruang kerja dokter | — | Dua halaman, bukan susunan Rawat Jalan |
| `RLN3-CON-06` | Kebijakan konsep saat kunjungan selesai | `RM-DEC-003` mengunci konsep | — (konflik kebijakan dengan `RWI-DEC-129`) |

### 17.7 Fakta, inferensi, dan rekomendasi

**Fakta.** Seluruh baris 17.3 dan langkah 17.4 dibaca langsung dari source pada SHA yang tercatat.

**Inferensi, belum dibuktikan saat berjalan:**

- `RLN3-CAP-17`: daftar pengkajian keperawatan selalu kosong di layar. Kesimpulan ini diambil dari
  bentuk tanggapan dan pembacaan kode, bukan dari aplikasi yang dijalankan.
- `RLN3-CAP-29`: angka enum 4 dan 5 tersimpan di database. Serializer bawaan menerima angka enum yang
  tidak terdefinisi, tetapi hal ini tidak diuji pada audit ini.

**Rekomendasi, untuk dipertimbangkan dan bukan untuk dijalankan tanpa persetujuan:**

1. `RLN3-CON-01`, `RLN3-CAP-17`, `RLN3-CAP-18`, `RLN3-CAP-22`, dan `RLN3-CAP-29` layak menjadi satu
   gelombang perbaikan keselamatan lintas repository **sebelum** redesign keperawatan. Arahnya sudah
   dikunci keputusan yang ada, sehingga tidak butuh wawancara baru.
2. `RLN3-CAP-25` dan `RLN3-CAP-33` s.d. `RLN3-CAP-36` layak disatukan dengan perbaikan `BE-RWI-076`
   yang sudah diwajibkan `RWI-DEC-128`.
3. `RLN3-CAP-37` dipakai sebagai titik awal "Catatan Saya" untuk bagian konsep, bukan dibangun ulang.
4. `RLN3-CAP-03` dan `RLN3-CAP-04` diteruskan ke `requirement-completeness-gate` dan desain, karena
   menyamakan layout menyentuh komponen milik Rawat Jalan.

### 17.8 Unknown dan pertanyaan penutup

| ID | Pertanyaan | Kenapa tidak dapat dijawab source | Pemilik |
|---|---|---|---|
| `RLN3-UNK-01` | Untuk `RWI-DEC-128`, siapa "penulis" tindakan dokter: pemesan tindakan atau pelaksananya? Source menandatangani atas nama **pelaksana** saat dilaksanakan | `BR-RWI-013` memisahkan pesanan dari pelaksanaan, dan keputusan tidak menyebutnya | Muhammad Hamzah |
| `RLN3-UNK-02` | Apakah menu Catatan Keperawatan PRD bagian 41 sama dengan CPPT berprofesi Perawat `RWI-DEC-115`, dan apakah isi V1 (perioperatif, diet, observasi cairan/WSD, sliding scale) ikut dipertahankan | PRD hanya menggambar kartu catatan | Muhammad Hamzah |
| `RLN3-UNK-03` | Apakah `RM-DEC-003` berlaku untuk episode rawat inap, sehingga bertabrakan dengan `RWI-DEC-129` | Keputusan modul Rekam Medis | Pemilik `MedicalRecordManagement` |
| `RLN3-UNK-04` | Tiga bagian Kajian Umum yang disebut PRD tetapi tidak ada di frontend V1: Psikososial (sebagai bagian), Alat Bantu (sebagai bagian), Catatan Relevan — tetap diminta atau PRD keliru | Backend V1 tidak tersedia | Muhammad Hamzah |
| `RLN3-UNK-05` | Apakah daftar "Catatan Saya" boleh memakai ulang `my-unsigned` milik `MedicalRecordManagement`, mengingat `RWI-DEC-127` menempatkannya di ruang kerja dokter | Kepemilikan endpoint lintas modul | Muhammad Hamzah bersama pemilik `MedicalRecordManagement` |

**Koreksi yang wajib dibawa ke decision log:** `RWI-FACT-028` butir (2) terlalu luas; lihat
`RLN3-CAP-37`. Dokumen ini **tidak** mengubah decision log karena di luar batas tulis skill ini.

### 17.9 Keterbatasan audit

- Aplikasi, build, test, dan database tidak dijalankan.
- Backend V1 tidak tersedia, sehingga isi V1 hanya dari frontend.
- Jalur pemanggilan frontend untuk tiap endpoint tidak ditelusuri seluruhnya; hanya jalur yang disebut
  pada bukti.
- Butir MVP-0 ditafsirkan dari judul pada PRD bagian 56 dan aturan `BR-RWI-*`, karena PRD tidak
  memberi definisi per butir.

### 17.10 Handoff dan staleness

**Temuan manifest yang ditutup bagian ini:** `RLN-07` bagian V1, `RLN-09`, `RLN-10`, dan `RLN-14`.
Penandaan status fase pada `blueprint-manifest.md` dikerjakan `manage-module-blueprint`.

Bagian 17 menjadi `STALE` bila salah satu berikut berubah:

- `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`,
  `DoctorConsultationController.cs`, `PatientProcedureController.cs`, atau
  `PatientIntegratedProgressNoteController.cs`;
- `Areas/HealthServices/ClinicalManagement/Services/CpptVerificationService.cs` atau
  `InpatientClinicalContextService.cs`;
- `Areas/HealthServices/PharmacyManagement/Services/PrescriptionTemplateService.cs` atau
  `ConsultationFinalizationService.cs`;
- `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs`;
- enum `FallRiskStatus`, `NutritionRiskStatus`, `AppetiteStatus`, `FunctionalStatus`,
  `PatientAssessmentStatus`;
- frontend `src/components/view/health-services/inpatient-management/nursing-workspace/**`,
  `physician-workspace/**`, `doctor-inpatient/**`, atau
  `src/components/view/health-services/registration-management/doctor-queues/**`;
- `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md` dengan SHA-256 berbeda;
- keputusan `RWI-DEC-107`, `RWI-DEC-115`, `RWI-DEC-122`, `RWI-DEC-124`, `RWI-DEC-125`,
  `RWI-DEC-128`, `RWI-DEC-129`, `RWI-DEC-135`, atau `RWI-DEC-136`.

---

## 18. Audit Kemampuan Integrasi Rawat Inap ↔ Billing (Pass A — Muhammad Hamzah, 17 September 2026)

> **`STALE` sejak 1 Oktober 2026.** `INT-CAP-06`, `INT-FACT-02`, dan endpoint target `billing-summary`
> pada bagian ini ditimpa bagian 19 (`BE@c8e99ce5`). Keputusan `RWI-DEC-166` dan `RWI-DEC-167` juga
> mengubah arah integrasi yang dijelaskan skenario 2 sampai 4 di bawah. Bagian ini dipertahankan
> sebagai riwayat.

### 18.1 Konteks, Batas Audit, dan Metodologi

Bagian ini disusun untuk menindaklanjuti kesepakatan **Amendment Pass Integrasi Rawat Inap ↔ Billing (Pass A — Muhammad Hamzah)** tertanggal 17 September 2026. Sesi tersebut telah menetapkan enam keputusan arsitektur kritis (`RWI-DEC-156` sampai `RWI-DEC-161`) dan sepuluh kriteria penerimaan (`RWI-AC-232` sampai `RWI-AC-241`) yang bersumber dari berkas masukan [PRD Integrasi-Rawat-Inap-dengan-Billing.md](../../Modul-RS/Rawat-Inap-To-Billing/PRD%20Integrasi-Rawat-Inap-dengan-Billing.md).

Tujuan audit ini adalah membuktikan perilaku nyata (*as-is*) pada kode program dan skema basis data di kedua repositori, mengidentifikasi komponen yang dapat langsung dipakai ulang (*ready to reuse*), komponen yang membutuhkan adaptasi (*adapter*), komponen yang harus diperluas (*extend*), komponen yang memerlukan perbaikan (*repair*), serta komponen yang masih belum terwujud (*missing*).

**Batas pemeriksaan kode (Snapshot SHA):**
- **Backend:** `NewQuilvianSystemBackend` pada commit `fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344` (cabang `MHamzah`).
- **Frontend:** `QuilvianSystemFrontendDev` pada commit `2c00758832f834cff0288bef4f0d2fcf1161fb52` (cabang `HamzahV2`).

**Area fungsional yang diaudit:**
1. Modul Rawat Inap (`Areas/HealthServices/InPatientManagement`): model penempatan tempat tidur (`InpBedPlacement`), episode (`InpEpisode`), penanda kelayakan keuangan sementara (`InpFinancialClearance`), pengontrol operasional kamar (`InpatientBedOccupancyController`), dan pengontrol pemulangan (`InpatientDischargeController`).
2. Modul Billing / Kasir (`Areas/HealthServices/BillingManagement`): layanan penerimaan beban tagihan dari modul lain (`ContractBillingChargeSourceAdapter`, `BillingInvoiceService`), kebijakan tarif kamar (`RoomChargePolicyService`, `MstRoomChargePolicy`), pengelolaan deposit pasien (`BillingDepositService`, `BillingPatientFundsController`), dan pratinjau kesiapan penutupan tagihan (`BillingFinalizationService`, `BillingFinalizationsController`).
3. Antarmuka Pengguna Rawat Inap (Frontend): langkah pembayaran deposit pada formulir admisi berlangkah (`inpatient-admission-deposit-step.jsx`, `use-inpatient-admission-deposit.jsx`), alur pemulangan pasien (`inpatient-discharge-view.jsx`), pemeriksaan syarat penutupan (`inpatient-closure-view.jsx`), dan layar kelayakan keuangan (`inpatient-financial-clearance-view.jsx`).

---

### 18.2 Matriks Kontrak Bukti Kemampuan Integrasi

Sesuai dengan kontrak audit rekayasa Quilvian, setiap kemampuan dievaluasi secara ketat dan diklasifikasikan ke dalam tepat satu status kanonik:

| ID | Kebutuhan & Keputusan | Pemilik Domain | Bukti Lapangan (`repo/path#symbol@SHA`) | Status | Analisis Gap & Kebutuhan Adapter | Risiko Operasional & Klinis |
|---|---|---|---|---|---|---|
| `INT-CAP-01` | **Sinkronisasi Awal Episode & Akun Billing saat Admisi Disahkan** (`RANAP-INT-001`, `RWI-DEC-156`, `RWI-AC-232`) | `InPatientManagement` (M. Hamzah) & `BillingManagement` (Yasmina) | `BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:854 #PlacePatientAsync`<br>`BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs:93,134`<br>`FE@2c00758 src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx:24`<br>`FE@2c00758 src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx:21` | **Extend** | **Backend:** Status `Admitted` diaktifkan saat penempatan awal, tetapi belum ada pemanggilan event outbox sinkronisasi akun episode ke Billing. Endpoint kebijakan deposit (`GET /deposit-policies`) dan top-up deposit (`POST /top-ups`) di Billing sudah berwujud.<br>**Frontend:** Formulir deposit admisi (`FE-RWI-058`) hanya mencatat nominal angka di browser tanpa menangkap `PaymentMethodId`, dan belum memanggil `GET /deposit-policies` (sebelumnya dianggap belum ada di backend). | Keluarga pasien yang telah berada di loket admisi tidak dapat langsung membayar uang muka/deposit di kasir bila data kunjungan belum dikenal oleh modul Billing. |
| `INT-CAP-02` | **Standardisasi Pemicu Room Charge & Waktu Akhir Hunian Kamar** (`RANAP-INT-002`, `RWI-DEC-156`, `RWI-DEC-159`, `RWI-AC-233`, `RWI-AC-237`) | `InPatientManagement` & `BillingManagement` | `BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:840 #PlacePatientAsync`<br>`BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs:587 #RecordPatientDepartureAsync`<br>`BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs:23 #SourcePolicies`<br>`BE@fe7e60d Areas/HealthServices/BillingManagement/MasterData/Services/RoomChargePolicyService.cs:10` | **Extend** | **Billing:** `ContractBillingChargeSourceAdapter` belum mendaftarkan domain `ROOM_STAY` atau `INPATIENT` ke dalam `SourcePolicies` (masih melempar galat validasi 422 jika dikirim). Billing sudah memiliki `MstRoomChargePolicy` dan formula potongan jam.<br>**Rawat Inap:** Penempatan fisik (`PlacePatientAsync`) dan pencatatan kepergian fisik (`RecordPatientDepartureAsync`, `episode.PhysicallyLeftAt`) sudah ada, tetapi belum mengirimkan event `ROOM_ASSIGNED` dan `BED_RELEASED` (dengan durasi `OccupancyEndAt = PhysicallyLeftAt`) ke Billing. | Tagihan sewa kamar dapat salah hitung jika dihitung sejak pendaftaran loket (pasien belum tiba di ranjang) atau dihentikan terlalu dini saat DPJP memberi izin pulang (padahal pasien masih berbaring menunggu jemputan ambulans/keluarga). |
| `INT-CAP-03` | **Mutasi Kamar & Koreksi Fakta Hunian saat Tagihan Terbuka** (`RANAP-INT-003`, `RWI-DEC-157`, `RWI-AC-234`, `RWI-AC-235`) | `InPatientManagement` & `BillingManagement` | `BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:1080 #TransferAsync`<br>`BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Corrections.cs:37 #OpenCorrectionSessionAsync`<br>`BE@fe7e60d Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs:8` | **Missing** | **Rawat Inap:** Pemindahan ranjang biasa (`TransferAsync`) sudah berfungsi menutup penempatan lama dan membuka yang baru, tetapi belum ada antarmuka maupun layanan untuk **koreksi salah input** (koreksi bed, kelas kamar, atau waktu masuk yang salah ketik). `InpCorrectionSession` yang ada hanya untuk episode berstatus `Closed`.<br>**Billing:** Belum tersedia penerima event `OCCUPANCY_CORRECTED` untuk memicu penghitungan ulang (*repricing/reversal*) tagihan kamar berjalan saat status tagihan `BillingStatus == OPEN`. | Kesalahan input kelas atau kamar oleh petugas admisi di awal perawatan tidak dapat dikoreksi di sistem, memicu sengketa selisih tagihan (*dispute billing*) atau keharusan manipulasi database langsung. |
| `INT-CAP-04` | **Konsumsi Ringkasan Tagihan (Billing Summary) & Hak Akses Tanpa Rupiah** (`RANAP-INT-004`, `RWI-DEC-160`, `RWI-AC-238`, `RWI-AC-239`) | `InPatientManagement` & `BillingManagement` | `BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs:112 #GetEpisodeDepositSummary`<br>`BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinalizationsController.cs:35 #Preview`<br>`FE@2c00758 src/components/view/health-services/inpatient-management/inpatient-financial-clearance-view.jsx:64` | **Extend** | **Backend:** Billing memiliki data ringkasan deposit (`EpisodeDepositSummaryResponse`) dan status kesiapan tagihan beserta kendala (`FinalizationPreviewResponse.BlockingReasons`), namun belum ada endpoint gabungan di Rawat Inap.<br>**Frontend & Hak Akses:** Rawat Inap belum memiliki klaim izin `InpatientBilling:View`. Layar perawat belum menampilkan lencana status operasional (Lunas / Tertahan), dan belum ada mekanisme penyembunyian angka rupiah bagi staf keperawatan. | Staf keperawatan bangsal terganggu fokus klinisnya jika dibebani melihat nominal uang tagihan pasien, atau sebaliknya perawat tidak mengetahui bahwa kepulangan pasien sedang tertahan karena masalah administrasi kasir. |
| `INT-CAP-05` | **Gerbang Kelayakan Keuangan & Penguncian Ulang Otomatis (Auto-Reblock)** (`RANAP-INT-005`, `RWI-DEC-158`, `RWI-AC-236`) | `InPatientManagement` & `BillingManagement` | `BE@fe7e60d Areas/HealthServices/InPatientManagement/Models/InpFinancialClearance.cs:19 #ClearanceStatus`<br>`BE@fe7e60d Areas/HealthServices/InPatientManagement/Enums/InpFinancialClearanceStatus.cs:3`<br>`BE@fe7e60d Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs:226,538,878`<br>`FE@2c00758 src/components/view/health-services/inpatient-management/inpatient-closure-view.jsx:340` | **Repair & Extend** | **Backend:** Enum `InpFinancialClearanceStatus` baru memuat `Pending`, `Cleared`, dan `Blocked`, **belum memuat nilai `Revoked`**. Layanan pencatatan kepergian fisik (`RecordPatientDepartureAsync`) sama sekali belum memeriksa kelayakan keuangan (pemeriksaan baru ada pada `CloseEpisodeAsync`).<br>**Frontend:** Antarmuka kelayakan keuangan masih berupa formulir penandaan manual lokal (`IsManualMarking = true`) tanpa membaca status faktual dari Billing, dan belum memiliki reaksi otomatis (*Auto-Reblock*) bila clearance dicabut oleh kasir. | Pasien diperbolehkan keluar kamar dan meninggalkan rumah sakit secara fisik padahal status keuangan yang sebelumnya disetujui telah ditarik kembali oleh kasir akibat adanya tagihan farmasi atau tindakan dokter susulan. |
| `INT-CAP-06` | **Ketahanan Integrasi Transaksional Outbox & Kunci Idempotensi** (`RANAP-INT-006`, `RWI-DEC-161`, `RWI-AC-240`, `RWI-AC-241`) | `InPatientManagement` & `BillingManagement` | `BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Models/BilChargeReceipt.cs:11 #IdempotencyKey`<br>`BE@fe7e60d Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs:140 #FromSource`<br>`BE@fe7e60d Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomIntegrationService.cs:12` (Referensi Pola Outbox) | **Missing (Outbox Ranap) & Reuse with Adapter (Billing)** | **Rawat Inap:** Modul `InPatientManagement` belum memiliki entitas outbox (`InpIntegrationOutbox`), tabel database, maupun background worker pengirim event ke Billing.<br>**Billing:** Billing memiliki tabel penerima idempoten `BilChargeReceipt`, namun kolom `IdempotencyKey` dan parameter header HTTP bertipe data `Guid`. Diperlukan adapter konversi deterministik (misalnya hashing UUIDv5) dari format string bisnis `SourceDomain:SourceType:SourceDetailId:Version` yang ditetapkan `RWI-DEC-161`. | Gangguan jaringan atau downtime sementara pada modul Billing dapat menggagalkan proses penempatan tempat tidur atau pemulangan pasien di bangsal jika integrasi dilakukan secara panggilan langsung tanpa antrean outbox. |

---

### 18.3 Analisis Detail Proses Bisnis dan Contoh Kasus Rumah Sakit

Integrasi antara modul Rawat Inap dan Billing menjembatani dua dunia yang berbeda: **dunia fisik-klinis perawatan pasien di bangsal** dan **dunia transaksi keuangan di kasir/akuntansi**. Berikut adalah alur proses bisnis ujung-ke-ujung beserta contoh skenario konkret rumah sakit:

#### Skenario 1: Pendaftaran Admisi Pasien & Pemungutan Deposit di Muka
1. **Pemicu:** Pasien "Tn. Budi Santoso" diputuskan rawat inap dari Poli Penyakit Dalam. Petugas admisi membuka berkas admisi dan memilih paket kamar Kelas 1 (`InpEpisodeStatus.Draft`).
2. **Pengesahan Admisi (`Admitted`):** Begitu admisi disahkan dan diverifikasi, sistem Rawat Inap menerbitkan event `ADMISSION_CONFIRMED` ke tabel `InpIntegrationOutbox`.
3. **Akun Tagihan di Billing:** Worker integrasi mengirim data ke Billing. Billing membentuk akun tagihan (`BilInvoice`) berstatus `Open` yang terikat pada `EncounterId`.
4. **Pembayaran Uang Muka:** Keluarga Tn. Budi mendatangi kasir pendaftaran. Kasir membuka menu penerimaan deposit, membaca kebijakan minimal deposit Kelas 1 sebesar Rp 3.000.000 (`GET /deposit-policies`), dan mencatat pembayaran top-up via transfer bank sebesar Rp 5.000.000 (`POST /deposits/{encounterId}/top-ups`).
5. **Keadaan Arloji Kamar:** Meskipun deposit sudah diterima kasir, **arloji tagihan kamar harian BELUM berdetak**, karena Tn. Budi secara fisik masih berada di ruang tunggu admisi dan belum diantar ke ranjang perawatan.

#### Skenario 2: Pasien Tiba di Bangsal dan Timer Sewa Kamar Aktif
1. **Pemicu:** Tn. Budi diantar perawat ke Bangsal Melati Kamar 201 Bed A. Perawat bangsal menekan tombol **Konfirmasi Pasien Tiba (Check-in Bed)** pada pukul 10.30 WIB.
2. **Pencatatan Penempatan:** Rawat Inap menyimpan baris `InpBedPlacement` baru dengan status aktif, waktu mulai `StartDateTime = 10:30`, dan mengubah status bed master menjadi `Occupied`.
3. **Pemicu Room Charge:** Transaksi lokal menyimpan event `ROOM_ASSIGNED` ke `InpIntegrationOutbox`. Dispatcher mengirimkan data hunian ke Billing (`POST /invoices/from-source`).
4. **Perhitungan Billing:** Billing mencatat bahwa Tn. Budi menempati Bed A Kamar 201 sejak 10.30 WIB. Sesuai `MstRoomChargePolicy`, penghitungan tarif harian kamar mulai diperhitungkan secara berkala sejak detik tersebut.

#### Skenario 3: Koreksi Salah Catat Kamar oleh Supervisor Ruangan
1. **Masalah di Lapangan:** Pada pukul 14.00 WIB, Kepala Ruangan menyadari bahwa petugas admisi salah memilih kamar pada sistem: Tn. Budi tertulis di Kamar VIP, padahal fisiknya berada di Kamar Kelas 1.
2. **Pemeriksaan Syarat Koreksi (`RWI-DEC-157`):**
   - Pelaku adalah Kepala Ruangan (Supervisor) yang memiliki wewenang.
   - Sistem memeriksa status tagihan Tn. Budi di Billing masih terbuka (`BillingStatus == OPEN`).
3. **Eksekusi Koreksi:** Kepala Ruangan mengisi formulir koreksi: memilih Kamar Kelas 1, memasukkan alasan wajib: *"Koreksi salah penetapan tipe kamar oleh petugas admisi saat pendaftaran"*.
4. **Penyimpanan Audit & Reversi Tagihan:**
   - Data penempatan lama versi 1 dinonaktifkan dengan jejak audit lengkap (*tanpa hard delete*).
   - Data penempatan baru versi 2 diaktifkan dengan waktu efektif yang disesuaikan.
   - Rawat Inap menyimpan event `OCCUPANCY_CORRECTED` ke outbox dengan kunci `INPATIENT:ROOM_STAY:PLC-TnBudi:2`.
   - Modul Billing menerima event tersebut, membatalkan (*reversal*) kalkulasi biaya kamar VIP sebelumnya, dan menerapkan tarif baru untuk Kelas 1.

#### Skenario 4: Alur Pemulangan, Penarikan Clearance, dan Auto-Reblock
1. **Izin Pulang Dokter (DPJP):** Dokter Spesialis Penyakit Dalam memeriksa Tn. Budi pada pukul 09.00 WIB dan menerbitkan izin pulang klinis (`DischargeRequested` / `DischargePending`). Tempat tidur fisik **belum dilepas** karena pasien masih berkemas.
2. **Kesiapan Tagihan Kasir:** Kasir memeriksa kelengkapan tagihan melalui menu finalisasi (`GET /invoices/{invoiceId}/preview`). Pada pukul 10.00 WIB, keluarga Tn. Budi melunasi sisa tagihan obat dan kamar. Kasir menerbitkan status **Lunas** (`Financial Clearance: CLEARED`).
3. **Kasus Penarikan Clearance (Revocation):**
   - Pada pukul 10.15 WIB, instalasi farmasi menemukan resep obat pulang kronis susulan senilai Rp 750.000 yang belum masuk ke kalkulasi tagihan.
   - Petugas kasir mencabut persetujuan kepulangan dan mengubah status menjadi **Tertahan / Ditarik** (`Financial Clearance: REVOKED` / `BLOCKED`) dengan alasan: *"Ada transaksi obat kronis susulan belum diselesaikan"*.
4. **Penguncian Otomatis di Bangsal (*Auto-Reblock* - `RWI-DEC-158`):**
   - Pada pukul 10.20 WIB, perawat bangsal bersiap memulangkan Tn. Budi dan membuka menu pelepasan fisik pasien.
   - Sistem Rawat Inap mendeteksi status clearance telah berubah menjadi `REVOKED`. Tombol pelepasan tempat tidur **otomatis terkunci kembali**.
   - Di layar perawat bangsal muncul peringatan: *"Pasien belum dapat dipulangkan. Kendala Kasir: Ada transaksi obat kronis susulan belum diselesaikan."* **Tidak ada nominal rupiah yang ditampilkan ke perawat**, menjaga kerahasiaan finansial pasien.
   - Perawat mengarahkan keluarga pasien untuk kembali ke kasir menyelesaikan tagihan susulan tersebut.
5. **Pelepasan Fisik Pasien & Penghentian Sewa Kamar (`RWI-DEC-159`):**
   - Pukul 11.00 WIB, kasir memperbarui status menjadi `CLEARED` setelah pelunasan obat susulan.
   - Gerbang pelepasan di bangsal kembali terbuka. Tn. Budi meninggalkan ruangan bersama keluarga pada pukul 11.15 WIB.
   - Perawat menekan tombol **Catat Kepergian Fisik (Patient Departed)** pada pukul 11.15 WIB.
   - Rawat Inap mencatat `episode.PhysicallyLeftAt = 11:15` dan `placement.EndDateTime = 11:15`.
   - Event `BED_RELEASED` dikirim ke Billing dengan parameter `OccupancyEndAt = 11:15`. Billing mengunci durasi sewa kamar Tn. Budi berakhir tepat pada pukul 11.15 WIB, bukan pukul 09.00 WIB saat dokter visit.
   - Tempat tidur Bed A Kamar 201 resmi kembali berstatus kosong (*Available*) dan siap dibersihkan untuk pasien berikutnya.

---

### 18.4 Spesifikasi Teknis Endpoint Bergaya Swagger

Berikut adalah pemetaan spesifikasi endpoint integrasi yang relevan antara kedua modul:

#### A. Endpoint Modul Billing yang Dikonsumsi atau Diadaptasi
```csharp
[Tags("Health Services / Billing Management / Billing / Patient Funds")]
```
| Metode | Rute Endpoint | Deskripsi Bisnis | Hak Akses (*Permission*) | Payload Permintaan / Respons |
|---|---|---|---|---|
| `GET` | `/api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` | Membaca kebijakan deposit minimal berdasarkan penjamin dan kelas kamar | `BillingDeposit:Read` | **Query:** `guarantorId`, `patientClassId`<br>**Response 200:** `DepositPolicyResponse` (Status wajib, nominal minimal, toleransi hari) |
| `POST` | `/api/v1/health-services/billing-management/billing/patient-funds/deposits/{encounterId}/top-ups` | Mencatat setoran deposit pasien rawat inap di kasir loket admisi | `BillingDeposit:Create` | **Header:** `Idempotency-Key: Guid`<br>**Body:** `DepositTopUpRequest` (`PaymentMethodId`, `Amount`, `Reason`, `CorrelationId`)<br>**Response 200:** `SettlementResponse` |
| `GET` | `/api/v1/health-services/billing-management/billing/patient-funds/deposits/episodes/{episodeId}` | Membaca ringkasan saldo, total pemakaian, dan sisa deposit per episode | `BillingDeposit:Read` | **Response 200:** `EpisodeDepositSummaryResponse` (`TotalReceived`, `TotalAllocated`, `AvailableBalance`) |

```csharp
[Tags("Health Services / Billing Management / Billing / Invoices")]
```
| Metode | Rute Endpoint | Deskripsi Bisnis | Hak Akses (*Permission*) | Payload Permintaan / Respons |
|---|---|---|---|---|
| `POST` | `/api/v1/health-services/billing-management/billing/invoices/from-source` | Penerimaan beban tagihan kamar (*room charge*) dan tindakan dari producer | `BillingInvoice:Create` | **Header:** `Idempotency-Key: Guid`<br>**Body:** `UpsertChargeRequest` (`SourceDomain="ROOM_STAY"`, `SourceDetailId`, `SourceVersion`, `Quantity`, `UnitPrice`, `ContractVersion="BIL-INTEGRATION-0.4"`)<br>**Response 200:** `InvoiceDetailResponse` |

```csharp
[Tags("Health Services / Billing Management / Billing / Finalizations")]
```
| Metode | Rute Endpoint | Deskripsi Bisnis | Hak Akses (*Permission*) | Payload Permintaan / Respons |
|---|---|---|---|---|
| `GET` | `/api/v1/health-services/billing-management/billing/finalizations/invoices/{invoiceId}/preview` | Membaca checklist kesiapan penutupan tagihan beserta daftar kendala (*blocking reasons*) | `BillingFinalization:Read` | **Response 200:** `FinalizationPreviewResponse` (`AllOrdersComplete`, `Outstanding`, `IsReadyForNormalFinalization`, `BlockingReasons[]`) |

#### B. Endpoint Modul Rawat Inap yang Perlu Diperluas (*Target To-Be*)
```csharp
[Tags("Health Services / Inpatient Management / Bed Occupancy")]
```
| Metode | Rute Endpoint | Deskripsi Bisnis | Hak Akses (*Permission*) | Status As-Is & Rencana Penyesuaian |
|---|---|---|---|---|
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements` | Menempatkan pasien ke ranjang kamar rawat inap (`Bed Occupied`) | `InpatientBedOccupancy:Create` | **As-Is:** Menyimpan `InpBedPlacement` lokal.<br>**To-Be:** Tambahkan penulisan event `ROOM_ASSIGNED` ke `InpIntegrationOutbox` dalam satu transaksi lokal. |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements/transfer` | Memindahkan pasien ke tempat tidur atau kamar lain (mutasi) | `InpatientBedOccupancy:Transfer` | **As-Is:** Menutup penempatan lama dan membuka penempatan baru.<br>**To-Be:** Tambahkan penulisan event `ROOM_TRANSFERRED` ke `InpIntegrationOutbox`. |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements/{placementId}/correct` | Mengoreksi salah catat kamar/bed/kelas/waktu hunian (`RWI-DEC-157`) | `InpatientBedOccupancy:Correct` *(Baru)* | **As-Is:** Belum ada (`Missing`).<br>**To-Be:** Buat endpoint khusus Supervisor/Admisi, validasi `BillingStatus == OPEN`, catat versi baru, dan tulis event `OCCUPANCY_CORRECTED` ke outbox. |

```csharp
[Tags("Health Services / Inpatient Management / Inpatient Discharge")]
```
| Metode | Rute Endpoint | Deskripsi Bisnis | Hak Akses (*Permission*) | Status As-Is & Rencana Penyesuaian |
|---|---|---|---|---|
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/record-departure` | Mencatat waktu kepulangan fisik pasien dari tempat tidur | `InpatientDischarge:RecordDeparture` | **As-Is:** Mencatat `episode.PhysicallyLeftAt` dan melepas bed lokal.<br>**To-Be:** Tambahkan validasi kelayakan keuangan (*Auto-Reblock*), serta tulis event `BED_RELEASED` dengan `OccupancyEndAt = PhysicallyLeftAt` ke outbox. |
| `GET` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/billing-summary` | Mengambil ringkasan billing untuk bangsal dengan filter hak akses nominal rupiah (`RWI-DEC-160`) | `InpatientDischarge:Read` / `InpatientBilling:View` *(Baru)* | **As-Is:** Belum ada endpoint terpadu.<br>**To-Be:** Mengonsumsi data Billing; kembalikan status dan blocker untuk perawat, sertakan nominal rupiah hanya jika pemohon memiliki izin `InpatientBilling:View`. |

---

### 18.5 Fakta Baru, Pertanyaan Penutup, dan Penandaan Staleness

#### A. Fakta Baru dari Audit Lapangan (`INT-FACT`)
- **`INT-FACT-01` (Kesiapan Endpoint Deposit di Billing):** Endpoint kebijakan deposit `GET /deposit-policies` dan ringkasan episode `GET deposits/episodes/{episodeId}` pada `BillingPatientFundsController` di modul Billing **sudah berwujud dan aktif** di backend `fe7e60d4`. Hal ini membuktikan bahwa task frontend `FE-RWI-059` yang selama ini tertahan karena disangka endpoint-nya belum ada di backend, kini **resmi dapat dibuka dan dikerjakan**.
- **`INT-FACT-02` (Keterbatasan Domain Adapter di Billing):** Berkas `ContractBillingChargeSourceAdapter.cs` pada repositori backend baris 23–53 saat ini **hanya** menerima domain: `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSUMABLE`, `ADHOC`, dan `ADHOC_CATALOG`. Domain `ROOM_STAY` atau `INPATIENT` belum terdaftar. Jika modul Rawat Inap mengirim tagihan kamar hari ini, sistem Billing akan menolaknya dengan galat validasi 422. Penambahan `ROOM_STAY` menjadi prasyarat mutlak pada sisi Billing.
- **`INT-FACT-03` (Beda Tipe Kunci Idempotensi):** Tabel `BilChargeReceipt` dan parameter header `[FromHeader(Name = "Idempotency-Key")]` pada endpoint Billing mewajibkan tipe data `Guid`, sedangkan keputusan bisnis `RWI-DEC-161` menetapkan format string semantik: `SourceDomain:SourceType:SourceDetailId:Version`. Diperlukan adapter konversi deterministik (misalnya UUIDv5 berbasis namespace) pada dispatcher outbox Rawat Inap agar integritas kunci idempoten tetap terjaga.
- **`INT-FACT-04` (Absensi Status `Revoked` & Celah Pelepasan Pasien):** Enum `InpFinancialClearanceStatus` saat ini hanya mengenal nilai `Pending = 0`, `Cleared = 1`, dan `Blocked = 2`. Nilai `Revoked = 3` belum tersedia. Selain itu, fungsi `RecordPatientDepartureAsync` di `InpDischargeService.Closure.cs` hanya mengecek apakah status episode `DischargePending`, tanpa memverifikasi kelayakan keuangan sama sekali. Ini merupakan celah keselamatan administrasi yang harus ditutup pada perancangan integrasi.
- **`INT-FACT-05` (Formulir Frontend Deposit Belum Lengkap):** Komponen `inpatient-admission-deposit-step.jsx` dan hook `use-inpatient-admission-deposit.jsx` saat ini baru mengelola satu kolom input teks untuk nominal uang (`amount`), tanpa dropdown pemilihan metode pembayaran (`PaymentMethodId`). Padahal endpoint `POST /patient-funds/deposits/{encounterId}/top-ups` mewajibkannya sebagai kolom validasi utama.

#### B. Pertanyaan Penutup (Unknowns)
| ID | Pertanyaan | Mengapa Tidak Dapat Dijawab dari Kode Sumber Saja | Pemilik Kewenangan |
|---|---|---|---|
| `INT-UNK-01` | Apakah modul Billing Management (Yasmina) bersedia membuka endpoint penerimaan `POST /invoices/from-source` untuk menerima `SourceDomain = "ROOM_STAY"` dengan tipe payload occupancy harian, ataukah Billing lebih memilih memanggil service internal Inpatient secara in-process? | Menyangkut kontrak integrasi lintas modul antara tim Rawat Inap dan tim Billing. | Yasmina (Billing) & M. Hamzah (Rawat Inap) |
| `INT-UNK-02` | Apakah penarikan clearance (`REVOKED`) di Billing akan dipublikasikan sebagai event asinkron (misal webhook/message) ke Rawat Inap, ataukah Rawat Inap melakukan pemeriksaan live-check (*synchronous query*) sesaat sebelum perawat menekan tombol pelepasan fisik pasien? | Menyangkut pola arsitektur runtime dan latensi komunikasi antar-service. | Arsitek Sistem bersama Yasmina & M. Hamzah |

#### C. Penandaan Ketinggalan Dokumen (*Staleness Trigger*)
Bagian 18 ini menjadi **basi (*STALE*)** dan wajib diaudit ulang apabila salah satu kondisi berikut terpenuhi:
1. Perubahan pada repositori backend `NewQuilvianSystemBackend`:
   - `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs` atau `InpEpisode.cs`;
   - `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` atau `InpatientDischargeController.cs`;
   - `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` atau `BillingInvoiceService.cs`;
   - `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs` atau `BillingFinalizationsController.cs`.
2. Perubahan pada repositori frontend `QuilvianSystemFrontendDev`:
   - `src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx`;
   - `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx`;
   - `src/components/view/health-services/inpatient-management/inpatient-financial-clearance-view.jsx`.
3. Terjadinya perubahan keputusan bisnis atau persetujuan wawancara pada Pass B bersama pemilik modul Billing (`BillingManagement`).


---

## 19. Impact scan terfokus — Finishing Rawat Inap `PRD-RWI-FINISHING-001` — 1 Oktober 2026

### 19.1 Identitas, masukan, dan batas audit

| Field | Nilai |
|---|---|
| Pemicu | Amendment Pass `PRD-RWI-FINISHING-001` tuntas 2026-10-01 (`00-interview-decisions.md` revision `27`, `RWI-DEC-163` s.d. `RWI-DEC-183`). Pass itu menyarankan langkah lanjutan karena bagian 18 sudah basi, audit PRD belum diserap, dan `RWI-OQ-101` butir (a)–(c) perlu jawaban dari source |
| Masukan bisnis | `00-interview-decisions.md` revision `27`; `PRD-RWI-FINISHING-001` v`0.1`, SHA-256 `047a43aaefa6965d378bcd3990d075067d07a359f91d91bdb612f5a3fddb4c29` |
| Backend | `NewQuilvianSystemBackend` branch `MHamzah`, commit **`c8e99ce54a677f512d2a7ae80432545e1b650248`** (`BE@c8e99ce`). Berjarak **347** commit dari snapshot bagian 18 (`fe7e60d4`). Selisih itu menyentuh 118 berkas di `InPatientManagement` dan `BillingManagement` |
| Frontend | `QuilvianSystemFrontendDev` branch `HamzahV2`, commit **`22ad6733096b9eb257fd9ae2af17df5ad25d8967`** (`FE@22ad673`). Hanya berselisih satu commit (URL API lokal dan CSP) dari `7a83e8574` yang diaudit PRD |
| Pembanding V1 | Tidak dibaca ulang. Bukti V1 yang dipakai berasal dari PRD bagian 2 dan 13 serta `RWI-FACT-026` |
| Klaster yang diaudit | Financial (outbox, invoice, tarif kamar, biaya admin, jembatan klinis), Workflow/Status (gerbang pulang, override), Authorization (hak lihat rupiah, PIN), Order/Result (Lab, Radiologi, Gizi, Bank Darah, Diet, Kamar Operasi), Documentation/Record (Catatan Keperawatan, WSD, Efek Samping Obat, serah terima), dan Master (tarif, alat, katalog tindakan) |
| Di luar audit | Hemodialisa dan Rehab Medik, karena statusnya tidak berubah menurut PRD dan keputusan; isi klinis V1; lingkungan berjalan; dan data role-permission di database |
| Cara audit | Pembacaan statis memakai `rg`, lalu membaca implementasi yang relevan: controller dan atribut hak akses, service, model, enum, registrasi DI pada `Program.cs`, service API, dan komponen frontend. **Tidak ada build, test, migration, maupun eksekusi aplikasi** |
| Batas tulis | Hanya bagian ini dan baris kepala dokumen. Tidak ada satu baris source aplikasi yang diubah |

**Status bagian 18.** Bagian 18 dinyatakan **`STALE`** untuk butir yang ditimpa bagian ini:

| Klaim bagian 18 pada `fe7e60d4` | Keadaan pada `BE@c8e99ce` |
|---|---|
| `INT-CAP-06`: Rawat Inap belum punya outbox | Outbox, tabel, dan worker **sudah ada**, tetapi pengirimnya palsu (`FIN-CAP-01`) |
| `INT-FACT-02`: Billing belum menerima domain `ROOM_STAY` atau `INPATIENT` | Kedua domain **sudah diterima** `BillingChargeSourceAdapter.cs:78-85` (`FIN-FACT-01`) |
| `INT-FACT-04`: `RecordPatientDepartureAsync` tidak memeriksa kelayakan keuangan | **Masih benar**, dan kini ada jalur pulang kedua yang memeriksa salinan status (`FIN-CAP-09`) |
| Endpoint target `GET discharges/{episodeId}/billing-summary` | Tidak dibangun. Yang ada justru `GET episodes/{episodeId}/billing-status` dan `billing-details` (`FIN-CAP-13`) |

### 19.2 Ringkasan untuk pembaca umum

Bayangkan pasien samaran Budi dirawat tiga hari lalu pulang. Begini perjalanannya di sistem hari ini:

1. **Admisi disahkan.** Rawat Inap menulis pesan `ADMISSION_CONFIRMED` ke outbox. Worker mengambilnya, menulis satu baris log, lalu menandainya "Published". **Billing tidak pernah menerima apa pun**, sehingga invoice rawat inap Budi tidak terbuka.
2. **Budi menerima tindakan, lab, dan obat.** Setiap layanan tercatat di folio Billing, karena pencatat tagihan klinis tidak membedakan rawat jalan dan rawat inap. Namun jembatan folio ke invoice **menolak** kunjungan Budi dengan alasan "Kunjungan bukan Rawat Jalan".
3. **Billing sebenarnya sudah bisa menghitung tarif kamar** dari linimasa bed dan kebijakan tarif kamar. Hitungan itu baru muncul setelah invoice ada, padahal invoice tidak pernah ada.
4. **Perawat membuka Tagihan Pasien.** Angkanya diambil dari folio, sehingga tarif kamar dan biaya admin tidak ikut. Rupiahnya tampil kepada setiap pemegang `PatientBillingSummary : Read`.
5. **Kasir menilai izin pulang.** Billing menyimpan hasilnya di `BilInpatientClearanceHandoff`, tetapi Rawat Inap membaca salinannya sendiri di `InpEpisode.ClearanceStatus`. Salinan itu hanya berubah lewat webhook tanpa login yang tidak pernah dipanggil Billing.
6. **Akibatnya, tombol "Konfirmasi Pasien Pulang Fisik" tidak pernah aktif lewat jalur normal.** Namun ada **pintu belakang**: endpoint lama `record-departure` dapat memulangkan Budi tanpa memeriksa izin kasir sama sekali.

Kesimpulannya, kerangka integrasinya sudah ada, tetapi sambungannya putus di beberapa titik.

### 19.3 Capability evidence map

Format bukti: `repo@sha path:baris #simbol`. Setiap baris memakai tepat satu status dari kontrak bukti kemampuan.

#### A. Tagihan dan izin pulang — sub-modul `integrasi-billing`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-01` | Outbox Rawat Inap → Billing sebagai ketukan pintu dengan konfirmasi terima (`CAP-RWF-04`, `RWI-DEC-166`, `RWI-DEC-161`) | `InPatientManagement` | `BE@c8e99ce Program.cs:602, 931` (service dan hosted worker terdaftar); `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs:67-70` (hanya mengambil `Pending` dan `Failed`), `:85-104` (`MarkPublished` setelah dispatch), `:129-145 #DispatchEventAsync` (hanya `LogDebug`); `Services/InpatientClearanceGateService.cs:215-224` (payload `BED_RELEASED` membawa `RoomId`, `RoomClassId`, `OccupancyStartAt`, `OccupancyEndAt`, dan `ClearanceStatus`); `Services/InpBedOccupancyService.cs:1156-1167` (payload `OCCUPANCY_CORRECTED` membawa ruang dan kelas lama/baru serta alasan) | **Repair** | (1) Penerima nyata di Billing beserta konfirmasi terima. (2) Isi payload dipangkas menjadi penanda kejadian (`RWI-DEC-166` butir 2). (3) Pesan yang tertinggal berstatus `Processing`, misalnya karena aplikasi mati di tengah pengiriman, tidak pernah diambil ulang | Status "Published" palsu menyesatkan audit dan roadmap (`RWI-DEC-168`). Pesan bisa tersangkut selamanya di `Processing` |
| `FIN-CAP-02` | Invoice `RANAP` terbuka saat admisi (`CAP-RWF-02`, `RWI-DEC-156`, `RWI-DEC-166` butir 3) | `BillingManagement` | `BE@c8e99ce Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:1528-1547` (satu-satunya tempat invoice dibuat, yaitu saat kasir menambah biaya); `:2064-2072 #MapServiceType` (`Inpatient` → `"RANAP"`) | **Missing** | Penerima `ADMISSION_CONFIRMED` yang membuka invoice. Wewenang Yasmina (`RWI-OQ-098`) | Tanpa invoice, tarif kamar, biaya admin, dan izin pulang tidak pernah terbentuk |
| `FIN-CAP-03` | Tarif kamar dari linimasa bed (`BKC-DEC-043`, `FR-RWF-012`) | `BillingManagement` | `BE@c8e99ce .../Billing/Services/BillingCalculationService.cs:196-198` (hanya untuk `ServiceType == "RANAP"`), `:607-741 #CalculateRoomChargeAsync` (semua `InpBedPlacement` yang tidak dihapus, `MstRoomChargePolicy` aktif dengan `MinimumMinutes`, `PeriodMinutes`, dan `RemainderRounding`, serta `MstTariff.IsRoomCharge`) | **Ready to reuse** | Siap dipakai begitu `FIN-CAP-02` ada. Tidak ada angka tarif tertanam | Lihat catatan `IsSuperseded` pada `FIN-CAP-15` |
| `FIN-CAP-04` | Biaya administrasi rawat inap (`FR-RWF-015`) | `BillingManagement` | `BE@c8e99ce .../BillingCalculationService.cs:189-195` (dihitung hanya bila `activeItems.Count > 0`), `:515-540` (dasar biaya non-farmasi; tanggal evaluasi = `PhysicallyLeftAt ?? DischargeDecidedAt`) | **Extend** | Tarif kamar dihitung terpisah dari item, sehingga invoice yang baru berisi tarif kamar **tidak** mendapat biaya admin. `RSK-RWF-04` terbukti. Aturannya wewenang Yasmina (`RWI-OQ-103` butir c) | Tagihan kurang untuk pasien yang hanya menginap tanpa layanan bertarif |
| `FIN-CAP-05` | Layanan klinis rawat inap masuk invoice (`FR-RWF-011`) | `BillingManagement`, dengan produsen di modul klinis | Produsen tagihan tidak menyaring jenis kunjungan: `BE@c8e99ce Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs:554 #AcceptAsync` → `:675`; `RadiologyManagement/Services/RadStudyService.cs:490 #DecideQualityAsync` → `:537`; `PharmacyManagement/Services/PrescriptionDispensingService.cs:276 #DispenseAsync` → `:389`; juga `ClinicalManagement/Controllers/PatientProcedureController.cs`, `BloodBankManagement/Services/BbkBloodBankProcedureService.cs`, dan `HemodialysisManagement/Services/HmdBillingHandoffService.cs`. Penolakan di jembatan: `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs:111-112` | **Extend** | Jembatan folio→invoice diperluas untuk `RANAP`. Wewenang Yasmina (`RWI-OQ-103` butir a) | Layanan rawat inap tercatat di folio, tetapi tidak pernah sampai ke invoice kasir |
| `FIN-CAP-06` | Satu hitungan tarif kamar, tanpa angka tertanam (`FR-RWF-013`, `AC-RWF-014`) | `BillingManagement` | `BE@c8e99ce .../Billing/Services/InpatientRoomChargeCalculationService.cs:16-19` (jam potong 18.00, 22.00, dan 12.00 tertanam), `:242` (cadangan Rp 500.000), `:449` (cadangan Rp 1.500.000). Hanya dipakai `Billing/Controllers/InpatientClearanceController.cs:55-86` `POST invoices/occupancy-charges`. `FE@22ad673`: **nol** pemanggil | **Conflict** | Ada dua hitungan tarif kamar untuk fakta yang sama. Pensiunnya wewenang Yasmina (`RWI-OQ-103` butir e) | Tagihan karangan bila endpoint itu dipanggil |
| `FIN-CAP-07` | Status izin pulang sebagai satu sumber yang dibaca langsung (`CAP-RWF-01`, `RWI-DEC-167`) | `BillingManagement` | `BE@c8e99ce .../Billing/Models/BilInpatientClearanceHandoff.cs:12-72` (menyimpan status, alasan, dan nominal `OutstandingBalance`, `TotalPatientResponsibility`, `TotalPaidOrAllocated`), `:75-81` (`PENDING`, `BLOCKED`, `CLEARED`, `REVOKED`); `Billing/Services/InpatientClearanceService.cs:30 #EvaluateClearanceAsync` (invoice tidak ada → `PENDING`, `:64-66`), `:332 #GetLatestClearanceForEncounterAsync`; `Billing/Controllers/InpatientClearanceController.cs:277-284` `GET inpatient-clearance/encounter/{encounterId}/latest` (`BillingInpatient : ReadLatest`) | **Reuse with adapter** | Rawat Inap belum membacanya. Bacaan untuk perawat harus membuang field rupiah dan hanya menyisakan status serta kendala (`RWI-DEC-160`). Wewenang Yasmina (`RWI-OQ-099`) | — |
| `FIN-CAP-08` | Penguncian ulang otomatis saat ada tagihan susulan (`RWI-DEC-158`) | `BillingManagement` | `BE@c8e99ce .../InpatientClearanceService.cs:263`, `BilConsumerHandoffService.cs:753`, dan `BillingInvoiceService.cs:1622, 1674` memeriksa `ServiceType == "INPATIENT"`. Padahal invoice rawat inap berlabel `"RANAP"` (`BillingInvoiceService.cs:2068`, `BillingRefundService.cs:18`, `MasterData/DTOs/AdministrationFeePolicyDtos.cs:10`) | **Repair** | Kosakata `ServiceType` diseragamkan. Wewenang Yasmina | **Pencabutan otomatis tidak pernah terjadi.** Pasien yang sudah `CLEARED` tetap `CLEARED` walaupun resep susulan masuk |
| `FIN-CAP-09` | Satu gerbang pulang fisik yang membaca Billing (`CAP-RWF-01`, `CAP-RWF-03`, `RWI-DEC-167`) | `InPatientManagement` | **Jalur 1:** `BE@c8e99ce Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs:132-147` `POST episodes/{episodeId}/confirm-physical-discharge` → `Services/InpatientClearanceGateService.cs:154-156`. Jalur ini memeriksa salinan `InpEpisode.ClearanceStatus` (`Models/InpEpisode.cs:68`), yang hanya ditulis webhook (`:59`, `:72`) dan override (`:124`). **Jalur 2:** `Controllers/InpatientDischargeController.cs:639-652` `POST discharges/{episodeId}/record-departure` → `Services/InpDischargeService.Closure.cs:574-630 #RecordPatientDepartureAsync`, yang hanya memeriksa status `DischargePending`, **tanpa** izin kasir. Penutupan episode membaca tanda manual: `InpDischargeService.Closure.cs:226-273 #GetFinancialClearanceAsync`, `:921` | **Conflict** | Ada tiga sumber status dan dua jalur pulang fisik. Keputusan sumbernya sudah ada (`RWI-DEC-167`), tetapi jalur mana yang dipertahankan belum diputuskan (`FIN-UNK-03`) | **Pintu belakang:** pasien dapat dipulangkan fisik lewat `record-departure` tanpa izin kasir |
| `FIN-CAP-10` | Endpoint pengubah izin pulang wajib login (`FR-RWF-001`) | `InPatientManagement` | `BE@c8e99ce .../InpatientDischargeClearanceController.cs:49-53`: `[AllowAnonymous]` pada `POST episodes/{episodeId}/discharge-clearance/webhook` | **Repair** | Dihapus menurut `RWI-DEC-167` butir 3 | Siapa pun yang tahu alamatnya dapat mengubah status izin pulang |
| `FIN-CAP-11` | Supervisor override darurat (`FR-RWF-005`, `RWI-DEC-015`) | `InPatientManagement` | `BE@c8e99ce .../InpatientClearanceGateService.cs:108-113` (alasan minimal 20 karakter), `:116-121` (PIN **hanya diperiksa tidak kosong**, tidak pernah dicocokkan), `:122-131` (override menempel permanen pada episode); `InpatientDischargeClearanceController.cs:93` `AccessPermission("InpatientDischargeClearance", "SupervisorOverride")`, ditambah `:99-101` (pemeriksaan nama peran `"SuperAdmin"` dan klaim `InpatientSupervisor:Override`) | **Repair** | PIN diverifikasi sungguhan atau dihapus, dan hak akses hanya lewat permission (`PR-RWF-07`). Lihat `FIN-UNK-04` | Pesan error menyebut "PIN … salah", padahal PIN apa pun diterima |
| `FIN-CAP-12` | Layar gerbang pulang menyegarkan status tiap 10 detik (`FR-RWF-003`) | `InPatientManagement` (FE) | `FE@22ad673 src/components/features/health-services/inpatient-management/billing-integration/discharge-clearance-gate-card.jsx:54, 86` (`pollingIntervalMs = 10000`), `:96-108` (membaca `CLEARED`, `REVOKED`, `OVERRIDDEN`); `src/lib/services/health-services/inpatient-management/inpatient-billing.service.js:7-9` `GET episodes/{episodeId}/billing-status`. Backend `InPatientManagement/Services/InpatientBillingQueryService.cs:56-107` membaca salinan `InpEpisode.ClearanceStatus` | **Extend** | Penyegarannya sudah benar. Sumber datanya harus diganti ke bacaan Billing (`FIN-CAP-07`) | Layar tidak pernah melihat persetujuan kasir |
| `FIN-CAP-13` | Tagihan Pasien di bangsal: rincian per kelompok tanpa rupiah, dan subtotal untuk pemegang izin (`CAP-RWF-05`, `CAP-RWF-15`, `RWI-DEC-170`) | `BillingManagement`, dengan permukaan `InPatientManagement` | Ada empat jalur paralel. (1) `BE@c8e99ce Areas/HealthServices/BillingManagement/Operational/Controllers/PatientBillingSummaryController.cs:62` (`PatientBillingSummary : Read`) → `Operational/Services/PatientBillingSummaryService.cs:95-189`: total dari `BilFolio`, dan rupiah tampil kepada semua pemegang hak baca. (2) `InPatientManagement/Controllers/InpatientBillingOperationalController.cs:48-57` `billing-status`: salinan status. (3) `:73-104` `billing-details` (`ViewBillingDetails`) → `InpatientBillingQueryService.cs:120-175`: **Rawat Inap membaca langsung tabel Billing** `BilFolio` dan `BilChargeLine`. (4) `BillingManagement/Billing/Controllers/InpatientClearanceController.cs:111-135` `inpatient-summary`: hak rupiah dari nama peran `Contains("Admin")`, `"Cashier"`, `"Finance"`. FE: `src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx:218, 279, 285, 374-399` menampilkan rupiah dengan cadangan "Rp 0" | **Conflict** | Satu sumber rincian dari hitungan invoice Billing, dengan rupiah disaring di server. Saat ini tidak satu pun jalur memuat tarif kamar, biaya admin, atau kelompok ala V1 | Perawat melihat rupiah, bertentangan dengan `RWI-DEC-160` dan `RWI-DEC-170`. Angka yang tampil juga lebih kecil dari tagihan sebenarnya |
| `FIN-CAP-14` | Putar ulang terkontrol untuk episode aktif saat rilis (`RWI-DEC-169`) | `InPatientManagement` | `BE@c8e99ce`: pencarian `replay`, `redispatch`, dan `requeue` di `InPatientManagement` mengembalikan nol hasil. Pola pembanding: `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs:71 #RedispatchAsync` | **Missing** | Mekanisme putar ulang dengan `IdempotencyKey` yang stabil | — |
| `FIN-CAP-15` | Koreksi salah catat penempatan memicu hitung ulang (`RWI-DEC-157`, `RWI-DEC-166` butir 3) | `InPatientManagement` | `BE@c8e99ce .../Services/InpBedOccupancyService.cs:1086-1100`: transfer biasa mengisi `EndReason = Transfer`, `IsSuperseded = true`, dan `SupersededAtUtc`. `:1156-1170`: event `OCCUPANCY_CORRECTED` terbit **pada setiap transfer**. `Models/InpBedPlacement.cs:51 IsSuperseded`. Hitungan Billing tidak menyaring `IsSuperseded` (`BillingCalculationService.cs:622-625`) | **Missing** | Belum ada jalur koreksi salah catat. Yang ada hanya transfer yang berlaku mulai "sekarang", sehingga periode salah kelas tetap tertagih di kelas yang salah | Bila koreksi kelak dibuat dengan `IsSuperseded` tanpa menutup baris lama, tarif kamar menjadi dobel (`RSK-RWF-02`) |

#### B. Penunjang dari bangsal

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-16` | Perawat memesan Lab dan Radiologi atas instruksi dokter (`CAP-RWF-06`, `RWI-DEC-114`) | `LaboratoryManagement` dan `RadiologyManagement`, dengan permukaan `keperawatan` | **Backend siap:** `BE@c8e99ce Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs:150-157` dan `RadiologyManagement/Models/RadOrder.cs:120-127` (`InstructingDoctorId`, `InstructionVerificationStatus`, `InstructionVerifiedAt`, `InstructionVerifiedByUserId`); `LaboratoryManagement/Services/LabOrderService.cs:405-432 #ResolveInstructionAsync` (dokter tanpa penugasan ditolak 403); daftar verifikasi di `LabOrderController.cs:354` dan `RadOrderController.cs:158`. **Frontend terkunci:** `FE@22ad673 .../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:189-205` (`disabled={true}`, dengan komentar "menunggu BE-RWI-104") | **Repair** | Kunci tombol pesan di frontend dibuka, lalu alurnya dibuktikan berjalan (`RWI-DEC-168` butir 3) | — |
| `FIN-CAP-17` | Konsultasi Gizi dari bangsal (`RWI-DEC-171`) | `NutritionManagement`, dengan permukaan `dokter-rawat-inap` dan `keperawatan` | `BE@c8e99ce Areas/HealthServices/NutritionManagement/Controllers/NutritionOrderController.cs:79` `POST orders` (`NutritionOrder : Create`); `Services/NutritionOrderService.cs:146-190` (penginput = `CreateBy` dari akun login, dan `RequesterDoctorId` disimpan terpisah); `:471-490 #ValidateReferencesAsync` (dokter hanya diperiksa ada dan aktif di `MstDoctors`). Frontend masih *placeholder*: `nursing-ancillary-section.jsx:35-39` dan `.../physician-workspace/tabs/supporting-service/supporting-service-tab.jsx:164-167` | **Reuse with adapter** | Adapter Rawat Inap memeriksa penugasan dokter peminta lewat `ClinicalManagement/Services/InpatientClinicalContextService.cs:1060 #IsDoctorAssignedAsync` sebelum meneruskan pesanan. **Tidak ada kolom status verifikasi** di modul Gizi (`FIN-UNK-01`) | Pesanan atas nama dokter yang tidak bertugas lolos bila adapter tidak memeriksa |
| `FIN-CAP-18` | Pesanan darah dari bangsal (`RWI-DEC-171`) | `BloodBankManagement`, dengan permukaan yang sama | `BE@c8e99ce Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs:742` (dokter peminta wajib), `:834` (dokter hanya diperiksa ada), `:918-929` (penginput = `CreateBy`), `:506 #ConfirmDuplicateAsync` (pesanan ganda wajib beralasan). Frontend *placeholder* seperti `FIN-CAP-17` | **Reuse with adapter** | Sama dengan `FIN-CAP-17`: adapter memeriksa penugasan, dan tempat status verifikasi belum ada (`FIN-UNK-01`) | Darah adalah produk berisiko tinggi. Tanpa pemeriksaan penugasan, nama dokter mana pun dapat dipakai |
| `FIN-CAP-19` | Diet Medis di bangsal (`CAP-RWF-12`, `RWI-DEC-178`) | `NutritionManagement`, dengan permukaan `keperawatan` | `BE@c8e99ce .../NutritionManagement/Controllers/NutritionDietController.cs:46-138` (`NutritionPatientDiet : Read` dan `: Update`); `Services/NutritionDietService.cs:180-217` (mengganti diet aktif wajib beralasan, kode `GIZ010`; diet lama ditutup sebagai riwayat; `PrescribedByWorkforceId` dan `CreateBy` disimpan terpisah). FE: `src/lib/services/health-services/nutrition-management/nutrition-diet.service.js` sudah ada, tetapi tidak ada layar bangsal | **Reuse with adapter** | Layar bangsal dan pemeriksaan penugasan penetap. Tempat status verifikasi sama dengan `FIN-UNK-01` | — |
| `FIN-CAP-20` | Daftar "perlu diverifikasi" dokter untuk pesanan yang diinput perawat (`RWI-DEC-114` butir 5) | Modul pemilik pesanan masing-masing | `BE@c8e99ce .../ClinicalManagement/Controllers/PatientProcedureController.cs:536-559` (tindakan), `LabOrderController.cs:354`, `RadOrderController.cs:158`, dan `PatientIntegratedProgressNoteController.cs:1147` (CPPT). FE: `src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx:10-19` (`getInstructionVerificationWorklist`, `verifyProcedureInstruction`) | **Reuse with adapter** | Polanya sudah ada per modul, tetapi tersebar di empat endpoint. Gizi, Bank Darah, dan Diet belum punya | Dokter harus membuka banyak tempat untuk memverifikasi |

#### C. Pasien operasi dari bangsal

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-21` | Pesan ruang bedah dari bangsal dengan merujuk order tindakan, termasuk Obgyn dan jenis anestesi (`CAP-RWF-07`, `RWI-DEC-175`, `RWI-DEC-176`) | `OperatingRoomManagement` | `BE@c8e99ce Areas/HealthServices/OperatingRoomManagement/DTOs/OperatingRoomCaseDtos.cs:24-42 CreateOprCaseRequest` (`PatientId`, `EncounterId`, `RequesterDoctorId`, `PrimarySurgeonId`, `CaseType`, `Priority`, `Indication`, `Laterality`, `EstimatedMinutes`, `PreferredAt`, `Procedures[].PatientProcedureId`); `Enums/OperatingRoomEnums.cs:4` (`OprCaseType` hanya `Elective` dan `Emergency`); `Controllers/OperatingRoomCaseController.cs:49-55` `POST cases` (`OperatingRoomCase : Create`). FE bangsal masih *placeholder*: `.../nursing-workspace/components/nursing-workspace-sections.jsx:90-99` | **Extend** | Rujukan ke order tindakan **sudah ada** lewat `PatientProcedureId`. Yang belum ada: jenis anestesi dan jenis layanan bedah "Obstetri". Wewenang pemilik OK (`RWI-OQ-100`) | — |
| `FIN-CAP-22` | Status kasus OK tampil di bangsal (`FR-RWF-044`) | `OperatingRoomManagement`, dengan permukaan `episode-rawat-inap` | `BE@c8e99ce .../Enums/OperatingRoomEnums.cs:3` (`Requested`, `Scheduled`, `Ready`, `InProgress`, `Completed`, `Postponed`, `Cancelled` — persis tujuh status PRD); `OperatingRoomCaseController.cs:26-40` `GET cases` dan `GET cases/{id}` (`OperatingRoomCase : Read`). FE Rawat Inap: nol rujukan ke modul OK | **Reuse with adapter** | Layar bangsal memanggil daftar kasus dengan saringan kunjungan | — |
| `FIN-CAP-23` | Persetujuan tindakan dan anestesi diperiksa sebelum "Siap" (`RWI-DEC-176` butir 5) | `OperatingRoomManagement` | `BE@c8e99ce .../Services/OperatingRoomPreparationService.cs:36-43` (wajib consent `Surgery` dan `Anesthesia` berstatus `Signed`, `Verified`, atau `Approved`), `:282` | **Ready to reuse** | — | — |
| `FIN-CAP-24` | Catatan Pra-Operasi bangsal dengan pengirim dan penerima berbeda akun (`CAP-RWF-08`, `RWI-DEC-173`, `RWI-DEC-174`) | `OperatingRoomManagement` | `BE@c8e99ce .../Enums/OperatingRoomEnums.cs:9` (`OprReadinessRole`: `PrimarySurgeon`, `Anesthesiologist`, `Nurse`), `:10` (`OprChecklistPhase` hanya `SignIn`, `TimeOut`, `SignOut` versi WHO) | **Missing** | Fase baru, penandaan gambar tubuh, dan gerbang "Siap" yang membacanya. Wewenang pemilik OK | — |
| `FIN-CAP-25` | Serah terima pasca operasi diterima bangsal tanpa memindahkan bed (`CAP-RWF-08`, `RWI-DEC-177`) | `OperatingRoomManagement`, dengan permukaan `episode-rawat-inap` | `BE@c8e99ce .../Models/OprHandover.cs:8-22` (`DestinationUnitId`, `Status` Draft/Sent/Accepted/Rejected, `ConditionSummary`, `InstructionSummary`, `SentBy`, `ReceivedBy`, `RejectionReason`, `Revision`); `Services/OperatingRoomRecoveryService.cs:317-360 #AcceptHandoverAsync` (penolakan wajib beralasan, `ReceivedBy` dari akun login, **tanpa** pemeriksaan unit penerima maupun tempat tidur); `Controllers/OperatingRoomRecoveryController.cs:99-144` (`OperatingRoomHandover : Read` dan `: Update`, permission yang sama untuk pengirim dan penerima). FE Rawat Inap: nol rujukan | **Extend** | Bangsal membaca serah terima untuk unitnya. Penerimaan dikunci bila pasien belum menempati bed di unit tujuan (`RWI-DEC-177` butir 5). Sisi OK wewenang pemilik OK | Perawat OK yang memegang `Update` bisa sekaligus "menerima" atas nama bangsal |
| `FIN-CAP-26` | Biaya operasi masuk invoice saat `Completed` (`FR-RWF-047`) | `OperatingRoomManagement` dan `BillingManagement` | `BE@c8e99ce .../Services/OperatingRoomIntegrationService.cs:28-40` (`BlockedDestinations = [BillingDestination]`, dengan komentar "kontraknya belum tersedia") | **Missing** | Kontrak OK→Billing. Wewenang pemilik OK dan Yasmina (`RWI-OQ-100` butir f) | Operasi tidak tertagih |

#### D. Ruang kerja keperawatan dan master

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-27` | Catatan Keperawatan dengan enam sub-menu V1, sedangkan narasi tetap entri CPPT (`CAP-RWF-09`, `RWI-DEC-172`) | `keperawatan` (FE), dengan data di `ClinicalManagement` | `FE@22ad673 src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js:61-72` (sub-tab Asuhan Keperawatan: Vital Sign, SOAP, Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, **Catatan Keperawatan sebagai narasi**, Rencana Asuhan); `.../nursing-care/tabs/nursing-narrative-tab.jsx:81`. Data narasi siap: `BE@c8e99ce Areas/HealthServices/ClinicalManagement/Enums/CpptNoteKind.cs:45` (`NursingNarrative = 3`), `Models/TrxPatientIntegratedProgressNote.cs:99 NoteKind` | **Extend** | Susunan menu diubah. Saringan "Naratif Keperawatan" pada Catatan Terintegrasi memakai `NoteKind` yang sudah ada | — |
| `FIN-CAP-28` | Obat & Alkes kembali ke empat sub-tab V1 (`RWI-DEC-172` butir 3) | `keperawatan` (FE) | `FE@22ad673 .../nursing-workspace/sections/medication/nursing-medication-section.jsx:24-31` (delapan sub-tab: Resep, Resep Harian, Alat Kesehatan, Summary Alkes, MAR, Sliding Scale, Obat Bawaan, Pemakaian Alkes) | **Extend** | MAR, Sliding Scale, dan Obat Bawaan dipindah. "Pemakaian Alkes" digabung ke Summary | — |
| `FIN-CAP-29` | Observasi WSD per shift dengan hitungan server (`CAP-RWF-10`, `FR-RWF-054`) | `ClinicalManagement` | `BE@c8e99ce Areas/HealthServices/ClinicalManagement/Enums/FluidSourceCategory.cs:35-36` (`DrainOrWsd = 14`). `Controllers/FluidBalanceController.cs` tidak punya endpoint pembacaan tabung WSD (pencarian `wsd` nol hasil) | **Extend** | Pembacaan tabung per shift dan hitungan "bertambah" di server, lalu dicatat sebagai output `DrainOrWsd` | — |
| `FIN-CAP-30` | Layar Efek Samping Obat dari dosis MAR (`CAP-RWF-11`) | `ClinicalManagement` | `BE@c8e99ce .../ClinicalManagement/Controllers/PatientAllergyController.cs:455-463` `POST patient-allergies/from-medication-administration` (`PatientAllergy : Create`). `FE@22ad673`: nol pemanggil | **Reuse with adapter** | Layar di Daftar Pemberian Obat | — |
| `FIN-CAP-31` | Pemakaian Alat dengan master jenis alat dan tarif (`CAP-RWF-13`, `RWI-DEC-179`, `RWI-DEC-180`) | `MasterData` dan `ClinicalManagement` | `BE@c8e99ce Areas/HealthServices/MasterData/Models/MstTariff.cs:19-42` (rujukan `TariffCategoryId`, `ServiceUnitId`, `ClinicId`, `PatientClassId`, `ProcedureId`, `DrugId`, serta `IsRoomCharge` dan `NormalPrice`; **tanpa** rujukan alat dan **tanpa** kolom penjamin). Tidak ada model master alat medis. FE *placeholder*: `.../nursing-workspace/components/nursing-workspace-sections.jsx:75-84` | **Missing** | Master jenis alat, rujukan alat pada master tarif, catatan pemakaian, dan layar. Cara menetapkan harga per penjamin belum terbaca dari `MstTariff` (`FIN-UNK-02`) | — |
| `FIN-CAP-32` | Katalog tindakan khusus rawat inap (`CAP-RWF-14`) | `ClinicalManagement` | `BE@c8e99ce .../ClinicalManagement/Controllers/PatientProcedureController.cs:128-136` (saringan `IsAvailableForOutpatient` dan `IsDoctorAction`); `MasterData/Models/MstProcedure.cs:65, 67` (`IsAvailableForInpatient` sudah ada, bawaan `true`). FE: `use-inpatient-procedure-tab.jsx` memanggil `getPatientProcedureMasterOptions` | **Repair** | Saringan diganti ke penanda rawat inap. **Inferensi:** saringan `IsDoctorAction` juga menyembunyikan tindakan khusus perawat dari katalog bangsal | Tindakan khusus rawat inap tidak bisa dipesan |
| `FIN-CAP-33` | Serah terima klinis saat transfer antarunit (`CAP-RWF-16`, `RWI-DEC-182`) | `ClinicalManagement`, dengan permukaan `episode-rawat-inap` | `FE@22ad673 .../nursing-workspace/sections/transfer/nursing-transfer-form-panel.jsx:33` (pemberitahuan "Integrasi belum tersedia", `RWI-DEC-113`). `BE@c8e99ce`: tidak ada model serah terima transfer | **Missing** | Dokumen serah terima, penanda "tertunda", dan rujukan potret tanda vital | — |

### 19.4 Kontrak as-is yang ditelusuri

Respons sukses seluruh endpoint di bawah berbentuk `ApiResponse<T>`. Kolom "Catatan as-is" menjelaskan perilaku nyata, bukan target.

**Inpatient Discharge Clearance** — base URL `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `POST` | `/{episodeId}/discharge-clearance/webhook` | Menerima sinyal izin pulang | **Tanpa login** (`[AllowAnonymous]`) | Satu-satunya penulis salinan `InpEpisode.ClearanceStatus` selain override. Dihapus menurut `RWI-DEC-167` |
| `POST` | `/{episodeId}/supervisor-override` | Pulang darurat | `InpatientDischargeClearance : SupervisorOverride`, ditambah pemeriksaan nama peran | Alasan minimal 20 karakter. PIN tidak diverifikasi |
| `POST` | `/{episodeId}/confirm-physical-discharge` | Pulang fisik dan melepas bed | `InpatientDischargeClearance : ConfirmPhysicalDischarge` | Membaca salinan status, lalu mengantrekan `BED_RELEASED` |

**Health Services / Inpatient Management / Inpatient Discharge** — base URL `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `POST` | `/{episodeId}/record-departure` | Mencatat kepergian fisik | `InpatientDischarge : RecordDeparture` | **Tidak memeriksa izin kasir.** Ini jalur pulang fisik kedua |
| — | Penandaan keuangan manual (`MarkFinancialClearance`) | Mengisi `InpFinancialClearance` | `InpatientDischarge : MarkFinancialClearance` | Dibaca kesiapan penutupan episode. Berstatus legacy menurut `RWI-DEC-102` dan `RWI-DEC-167` |

**Inpatient Billing Operational** — base URL `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `GET` | `/{episodeId}/billing-status` | Status dan kendala tanpa rupiah | `InpatientBillingOperational : Read` | Membaca salinan status episode, bukan Billing |
| `GET` | `/{episodeId}/billing-details` | Rincian dengan rupiah | `InpatientBillingOperational : ViewBillingDetails` | Rawat Inap membaca langsung tabel `BilFolio` dan `BilChargeLine` milik Billing, tanpa tarif kamar dan biaya admin |

**BillingInpatientIntegration** — base URL `api/v1/health-services/billing-management/billing`

| Method | Path | Kegunaan | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `POST` | `/invoices/occupancy-charges` | Hitungan tarif kamar kedua | `BillingInpatient : Create` | Jam dan tarif cadangan tertanam. Nol pemanggil frontend |
| `GET` | `/invoices/encounter/{encounterId}/inpatient-summary` | Ringkasan tagihan rawat inap | `BillingInpatient : Read` | Hak lihat rupiah ditentukan dari nama peran |
| `POST` | `/inpatient-clearance/reevaluate` | Kasir menilai ulang izin pulang | `BillingInpatient : Clearance` | Menulis `BilInpatientClearanceHandoff` |
| `GET` | `/inpatient-clearance/encounter/{encounterId}/latest` | Status izin pulang terakhir | `BillingInpatient : ReadLatest` | Calon satu-satunya sumber menurut `RWI-DEC-167`. Responsnya memuat nominal, sehingga perlu disaring untuk perawat |

**Health Services / Billing Management / Patient Billing Summary** — base URL `api/v1/health-services/billing-management/patient-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Ringkasan tagihan bangsal | `PatientBillingSummary : Read` | Total dari folio. Rupiah tampil kepada semua pemegang hak baca |

**Pesanan dari bangsal yang sudah ada**

| Tag | Base URL | Method dan path | Hak akses | Catatan as-is |
|---|---|---|---|---|
| `Health Services / Laboratory Management / Lab Order` | `api/v1/health-services/laboratory-management/lab-orders` | `GET /instruction-verification-worklist` | Sesuai atribut controller | Penugasan dokter pemberi instruksi diperiksa (403) |
| `Health Services / Radiology Management / Rad Order` | `api/v1/health-services/radiology-management/rad-orders` | `GET /instruction-verification-worklist` | Sesuai atribut controller | Sama dengan Lab |
| `Health Services / Nutrition Management / Nutrition Order` | `api/v1/health-services/nutrition-management/orders` | `POST /` | `NutritionOrder : Create` | Dokter peminta hanya diperiksa ada. Penginput dari `CreateBy` |
| `Health Services / Nutrition Management / Patient Diet` | `api/v1/health-services/nutrition-management/diets` | `POST /`, serta penghentian diet | `NutritionPatientDiet : Update` | Mengganti diet aktif wajib beralasan |
| `Health Services / Blood Bank Management / Blood Order` | `api/v1/health-services/blood-bank-management/blood-orders` | `POST /`, `POST /confirm-duplicate` | Sesuai atribut controller | Dokter peminta hanya diperiksa ada |
| `Health Services / Operating Room Management / Cases` | `api/v1/health-services/operating-room-management/cases` | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}` | `OperatingRoomCase : Read`, `: Create`, `: Update` | Pesanan merujuk `PatientProcedureId` |
| `Health Services / Operating Room Management / Execution` | `api/v1/health-services/operating-room-management/cases/{caseId}/execution` | `GET /handovers`, `POST /handovers`, `PATCH /handovers/{handoverId}/accept` | `OperatingRoomHandover : Read`, `: Update` | Unit penerima tidak diperiksa |
| `Health Services / Clinical Management / Patient Allergy` | `api/v1/health-services/clinical-management/patient-allergies` | `POST /from-medication-administration` | `PatientAllergy : Create` | Belum punya layar |

### 19.5 Ketidakcocokan antar-modul dan frontend–backend

| ID | Ketidakcocokan | Bukti | Akibat |
|---|---|---|---|
| `FIN-CON-01` | Billing memberi label invoice rawat inap `"RANAP"`, tetapi empat titik penguncian ulang memeriksa `"INPATIENT"` | `FIN-CAP-08` | Auto-reblock Billing tidak pernah aktif |
| `FIN-CON-02` | Billing menyimpan izin pulang di `BilInpatientClearanceHandoff`, Rawat Inap membaca `InpEpisode.ClearanceStatus`, dan penutupan episode membaca `InpFinancialClearance` | `FIN-CAP-07`, `FIN-CAP-09` | Tiga jawaban berbeda untuk satu pertanyaan. Sudah diputuskan `RWI-DEC-167` |
| `FIN-CON-03` | Ada dua jalur pulang fisik: satu memeriksa salinan status, satu lagi tidak memeriksa apa pun | `FIN-CAP-09` | Gerbang pulang bisa dilewati |
| `FIN-CON-04` | Frontend mengenal status `OVERRIDDEN`, sedangkan kosakata Billing hanya `PENDING`, `BLOCKED`, `CLEARED`, dan `REVOKED` | `discharge-clearance-gate-card.jsx:106-107`; `BilInpatientClearanceHandoff.cs:75-81` | Setelah sumber pindah ke Billing, status override harus tetap berasal dari Rawat Inap |
| `FIN-CON-05` | Komentar frontend berjanji "bukan Rp 0", tetapi komponen menampilkan cadangan "Rp 0" | `nursing-billing-section.jsx:41, 218, 279` | Data kosong terbaca sebagai tagihan nol, bertentangan dengan `RWI-DEC-108` |
| `FIN-CON-06` | Backend Lab/Rad sudah menerima pesanan perawat, tetapi frontend masih menguncinya | `FIN-CAP-16` | Fungsi yang sudah ✅ tidak bisa dipakai |
| `FIN-CON-07` | Rawat Inap membaca langsung tabel milik Billing (`BilFolio`, `BilChargeLine`) di `InpatientBillingQueryService` | `FIN-CAP-13` | Melanggar batas modul. Angkanya juga berbeda dari angka invoice yang dilihat kasir |

### 19.6 Fakta, inferensi, dan rekomendasi

**Fakta baru dari source (`FIN-FACT`).** Fakta yang sudah tercatat sebagai `RWI-FACT-041` s.d. `RWI-FACT-045` tidak diulang di sini.

| ID | Fakta |
|---|---|
| `FIN-FACT-01` | Billing kini menerima domain `ROOM_STAY` dan `INPATIENT` (`BillingChargeSourceAdapter.cs:78-85`). `INT-FACT-02` bagian 18 tidak berlaku lagi |
| `FIN-FACT-02` | Label `ServiceType` invoice rawat inap tidak seragam: `"RANAP"` saat invoice dibuat, `"INPATIENT"` saat penguncian ulang (`FIN-CON-01`) |
| `FIN-FACT-03` | `record-departure` memulangkan pasien tanpa memeriksa izin kasir (`FIN-CAP-09`) |
| `FIN-FACT-04` | PIN supervisor override tidak pernah diverifikasi (`FIN-CAP-11`) |
| `FIN-FACT-05` | `OCCUPANCY_CORRECTED` terbit pada setiap transfer, sedangkan jalur koreksi salah catat belum ada (`FIN-CAP-15`) |
| `FIN-FACT-06` | Payload outbox membawa salinan data penempatan, dan pesan berstatus `Processing` tidak pernah diambil ulang (`FIN-CAP-01`) |
| `FIN-FACT-07` | Semua angka tagihan di bangsal berasal dari folio, sehingga tarif kamar dan biaya admin tidak pernah tampil (`FIN-CAP-13`) |
| `FIN-FACT-08` | Biaya admin hanya dihitung bila invoice punya minimal satu item, sedangkan tarif kamar bukan item (`FIN-CAP-04`) |
| `FIN-FACT-09` | Titik tagih as-is: Lab saat spesimen diterima (`AcceptAsync`), Radiologi saat kualitas citra diputuskan (`DecideQualityAsync`), dan obat saat diserahkan (`DispenseAsync`) (`FIN-CAP-05`). Bahan untuk `RWI-OQ-103` butir (b) |
| `FIN-FACT-10` | Pesanan OK sudah merujuk order tindakan pasien, dan consent bedah serta anestesi sudah menjadi syarat "Siap" (`FIN-CAP-21`, `FIN-CAP-23`) |
| `FIN-FACT-11` | `CpptNoteKind.NursingNarrative` sudah ada, sehingga narasi perawat dapat dipisahkan tanpa kolom baru (`FIN-CAP-27`) |
| `FIN-FACT-12` | `MstTariff` tidak punya rujukan alat dan tidak punya kolom penjamin (`FIN-CAP-31`) |

**Jawaban `RWI-OQ-101` butir (a)–(c) dari source:**

| Butir | Pertanyaan | Jawaban as-is |
|---|---|---|
| (a) | Apakah penginput tersimpan terpisah dari dokter peminta atau penetap? | **Ya**, untuk ketiganya. Gizi: `CreateBy` terpisah dari `RequesterDoctorId` (`NutritionOrderService.cs:180-190`). Bank Darah: `CreateBy` terpisah dari `RequestingDoctorId` (`BbkBloodOrderService.cs:918-929`). Diet: `CreateBy` terpisah dari `PrescribedByWorkforceId` (`NutritionDietService.cs:214-217`) |
| (b) | Apakah modul membatasi dokter peminta pada dokter berpenugasan aktif? | **Tidak.** Ketiganya hanya memeriksa bahwa dokter atau tenaga itu ada dan aktif. Rawat Inap dapat memeriksa penugasan lewat `InpatientClinicalContextService.IsDoctorAssignedAsync` sebelum meneruskan, **tanpa** mengubah modul Gizi atau Bank Darah |
| (c) | Apakah status verifikasi dokter dapat disimpan di modul itu? | **Tidak.** Gizi, Bank Darah, dan Diet tidak punya kolom status verifikasi seperti `LabOrder` dan `RadOrder`. Tempat penyimpanannya butuh keputusan (`FIN-UNK-01`) |

**Inferensi.**

1. Status "selesai" pada task integrasi berasal dari pengujian di dalam satu modul. Belum ada jalur yang diuji dari Rawat Inap sampai layar kasir, sehingga `FIN-CON-01` sampai `FIN-CON-03` lolos tanpa terdeteksi.
2. Dari butir (a) dan (b) di atas, `RWI-DEC-171` dan `RWI-DEC-178` dapat berjalan **tanpa mengubah** modul Gizi dan Bank Darah, kecuali untuk status verifikasi.

**Rekomendasi (bukan keputusan).**

1. `FIN-CAP-09` dan `FIN-CAP-10` sebaiknya masuk gelombang `RWF-W0` bersama-sama. Keduanya celah keamanan dan keselamatan administrasi yang tidak bergantung pada Billing.
2. `FIN-CON-01` adalah perbaikan kecil di Billing dengan dampak besar, sehingga layak didahulukan di antara butir `RWI-OQ-103`.
3. `requirement-completeness-gate` sebaiknya memperlakukan `FIN-UNK-01` dan `FIN-UNK-03` sebagai pemblokir desain, tetapi hanya untuk slice yang bersangkutan.

### 19.7 Unknown dan pertanyaan penutup

| ID | Pertanyaan | Kenapa tidak bisa dijawab dari source | Pemilik | Memblokir |
|---|---|---|---|---|
| `FIN-UNK-01` | Di mana status verifikasi dokter untuk pesanan Gizi, pesanan darah, dan diet yang diinput perawat disimpan: kolom baru di modul pemilik seperti Lab/Rad, atau catatan terpisah? | Keputusan kepemilikan data lintas modul | Muhammad Hamzah bersama pemilik `NutritionManagement` dan `BloodBankManagement` (`RWI-OQ-101`) | `DESIGN` bagian verifikasi `RWI-DEC-171` dan `RWI-DEC-178` |
| `FIN-UNK-02` | Bagaimana tarif per penjamin ditetapkan bila `MstTariff` tidak punya kolom penjamin? | Kemungkinan diatur kontrak penjamin atau aturan Billing lain di luar batas audit ini | Yasmina | `DESIGN` `RWI-DEC-179` butir 2 |
| `FIN-UNK-03` | Jalur pulang fisik mana yang dipertahankan: `confirm-physical-discharge` atau `record-departure`? | Keputusan desain. Keduanya punya konsumen dan riwayat keputusan (`RWI-DEC-055`, `RWI-DEC-159`) | Muhammad Hamzah | `DESIGN` `CAP-RWF-01` |
| `FIN-UNK-04` | PIN supervisor override diverifikasi terhadap apa, atau dihapus? | Tidak ada tempat penyimpanan PIN di source | Muhammad Hamzah; pemilik security/privacy (masih `OPEN`) | `DESIGN` `FR-RWF-005` |
| `FIN-UNK-05` | Apakah role perawat di lingkungan target sudah memegang `NutritionOrder : Create`, `BloodOrder : Create`, dan `NutritionPatientDiet : Update`? | Pemetaan role ke permission tersimpan di database, bukan di source | Admin Akses Role | `IMPLEMENTATION` |
| `FIN-UNK-06` | Apakah worker outbox, master kebijakan tarif kamar, dan tarif kamar per kelas benar-benar aktif dan terisi di lingkungan uji? | Butuh aplikasi berjalan (`RWI-DEC-168`) | Tim QA dan penanggung jawab data master | `IMPLEMENTATION` dan UAT |

### 19.8 Keterbatasan audit

1. Audit ini hanya pembacaan statis. Tidak ada build, test, maupun aplikasi berjalan, sehingga seluruh status di atas **belum** memenuhi syarat ✅ menurut `RWI-DEC-168`.
2. Hemodialisa, Rehab Medik, MAR, sliding scale, dan rekonsiliasi obat tidak diaudit ulang. Statusnya mengikuti bagian 17 dan PRD.
3. Isi V1 tidak dibaca ulang.
4. Nomor baris dapat bergeser pada commit berikutnya. Nama simbol yang disebut tetap menjadi patokan.

### 19.9 Handoff dan pemicu impact scan

**Langkah berikutnya yang disarankan:** jalankan `requirement-completeness-gate` untuk slice Finishing, dengan masukan decision log revision `27`, PRD Finishing setelah direvisi ke v`0.2`, dan bagian ini. `FIN-UNK-01`, `FIN-UNK-03`, dan `FIN-UNK-04` dapat diajukan lewat `grill-me` bila gate menyatakannya memblokir.

**Bagian ini menjadi `STALE` bila salah satu hal berikut berubah:**

- **Backend:** `InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs`; `InPatientManagement/Services/InpatientClearanceGateService.cs`, `InpDischargeService.Closure.cs`, `InpBedOccupancyService.cs`, `InpatientBillingQueryService.cs`; `InPatientManagement/Controllers/InpatientDischargeClearanceController.cs`, `InpatientDischargeController.cs`, `InpatientBillingOperationalController.cs`; `BillingManagement/Billing/Services/BillingInvoiceService.cs`, `BillingCalculationService.cs`, `BillingClinicalChargeBridgeService.cs`, `InpatientClearanceService.cs`, `BilConsumerHandoffService.cs`, `InpatientRoomChargeCalculationService.cs`; `BillingManagement/Operational/Services/PatientBillingSummaryService.cs`; `NutritionManagement/Services/NutritionOrderService.cs`, `NutritionDietService.cs`; `BloodBankManagement/Services/BbkBloodOrderService.cs`; `OperatingRoomManagement/DTOs/OperatingRoomCaseDtos.cs`, `Enums/OperatingRoomEnums.cs`, `Services/OperatingRoomRecoveryService.cs`, `OperatingRoomIntegrationService.cs`; `ClinicalManagement/Controllers/PatientProcedureController.cs`; `MasterData/Models/MstTariff.cs`.
- **Frontend:** `nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`, `sections/billing/nursing-billing-section.jsx`, `sections/medication/nursing-medication-section.jsx`, `sections/transfer/nursing-transfer-form-panel.jsx`, `components/nursing-workspace-sections.jsx`; `billing-integration/discharge-clearance-gate-card.jsx`; `inpatient-nursing-constants.js`; `physician-workspace/tabs/supporting-service/supporting-service-tab.jsx`.
- **Keputusan:** jawaban atas `RWI-OQ-098` s.d. `RWI-OQ-103`, atau perubahan pada `RWI-DEC-163` s.d. `RWI-DEC-183`.

---

## 20. Audit kemampuan Workspace PPRI (Ruang Kerja Admisi) `PRD-RWI-ADMISI-001` — 7 Oktober 2026

### 20.1 Identitas, masukan, dan batas audit

| Field | Nilai |
|---|---|
| Pemicu | Amendment Pass `PRD-RWI-ADMISI-001` tuntas 2026-10-07 (`00-interview-decisions.md` revision `35`, `RWI-DEC-225` s.d. `RWI-DEC-248`). Pass itu menawarkan audit ini karena capability map belum memuat satu pun kemampuan Workspace PPRI (`RWI-FACT-060`), dan `RWI-OQ-117` adalah pertanyaan yang hanya bisa dijawab source |
| Masukan bisnis | `00-interview-decisions.md` revision `35`; `PRD-RWI-ADMISI-001` v`0.2`, SHA-256 `f1fd336f562d6936629d50f8f849fa7767e2dbea2b24678da9596de60dcc1192` (berkas `untracked`). Bila PRD dan decision log berbeda, decision log yang berlaku. Contohnya label tombol: PRD menulis "Workspace Admisi", sedangkan `RWI-DEC-245` menetapkan "Workspace PPRI" |
| Backend | `NewQuilvianSystemBackend` branch `MHamzah`, commit **`671191eb1aff3f618456cb6c909863bb0eca82f4`** (`BE@671191e`). Berjarak 126 commit dari snapshot bagian 19 (`c8e99ce5`). **Working tree tidak bersih:** enam berkas source belum di-commit, yaitu `MstCompanyGuarantor.cs`, `PatientProcedureController.cs`, `EncounterInsuranceService.cs`, `InsuranceCoverageService.cs`, `PatientProcedureOrderService.cs`, dan `InpAncillaryOrderAdapter.cs`. Bukti dari berkas-berkas itu dibaca dari `HEAD` lewat `git show` dan ditandai `@HEAD` |
| Frontend | `QuilvianSystemFrontendDev` branch `HamzahV2`, commit **`27889662ab1e6149bd7cd291b480796905e52594`** (`FE@2788966`). Berjarak 91 commit dari snapshot bagian 19 (`22ad6733`). **Audit dimulai pada `1f889d67c`**, ketika working tree masih berisi 34 berkas Finishing keperawatan dan dokter yang belum di-commit. Selama audit berjalan, pemilik meng-commit perubahan itu sebagai `27889662a` (37 berkas). Empat berkas yang dikutip ikut berubah: `inpatient-episode-detail-view.jsx` (+1 baris), `nursing-workspace-view.jsx` (+2 baris), `inpatient-nursing-constants.js`, dan `nursing-billing-section.jsx`. Seluruh bukti frontend **diperiksa ulang pada `27889662a`**: dua berkas pertama hanya bergeser nomor barisnya, dua berkas terakhir tidak berubah pada baris yang dikutip, dan tidak ada makna bukti yang berubah. Working tree frontend bersih di akhir audit |
| Pembanding V1 | Tidak dibaca ulang. Isian V1 diambil dari PRD Lampiran A |
| Klaster yang diaudit | Identity/Master (pasien, relasi, kontak darurat, profil rumah sakit, pengaturan Rawat Inap, master butir administrasi); Episode/Transaction (episode, penempatan, riwayat status, penjamin kunjungan); Actor/Workforce (identitas dan jabatan petugas, master tanda tangan); Location/Resource (kamar, bed, kelas, penanda intensif dan isolasi); Workflow/Status (penjaga tulis, penutupan episode); Documentation/Record (persetujuan, pengkajian keperawatan, serah terima); Order/Result (kasus kamar operasi); Financial (deposit, tarif, perkiraan harga, biaya admin); Authorization/Audit (hak akses, konkurensi, idempotensi); External Integration (SignalR, komponen cetak, QR) |
| Di luar audit | Isi V1; aturan internal Billing selain yang dibaca Workspace PPRI; Estimasi Biaya Rinci dan MP Benefit, yang tetap ditunda (`RWI-OQ-119`, `RWI-OQ-120`); data role dan permission di database; lingkungan berjalan |
| Cara audit | Pembacaan statis memakai `rg` dan `git show HEAD:`, lalu membaca implementasi yang relevan: controller dan atribut hak akses, service, model, enum, konfigurasi Entity Framework, seeder instrumen klinis, service API, hook, komponen frontend, dan `package.json`. **Tidak ada build, test, migration, maupun eksekusi aplikasi** |
| Batas tulis | Hanya bagian ini dan baris kepala dokumen. Tidak ada satu baris source aplikasi yang diubah |

**Status bagian 19.** Bagian 19 terikat `c8e99ce5` dan **tidak** diaudit ulang di sini. Tiga klaimnya terbukti sudah berubah, sehingga bagian 19 dinyatakan `STALE` untuk butir di bawah ini dan untuk butir Finishing lain yang belum diperiksa ulang:

| Klaim bagian 19 | Keadaan pada `BE@671191e` / `FE@2788966` |
|---|---|
| `FIN-CAP-31`, `FIN-FACT-12`: `MstTariff` tanpa rujukan alat | Kini ada `MedicalEquipmentId`, `SurgeryComponentType`, `ChargeBasis`, dan `ChargeRounding` (`Areas/HealthServices/MasterData/Models/MstTariff.cs:32-45`) |
| `FIN-CAP-22`: tujuh status kasus OK | Kini delapan, dengan `Rejected = 8` (`Areas/HealthServices/OperatingRoomManagement/Enums/OperatingRoomEnums.cs:7`) |
| `FIN-CAP-12`: kartu gerbang pulang `discharge-clearance-gate-card.jsx` | Berkas itu tidak ada lagi di `HEAD` frontend. Penyegaran berkala kini lewat `src/lib/hooks/health-services/inpatient-management/use-inpatient-billing-status.js` dan `billing-summary-card.jsx` |

### 20.2 Ringkasan untuk pembaca umum

Bayangkan Tn. Budi Santoso (samaran) baru diterima di kelas 2, kamar Melati 03 bed B, dengan penjamin asuransi. Begini keadaan sistem hari ini bila petugas admisi Sari ingin menyelesaikan dokumen penerimaannya:

1. **Belum ada tombol Workspace PPRI.** Detail Episode baru punya Workspace Keperawatan dan Workspace Dokter, dan keduanya tampil untuk semua orang tanpa pemeriksaan hak di layar. Kerangka ruang kerjanya sendiri (header, navigasi kiri, dan keadaan ditolak, gagal, atau hanya-baca) sudah ada dan bisa dipakai apa adanya.
2. **Datanya sebagian besar sudah ada, tetapi tersebar di banyak tempat.** Nama, No. RM, tanggal lahir, agama, status nikah, dan penanda bayi ada di master pasien. Kelas, kamar, bed, DPJP, dan perawat penanggung jawab ada di episode. Nomor kartu asuransi ada di penjamin kunjungan. Kekurangan deposit Rp 3.000.000 sudah dihitung Billing. Yang belum ada: pekerjaan, kewarganegaraan, RT/RW, kelurahan, dan diagnosis masuk.
3. **Semua dokumen admisinya belum ada.** Tidak ada tabel, endpoint, maupun layar untuk Serah Terima, Permintaan Privasi, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit, Estimasi Biaya, dan log cetak. Yang ada hanya cetak Surat Persetujuan 12 butir tanpa simpan, dan cetakan itu menanam identitas rumah sakit client di kode.
4. **Checklist serah terima tidak bisa langsung menumpang master butir yang sudah ada.** Penutupan episode membaca *semua* butir aktif di master itu. Bila 15 butir serah terima dimasukkan tanpa penanda jenis, butir wajibnya langsung ikut menahan penutupan episode Budi.
5. **Gelang bisa memakai QR pasien yang sudah ada**, karena backend membuat QR berisi No. RM saja. Namun kartu pasien kiosk di frontend membuat QR sendiri yang memuat nama, sehingga ada dua isi QR untuk pasien yang sama.
6. **Perkiraan biaya tindakan sudah bisa dihitung** lewat dua endpoint yang ada. Yang belum ada: tarif visit dokter rawat inap, serta aturan cito, *standby*, lembur, dan anestesi.

Dari 54 kemampuan yang diaudit: **10** `Ready to reuse`, **19** `Reuse with adapter`, **3** `Extend`, **1** `Repair`, **16** `Missing`, **3** `Conflict`, dan **2** `Unknown`.

### 20.3 Peta kemampuan PRD ke baris bukti

Bagian ini mendaftarkan `CAP-RWA-01` s.d. `CAP-RWA-17` sebagaimana diminta PRD bagian 20.4.

| Kemampuan PRD | Isi | Baris bukti | Ringkasan keadaan |
|---|---|---|---|
| `CAP-RWA-01` | Ruang kerja, header, kelengkapan | `PPRI-CAP-01` s.d. `08` | Template siap; tombol, halaman, dan aturan kelengkapan baru |
| `CAP-RWA-02` | General Consent (*fail-closed*) | `PPRI-CAP-09` s.d. `12` | Surat 12 butir perlu kop dari profil RS; Formulir V1 cetak saja masih baru |
| `CAP-RWA-03` | Serah Terima Pasien Baru | `PPRI-CAP-13` s.d. `15` | Dokumen baru; master butir diperluas bersama saringan jenis |
| `CAP-RWA-04` | Asesmen Edukasi | `PPRI-CAP-54` | Dibatalkan (`RWI-DEC-226`); tetap di Workspace Keperawatan |
| `CAP-RWA-05` | Gelang dan Label | `PPRI-CAP-16` s.d. `20` | Data siap; cetak dan log baru; isi QR berbeda antara backend dan kiosk |
| `CAP-RWA-06` | Permintaan Privasi | `PPRI-CAP-21`, `23` | Baru seluruhnya |
| `CAP-RWA-07` | IPD | `PPRI-CAP-24` s.d. `28` | Dirangkai dari banyak sumber; diagnosis masuk belum jelas sumbernya |
| `CAP-RWA-08` | Pelunasan Deposit | `PPRI-CAP-29` s.d. `33` | Angka siap; surat baru; ada dua nilai interval tindak lanjut |
| `CAP-RWA-09` | Estimasi Biaya Rekap | `PPRI-CAP-36` s.d. `41` | Tindakan dan kamar lewat adapter; visit dokter dan aturan cito belum ada |
| `CAP-RWA-10` | Estimasi Biaya Rinci | — | Ditunda (`RWI-OQ-119`); tidak diaudit |
| `CAP-RWA-11` | Selisih Biaya | `PPRI-CAP-34`, `35` | Jenis penjamin siap; surat baru |
| `CAP-RWA-12` | MP Benefit | — | Ditunda (`RWI-OQ-120`); satu fakta pendukung dicatat di `PPRI-FACT-13` |
| `CAP-RWA-13` | Nilai Kepercayaan | `PPRI-CAP-22`, `23` | Baru; bertumpang tindih dengan isian pengkajian keperawatan |
| `CAP-RWA-14` | Tanda tangan pasien/keluarga | `PPRI-CAP-12`, `48` | Slot kertas baru; jalur digital menunggu pemilik privasi |
| `CAP-RWA-15` | Atestasi petugas | `PPRI-CAP-46`, `47` | Nama dan jabatan siap; master gambar tanda tangan rusak di luar scope |
| `CAP-RWA-16` | Kop surat dan kode formulir | `PPRI-CAP-49`, `50` | Profil RS siap tanpa logo; kode formulir dan kota butuh kolom baru |
| `CAP-RWA-17` | Siklus, versi, log cetak | `PPRI-CAP-19`, `42` s.d. `45`, `51` s.d. `53` | Pola entity, konkurensi, dan idempotensi siap ditiru; entity dan resource hak baru |

### 20.4 Capability evidence map

Format bukti: `repo@sha path:baris #simbol`. `BE@671191e` adalah backend commit `671191eb1aff…`, dan `FE@2788966` adalah frontend commit `27889662ab1e…`. Penanda `@HEAD` berarti berkas itu sedang berubah di working tree dan buktinya dibaca dari commit. Setiap baris memakai tepat satu status dari kontrak bukti kemampuan.

#### A. Jalan masuk, kerangka, header, dan kelengkapan — `CAP-RWA-01`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-01` | Tombol "Workspace PPRI" tepat sesudah Workspace Dokter, hanya bagi pemegang hak baca dokumen admisi (`FR-RWA-001`, `RWI-DEC-245`) | `episode-rawat-inap` (FE) | `FE@2788966 src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx:610 #EpisodeActionBar`; tombol Workspace Keperawatan `:613` dan Workspace Dokter `:624` **tanpa** penjaga hak. Penjaga yang tersedia: `src/lib/hooks/auth/use-permission.jsx:28 #usePermission` | **Missing** | Tombol baru dengan penjaga `usePermission`. Ini tombol ruang kerja pertama di Detail Episode yang dijaga di layar | `usePermission` sengaja bernilai "boleh" selama daftar hak belum termuat (`use-permission.jsx:17-27`), sehingga tombol bisa tampil sekejap. Server tetap menolak dengan kode 403 |
| `PPRI-CAP-02` | Halaman dan rute ruang kerja per episode | `episode-rawat-inap` (FE) | Pola: `FE@2788966 src/app/health-services/inpatient-management/episodes/[id]/nursing/page.jsx`; `src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js:8-11 #buildInpatientNursingWorkspaceRoute`; `inpatient-episode-constants.jsx:5-32` | **Missing** | Halaman `episodes/[id]/<rute>` dan builder rutenya. Nama rute `DEV_DISCRETION` menurut PRD | — |
| `PPRI-CAP-03` | Template empat wilayah Workspace Keperawatan, menu dan tab di alamat halaman, batas keadaan (`FR-RWA-002`, `FR-RWA-008`) | `ClinicalWorkspace` bersama (FE) | `FE@2788966 src/components/ui/clinical-workspace/index.js` (20 komponen, antara lain `ClinicalWorkspaceShell`, `ClinicalSectionNav`, `ClinicalCompletionBar`, `ClinicalStatusBadge`, `ClinicalRevisionHistory`); `ClinicalStateBoundary.jsx:10-40` (urutan ditolak → gagal → memuat → kosong → isi dengan banner hanya-baca). Contoh pakai: `nursing-workspace/nursing-workspace-view.jsx:93 #updateUrl` (`?section=` dan `?tab=`), `:198` (`isReadOnlyEpisodeStatus`), `:336` (`ClinicalStateBoundary`), `:366` (`ClinicalWorkspaceShell`) | **Ready to reuse** | — | — |
| `PPRI-CAP-04` | Header pasien versi admisi: identitas, episode, penjamin dan kartu, kontak darurat, alergi (`FR-RWA-003`) | Banyak pemilik | Episode: `BE@671191e Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs:133 #InpatientEpisodeDetailResponse` (nama, No. RM, kelas, waktu admisi, isolasi, DPJP dan perawat aktif, `Version`) dan `DTOs/InpatientEpisodeReadDtos.cs:196-230` (bed, kamar, unit, kelas, perawat aktif); **tanpa** jenis kelamin, tanggal lahir, penjamin, dan kontak darurat. Pasien: `PatientManagement/MasterData/Controllers/PatientController.cs:453` `GET patients/admin/{id}` (`Patient : Read`). Kontak darurat: `PatientEmergencyContactController.cs:223-224` `GET patient-emergency-contacts/admin` (`PatientEmergencyContact : Read`). Penjamin: `RegistrationManagement/Controllers/PatientEncounterController.cs:375-380` `GET patient-encounters/{id}` (`PatientEncounter : Read`) → `DTOs/PatientEncounterDtos.cs:166-240 #PatientEncounterPaymentResponse`. Alergi: `ClinicalManagement/Controllers/PatientAllergyController.cs:98-99` `GET patient-allergies/active-alerts` (`PatientAllergy : Read`) | **Reuse with adapter** | Header butuh paling sedikit lima sumber. Rangkuman di server (usulan PRD `GET …/summary`) mengurangi jumlah panggilan dan jumlah hak yang harus dipegang CRO, perawat, dan kasir (`PPRI-CAP-53`) | Bila dirangkai di layar, pengguna yang tidak punya salah satu hak melihat header setengah kosong |
| `PPRI-CAP-05` | Status deposit di header; rupiah hanya bagi pemegang hak (`FR-RWA-003`) | `BillingManagement` | `BE@671191e Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs:108-110` `GET patient-funds/deposits/episodes/{episodeId}` (`BillingDeposit : Read`); `Dtos/EpisodeDepositSummaryDtos.cs:3-24` (`IsPolicyRequired`, `MinimumPolicyAmount`, `TotalReceived`, `PolicyShortfallAmount`, `FollowUpIntervalDays`). `FE@2788966`: **nol** pemanggil | **Reuse with adapter** | Service frontend baru. Pemetaan: `IsPolicyRequired = false` → "Tidak wajib"; `PolicyShortfallAmount > 0` → "Kurang Rp …"; selain itu → "Cukup" | Nama hak rupiah: PRD mengusulkan `ReadAmount`, sedangkan preseden Finishing memakai `PatientBillingSummary : ViewAmount` (`FE@2788966 …/nursing-workspace/sections/billing/nursing-billing-section.jsx:59`) |
| `PPRI-CAP-06` | Kelengkapan dan lencana per menu menurut `RWI-DEC-234` | `episode-rawat-inap` | Pencarian `InpatientAdmissionDocument` dan `AdmissionDocument` di `BE@671191e` dan `FE@2788966`: **nol** hasil. Masukan aturan yang sudah ada: jenis penjamin `RegistrationManagement/Enums/EncounterPaymentType.cs:7-19` (`Cash = 1`, `Insurance = 2`, `CompanyGuarantor = 3`); kekurangan deposit (`PPRI-CAP-05`); status kasus OK `OperatingRoomManagement/Enums/OperatingRoomEnums.cs:7` | **Missing** | Endpoint ringkasan kelengkapan. Arti "pemesanan ruang bedah yang aktif" belum terdefinisi (`PPRI-UNK-01`) | — |
| `PPRI-CAP-07` | Peringatan "Dokumen admisi belum lengkap" di Detail Episode (`RWI-DEC-234`) | `episode-rawat-inap` | `FE@2788966 inpatient-episode-detail-view.jsx:714-719` (tumpukan peringatan "Perlu diketahui" dari `episode.warnings`); `BE@671191e InpatientEpisodeDtos.cs:238 #Warnings` (`List<string>`) | **Reuse with adapter** | Wadah peringatannya sudah ada; sumber isinya, yaitu ringkasan kelengkapan, belum ada | — |
| `PPRI-CAP-08` | Hanya-baca pada episode `Closed`/`Cancelled`, dan server menolak penulisan (`FR-RWA-007`, `RWI-DEC-240` butir 7) | `InPatientManagement` | FE: `src/utils/health-services/inpatient-management/inpatient-nursing-workspace-utils.js:4 #isReadOnlyEpisodeStatus`. BE (pola pembanding): `Areas/HealthServices/ClinicalManagement/Services/NursingEpisodeWriteGuard.cs:123 #EnsureCanWriteAsync` (hanya `Admitted` dan `DischargePending`; hasil seragam berkode alasan; penanda kiriman ulang `IsReplay`) | **Reuse with adapter** | Penjaga tulis setara untuk dokumen admisi di `InPatientManagement` | — |

#### B. General Consent selama *fail-closed* — `CAP-RWA-02`, `RWI-DEC-230`, `RWI-DEC-233`, `RWI-DEC-246`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-09` | Tab Surat Persetujuan 12 butir di menu General Consent, cetak tanpa simpan (`RWI-DEC-077`, `RWI-DEC-233`) | `episode-rawat-inap` (FE) | `FE@2788966 src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx:19-31` (halaman `FE-INP-18`, dibungkus `AccessDeniedGate`, butuh `InpatientEpisode : Read` dan `PatientEncounter : Read`); `src/lib/hooks/health-services/inpatient-management/use-inpatient-consent-print.js:58-82` (membaca episode, kunjungan, dan pasien), `:152` (`canPrint`); `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:925-945 #INPATIENT_CONSENT_COPY` (nama, alamat, telepon, dan email rumah sakit client **ditanam di kode**) | **Repair** | Komponen formulirnya dipakai ulang sebagai tab, tetapi kopnya harus dibaca dari profil rumah sakit (`RWI-DEC-247`). Perbaikan itu otomatis berlaku juga pada langkah 8 alur admisi, yang memakai komponen yang sama | Tanpa perbaikan, cetakan di Workspace PPRI melanggar `RWI-DEC-247` sejak hari pertama |
| `PPRI-CAP-10` | Formulir General Consent V1 cetak saja, terisi otomatis (`RWI-DEC-233`, `FR-RWA-021`, `FR-RWA-022`) | `episode-rawat-inap` (FE) | Lembar Formulir GC V1 (kode V1 `GC/ADM/001`, dengan isian tipe kamar dan panduan rawat inap) belum ada di `FE@2788966`. Dua hasil pencarian "formulir persetujuan umum" menunjuk surat 12 butir `PPRI-CAP-09` (`inpatient-consent-form.jsx:10`, `inpatient-consent-print-view.jsx:40`). Sumber isi: hubungan terstruktur `BE@671191e Areas/HealthServices/MasterData/Enums/PatientRelationshipType.cs:5-15` (`Mother`, `Father`, `Child`, `Spouse`, `Guardian`, `Sibling`, `GrandParent`, `EmergencyContact`, `ResponsiblePerson`, `Other`) pada `PatientManagement/MasterData/Models/MstPatientRelationship.cs:18-47` (nama, alamat, penanda penanggung jawab dan wali sah); kontak darurat `MstPatientEmergencyContact.cs:20 #Relationship` **berupa teks bebas**; isolasi `InPatientManagement/Models/InpEpisode.cs:70 #RequiresIsolation`; penanda intensif dan isolasi `MasterData/Models/MstRoom.cs:45-47` (`IsIsolationRoom`, `IsIntensiveCare`), `MstBed.cs:35-37` (`IsIsolationBed`, `IsIntensiveCareBed`), `MstPatientClass.cs:37 #IsForIntensiveCare`; `MasterData/Enums/ServiceUnitType.cs:5-17` **tanpa** nilai perawatan intensif | **Missing** | Lembar cetak baru. "Istri" atau "Suami" → `Spouse`, "Anak" → `Child`, dan "Orang Tua" → `Mother`/`Father` bisa dicari otomatis dari relasi. Kontak darurat hanya bisa dipilih dari daftar karena hubungannya teks bebas (`PPRI-UNK-03`). "Tipe unit perawatan intensif" pada `RWI-DEC-233` tidak ada di source (`PPRI-CON-03`) | Pencocokan teks bebas ("istri", "Istri ", "ISTRI") rawan menunjuk orang yang salah |
| `PPRI-CAP-11` | Tombol Cetak Persetujuan di Detail Episode membuka tab Surat Persetujuan (`RWI-DEC-246`) | `episode-rawat-inap` (FE) | `FE@2788966 inpatient-episode-detail-view.jsx:672-680` (tombol `consent-print`); `inpatient-episode-constants.jsx:32 #buildInpatientEpisodeConsentPrintRoute`; halaman `src/app/health-services/inpatient-management/episodes/[id]/consent-print/page.jsx` | **Extend** | Tujuan dan penjaga tombol diubah. Nasib halaman lama ditetapkan desain (`RWI-DEC-246`) | Penjaga baru tidak menghapus kebutuhan hak data. Selama tab masih memanggil kunjungan dan pasien dari layar, pengguna tetap butuh `PatientEncounter : Read` dan `Patient : Read` (`PPRI-CAP-53`) |
| `PPRI-CAP-12` | Penyimpanan persetujuan umum kelak (`RWI-DEC-236`, `draft`) | `ClinicalManagement` | `BE@671191e Areas/HealthServices/ClinicalManagement/Enums/PatientConsentType.cs:14` (`Admission = 9`); `PatientConsentSignerType.cs:5-14` (`Patient`, `Parent`, `Spouse`, `Child`, `Sibling`, `Guardian`, `FamilyMember`, `LegalRepresentative`, `Other`), yang cukup untuk memetakan pilihan hubungan V1; `PatientConsentMethod.cs:6-7` (`WrittenPaper`, `DigitalSignature`); `Models/TrxPatientConsent.cs:137-212` (penanda tangan, jalur gambar tanda tangan, berkas, dan hash-nya) | **Ready to reuse** | **Terkunci keputusan:** tidak boleh dipakai selama `RWI-DEC-230` berlaku | — |

#### C. Serah Terima Pasien Baru — `CAP-RWA-03`, `RWI-DEC-239`, `RWI-DEC-241`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-13` | Master butir berjenis "Serah Terima Pasien Baru" dengan sub-butir (`RWI-DEC-241` butir 3) | `MasterData` (gerbang `RWI-DEC-193`) | `BE@671191e Areas/HealthServices/MasterData/Models/MstInpatientClearanceItem.cs:8-27` (`ItemCode`, `ItemName`, `Description`, `IsMandatory`, `SortOrder`, `IsActive`; **tanpa** jenis dan induk); `Controllers/InpatientClearanceItemController.cs:36-290` (`InpatientClearanceItem : Read/Create/Update/Delete`). Konsumen satu-satunya: `InPatientManagement/Services/InpDischargeService.Closure.cs:88-101` membaca **semua** butir aktif tanpa saringan, lalu `:115` menjadikan butir wajib yang belum ditandai sebagai penahan penutupan | **Extend** | Kolom jenis dan induk, ditambah saringan jenis pada query penutupan **di task yang sama**. `SortOrder` di tabel ini pola legacy: kontrak backend melarang `SortOrder` presentasi generik untuk kode baru (`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md:48`), sehingga perluasan ini berstatus `TOUCHED LEGACY` | **Tinggi.** Bila butir serah terima masuk sebelum saringan ada, butir wajibnya langsung menahan penutupan setiap episode |
| `PPRI-CAP-14` | Dokumen Serah Terima dengan butir beku dan tiga slot tanda tangan berbeda akun (`RWI-DEC-239`, `RWI-DEC-241`) | `InPatientManagement` (`RWI-DEC-228`) | Tidak ada model serah terima dari admisi ke ruangan. Yang ada dan **berbeda maksudnya**: `ClinicalManagement/Models/CliTransferHandover.cs:21-61` (serah terima klinis transfer, pengirim dan penerima berbeda akun, `SnapshotJson`, `Version`), `OperatingRoomManagement/Models/OprHandover.cs` (pasca operasi), dan `EmergencyInstallationManagement/Models/EmgDeparture.cs` (IGD ke rawat inap) | **Missing** | Dokumen, butir, slot, dan aturan satu akun satu slot. Polanya bisa meniru serah terima transfer | Belum diputuskan apakah aturan penerima `RWI-DEC-189` (pasien sudah menempati bed di unit tujuan) berlaku untuk slot Perawat (`PPRI-UNK-06`) |
| `PPRI-CAP-15` | Saran sistem "Sudah" dari dokumen lain (`RWI-DEC-241` butir 2) | `InPatientManagement` | Bergantung pada log cetak (`PPRI-CAP-19`) dan status dokumen admisi lain, yang seluruhnya belum ada | **Missing** | — | — |

#### D. Gelang dan Label — `CAP-RWA-05`, `RWI-DEC-243`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-16` | Identitas gelang: nama, sapaan, tanggal lahir, umur, No. RM, penanda bayi, dan ibu (`RWI-DEC-243`) | `PatientManagement` | `BE@671191e Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:22` (`MedicalRecordNumber`), `:32` (`FullName`), `:40` (`BirthDate`), `:42` (`Gender`), `:46` (`MaritalStatus`), `:90` (`IsNewborn`), `:92` (`MotherPatientId`); `Enums/MaritalStatus.cs:5-23` (`Unknown`, `Single`, `Married`, `Divorced`, `Widowed`, `Separated`) | **Ready to reuse** | Aturan sapaan dan batas umur bayi dihitung di Workspace PPRI. Batas umur menjadi pengaturan baru (`PPRI-CAP-50`) | — |
| `PPRI-CAP-17` | Isi QR gelang dan label = QR pasien yang sudah ada, hanya No. RM (`RWI-DEC-243`) | `PatientManagement` | BE: `PatientController.cs:1386-1400 #BuildPatientQrPayload` (No. RM terformat saja); gambar QR disimpan di `MstPatient.cs:82 #QrCodePath` dan ikut respons baca pasien (`DTOs/PatientDtos.cs:97, 445`). FE: `src/components/features/base-features/base-patient-card.jsx:69-80 #buildQrPayload` membuat QR kartu pasien kiosk **sendiri**, berisi judul kartu bernama rumah sakit, nama pasien, dan No. RM. Pemindai kiosk `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js:1508-1533 #parseQrPayload` membaca JSON atau memakai teks mentah sebagai kata kunci. Pustaka tersedia: `package.json:70-71, 89, 94` (`qrcode`, `qrcode.react`, `react-qr-code`, `react-to-print`) | **Conflict** | Gelang memakai isi QR backend (No. RM saja). Perbedaannya dengan QR kartu kiosk dicatat sebagai `PPRI-CON-01`, perbaikannya di luar scope | QR kartu kiosk memuat nama pasien, berlawanan dengan semangat `FR-RWA-052` |
| `PPRI-CAP-18` | Nomor kartu penjamin pada label, tanpa angka karangan (`FR-RWA-051`) | `RegistrationManagement` | `BE@671191e Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs:88 #CardNumberSnapshot`, `:91 #MemberNumberSnapshot`; dibaca lewat `GET patient-encounters/{id}` → `PatientEncounterDtos.cs:166-240` | **Reuse with adapter** | Pilih salah satu dari dua kolom (`PPRI-UNK-04`) | — |
| `PPRI-CAP-19` | Log cetak gelang, label, IPD, dan dokumen, dengan alasan cetak ulang (`FR-RWA-053`, `RWI-DEC-240` butir 7) | `InPatientManagement` | Pencarian `PrintLog`, `PrintCount`, `ReprintReason`, dan entity cetak di `BE@671191e Areas`: **nol** hasil | **Missing** | Tabel log cetak baru | — |
| `PPRI-CAP-20` | Kode singkat rumah sakit pada label (`FR-RWA-051`) | HR Master Data | `BE@671191e Areas/Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs:18 #SiteCode` | **Reuse with adapter** | **Inferensi:** `SiteCode` dapat dipakai sebagai kode singkat; isinya di lingkungan target belum diperiksa | — |

#### E. Permintaan Privasi dan Nilai Kepercayaan — `CAP-RWA-06`, `CAP-RWA-13`, `RWI-DEC-242`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-21` | Permintaan Privasi disimpan per baris dan bisa dibuka lagi (`FR-RWA-060` s.d. `062`) | `InPatientManagement` | Tidak ada entity, DTO, maupun isian permintaan privasi pasien di `BE@671191e` dan `FE@2788966`. Kata `privasi`/`privacy` di backend hanya muncul di komentar tentang privasi jenis kelamin kamar dan perlindungan data, misalnya `InPatientManagement/Services/InpBedOccupancyService.cs:1792` dan `MedicalRecordManagement/DTOs/MedicalRecordAccessLogDtos.cs:9` | **Missing** | Dokumen, baris kerabat, dan baris permintaan khusus | — |
| `PPRI-CAP-22` | Nilai Kepercayaan per episode, 1–5 butir, terisi dari episode lalu (`RWI-DEC-242`) | `InPatientManagement` | Agama tersedia di `MstPatient.cs:44 #Religion`. Dokumen nilai kepercayaan tidak ada. **Tumpang tindih:** `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs:395` (instrumen `GENERAL_NURSING_ASSESSMENT`), `:408` (isian teks bebas `SD_BELIEFS` "Nilai-Nilai Kepercayaan"); juga `:666` (`EDU_BELIEFS` di Asesmen Edukasi) dan `:908` (`EVAL_LEGAL` di evaluasi MPP) | **Missing** | Dokumen dan butirnya | Satu fakta tercatat di dua tempat: perawat mengisi `SD_BELIEFS` di pengkajian, sementara petugas admisi mengisi dokumen bertanda tangan (`PPRI-CON-07`) |
| `PPRI-CAP-23` | Endpoint baca ringkasan hak pasien untuk modul lain (`FR-RWA-112`, `RWI-DEC-228`) | `InPatientManagement` | Tidak ada | **Missing** | — | — |

#### F. Data Dasar Rawat Inap (IPD) — `CAP-RWA-07`, `RWI-DEC-244`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-24` | Identitas dan alamat (`RWI-DEC-244`) | `PatientManagement` | `MstPatient.cs:35` (`NickName`), `:51-54` (`IdentityType`, `IdentityNumber`), `:63` (`Email`), `:66-76` (`Address`, `CountryId`, `ProvinceId`, `CityId`, `DistrictId`, `PostalCodeId`). **Tidak ada** pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat domisili terpisah, dan alamat kantor | **Reuse with adapter** | Isian tanpa sumber dicetak sebagai garis kosong (`RWI-DEC-244`). **Inferensi:** `CountryId` adalah negara alamat, bukan kewarganegaraan, sehingga tidak boleh dicetak sebagai "Warga Negara" | — |
| `PPRI-CAP-25` | Data episode: waktu masuk, kamar, kelas, DPJP, perawat lantai, petugas admisi, riwayat pindah | `InPatientManagement` | `InpatientEpisodeDtos.cs:133` dan `InpatientEpisodeReadDtos.cs:196-230` (lokasi dan perawat aktif); `Models/InpBedPlacement.cs:16-76` (riwayat penempatan, `StartDateTime`, `EndDateTime`, `PlacedByUserId`); `Models/InpStatusHistory.cs:21-32` (`ToStatus`, `ChangedByUserId`, `ChangedAt`), sumber isian "Administrasi Rawat Inap" = pengguna yang mengubah status ke `Admitted` | **Reuse with adapter** | Satu bacaan terangkai | — |
| `PPRI-CAP-26` | Diagnosis masuk dan dokter perujuk | Belum jelas | `InpEpisode.cs` tidak punya kolom diagnosis masuk maupun dokter perujuk. Calon sumber: `RegistrationManagement/Models/RegPatientEncounter.cs:93 #ChiefComplaint`, `:139 #ReferralDoctorId` (master dokter perujuk luar `MasterData/Models/MstReferralDoctor.cs`); `ClinicalManagement/Models/TrxPatientDiagnosis.cs:90 #DiagnosisType` (`Enums/PatientDiagnosisType.cs:5-9`: `Primary`, `Secondary`, `Differential`, `WorkingDiagnosis`, `FinalDiagnosis`) | **Unknown** | Sumber yang sah belum ditetapkan (`PPRI-UNK-05`) | Mencetak keluhan utama sebagai "diagnosis masuk" menyesatkan pembaca |
| `PPRI-CAP-27` | Penjamin, nomor peserta dan polis, metode pembayaran, penanggung jawab | `RegistrationManagement`, `PatientManagement` | `PatientEncounterDtos.cs:166-240` (`PaymentType`, `InsuranceProviderName`, `PolicyNumberSnapshot`, `CardNumberSnapshot`, `MemberNumberSnapshot`, `CompanyGuarantorCodeSnapshot`, `EmployeeNumberSnapshot`); `MstPatientRelationship.cs:45-47` (`IsResponsiblePerson`, `IsLegalGuardian`); `MstPatientEmergencyContact.cs:42 #IsResponsiblePerson` | **Reuse with adapter** | — | — |
| `PPRI-CAP-28` | "Rencana @ Kamar (Rp)" hanya bagi pemegang hak rupiah | `MasterData`, `BillingManagement` | `MasterData/Models/MstTariff.cs:24 #PatientClassId`, `:54 #IsRoomCharge`, `:62 #NormalPrice`; harga per penjamin lewat `PPRI-CAP-37` | **Reuse with adapter** | Pemilihan tarif kamar per kelas | Bila ada lebih dari satu tarif kamar aktif untuk satu kelas, angkanya ambigu |

#### G. Pelunasan Deposit dan Selisih Biaya — `CAP-RWA-08`, `CAP-RWA-11`, `RWI-DEC-231`, `RWI-DEC-248`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-29` | Kekurangan deposit dari kebijakan Billing (`RWI-DEC-231`) | `BillingManagement` | `BE@671191e Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs:56-110 #GetDepositPolicyAsync` (kebijakan dicari berurutan: penjamin dan kelas → penjamin → kelas → umum; tanpa kebijakan: `IsRequired = false`, minimum 0, interval 0), `:116 #GetEpisodeDepositSummaryAsync`, `:184-208` (kekurangan = maks(0, minimum − (top-up − reversal))) | **Ready to reuse** | — | Billing membaca `InpEpisode` langsung (`:123`). Arahnya Billing ke Rawat Inap, bukan sebaliknya, sehingga tidak melanggar `RWI-DEC-102` |
| `PPRI-CAP-30` | Batas jatuh tempo = tanggal surat + interval kebijakan deposit (`RWI-DEC-231`, `RWI-DEC-248`) | `BillingManagement` | `BillingManagement/MasterData/Models/MstDepositPolicy.cs:17 #FollowUpIntervalDays` (bawaan 3, tanpa batas bawah), diteruskan lewat `EpisodeDepositSummaryDtos.cs`. **Ada nilai kedua yang serupa:** `MasterData/Models/MstInpatientSetting.cs:30-38 #DepositFollowUpIntervalDays` (milik Rawat Inap, `RWI-DEC-096`, rentang 1–365 menurut `MasterData/Services/InpatientSettingService.cs:158`), dipakai pengingat daftar pantau `InPatientManagement/Services/InpCensusQueryService.cs:1097-1099` | **Conflict** | Surat mengikuti nilai Billing menurut `RWI-DEC-231`, sedangkan pengingat daftar pantau tetap memakai nilai Rawat Inap (`PPRI-CON-02`) | Contoh: kebijakan Billing 1 hari, pengaturan Rawat Inap 3 hari. Surat jatuh tempo Sabtu, tetapi pengingat baru muncul Senin |
| `PPRI-CAP-31` | Surat Pelunasan Deposit tersimpan dengan angka beku | `InPatientManagement` | Tidak ada | **Missing** | — | — |
| `PPRI-CAP-32` | Data wali dari kontak darurat dan relasi pasien (`FR-RWA-082`) | `PatientManagement` | `PatientEmergencyContactController.cs:36, 51, 160` (aksi tanpa akhiran `/admin` dijaga policy `KioskRead` untuk akun kiosk), `:223-224` (`GET …/admin`, `PatientEmergencyContact : Read`); `PatientRelationshipController.cs:165-174` (`GET …/admin`, `PatientRelationship : Read`). FE sudah memakai varian `/admin`: `src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js:20-31` | **Reuse with adapter** | Pakai varian `/admin`, atau rangkai di server | Hubungan kontak darurat berupa teks bebas (`PPRI-UNK-03`) |
| `PPRI-CAP-33` | "Hari kerja berikutnya" untuk jatuh tempo bawaan | Belum jelas | Kalender libur ada di HR: `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Models/MstHoliday.cs:10-43` (`WorkCalendarId`, tanggal, `IsNationalHoliday`). Tidak ada fungsi hitung hari kerja bersama (pencarian `NextWorkingDay`, `AddWorkingDays`, `IsWorkingDay`: nol hasil) | **Unknown** | PRD `NFR-RWA-08` hanya menyebut Senin–Jumat; libur nasional belum diputuskan (`PPRI-UNK-11`) | Kecil, karena `RWI-DEC-248` tetap memotong bawaan ke batas kebijakan |
| `PPRI-CAP-34` | Selisih Biaya hanya untuk penjamin asuransi atau perusahaan (`FR-RWA-100`) | `RegistrationManagement` | `EncounterPaymentType.cs:7-19`; penjamin utama di `PatientEncounterPaymentResponse.PaymentType`. Penanda tambahan `IsAllowExcessPaymentByPatient` ada di `Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs:60`, `MstCompanyGuarantor.cs:58 @HEAD`, `PatientManagement/MasterData/Models/MstPatientInsurance.cs:72`, dan `MstPatientCompanyGuarantor.cs:72` | **Ready to reuse** | — | Penjamin yang **tidak** mengizinkan selisih dibebankan ke pasien tetap terhitung wajib Selisih Biaya menurut `RWI-DEC-234` (`PPRI-UNK-09`) |
| `PPRI-CAP-35` | Surat Selisih Biaya tersimpan, data pasien hanya-baca, subjek sebagai kode (`FR-RWA-101` s.d. `103`) | `InPatientManagement` | Tidak ada | **Missing** | — | — |

#### H. Estimasi Biaya Rekap — `CAP-RWA-09`, `RWI-DEC-232`, `RWI-OQ-117`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-36` | Perkiraan harga tindakan per penjamin tanpa pesanan (`RWI-DEC-232`, `RWI-OQ-117` butir a) | `ClinicalManagement` (layanan harga), dengan permukaan `InPatientManagement` | `BE@671191e Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs:12-26` `GET episodes/{episodeId}/ancillary-orders/coverage-status` (`InpatientEpisode : Read`) → `Services/InpAncillaryOrderAdapter.cs:38-85 @HEAD` (menerima **daftar ID tindakan apa pun** berjenis `Laboratory`, `Radiology`, atau `Procedure`; harga hanya tampil bila pengguna memegang `LabOrder`, `RadOrder`, atau `PatientProcedure` `: Create` menurut `:211-215`; label "perkiraan — tagihan final di kasir") → `ClinicalManagement/Services/InsuranceCoverageService.cs:67-100 @HEAD #ResolveProcedureAsync` (tarif tindakan per kelas, lalu tanggungan penjamin). FE: `src/lib/services/health-services/inpatient-management/inpatient-ancillary-order.service.js:11` | **Reuse with adapter** | Hak lihat harganya mengikuti hak **memesan**, bukan hak rupiah dokumen admisi (`PPRI-CON-09`) | Petugas admisi tanpa `PatientProcedure : Create` mendapat `NOT_PERMITTED` |
| `PPRI-CAP-37` | Perkiraan harga per tarif (kamar per hari, biaya lain) | `BillingManagement` | `Billing/Controllers/BillingInvoicesController.cs:328-350` `GET invoices/catalog-charges/coverage-preview?encounterId&tariffId&quantity` (`BillingInvoice : Read`; "advisory, read-only", tidak menambah baris tagihan) → `InsuranceCoverageService.cs:102-140 @HEAD #ResolveTariffAsync` | **Reuse with adapter** | Butuh `tariffId`, sehingga Workspace PPRI harus memilih tarif kamar per kelas sendiri (`PPRI-CAP-28`) | Hak `BillingInvoice : Read` biasanya milik kasir |
| `PPRI-CAP-38` | Biaya visit dokter per hari | Belum ada pemilik | `MstTariff.cs:11-81` tidak punya rujukan dokter maupun penanda visit (yang ada `IsConsultationFee`, `:57`); `ClinicalManagement/Services/PhysicianVisitService.cs` tidak terhubung ke tarif; domain jembatan tagihan `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs:761-771 #BillingBridgeSourceDomains` hanya `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSULTATION`, `OPERATING_ROOM`, dan `EQUIPMENT_USAGE` | **Missing** | Konsep tarif visit dokter rawat inap, wewenang Yasmina (`PPRI-UNK-08`). **Inferensi:** tarif `IsConsultationFee` per kelas bisa dijadikan calon, tetapi itu keputusan Billing | Baris visit selalu "Tarif belum tersedia" dan menahan penguncian sampai diisi manual (`FR-RWA-092`) |
| `PPRI-CAP-39` | Catatan biaya administrasi dibaca dari pengaturan Billing (`RWI-DEC-232`) | `BillingManagement` | `Areas/HealthServices/BillingManagement/MasterData/Models/MstAdministrationFeePolicy.cs:10-43` (`ServiceType`, `Percentage`, `CapAmount`, `CalculationType`, masa berlaku); `MasterData/Controllers/AdministrationFeePoliciesController.cs:14-48` (`AdministrationFeePolicy : Read`) | **Reuse with adapter** | Kalimat catatan disusun dari kebijakan aktif untuk `ServiceType` rawat inap. Label `ServiceType` rawat inap belum seragam (`FIN-CON-01`) | — |
| `PPRI-CAP-40` | Catatan cito +25%, lebih dari 4 jam +25% per jam, *standby* 20%, dan anestesi 50% dari pengaturan Billing (`RWI-DEC-232`) | `BillingManagement` | Pencarian `cito`, `standby`, `overtime`, `surcharge`, `multiplier`, dan `markup` pada `BillingManagement`, `MasterData`, dan `OperatingRoomManagement`: **nol** aturan harga. Kata `Cito` hanya muncul sebagai prioritas pesanan (Radiologi, Hemodialisa, tindakan). Yang ada hanya komponen tarif bedah `MasterData/Enums/MstSurgeryComponentType.cs:14-20` (`None`, `AnesthesiaService`, `OperatingRoomRent`) | **Missing** | Pengaturan yang dirujuk `RWI-DEC-232` tidak ada (`PPRI-CON-04`) | — |
| `PPRI-CAP-41` | Estimasi wajib bila ada pemesanan ruang bedah yang aktif (`RWI-DEC-234`) | `OperatingRoomManagement` | `OperatingRoomManagement/Enums/OperatingRoomEnums.cs:7` (`Requested`, `Scheduled`, `Ready`, `InProgress`, `Completed`, `Postponed`, `Cancelled`, `Rejected`); `Controllers/OperatingRoomCaseController.cs:13, 51` `GET cases` (`OperatingRoomCase : Read`); pemesanan dari bangsal `InPatientManagement/Controllers/InpatientSurgeryBookingController.cs:64-71` (hanya `POST`) | **Reuse with adapter** | Definisi "aktif" (`PPRI-UNK-01`) | — |

#### I. Fondasi: siklus, tanda tangan, cetak, hak akses — `CAP-RWA-14` s.d. `CAP-RWA-17`

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `PPRI-CAP-42` | Wewenang membuat entity `Inp*` baru (`RWI-DEC-228`) | Registry | `BE@671191e docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:30` (`InPatientManagement / Inpatient`, `Inp`, `ACTIVE`), `:113` (`RWI-DEC-068`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md:16` (`QBE-NAM-001`: tanpa `Trx*` baru), `:30-31` (`QBE-CODE-002`, `QBE-CODE-003`: nomor bisnis tidak dialokasikan controller dan tidak memakai Count+1) | **Ready to reuse** | — | — |
| `PPRI-CAP-43` | Pola entity `Inp*` terbaru: audit dan hapus-lunak, konkurensi, satu dokumen aktif per jenis per episode (`RWI-DEC-240`, `INV-RWA-01`) | `InPatientManagement` | `Areas/HealthServices/InPatientManagement/Models/InpAdmissionReferral.cs:23-59` (`IdentityModel`, `Guid RowVersion`); `Repositories/Configurations/HealthServices/InPatientManagement/InpAdmissionReferralConfiguration.cs:33` (`IsConcurrencyToken`), `:36-44` (indeks unik bersaring, misalnya satu permintaan `Pending` per pasien) | **Ready to reuse** | — | — |
| `PPRI-CAP-44` | Kiriman ulang aman dengan `Idempotency-Key` (`FR-RWA-128`) | `InPatientManagement`, `ClinicalManagement` | `InPatientManagement/Controllers/InpatientSurgeryBookingController.cs:75` (header), `Services/InpSurgeryBookingAdapter.cs:43, 171-175` (maksimal 80 karakter, diteruskan ke modul pemilik); pola simpan kolom `IdempotencyKey` dengan indeks unik di `ClinicalManagement/Models/CliNursingIntervention.cs` dan `Repositories/Configurations/HealthServices/ClinicalManagement/CliNursingInterventionConfiguration.cs:136`; penanda `IsReplay` pada `NursingEpisodeWriteGuard.cs` | **Ready to reuse** | — | — |
| `PPRI-CAP-45` | Identitas dan angka dibekukan saat dokumen `Completed` (`FR-RWA-124`) | `InPatientManagement` | Preseden `ClinicalManagement/Models/CliTransferHandover.cs:46 #SnapshotJson` | **Ready to reuse** | Pola salinan beku sudah dipakai serah terima transfer | — |
| `PPRI-CAP-46` | Atestasi petugas: nama lengkap dan jabatan penanda tangan (`RWI-DEC-237`) | Platform/Identity | `BE@671191e Models/ApplicationUser.cs:12 #DisplayName`, `:26 #PrimaryPositionId`, `:58 #PrimaryPosition` (`MstPosition`) | **Reuse with adapter** | Nama dan jabatan diambil dari akun penanda tangan, lalu dibekukan | Akun tanpa `PrimaryPositionId` tercetak tanpa jabatan |
| `PPRI-CAP-47` | Master gambar tanda tangan petugas, kelak (`RWI-DEC-237`) | Platform/Authorization | FE `src/components/view/settings/user/user-settings-client.jsx:233` memanggil `/v1/Auth/signature/register`; BE `Controllers/AuthController.cs:29` (`api/v1/[controller]`) hanya punya `login`, `fingerprint/candidates`, `fingerprint-login`, `permissions`, `me`, `refresh`, dan `logout` (`:65-837`) | **Conflict** | Di luar scope (`RWI-DEC-225`); dicatat sebagai `PPRI-CON-05` | Layar Pengaturan Pengguna gagal saat menyimpan tanda tangan |
| `PPRI-CAP-48` | Slot tanda tangan kertas pasien/keluarga (`RWI-DEC-230`) | `InPatientManagement` | Belum ada slot tanda tangan untuk dokumen admisi. Preseden metode `WrittenPaper` ada di persetujuan (`PatientConsentMethod.cs:6`) | **Missing** | — | — |
| `PPRI-CAP-49` | Kop surat dari profil rumah sakit (`RWI-DEC-247`) | HR Master Data, FE bersama | `MstHospitalSite.cs:18-60` (`SiteCode`, `SiteName`, `TimeZoneId` bawaan `Asia/Jakarta`, `Email`, `PhoneNumber`, `Address`, wilayah); **tanpa logo**. `Corporate/HumanResource/MasterData/Organization/Controllers/HospitalSiteController.cs:23, 127` (`HospitalSite : Read`). FE `src/components/features/surat-component/kop-surat.jsx:4-12` menerima `logoPath`, `hospitalName`, `address1`, `address2`, dan `contact` sebagai props, dengan nilai bawaan identitas rumah sakit client; `signature-section.jsx:20-31` berkota bawaan "Jakarta" | **Reuse with adapter** | Props diisi dari profil rumah sakit. Logo tetap berkas statis frontend sampai ada kolom logo | Pengguna Workspace PPRI butuh `HospitalSite : Read`, kecuali kop dikirim server bersama data cetak |
| `PPRI-CAP-50` | Kode formulir per dokumen, kota penandatanganan, dan batas umur gelang bayi sebagai pengaturan (`RWI-DEC-243`, `RWI-DEC-247`) | `MasterData` | `MasterData/Models/MstInpatientSetting.cs:10-62` (satu baris bawaan berisi angka waktu, interval deposit, dan awalan nomor episode); **tanpa** kode formulir, kota, dan batas umur bayi. `Controllers/InpatientSettingController.cs:26, 57, 89` (`InpatientSetting : Read/Update`) | **Extend** | Kolom baru atau master kecil, menurut `RWI-DEC-247` | — |
| `PPRI-CAP-51` | Resource hak akses `InpatientAdmissionDocument` dengan aksi kustom (`Sign`, `SignAsCro`, `SignAsNurse`, `SignAsHeadNurse`, `Print`, `Cancel`, rupiah) | `InPatientManagement` | Resource belum ada (pencarian: nol hasil). Mekanisme aksi kustom sudah dipakai: `InPatientManagement/Controllers/InpatientEpisodeController.cs:341-342` (`SetIsolation`), `:674` (`Reopen`); `Constants/AccessTypes.cs:3-14` (aksi dikelompokkan ke Read, Create, Update, Delete untuk role access) | **Missing** | Resource baru lewat atribut endpoint, karena registry hak lahir dari atribut | Nama aksi rupiah perlu diseragamkan dengan `ViewAmount` (`PPRI-CON-10`) |
| `PPRI-CAP-52` | Tanda tangan pihak lain tampil paling lambat 30 detik (`FR-RWA-035`) | Platform | `BE@671191e Hubs/QueueHub.cs` dan `Program.cs:1900` (`MapHub<QueueHub>("/hubs/queues")`), satu-satunya hub; FE `src/lib/signalr/signalrHubClient.jsx:149 #connectToHub` (umum per nama hub); penyegaran berkala yang sudah dipakai Rawat Inap: `src/lib/hooks/health-services/inpatient-management/use-inpatient-billing-status.js:27` (`intervalMs`) | **Reuse with adapter** | Penyegaran berkala 30 detik dengan pola yang ada; hub khusus dokumen admisi hanya bila ingin seketika | — |
| `PPRI-CAP-53` | Hak baca yang dibutuhkan bila data dirangkai di layar | Banyak pemilik | Sumber data membutuhkan `InpatientEpisode : Read`, `Patient : Read` (varian `/admin`), `PatientEmergencyContact : Read`, `PatientRelationship : Read`, `PatientEncounter : Read`, `PatientAllergy : Read`, `BillingDeposit : Read`, `HospitalSite : Read`, dan untuk harga `BillingInvoice : Read` atau hak memesan. Aksi tanpa `/admin` di master pasien hanya untuk akun kiosk (`PatientEmergencyContactController.cs:36, 51`) | **Reuse with adapter** | Lihat rekomendasi 1 pada bagian 20.8 | CRO, perawat, dan kasir harus diberi delapan hak modul lain bila seluruh data dirangkai di layar |
| `PPRI-CAP-54` | Asesmen Edukasi tetap di Workspace Keperawatan (`RWI-DEC-226`) | `keperawatan` | `FE@2788966 inpatient-nursing-constants.js:54` (tab `education` pada menu Pengkajian Pasien); `BE@671191e ClinicalInstrumentDraftSeeder.cs:634` (`EDUCATION_ASSESSMENT`) | **Ready to reuse** | Tidak dipakai Workspace PPRI | — |

### 20.5 Jawaban `RWI-OQ-117` dari source

| Butir | Pertanyaan | Jawaban as-is |
|---|---|---|
| (a) | Apakah Billing sudah menyediakan perkiraan harga tindakan, kamar per hari, dan visit dokter tanpa harus ada pesanan lebih dulu? | **Tindakan: ya**, lewat dua jalur. `coverage-status` milik Rawat Inap menerima ID tindakan apa pun untuk satu episode (`PPRI-CAP-36`), dan `coverage-preview` milik Billing menerima ID tarif apa pun untuk satu kunjungan (`PPRI-CAP-37`). Keduanya memakai `InsuranceCoverageService`, sehingga angkanya sama dengan yang dipakai saat pemesanan. **Kamar per hari: ya, lewat tarif**, yaitu `coverage-preview` dengan tarif kamar kelas pasien (`PPRI-CAP-28`). **Visit dokter: tidak ada** konsep tarifnya (`PPRI-CAP-38`) |
| (b) | Apakah ada pengaturan teks catatan aturan biaya? | **Biaya admin: ya**, sebagai kebijakan terstruktur, bukan teks (`PPRI-CAP-39`). **Cito, lebih dari 4 jam, *standby*, dan anestesi: tidak ada** (`PPRI-CAP-40`) |

**Akibat.** `RWI-OQ-117` terjawab sebagian. Sisanya, yaitu tarif visit dokter dan keempat aturan biaya bedah, menjadi wewenang Yasmina (`PPRI-UNK-08`). `EPIC-RWA-09` tetap belum bisa masuk gelombang sampai butir itu diputuskan.

**Contoh.** Rencana appendektomi Tn. Budi dapat dihitung hari ini lewat `coverage-status`, misalnya Rp 8.500.000, dan kamar kelas 2 lewat `coverage-preview`, misalnya 3 × Rp 750.000 = Rp 2.250.000. Baris visit DPJP 3 × Rp 250.000 tidak punya sumber dan tampil "Tarif belum tersedia". Catatan "biaya admin 7%, maksimal Rp 6.000.000" bisa disusun dari kebijakan biaya admin. Catatan "cito +25%" tidak punya sumber di source.

### 20.6 Kontrak as-is yang ditelusuri

Respons sukses seluruh endpoint di bawah berbentuk `ApiResponse<T>`. Kolom "Catatan as-is" menjelaskan perilaku nyata, bukan target. Endpoint baru Workspace PPRI tidak ditulis di sini; usulannya ada di PRD bagian 13.1 dan masih berlabel **Rencana (belum tersedia)**.

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Detail satu episode | `InpatientEpisode : Read` | - | `InpatientEpisodeDetailResponse` | Memuat lokasi kini, DPJP dan perawat aktif, `Version`, dan `Warnings`; **tanpa** jenis kelamin, tanggal lahir, penjamin, dan kontak darurat |
| `GET` | `/{id}/status-history` | Riwayat perubahan status episode | `InpatientEpisode : Read` | - | `List<InpatientStatusHistoryResponse>` | Sumber "petugas yang mengunci admisi" pada IPD |

#### Health Services / Inpatient Management / Inpatient Ancillary Order

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/coverage-status` | Status tanggungan dan perkiraan harga per tindakan | `InpatientEpisode : Read` | query `ItemType` (`Laboratory`, `Radiology`, `Procedure`, `Nutrition`, `Blood`), `ItemIds` | `List<CoverageStatusItem>` | Harga hanya tampil bila pengguna memegang hak memesan jenis itu; Gizi dan Darah tidak pernah diberi harga |

#### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/deposits/episodes/{episodeId}` | Ringkasan deposit episode menurut kebijakan | `BillingDeposit : Read` | - | `EpisodeDepositSummaryResponse` | Nol konsumen frontend |
| `GET` | `/deposit-policies` | Kebijakan deposit untuk penjamin dan kelas | `BillingDeposit : Read` | query `guarantorId`, `patientClassId` | `DepositPolicyResponse` | Tanpa kebijakan: tidak wajib, interval 0 |

#### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/catalog-charges/coverage-preview` | Pratinjau harga dan tanggungan satu tarif untuk satu kunjungan | `BillingInvoice : Read` | query `encounterId`, `tariffId`, `quantity` | `CatalogChargeCoveragePreviewResponse` | Hanya pratinjau; tidak menambah baris tagihan |

#### Health Services / Billing Management / Master Data / Administration Fee Policy

Base URL: `api/v1/health-services/billing-management/master-data/administration-fee-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kebijakan biaya administrasi | `AdministrationFeePolicy : Read` | query saringan | `PagedResult<AdministrationFeePolicyResponse>` | Persentase dan batas atas tersimpan terstruktur |

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/admin/{id}` | Detail pasien | `Patient : Read` | - | `PatientDetailResponse` | Varian tanpa `/admin` hanya untuk akun kiosk |

#### Health Services / Patient Management / Master Data / Patient Emergency Contact

Base URL: `api/v1/health-services/patient-management/master-data/patient-emergency-contacts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/admin` | Kontak darurat pasien | `PatientEmergencyContact : Read` | query saringan | Daftar berhalaman (tipe tidak dideklarasikan di atribut) | Hubungan berupa teks bebas |

#### Health Services / Patient Management / Master Data / Patient Relationship

Base URL: `api/v1/health-services/patient-management/master-data/patient-relationships`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/admin` | Relasi pasien | `PatientRelationship : Read` | query saringan | `ResponsePatientRelationshipPagedResult` | Hubungan berupa enum `PatientRelationshipType` |

#### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Detail kunjungan beserta penjamin utama | `PatientEncounter : Read` | - | `PatientEncounterDetailResponse` | `Payment` memuat jenis penjamin dan nomor kartu, peserta, serta polis |

#### Health Services / Clinical Management / Patient Allergy

Base URL: `api/v1/health-services/clinical-management/patient-allergies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/active-alerts` | Peringatan alergi aktif | `PatientAllergy : Read` | query `patientId` | `List<PatientAllergyAlertResponse>` | Sudah dipakai Workspace Keperawatan |

#### Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar butir administrasi | `InpatientClearanceItem : Read` | query saringan | `InpatientClearanceItemPagedResult` | Tanpa saringan jenis |
| `POST`, `PUT`, `DELETE` | `/`, `/{id}` | Kelola butir | `InpatientClearanceItem : Create/Update/Delete` | DTO butir | `InpatientClearanceItemResponse` | Setiap butir aktif langsung dibaca penutupan episode |

#### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/` | Pengaturan Rawat Inap aktif | `InpatientSetting : Read` | - | `InpatientSettingResponse` | Belum memuat kode formulir, kota, dan batas umur gelang bayi |

#### Corporate / Human Resource / Master Data / Hospital Site

Base URL: `api/v1/corporate/human-resource/master-data/hospital-sites`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar profil rumah sakit | `HospitalSite : Read` | query saringan | Daftar berhalaman (tipe tidak dideklarasikan di atribut) | Tanpa logo |

#### Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses | Request | Response | Catatan as-is |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kasus operasi | `OperatingRoomCase : Read` | query saringan | `PagedResult<OprCaseSummaryResponse>` | Dasar aturan wajib Estimasi Biaya |

**Kode status yang mungkin muncul dan artinya bagi pengguna:**

| Kode | Arti |
|---|---|
| `200` | Data berhasil dibaca |
| `400` | Isian permintaan tidak lengkap atau formatnya salah, misalnya `coverage-status` tanpa satu pun ID tindakan |
| `403` | Pengguna tidak punya hak untuk tindakan itu, misalnya membaca detail pasien lewat varian kiosk |
| `404` | Episode, pasien, atau kunjungan tidak ditemukan |
| `422` | Tarif atau kunjungan tidak valid untuk pratinjau harga, misalnya tarifnya sudah tidak berlaku |

### 20.7 Ketidakcocokan antar-modul dan frontend–backend

| ID | Ketidakcocokan | Bukti | Akibat |
|---|---|---|---|
| `PPRI-CON-01` | QR pasien dari backend berisi No. RM saja, sedangkan QR kartu kiosk frontend berisi judul kartu bernama rumah sakit, nama pasien, dan No. RM | `PPRI-CAP-17` | Ada dua isi QR untuk pasien yang sama. Gelang memakai versi backend |
| `PPRI-CON-02` | Dua nilai interval tindak lanjut deposit: kebijakan Billing (tanpa batas bawah, 0 bila tanpa kebijakan) dan pengaturan Rawat Inap (1–365) | `PPRI-CAP-30` | Jatuh tempo surat dan pengingat daftar pantau bisa berbeda hari |
| `PPRI-CON-03` | `RWI-DEC-233` menyebut "tipe unit perawatan intensif", sedangkan source hanya punya penanda intensif dan isolasi di kamar, bed, dan kelas | `PPRI-CAP-10` | Aturan tipe kamar Formulir GC V1 perlu dirumuskan ulang dengan penanda yang ada |
| `PPRI-CON-04` | `RWI-DEC-232` merujuk "pengaturan Billing" untuk catatan aturan biaya, padahal hanya kebijakan biaya admin yang ada; tarif visit dokter juga tidak ada | `PPRI-CAP-38`, `PPRI-CAP-40` | `EPIC-RWA-09` tidak bisa dibangun utuh tanpa keputusan Billing |
| `PPRI-CON-05` | Frontend memanggil `/v1/Auth/signature/register` yang tidak ada di backend | `PPRI-CAP-47` | Penyimpanan tanda tangan di Pengaturan Pengguna gagal. Di luar scope |
| `PPRI-CON-06` | Komponen cetak yang akan dipakai ulang menanam identitas rumah sakit client dan kota "Jakarta" sebagai nilai bawaan | `PPRI-CAP-09`, `PPRI-CAP-49` | Melanggar `RWI-DEC-247` bila dipakai apa adanya |
| `PPRI-CON-07` | Nilai kepercayaan pasien tercatat di dua tempat: isian `SD_BELIEFS` di Kajian Umum Keperawatan dan dokumen Nilai Kepercayaan admisi | `PPRI-CAP-22` | Isinya bisa saling berbeda |
| `PPRI-CON-08` | `RWI-DEC-241` mengandaikan butir serah terima tersaring per jenis, padahal query penutupan hari ini membaca semua butir aktif | `PPRI-CAP-13` | Penutupan episode tertahan bila saringan tidak dibangun bersamaan |
| `PPRI-CON-09` | Harga di `coverage-status` mengikuti hak memesan (`LabOrder`, `RadOrder`, `PatientProcedure` `: Create`), bukan hak rupiah dokumen admisi | `PPRI-CAP-36` | Petugas admisi yang tidak berhak memesan tidak melihat harga |
| `PPRI-CON-10` | PRD mengusulkan aksi rupiah `ReadAmount`, sedangkan preseden Finishing memakai `ViewAmount` | `PPRI-CAP-05`, `PPRI-CAP-51` | Dua nama untuk konsep yang sama di layar admin hak akses |

### 20.8 Fakta, inferensi, dan rekomendasi

**Fakta baru dari source (`PPRI-FACT`).** Fakta yang sudah tercatat sebagai `RWI-FACT-060` s.d. `RWI-FACT-064` tidak diulang di sini.

| ID | Fakta |
|---|---|
| `PPRI-FACT-01` | Tidak ada satu pun entity, endpoint, resource hak, atau layar dokumen admisi di kedua repository (`PPRI-CAP-06`) |
| `PPRI-FACT-02` | Detail episode tidak memuat jenis kelamin, tanggal lahir, penjamin, dan kontak darurat, sehingga header Workspace PPRI butuh paling sedikit lima sumber (`PPRI-CAP-04`) |
| `PPRI-FACT-03` | Aksi baca master pasien tanpa `/admin` hanya untuk akun kiosk; petugas memakai varian `/admin` dengan hak per resource (`PPRI-CAP-32`) |
| `PPRI-FACT-04` | Penutupan episode membaca semua butir administrasi aktif tanpa saringan, dan butir wajib yang belum ditandai menahan penutupan (`PPRI-CAP-13`) |
| `PPRI-FACT-05` | Perkiraan harga tindakan dan tarif bebas pesanan sudah tersedia lewat dua endpoint yang memakai layanan harga yang sama (`PPRI-CAP-36`, `PPRI-CAP-37`) |
| `PPRI-FACT-06` | Tarif visit dokter rawat inap dan aturan biaya cito, lembur, *standby*, serta anestesi tidak ada di source (`PPRI-CAP-38`, `PPRI-CAP-40`) |
| `PPRI-FACT-07` | Belum ada log cetak di modul mana pun (`PPRI-CAP-19`) |
| `PPRI-FACT-08` | Satu-satunya hub SignalR adalah hub antrean; Rawat Inap memakai penyegaran berkala (`PPRI-CAP-52`) |
| `PPRI-FACT-09` | Profil rumah sakit tidak punya logo, dan pengaturan Rawat Inap tidak punya kode formulir maupun kota (`PPRI-CAP-49`, `PPRI-CAP-50`) |
| `PPRI-FACT-10` | Penanda intensif dan isolasi ada di kamar, bed, dan kelas, bukan di jenis unit layanan (`PPRI-CAP-10`) |
| `PPRI-FACT-11` | Hubungan kontak darurat berupa teks bebas, sedangkan hubungan relasi pasien berupa enum yang tidak membedakan suami dan istri (`Spouse`) (`PPRI-CAP-10`) |
| `PPRI-FACT-12` | Episode tidak punya diagnosis masuk; kunjungan punya keluhan utama dan dokter perujuk luar (`PPRI-CAP-26`) |
| `PPRI-FACT-13` | Untuk `RWI-OQ-120` (MP Benefit, di luar scope): Billing sudah mengenal jenis biaya lain `MAKANAN_PENDAMPING` yang dibebankan kasir sebagai "Biaya Lain-Lain" (`BE@671191e Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs:31, 44`) |
| `PPRI-FACT-14` | Penjamin asuransi dan perusahaan punya penanda `IsAllowExcessPaymentByPatient` (`PPRI-CAP-34`) |

**Inferensi — penilaian agent, bukan fakta.**

1. `SiteCode` profil rumah sakit cocok menjadi kode singkat rumah sakit pada label (`PPRI-CAP-20`).
2. `CountryId` pasien adalah negara alamat, bukan kewarganegaraan (`PPRI-CAP-24`).
3. Tarif `IsConsultationFee` per kelas bisa menjadi calon tarif visit dokter, tetapi itu keputusan Billing (`PPRI-CAP-38`).
4. Karena seluruh dokumen admisi baru dan polanya sudah tersedia (`PPRI-CAP-42` s.d. `45`), risiko terbesar Workspace PPRI bukan pembuatan tabel, melainkan sambungan ke modul yang sudah berjalan: penutupan episode, cetakan bersama, dan hak akses peran non-admisi.

**Rekomendasi — untuk dipertimbangkan desain, bukan keputusan.**

1. **Rangkai data di server.** Endpoint ringkasan, isian bawaan, dan data cetak sebaiknya dibaca di bawah satu resource `InpatientAdmissionDocument`, seperti usulan PRD bagian 13.1. Dengan begitu CRO, perawat, dan kasir tidak perlu diberi delapan hak modul lain (`PPRI-CAP-53`), dan salinan beku dibentuk di satu tempat.
2. **Bangun saringan jenis checklist dalam task yang sama** dengan penambahan butir serah terima (`PPRI-CAP-13`, `PPRI-CON-08`).
3. **Pakai isi QR backend** (No. RM saja) untuk gelang dan label, lalu laporkan QR kartu kiosk sebagai issue terpisah (`PPRI-CON-01`).
4. **Seragamkan nama aksi rupiah** dengan preseden `ViewAmount` (`PPRI-CON-10`).
5. **Pakai penyegaran berkala 30 detik** untuk tanda tangan pihak lain pada MVP, dengan pola `use-inpatient-billing-status.js` (`PPRI-CAP-52`).
6. **Perbaiki kop cetakan persetujuan sekali di komponen bersamanya**, sehingga langkah 8 alur admisi ikut benar (`PPRI-CAP-09`).

### 20.9 Unknown dan pertanyaan penutup

| ID | Pertanyaan | Kenapa tidak bisa dijawab dari source | Pemilik | Memblokir |
|---|---|---|---|---|
| `PPRI-UNK-01` | Status kasus OK mana yang dihitung sebagai "pemesanan ruang bedah yang aktif" sehingga Estimasi Biaya wajib? Calon yang wajar: `Requested`, `Scheduled`, `Ready`, `InProgress`, dan `Postponed` | Definisi bisnis | Muhammad Hamzah | `DESIGN` aturan kelengkapan |
| `PPRI-UNK-02` | Penanda mana yang membuat tipe kamar "Khusus (ICU/Isolasi)" pada Formulir GC V1: `RequiresIsolation` episode, `IsIsolationRoom`/`IsIsolationBed`, `IsIntensiveCare`/`IsIntensiveCareBed`, atau `IsForIntensiveCare` kelas? | `RWI-DEC-233` merujuk jenis unit yang tidak ada | Muhammad Hamzah | `DESIGN` Formulir GC V1 |
| `PPRI-UNK-03` | Hubungan kontak darurat berupa teks bebas: dicocokkan otomatis dengan teks, atau petugas selalu memilih dari daftar? | Keputusan perilaku | Muhammad Hamzah | `DESIGN` Formulir GC V1 dan Data Wali Pelunasan Deposit |
| `PPRI-UNK-04` | "No. Kartu" pada label pasien diambil dari `CardNumberSnapshot` atau `MemberNumberSnapshot`? | Dua kolom ada, dan artinya bisa berbeda per penjamin | Muhammad Hamzah, bila perlu bersama Yasmina | `DESIGN` label |
| `PPRI-UNK-05` | Dari mana "Diagnosa masuk" dan "Dokter perujuk" IPD diambil: keluhan utama, diagnosis utama atau kerja pada kunjungan, dokter perujuk luar, atau garis kosong? | Tidak ada kolom khusus | Muhammad Hamzah | `DESIGN` IPD |
| `PPRI-UNK-06` | Apakah slot "Perawat penerima" pada Serah Terima mensyaratkan pasien sudah menempati bed di unit tujuan, seperti `RWI-DEC-189`? | Keputusan konsistensi | Muhammad Hamzah | `DESIGN` Serah Terima |
| `PPRI-UNK-07` | Apakah pengingat daftar pantau ikut memakai interval kebijakan Billing, atau dua nilai interval tetap dipertahankan? | Ada dua sumber | Muhammad Hamzah bersama Yasmina | Tidak memblokir Workspace PPRI; memengaruhi daftar pantau |
| `PPRI-UNK-08` | Tarif visit dokter rawat inap dan catatan cito, lembur, *standby*, serta anestesi: disediakan Billing, atau catatannya disimpan sebagai teks pengaturan? | Konsepnya belum ada di source | Yasmina | `IMPLEMENTATION` `EPIC-RWA-09` (lanjutan `RWI-OQ-117`) |
| `PPRI-UNK-09` | Apakah Selisih Biaya tetap wajib bila penjamin tidak mengizinkan selisih dibebankan ke pasien (`IsAllowExcessPaymentByPatient = false`)? | Aturan bisnis | Muhammad Hamzah bersama Yasmina | `DESIGN` aturan kelengkapan |
| `PPRI-UNK-10` | Apakah isian `SD_BELIEFS` di Kajian Umum Keperawatan tetap terpisah, atau membaca dokumen Nilai Kepercayaan? | Menyentuh sub-modul `keperawatan` | Muhammad Hamzah | `LATER SLICE` (amandemen keperawatan, di luar `RWI-DEC-225`) |
| `PPRI-UNK-11` | Apakah "hari kerja berikutnya" memperhitungkan libur nasional (`MstHoliday`), atau Senin–Jumat saja? | Aturan bisnis | Muhammad Hamzah bersama Yasmina | Tidak memblokir |
| `PPRI-UNK-12` | Apakah peran CRO dan supervisor admisi sudah ada di lingkungan target, dan siapa pemegang `SignAsHeadNurse`? | Data role ada di database | Admin Akses Role | `IMPLEMENTATION` |

### 20.10 Keterbatasan audit

1. Audit ini hanya pembacaan statis. Tidak ada build, test, maupun aplikasi berjalan, sehingga tidak satu pun status di atas membuktikan perilaku saat dijalankan.
2. Working tree backend tidak bersih, sehingga bukti backend dibaca dari `HEAD`. Frontend bergeser dari `1f889d67c` ke `27889662a` selama audit; bukti frontend sudah diperiksa ulang pada commit terakhir (bagian 20.1). Perubahan backend yang belum di-commit pada `InsuranceCoverageService.cs` dan `MstCompanyGuarantor.cs` (bawaan buku tarif penjamin perusahaan) dapat mengubah perkiraan harga untuk penjamin perusahaan; `PPRI-CAP-36` dan `PPRI-CAP-37` perlu dicek ulang setelah perubahan itu di-commit.
3. Pemetaan role ke permission tersimpan di database dan tidak diperiksa.
4. Isi V1 tidak dibaca ulang.
5. Nomor baris dapat bergeser pada commit berikutnya. Nama simbol yang disebut tetap menjadi patokan.

### 20.11 Handoff dan pemicu impact scan

**Langkah berikutnya yang disarankan:** `grill-me` lanjutan yang singkat untuk `PPRI-UNK-01` s.d. `PPRI-UNK-06` dan `PPRI-UNK-09`, karena semuanya keputusan bisnis yang memengaruhi desain dan pemiliknya sama. `PPRI-UNK-08` diteruskan ke Yasmina. Sesudah itu `requirement-completeness-gate` untuk slice Workspace PPRI, dengan masukan decision log revision `35` atau sesudahnya dan bagian ini.

**Bagian ini menjadi `STALE` bila salah satu hal berikut berubah:**

- **Backend:** `InPatientManagement/Controllers/InpatientEpisodeController.cs`, `InpatientAncillaryOrderController.cs`, `InpatientSurgeryBookingController.cs`; `InPatientManagement/DTOs/InpatientEpisodeDtos.cs`, `InpatientEpisodeReadDtos.cs`; `InPatientManagement/Services/InpDischargeService.Closure.cs`, `InpAncillaryOrderAdapter.cs`; `MasterData/Models/MstInpatientClearanceItem.cs`, `MstInpatientSetting.cs`, `MstTariff.cs`, `MstRoom.cs`, `MstBed.cs`, `MstPatientClass.cs`; `BillingManagement/Billing/Services/BillingDepositService.cs`, `BillingInvoiceService.cs`; `BillingManagement/MasterData/Models/MstDepositPolicy.cs`, `MstAdministrationFeePolicy.cs`; `ClinicalManagement/Services/InsuranceCoverageService.cs`; `ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs`; `PatientManagement/MasterData/Models/MstPatient.cs`, `MstPatientRelationship.cs`, `MstPatientEmergencyContact.cs`; `PatientManagement/MasterData/Controllers/PatientController.cs`; `RegistrationManagement/Models/RegPatientEncounterGuarantor.cs`; `Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs`; `Models/ApplicationUser.cs`; `Controllers/AuthController.cs`; atau ketika entity, endpoint, maupun resource `InpatientAdmissionDocument` pertama kali dibuat.
- **Frontend:** `inpatient-episode-detail-view.jsx`, `inpatient-consent-print-view.jsx`, `inpatient-consent-form.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-episode-constants.jsx`, `nursing-workspace/nursing-workspace-view.jsx`, `src/components/ui/clinical-workspace/*`, `src/components/features/surat-component/*`, `base-patient-card.jsx`, `use-permission.jsx`, `inpatient-admission-patient.service.js`, `inpatient-ancillary-order.service.js`.
- **Keputusan:** jawaban atas `PPRI-UNK-01` s.d. `PPRI-UNK-12`, `RWI-OQ-116`, `RWI-OQ-117`, atau perubahan pada `RWI-DEC-225` s.d. `RWI-DEC-248`.
