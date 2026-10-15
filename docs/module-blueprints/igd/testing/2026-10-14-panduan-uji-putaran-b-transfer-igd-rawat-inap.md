# Panduan Uji Putaran B — Transfer Pasien IGD ke Rawat Inap (`BE-IGD-073`…`075` + `FE-IGD-060`…`062`)

> **DITARIK — 14 Oktober 2026 (`IGD-DEC-304`).** Panduan ini menguji jalur permintaan admisi lewat mode *Admisi dari Kamar Pulih* yang sudah dibongkar. Jangan dipakai; hasil yang sudah tercatat dari panduan ini tidak dinilai. Pengganti: [panduan putaran B baru](2026-10-14-panduan-uji-putaran-b2-antrean-transfer-igd.md).


| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-073`, `BE-IGD-074`, `BE-IGD-075`, `FE-IGD-060`, `FE-IGD-061`, `FE-IGD-062` — diuji bersama dari layar (pasangan BE/FE); aturan §14 nomor 4 `BE-IGD-072` ikut dibuktikan |
| Acceptance yang dibuktikan | `AT-IGD-225`, `226`, `228`…`235`; acceptance 1–6 tiap task (laporan task bagian *Skenario uji*) |
| Kontrak | API `0.17.1` §12.2, §12.4, §12.5; validation `0.16.0` §14 aturan 4, 13–25; state `0.11.0` §11.2–11.4; integration `0.8.0` §9 |
| Lingkungan | Backend: build Rizki 14 Oktober 2026 (DLL 09.00.11) + build ulang untuk `BE-IGD-075`; migration `20261012080102_AddEmergencyAdmissionRequest` dan `20261013100736_ExtendInpatientAdmissionReferralForEmergency` sudah diterapkan. Frontend: `npm run build` 14 Oktober 2026 13.03 (`BUILD_ID` `CdZrb6rn2PX6y3xbZLSNX`), dijalankan sebagai server standalone port 3000 — **bukan** `next dev` |
| Disusun | 14 Oktober 2026, agent |

Panduan ini tidak menggantikan UAT. Hasilnya menjadi bukti *DevVerif*; UAT tetap milik tim terpisah.

## 1. Aturan pengujian

| No | Aturan |
| ---: | --- |
| B1 | Jangan memakai SuperAdmin untuk menguji, jangan menekan *Simpan Akses* atas nama peran lain, dan jangan menjalankan SQL apa pun untuk menyiapkan data atau membuat skenario lulus |
| B2 | Sandi dan token tidak ditulis di skrip, JSON, PNG, maupun laporan; skrip membaca sandi dari variabel lingkungan |
| B3 | Satu putaran, satu folder bukti: `test-with-agy/igd/uji-putaran-b-<YYYYMMDD>/`. Percobaan yang diulang tetap dilaporkan, termasuk jamnya |
| B4 | Hasil ditulis apa adanya: `PASS`, `FAIL`, atau `NOT RUN` beserta sebabnya. Tidak ada `PASS` yang ditulis tangan di skrip |
| B5 | Jangan menyunting source aplikasi, dokumen blueprint, laporan task, roadmap, maupun berkas CSS |
| B6 | Jalankan di build produksi (server standalone), bukan `next dev`; lencana *N* di pojok layar berarti `next dev` |

## 2. Prasyarat — dikerjakan sebelum skenario

| No | Prasyarat | Siapa | Cara memeriksa |
| ---: | --- | --- | --- |
| P1 | Jenis tindak lanjut `RANAP` bertanda `requiresAdmissionRequest = true` | Sudah dikerjakan pada putaran A | `GET …/master-data/emergency-disposition-types` |
| P2 | Hak akses lewat Pengaturan → Manajemen Role → Akses Role: **Perawat IGD** `EmergencyAdmissionRequest` `Read`/`Create`/`Update`/`Cancel`; **Dokter IGD** `EmergencyAdmissionRequest` `Read`/`Sign`; **Petugas admisi** `InpatientAdmissionReferral` `Read`/`Cancel` dan `InpatientEpisode` `Create` | Pengurus Akses Role | Tanpa izin `Sign`, butir *Perlu TTD* tidak tampil di layar dokter; tanpa izin `Cancel`, tombol *Batalkan Permintaan* tidak tampil di layar admisi — itu temuan prasyarat, bukan cacat |
| P3 | Tiga pasien IGD (**P-A**, **P-B**, **P-C**), masing-masing dengan DPJP IGD aktif = dokter akun `ranger.biru@admin.com` dan tindak lanjut `RANAP` **terkonfirmasi**; tidak satu pun punya permintaan admisi `Pending` | Rizki / penguji lewat layar | Tab *Tindak Lanjut* perawat menampilkan *Rawat inap · Dikonfirmasi* dan tombol *Buat Form*. Pasien putaran A (kunjungan `IGD-261003021124-4C83AE`, form Draf) boleh dipakai sebagai P-A |
| P4 | Akun: perawat `dimas.kurniawan`; dokter `ranger.biru@admin.com`; satu akun petugas admisi Rawat Inap; opsional satu akun dokter IGD lain yang **bukan** DPJP pasien | Rizki | Sandi lewat variabel lingkungan |
| P5 | Opsional: satu pasien dengan permintaan admisi kamar pulih `Pending` lewat jalur Kamar Operasi | Data yang ada | Untuk B-21 dan B-22; bila tidak ada, tulis `NOT RUN — data kamar pulih tidak tersedia` |

## 3. Skenario

### 3.1 Jalur utama — pasien P-A

| ID | Langkah | Hasil yang diharapkan | Bukti | Task |
| --- | --- | --- | --- | --- |
| B-01 | Perawat: P-A → *Tindak Lanjut* → *Buat Form* (atau *Buka Form*) → lengkapi isian (unit, kelas, DPJP usulan, lama rawat `4`, indikasi, **CITO**, *Bantuan oksigen*) → *Minta TTD* | Lencana *Menunggu TTD · CITO* | PNG | 072 |
| B-02 | Masih di tab *Tindak Lanjut*: lihat tombol *Jalankan* pada baris *Rawat inap* | *Jalankan* **nonaktif**, di bawahnya *"Form Permintaan Rawat Inap belum ditandatangani DPJP."* | PNG | 061-U1 |
| B-03 | Lewat Swagger (akun perawat): `PATCH …/emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 3 }` | `409` *"Form Permintaan Rawat Inap belum ditandatangani DPJP."*; status kunjungan tidak berubah | JSON | 074-S5 |
| B-04 | Dokter (`ranger.biru`): buka Ruang Kerja Dokter IGD | Ringkasan memuat butir **Perlu TTD 1**; kartu P-A bertanda *Perlu TTD* | PNG | 060-U1 |
| B-05 | Klik **Perlu TTD**, pilih P-A, klik **Buka Form** | Daftar tersaring; dialog *Tanda Tangan Form Permintaan Rawat Inap* menampilkan seluruh isian baca-saja, tanpa kolom isian | PNG | 060-U2 |
| B-06 | Buka *DevTools → Network*; klik **Tandatangani** dua kali cepat | Dialog tertutup, *"Form ditandatangani dan dikirim ke petugas admisi."*; butir *Perlu TTD* menjadi 0; setiap `POST …/sign` membawa header `Idempotency-Key` yang **sama**; jawaban `200` | PNG + rekam jaringan (HAR atau tangkapan tab Network) | 060-U3, 074-S1, S2 |
| B-07 | Perawat: muat ulang tab *Tindak Lanjut* | Lencana *Terkirim · CITO*, *"Menunggu petugas admisi."*; *Jalankan* **aktif**; *Buat Form* tidak muncul | PNG | 061-U2 |
| B-08 | Perawat: *Lihat Form* → **Cetak** → **Cetak Sekarang** → simpan sebagai PDF | Kertas: kop, judul *FORM PERMINTAAN RAWAT INAP*, identitas, informasi klinis, rencana penempatan, rencana perawatan, blok tanda tangan *"Ditandatangani secara elektronik oleh …, … WIB"*; hanya kertas yang tercetak | PDF + PNG | 061-U6, 074-S12 |
| B-09 | Ctrl+P di halaman pengkajian IGD tanpa pratinjau terbuka | Halaman tercetak seperti biasa | PNG pratinjau cetak | 061-U7 |
| B-10 | Petugas admisi: Admisi Rawat Inap | Pilihan *Admisi dari Kamar Pulih* berlencana `… · DARI IGD [ 1 ]` | PNG | 062-U1 |
| B-11 | Masuk mode itu | Kartu P-A berlencana **DARI IGD** dan **CITO**, paling atas; *Unit: … · Kelas …*; *DPJP usulan: …*; tab **Dari IGD** menyaring kartu IGD | PNG | 062-U1, 073 acc 2 |
| B-12 | Pilih kartu P-A | Panel *Form Permintaan Rawat Inap IGD* memuat no. kunjungan, unit & kelas, DPJP usulan, diagnosis, alergi, indikasi, rencana, kebutuhan khusus, lama rawat, penanda tangan; catatan unit tidak diisikan otomatis | PNG | 062-U2, 073 acc 3 |
| B-13 | Petugas admisi: admisi biasa lewat *Pasien Lama* untuk P-A | Ditolak `409` *"Pasien punya permintaan admisi yang menunggu. Buka admisi dari permintaan itu."* | PNG + JSON | 062-U4, 073 acc 5 |
| B-14 | Kembali ke kartu P-A → **Pilih & Lanjutkan Admisi** → sampai langkah Dokter → selesaikan admisi | Kelas di langkah Pembayaran dan DPJP di langkah Dokter **terisi**, unit **tidak**; keduanya dapat diganti; admisi terbuka | PNG ×2 | 062-U3 |
| B-15 | Perawat: muat ulang *Tindak Lanjut* P-A | Lencana *Selesai*; *"Admisi rawat inap sudah dibuka dari permintaan ini."*; di form: *"Selesai — admisi dibuka {tanggal jam}"*; *Batalkan Form* tidak ada; *Cetak* ada; *Jalankan* aktif | PNG | 061-U2 |
| B-16 | Swagger (perawat): `POST …/emergency-admission-requests/{id}/cancel` untuk form P-A; lalu (dokter) batalkan tindak lanjut `RANAP` P-A | Batal form: `409` *"Admisi rawat inap sudah dibuka dari permintaan ini. Form tidak dapat dibatalkan dari IGD."*; batal tindak lanjut: `409` *"Admisi rawat inap sudah dibuka dari tindak lanjut ini. Batalkan admisi di Rawat Inap lebih dulu."* | JSON | 074-S11, 061-U4 |
| B-17 | Perawat: **Jalankan** tindak lanjut `RANAP` P-A | `200`; tindak lanjut *Dijalankan* | PNG | 074-S6 |
| B-18 | Opsional — bila kasir memproses kuitansi rawat inap P-A: buka ringkasan tagihan episode | Tidak ada tautan kunjungan IGD pada invoice rawat inap | PNG | 075-S1 |

### 3.2 Pembatalan oleh petugas admisi — pasien P-B

| ID | Langkah | Hasil yang diharapkan | Bukti | Task |
| --- | --- | --- | --- | --- |
| B-19 | P-B: perawat buat form → *Minta TTD*; dokter **Tandatangani** | Form *Terkirim* | PNG | 074 |
| B-20 | Petugas admisi: kartu P-B → **Batalkan Permintaan** tanpa alasan, lalu dengan *"Kelas 2 penuh"* | Tanpa alasan tombol konfirmasi nonaktif; dengan alasan kartu hilang dan *"Permintaan admisi dibatalkan."* | PNG | 062-U5, 073 acc 4 |
| B-21 | Perawat: muat ulang *Tindak Lanjut* P-B | Lencana *Dibatalkan admisi* + alasan; *Jalankan* nonaktif dengan *"Permintaan admisi dibatalkan petugas admisi. Ajukan Form Permintaan Rawat Inap baru."*; *Buat Form* muncul | PNG | 061-U3, 074-S7 |
| B-22 | Perawat: *Buat Form* baru untuk P-B → *Minta TTD* (jangan ditandatangani); dokter batalkan tindak lanjut `RANAP` P-B dengan alasan *"Pasien membaik"* | Form baru *Dibatalkan* dengan alasan itu; riwayat memuat *Dibatalkan otomatis* | PNG | 074-S10, 061-U5 |

### 3.3 Pembatalan sesudah terkirim — pasien P-C

| ID | Langkah | Hasil yang diharapkan | Bukti | Task |
| --- | --- | --- | --- | --- |
| B-23 | P-C: buat form → *Minta TTD* → dokter **Tandatangani** | Form *Terkirim* | PNG | 074 |
| B-24 | Perawat: *Lihat Form* → **Batalkan Form** → alasan *"Keluarga minta rujuk RS lain"* | Dialog menjelaskan permintaan admisi ikut batal; lencana *Dibatalkan* + alasan; kartu P-C hilang dari layar admisi; ulangi batal lewat Swagger → `409` *"Form ini sudah dibatalkan."* | PNG + JSON | 074-S8, 061-U4 |
| B-25 | P-C: *Buat Form* baru → *Minta TTD* → dokter **Tandatangani**; lalu dokter batalkan tindak lanjut `RANAP` P-C | Permintaan admisi ikut `Cancelled` (`GET /admission-referrals/{id}` → `cancellationSource` = 3); form tetap *Signed* dengan lencana *Dibatalkan* | PNG + JSON | 074-S9 |

### 3.4 Penolakan, kewenangan, dan regresi

| ID | Langkah | Hasil yang diharapkan | Bukti | Task |
| --- | --- | --- | --- | --- |
| B-26 | Opsional — akun dokter IGD lain (bukan DPJP) → Swagger `POST …/{id}/sign` untuk form *Menunggu TTD* | `403` *"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."* | JSON | 074-S3, 060-U4 |
| B-27 | Swagger (dokter) `POST …/{id}/sign` pada form *Draf* | `409` *"Form ini tidak sedang menunggu tanda tangan."* | JSON | 074-S4 |
| B-28 | Dokter: dialog *Perlu TTD* → **Kembalikan ke Perawat…** tanpa catatan, lalu dengan catatan | Tanpa catatan tombol nonaktif; dengan catatan form hilang dari *Perlu TTD* dan tampil *Draf* + catatan di layar perawat | PNG | 060-U5 |
| B-29 | Swagger `GET …/{id}/print-data` pada form *Draf* | `409` *"Form Permintaan Rawat Inap hanya dapat dicetak sesudah ditandatangani."* | JSON | 074-S12 |
| B-30 | Regresi tindak lanjut lain: pasien dengan tindak lanjut *Pulang* (`PLG`) terkonfirmasi → *Jalankan*; pasien lain → batalkan tindak lanjut non-rawat-inap | Perilaku seperti sebelum task: tanpa penahanan, tanpa form | PNG | 074-S13 |
| B-31 | Opsional (P5) — layar admisi: kartu kamar pulih | Kartu, panel, dan alur sama seperti sebelumnya; tidak ada pengisian awal kelas/DPJP; tidak ada tombol batal | PNG | 062-U6, 073 acc 6 |
| B-32 | Opsional (P5) — admisi dari permintaan kamar pulih, lalu proses kuitansinya | Kunjungan asal tetap tertaut pada invoice rawat inap | PNG | 075-S2 |
| B-33 | Pasien dengan permintaan kamar pulih `Pending` (P5) → perawat IGD *Buat Form* | `409` *"Pasien sudah punya permintaan admisi yang menunggu petugas admisi."* | PNG | 072 aturan 4 |

## 4. Bukti yang dikumpulkan

1. PNG per skenario, satu layar penuh, nama berkas diawali ID skenario.
2. JSON respons Swagger dengan token dan sandi **disensor**.
3. Rekam jaringan B-06 yang memperlihatkan header `Idempotency-Key`.
4. PDF cetakan B-08.
5. Rentang jam pengujian dan daftar percobaan ulang, supaya agent mencocokkan jumlah permintaan dengan
   `Logs/quilvian-backend-20261014.json` (`UserName`, `Path`, `StatusCode`).

Hasil dinilai agent dari bukti mentah, bukan dari ringkasan.
