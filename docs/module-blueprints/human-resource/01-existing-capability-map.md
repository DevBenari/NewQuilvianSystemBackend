# Human Resource — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Capability-map revision | `1.4` — **revisi `1.4` (12 November 2026) menambah bagian 16: impact scan Penjadwalan (`S-B4`) dan Kehadiran (`S-B1`) untuk 12 menu `PRD-HRD-PK-001` dan keputusan `HRD-DEC-194` s.d. `HRD-DEC-214`, pada BE `a437ae42` / FE `6a5658269`; baris baru `HRD-CAP-67` s.d. `HRD-CAP-91`, pertanyaan penutup `HRD-CQ-01` s.d. `HRD-CQ-04`.** Revisi `1.3` — **revisi `1.3` (3 November 2026) menambah bagian 15: impact scan HR di luar Workforce Core dan Perencanaan Tenaga Kerja (`S-D1`) pada BE `26066c76` / FE `66b7e5600`.** Revisi `1.2` — **revisi `1.2` (2 November 2026) menambah bagian 14: impact scan terbatas Workforce Core & Employee 360 pada BE `396c8363` / FE `9c4000724`.** Revisi `1.1` — revisi `1.0` 27 Agustus 2026; revisi `1.1` hari yang sama **menarik `HRD-TF-001`** yang terbukti temuan palsu, dan menaikkan `HRD-CAP-26` dari `REPAIR` menjadi `READY TO REUSE` |
| Status | `source-audited`. **Untuk area 12 menu Penjadwalan & Kehadiran, bacaan yang berlaku adalah bagian 16 (revisi `1.4`, BE `a437ae42` / FE `6a5658269`).** **Sejak revisi `1.3`, keadaan bukti setiap baris `HRD-CAP-01` s.d. `HRD-CAP-27` tercatat pada bagian 15.3** (14 `CURRENT`, 13 `STALE`); bagian 14 diperiksa ulang terbatas pada bagian 15.4; `S-D1` diaudit mendalam pada bagian 15.5 pada HEAD 3 November 2026. Dokumen ini **belum** menyatakan modul siap dibangun dan **belum** menyatakan siap produksi |
| Tanggal audit | 27 Agustus 2026 (`Asia/Jakarta`) |
| Masukan bisnis | [`00-interview-decisions.md`](./00-interview-decisions.md), revision `0`, status `draft`, SHA-256 `f3e2a1633449210d69d1cd1e10cc11972670a2818563c5f53269caf33f005067` |
| Decision ID yang dirujuk | `HRD-DEC-003`, `HRD-DEC-004`, `HRD-DEC-005`, `HRD-DEC-006`, `HRD-DEC-007`; pertanyaan `HRD-Q-01`, `HRD-Q-03`, `HRD-Q-05` |
| Backend snapshot | `NewQuilvianSystemBackend` commit `ecdc135444f0110482c9702212bcea30043983c8` (branch `AndryZain`) |
| Frontend snapshot | `QuilvianSystemFrontendDev` commit `2a1cea7841a4433f8637d486204e60314c09d131` (branch `AgentCodexFrontend`) |
| Contract version | Belum ada kontrak modul HR. Yang berlaku hanya kontrak as-is yang dicatat pada bagian 6 dan 7 |
| Cara audit | Pembacaan statis: model/entity, konfigurasi Entity Framework, migration, `DbSet`, route controller, atribut hak akses, registrasi DI, seeder, konstanta endpoint frontend, Redux slice, hook, route App Router, menu sidebar, dan inventaris test |
| Batas tulis | Hanya dokumen ini. Tidak ada satu baris source aplikasi yang diubah, tidak ada build, tidak ada migration, tidak ada eksekusi database |

> **Cara membaca dokumen ini.** Dokumen ini menjawab satu pertanyaan: **apa yang sudah ada di
> dalam sistem hari ini, dan sejauh mana hal itu dapat dipakai ulang.** Dokumen ini tidak
> merancang tabel baru, tidak menetapkan API baru, dan tidak memutuskan aturan bisnis. Semua
> pertanyaan yang muncul dikumpulkan pada bagian 12.

**Singkatan bukti.**

- `BE@ecdc135` berarti repository `NewQuilvianSystemBackend` pada commit `ecdc135444f0110482c9702212bcea30043983c8`.
- `FE@2a1cea7` berarti repository `QuilvianSystemFrontendDev` pada commit `2a1cea7841a4433f8637d486204e60314c09d131`.

Contoh cara membaca satu baris bukti:

> `BE@ecdc135 Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendancePeriodController.cs:143 [HttpPost("{id:guid}/close")]`

Artinya: pada backend commit `ecdc135`, berkas `AttendancePeriodController.cs` baris 143,
terdapat endpoint untuk menutup periode kehadiran.

---

## 1. Batas audit

### 1.1 Yang diperiksa

| Klaster | Isi yang ditelusuri |
| --- | --- |
| Identity/Master Owner | Master data HR: organisasi, jabatan, shift, payroll, kredensial, pelatihan, kinerja, cuti, lembur, workflow |
| Actor/Workforce | `MstWorkforceProfile`, `MstEmployee`, `MstDoctor`, `MstExternalUser`, dan berkas profil turunannya |
| Episode/Transaction Owner | Kehadiran harian, permohonan cuti, permohonan lembur, periode payroll, kasus kredensial |
| Location/Resource | Lokasi kerja, kalender kerja, unit organisasi, pusat biaya |
| Workflow/Status | `WorkflowManagement`, matriks persetujuan, delegasi, riwayat status |
| Documentation/Record | Dokumen pegawai, sertifikat, lisensi, rekam kesehatan kerja |
| Financial | Payroll, tunjangan, potongan, benefit, reimbursement |
| Authorization/Audit | `[Authorize]`, `[AccessController]`, `[AccessAction]`, `AccessTypes` |
| External Integration | Serah terima payroll, titik sentuh ke Health Services, Finance, Identity |
| Konsumen Frontend | Route App Router, menu sidebar, konstanta endpoint, Redux slice, hook, test |

### 1.2 Yang tidak termasuk audit ini

- Merancang schema, API, atau alur layar target. Itu pekerjaan `/qv-design`.
- Menjalankan aplikasi, membuka database, atau memeriksa isi data produksi.
- Memperbaiki cacat yang ditemukan. Semua temuan hanya dicatat, tidak dikerjakan.
- Menilai kebenaran aturan bisnis. Audit hanya menyatakan apa yang dilakukan source.

### 1.3 Keterbatasan metode yang harus diketahui pembaca

Pencarian konsumen frontend dilakukan dengan mencocokkan teks endpoint. Sebagian URL di
frontend dibentuk dengan *template literal*, misalnya
`` `${WORKFORCE_PROFILE_ENDPOINT}/${id}/${path}` ``, sehingga tidak muncul pada pencarian teks
biasa. Setiap kali pola itu ditemukan, penelusuran dilanjutkan dengan membaca pembentuk URL-nya.
Bagian 7.2 mencatat satu kasus penting yang **hanya** terlihat setelah cara kedua dipakai, dan
kasus itu mengubah kesimpulan yang sempat diambil sebelum audit ini.

---

## 2. Ringkasan angka

Angka berikut adalah hasil hitung langsung pada snapshot, bukan perkiraan.

| Ukuran | Nilai |
| --- | --- |
| Domain HR backend | 21 di `Areas/Corporate/HumanResource/` ditambah `Areas/SelfServices/HumanResource/` |
| Controller HR backend | 150 |
| Endpoint HR backend | 1.343 |
| Endpoint master data | 618 |
| Endpoint operasional non-master-data | 725 |
| Model HR backend | 337 (`Trx` 178, `Mst` 104, `Wfp` 40, `Hrd` 15) |
| Berkas konfigurasi EF untuk HR | 337, cocok satu per satu dengan model |
| Migration yang membuat tabel HR | 1 migration, 279 tabel |
| Endpoint HR yang benar-benar dipanggil frontend | 66 pola URL literal ditambah 15 pola dinamis `workforce-profiles/{id}/…` |
| Route App Router HR | 64 kelompok master data, 1 dashboard layanan mandiri, 1 halaman absensi |
| Test backend untuk HR | **0** |
| Test frontend untuk HR | **0** |

Pembacaan singkatnya: backend HR sangat besar dan sebagian besar matang, sementara frontend
baru menyerap master data dan profil pegawai. Sekitar **577 endpoint operasional** belum punya
satu pun pemanggil di frontend.

---

## 3. Istilah status yang dipakai

Hanya tujuh nilai berikut yang dipakai, tidak boleh ada nilai lain.

| Status | Arti | Contoh di modul ini |
| --- | --- | --- |
| `READY TO REUSE` | Sudah ada, terpakai, dan cocok dipakai apa adanya | Master data cuti |
| `REUSE WITH ADAPTER` | Sudah ada dan benar, tetapi butuh lapisan penyesuaian di sisi pemakai | Serah terima payroll |
| `EXTEND` | Fondasinya benar, tetapi belum menutup kebutuhan penuh | Lifecycle pegawai |
| `REPAIR` | Sudah ada tetapi cacat, dan cacatnya harus diperbaiki sebelum dipakai | Enam menu yang menunjuk halaman kosong |
| `MISSING` | Tidak ada sama sekali | Antarmuka atasan |
| `CONFLICT` | Dua bukti saling bertentangan, atau melanggar aturan yang mengikat | Prefix `Wfp` yang tidak terdaftar |
| `UNKNOWN` | Tidak dapat dipastikan tanpa akses lingkungan atau keputusan manusia | Isi data pada tabel yang belum berpenghuni API |

---

## 4. Kontrak Bukti Kemampuan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-CAP-01` | Master data HR | HR | `BE@ecdc135 Areas/Corporate/HumanResource/MasterData/**` 65 controller, 618 endpoint; `FE@2a1cea7 src/app/hr/master-data/**` 64 kelompok route, `src/lib/constants/hr/master-data/**` | `READY TO REUSE` | Nama route tidak seragam, lihat `HRD-TF-006` | Rendah |
| `HRD-CAP-02` | Profil pegawai dan berkas kepegawaian | HR | `BE@ecdc135 Areas/Corporate/HumanResource/WorkforceCore/Controllers/**` 14 controller, 145 endpoint; `FE@2a1cea7 src/lib/state/slice/hr/workforce-profile/workforce-profile-all.jsx` 22 `WFP_RESOURCE_KEYS` | `READY TO REUSE` | Hanya dapat dicapai dari halaman detail pegawai, tidak ada halaman berdiri sendiri | Rendah |
| `HRD-CAP-03` | Menu Administrasi Kepegawaian | HR | `FE@2a1cea7 src/utils/menu-sidebar/menu-items.jsx:517-557` enam `pathname` `/hr/workforce-core/*`; tidak ada `page.jsx` yang cocok | `REPAIR` | Perlu enam route baru, atau menu disembunyikan | **Tinggi** — cacat yang sudah terlihat pengguna hari ini |
| `HRD-CAP-04` | Kehadiran dan koreksi kehadiran | HR | `BE@ecdc135 AttendanceManagement/Controllers/**` 9 controller, 71 endpoint termasuk `periods/{id}/close`, `periods/{id}/reopen`, `payroll-handoff/{id}/execute`, `payroll-handoff/{id}/rollback` | `EXTEND` | Backend matang, **nol** konsumen frontend untuk sisi administrasi | Sedang |
| `HRD-CAP-05` | Kehadiran layanan mandiri | HR | `BE@ecdc135 Areas/SelfServices/HumanResource/Controllers/AttendanceSelfServiceController.cs`; `FE@2a1cea7 src/lib/state/slice/hr/self-service/attendance-capture-slice.jsx`, `src/app/karyawan/Absensi-Karyawan/FormAbsensi/page.jsx` | `REPAIR` | Berfungsi, tetapi route melanggar `HRD-DEC-007` | Rendah |
| `HRD-CAP-06` | Konteks pengguna layanan mandiri | HR | `BE@ecdc135 Areas/SelfServices/HumanResource/Controllers/HumanResourceContextController.cs`, memakai `Shared/HumanResource/Services/HumanResourceContextService.cs`; `FE@2a1cea7 src/lib/hooks/hr/self-service/use-human-resource-context.jsx` | `READY TO REUSE` | Kepemilikan diturunkan dari pengguna terautentikasi, sesuai aturan `AGENTS.md` | Rendah |
| `HRD-CAP-07` | Cuti, izin, dan saldo | HR | `BE@ecdc135 LeaveManagement/Controllers/**` 12 controller, 93 endpoint | `EXTEND` | **Nol** konsumen frontend, baik administrasi maupun layanan mandiri | **Tinggi** — 93 endpoint tanpa pemakai |
| `HRD-CAP-08` | Lembur | HR | `BE@ecdc135 OvertimeManagement/Controllers/**` 9 controller, 78 endpoint | `EXTEND` | **Nol** konsumen frontend | **Tinggi** |
| `HRD-CAP-09` | Penjadwalan, shift, tukar shift | HR | `BE@ecdc135 SchedulingManagement/Controllers/**` 3 controller, 22 endpoint; 11 model | `EXTEND` | Controller tipis dibanding model; nol konsumen frontend | Sedang |
| `HRD-CAP-10` | Payroll | HR + Finance | `BE@ecdc135 PayrollManagement/Controllers/**` 6 controller, 49 endpoint; jalur masuk dari `AttendancePayrollHandoffController` | `REUSE WITH ADAPTER` | Batas serah terima ke Finance belum dinyatakan; nol konsumen frontend | **Tinggi** |
| `HRD-CAP-11` | Benefit | HR | `BE@ecdc135 BenefitManagement/Models/**` 9 model, 9 konfigurasi EF, **0 controller** | `MISSING` | Skema ada, perilaku tidak ada | Sedang |
| `HRD-CAP-12` | Kredensial dan kewenangan klinis | HR + Komite Medik | `BE@ecdc135 CredentialingManagement/Controllers/**` 5 controller, 46 endpoint; 18 model | `EXTEND` | Nol konsumen frontend. API pengecekan yang diminta `HRD-DEC-005` belum terbukti ada | **Tinggi** — menyentuh keselamatan pasien |
| `HRD-CAP-13` | OPPE dan FPPE | Komite Medik | Tidak ditemukan model, controller, maupun endpoint dengan konsep ini | `MISSING` | Seluruhnya belum ada | Sedang |
| `HRD-CAP-14` | Kompetensi dan pelatihan | HR | `BE@ecdc135 LearningAndDevelopment/Controllers/**` 2 controller, 18 endpoint; 13 model | `EXTEND` | Controller jauh lebih sempit daripada model | Sedang |
| `HRD-CAP-15` | Manajemen kinerja | HR | `BE@ecdc135 PerformanceManagement/Controllers/**` 2 controller, 18 endpoint; 11 model | `EXTEND` | Sama seperti di atas | Sedang |
| `HRD-CAP-16` | Kesehatan dan keselamatan kerja staf | K3RS | `BE@ecdc135 OccupationalHealthManagement/Controllers/**` 1 controller, 9 endpoint; 10 model | `EXTEND` | Aturan privasi khusus belum terbukti diterapkan | **Tinggi** — data kesehatan pribadi |
| `HRD-CAP-17` | Lifecycle dan offboarding | HR | `BE@ecdc135 LifecycleManagement/Controllers/**` 1 controller, 7 endpoint; **21 model** | `EXTEND` | Rasio paling timpang di seluruh modul | Sedang |
| `HRD-CAP-18` | Hubungan karyawan dan kedisiplinan | HR | `BE@ecdc135 EmployeeRelationManagement/Controllers/**` 1 controller, 10 endpoint; 8 model | `EXTEND` | Nol konsumen frontend untuk sisi transaksi | Rendah |
| `HRD-CAP-19` | Perencanaan tenaga kerja | HR | `BE@ecdc135 WorkforcePlanning/Models/**` 11 model, 11 konfigurasi EF, **0 controller**. Perkecualian: `MstWorkforceRequirement` dilayani `MasterData/Workforce/Controllers/WorkforceRequirementController.cs` | `MISSING` | Sepuluh dari sebelas entity tanpa perilaku; satu entity dilayani domain lain | Sedang |
| `HRD-CAP-20` | Rekrutmen dan hiring | HR | `BE@ecdc135 RecruitmentManagement/Models/**` 20 model, 20 konfigurasi EF, **0 controller** | `MISSING` | Domain terbesar yang sepenuhnya tanpa perilaku | Sedang |
| `HRD-CAP-21` | Layanan HR dan tiket kepegawaian | HR | `BE@ecdc135 HrServiceManagement/Models/**` 8 model, 8 konfigurasi EF, **0 controller** | `MISSING` | — | Rendah |
| `HRD-CAP-22` | Perjalanan dinas dan reimbursement | HR + Finance | `BE@ecdc135 BusinessTravelManagement` 13 model, `ExpenseManagement` 7 model, keduanya **0 controller** | `MISSING` | — | Rendah |
| `HRD-CAP-23` | Workflow dan persetujuan bersama | Shared platform | `BE@ecdc135 WorkflowManagement/Controllers/**` 6 controller, 48 endpoint | `EXTEND` | Mesin ada; **tidak ada** antarmuka kotak masuk persetujuan | **Tinggi** |
| `HRD-CAP-24` | Antarmuka atasan dan kotak masuk persetujuan | HR | Tidak ada `src/app/manajer`, tidak ada route persetujuan mana pun | `MISSING` | Seluruh rantai persetujuan HR tidak punya muka | **Tinggi** |
| `HRD-CAP-25` | Layanan mandiri selain kehadiran | HR | `BE@ecdc135 Areas/SelfServices/HumanResource/Controllers/**` 13 controller, 110 endpoint; frontend hanya memakai 2 di antaranya | `EXTEND` | Sebelas controller tanpa pemakai: cuti, saldo, kalender, pembatalan, kembali kerja, lembur, koreksi kehadiran, perubahan data, tukar shift, ubah jadwal, resign | **Tinggi** |
| `HRD-CAP-26` | Hak akses dan jejak audit | Shared platform | **150 dari 150** controller HR memakai `[Authorize]` dan `[AccessController]`; 146 memakai `[AccessAction]`; tidak ada `[AllowAnonymous]` di seluruh area HR | `READY TO REUSE` | Tidak ada gap. Temuan `HRD-TF-001` sudah ditarik pada revisi `1.1` | Rendah |
| `HRD-CAP-27` | Bukti pengujian | HR | Tidak ditemukan satu pun berkas test yang menyentuh HR di kedua repository | `MISSING` | Tidak ada jaring pengaman untuk 1.343 endpoint | **Tinggi** |

---

## 5. Temuan yang perlu tindakan

