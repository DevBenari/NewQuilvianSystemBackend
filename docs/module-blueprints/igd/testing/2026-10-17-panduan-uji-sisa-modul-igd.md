# Panduan Uji Sisa Modul IGD — Putaran B4…B7, K1, P1, D2, D3, L1, dan R1

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
| **B5** | Transfer dalam lingkup IGD: sampai _admit_, penutupan alur normal, pembatalan, kewenangan DPJP, unit tujuan salah (bagian 3B) | `BE-IGD-074`, `FE-IGD-060`, `FE-IGD-061`; regresi `FE-IGD-059` | 1 | Antigravity (persiapan boleh SuperAdmin, `IGD-DEC-320`, `322`) |
| **B6** | Satu langkah _admit_ sungguhan, penolakan pembatalan sesudah diproses, penutupan kunjungan alur normal (bagian 3C) — dibuat sesudah penilaian B5 (`IGD-FACT-126`, `127`). **Dijalankan 18 Oktober 2026:** `BE-IGD-074` dan `FE-IGD-061` ✅; B6-06 gagal karena cacat IGD (`IGD-FACT-128`) | `BE-IGD-074`, `FE-IGD-061` | 1 | Antigravity; Rizki membuka akses bed (3C.1) |
| **B7** | Penutupan kunjungan sesudah _admit_ (bagian 3D): ulang B6-06 sesudah `BE-IGD-083`; alur pasien tiba dulu menyusul sesudah Rawat Inap memasang panggilan (`IGD-REQ-003`). **Bagian 1 dijalankan 18 Oktober 2026: lulus, `BE-IGD-083` ✅** (`IGD-FACT-133`); bagian 2 belum | `BE-IGD-083`; `BE-IGD-077` acceptance 4 | 1 | Antigravity, sesudah `BE-IGD-083` dikerjakan dan dibuild Rizki (delta disetujui `IGD-DEC-324`) |
| **K1** | Tagihan IGD sampai kasir: tunai, asuransi, PKS (tindakan perawat, laboratorium, obat, jasa kunjungan) dan satu pasien transfer dengan dua invoice (bagian 3E) — _ditambahkan 18 Oktober 2026 malam_ | `BE-IGD-084`; `IGD-UNK-21`, `IGD-UNK-22`; demo Kasir IGD 21 Oktober | 1 | Antigravity; bagian 2 sesudah `BE-IGD-084` dibangun dan item serta tarif _Jasa kunjungan IGD_ ada (`IGD-UNK-19`) |
| **P1** | Pasien prioritas dari reservasi sampai Form Permintaan Rawat Inap, termasuk _check-in_ memakai encounter loket dan triage yang tidak dipengaruhi membership (bagian 3F) — _ditambahkan 18 Oktober 2026 malam_ | `BE-IGD-085`, `BE-IGD-080`, `FE-IGD-067`; demo pasien prioritas 20 Oktober | 1 | Antigravity, sesudah `BE-IGD-085` dibangun Rizki |
| **D2** | Ruang Kerja Dokter IGD: daftar pasien, kajian medis, catatan dokter, Catatan Saya, resep | `BE-IGD-065`…`068`, `FE-IGD-045`…`049` | 2 | Antigravity |
| **D3** | Tindakan dan tindak lanjut di layar dokter dan perawat | `BE-IGD-070`, `FE-IGD-050`, `051`, `054`, `055` | 3 | Antigravity |
| **L1** | Layar lama yang belum pernah dibuktikan lewat layar | `FE-IGD-012`, `013`, `022` | 4 | Antigravity |
| **R1** | Uji langkah mundur migration di salinan basis data. **R1-01 dijalankan 18 Oktober 2026: lulus, `BE-IGD-072` ✅** ([bukti](../evidence/2026-10-18-uji-down-migration-be-igd-072.md)); R1-02 belum | `BE-IGD-072` (acceptance 8), `BE-IGD-026` (kriteria 1), `BE-IGD-031` (kriteria 2) | boleh paralel | Rizki |

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
| **S — Persiapan** | Atas keputusan pemilik `IGD-DEC-320` dan `IGD-DEC-322`, login SuperAdmin **boleh** untuk perubahan apa pun yang dibutuhkan persiapan, **lewat aplikasi** (layar atau API): akun uji dan data HR-nya, penugasan unit, Akses Role, bypass geolokasi (nilai _enable_) pada setiap akun yang dipakai uji termasuk akun lama seperti `dimas.kurniawan` dan `ranger.biru`, kesiapan bed dan data master (misalnya menjadikan _Bed Melati 01/02_ siap pakai), serta data pasien uji. Akun uji B4 (`admisi.b4@rsmmc.local`, `dokter.igd.b4@rsmmc.local`) boleh dipakai ulang | SQL langsung ke basis data. Setiap perubahan tetap wajib dicatat (S1) |
| **T — Skenario** | Akun peran saja | SuperAdmin dalam bentuk apa pun. Logout SuperAdmin dan tutup sesinya sebelum B5-00 |

Aturan bagian 1 tetap berlaku; A1 hanya dilonggarkan untuk fase S seperti di atas. Tambahan khusus B5:

| No | Aturan |
| ---: | --- |
| S1 | Setiap tindakan fase S dicatat di `persiapan.json` (jam, akun, layar atau endpoint, keadaan sebelum dan sesudah), ditambah PNG layar Akses Role sebelum dan sesudah diubah. Pakai layar admin bila tersedia. Daftar akun yang bypass geolokasinya dinyalakan ditulis lengkap, supaya Rizki dapat mengembalikannya sesudah uji bila perlu |
| S2 | Sandi semua akun, termasuk SuperAdmin, hanya dari variabel lingkungan (`QUILVIAN_SUPERADMIN_PASSWORD`, `QUILVIAN_*_PASSWORD`). Skrip yang memuat sandi tertulis membuat putaran tidak sah |
| S3 | Akun uji baru dinamai jelas, misalnya `perawat.ruangan.uji.b5@rsmmc.local`, dengan nama tampilan diakhiri _(Uji B5)_ |
| S4 | Bila bangsal tujuan tidak punya bed siap pakai, siapkan lewat SuperAdmin di aplikasi — alur kesiapan bed, atau bed uji baru — lalu catat (`IGD-DEC-322`). Bila langkah fase S gagal karena galat server (misalnya `500` saat penugasan unit, seperti pada B4), catat galatnya dan coba jalan lain lewat aplikasi; bila tetap tidak bisa, berhenti dan laporkan |
| S5 | Pasien uji sebaiknya dibuat lewat alur IGD (loket → triage → DPJP → catatan dokter dan diagnosis → tindak lanjut) supaya datanya sama dengan data nyata. SuperAdmin atau rute admin boleh dipakai bila alur itu terhambat (`IGD-DEC-322`); tulis di `persiapan.json` pasien mana yang dibuat dengan cara itu |
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

## 3C. Putaran B6 — satu langkah _admit_ dan penutupan alur normal

Folder bukti: `test-with-agy/igd/uji-putaran-b6-<YYYYMMDD>/`.

**Kenapa ada B6.** Putaran B5 membuktikan seluruh jalur IGD sebelum _admit_ (`IGD-FACT-126`). Yang tersisa hanya kriteria
yang butuh satu _admit_ sungguhan: form _Selesai_, penolakan pembatalan sesudah diproses, dan penutupan kunjungan alur
normal. B5 tertahan karena akses bed petugas admisi diatur konfigurasi `BedManagement:UnitAssignments`
(`IGD-FACT-127`). Aturan bagian 1 dan 3B.1 berlaku penuh, termasuk dua fase S dan T.

