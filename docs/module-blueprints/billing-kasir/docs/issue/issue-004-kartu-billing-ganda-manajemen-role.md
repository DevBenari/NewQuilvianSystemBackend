# ISSUE-BKC-004 — Layar Manajemen Role menampilkan banyak kartu modul berlabel "Billing" yang identik

```yaml
issue_id: ISSUE-BKC-004
module_id: billing-kasir
submodule: null  # blueprint berbentuk SINGLE; <blueprint-root> = folder modul
layar: "Administrator > Pengaturan > Manajemen Role - bagian \"Daftar Modul\" (administrator-role-access-view.jsx, endpoint GET /api/v1/administrator/setting/role-access/resources). Layar ini milik blueprint platform-authorization; issue ini fokus pada akar masalah yang ada di controller milik billing-kasir, bukan pada mekanisme layarnya sendiri (lihat bagian 6)"
sumber_laporan: "Laporan pengguna 16-10-2026 disertai 2 screenshot: daftar modul pada layar Manajemen Role menampilkan banyak kartu berlabel sama persis \"Billing\" (0/3, 0/2, 0/5, 0/6, 0/11, dst.) - pengguna meminta semua controller milik modul billing-kasir dikategorikan jadi satu kartu \"Billing\". 1 butir, 2 lampiran"
tanggal_issue: "2026-10-16"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Medium
source_sha_backend: "738e03a0 (Yasmina)"
source_sha_frontend: "Tidak relevan - akar masalah dan perbaikan seluruhnya di backend; frontend merender apa adanya data backend tanpa bug (lihat bagian 4)"
rencana_perbaikan: ../plan-repair/plan-repair-004-kartu-billing-ganda-manajemen-role.md
```

## 1. Ringkasan

Layar Manajemen Role menampilkan "Daftar Modul" sebagai satu kartu per baris `SysApplicationModule`
di database. Delapan controller milik modul Billing (Invoices, Deposit Orders, Finalizations,
Financial Exceptions, Settlements, Consumer Handoffs, Cashier Shift, dan grup
ChargeReconciliation/EncounterBillingSummary/BillingFolio/PatientBillingSummary) mendaftarkan diri
dengan **delapan `ModuleCode` yang berbeda-beda** padahal nama tampilannya (`ModuleName`) ditulis
identik: `"Health Service Billing Management"`. Setelah diformat frontend, kedelapan baris itu
sama-sama menjadi label **"Billing"** — sehingga pengguna melihat banyak kartu yang terlihat sama
persis dan tidak bisa dibedakan. Ini **bukan** bug di layar Manajemen Role maupun di frontend;
keduanya bekerja benar sesuai desain "satu baris modul = satu kartu". Akar masalahnya murni di
pendaftaran atribut `[AccessController]` pada 8 controller billing-kasir. Perbaikannya **tidak
boleh** sekadar mengganti literal kode modul — ada pola berbahaya yang sudah pernah terjadi dan
sudah ada solusi amannya (presedan modul Farmasi), dijelaskan di bagian 4.

