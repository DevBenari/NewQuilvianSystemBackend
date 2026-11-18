# Laporan Perubahan Frontend — `FE-AST-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-001` |
| Judul | Fondasi frontend dan grup menu |
| Slice | `REG-FE-S1` — Fondasi dan pencatatan |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-001` |
| Trace | `AST-FE-002`, `AST-OQ-015`, `02-module-map.md` §3.1, `03-frontend-architecture.md` §2 dan §5 |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`); delta sumber backend dicatat di laporan `BE-AST-006` §3.3 |
| Wewenang UI | Nama dan urutan butir menu `approved` (`AST-FE-002`); letak grup menu `open` (`AST-OQ-015`, tidak memblokir menurut `04-prd-to-mvp.md`); rupa `DEV_DISCRETION` (`AST-FE-005`) |
| Dependency | `BE-AST-006` ✅ 16 November 2026. Kriteria 3 (pilih-aset) bergantung pada `BE-AST-007` yang **belum dikerjakan** |
| Klasifikasi | `MEDIUM` — menu, berkas label enum, konstanta endpoint, registry pilihan |
| Task mode | `FRONTEND`, diminta Rizki 16 November 2026 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`), plus laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `605d26dc3` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `2093cb9a` (branch `rizkiG`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 SEBAGIAN — kriteria 1 dan 2 terpenuhi; kriteria 3 terblokir `BE-AST-007` |

---

## 1. Keadaan yang ditemukan di awal

- Branch `RizkiV2` belum memuat kode Aset sama sekali: tidak ada route `src/app/corporate/asset-management/`,
  tidak ada grup menu "Manajemen Aset" pada `src/utils/menu-sidebar/menu-items.jsx`.
- Backend `BE-AST-006` sudah menyediakan 58 endpoint master. Endpoint daftar aset
  `GET /register/assets` (API 2.8) masih **Rencana** — `AssetsController` belum ada di source
  backend `2093cb9a`.
- Seluruh enum sub-modul `register` ada di `Areas/Corporate/AssetManagement/Register/Enums/AssetRegisterEnums.cs`
  dan diserialisasi sebagai **angka** (tidak ada `JsonStringEnumConverter` di backend).
- Repository frontend **tidak** memiliki folder `src/services/` seperti yang ditulis kartu; pola
  yang hidup adalah konstanta endpoint + Redux slice lewat `InstanceAxios`.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas yang memiliki hak baca master aset membuka sidebar Corporate dan melihat grup
   **Manajemen Aset** tepat di bawah **Akuntansi**.
2. Di dalamnya ada grup **Master Klasifikasi Aset** berisi delapan butir dengan urutan persis
   `02-module-map.md` §3.1: Kategori Aset, Kelompok Aset, Kode Inventaris, Jenis Kepemilikan,
   Lokasi, Klasifikasi Aset Medis & Non-Medis, Jenis Dokumen, Pengaturan Aset.
3. Butir hanya tampil bila pengguna memiliki hak `Read` atas resource-nya (misalnya
   `AssetCategory : Read`). Pengguna tanpa hak satu pun tidak melihat grup Manajemen Aset sama sekali.

Jalur tidak normal: pengguna yang mengetik URL langsung tanpa hak mendapat layar
"Ups! Akses Ditolak" dari `AccessDeniedGate` (dibuktikan dengan akun LOKET, lihat bagian 6).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md`, `rules/frontend/*` (arsitektur, katalog, gerbang keputusan, pola halaman,
standar master data, design token, checklist), `02-module-map.md`, `03-frontend-architecture.md`,
`02-backend-architecture.md` §5.4, `contracts/api-contract.md` 2.1–2.8, laporan `BE-AST-006`,
`AssetRegisterEnums.cs`, `menu-items.jsx`, `select-resource-registry.js`,
`finance/finance-select-resources.js`, `master-data-resource-slice-factory.jsx`, modul referensi
`corporate/accounting`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Grup tingkat 0 "Manajemen Aset" (`subMenu`) → grup "Master Klasifikasi Aset" (`subItems`) → 8 daun `pathname`, masing-masing ber-`requiredPermission` `Read`; impor ikon `RiArchiveStackLine`, `RiBarcodeLine`, `RiFileShieldLine` |
| `src/utils/corporate/asset-management/asset-status-labels.js` | Baru. Label Bahasa Indonesia untuk **14 enum** `register` di satu berkas, beserta nada `StatusBadge`; fungsi `getAssetEnumLabel`, `getAssetEnumTone`, `getAssetEnumOptions`, `findAssetEnumEntry` menerima nilai angka maupun nama enum |
| `src/lib/constants/corporate/asset-management/asset-management-constants.jsx` | Baru. Base URL API dan route modul, scope token route privat, pemetaan kode galat `AST-VAL-REG-050..055` ke isian, default filter, field dan kolom bersama |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | Baru. Registry pilihan domain Aset (`assetCategories`, `assetClassifications`) memakai `/options` |
| `src/lib/hooks/select/select-resource-registry.js` | Mendaftarkan registry domain Aset (tambahan murni, pola sama dengan Finance) |

