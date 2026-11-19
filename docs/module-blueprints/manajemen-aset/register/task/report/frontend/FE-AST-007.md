# Laporan Perubahan Frontend — `FE-AST-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-007` |
| Judul | Kondisi dan status |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-007` |
| Trace | `CAP-06.1`–`06.6`; `AST-DEC-017`, `AST-RULE-010..012`; `SCR-AST-REG-22..27`; `03-frontend-architecture.md` §3.1, §3.6; `02-module-map.md` §3.1 (grup "Kondisi & Status Aset") |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-010` (`0adf1dec`), termasuk delta yang tercatat di laporannya |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian; sumber data `SCR-AST-REG-27` diputuskan Rizki 2026-11-18 (bagian 1) |
| Dependency | `FE-AST-004` ✅, `BE-AST-010` ✅ |
| Klasifikasi | `HEAVY` — 8 route baru (6 butir menu, formulir, rincian), 1 aggregate ber-lifecycle dengan 3 aksi + berita acara; 22 berkas baru, 13 berkas diubah; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-18 ("commit semua nyaa, lalu lanjutkan FE-AST-007") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7fe254219` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `0adf1dec` (branch `rizkiG`) |
| Tanggal | 18 November 2026 |
| Status | ✅ SELESAI 18 November 2026 — ketiga kriteria terpetakan ke source; uji layar Playwright 27 langkah lulus dengan empat aktor; ESLint 35 berkas `0 errors, 0 warnings`; build Rizki berhasil |

---

## 1. Keadaan yang ditemukan di awal

- `BE-AST-010` menyediakan laporan kondisi (lapor berfoto, tetapkan/tolak, setujui hilang, berita acara), tetapi
  belum ada layar. Kondisi aset tidak dapat diubah dari aplikasi.
- `SCR-AST-REG-23..26` (Aset Aktif, Rusak Ringan/Berat, Tidak Digunakan, Hilang) cukup memakai
  `GET /register/assets?condition=…`, yang sudah dipakai Daftar Inventaris (`FE-AST-004`). Backend hanya menerima
  satu nilai `condition`, sehingga layar Rusak Ringan/Berat memakai pilihan tingkat kerusakan, bukan dua nilai
  sekaligus.
- `SCR-AST-REG-27` (Riwayat Perubahan Status) menurut kontrak membaca `GET /register/reports/history?eventType=KondisiBerubah`
  (API 2.17), padahal endpoint itu milik `BE-AST-013` yang belum dikerjakan, sedangkan kartu `FE-AST-007` hanya
  bergantung pada `BE-AST-010`. Pilihan diajukan ke Rizki; ia memilih **memakai laporan kondisi yang sudah
  ditetapkan** (`GET /condition-reports?status=Ditetapkan`). Karena saat ini kondisi aset hanya berubah lewat
  laporan kondisi, isinya sama. Dicatat sebagai delta; layar dapat dialihkan ke `/reports/history` saat
  `BE-AST-013` selesai.
- `UploadPhotoField.jsx` (`src/components/features/UplodFoto/`) mensyaratkan objek react-hook-form dan belum
  dipakai modul mana pun; formulir Quilvian lain memakai state sendiri.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Penanggung jawab / Kepala Unit | Aset Aktif (atau daftar kondisi lain) | Mencari aset → menu baris **Laporkan kondisi** → formulir terbuka dengan aset sudah terisi ("AST-MBL-000035 · Kursi Tunggu …") |
| 2 | Pelapor | Laporkan Kondisi Aset | Memilih kondisi, menulis keterangan, mengambil foto lewat **Kamera** atau memilih berkas → **Kirim Laporan** → rincian `AST-KND-######` berstatus Dilaporkan; "Kondisi aset belum berubah" |
| 3 | Petugas Aset | Pemeriksaan Kondisi | Kartu dan tab **Menunggu Penetapan (n)** → menu baris **Tetapkan kondisi** → modal dengan pilihan kondisi (bawaan = yang dilaporkan) → kondisi aset berubah |
| 3b | Petugas Aset | Pemeriksaan Kondisi / rincian | **Tolak laporan** beralasan → kondisi tidak berubah |
| 4 | Unit | Laporkan Kondisi Aset | Kondisi **Hilang** memunculkan isian Berita Acara Kehilangan (opsional saat melapor) |
| 5 | Kepala Bagian Aset | Tab **Menunggu Persetujuan Kehilangan** → rincian | **Setujui Hilang**: tombol konfirmasi nonaktif sampai berita acara dilampirkan (kecuali unit sudah melampirkannya) → kondisi Hilang → **Cetak Berita Acara Kehilangan** |
| 6 | Unit | Aset Hilang → Laporkan kondisi | Formulir memberi tahu "laporan dikirim sebagai ditemukan kembali"; pilihan Hilang disembunyikan |
| 7 | Kepala Bagian Aset | Pemeriksaan Kondisi | **Setujui ditemukan kembali** dengan pilihan kondisi → aset kembali |
| 8 | Siapa pun berhak baca | Riwayat Perubahan Status | Setiap penetapan: waktu dan penetap, aset, "Aktif → Rusak Berat", sumber, nomor laporan, alasan |

