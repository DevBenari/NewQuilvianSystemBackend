# Laboratorium — Arsitektur Domain Rumah Sakit

## A. Identitas Arsitektur

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Architecture ID | `LAB-DA-001` |
| Revision | `9` — bagian **A6**, 2026-09-25: `S4d-1` validasi dan rilis Mikrobiologi. Sebelumnya `8` — bagian **A5**, 2026-09-24: `S4` validasi dan rilis Patologi Klinik |
| Status | `draft` |
| **Kesiapan arsitektur** | **Revision 9: `S4d-1` ditambahkan `DOMAIN_ARCHITECTURE_READY` untuk desain saja; `S4d-2` dan `S4e` tidak dirancang (`DEC-LAB-020`, `DEC-LAB-021`). Lihat A6.14.** **Revision 8: `S4` ditambahkan `DOMAIN_ARCHITECTURE_READY` untuk desain saja — pemakaian nyatanya tertahan `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016`. Lihat A5.14.** Sebelumnya: **`DOMAIN_ARCHITECTURE_READY`** — **12 slice siap.** `S4c` **dirancang ulang revision 7** sesudah bukti `LAB-EVD-003`; bentuk lamanya pada A3 dicabut, bentuk barunya pada **A4**. Jumlah slice tidak berubah; yang berubah isinya. **12 slice siap.** Sepuluh slice revision 2-4 tetap `READY`; revision 5 menambahkan `S4b` dan `S4c` ke dalam scope, dan revision 6 menaikkan `S4b` menjadi `READY` sesudah `LAB-DEC-084` menutup `DEC-LAB-015`. **Satu bagian dikecualikan, bukan satu slice:** gambar hasil Patologi Anatomi menunggu `DEC-LAB-016`. Lihat A3.16 |
| Scope yang dinilai | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11` (rev 2); `S13a`, `S13b`, `S14`, `S15` (rev 3); **`S4b` dan `S4c` (rev 5-6)**; **`S4` (rev 8)**; **`S4d-1` (rev 9)** |
| Kesiapan requirement | `PARTIALLY_READY` dari `LAB-RCG-001` revision 4; seluruh slice dikirim sebagai slice siap yang berdiri sendiri |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Rujukan bukti | `00-interview-decisions.md` rev 14; `01-existing-capability-map.md` rev 1; `02-requirement-completeness-assessment.md` rev 2 |
| Baseline rujukan | **Tidak dipakai.** `indonesia-hospital-domain-reference` tidak dipanggil pada sesi ini |
| Sifat | **Read-only** terhadap repository aplikasi |

### Decision ID yang mengikat scope ini

| Decision ID | Isi ringkas | Slice |
|---|---|---|
| `LAB-DEC-013` | Cito ditandai dokter pemesan, batas waktu per jenis pemeriksaan, ada daftar pantau keterlambatan | `S1a`, `S7` |
| `LAB-DEC-009` | Melayani Rawat Jalan, Rawat Inap, dan IGD sekaligus | `S1a` |
| `LAB-INH-002` | Alur sampel `Planned → Collected → Received → Accepted` beserta pengecualiannya | `S2` |
| `LAB-INH-005` | Satu pesanan boleh punya banyak sampel; ambil ulang membuat identitas baru dan menyimpan tautan sebabnya | `S2` |
| `LAB-INH-008` | Sampel sampai di lab tidak sama dengan sampel dinyatakan layak | `S2` |
| `LAB-INH-009` sampai `LAB-INH-012` | Titik kelayakan tagih, Laboratorium hanya mengirim fakta, tanpa wewenang finansial | `S10` |
| `LAB-DEC-006`, `LAB-DEC-018` | Tabel batas nilai milik Laboratorium, menunjuk ke jenis pemeriksaan, banyak baris per pemeriksaan | `S3` |
| `LAB-DEC-021` | Hasil punya dua bentuk: angka dan pilihan terbatas | `S3` |
| `LAB-DEC-023` | Batas normal bebas; batas kritis memerlukan persetujuan klinis; semua perubahan berriwayat | `S3` |
| `LAB-DEC-019` | Alasan penolakan dikelola kepala instalasi, kecuali dua kolom yang terkunci | `S11` |

### Decision ID yang belum selesai dan tidak menyentuh scope ini

`LAB-SIGN-001`, `LAB-AMD-001`, `LAB-COORD-001`, `LAB-COORD-002`. Keempatnya mengikat slice
`S1b`, `S4`, `S5`, `S6`, `S8`, dan `S9` yang **tidak** dirancang di dokumen ini.

> **Cara membaca dokumen ini.**
> Dokumen ini menetapkan **makna bisnis** — konsep apa yang ada di laboratorium, siapa
> pemiliknya, bagaimana perjalanannya, dan aturan apa yang tidak boleh dilanggar. Ia **bukan**
> rancangan tabel, bukan rancangan endpoint, dan bukan rancangan layar. Ketiganya baru
> ditentukan pada tahap berikutnya.
>
> Sebuah konsep di sini tidak otomatis menjadi satu tabel database.

---

## A2. Perluasan Revision 3 — Slice `S13`, `S14`, `S15`

| Field | Value |
|---|---|
| Revision | `4` |
| Scope tambahan | `S13a`, `S13b`, `S14`, `S15` |
| Kesiapan requirement | `LAB-RCG-001` rev 4 — ketiganya `READY_FOR_DOMAIN_DESIGN`, `S14` hanya bagian penyajian |
| Decision ID yang mengikat | `LAB-DEC-025`, `LAB-DEC-028`, `LAB-DEC-029`, `LAB-DEC-032`, `LAB-DEC-033` |
| Kesiapan arsitektur bagian ini | **`DOMAIN_ARCHITECTURE_PARTIAL`** — `S13a`, `S14`, `S15` siap; `S13b` terblokir |

`S13` dipecah menjadi dua karena keduanya punya kebutuhan data yang berbeda:

| Slice | Isi |
|---|---|
| `S13a` | Pendaftaran **pasien datang langsung** yang tidak membawa rujukan |
| `S13b` | Pendaftaran **pasien rujukan luar** yang membawa surat rujukan dari dokter atau institusi lain |

---

### A2.1 Bahasa yang ditambahkan

| Istilah | Makna bisnis tunggal |
|---|---|
| **Pasien Datang Langsung** | Pasien yang datang sendiri ke laboratorium tanpa melewati loket pendaftaran dan tanpa membawa rujukan |
| **Pasien Rujukan Luar** | Pasien yang dikirim dokter atau institusi di luar rumah sakit ini, membawa surat rujukan |
| **Sumber Rujukan** | Dokter atau institusi yang mengirim pasien, beserta kontaknya |
| **Katalog Pemeriksaan Laboratorium** | Daftar jenis pemeriksaan yang dapat dipesan, disaring dari katalog tindakan rumah sakit |
| **Harga Berlaku** | Harga sebuah pemeriksaan pada tanggal kejadian, menurut unit layanan dan kelas pasien |
| **Cakupan Penjamin** | Keterangan apakah sebuah pemeriksaan ditanggung penjamin pasien, dan dengan harga kontrak berapa |
| **Daftar Pantau per Disiplin** | Susunan pesanan yang disaring menurut disiplin: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi |

**Perbedaan makna yang wajib dipertahankan:**

| Pasangan | Kenapa tidak boleh disatukan |
|---|---|
| **Harga Berlaku** vs **Cakupan Penjamin** | Yang pertama harga rumah sakit, yang kedua harga kontrak dengan penjamin. Keduanya dapat berbeda, dan yang menentukan tagihan tetap Billing |
| **Pasien Datang Langsung** vs **Pasien Rujukan Luar** | Yang pertama tidak membawa dokumen apa pun; yang kedua membawa surat rujukan yang perlu disimpan dan ditelusuri |

---

### A2.2 Hubungan antarcontext yang berubah

| Dari | Ke | Sebelum | Sesudah |
|---|---|---|---|
| `BC-LAB` | `BC-REG` | Hanya membaca kunjungan | **Membaca dan meminta pembuatan** kunjungan (`LAB-DEC-032`) |
| `BC-LAB` | `BC-MD` | Membaca jenis pemeriksaan dan tarif | Ditambah membaca **cakupan penjamin** |

**Yang tidak berubah.** `BC-REG` tetap **pemilik** kunjungan, dan `BC-MD` tetap **pemilik**
katalog serta tarif. `BC-LAB` tidak menulis satu baris pun ke tabel milik keduanya.

---

### A2.3 Konsep domain yang ditambahkan

| ID | Nama bisnis | Klasifikasi | Pemilik | Ownership | Bukti |
|---|---|---|---|---|---|
| `LAB-DC-030` | Permintaan Pembuatan Kunjungan | `EXTERNAL_CONTRACT` | `BC-REG` melaksanakan, `BC-LAB` meminta | `Existing` — memakai kemampuan yang sudah ada | `EncounterRegistrationSource.WalkIn`, `TrxPatientEncounter.IsWalkIn`, `PatientEncounterController@c87d9c0` |
| `LAB-DC-031` | Sumber Rujukan — instansi dan dokter perujuk | `REFERENCE_DATA` | `BC-MD` Data Induk | `New` di `BC-MD`, dirujuk `BC-LAB` (`LAB-DEC-035`) | Belum ada di `c87d9c0`; kunjungan hanya punya `IsReferral` dan `ReferralNumber` |
| `LAB-DC-032` | Katalog Pemeriksaan Laboratorium | `ADAPTER/VIEW` | `BC-MD` pemilik, `BC-LAB` menyajikan | `Adapter/View` | `MstProcedure.IsLaboratory@c87d9c0` |
| `LAB-DC-033` | Harga Berlaku | `ADAPTER/VIEW` | `BC-MD` pemilik, `BC-LAB` menyajikan | `Adapter/View` | `MstTariff@c87d9c0` beserta `EffectiveStartDate` dan `EffectiveEndDate` |
| `LAB-DC-034` | Cakupan Penjamin | `ADAPTER/VIEW` | `BC-MD` pemilik, `BC-LAB` menyajikan | `Adapter/View` | `MstInsuranceTariff@c87d9c0` |
| `LAB-DC-035` | Daftar Pantau per Disiplin | `ADAPTER/VIEW` | `BC-LAB` | `Adapter/View` | Diturunkan dari `LabOrder.Discipline` |

**Temuan pokok bagian ini.** Dari enam konsep yang ditambahkan, **lima tidak memerlukan satu
pun tabel baru**. Empat berupa penyajian data milik modul lain, satu berupa kontrak pemanggilan.
Hanya `LAB-DC-031` yang berpotensi memerlukan tempat penyimpanan baru, dan justru itulah yang
belum diputuskan.

#### Kenapa katalog, harga, dan cakupan tidak menjadi entity baru

| Yang dibutuhkan layar pemesanan | Sudah tersedia di | Perlu tabel baru? |
|---|---|---|
| Daftar pemeriksaan yang dapat dipesan | `MstProcedure` dengan penanda `IsLaboratory` | **Tidak** |
| Harga satuan pada tanggal kejadian | `MstTariff` dengan `ProcedureId`, `ServiceUnitId`, `PatientClassId`, dan masa berlaku | **Tidak** |
| Apakah ditanggung penjamin | `MstInsuranceTariff` dengan `InsuranceProviderId`, `TariffId`, `ContractPrice`, `IsUsingContractPrice`, `BenefitPlanCode` | **Tidak** |
| Penanda bawaan tercakup atau tidak | `MstProcedure.IsCoveredByInsuranceDefault` | **Tidak** |

Menurut `LAB-DEC-033`, seluruhnya disajikan **baca saja** oleh Laboratorium.

---

### A2.4 Model aggregate

Bagian ini **tidak menambah aggregate baru**. Alasannya jelas: tidak ada invariant baru yang
perlu dilindungi batas konsistensi milik Laboratorium.

| Kemampuan | Aggregate yang melindunginya |
|---|---|
| Pendaftaran pasien datang langsung | `AGG-REG-*` milik Registrasi. Laboratorium hanya meminta |
| Katalog, harga, cakupan | Aggregate data induk milik `BC-MD`. Laboratorium hanya membaca |
| Daftar pantau per disiplin | Tidak ada — seluruhnya diturunkan dari `AGG-LAB-01` |

**Invariant yang ditambahkan pada `AGG-LAB-01`:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-21` | Sebuah pesanan wajib memiliki tepat satu disiplin, dan disiplin itu tidak berubah setelah pesanan dibuat | `LAB-DEC-025` |
| `INV-22` | Jenis pemeriksaan pada sebuah pesanan wajib sesuai disiplin pesanan itu | `LAB-DEC-025` |
| `INV-23` | Laboratorium tidak boleh menulis ke tabel kunjungan, pasien, katalog, tarif, maupun cakupan penjamin | `LAB-DEC-032`, `LAB-DEC-033` |

**Contoh `INV-22`:** pesanan berdisiplin Mikrobiologi tidak boleh memuat pemeriksaan
Hemoglobin, karena Hemoglobin adalah pemeriksaan Patologi Klinik. Bagaimana penanda disiplin
melekat pada jenis pemeriksaan **belum diputuskan** — `MstProcedure` hanya punya penanda
`IsLaboratory` tanpa pembeda disiplin. Dicatat sebagai `DEC-LAB-010`.

---

### A2.5 Lifecycle yang ditambahkan

Bagian ini **tidak menambah lifecycle baru** bagi Laboratorium.

| Kemampuan | Lifecycle-nya milik siapa |
|---|---|
| Pendaftaran pasien datang langsung | Kunjungan mengikuti lifecycle milik Registrasi |
| Katalog, harga, cakupan | Data induk; lifecycle-nya aktif atau nonaktif, milik `BC-MD` |
| Daftar pantau per disiplin | Tidak punya lifecycle; ia adalah tampilan atas keadaan terkini |

---

### A2.6 Tanggung jawab authorization yang ditambahkan

| Kemampuan | Boleh melakukan | Tidak boleh |
|---|---|---|
| Mendaftarkan pasien datang langsung dari layar Laboratorium | Mengisi identitas dan menyimpan, yang memicu permintaan ke Registrasi | Mengubah kunjungan yang sudah ada, atau menutupnya |
| Melihat katalog, harga, dan cakupan | Membaca dan menyaring | Mengubah apa pun |
| Melihat daftar pantau per disiplin | Membaca dan menyaring | — |

**Yang tetap milik Registrasi.** Apakah petugas laboratorium berhak membuat kunjungan adalah
kebijakan milik Registrasi, bukan Laboratorium. Bila Registrasi menolak permintaan karena
kewenangan, Laboratorium **menampilkan penolakan itu apa adanya** dan tidak mencari jalan lain.

---

### A2.7 Audit yang ditambahkan

| Kejadian | Dicatat di mana |
|---|---|
| Permintaan pembuatan kunjungan dari Laboratorium | Jejak audit **milik Registrasi**, dengan sumber pendaftaran bernilai datang langsung |
| Pesanan yang lahir dari kunjungan itu | `TrxLabTransitionHistory` seperti biasa |
| Pembacaan katalog, harga, dan cakupan | **Tidak dicatat.** Sesuai konvensi, pembacaan tidak masuk logger |

---

### A2.8 Batas integrasi yang ditambahkan

#### `INT-05` — Laboratorium meminta Registrasi membuat kunjungan

| Aspek | Isi |
|---|---|
| Peminta | `BC-LAB` |
| Pelaksana | `BC-REG` |
| Tujuan bisnis | Memberi kunjungan kepada pasien yang datang langsung ke laboratorium |
| Sumber kebenaran | `BC-REG` — Laboratorium hanya menyimpan penunjuk hasilnya |
| Arah | Permintaan dan jawaban, satu kali jalan |
| Sinkron atau asinkron | **Sinkron.** Pesanan tidak dapat dibuat sebelum kunjungan ada |
| Idempotensi | **Wajib.** Petugas yang menekan simpan dua kali tidak boleh menghasilkan dua kunjungan untuk pasien yang sama |
| Perilaku saat gagal | Pendaftaran gagal seluruhnya. Tidak ada pesanan yang terbentuk, dan tidak ada data setengah jadi yang disimpan Laboratorium |
| Rekonsiliasi | Tidak diperlukan, karena sifatnya sinkron dan gagal berarti batal |

**Yang belum disepakati:** bentuk permintaan, bentuk jawaban, dan perilaku saat Registrasi
menolak. Dicatat sebagai `LAB-COORD-003`.

#### `INT-06` — Laboratorium membaca cakupan penjamin

| Aspek | Isi |
|---|---|
| Pembaca | `BC-LAB` |
| Pemilik | `BC-MD` |
| Yang dibaca | `MstInsuranceTariff` menurut penjamin pasien dan tarif pemeriksaan |
| Sifat | Baca saja, tanpa penyalinan |
| Bila tidak ditemukan | Pemeriksaan ditampilkan sebagai **tidak tercakup**, bukan sebagai kesalahan |

**Batas yang tegas.** Laboratorium **menampilkan** cakupan; ia tidak menghitung selisih, tidak
menentukan siapa membayar, dan tidak menyimpan keputusan cakupan. Aturan cakupan yang lebih
rinci tertahan `LAB-P0-007`.

---

### A2.9 Dampak billing

**Klasifikasi: tidak menambah dampak charge baru.**

Katalog, harga, dan cakupan hanya **ditampilkan**. Tidak ada fakta baru yang diterbitkan ke
Billing, dan titik kelayakan tagih tetap satu-satunya: wadah dinyatakan layak.

**Satu hal yang perlu diwaspadai.** Menampilkan total harga di layar pemesanan mudah disalahpahami
sebagai tagihan. Tampilan itu **wajib** diberi keterangan bahwa angkanya adalah perkiraan
biaya, bukan tagihan resmi.

---

### A2.10 Dampak keselamatan klinis

**Klasifikasi: klinis, tetapi tidak ada perpindahan yang kritis bagi keselamatan.**

| Aspek | Penilaian |
|---|---|
| Pendaftaran pasien datang langsung | Risiko utamanya adalah **identitas pasien ganda** — pasien yang sebenarnya sudah terdaftar didaftarkan lagi sebagai pasien baru. Pencegahannya berada di Registrasi, bukan Laboratorium |
| Katalog dan harga | Tidak ada dampak keselamatan |
| Cakupan penjamin | Tidak ada dampak keselamatan langsung. Dampak tidak langsungnya adalah pasien menolak pemeriksaan karena biaya |
| Daftar pantau per disiplin | Tidak ada dampak keselamatan |

---

### A2.11 Gap arsitektur bagian ini

#### `DEC-LAB-009` — Di mana identitas dokter dan instansi perujuk disimpan?

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat ditemukan, kini **`CONFIRMED`** |
| Dampak | `BLOCKING` saat ditemukan, kini **tertutup** `LAB-DEC-035` |
| Pemilik keputusan | Yoga Aji Pratama + pemilik `registration-management` |
| Diarahkan ke | `grill-me` |

**Bukti keadaan saat ini.** `TrxPatientEncounter@c87d9c0` hanya menyimpan **penanda dan nomor**
rujukan:

| Kolom yang ada | Kolom yang **tidak** ada |
|---|---|
| `IsReferral` | Nama dokter perujuk |
| `ReferralNumber` | Nama instansi perujuk |
| `IsReferralRequired` | Alamat dan telepon instansi perujuk |
| `IsReferralVerified` | — |

Tidak ditemukan pula data induk untuk instansi perujuk. `MstHospitalSite` adalah lokasi milik
rumah sakit ini sendiri, bukan institusi luar.

**Kenapa ini memblokir.** Bukti lapangan menunjukkan laboratorium mencatat "dokter perujuk,
instansi perujuk, telepon dan alamat instansi, surat rujukan". Tanpa tempat penyimpanan, data
itu akan hilang atau dipaksakan masuk kolom keterangan bebas — sehingga tidak dapat dicari,
tidak dapat dilaporkan, dan laporan "dokter pengirim" yang terlihat pada bukti tidak akan
pernah bisa dibuat.

**Contoh yang harus bisa dijawab.**

> Klinik Sehat Sentosa mengirim rata-rata 40 pasien per bulan ke laboratorium. Manajemen ingin
> tahu klinik mana saja yang paling banyak merujuk, untuk keperluan kerja sama.
>
> Bila nama klinik hanya diketik bebas, "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan
> "sehat sentosa" akan terhitung sebagai tiga institusi berbeda.

**Tiga arah yang mungkin, seluruhnya `PROPOSED` dan bukan keputusan:**

| Arah | Konsekuensi |
|---|---|
| Data induk instansi perujuk milik Master Data, dirujuk Laboratorium | Paling rapi dan dapat dilaporkan; perlu kesepakatan lintas modul dan pengisian data awal |
| Kolom tambahan pada kunjungan milik Registrasi | Terpusat pada kunjungan; mengubah tabel milik modul lain |
| Laboratorium menyimpan sumber rujukan sendiri | Paling cepat; berisiko menjadi data induk tandingan bila modul lain kelak menerima rujukan juga |

#### `DEC-LAB-010` — Bagaimana disiplin melekat pada jenis pemeriksaan?

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat ditemukan, kini **`CONFIRMED`** |
| Dampak | Kini **tertutup** `LAB-DEC-036`; `INV-22` dapat ditegakkan |
| Pemilik keputusan | Yoga Aji Pratama + pemilik `master-data` |

**Bukti keadaan saat ini.** `MstProcedure@c87d9c0` punya penanda `IsLaboratory`, `IsRadiology`,
`IsSurgery`, `IsTherapy` — tetapi **tidak ada pembeda antara Patologi Klinik, Patologi Anatomi,
dan Mikrobiologi**. Ada `ProcedureGroupName` dan `ProcedureCategoryName` berupa teks bebas,
yang tidak dapat diandalkan sebagai penanda.

