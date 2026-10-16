# Panduan Uji Putaran B (baru) — Pasien IGD ke Antrean Transfer IGD/Poli (`BE-IGD-074` + `FE-IGD-060`, `FE-IGD-061`)

| Field | Nilai |
| --- | --- |
| Menggantikan | [Panduan putaran B lama](2026-10-14-panduan-uji-putaran-b-transfer-igd-rawat-inap.md) — **ditarik**, jalurnya dibongkar (`IGD-DEC-304`) |
| Task | `BE-IGD-074`, `FE-IGD-060`, `FE-IGD-061` — diuji bersama dari layar (pasangan BE/FE). Antrean dan *Proses Admisi* milik Rawat Inap (`FE-RWI-228`/`229`) dipakai apa adanya |
| Acceptance yang dibuktikan | `AT-IGD-225`…`237` (PRD §12.4 dan amandemen §12.7.2); acceptance 1–8 `BE-IGD-074`, 1–5 `FE-IGD-060`, 1–6 `FE-IGD-061` |
| Kontrak | API `0.18.0` §12.2, §12.7; validation `0.17.0` §14 aturan 4, 13–17 dan §14.8; state `0.12.0` §11.5; integration `0.9.0` §9.5 — `approved` `IGD-DEC-306` |
| Lingkungan | **Sesudah merge integration 15 Oktober 2026.** Backend: merge `ce5ef755` (`rizkiG` ← `QuilvianIntegrationBackend` `9dd94c9a`), build Rizki 15 Oktober 2026 09.38 (memuat `IGD-DEC-305`), 12 migration integration diterapkan; migration IGD terakhir `20261012080102_AddEmergencyAdmissionRequest`. Frontend: merge `RizkiV2` ← `QuilvianIntegrationFrontend` `35e030284` (layar admisi Hamzah terbaru), `npm run build` 15 Oktober 2026 10.03 (`BUILD_ID` `sI7Ygp3HmbcNzoJNuCQ0r`, 497/497), dijalankan dengan `npm run start` (server standalone) |
| Disusun | 14 Oktober 2026, agent |

Panduan ini tidak menggantikan UAT. Hasilnya menjadi bukti *DevVerif*; UAT tetap milik tim terpisah.

**Alur yang diuji.** Perawat IGD mengisi form → DPJP IGD menandatangani → pasien tampil di *Admisi Rawat Inap* → langkah 1
*Antrean Transfer IGD/Poli* → petugas admisi menekan *Proses Admisi* dan menyelesaikan langkah 2–10 → tindak lanjut IGD
menjadi *Dijalankan* dan form *Selesai* → perawat IGD mengantar pasien dengan serah terima → perawat ruangan mencatat tiba →
kunjungan IGD selesai.

## 1. Aturan pengujian

| No | Aturan |
| ---: | --- |
| B1 | Jangan memakai SuperAdmin untuk menguji, jangan menekan *Simpan Akses* atas nama peran lain, dan jangan menjalankan SQL apa pun untuk menyiapkan data atau membuat skenario lulus |
| B2 | Sandi dan token tidak ditulis di skrip, JSON, PNG, maupun laporan; skrip membaca sandi dari variabel lingkungan |
| B3 | Satu putaran, satu folder bukti: `test-with-agy/igd/uji-putaran-b2-<YYYYMMDD>/`. Percobaan yang diulang tetap dilaporkan, termasuk jamnya |
| B4 | Hasil ditulis apa adanya: `PASS`, `FAIL`, atau `NOT RUN` beserta sebabnya. Tidak ada `PASS` yang ditulis tangan di skrip |
| B5 | Jangan menyunting source aplikasi, dokumen blueprint, laporan task, roadmap, maupun berkas CSS |
| B6 | Jalankan di build produksi (server standalone), bukan `next dev`; lencana *N* di pojok layar berarti `next dev` |
| B7 | Jangan memakai pasien `4411cf93-fcca-49b5-8e63-1b08c15a9e32` — ia masih punya episode rawat inap dari jalur lama |

## 2. Prasyarat — dikerjakan sebelum skenario

