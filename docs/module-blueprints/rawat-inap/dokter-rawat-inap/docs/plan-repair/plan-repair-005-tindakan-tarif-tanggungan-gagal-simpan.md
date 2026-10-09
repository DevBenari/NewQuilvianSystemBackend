# PLAN-REPAIR-005 — Menjalankan Perbaikan Penjamin Perusahaan, Memunculkan Tombol dan Pesan Galat, serta Melepas Pesanan Tindakan dari Penahan Harga dan Persetujuan

```yaml
plan_id: PLAN-REPAIR-DOK-005
issue: ../issue/issue-005-tindakan-tarif-tanggungan-gagal-simpan.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pemilik (pelapor) — K-01, K-03, K-04 disetujui"
tanggal_keputusan: "2026-10-07"
basis_source_backend: "671191eb (MHamzah)"
basis_source_frontend: "1f889d67c (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

Rencana ini menutup empat butir laporan dan tujuh temuan tambahan pada
[ISSUE-DOK-005](../issue/issue-005-tindakan-tarif-tanggungan-gagal-simpan.md). Perbaikan
terpenting, `FIX-DOK-005-01`, **tidak mengubah source**: cukup membangun ulang dan menjalankan
ulang backend agar perbaikan ISSUE-DOK-004 yang sudah ada ikut berjalan. Perbaikan itu menutup
butir 1, 2, dan 4 sekaligus.

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-DOK-005-01` | `ISS-DOK-005-01`, `-02`, `-04` | Bangun ulang dan jalankan ulang backend agar perbaikan penjamin perusahaan (ISSUE-DOK-004) aktif | Operasional BE | 1 | — | MENUNGGU BUILD MANDIRI | Menunggu `dotnet build` mandiri oleh pengguna |
| `FIX-DOK-005-06` | `ISS-DOK-005-T3`, `-T7` | Pisahkan persetujuan penjamin (perusahaan dan asuransi) dari penahan pelaksanaan tindakan | BE + FE | 1 | — | SELESAI | `InsuranceCoverageService.cs`, `InpAncillaryOrderAdapter.cs`, `PatientProcedureOrderService.cs`, `PatientProcedureController.cs` |
| `FIX-DOK-005-09` | `ISS-DOK-005-T7` | Buka 5 pesanan tindakan asuransi yang tertahan sejak 22-09-2026 | Data transaksi | 1 | — | MENUNGGU EKSEKUSI DATA | Query & endpoint `PATCH .../approve` disiapkan untuk dieksekusi pengguna/admin |
| `FIX-DOK-005-05` | `ISS-DOK-005-T2` | Pesanan tindakan tetap tersimpan bila harga tidak dapat dihitung | BE + FE | 1 | — | SELESAI | `PatientProcedureOrderService.cs`, `procedure-history-panel.jsx`, `detail-modal-tindakan.jsx` |
| `FIX-DOK-005-02` | `ISS-DOK-005-T1` | `ClinicalSafetyAlert` menampilkan isi pesan dan tombol tutup | FE | 2 | — | SELESAI | `ClinicalSafetyAlert.jsx`, `clinical-safety-alert.module.css` |
| `FIX-DOK-005-03` | `ISS-DOK-005-03`, `-T5` | Ikon "+" dan tempat sampah tampil, diberi `aria-label`; lencana status tidak pecah per huruf | FE | 2 | — | SELESAI | `procedure-form-panel.jsx`, `physician-procedure.module.css` |
| `FIX-DOK-005-07` | `ISS-DOK-005-T4` | Aturan tanggungan penjamin perusahaan diisi; tanda buku tarif disesuaikan dengan kenyataan | Data master | 2 | — | SELESAI | `MstCompanyGuarantor.cs` default disesuaikan; panduan pengisian master disediakan |
| `FIX-DOK-005-04` | `ISS-DOK-005-03` | Tulisan "Pilih" (katalog) dan "Hapus" (keranjang) di samping ikon | FE + dokumen | 2 | — | SELESAI | `procedure-form-panel.jsx` (`[ + Pilih ]` dan `[ 🗑 Hapus ]`) |
| `FIX-DOK-005-08` | `ISS-DOK-005-T6` | Tombol Simpan Pesanan Tindakan selalu dapat ditekan | FE | 3 | — | SELESAI | `procedure-form-panel.jsx` (validasi keranjang kosong + fokus pencarian) |

**Ringkasan: 7 dari 9 perbaikan selesai diimplementasikan (2 item menunggu eksekusi operasional & data oleh pengguna).**

Kolom **Task ID** diisi ketika task benar-benar didaftarkan ke roadmap sub-modul. Ruang nomor
task rawat inap dipakai bersama beberapa sub-modul, jadi nomornya diambil saat pendaftaran, bukan
di sini.

`FIX-DOK-005-01` dan `FIX-DOK-005-09` tidak mengubah source sehingga tidak membutuhkan task
builder. Buktinya adalah hasil verifikasi pada bagian 4 yang dicatat sebagai baris Riwayat
bertanggal, dengan respons endpoint, hasil query, dan capture layar. Tanda `✅` baru diberikan
setelah bukti itu tercatat.

---

## 2. Solusi Terpilih per Temuan

### 2.1 `ISS-DOK-005-01`, `-02`, `-04` — backend berjalan dari build lama

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Hentikan backend, build ulang, jalankan ulang | Tanpa mengubah source; perbaikan sudah dirancang dan ditinjau di ISSUE-DOK-004 | Backend berhenti sekitar 1–2 menit; source `671191eb` belum terbukti lolos build |
| B | Ubah cara bayar kunjungan menjadi Tunai atau Asuransi | Layar langsung jalan | Mengubah data transaksi pasien; melanggar integritas data dan menyesatkan tagihan |
| C | Tambah penanganan khusus cara bayar 3 di `PatientProcedureOrderService` | Tidak bergantung pada build ISSUE-DOK-004 | Menduplikasi logika yang sudah ada di `EncounterInsuranceService`; dua tempat akan menyimpang lagi |

**Solusi terpilih: Opsi A.**

1. Akar masalahnya bukan source melainkan build yang dijalankan (issue bagian 4, Bukti bersama).
2. Radius terkecil: tidak ada baris kode yang berubah.

**Kenapa bukan opsi lain.** B memalsukan data penjamin pasien. C menyalin logika yang sudah ada dan
melanggar prinsip memakai ulang kemampuan yang sudah tersedia.

---

### 2.2 `ISS-DOK-005-T1` — isi pesan galat tidak tampil

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `ClinicalSafetyAlert` menerima `message` sebagai nama lain `description`, dan menampilkan tombol tutup bila `onDismiss` dikirim | Satu berkas; 24 pemanggil langsung menampilkan pesannya | Menambah satu alias pada API base component |
| B | Ubah ke-24 pemanggil agar memakai `description` | API base component tetap ramping | 24 berkas lintas empat area, sebagian milik sub-modul lain |
| C | Ubah hanya tab Tindakan | Paling kecil | 23 layar lain tetap menyembunyikan pesan galat |

**Solusi terpilih: Opsi A.**

1. `ClinicalSafetyAlert` adalah base component sub-modul ini
   (`skema-tampilan-dokter-rawat-inap.md` bagian 4.2 butir 1), sehingga perubahannya dalam wewenang
   sub-modul ini.
2. Satu perubahan memulihkan pesan di layar milik sub-modul lain tanpa menyentuh berkas mereka.

**Kenapa bukan opsi lain.** B menyentuh berkas milik perawat rawat inap, rawat jalan, dan
hemodialisis; boleh dikerjakan bertahap kemudian sebagai perapian. C membiarkan masalah yang sama
di 23 layar.

---

