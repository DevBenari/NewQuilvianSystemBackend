# Module Ownership & Prefix Registry

Dokumen ini adalah wewenang yang disetujui untuk kepemilikan dan penamaan entity operasional. Nilai Lifecycle adalah metadata registry: `PLANNED`, `ACTIVE`, `LEGACY`, `DEPRECATED`. Tidak adanya folder di Git bukan wewenang perencanaan.

Persetujuan registry hanya memberi wewenang penamaan dan kepemilikan. Ia **tidak** memberi wewenang implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul berstatus `PLANNED`. Contohnya, `InsuranceManagement` / `Ins` / `PLANNED` menetapkan calon pemilik penamaannya bila kelak diberi wewenang terpisah; entri itu tidak memberi wewenang pekerjaan produksi Insurance.

> Tabel di bawah dibaca mesin oleh `tooling/qbe/Invoke-QbeConformanceCheck.ps1`. Pertahankan lima kolom beserta urutannya, pertahankan judul kolom pertama `Area`, dan tulis nilai Category serta Lifecycle persis dalam bentuk aslinya (`BUSINESS DOMAIN`, `MASTER / REFERENCE`, `SHARED PLATFORM CAPABILITY`, `ACTIVE`, `LEGACY`, `PLANNED`, `DEPRECATED`). Menerjemahkan nilai-nilai itu akan mematahkan checker.

| Area | Module/pemilik | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE / LEGACY |
| Corporate | WorkforceCore / WorkforceProfileManagement / Workforce Profile | BUSINESS DOMAIN / MODULE | Wfp | ACTIVE / LEGACY |
| Finance | Finance | BUSINESS DOMAIN | Fin | ACTIVE |
| Administrator / HealthServices | Master / Reference / MasterData | BUSINESS DOMAIN / MASTER / REFERENCE | Mst | ACTIVE |
| HealthServices | ClinicalManagement / Clinical | BUSINESS DOMAIN / MODULE | Cli | ACTIVE / LEGACY |
| HealthServices | RegistrationManagement / Registration | BUSINESS DOMAIN / MODULE | Reg | ACTIVE / LEGACY |
| HealthServices | PatientManagement operational | BUSINESS DOMAIN / MODULE | Pat | ACTIVE |
| HealthServices | PharmacyManagement / Pharmacy | BUSINESS DOMAIN / MODULE | Phm | ACTIVE / LEGACY |
| HealthServices | EmergencyInstallationManagement / Emergency | BUSINESS DOMAIN / MODULE | Emg | ACTIVE / LEGACY |
| HealthServices | BillingManagement / Billing | BUSINESS DOMAIN / MODULE | Bil | ACTIVE |
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | ACTIVE |
| HealthServices | RadiologyManagement / Radiology | BUSINESS DOMAIN / MODULE | Rad | PLANNED |
| HealthServices | InPatientManagement / Inpatient | BUSINESS DOMAIN / MODULE | Inp | ACTIVE |
| HealthServices | OutPatientManagement / Outpatient | BUSINESS DOMAIN / MODULE | Out | PLANNED |
| HealthServices | InsuranceManagement / Insurance | BUSINESS DOMAIN / MODULE | Ins | PLANNED |
| Corporate/HumanResource | WorkflowManagement / Workflow | SHARED PLATFORM CAPABILITY | Wfl | ACTIVE / LEGACY |
| HealthServices | OperatingRoomManagement / Operating Room | BUSINESS DOMAIN / MODULE | Opr | PLANNED |
| HealthServices | MedicalRecordManagement / Medical Record | BUSINESS DOMAIN / MODULE | Mrc | ACTIVE |

## Kepanjangan prefix

| Prefix | Kepanjangan |
|---|---|
| Hrd | Human Resource |
| Wfp | Workforce Profile |
| Fin | Finance |
| Mst | Master / Reference |
| Cli | Clinical |
| Reg | Registration |
| Pat | Patient |
| Phm | Pharmacy |
| Emg | Emergency |
| Bil | Billing |
| Lab | Laboratory |
| Rad | Radiology |
| Inp | Inpatient |
| Out | Outpatient |
| Ins | Insurance |
| Wfl | Workflow |
| Opr | Operating Room |
| Mrc | Medical Record |

`DoctorAndScheduleManagement` berkategori MASTER / REFERENCE menurut bukti saat ini dan tidak memiliki prefix operasional tersendiri. Untuk entity operasional baru pakai `<PrefixPemilikDisetujui><KonsepBisnis>` tanpa pengulangan nama pemilik, misalnya `RegPatientEncounter`, `EmgVisit`, `WflInstance`, `LabOrder`.

