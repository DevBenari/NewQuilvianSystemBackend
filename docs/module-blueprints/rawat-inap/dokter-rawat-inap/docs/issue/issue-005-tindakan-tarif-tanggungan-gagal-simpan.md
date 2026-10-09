# ISSUE-005 — Tab Tindakan: Tanggungan dan Tarif "Belum Tersedia", Tombol Aksi Tanpa Tanda, dan Simpan Pesanan Ditolak untuk Pasien Penjamin Perusahaan

```yaml
issue_id: ISSUE-DOK-005
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Dokter - Rawat Inap — Tab Tindakan, segmen Form Tindakan (FE-RWI-073; tata letak V1 FE-RWI-138; status tanggungan FE-RWI-176)"
sumber_laporan: "Laporan pengguna 06-10-2026: 4 butir, 2 screenshot berisi panel Network DevTools, payload dan respons POST inpatient-orders"
tanggal_issue: "2026-10-06"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "671191eb (MHamzah)"
source_sha_frontend: "1f889d67c (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-005-tindakan-tarif-tanggungan-gagal-simpan.md
ditulis_dengan: "skill diagnose-module-issue"
bukti_database: "SELECT baca-saja (transaksi READ ONLY, di-rollback) pada QuilvianNewDevHamzah — database pribadi pemilik — atas permintaan pelapor, 06-10-2026"
```

## 1. Ringkasan

Pelapor membuka tab **Tindakan** pada ruang kerja Dokter Rawat Inap untuk pasien kelas II yang
dijamin perusahaan (PT Telkom Indonesia). Ada empat keluhan: kolom Status menulis "Status
tanggungan belum tersedia", kolom Tarif menulis "tarif belum tersedia", tombol Aksi hanya berupa
kotak biru tanpa tanda, dan tombol **Simpan Pesanan Tindakan** ditolak server dengan pesan
*"Tipe pembayaran encounter tidak didukung."*

Tiga keluhan (1, 2, 4) berasal dari **satu sebab**: backend yang sedang berjalan adalah hasil
build pukul 10.01, sedangkan perbaikan untuk cara bayar "Penjamin Perusahaan"
([ISSUE-DOK-004](issue-004-tipe-pembayaran-encounter-resep.md)) baru disimpan pukul 15.42 dan
belum pernah dibangun ulang. Kode lama itu hanya mengenal Tunai dan Asuransi, sehingga setiap
perhitungan tarif dan tanggungan untuk pasien ini gagal di langkah pertama. **Tarifnya sendiri
ada di database** — Kumbah Lambung tercatat Rp 175.000. Keluhan 3 adalah bug tampilan terpisah:
tanda "+" pada tombol terjepit menjadi selebar nol piksel.

Yang paling berbahaya adalah butir 4 (**Blocker**): dokter tidak dapat memesan tindakan apa pun
untuk pasien penjamin perusahaan. Penelusuran juga menemukan tujuh temuan tambahan. Yang terberat:
lima pesanan tindakan pasien asuransi sudah tertahan sejak 22-09-2026 karena menunggu persetujuan
yang tidak punya layar (**Blocker**), server menolak pesanan setiap kali harga tidak dapat dihitung
(bertentangan dengan keputusan `RWI-DEC-218`), dan syarat persetujuan tindakan milik perusahaan
tidak ikut tersimpan. Setelah backend dibangun ulang, layar akan menulis **"Tidak Di-cover"** untuk semua
tindakan, karena PT Telkom belum punya satu pun aturan tanggungan — pemilik perlu memutuskan
apakah itu memang benar.

Tidak ada butir yang ternyata sesuai desain. Hanya permintaan tersirat "tombol diberi nama" yang
merupakan perubahan desain kecil.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli, diringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "Atasi masalah kenapa pada tindakan Status tanggungan belum tersedia — ini proses ini terjadi kenapa nya?" | Screenshot 1 dan 2: kolom STATUS pada seluruh baris katalog |
| 2 | "Kenapa tarif ya belum tersedia, apakah memang di database ya tidak ada harga" | Screenshot 1 dan 2: kolom TARIF katalog; screenshot 2: Tarif Satuan, Subtotal, dan Total perkiraan pada keranjang |
| 3 | "Button aksi kenapa tidak ada nama ya warna biru saja". Ditegaskan kemudian: "tombol pilih bukan hapus ya ada disini loh — ini adalah aksi pilih — nama ya gak ada" | Screenshot 1 dan 2: kolom AKSI katalog (kotak biru); screenshot 2: kolom AKSI keranjang (kotak merah muda); **lampiran 3** (06-10-2026): potongan kolom STATUS dan AKSI katalog — tombol Pilih tanpa nama |
| 4 | "Lalu kenapa bad request ketika saya melakukan simpan pesanan tindakan" — disertai request URL, payload, dan respons 400 | Teks request/respons pada laporan; panel Response DevTools pada kedua screenshot |
| — | "Coba anda lakukan analisis dan cek database" | — |

**Lampiran yang tidak dirujuk butir mana pun:** kotak merah **"Galat Tindakan" tanpa isi** pada
screenshot 1. Kotak itu dicatat sebagai temuan tambahan `ISS-DOK-005-T1`, karena justru itulah
sebab pelapor harus membuka DevTools untuk mengetahui alasan penolakan.

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-DOK-005-01` | 1 | Status tanggungan "belum tersedia" pada semua tindakan | `BUG` | Backend (build berjalan) | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-005-01` |
| `ISS-DOK-005-02` | 2 | Tarif "belum tersedia" padahal tarif ada di database | `BUG` | Backend (build berjalan) | Medium | SUDAH-VERIFIKASI (source + database) | `FIX-DOK-005-01` |
| `ISS-DOK-005-03` | 3 | Tombol aksi tampil sebagai kotak polos tanpa tanda | `BUG` (+ `DESIGN_CHANGE` untuk tulisan) | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-005-03`, `FIX-DOK-005-04` |
| `ISS-DOK-005-04` | 4 | Simpan Pesanan Tindakan ditolak 400 "Tipe pembayaran encounter tidak didukung." | `BUG` | Backend (build berjalan) | Blocker | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-005-01` |
| `ISS-DOK-005-T1` | — | Kotak "Galat Tindakan" tidak menampilkan isi pesan | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-005-02` |
| `ISS-DOK-005-T2` | — | Pesanan tindakan ditolak bila harga tidak dapat dihitung | `BUG` | Backend | High | SUDAH-VERIFIKASI | `FIX-DOK-005-05` |
| `ISS-DOK-005-T3` | — | Syarat persetujuan tindakan milik perusahaan penjamin tidak ikut tersimpan | `BUG` | Backend | High | SUDAH-VERIFIKASI; dampak hilir DUGAAN | `FIX-DOK-005-06` |
| `ISS-DOK-005-T4` | — | PT Telkom belum punya aturan tanggungan; setelah backend dibangun ulang semua tindakan "Tidak Di-cover" | `DATA` | Data master | Medium | SUDAH-VERIFIKASI (database) | `FIX-DOK-005-07` |
| `ISS-DOK-005-T5` | — | Lencana status pecah per huruf di kolom STATUS yang sempit | `BUG` | Frontend | Low | DARI-CAPTURE + SUDAH-VERIFIKASI | `FIX-DOK-005-03` |
| `ISS-DOK-005-T6` | — | Tombol Simpan Pesanan Tindakan mati selama keranjang kosong | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-DOK-005-08` |
| `ISS-DOK-005-T7` | — | Pesanan tindakan pasien asuransi tertahan "perlu persetujuan" tanpa layar persetujuan | `BUG` | Backend + Frontend | Blocker | SUDAH-VERIFIKASI (source + database) | `FIX-DOK-005-06`, `FIX-DOK-005-09` |

