# Panduan Uji Sisa Modul IGD — Putaran B4, D2, D3, L1, dan R1

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 17 Oktober 2026 |
| Disusun oleh | Agent pengembang, atas permintaan Rizki Gunawan (Product/Domain Owner IGD) |
| Dasar | `IGD-DEC-319` (hasil Putaran B3 diterima sebatas yang diuji), `IGD-FACT-123`, `IGD-FACT-124`, kartu task di `roadmap/backend-roadmap.md` dan `roadmap/frontend-roadmap.md` |
| Snapshot saat disusun | Backend `rizkiG` `bd645698`, frontend `RizkiV2` `58ee55d32` |
| Pelaksana | Putaran B4, D2, D3, L1: Antigravity (dijalankan Rizki). Putaran R1: Rizki, atau agent pengembang atas perintah tertulis Rizki |
| Yang menilai | Agent pengembang, dari bukti mentah dan log backend — **bukan** dari ringkasan penguji |

Panduan ini menjawab satu pertanyaan: **apa saja yang masih harus diuji supaya task IGD yang kodenya sudah ada bisa
dinilai selesai.** Setiap skenario ditulis dengan ID sendiri dan menunjuk acceptance kartu yang dijawabnya.

---

## 0. Ringkasan — apa saja yang perlu diuji

| Putaran | Isi | Task | Prioritas | Penguji |
| --- | --- | --- | :-: | --- |
| ~~**B4**~~ | Transfer ke rawat inap — **dijalankan 17 Oktober 2026, tidak sah sebagai uji build** (`IGD-FACT-125`); digantikan B5 | — | — | — |
| **B5** | Transfer dalam lingkup IGD: sampai _admit_, penutupan alur normal, pembatalan, kewenangan DPJP, unit tujuan salah (bagian 3B) | `BE-IGD-074`, `FE-IGD-060`, `FE-IGD-061`; regresi `FE-IGD-059` | 1 | Antigravity (persiapan boleh SuperAdmin, `IGD-DEC-320`) |
| **D2** | Ruang Kerja Dokter IGD: daftar pasien, kajian medis, catatan dokter, Catatan Saya, resep | `BE-IGD-065`…`068`, `FE-IGD-045`…`049` | 2 | Antigravity |
| **D3** | Tindakan dan tindak lanjut di layar dokter dan perawat | `BE-IGD-070`, `FE-IGD-050`, `051`, `054`, `055` | 3 | Antigravity |
| **L1** | Layar lama yang belum pernah dibuktikan lewat layar | `FE-IGD-012`, `013`, `022` | 4 | Antigravity |
| **R1** | Uji langkah mundur migration di salinan basis data | `BE-IGD-072` (acceptance 8), `BE-IGD-026` (kriteria 1), `BE-IGD-031` (kriteria 2) | boleh paralel | Rizki |

**Tidak diuji di panduan ini, beserta sebabnya:**

| Task | Sebab |
| --- | --- |
| `BE-IGD-071`, `FE-IGD-057`, `FE-IGD-058` | ⛔ menunggu `IGD-OQ-117` (pemilik Laboratorium dan Radiologi) |
| `FE-IGD-053` | ⛔ menunggu `IGD-OQ-119` (pemilik Gizi) |
| `BE-IGD-042` | ⛔ menunggu konfirmasi pemilik data |
| `BE-IGD-035` | Bukan uji: menunggu approval Clinical Governance atas `IGD-DEC-100`/`101` |
| `BE-IGD-017`, `BE-IGD-043`, `FE-IGD-025`, `FE-IGD-026` | Bukan uji: laporan tracked yang belum ditulis |
| `FE-IGD-010`, `FE-IGD-052`, `FE-IGD-056` | Belum dikerjakan; belum ada yang dapat diuji |

---

## 1. Aturan yang mengikat seluruh putaran