## Prosedur pendaftaran modul/prefix baru

Berlaku setiap kali muncul folder sub-domain/submodule baru, atau folder yang sudah ada tetapi belum pernah terdaftar, dan folder itu akan memuat model persisted. **Daftarkan lebih dulu di sini, baru buat modelnya** (QBE-MOD-002, QBE-MOD-003).

1. **Tetapkan penempatan.** Tentukan Area, Module/pemilik, dan Submodule sebenarnya dari capability tersebut, berdasarkan bukti — bukan dari nama task atau nama layar.
2. **Cek registry.** Bila pemiliknya sudah ada di tabel, tidak ada prefix baru: pakai prefix yang tercatat.
3. **Tentukan prefix bila memang belum ada.** Tiga huruf, PascalCase, singkatan dari konsep pemiliknya, dan belum dipakai baris lain. Tulis kepanjangannya secara eksplisit, contoh `Wfp` = *Workforce Profile*.
4. **Ajukan barisnya lengkap.** Area, Module/pemilik, Category, Prefix, Lifecycle. Kolom Module/pemilik memuat nama folder yang sesungguhnya supaya checker dapat mencocokkan path source, contoh `WorkforceCore / WorkforceProfileManagement / Workforce Profile`.
5. **Minta persetujuan pemilik modul**, lalu tambahkan barisnya ke tabel dan catat di *Catatan perubahan lifecycle*.
6. **Baru buat model pertama** memakai prefix tersebut, beserta file, configuration, DbSet, dan nama tabel yang sepaket.

Selama langkah 1–5 belum tuntas, pembuatan entity operasional berstatus `BLOCKED`. Jangan memakai `Trx*` sebagai jalan pintas (QBE-NAM-001) dan jangan mengarang prefix dari nama folder (QBE-NAM-004).

### Contoh terisi — `Wfp`

| Langkah | Hasil |
|---|---|
| Penempatan | `Areas/Corporate/HumanResource/WorkforceCore/` dan `.../WorkforceProfileManagement/` |
| Konsep pemilik | Profil tenaga kerja: alamat, pendidikan, keluarga, dokumen, penugasan |
| Prefix | `Wfp` = *Workforce Profile* |
| Baris registry | `Corporate` / `WorkforceCore / WorkforceProfileManagement / Workforce Profile` / `BUSINESS DOMAIN / MODULE` / `Wfp` / `ACTIVE / LEGACY` |
| Contoh entity | `WfpEducation`, `WfpAddress`, `WfpFamilyMember`, `WfpPositionAssignment` |

Submodule Human Resource lain yang memuat entity `Wfp*` sebelum pendaftaran ini — `CredentialingManagement`, `PayrollManagement`, `LifecycleManagement`, `SchedulingManagement`, `LearningAndDevelopment`, `LeaveManagement`, `EmployeeRelationManagement`, `OccupationalHealthManagement`, `OvertimeManagement`, `PerformanceManagement` — diperlakukan sebagai legacy. Entity operasional baru di submodule tersebut memakai prefix pemilik yang berlaku pada baris registry-nya, atau menempuh prosedur di atas bila submodule itu memang perlu pemilik dan prefix sendiri.

## QBE-MOD-002

Modul yang memiliki entity operasional persisted MUST punya entri registry berstatus APPROVED sebelum entity pertamanya dibuat. Developer/Codex wajib menetapkan Area, Module, pemilik, Category, prefix, dan perilaku tabelnya. Bila entri itu tidak ada, pembuatan entity operasional berstatus `BLOCKED`; prefix tidak boleh dikarang dari nama folder.

Task yang berwenang MAY membuat atau merencanakan folder modul yang belum terdaftar, tetapi entity operasional persisted pertamanya tetap `BLOCKED` sampai entri registry-nya disetujui. Jadi `HealthServices/RehabilitationManagement` tidak boleh membuat entity operasional `Reh*`, `Rhb*`, maupun `Trx*` tanpa keputusan registry.

## QBE-MOD-003

Folder Area/Module/Submodule baru — atau folder yang sudah ada namun belum terdaftar — yang akan memuat model persisted MUST didaftarkan di tabel ini sebelum file model pertama dibuat. Kelalaian mendaftarkan bukan alasan memakai prefix modul tetangga, `Trx*`, atau prefix karangan sendiri.

## Catatan perubahan lifecycle

