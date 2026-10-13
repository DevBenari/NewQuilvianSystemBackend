# IGD — Requirement Completeness Gate (slice Form Permintaan Rawat Inap dari IGD)

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Slice | Form Permintaan Rawat Inap dari IGD (transfer IGD → rawat inap lewat permintaan admisi) |
| Tanggal | 12 Oktober 2026 |
| Penilai | Agent (`requirement-completeness-gate`) |
| Snapshot | Backend `rizkiG` `6a1c6cf9`, frontend `RizkiV2` `03b057c03` — sejajar origin, bersih |
| Masukan | `00-interview-decisions.md` bagian *Form Permintaan Rawat Inap dari IGD — 12 Oktober 2026* (`IGD-DEC-267`…`291`, `IGD-FACT-110`…`116`, `IGD-OQ-127`, `128`); `01-existing-capability-map.md` suplemen 3.7 (`IGD-CAP-116`…`142`, `IGD-CONF-16`…`20`, `IGD-UNK-24`…`28`); blueprint Billing/Kasir `BKC-DEC-117` (dibaca, tidak disunting) |
| Baseline rujukan | Tidak dipakai. Seluruh butir bisnis berasal dari keputusan pemilik dan spesifikasi analis |
| Hasil | **`PARTIALLY_READY`** — tiga sub-slice siap desain domain; dua butir sempit tertahan keputusan |

## 1. Scope penilaian

**Batas scope** mengikuti `IGD-DEC-267`: form dan cara form sampai ke petugas admisi. Pembukaan admisi, bed, Workspace PPRI,
tarif, dan penggabungan invoice tetap milik Rawat Inap dan Billing; yang dinilai di sini hanya titik sentuhnya.

| Sub-slice | Isi | Pemilik data |
| --- | --- | --- |
| **S1 — Form di IGD** | Membuka form dari tindak lanjut `RANAP` terkonfirmasi, mengisi, meminta TTD, mengembalikan, menandatangani, membatalkan, mencetak, dan menahan *Jalankan* `RANAP` sampai form terkirim | IGD (keadaan *Draf* dan *Menunggu TTD*); penyimpanannya ditetapkan desain |
| **S2 — Permintaan admisi bersumber IGD** | Form terkirim menjadi `InpAdmissionReferral` `Pending`; daftar petugas admisi; admisi dari permintaan; batal oleh petugas admisi; CITO paling atas; DPJP usulan | Rawat Inap (`IGD-DEC-275`) |
| **S3 — Tagihan IGD ikut invoice rawat inap** | Kunjungan IGD tertaut ke invoice rawat inap saat admisi dibuka dari permintaan IGD | Billing |
| **S1.a — Isian Unit Layanan** | Arti "Unit Layanan (selection poli)" | IGD (isian), Rawat Inap (tujuan admisi) |
| **S1.b — Perawatan Intensif ↔ Unit Layanan** | Apakah centang "Perawatan Intensif" mewajibkan unit ICU | IGD, Rawat Inap |

## 2. Bukti dan wewenang

| Bukti | Wewenang | Menjawab |
| --- | --- | --- |
| Keputusan pemilik `IGD-DEC-267`…`291` (pilihan interaktif, 12 Oktober 2026) | Requirement eksplisit terkini — tertinggi untuk "apa yang dibangun" | Seluruh aturan form, aktor, status, hak akses |
| Spesifikasi analis *Form Permintaan Rawat Inap* (lewat pemilik) | Bukti analis — di bawah keputusan pemilik; dua penyimpangan disahkan pemilik (`IGD-DEC-284`, `286`) | Daftar isian |
| Suplemen 3.7 capability map (`6a1c6cf9` / `03b057c03`) | Bukti implementasi terverifikasi — tertinggi untuk "apa yang ada" | Wadah yang dapat diperluas, pola TTD, sumber data |
| `BKC-DEC-117`, `FR-BKC-247` (blueprint Billing/Kasir) | Keputusan modul pemilik Billing | Tagihan alihan IGD digabung ke invoice rawat inap tanpa kehilangan `SourceDomain` |
| `RWI-DEC-073`, `RWI-RULE-029`, `RWI-AC-071`…`077` (blueprint Rawat Inap) | Keputusan modul pemilik Rawat Inap | Batas MVP Rawat Inap (`INP-S09` di luar MVP) |

