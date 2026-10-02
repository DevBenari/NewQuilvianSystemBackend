# FE-BKC-038 — Kembalikan Sisa Uang dan Batalkan Pencairan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-038` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001` revisi `1.2`); layar `FE-PC-10`, `FE-PC-11` (baru), `FE-PC-04` diperbarui |
| Task type | Frontend, dua aksi baru pada baris voucher yang sudah tercairkan + pembaruan detail voucher |
| Task mode | `FRONTEND` (backend read-only) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-036` (kosakata status dan aksi baris dari server — source selesai, belum diverifikasi, lihat laporannya) dan `BE-BKC-057` ✅ — dikonfirmasi ADA di source: `POST .../returns`, `POST .../reversals` lengkap di `PettyCashVouchersController`/`Service`, KEDUANYA menerima header Idempotency-Key |
| Status task | **Source selesai.** Seluruh berkas pada tabel § Berkas dibuat/diubah. **Sesuai instruksi eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan** — hanya `node --check` pada berkas logika non-JSX (lihat § Validasi). Verifikasi manual di browser **tidak dilakukan**. Belum di-commit |

## Ringkasan untuk pembaca umum

Baris voucher yang uangnya sudah dicairkan (status "Menunggu Bukti" atau "Selesai") sekarang punya
dua tombol baru: "Kembalikan Sisa" untuk mencatat uang yang dikembalikan penerima (nominalnya
dibatasi jelas oleh sisa yang masih di tangan penerima, ditampilkan sebelum mengirim), dan
"Batalkan Pencairan" untuk membalikkan pencairan yang seharusnya tidak terjadi (mengembalikan
seluruh sisa ke anggaran sekaligus, hanya boleh sekali per voucher). Detail voucher juga
diperbarui: menampilkan nominal sebelum pengembalian, total yang sudah dikembalikan, dan sisa yang
masih di tangan penerima saat ini.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-098`, `FR-BKC-099`, `FR-BKC-100`, `FR-BKC-102`; `PC-DES-020`. `frontend-roadmap.md`
§ `FE-BKC-038` (amendment 15 September 2026). Acceptance `UAT-61`, `UAT-62`.

**Temuan yang wajib dilaporkan:**

1. **`PettyCashVoucherCommandResponse` TIDAK mengekspos nominal per kejadian Return** — dikonfirmasi
   dari DTO: hanya `CommandType`, `StatusBefore`/`StatusAfter`, `ActorName`, `Reason`, `OccurredAt`.
   Model `BilPettyCashVoucherCommand` memang punya kolom `Amount`, tapi nilainya SELALU nominal
   voucher penuh (`voucher.Amount`) pada setiap command apa pun — bukan nominal spesifik aksi
   Return itu. Karena itu "riwayat pengembalian" pada detail voucher (§ Scope task ini) diartikan
   sebagai KAPAN/SIAPA/KENAPA (lewat baris "Sisa Dikembalikan" pada tabel Riwayat Perintah yang
   sudah ada), BUKAN ledger bernominal per kejadian — itu keterbatasan kontrak backend, bukan
   sesuatu yang diakali dengan angka rekaan di frontend. "Saldo sebelum/sesudah" diimplementasikan
   sebagai snapshot kumulatif: Nominal Awal (`Amount`) vs Sisa Saat Ini (`OutstandingAmount`),
   bukan per-transaksi.
2. **`PETTY_CASH_BUDGET_MOVEMENT_TYPE_LABELS`/`_OPTIONS` diperbarui dengan 4 jenis pergerakan yang
   sebelumnya tidak terdaftar**: `RETURN`, `REVERSAL` (baru dari task ini), DAN `CARRY_FORWARD_OUT`/
   `CARRY_FORWARD_IN` (seharusnya ditambahkan saat `FE-BKC-037` tapi terlewat — dikonfirmasi dari
   `PettyCashBudgetMovementTypes`, backend). Tanpa ini, panel "Riwayat Pergerakan Anggaran" akan
   menampilkan kode mentah ("RETURN", "CARRY_FORWARD_OUT", dst.) alih-alih label berbahasa
   Indonesia begitu voucher dikembalikan/dibalik atau periode ditutup. Diperbaiki di sini karena
   berkas konstanta yang sama sudah disentuh ulang untuk `RETURN`/`REVERSAL`.
3. **RETURN dan REVERSE SENGAJA tidak masuk `PETTY_CASH_VOUCHER_ACTION_LABELS`** (peta khusus
   jalur `ConfirmModal` generik) — keduanya butuh form berbeda (RETURN: nominal + alasan; REVERSE:
   alasan saja dengan info nominal yang akan kembali), sehingga dirender lewat dua modal khusus
   baru (`return-voucher-modal.jsx`, `reverse-voucher-modal.jsx`), pola yang sama dengan
   `ATTACH_PROOF` yang sudah lebih dulu ditangani terpisah dari `ConfirmModal`.
