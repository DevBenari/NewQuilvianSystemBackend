# FE-BKC-026 — Anggaran Kas Kecil (`FE-PC-05`) dan butir menu "Anggaran Kas Kecil"

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-026` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, layar baru (route + view + hook + dua modal + perluasan satu Redux slice) |
| Task mode | `FRONTEND` (backend read-only — seluruh kontrak sudah dikunci dan diverifikasi `BE-BKC-036`/`038`, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-036` — `✅`, `dotnet test` (19/19) terverifikasi 8 September 2026 |
| Status task | Source selesai untuk seluruh Scope task ini. **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — verifikasi akan dilakukan manual oleh pengguna sendiri. Belum di-commit |

## Ringkasan untuk pembaca umum

`FE-BKC-023`–`025` menyelesaikan sisi voucher (`FE-PC-01`–`04`). Task ini adalah sisi anggarannya:
layar tersendiri (bukan bagian layar voucher) tempat Finance mengelola kolam kas kecil itu sendiri.
Butir menu "Anggaran Kas Kecil" baru pada sidebar, sejajar "Petty Cash". Layarnya menampilkan:

1. **Tiga kartu saldo** — Saldo Saat Ini, Sudah Dijanjikan, Sisa yang Bebas — ketiganya angka
   mentah dari `GET /petty-cash/budget/current`, sama sekali tidak dihitung ulang di layar
   (`PC-DES-005`, sama seperti kartu serupa pada `FE-BKC-023`).
2. **"+ Tambah Anggaran"** — modal Nominal + Alasan, memanggil `POST /top-ups`.
3. **"Koreksi Saldo"** — modal Arah (Tambah/Kurangi) + Nominal + Alasan, memanggil
   `POST /adjustments`. Pesan galat saat koreksi melanggar komitmen voucher yang sudah disetujui
   (`BIL-VAL-054`) ditampilkan **apa adanya** dari server — layar tidak menduga-duga sisa bebas
   sendiri sebelum mengirim.
4. **Riwayat Pergerakan** — tabel `GET /movements` (Penambahan/Pencairan Voucher/Koreksi) dengan
   saringan jenis dan periode, menampilkan saldo sebelum→sesudah per baris, voucher terkait (bila
   ada), alasan, dan pelaku.

