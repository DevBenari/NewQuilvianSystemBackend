# BE-SEC-002 — Audit dan Arsitektur Business Permission & Access Profile

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `SEC-BP-001` — Platform Authorization & Access Control |
| Task ID | `BE-SEC-002` |
| Judul | Business Permission & Access Profile Architecture |
| Task mode | `AUDIT + ARCHITECTURE / OWNER DECISION PREPARATION` |
| Status | `AUDIT DITERIMA — KEPUTUSAN OWNER SUDAH DITUTUP` |
| Dokumen lanjutan | [`02-be-sec-002-decision-closure.md`](02-be-sec-002-decision-closure.md) — decision closure dan epic decomposition |
| Wewenang tulis | Hanya dokumen ini. Tidak ada source, entity, migration, database, commit, atau push |
| Backend SHA yang diaudit | `e1d112142510baa86ccd89977bc7189c89ed012b` (branch `AndryZain`, working tree bersih) |
| Frontend SHA yang diaudit | `2b9e3b074f8a3839857e123515353dd2f3233ac3` (branch `AgentCodexFrontend`, working tree bersih) |
| Skill repository | `QuilvianEngineeringSkills`, branch `main` |
| Baseline | `BE-SEC-001` Authorization Integrity Foundation — `COMPLETED` |
| Pilot | Dokter → Rawat Jalan |
| Tanggal | 2 September 2026 |

> **Cara membaca dokumen ini.** Seluruh angka diturunkan dari source pada SHA di atas, bukan dari
> audit lama. Bila sebuah angka berbeda dari audit sebelumnya, yang berlaku adalah angka di
> dokumen ini. Setiap klaim disertai jalur berkas supaya dapat diperiksa ulang.

> **Status setelah keputusan pemilik sistem (2 September 2026).** Audit ini **diterima**. Tujuh
> keputusan pemilik sistem (`DECISION 1`–`DECISION 7`) sudah diberikan dan ditutup pada
> [`02-be-sec-002-decision-closure.md`](02-be-sec-002-decision-closure.md). Bila dokumen ini dan
> dokumen closure berbeda, **yang berlaku adalah dokumen closure**. Tiga hal yang berubah dan
> paling sering disalahpahami:
>
> | Bagian di sini | Diperbarui oleh closure |
> | --- | --- |
> | Bagian P — prefix `Sys` diusulkan | **Ditolak.** Prefix final adalah `Sec` = *Security*, sudah `OWNER APPROVED` tanpa collision. Lihat closure bagian G |
> | Bagian I — 19 Business Permission | Tetap 19, kini **diklasifikasi** menjadi `A`/`B`/`C`/`D`. Sepuluh dapat diaktifkan tanpa menyentuh controller. Lihat closure bagian B |
> | Bagian W — 10 fase | Digantikan **11 task formal** ber-Task ID beserta batas rollback-nya. Lihat closure bagian L |

---

## A. Ringkasan Eksekutif

### A.1 Masalah bisnis yang sedang diselesaikan

Hari ini seorang admin rumah sakit yang ingin memberi hak akses kepada seorang dokter rawat jalan
harus membuka layar **Manajemen Hak Akses**, lalu mencentang kotak-kotak yang namanya adalah nama
teknis buatan programmer: `DoctorQueue`, `PatientVitalSign`, `PrescriptionWorkspace`,
`DiagnosisRecommendationResolver`, `DrugUnitConversion`. Ia harus tahu bahwa satu halaman
"Dokter → Rawat Jalan" ternyata memerlukan **16 kotak resource yang tersebar di 4 modul backend**.
Bila satu saja terlewat, dokter akan menemukan satu tab yang tidak bisa dibuka, dan tidak ada
petunjuk apa pun yang menjelaskan kotak mana yang kurang.

Targetnya: admin cukup melihat struktur yang ia kenali —

```
Health Services
└── Dokter
    └── Rawat Jalan
        ├── Papan Antrean Pasien
        ├── Panggil Pasien
        ├── Hasil Skrining
        ├── SOAP
        ├── CPPT
        ├── Resep
        ├── Tindakan
        └── Selesaikan Konsultasi
```

— dan mencentangnya, tanpa pernah tahu bahwa di belakangnya ada 16 controller.

### A.2 Temuan utama audit

| No | Temuan | Bukti ringkas |
| ---: | --- | --- |
| 1 | Satu halaman bisnis Dokter → Rawat Jalan memakai **53 endpoint**, **16 controller**, **4 modul backend**, dan **30 identitas technical permission** | Bagian F dan G |
| 2 | Frontend **sama sekali tidak melakukan pemeriksaan izin**. Tidak ada satu pun guard di 92 berkas pilot | Bagian C.4 |
| 3 | `RouteGuard` ada di source tetapi **tidak dipakai satu berkas pun**, dan peta rutenya menunjuk rute yang sudah tidak ada (`/MasterData`, `/farmasi`, `/Optik`, `/pendaftaran`) | Bagian C.2 |
| 4 | `filterMenuItemsByRole` **tidak menyaring apa pun**. Ia mencari kunci menu `ManajemenKesehatan` yang tidak ada di registry menu saat ini, dan seluruh aturan perannya dikomentari | Bagian C.3 |
| 5 | Login backend **tidak pernah mengirim daftar izin**. Yang sampai ke browser hanya nama peran ASP.NET Identity, disimpan sebagai satu cookie `role` | Bagian C.5 |
| 6 | **Granularitas technical permission lebih kasar daripada kebutuhan bisnis.** Satu `PatientProcedure.Update` membuka 5 endpoint sekaligus, termasuk `approve` dan `execute` | Bagian J.2 |
| 7 | Tab **Surat Dokter** memanggil `/clinical-management/doctor-certificates` yang **tidak ada di backend**. Kemampuan yang sebenarnya bernama `MedicalCertificate` di `/clinical-management/medical-certificates` | Bagian F.11 |
| 8 | Audio panggilan antrean (`QueueVoice.GetAudio`, `QueueVoice.DownloadAudio`) **benar-benar dipakai pilot** dan sampai sekarang hanya dilindungi `[Authorize]` tanpa `[AccessPermission]` | Bagian F.12 |
| 9 | Tombol **Tidak Hadir** memanggil endpoint lewat `fetch` mentah, melewati `InstanceAxios` | Bagian E.3 |

### A.3 Rekomendasi arsitektur

**Pendekatan A, dijalankan lewat mekanisme transisi C.**

Endpoint tetap memeriksa technical permission `(resource, action)` persis seperti sekarang. Yang
berubah hanya **cara hak itu diberikan**: admin memilih Business Permission, dan sistem
menerjemahkannya menjadi himpunan technical permission saat request masuk. Kontrak kanonik
`BE-SEC-001` tidak disentuh sama sekali.

Alasan singkat: mengubah 2.324 penulisan `[AccessPermission]` pada 279 controller (pendekatan B)
adalah perubahan paling berisiko yang bisa dilakukan pada sistem rumah sakit yang sedang berjalan,
dan akan membatalkan kontrak terkunci `opr-permission-v1` beserta kontrak Billing. Rinciannya di
bagian K dan L.

Satu pekerjaan pendamping **wajib** menyertainya: memecah sebagian technical permission yang
terlalu kasar (bagian J.2). Tanpa itu, Business Permission tidak akan pernah bisa lebih halus
daripada lapisan di bawahnya.

### A.4 Yang menunggu keputusan pemilik sistem

Tujuh butir. Seluruhnya kewenangan bisnis atau klinis yang tidak dapat disimpulkan dari source.
Daftar lengkapnya di bagian Y.

---

## B. Baseline Otorisasi Setelah BE-SEC-001

### B.1 Bagaimana sistem memutuskan boleh atau tidak, hari ini

Ketika seorang petugas menekan tombol, request-nya melewati rantai berikut:

```
Request masuk
  → [Authorize]                    : apakah sudah login?
  → [AccessPermission(res, act)]   : filter otorisasi dijalankan
      → AccessPermissionService.HasAccessAsync(user, res, act)
          1. user ada dan IsActive?
          2. SuperAdmin? → lolos (kecuali konfigurasi klinis diaktifkan)
          3. cari SysActionAccess yang ActionName = act
             dan ControllerAccess.ControllerName = res,
             keduanya aktif, tidak terhapus, bukan IsSystemOnly
          4. cari SysAccessPolicy yang cocok dengan
             (DepartmentId, PositionId) milik SELURUH penempatan
             organisasi user yang masih sah
  → boleh / 403
```

Berkas: `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs`.

### B.2 Aturan yang sudah terkunci dan tidak boleh dilanggar

| Aturan | Isi | Bukti |
| --- | --- | --- |
| Identitas kanonik | Identitas otorisasi sebuah endpoint adalah pasangan `(resource, action)` pada `[AccessPermission]`. Tidak ada sumber lain | `Services/Security/PermissionRegistryDescriptor.cs`; `QuilvianSystemBackend.Tests/Security/CanonicalSecurityContractTests.cs` |
| `[AccessAction]` murni tampilan | Argumen pertamanya tidak pernah dipakai sebagai identitas otorisasi. Ia hanya menentukan nama tampil, deskripsi, urutan, dan kolom layar (`AccessType`) | Sama seperti di atas |
| `AccessType` terbatas | Wajib salah satu dari `Read`, `Create`, `Update`, `Delete` | `Constants/AccessTypes.cs`; `Services/Security/PermissionRegistryValidator.cs` |
| Izin efektif = gabungan | Izin adalah **gabungan** seluruh penempatan organisasi yang sah. Tidak ada DENY | `AccessPermissionService.HasAccessAsync` |
| `IsPrimary` bukan syarat | Penempatan sekunder yang aktif tetap menyumbang izin | Komentar eksplisit pada `AccessPermissionService.cs`; `Models/ApplicationUserOrganization.cs` |
| Seeder tidak pernah memberi hak | Registry hanya mendaftarkan kemampuan; pemberian hak selalu tindakan admin | `Seeders/AccessMenuSeeder.cs`; test `ReconcileNeverCreatesAccessPolicy` |

### B.3 Ukuran registry pada HEAD saat ini

Diukur ulang langsung dari source pada SHA `e1d1121`:

| Ukuran | Nilai | Cara mengukur |
| --- | ---: | --- |
| Penulisan `[AccessPermission(...)]` pada method | 2.324 | Pencarian teks pada `Areas/` dan `Controllers/` |
| Di antaranya memakai dua argumen literal | 2.318 | 6 sisanya memakai konstanta `RoleAccessControllerName` |
| Pasangan `(resource, action)` **berbeda** | 1.041 | Hasil deduplikasi pasangan literal |
| Nama resource berbeda | 278 | Kolom pertama, dideduplikasi |
| Controller ber-`[AccessController]` | 279 | Berkas yang memuat atribut tersebut |
| `moduleCode` berbeda | 33 | Nilai `moduleCode:` yang dideduplikasi |

Angka 1.041 di sini adalah pasangan literal murni. Laporan `BE-SEC-001` menyebut 1.076 kunci
registry; selisih 35 berasal dari kunci berbasis konstanta dan dari 69 endpoint warisan yang
didaftarkan lewat jalur kompatibilitas nama controller. Keduanya konsisten — yang satu menghitung
teks source, yang satu menghitung isi registry hasil refleksi assembly.

### B.4 Tabel yang menopang otorisasi hari ini

| Tabel | Isi | Berkas model |
| --- | --- | --- |
| `SysApplicationModule` | Modul aplikasi: `ModuleCode`, `ModuleName`, `AreaName` | `Models/SysApplicationModule.cs` |
| `SysControllerAccess` | Satu baris per **resource** permission | `Models/SysControllerAccess.cs` |
| `SysActionAccess` | Satu baris per **kemampuan** `(resource, action)`, plus `AccessType` untuk kolom layar | `Models/SysActionAccess.cs` |
| `SysAccessPolicy` | Pemberian hak: `DepartmentId` × `PositionId` × `ControllerAccessId` × `ActionAccessId` | `Models/SysAccessPolicy.cs` |
| `AspNetUserOrganization` | Proyeksi penempatan organisasi user yang dipakai pemeriksaan izin | `Models/ApplicationUserOrganization.cs` |

**Catatan penting.** Struktur ini sehat dan baru saja diperbaiki. Dokumen ini **tidak** mengusulkan
menghapus, mengganti nama, atau meninggalkan satu pun tabel di atas.

---

## C. Arsitektur Role Management Frontend Saat Ini

### C.1 Layar Manajemen Hak Akses

| Aspek | Nilai |
| --- | --- |
| Rute | `/administrator/settings/role-access` |
| Berkas halaman | `src/app/administrator/settings/role-access/page.jsx` |
| Berkas view | `src/components/view/administrator/settings/administrator-role-access-view.jsx` (2.166 baris) |
| Cara memanggil API | `fetch` langsung ke `${API_BASE_URL}/api/v1/administrator/setting/role-access`, **bukan** `InstanceAxios` |
| Token | Dibaca manual dari `localStorage`/`sessionStorage` (`accessToken` atau `token`) |

Endpoint yang dipakainya, bergaya Swagger:

| Tag | Method | Path | Technical permission | Dipakai layar |
| --- | --- | --- | --- | --- |
| Setting / Role Access | `GET` | `/api/v1/administrator/setting/role-access/resources` | `RoleAccess.Read` | tidak |
| Setting / Role Access | `GET` | `/api/v1/administrator/setting/role-access/resources/structured` | `RoleAccess.Read` | ya |
| Setting / Role Access | `GET` | `/api/v1/administrator/setting/role-access/summary` | `RoleAccess.Read` | ya |
| Setting / Role Access | `GET` | `/api/v1/administrator/setting/role-access/policies` | `RoleAccess.Read` | ya |
| Setting / Role Access | `POST` | `/api/v1/administrator/setting/role-access/policies` | `RoleAccess.Update` | ya |
| Setting / Role Access | `POST` | `/api/v1/administrator/setting/role-access/policies/copy` | `RoleAccess.Update` | tidak |

Berkas backend: `Areas/Administrator/Setting/Controllers/RoleAccessController.cs`.

**Cara layar ini mengelompokkan hak akses.** Frontend memakai empat tab area yang **ditebak dari
pencocokan kata**, bukan dari data. Potongan aslinya:

```js
const AREA_TABS = Object.freeze([
  { key: "administrator",   keywords: ["administrator", "administration", "admin", "setting", "auth"] },
  { key: "corporate",       keywords: ["corporate", "human resource", "hr", "finance", "general"] },
  { key: "health-services", keywords: ["health", "clinical", "clinic", "patient", "registration",
                                       "pharmacy", "billing", "medical", "doctor", "nurse",
                                       "laboratory", "radiology"] },
  { key: "self-services",   keywords: ["self", "employee self", "portal", "attendance", "request"] },
]);
```

Sebuah modul masuk tab "Health Services" hanya karena nama modulnya **mengandung** salah satu kata
di atas. Artinya pengelompokan layar hari ini adalah tebakan teks, bukan hierarki bisnis.

Di bawah tab, isinya tetap hierarki backend: **Area → Module → Controller → Action**, dengan empat
kolom centang tetap `Read`, `Create`, `Update`, `Delete` (`CRUD_BUCKETS` pada berkas yang sama).

### C.2 `RouteGuard` — ada, tetapi mati

Berkas `src/components/features/auth/route-guard.jsx` mendefinisikan `RouteGuard`. Pencarian ke
seluruh `src/` menemukan **nol** berkas yang mengimpornya. Komponen ini tidak pernah dipasang.

Logikanya bergantung pada `src/utils/auth/route-guard-link.js`:

```js
export const ROLE_ROUTE_PERMISSIONS = Object.freeze({
  Dokter:  ["/MasterData", "/farmasi", "/Optik", "/pendaftaran"],
  Perawat: ["/MasterData", "/farmasi", "/Optik"],
});
```

Empat rute itu **tidak ada** di App Router saat ini. Rute nyata berbentuk `/health-services/...`,
`/hr/...`, dan `/administrator/...`. Jadi seandainya `RouteGuard` dipasang hari ini, ia tidak akan
memblokir apa pun untuk peran `Dokter`, dan akan memblokir **seluruh aplikasi** untuk peran mana
pun yang namanya bukan `Dokter`, `Perawat`, `admin`, atau `manajer` — karena `getCanonicalRole`
mengembalikan `undefined` lalu `hasRouteAccess` langsung `false`.

**Klasifikasi:** `REPAIR`. Jangan dihidupkan apa adanya.

### C.3 `filterMenuItemsByRole` — tidak menyaring apa pun

Berkas `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`. Dipanggil dari
`src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx:220`.

Isinya:

1. Bila peran kosong, `Admin`, atau `Manajer` → kembalikan seluruh menu.
2. Selain itu, cari item bertekan `key === "ManajemenKesehatan"`.
3. Seluruh aturan penyaringan untuk `Perawat` dan `Dokter` **dikomentari**.
4. Kembalikan menu apa adanya.

Kunci `ManajemenKesehatan` tidak ada di `src/utils/menu-sidebar/menu-items.jsx` saat ini, sehingga
langkah 2 selalu gagal. Hasil akhirnya: **setiap pengguna yang sudah login melihat seluruh menu**,
termasuk menu Administrator dan Billing.

**Klasifikasi:** `REPAIR`.

### C.4 Tidak ada pemeriksaan izin di halaman pilot

Dari 92 berkas yang dapat dicapai halaman Dokter → Rawat Jalan, **tidak satu pun** memuat
pemeriksaan izin. Yang ditemukan hanya:

- `src/lib/axiosInstance/InstanceAxios.jsx` membaca cookie `role` untuk keperluan sesi, bukan untuk
  menyembunyikan tombol;
- satu komentar pada `src/lib/services/health-services/clinical-management/prescribing-drug.service.js:264`
  yang berbunyi *"Master-data permission is optional for doctor users"*.

