# Laporan Perubahan Frontend — `FE-FIN-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-019` |
| Judul | Alokasi penerimaan yang dibalik dan penghapusan piutang dapat ditelusuri dari layar sendiri |
| Slice | `REV-13B` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`; `03-frontend-architecture.md` §17.2 (FIN-LYR-AR-06, FIN-LYR-AR-16), §17.3 |
| Contract version | `FIN-API-1.4` §D.2 — `GET /receipts/reversed-allocations` (`BE-FIN-054`) dan `GET /receivables/write-offs` (`BE-FIN-053`), keduanya `approved`, nol endpoint baru dikonsumsi |
| Wewenang UI | `FIN-DEC-094` approved product brief — mengunci kedua layar berdiri sendiri sebagai permukaan **baca saja**. Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/R"); `BE-FIN-053` ✅ dan `BE-FIN-054` ✅ (keduanya selesai penuh, `dotnet build` PASS dikonfirmasi pengguna) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 8 berkas baru + 1 diubah (skor 1); 2 hook baru + 2 view baru (skor 1); nol endpoint baru dikonsumsi (skor 0); database — tidak relevan (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — 2 layar baru, keduanya baca saja (skor 1). Total 3 → `MEDIUM` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk memverifikasi DTO dan `[AccessPermission]` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/receipts/reversed-allocations/**`, `src/app/finance/receivable/write-offs/**`, `src/components/view/finance/receivable/{reversed-allocation,write-off}/**`, `src/lib/hooks/finance/receivable/{use-reversed-allocation-list,use-receivable-write-off-list}.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `99df41e1` — branch `Yasmina` (read-only) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS**. `npm run test:unit`/`npm run build` **NOT RUN** — menunggu konfirmasi eksplisit pengguna |

---

## 1. Keadaan yang ditemukan di awal

`BE-FIN-053`/`BE-FIN-054` membangun dua endpoint baca lintas-entitas (`GET /receivables/write-offs`,
`GET /receipts/reversed-allocations`) sejak awal sesi ini, tetapi belum ada satu pun halaman
frontend yang memanggilnya — keduanya baru terjangkau lewat endpoint, bukan lewat menu.

**Verifikasi kontrak sebelum implementasi (dibuktikan dari source, sesuai pengalaman `FE-FIN-017`
yang menemukan permission keliru di dokumen):**

- Kedua DTO (`ReversedAllocationQuery`/`ReversedAllocationRowResponse` dan
  `ReceivableWriteOffQuery`/`ReceivableWriteOffRowResponse`) dibaca persis dari source —
  **dikonfirmasi** `ReversedAllocationRowResponse` memang **tidak punya** field `ReversalReason`
  (dicabut `BE-FIN-054`, tercatat di komentar DTO itu sendiri), sehingga kolom itu **tidak**
  ditulis pada tabel baru.
- Kedua `[AccessPermission]` (`FinanceReceipt:Read` untuk reversed-allocations,
  `FinanceReceivable:Read` untuk write-offs) dibaca langsung dari controller —
  **cocok persis** dengan `03-frontend-architecture.md` §17.2, nol koreksi dibutuhkan (berbeda
  dari temuan `FE-FIN-017`, sama seperti `FE-FIN-018`).

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR Finance, Supervisor/Manajer Finance (keduanya **baca saja**).

**Pemicu:** Pengguna membuka "Receiveable AR Canceled" atau "Pemutihan Piutang" dari grup
"Transaksi A/R".

**Langkah normal — Receiveable AR Canceled:**

1. Pengguna melihat daftar baris **alokasi** (bukan penerimaan) yang pernah dibalik, lintas
   seluruh penerimaan.
2. Pengguna dapat menyaring berdasarkan rentang tanggal pembalikan.
3. **Nol tombol pembalikan di layar ini** — pembalikan baru tetap dilakukan dari detail
   Penerimaan asalnya (`POST /receipts/{id}/allocations/{allocationId}/reverse`, sudah ada).

**Langkah normal — Pemutihan Piutang:**

1. Pengguna melihat daftar seluruh penghapusan piutang (write-off) lintas piutang, beserta
   status maker-checker-nya (Menunggu Persetujuan/Disetujui/Ditolak).
2. Pengguna dapat menyaring berdasarkan status, penjamin, atau rentang tanggal pengajuan.
3. **Nol tombol pengajuan atau persetujuan di layar ini** — keduanya tetap dilakukan dari layar
   piutang asalnya beserta maker-checker-nya yang sudah ada.

**Jalur tidak normal:** Kosong/gagal/tanpa hak akses — identik pola `FE-FIN-017`/`018` (§4).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `03-frontend-architecture.md` §17.2 (baris FIN-LYR-AR-06, FIN-LYR-AR-16) dan §17.3 subbagian
  "FIN-LYR-AR-06 Receiveable AR Canceled dan FIN-LYR-AR-16 Pemutihan Piutang"
- `NewQuilvianSystemBackend/Areas/.../Collection/Dtos/FinanceReceiptDtos.cs` (read-only) —
  `ReversedAllocationQuery` (`StartDate`/`EndDate`/`ReceiptId`/`PageNumber`/`PageSize`,
  `StartDate`/`EndDate` bertipe `DateTimeOffset?`) dan `ReversedAllocationRowResponse`
  (`AllocationId`/`ReceiptId`/`ReceiptNumber`/`ReceivableId`/`ReceivableNumber`/`Amount`/
  `ReversalOfAllocationId`/`ReversedAt`) dibaca persis, termasuk komentar XML yang
  mengonfirmasi **nol** `ReversalReason`
- `.../Receivable/DTOs/FinanceReceivableDtos.cs` (read-only) — `ReceivableWriteOffQuery`
  (`StartDate`/`EndDate`/`Status`/`DebtorReferenceId`/`SortBy`/`SortDirection`/`PageNumber`/
  `PageSize`) dan `ReceivableWriteOffRowResponse` (`Id`/`WriteOffNumber`/`ReceivableId`/
  `ReceivableNumber`/`DebtorReferenceId`/`Amount`/`Reason`/`Status`/`RequestedAt`/`DecidedAt`)
  dibaca persis
- `.../Receivable/Models/FinReceivableAdjustment.cs` (read-only) —
  `FinReceivableApprovalStatuses` (`REQUESTED`/`APPROVED`/`REJECTED`) dibaca persis untuk
  memastikan nilai `Status` pada write-off sama dengan yang sudah dipetakan frontend
  (`LIFECYCLE_ACTION_STATUS_LABELS`/`_BADGE` pada `receivable-constants.jsx`, sudah ada sejak
  modul koreksi/write-off piutang pasien dibangun — **dipakai ulang persis, nol duplikasi**)
- `.../Collection/Controllers/FinanceReceiptsController.cs` (read-only) — `GET
  reversed-allocations`, `[AccessPermission("FinanceReceipt","Read")]` dikonfirmasi
- `.../Receivable/Controllers/FinanceReceivablesController.cs` (read-only) — `GET write-offs`,
  `[AccessPermission("FinanceReceivable","Read")]` dikonfirmasi
- `src/components/view/finance/receivable/invoice-batch/canceled-invoice-batch-view.jsx`
  (`FE-FIN-017`) — modul referensi pola hook+view+columns untuk permukaan baca lintas-entitas,
  **bukan** dipakai ulang langsung (endpoint dan bentuk data berbeda), hanya pola strukturnya
- `src/lib/constants/finance/receivable/receivable-constants.jsx` — dikonfirmasi
  `LIFECYCLE_ACTION_STATUS_LABELS`/`LIFECYCLE_ACTION_STATUS_BADGE` sudah ada dan cocok persis
  dengan tiga nilai status write-off, nol konstanta status baru dibutuhkan
- `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` — `FINANCE_RECEIPT_
  ENDPOINT_BASE` dipakai ulang untuk menyusun endpoint `reversed-allocations`
- `src/lib/hooks/finance/receivable/use-company-guarantor-name-resolver.jsx` — dipakai ulang
  persis untuk resolusi nama penjamin pada kolom "Penjamin" Pemutihan Piutang

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/receivable/use-reversed-allocation-list.jsx` **(baru)** | Hook state lokal — `GET /receipts/reversed-allocations` |
| `src/components/view/finance/receivable/reversed-allocation/reversed-allocation-table-columns.jsx` **(baru)** | Kolom: No, Nomor Penerimaan, Nomor Piutang, Nominal Dibalik, Tanggal Dibalik — **nol kolom alasan** |
| `src/components/view/finance/receivable/reversed-allocation/reversed-allocation-view.jsx` **(baru)** | Layar "Receiveable AR Canceled" — baca saja |
| `src/app/finance/receipts/reversed-allocations/page.jsx` **(baru)** | Route tipis |
| `src/lib/hooks/finance/receivable/use-receivable-write-off-list.jsx` **(baru)** | Hook state lokal — `GET /receivables/write-offs` |
| `src/components/view/finance/receivable/write-off/receivable-write-off-table-columns.jsx` **(baru)** | Kolom: No, Nomor Write-off, Nomor Piutang, Penjamin (diresolusi), Nominal, Alasan, Status (badge), Diajukan, Diputuskan |
| `src/components/view/finance/receivable/write-off/receivable-write-off-view.jsx` **(baru)** | Layar "Pemutihan Piutang" — baca saja |
| `src/app/finance/receivable/write-offs/page.jsx` **(baru)** | Route tipis |
| `src/utils/menu-sidebar/menu-items.jsx` | 2 butir baru disisipkan ke grup "Transaksi A/R" pada posisi persis urutan V1 |