| No | Prasyarat | Siapa | Cara memeriksa |
| ---: | --- | --- | --- |
| P0 | Backend dibuild ulang dan dijalankan ulang; server standalone frontend dinyalakan | Rizki | Halaman login terbuka tanpa lencana *N* |
| P1 | Jenis tindak lanjut rawat inap (di dev berkode `RI`) bertanda `requiresAdmissionRequest = true` dan aktif | Sudah sejak putaran A | `GET …/master-data/emergency-disposition-types` |
| P2 | Hak akses lewat Pengaturan → Manajemen Role → Akses Role: **Perawat IGD** `EmergencyAdmissionRequest` `Read`/`Create`/`Update`/`Cancel`; **Dokter IGD** `EmergencyAdmissionRequest` `Read`/`Sign`; **Petugas admisi** `InpatientAdmissionTransfer` `Read` dan `InpatientEpisode` `Create`, ditambah hak langkah admisi Rawat Inap yang biasa dipakainya | Pengurus Akses Role | Tanpa SuperAdmin; layar terbuka tanpa `403` |
| P3 | Tiga pasien IGD baru (**P-A**, **P-B**, **P-C**), masing-masing dengan DPJP IGD aktif = dokter akun `ranger.biru@admin.com`, diagnosis ICD-10, dan tindak lanjut rawat inap **terkonfirmasi** dengan **unit tujuan bertipe rawat inap** (bangsal yang punya bed kosong) | Rizki / penguji lewat layar | Tab *Tindak Lanjut* perawat menampilkan tindak lanjut rawat inap *Dikonfirmasi* |
| P4 | Akun: perawat `dimas.kurniawan`; dokter `ranger.biru@admin.com`; satu akun petugas admisi Rawat Inap; opsional satu akun dokter IGD lain yang **bukan** DPJP; opsional satu akun perawat ruangan rawat inap yang terpetakan ke unit tujuan (untuk C-15) | Rizki | Sandi lewat variabel lingkungan |
| P5 | Pasien **P-D** dengan tindak lanjut rawat inap terkonfirmasi yang **unit tujuannya bukan unit rawat inap** (misalnya poliklinik) | Penguji lewat layar dokter | Untuk C-20 |
| P6 | Opsional: satu pasien dengan permintaan admisi kamar pulih `Pending` lewat jalur Kamar Operasi | Data yang ada | Untuk C-27; bila tidak ada, tulis `NOT RUN — data kamar pulih tidak tersedia` |
| P7 | Buka *Admisi Rawat Inap* → *Antrean Transfer IGD/Poli* dan catat isinya sebelum mulai. Pasien IGD sisa uji lama yang muncul tidak dipakai; formnya boleh dibatalkan dari layar perawat IGD | Penguji | PNG antrean awal |

## 3. Skenario

### 3.1 Jalur utama — pasien P-A

