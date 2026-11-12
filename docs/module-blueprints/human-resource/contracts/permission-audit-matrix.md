# Human Resource — Matriks Hak Akses dan Jejak Audit

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/permission-audit-matrix.md` |
| `contract_version` | `v5` |
| `last_changed_in` | `v7` — bagian 11 (`S-D1`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v5` |
| Status | `draft` — **belum** `approved` |
| Owner | Pemilik keamanan bersama technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Hanya bagian 11** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `contracts/api-contract.md` `v1`; `data/data-dictionary.md` `v1` |
| `input_hash` — decision log | `da1d74f2e417fd31815cf69b401f390277c361e404d38579bcfa75e0f125f083` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Dampak kompatibilitas | Tidak ada butir hak akses yang dihapus atau diganti namanya |

---

## 0. Apa yang **tidak** ada di dokumen ini, dan mengapa

Dokumen ini **tidak** memuat tabel seluruh endpoint. Pemetaan endpoint ke hak akses sudah
dipegang kolom **Hak akses** pada [`contracts/api-contract.md`](./api-contract.md), dan dua hal
berikut **dihitung**, bukan ditulis ulang:

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| String atribut hak akses | `[AccessPermission("<Resource>", "<Action>")]`, disalin persis dari kolom `Hak akses` pada kontrak API |
| Status pencatatan logger | Konvensi project: `GET` **tidak** dicatat; selain `GET` **dicatat** |

Penyimpangan dari kedua turunan itu ditulis sebagai **daftar pengecualian bernama** pada bagian 6
dan 8 — bukan dengan mendaftar ulang seluruh 1.316 endpoint agar penyimpangannya kelihatan.

Yang justru ada di sini adalah bagian yang **tidak dapat** diturunkan dari daftar endpoint.

---

## 1. Cara kerja hak akses di repository ini

### 1.1 Tiga lapisan yang bekerja bersamaan

| Lapisan | Bentuk | Yang dijaganya | Bukti |
| --- | --- | --- | --- |
| Autentikasi | `[Authorize]` pada kelas controller | Pengguna sudah masuk | **150 dari 150** controller HR memilikinya; tidak ada `[AllowAnonymous]` di seluruh area HR `[EXISTING]` |
| Hak akses per aksi | `[AccessController]` pada kelas; `[AccessAction]` dan `[AccessPermission("Resource", "Action")]` pada setiap endpoint | Pengguna berhak melakukan aksi itu pada sumber daya itu | 148 dari 150 controller mengikuti pola ini |
| Kepemilikan dan penugasan | Pemeriksaan di dalam service | Pengguna berhak atas **baris data yang ini**, bukan sekadar berhak atas jenis aksinya | Lihat bagian 4 |

**Ketiganya wajib.** Melewatkan lapisan ketiga adalah kesalahan yang paling sering terjadi dan
paling sulit terlihat: seseorang yang memegang butir hak akses menyetujui bisa menyetujui
pengajuan **siapa pun** bila tidak ada pemeriksaan penugasan.

### 1.2 Bentuk penulisan yang harus diikuti

```csharp
[ApiController, Authorize]
[AccessController]
[Route("api/v1/corporate/human-resource/attendance/periods")]
[Tags("Corporate / Human Resource / Attendance Management / Attendance Period")]
public class AttendancePeriodController : ControllerBase
{
    [HttpPost("{id:guid}/close")]
    [AccessAction]
    [AccessPermission("AttendancePeriod", "Close")]
    public async Task<IActionResult> Close(...)
}
```

**Peringatan yang lahir dari kesalahan nyata.** Audit sebelumnya sempat menyatakan tiga
controller pelatihan tidak memiliki `[Authorize]`, sehingga 27 endpoint dianggap terbuka tanpa
autentikasi. **Pernyataan itu keliru dan sudah ditarik.** Ketiganya memiliki `[Authorize]`,
ditulis **menyatu** dengan `[ApiController]` pada satu baris. Pola pencarian yang dipakai saat
audit mensyaratkan kurung siku persis di depan kata `Authorize`, sehingga bentuk menyatu tidak
tertangkap.

Pelajarannya untuk audit berikutnya: **cari dengan kata polos, bukan dengan pola yang
mensyaratkan tanda baca tertentu.**

### 1.3 Bentuk nama butir hak akses

| Bagian | Aturan | Contoh |
| --- | --- | --- |
| Resource | Nama sumber daya dalam PascalCase, tanpa awalan modul | `AttendancePeriod`, `LeaveBalance`, `OvertimePlan` |
| Resource layanan mandiri | Diawali `My` untuk menandai kepemilikan pribadi | `MyLeaveRequest`, `MyOvertime`, `MyShiftSwap` |
| Action | Kata kerja dalam PascalCase | `Read`, `Create`, `Update`, `Delete`, `Submit`, `Approve`, `Close`, `Reopen`, `Execute`, `Rollback` |

**Aksi standar** yang berlaku di hampir seluruh sumber daya: `Read`, `Create`, `Update`,
`Delete`.

**Aksi khas** yang muncul karena alur bisnisnya memang menuntut: `Submit`, `Cancel`, `Approve`,
`Reject`, `NeedRevision`, `RequestRevision`, `Return`, `Verify`, `Acknowledge`, `Close`,
`Reopen`, `Process`, `Execute`, `Retry`, `Repair`, `Rollback`, `Reverse`, `Post`, `Preview`,
`Reconcile`, `Synchronize`, `Apply`, `Validate`, `Publish`, `Activate`, `Revoke`, `Delegate`,
`Expire`, `Classify`.

---

## 2. Peta peran rumah sakit ke butir hak akses

### 2.1 Peringatan yang harus dibaca lebih dulu

**Peta ini adalah usulan, bukan bukti.** Audit source membuktikan dua hal yang membuat peta ini
belum dapat dikunci:

1. **Peran seperti `Supervisor`, `Manager`, `HrAdmin`, dan `Payroll` pada domain lembur terbukti
   tidak terhubung ke pemeriksaan identitas apa pun.** Keempatnya hanya nilai bawaan pada sebuah
   kolom. Penegakan yang nyata adalah butir hak akses generik per aksi, yang **terputus** dari
   kosakata peran itu `[EXISTING]`.
2. **Belum ada dokumen yang memetakan peran aplikasi yang benar-benar ada ke butir hak akses
   HR.** `HRD-Q-33` bergeser dari "bagaimana pemetaannya" menjadi **"peta ini belum dibangun"**.

Karena itu, tabel di bawah adalah **usulan awal untuk didiskusikan pemilik produk bersama
keamanan**, bukan konfigurasi yang boleh langsung dipasang.

### 2.2 Usulan peta peran

| Peran rumah sakit | Butir hak akses yang diusulkan | Catatan |
| --- | --- | --- |
| **Pegawai** | Seluruh butir berawalan `My*` dengan aksi `Read`, `Create`, `Update`, `Submit`, `Cancel`, `Delete` | Kepemilikan tetap dijaga lapisan ketiga: identitas diturunkan dari pengguna yang masuk, bukan dari isian pemanggil |
| **Atasan atau kepala unit** | Seluruh butir `My*` milik dirinya sendiri; ditambah `ApprovalInbox : Read`, `: Approve`, `: Reject`, `: RequestRevision`, `: Return`, `: Verify`, `: Acknowledge`; `ApprovalDelegation : Read`, `: Create`, `: Update`, `: Submit`, `: Revoke`; `LeaveTeamCalendar : Read` | Gate nyata tetap penugasan, bukan peran. Lihat bagian 4.1 |
| **HR Admin** | Seluruh master data HR dengan aksi `Read`, `Create`, `Update`, `Delete`; seluruh butir `Wfp*` administrasi kepegawaian; `AttendanceDaily : Read`; `AttendanceRawLog : Read`, `: Create`, `: Update`; `AttendanceCorrection : Read`, `: Apply`, `: CreateOnBehalf`; `LeaveBalance : Read`; `LeaveAdjustment : Read`, `: Create`, `: Update`, `: Submit`; `OvertimePlan` lengkap; `ScheduleChangeRequest`, `ShiftSwapRequest`, `WorkScheduleAssignment`, `RosterPeriod`, `ShiftAssignment`; `WorkforceTrainingRecord`, `WorkforceCompetencyAssessment`, `PerformanceReview`, `ResignationRequest`, `OffboardingChecklist` | **Kecuali** nominal gaji pada daftar lintas-pegawai — `[OPEN]` `HRD-Q-20` |
| **HR Manager** | Seluruh butir HR Admin; ditambah `AttendancePeriod : Reopen`, `OvertimePeriod : Reopen`, `LeaveExecution : Reverse`, `LeaveRecall : OverrideAcknowledgement`, `LeaveAdjustment : Post`, `: Reverse`, `AttendanceException : Classify` | Aksi yang membuka kembali atau membalikkan sesuatu yang sudah selesai **MUST** dibatasi lebih ketat daripada aksi biasa |
| **Petugas payroll** | `AttendancePeriod : Read`, `: Close`, `: Process`; `OvertimePeriod : Read`, `: Close`, `: Validate`; `AttendancePayrollHandoff` lengkap; `OvertimePayrollHandoff` lengkap; `LeavePayrollIntegration` lengkap; `AttendanceDaily : Read`; `LeaveBalance : Read` | **Tidak ada** butir hak akses yang mengubah status pembayaran — memang tidak ada endpointnya `[DECISION]` `HRD-DEC-009` |
| **Kepala unit penjadwalan** | `RosterPeriod` lengkap; `RosterAssignment` lengkap; `ShiftAssignment` lengkap; `ShiftReplacement`; `EmergencyStaffing`; `OnCallAssignment` | Seluruhnya **Rencana** — endpointnya belum ada |
| **Auditor** | Seluruh butir dengan aksi `Read` saja | **Tidak boleh** memegang aksi apa pun selain `Read` |

### 2.3 Pemetaan peran fungsional ke peran aplikasi — `HRD-DEC-032`

| Field | Isi |
| --- | --- |
| Dasar | `HRD-DEC-032` |
| Status | **`SECURITY_APPROVED`** — co-sign 2026-08-30 |
| Disetujui | Pemilik teknis dan produk, 2026-08-30 |
| Co-approver | `Project final decision authority — Security` — **sudah menandatangani**, 2026-08-30 |

**Aturan yang paling menentukan bagian ini:** nama peran fungsional pada bagian 2.2 **MUST NOT**
dianggap sama dengan peran aplikasi pada Identity. Bagian 2.2 adalah **baseline fungsional**;
bagian ini adalah pemetaannya ke kenyataan.

#### 2.3.1 Keadaan infrastruktur peran hari ini

Hasil audit read-only:

| Yang diperiksa | Temuan | Bukti |
| --- | --- | --- |
| Model peran | `ApplicationRole : IdentityRole<Guid>`, punya penanda `IsSystemRole` | `Models/ApplicationRole.cs` |
| Peran yang di-seed | **Hanya `SuperAdmin` dan `User`** | `Seeders/SuperAdminSeeder.cs` baris 9–10 |
| Peran HR | **Tidak ada satu pun.** Peran lain dibuat administrator saat aplikasi berjalan | — |
| Katalog butir hak akses | **Dibangkitkan mesin** dari atribut `[AccessController]` dan `[AccessAction]` | `Seeders/AccessMenuSeeder.cs` |
| Pengikat peran ke aksi | `SysAccessPolicy`, dibuat saat berjalan | `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` — `POST /policies`, `POST /policies/copy` |
| Urutan penegakan | Pengguna → peran Identity → kebijakan akses → aksi | `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs` |

**Kesimpulan yang jujur, dan perlu dibaca apa adanya:**

- **Sisi hak akses sudah lengkap.** 152 controller HR memakai `[AccessPermission("<Resource>", "<Action>")]`, dan katalog butirnya dibangkitkan mesin. Tidak ada yang perlu dikarang.
- **Sisi peran belum ada.** Tidak satu pun peran fungsional HR punya padanan di Identity hari ini.

#### 2.3.2 Tabel pemetaan

| Peran fungsional | Peran aplikasi yang benar-benar ada | Cakupan butir hak akses | Status pemetaan |
| --- | --- | --- | --- |
| **Employee** | **Tidak ditemukan** | Seluruh butir berawalan `My*`, aksi `Read`, `Create`, `Update`, `Submit`, `Cancel`, `Delete` | **`MAPPING_REQUIRED`** |
| **Supervisor / Manager** | **Tidak ditemukan** | Butir `My*` miliknya sendiri; `ApprovalInbox : Read`, `: Approve`, `: Reject`, `: RequestRevision`, `: Return`; `AttendanceException : Classify` | **`MAPPING_REQUIRED`** |
| **HR Admin** | **Tidak ditemukan** | Master data HR; butir `Wfp*` administrasi kepegawaian; `AttendanceDaily : Read`; `AttendanceRawLog : Read`, `: Create`, `: Update`; `AttendanceCorrection : Read`, `: Apply`, `: CreateOnBehalf`; `LeaveBalance : Read`; `LeaveAdjustment` lengkap; `OvertimePlan` lengkap; penjadwalan | **`MAPPING_REQUIRED`** |
| **HR Manager** | **Tidak ditemukan** | Seluruh butir HR Admin, ditambah persetujuan `T8` perubahan penempatan dan remunerasi, ditambah butir sensitif nominal gaji | **`MAPPING_REQUIRED`** |
| **Payroll Officer** | **Tidak ditemukan** | `AttendancePeriod : Create`, `: Close`, `: Reopen`, `: Cancel`; `AttendancePayrollHandoff : Execute`, `: Rollback`; kesiapan payroll sisi HR | **`MAPPING_REQUIRED`** |
| **Scheduling Lead** | **Tidak ditemukan** | `RosterPeriod`, `ShiftAssignment`, `ScheduleChangeRequest`, `ShiftSwapRequest` — seluruh aksi | **`MAPPING_REQUIRED`** |
| **Auditor** | **Tidak ditemukan** | Hanya aksi `Read` lintas domain HR. **MUST NOT** memegang butir sensitif nominal gaji tanpa keputusan tersendiri | **`MAPPING_REQUIRED`** |
| — | `SuperAdmin` | **Melewati seluruh pemeriksaan** kecuali aksi bertanda khusus sistem | `EXISTS` — bukan peran HR; **MUST NOT** dipakai sebagai pengganti peran HR mana pun |
| — | `User` | Peran bawaan tanpa kebijakan akses HR | `EXISTS` — tidak mencukupi untuk pekerjaan HR apa pun |

#### 2.3.3 Aturan yang mengikat pemetaan ini

| Larangan | Sebabnya |
| --- | --- |
| **MUST NOT** membuat peran baru pada source aplikasi dari pekerjaan blueprint | Pembuatan peran adalah tindakan administrator pada aplikasi yang berjalan, bukan perubahan source |
| **MUST NOT** mengarang nama peran aplikasi | Peta yang menunjuk peran tidak ada adalah peta yang tidak menjaga apa pun |
| **MUST NOT** memakai `SuperAdmin` sebagai pengganti peran HR | `SuperAdmin` melewati seluruh pemeriksaan. Memakainya berarti meniadakan matriks ini |
| **MUST NOT** menutup `MAPPING_REQUIRED` tanpa co-sign keamanan | `HRD-DEC-032` menetapkan keamanan sebagai co-approver |

**Yang harus terjadi sebelum baris `MAPPING_REQUIRED` dapat ditutup:** administrator membuat
peran aplikasi yang bersesuaian, lalu mengikatnya ke butir hak akses lewat kebijakan akses. Itu
pekerjaan konfigurasi pada aplikasi berjalan, **bukan** task backend maupun frontend.

### 2.4 Pemisahan peran pada perubahan penempatan dan remunerasi — `HRD-DEC-031`

| Field | Isi |
| --- | --- |
| Dasar | `HRD-DEC-031` |
| Status | `approved` |
| Berlaku pada | Penetapan gaji, penempatan organisasi, penempatan jabatan, penetapan atasan |

| Aturan | Bunyi | Keadaan hari ini |
| --- | --- | --- |
| `APPROVAL_MANDATORY` | Perubahan **MUST NOT** berlaku efektif sebelum disetujui | **Belum dijaga.** Tidak ada yang memeriksa status persetujuan sebelum penempatan berlaku |
| `APPROVER_MUST_DIFFER_FROM_CREATOR` | Pembuat **MUST NOT** menjadi penyetuju transaksinya sendiri | **Belum dijaga.** `PATCH /{id}/approval` pada penetapan gaji memakai butir `WfpSalaryAssignment : Update` yang sama dengan buat dan ubah |
| Eskalasi, bukan pengecualian | Unit dengan satu petugas **MUST** dieskalasi ke otoritas berbeda, **MUST NOT** menjadi swa-setuju | **Belum ada** |

**Butir hak akses baru yang dibutuhkan** supaya pemisahan peran dapat dijaga mesin, bukan hanya
dijanjikan dokumen:

| Butir | Dipegang | Jenis transaksi | Alasan |
| --- | --- | --- | --- |
| `WfpSalaryAssignment : Approve` | HR Manager | Perubahan penetapan gaji | Memisahkan wewenang menyetujui dari wewenang membuat dan mengubah |
| `WfpOrganizationAssignment : Approve` | HR Manager | Perubahan penempatan organisasi | Sama |
| `WfpPositionAssignment : Approve` | HR Manager | Perubahan penempatan jabatan | Sama |
| `WfpManagerAssignment : Approve` | HR Manager | Perubahan penetapan atasan | Sama |

**Keempat butir sengaja dipisah** `[DECISION]` `HRD-DEC-036`. Keempat perubahan adalah **empat
jenis transaksi berbeda dengan definisi alur, siklus versi, dan jejak audit masing-masing**.
Menggabungkannya menjadi satu butir akan membuat pertanyaan "siapa menyetujui perubahan yang
mana" tidak dapat dijawab dari jejak audit.

**Penyelesaian saat pemrakarsa sama dengan calon penyetuju.** Bila HR Admin yang mengajukan juga
satu-satunya pemegang butir `: Approve` di unitnya, tugas persetujuan **MUST** diselesaikan ke
penyetuju tingkat lebih tinggi yang berwenang sesuai konfigurasi. Ia **MUST NOT** menjadi
swa-setuju, dan **MUST NOT** dilewati dengan alasan kekurangan personel.

Selama keempat butir itu belum ada, pemisahan peran **tidak dapat** ditegakkan hanya dengan
konfigurasi peran — karena menyetujui dan mengubah masih berbagi satu butir yang sama.

### 2.5 Keterlihatan nominal gaji — `HRD-DEC-033`

| Field | Isi |
| --- | --- |
| Dasar | `HRD-DEC-033` |
| Status | **`SECURITY_APPROVED`** — co-sign 2026-08-30 |
| Co-approver | `Project final decision authority — Security` — **sudah menandatangani**, 2026-08-30 |

**Prinsip:** `SALARY_AMOUNT_HIDDEN_BY_DEFAULT`.

| Tempat | Nominal gaji | Yang boleh tampil |
| --- | --- | --- |
| Daftar lintas pegawai | **Disembunyikan** | Struktur gaji, golongan, kelas jabatan, mata uang, tanggal berlaku, penanda utama, status persetujuan |
| Detail gaji satu pegawai | **Boleh**, hanya bagi pemegang butir sensitif | Seluruh isi penetapan |
| Laporan atau ekspor massal | **Tidak diberikan pada MVP** | — |

| Butir hak akses baru | Dipegang | Catatan |
| --- | --- | --- |
| `WfpSalaryAssignment : ViewAmount` | **HR Manager**, dan pemilik data atas gajinya sendiri lewat jalur layanan mandiri yang tunduk `HRD-DEC-038`. **Payroll Officer TIDAK** — `HRD-DEC-044` | Butir **sensitif**. Terpisah dari `WfpSalaryAssignment : Read` |
| `WfpSalaryAssignment : ViewAmountBulk` | **Belum diberikan kepada siapa pun pada MVP** | Memerlukan co-sign keamanan sebelum diberikan |
| `WorkforceEducation : Verify` | HR Admin dan HR Manager | **Butir baru** `[DECISION]` `HRD-DEC-041`. Hari ini verifikasi pendidikan memakai `WorkforceEducation : Update` — butir **yang sama** dengan buat dan ubah, sehingga pihak yang mencatat pendidikan juga dapat memverifikasinya. Tanpa butir terpisah, invariant "hanya jenjang terverifikasi yang dipakai kebijakan gaji" tidak dapat dijaga mesin |
| `SalaryPolicy : Read`, `: Create`, `: Update`, `: Activate` | **HR Manager saja** | `HRD-DEC-037` dan `HRD-DEC-043`. HR Admin biasa **MUST NOT** memegangnya |

**Larangan yang mengikat:** nominal **MUST NOT** terbuka hanya karena pengguna memegang butir
baca umum. `WfpSalaryAssignment : Read` **tidak** menyiratkan `: ViewAmount`. Memisahkan keduanya
adalah inti keputusan ini — menggabungkannya kembali membatalkannya.

### 2.5a Keamanan data gaji — `HRD-DEC-037` s.d. `HRD-DEC-040`

| Field | Isi |
| --- | --- |
| Dasar | `HRD-DEC-037`, `HRD-DEC-038`, `HRD-DEC-039`, `HRD-DEC-040` |
| Status | `approved` sebagai **kontrak sasaran** — belum diimplementasikan |
| Otoritas | `Project final decision authority — Security`, 2026-08-30 |

#### 2.5a.1 Kewenangan konfigurasi kebijakan gaji

| Aspek | Isi |
| --- | --- |
| Siapa yang boleh mengetahui, membaca, dan mengubah kebijakan gaji serta master gaji | **Hanya `HR Manager`** |
| Faktor penentu gaji yang sah | Golongan; Level; Status kerja; **Jenjang Pendidikan** — `HRD-DEC-041`. **Masa kerja tidak termasuk** — `HRD-DEC-045` |
| Menambah faktor | **MUST NOT** ditambah perancang maupun implementer. Hanya lewat keputusan bisnis yang disetujui |
| Bentuk konfigurasi | Berversi; bertanggal berlaku; dapat diaudit; riwayat lama **MUST NOT** dihapus saat kebijakan berubah |
| Audit perubahan | Siapa; kapan; rujukan versi sebelumnya; rujukan versi baru; tanggal berlaku |

**Diperbarui `HRD-DEC-041` dan `HRD-DEC-045`.** Istilah "masa studi" **ditarik**. Faktor yang
berlaku ada **empat**: Golongan, Level, Status kerja, dan **Jenjang Pendidikan**.
`HRD-Q-55` **tertutup**.

**Masa kerja BUKAN faktor kebijakan gaji pada MVP saat ini.** `HRD-DEC-045` menggantikan
`HRD-DEC-042`, dan `HRD-Q-56` berstatus `DEFERRED / NOT_APPLICABLE_TO_CURRENT_MVP`. Tidak ada
faktor yang tertahan.

#### 2.5a.2 Dua gerbang untuk slip gaji pegawai

Butir `MyPayslip : Read` — atau padanan canonical — **tidak cukup** dengan sendirinya. Otorisasi
menuntut dua gerbang sekaligus:

| # | Gerbang | Bunyi |
| ---: | --- | --- |
| 1 | Kepemilikan sumber daya | Backend **MUST** menurunkan kepemilikan dari `pengguna terautentikasi → profil workforce → pemilik slip gaji`. Pengenal dari frontend **MUST NOT** dipercaya sebagai bukti otorisasi |
| 2 | Otentikasi bertingkat gaji | `SALARY_SENSITIVE_SESSION` masih berlaku |

Permintaan atas slip gaji pegawai lain **MUST** ditolak walaupun pengenalnya benar dan diketahui
pemohon.

#### 2.5a.3 Otentikasi bertingkat dan sesi gaji sensitif

| Aspek | Sasaran |
| --- | --- |
| Pemicu | Sebelum data gaji atau slip gaji ditampilkan kepada pegawai |
| Cara | Konfirmasi kata sandi yang sama dengan akun yang sedang masuk, diverifikasi lewat mekanisme Identity **canonical** |
| Larangan | **Jangan** membuat pemverifikasi kata sandi baru untuk HR |
| Hasil | Otorisasi berumur pendek `SALARY_SENSITIVE_SESSION`, bawaan **5 menit** |
| Batal ketika | Pengguna keluar; sesi utama tidak sah; akun dinonaktifkan; keadaan kata sandi atau keamanan berubah |
| Kata sandi **MUST NOT** disimpan pada | `localStorage`; `sessionStorage`; state frontend yang dipersistkan; persistensi Redux; basis data tambahan; log; analytics |

#### 2.5a.4 Audit pembacaan gaji sensitif — pengecualian terhadap konvensi

Konvensi project **tidak** mencatat permintaan `GET`. Untuk data gaji sensitif, konvensi itu
**dikecualikan** — `SENSITIVE_GET_MUST_BE_AUDITED`.

| Kejadian yang wajib meninggalkan jejak |
| --- |
| Nominal gaji dilihat |
| Daftar slip gaji dibuka setelah otentikasi bertingkat |
| Detail slip gaji dilihat |
| Slip gaji diunduh |
| Detail gaji administratif sensitif dilihat |
| Percobaan yang ditolak terhadap gaji pegawai lain |
| Otentikasi bertingkat yang gagal |

| Boleh dicatat | **MUST NOT** dicatat |
| --- | --- |
| `UserId` pelaku; `WorkforceProfileId` pelaku bila ada; pengenal record yang dituju; pengenal periode payroll; aksi; waktu; berhasil atau gagal; IP, perangkat, user-agent bila didukung; pengenal korelasi | Kata sandi; **nominal gaji**; nomor rekening bank; nomor pajak; seluruh isi response sensitif; token otentikasi |

**Perhatikan kolom kanan.** Audit mencatat **bahwa** nominal dibaca, bukan nominalnya. Mencatat
nominalnya memindahkan kebocoran dari response ke log — tempat yang disimpan lebih lama dan
dibaca lebih banyak orang.

