# Laporan Perubahan Frontend — `FE-RHB-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-005` |
| Judul | Form asesmen medis (FE-RHB-06) dan asesmen profesi (FE-RHB-07), beserta addendum |
| Slice | FS-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-005` (r2) |
| Trace | FR-RHB-011..016; RHB-DEC-005, 006, 017; UAT-04, UAT-05; `03-frontend-architecture.md` skema FE-RHB-06/07 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Medical Assessment* dan *Rehab Discipline Assessment* (`BE-RHB-009` ✅), *Rehab Lookup* `/instruments` dan `/disciplines`, addendum Rekam Medis (`clinical-note-addendums`) |
| Dependency | `FE-RHB-004` ✅, `BE-RHB-009` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning pada berkas task. Uji browser + jaringan Playwright: putaran 1 25/28, putaran ulang 3/4. Dari empat FAIL itu, satu adalah bug aplikasi yang sudah diperbaiki dan diuji ulang, sisanya salah ukur skrip (§5). |

## 1. Masalah yang diselesaikan

Episode rehab sudah punya ruang kerja, tetapi dokter rehab belum bisa mengisi asesmen dari layar,
dan terapis belum bisa mengisi asesmen profesi. Padahal asesmen medis yang final itulah yang
membawa episode dari *Menunggu asesmen medis* ke *Perencanaan* dan menentukan layanan terapi yang
diresepkan.

**Contoh.** Dokter membuka episode `RHB-EPS-2026-000006` dan menekan *+ Asesmen Medis*. Ia mengisi:
- kunjungan `ENC-RSMMC-00165`;
- riwayat, temuan, diagnosis, dan kesimpulan;
- satu pengukuran terstruktur: *ROM fleksi lumbal* bagian *Lumbal*, 40 derajat;
- resep dua layanan, salah satunya dengan kewaspadaan "Hindari fleksi berlebih".

Ia menyimpan draft, lalu menandatanganinya. Asesmen terkunci, dan halaman berubah menjadi baca saja.
Status *Asesmen: Final*, *Rekam Medis: Ditandatangani*, dan *Episode: Perencanaan* tampil sebagai tiga
penanda terpisah. Ia salah ketik nilai ROM, jadi ia menambah addendum "ROM fleksi lumbal 45 derajat".
Isi asli tetap 40, dan addendumnya tercantum di bawah dokumen.

Terapis yang profesinya tidak sesuai dengan layanan akan menerima pesan "Profesi Anda tidak sesuai
dengan layanan ini." di atas form. Pesan itu tidak ditampilkan sebagai layar akses ditolak.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-06/07 `03-frontend-architecture.md`. Satu halaman per asesmen (form untuk draft milik penulis, baca saja untuk selainnya) adalah `DEV_DISCRETION`. |
| Keputusan owner (UI gate) | Renderer instrumen: **bungkus renderer keperawatan yang ada** (opsi yang direkomendasikan), tanpa menyalin dan tanpa mengubah modul rawat inap |
| Izin owner | ESLint dan uji browser (backend + `next dev` + Playwright; akun uji dinonaktifkan sesudahnya), sesuai instruksi task. Tanpa build, tanpa commit. |
| Menyentuh berkas bersama | Tidak ada base component, renderer rawat inap, atau komponen Rekam Medis yang diubah (`git diff` kosong untuk `inpatient-management`, `medical-record-management`, `base-features`) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-assessment.service.js` | Baru | 6 fungsi untuk kedua jenis asesmen: daftar per episode, rincian, buat, ubah, tanda tangan, batal. Jenis dipilih lewat `"medical"` atau `"discipline"`. |
| `lib/services/.../rehab-lookup.service.js` | Diperbarui | `getRehabLookupInstruments({ patientId, disciplineId })` |
| `lib/constants/.../assessments/rehab-assessment-constants.js` | Baru | Konfigurasi per jenis (label, Resource hak akses, aksi episode), isian dan kelompok form, batas panjang teks, isian wajib tanda tangan, nada badge status rekam dan status keutuhan RM, peta kode galat → isian |
| `utils/.../assessments/rehab-assessment-utils.js` | Baru | Pemetaan rincian ↔ form, penyusun payload (jawaban instrumen yang kosong tidak dikirim, baris pengukuran kosong diabaikan), validasi draft dan validasi tanda tangan, pemetaan galat server bertingkat (`measurements[0].unit`) ke bagiannya |
| `lib/hooks/.../assessments/use-rehab-assessment.js` | Baru | Controller halaman: muat episode + asesmen, instrumen, layanan aktif, addendum; simpan draft (lalu `router.replace` ke id baru); tanda tangan (simpan dulu bila ada perubahan); batal beralasan |
| `components/features/.../assessments/rehab-assessment-fields.jsx` | Baru | 4 komponen rangkaian: pengukuran terstruktur, resep layanan + kewaspadaan, bungkus renderer instrumen, panel addendum |
| `components/view/.../assessments/rehab-assessment-view.jsx` | Baru | Halaman FE-RHB-06/07: form (`BaseGroupedEditorView`) atau baca saja (`BaseDetailView`), modal tanda tangan, dan modal batal beralasan |
| `style/components/features/rehabilitation-management/rehab-assessment.module.css` | Baru | Tata letak saja (grid baris pengukuran, kartu instrumen, baris status). Hanya token `globals.css`; border `1px solid var(--color-border)` mengikuti konvensi proyek. |
| `app/health-services/rehabilitation-management/episodes/[episodeId]/assessments/[assessmentType]/[assessmentId]/page.jsx` | Baru | Route tipis. Jenis selain `medical`/`discipline`, atau id selain `new`/UUID, dijawab not-found tanpa memanggil API. |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js` | Diperbarui | Memuat daftar kedua jenis asesmen. Satu jenis yang gagal (misalnya 403) tidak menggagalkan yang lain. Ditambah `openAssessment`. |
| `components/view/.../episodes/rehab-episode-workspace-view.jsx` | Diperbarui | Bagian *Asesmen* kini berisi tabel asesmen (klik baris membuka halamannya) dan tombol *+ Asesmen Medis* / *+ Asesmen Profesi*. Tombol hanya tampil bila aksi `create-*-assessment` ada di `availableActions` episode, yang dihitung server dari status episode dan hak akses. |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Form isian teks, select kunjungan dan layanan | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` (select remote `rehabEncounters`) |
| 2 | Daftar pengukuran terstruktur (tambah/hapus baris) | `COMPOSE` | `BaseGroupedEditorField` × 5 per baris + `BaseButton` |
| 3 | Resep layanan + kewaspadaan per layanan | `COMPOSE` | `RehabRequestDisciplinePicker` (`FE-RHB-003`) + `BaseGroupedEditorField` textarea |
| 4 | Instrumen berversi | `WRAP` | `ClinicalInstrumentFormRenderer` keperawatan, dipanggil lewat jalur umumnya (`instrumentKind = 0`, kode/jenis dilucuti dari `resolvedInstrument`), tanpa tombol simpan milik renderer. **Keputusan owner.** |
| 5 | Tampilan baca saja + tabel pengukuran dan resep | `REUSE` / `COMPOSE` | `BaseDetailView`, `BaseDetailSection` + `DataTable` |
| 6 | Status rekam, status keutuhan RM, status episode | `REUSE` | `StatusBadge` × 3 |
| 7 | Addendum | `COMPOSE` | `DataTable` (daftar) + `MedicalRecordAddendumActions` milik RM apa adanya (kewenangan ditanya ke server, modal, penjaga kirim ganda) |
| 8 | Tanda tangan dan batal beralasan | `REUSE` | `ConfirmModal` (`requireReason`) |
| 9 | Galat aksi dan galat muat | `REUSE` | `InformationAlert` untuk aksi; `AccessDeniedGate` hanya untuk galat muat |
| 10 | Bagian Asesmen di ruang kerja | `COMPOSE` | `BaseDetailSection` + `DataTable` + `BaseButton` |

