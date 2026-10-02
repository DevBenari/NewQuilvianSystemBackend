# FE-BKC-035 — Halaman Kanonik Petty Cash dan Pengalihan Route Lama

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-035` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001` revisi `1.2`); layar `FE-PC-09` (baru), `FE-PC-01`/`FE-PC-05` menjadi panel |
| Task type | Frontend, penggabungan dua halaman existing (`petty-cash/vouchers`, `petty-cash/budget`) menjadi satu halaman kerja `petty-cash`, plus satu panggilan ringkasan baru (`GET /overview`) |
| Task mode | `FRONTEND` (backend read-only) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-058` ✅ — dikonfirmasi ADA di source (`PettyCashBudgetController.GetOverview`, `PettyCashBudgetService.GetOverviewAsync`), bukan hanya status roadmap |
| Status task | **Source selesai.** Seluruh berkas pada tabel § Berkas di bawah dibuat/diubah. **Sesuai instruksi eksplisit pengguna pada sesi ini, `npm run lint`/`test:unit`/`build` TIDAK dijalankan** — hanya `node --check` pada 4 berkas logika non-JSX (lihat § Validasi). Verifikasi manual di browser **tidak dilakukan** (dev server tidak dijalankan, mengikuti instruksi yang sama). Belum di-commit |

## Ringkasan untuk pembaca umum

Layar "Petty Cash" dan "Anggaran Kas Kecil" yang sebelumnya dua halaman terpisah kini digabung
menjadi satu halaman kerja di `/health-services/billing-management/petty-cash`. Kedua route lama
tetap hidup sebagai pengalihan permanen (bukan dihapus) supaya tautan yang sudah dibagikan petugas
sejak awal September tidak mati. Halaman baru menampilkan lima kartu ringkasan (Anggaran Periode,
Saldo Saat Ini, Pemakaian Periode, Sisa Anggaran, Menunggu Bukti) dari **satu** panggilan API baru
(`GET /overview`) yang belum pernah dipakai frontend sebelumnya, diikuti panel Monitoring
Permintaan dan panel Anggaran/Riwayat Pergerakan Anggaran — keduanya memakai ulang komponen, hook,
dan slice yang sudah ada, bukan ditulis ulang.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-103`–`106`, `FR-BKC-108`; `PC-DES-025`. `frontend-roadmap.md` § `FE-BKC-035`
(amendment 15 September 2026). Acceptance `UAT-63`, `UAT-64`.

**Temuan yang wajib dilaporkan:**

1. **Dokumentasi kontrak (`contracts/api-contract.md` baris 790–797) menandai `GET /overview`,
   `GET /periods`, dan ketiga endpoint `POST /periods*` sebagai "Rencana (belum tersedia)"** —
   ini **KELIRU/basi**. Dikonfirmasi langsung dari source: `PettyCashBudgetController.cs` baris
   37–125 sudah punya `GetOverview`, `GetPeriods`, `CreatePeriod`, `ActivatePeriod`, `ClosePeriod`
   lengkap dengan authorization attribute, dan `PettyCashBudgetService.GetOverviewAsync`/
   `GetPeriodsAsync` sudah terisi penuh (bukan stub). Sesuai `AGENTS.md` § "source backend saat ini
   adalah bukti otoritatif atas perilaku runtime" — source dipakai sebagai kebenaran, dokumen
   kontrak yang belum diperbarui **dilaporkan sebagai temuan**, bukan diikuti apa adanya. Task ini
   hanya memakai `GET /overview` (kontrak task); `GET /periods` dan ketiga `POST /periods*` adalah
   milik `FE-BKC-037` — tidak disentuh di sini.
