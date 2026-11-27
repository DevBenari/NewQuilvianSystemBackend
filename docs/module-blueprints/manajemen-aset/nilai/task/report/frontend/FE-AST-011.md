# Laporan Perubahan Frontend — `FE-AST-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-011` |
| Judul | Parameter nilai dan kebijakan kelompok |
| Gelombang | `MVP-0` — sub-modul `nilai` |
| Roadmap | `nilai/roadmap/frontend-roadmap.md` — kartu `FE-AST-011` |
| Trace | `CAP-EXT-02`, `AST-DEC-027`; `SCR-AST-NLI-06` bagian 1–2; `03-frontend-architecture.md` §3.3 |
| Contract version | `AST-NLI-1.0.0`; layar mengikuti **source** `BE-AST-015` (`GET /parameters/current`, `GET /parameters`, `POST /parameters`, `GET /depreciation-policies`, `GET /{groupId}/history`, `POST /depreciation-policies`) |
| Wewenang UI | Tata letak `DEV_DISCRETION`; satu layar empat tab (bagian 3–4 = `FE-AST-019`) |
| Dependency | `FE-AST-001` [FE-REG] 🟡 (fondasi dipakai, tidak diubah), `BE-AST-015` 🟡 (source lengkap untuk 2.1–2.2) |
| Klasifikasi | `STANDARD` — 1 route, 1 view + kolom, 3 hook, service, konstanta, utilitas, grup menu baru |
| Task mode | `FRONTEND` — perintah Rizki 2026-11-24 "lanjutkan" |
| Target tulis | `QuilvianSystemFrontendDev/src` (aset `valuation/*`, `menu-items.jsx`); laporan ini dan baris status |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `3b59685d8` (branch `RizkiV2`) |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar (Playwright **20/20** bersama `FE-AST-019` + cek perbaikan **3/3**); build Rizki `BUILD_ID` 10:48:23 memuat perbaikan terakhir. **Riwayat:** 🟡 pada hari yang sama karena `BUILD_ID` 10:36 mendahului perbaikan

---

## 1. Proses bisnis dari sisi pengguna

Menu **Nilai Aset & Rekonsiliasi → Parameter Nilai** (`/corporate/asset-management/valuation/parameters`, `?tab=`).

| Tab | Yang dilihat / dilakukan |
|---|---|
| **Parameter Nilai** (`AssetValuationParameter : Read`) | Kartu: berlaku sejak, batas kapitalisasi, batas penghapusan, batas pemanfaatan. Keterangan "Parameter baru berlaku mulai {bulan}: …" bila ada yang akan berlaku. Riwayat bertanda **Berlaku** / **Akan Berlaku**. Tombol **Tetapkan Parameter** (`Update`): modal tiga batas rupiah + dasar penetapan, dengan keterangan "Berlaku mulai {bulan}"; toast "Berlaku mulai {bulan}" sesudah simpan; penolakan backend (mis. bulan itu sudah punya parameter) tampil sebagai toast dan modal tetap terbuka |
| **Kebijakan Kelompok** (`AssetDepreciationPolicy : Read`) | Kelompok yang disusutkan: berlaku, akan berlaku, status kelompok; cari kode/nama. Aksi baris **Tetapkan kebijakan** (`Update`): umur manfaat (1–600 bulan) dan residu (0–100%, koma diterima), keterangan "Berlaku mulai {bulan}"; **Riwayat kebijakan** |