Keempat aksi tulis (top-up, adjustment) memakai `Idempotency-Key` yang dibentuk sekali saat modal
dibuka — pola identik seluruh aksi Petty Cash lain.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-054`,`056`,`057`,`059`. Kontrak `GET /petty-cash/budget/current`,
`GET /petty-cash/budget/movements`, `POST /petty-cash/budget/top-ups`,
`POST /petty-cash/budget/adjustments` (`BIL-API-0.9`, terkunci). `frontend-roadmap.md` § `FE-BKC-026`.

**Keputusan desain, dibuat pada task ini sendiri (`DEV_DISCRETION`), didokumentasikan di sini:**

1. **`MovementType` didaftarkan statis di frontend (`PETTY_CASH_BUDGET_MOVEMENT_TYPE_OPTIONS`),
   bukan dari endpoint metadata.** Berbeda dari `FE-BKC-023` yang memakai
   `GET .../vouchers/filters/metadata` (endpoint metadata khusus voucher yang memang ada),
   `PettyCashBudgetController` **tidak** punya endpoint `filters/metadata` sama sekali — hanya 4
   endpoint (`current`, `movements`, `top-ups`, `adjustments`), dikonfirmasi langsung ke source
   controller backend, bukan diasumsikan. Tiga nilai `MovementType`
   (`PettyCashBudgetMovementTypes` backend: `TOP_UP`/`DISBURSEMENT`/`ADJUSTMENT`) adalah enum
   tertutup yang hanya berubah lewat perubahan kode backend — pola identik
   `CASHIER_SHIFT_STATUS_OPTIONS` yang juga didaftarkan statis karena alasan yang sama.
2. **Dua modal terpisah (`top-up-budget-modal.jsx`, `adjust-budget-modal.jsx`), bukan satu modal
   dengan mode Tambah/Koreksi.** Kedua request punya bentuk field yang nyaris sama
   (`Amount`+`Reason`+`ExpectedRowVersion`), tetapi `Adjustment` punya field tambahan wajib
   (`Direction`) dan konsekuensi bisnis yang berbeda total (bisa ditolak `422` karena melanggar
   komitmen; `TopUp` tidak pernah). **Opsi 1 (direkomendasikan):** dua berkas modal terpisah,
   pola identik `create-voucher-modal.jsx`/`attach-proof-modal.jsx` (`FE-BKC-024`). **Opsi 2:**
   satu modal dengan prop `mode="topup"|"adjust"` yang menampilkan field `Direction` secara
   kondisional — ditolak karena akan mencampur dua alur validasi berbeda dalam satu komponen,
   menambah percabangan yang tidak perlu untuk penghematan satu berkas.
3. **Field `Direction` memakai `BaseNativeSelectField` (dropdown native), bukan
   `BaseSelectField`/`FilterSelect`.** Hanya dua opsi tetap (`INCREASE`/`DECREASE`) — sesuai
   dokumentasi komponennya sendiri ("daftar pendek dan tetap... tidak ada kotak pencarian... untuk
   daftar panjang tetap pakai `BaseSelectField`"). Memakai `BaseSelectField` untuk dua opsi akan
   menambah kompleksitas (popover, pencarian) yang tidak dibutuhkan.
4. **Ikon menu "Anggaran Kas Kecil" (`RiSafeLine`) berbeda dari "Petty Cash" (`RiWallet3Line`,
   `FE-BKC-023`)** — dikonfirmasi tersedia pada versi `react-icons` terpasang sebelum dipakai,
   sama seperti `FE-BKC-023`.

## Proses bisnis

Finance menambah anggaran kas kecil (top-up) dan mengoreksi saldo (adjustment, naik atau turun)
beserta alasannya — keduanya tercatat sebagai baris `BilPettyCashBudgetMovement` append-only,
tidak pernah dihapus atau ditimpa. Koreksi turun yang akan membuat saldo tidak cukup untuk voucher
yang sudah disetujui (komitmen "Sudah Dijanjikan") ditolak **oleh server** — pesannya menyebut
nominal yang sudah dijanjikan secara eksplisit (`BIL-VAL-054`), layar hanya meneruskan pesan itu
lewat toast galat, tidak menghitung ulang sendiri berapa sisa bebas sebelum mengirim.

## Base Component Decision Gate

`UI GATE: 2 elemen COMPOSE (berkas modal baru, seluruh isinya REUSE), 0 elemen NEW, 0 EXTEND`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Kerangka halaman (`Hero`, `region-page-shell`) | `REUSE` | Pola identik `petty-cash-vouchers-view.jsx` (`FE-BKC-023`) |
| Tiga kartu saldo | `REUSE` | `SummaryGrid` — pola identik kartu saldo `FE-BKC-023` |
| Saringan Jenis/Periode/Jumlah baris | `REUSE` | `DataFilter` + `FilterSelect` + `FilterDatePicker` × 2 — pola identik `cashier-shift-history.jsx`/`petty-cash-vouchers-view.jsx`. Tanpa kotak pencarian (`DataFilter` menyembunyikannya otomatis saat `onSearchChange` tidak diberikan) karena `PettyCashBudgetMovementQuery` backend tidak punya field `search` |
| Tabel riwayat pergerakan | `REUSE` | `DataTable` — pola identik tabel voucher `FE-BKC-023`/riwayat perintah `FE-BKC-025` |
| Field Nominal (Top Up & Koreksi) | `REUSE` | `BaseTextField` + `normalizeWholeRupiahInput`/`formatThousandsInput` — pola identik `openingCash`/nominal voucher (`FE-BKC-024`) |
| Field Alasan (Top Up & Koreksi) | `REUSE` | `BaseTextAreaField` — pola identik Tujuan voucher (`FE-BKC-024`) |
| Field Arah Koreksi | `REUSE` | `BaseNativeSelectField` — § Keputusan butir 3 |
| Tombol aksi, loading state | `REUSE` | `BaseButton` |
| Kerangka modal (`Modal`/`Form`) | `REUSE` | `react-bootstrap` — pola identik seluruh modal Petty Cash lain |
| Alert error, toast | `REUSE` | `InformationAlert`, `ToastStack`, `AccessDeniedGate` |
| Butir menu sidebar | `REUSE` | Format objek identik entri lain pada `menu-items.jsx`; ikon `RiSafeLine` dikonfirmasi tersedia — § Keputusan butir 4 |
| **`TopUpBudgetModal` (berkas baru)** | `COMPOSE` | Merangkai field `REUSE` di atas — § Keputusan butir 2 |
| **`AdjustBudgetModal` (berkas baru)** | `COMPOSE` | Merangkai field `REUSE` di atas, termasuk `Direction` — § Keputusan butir 2 dan 3 |

Tidak ada komponen baru dari nol, tidak ada `EXTEND` yang mengubah perilaku default komponen mana
pun. Kedua "elemen COMPOSE" adalah berkas JS baru yang murni merangkai base component yang sudah
ada — konsisten dengan pola `create-voucher-modal.jsx`/`attach-proof-modal.jsx` pada `FE-BKC-024`.

## Endpoint yang dikonsumsi

| Endpoint | Method | Dipakai untuk | Sejak task backend |
| --- | --- | --- | --- |
| `.../petty-cash/budget/current` | `GET` | Tiga kartu saldo (sudah dikonsumsi `FE-BKC-023`, dipakai ulang di sini) | `BE-BKC-036` |
| `.../petty-cash/budget/movements` | `GET` | Tabel riwayat pergerakan | `BE-BKC-036` |
| `.../petty-cash/budget/top-ups` | `POST` | "+ Tambah Anggaran" | `BE-BKC-036` |
| `.../petty-cash/budget/adjustments` | `POST` | "Koreksi Saldo" | `BE-BKC-036` |

Tidak ada delta kontrak — seluruh endpoint dan bentuk request/response-nya dipakai apa adanya.
`MovementType` sebagai enum statis frontend adalah keputusan presentasi (§ Keputusan butir 1),
bukan delta kontrak (tidak ada endpoint yang seharusnya ada tapi tidak dipakai).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-budget-slice.jsx` | **Diubah** (milik `FE-BKC-023`, hanya punya `getCurrentPettyCashBudget` sebelumnya). Tiga thunk baru: `getPettyCashBudgetMovements`, `topUpPettyCashBudget`, `adjustPettyCashBudget`; state `movements`/`actionLoading`/`actionError` + reducer `clearPettyCashBudgetActionError`; enam selector baru. `topUpPettyCashBudget`/`adjustPettyCashBudget` yang berhasil langsung memperbarui `state.current` dari response (response `PettyCashBudgetResponse` yang sama dengan `GET /current`) — kartu saldo ter-refresh tanpa fetch ulang terpisah |
| `src/lib/hooks/.../petty-cash/petty-cash-budget-constants.js` | **Baru.** `PETTY_CASH_BUDGET_MOVEMENT_TYPE_OPTIONS`/`_LABELS`, `DEFAULT_PETTY_CASH_BUDGET_MOVEMENT_FILTERS`, `PETTY_CASH_BUDGET_PAGE_SIZE_OPTIONS`, `PETTY_CASH_BUDGET_ADJUSTMENT_DIRECTION_OPTIONS` |
| `src/lib/hooks/.../petty-cash/use-petty-cash-budget.js` | **Baru.** Hook utama — filter+paginasi riwayat, dispatch saldo+riwayat, state+handler modal Top Up dan Adjustment (`idempotencyKey` dibentuk sekali per `open*`, pola identik `use-petty-cash-vouchers.js`) |
| `src/components/view/.../petty-cash/top-up-budget-modal.jsx` | **Baru.** Modal Tambah Anggaran |
| `src/components/view/.../petty-cash/adjust-budget-modal.jsx` | **Baru.** Modal Koreksi Saldo |
| `src/components/view/.../petty-cash/petty-cash-budget-view.jsx` | **Baru.** View utama — Hero, `SummaryGrid`, dua tombol aksi, `DataFilter`+`DataTable` riwayat, kedua modal |
| `src/app/health-services/billing-management/petty-cash/budget/page.jsx` | **Baru.** Route wrapper, pola identik `petty-cash/vouchers/page.jsx` |
| `src/app/health-services/billing-management/petty-cash/budget/petty-cash-budget-client.jsx` | **Baru.** Client wrapper |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu import ikon (`RiSafeLine`) + satu entri menu "Anggaran Kas Kecil" di bawah "Petty Cash" |

