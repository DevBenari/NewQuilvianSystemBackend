# Planning — Menu Sidebar Tanpa Duplikat Layanan Mandiri (lanjutan Ruang Kerja Saya)

| Field | Nilai |
| --- | --- |
| Tanggal | 24 November 2026 |
| Modul | Human Resource — Layanan Mandiri Pegawai (ESS) dan menu SDM |
| Repository terdampak | `QuilvianSystemFrontendDev` (frontend saja; backend tidak diubah) |
| Status | Disetujui dan diimplementasikan 24 November 2026 — hasil dan penyimpangan dari rencana ada di bagian 10 |
| Alur kerja | Analisis → Planning → Implementasi → Testing |
| Dokumen terkait | `PLANNING_RUANG_KERJA_SAYA_LAYANAN_MANDIRI.md` (workspace `/self-services`), `PLANNING_PENYESUAIAN_MENU_SDM_BERDASARKAN_ANALISIS_BISNIS.md` (susunan menu SDM) |

---

## 1. Latar belakang

Ruang Kerja Saya (`/self-services`) sudah menjadi pintu tunggal layanan mandiri pegawai. Grup sidebar
"Layanan Mandiri" dan butir "Pengajuan & Status Lembur (ESS)" sudah dihapus pada pekerjaan sebelumnya.
Analisis 24 November 2026 menemukan **8 butir menu SDM** yang fungsinya masih dobel dengan tab workspace:

| Butir menu | Lokasi (`src/utils/menu-sidebar/menu-items.jsx`) | Izin sekarang | Tab workspace yang sama |
| --- | --- | --- | --- |
| Dasbor Saldo & Cuti Saya (`corporateLeaveMyBalance`) | baris 1319, grup Cuti & Perizinan | tanpa izin (semua akun) | Cuti |
| Formulir Pengajuan Cuti (`corporateLeaveRequest`) | baris 1325, grup Cuti & Perizinan | tanpa izin (semua akun) | Cuti |
| Evaluasi Atasan (`corporatePerformanceAppraisals`) | baris 1821, Penilaian Kerja | `PerformanceAppraisal`: `ViewOwn` / `ViewSubordinates` | Evaluasi Kinerja |
| Sanggah Banding Nilai Kinerja (`corporatePerformanceDisputes`) | baris 1878, Penilaian Kerja | `PerformanceCalibration`: `SubmitDispute` / `ManageDispute` | Sanggah Nilai |
| Pengaduan Staf & Whistleblowing (`corporateRelationsGrievances`) | baris 1999, Hubungan Industrial | `EmployeeGrievance`: `Submit` / `SubmitWhistleblower` / `ViewOwn` / `Mediate` / `WhistleblowerReview` | Pengaduan |
| Portofolio 50 SKP Plataran Sehat (`corporateSkpTracker`) | baris 1784, Pelatihan | `PlataranSehatSkp:Read` | Portofolio SKP |
| Dashboard Rapor Kinerja Klinis (OPPE) (`corporateOppeDashboard`) | baris 1888, Penilaian Kerja | `OppeOngoingEvaluation:ViewEvaluation` | Rapor OPPE |
| Dasbor Pelatihan & Radar (`corporateLearningDashboard`) | baris 1712, Pelatihan | `MandatoryHospitalTraining` / `TrainingSession` / `TrainingCertificate` / `AnnualTrainingPlan`: `Read` | Dasbor Pelatihan |

Dua butir pertama murni layanan mandiri. Enam sisanya memakai view yang sama untuk karyawan dan admin
SDM; view-nya sudah menyesuaikan isi menurut izin (`useStrictPermissions`), sehingga yang perlu
diubah hanya siapa yang melihat butir menunya.

### 1.1 Temuan utama