Fakta tambahan yang dibaca gate ini: akun login menunjuk dokter lewat `ApplicationUser.DoctorId` (`Models/ApplicationUser.cs`
`:20`), dan layar dokter IGD sudah memakai `userInfo.doctorId` (`use-emergency-physician-patients.jsx` `:32`). Artinya aturan
"TTD hanya dari akun DPJP IGD aktif" (`IGD-DEC-291`) dapat diperiksa server tanpa kemampuan baru.

## 3. Temuan kelengkapan — 18 dimensi

| # | Dimensi | Status | Temuan |
| ---: | --- | --- | --- |
| 01 | Tujuan | `CONFIRMED` | Pasien IGD yang diputuskan dirawat inap sampai ke petugas admisi dengan permintaan tertulis yang ditandatangani DPJP; isi klinisnya tidak hilang di jalan (`IGD-DEC-271`, `275`) |
| 02 | Aktor | `CONFIRMED` | Dokter IGD membuat/mengonfirmasi `RANAP`; Perawat IGD mengisi dan meminta TTD; DPJP IGD aktif menandatangani atau mengembalikan; petugas admisi membuka admisi atau membatalkan (`IGD-DEC-268`, `271`, `273`, `274`, `279`, `285`) |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Tindak lanjut `RANAP` terkonfirmasi (`IGD-DEC-271`); DPJP IGD aktif ada untuk diminta TTD (`IGD-DEC-274`). Prasyarat *akun DPJP terhubung ke dokter* — lihat G-04 |
| 04 | Alur utama | `CONFIRMED` | Konfirmasi `RANAP` → form *Draf* → *Minta TTD* → *Tandatangani* → permintaan admisi `Pending` → *Jalankan* `RANAP` aktif → admisi dibuka dari permintaan → *Selesai* |
| 05 | Alur alternatif / exception | `CONFIRMED` sebagian | Dikembalikan dokter (`285`), DPJP dialihkan (`274`), batal oleh perawat (`278`), batal oleh petugas admisi (`279`), `RANAP` dibatalkan (`280`). Belum: kunjungan tanpa DPJP aktif atau DPJP tanpa akun (G-04), dua perawat membuka form bersamaan (G-05) |
| 06 | Data minimum | `CONFIRMED` kecuali Unit Layanan | Isian analis dan sumbernya (`IGD-DEC-282`, `283`, `284`, `286`, `287`, `289`, `290`); Unit Layanan tertahan `IGD-OQ-127` (G-01) |
| 07 | Aturan bisnis / validation | `CONFIRMED` sebagian | Lama rawat 1–90 (`289`); *Jalankan* tertahan (`277`); TTD hanya DPJP aktif (`291`); kebutuhan khusus boleh lebih dari satu (`284`). Belum: aturan Perawatan Intensif ↔ unit (G-02) |
| 08 | Status / perubahan status | `CONFIRMED` | *Draf* → *Menunggu TTD* → *Terkirim* → *Selesai*; *Dibatalkan* dari tiga jalan; *Menunggu TTD* → *Draf* bila dikembalikan (decision log, tabel *Keadaan form*) |
| 09 | Peran / authorization | `CONFIRMED` | Aksi baru untuk Perawat IGD lewat pengurus Akses Role; TTD dicek server; tanpa penjaga unit tambahan (`291`). Aksi batal petugas admisi — titik sentuh Rawat Inap (`279`, `IGD-OQ-128`) |
| 10 | Dependency antarmodul | `CONFIRMED` | Rawat Inap (permintaan admisi, layar admisi), ClinicalManagement (alergi, diagnosis), Registrasi (penjamin), HR/Master Data (jadwal dokter, kelas, unit), Billing (tautan invoice) |
| 11 | Integrasi internal / eksternal | `CONFIRMED` — internal saja | Tanpa sistem eksternal. SEP/eligibilitas penjamin rawat inap berada di luar slice (Registrasi/Asuransi) |
| 12 | Hasil akhir | `CONFIRMED` | Permintaan admisi *Selesai* karena admisi dibuka darinya, atau *Dibatalkan* beralasan |
| 13 | Pembatalan / koreksi | `CONFIRMED` | Isi bertanda tangan tidak diubah; batal lalu ajukan ulang sebelum admisi; sesudah admisi tidak dapat dibatalkan dari IGD (`278`) |
| 14 | Audit / histori | `PROPOSED` | Pelaku, waktu server, alasan, dan catatan *Kembalikan* untuk setiap perpindahan keadaan; jejak atestasi (akun, nama/jabatan beku, perangkat, IP) — G-06 |
| 15 | Notifikasi | `CONFIRMED` — tidak ada notifikasi baru | `IGD-DEC-288`: CITO hanya urutan dan tanda. Daftar *Perlu TTD* dokter juga tanpa notifikasi — G-07 |
| 16 | Dampak billing | `CONFIRMED` sebagian | Form tidak membuat tagihan. Tautan kunjungan IGD ke invoice rawat inap `CONFIRMED` oleh `BKC-DEC-117`; perilaku bila invoice IGD sudah dibayar `MISSING` (G-03) |
| 17 | Dampak keselamatan klinis | `CONFIRMED` | Identitas dari kunjungan; alergi baca-saja beku saat TTD dengan pembeda "belum ada data" (`283`); diagnosis lengkap, utama paling atas (`282`); pasien tidak berangkat tanpa permintaan (`277`) |
| 18 | Pelaporan / traceability | `PROPOSED` | Waktu permintaan, TTD, dan admisi tercatat sehingga lama tunggu IGD → admisi dapat dihitung; laporan khusus tidak diminta — G-08 |

