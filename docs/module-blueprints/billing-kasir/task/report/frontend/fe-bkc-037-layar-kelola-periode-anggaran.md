# FE-BKC-037 — Layar Kelola Periode Anggaran

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-037` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001` revisi `1.2`); layar `FE-PC-12` (baru) |
| Task type | Frontend, layar baru (daur hidup periode anggaran) menempel pada halaman kanonik `FE-BKC-035` |
| Task mode | `FRONTEND` (backend read-only) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-035` (halaman induk — source selesai, belum diverifikasi, lihat laporannya) dan `BE-BKC-054` ✅ — dikonfirmasi ADA di source: `GetPeriods`/`CreatePeriod`/`ActivatePeriod`/`ClosePeriod` lengkap di `PettyCashBudgetController`/`Service` |
| Status task | **Source selesai.** Seluruh berkas pada tabel § Berkas dibuat/diubah. **Sesuai instruksi eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan** — hanya `node --check` pada berkas logika non-JSX (lihat § Validasi). Verifikasi manual di browser **tidak dilakukan**. Belum di-commit |

## Ringkasan untuk pembaca umum

Tombol "Kelola Periode" pada halaman Petty Cash (sebelumnya dinonaktifkan oleh `FE-BKC-035`)
sekarang membuka modal "Kelola Periode Anggaran" berisi daftar seluruh periode anggaran beserta
statusnya (Draf/Aktif/Ditutup). Dari situ Finance bisa membuat periode baru, mengaktifkannya, dan
menutup periode yang sedang aktif. Menutup periode yang masih bersaldo **mewajibkan** memilih
periode penerus, dan dialognya secara eksplisit menjelaskan bahwa sisa saldo itu **dipindahkan**,
bukan hilang — sesuai kontrak terkunci.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-092`, `FR-BKC-095`, `FR-BKC-096`; `PC-DES-017`, `PC-DES-018`. `frontend-roadmap.md`
§ `FE-BKC-037` (amendment 15 September 2026). Acceptance `UAT-58`, `UAT-59`, `UAT-60`.

**Temuan yang wajib dilaporkan:**

1. **Ketiga endpoint daur hidup periode (`POST /periods`, `POST /periods/{id}/activate`,
   `POST /periods/{id}/close`) TIDAK menerima header `Idempotency-Key` sama sekali** — dikonfirmasi
   langsung dari `PettyCashBudgetController.cs`: hanya `TopUp`/`Adjust` yang punya parameter
   `[FromHeader(Name = "Idempotency-Key")]`, ketiga endpoint periode tidak. Ini beda dari pola
   "setiap POST membawa Idempotency-Key" yang berlaku di seluruh Petty Cash lainnya
   (`03-frontend-architecture.md` § Penanganan keadaan). Frontend **tidak mengirim** header yang
   tidak pernah dibaca server (mengirimnya akan memberi rasa aman palsu). Pengiriman ganda di
   ketiga aksi ini murni dicegah lewat tombol nonaktif selama pengiriman + `expectedRowVersion`
   (optimistic concurrency, sudah ditegakkan backend). **Direkomendasikan sebagai gap backend**
   untuk pemilik modul — bukan sesuatu yang bisa ditutupi dari sisi frontend.
2. **`PettyCashBudgetResponse` tidak punya field `StatusLabel` terpisah** (beda dari voucher yang
   `StatusLabel`-nya wajib diambil dari server, PC-DEC-013) — dikonfirmasi dari DTO. Karena itu
   label "Draf"/"Aktif"/"Ditutup" pada tabel periode dipetakan di frontend
   (`PETTY_CASH_BUDGET_PERIOD_STATUS_LABELS`), sah dilakukan karena backend memang tidak
   menyediakan label periode sama sekali — bukan pelanggaran PC-DEC-013.
3. **Bentuk wadah dipilih: modal, bukan halaman anak/route baru** — `DEV_DISCRETION` eksplisit
   pada roadmap. Alasan: (a) roadmap sendiri mengarahkan "Reuse: pola modal master data yang sudah
   dipakai `adjust-budget-modal.jsx` dan `top-up-budget-modal.jsx`"; (b) tidak menambah route baru
   berarti tidak ada risiko pengalihan/menu tambahan yang perlu didaftarkan terpisah.
