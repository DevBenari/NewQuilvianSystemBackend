# PLAN-REPAIR-004 — Konsolidasikan 8 ModuleCode Billing menjadi satu kode kanonik

```yaml
plan_id: PLAN-REPAIR-BKC-004
issue: ../issue/issue-004-kartu-billing-ganda-manajemen-role.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-16"
diputuskan_oleh: "Pemilik modul Billing — perintah langsung 'Perbaiki FE dan BE', scope dikonfirmasi lewat pertanyaan klarifikasi (hanya 8 controller berlabel identik, Master Data dan Petty Cash tetap terpisah)"
tanggal_keputusan: "2026-10-16"
basis_source_backend: "738e03a0 (Yasmina)"
basis_source_frontend: "Tidak relevan - tidak ada perubahan frontend"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-BKC-004-01` | `ISS-BKC-004-01` | Konsolidasikan 8 `ModuleCode` billing-kasir menjadi satu kode kanonik `HEALTH_SERVICE_BILLING_MANAGEMENT`, lewat seeder migrasi aman (bukan ganti literal langsung) | BE | 1 | `BE-BKC-FIX-024` | 🟡 SEBAGIAN | [BE-BKC-FIX-024.md](../../task/report/backend/BE-BKC-FIX-024.md) |

**Ringkasan: 0 dari 1 perbaikan selesai sepenuhnya.** Source selesai (seeder baru + 9 titik
atribut controller diubah, mengikuti pola `PharmacyModuleCodeConsolidationSeeder` yang sudah
terbukti aman). `dotnet build` dan seluruh verifikasi runtime **belum dijalankan** — pengguna
meminta di tengah task agar `dotnet build`/`dotnet run` tidak dijalankan otomatis di repository
ini. Tidak satu pun dari 6 acceptance criteria `FIX-BKC-004-01` sudah terverifikasi lewat eksekusi
nyata. Lihat `BE-BKC-FIX-024.md` bagian 5-7 untuk rincian dan langkah berikutnya.

## 2. Solusi terpilih per temuan

### ISS-BKC-004-01 — 8 controller billing-kasir mendaftarkan ModuleCode berbeda dengan ModuleName identik

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ganti literal `moduleCode` pada 7 controller langsung ke `"HEALTH_SERVICE_BILLING_MANAGEMENT"`, lalu jalankan `AccessMenuSeeder` biasa | Perubahan paling sederhana, tidak perlu kode seeder baru | **BERBAHAYA.** `AccessMenuSeeder` mengenali baris `SysControllerAccess` existing dari pasangan `(ModuleId, ControllerName)` (`Seeders/AccessMenuSeeder.cs:18-22`). Begitu `ModuleCode` berubah, pasangan itu berubah — seeder akan **membuat baris baru** ber-`Id` baru dan **menutup (soft-delete) baris lama**. `SysAccessPolicy` yang sudah diberikan admin ke role manapun untuk 7 controller ini menunjuk `ControllerAccessId`/`ActionAccessId` lama yang sudah ditutup — hak akses yang sudah diberikan **hilang secara senyap tanpa galat apa pun** (user yang tadinya bisa akses invoice/deposit/dst. tiba-tiba 403). Ini persis risiko yang sudah didokumentasikan dan dialami modul Farmasi |
| B | Tulis seeder migrasi khusus (`BillingModuleCodeConsolidationSeeder`) yang **memindahkan** `ModuleId` pada baris `SysControllerAccess` existing dari 7 kode lama ke kode kanonik — `Id` baris dipertahankan, sehingga `SysActionAccess.ControllerAccessId` dan `SysAccessPolicy.ControllerAccessId`/`ActionAccessId` tetap sah tanpa disentuh — mengikuti pola `PharmacyModuleCodeConsolidationSeeder.cs` yang sudah terbukti aman dan punya test. Seeder ini dijalankan **sebelum** `AccessMenuSeeder` di `Program.cs`, baru setelah itu literal `moduleCode` di 7 controller diganti ke kode kanonik (supaya ke depannya seeder registry biasa tidak perlu memindahkan apa pun lagi — sudah konsisten sejak sumbernya) | Aman — tidak ada `SysAccessPolicy` yang kehilangan acuan; reuse pola yang sudah terbukti bekerja dan sudah ada precedent test-nya; environment yang belum pernah punya data (`SysApplicationModules` masih kosong untuk kode lama) otomatis jadi no-op aman | Kerja lebih banyak dari opsi A — satu file seeder baru + satu baris wiring di `Program.cs` + mengubah 7 literal `moduleCode` controller (bukan cuma 7, tapi semua itu memang perlu diubah di kedua opsi) |