Contoh dari uji: Dimas melaporkan `AST-MBL-000035` Rusak Berat (`AST-KND-000007`); superadmin menetapkannya
→ kursi pindah ke daftar Rusak Ringan/Berat (pilihan Rusak Berat). Dimas melaporkan `AST-MBL-000036` Hilang tanpa
berita acara; superadmin membuka modal Setujui Hilang (tombol nonaktif), melampirkan `ba-kehilangan-satpam.pdf`,
menyetujui, dan mencetak berita acara. Kursi muncul di daftar Hilang; Dimas melaporkannya ditemukan kembali, dan
superadmin menyetujui Aktif.

**Jalur tidak normal**

| Kondisi | Perilaku |
|---|---|
| Laporan unit tanpa foto | Klien menolak sebelum kirim: "Lampirkan foto kondisi aset." (tidak ada `POST`) |
| Aset masih punya laporan terbuka (`409 035`) | Pesan backend tampil dan isian Aset ditandai |
| Sumber Pemeriksaan Bagian Aset tanpa hak `Decide` | Isian sumber disembunyikan; laporan selalu Laporan Unit (backend tetap menolak `403 AST-CND-SOURCE`) |
| Setujui Hilang tanpa berita acara | Tombol konfirmasi modal nonaktif |
| Tolak tanpa alasan | Tombol konfirmasi modal nonaktif |
| Laporan sudah diputuskan orang lain / versi basi (`409`) | Toast pesan backend, daftar/rincian dimuat ulang |
| Bukan pemegang `Decide`/`ApproveLoss` | Aksi tidak tampil (mengikuti `availableActions` dan hak) |
| Tanpa `AssetCondition : Read` | Pemeriksaan Kondisi dan Riwayat tidak tampil di menu; URL → Akses Ditolak |
| Tanpa `AssetCondition : Report` | Tombol **+ Laporkan Kondisi** dan aksi baris **Laporkan kondisi** tersembunyi; `/create` → Akses Ditolak |
| Tab baru diblokir | Berita acara/lampiran diunduh dengan pesan penjelasan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md` dan referensi skill (gerbang keputusan base
component, pola komposisi halaman, checklist UI); layar `FE-AST-004`..`006` (Daftar Inventaris bervarian,
`use-asset-pdf-tab`, rute bertoken, modal aksi); `UploadPhotoField.jsx`; base `base-grouped-editor-*`
(`renderField`, field berkas `accept`/`fileHint`, `hidden`), `confirm-modal` (`children`, `confirmProps`,
`requireReason`), `data-filter`, `summary-grid`, `row-action-menu`, `base-detail-view`; `use-select-resource`
(`extendOption`); interseptor FormData di `InstanceAxios`; kontrak 2.11, 2.17, §3.6, `02-module-map` §3.1;
source `BE-AST-010` dan laporannya (enum JSON wajib angka).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/condition/asset-condition-constants.jsx` | Baru — endpoint, route, tab, kartu, varian daftar (`reports`, `history`), isian formulir |
| `src/lib/services/corporate/asset-management/asset-condition-service.js` | Baru — daftar, ringkasan, rincian, lapor (FormData), tetapkan/tolak, setujui hilang (FormData), lampiran dan berita acara (blob) |
| `src/utils/corporate/asset-management/condition/asset-condition-utils.js` | Baru — parameter, validasi, FormData/payload (enum sebagai angka), label perubahan |
| `src/lib/hooks/corporate/asset-management/condition/use-asset-condition-route.jsx` | Baru — rute rincian bertoken; formulir dengan aset terisi (`?aset=`) |
| `src/lib/hooks/corporate/asset-management/condition/use-asset-condition-actions.jsx` | Baru — tetapkan, tolak, setujui hilang/ditemukan kembali, berita acara, lampiran; satu sumber untuk daftar dan rincian |
| `src/lib/hooks/corporate/asset-management/condition/use-asset-condition-report-list.jsx` | Baru — `SCR-AST-REG-22` dan `27` (satu hook, dua varian) |
| `src/lib/hooks/corporate/asset-management/condition/use-asset-condition-report-detail.jsx` | Baru |
| `src/lib/hooks/corporate/asset-management/condition/use-asset-condition-report-editor.jsx` | Baru — formulir; isian berubah menurut hak, kondisi aset, dan kondisi yang dilaporkan |
| `src/components/view/corporate/asset-management/condition/asset-condition-photo-field.jsx` | Baru — pembungkus `UploadPhotoField` (react-hook-form lokal ↔ state formulir) |
| `src/components/view/corporate/asset-management/condition/asset-condition-table-columns.jsx` | Baru |
| `src/components/view/corporate/asset-management/condition/asset-condition-action-modals.jsx` | Baru — tiga modal; berkas berita acara memakai `BaseGroupedEditorField` |
| `src/components/view/corporate/asset-management/condition/asset-condition-report-{list,detail,form}-view.jsx` | Baru |
| `src/app/corporate/asset-management/condition/{reports,reports/create,reports/[slug],active,damaged,unused,lost,history}/page.jsx` | Baru — 8 route tipis |
| `src/lib/constants/corporate/asset-management/register/asset-record-constants.jsx` | Diubah — `ASSET_CONDITION_LIST_VARIANTS` (4 varian bersaringan tetap) |
| `src/lib/hooks/corporate/asset-management/register/use-asset-inventory-list.jsx` | Diubah — menerima varian kondisi, opsi saringan dapat dibatasi (`optionValues`), `openConditionReport` bila berhak |
| `src/components/view/corporate/asset-management/register/asset-inventory-list-view.jsx` | Diubah — eyebrow per varian, placeholder saringan, aksi Laporkan kondisi |
| `src/components/view/corporate/asset-management/register/asset-record-table-columns.jsx` | Diubah — kolom Aksi daftar inventaris menerima `onReport` |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | Diubah — resource `assetsRegistered` (label "nomor inventaris · nama aset", kondisi, unit/ruangan) |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — grup "Kondisi & Status Aset" + 6 butir |
| `src/components/view/corporate/asset-management/master-data/shared/asset-master-form-view.jsx` + 6 konstanta master | Diubah — **permintaan Rizki di luar kartu** (bagian 3.4) |

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. Tidak ada slice atau perubahan store.
- Empat daftar kondisi memakai **satu** view dan hook Daftar Inventaris lewat varian (AC1); Pemeriksaan Kondisi dan
  Riwayat memakai satu hook/view dengan dua varian.
