# Laporan Perubahan Frontend — `FE-RHB-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-011` |
| Judul | Ringkasan akhir (FE-RHB-13): daftar prasyarat bertaut, isian ringkasan, tanda tangan dengan peringatan penutupan |
| Slice | FS-E — Reasesmen dan ringkasan akhir |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-011` (r2) |
| Trace | FR-RHB-050..052; FEAT-026, 027; V-DIS-01, V-DIS-02; UAT-13, UAT-14; `03-frontend-architecture.md` skema FE-RHB-13 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Discharge Summary* (`BE-RHB-015` ✅) |
| Dependency | `FE-RHB-008` ✅, `BE-RHB-015` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright: 14/14. |

## 1. Masalah yang diselesaikan

Episode rehab ditutup lewat ringkasan akhir: alasan penutupan, layanan utama, capaian tujuan, kondisi
akhir, dan tindak lanjut. Episode hanya boleh ditutup bila tidak ada lagi catatan yang belum final.
Begitu ditandatangani, episode tertutup dan tidak dapat dibuka kembali.

**Contoh.** Dokter membuka episode `RHB-EPS-2026-000007` dan menekan *Susun Ringkasan Akhir*. Bagian
*Prasyarat Penutupan* langsung menampilkan "Sesi terapi RHB-SES-2026-000012 — Berlangsung" dengan
tombol *Buka Dokumen*. Ia mengisi:
- alasan *Selesai secara klinis*;
- layanan utama, capaian tujuan, dan kondisi akhir;
- tindak lanjut *Kontrol*, beserta instruksinya.

Ia menekan *Tanda Tangan dan Tutup Episode*, dan modal memperingatkan "Episode akan ditutup dan tidak
dapat dibuka kembali". Server menolak: "Masih ada catatan yang belum final". Draft tetap tersimpan, dan
daftar menunjukkan sesi tadi. Setelah terapis menandatangani sesinya dan reasesmen *Selesai* final,
daftar prasyarat kosong. Tanda tangan kini berhasil: episode *Ditutup*, program *Selesai*, dan semua
tombol tulis di ruang kerja hilang.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-13: prasyarat (daftar draft dan sesi berjalan, bertaut); alasan penutupan; layanan utama; capaian tujuan; kondisi akhir; tindak lanjut (jenis, instruksi, tujuan, tanggal); tanda tangan dengan `confirm-modal` yang menyebut "Episode akan ditutup dan tidak dapat dibuka kembali". Satu halaman `episodes/[episodeId]/discharge` adalah `DEV_DISCRETION`. |
| Izin owner | "lanjutkan" (2026-11-09), batas sama: ESLint dan uji browser (akun uji dinonaktifkan sesudahnya). Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND` (§3.2) |
| Server uji | Memakai backend dan `next dev` yang sudah berjalan sejak 11:46 (bukan dijalankan task ini); tidak dihentikan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-discharge.service.js` | Baru | `GET`/`PUT`/`sign` per episode |
| `lib/constants/.../discharge/rehab-discharge-constants.js` | Baru | Pilihan alasan penutupan dan tindak lanjut (label sama dengan server), peta jenis catatan belum final → halaman, batas panjang, isian dan kelompok form |
| `utils/.../discharge/rehab-discharge-utils.js` | Baru | Pemetaan ringkasan ↔ form, payload, validasi draft dan V-DIS-01, pemetaan galat, pembaca daftar catatan belum final (`errors.details`) |
| `lib/hooks/.../discharge/use-rehab-discharge.js` | Baru | Controller: ringkasan + prasyarat, simpan, tanda tangan (simpan dulu bila berubah), buka dokumen prasyarat, addendum (jenis 51) |
| `components/view/.../discharge/rehab-discharge-view.jsx` | Baru | Bagian prasyarat + form (`BaseGroupedEditorView`) atau baca saja (`BaseDetailView`) + modal penutupan |
| `app/health-services/rehabilitation-management/episodes/[episodeId]/discharge/page.jsx` | Baru | Route tipis (id episode UUID) |
| `components/features/.../programs/rehab-workspace-program-sections.jsx` | Diperbarui | `WorkspaceDischargeSection`: status ringkasan + tombol *Susun* / *Lanjutkan* (aksi `edit-discharge-summary`) atau *Lihat Ringkasan Akhir*. Menu aksi tujuan disembunyikan bila episode *Ditutup*. |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js`, `components/view/.../episodes/rehab-episode-workspace-view.jsx` | Diperbarui | Memasang bagian Ringkasan Akhir. Kerangka "hitungan + Belum ada …" dari `FE-RHB-004` (`WorkspaceDocumentSection`) dihapus karena semua bagian kini punya isinya sendiri. |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Form penutupan, ringkasan, tindak lanjut (isian bersyarat) | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` (`shouldHideField`) |
| 2 | Prasyarat penutupan bertaut | `COMPOSE` | `BaseDetailSection` + `DataTable` + `StatusBadge` + `BaseButton` *Buka Dokumen* |
| 3 | Peringatan penutupan | `REUSE` | `ConfirmModal` (`variant="warning"`) |
| 4 | Baca saja + addendum | `REUSE`/`COMPOSE` | `BaseDetailView`, `RehabDocumentAddendumPanel` |
| 5 | Bagian Ringkasan Akhir di ruang kerja | `COMPOSE` | `BaseDetailSection` + `BaseButton` |

**Pilihan untuk elemen `COMPOSE` (direkomendasikan dan dipakai):**

1. **Rakit dari base component yang ada** *(direkomendasikan)*. Konsisten dengan layar rehab lain, tanpa
   regresi base component.
2. Membuat panel prasyarat khusus (checklist bertanda). Lebih menonjol, tetapi menambah gaya paralel dan
   butuh persetujuan `NEW`.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 2 COMPOSE (opsi 1).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Prasyarat | Ditampilkan sejak halaman dibuka (`pendingDocuments` dari `GET`), dan diperbarui dari 409 `RHB-VAL-060` (`errors.details`) saat tanda tangan ditolak. Setiap baris membuka dokumennya: asesmen medis/profesi, edukasi, program, sesi, reasesmen. |
| Tanda tangan | Menyimpan perubahan lebih dulu (`PUT`, 201 pada simpan pertama), lalu menandatangani. Ditolak = draft tetap tersimpan dan episode tetap terbuka. |
| Isian bersyarat | Instruksi dan tanggal tindak lanjut tampil (instruksi wajib) bila ada tindak lanjut. Tujuan rujukan tampil dan wajib untuk *Rujuk balik* / *Rujuk ke fasilitas lain*. |
| Episode belum diasesmen | Aturan alasan (hanya meninggal / menolak / pindah / putus layanan) dibiarkan dari server; pesannya tampil sebagai galat aksi dan menandai isian alasan |
| AC 3 | Tombol buat dokumen di ruang kerja sudah mengikuti `availableActions` episode, yang kosong saat *Ditutup*. Menu aksi tujuan dihitung server dari status tujuan saja, sehingga layar menyembunyikannya sendiri saat episode *Ditutup*. |
| Bagian kerangka | Bagian "hitungan + Belum ada …" (`FE-RHB-004`) tidak dipakai lagi dan dihapus |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas folder rehab `view`, `features`, `hooks` (discharge, episodes), `constants`/`utils` discharge, `services`, `app` | 0 error, 0 warning |
| Grep anti-regresi UI di berkas baru (hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. Tidak ada CSS baru. |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | Berkas existing yang berubah: hook + view ruang kerja (kerangka bagian dihapus), komponen bagian ruang kerja. Akhir baris LF dipertahankan, sama dengan berkas rehab lain. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium.

**Data uji:** episode baru `RHB-EPS-2026-000007` disiapkan lewat API dengan akun uji:
- rujukan eksternal diterima;
- asesmen medis dan profesi A final;
- tujuan, lalu program aktif;
- satu sesi *Berlangsung*.

Akun uji berprofesi Dokter Umum dengan kebijakan seluruh aksi 11 resource rehab + addendum (36 aksi).
Episode `RHB-EPS-2026-000006` tidak dipakai, karena masih menyimpan sesi residu.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Setup API: episode *Terapi berjalan*, program aktif, sesi `RHB-SES-2026-000012` berlangsung | PASS |
| 2 | Ruang kerja: bagian Ringkasan Akhir dengan tombol *Susun Ringkasan Akhir* | PASS |
| 3 | Prasyarat: "Sesi terapi RHB-SES-2026-000012 — Berlangsung" dengan *Buka Dokumen* | PASS |
| 4 | Isian bersyarat: instruksi/tujuan tersembunyi saat *Tanpa tindak lanjut*; *Kontrol* memunculkan instruksi saja | PASS |
| 5 | Tanda tangan tanpa instruksi kontrol → ditahan di layar, tanpa request | PASS |
| 6 | Modal memperingatkan "Episode akan ditutup dan tidak dapat dibuka kembali" | PASS |
| 7 | **UAT-14**: `PUT` 201 (draft), `sign` 409; pesan "Masih ada catatan yang belum final"; daftar memuat sesi; episode tetap terbuka | PASS |
| 8 | **UAT-14 / AC2**: *Buka Dokumen* pada baris sesi → membuka sesi itu | PASS |
| 9 | API: sesi diselesaikan + ditandatangani; reasesmen keputusan *Selesai* final | PASS |
| 10 | Prasyarat kosong: "Semua catatan sudah final. Episode siap ditutup." | PASS |
| 11 | Draft tersimpan dimuat ulang (ringkasan, instruksi kontrol) | PASS |
| 12 | **UAT-13 / AC1**: tanda tangan 200 → baca saja *Ringkasan: Final*, *Episode: Ditutup*; server: episode *Ditutup*, program *Selesai* | PASS |
| 13 | Baca saja: tanpa isian aktif, tanpa tombol tanda tangan/simpan | PASS |
| 14 | **AC3**: ruang kerja episode *Ditutup* hanya berisi *Kembali*, *Lihat Permintaan*, *Muat Ulang*, *Lihat Ringkasan Akhir*; tanpa tombol buat dokumen, susun ringkasan, atau menu aksi tujuan | PASS |

**Catatan #14.** Pada episode uji, tujuan sudah *Tercapai* oleh reasesmen, sehingga server memang tidak
memberi aksinya. Penyembunyian menu aksi tujuan untuk episode *Ditutup* yang tujuannya masih aktif
terbukti dari kode (`episodeClosed`), belum lewat data.

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 38 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

**Residu data dev:** `RHB-EPS-2026-000007` *Ditutup* (alasan *Selesai secara klinis*), dengan ringkasan
Final (tindak lanjut *Kontrol*) dan program *Selesai*.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-13 lewat layar | Terpenuhi | §5 #10–13. Reasesmen *Selesai* disiapkan lewat API; layarnya teruji di `FE-RHB-010`. |
| (2) UAT-14: daftar catatan draft tampil dan setiap barisnya bertaut | Terpenuhi | §5 #3, #7, #8 |
| (3) Setelah `Closed`, seluruh tombol tulis di ruang kerja hilang | Terpenuhi | §5 #14 (lihat catatan untuk menu tujuan) |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Menu aksi tujuan pada episode tertutup | Uji data dengan tujuan yang masih *Aktif* di episode *Ditutup* belum dilakukan. Dari kode `RhbGoalService`, aksi `revise`/`discontinue` dihitung dari status tujuan saja, tanpa status episode. Apakah `POST` revisi/henti pada episode tertutup ditolak server belum diuji di task ini; yang teruji di `BE-RHB-015` hanya pembuatan tujuan baru (409 `RHB-VAL-023`). Usulan untuk revisi backend: sertakan status episode dalam perhitungan aksi tujuan. |
| Halaman dokumen lain pada episode tertutup | Asesmen, program, sesi, dan reasesmen sudah final atau bertanda baca saja dari `availableActions` server. Tidak ada perubahan layar yang dibutuhkan. |
| Dependency backend | Tidak ada yang baru |