**Solusi terpilih: Opsi B.**

1. Satu-satunya opsi yang tidak berisiko menghapus hak akses yang sudah diberikan admin secara
   senyap — radius dampak keamanan terlalu besar untuk opsi A.
2. Reuse murni pola yang sudah ada dan terbukti bekerja
   (`PharmacyModuleCodeConsolidationSeeder.cs`) — bukan arsitektur baru, hanya diterapkan ulang
   untuk kasus billing-kasir dengan penyesuaian: Farmasi hanya punya 1 kode lama → 1 kode
   kanonik, billing-kasir punya **7 kode lama** → 1 kode kanonik, sehingga logikanya perlu
   digeneralisasi untuk menerima daftar kode lama (bukan ditulis ulang 7 kali secara duplikat).

**Kenapa bukan opsi lain.** Opsi A eksplisit ditolak karena risiko keamanan yang sudah
terdokumentasi dan sudah pernah nyaris terjadi di modul lain pada codebase yang sama.

## 3. Skema tampilan sebelum → sesudah

Tidak ada perubahan tampilan UI secara struktural — perbaikan ini mengubah DATA (jumlah baris
`SysApplicationModule`) yang dikonsumsi layar Manajemen Role existing, bukan komponen atau layout
layarnya.

**SEBELUM** (kartu "Daftar Modul" untuk area Billing, sesuai screenshot pelapor):

```text
Daftar Modul
┌──────────────────────────────────────┐
│ Billing                      0 / 3    │
├──────────────────────────────────────┤
│ Billing                      0 / 2    │
├──────────────────────────────────────┤
│ Billing                      0 / 5    │
├──────────────────────────────────────┤
│ Billing                      0 / 6    │
├──────────────────────────────────────┤
│ Billing                      0 / 11   │
├──────────────────────────────────────┤
│ Billing Master Data          0 / 26   │
├──────────────────────────────────────┤
│ Billing Management Petty Cash 0 / 7   │
└──────────────────────────────────────┘
(dan kemungkinan 1-2 kartu "Billing" lain yang tidak kelihatan di screenshot,
 total 8 kartu "Billing" identik sesuai temuan source)
```

**SESUDAH** (8 kartu "Billing" digabung jadi 1; Master Data dan Petty Cash tetap terpisah):

```text
Daftar Modul
┌──────────────────────────────────────┐
│ Billing                      0 / 97   │
├──────────────────────────────────────┤
│ Billing Master Data          0 / 62   │
├──────────────────────────────────────┤
│ Billing Management Petty Cash 0 / 11  │
└──────────────────────────────────────┘
```

(Angka `97` = jumlah total action pada 13 controller yang digabung: 11+2+44+5+15+4+5+11, dihitung
dari atribut `[AccessAction]` source — angka final di UI bisa sedikit berbeda setelah filter
`VisibleInRoleAccess`/`AllowedForRoleAccess` yang sudah ada di `RoleAccessController.cs:521-531`,
tidak diubah oleh perbaikan ini.)

**Wilayah:** Tidak ada wilayah baru — kartu "Billing" yang tersisa memakai komponen dan field yang
identik dengan kartu lain di layar yang sama, hanya isinya (jumlah controller/action di baliknya)
yang bertambah menjadi gabungan 13 controller.

**Tombol:** Tidak ada perubahan.

**Keadaan:** Tidak ada keadaan baru. Hak akses (`SysAccessPolicy`) yang sudah diberikan admin
untuk salah satu dari 8 controller lama **tetap utuh** setelah penggabungan — hanya tampil di
bawah satu kartu "Billing", bukan tersebar di beberapa kartu seperti sebelumnya.

## 4. Rincian perbaikan

### FIX-BKC-004-01 — Konsolidasikan 8 ModuleCode billing-kasir menjadi satu kode kanonik