**Akibatnya.** `LabOrder.Discipline` dapat diisi petugas, sehingga daftar pantau per disiplin
(`S15`) tetap berjalan. Tetapi sistem **tidak dapat memeriksa** apakah pemeriksaan yang dipilih
memang sesuai disiplin pesanannya. `INV-22` menjadi aturan tertulis tanpa penegak.

---

### A2.12 Kesiapan arsitektur bagian ini

**`DOMAIN_ARCHITECTURE_PARTIAL`**

| Slice | Kesiapan | Keterangan |
|---|---|---|
| `S13a` pendaftaran pasien datang langsung | **`DOMAIN_ARCHITECTURE_READY`** | Seluruh kemampuannya sudah ada di Registrasi; Laboratorium hanya memanggil |
| `S13b` pendaftaran pasien rujukan luar | **`DOMAIN_ARCHITECTURE_READY`** | Dibuka `LAB-DEC-035` — instansi dan dokter perujuk menjadi data induk global milik Master Data |
| `S14` katalog, harga, cakupan — bagian penyajian | **`DOMAIN_ARCHITECTURE_READY`** | Nol entity baru; seluruhnya penyajian data milik `BC-MD` |
| `S15` monitoring per disiplin | **`DOMAIN_ARCHITECTURE_READY`** | Diturunkan dari `LabOrder.Discipline`. `INV-22` belum dapat ditegakkan sampai `DEC-LAB-010` ditutup |

**Yang boleh diserahkan ke `design-business-module`:** `S13a`, `S13b`, `S14`, dan `S15`, dinyatakan
berdiri sendiri.

**Yang harus berhenti:** tidak ada pada scope ini.

---

## A3. Perluasan Revision 5 — Slice `S4b` dan `S4c`

### A3.1 Identitas dan gerbang masuk

| Field | Value |
|---|---|
| Revision arsitektur | `LAB-DA-001` revision `5` |
| Scope yang dinilai | `S4b` pengisian hasil Mikrobiologi; `S4c` pengisian hasil Patologi Anatomi |
| Kesiapan requirement | `PARTIALLY_READY` dari `LAB-RCG-001-r7`; **kedua slice dikirim sebagai slice siap yang berdiri sendiri** (bagian 0B.5) |
| Tanggal | 2026-09-18 |
| Backend SHA | `13665452` |
| Baseline rujukan | **Tidak dipakai.** `indonesia-hospital-domain-reference` tidak dipanggil |
| Sifat | **Read-only.** Nol migration, endpoint, repository, service, UI, atau task diturunkan di sini |

**Batas scope yang wajib dihormati, dan ia sempit:**

| Yang dirancang | Yang **TIDAK** dirancang |
|---|---|
| Pencatatan hasil Mikrobiologi dan Patologi Anatomi | **Validasi dan rilis** — itu `S4d` dan `S4e`, tertahan `DEC-LAB-011` dan `LAB-OPEN-034` |
| Bentuk dan makna datanya | **Status hasil dalam bentuk apa pun** — `LAB-DEC-080` menetapkan validasi/rilis dicatat sebagai fakta, nol status |
| Siapa mencatat, kapan dikerjakan | **Penanda `Definitif`** — `LAB-DEC-081` mengeluarkannya dari Rilis 1 |
| — | **Penilaian kritis otomatis** — BR-23 menyatakan kedua bentuk ini tidak dapat dinilai lewat perbandingan angka; percabangannya milik `S5` yang tertahan |

### A3.2 Decision ID yang mengikat scope ini

| Decision ID | Isi ringkas | Slice |
|---|---|---|
| `LAB-DEC-027` (BR-23) | Hasil punya **empat** bentuk; Mikrobiologi berstruktur dan narasi Patologi Anatomi adalah dua di antaranya | `S4b`, `S4c` |
| `LAB-DEC-005` | Hasil Rilis 1 **diketik manual analis**; struktur data Rilis 1 tidak menyediakan tempat bagi hasil dari alat | `S4b`, `S4c` |
| `LAB-DEC-079` | Aturan keselamatan ditandatangani **per disiplin** — `DR-LAB-002` untuk Mikrobiologi, `DR-LAB-003` untuk Patologi Anatomi | `S4b`, `S4c` |
| `LAB-DEC-080` | Validasi dan rilis dicatat sebagai **fakta**, bukan status | Mengikat sebagai **larangan** di sini |
| `LAB-DEC-081` | Penanda `Definitif` di luar Rilis 1 | `S4b` |
| `LAB-DEC-083` | Validasi/rilis kedua disiplin menjadi `S4d` dan `S4e` — **di luar scope ini** | `S4b`, `S4c` |

### A3.3 Ubiquitous language — istilah yang ditambahkan

| Istilah | Makna bisnis tunggal |
|---|---|
| **Isolat** | Satu organisme yang **ditemukan tumbuh** pada satu pemeriksaan mikrobiologi. Bukan "jenis kuman" secara umum — melainkan temuan pada pemeriksaan ini, pasien ini |
| **Kepekaan antibiotik** | Hasil pengujian satu antibiotik terhadap **satu isolat**: kadarnya, lebar zona hambat dalam milimeter, dan kesimpulan `R`/`I`/`S` |
| **`R` / `I` / `S`** | *Resistent* — kebal; *Intermediate* — di antara, perlu pertimbangan dosis; *Sensitive* — peka, antibiotik diperkirakan bekerja (BR-23) |
| **Makroskopik** | Uraian jaringan sebagaimana **terlihat mata telanjang** |
| **Mikroskopik** | Uraian jaringan sebagaimana **terlihat di bawah mikroskop** |
| **Kesimpulan** | Pernyataan diagnosis patolog atas kedua uraian di atas |
| **Status temuan** `Normal`/`Positif`/`Negatif` | **Nilai hasil**, bukan status lifecycle. Lihat peringatan A3.4 |

> ### A3.4 Satu kata yang paling mudah disalahartikan di seluruh dokumen ini
>
> BR-23 menyebut *"status `Normal`/`Positif`/`Negatif`"* pada bentuk Mikrobiologi berstruktur.
> **Itu bukan status lifecycle**, dan ia **tidak** bertentangan dengan `LAB-DEC-080`.
>
> | Yang dilarang `LAB-DEC-080` | Yang dimaksud BR-23 |
> |---|---|
> | Status **perjalanan** hasil: sudah divalidasi, sudah dirilis — janji tentang apa yang boleh terjadi berikutnya | **Isi** hasilnya: apa yang ditemukan pada biakan — fakta tentang pasien |
>
> Keduanya kebetulan memakai kata "status" dalam bahasa sehari-hari. Arsitektur ini memisahkan
> keduanya dengan menamai yang kedua **status temuan**, dan menempatkannya sebagai **nilai**,
> sederajat dengan `ResultNumeric` pada Patologi Klinik.

### A3.5 Peta bounded context — perubahan

**Nol bounded context baru.** Kedua slice sepenuhnya di dalam `BC-LAB`.

Dua data induk **baru**, dan `LAB-DEC-084` menempatkan keduanya **di dalam `BC-LAB`** — bukan
sebagai ketergantungan hilir ke `BC-MD`:

| Data induk | Untuk apa | Pemilik |
|---|---|---|
| **Organisme** | Nama kuman yang ditemukan | `BC-LAB`, berprefix `Lab` (`LAB-DEC-084`) |
| **Antibiotik** | Antibiotik yang diuji kepekaannya | `BC-LAB`, berprefix `Lab` (`LAB-DEC-084`) |

> **Ini bukan pelanggaran aturan "jangan menduplikasi data induk bersama", dan alasannya dua.**
> Pertama, **nol pemilik tandingan yang diduplikasi**: penelusuran menemukan sistem belum punya
> data induk organisme maupun antibiotik di mana pun, sehingga tidak ada sumber kebenaran yang
> disaingi. Kedua, keduanya memang **milik laboratorium secara domain** — panel antibiotik yang
> diuji kepekaannya adalah keputusan laboratorium tentang pekerjaannya sendiri, dan ia bukan
> formularium farmasi. Bila kelak farmasi mendirikan formularium, hubungannya adalah pemetaan
> antardua konsep yang berbeda, bukan penggabungan.

### A3.6 Katalog konsep domain — yang ditambahkan

| ID | Nama bisnis | Klasifikasi | Pemilik | Ownership | Identitas | Invariant penting | Bukti |
|---|---|---|---|---|---|---|---|
| `LAB-DC-036` | **Isolat Mikrobiologi** | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri di dalam satu pemeriksaan | Satu isolat milik tepat satu pemeriksaan; dapat ditambah dan dikurangi selama hasil belum dirilis | BR-23 aturan turunan butir 1 |
| `LAB-DC-037` | **Kepekaan Antibiotik** | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri di dalam satu isolat | Satu baris = satu antibiotik terhadap satu isolat; `R`/`I`/`S` wajib terisi | BR-23 |
| `LAB-DC-038` | **Laporan Patologi Anatomi** | `VALUE_OBJECT` | `BC-LAB` | `New` | **Tidak punya identitas sendiri** — melekat pada pemeriksaan | Makroskopik, mikroskopik, dan kesimpulan **ketiganya wajib**, dan berubah bersama-sama | BR-23 aturan turunan butir 3 |
| `LAB-DC-039` | **Gambar Hasil** | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri | Melekat pada tepat satu pemeriksaan; ukuran dibatasi | BR-23 aturan turunan butir 2 |
| `LAB-DC-040` | **Bentuk Hasil** | `REFERENCE_DATA` | `BC-LAB` | `Extend` | Nilai terbatas | Bertambah dua: *Mikrobiologi berstruktur* dan *Narasi Patologi Anatomi* | `LAB-DEC-027`; keadaan lama `LabResultForm@13665452` |
| `LAB-DC-041` | **Organisme** | `REFERENCE_DATA` | `BC-LAB` | `New` — berprefix `Lab` | Kode organisme yang unik | Kode unik; organisme nonaktif tidak dapat dipakai pada isolat baru | `LAB-DEC-084`; pola `LAB-OPEN-021` |
| `LAB-DC-042` | **Antibiotik** | `REFERENCE_DATA` | `BC-LAB` | `New` — berprefix `Lab` | Kode antibiotik yang unik | Kode unik; antibiotik nonaktif tidak dapat dipakai pada baris kepekaan baru | `LAB-DEC-084`; pola `LAB-OPEN-021` |

#### Kenapa `LAB-DC-038` sebuah `VALUE_OBJECT`, bukan `ENTITY`

Tiga alasan, dan ketiganya berasal dari BR-23 sendiri:

1. **Ketiga bagiannya wajib terisi.** Tidak ada keadaan sah di mana makroskopik ada tetapi
   kesimpulan tidak. Sesuatu yang selalu utuh tidak membutuhkan identitas untuk bagiannya.
2. **Tidak ada yang menunjuk kepadanya.** Nol konsep lain perlu berkata *"laporan PA nomor
   sekian"*; yang ditunjuk selalu **pemeriksaannya**.
3. **Ia berubah sebagai satu kesatuan.** Patolog menyunting laporannya, bukan menyunting
   mikroskopik secara terpisah dari kesimpulan.

Sebaliknya `LAB-DC-036` dan `LAB-DC-039` **memang** entity: barisnya ditambah dan dikurangi satu
per satu, masing-masing berdiri sendiri, dan hilangnya satu baris bukan berarti laporan berubah
bentuk.

#### Yang sengaja **tidak** dijadikan konsep

| Yang ditolak | Alasan |
|---|---|
| Konsep "Hasil Mikrobiologi" dan "Hasil Patologi Anatomi" sebagai dua aggregate root sendiri | Hasil adalah **bagian dari perjalanan sebuah pemeriksaan**, bukan benda yang berdiri sendiri. Memisahkannya menciptakan dua sumber kebenaran atas satu kejadian yang sama, persis yang sudah dihindari `S4a` |
| Konsep "Antibiogram" sebagai entity | Antibiogram adalah **cara menyajikan** kumpulan kepekaan antibiotik, bukan benda tersimpan. Sama seperti Daftar Kerja pada revision 1 |
| Satu konsep per bagian narasi — "Makroskopik", "Mikroskopik", "Kesimpulan" | Tiga form pada satu layar bukan tiga konsep. Melanggar aturan arsitektur secara langsung |
| Konsep "Status Temuan" sebagai entity | Ia sebuah **nilai**, sederajat dengan angka hasil. Lihat A3.4 |

### A3.7 Model aggregate

**Nol aggregate baru.** Keempat konsep baru masuk ke dalam `AGG-LAB-01` Pesanan Laboratorium,
melalui `LAB-DC-002` Pemeriksaan Terpesan.

| Pertanyaan | Jawaban |
|---|---|
| Kenapa tidak menjadi aggregate sendiri? | Batas konsistensi yang dilindungi tetap **pesanan**: membatalkan pesanan wajib membatalkan seluruh isinya serentak (`INV-03`), dan hasil termasuk isinya |
| Apakah batasnya tidak menjadi terlalu besar? | Ia memang membesar, dan itu dicatat sebagai gap `ARCH-GAP-LAB-05` — bukan disembunyikan. Selama validasi dan rilis di luar scope, pertumbuhannya belum menimbulkan pertentangan invariant |

**Invariant yang ditambahkan:**

| ID | Invariant | Slice | Bukti |
|---|---|---|---|
| `INV-24` | Sebuah pemeriksaan memiliki **tepat satu** bentuk hasil, dan bentuk itu ditentukan katalog batas nilainya — bukan dipilih pengisi | `S4b`, `S4c` | `LAB-DEC-027`; pola berjalan `LabValueBound.ResultForm@13665452` |
| `INV-25` | Pemeriksaan berbentuk **narasi Patologi Anatomi** tidak sah bila salah satu dari makroskopik, mikroskopik, atau kesimpulan kosong | `S4c` | BR-23 aturan turunan butir 3 |
| `INV-26` | Pemeriksaan berbentuk **Mikrobiologi berstruktur** menyimpan status temuan; isolat hanya boleh ada bila status temuannya memungkinkan pertumbuhan | `S4b` | BR-23 — **usulan arsitektur**; bentuk penegakannya menyusul saat blueprint disusun |
| `INV-30` | Isolat wajib menunjuk organisme yang dikenal, dan baris kepekaan wajib menunjuk antibiotik yang dikenal. **Pengetikan bebas ditolak pada kedua ruas itu** | `S4b` | `LAB-DEC-084` |
| `INV-31` | Organisme atau antibiotik yang dinonaktifkan **tidak dapat dipakai pada baris baru**, tetapi **tidak menghapus maupun mengubah** baris lama yang sudah menunjuknya | `S4b` | `LAB-DEC-084`; pola yang sama dengan penunjuk batas nilai pada `S4a` — hasil yang sudah terjadi tidak boleh berubah karena data induknya diperbarui |
| `INV-27` | Satu kepekaan antibiotik melekat pada **tepat satu** isolat, dan tidak dapat berpindah isolat | `S4b` | BR-23 |
| `INV-28` | Nol penilaian kritis otomatis dijalankan atas hasil Mikrobiologi maupun narasi Patologi Anatomi | `S4b`, `S4c` | BR-23 catatan penilaian kritis |
| `INV-29` | Nol status hasil disimpan oleh kedua bentuk ini | `S4b`, `S4c` | `LAB-DEC-080` |

**Tindakan bisnis yang ditambahkan:**

| Tindakan | Wewenang | Menerbitkan fakta? |
|---|---|---|
| Mencatat status temuan mikrobiologi | Petugas berwenang mengisi hasil | Tidak |
| Menambah, menyunting, dan menghapus isolat | Petugas berwenang mengisi hasil | Tidak |
| Menambah, menyunting, dan menghapus baris kepekaan antibiotik | Petugas berwenang mengisi hasil | Tidak |
| Mencatat dan menyunting laporan Patologi Anatomi | Petugas berwenang mengisi hasil | Tidak |
| Melampirkan dan melepas gambar hasil | Petugas berwenang mengisi hasil | Tidak |

> **Nol di antaranya menerbitkan fakta ke Billing, dan itu bukan kebetulan.** Titik kelayakan
> tagih sudah ditetapkan pada **penerimaan sampel** (`LAB-INH-009` sampai `LAB-INH-012`, slice
> `S10`), bukan pada hasil. Pengisian hasil karena itu **nol dampak charge**.

### A3.8 Model relasi

| Sumber | Tujuan | Makna | Kardinalitas | Wajib? | Lifecycle |
|---|---|---|---|---|---|
| `LAB-DC-002` Pemeriksaan | `LAB-DC-036` Isolat | Pemeriksaan mikrobiologi menemukan sejumlah organisme | 1 : 0..n | Opsional — nol isolat adalah hasil yang sah (tidak ada pertumbuhan) | Isolat mati bersama pemeriksaannya |
| `LAB-DC-036` Isolat | `LAB-DC-037` Kepekaan | Satu organisme diuji terhadap sejumlah antibiotik | 1 : 0..n | Opsional | Kepekaan mati bersama isolatnya |
| `LAB-DC-002` Pemeriksaan | `LAB-DC-038` Laporan PA | Pemeriksaan patologi anatomi menghasilkan satu laporan | 1 : 0..1 | Wajib begitu hasil diisi | Melekat; tidak punya umur sendiri |
| `LAB-DC-002` Pemeriksaan | `LAB-DC-039` Gambar | Laporan disertai gambar contoh | 1 : 0..n | Opsional | Gambar dapat dilepas tanpa mengubah laporan |
| `LAB-DC-036` Isolat | `LAB-DC-041` Organisme | Isolat **menunjuk** organisme yang dikenal | n : 1 | **Wajib** — pengetikan bebas tidak diterima | Data induk hidup lebih lama daripada isolat; nonaktif tidak menghapus isolat lama |
| `LAB-DC-037` Kepekaan | `LAB-DC-042` Antibiotik | Baris kepekaan **menunjuk** antibiotik yang dikenal | n : 1 | **Wajib** — pengetikan bebas tidak diterima | Sama seperti di atas |

**Dua pola `S4a` yang wajib diwarisi kedua slice ini**, dan alasannya sudah terbukti:

| Pola | Kenapa tetap berlaku |
|---|---|
| Menyimpan **penunjuk batas nilai yang berlaku saat hasil diisi** | Tanpa itu, hasil lama berubah artinya ketika data induknya diperbarui. Berlaku sama bagi organisme dan antibiotik: nama yang diperbarui tidak boleh berlaku surut pada hasil yang sudah tercetak |
| Memisahkan **kapan dikerjakan** dari **kapan diketik** | `ExaminedAt` dan `ResultEnteredAt` sudah ada pada `LabExamination@13665452` dan berlaku apa adanya untuk kedua bentuk baru. Nol ruas waktu baru dibutuhkan |

### A3.9 Model lifecycle dan status

**Nol status baru.** Ini konsekuensi langsung `LAB-DEC-080` dan `INV-29`.

| Pertanyaan bisnis | Bagaimana dijawab **tanpa** status |
|---|---|
| Apakah hasilnya sudah diisi? | `ResultEnteredAt != null` — pola yang sudah berjalan sejak `S4a` |
| Kapan pemeriksaannya dikerjakan? | `ExaminedAt` |
| Siapa yang mengetiknya? | Pelaku pengisian, sebagaimana `S4a` |
| Apakah hasilnya sudah **lengkap**? | **Tidak terjawab.** Lihat `ARCH-GAP-LAB-04` — dan ini gap yang nyata |

### A3.10 Tanggung jawab authorization

| Tanggung jawab | Keadaan |
|---|---|
| Siapa boleh **mengisi** hasil | Mengikuti model `resource : action` yang sudah berjalan, sebagaimana `S4a`. **Nol peran baru dikarang** |
| Siapa boleh **memvalidasi dan merilis** | **Di luar scope.** `S4d`/`S4e`, tertahan `DEC-LAB-011` |
| Apakah kewenangan mengisi berbeda antardisiplin | **Belum dinyatakan.** Bertaut `LAB-OPEN-034`, yang menanyakan hal serupa untuk validasi. **Tidak diputuskan di sini** |

### A3.11 Model audit dan histori

| Yang wajib terlacak | Alasan |
|---|---|
| Siapa mengisi, kapan diketik, kapan dikerjakan | Sama seperti `S4a` |
| Penambahan dan **penghapusan** baris isolat maupun kepekaan | **Inilah yang berbeda dari `S4a`.** Hasil Patologi Klinik berubah dengan ditimpa; hasil mikrobiologi berubah dengan **baris hilang** — dan baris yang hilang tanpa jejak adalah temuan yang lenyap tanpa ada yang tahu |
| Pelepasan gambar | Alasan yang sama |

> **Catatan jujur:** bentuk jejaknya — riwayat per baris, atau penandaan *soft delete* — **tidak
> ditetapkan di sini**; ia keputusan perancangan blueprint, bukan keputusan domain. Yang sudah
> pasti sejak `LAB-DEC-084`: baris yang hilang **menunjuk data induk**, bukan memuat teks bebas,
> sehingga jejaknya dapat menyimpan penunjuk dan tetap terbaca bertahun kemudian.

### A3.12 Model integrasi

| Batas | Keadaan |
|---|---|
| Alat laboratorium | **Nol.** `LAB-DEC-005` menetapkan hasil Rilis 1 diketik manual, dan struktur datanya sengaja tidak menyediakan tempat bagi hasil dari alat |
| Penyimpanan berkas gambar | **Ada polanya, belum ada pemiliknya.** Penelusuran 2026-09-18 menemukan unggahan berkas sudah berjalan di beberapa area — `WfpDocumentController`, `ProfileController`, `LeaveRequestController` — tetapi masing-masing memakai `SaveFileAsync` privatnya sendiri di atas `IWebHostEnvironment`. **Nol layanan penyimpanan bersama.** Lihat `DEC-LAB-016` |
| Billing | **Nol.** Pengisian hasil bukan titik kelayakan tagih |
| Rekam medis | **Di luar scope.** Pendaftaran hasil ke rekam medis adalah `S9` |

