# Laporan Perubahan Frontend — `FE-AST-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-008` |
| Judul | Dokumen, pengamanan, pemanfaatan, menganggur |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-008` |
| Trace | `CAP-03.5`, `CAP-08.1`–`08.5`; `SCR-AST-REG-14`, `28..32`; `03-frontend-architecture.md` §2, §3.6; `02-module-map.md` §3.1 (grup "Pemanfaatan & Pengamanan Aset") |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-011` termasuk deltanya (laporan `BE-AST-011` bagian 3.3) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-004` ✅, `BE-AST-011` 🟡 (sisa backend hanya pekerja harian dan `AT-REG-15` positif; tidak menghalangi layar) |
| Klasifikasi | `HEAVY` — 11 route baru, 1 aggregate ber-lifecycle dengan 9 aksi, CRUD dokumen dengan berkas, 3 daftar pengamanan; 37 berkas baru, 4 berkas diubah; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-18 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `82f593d3d` (branch `RizkiV2`) |
| Tanggal | 18 November 2026 |
| Status | 🟡 SEBAGIAN 18 November 2026 — ketiga kriteria terpetakan ke source; ESLint 41 berkas 0/0; build Rizki berhasil; uji layar 29/32 + 3 cek ulang lulus (3 kegagalan = pencocokan teks skrip, bagian 6); **belum terbukti**: baris aset menganggur di `SCR-AST-REG-31` (belum ada data, ikut `AT-REG-15` di `BE-AST-011`) |

---

## 1. Keadaan yang ditemukan di awal

- `BE-AST-011` baru menyediakan dokumen kepemilikan, lampiran generik, pemanfaatan, dan `GET /assets/idle`; belum
  ada layar menu 3.5 dan menu 8.
- Opsi master jenis dokumen (`/options`) tidak membawa `hasExpiry`, padahal formulir perlu tahu kapan tanggal
  berakhir wajib. Dipakai daftar master jenis dokumen (sudah memuat `hasExpiry`) sebagai sumber pilihan, seperti
  `assetGroupsDetailed`.
- `ConfirmModal` hanya mengenal alasan wajib; aksi setujui/teruskan tidak meminta catatan.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Petugas Aset | Dokumen Kepemilikan → **+ Tambah Dokumen** | Pilih aset terdaftar dan jenis dokumen; bila jenisnya punya masa berlaku, Tanggal Berakhir menjadi wajib; lampirkan pindaian → kembali ke daftar dengan badge masa berlaku |
| 2 | Petugas Aset | Dokumen Kepemilikan | Menu baris: Lihat aset, Buka berkas, Ubah dokumen (aset terkunci), Hapus dokumen (konfirmasi) |
| 3 | Bagian Aset | Kelengkapan Dokumen Kepemilikan | Aset terdaftar yang dokumen wajibnya belum lengkap: "1 dari 2" + badge jenis dokumen yang belum ada; menu baris Tambah dokumen |
| 4 | Bagian Aset | Pengamanan Administrasi | Dokumen habis atau hampir habis, terdekat di atas: "lewat 1 hari", "27 hari lagi" |
| 5 | Bagian Aset | Pemantauan Aset Menganggur | Aset Tidak Digunakan lebih lama dari batas: sejak kapan, lama menganggur, batas hari |
| 6 | Petugas Aset | Pengajuan Pemanfaatan → **+ Ajukan Pemanfaatan** | Isi perjanjian (jenis, mitra, nomor, nilai Rp, masa) dan centang aset pada tabel → **Simpan Draft** → rincian: "Penyetuju Wajib: Ditentukan saat diajukan" |
| 7 | Petugas Aset | Rincian → **Ajukan** | Penyetuju dihitung backend dan tampil: Kepala Bagian Aset atau Direktur |
| 8 | Kepala Bagian Aset | Pemanfaatan Sesuai Kewenangan, tab **Menunggu Keputusan Anda** | Kolom **Penyetuju Wajib**; untuk tingkat Kepala Bagian Aset menu Setujui/Tolak, untuk tingkat Direktur hanya **Teruskan ke Direktur**/Tolak |
| 9 | Direktur | Rincian | **Setujui (Direktur)** / **Tolak (Direktur)** |
| 10 | Siapa pun berhak | Rincian | Unggah/Lihat Perjanjian, Akhiri Lebih Awal, Batalkan sesuai `availableActions` ∩ hak |

**Jalur tidak normal**

| Kondisi | Perilaku |
|---|---|
| Jenis dokumen ber-masa-berlaku tanpa tanggal berakhir | Klien menolak sebelum kirim: "Dokumen jenis ini wajib punya tanggal berakhir." |
| Aset bukan milik RS / terkunci / hilang (409 `044`/`040`) | Pesan backend di atas tabel pilihan aset; tabel dimuat ulang |
| Aset hilang, rusak berat, atau dikunci proses lain | Kotak centang nonaktif dengan keterangan |
| Swasetuju | Tombol Setujui tidak tampil untuk pengaju |
| Tolak, akhiri, batal | Alasan wajib (tombol konfirmasi nonaktif tanpa alasan) |
| Data basi (409) | Toast pesan backend, daftar/rincian dimuat ulang |
| Tanpa hak `AssetUtilization`/`AssetDocument : Read` | Menu tidak tampil; URL → Akses Ditolak |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; referensi skill (gerbang base component, pola
komposisi, checklist UI); layar `FE-AST-006`/`007` (daftar bervarian, rincian bertoken, modal aksi, pemilih aset
penyerahan); `base-grouped-editor-*`, `filter-date-picker`, `confirm-modal`, `base-detail-view`/`-section`;
`asset-status-labels.js` (enum pemanfaatan sudah ada); kontrak 2.12–2.14, §3.6; source `BE-AST-011`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/register/asset-document-constants.jsx` | Baru — endpoint, route, varian daftar (`documents`, `expiring`), kartu, isian formulir |
| `src/lib/constants/corporate/asset-management/utilization/asset-utilization-constants.jsx` | Baru — endpoint, route, varian (`requests`, `approvals`), tab, aksi→hak, isian, pemilih aset |
| `src/lib/services/corporate/asset-management/asset-document-service.js` | Baru — daftar, `expiring`, `completeness`, summary, CRUD (FormData), unduh lampiran generik |
| `src/lib/services/corporate/asset-management/asset-utilization-service.js` | Baru — semua aksi; `status` berulang lewat `paramsSerializer` |
| `src/lib/services/corporate/asset-management/asset-record-service.js` | + `fetchIdleAssets` |
| `src/utils/corporate/asset-management/register/asset-document-utils.js` | Baru |
| `src/utils/corporate/asset-management/utilization/asset-utilization-utils.js` | Baru — parameter, validasi, payload (enum angka), alasan blokir pilihan aset |
| `src/utils/corporate/asset-management/asset-status-labels.js` | + enum `documentExpiryState` |
| `src/lib/hooks/corporate/asset-management/register/use-asset-document-{route,actions,list,editor}.jsx` | Baru |
| `src/lib/hooks/corporate/asset-management/register/use-asset-security-list.jsx` | Baru — Kelengkapan dan Menganggur (satu hook, dua varian) |
| `src/lib/hooks/corporate/asset-management/utilization/use-asset-utilization-{route,actions,list,detail,editor}.jsx` | Baru |
| `src/components/view/corporate/asset-management/register/asset-document-{list-view,form-view,table-columns}.jsx` | Baru |
| `src/components/view/corporate/asset-management/register/asset-security-list-view.jsx` | Baru |
| `src/components/view/corporate/asset-management/utilization/asset-utilization-{list-view,detail-view,form-view,action-modals,table-columns}.jsx` | Baru |
| `src/components/view/corporate/asset-management/placement/asset-placement-table-columns.jsx` | `buildHandoverPickerColumns` + prop opsional `readyLabel`, `selectAllLabel`, `extraColumns` (bawaan tidak berubah) |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | + resource `assetDocumentTypesDetailed` (membawa `hasExpiry`) |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir "Dokumen Kepemilikan" di Registrasi & Inventaris; grup baru "Pemanfaatan & Pengamanan Aset" (5 butir) |
| `src/app/corporate/asset-management/register/documents/{,create,[slug]/update}/page.jsx` | Baru — 3 route |
| `src/app/corporate/asset-management/security/{documents,administration,idle}/page.jsx` | Baru — 3 route |
| `src/app/corporate/asset-management/utilization/{requests,requests/create,requests/[slug],requests/[slug]/update,approvals}/page.jsx` | Baru — 5 route |

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. Tanpa slice baru.
- Pengamanan Administrasi memakai view daftar dokumen bervarian; Kelengkapan dan Menganggur satu hook; Pengajuan
  dan Antrean satu view bervarian.
- Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah.

`UI GATE: 15 elemen — REUSE 12, EXTEND 1, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kepala layar, kartu, tab | `Hero`, `SummaryGrid`, `DataFilter tabs` | dipakai `FE-AST-006`/`007` | REUSE | — |
| Saringan | `FilterSelect`, `AssetMasterResourceFilter` | dipakai `FE-AST-005`..`007` | REUSE | — |
| Tabel, aksi baris, badge | `DataTable`, `RowActionMenu`, `StatusBadge` | dipakai `FE-AST-003`..`007` | REUSE | — |
| Formulir dokumen dan perjanjian | `BaseGroupedEditorView` (teks, select remote, tanggal, berkas, rupiah) | `FE-AST-003`/`007` | REUSE | — |
| Rincian + tabel aset | `BaseDetailView` + `BaseDetailSection` | `FE-AST-006` | REUSE | — |
| Konfirmasi beralasan | `ConfirmModal requireReason` | `FE-AST-003` | REUSE | — |
| Unggah perjanjian di modal | `ConfirmModal children` + `BaseGroupedEditorField` berkas | `FE-AST-007` | REUSE | — |
| Buka PDF/berkas di tab baru | `use-asset-pdf-tab` | `FE-AST-006` | REUSE | — |
| Pesan, toast, akses ditolak | `InformationAlert`, `ToastStack`, `AccessDeniedGate` | semua layar Aset | REUSE | — |
| Pilihan jenis dokumen + `hasExpiry` | `useSelectResource` + resource baru | pola `assetGroupsDetailed` | REUSE | — |
| Pilih aset yang dimanfaatkan | `buildHandoverPickerColumns` (`FE-AST-006`) | kolom pilih, kotak centang, alasan blokir | EXTEND | A (dipilih): tiga prop opsional (label siap, label pilih semua, kolom tambahan Kepemilikan), bawaan tak berubah — satu tabel pemilih untuk dua alur. B: salin kolom pemilih — duplikasi 60 baris. Bukan base component |
| Formulir + tabel pemilih di satu halaman | `BaseGroupedEditorView afterForm` | `FE-AST-006` | COMPOSE | A (dipilih): pola Serahkan Aset |
| Tombol aksi rincian | `BaseButton` dirangkai dari `availableActions` | — | COMPOSE | A (dipilih): daftar tombol berkonfigurasi |

Catatan: catatan opsional pada Setujui/Teruskan tidak ditampilkan karena `ConfirmModal` belum punya alasan
opsional; backend menerima tanpa alasan. Menambah prop itu = `EXTEND` base component dan butuh keputusan Rizki.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable`/kartu memuat; saringan nonaktif; formulir ubah menunggu data |
| Kosong | Kalimat per layar, misalnya "Tidak ada aset menganggur pada saringan ini.", "Semua aset pada saringan ini sudah memiliki dokumen wajibnya." |
| Gagal / 403 | `InformationAlert`; `AccessDeniedGate` |
| Draft bukan milik pengguna / sudah diajukan | Formulir ubah menampilkan "Pemanfaatan ini tidak dapat diubah lagi…" dan tombol simpan nonaktif |
| Aksi berjalan | Tombol ber-`loading`; tombol rincian nonaktif selama muat ulang |