### 3C.1 Membuka langkah _admit_ — pilih satu

| Opsi | Cara | Catatan |
| --- | --- | --- |
| **1 — disarankan** | Rizki menambah **satu entri di indeks 4** `BedManagement:UnitAssignments` untuk `UserId` akun `admisi.b4@rsmmc.local` (dilihat di Manajemen Pengguna — `UserId`, bukan ID pegawai), lewat `dotnet user-secrets` **atau** `appsettings.Development.json` (di-gitignore), lalu menjalankan ulang backend. Isinya: `ServiceUnitId` `fddbe4ae-832b-484c-aba7-d6280e07c311` (Rawat Inap); `Operations` `InpatientBedManagement:Read`, `InpatientBedOccupancy:Read`, `InpatientBedOccupancy:Create`, `InpatientBedOccupancy:Update`, **`InpatientEpisode:Create`** (_ditambahkan 18 Oktober 2026 — tanpa operasi ini _admit_ ditolak `403` "Unit admisi di luar scope.", `IGD-FACT-134`_); `AssignmentReference` `UJI-IGD-B6`; `SopReference` `SOP-RWI-BED-001`; `OperationalOnly` `false`; `IsReadinessVerifier` `false` | Tidak ada berkas repo yang berubah. Bila memakai `appsettings.Development.json`, tulis kunci per indeks (indeks 4 saja) atau salin keempat entri lama lebih dulu — larik di berkas pengganti menimpa per indeks. Menguji jalur petugas admisi yang sebenarnya |
| **2** | Langkah _admit_ (B6-02) dijalankan akun SuperAdmin, yang sudah terdaftar di `UnitAssignments` | Kriteria IGD tidak bergantung pada siapa yang meng-_admit_. Pengecualian dari aturan fase T ini hanya untuk B6-02 dan wajib dicatat di `percobaan.json` |

Contoh opsi 1 dengan `dotnet user-secrets`, dijalankan Rizki di folder backend:

```powershell
$k = "BedManagement:UnitAssignments:4"
dotnet user-secrets set "${k}:UserId" "<UserId admisi.b4>"
dotnet user-secrets set "${k}:ServiceUnitId" "fddbe4ae-832b-484c-aba7-d6280e07c311"
dotnet user-secrets set "${k}:Operations:0" "InpatientBedManagement:Read"
dotnet user-secrets set "${k}:Operations:1" "InpatientBedOccupancy:Read"
dotnet user-secrets set "${k}:Operations:2" "InpatientBedOccupancy:Create"
dotnet user-secrets set "${k}:Operations:3" "InpatientBedOccupancy:Update"
dotnet user-secrets set "${k}:Operations:4" "InpatientEpisode:Create"
dotnet user-secrets set "${k}:AssignmentReference" "UJI-IGD-B6"
dotnet user-secrets set "${k}:SopReference" "SOP-RWI-BED-001"
dotnet user-secrets set "${k}:OperationalOnly" "false"
dotnet user-secrets set "${k}:IsReadinessVerifier" "false"
```

Sesudah uji, entri ini dihapus dengan `dotnet user-secrets remove` per kunci, atau dibiarkan bila Rizki memutuskan lain.

### 3C.2 Persiapan (fase S)

| No | Persiapan | Cek selesai |
| --- | --- | --- |
| S6-01 | Opsi 3C.1 yang dipilih Rizki sudah terpasang | Opsi 1: entri indeks 4 memuat kelima operasi, termasuk `InpatientEpisode:Create`; akun admisi melihat ≥ 1 bed pada langkah 6 tanpa menyimpan. _Melihat bed saja tidak membuktikan izin admit_ — bed memeriksa `InpatientBedOccupancy:Read`, _admit_ memeriksa `InpatientEpisode:Create` (`IGD-FACT-134`). Opsi 2: dicatat |
| S6-02 | Perawat ruangan (`perawat.ruangan.uji.b5@rsmmc.local`) dapat membuka tab _Transfer Pasien_ pasien IGD tanpa `403` — beri hak baca kunjungan IGD dan kepergian bila kurang; penugasan unitnya cocok dengan unit tujuan _Rawat Inap_ | Tab terbuka; daftar kepergian terbaca |
| S6-03 | Bed di unit tujuan siap pakai (seperti S-BED-02 pada B5) | ≥ 1 bed _Available_ |
| S6-04 | Dua pasien IGD **baru**: P-B6-A (tindak lanjut rawat inap _Dikonfirmasi_ ke _Rawat Inap_) dan P-B6-B (sama, tetapi **tanpa** form). P-B5-A tidak dipakai | `data-uji.json` |

### 3C.3 Skenario (fase T)

**Aturan berhenti:** bila B6-02 gagal, **berhenti**. Jangan menjalankan B6-03…06 — pada B5, menjalankan B5-07 tanpa _admit_
merusak data uji.

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B6-00 | — | Semua akun | Pemeriksaan pembuka seperti B5-00 | Tanpa lencana _N_; tanpa `403`. Bila gagal, berhenti | — |
| B6-01 | A | Perawat, dokter DPJP | _Buat Form_ (isian lengkap) → _Minta TTD_ → _Tandatangani_ | Form _Terkirim_; P-B6-A tampil di antrean | Regresi singkat |
| B6-02 | A | Petugas admisi (opsi 1) atau SuperAdmin (opsi 2) | _Antrean Transfer IGD/Poli_ → _Proses Admisi_. Catat pengisian awal unit, kelas, dan DPJP. Selesaikan **semua** langkah sampai persetujuan | `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` → `201`; P-B6-A hilang dari antrean | `BE-IGD-074` #6; C-12; `AT-IGD-229` |
| B6-03 | A | Perawat | Muat ulang tab _Tindak Lanjut_ | _Dijalankan_; lencana _Selesai_; keterangan _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."_; _Batalkan Form_ tidak ada; _Cetak_ ada; `GET …/emergency-admission-requests?emergencyVisitId=` memuat `displayStatus` `Completed` | `FE-IGD-061` #2, #5; `BE-IGD-074` #6 |
| B6-04 | A | **API** perawat, lalu dokter | `POST …/emergency-admission-requests/{id}/cancel` (versi terbaru, dengan alasan); lalu `PATCH …/emergency-dispositions/{id}/disposition-status` dengan `dispositionStatus: 4` **dan alasan** | `409` _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."_; `400` _"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."_ | `BE-IGD-074` #5, `AT-IGD-233` |
| B6-05 | A | Dokter | Tab _Tindak Lanjut_ dokter → cari aksi _Batalkan_ pada tindak lanjut yang sudah dijalankan | Tombol tidak ada, **atau** penolakan server tampil apa adanya. Tulis mana yang terjadi | `FE-IGD-061` #3 (bagian sesudah _admit_) |
| B6-06 | A | **Perawat IGD**, lalu **perawat ruangan** | Perawat IGD: tab _Transfer Pasien_ → _Buat Kepergian_ (unit _Rawat Inap_, SBAR lengkap) → _Ajukan Serah Terima_ → _Catat Berangkat_. Perawat ruangan, pada tab _Transfer Pasien_ pasien yang sama: _Terima Dokumen_ → _Catat Tiba_ | Kunjungan IGD _Selesai_; `GET …/emergency-visits/{id}` memuat `visitStatus` `Completed` dan waktu selesai. Log memuat `POST …/emergency-departures`, `submit-handover`, `depart`, `accept-handover`, `arrive` | `IGD-DEC-308` alur normal, `AT-IGD-237` |
| B6-07 | B | Petugas admisi; **API** | Muat ulang antrean; `GET …/inpatient-management/admission-transfers/{dispositionId P-B6-B}?sourceType=Emergency` | P-B6-B (tindak lanjut bertanda tanpa form) tidak tampil di antrean; `GET` → `404` | `BE-IGD-074` #3 |

