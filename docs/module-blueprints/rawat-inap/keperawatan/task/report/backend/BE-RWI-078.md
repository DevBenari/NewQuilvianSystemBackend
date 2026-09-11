# Laporan Perubahan Backend — `BE-RWI-078`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-078` |
| Judul | Perawat hanya menulis untuk pasien di unit tempat ia bertugas |
| Slice | Gelombang `KEP-1A` — Rawat Inap Safety Corrections |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian `S5` |
| Trace | `RWI-DEC-100`; `RWI-FACT-022`; `FR-KEP-031` s.d. `FR-KEP-034`; `04-prd-to-mvp.md` bagian 21.3 |
| Contract version | API `0.4.0` bagian 0.A.2; `permission-audit-matrix.md` `0.4.0` bagian 3A — `approved` 11 September 2026 lewat `RWI-DEC-105` |
| Dependency | Approval kontrak `0.4.0` — terpenuhi |
| Klasifikasi | `HIGH` — repository 1 (0), berkas diperiksa > 8 (1), berkas diubah 7 (1), logika bisnis tinggi (1), menetapkan sumber data yang belum diputuskan (1), keamanan dan otorisasi (1), workflow lintas empat permukaan tulis (1), dampak keselamatan klinis (1). Total **7** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `c11904ea`, branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | **Selesai untuk source.** Ketujuh acceptance criteria terpetakan ke source, sumber data unit ditetapkan beserta alasannya. Butir verifikasi yang tidak dijalankan ditulis `NOT RUN` apa adanya pada bagian 6 |

---

## Backend Governance Preflight

| Field | Isi |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | — |
| Pemilik / prefix registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 — `HealthServices \| ClinicalManagement / Clinical \| BUSINESS DOMAIN / MODULE \| Cli \| ACTIVE / LEGACY` |
| Keberlakuan | Campuran. `NEW CODE` untuk dua method baru pada `InpatientClinicalContextService` dan penjaga baru pada empat permukaan tulis; `TOUCHED LEGACY` untuk controller lama yang disentuh |
| Status registry | `ACTIVE / LEGACY`, sudah terdaftar. `QBE-MOD-002` tidak berlaku: **nol** entity operasional baru, **nol** tabel baru, **nol** kolom baru |
| QBE ID yang berlaku | `QBE-MOD-002` — tidak memblokir. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — tidak berlaku, tidak ada rename dan tidak ada pekerjaan database |
| Legacy ratchet | Ditegakkan. Pemakaian `ApplicationDbContext` langsung pada controller yang disentuh **tidak** diubah, dan tidak ada arsitektur generic repository yang diperkenalkan |
| Hardcode role access | **Nol.** Tidak ada `IsInRole`, tidak ada daftar nama peran, nama departemen, nama jabatan, maupun `UserType` yang dipakai sebagai penentu kewenangan. Yang diperiksa adalah kepemilikan dan penempatan pada data |

---

## 1. Masalah yang diperbaiki

`InpatientClinicalContextService` **nol menyebut perawat** pada seluruh berkasnya sebelum task ini.
Satu-satunya pemeriksaan kewenangan yang tersedia adalah `IsDoctorAssignedAsync`, dan ia hanya
dipanggil jalur dokter. Akibatnya, seperti dicatat `RWI-FACT-022` dan `permission-audit-matrix.md`
bagian 3A.1:

> Pengguna mana pun yang memegang `PatientAssessment : Create` dapat menulis pengkajian untuk pasien
> mana pun di rumah sakit.

Hal yang sama berlaku pada rencana asuhan, catatan tindakan, dan catatan terpadu berprofesi perawat.
Dua lubang berbeda hidup berdampingan:

| Lubang | Wujudnya sebelum task ini |
| --- | --- |
| Identitas penulis | Pada pencatatan tindakan, `PerformedByEmployeeId` pada payload dipakai apa adanya bila pegawainya ada dan aktif — siapa pun dapat menyimpan catatan atas nama perawat lain |
| Batas wilayah | Tidak ada satu pun pemeriksaan unit; pasien bangsal mana pun terbuka bagi pemegang hak akses mana pun |

`RWI-DEC-100` menutup keduanya: penulis diambil dari pengguna terautentikasi (`GUARD-INP-07`), dan
kewenangan menulis ditentukan unit tempat episode berada (`GUARD-INP-08`).