| # | Jenis | Temuan | Bukti | Dampak |
| ---: | --- | --- | --- | --- |
| 1 | Rusak | Form "Pengajuan Cuti Baru" di tab Cuti workspace hanya berisi tanggal dan alasan, tanpa jenis cuti. Yang terkirim `leaveTypeId: ""`, padahal backend mewajibkan GUID `LeaveTypeId`. Form juga tidak memanggil `/submit`, tanpa lampiran surat dokter dan tanpa pegawai pengganti | `src/components/view/self-services/human-resource/leave/leave-employee-view.jsx` baris 43, 129, 265–268; `Areas/Corporate/HumanResource/LeaveManagement/DTOs/LeaveRequestDtos.cs` baris 75 dan 146–151 | Pengajuan cuti dari workspace ditolak; tombol "Ajukan Cuti" di panel profil mengarah ke form ini |
| 2 | Rusak | Tabel dan kartu saldo di tab yang sama membaca nama field yang tidak ada di DTO: saldo diharapkan berupa array padahal backend mengirim `LeaveBalancePagedResponse { items }`; status dibaca dari `requestStatus` (DTO: `leaveRequestStatus`); durasi dari `totalDays` (DTO: `requestedDays`); paginasi dari `totalCount`/`totalPages` (DTO: `totalData`/`totalPage`) | `leave-employee-view.jsx` baris 58–66, 86–93, 160, 172; `LeaveEntitlementBalanceQueryDtos.cs` baris 299–306; `LeaveRequestDtos.cs` baris 202–236 | Saldo selalu "0 Hari", status selalu "Draft", durasi selalu "1 Hari" |
| 3 | Rusak (bila dipakai di workspace) | View lama `my-leave-balance-view.jsx` memakai endpoint korporat `GET leave/balances/my` (`LeaveBalanceLedger:Read`), `GET leave/requests/my` (`LeaveRequest:ViewSelf`), dan `POST requests/{id}/cancel` (`LeaveRequest:CancelSelf`), bukan izin `My*` milik layanan mandiri | `src/lib/services/hr/leave-service.js` baris 7, 30–97; `LeaveBalanceLedgerController.cs` baris 50–54; `LeaveRequestEssController.cs` baris 54–56, 123–125 | Pegawai yang hanya punya izin `My*` mendapat 403 bila view ini ditanam di workspace |
| 4 | Kurang ideal | Dua butir cuti tanpa `requiredPermission` ada di grup admin "Cuti & Perizinan" | `menu-items.jsx` baris 1318–1329 | Tampil untuk semua akun, termasuk akun non-pegawai |
| 5 | Kurang ideal | Izin butir dual-audience memuat aksi pribadi (`ViewOwn`, `SubmitDispute`, `Submit`, `PlataranSehatSkp:Read`, `ViewEvaluation`) | `menu-items.jsx` baris 1784–1791, 1821–1829, 1878–1896, 1999–2010 | Setiap karyawan melihat menu admin SDM yang isinya sudah ada di workspace |
| 6 | Kurang ideal | Tautan lama membawa pegawai keluar dari workspace: "Gunakan untuk Cuti" di Saldo Libur Pengganti → `/hr/leave/request?jenisCuti=CTO`; selesai revisi permohonan → `/hrd/self-service/my-requests`; kembali dari detail slip gaji milik sendiri → `/payroll/my-payslips` | `src/lib/constants/hr/overtime/overtime-constants.js` baris 102; `src/lib/constants/hr/approvals/approval-constants.js` baris 13; `src/lib/hooks/payroll/use-payroll-payslip-detail.js` baris 81 | Alur terputus; pegawai berpindah dari workspace ke halaman lama |
| 7 | Kurang ideal | Tab "Dasbor Pelatihan" di workspace sebenarnya dasbor admin Diklat (radar sertifikat seluruh RS, sesi, ikatan dinas) dan tab itu tidak punya syarat izin | `src/components/view/hr/learning/learning-dashboard-view.jsx` baris 1–4, 51–54; `src/lib/constants/self-services/self-service-workspace-constants.js` baris 102 | Pegawai tanpa izin baca Diklat melihat layar kosong |

Batasan yang dihormati:

- Unit test HR-08 (`tests/unit/hr-master-data-sidebar-grouping.test.mjs` baris 384–420) mewajibkan
  "Evaluasi Atasan" tetap di lima urutan awal Penilaian Kerja dan `corporateOppeDashboard` tetap di
  Penilaian Kerja. `PLANNING_PENYESUAIAN_MENU_SDM...` baris 280 juga mempertahankan Evaluasi Atasan.
  Karena itu butir dual-audience **tidak dihapus, hanya izinnya dipersempit**. Test itu membaca
  struktur menu, bukan izin.
- `tests/unit/bpjs-feature-deactivation.test.mjs` baris 98–106 memakai fixture `corporateMyCobClaims`,
  sehingga entri itu di `BPJS_ONLY_MENUS` dibiarkan.

### 1.2 Contoh di rumah sakit

**Hari ini:** Perawat Rina dari bangsal penyakit dalam membuka sidebar. Selain "Ruang Kerja Saya",
di modul SDM ia masih melihat "Dasbor Saldo & Cuti Saya", "Formulir Pengajuan Cuti", "Evaluasi
Atasan", "Sanggah Banding", dan "Pengaduan Staf". Saat anaknya sakit, Rina menekan "Ajukan Cuti" di
panel profil. Ia masuk tab Cuti, tidak menemukan pilihan Cuti Sakit, saldonya tertulis 0 hari, dan
pengajuannya gagal. Rina akhirnya mencari form lain di menu SDM.