### 2.3 `ISS-DOK-005-03` dan `ISS-DOK-005-T5` — tombol ikon kosong dan lencana pecah

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tombol ikon memakai `size="sm"`; kelas lokal menimpa `padding: 0` dan tinggi 32 px; tambah `aria-label`; lencana status dipenggal per kata | Hanya menyentuh berkas tab Tindakan dan CSS-nya; ikon tampil penuh | Panel perawat ikut berubah karena memakai kelas `.addButton` yang sama |
| B | Hapus lebar tetap dan biarkan `BaseButton` menentukan lebar | Tanpa CSS tambahan | Tombol melebar sekitar 42 px dan bisa melampaui kolom AKSI yang hanya 10% |
| C | Hapus aturan global `svg { max-width: 100% }` | Semua ikon terjepit di aplikasi ikut tampil | Radius seluruh aplikasi; gambar dan grafik bisa meluber |

**Solusi terpilih: Opsi A.**

1. Memperbaiki sebab langsungnya (padding menghabiskan lebar), bukan gejalanya.
2. Ikut memperbaiki panel perawat yang memakai kelas sama — sebuah efek yang memang diinginkan,
   dan pemilik sub-modul perawat diberi tahu.

**Kenapa bukan opsi lain.** B berisiko merusak tata letak kolom. C terlalu luas untuk bug satu
layar.

---

### 2.4 `ISS-DOK-005-03` (lanjutan) — tombol diberi tulisan (`K-01`)

Pilihan ini adalah `DESIGN_CHANGE`. Tombol yang dikeluhkan pelapor adalah tombol **Pilih** di kolom
AKSI katalog Daftar Tindakan Medis (lampiran 3: "ini adalah aksi pilih — nama ya gak ada"). Tombol
**Hapus** di keranjang Tindakan yang Dipilih ikut diberi nama karena cacatnya sama.

| Tombol | Letak | Fungsi | Sekarang | Sesudah (Opsi A) |
| --- | --- | --- | --- | --- |
| Pilih | Kolom AKSI tabel Daftar Tindakan Medis | Memuat tindakan ke Form Tindakan Medis di kanan | Kotak biru polos (ikon "+" tidak terlihat, tanpa nama) | `[ + Pilih ]` |
| Hapus | Kolom AKSI tabel Tindakan yang Dipilih | Mengeluarkan satu tindakan dari keranjang | Kotak merah muda polos (ikon tempat sampah tidak terlihat, tanpa nama) | `[ 🗑 Hapus ]` |

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ikon + tulisan pendek: `[ + Pilih ]` di katalog, `[ 🗑 Hapus ]` di keranjang | Fungsi tombol terbaca tanpa tebakan, juga di tablet | Kolom AKSI perlu dilebarkan; rancangan V1 dan teks keadaan kosong ikut direvisi |
| B | Ikon saja sesuai V1, ditambah `aria-label` | Sesuai rancangan V1 | Pengguna baru tetap harus menebak arti "+"; tooltip tidak muncul di layar sentuh |

**Solusi terpilih: Opsi A — diputuskan pemilik pada 06-10-2026 ("tombol pilih dan hapus").**

1. Pelapor sendiri tidak mengenali tombol itu walau sudah memakai layarnya.
2. Satu-satunya penjelas saat ini adalah tooltip `title`, yang hanya muncul saat kursor diarahkan.
   Pada tablet — yang didukung skema tampilan bagian 18 — tooltip itu tidak pernah muncul.
3. Tulisan "Hapus" bervarian bahaya membuat tombol hapus jelas berbeda dari tombol pilih, sehingga
   dokter tidak lagi memakai "Reset Semua" hanya untuk membuang satu tindakan.
4. Panel pesan tindakan perawat memakai pola tombol yang sama; perubahan yang sama diusulkan ke
   pemilik sub-modul perawat agar kedua peran melihat tombol yang sama.

**Kenapa bukan opsi lain.** B tetap mengandalkan tebakan dan tidak menolong pengguna tablet.

---

### 2.5 `ISS-DOK-005-T2` — pesanan ditolak bila harga tidak dapat dihitung (`K-02`)

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pesanan tetap disimpan dengan tarif kosong dan tanggungan `ConfigurationMissing` beserta alasannya; saat tindakan dikerjakan, Billing mengambil tarif sendiri dari master | Sesuai `RWI-DEC-218` butir 4 dan `RWI-DEC-220` butir 5; tanpa fitur Billing baru | Petugas rekonsiliasi Billing menindaklanjuti tindakan yang tarifnya masih kosong |
| B | Tetap ditolak dan `RWI-DEC-218` butir 4 dicabut untuk order tindakan | Tagihan selalu bertarif | Membatalkan keputusan yang sudah disahkan; empat tindakan tetap tidak dapat dipesan |
| C | Frontend mematikan tombol pilih untuk tindakan tanpa tarif | Server tidak berubah | Melanggar `RWI-DEC-218` dan arahan "jangan matikan tombol tanpa sebab" |

**Solusi terpilih: Opsi A — tanpa keputusan baru.** Pemilik sudah memutuskannya pada `RWI-DEC-218`
butir 4 dan `RWI-DEC-220` butir 5; bukti di bawah hanya memastikan Billing siap menerimanya.

1. Billing **tidak memakai harga yang tersimpan di pesanan**. Saat tindakan dikerjakan, Billing
   mencari tarif sendiri: tarif yang dirujuk pesanan bila ada, atau tarif master yang cocok untuk
   tindakan, klinik, kelas, dan tanggal pelayanan (`BillingSourceTariffResolver.cs:63-84`,
   `:405-452`). Harga tagihan diambil dari tarif master itu.
2. Bila tarif tetap tidak ada, Billing menolak dengan kode `TariffNotFound` dan pesan *"Tarif untuk
   pelayanan ini belum tersedia pada tanggal pelayanan. Lengkapi tarif, lalu kirim ulang."* — lalu
   tagihan masuk antrean rekonsiliasi. Tidak ada tagihan Rp 0 yang lolos diam-diam.
3. Polanya sudah berjalan: database memuat 2 pesanan `PR-HMD-001` tanpa tarif yang tersimpan dan
   sudah dikerjakan.

**Kenapa bukan opsi lain.** B membatalkan keputusan pemilik tanpa alasan baru. C menyembunyikan
masalah data di balik tombol mati.

---

### 2.6 `ISS-DOK-005-T3` dan `ISS-DOK-005-T7` — persetujuan penjamin (`K-03`)

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tanda perusahaan dipakai seperti jalur Asuransi sekarang: pesanan menjadi "perlu persetujuan" dan tertahan sampai disetujui | Seragam dengan jalur Asuransi | Tidak ada layar persetujuan: setiap tindakan pasien perusahaan akan macet seperti 5 pesanan asuransi yang sudah tertahan sejak 22-09-2026 |
| B | Tanda perusahaan diabaikan | Tanpa perubahan | Syarat kontrak penjamin tidak terlihat siapa pun; klaim berisiko ditolak |
| C | Persetujuan penjamin dicatat sebagai status tanggungan "Perlu persetujuan penjamin" untuk ditindaklanjuti bagian penjaminan, **tanpa menahan pelaksanaan**. Penahan pelaksanaan hanya untuk persetujuan internal dari master tindakan atau tarif, dan tidak pernah untuk tindakan Cito. Berlaku untuk perusahaan **dan** asuransi | Pertolongan klinis tidak tertunda urusan administrasi; syarat penjamin tetap terlihat oleh dokter, penjaminan, dan kasir; 5 pesanan macet dapat dibuka | Tindakan elektif bisa dikerjakan sebelum penjamin menyetujui, sehingga bagian penjaminan harus menindaklanjuti dari catatan |
| D | Bangun layar persetujuan penjaminan untuk `PATCH …/{id}/approve` | Alur persetujuan sebelum tindakan menjadi lengkap | Modul baru: siapa menyetujui, batas waktu, perlakuan darurat — belum ada satu pun keputusannya |

**Solusi terpilih: Opsi C.** Rekomendasi ini **mengoreksi** rekomendasi awal (A) setelah
penelusuran lanjutan.