### `HRD-TF-001` — DITARIK pada revision `1.1`

**Status: bukan temuan. Temuan ini keliru dan dicabut.**

Revision `1.0` menyatakan `MandatoryTrainingRuleController`, `TrainingCatalogController`, dan
`TrainingCategoryController` tidak memiliki `[Authorize]`, sehingga 27 endpoint berpotensi
terbuka tanpa autentikasi. **Pernyataan itu salah.**

Ketiganya memiliki `[Authorize]`, ditulis menyatu dengan `[ApiController]` pada satu baris:

| Controller | Bentuk penulisan | Baris |
| --- | --- | ---: |
| `MandatoryTrainingRuleController` | `[ApiController,Authorize]` | 20 |
| `TrainingCatalogController` | `[ApiController,Authorize]` | 19 |
| `TrainingCategoryController` | `[ApiController, Authorize]` | 17 |

Penyebab kekeliruan ada pada metode audit, bukan pada source. Pola pencarian yang dipakai
mensyaratkan kurung siku persis di depan kata `Authorize`, sehingga bentuk penulisan menyatu
tidak tertangkap. Ini kesalahan alat ukur yang menghasilkan temuan palsu.

Pemeriksaan ulang dengan pencarian kata polos memberi hasil yang benar:

- **150 dari 150** controller HR memiliki `[Authorize]`;
- **tidak ada** `[AllowAnonymous]` di seluruh `Areas/Corporate/HumanResource/**` maupun
  `Areas/SelfServices/HumanResource/**`;
- keempat controller yang salah tempat pada `HRD-TF-004` juga memiliki `[Authorize]`.

Ketiga controller itu bahkan memakai `[AccessPermission]` per action, misalnya
`AccessPermission("TrainingCatalog","Read")` dan `AccessPermission("TrainingCatalog","Create")`,
sehingga otorisasinya lebih rinci daripada sekadar mensyaratkan login.

Catatan tentang `Program.cs` tetap benar sebagai fakta — memang tidak ada `FallbackPolicy` dan
`app.MapControllers()` memang tanpa `.RequireAuthorization()`. Namun fakta itu tidak menghasilkan
celah apa pun di HR, karena seluruh controller HR sudah memberi `[Authorize]` secara eksplisit.

Akibat penarikan ini: `HRD-CAP-26` berubah dari `REPAIR` menjadi `READY TO REUSE`, dan
`HRD-TQ-03` gugur. Tidak ada perbaikan keamanan yang perlu dijalankan.
### `HRD-TF-002` — Empat puluh entity memakai prefix yang tidak terdaftar

**Status: `CONFLICT`. Risiko: tinggi terhadap tata kelola.**

`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 9 menetapkan prefix modul Human
Resource adalah **`Hrd`**. Registry juga mencantumkan `Wfl` untuk WorkflowManagement. Registry
**tidak** memuat baris apa pun untuk `Wfp`.

Kenyataan pada snapshot:

| Prefix | Jumlah model HR | Terdaftar? |
| --- | ---: | --- |
| `Hrd` | 15 | Ya |
| `Trx` | 178 | Legacy, bukan milik HR |
| `Mst` | 104 | Terdaftar sebagai kategori Master/Reference |
| `Wfp` | 40 | **Tidak terdaftar** |
| `Wfl` | 0 | Terdaftar, tetapi tidak dipakai |

`QBE-MOD-002` menyatakan prefix tidak boleh disimpulkan dari nama folder. Empat puluh entity
`Wfp*` tersebar di dua belas domain, termasuk `WfpLeaveRequest`, `WfpPayroll`,
`WfpClinicalPrivilege`, `WfpSalaryAssignment`, dan `WfpHealthRecord`.

Sebaliknya, `WorkflowManagement` yang justru punya prefix terdaftar `Wfl` memakai `Trx*` untuk
kedelapan entity-nya.

Ini bukan sekadar soal penamaan. Registry adalah dasar penentuan kepemilikan entity, dan
selisih ini membuat kepemilikan 40 entity tidak dapat dibaca dari namanya.

### `HRD-TF-003` — Ratchet prefix `Hrd` baru berjalan di satu domain

**Status: `CONFLICT` ringan. Risiko: sedang.**

Tiga migration berturut-turut mengubah nama tabel kehadiran dari `Trx*` menjadi `Hrd*`:

| Migration | Isi |
| --- | --- |
| `20260819092733_ChangeNameTrxAttendanceToHrdAttendance` | Kehadiran inti |
| `20260821075926_NormalizeAttendanceCorrectionFamilyToHrd` | Keluarga koreksi kehadiran |
| `20260822154039_RenameAttendancePersistenceToHrd` | Sisa persistensi kehadiran |

Pekerjaan itu benar arahnya dan sejalan dengan registry, tetapi berhenti di
`AttendanceManagement`. Dua puluh domain lain masih memakai `Trx*` dan `Wfp*`. Akibatnya modul
HR sekarang memakai empat gaya penamaan sekaligus.

### `HRD-TF-004` — Source aplikasi tersimpan di dalam folder konfigurasi

**Status: `REPAIR`. Risiko: sedang.**

Folder `BE@ecdc135 Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/`
seharusnya hanya berisi konfigurasi Entity Framework, tetapi berisi source aplikasi:

- `Controllers/` — `DisciplinaryActionTypeController.cs`, `EmployeeRelationCaseTypeController.cs`,
  `SanctionTypeController.cs`, `ViolationTypeController.cs`;
- `DTOs/` — lima berkas;
- `Models/` — `MstDisciplinaryActionType.cs`, `MstEmployeeRelationCaseType.cs`,
  `MstSanctionType.cs`, `MstViolationType.cs`.

`AGENTS.md` backend menyatakan DTO, model, controller, dan service ditempatkan di dalam domain
pemiliknya. Tempat yang benar adalah
`Areas/Corporate/HumanResource/MasterData/EmployeeRelation/`.

Temuan ini punya akibat yang terlihat di frontend, lihat `HRD-TF-006`.

### `HRD-TF-005` — Enam menu menunjuk halaman yang tidak ada

**Status: `REPAIR`. Risiko: tinggi.**

| Label menu | `pathname` | Halaman |
| --- | --- | --- |
| Perubahan Data Karyawan | `/hr/workforce-core/employee-profile-changes` | Tidak ada |
| Penempatan Organisasi | `/hr/workforce-core/organization-assignments` | Tidak ada |
| Penempatan Jabatan | `/hr/workforce-core/position-assignments` | Tidak ada |
| Relasi Atasan | `/hr/workforce-core/manager-assignments` | Tidak ada |
| Riwayat Kepegawaian | `/hr/workforce-core/employment-histories` | Tidak ada |
| Penetapan Gaji | `/hr/workforce-core/salary-assignments` | Tidak ada |

Bukti: `FE@2a1cea7 src/utils/menu-sidebar/menu-items.jsx:517-557`, dibandingkan dengan seluruh
`page.jsx` di bawah `src/app/hr/`, yang hanya berisi `master-data/`.

Bagian 7.2 menjelaskan mengapa ini berstatus `REPAIR` dan bukan `MISSING`.

### `HRD-TF-006` — Penamaan route master data tidak seragam

**Status: `REPAIR` ringan. Risiko: rendah, tetapi menyulitkan pemeliharaan.**

Sebagian besar route master data memakai kebab-case jamak, misalnya `work-locations`,
`job-families`, `salary-grades`, `benefit-plans`. Lima belas route lain memakai kata gabung
tanpa pemisah:

`actiontypes`, `casetypes`, `sanctiontypes`, `violationtypes`, `workcalendars`, `workschedules`,
`shiftgroups`, `shiftpatterns`, `shifts`, `competencies`, `doctors`, `employees`, `professions`,
`specializations`, `organization`.

Empat di antaranya — `actiontypes`, `casetypes`, `sanctiontypes`, `violationtypes` — persis
milik empat controller yang salah tempat pada `HRD-TF-004`. Keempatnya dibuat di luar folder
domain, dan konvensinya ikut menyimpang.

Perlu diingat: mengubah route adalah perubahan yang merusak konsumen. Temuan ini dicatat, bukan
diusulkan untuk langsung diubah.

### `HRD-TF-007` — Tidak ada satu pun test untuk HR

**Status: `MISSING`. Risiko: tinggi.**

Backend memiliki dua project test, `QuilvianSystemBackend.Tests` dan
`QuilvianSystemBackend.BillingTests`. Tidak ada berkas test di dalamnya yang menyentuh
kehadiran, cuti, lembur, payroll, kredensial, maupun workforce.

Frontend hanya memiliki empat berkas test di seluruh repository, tidak satu pun untuk HR.

Artinya 1.343 endpoint HR dan 64 kelompok halaman master data tidak punya jaring pengaman
otomatis sama sekali.

---

## 6. Kontrak as-is backend

Bagian ini mencatat kontrak yang **sudah berlaku hari ini**, bukan kontrak target.

### 6.1 Pola umum

- Seluruh route diawali `api/v1/`.
- Area korporat memakai `api/v1/corporate/human-resource/...`.
- Area layanan mandiri memakai `api/v1/self-services/human-resource/...`.
- Envelope sukses dan gagal memakai `Responses/ApiResponse.cs`.
- Pagination memakai `Responses/PagedResult.cs`.
- Sebagian besar controller menyediakan tiga endpoint pendukung yang seragam:
  `GET filters/metadata`, `GET summary`, dan `GET options`.

### 6.2 `[Tags("Self Services / Human Resource / Context")]`

| Method | Endpoint | Ringkasan | Response |
| --- | --- | --- | --- |
| `GET` | `/api/v1/self-services/human-resource/context` | Konteks pegawai, organisasi, atasan, dan peran dari pengguna yang sedang login | `200` `ApiResponse<HumanResourceUserContextDto>`, `401` `ApiResponse<object>` |

Kepemilikan diturunkan dari pengguna terautentikasi melalui
`Shared/HumanResource/Services/HumanResourceContextService.cs`, bukan dari identifier yang
dikirim pemanggil. Ini sesuai aturan `AGENTS.md` tentang kepemilikan layanan mandiri.

### 6.3 `[Tags]` Kehadiran — periode

| Method | Endpoint | Ringkasan |
| --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/attendance/periods` | Daftar periode |
| `GET` | `/api/v1/corporate/human-resource/attendance/periods/{id}` | Detail periode |
| `PUT` | `/api/v1/corporate/human-resource/attendance/periods/{id}` | Ubah periode |
| `GET` | `/api/v1/corporate/human-resource/attendance/periods/{id}/close-preview` | Pratinjau sebelum tutup |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/enqueue-processing` | Antrikan pemrosesan |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/close` | Tutup periode |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/reopen` | Buka kembali periode |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/cancel` | Batalkan periode |
| `DELETE` | `/api/v1/corporate/human-resource/attendance/periods/{id}` | Hapus periode |

### 6.4 `[Tags]` Kehadiran — serah terima payroll

| Method | Endpoint | Ringkasan |
| --- | --- | --- |
| `GET` | `.../attendance/payroll-handoff/payroll-runs/options` | Pilihan payroll run |
| `GET` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/summary` | Ringkasan |
| `GET` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/preview` | Pratinjau |
| `GET` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/reconciliation` | Rekonsiliasi |
| `POST` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/execute` | Jalankan serah terima |
| `POST` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/repair` | Perbaiki serah terima |
| `POST` | `.../attendance/payroll-handoff/payroll-runs/{payrollRunId}/rollback` | Batalkan serah terima |

Adanya `execute`, `repair`, dan `rollback` menunjukkan backend sudah memikirkan kegagalan
sebagian dan pengulangan yang aman. Ini bukti kuat bahwa kemampuan ini layak dipakai ulang.

### 6.5 `[Tags]` Profil workforce

Empat belas controller `WorkforceCore` mengikuti satu pola yang seragam:

`api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/<sumber-daya>`

dengan `<sumber-daya>` berupa `addresses`, `bank-accounts`, `contract-histories`, `dependents`,
`documents`, `educations`, `emergency-contacts`, `employment-histories`, `family-members`,
`manager-assignments`, `organization-assignments`, `position-assignments`, dan
`salary-assignments`.

Ditambah dua endpoint yang tidak mengikuti pola per-profil:

| Method | Endpoint | Pemilik |
| --- | --- | --- |
| `*` | `/api/v1/corporate/human-resource/employee-profile-changes` | `EmployeeProfileChangeController` |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/overview` | `WorkforceDetailController` |

### 6.6 Persistensi

- Seluruh 337 model HR memiliki berkas konfigurasi EF pasangannya di
  `Repositories/Configurations/Corporate/HumanResource/**`.
- Seluruhnya terdaftar sebagai `DbSet` pada `Repositories/ApplicationDbContext.cs`, yang memuat
  516 `DbSet` untuk seluruh aplikasi.
- Tabel HR dibuat oleh satu migration besar,
  `20260726161839_initializeBigModulHRD2.cs`, yang membuat **279 tabel**.
- `Seeders/DefaultWorkScheduleSeeder.cs` berjalan saat aplikasi start
  (`Program.cs:865`) dan memastikan ada jadwal kerja bawaan berkode `SCH-RSMMC-DEFAULT`.
  Perilaku ini dapat dimatikan lewat konfigurasi `SeedDefaultData:Enabled`.

---

## 7. Konsumen frontend

### 7.1 Cara frontend memanggil backend

Modul HR **tidak** memakai folder `src/lib/services/`. Panggilan API dibuat langsung di dalam
Redux thunk memakai `InstanceAxios`, dengan alamat disimpan sebagai konstanta di
`src/lib/constants/hr/**`.

Contoh:

> `FE@2a1cea7 src/lib/constants/hr/master-data/benefit-plan/benefit-plan-constants.jsx:259`
> `endpoint: "/v1/corporate/human-resource/master-data/benefit-plans"`

Awalan `/api` ditambahkan oleh `InstanceAxios`, sehingga alamat penuhnya menjadi
`/api/v1/corporate/human-resource/master-data/benefit-plans`.

### 7.2 Koreksi penting terhadap pembacaan sebelumnya

Pada pass `/grill-me` sebelumnya, enam menu `Administrasi Kepegawaian` yang menunjuk halaman
kosong sempat dibaca sebagai kemampuan yang hilang. **Audit ini menunjukkan pembacaan itu tidak
tepat, dan koreksinya mengubah prioritas pekerjaan.**

Kemampuannya sebenarnya **sudah ada dan sudah dipakai**. Yang hilang hanya halaman berdiri
sendirinya.

Buktinya ada di `FE@2a1cea7 src/lib/state/slice/hr/workforce-profile/workforce-profile-all.jsx`
baris 13 sampai 36, yang mendefinisikan `WFP_RESOURCE_KEYS` berisi 22 sumber daya, termasuk
`organizationAssignments`, `positionAssignments`, `managerAssignments`, `salaryAssignments`,
`employmentHistories`, dan `profileChangeRequests` — persis keenam menu yang tampak mati.

Sumber daya itu dipanggil lewat URL yang dibentuk dinamis pada baris 209 sampai 212, sehingga
tidak muncul pada pencarian teks biasa:

```javascript
const profileResourceUrl = (workforceProfileId, path) => {
  const normalizedPath = sanitizeText(path, 140).replace(/^\/+/, "");
  return `${profileBaseUrl(workforceProfileId)}/${normalizedPath}`;
};
```

Jalur yang benar-benar dipakai pengguna hari ini adalah:

`/hr/master-data/employee/{employeeSlug}/workforce/{resourceKey}` → memanggil
`/api/v1/corporate/human-resource/workforce-profiles/{id}/{resource}`

Pola yang sama tersedia untuk dokter dan pengguna eksternal.

**Akibatnya bagi rencana kerja:** enam menu itu bukan pekerjaan membangun kemampuan baru dari
nol. Kontraknya sudah ada, Redux-nya sudah ada, dan komponen editornya sudah ada. Yang
dibutuhkan adalah halaman daftar lintas-pegawai yang memakai ulang semua itu. Ini pekerjaan
yang jauh lebih kecil daripada dugaan awal.

### 7.3 Ringkasan pemakaian per domain

| Domain backend | Endpoint | Dipakai frontend? | Bukti |
| --- | ---: | --- | --- |
| `MasterData` | 618 | **Ya**, 64 kelompok | `src/lib/constants/hr/master-data/**` |
| `WorkforceCore` | 145 | **Ya**, lewat editor profil | `workforce-profile-all.jsx` |
| `WorkforceProfileManagement` | 1 | **Ya** | `workforce-profile-all.jsx` |
| `SelfServices/HumanResource` | 110 | **Sebagian**, 2 dari 13 controller | `attendance-capture-slice.jsx`, `use-human-resource-context.jsx` |
| `AttendanceManagement` | 71 | **Tidak** | — |
| `LeaveManagement` | 93 | **Tidak** | — |
| `OvertimeManagement` | 78 | **Tidak** | — |
| `PayrollManagement` | 49 | **Tidak** | — |
| `WorkflowManagement` | 48 | **Tidak** | — |
| `CredentialingManagement` | 46 | **Tidak** | — |
| `SchedulingManagement` | 22 | **Tidak** | — |
| `LearningAndDevelopment` | 18 | **Tidak** | — |
| `PerformanceManagement` | 18 | **Tidak** | — |
| `EmployeeRelationManagement` | 10 | **Tidak** | — |
| `OccupationalHealthManagement` | 9 | **Tidak** | — |
| `LifecycleManagement` | 7 | **Tidak** | — |

Pencarian pembanding: setiap kata kunci `leave/`, `onboarding`, `offboarding`, `resignation`,
`health-record`, `performance-review`, `training-record`, `disciplinary`, dan
`workflow-instances` menghasilkan **nol** berkas frontend. Kata kunci `overtime`, `payroll`,
`credential`, `clinical-privilege`, `approval`, dan `schedul` memang menghasilkan berkas,
tetapi seluruhnya adalah konstanta **master data**, bukan transaksi.

---

## 8. Jawaban terhadap `HRD-DEC-004`

`HRD-DEC-004` menetapkan otoritas skema bersifat hybrid. Audit ini memberi daftar yang tegas.

### 8.1 Skema yang dikunci — domain berjalan

Empat belas domain berikut punya controller aktif, sehingga skemanya diperlakukan sebagai
kontrak existing: `MasterData`, `WorkforceCore`, `WorkforceProfileManagement`,
`AttendanceManagement`, `LeaveManagement`, `OvertimeManagement`, `PayrollManagement`,
`CredentialingManagement`, `SchedulingManagement`, `LearningAndDevelopment`,
`PerformanceManagement`, `OccupationalHealthManagement`, `LifecycleManagement`,
`EmployeeRelationManagement`, `WorkflowManagement`, dan `SelfServices/HumanResource`.

