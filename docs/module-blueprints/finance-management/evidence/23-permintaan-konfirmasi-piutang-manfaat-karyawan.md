# Permintaan Konfirmasi Finance kepada Billing, Registrasi, dan HR — Piutang Manfaat Karyawan

| Field | Nilai |
|---|---|
| Status | **DRAF — belum dikirim.** Menunggu tinjauan Yasmin (lihat kotak "Catatan untuk pengirim") |
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Owner **Billing dan Kasir** (`BIL-CASH-001`); owner **Registrasi** (Patient Management); owner **HR** (Benefit, Penggajian, Siklus Kepegawaian) |
| Tembusan | Owner Accounting — hanya untuk butir B6 |
| Tanggal | 5 Oktober 2026 |
| Sifat | **Permintaan konfirmasi.** Finance sudah memutuskan aturan sisi Finance dan kini menanyakan hal-hal yang jatuh ke modul kalian. Belum meminta siapa pun menulis kode sekarang |
| Dasar | `docs/module-blueprints/finance-management/00-interview-decisions.md` — pass 5 Oktober 2026, `FIN-DEC-161`..`175`; `01-existing-capability-map.md` bagian 21. Melanjutkan permintaan bagian 3 pada `evidence/02-permintaan-kontrak-untuk-owner-billing.md` (20 September 2026) yang belum dijawab |
| Yang memblokir | **Implementasi** `EPIC FIN-04` (piutang manfaat karyawan). Tidak menahan MVP Finance |

> **Catatan untuk pengirim — hapus kotak ini sebelum dikirim.**
> 1. Bagian 6 memuat dua jalur untuk Billing. Finance **belum** menyatakan pilihan; tentukan apakah ingin mencantumkan usulan.
> 2. `FIN-ASM-EMP-01` (write-off karyawan mengikuti pola `FIN-DEC-012`) masih **asumsi** dan belum pernah ditanyakan kepada pemilik; konfirmasi sebelum dikirim, karena bagian 3 dan 7 menyiratkan penghapusan hanya lewat pengajuan dan persetujuan.
> 3. Nama pemilik Billing, Registrasi, dan HR belum tercatat di blueprint; isi pada kolom "Untuk".
> 4. Pekerjaan *Data Tagihan* (`BE-FIN-FIX-001`) belum di-commit. Bagian 4 menyebutnya sebagai "sedang dikerjakan", bukan sebagai fakta final.

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Finance.

---

## 1. Mengapa Finance menghubungi kalian

Rumah sakit memberi layanan kepada pegawainya dan keluarga pegawai. Bagian dari tagihan itu menjadi
tanggungan pegawai, dan Finance ingin mencatatnya sebagai piutang lalu dipotong dari gaji.

**Contoh singkat.** Istri pegawai Budi dirawat inap. Tagihan Rp 10.000.000; benefit Budi masih tersisa
Rp 6.000.000. Maka Rp 6.000.000 ditanggung benefit, dan **Rp 4.000.000** menjadi piutang atas nama Budi.

Pada 20 September 2026 Finance sudah meminta Billing dan HR menjawab dua hal (`evidence/02` bagian 3):
menambah jenis serah terima karyawan, dan menentukan siapa yang menetapkan pemilik manfaat. Permintaan itu
**belum dijawab**, dan rumpun ini sengaja ditahan.

Sejak itu dua hal berubah, sehingga permintaan ini **lebih kecil dan lebih konkret** daripada yang dulu:

1. Finance sudah **menyelesaikan aturan sisi Finance** lewat wawancara pemilik (bagian 3).
2. Audit source 5 Oktober 2026 menemukan bahwa **sebagian kemampuan yang dibutuhkan sudah ada** di modul kalian
   (bagian 4). Finance tidak ingin kalian membangun ulang sesuatu yang sudah ada.

Finance **tidak** akan mengubah modul kalian, dan **tidak** akan menentukan aturan internal kalian.

---

## 2. Satu contoh dari awal sampai lunas

Contoh ini memakai nama rekaan. Angka dan langkahnya mengikuti keputusan Finance.

