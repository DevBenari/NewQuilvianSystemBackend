# Laporan Perubahan Frontend — `FE-RHB-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-003` |
| Judul | Permintaan rehab (layar FE-RHB-01 daftar, FE-RHB-02 form, FE-RHB-03 rincian) |
| Slice | FS-B — Permintaan dan episode |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-003` (r2) |
| Trace | FR-RHB-001..004; RHB-INV-02, 07; RHB-DEC-016; `03-frontend-architecture.md` §2–3 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — API *Rehab Request* (`BE-RHB-007` ✅), *Rehab Lookup* (`BE-RHB-006` ✅) |
| Dependency | `FE-RHB-001` ✅, `BE-RHB-007` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). ESLint: 0 error, 0 warning baru. Uji browser + jaringan Playwright: 19/20 pada uji lengkap. Satu FAIL tidak bisa direproduksi dalam 3 pengulangan, dan penyebabnya teridentifikasi sebagai cara uji (§5). Pemeriksaan tambahan: riwayat 1/1. |

## 1. Masalah yang diselesaikan

Backend permintaan sudah siap, tetapi petugas belum punya layar untuk membuat, mengajukan, dan
menelaah permintaan rehab. Layar harus menjaga tiga hal yang mudah salah:
- **Kunjungan milik pasien yang benar.** Kunjungan dari pasien sebelumnya tidak boleh terbawa.
- **Duplikasi.** Permintaan untuk layanan yang sedang berjalan harus disertai alasan.
- **Pemisahan kewenangan.** Petugas admisi boleh mengembalikan berkas, tetapi tidak boleh menolak
  secara klinis.

**Contoh.** Admisi memilih pasien ANISA PRAMESTI, kunjungan Poli `ENC-RSMMC-00165`, dokter
peminta, indikasi, dan layanan. Form langsung menampilkan *Peringatan Duplikasi* karena pasien
masih punya episode berjalan. Admisi mengisi alasan lalu mengajukan. Di rincian, dr. Sp.KFR
melihat tombol *Terima/Tolak/Alihkan/Minta Klarifikasi*, sedangkan petugas admisi hanya melihat
*Kembalikan*.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-01..03 dan peta menu `03-frontend-architecture.md` §2–3; `DEV_DISCRETION` untuk rupa bagian rangkaian |
| Izin owner | ESLint dan uji browser (backend + `next dev` + Playwright, akun uji dinonaktifkan sesudahnya), dari instruksi task 2026-11-06 |
| Menyentuh berkas bersama | `menu-items.jsx`, registry select (`health-service-select-resources.js`, `select-resource-registry.js`). Hanya **menambah entri**; tidak ada perilaku komponen yang berubah. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/lib/services/health-services/rehabilitation-management/rehab-request.service.js` | Baru | 9 fungsi API permintaan (metadata, ringkasan, daftar, rincian, riwayat, cek duplikasi, buat, ubah, aksi status) |
| `src/lib/services/health-services/rehabilitation-management/rehab-lookup.service.js` | Baru | Layanan aktif untuk kartu centang |
| `src/lib/constants/health-services/rehabilitation-management/requests/rehab-request-constants.js` | Baru | Status + nada badge, sumber, periode, filter bawaan, ringkasan, field form + grup, peta kode galat → field, aksi rincian, label riwayat |
| `src/utils/health-services/rehabilitation-management/requests/rehab-request-utils.js` | Baru | Payload, rincian → form, validasi draf/pengajuan, gabung tanggal + jam, baris rincian, galat server → field, kandidat duplikasi |
| `src/lib/hooks/health-services/rehabilitation-management/requests/use-rehab-requests.js` | Baru | Controller daftar (`AbortController`, halaman kembali ke 1 setiap filter berubah) |
| `.../requests/use-rehab-request-editor.js` | Baru | Controller form: ganti pasien/sumber mengosongkan kunjungan dan surat; cek duplikasi otomatis (jeda 400 ms); 409 `RHB-VAL-007` membuka bagian duplikasi; 409 `RHB-VAL-010` menawarkan muat ulang versi; draf baru tidak dibuat dua kali |
| `.../requests/use-rehab-request-detail.js` | Baru | Controller rincian: aksi lewat modal; 409 basi menjaga modal dan isiannya |
| `src/components/view/.../requests/rehab-requests-view.jsx` | Baru | FE-RHB-01 |
| `src/components/view/.../requests/rehab-request-form-view.jsx` | Baru | FE-RHB-02 (`BaseGroupedEditorView` + `renderField`/`renderActions`/`shouldHideField`/`normalizeField`) |
| `src/components/view/.../requests/rehab-request-detail-view.jsx` | Baru | FE-RHB-03 + `ConfirmModal` + riwayat status |
| `src/components/features/.../requests/rehab-request-discipline-picker.jsx` | Baru | Kartu centang layanan (COMPOSE) |
| `src/components/features/.../requests/rehab-request-duplicate-panel.jsx` | Baru | Peringatan + tabel kandidat + isian alasan (COMPOSE) |
| `src/app/health-services/rehabilitation-management/requests/{page,new/page,[requestId]/page,[requestId]/edit/page}.jsx` + `[requestId]/request-route-param.js` | Baru | Route tipis; id selain UUID dijawab 404 |
| `src/lib/hooks/select/health-service/health-service-select-resources.js` | Diperbarui (+42) | Resource `rehabPatients` (minimal 3 karakter), `rehabEncounters`, `rehabReferralLetters`, `rehabDoctors` |
| `src/lib/hooks/select/select-resource-registry.js` | Diperbarui (+4) | Cascade `patientId` untuk kunjungan dan surat rujukan (untuk pemakai `BaseEditorField`) |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui (+20 di task ini) | Blok tingkat 0 *Rehab Medik* (`subMenu`) → *Permintaan Rehab* dengan `requiredPermission: { resource: "RehabRequest", action: "Read" }` |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Daftar: hero, ringkasan, filter, tabel, badge, paginasi, aksi baris, gerbang 403 | `REUSE` | `Hero`, `SummaryGrid`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `StatusBadge`, `RowActionMenu`, `AccessDeniedGate` |
| 2 | Form berkelompok | `REUSE` | `BaseGroupedEditorView` (slot `formProps`) |
| 3 | Select pasien/kunjungan/surat/dokter | `REUSE` | `BaseGroupedEditorField` + 4 resource registry baru |
| 4 | Kartu pasien terpilih | `REUSE` | `BasePatientCard` |
| 5 | Multi-pilih layanan | `COMPOSE` | `BaseCheckboxCard` |
| 6 | Peringatan duplikasi | `COMPOSE` | `InformationAlert` + `DataTable` + `BaseGroupedEditorField` |
| 7 | Tombol form | `COMPOSE` | `BaseButton` via `renderActions` |
| 8 | Rincian + aksi | `REUSE` | `BaseDetailView` + `usePermission` |
| 9 | Modal aksi (alasan, dokter penanggung jawab, tujuan pengalihan) | `REUSE` | `ConfirmModal` (`requireReason` + `children`) |
| 10 | Riwayat status | `COMPOSE` | `BaseDetailSection` + `DataTable` |
| 11 | Blok menu | `REUSE` | `menu-items.jsx` + `requiredPermission` |

