# Laporan Perubahan Frontend — `FE-FIN-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-013` |
| Judul | Petugas AR mencatat PPh 23/biaya admin bank saat mengalokasikan penerimaan |
| Slice | `REV-4` — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task, baris `FE-FIN-013`) dan `roadmap/00-delivery-roadmap.md` bagian 4 |
| Trace | `FR-FIN-093`..`095`; `FIN-DEC-049`, `055`, `058`; `FIN-API-1.2` C.2; `FIN-STATE-1.3` C.4; `FIN-VAL-1.3` C.1, C.3 (`FIN-VAL-118`, `121`, `128`, `137`) |
| Contract version | `FIN-API-1.1`/`FIN-API-1.2` §B.8/C.2 — `POST /receipts/{id}/allocations` diperluas aditif dengan `deductions[]` |
| Wewenang UI | `Tetap di dalam alur satu penerimaan` (`03-frontend-architecture.md` §12.3) — potongan **tidak** dijadikan layar sendiri, hanya baris tambahan pada modal alokasi yang sudah ada |
| Dependency | `BE-FIN-040` (source lengkap; `dotnet build` dikonfirmasi berhasil oleh pemilik repository), `FE-FIN-004` 🟡 (layar alokasi dasarnya sudah dibangun sesi yang sama — [laporan](FE-FIN-004.md)) |
| Klasifikasi | `LIGHT` — satu repository (skor 0); ≤8 berkas diperiksa domain terdekat (skor 0); 5 berkas diubah, 0 baru (skor 0); logika UI kecil — baris tambahan pada form yang sudah ada (skor 0); kontrak API — field aditif pada endpoint yang sudah dikonsumsi (skor 0); tanpa perubahan skema (skor 0); tanpa keputusan otorisasi baru (skor 0); UI — perluasan form existing, bukan layar baru (skor 0). Total 0 → `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/hooks/finance/receivable/use-finance-receipt-allocation-editor.jsx`, `use-finance-receipt-detail.jsx` (diubah); `src/components/view/finance/receivable/receipt/allocate-receipt-modal.jsx`, `detail/finance-receipt-detail-view.jsx` (diubah); `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` (diubah); `NewQuilvianSystemBackend` — laporan ini dan tautan bukti pada roadmap modul yang sama |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | Belum di-commit — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `50f29ccc` |
| Tanggal | 30 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap, `npm run lint:errors` PASS, `npm run build` PASS; verifikasi manual/runtime `NOT FEASIBLE` — sama seperti `FE-FIN-004`, tidak ada instance backend berjalan yang dikonfirmasi pada sesi ini |

---

## 1. Keadaan yang ditemukan di awal

`FE-FIN-004` (dikerjakan pada sesi yang sama, lihat laporannya) baru saja membangun modal
"Alokasikan Penerimaan" tanpa baris potongan — cakupan itu sengaja disisakan untuk task ini
supaya batas kedua task tetap jelas. Backend (`BE-FIN-040`) sudah mendukung penuh: `AllocateAsync`
menerima `Deductions[]` opsional per baris alokasi, menegakkan `FIN-VAL-118/128/137`, dan
`ReverseAllocationAsync` sudah membalik seluruh potongan pada satu alokasi dalam transaksi yang
sama. `GET /{id}/deductions` juga sudah ada untuk menampilkan riwayat potongan. Nol pekerjaan
backend baru dibutuhkan — murni menyambungkan UI ke kontrak yang sudah lengkap.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR (sama seperti `FE-FIN-004`).

**Langkah normal:**

1. Pada modal **Alokasikan Penerimaan** (dibuka dari Detail Penerimaan), setelah petugas memilih
   piutang pada satu baris alokasi, bagian **"Potongan (PPh 23 / Biaya Admin Bank) — opsional"**
   muncul di bawah baris itu.
2. Petugas menekan **Tambah Potongan**, memilih jenis (**PPh 23** atau **Biaya Admin Bank** —
   tidak ada opsi lain), mengisi nominal, dan opsional alasan/nomor referensi. Boleh menambah
   lebih dari satu baris potongan per alokasi.
3. Saat disimpan, `POST /{id}/allocations` mengirim `deductions[]` bersama baris alokasinya.
   Backend mengurangi sisa piutang dengan uang alokasi **dan** seluruh potongannya — nominal
   sisa piutang yang benar selalu dibaca ulang dari layar piutang (`/finance/receivable/[slug]`,
   tidak disentuh task ini), bukan dihitung di layar ini.