### 3C.4 Yang dapat dinilai sesudah B6

| Task | Bila B6 lulus |
| --- | --- |
| `BE-IGD-074` | ✅ — #3, #5, #6 lengkap |
| `FE-IGD-061` | ✅ — #2, #3, #5 lengkap |
| `BE-IGD-072` | Tetap menunggu R1-01 |
| `BE-IGD-077` | Tetap 🟡, menunggu `IGD-OQ-132` |

**Hasil 18 Oktober 2026:** `BE-IGD-074` dan `FE-IGD-061` ✅. B6-06 gagal — kunjungan tidak tertutup sesudah _admit_ lalu
pasien tiba; cacat IGD, diperbaiki `BE-IGD-083` dan diuji ulang pada Putaran B7 (bagian 3D). Penilaian lengkap:
[laporan B6, bagian penilaian](2026-10-18-laporan-uji-putaran-b6.md).

---

## 3D. Putaran B7 — penutupan kunjungan sesudah _admit_ (`BE-IGD-083`)

Folder bukti: `test-with-agy/igd/uji-putaran-b7-<YYYYMMDD>/`.

**Kenapa ada B7.** Pada B6-06 kunjungan pasien yang di-_admit_ dari Antrean Transfer tidak pernah tertutup, walaupun pasien
sudah diantar dan dicatat tiba (`IGD-FACT-128`). Dua sebabnya ada di IGD: kunjungan tidak pernah dipindahkan ke `Disposed`
sesudah _admit_, dan _Catat Tiba_ tidak mencoba penutupan (`IGD-FACT-130`). Keduanya diperbaiki `BE-IGD-083`. Rujukan tetap:
[penilaian B6 dan keselarasan](../evidence/2026-10-18-penilaian-b6-dan-keselarasan-rawat-inap.md).

| Bagian | Boleh dijalankan sesudah | Skenario |
| --- | --- | --- |
| **1** | `BE-IGD-083` dikerjakan dan Rizki membuild backend. Delta matriks state sudah disetujui (`IGD-DEC-324`; state `0.15.0` §11.8, validation `0.19.0` §14.10) | B7-00…B7-05 |
| **2** | Rawat Inap (Muhammad Hamzah) memasang panggilan `EvaluasiPenutupanKunjunganSusulanAsync` sesudah _admit_ (`IGD-REQ-003` butir 1) dan backend dibuild ulang | B7-06, B7-07 |

Aturan bagian 1, dua fase S dan T (3B.1), dan opsi _admit_ (3C.1) berlaku penuh. Kesalahan B6 yang tidak boleh terulang:
~~baris persiapan ditulis **SUCCESS** padahal opsi 1 belum terpasang~~ (_dikoreksi sesudah B7: opsi 1 terpasang sesuai templat §3C.1 yang kurang `InpatientEpisode:Create`; cacat panduan, `IGD-FACT-134`_), dan skrip memanggil
`…/undefined/cancel` karena ID tidak terbaca.

### 3D.1 Persiapan (fase S)

| No | Persiapan | Cek selesai |
| --- | --- | --- |
| S7-01 | Backend dari build Rizki yang memuat `BE-IGD-083` | Jam DLL di `kode-awal.txt` lebih baru dari laporan `task/report/backend/BE-IGD-083.md` |
| S7-02 | Opsi _admit_ 3C.1 terpasang. Opsi 1: entri indeks 4 memuat `InpatientEpisode:Create` (_ditambahkan 18 Oktober 2026_), lalu akun admisi membuka langkah 6 dan melihat ≥ 1 bed. Bukti akhir opsi 1 adalah `admit` `201` oleh akun admisi; bila `403`, tulis opsi 2 | `persiapan.json` menyebut opsi yang benar-benar dipakai |
| S7-03 | Perawat ruangan membuka tab _Transfer Pasien_ tanpa `403` (seperti S6-02) | Tab terbuka |
| S7-04 | Bed siap pakai di unit _Rawat Inap_: bagian 1 butuh 1 bed, bagian 2 butuh 2 bed | Bed _Available_ |
| S7-05 | Pasien. **P-B7-A:** boleh P-B6-B (`IGD-261018003405-FA1086`, No. RM `00-00-02-63`) bila tindak lanjut rawat inapnya masih _Dikonfirmasi_ tanpa form; bila tidak, pasien baru dengan keadaan itu. **P-B7-B:** pasien baru dengan tindak lanjut _Pulang_ _Dikonfirmasi_. **P-B6-A** (`IGD-261018003352-28FF6F`) dipakai apa adanya pada B7-05. Bagian 2: **P-B7-C** dan **P-B7-D**, masing-masing pasien baru dengan tindak lanjut rawat inap _Dikonfirmasi_ | `data-uji.json` |
| S7-06 | Nilai master kelas **KELAS II** `isForInpatient` dibiarkan seperti sesudah B6, kecuali pemilik Rawat Inap sudah menetapkan lain (`IGD-REQ-003` butir 4) | Nilai yang dipakai dicatat di `persiapan.json` |

### 3D.2 Skenario bagian 1 (fase T)

**Aturan berhenti:** bila _admit_ pada B7-01 gagal, **berhenti** — jangan menjalankan B7-02 dan B7-03.

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B7-00 | — | Semua akun | Pemeriksaan pembuka seperti B6-00 | Tanpa lencana _N_; tanpa `403`. Bila gagal, berhenti | — |
| B7-01 | A | Perawat, dokter DPJP, petugas admisi (opsi 1) atau SuperAdmin (opsi 2); **API** baca | _Buat Form_ (isian lengkap) → _Minta TTD_ → _Tandatangani_ → _Antrean Transfer IGD/Poli_ → _Proses Admisi_ sampai persetujuan. Lalu `GET …/emergency-visits/{id}` | `POST …/admission-transfers/admit` `201`. Tanpa panggilan Rawat Inap, kunjungan masih `visitStatus` `6` — catat nilainya apa adanya | Persiapan B7-02, B7-03 |
| B7-02 | A | **API** perawat | `PATCH …/emergency-visits/{id}/complete` dengan catatan, **sebelum** kepergian dibuat; lalu `GET …/emergency-visits/{id}` | `409` _"Pasien belum dicatat tiba di unit rawat inap."_; `visitStatus` tetap `6` | `BE-IGD-083` #3 |
| B7-03 | A | **Perawat IGD**, lalu **perawat ruangan**; **API** baca | Perawat IGD: tab _Transfer Pasien_ → _Buat Kepergian_ (unit _Rawat Inap_, SBAR lengkap) → _Ajukan Serah Terima_ → _Catat Berangkat_. Perawat ruangan, pasien yang sama: _Terima Dokumen_ → `GET …/emergency-visits/{id}` dan PNG `B7-03-terima.png` (lencana kunjungan) → _Catat Tiba_ → `GET …/emergency-visits/{id}` dan PNG `B7-03-tiba.png` | Sesudah _Terima Dokumen_: `visitStatus` `7` (`Disposed`), **belum** `Completed`; lencana bukan "Menunggu keputusan". Sesudah _Catat Tiba_: `visitStatus` `9` (`Completed`) dan `visitCompletedAt` terisi; log memuat `POST …/emergency-departures`, `submit-handover`, `depart`, `accept-handover`, `arrive`, dan `EmergencyVisit.CompleteByDisposition` pada aksi _Arrive_ | `BE-IGD-083` #1, #2; `AT-IGD-237` |
| B7-04 | B | Perawat | Tab _Tindak Lanjut_ → _Jalankan_ tindak lanjut _Pulang_ | `200`; kunjungan `Completed` seperti sebelum `BE-IGD-083` | `BE-IGD-083` #6 (regresi) |
| B7-05 | P-B6-A | **API** perawat | `PATCH …/emergency-visits/{id}/complete` dengan catatan "Perapian sesudah BE-IGD-083" | `200`; `visitStatus` `9` | `BE-IGD-083` #7 |

