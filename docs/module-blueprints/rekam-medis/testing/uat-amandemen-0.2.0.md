# Laporan Kesiapan UAT — Amandemen Rekam Medis Kontrak `0.2.0`

Laporan ini mencatat hasil 19 skenario UAT amandemen (`04-prd-to-mvp.md` bagian 18) terhadap
backend nyata, sebagai bukti task `FE-15`. Setiap UAT bertanda **lulus** atau **terbuka** beserta
sebabnya dan bukti pengganti yang sudah ada.

| Field | Nilai |
| --- | --- |
| Tanggal uji | 2 November 2026 |
| Lingkungan | Backend lokal (`https://localhost:7184`, Development) tersambung ke basis data devYoga (`QuilvianNewDevYoga`); frontend `next dev` (`http://localhost:3000`) |
| Snapshot | Backend `yoga` / `b2b10268` ditambah perubahan `BE-32`–`BE-41` yang belum di-commit. Frontend `YogaV2` / `1ea4ad263` ditambah perubahan `FE-10`–`FE-14` yang belum di-commit |
| Pelaku | Akun superadmin hasil seed (layar login sungguhan, geofence dari konfigurasi) |
| Kontrak | API `0.2.0` A1–A4 (`approved` 28 Oktober 2026) |
| Keputusan cakupan | Pemilik modul, 2 November 2026: UAT yang butuh data klinis baru di devYoga **dicatat terbuka**. Data ujinya dibuat di sesi UAT terpisah bersama pemilik modul sumber |

## 1. Ringkasan

| Keadaan | Jumlah | UAT |
| --- | --- | --- |
| **Lulus terhadap backend nyata** | 7 | `UAT-01`, `03`, `04`, `10`, `14`, `16`, `19` |
| **Terbuka — data uji belum ada di devYoga** | 8 | `UAT-05`, `06`, `07`, `08`, `09`, `11`, `12`, `17` |
| **Terbuka — butuh penulisan atau akun yang tidak dijalankan** | 3 | `UAT-13`, `15`, `18` |
| **Terbuka — butuh injeksi gangguan** | 1 | `UAT-02` |

Setiap UAT terbuka punya bukti pengganti berupa uji penerimaan backend (`AT-RM-*`, integrasi atau
API) dan/atau E2E antarmuka dengan API tiruan. Perilaku yang diuji sudah terbukti. Yang belum ada
adalah pengamatan di lingkungan nyata dengan data sungguhan.

**Kesiapan.** Amandemen **siap untuk sesi UAT bersama pemilik modul**. Bagian layar dan backend yang
diuji tidak menunjukkan cacat yang menghalangi. Ada dua celah backend yang ditemukan selama delivery
frontend, dan keduanya tidak menghalangi layar (bagian 4).

## 2. Hasil per UAT

