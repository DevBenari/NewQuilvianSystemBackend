# Integration Contract — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-INT-v1` |
| Revision | `3` |
| Status | `approved` — dikunci 2026-09-02 |
| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:75d285252aa5bce7fcaf5d90242da0d30fbd58a92a16aca3377683243be45f61` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

---

## 1. Ringkasan Batas Integrasi

| ID | Batas | Sifat | Status |
|---|---|---|---|
| `INT-01` | Laboratorium → Billing | Penerbitan fakta, satu arah | **Sudah ada dan berjalan**, diperbarui satuannya |
| `INT-05` | Laboratorium → Registrasi | **Permintaan pembuatan kunjungan**, sinkron | **Baru** (`LAB-DEC-032`, `LAB-DEC-035`) |
| `INT-06` | Laboratorium → Data Induk | Baca katalog, harga, dan cakupan penjamin | **Baru** (`LAB-DEC-033`) |
| `INT-02` | Laboratorium → Registrasi | Pembacaan langsung | Sudah ada |
| `INT-03` | Laboratorium → Data Induk | Pembacaan langsung dan penyalinan sesaat | Sudah ada |
| `INT-04` | Laboratorium → Platform | Pemeriksaan kewenangan | Sudah ada |
| — | Alat laboratorium | **Tidak ada** | Dikecualikan `LAB-DEC-005` |
| — | Pemberitahuan ke dokter | **Tidak ada pada rilis ini** | Terblokir `LAB-COORD-001` |
| — | Rekam medis | **Tidak ada pada rilis ini** | Terblokir `LAB-COORD-002` |

---

## 2. `INT-01` — Laboratorium ke Billing

### Identitas

| Aspek | Isi |
|---|---|
| Produsen | `BC-LAB` Operasional Laboratorium |
| Konsumen | `BC-BIL` Billing dan Kasir |
| Mekanisme | Pemanggilan langsung `ClinicalMilestoneFactProducer` di dalam transaksi yang sama |
| Berkas bukti | `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs@c87d9c0` |
| Sumber kebenaran | `BC-LAB` untuk keadaan operasional; `BC-BIL` untuk seluruh akibat uang |
| Arah | Satu arah |
| Sinkron atau asinkron | **Sinkron**, di dalam transaksi yang sama dengan perpindahan status |

### Kejadian yang diterbitkan

| Kejadian | Jenis fakta | Pemicu | Satuan |
|---|---|---|---|
| Kelayakan tagih | `ChargeEligibility` | Wadah berpindah ke `Accepted` | **Satu fakta per pemeriksaan** yang ditopang wadah itu |
| Pembatalan klinis | `ClinicalCancellation` | Pembatalan pemeriksaan atau pesanan yang pemeriksaannya pernah layak tagih | Satu fakta per pemeriksaan |

### Perubahan satuan akibat `LAB-DEC-024`

| Sebelum | Sesudah |
|---|---|
| Satu fakta per baris sampel | Satu fakta per baris pemeriksaan |
| `SourceItemId` menunjuk identitas sampel | `SourceItemId` menunjuk identitas pemeriksaan |
| Satu wadah selalu menghasilkan satu fakta | Satu wadah menghasilkan sebanyak pemeriksaan yang ditopangnya |

**Yang tidak berubah:** titik pemicunya tetap pada wadah dinyatakan layak, sesuai
`LAB-INH-009`. Isi faktanya tetap kejadian dan salinan tarif, tanpa keputusan tagihan.

### Isi fakta

| Ruas | Isi | Boleh kosong |
|---|---|:---:|
| Jenis fakta | Kelayakan tagih atau pembatalan klinis | Tidak |
| Konteks sumber | `Laboratory` | Tidak |
| Identitas pemeriksaan | Identitas baris `LabExamination` | Tidak |
| Kunjungan pasien | `EncounterId` | Tidak |
| Jumlah | `1` untuk kelayakan tagih | Ya, pada pembatalan |
| Satuan | Satuan pemeriksaan | Ya, pada pembatalan |
| Salinan tarif | Kode tarif dan harga saat kejadian | Ya, pada pembatalan |
| Waktu kejadian | Waktu wadah dinyatakan layak | Tidak |
| Pelaku | Petugas yang memutuskan | Tidak |
| Penanda milestone | `SpecimenAccepted` atau `SpecimenCancelled` | Tidak |

**Yang dilarang keras ada di dalam fakta:** keputusan tagihan, status pembayaran, potongan,
pajak, pembatalan tagihan, refund, dan pembalikan. Dilarang `LAB-INH-010` dan `LAB-INH-012`,
dan sudah dijaga pengujian `LaboratoryAuthorityTests.cs@c87d9c0`.

### Idempotensi

| Aspek | Perilaku |
|---|---|
| Kunci idempotensi | Kombinasi identitas pemeriksaan dan jenis fakta |
| Menyatakan layak berulang | Dikembalikan hasil yang sama; **tidak** menerbitkan fakta kedua |
| Bukti yang sudah ada | Diuji `#PenetapanLayakDiulang_TidakMenggandakanTagihan@c87d9c0` |