#### 2.5a.5 Perlindungan pada HTTP dan sisi klien

| Aturan | Bunyi |
| --- | --- |
| Response gaji sensitif | `Cache-Control: no-store` atau padanan canonical |
| Frontend **MUST NOT** menyimpan pada | Redux yang dipersistkan; `localStorage`; cache peramban berumur panjang; payload analytics |
| Unduhan slip gaji | **MUST NOT** bergantung pada URL statis publik yang dapat ditebak. **MUST** lewat endpoint terautentikasi yang memeriksa otentikasi, kepemilikan atau hak administratif sensitif, dan sesi gaji sensitif yang masih berlaku |

#### 2.5a.6 Keterlihatan massal tetap di luar MVP

`ViewAmountBulk` dan ekspor gaji sensitif **belum diberikan kepada siapa pun**. Bila kelak
dibutuhkan, ia memerlukan **seluruh** hal berikut: butir hak akses eksplisit; tinjauan keamanan
tersendiri; kejadian audit; serta keputusan dan kontrak baru.

### 2.6 Yang **tidak** dipetakan di sini

| Peran | Alasan |
| --- | --- |
| Komite Medik dan Subkomite Kredensial | `S-C1` `BLOCKED`. Memetakan hak aksesnya berarti menetapkan siapa yang berwenang atas kewenangan klinis |
| K3RS | `S-C6` `BLOCKED` |
| Perekrut | `S-D2` `BLOCKED` |

---

## 3. Kewenangan yang **tidak dapat** dijaga mesin hak akses

Ini bagian terpenting dokumen ini. Mesin hak akses menjawab *"boleh melakukan jenis aksi ini
atau tidak"*. Ia **tidak** menjawab *"boleh melakukannya pada baris data yang ini atau tidak"*.

### 3.1 Penjaga di tingkat aturan bisnis

| Penjaga | Apa yang dijaganya | Apa yang **tidak** dijaganya | Risiko bila hilang |
| --- | --- | --- | --- |
| Pelaku adalah penyetuju yang ditugaskan | Hanya orang yang namanya tertulis pada baris tugas yang dapat memutuskannya | Tidak menjaga apakah orang itu **seharusnya** menjadi penyetuju | Siapa pun pemegang butir menyetujui dapat memutuskan pengajuan siapa pun `[EXISTING]` — penjaga ini **ada** |
| Kepemilikan layanan mandiri | Pegawai hanya menyentuh datanya sendiri, karena identitasnya diturunkan dari pengguna yang masuk | Tidak menjaga apakah pegawai boleh melihat data unitnya | Pegawai membaca data pegawai lain dengan menukar identifier di alamat `[EXISTING]` — penjaga ini **ada** |
| Koreksi kehadiran hanya untuk data sendiri | HR Admin **tidak** dapat membuat koreksi atas nama pegawai lain | — | `[EXISTING]` — penjaga ini **ada**, dan justru inilah yang akan dilonggarkan secara terkendali oleh `HRD-DEC-028` |
| Delegasi tidak dapat disetujui sendiri | Pemberi maupun penerima delegasi tidak dapat menyetujui delegasinya sendiri | — | `[EXISTING]` — penjaga ini **ada**. Ini pola pemisahan peran yang benar |
| Penilaian kinerja final terkunci | Setelah difinalkan, penilaian dan rinciannya tidak dapat diubah | — | `[EXISTING]` — penjaga ini **ada** |
| Periode tertutup menolak perubahan | Kehadiran dan lembur pada periode tertutup tidak dapat diubah | — | `[EXISTING]` — penjaga ini **ada** |

### 3.2 Penjaga yang **belum ada**, beserta risikonya

| Penjaga yang belum ada | Yang terjadi hari ini | Risiko | Sumber |
| --- | --- | --- | --- |
| Pemisahan peran pada tindakan disiplin | **Pembuat tindakan dapat menyetujui tindakannya sendiri** | Keputusan yang menyangkut nasib seorang pegawai diambil satu orang tanpa pengawasan | `[OPEN]` `HRD-Q-51` |
| Tingkatan izin untuk data paling rahasia | Kasus kedisiplinan bertanda paling rahasia dapat dibaca siapa pun yang memegang butir baca umum | Data yang paling sensitif di modul ini justru dijaga sama seperti data biasa | `[OPEN]` `HRD-Q-52` |
| Pembatasan siapa boleh membaca nominal gaji orang lain | **Diputuskan** `HRD-DEC-033`: nominal disembunyikan pada daftar lintas pegawai; butir `: ViewAmount` terpisah dari `: Read` | Penegakannya **belum ada di kode** — butir sensitifnya belum dibuat | **`SECURITY_APPROVED`** — co-sign 2026-08-30 |
| Hak akses per aksi pada penempatan jadwal kerja | 8 endpoint tanpa butir hak akses | Siapa pun yang masuk dapat menempatkan, mengubah, dan menghapus jadwal kerja pegawai mana pun | Lihat bagian 6 |
| Guard perubahan jadwal berlaku surut | Belum ada | Jadwal pada periode yang sudah diproses dapat diubah langsung | `[DECISION]` `HRD-DEC-027`, `MISSING` |
| Permission khusus untuk membalikkan eksekusi cuti | Belum ada | Cuti yang sudah selesai dapat dibalikkan tanpa alasan dan tanpa pemeriksaan kunci payroll | `[DECISION]` `HRD-DEC-023`, `MISSING` |
| Guard `Applied` terminal pada koreksi kehadiran | Belum ada | Penerapan koreksi dapat berjalan dua kali dan memutasi ulang kehadiran | `[DECISION]` `HRD-DEC-022`, `MISSING` |
| Siapa yang berwenang membuka kembali periode | Mekanismenya ada; peran fungsional `Payroll Officer` ditetapkan `HRD-DEC-032`, pemetaan ke peran Identity `MAPPING_REQUIRED` | Pemegang butir membuka kembali dapat membuka periode mana pun | `[OPEN]` `HRD-Q-23`, `HRD-Q-32` |

### 3.3 Contoh nyata supaya perbedaan kedua lapisan terbaca

> Seorang kepala unit memegang butir hak akses menyetujui pada kotak masuk. Ia membuka daftar dan
> melihat lima pengajuan. **Kelimanya memang ditugaskan kepadanya**, sehingga ia dapat memutuskan
> kelimanya.
>
> Kepala unit dari unit lain juga memegang butir yang sama. Bila ia mencoba menyetujui salah satu
> dari lima pengajuan itu dengan memanggil alamatnya langsung, permintaannya **ditolak** — bukan
> karena butir hak aksesnya kurang, melainkan karena pengajuan itu tidak ditugaskan kepadanya.
>
> **Butir hak akses membuka pintu; penugasan menentukan ruangan mana yang boleh dimasuki.**

---

## 4. Kewenangan per jenis transaksi

Larangan menyamaratakan berlaku penuh di sini. Setiap baris di bawah adalah bukti **per jenis
transaksi**, bukan kesimpulan yang ditarik dari satu jenis lalu diterapkan ke semua.

### 4.1 Cuti

| Aspek | Isi |
| --- | --- |
| Penentuan penyetuju | **Dapat dikonfigurasi**, bukan tertulis di kode. Sumber penyetuju yang didukung: atasan pemohon, tingkat atasan tertentu, pengguna tertentu, jabatan tertentu, unit organisasi, peran, matriks persetujuan, dan pilihan pemohon `[EXISTING]` |
| Gate nyata | Pelaku adalah penyetuju yang ditugaskan pada baris itu `[EXISTING]` |
| Tingkatan persetujuan | Didukung secara struktural lewat urutan langkah, tetapi **status domainnya tetap satu nilai** sepanjang tingkatan apa pun |
| Isi konfigurasinya | **Belum ditemukan** data langkah workflow untuk cuti di repository. Tanpa isi ini, tidak ada penyetuju yang dapat ditentukan |

### 4.2 Lembur

| Aspek | Isi |
| --- | --- |
| Kosakata peran pada domain | `Supervisor`, `Manager`, `HrAdmin`, `Payroll` |
| Apakah kosakata itu ditegakkan | **Tidak.** Terbukti hanya nilai bawaan pada kolom, tidak dipetakan ke pemeriksaan identitas apa pun `[EXISTING]` |
| Penegakan nyata | Butir hak akses generik per aksi, **terputus** dari kosakata peran itu |
| Konsekuensi | `HRD-Q-33` bukan lagi "bagaimana pemetaannya", melainkan **"peta ini belum dibangun"** |

### 4.3 Koreksi kehadiran

| Aspek | Isi |
| --- | --- |
| Siapa yang boleh mengajukan | **Hanya pegawai pemilik data.** HR Admin **tidak dapat** mengajukan atas nama pegawai lain `[EXISTING]` |
| Siapa yang memutuskan | Mesin persetujuan generik, dengan gate penugasan yang sama |
| Perubahan target | `[DECISION]` `HRD-DEC-028` melonggarkan ini secara terkendali: HR Admin **boleh** mengajukan atas nama pegawai, **wajib** menyimpan initiator, pegawai yang diwakili, alasan, waktu, bukti bila kebijakan menuntut, notifikasi kepada pegawai, dan jejak audit lengkap. **Tidak ada jalur persetujuan baru** |

### 4.4 Penetapan gaji

| Aspek | Isi |
| --- | --- |
| Jalur persetujuan | **Tidak terbukti ada.** Kolom status persetujuan ada dan endpoint persetujuan ada, tetapi mesin persetujuan berjenjangnya tidak ditemukan `[OPEN]` `HRD-Q-19` |
| Yang tidak boleh disimpulkan | Jangan menyimpulkan bahwa penetapan gaji **tidak memerlukan** persetujuan hanya karena jalurnya tidak ditemukan. Yang benar: **belum diketahui** |

### 4.5 Tukar shift

| Aspek | Isi |
| --- | --- |
| Dua pihak yang berbeda | Rekan yang dituju menjawab lebih dulu; baru atasan memutuskan `[EXISTING]` |
| Siapa yang boleh menjawab sebagai rekan | **Hanya pegawai yang dituju**, dijaga guard eksplisit |
| Apakah dapat dilewati | **Tidak.** Guard menolak peneruskan ke atasan bila rekan belum menerima `[EXISTING]` |

### 4.6 Delegasi persetujuan

| Aspek | Isi |
| --- | --- |
| Siapa yang mengaktifkan | **Approver itu sendiri** yang mengajukan delegasinya |
| Siapa yang menyetujui | **Bukan** pemberi delegasi, **bukan** pula penerimanya. Guard eksplisit melarang keduanya `[EXISTING]` |
| Mekanismenya | Memindahkan penugasan pada tugas yang masih terbuka; pencabutan mengembalikannya ke penyetuju semula. **Bukan** percabangan kode pada alur persetujuan |

---

## 5. Kolom sensitif dan masa simpan

### 5.1 Kolom yang ditandai sensitif

Daftar lengkapnya ada di [`data/data-dictionary.md`](../data/data-dictionary.md). Ringkasnya:

| Kelompok data | Contoh kolom | Tingkat kepekaan | Siapa yang boleh membaca |
| --- | --- | --- | --- |
| Gaji dan komponennya | Gaji pokok, tunjangan, potongan, nomor rekening bank, nomor pajak | **Tinggi** | `[OPEN]` `HRD-Q-20` untuk daftar lintas-pegawai. Untuk halaman detail pegawai, HR Admin sudah memilikinya hari ini |
| Kasus dan tindakan kedisiplinan | Uraian pelanggaran, sanksi, hasil investigasi | **Sangat tinggi** — bertanda paling rahasia di model | `[OPEN]` `HRD-Q-52` — tingkatan izin khususnya **belum ada** |
| Rekam kesehatan kerja | Diagnosis, hasil pemeriksaan | **Sangat tinggi** | `[DECISION]` `HRD-DEC-010` berstatus `draft`: hanya K3RS dan pegawai bersangkutan. Pihak lain hanya melihat kesimpulan kelayakan kerja tanpa isi medis. **`S-C6` `BLOCKED`** |
| Data pribadi pegawai | Nomor identitas, alamat, kontak darurat, data keluarga | Sedang | HR Admin dan pegawai bersangkutan |
| Alasan permohonan | Alasan cuti, alasan koreksi, alasan pengunduran diri | Sedang | Pemohon dan penyetuju yang ditugaskan |
| Kredensial dan lisensi | Nomor STR, nomor SIP, masa berlaku | Sedang | **`S-C1` `BLOCKED`** |

### 5.2 Aturan yang mengikat kolom sensitif

1. Kolom bertanda sensitif **MUST NOT** masuk ke payload logger.
2. Kolom bertanda sensitif **MUST NOT** dipakai sebagai contoh berisi data asli di dokumentasi
   mana pun. Seluruh contoh pada blueprint ini memakai data samaran.
3. Kebutuhan penyamaran pada response perlu ditinjau per endpoint. Yang paling mendesak: daftar
   penetapan gaji lintas-pegawai.

### 5.3 Bentuk aman untuk data kesehatan kerja

`[DECISION]` `HRD-DEC-010`, berstatus `draft` menunggu K3RS.

> Seorang atasan membuka profil anak buahnya untuk menyusun jadwal. Ia **boleh** melihat
> keterangan seperti "Layak bekerja dengan pembatasan: tidak boleh shift malam sampai 30
> September". Ia **tidak boleh** melihat alasan medis di balik pembatasan itu.

Artinya satu endpoint yang mengembalikan **seluruh isi** rekam kesehatan **tidak cukup aman**
untuk dipakai atasan. Perlu bentuk ringkas yang memang dirancang untuk pembaca non-medis.

**Bentuk itu tidak dirancang di sini** karena `S-C6` `BLOCKED`. Yang dicatat hanyalah bahwa
kebutuhannya sudah teridentifikasi.

### 5.4 Masa simpan

| Data | Masa simpan | Keadaan |
| --- | --- | --- |
| Rekaman mentah kehadiran | Belum ditetapkan | **`[OPEN]` `HRD-Q-25`** |
| Berkas lampiran dan bukti | Belum ditetapkan | **`[OPEN]`** — bagian dari `HRD-DEP-006` |
| Riwayat kepegawaian, kehadiran, cuti, kinerja, payroll | **Tidak dihapus.** Penghapusan bersifat penandaan, bukan penghapusan baris | `[EXISTING]` — pola bawaan seluruh model |
| Jejak audit persetujuan | **Tidak dihapus** | `[EXISTING]` |

**Konsekuensi yang harus diketahui:** karena penghapusan bersifat penandaan, data pegawai yang
sudah berhenti **tetap ada** di basis data. Ini benar untuk keperluan audit dan pelaporan, tetapi
menuntut kebijakan retensi yang belum ada.

---

## 6. Pengecualian: endpoint yang tidak mengikuti pola hak akses

Dua controller **tidak memiliki `[AccessPermission]`** pada action-nya, sementara 148 lainnya
memilikinya. Ini adalah daftar pengecualian bernama, bukan pendaftaran ulang seluruh endpoint.

| Controller | Jumlah endpoint | Lokasi file | Autentikasi | Dampak |
| --- | ---: | --- | --- | --- |
| `WfpWorkScheduleAssignmentController` | 8 | `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs` | `[Authorize]` **ada** | **Paling berdampak.** Siapa pun yang masuk dapat menempatkan, mengubah, menonaktifkan, dan menghapus jadwal kerja pegawai mana pun. Jadwal kerja adalah dasar perhitungan kehadiran, sehingga perubahan di sini merambat ke kehadiran, lembur, dan pada akhirnya gaji |
| `AttendanceSelfServiceController` | 7 | `Areas/SelfServices/HumanResource/Controllers/AttendanceSelfServiceController.cs` | `[Authorize]` **ada** | **Lebih kecil.** Kepemilikan tetap diturunkan dari pengguna yang masuk, sehingga pegawai tetap hanya menyentuh datanya sendiri. Yang hilang hanya kemampuan membatasi per aksi — misalnya melarang sekelompok pengguna mencatat kehadiran lewat aplikasi |

**Butir hak akses target** untuk keduanya:

| Controller | Butir yang diusulkan |
| --- | --- |
| `WfpWorkScheduleAssignmentController` | `WorkScheduleAssignment : Read`, `: Create`, `: Update`, `: Delete` |
| `AttendanceSelfServiceController` | `MyAttendance : Read`, `: CheckIn`, `: CheckOut` |

**Ini temuan yang dicatat, bukan perbaikan yang dikerjakan dari alur blueprint.** Perbaikannya
menjadi task implementasi tersendiri.

Satu controller layanan mandiri lain, `HumanResourceContextController`, juga tidak memiliki butir
hak akses — tetapi itu **disengaja dan benar**. Ia hanya mengembalikan konteks pengguna yang
sedang masuk; membatasi aksesnya berarti pengguna tidak dapat mengetahui identitasnya sendiri.

---

## 7. Audit

### 7.1 Lapisan pencatatan

| Lapisan | Bentuk | Isi | Yang **MUST NOT** masuk |
| --- | --- | --- | --- |
| Logger aplikasi | Layanan pencatat yang dipanggil controller | Identifier baris data, nama controller, nama aksi, status hasil | Diagnosis, keluhan, alasan medis, nominal gaji, isi kasus kedisiplinan, nomor identitas |
| Kolom bawaan pada setiap baris | Sepuluh kolom yang diwarisi seluruh model | Siapa membuat dan kapan, siapa mengubah dan kapan, siapa menghapus dan kapan, siapa membatalkan dan kapan | — |
| Riwayat status persetujuan | Baris riwayat pada mesin persetujuan | Dari status apa ke status apa, siapa pelakunya, kapan | — |
| Aksi persetujuan | Baris aksi pada mesin persetujuan | Jenis aksi, pelaku, alasan, waktu | — |
| Buku besar saldo cuti | Baris buku besar | Jenis transaksi, arah, jumlah, rujukan penyebabnya | — |
| Riwayat pemrosesan kehadiran | Baris riwayat pemrosesan | Mode, status, jumlah yang diproses, kesalahan | — |

### 7.2 Konvensi pencatatan yang diturunkan, bukan didaftar ulang

| Method | Dicatat |
| --- | :---: |
| `GET` | **Tidak** |
| `POST`, `PUT`, `PATCH`, `DELETE` | **Ya** |

**Pengecualian yang perlu dicatat:** beberapa endpoint yang memakai `POST` sebenarnya hanya
membaca — misalnya menghitung jumlah hari cuti, pratinjau realisasi lembur, dan pratinjau
akrual. Ketiganya memakai `POST` karena parameternya kompleks, bukan karena mengubah data.
Pencatatannya **tidak berbahaya**, hanya menambah jumlah baris log tanpa nilai audit.

Ini dicatat sebagai catatan, bukan sebagai penyimpangan yang harus diperbaiki.

### 7.3 Kejadian yang **wajib** meninggalkan jejak tahan lama

Ini daftar kejadian yang jejaknya **tidak boleh hilang**, apa pun yang terjadi pada log aplikasi.
Jejaknya tinggal di basis data sebagai baris, bukan hanya sebagai catatan log.

| Kejadian | Jejak yang wajib ada | Keadaan |
| --- | --- | --- |
| Periode kehadiran ditutup | Siapa menutup dan kapan | `[EXISTING]` |
| Periode kehadiran dibuka kembali | Siapa membuka, kapan, dan alasannya | `[EXISTING]` untuk pelaku dan waktu; alasan perlu diverifikasi |
| Koreksi kehadiran diterapkan | Siapa menerapkan, kapan, dan nomor versi pemrosesan sebelum dan sesudah | `[EXISTING]` |
| Koreksi dibuat atas nama pegawai | Siapa yang membuat, pegawai yang diwakili, alasan, waktu, dan bukti pemberitahuan | **`MISSING`** `[DECISION]` `HRD-DEC-028` |
| Saldo cuti berubah | Satu baris buku besar beserta rujukan penyebabnya | `[EXISTING]` |
| Eksekusi cuti dibalikkan | Siapa membalikkan, kapan, alasannya, dan apakah periode payroll diperiksa | **`MISSING`** `[DECISION]` `HRD-DEC-023` |
| Pemberitahuan pemanggilan kembali ditandai tersampaikan oleh HR Manager | Siapa menandai, kapan, dan alasannya | **`MISSING`** `[DECISION]` `HRD-DEC-024` |
| Pengecualian kerja di luar jadwal diklasifikasikan | Siapa mengklasifikasikan, kapan, klasifikasi apa, dan alasannya | **`MISSING`** `[DECISION]` `HRD-DEC-025` |
| Serah terima payroll dijalankan, diperbaiki, atau dibatalkan | Siapa, kapan, dan berapa baris yang terpengaruh | `[EXISTING]` |
| Setiap keputusan persetujuan | Jenis aksi, pelaku, alasan, waktu | `[EXISTING]` |
| Delegasi diaktifkan atau dicabut | Siapa, kapan, dan tugas mana yang berpindah | `[EXISTING]` |
| Penetapan gaji dibuat atau diubah | Siapa, kapan, dan nilai sebelum-sesudah | `[EXISTING]` untuk pelaku dan waktu; riwayat nilai terjaga karena perubahan gaji membuat baris baru, bukan menimpa baris lama |
| Tindakan disiplin berpindah status | Siapa, kapan, dari status apa ke status apa | Sebagian `[EXISTING]` — pelaku dan waktu tercatat pada kolom bawaan, tetapi **tidak ada riwayat perpindahan status tersendiri** |

### 7.4 Jejak audit yang paling lemah hari ini

| No | Kelemahan | Akibat |
| ---: | --- | --- |
| 1 | Tindakan disiplin berpindah status tanpa riwayat perpindahan | Tidak dapat dibuktikan urutan keputusan pada kasus yang menyangkut nasib pegawai |
| 2 | Pembalikan eksekusi cuti tanpa alasan wajib | Saldo dan kehadiran berubah tanpa penjelasan yang dapat dibaca auditor |
| 3 | Tidak ada jejak pemberitahuan kepada pegawai | Klaim "pegawai sudah diberi tahu" tidak dapat dibuktikan |
| 4 | Delapan endpoint jadwal kerja tanpa hak akses per aksi | Perubahan jadwal tercatat pelakunya, tetapi tidak ada pembatasan siapa yang boleh melakukannya |

---

## 8. Butir hak akses baru yang dibutuhkan desain target

Seluruhnya **Rencana (belum tersedia)**. Butir ini lahir dari keputusan yang sudah dikunci, bukan
dari nama layar atau menu.

| Butir hak akses | Untuk apa | Sumber keputusan |
| --- | --- | --- |
| `AttendanceCorrection : CreateOnBehalf` | Mengajukan koreksi kehadiran atas nama pegawai | `HRD-DEC-028` |
| `AttendanceException : Classify` | Mengklasifikasikan pengecualian kerja di luar jadwal | `HRD-DEC-025` |
| `LeaveExecution : Reverse` | Membalikkan eksekusi cuti secara terkendali | `HRD-DEC-023` — butirnya **sudah ada** hari ini, tetapi tanpa keenam syaratnya |
| `LeaveRecall : OverrideAcknowledgement` | Menandai pemberitahuan pemanggilan kembali sudah tersampaikan | `HRD-DEC-024` |
| `WorkScheduleAssignment : Read`, `: Create`, `: Update`, `: Delete` | Menjaga delapan endpoint penempatan jadwal yang kini tanpa hak akses | Temuan bagian 6 |
| `MyAttendance : Read`, `: CheckIn`, `: CheckOut` | Menjaga tujuh endpoint kehadiran layanan mandiri | Temuan bagian 6 |
| `RosterPeriod : Read`, `: Create`, `: Update`, `: Validate`, `: Submit`, `: Publish`, `: Lock`, `: Close`, `: Cancel` | Siklus roster | `HRD-DEC-026` |
| `RosterAssignment : Read`, `: Create`, `: Update`, `: Cancel` | Penugasan roster per pegawai | `HRD-DEC-026` |
| `ShiftAssignment : Read`, `: Create`, `: Update`, `: Cancel` | Penugasan shift harian | `HRD-DEC-026` |
| `ShiftReplacement : Read`, `: Create`, `: Approve`, `: Cancel` | Penggantian shift | `HRD-DEC-026` |
| `EmergencyStaffing : Read`, `: Create`, `: Fulfill`, `: Cancel` | Permintaan tenaga darurat | `HRD-DEC-026` |
| `OnCallAssignment : Read`, `: Create`, `: Confirm`, `: Activate`, `: Cancel` | Penugasan siaga aktual | `HRD-DEC-026` |
| `WorkflowReminder : Read`, `: Run` | Mesin pengingat dan eskalasi | `HRD-DEC-030` |
| `OffboardingChecklist : Read`, `: Update`, `: Close` | Daftar periksa offboarding | `HRD-CAP-17` |
| `WfpOrganizationAssignment : ReadAll`, `WfpPositionAssignment : ReadAll`, `WfpManagerAssignment : ReadAll`, `WfpEmploymentHistory : ReadAll`, `WfpSalaryAssignment : ReadAll` | Daftar lintas-pegawai | `HRD-DEC-012` |

**Catatan pada butir `ReadAll`.** Butir ini sengaja **dipisahkan** dari butir `Read` biasa.
Alasannya: membaca penetapan gaji **satu** pegawai yang menjadi tanggung jawab seseorang berbeda
sifatnya dari membaca penetapan gaji **seluruh** pegawai sekaligus. Memakai butir yang sama untuk
keduanya akan membuat pembatasan tidak mungkin dilakukan.

---

## 9. Yang **tidak** dirancang di dokumen ini