| Langkah | Yang terjadi | Pihak | Angka |
|---:|---|---|---|
| 1 | Siti, istri pegawai Budi, dirawat inap. Petugas pendaftaran menetapkan **pemilik manfaat = Budi** dan **hubungan = pasangan** (`SPOUSE`) | Registrasi | — |
| 2 | Tagihan terbit. Benefit menanggung sampai sisa plafon; kelebihannya menjadi porsi pegawai | Billing | Tagihan Rp 10.000.000, sisa plafon Rp 6.000.000, porsi pegawai **Rp 4.000.000** |
| 3 | Billing mengirim serah terima ke Finance untuk porsi pegawai | Billing → Finance | Rp 4.000.000 |
| 4 | Finance mencatat piutang atas nama Budi dengan hubungan `SPOUSE` | Finance | Piutang Rp 4.000.000 |
| 5 | Budi dan rumah sakit sepakat membayar **4 kali Rp 1.000.000** mulai gaji November. Staf Finance mencatat perjanjian, lalu pihak berwenang lain menyetujuinya | Finance | 4 × Rp 1.000.000 |
| 6 | Finance mengirim jadwal ke HR. Gaji November diproses; HR melaporkan potongan **berhasil** | Finance → HR → Finance | Piutang menjadi **Rp 3.000.000** |
| 7 | Gaji Desember: potongan **gagal** (mis. cuti tanpa gaji). HR melaporkan gagal. Cicilan ke-2 ditandai tertunggak dan **ikut dipotong Januari** | HR → Finance | Januari dipotong Rp 2.000.000 |
| 8 | Budi mengundurkan diri saat sisa Rp 1.500.000. Gaji terakhir dan hak akhirnya dipotong lebih dulu; sisanya **wajib dilunasi**. Proses berhenti di HR **tidak dapat ditutup** sampai Finance menyatakan "bebas tanggungan" | HR ↔ Finance | Sisa Rp 0 = bebas tanggungan |
| 9 | **Bila salah orang:** ternyata pasiennya keluarga pegawai Andi. Piutang atas Budi dibatalkan, Billing menerbitkan serah terima baru atas Andi, dan HR menyelesaikan potongan yang sempat terjadi pada gaji Budi | Registrasi, Billing, HR | Pembalik Rp 4.000.000, baru Rp 4.000.000 |

---

## 3. Yang sudah diputuskan Finance

Bagian ini **bukan** permintaan. Ia memberi tahu kalian aturan yang sudah dipegang Finance, supaya
pertanyaan di bagian 5 dijawab dengan tahu konteksnya.

| Hal | Keputusan Finance | ID |
|---|---|---|
| Arti "AR karyawan" | Tagihan layanan RS atas pegawai atau keluarganya yang menjadi tanggungan pegawai. Kasbon dan piutang internal tanpa kunjungan **bukan** bagian ini | `FIN-DEC-162` |
| Cara pelunasan | Potong gaji, sekaligus atau dicicil. Cicilan selalu berdasarkan perjanjian (besaran dan waktu) | `FIN-DEC-163`, `165` |
| Nominal piutang | Hanya kelebihan di atas plafon benefit. Finance menyalin nominal dari Billing apa adanya dan **tidak** menghitung plafon sendiri | `FIN-DEC-164` |
| Perjanjian cicilan | Dicatat di Finance. Pengaju dan penyetuju orang berbeda | `FIN-DEC-166` |
| Hasil potongan | HR mengirim hasil (berhasil atau gagal) otomatis ke Finance. Kiriman ganda **tidak boleh** memotong piutang dua kali | `FIN-DEC-167` |
| Potongan gagal | Tetap terhutang dan ikut potongan periode berikutnya, tanpa mengubah perjanjian | `FIN-DEC-168` |
| Pegawai berhenti | Gaji terakhir dan hak akhir dipotong lebih dulu; sisa wajib dilunasi; tidak ada penghapusan otomatis | `FIN-DEC-169` |
| Gerbang berhenti | Proses berhenti di HR tertahan sampai Finance menyatakan bebas tanggungan | `FIN-DEC-170` |
| Penetapan pemilik manfaat | Petugas Registrasi saat pendaftaran, dibantu data HR. Finance **tidak** menentukan atau mengubah identitas ini | `FIN-DEC-171`, `FIN-DEC-016` |
| Siapa melihat | Siapa pun yang berhak pada AR, tanpa penyamaran nama | `FIN-DEC-172` |
| Pemilik manfaat salah | Piutang dibatalkan, serah terima baru atas orang yang benar; Finance tidak mengganti nama debitur | `FIN-DEC-173` |
| Saldo lama | Dimasukkan lewat batch migrasi yang diperluas, hanya bila pemilik manfaatnya sah | `FIN-DEC-174` |