### Perilaku saat gagal

Status penyaluran fakta memakai nilai yang sudah ada pada
`ClinicalMilestoneFactEnums.cs@c87d9c0`:

| Status | Arti | Tindakan yang diperlukan |
|---|---|---|
| `Pending` | Fakta terbentuk, belum disalurkan | Menunggu penyaluran |
| `Dispatched` | Billing sudah menerima | Selesai |
| `Rejected` | Billing menolak fakta | Perlu ditinjau manusia |
| `OutcomeUnknown` | Hasil penyaluran tidak diketahui | **Wajib direkonsiliasi.** Bukan berhasil, bukan gagal |
| `SuppressedNoPriorCharge` | Pembatalan atas sesuatu yang belum pernah ditagihkan | Tidak ada tindakan |

**Aturan penting.** `OutcomeUnknown` **tidak boleh** diperlakukan sebagai berhasil maupun
gagal. Fakta bertanda itu wajib muncul pada daftar rekonsiliasi sampai keadaannya jelas.

### Rekonsiliasi

| Aspek | Isi |
|---|---|
| Yang direkonsiliasi | Seluruh fakta berstatus `Pending`, `OutcomeUnknown`, dan `Rejected` |
| Siapa yang meninjau | Petugas Billing bersama kepala instalasi laboratorium |
| Yang tidak boleh dilakukan Laboratorium | Mengubah, membatalkan, atau menerbitkan ulang tagihan. Laboratorium hanya dapat menerbitkan ulang **fakta**, bukan akibatnya |

### Contoh

> Wadah tabung serum pasien Andi menopang Fungsi hati Rp150.000 dan Fungsi ginjal Rp120.000.
> Budi menyatakan wadah itu layak pukul 08.25.
>
> Yang terjadi: dua fakta kelayakan tagih terbit dalam satu transaksi — satu menunjuk
> pemeriksaan Fungsi hati dengan salinan tarif Rp150.000, satu menunjuk Fungsi ginjal dengan
> Rp120.000. Laboratorium tidak menjumlahkan keduanya, tidak membuat tagihan, dan tidak tahu
> apakah pasien akhirnya membayar.
>
> Bila Budi menekan tombol yang sama dua kali karena jaringan lambat, fakta **tetap dua**,
> bukan empat.

---

## 2b. `INT-05` — Laboratorium meminta Registrasi membuat kunjungan

| Aspek | Isi |
|---|---|
| Peminta | `BC-LAB` Operasional Laboratorium |
| Pelaksana | `BC-REG` Registrasi |
| Tujuan bisnis | Memberi kunjungan kepada pasien yang datang langsung ke laboratorium atau dikirim institusi luar |
| Sumber kebenaran | `BC-REG`. Laboratorium hanya menyimpan penunjuk hasilnya |
| Arah | Permintaan dan jawaban, satu kali jalan |
| Sinkron atau asinkron | **Sinkron.** Pesanan lab tidak dapat dibuat sebelum kunjungan ada |
| Dasar keputusan | `LAB-DEC-032`, `LAB-DEC-035` |

### Yang dikirim Laboratorium