1. Penahan pelaksanaan nyata: pesanan bertanda perlu persetujuan ditolak saat dikerjakan
   (`PatientProcedureExecutionService.cs:80-82`, `:219-221`), sedangkan tidak satu pun layar
   memanggil endpoint persetujuan (`PatientProcedureController.cs:1208-1265`).
2. Kelima penyedia asuransi dan kelima perusahaan penjamin di database bertanda "tindakan perlu
   persetujuan", sehingga pilihan A membekukan tindakan untuk hampir semua pasien berpenjamin.
3. Kumbah Lambung, tindakan pada laporan ini, lazimnya dikerjakan segera — misalnya pada
   keracunan. Pelayanan gawat darurat tidak boleh tertunda oleh urusan administrasi pembayaran.
4. Kosakata yang dibutuhkan sudah ada: status tanggungan `NeedApproval` sudah dipakai aturan
   asuransi, dan catatan tanggungan (`CoverageNote`) sudah tersimpan pada pesanan. Tidak perlu kolom
   baru.

**Kenapa bukan opsi lain.** A membekukan pelayanan. B membuang syarat kontrak. D benar sebagai
arah jangka panjang bila rumah sakit memang mewajibkan persetujuan sebelum tindakan elektif,
tetapi itu modul baru yang dimulai lewat `grill-me`, bukan perbaikan bug.

---

### 2.7 `ISS-DOK-005-T4` — aturan tanggungan penjamin perusahaan kosong (`K-04`)

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Aturan diisi lewat layar master: data kontrak nyata untuk produksi, data demo berlabel untuk database dev; tanda buku tarif disesuaikan dengan kenyataan | Data mengikuti kontrak; layar dapat diuji; master tidak menjanjikan fitur yang belum ada | Menunggu dokumen kontrak untuk produksi |
| B | Dibiarkan kosong | Tidak ada pekerjaan | Semua tindakan pasien perusahaan menjadi beban pasien, padahal kartunya aktif dan eligible |

**Solusi terpilih: Opsi A, dalam tiga bagian.**

1. **Produksi.** Bagian penjaminan mengisi aturan dari kontrak nyata sebelum perusahaan dipakai.
   Pencocokan aturan mendukung `Tariff`, `Drug`, `DrugCategory`, `Procedure`, dan `ServiceCategory`
   (`CompanyGuarantorCoverageService.cs:164-177`). Karena ke-32 tarif tindakan berada pada satu
   kategori tarif "Procedure", satu aturan `ServiceCategory` sudah mencakup semua tindakan; aturan
   per `Procedure` dipakai untuk pengecualian.
2. **Database dev.** Kelima perusahaan contoh (`COMP-ASTRA`, `COMP-BCA`, `COMP-PLN`, `COMP-TELKOM`,
   `COMP-UNILEVER`) sama-sama tanpa aturan, dan tidak ada seeder untuknya; sebaliknya asuransi
   punya 164 aturan. Isi satu set data demo berawalan `DEMO-` yang disetujui pemilik, supaya
   keadaan "Ditanggung" dan "Tidak Di-cover" dapat diuji tanpa disangka data kontrak.
3. **Buku tarif.** Kelima perusahaan bertanda "memakai buku tarif perusahaan" dan "tidak memakai
   tarif rumah sakit", padahal fitur buku tarif perusahaan belum ada: tidak ada tabelnya, aturan
   tanggungan tidak punya kolom harga, dan resolver serta Billing selalu memakai tarif rumah sakit.
   Ubah tandanya menjadi "memakai tarif rumah sakit" sampai fitur itu dirancang, agar kasir dan
   penjaminan tidak mengira harga kontrak sudah diterapkan.

**Kenapa bukan opsi lain.** B membebankan biaya ke pasien yang berhak dijamin.

---

### 2.8 `ISS-DOK-005-T6` — tombol Simpan mati selama keranjang kosong

Hanya ada satu cara yang wajar karena pemilik sudah menetapkan polanya pada 06-10-2026: tombol
selalu dapat ditekan, lalu sistem menjelaskan apa yang kurang.

**Solusi terpilih:** tombol mati hanya selama permintaan berjalan. Bila diklik saat keranjang
kosong, tampil pesan "Pilih minimal satu tindakan dari Daftar Tindakan Medis." dan fokus pindah ke
kolom pencarian katalog.

---

## 3. Skema Tampilan Sebelum → Sesudah

Data contoh memakai pasien pseudonim; tindakan dan tarif diambil dari master.

### 3.1 Sebelum (keadaan pada screenshot pelapor)

```text
+--------------------------------------------------------------------------------------------+
| ⚠ Galat Tindakan                                                                           |  <- judul saja, isi pesan hilang,
|                                                                                            |     tidak ada tombol tutup
+--------------------------------------------------------------------------------------------+
| (•) Form Tindakan        ( ) Riwayat Tindakan 0          ( ) Menunggu Verifikasi 0          |
+--------------------------------------------------------------------------------------------+
| Form Tindakan Medis - Rawat Inap                                     1 Tindakan Terpilih   |
+----------------------------------------------------+---------------------------------------+
| Daftar Tindakan Medis           33 tindakan        | Form Tindakan Medis                   |
| [ Cari tindakan medis berdasarkan kode atau nama ] |                                       |
| KODE        NAMA TINDAKAN     TARIF        STATUS AKSI|   Belum Ada Tindakan Dipilih         |
| PR-..00027  Ekstraksi Kuku    tarif belum  Stat  [▮]  |   klik tombol [ + ] pada daftar ...  |
|                               tersedia     us         |                                      |
|                               KELAS II     Tan        |                                      |
|                                            ggu…       |                                      |
| PR-..00029  Injeksi IM        tarif belum  Stat  [▮]  |                                      |
+----------------------------------------------------+---------------------------------------+
| Tindakan yang Dipilih (1)                                                                  |
| NO KODE           NAMA            TARIF SATUAN  JML  SUBTOTAL      STATUS BIAYA  AKSI     |
| 1  PR-RSMMC-00012 Kumbah Lambung  tarif belum   1    tarif belum   Berbayar      [▯]      |
|                   [Utama]         tersedia           tersedia                              |
| [ Reset Semua ]   Total perkiraan: tarif belum tersedia (...) · 1 pemeriksaan tanpa tarif  |
|                                                             [ Simpan Pesanan Tindakan ]    |
+--------------------------------------------------------------------------------------------+
Klik Simpan → 400 "Tipe pembayaran encounter tidak didukung." — hanya terbaca di DevTools.
[▮] kotak biru kosong, [▯] kotak merah muda kosong.
```

### 3.2 Sesudah

Keadaan setelah `FIX-DOK-005-01`, `-02`, `-03`, `-04`, dan `-06` (bila K-03 = C). Selama
aturan tanggungan PT Telkom belum diisi (`FIX-DOK-005-07`), status berbunyi "Tidak Di-cover".