---

## 4. Yang sudah ada hari ini

Hasil audit source 5 Oktober 2026 (`01-existing-capability-map.md` bagian 21). **Mohon dikoreksi bila keliru** —
audit membaca source, bukan data produksi.

| Modul | Yang sudah ada | Yang belum ada |
|---|---|---|
| **Registrasi** | Kartu hubungan pasien–perusahaan (`MstPatientCompanyGuarantor`) menyimpan nomor karyawan, nama, plan benefit, status eligible, plafon, dan ko-bayar. Kunjungan menyalinnya sebagai snapshot (`RegPatientEncounterGuarantor`). Sumber pembayaran "dijamin hubungan pasien–perusahaan" sudah ada | Hubungan keluarga (`SPOUSE`, `CHILD`). Rujukan ke profil pegawai di HR — nomor karyawan hanya teks. Pemeriksaan terhadap data HR |
| **Billing** | Perhitungan porsi penjamin perusahaan lewat aturan cakupan (batas maksimum, ko-bayar, kebijakan kelebihan). Serah terima AR `PAYER` dan `PATIENT_GUARANTOR` | Jenis serah terima karyawan. Pengurangan sisa plafon **per pegawai** (`RemainingLimitAmount` tidak dirujuk Billing). **`DebtorReferenceId` kosong** untuk kunjungan yang dijamin perusahaan |
| **HR** | Enrollment benefit dan tanggungan (hubungan, eligible, plafon, terpakai, sisa). Input variabel penggajian dengan `SourceType`/`SourceId` — **sudah dipakai modul Cuti** sebagai preseden. Pemisahan kerja (`TrxEmployeeSeparation`) dengan kolom `IsExitClearanceCompleted` dan template tugas *offboarding* | Endpoint baca eligibilitas untuk modul lain. Pemakai input variabel dari Finance. **Penulis** `IsExitClearanceCompleted` (kolomnya ada, aturannya tidak ditemukan). Pengirim hasil potongan ke Finance |
| **V1 (sistem lama)** | Tab Karyawan memakai penjamin bernama **"RS Benefit"** yang ditulis langsung di kode | — |
| **Finance** | Kolom pemilik manfaat dan hubungan pada piutang, segmen subledger "Manfaat Karyawan", write-off dengan pengaju–penyetuju | Penulis kolom itu. Perjanjian cicilan. Pelunasan tanpa penerimaan tunai. Status bebas tanggungan |

Pekerjaan *Data Tagihan* di Finance (sedang dikerjakan, belum di-commit) menampilkan tab **Karyawan** yang saat ini
**kosong** karena belum ada serah terima jenis karyawan.

---

## 5. Permintaan konfirmasi per pemilik

Finance **tidak** mengarang jawaban atas pertanyaan di bawah, dan rumpun ini tidak akan dimulai sebelum
jawabannya ada. Kolom "Usulan Finance" adalah **usulan**, bukan keputusan untuk modul kalian.

### 5.1 Untuk pemilik Billing dan Kasir

