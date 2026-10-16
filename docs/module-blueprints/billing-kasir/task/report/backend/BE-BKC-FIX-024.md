# Laporan Perubahan Backend — `BE-BKC-FIX-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-024` |
| Judul | Konsolidasikan 8 `ModuleCode` billing-kasir menjadi satu kode kanonik `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| Sumber | [`PLAN-REPAIR-BKC-004`](../../docs/plan-repair/plan-repair-004-kartu-billing-ganda-manajemen-role.md) `FIX-BKC-004-01`; cacat [`ISSUE-BKC-004`](../../docs/issue/issue-004-kartu-billing-ganda-manajemen-role.md) / `ISS-BKC-004-01` |
| Keputusan | Opsi B pada plan repair — seeder migrasi aman yang memindahkan relasi `SysControllerAccess.ModuleId`, bukan mengganti literal `moduleCode` langsung (opsi A ditolak karena berisiko menghapus `SysAccessPolicy` yang sudah diberikan admin secara senyap) |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit backend saat dikerjakan | `738e03a0` (`Yasmina`) + working tree; belum di-commit |
| Tanggal | 2026-10-16 |
| Status | 🟡 **Source selesai, perubahan 9 titik atribut pada 8 controller terverifikasi lewat pembacaan ulang.** `dotnet build` **TIDAK dijalankan** atas permintaan eksplisit pengguna di tengah task ini ("jangan lakukan build backend secara automatis") — bukan dilewatkan sengaja oleh agent. Pengguna perlu menjalankan `dotnet build` sendiri sebelum task ini dianggap selesai penuh |

---

## 1. Masalah yang diperbaiki

Layar **Manajemen Role** (Administrator → Pengaturan) menampilkan daftar modul sebagai satu kartu
per kode modul internal. Delapan controller pada modul Billing — `BillingConsumerHandoffs`,
`BillingDepositOrders`, `BillingInvoices`, `BillingPatientFunds`, `BillingFinalizations`,
`BillingFinancialExceptions`, `BillingSettlements`, `InpatientClearance`, dan `CashierShifts` —
masing-masing didaftarkan dengan **kode modul internal yang berbeda-beda** (tujuh kode berbeda
untuk sembilan controller, karena tiga di antaranya sudah berbagi satu kode yang sama), padahal
nama yang ditampilkan ke layar ditulis identik: "Health Service Billing Management". Akibatnya,
admin yang membuka layar Manajemen Role melihat **delapan kartu berlabel "Billing" yang sama
persis** tanpa cara membedakannya — harus membuka satu per satu untuk tahu kartu mana berisi hak
akses apa.

## 2. Proses bisnis

**Sebelum perbaikan:** admin membuka Manajemen Role untuk mengatur hak akses fitur Billing bagi
suatu peran (role). Daftar modul menampilkan delapan baris "Billing" dengan jumlah hak akses yang
berbeda-beda (mis. "Billing 0/3", "Billing 0/2", "Billing 0/11") tanpa keterangan pembeda apa pun.
Admin harus membuka kedelapannya satu-satu untuk memastikan tidak ada hak akses yang terlewat
diberikan.

**Sesudah perbaikan:** kedelapan kartu itu menjadi **satu kartu "Billing"** berisi gabungan seluruh
97 hak akses dari sembilan controller tersebut. Admin cukup membuka satu kartu untuk mengatur
semua hak akses fitur Billing inti sekaligus. Dua kartu lain milik modul yang sama — "Billing
Master Data" (7 controller) dan "Billing Management Petty Cash" (1 controller) — **sengaja tetap
terpisah** sesuai keputusan pemilik modul, karena namanya sudah jelas berbeda dan konsisten dengan
pola modul lain di layar yang sama (semuanya memisahkan Master Data dari modul utamanya).

**Jalur migrasi data (bukan sekadar ganti kode):** mengganti kode modul di controller secara
langsung tanpa migrasi akan membuat `AccessMenuSeeder` kehilangan jejak baris lama (ia mengenali
baris lewat pasangan kode modul + nama controller) — seluruh hak akses yang sudah diberikan admin
ke kedelapan controller ini sebelumnya akan berhenti berlaku **tanpa galat apa pun** (role yang
tadinya bisa akses invoice/deposit/shift kasir tiba-tiba ditolak 403). Karena itu, urutan
jalannya:

1. Saat aplikasi start, `BillingModuleCodeConsolidationSeeder` berjalan **sebelum**
   `AccessMenuSeeder`.
2. Untuk masing-masing dari tujuh kode modul lama, seeder memindahkan baris akses controller
   (`SysControllerAccess`) yang sudah ada ke modul kanonik — **ID barisnya dipertahankan**, hanya
   relasi modulnya yang dipindah.
3. Karena ID baris tidak berubah, seluruh hak akses (`SysAccessPolicy`) yang sudah pernah
   diberikan admin ke role manapun untuk kedelapan controller ini **tetap berlaku** tanpa perlu
   diberikan ulang.
4. `AccessMenuSeeder` berjalan sesudahnya dan menemukan kondisi sudah konsisten — tidak membuat
   baris baru untuk delapan controller ini.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/PharmacyManagement/Seeders/PharmacyModuleCodeConsolidationSeeder.cs` —
  pola migrasi aman yang sudah terbukti bekerja untuk kasus identik (konsolidasi modul Farmasi).