---

## 2. Sumber data "unit tempat perawat bertugas" — penetapan dan alasannya

Ini butir Definition of Done yang menuntut penetapan pada task, karena `RWI-DEC-100` dan
`permission-audit-matrix.md` bagian 3A.5 sengaja membiarkannya terbuka.

**Yang ditetapkan.** Rantai penempatan organisasi milik kepegawaian, dibaca dua sisi:

| Sisi | Rantai |
| --- | --- |
| Pasien | `InpEpisode.ServiceUnitId` → `MstServiceUnit.OrganizationUnitId` → `MstOrganizationUnit.DepartmentId` |
| Perawat | `ApplicationUser.EmployeeId` → `MstEmployee.WorkforceProfileId` → baris `WfpOrganizationAssignment` yang periodenya memuat saat itu → `OrganizationUnitId` dan `DepartmentId` |

Cocok pada `OrganizationUnitId` lebih dulu; bila unit pelayanan hanya terpetakan sampai departemen,
cocok pada `DepartmentId`.

**Kenapa rantai ini.**

1. **Sudah berperiode.** `WfpOrganizationAssignment` punya `EffectiveStartDate` dan
   `EffectiveEndDate`, sehingga mutasi perawat antarbangsal terbaca dengan sendirinya. Tidak ada
   tabel baru, tidak ada kolom baru.
2. **Sudah menjadi sumber kebenaran penempatan.** Modul kehadiran, tunjangan, klaim, dan roster
   sudah membaca baris yang sama. Memakai sumber lain akan melahirkan kebenaran kedua.
3. **Tidak menyalin unit ke baris penugasan pasien.** Justru itu yang dilarang bagian 3A.5: unit
   episode berubah ketika pasien dipindahkan, sedangkan salinan tidak.

**Yang ditolak, beserta sebabnya.**

| Calon sumber | Kenapa tidak dipakai |
| --- | --- |
| Kolom unit baru pada `InpNurseAssignment` | **Dilarang tertulis** oleh bagian 3A.5. Ia melahirkan sumber kebenaran kedua yang berbeda sejak perpindahan pasien pertama |
| `TrxShiftAssignment` roster | Menyimpan unit, tetapi `RWI-DEC-100` mencatat rumah sakit ini **belum menjalankan konsep shift perawat sama sekali**. Menggantungkan hak tulis pada roster yang belum terisi berarti menolak seluruh dokumentasi keperawatan pada hari pertama |
| `InpNurseAssignment` sebagai gerbang | Ditolak `RWI-DEC-100`. Maknanya tetap penunjukan perawat penanggung jawab, bukan kunci hak tulis |

**Dua kelonggaran yang disengaja.**

| Keadaan | Perlakuan | Alasannya |
| --- | --- | --- |
| Unit pelayanan hanya terpetakan sampai departemen | Cocokkan pada `DepartmentId` | Pemetaan organisasi tidak selalu turun sampai tingkat bangsal |
| Perawat belum punya satu pun baris penempatan yang berlaku | Jatuh ke `MstEmployee.PrimaryDepartmentId` | Perawat yang datanya belum lengkap tetap dapat mendokumentasikan pasien di departemennya. Perawat yang **sudah** punya baris penempatan dinilai dari baris itu, bukan dari kolom lama — supaya mutasi tidak terabaikan |

**Satu penolakan yang disengaja dan wajib dibaca sebagai syarat data induk.** Bila `MstServiceUnit`
sebuah bangsal **tidak** menyebut `OrganizationUnitId` maupun departemen mana pun, tidak ada yang
dapat dibandingkan, dan penjaganya menjawab `403`. Melewatkannya berarti mengembalikan persis lubang
yang sedang ditutup. **Akibatnya: pemetaan `MstServiceUnit.OrganizationUnitId` untuk seluruh bangsal
rawat inap adalah prasyarat sebelum perubahan ini dinyalakan di lingkungan berjalan.** Lihat bagian
8.

---

## 3. Proses bisnis

**Tujuan.** Dokumentasi keperawatan hanya dapat ditulis perawat yang benar-benar bertugas di unit
tempat pasien dirawat, dan penulisnya adalah orang yang sedang login.

**Pelaku.** Perawat pelaksana dan kepala ruangan.