Halaman ini juga **tidak** memakai `AccessDeniedGate`. Komponen tersebut memang ada
(`src/components/features/base-features/access-denied-gate.jsx`) dan dipakai 20-an layar lain,
tetapi sifatnya **reaktif**: ia baru menampilkan pesan setelah server mengembalikan `403`. Ia bukan
penyembunyi tombol dan bukan pengaman.

**Konsekuensi bisnis hari ini.** Seorang kasir yang membuka
`/health-services/registration-management/doctor-queues` akan melihat halaman dokter lengkap dengan
seluruh tombolnya. Setiap tombol yang ia tekan akan gagal dengan `403`. Data pasien tidak bocor —
server tetap menolak — tetapi pengalaman penggunanya buruk dan menimbulkan tiket helpdesk yang
tidak perlu.

### C.5 Apa yang sebenarnya diterima frontend saat login

Berkas: `src/lib/state/slice/auth/login-slice.jsx`.

```js
const roles = normalizeRoles(user.roles || user.roleNames || user.userRoles);
const mainRole = roles[0] || user.role || user.roleName || user.userRole || user.userType || null;
```

Yang disimpan ke cookie mencakup `userId`, `username`, `email`, `fullName`, `role`, `roles`,
`hospitalId`, `departmentId`, `positionId`, dan konteks tenaga kerja.

**Tidak ada satu pun izin di dalamnya.** Frontend tidak pernah tahu apa yang boleh dilakukan
penggunanya. Ini justru kabar baik untuk BE-SEC-002: karena frontend belum pernah membaca
`ControllerName`, `ActionName`, `SysControllerAccessId`, atau `SysActionAccessId`, maka
**larangan nomor 10 pada penetapan task sudah terpenuhi dengan sendirinya** dan tidak ada
kontrak frontend yang perlu dirusak. Yang ada hanyalah ruang kosong yang menunggu diisi
`GET /api/access/me`.

---

## D. Hierarki Navigasi Saat Ini

### D.1 Registry navigasi kanonik

| Aspek | Nilai |
| --- | --- |
| Berkas | `src/utils/menu-sidebar/menu-items.jsx` (1.037 baris) |
| Export | `export const menuItems = [...]` |
| Konsumen | `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`, `left-sidebar-menu.jsx`, `left-sidebar-menu-handle.jsx` |
| Bentuk simpul | `{ label, key, icon, pathname }` atau `{ label, key, icon, subMenu: [...] }`; tingkat ketiga memakai `subItems` |
| Pemisah | `{ type: "label", label, key }` |

Ini **satu-satunya** registry navigasi. Tidak ada registry kedua.

### D.2 Entri pilot, apa adanya

```js
{
  label: "Dokter",
  key: "healthServicesDoctorQueue",
  icon: <RiUserLine className="fs-4" />,
  subMenu: [
    {
      label: "Rawat Jalan",
      key: "healthServicesDoctorQueueOutpatient",
      icon: <RiFlowChart className="fs-4" />,
      pathname: "/health-services/registration-management/doctor-queues",
    },
  ],
},
```

### D.3 Tiga kenyataan yang harus diterima desain Business Permission

1. **Menu tidak selalu sama dengan modul backend.** Menu "Dokter" hanya punya satu submenu,
   sedangkan halamannya memakai empat modul backend.
2. **Nama menu tidak sama dengan nama rute.** Submenu "Rawat Jalan" berada di bawah menu "Dokter",
   tetapi rutenya `/health-services/registration-management/doctor-queues` — di bawah
   *registration-management*, bukan *outpatient*. Pemetaan Business Permission **tidak boleh**
   diturunkan dari potongan URL.
3. **Ada menu bernama sama di tempat berbeda.** Menu tingkat atas "Rawat Jalan"
   (`healthServicesRegistrationManagement`) juga ada, isinya "Skrining Pasien" untuk perawat.
   Kode Business Permission harus dibedakan berdasarkan **peran pemakai fitur**, bukan sekadar
   label menunya.

### D.4 Menu tingkat atas Health Services pada HEAD saat ini

| Label menu | `key` | Jumlah submenu aktif |
| --- | --- | ---: |
| Rekam Medis | `healthServicesMedicalRecordManagement` | 3 |
| **Dokter** | `healthServicesDoctorQueue` | **1 (pilot)** |
| Instalasi Gawat Darurat | `healthServicesEmergencyRoom` | 3 |
| Rawat Inap | `healthServicesInpatientManagement` | 9 |
| Operasi | `healthServicesOperatingRoomManagement` | 2 |
| Rawat Jalan | `healthServicesRegistrationManagement` | 1 (tiga lainnya dikomentari) |
| Billing dan Kasir | `healthServicesBillingManagement` | 2 + 5 master data |

---

## E. Peta Kemampuan Frontend Dokter → Rawat Jalan

### E.1 Titik masuk dan struktur

```
src/app/health-services/registration-management/doctor-queues/page.jsx
  └── doctor-queue-client.jsx        ("use client")
      └── doctor-queue-view.jsx      (301 baris — seluruh komposisi layar)
```

Layar terbagi dua panel:

- **Panel kiri** — papan antrean pasien: `SummaryBar`, daftar `QueuePatientCard`, gulir tak
  terbatas (`useInfiniteQueueScroll`), penyegaran realtime SignalR.
- **Panel kanan** — ruang kerja konsultasi: `ConsultationTabs`, `DoctorPatientContext`, isi tab,
  dan `FinalizeConsultationPanel` di bagian bawah.

### E.2 Delapan tab, enam di antaranya sudah berfungsi

Sumber: `src/lib/constants/health-services/registration-management/doctor-queue/doctor-queue.constants.js`,
dirender oleh `renderTabContent()` pada `doctor-queue-view.jsx:122-192`.

| # | `key` | Label di layar | Komponen | Status |
| ---: | --- | --- | --- | --- |
| 1 | `screening` | Hasil Skrining | `ScreeningTab` | Berfungsi |
| 2 | `soap` | SOAP | `DoctorSoapTab` | Berfungsi |
| 3 | `cppt` | CPPT | `DoctorCpptTab` | Berfungsi |
| 4 | `prescription` | Resep | `DoctorPrescriptionTab` | Berfungsi |
| 5 | `procedure` | Tindakan | `DoctorProcedureTab` | Berfungsi |
| 6 | `certificate` | Surat Dokter | `DoctorCertificateTab` | **Rusak** — endpoint backend tidak ada (bagian F.11) |
| 7 | `supportingExam` | Penunjang Medis | `WorkInProgressTab` | Placeholder — belum ada API |
| 8 | `cdss` | CDSS | `WorkInProgressTab` | Placeholder — belum ada API |

Tab 7 dan 8 **tidak boleh** mendapat Business Permission sekarang. Membuat izin untuk layar yang
isinya tulisan "sedang disiapkan" hanya melahirkan hak yang tidak berarti.

### E.3 Aksi pada papan antrean

| Aksi di layar | Handler | Berkas |
| --- | --- | --- |
| Panggil (dengan suara) | `handleCallWithVoice` | `useDoctorQueueBoard.js` + `useDoctorCallWithVoice.js` |
| Konsultasi | `handleStart` | `useDoctorConsultationWorkspace.js` |
| Lewati | `handleSkipPatient` | `useDoctorConsultationWorkspace.js` |
| Tidak Hadir | `handleNoShowPatient` | `useDoctorConsultationWorkspace.js` |
| Kembalikan ke antrean | `handleRequeuePatient` | `useDoctorConsultationWorkspace.js` |
| Selesaikan Konsultasi | `handleOpenFinalizeConfirmation` → `handleConfirmFinalizeConsultation` | `useDoctorConsultationWorkspace.js` |

**Temuan teknis pada "Tidak Hadir".** `use-doctor-queue.js:107-145` tidak memakai `InstanceAxios`.
Ia mencoba mencari empat nama fungsi yang tidak ada di service (`markDoctorQueueNoShow`,
`markDoctorQueueNoShowQueue`, `noShowDoctorQueue`, `markQueueNoShow`), gagal menemukannya, lalu
jatuh ke `fetch` mentah:

```js
const response = await fetch(
  buildApiUrl(`/v1/health-services/registration-management/doctor-queues/${encodeURIComponent(id)}/no-show`),
  { method: "POST", credentials: "include", ... },
);
```

Ini bekerja hari ini karena sesi memakai cookie, tetapi ia **melewati seluruh perilaku bersama**
`InstanceAxios`: penyegaran token, penanganan `401`, dan pemetaan pesan error. Dicatat sebagai
temuan di luar cakupan; **tidak diperbaiki** oleh audit ini.

### E.4 Hook dan service yang terlibat

| Lapisan | Jumlah | Daftar |
| --- | ---: | --- |
| Hook | 12 | `use-doctor-queue`, `useDoctorQueueBoard`, `useDoctorConsultationWorkspace`, `useDoctorCallWithVoice`, `useDoctorScreeningForm`, `useInfiniteQueueScroll`, `useQueueTick`, `use-doctor-certificate`, `use-doctor-soap`, `use-doctor-cppt`, `use-doctor-procedure`, `use-doctor-prescription` |
| Service API | 12 | `doctor-queue`, `queue-voice`, `doctor-consultation`, `patient-diagnosis`, `patient-procedure`, `patient-integrated-progress-note`, `diagnosis-recommendation`, `prescribing-drug`, `prescription`, `prescription-workspace`, `prescription-template`, `prescription-measurement` |
| Realtime | 1 | `src/lib/realtime/queue-realtime-client.js` → SignalR hub `/hubs/queues` |

**Redux:** halaman pilot **tidak memakai Redux sama sekali**. Seluruh state ditangani hook lokal.
Satu-satunya sentuhan Redux di alur ini adalah `login-slice` untuk sesi.

---

## F. Matriks Frontend → API → Technical Permission

Tabel berikut adalah hasil penelusuran penuh: dari fungsi yang dipanggil layar, ke service, ke
HTTP method dan route, ke controller/action backend, sampai ke `[AccessPermission(resource, action)]`
dan modul pemiliknya.

Tag Swagger untuk seluruh baris di bawah adalah nama `displayName` pada `[AccessController]`
controller yang bersangkutan.

### F.1 Papan antrean dan alur antrean — `Doctor Queue`

Controller: `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs`
Modul: `HEALTH_SERVICE_REGISTRATION_MANAGEMENT`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getDoctorQueueMetadata` | `GET` | `/api/v1/health-services/registration-management/doctor-queues/filters/metadata` | `GetFilterMetadata` | `DoctorQueue.Read` |
| `getDoctorQueueSummary` | `GET` | `/api/v1/.../doctor-queues/summary` | `GetSummary` | `DoctorQueue.Read` |
| `getDoctorQueues` | `GET` | `/api/v1/.../doctor-queues` | `GetQueues` | `DoctorQueue.Read` |
| `getDoctorQueueCallLock` | `GET` | `/api/v1/.../doctor-queues/call-lock` | `GetCallLock` | `DoctorQueue.Read` |
| `callDoctorQueue` | `POST` | `/api/v1/.../doctor-queues/{id}/call` | `Call` | `DoctorQueue.Update` |
| `startDoctorConsultation` | `POST` | `/api/v1/.../doctor-queues/{id}/start-consultation` | `StartConsultation` | `DoctorQueue.Update` |
| `finishDoctorConsultation` | `POST` | `/api/v1/.../doctor-queues/{id}/finish-consultation` | `FinishConsultation` | `DoctorQueue.Update` |
| `skipDoctorQueue` | `POST` | `/api/v1/.../doctor-queues/{id}/skip` | `Skip` | `DoctorQueue.Update` |
| `requestDoctorQueueNoShow` (`fetch` mentah) | `POST` | `/api/v1/.../doctor-queues/{id}/no-show` | `NoShow` | `DoctorQueue.Update` |
| `requeueDoctorQueue` | `POST` | `/api/v1/.../doctor-queues/{id}/requeue` | `Requeue` | `DoctorQueue.Update` |

**Catatan data-scope yang sudah berjalan.** Deskripsi `[AccessAction]` pada `GetQueues` berbunyi
*"Melihat antrean pasien dokter login"*. Artinya pembatasan "hanya antrean milik saya" sudah
ditegakkan backend hari ini, di dalam query, bukan lewat permission. Ini penting: Business
Permission **tidak perlu** dan **tidak boleh** mencoba menggantikan pembatasan itu.

### F.2 Konsultasi dokter — `Doctor Consultation`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getActiveDoctorConsultationByQueue` | `GET` | `/api/v1/health-services/clinical-management/doctor-consultations/active-by-queue/{queueId}` | `GetActiveByQueue` | `DoctorConsultation.Read` |
| `getDoctorConsultations` | `GET` | `/api/v1/.../doctor-consultations` | `GetConsultations` | `DoctorConsultation.Read` |
| `getDoctorConsultationById` | `GET` | `/api/v1/.../doctor-consultations/{id}` | `GetById` | `DoctorConsultation.Read` |
| `createDoctorConsultation` | `POST` | `/api/v1/.../doctor-consultations` | `CreateConsultation` | `DoctorConsultation.Create` |
| `patchDoctorConsultationSoap` | `PATCH` | `/api/v1/.../doctor-consultations/{id}/soap` | `UpdateSoap` | `DoctorConsultation.Update` |

### F.3 Skrining — pengkajian pasien — `Patient Assessment`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getActivePatientAssessmentByQueue` | `GET` | `/api/v1/.../patient-assessments/active-by-queue/{queueId}` | `GetActiveByQueue` | `PatientAssessment.Read` |
| `getPatientAssessmentHistoryByPatient` | `GET` | `/api/v1/.../patient-assessments` | `GetAssessments` | `PatientAssessment.Read` |
| `createPatientAssessment` | `POST` | `/api/v1/.../patient-assessments` | `CreateAssessment` | `PatientAssessment.Create` |
| `updatePatientAssessment` | `PUT` | `/api/v1/.../patient-assessments/{id}` | `UpdateAssessment` | `PatientAssessment.Update` |
| `completePatientAssessment` | `PATCH` | `/api/v1/.../patient-assessments/{id}/complete` | `CompleteAssessment` | `PatientAssessment.Update` |

### F.4 Skrining — tanda vital — `Patient Vital Sign`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getActivePatientVitalSignByQueue` | `GET` | `/api/v1/.../patient-vital-signs/active-by-queue/{queueId}` | `GetActiveByQueue` | `PatientVitalSign.Read` |
| `getPatientVitalSignHistoryByPatient` | `GET` | `/api/v1/.../patient-vital-signs` | `GetVitalSigns` | `PatientVitalSign.Read` |
| `createPatientVitalSign` | `POST` | `/api/v1/.../patient-vital-signs` | `CreateVitalSign` | `PatientVitalSign.Create` |
| `updatePatientVitalSign` | `PUT` | `/api/v1/.../patient-vital-signs/{id}` | `UpdateVitalSign` | `PatientVitalSign.Update` |

### F.5 SOAP — diagnosis — `Patient Diagnosis`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `searchMasterDiagnosisOptions` | `GET` | `/api/v1/.../patient-diagnoses/master-options` | `GetMasterDiagnosisOptions` | `PatientDiagnosis.Read` |
| `getPatientDiagnosisOptions` | `GET` | `/api/v1/.../patient-diagnoses/options` | `GetDiagnosisOptions` | `PatientDiagnosis.Read` |
| `createPatientDiagnosis` | `POST` | `/api/v1/.../patient-diagnoses` | `CreateDiagnosis` | `PatientDiagnosis.Create` |
| `setPrimaryPatientDiagnosis` | `PATCH` | `/api/v1/.../patient-diagnoses/{id}/set-primary` | `SetPrimary` | `PatientDiagnosis.Update` |
| `cancelPatientDiagnosis` | `PATCH` | `/api/v1/.../patient-diagnoses/{id}/cancel` | `CancelDiagnosis` | `PatientDiagnosis.Update` |

### F.6 SOAP — rekomendasi — `Diagnosis Recommendation Resolver`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/DiagnosisRecommendationResolverController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `resolveDiagnosisRecommendations` | `POST` | `/api/v1/health-services/clinical-management/diagnosis-recommendations/resolve` | `Resolve` | `DiagnosisRecommendationResolver.Read` |

Perhatikan: method-nya `POST`, tetapi permission-nya `Read`. Ini benar — endpoint ini hanya
membaca, `POST` dipakai karena parameter pencariannya panjang. Contoh nyata mengapa Business
Permission **tidak boleh** diturunkan dari HTTP method.

### F.7 CPPT — `Patient Integrated Progress Note`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getPatientIntegratedProgressNoteTimeline` | `GET` | `/api/v1/.../patient-integrated-progress-notes/timeline` | `GetTimeline` | `PatientIntegratedProgressNote.Read` |
| `createPatientIntegratedProgressNoteFromConsultation` | `POST` | `/api/v1/.../patient-integrated-progress-notes/from-consultation/{consultationId}` | `CreateFromConsultation` | `PatientIntegratedProgressNote.Create` |

### F.8 Tindakan — `Patient Procedure`

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs`
Modul: `HEALTH_SERVICE_CLINICAL`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getPatientProcedures` | `GET` | `/api/v1/.../patient-procedures` | `GetProcedures` | `PatientProcedure.Read` |
| `getPatientProcedureMasterOptions` | `GET` | `/api/v1/.../patient-procedures/master-options` | `GetMasterProcedureOptions` | `PatientProcedure.Read` |
| `selectPatientProcedure` | `POST` | `/api/v1/.../patient-procedures/select` | `SelectProcedure` | `PatientProcedure.Create` |
| `removeDraftPatientProcedure` | `PATCH` | `/api/v1/.../patient-procedures/{id}/remove-draft` | `RemoveDraftProcedure` | `PatientProcedure.Update` |

### F.9 Resep — katalog obat — `Prescribing Drug` dan master data