### 3D.3 Skenario bagian 2 — sesudah panggilan Rawat Inap terpasang

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| B7-06 | C | Seperti B7-01, lalu seperti B7-03 | Form sampai _admit_ → `GET` kunjungan → kepergian sampai _Catat Tiba_ → `GET` kunjungan | Segera sesudah _admit_: `visitStatus` `7`, **belum** `Completed` — kunjungan tidak boleh tertutup sebelum pasien tiba. Sesudah _Catat Tiba_: `9` | `BE-IGD-083` #4 (alur normal); `BE-IGD-077` #4 |
| B7-07 | D | Perawat IGD, perawat ruangan, lalu petugas admisi | Form sampai _Tandatangani_. Perawat IGD: _Buat Kepergian_ → _Ajukan Serah Terima_ → _Catat Berangkat_. Perawat ruangan: _Terima Dokumen_ → _Catat Tiba_ → `GET` kunjungan (tetap terbuka; tindak lanjut masih _Dikonfirmasi_). Lalu _Proses Admisi_ sampai `201` → `GET` kunjungan | Sesudah _admit_: `visitStatus` `9` dalam transaksi _admit_ yang sama | `AT-IGD-240`; `BE-IGD-077` #4; `FR-IGD-120` |

### 3D.4 Yang dapat dinilai sesudah B7

| Task | Bila lulus |
| --- | --- |
| `BE-IGD-083` | ✅ bila bagian 1 lulus dan acceptance 4, 5, 6, 8, 9 terbukti dari diff dan laporan task |
| `BE-IGD-077` | Acceptance 4 terbukti bila bagian 2 lulus. Acceptance 3 tetap menunggu tombol _Tolak_ Rawat Inap (`IGD-REQ-003` butir 2) |
| `FR-IGD-113` | `AT-IGD-237` terbukti bila B7-03 lulus |
| `FR-IGD-120` | `AT-IGD-240` terbukti bila B7-07 lulus |

Penilaian memakai prompt A.7 dengan B5 diganti B7.

**Hasil bagian 1 — 18 Oktober 2026:** B7-00…05 lulus; `BE-IGD-083` ✅; `AT-IGD-237` terbukti. Penilaian lengkap:
[laporan B7, bagian penilaian](2026-10-18-laporan-uji-putaran-b7.md). Bagian 2 (B7-06, B7-07) menunggu panggilan Rawat Inap.

---

## 3E. Putaran K1 — tagihan IGD sampai kasir (demo 21 Oktober)

Folder bukti: `test-with-agy/igd/uji-putaran-k1-<YYYYMMDD>/`. _Ditambahkan 18 Oktober 2026 malam._

**Kenapa ada K1.** Jembatan Billing kini menerima kunjungan IGD, tetapi belum ada satu pun bukti bahwa tagihan pasien
IGD sampai ke kasir (`IGD-FACT-140`); jasa kunjungan IGD baru direncanakan (`BE-IGD-084`). Rujukan tetap:
[audit pasien prioritas dan billing IGD](../evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md).

| Bagian | Boleh dijalankan sesudah | Skenario |
| --- | --- | --- |
| **1** | Build backend Rizki apa pun sesudah 18 Oktober 2026. Tanpa `BE-IGD-084`, baris K1-03 dan K1-10…11 ditulis `NOT RUN — BE-IGD-084 belum dibangun` | K1-00…K1-09, K1-12, K1-13 |
| **2** | `BE-IGD-084` dibangun Rizki, konfigurasi `EmergencyBilling:VisitFeeProcedureCode` terisi. **Tersedia sejak build Rizki 18 Oktober 2026 16.04.59** (konfigurasi `PR-RSMMC-00025` di user-secrets laptop Rizki — backend harus dijalankan dari laptop itu, lingkungan `Development`) | K1-03, K1-10, K1-11 (boleh satu putaran dengan bagian 1) |
| **3** | Jalur transfer seperti Putaran B7 (opsi _admit_ 3C.1) | K1-14 |

Aturan bagian 1, dua fase S dan T (3B.1), dan opsi _admit_ (3C.1) berlaku penuh.

**Pasien.** Empat pasien baru, semuanya didaftarkan lewat **loket IGD**: **K1-A** tunai, **K1-B** asuransi, **K1-C** PKS,
**K1-D** tunai (untuk transfer). Skenario yang berlaku untuk lebih dari satu pasien dicatat **per pasien** dengan akhiran
huruf pasien: `K1-01-A`, `K1-01-B`, dan seterusnya — satu baris per ID (A9).

### 3E.1 Persiapan (fase S)

| No | Persiapan | Cek selesai |
| --- | --- | --- |
| SK-01 | Akun peran dan variabel lingkungannya. **Sudah ada:** Petugas Pendaftaran IGD `QUILVIAN_LOKET_EMAIL`/`_PASSWORD`, Perawat IGD `QUILVIAN_PERAWAT_*`, Dokter IGD (DPJP) `QUILVIAN_DOKTER_*`, dokter IGD kedua `QUILVIAN_DOKTER2_*` (K1-10), petugas admisi Rawat Inap `QUILVIAN_ADMISI_*` dan perawat ruangan `QUILVIAN_PERAWAT_RUANGAN_*` (K1-14). **Disiapkan Rizki sebelum K1 — 18 Oktober 2026 belum ada:** petugas Laboratorium penerima spesimen `QUILVIAN_LAB_EMAIL`/`_PASSWORD`, Apoteker `QUILVIAN_APOTEKER_*`, Kasir `QUILVIAN_KASIR_*` dengan shift terbuka. Akun yang variabelnya tidak ada → skenarionya `NOT RUN — akun belum disiapkan` | Semua akun masuk tanpa `403` pada layarnya |
| SK-02 | **Item _Jasa kunjungan IGD_** di _Master Tindakan_ (aktif, tersedia untuk IGD) dan **tarifnya** di _Master Tarif_: klinik kosong, satu baris per kelas yang dipakai pasien K1 atau satu baris tanpa kelas, dasar hitung `PerService`, berlaku paling lambat hari uji (`IGD-UNK-19`). Rizki mengisi kodenya ke `EmergencyBilling:VisitFeeProcedureCode` lalu menyalakan ulang backend | Kode item dan nama kelas yang bertarif dicatat di `persiapan.json`. Bila belum ada: tulis `NOT AVAILABLE` dan jalankan bagian 1 saja. **Dev, 18 Oktober 2026 malam:** sudah dibuat — item `PR-RSMMC-00025` _Jasa Kunjungan IGD_ dan tarif `TF-RSMMC-00016` Rp150.000 (tanpa klinik dan kelas, berlaku sejak 17 Oktober 2026); konfigurasi terisi di user-secrets laptop Rizki. Penguji cukup memeriksa keduanya tampil di layar _Master Tindakan_ dan _Master Tarif_ |
| SK-03 | Tarif untuk tindakan perawat yang dipakai (misalnya pemasangan infus), satu pemeriksaan laboratorium (misalnya darah rutin), dan satu obat (misalnya ondansetron injeksi) untuk kelas pasien K1 | Nama item dan tarif dicatat |
| SK-04 | Satu penjamin asuransi dan satu perusahaan PKS aktif beserta aturan coverage-nya | Nama penjamin dicatat |
| SK-05 | Kebijakan biaya administrasi untuk jenis layanan `IGD`, dan kebijakan kirim ulang `INVOICE_SYNC` aktif — dibaca lewat `GET …/billing-management/master-data/billing-sync-policies` (`IGD-UNK-23`) | Nilai dicatat apa adanya; kosong bukan alasan berhenti |
| SK-06 | Shift kasir terbuka | PNG layar kasir |