**Sesudah:** Sidebar Rina hanya berisi "Ruang Kerja Saya" untuk urusan pribadi. "Ajukan Cuti" membuka
form lengkap di tab Cuti: jenis cuti dengan saldo, perhitungan hari kerja dari server, lampiran
surat dokter, dan pegawai pengganti. Sesudah dikirim, Rina kembali ke daftar cutinya di workspace
dan saldonya sudah terpotong cadangan. Kepala ruangan tetap melihat "Evaluasi Atasan" untuk menilai
stafnya; staf SDM tetap melihat "Sanggah Banding" dan "Pengaduan Staf" untuk memprosesnya.

---

## 2. Tujuan

Setiap layanan pribadi pegawai hanya punya satu pintu, yaitu Ruang Kerja Saya. Menu SDM hanya
berisi layar kerja atasan dan petugas SDM. Tab Cuti workspace diperbaiki lebih dulu supaya
menyembunyikan menu cuti tidak menghilangkan satu-satunya form cuti yang berfungsi.

---

## 3. Gambaran layar

Tab Cuti di Ruang Kerja Saya (`/self-services?section=leave&tab=leave`):

```text
┌─ Cuti Saya ─────────────────────────────────────────────── [+ Ajukan Cuti] ┐
│ Cuti Tahunan 2026      Cuti Sakit 2026      Libur Pengganti (CTO)           │
│ Tersedia 8 hari        Tersedia 12 hari     Tersedia 1 hari                 │
│ Menunggu 2 · Dipakai 2 Menunggu 0 · Dip. 0  Menunggu 0 · Dipakai 0          │
├──────────────────────────────────────────────────────────────────────────────┤
│ Riwayat Permohonan Cuti                                                      │
│ No. / Jenis           Tanggal               Durasi   Status          Aksi    │
│ LV-2611-0041          02–03 Des 2026        2 hari   Menunggu        Batalkan│
│ Cuti Tahunan                                         Persetujuan             │
│ LV-2610-0017          14 Okt 2026           1 hari   Disetujui               │
│ Cuti Sakit                                                                   │
│                                          ‹ 1 2 ›                             │
└──────────────────────────────────────────────────────────────────────────────┘
```

Sesudah menekan "+ Ajukan Cuti" (`...&tab=leave&form=baru`, opsional `&jenisCuti=CTO`), tab yang sama
menampilkan form lengkap yang sudah ada (`leave-request-view.jsx`):