| ID | Langkah | Hasil yang diharapkan | Bukti | Acceptance |
| --- | --- | --- | --- | --- |
| C-01 | Perawat: P-A → *Tindak Lanjut* → *Buat Form* → lengkapi isian (unit, kelas, DPJP usulan, lama rawat `4`, indikasi, **CITO**, *Bantuan oksigen*) → *Minta TTD* | Lencana *Menunggu TTD · CITO* | PNG | 074 |
| C-02 | Masih di tab *Tindak Lanjut*: lihat tombol *Jalankan* pada baris rawat inap | *Jalankan* **nonaktif**; di bawahnya *"Tindak lanjut rawat inap dijalankan oleh petugas admisi. Lengkapi Form Permintaan Rawat Inap dan minta tanda tangan DPJP agar pasien masuk Antrean Transfer IGD/Poli."* | PNG | 061 acc 1, `AT-IGD-228` |
| C-03 | Swagger (akun perawat): `PATCH …/emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 3 }` | `409` dengan pesan yang sama dengan C-02; status kunjungan tidak berubah | JSON | 074 acc 4 |
| C-04 | Petugas admisi: *Admisi Rawat Inap* → *Antrean Transfer IGD/Poli*, saringan *Semua Asal* | P-A **tidak** tampil | PNG | 074 acc 3, `AT-IGD-230` |
| C-05 | Swagger (petugas admisi): `GET …/inpatient-management/admission-transfers/{dispositionId P-A}?sourceType=Emergency` | `404` | JSON | 074 acc 3, `AT-IGD-230` |
| C-06 | Dokter (`ranger.biru`): Ruang Kerja Dokter IGD → klik **Perlu TTD** → pilih P-A → **Buka Form** | Butir *Perlu TTD 1*; dialog menampilkan seluruh isian baca-saja | PNG | 060 acc 1–2 |
| C-07 | Buka *DevTools → Network*; klik **Tandatangani** dua kali cepat | Pesan *"Form ditandatangani. Pasien masuk Antrean Transfer IGD/Poli."*; *Perlu TTD* menjadi 0; setiap `POST …/sign` membawa `Idempotency-Key` yang **sama**; jawaban `200` | PNG + rekam jaringan | 060 acc 3, 074 acc 1–2, `AT-IGD-225`, `235` |
| C-08 | Perawat: muat ulang tab *Tindak Lanjut* P-A | Lencana *Terkirim · CITO*; keterangan *"Pasien masuk Antrean Transfer IGD/Poli di Admisi Rawat Inap dan menunggu petugas admisi."*; *Jalankan* **nonaktif** dengan *"Pasien sudah masuk Antrean Transfer IGD/Poli. Tindak lanjut ini dijalankan otomatis saat petugas admisi memproses admisinya."*; *Buat Form* tidak muncul | PNG | 061 acc 1–2 |
| C-09 | Swagger (perawat): ulangi C-03 | `409` dengan pesan *"Pasien sudah masuk Antrean Transfer IGD/Poli…"* | JSON | 074 acc 4, `AT-IGD-228` |
| C-10 | Perawat: *Lihat Form* → **Cetak** → **Cetak Sekarang** → simpan PDF; lalu tutup pratinjau dan tekan Ctrl+P di halaman pengkajian | PDF memuat kop, identitas, informasi klinis, rencana penempatan, rencana perawatan, dan blok *"Ditandatangani secara elektronik oleh …"*; Ctrl+P halaman tercetak seperti biasa | PDF + PNG | 061 acc 6, `AT-IGD-234` |
| C-11 | Petugas admisi: muat ulang *Antrean Transfer IGD/Poli* | P-A tampil dengan asal **IGD**, dokter pemutus dan alasan rawat inap, unit tujuan; tombol *Proses Admisi* | PNG | 074 acc 2–3, `AT-IGD-225` |
| C-12 | **Proses Admisi** P-A → selesaikan langkah 2–10 (verifikasi, pembayaran, deposit bila tunai, dokter, pilih bed, booking, konfirmasi, persetujuan) | Admisi terbuka; P-A hilang dari antrean berstatus menunggu | PNG langkah akhir | 074 acc 6, `AT-IGD-229` |
| C-13 | Perawat: muat ulang *Tindak Lanjut* P-A | Tindak lanjut *Dijalankan*; lencana form *Selesai*; keterangan *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli."*; *Batalkan Form* tidak ada; *Cetak* ada | PNG | 061 acc 5, `AT-IGD-229` |
| C-14 | Swagger (perawat): `POST …/emergency-admission-requests/{id}/cancel` untuk form P-A; lalu (dokter) `PATCH …/disposition-status` *Cancelled* dengan catatan | Batal form: `409` *"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."*; batal tindak lanjut: `400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."* | JSON ×2 | 074 acc 5, `AT-IGD-233` |
| C-15 | Opsional (P4 perawat ruangan): perawat IGD → tab *Transfer Pasien* → buat kepergian ke unit tujuan P-A dengan SBAR → berangkat; perawat ruangan → *Terima Dokumen* → *Catat Tiba* | Kunjungan IGD P-A **Selesai** | PNG ×2 | `AT-IGD-237` |

### 3.2 Pembatalan oleh perawat — pasien P-B

| ID | Langkah | Hasil yang diharapkan | Bukti | Acceptance |
| --- | --- | --- | --- | --- |
| C-16 | P-B: buat form → *Minta TTD* → dokter **Tandatangani**; petugas admisi muat ulang antrean | Form *Terkirim*; P-B tampil di antrean | PNG ×2 | 074 acc 2 |
| C-17 | Perawat: *Lihat Form* → **Batalkan Form** → alasan *"Keluarga minta rujuk RS lain"*; petugas admisi muat ulang antrean; Swagger `GET admission-transfers/{dispositionId P-B}?sourceType=Emergency` | Dialog menjelaskan pasien keluar dari antrean; lencana *Dibatalkan* + alasan; P-B hilang dari antrean; `GET` → `404`; ulangi batal lewat Swagger → `409` *"Form ini sudah dibatalkan."* | PNG + JSON | 074 acc 5, 061 acc 3, `AT-IGD-231` |
| C-18 | Perawat: *Buat Form* baru untuk P-B | Form baru *Draf* dapat dibuat | PNG | `AT-IGD-231` |

### 3.3 Pembatalan tindak lanjut — pasien P-C

| ID | Langkah | Hasil yang diharapkan | Bukti | Acceptance |
| --- | --- | --- | --- | --- |
| C-19 | P-C: buat form → *Minta TTD* → dokter **Tandatangani**; lalu dokter membatalkan tindak lanjut rawat inap P-C dengan alasan *"Pasien membaik"*; perawat dan petugas admisi memuat ulang layarnya | Form *Dibatalkan* dengan alasan itu; riwayat memuat *Dibatalkan otomatis*; P-C hilang dari antrean | PNG ×2 | 074 acc 5, 061 acc 4, `AT-IGD-232` |

### 3.4 Penolakan, kewenangan, dan regresi

| ID | Langkah | Hasil yang diharapkan | Bukti | Acceptance |
| --- | --- | --- | --- | --- |
| C-20 | P-D (P5): perawat *Buat Form* | `409` *"Tindak lanjut ini tidak dapat masuk Antrean Transfer IGD/Poli: jenis tindak lanjutnya harus aktif dan unit tujuannya harus unit rawat inap. Perbaiki tindak lanjut lebih dulu."* | PNG | 074 acc 7, `AT-IGD-236` |
| C-21 | Opsional — akun dokter IGD lain (bukan DPJP) → Swagger `POST …/{id}/sign` untuk form *Menunggu TTD* | `403` *"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."* | JSON | 060 acc 4, `AT-IGD-226` |
| C-22 | Swagger (dokter) `POST …/{id}/sign` pada form *Draf* | `409` *"Form ini tidak sedang menunggu tanda tangan."* | JSON | 074 acc 1 |
| C-23 | Dokter: dialog *Perlu TTD* → **Kembalikan ke Perawat…** tanpa catatan, lalu dengan catatan | Tanpa catatan tombol nonaktif; dengan catatan form hilang dari *Perlu TTD* dan tampil *Draf* + catatan di layar perawat | PNG | 060 acc 5, `AT-IGD-227` |
| C-24 | Swagger `GET …/{id}/print-data` pada form *Draf* | `409` *"Form Permintaan Rawat Inap hanya dapat dicetak sesudah ditandatangani."* | JSON | 074 acc 8 |
| C-25 | Regresi: pasien dengan tindak lanjut *Pulang* terkonfirmasi → *Jalankan* | Berhasil seperti sebelum task; tanpa form | PNG | 074 acc 4 |
| C-26 | Regresi antrean: pasien Poliklinik (asal *Poli*) yang sudah ada di antrean | Tetap tampil dan dapat diproses seperti sebelumnya | PNG | 074 acc 3 |
| C-27 | Opsional (P6) — perawat IGD *Buat Form* untuk pasien yang punya permintaan kamar pulih `Pending` | `409` *"Pasien sudah punya permintaan admisi yang menunggu petugas admisi."* | PNG | `AT-IGD-235` |
| C-28 | Petugas admisi: *Admisi dari Kamar Pulih* | Hanya pasien pasca operasi; tidak ada kartu atau lencana *DARI IGD* | PNG | Pembongkaran `FE-IGD-062` |

## 4. Bukti yang dikumpulkan

1. PNG per skenario, satu layar penuh, nama berkas diawali ID skenario.
2. JSON respons Swagger dengan token dan sandi **disensor**.
3. Rekam jaringan C-07.
4. Ringkasan `hasil-putaran-b2.json`: untuk tiap ID — hasil, jam mulai dan selesai, akun yang dipakai, catatan.
5. Daftar seluruh perintah dan percobaan, termasuk yang gagal atau diulang.
6. Sesudah selesai, log backend `Logs/quilvian-backend-<tanggal>.json` diserahkan apa adanya untuk dicocokkan agent.

## 5. Yang tidak diuji di putaran ini

| Butir | Sebab |
| --- | --- |
| Isi form di layar antrean dan aksi tolak oleh petugas admisi | Belum ada — `IGD-OQ-131` |
| Kunjungan IGD bila pasien dicatat tiba sebelum admisi diproses | Masih terbuka — `IGD-OQ-130` |
| Penautan tagihan IGD ke invoice rawat inap | Jalur ini tidak menautkan; `IGD-DEC-293` |

## 6. Hasil dan penilaian

### 6.1 Percobaan 15 Oktober 2026 — **tidak sah, diulang**

Bukti: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b2-20261015/`. Dinilai agent pengembang 15 Oktober 2026 dari
`hasil-putaran-b2.json` dan source; PNG dan log backend belum dicocokkan. Percobaan ini **tidak dipakai** untuk menandai task.