Perubahan pada kelompok ini hanya lewat `EXTEND` atau `REPAIR` yang berbukti.

### 8.2 Skema yang boleh diturunkan ulang — domain belum berperilaku

| Domain | Model | Konfigurasi EF | Controller |
| --- | ---: | ---: | ---: |
| `RecruitmentManagement` | 20 | 20 | 0 |
| `BusinessTravelManagement` | 13 | 13 | 0 |
| `WorkforcePlanning` | 11 | 11 | 0 |
| `BenefitManagement` | 9 | 9 | 0 |
| `HrServiceManagement` | 8 | 8 | 0 |
| `ExpenseManagement` | 7 | 7 | 0 |
| **Total** | **68** | **68** | **0** |

**Satu pengecualian penting.** `MstWorkforceRequirement` milik `WorkforcePlanning` sudah
dilayani oleh `MasterData/Workforce/Controllers/WorkforceRequirementController.cs`, dan sudah
dipakai frontend lewat route `/hr/master-data/workforce-requirement`. Entity itu **tidak** boleh
ikut diturunkan ulang secara bebas. Yang bebas adalah sepuluh entity `WorkforcePlanning`
lainnya.

Jadi angka yang benar untuk perancangan ulang bebas adalah **67 entity**, bukan 68.

### 8.3 Yang mengoreksi asumsi `HRD-ASM-03`

`HRD-ASM-03` mengasumsikan enam domain itu bebas dirancang ulang karena belum pernah dipakai.
Audit memberi dua bukti yang memperjelas:

**Yang menguatkan.** Tidak ada controller, tidak ada service, dan tidak ada seeder yang menulis
ke keenam domain itu. Jalur aplikasi memang tidak dapat mengisinya.

**Yang memperberat.** Tabelnya **sudah ada di database mana pun yang sudah dimigrasi**. Seluruh
68 model punya konfigurasi EF dan terdaftar sebagai `DbSet`, dan tabelnya termasuk dalam 279
tabel yang dibuat migration `20260726161839_initializeBigModulHRD2`.

Konsekuensinya: perancangan ulang tidak berarti membuat tabel baru di ruang kosong. Perancangan
ulang akan menghasilkan migration yang **mengubah atau membuang tabel yang sudah ada**. Untuk
lingkungan pengembangan itu murah; untuk lingkungan bersama atau produksi itu perlu wewenang
terpisah.

`HRD-Q-05` karena itu **belum tertutup**. Audit source dapat membuktikan aplikasi tidak menulis
ke sana, tetapi tidak dapat membuktikan tidak ada yang mengisinya lewat impor manual, skrip, atau
migrasi data dari V1. Itu hanya dapat dijawab dengan memeriksa database, dan pemeriksaan itu di
luar batas audit ini.

---

## 9. Ketidakcocokan frontend dan backend

| ID | Ketidakcocokan | Sisi yang benar | Catatan |
| --- | --- | --- | --- |
| `HRD-MM-01` | Enam menu menunjuk route yang tidak ada | Backend | Kontrak sudah tersedia, halaman belum dibuat. Lihat `HRD-TF-005` dan bagian 7.2 |
| `HRD-MM-02` | Sebelas controller layanan mandiri tanpa pemakai | Backend | Backend siap; frontend belum |
| `HRD-MM-03` | Route absensi di luar konvensi | Konvensi frontend | Sudah diputuskan `HRD-DEC-007` |
| `HRD-MM-04` | Penamaan route master data tidak seragam | Backend, sebagai kontrak yang sudah berjalan | Frontend hanya mengikuti. Lihat `HRD-TF-006` |
| `HRD-MM-05` | Frontend menormalkan huruf besar-kecil field | Tidak ada yang salah | Slice menerima `camelCase` maupun `PascalCase`, contohnya `data.userId ?? data.UserId`. Ini pertanda bentuk response backend pernah berubah, atau belum dipastikan |

`HRD-MM-05` layak diperhatikan saat mengunci kontrak. Contoh nyatanya ada di
`FE@2a1cea7 src/lib/state/slice/hr/self-service/attendance-capture-slice.jsx:15-45`, yang
menuliskan pasangan `??` untuk hampir setiap field. Kalau kontrak sudah dikunci, lapisan
penyeragaman itu tidak lagi diperlukan.

---

## 10. Kemampuan modul tetangga yang disentuh

| Kemampuan | Pemilik | Status untuk HR | Catatan |
| --- | --- | --- | --- |
| `ApplicationDbContext` | Shared platform | `READY TO REUSE` | 516 `DbSet`; HR memakai pola akses langsung seperti domain lain |
| `ApiResponse<T>` dan `PagedResult<T>` | Shared platform | `READY TO REUSE` | Dipakai konsisten oleh seluruh controller HR |
| `[AccessController]`, `[AccessAction]`, `AccessTypes` | Shared platform | `READY TO REUSE` | Dipakai 150 dari 150 controller HR |
| `InstanceAxios` dan `getHeaders` | Frontend shared | `READY TO REUSE` | Dipakai seluruh slice HR |
| Jadwal praktik dokter | Health Services | Di luar scope | Sudah dipisahkan oleh `HRD-DEC-006` |
| Penyelesaian pembayaran payroll | Finance | `UNKNOWN` | Batas serah terima belum dinyatakan siapa pun |
| Penyediaan dan pencabutan akun | Administrator/Identity | `UNKNOWN` | Belum ada bukti integrasi dari sisi HR |

---

## 11. Pemicu impact scan

Peta ini harus ditandai basi lalu diperiksa ulang secara terbatas bila salah satu terjadi:

1. `ecdc135` bukan lagi HEAD backend, dan perubahannya menyentuh `Areas/Corporate/HumanResource/**`,
   `Areas/SelfServices/HumanResource/**`, `Shared/HumanResource/**`,
   `Repositories/ApplicationDbContext.cs`, `Repositories/Configurations/Corporate/HumanResource/**`,
   atau `Migrations/**`;
2. `2a1cea7` bukan lagi HEAD frontend, dan perubahannya menyentuh `src/app/hr/**`,
   `src/app/self-services/**`, `src/app/karyawan/**`, `src/lib/state/slice/hr/**`,
   `src/lib/hooks/hr/**`, `src/lib/constants/hr/**`, atau `src/utils/menu-sidebar/menu-items.jsx`;
3. `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` berubah, khususnya bila `Wfp` ditambahkan atau ditolak;
4. `HRD-Q-05` dijawab dengan pemeriksaan database yang sebenarnya;
5. `HRD-TF-002` diselesaikan lewat pembaruan registry, karena itu mengubah status prefix legacy HR.

---

## 12. Pertanyaan penutup untuk `/grill-me`

Pertanyaan berikut tidak dapat dijawab source code. Semuanya dibawa ke pass wawancara
berikutnya.

| ID | Pertanyaan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| `HRD-TQ-01` | Prefix `Wfp` yang dipakai 40 entity: didaftarkan resmi ke registry, atau entity-nya diganti nama menjadi `Hrd*`? Penggantian nama berarti migration besar | `DESIGN` | Pemilik modul + pemilik registry |
| `HRD-TQ-02` | Ratchet `Hrd` diteruskan ke 20 domain sisanya, atau berhenti dan `Trx*` diterima sebagai legacy yang dibekukan? | `DESIGN` | Pemilik modul |
| `HRD-TQ-03` | Tiga controller pelatihan tanpa `[Authorize]`: diperbaiki sebagai perbaikan keamanan mendesak, atau memang sengaja terbuka? | `IMPLEMENTATION` | Pemilik keamanan |
| `HRD-TQ-04` | Enam menu `Administrasi Kepegawaian`: dibuatkan halaman daftar lintas-pegawai, atau menunya dihapus karena datanya sudah dapat diakses dari halaman detail pegawai? | `IMPLEMENTATION` | Pemilik produk |
| `HRD-TQ-05` | Apakah tabel milik 67 entity tanpa API sudah berisi data dari impor manual atau migrasi V1? | `IMPLEMENTATION` | Pemilik database |
| `HRD-TQ-06` | Di mana batas serah terima payroll ke Finance? Sampai `execute` saja, atau termasuk pembayaran? | `DESIGN` | Pemilik produk + Finance |
| `HRD-TQ-07` | Aturan privasi apa yang berlaku untuk `WfpHealthRecord`? Siapa yang boleh membaca? | `DESIGN` | K3RS |
| `HRD-TQ-08` | Kotak masuk persetujuan: satu tempat untuk semua transaksi HR, atau per jenis transaksi? | `DESIGN` | Pemilik produk |
| `HRD-TQ-09` | Empat controller yang salah tempat pada `HRD-TF-004`: dipindahkan sekarang atau dibiarkan? Pemindahan mengubah namespace | `IMPLEMENTATION` | Pemilik modul |
| `HRD-TQ-10` | Route master data yang tidak seragam: dibiarkan sebagai kontrak berjalan, atau diseragamkan dengan risiko merusak konsumen? | `DESIGN` | Pemilik modul |

---

## 13. Yang sengaja tidak dilakukan

- Tidak ada satu baris source aplikasi yang diubah, di kedua repository.
- Tidak ada build, tidak ada test dijalankan, tidak ada migration dibuat, tidak ada database
  disentuh.
- Tidak ada arsitektur target, ERD target, kontrak API target, maupun roadmap yang dibuat.
- Cacat yang ditemukan hanya dicatat. `HRD-TF-002` sampai `HRD-TF-007` tidak diperbaiki. `HRD-TF-001` ditarik pada revisi `1.1` karena terbukti bukan temuan.
- Isi data tidak diperiksa, sehingga `HRD-Q-05` tetap terbuka.

---

## 14. Impact Scan Terbatas — Workforce Core & Employee 360, 2 November 2026 (revisi `1.2`)

| Field | Nilai |
| --- | --- |
| Mode | **Impact scan terbatas**. Hanya klaster yang disentuh slice `S-A1` dan keputusan `HRD-DEC-046` s.d. `HRD-DEC-057` |
| Pemicu | Bagian 11 butir 1 dan 2: HEAD kedua repository sudah berpindah dari titik verifikasi terakhir. Ditambah amendment pass decision log bagian 31 |
| Backend | Titik verifikasi terakhir `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` → HEAD `396c836380bba7cde722351c14c08e8b72535279` (branch `MHamzah`). Working tree jalur yang di-scan bersih |
| Frontend | Titik verifikasi terakhir `fff76a1b394d4b247c70a04f106c8ec098c9696e` → HEAD `9c4000724da817200b92ae36db29f6914023c90d` (branch `HamzahV2`). Working tree jalur yang di-scan bersih |
| Masukan bisnis | [`00-interview-decisions.md`](./00-interview-decisions.md) revisi `16`, SHA-256 `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` (sesudah koreksi `HRD-TF-014` pada scan ini) |
| Decision ID yang dirujuk | `HRD-DEC-011`, `HRD-DEC-012`, `HRD-DEC-031`, `HRD-DEC-033`, `HRD-DEC-034`, `HRD-DEC-036`, `HRD-DEC-046` s.d. `HRD-DEC-057` |
| Contract version | `v5` menurut `blueprint-manifest.md`. Kontrak itu **belum** memuat keputusan `HRD-DEC-046` s.d. `HRD-DEC-057`; yang berlaku untuk keputusan baru hanya kontrak as-is pada bagian 14.6 |
| Cara scan | `git diff` terbatas pada jalur di bawah, pembacaan statis controller/service/model, uji status HTTP ke backend lokal tanpa login (`curl`), dan hitung baris database read-only |
| Batas tulis | Dokumen ini dan satu koreksi baris `HRD-TF-014` pada decision log. Tidak ada source aplikasi, build, migration, atau penulisan database |

**Singkatan bukti bagian ini.** `BE@396c836` = `NewQuilvianSystemBackend` commit `396c8363`.
`FE@9c40007` = `QuilvianSystemFrontendDev` commit `9c4000724`.

### 14.1 Batas scan

| Di-scan | Jalur |
| --- | --- |
| Backend | `Areas/Corporate/HumanResource/WorkforceCore/**`, `WorkforceProfileManagement/**`, `MasterData/Workforce/**`, `Areas/SelfServices/HumanResource/**` (perubahan data), titik sentuh `WorkflowManagement/**` dan `AttendanceManagement/**` (periode), `Services/Security/OrganizationAuthorizationProjectionService.cs`, `Shared/HumanResource/Services/HumanResourceContextService.cs`, konfigurasi EF jalur yang sama, `Tests/**` |
| Frontend | `src/app/hr/workforce-core/**`, `src/components/view/hr/workforce-core/**`, `src/lib/services/hr/**`, `src/components/features/workforce/**`, `src/components/view/hr/master-data/employees/**`, `src/utils/menu-sidebar/menu-items.jsx` |

**Tidak di-scan ulang:** domain HR lain pada bagian 4 (kehadiran, cuti, lembur, payroll,
kredensial, dan seterusnya). Baris-baris itu kini berstatus **`STALE`**: buktinya dari `ecdc135` /
`2a1cea7` dan belum diperiksa terhadap HEAD sekarang. Jangan memakainya sebagai bukti desain
sebelum di-scan ulang.

### 14.2 Perubahan sejak titik verifikasi terakhir

**Backend — 8 berkas pada jalur yang di-scan berubah**, dari 4 commit. Yang material:

| Commit | Perubahan | Akibat bagi Workforce Core |
| --- | --- | --- |
| `e1d11214` fix(security): establish authorization integrity foundation | `OrganizationAuthorizationProjectionService` baru. Setiap perubahan penempatan organisasi kini diselaraskan ke `AspNetUserOrganization`, yaitu cakupan unit yang boleh diakses pengguna. Fungsi `SyncUserPrimaryOrganizationAsync` di tiga controller master dihapus dan diganti panggilan ke service ini | **Penempatan organisasi kini menentukan hak akses.** Penempatan yang salah tanggal (`HRD-TF-013`) bukan lagi sekadar riwayat yang keliru; ia ikut menentukan unit mana yang dapat dibuka pengguna |
| `6275ac34` feat(operating-room) | `EmployeeOptionResponse` menyertakan `WorkforceProfileId` | Modul lain mulai merujuk profil workforce, bukan Id pegawai — searah dengan `HRD-DEC-051` |
| (task `BE-HRD-003`) | `WorkforceCoreCrossEmployeeController`, DTO, dan query service baru — 5 daftar lintas-pegawai dan 1 endpoint nominal gaji | Backend daftar lintas-pegawai sudah ada |

**Frontend — 34 berkas pada jalur yang di-scan berubah.** Yang material: enam route
`/hr/workforce-core/*` beserta view-nya, komponen daftar bersama, dan **tiga belas service HR baru**
di `src/lib/services/hr/` (kotak masuk persetujuan, kehadiran, cuti, lembur, payroll, jadwal,
pelatihan, kinerja, hubungan karyawan, benefit, exit clearance, workforce core, layanan mandiri).

### 14.3 Kontrak Bukti Kemampuan — Workforce Core