| UAT | Skenario | Keadaan | Bukti |
| --- | --- | --- | --- |
| `UAT-01` | Pasien punya hasil lab yang dirilis → buka berkas → hasil lab tampil | **Lulus** | Pasien `5a656dd1…` (`ENC-RSMMC-00182`): `/timeline` `200`; linimasa memuat Hasil Laboratorium dan Hasil Radiologi; baris "Buka detail Hasil Laboratorium" tergambar |
| `UAT-02` | Farmasi dibuat lambat → berkas terbuka, peringatan menyebut Farmasi | **Terbuka** — memperlambat Farmasi di devYoga butuh injeksi gangguan: mengunci tabel Farmasi di basis data bersama, atau mengubah kode | `AT-RM-50` (`BE-21`: pembaca Farmasi dipalsukan menunggu, masuk `failedSources`, berkas tetap `200`); `AT-RM-49` (`BE-21`); E2E `FE-10` blok "RIWAYAT INI TIDAK LENGKAP" |
| `UAT-03` | Laporan radiologi dirilis lalu diamandemen → bertanda pernah diralat | **Lulus** | Pasien `5a656dd1…`: laporan "UJI-RAD Foto Thorax PA" (versi 2, `AmendmentReleased`) → `isCorrected = true` dan penanda "Pernah diralat" tampil; laporan "THORAX PA + LATERAL" (versi 1) → `isCorrected = false` |
| `UAT-04` | Hasil kultur belum dirilis → tidak tampil | **Lulus** | Pasien `3aa77413…`: Pewarnaan BTA Sputum yang **belum** dirilis (`1f3670d7…`) tidak ada di linimasa; yang dirilis (`025be4cf…`) ada. Histopatologi PA yang belum dirilis tampil sebagai jenis "belum tersedia", bukan sebagai dokumen |
| `UAT-05` | Pasien IGD dengan triase selesai dan disposisi dikonfirmasi → keduanya tampil | **Terbuka** — devYoga tidak punya kunjungan IGD dengan triase `Completed` dan disposisi `Confirmed`/`Executed` | `AT-RM-44` pembaca IGD (`BE-29`) |
| `UAT-06` | Ringkasan pulang belum ditandatangani → tidak tampil | **Terbuka** — devYoga tidak punya ringkasan pulang sama sekali | `AT-RM-45` (`BE-24`, `BE-30`: ringkasan tanpa `SignedAt` tidak tampil) |
| `UAT-07` | Rawat inap dengan banyak pengukuran gula darah → satu baris per hari, rincian dapat dibuka | **Terbuka** — devYoga tidak punya pengukuran gula darah | `AT-RM-52` (`BE-22`); E2E `FE-11` (baris ringkasan berkala dan panel rincian) |
| `UAT-08` | Satu pengukuran dibatalkan → tidak dihitung | **Terbuka** — sama dengan `UAT-07` | `AT-RM-52` (`BE-22`: enam pengukuran, satu batal → jumlah 5); uji unit `FE-11` |
| `UAT-09` | Peserta MCU terverifikasi → resume MCU tampil | **Terbuka** — devYoga tidak punya resume MCU berstatus terverifikasi | `AT-RM-44` pembaca MCU (`BE-36`) |
| `UAT-10` | Peserta MCU tanpa nomor rekam medis → cari berkas → tidak ada berkas, tidak ada galat | **Lulus** | Order `MCU-RSMMC-000001` ("UAT FEMCU BUDI") dan `MCU-RSMMC-000003` ("UAT FEMCU KONFIRMASI") tanpa pasien: pencarian `200`, `totalData 0`, layar "Data pasien tidak ditemukan.", nol galat |
| `UAT-11` | Episode rawat inap ditutup, hasil dirilis esoknya → bertanda Susulan | **Terbuka** — tidak ada hasil yang dirilis sesudah waktu tutup kunjungannya di devYoga | `AT-RM-58` (`BE-26`); `AT-RM-47` (`BE-26`); E2E `FE-10` penanda "Susulan" |
| `UAT-12` | Rekonsiliasi IGD mengubah waktu selesai → penanda Susulan tidak berubah | **Terbuka** — devYoga tidak punya rekonsiliasi IGD | `AT-RM-57` (`BE-25`) |
| `UAT-13` | Tambah kriteria pada profil Rawat Jalan → daftar kerja menampilkan kekurangan baru | **Terbuka** — butuh simpan profil `RJ` sungguhan, yang menandai ulang seluruh penilaian, dan kunjungan RJ tutup klinis sesudah rilis (devYoga: 0) | `AT-RM-69` (`BE-37`, `BE-40`); `FE-13` manual: `PUT` ke backend nyata membawa 4 kriteria tanpa `code` (diblokir); E2E `FE-13` |
| `UAT-14` | Profil Rawat Jalan sudah ada → buat yang kedua → ditolak | **Lulus** | `FE-13` manual: `409` "Profil untuk jenis kunjungan dan unit ini sudah ada." tampil persis (permintaan pasti ditolak sebelum menulis) |
| `UAT-15` | `FLOW-RM-MVP-001` langkah 1–8 → Menunggu hasil → Menunggu hasil → Lengkap | **Terbuka** — butuh satu kunjungan rawat inap penuh dengan operasi, kultur, penutupan klinis, pengesahan, dan rilis hasil di devYoga | `AT-RM-62` (`BE-39`), `AT-RM-72` (`BE-38`), `AT-RM-58` (`BE-26`), `AT-RM-60` (`BE-30`, `BE-39`); E2E `FE-14` (keadaan, rincian, nilai ulang) |
| `UAT-16` | Kunjungan masih berjalan → nilai ulang → ditolak | **Lulus** | `FE-14` manual: `ENC-RSMMC-00185` → `409` "Kunjungan ini masih berjalan. Kelengkapan dinilai setelah pelayanan selesai." tampil persis |
| `UAT-17` | Pasien rujukan hanya cek lab → hasil dirilis → Lengkap tanpa dokumen dokter | **Terbuka** — butuh kunjungan rujukan lab yang ditutup klinis sesudah rilis (devYoga: 0 penutupan klinis) | `AT-RM-66` (`BE-32`, `BE-39`); profil `RJ-LAB` tanpa kriteria terlihat di `FE-13` manual |
| `UAT-18` | Perawat mengesahkan catatan `LockedUnsigned` → kekurangan hilang, status tetap | **Terbuka** — addendum pengesahan permanen; butuh akun perawat penulis (devYoga: kesepuluh CPPT `LockedUnsigned` ditulis akun superadmin) | `AT-RM-72` (`BE-38`); E2E `FE-12` (klik ganda satu permintaan; status tetap dengan keterangan disahkan); `FE-12` manual (`POST` diblokir) |
| `UAT-19` | Catatan sudah ditandatangani → coba sahkan → ditolak | **Lulus** | Permintaan pengesahan dari layar berkas dialihkan ke hasil lab `Signed` `e1ead1e4…` → `409` "Pengesahan hanya untuk catatan yang terkunci tanpa tanda tangan." tampil persis di modal. `VAL-RM-A01` diperiksa sebelum kewenangan dan sebelum penulisan |

