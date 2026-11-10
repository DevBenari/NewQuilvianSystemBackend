# ISSUE-001 — Layar HR tampak kosong: alamat API salah, pesan gagal tertelan, dan nama kolom tidak cocok dengan backend

```yaml
issue_id: ISSUE-HRD-001
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Administrasi Kepegawaian — enam layar /hr/workforce-core/* (FE-HRD-01 s.d. FE-HRD-06), ditambah 13 layar HR lain yang memakai pola service yang sama"
sumber_laporan: "Laporan pemilik 02-11-2026: 1 keluhan tanpa nomor, 1 lampiran screenshot"
tanggal_issue: "2026-11-02"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "396c836380bba7cde722351c14c08e8b72535279 (MHamzah)"
source_sha_frontend: "9c4000724da817200b92ae36db29f6914023c90d (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md
```

## 1. Ringkasan

Pelapor membuka menu **Sumber Daya Manusia → Administrasi Kepegawaian → Perubahan Data Karyawan**
dan hanya melihat tabel kosong. Tabel itu kosong **bukan** karena datanya tidak ada. Layar memanggil
alamat API yang tidak pernah ada di backend, backend menjawab "tidak ditemukan" (kode 404), lalu
layar menelan pesan gagal itu, sehingga yang tampil hanya tulisan "Tidak ada riwayat perubahan
profil staf."

Kesalahan yang sama terjadi pada **keenam** layar Administrasi Kepegawaian. Pola service yang sama
juga dipakai **13 layar HR lain**, termasuk Kotak Masuk Persetujuan dan layar layanan mandiri pegawai
untuk cuti, lembur, jadwal, dan koreksi kehadiran.

Ada dua hal yang tetap berbahaya walaupun alamatnya sudah dibetulkan. Pertama, sebagian besar kolom
tabel akan tetap kosong karena nama field yang dibaca layar berbeda dengan yang dikirim backend.
Kedua, layar Penetapan Gaji akan menampilkan **"IDR 0"** sebagai gaji pokok setiap pegawai.

Tidak ada butir yang ternyata sesuai desain. Seluruh temuan menyimpang dari desain yang disetujui
pada `03-frontend-architecture.md` bagian 2.1, 4.1, dan 4.2.

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor, diringkas) | Lampiran |
| --- | --- | --- |
| 1 | "Coba cek pada source code apakah benar Workforce Core benar-benar dikerjakan, soalnya saya lihat hanya tabel-tabel kosong." | Lampiran 1: screenshot `localhost:3000/hr/workforce-core/employee-profile-changes`, 2 November 2026 pukul 10.47 |

Laporan tidak bernomor; satu keluhan diberi nomor 1. Lampiran 1 juga memperlihatkan **dua baris
pagination** di bawah tabel. Hal itu tidak dikeluhkan, dan dicatat sebagai temuan tambahan
`ISS-HRD-001-T4`.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ISS-HRD-001-01` | 1 | Enam layar Administrasi Kepegawaian memanggil alamat API yang tidak ada | `CONTRACT_MISMATCH` | Frontend | Blocker | SUDAH-VERIFIKASI | `FIX-HRD-001-01` |
| `ISS-HRD-001-T1` | — | Pesan "gagal memuat" tidak pernah tampil di 17 layar HR | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-001-03` |
| `ISS-HRD-001-T2` | — | Kolom tetap kosong karena nama field layar berbeda dengan backend | `CONTRACT_MISMATCH` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-001-02` |
| `ISS-HRD-001-T3` | — | Layar Penetapan Gaji akan menampilkan "IDR 0" sebagai gaji pokok | `RULE_VIOLATION`, `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-001-02` |
| `ISS-HRD-001-T4` | — | Pagination ganda; hitungan "dari 0 data" selalu nol | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI; DARI-CAPTURE | `FIX-HRD-001-04` |
| `ISS-HRD-001-T5` | — | 13 layar HR lain memanggil awalan `/v1/hr/` yang tidak ada | `CONTRACT_MISMATCH` | Frontend; sebagian Backend | Blocker | SUDAH-VERIFIKASI | `FIX-HRD-001-05`, `-12`, `-06`, `-07` |
| `ISS-HRD-001-T6` | — | Laporan task menyatakan "PASS" tanpa uji ujung-ke-ujung; roadmap bertanda ✅ | `BUG` (proses) | Dokumen | Medium | SUDAH-VERIFIKASI | `FIX-HRD-001-08` |
| `ISS-HRD-001-T7` | — | Layar detail Perubahan Data (`FE-HRD-07`) belum ada | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-001-09` |
| `ISS-HRD-001-T8` | — | Tiga layar akan tetap kosong walau sudah benar, karena datanya belum diisi | `DATA` | Data | Medium | SUDAH-VERIFIKASI (dokumen) | `FIX-HRD-001-10` |
| `ISS-HRD-001-T9` | — | Layar daftar tidak memuat kartu ringkasan dan saringan sesuai Pola A | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-HRD-001-11` |

## 4. Rincian per temuan

### ISS-HRD-001-01 — Enam layar Administrasi Kepegawaian memanggil alamat API yang tidak ada

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source dan dengan uji HTTP ke backend lokal |
| **Perbaikan** | `FIX-HRD-001-01` |

**Apa yang terjadi.** Setiap kali petugas HR membuka salah satu dari enam layar Administrasi
Kepegawaian, layar meminta data ke alamat yang tidak dikenal server. Server menjawab "alamat tidak
ditemukan", dan layar menampilkan tabel kosong seolah-olah rumah sakit belum punya data. Padahal
database pengembangan pada 2 November 2026 berisi 55 penempatan organisasi yang seharusnya tampil
di layar Penempatan Organisasi.

**Kenapa terjadi.**

```text
tabel kosong
  ← layar memanggil /api/v1/hr/workforce-core/cross-employee/...  → server menjawab 404
    ← BASE_URL service dikarang, tidak disalin dari [Route] controller backend
       ← laporan task FE-HRD-002 menyatakan "terhubung ke endpoint yang sah" tanpa uji langsung
