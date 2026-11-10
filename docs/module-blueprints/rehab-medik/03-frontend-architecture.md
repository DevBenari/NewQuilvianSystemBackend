# Rehab Medik — Arsitektur Frontend

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1, `draft` |
| Frontend SHA | `0c06c093` (`sukmagpV2`) |
| Kewenangan UI | RHB-FE-01..04 (`00-interview-decisions.md` §7) |
| Sumber data | `contracts/api-contract.md` v1 — semua endpoint **Rencana (belum tersedia)** |

Hirarki kewenangan: keamanan/privasi/invariant → brief yang disetujui (PRD §5, RHB-FE-01..03)
→ konvensi project (base components, pola Hemodialisa) → `DEV_DISCRETION`.

## 1. Kebutuhan layar

| ID | Layar | Jenis | Feature |
|---|---|---|---|
| FE-RHB-01 | Daftar Permintaan | Worklist | FEAT-001..003 |
| FE-RHB-02 | Form Permintaan (buat/ubah) | Form | FEAT-001, 002 |
| FE-RHB-03 | Detail Permintaan + disposisi | Detail | FEAT-002, 003 |
| FE-RHB-04 | Daftar Episode | Worklist | FEAT-007..029 |
| FE-RHB-05 | Ruang kerja episode | Detail + timeline | semua Phase 1 |
| FE-RHB-06 | Form Asesmen Medis | Form klinis | FEAT-007 |
| FE-RHB-07 | Form Asesmen Profesi | Form klinis | FEAT-008..010 |
| FE-RHB-08 | Form Program + Tujuan | Form klinis | FEAT-012, 013 |
| FE-RHB-09 | Form Edukasi / Persetujuan | Form klinis | FEAT-015 |
| FE-RHB-10 | Daftar Sesi Terapi | Worklist | FEAT-016..018, 032 |
| FE-RHB-11 | Form Sesi Terapi | Form klinis | FEAT-016..018, 032 |
| FE-RHB-12 | Form Reasesmen | Form klinis | FEAT-024, 025 |
| FE-RHB-13 | Ringkasan Akhir | Form klinis | FEAT-026, 027 |
| FE-RHB-14 | Konfigurasi Layanan Rehab | Master | RHB-DEC-004, 017, 020, 021 |

## 2. Peta butir menu

Berkas yang disunting: `src/utils/menu-sidebar/menu-items.jsx`. Blok modul mengikuti pola blok
Hemodialisa (`key`, `subMenu`, `pathname`, `permission: "<Resource>:Read"`). Konfigurasi masuk
grup **Pelayanan Kesehatan → Master Data** (`healthServicesMasterData` → `subItems`), mengikuti
preseden master Hemodialisa.