```text
┌─ Ajukan Cuti ───────────────────────────────────────── [Kembali ke Cuti Saya] ┐
│ Jenis Cuti *          [ Cuti Sakit — tersedia 12 hari            v ]          │
│ Tanggal               [ 25 Nov 2026 ] s/d [ 26 Nov 2026 ]  [ ] Setengah hari  │
│ Pegawai Pengganti *   [ Pilih pegawai pengganti                  v ]          │
│ Nomor selama cuti     [ 0812xxxxxxx ]                                         │
│ Alasan *              [ Anak demam, perlu pendampingan            ]           │
│ Surat Keterangan Dokter *  [ Pilih berkas PDF/JPG/PNG ]                       │
├─ Perhitungan Server ─────────────────────────────────────────────────────────┤
│ Durasi 2 hari · Libur dikecualikan 0 · Saldo sebelum 12 · sesudah 10          │
│                                                     [Batal]  [Ajukan Cuti]   │
└───────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Sumber |
| --- | --- |
| Kartu saldo | `balanceCards` dari `GET leave/balances/summary` (self-service) |
| Riwayat | `GET leave/requests` (self-service), dinormalisasi dengan `normalizeLeaveRequestItem`, label status dari `LEAVE_REQUEST_STATUS` |
| Form | `leave-request-view.jsx` apa adanya, hanya tujuan tombol kembali/selesai yang dapat diatur lewat prop |

---

## 4. Keputusan

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | Letak form cuti lengkap di workspace | Mode di dalam tab Cuti (`&form=baru`) yang dibuka tombol "+ Ajukan Cuti"; tab bar tetap 4 tab | Tab baru "Ajukan Cuti" di bagian Cuti & Lembur (tab bar jadi 5) |
| 2 | Sumber saldo dan riwayat di tab Cuti | Endpoint self-service (`MyLeaveBalance:Read`, `MyLeaveRequest:*`), view `leave-employee-view.jsx` ditulis ulang. **Ini mengoreksi rekomendasi analisis** yang menyarankan menanam `my-leave-balance-view.jsx` (temuan 3: view itu memakai izin korporat) | Menanam `my-leave-balance-view.jsx` dan menambahkan `LeaveBalanceLedger:Read` + `LeaveRequest:ViewSelf/CancelSelf` ke peran pegawai |
| 3 | Dua butir menu cuti | Dihapus dari `menu-items.jsx` | Diberi izin yang tidak dimiliki siapa pun (menyisakan butir mati) |
| 4 | Halaman lama `/hr/leave/my-balance` dan `/hr/leave/request` | Tetap hidup (bookmark dan tautan lama aman); tidak ada lagi tautan aktif dari workspace ke sana | Dialihkan (`redirect`) ke workspace |
| 5 | Butir dual-audience | Dipertahankan, izinnya dipersempit ke aksi atasan/admin (tabel 5.2) | Dihapus (melanggar test HR-08 dan keputusan menu SDM) |
| 6 | Tab "Dasbor Pelatihan" di workspace | Diberi syarat izin yang sama dengan menu Diklat (salah satu `Read` dari empat resource pelatihan) | Dibiarkan tampil untuk semua pegawai |
| 7 | Tautan lama | Dialihkan ke deep link workspace: "Gunakan untuk Cuti", selesai revisi permohonan, kembali dari detail slip gaji milik sendiri | Dibiarkan |
| 8 | Panel ringkasan workspace sesudah ajukan/batal cuti | Dimuat ulang lewat `refreshAll` yang sudah ada di hook workspace | Dibiarkan sampai halaman dimuat ulang |

---

## 5. Isi

### 5.1 Tab Cuti workspace

| No | Fitur | Isi | Endpoint | Izin |
| ---: | --- | --- | --- | --- |
| 1 | Kartu saldo | Satu kartu per jenis cuti: tersedia, menunggu, dipakai; status terkunci bila `isLocked` | `GET balances/summary` | `MyLeaveBalance:Read` |
| 2 | Riwayat permohonan | Nomor, jenis, tanggal, durasi (`requestedDays`), status, paginasi `totalData`/`totalPage` | `GET requests?pageNumber&pageSize` | `MyLeaveRequest:Read` (syarat tab) |
| 3 | Batalkan / tarik | Tombol muncul bila `availableActions` memuat `Cancel` atau `Withdraw`; modal dengan alasan wajib | `POST requests/{id}/cancel` | `MyLeaveRequest:Cancel` |
| 4 | Ajukan cuti | Tombol "+ Ajukan Cuti" membuka mode form; form lama dipakai ulang | `GET requests/balances/options`, `POST requests/calculate`, `POST requests`, `POST requests/{id}/attachments`, `POST requests/{id}/submit` | `MyLeaveRequest:Create` (tombol), `Update`, `Submit` |
| 5 | Deep link | `form=baru` dan `jenisCuti=<kode>` dibaca dari alamat; berpindah tab membuang keduanya (perilaku hook sekarang) | — | — |

Aturan validasi backend yang ditiru di frontend:

- Pengajuan: `LeaveTypeId` wajib; `Reason` wajib, maksimal 2000 karakter; `ContactNumberDuringLeave`
  maksimal 50; `HandoverNotes` maksimal 2000. Durasi, saldo, tabrakan jadwal, kewajiban lampiran, dan
  kewajiban pengganti dihitung server lewat `calculate` (sudah ditangani form lama).
- Pembatalan: `Reason` wajib, maksimal 1000 karakter. Hanya untuk status `Draft`, `NeedRevision`,
  `Submitted`, `WaitingApproval`. Backend memberi aksi `Withdraw` bila workflow sedang berjalan; keduanya
  memakai endpoint yang sama. Cuti yang sudah disetujui dibatalkan lewat tab "Pembatalan Cuti" yang
  sudah ada.

### 5.2 Menu sidebar

| Butir | Sekarang | Sesudah | Yang masih melihat |
| --- | --- | --- | --- |
| Dasbor Saldo & Cuti Saya | tanpa izin | **dihapus** | — (tab Cuti workspace) |
| Formulir Pengajuan Cuti | tanpa izin | **dihapus** | — (tab Cuti workspace) |
| Evaluasi Atasan | `ViewOwn` / `ViewSubordinates` | `PerformanceAppraisal:ViewSubordinates` | Atasan dan penilai |
| Sanggah Banding Nilai Kinerja | `SubmitDispute` / `ManageDispute` | `PerformanceCalibration:ManageDispute` | Petugas SDM pengelola sanggahan |
| Pengaduan Staf & Whistleblowing | lima aksi | `EmployeeGrievance:Mediate` / `WhistleblowerReview` | Mediator dan peninjau whistleblowing |
| Portofolio 50 SKP Plataran Sehat | `PlataranSehatSkp:Read` | `PlataranSehatSkp:Verify` / `Sync` | Verifikator Diklat/Komite |
| Dashboard Rapor Kinerja Klinis (OPPE) | `ViewEvaluation` | `OppeOngoingEvaluation:CalculateEvaluation` / `FinalizeEvaluation` | Komite Medik/Keperawatan |
| Dasbor Pelatihan & Radar | empat `Read` | tidak berubah | Admin Diklat |

Tidak disentuh: Pendelegasian Wewenang (Plh), Hasil Jasa Saya, Data Induk Pegawai, Pengunduran Diri
(layar admin), dan entri `corporateMyCobClaims` di `BPJS_ONLY_MENUS`.

### 5.3 Aturan tampil

| Kondisi | Yang terjadi |
| --- | --- |
| Pegawai biasa (hanya izin `My*` dan aksi pribadi) | Sidebar SDM tidak lagi memuat 7 butir di atas; semua urusan lewat Ruang Kerja Saya |
| Kepala ruangan (`ViewSubordinates`) | Tetap melihat "Evaluasi Atasan" di SDM; penilaian dirinya sendiri lewat workspace |
| Petugas SDM/Komite | Tetap melihat butir yang sesuai izinnya |
| Grup "Cuti & Perizinan" | Tetap ada karena butir admin (Persetujuan Cuti Staf, Manajemen Saldo Staf, dst.) masih ada; grup kosong otomatis disembunyikan `filterMenuItemsByPermission` |
| Tidak punya `MyLeaveRequest:Create` | Tombol "+ Ajukan Cuti" tidak tampil; `form=baru` di alamat diabaikan |
| Tidak punya `MyLeaveBalance:Read` | Kartu saldo diganti keterangan singkat; riwayat tetap tampil |
| Tidak punya izin baca Diklat | Tab "Dasbor Pelatihan" tidak tampil di workspace |

---

## 6. File yang dibuat dan diubah

Semua di `QuilvianSystemFrontendDev`. Backend tidak diubah; semua endpoint sudah ada.

### 6.1 File baru

| File | Isi |
| --- | --- |
| `tests/unit/self-service-menu-dedup.test.mjs` | Regresi: dua butir cuti tidak ada lagi di menu; lima butir dual-audience tidak memuat aksi pribadi; tab Cuti workspace tidak lagi mengirim `leaveTypeId` kosong |

### 6.2 File yang diubah

| File | Perubahan |
| --- | --- |
| `src/components/view/self-services/human-resource/leave/leave-employee-view.jsx` | Ditulis ulang: kartu saldo dari `balances/summary`, riwayat dari `requests` dengan field DTO yang benar, aksi batal, mode form yang menanam `LeaveRequestView`; menerima prop `onChanged` |
| `src/components/view/hr/leave/leave-request-view.jsx` | Prop opsional `backRoute` (bawaan `LEAVE_ROUTES.myBalance`), `backLabel`, dan `onSubmitted`; perilaku halaman `/hr/leave/request` tidak berubah |
| `src/lib/services/hr/employee-self-service.js` | Fungsi baru `cancelLeaveRequest(id, payload)` → `POST /v1/self-services/human-resource/leave/requests/{id}/cancel` |
| `src/lib/constants/self-services/self-service-workspace-constants.js` | `buildSelfServiceWorkspaceRoute(section, tab, extra)` menerima parameter tambahan; jalan pintas `leaveRequest`; syarat izin tab `learning` |
| `src/components/view/self-services/workspace/self-service-section-content.jsx` | Meneruskan `onChanged` ke tab Cuti |
| `src/components/view/self-services/workspace/self-service-workspace-view.jsx` | Mengirim `refreshAll` sebagai `onChanged` |
| `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` | "Ajukan Cuti" ke jalan pintas `leaveRequest` (langsung membuka form) |
| `src/utils/menu-sidebar/menu-items.jsx` | Hapus dua butir cuti; persempit izin lima butir (tabel 5.2); berkas CRLF dijaga |
| `src/lib/constants/hr/overtime/overtime-constants.js` | `leaveRequest` → deep link workspace `form=baru&jenisCuti=CTO` |
| `src/lib/constants/hr/approvals/approval-constants.js` | `myRequests` → `SELF_SERVICE_SHORTCUTS.myRequests` |
| `src/lib/hooks/payroll/use-payroll-payslip-detail.js` | Tombol kembali untuk slip milik sendiri → tab Slip Gaji workspace (`PAYROLL_ROUTES.myPayslips` tetap, karena dipakai untuk alamat detail) |

---

## 7. Urutan pengerjaan

### Fase 1 — Perbaiki tab Cuti workspace

1. Tambah `cancelLeaveRequest` di `employee-self-service.js`.
2. Tambah prop `backRoute`, `backLabel`, `onSubmitted` di `leave-request-view.jsx` dengan nilai bawaan
   yang sama dengan perilaku sekarang.
3. Perluas `buildSelfServiceWorkspaceRoute` dengan parameter ketiga, tambah jalan pintas `leaveRequest`.
4. Tulis ulang `leave-employee-view.jsx` (saldo, riwayat, batal, mode form) memakai komponen dasar
   dan pola tabel yang sudah dipakai tab lain (sel dua baris pakai `<div>`).
5. Teruskan `refreshAll` → `onChanged`; arahkan "Ajukan Cuti" di panel profil ke `leaveRequest`.

Endpoint yang dipakai:

#### Self Services / Human Resource / Leave Balance

Base URL: `api/v1/self-services/human-resource/leave/balances`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | `/summary` | Ringkasan dan kartu saldo per jenis cuti | `MyLeaveBalance:Read` |

#### Self Services / Human Resource / Leave Request

Base URL: `api/v1/self-services/human-resource/leave/requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | `/` | Riwayat permohonan cuti milik sendiri (paged) | `MyLeaveRequest:Read` |
| GET | `/balances/options` | Pilihan jenis cuti beserta saldo | `MyLeaveRequest:Read` |
| POST | `/calculate` | Hitung durasi, saldo sesudah, galat dan peringatan | `MyLeaveRequest:Read` |
| POST | `/` | Simpan draf permohonan | `MyLeaveRequest:Create` |
| POST | `/{id}/attachments` | Unggah surat dokter / dokumen pendukung | `MyLeaveRequest:Update` |
| POST | `/{id}/submit` | Ajukan ke atasan | `MyLeaveRequest:Submit` |
| POST | `/{id}/cancel` | Batalkan atau tarik permohonan yang belum disetujui | `MyLeaveRequest:Cancel` |

