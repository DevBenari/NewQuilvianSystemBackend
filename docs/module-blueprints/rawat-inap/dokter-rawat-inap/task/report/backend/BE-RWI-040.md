# Laporan Perubahan Backend — `BE-RWI-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-040` |
| Judul | Dokumen klinis menyimpan konteks perawatannya |
| Slice | `DOK-MVP-1` — fondasi konteks, kolom, tabel visite, pelonggaran |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-040` |
| Trace | `02-backend-architecture.md` §4.1 s.d. §4.4; `data/data-dictionary.md` §2 s.d. §5; `INV-DOK-01`; `INT-DOK-09` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-039` — **selesai**, lihat [laporan](BE-RWI-039.md) |
| Klasifikasi | `HEAVY`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 0, kontrak API 0, database 2, keamanan/auth 1, UI/workflow 0, ditambah satu tingkat karena menyentuh empat tabel klinis yang sedang melayani pasien |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; model dan configuration `ClinicalManagement`, `Migrations/`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | 🟡 **Sebagian.** Lima dari enam acceptance criteria terbukti; kriteria 4 — migration maju dan mundur berhasil — **belum dapat dibuktikan** karena tidak ada PostgreSQL yang tersedia di lingkungan kerja |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli`, `ACTIVE` |
| Applicability | `TOUCHED LEGACY` — keempat tabel adalah entity lama berawalan `Trx*`; kolom ditambahkan tanpa menamai ulang apa pun |
| QBE berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-MOD-001`, `QBE-ENUM-001`, `QBE-AUD-001` |
| QBE tidak berlaku | `QBE-NAM-001` dan `QBE-NAM-003` — task ini **tidak** menamai ulang entity `Trx*`. Normalisasinya adalah task tersendiri dengan approval pemilik arsitektur backend, dan sengaja tidak diselipkan ke sini |
| Archetype | Perubahan bentuk data pada aggregate transaksi klinis; tidak ada permukaan API baru |
| Database authority | Pembuatan migration `PROVIDED` oleh acceptance criteria task. **Eksekusi migration tidak diberikan dan tidak dilakukan** terhadap database mana pun |
| Frontend | Tidak disentuh. Kolom baru seluruhnya nullable atau bernilai bawaan sehingga bentuk balasan yang dibaca frontend tidak berubah |

---

## 1. Masalah yang diperbaiki

Pertanyaan "catatan ini milik perawatan A atau perawatan B" hanya bisa dijawab dengan penelusuran
berlapis: dari catatan ke kunjungan, dari kunjungan ke perawatan. Untuk satu catatan itu tidak
terasa. Untuk membuka lini masa perkembangan pasien yang dirawat sepuluh hari, penelusuran itu
diulang untuk setiap baris.

Masalah kedua lebih halus dan lebih berbahaya. Sistem hanya menyimpan **kapan catatan diketik**,
bukan **kapan pemeriksaannya terjadi**. Padahal keduanya sering berbeda.

**Contoh nyata.** Dokter melakukan visite pagi pukul 07.40, lalu baru sempat mengetik catatannya
pukul 11.00 setelah selesai di poliklinik. Dengan hanya satu waktu tersimpan, lini masa
perkembangan pasien menempatkan pemeriksaan itu pada pukul 11.00 — di belakang catatan perawat
pukul 09.00 yang sebenarnya terjadi **setelah** pemeriksaan dokter. Urutan yang terbaca menjadi
kebalikan dari urutan yang sungguh terjadi.

Masalah ketiga: catatan pada lembar terpadu belum punya tempat untuk menyimpan bahwa DPJP sudah
membacanya, dan siapa yang membacanya.

Masalah keempat: satu tindakan yang dikirim ulang karena sambungan terputus dapat melahirkan dua
baris tindakan — dan karena tindakan menerbitkan fakta ke Billing, dua baris berarti dua tagihan.

---

## 2. Proses bisnis

**Tujuan.** Empat tabel klinis dapat menyimpan perawatan yang menaunginya, waktu klinis yang
sebenarnya, keadaan verifikasi DPJP, dan kunci permintaan.

**Pelaku.** Bukan pengguna langsung. Kolom-kolom ini dipakai jalur pembuatan dan pembacaan dokumen
klinis pada task-task berikutnya.

**Pemicu.** Migration diterapkan oleh pemilik modul.

**Langkah yang berurutan.**

1. Tiga enum lahir: keadaan verifikasi catatan terpadu, dan dua enum peran serta keadaan visite
   yang dipakai `BE-RWI-041`. Ditambah satu enum jenis pengkajian.