Nol berkas backend disentuh.

### 3.3 Kepatuhan arsitektur frontend

**Base component — seluruhnya `REUSE`, nol `NEW`/`EXTEND`:**

| Elemen | Status | Sumber |
| --- | --- | --- |
| `AccessDeniedGate`, `DataFilter`, `DataTable`, `FilterSelect`, `FilterDatePicker`, `ResourceFilterSelect`, `Hero`, `InformationAlert`, `ToastStack`, `Pagination`, `FinanceBreadcrumb`, `StatusBadge` | `REUSE` | Dipakai apa adanya, sama seperti tiga task sebelumnya |
| `LIFECYCLE_ACTION_STATUS_LABELS`/`_BADGE` | `REUSE` | Konstanta status existing dipakai ulang persis — nol duplikasi definisi status |
| `useCompanyGuarantorNameResolver` | `REUSE` | Dipakai ulang persis untuk resolusi nama penjamin |

Karena seluruh elemen `REUSE`, gerbang keputusan **tidak menghasilkan pilihan bernomor apa pun**.

**Dua hook baru** mengikuti pola hook per-layar yang sudah mapan (`use-receivable-invoice-batch-
list.jsx`/`use-finance-receipt-register.jsx` dari sesi sebelumnya) — state lokal, bukan Redux,
nol hook generik lintas-layar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` menampilkan kerangka baris |
| Kosong | "Tidak ada alokasi yang dibalik pada rentang ini." / "Tidak ada penghapusan piutang pada saringan ini." |
| Gagal | `InformationAlert` variant `danger` |
| Tanpa hak akses | `AccessDeniedGate` menggantikan seluruh isi halaman |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receipts/reversed-allocations` | Receiveable AR Canceled | `FinanceReceipt : Read` |