### A3.13 Dampak billing

**Tidak ada dampak charge yang diketahui.** Kelayakan tagih ditetapkan pada penerimaan sampel
(`S10`), bukan pada hasil. `INV-09` tetap berlaku penuh: nol kolom maupun tindakan finansial di
dalam aggregate ini.

### A3.14 Dampak keselamatan klinis

**Klinis, relevan terhadap keselamatan — tetapi perpindahan kritisnya berada di luar scope ini.**

| Aspek | Keadaan |
|---|---|
| Aturan keselamatannya | ✅ Sudah **ditandatangani** pihak klinis per disiplin (`LAB-DEC-079`) |
| Perpindahan yang kritis bagi keselamatan | Validasi, rilis, nilai kritis, koreksi — **seluruhnya di luar scope ini** |
| Penilaian kritis otomatis | **Sengaja nol** (`INV-28`). BR-23 menyatakan bakteri resisten dan kesimpulan patologi yang mengkhawatirkan adalah **penilaian klinis**, bukan perbandingan angka |
| Akibat yang perlu disadari | Hasil kedua disiplin ini akan **tersimpan tanpa dapat dirilis**, sama seperti Patologi Klinik sejak 2026-09-17. Tumpukannya bertambah sampai `DEC-LAB-011` dijawab |

### A3.15 Gap arsitektur

| ID | Gap | Dampak | Pemilik |
|---|---|---|---|
| ~~`DEC-LAB-015`~~ | ✅ **Ditutup 2026-09-18** oleh `LAB-DEC-084` — keduanya menjadi data induk terkendali **milik `BC-LAB`**, berprefix `Lab`. `S4b` naik `DOMAIN_ARCHITECTURE_READY`. **Dua pekerjaan lahir dari penutupan ini dan perlu dibawa ke blueprint:** dua data induk baru beserta cara mengisinya — dan "cara mengisinya" ditulis tegas justru karena `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan data induk tanpa endpoint tulis adalah kegagalan yang berulang. Isi aslinya: **Apakah organisme dan antibiotik merupakan data induk terkendali, dan siapa pemiliknya?** Bila teks bebas, satu kuman yang sama akan tertulis sebagai `E. coli`, `E.coli`, `Escherichia coli`, dan `eschericia coli` — empat tulisan, satu kuman — dan **antibiogram rumah sakit menjadi mustahil disusun**. Ini alasan yang **sama persis** dengan yang dipakai pemilik modul menutup `LAB-DEC-082` pada alasan koreksi. Pemiliknya juga belum jelas: panel antibiotik uji kepekaan **bukan** formularium farmasi | **`BLOCKING` bagi `S4b`** — ia menentukan identitas, invariant, bentuk jejak audit, dan mungkin-tidaknya pelaporan | Yoga Aji Pratama + pemilik `master-data`; panel antibiotiknya kemungkinan `DR-LAB-002` |
| `DEC-LAB-016` | **Di mana gambar hasil Patologi Anatomi disimpan, dan bolehkah ia terlayani web?** Polanya sudah ada di platform, tetapi berupa `SaveFileAsync` privat per area di atas `IWebHostEnvironment` — nol layanan bersama. Gambar patologi adalah **data klinis yang dapat mengidentifikasi pasien**; menyimpannya di bawah direktori yang dilayani web adalah keputusan privasi, bukan keputusan teknis. Sekelas dengan penandaan `LAB-DEC-030` atas pengiriman hasil ke kanal pihak ketiga | **`BLOCKING` bagi bagian gambar `S4c`**; **tidak** memblokir laporan narasinya | Pemilik platform + Yoga Aji Pratama |
| `ARCH-GAP-LAB-04` | **Hasil mikrobiologi tidak punya cara menyatakan dirinya lengkap.** Pada Patologi Klinik, `ResultEnteredAt != null` cukup — satu angka, sekali isi. Pada mikrobiologi, biakan dibaca **bertahap selama berhari-hari**: satu isolat tercatat hari ini tidak berarti pekerjaannya selesai. **Inilah biaya `LAB-DEC-081` yang baru terlihat pada meja arsitektur** — penanda `Definitif` justru yang menjawab pertanyaan ini | **Tidak memblokir `S4b`** selama rilis di luar scope, sebab nol pihak hilir bergantung pada "lengkap". **Menjadi memblokir ketika `S4d` dibuka** | Yoga Aji Pratama, lewat `LAB-OPEN-017` |
| `ARCH-GAP-LAB-05` | **`AGG-LAB-01` membesar.** Satu pesanan kini dapat memuat pemeriksaan, wadah, isolat, kepekaan antibiotik, dan gambar sekaligus. Dicatat agar tidak ditemukan belakangan | Belum menimbulkan pertentangan invariant selama validasi/rilis di luar scope. **Perlu ditinjau ulang saat `S4d`/`S4e` dirancang** | Tim pembangun sistem |

### A3.16 Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY` untuk kedua slice — diperbarui revision 6.**

| Slice | Kesiapan | Alasan |
|---|---|---|
| `S4c` pengisian hasil Patologi Anatomi | ✅ **`DOMAIN_ARCHITECTURE_READY`**, dinyatakan **berdiri sendiri** | Bentuk hasilnya terkunci penuh BR-23, invariantnya dapat dinyatakan, ownership-nya jelas, nol data induk baru dibutuhkan. **Bagian gambarnya dikecualikan** sampai `DEC-LAB-016` dijawab — dan itu sah dikecualikan sebab BR-23 mewajibkan **tiga** bagian narasinya, **tidak** mewajibkan gambarnya |
| `S4b` pengisian hasil Mikrobiologi | ✅ **`DOMAIN_ARCHITECTURE_READY`** — **naik revision 6** | `DEC-LAB-015` ditutup `LAB-DEC-084`: organisme dan antibiotik menjadi data induk terkendali milik `BC-LAB`. Identitas konsep intinya kini ditetapkan, bukan ditebak |

> ### Riwayat penilaian slice ini, dipertahankan karena urutannya bermakna
>
> Revision 5 menyatakan `S4b` **`DOMAIN_ARCHITECTURE_BLOCKED`**, dan itu adalah **koreksi ketiga
> atas gerbang `LAB-RCG-001-r7`**: gerbang menyatakannya siap dengan nol penahan, sedangkan BR-23
> menyebut *"organisme per bakteri, antibiotik"* tanpa pernah menyatakan **apakah keduanya data
> induk**. Gerbang membacanya sebagai bentuk hasil yang sudah lengkap; arsitektur menemukan dua
> konsep intinya belum punya pemilik.
>
> Penahan itu **ditutup pada hari yang sama** lewat satu pertanyaan kepada pemilik modul. Yang
> perlu disimpan dari urutan ini bukan bahwa ia cepat selesai, melainkan bahwa **ia sempat ada**:
> gerbang menilai kelengkapan **keputusan**, dan sebagian gap hanya muncul ketika konsepnya
> benar-benar **dimodelkan**. Menghapus jejaknya akan membuat pelajaran itu ikut hilang.
>
> `S4c` tidak pernah terkena hal yang sama, dan sebabnya sederhana: **narasi tidak menunjuk data
> induk apa pun.**

### A3.17 Handoff

**Ke `design-business-module`:** **`S4b` dan `S4c`** — keduanya pengisian hasil, `S4c` **tanpa**
bagian gambar.

| Field | Nilai |
|---|---|
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN` dari `LAB-RCG-001-r7` bagian 0B.5 dan 0B.6 |
| Kesiapan arsitektur | `DOMAIN_ARCHITECTURE_READY` untuk keduanya |
| Konsep yang dibawa | `LAB-DC-036` Isolat, `LAB-DC-037` Kepekaan Antibiotik, `LAB-DC-038` Laporan Patologi Anatomi (`VALUE_OBJECT`), `LAB-DC-040` Bentuk Hasil (`Extend`), `LAB-DC-041` Organisme, `LAB-DC-042` Antibiotik |
| Invariant yang dibawa | `INV-24` sampai `INV-31`, kecuali `INV-27` yang hanya berlaku `S4b` |
| Pekerjaan yang **wajib ikut direncanakan** | Dua data induk baru **beserta cara mengisinya**. Ditulis tegas karena `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan data induk tanpa endpoint tulis adalah kegagalan yang **sudah berulang dua kali** di modul ini |
| Yang **tidak boleh** muncul | Status hasil apa pun; jalur validasi/rilis; penilaian kritis otomatis; penanda `Definitif`; skema penomoran wadah Patologi Anatomi (`S2b` belum siap) |
| Yang dikecualikan | `LAB-DC-039` Gambar Hasil — menunggu `DEC-LAB-016` |

**Ke `grill-me`:** `DEC-LAB-016` saja. `DEC-LAB-015` sudah ditutup `LAB-DEC-084`.

**Yang harus berhenti:** hanya bagian **gambar** `S4c`. Nol slice berhenti seluruhnya.

---

## A4. Revision 7 — `S4c` DIRANCANG ULANG sesudah `LAB-EVD-003`

> **Bagian ini MENGGANTI seluruh bagian A3 sejauh menyangkut `S4c`.** Bagian A3 untuk `S4b`
> Mikrobiologi, `LAB-DC-036`, `LAB-DC-037`, `LAB-DC-040` sampai `LAB-DC-042`, dan `INV-24`,
> `INV-26`, `INV-27`, `INV-28`, `INV-29`, `INV-30`, `INV-31` **tetap berlaku apa adanya**.
>
> **Yang dicabut:** `LAB-DC-038` Laporan Patologi Anatomi sebagai `VALUE_OBJECT`, dan `INV-25`.
> Keduanya benar terhadap BR-23; BR-23 sendiri yang ternyata **tidak lengkap**.

### A4.1 Identitas dan gerbang masuk

| Field | Nilai |
|---|---|
| Revision arsitektur | `LAB-DA-001` revision `7` |
| Scope yang dinilai | **`S4c` saja** — pengisian hasil Patologi Anatomi |
| Pemicu | Bukti `LAB-EVD-003`, direkonsiliasi `PARTIALLY_RECONCILED` putaran 4; amendment pass putaran 8 menutup 7 pertentangan dan 4 butir terbuka |
| Keputusan yang mengikat | `LAB-DEC-085` sampai `LAB-DEC-094` |
| Kesiapan requirement | Slice siap yang **berdiri sendiri**; nol keputusan pemblokir milik pemilik modul tersisa |
| Tanggal | 2026-09-18 |
| Backend SHA | `5ee03294` |
| Baseline rujukan | Tidak dipakai |

### A4.2 Ubiquitous language — istilah yang ditambahkan

| Istilah | Makna bisnis tunggal |
|---|---|
| **Laporan Patologi Anatomi** | Satu laporan diagnostik atas **satu pesanan** PA. Bukan per pemeriksaan — sebab beberapa pemeriksaan pada satu pesanan lazimnya mengerjakan **satu jaringan yang sama** |
| **Parameter** | Satu ruas isian pada laporan PA, misalnya *Makroskopik* atau *Reseptor Estrogen*. Punya **identitas**, sehingga ruas yang sama pada dua kategori tetap **satu** ruas |
| **Kategori Patologi Anatomi** | Histologi, Sitologi Ginekologi, Sitologi Non-Ginekologi, atau Imunohistokimia. Melekat pada **jenis pemeriksaan**, bukan pada namanya |
| **Keberlakuan parameter** | Pernyataan bahwa sebuah parameter dipakai oleh sebuah kategori, dan wajib atau tidak di sana |
| **Konteks klinis pesanan** | Diagnosa Awal, Riwayat Penyakit Relevan, Masa Terakhir Haid, dan Keterangan Klinis — **milik pesanan**, ditulis dokter pemesan |
| **Final** | Patolog menyatakan tulisannya selesai. **Bukan rilis.** Lihat peringatan A4.3 |
| **Status temuan PA** | Normal / Perlu Perhatian / Kritis — sebuah **nilai**, sederajat status temuan Mikrobiologi |

> ### A4.3 Satu kata yang paling berbahaya pada seluruh dokumen ini
>
> **`Final` di sini BUKAN rilis.** Ia berarti patolog selesai menulis, dan tidak lebih.
>
> | Yang `Final` berarti | Yang `Final` TIDAK berarti |
> |---|---|
> | Tulisan patolog selesai dan terkunci dari penyuntingan biasa | Hasil sudah sah untuk dikirim ke dokter pemesan maupun pasien |
> | Dicatat sebagai **fakta**: `FinalizedAt`, `FinalizedByUserId` | Sebuah **status** pada siklus hidup hasil |
>
> **Kenapa ini dijaga sekeras itu.** `LAB-DEC-003` — ditandatangani `DR-LAB-003` pada 2026-09-17 —
> melarang pengisi hasil memvalidasi dan merilis hasil yang sama. `LAB-DEC-090` menetapkan
> pengisi hasil PA adalah Dokter Lab. **Bila `Final` diartikan rilis, keduanya bertabrakan pada
> orang yang sama.** Rilis Patologi Anatomi tetap `S4e`, dan `S4e` tertahan `DEC-LAB-011`.

### A4.4 Katalog konsep domain — `S4c` sesudah dirancang ulang

| ID | Nama bisnis | Klasifikasi | Pemilik | Ownership | Identitas | Invariant penting | Bukti |
|---|---|---|---|---|---|---|---|
| ~~`LAB-DC-038`~~ | ~~Laporan PA sebagai `VALUE_OBJECT`~~ | **DICABUT** | — | — | — | — | Benar terhadap BR-23; BR-23 tidak lengkap |
| `LAB-DC-043` | **Laporan Patologi Anatomi** | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri; **tepat satu per pesanan PA** | Tidak dapat difinalkan bila parameter wajibnya belum lengkap | `LAB-DEC-085`, `LAB-DEC-088` |
| `LAB-DC-044` | **Nilai Parameter Laporan** | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri di dalam laporan | Satu parameter muncul **sekali** per laporan | `LAB-DEC-086` |
| `LAB-DC-045` | **Parameter Patologi Anatomi** | `REFERENCE_DATA` | `BC-LAB` | `New` — berprefix `Lab` | Kode parameter yang unik | Kode unik; nonaktif tidak dapat dipakai pada nilai baru | `LAB-DEC-086` |
| `LAB-DC-046` | **Kategori Patologi Anatomi** | `REFERENCE_DATA` | `BC-LAB` | `New` — berprefix `Lab` | Kode kategori yang unik | Empat baris awal; kategori kelima cukup menambah data | `LAB-DEC-086`, `LAB-DEC-087` |
| `LAB-DC-047` | **Keberlakuan Parameter per Kategori** | `ENTITY` | `BC-LAB` | `New` | Pasangan parameter + kategori | Satu pasangan tidak berulang; membawa penanda **wajib** | `LAB-DEC-086` |
| `LAB-DC-048` | **Pemetaan Jenis Pemeriksaan ke Kategori PA** | `ENTITY` | `BC-LAB` | `New` | Satu jenis pemeriksaan menunjuk satu kategori | Pemeriksaan tanpa pemetaan **nol menyumbang parameter** | `LAB-DEC-087` |
| `LAB-DC-049` | **Konteks Klinis Pesanan PA** | `ENTITY` | `BC-LAB` | `New` | Tepat satu per pesanan PA | Ditulis **dokter pemesan**, bukan patolog | `LAB-DEC-091` |
| `LAB-DC-050` | **Penanggung Jawab Analis** | — | `BC-PLAT` / tenaga kerja | `Existing` — **dirujuk saja** | Milik data induk tenaga kerja | Nol data induk baru; nol salinan | `LAB-DEC-093` |

#### Kenapa `LAB-DC-043` kini `ENTITY`, bukan `VALUE_OBJECT`

Revision 6 menilainya `VALUE_OBJECT` atas tiga alasan, dan **ketiganya gugur** oleh bukti baru:

| Alasan revision 6 | Kenapa gugur |
|---|---|
| Ketiga bagiannya selalu utuh | Ruasnya **bukan tiga**, melainkan sampai lima belas, dan **mana yang wajib bergantung kategori** |
| Nol konsep lain menunjuk kepadanya | Kini **nilai parameter** menunjuk kepadanya, dan nilai itu ditambah serta dikurangi satu per satu |
| Ia berubah sebagai satu kesatuan | Kini ia punya **lifecycle sendiri**: difinalkan, dibuka kembali, difinalkan lagi — dan tiap perpindahan meninggalkan jejak |

**Sesuatu yang punya lifecycle dan punya yang menunjuk kepadanya adalah entity.** Penilaian
revision 6 benar terhadap bukti yang ada saat itu; buktinya yang bertambah.

#### Yang sengaja **tidak** dijadikan konsep

| Yang ditolak | Alasan |
|---|---|
| Konsep terpisah per kategori — `LaporanHistologi`, `LaporanIHK` | Satu pesanan dapat memuat beberapa kategori sekaligus, dan `RULE-013` menuntut penggabungan tanpa duplikasi. Tiga konsep akan membuat penggabungan itu menjadi penggabungan lintas konsep — bagian tersulitnya, tanpa imbalan |
| Konsep "Status Hasil PA" sebagai entity | Ia sebuah **nilai** pada laporan, sederajat status temuan Mikrobiologi (`LAB-DEC-094`) |
| Konsep "Waktu Issued" dan "Waktu Efektif" | **Nol disimpan.** Keduanya diturunkan (`LAB-DEC-092`) |
| Konsep "Draft" dan "Final" sebagai status | `LAB-DEC-080`, `LAB-DEC-088`. Keduanya dibaca dari `FinalizedAt` |
| Data induk penanggung jawab analis milik Laboratorium | Duplikasi data induk tenaga kerja. Dilarang tegas |

### A4.5 Model aggregate

**Nol aggregate baru.** Ketujuh konsep transaksionalnya masuk `AGG-LAB-01` Pesanan Laboratorium.

| Pertanyaan | Jawaban |
|---|---|
| Kenapa laporan PA tidak menjadi aggregate sendiri? | Membatalkan pesanan wajib membatalkan isinya serentak (`INV-03`), dan laporan termasuk isinya. Laporan **nol bermakna** tanpa pesanannya |
| Apakah batasnya menjadi terlalu besar? | **Ya, dan itu dicatat.** `ARCH-GAP-LAB-05` sudah menandainya pada revision 5; revision ini **memperbesarnya lagi**. Lihat `ARCH-GAP-LAB-06` |

