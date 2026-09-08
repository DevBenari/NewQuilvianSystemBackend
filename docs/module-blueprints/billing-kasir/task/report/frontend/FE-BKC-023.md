# FE-BKC-023 — Monitoring Voucher Petty Cash (`FE-PC-01`) dan butir menu "Petty Cash"

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-023` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, layar baru (route + view + hook + tiga Redux slice baru) |
| Task mode | `FRONTEND` (backend read-only — seluruh kontrak sudah dikunci dan diverifikasi `BE-BKC-034`–`038`, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-037` — `✅`, `dotnet build`/`test` (17/17) terverifikasi 8 September 2026. `BE-BKC-036` — `✅`, `dotnet test` (19/19) terverifikasi tanggal yang sama. `BE-BKC-035` (opsi kategori) — `✅`, `dotnet test` (12/12) terverifikasi tanggal yang sama. `BKC-BLK-FE-001` — diverifikasi ulang sesi ini: `QuilvianSystemFrontendDev/AGENTS.md` terbaca penuh (385 baris), governance tersedia — **resolved**, bukan diasumsikan |
| Status task | Source selesai untuk seluruh Scope task ini (monitoring, kartu saldo, saringan, kolom Aksi untuk `APPROVE`/`REJECT`/`CANCEL`/`DISBURSE`, butir menu sidebar). `ATTACH_PROOF` ("Input Nota") dan `POST /` ("+ Buat Voucher") **sengaja tidak diimplementasikan** — scope `FE-BKC-024`, lihat § Scope yang dikecualikan. `npm run lint`/`test:unit`/`build` dijalankan dan **LULUS**. Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Sebelum task ini, tidak ada satu pun layar frontend untuk Petty Cash — backend (`BE-BKC-033`–`038`)
sudah lengkap dan terverifikasi, tetapi Kasir dan Kepala Kasir tidak punya tempat untuk melihat
voucher kas kecil atau menjalankan persetujuan/pencairannya. Task ini adalah **titik masuk**
seluruh rumpun Petty Cash di frontend:

1. Butir menu "Petty Cash" baru pada sidebar, di bawah "Billing dan Kasir" (sejajar "Shift Kasir").
2. Satu layar (`/health-services/billing-management/petty-cash/vouchers`) menampilkan:
   - Kartu saldo kas kecil berjalan (Saldo, Sedang Dijanjikan, Tersedia) — ketiganya angka mentah
     dari `GET /petty-cash/budget/current`, **tidak pernah** dihitung ulang di layar (`PC-DES-005`).
   - Kartu ringkasan jumlah voucher per status dari `GET /petty-cash/vouchers/summary`.
   - Saringan: pencarian, status, kategori, periode (dari–sampai), jumlah baris per halaman.
   - Tabel voucher dengan kolom Aksi yang **diturunkan langsung dari `availableActions`** milik
     setiap baris — bukan disimpulkan dari `status` — karena kelayakan sebuah aksi (mis. hanya
     pemohon sendiri yang boleh membatalkan pengajuannya, `PC-DEC-007`) hanya diketahui backend.
   - Empat aksi transisi status (Setujui, Tolak, Batalkan, "Uang Diberikan") lewat modal konfirmasi,
     masing-masing mengirim `Idempotency-Key` yang **dibentuk sekali** dan dipakai apa adanya pada
     setiap percobaan (termasuk retry jaringan) — terutama penting untuk "Uang Diberikan" karena
     klik ganda tidak boleh mencairkan uang dua kali.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-045`,`047`–`053`; `FE-PC-01`; kontrak `BIL-API-0.9` (revisi terkunci, `DESIGN_APPROVED`
menurut `blueprint-manifest.md`, diverifikasi ulang langsung ke source backend `BE-BKC-037`, bukan
hanya dokumen kontrak). `frontend-roadmap.md` § `FE-BKC-023`.

**Keputusan desain, dibuat pada task ini sendiri (`DEV_DISCRETION` — wadah presentasi), didokumentasikan di sini:**

1. **Dua kartu `SummaryGrid` terpisah untuk saldo (Saldo Kas Kecil / Sedang Dijanjikan / Tersedia),
   bukan satu kartu dengan dua baris teks.** Base component `SummaryGrid` yang sudah ada hanya
   merender satu judul + satu nilai per kartu — tidak ada slot "keterangan" sekunder. Menambah slot
   itu berarti `EXTEND` yang menyentuh 15+ pemakai `SummaryGrid` lain di seluruh aplikasi.
   Menampilkan `reservedAmount`/`availableAmount` sebagai kartu tersendiri (bukan baris kedua dalam
   satu kartu) memenuhi maksud "kartu TOTAL PETTY CASH beserta keterangan reservedAmount" tanpa
   menyentuh base component sama sekali — **REUSE murni**, nol risiko regresi pada modul lain. Lihat
   § Base Component Decision Gate.
2. **"+ Buat Voucher" dan "Input Nota" tidak dirender sama sekali pada task ini**, meski
   `frontend-roadmap.md` menyebut keduanya sebagai "titik masuk" yang ditunggu `FE-BKC-024`. Field
   `Scope` milik `FE-BKC-023` sendiri tidak menyebutkan kedua tombol itu — hanya
   `FE-BKC-024`'s `Dependency` yang menyebutnya sebagai sesuatu yang ditunggu dari task ini.
   Merender tombol yang tidak berfungsi (tanpa modal untuk dituju) berisiko terlihat seperti bug
   bagi Kasir/Kepala Kasir yang menguji layar ini sebelum `FE-BKC-024` selesai. `FE-BKC-024` akan
   menambahkan kedua tombol itu bersamaan dengan modal/form-nya sendiri (`FE-PC-02`/`FE-PC-03`) —
   pilihan ini dicatat eksplisit di sini, bukan kelalaian. Baris voucher yang **hanya** punya
   `ATTACH_PROOF` pada `availableActions` (status `CASH_RECEIVED`/`COMPLETED`) karena itu tampil
   tanpa tombol Aksi pada task ini — lihat § Scope yang dikecualikan.
3. **Kategori options (`GET /master-data/petty-cash-categories/options`) dipakai untuk saringan
   Kategori** walau endpoint ini tidak disebutkan pada field `Kontrak` `FE-BKC-023` (yang hanya
   menyebut endpoint voucher dan budget). Field `Scope` task ini secara eksplisit meminta saringan
   kategori, dan endpoint ini adalah endpoint options baku sembilan-endpoint master data
   (`BE-BKC-035`, sudah dikunci/diverifikasi) — bukan endpoint baru yang diusulkan sepihak.
   Didokumentasikan sebagai delta kontrak, bukan keputusan bisnis sepihak.

## Scope yang dikecualikan — `ATTACH_PROOF` dan `POST /` (create)

`FE-BKC-024` (`FE-PC-02`/`FE-PC-03`) eksplisit memiliki scope: form "+ Buat Voucher" (`POST
/petty-cash/vouchers`) dan form/koreksi "Input Nota" (`POST /petty-cash/vouchers/{id}/proofs`).
Task ini **tidak** mengimplementasikan keduanya (§ Keputusan butir 2). Konsekuensinya:

- Voucher berstatus `WAITING_APPROVAL` menampilkan tombol Setujui/Tolak/Batalkan — **lengkap**,
  karena ketiganya scope task ini.
- Voucher berstatus `APPROVED` menampilkan tombol "Uang Diberikan" — **lengkap**, scope task ini.
- Voucher berstatus `CASH_RECEIVED` atau `COMPLETED` (yang `availableActions`-nya hanya berisi
  `ATTACH_PROOF`) tampil **tanpa** tombol Aksi apa pun pada task ini — kolom menampilkan `-`.
  Ini bukan bug: `PETTY_CASH_VOUCHER_ACTION_LABELS` (konstanta) hanya memetakan empat aksi yang
  diimplementasikan; `buildColumns` memfilter `availableActions` terhadap peta itu sebelum
  merender tombol apa pun (lihat § File yang diubah/ditambah).
- Tidak ada tombol "+ Buat Voucher" pada header layar.

`FE-BKC-024` akan mengisi kedua celah ini di berkas yang sama (`petty-cash-vouchers-view.jsx`,
`use-petty-cash-vouchers.js`) — bukan berkas terpisah — karena keduanya adalah perluasan alami
layar yang sama, bukan layar baru.

## Proses bisnis

Kasir/petugas administrasi memantau seluruh voucher yang pernah diajukan. Kepala Kasir/Finance
Operations menyetujui atau menolak voucher yang menunggu. Kasir menekan "Uang Diberikan" untuk
voucher yang sudah disetujui. Pemohon (siapa pun perannya) dapat membatalkan pengajuannya sendiri
selama masih menunggu keputusan — backend menegakkan kepemilikan ini (`403` bila bukan pemohon,
`PC-DEC-007`), layar hanya menampilkan tombol bila `availableActions` mengizinkannya; layar tidak
pernah menebak siapa pemohonnya sendiri.

Saldo kas kecil berjalan (kartu ringkasan) turun **hanya** saat "Uang Diberikan" ditekan — persis
seperti dijelaskan `BE-BKC-037`: saat disetujui, saldo **tetap**, hanya komitmen ("Sedang
Dijanjikan") yang bertambah. Layar ini tidak mensimulasikan logika itu; ketiga angka pada kartu
saldo dibaca ulang dari server setiap kali daftar dimuat ulang (`refresh()`, dipanggil otomatis
setelah setiap aksi berhasil).

## Base Component Decision Gate

`UI GATE: 0 elemen baru, 0 elemen EXTEND — REUSE penuh`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Kerangka halaman (`Hero`, `region-page-shell`) | `REUSE` | Pola identik `doctor-discount-approvals-view.jsx` |
| Kartu saldo kas kecil | `REUSE` | `SummaryGrid` dipakai dua kali (saldo, ringkasan status voucher) — lihat § Keputusan butir 1 untuk kenapa bukan `EXTEND` |
| Saringan pencarian/status/kategori/periode/jumlah baris | `REUSE` | `DataFilter` + `FilterSelect` × 3 + `FilterDatePicker` × 2 — pola identik `cashier-shift-history.jsx` (status + dua `FilterDatePicker` untuk rentang tanggal) |
| Tabel voucher | `REUSE` | `DataTable` — pola identik `doctor-discount-approvals-view.jsx`/`cashier-shift-history.jsx` |
| Badge status | `REUSE` | `StatusBadge` dengan `label` per baris diambil langsung dari `StatusLabel` backend (bukan dipetakan di frontend) — mencegah drift label yang dilarang `PC-DEC-013`; `config` hanya menentukan warna, pola identik `CASHIER_SHIFT_STATUS_BADGE_CONFIG` |
| Tombol aksi baris (Setujui/Tolak/Batalkan/Uang Diberikan) | `REUSE` | `BaseButton`, pola identik `doctor-discount-approvals-view.jsx` |
| Modal konfirmasi aksi | `REUSE` | Satu instance `ConfirmModal` dipakai bergantian untuk keempat aksi (judul/varian/`requireReason` berubah mengikuti `actionTarget.type`) — `ConfirmModal` sendiri mengelola teks alasan secara internal dan meneruskannya lewat argumen kedua `onConfirm`, pola identik `doctor-discount-approvals-view.jsx` |
| Alert error/akses ditolak | `REUSE` | `AccessDeniedGate`, `InformationAlert` |
| Notifikasi hasil aksi | `REUSE` | `ToastStack` |
| Butir menu sidebar | `REUSE` | Format objek identik seluruh entri lain pada `menu-items.jsx`; ikon `RiWallet3Line` (react-icons/ri, dikonfirmasi tersedia pada versi terpasang) dipakai alih-alih menduplikasi ikon induk "Billing dan Kasir" |

Tidak ada komponen baru, tidak ada `EXTEND` yang mengubah perilaku default komponen mana pun —
seluruh 15+ pemakai `SummaryGrid`/`StatusBadge`/`ConfirmModal` lain di aplikasi **tidak
terpengaruh** sama sekali oleh task ini.

## Endpoint yang dikonsumsi

| Endpoint | Method | Dipakai untuk | Sejak task backend |
| --- | --- | --- | --- |
| `.../petty-cash/vouchers` | `GET` | Daftar voucher (paginasi + saringan) | `BE-BKC-037` |
| `.../petty-cash/vouchers/summary` | `GET` | Kartu ringkasan jumlah per status | `BE-BKC-037` |
| `.../petty-cash/vouchers/filters/metadata` | `GET` | `StatusOptions`/`PageSizeOptions` saringan | `BE-BKC-037` |
| `.../petty-cash/vouchers/{id}/approve` | `POST` | Aksi "Setujui" | `BE-BKC-037` |
| `.../petty-cash/vouchers/{id}/reject` | `POST` | Aksi "Tolak" | `BE-BKC-037` |
| `.../petty-cash/vouchers/{id}/cancel` | `POST` | Aksi "Batalkan" | `BE-BKC-037` |
| `.../petty-cash/vouchers/{id}/disburse` | `POST` | Aksi "Uang Diberikan" | `BE-BKC-037` |
| `.../petty-cash/budget/current` | `GET` | Kartu saldo kas kecil | `BE-BKC-036` |
| `.../master-data/petty-cash-categories/options` | `GET` | Opsi saringan Kategori (delta kontrak, § Keputusan butir 3) | `BE-BKC-035` |

`.../petty-cash/vouchers` (`POST`) dan `.../petty-cash/vouchers/{id}/proofs` (`POST`) **tidak**
dikonsumsi task ini — scope `FE-BKC-024`.

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx` | **Baru.** Thunk `getPettyCashVouchers`, `getPettyCashVoucherSummary`, `getPettyCashVoucherFilterMetadata`, `approvePettyCashVoucher`, `rejectPettyCashVoucher`, `cancelPettyCashVoucher`, `disbursePettyCashVoucher` (keempat aksi mengirim header `Idempotency-Key`); selector; `addMatcher` untuk `actionLoading`/`actionError` bersama (pola sudah dipakai `master-data-patient-*-slice.jsx`) |
| `src/lib/state/slice/.../petty-cash-budget-slice.jsx` | **Baru.** Thunk `getCurrentPettyCashBudget`; selector |
| `src/lib/state/slice/.../master-data-petty-cash-category-slice.jsx` | **Baru.** Thunk `getPettyCashCategoryOptions` (hanya endpoint `options` — bukan CRUD penuh, karena layar ini murni monitoring); selector |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | **Baru.** `DEFAULT_PETTY_CASH_VOUCHER_FILTERS`, `PETTY_CASH_VOUCHER_STATUS_BADGE_CONFIG` (warna saja, label dari server), `PETTY_CASH_VOUCHER_ACTION_LABELS` (empat aksi task ini) |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | **Baru.** Hook utama: filter+paginasi, dispatch list/summary/filterMetadata/budget/categoryOptions, `openAction`/`closeAction`/`confirmAction` dengan `idempotencyKey`/`correlationId` dibentuk sekali per target aksi (pola identik `use-cashier-shift.js`) |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | **Baru.** View utama — Hero, dua `SummaryGrid`, `DataFilter`, `DataTable` (kolom Aksi memfilter `availableActions` terhadap `PETTY_CASH_VOUCHER_ACTION_LABELS`), satu `ConfirmModal` bergantian per jenis aksi |
| `src/app/health-services/billing-management/petty-cash/vouchers/page.jsx` | **Baru.** Route wrapper, pola identik `cashier/shifts/page.jsx` |
| `src/app/health-services/billing-management/petty-cash/vouchers/petty-cash-vouchers-client.jsx` | **Baru.** Client wrapper, pola identik `cashier-shifts-client.jsx` |
| `src/lib/state/store.jsx` | **Diubah.** Tiga reducer baru didaftarkan (`pettyCashVoucher`, `pettyCashBudget`, `masterDataPettyCashCategory`) |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu import ikon (`RiWallet3Line`) + satu entri menu "Petty Cash" di bawah "Shift Kasir" |

