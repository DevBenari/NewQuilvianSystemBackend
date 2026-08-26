# Rawat Inap — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Capability-map revision | `1.2` — revisi `1.0` ditambah temuan `RWI-TF-026` s.d. `RWI-TF-028` tentang penguncian "satu konsultasi per kunjungan" dan "satu resep aktif per konsultasi", lalu revisi `1.2` menandai ke-17 pertanyaan penutup pada bagian 12.2 sudah tertutup oleh Closure Pass 21 Agustus 2026 |
| Status | `source-audited`. Dokumen ini **belum** menyatakan modul siap dibangun dan **belum** menyatakan siap produksi |
| Tanggal audit | 21 Agustus 2026 (`Asia/Jakarta`) |
| Masukan bisnis | [`00-interview-decisions.md`](./00-interview-decisions.md), revision `1`, status `draft`, SHA-256 `19a64b418f4004cf5ae1376db1961fcf4e56a3ce9ed8d506c662a0da42ae6692` |
| Daftar periksa audit | `RWI-TRC-001` sampai `RWI-TRC-009` pada dokumen keputusan |
| Decision ID yang dirujuk | `RWI-DEC-001`, `RWI-DEC-002`, `RWI-DEC-003`, `RWI-DEC-007` s.d. `RWI-DEC-035` |
| Backend snapshot | `NewQuilvianSystemBackend` commit `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`, 20 Agustus 2026) |
| Frontend snapshot | `QuilvianSystemFrontendDev` commit `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`, 20 Agustus 2026) |
| Contract version | Belum ada kontrak modul Rawat Inap. Yang berlaku saat ini hanya kontrak as-is milik modul tetangga yang dicatat pada bagian 6 dan 7 |
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