### Fase 2 — Rapikan menu sidebar

1. Hapus butir `corporateLeaveMyBalance` dan `corporateLeaveRequest`; pastikan impor ikon yang tidak
   terpakai ikut dibersihkan.
2. Ganti `requiredPermission` lima butir sesuai tabel 5.2.
3. Tambah `tests/unit/self-service-menu-dedup.test.mjs`.

### Fase 3 — Tautan lama dan tab Dasbor Pelatihan

1. Ubah `overtime-constants.js` (`leaveRequest`), `approval-constants.js` (`myRequests`), dan tombol
   kembali di `use-payroll-payslip-detail.js`.
2. Beri syarat izin tab `learning` di konstanta workspace.

### Fase 4 — Testing dan hasil

Menjalankan bagian 8, lalu menambahkan bagian 10 "Hasil implementasi" pada dokumen ini.

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | ESLint semua file yang berubah dibanding HEAD | Tanpa error atau warning baru |
| 2 | Unit test dibanding baseline HEAD | Tidak ada kegagalan baru; HR-08, BPJS, `workforce-core-cross-employee`, ESS-002/003 tetap lulus; test baru lulus |
| 3 | `npm run build` | Berhasil |
| 4 | Browser headed — tab Cuti | Kartu saldo menampilkan angka dari server; status dan durasi riwayat sesuai DTO; paginasi bekerja |
| 5 | Browser — ajukan cuti (POST dicegat `page.route`) | Jenis cuti bisa dipilih; payload `POST requests` memuat `leaveTypeId` GUID dan `reason`; `submit` terpanggil; kembali ke daftar Cuti di workspace |
| 6 | Browser — batalkan (POST dicegat) | Modal menolak alasan kosong; payload `cancel` memuat `reason` |
| 7 | Browser — deep link | `?section=leave&tab=leave&form=baru&jenisCuti=CTO` memilih Libur Pengganti otomatis; "Ajukan Cuti" di panel profil langsung membuka form; "Gunakan untuk Cuti" di Saldo Libur Pengganti mendarat di form workspace |
| 8 | Browser — sidebar | Akun pegawai tidak melihat 7 butir; akun penyetuju/SDM masih melihat butir sesuai izinnya |
| 9 | Lebar ponsel 390 px | Tab Cuti mode daftar dan mode form tanpa scroll horizontal (`diag-mobile`) |
| 10 | Regresi halaman lama | `/hr/leave/request` tetap bekerja dan kembali ke `/hr/leave/my-balance`; detail slip gaji milik admin tetap kembali ke daftar slip admin |