| No | Aturan |
| ---: | --- |
| A1 | Jangan memakai SuperAdmin, jangan menekan _Simpan Akses_, dan jangan menjalankan SQL atau skrip basis data apa pun, termasuk `SELECT`. Bila hak akses kurang, tulis `NOT RUN — 403` dan laporkan ke Rizki. Pelanggaran aturan ini pada putaran 2 (9 Oktober, skrip `grant-…sql`) tidak boleh terulang |
| A2 | Jangan menyunting source aplikasi, CSS, roadmap, laporan task, atau dokumen blueprint yang sudah ada. **Tidak ada perbaikan kode selama putaran berjalan.** Bila menemukan cacat, tulis `FAIL`, hentikan skenario yang bergantung padanya, lalu lanjutkan skenario lain |
| A3 | Catat keadaan kode **di awal dan di akhir** putaran ke `kode-awal.txt` dan `kode-akhir.txt`: `git rev-parse HEAD` dan `git status --short` kedua repo, jam berkas DLL backend, dan isi `.next/BUILD_ID` frontend. Bila awal dan akhir berbeda, putaran tidak sah |
| A4 | Jalankan pada build produksi (server standalone `npm run start`), bukan `next dev`. Lencana _N_ di pojok kiri bawah berarti `next dev`. Ukuran layar 1440 × 900 |
| A5 | Sandi dan token dibaca dari variabel lingkungan, tidak ditulis di skrip, JSON, PNG, atau laporan. Token di JSON disensor |
| A6 | Satu putaran, satu folder: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-<ID putaran>-<YYYYMMDD>/`. Folder ini masuk `.gitignore`, jadi Rizki menyalinnya manual bila agent memeriksa dari mesin lain |
| A7 | Rekam jaringan setiap panggilan `/api/v1/` per skenario ke `jaringan-<ID>.json` (metode, path, status, jam, isi respons tersensor) |
| A8 | Simpan setiap versi skrip tanpa menimpa (`skrip/<nama>-v1.mjs`, `-v2.mjs`, …). Catat **setiap** percobaan, termasuk yang gagal atau diulang, di `percobaan.json` beserta jamnya |
| A9 | Hasil hanya `PASS`, `FAIL`, `NOT RUN`, atau `NOT AVAILABLE`, ditentukan dari bukti. Satu baris per ID; **dilarang menggabungkan ID** (misalnya "B4-07..10"). Pesan galat dicatat **persis** seperti tampil |
| A10 | Penguji tidak menetapkan status task (✅, 🟡, "selesai"). Status dinilai agent pengembang |
| A11 | Skenario layar dijalankan lewat tombol di layar. API hanya dipakai pada skenario bertanda **API**. Tidak ada jalan pintas — memanggil API, mengubah data, atau memakai tombol tersembunyi — untuk membuat skenario layar lulus |
| A12 | Pasien uji baru per putaran, disiapkan lewat layar. Nama, No. RM, dan nomor kunjungan nyata dicatat di `data-uji.json`; jangan mengarang nama. Jangan memakai pasien `4411cf93-fcca-49b5-8e63-1b08c15a9e32` |
| A13 | Laporan ditulis ke `laporan.md` di folder bukti, dan boleh disalin sebagai **berkas baru** ke `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-<ID>.md`. Berkas lain tidak disentuh |
| A14 | Sesudah selesai, serahkan log backend `NewQuilvianSystemBackend/Logs/quilvian-backend-<YYYYMMDD>.json` apa adanya |

---

## 2. Akun dan hak akses — disiapkan Rizki lewat Akses Role

| Peran | Akun | Variabel sandi | Dipakai di |
| --- | --- | --- | --- |
| Perawat IGD | `dimas.kurniawan@rsmmc.local` | `QUILVIAN_PERAWAT_PASSWORD` | B4, D2, D3, L1 |
| Dokter IGD (DPJP) | `ranger.biru@admin.com` | `QUILVIAN_DOKTER_PASSWORD` | B4, D2, D3 |
| Dokter IGD kedua (bukan DPJP) | disiapkan Rizki | `QUILVIAN_DOKTER2_PASSWORD` | B4-04, D2-U03, D2-U15 — **opsional**; tanpa akun ini skenario tersebut `NOT RUN` |
| Petugas admisi Rawat Inap | `rendy.saputra@rsmmc.local` | `QUILVIAN_ADMISI_PASSWORD` | B4 |
| Perawat ruangan rawat inap, terpetakan ke unit tujuan pasien B4 | disiapkan Rizki | `QUILVIAN_PERAWAT_RUANGAN_PASSWORD` | B4-10, B4-18 — **wajib** untuk penutupan kunjungan |
| Dokter rawat inap | disiapkan Rizki | `QUILVIAN_DOKTER_RANAP_PASSWORD` | Regresi D2 — opsional |

Hak akses petugas admisi: `InpatientAdmissionTransfer : Read`, `InpatientEpisode : Create`,
`InpatientBedOccupancy : Read/Create/Update`, `Doctor : Read`, ditambah hak langkah admisi yang biasa dipakai (pembayaran,
deposit). Kekurangan hak inilah yang membuat C-12 terblokir pada Putaran B2.

---

## 3. Putaran B4 — Transfer ke rawat inap (prioritas 1)

Folder bukti: `test-with-agy/igd/uji-putaran-b4-<YYYYMMDD>/`.

### 3.1 Prasyarat

| No | Prasyarat | Siapa | Bukti |
| ---: | --- | --- | --- |
| P0 | `kode-awal.txt` (A3), ditambah keluaran `git grep -n "EvaluasiPenutupanKunjunganSusulanAsync\|BatalkanFormPermintaanOlehPetugasAdmisiAsync" -- Areas` di repo backend. Hasil ini menunjukkan apakah Rawat Inap sudah memanggil fungsi IGD | Penguji (baca saja) | `kode-awal.txt` |
| P1 | Buka _Admisi Rawat Inap_ → _Antrean Transfer IGD/Poli_. Potret antrean awal dan catat **apakah ada tombol _Tolak_** pada baris pasien. Bila tidak ada, B4-11…B4-16 ditulis `NOT AVAILABLE` tanpa jalur pengganti | Penguji | `P1-antrean-awal.png` |
| P2 | Jenis tindak lanjut rawat inap aktif dan bertanda `requiresAdmissionRequest = true` | Rizki | Layar atau API master jenis tindak lanjut |
| P3 | Tiga pasien IGD **baru**: **P-B4-A**, **P-B4-B**, **P-B4-C**. Masing-masing berpasien dan berencounter, DPJP IGD aktif `ranger.biru`, diagnosis kerja ICD-10, dan tindak lanjut rawat inap _Dikonfirmasi_ ke **bangsal rawat inap yang punya bed kosong dan terpetakan ke akun perawat ruangan** | Rizki / penguji lewat layar | `data-uji.json` |
| P4 | Akun bagian 2 siap tanpa `403` | Rizki | — |

### 3.2 Jalur utama sampai _admit_ — pasien P-B4-A

| ID | Aktor | Langkah | Hasil yang diharapkan | Bukti | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B4-01 | Perawat | Tab _Tindak Lanjut_ → _Buat Form_; isi unit, kelas, DPJP usulan, lama rawat, indikasi, tingkat ketergantungan; _Minta TTD_ | Lencana _Menunggu TTD_; _Jalankan_ nonaktif dengan keterangan | PNG + jaringan | Regresi `FE-IGD-059` |
| B4-02 | Dokter (DPJP) | _Ruang Kerja Dokter_ → _Perlu TTD_ → buka form → _Kembalikan ke Perawat…_ tanpa catatan, lalu dengan catatan | Tanpa catatan tombol nonaktif; dengan catatan form hilang dari _Perlu TTD_; layar perawat menampilkan _Draf_ beserta catatan | PNG ×2 | `FE-IGD-060` acc 5, `AT-IGD-227` |
| B4-03 | Perawat | Perbaiki isian sesuai catatan → _Minta TTD_ lagi | _Menunggu TTD_ | PNG | — |
| B4-04 | Rizki/perawat, lalu dokter kedua | **Opsional.** Alihkan DPJP P-B4-A ke dokter kedua lewat layar penugasan DPJP. Dokter kedua membuka _Perlu TTD_; `ranger.biru` mencoba _Tandatangani_ dari dialognya | Form pindah ke daftar dokter kedua; percobaan `ranger.biru` ditolak `403` dan pesannya tampil di dialog | PNG ×2 + jaringan | `FE-IGD-060` acc 4, `AT-IGD-226` |
| B4-05 | DPJP aktif saat itu | _Tandatangani_, klik dua kali cepat | Satu tanda tangan dan satu kejadian; pesan sukses menyebut Antrean Transfer IGD/Poli; form keluar dari _Perlu TTD_ | PNG + jaringan | `FE-IGD-060` acc 3, `BE-IGD-074` acc 2 |
| B4-06 | Petugas admisi | Muat ulang antrean → P-B4-A tampil berasal IGD → _Proses Admisi_. Catat apakah unit, kelas, dan DPJP usulan terisi awal dari form. Selesaikan seluruh langkah (verifikasi, pembayaran, deposit bila tunai, dokter, pilih bed, booking, konfirmasi, persetujuan) | `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` → `201`; P-B4-A hilang dari antrean. Pengisian awal yang tidak terjadi ditulis apa adanya | PNG langkah awal dan akhir + jaringan | `BE-IGD-074` acc 6 (C-12), `AT-IGD-229`; `IGD-DEC-309` butir C |
| B4-07 | Perawat | Muat ulang tab _Tindak Lanjut_ P-B4-A | Tindak lanjut _Dijalankan_; lencana form _Selesai_; keterangan _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."_; _Batalkan Form_ tidak ada; _Cetak_ ada | PNG + jaringan (`GET …/emergency-admission-requests?emergencyVisitId=` memuat `displayStatus` `Completed`) | `FE-IGD-061` acc 5, `BE-IGD-074` acc 6 |
| B4-08 | **API** — perawat, lalu dokter | `POST /api/v1/health-services/emergency-installation-management/emergency-admission-requests/{id}/cancel` untuk form P-B4-A; lalu `PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` ke `Cancelled` dengan catatan | Batal form: `409` _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."_ Batal tindak lanjut: `400` _"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."_ | JSON ×2 | `BE-IGD-074` acc 5, `AT-IGD-233` |
| B4-09 | Dokter | Layar dokter → tab _Tindak Lanjut_ P-B4-A → cari aksi _Batalkan_ | Tombol tidak tersedia, **atau** penolakan server tampil apa adanya di modal. Tulis mana yang terjadi | PNG | `FE-IGD-061` acc 3 (bagian sesudah admisi) |
| B4-10 | Perawat IGD, lalu perawat ruangan | Tab _Transfer Pasien_ → buat kepergian ke unit tujuan dengan SBAR → kirim serah terima → _Berangkat_. Perawat ruangan: _Terima Dokumen_ → _Catat Tiba_ | Kunjungan IGD P-B4-A _Selesai_; `GET …/emergency-visits/{id}` memuat `visitStatus` `Completed` dan waktu selesai | PNG ×2 + jaringan | `IGD-DEC-308` alur normal, `AT-IGD-237` |

### 3.3 Tolak admisi — pasien P-B4-B (hanya bila P1 menemukan tombol _Tolak_)

| ID | Aktor | Langkah | Hasil yang diharapkan | Bukti | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B4-11 | Perawat, dokter | Form P-B4-B (centang CITO) → _Minta TTD_ → _Tandatangani_ | P-B4-B tampil di antrean dengan lencana CITO, CITO paling atas | PNG | `IGD-DEC-309` butir B |
| B4-12 | Petugas admisi | _Tolak_ tanpa alasan; lalu dengan alasan _"Kelas 2 Bangsal Melati penuh"_ | Tanpa alasan tidak terkirim atau ditolak `400`; dengan alasan `200` dan P-B4-B hilang dari antrean. **Catat metode dan path endpoint tolak persis** dari rekam jaringan | PNG ×2 + jaringan | `BE-IGD-077` acc 1–2, `FR-IGD-119` |
| B4-13 | Perawat | Tab _Tindak Lanjut_ P-B4-B | Lencana merah _Dibatalkan admisi — Kelas 2 Bangsal Melati penuh_; tindak lanjut tetap _Dikonfirmasi_; _Buat Form Baru_ aktif; _Jalankan_ nonaktif dengan keterangan penolakan admisi | PNG | Regresi `FE-IGD-064` acc 1, 3, 4 |
| B4-14 | Perawat | _Lihat Form_ pada form yang ditolak | Baca-saja; baris _Alasan penolakan admisi_; riwayat _Ditolak petugas admisi_ beserta nama petugas | PNG | Regresi `FE-IGD-064` acc 2 |
| B4-15 | Perawat, dokter | _Buat Form Baru_ (unit lain) → _Minta TTD_ → _Tandatangani_ | P-B4-B kembali di antrean | PNG | `AT-IGD-239` |
| B4-16 | **API** — petugas admisi | Panggil endpoint tolak dari B4-12 untuk P-B4-A yang sudah diproses di B4-06 | `409` _"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."_ | JSON | `BE-IGD-077` acc 3 |

### 3.4 Pasien tiba lebih dulu, admisi kemudian — pasien P-B4-C

| ID | Aktor | Langkah | Hasil yang diharapkan | Bukti | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B4-17 | Perawat, dokter | Form P-B4-C → _Minta TTD_ → _Tandatangani_ | P-B4-C di antrean | PNG | — |
| B4-18 | Perawat IGD, lalu perawat ruangan | Kepergian dengan SBAR → _Berangkat_ → perawat ruangan _Terima Dokumen_ → _Catat Tiba_, **sebelum** admisi diproses | Perawat tidak diblokir mencatat kedatangan. Kunjungan IGD **belum** _Selesai_ karena tindak lanjut masih _Dikonfirmasi_. Catat status yang tampil apa adanya | PNG + jaringan | `IGD-DEC-308` |
| B4-19 | Petugas admisi | _Proses Admisi_ P-B4-C sampai selesai | `admit` → `201`; tindak lanjut _Dijalankan_ | PNG + jaringan | — |
| B4-20 | Perawat IGD | Muat ulang daftar dan detail kunjungan P-B4-C, **tanpa aksi lain** | Kunjungan _Selesai_ dengan waktu selesai tercatat. Bila tidak, tulis `FAIL` beserta status yang tampil — hasil ini dibaca bersama P0 | PNG + jaringan | `BE-IGD-077` acc 4, `AT-IGD-240`, `FR-IGD-120` |

---

## 3B. Putaran B5 — Transfer dalam lingkup IGD (menggantikan B4)

Folder bukti: `test-with-agy/igd/uji-putaran-b5-<YYYYMMDD>/`.

**Kenapa ada B5.** Putaran B4 (17 Oktober 2026) tidak sah sebagai uji build: sebagian besar layar dilayani `next dev`
(lencana _N_), persiapan memakai SuperAdmin di luar aturan, dan akun admisi serta perawat ruangan belum siap. Tidak ada cacat
kode IGD yang ditemukan. Penilaian lengkapnya ada di [laporan B4](2026-10-17-laporan-uji-putaran-b4.md), bagian
_Penilaian agent pengembang_, dan `IGD-FACT-125`.

**Lingkup B5** (`IGD-DEC-321`): semua kriteria transfer yang dapat dibuktikan IGD **tanpa mengubah kode Rawat Inap** —
memakai endpoint _admit_ Rawat Inap yang sudah ada, ditambah konfigurasi akun, Akses Role, dan penugasan unit.
**Tidak diuji:** tolak admisi (B4-11…16) dan penutupan alur pasien tiba lebih dulu (B4-17…20) — keduanya menunggu
`IGD-OQ-132` (Muhammad Hamzah).

### 3B.1 Dua fase

| Fase | Boleh | Dilarang |
| --- | --- | --- |
| **S — Persiapan** | Atas keputusan pemilik `IGD-DEC-320`, login SuperAdmin **hanya** untuk: (1) membuat akun uji — dokter IGD kedua, petugas admisi, perawat ruangan, dan petugas loket bila belum ada — beserta data HR-nya; (2) menugaskan perawat ruangan ke unit bangsal tujuan; (3) memberi Akses Role sesuai tabel 3B.2; (4) menyalakan bypass geolokasi (nilai _enable_) pada **setiap akun yang dipakai dalam B5** — akun uji baru maupun akun peran yang sudah ada seperti `dimas.kurniawan` dan `ranger.biru`. Akun uji B4 (`admisi.b4@rsmmc.local`, `dokter.igd.b4@rsmmc.local`) boleh dipakai ulang dengan menambah hak yang kurang | Membuat atau mengubah pasien, encounter, kunjungan, bed, atau data master lain lewat SuperAdmin atau rute admin; mengubah akun yang sudah ada **selain** menyalakan bypass geolokasi; memberi hak di luar tabel 3B.2; menjalankan SQL |
| **T — Skenario** | Akun peran saja | SuperAdmin dalam bentuk apa pun. Logout SuperAdmin dan tutup sesinya sebelum B5-00 |

Aturan bagian 1 tetap berlaku; A1 hanya dilonggarkan untuk fase S seperti di atas. Tambahan khusus B5:

| No | Aturan |
| ---: | --- |
| S1 | Setiap tindakan fase S dicatat di `persiapan.json` (jam, akun, layar atau endpoint, keadaan sebelum dan sesudah), ditambah PNG layar Akses Role sebelum dan sesudah diubah. Pakai layar admin bila tersedia. Daftar akun yang bypass geolokasinya dinyalakan ditulis lengkap, supaya Rizki dapat mengembalikannya sesudah uji bila perlu |
| S2 | Sandi semua akun, termasuk SuperAdmin, hanya dari variabel lingkungan (`QUILVIAN_SUPERADMIN_PASSWORD`, `QUILVIAN_*_PASSWORD`). Skrip yang memuat sandi tertulis membuat putaran tidak sah |
| S3 | Akun uji baru dinamai jelas, misalnya `perawat.ruangan.uji.b5@rsmmc.local`, dengan nama tampilan diakhiri _(Uji B5)_ |
| S4 | Bila langkah fase S gagal (misalnya galat `500` saat penugasan unit, seperti pada B4) atau bangsal tujuan tidak punya bed kosong: **berhenti dan laporkan** ke Rizki. Jangan membuat bed atau mencari jalan lain |
| S5 | Pasien dibuat lewat layar (loket → triage → DPJP → catatan dokter dan diagnosis → tindak lanjut), atau lewat API dengan **akun peran** yang berhak. Jangan memakai rute admin dan jangan memaksa status kunjungan lewat `PATCH visit-status` |
| T1 | Sebelum B5-00: hentikan `next dev`, jalankan `npm run build` lalu `npm run start` (port 3000). Simpan PNG halaman login tanpa lencana _N_ dan baris perintah proses `node` (harus `next start` atau `server.js`) ke `kode-awal.txt` |
| T2 | Status tindak lanjut lewat API memakai angka enum yang benar: Draft `1`, Confirmed `2`, Executed `3`, **Cancelled `4`** |
| T3 | Setiap PNG diambil sesudah aksi skenarionya sendiri. PNG yang identik dengan skenario lain tidak sah |

### 3B.2 Persiapan (fase S)

| No | Persiapan | Siapa | Cek selesai |
| --- | --- | --- | --- |
| S-01 | Backend dibuild dan dijalankan; frontend build produksi berjalan di port 3000 | Rizki (backend), penguji (frontend) | T1 |
| S-02 | Akun petugas admisi + Akses Role: `InpatientAdmissionTransfer : Read`, `InpatientEpisode : Create`, `InpatientBedOccupancy : Read/Create/Update`, `Doctor : Read`, ditambah hak langkah pembayaran dan deposit yang diminta wizard | Penguji, fase S | Login admisi → wizard _Proses Admisi_ terbuka sampai langkah 6 _Pilih Bed_ tanpa `403` (tanpa menyimpan) |
| S-03 | Akun perawat ruangan, ditugaskan ke unit bangsal tujuan | Penguji, fase S | Perawat ruangan membuka penerimaan serah terima tanpa pesan _"Anda tidak bertugas di unit …"_ |
| S-04 | Dokter IGD kedua (boleh `dokter.igd.b4`) dengan `EmergencyAdmissionRequest : Read/Sign` dan `EmergencyVisit : Read` | Penguji, fase S | Ruang Kerja Dokter terbuka |
| S-05 | Lima pasien IGD **baru** (aturan S5). P-B5-A, B, C, D: DPJP IGD aktif = dokter utama yang ditunjuk Rizki, diagnosis kerja ICD-10, tindak lanjut rawat inap _Dikonfirmasi_ ke bangsal ber-bed kosong. P-B5-E: tindak lanjut rawat inap ke unit **bukan** rawat inap | Penguji (akun peran) atau Rizki | `data-uji.json` dengan nama, No. RM, nomor kunjungan asli |

### 3B.3 Skenario (fase T)

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B5-00 | — | Semua akun | Pemeriksaan pembuka: T1; setiap akun login dan membuka layarnya | Tanpa lencana _N_; tanpa `403`. Bila gagal, **berhenti** | — |
| B5-01 | A | Perawat | _Buat Form_ (unit, kelas, DPJP usulan, lama rawat, indikasi, tingkat ketergantungan) → _Minta TTD_ | _Menunggu TTD_; _Jalankan_ nonaktif dengan keterangan | `FE-IGD-061` #1; regresi `FE-IGD-059` |
| B5-02 | A | Dokter DPJP | _Perlu TTD_ → _Kembalikan ke Perawat…_ tanpa catatan, lalu dengan catatan | Tanpa catatan tombol nonaktif; dengan catatan form hilang dari _Perlu TTD_; perawat melihat _Draf_ beserta catatan | `FE-IGD-060` #5, `AT-IGD-227` |
| B5-03 | A | Perawat | Perbaiki isian → _Minta TTD_ | _Menunggu TTD_ | — |
| B5-04 | A | Dokter DPJP | _Tandatangani_, klik dua kali cepat | Satu tanda tangan sukses di jaringan; pesan sukses tampil (catat persis); form keluar dari _Perlu TTD_ | `FE-IGD-060` #1–3, `BE-IGD-074` #1–2 |
| B5-05 | A | Petugas admisi | _Antrean Transfer IGD/Poli_ → P-B5-A tampil berasal IGD → _Proses Admisi_. Catat pengisian awal unit, kelas, dan DPJP. Selesaikan semua langkah sampai persetujuan | `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` → `201`; P-B5-A hilang dari antrean | `BE-IGD-074` #3, #6; C-12 `FE-IGD-060`; `AT-IGD-229` |
| B5-06 | A | Perawat | Muat ulang tab _Tindak Lanjut_ | _Dijalankan_; lencana _Selesai_; keterangan _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."_; _Batalkan Form_ tidak ada; _Cetak_ ada; `GET …/emergency-admission-requests?emergencyVisitId=` memuat `displayStatus` `Completed` | `FE-IGD-061` #2, #5; `BE-IGD-074` #6 |
| B5-07 | A | **API** perawat, lalu dokter | `POST /api/v1/health-services/emergency-installation-management/emergency-admission-requests/{id}/cancel`; lalu `PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` dengan `dispositionStatus: 4` dan catatan | `409` _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."_; `400` _"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."_ | `BE-IGD-074` #5, `AT-IGD-233` |
| B5-08 | A | Perawat IGD, lalu **perawat ruangan** | Tab _Transfer Pasien_ → kepergian ke bangsal tujuan dengan SBAR → kirim serah terima → _Berangkat_. Perawat ruangan: _Terima Dokumen_ → _Catat Tiba_ | Kunjungan IGD _Selesai_; `GET …/emergency-visits/{id}` memuat `visitStatus` `Completed` dan waktu selesai | `IGD-DEC-308` alur normal, `AT-IGD-237` |
| B5-09 | B | Perawat, dokter, admisi | Form → _Minta TTD_ → _Tandatangani_; admisi muat ulang antrean | P-B5-B tampil di antrean | `BE-IGD-074` #2 |
| B5-10 | B | Perawat; admisi; **API** | _Lihat Form_ → _Batalkan Form_ dengan alasan _"Keluarga minta rujuk RS lain"_; admisi muat ulang antrean; `GET …/inpatient-management/admission-transfers/{dispositionId P-B5-B}?sourceType=Emergency`; ulangi batal lewat API | Dialog menjelaskan pasien keluar dari antrean; lencana _Dibatalkan_ beserta alasan; P-B5-B hilang dari antrean; `GET` → `404`; batal ulang → `409` _"Form ini sudah dibatalkan."_ | `FE-IGD-061` #3; `BE-IGD-074` #3, #5; `AT-IGD-231` |
| B5-11 | B | Perawat | _Buat Form_ baru | Draf baru dapat dibuat | `AT-IGD-231` |
| B5-12 | C | Perawat, dokter, admisi | Form → _Minta TTD_ → _Tandatangani_ (P-B5-C di antrean). Dokter membatalkan tindak lanjut rawat inap dengan alasan _"Pasien membaik"_ lewat tab _Tindak Lanjut_ dokter; bila tombolnya tidak ada, catat itu lalu pakai **API** `PATCH …/disposition-status` dengan `dispositionStatus: 4` dan catatan. Perawat dan admisi muat ulang | Form _Dibatalkan_ beserta alasan; riwayat memuat _Dibatalkan otomatis_; P-B5-C hilang dari antrean | `FE-IGD-061` #4; `BE-IGD-074` #5; `AT-IGD-232` |
| B5-13 | D | Perawat; dokter DPJP; dokter kedua | Form → _Minta TTD_. Dokter DPJP membuka dialog form dari _Perlu TTD_ dan **membiarkannya terbuka**. Perawat mengalihkan DPJP ke dokter kedua lewat layar penugasan DPJP. Dokter lama menekan _Tandatangani_ di dialog yang masih terbuka. Dokter kedua membuka _Perlu TTD_, lalu _Tandatangani_ | Dialog dokter lama menampilkan `403` _"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."_; form tampil di _Perlu TTD_ dokter kedua dan tidak lagi di dokter lama; tanda tangan dokter kedua berhasil | `FE-IGD-060` #4; `BE-IGD-074` #1; `AT-IGD-226` |
| B5-14 | E | Perawat | _Buat Form_ | `409` _"Tindak lanjut ini tidak dapat masuk Antrean Transfer IGD/Poli: jenis tindak lanjutnya harus aktif dan unit tujuannya harus unit rawat inap. Perbaiki tindak lanjut lebih dulu."_ | `BE-IGD-074` #7, `AT-IGD-236` |

### 3B.4 Yang dapat dinilai sesudah B5

| Task | Bila B5 lulus | Bukti lain yang sudah ada |
| --- | --- | --- |
| `BE-IGD-074` | Acceptance 1–3, 5–7 | #4: `409` aturan 23 pada B4-08 (bukti API dan log); #8: C-10 Putaran B2 |
| `FE-IGD-060` | Acceptance 1–5 | #6: laporan task |
| `FE-IGD-061` | Acceptance 1–5 | #6: C-10 Putaran B2 |
| `BE-IGD-072` | — | #8 lewat R1-01 (bagian 7) |
| `BE-IGD-077` | Tetap 🟡 | Menunggu `IGD-OQ-132` |

---

## 4. Putaran D2 — Ruang Kerja Dokter IGD (prioritas 2)

Folder bukti: `test-with-agy/igd/uji-putaran-d2-<YYYYMMDD>/`. Putaran 2 (9 Oktober 2026, `IGD-DEC-236`) mencakup sebagian
acceptance pada kode lama; putaran ini mengulang seluruhnya pada build terbaru.

**Data (disiapkan lewat layar):** **P-D2-A** kunjungan berjalan, DPJP `ranger.biru`, belum ada diagnosis dan kajian medis;
**P-D2-B** kunjungan berjalan tanpa DPJP; **P-D2-C** kunjungan _Selesai_; opsional **P-D2-D** kunjungan yang DPJP-nya
dialihkan dari `ranger.biru` ke dokter kedua. Untuk regresi (opsional): satu encounter poliklinik dan satu pasien rawat inap.
Tanpa data regresi, tulis `NOT RUN — data tidak tersedia`.

Basis path di bawah: `/api/v1/health-services/emergency-installation-management` (disingkat **EIM**) dan
`/api/v1/health-services/clinical-management` (disingkat **CM**).

| ID | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- |
| D2-A01 | **API** dokter | `GET EIM/emergency-visits?doctorId={ranger}&ongoing=true` | Hanya kunjungan yang penugasan berjalannya `ranger.biru`; P-D2-B dan P-D2-D tidak ikut; tanpa `Completed`/`Cancelled` | `BE-IGD-065` acc 1, 3 |
| D2-A02 | **API** dokter | `GET EIM/emergency-visits` tanpa `doctorId` dan `ongoing`; lalu `?ongoing=true&search=<nama P-D2-A>` dan `?ongoing=true&awaitingClosure=true` | Catat `totalCount` dan urutan; gabungan saringan bekerja | `BE-IGD-065` acc 2, 3 |
| D2-A03 | **API** dokter | Untuk P-D2-A dan P-D2-B: bandingkan `activeDoctorId`/`activeDoctorName` di daftar dan `GET EIM/emergency-visits/{id}` dengan `GET EIM/emergency-doctor-assignments/active?emergencyVisitId=` | Sama ruas demi ruas; `null` untuk P-D2-B. Detail tetap memuat `isAwaitingClosure`, `awaitingClosureReason`, dan identitas pasien | `BE-IGD-065` acc 4, 5 |
| D2-U01 | Dokter | Buka `/doctor-emergency` | _Pasien saya_ aktif bawaan; tiap baris menampilkan nama DPJP atau _belum ada DPJP_; ganti ke _Semua pasien IGD_ menampilkan pasien lain. Rekam jaringan memuat `doctorId` dan `ongoing=true` | `FE-IGD-045` acc 1, `AT-IGD-200` |
| D2-U02 | Dokter | Cari dengan nama, No. RM, dan nomor kunjungan | Daftar tersaring | `FE-IGD-045` acc 2 |
| D2-U03 | Dokter kedua tanpa pasien | Buka `/doctor-emergency` | Kalimat _Pasien saya_ kosong tampil; catat persis | `FE-IGD-045` acc 3 |
| D2-U04 | Dokter | Pilih P-D2-A; lalu buka `/doctor-emergency?visitId=<P-D2-A>` langsung | Kartu: identitas, unit, status, waktu tiba, DPJP, alergi, diagnosis kerja atau _Belum ada diagnosis_; `?visitId=` langsung memilih pasien itu. Angka ringkasan sama dengan `totalCount` API | `FE-IGD-045` acc 5, 6 |
| D2-U05 | Dokter | Periksa seluruh layar yang dibuka di putaran ini | Tidak ada GUID mentah dan tanggal `0001` yang tampil. `403` (bila ada) tampil sebagai pesan | `FE-IGD-045` acc 7 |
| D2-U06 | Dokter | Tab _Pengkajian Medis_ P-D2-A → tulis kajian medis awal → simpan | Tersimpan dan tampil; jaringan tanpa `inpEpisodeId` | `FE-IGD-046` acc 1, `BE-IGD-066` acc 1, `AT-IGD-201` |
| D2-U07 | Dokter | Buat kajian medis awal kedua; lalu buat kajian ulang | Kajian awal kedua ditolak `409` dan pesannya tampil apa adanya (catat persis); kajian ulang tersimpan | `FE-IGD-046` acc 2, `BE-IGD-066` acc 2, `AT-IGD-202` |
| D2-A04 | **API** dokter | `POST CM/patient-assessments` kajian medis untuk P-D2-C | `409`; catat kalimat persis | `BE-IGD-066` acc 3 |
| D2-A05 | **API** perawat | `POST CM/patient-assessments` kajian medis untuk P-D2-A | `403` | `BE-IGD-066` acc 4 |
| D2-U08 | Dokter | Tab _Pengkajian Medis_: daftar diagnosis P-D2-A sebelum ada diagnosis | _Belum ada diagnosis_ | `FE-IGD-046` acc 3 |
| D2-U09 | Dokter | Tab _Catatan Dokter_ → catatan SOAP dengan diagnosis kerja ICD-10 → simpan sebagai draf | Draf tampil di riwayat tab ini | `FE-IGD-047` acc 1, 3, `AT-IGD-203` |
| D2-U10 | Dokter | Muat ulang data pasien | Diagnosis tadi tampil di kartu pasien sebagai _Diagnosis kerja_ | `FE-IGD-047` acc 6 |
| D2-U11 | Dokter | Tab _Resep_ dengan catatan terbuka → buat satu resep | Resep tampil di tab _Resep_ dokter | `FE-IGD-049` acc 1, `AT-IGD-204` |
| D2-U12 | Perawat | Tab _Resep_ layar perawat P-D2-A | Resep yang sama tampil | `FE-IGD-049` acc 2 |
| D2-U13 | Dokter | Selesaikan catatan dokter | Catatan terkunci; isinya tidak dapat diubah dari layar | `FE-IGD-047` acc 2 |
| D2-U14 | Rizki | Baca fakta tagih encounter P-D2-A lewat layar Billing yang sudah ada (penguji **tidak** menjalankan SQL) | Ada fakta resep; tidak ada fakta `ConsultationCompleted` untuk konsultasi itu | `BE-IGD-068` acc 1, `FE-IGD-049` acc 3 |
| D2-U15 | Dokter kedua | Coba ubah catatan yang ditulis `ranger.biru`, atau tulis catatan pada P-D2-C | Penolakan server tampil apa adanya di tempat aksi | `FE-IGD-047` acc 5 |
| D2-U16 | Dokter | Sub-tab CPPT → baca, lalu tulis satu entri | Entri tersimpan dan tampil | `FE-IGD-047` acc 4 |
| D2-U17 | Dokter | _Catatan Saya_ (`/doctor-emergency/my-notes`) | Catatan yang diselesaikan di D2-U13 tampil; tidak ada daftar draf, dan halaman menyatakan draf IGD dibuka dari tab _Catatan Dokter_. Catat `serviceContext` dari rekam jaringan | `FE-IGD-048` acc 1, 3 |
| D2-U18 | Dokter | Tambahkan addendum pada catatan itu | Addendum tampil di bawah catatan dengan pelaku dan waktu; isi lama tidak berubah | `FE-IGD-048` acc 2 |
| D2-U19 | Dokter | Tab _Resep_ pada pasien tanpa catatan terbuka | Kalimat penuntun tampil; catat persis. Catat juga perilaku resep yang `ClinicId`-nya kosong bila terjadi | `FE-IGD-049` acc 4, 5 |
| D2-A06 | **API** dokter | `GET CM/doctor-consultations/encounters/{encounterId}/soap-timeline` untuk P-D2-A (draf dan selesai), untuk kunjungan IGD tanpa catatan, untuk encounter poliklinik, dan untuk ID acak | `200` dengan `totalCount` sesuai; `200` dengan `items` kosong; `400`; `404` | `BE-IGD-067` acc 1–3 |
| D2-R01 | Dokter rawat inap | **Opsional regresi**: tab _Pengkajian Medis_, _Catatan Dokter_, CPPT, _Resep_, _Catatan Saya_ untuk satu pasien rawat inap | Perilaku sama seperti sebelumnya; `GET CM/doctor-consultations/episodes/{episodeId}/soap-timeline` tetap `200` | `FE-IGD-045` acc 8, `046` acc 4, `047` acc 7, `048` acc 4, `049` acc 6, `BE-IGD-066` acc 5, `BE-IGD-067` acc 4 |

---

## 5. Putaran D3 — Tindakan dan tindak lanjut (prioritas 3)

Folder bukti: `test-with-agy/igd/uji-putaran-d3-<YYYYMMDD>/`. `FE-IGD-050` dan `051` diulang karena bukti putaran 3 tidak
sah (`IGD-DEC-296`: dilayani `next dev`, tangkapan layar berulang).

**Data:** **P-D3-A** kunjungan berjalan, DPJP `ranger.biru`, **tanpa diagnosis**; **P-D3-B** kunjungan berjalan tanpa DPJP;
**P-D3-C** kunjungan _Selesai_.

| ID | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- |
| D3-U01 | Perawat | Tab _Tindakan_ P-D3-A → _Catat tindakan keperawatan_ → simpan | Tampil dengan pelaksana perawat dan DPJP sebagai dokter | `FE-IGD-051` acc 1, `AT-IGD-206` |
| D3-U02 | Perawat | Simpan dua kali cepat | Satu baris; `idempotencyKey` sama di rekam jaringan | `FE-IGD-051` acc 2 |
| D3-U03 | Perawat | Catat tindakan untuk P-D3-B, lalu untuk P-D3-C | Pesan server tampil apa adanya (catat persis); nol baris baru | `FE-IGD-051` acc 3, `AT-IGD-207` |
| D3-U04 | Dokter | Tab _Tindakan_ layar dokter P-D3-A tanpa catatan terbuka | Layar menuntun membuat atau membuka catatan | `FE-IGD-050` acc 4 |
| D3-U05 | Dokter | Buka catatan → tambah tindakan dokter → simpan | Tampil di daftar; daftar juga memuat tindakan keperawatan D3-U01 beserta asalnya | `FE-IGD-050` acc 1, 3, `AT-IGD-205` |
| D3-U06 | Rizki | Ringkasan tagihan pasien P-D3-A lewat layar Billing yang sudah ada | Tindakan dokter D3-U05 terbaca | `FE-IGD-050` acc 2 |
| D3-U07 | Perawat | Tab _Tindakan_ perawat P-D3-A | Daftar membaca tindakan klinis umum dokter dan perawat; tabel lama hanya tampil di bagian riwayat bila berisi | `FE-IGD-051` acc 4 |
| D3-A01 | **API** dokter | `POST EIM/emergency-dispositions` dengan `dispositionStatus` `2`, `3`, `4`, lalu `1` | `400` untuk 2–4 tanpa baris baru (catat kalimat); `1` tersimpan _Draft_ | `BE-IGD-070` acc 1, `AT-IGD-212` |
| D3-U08 | Dokter | Tab _Tindak Lanjut_ P-D3-A → buat tindak lanjut _Pulang_ | Rekam jaringan menunjukkan `dispositionStatus` `1`; tampil _Draf_ | `FE-IGD-054` acc 1 |
| D3-U09 | Dokter | _Konfirmasi_ tanpa diagnosis | `409` tampil apa adanya di modal; status tetap _Draf_; tautan ke tab _Catatan Dokter_ bekerja | `FE-IGD-054` acc 2, `BE-IGD-070` acc 2, `AT-IGD-211` |
| D3-A02 | **API** dokter | Tambah diagnosis `Differential` saja lewat catatan dokter → konfirmasi | Tetap `409` | `BE-IGD-070` acc 4 |
| D3-U10 | Dokter | Tambah diagnosis kerja ICD-10 aktif → _Konfirmasi_ | Berhasil; status _Dikonfirmasi_ tanpa muat ulang halaman | `FE-IGD-054` acc 3, `BE-IGD-070` acc 3 |
| D3-U11 | Dokter | Periksa aksi di tab dokter; buat satu tindak lanjut draf lain lalu _Batalkan_ | _Batalkan_ meminta alasan dan berhasil tanpa pemeriksaan diagnosis; aksi _Jalankan_ tidak ada di layar dokter | `FE-IGD-054` acc 4, `BE-IGD-070` acc 5 |
| D3-A03 | **API** dokter | `PATCH EIM/emergency-dispositions/{acak}/disposition-status`; lalu rangkaian status tidak sah (misalnya _Draft_ → _Executed_) pada tindak lanjut tanpa diagnosis | `404`; lalu `400` (bukan `409`) | `BE-IGD-070` acc 6 |
| D3-U12 | Perawat | Tab _Tindak Lanjut_ perawat P-D3-A | Draf tampil tanpa aksi; tidak ada formulir buat, _Konfirmasi_, atau _Batalkan_; pada yang _Dikonfirmasi_ hanya ada _Jalankan_ | `FE-IGD-055` acc 2 |
| D3-U13 | Perawat | _Jalankan_ tindak lanjut _Pulang_ yang dikonfirmasi di D3-U10 | Kunjungan tertutup sesuai `AT-IGD-186`; penanda dan kalimat penutupan sama seperti sebelumnya | `FE-IGD-055` acc 1, 3, `BE-IGD-070` acc 5, `AT-IGD-213` |

---

## 6. Putaran L1 — Layar lama (prioritas 4)

Folder bukti: `test-with-agy/igd/uji-putaran-l1-<YYYYMMDD>/`. Data: **P-L1-A** kunjungan berjalan sesudah triage;
**P-L1-B** kunjungan yang akan ditutup di tab lain.

| ID | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- |
| L1-01 | Perawat | Buka form triage P-L1-B di tab 1; tutup kunjungannya di tab 2; simpan triage di tab 1 | Pesan _"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."_, **bukan** _"Gagal menyimpan pemeriksaan triage."_ | `FE-IGD-012` kriteria 1 |
| L1-02 | Perawat | Picu satu penolakan transisi status kunjungan dari layar | Pesan menyebut status kunjungan saat ini; catat persis | `FE-IGD-012` kriteria 2 |
| L1-03 | Perawat | P-L1-A dalam penanganan → lakukan penilaian ulang triage → periksa daftar kunjungan, detail, dan ruang kerja | Tidak ada layar yang menampilkan _Triaged_ / _Sudah ditriage_ sebagai status kunjungan | `FE-IGD-012` kriteria 3 |
| L1-04 | Perawat | Isi pengkajian keperawatan P-L1-A (semua isian yang tampil) → simpan → tutup → buka ulang | Seluruh isian kembali. Isian yang hilang sesudah buka ulang dicatat satu per satu | `FE-IGD-013` kriteria 1, 3 |
| L1-05 | Perawat | Tanda vital, diagnosis, tindakan, dan CPPT: simpan masing-masing, lalu buka ulang | Keempatnya kembali | `FE-IGD-013` kriteria 4 |
| L1-06 | Perawat | Layar asuhan keperawatan: delapan kolom pengkajian dan pemantauan observasi → simpan → buka ulang | Isian kembali; pemantauan tampil | `FE-IGD-022` |
| L1-07 | Perawat | Tab _Penunjang_ P-L1-A | Hanya pesanan milik encounter P-L1-A (rekam jaringan: `lab-orders` membawa `encounterId`); tidak ada tulisan _modul Radiologi belum ada_ | `FE-IGD-022` (cacat lama `IGD-EV-112`, `115`) |

---

## 7. Putaran R1 — Uji langkah mundur migration (Rizki)

Bukan uji layar dan bukan untuk Antigravity. Caranya sama dengan uji `BE-IGD-044` yang lulus 21 September 2026
([evidence](../evidence/2026-09-21-uji-down-migration-be-igd-044.md)): kontainer PostgreSQL lokal dari image yang sudah ada,
seluruh rantai migration diterapkan ke basis data kosong, `Down` ke migration sebelumnya, `Up` kembali, lalu sidik jari
skema dibandingkan. **Basis data dev tidak disentuh.**

| ID | Task | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- |
| R1-01 | `BE-IGD-072` acc 8 | `Down` dari head ke `20261012041938_AddAdmissionTransferOrigin` (melewati `20261012080102_AddEmergencyAdmissionRequest` dan migration sesudahnya), lalu `Up` ke head | Penjaga `Down()` bekerja; tabel `EmgAdmissionRequest` dan `EmgAdmissionRequestEvent` hilang lalu kembali identik; skema tabel lain tidak berubah |
| R1-02 | `BE-IGD-026` kriteria 1, `BE-IGD-031` kriteria 2 | `Down` dari `20260826090500_ImplementIgdFullPatientJourney` ke `20260826025451_AddTableBillingKasirPArt3`, termasuk `RENAME` balik | Selesai tanpa galat; `QueueId` kembali wajib dan nama tabel kembali. **Catatan:** rantainya melewati sangat banyak migration sesudah 26 Agustus. Bila tidak praktis, Rizki boleh mengecualikan kedua kriteria ini secara tertulis — dicatat di decision log, bukan dicentang |

---

## 8. Bukti dan laporan

Setiap folder putaran memuat: `kode-awal.txt`, `kode-akhir.txt`, `data-uji.json`, satu PNG layar penuh per skenario
(nama diawali ID), `jaringan-<ID>.json`, folder `skrip/` dengan setiap versi, `percobaan.json`, `hasil.json`, dan
`laporan.md`. `hasil.json` berisi satu objek per ID:

```json
{ "id": "B4-06", "hasil": "PASS", "mulai": "2026-10-18T09:12:03+07:00", "selesai": "2026-10-18T09:20:41+07:00",
  "akun": "rendy.saputra@rsmmc.local", "pasien": "P-B4-A", "bukti": ["B4-06-akhir.png", "jaringan-B4-06.json"],
  "catatan": "Unit dan kelas terisi awal; DPJP usulan tidak terisi" }