## 4. Himpunan requirement yang `CONFIRMED`

Seluruh keputusan `IGD-DEC-267`…`291` kecuali `IGD-DEC-269` (`superseded` oleh `272`), ditambah `BKC-DEC-117` untuk S3.
Ringkasnya:

1. Form melengkapi tindak lanjut `RANAP` dokter; tersedia sesudah konfirmasi (`271`).
2. Perawat mengisi dan meminta TTD di layar perawat; DPJP IGD aktif menandatangani dari daftar *Perlu TTD* (`268`, `273`, `274`).
3. TTD = atestasi akun, tanpa gambar, tanpa kata sandi ulang (`272`).
4. Form terkirim = `InpAdmissionReferral` `Pending` bersumber IGD (`275`); admisi tetap manual (`276`).
5. *Jalankan* `RANAP` tertahan sampai form terkirim (`277`).
6. Koreksi lewat batal-ajukan ulang; batal oleh petugas admisi; batal otomatis bila `RANAP` dibatalkan; cetak dari salinan beku (`278`…`281`).
7. Isian dan sumbernya: diagnosis (`282`), identitas/alergi/penjamin (`283`), kebutuhan khusus (`284`), DPJP usulan (`286`), kelas (`287`), CITO (`288`), lama rawat (`289`), indikasi dan rencana perawatan (`290`).
8. Dokter dapat mengembalikan form dengan catatan (`285`); hak akses (`291`).

## 5. Register gap

