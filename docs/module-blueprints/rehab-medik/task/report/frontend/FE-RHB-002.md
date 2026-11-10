# Laporan Perubahan Frontend — `FE-RHB-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-002` |
| Judul | Konfigurasi Layanan Rehab (layar FE-RHB-14) |
| Slice | FS-A — Fondasi dan konfigurasi |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-002` (r2) |
| Trace | RHB-DEC-004, 017, 018, 020; V-CFG-01/02; temuan `BE-RHB-003`; `03-frontend-architecture.md` §2, FE-RHB-14 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — API *Rehab Discipline* (`BE-RHB-005` ✅) |
| Dependency | `FE-RHB-001` ✅, `BE-RHB-005` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). ESLint: 0 error, 2 warning (keduanya baseline modul rujukan). Uji browser Playwright: tahap 2 16/17, tahap 2b 13/14, tahap 3 10/10, cek status 1/1. Kedua FAIL sudah ditutup: satu cacat nyata diperbaiki lalu diuji ulang, satu lagi kesalahan skrip (§5). |

## 1. Masalah yang diselesaikan

Admin belum punya layar untuk mengaktifkan layanan rehab. Seeder mengisi empat layanan dalam
keadaan **tidak aktif tanpa profesi**, dan layanan hanya boleh aktif bila punya profesi klinis
(`RHB-VAL-070`). Tanpa layar ini, permintaan rehab (`FE-RHB-003`) tidak bisa memilih layanan apa pun.
Komite medik juga belum bisa membuat instrumen berjenis rehab, karena pilihan jenis di layar
instrumen klinis berhenti di nilai 6.

**Contoh.** Admin membuka *Master Data → Rehab Medik → Layanan Rehab*, membuka Fisioterapi, memilih
profesi Fisioterapis, mengaktifkannya, lalu menambahkan tindakan "Terapi latihan" dan memastikan
Berg Balance Scale tercentang. Bila ia lupa memilih profesi, form menolak di field *Profesi
Klinis* dengan pesan dari server.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-14 (`03-frontend-architecture.md`); standar master data (rujukan `hr/master-data/job-level`); `DEV_DISCRETION` untuk tata letak dua bagian tambahan |
| Izin owner | ESLint (2026-11-06); menjalankan backend + `next dev` + Playwright di dev (2026-11-06, jawaban "Agent jalankan dev + Playwright") |
| Menyentuh milik modul lain | `clinical-instrument-utils.js` (ClinicalManagement) — izin RHB-DEC-018 |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/lib/constants/health-services/rehabilitation-management/master-data/discipline/rehab-discipline-constants.jsx` | Baru | `REHAB_DISCIPLINE_CONFIG` tunggal; field kode (`createOnly`, pola, huruf besar), nama, profesi klinis (select registry `professions` + `filters: { isClinicalProfession: true }`), status (`updateOnly`); peta kode galat → field |
| `src/lib/state/slice/health-services/rehabilitation-management/master-data/master-data-rehab-discipline-slice.jsx` | Baru (salinan struktur rujukan) | 9 thunk baseline + **2 thunk sub-resource** `saveRehabDisciplineProcedures`, `saveRehabDisciplineInstruments` yang memperbarui `detail` dan hitungan baris tanpa memuat ulang |
| `src/utils/health-services/rehabilitation-management/master-data/discipline/rehab-discipline-utils.jsx` | Baru (salinan + penyesuaian) | `createOnly`, validasi `pattern`, normalisasi huruf besar, `isActive` tidak dikirim di `PUT`, format `status` (Aktif/Nonaktif) |
| `src/lib/hooks/.../discipline/use-master-data-rehab-discipline.jsx`, `-detail.jsx` | Baru (salinan) | Controller daftar dan rincian; fallback urutan `disciplineCode` |
| `src/lib/hooks/.../discipline/use-master-data-rehab-discipline-editor.jsx` | Baru (salinan + penyesuaian) | `PATCH /status` sesudah `PUT` bila status berubah; galat server dipetakan ke field lewat `errors.fields`/`errors.code` |
| `src/lib/hooks/.../discipline/use-rehab-discipline-configuration.jsx` | Baru | Draf tindakan dan jenis instrumen (draf `null` = ikuti server), tambah/lepas/simpan/batalkan; opsi jenis dari `/filters/metadata` |
| `src/components/view/.../discipline/master-data-rehab-discipline-view.jsx` | Baru (salinan) | Tombol *Tambah* hanya bagi `RehabConfiguration:Create` |
| `src/components/view/.../discipline/detail/rehab-discipline-detail-view.jsx` | Baru | *Perbarui* bagi `Update`, *Hapus* bagi `Delete`; dua bagian konfigurasi lewat `afterDetail`; toast digabung |
| `src/components/view/.../discipline/add/rehab-discipline-form-view.jsx` | Baru (salinan) | Deskripsi diperbaiki ke "master data Rehab Medik" |
| `src/components/features/health-services/rehabilitation-management/master-data/rehab-discipline-configuration-sections.jsx` | Baru | Bagian *Tindakan yang Boleh Dipakai* dan *Jenis Instrumen Asesmen* (COMPOSE) |
| `src/style/components/features/rehabilitation-management/rehab-discipline-configuration.module.css` | Baru | Tata letak saja, token `--space-*`, `--app-section-gap`, `--color-text-muted` |
| `src/app/health-services/rehabilitation-management/master-data/disciplines/{page,rehab-discipline-client,create/page,[slug]/page,[slug]/update/page}.jsx` | Baru (salinan) | Route tipis, `[slug]` menolak token cadangan/kosong |
| `src/lib/state/store.jsx` | Diperbarui (+2) | Reducer `masterDataRehabDiscipline` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui (+17) | Grup *Rehab Medik* → *Layanan Rehab* di Pelayanan Kesehatan → Master Data, dengan `requiredPermission: { resource: "RehabConfiguration", action: "Read" }` |
| `src/utils/health-services/clinical-management/clinical-instrument-utils.js` | Diperbarui (+23) | `ClinicalInstrumentKind` 8–13, label, dan opsi Bahasa Indonesia; nilai 1–6 tidak berubah |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Daftar: hero, ringkasan, filter, tabel, badge, paginasi, gerbang 403 | `REUSE` | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable`, `StatusBadge`, `AccessDeniedGate` |
| 2 | Form buat/ubah | `REUSE` | `BaseEditorView` + `BaseEditorField` |
| 3 | Rincian identitas | `REUSE` | `BaseDetailView` |
| 4 | Bagian Tindakan | `COMPOSE` | `BaseDetailSection` + `DataTable` + `BaseEditorField` (select `procedures`) + `BaseButton` + `StatusBadge` |
| 5 | Bagian Jenis Instrumen | `COMPOSE` | `BaseDetailSection` + `BaseCheckboxCard` + `BaseButton` |
| 6 | Butir menu | `REUSE` | Pola `menu-items.jsx` + `requiredPermission` |
| 7 | Instrumen asesmen medis Sp.KFR (`/medical-instruments`) | Ditunda | Tidak ada di skema FE-RHB-14 |

Pilihan yang disajikan untuk elemen 4, 5, dan 7 tercatat di percakapan task. Yang dipakai adalah
opsi yang direkomendasikan.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 2 COMPOSE, 1 ditunda (gap desain).`