```

`laporan.md` memuat tabel yang sama dengan `hasil.json`, daftar penyimpangan dari aturan bagian 1 (bila ada), dan
keterbatasan. Laporan **tidak** memuat kesimpulan status task.

---

## Lampiran A — Prompt Antigravity

Salin satu prompt per putaran. Ganti `<YYYYMMDD>` dengan tanggal uji.

### A.1 Putaran B4

```text
Kamu adalah penguji untuk modul IGD Quilvian. Jalankan HANYA Putaran B4 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
(baca bagian 1, 2, 3, dan 8 lebih dulu, seluruhnya).

Aturan paling penting (lengkapnya di bagian 1):
- Jangan memakai SuperAdmin, jangan mengubah Akses Role, jangan menjalankan SQL apa pun, termasuk SELECT.
- Jangan mengubah kode atau dokumen yang sudah ada. Tidak ada perbaikan kode selama uji. Cacat = FAIL, lanjutkan skenario lain.
- Catat kode-awal.txt sebelum mulai dan kode-akhir.txt sesudah selesai (git rev-parse HEAD, git status --short kedua repo,
  jam DLL backend, .next/BUILD_ID). Jalankan juga git grep pada prasyarat P0.
- Build produksi standalone (npm run start), bukan next dev; layar 1440x900; tanpa lencana N.
- Sandi dari variabel lingkungan QUILVIAN_*_PASSWORD; token disensor.
- Pakai tiga pasien IGD BARU (P-B4-A, P-B4-B, P-B4-C). Catat nama, No. RM, nomor kunjungan asli di data-uji.json.
- Satu baris hasil per ID (B4-01 sampai B4-20). Dilarang menggabungkan ID. Hasil hanya PASS, FAIL, NOT RUN, NOT AVAILABLE.
- Bila P1 tidak menemukan tombol Tolak di Antrean Transfer, tulis B4-11 sampai B4-16 NOT AVAILABLE. Jangan memakai API,
  skrip, atau basis data sebagai pengganti tombol.