Lanjutan penomoran bagian 4. Setiap baris memakai tepat satu status dari bagian 3.

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-CAP-28` | Enam layar lintas-pegawai menampilkan data (`HRD-DEC-012`, `HRD-DEC-055`) | HR | `FE@9c40007 src/lib/services/hr/workforce-core-cross-employee-service.js:6` memanggil `/v1/hr/workforce-core/cross-employee`; `BE@396c836 .../WorkforceCoreCrossEmployeeController.cs:13` melayani `api/v1/corporate/human-resource/workforce-core`. Uji lokal: alamat frontend **404**, alamat backend **401**. `FE@9c40007 .../workforce-core-cross-employee-list-view.jsx:112-116` mengirim `description` ke `InformationAlert` yang membaca `message` (`information-alert.jsx:47-66`) | `REPAIR` | Ganti base URL; arahkan layar Perubahan Data ke daftar `employee-profile-changes` yang sudah ada; ganti prop `description` menjadi `message` | **Tinggi** — keenam layar tampak "kosong" tanpa pesan |
| `HRD-CAP-29` | Backend daftar lintas-pegawai dengan label status baris (`HRD-DEC-055` `ROW_STATUS_LABEL`) | HR | `BE@396c836 .../WorkforceCoreCrossEmployeeController.cs:34-216`: 5 `GET` daftar + `GET salary-assignments/{id}/amount`; nominal gaji tidak dikirim (`HRD-DEC-033`) | `EXTEND` | Daftar sudah ada dan benar. Label **Menunggu persetujuan / Dijadwalkan** belum dapat dihasilkan karena penempatan organisasi, jabatan, dan atasan tidak punya kolom persetujuan | Sedang |
| `HRD-CAP-30` | Tepat satu penempatan utama per jenis per tanggal (`HRD-DEC-049`) | HR | `BE@396c836 WfpPositionAssignmentController.cs:396-400` dan `WfpManagerAssignmentController.cs:373-378` menolak kembar persis pada periode beririsan; `ClearPrimaryAsync` membalik `IsPrimary` tanpa melihat tanggal; `WfpOrganizationAssignmentController.cs:412-413` hanya memeriksa nomor penugasan ganda | `EXTEND` | Tambah aturan satu-utama-per-tanggal, larangan lubang penempatan utama, dan penutupan otomatis per sehari sebelum. Jenis penugasan `Primary/Secondary/Acting/Temporary/Project/Functional` sudah ada (`WfpOrganizationAssignmentController.cs:36`) | Sedang |
| `HRD-CAP-31` | Perubahan penempatan hanya lewat persetujuan (`HRD-DEC-031`, `HRD-DEC-047`) | HR | `BE@396c836` controller organisasi, jabatan, dan atasan menyediakan `PUT /{id}`, `PATCH /{id}/status`, `PATCH /{id}/primary`, `DELETE /{id}` yang langsung berlaku dengan hak `Update`/`Delete`, tanpa persetujuan | `CONFLICT` | Jalur ubah langsung bertentangan dengan keputusan mengikat. Desain harus menentukan nasib endpoint ini: dikunci, dijadikan koreksi berpersetujuan, atau dibatasi pada data yang belum berlaku | **Tinggi** |
| `HRD-CAP-32` | Penempatan sebagai satu-satunya sumber unit/jabatan; kolom master hanya cerminan (`HRD-DEC-047`) | HR | `BE@396c836 EmployeeController.cs:1355-1356, 1416, 2157-2223` (`EnsureWorkforcePrimaryOrganizationAssignmentAsync`: tanggal mulai = `JoinDate`, penempatan lama tidak diberi tanggal akhir); pola sama `DoctorController.cs:1456`, `ExternalUserController.cs:1288`. `FE@9c40007 .../employees/form/employee-form-view.jsx` tidak ditemukan penguncian isian unit/jabatan pada mode ubah | `CONFLICT` | Form master hanya mengisi unit/jabatan saat pembuatan; cerminan `PrimaryDepartmentId`/`PrimaryPositionId` diisi sistem | **Tinggi** — sejak `e1d11214` ikut menentukan hak akses |
| `HRD-CAP-33` | Penempatan masa depan mulai berlaku tepat pada tanggalnya (`HRD-DEC-047` `MIRROR_FOLLOWS_EFFECTIVE_DATE`, `HRD-DEC-048` `APPLY_ON_EFFECTIVE_DATE`) | Shared platform + HR | `BE@396c836 Services/Security/OrganizationAuthorizationProjectionService.cs:67-69` hanya menganggap penempatan sah bila `EffectiveStartDate <= now`; tetapi rekonsiliasi hanya dipanggil saat ada penulisan, dan `ReconcileAllAsync` (baris 78) **tidak dipanggil di mana pun**. Pola layanan terjadwal sudah ada: `AttendanceSchedulerHostedService`, `LeaveAccrualSchedulerHostedService` | `REUSE WITH ADAPTER` | Perlu proses terjadwal harian yang menerapkan dokumen peristiwa, memperbarui cerminan, dan menjalankan rekonsiliasi proyeksi | Sedang |
| `HRD-CAP-34` | Hanya atasan utama yang menerima rute persetujuan (`HRD-DEC-049` `ONLY_PRIMARY_MANAGER_ROUTES_APPROVAL`) | HR | `BE@396c836 WorkflowManagement/Services/WorkflowService.cs:1938-1951` memilih atasan aktif dengan `CanApproveRequests`, lalu `OrderByDescending(IsPrimaryManager)` — **jatuh ke atasan tambahan** bila atasan utama tidak ada | `CONFLICT` | Pemilihan atasan wajib dibatasi pada atasan utama; perilaku saat atasan utama kosong mengikuti eskalasi sub-modul 21 | Sedang |
| `HRD-CAP-35` | Dokumen peristiwa SK (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`) | HR | `BE@396c836 WorkforceCore/Models/TrxEmployeeTransfer.cs`, `TrxEmployeePromotion.cs`, `TrxEmployeeRotation.cs`, `TrxEmployeeDemotion.cs`, `TrxTemporaryAssignment.cs` beserta konfigurasi EF; **0** controller/service; **0** baris data | `MISSING` | Skema ada, perilaku tidak ada. Selisih skema terhadap keputusan: tidak ada kolom atasan asal/tujuan; tidak ada nomor SK maupun lampiran scan SK; `TrxEmployeePromotion` dan `TrxEmployeeDemotion` menyimpan `NewBaseSalary`, padahal `HRD-DEC-048` `SALARY_STAYS_SEPARATE` memisahkan gaji; kosakata status hanya bawaan `"Draft"`; satu peristiwa gabungan (mutasi + promosi) tersebar di dua tabel | Sedang |
| `HRD-CAP-36` | Mesin persetujuan untuk dokumen peristiwa | Shared platform | `BE@396c836 WorkflowManagement/Services/WorkflowReferenceLifecycleService.cs:50-83` mendaftar jenis rujukan (perubahan data, cuti, lembur, resign, dan lain-lain); pola integrasi `WorkforceCore/Services/EmployeeProfileChangeWorkflowIntegrationService.cs`; kelima model `Trx*` sudah punya `WorkflowDefinitionId` | `REUSE WITH ADAPTER` | Daftarkan jenis rujukan baru dan service integrasi per pola perubahan data | Rendah |
| `HRD-CAP-37` | Lampiran scan SK wajib (`HRD-DEC-056`) | HR | `BE@396c836 WorkflowManagement/Services/WorkflowFileStorageService.cs`; unggah dokumen `WorkforceCore/Controllers/WfpDocumentController.cs` (`IFormFile`) | `REUSE WITH ADAPTER` | Pola penyimpanan berkas sudah ada; perlu ditautkan ke dokumen peristiwa | Rendah |
| `HRD-CAP-38` | Batas berlaku surut ke periode kehadiran terbuka (`HRD-DEC-050`) | HR | `BE@396c836 AttendanceManagement/Services/AttendanceCorrectionOnBehalfService.cs:94-98` memeriksa `HrdAttendancePeriod.PeriodStatus == Closed` untuk satu tanggal | `REUSE WITH ADAPTER` | Logikanya ada tetapi ditulis di dalam satu service; perlu diangkat menjadi pemeriksaan bersama agar dipakai saat pengajuan, persetujuan, dan penerapan | Rendah |
| `HRD-CAP-39` | Satu nomor identitas = satu profil dengan peran jamak (`HRD-DEC-051`) | HR | Keunikan NIK per tabel saja: `MstEmployeeConfiguration.cs:144`, `MstDoctorConfiguration.cs:104`. Pembuatan selalu membuat profil baru: `EmployeeController.cs:1066`, `DoctorController.cs:1062`, `ExternalUserController.cs:921`. `MstWorkforceProfile.UserType` tunggal (`Enums/UserType.cs`: `Employee`, `PermanentDoctor`, `GuestDoctor`, `ExternalUser`, …). Fondasi peran jamak sudah ada: navigasi `Employee`, `Doctor`, `ExternalUser` pada `MstWorkforceProfile.cs:55-59`, dan overview sudah memisahkan `IsEmployee`/`IsDoctor`/`IsExternalUser` (`WorkforceDetailDtos.cs:41-43`) | `EXTEND` | Pemeriksaan keunikan lintas tabel; penautan ke profil yang ada; `UserType` tunggal dirancang ulang. Data: 0 NIK kembar lintas tabel; **14 profil yatim** — sebagian dibuat `OperatingRoomDemoSeeder.cs:167, 208, 256` | Sedang |
| `HRD-CAP-40` | Data pribadi sensitif hanya lewat permohonan berbukti dengan verifikator berbeda (`HRD-DEC-052`) | HR | Ubah langsung tersedia: `WfpBankAccountController.cs:412` (`PUT`), `:641` (`PATCH verify` dengan hak `Update` yang sama, tanpa pemisahan peran); `WfpFamilyMemberController.cs:320`; `WfpDependentController.cs:332`. Katalog perubahan data hanya kolom `MstEmployee` dan `MstWorkforceProfile`, termasuk `IdentityNumber` dan `MaritalStatus` (`EmployeeProfileChangeService.cs` `BuildFieldKey`), **tanpa** rekening, keluarga, tanggungan, NPWP. `DecideVerificationAsync` tidak memeriksa verifikator ≠ pembuat. Permohonan hanya dibuat dari layanan mandiri (`Areas/SelfServices/HumanResource/Controllers/EmployeeProfileChangeSelfServiceController.cs:231`). Pola "atas nama" sudah ada di kehadiran: `AttendanceCorrectionOnBehalfService.cs` | `CONFLICT` | Kunci ubah langsung data sensitif; perluas katalog ke rekening, keluarga, tanggungan, NPWP; tambah pengajuan atas nama pegawai; tambah verifikator ≠ pembuat | **Tinggi** — rekening gaji dapat dialihkan satu orang |
| `HRD-CAP-41` | Persetujuan penetapan gaji dengan penyetuju ≠ pembuat (`HRD-DEC-031`) | HR | `BE@396c836 WfpSalaryAssignmentController.cs:275-290`: `PATCH /{id}/approval` memakai hak `WfpSalaryAssignment : Update` yang sama dengan ubah, dan menyimpan `actor` tanpa membandingkan dengan `CreateBy` | `REPAIR` | Swa-setuju masih mungkin. Sudah tercatat pada decision log bagian 26.1 dan belum diperbaiki | **Tinggi** |
| `HRD-CAP-42` | Employee 360 untuk HR | HR | `BE@396c836 WorkforceProfileManagement/Controllers/WorkforceDetailController.cs:43-50` `GET .../overview`, hak `WorkforceProfileOverview : Read`; isinya jumlah per bagian, bukan nilai. `RestrictedSections` adalah daftar tetap (baris 462-480), **tidak** bergantung pada hak pengguna | `REUSE WITH ADAPTER` | Untuk HR dapat dipakai. Penanda bagian rahasia perlu diturunkan dari hak akses pengguna, bukan daftar tetap | Rendah |
| `HRD-CAP-43` | Employee 360 terbatas untuk atasan (`HRD-DEC-053`) | HR | Tidak ditemukan pembatasan "hanya bawahan langsung" pada overview maupun controller per profil. Bahan yang sudah ada: `Shared/HumanResource/Services/HumanResourceContextService.cs:472` `ResolveDirectReportsAsync` | `MISSING` | Perlu pemeriksaan bawahan langsung berbasis atasan utama yang berlaku hari ini, dan proyeksi bagian yang boleh dilihat | Sedang |
| `HRD-CAP-44` | Batch data awal penempatan (`HRD-DEC-054`) | HR | Tidak ada impor batch di area HR. Pola impor Excel sudah ada di Finance: `FinanceReceivableMigrationService.cs:300`, `FinancePayrollDeductionBatchService.cs:1104` (`new XLWorkbook(stream)`, paket `ClosedXML` 0.105.1) | `MISSING` | Fitur belum ada; pola teknis bisa ditiru | Sedang |
| `HRD-CAP-45` | Riwayat kepegawaian yang merujuk dokumen peristiwa (`HRD-DEC-048`) | HR | `BE@396c836 WorkforceCore/Models/WfpEmploymentHistory.cs` sudah punya `ReferenceType`, `ReferenceId`, nilai lama/baru unit, jabatan, grade, `ApprovedByUserId`, `FilePath`; 1 baris data | `READY TO REUSE` | Tidak ada kolom atasan lama/baru; cukup karena atasan dicatat di penempatannya sendiri | Rendah |
| `HRD-CAP-46` | Kotak masuk persetujuan terpadu dapat dicapai frontend (titik sentuh `HRD-DEC-055` `APPROVAL_STAYS_IN_INBOX`) | HR, sub-modul 21 | `FE@9c40007 src/lib/services/hr/approval-inbox-service.js:6` memanggil `/v1/hr/workflow/approval-inbox` → **404**; `BE@396c836 WorkflowManagement/Controllers/ApprovalInboxController.cs:17` melayani `api/v1/corporate/human-resource/approval-inbox` → **401** | `REPAIR` | Di luar scope `S-A1`, tetapi menjadi prasyarat `HRD-DEC-055`. Lihat `HRD-TF-020` | **Tinggi** |
| `HRD-CAP-47` | Bukti pengujian Workforce Core | HR | `BE@396c836 Tests/` berisi 4 proyek test (Nutrition, OperatingRoom, PatientManagement, Pharmacy); **tidak satu pun** menyentuh HR | `MISSING` | `HRD-CAP-27` masih berlaku untuk HR | **Tinggi** |

### 14.4 Temuan baru yang perlu tindakan

#### `HRD-TF-020` — Seluruh service HR frontend yang baru memanggil awalan API yang tidak ada

| Field | Isi |
| --- | --- |
| Bukti | `FE@9c40007 src/lib/services/hr/`: **11 berkas** memanggil alamat berawalan `/v1/hr/...` — `approval-inbox-service.js`, `attendance-management-service.js`, `leave-admin-service.js`, `overtime-admin-service.js`, `payroll-integration-service.js`, `roster-scheduling-service.js`, `learning-development-service.js`, `performance-management-service.js`, `employee-relations-service.js`, `workforce-core-cross-employee-service.js`, dan `employee-self-service.js` (layanan mandiri cuti dan lembur, `/v1/hr/self-service/...`). Dua service lain sudah benar: benefit (`lib/constants/hr/benefit/employee-benefit-enrollment-constants.js:7`) dan exit clearance (`lib/constants/hr/exit-clearance/exit-clearance-constants.js:7`) memakai `/v1/corporate/human-resource/...`. `BE@396c836`: **0** route berawalan `api/v1/hr`. Uji lokal: `api/v1/hr/workflow/approval-inbox`, `api/v1/hr/leave`, `api/v1/hr/attendance` semuanya **404** |
| Dampak | Bukan hanya enam layar Workforce Core. Layar HR lain yang dibangun lewat task `FE-HRD-*` dan memakai service ini kemungkinan besar juga tampil kosong atau gagal. **Belum diverifikasi per layar** karena di luar batas scan ini |
| Klasifikasi | `REPAIR`, lintas sub-modul |
| Tindak lanjut yang disarankan | `diagnose-module-issue` untuk seluruh layar HR yang memakai `src/lib/services/hr/**`, dan tinjau ulang status ✅ task `FE-HRD-*` pada roadmap (`HRD-TF-011`) |

#### `HRD-TF-021` — Penempatan organisasi kini ikut menentukan hak akses unit

| Field | Isi |
| --- | --- |
| Bukti | `BE@396c836` commit `e1d11214`; `WfpOrganizationAssignmentController.cs:39-63` memanggil `ReconcileWorkforceProfileAsync` setelah setiap perubahan |
| Dampak | Setiap aturan penempatan pada `HRD-DEC-047` s.d. `HRD-DEC-050` kini juga aturan keamanan. Contoh: bila HR mengubah unit Rina lewat form master (`HRD-TF-013`), proyeksi hak akses Rina ikut berubah seketika, padahal perubahan itu belum disetujui siapa pun |
| Klasifikasi | Fakta yang memperberat `HRD-CAP-31` dan `HRD-CAP-32` |

### 14.5 Koreksi terhadap decision log

| Butir | Sebelum | Sesudah | Bukti |
| --- | --- | --- | --- |
| `HRD-TF-014` | "Penempatan organisasi, jabatan, dan atasan tidak diperiksa sama sekali" | Jabatan dan atasan menolak **kembar persis** pada periode beririsan; organisasi tidak diperiksa; aturan satu-utama-per-tanggal belum ada | `BE@396c836 WfpPositionAssignmentController.cs:396-400`, `WfpManagerAssignmentController.cs:373-378` |

Koreksi ini **tidak** mengubah `HRD-DEC-049`: aturan satu-utama-per-tanggal memang belum ada.

### 14.6 Kontrak as-is yang disentuh

Hanya endpoint yang relevan bagi keputusan bagian 31. Kode status umum: **401** berarti belum
login; **403** berarti pengguna tidak punya hak akses untuk tindakan itu; **404** berarti data
atau alamat tidak ditemukan.

#### Corporate / Human Resource / Workforce Core / Cross-Employee Administration

Base URL: `api/v1/corporate/human-resource/workforce-core`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/organization-assignments` | Daftar penempatan unit seluruh pegawai | `WfpOrganizationAssignment : ReadAll` | Query periode, unit, jabatan, jenis, utama, aktif, `search`, halaman | `PagedResult<WfpOrganizationAssignmentResponse>` |
| `GET` | `/position-assignments` | Daftar penempatan jabatan seluruh pegawai | `WfpPositionAssignment : ReadAll` | Query serupa, ditambah `isActing`, `jobLevelId` | `PagedResult<WfpPositionAssignmentResponse>` |
| `GET` | `/manager-assignments` | Daftar relasi atasan seluruh pegawai | `WfpManagerAssignment : ReadAll` | Query serupa, ditambah `managerProfileId`, `canApproveRequests` | `PagedResult<WfpManagerAssignmentResponse>` |
| `GET` | `/employment-histories` | Daftar riwayat kepegawaian seluruh pegawai | `WfpEmploymentHistory : ReadAll` | Query periode, `historyType`, unit, jabatan | `PagedResult<WfpEmploymentHistoryResponse>` |
| `GET` | `/salary-assignments` | Daftar penetapan gaji **tanpa nominal** | `WfpSalaryAssignment : ReadAll` | Query periode, struktur, grade, `isApproved` | `PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>` |
| `GET` | `/salary-assignments/{id}/amount` | Nominal satu penetapan gaji | `WfpSalaryAssignment : ViewAmount` | Path `id` | `WfpSalaryAmountResponse` |

**Tidak ada** endpoint `employee-profile-changes` di grup ini. Daftar perubahan data lintas-pegawai
ada di grup berikut.

#### Corporate / Human Resource / Workforce Core / Employee Profile Change Administration

Base URL: `api/v1/corporate/human-resource/employee-profile-changes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permohonan perubahan data seluruh pegawai | `EmployeeProfileChange : Read` | Query saringan dan halaman | `PagedResult<EmployeeProfileChangeListResponse>` |
| `GET` | `/{id}` | Detail satu permohonan | `EmployeeProfileChange : Read` | Path `id` | `EmployeeProfileChangeResponse` |
| `POST` | `/{id}/start-verification` | Memulai verifikasi | `EmployeeProfileChange : Update` | Path `id` | `EmployeeProfileChangeResponse` |
| `POST` | `/{id}/verifications/{verificationId}/decision` | Keputusan verifikasi — tidak memeriksa verifikator ≠ pembuat | `EmployeeProfileChange : Update` | Body keputusan, bukti berkas | `EmployeeProfileChangeResponse` |
| `POST` | `/{id}/approve`, `/{id}/reject`, `/{id}/request-revision`, `/{id}/apply` | Setujui, tolak, minta revisi, terapkan | `EmployeeProfileChange : Update` | Path `id`, body alasan | `EmployeeProfileChangeResponse` / `EmployeeProfileChangeApplyResponse` |

Contoh: permohonan berstatus `Approved` baru mengubah data pegawai setelah `POST /{id}/apply`.
Selama itu, alamat lama tetap tampil (`AC-F01-06`).

#### Corporate / Human Resource / Workforce Profile Management / Workforce Profile Overview

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/overview`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Ringkasan Employee 360: identitas, peran, jumlah data per bagian, daftar bagian rahasia | `WorkforceProfileOverview : Read` | Path `workforceProfileId` | `WorkforceDetailResponse` |

#### Corporate / Human Resource / Workforce Core / Salary Assignment

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/salary-assignments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/approval` | Menyetujui atau membatalkan persetujuan penetapan gaji. **Pembuat dapat menyetujui sendiri** | `WfpSalaryAssignment : Update` | Body `IsApproved` | `object` |

#### Corporate / Human Resource / Workforce Core / Bank Account

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/bank-accounts`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah rekening langsung — bertentangan dengan `HRD-DEC-052` | `WorkforceBankAccount : Update` | Body rekening | `object` |
| `PATCH` | `/{id}/verify` | Menandai rekening terverifikasi — pemegang hak `Update` yang sama, tanpa pemisahan peran | `WorkforceBankAccount : Update` | Body `IsVerified` | `object` |

#### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata`, `/summary`, `/` | Metadata saringan, ringkasan, dan daftar pengajuan yang menunggu pengguna | `ApprovalInbox : Read` | Query saringan | Lihat controller |

