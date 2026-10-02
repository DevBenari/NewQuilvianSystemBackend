# Kepemilikan Data Bersama

Diaudit pada backend `f2c5090`.

Berkas ini paling menentukan untuk mencegah konflik antar modul. Ia menjawab: **data ini milik
siapa, dan siapa yang boleh mengubahnya.**

## Data bersama dan pemiliknya

| Konsep | Entity canonical | Modul pemilik | Hanya boleh membaca | Dilarang dibuat ulang sebagai |
| --- | --- | --- | --- | --- |
| Pasien | `MstPatient` | HealthServices / Patient Management | Seluruh modul pelayanan | `PatientIGD`, `PatientLab`, `PatientGizi`, `PasienRujukan` |
| Profil tenaga kerja | `MstWorkforceProfile` | Corporate / HR Master Data | Seluruh modul | Salinan tenaga per area |
| Dokter | `MstDoctor` | Corporate / HR Master Data | Seluruh modul pelayanan | `DokterPoli`, `DoctorLab`, `DokterGizi` |
| Pegawai | `MstEmployee` | Corporate / HR Master Data | Seluruh modul | Salinan pegawai per area |
| Episode pelayanan | `TrxPatientEncounter` | HealthServices / Registration Management | Clinical, Pharmacy, Billing, IGD, Operasi | `Kunjungan`, `VisitIGD`, `EpisodeGizi` |
| Konsultasi dokter | `TrxDoctorConsultation` | HealthServices / Clinical Management | Modul pelayanan lain | Konsultasi versi modul sendiri sebelum dibahas |
| Tindakan pasien | `TrxPatientProcedure` | HealthServices / Clinical Management | Operasi, Billing | `TindakanOperasi`, `TindakanIGD` |
| Consent pasien | `TrxPatientConsent` | HealthServices / Clinical Management | Operasi, Rawat Inap | Consent versi modul sendiri |
| Episode rawat inap | `InpEpisode` | HealthServices / InPatient Management | Modul pelayanan lain | `RanapEpisode`, `AdmisiPasien` |
| Poli | `MstClinic` | HealthServices / Master Data | Seluruh modul pelayanan | `PoliRujukan` |
| Unit layanan | `MstServiceUnit` | HealthServices / Master Data | Seluruh modul pelayanan | Salinan unit per modul |
| Kamar dan tempat tidur | `MstRoom`, `MstBed` | HealthServices / Master Data | Rawat inap, IGD, Operasi | Salinan kamar per unit |
| Obat dan bahan | `MstDrug` | HealthServices / Pharmacy Management | Clinical, Billing, Operasi | `ObatIGD`, `BahanOperasi` |
| Tarif | `MstTariff` | HealthServices / Master Data | Billing, Pharmacy, Operasi | Tabel tarif per modul |
| Diagnosis | `MstDiagnosis` | HealthServices / Master Data | Seluruh modul pelayanan | Master diagnosis per modul |

Kolom **Modul pemilik** ditentukan dari lokasi entity di dalam source. Ini bukti struktural,
bukan keputusan organisasi. Siapa yang **berwenang mengubah aturan bisnisnya** belum tertulis
di mana pun, dan itu tercatat sebagai zona konflik `KF-003`.

## Aturan pemakaian data bersama

**1. Simpan penunjuknya, bukan salinannya.**

Modul yang membutuhkan data bersama menyimpan `Id` entity pemilik. Jangan menyalin nama,
alamat, atau nomor identitas ke tabel sendiri.

**2. Snapshot transaksi adalah pengecualian yang sah.**

Penyalinan nilai pada saat transaksi terjadi diperbolehkan bila nilainya memang harus
dibekukan.

> **Contoh sah:** `TrxPatientProcedure` menyimpan `ProcedureNameSnapshot` dan
> `ProcedureCodeSnapshot`. Ketika nama tindakan di master berubah tahun depan, catatan
> tindakan lama tetap menunjukkan nama yang berlaku saat itu.
>
> **Contoh tidak sah:** menyalin nama pasien ke tabel antrean. Nama pasien tidak perlu
> dibekukan; cukup simpan `PatientId` dan baca namanya saat ditampilkan.

**3. Data bersama yang kurang lengkap diajukan ke pemiliknya.**

Modul yang merasa data bersama kurang tidak boleh membuat tabel tandingan. Ia mengajukan
penambahan kolom kepada modul pemilik.

> **Contoh nyata yang sudah terjadi:** modul Operasi membutuhkan `WorkforceProfileId` untuk
> menugaskan perawat. `DoctorOptionResponse` sudah menyertakannya, `EmployeeOptionResponse`
> belum. Penyelesaiannya adalah menambah satu properti pada DTO milik HR, bukan membuat master
> tenaga versi Operasi. Perubahan itu tercatat pada readiness report modul Operasi sebagai
> penyimpangan nomor 5.

**4. Kepemilikan yang belum jelas otomatis menjadi zona konflik.**

Seluruh baris `Belum ditentukan` pada berkas 01 masuk `KF-003` dan perlu diputuskan sebelum
modul baru menulis ke data tersebut.

## Konsep yang perlu diperiksa sebelum modul baru dibuat

Tiga konsep berikut sering muncul kembali dengan nama berbeda pada modul baru. Periksa dulu
sebelum mengusulkan entity:

| Bila modul baru membutuhkan | Sudah ada | Tingkat |
| --- | --- | --- |
| Permintaan konsultasi ke tenaga lain | `TrxDoctorConsultation` | `L4` |
| Penilaian atau asesmen pasien | Entity asesmen di Clinical Management | `L4` |
| Order tindakan untuk pasien | `TrxPatientProcedure` | `L4` |
| Penugasan tenaga ke pasien | `InpDoctorAssignment`, `InpNurseAssignment`, `OprTeamMember` | `L3` sampai `L4` |

Tabel ini **bukan** perintah memakai ulang. Kesesuaiannya untuk kebutuhan baru wajib
ditanyakan kepada pemilik kebutuhan lewat wawancara, karena registry tidak dapat menilai
apakah aturan bisnisnya cocok.