2. Tiga belas kolom ditambahkan pada empat tabel klinis.
3. Index lini masa per perawatan dibentuk pada ketiga tabel dokumen, ditambah index parsial pada
   catatan terpadu yang **hanya** memuat baris yang menunggu verifikasi.
4. Satu unique parsial dibentuk untuk kunci permintaan tindakan.
5. Baris lama menerima nilai bawaan dan tidak disentuh.

**Aturan yang berlaku.**

- **Seluruh kolom konteks nullable.** Catatan poliklinik, IGD, dan medical check-up memang tidak
  punya perawatan rawat inap. Memaksanya wajib akan mematahkan seluruh baris yang sudah ada.
- **Keadaan verifikasi bernilai bawaan "tidak diwajibkan", bukan "menunggu".** PRD menuliskan
  "**bila** verifikasi DPJP diwajibkan". Menyalakan "menunggu" sebagai bawaan membuat setiap
  catatan perawat langsung terhitung menunggu verifikasi pada rumah sakit yang tidak
  mewajibkannya, dan daftar pantau penuh sejak hari pertama.
- **Index parsial hanya pada baris yang menunggu.** Daftar pantau hanya membaca baris itu;
  meng-index seluruh baris memboroskan tanpa dipakai.
- **Nilai enum lama tidak boleh bergeser.** Nilainya disimpan sebagai angka, sehingga menggeser
  angka berarti menulis ulang arti baris yang sudah tersimpan.
- **Kunci permintaan dijaga database, bukan hanya oleh service.** Dua permintaan yang tiba
  benar-benar bersamaan tidak dapat dicegah oleh pemeriksaan di dalam aplikasi saja.

**Jalur tidak normal.** Bila migration gagal di tengah, langkah mundurnya mengembalikan bentuk
tabel apa adanya: seluruh kolomnya nullable atau bernilai bawaan, sehingga tidak ada data yang
hilang.

**Hasil akhirnya.** Empat tabel klinis siap menerima dokumentasi rawat inap tanpa satu pun kolom
milik modul lain di luar keempatnya ikut berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Keempat model dan configuration-nya, ditambah `CliClinicalMilestoneFact` sebagai pembanding
  entity berprefix pemilik
- `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`
- `data/data-dictionary.md` §2 s.d. §5 dan §11, `02-backend-architecture.md` §4.1 s.d. §4.4, §6, §7
- `docs/module-blueprints/rawat-inap/keperawatan/data/data-dictionary.md` §2 — untuk bentuk kolom
  yang dipakai bersama

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` | Tiga kolom: `InpEpisodeId`, `ClinicalDateTime`, `PhysicianVisitId` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` | Lima kolom: `InpEpisodeId`, `VerificationStatus`, `VerifiedAt`, `VerifiedByUserId`, `VerificationDueAt` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Tiga kolom: `InpEpisodeId`, `PhysicianVisitId`, `IdempotencyKey` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | Dua kolom: `InpEpisodeId`, `AssessmentType` |
| `Areas/HealthServices/ClinicalManagement/Enums/CpptVerificationStatus.cs` | **Baru.** `NotRequired`, `Pending`, `Verified`, `Overdue` |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientAssessmentType.cs` | **Baru.** Empat nilai keperawatan ditambah `MedicalInitial` dan `MedicalReassessment` |
| `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` | Bentuk kolom, foreign key ke perawatan, index lini masa |
| `Repositories/Configurations/HealthServices/TrxPatientIntegratedProgressNoteConfiguration.cs` | Bentuk kolom, dua foreign key, index lini masa, index keadaan, index parsial daftar pantau |
| `Repositories/Configurations/HealthServices/TrxPatientProcedureConfiguration.cs` | Bentuk kolom, foreign key, index lini masa, unique parsial kunci permintaan |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Bentuk kolom, foreign key, dua index |
| `Migrations/20260903092936_AddInpatientClinicalContextColumns.cs` | **Baru.** Migration tiga belas kolom |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientClinicalSchemaTests.cs` | **Baru.** Uji bentuk kolom, index, dan nilai enum |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Tidak ada endpoint, DTO, maupun bentuk balasan yang berubah. Kolom baru belum dibaca maupun ditulis satu pun endpoint |
| Database | **Tiga belas kolom baru pada empat tabel `ClinicalManagement`.** Sebelas nullable; dua wajib dengan nilai bawaan, yaitu keadaan verifikasi bernilai `NotRequired` dan jenis pengkajian bernilai `Initial`. Ditambah lima index biasa, satu index parsial, satu unique parsial, dan lima foreign key. Satu migration dihasilkan: `20260903092936_AddInpatientClinicalContextColumns`. **Migration belum diterapkan ke database mana pun, termasuk lokal** |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada metadata hak akses yang disentuh. `VerifiedByUserId` sengaja terpisah dari penulis catatan supaya verifikator tidak pernah menggantikan penulis asli — `INV-DOK-11` |