| Tanggal | Modul | Perubahan | Wewenang |
|---|---|---|---|
| 2026-09-04 | Master / Reference / `Mst` | `Module/pemilik` bertambah alias `MasterData`; `Category` menjadi `BUSINESS DOMAIN / MASTER / REFERENCE` | Yoga Aji Pratama, kontributor `master-data`, sesi 2026-09-04. **Mencabut penghalang QBE-MOD-002 atas pembuatan entity `Mst*` baru.** Dua hal menghalanginya sekaligus, dan keduanya baru terlihat ketika entity `Mst*` baru pertama kali dibuat sejak checker ada. **(a)** Nama pemilik `Master / Reference` tidak pernah cocok dengan folder sebenarnya, `Areas/HealthServices/MasterData`, sehingga checker melaporkan tidak ada pemilik yang cocok; alias `MasterData` ditambahkan mengikuti pola baris `Wfp` yang juga mencatat folder yang sudah ada. **(b)** `Category` berbunyi `MASTER / REFERENCE` saja, sementara checker hanya mengakui baris ber-`Category` diawali `BUSINESS DOMAIN` sebagai pemberi wewenang entity baru. Akibat keduanya, **tidak ada** modul yang berwenang membuat satu pun data induk baru — walaupun persetujuan bisnisnya sudah ada. Ditemukan saat `BE-EXT-02` menambahkan `MstReferralInstitution` dan `MstReferralDoctor`, yang disetujui `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001` (`LAB-COORD-004`). Berlaku untuk seluruh entity `Mst*` berikutnya, bukan hanya kedua tabel itu. Prefix, lifecycle, dan pemilik sebenarnya tidak berubah. |
| 2026-08-24 | InPatientManagement / `Inp` | `PLANNED` → `ACTIVE` | Muhammad Hamzah, blueprint `RWI-BP-001` keputusan `RWI-DEC-068`. Mencabut penghalang QBE-MOD-002 atas pembuatan entity operasional `Inp*`. Eksekusi database di luar lokal dan deployment tetap merupakan wewenang terpisah. |
| 2026-08-28 | WorkforceCore / WorkforceProfileManagement / `Wfp` | Baris baru — pendaftaran prefix `Wfp` = *Workforce Profile* | Instruksi pemilik repository, sesi 2026-08-28. Mencatat 40 entity `Wfp*` yang sudah ada di `Areas/Corporate/HumanResource/` yang selama ini belum terdaftar. Wewenangnya penamaan dan kepemilikan saja; tidak memberi wewenang implementasi, migration, maupun deployment. |
| 2026-08-31 | MedicalRecordManagement / `Mrc` | `PLANNED` → `ACTIVE` | Yoga Aji Pratama, blueprint `RM-BP-001` keputusan `RM-DEC-029`. Membuka normalisasi LEGACY MIGRATION empat entity `Trx*` rekam medis menjadi `Mrc*` beserta tabel fisiknya (QBE-NAM-003), diterapkan lewat migration `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix`. Wewenang ini mencakup source dan pembuatan migration; eksekusi database di luar dev pemilik dan deployment tetap merupakan wewenang terpisah. |
| 2026-09-03 | LaboratoryManagement / `Lab` | Normalisasi `LEGACY MIGRATION` dua entity `Trx*` | Yoga Aji Pratama, pemilik modul, sesi 2026-09-03. Membuka normalisasi `TrxLabSpecimen` → `LabSpecimen` dan `TrxLabTransitionHistory` → `LabTransitionHistory` beserta tabel fisiknya (QBE-NAM-003), diterapkan lewat migration `20260903094528_RenameLaboratoryTrxTablesToLabPrefix`. Seluruh entity Laboratorium kini berprefix `Lab`, kecuali `MstLabRejectionReason` yang tetap `Mst` sesuai catatan 2026-09-02. Wewenang ini mencakup source, pembuatan migration, dan eksekusi ke dev pemilik; deployment tetap merupakan wewenang terpisah. |
| 2026-09-02 | LaboratoryManagement / `Lab` | `PLANNED` → `ACTIVE` | Muhammad Hamzah, blueprint `LAB-BP-001` lewat permintaan `LAB-REQ-002`. Mencabut penghalang QBE-MOD-002 atas entity operasional `Lab*` dan atas migration modul Laboratorium. Sekaligus menetapkan prefix data induk milik Laboratorium: entity baru memakai `Lab`, sehingga dua tabel batas nilai bernama `LabValueBound` dan `LabValueOption`; `MstLabRejectionReason` yang sudah ada diperlakukan legacy dan tidak dinamai ulang. Wewenang ini mencakup source dan pembuatan migration; eksekusi database di luar dev pemilik dan deployment tetap merupakan wewenang terpisah. |