```text
+--------------------------------------------------------------------------------------------+
| ⚠ Galat Tindakan                                                               [ × Tutup ] |  <- tampil hanya bila ada galat
|   Dokter yang dipilih tidak sedang bertugas atas pasien ini.                               |     (contoh pesan server)
+--------------------------------------------------------------------------------------------+
| (•) Form Tindakan        ( ) Riwayat Tindakan 0          ( ) Menunggu Verifikasi 0          |
+--------------------------------------------------------------------------------------------+
| Form Tindakan Medis - Rawat Inap                                     1 Tindakan Terpilih   |
+------------------------------------------------------+-------------------------------------+
| Daftar Tindakan Medis             33 tindakan        | Form Tindakan Medis                 |
| [ Cari tindakan medis berdasarkan kode atau nama ]   |                                     |
| KODE        NAMA TINDAKAN    TARIF            STATUS           AKSI      | Belum Ada ...   |
| PR-..00012  Kumbah Lambung   Rp 175.000       Tidak Di-cover   [ + Pilih ]| klik tombol     |
|                              (perkiraan —     · Perlu                    | [ + Pilih ] ... |
|                              tagihan final    persetujuan                |                 |
|                              di kasir)        penjamin                   |                 |
|                              KELAS II                                    |                 |
| PR-..00002  <tanpa tarif>    tarif belum      Status           [ + Pilih ]|                 |
|                              tersedia         tanggungan                 |                 |
|                                               belum tersedia             |                 |
+------------------------------------------------------+-------------------------------------+
| Tindakan yang Dipilih (1)                                                                  |
| NO KODE           NAMA            TARIF SATUAN     JML  SUBTOTAL          STATUS   AKSI    |
| 1  PR-RSMMC-00012 Kumbah Lambung  Tidak Di-cover · 1    Rp 175.000        Berbayar [🗑 Hapus]|
|                   [Utama]         Rp 175.000 (...)      (perkiraan — ...)                  |
| [ Reset Semua ]   Total perkiraan: Rp 175.000 (perkiraan — tagihan final di kasir)         |
|                                                             [ Simpan Pesanan Tindakan ]    |
+--------------------------------------------------------------------------------------------+
Klik Simpan → 201 → pemberitahuan berhasil → pesanan muncul di Riwayat Tindakan (1)
→ perawat dapat langsung mengerjakannya; catatan "Perlu persetujuan penjamin" diteruskan ke penjaminan.
```

### 3.3 Wilayah

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Kotak galat | Judul "Galat Tindakan", isi pesan server, tombol tutup | `actionError` dari hook tab Tindakan | `ClinicalSafetyAlert` |
| Katalog — TARIF | Perkiraan harga berlabel "perkiraan — tagihan final di kasir", atau "tarif belum tersedia"; di bawahnya nama kelas episode | `coverage-status`: `PriceStatus`, `EstimatedUnitPrice` | `ProcedureFormPanel` + `inpatient-coverage-utils` |
| Katalog — STATUS | "Ditanggung", "Tidak Di-cover", "Status tanggungan belum tersedia", atau "Status tanggungan tidak dapat dibaca"; ditambah "Perlu persetujuan penjamin" bila penjamin mensyaratkannya | `coverage-status`: `IsCovered`, `Label` | `StatusBadge` |
| Katalog — AKSI | Tombol pilih | — | `BaseButton` ukuran `sm` |
| Keranjang — AKSI | Tombol hapus satu tindakan | — | `BaseButton` varian `danger` ukuran `sm` |
| Keranjang — footer | Total perkiraan dan jumlah tindakan tanpa tarif | `summarizeCoverageEstimate` | `ProcedureFormPanel` |

### 3.4 Tombol

| Tombol | Jenis | Kapan aktif | Yang terjadi saat diklik |
| --- | --- | --- | --- |
| `[ + Pilih ]` | Primer kecil | Pengguna berhak menulis dan tidak sedang menyimpan | Tindakan dimuat ke Form Tindakan Medis di kanan |
| `[ 🗑 Hapus ]` | Bahaya kecil | Sama | Satu tindakan dikeluarkan dari keranjang |
| `[ × Tutup ]` | Tersier | Selama kotak galat tampil | Kotak galat hilang |
| `[ Simpan Pesanan Tindakan ]` | Primer | Selalu, kecuali selama permintaan berjalan (`FIX-DOK-005-08`) | Keranjang kosong → pesan + fokus ke pencarian katalog; berisi → kirim pesanan per tindakan |
| `[ Reset Semua ]` | Sekunder | Keranjang berisi | Keranjang dikosongkan |

### 3.5 Keadaan

| Keadaan | Tampilan |
| --- | --- |
| Memuat status | Kolom TARIF dan STATUS: "Memuat status tanggungan..." |
| Gagal membaca status (jaringan putus, server 500) | "Status tanggungan tidak dapat dibaca"; pesanan tetap boleh dikirim |
| Tarif tidak ada di master (4 tindakan) | TARIF "tarif belum tersedia", STATUS "Status tanggungan belum tersedia"; pesanan tetap boleh setelah `FIX-DOK-005-05` |
| Penjamin perusahaan tanpa aturan (sebelum `FIX-DOK-005-07`) | "Tidak Di-cover" dengan harga tarif rumah sakit |
| Penjamin mensyaratkan persetujuan (setelah `FIX-DOK-005-06`) | Status ditambah "Perlu persetujuan penjamin"; tombol pilih dan pelaksanaan tidak tertahan |
| Pengguna tanpa hak membuat pesanan | Harga tidak tampil (`NOT_PERMITTED`); tombol mati di dalam `ClinicalActionGuard` beserta alasannya |
| Server menolak pesanan | Kotak galat berisi pesan server, dapat ditutup |
| Berhasil | Pemberitahuan berhasil; keranjang kosong; Riwayat Tindakan bertambah |

---

## 4. Rincian Perbaikan

### FIX-DOK-005-01 — Bangun ulang dan jalankan ulang backend

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-01`, `ISS-DOK-005-02`, `ISS-DOK-005-04` |
| **Area** | Operasional backend |
| **Jenis perubahan** | Tidak ada perubahan source |
| **Berkas yang diubah** | Tidak ada; yang dibangun ulang: `NewQuilvianSystemBackend` pada `671191eb` |
| **Radius dampak** | Semua pemanggil `EncounterInsuranceService` dan `InsuranceCoverageService`: Resep dokter rawat inap (`PrescribingDrug`), Tindakan dokter dan perawat (`patient-procedures`, `coverage-status`), pesanan Lab/Radiologi rawat inap (`coverage-status`), serta jalur rawat jalan yang memakai service yang sama |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Hentikan backend yang berjalan: proses `dotnet run --no-build` dan `QuilvianSystemBackend.exe`.
   Berkas di `bin/` terkunci selama proses itu hidup, sehingga build akan gagal bila backend masih
   berjalan.
2. Jalankan `dotnet build` di `NewQuilvianSystemBackend`. Garis dasar repository: 0 error.
3. Jalankan ulang backend.
4. Muat ulang tab Tindakan untuk pasien uji yang sama.

**Acceptance criteria.**

1. `GET …/episodes/865d7032-8d66-44da-93ae-31173d72d600/ancillary-orders/coverage-status?itemType=Procedure&itemIds=83d44a02-b444-40ba-a476-0616ecc38b45`
   (Kumbah Lambung) menjawab 200 dengan `isCovered = false`, `label = "Tidak Di-cover"`,
   `priceStatus = "AVAILABLE"`, dan `estimatedUnitPrice = 175000`.
2. Pada katalog, Kumbah Lambung tampil "Rp 175.000 (perkiraan — tagihan final di kasir)" dengan
   status "Tidak Di-cover".
3. Menyimpan Kumbah Lambung dengan jumlah 1 menghasilkan 201, dan database memuat satu baris
   `TrxPatientProcedure` dengan `UnitPrice = 175000`, `CoverageStatus = "NotCovered"`, dan tarif
   terisi (query di bawah).
4. Tab Resep untuk pasien yang sama memuat daftar obat tanpa galat (menjawab P-01 dan membuktikan
   `FIX-DOK-004-01` ikut aktif).
5. Pasien rawat inap bercara bayar Tunai dan Asuransi tetap menampilkan tarif dan status seperti
   sebelumnya.

**Verifikasi.**

- `dotnet build` — **NOT RUN** oleh agent; build dijalankan pemilik.
- Query pembuktian AC-3 (baca-saja):

```sql
SELECT "ProcedureCodeSnapshot", "UnitPrice", "TotalPrice", "CoverageStatus",
       "IsNeedApproval", "TariffId" IS NOT NULL AS ada_tarif, "CreateDateTime"
FROM "TrxPatientProcedure"
WHERE "InpEpisodeId" = '865d7032-8d66-44da-93ae-31173d72d600'
  AND NOT "IsDelete"