**Invariant yang ditambahkan:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-32` | Satu pesanan Patologi Anatomi memiliki **paling banyak satu** laporan | `LAB-DEC-085` |
| `INV-33` | Sebuah nilai parameter hanya sah bila parameternya **berlaku bagi sekurang-kurangnya satu kategori** yang melekat pada pesanan itu | `LAB-DEC-086`, `LAB-DEC-087` |
| `INV-34` | Laporan **tidak dapat difinalkan** selama masih ada parameter wajib yang kosong. Himpunan wajibnya adalah **gabungan** dari seluruh kategori pesanan, **tanpa duplikasi** | `LAB-DEC-086`; `RULE-010`, `RULE-013` |
| `INV-35` | `FinalizedAt` terisi berarti laporan **terkunci dari penyuntingan biasa**; pengosongannya hanya lewat `Reopen`, dan setiap `Reopen` meninggalkan jejak | `LAB-DEC-088` |
| `INV-36` | **Nol status lifecycle** disimpan pada laporan PA | `LAB-DEC-080`, menggantikan `INV-29` pada scope ini |
| `INV-37` | Parameter atau kategori yang **dinonaktifkan** tidak dapat dipakai pada nilai baru, tetapi **tidak mengubah** laporan lama yang sudah menunjuknya | `LAB-DEC-086`; pola `INV-31` |
| `INV-38` | **Waktu Issued dan Waktu Efektif nol disimpan.** Keduanya diturunkan dari `FinalizedAt` dan `LabSpecimen.CollectedAt` | `LAB-DEC-092` |
| `INV-39` | Jenis pemeriksaan **tanpa pemetaan kategori** nol menyumbang parameter, dan sistem **nol menebak** kategorinya | `LAB-DEC-087`; `RULE-036` |
| `INV-40` | Konteks klinis pesanan ditulis **dokter pemesan**; patolog membacanya dan **tidak menulisnya** | `LAB-DEC-091` |

**Tindakan bisnis yang ditambahkan:**

| Tindakan | Wewenang | Menerbitkan fakta? |
|---|---|---|
| Menulis konteks klinis pesanan PA | Dokter pemesan | Tidak |
| Mengisi dan menyunting nilai parameter laporan | **Dokter Lab** (`LAB-DEC-090`) | Tidak |
| Menetapkan status temuan PA | Dokter Lab | Tidak |
| Menunjuk penanggung jawab analis | Dokter Lab | Tidak |
| **Memfinalkan** laporan | Dokter Lab | Tidak — **dan ini yang paling mudah salah dikira menerbitkan sesuatu** |
| **Membuka kembali** laporan yang sudah final | Dokter Lab | Tidak |

> **Nol di antaranya menerbitkan fakta ke Billing.** Titik kelayakan tagih tetap pada penerimaan
> wadah (`LAB-INH-009`..`012`). Memfinalkan laporan **nol** mengubah uang.

### A4.6 Model relasi

| Sumber | Tujuan | Makna | Kardinalitas | Wajib? | Lifecycle |
|---|---|---|---|---|---|
| `LAB-DC-001` Pesanan | `LAB-DC-043` Laporan PA | Satu pesanan PA menghasilkan satu laporan | 1 : 0..1 | Opsional sampai diisi | Laporan mati bersama pesanannya |
| `LAB-DC-001` Pesanan | `LAB-DC-049` Konteks Klinis | Konteks klinis melekat pada pesanan | 1 : 0..1 | Opsional pada penyimpanan; **wajib dibaca** patolog | Mati bersama pesanannya |
| `LAB-DC-043` Laporan | `LAB-DC-044` Nilai Parameter | Laporan berisi sejumlah nilai | 1 : 0..n | — | Nilai mati bersama laporannya |
| `LAB-DC-044` Nilai | `LAB-DC-045` Parameter | Nilai **menunjuk** parameter yang dikenal | n : 1 | **Wajib** | Data induk hidup lebih lama |
| `LAB-DC-045` Parameter | `LAB-DC-047` Keberlakuan | Parameter berlaku bagi sejumlah kategori | 1 : 1..n | Wajib — parameter tanpa kategori nol dapat dipakai | — |
| `LAB-DC-046` Kategori | `LAB-DC-047` Keberlakuan | Kategori memakai sejumlah parameter | 1 : 1..n | Wajib | — |
| `LAB-DC-021` Jenis Pemeriksaan | `LAB-DC-048` Pemetaan | Jenis pemeriksaan menunjuk satu kategori PA | 1 : 0..1 | **Opsional** — dan ketiadaannya bermakna (`INV-39`) | `BC-MD` pemilik katalog; pemetaannya milik `BC-LAB` |
| `LAB-DC-043` Laporan | `LAB-DC-050` Penanggung Jawab Analis | Laporan menyebut analis yang menyiapkan jaringan | n : 1 | Opsional | Dirujuk saja |

**Dua pola `S4a` yang tetap diwarisi**, dan alasannya tidak berubah: **penunjuk beserta
snapshot** disimpan berdampingan — penunjuk untuk menghitung, snapshot untuk membaca ulang
bertahun kemudian; dan **kapan dikerjakan** tetap terpisah dari **kapan diketik**.

### A4.7 Model lifecycle

**Nol status.** Seluruh pertanyaan dijawab dari fakta:

| Pertanyaan bisnis | Dijawab dari |
|---|---|
| Apakah laporannya sudah mulai diisi? | Ada sekurang-kurangnya satu nilai parameter |
| Apakah patolog sudah menyatakan selesai? | `FinalizedAt != null` |
| Apakah pernah dibuka kembali? | `ReopenCount > 0`, beserta jejak tiap kejadiannya |
| Kapan laporan diterbitkan? | **Diturunkan** dari `FinalizedAt` (`INV-38`) |
| Kapan bahannya diambil? | **Diturunkan** dari `LabSpecimen.CollectedAt` (`INV-38`) |
| Apakah laporannya **sah dikirim**? | **Tidak terjawab, dan memang belum boleh.** Itu rilis — `S4e`, tertahan `DEC-LAB-011` |

**Perpindahan yang dikenali:**

| Dari | Tindakan | Ke | Wewenang | Syarat | Jejak |
|---|---|---|---|---|---|
| Belum final | Mengisi dan menyunting nilai | Belum final | Dokter Lab | — | Perubahan nilai |
| Belum final | **Memfinalkan** | Final | Dokter Lab | Seluruh parameter wajib terisi (`INV-34`) | `FinalizedAt`, `FinalizedByUserId` |
| Final | Menyunting nilai | **ditolak** | — | — | — |
| Final | **Membuka kembali** | Belum final | Dokter Lab | — | `ReopenedAt`, pelaku, **alasan** |

> **`Reopen` di sini bukan koreksi hasil terrilis.** Selama laporan belum dirilis, membukanya
> kembali adalah penyuntingan biasa — dan itulah sebab `REC4-CONF-006` larut tanpa perlu
> diputuskan. Koreksi **sesudah rilis** tetap `S6`, dan `DEC-LAB-014` tetap terbuka di sana.

### A4.8 Tanggung jawab authorization

| Tanggung jawab | Pemegang | Bukti |
|---|---|---|
| Menulis konteks klinis pesanan | **Dokter pemesan** | `LAB-DEC-091`, `INV-40` |
| Mengisi, memfinalkan, membuka kembali laporan | **Dokter Lab** | `LAB-DEC-090` |
| Membaca dan mencetak | Dokter Lab **dan** Petugas Lab | `LAB-DEC-090` |
| Mengelola data induk parameter, kategori, dan pemetaan | Kepala instalasi | Pola `LabSpecimenType` |
| **Memvalidasi dan merilis** | **Di luar scope** — `S4e`, tertahan `DEC-LAB-011` | `LAB-DEC-088` |

> **`LAB-DEC-068` tetap utuh dan tidak bertabrakan dengan baris pertama tabel ini.** Ia mengatur
> tombol pada **menu Hasil** — Nota Lab, Label, Kirim ke Pasien — dan itu tetap dipegang Petugas
> Lab dan/atau Admin. Halaman **Detail Hasil** adalah layar yang berbeda.

### A4.9 Model audit dan histori

| Yang wajib terlacak | Alasan |
|---|---|
| Siapa memfinalkan, dan kapan | Ia pernyataan profesional seorang patolog atas diagnosis |
| **Setiap `Reopen` beserta alasannya** | Membuka kembali laporan yang sudah dinyatakan selesai adalah tindakan yang perlu dapat dijelaskan. `ReopenCount` saja **tidak cukup** — yang dibutuhkan riwayat per kejadian |
| Perubahan nilai parameter sesudah `Reopen` | Yang berubah sesudah dibuka kembali adalah **diagnosis**, bukan catatan operasional |
| Siapa menulis konteks klinis, dan kapan | Ia masukan dokter pemesan yang menjadi dasar penafsiran patolog |

> **Bentuk jejaknya sengaja tidak ditetapkan di sini** — riwayat per baris atau penandaan versi
> adalah keputusan perancangan blueprint. Yang **ditetapkan** adalah bahwa `Reopen` dan perubahan
> nilai sesudahnya **wajib dapat ditelusuri per kejadian**, bukan hanya dihitung.

### A4.10 Model integrasi

| Batas | Keadaan |
|---|---|
| Data induk tenaga kerja | **Dirujuk**, untuk penanggung jawab analis. Nol salinan |
| Katalog jenis pemeriksaan (`BC-MD`) | **Dirujuk** oleh pemetaan kategori. Katalognya nol disentuh dari `BC-LAB` |
| `HL7` | **Di luar scope** — `LAB-COORD-012`, dan `HL7` nol kemunculan di seluruh backend |
| Layanan terjemahan otomatis | **Di luar scope** — `LAB-COORD-013`, beserta izin privasinya |
| Alat laboratorium | **Nol** — `LAB-DEC-005` |
| Billing | **Nol** |

### A4.11 Dampak billing

**Tidak ada dampak charge yang diketahui.** `INV-09` tetap berlaku penuh.

### A4.12 Dampak keselamatan klinis

**Klinis, relevan terhadap keselamatan — dan satu perpindahannya kini berada di dalam scope.**

| Aspek | Keadaan |
|---|---|
| Aturan keselamatannya | ✅ Ditandatangani `DR-LAB-003` (`LAB-DEC-079`) |
| **Memfinalkan laporan** | **Di dalam scope**, dan ia pernyataan profesional atas sebuah diagnosis. Karena itu `INV-34` menolak finalisasi yang belum lengkap, dan jejaknya wajib |
| Validasi, rilis, nilai kritis, koreksi terrilis | **Di luar scope** |
| Penilaian kritis otomatis | **Nol** (`INV-28`). Status temuan PA dipilih **manual** (`LAB-DEC-094`) |
| Akibat yang perlu disadari | Laporan PA yang sudah **Final** tetap **belum dapat dirilis** sampai `DEC-LAB-011` dijawab. Tumpukannya bertambah, sama seperti Patologi Klinik sejak 2026-09-17 |

### A4.13 Gap arsitektur

| ID | Gap | Dampak | Pemilik |
|---|---|---|---|
| `ARCH-GAP-LAB-06` | **`AGG-LAB-01` membesar lagi.** Satu pesanan kini dapat memuat pemeriksaan, wadah, isolat, kepekaan antibiotik, konteks klinis, laporan PA, dan nilai parameternya sekaligus. `ARCH-GAP-LAB-05` sudah menandainya pada revision 5; revision ini menambah **empat** konsep transaksional lagi | Belum menimbulkan pertentangan invariant. **Wajib ditinjau saat `S4d`/`S4e` dirancang** | Tim pembangun sistem |
| `ARCH-GAP-LAB-07` | **Konteks klinis menyentuh alur pemesanan yang SUDAH BERJALAN.** `LAB-DEC-091` menetapkan keempat ruas diisi dokter pemesan — dan layar pemesanan beserta `POST /lab-orders/by-examinations` sudah dibangun dan dipakai. Menambahkannya berarti menyentuh jalur yang sedang hidup | **Bukan penahan**, tetapi biayanya nyata dan **tidak boleh ditemukan saat implementasi**. Ruasnya opsional pada penyimpanan, sehingga pesanan lama nol terdampak | Tim pembangun sistem |
| `DEC-LAB-016` | Lampiran gambar laporan PA | **`BLOCKING`** bagi bagian gambar; nol memblokir laporan narasinya | Platform + pemilik modul |
| `LAB-COORD-012` | Sumber data `HL7` | **`BLOCKING`** bagi ruas HL7 saja | Platform |
| `LAB-COORD-013` | Terjemahan otomatis + izin privasi | **`BLOCKING`** bagi cetak bilingual saja | Platform + pemilik modul |
| `S2b` | Atribut wadah khas PA — lokasi, pola, metode pengambilan | **`BLOCKING`** bagi bagian Informasi Specimen halaman ini; nol memblokir laporannya | Yoga Aji Pratama |

### A4.14 Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY` untuk `S4c`**, dinyatakan **berdiri sendiri**.

| Yang dikirim | Yang dikecualikan |
|---|---|
| Laporan PA per pesanan beserta nilai parameternya, ketiga data induknya, pemetaan kategori, konteks klinis pesanan, status temuan, penanggung jawab analis, serta finalisasi dan pembukaan kembali | Lampiran gambar (`DEC-LAB-016`); ruas HL7 (`LAB-COORD-012`); cetak bilingual (`LAB-COORD-013`); Informasi Specimen lokasi/pola/metode (`S2b`); seluruh jalur validasi dan rilis (`S4e`) |

> **Keempat pengecualian itu bagian, bukan slice.** Laporan Patologi Anatomi tetap utuh dan dapat
> dipakai patolog tanpa satu pun di antaranya — ia menulis, menyatakan selesai, dan laporannya
> terbaca. Yang belum ada adalah **jalan keluarnya**, dan itu memang `S4e`.

### A4.15 Handoff

**Ke `design-business-module`:** `S4c` dengan bentuk barunya.

| Field | Nilai |
|---|---|
| Kesiapan arsitektur | `DOMAIN_ARCHITECTURE_READY`, berdiri sendiri |
| Revision arsitektur | `LAB-DA-001` rev 7 |
| Konsep yang dibawa | `LAB-DC-043` sampai `LAB-DC-050` |
| Invariant yang dibawa | `INV-32` sampai `INV-40`; ditambah `INV-24`, `INV-28`, `INV-37` dari revision 5-6 |
| Yang **wajib** ikut direncanakan | **Tiga data induk beserta cara mengisinya** — parameter, kategori, dan keberlakuan; ditambah **pemetaan jenis pemeriksaan**, yang tanpa isinya nol pemeriksaan PA punya kategori dan **nol parameter akan muncul di layar** |
| Yang **wajib** dinyatakan sebagai biaya | `ARCH-GAP-LAB-07` — alur pemesanan yang sudah berjalan ikut tersentuh |
| Yang **tidak boleh** muncul | Status hasil lifecycle; tombol Rilis; penilaian kritis otomatis; ruas Waktu Issued/Efektif yang dapat diketik; lampiran gambar; ruas HL7; cetak bilingual |
| Kontrak yang wajib diamandemen | `LAB-API-v1` **`r25`** — `r24` bagian 19.3 sudah ditandai `superseded` |

**Ke `grill-me`:** nol butir tersisa untuk `S4c` yang dapat ditutup pemilik modul.

---

## A5. Revision 8 — Slice `S4` validasi dan rilis hasil Patologi Klinik (2026-09-24)

### A5.1 Identitas dan gerbang masuk

| Butir | Isi |
|---|---|
| Scope | **`S4`** — validasi dan rilis hasil **Patologi Klinik** saja, dipersempit `LAB-DEC-076`. `S4d`, `S4e`, `S5`, dan `S6` **tidak** dirancang |
| Kesiapan requirement | `LAB-RCG-001-r8` bagian 0C: **`READY_FOR_DOMAIN_DESIGN` untuk desain saja** |
| Penahan pemakaian yang dibawa serta | `DEC-LAB-011` sisa (pemvalidasi kedua per shift), `DEC-LAB-017` (pemakaian sebelum `S5`), `DEC-LAB-018` (pemegang rilis), `LAB-COORD-016` (kode kewenangan di katalog Human Resource) |
| Bukti | Decisions rev 73; capability map rev 5; kontrak `LAB-API-v1` `r33`, `LAB-PERM-v1` rev 10, `LAB-STATE-v1` `r4` (approved 2026-09-24) |
| Snapshot kode | BE `ddeb5ed8`, FE `72607a087` |
| Baseline rujukan | **Tidak dipakai** — seluruh butir bersumber keputusan modul ini |

**Decision ID yang mengikat:** `LAB-DEC-003` empat mata, `LAB-DEC-017` hasil terdaftar di rekam
medis, `LAB-DEC-080` fakta bukan status, `LAB-DEC-120` perilis ≠ pemvalidasi, `LAB-DEC-135`
Draft/Final, `LAB-DEC-138` Kembalikan ke analis, `LAB-DEC-142`/`143` dua lapis per disiplin,
`LAB-DEC-146` izin hasil, `LAB-DEC-148` kredensial Human Resource, `LAB-DEC-150` hanya dokter.

### A5.2 Ubiquitous language — istilah yang ditambahkan

| Istilah | Arti |
|---|---|
| **Validasi** | Pernyataan dokter berkewenangan laboratorium bahwa angka hasil **benar** — sesudah diperiksa terhadap gambaran klinis dan data pasien |
| **Rilis** (tercetak: *Otorisasi*) | Pernyataan orang kedua yang berwenang bahwa hasil yang sudah divalidasi **boleh dibaca** dokter pemesan dan menjadi dokumen klinis pasien |
| **Pemvalidasi** / **Perilis** | Orang yang melakukan kedua tindakan di atas. Pada hasil yang sama, keduanya **tidak boleh** orang yang sama |
| **Penunjukan kewenangan** | Kewenangan klinis per orang, per disiplin, per jenis tindakan, bermasa berlaku — **dicatat di Human Resource**, dibaca Laboratorium |
| **Dasar kewenangan** | Penunjukan mana yang berlaku saat seseorang memvalidasi atau merilis |
| **Snapshot peran** | Jabatan orang itu **pada saat** bertindak, disimpan bersama tindakannya supaya tidak ikut berubah kemudian |
| **Kembalikan ke analis** | Membatalkan validasi atas hasil yang **belum dirilis** sehingga hasil kembali Draft — berbeda dari *koreksi*, yang hanya berlaku sesudah rilis |
| **Antrean validasi** | **Cara menyajikan** hasil Final yang belum divalidasi — **bukan** konsep tersimpan |

### A5.3 Peta bounded context — perubahan

| ID | Nama | Tanggung jawab bagi `S4` | Hubungan |
|---|---|---|---|
| `BC-LAB` | Laboratorium | Pemilik tunggal fakta validasi, rilis, dan pengembalian | — |
| `BC-HR` | Human Resource — Credentialing | **Pemilik** penunjukan kewenangan dan katalog kodenya (`WfpClinicalPrivilege`, `MstClinicalPrivilegeCatalog`) | **Upstream** bagi `BC-LAB`; `BC-LAB` **hanya membaca** (`LAB-DEC-148`) — sejalan preseden Kamar Operasi |
| `BC-MRC` | Rekam Medis | **Pemilik** catatan keutuhan dokumen klinis (`MrcClinicalDocumentIntegrity`) | **Downstream** bagi `BC-LAB`: menerima pendaftaran dokumen hasil saat rilis (`LAB-DEC-017`, `LAB-COORD-002` disetujui) |
| `BC-PLAT` | Platform | Hak akses per aksi | Sudah ada; tidak berubah |

**Dua context baru disebut di dokumen ini, dan nol yang baru dibuat:** `BC-HR` dan `BC-MRC`
sudah berdiri di aplikasi. Yang baru hanya **hubungannya** dengan `BC-LAB`.

### A5.4 Katalog konsep domain — yang ditambahkan

| ID | Nama bisnis | Klasifikasi | Pemilik | Ownership | Invariant penting | Bukti |
|---|---|---|---|---|---|---|
| `LAB-DC-051` | **Validasi Hasil** | `VALUE_OBJECT` pada `LAB-DC-002` Pemeriksaan | `BC-LAB` | `Extend` | Pelaku, waktu, snapshot peran, dan — bila pelaku juga pengisi — alasan dari `LAB-DC-058`; **paling banyak satu yang berlaku** pada satu saat | `LAB-DEC-080`, `LAB-DEC-150` |
| `LAB-DC-052` | **Rilis Hasil** | `VALUE_OBJECT` pada `LAB-DC-002` | `BC-LAB` | `Extend` | Pelaku, waktu, snapshot peran; hanya sesudah validasi; pelaku ≠ pemvalidasi, **kecuali** pengecualian beralasan dari `LAB-DC-058` dengan penanda permanen | `LAB-DEC-080`, `LAB-DEC-120` |
| `LAB-DC-053` | **Pengembalian ke Analis** | `DOMAIN_EVENT` yang tercatat di riwayat | `BC-LAB` | `Extend` riwayat perpindahan | Hanya sebelum rilis; wajib beralasan dari daftar terkendali; **validasi yang dibatalkan tetap terbaca** di riwayat | `LAB-DEC-138` |
| `LAB-DC-054` | **Alasan Koreksi dan Pengembalian** | `REFERENCE_DATA` | `BC-LAB` | `New` | Daftar **satu** untuk pengembalian sebelum rilis dan koreksi sesudah rilis; kode unik; nonaktif tidak dipakai baris baru | `LAB-DEC-082`, `LAB-DEC-138`; pola `MstLabRejectionReason` |
| `LAB-DC-055` | **Penunjukan Kewenangan Klinis** | `EXTERNAL_CONTRACT` | **`BC-HR`** | `Adapter/View` | Berlaku hanya bila kodenya sesuai, statusnya aktif, dan tanggalnya dalam masa berlaku | `LAB-DEC-148`; `CAP-P14-09` |
| `LAB-DC-056` | **Kode Kewenangan Laboratorium** | `REFERENCE_DATA` | **`BC-HR`** | `Extend` pada katalog Human Resource — lewat `LAB-COORD-016` | Enam kode: validasi dan rilis × tiga disiplin. `S4` hanya memakai **dua** — validasi dan rilis Patologi Klinik | `LAB-DEC-143`, `LAB-DEC-148` |
| `LAB-DC-057` | **Dokumen Hasil Laboratorium** di rekam medis | `EXTERNAL_CONTRACT` | **`BC-MRC`** | `Extend` jenis dokumen — sudah disepakati `LAB-COORD-002` | **Tepat satu** pendaftaran per rilis; angka hasil **tidak** disalin | `LAB-DEC-017` |
| `LAB-DC-058` | **Alasan Pengecualian Empat Mata** | `REFERENCE_DATA` | `BC-LAB` | `New` | Daftar terkendali alasan ketika satu orang memegang dua peran pada hasil yang sama — misalnya *"Shift tunggal, tidak ada dokter lain bertugas"*. **Terpisah dari `LAB-DC-054`**: yang satu menjelaskan *kenapa kewenangan dirangkap*, yang lain *kenapa hasil diubah* — dua pertanyaan pelaporan yang berbeda | `LAB-DEC-003` butir 1; `LAB-DEC-120` butir 2 |
| `LAB-EV-01` | **Hasil Divalidasi** | `DOMAIN_EVENT` | `BC-LAB` | `New` | Terbit sekali per validasi yang berlaku | — |
| `LAB-EV-02` | **Hasil Dirilis** | `DOMAIN_EVENT` | `BC-LAB` | `New` | Memicu pendaftaran `LAB-DC-057` | — |
| `LAB-EV-03` | **Hasil Dikembalikan** | `DOMAIN_EVENT` | `BC-LAB` | `New` | Sama dengan `LAB-DC-053` | — |

#### Yang sengaja **tidak** dijadikan konsep

| Yang ditolak | Alasan |
|---|---|
| Status `Validated` dan `Released` pada lifecycle pemeriksaan | `LAB-DEC-080` — fakta, bukan status |
| Antrean validasi sebagai entity | Ia **penyajian** hasil Final yang belum divalidasi |
| Daftar pantau pengecualian sebagai entity | Ia **penyajian** validasi atau rilis yang berpenanda pengecualian — contoh BR-01: kepala instalasi memeriksanya keesokan pagi. Penandanya sudah tersimpan pada tindakannya; daftar tersendiri hanya menjadi salinan kedua yang bisa berselisih |
| Daftar penunjukan milik Laboratorium | `LAB-DEC-148` — dua sumber penunjukan pasti suatu hari berbeda |
| Aggregate "Validasi" tersendiri | Validasi adalah **bagian dari perjalanan pemeriksaan**; memisahkannya menciptakan dua sumber kebenaran atas satu kejadian — alasan yang sama dengan A3.6 |

### A5.5 Model aggregate — dan tinjauan ulang `ARCH-GAP-LAB-05`

**Nol aggregate baru.** Validasi dan rilis masuk `AGG-LAB-01` lewat `LAB-DC-002` Pemeriksaan.

`ARCH-GAP-LAB-05` meminta aggregate ini **ditinjau ulang saat validasi dan rilis dirancang**.
Jawaban tinjauannya:

| Pertanyaan | Jawaban |
|---|---|
| Apakah validasi satu pemeriksaan perlu mengunci seluruh pesanan? | **Tidak.** Setiap tindakan mengubah **satu** pemeriksaan, dan konsistensinya cukup dijaga per pemeriksaan |
| Invariant lintas pesanan apa yang tetap ada? | **Satu:** pemeriksaan pada pesanan atau wadah yang **dibatalkan atau gugur** tidak dapat divalidasi maupun dirilis (`INV-51`). Ia cukup diperiksa pada saat tindakan, di dalam transaksi yang sama |
| Apakah gap-nya ditutup? | **Menyempit, belum ditutup.** Pertumbuhan aggregate belum menimbulkan pertentangan invariant; tinjauan berikutnya saat `S6` dirancang, sebab koreksi sesudah rilis menyentuh versi hasil |

**Invariant yang ditambahkan:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-41` | Hanya hasil **Final** yang dapat divalidasi | `LAB-DEC-135` |
| `INV-42` | Pemvalidasi **bukan** pengisi hasil, kecuali pengecualian tercatat beserta alasan dan penanda permanen | `LAB-DEC-003` |
| `INV-43` | Perilis **bukan** pemvalidasi, kecuali pengecualian `LAB-DEC-003` tercatat beserta alasan dari `LAB-DC-058` dan penanda permanen yang **ikut tercetak**. **Inilah jalur pengecualian yang nyata pada Patologi Klinik** — pengisi (analis) dan pemvalidasi (dokter) tidak mungkin orang yang sama, tetapi pemvalidasi dan perilis mungkin: dokter tunggal pada malam hari | `LAB-DEC-120` butir 2, `LAB-DEC-003` |
| `INV-44` | Hanya hasil **tervalidasi** yang dapat dirilis | `LAB-INH-007` |
| `INV-45` | Pemvalidasi wajib **dokter berkewenangan laboratorium** (lapis jabatan) **dan** memegang penunjukan *validasi Patologi Klinik* yang aktif dalam masa berlakunya. Perilis wajib memegang penunjukan *rilis Patologi Klinik* | `LAB-DEC-142`, `143`, `148`, `150` |
| `INV-46` | Setiap validasi dan rilis menyimpan **snapshot peran** pelakunya pada saat itu | `LAB-DEC-150` butir 4 — validator `CONFIRMED`; perilis **`PROPOSED`**, lihat `ARCH-GAP-LAB-08` |
| `INV-47` | *Kembalikan ke analis* hanya sebelum rilis; hasil kembali Draft; validasi yang dibatalkan **tetap terbaca** di riwayat | `LAB-DEC-138` |
| `INV-48` | Reopen oleh analis **ditolak** sesudah hasil divalidasi | `LAB-DEC-135` butir 3 |
| `INV-49` | Hasil yang sudah **dirilis** tidak berubah kecuali lewat koreksi `S6` | `LAB-DEC-007` |
| `INV-50` | Setiap rilis mendaftarkan **tepat satu** dokumen hasil ke rekam medis | `LAB-DEC-017` |
| `INV-51` | Pemeriksaan pada pesanan atau wadah yang dibatalkan atau gugur tidak dapat divalidasi maupun dirilis | `INV-03` diperluas |

**Tindakan bisnis yang ditambahkan:**

| Tindakan | Wewenang | Menerbitkan |
|---|---|---|
| Memvalidasi | Pemegang penunjukan *validasi PK* yang lolos lapis jabatan | `LAB-EV-01` |
| Merilis | Pemegang penunjukan *rilis PK* | `LAB-EV-02` → pendaftaran rekam medis |
| Mengembalikan ke analis | Pemegang penunjukan validasi **atau** rilis PK | `LAB-EV-03` |

### A5.6 Model relasi

| Sumber | Tujuan | Makna | Kardinalitas | Lifecycle |
|---|---|---|---|---|
| `LAB-DC-002` Pemeriksaan | `LAB-DC-051` Validasi | Pemeriksaan divalidasi | 1 : 0..1 yang **berlaku**; validasi yang dibatalkan tinggal di riwayat | Hilang dari pemeriksaan saat dikembalikan, **tidak** dari riwayat |
| `LAB-DC-002` Pemeriksaan | `LAB-DC-052` Rilis | Pemeriksaan dirilis | 1 : 0..1 | Tetap selamanya pada rilis ini; koreksi `S6` menambah versi, bukan menghapus |
| `LAB-DC-051`/`052` | `LAB-DC-055` Penunjukan | Tindakan didasari satu penunjukan | n : 1 | **Snapshot** — penunjukan yang kemudian dicabut **tidak** mengubah validasi lama |
| `LAB-DC-052` Rilis | `LAB-DC-057` Dokumen rekam medis | Rilis menjadi dokumen klinis | 1 : 1 | Dokumen dikunci aturan rekam medis; koreksi memakai addendum (`LAB-DEC-020`) |
| `LAB-DC-053` Pengembalian | `LAB-DC-054` Alasan | Pengembalian beralasan | n : 1 | Alasan nonaktif tidak menghapus pengembalian lama |
| `LAB-DC-051`/`052` | `LAB-DC-058` Alasan pengecualian | Tindakan yang merangkap peran beralasan | n : 0..1 — terisi **hanya** bila pelaku merangkap | Alasan nonaktif tidak menghapus tindakan lama; penandanya tetap tercetak |

### A5.7 Model lifecycle — keadaan turunan, bukan status

| Dari | Tindakan | Ke | Siapa | Syarat |
|---|---|---|---|---|
| Final | Validasi | **Tervalidasi** | Pemvalidasi | `INV-41`, `42`, `45`, `51` |
| Tervalidasi | Rilis | **Dirilis** | Perilis | `INV-43`, `44`, `45`, `50`, `51` |
| Tervalidasi | Kembalikan ke analis | Draft | Pemvalidasi atau perilis | Alasan dari `LAB-DC-054`; `INV-47` |
| Final | Reopen | Draft | Analis | **Hanya sebelum validasi** — `INV-48` |
| Dirilis | Koreksi | — | — | **`S6`**, di luar slice ini |

**Tindakan tidak sah:** memvalidasi Draft; merilis tanpa validasi; memvalidasi hasil yang diisi
sendiri tanpa pengecualian; merilis hasil yang divalidasi sendiri tanpa pengecualian; Reopen
sesudah validasi; mengembalikan hasil yang sudah dirilis; bertindak atas pemeriksaan yang batal
atau gugur; bertindak dengan penunjukan yang ditangguhkan, dicabut, atau kedaluwarsa.

Label order *Dalam Pemeriksaan* dan *Selesai* (`LAB-DEC-135`) kini dapat diturunkan: *Selesai*
bila seluruh pemeriksaan yang tidak batal berada pada keadaan **Dirilis**.

### A5.8 Tanggung jawab authorization

| Tanggung jawab | Pemegang | Status bukti |
|---|---|---|
| Mengisi dan menyelesaikan hasil | Jabatan analis | `CONFIRMED` — `LAB-DEC-134`, `146` |
| Memvalidasi | Dokter berkewenangan laboratorium yang ditunjuk | `CONFIRMED` bentuknya; **isinya** dr. Bima sebagai pemegang pertama; pemegang kedua **`MISSING`** (`DEC-LAB-011`) |
| Merilis | Pemegang penunjukan rilis | **`MISSING`** — `DEC-LAB-018`. Arsitektur **tidak** menetapkan apakah wajib dokter |
| Menetapkan pemegang | dr. Bima, lewat kredensial Human Resource | `CONFIRMED` sebagai jawaban sebagian — `LAB-DEC-150` butir 2; konfirmasi tertulis diminta `LAB-REQ-014` |

### A5.9 Model audit dan histori

| Yang wajib terlacak | Alasan |
|---|---|
| Pemvalidasi, waktu, snapshot peran, alasan pengecualian bila ada | `LAB-DEC-150` butir 4, `LAB-DEC-003` |
| Perilis, waktu, snapshot peran, dan — bila ia juga pemvalidasi — alasan dari `LAB-DC-058` beserta penanda permanen | Baris *Otorisasi oleh* pada cetakan (`LAB-DEC-120`); penanda ikut tercetak dan tersimpan di riwayat yang tidak bisa diubah (BR-01 butir 2–3) |
| **Dasar kewenangan** — penunjukan mana yang berlaku saat bertindak | **`PROPOSED`.** Tanpanya, auditor yang bertanya *"atas dasar apa dr. X memvalidasi pada tanggal itu"* harus merangkai sendiri riwayat Human Resource yang bisa sudah berubah |
| Pengembalian: pelaku, waktu, alasan, **dan validasi yang dibatalkan** | `LAB-DEC-138` |

### A5.10 Model integrasi

| Batas | Produsen → konsumen | Sumber kebenaran | Sifat | Saat gagal |
|---|---|---|---|---|
| Penunjukan kewenangan | `BC-HR` → `BC-LAB` | `BC-HR` | Dibaca **saat tindakan**, tanpa salinan tetap | **Fail-closed:** data kewenangan tidak terbaca atau kosong → tindakan **ditolak** dengan alasan terbaca. Nol jalur pintas |
| Dokumen hasil | `BC-LAB` → `BC-MRC` | `BC-LAB` atas angka hasil; `BC-MRC` atas keutuhan dokumen | Keduanya pada basis data yang sama | **`PROPOSED`: atomik** — rilis dan pendaftarannya berhasil bersama atau gagal bersama. Lihat `ARCH-GAP-LAB-08` |
| Hak akses per aksi | `BC-PLAT` → `BC-LAB` | `BC-PLAT` | Sudah berjalan | Sudah berjalan |

**Idempotency.** Validasi atau rilis yang dikirim dua kali atas hasil yang sama ditolak pada
kiriman kedua; pendaftaran rekam medis **tepat satu** per rilis (`INV-50`).

### A5.11 Dampak billing

**Tidak ada dampak charge yang diketahui.** Kelayakan tagih terbit saat wadah dinyatakan layak
(`LAB-INH-009`), bukan saat validasi maupun rilis.

### A5.12 Dampak keselamatan klinis

**Relevan terhadap keselamatan.** Batas yang dibuat eksplisit:

| Risiko | Penjaga dalam arsitektur | Yang tetap di luar arsitektur |
|---|---|---|
| Salah ketik lolos | Empat mata `INV-42`; perilis ketiga `INV-43` | — |
| Orang tanpa kompetensi mengesahkan hasil | Dua lapis `INV-45`; fail-closed pada data kewenangan | Isi daftar pemegang — `DEC-LAB-011`, `DEC-LAB-018` |
| Hasil kritis dirilis tanpa jaring pelaporan | — | **`DEC-LAB-017`** — keputusan klinis, bukan arsitektur |
| Hasil kritis malam hari tanpa pemvalidasi | — | **`DEC-LAB-011` sisa** |

### A5.13 Gap arsitektur

| ID | Gap | Dampak | Pemilik |
|---|---|---|---|
| `ARCH-GAP-LAB-05` | Aggregate pesanan membesar — **ditinjau ulang** A5.5 | Menyempit; tinjau lagi saat `S6` | Tim pembangun sistem |
| `ARCH-GAP-LAB-08` | **Lima** usulan arsitektur yang belum dikonfirmasi: (1) snapshot peran **perilis**; (2) penyimpanan **dasar kewenangan**; (3) rilis-plus-pendaftaran rekam medis yang **atomik**; (4) **pengelola** daftar `LAB-DC-058` — *usulan:* kepala instalasi lewat layar pengelolaan dan data awal disiapkan pada rilis, mengikuti pola `LAB-DEC-019` seperti yang **ditetapkan** `LAB-DEC-082` bagi `LAB-DC-054`; `LAB-DEC-003` sendiri hanya menyebut *"alasan terkendali"*; (5) **bunyi penanda** saat pemvalidasi juga merilis — `LAB-DEC-003` hanya merumuskan *"divalidasi oleh pengisi sendiri"*, yang menggambarkan pasangan pengisi–pemvalidasi; *usulan:* *"Dirilis oleh pemvalidasi sendiri"* | `NON_BLOCKING_STANDARD` — kelimanya dapat ditetapkan desain dan disetujui lewat kontrak. Butir 5 **tercetak pada dokumen klinis**, sehingga persetujuan kontraknya wajib menyebut bunyinya kata per kata. **✅ Ditutup 2026-09-25:** kelimanya ditetapkan `02-backend-architecture.md` 20.10 butir 1-5 dan **disetujui** Yoga Aji Pratama bersama kontrak `EPIC-LAB-15` — butir 5 kata per kata | Desain `S4` + persetujuan kontrak |
| `ARCH-GAP-LAB-09` | **Pembatalan pemeriksaan sesudah rilis.** Arsitektur **menolaknya** — dan penolakan ini **diturunkan**, bukan dipilih: `LAB-DEC-138` menyatakan hasil yang sudah dirilis *"tetap hanya lewat koreksi"*, `LAB-DEC-007` menetapkan siapa yang mengoreksi, dan `LAB-DEC-063` sudah menolak pembatalan **pesanan** pada `InProcess` dan `Completed`. **Yang tetap terbuka dan wajib terlihat:** `LAB-DEC-049` membiarkan pembatalan **pemeriksaan** terbuka *"sesudah kelayakan ditetapkan"* tanpa menyebut batas atasnya — ditulis sebelum rilis dirancang. Contoh yang belum terjawab: pemeriksaan salah pasien yang **sudah dirilis** — ditarik sebagai koreksi `S6`, atau dibatalkan? *(Revision 8 semula merujuk `DEC-LAB-014`; rujukan itu keliru — `DEC-LAB-014` hanya memuat tiga pertanyaan klinis tentang koreksi)* | Tidak memblokir `S4`: arah yang aman sudah berlaku. Wajib dijawab **sebelum `S6` dirancang** — dicatat sebagai **`DEC-LAB-019`** (decisions rev 74) | Yoga Aji Pratama, lewat `grill-me` saat `S6` dibuka |

### A5.14 Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY` untuk `S4` — desain saja.** Ownership jelas (`BC-LAB` pemilik
fakta, `BC-HR` dan `BC-MRC` dibaca dan diberi), lifecycle dan invariant terwakili, dampak billing
nol, dampak keselamatan eksplisit, dan lima usulan yang tersisa (`ARCH-GAP-LAB-08`) tidak memblokir
bentuk.

**Pemakaian nyata tetap tertahan:** `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`,
`LAB-COORD-016`. Desain dan roadmap `S4` **wajib** membawanya ke Definition of Done dan menandai
task rilisnya `BLOCKED`.

### A5.15 Handoff

**Ke `design-business-module`:**

| Field | Nilai |
|---|---|
| Slice | `S4` validasi dan rilis hasil Patologi Klinik |
| Kesiapan | `DOMAIN_ARCHITECTURE_READY` — desain saja |
| Revision arsitektur | `LAB-DA-001` rev 8 |
| Konsep | `LAB-DC-051`..`LAB-DC-058`, `LAB-EV-01`..`LAB-EV-03` |
| Invariant | `INV-41`..`INV-51` |
| Usulan yang wajib ditandai usulan | `ARCH-GAP-LAB-08` kelima butirnya — butir 5 (bunyi penanda) disetujui kata per kata |
| Yang **wajib ditolak** tanpa diputuskan ulang | Pembatalan pemeriksaan sesudah rilis — `ARCH-GAP-LAB-09` |
| Yang **tidak boleh** muncul | Status hasil baru; analis sebagai pemvalidasi; daftar penunjukan milik Laboratorium; jalur pintas saat data kewenangan kosong; formulir pelaporan kritis (`S5`); koreksi sesudah rilis (`S6`) |
| Yang **wajib** ikut direncanakan | Data induk `LAB-DC-054` dan `LAB-DC-058` **beserta pengisiannya** — tanpa isi, *Kembalikan ke analis* dan jalur pengecualian malam hari tidak dapat dipakai sama sekali |

---

## A6. Revision 9 — Slice `S4d-1` validasi dan rilis hasil Mikrobiologi (2026-09-25)

**A6 memperluas A5, bukan menggantikannya.** Seluruh konsep, invariant, lifecycle, integrasi,
dan batas `BC-HR`/`BC-MRC` pada A5 berlaku **apa adanya** bagi Mikrobiologi; yang ditulis di bawah
hanya yang **berbeda** atau **bertambah**. Menyalin ulang A5 akan membuat dua salinan yang pasti
suatu hari berselisih.

### A6.1 Identitas dan gerbang masuk

| Butir | Isi |
|---|---|
| Scope | **`S4d-1`** — validasi dan rilis hasil **Mikrobiologi** yang **tidak** berkualifikasi `Sementara`. `S4d-2` (hasil `Sementara`) dan `S4e` **tidak** dirancang |
| Kesiapan requirement | `LAB-RCG-001-r9` bagian 0D: **`READY_FOR_DOMAIN_DESIGN`**, dinyatakan **independen** dari `S4d-2` |
| Penahan pemakaian yang dibawa serta | `DEC-LAB-017` sejenis (pemilik `DR-LAB-002`); dua pemegang validasi Mikrobiologi tercatat (`LAB-DEC-152`); `LAB-COORD-016`; `LAB-OPEN-044` |
| Bukti | Decisions rev 76; `LAB-EVD-010`; capability map rev 5; `LAB-DA-001` rev 8 bagian A5; rancangan `S4` yang sudah disetujui (`02-backend-architecture.md` bagian 20) |
| Snapshot kode | BE `ddeb5ed8`, FE `72607a087` |
| Baseline rujukan | **Tidak dipakai** |

**Decision ID yang mengikat, di luar yang mengikat A5:** `LAB-DEC-085` hasil Mikrobiologi **per
pemeriksaan**, `LAB-DEC-097` Final bukan rilis, `LAB-DEC-106` konsultasi adalah fakta, `LAB-DEC-114`
kualifikasi dicetak, `LAB-DEC-120` baris cetak *Petugas Otorisasi* dan *Validasi oleh*,
`LAB-DEC-146` pengisi hasil Mikrobiologi adalah analis, `LAB-DEC-152` pemegang dan larangan lintas
disiplin, `LAB-DEC-153` perilis tidak wajib dokter.

### A6.2 Ubiquitous language — istilah yang ditambahkan

| Istilah | Arti |
|---|---|
| **Hasil Mikrobiologi berstruktur** | Satu kesatuan: status temuan, isolat beserta organismenya, dan antibiogram. **Divalidasi dan dirilis sebagai satu hasil**, bukan per isolat |
| **Kualifikasi hasil** | Nilai yang dicetak pada baris *HASIL YANG DIPEROLEH* — `Definitif` atau `Sementara` (`LAB-DEC-114`). **Boleh kosong** (`LAB-VAL-v1` `r10` 12.2) |
| **Konsultan** | Pihak yang diajak berkonsultasi atas hasil (`LAB-DEC-106`, `LAB-DEC-119`). **Bukan** pemvalidasi dan **bukan** perilis |

**Perbedaan makna yang wajib dijaga:** *Definitif* adalah **kualifikasi isi hasil**; *Tervalidasi*
dan *Dirilis* adalah **pengesahan**. Hasil `Definitif` yang belum divalidasi **belum sah**; hasil
tervalidasi tidak otomatis `Definitif`.

### A6.3 Peta bounded context — perubahan

**Nol.** `BC-LAB`, `BC-HR`, dan `BC-MRC` dengan hubungan A5.3. Yang bertambah hanya **dua kode**
Mikrobiologi di katalog Human Resource — bagian dari keenam kode `LAB-DC-056` yang sudah dirancang.

### A6.4 Katalog konsep domain

**Nol konsep baru.** `LAB-DC-051`..`LAB-DC-058` dan `LAB-EV-01`..`LAB-EV-03` dipakai apa adanya:

| Konsep | Yang berbeda pada Mikrobiologi |
|---|---|
| `LAB-DC-051` Validasi, `LAB-DC-052` Rilis | Melekat pada `LAB-DC-002` Pemeriksaan Mikrobiologi — per pemeriksaan (`LAB-DEC-085`) — dan mencakup **seluruh** hasil berstruktur |
| `LAB-DC-055` Penunjukan, `LAB-DC-056` Kode | Dua kode **Mikrobiologi**; kode Patologi Klinik **tidak berlaku** (`LAB-DEC-152`) |
| `LAB-DC-057` Dokumen rekam medis | Satu dokumen per pemeriksaan Mikrobiologi yang dirilis |
| `LAB-DC-054`, `LAB-DC-058` Daftar alasan | **Daftar yang sama** — bukan daftar per disiplin. *Sampel tertukar* adalah kejadian yang sama di mana pun |

**Yang sengaja tidak dijadikan konsep:**

| Yang ditolak | Alasan |
|---|---|
| Validasi per isolat atau per baris antibiogram | Hasil Mikrobiologi melekat **per pemeriksaan** (`LAB-DEC-085`); dokter memvalidasi satu kesimpulan biakan, bukan potongannya. Validasi per isolat membuat hasil *setengah sah* |
| Status `Definitif` sebagai tahap lifecycle | `LAB-DEC-114` — ia nilai isi hasil, bukan status (`LAB-DEC-080`) |
| Persetujuan konsultan sebagai syarat validasi | `LAB-DEC-106` — konsultasi adalah fakta opsional, bukan izin |
| Menyamakan rilis dengan persetujuan pengiriman ke pasien | Pengiriman ke pasien bergerbang sendiri (`LAB-DEC-139`) dan persetujuan Profesornya masih `LAB-OPEN-029`. **Rilis membuat hasil terbaca dokter pemesan dan menjadi dokumen rekam medis; ia tidak mengirim apa pun kepada pasien** |

### A6.5 Model aggregate