| Kelompok | Alasan |
| --- | --- |
| Hak akses kredensial, lisensi, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` `BLOCKED`. Menetapkan siapa yang berwenang atas kewenangan klinis adalah wewenang Komite Medik |
| Hak akses rekam kesehatan kerja | `S-C6` `BLOCKED`. `HRD-DEC-010` masih `draft` menunggu K3RS |
| Hak akses perencanaan tenaga kerja, rekrutmen, benefit, tiket HR | `S-D1` s.d. `S-D4` `BLOCKED` |
| Peta peran aplikasi yang sebenarnya ke butir hak akses HR | `[OPEN]` `HRD-Q-33`. Peta ini **belum dibangun**, dan mengarangnya berarti menetapkan siapa boleh melakukan apa tanpa wewenang |
| Nilai kebijakan pembatasan akses gaji | `[OPEN]` `HRD-Q-20` |
| Tingkatan izin untuk data kedisiplinan paling rahasia | `[OPEN]` `HRD-Q-52` |
| Kebijakan retensi rekaman mentah kehadiran | `[OPEN]` `HRD-Q-25` |

---

## 10. Traceability

| Bagian | Decision ID / Temuan | Sumber bukti |
| --- | --- | --- |
| Tiga lapisan hak akses | `HRD-CAP-26` | `01-existing-capability-map.md` |
| Penarikan temuan keamanan yang keliru | `HRD-TF-001` ditarik pada capability map revisi `1.1` | `00-interview-decisions.md` bagian 15.1 |
| Gate penugasan penyetuju | Audit `PHASE 2A.1` | `flows/09-unified-approval.md` |
| Peran lembur terputus dari hak akses | `HRD-Q-33` | `flows/04-overtime.md` |
| Koreksi hanya untuk data sendiri, dan pelonggarannya | `HRD-DEC-028` | `flows/07-attendance-correction.md` |
| Delegasi tidak dapat disetujui sendiri | Audit `PHASE 2B` | `flows/09-unified-approval.md` |
| Swa-setuju pada tindakan disiplin | `HRD-Q-51` | `flows/14-employee-relations-discipline.md` |
| Data kedisiplinan tanpa tingkatan izin | `HRD-Q-52` | `flows/14-employee-relations-discipline.md` |
| Privasi rekam kesehatan kerja | `HRD-DEC-010` (`draft`) | `00-interview-decisions.md` bagian 14.4 |
| Butir hak akses baru untuk roster | `HRD-DEC-026` | `flows/05-work-scheduling.md` |
| Butir hak akses baru untuk pengingat dan eskalasi | `HRD-DEC-030` | `flows/09-unified-approval.md` |
| Dua controller tanpa hak akses per aksi | Audit endpoint pada baseline saat ini | `contracts/api-contract.md` bagian 10 |

---

## 11. Perencanaan Tenaga Kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

Pemetaan endpoint ke butir hak akses ada pada kolom `Hak akses` di `api-contract.md` bagian 13 dan
**tidak** diulang di sini. Seluruh butir di bawah **Rencana (belum tersedia)**.

### 11.1 Butir hak akses baru

| Resource | Action | Untuk apa | Sumber keputusan |
| --- | --- | --- | --- |
| `WorkforceStaffingStandard` | `Read`, `Create`, `Update`, `Submit`, `Cancel` | Standar kebutuhan tenaga | `HRD-DEC-060`, `HRD-DEC-064` |
| `WorkforceStaffingWorkload` | `Read`, `Create`, `Update` | Angka beban manual | `HRD-DEC-060` |
| `WorkforcePositionFormation` | `Read` | Formasi berlaku dan riwayatnya — **tidak ada** action tulis | `HRD-DEC-062` |
| `WorkforceInitialFormationBatch` | `Read`, `Create`, `Update`, `Submit`, `Cancel` | Batch formasi awal | `HRD-DEC-070` |
| `WorkforceManpowerPlan` | `Read`, `Create`, `Update`, `Submit`, `Cancel` | Rencana tahunan dan addendum | `HRD-DEC-062`, `HRD-DEC-068` |
| `WorkforcePlanningCost` | `Read`, `Adjust` | Melihat angka biaya; koreksi biaya oleh Finance | `HRD-DEC-065` |
| `WorkforceHeadcountRequest` | `Read`, `Create`, `Update`, `Submit`, `Withdraw`, `RequestCancellation`, `ChangeFulfilmentPlan`, `LinkFulfilment` | Permintaan headcount dan kursinya | `HRD-DEC-063`, `HRD-DEC-066`, `HRD-DEC-067`, `HRD-DEC-071` |
| `WorkforceRecruitmentQueue` | `Read` | Antrean kursi luar bagi Recruiter | `HRD-DEC-066` |
| `WorkforceStaffingGap` | `Read` | Gap langsung dan potret | `HRD-DEC-061`, `HRD-DEC-069` |
| `WorkforcePlanningSetting` | `Read`, `Update` | Pengaturan kebijakan perencanaan | `HRD-DEC-061`, `HRD-DEC-071` |

`WorkforcePlanningCost` sengaja dipisah dari `Read` dokumen, mengikuti pola `: ViewAmount` pada
`HRD-DEC-033`: Recruiter dan Petugas payroll dapat diberi baca dokumen tertentu tanpa melihat biaya.

### 11.2 Peta peran ke butir hak akses

| Peran | Butir | Batas tambahan |
| --- | --- | --- |
| **HR Admin** | Standar `Read`, `Create`, `Update`, `Submit`, `Cancel`; angka beban seluruh action; formasi `Read`; batch seluruh action; rencana seluruh action; `WorkforcePlanningCost : Read`; permintaan `Read`, `LinkFulfilment`; antrean `Read`; gap `Read`; pengaturan `Read` | Pemrakarsa `T9` dan `T10` |
| **HR Manager** | Seluruh butir HR Admin; ditambah permintaan `ChangeFulfilmentPlan`, `RequestCancellation`; pengaturan `Update` | Penyetuju langkah 1 `T9`, `T10`, `T12`; langkah 2 `T11`; satu-satunya langkah `T13` |
| **Kepala Unit** | Standar `Read`, `Create`, `Update`, `Submit`; angka beban `Read`, `Create`, `Update`; formasi `Read`; rencana `Read`; `WorkforcePlanningCost : Read`; permintaan `Read`, `Create`, `Update`, `Submit`, `Withdraw`, `RequestCancellation`; gap `Read` | **Hanya unit dalam cakupan organisasinya** — dijaga aturan bisnis, bagian 11.3 |
| **Finance / Anggaran** | Rencana `Read`; permintaan `Read`; `WorkforcePlanningCost : Read`, `: Adjust`; gap `Read` | Penyetuju langkah Finance `T10` dan `T12`. `MAPPING_REQUIRED` |
| **Direktur** | Standar `Read`; rencana `Read`; permintaan `Read`; `WorkforcePlanningCost : Read`; gap `Read` | Penyetuju langkah terakhir `T9`, `T10`, `T12`. `MAPPING_REQUIRED` |
| **Recruiter** | `WorkforceRecruitmentQueue : Read`; permintaan `LinkFulfilment` | **Tanpa** `WorkforcePlanningCost`. `MAPPING_REQUIRED` |
| **Auditor** | Seluruh butir `Read` di atas, termasuk `WorkforcePlanningCost : Read` | Tidak boleh memegang action selain `Read` |
| **Petugas payroll** | **Tidak ada** | Konsisten dengan `HRD-DEC-044` |

### 11.3 Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga di aturan bisnis | Yang dijaganya | Yang **tidak** dijaganya | Risiko bila hilang |
| --- | --- | --- | --- |
| Cakupan unit Kepala Unit lewat proyeksi organisasi pengguna (`OrganizationAuthorizationProjectionService`) | Kepala ICU hanya melihat dan mengajukan untuk ICU | Bila penempatan organisasi Kepala Unit salah, cakupannya ikut salah (`HRD-TF-021`) | Kepala Unit melihat biaya unit lain |
| Pemisahan pembuat dan penyetuju (`AllowSelfApproval = false`) | Pembuat tidak menjadi penyetuju langkah mana pun | Dua orang yang bersekongkol | Swa-setuju formasi |
| Koreksi biaya hanya oleh penyetuju langkah Finance yang aktif | `WorkforcePlanningCost : Adjust` di luar langkahnya ditolak | Finance yang menyetujui angka koreksinya sendiri — diterima, karena koreksi adalah bagian dari langkah Finance | Angka anggaran berubah tanpa jejak bila penjaga ini hilang |
| Perubahan rencana pemenuhan hanya oleh HR Manager bukan pembuat | `ChangeFulfilmentPlan` oleh pembuat ditolak | — | Kursi luar dialihkan ke internal tanpa kendali |
| Tautan pemenuhan harus cocok dan unik | Satu penempatan satu kursi | Penempatan yang sendirinya salah unit | Kursi terisi oleh orang yang bekerja di unit lain |

### 11.4 Audit

| Kejadian yang wajib meninggalkan jejak tahan lama | Lapisan |
| --- | --- |
| Setiap perpindahan status standar, batch, rencana, permintaan, kursi, dan pembatalan | Custom logger (selain `GET`), ditambah kolom `SubmittedBy`/`ApprovedBy` pada tabelnya |
| Setiap versi formasi | Barisnya sendiri — `SourceType`, `SourceDocumentId`, `ApprovedByUserId`, `ApprovedAt` tidak pernah diubah |
| Koreksi biaya Finance | Angka awal dan angka koreksi sama-sama tersimpan, beserta pelaku dan waktunya |
| Perubahan rencana pemenuhan | Baris `HrdFulfilmentPlanChange` |
| Perubahan pengaturan | Custom logger dengan alasan |
| Potret gap | Barisnya sendiri; tidak dapat diubah |

Payload logger hanya memuat `EntityId`, controller, action, dan status. Kolom bertanda sensitif pada
`data/data-dictionary.md` bagian 8.5 — angka biaya dan teks alasan — **MUST NOT** masuk logger.
Membaca angka biaya **tidak** termasuk audit pembacaan gaji sensitif `HRD-DEC-039`, karena angkanya
dari gaji tengah grade, bukan gaji perorangan (`HRD-DEC-065`).


---

## 12. Rekrutmen dan Hiring — `S-D2` (`last_changed_in` `v8`, 3 November 2026)

### 12.1 Butir Hak Akses Baru

| Resource | Action | Peruntukan | Dasar Keputusan |
| --- | --- | --- | --- |
| `JobRequisition` | `Read`, `Create`, `Update`, `Submit`, `Cancel` | Pengelolaan alokasi requisition formasi | `HRD-DEC-073` |
| `JobVacancy` | `Read`, `Create`, `Update`, `Publish`, `Close` | Pengelolaan lowongan dan publikasi | `HRD-DEC-073` |
| `Candidate` | `Read`, `Create`, `Update`, `UploadDocument` | Registrasi pelamar dan berkas kualifikasi | `HRD-DEC-075` |
| `CandidateScreening` | `Read`, `Evaluate` | Penapisan berkas dan validasi STR klinis | `HRD-DEC-076` |
| `CandidateEvaluation` | `Read`, `Evaluate` | Formulir wawancara HR | `HRD-DEC-077` |
| `ClinicalAssessment` | `Read`, `Evaluate` | Uji kompetensi klinis unit (Hak Veto Mutlak) | `HRD-DEC-077` |
| `JobOffer` | `Read`, `Create`, `Update`, `Approve`, `Cancel` | Penyusunan dan persetujuan standar penawaran upah | `HRD-DEC-078`, `HRD-DEC-079` |
| `JobOfferTier2` | `Approve` | Persetujuan penawaran melebihi plafon skala grade | `HRD-DEC-079` |
| `OccupationalHealthClearance` | `Evaluate` | Input kesimpulan kelaikan fisik MCU (K3RS) | `HRD-DEC-078` |
| `TalentPool` | `Read`, `Promote` | Manajemen kandidat cadangan dan promosi cepat | `HRD-DEC-080` |
| `HiringHandoff` | `Execute` | Serah terima kandidat final ke Onboarding | `HRD-DEC-075` |

### 12.2 Peta Peran ke Butir Hak Akses

| Peran | Hak Akses yang Dipegang | Batas / Larangan Tambahan |
| --- | --- | --- |
| **HR Recruiter** | `JobRequisition` (seluruh action), `JobVacancy` (seluruh action), `Candidate` (seluruh action), `CandidateScreening` (seluruh action), `CandidateEvaluation : Evaluate`, `JobOffer : Read, Create, Update`, `TalentPool : Read, Promote` | DILARANG melihat detail diagnosa medis MCU; DILARANG swa-setuju offer |
| **HR Manager** | Seluruh hak akses HR Recruiter; ditambah `JobRequisition : Approve`, `JobOffer : Approve`, `HiringHandoff : Execute` | Penyetuju offer standar (`T13`) |
| **Kepala Unit / Hiring Manager** | `Candidate : Read` (khusus pelamar unitnya), `ClinicalAssessment : Evaluate`, `JobOffer : Approve` (standar `T13`) | Pemegang hak veto klinis; hanya dapat melihat pelamar unitnya |
| **Dokter K3RS / MCU** | `OccupationalHealthClearance : Evaluate` | Penginput kesimpulan kelaikan fisik MCU pra-kerja; memegang rekam medis |
| **Direktur Keuangan (Finance)** | `JobOfferTier2 : Approve` | Penyetuju eskalasi offer di atas plafon grade (`T14`) |
| **Direktur Utama** | `JobOfferTier2 : Approve` | Pengesah tertinggi offer di atas plafon grade (`T14`) |
| **Auditor** | Seluruh butir `Read` pada domain ini | DILARANG memegang action tulis apa pun |

### 12.3 Kewenangan yang Tidak Dapat Dijaga Mesin Hak Akses

1. **Hak Veto Klinis**: Jika Asesor Klinis Unit memilih `NOT_RECOMMENDED`, logika domain pada `RecruitmentService` mengunci status pelamar dan memblokir penerbitan penawaran kerja, bahkan jika user yang login adalah HR Manager (`HRD-DEC-077`).
2. **Kerahasiaan Medis MCU**: Lapisan DTO `McuClearanceSummaryResponse` tidak pernah memetakan kolom rekam medis/diagnosa dari modul K3RS, sehingga akun HR secara struktural tidak dapat menerima data tersebut (`HRD-DEC-078`).
3. **Pemisahan Pembuat dan Penyetuju (*AllowSelfApproval = false*)**: Pembuat draf penawaran dilarang menyetujui penawarannya sendiri (`HRD-DEC-079`).

### 12.4 Audit Logging S-D2

Kejadian yang wajib dicatat dalam audit log permanen:
- Setiap perubahan status Requisition, Vacancy, Application, dan Job Offer.
- Hasil penapisan administrasi STR (nomor STR, masa berlaku, dan hasil lolos/gugur).
- Nilai evaluasi wawancara HR dan uji klinis unit beserta nama akun penilainya.
- Keputusan persetujuan penawaran kerja bertingkat (waktu, akun penyetuju, dan alasan catatan).
- Input kesimpulan kelaikan fisik MCU oleh tim dokter K3RS.
- Aksi promosi cepat kandidat cadangan dari Talent Pool.


---

## 13. Onboarding dan Kontrak Kerja — `S-C4-ONB` (`last_changed_in` `v9`, 4 November 2026)

Status `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan". Pemetaan endpoint ke butir hak akses ada pada kolom `Hak akses` di `api-contract.md`
bagian 15 dan **tidak** diulang di sini. Seluruh butir di bawah **Rencana (belum tersedia)**.

### 13.1 Butir hak akses baru

| Resource | Action | Untuk apa | Sumber keputusan |
| --- | --- | --- | --- |
| `WorkforceOnboardingCase` | `Read`, `CompleteItem`, `CompleteUnitItem`, `Reassign`, `DeclareReady`, `CancelJoin` | Kasus onboarding dan checklist | `HRD-DEC-083`, `HRD-DEC-086`, `HRD-DEC-088` |
| `WorkforceOnboardingTemplate` | `Read`, `Create`, `Update` | Tiga lapis template | `HRD-DEC-086` |
| `WorkforceEmploymentContract` | `Read`, `MarkSigned`, `UploadDocument`, `RequestCorrection` | Kontrak kerja | `HRD-DEC-084`, `HRD-DEC-089`, `HRD-DEC-090` |
| `WorkforceContractDecision` | `Read`, `Submit`, `RecordNotification` | Keputusan perpanjang / pegawai tetap / tidak diperpanjang | `HRD-DEC-089` |
| `WorkforceProbation` | `Read`, `Submit` | Masa percobaan | `HRD-DEC-085` |
| `WorkforcePreceptorship` | `Read`, `Manage` | Pendampingan klinis | `HRD-DEC-087` |
| `WorkforceInitialContractBatch` | `Read`, `Create`, `Update`, `Submit`, `Cancel` | Batch data awal kontrak | `HRD-DEC-090` |
| `WorkforceOnboardingAccessRequest` | `Read`, `Complete`, `Retry` | Akun dan akses kerja | `HRD-DEC-088` |
| `WorkforceLifecycleWorklist` | `Read` | Daftar Perlu Tindakan | `HRD-DEC-085`, `HRD-DEC-089` |
| `WorkforceLifecycleSetting` | `Read`, `Update` | Pengaturan siklus | `HRD-DEC-085`, `HRD-DEC-089` |

Butir yang dipakai ulang: `WfpSalaryAssignment : ViewAmount` untuk nominal gaji pada data cetak kontrak
(`HRD-DEC-033`); butir kotak masuk persetujuan yang sudah ada untuk keputusan `T15` s.d. `T18`.

### 13.2 Peta peran ke butir hak akses

| Peran | Butir | Batas tambahan |
| --- | --- | --- |
| **HR Admin** (petugas HRD) | Kasus `Read`, `CompleteItem`, `Reassign`, `DeclareReady`, `CancelJoin`; template seluruh action; kontrak `Read`, `MarkSigned`, `UploadDocument`, `RequestCorrection`; keputusan `Read`, `RecordNotification`; masa percobaan `Read`; pendampingan `Read`; batch seluruh action; permintaan akses `Read`, `Retry`; daftar kerja `Read`; pengaturan `Read` | Penyetuju `T15` s.d. `T18` sebagai `SiteHr`, **tidak** atas pengajuannya sendiri |
| **HR Manager** | Seluruh butir HR Admin; pengaturan `Update` | Melihat kategori eskalasi pada daftar kerja |
| **Kepala Unit** | Kasus `Read`, `CompleteUnitItem`; kontrak `Read`; keputusan `Read`, `Submit`; masa percobaan `Read`, `Submit`; pendampingan `Read`, `Manage`; daftar kerja `Read` | **Hanya unit dalam cakupan organisasinya**; tanpa `ViewAmount` |
| **Atasan langsung** (bukan Kepala Unit) | Keputusan `Read`, `Submit`; masa percobaan `Read`, `Submit`; daftar kerja `Read` | Hanya untuk bawahan langsung aktif (`WfpManagerAssignment`), dijaga aturan bisnis 13.3 |
| **Administrator / IT** | Permintaan akses `Read`, `Complete` | Tidak melihat kasus maupun kontrak. `MAPPING_REQUIRED` |
| **Auditor** | Seluruh butir `Read` di atas | Tidak boleh memegang action selain `Read` |
| **Petugas payroll** | **Tidak ada** | Payroll membaca lewat service (`INT-HR-33`) |
| **Pegawai** (layanan mandiri) | **Tidak ada** pada slice ini | Akun aktif untuk layanan mandiri sejak hadir hari pertama |

### 13.3 Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga di aturan bisnis | Yang dijaganya | Yang **tidak** dijaganya | Risiko bila hilang |
| --- | --- | --- | --- |
| Penanggung jawab butir (`ResponsibleRole`) dan jalur endpoint yang terpisah | Kepala Unit tidak mencentang butir HRD, dan sebaliknya | Petugas yang memegang kedua butir sekaligus | Butir orientasi umum dicentang orang yang tidak memberi orientasi |
| Cakupan unit lewat `OrganizationAuthorizationProjectionService` | Kepala ICU hanya melihat pegawai baru ICU | Penempatan Kepala Unit yang salah (`HRD-TF-021`) | Kepala Unit melihat kontrak unit lain |
| Atasan langsung aktif (`WfpManagerAssignment`) sebagai pengaju alternatif | Atasan yang bukan Kepala Unit dapat menilai bawahannya | Relasi atasan yang salah input | Orang lain menilai masa percobaan |
| Pemisahan pemilih atasan langsung dan penyata siap kerja (`VR-ONB-12`) | Relasi atasan tidak lahir dari satu orang | Dua orang yang bersekongkol | Swa-penetapan atasan |
| `AllowSelfApproval = false` pada `T15` s.d. `T18` | Pengaju tidak memutus pengajuannya | — | Swa-setuju koreksi kontrak atau batch |
| **Siap Mandiri tidak masuk persetujuan** (`CLINICAL_DECISION_NOT_HRD`) | HRD tidak menganulir penilaian klinis | Kepala Unit yang menyatakan siap mandiri tanpa dasar | Dijaga lampiran wajib (`VR-ONB-60`) dan jejak audit |
| Penahan jadwal dibaca 06 (`INT-HR-31`) | Pegawai tanpa kontrak bertanda tangan tidak dijadwalkan | Penjadwalan yang dibuat di luar sistem | Risiko hukum PKWT tak tertulis |

### 13.4 Audit

| Kejadian yang wajib meninggalkan jejak tahan lama | Lapisan |
| --- | --- |
| Setiap perpindahan status kasus, butir, kontrak, masa percobaan, penilaian, keputusan, koreksi, batch, pendampingan, dan permintaan akses | Custom logger (selain `GET`), ditambah kolom pelaku dan waktu pada tabelnya |
| Penandatanganan kontrak | `SignedRecordedAt`, `SignedRecordedByUserId`, scan pada `DocumentPath` |
| Penempatan dan gaji awal dari tawaran | Jejak audit menyimpan nomor tawaran sebagai rujukan persetujuan (`02-backend-architecture.md` 18.9.6) |
| Koreksi kontrak | Nilai `Before*` dan `After*` beserta scan lama dan baru; tidak pernah dihapus |
| Pergantian preceptor | Baris `HrdClinicalPreceptorshipPreceptor` |
| Kontrak hasil batch data awal | `HistoryType = InitialData` beserta nomor batch, pembuat, dan penyetuju (`HRD-DEC-090` butir 9) |
| Pembacaan nominal gaji pada data cetak | Audit `GET` sensitif `HRD-DEC-039`; nominal **MUST NOT** masuk catatan |
| Perubahan pengaturan | Custom logger dengan nilai lama dan baru |

Payload logger hanya memuat `EntityId`, controller, action, dan status. Kolom bertanda sensitif pada
`data/data-dictionary.md` bagian 10.16 **MUST NOT** masuk logger.


---

## 14. Matriks Kewenangan dan Audit Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |

### 14.1 Resource dan Permission String

| Resource | Action | String atribut persis | Digunakan pada tindakan |
| --- | --- | --- | --- |
| `HrdScheduling` | `Read` | `[AccessPermission("HrdScheduling", "Read")]` | Melihat kisi kalender roster unit, riwayat mutasi, dan hasil validasi |
| `HrdScheduling` | `Create` | `[AccessPermission("HrdScheduling", "Create")]` | Membuat draft periode baru, salin periode lalu, terapkan pola shift |
| `HrdScheduling` | `Update` | `[AccessPermission("HrdScheduling", "Update")]` | Menyusun kotak shift, ganti petugas mendadak, koreksi mundur 3 hari |
| `HrdScheduling` | `Publish` | `[AccessPermission("HrdScheduling", "Publish")]` | Menerbitkan roster bulanan unit secara mandiri (`HRD-DEC-098`) |
| `HrdScheduling` | `Close` | `[AccessPermission("HrdScheduling", "Close")]` | Menutup dan mengunci mutlak periode roster saat cut-off payroll (`HRD-DEC-102`) |
| `HrdOnCall` | `Read` | `[AccessPermission("HrdOnCall", "Read")]` | Melihat jadwal siaga dokter spesialis dan staf penunjang |
| `HrdOnCall` | `Create` | `[AccessPermission("HrdOnCall", "Create")]` | Membuat penugasan siaga dokter/staf cito |
| `HrdOnCall` | `Update` | `[AccessPermission("HrdOnCall", "Update")]` | Mengonfirmasi siaga, mencatat pemanggilan (*call-in*) & waktu kedatangan |
| `HrdSchedulingSetting` | `Manage` | `[AccessPermission("HrdSchedulingSetting", "Manage")]` | Mengonfigurasi parameter jeda 11 jam, toleransi 3 hari, dan eskalasi aturan |

### 14.2 Matriks Peran x Tindakan

| Tindakan | Kepala Unit (Sari) | Kepala Bidang (Pak Dodi) | Pegawai (Budi/Rina) | Unit Pemanggil (Lab/IGD/OK) | HR Scheduling / Payroll |
| --- | :---: | :---: | :---: | :---: | :---: |
| Buat draft roster unit | **Boleh** (unit sendiri) | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (bila kepala unit cuti) |
| Salin periode & terapkan pola | **Boleh** | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** |
| Terbitkan roster bulanan | **Boleh** | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (atas nama unit) |
| Pantau roster terbit | **Boleh** | **Boleh** (seluruh unit subordinat) | **Boleh** (jadwal sendiri) | **Boleh** (staf terkait) | **Boleh** (seluruh RS) |
| Ganti Petugas (*Override*) | **Boleh** | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** |
| Koreksi mundur (<= 3 hari) | **Boleh** | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** |
| Koreksi mundur (> 3 hari) | Ditolak (403) | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (verifikasi khusus) |
| Tutup periode (*Close Lock*) | Ditolak (403) | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (Payroll Officer) |
| Susun siaga staf penunjang | **Boleh** | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** |
| Susun siaga dokter spesialis | Ditolak (403) | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (Pelayanan Medis/KSM) |
| Catat pemanggilan siaga cito | **Boleh** | Ditolak (403) | Ditolak (403) | **Boleh** (Analis/Perawat Jaga) | **Boleh** |
| Ubah parameter kerja HR | Ditolak (403) | Ditolak (403) | Ditolak (403) | Ditolak (403) | **Boleh** (HR Admin) |