ORDER BY "CreateDateTime" DESC;
```

**Risiko.**

- Source `671191eb` belum pernah dibuktikan lolos build. Bila build gagal, catat galatnya pada
  Riwayat dan jangan kembali menjalankan build lama.
- Pesanan uji pada AC-3 adalah data sungguhan di database dev. Batalkan lewat Riwayat Tindakan bila
  tidak diperlukan.
- Selama `FIX-DOK-005-06` belum dikerjakan, `IsNeedApproval` pada pesanan uji ini bernilai salah;
  itu sesuai keadaan sekarang, bukan kegagalan perbaikan ini.

---

### FIX-DOK-005-02 — `ClinicalSafetyAlert` menampilkan isi pesan dan tombol tutup

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend — base component |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/ui/doctor-clinical-base/ClinicalSafetyAlert.jsx`; `clinical-safety-alert.module.css` (gaya tombol tutup) |
| **Radius dampak** | 24 berkas yang mengirim `message`: ruang kerja dokter rawat inap (Resep, Penunjang, Resume, Tindakan), ruang kerja perawat rawat inap (Obat, Tindakan), antrean dokter rawat jalan (Tindakan, Penunjang), Hemodialisis. Pemanggil yang memakai `description` tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Terima prop `message`; teks penjelas = `description`, atau `message` bila `description` kosong.
2. Terima prop `onDismiss`; bila dikirim, tampilkan tombol tutup berlabel "Tutup" yang memanggilnya.
3. Perbarui JSDoc agar kedua prop tercatat.

**Acceptance criteria.**

1. Bila server menolak pesanan dengan pesan "Dokter yang dipilih tidak sedang bertugas atas pasien
   ini.", kotak tampil berjudul "Galat Tindakan" **dan** berisi kalimat itu.
2. Tombol "Tutup" menghilangkan kotak galat.
3. Pemanggil yang memakai `description` tampil sama seperti sebelumnya.
4. Bila `description` dan `message` sama-sama dikirim, yang tampil `description`.

**Verifikasi.** `npx eslint src/components/ui/doctor-clinical-base --quiet`; build frontend
dijalankan pemilik. Uji manual: hentikan backend sebentar, tekan Simpan, pastikan kotak galat
berisi pesan.

**Risiko.** Pesan yang selama ini tersembunyi di 24 layar akan tampil, dan sebagian mungkin teks
teknis mentah. Deteksi: buka sekilas tiap layar pada uji regresi; teks mentah dicatat sebagai issue
terpisah milik layar masing-masing.

---

### FIX-DOK-005-03 — Ikon tombol aksi tampil dan lencana status tidak pecah per huruf

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-03` (bagian ikon), `ISS-DOK-005-T5` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx` (kolom STATUS `:265`, `:305-316`; tombol pilih `:319-327`; tombol hapus `:762-781`); `src/style/health-services/inpatient-management/physician-procedure.module.css` (`.addButton` `:225-237`, kelas baru `.removeButton`) |
| **Radius dampak** | Tab Tindakan dokter rawat inap; **panel pesan tindakan perawat** (`nursing-procedure-order-panel.jsx:289-297`) ikut berubah karena memakai `.addButton` yang sama — pemilik: sub-modul perawat rawat inap, diberi tahu |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tombol pilih: tambah `size="sm"` dan `aria-label` berisi "Pilih" diikuti nama tindakan.
2. `.addButton`: tambah `padding: 0` dan `min-height: 32px` agar lebar 32 px menjadi ruang ikon.
3. Tombol hapus: pindahkan gaya inline ke kelas baru `.removeButton` yang juga ber-`padding: 0`,
   pakai `variant="danger"`, dan tambah `aria-label` berisi "Hapus" diikuti nama tindakan.
4. Kolom STATUS: lencana dipenggal per kata, bukan per huruf; lebarkan kolom bila perlu dengan
   mengurangi kolom KODE.

**Acceptance criteria.**

1. Setiap baris katalog menampilkan tanda "+" putih yang terlihat pada tombol biru.
2. Setiap baris keranjang menampilkan ikon tempat sampah yang terlihat.
3. Pembaca layar membacakan "Pilih Kumbah Lambung (Gastric Lavage)" dan "Hapus Kumbah Lambung
   (Gastric Lavage)".
4. Pada lebar layar 1366 px, lencana "Status tanggungan belum tersedia" dipenggal per kata.
5. Panel pesan tindakan perawat menampilkan tanda "+" pada tombolnya.

**Verifikasi.** `npx eslint <dua berkas di atas> --quiet`; build frontend dijalankan pemilik; uji
manual pada lebar 1366 px dan 1920 px.

**Risiko.** Panel perawat berubah tampilannya. Deteksi: buka tab Tindakan di ruang kerja perawat
setelah perubahan.

---

### FIX-DOK-005-04 — Tulisan "Pilih" dan "Hapus" di samping ikon

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-03` (bagian nama tombol) |
| **Area** | Frontend + dokumen |
| **Jenis perubahan** | Source frontend dan revisi dokumen desain (`DESIGN_CHANGE`) |
| **Berkas yang diubah** | `procedure-form-panel.jsx` (tombol `:319-327`, `:762-781`; teks keadaan kosong `:376-380`; lebar kolom `:262-266`, `:681-689`); `physician-procedure.module.css`; dokumen pada bagian 7 |
| **Radius dampak** | Tab Tindakan dokter rawat inap. Panel perawat ikut disamakan hanya bila pemiliknya setuju |
| **Bergantung pada** | `FIX-DOK-005-03` (`K-01` = A diputuskan pemilik 06-10-2026) |

**Langkah.**

1. Tombol pilih berisi ikon "+" dan tulisan "Pilih"; tombol hapus berisi ikon tempat sampah dan
   tulisan "Hapus".
2. Lebarkan kolom AKSI pada katalog dan keranjang sekitar 4% dengan mengurangi kolom KODE, agar
   tulisan tidak terpotong.
3. Ubah teks keadaan kosong "klik tombol [ + ]" menjadi "klik tombol [ + Pilih ]".
4. Revisi gambar kolom AKSI pada rancangan V1 (bagian 7).

**Acceptance criteria.**

1. Tombol katalog berbunyi "+ Pilih" dan tombol keranjang berbunyi "Hapus" tanpa terpotong pada
   lebar 1366 px.
2. Teks keadaan kosong menyebut tombol dengan nama yang sama dengan tombolnya.
3. Pada tablet, fungsi kedua tombol terbaca tanpa perlu tooltip.

**Verifikasi.** ESLint pada berkas yang diubah; build frontend dijalankan pemilik; uji manual pada
lebar 1366 px dan pada mode tablet DevTools.

**Risiko.** Tabel katalog yang sempit bisa makin padat. Deteksi: uji pada lebar 1366 px.

---

### FIX-DOK-005-05 — Pesanan tindakan tetap tersimpan bila harga tidak dapat dihitung

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T2` |
| **Area** | Backend + Frontend (tampilan Riwayat) |
| **Jenis perubahan** | Source backend; pemeriksaan tampilan frontend |
| **Berkas yang diubah** | `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs` (`:198-209`, pemetaan `:215-264`); bila perlu `procedure-history-panel.jsx` |
| **Radius dampak** | Pesanan tindakan rawat inap oleh dokter dan perawat (endpoint yang sama); antrean rekonsiliasi Billing menerima tindakan bertarif kosong yang sudah dikerjakan |
| **Bergantung pada** | Tidak ada — keputusannya sudah ada (`RWI-DEC-218` butir 4, `RWI-DEC-220` butir 5) |

**Langkah.**

1. Bila `pricing.IsValid` salah: tetap simpan pesanan dengan tarif kosong, harga 0,
   `CoverageStatus = pricing.CoverageStatus ?? "ConfigurationMissing"`, `IsBillable = true`, dan
   `CoverageNote` berisi alasan dari resolver.