| Ruas | Wajib | Keterangan |
|---|:---:|---|
| Identitas pasien, atau penunjuk pasien yang sudah terdaftar | Ya | Bila pasien sudah terdaftar, cukup penunjuknya |
| Penanda datang langsung | Ya | Dipetakan ke `IsWalkIn` dan sumber pendaftaran `WalkIn` |
| Penanda rujukan | Ya untuk `S13b` | Dipetakan ke `IsReferral` |
| Nomor surat rujukan | Ya untuk `S13b` | Dipetakan ke `ReferralNumber` |
| Penunjuk instansi perujuk | Ya untuk `S13b` | Data induk global, **bukan teks bebas** (`LAB-DEC-035`) |
| Penunjuk dokter perujuk | Ya untuk `S13b` | Data induk global |
| Penjamin | Ya | Dipakai untuk menghitung cakupan pada `INT-06` |

### Yang dikembalikan Registrasi

| Ruas | Keterangan |
|---|---|
| Penunjuk kunjungan | Disimpan pada `LabOrder.EncounterId` |
| Nomor kunjungan | Ditampilkan kepada petugas |
| Penunjuk pasien | Untuk menampilkan identitas pada layar berikutnya |

### Idempotensi

| Aspek | Perilaku |
|---|---|
| Kunci idempotensi | Ditetapkan Laboratorium per percobaan pendaftaran, dikirim bersama permintaan |
| Petugas menekan simpan dua kali | **Tidak boleh** menghasilkan dua kunjungan untuk pasien yang sama |
| Bila kunci sama dikirim ulang | Registrasi mengembalikan kunjungan yang sama, bukan membuat yang baru |

**Kenapa ini wajib.** Pendaftaran ganda menghasilkan **dua kunjungan untuk satu pasien** pada
hari yang sama. Akibatnya pesanan lab terbelah, hasil tersebar di dua kunjungan, dan Billing
menerima dua konteks tagihan.

### Perilaku saat gagal

| Keadaan | Yang terjadi |
|---|---|
| Registrasi menolak karena kewenangan | Pendaftaran gagal. Penolakan diteruskan apa adanya kepada petugas |
| Registrasi menolak karena isian tidak lengkap | Pendaftaran gagal. Pesan dari Registrasi ditampilkan apa adanya |
| Registrasi tidak dapat dihubungi | Pendaftaran gagal seluruhnya |
| Apa pun kegagalannya | **Tidak ada** data yang disimpan Laboratorium. Tidak ada kunjungan setengah jadi, tidak ada pesanan yatim |

### Rekonsiliasi

Tidak diperlukan. Sifatnya sinkron, dan gagal berarti batal seluruhnya.

**Yang belum disepakati:** bentuk teknis permintaan dan jawaban. Dicatat sebagai
`LAB-COORD-003` dan `LAB-COORD-004`.

---

## 2c. `INT-06` — Laboratorium membaca katalog, harga, dan cakupan penjamin

| Aspek | Isi |
|---|---|
| Pembaca | `BC-LAB` |
| Pemilik | `BC-MD` Data Induk |
| Sifat | **Baca saja.** Tidak ada penulisan, tidak ada penyalinan tetap |
| Dasar keputusan | `LAB-DEC-029`, `LAB-DEC-033`, `LAB-DEC-036` |

| Yang dibaca | Dari | Untuk |
|---|---|---|
| Pemeriksaan berpenanda `IsLaboratory` beserta disiplinnya | `MstProcedure` | Daftar yang dapat dipesan, dan penegakan `INV-22` |
| Harga berlaku menurut unit, kelas pasien, dan tanggal | `MstTariff` | Kolom harga satuan |
| Kontrak penjamin | `MstInsuranceTariff` | Penanda tercakup atau tidak, dan harga kontrak |

### Perilaku saat data tidak ditemukan

| Keadaan | Yang terjadi |
|---|---|
| Tidak ada kontrak penjamin untuk pemeriksaan itu | Ditampilkan **tidak tercakup**. Ini jawaban yang sah, bukan kesalahan |
| Tidak ada tarif berlaku pada tanggal kejadian | Pemeriksaan **tidak dapat** ditambahkan; petugas diarahkan ke Master Data |
| Pemeriksaan belum punya penanda disiplin | Tidak muncul pada daftar disiplin mana pun; diberi keterangan agar Master Data melengkapinya |