```text
Rehab Medik                               <- tingkat 0, `subMenu`
├── Permintaan Rehab                      -> /health-services/rehabilitation-management/requests
├── Episode Rehab                         -> /health-services/rehabilitation-management/episodes
└── Sesi Terapi                           -> /health-services/rehabilitation-management/sessions

Master Data (healthServicesMasterData)    <- sudah ada
└── Rehab Medik                           <- grup baru, `subItems`
    └── Layanan Rehab                     -> /health-services/rehabilitation-management/master-data/disciplines
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Rehab Medik | 0 | — | — | — | — | Baru |
| Permintaan Rehab | 1 | Rehab Medik | `/health-services/rehabilitation-management/requests` | FE-RHB-01 | `RehabRequest:Read` | Baru |
| Episode Rehab | 1 | Rehab Medik | `.../episodes` | FE-RHB-04 | `RehabEpisode:Read` | Baru |
| Sesi Terapi | 1 | Rehab Medik | `.../sessions` | FE-RHB-10 | `RehabSession:Read` | Baru |
| Rehab Medik (grup) | 1 | Master Data | — | — | — | Baru |
| Layanan Rehab | 2 | Master Data → Rehab Medik | `.../master-data/disciplines` | FE-RHB-14 | `RehabConfiguration:Read` | Baru |

**Layar anak** (tanpa butir menu):

| Layar | Jalan masuk |
|---|---|
| FE-RHB-02 | Tombol "Buat Permintaan" di FE-RHB-01 (`/requests/new`), "Ubah" di FE-RHB-03 (`/requests/[requestId]/edit`) |
| FE-RHB-03 | Baris FE-RHB-01 (`/requests/[requestId]`) |
| FE-RHB-05 | Baris FE-RHB-04, tautan episode di FE-RHB-03 dan FE-RHB-10 (`/episodes/[episodeId]`) |
| FE-RHB-06, 07, 08, 09, 12, 13 | Bagian terkait di FE-RHB-05 (`/episodes/[episodeId]/<dokumen>/...`) |
| FE-RHB-11 | Tombol "Mulai Sesi" pada program aktif di FE-RHB-05, dan baris FE-RHB-10 (`/sessions/[sessionId]`) |

Key `menuRehabMedis` di `left-sidebar-menu-handle.jsx` sudah ada dan tidak perlu diubah. Rupa
(nama, ikon, urutan) adalah `DEV_DISCRETION`.

## 3. Skema fitur per layar

Komponen yang dipakai berasal dari `src/components/features/base-features/`: `hero`,
`data-filter`, `data-table`, `pagination`, `filter-date-picker`, `filter-select`,
`resource-filter-select`, `status-badge`, `base-button`, `row-action-menu`,
`base-detail-view`, `base-detail-card`, `base-grouped-editor-view`, `base-patient-card`,
`confirm-modal`, `toast-stack`, `access-denied-gate`. Signature props diverifikasi saat task.

### FE-RHB-01 Daftar Permintaan

```text
+- Permintaan Rehab --------------------------------------------- FE-RHB-01 -+
| Hero: judul + [Buat Permintaan]                                            |
| [Tanggal awal] [Tanggal akhir] [Periode v] [Status v] [Sumber v] [Layanan v] [cari] [Refresh] |
| Ringkasan: Diajukan n | Perlu klarifikasi n | Diterima n | Ditolak n      |
+----------------------------------------------------------------------------+
| No | No. Permintaan | Pasien | No. RM | Sumber | Layanan | Tgl | Pengirim | Status | ⋮ |
+----------------------------------------------------------------------------+
| kosong -> "Belum ada permintaan pada saringan ini."  [Atur ulang]          |
| gagal  -> "Data permintaan gagal dimuat."            [Coba lagi]           |
+- pagination ---------------------------------------------------------------+
```

| Wilayah | Sumber data | Hak akses |
|---|---|---|
| Filter | `GET /rehab-requests/filters/metadata` | `RehabRequest:Read` |
| Ringkasan | `GET /rehab-requests/summary` | `RehabRequest:Read` |
| Tabel | `GET /rehab-requests` | `RehabRequest:Read` |
| Buat Permintaan | → FE-RHB-02 | `RehabRequest:Create` |
| ⋮ Detail | → FE-RHB-03 | `RehabRequest:Read` |

### FE-RHB-02 Form Permintaan

Grouped editor dengan empat bagian: **Pasien** (lookup `rehab-lookups/patients`, lalu kartu
pasien read-only); **Asal** (sumber, kunjungan dari `rehab-lookups/encounters?patientId=`,
surat rujukan internal, atau fasilitas luar); **Indikasi dan layanan** (teks indikasi,
multi-pilih `rehab-lookups/disciplines`); **Peringatan duplikasi** (`GET /duplicate-check`,
kandidat ditampilkan beserta isian alasan).

Tombol: Simpan draft (`POST`/`PUT`, `RehabRequest:Create/Update`), Ajukan (`/submit`,
`RehabRequest:Update`). Respons 409 `RHB-VAL-007` membuka bagian duplikasi, bukan toast saja.
Respons 422 menandai field.

### FE-RHB-03 Detail Permintaan

```text
+- RHB-REQ-2026-000014 · <pasien> ---------------------------- FE-RHB-03 -+
| Status chip   Sumber   Diajukan <waktu> oleh <pengirim>                  |
| [Ubah] [Ajukan] [Kembalikan] [Minta Klarifikasi] [Terima] [Alihkan] [Tolak] [Batalkan] |
+--------------------------------------------------------------------------+
| Pasien (read-only) | Asal & rujukan | Indikasi | Layanan diminta         |
| Riwayat status (GET /{id}/status-history)                                |
+--------------------------------------------------------------------------+
```

Tombol hanya tampil bila status mengizinkan **dan** pengguna memegang butirnya (`Return`,
`Disposition`, `Cancel`). Setiap aksi memakai `confirm-modal` dengan isian alasan. "Terima"
meminta dokter penanggung jawab (`rehab-lookups/doctors`), lalu menautkan ke FE-RHB-05.

### FE-RHB-04 Daftar Episode

Kolom: No, No. Episode, Pasien, No. RM, Layanan aktif, Dokter penanggung jawab, Evaluasi
berikutnya, Perlu telaah (badge), Status, ⋮. Filter: tanggal, periode, status, layanan, dokter,
"perlu telaah", "evaluasi jatuh tempo", cari, refresh. Sumber datanya `GET /rehab-episodes`
(`RehabEpisode:Read`). Bunyi kosong: "Belum ada episode pada saringan ini."

### FE-RHB-05 Ruang kerja episode

```text
+- RHB-EPS-2026-000009 · <pasien> ----------------------------- FE-RHB-05 -+
| Status chip  Dokter PJ  Dibuka <tgl>  Layanan: Fisio (aktif), Wicara (rencana) |
+---------------+----------------------------------------------------------+
| Ringkasan     | isi bagian terpilih                                       |
| Asesmen       |  daftar + [Asesmen Medis] / [Asesmen Profesi]            |
| Tujuan        |  daftar tujuan + versi                                    |
| Program       |  per layanan: versi aktif, perlu telaah, [Mulai Sesi]    |
| Edukasi       |  daftar persetujuan                                       |
| Sesi          |  sesi episode                                            |
| Reasesmen     |  daftar + [Reasesmen]                                     |
| Ringkasan akhir| prasyarat yang belum final + [Susun]                     |
| Riwayat       |  timeline + status history                               |
+---------------+----------------------------------------------------------+
```

| Wilayah | Sumber | Tombol → hak akses |
|---|---|---|
| Kepala | `GET /rehab-episodes/{id}` | — |
| Asesmen | `GET /rehab-medical-assessments/by-episode`, `/rehab-discipline-assessments/by-episode` | Asesmen Medis → `RehabMedicalAssessment:Create`; Asesmen Profesi → `RehabDisciplineAssessment:Create` |
| Tujuan | `GET /rehab-goals/by-episode` | `RehabGoal:Create/Update` |
| Program | `GET /rehab-programs/by-episode` | `RehabProgram:Create/Activate/Stop`; Mulai Sesi → `RehabSession:Create` |
| Edukasi | `GET /rehab-education-records/by-episode` | `RehabEducation:Create` |
| Sesi | `GET /rehab-sessions/by-episode` | — |
| Reasesmen | `GET /rehab-progress-reviews/by-episode` | `RehabProgressReview:Create` |
| Ringkasan akhir | `GET /rehab-discharge-summaries/by-episode` | `RehabDischargeSummary:Update/Sign` |
| Riwayat | `GET /{id}/timeline`, `/{id}/status-history` | — |
| Addendum (pada setiap dokumen final) | service FE `clinical-note-addendum.service.js` | `ClinicalNoteAddendum:Create` |

Navigasi bagian (tab, panel samping, atau anchor) adalah `DEV_DISCRETION`. Form klinis dibuka
sebagai **halaman**, bukan modal (PRD §5).

### FE-RHB-06/07 Form Asesmen

Bagian: konteks (kunjungan dari lookup, read-only setelah dipilih); riwayat; temuan
fisik/fungsi; **pengukuran** (instrumen dari `rehab-lookups/instruments`, dirender dari definisi
versi, ditambah daftar pengukuran terstruktur: nama, bagian tubuh/domain, nilai, satuan); risiko
dan kontraindikasi; masalah/diagnosis; prognosis; kesimpulan. Khusus asesmen medis: **resep
layanan** (multi-pilih disiplin + kewaspadaan).

Tombol: Simpan draft, Tanda tangan (`confirm-modal`). Setelah final, form menjadi baca saja
dengan tombol Addendum. Status keutuhan (Draf/Ditandatangani) ditampilkan terpisah dari status
episode (aturan RM-FE-008).

### FE-RHB-08 Form Program + Tujuan

Bagian: layanan (terkunci); tujuan (pilih tujuan aktif atau buat baru: jangka, deskripsi,
baseline, target, tanggal, disepakati pasien); intervensi (tindakan dari
`rehab-lookups/disciplines/{id}/procedures` + parameter); frekuensi; jumlah sesi rencana
(informatif); kewaspadaan; tanggal mulai dan evaluasi. Pada versi > 1 ada alasan modifikasi.

Tombol: Simpan draft, Aktifkan (`/activate`). Respons 422 `RHB-VAL-040..045` ditampilkan
sebagai daftar prasyarat yang belum terpenuhi, masing-masing dengan tautan ke bagian terkait.

### FE-RHB-09 Form Edukasi

Isian: program terkait, materi, penerima dan hubungannya, metode, tingkat pemahaman, hambatan,
status persetujuan, dan catatan bila menolak sebagian/seluruhnya. Tombol: Simpan, Tanda tangan.

### FE-RHB-10 Daftar Sesi Terapi

Kolom: No, No. Sesi, Pasien, Layanan, Terapis, Mulai, Status sesi, Status catatan, Status
billing, ⋮. Filter: tanggal, periode, status, layanan, "sesi saya", status billing, cari,
refresh. Sumber: `GET /rehab-sessions`. Status billing `Failed` diberi badge. Aksi "Ulang
billing" tampil untuk `RehabBillingHandoff:Retry`.

### FE-RHB-11 Form Sesi Terapi

```text
+- Sesi RHB-SES-2026-000101 · <pasien> · Fisioterapi v2 ------ FE-RHB-11 -+
| 1 Cek keselamatan: kunjungan [v]  [ ] identitas  [ ] program  kondisi [...] |
|   kontraindikasi [ ] ya -> keterangan         [Mulai] [Tidak terlaksana v] |
| 2 Tindakan aktual: + baris (tindakan v, parameter aktual, menit, jumlah)   |
| 3 Respons & hasil langsung                                                  |
| 4 Deviasi: jenis v, alasan, tindakan segera, pihak dihubungi, [x] perlu telaah |
| [Simpan] [Selesai] [Henti Dini] [Tanda tangan]     Billing: chip + [Ulang]  |
+----------------------------------------------------------------------------+
```

Bagian 2–4 terkunci sebelum `start` berhasil. Bila pasien RWI sudah pulang, pesan
`RHB-VAL-052` ditampilkan di bagian 1. Status billing dibaca dari
`GET /{id}/billing-handoff` (`RehabBillingHandoff:Read`).

### FE-RHB-12 Form Reasesmen

Bagian: layanan/program; pengukuran kini **berdampingan dengan baseline** (dari asesmen awal
atau reasesmen sebelumnya, instrumen dan satuan yang sama); alasan pergantian instrumen bila
berbeda; capaian per tujuan; ringkasan progres; hambatan; keputusan + alasan; tujuan rujukan.

### FE-RHB-13 Ringkasan Akhir

Bagian: prasyarat (daftar dokumen draft dan sesi berjalan, dengan tautan); alasan penutupan;
layanan utama; capaian tujuan; kondisi akhir; tindak lanjut (jenis, instruksi, tujuan, tanggal).
Tanda tangan dengan `confirm-modal` yang menyebut "Episode akan ditutup dan tidak dapat dibuka
kembali".

### FE-RHB-14 Konfigurasi Layanan Rehab

Pola master data (Hero + DataFilter + DataTable), dengan detail berisi tiga bagian: identitas
layanan + profesi (pilih `MstProfession` klinis), tindakan yang boleh dipakai, dan jenis
instrumen. Hak akses `RehabConfiguration:*`. Tautan ke layar instrumen klinis existing
disertakan untuk komite.

## 4. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` §3. FE **tidak** memeriksa nama peran.
FE hanya memakai butir hak akses dari profil pengguna untuk menyembunyikan tombol. Server tetap
menegakkannya: 403 dibalas `access-denied-gate` untuk halaman, atau toast untuk aksi.