### 3.3 Delta terhadap desain dan standar

| Delta | Rincian |
| --- | --- |
| Cakupan roadmap menyebut `rehab-discipline.service.js` | Diganti bentuk standar master data (slice + constants + utils + hook), sesuai `master-data-feature-standard.md` yang mengikat untuk fitur master data. Tidak ada berkas service terpisah. |
| Thunk tambahan | `saveRehabDisciplineProcedures` dan `saveRehabDisciplineInstruments` untuk sub-resource `PUT /{id}/procedures` dan `/{id}/instruments`. `GET` sub-resource tidak dipakai, karena `GET /{id}` sudah memuat keduanya. |
| Status aktif | Tetap `updateOnly` sesuai standar, sehingga layanan baru selalu dibuat tidak aktif. Karena `PUT` backend tidak menerima `isActive`, hook editor memanggil `PATCH /{id}/status` bila nilainya berubah. |
| Kode layanan | Diisi pengguna (`isCodeUserInput: true`), hanya di form buat (`createOnly`), dan dinormalkan ke huruf besar |
| Instrumen asesmen medis Sp.KFR | Tidak dibuatkan layar karena tidak ada di skema FE-RHB-14. Seeder sudah mengisi Skala Nyeri + Barthel, dan endpoint `/medical-instruments` tersedia. Usulan: tambahkan ke desain revisi berikutnya. |
| Nilai `ClinicalInstrumentKind` 7 | Tetap tidak ada di FE (selisih legacy, `BE-RHB-003` §7), di luar cakupan |

