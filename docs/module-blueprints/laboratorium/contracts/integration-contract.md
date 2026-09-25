# Integration Contract — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-INT-v1` |
| Revision | **`5` — `approved`** 2026-09-25, bagian 9 (`INT-07` kode Mikrobiologi, `INT-08` Mikrobiologi) — disetujui Yoga Aji Pratama. Sebelumnya: **`4` — `approved`** 2026-09-25, bagian 8 (`INT-07`, `INT-08`, `S4`). Sebelumnya: `3` |
| Status | `approved` — dikunci 2026-09-02; **amandemen `r4` disetujui 2026-09-25** (bagian 8); **amandemen `r5` disetujui 2026-09-25** (bagian 9) |
| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02; `r4`: Yoga Aji Pratama / 2026-09-25; `r5`: Yoga Aji Pratama / 2026-09-25 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
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

### Bentuk teknis — ditetapkan 2026-09-04 (`BE-EXT-03`)

Tempat penyimpanan penunjuknya **sudah ada**: `TrxPatientEncounter.ReferralInstitutionId` dan
`TrxPatientEncounter.ReferralDoctorId`, keduanya boleh kosong dan bertaut `Restrict` ke
`MstReferralInstitution` serta `MstReferralDoctor`.

| Ruas permintaan | Dipetakan ke |
|---|---|
| `patientId` atau identitas pasien baru | `TrxPatientEncounter.PatientId` |
| `serviceUnitId` | `ServiceUnitId` |
| `isWalkIn` | `IsWalkIn` dan `RegistrationSource = WalkIn` |
| `isReferral`, `referralNumber` | `IsReferral`, `ReferralNumber` |
| `referralInstitutionId` | `ReferralInstitutionId` |
| `referralDoctorId` | `ReferralDoctorId` |
| `paymentType` beserta penunjuk penjaminnya | Penjamin kunjungan |
| `idempotencyKey` | Tidak disimpan pada kunjungan; dipakai Registrasi mengenali pengiriman ulang |

| Ruas jawaban | Dipakai Laboratorium untuk |
|---|---|
| `encounterId` | Diisikan ke `LabOrder.EncounterId` |
| `encounterNumber` | Ditampilkan kepada petugas |
| `patientId` | Menampilkan identitas pada layar berikutnya |

### Pelaksana — tersedia sejak 2026-09-07 (`BE-LAB-08`)

Jalur pemanggilannya **sudah ada**, dan ia milik Registrasi:
`Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`.

| Aspek | Bentuk yang dipilih |
|---|---|
| Sifat pemanggilan | **Dalam proses**, bukan HTTP. Sama seperti `INT-02`, `INT-04`, dan `INT-06` yang juga sejalur-proses. Laboratorium memanggil service milik Registrasi, bukan menulis ke tabelnya |
| Penyimpanan kunci idempotensi | Kolom `TrxPatientEncounter.RegistrationIdempotencyKey`, boleh kosong, ber-**unique index tersaring** `"RegistrationIdempotencyKey" IS NOT NULL` |
| Penegak idempotensi | Dua lapis: pembacaan lebih dulu menangani pengiriman ulang berurutan; unique index menangani dua permintaan yang tiba bersamaan |
| Kewenangan | Registrasi memeriksa sendiri `PatientEncounter : Create` atas pemanggilnya. Hak akses layar Laboratorium hanya membuka layarnya (`VAL-41`) |

**Kenapa kuncinya tidak ditaruh pada tabel tersendiri.** Yang perlu dikenali saat permintaan
yang sama datang dua kali adalah *kunjungan mana* yang sudah terbentuk. Menyimpannya pada
kunjungan membuat pengenalan itu satu pembacaan index, dan membuat basis data sendiri yang
menolak kunjungan kedua — bukan kode aplikasi yang harus memenangkan balapan.

**Satu selisih yang perlu diketahui.** `VAL-42` — *"Registrasi tidak dapat dihubungi"* — ditulis
untuk pemanggilan jarak jauh. Karena pemanggilannya sejalur proses, keadaan itu berubah makna
menjadi *penyimpanannya* yang tidak dapat dicapai; kegagalan basis data dipetakan menjadi `503`
beserta pesan yang sama.

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

---

## 8. Amandemen `r4` — Validasi dan rilis hasil Patologi Klinik (`S4`), 2026-09-25

