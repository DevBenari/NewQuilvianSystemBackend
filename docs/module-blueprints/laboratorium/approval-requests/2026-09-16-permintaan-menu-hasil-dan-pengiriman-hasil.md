# Permintaan Persetujuan — Menu Hasil, Pencetakan, dan Pengiriman Hasil ke Pasien

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-007` |
| `tanggal` | 2026-09-16 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `00-interview-decisions.md` revision `34`; `05-evidence-reconciliation.md` revision `4` bagian 11; bukti `evidence/2026-09-16-menu-hasil-patologi-klinik-anatomi-mikrobiologi.md` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Pemegang wewenang Clinical Governance; pemilik platform; kepala instalasi laboratorium |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Bagian 3, 4, 5, dan 6 berdiri sendiri: penerima cukup
membaca bagian yang menyebut dirinya.

> **Catatan tentang penerima.** Blueprint Laboratorium mencatat `clinical_governance: belum
> ditetapkan` sejak 2026-09-01. `LAB-REQ-004` mengajukan penetapannya pada 2026-09-09 dan
> **belum dijawab**. Permintaan ini **tidak menggantikan** `LAB-REQ-004`; ia menambahkan satu
> perkara baru yang ikut tertahan oleh ketiadaan wewenang yang sama.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Pemilik modul melampirkan requirement untuk **Menu Hasil Patologi Klinik, Patologi Anatomi dan
Mikrobiologi** pada 2026-09-16 — daftar hasil, cetak Nota/Label, dan **pengiriman hasil ke pasien
lewat WhatsApp**. Sepuluh keputusan sudah diambil dan **empat pertentangan ditutup pemilik modul
pada hari yang sama**. Yang tersisa **empat hal, dan tidak satu pun ada di dalam wewenang
Laboratorium**: siapa yang berwenang menyetujui hasil secara klinis, satu jabatan dokter yang
belum dikenal sistem, gerbang pengiriman pesan beserta pembangkit berkas PDF yang **keduanya nol
pada platform**, dan ukuran cetak yang menuntut printer fisik yang nyata.

Tanpa jawaban atas keempatnya, menunya **tetap tidak dapat dibangun** — tetapi bukan karena
permintaan ini. Menu ini berdiri di atas hasil pemeriksaan yang belum ada, dan itu tertahan
`LAB-SIGN-001` sejak 2026-09-01.

Yang diminta di sini **empat hal**. Bagian 3, 4, 5, dan 6.

---

## 2. Kenapa permintaan ini muncul sekarang

Requirement yang dilampirkan diperiksa terhadap keputusan yang sudah dikunci dan terhadap source
aplikasi. **Tidak satu pun keputusan lama dicabut.** Satu butir artifact justru **tidak diadopsi
apa adanya** — usulan menyatukan tiga disiplin dalam satu datatable ditolak, dan pola tiga daftar
sejajar dipertahankan (`LAB-DEC-070`).

Pemeriksaan source kemudian menemukan empat hal yang tidak diketahui sebelumnya. Keempatnya
menjadi isi permintaan ini.

---

## 3. Kepada pemegang wewenang Clinical Governance — persetujuan hasil sebelum dikirim

**Yang dinyatakan requirement.** Hasil laboratorium hanya boleh dikirim kepada pasien sesudah
memperoleh persetujuan **Profesor** dan **Dokter Lab**.

**Yang menjadi persoalan.** Sistem hari ini **nol mengenal keduanya**. `LAB-PERM-v1` tidak memuat
peran Profesor maupun Dokter Lab. Nol entity persetujuan hasil — `LabCriticalBoundApproval` yang
ada adalah persetujuan **batas nilai**, bukan persetujuan hasil.

Lebih jauh: ini **persis perkara yang sama** dengan tanda tangan klinis `LAB-DEC-011` yang menahan
slice `S4`, `S4b`, `S4c`, `S5`, dan `S6` sejak 2026-09-01.

**Yang diminta — tiga kalimat jawaban sudah cukup:**

1. Apakah persetujuan Profesor dan Dokter Lab yang dimaksud requirement ini **sama dengan** tanda
   tangan klinis `LAB-DEC-011`, atau lapis persetujuan yang **berbeda dan tambahan**?
2. Siapa pemegang wewenang Clinical Governance yang mengesahkannya?
3. Apakah **keduanya wajib**, atau salah satu sudah cukup?

**Akibat bila tidak dijawab.** Pengiriman hasil ke pasien tidak dapat dibangun. Daftar hasil,
pencetakan Nota/Label, dan kolom lainnya **tetap dapat** dibangun bila slice `S4` sudah terbuka.

---

## 4. Kepada pemilik platform — gerbang pengiriman pesan dan pembangkit berkas PDF

**Yang dinyatakan requirement.** Hasil dikirim lewat **WhatsApp** ke nomor pasien, berbentuk
**PDF atau berkas laporan final yang tidak dapat diedit pasien**.

**Yang ditemukan pada source, dan sudah diverifikasi ulang 2026-09-16:**

| Yang dicari | Hasil |
|---|---|
| Pengirim WhatsApp (Twilio, Fonnte, dan sejenisnya) | **Nol** |
| Layanan surel (SMTP/MailKit) | **Nol** |
| Layanan notifikasi umum | **Nol** |
| Pustaka pembangkit PDF pada `.csproj` | **Nol** |
| Pembangkit barcode/QR | **Ada** — `QRCoder 1.8.0`, sudah dipakai `PatientController` |
| Nomor tujuan pada data induk pasien | **Ada** — `MstPatient.WhatsAppNumber`, terpisah dari `PhoneNumber` |

Temuan ini **mengulang `F7`** yang dicatat 2026-09-01 dan membuktikannya masih benar. Pencetakan
yang berjalan hari ini adalah **cetak peramban** dari frontend — menghasilkan tampilan cetak,
bukan berkas yang dapat dilampirkan ke sebuah pesan.

**Yang diminta:**

1. Apakah platform akan menyediakan **gerbang pengiriman pesan** sebagai layanan bersama, dan
   kapan? Laboratorium tidak berwenang memilih vendornya sendiri.
2. Apakah platform akan menyediakan **pembangkit berkas PDF** sebagai layanan bersama? Bila tidak,
   Laboratorium akan mengusulkan pustaka dan menanggung pemeliharaannya — tetapi modul lain
   kemungkinan besar membutuhkan hal yang sama, dan tiga pustaka berbeda di satu aplikasi adalah
   hasil yang buruk bagi semua.
3. Satu pertanyaan yang **melampaui teknis**: mengirim hasil laboratorium ke kanal pihak ketiga
   adalah pengiriman **data klinis ke luar sistem**. `LAB-DEC-030` menandainya *"menyentuh privasi,
   perlu keputusan tersendiri"* sejak 2026-09-01, dan requirement ini mengatur **mekanismenya**,
   bukan **izinnya**. Siapa yang memberi izin itu?

**Akibat bila tidak dijawab.** Dicatat sebagai `LAB-COORD-011`. Pengiriman dan pengiriman ulang
hasil tidak dapat dibangun sama sekali.

---

## 5. Kepada pemegang wewenang klinis — jabatan `Dokter Lantai`

**Yang dinyatakan requirement.** Kolom Dokter Konfirmator menampilkan nama dan jabatan
**`Dokter DPJP` atau `Dokter Lantai`** ketika hasil memuat angka kritis.

**Yang menjadi persoalan.** `Dokter Lantai` **nol kemunculan** di seluruh blueprint Laboratorium.
Ia bukan istilah yang pernah dipakai, dan bukan peran yang dikenal `LAB-PERM-v1`.

**Yang diminta:**

1. Apa yang dimaksud `Dokter Lantai` — dokter jaga ruangan, dokter jaga bangsal, atau peran lain?
2. Kapan konfirmator angka kritis adalah DPJP dan kapan `Dokter Lantai`? Siapa yang memilih?
3. Apakah ia peran tersendiri, atau **jabatan tampilan** dari dokter yang kebetulan bertugas?

**Akibat bila tidak dijawab.** Kolom Dokter Konfirmator tidak dapat dibangun. Perlu dicatat bahwa
kolom ini **sudah tertahan lebih dahulu** oleh `LAB-P0-004` dan `LAB-OPEN-014` — alur nilai kritis
belum ditetapkan sejak 2026-09-01 — sehingga jawabannya **tidak mempercepat apa pun sendirian**.

---

## 6. Kepada kepala instalasi laboratorium — ukuran cetak yang sebenarnya

**Yang dinyatakan requirement.** Nota Lab A4 portrait; Label Lab 100 x 50 mm landscape untuk
amplop; Label Goldar 50 x 25 mm landscape untuk tube.

**Yang perlu diluruskan lebih dulu.** Artifact **menandai sendiri** ketiga ukuran itu
`Confidence: Medium` dan menyebutnya *"default umum implementasi, bukan ukuran klinis/regulasi
yang ditetapkan oleh evidence"*. Karena itu ketiganya **tidak diadopsi** sebagai keputusan
(`LAB-OPEN-032`). Menaikkannya menjadi aturan berarti mengubah tingkat bukti diam-diam.

**Yang diminta — dan ini paling mudah dijawab dengan benda, bukan dokumen:**

1. Merek dan tipe printer label yang benar-benar dipakai laboratorium hari ini.
2. Ukuran media label yang benar-benar dibeli — untuk amplop dan untuk tube.
3. Satu contoh label yang sudah tercetak dan dianggap benar, bila ada.

**Akibat bila tidak dijawab.** Ketiga dokumen cetak tetap dapat dibangun memakai ukuran usulan,
tetapi **hampir pasti perlu disetel ulang saat UAT**. Ini tidak memblokir slice; ia hanya
menentukan apakah pekerjaannya dilakukan sekali atau dua kali.

---

## 7. Yang TIDAK diminta di sini

Supaya penerima tidak perlu menebak batas permintaan ini:

| Hal | Keadaan |
|---|---|
| Bentuk menu Hasil | **Sudah diputuskan** pemilik modul, `LAB-DEC-070`. Tiga daftar sejajar per disiplin |
| Aturan rentang tanggal | **Sudah diputuskan**, `LAB-DEC-064` dan `LAB-DEC-071` |
| Nomor order | **Sudah diputuskan**, `LAB-DEC-072`. Kolom baru berisi nomor urut terbaca |
| Cakupan baris menu | **Sudah diputuskan**, `LAB-DEC-073`. Hanya pesanan Laboratorium |
| Counter `Terkirim ke Pasien` | **Sudah diputuskan** bentuk angkanya, `LAB-DEC-066`. Bentuk penyimpanannya pekerjaan perancangan, bukan permintaan persetujuan |
| Status pembayaran pada layar | **Sudah diputuskan** `LAB-DEC-062`. Jalur bacanya perkara terpisah, `LAB-COORD-010`, dan diajukan kepada pemilik `billing-kasir` lewat **`LAB-REQ-008`** — [`2026-09-17-permintaan-jalur-baca-status-pembayaran.md`](2026-09-17-permintaan-jalur-baca-status-pembayaran.md) |

> **Koreksi, 2026-09-17.** Baris terakhir tabel di atas semula berbunyi *"jalur bacanya sudah
> diajukan terpisah sebagai `LAB-COORD-010`"*. Itu **keliru**: `LAB-COORD-010` saat itu hanya
> tercatat sebagai penahan pada `blueprint-manifest.md`, dan **belum pernah menjadi permintaan
> kepada siapa pun**. Penelusuran keempat dokumen `approval-requests/` pada 2026-09-17
> menghasilkan nol kemunculan sebagai butir permintaan. Kekeliruan ini membuat sebuah penahan
> disangka sedang menunggu jawaban padahal belum pernah ditanyakan, dan ia menggantung sejak
> 2026-09-15. Permintaannya kini benar-benar ada, sebagai `LAB-REQ-008`.

---

## 8. Cara menjawab

Balas dokumen ini per bagian. Satu kalimat per pertanyaan sudah cukup — yang dibutuhkan
**arah keputusannya**, bukan rumusannya. Rumusan formalnya akan ditulis kembali ke
`00-interview-decisions.md` sebagai keputusan bernomor, lengkap dengan nama penyetuju dan
tanggalnya, mengikuti cara `LAB-REQ-005` dan `LAB-REQ-006` dijawab sebelumnya.

Bila salah satu bagian **bukan wewenang Anda**, cukup dinyatakan dan mohon diteruskan kepada yang
berwenang.