**Nol aggregate baru.** Mikrobiologi tetap di `AGG-LAB-01` lewat `LAB-DC-002`, seperti A5.5.
`ARCH-GAP-LAB-05` dan `ARCH-GAP-LAB-06` **tidak memburuk**: validasi dan rilis tetap menyentuh
**satu** pemeriksaan per tindakan, dan isolat serta antibiogram sudah menjadi bagian aggregate yang
sama sejak `S4b`.

**Invariant yang ditambahkan:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-52` | Hasil Mikrobiologi berkualifikasi **`Sementara`** **tidak dapat** divalidasi maupun dirilis — sampai `DEC-LAB-020` dijawab | `LAB-RCG-001-r9` bagian 0D.5 |
| `INV-53` | Validasi dan rilis Mikrobiologi mencakup **seluruh** hasil berstruktur sebagai satu kesatuan; isolat dan antibiogram hasil yang tervalidasi atau dirilis **tidak berubah** kecuali lewat *Kembalikan* sebelum rilis atau koreksi `S6` sesudahnya | `LAB-DEC-085`; `VAL-120` sudah menolak simpan hasil selama `FinalizedAt` terisi |

**Invariant A5 yang berlaku dengan pembacaan per disiplin:** `INV-45` — pemvalidasi wajib dokter
berkewenangan laboratorium **dan** memegang kode validasi **Mikrobiologi**; perilis memegang kode
rilis **Mikrobiologi**. `INV-42` — pengisi hasil Mikrobiologi adalah analis (`LAB-DEC-146`),
sehingga seperti Patologi Klinik, jalur pengecualian yang nyata ada pada **rilis** oleh pemvalidasi
yang sama (`INV-43`).

### A6.6 Model relasi

**Nol relasi baru.** A5.6 berlaku. Satu penegasan: isolat dan baris kepekaan antibiotik **tidak**
punya relasi sendiri ke validasi maupun rilis — mereka disahkan **lewat** pemeriksaannya.

### A6.7 Model lifecycle

A5.7 berlaku, dengan **satu syarat tambahan** pada kedua perpindahan:

| Dari | Tindakan | Ke | Syarat tambahan bagi Mikrobiologi |
|---|---|---|---|
| Final | Validasi | Tervalidasi | Kualifikasi **bukan** `Sementara` (`INV-52`) |
| Tervalidasi | Rilis | Dirilis | Sama |

**Hasil `Sementara` yang ingin disahkan** hari ini hanya punya satu jalan: analis membuka kembali
(Reopen), mengubah kualifikasinya menjadi `Definitif` **bila memang sudah definitif**, lalu Final
ulang. **Tidak ada jalan untuk mengesahkan hasil yang benar-benar masih sementara** — itulah
`DEC-LAB-020`.

**Contoh:**

> Senin hasil kultur urin Final dengan kualifikasi `Sementara`: *tumbuh kuman batang Gram negatif,
> identifikasi menyusul*. dr. Contoh menekan Validasi dan ditolak: *"Hasil Mikrobiologi sementara
> belum dapat divalidasi."* Rabu identifikasinya selesai — *Escherichia coli*, lengkap dengan
> antibiogram. Analis membuka kembali, mengisi isolat dan antibiogram, mengubah kualifikasi menjadi
> `Definitif`, dan Final ulang. dr. Contoh memvalidasi; perilis merilis. **Yang tercatat di rekam
> medis hanya hasil Rabu** — hasil Senin tidak pernah keluar dari laboratorium lewat sistem.

**Label order** (`resultProgress`, A5.7): order Mikrobiologi *Selesai* bila seluruh pemeriksaan
tidak batalnya dirilis. Pemeriksaan yang masih `Sementara` **menahan** label itu — dan memang
seharusnya, sebab hasilnya belum akhir.

### A6.8 Tanggung jawab authorization

| Tanggung jawab | Pemegang | Status bukti |
|---|---|---|
| Mengisi hasil, Final, Reopen | Jabatan analis (`LAB-DEC-146`) | `CONFIRMED` — sudah berjalan sejak `S4b` |
| Memvalidasi | Dokter berkewenangan laboratorium yang ditunjuk validasi **Mikrobiologi**: dr. Nabila Rahmawati, Sp.MK, dan pemegang tambahan yang ditetapkan | `CONFIRMED` — `LAB-DEC-152` |
| Merilis | Pemegang kode rilis Mikrobiologi — dokter **atau** pejabat non-dokter | `CONFIRMED` bentuknya — `LAB-DEC-153`; jabatan calonnya `LAB-OPEN-044` |
| Mencatat konsultasi | Pemegang izin hasil (`LAB-DEC-106`) | `CONFIRMED` — **bukan** tindakan pengesahan |
| Melintas disiplin | **Dilarang** — pemegang Patologi Klinik tidak memvalidasi Mikrobiologi | `CONFIRMED` — `LAB-DEC-152`, `AC-241` |

### A6.9 Model audit dan histori

A5.9 berlaku apa adanya. **Satu tambahan:** kualifikasi hasil pada saat validasi dan rilis
**sudah** terkunci bersama hasil (`VAL-120`), sehingga tidak dibutuhkan salinan kualifikasi
tersendiri pada catatan validasi.

### A6.10 Model integrasi

| Batas | Yang berbeda dari A5.10 |
|---|---|
| Penunjukan kewenangan (`BC-HR`) | Kode **Mikrobiologi**; aturan menilai baris dan fail-closed **sama persis** |
| Dokumen hasil (`BC-MRC`) | Satu dokumen per **pemeriksaan** Mikrobiologi yang dirilis; atomik dengan rilisnya. **Tepat satu** — rilis kedua atas pemeriksaan yang sama tidak ada pada `S4d-1` |
| Cetakan Mikrobiologi | Ruas *Petugas Otorisasi* dan *Validasi oleh* yang hari ini **sengaja kosong** (`LAB-DEC-120`; `LabMicrobiologyResultService.cs:372-381`) **terisi** dari fakta rilis dan validasi. `IsReleased` pada respons Mikrobiologi kini bernilai sebenarnya — **`Extend`**, bukan konsep baru |

### A6.11 Dampak billing

**Tidak ada dampak charge yang diketahui** — sama dengan A5.11.

### A6.12 Dampak keselamatan klinis

**Relevan terhadap keselamatan.**

| Risiko | Penjaga dalam arsitektur | Yang tetap di luar arsitektur |
|---|---|---|
| Hasil sementara disahkan seolah akhir | `INV-52` — ditolak, fail-closed | Apakah hasil sementara **boleh** keluar — `DEC-LAB-020` |
| Hasil **tanpa** kualifikasi disahkan padahal sebenarnya sementara | Pemvalidasi dokter Sp.MK membaca hasilnya sebelum mengesahkan | **`ARCH-GAP-LAB-10`** |
| Pemvalidasi tanpa kompetensi Mikrobiologi | `INV-45` per disiplin; `AC-241` | Isi daftar pemegang tambahan — data |
| Hasil kritis Mikrobiologi dirilis tanpa pelaporan tercatat | — | **`DEC-LAB-017` sejenis.** Penanda kritis Mikrobiologi **sudah** menyala di layar hari ini (`CAP-P14-12`), sedangkan formulir pelaporannya `S5` |
| Antibiotik empiris tanpa dasar tertulis karena hasil hari pertama tidak dapat keluar | — | `DEC-LAB-020`. **Nol kemunduran** — hari ini tidak satu pun hasil Mikrobiologi dapat dirilis |

### A6.13 Gap arsitektur

| ID | Gap | Dampak | Pemilik |
|---|---|---|---|
| `ARCH-GAP-LAB-10` | **Hasil Mikrobiologi tanpa kualifikasi** — `resultQualifier` kosong — diperlakukan **bukan `Sementara`**, sehingga **dapat** divalidasi dan dirilis. Mengikuti batas gerbang 0D (*"tidak berkualifikasi `Sementara`"*) dan alasan `r10` 12.2 membiarkan ruas itu opsional: ada bentuk cetak yang belum pernah terlihat, yang mungkin memang tidak memuat baris kualifikasi. **Tafsiran sebaliknya** — kosong dianggap sementara — akan membuat setiap hasil tanpa kualifikasi tidak pernah dapat dirilis | `PROPOSED`, `NON_BLOCKING_STANDARD` — bila dibalik, yang berubah satu aturan (`bukan Sementara` → `Definitif`), bukan bentuk data | **Disarankan ikut ditanyakan bersama `DEC-LAB-020`** kepada `DR-LAB-002` |
| `ARCH-GAP-LAB-05`, `ARCH-GAP-LAB-06` | Aggregate pesanan membesar | Tidak memburuk (A6.5) | Tim pembangun sistem |

### A6.14 Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY` untuk `S4d-1` — desain saja.** Nol konsep, nol aggregate, nol
relasi baru; dua invariant bertambah (`INV-52`, `INV-53`); satu usulan yang wajib ditandai usulan
(`ARCH-GAP-LAB-10`). Dampak billing nol; dampak keselamatan eksplisit.

**Tetap tertahan:** `S4d-2` (`DEC-LAB-020`) dan `S4e` (`DEC-LAB-021`) — **tidak** dirancang.
**Pemakaian nyata `S4d-1`** tertahan `DEC-LAB-017` sejenis, dua pemegang validasi Mikrobiologi
tercatat, `LAB-COORD-016`, dan `LAB-OPEN-044`.

### A6.15 Handoff

**Ke `design-business-module`:**