| # | Pertanyaan | Usulan Finance | Bila tidak diterima |
|---|---|---|---|
| B1 | **SUDAH DIJAWAB Finance, mohon dikonfirmasi bentuknya.** Finance memutuskan **Jalur A dan Jalur B dipakai BERSAMA untuk dua porsi berbeda** (`FIN-DEC-177`, 5 Oktober 2026): porsi tanggungan pegawai lewat serah terima `EMPLOYEE_BENEFIT`, porsi yang ditanggung benefit rumah sakit lewat serah terima `PAYER` atas penjamin "RS Benefit". **Pertanyaannya sekarang:** apakah Billing dapat menerbitkan **dua** baris serah terima untuk satu tagihan manfaat karyawan? | Dua serah terima per tagihan, lihat bagian 6 | Finance tidak dapat memisahkan porsi pegawai dari porsi benefit |
| B2 | *(Jalur A)* Apakah perluasan serah terima `BilArHandoff` dapat diterima: satu nilai `DebtorType` baru `EMPLOYEE_BENEFIT`, ditambah kolom `BenefitOwnerId` dan `BenefitRelationship`, keduanya boleh kosong sehingga baris lama tetap sah? | Diterima. Migration milik Billing | Jalur B menjadi satu-satunya pilihan |
| B3 | Siapa yang menghitung **porsi pegawai** (kelebihan di atas plafon), dan dari data apa? **Contoh:** tagihan Rp 10.000.000, sisa plafon Rp 6.000.000 → Rp 4.000.000 | Billing menghitung, Finance menyalin. Sumber plafon ditentukan bersama HR (butir H1) | Finance tidak dapat mencatat nominal yang benar |
| B4 | **SUDAH DIKERJAKAN, bukan pertanyaan lagi — mohon ditinjau.** Serah terima `PAYER` untuk kunjungan yang dijamin **perusahaan** tidak membawa identitas debitur (`DebtorReferenceId` hanya diisi dari `InsuranceProviderId`), sehingga piutangnya tidak dapat digabung ke Batch Tagihan. Atas instruksi pemilik Finance, Finance sudah memperbaikinya langsung di `BillingArApHandoffService.cs`: identitas debitur kini `InsuranceProviderId ?? CompanyGuarantorId`, dan pemilihan baris penjamin disamakan dengan pola `BillingDepositService` (penjamin yang dibatalkan dibuang, penjamin utama diutamakan). Rinciannya pada `task/report/backend/BE-FIN-FIX-002.md`. **Yang Finance minta sekarang:** (1) apakah pemilik Billing menerima perubahan ini pada modulnya, dan (2) apakah baris serah terima LAMA yang identitas debiturnya kosong perlu diisi ulang (*backfill*) — pekerjaan database yang belum diberi wewenang | Ditinjau pemilik Billing; Finance tidak menunggu untuk serah terima baru | Perubahan dapat dicabut bila pemilik Billing menolak |
| B5 | Dapatkah Billing **membatalkan penuh** sebuah serah terima lalu menerbitkan yang baru atas orang lain (koreksi pemilik manfaat)? | Ya, sebagai pasangan serah terima pembalik dan serah terima baru | Koreksi harus manual dan tidak terlacak |
| B6 | *(Tembusan Accounting)* Porsi yang ditanggung benefit (Rp 6.000.000 pada contoh) **bukan** piutang Finance. Bagaimana porsi itu diakui di pembukuan, dan siapa yang mencatatnya? | Bukan urusan Finance; Finance hanya memastikan porsi itu **tidak** masuk piutangnya | Tagihan Rp 10.000.000 tetap menyisakan porsi yang tidak ditagih ke siapa pun |

### 5.2 Untuk pemilik Registrasi

| # | Pertanyaan | Usulan Finance | Bila tidak diterima |
|---|---|---|---|
| R1 | Apakah kartu hubungan pasien–perusahaan dan sumber pembayaran kunjungan yang **sudah ada** boleh dipakai untuk menandai pegawai dan keluarganya, atau perlu pilihan baru "Pegawai RS"? | Memakai yang ada bila memungkinkan, agar tidak ada dua cara untuk satu hal | Registrasi menentukan bentuk baru; Finance menyesuaikan |
| R2 | Di mana **hubungan keluarga** (`SELF`, `SPOUSE`, `CHILD`, dan seterusnya) disimpan: pada kartu pasien atau pada kunjungan? | Pada kunjungan, karena Finance memerlukannya per tagihan | Hubungan tidak sampai ke Finance |
| R3 | Pegawai saat ini hanya berupa **nomor karyawan teks**. Bersediakah petugas memilih pegawai dari data HR (sehingga tersambung ke profil pegawai)? | Ya. Constraint database Finance mewajibkan pemilik manfaat yang sah | Risiko salah ketik dan piutang atas orang yang salah |
| R4 | Bila petugas salah memilih pegawai, bagaimana koreksinya, dan siapa yang mengajukan? | Alur koreksi di Registrasi yang memicu pembalikan (B5) | Koreksi tidak pernah sampai ke Finance |
| R5 | Bagaimana pasien yang bukan keluarga inti, atau tanggungan **tanpa nomor identitas** (mis. anak kecil), ditandai? | Dijawab pemilik Registrasi bersama HR | Kasus ini tidak dapat dicatat |
| R6 | Apakah `RemainingLimitAmount` pada kartu perusahaan dipakai layar lain sebagai batas keras? | Konfirmasi saja | Finance tidak boleh bergantung pada kolom itu |

### 5.3 Untuk pemilik HR

