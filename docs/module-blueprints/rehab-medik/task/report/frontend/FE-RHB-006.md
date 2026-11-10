# Laporan Perubahan Frontend — `FE-RHB-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-006` |
| Judul | Form edukasi dan persetujuan (FE-RHB-09), beserta daftar edukasi di ruang kerja episode |
| Slice | FS-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-006` (r2) |
| Trace | FR-RHB-020; FEAT-015; V-EDU-01; `03-frontend-architecture.md` skema FE-RHB-09 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Education* (`BE-RHB-010` ✅), *Rehab Program* `GET /by-episode` (baca saja, `BE-RHB-011` ✅), addendum Rekam Medis (`clinical-note-addendums`) |
| Dependency | `FE-RHB-004` ✅, `BE-RHB-010` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright: putaran 1 15/16, putaran ulang 2/2. FAIL putaran 1 berasal dari cara hitung skrip (§5). |

## 1. Masalah yang diselesaikan

Sebelum program terapi diaktifkan, pasien atau keluarga harus diberi edukasi tentang rencana
rehabilitasi, dan keputusan mereka dicatat: setuju, setuju sebagian, menolak, atau tidak memerlukan
persetujuan. Backend sudah menyediakan pencatatan ini (`BE-RHB-010`), tetapi belum ada layar untuk
mengisinya.

**Contoh.** Terapis membuka episode `RHB-EPS-2026-000006` dan menekan *+ Edukasi dan Persetujuan*.
Ia mengisi:
- kunjungan;
- materi "Tujuan program fisioterapi dan latihan di rumah", dengan metode lisan dan demonstrasi;
- penerima *Keluarga Uji* (istri pasien);
- status persetujuan *Menolak*.

Layar langsung meminta catatan persetujuan karena pasien menolak. Terapis mengisi catatan "Menolak
latihan beban; setuju latihan jalan", lalu menekan *Tanda Tangan*. Server menolak tanda tangan itu:
"Nilai tingkat pemahaman penerima edukasi sebelum menandatangani." Pesan itu tampil di atas form,
dan draft yang baru dibuat tidak hilang. Terapis memilih *Memahami* dan menandatangani ulang.
Catatan menjadi final, lalu terkunci di Rekam Medis.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-09 `03-frontend-architecture.md`: isian program terkait, materi, penerima dan hubungan, metode, tingkat pemahaman, hambatan, status persetujuan, catatan; tombol Simpan dan Tanda tangan. Pola satu halaman (form untuk draft milik pemberi edukasi, baca saja untuk selainnya) mengikuti `FE-RHB-005`, sebagai `DEV_DISCRETION`. |
| Izin owner | ESLint dan uji browser (backend + `next dev` + Playwright; akun uji dinonaktifkan sesudahnya), dengan batas yang sama seperti `FE-RHB-005`. Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND`, jadi tidak ada keputusan yang perlu menunggu owner (§3.2) |
| Menyentuh berkas bersama | Tidak ada base component atau komponen Rekam Medis yang diubah. Panel addendum dipakai ulang dari `FE-RHB-005`. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-education.service.js` | Baru | Daftar per episode (saringan `programId` opsional), buat, ubah, tanda tangan |
| `lib/services/.../rehab-program.service.js` | Baru | Hanya `getRehabProgramsByEpisode`, untuk pilihan program terkait. Fungsi tulis program ditambahkan `FE-RHB-007` di berkas yang sama. |
| `lib/constants/.../education/rehab-education-constants.js` | Baru | Pilihan tingkat pemahaman dan status persetujuan (label sama dengan nama dari server), nada badge persetujuan, batas panjang, isian wajib, isian dan kelompok form |
| `utils/.../education/rehab-education-utils.js` | Baru | Pemetaan catatan ↔ form, payload, validasi isian wajib, catatan wajib bila *Menolak*/*Setuju sebagian*, batas panjang, pemetaan `fields` galat server ke isian |
| `lib/hooks/.../education/use-rehab-education.js` | Baru | Controller halaman. Catatan dibaca dari daftar per episode, karena kontrak tidak punya `GET /{id}`. Ada program, addendum RM jenis 52, simpan draft, dan tanda tangan (simpan dulu bila ada perubahan). |
| `components/view/.../education/rehab-education-view.jsx` | Baru | Form (`BaseGroupedEditorView`) atau baca saja (`BaseDetailView`) + panel addendum + modal tanda tangan |
| `app/health-services/rehabilitation-management/episodes/[episodeId]/education/[educationId]/page.jsx` | Baru | Route tipis. Id selain `new`/UUID dijawab not-found tanpa memanggil API. |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js` | Diperbarui | Memuat daftar edukasi (kegagalannya tidak menjatuhkan halaman); ditambah `openEducation` |
| `components/view/.../episodes/rehab-episode-workspace-view.jsx` | Diperbarui | Bagian *Edukasi dan Persetujuan* berisi tabel catatan (klik baris membuka halamannya) dan tombol *+ Edukasi dan Persetujuan*. Tombol hanya tampil bila `create-education` ada di `availableActions` episode. |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Form isian teks, select kunjungan (remote `rehabEncounters`), select program/pemahaman/persetujuan (opsi statis) | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` |
| 2 | Catatan persetujuan wajib bersyarat | `REUSE` | `normalizeField` menyalakan `required` saat *Menolak*/*Setuju sebagian* |
| 3 | Tampilan baca saja | `REUSE` | `BaseDetailView` |
| 4 | Status catatan, keutuhan RM, persetujuan, episode | `REUSE` | `StatusBadge` × 4 |
| 5 | Addendum | `REUSE` | `RehabDocumentAddendumPanel` (`FE-RHB-005`: `DataTable` + `MedicalRecordAddendumActions` RM) |
| 6 | Tanda tangan | `REUSE` | `ConfirmModal` |
| 7 | Galat aksi / galat muat | `REUSE` | `InformationAlert` / `AccessDeniedGate` |
| 8 | Bagian edukasi di ruang kerja | `COMPOSE` | `BaseDetailSection` + `DataTable` + `BaseButton` (pola sama dengan bagian Asesmen `FE-RHB-005`) |

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 1 COMPOSE (pola yang sudah dipakai FE-RHB-005).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Rincian catatan | Kontrak tidak menyediakan `GET /{id}`, jadi halaman mencari catatan pada `GET /by-episode/{episodeId}`. Id yang tidak ada pada episode itu menampilkan "Catatan edukasi tidak ditemukan pada episode ini." |
| Waktu edukasi | Tidak dijadikan isian. Server mengisinya dengan waktu saat ini, dan isian tanggal grouped editor tidak memuat jam. Waktu tampil di baca saja dan di daftar. |
| Tanpa tombol batal | Kontrak tidak menyediakan `cancel` untuk edukasi |
| Pemahaman "Belum dinilai" | Diizinkan untuk draft. Penolakan saat tanda tangan sengaja diserahkan ke server (`RHB-VAL-083`), supaya pesannya hanya punya satu sumber (AC 2). |
| Tanda tangan pertama ditolak | Draft yang baru dibuat tetap ada. Simpan berikutnya memakai `PUT` (ditentukan oleh `detail.id`, bukan oleh rute), sehingga tidak terbentuk catatan kembar. URL dipindah ke id draft lewat `history.replaceState` agar halaman tidak dipasang ulang dan pesan penolakan tetap tampil. |
| Pilihan program | Semua program episode kecuali yang dibatalkan, berlabel "Layanan vN · status". Tanpa `RehabProgram : Read` daftar kosong, tetapi edukasi umum tetap bisa dicatat. Apakah program *Dihentikan*/*Digantikan* perlu disembunyikan adalah `DEV_DISCRETION` (server menerima semua program episode). |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas seluruh berkas §3.1 serta folder rehab `view`, `services`, `app/episodes` | 0 error, 0 warning (satu warning `exhaustive-deps` di hook diperbaiki sebelum uji) |
| Grep anti-regresi UI di view baru dan view ruang kerja (warna hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. Tidak ada CSS baru; kelas tata letak dari `rehab-assessment.module.css` (`FE-RHB-005`) dipakai ulang. |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | 2 berkas existing berubah (hook dan view ruang kerja); sisanya berkas baru di folder rehab |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium dijalankan dari scratchpad, respons jaringan direkam lewat `page.on("response")`.

**Data uji:**
- episode `RHB-EPS-2026-000006` (*Perencanaan*, sisa uji `FE-RHB-005`, belum punya catatan edukasi);
- `RHB-EPS-2026-000001` untuk pilihan program (5 program);
- akun uji berprofesi Dokter Umum, dengan kebijakan seluruh aksi *RehabEpisode*, *RehabLookup*, *RehabEducation*, dan *ClinicalNoteAddendum*, ditambah *RehabProgram : Read*.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | **AC1** (API): *Menolak* tanpa catatan → 422 `RHB-VAL-083`, `fields` = `consentNote`, tanpa catatan terbentuk | PASS |
| 2 | Ruang kerja: tombol *+ Edukasi dan Persetujuan*; bagian kosong berbunyi "Belum ada catatan edukasi." | PASS |
| 3 | Form memuat bagian Konteks, Materi, Penerima dan Pemahaman, Persetujuan; pemahaman bawaan *Belum dinilai* | PASS |
| 4 | **AC1** (layar): *Menolak* tanpa catatan → *Simpan Draft* ditahan dengan "Wajib diisi bila menolak atau setuju sebagian.", tanpa `POST` | PASS |
| 5 | **AC2**: dengan *Belum dinilai*, *Tanda Tangan* → draft `POST` 201, `sign` 422. Pesan server "Nilai tingkat pemahaman penerima edukasi sebelum menandatangani." tampil, form tetap. | PASS |
| 6 | Setelah penolakan, URL pindah ke id draft tanpa memuat ulang halaman | PASS |
| 7 | Pilih *Memahami*, tanda tangan ulang → `PUT` 200 lalu `sign` 200, tanpa `POST` baru | PASS |
| 8 | Server: tepat satu catatan baru, Final, pemahaman 1, persetujuan *Menolak* + catatan | PASS |
| 9 | Setelah final: baca saja (0 isian aktif, tanpa Simpan/Tanda Tangan) | PASS |
| 10 | Status terpisah: *Catatan: Final*, *Rekam Medis: Ditandatangani*, *Persetujuan: Menolak*, *Episode: Perencanaan* | PASS |
| 11 | Baca saja memuat materi, penerima "Keluarga Uji (Istri)", dan catatan persetujuan | PASS |
| 12 | Addendum RM pada edukasi final (jenis 52) → `POST` 201, tercantum di daftar | PASS |
| 13 | Ruang kerja: daftar edukasi memuat catatan baru dengan badge *Menolak* dan status *Final* | PASS |
| 14 | Pilihan *Program Terkait* di `RHB-EPS-2026-000001`: 5 program, sama dengan API, berlabel layanan + versi + status | PASS (putaran ulang) |
| 15 | Pilihan *Tingkat Pemahaman* berlabel sama dengan server (*Memahami*, *Memahami sebagian*, *Belum memahami*, *Belum dinilai*) | PASS (putaran ulang) |
| 16 | Id bukan UUID → halaman not-found aplikasi; UUID yang tidak ada di episode → pesan "Catatan edukasi tidak ditemukan pada episode ini." | PASS ×2 |

**Kegagalan dan perbaikan:**

| Putaran | Temuan | Penyebab | Penanganan |
| --- | --- | --- | --- |
| 1 | FAIL #14: opsi 6, API 5 | Salah hitung skrip. Entri kosong "Edukasi umum rencana episode" milik select ikut terhitung. | Entri kosong dikeluarkan dari hitungan → 5 = 5, PASS |
| 1 | Label pemahaman form *Paham* berbeda dengan daftar (*Memahami*) | Label konstanta tidak mengikuti `UnderstandingLevelName` server | Label disamakan dengan server, lalu diuji ulang (#15) |
| ulang | Select pemahaman menampilkan placeholder bawaan "Masukkan data" | Isian tidak punya `placeholder` | Ditambah "Pilih tingkat pemahaman"; ESLint ulang 0/0 |

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 31 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

Kedua server dihentikan.

**Residu data dev** (dibiarkan): satu catatan edukasi Final di `RHB-EPS-2026-000006`, dengan keutuhan
Ditandatangani, persetujuan *Menolak*, dan 1 addendum.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `Refused` mewajibkan catatan | Terpenuhi | §5 #1 (server), #4 (layar) |
| (2) Tanda tangan dengan "belum dinilai" ditolak dan pesannya tampil | Terpenuhi | §5 #5–7 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Rincian lewat daftar | Episode dengan sangat banyak catatan edukasi membuat halaman rincian memuat seluruh daftar. Bila perlu, `GET /{id}` dapat ditambahkan di backend pada revisi kontrak. |
| Program berstatus akhir di pilihan | Lihat §3.3; dapat dipersempit bila owner menghendaki |
| `rehab-program.service.js` | Dibuat dengan satu fungsi baca; `FE-RHB-007` melengkapinya tanpa membuat berkas kedua |
| Dependency backend | Tidak ada yang baru. Pemblokiran aktivasi program tanpa persetujuan (`RHB-VAL-045`) tampil di `FE-RHB-007`. |