`UI GATE: PASS — 0 NEW, 0 EXTEND, 4 COMPOSE (direkomendasikan; pilihan bernomor disajikan di percakapan task).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| State | Service + hook per halaman, tanpa slice Redux: data tidak dibagi lintas halaman, karena setiap halaman memuat datanya sendiri. Sesuai cakupan kartu (`rehab-request.service.js`, `rehab-lookup.service.js`). |
| Jam permintaan | Isian teks `HH:mm` dengan validasi format, karena grouped editor tidak punya pemilih jam dan menambahkannya berarti mengubah komponen bersama. Tanggal memakai pemilih tanggal yang ada. Bawaannya waktu sekarang. |
| Filter select bergantung pasien | Grouped editor membaca `field.filters`, bukan cascade registry. `patientId` disuntikkan lewat `normalizeField`, dan selama pasien belum dipilih select tidak memanggil lookup sama sekali (sebelum perbaikan: dua 400 per form). |
| URL rincian | Id permintaan langsung (`/requests/[requestId]`), sesuai peta rute desain; anti-IDOR dijaga backend |
| Tautan ke episode | Rincian menampilkan nomor episode; tautan ke FE-RHB-05 menunggu `FE-RHB-004` (route belum ada) |
| Tombol rincian | Mengikuti `availableActions` dari server (status) **dan** `usePermission` per butir |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas seluruh berkas rehab + registry select + `menu-items.jsx` | 0 error, 2 warning (baseline FE-RHB-002, identik dengan modul rujukan `job-level`). Satu warning baru (`setState` di effect pada hook editor) sudah diperbaiki menjadi nilai turunan. Warning di `use-select-resource.jsx` (6) milik berkas bersama yang tidak disentuh. |
| Grep anti-regresi UI | `<button>`/kelas `btn` mentah 0; `<table>` mentah 0; kelas `fw-*`/`fs-*` 0; `style={{` 0; tidak ada CSS baru (kelas `base-grouped-editor` dan CSS rehab dari FE-RHB-002 dipakai ulang) |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) | 0 |
| Review diff | 5 berkas existing berubah (+108, termasuk perubahan FE-RHB-002); sisanya berkas baru di folder rehab |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

Backend Debug `https://localhost:7184` + `next dev` port 3000, Playwright Chromium dari scratchpad,
2026-11-06. Respons jaringan direkam lewat `page.on("response")` sebagai pengganti devtools.
Layanan uji `RHB_UJI_10722EA2` diaktifkan sementara supaya form punya layanan untuk dipilih.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Daftar: 9 permintaan, kolom sesuai skema; filter status *Diajukan* → semua baris *Diajukan* | PASS |
| 2 | Rincian permintaan *Diterima*: hanya *Kembali*; riwayat 3 baris | PASS |
| 3 | Form: lookup saat dibuka hanya `doctors` dan `disciplines`; pasien baru dicari setelah ≥3 huruf; kartu pasien tampil | PASS |
| 4 | **AC4**: kunjungan dipilih → pasien diganti → kunjungan kosong lagi, dan request kunjungan berikutnya membawa `patientId` pasien baru, bukan pasien lama | PASS ×3 |
| 5 | **UAT-02**: bagian duplikasi muncul otomatis; *Ajukan* tanpa alasan → `submit` 409, tabel kandidat berisi episode, field alasan ditandai; isi alasan → *Ajukan* → `submit` 200 dengan **1 POST + 1 PUT** (draf tidak dibuat dua kali) | PASS ×4 |
| 6 | **UAT-01**: halaman berpindah ke rincian bernomor `RHB-REQ-2026-0000xx` berstatus *Diajukan* | PASS |
| 7 | Rincian *Diajukan* (SuperAdmin): *Kembalikan, Minta Klarifikasi, Terima, Alihkan, Tolak, Batalkan* tampil, *Ubah/Ajukan* tidak; *Minta Klarifikasi* + alasan → *Perlu klarifikasi*, riwayat 3 baris | PASS ×2 |
| 8 | **AC5**: modal *Batalkan* diisi di tab A, permintaan dibatalkan di tab B, lalu tab A mengonfirmasi → 409; modal tetap terbuka, alasan "Pasien pindah kota (uji isian tetap)" utuh, tombol *Muat Ulang Versi Terbaru* tampil; sesudah muat ulang, status terbaru (*Dibatalkan*) terbaca dan alasan masih ada | PASS ×3 |
| 9 | **AC3 / UAT-03**: akun uji `RehabRequest : Read` + `Return`, permintaan *Diajukan* → hanya *Kembalikan* yang tampil; tanpa *Buat Permintaan* | PASS ×2 |
| 10 | **AC6**: SuperAdmin dan akun Read+Return melihat blok *Rehab Medik* → *Permintaan Rehab*; akun tanpa butir → halaman *Ups! Akses Ditolak* | PASS ×3 |
| 11 | **AC6** akun tanpa butir: *Permintaan Rehab* dan blok *Rehab Medik* tidak ada di menu | FAIL di uji lengkap (akun yang sama dengan langkah 9, sesudah kebijakan dikosongkan). **Tidak terulang** pada 3 pengulangan dengan akun baru, termasuk urutan Read+Return → kosong yang sama persis dan dengan menunggu respons `/auth/permissions`: ketiga label tidak ada. Diagnosa: di halaman `/` grup menu terlipat, sehingga ada atau tidaknya teks butir anak tidak bisa diandalkan sebagai bukti; uji lengkap membaca sidebar 2,5 detik setelah halaman dimuat, tanpa menunggu respons izin. |
| 12 | Riwayat status berlabel Bahasa Indonesia (*Buat draf*, *Diajukan → Perlu klarifikasi*, …) sesudah perbaikan | PASS |

Rekap: uji lengkap **19/20**, pengulangan AC6 **3/3**, cek riwayat **1/1**.

Perbaikan yang lahir dari uji: (a) dua 400 karena select kunjungan/surat memuat tanpa `patientId`,
(b) `patientId` tidak ikut ke filter grouped editor, (c) label riwayat masih nama enum. Ketiganya
diperbaiki di source lalu diuji ulang. Dua kegagalan skrip lain (memilih opsi placeholder dan
selector textarea) diperbaiki di skrip.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-01 lewat layar | Terpenuhi | §5 #5–6 |
| (2) UAT-02: 409 membuka bagian duplikasi berisi kandidat, dan isian alasan dikirim ulang | Terpenuhi | §5 #5 |
| (3) UAT-03: tombol Tolak tidak tampil tanpa `Disposition` | Terpenuhi | §5 #9 |
| (4) Pilihan kunjungan hanya milik pasien terpilih; ganti pasien mengosongkan kunjungan | Terpenuhi | §5 #4, jaringan |
| (5) 409 basi menawarkan muat ulang tanpa membuang isian | Terpenuhi untuk aksi di rincian (§5 #8). Di form, mekanismenya sama (`staleNotice` + *Muat Ulang Versi Terbaru* tanpa menimpa isian) dan terbukti lewat kode, tetapi tidak dijalankan di browser. | §5 #8; `use-rehab-request-editor.js` |
| (6) Menu r2: blok tingkat 0 *Rehab Medik* (`subMenu`) + *Permintaan Rehab* ber-`requiredPermission`; blok hilang tanpa anak yang tampil | Terpenuhi | §5 #10–11 |
| Verifikasi kartu: browser + jaringan | Terpenuhi | §5 |
| DoD: kriteria, laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan modul lain (tidak diubah) | (a) `BaseGroupedEditorField` memakai cascade filter yang dikodekan per resource (`positions`, `cities`, …), bukan `SELECT_RESOURCE_CASCADE` registry seperti `BaseEditorField`. Fitur lain dengan select bergantung field di grouped editor akan mengalami hal yang sama. (b) Grouped editor belum punya pemilih jam. (c) Footer tetap aplikasi menutupi bagian bawah konten (sudah dicatat di FE-RHB-002). |
| Data uji di `QuilvianNewDevSukma` | Dua permintaan baru dari uji (`b8523deb…`, dibuat lewat layar, berakhir *Dibatalkan*; `RHB-REQ-2026-000011`, dibuat lewat API untuk uji hak, *Dibatalkan*). Layanan uji `RHB_UJI_10722EA2` dinonaktifkan kembali. Kebijakan *Pendaftaran / Admission Officer* dikosongkan. **Empat** akun uji `rhb-uji-…@example.test` (uji lengkap + 3 pengulangan AC6) semuanya sudah dinonaktifkan; password awalnya tidak pernah dicetak atau disimpan. |
| Server | Backend dan `next dev` sudah dimatikan; port 7184 dan 3000 kosong |
| Status Git FE | `M` `menu-items.jsx`, `store.jsx`, `clinical-instrument-utils.js`, `health-service-select-resources.js`, `select-resource-registry.js`; `??` folder rehab |
| Dependency backend | `BE-RHB-006` ✅, `BE-RHB-007` ✅ |
| Langkah berikutnya | `FE-RHB-004` (daftar episode + kerangka ruang kerja; `BE-RHB-008` ✅) kini 🟡 |