SuperAdmin boleh dipakai **hanya** untuk persiapan lewat aplikasi (`IGD-DEC-320`, `322`), lalu logout dan dicatat.

### 3E.2 Skenario (fase T)

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| K1-00 | — | Semua akun | Pemeriksaan pembuka seperti B6-00 | Tanpa lencana _N_; tanpa `403`. Bila gagal, berhenti | — |
| K1-01 | A, B, C, D | Petugas Pendaftaran | Loket IGD: daftarkan pasien dengan penjamin sesuai hurufnya | Encounter `Emergency` lahir; penjamin tersimpan. Catat `encounterId` | Persiapan |
| K1-02 | A, B, C, D | Perawat IGD | _Mulai Triage_ → simpan triage | Kunjungan IGD tertaut encounter | Persiapan |
| K1-03 | A, B, C | Perawat IGD; **API** baca | Tetapkan DPJP pertama (dokter IGD) di layar triage, lalu `GET /api/v1/health-services/clinical-management/patient-procedures?encounterId={encounterId}` | Tepat satu baris _Jasa Kunjungan IGD_: `procedureSource` `4` (atau `EmergencyVisitFee`), kode `PR-RSMMC-00025`, dokter = DPJP, `procedureStatus` _Completed_. Catat `id` tindakan untuk K1-11. Log backend memuat `EmergencyVisitFee.Recorded`. **Bagian 2** | `BE-IGD-084` #1 |
| K1-04 | A, B, C | Perawat atau dokter; petugas Lab | Pesan darah rutin dari layar IGD → petugas Lab menerima spesimen | Spesimen _diterima_ | Persiapan K1-08 |
| K1-05 | A, B, C | Perawat IGD | Tab _Tindakan_ → _Catat tindakan keperawatan_ (pemasangan infus) | `200`; tindakan `Completed` | Persiapan K1-08 |
| K1-06 | A, B, C | Dokter DPJP | Tulis resep biasa (ondansetron) → finalkan bersama catatan dokter | Resep final | Persiapan K1-08, K1-12 |
| K1-07 | A | Dokter DPJP | Tab _Tindakan_ dokter → simpan satu tindakan (misalnya jahit luka) | Tindakan tersimpan. **Diharapkan tidak tertagih** (`IGD-FACT-140`); tulis hasilnya apa adanya di K1-08 — bukan `FAIL` K1-07 | Konfirmasi `IGD-FACT-140` |
| K1-08 | A, B, C | **API** kasir | `GET …/billing/invoices/encounters/{encounterId}/charge-summary` | Satu invoice IGD memuat: jasa kunjungan (bagian 2), pemasangan infus, darah rutin, ondansetron, biaya administrasi IGD. Untuk B dan C, bagian penjamin dan bagian pasien terpisah. Catat setiap baris beserta nominal dan sumbernya | `BE-IGD-084` #2; `IGD-UNK-21` |
| K1-09 | A, B, C | **Kasir** | Layar _Invoice & Billing Kasir_ (`/health-services/billing-management/billing/invoices/cashier-overview`) → saring label _IGD_ → buka invoice pasien → PNG. A: bayar tunai. B dan C: buka dokumen invoice penjamin dan dokumen pasien | Invoice IGD tampil dengan baris yang sama dengan K1-08; pembayaran A berhasil; dokumen B dan C terbuka | `BE-IGD-084` #2; demo 21 Oktober |
| K1-10 | A | Perawat IGD; **API** | Alihkan DPJP ke dokter IGD kedua, lalu ulangi `GET` tindakan dan `charge-summary` | Tetap **satu** baris jasa kunjungan. **Bagian 2** | `BE-IGD-084` #3 |
| K1-11 | A | **API** perawat | (1) `PATCH /api/v1/health-services/clinical-management/patient-procedures/{id}/cancel` body `{"cancelReason":"Uji K1-11"}`; (2) `PATCH …/patient-procedures/{id}/remove-draft` tanpa body — `{id}` dari K1-03; lalu ulangi `GET` K1-03 | Keduanya `400` _"Jasa kunjungan IGD tidak dapat dibatalkan dari modul klinis."_; baris tetap _Completed_. Bila `403`, tulis `NOT RUN — 403` (hak akses, bukan cacat) | `BE-IGD-084` #6 |
| K1-12 | A, B | Apoteker | Sebelum A membayar: buka resep ondansetron A untuk ditelaah. Sesudah K1-09: telaah, siapkan, serahkan. Untuk B: coba telaah tanpa pembayaran pasien | Catat pesan **persis**. Yang diharapkan hari ini: ditolak sebelum lunas, lolos sesudah lunas (gerbang Farmasi, `IGD-DEC-249` belum dikerjakan) | `IGD-UNK-22`; bahan keputusan obat IGD |
| K1-13 | A, B, C | **API** kasir | `GET …/billing/charge-reconciliations` disaring pasien atau encounter K1 | Catat setiap baris berikut sebabnya (misalnya `TARIFF_NOT_FOUND`). Kosong juga hasil yang sah | `BE-IGD-084` #5; `IGD-UNK-21` |
| K1-14 | D | Seperti B7-01 dan B7-03, lalu **Kasir** | Pasien D diberi satu tindakan perawat, lalu dibawa sampai _admit_ dan _Catat Tiba_. Kasir membuka invoice pasien D | **Dua invoice**: invoice IGD (label _IGD_) dan invoice rawat inap, tidak saling tertaut (`IGD-DEC-293`, `IGD-FACT-142`). Catat nomor, label, dan isi keduanya | Perilaku transfer untuk demo |

### 3E.3 Yang dapat dinilai sesudah K1

| Butir | Bila lulus |
| --- | --- |
| `BE-IGD-084` | Acceptance 1, 2, 3, 6 terbukti dari bagian 2; 4, 5, 7, 8, 9 dari diff, log, dan laporan build |
| `IGD-UNK-21` | Terjawab runtime per jenis layanan (tindakan perawat, laboratorium, obat, administrasi) |
| `IGD-FACT-140` | Tindakan dokter IGD terkonfirmasi tidak tertagih (K1-07, K1-08) atau dikoreksi |
| Demo 21 Oktober | Jenis layanan yang lulus K1-08 dan K1-09 boleh diperagakan; sisanya tidak dijanjikan |

---

## 3F. Putaran P1 — pasien prioritas dari reservasi sampai Form Permintaan Rawat Inap (demo 20 Oktober)

Folder bukti: `test-with-agy/igd/uji-putaran-p1-<YYYYMMDD>/`. _Ditambahkan 18 Oktober 2026 malam._ Boleh dijalankan
sesudah `BE-IGD-085` dibangun Rizki. Tanpa itu, P1-03 dan P1-04 akan menunjukkan cacat lama (`IGD-FACT-137`) — jangan
dijalankan.

**Kenapa ada P1.** Reservasi pra-kedatangan belum pernah diuji lewat layar, dan _check-in_ lama melahirkan kunjungan
tanpa encounter. `BE-IGD-085` mengubah _check-in_ supaya memakai encounter dari loket (`IGD-DEC-325`).