| Field | Nilai |
|---|---|
| `contract_version` | `LAB-INT-v1` |
| Revision | `r4` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-25 — instruksi *"Setujui kelima kontrak beserta 10 butir itu dan lanjut ke /quilvian-engineering-skills:plan-module-delivery"*, lihat `LAB-API-v1` `r34` bagian 29 |
| `input_revision` | decisions rev 74; `LAB-DA-001` rev 8 bagian A5.10; `02-backend-architecture.md` rev 10 bagian 20 |
| Keputusan | `LAB-DEC-017`, `LAB-DEC-148`; koordinasi `LAB-COORD-002` (**closed**) dan `LAB-COORD-016` (**terbuka**) |
| Sifat | **Aditif.** Dua batas baru. `INT-01`..`INT-06` tidak berubah |
| Mengubah bagian 6 | Baris **"Rekam medis"** pada bagian 6 **dicabut untuk Patologi Klinik** begitu revisi ini disetujui — digantikan `INT-08`. Teks bagian 6 dibiarkan sebagai jejak revisi 3 |

### 8.1 Ringkasan batas yang ditambahkan

| ID | Arah | Sifat | Sumber kebenaran |
|---|---|---|---|
| `INT-07` | Laboratorium → Human Resource | **Baca** penunjukan kewenangan klinis, sinkron, dalam proses | Human Resource |
| `INT-08` | Laboratorium → Rekam Medis | **Pendaftaran** dokumen hasil saat rilis, sinkron, dalam proses, **satu transaksi** | Laboratorium atas angka hasil; Rekam Medis atas keutuhan dokumen |

Keduanya **sejalur-proses**, sama dengan `INT-02`, `INT-04`, `INT-05`, dan `INT-06`: Laboratorium
membaca tabel atau memanggil service milik modul lain di dalam aplikasi yang sama, bukan lewat
HTTP.

### 8.2 `INT-07` — Laboratorium membaca penunjukan kewenangan dari Human Resource

| Aspek | Isi |
|---|---|
| Tujuan bisnis | Menjawab *"apakah orang ini ditunjuk memvalidasi atau merilis hasil Patologi Klinik hari ini"* — lapis orang `LAB-DEC-142` |
| Produsen | Human Resource — `WfpClinicalPrivilege` (`Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpClinicalPrivilege.cs`), dengan katalog kode `MstClinicalPrivilegeCatalog` |
| Konsumen | `LabClinicalPrivilegeResolver` (`Areas/HealthServices/LaboratoryManagement/Services/LabClinicalPrivilegeResolver.cs`) |
| Arah | **Satu arah, baca saja.** Laboratorium **nol menulis** ke tabel Human Resource dalam bentuk apa pun (`LAB-DEC-148`, `AC-232`) |
| Kapan dibaca | **Setiap kali** validasi, rilis, atau pengembalian ditekan — **tanpa salinan tetap** dan **tanpa cache**. Penunjukan yang ditangguhkan pukul 10.00 menolak validasi pukul 10.01 |
| Kunci pencocokan | `ApplicationUser.WorkforceProfileId` → `WfpClinicalPrivilege.WorkforceProfileId`, **ditambah** `PrivilegeCode` sama persis dengan `LabClinicalPrivilegeCodes` |
| Kode yang dipakai `S4` | Dua: validasi dan rilis **Patologi Klinik**. **Usulan** `LAB-VAL-PK` dan `LAB-REL-PK`; nilai finalnya **disepakati pemilik Human Resource lewat `LAB-COORD-016`**. Empat kode lain (Mikrobiologi, Patologi Anatomi) milik `S4d`/`S4e` |
| Aturan menilai | Lihat `02-backend-architecture.md` 20.4 `LabClinicalPrivilegeResolver` dan `LAB-VAL-v1` `r12` 14.2 |
| Idempotency | Tidak relevan — pembacaan murni |
| **Saat gagal** | **Fail-closed.** Akun tanpa `WorkforceProfileId`, nol baris, atau baris tidak berlaku → `403` dengan sebab terbaca. Pembacaan yang gagal karena galat basis data → tindakan **tidak dilakukan**, `503`. **Nol jalur pintas** — berbeda sengaja dari `OperatingRoomCredentialResolver`, yang melaporkan data kosong sebagai `NotAvailable` dan **tidak memblokir** |
| Rekonsiliasi | Tidak dibutuhkan — nol salinan. Dasar kewenangan yang **dipakai** saat bertindak disimpan sebagai penunjuk `ValidatedByPrivilegeId`/`ReleasedByPrivilegeId` (usulan `ARCH-GAP-LAB-08` butir 2), supaya auditor dapat membuka baris penunjukan itu kelak |
| Ketergantungan yang belum selesai | `LAB-COORD-016` — dua kode di katalog. **Tanpanya, setiap validasi ditolak `NotAppointed`**, dan itu perilaku yang benar |