```

`FE@9c40007 src/lib/services/hr/workforce-core-cross-employee-service.js:6`:

```js
const BASE_URL = "/v1/hr/workforce-core/cross-employee";
```

`InstanceAxios` menambahkan awalan `NEXT_PUBLIC_API_QUILVIAN` (`…/api`), sehingga alamat akhirnya
`/api/v1/hr/workforce-core/cross-employee/...`. Backend melayani alamat lain.
`BE@396c836 Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforceCoreCrossEmployeeController.cs:13`:

```csharp
[Route("api/v1/corporate/human-resource/workforce-core")]
```

Uji tanpa login ke backend lokal pada 2 November 2026 membuktikan selisihnya:

| Alamat | Jawaban server | Artinya |
| --- | --- | --- |
| `api/v1/hr/workforce-core/cross-employee/employee-profile-changes` | **404** | Alamat tidak ada |
| `api/v1/corporate/human-resource/workforce-core/organization-assignments` | **401** | Alamat ada; perlu login |
| `api/v1/corporate/human-resource/employee-profile-changes` | **401** | Alamat ada; perlu login |

Layar Perubahan Data Karyawan punya masalah tambahan. Grup `cross-employee` di backend **tidak
memiliki** endpoint `employee-profile-changes` sama sekali. Daftar lintas-pegawai untuk permohonan
perubahan data sudah lama tersedia di controller lain:
`BE@396c836 .../WorkforceCore/Controllers/EmployeeProfileChangeController.cs:18` dan `:89`
(`GET api/v1/corporate/human-resource/employee-profile-changes`). Saringannya cocok dengan yang
dikirim layar: `search`, `pageNumber`, dan `pageSize`.

**Apakah ini menyimpang dari desain?** Ya — `BUG`. `03-frontend-architecture.md` bagian 2.1
menetapkan `FE-HRD-01` s.d. `FE-HRD-06` sebagai daftar yang membaca data backend. Desain tidak
pernah meminta alamat `/v1/hr/...`.

**Dampak nyata.** Kepala Bagian Kepegawaian "Sari" ingin memastikan siapa saja yang sudah ditempatkan
di ICU. Layar menyatakan tidak ada data. Sari mengira penempatan belum pernah diisi, lalu meminta
staf mengisi ulang satu per satu dari berkas pegawai. Akibatnya ada pekerjaan ganda, dan muncul
risiko penempatan dobel.

**Rekomendasi.** Betulkan alamat di frontend agar sama persis dengan `[Route]` backend, dan arahkan
layar Perubahan Data Karyawan ke `employee-profile-changes`. Opsi lain, yaitu membuat alias
`api/v1/hr/...` di backend, tidak dipilih. Rinciannya di plan repair.

### ISS-HRD-001-T1 — Pesan "gagal memuat" tidak pernah tampil di 17 layar HR

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Inilah sebab layar menampilkan "tidak ada data" alih-alih "gagal memuat" pada lampiran 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-001-03` |

**Apa yang terjadi.** Ketika server menolak atau gagal, layar seharusnya menampilkan kotak merah
"Data gagal dimuat". Kotak itu tidak pernah muncul. Petugas hanya melihat tabel kosong dan tidak
punya cara mengetahui bahwa sistem sedang gagal.

**Kenapa terjadi.** View mengirim teks galat lewat prop `description`, sedangkan komponen
`InformationAlert` hanya membaca `message` dan `children`. Bila keduanya kosong, komponen tidak
menggambar apa pun.

`FE@9c40007 src/components/view/hr/workforce-core/workforce-core-cross-employee-list-view.jsx:112-116`:

```jsx
<InformationAlert
  variant="danger"
  title="Terjadi Kesalahan"
  description={error}
```

`FE@9c40007 src/components/features/base-features/information-alert.jsx:47-66`:

```jsx
export default function InformationAlert({ message, variant = "danger", title, ... children, ... }) {
  ...
  if (!safeMessage && !children) return null;
```

Toast galat juga tidak terlihat pada lampiran 1. Jadi kedua jalur pemberitahuan sama-sama gagal
menyampaikan pesan. Penyebab toast tidak terlihat belum dibuktikan (`DUGAAN`). Cara membuktikannya:
buka layar dengan backend mati, lalu periksa apakah `ToastStack` menerima item berbentuk
`{ id, type, message }`.

Pola salah yang sama ditemukan di **17 view** (pencarian `InformationAlert` yang diikuti
`description=`):

| Kelompok | View |
| --- | --- |
| Administrasi Kepegawaian | `workforce-core/workforce-core-cross-employee-list-view.jsx` (dipakai 6 layar) |
| Admin HR | `workflow/approval-inbox-view.jsx`, `attendance/attendance-monitoring-view.jsx`, `leave/leave-admin-view.jsx`, `overtime/overtime-admin-view.jsx`, `payroll/payroll-restitution-installment-view.jsx`, `scheduling/roster-scheduling-view.jsx`, `learning/training-certification-view.jsx`, `performance/performance-review-view.jsx`, `employee-relations/disciplinary-action-view.jsx` |
| Exit clearance | `exit-clearance/exit-clearance-list-view.jsx`, `exit-clearance-detail-view.jsx`, `exit-clearance-finance-status-card.jsx` (ditambah prop `actionLabel`/`onAction` yang juga tidak dibaca komponen) |
| Layanan mandiri | `self-services/human-resource/leave/leave-employee-view.jsx`, `overtime/overtime-employee-view.jsx`, `schedule/schedule-employee-view.jsx`, `attendance-correction/attendance-correction-employee-view.jsx` |

**Apakah ini menyimpang dari desain?** Ya — `BUG`. `03-frontend-architecture.md` bagian 4.1, baris
keadaan "gagal": *"Data gagal dimuat." [Coba lagi]*.

**Dampak nyata.** HR Manager "Dodi" membuka Kotak Masuk Persetujuan menjelang tutup periode. Layar
kosong, sehingga Dodi menyimpulkan tidak ada pengajuan yang menunggu. Cuti dan lembur pegawai tidak
pernah disetujui, padahal sebenarnya layarnya yang gagal memuat.

**Rekomendasi.** Ganti prop menjadi `message`. Saat gagal, tabel menampilkan keadaan "Data gagal
dimuat" dengan tombol Coba lagi, bukan pesan "tidak ada data".