Sesuai konfirmasi pengguna (16-10-2026): yang digabung **hanya** 8 controller berlabel identik
"Billing" di atas. Dua kartu lain milik billing-kasir — "Billing Master Data" (7 controller) dan
"Billing Management Petty Cash" (1 controller) — **tetap terpisah**, karena namanya sudah jelas
berbeda (bukan bagian dari keluhan "kartu yang tidak bisa dibedakan"), dan memisahkan Master Data
dari modul utamanya adalah pola yang konsisten dipakai seluruh modul lain di layar ini.

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | "Pada menu manajement role. Mengapa banyak terdapat pilihan access role bernama Billing. Saya ingin semua controller pada modul billing-kasir dikategorikan jadi satu yaitu bernama acess role modul billing saja... Perbaiki FE dan BE." | Screenshot 1 & 2: daftar modul pada layar Manajemen Role, memuat beberapa kartu berlabel "Billing" dengan hitungan berbeda (0/3, 0/2, 0/5, 0/6, 0/11), berdampingan dengan modul lain (Registration, Pharmacy, dll.) |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-BKC-004-01` | 1 | 8 controller billing-kasir mendaftarkan `ModuleCode` berbeda-beda dengan `ModuleName` identik, menghasilkan 8 kartu "Billing" yang tak terbedakan di layar Manajemen Role | `BUG` | Backend | Medium | `SUDAH-VERIFIKASI` di source | `FIX-BKC-004-01` |

## 4. Rincian per temuan

### ISS-BKC-004-01 — 8 controller billing-kasir mendaftarkan ModuleCode berbeda dengan ModuleName identik

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source |
| **Perbaikan** | `FIX-BKC-004-01` |

**Apa yang terjadi.** Saat admin membuka layar Manajemen Role untuk mengatur hak akses, daftar
modul yang seharusnya menampilkan satu baris "Billing" berisi seluruh fitur billing-kasir, justru
terpecah menjadi banyak kartu dengan nama sama persis "Billing" tanpa keterangan pembeda — admin
tidak bisa tahu kartu mana berisi hak akses apa tanpa membuka satu-satu.

**Kenapa terjadi.** Rantai sebab, dari gejala sampai akar:

1. Layar Manajemen Role (`QuilvianSystemFrontendDev/src/components/view/administrator/settings/administrator-role-access-view.jsx:1742` judul "Daftar Modul") memanggil
   `GET /api/v1/administrator/setting/role-access/resources`.
2. Backend `RoleAccessController.BuildModulesAsync`
   (`Areas/Administrator/Setting/Controllers/RoleAccessController.cs:464-564`) mengambil **setiap
   baris** `SysApplicationModules` (aktif, tidak dihapus) sebagai **satu kartu** — tidak ada
   `GroupBy` tambahan atas nama, karena desainnya memang "satu baris database = satu modul".
   Ini **benar**, bukan bug.
3. Frontend `normalizeModule`
   (`administrator-role-access-view.jsx:487-530`) merender **satu kartu per item** yang dikirim
   backend, dan `formatHealthServiceModuleName`
   (`administrator-role-access-view.jsx:285-304`) memformat `ModuleName` lewat strip prefix
   `"Health Services? "` lalu strip suffix `" Management"` — mengubah literal
   `"Health Service Billing Management"` menjadi **"Billing"**. Ini juga **benar**, murni
   transformasi tampilan dari data yang diterima.
4. **Akar masalahnya**: siapa yang menjadi baris `SysApplicationModule` ditentukan
   `AccessMenuSeeder.ReconcileAsync` (`Seeders/AccessMenuSeeder.cs:53-56`) lewat
   `GroupBy(x => x.ModuleCode, StringComparer.Ordinal)` atas seluruh atribut `[AccessController]`
   pada controller di assembly. **8 controller billing-kasir mendaftarkan `moduleCode` yang
   berbeda-beda**, padahal `moduleName` (parameter kedua atribut, murni label tampilan) ditulis
   **identik**: `"Health Service Billing Management"`. Dibuktikan langsung dari source:

   | `ModuleCode` | Controller | Jumlah action |
   | --- | --- | --- |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT` | `BillingChargeReconciliationController.cs:24-29`, `EncounterBillingSummaryController.cs:31-36`, `BillingFolioController.cs:18-23`, `PatientBillingSummaryController.cs:32-37` | 11 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_CONSUMER_HANDOFF` | `BillingConsumerHandoffsController.cs:15` | 2 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING` | `BillingDepositOrdersController.cs:16`, `BillingInvoicesController.cs:15`, `BillingPatientFundsController.cs:15` | 44 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINALIZATION` | `BillingFinalizationsController.cs:15` | 5 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION` | `BillingFinancialExceptionsController.cs:15` | 15 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_SETTLEMENT` | `BillingSettlementsController.cs:15` | 4 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT` | `InpatientClearanceController.cs:29-32` (label tampilan "Billing Inpatient Integration") | 5 |
   | `HEALTH_SERVICE_BILLING_MANAGEMENT_CASHIER_SHIFT` | `CashierShiftsController.cs:15` | 11 |

   Delapan `ModuleCode` berbeda → delapan baris `SysApplicationModule` berbeda → delapan kartu,
   semuanya berlabel "Billing" setelah transformasi tampilan. Angka hitungan "x/y" yang disebut
   pelapor (0/11, 0/2, 0/5, 0/6) cocok persis dengan kombinasi jumlah action di atas.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan desain yang menyatakan modul
Billing sengaja dipecah jadi 8 kartu bernama sama — ini murni inkonsistensi penulisan atribut
antar controller yang ditambahkan pada waktu berbeda-beda, dibanding `ModuleCode` tunggal yang
dipakai 7 controller `MasterData` (`HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA`, konsisten)
dan 10 controller `RegistrationManagement` (`HEALTH_SERVICE_REGISTRATION_MANAGEMENT`, konsisten;
dicek sekilas sebagai pembanding, tidak ditemukan masalah serupa di sana).

**Dampak nyata.** Admin yang mengatur hak akses role untuk fitur Billing harus membuka dan
memeriksa 8 kartu terpisah satu-satu untuk tahu action apa saja yang sebenarnya tersedia, berisiko
melewatkan satu kartu saat memberi atau mencabut akses, karena tidak ada cara membedakan kartu
"Billing" yang satu dari yang lain hanya dari namanya.

**Rekomendasi.** Konsolidasikan 8 `ModuleCode` menjadi satu kode kanonik
`HEALTH_SERVICE_BILLING_MANAGEMENT` (sudah dipakai 4 controller, jadi tetap dipertahankan sebagai
kanonik, bukan membuat kode baru). **Perbaikannya TIDAK BOLEH** sekadar mengganti literal
`moduleCode` di 7 controller sisanya lalu redeploy — ada precedent bahaya yang sudah terjadi dan
sudah ada solusinya untuk kasus yang identik. Rincian lengkap di plan repair.

## 5. Tanya-jawab pelapor