---

## 5. Endpoint yang dikonsumsi

| Layar | Endpoint |
|---|---|
| Dokumen Kepemilikan (`SCR-AST-REG-14`) | `GET /register/documents`, `/summary`; `POST`/`PUT` multipart; `DELETE /{id}`; `GET /{id}`; `GET /register/attachments/{id}` |
| Kelengkapan (`29`) | `GET /register/documents/completeness` |
| Pengamanan Administrasi (`30`) | `GET /register/documents/expiring`, `/summary` |
| Menganggur (`31`) | `GET /register/assets/idle` |
| Pengajuan (`28`) | `GET /register/utilizations`, `/summary`, `/{id}`; `POST`, `PUT /{id}`; `/{id}/agreement`, `/submit`, `/approve`, `/approve-director`, `/reject`, `/end`, `/cancel`; `GET /register/assets?recordStatus=Terdaftar` (pemilih) |
| Antrean (`32`) | `GET /register/utilizations?status=2&status=3&awaitingMyDecision=` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 41 berkas | `0 errors, 0 warnings` (satu peringatan React Compiler `preserve-manual-memoization` diperbaiki) | `PASS` | Keluaran perintah |
| Grep komentar baru dan inline style | Kosong | `PASS` | — |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-18 10:35:51; manifest memuat 11 route baru | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-008-20261118/uji-fe-008.mjs`) | **29/32**; 0 galat halaman, 0 respons 5xx untuk superadmin (46 panggilan), penyetuju (10), LOKET (5) | `PASS` setelah cek ulang | `hasil-uji.jsonl`, 10 tangkapan layar |
| Cek ulang baca-saja (`cek-ulang-teks.mjs`) | 3/3 | `PASS` | `hasil-uji.jsonl` (3 baris akhir) |

Tiga kegagalan putaran pertama (`SCR-14-KARTU-ANGKA-BACKEND`, `SCR-28-DAFTAR`, `SCR-32-ANTREAN`) berasal dari
skrip yang mencocokkan judul kartu dan kepala kolom dengan huruf kecil, padahal tema menampilkannya huruf besar
("PENYETUJU WAJIB", "MENUNGGU KEPUTUSAN ANDA"; terlihat di `08-antrean-kewenangan.png`). Cek ulang tanpa peka huruf
besar: kartu dokumen "3 / 1 / 1 / 0" = `GET /documents/summary`; daftar pengajuan memuat sewa ATM Berjalan; kepala
kolom antrean memuat "Penyetuju Wajib".

Langkah yang lulus (ringkas): menu grup + 5 butir; daftar dokumen dan badge; tanggal berakhir wajib untuk Polis;
tambah dengan berkas; aksi baris; ubah (aset terkunci); hapus; Pengamanan Administrasi hanya Habis/Hampir Habis
("lewat 1 hari", "27 hari lagi"); Kelengkapan "1 dari 2" + badge Faktur Pembelian (syarat kategori Alat Medis
dipasang sementara lalu dipulihkan); Menganggur kosong; draft pemanfaatan dengan pemilih aset (`AST-PMF-000009`,
Rp 15.000.000, 3 bulan); Ajukan → Kepala Bagian Aset, pengaju tanpa tombol Setujui; KSO 25 bulan (`AST-PMF-000010`)
→ Direktur; antrean penyetuju: tingkat Direktur hanya Teruskan/Tolak, tingkat Kepala Bagian Aset Setujui/Tolak;
setujui → Berjalan; teruskan → Menunggu Direktur → Setujui (Direktur) → Disetujui; unggah dan buka perjanjian
(PDF di tab baru); LOKET Akses Ditolak dan menu tidak tampil. Policy penyetuju dipulihkan sama persis.

**Belum terbukti:** baris aset menganggur di `SCR-AST-REG-31` (kolom "Lama Menganggur" dan batas dari backend).
Belum ada aset Tidak Digunakan lebih dari batas hari; ditutup bersama `AT-REG-15` di `BE-AST-011` mulai 20 Nov.

**Temuan tampilan (bukan cacat):** pada lebar 1600 px tabel antrean bergulir ke samping, sehingga badge kedua di kolom Status
("Menunggu Anda") terpotong sampai digulir.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
|---|---|---|
| 1. `SCR-AST-REG-14`, `28..32` | 11 route; uji layar per layar | ✅ (28–30, 32, 14); 31 tampil dengan keadaan kosong |
| 2. Antrean persetujuan menampilkan penyetuju yang diwajibkan | Kolom "Penyetuju Wajib", aksi sesuai tingkat | ✅ |
| 3. Dokumen hampir habis dan aset menganggur memakai angka backend | `daysToExpiry`/`expiryState`/kartu dari backend ✅; `idleDays`/`idleThresholdDays` belum terlihat dengan data | 🟡 |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi kecuali satu butir di atas |
| Commit | Belum |