| ID | Butir | Status | Dampak | Alasan dampak |
| --- | --- | --- | --- | --- |
| G-01 | Arti "Unit Layanan (selection poli)" | `MISSING` (`IGD-OQ-127`) | `BLOCKING` — hanya isian Unit Layanan | Bila poli diizinkan, tujuan form berelasi ke klinik, padahal admisi hanya menerima unit rawat inap; mengubah relasi data dan validation. *Catatan:* `IGD-DEC-271` mengisi unit dari tindak lanjut, dan tindak lanjut menyimpan unit layanan (bukan klinik) — konsisten dengan dugaan pemilik, tetapi belum dikonfirmasi analis |
| G-02 | "Perawatan Intensif" mewajibkan unit ICU? Tipe unit ICU di tiap lingkungan | `MISSING` (`IGD-UNK-25`) | `NON_BLOCKING_STANDARD` | Usulan eksplisit: tingkat perawatan (ICU/biasa) dan unit disimpan terpisah tanpa saling menolak sampai `IGD-UNK-25` dijawab. Tidak mengubah struktur; hanya menunda satu aturan validation |
| G-03 | Penautan kunjungan IGD ke invoice rawat inap bila invoice IGD sudah dibayar atau ditutup | `MISSING` (`IGD-UNK-26`) | `BLOCKING` — hanya S3 | Konsekuensi billing (tagihan ganda atau tertaut ke invoice lunas). Karena kode Billing hari ini menautkan **setiap** permintaan admisi yang selesai, perluasan S2 otomatis menyalakan penautan untuk IGD — desain S2 wajib menyatakan perilakunya sampai owner Billing menjawab |
| G-04 | Kunjungan tanpa DPJP IGD aktif, atau DPJP yang tidak punya akun login terhubung | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: *Minta TTD* ditolak dengan pesan *"Tetapkan DPJP IGD lebih dulu"* atau *"DPJP belum punya akun; alihkan DPJP"*. Mengikuti aturan yang sudah disahkan (`274`, `291`), tanpa aturan baru |
| G-05 | Satu form aktif per kunjungan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: paling banyak satu form *Draf*/*Menunggu TTD*/*Terkirim* per kunjungan IGD; sejalan dengan invariant satu permintaan `Pending` per pasien yang sudah ada |
| G-06 | Isi jejak audit | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: setiap perpindahan keadaan mencatat pelaku, waktu server, alasan/catatan; atestasi mengikuti pola Rawat Inap/Rekam Medis |
| G-07 | Notifikasi daftar *Perlu TTD* | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Usulan: tanpa notifikasi; jumlah *Perlu TTD* tampil di Ruang Kerja Dokter IGD. Tampilan diserahkan desain (`DEV_DISCRETION` dalam pola layar yang ada) |
| G-08 | Laporan lama tunggu IGD → admisi | `PROPOSED` | `NON_BLOCKING_STANDARD` | Datanya tersedia dari waktu yang dicatat; laporan khusus tidak diminta |
| G-09 | Persetujuan pemilik Rawat Inap dan owner Billing atas titik sentuh | `PROPOSED` (`IGD-OQ-128`) | `NON_BLOCKING_STANDARD` untuk desain | Persetujuan lisan dicatat dari laporan pemilik saat approval desain; tidak mengubah keputusan bisnis IGD. Menahan approval desain dan implementasi, bukan penyusunan desain |
| G-10 | Kode jenis tindak lanjut rawat inap di tiap lingkungan | `MISSING` (`IGD-UNK-24`) | `CONFIGURABLE_DEFAULT` | Cara mengenali `RANAP` dapat dibuat berbasis konfigurasi/penanda pada master; nilai tiap lingkungan dicek pemilik tanpa mengubah data |
| G-11 | Penjamin pada kunjungan rawat inap hasil admisi | `MISSING` (`IGD-UNK-28`) | Di luar slice | Wilayah layar admisi Rawat Inap; form hanya menampilkan penjamin IGD |

Nol `CONFLICT` terbuka: `IGD-CONF-16`…`20` selesai lewat `IGD-DEC-271`, `273`, `275`, `276`, `286`; `IGD-CONF-19` berubah
menjadi G-01.