`UI GATE: PASS — 0 NEW, 0 EXTEND, 1 WRAP (keputusan owner), sisanya REUSE/COMPOSE.`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| State | Service + hook per halaman, tanpa slice Redux, mengikuti `FE-RHB-003`/`004` |
| Pratinjau skor | Renderer hanya menghitung skor lokal dari pilihan berskor. Endpoint pratinjau skor tidak dipanggil, karena kontrak rehab tidak menyediakannya; skor resmi dihitung server saat menyimpan. |
| Isian kunjungan | Tidak memakai `dependsOn`. Pasien datang dari episode, bukan dari isian form, dan grouped form menonaktifkan isian ber-`dependsOn` bila isian rujukannya kosong. Saringan pasien dikirim lewat `filters.patientId`. |
| Kunjungan dan layanan setelah draft dibuat | Ditampilkan nonaktif dengan alasan "Tidak dapat diganti setelah draft dibuat." |
| Tombol buat di ruang kerja | Mengikuti `availableActions` episode dari server (status + hak akses), bukan `usePermission` di klien saja. Contoh: *+ Asesmen Medis* hilang setelah asesmen medis ada, *+ Asesmen Profesi* baru muncul saat episode *Perencanaan*. |
| Bagian lain ruang kerja | Tidak berubah; masih hitungan + "Belum ada …" sampai task layarnya |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas seluruh berkas §3.1 serta folder rehab `app`, `components`, `hooks`, `utils`, `constants`, `services` | 0 error, 0 warning pada berkas task. Dua warning `react-hooks/set-state-in-effect` muncul di hook master-data layanan milik `FE-RHB-002`; berkas itu tidak disentuh task ini. |
| Grep anti-regresi UI di berkas baru (warna hex/`rgba`, `style={{`, `font-size`/`font-weight`) | 0. Nilai `px` yang tersisa hanya border 1px, `minmax(280px)` grid, dan breakpoint 900px; ketiganya tidak punya token dan pola yang sama dipakai CSS rehab `FE-RHB-002`. |
| Token CSS dipakai ada di `globals.css` | `--space-3`, `--space-4`, `--radius-md`, `--color-surface`, `--color-text-muted`, `--color-border`: semua terdefinisi. Fallback ke `--color-border-subtle` dibuang, karena token itu tidak terdefinisi. |
| Review diff | 3 berkas existing berubah (lihat §3.1); sisanya berkas baru di folder rehab. Kode rawat inap, Rekam Medis, dan base component tidak berubah. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium dijalankan dari scratchpad, respons jaringan direkam lewat `page.on("response")`.