Uji browser tidak menulis database: semua POST dicegat. Sandi akun uji dibaca saat runtime.

---

## 9. Di luar cakupan

| Hal | Alasan |
| --- | --- |
| Perubahan backend | Semua endpoint dan izin yang dibutuhkan sudah ada |
| Buku besar mutasi saldo di workspace | Endpoint self-service ada (`GET balances/{id}/ledger`), tetapi bukan bagian dari menghapus duplikat; bisa menjadi pekerjaan berikutnya |
| Melanjutkan draf lama dari riwayat | Form lama selalu membuat draf baru; draf tertinggal bisa dibatalkan dari riwayat |
| Menghapus halaman `/hr/leave/my-balance`, `/hr/leave/request`, dan `my-leave-balance-view.jsx` | Keputusan 4: halaman tetap hidup untuk tautan lama |
| `PAYROLL_ROUTES.myPayslips` dan `RESTITUTION_ROUTES.essClaims` | Yang pertama dipakai membentuk alamat detail slip gaji; yang kedua tidak dipakai di mana pun |
| Plh, Hasil Jasa Saya, Data Induk Pegawai, Pengunduran Diri (admin), `BPJS_ONLY_MENUS` | Bukan duplikat workspace, atau dipakai fixture test |
| Mengatur izin peran pegawai di Akses Role | Konfigurasi data, bukan kode; rencana ini hanya memakai izin yang sudah ada |