- B4-06 harus sampai admisi benar-benar selesai (POST admission-transfers/admit 201), bukan berhenti di antrean.
- B4-20: setelah admisi P-B4-C, jangan melakukan aksi apa pun pada kunjungan IGD; cukup muat ulang dan catat statusnya.
- Rekam jaringan semua /api/v1/ per skenario, simpan setiap versi skrip, catat setiap percobaan termasuk yang gagal.
- Jangan menyatakan status task (selesai/✅). Itu dinilai agent pengembang.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b4-<YYYYMMDD>/.
Tulis laporan.md di folder itu, lalu salin sebagai berkas BARU ke
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-b4.md.
Di akhir, sebutkan path log backend hari itu (NewQuilvianSystemBackend/Logs/quilvian-backend-<YYYYMMDD>.json).
Bila prasyarat bagian 3.1 belum terpenuhi (akun, hak akses, pasien, perawat ruangan), berhenti dan laporkan apa yang kurang.
```

### A.1b Putaran B5 (pengganti B4)

```text
Kamu adalah penguji modul IGD Quilvian. Jalankan HANYA Putaran B5 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
Baca seluruh bagian 1, 2, 3B, dan 8 lebih dulu. Putaran B4 tidak sah; baca bagian "Penilaian agent pengembang" di
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-laporan-uji-putaran-b4.md agar kesalahannya tidak terulang.