#### Corporate / Finance Management / Receivable

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receivables/write-offs` | Pemutihan Piutang | `FinanceReceivable : Read` |

Keduanya **sudah ada dan tidak diubah** oleh task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`eslint . --quiet`, exit code 0) | `PASS` | Keluaran perintah |
| `npm run test:unit` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| `npm run build` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| Review diff/scope | 8 berkas baru + 1 diubah, persis sesuai §17.2/§17.3 | `PASS` | `git status --short` |
| Review field DTO vs kolom tabel | Seluruh field `ReversedAllocationRowResponse`/`ReceivableWriteOffRowResponse` dibandingkan satu per satu dengan source — **dikonfirmasi nol `ReversalReason`** | `PASS` | §3.1 |
| Review permission terhadap backend | Dibaca langsung dari `[AccessPermission]` kedua controller, cocok persis dengan dokumen | `PASS` | §3.1 |
| Review nol aksi tulis pada kedua layar | Dikonfirmasi lewat pembacaan kedua view — nol `BaseButton`/modal/form yang memanggil endpoint `POST` apa pun | `PASS` | §3.2 |
| Grep anti-regresi UI (6 pola) | Nol temuan pada kedua file view baru | `PASS` | Keluaran grep kosong di keenam pola |
| Review urutan butir menu grup "Transaksi A/R" | Urutan persis §17.2 V1 (17 butir total, 2 baru disisipkan tepat pada posisinya) | `PASS` | `grep -oP 'label: "\K[^"]+'` atas blok grup |

Uji manual: `NOT FEASIBLE` — server tidak dijalankan pada task ini.

**AUTOMATED TEST: NOT APPLICABLE — repository ini tidak memakai Jest; menulis test baru bersifat
opsional, tidak diminta eksplisit pada task ini.**

**Tidak dijalankan:** `npm run test:unit`, `npm run build` (menunggu konfirmasi eksplisit
pengguna); verifikasi manual/visual di browser (server tidak dijalankan).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-019`) | Status | Bukti |
| --- | --- | --- |
| Keduanya **tidak** menyediakan aksi membuat atau menyetujui apa pun | Terpenuhi | §3.3, §6 |
| Pembuatan write-off dan pembalikan alokasi tetap dari layar asalnya beserta jenjangnya | Terpenuhi | §2 — nol endpoint tulis dipanggil dari kedua layar baru |
| Lint PASS | Terpenuhi | `npm run lint:errors` PASS |
| Build PASS | **Belum terpenuhi** | `npm run build` `NOT RUN`, menunggu konfirmasi pengguna |
| Butir menu terdaftar | Terpenuhi | §3.2 |
| Laporan task tracked ada | Terpenuhi | Laporan ini |