Frontend memanggil grup ini di alamat yang salah (`HRD-CAP-46`).

### 14.7 Ketidakcocokan frontend dan backend yang baru

| ID | Ketidakcocokan | Sisi yang benar | Catatan |
| --- | --- | --- | --- |
| `HRD-MM-06` | Enam layar Workforce Core memanggil `/v1/hr/workforce-core/cross-employee/*` | Backend `api/v1/corporate/human-resource/workforce-core` | `HRD-CAP-28` |
| `HRD-MM-07` | Layar Perubahan Data memanggil endpoint lintas-pegawai yang tidak ada | Backend `api/v1/corporate/human-resource/employee-profile-changes` | `HRD-CAP-28` |
| `HRD-MM-08` | Sebelas service HR memakai awalan `/v1/hr/` | Backend `api/v1/corporate/human-resource/...` | `HRD-TF-020` |
| `HRD-MM-09` | Form Master Data Pegawai masih bisa mengubah unit dan jabatan | Keputusan `HRD-DEC-047` | Frontend dan backend sama-sama perlu disesuaikan |

### 14.8 Pertanyaan penutup

Hanya hal yang tidak dapat dijawab source.

| ID | Pertanyaan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| `HRD-TQ-11` | Ke-14 profil yatim: data demo yang boleh dinonaktifkan, atau orang sungguhan yang perlu ditautkan? Setidaknya sebagian dibuat seeder demo kamar operasi | `IMPLEMENTATION` (pembersihan data) | Pemilik database |
| `HRD-TQ-12` | `HRD-TF-020`: apakah awalan `/v1/hr/` memang disengaja menunggu alias backend, atau kekeliruan frontend? Source backend tidak memuat rencana alias apa pun; `HRD-DEC-016` hanya mengatur alias route master data | `IMPLEMENTATION` (cara perbaikan) | Pemilik teknis |

Hal yang **dapat** diturunkan dari keputusan yang sudah ada dan diserahkan ke
`design-business-module`, bukan ditanyakan ulang: nasib `NewBaseSalary` pada model promosi dan
demosi (`HRD-DEC-048` `SALARY_STAYS_SEPARATE`), bentuk satu tabel atau lima tabel dokumen
peristiwa, dan nasib endpoint ubah-langsung penempatan (`HRD-CAP-31`).

### 14.9 Pemicu impact scan berikutnya

Selain bagian 11, bagian 14 harus di-scan ulang bila:

1. `396c8363` bukan lagi HEAD backend dan perubahannya menyentuh jalur bagian 14.1, atau
   `Services/Security/OrganizationAuthorizationProjectionService.cs`;
2. `9c4000724` bukan lagi HEAD frontend dan perubahannya menyentuh `src/lib/services/hr/**` atau
   `src/components/view/hr/workforce-core/**`;
3. `HRD-TF-020` diperbaiki, karena status `HRD-CAP-28` dan `HRD-CAP-46` berubah;
4. `HRD-TQ-11` dijawab dan data profil yatim dibersihkan.

### 14.10 Yang sengaja tidak dilakukan pada scan ini

- Tidak ada source aplikasi yang diubah. `HRD-TF-020` dan seluruh `REPAIR` hanya dicatat.
- Tidak ada login ke aplikasi. Uji HTTP hanya membedakan alamat yang ada (401) dan yang tidak ada
  (404).
- Domain HR di luar Workforce Core tidak di-scan ulang dan ditandai `STALE`.
- Tidak ada arsitektur target, kontrak target, maupun roadmap yang dibuat.

---

## 15. Impact Scan — HR di luar Workforce Core dan Perencanaan Tenaga Kerja (`S-D1`), 3 November 2026 (revisi `1.3`)

| Field | Nilai |
| --- | --- |
| Mode | **Impact scan terbatas**. Seluruh baris `HRD-CAP-01` s.d. `HRD-CAP-27` diklasifikasi dampaknya dari selisih source; audit mendalam hanya untuk `S-D1` dan titik sentuhnya, karena `S-D1` adalah slice berikutnya yang akan dirancang |
| Pemicu | Bagian 11 butir 1–4 dan bagian 14.9 butir 1–3 seluruhnya terpenuhi: HEAD kedua repository berpindah; registry prefix berubah (`Wfp` didaftarkan); `HRD-Q-05` dijawab untuk `S-D1` lewat `HRD-DEC-059`; `HRD-TF-020` sebagian diperbaiki |
| Backend | Titik verifikasi bagian 1–13 `e0ee42c` dan bagian 14 `396c8363` → HEAD **`26066c76e69b021f68b63b31bde2258cd9d1bb74`** (branch `MHamzah`). Keduanya leluhur HEAD. Working tree source bersih; yang berubah hanya dokumen blueprint |
| Frontend | Titik verifikasi bagian 1–13 `fff76a1b39` dan bagian 14 `9c4000724` → HEAD **`66b7e5600e1707e4153ca07981ddef054856c8c9`** (branch `HamzahV2`). Keduanya leluhur HEAD |
| Masukan bisnis | `00-interview-decisions.md` revisi `17`, bagian 32 (`HRD-DEC-058` s.d. `HRD-DEC-071`) |
| Contract version | `v6` menurut manifest. Belum ada kontrak `S-D1`; yang berlaku untuk `S-D1` hanya kontrak as-is pada bagian 15.7 |
| Cara scan | `git diff --name-status` terbatas pada jalur bagian 11, pembacaan statis controller/service/model, dan hitung baris database `QuilvianNewDevHamzah` read-only |
| Batas tulis | Dokumen ini. Tidak ada source aplikasi, build, migration, maupun penulisan database |

**Singkatan bukti bagian ini.** `BE@26066c7` = `NewQuilvianSystemBackend` commit `26066c76`.
`FE@66b7e56` = `QuilvianSystemFrontendDev` commit `66b7e5600`.

> **Peringatan data hidup.** Database dev sedang dipakai saat scan berlangsung. Pada 3 November
> 2026 pagi, `WfpPositionAssignment` berubah dari 0 menjadi 3 baris di antara dua query, karena dua
> batch penempatan awal dieksekusi. Angka baris pada bagian ini adalah potret saat diperiksa.

### 15.1 Batas scan

| Di-scan | Jalur |
| --- | --- |
| Backend — klasifikasi dampak | Seluruh jalur bagian 11 butir 1, ditambah `Services/Security/**` dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Backend — audit mendalam | `WorkforcePlanning/**`, `RecruitmentManagement/Models/TrxJobRequisition.cs`, `SchedulingManagement/**`, `MasterData/PayrollAndBenefit/**` (grade gaji), `MasterData/Organization/**`, `MasterData/Workforce/**`, `WorkforceCore/**` (penempatan, SK, batch awal), `WorkflowManagement/**`, `Migrations/**` (pola pengaman) |
| Frontend — klasifikasi dampak | Seluruh jalur bagian 11 butir 2, ditambah `src/lib/services/hr/**` dan `src/components/view/hr/**` |
| Frontend — audit mendalam | `src/lib/services/hr/**` (awalan alamat API), `src/utils/menu-sidebar/menu-items.jsx`, pencarian layar perencanaan tenaga |

**Tidak diaudit mendalam:** isi perilaku Benefit, Payroll, Lifecycle, Kehadiran, dan Workforce Core
di luar titik sentuh `S-D1`. Baris-baris itu diberi klasifikasi dampak, bukan dinilai ulang.

### 15.2 Perubahan sejak titik verifikasi terakhir

**Backend `e0ee42c..26066c76`, per domain HR:**

| Domain | Berkas berubah | Yang material | Klasifikasi |
| --- | ---: | --- | --- |
| `WorkforceCore` | 27 | Implementasi `S-A1`: SK (`HrdPersonnelActionOrder`), batch penempatan awal, Employee 360, kolom persetujuan dan rujukan SK pada penempatan organisasi/jabatan | `CAPABILITY_IMPACT`, `PERSISTENCE_IMPACT` |
| `BenefitManagement` | 22 | 4 controller dan 7 service baru | `CAPABILITY_IMPACT`, `PERSISTENCE_IMPACT` |
| `MasterData` | 18 | Master shift/jadwal, jenis potongan, kunci unit/jabatan pada form Pegawai, profil rumah sakit | `CAPABILITY_IMPACT` |
| `SchedulingManagement` | 8 | `RosterPeriodController`, `ShiftReplacementController`, `OnCallAssignmentController` baru | `CAPABILITY_IMPACT` |
| `AttendanceManagement` | 8 | Koreksi kehadiran atas nama pegawai (`HRD-DEC-028`) | `CAPABILITY_IMPACT`, `PERSISTENCE_IMPACT` |
| `PayrollManagement` | 6 | Restitusi dan potongan cicilan | `CAPABILITY_IMPACT` |
| `LifecycleManagement` | 4 | `ExitClearanceController` baru | `CAPABILITY_IMPACT` |
| `Services/Security` | 4 | Proyeksi hak akses unit, validator registry hak akses | `CAPABILITY_IMPACT` |
| Registry prefix | 1 | Baris `Wfp` = *Workforce Profile* didaftarkan 28 Agustus 2026 | `DECISION_IMPACT` |
| `WorkforcePlanning`, `RecruitmentManagement`, `LeaveManagement`, `OvertimeManagement`, `LearningAndDevelopment`, `PerformanceManagement`, `CredentialingManagement`, `OccupationalHealthManagement`, `EmployeeRelationManagement`, `HrServiceManagement`, `BusinessTravelManagement`, `ExpenseManagement`, `WorkflowManagement`, `Areas/SelfServices/HumanResource`, `Shared/HumanResource` | **0** | — | `NO_CAPABILITY_IMPACT` |

**Frontend `fff76a1b39..66b7e5600`:** perubahan terbesar ada di `lib/constants/hr` (69 berkas),
`lib/hooks/hr` (56), `view/hr/master-data` (24), `lib/services/hr` (16), serta route dan view
`workforce-core` (masing-masing 13). Commit `66b7e5600` menambah dua berkas unit test Workforce Core.
Tidak ada satu pun route, view, maupun service perencanaan tenaga.

### 15.3 Dampak terhadap baris `HRD-CAP-01` s.d. `HRD-CAP-27`

"Bukti" menyatakan apakah bukti lama bagian 4 masih dapat dipakai. `CURRENT` berarti jalur
pemiliknya tidak berubah sejak `e0ee42c`/`fff76a1b39`.

| ID | Kebutuhan | Status lama | Status sekarang | Bukti | Alasan |
| --- | --- | --- | --- | --- | --- |
| `HRD-CAP-01` | Master data HR | `READY TO REUSE` | `READY TO REUSE` | `STALE` sebagian | Master shift/jadwal, jenis potongan, dan form Pegawai berubah. Master yang dipakai `S-D1` diperiksa ulang pada 15.5 |
| `HRD-CAP-02` | Profil pegawai dan berkas | `READY TO REUSE` | `READY TO REUSE` | `STALE` | Dicakup ulang bagian 14 dan 15.4 |
| `HRD-CAP-03` | Menu Administrasi Kepegawaian | `REPAIR` | `REPAIR` | `STALE` | Menu dan route `workforce-core` berubah; tambah temuan `HRD-TF-036` |
| `HRD-CAP-04` | Kehadiran dan koreksi | `EXTEND` | `EXTEND` | `STALE` | Penambahan koreksi atas nama; tidak ada yang dihapus |
| `HRD-CAP-05` | Kehadiran layanan mandiri | `REPAIR` | `REPAIR` | `STALE` (FE) | Backend tidak berubah; 5 berkas route layanan mandiri frontend berubah |
| `HRD-CAP-06` | Konteks pengguna layanan mandiri | `READY TO REUSE` | `READY TO REUSE` | `CURRENT` | — |
| `HRD-CAP-07` | Cuti, izin, saldo | `EXTEND` | `EXTEND` | `CURRENT` (BE), `STALE` (FE) | Service frontend kini memakai awalan yang benar |
| `HRD-CAP-08` | Lembur | `EXTEND` | `EXTEND` | `CURRENT` (BE), `STALE` (FE) | Service frontend masih memanggil awalan yang tidak ada — `HRD-TF-035` |
| `HRD-CAP-09` | Penjadwalan, shift, tukar shift | `EXTEND` | `EXTEND` | `STALE` | Kini 6 controller. `TrxEmergencyStaffingRequest` tetap tanpa controller. Pemeriksaan roster belum membaca standar tenaga — `HRD-CAP-59` |
| `HRD-CAP-10` | Payroll | `REUSE WITH ADAPTER` | `REUSE WITH ADAPTER` | `STALE` | Penambahan restitusi dan cicilan |
| `HRD-CAP-11` | Benefit | `MISSING` | **`UNKNOWN`** | `STALE` | `MISSING` **terbantah**: 4 controller dan 7 service baru. Status sebenarnya baru dapat ditetapkan lewat audit penuh |
| `HRD-CAP-12` | Kredensial dan kewenangan klinis | `EXTEND` | `EXTEND` | `CURRENT` | — |
| `HRD-CAP-13` | OPPE dan FPPE | `MISSING` | `MISSING` | `CURRENT` | — |
| `HRD-CAP-14` | Kompetensi dan pelatihan | `EXTEND` | `EXTEND` | `CURRENT` (BE), `STALE` (FE) | Service frontend memanggil awalan yang tidak ada — `HRD-TF-035` |
| `HRD-CAP-15` | Manajemen kinerja | `EXTEND` | `EXTEND` | `CURRENT` (BE), `STALE` (FE) | Sama |
| `HRD-CAP-16` | Kesehatan dan keselamatan kerja | `EXTEND` | `EXTEND` | `CURRENT` | — |
| `HRD-CAP-17` | Lifecycle dan offboarding | `EXTEND` | `EXTEND` | `STALE` | Penambahan exit clearance |
| `HRD-CAP-18` | Hubungan karyawan dan disiplin | `EXTEND` | `EXTEND` | `CURRENT` (BE domain), `STALE` (FE dan master) | Empat controller master hubungan karyawan di folder konfigurasi berubah (lihat `HRD-TF-004`); service frontend memanggil awalan yang tidak ada |
| `HRD-CAP-19` | Perencanaan tenaga kerja | `MISSING` | `MISSING` | **`CURRENT`** | Tidak ada perubahan. Dirinci per kebutuhan `S-D1` pada 15.5 |
| `HRD-CAP-20` | Rekrutmen dan hiring | `MISSING` | `MISSING` | `CURRENT` | — |
| `HRD-CAP-21` | Layanan HR | `MISSING` | `MISSING` | `CURRENT` | — |
| `HRD-CAP-22` | Perjalanan dinas | `MISSING` | `MISSING` | `CURRENT` | — |
| `HRD-CAP-23` | Workflow bersama | `EXTEND` | `EXTEND` | `CURRENT` | — |
| `HRD-CAP-24` | Antarmuka atasan dan kotak masuk | `MISSING` | **`REPAIR`** | `STALE` | Sudah digantikan `HRD-CAP-46`. Awalan alamat frontend kini benar; belum diuji end-to-end |
| `HRD-CAP-25` | Layanan mandiri selain kehadiran | `EXTEND` | `EXTEND` | `STALE` (FE) | Service layanan mandiri frontend kini cocok dengan route backend `api/v1/self-services/human-resource/**` |
| `HRD-CAP-26` | Hak akses dan jejak audit | `READY TO REUSE` | `READY TO REUSE` | `STALE` | Proyeksi hak akses unit dan validator registry hak akses baru |
| `HRD-CAP-27` | Bukti pengujian | `MISSING` | `MISSING` | `STALE` | Backend: tetap tanpa proyek test HR; satu test kepemilikan benefit menumpang di proyek Pharmacy. Frontend: dua unit test Workforce Core baru |

**Rekap:** 14 baris `CURRENT` (seluruhnya atau di sisi backend), 13 baris `STALE`. Dua status berubah:
`HRD-CAP-11` menjadi `UNKNOWN` dan `HRD-CAP-24` menjadi `REPAIR`.

### 15.4 Dampak terhadap bagian 14 (`HRD-CAP-28` s.d. `HRD-CAP-47`)

Commit `S-A1` mengubah banyak berkas yang menjadi bukti bagian 14. Yang diperiksa ulang hanya baris
yang menyangkut keputusan kunci atau titik sentuh `S-D1`. Sisanya **`STALE`** sampai
`verify-module-readiness` menilai `S-A1`.

| ID | Status bagian 14 | Status sekarang | Bukti | Catatan |
| --- | --- | --- | --- | --- |
| `HRD-CAP-28` | `REPAIR` | `REPAIR` — perlu uji ulang | `FE@66b7e56 workforce-core-cross-employee-service.js` memanggil `/v1/corporate/human-resource/workforce-core` dan `/employee-profile-changes` | Awalan sudah benar lewat `PLAN-REPAIR-HRD-001`; tampilan belum diuji |
| `HRD-CAP-31` | `CONFLICT` | `CONFLICT` | `BE@26066c7` ketiga controller penempatan masih punya 4 endpoint ubah langsung masing-masing, tidak berubah sejak `396c8363` | — |
| `HRD-CAP-32` | `CONFLICT` | `CONFLICT` — sebagian diperbaiki | `BE@26066c7 MasterData/Workforce/Controllers/EmployeeController.cs:1284` menolak perubahan unit/jabatan utama dan mengarahkan ke SK. `DoctorController` dan `ExternalUserController` **belum** punya kunci serupa; `EnsureWorkforcePrimaryOrganizationAssignmentAsync` masih ada di ketiganya | Pegawai sudah aman; Dokter dan Pengguna Eksternal belum |
| `HRD-CAP-35` | `MISSING` | `REUSE WITH ADAPTER` untuk `S-D1` | `BE@26066c7 WorkforceCore/Controllers/HrdPersonnelActionOrderController.cs`: buat, ubah, lampiran, ajukan, setujui, tolak, batal; `HrdPersonnelActionOrderService.cs:361-364` menolak pembuat menyetujui SK-nya sendiri | Dipakai `S-D1` sebagai jalan kursi internal (`HRD-CAP-58`). Penilaian penuh `S-A1` tetap pada `verify-module-readiness` |
| `HRD-CAP-41` | `REPAIR` | `REPAIR` | `WfpSalaryAssignmentController.cs` tidak berubah sejak `396c8363` | Swa-setuju gaji masih mungkin |
| `HRD-CAP-44` | `MISSING` | **`CONFLICT`** | `BE@26066c7 HrdInitialPlacementBatchController.cs:132-142` hanya punya `POST {id}/execute` dengan hak `Update`; `HrdInitialPlacementBatchService.cs:471` mengisi `ApprovedByUserId` dengan pengguna yang mengeksekusi. Data: 2 batch `Executed`, keduanya pengaju = penyetuju | Lihat `HRD-TF-033`, `HRD-TF-034` |
| `HRD-CAP-46` | `REPAIR` | `REPAIR` — perlu uji ulang | `FE@66b7e56 approval-inbox-service.js` kini memanggil `/v1/corporate/human-resource/approval-inbox` | Awalan sudah benar |
| `HRD-CAP-47` | `MISSING` | `MISSING` (BE) | Sama dengan `HRD-CAP-27` | Frontend punya `tests/unit/workforce-core-cross-employee.test.mjs` dan `workforce-core-target-state.test.mjs` |