### 3.3 Kepatuhan arsitektur frontend

- "Service klien per grup endpoint" diwujudkan sebagai **konstanta endpoint** + slice factory
  `createMasterDataResourceSlice` (dikerjakan bersama `FE-AST-002`), karena repository tidak memiliki
  `src/services/` dan aturan `frontend-architecture.md` menempatkan request di `src/lib/state`.
- Jalur label enum memakai konvensi domain `src/utils/corporate/asset-management/` (sejajar
  `src/utils/corporate/accounting/`), bukan `src/utils/asset-management/` seperti teks kartu.
- Nol baris komentar pada seluruh berkas baru dan pada baris yang ditambahkan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sidebar menunggu daftar hak akses sesi (`useEffectivePermissions`), lalu menampilkan butir yang diizinkan |
| Kosong | Tanpa hak `Read` satu pun, grup Manajemen Aset tidak dirender |
| Gagal | NOT APPLICABLE — menu statis; kegagalan daftar hak ditangani `permission-slice` yang sudah ada |
| Tanpa hak akses | URL langsung → "Ups! Akses Ditolak — Anda tidak memiliki akses ke menu atau fitur ini." |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Master Data / Asset Category

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/asset-management/master-data/categories/options` | Registry pilihan `assetCategories` | `AssetCategory : Read` |

#### Corporate / Asset Management / Master Data / Asset Classification

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/asset-management/master-data/classifications/options` | Registry pilihan `assetClassifications` | `AssetClassification : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 39 berkas yang diubah/dibuat `FE-AST-001` + `FE-AST-002` | 0 error, 0 warning | `PASS` | Keluaran perintah, exit 0 |
| Pemeriksaan statis import `@/` dan named export (skrip Node) | 38 berkas, 0 masalah | `PASS` | Keluaran skrip; satu temuan lama di `store.jsx` (`learning-development-slice` tanpa default export) bukan dari task ini |
| Grep baris komentar pada berkas baru | 0 baris | `PASS` | Keluaran grep |
| SuperAdmin: grup Manajemen Aset → Master Klasifikasi Aset tampil dengan 8 butir berurutan | Sesuai §3.1 | `PASS` | `test-with-agy/asset-management/fe-ast-002-20261116/list-*.png` |
| LOKET (tanpa izin Aset): grup Manajemen Aset tidak tampil; URL langsung → Akses Ditolak | Sesuai | `PASS` | `hak-akses-loket.png`, keluaran `uji-hak-akses.mjs` |
| `npm run build` (Rizki) | Berhasil | `PASS` | `npm run build` dijalankan Rizki 16 November 2026 — berhasil (`BUILD_ID` 14.20.20, `postbuild` standalone selesai; manifest memuat route `master-data/[feature]`, `[feature]/create`, `[feature]/[slug]`, `[feature]/[slug]/update`, `settings`) |

Uji manual: `PASS` untuk kriteria 1 dan 2; `NOT FEASIBLE` untuk kriteria 3 (endpoint belum ada).

**Tidak dijalankan:** unit test — tidak diwajibkan dan tidak ditulis atas keputusan Rizki
(`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Grup tingkat 0 memakai `subMenu`, grup tingkat 1 memakai `subItems`, daun `pathname` (`getChildrenFromItem`) | Terpenuhi | `menu-items.jsx` key `corporateAssetManagement` → `corporateAssetMasterData` → 8 daun |
| 2. Label seluruh enum `register` dalam Bahasa Indonesia di satu berkas | Terpenuhi | `asset-status-labels.js` — 14 enum, sama persis dengan `AssetRegisterEnums.cs` |
| 3. Pilih-aset menyaring aset terkunci/hilang/rusak berat sesuai parameter | **Belum terpenuhi** — ⛔ menunggu `BE-AST-007` | `GET /register/assets` belum ada di backend; membangun komponen terhadap payload yang belum ada dilarang (`build-module-frontend` langkah 4) |
| DoD: `V-FE` dicatat apa adanya | Terpenuhi | Lint `PASS`, build `PASS` (Rizki) |
| DoD: laporan + roadmap + traceability | Terpenuhi | Berkas ini, kartu roadmap, baris traceability |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | NONE |
| Masalah yang diketahui | Letak grup menu masih sementara (di bawah Akuntansi) sampai `AST-OQ-015` diputuskan Rizki. Butir "Dashboard Aset" sengaja belum didaftarkan karena layarnya milik `FE-AST-010` |
| Dependency backend | `BE-AST-007` — pilih-aset (`asset-picker.jsx`) baru dapat dibangun setelah `GET /register/assets` beserta `AssetPagedQuery` ada di source |
| Perubahan sampingan | NONE |
| Interupsi | NONE |
| Status Git | Lihat laporan `FE-AST-002` bagian 8 (satu working tree yang sama) |
| Langkah berikutnya | Setelah `BE-AST-007` selesai, kerjakan ulang kartu ini untuk kriteria 3 (pilih-aset) |