**Contoh.** dr. Contoh memegang baris `LAB-VAL-PK`, berstatus `Active`, berlaku 1 Oktober 2026
sampai 30 September 2027. Pada 15 Oktober ia memvalidasi — diterima, dan
`ValidatedByPrivilegeId` menunjuk baris itu. Pada 20 Oktober bagian SDM menangguhkannya karena
Surat Izin Praktiknya habis; pukul 10.01 hari itu validasinya ditolak *"Penunjukan validasi
Patologi Klinik Anda sedang ditangguhkan."* Validasi 15 Oktober **tidak berubah** — snapshot, bukan
penunjuk hidup (`A5.6`).

### 8.3 `INT-08` — Laboratorium mendaftarkan dokumen hasil ke Rekam Medis saat rilis

| Aspek | Isi |
|---|---|
| Tujuan bisnis | Hasil laboratorium yang dirilis menjadi bagian berkas rekam medis pasien — siapa penulisnya, kapan ditandatangani, kapan terkunci (`LAB-DEC-017`, `LAB-REQ-001` bagian 4.3) |
| Produsen | Laboratorium, pada tindakan rilis |
| Konsumen | Rekam Medis — `ClinicalDocumentIntegrityService.RegisterSignedAsync` (`Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs:200-234`) |
| Yang dikirim | `DocumentKind = LaboratoryResult` (**nilai baru `14`**, disepakati `LAB-COORD-002`), `DocumentId = LabExamination.Id`, `PatientId` dari kunjungan order, `EncounterId = LabOrder.EncounterId`, penulis sekaligus penanda tangan (**usulan: perilis**, `02-backend-architecture.md` 20.10 butir 6), perangkat dan alamat jaringan pemanggil, waktu rilis |
| Yang **tidak** dikirim | **Angka hasil.** Isi hasil tetap di tabel Laboratorium; rekam medis mencatat keutuhannya saja |
| Satuan | **Satu dokumen per pemeriksaan yang dirilis.** Order berisi 18 pemeriksaan yang dirilis bertahap menghasilkan 18 baris keutuhan, masing-masing pada saat rilisnya (`LAB-DEC-008`) |
| Sinkron atau asinkron | **Sinkron, dalam satu `SaveChangesAsync` dengan rilis.** `RegisterSignedAsync` sengaja tidak menyimpan sendiri, sehingga rilis dan pendaftaran **berhasil bersama atau gagal bersama** — usulan `ARCH-GAP-LAB-08` butir 3 |
| Idempotency | Index unik `(DocumentKind, DocumentId)` milik Rekam Medis. `RegisterSignedAsync` juga mengembalikan baris yang sudah ada tanpa tanda tangan kedua. Rilis kedua atas hasil yang sama sudah ditolak lebih dulu oleh `VAL-133` |
| **Saat gagal** | Seluruh rilis **dibatalkan** — nol `ReleasedAt`, nol baris riwayat, nol dokumen — dan pengguna menerima `422` `VAL-137` beserta sebabnya. Hasil **tetap tervalidasi** dan dapat dirilis ulang. Pola ini menyalin `ConsultationFinalizationService.cs:180-203` milik Farmasi |
| Rekonsiliasi | Tidak dibutuhkan pada keadaan normal: satu transaksi tidak dapat menghasilkan rilis tanpa dokumen. **Satu pemeriksaan yang layak dijalankan sesudah rilis pertama:** jumlah pemeriksaan ber-`ReleasedAt` harus sama dengan jumlah baris `LaboratoryResult` — selisih apa pun berarti ada jalur rilis yang melewati service ini |
| Kunjungan yang sudah ditutup | Pendaftaran **tetap berjalan** — `RegisterSignedAsync` tidak menolak kunjungan tertutup, dan dokumennya langsung tertanda tangan dan terkunci. Hasil pasien rawat jalan yang keluar sesudah pasien pulang adalah keadaan yang lazim |
| Yang **tidak** diubah pada Rekam Medis | Himpunan `JenisYangDitegakkan` (`:81-87`). Penegakan `EnsureMutableAsync` baru dibutuhkan koreksi `S6`, dan perubahannya milik pemilik rekam medis |