- Nol baris komentar, nol inline style, tidak ada CSS baru, tidak ada base component yang diubah.

`UI GATE: 16 elemen — REUSE 12, EXTEND 1, COMPOSE 2, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kepala layar, rincian | `Hero`, `BaseDetailView` | dipakai `FE-AST-004`..`006` | REUSE | — |
| Kartu ringkasan, tab | `SummaryGrid`, `DataFilter` `tabs` | dipakai `FE-AST-006` | REUSE | — |
| Saringan kondisi/sumber/unit/tanggal | `FilterSelect`, `AssetMasterResourceFilter`, `FilterDatePicker` | dipakai `FE-AST-005`/`006` | REUSE | — |
| Tabel, aksi baris, badge | `DataTable`, `RowActionMenu`, `StatusBadge` | dipakai `FE-AST-003`..`006` | REUSE | — |
| Tolak beralasan | `ConfirmModal` `requireReason` | dipakai `FE-AST-003` | REUSE | — |
| Pesan, toast | `InformationAlert`, `ToastStack` | dipakai `FE-AST-003`..`006` | REUSE | — |
| Formulir lapor | `BaseGroupedEditorView` | dipakai `FE-AST-003`/`006` | REUSE | — |
| Unggah berita acara (formulir dan modal) | `BaseGroupedEditorField` tipe berkas (`accept`, `fileHint`) | `base-grouped-editor-field.jsx` | REUSE | — |
| Buka PDF/lampiran di tab baru | `use-asset-pdf-tab` | `FE-AST-006` | REUSE | — |
| Pilihan aset | `useSelectResource` + resource baru `assetsRegistered` | pola `assetGroupsDetailed` | REUSE | — |
| Empat daftar kondisi | Daftar Inventaris (`FE-AST-004`) bervarian | `ASSET_LIST_VARIANTS` | EXTEND | A (dipilih): varian + `optionValues` + eyebrow + aksi opsional, bawaan tidak berubah — satu komponen (AC1). B: view daftar kondisi tersendiri — duplikasi. Bukan base component |
| Modal Tetapkan/Setujui dengan pilihan kondisi dan berkas | `ConfirmModal` `children` + `confirmProps.disabled` | `confirm-modal.jsx` | COMPOSE | A (dipilih): isi modal dirangkai dari `FilterSelect`/`BaseGroupedEditorField`. B: modal baru — tidak ada kebutuhan yang belum tercakup |
| Field foto di formulir grouped | `renderField` | `base-grouped-editor-form.jsx` | COMPOSE | A (dipilih): `renderField` merender pembungkus foto untuk satu field, sisanya `BaseGroupedEditorField` |
| Foto kondisi (kamera) | `UploadPhotoField.jsx` (diwajibkan AC2) | komponen fitur lama berbasis react-hook-form | WRAP | A (dipilih): pembungkus kecil dengan `useForm`/`useWatch` lokal; komponen tidak diubah. Konsekuensi: rupa field foto berbasis react-bootstrap lama (tombol "Choose File", "Kamera", "Hapus Foto") sedikit berbeda dari isian lain, dan komponen membatasi JPG/PNG 2 MB. B: field berkas `BaseGroupedEditorField` — rupa seragam tetapi tanpa kamera dan melanggar AC2 |

### 3.4 Perubahan di luar kartu atas permintaan Rizki

**Formulir tambah/ubah master Aset diseragamkan dengan master data lain (2026-11-18).** Rizki meminta formulir
tambah master klasifikasi aset tampil seperti formulir master Akuntansi (contoh: Tambah Akun COA). Penyebab beda:
`asset-master-form-view` memakai `BaseEditorView` yang sama, tetapi dengan `showPreview={false}` dan kelas tema
halaman tambahan. Perubahan: panel pratinjau kiri dinyalakan (judul "Master Klasifikasi Aset — <master>",
`previewDescription`), judul formulir "Informasi <master>" beserta `helperText` per konfigurasi, deskripsi hero dan
label tombol mengikuti pola COA/job-level, kelas tema tambahan dilepas. Berlaku untuk enam master (Kategori,
Kelompok, Jenis Kepemilikan, Lokasi, Klasifikasi, Jenis Dokumen); Pengaturan Aset dan formulir transaksi tidak
diubah. Diperiksa dengan tangkapan layar Kategori, Kelompok, Lokasi, dan pembanding COA (`cek-form-master.mjs`).

**Label pilihan aset memuat nomor inventaris.** Ditemukan saat uji: pilihan aset hanya menampilkan nama, sehingga
puluhan "Kursi Tunggu UJI-008-YWW1" tidak dapat dibedakan. Label diubah menjadi "nomor inventaris · nama aset".

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable`/kartu memuat; tab dan saringan nonaktif |
| Kosong | Kalimat per layar, misalnya "Belum ada aset tidak digunakan pada saringan ini." |
| Gagal muat / 403 | `InformationAlert`; `AccessDeniedGate` |
| Formulir belum lengkap | Isian bertanda merah + "Lengkapi isian bertanda merah sebelum mengirim laporan." |
| Kirim/aksi berjalan | Tombol ber-`loading`; tombol rincian nonaktif selama memuat ulang |
| Laporan menunggu | Peringatan "menunggu penetapan Bagian Aset" atau "menunggu persetujuan Kepala Bagian Aset" |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Register / Asset Condition Report (`…/register/condition-reports`)