| Field | Nilai |
|---|---|
| Slice | `S4d-1` validasi dan rilis hasil Mikrobiologi yang bukan `Sementara` |
| Kesiapan | `DOMAIN_ARCHITECTURE_READY` — desain saja |
| Revision arsitektur | `LAB-DA-001` rev 9 |
| Cara merancang | **Perluas bagian 20 `02-backend-architecture.md` dan kontrak `r34`/`r12`/rev 11/`r5`/`INT r4`**, jangan merancang ulang. Yang berubah pada `r34`: `VAL-126` tidak lagi menolak Mikrobiologi; antrean validasi menerima disiplin Mikrobiologi; `resultProgress` berlaku juga pada daftar Pemeriksaan Mikrobiologi |
| Konsep | `LAB-DC-051`..`LAB-DC-058`, `LAB-EV-01`..`LAB-EV-03` — nol baru |
| Invariant | `INV-41`..`INV-53` |
| Usulan yang wajib ditandai usulan | `ARCH-GAP-LAB-10` |
| Yang **tidak boleh** muncul | Validasi atau rilis hasil `Sementara`; rilis kedua atas pemeriksaan yang sama; validasi per isolat; konsultasi sebagai syarat validasi; rilis yang mengirim ke pasien; pemvalidasi lintas disiplin; daftar alasan terpisah per disiplin |
| Yang **wajib** ikut direncanakan | Dua kode Mikrobiologi di katalog Human Resource (`LAB-COORD-016`) dan ruas cetak *Petugas Otorisasi*/*Validasi oleh* Mikrobiologi |

---

## B. Ubiquitous Language

Satu istilah, satu makna. Bila satu kata dipakai dua arti oleh bagian berbeda, perbedaannya
dipertahankan dan tidak disatukan.

| Istilah | Makna bisnis tunggal |
|---|---|
| **Pesanan Laboratorium** | Permintaan resmi dokter agar seorang pasien menjalani satu atau lebih pemeriksaan laboratorium dalam satu kunjungan |
| **Pemeriksaan Terpesan** | Satu jenis pemeriksaan yang diminta di dalam sebuah pesanan, misalnya Hemoglobin. Inilah satuan yang ditagihkan dan yang kelak punya hasil |
| **Wadah Fisik** | Satu wadah nyata berisi bahan dari tubuh pasien, misalnya satu tabung darah EDTA. Satu wadah punya satu barcode dan dapat melayani beberapa pemeriksaan |
| **Tingkat Kesegeraan** | Penanda seberapa cepat pesanan harus diselesaikan: biasa atau cito |
| **Cito** | Pesanan yang harus didahulukan, ditandai dokter pemesan, dan punya batas waktu penyelesaian yang terukur |
| **Diterima** (*Received*) | Sampel sudah sampai secara fisik di laboratorium. **Belum** berarti boleh dikerjakan |
| **Dinyatakan Layak** (*Accepted*) | Sampel sudah diperiksa kelayakannya dan dinyatakan boleh dikerjakan. **Inilah** titik pemeriksaan sah ditagihkan |
| **Ditolak** | Sampel dinyatakan tidak layak diperiksa, dengan alasan dari daftar terkendali |
| **Ambil Ulang** | Pengambilan sampel pengganti karena sampel sebelumnya tidak dapat dipakai. Menciptakan identitas sampel baru dan menyimpan tautan ke sampel yang digantikan |
| **Batas Nilai** | Rentang atau daftar pilihan yang menyatakan sebuah hasil normal, di luar rujukan, atau kritis |
| **Batas Normal** | Rentang yang dianggap wajar. Dapat berubah mengikuti metode dan alat |
| **Batas Kritis** | Nilai yang menandakan pasien berada dalam bahaya. Perubahannya memerlukan persetujuan klinis |
| **Batas Waktu Cito** | Lama maksimum dari sampel dinyatakan layak sampai hasil dirilis, untuk pesanan bertanda cito |
| **Daftar Kerja** | Susunan pekerjaan laboratorium yang belum selesai, diurutkan dengan cito di atas |
| **Fakta Milestone Klinis** | Pemberitahuan satu arah dari Laboratorium ke Billing bahwa sebuah kejadian operasional telah terjadi. Bukan tagihan, bukan angka uang |
| **Kelayakan Tagih** | Pernyataan bahwa sebuah pemeriksaan sudah sah untuk ditagihkan. Keputusan uangnya tetap milik Billing |

### Perbedaan makna yang wajib dipertahankan

| Pasangan | Kenapa tidak boleh disatukan |
|---|---|
| **Diterima** vs **Dinyatakan Layak** | Dikunci `LAB-INH-008`. Sampel bisa sampai di lab lalu ditolak. Menyatukannya membuat pemeriksaan tertagih padahal tidak pernah dikerjakan |
| **Pemeriksaan Terpesan** vs **Wadah Fisik** | Dikunci `LAB-DEC-024`. Satu tabung dapat melayani beberapa pemeriksaan. Menyatukannya membuat penolakan sebagian atas satu tabung tampak sah, padahal mustahil secara fisik |
| **Batas Normal** vs **Batas Kritis** | Dikunci `LAB-DEC-023`. Yang pertama urusan teknis laboratorium, yang kedua penilaian klinis |
| **Fakta** vs **Tagihan** | Dikunci `LAB-INH-010`. Laboratorium mengirim kejadian; Billing yang memutuskan akibat uangnya |

---

## C. Peta Bounded Context

| ID | Bounded context | Tanggung jawab | Konsep yang dimiliki |
|---|---|---|---|
| `BC-LAB` | Operasional Laboratorium | Perjalanan pesanan dan sampel, kelayakan periksa, batas nilai, alasan penolakan, daftar kerja, riwayat perpindahan | `LAB-DC-001` sampai `LAB-DC-007`, `LAB-DC-011` sampai `LAB-DC-013` |
| `BC-REG` | Registrasi dan Kunjungan | Identitas kunjungan pasien beserta jenis unitnya | `LAB-DC-020` Kunjungan Pasien |
| `BC-MD` | Data Induk Layanan | Katalog jenis tindakan dan pemeriksaan, tarif | `LAB-DC-021` Jenis Pemeriksaan, `LAB-DC-022` Tarif |
| `BC-BIL` | Billing dan Kasir | Seluruh akibat finansial | Menerima `LAB-DC-008` |
| `BC-PLAT` | Platform dan Keamanan | Identitas pengguna, kewenangan per aksi | Menyediakan pelaku dan pemeriksaan kewenangan |

### Hubungan antarcontext

| Dari | Ke | Sifat hubungan | Arah kebenaran |
|---|---|---|---|
| `BC-LAB` | `BC-REG` | Hilir — Laboratorium menempel pada kunjungan yang sudah ada | `BC-REG` pemilik. `BC-LAB` hanya merujuk |
| `BC-LAB` | `BC-MD` | Hilir — Laboratorium memakai katalog pemeriksaan dan tarif | `BC-MD` pemilik. `BC-LAB` merujuk dan menyimpan salinan sesaat |
| `BC-LAB` | `BC-BIL` | Hulu — Laboratorium menerbitkan fakta, Billing menafsirkannya | `BC-LAB` pemilik fakta operasional; `BC-BIL` pemilik akibat finansial |
| `BC-LAB` | `BC-PLAT` | Hilir — Laboratorium memakai identitas dan kewenangan | `BC-PLAT` pemilik |

**Aturan yang tidak boleh dilanggar.** `BC-LAB` **tidak boleh** membuat salinan pasien, dokter,
kunjungan, jenis pemeriksaan, atau tarif sebagai sumber kebenaran tandingan. Yang boleh disimpan
hanyalah **rujukan** dan **salinan sesaat** untuk keperluan penelusuran harga saat kejadian —
pola yang sudah dipakai dan terbukti pada `TrxLabSpecimen.TariffCodeSnapshot@c87d9c0`.

---

## D. Katalog Konsep Domain

| ID | Nama bisnis | Klasifikasi | Pemilik | Ownership | Identitas | Bukti |
|---|---|---|---|---|---|---|
| `LAB-DC-001` | Pesanan Laboratorium | `AGGREGATE_ROOT` | `BC-LAB` | `Extend` | Identitas sendiri, terikat pada satu kunjungan | `LabOrder.cs@c87d9c0` |
| `LAB-DC-002` | Pemeriksaan Terpesan | `ENTITY` | `BC-LAB` | `New` — dipisahkan oleh `LAB-DEC-024` | Identitas sendiri di dalam pesanan; ditopang tepat satu wadah | `LAB-DEC-024`; keadaan lama pada `TrxLabSpecimen.ProcedureId@c87d9c0` |
| `LAB-DC-003` | Wadah Fisik | `ENTITY` | `BC-LAB` | `Extend` — dipersempit oleh `LAB-DEC-024` | Satu barcode per wadah nyata, bukan per pemeriksaan | `LAB-DEC-024`; keadaan lama pada `TrxLabSpecimen.SpecimenBarcode@c87d9c0` |
| `LAB-DC-004` | Tingkat Kesegeraan | `VALUE_OBJECT` | `BC-LAB` | `New` | Melekat pada pesanan, tidak berdiri sendiri | `LAB-DEC-013` |
| `LAB-DC-005` | Alasan Penolakan Sampel | `REFERENCE_DATA` | `BC-LAB` | `Extend` | Kode alasan yang unik | `MstLabRejectionReason.cs@c87d9c0` |
| `LAB-DC-006` | Batas Nilai Pemeriksaan | `ENTITY` | `BC-LAB` | `New` | Kombinasi jenis pemeriksaan, jenis kelamin, dan kelompok umur | `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021` |
| `LAB-DC-007` | Riwayat Perpindahan Laboratorium | `ENTITY` | `BC-LAB` | `Existing` | Identitas sendiri, tidak pernah diubah | `TrxLabTransitionHistory.cs@c87d9c0` |
| `LAB-DC-008` | Fakta Milestone Klinis | `DOMAIN_EVENT` | `BC-LAB` menerbitkan, `BC-BIL` mengonsumsi | `Existing` | Identitas fakta beserta kunci idempotensi | `ClinicalMilestoneFactProducer.cs@c87d9c0` |
| `LAB-DC-011` | Daftar Kerja Laboratorium | `ADAPTER/VIEW` | `BC-LAB` | `Adapter/View` | Tidak punya identitas; diturunkan dari pesanan dan sampel | `LAB-DEC-013` |
| `LAB-DC-012` | Pengajuan Perubahan Batas Kritis | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri | `LAB-DEC-023` |
| `LAB-DC-013` | Riwayat Perubahan Batas Nilai | `ENTITY` | `BC-LAB` | `New` | Identitas sendiri, tidak pernah diubah | `LAB-DEC-023` |
| `LAB-DC-020` | Kunjungan Pasien | `ENTITY` | `BC-REG` | `Existing` — dirujuk saja | Milik `BC-REG` | `TrxPatientEncounter.cs@c87d9c0` |
| `LAB-DC-021` | Jenis Pemeriksaan | `REFERENCE_DATA` | `BC-MD` | `Existing` — dirujuk saja | Milik `BC-MD` | `MstProcedure.IsLaboratory@c87d9c0` |
| `LAB-DC-022` | Tarif Pemeriksaan | `REFERENCE_DATA` | `BC-MD` | `Existing` — dirujuk dan disalin sesaat | Milik `BC-MD` | `ResolveTariffAsync@c87d9c0` |

### Yang sengaja **tidak** dijadikan konsep tersendiri

| Yang ditolak | Alasan |
|---|---|
| Satu konsep per status, misalnya "Sampel Ditolak" sebagai konsep sendiri | Status adalah keadaan sebuah konsep, bukan konsep baru. Melanggar aturan arsitektur |
| Konsep "Daftar Kerja" sebagai data tersimpan | Daftar kerja seluruhnya dapat diturunkan dari pesanan dan sampel yang sudah ada. Menyimpannya menciptakan sumber kebenaran kedua yang bisa tidak sinkron |
| Konsep "Pasien Laboratorium" atau "Dokter Laboratorium" | Duplikasi data induk bersama. Dilarang tegas |
| Konsep "Nilai Kritis" sebagai entity | Nilai kritis adalah **penilaian** atas sebuah hasil terhadap batas nilai, bukan benda tersendiri. Perancangannya menunggu slice `S5` yang masih terblokir |

---

## E. Model Aggregate

### `AGG-LAB-01` — Pesanan Laboratorium

| Field | Isi |
|---|---|
| Root | `LAB-DC-001` Pesanan Laboratorium |
| Batas | Pesanan beserta seluruh sampel dan pemeriksaan terpesan di dalamnya |
| Alasan batas | Kelayakan tagih, pembatalan, dan penahanan hanya konsisten bila diputuskan atas satu pesanan utuh. Membatalkan pesanan wajib membatalkan sampel di dalamnya secara serentak |

**Invariant yang dilindungi:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-01` | Sebuah pesanan wajib terikat pada tepat satu kunjungan pasien yang sudah ada | `LabOrder.EncounterId@c87d9c0` |
| `INV-02` | Sampel tidak dapat dinyatakan layak tanpa melewati penerimaan lebih dulu | Diuji `#PenetapanLayakTanpaMelaluiPenerimaan_Ditolak@c87d9c0` |
| `INV-03` | Pesanan yang sudah dibatalkan tidak dapat menerima sampel baru | Diuji `#PesananYangSudahDibatalkan_TidakDapatMenerimaSampelBaru@c87d9c0` |
| `INV-04` | Jenis pemeriksaan bukan laboratorium tidak dapat dipakai sebagai komponen | Diuji `#ProcedureBukanLaboratorium_TidakDapatDipakaiSebagaiKomponen@c87d9c0` |
| `INV-05` | Dua petugas yang menyatakan layak sampel yang sama secara bersamaan, hanya satu yang berhasil | Diuji `#DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak@c87d9c0` |
| `INV-06` | Penetapan layak yang diulang tidak menggandakan kelayakan tagih | Diuji `#PenetapanLayakDiulang_TidakMenggandakanTagihan@c87d9c0` |
| `INV-07` | Barcode sampel unik dan tidak memuat identitas pasien | Diuji `#BarcodeSampel_UnikDanTidakMemuatIdentitasPasien@c87d9c0` |
| `INV-08` | Sampel yang ditolak atau diambil ulang tetap terlihat dan tertaut ke sampel penggantinya | `LAB-INH-005`; diuji `#PengambilanUlang_MempertahankanSampelDitolakDanTautanSebabnya@c87d9c0` |
| `INV-09` | Tidak ada kolom maupun tindakan finansial di dalam aggregate ini | `LAB-INH-012`; diuji `#ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun@c87d9c0` |

**Tindakan bisnis yang dikenali aggregate ini:**

| Tindakan | Wewenang | Menerbitkan fakta? |
|---|---|---|
| Membuat pesanan | Dokter pemesan | Tidak |
| Menandai kesegeraan pesanan | Dokter pemesan | Tidak |
| Merencanakan sampel | Petugas berwenang merencanakan | Tidak |
| Mencatat pengambilan sampel | Petugas berwenang mengambil | Tidak |
| Mencatat sampel tiba di laboratorium | Petugas berwenang menerima | Tidak |
| Menyatakan sampel layak | Petugas berwenang menetapkan kelayakan | **Ya — kelayakan tagih** |
| Menolak sampel | Petugas berwenang menetapkan kelayakan | Tidak |
| Meminta ambil ulang | Petugas berwenang menetapkan kelayakan | Tidak |
| Menahan dan melanjutkan | Petugas berwenang menahan | Tidak |
| Membatalkan sampel atau pesanan | Petugas berwenang membatalkan | **Ya — pembatalan klinis, bila sudah pernah layak** |

### `AGG-LAB-02` — Batas Nilai Pemeriksaan

| Field | Isi |
|---|---|
| Root | `LAB-DC-006` Batas Nilai Pemeriksaan |
| Batas | Satu baris batas nilai beserta riwayat perubahannya dan pengajuan perubahan yang menyertainya |
| Alasan batas | Persetujuan klinis atas batas kritis hanya bermakna bila diputuskan atas satu baris batas yang utuh |

**Invariant yang dilindungi:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-10` | Satu jenis pemeriksaan boleh punya beberapa baris batas, dibedakan jenis kelamin dan kelompok umur; kombinasi ketiganya tidak boleh berulang | `LAB-DEC-018` |
| `INV-11` | Sebuah pemeriksaan berbentuk angka wajib punya satuan; sebuah pemeriksaan berbentuk pilihan wajib punya daftar pilihan yang sah | `LAB-DEC-021` |
| `INV-12` | Sebuah baris batas nilai memiliki tepat satu bentuk hasil, angka atau pilihan, tidak keduanya | `LAB-DEC-021` |
| `INV-13` | Perubahan batas kritis tidak berlaku sebelum disetujui pihak klinis | `LAB-DEC-023` |
| `INV-14` | Setiap perubahan batas nilai menghasilkan satu baris riwayat yang tidak dapat diubah | `LAB-DEC-023` |
| `INV-15` | Batas nilai wajib menunjuk ke jenis pemeriksaan yang ada di katalog `BC-MD`; katalog itu sendiri tidak boleh diubah dari `BC-LAB` | `LAB-DEC-018` |

### `AGG-LAB-03` — Alasan Penolakan Sampel

| Field | Isi |
|---|---|
| Root | `LAB-DC-005` Alasan Penolakan Sampel |
| Batas | Satu alasan penolakan |
| Alasan batas | Data rujukan sederhana; tidak melindungi invariant lintas baris |

**Invariant yang dilindungi:**

| ID | Invariant | Bukti |
|---|---|---|
| `INV-16` | Kode alasan unik | `MstLabRejectionReason.ReasonCode@c87d9c0` |
| `INV-17` | Alasan yang menuntut catatan tidak dapat dipakai tanpa catatan | Diuji `#AlasanPenolakanOther_WajibDisertaiCatatan@c87d9c0` |
| `INV-18` | Alasan yang tidak dikenal ditolak | Diuji `#AlasanPenolakanTidakDikenal_Ditolak@c87d9c0` |
| `INV-19` | Penanda kesalahan internal dan penanda wajib catatan tidak dapat diubah dari dalam Laboratorium | `LAB-DEC-019` |

---

## F. Model Relasi

| Sumber | Tujuan | Makna bisnis | Kardinalitas | Arah ownership | Wajib | Ketergantungan lifecycle |
|---|---|---|---|---|---|---|
| Pesanan Laboratorium | Kunjungan Pasien | Pesanan dibuat dalam konteks satu kunjungan | Banyak ke satu | `BC-REG` pemilik | Wajib | Pesanan tidak berarti tanpa kunjungan |
| Pesanan Laboratorium | Wadah Fisik | Satu pesanan dapat memuat beberapa sampel | Satu ke banyak | `BC-LAB` pemilik | Opsional saat dibuat | Sampel mati bersama pesanan yang dibatalkan |
| Pesanan Laboratorium | Pemeriksaan Terpesan | Satu pesanan memuat satu atau beberapa pemeriksaan | Satu ke banyak | `BC-LAB` pemilik | Wajib minimal satu | Pemeriksaan mati bersama pesanan yang dibatalkan |
| Wadah Fisik | Pemeriksaan Terpesan | Satu wadah menopang satu atau beberapa pemeriksaan | Satu ke banyak | `BC-LAB` pemilik | Wajib setelah wadah direncanakan | Penolakan wadah menggugurkan seluruh pemeriksaan yang ditopangnya |
| Pemeriksaan Terpesan | Jenis Pemeriksaan | Pemeriksaan merujuk jenis pemeriksaan di katalog | Banyak ke satu | `BC-MD` pemilik | Wajib | Tidak ada |
| Wadah Fisik | Wadah Fisik | Wadah pengganti menunjuk wadah yang digantikan | Satu ke satu, opsional | `BC-LAB` pemilik | Opsional | Sampel lama **tidak** dihapus |
| Wadah Fisik | Alasan Penolakan | Penolakan memakai alasan terkendali | Banyak ke satu | `BC-LAB` pemilik | Wajib saat ditolak | Alasan tidak boleh dihapus bila pernah dipakai |
| Riwayat Perpindahan | Pesanan, Wadah, dan Pemeriksaan | Setiap perpindahan penting tercatat | Banyak ke satu | `BC-LAB` pemilik | Wajib | Riwayat hidup lebih lama daripada keadaan terkini |
| Batas Nilai Pemeriksaan | Jenis Pemeriksaan | Batas berlaku untuk suatu pemeriksaan | Banyak ke satu | `BC-MD` pemilik | Wajib | Tidak ada |
| Riwayat Perubahan Batas | Batas Nilai Pemeriksaan | Setiap perubahan tercatat | Banyak ke satu | `BC-LAB` pemilik | Wajib | Riwayat hidup lebih lama |
| Fakta Milestone | Pemeriksaan Terpesan | Fakta menunjuk kejadian operasional asalnya | Banyak ke satu | `BC-LAB` pemilik fakta | Wajib | Fakta tidak dihapus |

---

## G. Model Lifecycle dan Status

### G.1 Perjalanan Pesanan Laboratorium

| Dari status | Tindakan | Ke status | Wewenang | Prasyarat | Kejadian audit |
|---|---|---|---|---|---|
| — | Membuat pesanan | `Requested` | Dokter pemesan | Kunjungan aktif ada | Ya |
| `Requested` | Sampel pertama dinyatakan layak | `Accepted` | Turunan otomatis | Ada sampel yang layak | Ya |
| `Accepted` | Mulai dikerjakan | `InProcess` | Petugas berwenang memproses | — | Ya |
| `InProcess` | Menyelesaikan | `Completed` | Petugas berwenang memproses | — | Ya |
| Mana pun kecuali terminal | Menahan | `OnHold` | Petugas berwenang menahan | Status sebelumnya disimpan | Ya |
| `OnHold` | Melanjutkan | Status sebelum ditahan | Petugas berwenang menahan | Status sebelumnya diketahui | Ya |
| Mana pun kecuali `Completed` | Membatalkan | `Cancelled` | Petugas berwenang membatalkan | — | Ya |

Status terminal: `Completed` dan `Cancelled`.

> **Catatan tentang `Draft` dan `CancelRequested`.** Keduanya ada pada bukti kode tetapi
> perlakuannya menjadi bagian slice `S1b` yang **terblokir** oleh `LAB-AMD-001`. Dokumen ini
> tidak merancangnya.

### G.2 Perjalanan Wadah Fisik

| Dari status | Tindakan | Ke status | Wewenang | Prasyarat | Kejadian audit |
|---|---|---|---|---|---|
| — | Merencanakan | `Planned` | Petugas berwenang merencanakan | Pesanan belum dibatalkan | Ya |
| `Planned` | Mencatat pengambilan | `Collected` | Petugas berwenang mengambil | — | Ya |
| `Collected` | Mencatat tiba di lab | `Received` | Petugas berwenang menerima | — | Ya |
| `Received` | Menyatakan layak | `Accepted` | Petugas berwenang menetapkan kelayakan | Wajib melewati `Received` | Ya, **dan menerbitkan fakta kelayakan tagih** |
| `Received` | Menolak | `Rejected` | Petugas berwenang menetapkan kelayakan | Alasan terkendali wajib | Ya |
| `Rejected` | Meminta ambil ulang | `RecollectionRequired` | Petugas berwenang menetapkan kelayakan | Sebab ambil ulang wajib | Ya |
| Mana pun kecuali terminal | Menahan | `OnHold` | Petugas berwenang menahan | Status sebelumnya disimpan | Ya |
| `OnHold` | Melanjutkan | Status sebelum ditahan | Petugas berwenang menahan | — | Ya |
| Mana pun kecuali terminal | Membatalkan | `Cancelled` | Petugas berwenang membatalkan | — | Ya, dan menerbitkan fakta pembatalan bila pernah layak |

Status terminal: `Accepted` yang berlanjut ke pemeriksaan, `Rejected`, dan `Cancelled`.

**Koreksi dan ambil ulang.** Ambil ulang **tidak** mengubah sampel lama. Ia menciptakan sampel
baru yang menunjuk sampel lama beserta sebabnya. Sebab ambil ulang menentukan akibat biayanya:

| Sebab | Akibat menurut `LAB-INH-011` |
|---|---|
| Kesalahan internal rumah sakit | **Tidak** menambah tanggungan pasien secara otomatis |
| Kondisi pasien atau sampel | Memerlukan alasan dan otorisasi sebelum tagihan baru dipertimbangkan |
| Sebab eksternal | Sama seperti di atas |

### G.3 Perjalanan Perubahan Batas Kritis

| Dari status | Tindakan | Ke status | Wewenang | Kejadian audit |
|---|---|---|---|---|
| — | Mengajukan perubahan batas kritis | `Diajukan` | Kepala instalasi laboratorium | Ya |
| `Diajukan` | Menyetujui | `Berlaku` | Pihak klinis yang berwenang | Ya |
| `Diajukan` | Menolak | `Ditolak` | Pihak klinis yang berwenang | Ya |
| `Diajukan` | Menarik pengajuan | `Ditarik` | Pengaju | Ya |

Perubahan **batas normal** tidak melewati perjalanan ini. Ia langsung berlaku dan tetap
menghasilkan riwayat.

---

## H. Tanggung Jawab Authorization

Wewenang dinyatakan sebagai **kemampuan**, bukan jabatan. Ini menegakkan `LAB-INH-007` dan
`LAB-DEC-022`.

| Kemampuan | Boleh melakukan | Tidak boleh |
|---|---|---|
| Memesan pemeriksaan | Membuat pesanan, menandai cito | Menyentuh sampel |
| Merencanakan sampel | Menambah rencana sampel pada pesanan | Menyatakan layak |
| Mengambil sampel | Mencatat pengambilan | Menyatakan layak — dijaga pengujian `#PermissionPengambilanDanPenetapanLayak_TidakBolehSama@c87d9c0` |
| Menerima sampel | Mencatat sampel tiba | Menyatakan layak |
| Menetapkan kelayakan | Menyatakan layak, menolak, meminta ambil ulang | Mengubah batas nilai |
| Menahan pekerjaan | Menahan dan melanjutkan | Membatalkan |
| Membatalkan | Membatalkan sampel dan pesanan | Menghapus riwayat |
| Mengelola batas nilai | Mengubah satuan, batas normal, daftar pilihan, batas waktu cito | **Mengubah batas kritis secara langsung** |
| Menyetujui batas kritis | Menyetujui atau menolak pengajuan perubahan batas kritis | — |
| Mengelola alasan penolakan | Menambah, menamai, mengurutkan, menonaktifkan alasan | Mengubah penanda kesalahan internal dan penanda wajib catatan |
| Administrasi sistem | Menyetel penanda kesalahan internal dan penanda wajib catatan | — |

**Yang tidak dirancang di sini.** Kemampuan **validasi hasil** dan **rilis hasil** memang sudah
diputuskan `LAB-DEC-022`, tetapi keduanya melekat pada slice `S4` yang terblokir `LAB-SIGN-001`.
Dokumen ini hanya mencatat keberadaannya, tidak merancang batasnya.

**Yang tetap milik rumah sakit.** Penetapan siapa yang layak memegang tiap kemampuan adalah
keputusan kepegawaian dan kompetensi. Arsitektur tidak menentukannya dan tidak mengarangnya.

---

## I. Model Audit dan Histori

Setiap perpindahan status yang material menghasilkan satu catatan yang **tidak pernah diubah
dan tidak pernah dihapus**.

| Yang wajib tercatat | Contoh isi |
|---|---|
| Objek yang berpindah | Pesanan atau sampel |
| Identitas objek | Pesanan, sampel, dan kunjungan yang bersangkutan |
| Tindakan | `Specimen.Accept` |
| Status asal dan tujuan | `Received` menjadi `Accepted` |
| Alasan terkendali dan catatannya | `INSUFFICIENT_QUANTITY`, "volume darah 0,5 mL" |
| Pelaku | Identitas pengguna yang melakukan |
| Waktu kejadian | Waktu sebenarnya tindakan terjadi |
| Korelasi | Penanda yang menghubungkan satu rangkaian tindakan |

Bukti bahwa isian ini sudah tersedia: `TrxLabTransitionHistory.cs@c87d9c0` memuat `Scope`,
`Action`, `FromStatus`, `ToStatus`, `ReasonCode`, `ReasonNote`, `ActorUserId`, `OccurredAt`,
dan `CorrelationId`.

**Tambahan untuk batas nilai.** Riwayat perubahan batas menyimpan kolom yang berubah, nilai
lama, nilai baru, pengaju, penyetuju bila ada, waktu, dan alasan. Ini konsep baru; belum ada
buktinya di kode.

---

## J. Model Integrasi

### `INT-01` — Laboratorium ke Billing

| Aspek | Isi |
|---|---|
| Produsen | `BC-LAB` |
| Konsumen | `BC-BIL` |
| Tujuan bisnis | Memberi tahu Billing bahwa sebuah pemeriksaan sudah sah ditagihkan, atau bahwa sebuah kejadian klinis dibatalkan |
| Sumber kebenaran | `BC-LAB` untuk keadaan operasional; `BC-BIL` untuk seluruh akibat uang |
| Arah | Satu arah, `BC-LAB` ke `BC-BIL` |
| Pemicu | Sampel berpindah ke `Accepted`, dan pembatalan atas sampel yang pernah `Accepted` |
| Idempotensi | **Wajib.** Penetapan layak yang diulang tidak boleh menggandakan kelayakan tagih. Sudah terbukti pada `#PenetapanLayakDiulang_TidakMenggandakanTagihan@c87d9c0` |
| Perilaku saat gagal | Fakta memiliki status penyaluran tersendiri: `Pending`, `Dispatched`, `Rejected`, `OutcomeUnknown`, `SuppressedNoPriorCharge` — `ClinicalMilestoneFactEnums.cs@c87d9c0` |
| Rekonsiliasi | Status `OutcomeUnknown` menyatakan hasil belum diketahui, dan **bukan** berarti berhasil maupun gagal. Fakta bertanda itu wajib direkonsiliasi |

**Batas yang tidak boleh dilanggar.** Isi fakta memuat kejadian, identitas sumber, dan salinan
tarif saat kejadian. Ia **tidak** memuat keputusan tagihan, tidak memuat status pembayaran, dan
tidak memuat pembalikan. `LAB-INH-010` dan `LAB-INH-012`.

### `INT-02` sampai `INT-04` — dirujuk, bukan diintegrasikan

| Batas | Sifat |
|---|---|
| `BC-LAB` ke `BC-REG` | Pembacaan langsung atas kunjungan yang sudah ada. Bukan integrasi asinkron |
| `BC-LAB` ke `BC-MD` | Pembacaan katalog pemeriksaan dan tarif, disertai penyimpanan salinan sesaat |
| `BC-LAB` ke `BC-PLAT` | Pemeriksaan kewenangan per aksi saat permintaan datang |

**Integrasi eksternal.** Tidak ada. `LAB-DEC-005` menyatakan Rilis 1 tidak menyambung ke alat
laboratorium. Tidak ada kontrak pihak ketiga yang dirancang maupun diasumsikan.

---

## K. Dampak Billing

**Klasifikasi: berdampak pada charge.**

| Kejadian | Akibat finansial | Pemilik keputusan |
|---|---|---|
| Sampel dinyatakan layak | Pemeriksaan menjadi sah ditagihkan | `BC-BIL` |
| Sampel ditolak | Secara bawaan tidak ada tagihan pemeriksaan | `BC-BIL` |
| Pembatalan sebelum layak | Tidak ada tagihan pemeriksaan | `BC-BIL` |
| Pembatalan setelah layak | Tagihan **tidak** hilang otomatis; Billing menentukan pembatalan, tagihan sebagian, atau penyesuaian | `BC-BIL` |
| Ambil ulang karena kesalahan internal | Tidak menambah tanggungan pasien secara otomatis | `BC-BIL` |

**Contoh yang menunjukkan batas ini bekerja.**

> Pesanan pasien Andi berisi tiga pemeriksaan: Darah lengkap Rp200.000, Fungsi hati
> Rp150.000, Urin lengkap Rp100.000. Dua dinyatakan layak, Urin lengkap ditolak karena volume
> kurang. Yang diserahkan ke Billing adalah dua kejadian kelayakan tagih senilai Rp350.000,
> bukan Rp450.000. Laboratorium **tidak** menghitung, tidak menjumlahkan untuk ditagihkan, dan
> tidak memutuskan apa pun soal uangnya — ia hanya menyerahkan dua kejadian beserta salinan
> tarifnya. Diuji pada `#DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu@c87d9c0`.

---

## L. Dampak Keselamatan Klinis

**Klasifikasi: relevan terhadap keselamatan.**

| Aspek | Kenapa relevan | Batas yang dibuat eksplisit |
|---|---|---|
| Identitas sampel | Sampel tertukar berarti pasien menerima hasil milik orang lain | Setiap sampel punya barcode sendiri dan penelusuran pasti ke pesanan, kunjungan, dan pasien. Barcode tidak menggantikan tautan data, dan tidak memuat identitas pasien |
| Kelayakan sampel | Sampel tidak layak yang tetap dikerjakan menghasilkan angka yang salah | Penerimaan dan penetapan kelayakan dipisah tegas. Penolakan wajib beralasan terkendali |
| Ambil ulang | Sampel pengganti yang tidak tertaut menyulitkan penelusuran saat terjadi masalah | Sampel lama tetap terlihat dan tertaut ke penggantinya |
| Batas kritis | Angka ini menentukan kapan pasien dinyatakan dalam bahaya | Perubahannya memerlukan persetujuan klinis dan seluruhnya berriwayat |
| Bentuk hasil | Hasil kualitatif yang diketik bebas tidak dapat dinilai kritis oleh sistem | Pemeriksaan berbentuk pilihan hanya menerima nilai dari daftar yang sah |

**Keputusan keselamatan yang belum selesai.** `LAB-SIGN-001` — tanda tangan klinis atas
`LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007`. Ketiganya mengikat slice yang **tidak**
dirancang di sini, sehingga tidak menghalangi arsitektur scope ini.

---

## M. Gap Arsitektur

### `DEC-LAB-008` — Apakah satu wadah fisik dapat melayani beberapa pemeriksaan? — **SUDAH DITUTUP**

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat ditemukan, kini **`CONFIRMED`** |
| Dampak | `BLOCKING` saat ditemukan, kini **tertutup** |
| Ditutup oleh | **`LAB-DEC-024`** pada 2026-09-01, `grill-me` closure pass putaran ketiga |
| Keputusan | **Wadah fisik dipisahkan dari pemeriksaan terpesan.** Satu wadah = satu barcode = satu keputusan layak atau tolak, dan dapat melayani beberapa pemeriksaan. Kelayakan tagih tetap terbit per pemeriksaan. Penolakan berlaku serentak bagi seluruh pemeriksaan pada wadah itu |
| Pemilik keputusan | Yoga Aji Pratama |

Uraian di bawah dipertahankan sebagai rekam jejak mengapa gap ini ditemukan dan apa
konsekuensinya, agar penilaian berikutnya tidak mengulang analisis yang sama.

**Bukti keadaan saat ini.** Satu baris sampel membawa **tepat satu** jenis pemeriksaan, punya
**barcode sendiri**, punya keputusan layak atau tolak **sendiri**, dan menghasilkan **satu baris
tagihan sendiri**. Terbukti pada `TrxLabSpecimen.ProcedureId@c87d9c0` dan pada pengujian
`#DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu@c87d9c0`, yang membuat tiga sampel
terpisah untuk tiga pemeriksaan lalu menyatakan dua layak dan satu ditolak.

**Kenapa ini menjadi masalah.** Dalam pengujian itu, ketiga pemeriksaan memang memakai wadah
yang berbeda: darah dengan tabung EDTA, fungsi hati dengan tabung serum, urin dengan wadah
urin. Modelnya tampak benar.

Masalahnya muncul ketika **dua pemeriksaan berbagi satu wadah fisik yang sama**. Contoh yang
lazim setiap hari:

> Fungsi hati dan fungsi ginjal keduanya diperiksa dari **satu tabung serum yang sama**.
> Perawat menusuk pasien sekali, mengisi satu tabung.

Dengan model sekarang, keadaan itu memaksa dua baris sampel dengan dua barcode berbeda untuk
satu tabung. Akibatnya:

| Akibat | Penjelasan |
|---|---|
| Pelabelan | Petugas harus menempelkan dua label pada satu tabung, atau memilih salah satu. Keduanya membuka peluang tertukar |
| Penolakan | Bila serum tabung itu keruh dan tidak layak, kenyataannya **kedua** pemeriksaan gagal serentak. Model sekarang mengizinkan menolak satu dan menerima yang lain — sesuatu yang tidak mungkin terjadi secara fisik |
| Ambil ulang | Satu penusukan ulang seharusnya menggantikan kedua pemeriksaan sekaligus. Model sekarang memperlakukannya sebagai dua penggantian terpisah |

**Usulan dekomposisi, berstatus `PROPOSED` dan bukan keputusan.** Pisahkan dua konsep yang saat
ini menyatu:

| Konsep | Peran | Satuan |
|---|---|---|
| **Wadah Fisik** (`LAB-DC-003`) | Wadah nyata berisi bahan dari pasien | Satu barcode, satu pengambilan, satu keputusan layak atau tolak |
| **Pemeriksaan Terpesan** (`LAB-DC-002`) | Jenis pemeriksaan yang diminta | Satu tarif, satu baris tagihan, dan kelak satu hasil |

Hubungannya satu sampel melayani banyak pemeriksaan. Kelayakan tagih tetap terbit **per
pemeriksaan**, dipicu oleh dinyatakan layaknya sampel yang menopangnya — tetap sesuai
`LAB-INH-009`.

**Kenapa ini harus diputuskan sekarang, bukan nanti.** Hasil pemeriksaan pada slice `S4`
melekat pada **pemeriksaan**, bukan pada wadah. Bila pemisahan ini diputuskan setelah tabel
hasil dibangun, perbaikannya menyentuh hasil pasien yang sudah tersimpan — jauh lebih mahal dan
berisiko daripada memutuskannya sekarang, ketika belum ada satu baris hasil pun.

**Bagaimana ini akhirnya diputuskan.** Pemilik modul memilih memisahkan kedua konsep, sesuai
usulan dekomposisi di atas. Aturan lengkapnya ada pada `00-interview-decisions.md#BR-20`.
Konsekuensi yang tercatat: keputusan layak atau tolak berpindah dari pemeriksaan ke wadah, dan
penolakan sebagian atas satu wadah tidak lagi mungkin dilakukan.

**Satu hal yang wajib diperiksa sebelum dikerjakan.** Perubahan ini menyentuh struktur data yang
sudah berjalan. Bukti `CAP-21` menunjukkan frontend Laboratorium masih nol, sehingga kemungkinan
besar belum ada data pasien sungguhan yang harus dipindahkan — tetapi itu **dugaan, bukan
bukti**. Diverifikasi lewat `LAB-OPEN-012`.

### Gap yang dibawa dari tahap sebelumnya

| ID | Isi | Slice terdampak | Dirancang di sini? |
|---|---|---|---|
| `LAB-SIGN-001` | Tanda tangan klinis | `S4`, `S5`, `S6` | Tidak |
| `LAB-AMD-001` | Amandemen `rawat-jalan` | `S1b` | Tidak |
| `LAB-COORD-001` | Kepemilikan pemberitahuan | `S5`, `S8` | Tidak |
| `LAB-COORD-002` | Jenis dokumen klinis baru | `S6`, `S9` | Tidak |

---

## N. Kesiapan Arsitektur

**`DOMAIN_ARCHITECTURE_READY`** untuk keenam slice.

### Slice yang siap

| Slice | Nama | Keterangan |
|---|---|---|
| `S1a` | Penandaan cito pada pesanan | Melekat pada pesanan. Tidak pernah tersentuh `DEC-LAB-008` |
| `S2` | Siklus hidup wadah dan pemeriksaan | **Baru terbuka** oleh `LAB-DEC-024`. Satuan keputusan kelayakan kini jelas: wadah |
| `S3` | Batas nilai dan batas kritis | Menempel pada jenis pemeriksaan milik `BC-MD`. Tidak pernah tersentuh `DEC-LAB-008` |
| `S7` | Daftar kerja dan pemantauan keterlambatan | **Baru terbuka.** Satuan pekerjaan kini dapat ditetapkan tanpa menebak |
| `S10` | Fakta kelayakan tagih | **Baru terbuka.** Fakta terbit per pemeriksaan, dipicu layaknya wadah yang menopangnya |
| `S11` | Master alasan penolakan sampel | Data rujukan mandiri. Tidak pernah tersentuh `DEC-LAB-008` |

Keenamnya boleh diteruskan ke `design-business-module`.

### Yang tetap berada di luar scope ini

`S1b`, `S4`, `S5`, `S6`, `S8`, dan `S9` **tidak** dirancang di dokumen ini dan tetap terblokir
oleh `LAB-SIGN-001`, `LAB-AMD-001`, `LAB-COORD-001`, dan `LAB-COORD-002`. Keempat blocker itu
memerlukan pihak di luar modul Laboratorium.

### Peringatan pelaksanaan

`LAB-DEC-024` mengubah struktur data yang sudah berjalan di produksi. Arsitektur ini menyatakan
**maksud bisnisnya**, bukan izin mengubah kode. Sebelum perubahan dikerjakan, `LAB-OPEN-012`
wajib dijawab: berapa banyak data laboratorium yang benar-benar sudah terisi. Selama belum
dijawab, yang berjalan di produksi tidak boleh disentuh.

---

## Handoff

### Ke `design-business-module`

| Field | Nilai |
|---|---|
| Modul | `laboratorium` |
| Slice yang diserahkan | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11` |
| Kesiapan requirement | `PARTIALLY_READY` — `LAB-RCG-001` rev 2 |
| Revision arsitektur | `LAB-DA-001` rev 2 |
| Kesiapan arsitektur | **`DOMAIN_ARCHITECTURE_READY`** untuk keenam slice |
| Decision ID yang mengikat | `LAB-DEC-013`, `LAB-DEC-009` (`S1a`); `LAB-INH-002`, `LAB-INH-005`, `LAB-INH-008`, `LAB-DEC-024` (`S2`); `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-023` (`S3`); `LAB-DEC-013` (`S7`); `LAB-INH-009` sampai `LAB-INH-012`, `LAB-DEC-024` (`S10`); `LAB-DEC-019` (`S11`) |
| Blocker yang belum selesai | `LAB-SIGN-001`, `LAB-AMD-001`, `LAB-COORD-001`, `LAB-COORD-002` — tidak satu pun menyentuh keenam slice ini |
| Peringatan yang wajib dibawa | `LAB-OPEN-012` — jumlah data laboratorium yang sudah terisi belum diverifikasi. `LAB-DEC-024` mengubah struktur data yang sudah berjalan |
| Source SHA | BE `c87d9c0`; FE `688daff90` |
| Baseline rujukan | Tidak dipakai |

### Ke `grill-me`

| Field | Nilai |
|---|---|
| Status | **Selesai untuk scope ini.** `DEC-LAB-008` ditutup `LAB-DEC-024` pada 2026-09-01 |
| Kapan dipanggil lagi | Setelah `LAB-SIGN-001`, `LAB-COORD-001`, `LAB-COORD-002`, atau `LAB-AMD-001` dijawab pihak berwenang |

### Ke `trace-existing-capabilities`

| Field | Nilai |
|---|---|
| Alasan | `LAB-DEC-024` mengubah disposisi `CAP-02` pada capability map: siklus hidup sampel tidak lagi `Ready to reuse` untuk arsitektur target, melainkan `Extend` berstruktur |
| Kapan | Sebelum `plan-module-delivery`, atau bila backend bergerak dari `c87d9c0` |

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 9 | 2026-09-25 | **`S4d-1` validasi dan rilis Mikrobiologi dirancang** (bagian A6), menurunkan `LAB-RCG-001-r9` bagian 0D. **A6 memperluas A5, tidak menyalinnya.** **Nol konsep, nol aggregate, nol relasi baru** — `LAB-DC-051`..`058` berlaku apa adanya, dengan kode kewenangan Mikrobiologi dan daftar alasan yang **sama** lintas disiplin. **Dua invariant baru:** `INV-52` menolak validasi dan rilis hasil `Sementara` sampai `DEC-LAB-020` dijawab; `INV-53` menegaskan hasil berstruktur — status temuan, isolat, antibiogram — disahkan sebagai **satu** kesatuan, bukan per isolat. **Satu usulan:** `ARCH-GAP-LAB-10` — hasil **tanpa** kualifikasi diperlakukan bukan `Sementara`, sebab ruas itu sengaja opsional (`r10` 12.2); disarankan ikut ditanyakan bersama `DEC-LAB-020`. **Satu perbedaan makna dijaga:** *Definitif* adalah kualifikasi isi hasil, *Tervalidasi*/*Dirilis* adalah pengesahan — dan rilis **tidak** mengirim apa pun kepada pasien (`LAB-OPEN-029` tetap terpisah). `DOMAIN_ARCHITECTURE_READY` untuk desain saja; `S4d-2` dan `S4e` tidak dirancang | `draft` |
| 8 | 2026-09-24 | **`S4` validasi dan rilis Patologi Klinik dirancang** (bagian A5), menurunkan `LAB-RCG-001-r8` bagian 0C. **Nol aggregate baru, nol status baru.** Delapan konsep (`LAB-DC-051`..`058`) dan tiga domain event — `LAB-DC-058` alasan pengecualian empat mata ditambahkan pada hari yang sama sesudah pemeriksaan ulang terhadap `LAB-DEC-003` menemukan jalur pengecualian **nyata** pada rilis malam hari; sebelas invariant (`INV-41`..`51`). **Dua context yang sudah ada disebut untuk pertama kali:** `BC-HR` sebagai upstream penunjukan kewenangan — Laboratorium hanya membaca, dan **fail-closed** bila datanya kosong — serta `BC-MRC` sebagai downstream dokumen hasil. **`ARCH-GAP-LAB-05` ditinjau ulang** seperti yang dimintanya sendiri: validasi dan rilis cukup dijaga per pemeriksaan, sehingga gap itu menyempit. **Dua gap baru:** `ARCH-GAP-LAB-08` lima usulan yang wajib ditandai usulan, dan `ARCH-GAP-LAB-09` pembatalan sesudah rilis yang ditolak. **Pemeriksaan kelengkapan 2026-09-25** — sesi 2026-09-24 terhenti sebelum A5 diperiksa terhadap kontrak — menyambungkan `LAB-DC-058` ke relasi dan audit, menambah dua usulan pada `ARCH-GAP-LAB-08` (pengelola daftar alasan pengecualian, bunyi penanda rangkap pemvalidasi–perilis), dan **membetulkan rujukan `ARCH-GAP-LAB-09`**: penolakannya diturunkan dari `LAB-DEC-138`/`007`/`063`, bukan `DEC-LAB-014`, dan ketegangannya dengan `LAB-DEC-049` kini tercatat. `DOMAIN_ARCHITECTURE_READY` **untuk desain saja** — pemakaian nyata tertahan `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, dan `LAB-COORD-016` | `draft` |
| 7 | 2026-09-18 | **`S4c` dirancang ulang seluruhnya sesudah bukti `LAB-EVD-003`** — ditulis sebagai bagian **A4**, dan A3 dicabut sejauh menyangkut `S4c`. **Delapan konsep, sembilan invariant baru, nol aggregate baru.** **Satu penilaian revision 6 saya cabut sendiri, dan alasannya pantas disimpan:** laporan Patologi Anatomi dinilai `VALUE_OBJECT` atas tiga alasan — ketiga bagiannya selalu utuh, nol yang menunjuk kepadanya, dan ia berubah sebagai satu kesatuan. **Ketiganya gugur** oleh bukti baru: ruasnya bukan tiga melainkan sampai lima belas dan mana yang wajib bergantung kategori; nilai parameter kini menunjuk kepadanya dan ditambah-kurangi satu per satu; dan ia punya **lifecycle sendiri** — difinalkan, dibuka kembali, difinalkan lagi. **Sesuatu yang punya lifecycle dan punya yang menunjuk kepadanya adalah entity.** Penilaian revision 6 benar terhadap bukti saat itu; buktinya yang bertambah. **Yang paling dijaga pada revision ini satu kata:** `Final` **bukan** rilis. Ia dicatat sebagai fakta `FinalizedAt`/`FinalizedByUserId`, dan `INV-36` menegakkan nol status lifecycle. Bila `Final` diartikan rilis, `LAB-DEC-003` yang melarang pengisi merilis hasilnya sendiri akan bertabrakan dengan `LAB-DEC-090` yang menetapkan pengisi hasil PA adalah Dokter Lab — **pada orang yang sama**. **`INV-38` menolak dua kolom yang diminta artifact:** Waktu Issued dan Waktu Efektif **nol disimpan**, keduanya diturunkan dari `FinalizedAt` dan `LabSpecimen.CollectedAt`. **Dua gap baru dicatat apa adanya:** `ARCH-GAP-LAB-06`, `AGG-LAB-01` membesar lagi oleh empat konsep transaksional dan wajib ditinjau saat `S4d`/`S4e` dirancang; dan `ARCH-GAP-LAB-07`, konteks klinis menyentuh **alur pemesanan yang sudah berjalan** — bukan penahan, tetapi biayanya nyata dan tidak boleh ditemukan saat implementasi. **Empat pengecualian pada handoff seluruhnya BAGIAN, bukan slice:** gambar, HL7, cetak bilingual, dan Informasi Specimen. Laporan PA tetap utuh dan dapat dipakai patolog tanpa satu pun di antaranya | `draft` |
| 6 | 2026-09-18 | **`S4b` naik `DOMAIN_ARCHITECTURE_READY` sesudah `LAB-DEC-084` menutup `DEC-LAB-015` pada hari yang sama.** Organisme dan antibiotik menjadi **dua data induk terkendali milik `BC-LAB`**, berprefix `Lab` mengikuti `LAB-OPEN-021`. `LAB-DC-041` dan `LAB-DC-042` memperoleh pemilik, identitas, dan invariant; relasi isolat→organisme dan kepekaan→antibiotik menjadi **wajib**, pengetikan bebas ditolak. Dua invariant ditambahkan: `INV-30` menolak teks bebas, dan `INV-31` menyatakan data induk yang dinonaktifkan tidak dapat dipakai pada baris **baru** tetapi **tidak mengubah** baris lama — pola yang sama dengan penunjuk batas nilai pada `S4a`, dan alasannya sama: hasil yang sudah terjadi tidak boleh berubah karena data induknya diperbarui. **Penempatan kedua data induk di dalam `BC-LAB` diberi pembelaan tersendiri** supaya tidak terbaca sebagai duplikasi data induk bersama: nol pemilik tandingan yang disaingi — sistem belum punya keduanya di mana pun — dan panel antibiotik uji kepekaan memang milik laboratorium secara domain, bukan formularium farmasi. **Satu pekerjaan diwajibkan ikut ke blueprint:** dua data induk baru **beserta cara mengisinya**, ditulis tegas karena `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan data induk tanpa endpoint tulis adalah kegagalan yang sudah berulang dua kali di modul ini. **Verdict naik menjadi `DOMAIN_ARCHITECTURE_READY`**, dan kedua slice diserahkan ke `design-business-module`. Yang dikecualikan tinggal **satu bagian, bukan satu slice**: gambar hasil Patologi Anatomi, menunggu `DEC-LAB-016`. Riwayat penilaian `S4b` pada revision 5 **sengaja dipertahankan** di A3.16 — urutannya bermakna, dan menghapusnya akan menghilangkan pelajarannya | `draft` |
| 5 | 2026-09-18 | **Perluasan untuk `S4b` dan `S4c`, dua slice pertama dari bagian hasil yang lolos gerbang** — ditulis sebagai bagian **A3**. Tujuh konsep ditambahkan, **nol bounded context baru**, **nol aggregate baru**, dan **nol status baru** — yang terakhir konsekuensi langsung `LAB-DEC-080`. **Keputusan pemodelan yang paling menentukan:** laporan Patologi Anatomi dimodelkan sebagai `VALUE_OBJECT`, bukan entity, sebab ketiga bagiannya wajib terisi, nol konsep lain menunjuk kepadanya, dan ia berubah sebagai satu kesatuan; sedangkan isolat mikrobiologi dan kepekaan antibiotik **memang** entity, sebab barisnya ditambah dan dikurangi satu per satu. **Satu kata diberi peringatan tersendiri:** BR-23 menyebut *"status `Normal`/`Positif`/`Negatif`"*, dan itu **bukan** status lifecycle melainkan **nilai hasil** — ia tidak bertentangan dengan `LAB-DEC-080`, dan dinamai ulang menjadi **status temuan** supaya tidak disalahartikan. **Verdict-nya `DOMAIN_ARCHITECTURE_PARTIAL`, dan itu koreksi ketiga atas gerbang `r7`:** `S4c` `READY` dan berdiri sendiri; **`S4b` `BLOCKED`** oleh `DEC-LAB-015` — BR-23 menyebut *"organisme per bakteri, antibiotik"* tanpa pernah menyatakan apakah keduanya **data induk terkendali**, dan tanpa itu identitas konsep intinya hanya dapat ditebak. Bila teks bebas, satu kuman akan tertulis empat cara dan **antibiogram rumah sakit mustahil disusun** — alasan yang sama persis dengan yang dipakai `LAB-DEC-082`. `S4c` tidak terkena hal itu karena **narasi tidak menunjuk data induk apa pun**. **Dua gap lain dicatat apa adanya:** `DEC-LAB-016` — gambar patologi adalah data klinis yang dapat mengidentifikasi pasien, sedangkan pola penyimpanan berkas yang ada di platform berupa `SaveFileAsync` privat per area di atas `IWebHostEnvironment`, nol layanan bersama; dan `ARCH-GAP-LAB-04` — hasil mikrobiologi **tidak punya cara menyatakan dirinya lengkap**, sebab biakan dibaca bertahap berhari-hari, dan itulah **biaya `LAB-DEC-081` yang baru terlihat di meja arsitektur**. Bagian gambar `S4c` dikecualikan dari handoff | `draft` |
| 4 | 2026-09-01 | `DEC-LAB-009` ditutup `LAB-DEC-035` dan `DEC-LAB-010` ditutup `LAB-DEC-036`. `S13b` terbuka, sehingga seluruh scope menjadi `DOMAIN_ARCHITECTURE_READY`. `LAB-DC-031` Sumber Rujukan berubah dari `UNRESOLVED` menjadi data induk global milik `BC-MD` | `draft` |
| 3 | 2026-09-01 | Perluasan untuk `S13`, `S14`, `S15` setelah bukti lapangan diadopsi. Enam konsep ditambahkan, lima di antaranya tanpa tabel baru. Tiga invariant baru. Dua batas integrasi baru. Dua gap ditemukan: `DEC-LAB-009` tempat sumber rujukan, `DEC-LAB-010` penanda disiplin pada jenis pemeriksaan. `S13b` terblokir | `draft` |
| 2 | 2026-09-01 | `DEC-LAB-008` ditutup `LAB-DEC-024`: wadah fisik dipisahkan dari pemeriksaan terpesan. `S2`, `S7`, dan `S10` terbuka sehingga keenam slice kini `DOMAIN_ARCHITECTURE_READY`. Katalog konsep dan bahasa domain disesuaikan. Peringatan `LAB-OPEN-012` dibawa ke hilir | `draft` |
| 1 | 2026-09-01 | Arsitektur domain pertama untuk enam slice yang dikirim gerbang requirement. Lima bounded context dipetakan, 14 konsep domain dikatalogkan, tiga aggregate ditetapkan dengan 19 invariant. Gap `DEC-LAB-008` ditemukan: wadah fisik dan pemeriksaan terpesan menyatu dalam satu konsep. Tiga slice dinyatakan siap, tiga berhenti | `draft` |
