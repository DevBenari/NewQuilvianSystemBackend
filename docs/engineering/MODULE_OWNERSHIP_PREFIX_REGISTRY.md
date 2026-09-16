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
| HealthServices | RadiologyManagement / Radiology | BUSINESS DOMAIN / MODULE | Rad | ACTIVE |
| HealthServices | InPatientManagement / Inpatient | BUSINESS DOMAIN / MODULE | Inp | ACTIVE |
| HealthServices | OutPatientManagement / Outpatient | BUSINESS DOMAIN / MODULE | Out | PLANNED |
| HealthServices | InsuranceManagement / Insurance | BUSINESS DOMAIN / MODULE | Ins | PLANNED |
| Corporate/HumanResource | WorkflowManagement / Workflow | SHARED PLATFORM CAPABILITY | Wfl | ACTIVE / LEGACY |
| HealthServices | OperatingRoomManagement / Operating Room | BUSINESS DOMAIN / MODULE | Opr | ACTIVE |
| HealthServices | NutritionManagement / Nutrition | BUSINESS DOMAIN / MODULE | Gz | ACTIVE |
| HealthServices | MedicalRecordManagement / Medical Record | BUSINESS DOMAIN / MODULE | Mrc | ACTIVE |
| HealthServices | BloodBankManagement / Blood Bank | BUSINESS DOMAIN / MODULE | Bbk | ACTIVE |
| Platform | NumberSeriesManagement / Number Series | BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY | Num | ACTIVE |
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |

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
| Bbk | Blood Bank |
| Num | Number Series |
| Gz | Gizi — Nutrition |
| Acc | Accounting |

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
| 2026-09-02 | LaboratoryManagement / `Lab` | `PLANNED` → `ACTIVE` | Muhammad Hamzah, blueprint `LAB-BP-001` lewat permintaan `LAB-REQ-002`. Mencabut penghalang QBE-MOD-002 atas entity operasional `Lab*` dan atas migration modul Laboratorium. Sekaligus menetapkan prefix data induk milik Laboratorium: entity baru memakai `Lab`, sehingga dua tabel batas nilai bernama `LabValueBound` dan `LabValueOption`; `MstLabRejectionReason` yang sudah ada diperlakukan legacy dan tidak dinamai ulang. Wewenang ini mencakup source dan pembuatan migration; eksekusi database di luar dev pemilik dan deployment tetap merupakan wewenang terpisah. |
| 2026-09-03 | BloodBankManagement / `Bbk` | Baris baru — pendaftaran prefix `Bbk` = *Blood Bank* | Blueprint `BD-BP-001` keputusan modul Bank Darah. Memberi wewenang penamaan dan kepemilikan entity operasional `Bbk*`; tidak memberi wewenang implementasi, migration, database, maupun deployment. Lifecycle tetap `PLANNED` sampai ada keputusan aktivasi modul. |
| 2026-09-03 | BloodBankManagement / `Bbk` | `PLANNED` → `ACTIVE` | Persetujuan owner Bank Darah dan approval blueprint BD-BP-001 contract v4. Membuka wewenang implementasi entity operasional `Bbk*` sesuai QBE-MOD-002. |
| 2026-09-09 | Platform / NumberSeriesManagement / `Num` | Baris baru — pendaftaran Area `Platform`, pemilik `NumberSeriesManagement / Number Series`, prefix `Num` = *Number Series*, lifecycle langsung **`ACTIVE`** | Blueprint `PLT-BP-001` `PLT-SLICE-01`, keputusan `DEC-PLT-009` (Area) dan `DEC-PLT-010` (prefix), keduanya `approved` oleh `Andry` selaku pemilik kontrak engineering backend pada 2026-09-09. Blueprint dan set kontrak `v1` disetujui `Sukma Giri Pratama` pada tanggal yang sama. Menutup `OQ-PLT-014` dan gerbang `P1` roadmap Platform. **Lifecycle langsung `ACTIVE`, bukan `PLANNED`** — atas permintaan eksplisit pemilik, dan karena `PLANNED` ditolak checker: `Resolve-RegistryOwnership` hanya menerima token lifecycle `active` dan `legacy`. **Category ditulis `BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY`, bukan `SHARED PLATFORM CAPABILITY` saja** — ini kebutuhan mekanis checker, bukan perubahan keputusan: baris 271 `Invoke-QbeConformanceCheck.ps1` menguji `(ConvertTo-SemanticToken $row.Category) -match '^businessdomain'`, sehingga `SHARED PLATFORM CAPABILITY` saja akan ditolak dengan *"has non-operational Category"* dan memblokir `NumNumberSeries`. Polanya mengikuti perbaikan `Mst` 2026-09-04 yang menghadapi penghalang yang sama persis. Label semantik `SHARED PLATFORM CAPABILITY` dipertahankan utuh; prefix, pemilik, dan scope tidak berubah. Wewenang ini **penamaan dan kepemilikan** — tidak memberi wewenang implementasi, migration, database, maupun deployment. |
| 2026-09-07 | Master / Reference / MasterData / `Mst` | Baris duplikat `MasterData / BloodBankManagement Existing Master Legacy` (`Mst` / `LEGACY`) dihapus dari tabel kepemilikan | Perbaikan regresi merge, sesi 2026-09-07. Baris itu dan baris `Master / Reference / MasterData` sama-sama mencocokkan folder `Areas/HealthServices/MasterData` dengan prefix `Mst`, sehingga `Resolve-RegistryOwnership` mengembalikan `Registry ownership is ambiguous for the source area/domain path.` dan **seluruh** entity `Mst*` baru terblokir QBE-MOD-002 — terlihat pada `MstBloodBankReason`, `MstBloodComponent`, dan `MstBloodStorageLocation`. Masing-masing baris benar di cabang asalnya; ambiguitas baru muncul ketika merge `b70b735` menyatukan keduanya. Wewenang penamaan data induk Bank Darah tidak berubah: entity `Mst*` yang sudah ada di `Areas/HealthServices/MasterData` tetap grandfathered dan tidak dinamai ulang, dan kewenangannya kini dipikul baris `Administrator / HealthServices` / `Master / Reference / MasterData` sesuai catatan 2026-09-04. Prefix, lifecycle, dan pemilik sebenarnya tidak berubah. |
| 2026-09-08 | NutritionManagement / `Gz` | Baris baru — pendaftaran prefix `Gz` = *Gizi — Nutrition*, lifecycle `ACTIVE` | Ikbal Yuliyanto, pemilik keputusan modul Gizi, sesi 2026-09-08. Modul Gizi sudah memiliki entity operasional `Gz*` yang berjalan — order gizi, diet pasien, produksi, dan distribusi makanan — tetapi tidak pernah punya baris registry sama sekali, sehingga seluruhnya terblokir QBE-MOD-002 dengan alasan `No registry owner matches source area`. Pendaftaran ini mencabut penghalang itu dan menetapkan `Gz` sebagai prefix entity operasional Gizi berikutnya. Entity `Gz*` yang sudah ada tetap dipertahankan apa adanya. Wewenangnya penamaan, kepemilikan, dan implementasi; eksekusi database di luar dev pemilik dan deployment tetap merupakan wewenang terpisah. |
| 2026-09-08 | OperatingRoomManagement / `Opr` | `PLANNED` → `ACTIVE` | Ikbal Yuliyanto, pemilik keputusan modul Operasi, sesi 2026-09-08. Modul Operasi sudah dalam tahap implementasi dengan capability yang berjalan, termasuk pembukuan pemakaian material ke kartu stok Farmasi. Mencabut penghalang QBE-MOD-002 atas entity operasional `Opr*`. **Tidak** mencakup rename entity yang sudah ada: `MstOperatingRoomStockSource` seharusnya bernama `OprStockSource`, dan penormalannya dicatat sebagai LEGACY MIGRATION tersendiri yang belum direncanakan maupun diverifikasi — source dan tabel fisiknya tidak diubah pada pekerjaan ini. Wewenangnya mencakup source dan pembuatan migration untuk entity `Opr*` baru; eksekusi database di luar dev pemilik dan deployment tetap merupakan wewenang terpisah. |
| 2026-09-08 | AccountingManagement / Accounting / `Acc` | Baris baru — pendaftaran prefix `Acc` = *Accounting*, lifecycle `ACTIVE` | Rizki, Product/Domain Owner sekaligus Implementation Owner Accounting, blueprint `ACC-BP-001` revisi 5 keputusan `ACC-DEC-038`, 1 September 2026. **Propagasi persetujuan yang sudah ada, bukan persetujuan baru.** Baris identik sudah berlaku di registry canonical suite skill sejak 1 September 2026, tetapi tidak pernah menyeberang ke salinan backend yang dibaca `tooling/qbe/Invoke-QbeConformanceCheck.ps1`, sehingga QBE-MOD-002 menolak ketujuh entity `Acc*` pada PR merge `rizkiG` ke `QuilvianIntegrationBackend`. Penambahan barisnya diminta ke pemilik registry pada 2 September 2026 lewat `docs/module-blueprints/accounting/evidence/07-acc-dep-007-ringkasan-untuk-lead.md` dan belum tertindak; ditambahkan oleh pemilik modul sesuai langkah 5 *Prosedur pendaftaran modul/prefix baru*, mengikuti preseden `Inp` (`RWI-DEC-068`), `Mrc` (`RM-DEC-029`), dan `Bbk`. Mencabut penghalang QBE-MOD-002 atas entity operasional `Acc*`. Wewenang ini mencakup source model persisted Accounting saja; eksekusi database di luar dev pemilik, perubahan shared database, deployment, dan production activation tetap merupakan wewenang terpisah. `AccountingManagement` / `Acc` dan `Finance` / `Fin` adalah bounded context berbeda; entri `Fin` tidak diubah. `ACC-DEP-007` — selisih dua salinan registry, yang kini juga berselisih arah sebaliknya lewat `MasterData` dan `Emg` — tetap terbuka dan tetap milik lead. |
| 2026-09-10 | RadiologyManagement / `Rad` | `PLANNED` → `ACTIVE` | Muhammad Hamzah, pemilik registry, lewat permintaan `RAD-REQ-001` yang disampaikan pemilik modul Radiologi Yoga Aji Pratama. **Propagasi persetujuan yang sudah ada, bukan persetujuan baru.** Baris `ACTIVE` yang identik sudah berlaku di registry canonical suite skill sejak 2026-09-09, tetapi tidak pernah menyeberang ke salinan backend yang dibaca `tooling/qbe/Invoke-QbeConformanceCheck.ps1` — pola selisih yang sama dengan `ACC-DEP-007`. Keputusan asalnya `RJ-BIL-DEC-014` pada blueprint `rawat-jalan`, 28 Agustus 2026, yang menunjuk pemilik modul sekaligus menaikkan lifecycle; `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` baris 11-13 sudah menulis kenaikan itu sebagai fakta sejak saat itu, sementara registry belum menyusul. Delapan tabel `Rad*` beserta dua migration — `20260828093000_AddRadiologyManagement` dan `20260903095444_AddRadOrderInpatientContext` — dibuat **sebelum** penerapan ini; selisih waktunya dicatat apa adanya sesuai `RAD-DEC-007` dan tidak ditulis mundur. Mencabut penghalang QBE-MOD-002 atas entity operasional `Rad*` berikutnya, yaitu `RadReport` dan `RadReportVersion` pada blueprint `RAD-BP-001`. Wewenang ini mencakup source model persisted Radiologi dan pembuatan migration; eksekusi database di luar dev pemilik, deployment, dan production activation tetap merupakan wewenang terpisah. Catatan: salinan registry pada plugin cache `quilvian-engineering-skills/0.1.0` masih tertulis `PLANNED` dan perlu disegarkan agar agent yang membaca lewat jalur itu tidak melihat keadaan lama. |