Total: **7 berkas baru, 2 berkas diubah**. Tidak ada berkas milik `FE-BKC-022` (paralel, tidak
disentuh — lihat § Risiko) yang tersentuh.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint .` (seluruh repo) | **PASS** | Exit 0, tanpa output |
| `npm run test:unit` | **PASS** | 445/445 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru (task ini murni komposisi base component + Redux thunk standar, tidak ada logika bercabang yang membutuhkan test unit terpisah, sesuai `test-policy.md` — menulis test baru bersifat opsional) |
| `npm run build` | **PASS** | Exit 0; route `/health-services/billing-management/petty-cash/vouchers` terkonfirmasi ada pada `.next/server/app/...` (dicek langsung, bukan hanya membaca log build); `postbuild`/`prepare-standalone.mjs` sukses |
| Kolom Aksi diturunkan dari `availableActions`, bukan `status` | **LULUS (tinjauan kode)** | `buildColumns` membaca `field(row, "availableActions", "AvailableActions")` langsung, memfilter terhadap `PETTY_CASH_VOUCHER_ACTION_LABELS`, tidak pernah membaca `status` untuk menentukan tombol mana yang muncul |
| Kartu saldo tidak dihitung ulang dari daftar voucher yang tampil | **LULUS (tinjauan kode)** | `budgetSummaryItems` dibentuk langsung dari `budget` (state `getCurrentPettyCashBudget`), tidak membaca/menjumlahkan `items` (daftar voucher) sama sekali |
| `Idempotency-Key` sama pada retry jaringan (khusus "Uang Diberikan") | **LULUS (tinjauan kode)** | `openAction` membentuk `idempotencyKey` SEKALI saat modal dibuka (`generateUuid()`); `confirmAction` memakainya apa adanya pada `payload.idempotencyKey`; modal tidak menutup otomatis saat gagal (kecuali `409`), sehingga percobaan ulang dari modal yang sama memakai kunci yang sama persis |
| Butir menu terjangkau dari sidebar | **LULUS (tinjauan kode)** | Entri terdaftar pada `menu-items.jsx` dengan format identik entri lain; tidak ada mekanisme permission per-item pada `menu-items.jsx` di seluruh aplikasi (dikonfirmasi lewat grep) — konsisten dengan pola existing, bukan celah baru task ini |
| Tidak ada UUID mentah dirender di layar | **LULUS (tinjauan kode)** | Kolom tabel merender `voucherNumber`/`recipientName`/`categoryName`/`amount`/`purpose`/`statusLabel` — tidak ada satu pun kolom yang merender `id`/`Id`/`categoryId`/`requestedBy` mentah |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada sesi ini |
| Verifikasi manual ter-autentikasi (memuat daftar voucher nyata, menjalankan keempat aksi, mengonfirmasi kartu saldo berubah sesuai, menekan "Uang Diberikan" dua kali untuk membuktikan idempotency sungguhan) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder pada sesi ini; membutuhkan data voucher nyata pada berbagai status dan akun dengan hak akses `PettyCashVoucher`/`PettyCashBudget` yang sesuai |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test/build lulus bersih dan seluruh
acceptance yang termasuk scope task ini terverifikasi secara statis/tinjauan kode, tetapi ini
**bukan** pengganti verifikasi manual ter-autentikasi dengan data nyata (per instruksi
`build-module-frontend`: "Lint/test/build yang PASS saja bukan bukti").

- MANUAL TEST: NOT FEASIBLE — tidak ada kredensial login/data voucher nyata pada sesi ini
- AUTOMATED TEST: `npm run test:unit` — PASS (445/445)

## Risiko yang tersisa

1. **Belum diverifikasi dengan data dan sesi login nyata.** Seluruh bukti di atas adalah tinjauan
   kode/statis atas hasil build. Sebelum ditandai selesai, layar ini **wajib** dibuka langsung di
   browser dengan akun berhak — termasuk mengonfirmasi label status yang tampil persis sama dengan
   `StatusLabel` backend, dan mengonfirmasi klik ganda "Uang Diberikan" benar-benar hanya mencairkan
   sekali (uji jaringan lambat/retry).
2. **`ATTACH_PROOF`/"+ Buat Voucher" sengaja belum ada (§ Scope yang dikecualikan).** `FE-BKC-024`
   akan mengisi kedua celah ini pada berkas yang sama — bukan kelalaian task ini.
3. **Tidak ada mekanisme permission per-item pada `menu-items.jsx`** (dikonfirmasi berlaku untuk
   seluruh entri lain juga, bukan hanya milik task ini) — visibilitas sidebar diasumsikan diatur
   mekanisme lain di luar berkas ini (mis. filter dari daftar controller yang dapat diakses
   pengguna). Tidak diselidiki lebih lanjut karena di luar scope task ini dan tidak ada bukti ini
   pola yang menyimpang dari existing.
4. **Warna badge status (`PETTY_CASH_VOUCHER_STATUS_BADGE_CONFIG`) adalah judgment call
   presentasi** (`DEV_DISCRETION`) — labelnya sendiri selalu dari server, hanya className/warna
   yang dipilih builder. Bila desain menginginkan skema warna berbeda, ini murni perubahan file
   konstanta, tidak menyentuh logika.
5. **Ditemukan aktivitas paralel tidak terkait** (`FE-BKC-022`/Struk Pasien:
   `menu-pembayaran-view.jsx`, `struk-pasien-document.jsx`, `use-dokumen-kasir-page.js`,
   `billing-invoice-calculation-breakdown.js`, `tests/unit/billing-invoice-calculation-breakdown.test.mjs`
   berstatus termodifikasi/baru di working tree). **Tidak disentuh** oleh task ini — di luar scope
   sepenuhnya.

## Langkah berikutnya yang direkomendasikan

1. Verifikasi manual ter-autentikasi oleh pengguna dengan akun berhak `PettyCashVoucher`/
   `PettyCashBudget` — lihat § Risiko butir 1.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-023`) dan
   `requirement-traceability.md` menjadi `✅`, menautkan laporan ini.
3. Lanjutkan ke `FE-BKC-024` (`FE-PC-02`/`FE-PC-03`) — form "+ Buat Voucher" dan "Input Nota",
   mengisi kedua celah pada § Scope yang dikecualikan, pada berkas yang sama.