---

## 10. Hasil implementasi (24 November 2026)

Semua keputusan bagian 4 dipakai sesuai rekomendasi. Backend tidak diubah. Tidak ada commit, migration,
atau penulisan database; uji browser mencegat semua POST.

### 10.1 Penyimpangan dari rencana

| Hal | Rencana | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Peta status riwayat cuti | `LEAVE_REQUEST_STATUS` apa adanya | `MY_LEAVE_REQUEST_STATUS` baru di `self-service-request-constants.js` = `LEAVE_REQUEST_STATUS` + `WaitingApproval`, `Taken`, `Expired` | Tiga status backend (`LeaveRequestValueConstants.Status`) belum punya label; peta bersama tidak diubah |
| Kolom tabel riwayat | Nomor, jenis, tanggal, durasi, diajukan, status, aksi | Empat kolom: Cuti (jenis + nomor), Tanggal (+ durasi), Status (+ tanggal diajukan), Aksi | Area tengah workspace hanya ±566 px di layar 1440 px; dengan 6 kolom, Status dan tombol Batalkan terdorong keluar layar |
| Lebar minimal tabel | — | Kelas `.compactTable table { min-width: 520px }` di `src/style/self-services/self-service-workspace.module.css` | `DataTable` bersama memaksa `min-width: max(840px, 100%)`; tabel kini 566/568 px (1440) dan 666/668 px (1280) tanpa geser |
| Kirim draf dari riwayat | Tidak disebut | Tombol **Kirim** pada baris `Draft` yang punya aksi `Submit` (`POST requests/{id}/submit`, `MyLeaveRequest:Submit`) | Pola layanan mandiri lain: draf yang gagal terkirim bisa dikirim ulang dari daftar |
| Halaman `/self-services/human-resource/leave` | Tidak disebut | `page.jsx` dibungkus `Suspense` | View kini membaca `useSearchParams` (mode form) |
| `onChanged` | `refreshAll` | Sesuai rencana; hanya diteruskan ke tab Cuti (`TABS_WITH_CHANGE_CALLBACK`) | Tab lain belum membutuhkan |
| Akun uji pegawai | Akun pegawai biasa | `bukanAtasan_nonAdmin` | Akun `bawahan` (UAT pkb) ditolak login 401 "Invalid email or password" |

### 10.2 Hasil pengujian

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 14 berkas JS/JSX yang berubah + test baru | 0 error, 5 warning; kelimanya identik di versi HEAD (`user-profile-sidebar.jsx` baris 435/504/512/668, `use-payroll-payslip-detail.js` `set-state-in-effect`) — tidak bertambah |
| Pemeriksa CSS (`check-css-usage.mjs`) pada `leave-employee-view.jsx` | 0 temuan |
| Unit test penuh dibanding HEAD | 5378 test, 104 gagal sekarang vs 140 di HEAD; 4 test baru `ESS-DEDUP-01..04` lulus; HR-08, BPJS, ESS-002/003 tetap lulus. Dua "calon regresi" adalah test isi-backend lama (finalisasi tender, gerbang BPJS backend) yang membaca folder backend dan tidak berjalan di salinan HEAD — tidak terkait perubahan ini |
| `npm run build` | `✓ Compiled successfully`, exit 0 (dijalankan di samping `next dev`) |
| Uji browser headed (CDP 9338), 15 kasus | **15 PASS, 0 FAIL** (putaran akhir). Skrip `test-with-agy/scripts/dedup-1124-01-cuti.mjs`, bukti `test-with-agy/screenshots/dedup-1124/01-cuti/` |
| Lebar ponsel 390 px (`dedup-1124-diag-mobile.mjs`) | LULUS untuk mode daftar dan mode form tab Cuti (0 elemen pelebar halaman) |

Rincian 15 kasus browser:

| ID | Kasus | Hasil |
| --- | --- | --- |
| C01 | Tab Cuti dengan data asli `hrB_penyetuju` | PASS — "Cuti Saya" + kartu cadangan "Saldo Tersedia 0 hari" (akun tanpa kartu saldo), tanpa 5xx |
| C02 | Pemetaan DTO (data suntikan) | PASS — kartu per jenis cuti (Cuti Tahunan 2026 8 hari, Cuti Sakit 2026 12 hari, Libur Pengganti 1 hari), status Menunggu/Draf/Disetujui/Sudah Dijalani, "0.5 hari · setengah hari", Batalkan 3, Kirim 1 |
| C03 | Batalkan tanpa alasan | PASS — tombol konfirmasi nonaktif |
| C04 | Batalkan dengan alasan | PASS — `POST .../requests/{id}/cancel` `{ reason, idempotencyKey }`, toast sukses |
| C05 | Kirim draf | PASS — `POST .../requests/{id}/submit` untuk baris Draf |
| C06 | + Ajukan Cuti | PASS — `?section=leave&tab=leave&form=baru`, form lengkap di tab yang sama |
| C07 | Ajukan cuti | PASS — `POST requests` memuat `leaveTypeId` GUID, `leaveBalanceId`, `reason`; `submit` terpanggil; kembali ke daftar + toast |
| C08 | Deep link `jenisCuti=CTO` | PASS — "Libur Pengganti Lembur" terpilih otomatis |
| C09 | Kembali ke Cuti Saya | PASS — parameter `form` hilang, daftar tampil |
| C10 | Panel profil → Pengajuan Cuti | PASS — langsung membuka form workspace |
| C11 | Saldo Libur Pengganti → Gunakan untuk Cuti | PASS — mendarat di form workspace dengan CTO terpilih |
| C12 | Sidebar `hrB_penyetuju` (semua izin) | PASS — `/hr/leave/my-balance` dan `/hr/leave/request` tidak ada; Evaluasi Atasan, Sanggah, Pengaduan, SKP, OPPE, Persetujuan Cuti Staf tetap ada |
| C13 | Regresi `/hr/leave/request` | PASS — "Kembali ke Saldo Saya" tetap ke `/hr/leave/my-balance` |
| C14 | Pegawai biasa `bukanAtasan_nonAdmin` | PASS — ketujuh butir tidak tampil di sidebar; akun ini tanpa `MyLeaveRequest:Read` sehingga bagian Cuti & Lembur memang tidak tampil (workspace ke Beranda, tanpa galat) |
| C15 | Pegawai biasa: tab Dasbor Pelatihan | PASS — tidak tampil |

Kegagalan selama pengujian dan perbaikannya:

| Kegagalan | Sebab | Perbaikan |
| --- | --- | --- |
| Status dan Aksi keluar layar (terlihat di screenshot C02/C05) | Aplikasi: 6 kolom + lebar minimal tabel bersama 840 px | Tabel 4 kolom + `.compactTable` (lihat 10.1) |
| C02 kartu "tidak ditemukan" | Skrip: judul kartu `text-transform: uppercase`, `innerText` jadi "CUTI TAHUNAN\n2026" | Regex tidak peka huruf |
| C07 tombol "Ajukan Cuti" ganda | Skrip: Aksi Cepat panel kanan juga punya tombol itu | Selector dibatasi ke area tab |
| C12 semua butir "tidak ada" | Skrip: sub-menu tidak ada di DOM selama grup tertutup | Cek lewat kotak "Cari menu…" (mencocokkan href) |
| C14 login macet | Data uji: sandi akun `bawahan` tidak berlaku | Pakai `bukanAtasan_nonAdmin` |

### 10.3 Berkas berubah (belum di-commit)

`QuilvianSystemFrontendDev`: `src/app/self-services/human-resource/leave/page.jsx`,
`src/components/view/hr/leave/leave-request-view.jsx`,
`src/components/view/self-services/human-resource/leave/leave-employee-view.jsx`,
`src/components/view/self-services/workspace/self-service-section-content.jsx`,
`src/components/view/self-services/workspace/self-service-workspace-view.jsx`,
`src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx`,
`src/lib/constants/hr/approvals/approval-constants.js`, `src/lib/constants/hr/overtime/overtime-constants.js`,
`src/lib/constants/self-services/self-service-request-constants.js`,
`src/lib/constants/self-services/self-service-workspace-constants.js`,
`src/lib/hooks/payroll/use-payroll-payslip-detail.js`, `src/lib/services/hr/employee-self-service.js`,
`src/style/self-services/self-service-workspace.module.css`, `src/utils/menu-sidebar/menu-items.jsx`,
dan baru `tests/unit/self-service-menu-dedup.test.mjs`.

`NewQuilvianSystemBackend`: hanya dokumen ini.

### 10.4 Catatan lanjutan

- Tab workspace lain yang memakai `DataTable` (Pembatalan Cuti, Panggilan Kembali, Dispensasi,
  Perubahan Jadwal, Pengunduran Diri, dst.) masih bergeser horizontal di area tengah karena lebar
  minimal 840 px yang sama. Bisa dirapikan dengan pola `.compactTable` bila diinginkan.
- Pegawai yang tidak melihat tab Cuti (contoh akun `bukanAtasan_nonAdmin`) perlu diberi izin
  `MyLeaveRequest` dan `MyLeaveBalance` di Akses Role. Ini konfigurasi data, bukan kode; sebelum
  perubahan ini pun halaman menu lama memanggil endpoint dengan izin yang sama.