## 4. Verifikasi

| Pemeriksaan | Hasil | Bukti |
| --- | --- | --- |
| ESLint (konfigurasi repo) atas seluruh berkas task + `menu-items.jsx`, `store.jsx`, `clinical-instrument-utils.js` | 0 error, 2 warning `react-hooks/set-state-in-effect` di hook detail/editor (resolusi token rute). Keduanya **identik** dengan modul rujukan `hr/master-data/job-level` (ESLint atas folder itu juga 2 warning yang sama), jadi bukan temuan baru. Tiga warning dari kode baru sudah diperbaiki. | Keluaran terminal |
| Grep anti-regresi UI (`ui-consistency-checklist.md`) | Warna literal 0; tipografi di CSS fitur 0 (satu `font-size` berbasis token dibuang); `<button>`/kelas `btn` mentah 0; `<table>` mentah 0; kelas `fw-*`/`fs-*` 0; `!important` 0 | grep |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) | 0 | grep |
| Review diff | 3 berkas existing berubah (+42 baris), sisanya berkas baru di folder rehab | `git diff --stat` |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku layar dibuktikan lewat uji browser.

## 5. Uji browser (MANUAL TEST)

Backend Debug `https://localhost:7184` + `next dev` port 3000, Playwright Chromium dari scratchpad,
dijalankan 2026-11-06. SuperAdmin dan satu akun uji; kredensial dibaca dari config/respons API tanpa
dicetak.

| Tahap | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Daftar: 5 layanan, semuanya *Nonaktif*; ringkasan 5/0/5/4; menu *Master Data → Rehab Medik → Layanan Rehab* aktif; request `filters/metadata`, `summary`, `list` 200 | PASS |
| 2 | Rincian Fisioterapi: bagian Tindakan dan Jenis Instrumen tampil; 7 kartu jenis; tercentang 3, 9, 10 sesuai seed; *Perbarui* dan *Hapus* tampil untuk SuperAdmin | PASS ×5 |
| 2 | **AC1**: form ubah tanpa input kode; aktifkan tanpa profesi → `PUT` 200 lalu `PATCH /status` 422 `RHB-VAL-070`; pesan "Tetapkan profesi klinis untuk layanan ini." tampil di blok field *Profesi Klinis* dan toast; tetap di halaman ubah | PASS ×5 |
| 2 | Form buat: kode `1salah` ditolak di klien tanpa request; `physiotherapy` dinormalkan → `POST` 409 `RHB-VAL-071`, pesan tampil | PASS ×3 |
| 2 | Simpan instrumen (layanan uji): `PUT /instruments` 200, kartu tetap tercentang, tombol nonaktif lagi | PASS ×3 |
| 2 | Tambah tindakan | **FAIL**, lalu diperbaiki: opsi pertama select adalah placeholder bernilai kosong yang mengaktifkan *Tambah* tanpa menambah apa pun |
| 2b | Ulang sesudah perbaikan: Simpan nonaktif tanpa perubahan → aktif sesudah diubah → `PUT` 200 → bertahan setelah muat ulang → dipulihkan; *Tambah* nonaktif sebelum memilih → aktif → baris draf → `PUT /procedures` 200 → bertahan setelah muat ulang → *Lepas* + simpan → kosong lagi | PASS ×12 |
| 2b | **AC5**: modal pembuatan di layar Instrumen Klinis memuat keenam jenis rehab | PASS (6/6) |
| 2b | Teks halaman ubah | FAIL palsu (pencarian teks tidak peka huruf besar-kecil menangkap menu "Sumber Daya Manusia"), diulang secara tepat di tahap 3 |
| 3 | Deskripsi halaman ubah "master data Rehab Medik", tanpa "sumber daya manusia" | PASS |
| 3 | **AC3/AC4** akun uji dengan `RehabConfiguration : Read` saja: daftar tampil; tanpa *Tambah*; butir menu *Layanan Rehab* tampil; rincian hanya *Kembali*; tanpa select tambah tindakan maupun tombol simpan; 7 kartu instrumen terlihat tetapi nonaktif; tautan *Buka Instrumen Klinis* tetap ada | PASS ×7 |
| 3 | **AC4** akun uji tanpa butir rehab: *Layanan Rehab* **dan** grup *Rehab Medik* hilang dari menu; halaman menampilkan *Ups! Akses Ditolak* (dicek juga lewat screenshot) | PASS ×2 |
| 3 | **AC2**: rincian O&P berstatus *Nonaktif* (sesudah format status diperbaiki dari "Tidak") | PASS |

