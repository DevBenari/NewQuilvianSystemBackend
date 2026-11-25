# Laporan Perubahan Frontend — `FE-AST-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-017` |
| Judul | Pencocokan sampai tindak lanjut |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/frontend-roadmap.md` — kartu `FE-AST-017` |
| Trace | `CAP-07.3..07.6`, tindak lanjut selisih; `SCR-AST-INV-03..06`; `03-frontend-architecture.md` §3; `AST-DEC-019`, `046` |
| Contract version | `AST-INV-1.0.0`; layar mengikuti **source** `BE-AST-023` termasuk deltanya (`GET /follow-ups/{id}`, `draft` pada proses, `followUps[]` per hasil, `rowVersion` pada ajukan/verifikasi) |
| Wewenang UI | Tata letak `DEV_DISCRETION` dalam pola layar aset yang sudah ada |
| Dependency | `FE-AST-016` ✅, `BE-AST-023` ✅ |
| Klasifikasi | `STANDARD` — 4 route, 1 view + 1 hook dengan 4 varian, tambahan kolom/konstanta/service/label, 4 butir menu |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-23 (berpasangan dengan `BE-AST-023`) |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7baebf2f4` (branch `RizkiV2`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — keempat kriteria terpetakan ke source dan teruji di layar (Playwright **12/12** pada build produksi Rizki `BUILD_ID` 15:31:41) |

---

## 1. Proses bisnis dari sisi pengguna

Satu view melayani empat butir menu di grup **Inventaris & Pemeriksaan Fisik**. Tanpa `?sessionId=`, setiap butir
menampilkan pemilih sesi yang disaring status sesuai tahapnya.

| Menu (layar) | Sesi yang dipilih | Yang dilihat / dilakukan |
|---|---|---|
| **Pencocokan Data** (`SCR-AST-INV-03`) | `Berjalan`, `Pencocokan` | Kartu enam kategori, tab kategori (Semua + enam), tabel hasil. Ketua tim: **Tutup Pemindaian**, **Buka Kembali** (alasan), **Ubah catatan tim** per baris, **Ajukan Verifikasi** |
| **Selisih Inventaris** (`SCR-AST-INV-04`) | `Pencocokan`, `MenungguVerifikasi`, `Diverifikasi` | Hanya selisih; kolom Usulan berisi chip "jenis: status" atau "jenis: nomor dokumen". Petugas Aset: **Proses: <jenis>** (modal berisi rencana tindakan, penanggung jawab tujuan opsional, catatan; untuk temuan: data draft aset). Kepala Bagian Aset: **Abaikan: <jenis>** dengan alasan |
| **Verifikasi Hasil** (`SCR-AST-INV-05`) | `MenungguVerifikasi` | Kepala Bagian Aset bukan anggota tim: **Verifikasi** atau **Kembalikan** (alasan). Anggota tim: tombol tidak ada, diganti keterangan "Anda anggota tim sesi ini…" |
| **Berita Acara** (`SCR-AST-INV-06`) | `Diverifikasi` | **Cetak Berita Acara** membuka PDF di tab baru; ringkasan dan daftar selisih |

Penolakan selain 409 (mis. ajukan dengan selisih tanpa catatan) tampil sebagai toast dan modal tetap terbuka agar
pengguna bisa kembali; 409 menutup modal dan memuat ulang sesi — pola yang sama dengan hook aksi aset lain.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/asset-management/stock-take/asset-stock-take-review-view.jsx` | Baru — satu view untuk empat varian |
| `src/lib/hooks/corporate/asset-management/stock-take/use-asset-stock-take-review.jsx` | Baru — pemilih sesi, hasil + tab + cari, aksi sesi (tutup, buka kembali, ajukan, verifikasi, kembalikan), catatan, proses/abaikan usulan, PDF |
| `src/app/corporate/asset-management/stock-take/{matching,discrepancies,verification,reports}/page.jsx` | Baru — 4 route, `?sessionId=` divalidasi UUID |
| `asset-stock-take-table-columns.jsx` | + `buildStockTakeResultColumns` (kategori, tercatat, ditemukan, catatan tim, chip usulan, aksi baris sesuai hak) |
| `asset-stock-take-constants.jsx` | + `ASSET_STOCK_TAKE_REVIEW_VARIANTS` (judul, deskripsi, status pemilih, tab), tab kategori, salinan modal |
| `asset-stock-take-service.js` | + catatan, ajukan, verifikasi, hasil, daftar/rincian/proses/abaikan usulan, unduh dokumen |
| `src/utils/corporate/asset-management/asset-status-labels.js` | + label/nada enum `stockTakeCategory`, `stockTakeFollowUpType`, `stockTakeFollowUpStatus` |
| `src/utils/menu-sidebar/menu-items.jsx` | + Pencocokan Data, Selisih Inventaris, Verifikasi Hasil, Berita Acara (`AssetStockTake : Read`) |

Tanpa baris komentar; tanpa paket baru; tanpa CSS baru.

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Hero, kartu kategori, tab + cari, tabel, paginasi, toast | `Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `Pagination`, `ToastStack` | `REUSE` |
| Pemilih sesi, jumlah baris, cara perolehan | `FilterSelect` | `REUSE` |
| Penanggung jawab tujuan, kelompok aset, jenis kepemilikan | `AssetMasterResourceFilter` dalam `BaseFormControl` | `REUSE` |
| Catatan, alasan, data draft (nama, tanggal, nilai) | `BaseInputField`, `BaseTextAreaField` | `REUSE` |
| Keterangan anggota tim, alasan pengembalian terakhir, status dokumen | `InformationAlert` | `REUSE` |
| Konfirmasi sesi, alasan, catatan, proses, abaikan | `ConfirmModal` (dengan `requireReason` atau isi field) | `REUSE` |
| Gerbang hak | `AccessDeniedGate` | `REUSE` |
| Aksi baris | `RowActionMenu` (lewat kolom) | `REUSE` |
| Buka PDF di tab baru | `useAssetPdfTab` (dipakai dokumen aset lain) | `REUSE` |

`UI GATE: 9 elemen — REUSE 9, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0`

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 11 berkas task (`--max-warnings=0`) | 0 galat, 0 peringatan |
| `npm run build` (Rizki) | Berhasil, `BUILD_ID` 2026-11-23 15:31:41; manifest memuat `stock-take/{matching,discrepancies,verification,reports}` |
| Grep anti-regresi | Tanpa CSS baru, tanpa `<button>`/`<table>` mentah |

**MANUAL TEST (Playwright, Edge, 1600×900, build produksi `start:local` di port 3000): 12/12** —
`QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-017-20261123/uji-fe-017.mjs`, `hasil-uji.jsonl`,
tangkapan `01`, `01b`, `02`, `03`, `04-*.png` (diperiksa langsung). Dimas Kurniawan ketua tim, Siti anggota; SuperAdmin
bukan anggota sebagai verifikator dan pemroses. Sesi `AST-OPN-000020`, 22 aset diharapkan, 1 aset dipindai di ruangan
lain.

| Langkah | Hasil |
| --- | --- |
| `AC4` empat butir menu tampil | Lulus |
| Pemilih sesi Pencocokan meminta `status=Berjalan&status=Pencocokan` | Lulus |
| `SCR-INV-03` Tutup Pemindaian dari layar | 200, toast "Pemindaian Ditutup", kartu Sesuai 1 / Tidak Ditemukan 20 / Beda Lokasi 1 |
| Ajukan tanpa catatan | 400; toast "Beri catatan pada 21 selisih sebelum mengajukan verifikasi." |
| Ubah catatan tim dari menu baris | `PUT` 200, catatan tampil di tabel |
| Ajukan Verifikasi dari layar | 200, "Diajukan untuk Verifikasi" |
| `AC2` anggota tim membuka Verifikasi Hasil | Tombol Verifikasi tidak ada; keterangan anggota tim tampil |
| `SCR-INV-05` verifikasi oleh bukan anggota | 200, "Sesi Diverifikasi" |
| `AC3` Proses: Koreksi Lokasi | Modal menyebut "Penyerahan koreksi opname ke ruangan Ruang Uji U9KOL B"; 200; toast dan chip baris "Koreksi Lokasi: AST-PNY-000016" |
| `SCR-INV-06` Cetak Berita Acara | `GET /document` 200 `application/pdf`, tab baru terbuka |
| Tanpa galat halaman dan tanpa respons 5xx | Lulus |

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

**Putaran 1 — 5/5 lalu berhenti** di langkah catatan: sesudah ajukan ditolak 400, modal konfirmasi tetap terbuka (perilaku
yang dimaksud, lihat bagian 1) dan toast di pojok kanan bawah menutupi tombol aksi baris. Skrip diperbaiki: tekan
**Kembali** lalu tutup toast sebelum klik aksi baris. Tidak ada perubahan source.

Tidak diuji dari layar di task ini: **Abaikan** usulan dan formulir **draft temuan**. Keduanya diuji di API
(`BE-AST-023`: abaikan tanpa/dengan alasan, temuan tanpa/dengan `draft`) dan memakai `ConfirmModal` + field yang sama
dengan langkah yang diuji.

Tangkapan `01`, `01b`, `03`, `04`: footer dan selubung modal tampak berhenti di tinggi 900 px — artefak `fullPage` pada
elemen berposisi tetap, bukan tata letak. Di `04` tab Beda Lokasi tampak aktif bersama Semua Selisih karena kursor
tertinggal di atasnya sesudah klik tombol modal (hover).

Data uji dirapikan: penyerahan `AST-PNY-000016` dibatalkan, usulan lain diabaikan; sesi `AST-OPN-000019` (putaran 1)
dibatalkan.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. `SCR-AST-INV-03..06` | View + hook 4 varian, 4 route | Langkah tutup, catatan, ajukan, verifikasi, proses, cetak | ✅ |
| 2. Tombol verifikasi tersembunyi bagi anggota tim | Hook (`availableActions` sesi) + view | `AC2`, tangkapan `02` | ✅ |
| 3. Proses usulan menampilkan nomor dokumen tujuan | Toast + chip usulan di kolom | `AC3`, tangkapan `04` | ✅ |
| 4. Empat butir menu terdaftar | `menu-items.jsx` | `AC4` | ✅ |

| DoD | Status |
|---|---|
| `V-FE` lint, build produksi, uji layar | Terpenuhi |
| `UAT-INV-01`, `04`, `05` | Milik tim UAT |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `BE-AST-023`, menunggu izin Rizki; berkas akuntansi yang sedang diubah Rizki tidak ikut di-stage |

---

## 5. Langkah berikutnya

- `FE-AST-018` sesudah `BE-AST-024`: kartu "Inventaris Tertunda" di dashboard dan laporan inventaris dengan ekspor.