**Langkah yang berurutan pada setiap aksi tulis.**

1. Pengguna menekan simpan pada salah satu permukaan keperawatan.
2. Backend menemukan pegawai di balik pengguna yang sedang masuk.
3. Backend membaca unit tempat **episode** berada, saat itu juga.
4. Backend menilai apakah pegawai itu bertugas di unit tersebut.
5. Bila ya, catatan tersimpan atas nama pegawai itu. Bila tidak, permintaan ditolak `403` dan **nol
   baris** tersimpan.

**Keadaan dan jawabannya.**

| Keadaan | Jawaban | Alasan |
| --- | --- | --- |
| Perawat menulis untuk pasien **di unitnya**, bukan pasien penanggung jawabnya | `200` / `201` | Jalur normal dinas malam. Ini yang **wajib tetap berhasil** |
| Perawat menulis untuk pasien di unit lain | `403` | `GUARD-INP-08` |
| Pengguna tanpa pemetaan pegawai | `403` | Unitnya tidak dapat dinilai, dan tidak dapat dinilai berarti tidak berwenang |
| Perawat mengirim penanda perawat lain sebagai pelaksana | `403`, nol baris tersimpan | `GUARD-INP-07` |
| Perawat penanggung jawab berganti, perawat lama masih di unit yang sama | `200` | `InpNurseAssignment` memang bukan gerbang |
| Pasien dipindahkan, perawat unit lama menulis | `403` | Unit dibaca dari episode, bukan dari salinan |
| Episode `Closed` atau `Cancelled` | Ditolak seperti sebelumnya | Penjaga lama, tidak berubah |

**Kenapa gerbang perawat lebih longgar daripada gerbang dokter, dan itu bukan kelalaian.** DPJP
melekat pada pasien berhari-hari; perawat berganti tiga shift sehari pada bangsal berisi 20 sampai
30 pasien, dan sistem tidak punya konsep shift perawat. Menuntut penugasan per episode berarti
sekitar 90 baris penugasan manual per hari per bangsal. Dasarnya `RWI-DEC-100`, dan perbedaan itu
dinyatakan tertulis pada `permission-audit-matrix.md` bagian 3A.3.

---

## 4. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| [`Services/InpatientClinicalContextService.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs) | Dua sebab penolakan baru `NurseNotAuthorized = 8` dan `NurseNotIdentified = 9`; properti `IsNurseAuthorized` pada konteks; dua kalimat penolakan `PenolakanPerawatUnitLain` dan `PenolakanPerawatTanpaPegawai`; **dua method baru** `IsNurseOnDutyAtEpisodeAsync` dan `IsNurseOnDutyAtUnitAsync`; parameter opsional `nurseEmployeeId` pada `ResolveAsync` |
| [`Services/NursingActorService.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/NursingActorService.cs) | `ApplicationUser.EmployeeId` dibaca sebagai rantai identitas **pertama** yang berbasis data, sebelum penautan lewat profil tenaga kerja dan sebelum surel; kalimat penolakan `PenolakanPegawaiPihakLain` |
| [`Services/NursingCarePlanService.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/NursingCarePlanService.cs) | Dua penjaga baru `ResolvePenulisEpisodeAsync` dan `ResolvePenulisBerwenangAsync`; keduanya dipasang pada `OpenAsync`, `AddItemAsync`, `UpdateItemAsync`, `EvaluateItemAsync`, dan `CloseItemAsync`. `CloseItemAsync` kini menerima `ClaimsPrincipal?` |
| [`Services/NursingInterventionService.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Services/NursingInterventionService.cs) | `ResolvePerformerAsync` **dihapus** dan digantikan penolakan tegas atas `PerformedByEmployeeId` pihak lain; penjaga unit pada `RecordAsync`; method publik `EnsureNurseUnitAuthorityAsync`; `UpdateAsync` dan `FinalizeAsync` kini menerima `ClaimsPrincipal?` dan memanggil penjaga itu |
| [`Controllers/PatientAssessmentController.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs) | `NursingActorService` disuntikkan; dua penjaga baru `ValidateNursingUnitAuthorityAsync` dan `EnsureNursingUnitAuthorityAsync`; dipasang pada pembuatan, pembaruan, penyelesaian, pembatalan, dan koreksi |
| [`Controllers/PatientIntegratedProgressNoteController.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs) | `NursingActorService` disuntikkan; penjaga `EnsureNursingUnitAuthorityAsync` yang **hanya** berlaku bagi catatan berprofesi `Nurse` pada perawatan rawat inap; dipasang pada pembuatan, pembaruan, dan pembatalan |
| [`Controllers/NursingCarePlanController.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingCarePlanController.cs) | Meneruskan `User` ke `CloseItemAsync` |
| [`Controllers/NursingInterventionController.cs`](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingInterventionController.cs) | Meneruskan `User` ke `UpdateAsync` dan `FinalizeAsync`; penjaga unit dipanggil sebelum koreksi ditambahkan |

