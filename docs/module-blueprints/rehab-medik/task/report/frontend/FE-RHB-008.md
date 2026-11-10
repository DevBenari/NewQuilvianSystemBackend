# Laporan Perubahan Frontend — `FE-RHB-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-008` |
| Judul | Daftar sesi (FE-RHB-10) dan form sesi terapi (FE-RHB-11), butir menu *Sesi Terapi*, tombol *Mulai Sesi* di program aktif |
| Slice | FS-D — Sesi dan billing |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-008` (r2) |
| Trace | FR-RHB-030..032, 036, 037; FEAT-016..018; RHB-DEC-009, 013; UAT-09, UAT-12; FR-RHB-062 (menu); `03-frontend-architecture.md` skema FE-RHB-10/11 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Session* (`BE-RHB-012` ✅); status handoff billing dibaca dari rincian sesi |
| Dependency | `FE-RHB-007` ✅, `BE-RHB-012` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning pada berkas task. Uji browser + jaringan Playwright putaran akhir: 22/22, ditambah 1/1 uji sesudah perbaikan tampilan. Dua putaran awal berhenti karena kesalahan skrip uji; dampaknya ke data dev dicatat di §5. |

## 1. Masalah yang diselesaikan

Program terapi sudah bisa diaktifkan, tetapi terapis belum punya layar untuk menjalankan sesinya.
Sebelum terapi dimulai, terapis wajib:
- memilih kunjungan pasien yang masih terbuka;
- memverifikasi identitas dan program;
- mencatat kondisi terkini.

Setelah sesi berjalan, terapis mencatat tindakan aktual, respons pasien, dan deviasi, lalu
menyelesaikan, menghentikan dini, atau mencatat sesi tidak terlaksana, dan menandatanganinya.

**Contoh.** Terapis membuka program *Uji BE-RHB-007 v2* di episode `RHB-EPS-2026-000006` dan menekan
*Mulai Sesi*. Pada halaman sesi baru, bagian 2–4 bertuliskan "Terkunci sampai sesi dimulai." Ia
memilih kunjungan, mencentang verifikasi identitas dan program, menulis kondisi terkini, lalu menekan
*Mulai Sesi*. Sesi `RHB-SES-…` terbuka dengan baris tindakan rencana yang sudah terisi.

Di tengah latihan pasien pusing. Terapis menekan *Henti Dini*, dan modal menolak karena *Tindakan
Segera* masih kosong. Setelah ia menulis "pasien dibaringkan, TD 100/70", sesi menjadi *Dihentikan
dini* dan program ditandai *Perlu telaah*. Ia lalu menandatanganinya.

Seandainya kunjungan yang dipilih ternyata sudah ditutup ketika tombol ditekan, pesan "Pilih kunjungan
pasien hari ini yang masih terbuka." tampil di bagian cek keselamatan. Pesan "Pasien sudah pulang dari
rawat inap." tampil di tempat yang sama.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-10 (kolom, saringan) dan FE-RHB-11 (4 bagian, tombol) `03-frontend-architecture.md`; peta menu (butir *Sesi Terapi*). Halaman sesi baru `sessions/new?programId=` dan pemakaian modal untuk henti dini/tidak terlaksana adalah `DEV_DISCRETION`. |
| Izin owner | "lanjutkan" (2026-11-09), batas sama dengan task sebelumnya: ESLint dan uji browser (akun uji dinonaktifkan sesudahnya). Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND` (§3.2) |
| Di luar cakupan | Tombol *Ulang billing* milik `FE-RHB-009` (⛔ menunggu `BE-RHB-013` / `RHB-OQ-015`); task ini hanya menampilkan status billing |
| Server uji | Memakai backend (`QuilvianSystemBackend.exe`, `bin\Debug`) dan `next dev` yang sudah berjalan sejak 11:46, bukan dijalankan task ini. Keduanya tidak dihentikan. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-session.service.js` | Baru | Metadata saringan, ringkasan, daftar, daftar per episode, rincian, mulai, tidak terlaksana, simpan, selesai, henti dini, tanda tangan, batal |
| `lib/constants/.../sessions/rehab-session-constants.js` | Baru | Nada badge status sesi dan billing (`Gagal` = merah), saringan bawaan, pilihan "Sesi saya", kartu ringkasan, pilihan deviasi / henti dini / alasan tidak terlaksana (label sama dengan server), isian dan kelompok 4 bagian, daftar isian yang dikunci |
| `utils/.../sessions/rehab-session-utils.js` | Baru | Pemetaan sesi ↔ form, baris tindakan (diisi dari rencana bila belum ada), payload mulai/simpan, validasi cek keselamatan dan baris tindakan, pemetaan galat, daftar kode galat bagian 1 (`RHB-VAL-050..054`) |
| `lib/hooks/.../sessions/use-rehab-sessions.js` | Baru | Controller daftar sesi (saringan, ringkasan, paginasi, "Sesi saya") |
| `lib/hooks/.../sessions/use-rehab-session.js` | Baru | Controller form sesi: program/sesi/episode, tindakan layanan, addendum (jenis 49), aksi dengan satu bendera `saving` |
| `components/features/.../sessions/rehab-session-fields.jsx` | Baru | Pesan cek keselamatan, baris tindakan aktual, modal henti dini, modal tidak terlaksana |
| `components/view/.../sessions/rehab-sessions-view.jsx` | Baru | FE-RHB-10 |
| `components/view/.../sessions/rehab-session-view.jsx` | Baru | FE-RHB-11 (form) + baca saja |
| `app/health-services/rehabilitation-management/sessions/page.jsx`, `sessions/[sessionId]/page.jsx` | Baru | Route tipis. `new` wajib `programId` UUID, selain itu id sesi UUID; parameter lain dijawab not-found. |
| `utils/menu-sidebar/menu-items.jsx` | Diperbarui (+9) | Butir *Sesi Terapi* (`RiCalendarCheckLine`, sudah diimpor), `requiredPermission: { resource: "RehabSession", action: "Read" }` |
| `lib/hooks/.../programs/use-rehab-program.js`, `components/view/.../programs/rehab-program-view.jsx` | Diperbarui | Tombol *Mulai Sesi* di program aktif. Hanya tampil bila episode memuat aksi `start-session`, yang dihitung server dari status episode dan `RehabSession : Create`. *Revisi Program* diubah ke tombol sekunder supaya *Mulai Sesi* menjadi aksi utama. |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js`, `components/view/.../episodes/rehab-episode-workspace-view.jsx`, `components/features/.../programs/rehab-workspace-program-sections.jsx` | Diperbarui | Bagian *Sesi Terapi* di ruang kerja berisi daftar sesi episode; klik baris membuka sesi |
| `style/components/features/rehabilitation-management/rehab-assessment.module.css` | Diperbarui | `.sessionRow` (tata letak, token saja) |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Daftar sesi: hero, ringkasan, saringan, tabel, badge, paginasi, aksi baris | `REUSE` | `Hero`, `SummaryGrid`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `StatusBadge`, `RowActionMenu`, `RegionPagination` (pola `FE-RHB-004`) |
| 2 | Form 4 bagian, kunci bagian 2–4 | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` (`disabled` + `disabledReason`) |
| 3 | Pesan cek keselamatan di bagian 1 | `COMPOSE` | `InformationAlert` sebagai isian khusus pertama di kelompok 1 |
| 4 | Baris tindakan aktual | `COMPOSE` | `BaseGroupedEditorField` select/text/number per baris (pola `FE-RHB-005`/`007`) |
| 5 | Modal henti dini dan tidak terlaksana | `COMPOSE` | `ConfirmModal` (children) + `BaseGroupedEditorField` + `BaseCheckboxCard` |
| 6 | Tanda tangan dan batal beralasan | `REUSE` | `ConfirmModal` (`requireReason` untuk batal) |
| 7 | Baca saja + tabel tindakan + addendum | `REUSE`/`COMPOSE` | `BaseDetailView`, `BaseDetailSection` + `DataTable`, `RehabDocumentAddendumPanel` |
| 8 | Bagian Sesi di ruang kerja | `COMPOSE` | `BaseDetailSection` + `DataTable` |

**Pilihan untuk elemen `COMPOSE` (direkomendasikan dan dipakai):**

1. **Rakit dari base component yang ada** *(direkomendasikan)*. Tampilannya konsisten dengan layar rehab
   sebelumnya, tanpa regresi base component, dan biayanya rendah.
2. Membuat komponen sesi khusus (misalnya stepper cek keselamatan). Lebih bebas, tetapi menambah gaya
   paralel dan perlu persetujuan `NEW`.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 5 COMPOSE (opsi 1).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Sesi baru | Kontrak tidak punya draft sebelum mulai: sesi dibuat oleh `start` atau `not-done`. Karena itu halaman `new?programId=` hanya berisi cek keselamatan; bagian 2–4 tampil terkunci "Terkunci sampai sesi dimulai." |
| `inpEpisodeId` | Tidak dikirim layar. Server mencarinya dari kunjungan (pasien RWI). |
| Pesan bagian 1 | Penolakan `RHB-VAL-050..054` (program tidak aktif, kunjungan, pasien pulang, cek belum lengkap, kontraindikasi) tampil di dalam kartu bagian 1, bukan sebagai galat umum |
| Tindakan rencana | Sesi yang baru dimulai langsung berisi baris tindakan rencana (parameter rencana sebagai awal). Tindakan di luar rencana boleh ditambahkan; server mewajibkan deviasi *Dimodifikasi*. |
| Simpan lalu aksi | *Selesai*, *Henti Dini*, dan *Tanda Tangan* menyimpan perubahan lebih dulu (`PUT`), lalu menjalankan aksinya dengan versi terbaru |
| Henti dini | Lewat modal: jenis (*Dihentikan dini*/*Dieskalasi*), alasan dan tindakan segera wajib, pihak dihubungi wajib bila eskalasi, "Program perlu telaah" tercentang bawaan. Deviasi pada form utama hanya *Tanpa deviasi*/*Dimodifikasi*. |
| AC 4 | Seluruh tombol aksi memakai satu bendera `saving`; tombol utama menampilkan "Memulai..." / "Menyimpan..." |
| Status billing | Ditampilkan di daftar dan badge sesi. Ulang handoff tidak ada (milik `FE-RHB-009`). |
| Sesi tidak terlaksana | Bagian 2–4 tetap terkunci tanpa baris tindakan; aksi *Tanda Tangan* dan *Batalkan Sesi* |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas seluruh folder rehab (`view`, `features`, `hooks`, `services`, `app`), konstanta/utils sesi, dan `menu-items.jsx` | 0 error, 0 warning pada berkas task. Dua warning lama di hook master-data `FE-RHB-002` tidak disentuh. Satu warning `exhaustive-deps` sesudah perbaikan tampilan sudah dibereskan. |
| Grep anti-regresi UI di berkas sesi (hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. CSS baru hanya `.sessionRow` dengan token `globals.css`. |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | Berkas existing yang berubah: `menu-items.jsx`, hook + view program, hook + view ruang kerja, komponen bagian ruang kerja, CSS rehab. Base component tidak berubah. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium; respons jaringan direkam, dan permintaan `start` diintersep pada dua butir (lihat catatan).

**Data uji:**
- program *Uji BE-RHB-007 v2* (aktif) di `RHB-EPS-2026-000006`;
- akun uji berprofesi Dokter Umum (cocok dengan layanan A), dengan kebijakan *RehabEpisode*, *RehabLookup*, *RehabSession*, *ClinicalNoteAddendum*, ditambah *RehabProgram : Read*.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | **AC5**: butir *Sesi Terapi* tampil di blok Rehab Medik untuk pemegang `RehabSession:Read`; daftar sesi termuat | PASS ×2 |
| 2 | Program aktif menampilkan *Mulai Sesi* → `sessions/new?programId=` | PASS |
| 3 | **AC2**: sebelum mulai, bagian 2–4 terkunci ("Terkunci sampai sesi dimulai.", respons dan deviasi nonaktif, tanpa baris tindakan) | PASS |
| 4 | Mulai tanpa cek keselamatan → ditahan di bagian 1, tanpa `POST` | PASS |
| 5 | **UAT-09 / AC1**: permintaan `start` diarahkan ke kunjungan `ENC-RSMMC-00127` (status *Selesai*) → server 422, pesan "Pilih kunjungan pasien hari ini yang masih terbuka." tampil **di dalam bagian 1**, sesi tidak dibuat | PASS |
| 6 | **AC3**: 422 `RHB-VAL-052` → "Pasien sudah pulang dari rawat inap." tampil di bagian cek keselamatan | PASS (respons di-stub, lihat catatan) |
| 7 | **AC4**: selama `start` berjalan (ditunda 2,5 detik), *Tidak Terlaksana* dan *Memulai...* nonaktif | PASS |
| 8 | Mulai sungguhan → 201, URL id sesi, *Sesi: Berlangsung*, bagian 1 terkunci, baris tindakan rencana terisi; tombol Batalkan/Simpan/Henti Dini/Selesai | PASS ×2 |
| 9 | **UAT-12 / AC1**: henti dini tanpa tindakan segera → ditahan di modal, tanpa request | PASS |
| 10 | **UAT-12**: dengan tindakan segera → `PUT` 200 lalu `stop-early` 200; *Sesi: Dihentikan dini*, *Program perlu telaah*; server `needsReview = true` | PASS ×2 |
| 11 | Sesi dihentikan: hanya Simpan + Tanda Tangan → `sign` 200 → baca saja *Catatan: Final*, tanpa isian aktif | PASS ×2 |
| 12 | Sesi kedua: *Selesai* → `PUT` 200 + `complete` 200 → *Sesi: Selesai* → tanda tangan → Final; badge *Billing: Tidak Ditagihkan* | PASS ×2 |
| 13 | Sesi ketiga: *Tidak Terlaksana* → alasan wajib di modal → `not-done` 201 → *Sesi: Tidak terlaksana*, tombol Tanda Tangan + Batalkan, bagian 2–4 terkunci | PASS |
| 14 | Daftar: saringan *Sesi saya* → request `mine=true`, 3 sesi milik akun uji; klik dua kali membuka sesi | PASS ×2 |
| 15 | Ruang kerja: bagian *Sesi Terapi* memuat sesi episode | PASS |
| 16 | Sesudah perbaikan tampilan: bagian terkunci tanpa tombol *+ Tambah Tindakan* dan tanpa baris (SuperAdmin, tanpa membuat data) | PASS |

**Catatan intersepsi.**
- Butir #5 mengubah `encounterId` pada permintaan `start` menjadi kunjungan yang sudah ditutup. Ini
  meniru kunjungan yang ditutup sesudah dipilih, karena pilihan kunjungan di layar memang hanya
  memuat kunjungan terbuka. Penolakan dan pesannya berasal dari server sungguhan.
- Butir #6 memakai respons tiruan, karena DB dev tidak punya pasien rawat inap yang sudah pulang
  fisik (keterbatasan yang sama dicatat di `BE-RHB-012`). Yang dibuktikan butir ini hanya letak dan
  bunyi pesan di layar; aturan servernya teruji statis di `BE-RHB-012`.

**Dua putaran awal berhenti karena kesalahan skrip uji.** Keduanya membuat sesi sungguhan:

| Putaran | Kejadian | Penyebab | Dampak data dev |
| --- | --- | --- | --- |
| 1 | Pengalihan kunjungan tidak berlaku, `start` 201 | Penangan intersepsi ikut menangkap permintaan `OPTIONS` (preflight CORS). Setelah diperbaiki (`POST` saja), intersepsi dibuktikan dengan diagnosa tanpa data: 1 permintaan tertangkap, pesan tampil. | 1 sesi *Berlangsung* |
| 2 | Pengalihan berlaku, tetapi `start` 201 | Kunjungan tujuan `ENC-RSMMC-00015` berstatus 8 (*Billing*), yang menurut server bukan kunjungan tertutup (tertutup = *Selesai*, *Dibatalkan*, *Tidak hadir*). Diganti `ENC-RSMMC-00127` (status 9, kunjungan yang sama dengan uji UAT-09 `BE-RHB-012`). | 1 sesi *Berlangsung* |

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 36 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

**Residu data dev** di `RHB-EPS-2026-000006`:
- 2 sesi *Berlangsung* dari putaran 1–2. Pelaksananya akun uji yang sudah dinonaktifkan, sehingga sesi
  ini hanya dapat dibatalkan pelaksananya.
- 1 sesi *Tidak terlaksana* yang belum ditandatangani;
- 1 sesi *Selesai* Final;
- 1 sesi *Dihentikan dini* Final;
- program v2 *Aktif* bertanda perlu telaah.

Sesi *Berlangsung* dan draft yang tersisa akan muncul sebagai catatan belum final saat ringkasan akhir
episode ini diuji (`FE-RHB-011`). Uji penutupan sebaiknya memakai episode baru.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-09, UAT-12 lewat layar | Terpenuhi | §5 #5 (UAT-09, permintaan diarahkan ke kunjungan tertutup; penolakan dari server), #9–10 (UAT-12) |
| (2) Bagian 2–4 terkunci sebelum `start` berhasil | Terpenuhi | §5 #3, #8, #16 |
| (3) `RHB-VAL-052` tampil di bagian cek keselamatan | Terpenuhi untuk tampilan | §5 #6 (respons tiruan; data pasien pulang belum ada di dev) |
| (4) Tombol aksi nonaktif selama permintaan berjalan | Terpenuhi | §5 #7 |
| (5) Butir menu *Sesi Terapi* ber-`requiredPermission` RehabSession:Read | Terpenuhi | §3.1, §5 #1 |
| Bawaan `FE-RHB-007` AC 4: *Mulai Sesi* di program aktif | Terpenuhi | §5 #2 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| `RHB-VAL-052` dengan data nyata | Uji ulang singkat bila DB dev sudah punya pasien RWI yang pulang fisik |
| Residu sesi *Berlangsung* | 2 sesi pada `RHB-EPS-2026-000006` tidak dapat dibatalkan tanpa akun pelaksananya. Pembersihan lewat DB dev butuh izin owner tersendiri. |
| Billing | Sesi final menunjukkan *Tidak Ditagihkan*. Penyerahan tindakan ke billing dan ulang handoff menunggu `BE-RHB-013` (`RHB-OQ-015`) dan `FE-RHB-009`. |
| Dependency backend | Tidak ada yang baru |