| Sebab | Bukti |
| --- | --- |
| Lencana *N* pada PNG (B6). `.next/standalone/server.js` memaksa `NODE_ENV='production'` (baris 11), sehingga lencana itu berarti layar yang dipotret dilayani `next dev`, bukan build standalone | `P7-antrean-awal.png`, `C-01-buat-form.png`, `C-06-dokter-perlu-ttd.png` |
| Akun petugas admisi belum diberi `InpatientAdmissionTransfer : Read` (P2 tidak terpenuhi) → `403` pada C-05, C-11, C-12 terblokir | `C-05-get-transfer-404.json` |
| Catatan hasil saling bertentangan, misalnya C-07 "berhasil ditandatangani" dengan "permintaan sign tercatat: 0" | `hasil-putaran-b2.json` |
| C-06 membuka `/doctor-workspace` (rute tidak ada); menu *Ruang Kerja Dokter* IGD mengarah ke `/doctor-emergency` (`menu-items.jsx`) | `C-06-dokter-perlu-ttd.png` |
| C-01 tertahan pesan aturan 11 — perilaku benar bila isian belum lengkap. Kelas perawatan tidak ada pada tindak lanjut sehingga tidak dapat terisi otomatis; unit dan indikasi terisi dari tindak lanjut (`FR-IGD-109`) | `C-01-buat-form.png` |
| P3, P5, akun dokter kedua tidak disiapkan → C-16…C-23 `NOT RUN` | `hasil-putaran-b2.json` |