### 15.5 Kontrak Bukti Kemampuan — Perencanaan Tenaga Kerja (`S-D1`)

Lanjutan penomoran. Setiap baris memetakan satu kebutuhan dari keputusan `HRD-DEC-058` s.d.
`HRD-DEC-071` ke bukti yang ada hari ini. Seluruh model `WorkforcePlanning` adalah **kandidat**
menurut `HRD-DEC-004`, bukan jawaban.

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-CAP-48` | Standar kebutuhan tenaga berversi dengan tiga metode (`HRD-DEC-060`) | HR | `BE@26066c7 WorkforcePlanning/Models/MstStaffingStandard.cs` (`StandardType` `FixedHeadcount`/`WorkloadBased`/`RatioBased`/`ServiceCoverage`/`Custom`, `CoverageHoursPerDay`, tanggal berlaku) dan `MstStaffingRatio.cs` (`RoundingMethod`); 0 controller; 0 baris | `MISSING` | Skema kandidat belum punya faktor cadangan, angka beban per periode beserta sumbernya, rujukan pengesahan `T9`, maupun rincian hitung | Sedang |
| `HRD-CAP-49` | Skill mix minimum per unit/shift sebagai aturan (`HRD-DEC-058`) | HR | `MstShiftSkillRequirement.cs` (`MinimumQualifiedHeadcount`, `CompetencyId`, `MinimumCompetencyLevel`, `IsMandatory`); 0 controller; dirujuk `TrxShiftAssignment.ShiftSkillRequirementId` | `MISSING` | Skema kandidat cukup dekat; perlu berada di bawah versi standar dan pengesahan `T9` | Rendah |
| `HRD-CAP-50` | Formasi berversi, tidak dapat diedit langsung, bersumber dokumen (`HRD-DEC-062`) | HR | `MstPositionHeadcountPlan.cs` per jabatan per `PlanYear` dengan `PlanStatus` `Draft`/`Proposed`/`Approved`/`Closed`/`Cancelled`; 0 controller; 0 baris | `MISSING` | Kandidat bersiklus status padahal ber-prefix master; tidak ada nomor versi, rujukan dokumen sumber, maupun jaminan satu versi berlaku per tanggal (`HRD-TF-027`) | Sedang |
| `HRD-CAP-51` | Rencana tahunan terkunci beserta addendum (`HRD-DEC-062`, `HRD-DEC-068`) | HR | `TrxAnnualManpowerPlan.cs` (status `Draft` s.d. `Cancelled`, `WorkflowDefinitionId`, `WorkflowInstanceId`, `ApprovedBudgetAmount`) dan `TrxManpowerPlanDetail.cs`; 0 controller | `MISSING` | Tidak ada konsep addendum maupun rujukan rencana induk | Sedang |
| `HRD-CAP-52` | Potret gap terkunci saat pengajuan dan bulanan (`HRD-DEC-069`) | HR | `TrxStaffingGapAnalysis.cs` (`RequiredHeadcount`, `AvailableHeadcount`, `OnLeaveHeadcount`, `QualifiedHeadcount`, `GapSeverity`, `AnalysisSource`); 0 controller | `MISSING` | Kandidat belum punya lapisan `HRD-DEC-061` (porsi kerja, akan kosong, rangkap/Plt, alih daya, kelengkapan data), tanggal acuan, maupun rujukan dokumen yang dipotret | Sedang |
| `HRD-CAP-53` | Tenaga tersedia dari penempatan utama per tanggal, dalam porsi kerja (`HRD-DEC-061`) | 02 Workforce Core | `BE@26066c7 WfpOrganizationAssignment.cs`, `WfpPositionAssignment.cs` kini punya `PersonnelActionOrderId`, `ApprovedByUserId`, `ApprovedAt`; `AssignmentType` jabatan `Substantive`/`Acting`/`Functional`/`Project`/`Temporary`. **Tidak ada kolom porsi kerja.** Data: 55 penempatan organisasi, 3 penempatan jabatan | `EXTEND` | Tambah porsi kerja (bawaan 1) di Workforce Core; isi master jenis tenaga (`MstWorkforceType` hanya 3 nilai `Legacy`) | Sedang |
| `HRD-CAP-54` | Permintaan headcount dengan sisa kursi, rencana pemenuhan, dan pembatalan per kursi (`HRD-DEC-063`, `HRD-DEC-067`, `HRD-DEC-071`) | HR | `TrxHeadcountRequest.cs` (`RequestType` `Additional`/`Replacement`/`Temporary`/`Project`/`Seasonal`, `RequestedHeadcount`, `ApprovedHeadcount`, `FulfilledAt`, workflow); 0 controller | `MISSING` | Tidak ada tanda "di luar formasi", rujukan orang yang digantikan, rencana pemenuhan luar/internal, status per kursi, maupun tautan ke penempatan yang memenuhi | Sedang |
| `HRD-CAP-55` | Rantai persetujuan `T9` s.d. `T12` (`HRD-DEC-064`) | Shared platform | `WorkflowManagement` tidak berubah sejak `e0ee42c`; pola pendaftaran jenis rujukan `WorkflowReferenceLifecycleService.cs` dan pola integrasi per domain (`HRD-CAP-36`); kandidat rencana dan permintaan sudah punya `WorkflowDefinitionId`/`WorkflowInstanceId` | `REUSE WITH ADAPTER` | Daftarkan empat jenis rujukan dan definisi alurnya | Rendah |
| `HRD-CAP-56` | Peran Finance / Anggaran, Direktur, Recruiter (`HRD-DEC-064`, `HRD-DEC-066`) | Administrator | Peran di database dev: `Manajer Finance`, `SuperAdmin`, `Supervisor`, `Supervisor Finance`, `User`. Tidak ada peran HR, Direktur, maupun Recruiter. `Migrations/scripts/be-sec-014-post-seeder-hr-initial-grants.sql` menyebut `Manajer HR` dan `Staff HR`, tetapi berstatus **rancangan, belum dijalankan** | `MISSING` | Pemetaan peran tetap `MAPPING_REQUIRED` (`HRD-DEC-032`). Peran Finance yang sudah ada **MUST NOT** dianggap otomatis sama dengan peran fungsional Finance / Anggaran | Sedang |
| `HRD-CAP-57` | Direktur yang membawahi unit dari struktur organisasi (`HRD-DEC-064` `DIRECTOR_FROM_ORG_STRUCTURE`) | 01 Master Data | `MasterData/Organization/Models/MstOrganizationUnit.cs`: `ParentOrganizationUnitId`, `UnitType`, `LevelNumber`. Data: 5 unit — 1 `Directorate`, 1 `Department`, 2 `Installation`, 1 `Unit` | `EXTEND` | Hierarki dan jenis direktorat dapat ditelusuri. **Siapa** direktur sebuah direktorat tidak dimodelkan: tidak ada kolom kepala unit | Sedang |
| `HRD-CAP-58` | Kursi internal dipenuhi lewat SK (`HRD-DEC-067`) | 02 Workforce Core | `HrdPersonnelActionOrderController.cs` (ajukan, setujui, tolak, batal), anti swa-setuju `HrdPersonnelActionOrderService.cs:361-364`; penempatan menyimpan `PersonnelActionOrderId` | `REUSE WITH ADAPTER` | Perlu tautan SK atau penempatannya ke kursi permintaan headcount | Rendah |
| `HRD-CAP-59` | Penjadwalan memeriksa roster terhadap standar dan mengirim ringkasan kekurangan (`HRD-DEC-058`) | 06 Penjadwalan | `SchedulingManagement/Controllers/RosterPeriodController.cs` punya `validate`, `publish`, `close`; `RosterPeriodService.cs:479-487` hanya memeriksa jeda istirahat minimum. Tidak ada rujukan ke `MstStaffingStandard`, `MstShiftSkillRequirement`, maupun `TrxDailyStaffingRequirement`. `TrxEmergencyStaffingRequest` tanpa controller | `MISSING` | Dirancang pada sisi Penjadwalan; `S-D1` hanya menyediakan kontrak baca standar | Sedang |
| `HRD-CAP-60` | Biaya dari gaji tengah grade (`HRD-DEC-065`) | 01 Master Data | `MasterData/PayrollAndBenefit/Models/MstSalaryGrade.cs` (`MinimumSalary`, `MidpointSalary`, `MaximumSalary`, tanggal berlaku, `EmployeeGradeId`); `SalaryGradeController` (`SalaryGrade : Read`); tidak berubah sejak `e0ee42c`; 5 baris | `READY TO REUSE` | Baca saja | Rendah |
| `HRD-CAP-61` | Batch formasi awal yang disetujui sekali, berdokumen sumber (`HRD-DEC-070`) | HR | Pola batch `HrdInitialPlacementBatch` (template, validasi, buat, eksekusi) dan lampiran `HrdPersonnelActionOrderAttachment` | `REUSE WITH ADAPTER` | **Jangan meniru cacat `HRD-TF-033`.** Batch formasi wajib punya langkah persetujuan oleh orang lain lewat `T10` dan lampiran wajib | Sedang |
| `HRD-CAP-62` | Pengaman tabel kosong pada migration (`HRD-DEC-059` `EMPTY_TABLE_GUARD`) | HR | Preseden `Migrations/20260902072756_DropTableMstBillingCategory.cs:36-42, 74-80`: blok `DO $$ … IF EXISTS (…) THEN RAISE EXCEPTION … END IF; $$` sebelum membuang tabel | `READY TO REUSE` | Pola ditiru per tabel `WorkforcePlanning` | Rendah |
| `HRD-CAP-63` | Requisition merujuk permintaan headcount (`HRD-DEC-066`) | 04 Rekrutmen | `RecruitmentManagement/Models/TrxJobRequisition.cs:21-23, 97` (`HeadcountRequestId`, `AnnualManpowerPlanId`, `ManpowerPlanDetailId`); 0 controller | `MISSING` | `S-D2` `BLOCKED`; sementara tautan manual dari penempatan ke permintaan | Sedang |
| `HRD-CAP-64` | Pemisahan "persyaratan berkas" dari "kebutuhan jumlah tenaga" | 01 Master Data | `MstWorkforceRequirement` dilayani `WorkforceRequirementController` (kategori `Document`, `License`, `ClinicalPrivilege`, …); menu `FE@66b7e56 menu-items.jsx:251` berlabel **"Kebutuhan Tenaga Kerja"** | `CONFLICT` | Label menu tabrakan dengan sub-modul 03. Lihat `HRD-TF-036` | Sedang |
| `HRD-CAP-65` | Layar perencanaan tenaga | HR | `FE@66b7e56`: tidak ada route, view, service, maupun item menu perencanaan tenaga | `MISSING` | — | Rendah |
| `HRD-CAP-66` | Prefix dan registry untuk entity `S-D1` baru (`HRD-DEC-019`) | Registry | `BE@26066c7 docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:11` `Human Resource` / `Hrd`; `:12` `WorkforceCore / WorkforceProfileManagement` / `Wfp`; `:103` submodule HR lain yang memuat `Wfp*` diperlakukan legacy. Tidak ada baris khusus `WorkforcePlanning` | `UNKNOWN` | Belum terbukti apakah checker `tooling/qbe/Invoke-QbeConformanceCheck.ps1` mencocokkan `Areas/Corporate/HumanResource/WorkforcePlanning/` ke baris `Human Resource` / `Hrd`, atau menuntut baris tersendiri (`QBE-MOD-003`). Lihat `HRD-TQ-13` | Sedang |

### 15.6 Temuan baru yang perlu tindakan

#### `HRD-TF-033` — Batch penempatan awal disetujui sendiri oleh pengeksekusinya

| Field | Isi |
| --- | --- |
| Bukti | `BE@26066c7 WorkforceCore/Controllers/HrdInitialPlacementBatchController.cs:132-142` — satu-satunya transisi adalah `POST {id}/execute` dengan hak `HrdInitialPlacementBatch : Update`. `WorkforceCore/Services/HrdInitialPlacementBatchService.cs:470-472` mengisi `BatchStatus = "Executed"`, `ApprovedByUserId = userId`, `ApprovedAt = now` tanpa membandingkan dengan pengaju. Data dev 3 November 2026: `BATCH-INIT-20261103-0001` dan `-0002`, keduanya **pengaju = penyetuju** |
| Dampak | Bertentangan dengan `HRD-DEC-031` (penyetuju ≠ pembuat) dan `HRD-DEC-054` (batch disetujui sekali). Bandingkan dengan SK, yang sudah menolak swa-setuju (`HrdPersonnelActionOrderService.cs:361-364`) |
| Klasifikasi | `CONFLICT` — milik `S-A1`, bukan `S-D1` |
| Tindak lanjut | `diagnose-module-issue` untuk `S-A1`; tinjau ulang tanda selesai task batch pada roadmap (`HRD-TF-011`) |

#### `HRD-TF-034` — Batch dengan baris gagal tetap dieksekusi dan dilaporkan sukses

| Field | Isi |
| --- | --- |
| Bukti | Data dev: `BATCH-INIT-20261103-0001` tercatat `TotalRows 1`, `ValidatedRows 0`, `FailedRows 1`, tetapi `BatchStatus Executed` dengan ringkasan "Sukses menerapkan penempatan awal untuk 1 pegawai."; barisnya bertanda `IsValid = true`. `HrdInitialPlacementBatchService.cs:332-335` hanya menolak batch yang sudah `Executed` |
| Dampak | Hitungan validasi dan hasil eksekusi tidak dapat dipercaya. Untuk `S-D1`, angka **terisi** bergantung pada penempatan hasil batch ini |
| Klasifikasi | `REPAIR` — milik `S-A1` |
| Tindak lanjut | Sama dengan `HRD-TF-033` |

#### `HRD-TF-035` — Empat service HR frontend masih memanggil awalan API yang tidak ada

| Field | Isi |
| --- | --- |
| Bukti | `FE@66b7e56 src/lib/services/hr/`: `employee-relations-service.js` (`/v1/hr/workforce-profile/disciplinary-actions`), `learning-development-service.js` (`/v1/hr/workforce-profile`), `overtime-admin-service.js` (`/v1/hr/overtime`), `performance-management-service.js` (`/v1/hr/workforce-profile/performance-reviews`). `BE@26066c7`: 0 route berawalan `api/v1/hr`; padanan backend ada di `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/...` dan `.../overtime-management/...` |
| Dampak | Lanjutan `HRD-TF-020`. Dua belas service lain sudah diperbaiki lewat `PLAN-REPAIR-HRD-001`; empat ini belum, sehingga layar disiplin, pelatihan, lembur admin, dan kinerja kemungkinan masih gagal memuat |
| Klasifikasi | `REPAIR` — lintas sub-modul, di luar `S-D1` |
| Tindak lanjut | Lanjutkan `PLAN-REPAIR-HRD-001` lewat `diagnose-module-issue` |

#### `HRD-TF-036` — Menu "Kebutuhan Tenaga Kerja" membuka master persyaratan berkas

| Field | Isi |
| --- | --- |
| Bukti | `FE@66b7e56 src/utils/menu-sidebar/menu-items.jsx:251-254`: label "Kebutuhan Tenaga Kerja", key `corporateWorkforceRequirement`, tujuan `/hr/master-data/workforce-requirement`, yaitu master `MstWorkforceRequirement` (`HRD-TF-023`) |
| Dampak | Begitu sub-modul 03 punya layar, pengguna akan melihat dua menu bermakna "kebutuhan tenaga" dengan isi yang sama sekali berbeda |
| Klasifikasi | `CONFLICT` penamaan — pengubahan label adalah keputusan UI, bukan keputusan skill ini |
| Tindak lanjut | Diputuskan saat desain frontend `S-D1` (`03-frontend-architecture.md`), dengan pengguna sebagai pemilik keputusan UI |

#### `HRD-TF-037` — `Wfp` sudah terdaftar di registry, sehingga `HRD-DEP-001` dan `HRD-TF-002` usang

| Field | Isi |
| --- | --- |
| Bukti | `BE@26066c7 docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:12, 52, 97-103, 122` — baris `Wfp` = *Workforce Profile* didaftarkan 28 Agustus 2026 |
| Dampak | `01-prerequisite-readiness.md` `HRD-DEP-001` masih berstatus `REPAIR` dengan alasan "`Wfp` belum terdaftar", dan `HRD-TF-002` bagian 5 menyatakan 40 entity memakai prefix tidak terdaftar. Keduanya tidak lagi sesuai source |
| Klasifikasi | Bukti untuk pembaruan status, bukan keputusan baru |
| Tindak lanjut | `manage-module-blueprint` memperbarui `HRD-DEP-001` dengan bukti ini |

### 15.7 Kontrak as-is yang disentuh `S-D1`

Hanya endpoint yang akan dibaca atau ditiru `S-D1`. Seluruhnya ada pada `BE@26066c7`.

#### Corporate / Human Resource / Master Data / Salary Grade

| Metode | Path | Hak akses | Dipakai `S-D1` untuk |
| --- | --- | --- | --- |
| `GET` | `api/v1/corporate/human-resource/master-data/salary-grades` | `SalaryGrade : Read` | Mengambil gaji tengah grade untuk estimasi biaya (`HRD-DEC-065`) |
| `GET` | `api/v1/corporate/human-resource/master-data/salary-grades/options` | `SalaryGrade : Read` | Pilihan grade pada baris rencana dan permintaan |
| `GET` | `api/v1/corporate/human-resource/master-data/salary-grades/{id}` | `SalaryGrade : Read` | Detail satu grade |

#### Corporate / Human Resource / Master Data / Workforce Requirement

| Metode | Path | Hak akses | Catatan |
| --- | --- | --- | --- |
| `GET`, `POST`, `PUT`, `PATCH`, `DELETE` | `api/v1/corporate/human-resource/master-data/workforce-requirements/**` | `WorkforceRequirement : *` | Master persyaratan berkas. **`S-D1` MUST NOT memakainya** sebagai sumber kebutuhan jumlah tenaga (`HRD-TF-023`) |

#### Corporate / Human Resource / Workforce Core / Personnel Action Order

| Metode | Path | Hak akses | Dipakai `S-D1` untuk |
| --- | --- | --- | --- |
| `POST` | `api/v1/corporate/human-resource/personnel-action-orders` | `HrdPersonnelActionOrder : Create` | Membuat SK mutasi untuk kursi internal |
| `POST` | `api/v1/corporate/human-resource/personnel-action-orders/{id}/submit` | `HrdPersonnelActionOrder : Update` | Mengajukan SK |
| `POST` | `api/v1/corporate/human-resource/personnel-action-orders/{id}/approve` | `HrdPersonnelActionOrder : Update` | Menyetujui SK — menolak pembuatnya sendiri |
| `POST` | `api/v1/corporate/human-resource/personnel-action-orders/{id}/cancel-request` | `HrdPersonnelActionOrder : Update` | Pembatalan SK sebelum berlaku (`HRD-DEC-057`); memicu perubahan rencana pemenuhan `HRD-DEC-067` |

#### Corporate / Human Resource / Workforce Core / Initial Placement Batch

| Metode | Path | Hak akses | Dipakai `S-D1` untuk |
| --- | --- | --- | --- |
| `GET` | `api/v1/corporate/human-resource/workforce-core/initial-batches/template` | `HrdInitialPlacementBatch : Read` | Pola templat impor batch formasi awal |
| `POST` | `api/v1/corporate/human-resource/workforce-core/initial-batches/validate` | `HrdInitialPlacementBatch : Read` | Pola validasi baris sebelum disimpan |
| `POST` | `api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute` | `HrdInitialPlacementBatch : Update` | **Tidak ditiru** — tanpa langkah persetujuan (`HRD-TF-033`) |

#### Corporate / Human Resource / Scheduling Management / Roster Periods

| Metode | Path | Hak akses | Titik sentuh dengan `S-D1` |
| --- | --- | --- | --- |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/validate` | `RosterPeriod : Update` | Tempat Penjadwalan kelak membaca standar `S-D1` (`HRD-CAP-59`) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/publish` | `RosterPeriod : Update` | Sama |

### 15.8 Ketidakcocokan frontend dan backend yang baru

| ID | Frontend | Backend | Akibat |
| --- | --- | --- | --- |
| `HRD-TF-035` | Empat service memanggil `/v1/hr/...` | Route sebenarnya `api/v1/corporate/human-resource/...` | Layar terkait gagal memuat |
| `HRD-TF-036` | Label menu "Kebutuhan Tenaga Kerja" | Master persyaratan berkas | Makna menu menyesatkan |

### 15.9 Pertanyaan penutup

Hanya hal yang tidak dapat dijawab source.

| ID | Pertanyaan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| `HRD-TQ-13` | Untuk entity `Hrd*` baru di folder `WorkforcePlanning`, cukupkah baris registry `Human Resource` / `Hrd`, atau perlu baris tersendiri sesuai `QBE-MOD-003`? | `IMPLEMENTATION` — sebelum file model pertama dibuat | Pemilik registry backend |
| `HRD-TQ-14` | Siapa yang dianggap direktur sebuah direktorat: pemegang jabatan tertentu pada unit `Directorate`, atau kolom kepala unit baru? (`HRD-CAP-57`) | `DESIGN` untuk `DIRECTOR_FROM_ORG_STRUCTURE` | Pengguna |
| `HRD-TQ-15` | Dua batch penempatan awal yang sudah dieksekusi dengan swa-setuju (`HRD-TF-033`) — dibiarkan sebagai data dev, atau diulang dengan persetujuan yang benar? | `IMPLEMENTATION` (data `S-A1`) | Pengguna / pemilik database |

**Ditutup oleh bukti:** `HRD-TQ-12` — awalan `/v1/hr/` ternyata kekeliruan frontend. `PLAN-REPAIR-HRD-001`
memperbaikinya di dua belas service (`approval-inbox-service.js` baris 4 menyebut `FIX-HRD-001-05`);
empat sisanya dicatat `HRD-TF-035`.

### 15.10 Pemicu impact scan berikutnya

1. `26066c76` bukan lagi HEAD backend, dan perubahannya menyentuh jalur bagian 11 atau 15.1.
2. `66b7e5600` bukan lagi HEAD frontend, dan perubahannya menyentuh jalur bagian 11 butir 2 atau
   `src/lib/services/hr/**`.
3. Desain `S-D1` ditulis. Baris `HRD-CAP-48` s.d. `HRD-CAP-66` dinilai ulang terhadap desain itu.
4. `EMPTY_TABLE_GUARD` berhenti di lingkungan mana pun (`HRD-DEC-059`).
5. `HRD-TF-033` s.d. `HRD-TF-035` diperbaiki.

### 15.11 Yang sengaja tidak dilakukan pada scan ini

- Tidak ada source aplikasi yang diubah. Seluruh `REPAIR` dan `CONFLICT` hanya dicatat.
- Perilaku Benefit, Payroll, Lifecycle, dan Kehadiran tidak diaudit ulang; hanya diberi klasifikasi
  dampak.
- Klaim selesai `S-A1` tidak dinilai. Itu pekerjaan `verify-module-readiness`.
- Checker QBE tidak dijalankan.
- Tidak ada arsitektur target, kontrak target, maupun roadmap yang dibuat.

---

## 16. Impact Scan — Penjadwalan (`S-B4`) dan Kehadiran (`S-B1`) sesudah Amendment Pass bagian 52, 12 November 2026 (revisi `1.4`)

### 16.1 Batas scan

| Field | Nilai |
| --- | --- |
| Mode | **Impact scan terbatas.** Hanya kemampuan yang disentuh 12 menu `PRD-HRD-PK-001` dan keputusan `HRD-DEC-194` s.d. `HRD-DEC-214` |
| Backend | `NewQuilvianSystemBackend` commit `a437ae42` (branch `yoga`) |
| Frontend | `QuilvianSystemFrontendDev` commit `6a5658269` (branch `YogaV2`) |
| Masukan bisnis | `00-interview-decisions.md` revision `36` (bagian 52), SHA-256 `f2cbab3243055a6359399d16190210e2e1f7b453fb4bbb5fc3aa3d222ec2b174` |
| Kontrak yang berlaku | `contract_versions: v25` pada manifest; `contract_status_s_b1` dan `contract_status_s_b4` = `approved` |
| Cara audit | Pembacaan statis model, service, controller, rute, atribut hak akses, service frontend, view, dan menu sidebar. **Tidak** ada build, tidak ada eksekusi database, tidak ada perubahan source |
| Batas tulis | Hanya dokumen ini |

**Cara membaca bukti.** `BE@a437ae42 path:baris` berarti berkas di repository
`NewQuilvianSystemBackend` pada commit `a437ae42`, baris tersebut. `FE@6a5658269` untuk
`QuilvianSystemFrontendDev`. Path backend dimulai dari `Areas/Corporate/HumanResource/` bila
tidak disebut lain.

### 16.2 Jawaban pertanyaan audit bagian 52.7

#### (1) Riwayat Ganti Petugas terhadap `HRD-DEC-195`

**Fakta.**

- Ganti Petugas langsung menyimpan satu baris `TrxShiftReplacement` berisi petugas asal,
  petugas pengganti, kategori alasan, alasan, pelaku, dan waktu
  (`BE@a437ae42 SchedulingManagement/Services/ShiftReplacementService.cs:387-410`).
- Petugas asal diambil dari `OriginalEmployeeId ?? WorkforceProfileId`
  (`ShiftReplacementService.cs:369`). Artinya, pada penggantian kedua, kolom "petugas asal"
  berisi **petugas rencana awal**, bukan petugas tepat sebelumnya.
- Koreksi mundur (`POST /{id}/retroactive-correct`) **menimpa** shift, jam mulai, dan jam
  selesai, lalu hanya menulis teks `OverrideReason`. Nilai lama **tidak disimpan** di tabel mana
  pun; `OldShiftId` hanya dikirim ke pemroses kehadiran (`ShiftReplacementService.cs:502-534`).
- Status periode dipaksa `Published` sesudah penggantian (`ShiftReplacementService.cs:382`).

**Contoh.** Roster 14 Februari: Budi diganti Tono pukul 19.00, lalu Tono diganti Andi pukul 20.00.
Baris riwayat kedua tertulis "Budi → Andi", sehingga "Tono → Andi" hanya bisa disimpulkan dari
urutan baris. Bila keesokan harinya Sari mengoreksi mundur shift Andi dari Malam ke Siang, tidak
ada baris riwayat yang mencatat jadwal lama "Malam".

**Kesimpulan.** `Extend`. Butir 3 `HRD-DEC-195` (nilai sebelum dan sesudah per perubahan) belum
terpenuhi untuk koreksi mundur, dan "petugas sebelum" perlu diturunkan dari urutan baris.

#### (2) Label status harian untuk satu scan dan nol scan, serta isi pratinjau tutup periode

**Fakta.**

- Satu scan → `IncompletePunch` dengan alasan "Hanya tercatat 1 tap"
  (`AttendanceManagement/Services/AttendanceProcessingService.cs:1050-1053`). Ini cocok dengan
  "Belum Lengkap" pada `HRD-DEC-197`; yang perlu hanya label tampilan.
- Nol scan pada hari kerja yang jadwalnya terbaca → **langsung `Absent`** begitu diproses, tanpa
  memeriksa apakah shift sudah selesai (`AttendanceProcessingService.cs:1041-1045`). Tidak ada
  status "Belum Hadir" maupun "Tidak Ada Scan" di daftar konstanta
  (`AttendanceManagement/Constants/AttendanceValueConstants.cs:36-48`).
- Tutup periode mengubah setiap `IncompletePunch` menjadi `Absent` **tanpa membaca** setelan
  `AutoConvertIncompleteToAbsent` (`AttendanceManagement/Services/AttendancePeriodService.cs:363-371`).
  Setelan itu, bersama `GracePeriodMinutes` dan `MaxBackdatedCorrectionDays`, tidak dibaca di
  mana pun selain service setelannya sendiri.
- Pratinjau tutup periode hanya memberi **jumlah** hari `IncompletePunch` dan peringatan, bukan
  daftar nama, tanggal, dan unit (`AttendancePeriodService.cs:677`, `:742`, `:767`).

**Contoh.** Shift Pagi Budi 10 Februari 07.00–14.00. Bila pemrosesan berjalan pukul 09.00 dan
Budi belum scan, rekapnya sudah `Absent`. Menurut `HRD-DEC-207`, seharusnya "Belum Hadir".

**Kesimpulan.** `Repair` untuk nol scan (`HRD-DEC-207`) dan setelan konversi yang diabaikan;
`Extend` untuk daftar pratinjau (`HRD-DEC-197` butir 3).

#### (3) Pemasangan scan ke shift dan scan di luar jendela

**Fakta.**

- Jendela pengambilan scan dihitung dari jadwal: mulai `max(240 menit, EarlyCheckInMinutes)`
  sebelum shift, berakhir `max(480 menit, LateCheckOutMinutes)` sesudah shift atau batas kerja
  maksimum kebijakan (`AttendanceProcessingService.cs:20-21`, `:1516-1545`). Angka 240 dan 480
  **ditulis mati** di kode.
- `EarlyCheckInMinutes` dan `LateCheckOutMinutes` berasal dari `MstGracePeriodPolicy` yang
  ditautkan lewat `MstAttendancePolicy` per jadwal kerja atau global
  (`AttendanceManagement/Services/AttendanceScheduleResolverService.cs:472-545`).
- Scan di luar jendela **tidak** diambil dan tetap berstatus terbuka di log mesin. Scan itu
  **tidak** dibuatkan baris `HrdAttendanceException`; daftar pengecualian yang ada hanya
  `ScheduleMismatch`, `ScheduleConflict`, `MissingCheckIn`, `MissingCheckOut`, `Absent`, `Late`,
  `EarlyLeave`, `ExcessiveWorkHours`, `OutsideGeofence`, dan `DuplicatePunch`
  (`AttendanceProcessingService.cs:1087-1240`).
- **Toleransi terlambat** dibaca dari `MstGracePeriodPolicy.LateCheckInGraceMinutes`, dengan
  angka cadangan 10 yang ditulis mati bila kebijakan kosong (`AttendanceProcessingService.cs:986`).
  Setelan `HrdAttendanceSetting.GracePeriodMinutes` **tidak dipakai** pemrosesan.

**Contoh.** Admin HRD mengubah toleransi di layar setelan dari 10 menjadi 15 menit. Rina scan
07.12 untuk shift 07.00. Pemrosesan tetap memakai 10 menit (cadangan kode), sehingga Rina
tercatat terlambat 12 menit.

**Kesimpulan.** `Conflict` antara mekanisme kebijakan per jadwal kerja di kode dan `HRD-DEC-196`/
`HRD-DEC-211` (satu angka untuk seluruh rumah sakit). `Repair` untuk angka tertanam dan scan di
luar jendela yang tidak masuk daftar pengecualian.

#### (4) Penempatan sementara dan pemeriksaan `B3`

**Fakta.**

- Penempatan pegawai ke unit disimpan di `WfpOrganizationAssignment` milik sub-modul 02, dengan
  jenis `Primary`, `Secondary`, `Acting`, **`Temporary`**, `Project`, `Functional`, tanggal
  berlaku, dan tautan surat perintah `PersonnelActionOrderId`
  (`WorkforceCore/Models/WfpOrganizationAssignment.cs:33-56`; API
  `WorkforceCore/Controllers/WfpOrganizationAssignmentController.cs:23`).
- "Isi pegawai otomatis" roster mengambil **semua** penempatan aktif pada unit periode, tanpa
  membedakan jenis (`SchedulingManagement/Services/RosterPeriodService.cs:457-466`).
- `RosterValidationService` **tidak** memuat pemeriksaan `B3`. Kode yang ada: `B1`, `B2`, `B4`,
  `B5`, `B6`, `B7`, `W1`, `W4`, `W5`; `W2` dan `W3` ada tanpa kode aturan
  (`SchedulingManagement/Services/RosterValidationService.cs:110-450`). `W6` dan `W7` juga tidak
  ada.
- `WfpWorkScheduleAssignment.IsTemporary` hanya dipakai resolver kehadiran untuk mendahulukan
  jadwal tetap sementara (`AttendanceScheduleResolverService.cs:346`), bukan untuk roster.

**Contoh.** Tono punya penempatan `Primary` di ICU dan `Temporary` di HCU 1–14 Maret. Isi pegawai
otomatis roster ICU Maret dan roster HCU Maret sama-sama memasukkan Tono, dan tidak ada
pemeriksaan yang menolak Tono dijadwalkan di ICU pada 5 Maret.

**Kesimpulan.** `Reuse with adapter` untuk data penempatan (`Temporary` sudah ada). `Missing`
untuk `B3`, dan aturan pengisian otomatis perlu menyaring unit asal selama penempatan sementara.

#### (5) Scan dari mesin tak terdaftar

**Fakta.**

- Untuk sumber `Device`, scan tanpa identitas mesin ditolak (kode 400). Scan dengan identitas
  mesin yang tidak ditemukan atau tidak aktif ditolak **dengan kode 404 dan tidak disimpan**
  (`AttendanceManagement/Services/AttendanceRawLogService.cs:771-830`).
- `MstAttendanceDevice` punya `DbSet` (`Repositories/ApplicationDbContext.cs:162`), tetapi
  **tidak punya controller** dan **tidak punya seeder**.
- Kiriman ganda dikenali lewat penanda `IsDuplicate` dan `EventHash`
  (`AttendanceRawLogService.cs:1301`; `Models/HrdAttendanceRawLog.cs:61`).

**Contoh.** Mesin GDB-01 belum terdaftar dan mengirim 50 scan. Kelima puluh scan ditolak kode 404
dan hilang dari server. Menurut `HRD-DEC-212`, scan itu seharusnya disimpan dan ditahan.

**Kesimpulan.** `Repair` untuk perlakuan scan mesin asing. `Missing` untuk API daftar mesin. Isi
tabel mesin di database `Unknown` karena tidak diperiksa (butuh izin eksekusi database).

### 16.3 Kontrak Bukti Kemampuan — 12 menu `PRD-HRD-PK-001`

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-CAP-67` | M03 susun, isi otomatis, salin, pola, periksa, terbit, tutup roster | HR (`S-B4`) | `BE@a437ae42 SchedulingManagement/Controllers/RosterPeriodController.cs:34-176` (hak akses `RosterPeriod`); `FE@6a5658269 src/app/hr/scheduling/roster/page.jsx`, `src/lib/services/hr/roster-scheduling-service.js` | `Extend` | Alur sesuai `HRD-DEC-194`; pemeriksaan belum lengkap (lihat `HRD-CAP-68`) | Sedang |
| `HRD-CAP-68` | Pemeriksaan roster `B1`–`B8`, `W1`–`W7` | HR (`S-B4`) | `RosterValidationService.cs:110-450` | `Extend` | Tidak ada `B3`, `W6`, `W7`, `B8`; `W2`/`W3` tanpa kode aturan | **Tinggi** — pegawai mutasi atau tugas belajar bisa terjadwal |
| `HRD-CAP-69` | Ganti Petugas langsung dengan pemeriksaan sama seperti terbit | HR (`S-B4`) | `ShiftReplacementService.cs:269-442`; `ShiftReplacementController.cs` `POST /{id}/replace` (`ShiftReplacement : Update`) | `Repair` | Tidak memanggil `RosterValidationService`; hanya memeriksa tabrakan, cuti, siap kerja, kontrak; **status kesiapan gagal dibaca dilewati** (seharusnya `B6` menolak); tanpa `B3`, `B7`, `W1`–`W7` | **Tinggi** — keselamatan pasien (`B7`) |
| `HRD-CAP-70` | Riwayat Revisi per perubahan (`HRD-DEC-195`) | HR (`S-B4`) | `Models/TrxShiftReplacement.cs:13-73`; `ShiftReplacementService.cs:369`, `:502-534` | `Extend` | Koreksi mundur tidak menyimpan nilai lama; "petugas asal" = rencana awal | Sedang |
| `HRD-CAP-71` | Tukar shift dan ubah jadwal oleh pegawai | HR (`S-B4`) | `Areas/SelfServices/HumanResource/Controllers/ShiftSwapSelfServiceController.cs`, `ScheduleChangeSelfServiceController.cs`; `FE@6a5658269 src/lib/services/hr/employee-self-service.js:92-107` | `Reuse with adapter` | Tanpa uji otomatis; jalur persetujuan belum diuji ujung ke ujung pada scan ini | Rendah |
| `HRD-CAP-72` | Siaga, panggilan, cadangan (`HRD-DEC-198`, `204`, `205`) | HR (`S-B4`) | `OnCallAssignmentService.cs:391-462` (`POST /{id}/call-in`, `OnCallAssignment : Update`); `Models/TrxOnCallAssignment.cs:25-68`; `FE@6a5658269 src/app/hr/scheduling/on-call/page.jsx` | `Extend` | Satu penugasan hanya menyimpan satu panggilan (ditimpa), dicatat sekaligus dengan jam tiba; belum ada catatan panggilan, status jawab, cadangan, batas jawab per jenis siaga. Status tetap `Active` sudah sesuai `HRD-DEC-103` | Sedang |
| `HRD-CAP-73` | Status harian: Belum Hadir, Tidak Ada Scan, Belum Lengkap, hari libur | HR (`S-B1`) | `AttendanceProcessingService.cs:1024-1070`; `AttendanceScheduleResolverService.cs:205-221`, `:447-469` | `Repair` | Nol scan langsung `Absent` (`HRD-DEC-207`); hari libur dipasang juga untuk pegawai roster sehingga pegawai roster yang tidak datang berstatus `Holiday` (`HRD-DEC-210` butir 3) | **Tinggi** — tuduhan alpa palsu dan alpa yang tersamar |
| `HRD-CAP-74` | Jendela scan satu untuk RS + scan di luar jendela masuk pengecualian (`HRD-DEC-211`) | HR (`S-B1`) | `AttendanceProcessingService.cs:20-21`, `:676-712`, `:1516-1545` | `Repair` | Angka 240/480 tertanam; scan di luar jendela tidak masuk daftar pengecualian | Sedang |
| `HRD-CAP-75` | Toleransi terlambat tunggal (`HRD-DEC-196`) | HR (`S-B1`) | `AttendanceProcessingService.cs:986`; `AttendanceScheduleResolverService.cs:472-545`; `AttendanceSettingService.cs:184-193` | `Conflict` | Kode membaca kebijakan per jadwal kerja (`MstGracePeriodPolicy`) dengan cadangan 10; setelan tunggal tidak dipakai | **Tinggi** — angka di layar setelan tidak berpengaruh |
| `HRD-CAP-76` | Tutup periode, pratinjau, buka kembali (`HRD-DEC-197`, `199`) | HR (`S-B1`) | `AttendancePeriodController.cs:125-188` (`AttendancePeriod : Read/Close/Reopen`); `AttendancePeriodService.cs:297-415`, `:634-767`; `FE@6a5658269 src/lib/services/hr/attendance-management-service.js` (`getPeriodClosePreview`, `closePeriod`, `reopenPeriod`) | `Extend` | Pratinjau hanya jumlah; konversi mengabaikan setelan; belum menampilkan scan mesin tertahan (`HRD-ASM-81`) | Sedang |
| `HRD-CAP-77` | Log mesin dan daftar mesin (`HRD-DEC-212`) | HR + IT | `AttendanceRawLogController.cs:146-239` (`AttendanceRawLog : Create/Update`); `AttendanceRawLogService.cs:771-830` | `Repair` | Scan mesin asing ditolak 404 dan hilang; tidak ada API daftar mesin; isi tabel mesin `Unknown` | **Tinggi** — scan pegawai bisa hilang tanpa jejak |
| `HRD-CAP-78` | Koreksi presensi dan kejadian gangguan mesin (`HRD-DEC-202`) | HR (`S-B1`) | `AttendanceCorrectionController.cs`, `AttendanceCorrectionMonitoringController.cs`; ESS `src/app/self-services/human-resource/attendance-correction/page.jsx`; koreksi atas nama pegawai `attendance-management-service.js` `createOnBehalfCorrection` | `Extend` | Koreksi ada; konsep kejadian gangguan belum ada | Sedang |
| `HRD-CAP-79` | Dispensasi (`HRD-DEC-200`) | HR (`S-B1`) | Tidak ditemukan konsep ini (`HRD-TF-065`) | `Missing` | Seluruhnya baru | Rendah |
| `HRD-CAP-80` | Tugas belajar memblokir roster (`HRD-DEC-201`) | HR | Tidak ditemukan konsep ini (`HRD-TF-065`) | `Missing` | Seluruhnya baru, termasuk `B8` | Sedang |
| `HRD-CAP-81` | Penugasan tambahan (`HRD-DEC-213`) | HR (02 + `S-B4`) | `WorkforceCore/Models/WfpOrganizationAssignment.cs:33-56`; `RosterPeriodService.cs:457-466` | `Reuse with adapter` | Pakai jenis `Temporary`; isi otomatis perlu menyaring unit asal; `B3` belum ada | Sedang |
| `HRD-CAP-82` | Master hari libur (`HRD-DEC-210`) | HR | `MasterData/AttendanceAndSchedule/Models/MstHoliday.cs`; `ApplicationDbContext.cs:161`; dibaca resolver `AttendanceScheduleResolverService.cs:447-469` | `Extend` | Tidak ada API, layar, maupun seeder | Sedang |
| `HRD-CAP-83` | Laporan dan ekspor M12 (`HRD-DEC-206`) | HR | Tidak ditemukan endpoint laporan/ekspor (`HRD-TF-061`) | `Missing` | Seluruhnya baru, termasuk log ekspor | Sedang |
| `HRD-CAP-84` | Dashboard M01 (`HRD-DEC-208`) | HR | Ringkasan per modul ada: `AttendanceDailyController` `GET /summary`, `RosterPeriodController` `GET /summary`; tidak ada dashboard gabungan | `Extend` | Perlu agregasi per cakupan unit | Sedang |
| `HRD-CAP-85` | Jadwal Saya (`HRD-DEC-209`) | HR (ESS) | `FE@6a5658269 src/lib/services/hr/employee-self-service.js:88` melempar galat; layar `src/app/self-services/human-resource/schedule/page.jsx` ada | `Missing` | Endpoint server belum ada; layar menunggu | Sedang |
| `HRD-CAP-86` | M08 cuti, izin pulang cepat, dinas luar | HR (`S-B1`, `S-B2`) | Cuti: `LeaveManagement/Controllers/**` 17 controller + `src/app/hr/leave/**`; izin pulang cepat `EarlyLeavePermissionController.cs`; dinas luar `OfficialDutyRequestController.cs` | `Extend` | Izin pulang cepat dan dinas luar belum punya layar frontend | Sedang |
| `HRD-CAP-87` | M09 lembur | HR (`S-B3`) | `OvertimeManagement/Controllers/**` 17 controller; `FE@6a5658269 src/components/view/hr/overtime/overtime-approvals-view.jsx:13-38` | `Extend` | Layar persetujuan memakai data contoh tertanam dan `currentUserId = "USER-SUPERVISOR-01"` | **Tinggi** — layar tampak bekerja tetapi bukan data nyata |
| `HRD-CAP-88` | M10 kebutuhan tenaga | HR (`S-D1`) | `WorkforcePlanning/Controllers/HrdStaffingGapController.cs`, `HrdStaffingStandardController.cs`; pencarian `workforce-planning/staffing-` pada `FE@6a5658269 src/` tanpa hasil | `Extend` | Backend ada, layar belum | Sedang |
| `HRD-CAP-89` | Grup menu 12 menu (`HRD-DEC-214`) | HR (FE) | `FE@6a5658269 src/utils/menu-sidebar/menu-items.jsx:675-947` (entri `/hr/attendance`, `/hr/scheduling/*`, `/hr/leave/*`, `/hr/overtime/*`) | `Extend` | Entri tersebar di beberapa grup; perlu disusun ulang tanpa mengubah route | Rendah |
| `HRD-CAP-90` | Pemberitahuan perubahan jadwal dan siaga | Shared HR | `ShiftReplacementService.cs:414-429` (teks saja); `BE-HRD-314` 🟡 | `Extend` | Bergantung kanal lonceng `BE-HRD-314` | Sedang |
| `HRD-CAP-91` | Bukti pengujian penjadwalan dan kehadiran | HR | `BE@a437ae42 Tests/` tanpa proyek uji HR; `FE@6a5658269 tests/unit/` tanpa uji roster, siaga, atau kehadiran | `Missing` | Tidak ada jaring pengaman untuk perbaikan `HRD-CAP-69`, `73`, `75`, `77` | **Tinggi** |

**Rekap status (25 baris):** `Reuse with adapter` 2, `Extend` 13, `Repair` 4, `Conflict` 1, `Missing` 5,
`Ready to reuse` 0, `Unknown` 0 (isi database dicatat terpisah pada 16.6).

### 16.4 Temuan yang perlu tindakan

| No | Temuan | Dampak di rumah sakit | Keputusan terkait |
| ---: | --- | --- | --- |
| 1 | Ganti Petugas melewati pemeriksaan bila status kesiapan gagal dibaca, dan tidak memeriksa `B7` | Perawat berstatus Didampingi bisa dijadikan pengganti di shift tanpa perawat Mandiri | `HRD-DEC-099` butir 6, `HRD-DEC-100`, `HRD-DEC-195` |
| 2 | Nol scan langsung `Absent` saat diproses | Kepala Unit melihat pegawai alpa pada pukul 09.00 padahal shift belum selesai | `HRD-DEC-207` |
| 3 | Hari libur dipasang untuk pegawai roster | Perawat ICU yang tidak datang pada 17 Agustus tercatat `Holiday`, bukan dicek kehadirannya | `HRD-DEC-210` |
| 4 | Toleransi, setelan konversi, dan batas koreksi di layar setelan tidak dibaca pemrosesan | HRD mengubah angka di layar tetapi hasil hitung tidak berubah | `HRD-DEC-196`, `HRD-DEC-197` |
| 5 | Scan mesin asing ditolak dan hilang | Scan pegawai Gedung B hari pertama mesin baru dipasang tidak bisa dipulihkan | `HRD-DEC-212` |
| 6 | Koreksi mundur tanpa nilai lama | Auditor tidak bisa melihat jadwal sebelum koreksi | `HRD-DEC-195` |
| 7 | Layar persetujuan lembur berisi data contoh | Supervisor mengira persetujuan tersimpan padahal tidak | Di luar scope bagian 52; dicatat untuk `S-B3` |

### 16.5 Kontrak as-is yang disentuh

Kode status yang mungkin muncul pada tabel di bawah: 400 isian salah; 403 tidak punya hak akses
atau periode terkunci; 404 data tidak ditemukan (untuk log mesin: mesin tidak terdaftar); 409
bentrok data (mis. satu identitas mesin cocok ke dua mesin aktif); 422 aturan bisnis menolak
(mis. pengganti sedang cuti).

#### HR Scheduling - Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/populate-staff` | Mengisi daftar pegawai roster dari penempatan unit | `RosterPeriod : Create` | - | Ringkasan pegawai terisi |
| `POST` | `/{id}/validate` | Menjalankan pemeriksaan blokir dan peringatan | `RosterPeriod : Update` | - | Hasil pemeriksaan per pegawai |
| `POST` | `/{id}/publish` | Menerbitkan roster (memeriksa ulang) | `RosterPeriod : Update` | Alasan peringatan | Detail periode |
| `POST` | `/{id}/close` | Menutup roster | `RosterPeriod : Update` | - | Detail periode |

#### HR Scheduling - Shift Assignment & Replacement

Base URL: `api/v1/corporate/human-resource/scheduling-management/shift-replacements`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/replace` | Ganti Petugas langsung pada roster terbit | `ShiftReplacement : Update` | Pengganti, kategori alasan, catatan | `DirectShiftReplacementResponse` |
| `POST` | `/{id}/retroactive-correct` | Koreksi mundur jadwal | `ShiftReplacement : Update` | Shift baru, kategori alasan, catatan | `RetroactiveShiftCorrectionResponse` |

#### HR Scheduling - On-Call Management

Base URL: `api/v1/corporate/human-resource/scheduling-management/on-call-assignments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/call-in` | Mencatat satu pemanggilan beserta jam tiba | `OnCallAssignment : Update` | Jam panggil, jam tiba | `OnCallCallInResponse` |
| `GET` | `/active-doctors` | Daftar dokter siaga aktif (dibaca Laboratorium, `INT-HR-39`) | `OnCallAssignment : Read` | Query | Daftar siaga aktif |

#### HR Attendance - Period Management

Base URL: `api/v1/corporate/human-resource/attendance/periods`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/close-preview` | Pratinjau sebelum tutup | `AttendancePeriod : Read` | - | `AttendancePeriodClosePreviewResponse` (jumlah, bukan daftar) |
| `POST` | `/{id}/close` | Menutup periode dan mengubah `IncompletePunch` → `Absent` | `AttendancePeriod : Close` | Catatan | `AttendancePeriodActionResponse` |
| `POST` | `/{id}/reopen` | Membuka kembali periode darurat | `AttendancePeriod : Reopen` | Alasan | `AttendancePeriodActionResponse` |

#### HR Attendance - Raw Logs

Base URL: `api/v1/corporate/human-resource/attendance/raw-logs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menerima satu scan | `AttendanceRawLog : Create` | Data scan | Detail log |
| `POST` | `/batch` | Menerima banyak scan sekaligus | `AttendanceRawLog : Create` | Daftar scan | Hasil per baris |
| `POST` | `/{id}/retry` | Mencocokkan ulang identitas pegawai | `AttendanceRawLog : Update` | - | Detail log |

#### HR Attendance - Settings

Base URL: `api/v1/corporate/human-resource/attendance-management/settings`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca setelan kehadiran | `AttendanceSetting : Read` | - | Setelan |
| `PUT` | `/` | Mengubah toleransi, batas koreksi, lampiran, konversi | `AttendanceSetting : Update` | Setelan | Setelan |

Catatan: nilai `GracePeriodMinutes`, `MaxBackdatedCorrectionDays`, dan
`AutoConvertIncompleteToAbsent` tersimpan lewat endpoint ini, tetapi tidak dibaca pemrosesan
(16.2 butir 2 dan 3).

### 16.6 Ketidakcocokan dan hal yang belum diketahui

| Jenis | Isi | Bukti |
| --- | --- | --- |
| Dokumen basi | `roadmap/frontend-unfinished-work.md` (audit `FE@d09dc74`) mencatat `FE-HRD-088`–`101` belum dikerjakan, padahal `FE@6a5658269` sudah punya monitoring harian, drawer log mesin, pratinjau/tutup/buka kembali periode, koreksi atas nama pegawai, dan setelan | `src/components/view/hr/attendance/attendance-monitoring-view.jsx`, `RawPunchAuditDrawer.jsx`, `attendance-settings-view.jsx`; commit `c8a07c169` "gelombang 1" |
| Frontend menunggu backend | Jadwal Saya melempar galat karena endpoint belum ada | `employee-self-service.js:88` |
| `Unknown` — isi database | Apakah `MstAttendancePolicy`, `MstGracePeriodPolicy`, `MstAttendanceDevice`, dan `MstHoliday` sudah berisi data di devYoga atau server dev. Menentukan apakah toleransi sekarang 10 (cadangan kode) atau angka kebijakan, dan apakah scan sumber `Device` selama ini selalu ditolak | Tidak diperiksa; butuh izin eksekusi database baca-saja |

### 16.7 Pertanyaan penutup untuk `/grill-me`

Pertanyaan berikut tidak dijawab di sini. Semuanya menunggu keputusan pengguna.

| ID | Pertanyaan | Mengapa perlu diputuskan | Memblokir |
| --- | --- | --- | --- |
| `HRD-CQ-01` | Kode sudah punya kebijakan toleransi dan jendela per jadwal kerja (`MstAttendancePolicy` + `MstGracePeriodPolicy`), sedangkan `HRD-DEC-196`/`211` memilih satu angka di setelan. Mekanisme per jadwal kerja dihentikan pemakaiannya (setelan menjadi satu-satunya sumber), atau setelan menjadi nilai bawaan yang boleh ditimpa kebijakan per jadwal? | `HRD-CAP-75` `Conflict`; pilihan kedua berarti meninjau ulang `HRD-DEC-196` | `DESIGN` |
| `HRD-CQ-02` | Setelan `AutoConvertIncompleteToAbsent` saat ini diabaikan. Bila HRD mematikannya, apa yang terjadi pada hari "Belum Lengkap" saat periode ditutup: tetap `IncompletePunch` dan diteruskan ke payroll, atau periode tidak boleh ditutup? Atau setelan ini dihapus dan konversi selalu terjadi? | `HRD-DEC-197` butir 4 menyebut "selama setelan menyala" tanpa menetapkan perilaku saat mati | `DESIGN` |
| `HRD-CQ-03` | Apakah koreksi mundur jadwal (`HRD-DEC-102`) juga wajib masuk Riwayat Revisi dengan nilai sebelum dan sesudah, sama seperti Ganti Petugas? | `HRD-DEC-195` butir 1 menyebut Ganti Petugas, tambah, dan hapus shift, tanpa menyebut koreksi mundur | `DESIGN` |
| `HRD-CQ-04` | Bolehkah agen membaca isi tabel master kehadiran (`MstAttendancePolicy`, `MstGracePeriodPolicy`, `MstAttendanceDevice`, `MstHoliday`) di devYoga lewat kueri baca-saja? | Menutup `Unknown` 16.6 sebelum desain memilih jalur migrasi data | `DESIGN` (perlu izin, bukan keputusan bisnis) |

### 16.8 Pemicu impact scan berikutnya

- Ada commit baru pada `SchedulingManagement/**`, `AttendanceManagement/**`, atau
  `WorkforceCore/Models/WfpOrganizationAssignment.cs`.
- Ada migration baru yang menyentuh tabel `Trx*Roster*`, `TrxShift*`, `TrxOnCallAssignment`,
  `HrdAttendance*`, atau `Mst*Attendance*`.
- Ada perubahan pada `src/lib/services/hr/attendance-management-service.js`,
  `roster-scheduling-service.js`, `on-call-service.js`, `employee-self-service.js`, atau
  `src/utils/menu-sidebar/menu-items.jsx`.
- Jawaban `HRD-CQ-01` s.d. `HRD-CQ-04`.

### 16.9 Yang sengaja tidak dilakukan pada scan ini

- Tidak ada build, tidak ada uji, tidak ada eksekusi database.
- Tidak ada desain target, tabel baru, maupun endpoint baru. Kebutuhan yang `Missing` hanya
  dicatat.
- Aturan internal cuti (`S-B2`), lembur (`S-B3`), dan kebutuhan tenaga (`S-D1`) tidak diaudit
  ulang; hanya titik sentuhnya dengan 12 menu.
- Baris `HRD-CAP-01` s.d. `HRD-CAP-66` tidak diklasifikasi ulang. Baris yang tumpang tindih
  (`HRD-CAP-04`, `05`, `07`, `08`, `09`) untuk area 12 menu digantikan bacaannya oleh
  `HRD-CAP-67` s.d. `HRD-CAP-91`.