FASE S - PERSIAPAN (keputusan pemilik IGD-DEC-320)
- SuperAdmin BOLEH dipakai hanya untuk:
  1. membuat akun uji: dokter IGD kedua (boleh pakai ulang dokter.igd.b4), petugas admisi (boleh pakai ulang admisi.b4),
     perawat ruangan, dan petugas loket bila belum ada - beserta data HR-nya;
  2. menugaskan perawat ruangan ke unit bangsal tujuan;
  3. memberi Akses Role sesuai tabel 3B.2 (petugas admisi: InpatientAdmissionTransfer Read, InpatientEpisode Create,
     InpatientBedOccupancy Read/Create/Update, Doctor Read, plus hak pembayaran/deposit yang diminta wizard;
     dokter kedua: EmergencyAdmissionRequest Read/Sign, EmergencyVisit Read);
  4. menyalakan bypass geolokasi (enable) pada SETIAP akun yang dipakai di B5, termasuk akun lama seperti
     dimas.kurniawan dan ranger.biru. Tulis daftar akun itu di persiapan.json.
- DILARANG: membuat atau mengubah pasien, encounter, kunjungan, bed, atau data master lain lewat SuperAdmin atau rute admin;
  mengubah akun lama selain bypass geolokasi; memberi hak di luar tabel 3B.2; menjalankan SQL.