Total: **6 berkas baru, 2 berkas diubah**. Tidak ada berkas milik `FE-BKC-022`/`023`/`024`/`025`
lain yang tersentuh selain perluasan `petty-cash-budget-slice.jsx` yang memang tercatat di atas.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint .` | **SKIPPED** — atas permintaan eksplisit pengguna ("tanpa test... karena saya akan lakukan secara manual") | Tidak dijalankan sesi ini |
| `npm run test:unit` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| `npm run build` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| Ketiga kartu saldo tidak dihitung ulang di layar | **LULUS (tinjauan kode)** | `summaryItems` dibentuk langsung dari `budget` (state `getCurrentPettyCashBudget`/hasil `topUpPettyCashBudget`/`adjustPettyCashBudget`), tidak membaca/menjumlahkan `items` (daftar riwayat) sama sekali |
| Pesan galat koreksi (`BIL-VAL-054`) diteruskan apa adanya, tidak divalidasi ulang di frontend | **LULUS (tinjauan kode)** | `confirmAdjust` hanya memvalidasi struktural (`direction`/`amount > 0`/`reason` terisi); tidak ada perbandingan terhadap `reservedAmount`/`availableAmount` sebelum mengirim; `error?.message` dari respons `422` diteruskan langsung ke toast |
| `Idempotency-Key` dibentuk sekali per modal dibuka | **LULUS (tinjauan kode)** | `openTopUp`/`openAdjust` masing-masing memanggil `generateUuid()` sekali; `confirmTopUp`/`confirmAdjust` memakai `topUpAction.idempotencyKey`/`adjustAction.idempotencyKey` apa adanya |
| Butir menu terjangkau dari sidebar | **LULUS (tinjauan kode)** | Entri terdaftar pada `menu-items.jsx` dengan format identik entri lain |
| `MovementType` yang ditampilkan sesuai enum backend (tidak ada nilai keempat yang tidak ada di backend) | **LULUS (tinjauan kode)** | `PETTY_CASH_BUDGET_MOVEMENT_TYPE_OPTIONS`/`_LABELS` persis tiga nilai `PettyCashBudgetMovementTypes` (dikonfirmasi ke source backend) |
| Verifikasi manual (browser, tambah anggaran, koreksi naik/turun termasuk yang ditolak `422`, saringan jenis+periode) | **NOT FEASIBLE (sesi ini) / akan dilakukan pengguna** | Tidak ada kredensial login pada sesi ini; sesuai permintaan eksplisit, pengguna akan menjalankan verifikasi manual sendiri |

**Task ini belum bisa ditandai selesai.** Tidak ada lint/test/build yang dijalankan sesi ini (atas
permintaan eksplisit pengguna) dan tidak ada verifikasi manual — seluruh bukti di atas murni
tinjauan kode/statis.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — akan dilakukan pengguna sendiri (permintaan eksplisit)
- AUTOMATED TEST: SKIPPED (atas permintaan eksplisit pengguna) — lint/test:unit/build tidak dijalankan

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat tooling maupun browser pada sesi ini** — sama seperti
   `FE-BKC-024`/`025`, murni tinjauan kode statis.
2. **`petty-cash-budget-slice.jsx` kini dipakai dua task (`FE-BKC-023` dan `FE-BKC-026`).**
   Perubahan pada slice ini (thunk/state baru) tidak menyentuh export yang sudah dipakai
   `FE-BKC-023` (`getCurrentPettyCashBudget`/`selectCurrentPettyCashBudget`/
   `selectPettyCashBudgetLoading` dipertahankan persis), tetapi risiko regresi silang antara kedua
   task tetap ada sampai keduanya diverifikasi lint/build bersamaan.
3. **`expectedRowVersion` pada Top Up/Koreksi diambil dari `budget` di Redux saat tombol Simpan
   ditekan** (bukan disegarkan ulang saat modal dibuka) — bila saldo berubah (mis. voucher lain
   dicairkan) tepat di antara modal dibuka dan disimpan, `409` akan muncul dan pengguna diminta
   mengulang lewat toast (`refresh()` otomatis dipanggil) — perilaku yang disengaja (fail-safe via
   concurrency guard backend), bukan celah.
4. **Aktivitas paralel `FE-BKC-022` tetap tidak tersentuh** (lihat laporan `FE-BKC-023` § Risiko
   butir 5) — tidak berubah pada task ini.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint`/`test:unit`/`build` dan verifikasi manual (tambah anggaran,
   koreksi naik, koreksi turun yang berhasil, koreksi turun yang ditolak `422` dengan pesan
   menyebut nominal dijanjikan, saringan jenis+periode, retry idempotency) sendiri.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-026`) dan
   `requirement-traceability.md` menautkan laporan ini.
3. Lanjutkan ke `FE-BKC-027` (Kategori Petty Cash, `FE-PC-06`–`08`) — mengikuti
   `master-data-feature-standard.md` apa adanya, tidak saling bergantung dengan task ini.
