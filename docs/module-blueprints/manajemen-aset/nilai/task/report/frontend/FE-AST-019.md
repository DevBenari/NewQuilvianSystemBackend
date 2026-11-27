# Laporan Perubahan Frontend — `FE-AST-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-019` |
| Judul | Akun kontrol dan pusat biaya |
| Gelombang | `MVP-4` — sub-modul `nilai` |
| Roadmap | `nilai/roadmap/frontend-roadmap.md` — kartu `FE-AST-019` |
| Trace | `AST-DES-011`, `035`; `SCR-AST-NLI-06` bagian 3–4 |
| Contract version | `AST-NLI-1.0.0`; layar mengikuti **source** `BE-AST-026` termasuk deltanya (`GET /unit-cost-centers/cost-center-options`, `unclearOnly`, `warnings`) |
| Wewenang UI | `DEV_DISCRETION`; tab tambahan pada layar `FE-AST-011` |
| Dependency | `FE-AST-011` ✅ (layar yang sama), `BE-AST-026` ✅ |
| Klasifikasi | `STANDARD` — 2 hook, kolom, tab dan modal pada view yang sama |
| Task mode | `FRONTEND` — perintah Rizki 2026-11-24 |
| Target tulis | `QuilvianSystemFrontendDev/src` (aset `valuation/*`); laporan ini dan baris status |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `3b59685d8` (branch `RizkiV2`) |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar (Playwright **20/20** bersama `FE-AST-011` + cek perbaikan **3/3**); build Rizki `BUILD_ID` 10:48:23 memuat perbaikan terakhir. **Riwayat:** 🟡 pada hari yang sama karena `BUILD_ID` 10:36 mendahului perbaikan

---

## 1. Proses bisnis dari sisi pengguna

| Tab | Yang dilihat / dilakukan |
|---|---|
| **Akun Kontrol** (`AssetSubledgerSetup : Read`) | Semua kelompok: akun harga perolehan, akumulasi penyusutan, akumulasi penurunan nilai (kode + nama dari daftar akun Akuntansi); kolom Catatan **Lengkap** / **Perlu dicek** / **Belum diatur** beserta peringatannya; keterangan "{n} kelompok aktif belum punya akun kontrol…". Aksi baris **Tetapkan akun kontrol** (`Update`): tiga kode akun; kode yang ditolak backend (`AST-VAL-NLI-017`) ditandai di field-nya masing-masing |
| **Pusat Biaya** (`AssetSubledgerSetup : Read`) | Semua unit: pusat biaya pembebanan, sumber **Otomatis** / **Pemetaan** / **Belum Jelas**, jumlah pusat biaya aktif milik unit, alasan; saringan **Belum jelas pusat biayanya** dan cari. Aksi baris **Petakan pusat biaya** / **Ubah pemetaan** (pilihan pusat biaya aktif badan hukum, dapat dicari) dan **Hapus pemetaan** (konfirmasi; unit kembali otomatis) |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/asset-management/valuation/use-asset-subledger-accounts.jsx` | Baru — daftar, cari, simpan, pemetaan galat `017` ke field |
| `src/lib/hooks/corporate/asset-management/valuation/use-asset-unit-cost-centers.jsx` | Baru — daftar + saringan, pilihan pusat biaya (dimuat saat modal dibuka), petakan, hapus |
| `use-asset-valuation-settings.jsx` | + tab Akun Kontrol dan Pusat Biaya menurut hak `AssetSubledgerSetup : Read` |
| `asset-valuation-settings-view.jsx` | + isi dua tab, tiga modal, saringan unit |
| `asset-valuation-table-columns.jsx` | + kolom akun kontrol dan pusat biaya |
| `asset-valuation-service.js`, `asset-valuation-constants.jsx` | + 6 panggilan; tab, field akun, opsi saringan, nada sumber |

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Tabel, lencana, aksi baris, keterangan | `DataTable` (`pagination={false}`), `StatusBadge`, `RowActionMenu`, `InformationAlert` | `REUSE` |
| Saringan belum jelas, pilihan pusat biaya | `FilterSelect` (pilihan dengan `searchable`) | `REUSE` |
| Form kode akun, konfirmasi hapus | `ConfirmModal` + `BaseInputField` | `REUSE` |

`UI GATE: 3 elemen — REUSE 3, NEW 0`

Pilihan pusat biaya memakai endpoint aset, bukan resource `costCenters` HR yang menuntut `CostCenter : Read`.

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint berkas task | 0/0 |
| `npm run build` (Rizki) | Berhasil `BUILD_ID` 2026-11-24 10:36:55; diulang `BUILD_ID` 10:48:23 sesudah perbaikan 3.1 (bundel memuat `pagination:!1`) |

**MANUAL TEST (Playwright, server `next dev` port 3000 yang dinyalakan Antigravity): 20/20** bersama `FE-AST-011` — skrip dan bukti sama
(`fe-ast-019-20261124/`). Langkah milik task ini:

| Langkah | Hasil |
| --- | --- |
| `AC1` tab Akun Kontrol: ALKMED `1-4001` Aset Tetap – Alat Medis; "6 kelompok aktif belum punya akun kontrol"; lencana Belum diatur | Lulus |
| `AC3` simpan kode `9-9999` → 400; galat tampil di field Harga Perolehan saja, field Akumulasi Penyusutan bersih; toast pesan backend | Lulus |
| Perbaiki ke `1-4001` → 200, toast Akun Kontrol Ditetapkan | Lulus |
| `AC1` tab Pusat Biaya: Rawat Inap Otomatis `CC-MMC-00003` | Lulus |
| Pusat biaya kedua dibuat lewat API → `AC2` saringan Belum jelas: satu baris Rawat Inap "punya 2 pusat biaya aktif", keterangan "1 unit belum jelas" | Lulus |
| Petakan dari modal (pilih `CC-MMC-00006`) → 200, toast, saringan menjadi kosong "Semua unit sudah jelas…"; tanpa saringan baris bersumber Pemetaan | Lulus |
| Hapus pemetaan dari konfirmasi → 200, unit kembali belum jelas | Lulus |
| Tanpa galat halaman dan respons 5xx | Lulus |
| Rapikan: pusat biaya uji dihapus; tidak ada unit belum jelas | Lulus |

### 3.1 Perbaikan sesudah uji

| Temuan pada tangkapan `06`/`07` | Perbaikan | Bukti |
|---|---|---|
| Tabel tanpa halaman (akun kontrol, pusat biaya, riwayat kebijakan) menampilkan "Menampilkan 0 sampai 0 dari 0 data" dan tombol halaman | `pagination={false}` (pola neraca saldo/jurnal berulang) | `cek-perbaikan.mjs` 3/3; tangkapan `11`, `12` |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Bagian 3–4 `SCR-AST-NLI-06` | Tab Akun Kontrol, Pusat Biaya | `AC1` ×2 | ✅ |
| 2. Saringan "belum jelas" untuk unit | `FilterSelect` → `unclearOnly=true` | `AC2` | ✅ |
| 3. Galat kode akun dari backend tampil per baris | `use-asset-subledger-accounts` (kode `017` → field yang kodenya disebut) | `AC3` | ✅ |

| DoD | Status |
|---|---|
| `V-FE` lint, uji layar | Terpenuhi |
| `V-FE` build produksi memuat kode akhir | Terpenuhi (`BUILD_ID` 10:48:23) |
| `UAT-NLI-02` | Milik tim UAT |
| Laporan, roadmap, traceability | Ditulis |