---

## 4. Rincian per Temuan

### Bukti bersama: proses backend yang berjalan belum memuat perbaikan penjamin perusahaan

Butir 1, 2, dan 4 bermuara pada fakta yang sama. Buktinya dicatat sekali di sini.

| Bukti | Nilai | Status bukti |
| --- | --- | --- |
| Cara bayar kunjungan pasien (`RegPatientEncounter.PaymentType`) | `3` = Penjamin Perusahaan | SUDAH-VERIFIKASI (database) |
| Sumber bayar kunjungan (`RegPatientEncounterGuarantor`) | `PaymentType = 3`, aktif, eligible, perusahaan `COMP-TELKOM` (PT Telkom Indonesia), kartu pasien terhubung, paket `PLAN-GOLD` | SUDAH-VERIFIKASI (database) |
| Kode yang mengenal cara bayar 3 | `EncounterInsuranceService.cs:84-199` — ditambahkan commit `671191eb` (06-10-2026 15.47.24 WIB); berkas disimpan 15.42.42 | SUDAH-VERIFIKASI (`git log`, waktu berkas) |
| Kode lama yang menolak cara bayar 3 | `EncounterInsuranceService.cs:79-84` pada SHA `155a9fcb`: selain Tunai, apa pun yang bukan Asuransi ditolak dengan *"Tipe pembayaran encounter tidak didukung."* | SUDAH-VERIFIKASI (`git show`) |
| Proses backend yang sedang berjalan | `dotnet run --no-build` dimulai 14.34.13; `bin/Debug/net9.0/QuilvianSystemBackend.exe` dimulai 14.34.14 | SUDAH-VERIFIKASI (daftar proses Windows) |
| Hasil build yang dijalankan | `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 06-10-2026 10.01.53 | SUDAH-VERIFIKASI |
| Waktu galat pelapor | `timestamp: 2026-10-06T15:45:17` — sesudah berkas disimpan, sebelum ada build baru | DARI-CAPTURE |

Arti sederhananya: kode perbaikan sudah ada di folder source, tetapi server yang melayani layar
masih menjalankan hasil kompilasi pagi hari. Opsi `--no-build` berarti tidak ada kompilasi ulang
dan tidak ada *hot reload* (pemuatan ulang kode otomatis), jadi perubahan source tidak ikut
berjalan sampai backend dibangun ulang dan dijalankan ulang.

Pada source terbaru, pesan *"Tipe pembayaran encounter tidak didukung."* hanya muncul bila cara
bayar di luar 1, 2, dan 3 (`EncounterInsuranceService.cs:201-206`). Karena kunjungan ini bernilai
3, pesan itu **tidak mungkin** berasal dari source terbaru — hanya dari build lama.

Endpoint yang terlibat:

**Health Services / Inpatient Management / Inpatient Ancillary Order**

| Method | Path | Hak akses | Kegunaan |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders/coverage-status?itemType=Procedure&itemIds=…` | `InpatientEpisode : Read`; harga hanya bila memegang `PatientProcedure : Create` | Status tanggungan dan perkiraan harga per tindakan di katalog |

**Health Services / Clinical Management / Patient Procedure**

| Method | Path | Hak akses | Kegunaan |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/patient-procedures/inpatient-orders` | `PatientProcedure : Create` | Menyimpan satu pesanan tindakan rawat inap |

---

### ISS-DOK-005-01 — Status tanggungan "belum tersedia" pada semua tindakan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend (build yang berjalan) |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source, database, dan daftar proses; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-DOK-005-01` |

**Apa yang terjadi.** Setiap baris katalog (33 tindakan) menampilkan lencana "Status tanggungan
belum tersedia". Dokter tidak dapat melihat apakah suatu tindakan ditanggung perusahaan atau
harus dibayar pasien sendiri.

**Kenapa terjadi.**

1. Layar meminta status tanggungan seluruh tindakan sekaligus lewat hook
   `useInpatientAncillaryCoverage` (`procedure-form-panel.jsx:96-104`) ke endpoint
   `coverage-status`.
2. Backend `InpAncillaryOrderAdapter.GetCoverageStatusAsync` memberi setiap item nilai awal
   "Tarif belum tersedia" dengan status tanggungan kosong (`InpAncillaryOrderAdapter.cs:49-53`),
   lalu memanggil `InsuranceCoverageService.ResolveProcedureAsync` (`:62`). Status tanggungan
   hanya diisi "Ditanggung" atau "Tidak Di-cover" bila hasilnya valid dan punya tarif (`:64-67`).
   Bila gagal, galatnya ditelan (`:75-79`) dan respons tetap 200.
3. `ResolveProcedureAsync` lebih dulu membaca cara bayar kunjungan
   (`InsuranceCoverageService.cs:75-81`). Pada build yang berjalan, langkah ini gagal untuk cara
   bayar 3 — lihat Bukti bersama.
4. Frontend menulis "Status tanggungan belum tersedia" setiap kali status tanggungan bukan
   benar/salah (`procedure-form-panel.jsx:311-315`).

```text
"Status tanggungan belum tersedia"            ← procedure-form-panel.jsx:311-315
  ← isCovered kosong pada respons 200          ← InpAncillaryOrderAdapter.cs:64-67, galat ditelan :75-79
    ← konteks cara bayar gagal                 ← InsuranceCoverageService.cs:75-81
      ← build 10.01 belum mengenal cara bayar 3 (perbaikan ISSUE-DOK-004 belum dibangun ulang)
```

**Apakah ini menyimpang dari desain?** `BUG`. `03-frontend-architecture.md:758` dan
`RWI-DEC-218` butir 3 meminta status "Ditanggung" atau "Tidak Di-cover" sesuai penjamin pasien.
Kunjungan dengan penjamin yang sah tidak dirancang untuk menampilkan "belum tersedia".

**Dampak nyata.** Dokter memesan tindakan tanpa mengetahui tanggungannya. Contoh: Kumbah Lambung
dipesan, lalu keluarga pasien terkejut di kasir karena biayanya ternyata tidak ditanggung.