4. **Validasi nominal Return di frontend murni struktural (nominal > 0, tidak melebihi
   `outstandingAmount` yang SUDAH ditampilkan ke pengguna)** — batas sebenarnya (`BIL-VAL-098`)
   tetap ditegakkan server; pesan penolakan server ditampilkan apa adanya bila entah bagaimana
   lolos ke server (mis. race condition sisa berubah di antara render dan submit).
5. **Kunci idempotensi DAN tombol nonaktif selama pengiriman diterapkan pada KEDUANYA** (bukan
   salah satu, sesuai risiko terkunci pada roadmap) — `idempotencyKey` dibentuk sekali saat modal
   dibuka (pola sama dengan seluruh aksi Petty Cash lain), dan `actionLoading` (state bersama
   seluruh aksi voucher) menonaktifkan tombol baris maupun tombol submit modal.

## Berkas yang diperiksa

- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (amendment 15 September 2026, § `FE-BKC-038`)
- `docs/module-blueprints/billing-kasir/03-frontend-architecture.md` (skema fitur `FE-PC-09`, tabel "Aksi per baris pada Monitoring Permintaan")
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (bentuk `PettyCashVoucherReturnRequest`/`PettyCashVoucherReversalRequest`, perubahan `PettyCashVoucherResponse`)
- `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` (dikonfirmasi `returns`/`reversals` ADA dengan header Idempotency-Key)
- `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` (`ReturnAsync`, `ReverseAsync`, `IsEligibleForReturnOrReversal`, `AvailableActions`, `ChangeVoucherAsync` gerbang REVERSED terminal)
- `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs` (`PettyCashVoucherReturnRequest`, `PettyCashVoucherReversalRequest`, `PettyCashVoucherResponse`, `PettyCashVoucherCommandResponse`)
- `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucherCommand.cs`, `BilPettyCashBudgetMovement.cs` (kode `RETURN`/`REVERSAL`/`CARRY_FORWARD_OUT`/`CARRY_FORWARD_IN`)
- Berkas frontend hasil `FE-BKC-035`/`036` (`petty-cash-vouchers-view.jsx`, `use-petty-cash-vouchers.js`, `petty-cash-voucher-slice.jsx`, `petty-cash-voucher-constants.js`, `voucher-detail-modal.jsx`) sebelum diubah task ini

## Berkas yang diubah/dibuat (frontend, `QuilvianSystemFrontendDev`)

