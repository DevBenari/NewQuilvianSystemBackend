# FE-BKC-010 — Accessibility, Privacy, dan Regression Lintas Workspace

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-010` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Audit lintas workspace (bukan vertical slice fitur baru) — mencakup `FE-BKC-001`–`009` |
| Task mode | `FRONTEND` (backend read-only) |
| Write target | `QuilvianSystemFrontendDev` (source; **tidak ada perubahan source pada sesi ini** — lihat Ringkasan); laporan ini ditulis di `NewQuilvianSystemBackend` mengikuti presedens task sebelumnya |
| Branch frontend | `yasmina` |
| Dependency | `FE-BKC-001`–`009` (semua sudah source-complete; `FE-BKC-009` diverifikasi sesi sebelumnya) |
| Status task | **Sebagian terpenuhi**: acceptance test `BIL-AT-024` (privacy/status/label) diverifikasi terpenuhi lewat audit source langsung. Cakupan "critical E2E journeys" dan tooling a11y otomatis **belum bisa dipenuhi** pada sesi ini — lihat Blocker. |

## Ringkasan untuk pembaca umum

Task ini adalah audit, bukan pembangunan fitur baru: memeriksa apakah seluruh workspace Billing/Kasir
yang sudah dibangun (`FE-BKC-001`–`009`) bisa dipakai dengan keyboard, tidak membocorkan data sensitif
ke log/console browser, tidak menandai status hanya dengan warna, dan tetap berfungsi wajar pada
kondisi loading/kosong/error/tidak dikenal. **Tidak ada perubahan source pada sesi ini** — audit
ini murni pembacaan langsung kode lintas 9 slice yang sudah ada, karena satu-satunya defect konkret
yang ditemukan (transisi `isFinal`/`CLOSED`) sudah diperbaiki dan dilaporkan pada sesi `FE-BKC-009`
sebelumnya.

## Metodologi

Karena tidak ada environment ter-autentikasi maupun kredensial pengguna yang tersedia pada sesi ini
(konsisten dengan keterbatasan yang sama pada seluruh laporan `FE-BKC-003`–`009`), audit dilakukan
sebagai **pembacaan source langsung** (bukan tooling otomatis — lihat Blocker #1) terhadap:

- Seluruh `src/components/view/health-services/billing-management/**` (9 slice: invoice/charge,
  master policy, diskon, deposit, settlement, shift kasir, financial exception, finalisasi, master
  data Register+4 policy lain).
- Hook terkait di `src/lib/hooks/health-services/billing-management/**`.
- Base component yang dipakai bersama di seluruh slice: `DataTable`, `StatusBadge`, `BaseButton`,
  `InformationAlert` (satu perbaikan pada base component ini otomatis berlaku ke seluruh 9 slice
  karena reuse, sehingga diperiksa lebih dalam daripada tiap slice satu per satu).

## Temuan per acceptance criteria (`BIL-AT-024` — "Scan logs/UI keyboard/status")

| Kriteria (`testing/acceptance-test-matrix.md`) | Status | Bukti |
| --- | --- | --- |
| Tak ada field sensitif di log | **Terpenuhi** | `grep` untuk `console.(log\|error\|warn\|info\|debug)` di seluruh `components/view/.../billing-management`, `lib/hooks/.../billing-management`, dan `lib/state/slice/.../billing-management` — **nol hasil**. Tidak ada tooling analytics/tracking (`gtag`, `mixpanel`, `Sentry`, dll.) terpasang di aplikasi sama sekali (`grep` di seluruh `src/` — nol hasil relevan). |
| Status tidak hanya warna | **Terpenuhi** | `StatusBadge` (`components/features/base-features/status-badge.jsx:108-121`) selalu merender `region-status-icon` **dan** `region-status-label` (teks) bersama `className` warna — dipakai di seluruh 9 slice (invoice, AR/AP handoff, cashier shift, financial exception, dll.), bukan pola per-slice yang bisa berbeda-beda. |
| Fokus/label valid | **Terpenuhi (sampel representatif)** | Setiap `<input>`/`<textarea>`/`<select>` yang diperiksa di 8 modal lintas 4 slice berbeda (`finalize-invoice-modal`, `handover-shift-modal`, `review-variance-modal`, dll.) memakai pasangan `<label htmlFor="x">` + `id="x"` yang konsisten. Radio button (`add-tender-modal.jsx:57`) dibungkus `<label>` (asosiasi implisit). Tidak ditemukan `<div>`/`<span>` dengan `onClick` tanpa semantik native di seluruh `components/view/.../billing-management` (nol hasil `grep`). Baris tabel yang bisa diklik (`DataTable`, dipakai di seluruh slice) memakai `tabIndex={0}` + `onKeyDown` yang menangani `Enter`/`Space` (`data-table.jsx:253-273`) — bukan hanya `onClick` mouse-only. |

`BIL-AT-024` **terpenuhi berdasarkan bukti source langsung** untuk ketiga sub-kriterianya di atas.

## Temuan tambahan (di luar `BIL-AT-024`, dalam scope "regression lintas workspace")

1. **Tabel responsif** — `tableWrapper` (base CSS `base-data-components.module.css:603-607`) memakai
   `overflow-x: auto`, dipakai oleh `DataTable` di seluruh slice — regression responsive untuk tabel
   sudah tertangani di level base component, bukan per-slice.
2. **Loading/empty/error state konsisten** — `DataTable` menangani `loading` (`aria-busy`, baris
   loading dengan `colSpan` penuh) dan empty state (`emptyTitle`/`emptyDescription`) secara seragam;
   setiap hook billing yang diperiksa (`use-billing-finalization.js`, dll.) mengekspos
   `xxxError`/`xxxLoading` terpisah per resource yang ditangkap panel lewat `InformationAlert` —
   pola yang sama sejak `FE-BKC-001`.
3. **Koreksi atas temuan sesi ini sendiri**: laporan versi pertama task ini (30 Agustus 2026) sempat
   mengklaim `FE-BKC-008` punya regresi — case refund/adjustment/write-off diduga hilang saat
   refresh karena tidak ditemukan `localStorage` di hook-nya. **Klaim itu salah dan sudah dikoreksi**
   setelah pembacaan lebih lengkap: `casesByInvoice` memang berasal dari `useSelector` Redux
   (`selectFinancialExceptionCasesByInvoice`), tapi diisi lewat `getFinancialExceptionsByInvoice`
   (`GET {ENDPOINT}/invoices/{invoiceId}`) yang dipanggil di `useEffect` setiap mount
   (`use-billing-financial-exception.js:130-138`) — bukan lewat `localStorage`. Backend memang
   sebelumnya (saat laporan `FE-BKC-008` ditulis) **tidak punya satu pun endpoint `GET`** (`ISSUE-FE-008`),
   tapi gap itu **sudah diperbaiki di backend** sejak commit `f5e2106` ("update billing-kasir part 3"):
   `BillingFinancialExceptionsController.cs` sekarang punya lima endpoint `GET`
   (`invoices/{invoiceId}`, `invoices/{invoiceId}/refundable-credits`, `refunds/{id}`,
   `adjustments/{id}`, `write-offs/{id}`), dan frontend **sudah dimigrasikan** untuk memakainya
   sebagai source of truth (bukan localStorage) — `openApprove`/`openReverse` per-baris dari daftar
   nyata, dengan `openApproveManual`/`openReverseManual` sebagai fallback relay ID manual yang tetap
   dipertahankan untuk case dari sesi/perangkat lain. **Kesimpulan: tidak ada gap untuk diperbaiki —
   `ISSUE-FE-008` sudah closed, tidak perlu task perbaikan terpisah.**

## Blocker — bagian scope yang belum bisa dipenuhi pada sesi ini

1. **Tidak ada tooling a11y otomatis di repository.** `package.json` tidak memiliki `axe-core`,
   `@axe-core/playwright`, `jest-axe`, atau setara — hanya `@playwright/test` untuk E2E generik.
   `AGENTS.md` frontend baris 86–92 melarang instalasi package baru kecuali task dependency
   eksplisit mengizinkannya; task `FE-BKC-010` yang diterima sesi ini tidak memberi wewenang
   tersebut. **Audit pada laporan ini karena itu bersifat manual/structural (pembacaan source),
   bukan hasil scan tooling otomatis** — sesuai catatan field "Risiko" pada roadmap ("Test tooling
   belum diketahui sampai governance dibaca").
2. **Tidak ada "critical E2E journey" untuk Billing/Kasir.** `tests/e2e/` hanya berisi
   `auth-security.spec.mjs` dan `route-smoke.spec.mjs` (smoke tanpa login) — tidak ada satu pun
   skenario E2E yang menjalankan alur bisnis Billing (charge → settlement → finalisasi, dst).
   Menulis skenario ini membutuhkan environment ter-autentikasi dengan data invoice/shift nyata,
   yang tidak tersedia pada sesi ini — konsisten dengan keterbatasan yang sama pada seluruh laporan
   `FE-BKC-003`–`009` sebelumnya (`MANUAL TEST: NOT FEASIBLE`).
3. Karena Blocker #1 dan #2, `FE-BKC-010` **tidak bisa ditandai selesai sepenuhnya** menurut DoD
   roadmap ("Evidence `BIL-AT-024`, zero critical accessibility/privacy finding" — terpenuhi;
   "critical journeys pass" — belum bisa dibuktikan).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| Audit source manual (a11y/privacy/status/label) lintas 9 slice | **PASS** | Lihat tabel `BIL-AT-024` di atas. |
| `npm run lint:errors` / `test:unit` / `build` | **Tidak dijalankan ulang pada sesi ini** | Tidak ada perubahan source pada sesi ini (audit read-only); hasil validasi terakhir (sesi `FE-BKC-009`, 30 Agustus 2026) tetap berlaku: PASS ketiganya. |
| a11y automated scan (axe/lighthouse) | **NOT FEASIBLE** | Tooling tidak terpasang; instalasi package baru butuh otorisasi task dependency terpisah (lihat Blocker #1). |
| Critical E2E journeys | **NOT FEASIBLE** | Tidak ada spec sama sekali untuk Billing; environment ter-autentikasi tidak tersedia (lihat Blocker #2). |

## Git status

```
(tidak ada perubahan pada QuilvianSystemFrontendDev di sesi ini)
```

## Langkah berikutnya yang direkomendasikan

1. **Keputusan pemilik modul dibutuhkan**: apakah mengotorisasi instalasi `@axe-core/playwright`
   (atau setara) sebagai task dependency terpisah, supaya `FE-BKC-010` bisa punya scan a11y otomatis
   alih-alih hanya audit manual.
2. Sediakan environment ter-autentikasi (≥1 akun per role kunci: Kasir, Kepala Kasir, Billing,
   Finance, Dokter) dan data invoice/shift nyata, supaya baik verifikasi manual yang tertunda di
   `FE-BKC-003`–`009` maupun "critical E2E journeys" `FE-BKC-010` bisa dijalankan sekaligus.
3. ~~Perbaiki gap `FE-BKC-008`~~ — **tidak diperlukan**, lihat koreksi pada bagian "Temuan tambahan"
   di atas: `ISSUE-FE-008` sudah closed sejak backend commit `f5e2106`, frontend sudah memakai
   endpoint `GET` yang sebenarnya, tidak ada regresi persistence.
4. Commit tumpukan delapan task frontend (`FE-BKC-003`–`009` + navigasi menu) yang sudah lulus
   validasi tapi belum ter-commit, supaya progress tidak hilang.