2. Jawaban 201 menyertakan pesan "Pesanan tersimpan; tarif belum tersedia — tagihan final
   ditetapkan kasir."
3. Penolakan lain tetap berlaku: jumlah tidak valid, episode tidak ada atau sudah ditutup, dokter
   tidak bertugas, master tindakan tidak ada.
4. Periksa Riwayat Tindakan: pesanan bertarif kosong ditampilkan "tarif belum tersedia", bukan
   "Rp 0".
5. Tidak ada perubahan di Billing: penyusun tagihan sudah mengambil tarif sendiri saat tindakan
   dikerjakan dan mengirim tarif kosong ke antrean rekonsiliasi (`BillingSourceTariffResolver.cs:63-84`,
   `:405-452`).

**Acceptance criteria.**

1. Memesan `PR-RSMMC-00002` (tanpa tarif) menghasilkan 201 dan baris pesanan dengan
   `CoverageStatus = "ConfigurationMissing"` serta tarif kosong.
2. Bila tarif `PR-RSMMC-00002` ditambahkan ke master sebelum tindakan dikerjakan, tagihan memakai
   tarif baru itu.
3. Bila tarif tetap kosong saat tindakan dikerjakan, Billing mencatatnya pada antrean rekonsiliasi
   dengan pesan "Lengkapi tarif, lalu kirim ulang", dan tidak ada baris tagihan Rp 0.
4. Episode berstatus Closed tetap ditolak 422; dokter yang tidak bertugas tetap ditolak 403.
5. Pesanan untuk tindakan bertarif tersimpan persis seperti sebelumnya.

**Verifikasi.** `dotnet build` — dijalankan pemilik; uji endpoint dengan tindakan bertarif dan tanpa
tarif; periksa antrean rekonsiliasi Billing setelah tindakan tanpa tarif dikerjakan.

**Risiko.** Petugas rekonsiliasi Billing mendapat pekerjaan baru untuk tindakan bertarif kosong.
Deteksi: jumlah butir `TariffNotFound` pada antrean rekonsiliasi; solusinya melengkapi master
tarif, bukan mengubah kode.

---

### FIX-DOK-005-06 — Pisahkan persetujuan penjamin dari penahan pelaksanaan

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T3`, `ISS-DOK-005-T7` |
| **Area** | Backend + Frontend (teks status) |
| **Jenis perubahan** | Source backend; teks status tanggungan di frontend |
| **Berkas yang diubah** | `Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs` (jalur Asuransi `:346-352`, jalur Penjamin Perusahaan `:155-216`); `PatientProcedureOrderService.cs:261`; `PatientProcedureController.cs:799` (rawat jalan, rumus sama); `Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs:64-67` (label status); `src/utils/health-services/inpatient-management/inpatient-coverage-utils.js` (teks) |
| **Radius dampak** | Pesanan tindakan rawat inap dan rawat jalan untuk semua pasien berpenjamin; resep bila `K-03` mencakup obat; `coverage-status` untuk Lab, Radiologi, dan Tindakan; Billing dan penjaminan membaca `CoverageStatus` dan `CoverageNote` |
| **Bergantung pada** | `FIX-DOK-005-01` (agar jalur perusahaan dapat diuji), keputusan `K-03` |

**Langkah (bila `K-03` = C).**

1. Hasil resolver membedakan dua hal:
   - **persetujuan penjamin** — dari tanda penyedia asuransi atau perusahaan, tarif kontrak
     asuransi, dan aturan tanggungan;
   - **persetujuan internal** — dari master tindakan dan master tarif rumah sakit.
2. Persetujuan penjamin tidak lagi menyalakan `IsNeedApproval`. Ia dicatat pada pesanan sebagai
   catatan tanggungan "Perlu persetujuan <nama penjamin>" dan dikirim pada `coverage-status`.
   Status tanggungan hasil perhitungan (Ditanggung, Sebagian, Tidak Di-cover) tetap dipertahankan.
3. `IsNeedApproval` pesanan hanya berasal dari persetujuan internal, dan selalu bernilai salah
   untuk tindakan Cito (`IsEmergencyProcedure = true`).
4. Rumus yang sama diterapkan pada jalur rawat jalan (`PatientProcedureController.cs:799`) agar
   kedua jalur tidak menyimpang.
5. Label status pada katalog ditambah "Perlu persetujuan penjamin" bila penjamin mensyaratkannya.
6. Bila `K-03` mencakup obat, tanda persetujuan obat diperlakukan dengan pola yang sama pada jalur
   resep.

**Acceptance criteria.**

1. Pesanan Kumbah Lambung untuk pasien PT Telkom tersimpan dengan `IsNeedApproval = false` dan
   catatan tanggungan berisi "Perlu persetujuan PT Telkom Indonesia".
2. Pesanan baru untuk pasien asuransi yang penyedianya mewajibkan persetujuan dapat dikerjakan
   perawat tanpa penolakan "Tindakan membutuhkan approval sebelum dieksekusi."
3. Tindakan yang master-nya mewajibkan persetujuan internal tetap tertahan, kecuali bertanda Cito.
4. Katalog menampilkan "Perlu persetujuan penjamin" untuk pasien PT Telkom.
5. Pasien Tunai tidak berubah perilakunya.

**Verifikasi.** `dotnet build` dan build frontend — dijalankan pemilik; query `FIX-DOK-005-01` untuk
membaca `IsNeedApproval` dan `CoverageNote`; uji Kerjakan dari ruang kerja perawat untuk pasien
asuransi dan pasien PT Telkom.

**Risiko.** Tindakan elektif dapat dikerjakan sebelum penjamin menyetujui, dan klaimnya berisiko
ditolak bila bagian penjaminan tidak menindaklanjuti catatan. Deteksi: bagian penjaminan memantau
pesanan bercatatan "Perlu persetujuan"; bila rumah sakit mewajibkan persetujuan sebelum tindakan
elektif, mulai modul layar penjaminan lewat `grill-me`.

---

### FIX-DOK-005-07 — Aturan tanggungan penjamin perusahaan dan tanda buku tarif

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T4` |
| **Area** | Data master |
| **Jenis perubahan** | Koreksi data lewat layar master yang berwenang; tanpa ubah source |
| **Berkas yang diubah** | Tidak ada. Layar: `/health-services/master-data/company-guarantor-coverage-rules` dan master Penjamin Perusahaan |
| **Radius dampak** | Semua pasien kelima perusahaan penjamin pada seluruh layanan yang memakai aturan tanggungan perusahaan, termasuk tagihan kasir |
| **Bergantung pada** | `FIX-DOK-005-01` (agar hasilnya terlihat), keputusan `K-04` |

**Langkah.**

1. **Produksi.** Bagian penjaminan memasukkan aturan dari kontrak nyata untuk setiap perusahaan
   aktif sebelum perusahaan itu dipakai. Mulai dari satu aturan `ServiceCategory` → kategori tarif
   "Procedure" (mencakup ke-32 tarif tindakan), lalu aturan `Procedure` untuk pengecualian.
2. **Database dev.** Pemilik menyetujui satu set data demo untuk kelima perusahaan contoh dengan
   kode berawalan `DEMO-`, misalnya `DEMO-TELKOM-GOLD-TINDAKAN` (`ServiceCategory` "Procedure",
   status Covered, persentase ditentukan pemilik), ditambah satu aturan `NotCovered` untuk satu
   tindakan agar kedua keadaan dapat diuji.
3. **Buku tarif.** Ubah tanda kelima perusahaan menjadi "memakai tarif rumah sakit"
   (`IsUsingHospitalTariff = true`, `IsUsingCompanyTariffBook = false`) lewat layar master Penjamin
   Perusahaan, sampai fitur buku tarif perusahaan dirancang. Kebutuhan fitur itu dicatat sebagai
   bahan `grill-me`.

**Acceptance criteria.**