- `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` — memastikan mekanisme
  "satu baris `SysApplicationModule` = satu kartu" di layar Manajemen Role tidak perlu diubah.
- `Seeders/AccessMenuSeeder.cs` — memastikan urutan jalan seeder baru (harus sebelum ini).
- `Program.cs` — titik wiring seeder saat start aplikasi.
- Sembilan file controller billing-kasir yang atribut `[AccessController]`-nya diubah (lihat 3.2).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Seeders/BillingModuleCodeConsolidationSeeder.cs` (**baru**) | Seeder migrasi yang memindahkan `SysControllerAccess.ModuleId` dari tujuh kode modul lama ke kode kanonik `HEALTH_SERVICE_BILLING_MANAGEMENT`, mengikuti pola `PharmacyModuleCodeConsolidationSeeder` persis, digeneralisasi untuk menerima daftar (`KodeLegacyList`) alih-alih satu kode tunggal — Pharmacy hanya punya 1 kode lama, Billing punya 7. Tidak ada baris yang dihapus fisik; hanya `IsActive=false`/`IsDelete=true` pada baris yang relasinya sudah dipindahkan, dan tidak ada `SysAccessPolicy` yang dibuang |
| `Program.cs` | Tambah `using` untuk namespace seeder baru; daftarkan `BillingModuleCodeConsolidationSeeder.SeedAsync` di urutan startup, setelah `PharmacyModuleCodeConsolidationSeeder` dan **sebelum** `AccessMenuSeeder`, dengan komentar yang menjelaskan alasannya (sama seperti komentar Farmasi di atasnya) |
| `BillingConsumerHandoffsController.cs` | `moduleCode` atribut `[AccessController]`: `HEALTH_SERVICE_BILLING_MANAGEMENT_CONSUMER_HANDOFF` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingDepositOrdersController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingInvoicesController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingPatientFundsController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingFinalizationsController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINALIZATION` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingFinancialExceptionsController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `BillingSettlementsController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_SETTLEMENT` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `InpatientClearanceController.cs` | `moduleCode` (argumen posisional pertama atribut `[AccessController]`): `HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |
| `CashierShiftsController.cs` | `moduleCode`: `HEALTH_SERVICE_BILLING_MANAGEMENT_CASHIER_SHIFT` → `HEALTH_SERVICE_BILLING_MANAGEMENT` |

Pada seluruh sembilan file di atas, `moduleName` dan `displayName` (label per controller, bukan
label modul) **tidak diubah** — hanya kode modul internalnya. Empat controller yang sudah memakai
kode kanonik (`BillingChargeReconciliation`, `EncounterBillingSummary`, `BillingFolio`,
`PatientBillingSummary`) dan controller `MasterData`/`PettyCashVouchers` **tidak disentuh**, sesuai
scope yang disetujui pemilik modul (K-01 pada `issue-004`).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada route, request, atau response yang berubah pada endpoint manapun; hanya metadata internal kode modul untuk layar Manajemen Role |
| Database | Tidak ada migration EF Core (tidak ada perubahan schema/model persisted). Perubahan data terjadi lewat seeder (`BillingModuleCodeConsolidationSeeder`) yang berjalan otomatis saat aplikasi start, memindahkan baris `SysControllerAccess` existing ke modul kanonik — **belum dijalankan** pada database manapun karena aplikasi belum dijalankan ulang (lihat bagian 5) |
| Keamanan/Auth | Hak akses (`SysAccessPolicy`) yang sudah diberikan admin untuk kedelapan controller ini dirancang **tetap berlaku** setelah seeder berjalan (ID baris dipertahankan, bukan dibuat ulang) — tapi ini baru tervalidasi lewat pembacaan logika, **belum** diverifikasi dengan menjalankan aplikasi pada database yang punya data `SysAccessPolicy` nyata untuk controller-controller ini (lihat Risiko tersisa) |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat, mengubah, atau menghapus endpoint apa pun. Seluruh
route pada sembilan controller yang disentuh tetap identik persis seperti sebelumnya.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pembacaan ulang (review) seluruh 9 titik perubahan atribut `[AccessController]` | Tidak ada literal `moduleCode` lama yang tersisa pada atribut aktif manapun di `Areas/HealthServices/BillingManagement/`; hanya tersisa satu komentar historis yang menyebut kode lama sebagai referensi dokumentasi (tidak memengaruhi kompilasi/perilaku) | `PASS` | `grep` ulang atas ketujuh pola kode lama setelah seluruh edit selesai |
| `dotnet build` | **Tidak dijalankan.** Sempat dimulai di latar belakang, lalu dihentikan paksa di tengah tahap `restore` (sebelum kompilasi dimulai) karena pengguna secara eksplisit meminta agar `dotnet build`/`dotnet run` tidak dijalankan otomatis di repository ini pada task ini | `NOT RUN` | Instruksi pengguna pada sesi ini: "jangan lakukan build backend secara automatis" |
| Verifikasi manual: jalankan aplikasi pada database dev yang punya `SysAccessPolicy` nyata untuk salah satu dari 8 controller lama, buka layar Manajemen Role, pastikan tersisa 1 kartu "Billing" dan role yang sudah punya akses tetap bisa mengakses endpoint terkait | — | `NOT RUN` | Memerlukan aplikasi dijalankan (`dotnet run`), yang juga termasuk dalam permintaan "jangan dijalankan otomatis" pada task ini |

**Uji manual:** `NOT FEASIBLE` pada task ini — memerlukan `dotnet build`/`dotnet run` yang
sengaja tidak dijalankan sesuai permintaan eksplisit pengguna.

**Tidak dijalankan:** `dotnet build`, menjalankan aplikasi, dan pengujian layar Manajemen Role
secara langsung — ketiganya memerlukan menjalankan backend, yang diminta pengguna untuk **tidak**
dilakukan otomatis pada sesi ini. Pengguna perlu menjalankan sendiri `dotnet build` lalu
`dotnet run`/`dotnet ef` sesuai kebutuhan untuk memverifikasi migrasi data berjalan benar pada
database dev sebelum perbaikan ini dianggap selesai penuh.

## 6. Acceptance criteria dan Definition of Done

Disalin dari `PLAN-REPAIR-BKC-004` bagian 4 (`FIX-BKC-004-01`):

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setelah seeder migrasi dan `AccessMenuSeeder` berjalan pada database dev dengan data lama, hanya tersisa 1 baris aktif `SysApplicationModule` kanonik menaungi 13 controller; 7 baris lama `IsActive=false` | **Belum terpenuhi** — logika sudah ditulis sesuai pola Pharmacy yang terbukti, tapi belum dijalankan pada database manapun (aplikasi belum di-start ulang) |
| 2. `SysControllerAccess`/`SysActionAccess` milik 13 controller tetap `Id` yang sama sebelum/sesudah | **Belum terpenuhi** — sama seperti kriteria 1, logika menjamin ini (`controller.ModuleId = modulKanonik.Id` tanpa mengubah `Id`), tapi belum diverifikasi lewat eksekusi nyata |
| 3. `SysAccessPolicy` existing tetap valid, role yang sudah punya akses tidak menerima 403 setelah migrasi | **Belum terpenuhi** — belum ada data uji `SysAccessPolicy` yang dijalankan lewat migrasi ini |
| 4. Pada database baru (belum pernah punya data modul Billing), seeder migrasi no-op aman | **Belum terpenuhi** — belum dijalankan |
| 5. Layar Manajemen Role menampilkan 1 kartu "Billing"; Master Data dan Petty Cash tetap terpisah | **Belum terpenuhi** — perlu aplikasi berjalan dan layar dibuka untuk verifikasi visual |
| 6. Seeder migrasi idempotent (jalan dua kali tidak menghasilkan perubahan tambahan) | **Belum terpenuhi** — logika sudah menangani ini (baris 137-144 pada seeder: bila `controllerLegacy.Count == 0`, hanya menutup modul lama, tanpa kerja tambahan), tapi belum diverifikasi lewat eksekusi dua kali |

**Definition of Done belum tercapai sepenuhnya.** Source selesai dan diyakini benar berdasarkan
kesesuaian pola dengan precedent yang sudah teruji (`PharmacyModuleCodeConsolidationSeeder`), tapi
**tidak satu pun** acceptance criteria di atas sudah diverifikasi lewat eksekusi nyata, karena
`dotnet build`/`dotnet run` sengaja tidak dijalankan pada task ini atas permintaan pengguna.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengguna menginterupsi task di tengah proses untuk meminta agar `dotnet build` tidak dijalankan otomatis — proses build yang sudah terlanjur dimulai di latar belakang segera dihentikan paksa (belum sempat mengkompilasi apa pun, masih di tahap `restore` paket NuGet) |
| Masalah yang diketahui | Tidak ada migration EF Core yang dibutuhkan untuk task ini (tidak ada perubahan schema), tapi perubahan DATA lewat seeder tetap memerlukan aplikasi dijalankan ulang (bukan hanya `dotnet build`) agar benar-benar berefek pada database manapun |
| Risiko tersisa | **Belum diverifikasi dengan data nyata**: logika pemindahan (`GabungkanAsync`, penanganan tabrakan nama controller) adalah bagian paling berisiko bila ada kesalahan — berpotensi mencabut hak akses role secara tidak sengaja bila logikanya ternyata punya celah yang tidak terlihat dari pembacaan source saja. **Wajib** diuji pada database dev yang sudah punya data `SysAccessPolicy` nyata untuk billing-kasir sebelum dianggap aman diterapkan lebih luas |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meminta build backend tidak dijalankan otomatis, di tengah task ini sedang menjalankan `dotnet build` validasi. Build dihentikan paksa (`kill`); tidak ada perubahan source yang hilang atau perlu dipulihkan akibat interupsi ini — hanya proses build latar belakang yang dihentikan |
| Status Git | Lihat `git status --short` — file baru: `Areas/HealthServices/BillingManagement/Seeders/BillingModuleCodeConsolidationSeeder.cs`; diubah: `Program.cs` dan 9 file controller billing-kasir yang disebut pada 3.2, beserta berkas dokumentasi blueprint (issue/plan-repair/laporan task/roadmap) |
| Langkah berikutnya | Pengguna menjalankan `dotnet build` sendiri untuk memastikan kompilasi lulus, lalu `dotnet run`/restart aplikasi pada database dev (idealnya yang sudah punya data `SysAccessPolicy` nyata untuk controller billing-kasir) untuk memverifikasi seluruh 6 acceptance criteria di atas secara nyata, lalu buka layar Manajemen Role untuk konfirmasi visual 1 kartu "Billing" |