### 14.3 Jejak Audit Tahan Lama

| Kejadian | Yang dicatat dalam Audit Trail | Tabel penyimpanan |
| --- | --- | --- |
| Penerbitan Roster | User ID penerbit, waktu terbit, total staf, daftar peringatan dan alasan yang dipilih | `TrxRosterPublication` & Custom Logger |
| Penggantian Shift Mendadak | User ID eksekutor, waktu eksekusi, pegawai asal, pegawai pengganti, kategori alasan, catatan | `TrxShiftReplacement` & Custom Logger |
| Koreksi Mundur Jadwal | User ID eksekutor, waktu koreksi, tanggal shift mundur yang diubah, selisih hari, alasan koreksi | Custom Logger & event `RetroactiveShiftCorrectedEvent` |
| Penguncian Periode Roster | User ID HRD Payroll, waktu penutupan, periode ID | `TrxRosterPeriod.ClosedAt`, `ClosedByUserId` |
| Pemanggilan Siaga Darurat | Unit pemanggil, user ID pemanggil, waktu telepon (*CallInTime*), waktu tiba (*ArrivalTime*), selisih menit, status SLA | `TrxOnCallAssignment` & Custom Logger |
| Perubahan Parameter Penjadwalan | User ID HR Admin, waktu ubah, nilai parameter lama, nilai parameter baru | `HrdSchedulingSetting` & Custom Logger |


---

## 15. Bagian `S-B1` — Matriks Hak Akses dan Audit Trail Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

### 15.1 Matriks Hak Akses Persis (Permission Matrix)

| Resource | Action | String Hak Akses Persis | Peran yang Memiliki | Kegunaan Bisnis |
| --- | --- | --- | --- | --- |
| `AttendanceDaily` | `View` | `[AccessPermission("AttendanceDaily", "View")]` | Pegawai, Kepala Unit, HR Officer, Manager | Melihat matriks kehadiran harian staf |
| `AttendanceDaily` | `ViewRawLog` | `[AccessPermission("AttendanceDaily", "ViewRawLog")]` | HR Officer, Auditor Internal | Membuka drawer riwayat log biometrik mentah |
| `AttendanceDaily` | `Manage` | `[AccessPermission("AttendanceDaily", "Manage")]` | HR Attendance Officer | Memicu hitung ulang presensi harian |
| `AttendanceCorrection` | `View` | `[AccessPermission("AttendanceCorrection", "View")]` | Pegawai, Kepala Ruangan, HR Officer | Melihat daftar pengajuan koreksi presensi |
| `AttendanceCorrection` | `Create` | `[AccessPermission("AttendanceCorrection", "Create")]` | Pegawai (mandiri), HR Admin (on-behalf) | Mengajukan koreksi lupa tap/jam salah |
| `AttendanceCorrection` | `Approve` | `[AccessPermission("AttendanceCorrection", "Approve")]` | Kepala Ruangan / Atasan Langsung | Menyetujui atau menolak koreksi presensi |
| `AttendanceCorrection` | `Apply` | `[AccessPermission("AttendanceCorrection", "Apply")]` | HR Attendance Officer | Menerapkan koreksi dan hitung ulang jam kerja |
| `EarlyLeavePermission` | `View` | `[AccessPermission("EarlyLeavePermission", "View")]` | Pegawai, Kepala Ruangan, HR Officer | Melihat daftar izin pulang cepat |
| `EarlyLeavePermission` | `Create` | `[AccessPermission("EarlyLeavePermission", "Create")]` | Pegawai | Mengajukan izin pulang darurat administratif |
| `EarlyLeavePermission` | `Approve` | `[AccessPermission("EarlyLeavePermission", "Approve")]` | Kepala Ruangan / Atasan Langsung | Menyetujui izin pulang cepat (menandai Waived) |
| `OfficialDuty` | `View` | `[AccessPermission("OfficialDuty", "View")]` | Pegawai, Koordinator Pelayanan, HR | Melihat daftar surat tugas dinas luar |
| `OfficialDuty` | `Create` | `[AccessPermission("OfficialDuty", "Create")]` | Pegawai, Koordinator Pelayanan, HR | Mengajukan surat tugas ambulans/home care |
| `OfficialDuty` | `Approve` | `[AccessPermission("OfficialDuty", "Approve")]` | Atasan Langsung / Kepala Bidang | Menyetujui penugasan dinas luar |
| `OfficialDuty` | `CheckIn` | `[AccessPermission("OfficialDuty", "CheckIn")]` | Pegawai yang ditugaskan | Melakukan tap mobile di lokasi tugas (GPS/foto) |
| `AttendancePeriod` | `View` | `[AccessPermission("AttendancePeriod", "View")]` | HR Officer, HR Manager, Finance | Melihat daftar dan status periode presensi |
| `AttendancePeriod` | `Close` | `[AccessPermission("AttendancePeriod", "Close")]` | HR Attendance Officer | Menutup periode dan mengunci transaksi |
| `AttendancePeriod` | `Reopen` | `[AccessPermission("AttendancePeriod", "Reopen")]` | **HR Manager / Kadiv SDM** | Buka kembali periode darurat & rollback payroll |
| `AttendanceSetting` | `View` | `[AccessPermission("AttendanceSetting", "View")]` | HR Officer, HR Manager | Melihat konfigurasi toleransi keterlambatan |
| `AttendanceSetting` | `Manage` | `[AccessPermission("AttendanceSetting", "Manage")]` | HR Administrator | Mengubah nilai parameter toleransi keterlambatan |

### 15.2 Jejak Audit (Audit Trail Events)

| Peristiwa Bisnis | Tingkat Sensitivitas | Data yang Dicatat dalam Audit Trail |
| --- | :---: | --- |
| `RawAttendanceLog.Ingested` | INFO | DeviceId, WorkforceProfileId, EventAt, SourceType, IngestedAt. |
| `AttendanceDaily.Recalculated` | INFO | AttendanceDailyId, OldStatus, NewStatus, EffectiveWorkMinutes, ActorId. |
| `AttendanceCorrection.Created` | AUDIT | CorrectionId, TargetWorkforceId, IsOnBehalf, InitiatorWorkforceId, Reason, Timestamp. |
| `AttendanceCorrection.Approved` | AUDIT | CorrectionId, ApproverWorkforceId, ApprovedAt, Notes. |
| `EarlyLeavePermission.Approved` | AUDIT | PermissionId, ApproverWorkforceId, RequestedEarlyLeaveAt, IsDeductible = false. |
| `OfficialDutyRequest.Approved` | AUDIT | OfficialDutyId, ApproverWorkforceId, StartAt, EndAt, DutyCategory. |
| `AttendancePeriod.Closed` | CRITICAL | PeriodId, ClosedByWorkforceId, ClosedAt, TotalPresent, TotalLate, TotalAbsent. |
| `AttendancePeriod.Reopened` | **SECURITY CRITICAL** | PeriodId, ReopenedByWorkforceId, ReopenReason (wajib), RolledBackPayrollRunId, Timestamp. |


---

## 16. Bagian `S-B2` — Matriks Hak Akses dan Audit Trail Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Bagian ini mendokumentasikan spesifikasi hak akses berbasis peran (*Role-Based Access Control*) dan konfigurasi pencatatan jejak audit (*Audit Trail Events*) untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**.

### 16.1 Matriks Hak Akses (Permission Matrix)

| Resource | Action | String Permission Persis | Peran Pengguna yang Berhak | Keterangan & Batas Operasional |
| --- | --- | --- | --- | --- |
| `LeaveBalance` | `ViewSelf` | `[AccessPermission("LeaveBalance", "ViewSelf")]` | Seluruh Pegawai RS | Melihat saldo cuti pribadi dan mutasi ledger mandiri |
| `LeaveBalance` | `Manage` | `[AccessPermission("LeaveBalance", "Manage")]` | HR Leave Officer, HR Admin | Melihat saldo seluruh staf RS dan menjalankan runner kuota carry-forward |
| `LeaveRequest` | `ViewSelf` | `[AccessPermission("LeaveRequest", "ViewSelf")]` | Seluruh Pegawai RS | Melihat riwayat pengajuan cuti pribadi |
| `LeaveRequest` | `View` | `[AccessPermission("LeaveRequest", "View")]` | Kepala Ruangan, HR Officer, Manajer | Melihat rincian permohonan cuti bawahan atau staf unit |
| `LeaveRequest` | `Create` | `[AccessPermission("LeaveRequest", "Create")]` | Seluruh Pegawai RS | Mengajukan cuti mandiri, half-day, dan cuti sakit |
| `LeaveRequest` | `CancelSelf` | `[AccessPermission("LeaveRequest", "CancelSelf")]` | Seluruh Pegawai RS | Membatalkan permohonan cuti sebelum hari pelaksanaan |
| `LeaveApproval` | `View` | `[AccessPermission("LeaveApproval", "View")]` | Kepala Ruangan, Kepala Instalasi, Manajer | Mengakses kotak masuk persetujuan cuti bawahan berindikator kuota |
| `LeaveApproval` | `Approve` | `[AccessPermission("LeaveApproval", "Approve")]` | Kepala Ruangan, Kepala Instalasi, Manajer | Menyetujui, menyetujui dengan mitigasi, atau menolak cuti bawahan |
| `LeaveRecall` | `View` | `[AccessPermission("LeaveRecall", "View")]` | Kepala Ruangan, HR Manager, Direktur Pelayanan | Melihat daftar kasus pemanggilan darurat staf cuti |
| `LeaveRecall` | `Create` | `[AccessPermission("LeaveRecall", "Create")]` | Kepala Ruangan, Kepala Instalasi | Menginisiasi pemanggilan staf cuti saat darurat RS (`HRD-DEC-113`) |
| `LeaveRecall` | `Approve` | `[AccessPermission("LeaveRecall", "Approve")]` | HR Manager | Otorisasi tunggal persetujuan pemanggilan darurat & restore saldo |
| `LeaveAdjustment` | `Execute` | `[AccessPermission("LeaveAdjustment", "Execute")]` | HR Manager | Melakukan koreksi saldo resmi berdokumen audit (`HRD-DEC-115`) |
| `LeaveReconciliation`| `View` | `[AccessPermission("LeaveReconciliation", "View")]` | HR Leave Officer, HR Payroll Officer | Melihat kesiapan data cuti bulanan dan cuti pending |
| `LeaveReconciliation`| `Close`| `[AccessPermission("LeaveReconciliation", "Close")]`| HR Payroll Officer, HR Manager | Mengunci periode cuti dan menyerahkan snapshot ke Payroll |
| `MedicalCertificate` | `ViewConfidential` | `[AccessPermission("MedicalCertificate", "ViewConfidential")]` | Dokter Pemeriksa K3RS, HR Medis | Mengakses diagnosa penyakit staf dan berkas rekam medis K3RS (`HRD-DEC-112`) |
| `LeaveExecution` | `Reverse` | `[AccessPermission("LeaveExecution", "Reverse")]` | System Admin (Guarded) | **DITOLAK OTOMATIS** jika status `Completed` / `PayrollClosed` (`HRD-DEC-115`) |

### 16.2 Matriks Jejak Audit Trail (Audit Trail Events)

| Kode Event Audit | Tingkat Kepentingan | Data yang Dicatat | Pemicu Peristiwa |
| --- | :---: | --- | --- |
| `LEAVE_REQUEST_SUBMITTED` | `INFO` | `WorkforceId`, `LeaveTypeId`, `StartDate`, `EndDate`, `DurationDays`, `IsHalfDay` | Pegawai submit permohonan cuti ESS |
| `LEAVE_REQUEST_APPROVED` | `INFO` | `LeaveRequestId`, `ApprovedByWorkforceId`, `HasStaffingWarning = false` | Atasan menyetujui cuti berkuota aman |
| `LEAVE_REQUEST_APPROVED_WITH_MITIGATION` | `WARNING` | `LeaveRequestId`, `ApprovedByWorkforceId`, `CoverageWarningDetails`, `MitigationNotes` | Atasan menyetujui cuti dengan mitigasi kuota staf kritis |
| `LEAVE_REQUEST_REJECTED` | `INFO` | `LeaveRequestId`, `RejectedByWorkforceId`, `RejectionReason` | Atasan menolak permohonan cuti |
| `LEAVE_PRE_CANCELLED` | `INFO` | `LeaveRequestId`, `WorkforceId`, `RestoredDays`, `LedgerTransactionId` | Pembatalan mandiri sebelum hari-H memulihkan saldo 100% |
| `LEAVE_RECALL_INITIATED` | `WARNING` | `LeaveRequestId`, `RequestedByHeadUnitId`, `EmergencyCategory`, `EffectiveRecallDate` | Inisiasi recall darurat oleh Kepala Unit |
| `LEAVE_RECALL_APPROVED` | `CRITICAL` | `LeaveRecallId`, `ApprovedByHrManagerId`, `RestoredDays`, `LedgerTransactionId` | Persetujuan resmi recall darurat oleh HR Manager |
| `LEAVE_BALANCE_FORFEITED` | `WARNING` | `WorkforceProfileId`, `LeaveTypeId`, `ForfeitedDays`, `Reason: CarryForwardExpiry` | Scheduler menghanguskan sisa bawaan per 30 Juni |
| `LEAVE_BALANCE_ADJUSTED` | `CRITICAL` | `WorkforceProfileId`, `AdjustmentDays`, `AuditDocumentNumber`, `AdjustedByHrManagerId` | Eksekusi penyesuaian saldo resmi HR pasca-payroll |
| `LEAVE_PERIOD_CLOSED` | `INFO` | `PeriodYear`, `PeriodMonth`, `ClosedByWorkforceId`, `TotalApprovedRequests` | Penutupan periode cuti bulanan |
| `LEAVE_REVERSE_ATTEMPT_BLOCKED` | `SECURITY` | `LeaveRequestId`, `CallerUserId`, `AttemptedAction: /reverse`, `Reason: CompletedImmutable` | Percobaan pembalikan bebas yang digagalkan guard |


---

## 17. Bagian `S-B3` — Matriks Hak Akses & Audit Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Matriks ini mengunci seluruh string penanda hak akses API (`[AccessPermission("Resource", "Action")]`), peran pengguna yang berhak, serta daftar peristiwa audit kepatuhan (*audit trail events*) untuk **Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit**.

### 17.1 Matriks Hak Akses (Permissions)

| Resource | Action | String Permission Persis | Peran Pengguna yang Berhak | Keterangan & Batas Operasional |
| --- | --- | --- | --- | --- |
| `OvertimePlan` | `ViewSelf` | `[AccessPermission("OvertimePlan", "ViewSelf")]` | Seluruh Pegawai RS | Melihat riwayat pengajuan SPKL terencana pribadi |
| `OvertimePlan` | `View` | `[AccessPermission("OvertimePlan", "View")]` | Kepala Ruangan, HR Officer, Manajer | Melihat rincian SPKL staf bawahan atau unit kerja |
| `OvertimePlan` | `Create` | `[AccessPermission("OvertimePlan", "Create")]` | Seluruh Pegawai RS, Ka. Ruangan | Mengajukan SPKL pra-dinas mandiri atau SPKL kelompok tim |
| `OvertimePlan` | `CancelSelf` | `[AccessPermission("OvertimePlan", "CancelSelf")]` | Seluruh Pegawai RS | Membatalkan rencana SPKL sebelum jam dinas dimulai |
| `OvertimeRealization`| `ViewSelf` | `[AccessPermission("OvertimeRealization", "ViewSelf")]` | Seluruh Pegawai RS | Melihat daftar klaim realisasi lembur pribadi |
| `OvertimeRealization`| `View` | `[AccessPermission("OvertimeRealization", "View")]` | Kepala Ruangan, HR Officer | Melihat rincian klaim aktual dan bukti rekam medis |
| `OvertimeRealization`| `Create` | `[AccessPermission("OvertimeRealization", "Create")]` | Seluruh Pegawai RS | Melaporkan realisasi dinas aktual atau klaim darurat cito H+1 |
| `OvertimeApproval` | `View` | `[AccessPermission("OvertimeApproval", "View")]` | Kepala Ruangan, Kepala Instalasi | Mengakses kotak masuk persetujuan lembur unit |
| `OvertimeApproval` | `Approve` | `[AccessPermission("OvertimeApproval", "Approve")]` | Kepala Ruangan, Kepala Instalasi | Menyetujui atau menolak SPKL (Ditolak mutlak jika self-approval) |
| `OvertimeApproval` | `MedicalDirectorApprove`| `[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]` | Direktur Pelayanan Medik | Otorisasi eskalasi khusus darurat cito yang melampaui kuota PP 35 |
| `OvertimeVerification`| `View` | `[AccessPermission("OvertimeVerification", "View")]` | HR Officer, HR Admin | Melihat hasil komparasi klaim vs mesin presensi |
| `OvertimeVerification`| `Manage` | `[AccessPermission("OvertimeVerification", "Manage")]` | HR Officer, HR Admin | Menyelesaikan anomali discrepancy dan input penyesuaian manual |
| `OvertimeCompensatoryLeave`| `ViewSelf` | `[AccessPermission("OvertimeCompensatoryLeave", "ViewSelf")]` | Seluruh Pegawai RS | Melihat tiket dan saldo cuti pengganti pribadi |
| `OvertimeCompensatoryLeave`| `Manage` | `[AccessPermission("OvertimeCompensatoryLeave", "Manage")]` | HR Leave Admin, HR Officer | Monitoring seluruh saldo CTO RS dan menjalankan runner hangus |
| `OvertimePeriod` | `View` | `[AccessPermission("OvertimePeriod", "View")]` | HR Officer, HR Payroll Officer | Melihat informasi periode berjalan dan tanggal cut-off |
| `OvertimePeriod` | `Manage` | `[AccessPermission("OvertimePeriod", "Manage")]` | HR Payroll Officer, HR Manager | Mengunci snapshot cut-off bulanan dan rekonsiliasi retrospektif |
| `OvertimePeriod` | `HandoffPayroll` | `[AccessPermission("OvertimePeriod", "HandoffPayroll")]` | HR Payroll Officer | Mengeksekusi serah terima data lembur siap bayar ke Modul 10 |
| `OvertimeSetting` | `Manage` | `[AccessPermission("OvertimeSetting", "Manage")]` | HR Director, System Administrator | Mengonfigurasi kuota PP 35/2021, rumus pengali, dan batas waktu |

---

### 17.2 Matriks Jejak Audit Trail (Audit Trail Events)

| Kode Event Audit | Tingkat Kepentingan | Data yang Dicatat | Pemicu Peristiwa |
| --- | :---: | --- | --- |
| `OVERTIME_PLAN_CREATED` | `INFO` | `PlanId`, `UnitId`, `PlanDate`, `EstimatedMinutes`, `CompensationType` | Pengajuan SPKL pra-dinas terencana |
| `OVERTIME_PLAN_APPROVED` | `INFO` | `PlanId`, `ApprovedBySupervisorId`, `WeeklyQuotaHours` | Persetujuan SPKL oleh Kepala Ruangan |
| `OVERTIME_CITO_CLAIM_SUBMITTED` | `WARNING` | `RealizationId`, `WorkforceId`, `EmergencyClinicalRefNo`, `ClaimedMinutes` | Pelaporan klaim lembur darurat medis cito H+1 |
| `OVERTIME_MEDICAL_DIRECTOR_ESCALATED`| `CRITICAL` | `RealizationId`, `WorkforceId`, `ExcessHours`, `Reason: QuotaPP35Exceeded` | Klaim cito dialihkan ke Direktur Pelayanan Medik |
| `OVERTIME_MEDICAL_DIRECTOR_APPROVED` | `CRITICAL` | `RealizationId`, `ApprovedByDirectorId`, `DispensasiHours`, `MitigationNotes`| Persetujuan darurat khusus oleh Direktur Pelayanan Medik |
| `OVERTIME_BIOMETRIC_AUTO_VERIFIED` | `INFO` | `RealizationId`, `VerifiedPayableMinutes`, `BiometricExcessMinutes` | Verifikasi mesin presensi sukses tanpa selisih |
| `OVERTIME_DISCREPANCY_FLAGGED` | `WARNING` | `RealizationId`, `DiscrepancyType`, `ClaimedMinutes`, `ActualTapMinutes` | Deteksi anomali selisih waktu > 30 menit / lupa tap |
| `OVERTIME_DISCREPANCY_MANUALLY_ADJUSTED`| `WARNING`| `RealizationId`, `AdjustedByHrAdminId`, `VerifiedMinutes`, `LogbookDocNo` | Penyelesaian anomali secara manual berdasar logbook fisik |
| `OVERTIME_CTO_GRANTED` | `INFO` | `WorkforceProfileId`, `GrantedHours`, `GrantedDays`, `ExpiryDate` | Posting mutasi kredit hak cuti pengganti ke Sub-modul 08 |
| `OVERTIME_CTO_FORFEITED` | `WARNING` | `CtoTicketId`, `WorkforceProfileId`, `ForfeitedHours`, `ExpiryDate` | Scheduler menghanguskan sisa CTO setelah 90 hari kalender |
| `OVERTIME_PERIOD_CUTOFF_LOCKED` | `INFO` | `PeriodYear`, `PeriodMonth`, `CutoffDate`, `TotalVerifiedRequests` | Penguncian snapshot transaksi lembur bulanan (tgl 20) |
| `OVERTIME_PAYROLL_HANDOFF_EXECUTED`| `CRITICAL` | `PeriodYear`, `PeriodMonth`, `TargetBatchId`, `TotalPayableHours` | Serah terima resmi data lembur ke modul 10 Payroll |
| `OVERTIME_SELF_APPROVAL_ATTEMPT_BLOCKED`| `SECURITY`| `PlanId`, `CallerUserId`, `AttemptedAction: Approve`, `Reason: SelfApproval` | Percobaan persetujuan lembur sendiri digagalkan sistem |

---

## 18. Bagian `S-B5` — Matriks Hak Akses & Audit Penggajian Pegawai Rumah Sakit (Sub-modul 10, `last_changed_in` `v14`, 5 November 2026)

Matriks ini mengunci seluruh string penanda izin akses API (`[AccessPermission("Resource", "Action")]`), peran pengguna yang berhak, serta daftar peristiwa audit kepatuhan (*audit trail events*) untuk **Slice `S-B5` — Penggajian Pegawai Rumah Sakit**.

### 18.1 Matriks Hak Akses (Permissions)

| Resource | Action | String Permission Persis | Peran Pengguna yang Berhak | Keterangan & Batas Operasional |
| :--- | :--- | :--- | :--- | :--- |
| `PayrollRun` | `View` | `[AccessPermission("PayrollRun", "View")]` | Payroll Officer, HR Manager, Direksi | Melihat daftar siklus dan ringkasan eksekutif payroll run |
| `PayrollRun` | `Create` | `[AccessPermission("PayrollRun", "Create")]` | Payroll Officer | Membuka dan menginisiasi siklus penggajian bulanan baru |
| `PayrollRun` | `Cancel` | `[AccessPermission("PayrollRun", "Cancel")]` | Payroll Officer, HR Manager | Membatalkan draf siklus penggajian sebelum diajukan |
| `PayrollCalculation` | `Execute` | `[AccessPermission("PayrollCalculation", "Execute")]` | Payroll Officer | Memicu eksekusi kalkulasi formula pendapatan dan potongan |
| `PayrollCalculation` | `ViewWorksheet`| `[AccessPermission("PayrollCalculation", "ViewWorksheet")]`| Payroll Officer, HR Manager | Membuka lembar kerja audit individual gaji pegawai |
| `PayrollCalculation` | `ViewDetail` | `[AccessPermission("PayrollCalculation", "ViewDetail")]`| Payroll Officer, HR Manager | Melihat rincian breakdown formula komponen per pegawai |
| `PayrollCalculation` | `Submit` | `[AccessPermission("PayrollCalculation", "Submit")]` | Payroll Officer | Mengajukan draf gaji terjustifikasi ke HR Manager |
| `PayrollVariance` | `Justify` | `[AccessPermission("PayrollVariance", "Justify")]` | Payroll Officer | Mengisi catatan justifikasi resmi atas deviasi variansi $\ge 20\%$ |
| `PayrollApproval` | `View` | `[AccessPermission("PayrollApproval", "View")]` | HR Manager, Direktur SDM, Direktur Keuangan | Mengakses kotak masuk persetujuan siklus penggajian |
| `PayrollApproval` | `Review` | `[AccessPermission("PayrollApproval", "Review")]` | HR Manager | Menyetujui hasil tinjauan administratif Tahap 2 |
| `PayrollApproval` | `AuthorizeFinal`| `[AccessPermission("PayrollApproval", "AuthorizeFinal")]`| Direktur SDM, Direktur Keuangan | Memberikan otorisasi final Tahap 3 & penguncian mutlak (*Final Lock*) |
| `PayrollHandoff` | `Dispatch` | `[AccessPermission("PayrollHandoff", "Dispatch")]` | Payroll Officer, HR Manager | Membentuk berkas serah terima terenkripsi ke Finance |
| `PayrollHandoff` | `View` | `[AccessPermission("PayrollHandoff", "View")]` | Payroll Officer, HR Manager, Finance User | Memantau status berkas, tanda terima, dan konfirmasi Finance |
| `PayrollHandoff` | `HandleRejection`| `[AccessPermission("PayrollHandoff", "HandleRejection")]`| Finance Manager, HR Manager | Menangani pembukaan kunci terkendali saat batch ditolak Finance |
| `PayrollHandoff` | `ConfirmPayment`| `[AccessPermission("PayrollHandoff", "ConfirmPayment")]`| Treasury / Kasir Finance | Menerbitkan konfirmasi transfer kliring bank sukses diselesaikan |
| `SelfServicePayroll` | `ViewSelf` | `[AccessPermission("SelfServicePayroll", "ViewSelf")]` | Seluruh Staf Rumah Sakit | Membuka riwayat slip gaji pribadi di portal ESS |
| `SelfServicePayroll` | `VerifyPin` | `[AccessPermission("SelfServicePayroll", "VerifyPin")]` | Seluruh Staf Rumah Sakit | Autentikasi sekunder PIN 6-digit untuk membuka lembar gaji |
| `SelfServicePayroll` | `DownloadPdf` | `[AccessPermission("SelfServicePayroll", "DownloadPdf")]`| Seluruh Staf Rumah Sakit | Mengunduh berkas PDF slip gaji resmi ber-password |
| `PayrollSetting` | `View` | `[AccessPermission("PayrollSetting", "View")]` | HR Officer, HR Manager, Admin | Membaca parameter kebijakan cut-off dan tarif BPJS |
| `PayrollSetting` | `Manage` | `[AccessPermission("PayrollSetting", "Manage")]` | HR Director, System Administrator | Memperbarui parameter operasional penggajian rumah sakit |