**Nol tabel baru, nol kolom baru, nol migration.** `InpNurseAssignment` tidak berubah satu kolom
pun, sesuai butir DoD yang menyebutnya bernama.

**Nol `[AccessPermission]` baru dan nol `[AccessAction]` baru.** Sesuai
`permission-audit-matrix.md` bagian 3A.6. Akibat yang wajib disadari: **test hak akses lama akan
tetap lulus tanpa disentuh, sehingga ia tidak dapat dipakai sebagai bukti bahwa penjaga baru
bekerja.**

**Nol registrasi DI baru.** `NursingActorService` dan `InpatientClinicalContextService` sudah
terdaftar `AddScoped` pada `Program.cs` sejak `BE-RWI-059`.

---

## 5. Keputusan teknis yang perlu dibaca

**Kenapa penjaga dipasang pada jalur tulis, bukan pada satu tempat di tengah.** Empat permukaan
keperawatan tidak berbagi satu pintu masuk: pengkajian lewat controller, rencana asuhan dan tindakan
lewat service, catatan terpadu lewat controller yang dipakai seluruh profesi. Memaksa satu pintu
berarti menulis ulang empat slice yang sudah selesai. Yang dipakai bersama adalah **penilaiannya**,
dan itu memang berada di satu tempat: `InpatientClinicalContextService`.

**Kenapa catatan terpadu disaring tiga kali sebelum penjaga bekerja.** `TrxPatientIntegratedProgressNote`
dipakai dokter, farmasi, gizi, bidan, fisioterapi, laboratorium, dan radiologi, pada rawat jalan
maupun IGD. Penjaganya karena itu hanya menyala bagi `ProfessionType` bernilai `Nurse` **dan**
catatan yang menempel pada perawatan rawat inap. Tanpa penyaringan itu, gerbang unit akan menutup
jauh lebih banyak daripada yang diminta kontrak.

**Kenapa pengkajian disaring jenisnya.** `TrxPatientAssessment` memuat pengkajian keperawatan dan
kajian medis pada satu tabel. Kajian medis punya penjaganya sendiri lewat
`ValidateMedicalAssessmentRuleAsync`, dan pengkajian pada kunjungan yang tidak menaungi perawatan
rawat inap — poliklinik, medical check-up, dan IGD — dilewatkan apa adanya. Tanpa itu, screening
rawat jalan akan tertutup karena tidak punya unit rawat inap untuk dibandingkan.

**Penjaga kepemilikan baris tetap terpisah dan tetap berlaku.** `VAL-KEP-06` menolak penyuntingan
catatan tindakan milik orang lain lewat `tindakan.CreateBy != actorUserId`, dan penjaga itu tidak
disentuh. Gerbang unit menjawab pertanyaan yang berbeda, sehingga catatan perawat lain tetap tidak
dapat disunting walaupun keduanya bertugas di bangsal yang sama.

**Kiriman ulang idempotent dijawab sebelum penjaga unit.** Pada pencatatan tindakan, kunci
idempotency diperiksa paling awal dan tetap menjawab `200` apa adanya; pada finalisasi, catatan yang
sudah final tetap dijawab sebagai pengulangan. Memasang penjaga di depan keduanya akan mengubah
jawaban idempotent yang sudah dikunci `VAL-KEP-15`.

---

## 6. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(rules/backend/TEST_POLICY.md)`