- Catat setiap tindakan fase S di persiapan.json (jam, akun, layar/endpoint, sebelum-sesudah) + PNG Akses Role sebelum/sesudah.
- Bila ada langkah gagal (mis. galat 500 saat penugasan unit) atau bangsal tujuan tidak punya bed kosong: BERHENTI dan laporkan.
- Pasien P-B5-A sampai P-B5-E dibuat lewat layar (loket -> triage -> DPJP -> catatan dokter + diagnosis ICD-10 -> tindak
  lanjut), atau lewat API dengan akun peran yang berhak. Jangan pakai rute admin, jangan PATCH visit-status.
- Sesudah fase S selesai: logout SuperAdmin dan tutup sesinya.

FASE T - SKENARIO B5-00 SAMPAI B5-14
- SuperAdmin DILARANG. Hanya akun peran.
- B5-00 wajib lulus dulu: hentikan next dev, jalankan npm run build lalu npm run start (port 3000). Simpan PNG halaman login
  TANPA lencana "N" dan baris perintah proses node (harus next start atau server.js) ke kode-awal.txt. Setiap akun login
  tanpa 403. Bila gagal, berhenti.
- Status tindak lanjut lewat API: Draft 1, Confirmed 2, Executed 3, Cancelled 4. B5-07 dan B5-12 memakai 4.
- B5-05 harus sampai admisi benar-benar selesai (POST admission-transfers/admit 201).
- B5-08: yang menekan Terima Dokumen dan Catat Tiba adalah perawat ruangan, bukan perawat IGD.
- B5-13: dialog dokter lama dibiarkan terbuka sebelum DPJP dialihkan, lalu Tandatangani ditekan di dialog itu.
- Satu baris hasil per ID; dilarang menggabungkan ID; hasil hanya PASS, FAIL, NOT RUN, NOT AVAILABLE.
- Pesan galat dan pesan sukses dicatat persis. Setiap PNG diambil sesudah aksi skenarionya sendiri; PNG identik antar-skenario
  tidak sah.