### 18.2 Matriks Jejak Audit Trail (Audit Trail Events)

| Kode Event Audit | Tingkat Kepentingan | Data yang Dicatat | Pemicu Peristiwa |
| :--- | :---: | :--- | :--- |
| `PAYROLL_RUN_INITIALIZED` | `INFO` | `RunId`, `PeriodId`, `CutoffDate`, `InitiatedByUserId` | Inisiasi pembukaan siklus penggajian baru |
| `PAYROLL_INPUT_SNAPSHOT_FROZEN` | `INFO` | `RunId`, `TotalAttendanceInputs`, `TotalOvertimeInputs`, `TotalLeaveInputs` | Pembekuan snapshot data masukan dari Sub-07, 08, 09 |
| `PAYROLL_CALCULATION_EXECUTED` | `INFO` | `RunId`, `TotalEmployees`, `GrossPayAmount`, `NetPayAmount` | Eksekusi kalkulasi agregat formula gaji seluruh staf |
| `PAYROLL_VARIANCE_ALERT_TRIGGERED` | `WARNING` | `RunId`, `EmployeeId`, `DeviationPercent`, `NetPayDiff` | Deteksi otomatis lonjakan gaji $\ge 20\%$ vs bulan lalu |
| `PAYROLL_VARIANCE_JUSTIFIED` | `INFO` | `RunId`, `AlertId`, `JustifiedByUserId`, `ReasonText` | Pengisian catatan penjelasan formal variansi anomali |
| `PAYROLL_SUBMITTED_FOR_REVIEW` | `INFO` | `RunId`, `SubmittedByUserId`, `TotalGrossPay` | Pengajuan paket penggajian ke tahap review HR Manager |
| `PAYROLL_HR_REVIEW_APPROVED` | `INFO` | `RunId`, `ReviewedByManagerId`, `SamplingAuditNotes` | Persetujuan administratif Tahap 2 oleh HR Manager |
| `PAYROLL_FINAL_AUTHORIZED_LOCKED` | `CRITICAL` | `RunId`, `AuthorizedByDirectorId`, `TotalNetPay`, `Checksum` | Otorisasi final Direksi & penguncian immutable data |
| `PAYROLL_SELF_APPROVAL_BLOCKED` | `SECURITY` | `RunId`, `CallerUserId`, `AttemptedAction`, `Reason: SelfApproval` | Percobaan persetujuan paket sendiri digagalkan sistem |
| `PAYROLL_FINANCE_HANDOFF_DISPATCHED` | `CRITICAL` | `RunId`, `BatchNumber`, `IntegrityChecksumSha256`, `RecipientDept` | Penyerahan paket data terenkripsi ke Departemen Keuangan |
| `PAYROLL_FINANCE_REJECTION_RECORDED` | `WARNING` | `RunId`, `BatchNumber`, `FinanceRejectorId`, `RejectionReason` | Sinyal penolakan resmi Finance & pembukaan kunci terkendali |
| `PAYROLL_PAYMENT_CONFIRMED` | `CRITICAL` | `RunId`, `BankDisbursementRefNo`, `ConfirmedByFinanceId` | Konfirmasi transfer kliring bank sukses & picu rilis ESS |
| `PAYROLL_ESS_PAYSLIP_ACCESSED` | `INFO` | `PayslipId`, `EmployeeId`, `SessionIp`, `AuthMethod: PIN` | Pembukaan lembar rincian slip gaji mandiri di ESS |
| `PAYROLL_FAILED_PIN_ATTEMPT_LOCKED` | `SECURITY` | `EmployeeId`, `FailedAttempts: 3`, `LockoutDuration: 30m` | Pembekuan akses slip gaji akibat 3 kali salah input PIN |

## 19. Bagian `S-B6` — Matriks Hak Akses & Audit Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, `last_changed_in` `v15`, 5 November 2026)

Bagian ini mendefinisikan matriks hak akses berbasis peran (*Role-Based Access Control* - RBAC) dan jejak audit tak-terbantahkan (*Immutable Audit Trail*) untuk **Sub-modul 11 (Restitution & Payroll Deduction Integration — Slice `S-B6`)**.

---

### 19.1 Matriks Butir Hak Akses (Permissions)

| String Hak Akses Persis | Modul / Sumber Daya | Aksi Diizinkan | Peran yang Berhak (Role Mapping) | Deskripsi Fungsional Hak Akses |
| :--- | :--- | :--- | :--- | :--- |
| `[AccessPermission("ESS_Restitution", "View")]` | ESS Portal | Baca | Seluruh Pegawai RS (Nakes & Non-Nakes) | Melihat histori klaim pribadi, progress approval, dan sisa kuota plafon tahunan |
| `[AccessPermission("ESS_Restitution", "Create")]` | ESS Portal | Buat | Seluruh Pegawai RS (Masa Kerja $\ge$ Min) | Mengisi formulir klaim kacamata/medis dan mengunggah pindaian kuitansi asli |
| `[AccessPermission("HR_Deduction", "View")]` | Modul HRD | Baca | HR Officer, Payroll Specialist, Finance Reviewer | Membuka dasbor pemotongan, daftar jadwal cicilan, dan monitoring rasio upah 50% |
| `[AccessPermission("HR_Deduction", "Manage")]` | Modul HRD | Kelola | Payroll Specialist, HR C&B Lead | Memicu sinkronisasi Finance H-3, simulasi batas 50%, dan menjeda jadwal cicilan |
| `[AccessPermission("HR_Deduction", "Approve")]` | Modul HRD | Setujui | Kepala Ruangan, HR C&B Lead, Manajer SDM, Direktur | Menyetujui klaim restitusi bertingkat dan menerbitkan voucher kasir cito 24/7 |
| `[AccessPermission("HR_Deduction", "Correct")]` | Modul HRD | Koreksi | HR Payroll Specialist, Manajer Keuangan | Menerbitkan transaksi kompensasi salah potong periode lampau (`PAY-B6`) |
| `[AccessPermission("HR_Deduction", "Audit")]` | Modul HRD | Audit | Tim SPI (Internal Audit), Manajer Kepatuhan RS | Mengunduh berkas jejak audit pemotongan berjenjang dan log penundaan tagihan |
| `[AccessPermission("Cashier_Desk", "Verify")]` | Loket Kasir RS | Verifikasi | Petugas Loket Kasir RS 24/7 | Melakukan pemindaian QR code / verifikasi token voucher darurat cito di loket |
| `[AccessPermission("Cashier_Desk", "Disburse")]`| Loket Kasir RS | Eksekusi | Petugas Loket Kasir RS 24/7, Supervisor Kasir | Mengonfirmasi penyerahan fisik uang tunai dan mengunci status `DisbursedViaCashier` |
| `[AccessPermission("HR_Setting", "View")]` | Pengaturan | Baca | HR Officer, Auditor Internal | Melihat tabel konfigurasi plafon tahunan dan persentase batasan pemotongan upah |
| `[AccessPermission("HR_Setting", "Manage")]` | Pengaturan | Kelola | Direktur SDM & Umum | Memperbarui nominal plafon tahunan per grade dan mengubah toleransi batas sistem |

---

### 19.2 Matriks Jejak Audit Tak-Terbantahkan (Audit Events)

Seluruh peristiwa transaksi penting dicatat pada penyimpanan log audit tak-terbantahkan (*append-only audit log*):

| Kode Peristiwa | Nama Peristiwa Audit | Pemicu (Trigger) | Data Kunci yang Direkam | Retensi Log |
| :--- | :--- | :--- | :--- | :---: |
| `AUDIT-DED-01` | *Finance Snapshot Synchronized* | Penarikan jadwal cicilan H-3 dari modul Keuangan | Waktu sinkronisasi, jumlah kontrak aktif diterima, total nilai piutang, status respons | 10 Tahun |
| `AUDIT-DED-02` | *Restitution Claim Submitted* | Pegawai submit klaim mandiri di ESS | ID Pegawai, nomor klaim, kategori manfaat, nominal diajukan, hash berkas kuitansi | 10 Tahun |
| `AUDIT-DED-03` | *Supervisor Claim Approved* | Persetujuan tahap 1 oleh Atasan Langsung | ID Atasan, ID Klaim, waktu persetujuan, catatan rekomendasi kedinasan | 10 Tahun |
| `AUDIT-DED-04` | *HR Physical Receipt Verified* | Verifikasi kuitansi nota oleh HR C&B | ID Petugas C&B, nomor seri kuitansi fisik, cap stempel faskes luar terverifikasi | 10 Tahun |
| `AUDIT-DED-05` | *Manager Final Claim Authorized* | Otorisasi final oleh Manajer SDM | ID Manajer, nominal akhir disetujui, kanal pencairan terpilih (Payroll / Kasir) | 10 Tahun |
| `AUDIT-DED-06` | *Cito Cashier Voucher Issued* | Penerbitan voucher kasir darurat 24/7 | Nomor voucher, ID Pegawai nakes, nominal tunai, hash token SHA-256, batas kedaluwarsa | 10 Tahun |
| `AUDIT-DED-07` | *Cashier Physical Fund Disbursed* | Kasir menyerahkan dana tunai di loket | ID Kasir, nomor tanda terima kasir, waktu penyerahan, penguncian flag non-payroll | 10 Tahun |
| `AUDIT-DED-08` | *Hard-Cap 50% Rule Enforced* | Pemangkasan tagihan karena mentok batas 50% | Nilai Gross Pay, plafon 50%, daftar tagihan terpotong vs terpotong parsial vs ditunda | 10 Tahun |
| `AUDIT-DED-09` | *Deduction Deferred Carried Over*| Tagihan sisa ditunda ke bulan depan | ID Jadwal, ID Pegawai, nominal tertunda, nomor siklus bulan asal & tujuan | 10 Tahun |
| `AUDIT-DED-10` | *Salary Past Correction Issued* | Kompensasi salah potong periode lampau dibuat | ID Slip Gaji lampau terdampak, nominal kompensasi, dasar justifikasi, otorisasi HR-Finance | 10 Tahun |
| `AUDIT-DED-11` | *Separation Debt Fully Cleared* | Pelunasan pinjaman saat PHK/Resign | ID Pegawai, nomor slip penutupan (Final Payslip), total utang dilunasi dari pesangon | 10 Tahun |
| `AUDIT-DED-12` | *Receivable Statement Issued* | Hak pesangon tidak cukup melunasi utang | Nilai selisih piutang tertunggak, nomor tagihan kasir, pencegahan sign-off Exit Clearance | 10 Tahun |


---

## 20. Matriks Hak Akses & Jejak Audit Sub-Modul 12 (Slice S-D3 Benefit Management)

Bagian ini mendefinisikan matriks kewenangan hak akses berbasis peran (*Role-Based Access Control* - RBAC) dan spesifikasi rekaman jejak audit (*audit trail logging*) untuk seluruh operasi pada **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**.

---

### 20.1 Matriks Butir Hak Akses (Access Permissions)

| String Butir Hak Akses | Deskripsi Kewenangan Operasional | Peran Pengguna yang Berhak (Roles) |
| :--- | :--- | :--- |
| `[AccessPermission("HR_Benefit", "View")]` | Melihat ringkasan eksekutif dasbor analitik manfaat, rasio penyerapan plafon, dan status kepesertaan BPJS. | HR Director, HR Manager, HR Benefit Officer |
| `[AccessPermission("HR_Benefit_Plan", "View")]` | Melihat katalog master paket manfaat 3-lapisan dan matriks kelayakan grade staf rumah sakit. | HR Benefit Officer, HR Specialist, Finance Manager |
| `[AccessPermission("HR_Benefit_Plan", "Manage")]` | Membuat, mengedit pagu limit tahunan, persentase diskon tarif staf RS, atau tier kamar rawat inap. | HR Director, HR Manager |
| `[AccessPermission("HR_Benefit_Enrollment", "View")]` | Melihat daftar kepesertaan asuransi seluruh pegawai rumah sakit beserta nomor BPJS dan masa berlakunya. | HR Benefit Officer, HR Supervisor |
| `[AccessPermission("HR_Benefit_Enrollment", "Manage")]`| Mengaktifkan paket benefit lengkap pasca-lulus masa percobaan 3 bulan atau memperbarui nomor polis asuransi. | HR Benefit Officer, HR Manager |
| `[AccessPermission("HR_Benefit_Dependent", "Verify")]` | Memeriksa keabsahan berkas fisik scan KK/Akta tanggungan keluarga dan menetapkan status persetujuan verifikasi. | HR Benefit Officer |
| `[AccessPermission("HR_Benefit_InHouse", "View")]` | Membaca histori buku besar mutasi saldo subsidi plafon berobat di RS Quilvian sendiri per pegawai. | HR Benefit Officer, Finance Officer |
| `[AccessPermission("HR_Benefit_InHouse", "Adjust")]` | Melakukan koreksi/penyesuaian manual saldo plafon nakes atas pembatalan tindakan medis di SIMRS. | HR Director, HR Manager |
| `[AccessPermission("HR_Benefit_Claim", "View")]` | Melihat antrean transaksi pengajuan klaim penjamin sekunder (COB) berlampiran SEP BPJS. | HR Benefit Officer, Finance AR Officer |
| `[AccessPermission("HR_Benefit_Claim", "Settle")]` | Menyetujui dan mengonfirmasi penyelesaian transfer santunan klaim sekunder serta mendelegasikan ekses ke payroll. | HR Manager, Finance Disbursement |
| `[AccessPermission("ESS_Benefit", "ViewSelf")]` | Pegawai melihat kepesertaan paket benefit sendiri, sisa plafon berobat RS, dan status kartu BPJS. | Seluruh Pegawai Aktif (Employee Self-Service) |
| `[AccessPermission("ESS_Benefit", "ManageFamily")]` | Pegawai mendaftarkan pasangan sah atau anak berlampiran berkas KK/akta di portal ESS. | Seluruh Pegawai Aktif (Employee Self-Service) |
| `[AccessPermission("ESS_Benefit", "SubmitClaim")]` | Pegawai mengajukan klaim koordinasi manfaat (COB) atas biaya perawatan medis berlampiran nomor SEP. | Seluruh Pegawai Aktif (Employee Self-Service) |
| `[AccessPermission("SIMRS_Integration", "Inquiry")]` | Petugas loket pendaftaran/admisi SIMRS mengecek eligibilitas hak subsidi dan sisa plafon staf/keluarga. | Sistem SIMRS Admission Service Account |
| `[AccessPermission("SIMRS_Integration", "Deduct")]` | Petugas kasir SIMRS memotong saldo plafon nakes secara otomatis saat billing pasien ditutup. | Sistem SIMRS Cashier Service Account |

---

### 20.2 Matriks Jejak Audit (Audit Trail Events)

Seluruh perubahan status, verifikasi dokumen, pengurangan saldo buku besar, dan persetujuan klaim dicatat ke tabel jejak audit sistem dengan struktur data payload terenkripsi.

| Kode Event Audit | Tingkat Keparahan | Pemicu (Trigger) | Data Kunci yang Direkam (Audit Payload) |
| :--- | :---: | :--- | :--- |
| `HRD_BENEFIT_PLAN_CREATED` | `Normal` | Pembuatan master paket manfaat baru | `PlanCode`, `PlanName`, `Category`, `AnnualLimit`, `CreatedBy`, `Timestamp` |
| `HRD_BENEFIT_PLAN_UPDATED` | `Warning` | Perubahan limit tahunan / % diskon RS | `PlanCode`, `OldLimit`, `NewLimit`, `OldDiscount`, `NewDiscount`, `UpdatedBy` |
| `HRD_BENEFIT_PROBATION_ACTIVATED` | `Normal` | Aktivasi paket benefit pasca-probation | `EmployeeId`, `NIP`, `ProbationEvaluationId`, `EffectiveDate`, `ActivatedBy` |
| `HRD_BENEFIT_POLICY_UPDATED` | `Normal` | Pengkinian nomor BPJS atau polis swasta | `EmployeeId`, `PolicyType`, `OldPolicyNumber`, `NewPolicyNumber`, `UpdatedBy` |
| `HRD_BENEFIT_DEPENDENT_VERIFIED` | `Normal` | Persetujuan tanggungan keluarga ESS | `DependentId`, `NIK`, `Relationship`, `FamilyCardNumber`, `VerifiedBy` |
| `HRD_BENEFIT_DEPENDENT_REJECTED` | `Warning` | Penolakan berkas tanggungan keluarga | `DependentId`, `NIK`, `RejectionReason`, `RejectedBy`, `Timestamp` |
| `HRD_BENEFIT_INHOUSE_BALANCE_DEDUCTED` | `Critical`| Kasir SIMRS memotong saldo plafon nakes | `EmployeeId`, `SimrsRegNumber`, `DeductedAmount`, `BalanceBefore`, `BalanceAfter` |
| `HRD_BENEFIT_INHOUSE_BALANCE_ADJUSTED` | `Critical`| Koreksi manual saldo plafon oleh HR Mgr | `EmployeeId`, `AdjustmentAmount`, `Reason`, `ReferenceNumber`, `AdjustedBy` |
| `HRD_BENEFIT_COB_CLAIM_SUBMITTED` | `Normal` | Pegawai men-submit klaim COB di ESS | `ClaimNumber`, `EmployeeId`, `SepBpjsNumber`, `TotalBill`, `SecondaryClaimAmount` |
| `HRD_BENEFIT_COB_CLAIM_SETTLED` | `Critical`| HR/Keuangan melunasi santunan klaim | `ClaimNumber`, `SettledAmount`, `ExcessAmount`, `SettlementRef`, `SettledBy` |
| `HRD_BENEFIT_TERMINATED_EXIT_CLEARANCE` | `Critical`| Penonaktifan benefit saat resign/PHK | `EmployeeId`, `EffectiveTerminationDate`, `ExitClearanceId`, `TerminatedBy` |
| `HRD_BENEFIT_YEAR_END_FORFEITED` | `Warning` | Sisa plafon hangus saat reset 1 Januari | `EmployeeId`, `Year`, `ForfeitedBalanceAmount`, `SystemResetTimestamp` |


---

## 21. Matriks Hak Akses & Jejak Audit Sub-Modul 13 (Slice S-D4 HR Service Management & Request — Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Bagian ini mendefinisikan seluruh string hak akses persis (*exact permission strings*) dan matriks jejak audit (*audit trail matrix*) untuk **Sub-Modul 13 (Slice `S-D4`)**, memastikan tata kelola layanan helpdesk SDM, otorisasi penerbitan dokumen resmi, proteksi kerahasiaan gaji, serta forensik pemutakhiran profil kepegawaian berjalan aman dan patuh regulasi.

---

### 21.1 Matriks Butir Hak Akses (Access Permissions)

| Butir Hak Akses (Exact Permission String) | Deskripsi Fungsional & Batasan Akses | Role Pengguna yang Memiliki Hak |
| :--- | :--- | :--- |
| `[AccessPermission("HR_Service", "Access")]` | Hak membuka grup menu induk Layanan & Helpdesk SDM pada sidebar aplikasi. | HR Administrator, HR Specialist, HR Manager |
| `[AccessPermission("HR_Service_Catalog", "View")]` | Melihat konfigurasi master kategori layanan, tipe layanan, target jam SLA, dan master template dokumen. | HR Administrator, HR Specialist, HR Manager |
| `[AccessPermission("HR_Service_Catalog", "Manage")]` | Membuat dan mengedit kategori layanan, durasi SLA, token template dokumen dinamis, serta pola penomoran surat. | HR Administrator, HR Manager |
| `[AccessPermission("HR_Service_Ticket", "View")]` | Melihat antrean tiket permohonan staf, memantau countdown timer SLA, dan membuka statistik helpdesk. | HR Administrator, HR Specialist, HR Manager |
| `[AccessPermission("HR_Service_Ticket", "Assign")]` | Menugaskan atau mere-assign tiket dari antrean umum ke analis tertentu, atau mengambil alih tiket (*claim*). | HR Specialist, HR Supervisor, HR Manager |
| `[AccessPermission("HR_Service_Ticket", "Process")]` | Memproses tiket, memeriksa berkas bukti, mengirim pesan klarifikasi, dan mengeksekusi penyelesaian tiket (*resolve*). | HR Specialist, HR Supervisor |
| `[AccessPermission("HR_Document", "Issue")]` | Memeriksa draf surat resmi, mengesahkan penarikan gaji otomatis Sub-10, menerbitkan nomor surat seri gapless, dan stempel QR Code. | Supervisor Kompensasi & Benefit, Manajer SDM |
| `[AccessPermission("HR_Document", "Revoke")]` | Mencabut keabsahan hukum dokumen resmi kepegawaian terbit dengan menginputkan Berita Acara Pemeriksaan (BAP). | Manajer SDM, Bagian Hukum / Legal Rumah Sakit |
| `[AccessPermission("ESS_Service", "Access")]` | Hak membuka menu induk portal mandiri Layanan Kepegawaian di portal ESS. | Seluruh Staf Rumah Sakit Aktif (Medis & Non-Medis) |
| `[AccessPermission("ESS_Service", "CreateRequest")]` | Mengajukan tiket permohonan mandiri, meminta surat resmi, membalas utas klarifikasi, dan melihat riwayat tiket sendiri. | Seluruh Staf Rumah Sakit Aktif (Medis & Non-Medis) |
| `[AccessPermission("ESS_Document", "ViewMyArchive")]` | Mengakses menu "Dokumen Saya", mengunduh salinan PDF resmi, dan membuka 4-digit PIN rahasia surat finansial. | Seluruh Staf Rumah Sakit Aktif (Medis & Non-Medis) |

---

### 21.2 Matriks Jejak Audit (Audit Trail Events)

Seluruh aktivitas pengajuan tiket, penugasan antrean, perubahan data profil, penerbitan surat ber-QR, pembukaan PIN gaji di portal publik, serta pencabutan keabsahan dokumen wajib direkam ke dalam log audit sistem yang tidak dapat diubah (*tamper-evident*).

| Kode Event Audit | Tingkat Keparahan | Pemicu (Trigger) | Data Kunci yang Direkam (Audit Payload) |
| :--- | :---: | :--- | :--- |
| `HRS_TICKET_SUBMITTED` | `Normal` | Staf men-submit tiket di ESS | `TicketNumber`, `EmployeeId`, `ServiceTypeCode`, `PriorityTier`, `SlaDeadlineUtc`, `Timestamp` |
| `HRS_TICKET_ASSIGNED` | `Normal` | Tiket ditugaskan ke analis HR | `TicketNumber`, `AssignedToUserId`, `AssignedByUserId`, `OldAssigneeUserId`, `Timestamp` |
| `HRS_TICKET_CLARIFICATION_REQUESTED` | `Normal` | Analis meminta kelengkapan berkas | `TicketNumber`, `AuthorUserId`, `IsInternalNote`, `CommentTextLength`, `TimerPaused` |
| `HRS_TICKET_RESOLVED` | `Normal` | Analis menyelesaikan tiket tuntas | `TicketNumber`, `ResolvedByUserId`, `ResolutionSummary`, `DurationMinutes`, `SlaResult` |
| `HRS_PROFILE_CHANGE_AUTO_APPLIED` | `Critical`| Sistem mutasi data master Sub-02 | `TicketNumber`, `EmployeeId`, `ComponentModified`, `OldValuesSnapshot`, `NewValuesApplied` |
| `HRS_DOCUMENT_DRAFT_GENERATED` | `Normal` | Pembuatan draf naskah dinas | `DocumentRequestId`, `DocumentTypeCode`, `EmployeeId`, `IsFinancialSensitive` |
| `HRS_DOCUMENT_CB_APPROVED` | `Critical`| Supervisor C&B verifikasi gaji | `DocumentRequestId`, `EmployeeId`, `PayrollPayslipId`, `SalaryBruto`, `ApprovedByUserId` |
| `HRS_DOCUMENT_ISSUED` | `Critical`| Pejabat sahkan surat & rilis QR | `OfficialDocumentNumber`, `VerificationHash`, `SignerUserId`, `ValidUntilDate`, `PinGenerated` |
| `HRS_DOCUMENT_REVOKED` | `Critical`| Pencabutan keabsahan surat | `OfficialDocumentNumber`, `RevokedByUserId`, `RevocationBapNumber`, `RevocationReason` |
| `HRS_PUBLIC_DOCUMENT_VERIFIED` | `Normal` | Pihak ketiga scan QR publik | `VerificationHash`, `OfficialDocumentNumber`, `RequestIpAddress`, `UserAgent`, `Timestamp` |
| `HRS_PUBLIC_SALARY_PIN_REVEALED`| `Critical`| Pihak ketiga berhasil buka PIN | `VerificationHash`, `OfficialDocumentNumber`, `RequestIpAddress`, `PinAttemptCount`, `Timestamp` |
| `HRS_SLA_BREACH_ESCALATED` | `Warning` | Tiket helpdesk melewati batas 100% | `TicketNumber`, `ServiceTypeCode`, `AssignedToUserId`, `OverdueMinutes`, `EscalatedToManager` |