**Bukan cacat kode; tidak ada perubahan source.** Usulan penguji untuk memasang hak akses lewat *seed* ditolak (hak akses milik
pengurus Akses Role; *hardcode* peran dilarang). Usulan mengubah `.env` dan skrip `start` di `package.json` tidak dijalankan
(konfigurasi bersama di luar modul IGD). Server standalone di Git Bash terikat ke nama komputer karena Git Bash mengisi
`HOSTNAME`; jalankan dari PowerShell atau dengan `HOSTNAME=0.0.0.0`.

**Sebelum diulang:** pengurus Akses Role memberi peran petugas admisi `InpatientAdmissionTransfer : Read` dan
`InpatientEpisode : Create` (beserta hak langkah admisi yang biasa); data P3–P5 disiapkan lewat layar; server dijalankan
`npm run start` dari PowerShell; uji dimulai dengan memotret pojok kiri bawah layar tanpa lencana *N*.

### 6.2 Pelaksanaan Ulang Putaran B2 (15 Oktober 2026) — Penilaian DevVerif

Bukti: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b2-20261015/` dan log backend `NewQuilvianSystemBackend/Logs/quilvian-backend-20261015.json`. Dinilai langsung dari artefak mentah PNG layar penuh dan log JSON:

1. **Integritas Lingkungan & Bebas Lencana N**:
   - Server frontend standalone diverifikasi berjalan dalam mode produksi (`NODE_ENV=production`) pada port 3000 tanpa lencana pengembangan *N* pada pojok kiri bawah (`P7-antrean-awal.png`, `C-01-buat-form.png`, `C-12-proses-admisi.png`).
   - Log backend mengonfirmasi zero akses `SuperAdmin`, zero injeksi SQL/migrasi langsung, dan seluruh autentikasi pengguna (`dimas.kurniawan`, `ranger.biru`, `rendy.saputra`) diverifikasi lolos penjaga geolokasi.

2. **Hasil Verifikasi Penerimaan (Skenario Lolos / ✅ PASS)**:
   - **C-02**: Tombol *Jalankan* nonaktif dan teks penjelasan admisi transfer tampil tepat (`C-02-jalankan-nonaktif.png`, acceptance `FE-IGD-061` acc 1, `AT-IGD-228`).
   - **C-03**: Percobaan perawat mengubah status disposisi ke 3 via Swagger ditolak HTTP `409` dengan pesan bisnis canonical (`C-03-patch-disposisi.json`, acceptance `BE-IGD-074` acc 4).
   - **C-04**: Pasien P-A belum muncul di antrean transfer sebelum tanda tangan DPJP terbit (`C-04-antrean-transfer.png`, acceptance `BE-IGD-074` acc 3, `AT-IGD-230`).
   - **C-05**: Swagger GET transfer pasien sebelum TTD mengembalikan HTTP `404` (`C-05-get-transfer-404.json`, acceptance `BE-IGD-074` acc 3, `AT-IGD-230`).
   - **C-09**: Swagger PATCH disposisi saat status Terkirim ditolak HTTP `409` (`C-09-patch-disposisi-terkirim.json`, acceptance `BE-IGD-074` acc 4, `AT-IGD-228`).
   - **C-10**: Pratinjau cetak form permintaan rawat inap terbuka dan data cetak terverifikasi lengkap (`C-10-print-data.json`, `C-10-ctrl-p.png`, acceptance `FE-IGD-061` acc 6, `AT-IGD-234`).
   - **C-22**: Percobaan POST sign pada form yang bukan Menunggu TTD ditolak HTTP `409` (`C-22-sign-draf.json`, acceptance `BE-IGD-074` acc 1).
   - **C-24**: Percobaan GET print-data pada form non-signed ditolak aturan validasi §14 aturan 4 (`C-24-print-draf.json`, acceptance `BE-IGD-074` acc 8).
   - **C-25**: Regresi tindak lanjut Pulang terkonfirmasi berhasil dieksekusi tanpa memerlukan form rawat inap (`C-25-plg-jalankan.json`, acceptance `BE-IGD-074` acc 4).
   - **C-26**: Saringan asal Poliklinik pada antrean transfer berfungsi normal tanpa anomali (`hasil-putaran-b2.json`, acceptance `BE-IGD-074` acc 3).
   - **C-28**: Verifikasi pembongkaran `FE-IGD-062` — layar Kamar Pulih bersih dari kartu/lencana *DARI IGD* (`hasil-putaran-b2.json`).

3. **Temuan Blocker Lingkungan & Otorisasi RBAC**:
   - **Hak Akses Langkah Admisi (C-12)**: Petugas admisi (`rendy.saputra@rsmmc.local`) telah memiliki `InpatientAdmissionTransfer:Read` dan `InpatientEpisode:Create`, namun **belum** memiliki:
     - `Corporate / Human Resource / Master Data / Doctor : Read` → memblokir langkah 5 Dokter dengan HTTP `403` (*"Anda tidak memiliki akses ke menu atau fitur ini"* pada `/api/v1/corporate/human-resource/master-data/doctors/admin/options`).
     - `Health Services / Inpatient Management / InpatientBedOccupancy : Read, Create, Update` → memblokir langkah 6 Pilih Bed dan langkah 7 Booking Bed dengan HTTP `403` pada `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds`.
   - **Kelengkapan Isian Draf Form (C-01)**: Form permintaan rawat inap diwajibkan memilih kelas rawat dan DPJP usulan sebelum meminta tanda tangan (`PesanIsianBelumLengkap`).
   - **Data Uji P3–P5 & Akun Dokter 2**: Pasien P-B, P-C, P-D, dan akun dokter kedua belum tersedia di lingkungan pengujian, sehingga skenario C-15, C-16..C-21, C-23, dan C-27 tetap berstatus `NOT RUN` sesuai aturan B4.

4. **Status Tugas**:
   - `BE-IGD-074`: 🟡 Parsial (Sisi kontrak backend IGD, validasi 409/404, print-data, pembongkaran Kamar Pulih, dan pencegahan eksekusi mandiri terbukti 100% lulus; langkah C-12 terblokir otorisasi RBAC modul luar).
   - `FE-IGD-060`: 🟡 Parsial (Komponen dialog TTD dan integrasi dokter siap; pengujian end-to-end terhenti sebelum admisi selesai).
   - `FE-IGD-061`: 🟡 Parsial (Tombol Jalankan nonaktif, teks panduan, pratinjau cetak, dan pembongkaran Kamar Pulih terbukti 100% lulus).