| # | Pertanyaan | Usulan Finance | Bila tidak diterima |
|---|---|---|---|
| H1 | Dari mana sisa **plafon** dibaca untuk kunjungan, dan untuk **tanggungan** apakah memakai plafon milik tanggungan itu atau plafon bersama keluarga? Adakah cara baca eligibilitas bagi modul lain? **Contoh:** anak Budi dirawat — plafon anak atau plafon keluarga Budi? | Ditentukan HR; Finance hanya memakai hasilnya | Billing tidak dapat menghitung porsi pegawai |
| H2 | Bersediakah HR **menerima jadwal cicilan** yang sudah disetujui Finance sebagai potongan gaji, memakai pola input variabel penggajian yang sudah dipakai modul Cuti (dengan `SourceType` baku untuk piutang Finance)? | Ya, memakai pola yang ada | Potongan harus dicatat manual di dua tempat |
| H3 | Bersediakah HR **mengirim hasil potongan** (berhasil atau gagal) ke Finance tiap periode, dengan kiriman ganda yang aman? | Ya. Finance menyediakan penerimanya | Finance mencatat manual dari rekap (jalan cadangan) |
| H4 | Berapa **batas potongan per periode** saat cicilan gagal menumpuk, dan siapa yang menetapkannya? **Contoh:** dua bulan gagal berturut-turut, bulan ketiga dipotong Rp 3.000.000 | Memakai batas maksimum dan aturan potong sebagian yang sudah ada di master jenis potongan | Gaji pegawai berisiko terpotong melebihi kemampuan |
| H5 | Bersediakah HR membuat **gerbang berhenti**: administrasi berhenti tidak dapat diselesaikan sebelum Finance menyatakan bebas tanggungan? Termasuk jalur pengecualian bila pegawai meninggal | Ya, lewat penanda *exit clearance* dan tugas *offboarding* yang sudah ada | Pegawai dapat berhenti membawa piutang |
| H6 | Bersediakah HR **memberi tahu Finance** saat status kepegawaian berubah menjadi berhenti? | Ya | Finance tidak tahu pegawai sudah berhenti |
| H7 | Bila potongan sudah terjadi pada pegawai yang **salah**, siapa yang mengembalikan atau menyelesaikannya? | HR, dipicu pembalikan dari Finance | Potongan salah menggantung |
| H8 | Kunci pegawai apa yang dipakai templat impor saldo lama: nomor pegawai, nomor identitas, atau kode lain? | Ditentukan HR | Impor saldo lama tidak dapat dipetakan ke profil pegawai |

---

## 6. Dua jalur untuk Billing

Finance meminta pemilik Billing memilih **satu** (atau urutannya). Keduanya sah menurut blueprint Finance.

| Hal | **Jalur A — perluasan resmi** (`FIN-DEC-006`) | **Jalur B — penjamin sementara** (jalan yang dipakai V1 dan tercatat di PRD) |
|---|---|---|
| Cara menandai | Serah terima jenis baru `EMPLOYEE_BENEFIT` dengan pemilik manfaat dan hubungan | Penjamin asuransi bernama "RS Benefit" bertipe `Other`; serah terima `PAYER` seperti biasa |
| Perubahan di Billing | Tambah satu nilai dan dua kolom pada `BilArHandoff`; migration milik Billing | **Tidak ada kode baru** (bila data "RS Benefit" sudah ada; lihat di bawah) |
| Pemilik manfaat per pegawai | **Ada** | **Tidak ada**; pegawai hanya terlihat dari pasien pada tagihan |
| Hubungan keluarga | **Ada** | Tidak ada |
| Status bebas tanggungan per pegawai | Dapat dihitung | **Tidak dapat** dihitung dengan andal |
| Potong gaji, cicilan, gerbang berhenti | Dapat dirancang di atas pemilik manfaat | Tidak dapat dirancang tanpa pemilik manfaat |
| Dapat dipakai kapan | Setelah Billing, Registrasi, dan HR selesai | **Segera** |
| Kelemahan | Pekerjaan terbesar di tiga modul | Tidak memenuhi aturan potong gaji dan gerbang berhenti; semua pegawai menjadi satu debitur |

Satu prasyarat **tidak dapat dijawab dari source** dan kini dilacak sebagai `FIN-OQ-102`: apakah penjamin "RS Benefit" benar-benar ada pada data V2, dan apakah ia dicatat sebagai penjamin **asuransi** atau penjamin **perusahaan**. Pilihan itu menentukan master mana yang dirujuk identitas debitur. Mohon dikonfirmasi pemilik Administrator (master data).
produksi V2. Mohon dikonfirmasi pemilik Administrator.