**Pasien.** **P1-A** pasien member aktif dengan tier prioritas (misalnya Platinum, `PriorityQueue` aktif), belum punya
kunjungan IGD; **P1-B** pasien reguler untuk reservasi yang dibatalkan; **P1-C** pasien reguler untuk P1-07. Semua pasien
baru; jangan memakai pasien yang pernah di-_check-in_ sebelum 19 Oktober 2026 (kunjungannya tanpa encounter).

### 3F.1 Persiapan (fase S)

| No | Persiapan | Cek selesai |
| --- | --- | --- |
| SP-01 | Backend dari build Rizki yang memuat `BE-IGD-085` — DLL 18 Oktober 2026 16.04.59 atau lebih baru | Jam DLL dicatat di `kode-awal.txt` |
| SP-02 | Akun Petugas Pendaftaran IGD (`QUILVIAN_LOKET_*`) dipakai juga sebagai petugas admisi IGD: rolenya diberi `EmergencyPreArrivalReservation : Read/Create/Update` lewat Akses Role (boleh SuperAdmin, `IGD-DEC-322`). Perawat IGD `QUILVIAN_PERAWAT_*`, Dokter IGD `QUILVIAN_DOKTER_*` | Layar _Reservasi Pra-Kedatangan_ (`/health-services/emergency-installation-management/pre-arrival-reservations`) terbuka tanpa `403` |
| SP-03 | P1-A punya keanggotaan CRM aktif dengan tier prioritas (boleh disiapkan SuperAdmin lewat layar CRM, `IGD-DEC-322`) | No. RM, tier, dan masa berlaku dicatat |
| SP-04 | Satu moda kedatangan ambulans aktif di master | Nama moda dicatat |

### 3F.2 Skenario (fase T)

| ID | Pasien | Aktor | Langkah | Hasil yang diharapkan | Menjawab |
| --- | --- | --- | --- | --- | --- |
| P1-00 | — | Semua akun | Pemeriksaan pembuka seperti B6-00 | Tanpa lencana _N_; tanpa `403` | — |
| P1-01 | A | Petugas admisi IGD | _Buat Reservasi_: pemesan, telepon, ETA, keluhan, pasien P1-A, ambulans, petugas pendamping → simpan | `201`; nomor `RES-EMG-…`; lencana tier tampil di ringkasan | `FE-IGD-067` #1, #2; `BE-IGD-080` #4 |
| P1-02 | A, B | Petugas admisi IGD | Buat reservasi kedua untuk P1-B, lalu buka daftar dan rincian | Daftar urut ETA terdekat; rincian sama dengan isian | `FE-IGD-067` #3; `BE-IGD-080` #5 |
| P1-03 | A | Petugas admisi IGD | _Konfirmasi Tiba_ untuk P1-A **sebelum** loket | `409` _"Pasien belum didaftarkan di loket IGD. Daftarkan di loket lebih dulu, lalu ulangi Konfirmasi Tiba."_ tampil sebagai toast; reservasi tetap _Menunggu_ (`1`) | `BE-IGD-085` #2; `FE-IGD-067` #4 |
| P1-04 | A | Petugas Pendaftaran, lalu petugas admisi IGD; **API** baca | Loket IGD: daftarkan P1-A sebagai pasien asuransi. Lalu _Konfirmasi Tiba_ reservasi P1-A. `GET …/emergency-visits/{id}` dan `GET` reservasi | `200`; kunjungan `encounterId` = encounter loket, `visitStatus` `2` (`WaitingForTriage`); reservasi `2` dengan `resultEmergencyVisitId` terisi; pasien muncul di antrean triage | `BE-IGD-085` #1; `BE-IGD-080` #7; `FE-IGD-067` #4, #7 |
| P1-05 | A | Perawat IGD, Dokter DPJP | Triage → tetapkan DPJP → dokter menulis diagnosis → tindak lanjut _Rawat Inap_ dikonfirmasi → perawat menekan _Buat Form_ | Tindak lanjut dapat dikonfirmasi; _Buat Form_ **tidak** ditolak _"Kunjungan ini belum punya pasien dan encounter…"_ | `BE-IGD-085` #4 |
| P1-06 | B | Petugas admisi IGD | _Batalkan Reservasi_ P1-B dengan alasan | Reservasi `3`; alasan tersimpan | `BE-IGD-080` #6; `FE-IGD-067` #5 |
| P1-07 | A, C | Petugas Pendaftaran, Perawat IGD | Daftarkan P1-C di loket dan triage dengan kategori lebih gawat daripada P1-A (misalnya P1-C Merah, P1-A Kuning). Buka antrean penanganan IGD | Urutan mengikuti kategori triage, bukan keanggotaan; P1-C di atas P1-A | `AT-IGD-243` (`IGD-DEC-315`) |
| P1-08 | A | Seperti B7-01 | **Hanya bila** Rawat Inap sudah memasang butir 5 `IGD-REQ-003`: lanjutkan P1-A sampai _Tandatangani_, lalu buka Antrean Transfer bersama satu pasien CITO reguler dan satu pasien reguler | Urutan: CITO → member prioritas (lencana tier) → reguler. Bila butir 5 belum ada: `NOT AVAILABLE` | `AT-IGD-244` (`IGD-DEC-327`) |
| P1-06R | Reservasi baru tanpa pasien | Petugas admisi IGD (`QUILVIAN_LOKET_*`) | _Ditambahkan 18 Oktober 2026 malam, sesudah perbaikan `FE-IGD-067`._ Pada build `JthyTCQWcFzQhAtAEA3MD` atau lebih baru: buat reservasi baru, lalu dari layar tekan _Batalkan Reservasi_, isi alasan, tekan _Ya, Batalkan Reservasi_ dengan klik biasa. **Dilarang** menyuntik alasan ke objek event (jalan pintas P1-06) | `POST …/cancel` `200`; reservasi `3` dengan alasan tersimpan; toast sukses. Bila login loket `401`, tulis `NOT RUN — kredensial` | `FE-IGD-067` #5, #7 |

### 3F.3 Yang dapat dinilai sesudah P1

| Task | Bila lulus |
| --- | --- |
| `BE-IGD-085` | Acceptance 1, 2, 4 terbukti; 3, 5, 6, 7, 8 dari diff dan laporan build |
| `BE-IGD-080` | ✅ bila P1-01, P1-02, P1-04, P1-06 lulus (acceptance 4–7) |
| `FE-IGD-067` | ✅ bila P1-01…P1-04 dan P1-06 lulus pada build produksi (acceptance 4, 5, 7) |
| `FR-IGD-123` | `AT-IGD-243` terbukti bila P1-07 lulus |
| `FR-IGD-124` | `AT-IGD-244` terbukti bila P1-08 lulus |

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
| R1-01 | `BE-IGD-072` acc 8 — **lulus 18 Oktober 2026** (penjaga menolak bila ada form; rollback dan naik kembali identik). Catatan: kontainer perlu `-c max_locks_per_transaction=4096`, karena EF Core 9 menerapkan seluruh rantai dalam satu transaksi | `Down` dari head ke `20261012041938_AddAdmissionTransferOrigin` (melewati `20261012080102_AddEmergencyAdmissionRequest` dan migration sesudahnya), lalu `Up` ke head | Penjaga `Down()` bekerja; tabel `EmgAdmissionRequest` dan `EmgAdmissionRequestEvent` hilang lalu kembali identik; skema tabel lain tidak berubah |
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

