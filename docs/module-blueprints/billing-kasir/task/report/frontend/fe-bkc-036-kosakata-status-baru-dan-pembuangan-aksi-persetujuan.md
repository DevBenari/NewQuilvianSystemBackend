# FE-BKC-036 — Kosakata Status Baru dan Pembuangan Aksi Persetujuan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-036` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001` revisi `1.2`); layar `FE-PC-01` (panel monitoring), `FE-PC-02`, `FE-PC-04` |
| Task type | Frontend, pembuangan aksi (approve/reject) dan pembaruan kosakata status pada komponen/hook/slice yang sudah ada — bukan fitur baru |
| Task mode | `FRONTEND` (backend read-only) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-055` ✅ — dikonfirmasi ADA di source: endpoint `approve`/`reject` sudah tidak ada di `PettyCashVouchersController`, `disburse` menerima status `REQUESTED`, dan `PettyCashVoucherSummaryResponse` sudah berkosakata baru |
| Status task | **Source selesai.** Seluruh berkas pada tabel § Berkas diubah. **Sesuai instruksi eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan** — hanya `node --check` pada 3 berkas logika non-JSX (lihat § Validasi). Verifikasi manual di browser **tidak dilakukan** (dev server tidak dijalankan, instruksi yang sama). Belum di-commit |

## Ringkasan untuk pembaca umum

Layar Monitoring Petty Cash tidak lagi punya tombol "Setujui"/"Tolak" sama sekali — baik di kolom
Aksi tabel maupun di kode yang memanggil API-nya — karena backend sudah menghapus kedua endpoint
itu (memanggilnya sekarang menghasilkan `404`). Label status dan tombol aksi kini memakai kosakata
baru: `Menunggu Pencairan` menggantikan `Menunggu Persetujuan`, tombol aksinya berubah dari
`Uang Diberikan` menjadi `Cairkan`, dan modal pembuatan permintaan judulnya berubah dari
"Buat Voucher Kas Kecil" menjadi "Buat Permintaan Petty Cash". Kartu ringkasan jumlah voucher per
status juga diperbarui mengikuti bentuk response backend yang baru (`PendingDisbursementCount`
menggantikan dua hitungan lama sekaligus).

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-088`, `FR-BKC-107`; `PC-DES-015`, `PC-DES-021`. `frontend-roadmap.md` §
`FE-BKC-036` (amendment 15 September 2026). Acceptance `UAT-55`, `UAT-56`, `UAT-57`.

**Temuan yang wajib dilaporkan:**

1. **`GET /vouchers/summary` sudah berubah bentuk di backend** (`PettyCashVoucherSummaryResponse`,
   `PettyCashVoucherService.GetSummaryAsync`, dikonfirmasi langsung dari source) — bukan hanya
   label yang berganti, tapi **field-nya sendiri berbeda**: `WaitingApprovalCount`+`ApprovedCount`
   digabung jadi satu `PendingDisbursementCount` (backend sendiri menjumlah kosakata lama dan baru
   sekaligus, supaya baris warisan yang belum sempat dipetakan migration tetap terhitung), field
   `TotalWaitingApprovalAmount` **hilang**, dan `CancelledCount` **baru**. Kontrak eksplisit
   `FE-BKC-036` hanya menyebut `GET /vouchers` (bukan `/summary`), tetapi `/summary` dipanggil oleh
   hook yang SAMA (`usePettyCashVouchers`, sudah dalam wewenang tulis task ini) dan tampil di panel
   yang SAMA (`FE-PC-01`) — membiarkan field lama berarti kartu ringkasan menunggu-persetujuan/
   disetujui akan diam-diam menampilkan `0` selamanya (field itu sudah tidak ada di response),
   bukan error yang kelihatan. Diperbaiki di sini karena tergolong "kosakata status" yang menjadi
   inti scope task ini, bukan penambahan fitur baru.