Keputusan Finance, `FIN-DEC-177` (5 Oktober 2026): **kedua jalur dipakai bersama**, masing-masing untuk satu porsi, sehingga keduanya **bukan** pilihan yang saling meniadakan. Jalur A memberi identitas pegawai pada porsi yang memang menjadi tanggungannya, dan Jalur B menampung porsi yang ditanggung benefit rumah sakit — meneruskan praktik V1. Yang Finance minta dari pemilik Billing adalah konfirmasi **bentuknya** (butir `B1` dan `B2`), bukan memilih salah satu.

---

## 7. Yang tidak Finance minta

| Tidak diminta | Alasan |
|---|---|
| Perubahan pada perhitungan nominal tagihan atau tender Billing | Finance hanya menyalin nominal porsi pegawai |
| Aturan eligibilitas dan plafon benefit | Milik HR; Finance hanya memakai hasilnya |
| Eksekusi penggajian dan perhitungan pajak | Milik HR/Payroll; Finance hanya mengirim jadwal dan menerima hasil |
| Mengubah alur pendaftaran pasien di luar penanda pemilik manfaat | Milik Registrasi |
| Billing atau HR mengirim apa pun ke Accounting | Finance yang menerbitkan seluruh kejadian |
| Kasbon dan piutang internal tanpa kunjungan | Dibahas di pass terpisah (`FIN-OQ-085`) |

---

## 8. Dampak bila permintaan ini belum turun

| Permintaan | Yang tertahan | Yang tetap jalan |
|---|---|---|
| Billing (B1–B6) | Pembuatan piutang karyawan lewat serah terima; tab **Karyawan** pada Data Tagihan tetap kosong | Seluruh piutang penjamin dan pasien |
| Registrasi (R1–R6) | Sumber `BenefitOwnerId` untuk intake karyawan | Pendaftaran dan penjaminan seperti sekarang |
| HR (H1–H8) | Potong gaji otomatis, status bebas tanggungan, gerbang berhenti, dan koreksi potongan | Penggajian dan siklus kepegawaian seperti sekarang |
| Semua | `EPIC FIN-04` tetap `OPEN DECISION` dan tidak masuk gelombang pengiriman mana pun | MVP Finance lainnya |

Sementara menunggu, piutang pegawai dapat dicatat sebagai piutang penjamin biasa dengan keterangan tertulis
(`04-prd-to-mvp.md` bagian 8). Itu **tidak** dapat dikelompokkan per pegawai dan tidak memenuhi aturan potong gaji.

---

## 9. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` — pass 5 Oktober 2026 | `FIN-DEC-161`..`175`, `FIN-OQ-084`..`099`, kriteria penerimaan |
| `docs/module-blueprints/finance-management/01-existing-capability-map.md` bagian 21 | Bukti per kemampuan: `FIN-CAP-067`..`085`, `FIN-CQ-11`, `FIN-UQ-01`..`04` |
| `docs/module-blueprints/finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md` bagian 3 | Permintaan 20 September 2026 yang dilanjutkan berkas ini |
| `docs/module-blueprints/finance-management/04-prd-to-mvp.md` bagian 8 | Pengganti selama MVP berjalan |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` bagian 2 dan 3 | Bentuk kontrak serah terima |

---

## 10. Jawaban owner

Diisi saat jawaban diterima. **Belum ada jawaban.**

### 10.1 Billing dan Kasir

| Butir | Jawaban | Tanggal | Dicatat sebagai |
|---|---|---|---|
| B1 | — | — | — |
| B2 | — | — | — |
| B3 | — | — | — |
| B4 | — | — | — |
| B5 | — | — | — |
| B6 | — | — | — |

### 10.2 Registrasi

| Butir | Jawaban | Tanggal | Dicatat sebagai |
|---|---|---|---|
| R1 | — | — | — |
| R2 | — | — | — |
| R3 | — | — | — |
| R4 | — | — | — |
| R5 | — | — | — |
| R6 | — | — | — |

### 10.3 HR

| Butir | Jawaban | Tanggal | Dicatat sebagai |
|---|---|---|---|
| H1 | — | — | — |
| H2 | — | — | — |
| H3 | — | — | — |
| H4 | — | — | — |
| H5 | — | — | — |
| H6 | — | — | — |
| H7 | — | — | — |
| H8 | — | — | — |