> **T:** Mengapa banyak terdapat pilihan access role bernama Billing?
>
> **J:** Karena 8 controller berbeda di modul Billing didaftarkan dengan 8 kode modul internal
> yang berbeda-beda, padahal nama yang ditampilkan ke layar sengaja/tidak sengaja ditulis sama
> persis "Health Service Billing Management" oleh pengembang yang menambahkan tiap controller
> pada waktu berbeda. Layar Manajemen Role menampilkan satu kartu per kode modul — jadi 8 kode
> berbeda menghasilkan 8 kartu, yang semuanya memakai nama tampilan yang sama. Lihat
> `ISS-BKC-004-01`.

> **T:** Saya ingin semua controller pada modul billing-kasir dikategorikan jadi satu yaitu
> bernama access role modul billing saja. Perbaiki FE dan BE.
>
> **J:** Dikonfirmasi scope-nya (16-10-2026): 8 controller berlabel identik "Billing" digabung
> jadi satu kartu. "Billing Master Data" dan "Billing Management Petty Cash" **tetap terpisah**
> karena namanya sudah berbeda dan jelas (bukan bagian dari masalah "tak bisa dibedakan"), serta
> supaya konsisten dengan modul lain di layar ini yang semuanya memisahkan Master Data dari modul
> utamanya. Soal **FE**: setelah ditelusuri, frontend tidak punya bug apa pun di sini — ia hanya
> merender satu kartu per baris data yang dikirim backend, sesuai desain. Begitu backend
> dikonsolidasikan jadi 1 baris, frontend otomatis menampilkan 1 kartu tanpa perlu perubahan kode
> apa pun di frontend. Jadi perbaikan ini **backend-only** — lihat `FIX-BKC-004-01` di plan
> repair untuk detail kenapa tidak bisa sekadar ganti literal kode modul.

## 6. Temuan tambahan

### ISS-BKC-004-T1 — Dua modul lain di layar yang sama (Master Data, Petty Cash) juga terpisah dari "Billing" utama

| | |
| --- | --- |
| **Jenis** | `DESIGN_CHANGE` (ditolak — bukan perbaikan, dikonfirmasi sebagai perilaku yang diinginkan) |
| **Area** | Backend |
| **Keparahan** | — |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; keputusan dikonfirmasi pelapor |
| **Kenapa dicantumkan** | Memastikan scope `FIX-BKC-004-01` tidak melebar tanpa sengaja ke 8 controller `MasterData` dan 1 controller `PettyCash` |

Kartu "Billing Master Data" (`HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA`, 7 controller, 62
action) dan "Billing Management Petty Cash" (`HEALTH_SERVICE_BILLING_MANAGEMENT_PETTY_CASH`, 1
controller, 11 action) sempat dipertimbangkan sebagai kandidat penggabungan karena sama-sama
"milik modul billing-kasir". Dikonfirmasi pelapor (bagian 5) bahwa keduanya **tidak** termasuk
scope — tetap sebagai kartu tersendiri. Dicatat di sini supaya keputusan ini terdokumentasi dan
tidak ditafsirkan ulang oleh pembaca berikutnya sebagai celah yang belum diputuskan.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada — scope sudah dikonfirmasi (bagian 5), tidak ada pertanyaan
tersisa.

**Untuk pemilik (modul billing-kasir, sekaligus pelapor dalam sesi ini):**

| No | Keputusan | Pilihan | Keputusan diambil | Status |
| ---: | --- | --- | --- | --- |
| K-01 | Scope penggabungan kartu "Billing" | Hanya 8 controller berlabel identik / Semua 21 controller termasuk Master Data & Petty Cash | **Hanya 8 controller berlabel identik** | **Diputuskan 16-10-2026** — `FIX-BKC-004-01` tidak tertahan |

## 8. Catatan pola

Pola ini (banyak controller dalam satu modul logis mendaftarkan `ModuleCode` berbeda-beda dengan
`ModuleName` yang sama) sudah pernah terjadi persis untuk modul Farmasi — lihat
`Areas/HealthServices/PharmacyManagement/Seeders/PharmacyModuleCodeConsolidationSeeder.cs`
beserta dokumentasi risikonya. Disarankan audit serupa untuk modul lain yang mendaftarkan banyak
controller dari waktu ke waktu (pola yang sama berisiko berulang setiap kali controller baru
ditambahkan tanpa memeriksa `ModuleCode` controller sejenis yang sudah ada).

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Issue dibuat dari laporan 1 butir disertai 2 screenshot; scope dikonfirmasi langsung oleh pelapor (K-01); `status` langsung `DALAM_PERBAIKAN` karena perbaikan diberi wewenang langsung ("Perbaiki FE dan BE") | `diagnose-module-issue` |
| 2026-10-16 | `FIX-BKC-004-01` dikerjakan sebagai `BE-BKC-FIX-024`: source selesai (seeder migrasi baru + 9 titik atribut controller), tapi `dotnet build` dan seluruh verifikasi runtime belum dijalankan (pengguna meminta build backend tidak dijalankan otomatis). `status` tetap `DALAM_PERBAIKAN` — belum `SELESAI` karena perbaikan baru 🟡 sebagian. Laporan: [BE-BKC-FIX-024.md](../../task/report/backend/BE-BKC-FIX-024.md) | `build-module-backend` atas perintah pemilik |