---

## 22. Matriks Hak Akses & Jejak Audit Sub-Modul 14 (Slice `S-C1` Credentialing, License & Clinical Privilege)

Bagian ini mendefinisikan seluruh string otorisasi berbasis wewenang (*role-based access control* - RBAC) dan jejak audit (*audit trail*) untuk **Sub-Modul 14 (Slice `S-C1`)**, guna menjamin pemisahan kewenangan klinis yang ketat (*clinical governance*), perlindungan integritas dokumen lisensi STR/SIP, kerahasiaan rekam jejak penilaian Mitra Bestari (*Peer Review*), otorisasi tunggal Direktur Utama RS, serta transparansi insiden bypass darurat keselamatan pasien di SIMRS.

---

### 22.1 Matriks Butir Hak Akses (Access Permissions)

| Butir Hak Akses (Exact Permission String) | Deskripsi Fungsional & Batasan Akses | Role Pengguna yang Memiliki Hak |
| :--- | :--- | :--- |
| `[AccessPermission("Credential_Governance", "Access")]` | Hak membuka grup menu induk Kredensialing & Tata Kelola Profesi Rumah Sakit. | Direktur Utama, Direktur Medis, Ketua Komite Medik/Keperawatan/Nakes, Staf Kredensialing |
| `[AccessPermission("Credential_Application", "Create")]` | Hak mengajukan berkas kredensial baru, mengunggah portofolio, dan checklist whitebook via portal ESS. | Tenaga Medis, Dokter Spesialis, Perawat Klinis, Bidan, Nakes Lainnya |
| `[AccessPermission("Credential_Application", "View")]` | Melihat daftar berkas permohonan kredensial dan rincian portofolio pemohon. | Pengurus Komite Profesi, Staf Sekretariat Kredensialing |
| `[AccessPermission("Credential_Verification", "Execute")]` | Menjalankan audit verifikasi sumber primer (PSV) ke database KKI / SatuSehat SDMK Kemenkes. | Staf Sekretariat Kredensialing, Subkomite Kredensial |
| `[AccessPermission("Credential_License", "View")]` | Melihat daftar dokumen STR seumur hidup, SIP 5 tahun, dan radar pantauan kedaluwarsa izin. | Staf Kredensialing, HR Specialist, Kepala Bidang Pelayanan |
| `[AccessPermission("Peer_Review", "Assess")]` | Menilai kesesuaian logbook portofolio, menentukan derajat wewenang tindakan, dan submit rekomendasi. | Dokter/Nakes yang Ditunjuk Sebagai Anggota Panel Mitra Bestari |
| `[AccessPermission("Committee_Review", "Assign")]` | Menetapkan anggota panel Mitra Bestari sebidang untuk menilai berkas permohonan klinis. | Ketua Subkomite Kredensial Komite Terkait |
| `[AccessPermission("Committee_Review", "Approve")]` | Mengesahkan hasil sidang pleno komite dan menerbitkan surat usulan resmi Ketua Komite ke Direksi. | Ketua Komite Medik, Ketua Komite Keperawatan, Ketua Komite Nakes Lain |
| `[AccessPermission("Spk_Rkk", "ParafDirector")]` | Memberikan paraf telaah manajerial dan kesesuaian operasional layanan rumah sakit. | Direktur Pelayanan Medik, Direktur Keperawatan |
| `[AccessPermission("Spk_Rkk", "SignDirector")]` | Otorisasi pengesahan tunggal dan penandatanganan elektronik (e-Sign) SPK & lampiran RKK sah. | Direktur Utama Rumah Sakit |
| `[AccessPermission("Clinical_Privilege", "CoSign")]` | Memberikan persetujuan digital (*co-signature*) atas order tindakan medis di bawah supervisi Level 3. | Dokter Supervisor Senior yang Ditunjuk dalam SPK |
| `[AccessPermission("Clinical_Privilege", "EmergencyOverride")]` | Menjalankan tombol bypass darurat penyelamatan nyawa di SIMRS/EMR dengan input alasan klinis cito. | Dokter Spesialis IGD/ICU/OK yang Sedang Bertugas Dinas |
| `[AccessPermission("Kmkp_Audit", "Decide")]` | Melakukan audit telaah keselamatan pasien dan menjatuhkan putusan sidang terhadap kejadian override darurat. | Ketua Komite Mutu & Keselamatan Pasien (KMKP), Komite Medik |
| `[AccessPermission("FastTrack_Privilege", "Grant")]` | Menerbitkan kewenangan sementara dokter tamu atau kewenangan darurat cito bencana massal. | Direktur Pelayanan Medik, Direktur Utama RS |

---

### 22.2 Matriks Jejak Audit (Audit Trail Events)

| Nama Event Audit | Pemicu (Trigger) & Sumber Aksi | Data Kritis yang Direkam dalam Log | Tingkat Risiko |
| :--- | :--- | :--- | :---: |
| `CredentialApplicationSubmitted` | Nakes men-submit berkas kredensial via ESS | `ApplicationId`, `WorkforceId`, `CommitteeId`, `TargetLevel`, `Timestamp`, `IPAddress` | Rendah |
| `PrimarySourceVerificationCompleted` | Petugas mencatat hasil verifikasi PSV STR/SIP | `LicenseId`, `PsvSourceUrl`, `PsvResult`, `PsvVerifiedBy`, `VerificationNotes`, `Timestamp` | Tinggi |
| `PeerReviewAssessmentSubmitted` | Reviewer Mitra Bestari menyerahkan rekomendasi | `AssessmentId`, `PeerReviewerId`, `PrivilegeCatalogId`, `RecommendedLevel`, `SignatureHash` | Kritis |
| `CommitteePlenaryFinalized` | Ketua Komite mengesahkan risalah sidang pleno | `CommitteeReviewId`, `CommitteeCode`, `ChairmanId`, `TotalApprovedPrivileges`, `Timestamp` | Kritis |
| `SpkRkkSignedByDirector` | Direktur Utama menandatangani SPK & RKK sah | `SpkIssuanceId`, `SpkNumber`, `DirectorId`, `DigitalSignatureHash`, `EffectiveDates`, `QrToken` | Kritis |
| `ClinicalGateValidated` | SIMRS EMR memvalidasi wewenang tindakan klinis | `DoctorId`, `ProcedureCode`, `UnitId`, `Verdict` (`ALLOW_INDEPENDENT` / `DENIED`), `LatencyMs` | Sedang |
| `SupervisedOrderCoSigned` | Dokter supervisor menyetujui order tindakan Level 3 | `OrderId`, `SupervisorDoctorId`, `CandidateDoctorId`, `ProcedureCode`, `CoSignedAt` | Tinggi |
| `LifeSavingEmergencyOverrideTriggered` | Dokter menekan tombol bypass darurat cito EMR | `OverrideLogId`, `DoctorId`, `PatientEpisodeId`, `ActionAttempted`, `EmergencyJustification`, `IP` | Fatal |
| `EmergencyOverrideKmkpAudited` | KMKP mencatat vonis audit telaah bypass darurat | `OverrideLogId`, `KmkpAuditorId`, `AuditVerdict`, `AuditNotes`, `Timestamp` | Kritis |
| `TemporaryPrivilegeGranted` | Direksi menerbitkan kewenangan dokter konsultan tamu | `IssuanceId`, `GuestDoctorId`, `HomeFacility`, `DurationDays`, `SupervisorId`, `ApprovedBy` | Tinggi |
| `EmergencyPrivilegeGranted` | Direktur Medis mengotorisasi kewenangan bencana cito | `IssuanceId`, `VolunteerDoctorId`, `DisasterEventCode`, `ExpiryHours`, `ApprovedBy` | Kritis |
| `ClinicalPrivilegeSuspended` | Sistem menangguhkan wewenang akibat SIP/SPK mati | `PrivilegeId`, `DoctorId`, `TriggerSource` (`SIP_EXPIRED` / `SPK_3YR_EXPIRED`), `Timestamp` | Fatal |
| `ClinicalPrivilegeRevoked` | Direktur Utama mencabut SPK akibat pelanggaran etik | `PrivilegeId`, `DoctorId`, `RevocationBapUrl`, `RevokedByDirectorId`, `Reason`, `Timestamp` | Fatal |
| `SipExpirationRadarAlertSent` | Scheduler mengirimkan alert sisa masa berlaku SIP | `LicenseId`, `DoctorId`, `DaysRemaining` (90/60/30/7), `DeliveryChannels`, `Timestamp` | Rendah |

---

## 23. Matriks Hak Akses & Jejak Audit Sub-Modul 15: OPPE, FPPE & Recredentialing (`S-C1-OPPE`)

### 23.1 Matriks Hak Akses Granular (Granular Permissions)

| Kode Hak Akses (`PermissionCode`) | Deskripsi Wewenang & Batas Operasi | Peran yang Diberikan Hak (*Roles*) |
| :--- | :--- | :--- |
| `[AccessPermission("Oppe_Governance", "Access")]` | Hak membuka grup menu induk Kinerja Klinis, Evaluasi OPPE, FPPE, dan Rekredensialing Mutu. | Direktur Utama, Direktur Medis, Ketua Komite Medik, Komite Mutu (KMKP), Ketua KSM |
| `[AccessPermission("Oppe_Period", "Manage")]` | Membuka periode evaluasi OPPE baru, mengonfigurasi matriks indikator KSM, mengunci, dan memfinalisasi siklus. | Ketua Subkomite Mutu Profesi, Sekretariat Komite Medik |
| `[AccessPermission("Oppe_Period", "View")]` | Melihat daftar periode evaluasi OPPE yang sedang berjalan dan memantau status partisipasi KSM. | Ketua KSM, Kepala Bidang Pelayanan Medis, Anggota Komite Medik |
| `[AccessPermission("Oppe_Evaluation", "View")]` | Melihat rapor capaian performa klinis individu dokter (mandiri untuk dokter ybs, atau seluruh KSM bagi Ketua KSM). | Dokter Spesialis Ybs, Ketua KSM, Ketua Subkomite Mutu Profesi |
| `[AccessPermission("Oppe_Evaluation", "Calculate")]` | Memicu ekstraksi & perhitungan agregasi nilai kuantitatif 60% dari pangkalan data SIMRS/KMKP. | Admin Komite Medik, Sistem Background Worker |
| `[AccessPermission("Oppe_Evaluation", "Finalize")]` | Mengesahkan nilai komposit akhir rapor OPPE dan menetapkan status kelulusan standar mutu klinis. | Ketua Subkomite Mutu Profesi, Ketua Komite Medik |
| `[AccessPermission("Peer_Review", "Assess")]` | Mengisi instrumen evaluasi kualitatif sejawat konfidensial 40% bagi dokter rekan sebidang. | Dokter Spesialis Penilai yang Ditugaskan dalam KSM |
| `[AccessPermission("Fppe_Trigger", "View")]` | Memantau feed real-time deteksi ambang batas kritis pemicu FPPE (sentinel, komplikasi, audit resume). | Komite Mutu & Keselamatan Pasien (KMKP), Subkomite Mutu Profesi |
| `[AccessPermission("Fppe_Engagement", "Create")]` | Menginisiasi berkas penugasan FPPE terarah, menetapkan target kasus, dan menunjuk Dokter Proctor. | Ketua Komite Medik, Ketua Subkomite Mutu Profesi |
| `[AccessPermission("Fppe_CaseLog", "Verify")]` | Menelaah dan memberikan pengesahan verifikasi penilaian tindakan mandiri pada logbook proctoring. | Dokter Proctor Senior yang Ditunjuk Resmi |
| `[AccessPermission("Fppe_Conclusion", "Approve")]` | Mengesahkan berita acara putusan sidang pleno FPPE (Lolos, Perpanjang Proctoring, Supervisi, Cabut). | Ketua Komite Medik, Ketua Subkomite Mutu Profesi |
| `[AccessPermission("Recred_Verdict", "Decide")]` | Mengoperasikan kalkulator komposit 3 tahun, override diskresi tier, dan menetapkan 4 putusan rekredensialing. | Ketua Komite Medik, Direktur Pelayanan Medik |

---

### 23.2 Matriks Jejak Audit (Audit Trail Events)

| Nama Event Audit | Pemicu (Trigger) & Sumber Aksi | Data Kritis yang Direkam dalam Log | Tingkat Risiko |
| :--- | :--- | :--- | :---: |
| `OppePeriodCreated` | Subkomite membuka siklus periode evaluasi OPPE baru | `PeriodId`, `PeriodCode`, `Year`, `Cycle`, `CreatedBy`, `Timestamp` | Rendah |
| `OppePeriodFinalized` | Subkomite mengesahkan dan mengunci periode evaluasi | `PeriodId`, `TotalStaffEvaluated`, `FinalizedBy`, `Timestamp` | Kritis |
| `OppeSimrsDataAggregated` | Sistem/Petugas menarik data kuantitatif SIMRS | `EvaluationId`, `DoctorId`, `CalculatedQuantitativeScore`, `LatencyMs`, `Timestamp` | Sedang |
| `PeerReviewEvaluationSubmitted` | Dokter penilai menyerahkan evaluasi kualitatif sejawat | `DetailId`, `ReviewerDoctorId`, `CandidateDoctorId`, `QualitativeScore`, `Timestamp` | Kritis |
| `PeerReviewDocumentRead` | Pengguna membuka dan membaca lembar ulasan sejawat | `DetailId`, `AccessorDoctorId`, `CandidateDoctorId`, `Channel`, `IpAddress`, `UserAgent`, `Timestamp` | Fatal |
| `OppeStaffEvaluationCompleted` | Subkomite mengesahkan nilai komposit rapor akhir OPPE | `EvaluationId`, `DoctorId`, `CompositeScore`, `PassedStatus`, `ApprovedBy`, `Timestamp` | Kritis |
| `FppeTriggerIncidentDetected` | SIMRS/KMKP mendeteksi kejadian insiden sentinel / anomali | `IncidentId`, `DoctorId`, `TriggerType`, `IncidentDate`, `SimrsReferenceCode`, `Timestamp` | Fatal |
| `FppeEngagementInitiated` | Komite menerbitkan penugasan investigasi FPPE terarah | `FppeId`, `DoctorId`, `ProctorDoctorId`, `TriggerType`, `TargetCaseCount`, `Timestamp` | Kritis |
| `FppeCaseLogRecorded` | Dokter mengentri tindakan medis ke dalam logbook | `CaseLogId`, `FppeId`, `DoctorId`, `ProcedureCatalogId`, `PseudonymId`, `Timestamp` | Sedang |
| `FppeCaseLogVerifiedByProctor` | Dokter proctor menyetujui evaluasi kemandirian kasus | `CaseLogId`, `ProctorDoctorId`, `Verdict`, `ComplicationObserved`, `VerifiedAt` | Kritis |
| `FppeConclusionApproved` | Sidang pleno mengesahkan berita acara vonis akhir FPPE | `FppeId`, `DoctorId`, `FinalVerdict`, `PlenaryChairmanId`, `RemediationNotes`, `Timestamp` | Fatal |
| `RecredentialingCompositeCalculated`| Sistem menghitung rapor komposit 3 tahun (25-35-40) | `StaffId`, `Year1Score`, `Year2Score`, `Year3Score`, `CompositeScore`, `CalculatedTier` | Tinggi |
| `RecredentialingTierOverridden` | Ketua Komite menyesuaikan tier kelayakan kalkulator | `StaffId`, `OriginalTier`, `NewTier`, `OverrideReasonJustification`, `ChairmanId`, `Timestamp` | Kritis |
| `RecredentialingVerdictDispatched` | Putusan rekredensialing dikirim ke SIMRS EMR (`INT-HR-76`) | `StaffId`, `VerdictType`, `SimrsLevelGranted` (1-4), `SimrsResponseCode`, `Timestamp` | Fatal |

## 24. Matriks Hak Akses & Jejak Audit Sub-Modul 16: Learning, Training & Competency (Slice S-C2)

Matriks ini merinci 13 hak akses berbasis peran (*Role-Based Access Control* - RBAC) dan 14 jejak audit (*Audit Trail Events*) permanen untuk memastikan tata kelola pembelajaran, penegakan integritas sertifikasi, kepatuhan ikatan dinas, serta perlindungan data kompetensi nakes rumah sakit.

### 24.1 Matriks Hak Akses (RBAC Permissions)

| Kode Hak Akses | Nama Perizinan | Modul / Konteks | Deskripsi Kewenangan Operasional | Peran Pemegang (Roles) |
| :--- | :--- | :--- | :--- | :--- |
| `Training_Mandatory:View` | Baca Aturan Pelatihan Wajib | Diklat / Akreditasi | Melihat katalog master aturan pelatihan wajib STARKES KPS 8, target nakes, dan masa berlaku | `HRD_Admin`, `Diklat_Staff`, `Unit_Head`, `Employee` |
| `Training_Mandatory:Manage` | Kelola Aturan Wajib & Radar | Diklat / Akreditasi | Mengonfigurasi parameter pelatihan wajib, grace period, dan penegakan flag *Overdue* | `HRD_Admin`, `Diklat_Manager` |
| `Training_Plan:Create` | Susun Usulan Program TNA | Perencanaan Diklat | Membuat dan mengajukan draf Rencana Program Pelatihan Tahunan RS dan TNA per unit kerja | `Diklat_Staff`, `Department_Head` |
| `Training_Plan:Approve` | Sahkan Rencana Kerja Tahunan | Manajemen Eksekutif | Menyetujui dokumen master program tahunan diklat dan membuka pagu anggaran resmi RS | `HRD_Director`, `Medical_Director`, `Finance_Director` |
| `Training_Session:View` | Lihat Jadwal Sesi Pelatihan | Penyelenggaraan Diklat| Melihat kalender batch kelas, workshop simulasi, instruktur, dan kuota peserta | Seluruh Karyawan (`All_Authenticated`) |
| `Training_Session:Manage` | Kelola Sesi Kelas & Presensi | Penyelenggaraan Diklat| Membuat jadwal sesi, mengatur ruang/tautan webinar, mencatat absensi, dan membatalkan sesi | `Diklat_Staff`, `Diklat_Manager` |
| `Training_Enrollment:Submit` | Ajukan Pendaftaran Diklat | ESS / Penugasan Dinas | Mendaftarkan diri sendiri (ESS) atau mendaftarkan staf bawahan ke sesi pelatihan internal/eksternal | `Employee`, `Unit_Head`, `Diklat_Staff` |
| `Training_Enrollment:Approve`| Setujui Pendaftaran Diklat | Alur Persetujuan | Menyetujui dispensasi dinas dan ketersediaan pagu anggaran diklat secara berjenjang | `Unit_Head`, `Diklat_Manager` |
| `Training_Assessment:Grade` | Nilai Ujian Teori & OSCE | Evaluasi Kompetensi | Memasukkan skor post-test teori serta mengesahkan rubrik simulasi OSCE bertanda tangan digital | `Clinical_Assessor`, `Training_Instructor` |
| `Training_Certificate:Issue` | Terbitkan Sertifikat & Radar | Sertifikasi Kepegawaian| Menerbitkan e-Sertifikat resmi kelulusan, memverifikasi sertifikat eksternal, dan arsip e-Dossier | `Diklat_Staff`, `Diklat_Manager`, `HRD_Admin` |
| `Training_Budget:Manage` | Kelola Pagu Anggaran Diklat | Keuangan & Diklat | Menetapkan alokasi pagu anggaran tahunan diklat per departemen dan memantau realisasi biaya | `Finance_Manager`, `Diklat_Manager` |
| `Training_Bond:Manage` | Kelola Kontrak Ikatan Dinas | Legal SDM & Retensi | Menerbitkan draf akta ikatan dinas, memantau masa retensi, dan mencatat tagihan penalti wanprestasi | `HR_Legal_Officer`, `HRD_Manager` |
| `Training_PlataranSehat:Sync`| Sinkronisasi SKP Kemenkes | Portofolio Profesi | Mencatat kegiatan perolehan SKP nakes, memvalidasi ID, dan menyinkronkan data ke Plataran Sehat | `Diklat_Staff`, `Komite_Medik_Admin`, `Nakes_User` |

---

### 24.2 Matriks Jejak Audit Keamanan & Akuntabilitas (Audit Events)

Seluruh perubahan status, penilaian klinis berisiko tinggi, transaksi anggaran, dan penerbitan penalti ganti rugi dicatat secara permanen (*append-only*) pada log audit sistem.

| Kode Event Audit | Kategori Event | Sumber Pemicu | Data Kritis yang Direkam | Tingkat Kritis |
| :--- | :--- | :--- | :--- | :---: |
| `MandatoryTrainingRuleConfigured` | Konfigurasi | Pengubahan Aturan | Kode pelatihan, masa berlaku bulan, ambang nilai, target nakes, user ID | `Medium` |
| `AnnualTrainingPlanApproved` | Anggaran | Pengesahan Direksi | ID rencana tahunan, tahun fiskal, total pagu biaya yang disahkan, approver ID | `High` |
| `TrainingSessionScheduled` | Operasional | Penjadwalan Sesi | ID sesi, judul pelatihan, tanggal mulai/selesai, kuota kapasitas, instruktur | `Low` |
| `TrainingEnrollmentSubmitted` | Pendaftaran | Input ESS / Dinas | ID pendaftaran, staf pemohon, sesi target, sumber dana, estimasi biaya | `Low` |
| `TrainingEnrollmentApproved` | Otorisasi | Persetujuan Berjenjang | ID pendaftaran, level persetujuan (Atasan/Diklat), waktu persetujuan, user ID | `Medium` |
| `TrainingAttendanceRecorded` | Presensi | Check-in Kelas | ID pendaftaran, sesi pelatihan, waktu check-in/out, status kehadiran, pengawas | `Low` |
| `TrainingTheoryPostTestGraded` | Penilaian | Input Nilai Teori | ID evaluasi, peserta, nilai ujian post-test tertulis, status kelulusan teori | `Medium` |
| `TrainingOsceSimGraded` | Ujian Klinis | Pengesahan Asesmen | ID evaluasi, nilai OSCE, tanda tangan digital assessor, rincian skor checklist | `High` |
| `TrainingCertificateIssued` | Sertifikasi | Penerbitan e-Sertifikat | Nomor seri sertifikat, ID nakes, tanggal kedaluwarsa, URL dokumen e-Dossier | `High` |
| `MandatoryTrainingExpiryRadarTriggered`| Kepatuhan | Evaluasi Worker | ID staf, jenis pelatihan wajib, tanggal kedaluwarsa, kanal peringatan (H-90/60/30) | `Low` |
| `StaffShiftBuddySystemAssigned` | Keselamatan | Safeguard Jadwal Dinas | ID nakes Overdue, ID unit ruangan, tanggal dinas, ID staf senior pendamping | `Critical` |
| `TrainingBondContractGenerated` | Kontrak Legal | Penerbitan Ikatan Dinas | Nomor akta kontrak, ID staf nakes, total nominal biaya, masa retensi bulan | `High` |
| `TrainingBondBreachFlagged` | Penalti | Wanprestasi Resign | ID kontrak, tanggal breach, nominal denda penalti ganti rugi, dasar kalkulasi | `Critical` |
| `PlataranSehatCertificateVerified`| Sinkronisasi | Verifikasi Kemenkes | ID sertifikat Plataran Sehat, ID nakes, jumlah SKP diakui, status respon API | `Medium` |

## 25. Matriks Hak Akses & Jejak Audit Sub-Modul 17: Performance Management & Clinical Career (Slice S-C3)

Matriks ini merinci 14 hak akses berbasis peran (*Role-Based Access Control* - RBAC) dan 14 jejak audit (*Audit Trail Events*) permanen untuk memastikan tata kelola penilaian kinerja, perlindungan kerahasiaan evaluasi, pengawasan sidang kalibrasi, akuntabilitas pembinaan PIP, serta keabsahan jenjang profesi nakes rumah sakit.

### 25.1 Matriks Hak Akses (RBAC Permissions)