**Rincian tiga belas kolom.**

| Tabel | Kolom | Wajib | Bawaan |
| --- | --- | :---: | --- |
| `TrxDoctorConsultation` | `InpEpisodeId` | Tidak | kosong |
| `TrxDoctorConsultation` | `ClinicalDateTime` | Tidak | kosong |
| `TrxDoctorConsultation` | `PhysicianVisitId` | Tidak | kosong |
| `TrxPatientIntegratedProgressNote` | `InpEpisodeId` | Tidak | kosong |
| `TrxPatientIntegratedProgressNote` | `VerificationStatus` | **Ya** | `NotRequired` |
| `TrxPatientIntegratedProgressNote` | `VerifiedAt` | Tidak | kosong |
| `TrxPatientIntegratedProgressNote` | `VerifiedByUserId` | Tidak | kosong |
| `TrxPatientIntegratedProgressNote` | `VerificationDueAt` | Tidak | kosong |
| `TrxPatientProcedure` | `InpEpisodeId` | Tidak | kosong |
| `TrxPatientProcedure` | `PhysicianVisitId` | Tidak | kosong |
| `TrxPatientProcedure` | `IdempotencyKey` | Tidak | kosong |
| `TrxPatientAssessment` | `InpEpisodeId` | Tidak | kosong |
| `TrxPatientAssessment` | `AssessmentType` | **Ya** | `Initial` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menyentuh satu pun endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Tiga belas kolom terbentuk dengan nullability sesuai kamus data | Sebelas nullable, dua wajib bernilai bawaan | `PASS` | `InpatientClinicalSchemaTests.TigaBelasKolomKonteks_TerbentukSesuaiKamusData` |
| Index lini masa per perawatan pada tiga tabel dokumen | Ketiganya ada | `PASS` | `…IndexLiniMasaPerPerawatan_Terbentuk` |
| Jumlah nilai enum jenis pengkajian dan angka nilainya | Enam nilai; empat nilai keperawatan menempati angka `0` s.d. `3`, dua nilai kajian medis `4` dan `5` | `PASS` | `…EnumJenisPengkajian_MemuatDuaNilaiKajianMedis` |
| Jumlah nilai enum keadaan verifikasi | Empat nilai, bawaan `NotRequired` bernilai `0` | `PASS` | `…EnumKeadaanVerifikasi_MemuatEmpatNilai` |
| Unique parsial kunci permintaan tindakan | Unique dengan penyaring `IsDelete` | `PASS` | `…KunciPermintaanTindakan_DijagaUniqueParsial` |
| Pembangkitan SQL migration arah maju | 186 baris SQL dihasilkan tanpa galat | `PASS` | `dotnet ef migrations script AmendLabValueBoundUniquenessAndSortOrder RelaxSingleConsultationAndPrescriptionForInpatient` |
| Pembangkitan SQL migration arah mundur | 119 baris SQL dihasilkan tanpa galat; seluruh kolom, index, dan foreign key task ini dikembalikan | `PASS` | Perintah yang sama dengan urutan terbalik |
| **Uji migration maju dan mundur terhadap PostgreSQL** | **Tidak dijalankan** | `NOT RUN` | Lihat "Tidak dijalankan" |
| `dotnet test` seluruh berkas uji SQLite | `Failed: 0, Passed: 219` | `PASS` | Keluaran perintah |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:**

- **Uji migration maju dan mundur terhadap PostgreSQL sungguhan.** Lingkungan kerja tidak memiliki
  PostgreSQL lokal, dan Docker Desktop tidak berjalan sehingga container PostgreSQL sekali pakai
  tidak dapat dinyalakan. Menjalankannya terhadap database bersama dilarang tegas `AGENTS.md`, dan
  itu tidak dilakukan. Yang berhasil dibuktikan adalah **SQL kedua arah dihasilkan lengkap dan
  tanpa galat**, bukan bahwa keduanya benar-benar berjalan di atas PostgreSQL.