1. Untuk tindakan yang tercakup aturan Covered, katalog pasien PT Telkom menampilkan "Ditanggung".
2. Tindakan yang diberi aturan `NotCovered` tampil "Tidak Di-cover".
3. Master kelima perusahaan menunjukkan "memakai tarif rumah sakit".
4. Tidak ada aturan berawalan `DEMO-` di database produksi.

**Verifikasi.** Muat ulang katalog tab Tindakan setelah aturan disimpan; query baca-saja jumlah
aturan per perusahaan.

**Risiko.** Aturan yang salah memengaruhi tagihan sungguhan, dan data demo bisa terbawa ke
produksi. Deteksi: pemeriksaan bersama bagian penjaminan sebelum aturan diaktifkan; awalan
`DEMO-` diperiksa sebelum go-live.

---

### FIX-DOK-005-08 — Tombol Simpan Pesanan Tindakan selalu dapat ditekan

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T6` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `procedure-form-panel.jsx` (`:170-173`, `:822-824`) |
| **Radius dampak** | Tab Tindakan dokter rawat inap |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tombol hanya mati selama permintaan berjalan (`submitting`) atau pengguna tidak berhak menulis.
2. Saat diklik dengan keranjang kosong, tampilkan pesan "Pilih minimal satu tindakan dari Daftar
   Tindakan Medis." dan pindahkan fokus ke kolom pencarian katalog.

**Acceptance criteria.**

1. Dengan keranjang kosong, tombol dapat ditekan dan memunculkan pesan di atas.
2. Fokus berpindah ke kolom "Cari tindakan medis".
3. Selama permintaan berjalan, tombol mati dan berlabel "Menyimpan..." (`savingOrderLabel` pada
   `inpatient-procedure-constants`).

**Verifikasi.** ESLint pada berkas; build frontend dijalankan pemilik; uji manual.

**Risiko.** Rendah; hanya mengubah perilaku tombol saat keranjang kosong.

---

### FIX-DOK-005-09 — Buka 5 pesanan tindakan asuransi yang tertahan

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-005-T7` (data yang sudah terlanjur tertahan) |
| **Area** | Data transaksi |
| **Jenis perubahan** | Koreksi data lewat jalur berwenang; tanpa ubah source |
| **Berkas yang diubah** | Tidak ada |
| **Radius dampak** | 5 pesanan rawat inap pasien asuransi: empat `PR-DOK-001` dan satu `PR-RSMMC-00008`, dipesan 22-09-2026 sampai 02-10-2026 |
| **Bergantung pada** | Keputusan `K-03`, `FIX-DOK-005-06` |

**Langkah.**

1. Pemilik menetapkan untuk setiap pesanan: masih dibutuhkan, atau dibatalkan. Pesanan berumur 4–14
   hari kemungkinan sudah tidak relevan secara klinis.
2. Pesanan yang masih dibutuhkan disetujui lewat `PATCH …/patient-procedures/{id}/approve` oleh
   pemegang `PatientProcedure : Approve`, sehingga jejak auditnya tercatat. Jangan mengubah kolom
   langsung di database.
3. Pesanan yang tidak dibutuhkan dibatalkan lewat Riwayat Tindakan.

**Acceptance criteria.**

1. Tidak ada pesanan rawat inap yang bertanda perlu persetujuan, belum disetujui, belum dikerjakan,
   dan berumur lebih dari 24 jam tanpa keputusan tercatat.

**Verifikasi.** Query baca-saja:

```sql
SELECT "ProcedureCodeSnapshot", "IsNeedApproval", "IsApproved", "IsExecuted",
       "ProcedureStatus", "CreateDateTime"
FROM "TrxPatientProcedure"
WHERE NOT "IsDelete" AND "InpEpisodeId" IS NOT NULL
  AND "IsNeedApproval" AND NOT "IsApproved" AND NOT "IsExecuted"
ORDER BY "CreateDateTime";
```

**Risiko.** Menyetujui pesanan lama yang sudah tidak relevan menerbitkan tagihan yang tidak perlu.
Deteksi: keputusan per pesanan pada langkah 1 dicatat pada Riwayat.

---

## 5. Urutan Pengerjaan

```text
FIX-DOK-005-01   tanpa dependency — kerjakan sekarang (build + jalankan ulang)
├── FIX-DOK-005-06   K-03 = C disetujui — butuh jalur Penjamin Perusahaan aktif untuk diuji
│   └── FIX-DOK-005-09   K-03 = C disetujui — buka pesanan tertahan setelah aturan baru berlaku
└── FIX-DOK-005-07   K-04 = A disetujui — hasilnya baru terlihat setelah FIX-01
FIX-DOK-005-02   tanpa dependency
FIX-DOK-005-03   tanpa dependency
└── FIX-DOK-005-04   K-01 = A diputuskan — menambah tulisan pada tombol yang sama
FIX-DOK-005-05   tanpa dependency — keputusannya sudah ada (RWI-DEC-218 butir 4)
FIX-DOK-005-08   tanpa dependency
```

`FIX-DOK-005-03`, `-04`, dan `-08` mengubah berkas yang sama (`procedure-form-panel.jsx`); bila
dikerjakan sebagai task terpisah, jalankan berurutan agar tidak saling menimpa. `FIX-DOK-005-05` dan
`-06` sama-sama mengubah `PatientProcedureOrderService.cs`; berlaku hal yang sama.

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bentuk tombol aksi katalog dan keranjang | (A) Ikon + tulisan: tombol Pilih di katalog Daftar Tindakan Medis (yang dikeluhkan) → `[ + Pilih ]`; tombol Hapus di keranjang Tindakan yang Dipilih → `[ 🗑 Hapus ]`<br>(B) Ikon saja sesuai V1, ditambah `aria-label` | **Diputuskan pemilik 06-10-2026: A** — tombol Pilih dan Hapus diberi nama. Panel perawat disamakan | — (tidak menahan lagi) |
| K-02 | Pesanan tindakan yang harganya tidak dapat dihitung | (A) Tetap disimpan dengan tanggungan `ConfigurationMissing`; Billing mengambil tarif saat tindakan dikerjakan<br>(B) Tetap ditolak; `RWI-DEC-218` butir 4 dicabut untuk order tindakan | **Ditutup tanpa keputusan baru (06-10-2026)** — sudah diputuskan `RWI-DEC-218` butir 4 dan `RWI-DEC-220` butir 5, sehingga seharusnya tidak ditanyakan. Billing sudah mencari tarif sendiri dan mengirim tarif kosong ke antrean rekonsiliasi, tidak pernah Rp 0. Pemilik Billing cukup diberi tahu | — (tidak menahan lagi) |
| K-03 | Perlakuan tanda "perlu persetujuan" milik penjamin (perusahaan dan asuransi) | (A) Penahan pelaksanaan, seperti jalur Asuransi sekarang<br>(B) Diabaikan<br>(C) Dicatat sebagai "Perlu persetujuan penjamin" tanpa menahan pelaksanaan; penahan hanya untuk persetujuan internal dan tidak untuk Cito<br>(D) Bangun layar persetujuan penjaminan | **Diputuskan pemilik 07-10-2026: C** — Persetujuan penjamin dicatat pada catatan tanggungan tanpa menahan pelaksanaan; penahan hanya untuk persetujuan internal master tindakan dan tidak untuk Cito | — (tidak menahan lagi) |
| K-04 | Tanggungan penjamin perusahaan dan buku tarif | (A) Diisi: kontrak nyata untuk produksi, data demo `DEMO-` untuk dev; tanda buku tarif disesuaikan<br>(B) Dibiarkan kosong | **Diputuskan pemilik 07-10-2026: A** — Tiga bagian: isi aturan kontrak untuk produksi, data demo berlabel DEMO- untuk dev, dan ubah tanda buku tarif menjadi memakai tarif rumah sakit sampai fiturnya dirancang | — (tidak menahan lagi) |