## 3. Cara uji dan pengaman data

- Seluruh uji memakai layar sungguhan lewat Playwright: login superadmin, membuka berkas dengan
  `?kunjungan=aktif` (master keperluan akses devYoga masih kosong, `BE-09`), dan membaca jawaban API
  asli.
- **Tidak ada data klinis maupun master yang ditulis.** Semua permintaan selain `GET` diblokir di jaringan,
  kecuali permintaan yang pasti ditolak validasi server sebelum menulis: `VAL-RM-A04`–`A07` (`FE-13`),
  `POST /evaluate` atas kunjungan lama dan kunjungan berjalan (`FE-14`), dan pengesahan atas dokumen
  `Signed` (`UAT-19`).
- Yang tertulis sebagai efek samping adalah jejak akses pembukaan berkas (`MrcAccessLog`) dan jejak
  audit penilaian ulang. Keduanya atas nama superadmin.
- Data kandidat dicari dengan konsol Npgsql **baca-saja** (`default_transaction_read_only=on`).
- Skrip uji dan tangkapan layar: `fe15-manual.mjs` dan `fe15-shots/` (UAT-01, 03, 04, 10, 19),
  `fe13-manual.mjs` (UAT-14), `fe14-manual.mjs` (UAT-16) di scratchpad sesi delivery.

## 4. Temuan yang dicatat

| Temuan | Dampak | Tindak lanjut |
| --- | --- | --- |
| Penyaring `closedFrom`/`closedTo` daftar kerja kelengkapan menjawab `500` untuk tanggal polos `YYYY-MM-DD` | Layar `FE-14` tidak terdampak (mengirim UTC ber-`Z`); klien lain akan terdampak | Usul perbaikan `BE-39` (traceability T2.2) |
| Metadata profil dan daftar kerja menawarkan jenis kunjungan `Unknown` | Layar daftar kerja membuangnya dari penyaring; layar profil menampilkannya apa adanya | Usul perbaikan `BE-37`/`BE-39` (traceability T2.2) |
| devYoga belum punya penutupan klinis sejak rilis | Daftar kerja kelengkapan kosong; `UAT-13`, `15`, `17` tidak dapat dijalankan | Sesi UAT dengan alur klinis penuh |
| Master keperluan akses devYoga kosong (`BE-09`) | Berkas pasien di luar rawatan tidak dapat dibuka siapa pun; uji memakai jalur kunjungan aktif | Isi master sesuai SOP rumah sakit |

## 5. Data uji yang dibutuhkan untuk menutup UAT terbuka

| UAT | Data yang perlu dibuat | Modul pemilik |
| --- | --- | --- |
| `UAT-02` | Injeksi gangguan pada Farmasi di lingkungan uji terpisah (bukan basis data bersama) | Farmasi, infrastruktur |
| `UAT-05` | Kunjungan IGD: triase selesai, disposisi dikonfirmasi | IGD |
| `UAT-06` | Episode rawat inap dengan ringkasan pulang belum ditandatangani | Rawat Inap |
| `UAT-07`, `08` | Beberapa pengukuran gula darah dalam satu hari, satu dibatalkan | Manajemen Klinis |
| `UAT-09` | Order MCU dengan resume terverifikasi | MCU |
| `UAT-11` | Episode rawat inap ditutup, hasil lab dirilis sesudahnya | Rawat Inap, Laboratorium |
| `UAT-12` | Rekonsiliasi IGD yang menggeser waktu selesai kunjungan | IGD |
| `UAT-13` | Kunjungan RJ tutup klinis sesudah rilis, lalu simpan profil `RJ` dengan kriteria tambahan | Rekam Medis, Registrasi |
| `UAT-15` | `FLOW-RM-MVP-001` lengkap (rawat inap, operasi, kultur, penutupan, pengesahan, rilis) | Rawat Inap, Kamar Operasi, Laboratorium, Rekam Medis |
| `UAT-17` | Kunjungan rujukan lab ditutup klinis, hasil dirilis | Registrasi, Laboratorium |
| `UAT-18` | Akun perawat penulis CPPT `LockedUnsigned`, dan kesediaan menyimpan addendum permanen | Rekam Medis, keperawatan |