Tab hanya tampil bila pengguna berhak membacanya; tanpa satu pun hak, halaman menampilkan Akses Ditolak.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/asset-management/valuation/parameters/page.jsx` | Baru — route + `?tab=` |
| `src/components/view/corporate/asset-management/valuation/asset-valuation-settings-view.jsx` | Baru — hero, tab, isi tab, modal |
| `src/components/view/corporate/asset-management/valuation/asset-valuation-table-columns.jsx` | Baru — kolom riwayat parameter, kebijakan, riwayat kebijakan |
| `src/lib/hooks/corporate/asset-management/valuation/use-asset-valuation-settings.jsx` | Baru — tab per hak, toast, galat akses |
| `.../valuation/use-asset-valuation-parameter.jsx`, `use-asset-depreciation-policies.jsx` | Baru — muat, simpan, validasi di layar |
| `src/lib/services/corporate/asset-management/asset-valuation-service.js` | Baru |
| `src/lib/constants/corporate/asset-management/valuation/asset-valuation-constants.jsx` | Baru |
| `src/utils/corporate/asset-management/valuation/asset-valuation-utils.js` | Baru — format bulan/persen, form, validasi, payload |
| `src/utils/menu-sidebar/menu-items.jsx` | + grup **Nilai Aset & Rekonsiliasi** (`RiCalculatorLine`) dengan butir **Parameter Nilai** (`AssetValuationParameter : Read`) |

Tanpa baris komentar, tanpa CSS baru, tanpa paket baru.

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Hero, tab + cari, tabel, paginasi, toast, gerbang akses | `Hero`, `DataFilter`, `DataTable`, `Pagination`, `ToastStack`, `AccessDeniedGate` | `REUSE` |
| Kartu parameter, keterangan | `SummaryCards`, `InformationAlert` | `REUSE` |
| Form dalam modal, riwayat dalam modal | `ConfirmModal` + `BaseInputField`/`BaseTextAreaField`; `ConfirmModal` `hideCancel` + `DataTable` | `REUSE` |
| Input rupiah | `BaseInputField` + `normalizeWholeRupiahInput`/`formatThousandsInput` (pola harga perolehan register) | `REUSE` |
| Lencana, aksi baris | `StatusBadge`, `RowActionMenu` | `REUSE` |

`UI GATE: 5 elemen — REUSE 5, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0`

Bagian 1–4 disatukan dalam satu layar bertab karena `03-frontend-architecture.md` §3.3 menulis satu layar empat bagian.

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint berkas task (`--max-warnings=0`) | 0/0 (sesudah perbaikan nama properti `current` → `snapshot` yang dikira ref oleh React Compiler) |
| `npm run build` (Rizki) | Berhasil `BUILD_ID` 2026-11-24 10:36:55, manifest memuat `valuation/parameters`; diulang `BUILD_ID` 10:48:23 sesudah perbaikan 3.1 (bundel memuat "Batas Penghapusan", tanpa "Batas Direktur") |
| Grep anti-regresi | Tanpa CSS baru, tanpa warna/ukuran literal, tanpa komentar |

**MANUAL TEST (Playwright, Edge, 1600×900, server `next dev` port 3000 (dinyalakan pukul 10:37 oleh Antigravity, bukan agent ini)): 20/20** bersama `FE-AST-019` —
`QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-019-20261124/uji-fe-011-019.mjs`, `hasil-uji.jsonl`,
`jaringan-api.json`, tangkapan `01..09-*.png` (diperiksa langsung). Langkah milik task ini:

| Langkah | Hasil |
| --- | --- |
| `AC3` grup menu Nilai Aset & Rekonsiliasi + butir Parameter Nilai | Lulus |
| `AC1` kartu = parameter berlaku (November 2026, Rp 1.000.000 / Rp 50.000.000 / Rp 50.000.000); keterangan akan berlaku Desember 2026; riwayat 2 baris = API | Lulus |
| `AC2` modal Tetapkan Parameter menyebut "Berlaku mulai Desember 2026" | Lulus |
| Simpan parameter Desember (sudah ada) → 409, toast "Sudah ada parameter…", modal tetap terbuka | Lulus |
| Tab Kebijakan: 6 kelompok, ALKMED berlaku + akan berlaku, lainnya "Belum Ada" | Lulus |
| Riwayat kebijakan ALKMED: 2 baris, 60 bulan | Lulus |
| Umur 0 → galat di layar, tanpa permintaan ke server | Lulus |
| `AC2` Tetapkan kebijakan **TI** 48 bulan residu 0% → 200, modal dan toast "TI: berlaku mulai November 2026", baris 48 bulan | Lulus |
| `AC3` akun tanpa hak (penyetuju aset): menu tidak tampil, halaman Akses Ditolak | Lulus |
| Tanpa galat halaman dan respons 5xx | Lulus |

**Data yang ditulis (permanen, disetujui Rizki 2026-11-24):** kebijakan kelompok TI 48 bulan, residu 0%, berlaku
November 2026 — sekaligus bahan skenario `AT-NLI-07` (48 → 60 bulan) di `BE-AST-027`.

### 3.1 Perbaikan sesudah uji

| Temuan pada tangkapan `01` | Perbaikan | Bukti |
|---|---|---|
| Judul kartu "Batas Direktur · Penghapusan/Pemanfaatan" terpotong di tengah kata | Judul dipendekkan menjadi "Batas Penghapusan" / "Batas Pemanfaatan" (penjelasan Direktur ada di modal) | `cek-perbaikan.mjs` 3/3 pada server dev Rizki; tangkapan `10` |

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Parameter berlaku, yang akan berlaku, dan riwayat | `use-asset-valuation-parameter`, tab Parameter | `AC1` | ✅ |
| 2. Setiap simpan menampilkan "Berlaku mulai {bulan}" | Modal parameter/kebijakan + toast | `AC2` ×2 | ✅ |
| 3. Grup menu dan butir terdaftar, tersembunyi tanpa hak | `menu-items.jsx` | `AC3` ×2 | ✅ |

| DoD | Status |
|---|---|
| `V-FE` lint, uji layar | Terpenuhi |
| `V-FE` build produksi memuat kode akhir | Terpenuhi (`BUILD_ID` 10:48:23) |
| `UAT-NLI-01` | Milik tim UAT |
| Laporan, roadmap, traceability | Ditulis |

---

## 5. Catatan

- Pada tab Parameter, panel saringan hanya berisi tombol muat ulang (tab ini tanpa pencarian).
- Agent tidak menyalakan frontend; server uji adalah `next dev` di port 3000 yang dinyalakan Antigravity pukul 10:37 (ditelusuri dari rantai proses).