4. Pada Detail Penerimaan, baris potongan setiap alokasi tampil di bawah baris alokasinya (jenis,
   alasan, nominal) — apa adanya dari `GET /{id}/deductions`, nol perhitungan ulang.
5. Bila alokasi dibalik (tombol Balik yang sudah ada dari `FE-FIN-004`), seluruh potongan pada
   alokasi itu ikut dibalik otomatis oleh backend dalam transaksi yang sama — layar tidak
   menyediakan tombol balik terpisah untuk potongan, sesuai perilaku backend.

**Jalur tidak normal:**

- **Piutang belum dipilih:** bagian potongan tersembunyi total — sesuai `FIN-VAL-128`, potongan
  tidak pernah ditawarkan pada baris `INVOICE_DIRECT`.
- **Jenis potongan `OTHER`:** **tidak pernah ditawarkan sebagai opsi** — backend menolaknya
  fail-closed (`FIN-VAL-137`, kode akuntansi ketiga belum diratifikasi Accounting, `FIN-OQ-033`).
  Menawarkannya di klien hanya akan menjanjikan sesuatu yang pasti gagal disimpan.
- **Gagal:** pesan error backend apa adanya (mis. validasi nominal, atau piutang belum dipilih).
- **Status kirim kejadian akuntansi:** layar ini **tidak pernah** menampilkan indikator "berhasil
  terkirim ke Accounting" untuk kode `PotonganPph23Piutang`/`PotonganBiayaBankPiutang` — sesuai
  pola bagian 12.7 (status pengiriman kejadian akuntansi adalah tanggung jawab layar pemantauan
  kejadian yang sudah ada, bukan layar ini).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (kartu task
  `FE-FIN-013`), `03-frontend-architecture.md` §12.3, §12.7
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.8/C.2,
  `state-transition-matrix.md` §B.8/C.4, `validation-matrix.md` (`FIN-VAL-118`, `121`, `128`, `137`)
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` (`AllocateAsync`
  bagian validasi/penulisan `Deductions`, `ReverseAllocationAsync` bagian cascade pembalikan) —
  dibaca ulang sebagai bukti kontrak nyata, **tidak diubah**
- Frontend — berkas `FE-FIN-004` yang baru dibangun pada sesi yang sama (dibaca sebagai basis
  perluasan, bukan rujukan pola dari fitur lain)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/receivable/finance-receipt-constants.jsx` | Tambah `FINANCE_RECEIPT_DEDUCTION_TYPES`/`_LABELS`/`_OPTIONS` (hanya PPh 23 dan Biaya Admin Bank — `OTHER` sengaja tidak ditawarkan) |
| `src/lib/hooks/finance/receivable/use-finance-receipt-allocation-editor.jsx` | Tambah state `deductions` per baris alokasi, `addDeduction`/`removeDeduction`/`updateDeduction`, validasi (piutang wajib terisi, jenis wajib, nominal > 0), payload `POST` diperluas mengirim `deductions[]` |
| `src/components/view/finance/receivable/receipt/allocate-receipt-modal.jsx` | Tambah sub-bagian "Potongan" per baris alokasi — hanya tampil bila piutang sudah dipilih |
| `src/lib/hooks/finance/receivable/use-finance-receipt-detail.jsx` | Tambah pemanggilan `GET /{id}/deductions`, pengelompokan baris potongan per `receiptAllocationId` |
| `src/components/view/finance/receivable/receipt/detail/finance-receipt-detail-view.jsx` | Tambah baris tampilan potongan di bawah setiap baris alokasi pada tabel Daftar Alokasi |

Tidak ada berkas baru — seluruhnya perluasan aditif atas `FE-FIN-004` yang baru dibangun.

### 3.3 Kepatuhan arsitektur frontend

Perluasan mengikuti struktur hook/komponen yang sama persis dengan `FE-FIN-004` (tidak ada state
management atau pola baru diperkenalkan). Bagian potongan memakai elemen form Bootstrap mentah
yang sama dengan baris alokasi di atasnya — konsisten, bukan komponen terpisah.

**Tabel keputusan base component:** tidak ada elemen baru pada task ini — seluruhnya perluasan
form yang sudah dinilai pada laporan `FE-FIN-004` (`allocate-receipt-modal.jsx` tetap `COMPOSE`
form modal + tabel mentah, tidak berubah keputusannya).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Piutang belum dipilih | Bagian potongan tidak dirender sama sekali |
| Gagal simpan | Pesan error backend apa adanya (alert modal) |
| Potongan kosong pada satu alokasi | Baris tampilan potongan di Detail Penerimaan tidak dirender untuk alokasi itu |