4. **"Detail" untuk periode berstatus Ditutup TIDAK memanggil endpoint baru** — tidak ada endpoint
   `GET /budget/periods/{id}` pada backend (dikonfirmasi dari controller), sehingga
   `budget-period-detail-modal.jsx` murni menyajikan ulang data baris yang sudah dimuat
   `GET /budget/periods`, tanpa panggilan tambahan.
5. **Successor dropdown pada dialog Tutup Periode diambil dari pengambilan TERPISAH**
   (`GET /budget/periods?pageSize=100`, tanpa filter status tabel), bukan dari baris yang sedang
   tampil di tabel utama — supaya daftar periode penerus (Draf/Aktif) tetap lengkap walau tabel
   sedang difilter ke status lain. Pengambilan ini SEMENTARA menimpa state Redux `periods` yang
   sama dipakai tabel; `refresh()` dipanggil otomatis saat dialog ditutup untuk memulihkan
   tampilan tabel ke filter/paginasi semula. Efek sampingnya: tabel di belakang dialog bisa
   sekilas menampilkan data berbeda selama dialog terbuka — trade-off yang disengaja dibanding
   membangun slice/thunk paralel hanya untuk satu dropdown.
6. **`onMutated` dari aksi periode (aktifkan/tutup) MUST NOT hanya memuat ulang kartu ringkasan
   halaman** — keduanya mengubah saldo/riwayat pergerakan periode aktif yang ditampilkan panel
   "Anggaran dan Riwayat Pergerakan" (`FE-BKC-035`), yang punya hook terpisah
   (`usePettyCashBudget`) dan tidak tahu-menahu soal state modal ini. Ditambahkan parameter
   opsional baru `externalReloadSignal` pada `usePettyCashBudget`/`PettyCashBudgetView` (aditif,
   dampak nol pada pemanggil lama) supaya panel itu ikut memuat ulang saat periode
   diaktifkan/ditutup dari modal ini — lihat § Berkas.
7. **Modal `BudgetPeriodModal` (dan hook datanya) baru dipasang setelah tombol "Kelola Periode"
   pernah diklik** (`hasOpenedPeriodsModal`, di `petty-cash-page-view.jsx`) — mencegah
   `GET /budget/periods` terpanggil di setiap pemuatan awal halaman Petty Cash padahal fiturnya
   belum tentu dipakai pengguna pada sesi itu.

## Berkas yang diperiksa

- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (amendment 15 September 2026, § `FE-BKC-037`)
- `docs/module-blueprints/billing-kasir/03-frontend-architecture.md` (skema fitur `FE-PC-12`, tabel wilayah)
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (bentuk `CreatePettyCashBudgetPeriodRequest`/`ClosePettyCashBudgetPeriodRequest`, contoh carry-forward)
- `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` (dikonfirmasi tidak ada header Idempotency-Key pada ketiga endpoint periode)
- `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` (`CreatePeriodAsync`, `ActivatePeriodAsync`, `ClosePeriodAsync` — pesan validasi persis, syarat successor)
- `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs` (`CreatePettyCashBudgetPeriodRequest`, `ActivatePettyCashBudgetPeriodRequest`, `ClosePettyCashBudgetPeriodRequest`, `PettyCashBudgetPeriodQuery`)
- `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudget.cs` (`PettyCashBudgetStatuses`: `DRAFT`/`ACTIVE`/`CLOSED`)
- Berkas frontend hasil `FE-BKC-035` (`petty-cash-page-view.jsx`, `petty-cash-budget-slice.jsx`, `use-petty-cash-budget.js`, `petty-cash-budget-view.jsx`) sebelum diubah task ini

## Berkas yang diubah/dibuat (frontend, `QuilvianSystemFrontendDev`)