## 6. Decision Log yang dirujuk

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan | Pemilik | Status | Dampak domain |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-127` | Arti "Unit Layanan (selection poli)" | S1.a | Spesifikasi analis; dugaan pemilik "bangsal"; tindak lanjut menyimpan unit layanan | Unit rawat inap dari tindak lanjut | Analis lewat pemilik | `OPEN` | Isian Unit Layanan dirancang sesudah jawaban; sisa S1 tidak tertahan |
| `IGD-OQ-129` | Bagaimana penautan kunjungan IGD ke invoice rawat inap ketika invoice IGD sudah dibayar atau ditutup? (`IGD-UNK-26`) | S3 | `EnsureEncounterLinksAsync` menautkan setiap permintaan selesai; `BKC-DEC-117` mengonsolidasi tagihan alihan IGD | — (milik Billing) | Owner Billing | `OPEN` | S3 tertahan; desain S2 wajib menyatakan perilaku penautan untuk sumber IGD sampai dijawab |
| `IGD-OQ-128` | Persetujuan Rawat Inap dan Billing atas titik sentuh | S2, S3 | Titipan di decision log | Persetujuan lisan dari laporan pemilik | Pemilik | `OPEN` | Approval desain |

`IGD-OQ-129` adalah nomor baru dari gate ini; ia mengangkat `IGD-UNK-26` menjadi pertanyaan keputusan bagi owner Billing dan
ditambahkan ke daftar titipan Billing.

## 7. Kesiapan per sub-slice

| Sub-slice | Kesiapan | Catatan |
| --- | --- | --- |
| S1 — Form di IGD | `READY_FOR_DOMAIN_DESIGN` (kecuali S1.a) | G-04…G-08 dibawa sebagai usulan eksplisit |
| S1.a — Isian Unit Layanan | `BUSINESS_DECISION_REQUIRED` | `IGD-OQ-127` |
| S1.b — Perawatan Intensif ↔ Unit | `READY_FOR_DOMAIN_DESIGN` dengan usulan G-02 | Aturan silang ditunda |
| S2 — Permintaan admisi bersumber IGD | `READY_FOR_DOMAIN_DESIGN` | Bagian yang menyentuh Rawat Inap ditandai butuh persetujuan pemiliknya (`IGD-OQ-128`) |
| S3 — Tagihan IGD ikut invoice rawat inap | `PARTIALLY_READY` | Jalur utama `CONFIRMED` (`BKC-DEC-117`); kasus invoice IGD sudah dibayar `BLOCKING` (`IGD-OQ-129`) |
| **Slice keseluruhan** | **`PARTIALLY_READY`** | — |

**Dependency antar sub-slice:** S2 bergantung pada keadaan *Terkirim* dari S1; S3 menyala sebagai akibat S2 (kode Billing
yang ada). S1.a tidak menahan S1 karena unit terisi dari tindak lanjut; ia hanya menahan arti dan validation isian itu.

## 8. Yang boleh berjalan

1. `hospital-domain-architect` untuk S1, S1.b, dan S2 — slice ini melintasi IGD, Rawat Inap, dan Billing, dan menyentuh
   ownership tabel milik Rawat Inap.
2. Desain S2 menyatakan perilaku penautan Billing untuk sumber IGD sebagai butir terbuka `IGD-OQ-129`, bukan diam-diam
   mengikuti kode yang ada.

## 9. Yang harus berhenti

1. Desain arti dan validation isian Unit Layanan — sampai `IGD-OQ-127`.
2. Desain perilaku Billing untuk invoice IGD yang sudah dibayar — sampai `IGD-OQ-129`.
3. Implementasi apa pun — sampai desain dan kartu disetujui pemilik, termasuk persetujuan titik sentuh (`IGD-OQ-128`), dan
   prioritas terhadap Billing IGD ditanyakan (`IGD-DEC-252`).

## 10. Handoff ke `hospital-domain-architect`

| Butir | Isi |
| --- | --- |
| Modul / slice | `IGD-BP-001` — Form Permintaan Rawat Inap dari IGD, sub-slice S1, S1.b, S2 (dan S3 sebagai titik sentuh terbuka) |
| Snapshot bukti | `6a1c6cf9` / `03b057c03`; suplemen 3.7; decision log `IGD-DEC-267`…`291` |
| Klasifikasi | 25 keputusan `CONFIRMED`; usulan G-02, G-04…G-08, G-10 eksplisit; blocker `IGD-OQ-127`, `IGD-OQ-129` |
| Keluaran yang diharapkan | Bounded context dan ownership (form IGD vs permintaan admisi Rawat Inap), aggregate dan invariant, lifecycle form, otorisasi TTD, titik sentuh Billing, dampak keselamatan klinis, kontrak traceability |