FASE S - PERSIAPAN (keputusan pemilik IGD-DEC-320 dan IGD-DEC-322)
- SuperAdmin BOLEH dipakai untuk perubahan apa pun yang dibutuhkan persiapan, LEWAT APLIKASI (layar atau API), antara lain:
  1. akun uji dan data HR-nya: dokter IGD kedua (boleh pakai ulang dokter.igd.b4), petugas admisi (boleh pakai ulang
     admisi.b4), perawat ruangan, petugas loket bila perlu;
  2. penugasan perawat ruangan ke unit bangsal tujuan;
  3. Akses Role sesuai tabel 3B.2 (petugas admisi: InpatientAdmissionTransfer Read, InpatientEpisode Create,
     InpatientBedOccupancy Read/Create/Update, Doctor Read, plus hak pembayaran/deposit yang diminta wizard;
     dokter kedua: EmergencyAdmissionRequest Read/Sign, EmergencyVisit Read);
  4. bypass geolokasi (enable) pada SETIAP akun yang dipakai di B5, termasuk dimas.kurniawan dan ranger.biru;
  5. kesiapan bed dan data master: jadikan bed di bangsal tujuan (mis. Bed Melati 01/02) siap pakai (Available,
     isBookable true) lewat alur kesiapan bed di aplikasi, atau buat bed uji bila perlu;
  6. data pasien uji bila alur IGD terhambat.
- Yang tetap DILARANG: SQL langsung ke basis data.
- Catat SETIAP perubahan fase S di persiapan.json (jam, akun, layar/endpoint, sebelum-sesudah), termasuk daftar akun yang
  bypass geolokasinya dinyalakan dan bed yang diubah, + PNG Akses Role sebelum/sesudah.
- Bila sebuah langkah gagal karena galat server (mis. 500), catat galatnya dan coba jalan lain lewat aplikasi; bila tetap
  tidak bisa, berhenti dan laporkan.
- Pasien P-B5-A sampai P-B5-E sebaiknya dibuat lewat alur IGD (loket -> triage -> DPJP -> catatan dokter + diagnosis
  ICD-10 -> tindak lanjut). Bila memakai SuperAdmin atau rute admin, tulis pasien mana di persiapan.json.
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

### A.1c Putaran B6 (satu langkah _admit_)

```text
Kamu adalah penguji modul IGD Quilvian. Jalankan HANYA Putaran B6 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
Baca seluruh bagian 1, 3B.1, 3C, dan 8 lebih dulu, lalu bagian "Penilaian agent pengembang" di
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-laporan-uji-putaran-b5.md.
Tiga kesalahan B5 yang tidak boleh terulang: (1) menjalankan skenario lanjutan padahal admit gagal, sehingga data uji rusak;
(2) menulis bahwa serah terima sudah dicatat padahal tidak ada satu pun penulisan kepergian di log; (3) memakai SuperAdmin
di fase T tanpa dicatat.

Opsi admit yang dipakai: <isi oleh Rizki: OPSI 1 (penugasan UnitAssignments untuk admisi.b4 sudah dipasang) atau OPSI 2
(B6-02 dijalankan SuperAdmin)>.

FASE S (IGD-DEC-320, IGD-DEC-322): SuperAdmin boleh untuk persiapan lewat aplikasi. Pastikan S6-01 sampai S6-04:
- opsi admit terpasang (opsi 1: akun admisi melihat minimal 1 bed di langkah 6, tanpa menyimpan);
- perawat.ruangan.uji.b5 membuka tab Transfer Pasien pasien IGD tanpa 403;
- minimal 1 bed Available di unit Rawat Inap;
- dua pasien IGD BARU: P-B6-A (tindak lanjut rawat inap Dikonfirmasi) dan P-B6-B (sama, tanpa form). Jangan pakai P-B5-A.
Catat semua di persiapan.json. Sesudah itu logout SuperAdmin.

FASE T, B6-00 sampai B6-07, akun peran saja (kecuali B6-02 bila OPSI 2, dan itu dicatat):
- B6-00: build produksi (npm run start, .next/standalone/server.js), PNG login tanpa lencana N, semua akun tanpa 403.
- B6-02 harus sampai POST admission-transfers/admit 201. BILA GAGAL: BERHENTI. Jangan jalankan B6-03 sampai B6-06.
- B6-04: kirim versi form terbaru dan alasan; disposition-status memakai dispositionStatus 4 DENGAN alasan.
- B6-06: perawat IGD yang membuat kepergian, mengajukan serah terima, dan mencatat berangkat; perawat ruangan yang menekan
  Terima Dokumen dan Catat Tiba di tab Transfer Pasien pasien yang sama. Hasil hanya boleh PASS bila log memuat
  POST emergency-departures, submit-handover, depart, accept-handover, dan arrive, dan kunjungan IGD berstatus Completed.
- Satu baris per ID; pesan dicatat persis; setiap PNG diambil sesudah aksi skenarionya sendiri; sandi hanya dari variabel
  lingkungan; rekam jaringan semua /api/v1/; catat SETIAP percobaan, login gagal, dan setiap pemakaian SuperAdmin.
- Jangan mengubah kode atau dokumen yang sudah ada. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b6-<YYYYMMDD>/ (persiapan.json, kode-awal.txt,
kode-akhir.txt, data-uji.json, PNG, jaringan-<ID>.json, skrip/, percobaan.json, hasil.json, laporan.md). Salin laporan.md
sebagai berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-b6.md.
Di akhir sebutkan path log backend hari itu.
```

### A.1d Putaran B7 (penutupan kunjungan sesudah _admit_)

Jalankan hanya sesudah `BE-IGD-083` selesai dikerjakan dan Rizki membuild backend (delta sudah disetujui `IGD-DEC-324`). Bagian 2
menunggu panggilan Rawat Inap (`IGD-REQ-003` butir 1).

```text
Kamu adalah penguji modul IGD Quilvian. Jalankan HANYA Putaran B7 bagian <isi oleh Rizki: 1, atau 1 dan 2> dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
Baca seluruh bagian 1, 3B.1, 3C.1, 3D, dan 8 lebih dulu, lalu bagian "Penilaian agent pengembang" di
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-18-laporan-uji-putaran-b6.md.
Dua hal dari B6: (1) opsi 1 butuh operasi InpatientEpisode:Create di entri indeks 4 (panduan 3C.1, dikoreksi 18 Oktober 2026); (2) skrip yang
memanggil URL berisi "undefined" karena ID tidak terbaca.

Opsi admit yang dipakai: <isi oleh Rizki: OPSI 1 atau OPSI 2>.

FASE S (IGD-DEC-320, IGD-DEC-322): SuperAdmin boleh untuk persiapan lewat aplikasi. Pastikan S7-01 sampai S7-06, catat di
persiapan.json, lalu logout SuperAdmin. Opsi 1 dibuktikan dengan akun admisi yang melihat minimal 1 bed di langkah 6.

FASE T, akun peran saja (kecuali langkah admit bila OPSI 2, dan itu dicatat):
- B7-00: build produksi (npm run start, .next/standalone/server.js), PNG login tanpa lencana N, semua akun tanpa 403.
- B7-01 harus sampai POST admission-transfers/admit 201. BILA GAGAL: BERHENTI. Catat visitStatus apa adanya.
- B7-02 dijalankan SEBELUM kepergian dibuat.
- B7-03: perawat IGD membuat kepergian, mengajukan serah terima, dan mencatat berangkat; perawat ruangan menekan Terima
  Dokumen, lalu GET kunjungan dan PNG, lalu Catat Tiba, lalu GET kunjungan dan PNG. Hasil hanya PASS bila visitStatus 7
  sesudah Terima Dokumen dan 9 sesudah Catat Tiba, dan log memuat kelima penulisan kepergian.
- B7-05 hanya untuk P-B6-A, lewat API PATCH emergency-visits/{id}/complete.
- Bagian 2 (bila diminta): B7-06 dan B7-07 persis seperti tabel 3D.3.
- Satu baris per ID; pesan dicatat persis; setiap PNG diambil sesudah aksi skenarionya sendiri; sandi hanya dari variabel
  lingkungan; rekam jaringan semua /api/v1/; catat SETIAP percobaan, login gagal, dan setiap pemakaian SuperAdmin.
- Jangan mengubah kode atau dokumen yang sudah ada. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b7-<YYYYMMDD>/ (persiapan.json, kode-awal.txt,
kode-akhir.txt, data-uji.json, PNG, jaringan-<ID>.json, skrip/, percobaan.json, hasil.json, laporan.md). Salin laporan.md
sebagai berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-b7.md.
Di akhir sebutkan path log backend hari itu.
```