**Batas yang tegas.** Laboratorium **menampilkan** harga dan cakupan. Ia tidak menghitung
selisih, tidak menentukan siapa membayar, dan tidak menyimpan keputusan cakupan sebagai
kebenaran. Salinan tarif yang disimpan pada baris pemeriksaan adalah **jejak harga saat
kejadian**, bukan tagihan.

---

## 3. `INT-02` — Laboratorium ke Registrasi

| Aspek | Isi |
|---|---|
| Sifat | Pembacaan langsung `TrxPatientEncounter` |
| Yang dibaca | Identitas kunjungan, pasien, dokter, dan jenis unit |
| Yang **tidak** dilakukan | Menyalin, mengubah, atau menutup kunjungan |
| Kegagalan | Bila kunjungan tidak ditemukan, pembuatan pesanan ditolak `404` |

Berlaku untuk seluruh jenis kunjungan — Rawat Jalan, IGD, dan Rawat Inap — sesuai
`LAB-DEC-009`. Tidak ada perlakuan berbeda berdasarkan unit asal.

---

## 4. `INT-03` — Laboratorium ke Data Induk

| Aspek | Isi |
|---|---|
| Sifat | Pembacaan langsung, disertai penyalinan sesaat |
| Yang dibaca | `MstProcedure` beserta penanda `IsLaboratory`, tarif yang berlaku, `MstAgeCategory` |
| Yang disalin sesaat | Kode dan nama pemeriksaan, kode tarif, dan harga — disimpan pada `LabExamination` |
| Alasan penyalinan | Agar harga saat kejadian tetap dapat ditelusuri walaupun tarif induk berubah kemudian |
| Yang **tidak** dilakukan | Mengubah `MstProcedure`, menambah kolom padanya, atau membuat katalog tandingan |
| Kegagalan | Bila tarif tidak ditemukan, penambahan pemeriksaan ditolak `422` |

---

## 5. `INT-04` — Laboratorium ke Platform

| Aspek | Isi |
|---|---|
| Sifat | Pemeriksaan kewenangan per permintaan |
| Mekanisme | `AccessPermissionFilter` memanggil `AccessPermissionService.HasAccessAsync@c87d9c0` |
| Pendaftaran kewenangan | Otomatis lewat `AccessMenuSeeder@c87d9c0` saat aplikasi mulai, berdasarkan atribut pada controller |
| Kegagalan | Kewenangan tidak ada berarti `403`. Kewenangan yang belum terdaftar juga berarti `403`, bukan diizinkan |

**Konsekuensi yang harus diketahui implementer.** Controller baru **wajib** membawa
`[AccessController(...)]` dan `[AccessAction(...)]`, karena tanpa keduanya kewenangannya tidak
akan pernah terdaftar dan seluruh endpointnya akan menolak semua orang.

---

## 6. Integrasi yang Sengaja Tidak Ada

| Integrasi | Alasan | Kapan ditinjau ulang |
|---|---|---|
| Alat laboratorium | `LAB-DEC-005` menetapkan hasil diketik manual pada Rilis 1 | Saat slice hasil dibuka dan alat diadakan. Perhatikan `LAB-RISK-001` |
| Pemberitahuan ke dokter | `LAB-DEC-016` menetapkannya kemampuan platform yang belum ada | Setelah `LAB-COORD-001` disepakati |
| Rekam medis | `LAB-DEC-017` menetapkan pendaftaran dokumen, tetapi slicenya terblokir | Setelah `LAB-COORD-002` disepakati |
| Sistem luar rumah sakit | Tidak ada requirement | Bila muncul kebutuhan rujukan lab luar |

---

## 7. Traceability

| Batas | Decision ID | Acceptance criteria |
|---|---|---|
| `INT-01` | `LAB-INH-009` sampai `LAB-INH-012`, `LAB-DEC-024` | AC-12, AC-13, AC-37 |
| `INT-02` | `LAB-DEC-009` | AC-11 |
| `INT-03` | `LAB-DEC-018` | AC-25 |
| `INT-04` | `LAB-INH-007`, `LAB-DEC-019` | AC-26 |
