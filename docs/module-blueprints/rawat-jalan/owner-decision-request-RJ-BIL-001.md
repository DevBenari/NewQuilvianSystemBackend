# Permintaan Keputusan Owner — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Dokumen ID | `RJ-BIL-DECREQ-001` |
| Revisi | `1` |
| Tanggal | `2026-08-24` |
| Blueprint | `RJ-BIL-BP-001`, mengacu `MODULE-STATUS.md` revision `13` |
| Sifat dokumen | Permintaan keputusan. Dokumen ini **tidak** mengubah status blueprint dan **tidak** memberi wewenang implementasi |
| Task yang tertahan | `RJ-BIL-BE-002` dan `RJ-BIL-BE-005` |
| Task yang tidak tertahan | `RJ-BIL-BE-003` dan `RJ-BIL-BE-004` |

## Untuk siapa dokumen ini

Dokumen ini ditujukan kepada pemilik proses bisnis, bukan hanya kepada programmer. Karena itu
setiap pertanyaan disertai contoh kasus nyata di rumah sakit, dan setiap pilihan jawaban
disertai penjelasan akibatnya.

Pembaca tidak perlu memahami kode untuk menjawab. Yang dibutuhkan hanyalah jawaban atas
pertanyaan seperti *"apakah satu kunjungan boleh ditanggung dua asuransi sekaligus?"*.

## Cara mengisi