### A.1e Putaran K1 (tagihan IGD sampai kasir)

Bagian 1 boleh dijalankan sekarang. Bagian 2 sesudah `BE-IGD-084` dibangun Rizki dan konfigurasinya terisi. Bagian 3
memakai opsi _admit_ 3C.1.

```text
Kamu adalah penguji modul IGD Quilvian. Jalankan HANYA Putaran K1 bagian <isi oleh Rizki: 1; 1 dan 2; atau 1, 2, dan 3> dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
Baca seluruh bagian 1, 3B.1, 3C.1, 3E, dan 8 lebih dulu, lalu bagian 4 dan 5 berkas
NewQuilvianSystemBackend/docs/module-blueprints/igd/evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md.

FASE S (IGD-DEC-320, IGD-DEC-322): SuperAdmin boleh untuk persiapan lewat aplikasi. Pastikan SK-01 sampai SK-06, catat
di persiapan.json (termasuk kode item Jasa kunjungan IGD dan kelas yang bertarif), lalu logout SuperAdmin. Bila item atau
tarif Jasa kunjungan IGD belum ada, tulis NOT AVAILABLE pada SK-02 dan jangan menjalankan bagian 2.

FASE T, akun peran saja:
- Empat pasien baru lewat loket IGD: K1-A tunai, K1-B asuransi, K1-C PKS, K1-D tunai. Catat encounterId tiap pasien.
- Akun hanya dari variabel lingkungan QUILVIAN_LOKET_*, QUILVIAN_PERAWAT_*, QUILVIAN_DOKTER_*, QUILVIAN_DOKTER2_*,
  QUILVIAN_LAB_*, QUILVIAN_APOTEKER_*, QUILVIAN_KASIR_*, QUILVIAN_ADMISI_*, QUILVIAN_PERAWAT_RUANGAN_*. Variabel yang tidak ada
  -> skenarionya NOT RUN, jangan diganti SuperAdmin.
- Item jasa kunjungan dev sudah ada: PR-RSMMC-00025, tarif TF-RSMMC-00016 Rp150.000. SK-02 cukup diperiksa di layar.
- Skenario per pasien dicatat dengan akhiran huruf pasien (K1-01-A, K1-01-B, ...). Satu baris per ID.
- K1-07 adalah pengamatan: tindakan dokter IGD diharapkan TIDAK tertagih; catat apa adanya di K1-08.
- K1-08 dan K1-13 lewat API; catat setiap baris tagihan beserta nominal, sumber, dan bagian penjamin/pasien.
- K1-09 lewat layar kasir dengan PNG invoice tiap pasien. K1-12: catat pesan Farmasi persis, sebelum dan sesudah bayar.
- K1-14 (bagian 3): pasien D sampai admit dan Catat Tiba seperti B7-01 dan B7-03; lalu kasir membuka invoice pasien D.
- Pesan dicatat persis; sandi hanya dari variabel lingkungan; rekam jaringan semua /api/v1/; catat SETIAP percobaan dan
  setiap pemakaian SuperAdmin. Jangan mengubah kode atau dokumen yang sudah ada. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-k1-<YYYYMMDD>/ (persiapan.json, kode-awal.txt,
kode-akhir.txt, data-uji.json, PNG, jaringan-<ID>.json, skrip/, percobaan.json, hasil.json, laporan.md). Salin laporan.md
sebagai berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-k1.md.
Di akhir sebutkan path log backend hari itu.
```

### A.1f Putaran P1 (pasien prioritas dari reservasi)

Jalankan hanya sesudah `BE-IGD-085` dibangun Rizki.

```text
Kamu adalah penguji modul IGD Quilvian. Jalankan HANYA Putaran P1 dari panduan:
NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md
Baca seluruh bagian 1, 3B.1, 3F, dan 8 lebih dulu, lalu bagian 2 berkas
NewQuilvianSystemBackend/docs/module-blueprints/igd/evidence/2026-10-18-audit-pasien-prioritas-dan-billing-igd.md.

FASE S (IGD-DEC-320, IGD-DEC-322): SuperAdmin boleh untuk persiapan lewat aplikasi, termasuk membuat keanggotaan CRM aktif
untuk P1-A. Pastikan SP-01 sampai SP-04, catat di persiapan.json, lalu logout SuperAdmin.

FASE T, akun peran saja:
- Pasien baru saja. Jangan memakai pasien yang pernah di-check-in sebelum 19 Oktober 2026.
- Akun hanya dari variabel lingkungan: QUILVIAN_LOKET_* (loket sekaligus admisi IGD, hak reservasi disiapkan di fase S),
  QUILVIAN_PERAWAT_*, QUILVIAN_DOKTER_*. Layar reservasi: /health-services/emergency-installation-management/pre-arrival-reservations.
- P1-03 dijalankan SEBELUM P1-A didaftarkan di loket; P1-04 sesudahnya. Catat pesan toast persis dan GET kunjungan.
- P1-05 berhenti di Buat Form (tidak perlu sampai admit), kecuali Rizki meminta gladi transfer.
- P1-08 hanya bila Rawat Inap sudah memasang IGD-REQ-004 (butir 5 IGD-REQ-003); bila belum, tulis NOT AVAILABLE.
- Satu baris per ID; pesan dicatat persis; setiap PNG diambil sesudah aksi skenarionya; sandi hanya dari variabel
  lingkungan; rekam jaringan semua /api/v1/; catat SETIAP percobaan dan setiap pemakaian SuperAdmin.
- Jangan mengubah kode atau dokumen yang sudah ada. Jangan menyatakan status task.

Simpan bukti ke QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-p1-<YYYYMMDD>/ (berkas sama seperti K1). Salin
laporan.md sebagai berkas BARU ke NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/<YYYY-MM-DD>-laporan-uji-putaran-p1.md.
Di akhir sebutkan path log backend hari itu.
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
2026-10-17-panduan-uji-sisa-modul-igd.md bagian 3B. Konteks: IGD-DEC-319 sampai IGD-DEC-322 dan IGD-FACT-123 sampai
IGD-FACT-125 di 00-interview-decisions.md.
Putuskan per skenario dari bukti mentah dan log, bukan dari laporan penguji. Wajib periksa: lencana N di setiap PNG (pojok
kiri bawah), PNG identik antar-skenario, sandi tertulis di skrip, kode-awal lawan kode-akhir, persiapan.json lawan log
(setiap penulisan SuperAdmin di log harus tercatat di persiapan.json sesuai IGD-DEC-320 dan IGD-DEC-322; tidak ada SQL),
dan tidak ada SuperAdmin di fase T.
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