| | |
| --- | --- |
| **Menutup** | `ISS-BKC-004-01` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (seeder baru + migrasi data existing) + perubahan literal atribut pada 7 file controller |
| **Berkas yang diubah** | Baru: `Areas/HealthServices/BillingManagement/Seeders/BillingModuleCodeConsolidationSeeder.cs`. Diubah: `Program.cs` (wiring seeder); `BillingConsumerHandoffsController.cs`, `BillingDepositOrdersController.cs`, `BillingInvoicesController.cs`, `BillingPatientFundsController.cs`, `BillingFinalizationsController.cs`, `BillingFinancialExceptionsController.cs`, `BillingSettlementsController.cs`, `InpatientClearanceController.cs`, `CashierShiftsController.cs` (ganti literal `moduleCode` ke kode kanonik; `BillingDepositOrdersController`/`BillingInvoicesController`/`BillingPatientFundsController` sudah berbagi 1 kode yang sama `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING`, jadi hanya 7 nilai `moduleCode` unik yang berubah, bukan 9 file dengan 9 nilai berbeda) |
| **Radius dampak** | Seluruh role yang sudah memiliki hak akses ke salah satu dari 7 controller/kode lama — perbaikan ini WAJIB mempertahankan hak akses tersebut, bukan mereset |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tulis `BillingModuleCodeConsolidationSeeder.cs` mengikuti struktur
   `PharmacyModuleCodeConsolidationSeeder.cs` persis, dengan generalisasi: ganti konstanta
   tunggal `KodeLegacy` menjadi daftar `KodeLegacyList` berisi 7 kode lama (lihat tabel pada
   `ISS-BKC-004-01`, kecualikan `HEALTH_SERVICE_BILLING_MANAGEMENT` karena itu kanonik), dan
   `KodeKanonik = "HEALTH_SERVICE_BILLING_MANAGEMENT"`. Method `ConsolidateAsync` melakukan
   proses yang sama (pindahkan `SysControllerAccess.ModuleId`, pertahankan `Id`, tangani kasus
   penggabungan nama controller yang sudah ada di kanonik) untuk **setiap** kode lama di daftar,
   berurutan, memakai `modulKanonik` yang sama di seluruh iterasi (bukan membuat ulang per
   iterasi). Pertahankan seluruh pengaman yang ada di versi Farmasi: tidak ada baris yang
   dihapus fisik, hanya `IsActive=false`/`IsDelete=true` untuk baris yang sudah dipindahkan
   relasinya; tidak ada `SysAccessPolicy` yang dibuang.
2. Daftarkan di `Program.cs`, **sebelum** `AccessMenuSeeder` dan **setelah**
   `PharmacyModuleCodeConsolidationSeeder` (urutan antar seeder migrasi semacam ini tidak saling
   bergantung, tapi ikuti pola penempatan yang sudah ada — baris 1924-1930), dengan komentar yang
   menjelaskan alasan yang sama seperti komentar Farmasi (`Program.cs:1924-1927`).
3. Ganti literal `moduleCode` pada 9 titik di 7 file controller (lihat tabel "Berkas yang diubah")
   menjadi `"HEALTH_SERVICE_BILLING_MANAGEMENT"`. Jangan mengubah `moduleName` atau `displayName`
   — keduanya tetap per-controller sebagaimana adanya (label tampilan per controller tidak
   terdampak oleh penggabungan modul).
4. Jangan menyentuh 4 controller yang sudah memakai kode kanonik
   (`BillingChargeReconciliationController`, `EncounterBillingSummaryController`,
   `BillingFolioController`, `PatientBillingSummaryController`) maupun `MasterData`/`PettyCash`
   — di luar scope yang dikonfirmasi pelapor.

**Acceptance criteria.**

1. Setelah seeder migrasi dan `AccessMenuSeeder` berjalan pada database dev yang sudah punya data
   lama (8 baris `SysApplicationModule` Billing dari kondisi sebelum perbaikan), hanya tersisa
   **1 baris aktif** `SysApplicationModule` dengan `ModuleCode = HEALTH_SERVICE_BILLING_MANAGEMENT`
   yang menaungi 13 controller; 7 baris modul lama menjadi `IsActive = false`.
2. Seluruh `SysControllerAccess` dan `SysActionAccess` milik 13 controller tersebut tetap
   memiliki `Id` yang **sama** sebelum dan sesudah migrasi (dibuktikan dengan membandingkan daftar
   `Id` sebelum/sesudah pada database uji).
3. `SysAccessPolicy` yang sudah ada sebelum migrasi (buat data uji: beri satu role akses ke salah
   satu dari 7 controller lama sebelum migrasi) **tetap aktif dan menunjuk baris yang valid**
   setelah migrasi — role tersebut tetap bisa mengakses endpoint yang sama, tidak menerima 403.
4. Pada database baru (belum pernah punya data modul Billing sama sekali), seeder migrasi berjalan
   sebagai no-op aman (tidak error), dan `AccessMenuSeeder` sesudahnya membuat langsung 1 baris
   kanonik untuk 13 controller.