Task ini **belum** dapat ditandai ✅ selama `npm run build` belum dikonfirmasi pengguna.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` belum dijalankan sama sekali untuk task ini — sengaja, mengikuti konvensi sesi saat ini. Risiko dinilai rendah (lint PASS, pola hook/view identik tiga task sebelumnya yang sudah terverifikasi berulang) |
| Masalah yang diketahui | Tidak ada temuan baru di luar yang sudah dicatat `FE-FIN-017`/`018` (permission `/finance/receivable` legacy, bug `onCloseToast` pada halaman tab Purchasing Reports) — keduanya tidak tersentuh task ini |
| Dependency backend | Tidak ada — `BE-FIN-053`/`054` sudah ✅ penuh (build dan, untuk `BE-FIN-052`, migration dikonfirmasi; `BE-FIN-053`/`054` murni permukaan baca, nol migration). `FE-FIN-016` masih 🟡, belum dikonfirmasi build — risiko ditanggung bersama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan 8 berkas baru + 1 berkas diubah, seluruhnya sesuai §3.2 |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run build` dan mengonfirmasi hasilnya untuk `FE-FIN-016`, `017`, `018`, **dan** `019` sekaligus; (2) `FE-FIN-020` (`REV-13C`, bergantung `BE-FIN-052` ✅) atau `FE-FIN-021` (`REV-13B`, bergantung `BE-FIN-055` ✅) sebagai lanjutan berikutnya |