2. **Label tombol aksi `DISBURSE` berubah dari "Uang Diberikan" menjadi "Cairkan"** — bukan cuma
   dugaan gaya bahasa, tapi mengikuti tabel "Aksi per baris pada Monitoring Permintaan" yang
   dikunci pada `03-frontend-architecture.md` (baris `Menunggu Pencairan`: "Cairkan, Batalkan,
   Detail"). Dialog konfirmasinya (`ACTION_CONFIRM_COPY.DISBURSE`) turut disesuaikan.
3. **Judul tombol trigger + tombol submit pada alur "Buat Permintaan" turut diselaraskan** —
   `+ Buat Voucher` → `+ Buat Permintaan`, `Ajukan Voucher` → `Ajukan Permintaan`. Yang **dikunci**
   eksplisit oleh roadmap hanya judul modal ("judul modal berubah menjadi Buat Permintaan"); kedua
   label tombol ini diselaraskan sebagai `DEV_DISCRETION` supaya tidak ada UI yang bilang "Voucher"
   tepat di sebelah modal yang sudah bilang "Permintaan".
4. **`RETURN` dan `REVERSE` (milik `FE-BKC-038`) SENGAJA tidak ditambahkan** ke
   `PETTY_CASH_VOUCHER_ACTION_LABELS` pada task ini. Filter `actionableTypes` pada
   `petty-cash-vouchers-view.jsx` sudah menyaring berdasarkan map ini, sehingga bila server sudah
   mengirim kedua kode itu di `availableActions` (kemungkinan, karena `BE-BKC-057` sudah selesai),
   baris terkait untuk sementara **hanya menampilkan Batalkan/Cairkan/Detail** sampai
   `FE-BKC-038` menambahkan modalnya — bukan bug, melainkan batas wewenang task ini.
5. **Penanda terpisah `isCancelled`** (baris "Dibatalkan" pada tabel Penanda Status,
   "tetap seperti sebelumnya" menurut `03-frontend-architecture.md`) **tidak ditemukan** di
   source frontend mana pun sebelum maupun sesudah task ini — dikonfirmasi lewat pencarian
   `isCancelled`/`IsCancelled` di seluruh `src/components/view/.../petty-cash/`. Karena dokumen
   sendiri menyatakan perilaku ini "tetap seperti sebelumnya" (bukan requirement baru task ini),
   **tidak dibangun di sini** — dicatat sebagai gap pre-existing untuk pemilik modul, bukan
   dianggap sudah tersedia.

## Berkas yang diperiksa

- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (amendment 15 September 2026, § `FE-BKC-036`)
- `docs/module-blueprints/billing-kasir/03-frontend-architecture.md` (tabel "Penanda status", tabel "Aksi per baris pada Monitoring Permintaan")
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (amendment 15 September 2026, § "Perubahan yang merusak konsumen")
- `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` (dikonfirmasi tidak ada route `approve`/`reject`)
- `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs` (`PettyCashVoucherSummaryResponse`)
- `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` (`GetSummaryAsync`)
- Seluruh berkas frontend `src/**/petty-cash/**` yang disebut § Berkas di bawah, sebelum diubah

## Berkas yang diubah (frontend, `QuilvianSystemFrontendDev`)

| Berkas | Perlakuan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx` | Thunk `approvePettyCashVoucher`/`rejectPettyCashVoucher` **dihapus total** (bukan disembunyikan) beserta seluruh referensinya di tiga `addMatcher`; thunk lain tidak disentuh |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | Import kedua thunk dihapus; `ACTION_THUNK_BY_TYPE` hanya `CANCEL`/`DISBURSE`; cabang payload `rejectionReason` dihapus dari `confirmAction`; komentar diperbarui |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | `PETTY_CASH_VOUCHER_STATUS_BADGE_CONFIG` diganti total ke kosakata baru (`requested`/`cash_received`/`completed`/`reversed`/`rejected`); `PETTY_CASH_VOUCHER_ACTION_LABELS` hanya `CANCEL`/`DISBURSE` ("Cairkan") |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | `ACTION_CONFIRM_COPY`/`ACTION_BUTTON_VARIANT` hanya `CANCEL`/`DISBURSE`; `voucherSummaryItems` dipetakan ulang ke field `PendingDisbursementCount`/`CashReceivedCount`/`CompletedCount`/`CancelledCount`/`RejectedCount`; label tombol "+ Buat Voucher" → "+ Buat Permintaan" |
| `src/components/view/.../petty-cash/create-voucher-modal.jsx` | Judul modal "Buat Voucher Kas Kecil" → "Buat Permintaan Petty Cash"; tombol submit "Ajukan Voucher" → "Ajukan Permintaan" |

**Tidak disentuh** (di luar scope task ini, milik `FE-BKC-038` atau bukan requirement task ini):
`voucher-detail-modal.jsx` (riwayat perintah legacy tetap menampilkan `Disetujui`/`Ditolak` untuk
baris sebelum 15 September — itu benar, bukan bug), `attach-proof-modal.jsx`,
`PETTY_CASH_VOUCHER_COMMAND_TYPE_LABELS` (vocabulary audit-trail historis, bukan status hidup),
`petty-cash-budget-*` (milik `FE-BKC-035`/`037`), penanda `isCancelled` (lihat temuan #5).

## Pemetaan kontrak API

| Endpoint | Perubahan | Dampak pada frontend |
| --- | --- | --- |
| `POST /vouchers/{id}/approve` | **Dihapus** dari backend | Thunk dan pemanggilannya dihapus total dari frontend |
| `POST /vouchers/{id}/reject` | **Dihapus** dari backend | Thunk dan pemanggilannya dihapus total dari frontend |
| `POST /vouchers/{id}/disburse` | Gerbang berubah — kini menerima `REQUESTED` | Tidak ada perbandingan status hardcode di frontend (dikonfirmasi lewat pencarian `WAITING_APPROVAL`/`APPROVED` — nihil), sehingga tombol Cairkan otomatis mengikuti `availableActions` server tanpa perubahan tambahan |
| `GET /vouchers` | Query tidak berubah | Tidak disentuh |
| `GET /vouchers/summary` | **Bentuk response berubah** (lihat temuan #1) | `voucherSummaryItems` dipetakan ulang ke field baru |

## Validasi

| Command/aktivitas | Hasil |
| --- | --- |
| `node --check` pada 3 berkas logika non-JSX (`use-petty-cash-vouchers.js`, `petty-cash-voucher-constants.js`, `petty-cash-voucher-slice.jsx` — via salinan sementara `.mjs`) | **PASS** — tidak ada error sintaks |
| Pencarian `approvePettyCashVoucher`/`rejectPettyCashVoucher`/`/approve`/`/reject` di seluruh `src/` | Hanya tersisa pada komentar penjelas di 4 berkas yang memang sengaja menyebutnya (dokumentasi penghapusan); tidak ada pemanggilan aktif tersisa |
| Pencarian `WAITING_APPROVAL`/`APPROVED` (perbandingan status hardcode) di `src/components/view/.../petty-cash/` | Nihil — risiko "tombol Cairkan tidak pernah muncul" yang disebut roadmap tidak berlaku di codebase ini |
| `npm run lint:errors` | **Tidak dijalankan** — instruksi eksplisit pengguna |
| `npm run test:unit` | **Tidak dijalankan** — instruksi yang sama |
| `npm run build` | **Tidak dijalankan** — instruksi yang sama |
| Verifikasi manual browser (tombol Setujui/Tolak tidak ada di layar mana pun; voucher warisan tampil `Menunggu Pencairan` dengan tombol Cairkan; baris `Ditolak (arsip)` hanya Detail) | **MANUAL TEST: NOT FEASIBLE** — dev server tidak dijalankan sesi ini mengikuti instruksi yang sama |

**Konsekuensi eksplisit:** task ini berstatus **source selesai, belum diverifikasi**. DoD
`FE-BKC-036` yang menuntut "`npm run build` lulus" dan verifikasi manual browser **belum
terpenuhi** — keduanya menunggu pengguna. Jangan menandai task ini "Selesai" pada roadmap sampai
kedua bukti itu ada.

## Status Git

Belum di-stage, belum di-commit. `git status --short` pada repository frontend (gabungan dengan
sisa perubahan `FE-BKC-035` yang juga belum di-commit): 13 berkas berubah, 2 dihapus, 4 baru —
lima di antaranya (`create-voucher-modal.jsx`, `petty-cash-vouchers-view.jsx`,
`petty-cash-voucher-constants.js`, `use-petty-cash-vouchers.js`, `petty-cash-voucher-slice.jsx`)
adalah hasil task ini; sisanya hasil `FE-BKC-035` (lihat laporannya).

## Risiko dan pemilik

- **Belum ada bukti build/lint/manual test** — sama seperti `FE-BKC-035`, ini risiko terbesar saat
  ini. Owner: pengguna, sebelum lanjut ke `FE-BKC-038`.
- **`RETURN`/`REVERSE` belum punya tombol** (temuan #4) — diharapkan sesuai urutan roadmap
  (`FE-BKC-038` menempel pada task ini), bukan regresi, tapi risiko kalau ada yang membaca
  `availableActions` server sekarang mengira baris `Menunggu Bukti`/`Selesai` seharusnya sudah
  bisa dikembalikan sisanya dari layar — belum bisa sampai `FE-BKC-038` selesai. Owner: Frontend.
- **Penanda `isCancelled` tidak pernah ada** (temuan #5) — technical debt pre-existing di luar
  cakupan task ini, dilaporkan tanpa diperbaiki sesuai aturan cakupan perubahan. Owner: pemilik
  modul Billing/Kasir, untuk diputuskan apakah perlu task tersendiri.

## Langkah berikutnya

1. Pengguna menjalankan `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan
   memverifikasi manual di browser sesuai tiga butir Verifikasi pada roadmap
   (`frontend-roadmap.md` § `FE-BKC-036`).
2. Setelah bukti di atas ada, perbarui laporan ini (§ Validasi) dan tandai `FE-BKC-036` selesai
   pada `roadmap/frontend-roadmap.md` beserta bukti pada `requirement-traceability.md`.
3. Lanjut ke `FE-BKC-038` (kembalikan sisa uang, batalkan pencairan) — dependency-nya
   (`FE-BKC-036` di atas, `BE-BKC-057` ✅) sudah terpenuhi secara source, tinggal menunggu
   verifikasi task ini.