**Data uji:**
- episode baru `RHB-EPS-2026-000006`, dibuka lewat API dari rujukan eksternal;
- layanan uji A *Uji BE-RHB-007* (profesi Dokter Umum) dan B *Uji BE-RHB-009 B* (profesi Perawat), diaktifkan sementara;
- akun uji berprofesi Dokter Umum, dengan kebijakan seluruh aksi *RehabEpisode*, *RehabLookup*, *RehabMedicalAssessment*, *RehabDisciplineAssessment*, dan *ClinicalNoteAddendum*.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Ruang kerja *Menunggu asesmen medis*: hanya *+ Asesmen Medis* tampil; bagian kosong berbunyi "Belum ada asesmen." | PASS ×2 |
| 2 | Form medis memuat bagian Konteks, Riwayat dan Temuan, Pengukuran, Penilaian Klinis, Resep Layanan; isian *Layanan* (khusus asesmen profesi) tersembunyi | PASS ×2 |
| 3 | Instrumen: lookup dev tidak punya instrumen rehab yang disahkan, dan layar menampilkan info "Instrumen belum dapat dipakai" beserta alasannya | PASS |
| 4 | Tanda tangan dengan isian kosong ditahan di layar (pesan, isian wajib ditandai, "Resepkan minimal satu layanan terapi") tanpa request ke server | PASS |
| 5 | **UAT-04**: kunjungan dari select (22 kunjungan terbuka pasien episode), teks, 1 pengukuran, resep A + B, dan kewaspadaan A. *Simpan Draft* → `POST` 201, URL diganti ke id asesmen. | PASS |
| 6 | Server menyimpan teks, pengukuran 40, 2 layanan, dan kewaspadaan A; muat ulang halaman mengisi form kembali | PASS ×2 |
| 7 | **AC3** (draft): *Asesmen: Draf*, *Rekam Medis: Draf*, *Episode: Menunggu Asesmen Medis* tampil terpisah; tombol penulis Kembali/Batalkan Draft/Simpan Draft/Tanda Tangan | PASS ×2 |
| 8 | **UAT-04**: ubah prognosis lalu *Tanda Tangan* → `PUT` 200 lalu `sign` 200 | PASS |
| 9 | **UAT-05**: setelah final, halaman baca saja: 0 isian aktif, tanpa tombol Simpan/Tanda Tangan; isi, pengukuran "40 derajat", dan resep tampil | PASS ×2 |
| 10 | **AC3** (final): *Asesmen: Final*, *Rekam Medis: Ditandatangani*, *Episode: Perencanaan* | PASS |
| 11 | **UAT-05**: *Tambah addendum* tampil untuk penulis. Isi + alasan → `POST` 201, dan addendum tercantum di daftar. Isi asli di server tetap 40 dan Final. | PASS ×3 |
| 12 | Ruang kerja *Perencanaan*: *+ Asesmen Profesi* tampil, *+ Asesmen Medis* hilang. Daftar memuat asesmen medis Final dan asesmen profesi; klik baris membuka halamannya. | PASS ×2 (putaran ulang) |
| 13 | Form profesi: isian *Layanan* tampil, bagian Resep tidak; opsi layanan = 2 layanan yang diresepkan asesmen medis final | PASS ×2 |
| 14 | **AC4**: layanan B (Perawat) oleh akun Dokter Umum → `POST` 403. Pesan "Profesi Anda tidak sesuai" tampil di atas form, isian tidak hilang, dan layar akses ditolak tidak muncul. | PASS |
| 15 | Ganti ke layanan A → `POST` 201; layanan dikunci setelah draft dibuat | PASS ×2 |
| 16 | *Batalkan Draft*: tombol konfirmasi nonaktif sebelum alasan diisi → `POST cancel` 200 → baca saja *Asesmen: Dibatalkan* (server: Dibatalkan) | PASS (putaran ulang) |
| 17 | Pengguna lain (SuperAdmin) membuka asesmen final → baca saja, addendum tercantum | PASS |
| 18 | Rute jenis tidak dikenal (`assessments/unknown/new`) dan id bukan UUID → halaman not-found aplikasi, 0 panggilan API rehab | PASS (diperiksa terpisah, sesi login) |