| Layar | Endpoint |
|---|---|
| Pemeriksaan Kondisi | `GET /?status=&awaitingLossApproval=&reportedCondition=&source=&organizationUnitId=&from=&to=&search=`; `GET /summary` |
| Riwayat Perubahan Status | `GET /?status=Ditetapkan&…` (delta: kontrak `GET /register/reports/history?eventType=KondisiBerubah`, `BE-AST-013`) |
| Rincian | `GET /{id}`; `GET /{id}/attachments/{attachmentId}` (blob); `GET /{id}/loss-document` (blob) |
| Formulir | `POST /` (multipart: `assetId`, `reportedCondition` angka, `source`, `isFoundAgain`, `notes`, `photo`, `lossReport`) |
| Daftar, rincian | `POST /{id}/decide` (`{accept, decidedCondition: angka, reason, rowVersion}`); `POST /{id}/approve-loss` (multipart) |

Pendukung: `GET /register/assets?recordStatus=Terdaftar&condition=…` (daftar kondisi, pilihan aset),
`GET /register/assets/{id}` (aset terisi dari daftar).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 35 berkas | `0 errors, 0 warnings` (satu peringatan React Compiler pada `watch()` diperbaiki dengan `useWatch`) | `PASS` | Keluaran perintah |
| Grep komentar dan inline style | Kosong | `PASS` | — |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-18 09:29:34; manifest memuat 8 route `condition`; tidak ada source yang lebih baru | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-007-20261118/`) | **27 langkah sesuai harapan**; 0 galat halaman; 0 respons 5xx | `PASS` | `hasil-uji-bagian1.jsonl`, `hasil-uji-bagian2.jsonl`, `cek-data.mjs`, tangkapan layar |
| Formulir master vs COA | Panel pratinjau dan "Informasi …" tampil pada Kategori, Kelompok, Lokasi seperti COA; 0 galat halaman | `PASS` | `master-*-tambah.png`, `pembanding-coa-tambah.png` |

Aktor: superadmin uji (Petugas dan Kepala Bagian Aset), Dimas Kurniawan / akun PERAWAT (penanggung jawab), akun
penyetuju aset (tanpa `AssetCondition`), LOKET.

| Langkah | Hasil |
|---|---|
| **AC1** `SCR-23` Aset Aktif | `recordStatus=Terdaftar&condition=Aktif`; eyebrow "Kondisi & Status Aset"; 25 baris semuanya Aktif; grup menu tampil |
| **AC1** `SCR-24` Rusak Ringan/Berat | Bawaan `condition=RusakRingan` (memuat `000031`); pilihan hanya Rusak Ringan / Rusak Berat → `condition=RusakBerat` |
| **AC1** `SCR-25`/`26` | `condition=TidakDigunakan` (kalimat kosong tampil), `condition=Hilang` |
| Laporkan dari daftar | Menu baris "Lihat detail", "Laporkan kondisi" → formulir dengan aset terisi "AST-MBL-000034 · Kursi Tunggu …" |
| **AC2** foto | Tombol **Kamera** `UploadPhotoField` ada; kirim tanpa foto → tanpa `POST`, pesan "Lampirkan foto kondisi aset."; berkas PNG → pratinjau; kirim → `AST-KND-000007` Dilaporkan |
| **AC1** `SCR-22` | Tab Semua / Menunggu Penetapan (1) / Menunggu Persetujuan Kehilangan (0) / Ditetapkan / Ditolak; kartu "Menunggu Penetapan 1" |
| Tetapkan dari menu baris | Menu Lihat detail / Tetapkan kondisi / Tolak laporan; modal bawaan "Rusak Berat"; body `{"accept":true,"decidedCondition":3,…}` 200; toast; aset muncul di daftar Rusak Berat |
| Pilih aset di formulir | Pencarian `search=AST-MBL-000036` → "AST-MBL-000036 · Kursi Tunggu UJI-008-YWW1"; isian berita acara muncul untuk Hilang |
| Laporan kedua saat terbuka | `409 AST-VAL-REG-035` ditampilkan |
| **AC3** Setujui Hilang | Tab `awaitingLossApproval=true`; rincian hanya Setujui Hilang (tanpa Tetapkan); tombol modal **nonaktif tanpa berkas**, aktif sesudah PDF dilampirkan; 200; status Hilang; Cetak Berita Acara Kehilangan → PDF 43.077 byte di tab `blob:` |
| Ditemukan kembali | Aset muncul di daftar Hilang; formulir: pemberitahuan ditemukan kembali, opsi tanpa Hilang; setujui dengan bawaan Aktif → 200 |
| Tolak | Tombol nonaktif tanpa alasan; body `{"accept":false,"reason":…}` 200; toast |
| **AC1** `SCR-27` | `status=Ditetapkan`; kolom Ditetapkan / Aset / Perubahan / Sumber / No. Laporan / Alasan; 6 baris; "Aktif → Rusak Berat (AST-KND-000007)"; tanpa tab |
| LOKET | Akses Ditolak; grup menu tidak tampil |
| Penyetuju tanpa `AssetCondition` | Pemeriksaan Kondisi → Akses Ditolak; daftar Aset Aktif hanya "Lihat detail" |
| Regresi Daftar Inventaris | Menu hanya "Lihat detail"; eyebrow "Registrasi & Inventaris" |

Catatan jalannya uji: putaran pertama berhenti dua kali karena skrip (aset dicari di halaman 1 tanpa pencarian;
pilihan aset dicari dari teks nomor yang belum tampil di label). Yang kedua mengungkap cacat label (bagian 3.4) yang
lalu diperbaiki. Satu percobaan skrip sempat mengirim laporan Hilang `AST-KND-000008` sebelum berhenti; putaran
berikutnya karenanya mendapat `409 035` (benar) dan menyetujui `000008`. Data akhir diperiksa lewat API
(`cek-data.mjs`). Server FE memuat ulang perbaikan label tanpa build; perbaikan itu diperiksa di layar
(`cek-label.mjs`) tetapi belum masuk `BUILD_ID` 09:29:34.

`MANUAL TEST: PASS` (otomatis lewat Playwright di browser sungguhan, ditambah pemeriksaan visual tangkapan layar).
`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`** (lewat layar, melanjutkan izin uji modul Aset): `AST-KND-000007`
(Ditetapkan Rusak Berat, `AST-MBL-000035`), `000008` (Hilang disetujui, `000036`), `000009` (ditemukan kembali →
Aktif, `000036`), `000010` (Ditolak, `000035`). Kini `AST-MBL-000035` **Rusak Berat**, `000036` **Aktif**.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `SCR-AST-REG-22..27`; empat daftar kondisi memakai satu komponen bersaringan tetap | Terpenuhi | `asset-condition-report-list-view` (varian `reports`, `history`), rincian, formulir; `AssetInventoryListView` + `ASSET_CONDITION_LIST_VARIANTS`; uji AC1. `SCR-27` memakai laporan kondisi atas keputusan Rizki (delta) |
| 2. Foto memakai `UploadPhotoField.jsx` | Terpenuhi | `asset-condition-photo-field` membungkus komponen tanpa mengubahnya; uji AC2 |
| 3. Setujui Hilang mewajibkan lampiran berita acara | Terpenuhi | `use-asset-condition-actions.confirmDisabled`/`needsLossFile`; backend `032` sebagai lapis kedua; uji AC3 |
| `DoD-FE` | Terpenuhi | Lint, build pemilik, uji layar, laporan, roadmap, traceability |

Rekap: **3 dari 3 kriteria terpenuhi** → `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) `SCR-27` memakai laporan kondisi yang ditetapkan; penetapan tanpa perubahan (mis. Aktif → Aktif) ikut tampil, sedangkan linimasa hanya mencatat perubahan. Alihkan ke `/reports/history` saat `BE-AST-013`. (2) Rupa field foto mengikuti `UploadPhotoField` lama (react-bootstrap), batas klien 2 MB JPG/PNG. (3) Saringan Ruangan di daftar kondisi tidak dibatasi unit terpilih (perilaku Daftar Inventaris `FE-AST-004`). (4) Di DB dev, akun Perawat memegang `Decide`, sehingga ia juga melihat Tetapkan/Tolak dan isian sumber |
| Risiko | Data uji baru di DB dev (bagian 6) |
| Perubahan sampingan | Daftar Inventaris menerima varian dan aksi opsional (perilaku lama tetap, diuji); formulir master Aset diseragamkan atas permintaan Rizki (bagian 3.4) |
| Status Git | Frontend `RizkiV2`: 13 berkas diubah, 22 berkas baru, belum di-commit. Backend: hanya dokumen ini dan baris status |
| Langkah berikutnya | `BE-AST-011` → `FE-AST-008` (dokumen, pengamanan, pemanfaatan, menganggur) |
