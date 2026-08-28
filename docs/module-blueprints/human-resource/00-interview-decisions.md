# Human Resource — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `HRD-BP-001` |
| Revision | `10` — revision `0` Scope Pass, `1` Closure Pass, `2` Amendment Pass, `3` Amendment Pass 1.1 Konsistensi dan Penamaan, seluruhnya 27 Agustus 2026. Revision `2` menyerap `HRD-DEC-016` s.d. `HRD-DEC-018` dan menarik `HRD-TF-001`. **Revision `3` menyerap `HRD-DEC-019`** kebijakan penamaan canonical yang menggantikan `HRD-DEC-017`, ditambah perbaikan hitungan slice dan definisi angka 68/67. Revision `4` menutup `HRD-Q-16` dan `HRD-Q-17` lewat `HRD-DEC-020` dan `HRD-DEC-021`. Revision `5` mendaftarkan `HRD-Q-18` s.d. `HRD-Q-33` yang lahir dari PHASE 2A. **Revision `6` (bagian 20) adalah PHASE 2A.1 — Flow Evidence Hardening**: audit source read-only atas flow 01–04, menutup `HRD-Q-21`, `HRD-Q-24`, `HRD-Q-28`, dan bagian source-resolvable `HRD-Q-22`; menemukan tiga celah implementasi baru (`HRD-Q-34`, `HRD-Q-35`, `HRD-Q-36`); dan menurunkan sejumlah edge transisi dari `[EXISTING]` menjadi `[OPEN]` atau `PERMISSION_MAPPING`. **Revision `7` (bagian 21) adalah PHASE 2A.2 — Owner Decision Closure**: `HRD-DEC-022` s.d. `HRD-DEC-025` menutup `HRD-Q-34`, `HRD-Q-35`, `HRD-Q-36`, dan sisa `HRD-Q-22`; header baseline SHA dipisah audited vs current verified. **Revision `8` (bagian 22) adalah PHASE 2B**: flow 05–09 ditulis (penjadwalan kerja, ubah jadwal/tukar shift, koreksi kehadiran, izin pulang cepat, kotak masuk terpadu); mendaftarkan `HRD-Q-37` s.d. `HRD-Q-46`; menutup `HRD-Q-12` dan `HRD-Q-13`; mencatat satu kontradiksi belum-rekonsiliasi terhadap flow 03 (`HRD-Q-44`). **Revision `9` (bagian 23) adalah PHASE 2B.1 — Source Closure & Product Decision Pass**: menutup `HRD-Q-39`, `HRD-Q-41`, `HRD-Q-44`, `HRD-Q-46`, dan `AC-F07-02` lewat audit source; `HRD-DEC-026` s.d. `HRD-DEC-030` menutup `HRD-Q-37`, `HRD-Q-38`, `HRD-Q-40`, `HRD-Q-42`, `HRD-Q-43`, dan `HRD-Q-45`; mendaftarkan `HRD-Q-47`; mengoreksi wording flow 05 dan flow 08; memperbarui classification roadmap; mengoreksi cakupan `PHASE 2C` menjadi flow 10–14. **Revision `10` (bagian 24–25) mencatat `HRD-Q-48` (fallback 480 menit) dan penegasan klasifikasi `TrxLeaveRequestApproval`, lalu menulis `PHASE 2C`**: flow 10–14 (payroll `PARTIAL`, lifecycle/offboarding, kompetensi/pelatihan, kinerja, hubungan karyawan/disiplin); mendaftarkan `HRD-Q-49` s.d. `HRD-Q-53`. Tidak ada source code, database, atau frontend yang diubah pada revision manapun sejak revision 5. Seluruh pass sebelumnya tetap utuh dan ditandai HISTORICAL SNAPSHOT |
| Status | `draft`. Sebelas keputusan rekayasa dan produk teknis sudah `approved` oleh pemilik teknis yang ditetapkan `HRD-DEC-015`. Dua keputusan sensitif — `HRD-DEC-005` gerbang kredensial dan `HRD-DEC-010` privasi rekam kesehatan — tetap `draft` sampai Komite Medik dan K3RS mengesahkan. Nilai kebijakan PRD pasal 28 belum tersentuh |
| Pass | **Scope pass**, **Closure pass**, **Amendment pass**, lalu **Amendment pass 1.1**. Rinciannya ada di bagian 14, 15, dan 16 |
| Module | `human-resource` / `HumanResource`, prefix entity `Hrd` |
| Product/domain owner | **Pemilik teknis:** pengguna, ditetapkan `HRD-DEC-015`. **Pemilik kebijakan bisnis, Komite Medik, dan K3RS:** masih `OPEN`, lihat `HRD-Q-01` |
| Backend audited SHA | `ecdc135` (branch `AndryZain`, repository `NewQuilvianSystemBackend`) — **historical**, tempat seluruh fakta source pada dokumen ini pertama kali dibuktikan. Tidak diganti agar provenance audit lama tetap utuh |
| Backend current verified baseline | `16b8b71` (`origin/QuilvianIntegrationBackend`) — **implementation authority terkini**, ditetapkan `HRD-DEC-021`. Impact scan bagian 17.1 membuktikan seluruh source HR identik byte-per-byte dengan `ecdc135`, sehingga fakta yang diaudit tetap berlaku penuh di baseline ini |
| Frontend SHA | `2a1cea784` (branch `AgentCodexFrontend`, repository `QuilvianSystemFrontendDev`) |
| Tanggal pass | 2026-08-27 |
| Capability map | **Sudah ada sejak 27 Agustus 2026.** [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `1.0`, status `source-audited`. Scope pada dokumen ini semula dikunci tanpa audit; audit itu kini tersedia dan mengoreksi beberapa pembacaan awal, lihat bagian 13 |
| Masukan produk | `docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md` — dibuat di luar alur skill, statusnya di sini **belum** menjadi PRD blueprint yang berlaku |

---

## 0. Kenapa dokumen ini ada

Modul Human Resource sudah dikerjakan lebih dulu di backend dan frontend, sementara jalur
blueprint yang dipakai modul lain (`rawat-inap`, `igd`, `billing-kasir`, `pharmacy`, dan
seterusnya) belum pernah dibuka untuk modul ini. Akibatnya modul HR punya banyak source code
tetapi tidak punya rantai telusur: tidak ada decision log, tidak ada capability map, tidak ada
kontrak API terkunci, tidak ada roadmap task, dan tidak ada laporan task tracked.

Dokumen ini adalah artefak **pertama** dari rantai tersebut. Isinya bukan desain dan bukan
rencana kerja. Isinya adalah pemisahan tegas antara:

- **Fact** — hal yang sudah dapat dibuktikan dari source code hari ini;
- **Conflict** — hal yang saling bertentangan antara dokumen dan source code;
- **Assumption** — hal yang saya simpulkan sendiri dan masih boleh dibantah;
- **Open Question** — hal yang hanya boleh dijawab manusia yang berwenang;
- **Decision** — jawaban yang sudah dikunci pemilik berwenang.

Pemisahan wewenang yang dipakai dokumen ini:

- Keputusan **proses rekayasa** — bagaimana blueprint disusun, konvensi route, otoritas skema,
  batas kepemilikan antar modul — sah diputuskan pengguna sebagai pemilik pekerjaan, dan
  ditandai `approved`.
- Keputusan **kebijakan bisnis dan klinis** — nilai cuti, tarif lembur, aturan payroll, syarat
  kredensial, gerbang keselamatan pasien — hanya sah bila diputuskan pemilik berwenang seperti
  pemilik produk, Komite Medik, Komite Keperawatan, atau K3RS. Jawaban informal atas hal-hal
  itu dicatat sebagai **posisi sementara** berstatus `draft`, bukan approval.

---

## 1. Scope dan Outcome

### 1.1 Kalimat batas scope

> Modul `human-resource` mengelola seluruh siklus hidup tenaga kerja rumah sakit — dari
> perencanaan kebutuhan tenaga, rekrutmen, pengangkatan, penempatan, penjadwalan, kehadiran,
> cuti dan lembur, penggajian, kompetensi, kinerja, kredensial dan kewenangan klinis,
> kesehatan kerja staf, sampai pemberhentian — beserta layanan mandiri pegawai dan atasan
> atas transaksi-transaksi tersebut.

### 1.2 Di dalam scope

| Ref | Kemampuan | Bukti source hari ini |
|---|---|---|
| `IN-01` | Master data HR (organisasi, jabatan, shift, payroll, kredensial, pelatihan, kinerja, cuti, lembur, workflow) | `Areas/Corporate/HumanResource/MasterData/**` — 65 controller |
| `IN-02` | Profil dan administrasi kepegawaian (perubahan data, penempatan organisasi/jabatan/atasan, riwayat, penetapan gaji) | `Areas/Corporate/HumanResource/WorkforceCore/**` — 14 controller |
| `IN-03` | Perencanaan tenaga kerja dan staffing | `Areas/Corporate/HumanResource/WorkforcePlanning/**` — 11 model, **0 controller** |
| `IN-04` | Rekrutmen dan hiring | `Areas/Corporate/HumanResource/RecruitmentManagement/**` — 20 model, **0 controller** |
| `IN-05` | Onboarding, orientasi, probation, lifecycle, offboarding | `Areas/Corporate/HumanResource/LifecycleManagement/**` — 21 model, 1 controller |
| `IN-06` | Penjadwalan, shift, on-call, tukar shift | `Areas/Corporate/HumanResource/SchedulingManagement/**` — 11 model, 3 controller |
| `IN-07` | Kehadiran dan koreksi kehadiran | `Areas/Corporate/HumanResource/AttendanceManagement/**` — 14 model, 9 controller |
| `IN-08` | Cuti, izin, dan saldo cuti | `Areas/Corporate/HumanResource/LeaveManagement/**` — 17 model, 12 controller |
| `IN-09` | Lembur dan kompensasi lembur | `Areas/Corporate/HumanResource/OvertimeManagement/**` — 11 model, 9 controller |
| `IN-10` | Payroll dan benefit | `PayrollManagement` 19 model / 6 controller; `BenefitManagement` 9 model / **0 controller** |
| `IN-11` | Kredensial, lisensi, sertifikasi, kewenangan klinis | `Areas/Corporate/HumanResource/CredentialingManagement/**` — 18 model, 5 controller |
| `IN-12` | Kompetensi dan pelatihan | `Areas/Corporate/HumanResource/LearningAndDevelopment/**` — 13 model, 2 controller |
| `IN-13` | Manajemen kinerja | `Areas/Corporate/HumanResource/PerformanceManagement/**` — 11 model, 2 controller |
| `IN-14` | Kesehatan dan keselamatan kerja staf | `Areas/Corporate/HumanResource/OccupationalHealthManagement/**` — 10 model, 1 controller |
| `IN-15` | Hubungan karyawan dan kedisiplinan | `Areas/Corporate/HumanResource/EmployeeRelationManagement/**` — 8 model, 1 controller |
| `IN-16` | Layanan HR dan tiket kepegawaian | `Areas/Corporate/HumanResource/HrServiceManagement/**` — 8 model, **0 controller** |
| `IN-17` | Perjalanan dinas dan reimbursement | `BusinessTravelManagement` 13 model / **0 controller**; `ExpenseManagement` 7 model / **0 controller** |
| `IN-18` | Workflow dan approval HR bersama | `Areas/Corporate/HumanResource/WorkflowManagement/**` — 9 model, 6 controller |
| `IN-19` | Layanan mandiri pegawai dan atasan | `Areas/SelfServices/HumanResource/Controllers/**` — 13 controller |

### 1.3 Di luar scope — untuk modul lain

| Ref | Kemampuan | Pemilik | Titik sentuh yang boleh dibahas di sini |
|---|---|---|---|
| `OUT-01` | Pembayaran, posting akuntansi, dan penyelesaian kas payroll | Finance / Billing | Bentuk data serah terima payroll ke Finance, siapa yang menutup periode |
| `OUT-02` | Data klinis pasien, tindakan, volume, dan mutu layanan | Health Services (Rawat Jalan, Rawat Inap, IGD) | Sumber angka untuk OPPE/FPPE, dan pengecekan kewenangan klinis saat pelayanan |
| `OUT-03` | Akun aplikasi, role, permission, dan pencabutan akses | Administrator / Identity | Perintah buat akun saat onboarding dan cabut akses saat offboarding |
| `OUT-04` | Penyimpanan berkas dan dokumen terlampir | Shared platform | Cara HR menyimpan ijazah, STR, SIP, sertifikat, dan hasil MCU |
| `OUT-05` | Jadwal praktik dokter untuk pendaftaran pasien | Health Services | **Sudah diputuskan `HRD-DEC-006`:** dua hal berbeda. Yang masih dibahas hanya perlakuan jam praktik yang berada di luar jadwal kerja (`HRD-Q-09`) |
| `OUT-06` | Aturan pengendalian infeksi dan tindak lanjut pajanan | PPI / K3RS | Siapa pemilik proses saat terjadi tertusuk jarum |

Daftar **Di dalam scope** sudah dikonfirmasi lewat `HRD-DEC-003`: seluruh 21 capability
dirancang dalam satu blueprint utuh. Daftar **Di luar scope** masih usulan, kecuali `OUT-05`
yang sudah dikunci `HRD-DEC-006`. Titik sentuh dengan Finance, Administrator/Identity, dan
PPI/K3RS belum dikonfirmasi pemilik masing-masing modul.

---

## 2. Glossary

Istilah berikut dipakai konsisten di seluruh artefak blueprint HR. Definisi ini diambil dari
source code, bukan dikarang.

| Istilah | Arti yang dipakai di sini | Contoh |
|---|---|---|
| Workforce | Orang yang dikelola HR, mencakup pegawai internal, dokter, dan pengguna eksternal | Satu perawat ruang Melati adalah satu workforce |
| Employee | Workforce yang berstatus pegawai rumah sakit | Perawat tetap, staf administrasi |
| External User | Workforce yang bekerja untuk rumah sakit tetapi bukan pegawai | Dokter tamu, vendor, mahasiswa praktik |
| Assignment | Penempatan seorang workforce pada organisasi, jabatan, atau atasan, yang berlaku mulai tanggal tertentu | Perawat A ditempatkan di Unit Melati mulai 1 September |
| Effective date | Tanggal mulai berlakunya sebuah perubahan, terpisah dari tanggal pencatatan | Kenaikan gaji dicatat 27 Agustus, berlaku 1 September |
| Raw attendance log | Rekaman mentah dari mesin absensi atau aplikasi, tidak boleh diubah hasil olahan | Tap sidik jari jam 07:58 |
| Daily attendance | Hasil olahan satu hari kerja setelah jadwal, cuti, dan lembur diperhitungkan | Hadir, terlambat 12 menit |
| Credential | Bukti kelayakan tenaga kesehatan: ijazah, STR, SIP, sertifikat | STR perawat berlaku sampai 2028 |
| Clinical privilege | Kewenangan klinis yang secara resmi diberikan rumah sakit kepada tenaga medis | Dokter B berwenang melakukan tindakan tertentu |
| SPK/RKK | Surat Penugasan Klinis dan Rincian Kewenangan Klinis | Dokumen resmi hasil kredensial |
| OPPE | Evaluasi praktik profesional berkelanjutan | Penilaian rutin kinerja praktik dokter |
| FPPE | Evaluasi praktik profesional terfokus | Penilaian khusus saat ada temuan |
| Self service | Transaksi yang diajukan sendiri oleh pegawai dari akunnya | Pegawai mengajukan cuti |

---

## 3. Aktor dan Tanggung Jawab

Daftar aktor diambil dari PRD dan **belum** diverifikasi terhadap role/permission yang benar
ada di sistem. Verifikasi itu pekerjaan `/qv-trace`, bukan wawancara ini.

| Aktor | Tanggung jawab utama di modul ini | Status verifikasi |
|---|---|---|
| HR Admin | Master data, profil pegawai, administrasi kepegawaian | `UNKNOWN` |
| HR Manager | Approval, kebijakan, exception, audit | `UNKNOWN` |
| Recruiter | Requisition sampai hiring | `UNKNOWN` |
| Kepala Unit / Manager | Kebutuhan tenaga, jadwal, approval tim | `UNKNOWN` |
| Pegawai | Layanan mandiri | `UNKNOWN` |
| Payroll Officer | Periode payroll, rekonsiliasi, finalisasi | `UNKNOWN` |
| Komite Medik / Subkomite Kredensial | Kredensial dan kewenangan klinis dokter | `UNKNOWN` |
| Komite Keperawatan | Kredensial dan kewenangan klinis perawat | `UNKNOWN` |
| K3RS / Occupational Health | Kesehatan dan keselamatan kerja staf | `UNKNOWN` |
| Auditor / Akreditasi | Baca evidence, tidak mengubah data | `UNKNOWN` |

---

## 4. Fakta Source Code

Semua baris di bawah ini dapat diperiksa ulang pada SHA yang tercatat di header. Ini **fakta**,
bukan keputusan.

### 4.1 Backend — sebaran controller terhadap model

| Domain | Model | Controller | Pembacaan |
|---|---:|---:|---|
| `MasterData` | 87 | 65 | Matang |
| `WorkforceCore` | 21 | 14 | Matang |
| `LeaveManagement` | 17 | 12 | Matang |
| `AttendanceManagement` | 14 | 9 | Matang |
| `OvertimeManagement` | 11 | 9 | Matang |
| `PayrollManagement` | 19 | 6 | Sebagian |
| `WorkflowManagement` | 9 | 6 | Sebagian |
| `CredentialingManagement` | 18 | 5 | Sebagian |
| `SchedulingManagement` | 11 | 3 | Tipis |
| `PerformanceManagement` | 11 | 2 | Tipis |
| `LearningAndDevelopment` | 13 | 2 | Tipis |
| `LifecycleManagement` | 21 | 1 | Sangat tipis |
| `OccupationalHealthManagement` | 10 | 1 | Sangat tipis |
| `EmployeeRelationManagement` | 8 | 1 | Sangat tipis |
| `RecruitmentManagement` | 20 | **0** | Skema tanpa API |
| `BusinessTravelManagement` | 13 | **0** | Skema tanpa API |
| `WorkforcePlanning` | 11 | **0** | Skema tanpa API |
| `BenefitManagement` | 9 | **0** | Skema tanpa API |
| `HrServiceManagement` | 8 | **0** | Skema tanpa API |
| `ExpenseManagement` | 7 | **0** | Skema tanpa API |
| `WorkforceProfileManagement` | 0 | 1 | Hanya controller |
| `SelfServices/HumanResource` | — | 13 | Matang |

`FACT-01` — Total **68 model** tersebar di enam domain yang tidak punya satu pun controller.
Menurut gate nomor 7 pada PRD, kondisi ini secara langsung membuat target 100% tidak tercapai.

### 4.2 Backend — endpoint layanan mandiri yang sudah ada

Tag Swagger dan tabel API resmi belum ditulis, tetapi route-nya sudah dapat dibuktikan.

| Controller | Base route |
|---|---|
| `HumanResourceContextController` | `api/v1/self-services/human-resource/context` |
| `AttendanceSelfServiceController` | `api/v1/self-services/human-resource/attendance` |
| `AttendanceCorrectionSelfServiceController` | `api/v1/self-services/human-resource/attendance-corrections` |
| `LeaveRequestController` | `api/v1/self-services/human-resource/leave/requests` |
| `LeaveBalanceSelfServiceController` | `api/v1/self-services/human-resource/leave/balances` |
| `LeaveCalendarSelfServiceController` | `api/v1/self-services/human-resource/leave/calendar` |
| `LeaveCancellationSelfServiceController` | `api/v1/self-services/human-resource/leave/cancellations` |
| `LeaveReturnToWorkController` | `api/v1/self-services/human-resource/leave/return-to-work` |
| `OvertimeSelfServiceController` | `api/v1/self-services/human-resource/overtime` |
| `ScheduleChangeSelfServiceController` | `api/v1/self-services/human-resource/schedule-change-requests` |
| `ShiftSwapSelfServiceController` | `api/v1/self-services/human-resource/shift-swap-requests` |
| `EmployeeProfileChangeSelfServiceController` | `api/v1/self-services/human-resource/profile-changes` |
| `ResignationSelfServiceController` | `api/v1/self-services/human-resource/resignation-requests` |

### 4.3 Frontend — route yang benar-benar ada

`FACT-02` — `src/app/hr/` **hanya** berisi `master-data/`, dengan 64 kelompok entity berpola
lengkap `list` → `create` → `[slug]` → `[slug]/update`.

`FACT-03` — Di luar master data, route HR yang ada hanya dua:

- `src/app/self-services/human-resource/employee/dashboard/page.jsx`
- `src/app/karyawan/Absensi-Karyawan/FormAbsensi/page.jsx`

`FACT-04` — Tidak ada route sama sekali untuk sebelas dari tiga belas controller layanan
mandiri: cuti, saldo cuti, kalender cuti, pembatalan cuti, kembali kerja, lembur, koreksi
kehadiran, perubahan data, tukar shift, ubah jadwal, dan resign.

`FACT-05` — Menu `Administrasi Kepegawaian` pada
[`src/utils/menu-sidebar/menu-items.jsx`](../../../../../../d:/Projects/QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx)
baris 517–557 menunjuk ke enam alamat yang **tidak punya halaman**:

| Label menu | `pathname` | Halaman ada? |
|---|---|---|
| Perubahan Data Karyawan | `/hr/workforce-core/employee-profile-changes` | **Tidak** |
| Penempatan Organisasi | `/hr/workforce-core/organization-assignments` | **Tidak** |
| Penempatan Jabatan | `/hr/workforce-core/position-assignments` | **Tidak** |
| Relasi Atasan | `/hr/workforce-core/manager-assignments` | **Tidak** |
| Riwayat Kepegawaian | `/hr/workforce-core/employment-histories` | **Tidak** |
| Penetapan Gaji | `/hr/workforce-core/salary-assignments` | **Tidak** |

Enam menu ini akan membawa pengguna ke halaman kosong atau error. Ini melanggar gate nomor 6
pada PRD, dan sudah bisa dinyatakan sebagai cacat produksi hari ini.

`FACT-06` — Tidak ada satu pun route untuk atasan atau manajer. Folder `src/app/manajer`
tidak ada, dan tidak ada kotak masuk approval di mana pun.

### 4.4 Artefak tata kelola

`FACT-07` — `docs/module-blueprints/human-resource/` sebelumnya **belum ada**. Modul HR adalah
satu-satunya modul besar tanpa blueprint, sementara `rawat-inap`, `igd`, `billing-kasir`,
`insurance-management`, `operations`, `pharmacy`, dan `rawat-jalan` sudah punya.

`FACT-08` — Registry sudah mengenali modul ini:
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 9 mencatat
`Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE / LEGACY`. Jadi
prefix `Hrd` sudah sah dan `QBE-MOD-002` tidak memblokir modul ini.

---

## 5. Conflict — dokumen versus source code

Bagian ini adalah koreksi terhadap `PRD_to_MVP_HRD_Quilvian_Target_100.md`. Setiap butir
menyebut lokasi yang perlu diperbaiki.

| ID | Isi PRD | Kenyataan pada SHA hari ini | Usulan koreksi | Status |
|---|---|---|---|---|
| `HRD-CONF-01` | §6.1 menyebut navigasi `Layanan Kepegawaian` berisi `Akun`, `Karyawan`, `Manajer` sebagai *existing yang harus dipertahankan verbatim* | Ketiga label itu **tidak ada** di `menu-items.jsx`. Yang ada hanya `Sumber Daya Manusia` dengan dua anak: `Master Data` dan `Administrasi Kepegawaian` | Turunkan dari "existing" menjadi "menu usulan", atau tunjukkan branch tempat menu itu benar-benar ada | `draft` |
| `HRD-CONF-02` | §14 menyebut Recruitment dan Workforce Planning "controller operasional belum terbukti" | Bukan "belum terbukti" — hasil hitung menunjukkan **nol controller**. Sama untuk Benefit, HrService, BusinessTravel, dan Expense yang tidak disebut PRD | Ganti label menjadi `MISSING` yang terverifikasi, dan tambahkan empat domain yang terlewat | `draft` |
| `HRD-CONF-03` | §1 menyebut *existing functional coverage sekitar 83%* dan *operational readiness 65–70%* | Angka ini tidak dapat direproduksi dari bukti apa pun. Dari sisi frontend, dari 21 capability hanya master data yang benar-benar operasional; sisi operasional HR praktis 0% | Hapus angka persentase, atau ganti dengan hitungan yang punya rumus dan bukti | `draft` |
| `HRD-CONF-04` | §29 menyatakan evidence backend `branch master` dan frontend `branch QuilvianDevV2` | Pekerjaan nyata ada di backend `AndryZain` (`ecdc135`) dan frontend `AgentCodexFrontend` (`2a1cea784`) | Perbarui evidence ke branch dan SHA yang benar-benar dibaca | `draft` |
| `HRD-CONF-05` | §6.1 menyebut gap frontend, tetapi tidak menyebut enam menu yang menunjuk ke halaman kosong | Enam menu `Administrasi Kepegawaian` semuanya menunjuk ke route yang tidak ada | Naikkan menjadi temuan cacat produksi, bukan sekadar "gap" | `draft` |
| `HRD-CONF-06` | PRD tidak membahas konvensi route | Halaman absensi baru dipasang di `src/app/karyawan/Absensi-Karyawan/FormAbsensi`, memakai Bahasa Indonesia dan PascalCase, sedangkan konvensi modul ini adalah `src/app/self-services/human-resource/**` dengan kebab-case | Perlu keputusan: pindahkan ke konvensi baku, atau resmikan pengecualian | `draft` |
| `HRD-CONF-07` | §20 memuat 21 "Task HRD-xx" | Ukurannya bukan task. Satu baris seperti "Operasionalisasi Recruitment dan Hiring" mencakup 20 model dan belasan endpoint. Ini epic, bukan task yang bisa dikerjakan satu orang dalam satu slice | Turunkan namanya menjadi epic; task sesungguhnya dibuat `/qv-plan` | `draft` |

---

## 6. Assumption

Butir berikut saya simpulkan sendiri. Boleh dibantah kapan pun.

| ID | Asumsi | Kalau salah, dampaknya |
|---|---|---|
| `HRD-ASM-01` | ~~Modul HR terlalu besar untuk satu blueprint tunggal dan perlu dipecah~~ — **ditolak** oleh `HRD-DEC-003`; pengguna memilih satu blueprint utuh | Risiko yang tersisa dan harus dipantau: dokumen desain berukuran besar, dan persetujuannya menunggu pemilik produk yang sampai sekarang belum ditunjuk (`HRD-Q-01`) |
| `HRD-ASM-02` | Backend tetap menjadi sumber kebenaran kontrak; frontend menyesuaikan | Kalau terbalik, banyak endpoint existing harus diubah |
| `HRD-ASM-03` | Enam domain tanpa controller memang belum pernah dipakai produksi, sehingga masih bebas dirancang ulang. Asumsi ini **dipakai sebagai dasar** `HRD-DEC-004` | Kalau ternyata tabelnya sudah berisi data yang masuk lewat jalur lain — impor manual, skrip, atau migrasi V1 — maka penurunan ERD ulang berubah menjadi pekerjaan migration, bukan perancangan bebas. Lihat `HRD-Q-05` |
| `HRD-ASM-04` | Prioritas terdekat adalah menutup jalur yang backend-nya sudah matang tetapi frontend-nya belum ada, karena itu nilai tercepat dengan risiko terendah | Kalau prioritas rumah sakit ternyata rekrutmen, urutan kerja berubah total |

---

## 7. Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `HRD-FE-01` | Penempatan route halaman operasional HR | `OPEN` | `draft` | `/hr/<domain>/<capability>` mengikuti pola `src/app/hr/master-data/**` | `FACT-02`, `FACT-05` |
| `HRD-FE-02` | Penempatan route layanan mandiri | Pengguna | `approved` | **Wajib** `src/app/self-services/human-resource/**` dengan kebab-case, mengikuti `employee/dashboard`. `src/app/karyawan/**` tidak dipakai lagi untuk halaman baru | `HRD-DEC-007` |
| `HRD-FE-03` | Bentuk tampilan daftar, form, dan modal | `DEV_DISCRETION` | `draft` | Wajib mengikuti halaman master data terdekat; tidak boleh membuat design system baru | `AGENTS.md` bagian UI and UX Rules |
| `HRD-FE-04` | Label menu berbahasa Indonesia | `OPEN` | `draft` | Label baru harus disetujui pemilik produk; label existing tidak diubah | `menu-items.jsx` |
| `HRD-FE-05` | Kotak masuk approval atasan: menu tersendiri atau menyatu per transaksi | `OPEN` | `draft` | — | `FACT-06` |

---

## 8. Decision Log

Belum ada satu pun `Decision` berstatus `approved`. Yang tercatat baru fakta, konflik, asumsi,
dan pertanyaan.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-001` | Decision | Blueprint modul `human-resource` dibuka dan seluruh artefak berikutnya ditulis di `docs/module-blueprints/human-resource/` | Pengguna | `draft` | — | Permintaan pengguna 2026-08-27 |
| `HRD-DEC-002` | Decision | `PRD_to_MVP_HRD_Quilvian_Target_100.md` diperlakukan sebagai **masukan produk**, bukan PRD blueprint. PRD resmi lahir sebagai `04-prd-to-mvp.md` setelah desain | Pengguna | `draft` | — | Struktur `rawat-inap`, `igd` |
| `HRD-DEC-004` | Decision | **Otoritas skema bersifat hybrid.** Domain yang sudah punya controller diperlakukan sebagai kontrak existing: skemanya dikunci, perubahan hanya lewat `EXTEND` atau `REPAIR` yang berbukti. Enam domain tanpa controller (`RecruitmentManagement`, `BusinessTravelManagement`, `WorkforcePlanning`, `BenefitManagement`, `HrServiceManagement`, `ExpenseManagement`, total 68 model) diturunkan ulang ERD-nya dari proses bisnis rumah sakit; 68 model existing berstatus **kandidat**, bukan jawaban | Pengguna | `approved` | Pengguna, 2026-08-27 | Jawaban wawancara 2026-08-27; bukti hitung pada bagian 4.1 |
| `HRD-Q-01` | Open Question | Siapa pemilik produk/domain HR yang berwenang menyetujui keputusan modul ini? | — | `draft` | — | Header dokumen |
| `HRD-DEC-003` | Decision | Modul dirancang sebagai **satu blueprint utuh** untuk seluruh 21 capability, bukan dipecah menjadi beberapa sub-blueprint atau gelombang terpisah. Batas rilis pertama tetap ditulis di dalam `04-prd-to-mvp.md`, sehingga desain yang menyeluruh tidak memaksa pengerjaan serentak | Pengguna | `approved` | Pengguna, 2026-08-27 | Jawaban wawancara 2026-08-27 |
| `HRD-Q-02` | Open Question | ~~Bagaimana modul sebesar ini dipecah agar bisa didesain dan disetujui?~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-003` | `HRD-ASM-01` |
| `HRD-Q-03` | Open Question | Enam menu `Administrasi Kepegawaian` yang menunjuk halaman kosong: diperbaiki lebih dulu, disembunyikan, atau dibiarkan? | — | `draft` | — | `FACT-05` |
| `HRD-Q-04` | Open Question | Halaman absensi di `src/app/karyawan/Absensi-Karyawan/FormAbsensi` dipindahkan ke konvensi baku atau diresmikan sebagai pengecualian? | — | `draft` | — | `HRD-CONF-06` |
| `HRD-Q-05` | Open Question | Enam domain tanpa controller: apakah sudah ada data produksi di tabelnya? | — | `draft` | — | `FACT-01`, `HRD-ASM-03` |
| `HRD-Q-06` | Open Question | Dua puluh nilai kebijakan pada PRD §28 belum punya pemilik keputusan | — | `draft` | — | PRD §28 |
| `HRD-DEC-006` | Decision | **Jadwal kerja dan jadwal praktik adalah dua hal berbeda.** Jadwal kerja HR dipakai untuk kehadiran, lembur, dan tunjangan shift. Jadwal praktik dokter tetap milik Health Services dan dipakai untuk pendaftaran pasien. HR **bukan** sumber kebenaran jadwal praktik dan **bukan** jalur kritis pendaftaran pasien | Pengguna | `approved` | Pengguna, 2026-08-27 | Jawaban wawancara 2026-08-27 |
| `HRD-Q-09` | Open Question | Turunan `HRD-DEC-006`: apa yang berlaku bila seorang dokter praktik pada jam yang tidak ada dalam jadwal kerjanya? Apakah jam itu tetap dihitung kehadiran, dianggap lembur, atau diabaikan? | — | `draft` | — | `HRD-DEC-006` |
| `HRD-DEC-007` | Decision | **Konvensi route layanan mandiri adalah `src/app/self-services/human-resource/**` dengan kebab-case.** Halaman absensi dipindahkan dari `src/app/karyawan/Absensi-Karyawan/FormAbsensi/` ke `src/app/self-services/human-resource/employee/attendance/`, sejajar dengan `employee/dashboard` yang sudah ada. Sepuluh halaman layanan mandiri berikutnya mengikuti konvensi yang sama | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-CONF-06`, `FACT-03` |
| `HRD-Q-07` | Open Question | ~~Jadwal kerja versus jadwal praktik~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-006` | `OUT-05` |
| `HRD-Q-04` | Open Question | ~~Konvensi route absensi~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-007` | `HRD-CONF-06` |
| `HRD-DEC-005` | Decision | **Posisi sementara yang fail-safe.** Kredensial dan kewenangan klinis kedaluwarsa **tidak** menghentikan pelayanan. HR menyediakan API pengecekan dan daftar pantau kedaluwarsa; modul klinis menampilkan peringatan dan mencatat siapa yang tetap melanjutkan beserta alasannya. Keputusan blokir keras ditahan sampai **Komite Medik** memutuskan per skenario klinis. Dasarnya: prinsip 9 PRD menyatakan sistem tidak boleh menciptakan hambatan yang membahayakan pasien | Komite Medik | `draft` | **Belum** — pilihan pengguna 2026-08-27 dicatat sebagai posisi sementara, bukan approval. Rilis produksi slice kredensial menunggu Komite Medik | PRD §9 prinsip 9, PRD §28 butir 18 |
| `HRD-Q-08` | Open Question | ~~Blokir keras versus peringatan~~ — posisi sementara diambil oleh `HRD-DEC-005`; yang masih terbuka adalah **pengesahan Komite Medik** dan daftar skenario klinis mana yang boleh diblokir keras | Komite Medik | `draft` | — | `HRD-DEC-005` |

---

## 9. Acceptance Criteria yang sudah dapat diuji hari ini

Hanya dua butir yang sudah cukup tegas untuk diuji tanpa menunggu keputusan siapa pun. Sisanya
menunggu jawaban pada bagian 8.

| ID | Kriteria | Cara menguji |
|---|---|---|
| `HRD-AC-01` | Tidak ada menu pada sidebar HR yang menunjuk ke alamat tanpa halaman | Untuk setiap `pathname` di bawah `corporateHumanResource`, harus ada `page.jsx` yang cocok di `src/app` |
| `HRD-AC-02` | Setiap controller layanan mandiri HR punya pemakai di frontend, atau dinyatakan resmi sebagai integration-only | Petakan 13 base route pada bagian 4.2 ke service frontend; setiap route tanpa pemakai harus punya alasan tertulis |
| `HRD-AC-03` | Seluruh halaman layanan mandiri HR berada di bawah `src/app/self-services/human-resource/` dengan kebab-case, sesuai `HRD-DEC-007` | Tidak boleh ada `page.jsx` layanan mandiri HR di luar folder itu; `src/app/karyawan/**` kosong atau tinggal pengalihan |
| `HRD-AC-04` | HR tidak menjadi jalur kritis pendaftaran pasien, sesuai `HRD-DEC-006` | Pendaftaran pasien tetap berjalan normal walau seluruh endpoint HR tidak dapat dihubungi |
| `HRD-AC-05` | Kredensial kedaluwarsa memberi peringatan yang tercatat, bukan penolakan, sesuai posisi sementara `HRD-DEC-005` | Tenaga medis dengan STR kedaluwarsa tetap dapat melanjutkan tindakan; sistem menyimpan identitas pelaku, waktu, dan alasan lanjut |

---

## 10. Open Questions dan Blocker

| ID | Pertanyaan | Memblokir | Pemilik |
|---|---|---|---|
| `HRD-Q-01` | Pemilik produk/domain HR | `DESIGN` — tanpa ini tidak ada yang berwenang menyetujui apa pun | Manajemen |
| `HRD-Q-05` | Ada tidaknya data produksi | `IMPLEMENTATION` — menentukan boleh tidaknya ubah skema | Backend owner |
| `HRD-Q-06` | Dua puluh nilai kebijakan | `LATER SLICE` untuk sebagian besar; `IMPLEMENTATION` untuk payroll, kredensial, dan cuti | Pemilik produk, Komite Medik, K3RS |
| `HRD-Q-09` | Dokter praktik di luar jadwal kerjanya dihitung apa | `DESIGN` untuk slice kehadiran dan lembur tenaga medis | Pemilik produk + Health Services |
| `HRD-Q-08` | Pengesahan Komite Medik atas `HRD-DEC-005`, dan daftar skenario klinis mana yang boleh diblokir keras | Tidak lagi memblokir `DESIGN` — posisi sementara sudah ada. Memblokir **rilis produksi** slice kredensial. **Ini pertanyaan keselamatan pasien**, tidak boleh diputuskan developer | Komite Medik |
| `HRD-Q-03` | Menu menunjuk halaman kosong | `IMPLEMENTATION` — cacat yang sudah nyata hari ini | Pemilik produk |

---

## 11. Yang sengaja tidak dikerjakan pada pass ini

- Tidak ada capability map resmi. Angka pada bagian 4 adalah hitungan cepat, bukan audit
  `READY TO REUSE` / `EXTEND` / `MISSING` per kemampuan. Itu tugas `/qv-trace`.
- Tidak ada arsitektur, ERD, kontrak API, roadmap, maupun migration di dokumen ini. Sesuai
  aturan `grill-me`, semua itu dilarang dibuat di sini.
- Tidak ada perubahan pada source code backend maupun frontend.
- Aturan internal modul tetangga tidak digali; hanya titik sentuhnya yang dicatat pada
  bagian 1.3.

---

## 12. Ringkasan pass ini

### 12.1 Keputusan yang lahir

| ID | Isi singkat | Status |
|---|---|---|
| `HRD-DEC-001` | Blueprint `human-resource` dibuka | `draft` |
| `HRD-DEC-002` | PRD lama diperlakukan sebagai masukan produk, bukan PRD blueprint | `draft` |
| `HRD-DEC-003` | Satu blueprint utuh untuk 21 capability; batas rilis ditulis di `04-prd-to-mvp.md` | `approved` |
| `HRD-DEC-004` | Otoritas skema hybrid: yang berjalan dikunci, 68 model tanpa controller diturunkan ulang | `approved` |
| `HRD-DEC-005` | Kredensial kedaluwarsa memberi peringatan, tidak memblokir pelayanan | `draft` — menunggu Komite Medik |
| `HRD-DEC-006` | Jadwal kerja HR bukan sumber jadwal praktik dokter | `approved` |
| `HRD-DEC-007` | Route layanan mandiri wajib `src/app/self-services/human-resource/**` kebab-case | `approved` |

### 12.2 Blocker yang tersisa

1. `HRD-Q-01` pemilik produk/domain belum ditunjuk. Ini blocker paling berat: tanpa pemilik,
   seluruh keputusan kebijakan bisnis tetap `draft` selamanya dan blueprint tidak bisa naik
   status `approved`.
2. `HRD-Q-05` belum diketahui apakah tabel milik enam domain tanpa controller sudah berisi
   data dari jalur lain. Ini menentukan apakah `HRD-DEC-004` menjadi perancangan bebas atau
   pekerjaan migration.
3. `HRD-Q-06` dua puluh nilai kebijakan pada PRD §28 belum punya pemilik keputusan.
4. `HRD-Q-08` pengesahan Komite Medik atas posisi sementara keselamatan pasien.
5. `HRD-Q-09` perlakuan jam praktik dokter di luar jadwal kerjanya.
6. `HRD-Q-03` enam menu yang menunjuk halaman kosong belum diputuskan penanganannya.

### 12.3 Langkah berikutnya

Scope pass sudah selesai: tujuan, batas, dan pertanyaan yang perlu diaudit sudah jelas.
Langkah yang tepat berikutnya adalah **`/trace-existing-capabilities`**, dengan alasan:

- `HRD-DEC-004` menuntut pemisahan tegas antara domain yang berjalan dan domain yang belum
  pernah dipakai. Pemisahan itu hanya sah bila berbasis audit, bukan hitungan folder;
- angka pada bagian 4 masih hitungan cepat dan belum berlabel `READY TO REUSE`,
  `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`;
- `HRD-CONF-03` menuntut angka cakupan yang punya rumus dan bukti, dan itu keluaran audit;
- `HRD-Q-05` sebagian dapat dijawab audit, misalnya dengan memeriksa `ApplicationDbContext`,
  konfigurasi EF, dan migration milik enam domain tersebut.

Setelah capability map ada, urutannya menjadi `/design-business-module` untuk arsitektur, ERD,
kontrak, dan `04-prd-to-mvp.md`, lalu `/plan-module-delivery` untuk memecah menjadi task.

---

## 13. Koreksi dari capability map 27 Agustus 2026 — HISTORICAL SNAPSHOT

> **HISTORICAL SNAPSHOT.** Isi bagian ini adalah keadaan pada saat pass tersebut dijalankan,
> bukan wewenang terbaru. Keputusan yang sudah digantikan ditandai `superseded` beserta
> penggantinya. Wewenang terbaru selalu ada pada bagian bernomor tertinggi.

`/trace-existing-capabilities` dijalankan setelah pass wawancara ini dan menghasilkan
[`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `1.0`. Audit itu
mengoreksi tiga hal pada dokumen ini. Baris aslinya **tidak dihapus**; koreksinya dicatat di
sini supaya jejaknya utuh.

| Yang dikoreksi | Pembacaan awal | Hasil audit |
|---|---|---|
| `FACT-05` enam menu `Administrasi Kepegawaian` | Dibaca sebagai kemampuan yang hilang | Kemampuannya **sudah ada dan sudah dipakai** lewat editor profil di halaman detail pegawai, memanggil 14 controller `WorkforceCore`. Yang hilang hanya halaman berdiri sendirinya. Statusnya `REPAIR`, bukan `MISSING` — pekerjaannya jauh lebih kecil dari dugaan. Lihat capability map bagian 7.2 |
| `HRD-ASM-03` enam domain bebas dirancang ulang | Diasumsikan tabelnya belum ada | Tabelnya **sudah dibuat** oleh migration `20260726161839_initializeBigModulHRD2`, lengkap dengan konfigurasi EF dan `DbSet`. Perancangan ulang berarti mengubah atau membuang tabel yang sudah ada, bukan membuat di ruang kosong |
| `HRD-DEC-004` jumlah entity yang boleh dirancang ulang | 68 model | **67**. `MstWorkforceRequirement` sudah dilayani `WorkforceRequirementController` dan sudah dipakai frontend, sehingga tidak ikut dibebaskan |

Audit juga menemukan empat hal yang tidak terlihat pada pass ini dan menjadi pertanyaan baru
`HRD-TQ-01` sampai `HRD-TQ-10` pada capability map bagian 12. Yang paling mendesak:

- **`HRD-TF-001`** — tiga controller master data pelatihan tidak memakai `[Authorize]`, dan
  tidak ada `FallbackPolicy` maupun `RequireAuthorization` yang menutupinya. Dua puluh tujuh
  endpoint berpotensi terbuka tanpa autentikasi;
- **`HRD-TF-002`** — 40 entity memakai prefix `Wfp` yang tidak terdaftar di
  `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, bertentangan dengan `QBE-MOD-002`;
- **`HRD-TF-007`** — tidak ada satu pun test untuk HR di kedua repository.

`HRD-Q-05` tetap **terbuka**. Audit membuktikan aplikasi tidak dapat menulis ke enam domain
itu, tetapi tidak dapat membuktikan tidak ada data yang masuk lewat impor manual atau migrasi
V1. Itu memerlukan pemeriksaan database yang berada di luar wewenang audit source.

---

## 14. Closure Pass — 27 Agustus 2026 — HISTORICAL SNAPSHOT

> **HISTORICAL SNAPSHOT.** Isi bagian ini adalah keadaan pada saat pass tersebut dijalankan,
> bukan wewenang terbaru. Keputusan yang sudah digantikan ditandai `superseded` beserta
> penggantinya. Wewenang terbaru selalu ada pada bagian bernomor tertinggi.

Pass ini dijalankan setelah [`01-existing-capability-map.md`](./01-existing-capability-map.md)
revision `1.0` tersedia. SHA backend `ecdc135` dan frontend `2a1cea784` diperiksa ulang dan
masih sama dengan yang diaudit, sehingga peta **tidak basi**. Batas scope tidak berubah dan
tetap terkunci oleh `HRD-DEC-003`.

### 14.1 Keputusan yang lahir pada pass ini

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-008` | Decision | **Seluruh entity HR dinormalkan ke prefix `Hrd`.** Ratchet yang sudah dimulai di `AttendanceManagement` diteruskan sampai 322 entity `Trx*`, `Wfp*`, dan `Mst*` operasional ikut berganti nama. Pekerjaan ini dijalankan sebagai `LEGACY MIGRATION`, yaitu kampanye bertahap per domain dengan wewenang terpisah setiap kali, **bukan** penulisan ulang massal dalam satu task | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-TF-002`, `HRD-TF-003`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md:7` |
| `HRD-DEC-009` | Decision | **Tanggung jawab HR atas payroll berhenti setelah `execute` serah terima.** HR bertanggung jawab sampai data terhitung, terekonsiliasi, dan diserahkan. Pembayaran, posting akuntansi, pajak, dan pelaporan adalah milik Finance | Pengguna | `approved` | Pengguna, 2026-08-27 | Capability map §6.4 |
| `HRD-DEC-010` | Decision | **Rekam kesehatan kerja hanya dapat dibaca K3RS dan pegawai yang bersangkutan.** HR Admin dan atasan hanya melihat kesimpulan kelayakan kerja — layak, layak dengan pembatasan, atau belum layak — tanpa diagnosis maupun hasil pemeriksaan | Pengguna, menunggu pengesahan K3RS | `draft` | **Belum** — posisi fail-closed, pengesahan K3RS diperlukan sebelum rilis | PRD §9 prinsip 7; `WfpHealthRecord` |
| `HRD-DEC-011` | Decision | **Persetujuan HR memakai satu kotak masuk terpadu lintas jenis transaksi.** Atasan membuka satu halaman berisi seluruh pengajuan yang menunggu persetujuannya. Detail tiap jenis tetap dibuka di halaman transaksinya sendiri | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-CAP-23`, `HRD-CAP-24` |

### 14.2 Konsekuensi `HRD-DEC-008` yang harus dijaga

Keputusan ini adalah yang paling mahal pada blueprint ini, dan jalannya sudah diatur kontrak.
Aturan yang mengikat setiap kampanye rename:

| Aturan | Isi |
|---|---|
| `QBE-NAM-003` | Nama di source dan nama tabel fisik dinormalkan **bersamaan**, tidak boleh salah satu saja |
| `QBE-DB-001` | Dependency fisik diaudit lebih dulu sebelum rename dijalankan |
| `QBE-DB-002` | Dilarang memakai `DROP` lalu `CREATE` bila rename yang mempertahankan data masih aman |

Yang perlu dijaga selama kampanye berjalan:

1. **Satu domain per kampanye.** Pola yang sudah terbukti adalah tiga migration kehadiran:
   `ChangeNameTrxAttendanceToHrdAttendance`, `NormalizeAttendanceCorrectionFamilyToHrd`, dan
   `RenameAttendancePersistenceToHrd`. Ulangi bentuk itu, jangan gabungkan banyak domain.
2. **Registry diperbarui lebih dulu.** Sebelum kampanye pertama berjalan, `Wfp` dan `Trx`
   harus tercatat di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` sebagai prefix legacy milik HR yang
   sedang dimigrasikan. Tanpa itu, kepemilikan 218 entity tetap tidak terbaca selama masa
   transisi yang panjang.
3. **Entity baru langsung `Hrd`.** Enam domain yang akan diturunkan ulang oleh `HRD-DEC-004`
   tidak boleh lagi memakai `Trx*` atau `Wfp*`.
4. **Risiko yang diterima.** Selama kampanye berjalan, modul HR akan memakai lebih dari satu
   gaya penamaan sekaligus. Ini konsekuensi yang sudah diketahui dan diterima, bukan cacat yang
   tidak disadari.

### 14.3 Konsekuensi `HRD-DEC-009` — yang masih harus dijawab

Batas sudah jelas, tetapi bentuk serah terimanya belum. Dua hal berikut menjadi pertanyaan
baru dan harus disepakati bersama Finance sebelum slice payroll dirancang:

- `HRD-Q-10` — bentuk data serah terima apa yang diterima Finance, dan apakah Finance menarik
  sendiri atau HR mengirim;
- `HRD-Q-11` — apa yang terjadi bila Finance menolak satu batch yang sudah `execute`. Apakah HR
  memakai `rollback` yang sudah ada, atau Finance memperbaiki di sisinya.

### 14.4 Konsekuensi `HRD-DEC-010` — bentuk yang harus dibangun

Keputusan ini menuntut pemeriksaan hak akses **per field**, bukan per halaman. Contoh nyata
supaya tidak salah bangun:

> Seorang atasan membuka profil anak buahnya untuk menyusun jadwal. Ia **boleh** melihat
> keterangan `Layak bekerja dengan pembatasan: tidak boleh shift malam sampai 30 September`.
> Ia **tidak boleh** melihat alasan medis di balik pembatasan itu.

Artinya satu endpoint yang mengembalikan seluruh isi `WfpHealthRecord` tidak cukup aman untuk
dipakai atasan. Perlu ada bentuk ringkas yang memang dirancang untuk konsumsi non-medis.

### 14.5 Konsekuensi `HRD-DEC-011` — batas yang perlu dikunci

Kotak masuk terpadu memakai ulang `WorkflowManagement` yang sudah ada. Yang masih terbuka:

- `HRD-Q-12` — apakah kotak masuk juga menampilkan pengajuan yang **sudah** diputuskan, atau
  hanya yang menunggu;
- `HRD-Q-13` — bagaimana perilaku saat atasan sedang cuti. Apakah delegasi otomatis memakai
  `TrxApprovalDelegation` yang sudah ada, dan siapa yang berwenang mengaktifkannya.

### 14.6 Keputusan ronde kedua

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-012` | Decision | **Enam menu `Administrasi Kepegawaian` dibuatkan halaman daftar lintas-pegawai.** Halaman menampilkan data seluruh pegawai pada satu periode, bukan data satu orang. Menu tidak dihapus dan tidak dialihkan | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-TF-005`, capability map §7.2 |
| `HRD-DEC-013` | Decision | **Jam praktik dokter di luar jadwal kerjanya dicatat sebagai pengecualian kehadiran yang menunggu keputusan atasan.** Atasan menentukan apakah jam itu menjadi lembur, penyesuaian jadwal, atau tidak dihitung. Tidak ada perhitungan otomatis | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-DEC-006`; mesin pengecualian pada `AttendanceManagement` |
| `HRD-DEC-014` | Decision | **Delapan route master data yang memakai kata gabung diseragamkan ke kebab-case sekarang**, bersamaan dengan konstanta frontend yang memanggilnya. Route baru wajib kebab-case | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-TF-006` |
| `HRD-DEC-015` | Decision | **Pengguna bertindak sebagai pemilik keputusan rekayasa dan produk teknis.** Keputusan yang menyentuh keselamatan pasien, privasi kesehatan, dan uang tetap `draft` sampai pemilik berwenangnya menetapkan | Pengguna | `approved` | Pengguna, 2026-08-27 | `HRD-Q-01` |
| `HRD-Q-01` | Open Question | ~~Siapa pemilik produk/domain HR~~ — sebagian tertutup oleh `HRD-DEC-015`. Yang masih terbuka: nama pemilik untuk kebijakan bisnis, Komite Medik, dan K3RS | Manajemen | `draft` | — | `HRD-DEC-015` |
| `HRD-Q-03` | Open Question | ~~Penanganan menu yang menunjuk halaman kosong~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-012` | `HRD-TF-005` |
| `HRD-Q-09` | Open Question | ~~Dokter praktik di luar jadwal kerja~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-013` | `HRD-DEC-006` |

### 14.7 Koreksi angka pada `HRD-TF-006`

Capability map menyebut lima belas route master data tidak seragam. Angka itu terlalu besar.
Pencarian awal menandai setiap route yang tidak memuat tanda hubung, padahal sebagian memang
kata tunggal yang tidak perlu dipisah.

Yang benar-benar perlu diubah oleh `HRD-DEC-014` ada **delapan**:

| Route sekarang | Menjadi | Controller |
|---|---|---|
| `actiontypes` | `action-types` | `DisciplinaryActionTypeController` |
| `casetypes` | `case-types` | `EmployeeRelationCaseTypeController` |
| `sanctiontypes` | `sanction-types` | `SanctionTypeController` |
| `violationtypes` | `violation-types` | `ViolationTypeController` |
| `workcalendars` | `work-calendars` | `WorkCalendarController` |
| `workschedules` | `work-schedules` | `WorkScheduleController` |
| `shiftgroups` | `shift-groups` | `ShiftGroupController` |
| `shiftpatterns` | `shift-patterns` | `ShiftPatternController` |

Yang **tidak** termasuk dan tetap dibiarkan: `shifts`, `doctors`, `employees`, `competencies`,
`professions`, dan `specializations`. Keenamnya kata tunggal berbentuk jamak dan sudah benar.

`organization` yang berbentuk tunggal sementara route lain jamak juga **tidak** termasuk.
Route itu punya sub-jalur `[type]` di frontend, sehingga perubahannya lebih berisiko daripada
tujuh route lain dan perlu dinilai terpisah. Dicatat sebagai `HRD-Q-14`.

### 14.8 Mengapa `HRD-DEC-014` dapat dijalankan dengan aman

Mengubah route adalah breaking change, dan `AGENTS.md` backend meminta konsumennya dinilai
lebih dulu. Penilaian itu sudah tersedia dari capability map, dan hasilnya mendukung keputusan
ini:

1. **Konsumen yang diketahui hanya satu**, yaitu frontend `QuilvianSystemFrontendDev`. Audit
   tidak menemukan konsumen lain di dalam workspace.
2. **Alamatnya terpusat.** Setiap route disimpan sebagai satu baris konstanta di
   `src/lib/constants/hr/master-data/<entity>/<entity>-constants.jsx`. Perubahan di sisi
   frontend berarti mengubah delapan baris, bukan menyisir seluruh source.
3. **Empat dari delapan route itu memang bermasalah sejak awal.** `actiontypes`, `casetypes`,
   `sanctiontypes`, dan `violationtypes` dimiliki empat controller yang salah tempat pada
   `HRD-TF-004`. Memperbaiki route dan memindahkan controller-nya masuk akal dikerjakan
   bersamaan.

Risiko yang tetap harus diterima: bila ada konsumen di luar workspace ini — misalnya aplikasi
lain, skrip integrasi, atau koleksi Postman tim — konsumen itu akan rusak tanpa peringatan.
Audit tidak dapat melihat ke luar workspace, jadi hal ini harus dipastikan manusia sebelum
perubahan dijalankan. Dicatat sebagai `HRD-Q-15`.

### 14.9 Status penutupan pertanyaan capability map

| Pertanyaan | Isi ringkas | Status |
|---|---|---|
| `HRD-TQ-01` | Nasib prefix `Wfp` | **Tertutup** oleh `HRD-DEC-008` |
| `HRD-TQ-02` | Kelanjutan ratchet `Hrd` | **Tertutup** oleh `HRD-DEC-008` |
| `HRD-TQ-03` | Tiga controller tanpa `[Authorize]` | **Terbuka** — perbaikan keamanan, menunggu wewenang tulis backend |
| `HRD-TQ-04` | Enam menu Administrasi Kepegawaian | **Tertutup** oleh `HRD-DEC-012` |
| `HRD-TQ-05` | Isi data pada 67 entity tanpa API | **Terbuka** — sama dengan `HRD-Q-05`, perlu pemeriksaan database |
| `HRD-TQ-06` | Batas serah terima payroll | **Tertutup** oleh `HRD-DEC-009`; bentuknya menyisakan `HRD-Q-10` dan `HRD-Q-11` |
| `HRD-TQ-07` | Privasi rekam kesehatan | **Tertutup sementara** oleh `HRD-DEC-010`, menunggu K3RS |
| `HRD-TQ-08` | Bentuk kotak masuk persetujuan | **Tertutup** oleh `HRD-DEC-011`; menyisakan `HRD-Q-12` dan `HRD-Q-13` |
| `HRD-TQ-09` | Empat controller yang salah tempat | **Tertutup** — digabungkan ke `HRD-DEC-014`, lihat bagian 14.8 butir 3 |
| `HRD-TQ-10` | Penamaan route master data | **Tertutup** oleh `HRD-DEC-014`, dengan koreksi jumlah pada bagian 14.7 |

Delapan dari sepuluh tertutup. Dua yang tersisa bukan keputusan desain: satu perbaikan
keamanan yang menunggu wewenang tulis, satu pemeriksaan database.

### 14.10 Open question yang masih terbuka

| ID | Pertanyaan | Memblokir | Pemilik |
|---|---|---|---|
| `HRD-Q-01` | Nama pemilik kebijakan bisnis, wakil Komite Medik, dan wakil K3RS | **Rilis produksi** slice kredensial dan kesehatan kerja. Tidak memblokir desain | Manajemen |
| `HRD-Q-05` | Apakah tabel 67 entity tanpa API sudah berisi data dari impor manual atau migrasi V1 | `IMPLEMENTATION` untuk enam domain yang diturunkan ulang | Pemilik database |
| `HRD-Q-06` | Dua puluh nilai kebijakan PRD pasal 28 | `LATER SLICE` untuk sebagian besar; `IMPLEMENTATION` untuk payroll, kredensial, dan cuti | Pemilik produk, Komite Medik, K3RS |
| `HRD-Q-08` | Pengesahan Komite Medik atas `HRD-DEC-005` dan daftar skenario yang boleh diblokir keras | **Rilis produksi** slice kredensial | Komite Medik |
| `HRD-Q-10` | Bentuk data serah terima payroll ke Finance, dan siapa yang menarik atau mengirim | `DESIGN` **hanya** untuk slice payroll | Pemilik produk + Finance |
| `HRD-Q-11` | Perilaku bila Finance menolak batch yang sudah `execute` | `DESIGN` **hanya** untuk slice payroll | Pemilik produk + Finance |
| `HRD-Q-12` | Apakah kotak masuk menampilkan pengajuan yang sudah diputuskan, atau hanya yang menunggu | `DESIGN` ringan untuk slice persetujuan | Pemilik teknis |
| `HRD-Q-13` | Perilaku delegasi saat atasan cuti, dan siapa yang berwenang mengaktifkannya | `DESIGN` untuk slice persetujuan | Pemilik produk |
| `HRD-Q-14` | Route `organization` yang berbentuk tunggal, ikut diseragamkan atau tidak | `LATER SLICE` | Pemilik teknis |
| `HRD-Q-15` | Apakah ada konsumen API HR di luar workspace ini yang akan rusak oleh `HRD-DEC-014` | `IMPLEMENTATION` untuk penggantian route | Pemilik teknis |
| `HRD-TQ-03` | Perbaikan tiga controller tanpa `[Authorize]` | `IMPLEMENTATION`. **Masalah keamanan yang sudah nyata hari ini** | Pemilik keamanan |

### 14.11 Acceptance criteria tambahan yang sudah dapat diuji

| ID | Kriteria | Cara menguji |
|---|---|---|
| `HRD-AC-06` | Rekam kesehatan kerja tidak bocor ke pemakai non-medis | Atasan membuka profil anak buah; response yang diterima memuat status kelayakan kerja, dan **tidak** memuat diagnosis maupun hasil pemeriksaan |
| `HRD-AC-07` | Satu kotak masuk memuat seluruh jenis pengajuan yang menunggu | Ajukan cuti, lembur, dan tukar shift atas nama tiga pegawai berbeda dengan atasan yang sama; ketiganya muncul pada satu halaman |
| `HRD-AC-08` | HR tidak melakukan pembayaran | Tidak ada endpoint HR yang mengubah status pembayaran; rantai berhenti pada `payroll-handoff/.../execute` |
| `HRD-AC-09` | Jam praktik di luar jadwal kerja tidak dihitung otomatis | Catat kehadiran dokter di luar jadwal kerjanya; hasilnya muncul sebagai pengecualian berstatus menunggu, bukan sebagai lembur yang sudah terhitung |
| `HRD-AC-10` | Delapan route lama tidak lagi dilayani setelah `HRD-DEC-014` dijalankan | Permintaan ke `workcalendars`, `shiftgroups`, `actiontypes`, dan lima route lain mengembalikan `404`, sementara bentuk kebab-case-nya berhasil |

### 14.12 Langkah berikutnya

Closure pass ini menutup delapan dari sepuluh pertanyaan capability map, dan seluruh keputusan
yang memblokir desain **untuk sebagian besar modul** sudah ada. Langkah berikutnya adalah
**`/design-business-module`**, dengan tiga pengecualian yang harus dinyatakan di dalam blueprint
dan tidak boleh dirancang final:

1. **Bentuk serah terima payroll ke Finance** menunggu `HRD-Q-10` dan `HRD-Q-11`. Batasnya sudah
   jelas lewat `HRD-DEC-009`, bentuk datanya belum.
2. **Slice kredensial** boleh dirancang di atas posisi sementara `HRD-DEC-005`, tetapi tidak
   boleh dinyatakan siap rilis sebelum Komite Medik mengesahkan.
3. **Slice kesehatan kerja** boleh dirancang di atas posisi sementara `HRD-DEC-010`, dengan
   pembatasan yang sama dari K3RS.

Dua hal yang berjalan di jalur terpisah dan tidak menunggu desain:

- **`HRD-TQ-03`** perbaikan keamanan tiga controller. Kecil, mendesak, dan hanya memerlukan
  wewenang tulis backend beserta penetapan branch.
- **`HRD-Q-05`** pemeriksaan isi database untuk 67 entity. Hasilnya menentukan apakah
  `HRD-DEC-004` berjalan sebagai perancangan bebas atau pekerjaan migration.

---

## 15. Amendment Pass — 27 Agustus 2026 — HISTORICAL SNAPSHOT

> **HISTORICAL SNAPSHOT.** Isi bagian ini adalah keadaan pada saat pass tersebut dijalankan,
> bukan wewenang terbaru. Keputusan yang sudah digantikan ditandai `superseded` beserta
> penggantinya. Wewenang terbaru selalu ada pada bagian bernomor tertinggi.

Pass ini mencatat dua koreksi pengguna atas keputusan Closure Pass, satu penegasan bentuk kotak
masuk, satu penarikan temuan audit yang keliru, dan daftar bagian yang sengaja tidak difinalkan.
Keputusan lama **tidak dihapus**; yang digantikan ditandai `superseded`.

### 15.1 Penarikan `HRD-TF-001` — temuan keamanan yang keliru

**`HRD-TF-001` pada capability map dinyatakan tidak berlaku.**

Capability map revision `1.0` menyatakan `MandatoryTrainingRuleController`,
`TrainingCatalogController`, dan `TrainingCategoryController` tidak memiliki `[Authorize]`,
sehingga 27 endpoint berpotensi terbuka tanpa autentikasi.

**Itu keliru.** Ketiganya memiliki `[Authorize]`, ditulis menyatu dengan `[ApiController]`:

| Controller | Bentuk penulisan | Baris |
|---|---|---|
| `MandatoryTrainingRuleController` | `[ApiController,Authorize]` | 20 |
| `TrainingCatalogController` | `[ApiController,Authorize]` | 19 |
| `TrainingCategoryController` | `[ApiController, Authorize]` | 17 |

Penyebabnya adalah pola pencarian yang dipakai saat audit, yang mensyaratkan kurung siku persis
di depan kata `Authorize`. Bentuk penulisan menyatu tidak tertangkap.

Pemeriksaan ulang dengan pencarian kata polos memberi hasil yang benar:

- **150 dari 150** controller HR memiliki `[Authorize]`;
- **tidak ada** `[AllowAnonymous]` di seluruh `Areas/Corporate/HumanResource/**` dan
  `Areas/SelfServices/HumanResource/**`;
- keempat controller yang salah tempat pada `HRD-TF-004` juga memiliki `[Authorize]`.

Ketiga controller itu bahkan sudah memakai `[AccessPermission]` per action, misalnya
`AccessPermission("TrainingCatalog","Read")` dan `AccessPermission("TrainingCatalog","Create")`,
sehingga otorisasinya lebih rinci daripada sekadar mensyaratkan login.

Akibat penarikan ini:

| Yang terpengaruh | Perubahan |
|---|---|
| `HRD-TF-001` | **Ditarik.** Bukan temuan |
| `HRD-TQ-03` | **Gugur.** Tidak ada perbaikan keamanan yang perlu dijalankan |
| `HRD-CAP-26` hak akses dan jejak audit | Status berubah dari `REPAIR` menjadi `READY TO REUSE` |
| Instruksi pengguna tentang security remediation paralel | Tidak ada yang perlu dikerjakan |

### 15.2 Koreksi `HRD-DEC-014` — alias, bukan breaking change

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-014` | Decision | ~~Delapan route diseragamkan sekarang, bersamaan dengan konstanta frontend~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-016` | `HRD-TF-006` |
| `HRD-DEC-016` | Decision | **Kebab-case ditetapkan sebagai route canonical untuk delapan route master data, tanpa breaking change.** Route lama tetap hidup sebagai *compatibility alias* dan **wajib memakai controller, service, dan business logic yang sama** — bukan salinan. Frontend dipindahkan bertahap ke route canonical. Alias lama hanya boleh dihapus setelah audit consumer selesai dan masa deprecation berakhir | Pengguna | `approved` | Pengguna, 2026-08-27 | Koreksi pengguna 2026-08-27 |

Yang berubah dari keputusan sebelumnya:

| Aspek | `HRD-DEC-014` | `HRD-DEC-016` |
|---|---|---|
| Route lama | Dimatikan | Tetap hidup sebagai alias |
| Perpindahan frontend | Serentak | Bertahap |
| Risiko konsumen luar | Rusak tanpa peringatan | Tidak ada yang rusak |
| Syarat penghapusan alias | — | Audit consumer selesai dan masa deprecation berakhir |

Aturan yang mengikat implementasinya: **satu action, satu implementasi**. Alias hanya menambah
route template pada action yang sama. Dilarang menggandakan controller, service, validasi, atau
aturan bisnis hanya untuk melayani nama lama. Bila dua nama menghasilkan dua implementasi,
keputusan ini dilanggar.

`HRD-Q-15` tetap **terbuka**, dan perannya berubah: bukan lagi gerbang sebelum penggantian
route, melainkan gerbang sebelum **penghapusan alias**.

`HRD-AC-10` diganti. Kriteria yang benar:

| ID | Kriteria | Cara menguji |
|---|---|---|
| `HRD-AC-10` | Nama canonical dan alias mengembalikan hasil yang sama dari implementasi yang sama | Panggil `work-calendars` dan `workcalendars` dengan parameter identik; response harus identik, dan keduanya menuju action yang sama |
| `HRD-AC-11` | Tidak ada implementasi ganda untuk alias | Setiap alias hanya berupa tambahan route template; tidak ada controller, service, atau validasi baru yang dibuat untuk melayani nama lama |

### 15.3 Penegasan `HRD-DEC-008` — target, bukan perintah rename massal

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-008` | Decision | ~~Seluruh entity HR dinormalkan ke prefix `Hrd`~~ | Pengguna | `superseded` | Digantikan `HRD-DEC-017` | `HRD-TF-002`, `HRD-TF-003` |
| `HRD-DEC-017` | Decision | **`Hrd` adalah target naming convention HR, bukan perintah rename seluruh entity sekaligus.** Rinciannya di bawah | Pengguna | `approved` | Pengguna, 2026-08-27 | Koreksi pengguna 2026-08-27 |

Isi `HRD-DEC-017` selengkapnya:

1. **Entity baru wajib `Hrd`.** Termasuk seluruh entity yang lahir dari penurunan ulang enam
   domain pada `HRD-DEC-004`.
2. **Entity `Wfp` dan `Trx` existing yang terbukti milik HR diperlakukan sebagai legacy**, dan
   dimigrasikan bertahap per domain sebagai kampanye tersendiri.
3. **Rename wajib mempertahankan data.** `QBE-DB-002` melarang `DROP` lalu `CREATE` bila rename
   yang mempertahankan data masih aman.
4. **Dilarang membuat satu migration besar untuk seluruh HR.** Satu kampanye satu domain,
   mengikuti pola tiga migration kehadiran yang sudah terbukti.
5. **Registry diperbarui lebih dulu** untuk mengenali prefix legacy milik HR selama masa
   transisi, sebelum kampanye pertama berjalan.
6. **Kepemilikan tidak boleh disimpulkan dari prefix.** Tidak semua `Trx*` milik HR.

Butir 6 terbukti dari hitungan pada snapshot:

| Area | Jumlah model `Trx*` |
|---|---:|
| `Areas/Corporate/**`, seluruhnya di bawah `HumanResource` | 178 |
| `Areas/HealthServices/**` | 40 |

Empat puluh entity `Trx*` adalah milik modul Health Services, bukan HR. Karena itu kepemilikan
ditetapkan dari **lokasi dan bukti**, bukan dari prefix. Klaim HR terbatas pada 178 entity di
bawah `Areas/Corporate/HumanResource/**`, dan itu pun masih diverifikasi per entity saat
kampanye domain masing-masing disiapkan.

### 15.4 Penegasan `HRD-DEC-011` — satu UX, banyak aturan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at |
|---|---|---|---|---|---|
| `HRD-DEC-018` | Decision | **Kotak masuk persetujuan terpadu hanya menyatukan pengalaman pengguna.** Workflow, policy, permission, validasi, SLA, dan eskalasi tetap dimiliki dan dijalankan **per jenis transaksi**. Kotak masuk tidak boleh menyeragamkan aturan bisnis | Pengguna | `approved` | Pengguna, 2026-08-27 |

Contoh supaya tidak salah bangun:

> Seorang kepala unit membuka satu kotak masuk dan melihat dua baris: permohonan cuti dan
> permohonan lembur. Keduanya tampil dengan bentuk ringkasan yang seragam. Namun permohonan
> cuti diperiksa dengan aturan saldo cuti dan matriks persetujuan cuti, sementara permohonan
> lembur diperiksa dengan aturan kelayakan lembur dan matriks persetujuan lembur. Batas waktu
> tanggapan dan jalur eskalasi keduanya juga berbeda, dan perbedaan itu **tetap berlaku**.

Yang boleh diseragamkan kotak masuk hanyalah: bentuk ringkasan baris, cara memfilter dan
mengurutkan, penanda status, dan cara berpindah ke halaman detail. Selebihnya milik domain
masing-masing.

### 15.5 Bagian yang sengaja tidak difinalkan pada blueprint

Empat bagian berikut boleh dirancang sampai batas yang jelas, tetapi **tidak boleh dinyatakan
final** dan tidak boleh dipakai sebagai dasar rilis:

| Bagian | Menunggu | Batas yang berlaku |
|---|---|---|
| Serah terima payroll dan penolakan batch oleh Finance | `HRD-Q-10`, `HRD-Q-11` | Batas tanggung jawab `HRD-DEC-009` sudah final; bentuk data dan perilaku penolakan belum |
| Kesiapan rilis slice kredensial | `HRD-Q-08`, Komite Medik | Desain boleh berdiri di atas posisi sementara `HRD-DEC-005` |
| Rincian kesehatan kerja | K3RS | Desain boleh berdiri di atas posisi sementara `HRD-DEC-010` |
| Keputusan skema yang merusak data | `HRD-Q-05`, audit database | Penurunan ulang enam domain tidak boleh menghasilkan migration destruktif sebelum isi tabel diketahui |

### 15.6 Keputusan yang tetap berlaku tanpa koreksi

`HRD-DEC-009`, `HRD-DEC-010`, `HRD-DEC-011`, `HRD-DEC-012`, `HRD-DEC-013`, dan `HRD-DEC-015`
tetap berlaku apa adanya. `HRD-DEC-010` tetap `draft` menunggu K3RS.

---

## 16. Amendment Pass 1.1 — Kebijakan Penamaan, 27 Agustus 2026

Pass ini mencatat koreksi material dari technical owner atas `HRD-DEC-017`, ditambah beberapa
perbaikan konsistensi angka. Keputusan lama **tidak dihapus**.

### 16.1 `HRD-DEC-019` — kebijakan penamaan canonical modul HR

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HRD-DEC-017` | Decision | ~~`Hrd` adalah target naming convention HR; seluruh entity `Wfp` dan `Trx` milik HR dimigrasikan bertahap per domain~~ | Pengguna | `superseded` | **Digantikan `HRD-DEC-019`** | `HRD-TF-002`, `HRD-TF-003` |
| `HRD-DEC-019` | Decision | **Kebijakan penamaan canonical modul HR** — empat keluarga prefix dengan perlakuan berbeda. Rinciannya di bawah | Technical owner | `approved` | Technical owner, 2026-08-27 | Koreksi technical owner 2026-08-27 |

Yang berubah secara material dari `HRD-DEC-017`:

| Aspek | `HRD-DEC-017` | `HRD-DEC-019` |
|---|---|---|
| `Wfp` | Legacy yang akan dimigrasikan ke `Hrd` | **Prefix yang sah dan tetap dipakai.** Bukan legacy |
| `Mst` | Tidak dibahas tegas | **Tetap prefix master/reference.** Tidak diubah hanya karena berada di domain HR |
| `Trx` | Dimigrasikan bertahap per domain sebagai kampanye | **Ratchet saat disentuh saja.** Tidak ada kampanye yang mengejar seluruh `Trx*` |
| Bentuk pekerjaan | Kampanye per domain yang dijadwalkan | **Aturan lintas-slice** yang berlaku sepanjang implementasi |
| Target akhir | Seluruh entity operasional HR menjadi `Hrd*` | **Tidak ada target pembersihan menyeluruh** dan tidak ada tenggat |

### 16.2 Matriks prefix yang berlaku

| Prefix | Arti | Entity baru boleh memakainya? | Kebijakan untuk entity yang sudah ada |
|---|---|---|---|
| `Mst` | Data master atau referensi | **Ya**, bila entity-nya memang master/reference | Tidak diubah. Berada di domain HR bukan alasan untuk mengubahnya menjadi `Hrd` |
| `Wfp` | Keluarga entity workforce dan profil HR | **Ya**, bila entity-nya memang bagian keluarga itu | Tidak diubah. `Wfp` sah dan tetap dipakai. **Bukan** legacy yang akan dihapus |
| `Hrd` | Entity operasional atau transaksional HR — canonical dan default | **Ya.** Ini pilihan bawaan untuk entity HR baru yang tidak termasuk keluarga `Mst` maupun `Wfp` | Tetap `Hrd` |
| `Trx` | Prefix generik warisan | **Tidak.** Entity transaksional HR baru dilarang memakainya | Dibiarkan berjalan. Berubah menjadi `Hrd*` **hanya** saat entity itu benar-benar disentuh |

Penjelasan singkat supaya tidak salah pilih saat membuat entity baru:

> Sebuah tabel berisi daftar jenis cuti yang dipakai sebagai pilihan di formulir adalah data
> master, jadi namanya `Mst...`.
>
> Sebuah tabel berisi riwayat pendidikan seorang pegawai adalah bagian profil workforce, jadi
> namanya `Wfp...`.
>
> Sebuah tabel berisi pengajuan lembur yang punya status, pemohon, dan penyetuju adalah entity
> operasional, jadi namanya `Hrd...`.
>
> Tidak ada satu pun dari ketiganya yang boleh memakai `Trx...` bila dibuat sekarang.

### 16.3 Pendaftaran prefix pada registry

Yang perlu dicapai pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`:

| Prefix | Keadaan sekarang | Target |
|---|---|---|
| `Hrd` | Sudah ada. Baris 9 mencatat `Corporate / SelfServices \| Human Resource \| BUSINESS DOMAIN \| Hrd \| ACTIVE / LEGACY` | Tetap, dengan pemahaman bahwa `Hrd` adalah prefix operasional canonical dan default modul HR |
| `Wfp` | **Belum ada barisnya sama sekali** | Didaftarkan sebagai prefix yang sah untuk keluarga workforce/profil HR |
| `Mst` | Sudah ada sebagai kategori `MASTER / REFERENCE` | Tetap mengikuti kepemilikan master yang berlaku. Tidak ada perubahan |
| `Trx` | Tidak dimiliki HR | **Registry tidak boleh menyatakan seluruh `Trx` milik HR** |

Aturan penting saat mendaftarkan `Wfp`: **pakai format dan kosakata yang memang sudah ada di
registry.** Registry saat ini memakai kolom `Area`, `Module/owner`, `Category`, `Prefix`, dan
`Lifecycle`, dengan nilai lifecycle `PLANNED`, `ACTIVE`, `LEGACY`, dan `DEPRECATED`. Jangan
mengarang kolom, kategori, maupun nilai lifecycle baru. Bila registry sudah menyediakan mekanisme
baris terpisah atau alias, pakai mekanisme itu.

**`Wfp` tidak boleh diberi label legacy yang akan dihapus**, kecuali ada keputusan manusia
terpisah di kemudian hari.

### 16.4 Kepemilikan `Trx` tidak boleh disimpulkan dari prefixnya

Bukti pada snapshot `ecdc135`:

| Lokasi | Jumlah model `Trx*` |
|---|---:|
| `Areas/Corporate/HumanResource/**` | 178 |
| `Areas/HealthServices/**` | 40 |

Empat puluh entity `Trx*` adalah milik modul Health Services. Aturan ratchet pada `HRD-DEC-019`
**hanya** berlaku untuk entity yang kepemilikannya terbukti berada pada Human Resource, dan
pembuktiannya memakai domain, lokasi berkas, dan bukti — bukan nama prefixnya.

### 16.5 Definisi material touch

Aturan ratchet `Trx*` menjadi `Hrd*` hanya berlaku ketika entity itu **materially touched**.
Definisi ini sengaja dibuat tegas supaya proses bisnis yang sedang berjalan tidak terganggu.

**Termasuk material touch** — sebuah task mengubah salah satu dari:

| Yang berubah | Contoh |
|---|---|
| Entity atau class persistence | Menambah properti pada `TrxJobRequisition` |
| Konfigurasi Entity Framework | Mengubah `TrxWorkflowInstanceConfiguration` |
| Tabel atau kolom fisik | Menambah kolom baru pada tabel |
| Relasi atau foreign key | Menambah relasi ke entity lain |
| Index atau constraint | Menambah unique constraint |
| Lifecycle persistence | Mengubah perilaku soft-delete atau audit |
| Migration yang memang mengenai entity itu | Migration yang menyentuh tabelnya |

**Bukan material touch** — hal berikut **tidak** memicu rename:

| Yang dikerjakan | Alasan |
|---|---|
| Pekerjaan frontend saja | Tidak menyentuh persistence sama sekali |
| Membaca entity dari controller atau service | Membaca bukan mengubah |
| Dokumentasi | Tidak menyentuh source |
| Perubahan tampilan | Tidak menyentuh persistence |
| Perbaikan bug pada controller atau service yang tidak mengubah kontrak persistence | Bentuk datanya tidak berubah |
| Refactor yang tidak mengubah entity maupun schema | Bentuk datanya tidak berubah |

Contoh supaya batasnya jelas:

> Sebuah task memperbaiki pesan error pada `LeaveRequestController`. Task itu membaca
> `WfpLeaveRequest`, tidak mengubah kolom apa pun, dan tidak membuat migration. **Tidak ada
> rename.** Task selesai apa adanya.
>
> Sebuah task menambahkan kolom alasan pembatalan pada `TrxWorkflowInstance`. Ini menyentuh
> entity dan tabel fisik, sehingga **ratchet berlaku**: entity itu sekaligus dinormalkan menjadi
> `HrdWorkflowInstance` dalam task yang sama.

Semangatnya adalah **ratchet saat disentuh**, bukan pembersihan menyeluruh repository.

### 16.6 Aturan pelaksanaan ratchet

Ketika ratchet memang berlaku, langkah berikut mengikat:

1. **Rename wajib mempertahankan data.** `QBE-DB-002` melarang `DROP` lalu `CREATE` selama
   rename yang mempertahankan data masih aman.
2. **Nama class di source dan nama tabel fisik dinormalkan bersamaan**, sesuai `QBE-NAM-003`.
   Tidak boleh salah satu saja.
3. **Audit lebih dulu** foreign key, index, constraint, dependency, dan riwayat migration yang
   menyentuh entity itu, sesuai `QBE-DB-001`.
4. **Ikuti cakupan task atau domain yang sedang dikerjakan.** Jangan melebar ke entity lain yang
   kebetulan bertetangga.
5. **Proses bisnis yang sedang berjalan tidak boleh terputus** hanya demi konsistensi penamaan.

### 16.7 Yang dilarang oleh `HRD-DEC-019`

1. Rename massal seluruh entity HR.
2. Kampanye migration yang tujuannya semata-mata mengejar seluruh `Trx*` sekaligus.
3. Menetapkan tenggat untuk membersihkan seluruh `Trx*`.
4. Mengubah `Wfp*` menjadi `Hrd*` hanya karena namanya berbeda.
5. Mengubah `Mst*` menjadi `Hrd*` hanya karena entity-nya berada di domain HR.
6. Menyimpulkan kepemilikan entity dari prefix `Trx`.
7. Membuat entity transaksional HR baru dengan prefix `Trx`.

### 16.8 Perbaikan konsistensi angka

**Jumlah slice pada roadmap.** Angka pada ringkasan roadmap salah hitung; daftar nama slicenya
sudah benar sejak awal.

| Status | Sebelum | Sesudah |
|---|---:|---:|
| `READY` | 15 | **18** |
| `PARTIAL` | 1 | 1 |
| `BLOCKED` | 6 | 6 |
| `DEFERRED` | 1 | 1 |
| **Total** | 23 | **26** |

Delapan belas slice `READY`: `S0-A`, `S0-B`, `S-A1` sampai `S-A7`, `S-B1` sampai `S-B4`,
`S-C2` sampai `S-C5`, dan `S-E`.

Angka 15 pada `MODULE-STATUS.md` bagian 2 **tidak diubah**, karena angka itu menghitung
**kelompok kemampuan**, bukan slice implementasi. Keduanya memang berbeda dan tidak perlu sama.

**Angka 68 dan 67.** Definisi yang dipakai konsisten mulai sekarang:

| Pernyataan | Angka |
|---|---:|
| Model yang berada di dalam enam domain tanpa controller | **68** |
| Di antaranya yang sudah punya API lewat domain lain — `MstWorkforceRequirement` | **1** |
| Entity yang benar-benar belum punya API dan menjadi kandidat penurunan ulang | **67** |

Kalimat "68 model tanpa API" adalah keliru dan diganti. Yang benar: 68 model berada di dalam
enam domain tanpa controller, dan 67 di antaranya benar-benar belum punya API.

### 16.9 Artefak yang diperbarui pada pass ini

| Berkas | Yang berubah |
|---|---|
| `00-interview-decisions.md` | Bagian 16 ini; `HRD-DEC-017` ditandai `superseded` |
| `blueprint-manifest.md` | Revision naik ke `2`; tabel keputusan mengikat; daftar larangan |
| `MODULE-STATUS.md` | Baris `S-E`; wording blocker `HRD-BLK-004` |
| `00-business-overview.md` | Angka 68 diganti 67 beserta definisinya |
| `01-prerequisite-readiness.md` | `HRD-DEP-001` dan `HRD-PRE-001` ditulis ulang |
| `roadmap/00-slice-roadmap.md` | `S0-A` dan `S-E` ditulis ulang; hitungan slice; wording 68/67 |
| `02-existing-capability-map.md` | Angka ringkasan diperbaiki; tetap hanya penunjuk |

`01-existing-capability-map.md` **tidak diubah**. Berkas itu mencatat bukti source apa adanya
pada `ecdc135`, bukan kebijakan penamaan target. Fakta bahwa 40 entity memakai `Wfp` dan 178
memakai `Trx` tetap benar sebagai pengamatan; yang berubah hanyalah apa yang hendak dilakukan
terhadap fakta itu.

Konsekuensi lain: `HRD-TF-002` pada capability map menyebut `Wfp` sebagai prefix yang tidak
terdaftar dan memberinya status `CONFLICT`. Pengamatan itu **tetap benar** — `Wfp` memang belum
ada barisnya di registry hari ini. Yang berubah adalah penyelesaiannya: dulu diselesaikan dengan
migrasi ke `Hrd`, sekarang diselesaikan dengan **mendaftarkan `Wfp` sebagai prefix yang sah**.

---

## 17. Baseline Impact Gate — 27 Agustus 2026

Pass ini tidak mengubah satu pun keputusan bisnis. Isinya perpindahan baseline dan dua
pertanyaan baru yang lahir darinya.

### 17.1 Perpindahan baseline backend

Baseline backend canonical berpindah dari branch `AndryZain` ke
**`origin/QuilvianIntegrationBackend`**. Branch `master` dan `AgentCodexBackend` **tidak** dipakai
sebagai baseline.

| Field | Isi |
| --- | --- |
| SHA lama | `ecdc135` — branch `AndryZain` |
| SHA baru | `16b8b71` — `origin/QuilvianIntegrationBackend` |
| Hubungan | Divergen. Merge-base `7a0f60d` |
| Hasil impact scan | **`NO_IMPACT`** pada seluruh tujuh jalur yang di-scan |
| Keadaan capability map | **`CURRENT`** — tidak ada audit ulang, tidak ada capability yang `STALE` |
| Frontend | Tidak ada drift. `origin/AgentCodexFrontend` tetap `2a1cea784` |

Rincian lengkap beserta tabel per jalur ada di [`MODULE-STATUS.md`](./MODULE-STATUS.md)
bagian 6.1.

Seluruh 746 berkas source HR, 354 konfigurasi EF, 214 migration, `ApplicationDbContext.cs`, dan
registry **identik byte per byte** di kedua sisi. Sembilan commit yang hanya ada di Integration
seluruhnya pekerjaan Rawat Inap dan tidak menyentuh HR.

### 17.2 Dua pertanyaan baru

| Decision ID | Type | Pertanyaan | Owner | Status | Memblokir |
| --- | --- | --- | --- | --- | --- |
| `HRD-Q-16` | Open Question | Berkas `docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md` hanya ada di branch `AndryZain` dan **tidak ada** di baseline canonical. Apakah berkas itu ikut dibawa ke Integration, atau rujukannya pada `HRD-DEC-002` diubah menjadi rujukan historis? | Pemilik teknis | `draft` | Tidak memblokir desain. Memblokir kerapian rujukan saat blueprint masuk Integration |
| `HRD-Q-17` | Open Question | Branch kerja mana yang dipakai untuk task implementasi HR berikutnya? Karena `AndryZain` dan Integration divergen, pemindahan pekerjaan HR bukan fast-forward | Pemegang modul HR | `draft` | `IMPLEMENTATION`. Tidak memblokir desain maupun flow |

`HRD-Q-17` perlu diperhatikan sebelum task implementasi pertama dimulai. `AGENTS.md` backend
menyatakan bila penetapan branch yang otoritatif tidak tersedia, pekerjaan berhenti dan
penetapannya diminta. Penetapan itu belum ada untuk modul HR.

### 17.3 Koreksi konsistensi pada pass ini

| Yang dikoreksi | Sebelum | Sesudah |
| --- | --- | --- |
| Header revision decision log | `2`, padahal isinya sudah memuat `HRD-DEC-019` | `3`, dengan keterangan bahwa revision `3` memuat amendment penamaan |
| Jumlah flow pada `MODULE-STATUS.md` | "dua belas business flow" | **15 berkas**, satu module context dan empat belas business process, beserta daftarnya |
| Kelas QBE pada `S-C4` | Menyatakan model `Wfp*` dan `Trx*` sama-sama `TOUCHED LEGACY` | Ditentukan **per entity**: `Wfp*` tetap `Wfp*` dan **bukan** `TOUCHED LEGACY`; hanya `Trx*` milik HR yang tunduk ratchet |
| Baseline SHA pada manifest dan status | Satu nilai `ecdc135` | Dua nilai terpisah: diaudit pada `ecdc135`, diverifikasi berlaku pada `16b8b71` |

Audit wording `Wfp` dijalankan pada seluruh roadmap. Hanya satu tempat yang keliru, yaitu kelas
QBE pada `S-C4`. Sebelas rujukan `Wfp` lainnya sudah benar dan tidak diubah.

### 17.4 Yang tetap berlaku tanpa perubahan

- `01-existing-capability-map.md` **tidak disentuh**. Berkas itu mencatat bukti source apa adanya
  dan tidak boleh diubah agar cocok dengan kebijakan penamaan yang baru.
- `HRD-TF-001` tetap **ditarik**, dan `HRD-TQ-03` tetap **gugur**, sesuai capability map revisi
  `1.1`. Tidak ada security remediation yang lahir kembali dari bagian historis.
- Kebijakan target tetap bersumber pada `HRD-DEC-016`, `HRD-DEC-018`, dan `HRD-DEC-019`.
- Seluruh blocking decision tidak berubah: kredensial dan kesehatan kerja menunggu upstream,
  serah terima payroll menunggu `HRD-Q-10` dan `HRD-Q-11`, keputusan skema destruktif menunggu
  `HRD-Q-05`.

---

## 18. Housekeeping Decisions — 27 Agustus 2026

Dua pertanyaan yang lahir dari Baseline Impact Gate ditutup pada pass ini. Keduanya keputusan
teknis, bukan kebijakan bisnis.

### 18.1 `HRD-DEC-020` — Provenance masukan produk

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-020` | Decision | **PRD HRD lama disimpan sebagai snapshot di dalam folder blueprint**, lengkap dengan sidik jari dan asal-usulnya. Statusnya tetap **masukan produk historis**, bukan PRD blueprint dan bukan sumber kebenaran. Membawanya ke repository canonical **tidak** mengubah kewenangannya | Technical owner | `approved` | Technical owner, 2026-08-27 | `HRD-Q-16`, `HRD-IMP-001` |

Yang dikerjakan:

| Hal | Isi |
| --- | --- |
| Snapshot | `evidence/01-product-input-prd-hrd.snapshot.md`, 1.650 baris, identik dengan aslinya |
| SHA-256 | `3364e50060b95cd7c9a540d9dc943e59e178a485b167a6d585c80f4629e79169` |
| Commit asal | `ecdc135` — branch `AndryZain` |
| Catatan provenance | `evidence/00-product-input-provenance.md` |

Susunan kewenangan yang ditegaskan ulang oleh keputusan ini:

| Artefak | Kewenangannya |
| --- | --- |
| `00-interview-decisions.md` | Keputusan manusia |
| `01-existing-capability-map.md` | Bukti source apa adanya |
| Arsitektur dan kontrak blueprint | Desain target |
| `04-prd-to-mvp.md` | **PRD resmi**, ditulis paling akhir |
| Snapshot masukan produk | Historis. Tidak mengikat |

| `HRD-Q-16` | Open Question | ~~PRD tidak ada di baseline canonical, rujukan berpotensi putus~~ | Technical owner | `resolved` | Ditutup `HRD-DEC-020` 2026-08-27. Blueprint kini mandiri; rujukan tidak lagi putus | `HRD-IMP-001` |

### 18.2 `HRD-DEC-021` — Baseline implementasi canonical

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-021` | Decision | **Baseline implementasi canonical backend adalah `origin/QuilvianIntegrationBackend`.** Setiap pekerjaan implementasi HR wajib membuat personal atau feature branch dari HEAD terbaru branch itu | Technical owner | `approved` | Technical owner, 2026-08-27 | `HRD-Q-17` |

Isi selengkapnya:

**Yang menjadi baseline**

`origin/QuilvianIntegrationBackend`, HEAD terbaru pada saat pekerjaan dimulai.

**Yang tidak boleh menjadi kewenangan implementasi**

`AndryZain`, `AgentCodexBackend`, `master`, branch historis mana pun, dan branch lain di luar
yang disebut di atas.

**Jalur implementasi yang wajib diikuti**

```text
personal / feature branch dari HEAD QuilvianIntegrationBackend
  -> Pull Request ke QuilvianIntegrationBackend
  -> pemeriksaan QBE yang diwajibkan
  -> gerbang review, build, test, dan migration
  -> setelah lolos, promosi ke QuilvianDevDeploy
```

**Yang tidak diberikan oleh keputusan ini**

Blueprint tetap **tidak** memberi wewenang tulis secara otomatis. Wewenang implementasi tetap
diberikan terpisah per task, sesuai `AGENTS.md` backend. Keputusan ini hanya menetapkan **dari
mana** branch kerja dibuat dan **ke mana** hasilnya dikembalikan, bukan izin untuk mulai
mengerjakannya.

| `HRD-Q-17` | Open Question | ~~Branch kerja untuk task implementasi HR belum ditetapkan~~ | Pemegang modul HR | `resolved` | Ditutup `HRD-DEC-021` 2026-08-27 | Impact scan bagian 17.1 |

### 18.3 Catatan penting tentang divergensi

`AndryZain` dan `QuilvianIntegrationBackend` divergen dengan merge-base `7a0f60d`. Namun impact
scan sudah membuktikan seluruh source HR **identik** di kedua sisi, dan satu-satunya berkas yang
hanya ada di `AndryZain` adalah PRD yang kini sudah disnapshot lewat `HRD-DEC-020`.

Artinya tidak ada source HR yang perlu dipindahkan. Feature branch berikutnya cukup dibuat dari
HEAD `QuilvianIntegrationBackend` seperti biasa, tanpa merge, cherry-pick, maupun rebase dari
`AndryZain`.

---

## 19. Pertanyaan Baru dari PHASE 2A — 27 Agustus 2026

Lima flow inti administratif ditulis pada `flows/`. Prosesnya memunculkan enam belas pertanyaan
yang tidak dapat dijawab source code. Seluruhnya ditandai `[OPEN]` di dalam flow masing-masing
dan **tidak** diisi dengan praktik umum maupun dugaan.

### 19.1 Daftar pertanyaan

| ID | Pertanyaan | Asal flow | Owner | Memblokir |
| --- | --- | --- | --- | --- |
| `HRD-Q-18` | Apa yang terjadi bila penetapan gaji berlaku surut ke periode payroll yang sudah tertutup? Ini berpotensi mengubah gaji yang sudah dibayarkan | 01 | Pemilik produk + Finance | Desain final penetapan gaji berlaku surut |
| `HRD-Q-19` | Apakah penetapan gaji dan penempatan memerlukan persetujuan? Source **tidak** menunjukkan jalur persetujuan untuk keduanya, padahal keduanya sensitif | 01 | Pemilik produk | Desain final jalur persetujuan administrasi |
| `HRD-Q-20` | Siapa yang boleh membaca penetapan gaji pegawai lain, dan sampai tingkat apa? | 01 | Pemilik produk + keamanan | Desain final hak akses halaman lintas-pegawai |
| `HRD-Q-21` | Apakah `EmployeeProfileChange` memakai kosakata status yang sama dengan pengajuan HR lain? | 01 | Backend owner | Tabel state transition final flow 01 |
| `HRD-Q-22` | Jenis pengecualian apa yang dipakai untuk aktivitas dokter di luar jadwal kerja? `ScheduleMismatch` sudah ada, tetapi belum tentu untuk kasus ini | 02 | Pemilik produk + backend owner | Desain final `HRD-DEC-013` |
| `HRD-Q-23` | Siapa yang berwenang membuka kembali periode kehadiran yang sudah ditutup? | 02 | Pemilik produk | Desain final penutupan periode |
| `HRD-Q-24` | Apakah koreksi kehadiran berstatus `Applied` benar-benar tidak dapat dibatalkan, dan hanya dapat diperbaiki lewat permohonan baru? | 02 | Backend owner | Tabel state transition final flow 02 |
| `HRD-Q-25` | Berapa lama rekaman mentah kehadiran disimpan? | 02 | Pemilik produk | Kebijakan retensi |
| `HRD-Q-26` | Berapa lama pengajuan cuti menunggu sebelum menjadi `Expired`, dan apa akibatnya bagi pegawai? | 03 | Pemilik produk | Desain final batas waktu persetujuan |
| `HRD-Q-27` | Apakah penyesuaian saldo cuti oleh HR memerlukan persetujuan? Source tidak menunjukkan jalurnya, padahal ini mengubah hak pegawai | 03 | Pemilik produk | Desain final penyesuaian saldo |
| `HRD-Q-28` | Apakah cuti berstatus `Completed` benar-benar tidak dapat dibatalkan, dan koreksinya hanya lewat penyesuaian saldo? | 03 | Backend owner | Tabel state transition final flow 03 |
| `HRD-Q-29` | Saat pemanggilan kembali dari cuti, apakah sisa hari dikembalikan penuh ke saldo? | 03 | Pemilik produk | Desain final pemanggilan kembali |
| `HRD-Q-30` | Dalam keadaan apa verifikasi realisasi lembur boleh dilewati? Nilai `Skipped` ada, aturannya belum | 04 | Pemilik produk | Desain final verifikasi lembur |
| `HRD-Q-31` | Berapa lama cuti pengganti berlaku sebelum kedaluwarsa, dan apakah dapat diperpanjang? | 04 | Pemilik produk | Desain final cuti pengganti |
| `HRD-Q-32` | Siapa yang berwenang membuka kembali periode lembur yang sudah ditutup? | 04 | Pemilik produk | Desain final penutupan periode lembur |
| `HRD-Q-33` | Bagaimana peran `Supervisor`, `Manager`, `HrAdmin`, dan `Payroll` pada alur lembur dipetakan ke role aplikasi yang sebenarnya? | 04 | Pemilik produk + keamanan | Matriks kewenangan |

### 19.2 Pengelompokan

| Kelompok | Pertanyaan | Sifatnya |
| --- | --- | --- |
| Kewenangan membuka kembali periode | `HRD-Q-23`, `HRD-Q-32` | Sama bentuknya untuk kehadiran dan lembur. Dapat dijawab sekaligus |
| Persetujuan yang tidak terbukti ada | `HRD-Q-19`, `HRD-Q-27` | Source tidak menunjukkan jalur persetujuan untuk transaksi sensitif. Perlu dikonfirmasi apakah memang tidak ada, atau memang belum dibuat |
| Keadaan akhir yang tidak dapat dibatalkan | `HRD-Q-24`, `HRD-Q-28` | Disimpulkan dari ketiadaan endpoint. Perlu konfirmasi backend owner |
| Nilai kebijakan | `HRD-Q-25`, `HRD-Q-26`, `HRD-Q-29`, `HRD-Q-30`, `HRD-Q-31` | Turunan `HRD-Q-06`. Tidak memblokir alurnya, memblokir rilis produksi |
| Hak akses data sensitif | `HRD-Q-20`, `HRD-Q-33` | Menyangkut data gaji dan pemetaan peran |
| Konfirmasi kosakata source | `HRD-Q-21`, `HRD-Q-22` | Dapat dijawab dengan pembacaan source lanjutan, bukan keputusan bisnis |

Dua pertanyaan terakhir — `HRD-Q-21` dan `HRD-Q-22` — sebenarnya dapat dijawab tanpa rapat.
Keduanya butuh pembacaan source yang lebih dalam pada `EmployeeProfileChange` dan pada cara
`ScheduleMismatch` dipakai. Sisanya memerlukan manusia yang berwenang.

### 19.3 Yang sengaja tidak diisi

Selama menulis lima flow, beberapa tempat terasa "jelas" jawabannya menurut praktik umum:
berapa hari cuti tahunan, berapa menit toleransi keterlambatan, berapa lama cuti pengganti
berlaku, berapa tingkat persetujuan yang wajar. **Tidak satu pun diisi.**

Alasannya: nilai-nilai itu adalah kebijakan ketenagakerjaan rumah sakit, bukan pilihan teknis.
Menuliskannya di blueprint akan membuat orang berikutnya mengiranya sudah disetujui, padahal
tidak ada seorang pun yang pernah memutuskannya.

---

## 20. PHASE 2A.1 — Flow Evidence Hardening, 27 Agustus 2026

Pengguna menyetujui arah `PHASE 2A` tetapi meminta satu pass pengerasan sebelum `PHASE 2B`:
membedakan tegas antara **state vocabulary** (nilai enum/status terbukti ada) dan **transition
edge** (bukti bahwa satu transisi tertentu benar-benar dijaga guard/controller/service/validator).
Pass ini murni audit source **read-only** atas flow 01–04. **Tidak ada source code, database,
migration, controller, entity, maupun frontend yang diubah** — seluruh perubahan pada pass ini
adalah dokumentasi.

### 20.1 Metode

Empat sub-agent riset dijalankan paralel, masing-masing terbatas pada satu domain backend
(`AttendanceManagement`, `LeaveManagement`, `OvertimeManagement`, dan `WorkforceCore` untuk
kosakata `EmployeeProfileChange`), dengan instruksi eksplisit: kutip file dan baris, bedakan
"nilai enum ada" dari "guard yang menegakkannya ada", dan laporkan `UNVERIFIED` bila tidak
ditemukan alih-alih menebak.

### 20.2 Triase `HRD-Q-18` s.d. `HRD-Q-33`

| Kelompok | Pertanyaan | Alasan |
| --- | --- | --- |
| `SOURCE_RESOLVABLE` | `HRD-Q-21`, `HRD-Q-22` (bagian kosakata), `HRD-Q-24`, `HRD-Q-28` | Terjawab murni dari membaca source, tanpa keputusan manusia |
| `PERMISSION_MAPPING` | `HRD-Q-20`, `HRD-Q-23`, `HRD-Q-32`, `HRD-Q-33` | Mekanismenya (guard status, gerbang permission) sudah terbukti ada; yang tersisa adalah **siapa** yang seharusnya diberi permission atau bagaimana peran dipetakan ke permission nyata — bukan lagi dapat dijawab source, tapi juga bukan murni nilai kebijakan |
| `BUSINESS_DECISION` | `HRD-Q-18`, `HRD-Q-19`, `HRD-Q-25`, `HRD-Q-26`, `HRD-Q-27`, `HRD-Q-29` (nilai kebijakannya), `HRD-Q-30`, `HRD-Q-31` | Nilai kebijakan ketenagakerjaan atau keputusan proses yang tidak dapat diturunkan dari source apa pun |

Seluruh `SOURCE_RESOLVABLE` sudah diselesaikan pada pass ini. Tidak ada keputusan manusia yang
diminta untuk sesuatu yang sebenarnya dapat dibuktikan dari source.

### 20.3 Pertanyaan yang tertutup lewat audit source

| ID | Jawaban | Evidence |
| --- | --- | --- |
| `HRD-Q-21` | **Tidak.** `EmployeeProfileChange` **tidak** memakai `LeaveRequestValueConstants.Status`. Statusnya `string` polos pada `TrxEmployeeProfileChangeRequest.RequestStatus`, divalidasi array privat `EmployeeProfileChangeService.RequestStatuses = {Draft, Submitted, UnderVerification, NeedRevision, Approved, Rejected, Cancelled, Applied}` — tipe berbeda dari `LeaveManagement`, kebetulan berbagi sebagian nama nilai | `flows/01-employee-administration.md` bagian 9 |
| `HRD-Q-22` (bagian kosakata) | **Tidak.** `ScheduleMismatch` satu-satunya titik pakainya adalah `AttendanceProcessingService.BuildExceptions`, guard `!schedule.IsResolved`, kode `SCHEDULE_UNRESOLVED` — artinya "jadwal tidak dapat diselesaikan", bukan "kehadiran di luar jendela jadwal yang sudah ada". Tidak ada kode yang mendeteksi kasus dokter di luar jadwal kerja hari ini | `flows/02-attendance.md` bagian 6.1 dan 7 |
| `HRD-Q-24` | **Tidak sepenuhnya.** `TerminalRequestStatuses` menyatakan niat `Applied` final tapi tidak pernah dirujuk sebagai guard di tempat lain. Endpoint `synchronize` dapat menurunkan `Applied` kembali ke `Approved` dan memicu ulang apply. Ini celah implementasi, bukan pertanyaan kebijakan | `flows/02-attendance.md` bagian 9.4 |
| `HRD-Q-28` | **Tidak.** `POST /{leaveRequestId}/reverse` tidak punya guard status; hanya memblokir bila `ExecutionStatus == Reversed`. `Completed` dapat kembali ke `Cancelled`/`Taken` | `flows/03-leave.md` bagian 9.1 |

### 20.4 Pertanyaan baru dari temuan audit

Tiga celah ditemukan yang sebelumnya tidak diduga — dokumen lama menganggap suatu invariant
terbukti hanya karena nilai enum-nya ada, padahal guard-nya tidak pernah ditulis atau tidak
efektif. Business requirement di baliknya **tidak dihapus**; hanya diturunkan menjadi pertanyaan
implementasi baru:

| ID | Isi | Owner | Memblokir |
| --- | --- | --- | --- |
| `HRD-Q-34` | Celah `AttendanceCorrection.synchronize` dapat menurunkan `Applied` kembali ke `Approved` dan memicu ulang apply. Perlu ditutup lewat perbaikan kode, atau ada alasan bisnis yang membenarkannya? | Backend owner | Keputusan perbaikan implementasi |
| `HRD-Q-35` | `LeaveExecution./reverse` dapat membalik cuti `Completed` tanpa guard status. Ini jalur resmi yang disengaja, atau celah yang perlu ditutup? | Backend owner + pemilik produk | Keputusan perbaikan implementasi |
| `HRD-Q-36` | `RecallStatus.Acknowledged` terbukti tidak digerbangi kode — alur dapat lompat `WaitingApproval` → `Approved` tanpa melaluinya. Apakah ini seharusnya menjadi gate wajib? | Pemilik produk | Desain final pemanggilan kembali |

### 20.5 Verifikasi lima rule high-impact

| Domain | Rule | Hasil |
| --- | --- | --- |
| Attendance | Periode tidak dapat ditutup bila exception `Open` masih ada | **PROVEN.** `AttendancePeriodService.CloseAsync`/`BuildClosePreviewAsync` memblokir bila ada `HrdAttendanceException` `IsPayrollBlocking` berstatus `Open`/`UnderReview`, atau koreksi aktif |
| Attendance | Transition guard close/reopen/cancel | **PROVEN**, guard eksplisit per edge (`IsEditableStatus`, syarat `Closed` khusus untuk reopen) — bukan penulisan status membabi buta |
| Attendance | Koreksi `Applied` benar-benar terminal | **DISPROVEN.** `synchronize` dapat menurunkannya; lihat `HRD-Q-34` |
| Leave | Service yang mengubah saldo pada `OnLeaveStart` | **PROVEN** — `LeaveExecutionProcessorService.ExecuteAsync` → `ApplyDeductionStageAsync(..., OnLeaveStart)` |
| Leave | `OnCompletion` | **PROVEN** — method yang sama, dipanggil ulang dengan `BalanceStage.OnCompletion` |
| Leave | `CancellationRestore` | **PROVEN, tapi tidak selalu penuh** — prorata harian kalender bila pembatalan terjadi setelah tanggal mulai |
| Leave | Transition guard `Completed` | **DISPROVEN.** `/reverse` dapat membaliknya; lihat `HRD-Q-35` |
| Leave | `RecallStatus.Acknowledged` prasyarat sebelum `Approved` | **DISPROVEN.** Tidak pernah diperiksa oleh mesin workflow; lihat `HRD-Q-36` |
| Overtime | Guard `PostedToPayroll` | **PROVEN**, dengan catatan: memeriksa status Realisasi (`Verified` + verifikasi `Approved`), bukan status Permohonan secara langsung |
| Overtime | Mekanisme koreksi setelah posted | **DISPROVEN (lebih baik dari dugaan).** Ada `POST realizations/{id}/rollback` dan `Reconcile` dengan `AllowRepair`, setara `repair`/`rollback` kehadiran — tidak selalu perlu membuka kembali periode penuh |
| Overtime | Transition guard period reopen | **PROVEN** — hanya dari `Closed`/`Closing`, dijaga permission generik `AccessPermission("OvertimePeriod","Reopen")` |
| Overtime | Overtime overlap benar-benar ditolak | **PROVEN** — `HasRequestOverlapAsync` menandai `REQUEST_OVERLAP` sebagai isu pemblokir; `SubmitAsync` menolak 409 |

### 20.6 Otoritas aktor per jenis transaksi

Larangan kalimat umum "atasan menyetujui seluruh pengajuan anak buah" dijaga — setiap baris di
bawah adalah evidence per jenis transaksi, bukan generalisasi lintas transaksi.

| Jenis transaksi | Evidence otoritas |
| --- | --- |
| Leave | **Matriks dapat dikonfigurasi**, bukan hardcode. `WorkflowService.ResolveApproversAsync` mendukung sumber `RequesterManager`, `ManagerLevel`, `SpecificUser`, `Position`, `OrganizationUnit`, `Role*`, `ApprovalMatrix`, `RequesterSelected` dari `MstWorkflowStep`/`MstApprovalMatrix`. Gate nyata: `assignment.AssignedApproverUserId == actorContext.UserId` |
| Overtime | Peran `Supervisor`/`Manager`/`HrAdmin`/`Payroll` **terbukti tidak dipetakan** ke pemeriksaan identitas apa pun — hanya nilai default field. Penegakan nyata: `[AccessPermission]` generik per aksi, terputus dari kosakata peran. `HRD-Q-33` bergeser dari "bagaimana pemetaannya" menjadi "peta ini belum dibangun" |
| Attendance correction | Mesin workflow generik. `ApprovalInboxController.Approve` → `WorkflowService.ApproveAsync`, gate `assignment.AssignedApproverUserId == actorContext.UserId` — approver ditentukan `TrxWorkflowApproverAssignment`, bukan role hardcode |
| Salary assignment | **`[OPEN]`, tidak terbukti.** Tidak ada jalur persetujuan yang ditemukan untuk `WfpSalaryAssignment`. Dicatat `HRD-Q-19`, bukan diasumsikan tidak perlu persetujuan |

### 20.7 Yang dipertahankan tanpa perubahan

Sesuai batasan pengguna, tidak ada satu pun dari berikut yang disentuh: raw attendance sebagai
fakta immutable (`HrdAttendanceRawLog` tidak pernah ditulis ulang oleh koreksi apa pun — tetap
`[EXISTING]`, tidak terpengaruh temuan `Applied`/`synchronize`, karena celah itu memutasi
`HrdAttendanceDaily`, bukan rekaman mentah), frontend tidak menghitung kelayakan kehadiran,
backend sebagai otoritas saldo cuti, dokter di luar jadwal tidak otomatis menjadi lembur
(`HRD-DEC-013`), lima tahap lembur tetap terpisah, `Wfp`/`Mst` tidak diratchet, `Trx` HR hanya
diratchet saat materially touched, dan batas payroll `HRD-DEC-009`.

### 20.8 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `flows/01-employee-administration.md` | Provenance cross-employee dipisah `[DECISION] HRD-DEC-012` vs `[EXISTING] MISSING/REPAIR`; state transition `EmployeeProfileChange` ditulis ulang dengan kosakata benar; `HRD-Q-21` ditutup |
| `flows/02-attendance.md` | Guard close/reopen/cancel period diberi evidence per-edge; koreksi `Applied` dikoreksi jadi DISPROVEN + `HRD-Q-34`; `ScheduleMismatch` dikoreksi maknanya; `HRD-Q-22`/`Q-23`/`Q-24` diperbarui; AC-F02-05 dikonfirmasi PROVEN, AC-F02-09 baru ditambahkan |
| `flows/03-leave.md` | `OnLeaveStart`/`OnCompletion`/`CancellationRestore` diberi evidence; `Completed` dikoreksi jadi DISPROVEN + `HRD-Q-35`; `Acknowledged` dikoreksi jadi bukan gate + `HRD-Q-36`; `AC-F03-04`/`AC-F03-06` direvisi; diagram diperbarui |
| `flows/04-overtime.md` | `PostedToPayroll` guard diberi evidence dengan catatan; klaim "koreksi hanya lewat reopen" dikoreksi (ada rollback); period reopen guard dan overlap rejection dikonfirmasi PROVEN; peran workflow dikoreksi jadi terputus dari permission; `HRD-Q-32`/`Q-33` diperbarui |
| `00-interview-decisions.md` | Revision naik ke `6`; bagian 20 ini ditambahkan |

Tidak ada file source, migration, entity, controller, maupun frontend yang diubah pada pass ini.

---

## 21. PHASE 2A.2 — Owner Decision Closure, 27 Agustus 2026

Pengguna menyetujui hasil `PHASE 2A.1` dan menutup empat pertanyaan yang lahir darinya lewat
keputusan eksplisit. **Tidak ada source code yang diubah pada pass ini** — keempatnya adalah
**target business behavior**, terpisah dari **current implementation** yang sudah dibuktikan
`PHASE 2A.1`. Di mana keduanya berbeda, perbedaan itu dicatat tegas sebagai `IMPLEMENTATION
DEFECT / REPAIR`, bukan dirapikan diam-diam.

### 21.1 `HRD-DEC-022` — Attendance Correction: `Applied` terminal terhadap normal workflow synchronization

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-022` | Decision | **`AttendanceCorrection.RequestStatus = Applied` adalah terminal terhadap normal workflow synchronization.** `synchronize` **tidak boleh** menurunkan `Applied` kembali ke `Approved`, `PartiallyApproved`, atau status sebelumnya mana pun. Bila kesalahan ditemukan setelah `Applied`, jalur yang sah hanya: (a) permohonan koreksi baru, atau (b) aksi repair/koreksi eksplisit yang terotorisasi dan diaudit tersendiri. **Dilarang** menghidupkan kembali status permohonan lama | Pengguna | `approved` | Pengguna, 27 Agustus 2026 | `HRD-Q-34`; `flows/02-attendance.md` bagian 9.4 |

**Menutup `HRD-Q-34`.**

**Konsekuensi terhadap current implementation — ditandai `IMPLEMENTATION DEFECT / REPAIR`, bukan
target business behavior:**

| Current behavior | Status |
| --- | --- |
| `synchronize` dapat menurunkan `Applied` → `Approved` lalu memicu apply ulang (`HrdAttendanceDaily` termutasi ulang) | **`IMPLEMENTATION DEFECT`** — bertentangan dengan `HRD-DEC-022`, perlu `REPAIR` |
| Tidak ada aksi "repair/koreksi eksplisit yang terotorisasi dan diaudit" khusus untuk `AttendanceCorrection` selain permohonan baru | **`MISSING`** terhadap target — `HRD-DEC-022` butir (b) belum punya implementasi. (Catatan: `repair`/`rollback` yang ada hari ini adalah milik `payroll-handoff`, domain berbeda, bukan milik `AttendanceCorrection` itu sendiri) |
| Permohonan koreksi baru terhadap `AttendanceDailyId` yang sama setelah `Applied` | Sudah sesuai target — `[EXISTING]`, tidak perlu `REPAIR` |

### 21.2 `HRD-DEC-023` — Leave Completed: business-final dengan controlled reversal yang terkendali

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-023` | Decision | **Cuti `Completed` adalah business-final state untuk operasi normal**, tetapi *controlled reversal* tetap kemampuan yang sah. `Reverse` wajib memiliki: permission khusus; alasan (`reason`) wajib; pelaku dan waktu (`actor`/`timestamp`); rekonsiliasi kehadiran; pembalikan/perhitungan ulang saldo; dan guard periode payroll locked/finalized. **Bila payroll sudah locked/finalized, histori `Completed` tidak boleh dimutasi langsung** — gunakan transaksi adjustment/revision terpisah, bukan menulis ulang eksekusi lama | Pengguna | `approved` | Pengguna, 27 Agustus 2026 | `HRD-Q-35`; `flows/03-leave.md` bagian 9.1 |

**Menutup `HRD-Q-35`.**

**Konsekuensi terhadap current implementation — ditandai `IMPLEMENTATION DEFECT / REPAIR`:**

| Current behavior | Status |
| --- | --- |
| `POST /{leaveRequestId}/reverse` tanpa permission khusus yang terbukti, tanpa mewajibkan `reason`, dan tanpa guard periode payroll locked/finalized | **`IMPLEMENTATION DEFECT`** — perlu `REPAIR` agar mengikuti enam syarat `HRD-DEC-023` |
| Rekonsiliasi kehadiran dan pembalikan saldo saat reverse | Sebagian `[EXISTING]` (`ReverseAsync`/`RestoreAsync` memang memutasi saldo dan status eksekusi) — belum diverifikasi apakah rekonsiliasi kehadiran ikut dijalankan otomatis atau perlu langkah manual terpisah |
| Guard "payroll locked/finalized mencegah mutasi langsung, wajib pakai adjustment/revision" | **`MISSING`** — tidak ditemukan pada audit `PHASE 2A.1` |

### 21.3 `HRD-DEC-024` — Recall Acknowledgement: notification-then-acknowledge, bukan gate persetujuan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-024` | Decision | **`Acknowledged` bukan prerequisite untuk `Approved`.** Persetujuan pemanggilan kembali adalah keputusan organisasi, bukan keputusan pegawai. Target flow: `WaitingApproval` → `Approved` → notifikasi dikirim → `Acknowledged` → `Applied`. `Acknowledged` adalah bukti pegawai menerima pemberitahuan, bukan syarat sebelum organisasi memutuskan. Dalam kondisi operasional tertentu, **HR Manager dapat melakukan acknowledgement override** sebelum `Applied`, dengan `reason` wajib, `actor`/`timestamp` wajib, dan jejak audit wajib. **Pegawai tidak boleh memblokir keputusan recall selamanya hanya dengan tidak melakukan acknowledge** | Pengguna | `approved` | Pengguna, 27 Agustus 2026 | `HRD-Q-36`; `flows/03-leave.md` bagian 9.3 |

**Menutup `HRD-Q-36`.**

**Konsekuensi terhadap current implementation:**

| Current behavior | Status |
| --- | --- |
| `LeaveRecallWorkflowLifecycleService.MapStatus` memetakan `WaitingApproval` → `Approved` langsung, tanpa pernah melalui `Acknowledged` | **Sudah sejalan dengan target** — `Acknowledged` memang tidak dimaksudkan menjadi gate. `[EXISTING]`, tidak perlu `REPAIR` pada urutan ini |
| Notifikasi otomatis terkirim ke pegawai setelah `Approved`, sebelum `Applied` | **`[OPEN]`/`UNVERIFIED`** — belum diaudit apakah ada mekanisme notifikasi otomatis pada titik ini |
| Mekanisme "HR Manager acknowledgement override" dengan `reason`/`actor`/`timestamp`/audit trail wajib | **`MISSING`** terhadap target — `AcknowledgeReturnToWorkAsync` yang ada hari ini adalah aksi pegawai sendiri, bukan override HR Manager dengan syarat wajib tersebut |

### 21.4 `HRD-DEC-025` — Exception dokter di luar jadwal: `OutOfScheduleWork` terpisah dari `ScheduleMismatch`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-025` | Decision | **`ScheduleMismatch` tidak diperluas maknanya.** Tetap bermakna *schedule unresolved/conflict* seperti pada source hari ini (`SCHEDULE_UNRESOLVED`). Target desain memerlukan **exception type baru dan terpisah** untuk aktivitas kerja nyata di luar jadwal yang valid — contoh nama `OutOfScheduleWork` (nama final menyesuaikan konvensi enum existing, tetapi semantiknya wajib terpisah dari `ScheduleMismatch`). Alur target: `OutOfScheduleWork` → `pending classification` → manager/reviewer terotorisasi menentukan salah satu dari: lembur, koreksi jadwal, tercatat/non-compensable, atau klasifikasi resmi lain. **Tidak pernah otomatis menjadi lembur**, sejalan `HRD-DEC-013` | Pengguna | `approved` | Pengguna, 27 Agustus 2026 | Sisa `HRD-Q-22`; `flows/02-attendance.md` bagian 6.1 |

**Menutup sisa `HRD-Q-22`** (bagian source-resolvable sudah ditutup `PHASE 2A.1`; bagian keputusan
desain ditutup di sini).

**Konsekuensi terhadap current implementation:**

| Current behavior | Status |
| --- | --- |
| Nilai `AttendanceExceptionType.OutOfScheduleWork` (atau setara) | **`MISSING`** — belum ada di enum `AttendanceValueConstants.AttendanceExceptionType` |
| Kode yang mendeteksi "aktivitas kerja nyata di luar jadwal yang valid" dan menandainya dengan tipe baru itu | **`MISSING`** — tidak ada jalur kode untuk skenario ini hari ini |
| Alur "pending classification → manager/reviewer menentukan klasifikasi akhir" | **`[DECISION]`** `HRD-DEC-013` sudah menetapkan **siapa** yang memutuskan (atasan) dan **larangan** otomatisasi; `HRD-DEC-025` menetapkan **wadah teknisnya** (exception type terpisah). Implementasinya sendiri `MISSING` |

### 21.5 Ringkasan penutupan

| ID | Ditutup oleh | Status akhir |
| --- | --- | --- |
| `HRD-Q-22` (sisa) | `HRD-DEC-025` | `resolved` |
| `HRD-Q-34` | `HRD-DEC-022` | `resolved` |
| `HRD-Q-35` | `HRD-DEC-023` | `resolved` |
| `HRD-Q-36` | `HRD-DEC-024` | `resolved` |

Keempat keputusan ini **target business behavior**. Selisih antara target dan current
implementation dicatat sebagai `IMPLEMENTATION DEFECT`, `REPAIR`, atau `MISSING` pada tabel
masing-masing di atas — bukan diselesaikan lewat perubahan source pada pass ini, sesuai batasan
pengguna. Perbaikan actual menjadi task implementasi terpisah di luar cakupan blueprint.

Tidak ada file source, migration, entity, controller, maupun frontend yang diubah pada pass ini.

---

## 22. PHASE 2B — Flow 05–09, 27 Agustus 2026

Lima flow baru ditulis: `05-work-scheduling.md`, `06-shift-change-swap.md`,
`07-attendance-correction.md`, `08-early-leave-permission.md`, `09-unified-approval.md`. Empat
sub-agent riset read-only dijalankan paralel, satu per domain backend. **Tidak ada source code
yang diubah.**

### 22.1 Temuan ringkas per flow

| Flow | Temuan utama |
| --- | --- |
| 05 — Penjadwalan kerja | Dari 11 model `SchedulingManagement`, **hanya 3 punya controller**. Roster, shift harian, penggantian, tenaga darurat, dan siaga seluruhnya `MISSING` di backend — bukan sekadar frontend |
| 06 — Ubah jadwal/tukar shift | Tukar shift terbukti **dua tahap terpisah** (persetujuan rekan lalu manajer) dengan `WorkflowDefinitionCode` berbeda dari ubah jadwal — larangan menyamakan keduanya terbukti benar |
| 07 — Koreksi kehadiran | Dikonfirmasi ulang: HR Admin **tidak dapat** membuat koreksi atas nama pegawai lain (guard `daily.WorkforceProfileId == actorWorkforceProfileId`); tidak ada aksi repair resmi untuk koreksi `Applied`, sejalan `HRD-DEC-022` |
| 08 — Izin pulang cepat | **Tidak ada kapabilitas berdiri sendiri.** Ditemukan mode `IsHourly` pada `WfpLeaveRequest` sebagai kandidat terdekat — tapi ditemukan pula kemungkinan kontradiksi terhadap flow 03 (`HRD-Q-44`) |
| 09 — Kotak masuk terpadu | Pemisahan per domain terbukti sampai lapisan data (`MstWorkflowStep`/`MstApprovalMatrix` di-scope per `WorkflowDefinitionId`). `HRD-Q-12`/`HRD-Q-13` tertutup. SLA/eskalasi ada sebagai konfigurasi tapi **tidak ada mesin penegakan** |

### 22.2 Open question baru (`HRD-Q-37` s.d. `HRD-Q-46`)

| ID | Flow | Isi singkat |
| --- | --- | --- |
| `HRD-Q-37` | 05 | Roster/shift-harian/darurat/siaga: prioritas `DEFERRED` atau `EXTEND` segera? |
| `HRD-Q-38` | 05 | Apakah penempatan jadwal kerja memerlukan persetujuan? |
| `HRD-Q-39` | 06 | Apakah tukar shift `Applied` otomatis memutakhirkan `ScheduleSource` kehadiran? |
| `HRD-Q-40` | 07 | Apakah HR Admin seharusnya dapat membuat koreksi atas nama pegawai? |
| `HRD-Q-41` | 08 | Potongan saldo mode `IsHourly`: proporsional atau satuan hari penuh? |
| `HRD-Q-42` | 08 | Perlu jalur izin pulang cepat tanpa potongan saldo, terpisah dari `IsHourly`? |
| `HRD-Q-43` | 08 | Apakah `IsHourly` resmi ditetapkan sebagai fitur "izin pulang cepat", atau perlu fitur baru? |
| `HRD-Q-44` | 08 (mengoreksi flow 03) | Rantai status granular pada `WfpLeaveRequest.cs` komentar vs `WaitingApproval` tunggal — mana yang berlaku nyata? |
| `HRD-Q-45` | 09 | SLA/eskalasi ada sebagai konfigurasi tanpa mesin penegakan — prioritas dibangun atau tidak? |
| `HRD-Q-46` | 09 | `TrxLeaveRequestApproval` tanpa `WorkflowInstanceId` — mekanisme paralel aktif atau kode mati? |

### 22.3 Open question tertutup pada pass ini

| ID | Jawaban | Evidence |
| --- | --- | --- |
| `HRD-Q-12` | Kotak masuk menampilkan **keduanya** — pending (`view=open`) dan riwayat (`view=completed`/`all`) | `flows/09-unified-approval.md` bagian 8 |
| `HRD-Q-13` | Delegasi diaktifkan **oleh approver itu sendiri**; mekanismenya mutasi kolom `AssignedApproverUserId`, bukan percabangan kode approval | `flows/09-unified-approval.md` bagian 8 |

### 22.4 Kontradiksi terhadap flow 00–04

Satu kontradiksi ditemukan, **belum direkonsiliasi**: `WfpLeaveRequest.cs` baris 89–92 menunjukkan
rantai status `WaitingSupervisorApproval → WaitingManagerApproval → WaitingHrVerification`,
berbeda dari `WaitingApproval` tunggal yang didokumentasikan flow 03 (dan diverifikasi
`PHASE 2A.1` terhadap `LeaveRequestValueConstants.Status`). Flow 03 **tidak diubah** temuan
utamanya — hanya diberi catatan lanjutan yang merujuk `HRD-Q-44`, karena belum ada audit yang
membandingkan langsung komentar model ini dengan konstanta yang benar-benar dirujuk mesin
approval matrix.

### 22.5 Implementation defect yang ditemukan, belum diperbaiki

Tidak ada temuan defect baru sekelas `HRD-DEC-022`/`023`/`024` pada pass ini. Yang ditemukan
adalah kesenjangan **cakupan** (backend `MISSING`, bukan cacat pada backend yang ada):

| Kesenjangan | Domain | Status |
| --- | --- | --- |
| Roster, shift harian, penggantian shift, tenaga darurat, siaga — tanpa controller | Penjadwalan (flow 05) | `MISSING`, `HRD-Q-37` |
| Aksi repair/koreksi eksplisit untuk `AttendanceCorrection` pasca-`Applied` | Koreksi kehadiran (flow 07) | `MISSING`, sudah tercatat `HRD-DEC-022` bagian 21.1 |
| Mesin penegakan SLA/eskalasi/auto-approve/auto-reject | Kotak masuk (flow 09) | `MISSING`, `HRD-Q-45` |

### 22.6 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `flows/05-work-scheduling.md` | Baru |
| `flows/06-shift-change-swap.md` | Baru |
| `flows/07-attendance-correction.md` | Baru |
| `flows/08-early-leave-permission.md` | Baru |
| `flows/09-unified-approval.md` | Baru |
| `flows/03-leave.md` | Catatan lanjutan `HRD-Q-44` ditambahkan pada bagian 9.1, tanpa mengubah kesimpulan `PHASE 2A.1` |
| `flows/README.md` | Status flow 05–09 diperbarui menjadi `Ada`; flow 11–14 diberi label `PHASE 2C` |
| `00-interview-decisions.md` | Bagian 22 ini ditambahkan |

Tidak ada file source, migration, entity, controller, maupun frontend yang diubah pada pass ini.

---

## 23. PHASE 2B.1 — Source Closure & Product Decision Pass, 28 Agustus 2026

Lima sub-agent riset read-only dijalankan paralel untuk menutup pertanyaan source-resolvable
yang tersisa dari `PHASE 2B`, dan pengguna mencatat lima keputusan produk baru. **Tidak ada
source code, frontend, database, migration, maupun registry yang diubah.**

### 23.1 Hasil `HRD-Q-39` — integrasi tukar shift ke kehadiran

**Tertutup. Jawabannya YA, terbukti dari jalur tulis-lalu-baca yang nyata, bukan dari nama
status.**

`ShiftSwapService.ApplyAsync` (baris 661–752) **tidak** sekadar mengubah status. Ia memuat kedua
baris `TrxShiftAssignment` (pemohon dan target), lalu saling menukar `ShiftDate`, `ShiftId`,
`WorkScheduleId`, `ScheduledStartAt/EndAt`, `PlannedWorkMinutes` lewat `ShiftAssignmentPayload.
ApplyTo` (baris 1083–1110), menandai kedua baris `AssignmentSource = "ShiftSwap"` dan
`IsManualOverride = true`, baru kemudian men-set `WfpShiftSwapRequest.RequestStatus = Applied`
dalam transaksi database yang sama. `TrxShiftAssignment` memang tidak punya controller (temuan
`PHASE 2B`), tetapi **ditulis langsung lewat `ApplicationDbContext` di lapisan service** — bukan
berarti tidak tertulis.

`AttendanceScheduleResolverService.ResolveCoreAsync` (baris 182–226) membaca `TrxShiftAssignment`
lebih dulu (baris 196–208), difilter hanya `WorkforceProfileId`/`ShiftDate`/`IsActive`/status
aktif — **tanpa** pengecualian untuk baris bersumber `ShiftSwap`. Saat baris hasil swap
ditemukan, `MapRosterResolution` (baris 286–328) mengembalikan `ScheduleSource.ManualOverride`
justru karena `IsManualOverride` yang diset `ApplyAsync` — bukti langsung bahwa resolver
memungut hasil swap. `AttendanceProcessingService` memanggil resolver ini pada baris 640.

**Kesimpulan:** tukar shift yang `Applied` benar-benar mengubah apa yang dihitung Attendance
Processing pada hari yang ditukar. Flow 06 diperbarui untuk mencerminkan ini sebagai `[EXISTING]`
terbukti, bukan `[OPEN]`.

### 23.2 Hasil `HRD-Q-41` — matematika saldo cuti per jam

**Tertutup. Formula proporsional per menit, tersimpan sebagai pecahan hari.**

`LeaveRequestCalculationService.CalculateDays` (baris 547–561): `planned = PlannedWorkMinutes`
hasil resolusi jadwal hari itu, **fallback hardcode 480 menit** bila jadwal tidak terselesaikan
(baris 555 — bukan dari master data mana pun). `CountedDays = Math.Round(RequestedMinutes /
(decimal)planned, 4, AwayFromZero)` (baris 560). Nilai ini mengalir sebagai `RequestedDays`/
`EstimatedBalanceDeduction` (baris 586–587), diteruskan `ApplyDeductionStageAsync` sebagai
`desiredUsage` (baris 129 `LeaveExecutionBalanceService.cs`). **Unit yang tersimpan di buku
besar (`TrxLeaveBalanceTransaction`, `WfpLeaveBalance`) adalah pecahan HARI, bukan menit/jam** —
konversi menit→hari terjadi satu kali di titik kalkulasi, tidak pernah direpresentasikan ulang
sebagai jam/menit di sisi saldo.

**Catatan tambahan yang perlu diperhatikan:** fallback 480 menit adalah **konstanta hardcode**,
bukan nilai kebijakan yang dapat dikonfigurasi per rumah sakit. Ini bukan pertanyaan kebijakan
baru — dicatat sebagai catatan teknis pada flow 08, bukan `HRD-Q` baru, karena tidak memblokir
alur, hanya perlu diketahui pemilik teknis.

### 23.3 Hasil `HRD-Q-44` — rekonsiliasi rantai status cuti

**Tertutup. Verdict (b) dengan kualifikasi: rantai granular nyata HANYA sebagai step-order di
mesin workflow generik, TIDAK PERNAH sebagai status bernama.**

Pencarian literal `WaitingSupervisorApproval`/`WaitingManagerApproval`/`WaitingHrVerification`
di seluruh source: **nol hasil eksekutabel.** Satu-satunya kemunculan adalah komentar pada
`WfpLeaveRequest.cs` baris 90–91 — dan komentar **identik** juga ditemukan pada
`TrxExpenseClaim.cs` baris 95–96, entity yang sama sekali tidak berhubungan. Ini membuktikan
komentar itu adalah **template yang disalin**, bukan catatan desain yang disengaja untuk cuti.

`WfpLeaveRequest.LeaveRequestStatus` adalah `string` polos, dan setiap jalur tulis yang aktif
memakai konstanta `LeaveRequestValueConstants.Status.*` — tidak ada satu pun yang menulis string
bergaya "WaitingSupervisorApproval". `LeaveRequestWorkflowLifecycleService.MapStatus` (baris
143–197) memetakan **seluruh** status workflow non-terminal ke **satu** nilai `WaitingApproval`
(baris 197, cabang fallback).

Namun, di lapisan mesin workflow generik, granularitas itu **nyata secara struktural**:
`MstWorkflowStep.StepOrder`, `ApprovalMode = Sequential`, `ApproverSourceType` (`ManagerLevel`,
`Position`, `SiteHr`, `CorporateHr`, dst.), dan `StepType.Verification` mendukung rantai
bertingkat — dilacak lewat `WfpLeaveRequest.CurrentApprovalStep`/`TrxWorkflowInstance.
CurrentStepOrder`. Tidak ditemukan seed data `MstWorkflowStep` untuk `LEAVE_REQUEST` di repo,
jadi rantai tiga tingkat ini **mungkin** dikonfigurasi sebagai master data saat implementasi,
tetapi **tidak dijamin ada** hari ini.

**Konsekuensi untuk Flow 03 dan Flow 08:** klaim `WaitingApproval` tunggal pada flow 03 bagian
9.1 **tetap benar dan final** di lapisan status domain `WfpLeaveRequest`. Catatan lanjutan yang
ditambahkan `PHASE 2B` diperbarui — bukan lagi "belum direkonsiliasi", melainkan **tertutup**:
komentar granular adalah artefak template, bukan implementasi. Detail step-order yang nyata di
mesin workflow dicatat sebagai lapisan terpisah, tidak menggantikan tabel status domain.

### 23.4 Hasil `HRD-Q-46` — klasifikasi `TrxLeaveRequestApproval`

**Tertutup. Klasifikasi: `LEGACY_UNUSED`.**

Seluruh rujukan hanya pada lapisan skema: `ApplicationDbContext.cs:333` (`DbSet` saja),
`TrxLeaveRequestApprovalConfiguration.cs` (konfigurasi EF saja), migration
`20260726161839_initializeBigModulHRD2.cs` baris 42036–42086 (`CreateTable` saja, **tanpa**
`InsertData`/backfill), dan `WfpLeaveRequest.Approvals` (navigasi yang tidak pernah dipakai).
**Tidak ada** controller, service, atau repository yang membaca atau menulis baris ke tabel ini.
Alur persetujuan cuti yang aktif seluruhnya lewat `TrxWorkflowInstance`/`TrxApprovalAction`/
`TrxWorkflowApproverAssignment` — mesin generik yang sudah dibuktikan `PHASE 2A.1`.

**Kesimpulan:** tabel ini sisa dari mekanisme persetujuan khusus-cuti sebelum mesin workflow
generik ada, dan tidak pernah dihapus. Tidak boleh dipakai sebagai dasar desain apa pun.

### 23.5 Hasil audit `AC-F07-02` — perilaku unggah bukti kedua

**Tertutup. Klasifikasi: DELETE-OLD-THEN-REPLACE (secara fungsional), tanpa file yatim.**

`AttendanceCorrectionService.UploadEvidenceAsync`: tidak ada guard `if (EvidenceFilePath !=
null)` — unggahan kedua diproses identik dengan yang pertama. Urutan nyata (baris 957–976):
simpan berkas baru ke storage → timpa field DB (`EvidenceFilePath`/`FileName`/`ContentType`) →
`SaveChangesAsync` → **baru kemudian** `_fileStorageService.DeletePhysicalFileAsync(oldPath, ...)`
menghapus berkas fisik lama. `DeletePhysicalFileAsync` aman dipanggil dengan path kosong (kasus
unggahan pertama). Setiap simpanan memakai nama GUID baru, sehingga tidak ada risiko tabrakan
selama jeda sebelum penghapusan. **Tidak ada endpoint hapus yang wajib dipanggil lebih dulu** —
`DeleteEvidenceAsync` ada tapi opsional, bukan prasyarat.

### 23.6 Koreksi Flow 08 — `RequestedEarlyLeaveAt` vs `ActualCheckOutAt` vs `ApprovedAt`

**Klaim lama pada flow 08 bagian 5 poin 2 DICABUT.** Klaim itu menyatakan "`StartTime` yang
diajukan pegawai tersimpan pada saat pengajuan, terpisah dari waktu keputusan" sebagai bukti
"waktu efektif pulang cepat adalah waktu yang diajukan". Audit `PHASE 2B.1` membuktikan klaim itu
**tidak lengkap dan menyesatkan**:

1. `WfpLeaveRequest.StartTime`/`EndTime` memang tersimpan saat pengajuan — **tetapi nilai ini
   tidak pernah mengalir ke `TrxLeaveAttendanceIntegration`**, yang hanya membawa
   `RequestedMinutes` (`LeaveExecutionProcessorService.cs` baris 703). `StartTime`/`EndTime`
   "mati" di entity permohonan, tidak dipakai sisi kehadiran manapun. Pencarian
   `LeaveRequest|WfpLeaveRequest|IsHourly|StartTime|EndTime` di seluruh
   `AttendanceProcessingService.cs` menghasilkan **nol** kecocokan.
2. **Cuti per jam (`IsHourly`) dan pengecualian `EarlyLeave` pada kehadiran adalah dua mekanisme
   yang TERPUTUS.** `LeaveExecutionProcessorService.ApplyAttendanceAsync` baris 881:
   `fullDay = RequestedLeaveDays >= 0.999m && !IsHourly` — **untuk cuti per jam, `fullDay` SELALU
   `false`**. Blok waiver yang mereset `IsEarlyLeave`/`EarlyLeaveMinutes` dan menutup pengecualian
   (baris 969–991) **hanya berjalan di cabang `fullDay`**. Menyetujui/menjalankan cuti per jam
   **tidak** memengaruhi pengecualian `EarlyLeave` yang dihitung independen dari rekaman mentah.
   (Cuti **penuh sehari** memang mewaiver `EarlyLeave` — tapi itu di luar skenario izin pulang
   cepat.)
3. **Tidak ada field bernama `RequestedEarlyLeaveAt`/`ActualCheckOutAt` untuk cuti.** Field
   `ActualCheckOutAt` memang ada di source, tapi milik `HrdMissingAttendance` — domain koreksi
   kehadiran hilang, sama sekali tidak berhubungan dengan cuti. Kerangka tiga waktu
   (`RequestedEarlyLeaveAt`/`ActualCheckOutAt`/`ApprovedAt`) yang diminta pengguna pada bagian B
   surat ini **murni konseptual/target**, belum tercermin pada satu pun nama field yang ada.

Flow 08 ditulis ulang mengikuti temuan ini — lihat bagian 23.9.

### 23.7 Keputusan produk baru — `HRD-DEC-026` s.d. `HRD-DEC-030`

#### `HRD-DEC-026` — Roster dan operational scheduling: target `EXTEND`, menutup `HRD-Q-37`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-026` | Decision | **Untuk rumah sakit 24/7, kapabilitas berikut adalah bagian target HR V2, bukan `DEFERRED`:** roster period; roster assignment/publication; daily shift assignment; shift replacement; emergency staffing; actual on-call assignment. Current state = `MISSING API` (dikonfirmasi `PHASE 2B`: 8 dari 11 model `SchedulingManagement` tanpa controller). **Target implementation classification = `EXTEND` terhadap schema existing** (`TrxRosterPeriod`, `TrxRosterAssignment`, `TrxRosterPublication`, `TrxRosterApproval`, `TrxShiftAssignment`, `TrxShiftReplacement`, `TrxEmergencyStaffingRequest`, `TrxOnCallAssignment` sudah model+EF+migration). **Larangan:** jangan membuat schema baru sebelum audit model existing, dan `HRD-Q-05` wajib terjawab lebih dulu bila perubahan destruktif diperlukan | Pengguna | `approved` | Pengguna, 28 Agustus 2026 | `HRD-Q-37`; `flows/05-work-scheduling.md` |

**Menutup `HRD-Q-37`.**

#### `HRD-DEC-027` — Work schedule assignment approval: rule-based, menutup `HRD-Q-38`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-027` | Decision | **Penempatan jadwal kerja current/future oleh HR berwenang pada periode yang masih editable TIDAK membutuhkan approval tambahan; audit trail tetap wajib.** Perubahan retroactive, atau perubahan yang menyentuh periode kehadiran/payroll yang sudah diproses/locked, **wajib** melalui controlled correction/approval — tidak boleh direct edit. **Larangan:** jangan membuat approval untuk setiap edit kecil, karena akan membebani pekerjaan HR administratif sehari-hari | Pengguna | `approved` | Pengguna, 28 Agustus 2026 | `HRD-Q-38`; `flows/05-work-scheduling.md` bagian 8 |

**Menutup `HRD-Q-38`.**

**Konsekuensi terhadap current implementation:** `WfpWorkScheduleAssignmentController.Create`/
`Update`/`PATCH status` hari ini adalah aksi langsung HR Admin tanpa pemeriksaan retroactive atau
periode locked — sejalan dengan bagian pertama keputusan (penempatan current/future memang tidak
perlu approval). **`MISSING`** terhadap bagian kedua: tidak ada guard yang mendeteksi perubahan
retroactive atau periode locked dan mengarahkannya ke controlled correction.

#### `HRD-DEC-028` — Attendance correction on-behalf oleh HR Admin, menutup `HRD-Q-40`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-028` | Decision | **HR Admin boleh membuat permohonan koreksi kehadiran atas nama pegawai, bila pegawai tidak dapat mengakses ESS.** Wajib menyimpan: initiator HR; workforce yang diwakili; alasan (`reason`); waktu (`timestamp`); bukti (`evidence`) bila policy membutuhkan; notifikasi kepada pegawai; dan jejak audit lengkap. **Rekaman mentah kehadiran tetap immutable** — ketentuan ini tidak mengubah invariant flow 02. Persetujuan setelah pengajuan tetap memakai workflow/policy transaksi koreksi yang berlaku — **tidak ada jalur approval baru** khusus untuk permohonan on-behalf | Pengguna | `approved` | Pengguna, 28 Agustus 2026 | `HRD-Q-40`; `flows/07-attendance-correction.md` bagian 2 |

**Menutup `HRD-Q-40`.**

**Konsekuensi terhadap current implementation:** **`MISSING`** sepenuhnya. `AttendanceCorrectionService.CreateAsync` baris 268 mensyaratkan `daily.WorkforceProfileId == actorWorkforceProfileId` — tidak ada jalur on-behalf sama sekali hari ini. Ini target baru, bukan repair atas cacat lama.

#### `HRD-DEC-029` — Early Leave Permission terpisah dari Hourly Leave, menutup `HRD-Q-42` dan `HRD-Q-43`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-029` | Decision | **`WfpLeaveRequest.IsHourly` TIDAK sama dengan fitur Izin Pulang Cepat.** Dua konsep ditetapkan: **Hourly Leave** = bagian Leave Management, memakai entitlement/saldo cuti sesuai policy (mekanisme yang sudah terverifikasi `HRD-Q-41`). **Early Leave Permission** = izin administratif meninggalkan pekerjaan sebelum jadwal selesai, menjadi bagian alur attendance/permission — **bukan** bagian Leave Management. Keduanya boleh memakai ulang infrastruktur workflow (mesin approval generik, flow 09) tetapi **bukan business transaction yang sama** — dilarang disatukan entity maupun state machine-nya. Early Leave Permission boleh memiliki policy `deductible`/`non-deductible`/dikonversi ke hourly leave, **tetapi nilai policy itu tidak boleh di-hardcode** sebelum pemilik produk menentukannya. **Invariant yang mengikat:** waktu approval tidak pernah menjadi actual checkout time; actual attendance tetap berasal dari raw attendance; waktu yang diminta/diizinkan (requested/authorized early-leave time) disimpan terpisah sebagai dasar penilaian exception | Pengguna | `approved` | Pengguna, 28 Agustus 2026 | `HRD-Q-42`, `HRD-Q-43`; `flows/08-early-leave-permission.md` |

**Menutup `HRD-Q-42` dan `HRD-Q-43`.**

**Konsekuensi terhadap current implementation:** Early Leave Permission sebagai kapabilitas
berdiri sendiri = **`MISSING`**. **Tidak ada entity yang dibuat pada pass ini**, sesuai batasan
pengguna — ini keputusan arsitektur target, menunggu task desain/implementasi terpisah. Nilai
policy (`deductible`/`non-deductible`/konversi) tetap `[OPEN]`, dicatat `HRD-Q-47`.

#### `HRD-DEC-030` — SLA/Escalation: target `EXTEND`, default OFF untuk auto-approve/auto-reject, menutup `HRD-Q-45`

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `HRD-DEC-030` | Decision | **Reminder dan escalation engine adalah target `EXTEND`.** `DueAt`, `ReminderAfterHours`, dan `EscalationAfterHours` **harus benar-benar dieksekusi** oleh scheduled processing — bukan sekadar field konfigurasi tanpa penegakan seperti kondisi hari ini. `AutoApproveAfterHours` dan `AutoRejectAfterHours`: **default OFF**; hanya boleh aktif bila `WorkflowDefinitionId` transaksi secara eksplisit mengizinkannya (opt-in per definisi workflow); **dilarang** diberlakukan otomatis ke seluruh transaksi HR | Pengguna | `approved` | Pengguna, 28 Agustus 2026 | `HRD-Q-45`; `flows/09-unified-approval.md` bagian 7 |

**Menutup `HRD-Q-45`.**

**Konsekuensi terhadap current implementation:** **`MISSING`** sepenuhnya — `PHASE 2B` sudah
membuktikan tidak ada `BackgroundService`/`IHostedService` yang membaca `DueAt` atau keempat
field itu. Target `EXTEND` di sini berarti membangun mesin baru, bukan memperbaiki mesin yang ada.

### 23.8 Koreksi Flow 05 — wording route "sudah diseragamkan"

**Klaim lama DICABUT.** Flow 05 bagian 4 sebelumnya menulis "delapan route sudah diseragamkan
kebab-case oleh `HRD-DEC-014`/`016`" — ini keliru, mencampur **keputusan target** dengan **bukti
implementasi**. Audit ulang `WorkCalendarController.cs:18`, `WorkScheduleController.cs:18`,
`ShiftPatternController.cs:18`, `ShiftGroupController.cs:18` membuktikan keempatnya **masih**
memakai `[Route]` lama (`workcalendars`, `workschedules`, `shiftpatterns`, `shiftgroups`) **tanpa**
route template kebab-case kedua. `HRD-DEC-016` tetap berlaku sebagai **canonical target = kebab-
case + compatibility alias**, tetapi belum diimplementasikan pada baseline `16b8b71`. Flow 05
sudah diperbarui dengan kutipan baris ini.

### 23.9 Roadmap impact — classification before → after

Hanya classification yang terdampak yang diperbarui; slice lain pada
`roadmap/00-slice-roadmap.md` tidak disentuh.

| Kapabilitas | Before | After | Evidence |
| --- | --- | --- | --- |
| `S-B4` Penjadwalan kerja — roster/shift-harian/darurat/siaga | Current State: "Rasio paling timpang... 22 endpoint pada 3 controller, model 11" (dibaca sebagai "backend tipis" tanpa rincian); Target State umum "penyusunan jadwal, penugasan shift, deteksi bentrok" | **Current = `MISSING API`** untuk 8 dari 11 model (roster period/assignment/publication/approval, shift harian, penggantian shift, tenaga darurat, siaga — nol controller, dibuktikan `PHASE 2B`). **Target = `EXTEND`** terhadap schema existing, ditetapkan `HRD-DEC-026`, bukan `DEFERRED`. Deskripsi "backend tipis" tidak lagi dipakai tanpa penjelasan operational roster core tanpa API | `HRD-DEC-026`; `flows/05-work-scheduling.md` |
| Koreksi kehadiran atas nama pegawai (HR-on-behalf) | Tidak disebutkan sebagai kapabilitas terpisah pada `S-B1`/`S-A5` | **`EXTEND`** — ditetapkan `HRD-DEC-028`. Current implementation `MISSING` (guard `actorWorkforceProfileId` memblokir on-behalf sepenuhnya) | `HRD-DEC-028`; `flows/07-attendance-correction.md` |
| Early Leave Permission | Tidak ada sebagai kapabilitas terpisah; sebelumnya berisiko dicampur dengan `S-A2` (layanan mandiri cuti, mode `IsHourly`) | **`NEW/EXTEND` sesuai hasil arsitektur nanti.** Ditetapkan sebagai konsep terpisah dari Hourly Leave oleh `HRD-DEC-029`. **Tidak ada entity dibuat pada pass ini** | `HRD-DEC-029`; `flows/08-early-leave-permission.md` |
| SLA/escalation executor (`S-A7`) | `S-A7` Target State menyebut kotak masuk terpadu tanpa menyebut mesin SLA/eskalasi terpisah | **`EXTEND`** — mesin reminder/escalation harus dieksekusi scheduled processing, ditetapkan `HRD-DEC-030`. Current `MISSING` sepenuhnya (tidak ada `BackgroundService` ditemukan) | `HRD-DEC-030`; `flows/09-unified-approval.md` |

### 23.10 Koreksi cakupan `PHASE 2C`

**`PHASE 2C` bukan hanya flow 11–14.** Cakupan yang benar:

| # | Berkas | Status target |
| --- | --- | --- |
| 10 | `10-payroll-processing-handoff.md` | **`PARTIAL`** — wajib tetap ditulis. Boleh mendesain HR calculation → reconciliation → execute → batas HR/Finance. **Sesudah batas itu tetap `[BLOCKED]`** oleh `HRD-Q-10` dan `HRD-Q-11`. **Dilarang** mengarang payload Finance atau perilaku penolakan batch |
| 11 | `11-lifecycle-offboarding.md` | `READY` untuk ditulis |
| 12 | `12-competency-training.md` | `READY` untuk ditulis |
| 13 | `13-performance-management.md` | `READY` untuk ditulis |
| 14 | `14-employee-relations-discipline.md` | `READY` untuk ditulis |

`flows/README.md` diperbarui agar tidak lagi menyiratkan flow 10 sebagai "sebagian `BLOCKED`"
tanpa kejelasan batasnya — diganti eksplisit `PARTIAL` dengan penjelasan batas HR/Finance.

### 23.11 Open question baru

| ID | Isi | Owner | Memblokir |
| --- | --- | --- | --- |
| `HRD-Q-47` | **Baru.** Nilai policy Early Leave Permission — `deductible`, `non-deductible`, atau dikonversi ke hourly leave — belum ditentukan pemilik produk | Pemilik produk | Desain final policy Early Leave Permission, bukan keberadaan kapabilitasnya |

### 23.12 Ringkasan penutupan

| ID | Ditutup oleh | Status akhir |
| --- | --- | --- |
| `HRD-Q-37` | `HRD-DEC-026` | `resolved` |
| `HRD-Q-38` | `HRD-DEC-027` | `resolved` |
| `HRD-Q-39` | Audit source `PHASE 2B.1` | `resolved` — integrasi terbukti |
| `HRD-Q-40` | `HRD-DEC-028` | `resolved` |
| `HRD-Q-41` | Audit source `PHASE 2B.1` | `resolved` — formula proporsional terbukti |
| `HRD-Q-42` | `HRD-DEC-029` | `resolved` |
| `HRD-Q-43` | `HRD-DEC-029` | `resolved` |
| `HRD-Q-44` | Audit source `PHASE 2B.1` | `resolved` — komentar adalah artefak template, bukan implementasi |
| `HRD-Q-45` | `HRD-DEC-030` | `resolved` |
| `HRD-Q-46` | Audit source `PHASE 2B.1` | `resolved` — `LEGACY_UNUSED` |
| `AC-F07-02` | Audit source `PHASE 2B.1` | `resolved` — DELETE-OLD-THEN-REPLACE |

### 23.13 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `flows/03-leave.md` | Catatan lanjutan `HRD-Q-44` diperbarui dari "belum direkonsiliasi" menjadi tertutup; rujukan `TrxLeaveRequestApproval = LEGACY_UNUSED` ditambahkan |
| `flows/05-work-scheduling.md` | Wording route "sudah diseragamkan" dicabut dan dikoreksi dengan kutipan baris; `HRD-Q-37` ditutup `HRD-DEC-026` |
| `flows/06-shift-change-swap.md` | `HRD-Q-39` ditutup — integrasi tukar shift ke `ScheduleSource` kehadiran dinyatakan terbukti dengan evidence lengkap |
| `flows/07-attendance-correction.md` | `AC-F07-02` ditutup; `HRD-Q-40` ditutup `HRD-DEC-028` dengan tabel target vs current |
| `flows/08-early-leave-permission.md` | Ditulis ulang signifikan: klaim `StartTime` dicabut, formula `HRD-Q-41` ditambahkan, temuan keterputusan `IsHourly`/`EarlyLeave` ditambahkan, `HRD-DEC-029` diterapkan |
| `roadmap/00-slice-roadmap.md` | `S-B4` classification diperbarui; catatan HR-on-behalf, Early Leave Permission, dan SLA/escalation executor ditambahkan pada slice terkait |
| `flows/README.md` | Flow 10 diberi status target `PARTIAL` eksplisit dengan batas HR/Finance |
| `00-interview-decisions.md` | Revisi naik ke `9`; bagian 23 ini ditambahkan |

Tidak ada file source, migration, entity, controller, database, maupun frontend yang diubah pada
pass ini.

---

## 24. Catatan susulan sebelum PHASE 2C, 28 Agustus 2026

Dua koreksi pengguna atas hasil `PHASE 2B.1`, dicatat sebelum `PHASE 2C` dimulai.

### 24.1 `HRD-Q-48` — fallback 480 menit pada Hourly Leave bukan kebijakan yang disetujui

Temuan `PHASE 2B.1` bahwa `LeaveRequestCalculationService.cs` baris 555 memakai fallback hardcode
480 menit bila jadwal tidak dapat diselesaikan adalah **current implementation behavior**,
**bukan** business policy yang pernah disetujui pemilik produk.

| ID | Pertanyaan | Owner | Memblokir |
| --- | --- | --- | --- |
| `HRD-Q-48` | **Baru.** Apakah fallback 480 menit masih boleh dipakai saat jadwal tidak berhasil diselesaikan, atau perhitungan Hourly Leave harus menjadi *calculation exception* sampai `PlannedWorkMinutes` yang valid tersedia? | Pemilik produk | Nilai kebijakan Hourly Leave, tidak memblokir keberadaan alurnya |

**Larangan:** jangan menganggap 480 menit sebagai standar universal rumah sakit. Nilai itu
konstanta kode, bukan kebijakan yang pernah diverifikasi ke pemilik produk manapun.

### 24.2 Penegasan klasifikasi `TrxLeaveRequestApproval`

Diterapkan pada `flows/03-leave.md` bagian 9.3 (lihat berkas untuk detail lengkap):

| Aspek | Nilai |
| --- | --- |
| `CURRENT` | `LEGACY_UNUSED` |
| `TARGET` | `retirement candidate` |
| `DESTRUCTIVE ACTION` | `[BLOCKED]` oleh `HRD-Q-05` / bukti database — **dilarang** menghapus, men-`DROP`, atau menganggap tabelnya kosong |

---

## 25. PHASE 2C — Remaining Administrative Flows, 28 Agustus 2026

Lima flow ditulis: `10-payroll-processing-handoff.md` (`PARTIAL`), `11-lifecycle-offboarding.md`,
`12-competency-training.md`, `13-performance-management.md`,
`14-employee-relations-discipline.md`. Lima sub-agent riset read-only dijalankan paralel, satu
per domain. **Tidak ada architecture, ERD, contracts, frontend, backend code, migration, maupun
database change** pada pass ini.

### 25.1 Ringkasan flow 10–14

| Flow | Rasio API/model | Temuan utama |
| --- | --- | --- |
| 10 — Payroll | 3 jalur handoff domain, `TrxPayrollRun` tanpa controller sama sekali | **`Payroll Executed` ≠ `Employee Paid`**, dibuktikan tegas. Kalkulasi dan approval run-level `MISSING`. Satu-satunya guard run-level nyata: penulisan ditolak bila status sudah terminal |
| 11 — Lifecycle/Offboarding | 1 dari 21 model operasional (resign) | Pencabutan akun aplikasi **tidak otomatis** — dikonfirmasi peringatan eksplisit di source sendiri. Checklist offboarding dibuat sekali, tidak pernah diperbarui lagi |
| 12 — Kompetensi/Pelatihan | 4 dari 13 model operasional | Hanya pencatatan pasca-kejadian + flag verifikasi bebas, bukan lifecycle enrollment→completion. Terputus bersih dari credentialing (aman, sesuai batas) |
| 13 — Manajemen Kinerja | 2 dari 11 model operasional | `Finalize`/`Acknowledge` benar-benar tergerbangi (berbeda dari flow 03's `Acknowledged` yang tidak tergerbangi). `CycleStatus` tidak menjaga urutan. OPPE/FPPE dikonfirmasi ulang: nol kode |
| 14 — Hubungan Karyawan/Disiplin | 1 dari 8 model operasional | **Swa-setuju** ditemukan — aktor dapat menyetujui tindakan disiplinnya sendiri. Data `HighlyRestricted` tanpa tingkatan izin khusus. Enum resmi `DisciplinaryActionStatus` adalah dead code |

### 25.2 Transition edge yang terbukti (contoh kunci)

Resign: `Draft→Submitted→UnderReview→Approved→HandoffCompleted`, seluruhnya guard nyata via
generic workflow engine. Performance: `Finalize` (mensyaratkan semua detail berskor) dan
`Acknowledge` (mensyaratkan `IsFinalized`) — keduanya tergerbangi kode. Payroll: penulisan
snapshot ditolak bila `TrxPayrollRun.RunStatus` terminal. Disiplin: `UpdateStatus` hanya
memeriksa keanggotaan himpunan, **bukan** urutan transisi — dicatat sebagai transisi lemah, bukan
state machine penuh.

### 25.3 Capability classification

| Kelas | Contoh |
| --- | --- |
| `READY TO REUSE` | Resign (flow 11), review kinerja (flow 13), rekaman pelatihan/asesmen pasca-kejadian (flow 12), tindakan disiplin (flow 14, dengan catatan), tiga jalur handoff payroll sampai `execute` (flow 10) |
| `EXTEND` | Checklist offboarding (flow 11), kalkulasi/approval run payroll (flow 10) |
| `MISSING` | Onboarding/probation/termination (flow 11), lifecycle pelatihan formal 11 entity (flow 12), goal/KPI berkelanjutan (flow 13), kasus/investigasi/keluhan (flow 14), pencabutan akun otomatis (flow 11) |
| `BLOCKED` | OPPE/FPPE (flow 13), kredensial klinis sebagai tujuan pelatihan (flow 12), pembayaran/GL/pajak (flow 10) |
| `LEGACY_UNUSED` | `TrxLeaveRequestApproval` (rujukan silang dari `PHASE 2B.1`, tidak berubah) |

### 25.4 Integration boundaries yang dikonfirmasi

HR employment lifecycle terpisah tegas dari identity/account deactivation, asset return, Finance
final settlement (flow 11 — ketiganya `MISSING` di sisi HR, bukan diasumsikan dimiliki HR).
Kompetensi/pelatihan terpisah bersih dari credentialing (flow 12). Manajemen kinerja terpisah
dari OPPE/FPPE (flow 13). Payroll berhenti di batas `HRD-DEC-009` (flow 10).

### 25.5 Open question baru — `HRD-Q-49` s.d. `HRD-Q-53`

| ID | Flow | Isi |
| --- | --- | --- |
| `HRD-Q-49` | 10 | Tidak ditemukan jalur yang membuat/memajukan `TrxPayrollRun.RunStatus` — bagaimana payroll run benar-benar dimulai? |
| `HRD-Q-50` | 11 | Tanggal efektif terakhir bekerja tidak terhubung ke kehadiran/payroll — perlu integrasi otomatis atau cukup manual? |
| `HRD-Q-51` | 14 | Tindakan disiplin dapat disetujui oleh pembuatnya sendiri — dapat diterima, atau perlu pemisahan peran? |
| `HRD-Q-52` | 14 | Data kedisiplinan `HighlyRestricted` tanpa tingkatan izin khusus — perlu dibangun sebelum kapabilitas diperluas? |
| `HRD-Q-53` | 12, 13 | Kompetensi/pelatihan dan kinerja memakai flag verifikasi bespoke, bukan mesin workflow generik seperti domain lain — disengaja atau perlu disatukan? |

### 25.6 Implementation defect baru

Tidak ada defect setingkat `HRD-DEC-022`/`023`/`024` (yaitu invariant yang secara eksplisit
diklaim ada lalu terbukti dilanggar kode). Temuan pass ini seluruhnya kesenjangan **cakupan**
(`MISSING`) yang jujur dari awal, kecuali satu: **swa-setuju pada tindakan disiplin** (flow 14)
adalah pola berjalan yang berpotensi tidak diinginkan — dicatat `HRD-Q-51`, bukan diperbaiki
diam-diam.

### 25.7 Contradiction terhadap flow 00–09

Tidak ditemukan kontradiksi baru terhadap flow 00–09 pada pass ini. `HRD-Q-44` (flow 03/08) tetap
tertutup sejak `PHASE 2B.1`.

### 25.8 Siap masuk architecture phase vs tetap `PARTIAL`/`BLOCKED`

| Status | Flow / bagian |
| --- | --- |
| Siap dirancang penuh (`READY`) | Resign (11), review kinerja + master data (13), pencatatan pelatihan/kompetensi pasca-kejadian + master data (12), tindakan disiplin + master data (14) — **dengan catatan `HRD-Q-51`/`Q-52` sebelum diperluas** |
| `EXTEND` — perlu keputusan cakupan sebelum desain final | Checklist offboarding (11), kalkulasi/approval payroll run (10), lifecycle pelatihan formal (12), goal/KPI berkelanjutan (13), kasus/investigasi (14) |
| Tetap `PARTIAL` | Flow 10 — batas HR/Finance final, sesudahnya tidak dirancang |
| Tetap `BLOCKED` | OPPE/FPPE (13), kredensial klinis (12, 05 kredensial terpisah), pembayaran/GL/pajak (10) |

### 25.9 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `flows/10-payroll-processing-handoff.md` | Baru, status `PARTIAL` |
| `flows/11-lifecycle-offboarding.md` | Baru |
| `flows/12-competency-training.md` | Baru |
| `flows/13-performance-management.md` | Baru |
| `flows/14-employee-relations-discipline.md` | Baru |
| `flows/README.md` | Status flow 10–14 diperbarui menjadi `Ada` |
| `00-interview-decisions.md` | Revisi naik ke `10`; bagian 24 (catatan susulan) dan 25 (`PHASE 2C`) ditambahkan |

Tidak ada file source, migration, entity, controller, database, maupun frontend yang diubah pada
pass ini.