| Berkas | Perlakuan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx` | **Aditif** — thunk `returnPettyCashVoucher`, `reversePettyCashVoucher` (dengan Idempotency-Key), ditambahkan ke tiga `addMatcher` actionLoading; thunk lain tidak disentuh |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | **Aditif** — `PETTY_CASH_VOUCHER_RETURN_ACTION`/`_REVERSE_ACTION`; `PETTY_CASH_VOUCHER_COMMAND_TYPE_LABELS.RETURN`/`.REVERSAL` |
| `src/lib/hooks/.../petty-cash/petty-cash-budget-constants.js` | **Aditif** — label `RETURN`/`REVERSAL`/`CARRY_FORWARD_OUT`/`CARRY_FORWARD_IN` pada movement type labels/options (temuan #2) |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | **Aditif** — state + handler `openReturn`/`confirmReturn`/`openReverse`/`confirmReverse` lengkap (mengikuti pola `openProof`/`confirmProof`) |
| `src/components/view/.../petty-cash/return-voucher-modal.jsx` | **Baru** — `FE-PC-10`, form nominal (dibatasi outstandingAmount yang ditampilkan) + alasan |
| `src/components/view/.../petty-cash/reverse-voucher-modal.jsx` | **Baru** — `FE-PC-11`, info nominal yang akan kembali (baca-saja) + alasan |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | Kolom Aksi menambah `canReturn`/`canReverse` dari `availableActions`; render kedua modal baru |
| `src/components/view/.../petty-cash/voucher-detail-modal.jsx` | `FE-PC-04` diperbarui — blok saldo sebelum/sesudah (saat `returnedAmount > 0`), info pembalikan (`reversedByName`/`reversedAt`/`reversalReason`), catatan keterbatasan riwayat pengembalian (temuan #1) |

**Tidak disentuh** (di luar scope task ini): `create-voucher-modal.jsx`, `attach-proof-modal.jsx`,
`cancel`/`disburse` flow, seluruh berkas `petty-cash-budget-*` selain penambahan label movement
type (temuan #2, konsekuensi langsung dari berkas yang sama).

## Pemetaan kontrak API

| Endpoint | Request | Response tambahan | Status backend (dikonfirmasi dari source) |
| --- | --- | --- | --- |
| `POST /petty-cash/vouchers/{id}/returns` | `amount`, `reason`, `expectedRowVersion` + header `Idempotency-Key` | `returnedAmount`, `outstandingAmount` bertambah/berkurang; status TIDAK berubah | Ada |
| `POST /petty-cash/vouchers/{id}/reversals` | `reason`, `expectedRowVersion` (tanpa `amount`) + header `Idempotency-Key` | `status` → `REVERSED`; `reversedAt`/`reversedByName`/`reversalReason` terisi | Ada |

## Validasi

| Command/aktivitas | Hasil |
| --- | --- |
| `node --check` pada berkas logika non-JSX (`use-petty-cash-vouchers.js`, `petty-cash-voucher-constants.js`, `petty-cash-budget-constants.js`, `petty-cash-voucher-slice.jsx` via salinan `.mjs`) | **PASS** — tidak ada error sintaks |
| Tinjauan manual berkas `.jsx` baru/berubah (tidak bisa `node --check` karena JSX) | Dibaca ulang penuh; prop antar komponen (`target`, `form`, `fieldErrors`, `handleChange`, `handleSubmit`) konsisten dengan pola modal existing |
| `npm run lint:errors` | **Tidak dijalankan** — instruksi eksplisit pengguna |
| `npm run test:unit` | **Tidak dijalankan** — instruksi yang sama |
| `npm run build` | **Tidak dijalankan** — instruksi yang sama |
| Verifikasi manual browser (pengembalian bertahap dua kali; pengembalian melebihi sisa ditolak server; pembalikan kedua ditolak; baris `Dibatalkan (Uang Dikembalikan)` hanya Detail) | **MANUAL TEST: NOT FEASIBLE** — dev server tidak dijalankan sesi ini mengikuti instruksi yang sama |

**Konsekuensi eksplisit:** task ini berstatus **source selesai, belum diverifikasi**. DoD
`FE-BKC-038` yang menuntut "`npm run build` lulus" dan verifikasi manual browser **belum
terpenuhi**. Jangan menandai task ini "Selesai" pada roadmap sampai kedua bukti itu ada.

## Status Git

Belum di-stage, belum di-commit. `git status --short` pada repository frontend (gabungan dengan
sisa perubahan `FE-BKC-035`/`036`/`037` yang juga belum di-commit): 15 berkas berubah, 2 dihapus,
11 baru — 2 berkas baru dan 5 dari 15 berkas berubah (`petty-cash-voucher-slice.jsx`,
`petty-cash-voucher-constants.js`, `petty-cash-budget-constants.js`, `use-petty-cash-vouchers.js`,
`petty-cash-vouchers-view.jsx`, `voucher-detail-modal.jsx`) adalah hasil task ini; sisanya hasil
`FE-BKC-035`/`036`/`037` (lihat laporan masing-masing).

## Risiko dan pemilik

- **Belum ada bukti build/lint/manual test** — risiko terbesar saat ini, sama seperti tiga task
  sebelumnya. Owner: pengguna.
- **"Riwayat pengembalian" tanpa nominal per kejadian** (temuan #1) — risiko UX: Finance yang
  melihat detail voucher dengan banyak pengembalian bertahap tidak bisa tahu berapa nominal
  masing-masing kejadian dari riwayat, hanya total kumulatif. Owner: Backend/API Owner, untuk
  diputuskan apakah `PettyCashVoucherCommandResponse` perlu field nominal per command ke depannya.
- **Label movement type yang terlewat sejak `FE-BKC-037`** (temuan #2) — sudah diperbaiki di task
  ini tanpa menunggu revisi terpisah, karena berkas yang sama sedang disentuh untuk kebutuhan
  task ini sendiri.

## Langkah berikutnya

1. Pengguna menjalankan `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan
   memverifikasi manual di browser: pengembalian bertahap dua kali pada satu voucher, percobaan
   pengembalian melebihi sisa, percobaan pembalikan kedua pada voucher yang sudah `REVERSED`, dan
   tampilan panel "Riwayat Pergerakan Anggaran" setelah kedua aksi ini (memastikan label baru
   terbaca, bukan kode mentah).
2. Setelah bukti di atas ada, perbarui laporan ini (§ Validasi) dan tandai `FE-BKC-038` selesai
   pada `roadmap/frontend-roadmap.md` beserta bukti pada `requirement-traceability.md`.
3. Dengan ini, seluruh empat task revisi Petty Cash (`FE-BKC-035`–`038`) sudah source-complete.
   Tidak ada task frontend baru yang tertunda pada rumpun Petty Cash di `frontend-roadmap.md`
   per pemeriksaan sesi ini — validasi keempatnya sekaligus direkomendasikan sebelum modul ini
   dianggap siap dipakai.