**Contoh.** Kalium 7,2 dirilis pukul 02.14. Pada `SaveChangesAsync` yang sama tersimpan: kolom rilis
pada `LabExamination`, satu baris `LabTransitionHistory` `ReleaseResult`, dan satu baris
`MrcClinicalDocumentIntegrity` berjenis `LaboratoryResult`, berstatus `Signed`, `LockedAt` 02.14,
pemicu kunci `AuthorSigned`. Bila baris ketiga gagal karena `EncounterId` tidak sah, ketiganya
batal, dan perilis membaca *"Hasil tidak dapat dirilis karena pendaftaran ke rekam medis gagal:
Id kunjungan tidak valid. Hasil tetap tervalidasi."*

### 8.4 Traceability `r4`

| Batas | Decision ID | Arsitektur domain | Acceptance criteria |
|---|---|---|---|
| `INT-07` | `LAB-DEC-148`, `LAB-DEC-142`, `LAB-DEC-143`, `LAB-DEC-150` | `LAB-DC-055`, `LAB-DC-056`, A5.10 | `AC-229`..`AC-233` |
| `INT-08` | `LAB-DEC-017` | `LAB-DC-057`, `INV-50`, A5.10 | — belum ada AC di decision log; diuji lewat baris `INT-08` pada `testing/acceptance-test-matrix.md` amandemen 2026-09-25 |

---

## 9. Amandemen `r5` — Validasi dan rilis hasil Mikrobiologi (`S4d-1`), 2026-09-25

| Field | Nilai |
|---|---|
| `contract_version` | `LAB-INT-v1` |
| Revision | `r5` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-25 — instruksi *"Setujui keempat kontrak beserta lima butir di atas, termasuk perubahan bunyi VAL-126, lalu jalankan /plan-module-delivery untuk MVP-10a sampai MVP-10c"*, lihat `LAB-API-v1` `r35` bagian 30. **Kedua kode `LAB-VAL-MB`/`LAB-REL-MB` disetujui sebagai usulan pemilik modul**; nilai finalnya tetap lewat `LAB-COORD-016` bersama pemilik Human Resource |
| `input_revision` | decisions rev 76; `LAB-DA-001` rev 9 bagian A6.10; `02-backend-architecture.md` rev 11 bagian 21 |
| Sifat | **Aditif.** `INT-07` memakai dua kode lagi; `INT-08` melayani satu disiplin lagi |

### 9.1 `INT-07` — kode yang dipakai bertambah

| Kode | Disiplin | Tindakan | Keadaan |
|---|---|---|---|
| `LAB-VAL-PK` | Patologi Klinik | Validasi | Usulan `r4` — final lewat `LAB-COORD-016` |
| `LAB-REL-PK` | Patologi Klinik | Rilis | Sama |
| **`LAB-VAL-MB`** | **Mikrobiologi** | Validasi | **Usulan `r5`** — final lewat `LAB-COORD-016` |
| **`LAB-REL-MB`** | **Mikrobiologi** | Rilis | Sama |

**Kode dipilih menurut disiplin order pemeriksaan**, bukan menurut jabatan pelaku. Aturan menilai
baris, fail-closed, dan larangan menulis ke Human Resource **tidak berubah** dari 8.2. Dua kode
Patologi Anatomi tetap milik `S4e`.

**Contoh.** dr. Contoh memegang `LAB-VAL-PK` aktif. Ia memvalidasi kultur urin → resolver
mencari `LAB-VAL-MB` atas namanya, menemukan nol baris, dan menolak *"belum ditunjuk sebagai
pemegang kewenangan validasi Mikrobiologi"* — walaupun ia dokter berkewenangan laboratorium yang
sah bagi Patologi Klinik (`LAB-DEC-152`, `AC-241`).

### 9.2 `INT-08` — pendaftaran dokumen Mikrobiologi

Bagian 8.3 berlaku apa adanya: **satu** dokumen `LaboratoryResult` per pemeriksaan Mikrobiologi
yang dirilis, `DocumentId` = id pemeriksaan, atomik dengan rilisnya. Isolat dan antibiogram
**tidak** didaftarkan terpisah — mereka bagian isi hasil yang tetap tinggal di Laboratorium.
**Rilis kedua atas pemeriksaan yang sama tidak ada** pada `S4d-1`; bentuk dokumen bagi hasil
bertahap menunggu `DEC-LAB-020`.

### 9.3 Traceability `r5`

| Batas | Decision ID | Acceptance criteria |
|---|---|---|
| `INT-07` kode Mikrobiologi | `LAB-DEC-143`, `LAB-DEC-148`, `LAB-DEC-152` | `AC-231`, `AC-241` |
| `INT-08` Mikrobiologi | `LAB-DEC-017` | Baris `INT-08` matriks uji, disiplin Mikrobiologi |