5. Layar Manajemen Role menampilkan tepat 1 kartu "Billing" (bukan 8), dengan hitungan total
   action sesuai gabungan 13 controller; "Billing Master Data" dan "Billing Management Petty
   Cash" tetap tampil sebagai kartu terpisah, tidak berubah.
6. Menjalankan seeder migrasi dua kali berturut-turut (idempotency) tidak menghasilkan perubahan
   tambahan pada pemanggilan kedua.

**Verifikasi.** `dotnet build` pada project terdampak (dijalankan manual sesuai instruksi kerja
repository ini, tidak otomatis); test unit pada `BillingModuleCodeConsolidationSeeder` mengikuti
pola `Tests/QuilvianSystemBackend.PharmacyTests/ModuleCodeConsolidationTests.cs` (sesuaikan nama
project/namespace Billing bila berbeda — cek struktur folder `Tests/` terdekat sebelum menulis
project test baru, ikuti pola existing); verifikasi manual pada database dev: jalankan migrasi,
cek layar Manajemen Role menampilkan 1 kartu "Billing", cek role yang sudah punya akses billing
sebelum migrasi tetap bisa mengakses endpoint billing sesudahnya.

**Risiko.** Kesalahan pada logika pemindahan (`GabungkanAsync`/penanganan tabrakan nama
controller) berisiko mencabut hak akses role secara tidak sengaja — ini alasan kenapa pola yang
sudah teruji (Farmasi) harus diikuti persis, bukan ditulis ulang dari nol. Wajib diuji pada
database dev yang sudah punya data `SysAccessPolicy` nyata untuk billing-kasir, bukan hanya
database kosong, sebelum dianggap selesai.

## 5. Urutan pengerjaan

```text
FIX-BKC-004-01   tanpa dependency — boleh dikerjakan sekarang
```

## 6. Keputusan pemilik yang dibutuhkan

Tidak ada yang tersisa. K-01 (scope penggabungan) sudah diputuskan pelapor pada `issue-004`
bagian 7, sebelum plan repair ini ditulis.

## 7. Dokumen hulu yang ikut direvisi

Tidak ada. `FIX-BKC-004-01` berjenis `BUG` pada data registry otorisasi, bukan `DESIGN_CHANGE`
pada tampilan atau kontrak API manapun.

## 8. Verifikasi menyeluruh

Setelah `FIX-BKC-004-01` selesai:

1. Pada database dev dengan data lama (8 baris modul Billing + minimal satu `SysAccessPolicy`
   nyata menunjuk salah satu controller lama), jalankan aplikasi dan amati urutan seeder berjalan
   tanpa galat.
2. Buka layar Manajemen Role, pastikan hanya 1 kartu "Billing" (bukan 8), dengan Master
   Data/Petty Cash tetap terpisah.
3. Login sebagai role yang sebelumnya sudah diberi akses ke salah satu dari 7 controller lama
   (mis. Billing Invoices), pastikan masih bisa mengakses endpoint terkait tanpa 403.
4. Jalankan ulang aplikasi (restart) untuk memastikan seeder migrasi bersifat idempotent — tidak
   ada perubahan/galat pada jalan kedua.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Rencana dibuat dan langsung disetujui (perintah langsung pemilik "Perbaiki FE dan BE", scope K-01 sudah dikonfirmasi sebelum plan repair ditulis). 1 perbaikan, siap didaftarkan ke roadmap | `diagnose-module-issue` atas perintah pemilik |
| 2026-10-16 | `FIX-BKC-004-01` didaftarkan sebagai `BE-BKC-FIX-024` dan dikerjakan lewat `build-module-backend`: seeder `BillingModuleCodeConsolidationSeeder` baru + 9 titik atribut `moduleCode` diubah di 8 controller, mengikuti pola `PharmacyModuleCodeConsolidationSeeder`. Di tengah pengerjaan, pengguna meminta `dotnet build`/`dotnet run` tidak dijalankan otomatis di repository ini — proses build yang sudah terlanjur berjalan dihentikan paksa sebelum sempat mengkompilasi apa pun. Register ditandai `🟡 SEBAGIAN`: source selesai, tapi seluruh verifikasi runtime (build, migrasi data pada database nyata, uji layar) belum dijalankan. Laporan: [BE-BKC-FIX-024.md](../../task/report/backend/BE-BKC-FIX-024.md) | `build-module-backend` atas perintah pemilik |