2. **`PettyCashBudgetResponse` MASIH mengandung field `reservedAmount`/`availableAmount`** di DTO
   backend (`PettyCashBudgetDtos.cs` baris 105–108), walau `api-contract.md` amendment 15 September
   menyatakan keduanya "DIHAPUS". Komentar pada DTO menjelaskan field ini sengaja dipertahankan
   sementara karena alasan historis lain di service (bukan bug task ini) dan nilainya tidak lagi
   otoritatif. Konsekuensinya: kartu ringkasan lama yang menampilkan field ini
   (`petty-cash-vouchers-view.jsx` dan `petty-cash-budget-view.jsx`, keduanya memakai label
   "Sedang Dijanjikan"/"Tersedia") **dihapus** dari kedua panel pada task ini — bukan permintaan
   eksplisit `FE-BKC-035`, tetapi konsekuensi langsung dari kontrak terkunci "layar yang
   menampilkan 'tersedia' MUST memakai `currentBalance`" dan dari mandat task ini sendiri atas
   "sumber data tiap wilayah" pada `FE-PC-09` (kartu itu tidak berpadanan dengan wilayah resmi mana
   pun di wireframe baru). Info jumlah voucher per status (`voucherSummaryItems` pada panel
   Monitoring) **tidak disentuh** — itu tetap memakai kosakata lama (`Menunggu Persetujuan`,
   `Disetujui`, dst.) yang menjadi tanggung jawab `FE-BKC-036`.
3. **Tombol "Kelola Periode" pada kepala halaman sengaja dinonaktifkan** (dengan `title` tooltip
   penjelas). Wilayah "Kepala Halaman" pada `03-frontend-architecture.md` mewajibkan tombol ini
   ada, tetapi layar tujuannya (`FE-PC-12`, `FE-BKC-037`) **belum dibangun** — dependency graph
   roadmap sendiri menyatakan `FE-BKC-037` baru boleh mulai setelah `FE-BKC-035` ada. Menonaktifkan
   tombol (bukan menyembunyikannya, dan bukan mengarahkannya ke route yang belum ada) dipilih
   sebagai `DEV_DISCRETION` atas bentuk wadah — didokumentasikan di sini supaya `FE-BKC-037`
   tinggal menghapus atribut `disabled` dan mengisi `onClick`-nya.
4. **Tombol aksi tingkat halaman ("+ Buat Permintaan", "Tambah Saldo") TIDAK diduplikasi di kepala
   halaman.** Wireframe `FE-PC-09` menggambarkannya sebagai baris tersendiri antara kartu ringkasan
   dan panel Monitoring. Kedua aksi ini sepenuhnya dikelola state lokal masing-masing hook
   (`usePettyCashVouchers().openCreate`, `usePettyCashBudget().openTopUp`) — mengangkatnya ke induk
   halaman berarti me-render hook yang sama dua kali (sekali di halaman, sekali di panel) dengan
   dua salinan state independen yang bisa saling menyimpang. Tombolnya **tetap ada dan berfungsi
   penuh**, hanya saja di lokasi aslinya (di dalam DataFilter masing-masing panel) — bukan
   dipindah ke kepala halaman. Didokumentasikan sebagai `DEV_DISCRETION` atas bentuk wadah, bukan
   pengurangan scope.
5. **"Data basi" (DoD baris "Setiap aksi yang berhasil MUST memuat ulang GET /budget/overview")
   ditangani lewat prop opsional baru `onMutated`** pada `usePettyCashVouchers`/`usePettyCashBudget`
   dan pada kedua view-nya (`PettyCashVouchersView`/`PettyCashBudgetView`) — dipanggil setelah
   setiap create/action/proof/top-up/adjust berhasil (termasuk pada jalur konflik `409`). Halaman
   gabungan mengoper `refresh` dari `usePettyCashOverview()` sebagai `onMutated`, sehingga kelima
   kartu ringkasan selalu dimuat ulang setelah mutasi apa pun pada kedua panel. Ini perubahan
   **aditif** pada tanda tangan kedua hook (parameter opsional, default no-op) — pemanggilan lama
   tanpa argumen tetap identik perilakunya secara fungsional.
6. **Route lama dialihkan lewat `redirect()` App Router (bukan `next.config.js` `redirects()`)** —
   tidak ada preseden salah satu pola di repository ini; dipilih yang paling lokal ke dua folder
   route yang terdampak. `/vouchers` mengarah ke `#monitoring-permintaan`, `/budget` mengarah ke
   `#riwayat-pergerakan-anggaran`, keduanya anchor `id` pada wadah panel — memenuhi syarat "panel
   Anggaran dalam keadaan terbuka" karena kedua panel **selalu terbuka** (bentuk wadah yang dipilih:
   section bertumpuk, bukan accordion/tab yang butuh state buka/tutup terpisah).