### ISS-HRD-001-T2 — Kolom tetap kosong karena nama field layar berbeda dengan backend

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Setelah `FIX-HRD-001-01` dikerjakan, tabel akan berisi baris tetapi banyak kolomnya "-". Pelapor akan melihat gejala "tabel kosong" dalam bentuk lain |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-001-02` |

**Apa yang terjadi.** Layar membaca field yang tidak dikirim server. Contohnya, layar mencari
`startDate`, padahal server mengirim `effectiveStartDate`. Tidak ada galat apa pun; kolomnya
hanya berisi "-".

**Kenapa terjadi.** Kolom tabel ditulis tanpa dicocokkan dengan DTO backend. Pencocokan harfiah
antara `row.<field>` di view dan properti DTO
(`BE@396c836 .../WorkforceCore/DTOs/*.cs`, `WorkforceCoreCrossEmployeeDtos.cs`) menghasilkan:

| Layar | Field dibaca layar yang **tidak** dikirim backend | Padanan yang dikirim backend |
| --- | --- | --- |
| Perubahan Data Karyawan | `changeType`, `changeDate`, `effectiveDate`, `status`, `reason`, `approvedByName` | `requestCategory`, `submittedAt`, *(tidak ada)*, `requestStatus`, `requestReasonText`, *(hanya `approvedAt`)* |
| Penempatan Organisasi | `startDate`, `endDate`, `hospitalSiteName`, `organizationUnitName`, `costCenterName` | `effectiveStartDate`, `effectiveEndDate`; ketiga nama hanya dikirim sebagai Id |
| Penempatan Jabatan | `startDate`, `endDate`, `jobLevelName`, `jobFamilyName`, `employeeGradeName` | `effectiveStartDate`, `effectiveEndDate`; ketiga nama hanya dikirim sebagai Id |
| Relasi Atasan | `effectiveDate`, `endDate`, `isPrimary`, `managerWorkforceProfileCode`, `relationshipType` | `effectiveStartDate`, `effectiveEndDate`, `isPrimaryManager`, `managerProfileCode`, `managerType` |
| Riwayat Kepegawaian | `approvedByName` | `approvedByUserName` |
| Penetapan Gaji | `currency`, `payFrequency`, `effectiveDate`, `endDate`, `baseSalary` | `currencyCode`, `paymentFrequency`, `effectiveStartDate`, `effectiveEndDate`, *(sengaja tidak dikirim — T3)* |

Status Perubahan Data juga memakai kosakata yang berbeda. Layar mengenali `approved`, `rejected`,
`submitted`, dan `pending`, sedangkan backend memakai `Draft`, `Submitted`, `UnderVerification`,
`NeedRevision`, `Approved`, `Rejected`, `Cancelled`, dan `Applied`
(`EmployeeProfileChangeService.RequestStatuses`). Akibatnya `UnderVerification` dan `Applied` tampil
dengan warna "info" tanpa label Bahasa Indonesia.

**Apakah ini menyimpang dari desain?** Ya — `BUG`. Kontrak backend yang berlaku adalah DTO di atas.
Desain tidak menetapkan nama field yang berbeda.

**Dampak nyata.** Setelah alamat dibetulkan, layar Relasi Atasan menampilkan nama pegawai dan
atasan, tetapi kolom "Atasan Utama" selalu "Tidak" dan "Tgl Efektif" selalu "-". Kepala Ruangan
"Andi" akan mengira tidak ada satu pun relasi atasan utama, padahal relasi itulah yang menentukan
ke mana pengajuan cuti anak buahnya dikirim.

**Rekomendasi.** Samakan nama field di layar dengan DTO. Kolom yang backend hanya kirim sebagai Id
diganti kolom yang namanya memang dikirim (misalnya Departemen dan Jabatan), atau dihapus dulu.

### ISS-HRD-001-T3 — Layar Penetapan Gaji akan menampilkan "IDR 0" sebagai gaji pokok

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Akan terlihat begitu `FIX-HRD-001-01` dikerjakan, dan melanggar keputusan keamanan |
| **Jenis** | `RULE_VIOLATION` (`HRD-DEC-033`), `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-001-02` |

**Apa yang terjadi.** Layar Penetapan Gaji punya kolom "Gaji Pokok". Backend sengaja tidak mengirim
nominal gaji, sehingga layar menuliskan **"IDR 0"** untuk setiap pegawai.

**Kenapa terjadi.**
`FE@9c40007 src/components/view/hr/workforce-core/salary-assignments-view.jsx:32-40`:

```jsx
key: "baseSalary",
title: "Gaji Pokok",
render: (row) => {
  const curr = row.currency || "IDR";
  const val = Number(row.baseSalary || 0).toLocaleString("id-ID");
  return `${curr} ${val}`;
},
```

Backend tidak mengirim `baseSalary` pada daftar lintas-pegawai, sesuai `HRD-DEC-033`
(`WorkforceCoreCrossEmployeeController.cs`, ringkasan endpoint `salary-assignments`). Nilai kosong
diubah menjadi `0`.

**Apakah ini menyimpang dari desain?** Ya. `03-frontend-architecture.md` bagian 4.2: *"Kolom nominal
gaji TIDAK ADA pada daftar ini — HRD-DEC-033."* Keputusan itu berstatus `SECURITY_APPROVED`.

**Dampak nyata.** Petugas payroll "Rina" memeriksa penetapan gaji sebelum tutup periode. Semua
pegawai tampak bergaji "IDR 0". Rina bisa menyimpulkan penetapan gaji belum diisi lalu
memasukkannya ulang, atau lebih buruk, mempercayai angka itu.

**Rekomendasi.** Hapus kolom "Gaji Pokok". Sesuai bagian 4.2, tampilkan mata uang, tanggal berlaku,
penanda utama, dan status persetujuan (`isApproved`, `approvedByUserName`).

### ISS-HRD-001-T4 — Pagination ganda; hitungan "dari 0 data" selalu nol

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Terlihat pada lampiran 1 tetapi tidak dikeluhkan |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI; tampilannya DARI-CAPTURE lampiran 1 |
| **Perbaikan** | `FIX-HRD-001-04` |

**Apa yang terjadi.** Di bawah tabel ada dua pengatur halaman. Yang pertama menulis "Menampilkan 0
sampai 0 dari 0 data" dengan tombol Sebelumnya/Berikutnya, dan yang kedua berbentuk « ‹ 1 › ».
Setelah data termuat, baris pertama tetap menyebut "dari 0 data" karena tidak pernah diberi jumlah
data.

**Kenapa terjadi.** `DataTable` sudah punya pagination bawaan (`pagination = true`,
`FE@9c40007 src/components/features/base-features/data-table.jsx:152`, penggambaran `:336-372`).
View tidak mengirim `pageNumber`, `totalData`, `totalPage`, maupun `onPageChange` ke `DataTable`,
lalu menambahkan komponen `Pagination` sendiri di bawahnya. Pola yang benar sudah ada di layar
Master Data Pegawai: `components/view/hr/master-data/employees/master-data-employee-view.jsx:443-458`
mengirim keempat prop itu dan `PaginationComponent={RegionPagination}`.

Pola ganda yang sama ada pada 13 view lain yang memakai service HR (lihat `FIX-HRD-001-04`).

**Apakah ini menyimpang dari desain?** Ya — `BUG`. Pola A (bagian 4.1) hanya punya satu baris
"Halaman 1 dari n".

**Dampak nyata.** Petugas melihat "dari 0 data" padahal tabel berisi 25 baris, lalu ragu apakah
datanya benar.

**Rekomendasi.** Ikuti pola Master Data Pegawai dan hapus komponen `Pagination` terpisah.

### ISS-HRD-001-T5 — 13 layar HR lain memanggil awalan `/v1/hr/` yang tidak ada

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Akar masalahnya sama dengan `ISS-HRD-001-01`; tercatat sebagai `HRD-TF-020` pada capability map bagian 14.4 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend; golongan B juga Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source dan uji HTTP. Bahwa layar-layar itu **tampak** kosong di peramban adalah `DUGAAN` yang sangat kuat (alamat 404 ditambah T1); buktikan dengan membuka tiap layar dan melihat tab Network |
| **Perbaikan** | `FIX-HRD-001-05` (admin), `FIX-HRD-001-12` (layanan mandiri), `FIX-HRD-001-06`, `FIX-HRD-001-07` |

**Apa yang terjadi.** Sebelas berkas service HR memanggil alamat berawalan `/v1/hr/...`. Backend
punya **0** route berawalan `api/v1/hr`. Uji lokal: `api/v1/hr/workflow/approval-inbox`,
`api/v1/hr/leave`, dan `api/v1/hr/attendance` semuanya menjawab 404, sedangkan
`api/v1/corporate/human-resource/approval-inbox` menjawab 401.

Dua service lain sudah benar: benefit
(`lib/constants/hr/benefit/employee-benefit-enrollment-constants.js:7`) dan exit clearance
(`lib/constants/hr/exit-clearance/exit-clearance-constants.js:7`).

**Kenapa terjadi.** Sama dengan `ISS-HRD-001-01`: alamat ditulis tanpa disalin dari `[Route]`
controller. Untuk sebagian layar, rancangan frontend juga mengandaikan endpoint yang memang belum
pernah dibuat di backend.

**Pemetaan 56 pemanggilan API** (lengkap pada Lampiran A) dibagi empat golongan:

| Golongan | Artinya | Jumlah | Contoh |
| :---: | --- | ---: | --- |
| `A` | Padanan ada di backend; cukup betulkan alamat (kadang segmen atau method ikut berubah) | 34 (termasuk 6 milik Workforce Core) | `/v1/hr/leave/balance-adjustments` → `api/v1/corporate/human-resource/leave/adjustments` |
| `B` | Backend hanya menyediakan data **per pegawai**, sedangkan layar meminta daftar seluruh pegawai | 6 | Tindakan Disiplin, Pelatihan & Sertifikasi, Penilaian Kinerja |
| `C` | Tidak ada padanan sama sekali di backend | 11 | persetujuan massal, klaim lembur layanan mandiri, daftar permohonan cuti untuk admin |
| `D` | Padanan terdekat ada, tetapi makna atau bentuk isiannya belum pasti | 5 | `roster-periods/{id}/lock` versus `/close` |

**Layar yang terdampak:**

| Layar | Route | Golongan pemanggilannya |
| --- | --- | --- |
| Kotak Masuk Persetujuan | `/hr/workflow/approval-inbox` | 6 `A`, 1 `C` |
| Monitoring Kehadiran | `/hr/attendance` | 3 `A` |
| Penyesuaian Cuti | `/hr/leave/adjustments` | 2 `A`, 1 `C` |
| Rekonsiliasi Lembur | `/hr/overtime/reconciliation` | 2 `C` |
| Restitusi & Cicilan | `/hr/payroll/restitution-installment` | 1 `A`, 2 `C`, 1 `D` |
| Roster | `/hr/scheduling/roster` | 9 `A`, 1 `C`, 1 `D` |
| Pelatihan & Sertifikasi | `/hr/learning/training-records` | 2 `B` |
| Penilaian Kinerja | `/hr/performance/reviews` | 2 `B` |
| Tindakan Disiplin | `/hr/employee-relations/disciplinary-actions` | 2 `B` |
| Layanan mandiri: cuti, lembur, jadwal, koreksi kehadiran | `/self-services/human-resource/{leave,overtime,schedule,attendance-correction}` | 7 `A`, 4 `C`, 3 `D` |

**Apakah ini menyimpang dari desain?** Golongan `A` adalah `BUG`. Golongan `B`, `C`, dan `D`
menunjukkan layar yang dibangun di atas endpoint yang belum dirancang. Memperbaikinya membutuhkan
keputusan pemilik (K-02, K-03), bukan sekadar membetulkan alamat.

**Dampak nyata.** Pegawai "Budi" mengajukan cuti lewat layanan mandiri. Permintaan dikirim ke alamat
yang tidak ada, sehingga cutinya tidak pernah tercatat, dan pesan gagalnya pun tidak tampil (T1).

**Rekomendasi.** Golongan `A` dibetulkan langsung di frontend. Golongan `B` dan `C/D` menunggu
keputusan K-02 dan K-03. Sementara itu, kontrol yang memanggil endpoint yang tidak ada
disembunyikan atau ditandai "belum tersedia", bukan dibiarkan gagal diam-diam.

### ISS-HRD-001-T6 — Laporan task menyatakan "PASS" tanpa uji ujung-ke-ujung

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Menjelaskan kenapa cacat sebesar ini lolos dan roadmap menyatakan selesai |
| **Jenis** | `BUG` (proses) |
| **Area** | Dokumen |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-001-08` |

**Apa yang terjadi.** `task/report/frontend/FE-HRD-002.md` baris 85 dan 95 menyatakan "Komunikasi
API Backend — PASS — seluruh endpoint terhubung ke controller `BE-HRD-003`". Baris 96 menyatakan
layar memakai `SummaryCards`. Keduanya tidak benar pada source: alamatnya 404 (`ISS-HRD-001-01`), dan
view tidak memuat kartu ringkasan (`ISS-HRD-001-T9`). `roadmap/frontend-roadmap.md` baris 41, 81, dan
95 menandai `FE-HRD-002` ✅. Hal yang sama dicatat sebagai `HRD-TF-011` pada decision log bagian 31.2.

**Kenapa terjadi.** Bukti "PASS" ditulis dari pembacaan kode, bukan dari layar yang dibuka dan
diisi data. Tidak ada test frontend yang menyentuh HR (`HRD-CAP-47`).

**Rekomendasi.** Status ✅ task yang memakai service terdampak — `FE-HRD-002` s.d. `FE-HRD-006`, `FE-HRD-008` s.d. `FE-HRD-013`, `FE-HRD-015`, `FE-HRD-016`, `FE-HRD-018` (alamat API) dan `FE-HRD-017` (pesan gagal) — ditinjau ulang lewat jalur
roadmap yang berwenang. Ke depan, laporan task frontend wajib menyertakan bukti uji langsung, misalnya
lewat skill `live-browser-testing`.

### ISS-HRD-001-T7 — Layar detail Perubahan Data (`FE-HRD-07`) belum ada

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Tanpa layar ini, HR tidak dapat memverifikasi permohonan dari layar daftar |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-001-09` |

**Apa yang terjadi.** `03-frontend-architecture.md` bagian 2.1 merancang `FE-HRD-07` di route
`/hr/workforce-core/employee-profile-changes/[slug]`. Folder itu tidak ada di
`FE@9c40007 src/app/hr/workforce-core/employee-profile-changes/`; yang ada hanya `page.jsx`.
Keputusan `HRD-DEC-055` (`PROFILE_CHANGE_IS_TRANSACTION_PAGE`) dan `HRD-DEC-052` menambah isi layar
ini, sehingga ia perlu dirancang ulang lebih dulu.

**Rekomendasi.** Dikerjakan sesudah `design-business-module` untuk slice `S-A1`. Tidak termasuk
perbaikan cepat.

### ISS-HRD-001-T8 — Tiga layar akan tetap kosong walau sudah benar

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Agar pelapor tidak mengira perbaikan gagal |
| **Jenis** | `DATA` |
| **Area** | Data |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada dokumen: decision log `HRD-TF-016` dan `HRD-TF-017` (hitung baris read-only, 2 November 2026, sesi audit sebelumnya). Skill ini tidak menjalankan query |
| **Perbaikan** | `FIX-HRD-001-10` |

**Apa yang terjadi.** Database pengembangan `QuilvianNewDevHamzah` pada 2 November 2026 berisi
**0** penempatan jabatan, **0** relasi atasan, **0** penetapan gaji, **0** permohonan perubahan data,
dan **1** riwayat kepegawaian. Setelah perbaikan, layar Penempatan Jabatan, Relasi Atasan, Penetapan
Gaji, dan Perubahan Data Karyawan akan **benar** menampilkan "belum ada data". Hanya Penempatan
Organisasi yang langsung berisi 55 baris. Selain itu, 14 dari 55 profil tidak terhubung ke data
pegawai, dokter, maupun pengguna eksternal (`HRD-TQ-11`).

**Rekomendasi.** Pengisian data awal mengikuti `HRD-DEC-054` (batch data awal yang disetujui sekali),
yang masih harus dirancang. Query pemeriksaan untuk pelapor ada pada Lampiran B.

### ISS-HRD-001-T9 — Layar daftar tidak memuat kartu ringkasan dan saringan sesuai Pola A

| | |
| --- | --- |
| **No. laporan** | — |
| **Kenapa dicantumkan** | Terlihat pada lampiran 1: hanya ada kotak cari, tanpa ringkasan dan tanpa saringan unit, status, maupun periode |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI; DARI-CAPTURE lampiran 1 |
| **Perbaikan** | `FIX-HRD-001-11` |

**Apa yang terjadi.** Pola A (`03-frontend-architecture.md` bagian 4.1) meminta kartu ringkasan
(total, menunggu, selesai, bermasalah) dan saringan Unit, Status, dan Periode yang isinya diambil
dari `filters/metadata`. Layar hanya punya kotak cari. Backend daftar lintas-pegawai sudah menerima
saringan `startDate`, `endDate`, `departmentId`, `isActive`, dan lainnya
(`WorkforceCoreCrossEmployeeController.cs:37-55`).

**Rekomendasi.** Dikerjakan setelah perbaikan prioritas 1. Label status baris yang diminta
`HRD-DEC-055` menunggu desain `S-A1`, karena backend belum punya kolom persetujuan penempatan
(`HRD-CAP-29`).

## 5. Tanya-jawab pelapor

> **T:** Apakah benar Workforce Core sudah benar-benar dikerjakan?
>
> **J:** Baru sebagian. Backend-nya sudah cukup lengkap: 15 controller berkas pegawai, alur
> Perubahan Data Karyawan, ringkasan Employee 360, dan 5 daftar lintas-pegawai yang dibuat untuk
> keenam layar ini. Frontend-nya sudah membuat keenam layar, tetapi **tidak pernah tersambung** ke
> backend: alamatnya salah, nama kolomnya tidak cocok, dan pesan gagalnya tertelan. Roadmap
> menandainya selesai tanpa uji langsung. Kemampuan bisnis bab 02 Buku Panduan yang lebih besar,
> seperti mutasi/promosi lewat SK dan persetujuan penempatan, memang belum dibangun. Hal itu
> ditangani lewat decision log bagian 31 (`HRD-DEC-046` s.d. `HRD-DEC-057`), bukan lewat issue ini.

> **T (tersirat):** Kenapa tabelnya kosong?
>
> **J:** Karena layar meminta data ke alamat yang tidak ada, lalu menyembunyikan pesan gagalnya.
> Datanya sendiri ada: 55 penempatan organisasi sudah tersimpan. Lihat `ISS-HRD-001-01` dan
> `ISS-HRD-001-T1`.

## 6. Temuan tambahan

Seluruh temuan tambahan sudah dirinci pada bagian 4 (`ISS-HRD-001-T1` s.d. `ISS-HRD-001-T9`).
Semuanya berada di dalam blueprint `human-resource`, yang berbentuk tunggal (`SINGLE`). Tidak ada
temuan di modul lain.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Apakah layar HR lain, misalnya Kotak Masuk Persetujuan, Penyesuaian Cuti, dan cuti di layanan mandiri, juga tampak kosong di tempat Anda? | Membuktikan dampak `ISS-HRD-001-T5` di peramban, bukan hanya di source | Tidak menahan perbaikan; hanya menguatkan bukti |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Cara memperbaiki awalan `/v1/hr/` (`HRD-TQ-12`) | (a) Betulkan di frontend agar sama dengan `[Route]` backend; (b) tambah alias `api/v1/hr/...` di backend | **(a).** Satu alamat per fitur. Backend tidak pernah merencanakan alias; `HRD-DEC-016` hanya mengatur alias route master data | `FIX-HRD-001-01`, `FIX-HRD-001-05` |
| K-02 | Layar golongan `B` (Disiplin, Pelatihan & Sertifikasi, Kinerja) yang meminta daftar seluruh pegawai | (a) Tambah endpoint daftar lintas-pegawai di backend mengikuti pola `BE-HRD-003`; (b) ubah layar menjadi "pilih pegawai dulu" memakai endpoint per pegawai | **(a)**, dengan butir hak `ReadAll` terpisah per resource. Untuk Disiplin, butir hak sensitif tersendiri, karena `HRD-DEC-053` menutup data disiplin bahkan dari atasan | `FIX-HRD-001-06` |
| K-03 | Kontrol pada golongan `C` dan `D` yang memanggil endpoint tidak ada atau belum pasti | (a) Sembunyikan atau tandai "belum tersedia" sampai dirancang; (b) biarkan apa adanya | **(a).** Layar tidak boleh gagal diam-diam. Rancangan fitur aslinya didaftarkan ke `design-business-module` per sub-modul | `FIX-HRD-001-07` |
| K-04 | Nasib 14 profil yatim (`HRD-TQ-11`) | (a) Data demo, dinonaktifkan; (b) orang sungguhan, ditautkan | Tetapkan per profil oleh pemilik database | Sebagian `FIX-HRD-001-10` |

## 8. Catatan pola

1. **Satu pola salah disalin ke banyak layar.** Prop `description` pada `InformationAlert` (17 view),
   pagination ganda (14 view), dan awalan `/v1/hr/` (11 service) muncul dengan bentuk yang identik.
   Ketiganya kemungkinan besar berasal dari satu templat yang disalin antar-task `FE-HRD-*`.
2. **Alamat API dikarang, bukan disalin.** Tidak satu pun service yang salah merujuk `[Route]`
   controller dalam komentarnya. Service yang benar (benefit, exit clearance) menyimpan alamatnya di
   konstanta yang mengikuti route backend.
3. **"PASS" tanpa layar dibuka.** Laporan task menilai keterhubungan API dari membaca kode.
   Usulan pencegahan berikut diajukan untuk dipertimbangkan masuk ke `rules/frontend/`, **bukan**
   ditetapkan oleh issue ini:
   - setiap service baru menyebut `controller:baris` route backend yang dipanggilnya;
   - laporan task frontend yang menyentuh API wajib menyertakan bukti uji langsung (layar dibuka,
     data tampil, tab Network tanpa 404);
   - `InformationAlert` menerima teks lewat `message`; prop lain yang tidak dikenal sebaiknya
     memicu peringatan saat pengembangan.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Issue dibuat dari 1 keluhan dan 1 lampiran; 1 butir pelapor dan 9 temuan tambahan | `diagnose-module-issue` |

---

## Lampiran A — Pemetaan 56 pemanggilan API service HR ke backend

Golongan: `A` padanan ada · `B` backend hanya per pegawai · `C` tidak ada · `D` padanan belum pasti.

| Pemanggilan (FE@9c40007 `src/lib/services/hr/`) | Method | Alamat yang dipanggil | Golongan | Padanan di backend BE@396c836 |
| --- | --- | --- | :---: | --- |
| **Kotak Masuk Persetujuan `/hr/workflow/approval-inbox`** | | | | |
| `approval-inbox-service.js:24` | `GET` | `/v1/hr/workflow/approval-inbox/summary` | `A` | `GET api/v1/corporate/human-resource/approval-inbox/summary` |
| `approval-inbox-service.js:30` | `GET` | `/v1/hr/workflow/approval-inbox` | `A` | `GET api/v1/corporate/human-resource/approval-inbox` |
| `approval-inbox-service.js:38` | `GET` | `/v1/hr/workflow/approval-inbox/{}` | `A` | `GET api/v1/corporate/human-resource/approval-inbox/{assignmentId}` |
| `approval-inbox-service.js:44` | `POST` | `/v1/hr/workflow/approval-inbox/{}/approve` | `A` | `POST api/v1/corporate/human-resource/approval-inbox/{assignmentId}/approve` |
| `approval-inbox-service.js:50` | `POST` | `/v1/hr/workflow/approval-inbox/{}/reject` | `A` | `POST api/v1/corporate/human-resource/approval-inbox/{assignmentId}/reject` |
| `approval-inbox-service.js:56` | `POST` | `/v1/hr/workflow/approval-inbox/{}/request-revision` | `A` | `POST api/v1/corporate/human-resource/approval-inbox/{assignmentId}/request-revision` |
| `approval-inbox-service.js:62` | `POST` | `/v1/hr/workflow/approval-inbox/batch-approve` | `C` | Tidak ada persetujuan massal di backend |
| **Monitoring Kehadiran `/hr/attendance`** | | | | |
| `attendance-management-service.js:24` | `GET` | `/v1/hr/attendance/dailies` | `A` | `GET api/v1/corporate/human-resource/attendance/dailies` |
| `attendance-management-service.js:32` | `GET` | `/v1/hr/attendance/periods` | `A` | `GET api/v1/corporate/human-resource/attendance/periods` |
| `attendance-management-service.js:40` | `POST` | `/v1/hr/attendance/corrections/on-behalf` | `A` | `POST api/v1/corporate/human-resource/attendance/correction-requests/on-behalf` |
| **Tindakan Disiplin `/hr/employee-relations/disciplinary-actions`** | | | | |
| `employee-relations-service.js:23` | `GET` | `/v1/hr/workforce-profile/disciplinary-actions` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/disciplinary-actions` |
| `employee-relations-service.js:29` | `GET` | `/v1/hr/workforce-profile/disciplinary-actions/{}` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/disciplinary-actions/{id}` |
| **Layanan mandiri: cuti, lembur, jadwal, koreksi kehadiran `/self-services/human-resource/*`** | | | | |
| `employee-self-service.js:22` | `GET` | `/v1/hr/self-service/leave/balances` | `A` | `GET api/v1/self-services/human-resource/leave/balances` |
| `employee-self-service.js:26` | `GET` | `/v1/hr/self-service/leave/requests` | `A` | `GET api/v1/self-services/human-resource/leave/requests` |
| `employee-self-service.js:33` | `POST` | `/v1/hr/self-service/leave/requests` | `D` | `POST api/v1/self-services/human-resource/leave/requests` membuat draf; pengajuan lewat `POST .../{id}/submit` terpisah |
| `employee-self-service.js:39` | `GET` | `/v1/hr/self-service/overtime/plans` | `D` | Terdekat `GET api/v1/self-services/human-resource/overtime`; FE menyebutnya "plans" |
| `employee-self-service.js:46` | `POST` | `/v1/hr/self-service/overtime/plans` | `D` | Terdekat `POST api/v1/self-services/human-resource/overtime`; bentuk isian perlu dicocokkan |
| `employee-self-service.js:50` | `GET` | `/v1/hr/self-service/overtime/claims` | `C` | Tidak ada klaim lembur layanan mandiri |
| `employee-self-service.js:57` | `POST` | `/v1/hr/self-service/overtime/claims` | `C` | Tidak ada klaim lembur layanan mandiri |
| `employee-self-service.js:63` | `GET` | `/v1/hr/self-service/schedule/assignments` | `C` | Tidak ada daftar jadwal milik sendiri di layanan mandiri |
| `employee-self-service.js:70` | `POST` | `/v1/hr/self-service/schedule/change-requests` | `A` | `POST api/v1/self-services/human-resource/schedule-change-requests` |
| `employee-self-service.js:77` | `POST` | `/v1/hr/self-service/schedule/shift-swap-requests` | `A` | `POST api/v1/self-services/human-resource/shift-swap-requests` |
| `employee-self-service.js:86` | `GET` | `/v1/hr/attendance/corrections` | `A` | `GET api/v1/self-services/human-resource/attendance-corrections` |
| `employee-self-service.js:93` | `POST` | `/v1/hr/attendance/corrections` | `A` | `POST api/v1/self-services/human-resource/attendance-corrections` |
| `employee-self-service.js:97` | `GET` | `/v1/hr/attendance/corrections/fields/options` | `C` | Tidak ada; terdekat `GET api/v1/self-services/human-resource/attendance-corrections/filters/metadata` |
| `employee-self-service.js:106` | `GET` | `/v1/hr/attendance/dailies` | `A` | `GET api/v1/self-services/human-resource/attendance` |
| **Pelatihan & Sertifikasi `/hr/learning/training-records`** | | | | |
| `learning-development-service.js:23` | `GET` | `/v1/hr/workforce-profile/training-records` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/training-records` |
| `learning-development-service.js:30` | `GET` | `/v1/hr/workforce-profile/certifications` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications` |
| **Penyesuaian Cuti `/hr/leave/adjustments`** | | | | |
| `leave-admin-service.js:23` | `GET` | `/v1/hr/leave/balance-adjustments` | `A` | `GET api/v1/corporate/human-resource/leave/adjustments` |
| `leave-admin-service.js:30` | `POST` | `/v1/hr/leave/balance-adjustments` | `A` | `POST api/v1/corporate/human-resource/leave/adjustments` |
| `leave-admin-service.js:34` | `GET` | `/v1/hr/leave/requests` | `C` | Tidak ada daftar permohonan cuti lintas-pegawai untuk admin |
| **Rekonsiliasi Lembur `/hr/overtime/reconciliation`** | | | | |
| `overtime-admin-service.js:23` | `GET` | `/v1/hr/overtime/requests` | `C` | Tidak ada daftar permohonan lembur admin; terdekat `GET api/v1/corporate/human-resource/overtime-management/realizations` |
| `overtime-admin-service.js:30` | `GET` | `/v1/hr/overtime/calculations` | `C` | Tidak ada perhitungan lembur sebagai daftar |
| **Restitusi & Cicilan `/hr/payroll/restitution-installment`** | | | | |
| `payroll-integration-service.js:24` | `GET` | `/v1/hr/payroll/restitutions` | `C` | Tidak ada daftar; hanya `GET api/v1/corporate/human-resource/payroll-restitutions/by-source/{sourceId}` |
| `payroll-integration-service.js:31` | `POST` | `/v1/hr/payroll/restitutions` | `A` | `POST api/v1/corporate/human-resource/payroll-restitutions` |
| `payroll-integration-service.js:37` | `GET` | `/v1/hr/payroll/installment-deductions` | `C` | Tidak ada daftar potongan cicilan |
| `payroll-integration-service.js:44` | `POST` | `/v1/hr/payroll/installment-deductions` | `D` | `POST api/v1/corporate/human-resource/payroll-runs/{runId}/installment-deductions` — butuh `runId` |
| **Penilaian Kinerja `/hr/performance/reviews`** | | | | |
| `performance-management-service.js:23` | `GET` | `/v1/hr/workforce-profile/performance-reviews` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/performance-reviews` |
| `performance-management-service.js:29` | `GET` | `/v1/hr/workforce-profile/performance-reviews/{}` | `B` | Hanya `GET api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/performance-reviews/{id}` |
| **Roster `/hr/scheduling/roster`** | | | | |
| `roster-scheduling-service.js:24` | `GET` | `/v1/hr/scheduling/roster-periods` | `A` | `GET api/v1/corporate/human-resource/scheduling-management/roster-periods` |
| `roster-scheduling-service.js:31` | `POST` | `/v1/hr/scheduling/roster-periods` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/roster-periods` |
| `roster-scheduling-service.js:35` | `POST` | `/v1/hr/scheduling/roster-periods/{}/publish` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/publish` |
| `roster-scheduling-service.js:39` | `POST` | `/v1/hr/scheduling/roster-periods/{}/lock` | `D` | Tidak ada `lock`; terdekat `POST .../roster-periods/{id}/close` |
| `roster-scheduling-service.js:45` | `GET` | `/v1/hr/scheduling/on-call-assignments` | `A` | `GET api/v1/corporate/human-resource/scheduling-management/on-call-assignments` |
| `roster-scheduling-service.js:52` | `POST` | `/v1/hr/scheduling/on-call-assignments` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/on-call-assignments` |
| `roster-scheduling-service.js:56` | `PUT` | `/v1/hr/scheduling/on-call-assignments/{}/confirm` | `C` | Tidak ada `confirm` |
| `roster-scheduling-service.js:60` | `PUT` | `/v1/hr/scheduling/on-call-assignments/{}/activate` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/on-call-assignments/{id}/activate` (FE memakai `PUT`) |
| `roster-scheduling-service.js:66` | `GET` | `/v1/hr/scheduling/shift-replacements` | `A` | `GET api/v1/corporate/human-resource/scheduling-management/shift-replacements` |
| `roster-scheduling-service.js:73` | `POST` | `/v1/hr/scheduling/shift-replacements` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/shift-replacements` |
| `roster-scheduling-service.js:77` | `PUT` | `/v1/hr/scheduling/shift-replacements/{}/approve` | `A` | `POST api/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/approve` (FE memakai `PUT`) |
| **Enam layar Administrasi Kepegawaian `/hr/workforce-core/*`** | | | | |
| `workforce-core-cross-employee-service.js:24` | `GET` | `/v1/hr/workforce-core/cross-employee/employee-profile-changes` | `A` | `GET api/v1/corporate/human-resource/employee-profile-changes` |
| `workforce-core-cross-employee-service.js:33` | `GET` | `/v1/hr/workforce-core/cross-employee/organization-assignments` | `A` | `GET api/v1/corporate/human-resource/workforce-core/organization-assignments` |
| `workforce-core-cross-employee-service.js:42` | `GET` | `/v1/hr/workforce-core/cross-employee/position-assignments` | `A` | `GET api/v1/corporate/human-resource/workforce-core/position-assignments` |
| `workforce-core-cross-employee-service.js:51` | `GET` | `/v1/hr/workforce-core/cross-employee/manager-assignments` | `A` | `GET api/v1/corporate/human-resource/workforce-core/manager-assignments` |
| `workforce-core-cross-employee-service.js:60` | `GET` | `/v1/hr/workforce-core/cross-employee/employment-histories` | `A` | `GET api/v1/corporate/human-resource/workforce-core/employment-histories` |
| `workforce-core-cross-employee-service.js:69` | `GET` | `/v1/hr/workforce-core/cross-employee/salary-assignments` | `A` | `GET api/v1/corporate/human-resource/workforce-core/salary-assignments` |


Ringkasan: `A` 34 · `B` 6 · `C` 11 · `D` 5 · total 56. Pemetaan dibuat dengan membandingkan setiap
pemanggilan `InstanceAxios` pada `src/lib/services/hr/*.js` terhadap gabungan `[Route]` dan
`[Http*]` seluruh controller backend, lalu diperiksa manual untuk golongan `B`, `C`, dan `D`.

## Lampiran B — Query pemeriksaan data untuk pelapor

Skill ini tidak menjalankan query. Bila pelapor ingin memastikan keadaan data sendiri, query
**baca-saja** berikut dapat dijalankan pada database pengembangan:

```sql
-- Jumlah baris data Workforce Core
SELECT 'MstWorkforceProfile' AS tabel, count(*) FROM "MstWorkforceProfile"
UNION ALL SELECT 'WfpOrganizationAssignment', count(*) FROM "WfpOrganizationAssignment"
UNION ALL SELECT 'WfpPositionAssignment', count(*) FROM "WfpPositionAssignment"
UNION ALL SELECT 'WfpManagerAssignment', count(*) FROM "WfpManagerAssignment"
UNION ALL SELECT 'WfpSalaryAssignment', count(*) FROM "WfpSalaryAssignment"
UNION ALL SELECT 'WfpEmploymentHistory', count(*) FROM "WfpEmploymentHistory"
UNION ALL SELECT 'TrxEmployeeProfileChangeRequest', count(*) FROM "TrxEmployeeProfileChangeRequest";

-- Profil workforce yang tidak terhubung ke pegawai, dokter, maupun pengguna eksternal
SELECT p."ProfileCode", p."DisplayName", p."UserType", p."CreateDateTime"
FROM "MstWorkforceProfile" p
WHERE NOT EXISTS (SELECT 1 FROM "MstEmployee" e WHERE e."WorkforceProfileId" = p."Id")
  AND NOT EXISTS (SELECT 1 FROM "MstDoctor" d WHERE d."WorkforceProfileId" = p."Id")
  AND NOT EXISTS (SELECT 1 FROM "MstExternalUser" x WHERE x."WorkforceProfileId" = p."Id")
ORDER BY p."CreateDateTime";
```

Hasil yang diharapkan pada 2 November 2026: 55, 55, 0, 0, 0, 1, 0, dan 14 baris profil yatim.