| Berkas | Perlakuan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-budget-slice.jsx` | **Aditif** — 4 thunk baru (`getPettyCashBudgetPeriods`, `createPettyCashBudgetPeriod`, `activatePettyCashBudgetPeriod`, `closePettyCashBudgetPeriod`), state `periods*`, reducer `clearPettyCashBudgetPeriodActionError`, selector terkait; thunk/state lama tidak disentuh |
| `src/lib/hooks/.../petty-cash/petty-cash-budget-constants.js` | **Aditif** — `PETTY_CASH_BUDGET_PERIOD_STATUS_LABELS`/`_OPTIONS`/`_BADGE_CONFIG`, `DEFAULT_PETTY_CASH_BUDGET_PERIOD_FILTERS` |
| `src/lib/hooks/.../petty-cash/use-petty-cash-budget-periods.js` | **Baru** — hook daur hidup periode (list, buat, aktifkan, tutup, detail read-only, successor lookup) |
| `src/lib/hooks/.../petty-cash/use-petty-cash-budget.js` | Tambah parameter opsional `options.externalReloadSignal` (temuan #6); pemanggilan lama tanpa opsi ini tidak berubah perilakunya |
| `src/components/view/.../petty-cash/petty-cash-budget-view.jsx` | Terima dan teruskan prop opsional baru `reloadSignal` ke hook |
| `src/components/view/.../petty-cash/budget-period-modal.jsx` | **Baru** — `FE-PC-12`, daftar periode + filter status + wiring ketiga aksi + tiga modal anak |
| `src/components/view/.../petty-cash/create-budget-period-modal.jsx` | **Baru** — form "Buat Periode" |
| `src/components/view/.../petty-cash/close-budget-period-modal.jsx` | **Baru** — dialog "Tutup Periode" dengan sisa saldo + successor + kalimat pemindahan (kontrak terkunci) |
| `src/components/view/.../petty-cash/budget-period-detail-modal.jsx` | **Baru** — detail baca-saja untuk periode Ditutup (temuan #4) |
| `src/components/view/.../petty-cash/petty-cash-page-view.jsx` | Tombol "Kelola Periode" diaktifkan (sebelumnya `disabled` dari `FE-BKC-035`); wiring `BudgetPeriodModal` + `budgetPanelReloadSignal` (temuan #6, #7) |

**Catatan file-naming vs `03-frontend-architecture.md`:** dokumen arsitektur menyebut satu berkas
baru `budget-period-modal.jsx`. Task ini memecahnya menjadi 4 berkas (`budget-period-modal.jsx`
sebagai kontainer daftar + 3 modal anak per konsep) mengikuti pola satu-modal-per-konsep yang
SUDAH ada di modul ini sendiri (`create-voucher-modal.jsx`, `attach-proof-modal.jsx`,
`top-up-budget-modal.jsx`, `adjust-budget-modal.jsx` — semuanya berkas terpisah, bukan satu berkas
gabungan). Dilaporkan di sini sebagai keputusan `DEV_DISCRETION` atas bentuk wadah internal, bukan
penyimpangan dari kontrak yang dikunci (kontrak hanya mengunci ISI dialog tutup dan sumber
datanya, bukan jumlah berkas).

## Pemetaan kontrak API

| Endpoint | Request | Response | Status backend (dikonfirmasi dari source) |
| --- | --- | --- | --- |
| `GET /petty-cash/budget/periods` | `status?`, `pageNumber`, `pageSize` | `PagedResult<PettyCashBudgetResponse>` | Ada |
| `POST /petty-cash/budget/periods` | `periodStart`, `periodEnd?`, `budgetAmount` (poolCode/poolName tidak diekspos di UI) | `PettyCashBudgetResponse` | Ada — **tanpa** header Idempotency-Key (temuan #1) |
| `POST /petty-cash/budget/periods/{id}/activate` | `expectedRowVersion` | `PettyCashBudgetResponse` | Ada — **tanpa** header Idempotency-Key |
| `POST /petty-cash/budget/periods/{id}/close` | `successorBudgetId?`, `reason`, `expectedRowVersion` | `PettyCashBudgetResponse` | Ada — **tanpa** header Idempotency-Key |

## Validasi

| Command/aktivitas | Hasil |
| --- | --- |
| `node --check` pada berkas logika non-JSX (`use-petty-cash-budget-periods.js`, `petty-cash-budget-constants.js`, `use-petty-cash-budget.js`, `petty-cash-budget-slice.jsx` via salinan `.mjs`) | **PASS** — tidak ada error sintaks |
| Tinjauan manual seluruh berkas `.jsx` baru (tidak bisa `node --check` karena JSX) | Dibaca ulang penuh setelah ditulis; tidak ditemukan ketidakcocokan prop/import |
| `npm run lint:errors` | **Tidak dijalankan** — instruksi eksplisit pengguna |
| `npm run test:unit` | **Tidak dijalankan** — instruksi yang sama |
| `npm run build` | **Tidak dijalankan** — instruksi yang sama |
| Verifikasi manual browser (alur buat → aktifkan → tutup; pesan `BIL-VAL-102`/`103`/`104` apa adanya; dialog tutup menampilkan sisa dan tujuan pemindahan) | **MANUAL TEST: NOT FEASIBLE** — dev server tidak dijalankan sesi ini mengikuti instruksi yang sama |

**Konsekuensi eksplisit:** task ini berstatus **source selesai, belum diverifikasi**. DoD
`FE-BKC-037` yang menuntut "`npm run build` lulus" dan verifikasi manual browser **belum
terpenuhi**. Jangan menandai task ini "Selesai" pada roadmap sampai kedua bukti itu ada.

## Status Git

Belum di-stage, belum di-commit. `git status --short` pada repository frontend (gabungan dengan
sisa perubahan `FE-BKC-035`/`036` yang juga belum di-commit): 14 berkas berubah, 2 dihapus, 9 baru
— sembilan berkas baru dan 5 dari 14 berkas berubah (`petty-cash-budget-constants.js`,
`use-petty-cash-budget.js`, `petty-cash-budget-view.jsx`, `petty-cash-budget-slice.jsx`,
`petty-cash-page-view.jsx`) adalah hasil task ini; sisanya hasil `FE-BKC-035`/`036` (lihat
laporan masing-masing).

## Risiko dan pemilik

- **Belum ada bukti build/lint/manual test** — risiko terbesar saat ini, sama seperti dua task
  sebelumnya. Owner: pengguna.
- **Ketiadaan Idempotency-Key pada tiga endpoint periode** (temuan #1) — risiko nyata pada
  koneksi tidak stabil (klik "Buat Periode" lalu retry jaringan bisa membuat dua periode
  tumpang-tindih tanggal, walau `BIL-VAL-102` akan menolak percobaan kedua *bila* tanggalnya
  benar-benar sama; risiko lebih nyata pada `activate`/`close` yang idempoten secara alami lewat
  `expectedRowVersion`). Owner: Backend/API Owner, untuk diputuskan apakah ketiga endpoint ini
  perlu menyusul pola Idempotency-Key seperti aksi Petty Cash lain.
- **Tabel periode sekilas berubah data selama dialog Tutup terbuka** (temuan #5) — kosmetik,
  pulih otomatis begitu dialog ditutup; didokumentasikan supaya tidak disalahartikan sebagai bug
  saat verifikasi manual nanti.

## Langkah berikutnya

1. Pengguna menjalankan `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan
   memverifikasi manual di browser: alur buat → aktifkan → tutup periode (termasuk kasus sisa
   saldo nol dan sisa saldo > 0), pesan penolakan server (coba buat periode tumpang-tindih tanggal,
   coba aktifkan saat sudah ada periode aktif, coba tutup periode dengan voucher belum dicairkan).
2. Setelah bukti di atas ada, perbarui laporan ini (§ Validasi) dan tandai `FE-BKC-037` selesai
   pada `roadmap/frontend-roadmap.md` beserta bukti pada `requirement-traceability.md`.
3. `FE-BKC-038` (kembalikan sisa uang, batalkan pencairan) sudah bisa dimulai — dependency-nya
   (`FE-BKC-036`, `BE-BKC-057` ✅) terpenuhi secara source dan tidak bergantung pada task ini.
