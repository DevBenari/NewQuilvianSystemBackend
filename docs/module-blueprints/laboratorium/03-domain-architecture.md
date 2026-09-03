# Laboratorium — Arsitektur Domain Rumah Sakit

## A. Identitas Arsitektur

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Architecture ID | `LAB-DA-001` |
| Revision | `4` |
| Status | `draft` |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_READY`** — 10 slice siap |
| Scope yang dinilai | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11` (rev 2); `S13a`, `S13b`, `S14`, `S15` (rev 3) |
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
| 4 | 2026-09-01 | `DEC-LAB-009` ditutup `LAB-DEC-035` dan `DEC-LAB-010` ditutup `LAB-DEC-036`. `S13b` terbuka, sehingga seluruh scope menjadi `DOMAIN_ARCHITECTURE_READY`. `LAB-DC-031` Sumber Rujukan berubah dari `UNRESOLVED` menjadi data induk global milik `BC-MD` | `draft` |
| 3 | 2026-09-01 | Perluasan untuk `S13`, `S14`, `S15` setelah bukti lapangan diadopsi. Enam konsep ditambahkan, lima di antaranya tanpa tabel baru. Tiga invariant baru. Dua batas integrasi baru. Dua gap ditemukan: `DEC-LAB-009` tempat sumber rujukan, `DEC-LAB-010` penanda disiplin pada jenis pemeriksaan. `S13b` terblokir | `draft` |
| 2 | 2026-09-01 | `DEC-LAB-008` ditutup `LAB-DEC-024`: wadah fisik dipisahkan dari pemeriksaan terpesan. `S2`, `S7`, dan `S10` terbuka sehingga keenam slice kini `DOMAIN_ARCHITECTURE_READY`. Katalog konsep dan bahasa domain disesuaikan. Peringatan `LAB-OPEN-012` dibawa ke hilir | `draft` |
| 1 | 2026-09-01 | Arsitektur domain pertama untuk enam slice yang dikirim gerbang requirement. Lima bounded context dipetakan, 14 konsep domain dikatalogkan, tiga aggregate ditetapkan dengan 19 invariant. Gap `DEC-LAB-008` ditemukan: wadah fisik dan pemeriksaan terpesan menyatu dalam satu konsep. Tiga slice dinyatakan siap, tiga berhenti | `draft` |