**Kegagalan dan penanganannya:**

| Putaran | FAIL | Penyebab | Penanganan |
| --- | --- | --- | --- |
| 0 | Select kunjungan tidak dapat diklik | **Bug aplikasi.** Isian kunjungan memakai `dependsOn: "patientId"`, dan grouped form menonaktifkannya karena form asesmen tidak punya `patientId`. | `dependsOn` dihapus; saringan tetap lewat `filters.patientId` (§3.3). Putaran 1 lulus pada butir ini. |
| 1 | Daftar ruang kerja tidak memuat "Asesmen Medis … Final" | Skrip mencari huruf besar, sedangkan server mengirim "Asesmen medis" | Pola skrip diperbaiki → PASS (#12) |
| 1 | *Batalkan Draft* tidak mengirim request | **Bug aplikasi.** `ConfirmModal` memanggil `onConfirm(event, alasan)`, tetapi view meneruskan `confirmCancel` langsung, sehingga yang diterima sebagai alasan adalah objek event. | View diubah menjadi `(_event, reason) => confirmCancel(reason)` → PASS (#16) |
| 1, ulang | Rute tidak valid membalas HTTP 200, bukan 404 | `not-found.jsx` aplikasi (komponen *COMING SOON*) tampil dengan status 200 di `next dev`. Rute episode milik `FE-RHB-004` berperilaku sama, jadi ini perilaku tingkat aplikasi. | Diperiksa ulang lewat isi halaman: not-found tampil dan 0 panggilan API → PASS (#18) |

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 30 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

Kedua server dihentikan.

**Residu data dev** (episode uji, dibiarkan):
- `RHB-EPS-2026-000006` berstatus *Perencanaan*;
- asesmen medis Final, keutuhan Ditandatangani, dengan 1 addendum;
- 1 asesmen profesi Dibatalkan;
- 1 draft asesmen profesi dari putaran 1. Penulisnya akun uji yang sudah dinonaktifkan, jadi draft ini hanya dapat dibatalkan oleh penulisnya.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-04 | Terpenuhi | §5 #5–8: asesmen + resep dua layanan ditandatangani, episode menjadi *Perencanaan* |
| (2) UAT-05: setelah final, form baca saja dan addendum berhasil | Terpenuhi | §5 #9, #11, #17. Penolakan `PUT` server pada dokumen final (409 `RHB-VAL-021`) sudah dibuktikan di `BE-RHB-009`; layar tidak menyediakan jalan untuk mengubahnya. |
| (3) Status keutuhan tampil terpisah dari status episode | Terpenuhi | §5 #7, #10 |
| (4) 403 `RHB-VAL-031` tampil sebagai pesan, bukan layar kosong | Terpenuhi | §5 #14 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Jalur renderer berisi instrumen belum teruji di browser | DB dev belum punya versi instrumen rehab (jenis 8–13) yang disahkan, jadi yang teruji hanya jalur info "belum dapat dipakai". Penyusunan payload instrumen (`source: 1`, `instrumentVersionId`, `answers`) sudah diperiksa di kode. Uji ulang singkat disarankan setelah instrumen rehab pertama disahkan di Konfigurasi Klinis. |
| Renderer keperawatan dipakai lintas modul | Perubahan pada `ClinicalInstrumentFormRenderer` di modul rawat inap kini ikut berdampak ke asesmen rehab. Jalur umumnya (`instrumentKind = 0`) adalah satu-satunya yang dipakai. |
| Status 404 aplikasi | `not-found` aplikasi membalas 200 di `next dev` (perilaku seluruh aplikasi, di luar cakupan) |
| Draft residu | Lihat §5, pembersihan |
| Dependency backend | Tidak ada yang baru. `FE-RHB-007` kini hanya menunggu dirinya sendiri dikerjakan (`BE-RHB-011` ✅). |