| Kode Hak Akses | Nama Perizinan | Modul / Konteks | Deskripsi Kewenangan Operasional | Peran Pemegang (Roles) |
| :--- | :--- | :--- | :--- | :--- |
| `Performance_Cycle:View` | Baca Siklus Kinerja | Manajemen Kinerja | Melihat daftar periode evaluasi kerja tahunan/semesteran dan kalender tenggat | `All_Authenticated_Employees` |
| `Performance_Cycle:Manage` | Kelola Siklus Kinerja | Manajemen Kinerja | Membuka periode baru, menetapkan jadwal batas waktu, dan mengunci siklus evaluasi | `HRD_Admin`, `HR_Performance_Manager` |
| `Performance_Metric:View` | Baca Katalog KPI & Nilai | Standar Mutu SDM | Melihat master indikator metrik kuantitatif dan rubrik deskriptor Core Values | `All_Authenticated_Employees` |
| `Performance_Metric:Manage` | Kelola Katalog KPI & Nilai| Standar Mutu SDM | Menambah, mengubah, menonaktifkan indikator kinerja, satuan, dan formula polaritas | `HR_Performance_Manager`, `Quality_Director` |
| `Performance_Goal:View` | Baca Sasaran Individu | Perencanaan Kinerja| Melihat rincian butir sasaran kerja target tahunan pegawai | `Employee`, `Unit_Head`, `Department_Head` |
| `Performance_Goal:Manage` | Otorisasi Sasaran Kerja | Perencanaan Kinerja| Menetapkan cascading sasaran target unit dan menyetujui target individu pegawai | `Unit_Head`, `Department_Head` |
| `Performance_Appraisal:SelfAssess` | Isi Evaluasi Mandiri | Evaluasi Pegawai | Mengisi lembar refleksi diri, unggah bukti dukung di ESS, dan tanda tangan rilis | `Employee` (Khusus Data Pribadi) |
| `Performance_Appraisal:ManagerAssess` | Nilai Staf Bawahan (Karu) | Penilaian Berjenjang | Memberi skor resmi 60% KPI dan 40% perilaku Core Values untuk staf di ruangannya | `Unit_Head`, `Clinical_Supervisor` |
| `Performance_Appraisal:SecondLineReview`| Review & Otorisasi Kabid | Penilaian Berjenjang | Menelaah konsistensi nilai antar ruangan dan mengesahkan berkas ke komite | `Department_Head`, `Division_Head` |
| `Performance_Calibration:View` | Baca Kurva Kalibrasi RS | Sidang Kalibrasi | Melihat grafik sebaran nilai aktual RS terhadap pedoman distribusi fleksibel | `Calibration_Committee_Members` |
| `Performance_Calibration:Manage` | Otorisasi Skor Sidang | Sidang Kalibrasi | Melakukan penyesuaian skor (*override*), mengesahkan hasil pleno, dan rilis skor | `Board_of_Directors`, `HR_Director` |
| `Performance_Pip:Manage` | Kelola Program PIP 60 Hari | Pembinaan Kinerja | Menerbitkan kontrak PIP, mencatat logbook mingguan mentor, dan putusan sidang | `HR_Performance_Staff`, `Unit_Head`, `Mentor` |
| `Clinical_Career:Apply` | Ajukan Jenjang Klinis | Profesi Nakes | Mengajukan permohonan kenaikan jenjang klinis (PK I-V) dan unggah logbook di ESS | `Clinical_Staff` (Perawat, Bidan, Nakes) |
| `Clinical_Career:Manage` | Kelola Sidang Jenjang Nakes| Komite Keperawatan | Mengaudit logbook portofolio, memimpin sidang komite, dan rekomendasi SK Direksi | `Komite_Keperawatan_Admin`, `Clinical_Assessor` |
| `Performance_Dispute:Submit` | Ajukan Sanggahan Banding | Hak Pegawai ESS | Mendaftarkan formulir sanggah nilai resmi dan melampirkan berkas bukti pembanding | `Employee` (Pemohon Banding) |
| `Performance_Dispute:Manage` | Adjudikasi Sengketa Nilai | Majelis Banding | Memfasilitasi mediasi klarifikasi HRD dan menetapkan putusan final Direktur SDM | `HR_Performance_Manager`, `HR_Director` |

---

### 25.2 Matriks Jejak Audit Keamanan & Akuntabilitas (Audit Events)

Seluruh perubahan siklus, penyesuaian skor kalibrasi, penerbitan kontrak pembinaan PIP, dan putusan banding dicatat secara permanen (*append-only*) pada log audit sistem.

| Kode Event Audit | Kategori Event | Sumber Pemicu | Data Kritis yang Direkam | Tingkat Kritis |
| :--- | :--- | :--- | :--- | :---: |
| `PerformanceCycleConfigured` | Konfigurasi | Input HRD | ID siklus, tahun evaluasi, jadwal tenggat waktu, user pembuat | `Medium` |
| `PerformanceGoalsCascaded` | Perencanaan | Approval Karu | ID pegawai, daftar butir target KPI, bobot, target nilai disepakati | `Low` |
| `SelfAppraisalSubmitted` | Pengisian | Submit ESS Staf | ID evaluasi, ID pegawai, waktu penyerahan mandiri, tautan bukti | `Low` |
| `ManagerAppraisalCompleted` | Penilaian | Submit Karu | ID evaluasi, skor KPI (60%), skor perilaku (40%), skor komposit awal | `Medium` |
| `SecondLineAppraisalReviewed` | Penelaahan | Otorisasi Kabid | ID evaluasi, ID atasan kedua, waktu telaah, catatan pertimbangan | `Medium` |
| `CalibrationSessionOpened` | Sidang | Pembukaan Pleno | Nomor register sidang, tanggal, daftar komite hadir, pedoman distribusi | `Medium` |
| `AppraisalScoreOverridden` | Penyesuaian | Override Komite | ID evaluasi, skor awal, skor baru, alasan justifikasi resmi komite | `Critical` |
| `CalibrationSessionFinalized` | Pengesahan | Penutupan Pleno | ID sidang, total pegawai terkalibrasi, sebaran nilai final, approver | `High` |
| `AppraisalScoresReleased` | Publikasi | Rilis Nilai RS | ID siklus, total nilai dirilis, waktu rilis, batas akhir masa sanggah (H+7) | `High` |
| `PerformancePipContractIssued` | Pembinaan | Pemicu Skor < 65 | ID pegawai, nomor kontrak PIP, ID mentor, sasaran perbaikan terukur | `High` |
| `PipWeeklyLogbookRecorded` | Monitoring | Input Mentor | ID kontrak PIP, minggu ke, ringkasan progres, status on-track/at-risk | `Medium` |
| `PipFinalVerdictDelivered` | Keputusan | Sidang Hari Ke-60 | ID kontrak PIP, putusan (Lulus/Perpanjang/Eskalasi HR Legal), alasan | `Critical` |
| `ClinicalLadderReviewApproved` | Jenjang Karir | Rekomendasi Komite | ID nakes, jenjang asal, jenjang baru (PK I-V), nomor SK penetapan | `High` |
| `AppraisalDisputeVerdictDelivered`| Sengketa Nilai| Putusan Majelis | Nomor sanggahan, ID pemohon, putusan (Tetap/Disesuaikan), skor akhir baru | `Critical` |


---

## 26. Matriks Hak Akses & Jejak Audit Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 26.1 Matriks Hak Akses (RBAC Permissions)

| Kode Izin (*Permission Code*) | Deskripsi Kewenangan | Peran yang Memiliki (*Assigned Roles*) | Sifat & Batas Tindakan |
|---|---|---|---|
| `Hrd.Disciplinary.CreateIncident` | Melaporkan dugaan insiden tata tertib | Seluruh Staf, Kepala Ruangan, Supervisor | Pembuatan tiket insiden awal |
| `Hrd.Disciplinary.ViewIncidents` | Melihat daftar laporan insiden tata tertib RS | HR Relations, Tim Kepatuhan, Direktur SDM | Pembacaan terbatas unit wewenang |
| `Hrd.Disciplinary.ConductInvestigation`| Melaksanakan sidang klarifikasi BAP disiplin | HR Legal, Tim Pemeriksa, Komite Disiplin | Input bukti dan temuan investigasi |
| `Hrd.Disciplinary.IssueSanction` | Menerbitkan sanksi Surat Peringatan (SP 1-3) | Manajer HR, Direktur SDM | Otorisasi penerbitan sanksi sah |
| `Hrd.Disciplinary.ViewSanctions` | Melihat riwayat sanksi SP pegawai | HR Officer, Atasan Langsung, Pegawai Bersangkutan | Tampilan profil sanksi e-Dossier |
| `Hrd.Disciplinary.AppealSanction` | Mengajukan sanggah banding sanksi SP | Pegawai Penerima Sanksi | Pembukaan tiket banding formal |
| `Hrd.Disciplinary.IssueSuspension` | Menerbitkan surat skorsing (*paid suspension*) | Direktur Utama | Tindakan hukum skorsing resmi |
| `Hrd.Disciplinary.ManageSuspension` | Memantau & memulihkan pegawai diskorsing | Direktur SDM, HR Legal | Pengendalian masa berlaku skorsing |
| `Hrd.Disciplinary.WhistleblowerReview` | Mengakses kotak masuk laporan whistleblower | Tim Kepatuhan Khusus, Direktur SDM | Sangat Rahasia (*Restricted Access*) |
| `Hrd.Grievance.Submit` | Mengirimkan keluhan hubungan kerja via ESS | Seluruh Staf / Karyawan Rumah Sakit | Pengaduan mandiri terbuka |
| `Hrd.Grievance.SubmitWhistleblower` | Mengirimkan pengaduan whistleblower terenkripsi| Seluruh Staf / Publik Internal RS | Pengaduan rahasia anonim |
| `Hrd.Grievance.ViewOwn` | Memantau perkembangan status tiket keluhan | Pegawai Pengadu | Hanya melihat tiket milik sendiri |
| `Hrd.Grievance.Mediate` | Mengelola mediasi keluhan & pencatatan solusi | Mediator HR, Kepala Ruangan, Supervisor | Penyelesaian damai hubungan kerja |
| `Hrd.Bipartite.Schedule` | Menjadwalkan sidang perundingan Bipartit | HR Industrial Relations | Penjadwalan resmi forum Bipartit |
| `Hrd.Bipartite.View` | Melihat arsip dokumen perundingan Bipartit | Pengurus LKS Bipartit, HR, Direksi | Transparansi hubungan industrial |
| `Hrd.Bipartite.RecordMinutes` | Mencatat risalah resmi sidang Bipartit | Sekretaris Forum Bipartit, HR Legal | Dokumentasi Berita Acara Rapat |
| `Hrd.Bipartite.UploadAgreement` | Mengunggah dokumen Perjanjian Bersama (PB) | HR Legal | Pengarsipan naskah berkekuatan hukum |
| `Hrd.Separation.Initiate` | Mengajukan terminasi kerja / pengunduran diri | Pegawai (Resign), HR Officer (Kontrak/Pensiun/SP3) | Pembukaan proses terminasi |
| `Hrd.Separation.View` | Melihat daftar pegawai dalam proses terminasi | HR Offboarding, Manajer Unit Terkait | Monitoring status pengakhiran ikatan kerja |
| `Hrd.Separation.CalculateSeverance`| Mengoperasikan kalkulator pesangon PP 35/2021 | Analis Kompensasi (C&B Specialist) | Perhitungan UP, UPMK, UPH, Uang Pisah |
| `Hrd.Separation.ViewSeverance` | Melihat rincian lembar kompensasi pesangon | Pegawai Terkait, Manajer C&B, Direksi | Transparansi rincian finansial |
| `Hrd.Separation.ApproveSeveranceFinance`| Mengesahkan nilai pencairan pesangon | Direktur Keuangan (CFO) | Kunci persetujuan anggaran pesangon |
| `Hrd.ExitClearance.View` | Melihat papan kendali status exit clearance | HR Offboarding, Petugas Kliring 5 Unit | Pemantauan matriks serah terima |
| `Hrd.ExitClearance.SignoffChecklist` | Memberikan persetujuan kliring unit kerja | Verifikator Unit (Aset, Farmasi, Keu, IT) | Verifikasi fisik & tanda tangan digital |
| `Hrd.ExitClearance.ViewLockStatus` | Memeriksa status gembok payroll akhir | Petugas Payroll Sub-10, Staf Offboarding | Pengecekan gerbang kelayakan rilis gaji |
| `Hrd.ExitClearance.Finalize` | Menyelesaikan clearance & membuka gembok payroll | Manajer SDM / Kepala Bagian Kepegawaian | Otorisasi pembukaan gembok payroll final |

---

### 26.2 Matriks Jejak Audit Keamanan & Akuntabilitas (Audit Events)

| Kode Peristiwa Audit | Deskripsi Peristiwa | Kategori Tingkat Risiko | Detail Data yang Dicatat |
|---|---|---|---|
| `HRD_DISCIPLINARY_INCIDENT_REPORTED` | Laporan insiden tata tertib dibuat di sistem | `MEDIUM` | IncidentId, EmployeeId, Category, ReporterType, Timestamp |
| `HRD_JUST_CULTURE_CLASSIFICATION_RECORDED`| Klasifikasi Just Culture ditetapkan tim BAP | `HIGH` | IncidentId, JustCultureClass, KmKpAuditRef, InvestigatorNames |
| `HRD_BAP_INVESTIGATION_CONCLUDED` | Sidang pemeriksaan BAP selesai & disahkan | `HIGH` | InvestigationId, RecommendedAction, FindingsSummary |
| `HRD_DISCIPLINARY_SANCTION_ISSUED` | Surat Peringatan (Teguran/SP 1-3) resmi diterbitkan | `CRITICAL` | SanctionId, EmployeeId, Level, IssuedDate, ValidUntil (+6M) |
| `HRD_DISCIPLINARY_SANCTION_EXPIRED` | Status sanksi SP berubah kedaluwarsa otomatis | `LOW` | SanctionId, EmployeeId, ExpiredAt (H+180 Worker) |
| `HRD_DISCIPLINARY_SANCTION_APPEALED` | Pegawai mengajukan sanggah banding sanksi | `MEDIUM` | SanctionId, EmployeeId, AppealStatementUrl, SubmittedAt |
| `HRD_PAID_SUSPENSION_ISSUED` | Penetapan skorsing investigasi (upah penuh) | `CRITICAL` | SuspensionId, EmployeeId, StartDate, ExpectedEndDate |
| `HRD_SIMRS_ACCESS_REVOKED_SUSPENSION` | Sesi SSO SIMRS/EMR dicabut instan karena skorsing | `CRITICAL` | EmployeeId, UserId, SessionTokenRevokedList, Timestamp |
| `HRD_GRIEVANCE_TICKET_SUBMITTED` | Tiket keluhan hubungan kerja dikirim pegawai | `LOW` | TicketNumber, Category, TargetUnit, SLA_Deadline |
| `HRD_WHISTLEBLOWER_REPORT_ENCRYPTED` | Laporan whistleblower terenkripsi diterima sistem | `HIGH` | TicketNumber, EncryptedHash, AnonymousFlag, ReceivedAt |
| `HRD_BIPARTITE_AGREEMENT_REGISTERED` | Perjanjian Bersama (PB) resmi didaftarkan | `HIGH` | ForumId, RegisterNumber, AgreementDocUrl, DisnakerRef |
| `HRD_SEVERANCE_CALCULATION_FINALIZED` | Rincian pesangon PP 35/2021 disetujui Direktur Keu | `CRITICAL` | SeparationId, GrossAmount, TaxFinal, NetAmount, ApprovedBy |
| `HRD_EXIT_CLEARANCE_CHECKLIST_APPROVED` | Item serah terima disetujui unit kliring | `MEDIUM` | ChecklistId, DepartmentType, ItemName, ApprovedByUserId |
| `HRD_FINAL_PAYROLL_UNLOCKED` | Gembok rilis payroll & pesangon dibuka otomatis | `CRITICAL` | SeparationId, EmployeeId, UnlockedAt, AllChecklistsStatus |


---

## 27. Sub-Modul 19: Occupational Health & Staff Safety (K3RS & Medical Check-Up) — Slice `S-C6`

### 27.1 Matriks Hak Akses (Permissions)

| Kode Hak Akses (Permission Key) | Deskripsi Kewenangan | Peran Terotorisasi (Roles) | Batasan & Aturan Keamanan |
|---|---|---|---|
| `Hrd.OccupationalHealth.ViewSummary` | Melihat ringkasan dasbor K3RS operasional & statistik kelaikan | HR Admin, Manajer SDM, Komite K3RS, Direksi RS | Hanya menampilkan angka agregat & status operasional |
| `Hrd.OccupationalHealth.ViewClinicalDetail`| Membaca rincian medis klinis lengkap terenkripsi (ICD-10, lab, rontgen)| Dokter K3RS Bersertifikat, Staf Bersangkutan (ESS) | Terkunci AES-256; HR Admin & Atasan diblokir total |
| `Hrd.OccupationalHealth.CreateMedicalRecord`| Membuat berkas rekam kesehatan staf awal & riwayat medis | Perawat K3RS, Dokter K3RS | Pencatatan data dasar pra-kerja & kesehatan berkala |
| `Hrd.OccupationalHealth.ScheduleMcu` | Menjadwalkan pemeriksaan MCU berkala atau khusus staf | Staf Administrasi K3RS, HR Training & Safety | Penentuan tanggal, lokasi, dan paket pemeriksaan |
| `Hrd.OccupationalHealth.RecordMcuCheckin`| Mencatat kehadiran staf & log pengambilan spesimen MCU | Petugas Registrasi MCU K3RS | Pencetakan label barcode dan bukti kehadiran MCU |
| `Hrd.OccupationalHealth.UploadMcuResults`| Mengunggah berkas hasil laboratorium & radiologi terenkripsi | Analis Laboratorium, Radiografer, Perawat K3RS | Validasi integritas berkas digital (PDF/DICOM/LIS) |
| `Hrd.OccupationalHealth.AssessFitnessToWork`| Menerbitkan vonis status kelayakan kerja staf (Fit/Unfit) | Dokter K3RS Bersertifikat Hiperkes | Wajib menyertakan tanda tangan digital sah dokter |
| `Hrd.OccupationalHealth.IssueWorkRestriction`| Menerbitkan surat pembatasan tugas dinas medis (*Work Restriction*)| Dokter K3RS Bersertifikat Hiperkes | Menyalakan hard-stop validasi penjadwalan Sub-06 |
| `Hrd.OccupationalHealth.RevokeWorkRestriction`| Memperpanjang atau mencabut surat pembatasan tugas kerja | Dokter K3RS Bersertifikat Hiperkes | Wajib mencatat alasan klinis perubahan status restriksi |
| `Hrd.OccupationalHealth.AssessReturnToWork`| Melakukan evaluasi kelaikan kembali bekerja penuh (*Return-to-Work*) | Dokter K3RS Bersertifikat Hiperkes | Syarat mutlak pemulihan jadwal dinas normal staf |
| `Hrd.OccupationalHealth.ReportNeedleStickEmergency`| Menekan tombol darurat pelaporan tertusuk jarum / pajanan darah | Seluruh Pegawai RS (ESS), Atasan Shift | Membuka tiket darurat cito berprioritas tertinggi |
| `Hrd.OccupationalHealth.TriageNeedleStick`| Melakukan evaluasi triase medis darurat pajanan okupasional | Dokter Jaga IGD, Dokter K3RS | Wajib diselesaikan dalam batas waktu < 1 jam kejadian |
| `Hrd.OccupationalHealth.PrescribePEP` | Meresepkan terapi Profilaksis Pasca Pajanan (PPP/PEP) HIV/HBV | Dokter K3RS, Dokter Jaga IGD | Batas target pemberian obat profilaksis < 4 jam |
| `Hrd.OccupationalHealth.ManageSerialSurveillance`| Mengelola jadwal dan hasil surveilans serologi serial 1, 3, 6 bulan | Tim Pencegahan Penyakit Infeksi K3RS | Pemantauan konversi serologis hingga tuntas |
| `Hrd.OccupationalHealth.RecordVaccination`| Mencatat log vaksinasi staf, nomor batch, dan tanggal kedaluwarsa | Vaksinator K3RS, Perawat K3RS | Pencatatan buku vaksinasi elektronik resmi pegawai |
| `Hrd.OccupationalHealth.ViewImmunityRadar`| Melihat radar kepatuhan antibodi & titer protektif nakes RS | Komite K3RS, Tim PPI RS, Manajer SDM | Pemantauan cakupan herd immunity staf per ruangan |
| `Hrd.OccupationalHealth.RecordHazardSurveillance`| Mencatat hasil pemantauan dosis radiasi TLD, audiometri, spirometri | Petugas Proteksi Radiasi (PPR), Staf K3RS | Evaluasi keselamatan lingkungan kerja khusus |
| `Hrd.OccupationalHealth.ReportOccupationalInjury`| Membuat laporan kecelakaan akibat kerja (KAK/PAK) | Staf Korban, Kepala Ruangan, Supervisor | Dokumentasi awal kronologi, saksi, dan waktu kejadian |
| `Hrd.OccupationalHealth.InvestigateInjury`| Mengisi berita acara investigasi insiden, RCA, dan rekomendasi CAPA | Tim Investigator K3RS, Manajer Mutu K3RS | Analisis akar masalah keselamatan kerja rumah sakit |
| `Hrd.OccupationalHealth.SubmitBPJSClaimStage1`| Mengunggah dan mengirim formulir pelaporan BPJS JKK Tahap I | Staf Klaim Asuransi HR K3RS | Pengiriman ke portal BPJS maksimal 2x24 jam insiden |
| `Hrd.OccupationalHealth.FinalizeBPJSClaimStage2`| Mengajukan berkas penggantian biaya medis BPJS JKK Tahap II | Staf Klaim Asuransi HR K3RS | Pengesahan kwitansi biaya pengobatan dan santunan |
| `Hrd.OccupationalHealth.ExportStarkesReport`| Mengunduh laporan kepatuhan K3RS standar akreditasi STARKES KPS | Direktur SDM, Komite K3RS, Tim Akreditasi | Format resmi matriks pemenuhan kesehatan pegawai |
| `Hrd.OccupationalHealth.ConfigureHazardMatrix`| Mengonfigurasi pemetaan bahaya ruangan kerja & paket MCU wajib | Administrator K3RS, Kepala Instalasi K3RS | Pengaturan aturan bisnis penapisan risiko lingkungan |
| `Hrd.OccupationalHealth.ManageAuditTrail`| Mengaudit rekam jejak akses berkas medis sensitif staf K3RS | Petugas Kepatuhan Privasi Data (DPO), IT Auditor | Verifikasi tidak adanya kebocoran rekam medis |

---

### 27.2 Matriks Jejak Audit Keamanan & Akuntabilitas (Audit Events)

| Kode Peristiwa Audit | Deskripsi Peristiwa | Kategori Tingkat Risiko | Detail Data yang Dicatat |
|---|---|---|---|
| `HRD_OHS_CLINICAL_RECORD_ACCESSED` | Berkas rekam medis klinis sensitif staf dibuka | `CRITICAL` | EmployeeId, AccessingDoctorId, RecordId, Purpose, AccessTimestamp |
| `HRD_OHS_CLINICAL_RECORD_UNAUTHORIZED_BLOCKED`| Upaya akses berkas klinis oleh staf non-medis diblokir | `CRITICAL` | BlockedUserId, TargetEmployeeId, AttemptedEndpoint, ClientIpAddress |
| `HRD_OHS_FITNESS_TO_WORK_DECIDED` | Putusan status kelaikan kerja staf disahkan | `HIGH` | EmployeeId, FitnessStatus, DoctorLicenseNumber, ValidUntil |
| `HRD_OHS_WORK_RESTRICTION_ISSUED` | Surat pembatasan tugas kerja resmi diterbitkan | `HIGH` | EmployeeId, RestrictionCategory, ValidUntil, RosterLockApplied |
| `HRD_OHS_WORK_RESTRICTION_CLEARED` | Pembatasan tugas kerja dicabut pasca asesmen RTW | `MEDIUM` | EmployeeId, RestrictionId, RtwAssessmentId, ClearedAt |
| `HRD_OHS_NEEDLE_STICK_EMERGENCY_REPORTED`| Tombol darurat tertusuk jarum ditekan via portal ESS | `CRITICAL` | EmployeeId, IncidentTimestamp, ExposureLocation, SirenDispatched |
| `HRD_OHS_PEP_PRESCRIBED_TIMELY` | Resep Profilaksis Pasca Pajanan diberikan tepat waktu | `HIGH` | IncidentId, EmployeeId, PepStartTime, ElapsedHoursFromIncident |
| `HRD_OHS_SERIAL_SURVEILLANCE_SCHEDULED`| Jadwal pemantauan serologi serial 1, 3, 6 bulan aktif | `MEDIUM` | IncidentId, EmployeeId, SerialSurveillanceMilestonesList |
| `HRD_OHS_VACCINATION_LOGGED` | Log imunisasi staf dan nomor batch vaksin dicatat | `LOW` | EmployeeId, VaccineName, BatchNumber, DoseSequence, ExpiryDate |
| `HRD_OHS_LOW_ANTIBODY_BOOSTER_TRIGGERED`| Titer Anti-HBs non-protektif memicu jadwal booster | `MEDIUM` | EmployeeId, TiterValue, ThresholdValue, AutoBoosterTicketId |
| `HRD_OHS_RADIATION_DOSE_LIMIT_EXCEEDED`| Dosis serap radiasi TLD melampaui batas ambang aman | `CRITICAL` | EmployeeId, CumulativeDose_mSv, UnitName, UrgentRotationTriggered |
| `HRD_OHS_INJURY_BPJS_STAGE1_SUBMITTED`| Formulir klaim JKK Tahap I terkirim ke portal BPJS | `HIGH` | InjuryId, BPJSTicketNumber, SubmissionTimestamp, ElapsedHours |
| `HRD_OHS_INJURY_BPJS_STAGE2_FINALIZED`| Berkas klaim biaya JKK Tahap II selesai dan cair | `HIGH` | InjuryId, BPJSApprovalNumber, ReimbursedAmount, FinalizedAt |
| `HRD_OHS_STARKES_COMPLIANCE_EXPORTED`| Dokumen kepatuhan K3RS STARKES KPS diunduh resmi | `LOW` | ExportedByUserId, ReportingPeriod, TotalStaffAuditedCount |


---

## 28. Sub-Modul 21: Employee Self-Service (ESS) & Unified Approval Inbox (V2 Hardening) — Slice `S-A7-V2`

### 28.1 Matriks Hak Akses (Permissions)

