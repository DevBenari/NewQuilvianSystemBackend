# Human Resource — Matriks Hak Akses dan Jejak Audit

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/permission-audit-matrix.md` |
| `contract_version` | `v5` |
| `last_changed_in` | `v5` |
| Status | `draft` — **belum** `approved` |
| Owner | Pemilik keamanan bersama technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Belum ada** |
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
