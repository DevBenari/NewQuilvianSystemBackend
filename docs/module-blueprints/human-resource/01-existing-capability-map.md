# Human Resource — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Capability-map revision | `1.1` — revisi `1.0` 27 Agustus 2026; revisi `1.1` hari yang sama **menarik `HRD-TF-001`** yang terbukti temuan palsu, dan menaikkan `HRD-CAP-26` dari `REPAIR` menjadi `READY TO REUSE` |
| Status | `source-audited`. Dokumen ini **belum** menyatakan modul siap dibangun dan **belum** menyatakan siap produksi |
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