| Fungsi frontend | Method | Path | Controller | Technical permission | Modul |
| --- | --- | --- | --- | --- | --- |
| `getPrescribingDrugs` | `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs` | `PrescribingDrug` | `PrescribingDrug.Read` | `HEALTH_SERVICE_CLINICAL` |
| `getPrescribingDrugById` | `GET` | `/api/v1/.../prescribing-drugs/{drugId}` | `PrescribingDrug` | `PrescribingDrug.Read` | `HEALTH_SERVICE_CLINICAL` |
| `getMasterDrugById` | `GET` | `/api/v1/health-services/master-data/drugs/{id}` | `Drug` | `Drug.Read` | `HEALTH_SERVICE_MASTER_DATA` |
| `getDrugClinicalInformation` | `GET` | `/api/v1/health-services/master-data/drugs/{id}/clinical-information` | `Drug` | `Drug.Read` | `HEALTH_SERVICE_MASTER_DATA` |
| `getDrugUnitConversionOptions` | `GET` | `/api/v1/health-services/master-data/drug-unit-conversions/options` | `DrugUnitConversion` | `DrugUnitConversion.Read` | `HEALTH_SERVICE_MASTER_DATA` |
| `getPrescriptionMeasurementOptions` | `GET` | `/api/v1/health-services/master-data/measurements/options` | `Measurement` | `Measurement.Read` | `HEALTH_SERVICE_MASTER_DATA` |

**Empat baris terakhir bersifat opsional, dan source membuktikannya.** Frontend sengaja menelan
kegagalan `403` pada endpoint master data:

```js
} catch {
  // Master-data permission is optional for doctor users. Keep the
  // prescribing response as the source of truth when access is denied.
}
```
(`prescribing-drug.service.js:263-266`)

```js
} catch {
  // Clinical-information endpoint is a safe fallback only. Do not replace
  // the actual prescribing/coverage error with a master-data 403/404.
}
```
(`prescribing-drug.service.js:324-327`)

`getCompoundMeasurementCatalog` juga memakai `.catch(() => [])` per jenis satuan.

Ini bukti kuat bahwa `Drug.Read`, `DrugUnitConversion.Read`, dan `Measurement.Read` harus
diklasifikasikan **`OPTIONAL`** pada peta Business Permission. Bila tidak diberikan, resep tetap
bisa dibuat; hanya tampilan satuan dan informasi klinis obat yang berkurang.

### F.10 Resep — inti — `Prescription`, `Prescription Workspace`, `Prescription Template`

Modul: `HEALTH_SERVICE_PHARMACY`

| Fungsi frontend | Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- | --- |
| `getActivePrescriptionByConsultation` | `GET` | `/api/v1/health-services/pharmacy-management/prescriptions/active-by-consultation/{consultationId}` | `GetActiveByConsultation` | `Prescription.Read` |
| `createPrescription` | `POST` | `/api/v1/.../prescriptions` | `CreatePrescription` | `Prescription.Create` |
| `getPrescriptionWorkspace` | `GET` | `/api/v1/.../prescription-workspaces/{prescriptionId}` | `GetWorkspace` | `PrescriptionWorkspace.Read` |
| `getPrescriptionWorkspaceByConsultation` | `GET` | `/api/v1/.../prescription-workspaces/by-consultation/{consultationId}` | `GetWorkspaceByConsultation` | `PrescriptionWorkspace.Read` |
| `autosavePrescriptionWorkspace` | `PATCH` | `/api/v1/.../prescription-workspaces/{prescriptionId}/autosave` | `Autosave` | `PrescriptionWorkspace.Update` |
| `getPrescriptionTemplates` | `GET` | `/api/v1/.../prescription-templates` | `GetTemplates` | `PrescriptionTemplate.Read` |
| `getPrescriptionTemplateById` | `GET` | `/api/v1/.../prescription-templates/{id}` | `GetById` | `PrescriptionTemplate.Read` |
| `applyPrescriptionTemplate` | `POST` | `/api/v1/.../prescription-templates/{id}/apply` | `Apply` | `PrescriptionTemplate.Create` |
| `createTemplateFromPrescription` | `POST` | `/api/v1/.../prescription-templates/from-prescription` | `CreateFromPrescription` | `PrescriptionTemplate.Create` |

### F.11 Surat Dokter — `CONFLICT`, endpoint tidak ada

`src/lib/hooks/.../use-doctor-certificate.js:304-305` memanggil `updateDoctorCertificate` atau
`createDoctorCertificate`. Keduanya menunjuk:

| Fungsi frontend | Method | Path yang dipanggil | Hasil di backend |
| --- | --- | --- | --- |
| `createDoctorCertificate` | `POST` | `/api/v1/health-services/clinical-management/doctor-certificates` | **404 — route tidak terdaftar** |
| `updateDoctorCertificate` | `PUT` | `/api/v1/health-services/clinical-management/doctor-certificates/{id}` | **404 — route tidak terdaftar** |

Pencarian ke seluruh backend menemukan **nol** kemunculan teks `doctor-certificate`. Route yang
benar-benar ada di `clinical-management` adalah 16 route, dan yang paling dekat maknanya adalah:

| Tag Swagger | Method | Path | Technical permission |
| --- | --- | --- | --- |
| Medical Certificate | `POST` | `/api/v1/health-services/clinical-management/medical-certificates` | `MedicalCertificate.Create` |
| Medical Certificate | `PUT` | `/api/v1/.../medical-certificates/{id}` | `MedicalCertificate.Update` |
| Medical Certificate | `PATCH` | `/api/v1/.../medical-certificates/{id}/issue` | `MedicalCertificate.Update` |
| Medical Certificate | `PATCH` | `/api/v1/.../medical-certificates/{id}/verify` | `MedicalCertificate.Update` |
| Medical Certificate | `PATCH` | `/api/v1/.../medical-certificates/{id}/approve` | `MedicalCertificate.Update` |

Controller: `Areas/HealthServices/ClinicalManagement/Controllers/MedicalCertificateController.cs`,
`displayName: "Medical Certificate"`, deskripsi *"Surat keterangan medis pasien"*.

**Status:** `CONFLICT`. Business Permission untuk Surat Dokter **boleh dirancang**, tetapi
pemetaannya ke technical permission belum bisa dikunci sampai ketidaksesuaian ini diputuskan.
Perbaikannya adalah pekerjaan frontend terpisah, bukan bagian dari BE-SEC-002.

### F.12 Suara panggilan antrean — `Queue Voice`

Controller: `Areas/HealthServices/RegistrationManagement/Controllers/QueueVoiceController.cs`
Modul: `HEALTH_SERVICE_REGISTRATION_MANAGEMENT`

Alur nyatanya: tombol **Panggil** memanggil `POST /doctor-queues/{id}/call`. Response-nya memuat
`audioUrl` dan/atau `downloadUrl`. `playQueueCallVoice` lalu mengambil berkas audio dari URL itu
lewat `fetchQueueVoiceAudioBlob` (`queue-voice.service.js:679`).