- Sandi semua akun, termasuk SuperAdmin, hanya dari variabel lingkungan (QUILVIAN_SUPERADMIN_PASSWORD, QUILVIAN_*_PASSWORD).
  Skrip yang memuat sandi tertulis membuat putaran tidak sah.
- Rekam jaringan semua /api/v1/ per skenario; simpan setiap versi skrip; catat SETIAP percobaan di percobaan.json, termasuk
  yang gagal, login gagal, dan seluruh tindakan fase S.
- Jangan mengubah kode atau dokumen yang sudah ada. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b5-<YYYYMMDD>/ (persiapan.json, kode-awal.txt,
kode-akhir.txt, data-uji.json, PNG, jaringan-<ID>.json, skrip/, percobaan.json, hasil.json, laporan.md). Salin laporan.md
sebagai berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-b5.md.
Di akhir sebutkan path log backend hari itu (NewQuilvianSystemBackend/Logs/quilvian-backend-<YYYYMMDD>.json).
```

### A.2 Putaran D2

```text
Kamu adalah penguji untuk modul IGD Quilvian. Jalankan HANYA Putaran D2 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
(baca bagian 1, 2, 4, dan 8 lebih dulu, seluruhnya).

Aturan bagian 1 berlaku penuh: tanpa SuperAdmin, tanpa mengubah Akses Role, tanpa SQL apa pun (termasuk SELECT),
tanpa mengubah kode atau dokumen, build produksi standalone tanpa lencana N, sandi dari variabel lingkungan,
kode-awal.txt dan kode-akhir.txt wajib, satu baris hasil per ID tanpa penggabungan, pesan galat dicatat persis,
rekam jaringan semua /api/v1/, setiap versi skrip disimpan, setiap percobaan dicatat.