1. Baca bagian **Keputusan 1** dan **Keputusan 2**.
2. Isi kolom `Jawaban owner` pada [formulir jawaban](#formulir-jawaban) di bagian akhir.
3. Bila tidak ada pilihan yang cocok, pilih `Other` dan tuliskan penjelasannya.
4. Bila sebuah pertanyaan bukan wewenang Anda, tuliskan nama pemilik yang tepat.

Tidak semua pertanyaan harus dijawab sekaligus. Bagian [Urutan menjawab](#urutan-menjawab)
menjelaskan dua pertanyaan mana yang paling menentukan, sehingga menjawab dua itu saja sudah
membuka pekerjaan berikutnya.

---

## Ringkasan: dua hal yang sedang ditunggu

| Keputusan | Pokok persoalan | Task yang terbuka bila dijawab | Bobot |
|---|---|---|---|
| Keputusan 1 — `RJ-BIL-CONFLICT-006` | Modul Farmasi masih boleh menyatakan resep lunas | `RJ-BIL-BE-002` | Bagian A ringan, Bagian B perlu desain |
| Keputusan 2 — `RJ-BIL-CONFLICT-001` | Satu kunjungan hanya bisa punya satu penanggung | `RJ-BIL-BE-005` | Bergantung jawaban; bisa menyusut drastis |

Keduanya berakar pada satu sebab yang sama: **angka dan status keuangan saat ini dimiliki dan
ditulis oleh modul klinis**, bukan oleh modul Billing. Karena itu sebaiknya keduanya dijawab
dalam satu forum, agar solusinya tidak saling bertentangan.

---

## Keputusan 1 — Kewenangan finansial pada modul Farmasi

### Keadaan sekarang

Alur resep rawat jalan yang berjalan hari ini:

1. Dokter membuat resep pada saat konsultasi.
2. Resep masuk ke antrean farmasi.
3. Farmasi menyiapkan lalu menyerahkan obat.
4. Kasir menerima pembayaran.

Yang menjadi masalah ada pada langkah keempat. Di dalam sistem, penetapan status pembayaran
resep **tidak berada di modul kasir/Billing**, melainkan tersedia sebagai endpoint pada modul
Farmasi.

### Bagian 1A — Empat endpoint pembayaran yang tidak dipakai layar mana pun

Grup Swagger: `[Tags("Health Services / Pharmacy Management / Prescription")]`
Base route: `api/v1/health-services/pharmacy-management/prescriptions`

| Method | Endpoint | Fungsi | Hak akses | Status yang ditulis | Baris |
|---|---|---|---|---|---|
| `PATCH` | `/{id}/billing-generated` | Menandai billing resep sudah dibuat | `Prescription : Update` | `BillingGenerated` | `375` |
| `PATCH` | `/{id}/payment-paid` | Menandai pembayaran resep lunas | `Prescription : Update` | `Paid` | `394` |
| `PATCH` | `/{id}/insurance-approved` | Menandai resep disetujui asuransi | `Prescription : Update` | `InsuranceApproved` | `401` |
| `PATCH` | `/{id}/payment-waived` | Menandai pembayaran resep ditiadakan | `Prescription : Update` | `PaymentWaived` | `408` |

Arti praktisnya bagi rumah sakit: **siapa pun yang berhak mengubah resep juga berhak menyatakan
resep itu lunas.** Hak akses yang dipakai keempat endpoint tersebut adalah hak klinis
`Prescription : Update`, bukan hak kasir atau hak keuangan.

Contoh risikonya secara konkret. Seorang petugas farmasi yang seharusnya hanya boleh mengubah
catatan resep, dengan hak akses yang sama dapat memanggil `payment-waived` dan membuat resep
senilai Rp2.500.000 tercatat sebagai tidak perlu dibayar — tanpa melalui kasir, tanpa persetujuan
keuangan, dan tanpa jejak persetujuan berlapis.

Yang membuat keputusan ini ringan: **penelusuran pada frontend commit `29422c8` tidak menemukan
satu pun layar yang memanggil keempat endpoint tersebut.** Di sisi backend, keempat method
tersebut juga tidak dipanggil dari mana pun selain controller-nya sendiri. Jadi menonaktifkannya
tidak merusak layar apa pun yang sedang dipakai.

```text
Pertanyaan 1A:
Apakah keempat endpoint di atas boleh dinonaktifkan sebagai bagian RJ-BIL-BE-002?

Pilihan A:
Boleh dinonaktifkan.
Akibat: permukaan API berbahaya hilang. Tidak ada layar yang rusak.
Penetapan lunas selanjutnya hanya boleh melalui Billing.

Pilihan B:
Belum boleh. Masih ada konsumen di luar frontend ini.
Bila memilih ini, mohon sebutkan konsumennya: sistem lain, integrasi, script internal,
atau pemakaian manual lewat Postman.
Akibat: perlu masa peralihan dan pengetatan hak akses lebih dahulu.

Pilihan C:
Other.
```

### Bagian 1B — Endpoint pembatalan resep yang aktif dipakai

Bagian ini **tidak boleh disamakan** dengan Bagian 1A.

| Method | Endpoint | Fungsi | Hak akses | Baris |
|---|---|---|---|---|
| `PATCH` | `/{id}/cancel` | Membatalkan resep | `Prescription : Update` | `415` |

| Aspek | Empat endpoint 1A | `PATCH /{id}/cancel` |
|---|---|---|
| Dipakai frontend | Tidak ada | **Ada** — `prescription-workspace-service.js:94`, dipakai `use-prescription-workspace.js:608` |
| Yang ditulis | `Paid`, `InsuranceApproved`, `PaymentWaived`, `BillingGenerated` | `PaymentStatus = Cancelled` (`PrescriptionWorkflowService.cs:118`) |
| Risiko bila dinonaktifkan | Rendah | **Tinggi** — pembatalan resep adalah alur dokter yang aktif dipakai |

Endpoint ini sudah memiliki pengaman: pembatalan ditolak bila resep sudah diproses farmasi
(`QueuedAtPharmacy` sampai `Dispensed`). Namun ia tetap menulis status pembayaran dari modul
klinis, sehingga tetap termasuk pelanggaran kepemilikan yang sama.

Contoh kasus yang membuat ini rumit. Dokter membatalkan resep pukul 10.00. Bila resep tersebut
sudah memiliki charge di Billing, pertanyaannya menjadi: apakah charge itu ikut hangus, ikut
dibatalkan, atau memerlukan pembatalan tagihan tersendiri oleh kasir? Sistem saat ini menjawabnya
dengan menulis `Cancelled` langsung pada status pembayaran — tanpa melewati Billing sama sekali.

```text
Pertanyaan 1B:
Bagaimana pembatalan resep oleh dokter seharusnya bekerja?

Pilihan A:
Pembatalan klinis hanya menetapkan status klinis dan status pemenuhan, lalu mengirim
pemberitahuan ke Billing. Billing yang memutuskan akibat finansialnya.
Akibat: paling selaras dengan arah blueprint. Perlu desain di dalam RJ-BIL-BE-002.

Pilihan B:
Biarkan seperti sekarang untuk sementara, perbaiki pada tahap berikutnya.
Akibat: pelanggaran kepemilikan tetap ada dan tercatat sebagai utang teknis.

Pilihan C:
Other.

Pertanyaan tambahan yang perlu dijawab bila memilih A:
Resep yang sudah memiliki charge dan kemudian dibatalkan diperlakukan bagaimana?
```

---

## Keputusan 2 — Jumlah penanggung dalam satu kunjungan

Bukti lengkap ada pada [RJ-BIL-CONFLICT-001-source-audit.md](RJ-BIL-CONFLICT-001-source-audit.md).
Bagian ini hanya menyajikan pertanyaannya.

### Dua temuan yang mengubah bentuk pertanyaan

**Temuan pertama: kemampuan banyak penanggung pernah ada, lalu dihapus.**

Migration `20260712123508` menghapus `44` kolom dari tabel penjamin encounter, termasuk persentase
coverage, jumlah co-payment, jumlah deductible, penanda penjamin utama, dan peran penjamin.
Alasan penghapusan tidak tercatat di source.

Karena itu pertanyaan pembuka kepada owner sebaiknya bukan *"apakah kita butuh banyak
penanggung?"* melainkan:

> **Mengapa dulu kemampuan itu dihapus, dan apakah alasan tersebut masih berlaku hari ini?**

**Temuan kedua: pembagian dua pihak sudah berjalan.**

Sistem sudah menghitung dan menyimpan bagian asuransi dan bagian pasien. Contohnya pada tindakan
dan resep, angka `CoveredAmount` dan `PatientPayAmount` sudah tersimpan. Jadi yang benar-benar
belum ada bukan "bagian pasien", melainkan **lebih dari satu penanggung sekaligus**.

Bedanya penting. Kalau tagihan Rp10.000.000 dibagi menjadi asuransi Rp8.000.000 dan pasien
Rp2.000.000, itu **sudah bisa** hari ini. Kalau dibagi menjadi Asuransi A Rp6.000.000, Asuransi B
Rp2.500.000, dan pasien Rp1.500.000, itu **tidak bisa** hari ini.

### Yang tidak boleh dicampur: banyak penanggung dan banyak cara bayar

Dua hal ini sering tertukar dalam pembicaraan, padahal berbeda:

| Istilah | Arti | Contoh | Keadaan sistem |
|---|---|---|---|
| Banyak penanggung (*multi-payer*) | Beberapa **pihak** menanggung satu tagihan | Asuransi A + Asuransi B + pasien | Tidak bisa |
| Banyak cara bayar (*split payment*) | **Satu pihak** membayar dengan beberapa metode | Pasien bayar Rp1.000.000 tunai + Rp1.500.000 kartu debit | Sebagian; master metode bayar ada, tetapi kunjungan hanya menyimpan satu metode |

Mohon dipastikan pertanyaan mana yang sebenarnya sedang dijawab.

### Tujuh pertanyaan

Nomor pertanyaan mengikuti audit agar mudah dirujuk.

#### `RJ-BIL-OQ-001` — Apakah satu kunjungan boleh punya lebih dari satu penanggung?

Keadaan sekarang: database mengunci satu kunjungan tepat satu sumber pembayaran.

```text
Pilihan A: Tetap satu penanggung utama per kunjungan; pembagian dilakukan saat penagihan.
  Akibat: pendaftaran, kiosk, dan laporan tidak berubah. Pekerjaan terpusat di Billing.
Pilihan B: Boleh beberapa penanggung sejak pendaftaran.
  Akibat: kunci database dilonggarkan, kontrak pendaftaran berubah, layar kiosk dan
  pendaftaran ikut berubah, laporan kategori pembayaran didefinisikan ulang.
Pilihan C: Tidak ada banyak penanggung sama sekali; satu kunjungan selalu satu penanggung.
  Akibat: RJ-BIL-BE-005 perlu ditinjau ulang karena judulnya menyebut allocation multi-payer.
Pilihan D: Other.

Pilihan dengan perubahan paling sedikit: A.
Ini pernyataan teknis tentang banyaknya perubahan, bukan rekomendasi kebijakan rumah sakit.
```

#### `RJ-BIL-OQ-002` — Bila dipakai, kapan pembagiannya ditentukan?

```text
Pilihan A: Saat pendaftaran.
Pilihan B: Saat pelayanan berlangsung.
Pilihan C: Saat penagihan atau finalisasi tagihan.
Pilihan D: Saat verifikasi klaim.
Pilihan E: Other.

Akibat: A menuntut perubahan kontrak pendaftaran dan kiosk.
C paling selaras dengan struktur sistem sekarang karena tidak menyentuh pendaftaran.
D memerlukan entitas klaim pasien yang saat ini belum ada sama sekali.
```

#### `RJ-BIL-OQ-003` — Apakah pasien otomatis menanggung sisanya?

Contoh: tagihan Rp10.000.000, coverage asuransi Rp8.000.000.

```text
Pilihan A: Rp2.000.000 otomatis menjadi tanggungan pasien.
Pilihan B: Perlu keputusan atau alokasi manual.
Pilihan C: Bergantung pada isi kontrak dengan penjamin.
Pilihan D: Other.

Akibat: A selaras dengan perhitungan yang sudah berjalan sekarang.
B dan C menuntut penyimpanan siapa yang memutuskan dan atas dasar apa.
```

#### `RJ-BIL-OQ-004` — Apakah dua asuransi boleh membayar satu kunjungan yang sama?

**Ini pertanyaan yang paling menentukan.** Bila jawabannya "tidak pernah terjadi", sebagian besar
dampak teknis gugur dan `RJ-BIL-BE-005` menyusut menjadi pekerjaan yang jauh lebih kecil.

Latar belakang: peninggalan kode memuat daftar peran penjamin `Primary`, `Secondary`, `Tertiary`,
`ExcessPayer`, `CoPaymentPayer`, dan `Backup`. Tidak satu pun dipakai oleh kode mana pun.

Contoh kasus yang perlu dinilai: Pak Budi datang dengan BPJS, dan juga memiliki asuransi swasta
dari perusahaannya. Tagihan Rp10.000.000. BPJS menanggung Rp6.000.000, asuransi swasta menanggung
Rp3.000.000 sebagai penjamin kedua, pasien membayar Rp1.000.000.

```text
Pertanyaan: Apakah kasus seperti Pak Budi nyata terjadi di rumah sakit ini?

Pilihan A: Nyata terjadi dan harus didukung sistem.
Pilihan B: Tidak pernah terjadi; peninggalan kode boleh dibersihkan.
Pilihan C: Terjadi, tetapi ditangani manual di luar sistem.
Pilihan D: Other.

Akibat: B membuat konflik ini menyusut drastis.
C perlu diikuti penjelasan bagaimana hasil penanganan manual dicatat kembali ke sistem.
```

#### `RJ-BIL-OQ-005` — Pembagian berlaku pada tingkat apa?

```text
Pilihan A: Persentase atas total tagihan.
Pilihan B: Per item — misalnya laboratorium ke Asuransi A, obat ke Asuransi B,
  administrasi ke pasien.
Pilihan C: Kombinasi keduanya.
Pilihan D: Other.

Akibat: A cukup ditangani pada tingkat folio dan lebih sederhana.
B menuntut alokasi pada tingkat baris tagihan, sehingga menyentuh struktur yang baru
dibangun pada RJ-BIL-BE-001.
```

#### `RJ-BIL-OQ-006` — Apakah penanggung boleh berubah setelah kunjungan dibuat?

Contoh: pasien terdaftar sebagai Tunai, lalu ditemukan kartu asuransi yang masih aktif.

```text
Pilihan A: Tidak boleh berubah setelah kunjungan dibuat.
Pilihan B: Boleh berubah, dan seluruh tagihan yang sudah terbentuk ikut berpindah.
Pilihan C: Boleh berubah, tetapi hanya berlaku untuk tagihan berikutnya.
Pilihan D: Other.

Akibat: B bertentangan dengan invariant #4 decision log yang melarang penghapusan
riwayat keuangan, sehingga kemungkinan memerlukan mekanisme koreksi bernomor versi,
bukan penulisan ulang.
```

#### `RJ-BIL-OQ-007` — Bagaimana hubungannya dengan piutang?

Catatan penting: sistem saat ini **tidak memiliki** entitas invoice, piutang, maupun klaim
pasien. `TrxBenefitClaim` dan `TrxExpenseClaim` yang ada adalah milik modul Sumber Daya Manusia,
bukan klaim pasien.

```text
Pilihan A: Piutang terpisah per penanggung.
Pilihan B: Satu piutang dengan rincian per penanggung.
Pilihan C: Piutang tidak dikelola sistem ini.
Pilihan D: Other.

Akibat: A dan B sama-sama memerlukan lapisan yang saat ini belum ada sama sekali.
C membatasi cakupan modul secara signifikan.
```

---

## Urutan menjawab

Tidak semua pertanyaan berbobot sama. Bila waktu terbatas, dua pertanyaan berikut sudah cukup
untuk membuka pekerjaan berikutnya.

| Prioritas | Pertanyaan | Alasan |
|---:|---|---|
| 1 | `RJ-BIL-OQ-004` | Jawaban `B` membuat lima pertanyaan lain menjadi jauh lebih ringan atau tidak relevan |
| 2 | Pertanyaan 1A | Keputusan paling murah dengan penurunan risiko paling besar; tidak ada layar yang terdampak |
| 3 | `RJ-BIL-OQ-001` | Menentukan apakah perubahan menyentuh pendaftaran dan kiosk, atau cukup di Billing |
| 4 | `RJ-BIL-OQ-002`, `OQ-005` | Menentukan bentuk teknis alokasi pada `RJ-BIL-BE-005` |
| 5 | Pertanyaan 1B | Memerlukan desain, bukan sekadar persetujuan |
| 6 | `RJ-BIL-OQ-003`, `OQ-006`, `OQ-007` | Dapat menyusul; tidak menahan permulaan `RJ-BIL-BE-005` |

---

## Formulir jawaban

Silakan isi kolom `Jawaban owner`. Kolom `Pemilik` boleh dikoreksi bila keliru.

| ID | Pertanyaan singkat | Pemilik | Jawaban owner | Catatan |
|---|---|---|---|---|
| `1A` | Empat endpoint pembayaran resep boleh dinonaktifkan? | Pharmacy + Billing/Payer | | |
| `1B` | Bagaimana pembatalan resep seharusnya bekerja? | Pharmacy + Billing/Payer | | |
| `RJ-BIL-OQ-001` | Satu kunjungan boleh lebih dari satu penanggung? | Registration + Billing/Finance | | |
| `RJ-BIL-OQ-002` | Kapan pembagian ditentukan? | Billing/Finance | | |
| `RJ-BIL-OQ-003` | Pasien otomatis menanggung sisa? | Billing/Finance | | |
| `RJ-BIL-OQ-004` | Dua asuransi pada satu kunjungan — nyata terjadi? | Registration + Payer | | |
| `RJ-BIL-OQ-005` | Pembagian per total atau per item? | Billing/Finance | | |
| `RJ-BIL-OQ-006` | Penanggung boleh berubah setelah kunjungan dibuat? | Registration + Billing/Finance | | |
| `RJ-BIL-OQ-007` | Piutang terpisah per penanggung? | Finance | | |

Pertanyaan tambahan di luar daftar, bila owner bersedia menjawab:

| ID | Pertanyaan | Jawaban owner |
|---|---|---|
| `T-01` | Mengapa `44` kolom penjamin dihapus pada migration `20260712123508`, dan apakah alasannya masih berlaku? | |

---

## Apa yang terbuka setelah dijawab

| Jawaban yang masuk | Yang menjadi mungkin |
|---|---|
| Pertanyaan 1A dijawab `A` | Bagian A `RJ-BIL-BE-002` dapat direncanakan; penonaktifan empat endpoint masuk cakupan |
| Pertanyaan 1A dan 1B dijawab | `RJ-BIL-BE-002` keluar dari status `BLOCKED` |
| `RJ-BIL-OQ-004` dijawab `B` | `RJ-BIL-BE-005` menyusut menjadi pemindahan kepemilikan angka dari modul klinis ke Billing |
| `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005` dijawab | Bentuk teknis alokasi dapat dirancang; `RJ-BIL-BE-005` keluar dari status `BLOCKED` |
| Tidak ada jawaban | `RJ-BIL-BE-003` atau `RJ-BIL-BE-004` tetap dapat dimulai; keduanya tidak bergantung pada kedua konflik ini, tetapi memerlukan owner Laboratorium atau Radiologi beserta Clinical Governance |

## Batas dokumen ini

Dokumen ini hanya mengumpulkan pertanyaan dan bukti. Mengisinya tidak dengan sendirinya:

- mengubah status task pada roadmap;
- memberi wewenang mengubah source code;
- memberi wewenang menjalankan migration atau mengubah database;
- mengaktifkan adapter payer eksternal `RJ-BIL-DEP-009`, yang tetap `INACTIVE`.

Setiap task tetap memerlukan handoff tersendiri beserta wewenang tulis pada saat eksekusi.

## Rujukan

| Dokumen | Isi |
|---|---|
| [RJ-BIL-CONFLICT-001-source-audit.md](RJ-BIL-CONFLICT-001-source-audit.md) | Bukti lengkap Keputusan 2, termasuk `20` baris bukti source dan enam konflik terkonfirmasi |
| [MODULE-STATUS.md](MODULE-STATUS.md) | Bukti lengkap Keputusan 1 dan keadaan modul terkini |
| [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) | Cakupan dan acceptance criteria `RJ-BIL-BE-002` serta `RJ-BIL-BE-005` |
| [01-requirement-completeness-gate.md](01-requirement-completeness-gate.md) | Arah kebijakan compatibility boundary dan deprecation bertahap |