Keempat state umum (memuat/kosong/gagal/tanpa hak akses) sudah dibahas laporan `FE-FIN-004` —
tidak berubah oleh task ini.

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru — task ini memakai **field tambahan** pada endpoint yang sama dengan
`FE-FIN-004`.

#### Corporate / Finance Management / Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/finance-management/receipts/{id}/allocations` | Sekarang mengirim `deductions[]` opsional per baris | `FinanceReceipt : Allocate` |
| `GET` | `/v1/corporate/finance-management/receipts/{id}/deductions` | Menampilkan riwayat potongan per alokasi | `FinanceReceipt : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error | `PASS` | Keluaran `eslint . --quiet` selesai tanpa output error |
| `npm run build` | Route `/finance/receipts`, `/finance/receipts/[slug]` tetap ter-compile setelah perubahan | `PASS` | Keluaran build tanpa `Failed to compile` |
| Potongan tersembunyi sebelum piutang dipilih | — | `PASS` (review kode) | `allocate-receipt-modal.jsx` — render kondisional `line.receivableId ? ... : null` |
| Jenis `OTHER` tidak dapat dipilih | — | `PASS` (review kode) | `FINANCE_RECEIPT_DEDUCTION_TYPE_OPTIONS` hanya berisi `PPH23`/`BANK_ADMIN_FEE` |
| Susun alokasi + potongan PPh 23, simpan, verifikasi sisa piutang berkurang uang alokasi + potongan | — | `NOT FEASIBLE` | Memerlukan backend berjalan dan data nyata |
| Balik alokasi yang punya potongan, verifikasi potongan ikut terbalik di tampilan | — | `NOT FEASIBLE` | Sama seperti di atas |

Uji manual: `NOT FEASIBLE` — sama seperti `FE-FIN-004`, tidak ada instance backend yang
dikonfirmasi berjalan pada sesi ini.

**AUTOMATED TEST:** `NOT APPLICABLE — repository ini tidak memelihara Jest/test runner otomatis (rules/frontend/test-policy.md).`

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat` — tidak diminta task ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Sisa piutang sesudah potongan dari response backend | Terpenuhi | Layar ini tidak menghitung sisa piutang sama sekali — nol logika pengurangan di klien; angka resmi tetap dibaca dari layar piutang yang sudah ada |
| Baris potongan tetap di dalam alur satu penerimaan | Terpenuhi | Ditambahkan ke modal alokasi yang sudah ada, bukan layar terpisah |
| `npm run lint:errors` | Terpenuhi | `PASS` |
| `npm run build` | Terpenuhi | `PASS` |
| DoD: status kirim kejadian `POTONGAN-PIUTANG-NON-TUNAI` tidak ditampilkan sebagai berhasil | `NOT APPLICABLE` — kode itu sendiri sudah **superseded** (`AMENDMENT REVISI 6`, komentar `FinanceReceiptService.cs`); backend kini menulis `PotonganPph23Piutang`/`PotonganBiayaBankPiutang`. Layar ini tidak menampilkan indikator status kirim kejadian akuntansi sama sekali untuk kode apa pun — DoD terpenuhi secara struktural | Review kode `finance-receipt-detail-view.jsx`, `allocate-receipt-modal.jsx` |

Task ini **belum** dapat ditandai `✅` — verifikasi manual/runtime belum terbukti lewat eksekusi
sungguhan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Tidak ada temuan baru — seluruh gap backend terkait rumpun ini sudah dicatat pada laporan `FE-FIN-004` |
| Dependency backend | `BE-FIN-040` — dikonfirmasi lengkap dan `dotnet build` berhasil oleh pemilik repository. `FE-FIN-004` 🟡 (prasyarat langsung) — bagian yang relevan untuk task ini (alokasi manual) sudah selesai; gap rekonsiliasi shift pada `FE-FIN-004` tidak berdampak ke task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup 5 berkas yang diubah pada task ini (lihat bagian 3.2) — seluruhnya berkas yang baru dibuat `FE-FIN-004` pada sesi yang sama, belum di-commit siapa pun |
| Langkah berikutnya | (1) Jalankan backend dan uji alokasi+potongan hidup terhadap data `FinReceipt`/`FinReceivable` nyata; (2) verifikasi `FIN-VAL-118/128/137` benar-benar ditolak sesuai pesan; (3) setelah runtime terbukti, perbarui laporan ini dan `FE-FIN-004` bersamaan, lalu tanda status roadmap ke ✅ untuk keduanya (kecuali gap rekonsiliasi shift `FE-FIN-004` yang menunggu backend terpisah) |