| Kode Hak Akses (Permission Key) | Deskripsi Kewenangan | Peran Terotorisasi (Roles) | Batasan & Aturan Keamanan |
|---|---|---|---|
| `Hrd.Workflow.ViewInbox` | Melihat kotak masuk persetujuan terpadu pribadi | Kepala Ruangan, Supervisor, Manajer, Direksi, Plh | Hanya menampilkan tiket yang ditugaskan ke approver |
| `Hrd.Workflow.ViewInstanceDetail` | Membaca rincian lengkap tiket permohonan, data pemohon, dan lampiran | Pemohon Asli, Approver Aktif, HR Admin | Pemohon hanya bisa melihat tiket miliknya sendiri |
| `Hrd.Workflow.ApproveRequest` | Mengeksekusi keputusan persetujuan pada tahapan aktif | Approver Ditugaskan, Pejabat Plh Sah | Dilarang self-approval (`VR-WF-02`); wajib PIN |
| `Hrd.Workflow.RejectRequest` | Menolak permohonan kepegawaian | Approver Ditugaskan, Pejabat Plh Sah | Wajib menyertakan alasan tertulis minimal 15 karakter |
| `Hrd.Workflow.ReturnForRevision` | Mengembalikan permohonan ke staf untuk perbaikan data | Approver Ditugaskan, Pejabat Plh Sah | Wajib memberikan instruksi perbaikan tertulis |
| `Hrd.Workflow.BulkApprove` | Melakukan persetujuan massal untuk permohonan berisiko rendah | Approver Manajerial (Karu, Kabid, Manajer) | Hanya aktif untuk permohonan tervalidasi bersih |
| `Hrd.Workflow.CreateInstance` | Mengajukan permohonan mandiri dari portal ESS | Seluruh Pegawai Rumah Sakit (ESS) | Pengisian data permohonan sesuai hak aksesnya |
| `Hrd.Workflow.CancelMyInstance` | Membatalkan permohonan mandiri yang belum disetujui | Staf Pemohon Asli | Hanya aktif jika tiket belum berstatus disetujui |
| `Hrd.Workflow.ResubmitRevision` | Mengajukan ulang permohonan yang telah diperbaiki staf | Staf Pemohon Asli | Wajib menyertakan ringkasan keterangan perbaikan |
| `Hrd.Workflow.AddComment` | Menambahkan komentar klarifikasi pada thread tiket | Pemohon, Approver Aktif | Percakapan terekam permanen dalam riwayat tiket |
| `Hrd.Workflow.ViewInternalComment`| Membaca catatan rahasia antar approver | Approver Struktural, Komite Medik, HR | Dilarang tampak pada layar staf pemohon |
| `Hrd.Workflow.UploadAttachment` | Mengunggah berkas lampiran pendukung baru | Pemohon, Approver | Format PDF/JPG maksimal 10 MB |
| `Hrd.Workflow.ViewDelegationList` | Melihat daftar riwayat pendelegasian wewenang Plh | Pejabat Approver, HR Admin | Monitoring penugasan Plh aktif |
| `Hrd.Workflow.CreateDelegation` | Mendaftarkan penunjukan Pelaksana Harian (Plh) berbatas waktu | Pejabat Struktural Definitif | Plh minimal setara atau 1 tingkat di bawahnya |
| `Hrd.Workflow.RevokeDelegation` | Mencabut wewenang delegasi Plh sebelum masa berlaku habis | Pemberi Wewenang, HR Admin | Mengembalikan wewenang ke pejabat definitif |
| `Hrd.Workflow.ActAsDelegate` | Mengeksekusi aksi persetujuan atas nama pejabat definitif | Pegawai Ditunjuk sebagai Plh Sah | Mencatat jejak audit dual-identity secara permanen |
| `Hrd.Workflow.ConfigureMatrix` | Mengonfigurasi matriks alur persetujuan per unit kerja | HR Admin, Direktur SDM | Pengaturan aturan alur workflow rumah sakit |
| `Hrd.Workflow.ViewMatrix` | Melihat visualisasi diagram pohon alur persetujuan | HR Admin, Kepala Bagian, Auditor Internal | Pemeriksaan kepatuhan alur otorisasi |
| `Hrd.Workflow.ViewSlaOverdue` | Melihat papan radar pemantauan tiket yang melewati batas SLA | Manajer SDM, Direksi, Tim Mutu RS | Pemantauan kedisiplinan respon atasan |
| `Hrd.Workflow.ManualEscalate` | Melakukan eskalasi manual tiket yang macet ke atasan atas | HR Admin, Manajer SDM | Intervensi administratif atas permohonan darurat |
| `Hrd.Workflow.ExportSlaReport` | Mengunduh laporan kepatuhan SLA persetujuan per unit kerja | Direktur SDM, Direktur Utama, Komite Mutu | Evaluasi kecepatan birokrasi internal RS |
| `Hrd.Workflow.ViewAuditTrail` | Memeriksa seluruh rekam jejak kronologis audit workflow | Petugas Kepatuhan, IT Auditor | Verifikasi tidak adanya manipulasi wewenang |
| `Hrd.Workflow.ManageSystemWorkers`| Mengatur konfigurasi worker pemantau SLA dan delegasi | IT Administrator SIMRS | Pemeliharaan berkala background workers |
| `Hrd.Workflow.SuperAdminBypass` | Intervensi darurat tingkat tinggi atas izin Direktur Utama | Direktur Utama, HR Director | Tindakan darurat kebencanaan/force majeure |

---

### 28.2 Matriks Jejak Audit Keamanan & Akuntabilitas (Audit Events)

| Kode Peristiwa Audit | Deskripsi Peristiwa | Kategori Tingkat Risiko | Detail Data yang Dicatat |
|---|---|---|---|
| `HRD_WF_INSTANCE_SUBMITTED` | Permohonan baru diajukan secara resmi dari ESS | `LOW` | WorkflowNumber, RequestType, ApplicantEmployeeId, DepartmentId, SubmittedAt |
| `HRD_WF_STEP_ASSIGNED` | Tiket berhasil masuk ke antrean inbox approver tertentu | `LOW` | WorkflowNumber, StepNumber, ApproverEmployeeId, SlaDeadline |
| `HRD_WF_REQUEST_APPROVED` | Keputusan persetujuan disahkan oleh approver | `MEDIUM` | WorkflowNumber, StepNumber, ApproverEmployeeId, ActionTimestamp, SignatureHash |
| `HRD_WF_REQUEST_REJECTED` | Keputusan penolakan final disahkan oleh approver | `MEDIUM` | WorkflowNumber, StepNumber, ApproverEmployeeId, RejectionReasonText |
| `HRD_WF_RETURNED_FOR_REVISION` | Permohonan dikembalikan ke staf untuk perbaikan data | `LOW` | WorkflowNumber, StepNumber, ApproverEmployeeId, RevisionInstructions |
| `HRD_WF_INSTANCE_RESUBMITTED` | Staf mengajukan ulang permohonan yang telah direvisi | `LOW` | WorkflowNumber, ApplicantEmployeeId, ResubmittedAt, ChangesSummary |
| `HRD_WF_BULK_APPROVAL_EXECUTED` | Persetujuan massal bersyarat berhasil dieksekusi | `HIGH` | ApproverEmployeeId, TotalProcessedCount, SuccessCount, SkippedCount |
| `HRD_WF_DELEGATION_REGISTERED` | Surat kuasa digital penunjukan Plh diterbitkan | `HIGH` | PrincipalEmployeeId, DelegateEmployeeId, StartDate, EndDate, RequestTypes |
| `HRD_WF_DELEGATION_REVOKED` | Wewenang delegasi Plh dicabut sebelum tanggal akhir | `MEDIUM` | PrincipalEmployeeId, RevokedByEmployeeId, RevokedAt, Reason |
| `HRD_WF_ACTING_APPROVAL_EXECUTED`| Keputusan persetujuan dieksekusi oleh Plh atas nama definitif | `HIGH` | WorkflowNumber, ActingPlhId, OnBehalfOfId, ActionType, Timestamp |
| `HRD_WF_SLA_TIMEOUT_ESCALATED` | Tiket melewati batas SLA dan otomatis dieskalasi ke atasan | `HIGH` | WorkflowNumber, StepNumber, DefaultApproverId, EscalatedToApproverId |
| `HRD_WF_DOWNSTREAM_ACTION_TRIGGERED`| Aksi final memicu perubahan data pada domain modul hilir | `CRITICAL` | WorkflowNumber, ReferenceDomain, ReferenceEntityId, TriggerTimestamp |
| `HRD_WF_DOWNSTREAM_RECONCILIATION_RUN`| Worker rekonsiliasi mengeksekusi ulang aksi hilir yang macet | `HIGH` | WorkflowNumber, ReferenceDomain, RetryAttemptCount, ResolutionStatus |
| `HRD_WF_MATRIX_POLICY_UPDATED` | Konfigurasi matriks alur persetujuan diperbarui admin | `CRITICAL` | UpdatedByUserId, DepartmentId, RequestType, PreviousSteps, NewStepsConfig |


---

## 29. Matriks Hak Akses & Audit Trail Sub-Modul 13: Perjalanan Dinas & Manajemen Biaya Operasional (Business Travel & Expense Management) — Slice `S-D5`

### 29.1 Matriks 24 Hak Akses (Permissions)

| Kode Permission | Deskripsi Wewenang | Role Pemilik Akses |
|---|---|---|
| `Hrd.BusinessTravel.Request.View` | Melihat riwayat pengajuan SPPD dinas | Seluruh Pegawai RS (ESS), Atasan, HR |
| `Hrd.BusinessTravel.Request.Create` | Mengajukan permohonan perjalanan dinas baru | Seluruh Pegawai RS (ESS) |
| `Hrd.BusinessTravel.Request.Edit` | Mengubah draf SPPD sebelum diajukan | Pemohon Asli |
| `Hrd.BusinessTravel.Request.Cancel` | Membatalkan perjalanan dinas sebelum berangkat | Pemohon, Atasan Langsung |
| `Hrd.BusinessTravel.Request.Approve` | Otorisasi persetujuan penugasan dinas SPPD | Kepala Ruangan, Manajer, Direksi |
| `Hrd.BusinessTravel.Advance.Request` | Mengajukan uang muka perjalanan dinas | Pemohon SPPD |
| `Hrd.BusinessTravel.Advance.Approve` | Persetujuan nominal uang muka kerja | Finance Manager, Kabid Keuangan |
| `Hrd.BusinessTravel.Advance.Disburse` | Otorisasi pencairan dana uang muka kasir | Kasir RS, Treasury |
| `Hrd.BusinessTravel.Settlement.Submit` | Menyerahkan laporan pertanggungjawaban biaya | Pemohon SPPD |
| `Hrd.BusinessTravel.Settlement.Verify` | Memeriksa keabsahan fisik bukti kuitansi riil | Verifikator Keuangan RS |
| `Hrd.BusinessTravel.Settlement.Close` | Menyelesaikan transaksi kas selisih settlement | Kasir RS, Treasury |
| `Hrd.BusinessTravel.MissingReceipt.Declare` | Mengajukan deklarasi darurat tanpa nota | Pemohon SPPD |
| `Hrd.BusinessTravel.MissingReceipt.Approve` | Otorisasi persetujuan deklarasi darurat | Atasan Langsung, Direktur Keuangan |
| `Hrd.BusinessTravel.Rate.View` | Melihat tabel matriks tarif lumpsum per diem | Seluruh Pegawai RS |
| `Hrd.BusinessTravel.Rate.Manage` | Mengonfigurasi tarif uang harian dan zonasi | HR Administrator, Manajer SDM |
| `Hrd.BusinessTravel.Overdue.Monitor` | Memantau radar keterlambatan settlement | HR Admin, Finance Manager, Direksi |
| `Hrd.Expense.Claim.View` | Melihat daftar klaim pengeluaran non-trip | Seluruh Pegawai RS, Kasir |
| `Hrd.Expense.Claim.Create` | Mengajukan klaim biaya operasional langsung | Seluruh Pegawai RS (ESS) |
| `Hrd.Expense.Claim.Edit` | Mengedit draf klaim biaya operasional non-trip | Pemohon Asli |
| `Hrd.Expense.Claim.Approve` | Persetujuan klaim operasional oleh atasan | Kepala Unit Kerja, Manajer Divisi |
| `Hrd.Expense.Claim.Verify` | Audit keabsahan nota pembelian darurat | Verifikator Keuangan RS |
| `Hrd.Expense.Claim.Pay` | Pencairan dana reimbursement non-trip kasir | Kasir RS, Treasury |
| `Hrd.Expense.Category.Manage` | Mengelola master pos beban biaya operasional | Finance Admin, Akuntansi |
| `Hrd.BusinessTravel.Audit.View` | Mengunduh log audit transaksi perjalanan dinas | Auditor Internal RS, Komite Etik |

---

### 29.2 Matriks 14 Event Audit Permanen (*Immutable Audit Trail*)

| Kode Event Audit | Pemicu Peristiwa (Trigger) | Data Kunci yang Direkam | Tingkat Kritis |
|---|---|---|---|
| `HRD_TRV_REQUEST_SUBMITTED` | Staf mengirimkan permohonan SPPD baru | `TravelRequestNumber`, `ApplicantId`, `Destination`, `StartDate` | Normal |
| `HRD_TRV_REQUEST_APPROVED` | Atasan mengesahkan surat tugas dinas | `TravelRequestNumber`, `ApproverId`, `ApprovedBudgetAmount` | Tinggi |
| `HRD_TRV_ADVANCE_REQUESTED` | Permohonan uang muka diajukan | `AdvanceNumber`, `RequestedAmount`, `TravelRequestNumber` | Tinggi |
| `HRD_TRV_ADVANCE_PAID` | Kasir mencairkan uang muka dinas | `AdvanceNumber`, `DisbursedAmount`, `VoucherNumber`, `CashierId` | Finansial Kritis |
| `HRD_TRV_SETTLEMENT_SUBMITTED` | Staf menyerahkan laporan settlement LPJ | `SettlementNumber`, `TotalAdvance`, `TotalActual`, `DiffAmount` | Tinggi |
| `HRD_TRV_MISSING_RECEIPT_DECLARED` | Pengajuan deklarasi darurat tanpa nota | `SettlementNumber`, `ItemAmount`, `EmergencyReason`, `ApproverId` | Finansial Kritis |
| `HRD_TRV_SETTLEMENT_VERIFIED` | Tim keuangan mengesahkan bukti kuitansi | `SettlementNumber`, `VerifiedActualAmount`, `VerifierId` | Finansial Kritis |
| `HRD_TRV_SETTLEMENT_CLOSED` | Selisih kas dituntaskan oleh Kasir RS | `SettlementNumber`, `FinalDiffPaidOrRefunded`, `CashierId` | Finansial Kritis |
| `HRD_TRV_SPPD_FROZEN_OVERDUE` | Pembekuan hak SPPD karena telat LPJ > 14 hari | `EmployeeId`, `OverdueDays`, `UnsettledAdvanceAmount` | Tinggi |
| `HRD_TRV_PAYROLL_DEDUCTION_TRIGGERED`| Pemotongan gaji otomatis tunggakan > 30 hari | `EmployeeId`, `DeductionAmount`, `PayrollPeriodId` | Finansial Kritis |
| `HRD_TRV_ATTENDANCE_SYNCED` | Suntik presensi otomatis "Hadir Dinas Luar" | `EmployeeId`, `DutyDate`, `TravelRequestNumber` | Normal |
| `HRD_TRV_ROSTER_LOCK_APPLIED` | Pemasangan gembok hard-stop roster dinas bangsal | `EmployeeId`, `RestrictedDates`, `WardUnitId` | Keselamatan Pasien |
| `HRD_EXP_CLAIM_SUBMITTED` | Staf mengajukan klaim operasional non-trip | `ExpenseClaimNumber`, `EmployeeId`, `TotalAmount`, `CategoryId`| Normal |
| `HRD_EXP_CLAIM_PAID` | Kasir mencairkan klaim operasional non-trip | `ExpenseClaimNumber`, `PaidAmount`, `VoucherNumber`, `CashierId` | Finansial Kritis |

---

## 30. Matriks Hak Akses & Audit Amandemen `S-B4`/`S-B1` (`A52`, kontrak `v26`, 12 November 2026)

| Field | Nilai |
| --- | --- |
| `contract_version` | `v26` — `approved` (pengguna, 12 November 2026) |
| Owner | Pengguna (`HRD-DEC-015`) |
| `input_revision` | `00-interview-decisions.md` revision `37`; `02-backend-architecture.md` bagian 35 |
| Aturan string | Argumen pertama `[AccessPermission]` sama persis dengan `ControllerName`; argumen kedua sama dengan argumen pertama `[AccessAction]` (`evidence/03-hr-authorization-remediation.md` bagian 2.1) |
| Selisih dengan kontrak `v10` | String `v10` bagian 14 (`HrdScheduling`, `HrdOnCall`, `HrdSchedulingSetting`) tidak dipakai kode. Kontrak `v26` memakai string kode yang berlaku |

### 30.1 String hak akses

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET roster-periods/{id}/revisions` | `RosterPeriod` | `Read` | `[AccessPermission("RosterPeriod", "Read")]` | Tidak |
| `POST roster-periods/{id}/assignments` | `RosterPeriod` | `Create` | `[AccessPermission("RosterPeriod", "Create")]` | Ya |
| `POST roster-periods/{id}/shifts/{shiftAssignmentId}/remove` | `RosterPeriod` | `RemoveShift` | `[AccessPermission("RosterPeriod", "RemoveShift")]` | Ya |
| `POST shift-replacements/{id}/replace`, `/retroactive-correct` | `ShiftReplacement` | `Update` | `[AccessPermission("ShiftReplacement", "Update")]` | Ya |
| `GET on-call-assignments/{id}/backups`, `/calls` | `OnCallAssignment` | `Read` | `[AccessPermission("OnCallAssignment", "Read")]` | Tidak |
| `PUT on-call-assignments/{id}/backups` | `OnCallAssignment` | `ManageBackup` | `[AccessPermission("OnCallAssignment", "ManageBackup")]` | Ya |
| `POST on-call-assignments/{id}/calls` dan `/calls/{callId}/accept`, `/decline`, `/expire`, `/complete` | `OnCallAssignment` | `RecordCall` | `[AccessPermission("OnCallAssignment", "RecordCall")]` | Ya |
| `GET`/`PUT attendance-management/settings` | `AttendanceSetting` | `Read` / `Update` | `[AccessPermission("AttendanceSetting", "Read")]`, `[AccessPermission("AttendanceSetting", "Update")]` | `PUT` ya |
| `GET attendance/dispensations`, `/{id}`, `/filters/metadata` | `AttendanceDispensation` | `Read` | `[AccessPermission("AttendanceDispensation", "Read")]` | Tidak |
| `POST attendance/dispensations` | `AttendanceDispensation` | `Create` | `[AccessPermission("AttendanceDispensation", "Create")]` | Ya |
| `POST attendance/dispensations/{id}/approve`, `/reject` | `AttendanceDispensation` | `Approve` | `[AccessPermission("AttendanceDispensation", "Approve")]` | Ya |
| `GET self-services/.../attendance-dispensations` | `MyAttendanceDispensation` | `Read` | `[AccessPermission("MyAttendanceDispensation", "Read")]` | Tidak |
| `POST self-services/.../attendance-dispensations` | `MyAttendanceDispensation` | `Create` | `[AccessPermission("MyAttendanceDispensation", "Create")]` | Ya |
| `POST self-services/.../attendance-dispensations/{id}/cancel` | `MyAttendanceDispensation` | `Cancel` | `[AccessPermission("MyAttendanceDispensation", "Cancel")]` | Ya |
| `GET attendance/study-assignments`, `/{id}` | `StudyAssignment` | `Read` | `[AccessPermission("StudyAssignment", "Read")]` | Tidak |
| `POST attendance/study-assignments` | `StudyAssignment` | `Create` | `[AccessPermission("StudyAssignment", "Create")]` | Ya |
| `PUT attendance/study-assignments/{id}` | `StudyAssignment` | `Update` | `[AccessPermission("StudyAssignment", "Update")]` | Ya |
| `POST attendance/study-assignments/{id}/cancel` | `StudyAssignment` | `Cancel` | `[AccessPermission("StudyAssignment", "Cancel")]` | Ya |
| `GET attendance/device-incidents` | `AttendanceDeviceIncident` | `Read` | `[AccessPermission("AttendanceDeviceIncident", "Read")]` | Tidak |
| `POST attendance/device-incidents` | `AttendanceDeviceIncident` | `Create` | `[AccessPermission("AttendanceDeviceIncident", "Create")]` | Ya |
| `POST attendance/device-incidents/{id}/cancel` | `AttendanceDeviceIncident` | `Cancel` | `[AccessPermission("AttendanceDeviceIncident", "Cancel")]` | Ya |
| `GET self-services/.../attendance-corrections/device-incident-options` | `MyAttendanceCorrection` | `Read` | `[AccessPermission("MyAttendanceCorrection", "Read")]` | Tidak |
| `GET master-data/holidays*` | `Holiday` | `Read` | `[AccessPermission("Holiday", "Read")]` | Tidak |
| `POST master-data/holidays` | `Holiday` | `Create` | `[AccessPermission("Holiday", "Create")]` | Ya |
| `PUT`/`PATCH master-data/holidays/{id}` | `Holiday` | `Update` | `[AccessPermission("Holiday", "Update")]` | Ya |
| `DELETE master-data/holidays/{id}` | `Holiday` | `Delete` | `[AccessPermission("Holiday", "Delete")]` | Ya |
| `GET master-data/attendance-devices*` | `AttendanceDevice` | `Read` | `[AccessPermission("AttendanceDevice", "Read")]` | Tidak |
| `POST master-data/attendance-devices` | `AttendanceDevice` | `Create` | `[AccessPermission("AttendanceDevice", "Create")]` | Ya |
| `PUT`/`PATCH master-data/attendance-devices/{id}` | `AttendanceDevice` | `Update` | `[AccessPermission("AttendanceDevice", "Update")]` | Ya |
| `DELETE master-data/attendance-devices/{id}` | `AttendanceDevice` | `Delete` | `[AccessPermission("AttendanceDevice", "Delete")]` | Ya |
| `GET attendance/dashboard*` | `AttendanceDashboard` | `Read` | `[AccessPermission("AttendanceDashboard", "Read")]` | Tidak |
| `GET attendance/reports/{reportCode}` | `AttendanceReport` | `Read` | `[AccessPermission("AttendanceReport", "Read")]` | Tidak |
| `GET attendance/reports/{reportCode}/export` | `AttendanceReport` | `Export` | `[AccessPermission("AttendanceReport", "Export")]` | **Ya** — pengecualian aturan "GET tidak dicatat", karena `HRD-DEC-206` mewajibkan setiap ekspor tercatat |
| `GET self-services/.../my-schedule`, `/unit-roster` | `MySchedule` | `Read` | `[AccessPermission("MySchedule", "Read")]` | Tidak |

**Penanda tanpa endpoint** (`[assembly: AccessExplicitPermission(...)]`):

| Resource | Action | Arti |
| --- | --- | --- |
| `RosterPeriod` | `ManageAllUnits` | Mengelola roster semua unit |
| `AttendanceReport` | `ReadAllUnits` | Membaca dashboard, laporan, roster, dan dispensasi semua unit |

### 30.2 Matriks peran dan tindakan

| Tindakan | Kepala Unit | Kepala Bidang | Admin HRD | Petugas IT | Unit pemanggil siaga | Pegawai |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| Ubah roster terbit (Ganti Petugas, tambah, hapus, koreksi ≤ 3 hari) | Unit sendiri | 403 | Bila diberi `ManageAllUnits` | 403 | 403 | 403 |
| Lihat Riwayat Revisi | Unit sendiri | Unit di bawahnya | Semua (`ReadAllUnits`) | 403 | 403 | 403 |
| Kelola cadangan siaga | Unit sendiri (staf penunjang) | 403 | Ya | 403 | 403 | 403 |
| Catat panggilan dan jawaban siaga | Ya | 403 | Ya | 403 | Ya | 403 |
| Ajukan dispensasi | Atas nama pegawai unitnya | 403 | 403 | 403 | 403 | Untuk diri sendiri |
| Setujui/tolak dispensasi | Bila tercatat sebagai atasan langsung | Bila atasan langsung | 403 | 403 | 403 | 403 |
| Catat tugas belajar | 403 | 403 | Ya | 403 | 403 | 403 |
| Catat gangguan mesin | 403 | 403 | Ya | Ya | 403 | 403 |
| Kelola daftar mesin | 403 | 403 | 403 | Ya | 403 | 403 |
| Kelola hari libur | 403 | 403 | Ya | 403 | 403 | 403 |
| Dashboard dan laporan | Unit sendiri | Unit di bawahnya | Semua | 403 | 403 | 403 |
| Ekspor laporan | Unit sendiri | Unit di bawahnya | Semua | 403 | 403 | 403 |
| Jadwal Saya | Ya | Ya | Ya | Ya | Ya | Ya |
| Penempatan sementara (penugasan tambahan) | 403 | 403 | Ya (hak `WfpOrganizationAssignment` yang ada) | 403 | 403 | 403 |

Auditor, petugas payroll, dan direksi dapat diberi `AttendanceReport : Read`/`Export` dan
`ReadAllUnits` sesuai kebutuhan (`HRD-ASM-79`).

### 30.3 Jejak audit

Ditulis lewat `LoggerService.AuditAsync`. **Isi ditulis di teks pesan**, karena metode itu tidak
menyimpan objek payload. Kolom bertanda sensitif pada kamus data bagian 27 tidak pernah masuk.

| Kejadian | Isi teks pesan | Tidak boleh dimuat |
| --- | --- | --- |
| Perubahan roster terbit | Jenis perubahan, ID shift, tanggal, ID pegawai sebelum/sesudah, kategori alasan, pelaku | `ReasonNote` |
| Catat/jawab/kedaluwarsa/selesai panggilan siaga | ID penugasan, ID panggilan, status baru, pelaku, jam | `CallReason`, `DeclineReason` |
| Dispensasi diajukan/disetujui/ditolak/dibatalkan | ID dispensasi, ID pegawai, tanggal, status baru, pelaku | `Reason` |
| Tugas belajar dicatat/diubah/dibatalkan | ID, ID pegawai, tanggal mulai/selesai, pelaku | — |
| Gangguan mesin dicatat/dibatalkan | ID kejadian, ID mesin, rentang jam, pelaku | — |
| Mesin didaftarkan/diaktifkan | ID mesin, jumlah scan tertahan yang dilepas, pelaku | — |
| Setelan kehadiran diubah | Nilai lama dan baru toleransi serta jendela, pelaku | — |
| **Ekspor laporan** | Kode laporan, filter (unit, rentang tanggal, status), jumlah baris, pelaku, waktu | Isi baris |