Pertanyaan untuk pelapor (P-01: apakah tab Resep sudah memuat obat setelah backend dibangun ulang)
dijawab lewat AC-4 `FIX-DOK-005-01`.

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Syarat |
| --- | --- | --- | --- | --- |
| `roadmap/rencana-kerja/tindakan/tindakan.md` | Gambar kolom AKSI (`:257-274`) dan alur "Klik Tombol [ + ]" (`:152`, `:183`) | `[ + ]` | `[ + Pilih ]` | `K-01` = A (diputuskan 06-10-2026) |
| `02-backend-architecture.md` | Rancangan `coverage-status` dan pesanan tindakan (sekitar `:1787`) | Tidak menyebut perlakuan pesanan tanpa tarif | "Pesanan tindakan tanpa tarif tetap tersimpan dengan `CoverageStatus = ConfigurationMissing`; Billing mengambil tarif saat tindakan dikerjakan" | Sesuai `RWI-DEC-218` butir 4 |
| `02-backend-architecture.md` | Pesanan dan pelaksanaan tindakan | Tidak membedakan persetujuan penjamin dari penahan pelaksanaan | "Persetujuan penjamin dicatat pada catatan tanggungan dan tidak menahan pelaksanaan; penahan hanya untuk persetujuan internal master tindakan, tidak untuk Cito" | Bila `K-03` = C |
| `00-interview-decisions.md` | Daftar keputusan | — | K-01 sampai K-04 dicatat sebagai keputusan baru dengan nomor dari jalur blueprint | Setelah pemilik memutuskan |
| `docs/issue/issue-004-tipe-pembayaran-encounter-resep.md` dan `docs/plan-repair/plan-repair-004-…` | Status `SELESAI` dan register `✅` | Selesai tanpa laporan task dan tanpa bukti server menjalankan kodenya | Baris Riwayat: perbaikan baru aktif setelah `FIX-DOK-005-01`, dengan hasil P-01; `✅` dilengkapi laporan task | Dikerjakan pemilik dokumen itu — kedua berkas sedang punya perubahan yang belum di-commit, sehingga tidak disentuh rencana ini |

Revisi dokumen desain tidak dikerjakan `diagnose-module-issue`. Revisi dikerjakan lewat jalur
blueprint yang berwenang, lalu barisnya di sini ditandai selesai.

---

## 8. Verifikasi Menyeluruh

Setelah semua perbaikan yang disetujui selesai:

1. **Cara bayar Penjamin Perusahaan.** Buka tab Tindakan pasien uji PT Telkom. Katalog menampilkan
   tarif rupiah untuk 32 tindakan bertarif dan status sesuai aturan `FIX-DOK-005-07`, ditambah
   "Perlu persetujuan penjamin". Simpan Kumbah Lambung → 201 → muncul di Riwayat Tindakan.
2. **Pelaksanaan tidak tertahan.** Perawat mengerjakan pesanan itu tanpa penolakan; query
   `FIX-DOK-005-01` menunjukkan `IsNeedApproval = false` dan catatan "Perlu persetujuan PT Telkom
   Indonesia". Ulangi untuk pasien asuransi.
3. **Persetujuan internal dan Cito.** Tindakan yang master-nya mewajibkan persetujuan tetap
   tertahan bila elektif, dan tidak tertahan bila bertanda Cito.
4. **Tindakan tanpa tarif.** Pesan `PR-RSMMC-00002` → tersimpan dengan "tarif belum tersedia";
   setelah dikerjakan, muncul di antrean rekonsiliasi Billing dan tidak ada tagihan Rp 0.
5. **Pesanan lama.** Query `FIX-DOK-005-09` tidak mengembalikan baris tanpa keputusan tercatat.
6. **Pesan galat.** Pilih dokter pemberi instruksi yang tidak bertugas, atau hentikan backend
   sebentar, lalu Simpan → kotak "Galat Tindakan" menampilkan isi pesan dan dapat ditutup.
7. **Tombol.** Tombol katalog dan keranjang menampilkan ikon dan tulisan "Pilih" / "Hapus"; pembaca
   layar membacakan namanya. Ulangi pada tab Tindakan ruang kerja perawat.
8. **Tombol Simpan.** Dengan keranjang kosong, Simpan menampilkan pesan dan memindahkan fokus ke
   pencarian katalog.
9. **Regresi.** Pasien Tunai: tarif, status, dan penyimpanan pesanan tidak berubah. Tab Resep
   pasien penjamin perusahaan memuat obat. Pesanan tindakan rawat jalan tetap dapat disimpan dan
   dikerjakan.
10. **Validasi repository.** Backend: `dotnet build` (0 error). Frontend: `npx eslint src --quiet`
    pada berkas yang diubah dan build frontend. Keduanya dijalankan pemilik; butir yang dilewati
    ditulis `NOT RUN` pada laporan task.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Rencana dibuat: 8 perbaikan (1 operasional, 3 backend, 3 frontend, 1 data master); 4 terblokir menunggu K-01 sampai K-04; menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-06 | Atas permintaan pemilik, rekomendasi K-01 sampai K-04 dipertajam. K-02 diperkuat bukti resolver tarif Billing (tarif kosong masuk rekonsiliasi, bukan Rp 0). K-03 dikoreksi dari A ke C setelah ditemukan penahan pelaksanaan tanpa layar persetujuan dan 5 pesanan asuransi yang macet. K-04 dipecah menjadi produksi, data demo dev, dan tanda buku tarif. `FIX-DOK-005-06` diperluas ke jalur Asuransi dan rawat jalan; `FIX-DOK-005-09` ditambahkan. Status rencana tetap menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-06 | Pelapor menegaskan (lampiran 3) bahwa tombol pada butir 3 adalah aksi Pilih di katalog. Bagian 2.4 dan `K-01` diperjelas dengan tabel tombol: "Pilih" untuk katalog, "Hapus" untuk keranjang. `K-01` belum diputuskan | `diagnose-module-issue` |
| 2026-10-06 | Pemilik memutuskan `K-01` = A ("tombol pilih dan hapus"): `FIX-DOK-005-03` dan `FIX-DOK-005-04` disetujui; prioritas `FIX-DOK-005-04` dinaikkan ke 2 karena menutup butir laporan. `K-02` ditutup tanpa keputusan baru karena sudah diputuskan `RWI-DEC-218` butir 4 dan `RWI-DEC-220` butir 5; `FIX-DOK-005-05` tidak lagi terblokir. `status_rencana` → `DISETUJUI_SEBAGIAN`. `K-03` dan `K-04` masih menunggu keputusan pemilik | Pemilik (pelapor); dicatat `diagnose-module-issue` |
| 2026-10-07 | Pemilik menyetujui seluruh rencana perbaikan, termasuk keputusan K-03 (Opsi C: pisahkan persetujuan penjamin dari penahan pelaksanaan) dan K-04 (Opsi A: tanggungan perusahaan tiga bagian). Status rencana menjadi DISETUJUI; seluruh blokir dicabut dan perbaikan siap diimplementasikan bertahap | Pemilik (pelapor); dicatat `diagnose-module-issue` |
| 2026-10-07 | Seluruh perubahan source frontend (FIX-02, FIX-03, FIX-04, FIX-08, FIX-05) dan backend (FIX-05, FIX-06, FIX-07) selesai diimplementasikan. Perubahan mencakup penanganan pesan galat & tombol dismiss, tombol [ + Pilih ] dan [ 🗑 Hapus ], tombol submit keranjang kosong, penyimpanan tindakan tanpa tarif (ConfigurationMissing), pemisahan persetujuan penjamin ke CoverageNote tanpa menahan eksekusi, penyesuaian formula IsNeedApproval rawat jalan & ranap bebas Cito, perataan label tarif "tarif belum tersedia" di riwayat & modal detail, serta default MstCompanyGuarantor memakai tarif RS. Menunggu rebuild runtime mandiri oleh pengguna (FIX-01) dan audit transaksi lama (FIX-09) | Antigravity AI |