| Method | Path | Action backend | Technical permission |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/queue-voice/audio/{dateKey}/{fileName}` | `GetAudio` | **tidak ada `[AccessPermission]`** — hanya `[Authorize]` |
| `GET` | `/api/v1/.../queue-voice/download/{dateKey}/{fileName}` | `DownloadAudio` | **tidak ada `[AccessPermission]`** — hanya `[Authorize]` |

Dua endpoint inilah yang disebut `BE-SEC-001` sebagai "belum diklasifikasi `[AllowAnonymous]` versus
policy". Audit ini menambahkan satu fakta baru: **keduanya benar-benar dipakai oleh alur pilot**,
bukan endpoint mati. Keputusan klasifikasinya karena itu menjadi prasyarat nyata, bukan sekadar
kerapian.

Tiga endpoint `QueueVoice` lainnya (`profiles`, `preview`, `queues/{id}/regenerate`) punya
`[AccessPermission]` lengkap tetapi **tidak dipakai** halaman pilot.

### F.13 Realtime

| Protokol | Path | Perlindungan | Berkas |
| --- | --- | --- | --- |
| SignalR (WebSocket) | `/hubs/queues` | `[Authorize]` pada `Hubs/QueueHub.cs:16` | `src/lib/realtime/queue-realtime-client.js` |

Hub hanya mensyaratkan login. Tidak ada pemeriksaan permission per-hub maupun per-group.
Dicatat sebagai risiko di bagian U.

---

## G. Angka Aktual pada HEAD Saat Ini

Seluruh angka di bawah dihitung ulang dari SHA yang tercantum di kepala dokumen.

### G.1 Berkas frontend

Dihitung dengan penelusuran impor transitif dari
`src/app/health-services/registration-management/doctor-queues/page.jsx`, dengan resolusi alias
`@/*` → `./src/*` dari `jsconfig.json`.

| Kategori | Jumlah |
| --- | ---: |
| **Total berkas yang dapat dicapai** | **92** |
| — `.js` / `.jsx` | 82 |
| — CSS Module | 10 |
| Rincian 82 berkas `.js`/`.jsx`: | |
| — halaman App Router (`src/app/`) | 1 |
| — view halaman dan tab (`src/components/view/`) | 11 |
| — komponen fitur (`doctor-queue-features/`) | 17 |
| — komponen base (`features/base-features/`) | 3 |
| — komponen UI bersama (`components/ui/`, termasuk `index.js`) | 7 |
| — hook (`src/lib/hooks/`) | 12 |
| — service API (`src/lib/services/`) | 12 |
| — constant (`src/lib/constants/`) | 5 |
| — utility (`src/utils/`) | 12 |
| — infrastruktur (`InstanceAxios`, `realtime/`) | 2 |

> Audit lama menyebut "kurang lebih 91 berkas". Angka pada HEAD saat ini adalah **92**. Selisih 1
> tidak signifikan dan tidak dipaksakan agar sama.

### G.2 Endpoint dan permission

| Ukuran | Jumlah | Catatan |
| --- | ---: | --- |
| Titik panggilan jaringan pilot | **56** | 53 HTTP terselesaikan + 2 HTTP `404` + 1 SignalR |
| Endpoint HTTP terselesaikan | **53** | Punya controller/action nyata |
| — dilindungi `[AccessPermission]` | 51 | |
| — hanya `[Authorize]` | 2 | `QueueVoice.GetAudio`, `QueueVoice.DownloadAudio` |
| Endpoint HTTP `404` | **2** | `doctor-certificates` `POST` dan `PUT` |
| Koneksi realtime | **1** | `/hubs/queues`, `[Authorize]` |
| **Identitas technical permission berbeda** | **30** | Daftar lengkap di G.4 |
| Controller backend terlibat | **16** | Daftar di G.3 |
| Modul backend terlibat | **4** | `HEALTH_SERVICE_REGISTRATION_MANAGEMENT`, `HEALTH_SERVICE_CLINICAL`, `HEALTH_SERVICE_PHARMACY`, `HEALTH_SERVICE_MASTER_DATA` |

> Audit lama menyebut "49 technical permission rows" dan "16 backend controllers". Pada HEAD saat
> ini: **30 identitas permission berbeda** dan **16 controller**. Angka 49 kemungkinan menghitung
> baris registry atau pasangan endpoint-permission, bukan identitas unik. Yang berlaku untuk
> BE-SEC-002 adalah **30 identitas unik**, karena itulah satuan yang diberikan admin.

### G.3 Enam belas controller, dikelompokkan per modul

| Modul | Controller | Jumlah endpoint pilot |
| --- | --- | ---: |
| `HEALTH_SERVICE_REGISTRATION_MANAGEMENT` | `DoctorQueue` | 10 |
| | `QueueVoice` | 2 |
| `HEALTH_SERVICE_CLINICAL` | `DoctorConsultation` | 5 |
| | `PatientAssessment` | 5 |
| | `PatientVitalSign` | 4 |
| | `PatientDiagnosis` | 5 |
| | `PatientProcedure` | 4 |
| | `PatientIntegratedProgressNote` | 2 |
| | `DiagnosisRecommendationResolver` | 1 |
| | `PrescribingDrug` | 2 |
| | `MedicalCertificate` | 0 — *target perbaikan tab Surat Dokter* |
| `HEALTH_SERVICE_PHARMACY` | `Prescription` | 2 |
| | `PrescriptionWorkspace` | 3 |
| | `PrescriptionTemplate` | 4 |
| `HEALTH_SERVICE_MASTER_DATA` | `Drug` | 2 |
| | `DrugUnitConversion` | 1 |
| | `Measurement` | 1 |

### G.4 Tiga puluh identitas technical permission

| # | `(resource, action)` | Modul |
| ---: | --- | --- |
| 1 | `DoctorQueue.Read` | Registration |
| 2 | `DoctorQueue.Update` | Registration |
| 3 | `DoctorConsultation.Read` | Clinical |
| 4 | `DoctorConsultation.Create` | Clinical |
| 5 | `DoctorConsultation.Update` | Clinical |
| 6 | `PatientAssessment.Read` | Clinical |
| 7 | `PatientAssessment.Create` | Clinical |
| 8 | `PatientAssessment.Update` | Clinical |
| 9 | `PatientVitalSign.Read` | Clinical |
| 10 | `PatientVitalSign.Create` | Clinical |
| 11 | `PatientVitalSign.Update` | Clinical |
| 12 | `PatientDiagnosis.Read` | Clinical |
| 13 | `PatientDiagnosis.Create` | Clinical |
| 14 | `PatientDiagnosis.Update` | Clinical |
| 15 | `DiagnosisRecommendationResolver.Read` | Clinical |
| 16 | `PatientIntegratedProgressNote.Read` | Clinical |
| 17 | `PatientIntegratedProgressNote.Create` | Clinical |
| 18 | `PatientProcedure.Read` | Clinical |
| 19 | `PatientProcedure.Create` | Clinical |
| 20 | `PatientProcedure.Update` | Clinical |
| 21 | `PrescribingDrug.Read` | Clinical |
| 22 | `Prescription.Read` | Pharmacy |
| 23 | `Prescription.Create` | Pharmacy |
| 24 | `PrescriptionWorkspace.Read` | Pharmacy |
| 25 | `PrescriptionWorkspace.Update` | Pharmacy |
| 26 | `PrescriptionTemplate.Read` | Pharmacy |
| 27 | `PrescriptionTemplate.Create` | Pharmacy |
| 28 | `Drug.Read` | Master Data |
| 29 | `DrugUnitConversion.Read` | Master Data |
| 30 | `Measurement.Read` | Master Data |

Dua identitas tambahan menyusul bila tab Surat Dokter diperbaiki: `MedicalCertificate.Create` dan
`MedicalCertificate.Update` → menjadi **32**.

---

## H. Usulan Hierarki Business Feature

Diturunkan **hanya** dari apa yang benar-benar ada di frontend V2 pada SHA yang diaudit. Tidak ada
simpul yang dibuat karena backend punya controller.

```
Health Services                              [Area Bisnis]
└── Dokter                                   [Menu]      ← menu key: healthServicesDoctorQueue
    └── Rawat Jalan                          [Submenu]   ← menu key: healthServicesDoctorQueueOutpatient
        ├── Papan Antrean Pasien             [Feature]   ← panel kiri
        ├── Panggil Pasien                   [Feature]   ← tombol Panggil + suara
        ├── Alur Antrean                     [Feature]   ← Lewati / Tidak Hadir / Kembalikan
        ├── Ruang Konsultasi                 [Feature]   ← tombol Konsultasi, panel kanan
        ├── Hasil Skrining                   [Feature]   ← tab 1
        ├── SOAP                             [Feature]   ← tab 2
        ├── CPPT                             [Feature]   ← tab 3
        ├── Resep                            [Feature]   ← tab 4
        ├── Tindakan                         [Feature]   ← tab 5
        ├── Surat Dokter                     [Feature]   ← tab 6  (BLOKIR: endpoint belum ada)
        └── Selesaikan Konsultasi            [Feature]   ← panel finalisasi
```

**Yang sengaja TIDAK dimasukkan:**

| Kandidat | Alasan ditolak |
| --- | --- |
| Penunjang Medis | Tab-nya hanya menampilkan `WorkInProgressTab`. Tidak ada API. Izin untuk layar kosong tidak bermakna |
| CDSS | Sama seperti di atas |
| Pengkajian dan Vital Sign sebagai dua feature terpisah | Di layar keduanya satu tab (`ScreeningTab`) dengan satu tombol simpan. Memisahkannya berarti mengarang UI yang tidak ada |
| Diagnosis sebagai feature terpisah dari SOAP | Diagnosis dikelola **di dalam** `DoctorSoapTab` (`use-doctor-soap.js` memanggil `patient-diagnosis.service`). Tidak ada tab Diagnosis tersendiri |
| Katalog Obat sebagai feature terpisah | Ia adalah pencarian di dalam tab Resep, bukan layar tersendiri |
| Template Resep sebagai submenu | Ia panel di dalam tab Resep (`doctor-prescription-template-panel.jsx`) |

**Catatan penamaan.** Nama-nama di atas memakai label yang benar-benar tampil di layar (`Hasil
Skrining`, `SOAP`, `CPPT`, `Resep`, `Tindakan`, `Surat Dokter`) supaya admin melihat kata yang sama
dengan yang dilihat dokter. Ini keputusan sadar, bukan terjemahan bebas.

---

## I. Katalog Business Permission untuk Rawat Jalan

### I.1 Aturan penamaan kode

```
<area>.<menu>.<submenu>.<feature>[.<sub-feature>].<aksi>
```

- Huruf kecil semua, dipisah titik.
- **Tidak** memuat nama class, nama controller, nama tabel, potongan route, atau `SysActionAccessId`.
- Aksi memakai kosakata bisnis yang tetap: `view`, `read`, `write`, `call`, `manage`, `finalize`.
- Kode bersifat **permanen**. Bila kelak controller backend dipindah modul, kodenya tidak berubah —
  yang berubah hanya isi tabel pemetaan.

Contoh mengapa ini stabil: seandainya `PrescribingDrugController` suatu hari dipindahkan dari
`ClinicalManagement` ke `PharmacyManagement`, kode `health.doctor.outpatient.prescription.read`
tetap sama. Yang berubah hanya satu baris pada tabel pemetaan technical permission. Admin rumah
sakit tidak perlu tahu apa pun.

### I.2 Katalog — 19 Business Permission

Kolom **Wajib/Opsional** pada pemetaan berarti: `WAJIB` = tanpa izin ini fiturnya gagal;
`OPSIONAL` = bila tidak diberikan, fitur tetap jalan dengan tampilan berkurang (dibuktikan oleh
blok `catch` di frontend, lihat F.9).

Kolom **Sensitivitas**: `NORMAL` = kemampuan kerja harian; `TINGGI` = menyentuh keputusan klinis,
finansial, atau penandatanganan; `TERBLOKIR` = tidak dapat dipetakan sampai ada perbaikan.

---

**BP-01**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.view` |
| Nama tampil | Buka Rawat Jalan Dokter |
| Makna bisnis | Membuka halaman Dokter → Rawat Jalan dan melihat papan antrean pasien beserta ringkasannya. Ini izin dasar; tanpa ini seluruh izin lain di bawahnya tidak berguna |
| Fitur frontend | Papan Antrean Pasien, `SummaryBar`, `QueuePatientCard` |
| Technical permission | `DoctorQueue.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `OWN` — sudah ditegakkan backend: `GetQueues` hanya mengembalikan antrean dokter yang login |
| Bukti | `doctor-queue-view.jsx:196-256`; `DoctorQueueController.cs:85,112,142,190` |

**BP-02**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.queue.call` |
| Nama tampil | Panggil Pasien |
| Makna bisnis | Menekan tombol Panggil sehingga nomor antrean pasien diumumkan lewat pengeras suara |
| Fitur frontend | Tombol Panggil pada `QueuePatientCard` + `useDoctorCallWithVoice` |
| Technical permission | `DoctorQueue.Update` (WAJIB); `DoctorQueue.Read` untuk `call-lock` (WAJIB); pengambilan berkas audio saat ini **tidak punya technical permission** |
| Sensitivitas | `NORMAL` |
| Data-scope | `OWN` |
| Bukti | `useDoctorQueueBoard.js`; `DoctorQueueController.cs:260`; `QueueVoiceController.cs:45,63` |
| Catatan | Pemetaan belum lengkap sampai klasifikasi `QueueVoice.GetAudio`/`DownloadAudio` diputuskan |

**BP-03**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.queue.flow` |
| Nama tampil | Kelola Alur Antrean |
| Makna bisnis | Melewati pasien yang tidak muncul, menandainya tidak hadir, atau mengembalikannya ke antrean |
| Fitur frontend | Tombol Lewati, Tidak Hadir, Kembalikan ke Antrean |
| Technical permission | `DoctorQueue.Update` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `OWN` |
| Bukti | `useDoctorConsultationWorkspace.js:381-383`; `DoctorQueueController.cs:489,545,613` |

**BP-04**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.consultation.start` |
| Nama tampil | Mulai Konsultasi |
| Makna bisnis | Membuka ruang kerja konsultasi untuk satu pasien dan menandai konsultasi dimulai |
| Fitur frontend | Tombol Konsultasi; panel kanan terbuka |
| Technical permission | `DoctorQueue.Update` (WAJIB), `DoctorConsultation.Create` (WAJIB), `DoctorConsultation.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `OWN` |
| Bukti | `useDoctorConsultationWorkspace.js:177-194`; `DoctorQueueController.cs:367`; `DoctorConsultationController.cs:202,242` |

**BP-05**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.consultation.finalize` |
| Nama tampil | Selesaikan Konsultasi |
| Makna bisnis | Menutup konsultasi pasien. Setelah ini status antrean berubah dan pasien lanjut ke proses berikutnya (farmasi, kasir) |
| Fitur frontend | `FinalizeConsultationPanel` + `FinalizeConsultationModal` |
| Technical permission | `DoctorQueue.Update` (WAJIB) |
| Sensitivitas | **`TINGGI`** — menutup episode pelayanan dan memicu proses hilir |
| Data-scope | `OWN` |
| Bukti | `doctor-queue-view.jsx:279-296`; `DoctorQueueController.cs:443` |

**BP-06**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.screening.read` |
| Nama tampil | Lihat Hasil Skrining |
| Makna bisnis | Melihat hasil pengkajian dan tanda vital yang sudah diisi perawat, beserta riwayatnya |
| Fitur frontend | Tab Hasil Skrining — mode baca; `AssessmentHistoryTable`, `VitalSignHistoryTable` |
| Technical permission | `PatientAssessment.Read` (WAJIB), `PatientVitalSign.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `ScreeningTab.jsx`; `PatientAssessmentController.cs:55,216`; `PatientVitalSignController.cs:168,396` |

**BP-07**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.screening.write` |
| Nama tampil | Isi / Ulangi Skrining Dokter |
| Makna bisnis | Dokter mengisi ulang pengkajian dan tanda vital ketika hasil perawat perlu dikoreksi atau dilengkapi |
| Fitur frontend | `DoctorScreeningForm` + tombol simpan draft dan simpan skrining |
| Technical permission | `PatientAssessment.Create` (WAJIB), `PatientAssessment.Update` (WAJIB), `PatientVitalSign.Create` (WAJIB), `PatientVitalSign.Update` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `useDoctorScreeningForm.js`; `use-doctor-queue.js` (`handleCreateDoctorScreening`) |
| Peringatan granularitas | `PatientVitalSign.Update` juga membuka `verify` dan `notify-doctor`. Lihat J.2 |

**BP-08**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.soap.read` |
| Nama tampil | Lihat SOAP |
| Makna bisnis | Membaca catatan Subjective, Objective, Assessment, Plan pada konsultasi berjalan |
| Fitur frontend | Tab SOAP — mode baca |
| Technical permission | `DoctorConsultation.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `doctor-soap-tab.jsx`; `use-doctor-soap.js`; `DoctorConsultationController.cs:176` |

**BP-09**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.soap.write` |
| Nama tampil | Tulis SOAP |
| Makna bisnis | Menulis dan menyimpan otomatis catatan SOAP |
| Fitur frontend | Formulir SOAP dengan autosave (`DOCTOR_SOAP_AUTOSAVE_DELAY_MS`) |
| Technical permission | `DoctorConsultation.Update` (WAJIB) |
| Sensitivitas | **`TINGGI`** — isi rekam medis klinis |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-soap.js`; `DoctorConsultationController.cs:503` |
| Peringatan granularitas | `DoctorConsultation.Update` juga membuka `complete` dan `cancel`. Lihat J.2 |

**BP-10**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.diagnosis.read` |
| Nama tampil | Lihat Diagnosis |
| Makna bisnis | Mencari kode diagnosis ICD, melihat diagnosis pasien, dan melihat rekomendasi klinis terkait |
| Fitur frontend | Pencarian diagnosis dan panel rekomendasi di dalam tab SOAP |
| Technical permission | `PatientDiagnosis.Read` (WAJIB), `DiagnosisRecommendationResolver.Read` (OPSIONAL — hanya menonaktifkan saran otomatis) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-soap.js`; `PatientDiagnosisController.cs:91,221`; `DiagnosisRecommendationResolverController.cs:48` |

**BP-11**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.diagnosis.write` |
| Nama tampil | Tetapkan Diagnosis |
| Makna bisnis | Menambahkan diagnosis pasien, menandai diagnosis utama, dan membatalkan diagnosis yang salah |
| Fitur frontend | Tombol tambah / jadikan utama / batalkan pada tab SOAP |
| Technical permission | `PatientDiagnosis.Create` (WAJIB), `PatientDiagnosis.Update` (WAJIB) |
| Sensitivitas | **`TINGGI`** — diagnosis memengaruhi penagihan dan pelaporan |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-soap.js`; `PatientDiagnosisController.cs:310,571,682` |
| Peringatan granularitas | `PatientDiagnosis.Update` juga membuka `resolve`. Lihat J.2 |

**BP-12**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.cppt.read` |
| Nama tampil | Lihat CPPT |
| Makna bisnis | Membaca Catatan Perkembangan Pasien Terintegrasi — riwayat catatan seluruh profesi |
| Fitur frontend | Tab CPPT (`DoctorCpptTab` + `use-doctor-cppt`) |
| Technical permission | `PatientIntegratedProgressNote.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-cppt.js`; `PatientIntegratedProgressNoteController.cs:174` |

**BP-13**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.cppt.write` |
| Nama tampil | Tulis CPPT dari SOAP |
| Makna bisnis | Membuat entri CPPT baru yang isinya diambil dari SOAP konsultasi berjalan |
| Fitur frontend | Dipicu saat finalisasi (`createPatientIntegratedProgressNoteFromConsultation` pada `useDoctorConsultationWorkspace.js`) |
| Technical permission | `PatientIntegratedProgressNote.Create` (WAJIB) |
| Sensitivitas | **`TINGGI`** — masuk rekam medis resmi |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `useDoctorConsultationWorkspace.js`; `PatientIntegratedProgressNoteController.cs:374` |

**BP-14**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.prescription.read` |
| Nama tampil | Lihat Resep |
| Makna bisnis | Membuka tab Resep, melihat resep berjalan, dan mencari obat di katalog |
| Fitur frontend | Tab Resep — mode baca; pencarian obat pada panel obat umum dan racikan |
| Technical permission | `Prescription.Read` (WAJIB), `PrescriptionWorkspace.Read` (WAJIB), `PrescribingDrug.Read` (WAJIB), `Drug.Read` (OPSIONAL), `DrugUnitConversion.Read` (OPSIONAL), `Measurement.Read` (OPSIONAL) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-prescription.js`; `prescribing-drug.service.js:263-266,324-327` (bukti opsional) |

**BP-15**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.prescription.write` |
| Nama tampil | Tulis Resep |
| Makna bisnis | Membuat resep dan menyimpan otomatis seluruh isinya, termasuk obat racikan |
| Fitur frontend | Panel obat umum dan racikan + autosave (`PRESCRIPTION_AUTOSAVE_DELAY_MS`) |
| Technical permission | `Prescription.Create` (WAJIB), `PrescriptionWorkspace.Update` (WAJIB) |
| Sensitivitas | **`TINGGI`** — resep obat |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-prescription.js`; `PrescriptionController.cs:273`; `PrescriptionWorkspaceController.cs:92` |

**BP-16**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.prescription.template` |
| Nama tampil | Gunakan dan Simpan Template Resep |
| Makna bisnis | Memakai template resep yang sudah ada, dan menyimpan resep berjalan menjadi template baru |
| Fitur frontend | `doctor-prescription-template-panel.jsx` |
| Technical permission | `PrescriptionTemplate.Read` (WAJIB), `PrescriptionTemplate.Create` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-prescription.js`; `PrescriptionTemplateController.cs:77,126,151,181` |
| Catatan penggabungan | "pakai template" dan "simpan template" **tidak dapat dipisahkan** hari ini: keduanya memakai identitas yang sama, `PrescriptionTemplate.Create`. Pemisahannya memerlukan pemecahan technical permission (J.2). Sampai itu terjadi, keduanya sengaja disatukan agar tidak ada hak tersembunyi |

**BP-17**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.procedure.read` |
| Nama tampil | Lihat Tindakan |
| Makna bisnis | Melihat daftar tindakan pasien dan mencari tindakan di master |
| Fitur frontend | Tab Tindakan — mode baca |
| Technical permission | `PatientProcedure.Read` (WAJIB) |
| Sensitivitas | `NORMAL` |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-procedure.js`; `PatientProcedureController.cs:107,179` |

**BP-18**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.procedure.write` |
| Nama tampil | Pilih Tindakan |
| Makna bisnis | Memilih tindakan yang akan dikerjakan untuk pasien, dan membatalkan pilihan dari draft |
| Fitur frontend | Tombol pilih dan hapus pilihan pada tab Tindakan |
| Technical permission | `PatientProcedure.Create` (WAJIB), `PatientProcedure.Update` (WAJIB) |
| Sensitivitas | **`TINGGI`** — tindakan menimbulkan tagihan |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-procedure.js`; `PatientProcedureController.cs:348,997` |
| Peringatan granularitas | **Ini kasus terburuk.** `PatientProcedure.Update` juga membuka `approve` dan `execute` — dua kemampuan yang secara bisnis jelas bukan milik dokter yang sedang memilih tindakan. Lihat J.2 |

**BP-19**

| Field | Isi |
| --- | --- |
| Kode | `health.doctor.outpatient.certificate.write` |
| Nama tampil | Buat Surat Dokter |
| Makna bisnis | Membuat surat sakit, surat sehat, dan dokumen medis lain untuk pasien |
| Fitur frontend | Tab Surat Dokter (`DoctorCertificateTab` + `use-doctor-certificate`) |
| Technical permission | **`TERBLOKIR`** — target seharusnya `MedicalCertificate.Create` dan `MedicalCertificate.Update`, tetapi frontend memanggil route yang tidak ada |
| Sensitivitas | **`TINGGI`** — dokumen bertanda tangan dokter dengan akibat hukum dan ketenagakerjaan |
| Data-scope | `ORGANIZATION_SCOPE` |
| Bukti | `use-doctor-certificate.js:304-305`; ketiadaan route `doctor-certificates` di backend; `MedicalCertificateController.cs:248,393` |
| Status | Kode Business Permission **boleh** didaftarkan sekarang, pemetaannya **fail closed** sampai ketidaksesuaian rute diperbaiki |

### I.3 Ringkasan katalog

| Sensitivitas | Jumlah | Kode |
| --- | ---: | --- |
| `NORMAL` | 12 | BP-01, 02, 03, 04, 06, 07, 08, 10, 12, 14, 16, 17 |
| `TINGGI` | 7 | BP-05, 09, 11, 13, 15, 18, 19 |
| **Total** | **19** | |

Dua di antaranya **belum dapat diberikan** hari ini, dan keduanya sudah termasuk dalam hitungan
di atas:

| Kode | Alasan tertahan |
| --- | --- |
| BP-18 `procedure.write` | Pemetaannya membawa serta `approve` dan `execute` (J.2) — menunggu keputusan owner |
| BP-19 `certificate.write` | Pemetaannya `TERBLOKIR` karena endpoint yang dipanggil frontend tidak ada (F.11) |

> Audit lama memperkirakan "~17 candidate business permissions". Angka pada HEAD saat ini adalah
> **19**, dengan 1 di antaranya belum dapat dipetakan. Selisihnya wajar dan tidak dipaksakan.

---

## J. Strategi Pemetaan Business Permission → Technical Permission

### J.1 Bentuk pemetaan

Satu Business Permission memetakan ke **satu atau lebih** technical permission, masing-masing
dengan penanda `WAJIB` atau `OPSIONAL`:

```
health.doctor.outpatient.prescription.read
   ├── WAJIB    Prescription.Read
   ├── WAJIB    PrescriptionWorkspace.Read
   ├── WAJIB    PrescribingDrug.Read
   ├── OPSIONAL Drug.Read
   ├── OPSIONAL DrugUnitConversion.Read
   └── OPSIONAL Measurement.Read
```

Satu technical permission juga **boleh** dipakai lebih dari satu Business Permission. Contohnya
`DoctorQueue.Update` dipakai BP-02, BP-03, BP-04, dan BP-05. Ini normal dan bukan cacat: itulah
konsekuensi granularitas backend saat ini.

**Aturan yang tidak boleh dilanggar:** Business Permission **tidak pernah** menggantikan pemeriksaan
technical permission di endpoint. Endpoint tetap menegakkan `(resource, action)`-nya sendiri. Bila
seseorang memanggil endpoint langsung dengan `curl`, ia tetap dihadang filter yang sama.

### J.2 Masalah granularitas — temuan paling penting audit ini

Business Permission tidak bisa lebih halus daripada technical permission di bawahnya. Bila satu
`(resource, action)` menutupi banyak endpoint, maka memberikan Business Permission apa pun yang
memetakannya akan membuka **seluruh** endpoint itu.

Berikut ukurannya, dihitung dari source:

| Technical permission | Endpoint yang dibukanya | Rincian |
| --- | ---: | --- |
| `PatientProcedure.Update` | **5** | `PUT {id}`, `PATCH {id}/approve`, `PATCH {id}/execute`, `PATCH {id}/remove-draft`, `PATCH {id}/cancel` |
| `DoctorQueue.Update` | **6** | `call`, `start-consultation`, `finish-consultation`, `skip`, `no-show`, `requeue` |
| `PatientVitalSign.Update` | **4** | `PUT {id}`, `verify`, `notify-doctor`, `cancel` |
| `PatientDiagnosis.Update` | **4** | `PUT {id}`, `set-primary`, `resolve`, `cancel` |
| `DoctorConsultation.Update` | **4** | `PUT {id}`, `soap`, `complete`, `cancel` |
| `PrescriptionTemplate.Create` | **3** | `POST`, `from-prescription`, `{id}/apply` |
| `PatientAssessment.Update` | **3** | `PUT {id}`, `complete`, `cancel` |
| `Drug.Read` | **6** | `filters/metadata`, `summary`, daftar, `options`, `{id}/clinical-information`, `{id}` |
| `PatientVitalSign.Read` | **7** | termasuk `critical-alerts` dan daftar seluruh pasien |

**Contoh berangka yang paling mengkhawatirkan.** Dokter rawat jalan perlu satu kemampuan: menghapus
tindakan yang salah pilih dari draft (`PATCH /patient-procedures/{id}/remove-draft`). Memberikannya
hari ini berarti memberi `PatientProcedure.Update`, yang **sekaligus** memberi:

- `PATCH {id}/approve` — menyetujui tindakan;
- `PATCH {id}/execute` — menyatakan tindakan sudah dikerjakan;
- `PATCH {id}/cancel` — membatalkan tindakan yang sudah berjalan.

Ketiganya menimbulkan konsekuensi tagihan. Jadi `BP-18` hari ini **tidak dapat** diberikan tanpa
memberi kemampuan approve dan execute sekaligus.

Contoh kedua: `BP-09` (Tulis SOAP) memerlukan `DoctorConsultation.Update`, yang sekaligus memberi
`PATCH {id}/complete`. Artinya "boleh menulis SOAP" hari ini otomatis berarti "boleh menyelesaikan
konsultasi", padahal `BP-05` sengaja dipisahkan sebagai izin tersendiri bersensitivitas tinggi.

**Kesimpulan.** Pemisahan Business Permission menjadi `read`/`write` sudah benar dan berguna, tetapi
**tanpa pemecahan technical permission, sebagian pemisahan itu semu.** Ini bukan cacat arsitektur
Business Permission — ini utang granularitas pada lapisan teknis.

### J.3 Usulan pemecahan technical permission

`BE-SEC-001` sudah membuktikan bahwa nama aksi tidak harus `Read`/`Create`/`Update`/`Delete`.
Verba bisnis seperti `Collect` dan `Accept` sudah ada di registry. Yang membatasi hanya
`AccessType`, yang memang wajib salah satu dari empat kolom layar — dan itu **metadata tampilan**,
bukan identitas.

Jadi pemecahan berikut sah secara kontrak, tanpa melanggar aturan mana pun dari `BE-SEC-001`:

| Endpoint | Identitas sekarang | Usulan identitas | Alasan |
| --- | --- | --- | --- |
| `PATCH /patient-procedures/{id}/approve` | `PatientProcedure.Update` | `PatientProcedure.Approve` | Persetujuan tindakan adalah kewenangan berbeda |
| `PATCH /patient-procedures/{id}/execute` | `PatientProcedure.Update` | `PatientProcedure.Execute` | Pelaksanaan tindakan adalah kewenangan berbeda |
| `PATCH /doctor-consultations/{id}/complete` | `DoctorConsultation.Update` | `DoctorConsultation.Complete` | Menutup episode ≠ menulis catatan |
| `PATCH /patient-vital-signs/{id}/verify` | `PatientVitalSign.Update` | `PatientVitalSign.Verify` | Verifikasi adalah kendali mutu |
| `POST /prescription-templates/{id}/apply` | `PrescriptionTemplate.Create` | `PrescriptionTemplate.Apply` | Memakai template ≠ membuat template |
| `GET /queue-voice/audio/...` | *(tidak ada)* | `QueueVoice.Read` atau `[AllowAnonymous]` | Menutup celah yang tertinggal dari A0 |
| `GET /queue-voice/download/...` | *(tidak ada)* | `QueueVoice.Read` atau `[AllowAnonymous]` | Sama |

**Peringatan keras.** Pemecahan ini **mempersempit** hak yang sudah diberikan. Setiap
Departemen × Posisi yang hari ini memegang `PatientProcedure.Update` akan kehilangan kemampuan
`approve` dan `execute` begitu pemecahan diterapkan, kecuali identitas barunya ikut diberikan.
Ini persis kelas risiko "silent privilege loss" yang dicegah `BE-SEC-001`.

Karena itu pemecahan ini **wajib** memakai prosedur yang sama dengan A0: klasifikasi terlebih dulu,
laporan sebelum tulis, dan keputusan sadar per baris. Ia **tidak boleh** dilakukan diam-diam
bersamaan dengan pekerjaan Business Permission — ia harus menjadi task tersendiri.

### J.4 Cara mencegah pemetaan yang salah

| Risiko | Penjagaan |
| --- | --- |
| Business Permission menunjuk technical permission yang tidak ada | Test invarian: setiap baris pemetaan wajib cocok dengan `PermissionRegistryDescriptor` |
| Technical permission dipakai halaman tetapi tidak ada Business Permission yang memetakannya | Test cakupan per pilot: 30 identitas pilot wajib tercakup |
| Business Permission diberikan tetapi endpoint tetap `403` karena satu technical permission `WAJIB` terlewat | Test resolusi: perluasan Business Permission wajib menghasilkan seluruh identitas `WAJIB`-nya |
| Pemetaan diam-diam melebar | Test perbandingan: perubahan pada tabel pemetaan wajib menghasilkan laporan selisih sebelum diterapkan |

---

## K. Alternatif Arsitektur A / B / C

### K.1 Pendekatan A — endpoint tetap memeriksa technical permission

Business Permission diterjemahkan menjadi himpunan technical permission. Endpoint tidak berubah
satu baris pun.

```
Admin memilih  →  Business Permission
                        ↓ (tabel pemetaan)
                  himpunan (resource, action)
                        ↓
        AccessPermissionService.HasAccessAsync — TIDAK BERUBAH
```

| Kriteria | Penilaian |
| --- | --- |
| Keamanan | **Terbaik.** Seluruh 2.324 titik penegakan tetap utuh. Tidak ada endpoint yang kehilangan pemeriksaan walau sedetik |
| Risiko migrasi | **Terendah.** Nol perubahan pada controller. Nol perubahan kontrak API |
| Kompatibilitas dengan `BE-SEC-001` | **Sempurna.** `CanonicalSecurityContractTests` tetap hijau tanpa disentuh |
| Kerumitan operasional | Sedang. Bertambah satu tabel pemetaan yang harus dipelihara |
| Auditabilitas | **Baik.** Pertanyaan "mengapa orang ini boleh?" dapat dijawab berlapis: Business Permission apa → technical permission apa → policy mana |
| Performa | Sedang. Perlu satu sumber izin tambahan pada jalur pemeriksaan; diatasi dengan cache per-request |
| Kontrak frontend | **Baik.** Frontend menerima kode Business Permission dan tidak pernah melihat nama teknis |
| Pemeliharaan jangka panjang | **Baik**, dengan satu syarat: granularitas teknis harus diperbaiki (J.3), kalau tidak sebagian pemisahan bisnis akan semu |

### K.2 Pendekatan B — endpoint langsung memeriksa Business Permission

`[AccessPermission("DoctorQueue", "Update")]` diganti menjadi sesuatu seperti
`[BusinessPermission("health.doctor.outpatient.queue.flow")]`.

| Kriteria | Penilaian |
| --- | --- |
| Keamanan | **Buruk selama transisi.** 2.324 atribut harus diubah. Setiap atribut yang salah ketik menjadi lubang atau penolakan permanen — persis 89 endpoint rusak yang baru saja diperbaiki `BE-SEC-001` |
| Risiko migrasi | **Tertinggi.** 279 controller, 33 modul. Tidak ada cara menerapkannya sebagian tanpa dua sistem otorisasi hidup bersamaan |
| Kompatibilitas dengan `BE-SEC-001` | **Melanggar.** Aturan kanonik nomor 1 dan 2 batal. `CanonicalSecurityContractTests`, `PermissionRegistryInvariantTests`, dan kontrak terkunci `opr-permission-v1` beserta kontrak Billing seluruhnya harus ditulis ulang |
| Kerumitan operasional | Tinggi. Pengembang backend harus tahu kode bisnis rumah sakit untuk menulis satu endpoint |
| Auditabilitas | Sedang. Satu lapis lebih pendek, tetapi jejak sejarahnya putus |
| Performa | Sedikit lebih baik — satu lapis lebih sedikit |
| Kontrak frontend | Sama baiknya dengan A |
| Pemeliharaan jangka panjang | **Buruk.** Endpoint menjadi terikat pada hierarki menu. Bila menu berubah, kode endpoint ikut berubah — persis kebalikan dari tujuan blueprint ini |

**Catatan penting.** Pendekatan B secara diam-diam membalik arah masalah. Hari ini frontend
terpaksa mengikuti struktur backend. Pendekatan B memaksa backend mengikuti struktur menu
frontend. Keduanya sama-sama kopling yang salah. Prinsip nomor 1 pada penetapan task —
"Backend tetap domain-oriented" — sudah melarangnya.

### K.3 Pendekatan C — hybrid selama migrasi

Sebagian endpoint memeriksa technical permission, sebagian sudah memeriksa Business Permission,
dengan lapisan kompatibilitas di antaranya.

| Kriteria | Penilaian |
| --- | --- |
| Keamanan | Sedang. Dua jalur penegakan hidup bersamaan; celah biasanya muncul di sambungannya |
| Risiko migrasi | Sedang. Dapat dipotong per modul |
| Kompatibilitas dengan `BE-SEC-001` | Sebagian. Aturan kanonik berlaku untuk sebagian endpoint saja — dan aturan yang berlaku separuh sulit ditegakkan lewat test |
| Kerumitan operasional | **Tertinggi.** Dua model mental, dua layar admin, dua cara menjelaskan ke rumah sakit |
| Auditabilitas | Buruk selama transisi. Jawaban "mengapa boleh" berbeda tergantung endpoint mana |
| Performa | Sama dengan A |
| Kontrak frontend | Membingungkan bila transisinya lama |
| Pemeliharaan jangka panjang | Buruk sebagai **tujuan akhir**; baik sebagai **mekanisme sementara** |

---

## L. Rekomendasi Arsitektur Runtime

### L.1 Rekomendasi

> **Tujuan akhir: pendekatan A. Mekanisme transisi: sifat aditif ala C, pada lapisan pemberian hak
> saja — bukan pada lapisan penegakan.**

Dengan kata lain: yang hidup berdampingan selama transisi bukan dua cara **memeriksa** izin, tetapi
dua cara **memberi** izin. Endpoint hanya punya satu cara memeriksa, dari dulu sampai nanti.

### L.2 Bentuk runtime yang diusulkan

`AccessPermissionService.HasAccessAsync` tetap menjadi satu-satunya pintu. Yang ditambahkan hanya
**sumber hak kedua** di dalamnya:

```
HasAccessAsync(user, resource, action)
  1..3  tidak berubah    (user aktif, SuperAdmin, cari SysActionAccess)
  4a.   SUMBER LAMA      SysAccessPolicy  ← Department × Position langsung
  4b.   SUMBER BARU      Access Profile → Business Permission → (resource, action)
                          untuk Department × Position yang sama
  hasil = 4a ATAU 4b     (gabungan, tidak ada DENY)
```

Alasan bentuk ini dipilih:

| Sifat | Mengapa penting |
| --- | --- |
| **Aditif murni** | Tidak ada hak lama yang hilang saat sumber baru dinyalakan. Tidak ada *silent privilege loss* |
| **Tidak ada DENY** | Sesuai prinsip nomor 8. Menambah DENY berarti mengubah semantik gabungan yang baru saja dikunci A0 |
| **Dapat dimatikan** | Bila sumber baru bermasalah, mematikannya mengembalikan sistem **persis** ke baseline `BE-SEC-001` |
| **Satu penulis per tabel** | `SysAccessPolicy` tetap hanya ditulis layar Role Access lama. Tabel baru hanya ditulis layar baru. Tidak ada dua penulis yang berebut |
| **Dapat diaudit** | Sistem selalu bisa menjawab "izin ini datang dari sumber lama atau profil akses" |

### L.3 Mengapa BUKAN materialisasi ke `SysAccessPolicy`

Alternatif yang tampak lebih sederhana: setiap kali admin memberi Business Permission, sistem
langsung menuliskan baris-baris `SysAccessPolicy` yang setara. Ditolak, karena:

1. `BE-SEC-001` bekerja keras memastikan **tidak ada proses otomatis yang membuat `SysAccessPolicy`**
   (`ReconcileNeverCreatesAccessPolicy`). Materialisasi menghidupkan kembali penulis otomatis.
2. Bila pemetaan Business Permission kelak diubah, baris yang sudah dimaterialisasi menjadi basi.
   Tidak ada cara aman menghapusnya tanpa menebak mana yang hasil materialisasi dan mana yang
   pemberian manual.
3. Jejak audit hilang. Setelah materialisasi, yang tersisa hanya baris teknis; alasan bisnisnya
   lenyap.

### L.4 Performa

Pemeriksaan hari ini sudah melakukan 3–4 perjalanan ke database per request terproteksi. Menambah
sumber kedua menaikkannya. Penanganan yang diusulkan, urut dari yang paling aman:

1. **Cache per-request** — himpunan kunci izin efektif user dihitung sekali per request HTTP.
   Aman, tidak ada masalah basi.
2. **Cache ber-TTL pendek** (30–60 detik) per user. Konsekuensinya: pencabutan hak baru terasa
   setelah TTL habis. Ini **keputusan bisnis**, bukan teknis, karena menyangkut seberapa cepat
   pencabutan hak harus berlaku. Masuk daftar keputusan owner (bagian Y).
3. Optimasi query. Terakhir, hanya bila 1 dan 2 belum cukup.

### L.5 Yang tidak berubah sama sekali

| Hal | Status |
| --- | --- |
| `[AccessPermission(resource, action)]` sebagai identitas kanonik | Tidak berubah |
| Isi 279 controller | Tidak berubah |
| `AccessPermissionFilter` | Tidak berubah |
| Kontrak API mana pun | Tidak berubah |
| `opr-permission-v1` dan kontrak Billing | Tidak berubah |
| Perilaku SuperAdmin | Tidak berubah |
| `SysAccessPolicy`, `SysControllerAccess`, `SysActionAccess`, `SysApplicationModule` | Tidak dihapus, tidak diganti nama |
| Layar Role Access lama | Tetap hidup selama transisi |

---

## M. Desain Access Profile

### M.1 Bentuk

```
Departemen × Posisi
        ↓  (penetapan, dengan masa berlaku)
   Access Profile   (boleh lebih dari satu)
        ↓
  Business Permission (banyak)
        ↓  (tabel pemetaan)
  Technical Permission (resource, action)
```

### M.2 Profil yang dibutuhkan pilot

Hanya dua. Dokumen ini sengaja **tidak** merancang profil untuk seluruh rumah sakit.

**`DOCTOR_OUTPATIENT` — Dokter Rawat Jalan**

| Business Permission | Termasuk? | Catatan |
| --- | --- | --- |
| `health.doctor.outpatient.view` | ya | |
| `health.doctor.outpatient.queue.call` | ya | |
| `health.doctor.outpatient.queue.flow` | ya | |
| `health.doctor.outpatient.consultation.start` | ya | |
| `health.doctor.outpatient.consultation.finalize` | ya | Sensitif; termasuk karena dokterlah yang menutup konsultasinya sendiri |
| `health.doctor.outpatient.screening.read` | ya | |
| `health.doctor.outpatient.screening.write` | ya | |
| `health.doctor.outpatient.soap.read` | ya | |
| `health.doctor.outpatient.soap.write` | ya | |
| `health.doctor.outpatient.diagnosis.read` | ya | |
| `health.doctor.outpatient.diagnosis.write` | ya | |
| `health.doctor.outpatient.cppt.read` | ya | |
| `health.doctor.outpatient.cppt.write` | ya | |
| `health.doctor.outpatient.prescription.read` | ya | |
| `health.doctor.outpatient.prescription.write` | ya | |
| `health.doctor.outpatient.prescription.template` | ya | |
| `health.doctor.outpatient.procedure.read` | ya | |
| `health.doctor.outpatient.procedure.write` | **menunggu keputusan** | Hari ini membawa serta `approve` dan `execute` (J.2). Sampai technical permission dipecah, memasukkannya berarti memberi kewenangan approve tindakan kepada setiap dokter rawat jalan |
| `health.doctor.outpatient.certificate.write` | **menunggu keputusan** | Penandatanganan surat dokter; sekaligus masih terblokir secara teknis |

**`DOCTOR_OUTPATIENT_OBSERVER` — Peninjau Rawat Jalan**

Seluruh Business Permission berakhiran `.view` dan `.read` saja. Tidak ada satu pun `.write`,
`.call`, `.flow`, atau `.finalize`.

Kegunaannya: dokter penyelia, peserta didik yang belum berwenang menulis, dan auditor mutu.

### M.3 Profil yang sengaja belum dirancang

Bukti source menunjukkan perawat juga menulis pengkajian dan tanda vital — deskripsi
`[AccessController]` pada `PatientAssessmentController` berbunyi *"Screening awal pasien oleh
perawat"*, dan ada halaman `nurse-station-queue` tersendiri. Jadi `NURSE_OUTPATIENT` **pasti**
akan dibutuhkan.

Tetapi halaman itu **bukan pilot ini**. Merancang profilnya sekarang berarti mengarang tanpa
menelusuri halamannya. Ditunda sampai pilot berikutnya.

### M.4 Aturan Access Profile

| Aturan | Isi |
| --- | --- |
| Satu Departemen × Posisi boleh punya lebih dari satu profil | Hasilnya **gabungan** |
| Satu Business Permission boleh ada di banyak profil | Tidak masalah; gabungan tidak menghitung ganda |
| Profil punya masa berlaku | Mengikuti pola `EffectiveStartDate`/`EffectiveEndDate` yang sudah dipakai `AspNetUserOrganization` |
| Tidak ada DENY di profil | Profil hanya memberi. Mencabut = melepas profil atau mengakhiri masa berlakunya |
| Profil tidak pernah otomatis diberikan | Sama seperti `SysAccessPolicy`: tidak ada seeder yang memberi hak sendiri |

---

## N. Departemen + Posisi dan Resolusi Multi-Organisasi

### N.1 Aturan resolusi

Aturan ini **melanjutkan persis** apa yang sudah berlaku setelah `BE-SEC-001`, hanya diperluas
dengan satu sumber baru:

```
Izin efektif seorang user
=
GABUNGAN, untuk SETIAP penempatan organisasi yang sah:
      izin dari SysAccessPolicy  (Departemen × Posisi)          ← sumber lama
    ∪ izin dari Access Profile   (Departemen × Posisi)          ← sumber baru
    ∪ izin dari override langsung (Departemen × Posisi)         ← sumber baru, bila dipakai
```

Sebuah penempatan disebut sah bila: belum dihapus, belum dibatalkan, `IsActive`, tanggal mulainya
sudah lewat, dan tanggal berakhirnya belum lewat. `IsPrimary` dan `AssignmentType` **tidak**
diperiksa. Ini persis aturan `AccessPermissionService.HasAccessAsync` hari ini.

### N.2 Contoh berangka

Dr. Sinta punya dua penempatan yang sah pada 1 Juli:

| Penempatan | Departemen | Posisi | Masa berlaku | `IsPrimary` |
| --- | --- | --- | --- | --- |
| 1 | Poliklinik Umum | Dokter Umum | 1 Jan — tanpa akhir | ya |
| 2 | IGD | Dokter Jaga | 1 Jun — 31 Des | tidak |

Penetapan profil:

| Departemen × Posisi | Access Profile |
| --- | --- |
| Poliklinik Umum × Dokter Umum | `DOCTOR_OUTPATIENT` |
| IGD × Dokter Jaga | `DOCTOR_EMERGENCY` *(belum dirancang; dicontohkan saja)* |

Hasil pada 1 Juli: Dr. Sinta memegang **gabungan** Business Permission kedua profil. Ia bisa
membuka halaman rawat jalan **dan** halaman IGD.

Hasil pada 1 Januari tahun berikutnya: penempatan IGD sudah lewat masa berlakunya. Izin dari
`DOCTOR_EMERGENCY` berhenti dengan sendirinya; izin rawat jalan tetap ada. Tidak ada tindakan admin
yang diperlukan.

Perhatikan bahwa penempatan 2 bukan penempatan utama, dan itu **tidak** mengurangi izinnya sedikit
pun — sesuai prinsip nomor 6 dan 7.

### N.3 Ketika dua sumber memberi izin yang sama

Tidak ada masalah. Gabungan bersifat idempoten: diberi dua kali tetap sama dengan diberi sekali.
Tidak ada penghitungan ganda dan tidak ada konflik, karena **tidak ada DENY** yang bisa
bertentangan.

Yang **wajib** ada adalah kemampuan menjawab pertanyaan audit: "izin ini datang dari mana?"
Jawabannya bisa lebih dari satu sumber, dan laporan harus menampilkan seluruhnya.

---

## O. Model Data Konseptual

> **Ini konsep, bukan entity.** Tidak ada berkas model yang dibuat. Nama-nama di bawah adalah
> usulan yang masih harus disetujui, dan pembuatannya diblokir sampai registry prefix disetujui
> (bagian P).

### O.1 Tujuh konsep

| # | Konsep | Isi | Mengapa perlu |
| ---: | --- | --- | --- |
| 1 | **Business Feature** | Simpul hierarki bisnis: Area → Menu → Submenu → Feature. Punya kode, nama tampil, induk, urutan, dan status aktif | Supaya layar admin punya pohon bisnis nyata, bukan tebakan kata seperti `AREA_TABS` sekarang |
| 2 | **Business Permission** | Kode stabil, nama tampil, makna bisnis, simpul feature pemiliknya, sensitivitas, kebutuhan data-scope | Satuan yang dicentang admin |
| 3 | **Business Permission Mapping** | Business Permission → `(resource, action)`, dengan penanda `WAJIB`/`OPSIONAL` | Terjemahan ke lapisan teknis. Inilah satu-satunya tempat nama teknis boleh muncul |
| 4 | **Access Profile** | Kode, nama tampil, deskripsi, kategori, status aktif | Template hak akses siap pakai |
| 5 | **Access Profile Permission** | Access Profile → Business Permission | Isi profil |
| 6 | **Organization Access Profile** | Departemen × Posisi → Access Profile, dengan masa berlaku | Penetapan profil ke organisasi |
| 7 | **Organization Permission Override** | Departemen × Posisi → Business Permission, **hanya GRANT**, dengan masa berlaku dan alasan wajib | Pengecualian terkendali (bagian Q pada model overlay, lihat juga bagian "Direct vs Profile" di O.4) |

### O.2 Hubungan antar konsep

```
Business Feature (pohon)
        │ 1..n
        ▼
Business Permission ─────────────┐
        │ 1..n                   │ 1..n
        ▼                        ▼
Business Permission Mapping   Access Profile Permission
        │ n..1                    │ n..1
        ▼                        ▼
(resource, action)            Access Profile
= SysActionAccess                 │ 1..n
  + SysControllerAccess           ▼
  YANG SUDAH ADA          Organization Access Profile
                                  │ n..1
                                  ▼
                          MstDepartment × MstPosition
                             YANG SUDAH ADA
```

Perhatikan dua ujungnya: model baru **menempel** pada `SysControllerAccess`/`SysActionAccess` yang
sudah ada di satu sisi, dan pada `MstDepartment`/`MstPosition` yang sudah ada di sisi lain. Tidak
ada yang diduplikasi.

### O.3 Sifat wajib setiap konsep

| Sifat | Berlaku untuk | Alasan |
| --- | --- | --- |
| Soft delete + status aktif | Semua | Mengikuti pola `IdentityModel` repository |
| Jejak audit siapa dan kapan | Semua | Mengikuti pola `IdentityModel` |
| Masa berlaku (`EffectiveStartDate`/`EffectiveEndDate`) | Konsep 6 dan 7 | Supaya hak sementara berakhir sendiri, seperti penempatan organisasi |
| Alasan wajib diisi | Konsep 7 | Override tanpa alasan tertulis adalah sumber utama privilege creep |
| Kode unik dan tidak boleh diubah | Konsep 1, 2, 4 | Kode adalah kontrak; frontend akan memakainya |
| Tidak ada kolom DENY | Semua | Prinsip nomor 8 |

### O.4 Direct Permission versus Access Profile — rekomendasi

Tiga pilihan yang diminta dinilai:

| Pilihan | Untuk IT rumah sakit | Pemeliharaan | Audit | Privilege creep | Rumah sakit berbeda | Onboarding |
| --- | --- | --- | --- | --- | --- | --- |
| **A. Selalu Access Profile** | Paling mudah | Paling mudah | Paling jelas | Paling aman | **Kaku** — pengecualian nyata memaksa membuat profil baru terus-menerus | Cepat |
| **B. Business Permission langsung per Departemen+Posisi** | Berat — kembali ke mencentang ratusan kotak | Berat | Jejaknya rata, sulit melihat pola | **Paling rawan** | Fleksibel | Lambat |
| **C. Profile sebagai template + override terkendali** | Mudah untuk kasus normal, mungkin untuk kasus khusus | Sedang | **Terbaik** — laporan dapat memisahkan "dari profil" dan "override" | Terkendali bila override wajib beralasan dan bermasa berlaku | **Paling sesuai** | Cepat |

> **Rekomendasi: pilihan C.**

Alasan konkretnya, dari bukti audit ini sendiri: `BP-18` (Pilih Tindakan) hari ini membawa serta
kemampuan `approve` dan `execute`. Sebuah rumah sakit kecil mungkin memang ingin dokternya
menyetujui tindakannya sendiri; rumah sakit besar hampir pasti tidak. Kalau hanya ada pilihan A,
kedua rumah sakit itu memerlukan dua profil `DOCTOR_OUTPATIENT` yang berbeda, dan lama-kelamaan
lahir belasan profil yang hampir sama. Kalau hanya ada pilihan B, admin kembali mencentang satu per
satu.

Pagar pengaman yang wajib menyertai pilihan C:

1. Override **hanya menambah**, tidak pernah mengurangi. Untuk mengurangi, lepas profilnya.
2. Alasan wajib diisi, minimal beberapa kata bermakna.
3. Override untuk Business Permission bersensitivitas `TINGGI` memerlukan persetujuan terpisah.
4. Ada laporan tetap: "seluruh override yang berlaku, beserta alasan dan tanggal berakhirnya".
5. Layar admin selalu menandai mana yang berasal dari profil dan mana yang override.

---

## P. Usulan Owner/Prefix QBE

### P.1 Status: `REQUIRED`

Model data pada bagian O membutuhkan **tabel persisted baru**. `QBE-MOD-002` dan `QBE-MOD-003` pada
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` menyatakan: entity operasional persisted
berstatus `BLOCKED` sampai ada baris registry yang disetujui, dan pendaftaran wajib dilakukan
**sebelum** berkas model pertama dibuat.

`blueprint-manifest.md` sudah mengantisipasi ini:

> "Bila kelak fase Business Permission membuat entity persisted baru — misalnya `SysBusinessFeature`,
> `SysPermission`, atau `SysAccessProfile` — pendaftaran prefix menjadi wajib dan **harus diusulkan
> lebih dulu** kepada pemilik sistem sebelum modelnya dibuat."

### P.2 Fakta registry saat ini

Tabel registry memuat 18 baris. **Prefix `Sys` tidak ada di dalamnya**, padahal empat tabel platform
yang sudah berjalan memakainya: `SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`,
`SysAccessPolicy`. Keempatnya tinggal di `Models/` root, bukan di dalam folder Area/Module.

Jadi ada dua hal yang perlu diputuskan sekaligus: prefix untuk tabel baru, dan status keempat tabel
lama yang selama ini tidak terdaftar.

### P.3 Usulan baris registry

| Kolom | Usulan | Alasan |
| --- | --- | --- |
| Area | `Administrator` | Layar pengelolanya ada di `Areas/Administrator/Setting/`, dan `AREA_TABS` frontend juga menempatkannya di tab Administrator |
| Module/pemilik | `PlatformAuthorization / Access Control` | Nama folder sebenarnya yang akan dibuat, supaya checker dapat mencocokkan path source |
| Category | `SHARED PLATFORM CAPABILITY` | Sama dengan `WorkflowManagement / Workflow`. Otorisasi dipakai bersama seluruh modul dan tidak dimiliki satu domain bisnis pun |
| Prefix | `Sys` | Sudah menjadi prefix de facto empat tabel otorisasi yang berjalan. Memakai prefix baru akan memecah satu kemampuan menjadi dua penampilan |
| Lifecycle | `ACTIVE / LEGACY` | `LEGACY` untuk empat tabel yang sudah ada, `ACTIVE` untuk yang baru — pola yang sama dipakai `Hrd`, `Wfp`, `Cli` |
| Kepanjangan | `Sys` = *System / Platform Authorization* | Wajib ditulis eksplisit sesuai prosedur langkah 3 |

**Alternatif bila pemilik sistem lebih suka pemisahan tegas:** prefix `Acs` = *Access Control*
untuk tabel baru saja, `Sys` tetap tidak terdaftar sebagai warisan. Konsekuensinya: satu kemampuan
punya dua prefix, dan pembaca baru akan bertanya-tanya mengapa. Tidak direkomendasikan, tetapi
merupakan pilihan yang sah.

### P.4 Nama entity yang diusulkan

| Konsep | Nama yang pernah disebut roadmap | Usulan audit ini | Alasan perubahan |
| --- | --- | --- | --- |
| 1 | `SysBusinessFeature` | `SysBusinessFeature` | Sudah tepat |
| 2 | `SysPermission` | **`SysBusinessPermission`** | `SysPermission` terlalu umum dan akan tertukar dengan `SysActionAccess` yang juga "permission". Nama harus menyebut *business* secara eksplisit |
| 3 | — | `SysBusinessPermissionMapping` | Belum pernah disebut roadmap; dibutuhkan model data |
| 4 | `SysAccessProfile` | `SysAccessProfile` | Sudah tepat |
| 5 | `SysAccessProfilePermission` | `SysAccessProfilePermission` | Sudah tepat |
| 6 | `SysOrganizationAccessProfile` | `SysOrganizationAccessProfile` | Sudah tepat |
| 7 | — | `SysOrganizationPermissionOverride` | Belum pernah disebut roadmap; dibutuhkan oleh rekomendasi C pada O.4 |

### P.5 Yang TIDAK dilakukan audit ini

Baris registry **tidak** ditambahkan ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
Prosedur langkah 5 mensyaratkan persetujuan pemilik modul lebih dulu. Selama itu belum ada,
pembuatan entity berstatus `BLOCKED` — dan memang tidak ada entity yang dibuat.

---

## Q. Kompatibilitas Legacy dan Strategi Migrasi

### Q.1 Prinsip

**Tidak ada yang dihapus.** `SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`, dan
`SysAccessPolicy` tetap hidup, tetap ditulis layar Role Access lama, dan tetap menjadi sumber izin
yang sah.

### Q.2 Bagaimana keduanya hidup berdampingan

| Aspek | Sumber lama | Sumber baru |
| --- | --- | --- |
| Yang mengisi | Layar Role Access lama (`/administrator/settings/role-access`) | Layar Manajemen Hak Akses baru |
| Yang disimpan | `SysAccessPolicy` | `SysOrganizationAccessProfile` + `SysOrganizationPermissionOverride` |
| Satuannya | `(resource, action)` teknis | Business Permission |
| Cara masuk ke keputusan izin | Langkah 4a pada L.2 | Langkah 4b pada L.2 |
| Hubungan keduanya | **Gabungan.** Tidak saling meniadakan | |

### Q.3 Tahapan migrasi yang aman

**Tahap 1 — Bayangan (shadow).** Model data dan resolusi dibuat, tetapi sumber baru **belum**
disertakan dalam keputusan izin. Sistem hanya melaporkan: "seandainya sumber baru aktif, siapa
mendapat apa". Dibandingkan dengan hak yang berlaku sekarang.

Keluarannya adalah dua laporan yang wajib ditinjau manusia:

- **Calon hak hilang** — hak yang dimiliki hari ini tetapi tidak akan diberikan profil mana pun.
  Harus nol, atau setiap barisnya dijelaskan.
- **Calon hak melebar** — hak yang tidak dimiliki hari ini tetapi akan diberikan profil.
  Setiap barisnya adalah keputusan sadar, bukan efek samping.

**Tahap 2 — Aktif tetapi aditif.** Sumber baru dinyalakan. Karena aditif, tidak ada hak yang bisa
hilang. Yang mungkin terjadi hanya pelebaran, dan pelebarannya sudah ditinjau di Tahap 1.

**Tahap 3 — Pemindahan bertahap per Departemen × Posisi.** Untuk satu pasangan yang sudah dipetakan
ke profil dan terbukti setara, baris `SysAccessPolicy`-nya dinonaktifkan. Setiap pemindahan
didahului laporan dan dapat dikembalikan.

**Tahap 4 — Layar lama menjadi mode ahli.** Layar Role Access lama tetap ada, tetapi diberi label
"tampilan teknis", dipakai untuk penelusuran masalah dan untuk resource yang belum punya Business
Permission.

**Tidak ada Tahap 5 yang menghapus tabel lama.** Penghapusan bukan bagian dari rencana ini.

### Q.4 Jaminan yang harus dibuktikan test

| Jaminan | Cara membuktikan |
| --- | --- |
| Tidak ada hak hilang diam-diam | Bandingkan himpunan izin efektif sebelum dan sesudah, per Departemen × Posisi. Selisih negatif = gagal |
| Tidak ada hak melebar diam-diam | Selisih positif wajib cocok dengan daftar yang sudah disetujui |
| Sumber baru dapat dimatikan | Test: dengan sumber baru mati, hasil `HasAccessAsync` identik dengan baseline A0 |
| Kemampuan sensitif tetap tertutup | Test: Business Permission bersensitivitas `TINGGI` yang belum ditetapkan tetap menghasilkan `403` |

---

## R. Integrasi Self Service di Masa Depan

### R.1 Status audit

Self Service **tidak diimplementasikan** oleh BE-SEC-002. Bagian ini hanya memastikan desain
sekarang tidak menutup pintunya.

### R.2 Bukti keadaan sekarang

`BE-SEC-001` mencatat 8 endpoint Self Service (presensi dan konteks HR) yang masuk daftar
kompatibilitas warisan: mereka hanya punya `[AccessAction]` tanpa `[AccessPermission]`, dan
dilindungi `[Authorize]` ditambah pembatasan data milik sendiri di dalam kode. Backend punya area
tersendiri `Areas/SelfServices/`.

### R.3 Target yang sudah diputuskan

> Pegawai aktif otomatis mendapat baseline Self Service, tanpa admin mengatur satu per satu.

### R.4 Bagaimana desain ini mendukungnya nanti

Tiga hal harus disiapkan, dan ketiganya **sudah tersedia** dalam model data usulan:

1. **Access Profile khusus baseline.** Satu profil, misalnya `EMPLOYEE_SELF_SERVICE`, berisi
   Business Permission Self Service. Bedanya dengan profil lain: penetapannya tidak lewat
   Departemen × Posisi, melainkan berlaku bagi setiap pegawai aktif. Ini menuntut satu jenis
   penetapan tambahan yang **belum** dirancang dokumen ini — dan memang tidak boleh dirancang
   sekarang, karena aturan siapa yang disebut "pegawai aktif" adalah keputusan HR.

2. **Data-scope pada Business Permission.** Konsep 2 pada model data sudah memuat kolom kebutuhan
   data-scope dengan nilai `OWN`, `SUBORDINATES`, dan `ORGANIZATION_SCOPE`. Untuk BE-SEC-002 kolom
   ini **hanya dicatat, tidak ditegakkan**. Nilainya menjadi masukan bagi fase data-scope kelak.

3. **Kesetaraan sumber.** Karena resolusi bersifat gabungan dan aditif, baseline Self Service kelak
   cukup menjadi sumber ketiga di samping `SysAccessPolicy` dan Access Profile. Tidak ada
   perubahan arsitektur yang diperlukan.

### R.5 Yang sengaja tidak dikerjakan sekarang

| Hal | Alasan |
| --- | --- |
| Penegakan data-scope | Bukan cakupan BE-SEC-002 |
| Definisi "pegawai aktif" | Keputusan HR |
| Baseline otomatis | Cakupan fase terpisah |
| Memberi `[AccessPermission]` pada 8 endpoint Self Service warisan | `BE-SEC-001` sudah mengunci daftar 69 endpoint warisan sebagai himpunan persis. Mengubahnya adalah task tersendiri |

---

## S. Kebutuhan Kontrak `GET /api/access/me` di Masa Depan

### S.1 Status

**Tidak diimplementasikan pada BE-SEC-002.** Bagian ini hanya mendefinisikan apa yang dibutuhkan
frontend, supaya fase berikutnya tidak menebak.

### S.2 Mengapa frontend membutuhkannya

Hari ini frontend tidak tahu apa pun tentang izin (bagian C.5). Akibatnya:

- menu tidak bisa disaring — semua orang melihat semua menu;
- tombol tidak bisa disembunyikan — kasir melihat tombol "Selesaikan Konsultasi";
- pesan gagal baru muncul setelah server menolak.

### S.3 Kebutuhan kontrak

| Kebutuhan | Isi | Alasan |
| --- | --- | --- |
| Hanya kode bisnis | Response memuat kode Business Permission saja | Prinsip nomor 10: frontend tidak boleh melihat `ControllerName`, `ActionName`, `SysControllerAccessId`, `SysActionAccessId` |
| Gabungan sudah dihitung server | Server mengirim hasil akhir, bukan bahan mentah per organisasi | Mencegah frontend salah menghitung gabungan |
| Pohon fitur ikut dikirim | Simpul Business Feature yang dapat dijangkau user, dengan kode dan induknya | Supaya penyaringan menu tidak perlu tabel pemetaan di frontend |
| Ukurannya wajar | Untuk dokter rawat jalan sekitar 19 kode; untuk SuperAdmin bisa ratusan | Perlu diperhatikan agar tidak membengkak |
| Ada penanda versi | Supaya frontend tahu kapan izinnya berubah | Untuk memuat ulang menu setelah admin mengubah hak |
| Tidak boleh menjadi pengaman | Server tetap menegakkan izin di setiap endpoint | Prinsip nomor 2: menu hiding bukan keamanan |
| Cocok dengan `ApiResponse<T>` | Mengikuti pola envelope repository | Konvensi backend |

### S.4 Bentuk konseptual

```
GET /api/v1/access/me

200 OK
{
  "success": true,
  "data": {
    "version": "<penanda perubahan izin>",
    "permissions": [
      "health.doctor.outpatient.view",
      "health.doctor.outpatient.soap.read",
      "health.doctor.outpatient.soap.write"
    ],
    "features": [
      { "code": "health",                     "parent": null,     "name": "Health Services" },
      { "code": "health.doctor",              "parent": "health", "name": "Dokter" },
      { "code": "health.doctor.outpatient",   "parent": "health.doctor", "name": "Rawat Jalan" }
    ]
  }
}
```

Bentuk pastinya diputuskan pada fase implementasinya, bukan sekarang.

### S.5 Perubahan frontend yang menyertainya

| Berkas | Perubahan yang dibutuhkan | Status hari ini |
| --- | --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Setiap simpul menu perlu penanda kode Business Feature | Belum ada |
| `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx` | Diganti penyaringan berbasis kode, bukan nama peran | `REPAIR` |
| `src/utils/auth/route-guard-link.js` | Diganti pemetaan rute → kode Business Permission | `REPAIR` |
| `src/components/features/auth/route-guard.jsx` | Dipasang di layout, bukan dibiarkan mati | `REPAIR` |
| Baru: penyedia state izin | Menyimpan hasil `/api/access/me` | `MISSING` |
| Baru: pembungkus tombol berizin | Menyembunyikan aksi yang tidak diizinkan | `MISSING` |

---

## T. Target Information Architecture Layar Manajemen Hak Akses

### T.1 Yang dilihat admin hari ini versus target

**Hari ini** (`administrator-role-access-view.jsx`):

```
[Administrator] [Corporate] [Health Services] [Self Services]   ← tebakan kata
└── Health Service Clinical                                      ← moduleCode backend
    ├── Doctor Consultation           [✓Read] [✓Create] [✓Update] [ Delete]
    ├── Patient Assessment            [✓Read] [ Create] [ Update] [ Delete]
    ├── Patient Vital Sign            [✓Read] [ Create] [ Update] [ Delete]
    ├── Patient Diagnosis             [✓Read] [ Create] [ Update] [ Delete]
    ├── Diagnosis Recommendation Resolver [✓Read] ...
    └── ... 
```

Admin harus tahu bahwa lima baris itu bersama-sama menyusun satu tab "SOAP".

**Target:**

```
Area Bisnis:  Health Services
  └── Menu:     Dokter
      └── Submenu: Rawat Jalan
          ├── ☑ Buka Rawat Jalan Dokter
          ├── ☑ Panggil Pasien
          ├── ☑ Kelola Alur Antrean
          ├── ☑ Mulai Konsultasi
          ├── ⚠ Selesaikan Konsultasi            (sensitif)
          ├── Hasil Skrining     ☑ Lihat   ☑ Isi / Ulangi
          ├── SOAP               ☑ Lihat   ⚠ Tulis
          ├── Diagnosis          ☑ Lihat   ⚠ Tetapkan
          ├── CPPT               ☑ Lihat   ⚠ Tulis dari SOAP
          ├── Resep              ☑ Lihat   ⚠ Tulis   ☑ Template
          ├── Tindakan           ☑ Lihat   ⚠ Pilih
          └── Surat Dokter       ⛔ belum tersedia
```

### T.2 Struktur layar yang diusulkan

Empat panel, dari kiri ke kanan:

| Panel | Isi | Pola frontend yang dipakai ulang |
| --- | --- | --- |
| 1. Konteks organisasi | Pemilih Departemen dan Posisi | `FilterSelect` (sudah dipakai layar sekarang) |
| 2. Access Profile | Profil yang berlaku untuk pasangan itu, tombol tambah/lepas | `BaseButton`, pola daftar kartu |
| 3. Pohon fitur bisnis | Area → Menu → Submenu → Feature, dengan centang Business Permission | Pola aksordion `left-sidebar` |
| 4. Rincian dan asal | Untuk satu Business Permission terpilih: makna bisnis, sensitivitas, asal hak (profil mana / override), dan technical permission di baliknya | `SummaryGrid`, panel detail |

### T.3 Aturan tampilan yang wajib

| Aturan | Alasan |
| --- | --- |
| Nama teknis **hanya** muncul di panel 4, di bagian "rincian teknis" yang tertutup secara default | Admin tidak perlu melihatnya, tetapi IT yang menelusuri masalah membutuhkannya |
| Setiap Business Permission menampilkan **asal haknya** | Menjawab "kenapa ini tercentang padahal saya tidak mencentangnya" |
| Business Permission bersensitivitas `TINGGI` diberi tanda visual dan konfirmasi terpisah | Mengurangi pemberian tidak sengaja |
| Business Permission yang pemetaannya terblokir ditampilkan **nonaktif** dengan penjelasan | Lebih jujur daripada menyembunyikannya |
| Perbedaan "dari profil" dan "override" selalu terlihat | Syarat rekomendasi C pada O.4 |
| Layar teknis lama tetap dapat dibuka | Untuk penelusuran masalah, sesuai Q.3 Tahap 4 |

### T.4 Alur kerja admin — contoh lengkap

**Tujuan.** Memberi hak akses kepada dokter rawat jalan baru di Poliklinik Umum.

**Pelaku.** Admin sistem rumah sakit. Yang menyetujui kemampuan sensitif: pemilik sistem atau
manajer yang ditunjuk.

**Pemicu.** HR memberitahu ada dokter baru mulai bekerja.

**Prasyarat.** Departemen "Poliklinik Umum" dan Posisi "Dokter Umum" sudah ada; HR sudah membuat
penempatan organisasi untuk dokter tersebut.

**Langkah:**

1. Admin membuka Manajemen Hak Akses.
2. Pada panel 1 ia memilih Departemen "Poliklinik Umum" dan Posisi "Dokter Umum".
3. Panel 2 menampilkan profil yang sudah berlaku. Misalnya masih kosong.
4. Admin menekan Tambah Profil dan memilih `DOCTOR_OUTPATIENT`.
5. Panel 3 langsung menampilkan seluruh Business Permission yang tercentang karena profil itu,
   masing-masing bertanda "dari profil `DOCTOR_OUTPATIENT`".
6. Admin melihat `Pilih Tindakan` masih abu-abu bertanda "menunggu keputusan". Ia meninggalkannya.
7. Admin menekan Simpan.
8. Dokter tersebut langsung dapat membuka halaman Rawat Jalan tanpa admin pernah melihat kata
   `PatientVitalSign` atau `PrescriptionWorkspace`.

**Aturan bisnis yang berlaku:**

- Menambahkan profil **hanya menambah** hak. Tidak ada hak yang hilang.
- Bila dokter itu juga ditempatkan HR di IGD, ia otomatis mendapat gabungan hak kedua penempatan
  tanpa admin melakukan apa pun.
- Melepas profil mencabut haknya, kecuali hak yang sama juga datang dari sumber lain.

**Perubahan status:** tidak ada status dokumen; yang berubah adalah penetapan profil, dari
"belum ada" menjadi "berlaku sejak tanggal X".

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Admin melepas profil yang haknya juga datang dari `SysAccessPolicy` lama | Hak tetap ada. Layar wajib memberi tahu, bukan diam |
| Admin mencoba memberi kemampuan sensitif yang belum diputuskan | Ditolak dengan pesan yang menyebutkan siapa yang berwenang memutuskan |
| Departemen atau Posisi dinonaktifkan | Penetapan profilnya ikut tidak berlaku, mengikuti kelayakan penempatan |
| Dua admin mengubah pasangan yang sama pada waktu hampir bersamaan | Yang kedua diberi tahu bahwa datanya sudah berubah, lalu diminta memuat ulang |

**Hasil akhir.** Pasangan Departemen × Posisi memiliki daftar Business Permission efektif yang
dapat ditelusuri asalnya, dan setiap pegawai dengan penempatan sah pada pasangan itu langsung
memperolehnya.

---

## U. Risiko Keamanan

| # | Risiko | Tingkat | Bukti | Penanganan yang diusulkan |
| ---: | --- | --- | --- | --- |
| 1 | **Pemetaan Business Permission melebar diam-diam.** Satu baris pemetaan yang salah dapat memberi ratusan orang hak yang tidak dimaksudkan | Tinggi | Sifat pemetaan satu-ke-banyak | Laporan selisih wajib sebelum tulis; test invarian; tinjauan manusia (Q.3 Tahap 1) |
| 2 | **Granularitas teknis lebih kasar daripada bisnis.** `BP-18` membawa serta `approve` dan `execute` | Tinggi | J.2 | Pecah technical permission sebagai task tersendiri (J.3). Sampai itu, `BP-18` fail closed |
| 3 | **Audio antrean tanpa technical permission.** Dua endpoint yang benar-benar dipakai hanya dilindungi `[Authorize]` | Sedang | F.12 | Keputusan owner: `[AllowAnonymous]` atau `QueueVoice.Read` |
| 4 | **SignalR hub tanpa pemeriksaan permission.** Setiap user yang login dapat bergabung ke group antrean | Sedang | `Hubs/QueueHub.cs:16` | Audit terpisah; tentukan apakah join group perlu izin |
| 5 | **Frontend tanpa guard.** Setiap user melihat seluruh menu dan seluruh tombol | Sedang (bukan kebocoran data, tetapi memperbesar permukaan percobaan) | C.2, C.3, C.4 | Fase frontend setelah `/api/access/me` |
| 6 | **Tombol Tidak Hadir memakai `fetch` mentah** sehingga melewati penanganan `401` bersama | Rendah | E.3 | Perbaikan frontend terpisah |
| 7 | **Cache izin menunda pencabutan hak.** Bila TTL dipakai, hak yang dicabut masih berlaku sesaat | Sedang | L.4 | Keputusan owner tentang batas waktu yang dapat diterima |
| 8 | **Override menumpuk tanpa kedaluwarsa** sehingga privilege creep kembali | Sedang | O.4 | Alasan wajib, masa berlaku wajib, laporan tetap |
| 9 | **Dua layar admin hidup bersamaan** dan admin memberi hak yang sama lewat dua jalur | Rendah | Q.2 | Panel 4 wajib menampilkan seluruh asal hak |
| 10 | **Surat Dokter memanggil endpoint yang tidak ada** sehingga fitur klinis tampak tersedia padahal gagal | Sedang | F.11 | Perbaikan frontend terpisah; sampai itu `BP-19` fail closed |

---

## V. Strategi Test

Mengikuti pola yang sudah terbukti pada `BE-SEC-001`
(`QuilvianSystemBackend.Tests/Security/`), yaitu test invarian yang menurunkan kebenarannya dari
source, bukan dari data yang ditanam sendiri.

### V.1 Test invarian katalog

| Test | Yang dijamin |
| --- | --- |
| `EveryBusinessPermissionMapsToExistingTechnicalPermission` | Setiap baris pemetaan menunjuk `(resource, action)` yang benar-benar ada di `PermissionRegistryDescriptor` |
| `EveryBusinessPermissionBelongsToExistingFeature` | Tidak ada izin yatim tanpa simpul fitur |
| `BusinessPermissionCodeFormatIsStable` | Kode mengikuti aturan penamaan dan tidak memuat nama class, route, atau GUID |
| `NoBusinessPermissionMapsToRetiredTechnicalPermission` | Pemetaan tidak menunjuk baris registry yang sudah ditutup |
| `SensitivePermissionsAreDeclaredExplicitly` | Setiap izin bersensitivitas `TINGGI` terdaftar sebagai himpunan persis, sehingga penambahan diam-diam menggagalkan test |

### V.2 Test cakupan pilot

| Test | Yang dijamin |
| --- | --- |
| `OutpatientDoctorPilotCoversAllThirtyTechnicalPermissions` | 30 identitas pada G.4 seluruhnya tercakup katalog |
| `DoctorOutpatientProfileGrantsEveryRequiredMapping` | Profil `DOCTOR_OUTPATIENT` menghasilkan seluruh technical permission bertanda `WAJIB` |
| `OptionalMappingsAreNotRequiredForFeatureToWork` | Tanpa `Drug.Read`, `DrugUnitConversion.Read`, `Measurement.Read`, alur resep tetap dianggap lengkap |

### V.3 Test resolusi runtime

| Test | Yang dijamin |
| --- | --- |
| `EffectiveAccessIsUnionOfLegacyAndProfileSources` | Gabungan dua sumber, bukan salah satu |
| `DisablingProfileSourceReproducesA0Baseline` | Sumber baru dimatikan → hasil identik dengan baseline `BE-SEC-001` |
| `ProfileSourceNeverRemovesLegacyGrant` | Aditif murni; tidak ada pengurangan |
| `NoDenyPrecedenceExists` | Tidak ada jalur yang bisa menolak apa yang sudah diberikan sumber lain |
| `InvalidOrganizationAssignmentNeverGrantsThroughProfile` | Aturan kelayakan penempatan A0 berlaku sama bagi sumber baru |
| `IsPrimaryAndAssignmentTypeAreNotEligibilityFilters` | Prinsip 6 dan 7 tetap berlaku di jalur baru |
| `ExpiredProfileAssignmentStopsGranting` | Masa berlaku profil benar-benar berakhir |

### V.4 Test keselamatan migrasi

| Test | Yang dijamin |
| --- | --- |
| `NoSilentPrivilegeLossAcrossMigration` | Perbandingan himpunan izin sebelum dan sesudah, per Departemen × Posisi |
| `NoSilentPrivilegeBroadeningAcrossMigration` | Setiap pelebaran cocok dengan daftar yang disetujui |
| `SensitiveCapabilityStaysFailClosedUntilAssigned` | `BP-18` dan `BP-19` menghasilkan `403` sampai diputuskan |
| `SeederNeverCreatesAccessProfileAssignment` | Tidak ada pemberian otomatis, sama seperti `ReconcileNeverCreatesAccessPolicy` |

### V.5 Verifikasi manual yang wajib

Mengikuti pola smoke test `BE-SEC-001` — dijalankan dengan akun nyata bukan SuperAdmin pada
database development:

1. Dokter dengan profil `DOCTOR_OUTPATIENT` dapat membuka seluruh tab yang seharusnya.
2. Dokter yang sama **ditolak** pada kemampuan sensitif yang belum ditetapkan.
3. Akun tanpa profil apa pun ditolak di seluruh halaman.
4. Dokter dengan dua penempatan mendapat gabungan izin keduanya.
5. Melepas profil benar-benar mencabut hak.
6. Mematikan sumber baru mengembalikan perilaku persis seperti sebelum perubahan.

---

## W. Fase Implementasi Setelah Persetujuan

Setiap fase adalah task terpisah dengan persetujuan sendiri. Tidak ada fase yang boleh dimulai
sebelum fase sebelumnya diterima.

| Fase | Task ID usulan | Isi | Prasyarat |
| --- | --- | --- | --- |
| 0 | — | **Keputusan pemilik sistem** atas 7 butir pada bagian Y, termasuk persetujuan baris registry prefix | Dokumen ini |
| 1 | `BE-SEC-003` | Katalog dan model data: 7 entity, migration aditif, seeder katalog Business Permission pilot. **Belum** menyentuh jalur otorisasi | Fase 0; registry prefix disetujui |
| 2 | `BE-SEC-004` | Resolusi bayangan: service resolusi Business Permission → technical permission, mode laporan saja. **Belum** ikut memutuskan izin | Fase 1 |
| 3 | `BE-SEC-005` | Pemecahan granularitas technical permission (J.3) — task tersendiri dengan prosedur klasifikasi, laporan, dan rebind aman seperti A0 | Fase 2; keputusan owner atas daftar pemecahan |
| 4 | `BE-SEC-006` | Sumber izin kedua diaktifkan pada `AccessPermissionService`, aditif, dapat dimatikan lewat konfigurasi | Fase 2 dan 3; laporan selisih ditinjau |
| 5 | `BE-SEC-007` | API admin Business Permission dan Access Profile | Fase 4 |
| 6 | `BE-SEC-008` | `GET /api/v1/access/me` | Fase 5 |
| 7 | `FE-SEC-001` | Penyedia state izin frontend + perbaikan `RouteGuard` dan `filterMenuItemsByRole` | Fase 6 |
| 8 | `FE-SEC-002` | Layar Manajemen Hak Akses baru sesuai bagian T | Fase 5 dan 7 |
| 9 | `FE-SEC-003` | Guard tab dan tombol pada halaman pilot Dokter → Rawat Jalan | Fase 7 |
| 10 | Terpisah | Baseline Self Service otomatis; penegakan data-scope | Fase 4; keputusan HR |

**Di luar rantai ini, sebagai perbaikan mandiri:**

| Perbaikan | Repository | Alasan tidak masuk rantai |
| --- | --- | --- |
| Tab Surat Dokter menunjuk `medical-certificates` | Frontend | Cacat kontrak yang sudah ada; tidak menunggu Business Permission |
| Tombol Tidak Hadir memakai `InstanceAxios` | Frontend | Kebersihan arsitektur |
| Klasifikasi dua endpoint audio antrean | Backend | Utang terbuka `BE-SEC-001` |

---

## X. Berkas yang Diperkirakan Berubah per Fase

> Daftar perkiraan berdasarkan struktur repository saat ini. Bukan wewenang untuk mengubahnya.

### Fase 1 — `BE-SEC-003` (backend)

| Berkas | Sifat |
| --- | --- |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Ubah — tambah baris, **hanya setelah persetujuan** |
| `Areas/Administrator/PlatformAuthorization/Models/SysBusinessFeature.cs` | Baru |
| `.../Models/SysBusinessPermission.cs` | Baru |
| `.../Models/SysBusinessPermissionMapping.cs` | Baru |
| `.../Models/SysAccessProfile.cs` | Baru |
| `.../Models/SysAccessProfilePermission.cs` | Baru |
| `.../Models/SysOrganizationAccessProfile.cs` | Baru |
| `.../Models/SysOrganizationPermissionOverride.cs` | Baru |
| `Repositories/ApplicationDbContext.cs` | Ubah — pendaftaran `DbSet` |
| `Repositories/Configurations/...` | Baru — konfigurasi EF per entity |
| `Migrations/<timestamp>_BusinessPermissionCatalog.cs` | Baru — aditif |
| `Seeders/BusinessPermissionCatalogSeeder.cs` | Baru — mendaftarkan katalog, **tidak pernah** memberi hak |
| `Program.cs` | Ubah — registrasi seeder dan service |
| `QuilvianSystemBackend.Tests/Security/BusinessPermissionCatalogTests.cs` | Baru |

### Fase 2 — `BE-SEC-004` (backend)

| Berkas | Sifat |
| --- | --- |
| `Services/Security/BusinessPermissionResolutionService.cs` | Baru — termasuk mode laporan tanpa tulis |
| `Program.cs` | Ubah — registrasi service |
| `QuilvianSystemBackend.Tests/Security/BusinessPermissionResolutionTests.cs` | Baru |

### Fase 3 — `BE-SEC-005` (backend)

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Ubah — `[AccessPermission]` pada `approve`, `execute` |
| `.../DoctorConsultationController.cs` | Ubah — `complete` |
| `.../PatientVitalSignController.cs` | Ubah — `verify` |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionTemplateController.cs` | Ubah — `apply` |
| `Areas/HealthServices/RegistrationManagement/Controllers/QueueVoiceController.cs` | Ubah — klasifikasi dua endpoint audio |
| `QuilvianSystemBackend.Tests/Security/PermissionRegistryInvariantTests.cs` | Ubah — perbarui daftar terkunci |
| `QuilvianSystemBackend.Tests/Security/CanonicalSecurityContractTests.cs` | Ubah — daftar 69 endpoint warisan bila terdampak |

### Fase 4 — `BE-SEC-006` (backend)

| Berkas | Sifat |
| --- | --- |
| `Services/Security/AccessPermissionService.cs` | Ubah — tambah sumber izin kedua |
| `appsettings.json` / `appsettings.Development.json` | Ubah — sakelar aktivasi (nama kunci saja, tanpa nilai rahasia) |
| `QuilvianSystemBackend.Tests/Security/EffectiveAccessUnionTests.cs` | Baru |

### Fase 5 — `BE-SEC-007` (backend)

| Berkas | Sifat |
| --- | --- |
| `Areas/Administrator/PlatformAuthorization/Controllers/BusinessAccessController.cs` | Baru |
| `Areas/Administrator/PlatformAuthorization/DTOs/...` | Baru |
| `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` | **Tidak diubah** — layar lama tetap utuh |

### Fase 6 — `BE-SEC-008` (backend)

| Berkas | Sifat |
| --- | --- |
| `Controllers/AccessMeController.cs` (atau di dalam area yang sesuai) | Baru |

### Fase 7 — `FE-SEC-001` (frontend)

| Berkas | Sifat |
| --- | --- |
| `src/lib/services/access/access-me.service.js` | Baru |
| `src/lib/state/slice/access/access-slice.jsx` | Baru |
| `src/lib/state/store.jsx` | Ubah — daftarkan reducer |
| `src/lib/hooks/access/use-business-permission.js` | Baru |
| `src/utils/auth/route-guard-link.js` | Ubah — ganti peta peran dengan peta kode |
| `src/components/features/auth/route-guard.jsx` | Ubah — dipasang, bukan dibiarkan mati |
| `src/utils/menu-sidebar/menu-items.jsx` | Ubah — tambah penanda kode fitur per simpul |
| `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx` | Ubah — penyaringan berbasis kode |
| `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx` | Ubah — sumber penyaringan |

### Fase 8 — `FE-SEC-002` (frontend)

| Berkas | Sifat |
| --- | --- |
| `src/components/view/administrator/settings/administrator-role-access-view.jsx` | Ubah besar atau diganti view baru berdampingan |
| `src/style/administrator/settings/...` | Ubah/baru |
| `src/lib/services/administrator/settings/business-access.service.js` | Baru |

### Fase 9 — `FE-SEC-003` (frontend)

| Berkas | Sifat |
| --- | --- |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Ubah — guard tab |
| `src/components/features/health-services/doctor-queue-features/ConsultationTabs.jsx` | Ubah — sembunyikan tab tanpa izin |
| `src/components/features/health-services/doctor-queue-features/QueuePatientCard.jsx` | Ubah — guard tombol |
| `src/components/features/health-services/doctor-queue-features/FinalizeConsultationPanel.jsx` | Ubah — guard tombol |

---

## Y. Keputusan yang Masih Menunggu Pemilik Sistem

Setiap butir diklasifikasikan sesuai disiplin yang diminta.

### Y.1 `AUTO-RESOLVABLE` — sudah terjawab source, tidak perlu ditanyakan

| Pertanyaan | Jawaban dari source |
| --- | --- |
| Apakah frontend saat ini bergantung pada `ControllerName`/`ActionName`/`SysControllerAccessId`/`SysActionAccessId`? | **Tidak.** Login tidak pernah mengirim izin (C.5) |
| Di mana registry navigasi kanonik? | `src/utils/menu-sidebar/menu-items.jsx` (D.1) |
| Apakah `RouteGuard` dan `filterMenuItemsByRole` sedang menjaga sesuatu? | **Tidak.** Keduanya mati atau tidak dipakai (C.2, C.3) |
| Berapa banyak endpoint, controller, modul, dan permission yang dipakai pilot? | 53 / 16 / 4 / 30 (bagian G) |
| Apakah pemisahan `read`/`write` bermakna hari ini? | Sebagian. Terhalang granularitas teknis (J.2) |
| Apakah Business Permission perlu menggantikan `[AccessPermission]`? | **Tidak.** Pendekatan A cukup dan paling aman (K, L) |
| Apakah `IsPrimary`/`AssignmentType` perlu diperiksa? | **Tidak.** Sudah diputuskan A0 dan sudah dikunci test |
| Apakah tabel `Sys*` lama perlu dihapus? | **Tidak.** Rencana ini tidak menghapusnya sama sekali (Q.1) |

### Y.2 `ARCHITECTURE RECOMMENDATION` — satu rekomendasi, siap disetujui atau ditolak

| # | Butir | Rekomendasi |
| ---: | --- | --- |
| 1 | Arsitektur runtime | **Pendekatan A** sebagai tujuan; sumber izin kedua yang aditif dan dapat dimatikan sebagai mekanisme transisi (L) |
| 2 | Cara resolusi | **Runtime**, bukan materialisasi ke `SysAccessPolicy` (L.3) |
| 3 | Direct permission vs Access Profile | **Profil sebagai template + override terkendali**, dengan lima pagar pengaman (O.4) |
| 4 | Nama entity | `SysBusinessPermission`, bukan `SysPermission` (P.4) |
| 5 | Prefix registry | `Sys`, dengan kepanjangan *System / Platform Authorization* (P.3) |
| 6 | Profil pilot | Dua saja: `DOCTOR_OUTPATIENT` dan `DOCTOR_OUTPATIENT_OBSERVER` (M.2) |
| 7 | Katalog pilot | 19 Business Permission (bagian I) |
| 8 | Fitur placeholder | `Penunjang Medis` dan `CDSS` **tidak** mendapat Business Permission sekarang (H) |
| 9 | Urutan fase | Katalog → resolusi bayangan → pemecahan granularitas → aktivasi → API admin → `/api/access/me` → frontend (W) |

### Y.3 `OWNER BUSINESS DECISION` — hanya ini yang benar-benar butuh keputusan Anda

| # | Keputusan | Mengapa hanya Anda yang bisa memutuskan | Akibat bila belum diputuskan |
| ---: | --- | --- | --- |
| **1** | **Persetujuan baris registry prefix** untuk Platform Authorization | `QBE-MOD-002` mensyaratkan persetujuan pemilik modul | Seluruh Fase 1 `BLOCKED`. Tidak ada entity boleh dibuat |
| **2** | **Apakah dokter rawat jalan boleh menyetujui dan melaksanakan tindakan?** Hari ini `BP-18` membawa serta `PatientProcedure.Approve` dan `Execute` | Kewenangan klinis dan finansial. Tindakan menimbulkan tagihan | `BP-18` fail closed. Dokter tidak dapat menghapus pilihan tindakan dari draft |
| **3** | **Siapa yang berwenang menandatangani surat dokter?** (`BP-19`) | Dokumen berakibat hukum dan ketenagakerjaan | `BP-19` fail closed. Tab Surat Dokter tetap tidak berfungsi (dan memang masih rusak secara teknis) |
| **4** | **Apakah "boleh menulis SOAP" harus terpisah dari "boleh menyelesaikan konsultasi"?** Hari ini keduanya satu `DoctorConsultation.Update` | Menentukan apakah Fase 3 wajib dikerjakan sebelum Fase 4 | Pemisahan `BP-05` dan `BP-09` semu |
| **5** | **Klasifikasi dua endpoint audio antrean:** `[AllowAnonymous]` atau `QueueVoice.Read`? | Menyangkut apakah rekaman panggilan boleh diakses siapa saja yang login | Fitur panggil suara bergantung pada endpoint yang belum diklasifikasi |
| **6** | **Berapa lama penundaan pencabutan hak yang dapat diterima?** 0 detik (tanpa cache) atau 30–60 detik (dengan cache) | Menyangkut seberapa cepat hak harus benar-benar hilang setelah dicabut | Strategi performa Fase 4 belum dapat dikunci |
| **7** | **Apakah Access Profile boleh dipakai lintas rumah sakit (`HospitalId`) atau harus per rumah sakit?** | Menyangkut model tenancy yang tidak dapat disimpulkan dari source | Model data Fase 1 belum lengkap |

### Y.4 Yang diwarisi dari `BE-SEC-001` dan masih terbuka

Dicatat ulang agar tidak hilang. Bukan hasil temuan baru audit ini.

| Butir | Status |
| --- | --- |
| Pemberian kemampuan sensitif (kasir, refund, write-off, penerimaan sampel, penandatanganan discharge) | Masih menunggu keputusan |
| 17 policy inert (5 `SEMANTIC_CHANGED`, 12 `REMOVED_CAPABILITY`) | Sengaja fail closed |
| Dua baris proyeksi legacy-unresolved | Dipertahankan, menunggu peninjauan |
| Penerapan ke lingkungan selain development | Belum |

---

## Z. Klasifikasi Kemampuan (Kontrak Bukti)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-01` | Identitas technical permission kanonik | Backend Platform | `Services/Security/PermissionRegistryDescriptor.cs@e1d1121` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-02` | Resolusi izin efektif multi-organisasi | Backend Platform | `Services/Security/AccessPermissionService.cs@e1d1121` | `Extend` | Tambah sumber izin kedua | Sedang |
| `CAP-03` | Registry kemampuan aplikasi | Backend Platform | `Seeders/AccessMenuSeeder.cs@e1d1121` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-04` | Pemberian hak Departemen × Posisi | Backend Platform | `Models/SysAccessPolicy.cs@e1d1121` | `Ready to reuse` | Tetap sebagai sumber lama | Rendah |
| `CAP-05` | API pengelolaan hak akses | Backend Administrator | `Areas/Administrator/Setting/Controllers/RoleAccessController.cs@e1d1121` | `Extend` | API baru berdampingan, layar lama tidak diubah | Rendah |
| `CAP-06` | Katalog Business Feature dan Business Permission | — | Tidak ada | `Missing` | Seluruh model data bagian O | Sedang |
| `CAP-07` | Access Profile | — | Tidak ada | `Missing` | Seluruh model data bagian O | Sedang |
| `CAP-08` | Kontrak izin untuk frontend (`/api/access/me`) | — | Tidak ada | `Missing` | Bagian S | Sedang |
| `CAP-09` | Registry navigasi frontend | Frontend | `src/utils/menu-sidebar/menu-items.jsx@2b9e3b0` | `Reuse with adapter` | Perlu penanda kode fitur per simpul | Rendah |
| `CAP-10` | Penyaringan menu berbasis izin | Frontend | `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx@2b9e3b0` | `Repair` | Tidak menyaring apa pun | Sedang |
| `CAP-11` | Penjagaan rute frontend | Frontend | `src/components/features/auth/route-guard.jsx@2b9e3b0`; `src/utils/auth/route-guard-link.js@2b9e3b0` | `Repair` | Tidak dipakai; peta rutenya usang | Sedang |
| `CAP-12` | Penanganan akses ditolak di layar | Frontend | `src/components/features/base-features/access-denied-gate.jsx@2b9e3b0` | `Ready to reuse` | Reaktif, bukan preventif — memang bukan pengaman | Rendah |
| `CAP-13` | State izin di frontend | — | Tidak ada | `Missing` | Bagian S.5 | Sedang |
| `CAP-14` | Surat dokter pada alur rawat jalan | Frontend + Backend | `use-doctor-certificate.js:304-305@2b9e3b0` vs `MedicalCertificateController.cs@e1d1121` | `Conflict` | Rute frontend salah | Sedang |
| `CAP-15` | Perlindungan audio panggilan antrean | Backend Registration | `QueueVoiceController.cs:45,63@e1d1121` | `Repair` | Tidak punya `[AccessPermission]` | Sedang |
| `CAP-16` | Otorisasi realtime antrean | Backend Registration | `Hubs/QueueHub.cs:16@e1d1121` | `Unknown` | Hanya `[Authorize]`; belum diaudit apakah cukup | Sedang |
| `CAP-17` | Granularitas technical permission | Backend lintas modul | Bagian J.2 | `Repair` | Sembilan identitas menutupi banyak endpoint sekaligus | Tinggi |
| `CAP-18` | Baseline Self Service otomatis | — | Tidak ada | `Missing` | Di luar cakupan BE-SEC-002 | Rendah |

**Pemicu impact scan.** Peta ini menjadi `stale` bila SHA backend berubah dari `e1d1121` atau SHA
frontend berubah dari `2b9e3b0`. Yang wajib diperiksa ulang saat itu: jumlah pasangan
`[AccessPermission]`, daftar 30 identitas pilot, isi `menu-items.jsx`, dan status `CAP-14` sampai
`CAP-17`.

**Yang tidak diketahui dan sengaja tidak ditebak:**

| Hal | Mengapa `Unknown` |
| --- | --- |
| Apakah `QueueHub` perlu izin per-group | Perlu keputusan keamanan, bukan pembacaan source |
| Apakah `Drug.Read` juga dipakai layar master data oleh peran lain | Perlu penelusuran halaman master data, di luar cakupan pilot |
| Model tenancy `HospitalId` pada Access Profile | Tidak dapat disimpulkan dari source |

---

## Pernyataan Penutup

Audit ini **read-only**. Yang dihasilkan hanya dokumen ini.

| Batasan | Status |
| --- | --- |
| Perubahan source aplikasi | **Tidak ada** |
| Entity baru | **Tidak ada** |
| Migration | **Tidak ada** |
| Eksekusi database | **Tidak ada** |
| Perubahan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Tidak ada** — menunggu persetujuan |
| `git commit` | **Tidak ada** |
| `git push` | **Tidak ada** |
| Working tree backend | Bersih sebelum audit; hanya bertambah dokumen ini |
| Working tree frontend | Bersih, tidak disentuh |

Implementasi BE-SEC-002 belum diberi izin. Fase 1 tetap `BLOCKED` sampai tujuh keputusan pada
bagian Y.3 diputuskan pemilik sistem.