| Butir | Hasil |
| --- | --- |
| QBE preflight dan conformance | **Lulus.** Lihat bagian Backend Governance Preflight |
| Pemeriksaan hardcode role access | **Lulus.** Penelusuran `IsInRole`, nama peran, nama departemen, dan `UserType` pada seluruh berkas yang diubah bernilai nol hasil sebagai penentu kewenangan |
| Pemeriksaan butir hak akses | **Lulus.** Nol `[AccessAction]` dan nol `[AccessPermission]` ditambah, diubah, maupun dihapus oleh task ini. Satu pencabutan `PatientIntegratedProgressNote : Delete` yang terlihat pada working tree **sudah ada sebelum task ini dimulai** dan milik `BE-RWI-075` |
| Pemeriksaan kolom `InpNurseAssignment` | **Lulus.** Berkas modelnya tidak disentuh; nol kolom baru |
| Pemeriksaan migration | **Lulus.** Nol berkas migration dibuat task ini. Satu berkas belum terlacak `20260911000000_AddAssignmentRoleToInpDoctorAssignment.cs` **sudah ada pada working tree sebelum task ini dimulai** dan milik `BE-RWI-074` |
| Review diff dan cakupan | **Lulus.** Tujuh berkas disentuh task ini, seluruhnya di dalam `Areas/HealthServices/ClinicalManagement/`. Tidak ada berkas di luar modul pemilik yang disentuh |
| Keadaan working tree saat mulai | **Dicatat, bukan diubah.** Branch `MHamzah` sudah memuat pekerjaan `BE-RWI-073`, `BE-RWI-074`, dan `BE-RWI-075` yang **belum di-commit** pemilik, termasuk pada `InpatientClinicalContextService.cs`, `PatientIntegratedProgressNoteController.cs`, `DoctorConsultationController.cs`, `PatientDiagnosisController.cs`, `InPatientManagement/**`, dan satu berkas migration. `git diff --stat` karena itu menampilkan lebih banyak berkas daripada cakupan task ini. **Tidak satu pun pekerjaan itu dibatalkan, ditimpa, atau dirapikan** |
| Pemeriksaan pemanggil yang tertinggal | **Lulus.** `ResolvePerformerAsync` bernilai nol rujukan setelah dihapus; seluruh pemanggil `CloseItemAsync`, `UpdateAsync`, dan `FinalizeAsync` sudah menyesuaikan tanda tangannya |
| Pemeriksaan keseimbangan blok | **Lulus.** Jumlah `{` dan `}` seimbang pada ketujuh berkas |
| Verifikasi kontrak API | **Lulus secara telaah.** Ketetapan `api-contract.md` `0.4.0` bagian 0.A.2 terpetakan seluruhnya; lihat bagian 7 |
| `dotnet restore` | `NOT RUN` — pemilik meminta task ditutup tanpa build pada sesi ini |
| `dotnet build QuilvianSystemBackend.sln` | `NOT RUN` — pemilik meminta task ditutup tanpa build pada sesi ini. Ini **butir DoD yang dikecualikan pemilik**, bukan butir yang lulus |
| `AC-KEP-044` s.d. `AC-KEP-049` — uji integrasi | `NOT RUN` — backend tidak memelihara project test otomatis, dan pemilik tidak meminta pembuatannya pada task ini |
| `AC-KEP-050` — seluruh skenario negatif memakai peran nyata, bukan SuperAdmin | `NOT RUN`. **Peran yang dipakai belum dapat dicatat karena ujinya belum dijalankan.** Butir ini tetap terbuka |
| Verifikasi manual atau runtime | `NOT RUN` |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bunyi | Pemetaan ke source | Keadaan |
| --- | --- | --- | --- |
| `AC-KEP-044` | Perawat menulis untuk pasien **di unitnya**, bukan pasien penanggung jawabnya → `200` | `IsNurseOnDutyAtUnitAsync` **tidak membaca** `InpNurseAssignment` sama sekali. Yang dibandingkan hanya penempatan organisasi perawat terhadap unit episode | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-045` | Perawat menulis untuk pasien di unit lain → `403` | Ketidakcocokan `OrganizationUnitId` dan `DepartmentId` mengembalikan `false`, dan setiap pemanggil menjawab `403` beserta `PenolakanPerawatUnitLain` | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-046` | Pengguna tanpa pemetaan pegawai → `403` | `ResolveEmployeeIdAsync` mengembalikan kosong, dan keempat permukaan menjawab `403` beserta `PenolakanPerawatTanpaPegawai` | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-047` | Perawat mengirim penanda perawat lain → `403`, nol baris tersimpan atas nama pihak lain | `RecordAsync` membandingkan `PerformedByEmployeeId` terhadap pegawai pengguna yang sedang masuk dan **keluar sebelum** `_dbContext.Add`, sehingga nol baris tersimpan | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-048` | Perawat penanggung jawab berganti, perawat lama tetap dapat menulis selama masih di unit yang sama | Sama seperti `AC-KEP-044`: `InpNurseAssignment` tidak ikut dibaca, sehingga pergantiannya tidak berpengaruh | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-049` | Pasien dipindahkan, perawat unit lama menulis → `403` | `IsNurseOnDutyAtEpisodeAsync` membaca `InpEpisode.ServiceUnitId` **saat itu juga** pada setiap permintaan; tidak ada salinan unit di mana pun | **Terpetakan.** Uji `NOT RUN` |
| `AC-KEP-050` | Seluruh skenario negatif memakai peran nyata, bukan SuperAdmin; peran yang dipakai tercatat pada laporan | Nol butir hak akses baru dibuat, sehingga penjaga ini bekerja **di bawah** mesin hak akses dan tidak dapat dilewati SuperAdmin maupun peran mana pun | **Belum terpenuhi.** Ujinya `NOT RUN`, dan karena itu **peran yang dipakai belum dapat dicatat** |

**Definition of Done.**

| Butir DoD | Keadaan |
| --- | --- |
| Ketujuh acceptance criteria terpetakan ke source | **Terpenuhi** |
| Sumber data unit ditetapkan dan alasannya tertulis pada laporan | **Terpenuhi.** Bagian 2 |
| Nol kolom baru pada `InpNurseAssignment` | **Terpenuhi** |
| Peran nyata dipakai pada test negatif | **Tidak terpenuhi.** `NOT RUN`; ujinya belum dijalankan |
| Nol `[AccessPermission]` baru | **Terpenuhi** |
| `dotnet build` tanpa error baru | **Dikecualikan pemilik pada sesi ini.** `NOT RUN` |
| Kesesuaian QBE dan preflight engineering | **Terpenuhi** |

---

## 8. Catatan penutup

**Delta kontrak — satu kemampuan lama tertutup, dan itu memang yang diminta.** `BE-RWI-061` membuka
jalur "perawat mencatatkan tindakan rekannya" lewat `PerformedByEmployeeId` pada payload.
`api-contract.md` `0.4.0` bagian 0.A.2 dan `AC-KEP-047` menutupnya: penanda perawat lain kini
dijawab `403`. Pencatatan atas nama orang lain karena itu tinggal pada jalur pengganti milik
`MedicalRecordManagement`, sama seperti pada koreksi. **Ini perubahan perilaku yang terlihat
pengguna**, dan ia disebut di sini supaya tidak terbaca sebagai regresi.

**Prasyarat data induk sebelum dinyalakan.** Pemetaan `MstServiceUnit.OrganizationUnitId` untuk
seluruh bangsal rawat inap wajib terisi sebelum perubahan ini berjalan di lingkungan mana pun yang
dipakai. Bangsal yang tidak terpetakan akan menolak seluruh dokumentasi keperawatannya dengan `403`.
Penolakan itu disengaja — melewatkannya berarti membuka kembali lubang yang sedang ditutup — tetapi
akibatnya harus diketahui sebelum penerapan, bukan sesudah.

**Satu method menjadi tidak terpakai.** `NursingActorService.IsActiveEmployeeAsync` kehilangan
pemanggilnya ketika `ResolvePerformerAsync` dihapus. Ia dibiarkan apa adanya karena bersifat publik
dan tidak berbahaya; pencabutannya bukan cakupan task ini.

**Frontend belum menyesuaikan.** Layar keperawatan belum menampilkan pesan `403` yang baru, dan
jalur pencatatan atas nama rekan masih dapat terkirim dari sana. Penyesuaiannya pekerjaan frontend
terpisah dan **tidak** dikerjakan di sini.

**Migration dan database.** Nol. Tidak ada migration yang dibuat, dan tidak ada eksekusi database
yang diminta maupun dijalankan.

**Git.** Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy yang dilakukan.

**Task berikutnya.** `AC-KEP-050` menunggu uji dengan peran nyata, dan `V2-UNK-01` pada
`BE-RWI-077` tetap milik `MedicalRecordManagement`.