Khusus D2:
- Siapkan pasien P-D2-A, P-D2-B, P-D2-C (dan P-D2-D bila ada dokter kedua) lewat layar; catat di data-uji.json.
- D2-U14 dikerjakan Rizki lewat layar Billing; kamu cukup mencatat ID encounter dan jam penyelesaian catatan.
- Tanpa akun dokter kedua atau dokter rawat inap, tulis skenario terkait NOT RUN beserta sebabnya.
- Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2-<YYYYMMDD>/ dan salin laporan.md sebagai
berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-d2.md.
Sebutkan path log backend hari itu di akhir.
```

### A.3 Putaran D3

```text
Kamu adalah penguji untuk modul IGD Quilvian. Jalankan HANYA Putaran D3 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
(baca bagian 1, 2, 5, dan 8 lebih dulu, seluruhnya).

Aturan bagian 1 berlaku penuh (tanpa SuperAdmin, tanpa SQL, tanpa ubah kode/dokumen, build produksi tanpa lencana N,
kode-awal/kode-akhir, satu baris per ID, pesan galat persis, rekam jaringan, simpan setiap versi skrip).
Bukti lama FE-IGD-050/051 ditolak karena dilayani next dev dan tangkapan layarnya berulang — pastikan setiap PNG
diambil sesudah aksi skenarionya sendiri dan jamnya tercatat di percobaan.json.
D3-U06 dikerjakan Rizki lewat layar Billing. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d3-<YYYYMMDD>/ dan salin laporan.md sebagai
berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-d3.md.
Sebutkan path log backend hari itu di akhir.
```

### A.4 Putaran L1

```text
Kamu adalah penguji untuk modul IGD Quilvian. Jalankan HANYA Putaran L1 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
(baca bagian 1, 2, 6, dan 8 lebih dulu, seluruhnya).

Aturan bagian 1 berlaku penuh. Untuk L1-04 sampai L1-06, ambil PNG sebelum simpan dan sesudah buka ulang, dan catat
setiap isian yang hilang satu per satu. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1-<YYYYMMDD>/ dan salin laporan.md sebagai
berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-l1.md.
Sebutkan path log backend hari itu di akhir.
```

### A.5 Prompt untuk agent pengembang sesudah uji

```text
Periksa hasil Putaran <ID> modul IGD. Bukti mentah ada di
QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-<id>-<YYYYMMDD>/ dan log backend di
NewQuilvianSystemBackend/Logs/quilvian-backend-<YYYYMMDD>.json. Panduannya
docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md.
Putuskan per skenario dari bukti mentah (PNG, JSON, skrip, percobaan.json) dan log backend, bukan dari laporan penguji.
Periksa kode-awal.txt lawan kode-akhir.txt, lencana N, sensor sandi, dan penulisan ke Akses Role atau basis data.
Sesudah itu perbarui roadmap, traceability, MODULE-STATUS, dan decision log (MODULE BLUEPRINT MODE), tanpa commit.
```

### A.6 Claude — uji `Down()` `BE-IGD-072` (R1-01)

Jalankan di mesin yang punya Docker Desktop dan DLL backend hasil build Rizki. Uji ini berat; pakai PC kantor bila laptop
sering mati saat beban tinggi.

```text
/quilvian-engineering-skills:build-module-backend
Task BE-IGD-072, HANYA acceptance 8: uji penjaga Down() migration 20261012080102_AddEmergencyAdmissionRequest.
Rujukan: NewQuilvianSystemBackend/docs/module-blueprints/igd/roadmap/backend-roadmap.md (kartu BE-IGD-072) dan
docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md bagian 7 (R1-01).
Wewenang dari saya, Rizki Gunawan (Product/Domain Owner IGD):
- menjalankan kontainer PostgreSQL lokal dari image yang SUDAH ADA (--pull never), tanpa instalasi atau unduhan;
- dotnet ef database update --connection <kontainer lokal> --no-build memakai DLL hasil build saya: terapkan seluruh rantai
  ke basis data kosong, Down ke 20261012041938_AddAdmissionTransferOrigin, lalu Up kembali ke head;
- kueri baca-saja untuk sidik jari skema (kolom, index, constraint) di kontainer itu saja.
Larangan: basis data dev tidak disentuh; tidak menyunting source; tidak menjalankan dotnet build; tidak commit atau push.
Ikuti cara dan bentuk bukti di docs/module-blueprints/igd/evidence/2026-09-21-uji-down-migration-be-igd-044.md.
Tulis bukti ke docs/module-blueprints/igd/evidence/<YYYY-MM-DD>-uji-down-migration-be-igd-072.md, perbarui laporan task
BE-IGD-072 serta status kartu, register, dan traceability. Hapus kontainer dan volumenya di akhir.
```

### A.7 Claude — menilai hasil Putaran B5

```text
/quilvian-engineering-skills:manage-module-blueprint
MODULE BLUEPRINT MODE untuk docs/module-blueprints/igd/**. Nilai hasil Putaran B5 modul IGD.
Bukti mentah: QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b5-<YYYYMMDD>/. Log backend:
NewQuilvianSystemBackend/Logs/quilvian-backend-<YYYYMMDD>.json. Panduan: docs/module-blueprints/igd/testing/
2026-10-17-panduan-uji-sisa-modul-igd.md bagian 3B. Konteks: IGD-DEC-319 sampai IGD-DEC-321 dan IGD-FACT-123 sampai
IGD-FACT-125 di 00-interview-decisions.md.
Putuskan per skenario dari bukti mentah dan log, bukan dari laporan penguji. Wajib periksa: lencana N di setiap PNG (pojok
kiri bawah), PNG identik antar-skenario, sandi tertulis di skrip, kode-awal lawan kode-akhir, persiapan.json lawan log
(SuperAdmin hanya untuk akun, penugasan unit, Akses Role, dan bypass geolokasi sesuai IGD-DEC-320; tidak ada pasien, bed,
atau encounter lewat SuperAdmin atau rute admin), dan tidak ada SuperAdmin di fase T.
Sesudah itu: tambahkan bagian penilaian ke laporan B5 di testing/; perbarui status BE-IGD-074, FE-IGD-060, FE-IGD-061
(dan BE-IGD-072 bila bukti R1-01 sudah ada) di kartu, register, grafik, dan traceability; perbarui MODULE-STATUS dan decision
log dengan nomor berikutnya yang kosong. BE-IGD-077 tetap 🟡 menunggu IGD-OQ-132. Jangan commit.
```

### A.8 Claude — kesiapan `MVP-11` dalam lingkup IGD

```text
/quilvian-engineering-skills:verify-module-readiness
Modul IGD, slice MVP-11 (EPIC IGD-15), dalam lingkup IGD sesuai IGD-DEC-321. Audit kesiapan ujung-ke-ujung terhadap
keputusan, kontrak revisi 13 (IGD-DEC-318), source backend dan frontend terkini, bukti Putaran B5, dan bukti R1-01.
Pisahkan dengan jelas apa yang siap dari sisi IGD dan apa yang masih menunggu Rawat Inap (IGD-OQ-132: tombol Tolak,
penutupan kunjungan pada alur pasien tiba lebih dulu). Sebutkan juga syarat rilis R3.18.5 yang belum terpenuhi.
Jangan mengerjakan perbaikan.
```