## 5. Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | Kerangka baris/bagian, bukan layar kosong |
| Kosong | Kalimat spesifik per layar + tombol atur ulang/aksi utama bila berhak |
| Gagal | "Data … gagal dimuat." + Coba lagi. Pesan server ditampilkan tanpa stack trace. |
| Data basi (409 `RHB-VAL-010`) | Toast + tombol "Muat ulang". Isian pengguna tidak dibuang diam-diam. |
| Kirim ganda | Tombol aksi dinonaktifkan selama permintaan berjalan. Server menjaga lewat `expectedVersion`. |
| Permintaan basi | Pencarian/filter membatalkan permintaan sebelumnya (`AbortController`) |
| 403 halaman | `access-denied-gate` |
| Privasi | Tidak ada PHI di `console`, `localStorage`, atau `sessionStorage`. Kolom S hanya di layar detail. |

## 6. Struktur berkas (konvensi Hemodialisa)

```text
src/app/health-services/rehabilitation-management/
├── requests/ page.jsx, new/page.jsx, [requestId]/page.jsx, [requestId]/edit/page.jsx
├── episodes/ page.jsx, [episodeId]/page.jsx, [episodeId]/<dokumen>/...
├── sessions/ page.jsx, [sessionId]/page.jsx
└── master-data/disciplines/ page.jsx, [disciplineId]/page.jsx
src/components/view/health-services/rehabilitation-management/<layar>/...
src/lib/services/health-services/rehabilitation-management/<resource>.service.js
src/lib/constants/health-services/rehabilitation-management/rehabConstants.js
```

## 7. Kewenangan UI

| Hal | Status |
|---|---|
| Keberadaan layar, sumber data, tombol per hak akses, form klinis sebagai halaman | Mengikat (blueprint ini) |
| Pemakaian base components, satu hero per halaman, tanpa dashboard dekoratif | Mengikat (PRD §5–6, RHB-FE-02) |
| Nama butir menu, ikon, urutan, navigasi bagian FE-RHB-05, tata letak, warna | `DEV_DISCRETION` (RHB-FE-04) |