Rekap per tahap: tahap 2 **16/17**, tahap 2b **13/14**, tahap 3 **10/10**, cek status **1/1**.
- FAIL tahap 2 (tambah tindakan) adalah cacat nyata. Sudah diperbaiki di hook, lalu seluruh alurnya
  lolos di tahap 2b.
- FAIL tahap 2b (teks halaman ubah) adalah kesalahan skrip. Diulang secara tepat di tahap 3 dan lolos.

Teks sisa salinan di halaman ubah ("…sumber daya manusia") terlihat di screenshot tahap 2. Teks itu
sudah diperbaiki dan dibuktikan di tahap 3.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) 422 `RHB-VAL-070` tampil di field profesi | Terpenuhi (browser) | §5 tahap 2 |
| (2) Disiplin O&P tampil tidak aktif | Terpenuhi (browser) | §5 tahap 1 dan 3 |
| (3) Tombol sesuai `RehabConfiguration:*` | Terpenuhi (browser): SuperAdmin melihat semua tombol; pemegang `Read` saja tidak melihat *Tambah*/*Perbarui*/*Hapus*/simpan. Kombinasi `Update` tanpa `Delete` tidak diuji terpisah; logikanya terpisah per butir (`usePermission` per tombol). | §5 tahap 2–3 |
| (4) Menu r2: `requiredPermission` RehabConfiguration:Read; grup hilang bila anaknya tersembunyi | Terpenuhi (browser) | §5 tahap 3 |
| (5) Instrumen r2: 8–13 di `ClinicalInstrumentKind`, label, opsi; 1–6 tidak berubah; komite dapat memilih jenis rehab | Terpenuhi (diff + browser). Pembuatan instrumen sampai simpan tidak dijalankan, agar tidak menambah data komite; modal menampilkan keenam jenis. | §3.1, §5 tahap 2b |
| Verifikasi kartu: browser | Terpenuhi | §5 |
| DoD: kriteria, laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Prasyarat hak admin konfigurasi | Select profesi memanggil `GET /corporate/human-resource/master-data/professions/options`, dijaga **`Profession : Read`** (HR). Select tindakan memanggil `GET /health-services/master-data/procedures/options`, dijaga **`Procedure : Read`**. Admin konfigurasi rehab perlu kedua butir ini selain `RehabConfiguration : *`; tanpa keduanya, select kosong atau dijawab 403. Usulan: tambahkan ke peta peran `permission-audit-matrix.md` §3 saat revisi. |
| Temuan modul lain (tidak diubah) | Footer tetap aplikasi menutupi bagian bawah konten di layar setinggi 900–1000 px (terlihat di layar daftar dan rincian). Ini milik layout bersama. |
| Data uji di `QuilvianNewDevSukma` | Instrumen dan tindakan layanan uji `RHB_UJI_10722EA2` sudah dipulihkan seperti semula (tanpa Barthel, tanpa tindakan). `PUT` tanpa perubahan pada Fisioterapi mengisi `UpdateBy`/`UpdateDateTime`; isi data tidak berubah dan status tetap nonaktif. Akun uji `rhb-uji-ba8e67a1@example.test` sudah dinonaktifkan, dan kebijakan *Pendaftaran / Admission Officer* dikembalikan ke kosong. |
| Server | Backend dan `next dev` sudah dimatikan; port 7184 dan 3000 kosong |
| Status Git FE | `M` `store.jsx`, `menu-items.jsx`, `clinical-instrument-utils.js`; `??` folder rehab di `app`, `components/features`, `components/view`, `lib/constants`, `lib/hooks`, `lib/services`, `lib/state/slice`, `style`, `utils` |
| Dependency backend | `BE-RHB-005` ✅ |
| Langkah berikutnya | `FE-RHB-003` (permintaan rehab; `FE-RHB-001` ✅, `BE-RHB-007` ✅) |