## Berkas yang diperiksa

- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (amendment 15 September 2026)
- `docs/module-blueprints/billing-kasir/03-frontend-architecture.md` (wilayah `FE-PC-09`, § Berkas)
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (amendment 15 September 2026,
  baris 723–831 — lihat temuan #1 dan #2)
- `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs`
- `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs`
  (`GetOverviewAsync`, `LoadActiveBudgetAsync` — konfirmasi 404 saat belum ada periode aktif)
- `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs`
  (`PettyCashOverviewResponse`, `PettyCashBudgetResponse`)
- Seluruh berkas frontend existing di `src/**/petty-cash/**` (view, hook, slice, route) sebelum
  diubah

## Berkas yang diubah/dibuat (frontend, `QuilvianSystemFrontendDev`)

| Berkas | Perlakuan |
| --- | --- |
| `src/app/health-services/billing-management/petty-cash/page.jsx` | **Baru** — halaman kanonik |
| `src/app/health-services/billing-management/petty-cash/petty-cash-client.jsx` | **Baru** |
| `src/app/health-services/billing-management/petty-cash/vouchers/page.jsx` | Diubah — jadi `redirect()` ke `#monitoring-permintaan` |
| `src/app/health-services/billing-management/petty-cash/vouchers/petty-cash-vouchers-client.jsx` | **Dihapus** — tidak lagi dipakai (tidak ada referensi lain, dikonfirmasi lewat pencarian) |
| `src/app/health-services/billing-management/petty-cash/budget/page.jsx` | Diubah — jadi `redirect()` ke `#riwayat-pergerakan-anggaran` |
| `src/app/health-services/billing-management/petty-cash/budget/petty-cash-budget-client.jsx` | **Dihapus** — tidak lagi dipakai |
| `src/components/view/.../petty-cash/petty-cash-page-view.jsx` | **Baru** — `FE-PC-09`, sumber `GET /overview` untuk 5 kartu, merangkai kedua panel |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | Diubah — `Hero` dan `SummaryGrid` anggaran (deprecated) dihapus, jadi panel; terima prop opsional `onMutated`; `id="monitoring-permintaan"` |
| `src/components/view/.../petty-cash/petty-cash-budget-view.jsx` | Diubah — `Hero` dan `SummaryGrid` anggaran (deprecated) dihapus, jadi panel; terima prop opsional `onMutated`; `id="riwayat-pergerakan-anggaran"` |
| `src/lib/hooks/.../petty-cash/use-petty-cash-overview.js` | **Baru** — dispatch `getPettyCashOverview`, expose `overview`/`loading`/`errorMessage`/`notFound`/`refresh` |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | Diubah — tambah parameter opsional `options.onMutated`, dipanggil setelah create/action/proof sukses (aditif, tidak mengubah perilaku lama) |
| `src/lib/hooks/.../petty-cash/use-petty-cash-budget.js` | Diubah — tambah parameter opsional `options.onMutated`, dipanggil setelah top-up/adjust sukses (aditif) |
| `src/lib/state/slice/.../petty-cash-budget-slice.jsx` | Diubah — tambah thunk `getPettyCashOverview` + state (`overview`, `overviewLoading`, `overviewError`, `overviewNotFound`) + selector; thunk/state lama tidak disentuh |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — butir "Anggaran Kas Kecil" dihapus; `pathname` butir "Petty Cash" jadi `.../petty-cash` (tanpa `/vouchers`); import ikon `RiSafeLine` yang jadi tidak terpakai ikut dihapus |

**Tidak disentuh** (di luar scope task ini, milik `FE-BKC-036`/`FE-BKC-037`/`FE-BKC-038`):
`voucher-detail-modal.jsx`, `create-voucher-modal.jsx`, `attach-proof-modal.jsx`,
`top-up-budget-modal.jsx`, `adjust-budget-modal.jsx`, `petty-cash-voucher-slice.jsx`,
`petty-cash-voucher-constants.js`, seluruh logika `approve`/`reject`/status vocabulary.

## Pemetaan kontrak API

| Endpoint | Dipakai oleh | Status backend (dikonfirmasi dari source) |
| --- | --- | --- |
| `GET /petty-cash/budget/overview` | `use-petty-cash-overview.js` (baru) | Ada — `PettyCashBudgetController.GetOverview` |
| `GET /petty-cash/vouchers` | `use-petty-cash-vouchers.js` (existing, dipakai ulang) | Ada (tidak diubah task ini) |
| `GET /petty-cash/budget/movements` | `use-petty-cash-budget.js` (existing, dipakai ulang) | Ada (tidak diubah task ini) |

## Validasi

| Command/aktivitas | Hasil |
| --- | --- |
| `node --check` pada 4 berkas logika non-JSX (`use-petty-cash-overview.js`, `use-petty-cash-vouchers.js`, `use-petty-cash-budget.js`, `petty-cash-budget-slice.jsx` — via salinan sementara berekstensi `.mjs` karena `node --check` tidak mengenali `.jsx`) | **PASS** — tidak ada error sintaks |
| `npm run lint:errors` | **Tidak dijalankan** — instruksi eksplisit pengguna pada sesi ini ("jangan lakukan build fe secara automatis") |
| `npm run test:unit` | **Tidak dijalankan** — instruksi yang sama |
| `npm run build` | **Tidak dijalankan** — instruksi yang sama |
| Verifikasi manual browser (ketiga wilayah, kedua redirect, kartu gagal-memuat) | **MANUAL TEST: NOT FEASIBLE** — dev server tidak dijalankan pada sesi ini mengikuti instruksi yang sama; belum ada bukti visual |

**Konsekuensi eksplisit:** task ini berstatus **source selesai, belum diverifikasi**. DoD
`FE-BKC-035` yang menuntut "`npm run build` lulus" dan "verifikasi manual di browser atas ketiga
wilayah" **belum terpenuhi** — keduanya menunggu pengguna menjalankannya sendiri. Jangan menandai
task ini "Selesai" pada roadmap sampai kedua bukti itu ada.

## Status Git

Belum di-stage, belum di-commit (sesuai wewenang task — tidak ada instruksi eksplisit untuk
commit/push). `git status --short` pada repository frontend menunjukkan 10 berkas berubah, 2
dihapus, 4 baru — seluruhnya tercantum pada tabel § Berkas di atas.

## Risiko dan pemilik

- **Dokumentasi kontrak basi** (temuan #1) berisiko menyesatkan task lain yang membaca
  `api-contract.md` tanpa memverifikasi ke source — direkomendasikan `plan-module-delivery`/pemilik
  kontrak memperbarui baris status endpoint tersebut. Owner: API/Billing Owner.
- **Tombol "Kelola Periode" nonaktif** adalah gap UX sementara sampai `FE-BKC-037` selesai — sesuai
  dependency graph roadmap sendiri, ini diharapkan, bukan regresi. Owner: Frontend, ditutup oleh
  `FE-BKC-037`.
- **Belum ada bukti build/lint/manual test** — risiko terbesar saat ini. Sebelum dipakai lebih
  jauh (termasuk sebagai basis `FE-BKC-037`/`FE-BKC-038`), pengguna **MUST** menjalankan
  `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan membuka halaman
  `/health-services/billing-management/petty-cash` di browser secara manual.

## Langkah berikutnya

1. Pengguna menjalankan `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan
   memverifikasi manual di browser (kelima kartu, kedua panel, kedua redirect, keadaan
   kosong/gagal kartu ringkasan).
2. Setelah bukti di atas ada, perbarui laporan ini (§ Validasi) dan tandai `FE-BKC-035` selesai
   pada `roadmap/frontend-roadmap.md` beserta bukti pada `requirement-traceability.md`.
3. Lanjut ke `FE-BKC-036` (kosakata status baru, buang aksi persetujuan) — boleh paralel dengan
   task ini secara desain roadmap, tetapi menyentuh berkas yang sama
   (`petty-cash-vouchers-view.jsx`) sehingga urutan commit harus disepakati dulu.