- Eksekusi migration ke database mana pun. Wewenangnya memang terpisah dan tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga belas kolom terbentuk sesuai kamus data, seluruhnya nullable kecuali keadaan verifikasi yang bernilai bawaan tidak-diwajibkan | Terpenuhi, **dengan satu selisih** | `…TigaBelasKolomKonteks_TerbentukSesuaiKamusData` dan migration. Selisihnya: kolom wajib bernilai bawaan ada **dua**, bukan satu — lihat catatan di bawah |
| 2. Index lini masa per perawatan terbentuk | Terpenuhi | `…IndexLiniMasaPerPerawatan_Terbentuk` |
| 3. Enum jenis pengkajian bertambah dua nilai kajian medis **tanpa mengubah nilai lama** | Terpenuhi, **dengan catatan** | `…EnumJenisPengkajian_MemuatDuaNilaiKajianMedis`. Enum-nya belum pernah ada di source, sehingga ia **dibuat** pada task ini — lihat catatan di bawah |
| 4. Migration maju dan mundur berhasil | **Belum terpenuhi** | SQL kedua arah dihasilkan lengkap dan tanpa galat, tetapi **belum dijalankan** terhadap PostgreSQL. Tidak ada PostgreSQL yang tersedia di lingkungan kerja |
| 5. Baris lama menerima nilai bawaan dan **tidak disentuh** | Terpenuhi pada bentuknya | Migration hanya menambah kolom; sebelas nullable dan dua bernilai bawaan. Tidak ada satu pun perintah `UPDATE` terhadap baris lama di dalam migration |
| 6. Nol kolom milik modul lain di luar keempat tabel ini yang berubah | Terpenuhi | Migration `20260903092936` hanya menyentuh `TrxDoctorConsultation`, `TrxPatientIntegratedProgressNote`, `TrxPatientProcedure`, dan `TrxPatientAssessment` |

**Catatan kriteria 1 — dua kolom wajib, bukan satu.** Kriteria menyebut satu-satunya kolom wajib
adalah keadaan verifikasi. Kolom kedua yang juga wajib adalah `AssessmentType` pada tabel
pengkajian, yang memang harus wajib supaya baris lama terbaca sebagai pengkajian awal — bentuk itu
yang tertulis pada kamus data sub-modul `keperawatan`, dan kriteria 1 sendiri menyuruh mengikuti
kamus data. Tanpa `AssessmentType`, jumlah kolomnya menjadi dua belas, bukan tiga belas seperti
yang diminta kriteria. Keduanya bernilai bawaan sehingga baris lama tetap tidak disentuh.

**Catatan kriteria 3 — enum-nya dibuat, bukan ditambah.** Blueprint mengasumsikan enum
`PatientAssessmentType` sudah ada karena sub-modul `keperawatan` yang memintanya lebih dulu.
Penelusuran source menemukan enum itu **belum pernah dibuat**, dan roadmap `keperawatan` masih
berstatus `DRAFT_STALE`. Baris "Dependency lintas sub-modul" pada kartu task ini memberi wewenang
untuk keadaan persis ini: "siapa pun yang mendarat lebih dulu membuatnya". Enum dibuat di sini
dengan empat nilai keperawatan sesuai kamus data `keperawatan` ditambah dua nilai kajian medis.
Karena nilai lamanya belum pernah ada, "tanpa mengubah nilai lama" terpenuhi tanpa perlu
diuji terhadap data.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | **Belum** — kriteria 4 belum terbukti |
| Satu migration | Terpenuhi — `20260903092936_AddInpatientClinicalContextColumns` |
| Uji maju-mundur lulus | **Belum terpenuhi** — tidak ada PostgreSQL yang tersedia |
| Laporan menyatakan migration belum diterapkan di luar lokal | Terpenuhi — migration belum diterapkan **di mana pun**, termasuk lokal |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas task ini |
| Masalah yang diketahui | Kolom `PhysicianVisitId` lahir pada migration ini tanpa foreign key; foreign key-nya dipasang `BE-RWI-041` bersama tabel `CliPhysicianVisit`. Urutan ini disengaja dan mengikuti urutan dependency roadmap |
| Risiko tersisa | Migration belum pernah dijalankan. Sebelum diterapkan ke database mana pun, pemilik modul wajib menjalankan uji maju-mundur terhadap PostgreSQL lebih dulu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada bagian task ini |
| Status Git | Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Menyalakan PostgreSQL sekali pakai lalu menjalankan uji migration maju-mundur untuk menutup kriteria 4. Setelah itu status task dapat dinaikkan menjadi selesai |