**Rekomendasi.** Bangun ulang dan jalankan ulang backend (`FIX-DOK-005-01`); tidak ada source yang
perlu diubah untuk butir ini. Perlu diketahui sebelumnya: setelah itu lencana akan berbunyi
"Tidak Di-cover" untuk semua tindakan, karena aturan tanggungan PT Telkom masih kosong
(`ISS-DOK-005-T4`).

---

### ISS-DOK-005-02 — Tarif "belum tersedia" padahal tarif ada di database

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Backend (build yang berjalan) |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan database |
| **Perbaikan** | `FIX-DOK-005-01` |

**Apa yang terjadi.** Kolom TARIF katalog, kolom Tarif Satuan dan Subtotal pada keranjang, serta
Total perkiraan semuanya menulis "tarif belum tersedia". Footer bahkan menyebut "1 pemeriksaan
tanpa tarif" untuk Kumbah Lambung.

**Jawaban langsung: harga ada di database.** Hasil `SELECT` baca-saja:

| Kode | Tindakan | Tarif normal | Berlaku untuk |
| --- | --- | ---: | --- |
| `PR-RSMMC-00012` | Kumbah Lambung (Gastric Lavage) | Rp 175.000 | Semua kelas, unit, dan klinik |
| `PR-RSMMC-00027` | Ekstraksi Kuku | Rp 250.000 | Semua kelas, unit, dan klinik |
| `PR-RSMMC-00028` | Injeksi Intravena (IV Bolus/Push) | Rp 30.000 | Semua kelas, unit, dan klinik |
| `PR-RSMMC-00029` | Injeksi Intramuskular (IM Injection) | Rp 30.000 | Semua kelas, unit, dan klinik |

Secara keseluruhan: 36 tindakan aktif, **32 punya tarif aktif** (Rp 25.000 sampai Rp 500.000;
semuanya berlaku untuk semua kelas, unit, dan klinik; tidak ada yang kedaluwarsa). Hanya
**4 tindakan yang benar-benar tanpa tarif**: `PR-HMD-001`, `PR-RSMMC-00002`, `PR-RSMMC-00003`,
dan `PR-RSMMC-00004`.

**Kenapa terjadi.** Sebabnya sama dengan `ISS-DOK-005-01`. Pencarian tarif
(`InsuranceCoverageService.FindProcedureTariffAsync`, `:493-523`) tidak pernah dijalankan karena
`ResolveProcedureAsync` sudah berhenti di pemeriksaan cara bayar (`:75-81`). Adapter lalu
mengirim `PriceStatus = "NOT_ESTIMABLE"` (`InpAncillaryOrderAdapter.cs:51-53`), dan frontend
menerjemahkannya menjadi "tarif belum tersedia" (`inpatient-coverage-utils.js:23-25`).

Pelapor wajar menduga database kosong, karena kontrak memakai satu tulisan untuk dua keadaan yang
berbeda: `NOT_ESTIMABLE` berarti "tarif tidak ada di master" **atau** "resolver gagal"
(`02-backend-architecture.md:1787`). Layar tidak dapat membedakan keduanya.

Label "KELAS II" di bawah tarif hanyalah nama kelas episode (`procedure-form-panel.jsx:298-302`),
bukan tanda bahwa tarifnya khusus kelas II.

**Apakah ini menyimpang dari desain?** `BUG` (sebab sama dengan butir 1). Menurut `RWI-DEC-218`
butir 4, "tarif belum tersedia" seharusnya hanya muncul untuk keempat tindakan tanpa tarif.

**Dampak nyata.** Dokter dan keluarga pasien tidak mendapat perkiraan biaya, padahal datanya ada.
Pelapor terdorong memeriksa database untuk masalah yang sebenarnya ada di server.

**Rekomendasi.** `FIX-DOK-005-01`. Sesudahnya Kumbah Lambung tampil
"Rp 175.000 (perkiraan — tagihan final di kasir)". Pemisahan tulisan "tarif tidak ada" dari
"server gagal menghitung" dicatat sebagai usulan pada bagian 8.

---

### ISS-DOK-005-03 — Tombol aksi tampil sebagai kotak polos tanpa tanda

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` (ikon hilang); `DESIGN_CHANGE` bila tombol diberi tulisan |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-DOK-005-03` (ikon tampil), `FIX-DOK-005-04` (tulisan, menunggu `K-01`) |

**Apa yang terjadi.** Tombol yang dikeluhkan pelapor adalah tombol **Pilih**: kotak biru
tinggi-sempit di kolom AKSI tabel Daftar Tindakan Medis, yang memindahkan tindakan ke Form Tindakan
Medis di sebelah kanan. Pelapor menegaskannya pada lampiran 3: "ini adalah aksi pilih — nama ya gak
ada". Tombol itu tidak menampilkan tanda maupun nama apa pun. Tombol **Hapus** di keranjang
"Tindakan yang Dipilih" — kotak merah muda — mengalami cacat yang sama walau tidak dikeluhkan.
Fungsi keduanya baru diketahui bila kursor diarahkan ke tombol (tooltip "Pilih tindakan untuk
dikonfigurasi" dan "Hapus tindakan").

**Kenapa terjadi.** Tombol itu sebenarnya berisi ikon "+" dan ikon tempat sampah, tetapi ikonnya
terjepit menjadi selebar 0 piksel:

1. Tombol pilih dibuat dengan `BaseButton` tanpa prop `size`, sehingga memakai ukuran bawaan `md`
   (`procedure-form-panel.jsx:319-327`). Isinya hanya `<RiAddLine size={18} />`.
2. Ukuran `md` memberi jarak dalam (*padding*) 16 px kiri dan 16 px kanan, serta tinggi minimum
   42 px (`base-button.module.css:75-79`).
3. Gaya lokal `.addButton` menetapkan lebar 32 px tetapi tidak menimpa padding
   (`physician-procedure.module.css:225-237`). Karena seluruh aplikasi memakai
   `box-sizing: border-box` (`globals.css:127`), ruang untuk isi = 32 − 16 − 16 = **0 px**.
   Tinggi tetap 42 px, sehingga kotak tampil tinggi-sempit seperti di screenshot.
4. Aturan global `img, svg, video, canvas { max-width: 100% }` (`globals.css:216-221`) memaksa
   ikon selebar ruang isinya, yaitu 0 px. Ikon hilang tanpa galat apa pun.
5. Tombol hapus di keranjang mengalami hal yang sama: lebar 30 px lewat gaya inline, padding
   16 + 16 px, ruang isi 0 px, ikon `RiDeleteBinLine` hilang (`procedure-form-panel.jsx:762-781`).
6. Kedua tombol hanya punya `title`, tanpa tulisan dan tanpa `aria-label`, sehingga pembaca layar
   pun tidak mengetahui namanya.

Akar polanya: gaya lokal mengatur lebar tombol ikon tanpa menimpa padding bawaan `BaseButton`.

**Apakah ini menyimpang dari desain?** Ikon yang hilang adalah `BUG`. Rancangan V1
(`roadmap/rencana-kerja/tindakan/tindakan.md:257-274`) menggambar kolom AKSI sebagai `[ + ]`, dan
teks keadaan kosong di layar sendiri meminta "klik tombol [ + ]" (`procedure-form-panel.jsx:376-380`).
**Tulisan nama** pada tombol memang tidak pernah dirancang; menambahkannya adalah
`DESIGN_CHANGE` kecil yang perlu persetujuan (`K-01`).

**Dampak nyata.** Dokter yang baru memakai layar tidak tahu bahwa kotak biru itu untuk memilih
tindakan. Kotak merah muda di keranjang tidak terlihat sebagai tombol hapus, sehingga dokter
memakai "Reset Semua" dan kehilangan seluruh pilihan hanya untuk membuang satu tindakan.

**Radius.** Kelas `.addButton` yang sama dipakai ruang kerja perawat
(`nursing-procedure-order-panel.jsx:289-297`, pola `BaseButton` identik), sehingga panel pesan
tindakan perawat hampir pasti menampilkan kotak kosong yang sama. Pemilik panel itu adalah
sub-modul perawat rawat inap.

**Rekomendasi.** Tampilkan kembali ikon dan beri `aria-label` (`FIX-DOK-005-03`). Bila `K-01`
disetujui, tambahkan tulisan pendek "Pilih" dan "Hapus" di samping ikon (`FIX-DOK-005-04`).

---

### ISS-DOK-005-04 — Simpan Pesanan Tindakan ditolak 400 "Tipe pembayaran encounter tidak didukung."

| | |
| --- | --- |
| **No. laporan** | 4 |
| **Jenis** | `BUG` |
| **Area** | Backend (build yang berjalan) |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source, database, dan daftar proses; respons DARI-CAPTURE |
| **Perbaikan** | `FIX-DOK-005-01` |

**Apa yang terjadi.** Dokter memilih Kumbah Lambung, mengisi indikasi dan disposisi, lalu
menekan **Simpan Pesanan Tindakan**. Server menjawab 400 dengan pesan
*"Tipe pembayaran encounter tidak didukung."* Pesanan tidak tersimpan — database tidak memuat satu
pun baris `TrxPatientProcedure` untuk kunjungan ini.

**Isian pelapor tidak salah.** Payload memuat episode, tindakan, dokter, jumlah 1, indikasi, dan
disposisi dengan benar.

**Kenapa terjadi.**

1. Endpoint `inpatient-orders` diteruskan ke
   `PatientProcedureOrderService.CreateInpatientOrderAsync`
   (`PatientProcedureController.cs:537-563`).
2. Pemeriksaan episode, penugasan dokter, dan master tindakan lolos — pesan galat masing-masing
   berbeda dan tidak muncul.
3. Sebelum menyimpan, service menghitung harga lewat `ResolveProcedureAsync`; bila hasilnya tidak
   valid, service langsung menolak dengan pesan dari resolver
   (`PatientProcedureOrderService.cs:198-209`).
4. Resolver gagal di pemeriksaan cara bayar karena build lama (Bukti bersama), dan pesannya
   diteruskan apa adanya ke layar.

**Apakah ini menyimpang dari desain?** `BUG`. "Penjamin Perusahaan" adalah cara bayar sah
(`EncounterPaymentType.CompanyGuarantor = 3`, ditambahkan `RWI-ENC-PAYER-001` lewat `BE-RWI-035`).
Selain itu, menolak pesanan karena harga tidak dapat dihitung juga bertentangan dengan
`RWI-DEC-218` butir 4 — lihat `ISS-DOK-005-T2`.

**Dampak nyata.** Dokter tidak dapat memesan tindakan apa pun untuk pasien penjamin perusahaan.
Database dev memuat 3 kunjungan dengan cara bayar ini; salah satunya episode rawat inap aktif
pasien pada laporan.

**Rekomendasi.** `FIX-DOK-005-01`. Sesudahnya pesanan Kumbah Lambung tersimpan dengan tarif
Rp 175.000 dan status tanggungan `NotCovered` (lihat `ISS-DOK-005-T4`). Catatan: empat tindakan
tanpa tarif tetap akan ditolak dengan pesan lain sampai `FIX-DOK-005-05` dikerjakan.

---

## 5. Tanya-Jawab Pelapor

> **T:** Status tanggungan belum tersedia — proses ini terjadi kenapa?
>
> **J:** Karena server backend yang sedang berjalan adalah versi lama yang belum mengenal cara
> bayar "Penjamin Perusahaan". Status tanggungan dihitung server per tindakan, dan langkah
> pertamanya membaca cara bayar pasien. Untuk pasien PT Telkom (cara bayar 3), versi lama itu
> berhenti dengan galat sebelum sempat menjawab "Ditanggung" atau "Tidak Di-cover", sehingga layar
> menulis "belum tersedia". Kode perbaikannya sudah ada sejak 15.42 (ISSUE-DOK-004), tetapi
> backend dijalankan dengan `dotnet run --no-build` sejak 14.34 memakai hasil build pukul 10.01,
> jadi perbaikan itu belum ikut berjalan. Solusinya: hentikan backend, build ulang, jalankan lagi.

> **T:** Kenapa tarif belum tersedia — apakah memang di database tidak ada harga?
>
> **J:** Tidak, harganya ada. Kumbah Lambung (`PR-RSMMC-00012`) tercatat Rp 175.000 dan berlaku
> untuk semua kelas; 32 dari 36 tindakan aktif punya tarif. Tulisan "tarif belum tersedia" muncul
> karena server gagal di langkah membaca cara bayar sebelum sempat mencari tarif, sedangkan layar
> memakai tulisan yang sama untuk "tarif tidak ada" dan "server gagal menghitung". Yang memang
> tanpa tarif hanya empat tindakan: `PR-HMD-001`, `PR-RSMMC-00002`, `PR-RSMMC-00003`, dan
> `PR-RSMMC-00004`.

> **T:** Button aksi kenapa tidak ada nama, warnanya biru saja? (ditegaskan: "ini adalah aksi
> pilih — nama ya gak ada")
>
> **J:** Benar, tombol **Pilih** itu memang tidak punya nama. Di kode, isinya hanya ikon "+" dan
> tooltip "Pilih tindakan untuk dikonfigurasi" yang baru muncul saat kursor diarahkan
> (`procedure-form-panel.jsx:319-327`); tulisan nama tidak pernah dirancang (rancangan V1 hanya
> `[ + ]`). Ikon "+"-nya pun tidak terlihat karena terjepit menjadi selebar nol piksel oleh jarak
> dalam tombol, sehingga yang tersisa hanya kotak biru polos. Rekomendasi kami: tombol ini menjadi
> `[ + Pilih ]`. Tulisan "Hapus" **bukan** untuk tombol ini, melainkan untuk tombol lain di keranjang
> "Tindakan yang Dipilih" (kotak merah muda di bagian bawah layar), yang cacatnya sama. Bagian
> tulisan menunggu persetujuan pemilik (`K-01`).

> **T:** Kenapa bad request ketika saya simpan pesanan tindakan?
>
> **J:** Sebabnya sama dengan butir 1. Sebelum menyimpan, server menghitung tarif tindakan;
> perhitungan itu gagal di cara bayar penjamin perusahaan, lalu server menolak seluruh pesanan.
> Isian Anda tidak salah. Ada masalah kedua yang ikut terungkap: server menolak pesanan **setiap
> kali** harga tidak dapat dihitung, padahal keputusan `RWI-DEC-218` butir 4 menyatakan pesanan
> tetap boleh dan harga hanya informasi (`ISS-DOK-005-T2`).

> **T:** Coba lakukan analisis dan cek database.
>
> **J:** Sudah. Pemeriksaan memakai `SELECT` baca-saja di dalam transaksi `READ ONLY` yang
> dibatalkan di akhir, pada database pribadi pemilik `QuilvianNewDevHamzah`. Tidak ada data yang
> diubah. Ringkasannya:
>
> | Data | Nilai | Arti |
> | --- | --- | --- |
> | Cara bayar kunjungan | 3 (Penjamin Perusahaan) pada kunjungan dan sumber bayar | Konsisten |
> | Perusahaan `COMP-TELKOM` | Aktif; kontrak 01-01-2026 s.d. 31-12-2026; memakai buku tarif perusahaan; tindakan perlu persetujuan | Kontrak berlaku pada tanggal pelayanan |
> | Kartu penjamin pasien | Aktif, eligible, berlaku 2024–2028, paket `PLAN-GOLD`, Grade 4 - Executive | Sah |
> | Aturan tanggungan PT Telkom | **0 baris** | Belum dikonfigurasi (`ISS-DOK-005-T4`) |
> | Tarif tindakan | 32 dari 36 tindakan aktif; Kumbah Lambung Rp 175.000 | Tarif ada |
> | Pesanan tindakan kunjungan ini | 0 baris | Penolakan 400 tidak menyimpan apa pun |
> | Sebaran cara bayar seluruh kunjungan | Tunai 172, Asuransi 17, Penjamin Perusahaan 3 | Cara bayar 3 masih jarang, sehingga celahnya baru terlihat sekarang |
>
> Dengan data ini, setelah backend dibangun ulang seluruh pemeriksaan cara bayar akan lolos.

---

## 6. Temuan Tambahan

### ISS-DOK-005-T1 — Kotak "Galat Tindakan" tidak menampilkan isi pesan

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Terlihat pada screenshot 1 tetapi tidak dikeluhkan; inilah sebab pelapor harus membuka DevTools untuk mengetahui alasan penolakan |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-DOK-005-02` |

**Apa yang terjadi.** Setelah Simpan ditolak, muncul kotak merah berjudul "Galat Tindakan" tanpa
penjelasan apa pun dan tanpa tombol tutup.

**Kenapa terjadi.** Tab Tindakan mengirim pesan galat lewat prop `message` dan penutup lewat
`onDismiss` (`inpatient-procedure-tab.jsx:108-116`). Komponen `ClinicalSafetyAlert` hanya mengenal
`title`, `description`, `items`, dan `action` (`ClinicalSafetyAlert.jsx:39-47`); isi penjelasan
dirender dari `description` (`:66-69`). Prop `message` dan `onDismiss` diabaikan diam-diam.

**Radius.** Pencarian kode menemukan **24 berkas** yang mengirim `message` ke `ClinicalSafetyAlert`:
ruang kerja dokter rawat inap (Resep, Penunjang, Resume, Tindakan), ruang kerja perawat rawat inap
(Obat, Tindakan), antrean dokter rawat jalan (Tindakan, Penunjang), dan Hemodialisis. Di semua
layar itu, isi pesan galat tidak pernah tampil.

**Apakah ini menyimpang dari desain?** `BUG`. `ClinicalSafetyAlert` adalah base component sub-modul
ini untuk "warning data gagal" (`skema-tampilan-dokter-rawat-inap.md` bagian 4.2 butir 1); pesan
yang tidak terbaca menggagalkan tujuannya.

**Dampak nyata.** Petugas tidak tahu kenapa aksinya ditolak dan harus meminta bantuan tim IT.

**Rekomendasi.** Komponen menerima `message` sebagai nama lain `description` dan menampilkan tombol
tutup bila `onDismiss` dikirim. Satu berkas, memperbaiki 24 pemanggil sekaligus.

---

### ISS-DOK-005-T2 — Pesanan tindakan ditolak bila harga tidak dapat dihitung

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Inilah yang mengubah gangguan harga pada butir 4 menjadi Blocker; tetap berlaku setelah `FIX-DOK-005-01` |
| **Jenis** | `BUG` (menyimpang dari keputusan yang disahkan) |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source dan dokumen keputusan |
| **Perbaikan** | `FIX-DOK-005-05` |

**Apa yang terjadi.** Bila server tidak dapat menghitung harga — tarif tidak ada di master, atau
data penjamin bermasalah — seluruh pesanan ditolak 400.

**Kenapa terjadi.** `PatientProcedureOrderService.cs:205-209` menolak pesanan setiap kali
`pricing.IsValid` bernilai salah. Resolver mengembalikan tidak valid antara lain bila tarif tidak
ditemukan, dengan pesan *"Tarif rumah sakit untuk tindakan belum dikonfigurasi."*
(`InsuranceCoverageService.cs:88-93`).

**Apakah ini menyimpang dari desain?** `BUG`. `RWI-DEC-218` butir 1 memasukkan "order tindakan" ke
layar pemesanan berharga, dan butir 4 menyatakan: *"Bila tarif tidak dapat diresolusi (tarif
belum ada di master, atau Billing tidak dapat dibaca), layar menampilkan 'tarif belum tersedia'
dan pemesanan tetap boleh — harga adalah informasi, bukan syarat."*
(`00-interview-decisions.md:2630`). `RWI-DEC-220` butir 5 mengulanginya (`:2632`). Layar katalog
sendiri menawarkan tindakan tanpa tarif dengan tombol pilih yang aktif.

**Dampak nyata.** Empat tindakan tanpa tarif (`PR-HMD-001`, `PR-RSMMC-00002`, `PR-RSMMC-00003`,
`PR-RSMMC-00004`) tidak dapat dipesan oleh siapa pun, dan setiap gangguan data penjamin menahan
instruksi klinis dokter.

**Rekomendasi.** Simpan pesanan tanpa tarif dengan penanda tanggungan `ConfigurationMissing` —
nilai yang sudah dipakai resolver (`InsuranceCoverageService.cs:91-92`) — beserta alasannya. Saat
tindakan dikerjakan, Billing mengambil tarif sendiri dari master; bila masih kosong, tagihan masuk
antrean rekonsiliasi, bukan Rp 0 (`BillingSourceTariffResolver.cs:63-84`, `:405-452`). Keputusannya
sudah ada (`RWI-DEC-218` butir 4), sehingga `K-02` ditutup tanpa keputusan baru.

---

### ISS-DOK-005-T3 — Syarat persetujuan tindakan milik perusahaan penjamin tidak ikut tersimpan

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri apa yang akan tersimpan untuk butir 4 setelah `FIX-DOK-005-01` |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source dan database; dampak pada proses penjaminan DUGAAN |
| **Perbaikan** | `FIX-DOK-005-06` (menunggu `K-03`) |

**Apa yang terjadi.** Master PT Telkom menandai "tindakan perlu persetujuan"
(`MstCompanyGuarantor.IsNeedApprovalForProcedure = true`). Setelah backend dibangun ulang, pesanan
tindakan untuk pasien PT Telkom akan tersimpan dengan `IsNeedApproval = false`.

**Kenapa terjadi.**

1. Konteks cara bayar sudah membawa tanda itu: `IsNeedApprovalForProcedure =
   company.IsNeedApprovalForProcedure` (`EncounterInsuranceService.cs:194`).
2. Jalur Asuransi memakainya (`InsuranceCoverageService.cs:346-352`). Jalur Penjamin Perusahaan
   tidak: hasilnya diambil dari `CompanyGuarantorCoverageService` (`InsuranceCoverageService.cs:190`),
   yang hanya membaca tanda persetujuan per aturan (`CompanyGuarantorCoverageService.cs:362`).
   Bila tidak ada aturan yang cocok, hasilnya tidak pernah meminta persetujuan (`:294-321`).
3. Pesanan menyimpan `IsNeedApproval = pricing.IsNeedApproval || procedure.IsNeedApproval`
   (`PatientProcedureOrderService.cs:261`).

**Apakah ini menyimpang dari desain?** `BUG` menurut data master yang sudah ada. Namun apakah tanda
tingkat perusahaan berlaku untuk **setiap** tindakan adalah kebijakan penjaminan, sehingga perlu
dikonfirmasi pemilik (`K-03`).

**Dampak nyata (DUGAAN).** Tindakan yang menurut kontrak perlu persetujuan perusahaan dapat
dikerjakan dan ditagihkan tanpa persetujuan, lalu klaimnya ditolak perusahaan. **Cara membuktikan:**
setelah `FIX-DOK-005-01`, simpan satu pesanan uji lalu baca `TrxPatientProcedure.IsNeedApproval`
(query pada plan repair `FIX-DOK-005-01`).

**Rekomendasi.** Jangan menyalin cara jalur Asuransi. Di sistem ini `IsNeedApproval` bukan sekadar
catatan: tindakan yang bertanda itu **tidak dapat dikerjakan** sebelum disetujui, dan tidak ada
layar untuk menyetujuinya (`ISS-DOK-005-T7`). Syarat persetujuan dari penjamin sebaiknya dicatat
sebagai status tanggungan "Perlu persetujuan penjamin" yang ditindaklanjuti bagian penjaminan,
tanpa menahan pelaksanaan klinis (`K-03` pilihan C).

---

### ISS-DOK-005-T4 — PT Telkom belum punya aturan tanggungan

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Menentukan apa yang dilihat pelapor setelah `FIX-DOK-005-01`; tanpa diberi tahu, "Tidak Di-cover" akan dikira bug baru |
| **Jenis** | `DATA` |
| **Area** | Data master penjamin perusahaan (pemilik: Billing — kontrak `BE-BKC-044`) |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di database dan source |
| **Perbaikan** | `FIX-DOK-005-07` (menunggu `K-04`) |

**Apa yang terjadi.** Tabel `MstCompanyGuarantorCoverageRule` tidak memuat satu pun aturan untuk
`COMP-TELKOM`. Akibatnya, setelah backend dibangun ulang, setiap tindakan untuk pasien PT Telkom
dihitung "tidak ada aturan yang cocok": `IsCovered = false`, `CoverageStatus = "NotCovered"`,
seluruh biaya dibebankan ke pasien, dengan catatan *"Item tidak memiliki aturan tanggungan yang
cocok pada perusahaan penjamin ini."* (`CompanyGuarantorCoverageService.cs:294-321`).

Contoh hasil di layar: Kumbah Lambung → "Tidak Di-cover · Rp 175.000 (perkiraan — tagihan final di
kasir)".

**Catatan tambahan untuk pemilik Billing.** Master PT Telkom bertanda "memakai buku tarif
perusahaan" (`IsUsingCompanyTariffBook = true`) dan "tidak memakai tarif rumah sakit"
(`IsUsingHospitalTariff = false`), tetapi resolver selalu memakai tarif rumah sakit
(`NormalPrice`). Pencarian kode tidak menemukan pembacaan buku tarif perusahaan di luar layar master
dan pemetaan konteks (`EncounterInsuranceService.cs:190`). Perkiraan harga untuk PT Telkom karena
itu mungkin tidak sama dengan harga kontrak.

**Apakah ini menyimpang dari desain?** `DATA`. Source bekerja sesuai rancangannya; isi master yang
belum lengkap.

**Rekomendasi.** Bagian penjaminan mengisi aturan tanggungan PT Telkom sesuai kontrak lewat layar
master *Aturan Tanggungan Penjamin Perusahaan* (`/health-services/master-data/company-guarantor-coverage-rules`).
Isi kontraknya tidak boleh dikarang agent (`K-04`).

---

### ISS-DOK-005-T5 — Lencana status pecah per huruf

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Terlihat jelas pada ketiga lampiran ("Stat / us / Tan / ggu / nga / n …") |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | Gejala DARI-CAPTURE; lebar kolom SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-DOK-005-03` |

**Apa yang terjadi.** Lencana "Status tanggungan belum tersedia" dipecah per tiga-empat huruf
sehingga sulit dibaca dan membuat baris katalog sangat tinggi.

**Kenapa terjadi.** Kolom STATUS hanya 15% dari tabel katalog yang sudah selebar setengah layar
(`procedure-form-panel.jsx:265`), sedangkan labelnya 32 karakter (`:311-315`). Aturan pemenggalan
kata di dalam lencana yang membuatnya pecah per huruf berstatus DUGAAN — **cara membuktikan:**
periksa gaya hasil akhir lencana dengan *Inspect Element*.

**Rekomendasi.** Diperbaiki bersama tombol aksi pada berkas yang sama (`FIX-DOK-005-03`).

---

### ISS-DOK-005-T6 — Tombol Simpan Pesanan Tindakan mati selama keranjang kosong

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Hasil daftar periksa "tombol aksi mati"; bertentangan dengan arahan pemilik 06-10-2026 untuk tombol simpan |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-DOK-005-08` |

**Apa yang terjadi.** Selama keranjang kosong, tombol Simpan Pesanan Tindakan tampil pudar dan
tidak dapat ditekan (`procedure-form-panel.jsx:822-824`).

**Kenapa dicantumkan sebagai pelanggaran.** Pada 06-10-2026 pemilik menetapkan (ISSUE-EPS-003,
tombol "Simpan & Lanjut ke Pembayaran") bahwa tombol simpan tidak dimatikan karena isian belum
lengkap: saat diklik, sistem memvalidasi lalu mengarahkan pengguna ke bagian yang kurang. Tombol
hanya boleh terkunci selama permintaan berjalan. Keranjang memang menulis "Belum ada tindakan yang
dipilih", sehingga keparahannya rendah.

**Rekomendasi.** Tombol selalu dapat ditekan; bila keranjang kosong, tampilkan pesan "Pilih minimal
satu tindakan dari Daftar Tindakan Medis" dan arahkan fokus ke kolom pencarian katalog.

---

### ISS-DOK-005-T7 — Pesanan tindakan pasien asuransi tertahan menunggu persetujuan yang tidak punya layar

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan |
| **Kenapa dicantumkan** | Ditemukan saat menimbang `K-03`: menerapkan tanda persetujuan perusahaan dengan cara jalur Asuransi akan menulari pasien perusahaan dengan masalah yang sama |
| **Jenis** | `BUG` |
| **Area** | Backend + Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source dan database |
| **Perbaikan** | `FIX-DOK-005-06`, `FIX-DOK-005-09` (menunggu `K-03`) |

**Apa yang terjadi.** Database dev memuat 11 pesanan tindakan. Lima di antaranya — semuanya pasien
rawat inap dengan cara bayar Asuransi, dipesan dokter antara 22-09-2026 dan 02-10-2026 (empat
`PR-DOK-001`, satu `PR-RSMMC-00008`) — bertanda "perlu persetujuan", belum disetujui, dan belum
pernah dikerjakan.

**Kenapa terjadi.**

1. Jalur Asuransi menandai pesanan perlu persetujuan bila penyedia asuransi menandai tindakan
   perlu persetujuan (`InsuranceCoverageService.cs:346-352`). Kelima penyedia asuransi aktif di
   database bertanda itu, sehingga **setiap** pesanan tindakan pasien asuransi perlu persetujuan.
   Kelima perusahaan penjamin juga bertanda sama.
2. Pelaksanaan tindakan menolak pesanan yang perlu persetujuan dan belum disetujui dengan pesan
   *"Tindakan membutuhkan approval sebelum dieksekusi."* (`PatientProcedureExecutionService.cs:80-82`;
   jalur Kamar Operasi `:219-221`).
3. Endpoint persetujuan ada — `PATCH /api/v1/health-services/clinical-management/patient-procedures/{id}/approve`,
   hak `PatientProcedure : Approve` (`PatientProcedureController.cs:1208-1265`) — tetapi **tidak satu
   pun layar frontend memanggilnya**, dan blueprint rawat inap tidak merancang layar persetujuan
   tindakan.
4. Akibatnya pesanan berhenti di status Ordered tanpa batas waktu, kecuali seseorang memanggil
   endpoint itu langsung lewat Swagger.

Rawat jalan memakai rumus yang sama (`PatientProcedureController.cs:799`), sehingga pasien asuransi
rawat jalan berisiko mengalami hal serupa.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan rawat inap yang meminta
pelaksanaan tindakan ditahan oleh persetujuan penjamin. `RWI-DEC-218` justru menegaskan bahwa urusan
harga dan tanggungan tidak menahan pemesanan.

**Dampak nyata.** Contoh: dokter memesan Kumbah Lambung Cito untuk pasien keracunan yang
berasuransi. Perawat menekan Kerjakan, server menolak, dan tidak ada orang yang dapat menyetujui
dari layar mana pun. Pertolongan tertunda karena urusan administrasi penjamin.

**Rekomendasi.** Pisahkan persetujuan penjamin dari penahan pelaksanaan. Persetujuan penjamin
dicatat sebagai status tanggungan "Perlu persetujuan penjamin" untuk ditindaklanjuti bagian
penjaminan; penahan pelaksanaan hanya untuk persetujuan internal dari master tindakan dan tidak
pernah untuk tindakan Cito (`K-03` pilihan C, `FIX-DOK-005-06`). Lima pesanan yang sudah tertahan
dibuka lewat koreksi data yang disetujui pemilik (`FIX-DOK-005-09`).

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Setelah backend dibangun ulang dan dijalankan ulang, apakah tab **Resep** untuk pasien yang sama sudah memuat daftar obat? | ISSUE-DOK-004 berstatus SELESAI, tetapi perbaikannya belum pernah berjalan di server. Jawaban ini membuktikan perbaikan itu ikut aktif | Status SELESAI pada ISSUE-DOK-004 tetap belum terbukti |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bentuk tombol aksi katalog dan keranjang | (A) Ikon + tulisan: tombol Pilih di katalog Daftar Tindakan Medis (yang dikeluhkan) → `[ + Pilih ]`; tombol Hapus di keranjang Tindakan yang Dipilih → `[ 🗑 Hapus ]`<br>(B) Ikon saja sesuai V1, ditambah `aria-label` | **Diputuskan pemilik 06-10-2026: A** ("tombol pilih dan hapus"). Alasan rekomendasi: (1) Pelapor sendiri tidak mengenali tombolnya. (2) Tooltip `title` tidak muncul pada layar sentuh, padahal skema tampilan bagian 18 mendukung tablet. (3) Tombol "Hapus" bervarian bahaya lebih jelas daripada kotak merah muda polos. Terapkan juga di panel perawat agar kedua peran sama. Biaya: kolom AKSI dilebarkan sekitar 4% | `FIX-DOK-005-04` |
| K-02 | Pesanan tindakan yang harganya tidak dapat dihitung | (A) Tetap disimpan dengan tanggungan `ConfigurationMissing`; Billing mengambil tarif saat tindakan dikerjakan<br>(B) Tetap ditolak — artinya `RWI-DEC-218` butir 4 dicabut untuk order tindakan | **Ditutup tanpa keputusan baru (06-10-2026)** — sudah diputuskan `RWI-DEC-218` butir 4 dan `RWI-DEC-220` butir 5, sehingga seharusnya tidak ditanyakan. Bukti kesiapan Billing: saat tindakan dikerjakan, Billing mencari tarif sendiri dari master (`BillingSourceTariffResolver.cs:63-84`, `:405-452`); bila masih kosong, tagihan masuk antrean rekonsiliasi dengan pesan "Lengkapi tarif, lalu kirim ulang" — tidak pernah Rp 0. Database sudah memuat 2 pesanan `PR-HMD-001` tanpa tarif yang tersimpan dan dikerjakan. Pemilik Billing cukup diberi tahu | `FIX-DOK-005-05` |
| K-03 | Bagaimana tanda "perlu persetujuan" milik penjamin (perusahaan maupun asuransi) diperlakukan pada pesanan tindakan? | (A) Seperti jalur Asuransi sekarang: menjadi penahan pelaksanaan<br>(B) Diabaikan<br>(C) Dicatat sebagai status tanggungan "Perlu persetujuan penjamin" tanpa menahan pelaksanaan; penahan hanya untuk persetujuan internal master tindakan dan tidak pernah untuk Cito | **C** — berubah dari rekomendasi awal A. Pilihan A membuat setiap tindakan pasien perusahaan tertahan seperti 5 pesanan asuransi yang sudah macet (`ISS-DOK-005-T7`), karena tidak ada layar persetujuan. Bila rumah sakit memang membutuhkan persetujuan sebelum tindakan elektif, itu modul baru (layar penjaminan) lewat `grill-me`, bukan bagian perbaikan ini | `FIX-DOK-005-06`, `FIX-DOK-005-09` |
| K-04 | Tanggungan PT Telkom paket `PLAN-GOLD`, dan buku tarif yang berlaku | (A) Diisi sesuai kontrak<br>(B) Dibiarkan kosong — semua tindakan menjadi beban pasien | **A, dipecah tiga.** (1) Produksi: bagian penjaminan mengisi aturan dari kontrak nyata sebelum perusahaan dipakai; satu aturan `ServiceCategory` → kategori tarif "Procedure" sudah mencakup ke-32 tarif tindakan. (2) Database dev: kelima perusahaan contoh (ASTRA, BCA, PLN, TELKOM, UNILEVER) sama-sama tanpa aturan dan tanpa seeder; isi satu set data demo berawalan `DEMO-` yang disetujui pemilik, jangan disamarkan sebagai kontrak. (3) Buku tarif: kelima perusahaan bertanda "memakai buku tarif perusahaan", padahal fiturnya belum ada (tidak ada tabelnya, aturan tidak punya kolom harga, resolver dan Billing selalu memakai tarif rumah sakit); ubah tandanya menjadi "memakai tarif rumah sakit" sampai fitur itu dirancang | `FIX-DOK-005-07` |

Keputusan yang sudah diambil sebelumnya dan dipakai tanpa ditanyakan ulang:

| Keputusan | Sumber | Dipakai pada |
| --- | --- | --- |
| Harga adalah informasi, bukan syarat pemesanan | `RWI-DEC-218` butir 4, `RWI-DEC-220` butir 5 | `ISS-DOK-005-T2` |
| Tombol simpan tidak dimatikan karena isian belum lengkap | Arahan pemilik 06-10-2026 pada ISSUE-EPS-003 | `ISS-DOK-005-T6` |

---

## 8. Catatan Pola

1. **Perbaikan di source dianggap selesai tanpa build dan restart.** ISSUE-DOK-004 berstatus
   `SELESAI` dan register-nya bertanda `✅` tanpa laporan task dan tanpa bukti server menjalankan
   kodenya. Radius dampaknya menyebut `PatientProcedureController`, tetapi verifikasinya hanya
   untuk Resep. **Usulan:** setiap perbaikan backend wajib punya langkah "build, jalankan ulang,
   panggil endpoint" sebelum diberi `✅`, dan `✅` hanya dengan laporan task
   (`status-task-roadmap.md` bagian 2.1).
2. **Salah nama prop pada base component klinis.** `message` lawan `description` pada
   `ClinicalSafetyAlert` (24 berkas) mengulang pola salah-prop yang sudah pernah terjadi pada
   `ConfirmModal`, `ClinicalActionGuard`, dan `Badge`. Prop yang tidak dikenal diabaikan diam-diam.
   **Usulan:** base component klinis memberi peringatan di mode development untuk prop yang tidak
   dikenal, atau mendukung nama lazim sebagai alias.
3. **Lebar tombol ikon diatur CSS lokal tanpa menimpa padding `BaseButton`.** Ikon hilang tanpa
   galat karena aturan global `svg { max-width: 100% }`. Pola yang sama ada di panel perawat.
   **Usulan:** `BaseButton` mendapat bentuk khusus tombol ikon (misalnya prop `iconOnly`) sehingga
   tidak ada lagi lebar tetap di CSS lokal.
4. **Satu tulisan untuk dua keadaan.** "tarif belum tersedia" dipakai untuk "tarif tidak ada di
   master" dan "server gagal menghitung", sehingga pelapor diarahkan memeriksa database untuk
   masalah server. **Usulan:** kontrak `coverage-status` membedakan alasan `NOT_ESTIMABLE`
   (misalnya `PriceReason = TARIFF_MISSING | RESOLVER_FAILED`) dengan tulisan layar yang berbeda.
   Ini perubahan kontrak, sehingga jalurnya lewat dokumen blueprint, bukan perbaikan langsung.
5. **Galat resolver ditelan menjadi respons 200.** `InpAncillaryOrderAdapter.cs:75-79` hanya
   mencatat log peringatan. Itu sesuai desain ("item lain tetap"), tetapi membuat gangguan
   menyeluruh — seperti build lama ini — tampak seperti data kosong.
6. **Penahan tanpa jalan keluar.** Tanda yang mengunci alur (`IsNeedApproval`) dinyalakan oleh data
   master penjamin, sementara tidak ada layar untuk membukanya (`ISS-DOK-005-T7`). **Usulan:** setiap
   status yang menahan alur klinis wajib punya pemilik, layar untuk melepasnya, dan pengecualian
   untuk keadaan darurat sebelum diaktifkan.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Issue dibuat dari laporan 4 butir dan 2 screenshot; audit source backend `671191eb` dan frontend `1f889d67c`; `SELECT` baca-saja pada database pribadi pemilik atas permintaan pelapor | `diagnose-module-issue` |
| 2026-10-06 | Atas permintaan pemilik, rekomendasi K-01 sampai K-04 dipertajam dengan bukti tambahan: penahan pelaksanaan tanpa layar persetujuan (5 pesanan asuransi macet), resolver tarif Billing, dan data penjamin perusahaan contoh. Rekomendasi K-03 dikoreksi dari A ke C karena pilihan A akan membekukan semua tindakan pasien perusahaan. Temuan `ISS-DOK-005-T7` ditambahkan | `diagnose-module-issue` |
| 2026-10-06 | Pelapor menegaskan lewat lampiran 3 bahwa tombol pada butir 3 adalah aksi **Pilih** di katalog Daftar Tindakan Medis, dan tombol itu tidak bernama. `ISS-DOK-005-03`, tanya-jawab butir 3, dan pilihan `K-01` diperjelas: "Pilih" untuk tombol katalog, "Hapus" untuk tombol keranjang. `K-01` belum diputuskan | `diagnose-module-issue` |
| 2026-10-06 | Pemilik memutuskan `K-01` = A ("tombol pilih dan hapus"). `K-02` ditutup tanpa keputusan baru karena sudah diputuskan `RWI-DEC-218` butir 4 dan `RWI-DEC-220` butir 5. Rencana disetujui sebagian, status issue → `DALAM_PERBAIKAN`. `K-03` dan `K-04` masih menunggu keputusan pemilik | Pemilik (pelapor); dicatat `diagnose-module-issue` |
