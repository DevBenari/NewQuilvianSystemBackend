# Rawat Inap — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RWI-BP-001` |
| Revision | `7` |
| Status | `draft` |
| Interview mode | Pembahasan ulang arsitektur frontend tuntas 2026-08-27, 5 keputusan. Sebelumnya `Amendment pass` tuntas 2026-08-24, 4 butir, atas tiga usulan lintas modul dari blueprint IGD. Sebelumnya `Amendment pass` tuntas 2026-08-21, 8 butir. Sebelumnya `Closure pass` 17 pertanyaan tuntas 2026-08-21, dan `Scope pass` 30 pertanyaan tuntas 2026-08-20 |
| Product/domain owner | **Muhammad Hamzah**, ditunjuk 2026-08-21 lewat `RWI-DEC-061`. Jabatan formal belum diisi |
| Clinical governance owner | **Sebagian terisi.** Keputusan isolasi dan jenis kelamin diambil pemilik pada `RWI-DEC-064`. Belum dinyatakan apakah penunjukan itu mencakup seluruh peran clinical governance |
| Security/privacy owner | `OPEN` — menjadi syarat sebelum produksi |
| Backend SHA | `45dcfa1` (branch `MHamzah`) |
| Frontend SHA | `dec4fdeff` |
| Capability map | **Sudah ada per 2026-08-21.** [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `1.0`, backend SHA `5afb54b`, frontend SHA `dec4fdeff`. Pemeriksaan duplikasi selesai; tidak ditemukan rencana tabel baru yang menduplikasi tabel existing |
| Primary evidence | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md`, status `TARGET PROPOSAL`, baseline commit `5103e68` |
| Tanggal pass | 2026-08-20 |

> **Peringatan sebelum membaca.** Dokumen ini adalah catatan keputusan wawancara, bukan desain
> dan bukan persetujuan. Semua baris berstatus `draft` masih bisa berubah. Tidak ada satu pun
> baris di sini yang boleh dipakai sebagai izin menulis source code.

---

## Ringkasan Status Pass — per 2026-08-20

Dokumen ini panjang. Bagian ini ada supaya pembaca tahu posisinya tanpa membaca seluruh isi.

| Hal | Jumlah | Keterangan |
|---|---:|---|
| Aturan bisnis tertulis | 37 | `RWI-RULE-001` sampai `RWI-RULE-037`, seluruhnya disertai contoh berangka |
| — di antaranya **belum final** | 3 | `RWI-RULE-021`, `RWI-RULE-025`, dan `RWI-RULE-037`. `RWI-RULE-012` sudah final sejak `RWI-DEC-064`, walaupun sebagiannya belum dapat dijalankan |
| Keputusan tercatat | 77 | 68 berstatus `approved`, 7 berstatus `draft`, 1 berstatus `closed`, 1 berstatus `superseded` penuh yaitu `RWI-DEC-018`. Satu keputusan `approved` juga bertanda `superseded` sebagian, yaitu `RWI-DEC-011` |
| Fakta yang terbukti dari repository dan PRD | 12 | `RWI-FACT-001` sampai `RWI-FACT-012`. Dua terakhir berasal dari capability map, bukan dari PRD |
| Acceptance criteria yang sudah dapat diuji | 149 | `RWI-AC-001` sampai `RWI-AC-149` |
| Keputusan yang didelegasikan ke pelaksana | 5 | `RWI-FE-001` s.d. `RWI-FE-005`, seluruhnya `DEV_DISCRETION` dengan batas mengikat yang tertulis |
| Konflik | **8 dari 8 tertutup** | Tujuh berasal dari PRD, satu ditemukan antar keputusan di dokumen ini sendiri |
| Lubang cakupan | 9 dari 11 tertutup | Dua sisanya sudah dijawab tetapi menunggu pemilik klinis |
| Pertanyaan wawancara tersisa | **0** | Scope Pass 30 pertanyaan tuntas 2026-08-20. Closure Pass 17 pertanyaan tuntas 2026-08-21. Amendment Pass 8 butir tuntas 2026-08-21. Yang tersisa bukan pertanyaan, melainkan tindakan organisasi dan penunjukan pemilik |
| Pertanyaan penutup capability map | **17 dari 17 tertutup** | `RWI-TRQ-001` sampai `RWI-TRQ-017`, ditutup pada Closure Pass 2026-08-21 |
| Konflik frontend–backend dari audit source | **3 dari 3 ditangani** | `RWI-CON-TRC-001` ditutup `RWI-DEC-049`. `RWI-CON-TRC-002` hanya soal penamaan dan tidak perlu diperbaiki. `RWI-CON-TRC-003` ditutup `RWI-DEC-041` |
| Butir terbuka yang tersisa | 6 | **Satu tindakan organisasi:** `RWI-OQ-034` persetujuan pemilik `EmergencyInstallationManagement`. **Tiga sudah dijawab tetapi menunggu pemilik klinis atau hukum:** `RWI-OQ-035`, `RWI-OQ-038`, `RWI-OQ-039`. **Dua keputusan implementasi nonblocking:** `RWI-OQ-045` hak akses konfirmasi masuk dan `RWI-OQ-046` jalur admisi backend tanpa `EncounterId`. Tidak satu pun memblokir desain; `RWI-OQ-034` hanya menahan `INP-S09`, sedangkan `RWI-OQ-045` dan `RWI-OQ-046` tidak menahan roadmap frontend revision `3` |
| Gerbang sebelum produksi | 3 gerbang tata kelola + 5 baris aturan klinis/privasi + 4 gerbang implementasi | Lihat bagian Gate Sebelum Produksi |

**Enam butir yang masih terbuka, dan tidak satu pun memblokir desain frontend revision `0.4`:**

| Butir | Isinya | Memblokir |
|---|---|---|
| `RWI-OQ-034` | Persetujuan pemilik `EmergencyInstallationManagement` atas serah terima IGD ke rawat inap. Pemiliknya **Rizki Gunawan** (`RWI-DEC-069`); jawabannya sudah tersedia pada `IGD-DEC-067` yang masih `draft` | `INP-S09`, di luar MVP |
| `RWI-OQ-035` | Berapa lama riwayat perubahan status disimpan. **Sudah dijawab** `RWI-DEC-060`, menunggu pemilik hukum | Slice berikutnya |
| `RWI-OQ-038` | Isi minimal serah terima klinis antar shift. **Scope sudah tertutup** `RWI-DEC-058`; isinya menunggu pemilik klinis | Sign-off |
| `RWI-OQ-039` | Aturan klinis pasien meninggal dan pasien kabur. **Sudah dijawab** `RWI-DEC-059`, menunggu pemilik klinis | Sign-off |
| `RWI-OQ-045` | Apakah kepala ruangan perlu `InpatientBedOccupancy : Create` untuk mengonfirmasi pasien masuk dari ruangan | Tidak menahan; `FE-RWI-030` mengikuti kontrak saat ini dan hanya merender aksi bagi petugas admisi serta supervisor |
| `RWI-OQ-046` | Apakah jalur `POST /episodes` tanpa `EncounterId`, yang menanam `PaymentType = Cash`, perlu ditutup | Tidak menahan; alur revision `0.4` selalu membuat kunjungan beserta penjaminnya lebih dulu |

**Butir organisasi ditutup pada 2026-08-21** lewat `RWI-DEC-061` sampai `RWI-DEC-066`: pemilik
berwenang ditunjuk, kepemilikan modul tetangga dinyatakan berada pada pemilik yang sama,
penanggung jawab pengisian data master ditetapkan beserta target tanggalnya, dan aturan isolasi
serta pemisahan jenis kelamin dikunci utuh. **Satu di antaranya dikoreksi pada 2026-08-24** lewat
`RWI-DEC-069`: modul `EmergencyInstallationManagement` ternyata dimiliki Rizki Gunawan, bukan
pemilik pada `RWI-DEC-061`, sehingga `RWI-OQ-034` terbuka kembali — kali ini dengan pemilik yang
bernama dan dengan jawaban yang sudah tersedia pada `IGD-DEC-067`.

Ketiga butir klinis dan hukum di atas **sudah dijawab**, tetapi jawabannya berada di area yang
pemiliknya belum ditunjuk sehingga belum dapat naik ke `approved`. Tidak satu pun memblokir desain
maupun implementasi MVP; yang tertahan olehnya hanya kesiapan melayani pasien sungguhan.
`RWI-OQ-034` juga tidak menahan MVP, karena `INP-S09` memang di luar MVP. `RWI-OQ-045` dan
`RWI-OQ-046` tidak menahan task frontend revision `3`; keduanya tetap perlu keputusan pemilik pada
scope backend/permission masing-masing.

**Yang sudah dikerjakan per 2026-08-21:** audit kemampuan existing lewat `/qv-trace` selesai.
Sembilan butir `RWI-TRC-001` sampai `RWI-TRC-009` terjawab seluruhnya, 44 kemampuan
diklasifikasi, tiga konflik frontend–backend dikonfirmasi, dan 17 pertanyaan penutup
`RWI-TRQ-001` sampai `RWI-TRQ-017` dikumpulkan. Hasilnya ada di
[`01-existing-capability-map.md`](./01-existing-capability-map.md).

**Closure Pass juga sudah selesai pada 2026-08-21.** Ketujuh belas pertanyaan penutup tertutup
seluruhnya lewat 13 pertanyaan wawancara, menghasilkan 14 keputusan baru (`RWI-DEC-038` sampai
`RWI-DEC-051`), sembilan aturan bisnis baru (`RWI-RULE-026` sampai `RWI-RULE-034`), dan 38
acceptance criteria baru. Empat blocker desain tertutup: ketergantungan antrean pada dokumentasi
klinis, sumber kebenaran penghunian tempat tidur, gerbang keuangan tanpa Billing operasional, dan
nasib kunjungan IGD saat pasien naik ke bangsal.

**Gerbang kelengkapan requirement dijalankan 2026-08-21** dan menghasilkan
[`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md)
revision `1.0`, dengan hasil keseluruhan `PARTIALLY_READY`. Delapan slice sudah boleh dirancang
arsitektur domainnya, dua sebagian, dan lima berhenti. Gerbang itu menemukan tiga gap baru yang
tidak terlihat pada Scope Pass maupun Closure Pass, tercatat sebagai `RWI-OQ-037` sampai
`RWI-OQ-039`.

**Arsitektur domain disusun 2026-08-21** dan menghasilkan
[`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md)
revision `0.1`, berstatus `draft`, dengan kesiapan `DOMAIN_ARCHITECTURE_PARTIAL`. Sembilan slice
dinyatakan siap dan berdiri sendiri, sehingga boleh diteruskan ke penyusunan blueprint.

**Blueprint modul disusun 2026-08-21** dan menghasilkan 13 berkas canonical beserta
[`blueprint-manifest.md`](./blueprint-manifest.md) revision `1`, seluruhnya berstatus `draft`.

**Amendment Pass dijalankan 2026-08-21 setelah blueprint jadi.** Delapan butir terbuka ditangani:
lima tertutup penuh, tiga dijawab tetapi menunggu pemilik klinis atau hukum. **Empat di antaranya
mengubah blueprint**, sehingga blueprint wajib naik revision sebelum dipakai:

| Keputusan | Yang berubah pada blueprint |
|---|---|
| `RWI-DEC-054` | Menambah invariant `INV-INP-10`, satu aturan validasi, dan satu unique index parsial |
| `RWI-DEC-055` | Melonggarkan `INV-INP-01`, menambah kolom waktu kepergian pada episode, satu perintah bisnis, satu nilai baru pada alasan berakhirnya penempatan, satu endpoint, dan mengubah tabel pasangan status pada `RWI-RULE-003` |
| `RWI-DEC-056` | Menambah satu kolom opsional rujukan episode ibu pada episode bayi |
| `RWI-DEC-057` | Menambah satu tabel salinan versi resume pulang |

`RWI-DEC-053` sengaja dipilih supaya **tidak** mengubah blueprint: riwayat lokasi tetap milik Rawat
Inap, sehingga catatan penempatan dan seluruh kontrak yang sudah disusun tetap berlaku.

**Blueprint sudah dinaikkan ke revision `2` pada 2026-08-21**, menyerap keempat perubahan Amendment
Pass. Penanda STALE pada manifest dicabut, dan seluruh contract naik ke `0.2.0`.

**Keempat pertanyaan memblokir sudah tertutup pada 2026-08-21** lewat `RWI-DEC-061` sampai
`RWI-DEC-064`. Tiga dari empat gerbang implementasi dicabut; yang tersisa hanya kesiapan data master
yang tertutup begitu datanya benar-benar terisi.

**Blueprint sudah dinaikkan ke revision `3` pada 2026-08-21**, menyerap `RWI-DEC-064` sampai
`RWI-DEC-066`. Satu epic baru `EPIC RI-34` lahir, `INP-S11` berpindah dari slice yang dihentikan
menjadi slice yang dirancang, dan seluruh contract naik ke `0.3.0`. Tidak ada tabel baru dan tidak
ada perubahan kolom pada tabel modul lain.

**Yang harus dikerjakan berikutnya, urut:**

1. `/hospital-domain-architect` untuk menyelaraskan arsitektur domain yang kini tertinggal tiga
   langkah — `INV-INP-10`, `CMD-INP-15`, `CMD-INP-16`, dan masuknya `INP-S11` ke dalam scope.
   **Tidak memblokir**, karena isi blueprint dan decision log sudah sejalan.
2. `/qv-plan` untuk menurunkan blueprint revision `3` menjadi task berukuran kecil.

Yang **belum** boleh dimulai adalah penulisan source code. Satu gerbang implementasi masih terbuka —
kesiapan data master, yang sejak revision `3` menuntut penandanya **benar**, bukan sekadar terisi —
ditambah dua pekerjaan yang bukan keputusan: perbaikan tombol tempat tidur dan kesiapan test
regresi.

**Yang belum boleh dikerjakan:** implementasi, migration, dan pekerjaan database. Modul
`InPatientManagement` masih berstatus `PLANNED` pada registry, dan menurut `RWI-FACT-002` status
itu hanya memberi hak penamaan.

---

## Scope dan Outcome

### Kalimat batas scope

> Modul Rawat Inap mengelola satu episode perawatan pasien menginap, mulai dari pasien
> diterima masuk (admisi) sampai episode ditutup dan tempat tidur kembali kosong, beserta
> pencatatan siapa yang merawat, di mana pasien berbaring, dan apa yang dikerjakan selama
> pasien dirawat.

### Outcome yang diharapkan PRD

Satu pasien dapat menjalani rangkaian berikut tanpa ada petugas yang harus mengubah database
secara manual:

`Admisi → penempatan bed → masuk daftar pasien dirawat (census) → penugasan perawat →
pengkajian awal → dokumentasi dokter dan perawat → resep → pindah bed → keputusan pulang →
resume pulang → pemeriksaan kelengkapan (clearance) → penutupan episode → bed kembali kosong.`

### Di dalam scope — DIKONFIRMASI 2026-08-20

Daftar ini disusun dari PRD bagian 6.1 dan bagian 23, bukan dari penilaian agent tentang
kelengkapan sistem rumah sakit. Dikonfirmasi pada wawancara pertanyaan 1, lihat
`RWI-DEC-004`.

| No | Kemampuan | Rujukan PRD |
|---:|---|---|
| 1 | Memilih pasien yang sudah terdaftar untuk dirawat inap | CAP-002 |
| 2 | Menentukan penjamin atau cara bayar saat masuk | CAP-003 |
| 3 | Menentukan DPJP (dokter penanggung jawab pelayanan) | CAP-004 |
| 4 | Mencari tempat tidur yang tersedia | CAP-005 |
| 5 | Mengunci dan menempatkan pasien ke tempat tidur, lalu mengaktifkan episode | CAP-006 |
| 6 | Daftar pasien yang sedang dirawat beserta lokasinya (census) | CAP-008 |
| 7 | Penugasan perawat penanggung jawab | CAP-011 |
| 8 | Pengkajian awal pasien oleh perawat | CAP-012 |
| 9 | Catatan dan tindakan keperawatan dasar | CAP-014 |
| 10 | Pindah kamar atau pindah tempat tidur | CAP-017 |
| 11 | Dokumentasi dokter bentuk SOAP | CAP-020 |
| 12 | CPPT (Catatan Perkembangan Pasien Terintegrasi) | CAP-021 |
| 13 | Kajian dokter | CAP-022 |
| 14 | Resep untuk pasien rawat inap | CAP-023 |
| 15 | Tindakan dokter | CAP-024 |
| 16 | Pencatatan visite dokter | CAP-025 |
| 17 | Resume medis atau resume pulang | CAP-026 |
| 18 | Penutupan episode dan pelepasan tempat tidur | CAP-028 |

### Di luar scope — untuk modul lain — DIKONFIRMASI 2026-08-20

| Kemampuan | Pemilik atau alasan | Rujukan |
|---|---|---|
| Daftar tunggu masuk rawat inap yang rumit | Ditunda setelah MVP | CAP-001 |
| Cetak kartu, gelang, dan label pasien | Ditunda setelah MVP | CAP-007 |
| Paket dokumen serah terima, edukasi, dan privasi lengkap | Ditunda; sementara memakai Patient Consent | CAP-009 |
| Deposit, estimasi biaya, dan cek manfaat penjamin | Menunggu domain Billing operasional | CAP-010 |
| Rencana asuhan keperawatan SDKI penuh | Ditunda setelah MVP | CAP-013 |
| Pemeriksaan penunjang ujung-ke-ujung (lab, radiologi) | Modul Laboratory dan Radiology, keduanya `PLANNED` di registry | CAP-015 |
| Pencatatan pemakaian alat | Ditunda setelah MVP | CAP-016 |
| Booking operasi | Modul `OperatingRoomManagement`, berstatus `PLANNED` | CAP-018 |
| Running bill atau tagihan berjalan penuh | Modul `BillingManagement` | CAP-019 |
| Asuhan gizi penuh | Modul Gizi | CAP-027 |
| Mesin farmasi: penyiapan, peracikan, review obat | Modul `PharmacyManagement`, berstatus `ACTIVE` | EPIC RI-07 |
| Buku besar keuangan, invoice, pembayaran, klaim asuransi | `BillingManagement` dan `InsuranceManagement` | PRD bagian 15 |
| Medication Administration Record dan pemberian obat di samping tempat tidur | Dinyatakan di luar MVP oleh PRD | EPIC RI-07 |
| Pasien titipan: kelas hak yang tersimpan terpisah dari kelas kamar | Dikeluarkan dari MVP lewat `RWI-DEC-019`. Keringanan biaya diurus petugas billing di luar sistem ini | `RWI-GAP-004` |

### Cara menangani 11 lubang cakupan — DIKONFIRMASI 2026-08-20

Pemilik kebutuhan memilih **tidak menambah item MUST**. Sebelas lubang cakupan diperlakukan
sebagai aturan yang hilang dari kemampuan yang sudah ada di dalam scope, bukan sebagai fitur
baru. Pemetaannya sebagai berikut.

| Lubang | Ditempelkan ke kemampuan | Penjelasan sederhana |
|---|---|---|
| `RWI-GAP-001` cara pulang lain | CAP-026 dan CAP-028 | Pulang atas permintaan sendiri, dirujuk, meninggal, dan kabur tetap merupakan cara menutup episode dan melepas tempat tidur, hanya syaratnya berbeda |
| `RWI-GAP-002` pasien meninggal | CAP-026 dan CAP-028 | Sama seperti di atas, dengan pencatat dan dokumen yang berbeda |
| `RWI-GAP-003` pindah kelas perawatan | CAP-017 | Naik atau turun kelas adalah bentuk lain dari pindah lokasi |
| `RWI-GAP-004` pasien titipan | CAP-006 dan CAP-017 | Penempatan pasien di kamar yang bukan haknya adalah penempatan bed dengan penanda tambahan |
| `RWI-GAP-005` jenis kelamin dan isolasi | CAP-005 dan CAP-006 | Menentukan apakah aturan ini menolak penempatan, bukan sekadar menyaring pencarian |
| `RWI-GAP-006` batas waktu pengkajian dan verifikasi CPPT | CAP-012 dan CAP-021 | Aturan waktu dan tanda tangan pada dokumen yang sudah masuk scope |
| `RWI-GAP-007` definisi visite | CAP-025 | Melengkapi kemampuan yang sudah MUST tetapi belum punya definisi |
| `RWI-GAP-008` cara hitung lama dirawat | CAP-008 | Aturan tampilan pada census yang sudah masuk scope |
| `RWI-GAP-009` obat pulang | CAP-023 dan CAP-026 | Jenis resep pada kemampuan resep yang sudah masuk scope |
| `RWI-GAP-010` pembatalan admisi | CAP-006 | Jalur batal dari penempatan bed yang sudah masuk scope |
| `RWI-GAP-011` bayi baru lahir rawat gabung | CAP-002 dan CAP-006 | Kasus khusus pemilihan pasien dan penempatan bed |

Konsekuensi yang disadari: jumlah pertanyaan aturan bertambah, dan tahap MVP-5 (pulang dan
penutupan) menjadi bagian terberat pada rencana pengerjaan.

### Titik sentuh dengan modul tetangga

Modul tetangga hanya dibahas sebatas titik sentuhnya. Aturan internal modul tetangga tidak
digali dalam wawancara ini.

| Modul tetangga | Titik sentuh yang perlu dikunci |
|---|---|
| IGD (`EmergencyInstallationManagement`) | Bagaimana keputusan "rawat inap" dari IGD menjadi admisi rawat inap. Blueprint IGD sudah ada dan berstatus `approved sebagian` |
| Registrasi (`RegistrationManagement`) | `TrxPatientEncounter` sebagai jangkar episode |
| Farmasi (`PharmacyManagement`) | Resep dikirim dengan konteks pasien dan encounter; status resep dibaca balik |
| Billing (`BillingManagement`) | Hanya status kelayakan keuangan: `Pending`, `Cleared`, `Blocked` |
| Clinical (`ClinicalManagement`) | Pengkajian, vital sign, diagnosis, tindakan, CPPT, consent |
| Master data (`Mst`) | Bed, room, service unit, kelas pasien, dokter, perawat |

---

## Fakta Repository yang Sudah Terbukti

| ID | Fakta | Bukti |
|---|---|---|
| `RWI-FACT-001` | Registry sudah memuat entri `HealthServices / InPatientManagement / Inpatient`, prefix `Inp`, lifecycle `PLANNED`. Artinya hak penamaan sudah ada, tetapi izin implementasi belum | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 20 |
| `RWI-FACT-002` | Persetujuan registry hanya memberi hak penamaan dan kepemilikan. Registry secara eksplisit **tidak** memberi izin implementasi, migration, pekerjaan database, deployment, atau aktivasi modul berstatus `PLANNED` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` paragraf 3 |
| `RWI-FACT-003` | Entity operasional baru wajib bernama `<PrefixPemilik><KonsepBisnis>`, contohnya `RegPatientEncounter` dan `EmgVisit`. Untuk modul ini berarti berawalan `Inp` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, penjelasan setelah tabel |
| `RWI-FACT-004` | Backend aktif berada pada branch `MHamzah` commit `45dcfa1`, sedangkan PRD ditulis di atas baseline `5103e68`. Baseline PRD sudah tidak sama dengan kondisi sekarang | `git rev-parse` pada kedua repository |
| `RWI-FACT-010` | Kata "visite" hanya muncul satu kali di seluruh PRD, yaitu baris 149 pada tabel kemampuan, sebagai baris "Visit dokter" dengan kode CAP-025 berprioritas MUST. Tidak ada Functional Requirement, EPIC, definisi, maupun baris kewenangan yang menyebutnya. Entity `DoctorConsultation` disebut terpisah pada baris 685 dengan disposisi `EXTEND`, tanpa dikaitkan ke CAP-025 | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 149 dan 685 |
| `RWI-FACT-009` | FR-RI-010 menyebut `gender compatibility jika digunakan` dan `isolation` hanya sebagai pilihan **penyaring pencarian** bed. FR-RI-011, satu-satunya aturan yang menentukan bed boleh dipilih atau tidak, hanya menyebut `IsActive = true` dan status ketersediaan; jenis kelamin dan isolasi tidak disebut sama sekali. Dibaca harfiah, PRD mengizinkan pasien laki-laki dan perempuan sekamar | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 284-300 |
| `RWI-FACT-008` | Baris `Financial clearance` pada tabel kewenangan PRD bagian 14 hanya mencentang dua peran: Billing dan Supervisor. Artinya Supervisor sudah memegang kewenangan atas kelayakan keuangan menurut PRD sendiri, bukan kewenangan baru | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 769 |
| `RWI-FACT-007` | PRD memakai tiga rumusan berbeda untuk gerbang keuangan. Baris 103 menulis clearance "telah terpenuhi **atau dikonfirmasi** melalui integration contract", baris 571 menulis "status financial clearance **tersedia** dari Billing/Kasir", sedangkan bagian 15 mendefinisikan tiga nilai status `Pending`, `Cleared`, dan `Blocked`. Kata "tersedia" dan "terpenuhi" bukan hal yang sama | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 103, 571, dan 789 |
| `RWI-FACT-006` | Baris `Transfer` pada tabel kewenangan PRD bagian 14 berbunyi: Admisi kosong, Kepala Perawat centang, Perawat centang, Dokter/DPJP ditulis "sesuai SOP", Billing kosong, Supervisor centang. Perawat pelaksana mendapat centang penuh, sedangkan Dokter/DPJP tidak | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 767 |
| `RWI-FACT-005` | Kata `InCare` hanya muncul dua kali di seluruh PRD, yaitu baris 613 dan baris 1088. Keduanya hanya berupa kotak pada diagram alur. Tidak ada satu pun Functional Requirement, definisi, pemicu perpindahan, maupun baris kewenangan yang menyebut `InCare` | Pencarian kata pada `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` |
| `RWI-FACT-011` | Pengkajian, konsultasi dokter, diagnosis, tindakan, dan resep **mewajibkan antrean atau konsultasi** yang benar-benar ada. Pasien rawat inap tidak punya antrean, sehingga tanpa penyesuaian lima jenis catatan itu tidak dapat ditulis sama sekali | `01-existing-capability-map.md` revision `1.1` bagian `RWI-TRC-004`; backend `5afb54b` `.../PatientAssessmentController.cs:265-267`, `.../DoctorConsultationController.cs:206` dan `:255-258`, `.../PrescriptionController.cs:278-281` |
| `RWI-FACT-012` | Satu kunjungan hanya boleh punya **satu konsultasi dokter**, dan satu konsultasi hanya boleh punya **satu resep aktif**. Penjaga konsultasi memeriksa `EncounterId`, bukan `QueueId`, sehingga membuat antrean semu tidak dapat melewatinya. Akibatnya perawatan lima hari hanya boleh punya satu catatan konsultasi dan satu resep aktif | `01-existing-capability-map.md` revision `1.1` fakta `RWI-TF-026` dan `RWI-TF-027`; backend `5afb54b` `.../DoctorConsultationController.cs:809-815`, `.../PrescriptionController.cs:575` dan `:578-581` |

---

## Konflik yang Ditemukan pada PRD

Konflik berikut ditemukan agent saat membaca PRD. Konflik ini **tidak boleh diputus sendiri
oleh agent**; semuanya masuk antrean wawancara atau antrean audit source.

**Status per 2026-08-20: kedelapan konflik sudah tertutup.** Tujuh berasal dari PRD;
`RWI-CON-008` ditemukan pada wawancara pertanyaan 18 sebagai tabrakan antar dua keputusan di
dalam dokumen ini sendiri, lalu ditutup pada pertanyaan 19. Kolom Status pada tabel di bawah
menyebutkan keputusan mana yang menutup tiap baris.

| ID | Konflik | Letak | Status |
|---|---|---|---|
| `RWI-CON-001` | Model status episode tidak konsisten. Bagian 10 menulis `Draft → Admitted → InCare → DischargePending → Closed`, sedangkan bagian 24 Contract A menulis `Admission → InCare → DischargePending → Closed` tanpa `Draft` dan dengan nama berbeda | PRD baris 608-618 vs 1086-1091 | `TERTUTUP` oleh `RWI-DEC-009` |
| `RWI-CON-002` | Status tempat tidur tidak konsisten. Bagian 11 mewajibkan `Reserved` sebelum `Occupied`, tetapi OQ-RI-002 justru masih menanyakan apakah `Reserved` diperlukan. FR-RI-013 dan FR-RI-014 hanya menyebut `Occupied` dan `Available` | PRD baris 634-640, 1052, 311-318 | `TERTUTUP` oleh `RWI-DEC-007` |
| `RWI-CON-003` | Kewenangan transfer bertabrakan. Tabel kewenangan bagian 14 sudah memberi tanda centang kepada Kepala Perawat dan Perawat, tetapi OQ-RI-004 masih menanyakan siapa yang berwenang final | PRD baris 767 vs 1054 | `TERTUTUP` oleh `RWI-DEC-012` |
| `RWI-CON-004` | Kewenangan penutupan episode bertabrakan. Tabel bagian 14 menulis "sesuai SOP" untuk Admisi dan Dokter serta centang untuk Supervisor, tetapi OQ-RI-006 masih menanyakan siapa yang mengeksekusi penutupan | PRD baris 770 vs 1056 | `TERTUTUP` oleh `RWI-DEC-016` |
| `RWI-CON-005` | Gerbang keuangan ambigu. EPIC RI-10 hanya menuntut "status financial clearance tersedia", yang secara harfiah tetap lolos walaupun statusnya `Blocked`. OQ-RI-008 masih menanyakan apakah gerbang ini memblokir atau sekadar peringatan | PRD baris 570-571 vs 1058 | `TERTUTUP` oleh `RWI-DEC-015` |
| `RWI-CON-006` | Nama entity usulan PRD (`InpatientEpisode`, `InpatientBedAssignment`, dan seterusnya) tidak mengikuti aturan penamaan registry yang mewajibkan prefix `Inp`. PRD sendiri menyatakan nama final mengikuti governance backend, jadi registry yang menang | PRD baris 691-700 vs `RWI-FACT-003` | `TERTUTUP` oleh `RWI-DEC-002` |
| `RWI-CON-008` | Bertabrakan antar keputusan di dokumen ini sendiri. `RWI-DEC-012` menjawab `OQ-RI-005` dengan menyatakan penerimaan unit tujuan **tidak wajib** dan perpindahan berjalan satu langkah. Namun `RWI-DEC-022` menyebut DPJP menimbang "kesiapan unit tujuan" saat memindahkan pasien. Belum jelas apakah kesiapan itu diperiksa sistem atau semata pertimbangan DPJP sendiri | `RWI-DEC-012` vs `RWI-DEC-022` | `TERTUTUP` oleh `RWI-DEC-023` |
| `RWI-CON-007` | Urutan transfer atomik pada EPIC RI-09 menutup penempatan bed lama sebelum menempati bed baru, sedangkan invariant INV-02 mensyaratkan satu episode selalu punya satu bed aktif. Titik tengah transaksi berpotensi melanggar invariant bila dibaca harfiah | PRD baris 512-530 vs 656 | `TERTUTUP` oleh `RWI-DEC-014` |

---

## Lubang Cakupan yang Ditemukan Agent

Hal berikut tidak dibahas PRD sama sekali, padahal berada di dalam batas scope yang diusulkan
dan berpotensi memblokir desain.

| ID | Lubang | Kenapa penting | Status |
|---|---|---|---|
| `RWI-GAP-001` | PRD hanya mengenal satu cara pulang, yaitu pasien diizinkan pulang oleh DPJP. Tidak ada pulang atas permintaan sendiri, dirujuk ke rumah sakit lain, pasien meninggal, dan pasien kabur | Keempat kondisi itu tetap harus melepas tempat tidur dan menutup episode, tetapi gerbangnya berbeda. Tanpa ini petugas akan mencari jalan pintas | `TERTUTUP` oleh `RWI-DEC-017` |
| `RWI-GAP-002` | Tidak ada aturan pasien meninggal: siapa yang mencatat, apakah resume pulang tetap wajib, dan kapan bed dilepas | Menyangkut rekam medis dan pelaporan wajib | `TERTUTUP` oleh `RWI-DEC-017` |
| `RWI-GAP-003` | Tidak ada aturan pindah kelas perawatan (naik kelas atau turun kelas), padahal transfer hanya digambarkan sebagai pindah bed | Pindah kelas punya akibat biaya; datanya harus tercatat sebagai perubahan kelas, bukan sekadar pindah bed | `TERTUTUP` oleh `RWI-DEC-013` |
| `RWI-GAP-004` | Tidak ada aturan pasien titipan, yaitu pasien yang dirawat di kelas atau ruang yang bukan haknya karena kamar penuh | Sangat lazim di rumah sakit Indonesia dan memengaruhi census, kelas, dan biaya | `TERTUTUP` oleh `RWI-DEC-019` — di luar MVP |
| `RWI-GAP-005` | Aturan pemisahan jenis kelamin dan isolasi hanya ditulis sebagai penyaring pencarian yang opsional ("gender compatibility jika digunakan"), bukan sebagai aturan keras | Bila hanya penyaring, sistem tetap mengizinkan laki-laki dan perempuan satu kamar | `DIJAWAB` oleh `RWI-DEC-018`, menunggu pemilik klinis dan privasi |
| `RWI-GAP-006` | Tidak ada batas waktu pengkajian awal keperawatan dan tidak ada aturan verifikasi CPPT oleh DPJP | Keduanya kewajiban akreditasi dan biasanya diaudit | `DIJAWAB` oleh `RWI-DEC-029`, menunggu pemilik klinis |
| `RWI-GAP-007` | "Visite dokter" masuk daftar MUST (CAP-025) tetapi tidak punya satu pun Functional Requirement yang mendefinisikannya | Tidak jelas apa yang dianggap satu visite dan siapa yang mencatatnya | `TERTUTUP` oleh `RWI-DEC-025` |
| `RWI-GAP-008` | Cara menghitung lama dirawat (LOS) hanya disebut "berdasarkan admission time" | Hitungan hari rawat berbeda antara selisih jam dan hitungan hari kalender; angkanya dipakai pihak lain | `TERTUTUP` oleh `RWI-DEC-027` |
| `RWI-GAP-009` | Tidak ada aturan obat pulang | Resep obat pulang berbeda perlakuan dari resep harian dan biasanya menjadi bagian gerbang pulang | `TERTUTUP` oleh `RWI-DEC-033` |
| `RWI-GAP-010` | Pembatalan admisi (`Draft/Admitted → Cancelled`) disebut ada, tetapi tidak ada aturan siapa yang boleh membatalkan, apakah alasan wajib, dan apa yang terjadi pada bed | Pembatalan yang tidak melepas bed membuat kamar terlihat penuh padahal kosong | `TERTUTUP` oleh `RWI-DEC-010` |
| `RWI-GAP-011` | Tidak ada aturan bayi baru lahir yang dirawat gabung dengan ibunya, walaupun OQ-RI-010 menyinggungnya | Bayi biasanya perlu episode sendiri tetapi menempati boks di kamar ibu | `TERTUTUP` oleh `RWI-DEC-020` |

---

## Pertanyaan yang Harus Dijawab Source Code, Bukan Manusia

Butir berikut sengaja **tidak** ditanyakan kepada pemilik kebutuhan. Semuanya diteruskan ke
`/qv-trace` (`/trace-existing-capabilities`).

| ID | Yang harus dibuktikan dari source |
|---|---|
| `RWI-TRC-001` | Apakah `MstBed` benar sudah punya `BedStatus`, `IsReservable`, penyaring room/service unit/patient class, dan ringkasan Available/Occupied seperti klaim EPIC RI-02 |
| `RWI-TRC-002` | Apakah `PatientEncounterController` benar memaksa kelas pasien `"RAWAT JALAN"` seperti klaim PRD bagian 2. **Naik menjadi prasyarat** sejak `RWI-DEC-011`, karena jalur pasien datang langsung menuntut pembuatan kunjungan bertipe rawat inap |
| `RWI-TRC-003` | Bentuk nyata `TrxPatientEncounter`: status, relasi lokasi, dan apakah sudah menyimpan riwayat lokasi |
| `RWI-TRC-004` | Apakah `PatientAssessment`, `PatientVitalSign`, `PatientDiagnosis`, `PatientProcedure`, CPPT, `PatientConsent`, dan `Prescription` benar sudah terhubung ke `EncounterId`. Sejak `RWI-DEC-025`, bentuk CPPT dokter juga menentukan bentuk laporan visite, karena visite dibaca dari catatan itu dan bukan dari data tersendiri |
| `RWI-TRC-005` | Apakah `BillingManagement` benar baru punya `MasterData` saja |
| `RWI-TRC-006` | Apakah master bed, room, service unit, dan kelas pasien sudah terisi data, karena Definition of Done melarang manipulasi database manual. Sejak `RWI-DEC-020`, pemeriksaan ini juga harus mencakup **boks bayi** sebagai tempat tidur |
| `RWI-TRC-007` | Pola permission yang dipakai repository, agar NFR-004 bisa dipetakan ke permission nyata |
| `RWI-TRC-008` | Apa yang sudah dihasilkan modul IGD saat disposition "rawat inap", karena itu jalur masuk paling lazim |
| `RWI-TRC-009` | Apakah sudah ada mekanisme audit perubahan status yang bisa dipakai ulang untuk NFR-003 |

---

## Glossary — draft

| Istilah | Makna kerja saat ini | Status |
|---|---|---|
| Episode rawat inap | Satu rangkaian perawatan menginap dari pasien diterima masuk sampai episode ditutup, berpusat pada satu encounter | `draft` |
| Admisi | Proses menerima pasien untuk dirawat inap, termasuk memilih penjamin, DPJP, kelas, dan tempat tidur | `draft` |
| Census | Daftar pasien yang sedang dirawat inap beserta lokasi dan penanggung jawabnya | `draft` |
| Bed assignment | Catatan penempatan satu pasien pada satu tempat tidur, beserta kapan mulai dan kapan berakhir | `draft` |
| Pemesanan tempat tidur (`Reserved`) | Tempat tidur yang sudah dikunci untuk satu calon pasien tetapi pasiennya belum berbaring di sana. Berlaku 2 jam, lalu gugur sendiri | `approved` lewat `RWI-DEC-007` dan `RWI-DEC-008` |
| Transfer | Perpindahan lokasi pasien di dalam episode yang sama; tidak membuat episode baru | `draft` |
| Rencana pulang (`DischargePending`) | Keadaan ketika pasien sudah diputuskan boleh pulang, tetapi episodenya belum ditutup dan tempat tidurnya belum dilepas | `approved` lewat `RWI-DEC-009` |
| Clearance | Pemeriksaan bahwa syarat administrasi dan keuangan sudah terpenuhi sebelum episode ditutup | `draft`; sifat memblokir atau tidak masih terbuka |
| Closure | Penutupan episode yang sekaligus melepas tempat tidur | `draft` |
| Reopen | Membuka kembali episode yang sudah ditutup, semata untuk membetulkan catatannya. Tidak mengembalikan tempat tidur dan tidak melanjutkan perawatan | `approved` lewat `RWI-DEC-028` |
| DPJP | Dokter Penanggung Jawab Pelayanan, dokter yang bertanggung jawab atas perawatan pasien | `draft` |
| CPPT | Catatan Perkembangan Pasien Terintegrasi, satu lembar catatan yang diisi bersama oleh dokter, perawat, dan tenaga kesehatan lain | `draft` |

---

## Aturan Bisnis yang Sudah Dikunci

Bagian ini hanya memuat aturan yang **sudah diputuskan** pemilik kebutuhan. Aturan yang masih
ditanyakan tidak boleh ditulis di sini.

### `RWI-RULE-001` — Tempat tidur dipesan dulu, baru ditempati

Dasar keputusan: `RWI-DEC-007`. Menutup `OQ-RI-002` dan `RWI-CON-002`.

Satu tempat tidur berjalan melalui keadaan berikut:

`Available (kosong) -> Reserved (dipesan) -> Occupied (ditempati) -> Available lagi`

Arti tiap keadaan dengan bahasa sehari-hari:

1. **Available** — tempat tidur benar-benar kosong dan boleh dipilih petugas mana pun.
2. **Reserved** — tempat tidur sudah dikunci atas nama satu calon pasien, tetapi pasiennya
   belum berbaring di sana. Petugas lain **tidak boleh** memilih tempat tidur ini.
3. **Occupied** — pasien sudah benar-benar menempati tempat tidur tersebut dan namanya muncul
   pada daftar pasien dirawat (census).
4. Kembali **Available** ketika episode ditutup, pasien dipindahkan ke tempat tidur lain, atau
   pemesanan gugur.

Jalur batal: `Reserved -> Available`, terjadi bila petugas membatalkan pemesanan atau
pemesanan gugur karena lewat waktu.

Alasan aturan ini ada: tanpa `Reserved`, dua petugas admisi yang bekerja pada waktu hampir
bersamaan bisa memilih tempat tidur yang sama, dan baru ketahuan bentrok saat pasien sudah
diantar ke kamar.

### `RWI-RULE-002` — Pemesanan tempat tidur gugur sendiri setelah 2 jam

Dasar keputusan: `RWI-DEC-008`. Menutup `OQ-RI-003`.

Isi aturannya:

| Butir | Ketentuan |
|---|---|
| Lama berlaku | 2 jam terhitung sejak pemesanan dibuat |
| Berlaku untuk siapa | Semua pemesanan, tanpa membedakan unit, kelas kamar, maupun asal pemesanan (IGD, poliklinik, atau admisi terencana) |
| Sifat angka 2 jam | Disimpan sebagai parameter yang boleh diubah admin lewat pengaturan, tanpa perlu mengubah program |
| Cara menghitung kedaluwarsa | Dihitung saat data dibaca, yaitu `waktu_pemesanan + 2 jam` dibandingkan dengan waktu sekarang. Tidak ada program penjadwal yang berjalan di latar belakang |
| Akibat saat gugur | Tempat tidur langsung terbaca `Available` lagi oleh siapa pun yang membuka pencarian tempat tidur |

Contoh nyata:

> Pukul **09:15**, petugas admisi memesan bed `MELATI-03` untuk Ibu Sari. Bed itu langsung
> terbaca `Reserved` dan hilang dari hasil pencarian tempat tidur kosong.
>
> Pukul **10:40**, petugas lain membuka pencarian tempat tidur. `MELATI-03` masih `Reserved`
> karena batas waktunya jatuh pukul 11:15, jadi bed itu tetap tidak bisa dipilih.
>
> Pukul **11:20**, petugas lain membuka pencarian lagi. Sistem menghitung 09:15 + 2 jam =
> 11:15, yang sudah lewat, sehingga `MELATI-03` langsung terbaca `Available` dan boleh
> dipesan pasien lain. Tidak ada petugas yang perlu menekan tombol apa pun untuk
> membebaskannya.

Alasan memilih satu angka seragam, bukan angka berbeda per asal pemesanan: daftar tunggu
masuk rawat inap (CAP-001) berada **di luar scope**, sehingga `Reserved` di sini hanya
berfungsi sebagai kunci sementara selama proses admisi berjalan, bukan sebagai pemesanan
jauh hari. Satu angka membuat aturannya mudah dijelaskan ke petugas dan mudah diaudit.

Yang **belum** diputuskan dan sengaja tidak ditulis sebagai aturan: apa yang terjadi bila
petugas menyelesaikan admisi setelah pemesanannya gugur dan tempat tidur itu sudah diambil
pasien lain. Butir ini dicatat sebagai `RWI-OQ-024`.

### `RWI-RULE-003` — Model status episode rawat inap

Dasar keputusan: `RWI-DEC-009`. Menutup `RWI-CON-001`.

Alur utamanya:

`Draft -> Admitted -> DischargePending -> Closed`

Jalur batal: `Draft -> Cancelled` dan `Admitted -> Cancelled`. Siapa yang boleh membatalkan
dan sampai kapan masih ditanyakan, lihat `RWI-OQ-022`.

Arti tiap keadaan, dan pasangannya dengan tempat tidur:

| Status episode | Artinya dengan bahasa sehari-hari | Tempat tidur saat itu | Muncul di census? |
|---|---|---|---|
| `Draft` | Petugas sedang menyiapkan admisi. Penjamin, DPJP, dan tempat tidur sedang dipilih. Pasien belum tentu ada di kamar | `Reserved`, berlaku 2 jam | Tidak |
| `Admitted` | Pasien sudah benar-benar menempati tempat tidur. Perawatan berjalan, dokumentasi boleh ditulis | `Occupied` | Ya |
| `DischargePending` | Pasien sudah diputuskan boleh pulang, tetapi episodenya belum ditutup | Masih `Occupied` | Ya |
| `Closed` | Episode ditutup dan tempat tidur dilepas | Kembali `Available` | Tidak |
| `Cancelled` | Admisi dibatalkan dan tidak jadi berjalan | Kembali `Available` | Tidak |

Contoh nyata:

> Pukul **09:15** petugas admisi membuka admisi Ibu Sari, memilih penjamin BPJS dan DPJP
> dr. Andi, lalu memesan bed `MELATI-03`. Episode berstatus `Draft`, bed `Reserved`.
>
> Pukul **09:50** Ibu Sari sampai di kamar dan berbaring di `MELATI-03`. Petugas menekan
> konfirmasi masuk. Episode menjadi `Admitted`, bed menjadi `Occupied`, dan nama Ibu Sari
> muncul di daftar pasien dirawat. Sejak detik ini perawat boleh mengisi pengkajian awal dan
> dokter boleh menulis instruksi maupun resep.
>
> Hari keempat pukul **08:30** dr. Andi menyatakan Ibu Sari boleh pulang. Episode menjadi
> `DischargePending`. Bed `MELATI-03` **belum** dilepas karena Ibu Sari masih menunggu obat
> pulang dan urusan administrasi.
>
> Pukul **13:10** semua syarat selesai dan episode ditutup. Episode menjadi `Closed`, bed
> `MELATI-03` kembali `Available` dan boleh dipesan pasien berikutnya.

### Kenapa `InCare` dibuang

`RWI-FACT-005` membuktikan `InCare` hanya muncul dua kali di PRD dan keduanya sekadar kotak
pada diagram. Tidak ada definisi, tidak ada pemicu perpindahan, dan tidak ada baris kewenangan.

Informasi yang biasanya diwakili `InCare` sudah tersimpan di tempat lain:

- "pasien ini sudah dikaji atau belum" dibaca dari ada tidaknya catatan pengkajian awal
  (CAP-012);
- "dokter sudah visite atau belum" dibaca dari catatan visite (CAP-025).

Menyimpan fakta yang sama di dua tempat membuat keduanya berpotensi tidak cocok, dan petugas
akan bingung mana yang benar. Bila nanti dibutuhkan cara cepat melihat pasien yang belum
dikaji, itu ditambahkan sebagai penyaring pada layar census, bukan sebagai status episode baru.

Ditolak secara sadar: menjadikan status episode sebagai syarat sebelum dokumentasi klinis
boleh ditulis. Pasien yang masuk malam hari bisa tertahan berjam-jam sebelum dokter boleh
menulis instruksi, dan itu mendorong petugas mencari jalan pintas.

### Akibat pada PRD

Contract A pada PRD bagian 24 (`Admission -> InCare -> DischargePending -> Closed`) sudah
tidak berlaku. Bagian 10.1 juga perlu dikoreksi karena masih memuat `InCare`. PRD tidak
diubah oleh sesi wawancara ini; koreksinya menjadi pekerjaan terpisah pada pemilik PRD.


### `RWI-RULE-004` — Pembatalan admisi

Dasar keputusan: `RWI-DEC-010`. Menutup `RWI-OQ-022` dan `RWI-GAP-010`.

Pembatalan adalah cara mengakhiri admisi yang **tidak jadi berjalan**. Pembatalan berbeda dari
pemulangan: pemulangan dipakai ketika pasien memang sudah dirawat lalu selesai.

| Keadaan episode | Siapa yang boleh membatalkan | Syarat | Akibat pada tempat tidur |
|---|---|---|---|
| `Draft` | Petugas admisi yang membuat admisi itu | Alasan wajib diisi | `Reserved` kembali menjadi `Available` |
| `Admitted`, belum ada catatan klinis sama sekali | Hanya supervisor atau kepala ruangan | Alasan wajib diisi, dan sistem memastikan tidak ada satu pun catatan klinis | `Occupied` kembali menjadi `Available` |
| `Admitted`, sudah ada catatan klinis | Tidak ada yang boleh | — | Tidak boleh dibatalkan. Episode hanya bisa diakhiri lewat penutupan biasa |
| `DischargePending` dan `Closed` | Tidak ada yang boleh | — | Tidak boleh dibatalkan |

Yang dihitung sebagai **catatan klinis** untuk pemeriksaan di atas:

1. pengkajian awal keperawatan (CAP-012);
2. catatan dan tindakan keperawatan (CAP-014);
3. CPPT (CAP-021);
4. resep rawat inap (CAP-023);
5. tindakan dokter (CAP-024);
6. tanda vital pasien.

Setiap pembatalan wajib menyimpan tiga hal: **siapa** yang membatalkan, **kapan**, dan **apa
alasannya**. Data pembatalan tidak dihapus, hanya ditandai dibatalkan, sehingga masih bisa
ditelusuri saat diaudit.

Pelepasan tempat tidur adalah **bagian dari** pembatalan, bukan langkah terpisah yang
dikerjakan petugas sesudahnya. Bila tempat tidur gagal dilepas, pembatalannya ikut gagal dan
episode tetap seperti semula. Aturan ini menjawab risiko pada `RWI-GAP-010`, yaitu kamar yang
terlihat penuh padahal pasiennya tidak pernah ada.

Contoh nyata:

> Pukul **10:02** petugas admisi salah memilih pasien. Yang datang Ibu Sarinah, tetapi yang
> dipilih Ibu Sari. Episode sudah `Admitted` dan bed `MELATI-03` sudah `Occupied`.
>
> Pukul **10:12** kesalahan disadari. Perawat belum sempat mengisi apa pun: belum ada
> pengkajian, belum ada tanda vital, belum ada resep. Petugas meminta kepala ruangan
> membatalkan episode. Kepala ruangan mengisi alasan "salah pilih pasien saat admisi".
> Episode Ibu Sari menjadi `Cancelled`, bed `MELATI-03` kembali `Available` pada saat yang
> sama, lalu admisi diulang atas nama Ibu Sarinah.
>
> Bandingkan dengan keadaan lain: bila kesalahan baru disadari pukul **14:30** dan perawat
> sudah mengisi pengkajian awal serta tanda vital, pembatalan **tidak lagi diizinkan**.
> Catatan klinis yang sudah masuk harus tetap ada, dan episodenya diselesaikan lewat
> penutupan dengan alasan tertulis.


### `RWI-RULE-005` — Setiap episode selalu menempel pada satu kunjungan

Dasar keputusan: `RWI-DEC-011`. Menutup `OQ-RI-001`.

> **Sebagian aturan ini sudah diganti pada 2026-08-21.** Baris "Pasien di IGD lalu diputuskan
> rawat inap" pada tabel di bawah berstatus `superseded` oleh `RWI-DEC-041`. Kunjungan IGD
> **tidak lagi** dipakai sebagai jangkar; kunjungan IGD ditutup dan kunjungan baru bertipe rawat
> inap dibuat. Rinciannya ada pada `RWI-RULE-029`. Kalimat pokok aturan ini — tidak boleh ada
> episode rawat inap tanpa kunjungan, dan satu episode menempel pada tepat satu kunjungan —
> **tetap berlaku utuh**.

Aturannya satu kalimat: **tidak boleh ada episode rawat inap tanpa kunjungan.** Satu episode
menempel pada tepat satu kunjungan pasien, dan kunjungan itulah yang dipakai modul lain untuk
mengenali konteks pasien.

Ada tiga jalur masuk, dan ketiganya berakhir pada bentuk data yang sama:

| Jalur masuk pasien | Yang terjadi pada kunjungan |
|---|---|
| Pasien di IGD lalu diputuskan rawat inap | Kunjungan IGD yang sudah ada dipakai sebagai jangkar episode. Tidak ada kunjungan baru |
| Pasien kontrol di poliklinik lalu dirujuk rawat inap | Kunjungan poliklinik yang sudah ada dipakai sebagai jangkar episode |
| Pasien datang langsung untuk rawat inap terencana | Sistem membuat kunjungan bertipe rawat inap secara otomatis di dalam proses admisi |

Bagi petugas admisi, ketiga jalur itu terasa sama: tetap satu form, tetap satu kali isi.
Pembuatan kunjungan otomatis pada jalur ketiga berjalan di belakang layar.

Contoh nyata:

> **Jalur dari poliklinik.** Pak Budi kontrol di poliklinik penyakit dalam pukul 09:00. Dokter
> memutuskan Pak Budi harus dirawat. Petugas admisi membuka admisi dari kunjungan poliklinik
> itu juga. Tidak ada kunjungan baru dibuat, dan resep yang ditulis nanti tetap terhubung ke
> kunjungan yang sama.
>
> **Jalur langsung.** Ibu Rina datang pukul 07:00 untuk operasi yang sudah dijadwalkan minggu
> lalu, dan hari itu tidak melewati poliklikinik sama sekali. Petugas membuka admisi langsung.
> Sistem membuat kunjungan bertipe rawat inap atas nama Ibu Rina, lalu episodenya menempel di
> situ. Petugas **tidak perlu** mendaftarkan kunjungan poliklinik yang sebenarnya tidak
> terjadi, sehingga laporan kunjungan poliklinik tetap bersih.

Ketergantungan yang harus dibereskan lebih dulu: `RWI-TRC-002`. PRD mengklaim sistem sekarang
memaksa kelas pasien `"RAWAT JALAN"` saat kunjungan dibuat. Bila klaim itu terbukti benar,
jalur ketiga tidak akan bisa berjalan sebelum pembatasan itu diperbaiki.


### `RWI-RULE-006` — Perpindahan pasien

Dasar keputusan: `RWI-DEC-012`. Menutup `RWI-CON-003`, `OQ-RI-004`, dan `OQ-RI-005`.

Kewenangan mengikuti tabel PRD bagian 14 apa adanya, sesuai `RWI-FACT-006`:

| Peran | Boleh memindahkan pasien? | Dasarnya |
|---|---|---|
| Kepala Perawat atau kepala ruangan | Ya | Tanda centang pada tabel bagian 14 |
| Perawat pelaksana | Ya | Tanda centang pada tabel bagian 14 |
| Supervisor | Ya | Tanda centang pada tabel bagian 14 |
| Dokter atau DPJP | **Belum jelas** | Tabel hanya menulis "sesuai SOP" tanpa menyebut SOP mana. Lihat `RWI-OQ-025` |
| Petugas admisi | Tidak | Kolomnya dikosongkan pada tabel bagian 14 |
| Petugas billing | Tidak | Kolomnya dikosongkan pada tabel bagian 14 |

Cara kerja perpindahan:

1. Perpindahan berjalan **satu langkah**. Unit tujuan tidak perlu menyatakan menerima lebih
   dulu. Ini jawaban untuk `OQ-RI-005`: penerimaan unit tujuan **tidak wajib**.
2. Karena tidak ada masa menunggu jawaban, tempat tidur tujuan **tidak** melewati status
   `Reserved`. Begitu perpindahan disimpan, tempat tidur lama kembali `Available` dan tempat
   tidur tujuan menjadi `Occupied`.
3. Perpindahan **tidak** membuat episode baru. Episode dan kunjungannya tetap sama; yang
   berubah hanya lokasi pasien, dan lokasi lamanya tetap tersimpan sebagai riwayat.

Contoh nyata:

> Pukul **15:20** perawat pelaksana Ruang Melati memindahkan Pak Budi dari bed `MELATI-03` ke
> bed `MELATI-07` karena teman sekamarnya batuk terus-menerus. Perawat itu menyimpan
> perpindahan tersebut sendiri, tanpa meminta persetujuan siapa pun dan tanpa menunggu jawaban
> dari pihak mana pun. Saat itu juga `MELATI-03` menjadi `Available`, `MELATI-07` menjadi
> `Occupied`, dan lokasi Pak Budi pada census berubah.

**Konsekuensi yang disadari — `RWI-RISK-001`.** Lewat `RWI-DEC-005`, pindah kelas perawatan
(`RWI-GAP-003`) menumpang pada kemampuan yang sama, yaitu CAP-017. Karena itu aturan di atas
juga berarti seorang perawat pelaksana dapat memindahkan pasien dari kelas 3 ke kelas 1 tanpa
persetujuan siapa pun, dan tagihan pasien ikut berubah mengikuti kelas barunya. Pemilik
kebutuhan memilih ini **secara sadar** setelah konsekuensinya dijelaskan pada wawancara
pertanyaan 8. Apakah pindah kelas perlu dikecualikan masih ditanyakan pada `RWI-OQ-026`.

**Kejanggalan yang perlu diperhatikan.** Pada tabel bagian 14, perawat pelaksana mendapat
tanda centang penuh, sedangkan Dokter/DPJP hanya ditulis "sesuai SOP" tanpa SOP yang disebut.
Bila tabel diikuti harfiah, dokter penanggung jawab justru memiliki kewenangan paling tidak
jelas atas perpindahan pasiennya sendiri. Ini bukan tafsiran, melainkan isi tabelnya; lihat
`RWI-FACT-006`.


### `RWI-RULE-007` — Pindah kelas perawatan

Dasar keputusan: `RWI-DEC-013`. Menutup `RWI-OQ-026` dan `RWI-OQ-015`.

Pindah kelas **tidak dibedakan** dari pindah tempat tidur biasa. Seluruh aturan kewenangan
pada `RWI-RULE-006` berlaku penuh, termasuk untuk perpindahan yang menaikkan atau menurunkan
kelas.

| Butir | Ketentuan |
|---|---|
| Siapa yang boleh | Sama dengan `RWI-RULE-006`: Kepala Perawat, Perawat pelaksana, dan Supervisor. Tidak ada persetujuan tambahan |
| Kelas yang ditagihkan | Mengikuti kamar yang ditempati saat itu. Tidak ada kelas tagihan yang berdiri sendiri |
| Sejak kapan berlaku | Sejak waktu perpindahan disimpan, bukan sejak awal episode |
| Yang wajib disimpan | Kelas lama, kelas baru, waktu perpindahan, dan siapa yang memindahkan |

Contoh nyata:

> Pak Budi masuk tanggal **12 Agustus** dan ditempatkan di kamar kelas 3. Tanggal **14
> Agustus pukul 21:40** kamar kelas 3 harus dikosongkan untuk pasien isolasi, dan perawat jaga
> memindahkan Pak Budi ke bed `MAWAR-02` yang berada di kelas 1.
>
> Sejak pukul 21:40 itu juga, kelas yang ditagihkan untuk Pak Budi menjadi kelas 1. Sistem
> tidak meminta persetujuan siapa pun. Riwayat menyimpan bahwa perpindahan dari kelas 3 ke
> kelas 1 dilakukan oleh perawat jaga tersebut pada pukul 21:40, sehingga bila kemudian
> dipertanyakan, jejaknya bisa ditelusuri.

**Konsekuensi yang diterima secara sadar.** Pemilik kebutuhan memilih opsi ini pada wawancara
pertanyaan 9 setelah dua akibat berikut dijelaskan:

1. `RWI-RISK-001` tetap terbuka dan **diterima**. Tagihan pasien dapat naik tanpa ada yang
   menyetujui lebih dulu. Pengendaliannya bertumpu pada riwayat dan audit setelah kejadian,
   bukan pada pencegahan sebelum kejadian.
2. Karena kelas tagihan selalu mengikuti kamar, sistem **tidak punya tempat** untuk mencatat
   pasien titipan, yaitu pasien yang dirawat di kamar yang bukan haknya karena kamar penuh.
   `RWI-GAP-004` karena itu belum terjawab, dan penyelesaiannya menunggu jawaban `RWI-OQ-016`
   tentang apakah pasien titipan masuk MVP. Bila nanti pasien titipan dinyatakan masuk MVP,
   keputusan ini perlu ditinjau ulang lewat Amendment Pass.


### `RWI-RULE-008` — Perpindahan yang gagal di tengah jalan

Dasar keputusan: `RWI-DEC-014`. Menutup `RWI-CON-007`.

Perpindahan pasien adalah **satu tindakan yang tidak bisa setengah jadi**. Penempatan pada
tempat tidur baru dan pelepasan tempat tidur lama terjadi sebagai satu kesatuan, bukan sebagai
dua langkah berurutan.

| Yang terjadi | Yang tercatat di sistem |
|---|---|
| Perpindahan berhasil seluruhnya | Tempat tidur lama menjadi `Available`, tempat tidur tujuan menjadi `Occupied`, dan riwayat lokasi bertambah satu baris |
| Ada bagian mana pun yang gagal | **Tidak ada satu pun data yang berubah.** Pasien tetap di tempat tidur lama, tempat tidur tujuan tetap kosong, dan tidak ada riwayat setengah jadi |

Penegasan invariant INV-02: satu episode yang sedang berjalan **selalu** menempati tepat satu
tempat tidur, tanpa kecuali, termasuk pada saat perpindahan sedang diproses. Tidak pernah ada
keadaan pasien tercatat tanpa lokasi, sehingga census juga tidak pernah menampilkan baris
pasien tanpa tempat tidur.

Contoh nyata:

> Pukul **15:20** perawat menekan tombol simpan untuk memindahkan Pak Budi dari bed
> `MELATI-03` ke bed `MAWAR-02`. Tepat pada detik itu jaringan di ruangan terputus dan layar
> menampilkan pesan gagal.
>
> Pukul **15:26** jaringan pulih dan perawat membuka kembali daftar pasien dirawat. Pak Budi
> **masih** tercatat di `MELATI-03`, bed `MAWAR-02` **masih** kosong dan boleh dipakai pasien
> lain, dan tidak ada catatan perpindahan yang tersimpan separuh. Perawat tinggal mengulang
> perpindahan dari awal tanpa perlu membereskan apa pun lebih dulu.

**Akibat pada PRD.** Urutan pada EPIC RI-09, yaitu menutup penempatan tempat tidur lama lebih
dulu kemudian menempati tempat tidur baru, **tidak berlaku**. Urutan itulah yang menyebabkan
`RWI-CON-007`, dan PRD perlu dikoreksi mengikuti keputusan ini.


### `RWI-RULE-009` — Gerbang keuangan sebelum episode ditutup

Dasar keputusan: `RWI-DEC-015`. Menutup `RWI-CON-005` dan `OQ-RI-008`.

**Penegasan bahasa.** Rumusan EPIC RI-10 yang berbunyi "status financial clearance tersedia"
dinyatakan **tidak berlaku** karena terlalu longgar; status `Blocked` juga "tersedia". Yang
berlaku adalah rumusan berikut: status kelayakan keuangan harus **bernilai `Cleared`**.

| Status kelayakan keuangan | Episode boleh ditutup? |
|---|---|
| `Cleared` | Ya, lewat jalur biasa |
| `Pending` | Tidak. Petugas menunggu kasir menyelesaikan hitungannya |
| `Blocked` | Tidak. Ada masalah yang harus dibereskan lebih dulu |
| Statusnya belum ada sama sekali, karena modul Billing belum berjalan | Tidak. Diperlakukan sama dengan `Pending` |

### Jalan keluar lewat supervisor

Supervisor boleh menutup episode walaupun kelayakan keuangannya belum `Cleared`.

| Butir | Ketentuan |
|---|---|
| Siapa yang boleh | Hanya Supervisor. Ini bukan kewenangan baru; tabel PRD bagian 14 sudah mencentang Supervisor pada baris `Financial clearance`, lihat `RWI-FACT-008` |
| Syarat | Alasan wajib diisi. Tidak boleh dikosongkan dan tidak boleh diisi tanda baca saja |
| Yang wajib disimpan | Nama supervisor, waktu penutupan, alasan, dan nilai status kelayakan keuangan pada saat itu |
| Akibat pada episode | Episode ditutup dan tempat tidur dilepas seperti penutupan biasa |
| Penandaan | Episode ditandai **ditutup tanpa kelayakan keuangan** dan masuk laporan tersendiri |

Gerbang ini hanya berlaku pada **penutupan** episode. Keputusan pulang oleh DPJP dan
perpindahan episode ke `DischargePending` tidak menunggu kelayakan keuangan, sehingga proses
menyiapkan pasien pulang tetap bisa berjalan bersamaan dengan kasir menghitung.

Contoh nyata:

> Ibu Sari berstatus `DischargePending` sejak pukul **08:30**. Resume pulang sudah
> ditandatangani DPJP. Pukul **13:10** keluarganya sudah menunggu, tetapi status keuangannya
> `Blocked` karena berkas rujukan BPJS tidak lengkap.
>
> Petugas admisi mencoba menutup episode dan **ditolak sistem**. Bed `MELATI-03` masih
> tercatat ditempati Ibu Sari.
>
> Pukul **13:40** supervisor menilai berkas BPJS bisa dilengkapi menyusul, lalu menutup
> episode dengan alasan "berkas rujukan BPJS menyusul, disepakati keluarga". Episode menjadi
> `Closed`, bed `MELATI-03` kembali `Available`, dan episode Ibu Sari muncul pada laporan
> penutupan tanpa kelayakan keuangan lengkap dengan nama supervisor dan alasannya.

**Cara mengukur apakah gerbang ini masih berguna.** Bila jumlah baris pada laporan penutupan
tanpa kelayakan keuangan tidak berkurang setelah modul Billing operasional, berarti gerbangnya
sudah berubah menjadi formalitas dan aturannya perlu ditinjau ulang.


### `RWI-RULE-010` — Kewenangan penutupan episode

Dasar keputusan: `RWI-DEC-016`. Menutup `RWI-CON-004` dan `OQ-RI-006`.

Ada **dua pekerjaan berbeda** yang selama ini tercampur dalam satu baris tabel:

| Pekerjaan | Siapa yang boleh | Sifatnya |
|---|---|---|
| Memutuskan pasien boleh pulang | **Hanya DPJP** | Keputusan klinis. Tidak bisa didelegasikan |
| Menutup episode dan melepas tempat tidur | Petugas admisi, atau Supervisor | Menjalankan keputusan yang sudah dibuat DPJP. Bukan keputusan baru |

Alasan pemisahan ini: menutup episode tidak menambah atau mengurangi apa pun secara klinis.
Yang menjaga agar penutupan tidak terjadi terlalu cepat bukanlah jabatan orang yang menekan
tombol, melainkan daftar syarat di bawah ini, dan syarat itu diperiksa sistem, bukan diingat
manusia.

Syarat yang diperiksa sistem sebelum episode boleh ditutup:

1. keputusan pulang dari DPJP sudah ada;
2. resume pulang sudah ada;
3. seluruh butir wajib pada daftar periksa administrasi sudah ditandai selesai, sesuai
   `RWI-RULE-018`;
4. kelayakan keuangan bernilai `Cleared` sesuai `RWI-RULE-009`, **atau** episode ditutup
   Supervisor lewat jalan keluar pada aturan yang sama;
5. tempat tidur aktif ditemukan, sesuai INV-02 dan `RWI-RULE-008`.

Bila salah satu saja belum terpenuhi, penutupan ditolak dan sistem menyebutkan syarat mana
yang belum beres, bukan sekadar menolak tanpa keterangan.

**Frasa "sesuai SOP"** pada baris `Close episode` tabel kewenangan PRD bagian 14 dinyatakan
diganti oleh aturan ini. Frasa yang sama pada baris `Transfer` untuk Dokter/DPJP masih terbuka
dan ditangani terpisah lewat `RWI-OQ-025`.

Contoh nyata:

> Pukul **08:30** dr. Andi menyatakan Ibu Sari boleh pulang dan menulis resume pulang. Sejak
> saat itu episode berstatus `DischargePending`. dr. Andi lalu melanjutkan visite ke pasien
> lain dan tidak kembali ke sistem.
>
> Pukul **13:10** kasir menyelesaikan hitungan dan status keuangan menjadi `Cleared`.
>
> Pukul **13:15** petugas admisi menutup episode Ibu Sari. Sistem memeriksa kelima syarat,
> semuanya terpenuhi, episode menjadi `Closed`, dan bed `MELATI-03` kembali `Available`.
> dr. Andi tidak perlu dihubungi lagi.

**Kebutuhan yang timbul dari keputusan ini.** Karena yang memutuskan dan yang menutup adalah
orang berbeda, episode bisa menggantung di `DischargePending` bila petugas admisi lalai.
Diperlukan daftar pantau berisi episode yang sudah boleh pulang tetapi belum ditutup, beserta
lama menggantungnya. Rinciannya dicatat sebagai `RWI-OQ-027`.


### `RWI-RULE-011` — Lima cara pulang

Dasar keputusan: `RWI-DEC-017`. Menutup `RWI-OQ-013`, `RWI-OQ-014`, `RWI-GAP-001`, dan
`RWI-GAP-002`.

Episode menyimpan satu penanda **cara pulang** dengan lima nilai. Kelima-limanya sama-sama
menutup episode dan melepas tempat tidur; yang berbeda adalah syaratnya.

| Cara pulang | Artinya dengan bahasa sehari-hari | Izin DPJP wajib? | Resume pulang wajib? | Syarat khusus |
|---|---|---|---|---|
| Atas izin DPJP | Dokter menyatakan pasien sudah boleh pulang | Ya | Ya | — |
| Atas permintaan sendiri (APS) | Pasien atau keluarga minta pulang walaupun dokter belum mengizinkan | Tidak | Ya | Pernyataan pasien atau keluarga wajib tersimpan. DPJP wajib diberi tahu |
| Dirujuk | Pasien dipindahkan ke rumah sakit lain | Ya | Ya | Rumah sakit tujuan wajib dicatat |
| Meninggal | Pasien meninggal saat dirawat | Tidak berlaku | Diganti catatan kematian | Waktu meninggal dan nama pencatat wajib diisi |
| Kabur | Pasien pergi tanpa memberi tahu siapa pun | Tidak | Tidak | Waktu pasien terakhir terlihat dan nama pelapor wajib diisi |

Yang **tidak berubah** untuk kelima cara pulang:

1. Tempat tidur tetap dilepas dan kembali `Available`.
2. Gerbang keuangan `RWI-RULE-009` tetap berlaku. Untuk pasien kabur dan pasien meninggal,
   status `Cleared` biasanya belum ada, sehingga penutupan memakai jalan keluar Supervisor
   pada aturan yang sama, lengkap dengan alasan tertulis.
3. Kewenangan penutupan tetap mengikuti `RWI-RULE-010`.
4. Cara pulang wajib dipilih; tidak boleh dikosongkan dan tidak boleh berupa teks bebas.

Contoh nyata:

> **Pasien kabur.** Pukul **02:10** perawat jaga mendapati bed `MELATI-03` kosong dan barang
> Pak Budi sudah tidak ada. Pasien terakhir terlihat pukul **01:30** saat pemeriksaan tanda
> vital. Perawat mencatat cara pulang "kabur", waktu terakhir terlihat 01:30, dan namanya
> sendiri sebagai pelapor. Karena status keuangan Pak Budi masih `Pending`, penutupan
> memerlukan Supervisor, yang menutup episode pukul **06:45** dengan alasan "pasien kabur,
> tagihan ditindaklanjuti terpisah". Bed `MELATI-03` kembali `Available` pagi itu juga, bukan
> tertahan berhari-hari.
>
> **Pasien meninggal.** Ibu Sari meninggal pukul **17:20**. Dokter jaga mencatat waktu
> meninggal dan namanya sebagai pencatat. Resume pulang tidak diminta sistem; yang diminta
> catatan kematian. Episode ditutup dan bed dilepas setelah jenazah dipindahkan.

**Wajib ditinjau pemilik klinis.** Aturan untuk pasien meninggal dan pasien kabur disusun dari
praktik umum, bukan dari persetujuan komite klinis. Keduanya menyangkut rekam medis dan
pelaporan wajib. Sesuai `RWI-DEC-006`, dua baris itu tetap **terbuka secara klinis** dan
menjadi syarat sebelum modul dipakai melayani pasien sungguhan, walaupun keputusan produknya
sudah `approved`.

**Terkait.** Apakah episode yang ditutup karena kabur boleh dibuka kembali bila pasien datang
lagi, masih terbuka pada `OQ-RI-012`.


### `RWI-RULE-012` — Pemisahan jenis kelamin dan isolasi — **ATURAN KERAS**

Dasar keputusan: `RWI-DEC-064`, dilengkapi `RWI-DEC-065` dan `RWI-DEC-066`.
**Menggantikan `RWI-DEC-018`** yang sebelumnya memilih keduanya hanya menjadi penyaring pencarian.

> **Perubahan arah pada 2026-08-21.** Sampai revisi sebelumnya, jenis kelamin dan isolasi hanya
> menyaring hasil pencarian tempat tidur — sistem tetap **mengizinkan** penempatan yang bercampur.
> Sejak `RWI-DEC-064`, keduanya menjadi **aturan yang menolak penempatan**. Ini keputusan
> pengendalian infeksi dan privasi, diambil Muhammad Hamzah selaku pemilik berwenang.

**Tujuan.** Sistem tidak boleh menempatkan pasien pada tempat tidur atau kamar yang secara klinis
atau privasi tidak layak baginya, walaupun petugas memaksa.

**Di mana aturan ini bekerja.** Di dalam pemeriksaan **Kelayakan Penempatan**, yang dipanggil pada
dua tindakan: menempatkan pasien dan memindahkan pasien. Titik penyisipannya memang sudah disiapkan
sejak arsitektur domain revision `0.1` untuk keperluan ini.

---

### Bagian A — Kebutuhan isolasi

**Di mana kebutuhan isolasi dicatat.** Pada **episode rawat inap**, bukan pada pasien dan bukan
pada catatan klinis. Alasannya: kebutuhan isolasi melekat pada satu masa perawatan, bukan pada
orangnya selamanya.

| No | Aturan |
| ---: | --- |
| 1 | Setiap episode punya penanda **membutuhkan isolasi**, bernilai tidak secara bawaan |
| 2 | Keputusan klinisnya milik **DPJP**, dan dapat diperbarui kapan saja selama perawatan berjalan |
| 3 | Pada admisi awal, petugas admisi **boleh merekam** nilainya berdasarkan instruksi atau keterangan dokter pengirim. Yang direkam petugas admisi ditandai sebagai **catatan awal**, bukan keputusan klinis |
| 4 | Setelah episode aktif, hanya **DPJP aktif** yang boleh mengubahnya, dan perubahannya ditandai sebagai **keputusan klinis** |
| 5 | Pasien yang membutuhkan isolasi **hanya boleh** ditempatkan pada tempat tidur bertanda isolasi. Penempatan lain ditolak |
| 6 | Pasien yang **tidak** membutuhkan isolasi **tidak boleh** menempati tempat tidur isolasi, supaya kapasitas isolasi tidak terpakai sia-sia. Penempatan lain ditolak |
| 7 | Bila kebutuhan isolasi berubah menjadi ya sementara pasien sudah berada di tempat tidur biasa, perubahan itu **tetap diterima**. Sistem tidak menahan pencatatan klinis. Episode itu muncul pada daftar pantau **penempatan tidak sesuai** sampai pasien dipindahkan |

**Kenapa aturan nomor 3 dan 4 dipisah.** Petugas admisi sering menerima pasien rujukan dengan
surat yang sudah menyebut kebutuhan isolasi, dan penempatan tidak boleh menunggu pengkajian klinis
yang slice-nya masih di luar MVP. Tetapi merekam keterangan orang lain berbeda dari memutuskan
secara klinis. Karena itu keduanya dibedakan penandanya, bukan disamakan.

**Kenapa aturan nomor 7 tidak menahan.** Menahan pencatatan klinis demi menjaga aturan penempatan
adalah urutan yang terbalik: yang benar adalah fakta klinis dicatat lebih dulu, lalu sistem
menunjukkan bahwa penempatannya perlu dibetulkan.

**Contoh konkret.**

> Tn. Budi datang dari puskesmas dengan surat rujukan yang menyebut suspek penyakit menular. Pukul
> 09:15 petugas admisi merekam "membutuhkan isolasi" sebagai catatan awal. Pencarian tempat tidur
> otomatis hanya menampilkan tempat tidur isolasi, dan percobaan menempatkannya di `BD-RSMMC-00042`
> yang bukan isolasi **ditolak**.
>
> Hari kedua, dr. Andi selaku DPJP menyatakan hasil pemeriksaan negatif dan mengubah penandanya
> menjadi tidak membutuhkan isolasi. Perubahan itu ditandai keputusan klinis atas nama dr. Andi.
> Tn. Budi kini justru **tidak boleh** tetap menempati tempat tidur isolasi, sehingga episodenya
> muncul pada daftar pantau penempatan tidak sesuai sampai ia dipindahkan.

---

### Bagian B — Pemisahan jenis kelamin

| No | Aturan |
| ---: | --- |
| 1 | Penempatan **ditolak** bila penanda tempat tidur tidak menerima jenis kelamin pasien |
| 2 | Bila jenis kelamin pasien **belum tercatat**, penempatan hanya boleh ke tempat tidur yang menerima laki-laki dan perempuan sekaligus, **dan** ke kamar yang belum ada penghuninya |
| 3 | **Seluruh kamar dianggap tidak boleh ditempati campur.** Penempatan ke kamar yang sudah punya penghuni berjenis kelamin berbeda **ditolak** |
| 4 | Penghuni yang menempati **boks bayi** tidak dihitung saat memeriksa aturan nomor 3 |
| 5 | Penempatan **ke** boks bayi dikecualikan dari aturan nomor 1, 2, dan 3 |
| 6 | Kamar berisi satu tempat tidur tidak pernah tersentuh aturan nomor 3, karena tidak mungkin ada penghuni lain |
| 7 | Aturan nomor 1 sampai 6 berlaku sama pada penempatan **dan** perpindahan |

**Tidak ada kolom baru pada master kamar.** `RWI-DEC-066` secara tegas menolak menambah penanda
"boleh campur" pada `MstRoom`. Alasannya: penanda `IsForMale` dan `IsForFemale` yang sudah ada
bernilai benar secara bawaan untuk **setiap** kamar, sehingga menambah penanda ketiga hanya akan
menambah cara baru untuk salah setel. Aturan nomor 3 dijalankan dengan memeriksa **penghuni yang
sedang ada**, bukan dengan membaca penanda.

**Kenapa aturan nomor 4 dan 5 dua arah.** Bayi laki-laki yang dirawat gabung di kamar ibunya tidak
boleh membuat kamar itu tertutup bagi pasien perempuan lain, dan bayi itu sendiri tidak boleh
ditolak hanya karena ibunya berjenis kelamin berbeda. Karena itu boks bayi dikecualikan dari kedua
sisi pemeriksaan.

**Contoh konkret.**

> Kamar Melati 3 berisi tiga tempat tidur dan satu boks bayi. Pukul 08:00 Ny. Sari menempati
> `MELATI-03-A`, dan bayinya menempati boks `BOX-MELATI-03-A`.
>
> Pukul 10:00 petugas mencoba menempatkan Tn. Budi di `MELATI-03-B`. **Ditolak**, karena kamar itu
> sudah dihuni pasien perempuan. Bayi Ny. Sari tidak ikut dihitung, walaupun bayinya laki-laki.
>
> Pukul 10:30 petugas menempatkan Ny. Rina di `MELATI-03-B`. **Diterima**, karena sesama perempuan.

---

### Yang berubah dibanding revisi sebelumnya

| Hal | Sebelum `RWI-DEC-064` | Sesudah |
| --- | --- | --- |
| Pasien butuh isolasi di kamar biasa | Diizinkan | **Ditolak** |
| Kapasitas isolasi dipakai pasien biasa | Diizinkan | **Ditolak** |
| Laki-laki dan perempuan sekamar | Diizinkan | **Ditolak**, kecuali boks bayi |
| Peran penanda pada master | Hanya menyaring pencarian | Menolak penempatan |
| Kebutuhan isolasi | Tidak tercatat di mana pun | Atribut episode, dengan pembeda catatan awal dan keputusan klinis |

### `RWI-RULE-013` — Pasien yang ditempatkan di kamar bukan haknya

Dasar keputusan: `RWI-DEC-019`. Menutup `RWI-OQ-016` dan `RWI-GAP-004`.

Pada MVP ini, keadaan "pasien titipan" **tidak dikenali sistem sebagai keadaan khusus**. Tidak
ada kelas hak yang disimpan terpisah, dan tidak ada penanda titipan.

| Butir | Ketentuan pada MVP |
|---|---|
| Kelas yang ditagihkan | Selalu kelas kamar yang ditempati, sesuai `RWI-RULE-007`. Tidak ada pengecualian |
| Penanda titipan | Tidak ada |
| Kelas hak pasien | Tidak disimpan terpisah dari kelas kamar |
| Keringanan biaya karena kamar penuh | Diurus petugas billing di luar modul ini |

Contoh nyata:

> Pukul **22:15** Pak Budi masuk sebagai pasien kelas 3. Semua kamar kelas 3 penuh, sehingga
> petugas menempatkannya di bed `MAWAR-04` yang berada di kelas 1.
>
> Sistem mencatat Pak Budi berada di kelas 1 dan menagihnya kelas 1 sejak pukul 22:15. Tidak
> ada penanda bahwa ini sebenarnya penempatan darurat. Bila rumah sakit ingin tetap menagih
> Pak Budi sebagai kelas 3, koreksinya dikerjakan petugas billing secara manual, di luar modul
> Rawat Inap.

**Konsekuensi yang diterima secara sadar.** Penempatan pasien di kamar yang bukan haknya
adalah keadaan yang sangat lazim di rumah sakit Indonesia, dan keadaan itu menjadi lebih
sering setelah `RWI-DEC-018` membuat sistem tidak menolak penempatan apa pun. Selama MVP,
pasien yang terpaksa ditempatkan di kelas lebih tinggi akan menerima tagihan lebih besar
sampai ada yang mengoreksinya secara manual. Pemilik kebutuhan memilih ini setelah
konsekuensinya dijelaskan pada wawancara pertanyaan 15.

Topik ini kembali sebagai **Amendment Pass** bila modul Billing sudah operasional dan rumah
sakit memutuskan keringanan harus otomatis.


### `RWI-RULE-014` — Bayi baru lahir dan ICU

Dasar keputusan: `RWI-DEC-020`. Menutup `OQ-RI-010` dan `RWI-GAP-011`.

#### Bayi baru lahir yang dirawat gabung

| Butir | Ketentuan |
|---|---|
| Episode | Bayi punya episode rawat inap **sendiri**, terpisah dari episode ibunya |
| Kunjungan | Bayi punya kunjungan sendiri, mengikuti `RWI-RULE-005` yang mewajibkan setiap episode menempel pada satu kunjungan |
| Tempat tidur | Boks bayi didaftarkan sebagai **tempat tidur tersendiri** di dalam kamar ibu. Dengan begitu invariant satu tempat tidur satu penempatan aktif tetap utuh |
| Hubungan bayi dan ibu | Disimpan satu penanda, supaya census dapat menampilkan keduanya berpasangan |
| Pendaftaran pasien bayi | Mengikuti modul Registrasi. Cara memberi nama sementara bagi bayi bukan urusan modul ini |

#### ICU

| Butir | Ketentuan |
|---|---|
| Perlakuan | ICU adalah unit layanan biasa dengan tempat tidur biasa. Tidak ada aturan khusus pada MVP |
| Yang sengaja tidak dibuat | Tidak ada skor keparahan pasien, tidak ada aturan rasio perawat terhadap pasien, dan tidak ada syarat khusus untuk masuk maupun keluar ICU |

Contoh nyata:

> Ibu Rina melahirkan pukul **03:20** dan tetap dirawat di bed `MELATI-02`. Bayinya
> didaftarkan sebagai pasien baru, lalu dibuatkan episode rawat inap sendiri yang menempati
> boks `BOKS-MELATI-02A` di kamar yang sama.
>
> Census menampilkan dua baris: Ibu Rina di `MELATI-02` dan bayinya di `BOKS-MELATI-02A`,
> keduanya bertanda rawat gabung. Obat dan tindakan untuk bayi tercatat atas nama bayi, bukan
> atas nama Ibu Rina.
>
> Pukul **09:00** bayi harus dipindahkan ke NICU. Itu **perpindahan biasa** pada episode bayi
> sendiri, mengikuti `RWI-RULE-006`. Episode Ibu Rina tidak terpengaruh sama sekali, dan
> penanda rawat gabungnya berakhir.

**Konsekuensi.** Master data tempat tidur wajib memuat boks bayi sebagai tempat tidur, sehingga
`RWI-TRC-006` menjadi lebih penting. Aturan klinis perawatan bayi baru lahir dan aturan klinis
perawatan intensif tetap **di luar scope** modul ini; yang dikunci di sini hanya bentuk data
dan penempatannya.


### `RWI-RULE-015` — Admisi yang diselesaikan setelah pemesanan gugur

Dasar keputusan: `RWI-DEC-021`. Menutup `RWI-OQ-024`.

Keadaan tempat tidur diperiksa **dua kali**: sekali saat dipesan, dan sekali lagi saat admisi
benar-benar diaktifkan. Pemeriksaan kedua inilah yang menentukan.

| Keadaan tempat tidur saat admisi diaktifkan | Yang terjadi |
|---|---|
| Masih kosong | Penempatan diteruskan seperti biasa. Petugas tidak diberi peringatan apa pun, walaupun pemesanannya sudah lewat 2 jam |
| Sudah ditempati atau dipesan pasien lain | Penempatan **ditolak**, dan pesannya menyebut tempat tidur mana yang sudah terpakai |

Yang terjadi pada data ketika penempatan ditolak:

1. Episode tetap berstatus `Draft`, tidak dibatalkan dan tidak dihapus.
2. Seluruh isian admisi tetap utuh: penjamin, DPJP, kelas, dan data lain yang sudah diisi.
3. Petugas cukup memilih tempat tidur lain, tanpa mengetik ulang apa pun.

Contoh nyata:

> **Cabang aman.** Petugas membuka admisi Ibu Sari pukul **09:15** dan memesan bed
> `MELATI-03`. Ia terputus karena pergantian sif. Pukul **11:40** petugas sif berikutnya
> menyelesaikan admisi itu. Pemesanan memang sudah gugur pukul 11:15, tetapi `MELATI-03`
> ternyata masih kosong. Penempatan berhasil, dan tidak ada peringatan yang muncul.
>
> **Cabang bentrok.** Keadaan yang sama, tetapi pukul **11:20** petugas lain sudah memesan
> `MELATI-03` untuk Pak Budi. Ketika admisi Ibu Sari diaktifkan pukul 11:40, sistem menolak
> dengan pesan bahwa `MELATI-03` sudah dipesan pasien lain. Data admisi Ibu Sari **tidak
> hilang**; petugas memilih bed `MELATI-08` yang masih kosong dan menyelesaikannya saat itu
> juga.

**Konsekuensi.** Karena episode `Draft` sendiri tidak ikut kedaluwarsa, akan ada `Draft` yang
ditinggalkan berhari-hari tanpa pernah diaktifkan maupun dibatalkan. Cara membersihkannya
belum diputuskan dan dicatat sebagai `RWI-OQ-028`.


### `RWI-RULE-016` — Kewenangan DPJP atas perpindahan pasien

Dasar keputusan: `RWI-DEC-022`. Menutup `RWI-OQ-025`, dan memunculkan `RWI-CON-008`.

Frasa "sesuai SOP" pada baris `Transfer` untuk Dokter/DPJP di tabel kewenangan PRD bagian 14
diganti aturan berikut.

| Keadaan pasien | Yang boleh dilakukan DPJP |
|---|---|
| Pasien berada dalam tanggung jawab klinis DPJP tersebut | DPJP dapat **menginisiasi** perpindahan, dan dapat pula **menyetujui** perpindahan yang diusulkan pihak lain. Dasar pertimbangannya adalah indikasi medis dan kesiapan unit tujuan |
| Pasien berada di bawah DPJP lain | DPJP **tidak** boleh memindahkan sendiri. Perpindahan harus melalui koordinasi dengan DPJP yang bertanggung jawab, **atau** melalui proses pengalihan tanggung jawab DPJP yang terdokumentasi |

Kutipan keputusan pemilik kebutuhan, disimpan apa adanya agar tidak berubah makna saat
diringkas:

> "DPJP dapat menginisiasi dan/atau menyetujui transfer pasien yang berada dalam tanggung
> jawab klinisnya, berdasarkan indikasi medis dan kesiapan unit tujuan. Untuk pasien yang
> berada di bawah DPJP lain, transfer harus melalui koordinasi dengan DPJP terkait atau proses
> pengalihan tanggung jawab DPJP yang terdokumentasi."

Contoh nyata:

> **Pasien sendiri.** dr. Andi adalah DPJP Pak Budi. Pukul **10:15** dr. Andi menilai Pak Budi
> perlu dipindahkan ke ruang yang lebih dekat pos perawat. dr. Andi dapat memindahkan sendiri
> tanpa meminta izin siapa pun.
>
> **Pasien DPJP lain.** dr. Sinta bukan DPJP Pak Budi. dr. Sinta **tidak** dapat memindahkan
> Pak Budi. Ia harus berkoordinasi dengan dr. Andi lebih dulu, atau tanggung jawab DPJP atas
> Pak Budi dialihkan kepadanya melalui proses yang tercatat.

#### Penegasan: bagian mana yang dijaga sistem, bagian mana yang tidak

Dasar penegasan: `RWI-DEC-023`. Menutup `RWI-CON-008`.

| Bagian kalimat keputusan | Dijaga sistem? | Penjelasan |
|---|---|---|
| "pasien yang berada dalam tanggung jawab klinisnya" | **Ya** | Sistem memeriksa apakah dokter yang meminta benar DPJP episode tersebut. Dokter lain ditolak |
| "berdasarkan indikasi medis" | **Sebagian** | Sistem tidak menilai benar atau salahnya indikasi, tetapi alasan medis wajib diisi dan disimpan bersama perpindahan |
| "dan kesiapan unit tujuan" | **Tidak** | Ini pertimbangan profesional DPJP, bukan pemeriksaan mesin. Sistem tetap satu langkah sesuai `RWI-DEC-012` dan tidak menunggu pernyataan siap dari unit tujuan |
| "koordinasi dengan DPJP terkait" | **Belum diputuskan** | Masih digali pada `RWI-OQ-029` |

Dengan penegasan ini, `RWI-DEC-012` **tetap berlaku utuh**. Tidak ada bagian yang perlu
ditandai `superseded`, dan alur perpindahan tetap sama untuk semua peran: satu langkah, tanpa
menunggu jawaban unit tujuan.

**Konsekuensi yang diterima secara sadar.** Bila unit tujuan ternyata tidak siap menerima,
misalnya alatnya tidak tersedia atau perawatnya kurang, sistem tidak akan mencegah
perpindahan. Risiko itu ditanggung oleh SOP dan penilaian profesional, bukan oleh aplikasi.

#### Dokter yang bukan DPJP episode tersebut

Dasar: `RWI-DEC-024`. Menutup `RWI-OQ-029`.

Koordinasi antar dokter **tidak direkam** sistem. Yang masuk sistem adalah hasilnya, bukan
percakapannya. Sistem hanya mengenal dua jalan sah bagi dokter:

1. DPJP episode itu sendiri yang memindahkan pasien; atau
2. tanggung jawab DPJP dialihkan lebih dulu secara tercatat, lalu DPJP yang baru memindahkan.

Dokter di luar dua keadaan itu **selalu ditolak**, tanpa pengecualian dan tanpa kolom
keterangan yang bisa dipakai melewatinya.

Contoh nyata:

> Pukul **14:00** dr. Sinta menelepon dr. Andi dan keduanya sepakat Pak Budi sebaiknya
> dipindahkan ke ruang yang lebih dekat pos perawat. dr. Andi sedang di kamar operasi dan
> tidak bisa membuka sistem.
>
> dr. Sinta mencoba memindahkan Pak Budi dan **ditolak**, karena ia bukan DPJP Pak Budi.
> Tidak ada kolom apa pun untuk mencatat bahwa koordinasi sudah terjadi.

**Konsekuensi yang diterima secara sadar.** Pada contoh di atas, jalan yang hampir pasti
ditempuh adalah meminta perawat memindahkan Pak Budi, karena lewat `RWI-DEC-012` perawat
pelaksana boleh memindahkan pasien tanpa izin siapa pun. Akibatnya nama yang tercatat sebagai
pelaku perpindahan adalah nama perawat, bukan nama dokter yang sebenarnya memutuskan.
Pembatasan terhadap dokter karena itu tidak mengunci tindakannya, melainkan hanya menentukan
nama siapa yang muncul di riwayat.


### `RWI-RULE-017` — Visite dokter

Dasar keputusan: `RWI-DEC-025`. Menutup `RWI-OQ-019` dan `RWI-GAP-007`.

**Tidak ada formulir visite tersendiri.** Visite tercatat sebagai akibat dari catatan
perkembangan yang ditulis dokter, bukan sebagai isian terpisah.

| Butir | Ketentuan |
|---|---|
| Apa yang menandai satu visite | Catatan perkembangan pasien yang ditulis dokter (CPPT dokter atau SOAP) pada hari itu |
| Siapa yang mencatat | Dokter sendiri. Bukan perawat, bukan petugas admisi |
| Yang tersimpan | Waktu penulisan catatan, nama dokter, dan isi catatannya |
| Formulir visite terpisah | Tidak ada, dan tidak akan dibuat |
| Kunjungan dokter tanpa catatan | Tidak terhitung sebagai visite |

Alasan aturan ini sama persis dengan alasan membuang `InCare` pada `RWI-DEC-009`: fakta yang
sama tidak disimpan di dua tempat. Dokter cukup mengisi satu kali untuk satu kunjungan, dan
tidak ada dua sumber angka yang bisa saling bertentangan ketika diaudit.

Contoh nyata:

> Pukul **07:40** dr. Andi memeriksa Pak Budi di kamarnya, lalu menulis catatan perkembangan
> pukul **07:52**. Sistem mencatat satu visite dr. Andi untuk Pak Budi pada tanggal itu,
> dengan waktu 07:52. Tidak ada tombol lain yang perlu ditekan dr. Andi.
>
> Pukul **16:10** dr. Andi lewat dan menengok Pak Budi sebentar tanpa menulis apa pun.
> Kunjungan itu **tidak** tercatat sebagai visite, karena tidak meninggalkan catatan yang bisa
> dibuktikan.

#### Cara menghitung jumlah visite

Dasar: `RWI-DEC-031`. Menutup `RWI-OQ-030`.

Aturannya: **satu visite per dokter per tanggal.**

| Keadaan | Jumlah visite | Waktu yang dipakai |
|---|---|---|
| Satu dokter menulis satu catatan pada satu tanggal | 1 | Waktu catatan itu |
| Satu dokter menulis dua catatan atau lebih pada tanggal yang sama | Tetap 1 | Waktu catatan **pertama** |
| Dua dokter berbeda menulis pada tanggal yang sama | 2, satu untuk tiap dokter | Waktu catatan pertama masing-masing dokter |

Alasannya sama dengan pilihan cara menghitung lama dirawat pada `RWI-RULE-019`: penjamin
memakai hitungan ini, sehingga angka di sistem tidak berselisih dengan angka pihak luar.

Contoh nyata:

> **12 Agustus.** dr. Andi visite pagi dan menulis catatan pukul **07:52**. Sore pukul
> **16:30** kondisi Pak Budi memburuk, dr. Andi datang lagi dan menulis catatan kedua.
> Jumlah visite dr. Andi hari itu tetap **1**, dengan waktu 07:52. Catatan pukul 16:30 tetap
> tersimpan lengkap sebagai catatan perkembangan, hanya tidak menambah hitungan visite.
>
> **13 Agustus.** dr. Andi menulis catatan pukul **08:10**, dan dr. Sinta selaku konsulen
> jantung menulis catatan pukul **11:00**. Hari itu tercatat **2** visite, satu untuk tiap
> dokter.


### `RWI-RULE-018` — Daftar periksa administrasi sebelum episode ditutup

Dasar keputusan: `RWI-DEC-026`. Menutup `OQ-RI-007`, dan melengkapi syarat ketiga pada
`RWI-RULE-010` yang sebelumnya hanya berbunyi "clearance administrasi sudah terpenuhi" tanpa
isi.

| Butir | Ketentuan |
|---|---|
| Bentuk | Daftar periksa yang butir-butirnya disimpan sebagai master data |
| Siapa yang mengatur isinya | Admin rumah sakit, tanpa perlu mengubah program |
| Siapa yang menandai selesai | Petugas admisi |
| Sifat | **Menahan.** Penutupan episode ditolak selama masih ada butir wajib yang belum ditandai, dan pesan penolakannya menyebut butir mana yang belum beres |
| Yang tersimpan tiap penandaan | Nama petugas dan waktu penandaan |

Butir bawaan yang dikirim bersama MVP, dan boleh diubah rumah sakit:

1. tidak ada berkas rekam medis episode ini yang masih berstatus draft;
2. barang milik rumah sakit sudah dikembalikan;
3. surat-surat yang diminta pasien sudah diserahkan.

Yang **sengaja tidak** dimasukkan ke daftar ini, karena sudah dijaga sebagai syarat tersendiri
pada `RWI-RULE-010`: keputusan pulang dari DPJP, resume pulang, kelayakan keuangan, dan
keberadaan tempat tidur aktif. Memasukkannya dua kali hanya akan membuat penguji bingung
syarat mana yang sebenarnya menolak.

Contoh nyata:

> Pukul **13:15** petugas admisi menutup episode Ibu Sari. Sistem menolak dengan pesan bahwa
> butir "barang milik rumah sakit sudah dikembalikan" belum ditandai.
>
> Pukul **13:22** perawat mengembalikan tabung oksigen portabel ke ruang penyimpanan, petugas
> admisi menandai butir itu selesai, dan sistem menyimpan nama petugas beserta waktunya.
> Penutupan diulang dan berhasil.
>
> Bulan berikutnya rumah sakit menambah satu butir baru, "kartu penunggu pasien sudah
> dikembalikan", lewat pengaturan master data. Butir itu langsung berlaku untuk penutupan
> berikutnya tanpa ada program yang diubah.

**Konsekuensi.** Ada satu master data baru yang harus disiapkan sebelum modul dipakai. Bila
admin mengisi daftar terlalu panjang, penutupan menjadi lambat dan tempat tidur berputar lebih
pelan. Risiko itu berada di tangan admin rumah sakit, bukan pada program.


### `RWI-RULE-019` — Cara menghitung lama dirawat

Dasar keputusan: `RWI-DEC-027`. Menutup `RWI-OQ-020` dan `RWI-GAP-008`.

Rumusnya: **lama dirawat = tanggal pulang dikurangi tanggal masuk, dengan hasil paling sedikit
1 hari.** Bagian jam tidak ikut dihitung sama sekali; yang dipakai hanya bagian tanggalnya.

| Keadaan | Cara hitung | Contoh |
|---|---|---|
| Episode sudah ditutup | Tanggal pulang dikurangi tanggal masuk | Masuk 12 Agustus pukul 22:40, pulang 15 Agustus pukul 09:00 → **3 hari** |
| Masuk dan pulang pada tanggal yang sama | Selalu 1 hari, bukan 0 | Masuk 12 Agustus pukul 06:00, pulang 12 Agustus pukul 20:00 → **1 hari** |
| Episode masih berjalan, tampil di census | Tanggal hari ini dikurangi tanggal masuk, paling sedikit 1 hari | Masuk 12 Agustus, hari ini 14 Agustus → **2 hari** |

Alasan memilih cara ini: penjamin dan pelaporan rumah sakit memakai hitungan yang sama,
sehingga angka pada layar census identik dengan angka yang dipakai pihak luar. Rumah sakit
tidak perlu menjelaskan selisih antara dua versi angka untuk pasien yang sama.

Contoh nyata:

> Pak Budi masuk **12 Agustus pukul 22:40**. Pada census tanggal **13 Agustus** angkanya
> tertulis 1 hari, pada tanggal **14 Agustus** menjadi 2 hari, dan seterusnya. Angka itu naik
> setiap pergantian tanggal, **bukan** setiap genap 24 jam.
>
> Pak Budi pulang **15 Agustus pukul 09:00**. Lama dirawatnya tercatat **3 hari**, walaupun
> waktu sebenarnya 58 jam 20 menit.
>
> Ibu Rina masuk **12 Agustus pukul 06:00** dan pulang hari itu juga pukul **20:00**. Lama
> dirawatnya tercatat **1 hari**, bukan 0 hari.

**Konsekuensi yang perlu diwaspadai.** Pasien yang hanya dirawat dua jam tetap terhitung 1
hari. Angka ini adalah hitungan **hari rawat**, bukan ukuran lama waktu sebenarnya, dan layar
wajib menyebutkan hal itu dengan jelas agar tidak disalahartikan pembaca. Kata persis yang
dipakai diserahkan kepada pelaksana; lihat `RWI-FE-001`.


### `RWI-RULE-020` — Membuka kembali episode yang sudah ditutup

Dasar keputusan: `RWI-DEC-028`. Menutup `OQ-RI-012`.

Membuka kembali episode adalah cara **memperbaiki catatan masa lalu**, bukan cara melanjutkan
perawatan. Dua hal itu sering dikira sama, padahal hanya yang pertama membutuhkan reopen, dan
yang pertama tidak membutuhkan tempat tidur.

| Butir | Ketentuan |
|---|---|
| Siapa yang boleh | Hanya Supervisor |
| Untuk keperluan apa | Melengkapi atau membetulkan dokumen episode: resume pulang, diagnosis, cara pulang, dan catatan lain |
| Alasan | Wajib diisi. Reopen tanpa alasan ditolak |
| Tempat tidur | **Tidak** dikembalikan. Episode yang dibuka kembali tidak menempati tempat tidur mana pun |
| Census | **Tidak** muncul. Pasien yang sudah pulang tidak boleh terlihat sedang dirawat |
| Lama dirawat | **Tidak** berubah. Reopen tidak menambah hari rawat |
| Setelah perbaikan selesai | Episode ditutup kembali |
| Pasien yang benar-benar kembali dirawat | Selalu mendapat episode baru dan kunjungan baru, termasuk pasien yang sebelumnya kabur lalu datang lagi |

Yang wajib disimpan setiap kali reopen: nama supervisor, waktu dibuka, alasan, waktu ditutup
kembali, dan apa saja yang berubah.

Contoh nyata:

> **Koreksi catatan.** Episode Ibu Sari ditutup **15 Agustus** dengan cara pulang "kabur".
> Tanggal **17 Agustus** keluarganya datang membawa surat pernyataan pulang atas permintaan
> sendiri yang ternyata sudah ditandatangani sebelum Ibu Sari pergi.
>
> Supervisor membuka kembali episode itu dengan alasan "koreksi cara pulang, pernyataan APS
> ditemukan", mengubah cara pulang menjadi APS, lalu menutupnya lagi. Bed `MELATI-03` sama
> sekali tidak terganggu: sejak 15 Agustus sudah ditempati pasien lain dan tetap begitu. Lama
> dirawat Ibu Sari tetap tercatat 3 hari.
>
> **Bukan koreksi.** Bila Ibu Sari datang lagi tanggal **20 Agustus** karena sakitnya kambuh,
> itu **bukan** reopen. Ia menjalani admisi baru dengan episode baru dan kunjungan baru,
> mengikuti `RWI-RULE-005`.


### `RWI-RULE-021` — Batas waktu pengkajian awal dan verifikasi CPPT — **BELUM FINAL**

Dasar keputusan: `RWI-DEC-029`. Menjawab `RWI-OQ-018` dan `RWI-GAP-006`, tetapi **belum**
berstatus `approved`. Baca peringatan di akhir bagian ini sebelum memakainya.

| Yang diukur | Target | Dihitung sejak | Sifat |
|---|---|---|---|
| Pengkajian awal keperawatan | 24 jam | Pasien menempati tempat tidur, yaitu saat episode menjadi `Admitted` | **Tidak menahan apa pun** |
| Verifikasi CPPT oleh DPJP | 24 jam | Catatan CPPT ditulis | **Tidak menahan apa pun** |

Kedua angka 24 jam disimpan sebagai parameter yang bisa diubah admin, sama seperti batas
pemesanan tempat tidur pada `RWI-RULE-002`.

Yang terjadi bila batas terlewat: episode itu muncul pada daftar pantau kepatuhan beserta lama
keterlambatannya, dan angkanya dapat dilaporkan saat akreditasi. **Tidak ada satu pun tindakan
yang dihalangi.** Perawat tetap bisa mengkaji, dokter tetap bisa menulis instruksi dan resep,
dan episode tetap bisa ditutup.

**Penegasan.** Sifat tidak menahan itu disengaja, supaya aturan ini tidak melanggar
`RWI-DEC-009` yang sudah `approved`: status dan kelengkapan dokumen tidak boleh dipakai sebagai
syarat sebelum dokumentasi klinis boleh ditulis.

Contoh nyata:

> Ibu Sari menempati bed `MELATI-03` pukul **22:40** tanggal 12 Agustus. Batas pengkajian awal
> jatuh pukul **22:40 tanggal 13 Agustus**.
>
> Perawat mengisi pengkajian awal pukul **01:15** tanggal 13 Agustus, jauh sebelum batas. Tidak
> ada apa pun yang muncul di daftar pantau.
>
> Bandingkan: bila pengkajian baru diisi pukul **06:30 tanggal 14 Agustus**, episode Ibu Sari
> muncul pada daftar pantau kepatuhan dengan keterangan terlambat 7 jam 50 menit. Meski begitu,
> sepanjang waktu itu dokter tetap boleh menulis instruksi dan resep untuk Ibu Sari.

**Bentuk daftar pantau kepatuhan belum diputuskan**, dan dicatat sebagai `RWI-OQ-031`.

> **PERINGATAN — aturan ini belum boleh dipakai melayani pasien sungguhan.** Batas waktu
> pengkajian awal dan kewajiban verifikasi CPPT adalah aturan klinis dan akreditasi. Angka 24
> jam yang tertulis di atas berasal dari praktik akreditasi yang lazim, **bukan** dari
> persetujuan komite klinis. Sesuai `RWI-DEC-006`, pemegang sementara tidak berwenang
> menutupnya, sehingga status keputusan tetap `draft` sampai pemilik klinis ditunjuk dan
> meninjau baris ini. Karena belum `approved`, tidak ada acceptance criteria yang ditulis untuk
> aturan ini. Lihat Gate Sebelum Produksi.


### `RWI-RULE-022` — Episode `Draft` yang ditinggalkan

Dasar keputusan: `RWI-DEC-030`. Menutup `RWI-OQ-028`.

| Butir | Ketentuan |
|---|---|
| Batas | 1 hari sejak episode terakhir disentuh |
| Cara menghitung | Dihitung saat data dibaca, sama persis seperti `RWI-RULE-002`. Tidak ada program penjadwal yang berjalan di latar belakang |
| Yang terjadi setelah lewat batas | Episode terbaca `Cancelled` dengan alasan sistem "kedaluwarsa, tidak pernah diaktifkan" |
| Kunjungan yang terlanjur dibuat | Ikut ditandai batal, sehingga tidak muncul sebagai kunjungan rawat inap yang benar-benar terjadi |
| Tempat tidur | Tidak ada yang perlu dilepas. Pemesanannya sudah gugur sendiri setelah 2 jam lewat `RWI-RULE-002` |
| Angka 1 hari | Parameter yang bisa diubah admin tanpa mengubah program |

**Hubungan dengan `RWI-RULE-004`.** Pembatalan otomatis ini tidak melanggar aturan pembatalan
manual. `RWI-RULE-004` mengatur pembatalan **oleh manusia** dan mewajibkan alasan diisi orang.
Pembatalan otomatis hanya berlaku pada episode `Draft`, yang menurut definisinya belum pernah
aktif dan karena itu belum mungkin punya catatan klinis apa pun. Syarat "tidak ada catatan
klinis" pada `RWI-RULE-004` dengan sendirinya terpenuhi.

Contoh nyata:

> Pukul **09:15** tanggal 12 Agustus petugas membuka admisi Ibu Sari lewat jalur pasien datang
> langsung. Sistem membuat kunjungan rawat inap, episode berstatus `Draft`, dan bed
> `MELATI-03` dipesan.
>
> Pukul **11:15** pemesanan bed gugur sendiri. `MELATI-03` kembali `Available` dan dipakai
> pasien lain sore itu. Episode Ibu Sari tetap `Draft`, karena ternyata Ibu Sari batal datang
> dan tidak ada yang memberi tahu petugas.
>
> Pukul **09:20** tanggal 13 Agustus seseorang membuka daftar admisi. Sistem menghitung bahwa
> episode itu sudah lewat 1 hari sejak terakhir disentuh, sehingga episode terbaca `Cancelled`
> dan kunjungan rawat inap Ibu Sari ikut ditandai batal. Laporan kunjungan bulan itu tidak
> berisi kunjungan rawat inap Ibu Sari yang tidak pernah terjadi.


### `RWI-RULE-023` — Daftar pantau dan penanggung jawabnya

Dasar keputusan: `RWI-DEC-032`. Menutup `RWI-OQ-027` dan `RWI-OQ-031`.

Modul ini menumbuhkan tiga daftar pantau selama wawancara. Ketiganya kini punya pemilik yang
jelas, karena daftar tanpa pemilik biasanya tidak dibaca siapa pun.

| Daftar pantau | Lahir dari | Isinya | Penanggung jawab | Ambang "terlambat" |
|---|---|---|---|---|
| Penutupan tertunda | `RWI-DEC-016` | Episode `DischargePending` yang belum ditutup | Petugas admisi | 4 jam sejak episode menjadi `DischargePending` |
| Kepatuhan pengkajian dan CPPT | `RWI-DEC-029` | Episode yang melewati batas waktu pengkajian awal atau verifikasi CPPT | Kepala ruangan | Mengikuti `RWI-RULE-021`, yaitu 24 jam |
| Penutupan tanpa kelayakan keuangan | `RWI-RULE-009` | Episode yang ditutup Supervisor tanpa status `Cleared` | Supervisor | Ditinjau berkala, tanpa ambang per baris |

Yang berlaku untuk ketiganya:

1. Ambang waktu disimpan sebagai parameter yang bisa diubah admin, sama seperti batas
   pemesanan tempat tidur pada `RWI-RULE-002`.
2. Baris yang melewati ambang ditandai terlambat beserta lama keterlambatannya, bukan sekadar
   muncul di daftar.
3. **Tidak satu pun daftar ini menahan tindakan apa pun.** Sifatnya memantau, bukan menggerbang.
   Yang menggerbang hanya `RWI-RULE-009`, `RWI-RULE-010`, dan `RWI-RULE-018`.
4. Bentuk tampilannya — satu halaman gabungan atau tiga halaman terpisah, urutan kolom, dan
   cara menandai keterlambatan — diserahkan kepada pelaksana; lihat `RWI-FE-002`.

Contoh nyata:

> dr. Andi menyatakan Ibu Sari boleh pulang pukul **08:30**, sehingga episodenya menjadi
> `DischargePending`. Ambang penutupan tertunda jatuh pukul **12:30**.
>
> Pukul **13:10** episode itu belum juga ditutup karena kelayakan keuangannya masih `Blocked`.
> Episode Ibu Sari muncul pada daftar penutupan tertunda milik petugas admisi dengan keterangan
> terlambat 40 menit. Bed `MELATI-03` masih tercatat ditempati, sehingga keterlambatan itu
> langsung terlihat sebagai tempat tidur yang tidak berputar.
>
> Pukul **13:40** supervisor menutup episode lewat jalan keluar pada `RWI-RULE-009`. Baris itu
> hilang dari daftar penutupan tertunda, dan muncul di daftar penutupan tanpa kelayakan
> keuangan milik supervisor.

**Konsekuensi.** Butuh tiga parameter ambang baru. Bila peran penanggung jawabnya kosong pada
sif tertentu — misalnya kepala ruangan tidak bertugas malam — daftarnya menganggur sampai sif
berikutnya. Pemilik kebutuhan menerima ini karena ketiga daftar bersifat memantau dan tidak
menghalangi perawatan pasien.


### `RWI-RULE-024` — Obat pulang

Dasar keputusan: `RWI-DEC-033`. Menutup `RWI-OQ-021` dan `RWI-GAP-009`.

Obat pulang **bukan kemampuan baru**. Ia adalah jenis resep pada kemampuan resep yang sudah
masuk scope, ditambah satu butir pada daftar periksa yang sudah ada.

| Butir | Ketentuan |
|---|---|
| Bentuknya | Jenis resep pada CAP-023, ditandai sebagai obat pulang |
| Pengiriman ke Farmasi | Sama seperti resep harian: dikirim dengan konteks pasien dan encounter yang sama |
| Status penyerahan | Dibaca balik dari Farmasi, mengikuti titik sentuh yang sudah disepakati |
| Kaitan dengan penutupan episode | Satu butir pada daftar periksa administrasi `RWI-RULE-018`, berbunyi "obat pulang sudah diserahkan". **Bukan** gerbang tersendiri |
| Bila rumah sakit belum menghendakinya menahan | Butir itu dinonaktifkan admin lewat master data, tanpa mengubah program |
| Bila Farmasi belum bisa mengembalikan status penyerahan | Butir ditandai manual oleh petugas admisi, sama seperti butir daftar periksa lainnya |

Yang **tetap di luar scope**: penyiapan, peracikan, dan review obat. Ketiganya milik
`PharmacyManagement` sesuai daftar Di luar scope.

Contoh nyata:

> dr. Andi menyatakan Ibu Sari boleh pulang pukul **08:30** dan menulis resep obat pulang
> berisi antibiotik untuk lima hari. Resep itu ditandai sebagai obat pulang lalu terkirim ke
> Farmasi dengan konteks encounter yang sama seperti resep harian Ibu Sari selama dirawat.
>
> Pukul **12:50** Farmasi menyerahkan obat kepada keluarga, dan status penyerahan terbaca di
> modul Rawat Inap. Butir "obat pulang sudah diserahkan" pada daftar periksa administrasi
> tertandai selesai dengan sendirinya.
>
> Pukul **13:15** petugas admisi menutup episode. Bila obat belum diserahkan, penutupan
> tertahan pada butir itu — bukan pada gerbang baru, melainkan pada daftar periksa yang sudah
> ada sejak `RWI-DEC-026`.


> **Dilengkapi pada 2026-08-21 oleh `RWI-DEC-046`.** Penanda "resep ini obat pulang" disimpan
> sebagai **jenis resep pada tabel resep milik Farmasi**, bukan sebagai daftar terpisah milik
> Rawat Inap. Alasannya: yang menyiapkan dan menyerahkan obat adalah petugas farmasi, dan mereka
> perlu melihat penanda itu di layar mereka sendiri untuk memberi edukasi obat pulang. Bila
> penandanya disimpan di Rawat Inap, layar Farmasi tidak akan menampilkannya.
>
> Contoh: dr. Andi menulis dua resep pada 25 September untuk Tn. Budi — satu resep harian dan satu
> resep obat pulang. Di layar Farmasi keduanya muncul, dan yang bertanda obat pulang ditampilkan
> berbeda supaya petugas tahu harus menyertakan penjelasan cara minum di rumah. Setelah
> diserahkan, statusnya dibaca balik oleh Rawat Inap dan menutup butir daftar periksa
> "obat pulang sudah diserahkan" pada `RWI-RULE-018`.
>
> Perubahan ini menyentuh modul `PharmacyManagement`, dan persetujuannya sudah tercakup pada
> `RWI-OQ-032` yang memang sudah dibuka untuk modul yang sama.

### `RWI-RULE-025` — Persetujuan umum saat masuk rawat inap — **BELUM FINAL**

Dasar keputusan: `RWI-DEC-035`. Menutup `OQ-RI-009`, tetapi **belum** berstatus `approved`.
Baca peringatan di akhir bagian ini sebelum memakainya.

| Butir | Ketentuan |
|---|---|
| Apa yang wajib | Satu **persetujuan umum rawat inap** per episode |
| Isi minimalnya | (1) persetujuan tindakan kedokteran umum; (2) persetujuan pemberian informasi kepada penjamin; (3) penunjukan orang yang boleh menerima informasi tentang pasien |
| Disimpan di mana | `PatientConsent` yang sudah ada, terhubung ke encounter episode itu |
| Menahan admisi? | **Tidak.** Pasien tetap dapat diterima dan ditempatkan walaupun belum ada yang menandatangani |
| Menahan penutupan? | **Ya**, lewat satu butir pada daftar periksa administrasi `RWI-RULE-018` yang berbunyi "persetujuan umum sudah ditandatangani" |
| Persetujuan tindakan khusus per tindakan | Tetap **di luar scope**, mengikuti CAP-009 |

Alasan menahan penutupan dan bukan admisi: menerima pasien tidak boleh tertahan urusan tanda
tangan, tetapi pasien juga tidak boleh pulang tanpa persetujuan pernah diambil. Mengejar tanda
tangan selama pasien dirawat jauh lebih mungkin berhasil daripada di menit pertama kedatangan.

Contoh nyata:

> Pukul **02:15** IGD mengirim Pak Budi dalam keadaan tidak sadar, tanpa keluarga yang bisa
> dihubungi. Admisi tetap berjalan, bed `MELATI-03` ditempati, dan perawatan dimulai saat itu
> juga. Butir persetujuan umum pada daftar periksa masih kosong.
>
> Pukul **09:40** anak Pak Budi tiba dan menandatangani persetujuan umum. Butir itu tertandai
> selesai.
>
> Bila sampai hari kepulangan tidak ada seorang pun yang menandatangani, penutupan episode
> **tertahan** pada butir itu, dan penyelesaiannya naik ke Supervisor.

> **PERINGATAN — aturan ini belum boleh dipakai melayani pasien sungguhan.** Persetujuan umum
> menyangkut kewajiban hukum dan perlindungan data pasien. Sesuai `RWI-DEC-006`, pemegang
> sementara **tidak berwenang** menutup keputusan privasi. Jeda waktu ketika pasien dirawat
> tanpa persetujuan tertulis adalah risiko hukum yang harus diterima secara sadar oleh pemilik
> keamanan/privasi, bukan oleh agent dan bukan oleh pemegang sementara. Status keputusan tetap
> `draft`, dan karena belum `approved` tidak ada acceptance criteria yang ditulis untuk aturan
> ini. Lihat Gate Sebelum Produksi.

### `RWI-RULE-026` — Dokumentasi klinis rawat inap memakai mesin klinis yang sudah ada

**Tujuan.** Satu pasien hanya boleh punya satu tempat rekam medis. Catatan perawatan menginap
harus masuk ke tabel yang sama dengan catatan rawat jalan, bukan ke tabel tandingan milik Rawat
Inap.

**Keadaan yang menjadi masalah.** Mesin klinis yang ada hari ini dibuat untuk pasien poliklinik
yang datang sekali, ambil nomor antrean, diperiksa, lalu pulang. Karena itu ada tiga pembatas
yang tidak cocok untuk pasien menginap, sesuai `RWI-FACT-011` dan `RWI-FACT-012`:

1. pengkajian dan konsultasi mewajibkan baris antrean yang benar-benar ada;
2. diagnosis, tindakan, dan resep mewajibkan baris konsultasi;
3. satu kunjungan hanya boleh punya satu konsultasi, dan satu konsultasi hanya boleh punya satu
   resep aktif.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Rawat Inap **tidak membuat** tabel pengkajian, catatan dokter, diagnosis, tindakan, atau resep tandingan. Seluruhnya memakai tabel yang sudah ada |
| 2 | Rawat Inap **tidak membuat antrean semu**. Pasien menginap tidak masuk daftar antrean poliklinik, dan laporan antrean poliklinik tidak boleh tercemar baris rawat inap |
| 3 | Untuk kunjungan bertipe rawat inap **atau IGD**, keharusan mengisi antrean dan konsultasi **dilonggarkan** sehingga catatan boleh menempel langsung pada kunjungan |
| 4 | Untuk kunjungan bertipe rawat inap **atau IGD**, batas "satu konsultasi per kunjungan" **dilonggarkan** menjadi banyak catatan dokter selama episode atau kunjungan itu berlangsung |
| 5 | Untuk kunjungan bertipe rawat inap **atau IGD**, batas "satu resep aktif per konsultasi" **dilonggarkan** sehingga dokter dapat menulis resep lebih dari sekali dalam satu kunjungan |
| 6 | Pelonggaran hanya berlaku bila kunjungan bertipe rawat inap (`Inpatient`) **atau IGD** (`Emergency`). Perilaku untuk rawat jalan dan medical check-up **tidak boleh berubah sedikit pun**. **Direvisi 2026-08-24 oleh `RWI-DEC-070`:** IGD dikeluarkan dari daftar yang tidak boleh berubah, karena pembatas yang sama membuat pengkajian IGD mustahil disimpan |

**Prasyarat yang tidak boleh dilewatkan.** Pelonggaran ini disaring dari **tipe kunjungan**.
Hari ini kunjungan IGD bertipe `Outpatient`, sama persis dengan poliklinik, sehingga menyalakan
pelonggaran atas dasar tipe justru akan ikut melonggarkan poliklinik. Karena itu `RWI-DEC-070`
hanya dapat dijalankan setelah `IGD-DEC-074` berlaku, yaitu setelah kunjungan IGD benar-benar
bertipe `Emergency` dan data lama diperbaiki. Pekerjaan itu ada di sisi IGD, bukan di sini.

**Contoh konkret.** Tn. Budi dirawat lima hari, tanggal 1 sampai 5 September 2026, dan diperiksa
dokter setiap hari.

| Hari | Yang terjadi hari ini | Yang terjadi setelah aturan ini berlaku |
| --- | --- | --- |
| 1 | Catatan dokter pertama bisa dibuat, resep pertama bisa dibuat | Sama |
| 2 | **Ditolak** dengan pesan "Konsultasi dokter untuk encounter ini sudah ada." | Catatan dokter kedua dibuat, resep hari kedua dibuat |
| 3 s.d. 5 | **Ditolak** dengan pesan yang sama | Berjalan normal, sehingga episode Tn. Budi berisi 5 catatan dokter dan 5 resep |

**Yang belum boleh dianggap selesai.** Aturan ini menyentuh dua modul yang bukan milik Rawat
Inap, yaitu `ClinicalManagement` dan `PharmacyManagement`, keduanya berstatus `ACTIVE` pada
registry. Persetujuan pemiliknya dicatat sebagai `RWI-OQ-032` dan **sudah diberikan**
2026-08-21 lewat `RWI-DEC-062`: kedua modul berada di bawah kepemilikan Muhammad Hamzah.
Yang tersisa bukan lagi persetujuan, melainkan prasyarat teknis `IGD-DEC-074` di atas dan
test regresi yang dituntut `RWI-DEC-051`.

**Risiko yang harus diterima secara sadar.** Perubahan ini menyentuh alur poliklinik yang sudah
melayani pasien, sedangkan menurut `01-existing-capability-map.md` bagian 11.2 butir 5, **tidak
ada satu pun test** yang menyentuh pengkajian, konsultasi, maupun resep. Artinya tidak ada jaring
pengaman yang memberi tahu bila poliklinik rusak akibat perubahan ini. Risiko ini dicatat sebagai
`RWI-RISK-002`.

### `RWI-RULE-027` — Sumber kebenaran penghunian tempat tidur

**Tujuan.** Sistem harus selalu bisa menjawab tiga pertanyaan untuk setiap tempat tidur: sedang
dipakai atau tidak, oleh pasien siapa, dan sejak kapan. Hari ini sistem hanya bisa menjawab
pertanyaan pertama, dan jawabannya pun tidak dapat dipercaya.

**Keadaan yang menjadi masalah.** Kolom `MstBed.BedStatus` dapat disetel siapa saja yang punya
hak `Bed : Update` lewat menu master data, tanpa menyebut pasien, tanpa waktu mulai, dan tanpa
pemeriksaan tabrakan. Tidak ada satu pun tempat lain di dalam sistem yang menulis kolom itu.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | **Catatan penempatan** milik Rawat Inap adalah satu-satunya sumber kebenaran tentang siapa menempati tempat tidur mana dan sejak kapan |
| 2 | Kolom `MstBed.BedStatus` tetap disimpan, tetapi kedudukannya turun menjadi **salinan** dari catatan penempatan, bukan sumber |
| 3 | Setiap pemesanan, penempatan, perpindahan, dan pelepasan menulis catatan penempatan **dan** memperbarui `BedStatus` **dalam satu transaksi yang sama**. Berhasil dua-duanya, atau tidak ada yang berubah sama sekali |
| 4 | Nilai `Reserved` dan `Occupied` **tidak boleh lagi** disetel manusia lewat endpoint master data. Nilai itu hanya boleh lahir dari tindakan Rawat Inap |
| 5 | Nilai yang tetap menjadi wewenang admin master data adalah `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive`, yaitu keadaan yang tidak menyangkut pasien |
| 6 | Disediakan satu **laporan selisih** yang menampilkan tempat tidur yang kolom statusnya tidak cocok dengan catatan penempatannya, supaya kesalahan dapat ditemukan dan bukan hanya dicurigai |

**Contoh konkret.** Bed `BD-RSMMC-00042` di kamar Melati 3B.

| Waktu | Tindakan | Catatan penempatan | Kolom `BedStatus` |
| --- | --- | --- | --- |
| 09:15 | Petugas admisi memesan bed untuk Tn. Budi | Baris pemesanan dibuat, berlaku sampai 11:15 | `Reserved` |
| 10:40 | Admisi diaktifkan, Tn. Budi berbaring | Baris penempatan dibuat, mulai 10:40, belum berakhir | `Occupied` |
| Hari ke-3 | Tn. Budi pindah ke Anggrek 1A | Baris lama diberi waktu berakhir, baris baru dibuat untuk bed Anggrek | Bed Melati `Available`, bed Anggrek `Occupied` |
| Hari ke-5 | Episode ditutup | Baris penempatan diberi waktu berakhir | `Available` |

Contoh kegagalan yang dicegah aturan ini: seandainya admin master data menyetel bed Melati
menjadi `Available` pada hari ke-2 padahal Tn. Budi masih berbaring di sana, aturan nomor 4
menolak tindakan itu. Seandainya selisih tetap terjadi karena sebab lain, laporan pada aturan
nomor 6 menampilkannya sebagai satu baris: "bed `BD-RSMMC-00042` tertulis `Available`, tetapi
masih ada penempatan aktif atas nama Tn. Budi sejak 21 Agustus 2026 pukul 10:40".

**Kenapa kolom `BedStatus` tidak dihapus saja.** Karena seluruh pembaca yang sudah ada masih
membacanya: daftar bed, ringkasan 13 angka, isian pilihan bed, dan layar master tempat tidur di
frontend. Mempertahankan kolom membuat semua itu tetap jalan tanpa diubah.

**Pelajaran yang mendasari aturan nomor 3 dan 6.** Audit modul Billing menemukan penanda
`IsBillingGenerated` pada tindakan pasien yang dapat berkata "sudah ditagih" tanpa ada tagihan
yang benar-benar dibuat, dan penanda itu diberi status `Repair`. Aturan nomor 3 mencegah pola itu
terulang dengan mengikat salinan pada satu transaksi yang sama, dan aturan nomor 6 menyediakan
cara menemukannya bila tetap lolos.

**Yang belum boleh dianggap selesai.** Aturan nomor 4 dan 5 menyentuh modul `MasterData` yang
bukan milik Rawat Inap. Persetujuan pemiliknya dicatat sebagai `RWI-OQ-033` dan **memblokir
implementasi**, walaupun tidak memblokir desain.

### `RWI-RULE-028` — Sumber status kelayakan keuangan selama Billing belum siap

**Tujuan.** Gerbang keuangan pada `RWI-RULE-009` tetap dapat ditegakkan hari ini, tanpa menunggu
modul `BillingManagement` punya kemampuan transaksi, dan tanpa membuat jalan keluar supervisor
berubah menjadi jalur normal.

**Keadaan yang menjadi masalah.** `RWI-DEC-015` mengunci bahwa hanya status `Cleared` yang
membuka penutupan episode. Namun `BillingManagement` hari ini hanya berisi dua tabel master dan
satu service kosong, sehingga nilai `Pending`, `Cleared`, dan `Blocked` tidak ada sumbernya.
Bila dibiarkan, setiap penutupan akan tertahan dan setiap penutupan harus lewat supervisor.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Status kelayakan keuangan disimpan **pada episode rawat inap**, bukan dibaca dari modul lain, selama `BillingManagement` belum punya kemampuan transaksi |
| 2 | Nilai yang dipakai tetap tiga: `Pending`, `Cleared`, dan `Blocked`. Nilai bawaan saat episode dibuat adalah `Pending` |
| 3 | Yang berwenang menandai adalah **petugas kasir atau petugas billing**. Petugas admisi, perawat, dan dokter tidak berwenang |
| 4 | Setiap penandaan wajib menyimpan **siapa yang menandai, kapan, dan catatan singkat**. Penandaan tanpa ketiganya ditolak |
| 5 | Penandaan bersifat **sementara** dan wajib ditampilkan sebagai penandaan manual pada layar dan laporan, supaya tidak terbaca seolah berasal dari tagihan yang sungguh ada |
| 6 | `RWI-DEC-015` dan `RWI-RULE-009` **tetap berlaku utuh**. Gerbang tetap memblokir, dan jalan keluar supervisor tetap merupakan pengecualian |
| 7 | Ketika `BillingManagement` kelak punya kemampuan transaksi, sumber nilainya berpindah dari penandaan manual menjadi bacaan dari Billing. **Aturan penutupan tidak berubah**, hanya sumber datanya |

**Contoh konkret.** Tn. Budi dinyatakan boleh pulang oleh DPJP pada 25 September 2026 pukul 09:00.

| Waktu | Yang terjadi | Status kelayakan keuangan | Bisakah episode ditutup |
| --- | --- | --- | --- |
| Saat admisi, 21 Sept | Episode dibuat | `Pending` bawaan | Tidak |
| 25 Sept 09:00 | DPJP menyatakan boleh pulang | Masih `Pending` | Tidak |
| 25 Sept 10:30 | Kasir memeriksa tagihan, keluarga belum melunasi | Kasir menandai `Blocked`, catatan "menunggu pelunasan keluarga" | Tidak |
| 25 Sept 13:15 | Keluarga melunasi | Kasir menandai `Cleared`, catatan "lunas tunai" | Ya |

Contoh jalur tidak normal: seandainya kasir sedang tidak di tempat dan pasien harus segera
pulang, supervisor tetap dapat menutup episode dengan alasan wajib sesuai `RWI-RULE-009`. Episode
itu ditandai dan masuk laporan penutupan tanpa kelayakan keuangan. Karena penandaan manual sudah
tersedia, jalur ini tetap jarang dipakai dan laporannya tetap berguna.

**Risiko yang harus diterima secara sadar.** Petugas kasir dapat menandai `Cleared` tanpa ada
tagihan yang sungguh dibuat, karena hari ini memang belum ada tagihan yang bisa diperiksa sistem.
Ini pola yang sama dengan penanda `IsBillingGenerated` pada modul Billing yang diberi status
`Repair` oleh audit. Yang membedakan di sini: penandaan menyimpan pelaku dan waktu, ditampilkan
sebagai penandaan manual, dan bersifat sementara. Risiko ini dicatat sebagai `RWI-RISK-003` dan
hanya hilang setelah aturan nomor 7 dijalankan.

### `RWI-RULE-029` — Serah terima dari IGD ke rawat inap

**Tujuan.** Pasien IGD yang diputuskan rawat inap dapat berpindah ke bangsal tanpa petugas harus
mendaftarkan ulang pasien secara manual, dan tanpa riwayat pasien terputus.

**Keadaan yang menjadi masalah.** Hari ini IGD hanya menyimpan satu baris keputusan berisi jenis
disposisi dan unit layanan tujuan. Tidak ada tempat tidur yang dipesan, tidak ada episode yang
dibuat, dan tidak ada apa pun yang diteruskan ke rawat inap.

**Catatan 2026-08-24.** Di tempat ini dulu tertulis alasan kedua: kunjungan IGD tidak akan
mendapat pelonggaran `RWI-RULE-026` karena bertipe `Emergency`. Alasan itu **sudah tidak
berlaku** sejak `RWI-DEC-070` memperluas pelonggaran ke `Emergency`. Aturan di bawah tetap
berlaku utuh, tetapi dasarnya sekarang adalah batas episode, kelas yang ditagihkan, unit
layanan, dan DPJP — lihat `RWI-DEC-071`.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Ketika disposisi `RANAP` dijalankan, kunjungan IGD **ditutup** dan kunjungan baru bertipe rawat inap **dibuat**. Kunjungan baru itulah jangkar episode rawat inap |
| 2 | Kedua kunjungan dihubungkan dengan penanda **satu rangkaian kedatangan yang sama**, sehingga riwayat pasien dapat dibaca utuh dari IGD sampai pulang. **Mekanismenya**, sejak `RWI-DEC-073`: kolom `OriginEncounterId` yang boleh kosong pada `TrxPatientEncounter`, diisi kunjungan rawat inap dengan Id kunjungan IGD. Kolom itu dimiliki `RegistrationManagement` dan **dikerjakan modul IGD** lewat `IGD-DEC-075`; Rawat Inap hanya membacanya |
| 3 | Kunjungan rawat inap mewarisi pasien dan penjamin dari kunjungan IGD. Unit layanan, kelas pasien, dan DPJP diisi sesuai keputusan admisi rawat inap, bukan diwarisi dari IGD |
| 4 | Penutupan kunjungan IGD dan pembuatan kunjungan rawat inap adalah **satu tindakan utuh**: berhasil dua-duanya, atau tidak ada yang berubah sama sekali |
| 5 | Bila serah terima gagal di tengah jalan, kunjungan IGD tetap terbuka dan pasien tetap tercatat di IGD. Tidak boleh ada keadaan pasien "tidak ada di mana-mana" |
| 6 | Catatan klinis yang sudah ditulis selama di IGD **tetap menempel pada kunjungan IGD**. Catatan itu tidak dipindahkan, tidak disalin, dan tidak diubah |
| 7 | Penanda `ClosesEmergencyVisit` pada master jenis disposisi menjadi penentu perilaku ini, dan mulai benar-benar dijalankan. Untuk jenis `RANAP` nilainya tetap `true` seperti yang sudah diisi seeder |
| 8 | Waktu pasien tiba di bangsal adalah **event `Tiba` pada catatan kepergian IGD**, bukan waktu yang ditetapkan Rawat Inap. Penempatan tempat tidur untuk pasien asal IGD **ditolak** selama event itu belum tercatat, dan `InpBedPlacement.StartDateTime` diisi dari waktu itu. Jalur datang langsung dan poliklinik tidak berubah. Lihat `RWI-DEC-072` |

**Contoh konkret.** Ny. Sari datang ke IGD pada 21 September 2026 pukul 20:10 dengan sesak napas.

| Waktu | Yang terjadi | Kunjungan IGD | Kunjungan rawat inap | Episode |
| --- | --- | --- | --- | --- |
| 20:10 | Ny. Sari mendaftar di IGD | Dibuat, tipe `Emergency` | Belum ada | Belum ada |
| 20:25 – 22:00 | Triase, pemeriksaan, dan tindakan IGD | Terbuka, catatan klinis IGD masuk ke sini | Belum ada | Belum ada |
| 22:15 | Dokter IGD memutuskan rawat inap ke bangsal Melati | Masih terbuka | Belum ada | Belum ada |
| 22:40 | Ny. Sari diantar ke bangsal Melati. Perawat penerima mencatat event `Tiba` pada catatan kepergian IGD | Masih terbuka | Belum ada | Belum ada |
| 22:45 | Petugas admisi menempatkan Ny. Sari di bed Melati 2A. `StartDateTime` diisi **22:40**, waktu tiba, bukan 22:45 | **Ditutup**, dihubungkan lewat `OriginEncounterId` | **Dibuat**, tipe `Inpatient`, unit Melati, kelas 2, DPJP dr. Andi | Dibuat dan berstatus `Admitted` |
| Hari ke-2 | Dokter menulis catatan perkembangan | Tidak berubah | Catatan menempel di sini | Berjalan |

Yang perlu diperhatikan pada contoh itu: catatan IGD pukul 20:25 sampai 22:00 tetap berada pada
kunjungan IGD dan tidak ikut pindah. Ketika perawat bangsal membuka riwayat Ny. Sari, penanda
rangkaian pada aturan nomor 2 yang membuat kedua kunjungan terbaca sebagai satu kedatangan.

**Contoh jalur tidak normal.** Seandainya pada pukul 22:45 tempat tidur Melati 2A ternyata sudah
diambil pasien lain, penempatan ditolak sesuai `RWI-RULE-015`. Karena aturan nomor 4 dan 5,
kunjungan IGD **tidak jadi ditutup** dan kunjungan rawat inap **tidak jadi dibuat**. Ny. Sari
tetap tercatat sebagai pasien IGD, dan petugas admisi memilih tempat tidur lain lalu mengulang.

**Contoh jalur tidak normal kedua.** Seandainya petugas admisi mencoba menempatkan Ny. Sari
sebelum perawat penerima mencatat event `Tiba`, penempatan **ditolak** sesuai aturan nomor 8.
Ny. Sari tetap pasien IGD, tempat tidurnya tetap terpesan, dan tidak ada penempatan bermula
pada waktu yang dikarang sistem.

**Akibat pada keputusan lama.** Baris "kunjungan IGD dipakai apa adanya" pada `RWI-RULE-005` dan
`RWI-DEC-011` ditandai `superseded`. Kalimat pokoknya — satu episode menempel pada tepat satu
kunjungan — tetap berlaku.

**Yang belum boleh dianggap selesai.** Aturan nomor 1, 2, 4, 7, dan 8 menuntut perubahan atau
pembacaan pada modul `EmergencyInstallationManagement` yang bukan milik Rawat Inap dan berstatus
`ACTIVE`. Sejak `RWI-DEC-069` pemiliknya bernama: **Rizki Gunawan**. Persetujuan
pemiliknya dicatat sebagai `RWI-OQ-034` dan **memblokir implementasi**, walaupun tidak memblokir
desain.

### `RWI-RULE-030` — Catatan DPJP episode dan cara menegakkannya

**Tujuan.** Sistem harus selalu bisa menjawab: siapa DPJP pasien ini **pada tanggal tertentu**,
bukan hanya siapa DPJP-nya sekarang. Dari jawaban itulah kewenangan perpindahan ditegakkan dan
resume pulang diisi.

**Keadaan yang menjadi masalah.** DPJP hari ini hanya berupa satu kolom pada kunjungan, tanpa
riwayat. Mesin hak akses juga hanya mengenal peran terhadap endpoint, tidak mengenal hubungan
seorang dokter dengan satu pasien tertentu.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Setiap episode rawat inap punya **catatan DPJP** tersendiri berisi riwayat penugasan, bukan satu kolom yang ditimpa |
| 2 | Satu baris catatan memuat: dokter siapa, berlaku sejak kapan, berakhir kapan, siapa yang menugaskan atau mengalihkan, dan alasannya |
| 3 | Pada satu waktu hanya boleh ada **tepat satu** DPJP aktif untuk satu episode. Tidak boleh nol, tidak boleh dua |
| 4 | Permintaan perpindahan pasien oleh dokter hanya diterima bila dokter itu adalah DPJP yang **sedang aktif** pada saat permintaan diajukan. Dokter lain ditolak, dan tidak ada kolom keterangan yang dapat dipakai melewatinya |
| 5 | Pengalihan DPJP adalah tindakan tersendiri yang wajib menyimpan alasan. Baris lama diberi waktu berakhir, baris baru dibuat. Baris lama **tidak dihapus dan tidak ditimpa** |
| 6 | Penjaga pada aturan nomor 4 ditulis di dalam service Rawat Inap, bukan di mesin hak akses. Setiap endpoint yang menuntut kewenangan DPJP wajib memanggilnya |
| 7 | Resume pulang dan perhitungan visite membaca catatan ini untuk menentukan DPJP yang berlaku pada tanggal yang dimaksud |

**Contoh konkret.** Tn. Budi dirawat 21 sampai 25 September 2026.

| Waktu | Tindakan | Isi catatan DPJP |
| --- | --- | --- |
| 21 Sept 10:40 | Admisi diaktifkan, DPJP dr. Andi | Baris 1: dr. Andi, sejak 21 Sept 10:40, belum berakhir |
| 22 Sept 08:15 | dr. Andi meminta pasien pindah ke Anggrek 1A | Diterima, karena dr. Andi DPJP aktif |
| 22 Sept 14:00 | dr. Rina, dokter jaga, mencoba memindahkan Tn. Budi | **Ditolak.** dr. Rina bukan DPJP episode ini |
| 23 Sept 07:00 | dr. Andi cuti, tanggung jawab dialihkan ke dr. Rina, alasan "DPJP cuti 23–25 Sept" | Baris 1 diberi waktu berakhir 23 Sept 07:00. Baris 2: dr. Rina, sejak 23 Sept 07:00 |
| 23 Sept 09:30 | dr. Rina meminta pasien pindah | Diterima, karena dr. Rina sudah menjadi DPJP aktif |
| 25 Sept | Resume pulang dibuat | Menyebut dr. Andi untuk 21–23 Sept dan dr. Rina untuk 23–25 Sept, bukan hanya yang terakhir |

**Kenapa riwayat, bukan satu kolom.** Tanpa riwayat, pada 25 September sistem hanya tahu DPJP-nya
dr. Rina. Sistem tidak lagi dapat membuktikan bahwa perpindahan 22 September memang diminta
dokter yang saat itu berwenang. Padahal justru itu yang diaudit.

**Risiko yang harus diterima secara sadar.** Karena penjaga ditulis di dalam service dan bukan di
mesin hak akses, penjaga itu hanya bekerja bila benar-benar dipanggil. Endpoint baru yang lupa
memanggilnya akan lolos tanpa peringatan apa pun. Risiko ini dicatat sebagai `RWI-RISK-004`, dan
penurunannya bergantung pada test yang saat ini belum ada.

### `RWI-RULE-031` — Riwayat perubahan status episode

**Tujuan.** Setiap perubahan status episode rawat inap dapat ditelusuri urut: dari status apa ke
status apa, oleh siapa, kapan, dan atas dasar apa. Riwayat ini bukan sekadar untuk audit, tetapi
menjadi sumber angka bagi empat aturan yang sudah dikunci.

**Keadaan yang menjadi masalah.** Kolom jejak pada setiap tabel hanya menyimpan perubahan
terakhir, sedangkan catatan aktivitas keluarannya berupa berkas log yang tidak dapat ditampilkan
di layar dan tidak dapat disaring per episode.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Rawat Inap punya **tabel riwayat status** sendiri, meniru bentuk `TrxWorkflowStatusHistory` milik modul Workflow, tetapi tidak menumpang pada tabel itu |
| 2 | Satu baris memuat: episode mana, dari status apa, ke status apa, jenis tindakan, siapa pelakunya, kapan, alasan, dan nomor urut |
| 3 | Baris riwayat ditulis **dalam transaksi yang sama** dengan perubahan statusnya. Tidak boleh ada status yang berubah tanpa baris riwayat |
| 4 | Seluruh perubahan status wajib lewat **satu pintu** di dalam service Rawat Inap. Tidak boleh ada endpoint yang menyetel status langsung ke tabel episode |
| 5 | Baris riwayat **tidak boleh diubah dan tidak boleh dihapus**. Koreksi dilakukan dengan menambah baris baru, bukan menimpa baris lama |
| 6 | Perubahan yang dihitung saat pembacaan — pemesanan bed yang gugur pada `RWI-RULE-002` dan episode `Draft` yang batal sendiri pada `RWI-RULE-022` — ditulis sebagai baris riwayat bertanda **dilakukan sistem**, bukan dilakukan orang |
| 7 | Tabel ini menjadi sumber data untuk laporan penutupan tanpa kelayakan keuangan (`RWI-RULE-009`), tiga daftar pantau (`RWI-RULE-023`), pembukaan kembali episode (`RWI-RULE-020`), dan pembuktian belum adanya catatan klinis saat pembatalan (`RWI-RULE-004`) |

**Contoh konkret.** Episode Tn. Budi, 21 sampai 25 September 2026.

| Urut | Waktu | Dari | Ke | Pelaku | Alasan |
| ---: | --- | --- | --- | --- | --- |
| 1 | 21 Sept 09:15 | — | `Draft` | Sdri. Wati, petugas admisi | Admisi dimulai |
| 2 | 21 Sept 10:40 | `Draft` | `Admitted` | Sdri. Wati, petugas admisi | Pasien menempati bed Melati 3B |
| 3 | 25 Sept 09:00 | `Admitted` | `DischargePending` | dr. Andi, DPJP | Pasien diizinkan pulang |
| 4 | 25 Sept 13:40 | `DischargePending` | `Closed` | Sdri. Wati, petugas admisi | Seluruh syarat penutupan terpenuhi |

Contoh baris yang ditulis sistem, bukan orang: pemesanan bed atas nama Ny. Sari dibuat pukul
09:15 dan tidak diselesaikan. Pada pembacaan pukul 11:16, sistem menulis satu baris riwayat
bertanda dilakukan sistem, dari `Reserved` ke `Available`, dengan alasan "pemesanan lewat batas 2
jam". Tidak ada nama orang pada baris itu.

**Kenapa aturan nomor 4 penting.** Tanpa satu pintu, endpoint baru yang menyetel status langsung
akan lolos tanpa meninggalkan baris riwayat, dan cacat itu baru ketahuan saat diaudit. Ini pola
yang sama dengan yang ditemukan pada kunjungan hari ini: `PATCH /patient-encounters/{id}/status`
menimpa status tanpa aturan perpindahan dan tanpa riwayat.

**Yang belum diputuskan.** Berapa lama baris riwayat disimpan sebelum boleh diarsipkan. Ini
keputusan hukum dan audit yang menurut `RWI-DEC-006` berada di luar wewenang pemegang sementara.
Dicatat sebagai `RWI-OQ-035`.

### `RWI-RULE-032` — Resume pulang sebagai catatan resmi episode

**Tujuan.** Setiap episode rawat inap yang ditutup meninggalkan satu ringkasan perawatan yang
terbaca sistem, bukan sekadar berkas yang diunggah.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Satu episode rawat inap punya **tepat satu** resume pulang. Resume ini milik Rawat Inap, bukan menumpang tabel surat keterangan milik modul Klinis |
| 2 | Isi resume: diagnosis utama, diagnosis sekunder, tindakan selama dirawat, obat pulang, instruksi kontrol, cara pulang, tanggal masuk, tanggal pulang, dan DPJP yang merawat beserta periodenya |
| 3 | DPJP beserta periodenya diisi otomatis dari catatan DPJP pada `RWI-RULE-030`, bukan diketik ulang |
| 4 | Resume ditandatangani DPJP yang aktif saat pasien pulang. Resume yang belum ditandatangani **menahan penutupan** episode sesuai `RWI-RULE-010` |
| 5 | Isi resume menyesuaikan cara pulang pada `RWI-RULE-011`. Untuk pasien meninggal, kolom instruksi kontrol dan obat pulang tidak berlaku dan diganti waktu serta sebab kematian. Untuk pasien dirujuk, tujuan rujukan wajib diisi |
| 6 | Surat keterangan untuk pasien, kantor, atau penjamin **tetap** dibuat lewat `TrxMedicalCertificate` seperti sekarang. Surat itu boleh menyalin isi resume, tetapi bukan resume itu sendiri |
| 7 | Setelah episode ditutup, resume tidak dapat diubah. Koreksi hanya lewat pembukaan kembali episode sesuai `RWI-RULE-020` |

**Contoh konkret.** Tn. Budi, dirawat 21 sampai 25 September 2026, pulang atas izin DPJP.

| Bagian resume | Isi |
| --- | --- |
| Diagnosis utama | Demam tifoid |
| Diagnosis sekunder | Anemia ringan |
| Tindakan selama dirawat | Pemasangan infus, pemeriksaan darah lengkap 2 kali |
| Obat pulang | Sudah diserahkan Farmasi 25 Sept pukul 12:40 |
| Instruksi kontrol | Kontrol poliklinik penyakit dalam 2 Oktober 2026 |
| Cara pulang | Atas izin DPJP |
| DPJP yang merawat | dr. Andi, 21–23 Sept; dr. Rina, 23–25 Sept |

Contoh jalur tidak normal: seandainya Tn. Budi meninggal pada 24 September, baris obat pulang dan
instruksi kontrol tidak ditampilkan, dan sebagai gantinya wajib diisi waktu serta sebab kematian.
Bila keluarga kemudian meminta surat keterangan kematian untuk keperluan pemakaman, surat itu
dibuat terpisah lewat `TrxMedicalCertificate` bertipe `DeathCertificate`, menyalin isi resume.

**Catatan tata kelola.** Daftar isi resume pada aturan nomor 2 disusun dari praktik umum, bukan
dari persetujuan komite klinis. Isi minimal resume medis adalah keputusan klinis, sehingga tetap
berada di bawah gerbang pemilik klinis yang belum ditunjuk.

### `RWI-RULE-033` — Penugasan perawat penanggung jawab

**Tujuan.** Census dapat menjawab siapa perawat penanggung jawab seorang pasien, dan daftar pantau
kepatuhan pada `RWI-RULE-023` punya orang yang jelas untuk ditagih.

**Keadaan yang menjadi masalah.** Yang ada sekarang hanyalah penugasan perawat ke klaster nurse
station untuk memanggil antrean poliklinik, bukan penugasan perawat kepada seorang pasien.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Penugasan perawat dicatat **per episode**, dengan bentuk yang sama seperti catatan DPJP: perawat siapa, sejak kapan, sampai kapan, siapa yang menugaskan |
| 2 | Yang berwenang menugaskan dan mengganti adalah **kepala ruangan** |
| 3 | Pada satu waktu satu episode punya tepat satu perawat penanggung jawab. Penggantian menutup baris lama dan membuka baris baru; baris lama tidak ditimpa |
| 4 | Modul ini **tidak** mengelola jadwal jaga. Penugasan diisi manual, bukan diturunkan dari jadwal kerja milik modul HR |
| 5 | Episode boleh berjalan sementara tanpa perawat penanggung jawab, dan keadaan itu **tidak menahan** tindakan apa pun. Episode tanpa perawat muncul pada daftar pantau kepatuhan sebagai baris yang perlu ditindaklanjuti kepala ruangan |

**Contoh konkret.** Tn. Budi masuk bangsal Melati 21 September pukul 10:40. Kepala ruangan Ns. Dewi
menugaskan Ns. Sinta sebagai perawat penanggung jawab pukul 11:00. Pada 23 September Ns. Sinta
cuti, dan kepala ruangan menugaskan Ns. Rani. Riwayatnya terbaca dua baris: Ns. Sinta 21–23
September, Ns. Rani 23 September sampai episode ditutup. Antara pukul 10:40 dan 11:00 episode
sempat tanpa perawat penanggung jawab, dan selama 20 menit itu Tn. Budi muncul di daftar pantau
kepala ruangan tanpa satu pun tindakan yang tertahan.

**Kenapa bukan per giliran jaga.** Penugasan per giliran jaga menuntut sistem tahu jadwal jaga
setiap perawat, dan jadwal itu milik modul HR. Memasukkannya berarti menarik modul HR ke dalam
scope Rawat Inap, padahal `RWI-DEC-004` tidak memasukkannya.

### `RWI-RULE-034` — Pengaturan Rawat Inap yang dapat diubah admin

**Tujuan.** Seluruh angka yang boleh diubah admin berada di satu tempat, sehingga admin tidak
perlu mencari ke banyak layar dan pengembang tidak perlu menyebar nilai ke banyak tabel.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Rawat Inap punya **satu tabel pengaturan**, mengikuti pola `MstEmergencySetting` yang sudah dipakai IGD |
| 2 | Nilai yang disimpan di sana dan nilai bawaannya: batas pemesanan tempat tidur 2 jam (`RWI-RULE-002`), target pengkajian awal 24 jam dan verifikasi CPPT 24 jam (`RWI-RULE-021`), batas episode `Draft` telantar 1 hari (`RWI-RULE-022`), ambang penutupan tertunda 4 jam dan ambang kepatuhan 24 jam (`RWI-RULE-023`) |
| 3 | Perubahan nilai berlaku pada **pembacaan berikutnya**, tanpa perlu aplikasi dinyalakan ulang |
| 4 | Butir daftar periksa administrasi pada `RWI-RULE-018` **tidak** disimpan di sini, karena bentuknya daftar baris yang dapat ditambah dan dikurangi, bukan satu nilai. Butir itu tetap punya tabel master tersendiri |
| 5 | Setiap perubahan nilai pengaturan menyimpan siapa yang mengubah dan kapan |

**Contoh konkret.** Batas pemesanan tempat tidur semula 2 jam. Pada 1 Oktober 2026 pukul 14:00,
admin mengubahnya menjadi 3 jam. Pemesanan yang dibuat pukul 13:30 dan dibaca pukul 15:45 masih
terkunci, karena pada saat pembacaan batas yang berlaku sudah 3 jam. Tidak ada aplikasi yang
perlu dinyalakan ulang, dan perubahan itu tercatat atas nama admin yang melakukannya.

### `RWI-RULE-035` — Satu pasien satu episode rawat inap aktif

**Tujuan.** Mencegah satu pasien tercatat dirawat di dua tempat sekaligus, yang membuat census,
lama dirawat, dan tagihan kamar menjadi ganda.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Satu pasien **paling banyak punya satu** episode berstatus `Admitted` atau `DischargePending` pada satu waktu |
| 2 | Percobaan menempatkan pasien yang sudah punya episode aktif **ditolak**, bukan sekadar diberi peringatan |
| 3 | Episode berstatus `Draft` **tidak** ikut dihitung. Pasien boleh punya beberapa `Draft` sekaligus |
| 4 | Bila pasien sudah punya `Draft` lain, sistem menampilkan **peringatan** saat admisi baru dibuka, dan petugas boleh meneruskan atau membatalkan yang lama |
| 5 | Larangan ini berlaku per **pasien**, bukan per kunjungan. Bayi dan ibunya adalah dua pasien berbeda, sehingga keduanya boleh punya episode aktif bersamaan |

**Kenapa `Draft` tidak ikut dilarang.** Episode `Draft` sering lahir dari percobaan yang tidak
selesai — petugas membuka admisi lalu terganggu, atau penempatan ditolak karena tempat tidur
diambil pasien lain. Melarangnya akan membuat petugas terkunci oleh percobaannya sendiri. Karena
`RWI-RULE-022` sudah membatalkan `Draft` telantar setelah 1 hari, risikonya kecil.

**Contoh konkret.**

| Keadaan | Yang terjadi |
| --- | --- |
| Tn. Budi sedang dirawat di Melati 3B. Petugas mencoba menempatkannya lagi di Anggrek 1A | **Ditolak.** "Tn. Budi sudah dirawat pada episode `RI-2026-09-000123` di Melati 3B." Bila memang pindah kamar, pakai perpindahan, bukan admisi baru |
| Tn. Budi punya satu episode `Draft` dari percobaan kemarin. Petugas membuka admisi baru | **Diteruskan dengan peringatan.** "Pasien ini punya admisi lain yang sedang disiapkan sejak kemarin." Petugas boleh lanjut atau membatalkan yang lama |
| Ny. Sari dirawat, dan bayinya juga dirawat di boks kamar yang sama | **Diizinkan.** Keduanya pasien berbeda |
| Tn. Budi sudah `Closed` kemarin, lalu hari ini kembali dirawat | **Diizinkan.** Episode baru, sesuai `RWI-RULE-020` |

### `RWI-RULE-036` — Kepergian fisik pasien dan pelepasan tempat tidur

**Tujuan.** Tempat tidur kembali dapat dipakai pasien berikutnya sejak pasien benar-benar
meninggalkan ruangan, bukan menunggu urusan administrasi selesai.

**Keadaan yang menjadi masalah.** `RWI-RULE-003` melepas tempat tidur pada saat episode ditutup.
Padahal `RWI-RULE-023` sendiri mengakui penutupan bisa tertunda berjam-jam — ambang daftar
pantaunya 4 jam. Selama jeda itu tempat tidur terbaca terisi padahal pasien sudah pulang. Di
rumah sakit yang penuh, itu berarti kamar terlihat penuh padahal ada yang kosong.

**Aturan yang dikunci.**

| No | Aturan |
| ---: | --- |
| 1 | Ada tindakan tersendiri bernama **"catat pasien sudah meninggalkan ruangan"**, terpisah dari penutupan episode |
| 2 | Tindakan itu hanya boleh saat episode berstatus `DischargePending`, dan wajib menyimpan waktu kepergian beserta pencatatnya |
| 3 | Yang berwenang mencatat: petugas admisi, perawat pelaksana, kepala ruangan, dan supervisor |
| 4 | Tindakan itu **melepas tempat tidur seketika**. Tempat tidur kembali `Available` dan boleh dipesan pasien berikutnya |
| 5 | Episode **tetap** `DischargePending` dan tetap wajib ditutup. Kepergian fisik bukan penutupan |
| 6 | Setelah kepergian dicatat, episode `DischargePending` boleh **tanpa** penempatan aktif. Ini melonggarkan `INV-INP-01`, dan hanya untuk keadaan ini |
| 7 | Penutupan episode tetap dapat dilakukan tanpa mencatat kepergian lebih dulu. Bila itu terjadi, tempat tidur dilepas saat penutupan seperti semula |
| 8 | Pasien yang sudah dicatat pergi **tidak** dapat dipindahkan lagi, dan tidak muncul lagi pada census |

**Contoh konkret.** Tn. Budi, episode `RI-2026-09-000123`, bed `BD-RSMMC-00105`.

| Waktu | Yang terjadi | Status episode | Bed | Census |
| --- | --- | --- | --- | --- |
| 25 Sept 09:20 | dr. Andi menyatakan boleh pulang | `DischargePending` | `Occupied` | Muncul |
| 25 Sept 10:15 | Keluarga menjemput, Tn. Budi meninggalkan kamar. Perawat mencatat kepergian | `DischargePending` | **`Available`** | **Tidak muncul** |
| 25 Sept 10:40 | Ny. Sari dipesankan `BD-RSMMC-00105` | — | `Reserved` | — |
| 25 Sept 13:10 | Urusan administrasi Tn. Budi selesai, episode ditutup | `Closed` | Tidak berubah | — |

Tanpa aturan ini, `BD-RSMMC-00105` baru bisa dipakai pukul 13:10. Dengan aturan ini, pukul 10:40 —
selisih dua setengah jam pada satu tempat tidur saja.

**Jalur tidak normal.** Bila ternyata Tn. Budi belum jadi pulang setelah kepergian terlanjur
dicatat, tidak ada pembatalan. Yang dilakukan: episode ditutup, lalu pasien menjalani admisi baru
dengan episode baru. Alasannya sama dengan `RWI-RULE-020` — pasien yang benar-benar kembali
dirawat selalu mendapat episode baru.

**Yang tidak dilakukan aturan ini.** Tempat tidur dikembalikan ke `Available`, **bukan** ke
`Cleaning`. Modul ini tidak mengelola pekerjaan pembersihan kamar; bila rumah sakit ingin menahan
tempat tidur untuk dibersihkan lebih dulu, admin menyetelnya `Cleaning` lewat menu master data
seperti biasa.

### `RWI-RULE-037` — Pasien meninggal dan pasien kabur — **BELUM FINAL**

Dasar keputusan: `RWI-DEC-059`. Menjawab `RWI-OQ-039` dan melengkapi `RWI-RULE-011`.

> **Keputusan ini tidak dapat naik ke `approved`.** Isinya menyangkut rekam medis, pelaporan wajib,
> dan dokumen hukum — area klinis yang dikecualikan `RWI-DEC-006`. Yang tertulis di bawah adalah
> **usulan** yang wajib ditinjau pemilik klinis sebelum modul dipakai melayani pasien sungguhan.
> Karena belum `approved`, tidak ada acceptance criteria yang ditulis untuk aturan ini, dan
> keduanya **tetap di luar MVP**.

**Usulan untuk pasien meninggal.**

| Butir | Usulan |
| --- | --- |
| Siapa mencatat | Petugas admisi, atas dasar pernyataan dokter |
| Resume pulang | **Tetap wajib.** Instruksi kontrol dan obat pulang tidak berlaku, digantikan waktu dan sebab kematian |
| Tempat tidur | Dilepas saat jenazah dipindahkan dari ruangan, memakai mekanisme `RWI-RULE-036` |
| Dokumen tambahan | Surat keterangan kematian dibuat terpisah lewat modul Klinis, bukan oleh modul ini |
| Pelaporan | Belum ditentukan; menunggu pemilik klinis |

**Usulan untuk pasien kabur.**

| Butir | Usulan |
| --- | --- |
| Siapa mencatat | Kepala ruangan atau supervisor |
| Resume pulang | **Tetap wajib**, berisi kondisi terakhir yang diketahui |
| Alasan dan waktu | Wajib diisi: kapan kaburnya diketahui dan bagaimana dipastikan |
| Tempat tidur | Dilepas saat kaburnya dipastikan, memakai mekanisme `RWI-RULE-036` |
| Bila pasien kembali | Episode baru, bukan melanjutkan yang lama. Sesuai `RWI-RULE-020` |

**Nomor yang dicadangkan.** Nilai `4` dan `5` pada daftar cara pulang dicadangkan untuk kedua
kasus ini dan **belum dipakai**, supaya penambahan kelak tidak mengubah angka yang sudah tersimpan.

---

## Frontend Decision Authority

Baris di sini adalah keputusan yang **sengaja didelegasikan** kepada pelaksana. Agent tidak
menetapkan menu, route, tab, modal, warna, maupun tata letak berdasarkan seleranya sendiri.

| Decision ID | Area | Owner | Status | Batas yang diizinkan | Dasar |
|---|---|---|---|---|---|
| `RWI-FE-001` | Kata yang dipakai untuk menamai angka hari rawat pada census | Pelaksana frontend | `DEV_DISCRETION` | Wajib menyebut dengan jelas bahwa angka itu hitungan hari rawat, bukan lama waktu sebenarnya. Bentuk kalimat, singkatan, penempatan, dan gaya tampilan bebas | `RWI-RULE-019` |
| `RWI-FE-002` | Bentuk tampilan empat daftar pantau | Pelaksana frontend | `DEV_DISCRETION` | Boleh satu halaman gabungan atau empat halaman terpisah. Urutan kolom, cara menandai keterlambatan, dan penempatan menu bebas. Yang wajib: lama keterlambatan terbaca bila konsep keterlambatan berlaku, dan daftar tidak boleh menghalangi tindakan apa pun | `RWI-RULE-023`, `RWI-AC-138` |
| `RWI-FE-003` | Nama dan label kesembilan langkah alur admisi | Pelaksana frontend | `DEV_DISCRETION` | Urutan dan isi langkah tetap mengikat sesuai `03-frontend-architecture.md` bagian 3A.2 dan 3A.3; kata yang tampil bebas | `RWI-DEC-075` |
| `RWI-FE-004` | Bentuk penanda langkah, misalnya garis, angka, atau tab | Pelaksana frontend | `DEV_DISCRETION` | Langkah yang sedang berjalan dan yang sudah lewat wajib terbeda; memuat ulang halaman wajib memulihkan langkah dari URL | `RWI-DEC-075`, `RWI-DEC-076` |
| `RWI-FE-005` | Tata letak Beranda Rawat Inap | Pelaksana frontend | `DEV_DISCRETION` | Ketiga isi wajib `FE-INP-19` harus tercapai dan dapat diklik; seluruh layar tingkat dua tetap tunduk pada `IA-INP-01` | `RWI-DEC-078` |

---

## Decision Log

| Decision ID | Type | Keputusan atau pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RWI-DEC-001` | Fact | Scope dikunci lebih dulu tanpa capability map. **Sudah ditutup pada 2026-08-21:** audit `/qv-trace` selesai dan risiko duplikasi sudah diperiksa. Hasilnya, tidak ada satu pun rencana tabel baru yang menduplikasi tabel existing; pasien, dokter, kunjungan, penjamin, tindakan, resep, kamar, tempat tidur, kelas pasien, dan persetujuan semuanya dipakai ulang | Agent | `closed` | — | `01-existing-capability-map.md` revision `1.0`, bagian 14.4, backend SHA `5afb54b`, frontend SHA `dec4fdeff` |
| `RWI-DEC-002` | Fact | Prefix entity operasional modul ini adalah `Inp`, bukan `Inpatient` | Backend governance | `draft` | — | `RWI-FACT-003` |
| `RWI-DEC-003` | Fact | Status registry `PLANNED` berarti belum ada izin implementasi, migration, atau database | Backend governance | `draft` | — | `RWI-FACT-002` |
| `RWI-DEC-004` | Decision | Batas scope MVP dikunci pada 18 kemampuan MUST milik PRD. Daftar Di dalam scope dan Di luar scope disetujui apa adanya | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — dinaikkan menyusul `RWI-DEC-006` | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-005` | Decision | Sebelas lubang cakupan diselesaikan sebagai aturan di dalam kemampuan yang sudah masuk scope, bukan sebagai item MUST baru. Jumlah MUST tetap 18 | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — dinaikkan menyusul `RWI-DEC-006` | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-006` | Decision | Pemilik suite skill Quilvian ditetapkan sebagai Product/Domain Owner sementara. Keputusan produk dan alur kerja boleh naik ke `approved`. Keputusan klinis dan keputusan keamanan/privasi tetap ditandai terbuka dan menjadi syarat sebelum produksi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — **nama orang belum diisi** | Wawancara pertanyaan 2, 2026-08-20 |
| `RWI-DEC-007` | Decision | Tempat tidur memakai status `Reserved` sebelum `Occupied`. Pemesanan gugur sendiri setelah lewat batas waktu, dan kedaluwarsa dihitung saat data dibaca sehingga tidak memerlukan program penjadwal | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 3; menutup OQ-RI-002 dan `RWI-CON-002` |
| `RWI-DEC-008` | Decision | Pemesanan tempat tidur berlaku 2 jam sejak dibuat. Satu angka yang sama berlaku untuk semua unit dan semua asal pemesanan, dan angka itu disimpan sebagai parameter yang boleh diubah admin tanpa mengubah program | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 4; menutup OQ-RI-003; dirinci pada `RWI-RULE-002` |
| `RWI-DEC-009` | Decision | Model status episode adalah `Draft` → `Admitted` → `DischargePending` → `Closed`, ditambah `Cancelled`. `InCare` dibuang karena tidak punya definisi maupun pemicu di PRD, dan informasinya sudah tersimpan pada catatan pengkajian serta catatan visite. Status episode tidak dipakai sebagai syarat sebelum dokumentasi klinis boleh ditulis | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 5; menutup `RWI-CON-001`; dasar `RWI-FACT-005`; dirinci pada `RWI-RULE-003` |
| `RWI-DEC-010` | Decision | Pembatalan boleh oleh petugas admisi selagi episode `Draft`, dan oleh supervisor atau kepala ruangan selagi `Admitted` **selama episode belum punya satu pun catatan klinis**. Setelah ada catatan klinis, pembatalan tertutup. Alasan wajib diisi, dan pelepasan tempat tidur menjadi satu kesatuan dengan pembatalan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 6; menutup `RWI-OQ-022` dan `RWI-GAP-010`; dirinci pada `RWI-RULE-004` |
| `RWI-DEC-011` | Decision | Setiap episode rawat inap selalu menempel pada tepat satu kunjungan. ~~Kunjungan IGD atau poliklinik yang sudah ada dipakai apa adanya;~~ untuk pasien yang datang langsung, sistem membuat kunjungan bertipe rawat inap secara otomatis di dalam proses admisi. **Sebagian `superseded` oleh `RWI-DEC-041` pada 2026-08-21:** bagian yang dicoret dicabut untuk jalur IGD, karena kunjungan IGD kini ditutup dan diganti kunjungan rawat inap baru. Kalimat pokok "satu episode menempel pada tepat satu kunjungan" tetap berlaku utuh | Product/domain owner sementara | `approved` untuk kalimat pokok; `superseded` sebagian untuk jalur IGD | Pemegang sementara, 2026-08-20; direvisi 2026-08-21 | Wawancara pertanyaan 7; menutup `OQ-RI-001`; dirinci pada `RWI-RULE-005`; direvisi pada Closure Pass pertanyaan 4, lihat `RWI-DEC-041` dan `RWI-RULE-029` |
| `RWI-DEC-012` | Decision | Kewenangan transfer mengikuti tabel PRD bagian 14 apa adanya: Kepala Perawat, Perawat pelaksana, dan Supervisor boleh memindahkan pasien. Perpindahan berjalan satu langkah tanpa penerimaan unit tujuan. Risiko pindah kelas tanpa persetujuan diterima secara sadar, tercatat sebagai `RWI-RISK-001` | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 8; menutup `RWI-CON-003`, `OQ-RI-004`, dan `OQ-RI-005`; dirinci pada `RWI-RULE-006` |
| `RWI-DEC-013` | Decision | Pindah kelas tidak dikecualikan. Kewenangannya sama dengan pindah tempat tidur biasa, dan kelas yang ditagihkan selalu mengikuti kamar yang ditempati. Perubahannya disimpan sebagai riwayat. `RWI-RISK-001` diterima secara sadar, dan `RWI-GAP-004` pasien titipan dinyatakan belum terjawab | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 9; menutup `RWI-OQ-026` dan `RWI-OQ-015`; dirinci pada `RWI-RULE-007` |
| `RWI-DEC-014` | Decision | Perpindahan adalah satu tindakan utuh: berhasil seluruhnya atau tidak ada yang berubah sama sekali. Pasien tidak pernah tercatat tanpa tempat tidur, sehingga INV-02 berlaku setiap saat. Urutan EPIC RI-09 dinyatakan tidak berlaku | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 10; menutup `RWI-CON-007`; dirinci pada `RWI-RULE-008` |
| `RWI-DEC-015` | Decision | Kelayakan keuangan **memblokir** penutupan episode. Hanya `Cleared` yang membuka penutupan; `Pending`, `Blocked`, dan status yang belum ada sama-sama menahan. Supervisor boleh menutup dengan alasan wajib, dan episode itu ditandai serta masuk laporan tersendiri. Rumusan "tersedia" pada EPIC RI-10 dinyatakan tidak berlaku | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 11; menutup `RWI-CON-005` dan `OQ-RI-008`; dasar `RWI-FACT-007` dan `RWI-FACT-008`; dirinci pada `RWI-RULE-009` |
| `RWI-DEC-016` | Decision | Keputusan pulang tetap milik DPJP sendiri. Penutupan episode dikerjakan petugas admisi atau Supervisor, dan hanya bisa berjalan bila kelima syarat penutupan terpenuhi. Frasa "sesuai SOP" pada baris `Close episode` diganti aturan tegas ini | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 12; menutup `RWI-CON-004` dan `OQ-RI-006`; dirinci pada `RWI-RULE-010` |
| `RWI-DEC-018` | Decision | ~~Pemisahan jenis kelamin dan isolasi tetap berupa penyaring pencarian, bukan aturan yang menolak penempatan.~~ **`superseded` oleh `RWI-DEC-064` pada 2026-08-21.** Keduanya kini menjadi aturan keras yang menolak penempatan | Product/domain owner sementara | `superseded` | Digantikan `RWI-DEC-064` | Wawancara pertanyaan 14; digantikan setelah pemilik berwenang ditunjuk lewat `RWI-DEC-061` |
| `RWI-DEC-019` | Decision | Pasien titipan **tidak masuk MVP**. Tidak ada kelas hak terpisah dan tidak ada penanda titipan; kelas tagihan tetap mengikuti kamar. Keringanan biaya diurus petugas billing di luar modul ini | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 15; menutup `RWI-OQ-016` dan `RWI-GAP-004`; dirinci pada `RWI-RULE-013` |
| `RWI-DEC-020` | Decision | Bayi baru lahir masuk MVP dengan episode dan kunjungan sendiri, dan boks bayi didaftarkan sebagai tempat tidur tersendiri di kamar ibu. ICU tidak diberi aturan khusus; ICU diperlakukan sebagai unit layanan biasa | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 16; menutup `OQ-RI-010` dan `RWI-GAP-011`; dirinci pada `RWI-RULE-014` |
| `RWI-DEC-021` | Decision | Keadaan tempat tidur diperiksa ulang saat admisi diaktifkan. Bila masih kosong, penempatan diteruskan tanpa peringatan walaupun pemesanan sudah gugur. Bila sudah diambil pasien lain, penempatan ditolak sementara episode tetap `Draft` dan seluruh isian admisi tetap utuh | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 17; menutup `RWI-OQ-024`; dirinci pada `RWI-RULE-015` |
| `RWI-DEC-022` | Decision | DPJP dapat menginisiasi dan/atau menyetujui transfer pasien yang berada dalam tanggung jawab klinisnya, berdasarkan indikasi medis dan kesiapan unit tujuan. Untuk pasien di bawah DPJP lain, transfer harus melalui koordinasi dengan DPJP terkait atau pengalihan tanggung jawab DPJP yang terdokumentasi | Product/domain owner sementara | `approved` untuk pembagian kewenangan; **dua istilah masih terbuka** | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 18; menutup `RWI-OQ-025`; memunculkan `RWI-CON-008` dan `RWI-OQ-029`; dirinci pada `RWI-RULE-016` |
| `RWI-DEC-023` | Decision | "Kesiapan unit tujuan" adalah pertimbangan profesional DPJP, **bukan** pemeriksaan sistem. Alur perpindahan tetap satu langkah untuk semua peran sesuai `RWI-DEC-012`, yang karena itu tetap berlaku utuh. Yang wajib dijaga sistem hanya dua: dokter yang meminta harus DPJP episode tersebut, dan alasan medis wajib diisi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 19; menutup `RWI-CON-008`; ditulis pada `RWI-RULE-016` |
| `RWI-DEC-024` | Decision | Koordinasi antar DPJP **tidak direkam** sistem. Sistem hanya mengenal dua jalan sah: DPJP episode itu sendiri yang memindahkan, atau tanggung jawab DPJP dialihkan lebih dulu secara tercatat. Dokter lain selalu ditolak, tanpa kolom keterangan yang bisa dipakai melewatinya | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 20; menutup `RWI-OQ-029`; ditulis pada `RWI-RULE-016` |
| `RWI-DEC-025` | Decision | Visite tercatat dari catatan perkembangan yang ditulis dokter pada hari itu, bukan dari formulir tersendiri. Dokter sendiri yang mencatat, dan kunjungan tanpa catatan tidak terhitung sebagai visite | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 21; menutup `RWI-OQ-019` dan `RWI-GAP-007`; dasar `RWI-FACT-010`; dirinci pada `RWI-RULE-017` |
| `RWI-DEC-026` | Decision | Clearance administrasi berbentuk daftar periksa yang butirnya diatur admin lewat master data. Sifatnya menahan: penutupan ditolak selama ada butir wajib yang belum ditandai petugas admisi. MVP dikirim dengan tiga butir bawaan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 22; menutup `OQ-RI-007`; dirinci pada `RWI-RULE-018` |
| `RWI-DEC-027` | Decision | Lama dirawat dihitung dari selisih tanggal, bukan selisih jam, dengan hasil paling sedikit 1 hari. Untuk pasien yang masih dirawat, angkanya naik setiap pergantian tanggal | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 23; menutup `RWI-OQ-020` dan `RWI-GAP-008`; dirinci pada `RWI-RULE-019` |
| `RWI-DEC-028` | Decision | Hanya Supervisor yang boleh membuka kembali episode `Closed`, dan semata untuk membetulkan catatan. Reopen tidak mengembalikan tempat tidur, tidak memunculkan pasien di census, dan tidak menambah lama dirawat. Pasien yang benar-benar kembali dirawat selalu mendapat episode baru | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 24; menutup `OQ-RI-012`; dirinci pada `RWI-RULE-020` |
| `RWI-DEC-029` | Decision | Pengkajian awal keperawatan ditargetkan 24 jam sejak pasien menempati tempat tidur, dan verifikasi CPPT oleh DPJP ditargetkan 24 jam sejak catatan ditulis. Keduanya **tidak menahan apa pun**; keterlambatan hanya muncul pada daftar pantau kepatuhan. Kedua angka dapat diubah admin | Product/domain owner sementara | `draft` — **tidak dapat naik ke `approved`** | Belum di-approve. Aturan klinis dan akreditasi yang dikecualikan `RWI-DEC-006` | Wawancara pertanyaan 25; menjawab `RWI-OQ-018` dan `RWI-GAP-006`; dirinci pada `RWI-RULE-021` |
| `RWI-DEC-030` | Decision | Episode `Draft` yang tidak disentuh selama 1 hari terbaca `Cancelled` dengan alasan sistem, dihitung saat data dibaca tanpa program penjadwal. Kunjungan yang terlanjur dibuat ikut ditandai batal. Batas 1 hari dapat diubah admin | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 26; menutup `RWI-OQ-028`; dirinci pada `RWI-RULE-022` |
| `RWI-DEC-031` | Decision | Visite dihitung satu per dokter per tanggal. Dua catatan dari dokter yang sama pada tanggal yang sama tetap satu visite dengan waktu catatan pertama; dua dokter berbeda menghasilkan dua visite | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 27; menutup `RWI-OQ-030`; ditulis pada `RWI-RULE-017` |
| `RWI-DEC-032` | Decision | Tiap daftar pantau punya satu peran penanggung jawab: penutupan tertunda ke petugas admisi dengan ambang 4 jam, kepatuhan pengkajian dan CPPT ke kepala ruangan dengan ambang 24 jam, dan penutupan tanpa kelayakan keuangan ke supervisor tanpa ambang per baris. Ketiga ambang diatur admin, dan ketiga daftar tidak menahan tindakan apa pun | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 28; menutup `RWI-OQ-027` dan `RWI-OQ-031`; dirinci pada `RWI-RULE-023` |
| `RWI-DEC-033` | Decision | Obat pulang adalah jenis resep pada CAP-023, dikirim ke Farmasi dengan konteks encounter yang sama dan status penyerahannya dibaca balik. Penyerahannya menjadi satu butir pada daftar periksa administrasi `RWI-RULE-018`, bukan gerbang tersendiri, dan butir itu dapat dinonaktifkan admin | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 29; menutup `RWI-OQ-021` dan `RWI-GAP-009`; dirinci pada `RWI-RULE-024` |
| `RWI-DEC-034` | Fact | `OQ-RI-011` tentang rencana asuhan keperawatan SDKI sudah terjawab sejak `RWI-DEC-004`. CAP-013 berada pada daftar Di luar scope dengan keterangan ditunda setelah MVP, sehingga tidak perlu ditanyakan ulang | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Turunan `RWI-DEC-004`; dicatat pada 2026-08-20 |
| `RWI-DEC-035` | Decision | Wajib ada satu persetujuan umum rawat inap berisi persetujuan tindakan kedokteran umum, persetujuan pemberian informasi kepada penjamin, dan penunjukan penerima informasi. Tidak menahan admisi, tetapi menahan penutupan lewat butir daftar periksa `RWI-RULE-018` | Product/domain owner sementara | `draft` — **tidak dapat naik ke `approved`** | Belum di-approve. Keputusan privasi dan hukum yang dikecualikan `RWI-DEC-006` | Wawancara pertanyaan 30; **diputuskan lewat delegasi**, lihat `RWI-DEC-036`; menjawab `OQ-RI-009`; dirinci pada `RWI-RULE-025` |
| `RWI-DEC-036` | Fact | Pada 2026-08-20 pemilik kebutuhan menyatakan: "jawab semua pertanyaan dengan rekomendasi anda berikan, tidak perlu bertanya kepada saya lagi". Sejak titik itu, opsi yang ditandai **(Direkomendasikan)** diambil sebagai pilihan pemilik kebutuhan. Keputusan yang lahir dari delegasi ini ditandai pada kolom Evidence-nya, agar pembaca berikutnya tahu butir itu tidak ditimbang satu per satu oleh pemilik kebutuhan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Pernyataan pemilik kebutuhan pada wawancara pertanyaan 30 |
| `RWI-DEC-037` | Open Question | Siapa nama orang atau komite yang berwenang menyetujui modul ini, menggantikan pemegang sementara | **Tidak dapat diselesaikan lewat wawancara** | `draft` | — | `RWI-OQ-023`. Delegasi `RWI-DEC-036` tidak berlaku untuk butir ini karena jawabannya adalah nama orang yang sungguh ada, dan agent tidak boleh mengarangnya |
| `RWI-DEC-038` | Decision | Dokumentasi klinis rawat inap memakai mesin klinis yang sudah ada, bukan entity tandingan dan bukan antrean semu. Keharusan antrean dan konsultasi dilonggarkan khusus untuk kunjungan bertipe rawat inap, begitu pula batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi. ~~Perilaku rawat jalan, IGD, dan medical check-up tidak boleh berubah~~ — **sebagian `superseded` oleh `RWI-DEC-070` pada 2026-08-24:** IGD dikeluarkan dari daftar itu; rawat jalan dan medical check-up tetap tidak boleh berubah | Product/domain owner sementara | `approved` untuk arah desain; **implementasi terblokir** sampai pemilik `ClinicalManagement` dan `PharmacyManagement` menyetujui | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 1; dasar `RWI-FACT-011` dan `RWI-FACT-012`; menutup `RWI-TRQ-001`, `RWI-TRQ-002`, `RWI-TRQ-003`; memunculkan `RWI-OQ-032` dan `RWI-RISK-002`; dirinci pada `RWI-RULE-026` |
| `RWI-DEC-039` | Decision | Catatan penempatan milik Rawat Inap menjadi satu-satunya sumber kebenaran penghunian tempat tidur. Kolom `MstBed.BedStatus` turun kedudukan menjadi salinan yang ditulis dalam transaksi yang sama. Nilai `Reserved` dan `Occupied` tidak boleh lagi disetel manusia lewat menu master data; admin hanya berwenang atas `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive`. Disediakan laporan selisih | Product/domain owner sementara | `approved` untuk arah desain; **implementasi terblokir** sampai pemilik `MasterData` menyetujui pembatasan endpoint `/availability` | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 2; dasar `01-existing-capability-map.md` bagian 3.2 hambatan kedua dan `RWI-TF-003`; menutup `RWI-TRQ-004` dan `RWI-TRQ-005`; memunculkan `RWI-OQ-033`; dirinci pada `RWI-RULE-027` |
| `RWI-DEC-040` | Decision | Selama `BillingManagement` belum punya kemampuan transaksi, status kelayakan keuangan disimpan pada episode rawat inap dan ditandai manual oleh petugas kasir atau billing, dengan pelaku, waktu, dan catatan wajib. Penandaan ditampilkan sebagai penandaan sementara. `RWI-DEC-015` dan `RWI-RULE-009` tetap berlaku utuh; yang berpindah kelak hanya sumber datanya, bukan aturan penutupannya | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 3; dasar `RWI-TRC-005` pada capability map dan `RWI-TF-012`; menutup `RWI-TRQ-006`; memunculkan `RWI-RISK-003`; dirinci pada `RWI-RULE-028` |
| `RWI-DEC-041` | Decision | Saat disposisi `RANAP` dijalankan, kunjungan IGD ditutup dan kunjungan baru bertipe rawat inap dibuat sebagai jangkar episode. Keduanya dihubungkan sebagai satu rangkaian kedatangan. Serah terima bersifat utuh: berhasil dua-duanya atau tidak ada yang berubah. Catatan klinis IGD tetap menempel pada kunjungan IGD | Product/domain owner sementara | `approved` untuk arah desain; **implementasi terblokir** sampai pemilik `EmergencyInstallationManagement` menyetujui | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 4; dasar `RWI-TRC-008` pada capability map, `RWI-TF-016`, dan `RWI-TF-017`; menutup `RWI-TRQ-007` dan `RWI-TRQ-008`; **men-`superseded` sebagian `RWI-DEC-011`**; memunculkan `RWI-OQ-034`; dirinci pada `RWI-RULE-029`. **Justifikasinya ditulis ulang 2026-08-24 oleh `RWI-DEC-071`** — keputusannya tidak berubah, alasannya yang diperbarui |
| `RWI-DEC-042` | Decision | Episode rawat inap punya catatan DPJP tersendiri berisi riwayat penugasan: dokter, masa berlaku, pengalih, dan alasan. Pada satu waktu tepat satu DPJP aktif. Permintaan perpindahan oleh dokter hanya diterima bila ia DPJP aktif saat itu, dan penjaganya ditulis di dalam service Rawat Inap karena mesin hak akses tidak mengenal kewenangan per pasien | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 5; dasar `RWI-TRC-007` pada capability map dan `RWI-TF-014`; menegakkan `RWI-DEC-023` dan `RWI-DEC-024`; menutup `RWI-TRQ-009`; memunculkan `RWI-RISK-004`; dirinci pada `RWI-RULE-030` |
| `RWI-DEC-043` | Decision | Rawat Inap punya tabel riwayat perubahan status episode sendiri, meniru bentuk `TrxWorkflowStatusHistory` tanpa menumpang padanya. Riwayat ditulis dalam transaksi yang sama dengan perubahan status, lewat satu pintu di service, bersifat tidak dapat diubah, dan menjadi sumber data bagi laporan pengecualian, tiga daftar pantau, reopen, serta pembuktian pembatalan admisi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 | Closure Pass pertanyaan 6; dasar `RWI-TRC-009` pada capability map, `RWI-TF-019`, dan `RWI-TF-020`; menutup `RWI-TRQ-010`; memunculkan `RWI-OQ-035`; dirinci pada `RWI-RULE-031` |
| `RWI-DEC-044` | Fact | Pada 2026-08-21 pemilik kebutuhan menyatakan: "oke semua saya percayakan anda, jawab saja semua ya dengan rekomendasi yang anda berikan". Sejak titik itu, sisa pertanyaan Closure Pass diputuskan memakai opsi yang ditandai **(Direkomendasikan)**. Delegasi ini **tidak berlaku** untuk keputusan klinis, privasi, hukum, dan penunjukan nama orang, sesuai batas yang sudah ditetapkan `RWI-DEC-006` | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 | Pernyataan pemilik kebutuhan pada Closure Pass pertanyaan 7. Sejalan dengan `RWI-DEC-036` pada Scope Pass |
| `RWI-DEC-045` | Decision | Resume pulang adalah catatan resmi milik episode rawat inap, satu episode tepat satu resume, isinya terbaca sistem dan menyesuaikan cara pulang. Surat keterangan untuk pasien dan pihak ketiga tetap dibuat lewat `TrxMedicalCertificate`. Resume yang belum ditandatangani DPJP menahan penutupan | Product/domain owner sementara | `approved` untuk bentuk dan kepemilikan data; **daftar isi minimal resume tetap terbuka secara klinis** | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 7; dasar `RWI-CAP-025` pada capability map; menutup `RWI-TRQ-011`; dirinci pada `RWI-RULE-032` |
| `RWI-DEC-046` | Decision | Penanda obat pulang disimpan sebagai jenis resep pada tabel resep milik Farmasi, bukan sebagai daftar terpisah milik Rawat Inap, supaya petugas farmasi melihatnya di layar mereka sendiri | Product/domain owner sementara | `approved` untuk arah desain; **implementasi terblokir** bersama `RWI-OQ-032` | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 8; dasar `RWI-CAP-022` pada capability map; menutup `RWI-TRQ-012`; melengkapi `RWI-RULE-024` |
| `RWI-DEC-047` | Decision | Perawat penanggung jawab ditugaskan per episode dengan riwayat, ditugaskan kepala ruangan, satu perawat aktif pada satu waktu. Modul ini tidak mengelola jadwal jaga dan tidak menarik modul HR ke dalam scope. Episode tanpa perawat tidak menahan tindakan apa pun, hanya muncul pada daftar pantau | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 9; dasar `RWI-CAP-014` pada capability map; menutup `RWI-TRQ-013`; dirinci pada `RWI-RULE-033` |
| `RWI-DEC-048` | Decision | Data master tempat tidur, kamar, unit layanan, dan kelas pasien yang sungguhan diisi admin lewat layar master yang sudah ada, bukan lewat perintah database. Rawat Inap hanya menyediakan seeder data contoh untuk lingkungan pengembangan dan pengujian, dan seeder itu tidak boleh dijalankan di lingkungan produksi. Kesiapan data master menjadi satu baris gerbang sebelum modul dipakai | Product/domain owner sementara | `approved` untuk aturannya; **nama penanggung jawab pengisian masih terbuka** | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 10; dasar `RWI-TRC-006` pada capability map; menutup `RWI-TRQ-014` sebagian; memunculkan `RWI-OQ-036` |
| `RWI-DEC-049` | Decision | Cacat tombol aktif dan nonaktif tempat tidur diperbaiki **di sisi frontend**, dengan mengubah pemanggilan dari `/beds/{id}/activate` dan `/deactivate` menjadi `PATCH /beds/{id}/status` yang sudah ada dan sudah dipakai hampir semua master lain. Tidak ada perubahan backend, sehingga tidak menambah pemilik modul yang persetujuannya ditunggu. Perbaikan ini dikerjakan sebagai pekerjaan tersendiri milik pelaksana frontend dan menjadi prasyarat sebelum Rawat Inap dipakai | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 11; dasar `RWI-CON-TRC-001` pada capability map dan `RWI-TF-022`; menutup `RWI-TRQ-015` |
| `RWI-DEC-050` | Decision | Seluruh angka yang dapat diubah admin disatukan dalam satu tabel pengaturan Rawat Inap, mengikuti pola `MstEmergencySetting`. Butir daftar periksa administrasi tetap punya tabel master tersendiri karena bentuknya daftar baris, bukan satu nilai | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 12; dasar `RWI-CAP-034` pada capability map; menutup `RWI-TRQ-016`; dirinci pada `RWI-RULE-034` |
| `RWI-DEC-051` | Decision | Pembuatan test adalah bagian dari pekerjaan Rawat Inap, bukan pekerjaan terpisah yang ditunda. Setiap task yang menyentuh modul milik pihak lain wajib membawa test regresi untuk jalur lama yang disentuhnya, dan test itu menjadi syarat selesainya task. Alasannya: `RWI-RULE-026`, `RWI-RULE-027`, dan `RWI-RULE-029` menyentuh empat modul berstatus `ACTIVE`, sedangkan hari ini tidak ada satu pun test yang menjaga jalur poliklinik, IGD, maupun farmasi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — diputuskan lewat delegasi `RWI-DEC-044` | Closure Pass pertanyaan 13; dasar `RWI-CAP-044` pada capability map dan keterbatasan bagian 11.2 butir 5; menutup `RWI-TRQ-017`; menurunkan `RWI-RISK-002` dan `RWI-RISK-004` |
| `RWI-DEC-052` | Fact | Pada 2026-08-21 pemilik kebutuhan menyatakan "jawab semua dengan rekomendasi anda yang berikan" untuk Amendment Pass. Sejak titik itu, sisa pertanyaan pass ini diputuskan memakai opsi yang ditandai **(Direkomendasikan)**. Delegasi ini **tidak berlaku** untuk keputusan klinis, privasi, hukum, dan penunjukan nama orang, sesuai batas `RWI-DEC-006`. Delegasi ini juga **tidak** berlaku surut maupun ke pass berikutnya | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 | Amendment Pass pertanyaan 1. Sejalan dengan `RWI-DEC-036` dan `RWI-DEC-044` |
| `RWI-DEC-053` | Decision | Riwayat lokasi pasien rawat inap **tetap dimiliki modul Rawat Inap** pada catatan penempatan tempat tidur. Pengiriman SATUSEHAT kelak dibangun sebagai kemampuan tersendiri yang membaca dari sana, bukan dengan memindahkan penyimpanan ke kunjungan milik Registrasi. Bentuk yang diminta SATUSEHAT adalah bentuk kiriman, bukan bentuk penyimpanan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 1; menutup `RWI-OQ-037` dan bagian kepemilikan data pada `DEC-INP-005`; dasar baseline `ID-INP-INT-001` yang melarang memetakan Encounter satu lawan satu ke satu tabel setempat. **Tidak mengubah blueprint yang sudah ada** |
| `RWI-DEC-054` | Decision | Satu pasien paling banyak punya satu episode rawat inap aktif pada satu waktu. Yang dihitung hanya status `Admitted` dan `DischargePending`; episode `Draft` tidak dihitung dan hanya memunculkan peringatan. Larangan berlaku per pasien, sehingga bayi dan ibunya tetap boleh punya episode aktif bersamaan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 2; menutup `RWI-OQ-042` dan `ARCH-GAP-004`; dirinci pada `RWI-RULE-035`. **Mengubah blueprint:** menambah invariant `INV-INP-10` |
| `RWI-DEC-055` | Decision | Kepergian fisik pasien dicatat sebagai tindakan tersendiri yang melepas tempat tidur seketika, terpisah dari penutupan episode. Episode tetap `DischargePending` dan tetap wajib ditutup. Setelah kepergian dicatat, episode `DischargePending` boleh tanpa penempatan aktif | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 3; menutup `RWI-OQ-041` dan `ARCH-GAP-003`; dirinci pada `RWI-RULE-036`. **Mengubah blueprint:** melonggarkan `INV-INP-01`, menambah kolom waktu kepergian, satu perintah baru, satu nilai baru pada alasan berakhirnya penempatan, dan satu endpoint |
| `RWI-DEC-056` | Decision | Episode bayi menyimpan penanda hubungan ke episode ibunya, berupa satu rujukan opsional. Bukan tabel baru. Tujuannya kepastian identitas: sistem dapat menjawab bayi siapa yang berada di boks kamar mana | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 4; menutup `RWI-OQ-040` dan `ARCH-GAP-002`; dasar baseline pasal 13 tentang kepastian identitas pasien. **Mengubah blueprint:** menambah satu kolom opsional pada episode |
| `RWI-DEC-057` | Decision | Resume pulang menyimpan riwayat versi, tetapi **hanya versi yang sudah ditandatangani**. Penyuntingan sebelum tanda tangan menimpa biasa. Setiap perubahan setelah tanda tangan — yang hanya mungkin lewat sesi koreksi — menyimpan salinan versi sebelumnya | Product/domain owner sementara | `approved` untuk bentuknya; **isi minimal resume tetap terbuka secara klinis** | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 5; menutup `RWI-OQ-043` dan `ARCH-GAP-005`; dasar baseline `ID-INP-CAP-019`. **Mengubah blueprint:** menambah satu tabel salinan versi |
| `RWI-DEC-058` | Decision | Serah terima klinis antar shift keperawatan **tidak masuk MVP**. Alasannya bersebab: isi serah terima adalah keputusan klinis yang pemiliknya belum ditunjuk, dan slice dokumentasi klinis juga masih tertahan `DEC-INP-001` — serah terima tanpa dokumentasi klinis hanya akan setengah jalan. Pengganti selama MVP: riwayat perawat penanggung jawab sudah menjawab siapa yang bertanggung jawab pada waktu tertentu | Product/domain owner sementara | `approved` sebagai keputusan scope; **isi serah terima tetap terbuka secara klinis** | Pemegang sementara, 2026-08-21 — lewat delegasi `RWI-DEC-052` | Amendment Pass pertanyaan 6; menjawab bagian scope `RWI-OQ-038` dan `DEC-INP-006`; bagian isinya tetap terbuka. **Tidak mengubah blueprint** |
| `RWI-DEC-059` | Decision | Usulan aturan pasien meninggal dan pasien kabur ditulis: pencatat, kewajiban resume, pelepasan tempat tidur lewat mekanisme kepergian fisik, dan dokumen tambahan. Kedua cara pulang **tetap di luar MVP** | Product/domain owner sementara | `draft` — **tidak dapat naik ke `approved`** | Belum di-approve. Area klinis, rekam medis, dan hukum yang dikecualikan `RWI-DEC-006` | Amendment Pass pertanyaan 7; menjawab `RWI-OQ-039` dan `DEC-INP-007`; dirinci pada `RWI-RULE-037`. **Tidak mengubah blueprint**, karena tetap di luar MVP |
| `RWI-DEC-060` | Decision | Sampai pemilik hukum menetapkan angkanya, **tidak ada pengarsipan maupun penghapusan otomatis** atas riwayat status, riwayat penempatan, riwayat penanggung jawab, dan resume pulang. Seluruhnya disimpan apa adanya. Ini pilihan yang menahan diri, bukan penetapan masa simpan | Product/domain owner sementara | `draft` — **tidak dapat naik ke `approved`** | Belum di-approve. Keputusan hukum dan audit yang dikecualikan `RWI-DEC-006` | Amendment Pass pertanyaan 8; menjawab `RWI-OQ-035`. **Tidak mengubah blueprint** |
| `RWI-DEC-061` | Decision | Pemilik kebutuhan yang menjalankan sesi ini menyatakan dirinya sebagai **pemilik yang berwenang menyetujui modul Rawat Inap**, menggantikan kedudukan "pemegang sementara". Sejak titik ini, kata "sementara" pada `RWI-DEC-006` tidak berlaku lagi | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-21. Jabatan formal belum diisi | Menutup `RWI-OQ-023` dan `RWI-DEC-037`. Men-`superseded` sebagian `RWI-DEC-006` pada bagian "sementara" |
| `RWI-DEC-062` | Decision | Modul `ClinicalManagement`, `PharmacyManagement`, `MasterData` HealthServices, ~~dan `EmergencyInstallationManagement`~~ berada di bawah kepemilikan yang sama dengan pemilik pada `RWI-DEC-061`. Persetujuan atas seluruh perubahan lintas modul yang dituntut blueprint ini **diberikan**. **Sebagian `superseded` oleh `RWI-DEC-069` pada 2026-08-24:** bagian `EmergencyInstallationManagement` dicabut, karena pemilik modul itu adalah Rizki Gunawan dan bukan pemilik pada `RWI-DEC-061`. Persetujuan atas tiga modul lainnya tetap berlaku utuh | Muhammad Hamzah | `approved` untuk tiga modul; `superseded` sebagian untuk `EmergencyInstallationManagement` | Muhammad Hamzah, 2026-08-21 | Menutup `RWI-OQ-032` dan `RWI-OQ-033`. Penutupan `RWI-OQ-034` **dicabut** oleh `RWI-DEC-069`; gerbang “Persetujuan pemilik modul tetangga” tidak lagi tercabut utuh |
| `RWI-DEC-063` | Decision | Pengisian dan validasi master kamar serta tempat tidur menjadi tanggung jawab **Admin Master Data / Tim Master Data**, dengan target selesai **22 Agustus 2026** | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-21 | Menutup `RWI-OQ-036`. Mencabut satu gerbang implementasi, dengan syarat target tanggalnya terpenuhi |
| `RWI-DEC-064` | Decision | Pemisahan jenis kelamin dan isolasi diubah menjadi **aturan keras yang menolak penempatan**, dijalankan di dalam pemeriksaan Kelayakan Penempatan. Bagian jenis kelamin berbasis penanda tempat tidur sudah dapat dijalankan; bagian isolasi dan pencampuran sekamar belum, karena datanya tidak ada di sistem | Muhammad Hamzah | `approved` untuk arahnya; **sebagian belum dapat dijalankan** | Muhammad Hamzah, 2026-08-21 | Menutup `RWI-OQ-017`, `RWI-GAP-005`, dan `DEC-INP-004`. **Men-`superseded` `RWI-DEC-018`**. Memunculkan `RWI-OQ-044`. Dirinci pada `RWI-RULE-012` |
| `RWI-DEC-065` | Decision | Kebutuhan isolasi menjadi **atribut episode rawat inap** dan dipakai di dalam Kelayakan Penempatan. Keputusan klinisnya milik DPJP dan dapat diperbarui selama perawatan. Pada admisi awal, petugas admisi boleh merekam nilainya berdasarkan keterangan dokter pengirim, ditandai sebagai catatan awal dan bukan keputusan klinis, supaya penempatan tidak menunggu pengkajian klinis yang slice-nya masih di luar MVP. Pasien yang butuh isolasi hanya boleh di tempat tidur isolasi, dan sebaliknya | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-21 | Menutup bagian isolasi pada `RWI-OQ-044` dan `DEC-INP-004A`; dirinci pada `RWI-RULE-012` bagian A. **Mengubah blueprint:** menambah kolom pada episode, satu enum, satu endpoint, dan satu daftar pantau |
| `RWI-DEC-066` | Decision | Seluruh kamar dianggap **tidak boleh ditempati campur** laki-laki dan perempuan. Penempatan dan perpindahan ditolak bila kamar sudah punya penghuni berjenis kelamin berbeda. Penghuni boks bayi tidak dihitung, dan penempatan ke boks bayi dikecualikan. **Tidak** ditambahkan kolom boleh-campur pada `MstRoom` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-21 | Menutup bagian jenis kelamin pada `RWI-OQ-044` dan `DEC-INP-004B`; dirinci pada `RWI-RULE-012` bagian B. **Mengubah blueprint:** menambah aturan pada Kelayakan Penempatan, tanpa kolom baru pada modul lain |
| `RWI-DEC-017` | Decision | Diakui lima cara pulang: atas izin DPJP, atas permintaan sendiri, dirujuk, meninggal, dan kabur. Syarat penutupan menyesuaikan cara pulangnya, dan kelimanya sama-sama melepas tempat tidur. Baris meninggal dan kabur tetap **terbuka secara klinis** sesuai `RWI-DEC-006` | Product/domain owner sementara | `approved` untuk keputusan produk; **terbuka** untuk sisi klinis | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 13; menutup `RWI-OQ-013`, `RWI-OQ-014`, `RWI-GAP-001`, dan `RWI-GAP-002`; dirinci pada `RWI-RULE-011` |
| `RWI-DEC-067` | Decision | Blueprint modul Rawat Inap revision `3` **disetujui**. `blueprint-manifest.md` naik dari `draft` menjadi `approved`, dan kedua roadmap delivery naik dari `PROVISIONAL` menjadi `APPROVED`. Sejak titik ini penulisan source code dibuka, dikerjakan **satu task per pengerjaan** mengikuti urutan dependency pada roadmap | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Mencabut gerbang “Approval blueprint” yang sebelumnya menahan **seluruh** task `BE-RWI-001` s.d. `BE-RWI-033` dan `FE-RWI-001` s.d. `FE-RWI-019`. **Tidak mengubah isi desain** — tidak ada kontrak, ERD, aturan, atau acceptance criteria yang berubah; yang berubah hanya status persetujuannya. Gerbang sebelum produksi pada dokumen ini **tidak** ikut tercabut |
| `RWI-DEC-068` | Decision | Lifecycle modul `InPatientManagement` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dinaikkan dari `PLANNED` menjadi `ACTIVE`. Prefix `Inp` yang sudah terdaftar tidak berubah. Sejak titik ini `QBE-MOD-002` tidak lagi menahan pembuatan entity operasional `Inp*` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Mencabut gerbang “Registry lifecycle” yang menahan seluruh task. Men-`superseded` bagian `RWI-FACT-002` yang menyatakan modul hanya berhak atas penamaan. **Wewenang eksekusi database di luar lokal dan deployment tetap terpisah** dan tidak diberikan oleh keputusan ini |
| `RWI-DEC-069` | Decision | Pemilik modul `EmergencyInstallationManagement` adalah **Rizki Gunawan**, bukan pemilik pada `RWI-DEC-061`. Karena itu persetujuan lintas modul yang diberikan `RWI-DEC-062` **tidak mencakup IGD**. `RWI-OQ-034` dan `DEC-INP-002` terbuka kembali, kini dengan pemilik yang bernama. Jawabannya sudah tersedia pada `IGD-DEC-067` — IGD menyatakan mengikuti `RWI-DEC-041` dan `RWI-RULE-029`, termasuk menjalankan penanda `ClosesEmergencyVisit` — tetapi keputusan itu masih `draft` sampai Rizki mencatatkannya atas namanya. Kepemilikan `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` HealthServices **tidak berubah** | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Koreksi fakta kepemilikan, **bukan** perubahan desain: tidak ada kontrak, ERD, aturan, maupun acceptance criteria yang berubah. Men-`superseded` sebagian `RWI-DEC-062`; membuka kembali `RWI-OQ-034` dan `DEC-INP-002`; membuat gerbang “Persetujuan pemilik modul tetangga” menjadi terbuka sebagian. Sumber: blueprint IGD `docs/module-blueprints/igd/00-interview-decisions.md` pass 2026-08-24, `IGD-DEC-067` |
| `RWI-DEC-070` | Decision | Pelonggaran `RWI-RULE-026` diperluas ke kunjungan bertipe IGD (`Emergency`), mencakup **ketiga** pelonggaran — aturan 3, 4, dan 5 — bukan hanya aturan 3. Aturan 6 direvisi: yang tidak boleh berubah sedikit pun tinggal rawat jalan dan medical check-up. Rawat Inap tetap tidak membuat tabel klinis tandingan dan tidak membuat antrean semu. Perluasan ini **melampaui** bunyi `IGD-DEC-068` yang hanya menyebut aturan 3, dan dikirim balik ke pemilik IGD sebagai koreksi, bukan diberlakukan diam-diam | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Amendment Pass pertanyaan 4. Alasannya: pasien IGD ditangani berulang kali dalam satu kunjungan, berganti dokter jaga, dan dikaji ulang berkala sesuai `IGD-DEC-083`; melonggarkan aturan 3 saja hanya memindahkan cacatnya ke catatan dokter kedua dan resep kedua. Wewenang ada pada pemilik `ClinicalManagement` dan `PharmacyManagement`, yaitu pemilik yang sama lewat `RWI-DEC-062`. **Prasyarat:** `IGD-DEC-074` harus berlaku lebih dulu. Men-`superseded` sebagian `RWI-DEC-038`; menjawab `IGD-DEC-068` |
| `RWI-DEC-071` | Decision | `RWI-DEC-041` **tetap berlaku**: disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap baru sebagai jangkar episode. Justifikasinya ditulis ulang. Dasarnya bukan lagi “pelonggaran `RWI-RULE-026` tidak menyentuh tipe `Emergency`” — alasan itu gugur oleh `RWI-DEC-070` — melainkan batas episode, kelas pasien yang ditagihkan, unit layanan, dan DPJP, yang menurut `RWI-RULE-029` aturan 3 wajib berasal dari keputusan admisi dan bukan warisan IGD | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Amendment Pass pertanyaan 1. Ditemukan bahwa changelog 2026-08-21 mencatat satu-satunya alasan `RWI-DEC-041` adalah tabrakan aturan 6, sehingga `RWI-DEC-070` meruntuhkannya. Kaki kedua sudah tertulis pada `RWI-RULE-029` aturan 3 dan diperkuat `IGD-DEC-076` tentang kelas kunjungan IGD. Tidak ada desain yang berubah; `RWI-DEC-011` tetap `superseded` sebagian; `IGD-DEC-067` milik Rizki tetap sah |
| `RWI-DEC-072` | Decision | Untuk pasien asal IGD, `InpBedPlacement.StartDateTime` diisi dari **event `Tiba` pada catatan kepergian IGD**, dan penempatan **ditolak** selama event itu belum tercatat. Waktu tiba tidak pernah ditetapkan Rawat Inap sendiri dan tidak pernah dikoreksi setelah tersimpan. Untuk jalur datang langsung dan poliklinik tidak ada yang berubah: `StartDateTime` tetap waktu penempatan dibuat | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Amendment Pass pertanyaan 2; menerima `IGD-DEC-071`. Urutan kerjanya menjadi: pasien tiba → perawat penerima mencatat `Tiba` → penempatan dibuat → kunjungan IGD ditutup, sejalan dengan `RWI-RULE-029` aturan 5. Opsi mengoreksi `StartDateTime` belakangan **ditolak** karena kolom itu dipakai menghitung lama rawat dan kelas tagihan. Menambah `RWI-RULE-029` aturan 8; menyentuh `INP-S01` dan `BE-RWI-011` |
| `RWI-DEC-073` | Decision | Rangkaian kedatangan diwujudkan sebagai kolom `OriginEncounterId` yang boleh kosong pada `TrxPatientEncounter`, sesuai `IGD-DEC-075`. Kolom itu dimiliki `RegistrationManagement` dan **pekerjaannya ada pada modul IGD**, bukan pada Rawat Inap, karena slice serah terima `INP-S09` memang berada di sisi sana. Rawat Inap hanya membacanya, sama seperti `EncounterId` hari ini. `compatibility_impact` pada manifest ditulis ulang jujur: nol perubahan kolom **oleh task Rawat Inap**, ditambah satu kolom pada `TrxPatientEncounter` yang dituntut `RWI-RULE-029` aturan 2 dan dikerjakan modul IGD | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Amendment Pass pertanyaan 3; menerima `IGD-DEC-075`. Acceptance criteria nomor 5 pada `BE-RWI-003` — “tidak ada kolom tabel modul lain yang berubah” — **tetap utuh dan tetap dapat diuji**. Persetujuan Registration API owner dan otorisasi migration pada basis data bersama menjadi urusan pemilik IGD. Melengkapi `RWI-RULE-029` aturan 2 yang selama ini kosong mekanismenya |
| `RWI-DEC-074` | Decision | Blueprint modul Rawat Inap revision `4` **disetujui**. `blueprint-manifest.md` naik dari `draft` menjadi `approved`, dan ketiga berkas roadmap disinkronkan ke masukan baru sebagai `roadmap_revision` `2`. Isi revision `4` adalah penyerapan empat keputusan Amendment Pass 2026-08-24: **nol tabel baru, nol kolom baru, nol endpoint baru, nol task bertambah** | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-24 | Menutup gerbang `BLUEPRINT_APPROVED` bagi sinkronisasi roadmap. Seluruh perubahan perilaku revision `4` hanya menyala pada `INP-S09` yang di luar MVP, sehingga tidak ada task berjalan yang perlu diulang dan `BE-RWI-011` **tidak** tertahan. Sejalan dengan `RWI-DEC-067` yang menyetujui revision `3` |
| `RWI-DEC-075` | Decision | Admisi rawat inap berbentuk **alur berlangkah dua jalur**, bukan satu formulir. Jalur pertama pendaftaran pasien baru dengan sembilan langkah: Tipe Pasien, Pendaftaran, Pembayaran, Dokter, Pilih Bed, Booking Bed, Konfirmasi, Cetak Persetujuan Pasien Ranap, Kartu Pasien. Jalur kedua pendaftaran pasien lama dengan delapan langkah: Pasien Lama, Informasi Pasien Lama, Tipe Pasien, lalu lima langkah yang sama sampai Cetak Persetujuan. Bentuknya mengikuti pola pendaftaran IGD yang sudah berjalan di `emergency-registration/` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-27 | Menutup cacat terbesar revision `0.3`: memilih penjamin (`RWI-CAP-002`, Wajib) dan memesan tempat tidur (`RWI-CAP-006`, Wajib) tidak punya layar sama sekali, sehingga `FLOW-RI-MVP-001` tidak dapat dijalankan. **Nol perubahan backend** — langkah Pembayaran memakai `POST /patient-encounters` milik Registrasi yang sudah membuat baris kunjungan beserta penjaminnya. Diserap ke `03-frontend-architecture.md` revision `0.4` bagian 3A |
| `RWI-DEC-076` | Decision | Tulisan ke server pada alur admisi terjadi **bertahap**, bukan ditahan sampai Konfirmasi: pasien pada langkah Pendaftaran, kunjungan dan episode `Draft` pada akhir langkah Dokter, pemesanan tempat tidur pada langkah Booking Bed, perubahan isian pada langkah Konfirmasi. Alur admisi **berhenti pada tempat tidur `Reserved`**; pasien menjadi `Admitted` hanya ketika kedatangannya dikonfirmasi dari papan tempat tidur | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-27 | Menahan semuanya sampai Konfirmasi meniadakan guna `Reserved`, padahal `RWI-CAP-006` ditandai wajib justru supaya dua petugas tidak merebut tempat tidur yang sama. Memisahkan penempatan mempertahankan pemeriksaan **ulang** Kelayakan Penempatan pada `FLOW-RI-MVP-001` langkah 6. **Akibat wajib:** alur yang ditinggal meninggalkan episode `Draft` di server, sehingga daftar kerja episode `FE-INP-16` menjadi wajib. Ditemukan bahwa kontrak hak akses `0.4.0` tidak memberi `InpatientBedOccupancy : Create` kepada perawat maupun kepala ruangan, sehingga konfirmasi masuk dijalankan petugas admisi dan supervisor; dicatat sebagai `RWI-OQ-045` |
| `RWI-DEC-077` | Decision | Persetujuan umum rawat inap **dicetak tanpa disimpan**. Sistem menyusun formulir berisi identitas pasien, penjamin, unit layanan, kelas, DPJP, nomor episode, dan ketiga isi minimal `RWI-DEC-035`; tanda tangan tetap di atas kertas dan disimpan manual. Layar tidak boleh menyatakan persetujuan sudah tersimpan atau tertanda tangan | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-27 | Memenuhi langkah "Cetak Persetujuan Pasien Ranap" pada `RWI-DEC-075` **tanpa** menyentuh `DEC-INP-003` yang pemiliknya belum ditunjuk. `RWI-CAP-031` **tetap** berada pada daftar kemampuan ditunda `04-prd-to-mvp.md` bagian 8, dan gerbang produksi yang menyangkut persetujuan tetap terbuka. Nol tabel baru, nol endpoint baru |
| `RWI-DEC-078` | Decision | Keterjangkauan menjadi **wewenang blueprint**, sejajar dengan keamanan, privasi, dan invariant — bukan lagi `DEV_DISCRETION`. Lima aturan ditulis sebagai `IA-INP-01` s.d. `IA-INP-05`, termasuk `IA-INP-04`: layar yang tidak terjangkau dari mana pun dihitung **belum selesai** walaupun kodenya ada dan test-nya lulus. Nama route, nama menu, dan urutan menu **tetap** `DEV_DISCRETION` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-27 | Lahir dari temuan bahwa layar sesi koreksi `FE-RWI-018` sudah jadi dan lulus test, tetapi tidak dapat dicapai siapa pun karena satu-satunya jalannya lewat census yang tidak memuat episode tertutup. Menyerahkan navigasi penuh ke pelaksana membuat cacat semacam ini tidak terdeteksi sampai modul dipakai. Menambah dua layar baru `FE-INP-16` daftar kerja episode dan `FE-INP-19` beranda; menghidupkan sembilan endpoint yang selama ini menganggur |
| `RWI-DEC-079` | Decision | Layar admisi formulir tunggal hasil `FE-RWI-006` **diganti total** oleh alur berlangkah, bukan dipertahankan sebagai jalur cepat. Delapan belas task frontend revision `0.3` yang sudah selesai **tetap dihitung selesai**; roadmap frontend naik ke revision `3` dengan menambahkan enam belas task di atasnya, bukan menyusun ulang dari nol | Muhammad Hamzah | `approved` | Muhammad Hamzah, 2026-08-27 | Mempertahankan formulir lama akan melahirkan dua jalur menuju admisi yang sama, dan salah satunya pasti lupa diperbarui — pola cacat yang sama dengan koreksi resume yang sengaja hanya ditempatkan pada satu layar. Tidak ada hasil kerja yang dibuang: kemampuan yang hilang memang tidak pernah dispesifikasikan, bukan dikerjakan salah. `FE-RWI-019` dibuka ulang karena cakupannya disusun ketika layarnya masih lima belas |
| `RWI-OQ-045` | Open question | Apakah `InpatientBedOccupancy : Create` perlu diberikan kepada kepala ruangan supaya konfirmasi kedatangan pasien dapat dilakukan dari ruangan, bukan dari meja admisi | Product/Domain bersama Backend/API | `open` | — | Muncul dari `RWI-DEC-076`. Kontrak hak akses `0.4.0` hanya memberi `Create` kepada petugas admisi dan supervisor. Tidak menahan pekerjaan: `FE-RWI-030` berjalan dengan peran yang kontraknya izinkan. Bila dibuka, perubahannya ada pada `contracts/permission-audit-matrix.md` dan seeder hak akses backend |
| `RWI-OQ-046` | Open question | Apakah jalur `POST /episodes` **tanpa** `EncounterId` perlu ditutup di backend | Backend/API bersama Product/Domain | `open` | — | Ditemukan 2026-08-27: `InpEpisodeService.BuildInpatientEncounter` membuat kunjungan sendiri dengan `PaymentType = EncounterPaymentType.Cash` yang ditanam di kode dan **tanpa** baris `TrxPatientEncounterGuarantor`, sehingga setiap admisi lewat jalur itu tercatat tunai termasuk untuk pasien berpenjamin. Setelah `RWI-DEC-075` tidak ada layar yang menempuhnya, tetapi jalurnya tetap terbuka bagi pemanggil lain. Tidak menahan pekerjaan frontend |

---

## Gate Sebelum Produksi

Persetujuan pemegang sementara **tidak** menutup gerbang berikut. Seluruhnya harus terpenuhi
sebelum modul ini boleh dipakai melayani pasien sungguhan. Tiga baris pertama dan terakhir
adalah gerbang tata kelola; empat baris `RWI-RULE-*` adalah aturan yang isinya sudah dipilih
pemegang sementara tetapi berada di area klinis atau privasi yang dikecualikan `RWI-DEC-006`.
Empat baris bertanda **Gerbang implementasi** ditambahkan pada Closure Pass 2026-08-21: baris itu
tidak menghalangi penyusunan desain, tetapi menghalangi penulisan source code.

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk. Semua aturan klinis pada dokumen ini memakai praktik umum dan regulasi sebagai dasar, bukan persetujuan komite klinis. Termasuk batas waktu pengkajian awal, verifikasi CPPT oleh DPJP, aturan pasien meninggal, dan syarat pasien boleh pulang |
| `RWI-RULE-012` — isolasi dan jenis kelamin | **Sebagian dicabut 2026-08-21.** `RWI-DEC-064` mengubah keduanya menjadi aturan keras yang menolak penempatan, diputuskan pemilik berwenang. Bagian jenis kelamin berbasis penanda tempat tidur sudah dapat dijalankan. Yang **masih menjadi gerbang**: aturan isolasi dan pencampuran sekamar belum dapat dijalankan karena datanya tidak ada, lihat `RWI-OQ-044` |
| `RWI-RULE-025` — persetujuan umum | **Gerbang keras.** `RWI-DEC-035` mewajibkan satu persetujuan umum tetapi tidak menahan admisi, sehingga ada jeda ketika pasien dirawat tanpa persetujuan tertulis. Ini keputusan privasi dan hukum, wajib ditinjau pemilik keamanan/privasi sebelum modul dipakai melayani pasien sungguhan |
| `RWI-RULE-021` — batas waktu klinis | **Gerbang keras.** `RWI-DEC-029` menetapkan target 24 jam untuk pengkajian awal dan verifikasi CPPT, dan angka itu diambil dari praktik akreditasi yang lazim, bukan dari persetujuan komite klinis. Wajib ditinjau pemilik klinis sebelum modul dipakai melayani pasien sungguhan |
| `RWI-RULE-037` — meninggal dan kabur | **Gerbang keras.** `RWI-DEC-059` menuliskan usulan aturan pasien meninggal dan pasien kabur, tetapi keduanya menyangkut rekam medis, pelaporan wajib, dan dokumen hukum. Wajib ditinjau pemilik klinis. Kedua cara pulang tetap di luar MVP sampai peninjauan itu selesai |
| Security/privacy owner | Belum ditunjuk. Hak akses ke rekam medis rawat inap, penelusuran audit, dan aturan koreksi data harus disetujui pemiliknya |
| Persetujuan pemilik modul tetangga | **TERBUKA SEBAGIAN.** Dicabut 2026-08-21 oleh `RWI-DEC-062` untuk `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` HealthServices. Bagian `EmergencyInstallationManagement` **terbuka kembali** 2026-08-24 lewat `RWI-DEC-069`: pemiliknya Rizki Gunawan dan persetujuan formalnya belum tercatat. Menahan `INP-S09` saja; slice lain tidak tertahan |
| Kesiapan data master | **Penanggung jawab sudah ditetapkan** `RWI-DEC-063`: Admin Master Data / Tim Master Data, target 22 Agustus 2026. Gerbang ini **tertutup begitu datanya benar-benar terisi**, bukan begitu penanggung jawabnya ditunjuk |
| Perbaikan tombol tempat tidur | **Gerbang implementasi.** Tombol aktifkan dan nonaktifkan pada halaman detail tempat tidur hari ini memanggil endpoint yang tidak ada dan selalu gagal. Harus diperbaiki lebih dulu sesuai `RWI-DEC-049`, karena tanpa itu admin tidak dapat menutup tempat tidur yang sedang rusak |
| Test regresi modul tetangga | **Gerbang implementasi.** Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi hari ini, padahal keempat modul itu akan disentuh. Sesuai `RWI-DEC-051`, test regresi menjadi syarat selesainya setiap task yang menyentuh modul tetangga |
| ~~Registry lifecycle~~ | **DICABUT 2026-08-24** oleh `RWI-DEC-068`. Modul `InPatientManagement` naik `PLANNED` → `ACTIVE`. Wewenang eksekusi database di luar lokal dan deployment tetap terpisah |

---

## Acceptance Criteria yang Sudah Dapat Diuji

Daftar ini disusun **hanya** dari aturan yang sudah `approved`. Setiap baris ditulis agar bisa
dicoba langsung oleh penguji tanpa perlu menafsirkan lagi. Butir yang aturannya masih terbuka
sengaja tidak dimasukkan.

| ID | Yang harus terjadi | Dasar |
|---|---|---|
| `RWI-AC-001` | Tempat tidur yang sedang berstatus `Reserved` tidak muncul pada hasil pencarian tempat tidur kosong | `RWI-RULE-001` |
| `RWI-AC-002` | Pemesanan yang dibuat pukul 09:15 masih mengunci tempat tidur pada pembacaan pukul 11:14, dan sudah terbaca `Available` pada pembacaan pukul 11:16, tanpa ada proses latar belakang yang dijalankan | `RWI-RULE-002` |
| `RWI-AC-003` | Batas 2 jam dapat diubah admin lewat pengaturan tanpa mengubah program, dan nilai barunya langsung dipakai pada pembacaan berikutnya | `RWI-RULE-002` |
| `RWI-AC-004` | Status episode yang tersedia hanya `Draft`, `Admitted`, `DischargePending`, `Closed`, dan `Cancelled`. Nilai `InCare` ditolak | `RWI-RULE-003` |
| `RWI-AC-005` | Dokter dapat menulis instruksi dan resep pada episode berstatus `Admitted` walaupun pengkajian awal keperawatan belum diisi sama sekali | `RWI-RULE-003` |
| `RWI-AC-006` | Membatalkan episode `Draft` mengembalikan tempat tidurnya ke `Available` pada tindakan yang sama, bukan pada langkah terpisah | `RWI-RULE-004` |
| `RWI-AC-007` | Membatalkan episode `Admitted` ditolak bila sudah ada satu saja dari enam jenis catatan klinis pada episode itu | `RWI-RULE-004` |
| `RWI-AC-008` | Pembatalan tanpa alasan ditolak, dan alasan yang hanya berisi tanda baca juga ditolak | `RWI-RULE-004` |
| `RWI-AC-009` | Admisi untuk pasien yang datang langsung menghasilkan satu kunjungan bertipe rawat inap, dan petugas tidak diminta mengisi form kedua | `RWI-RULE-005` |
| `RWI-AC-010` | Tidak ada episode rawat inap yang bisa tersimpan tanpa kunjungan | `RWI-RULE-005` |
| `RWI-AC-011` | Perawat pelaksana dapat memindahkan pasien tanpa persetujuan siapa pun dan tanpa menunggu jawaban unit tujuan | `RWI-RULE-006` |
| `RWI-AC-012` | Petugas admisi tidak dapat memindahkan pasien | `RWI-RULE-006` |
| `RWI-AC-013` | Perpindahan ke kamar berkelas berbeda mengubah kelas yang ditagihkan sejak waktu perpindahan, dan riwayatnya menyimpan kelas lama, kelas baru, waktu, serta nama pelakunya | `RWI-RULE-007` |
| `RWI-AC-014` | Bila perpindahan gagal di tengah jalan, pembacaan berikutnya menunjukkan pasien masih di tempat tidur lama, tempat tidur tujuan masih kosong, dan tidak ada riwayat perpindahan yang tersimpan | `RWI-RULE-008` |
| `RWI-AC-015` | Tidak pernah ditemukan episode berjalan yang terbaca tanpa tempat tidur aktif, termasuk saat perpindahan sedang diproses | `RWI-RULE-008`, INV-02 |
| `RWI-AC-016` | Penutupan episode ditolak bila status kelayakan keuangan bernilai `Pending`, `Blocked`, atau belum ada, dan pesan penolakannya menyebut syarat mana yang belum terpenuhi | `RWI-RULE-009`, `RWI-RULE-010` |
| `RWI-AC-017` | Supervisor dapat menutup episode yang belum `Cleared` dengan alasan wajib, dan episode itu muncul pada laporan penutupan tanpa kelayakan keuangan beserta nama supervisor dan alasannya | `RWI-RULE-009` |
| `RWI-AC-018` | Petugas admisi dapat menutup episode yang kelima syaratnya sudah terpenuhi tanpa keterlibatan DPJP lagi | `RWI-RULE-010` |
| `RWI-AC-019` | Petugas admisi tidak dapat membuat keputusan pasien boleh pulang | `RWI-RULE-010` |
| `RWI-AC-020` | Cara pulang wajib dipilih dari lima nilai yang tersedia. Teks bebas dan nilai kosong ditolak | `RWI-RULE-011` |
| `RWI-AC-021` | Penutupan dengan cara pulang "kabur" berhasil tanpa resume pulang dan tanpa keputusan pulang DPJP, tetapi ditolak bila waktu terakhir pasien terlihat atau nama pelapor dikosongkan | `RWI-RULE-011` |
| `RWI-AC-022` | Kelima cara pulang sama-sama mengembalikan tempat tidur ke `Available` | `RWI-RULE-011` |
| `RWI-AC-023` | Pasien yang ditempatkan di kamar berkelas lebih tinggi karena kelasnya penuh ditagih sesuai kamar yang ditempati, dan tidak ada penanda titipan di mana pun | `RWI-RULE-013` |
| `RWI-AC-024` | Bayi baru lahir yang dirawat gabung punya episode dan kunjungan sendiri, dan menempati boks yang terdaftar sebagai tempat tidur tersendiri | `RWI-RULE-014` |
| `RWI-AC-025` | Memindahkan bayi ke NICU tidak mengubah apa pun pada episode ibunya | `RWI-RULE-014` |
| `RWI-AC-026` | Mengaktifkan admisi yang pemesanannya sudah lewat 2 jam tetap berhasil selama tempat tidurnya masih kosong, tanpa peringatan apa pun | `RWI-RULE-015` |
| `RWI-AC-027` | Mengaktifkan admisi yang tempat tidurnya sudah diambil pasien lain ditolak, dan setelah penolakan seluruh isian admisi masih tersimpan utuh | `RWI-RULE-015` |
| `RWI-AC-028` | DPJP dapat memindahkan pasien yang ia DPJP-i tanpa menunggu jawaban unit tujuan, dan perpindahannya ditolak bila alasan medis dikosongkan | `RWI-RULE-016` |
| `RWI-AC-029` | Dokter yang bukan DPJP episode tersebut ditolak ketika mencoba memindahkan pasien itu | `RWI-RULE-016` |
| `RWI-AC-030` | Tidak tersedia kolom keterangan apa pun yang memungkinkan dokter bukan DPJP melewati penolakan perpindahan | `RWI-RULE-016` |
| `RWI-AC-031` | Setelah tanggung jawab DPJP dialihkan secara tercatat, DPJP yang baru dapat memindahkan pasien itu, dan DPJP lama tidak lagi bisa | `RWI-RULE-016` |
| `RWI-AC-032` | Menulis satu catatan perkembangan dokter langsung menghasilkan satu visite tercatat untuk dokter dan tanggal itu, tanpa dokter mengisi formulir kedua | `RWI-RULE-017` |
| `RWI-AC-033` | Kunjungan dokter yang tidak meninggalkan catatan tidak muncul di mana pun sebagai visite | `RWI-RULE-017` |
| `RWI-AC-034` | Perawat tidak dapat mencatatkan visite atas nama dokter | `RWI-RULE-017` |
| `RWI-AC-035` | Penutupan episode ditolak selama ada butir wajib daftar periksa administrasi yang belum ditandai, dan pesan penolakannya menyebut butir mana | `RWI-RULE-018` |
| `RWI-AC-036` | Admin dapat menambah dan menonaktifkan butir daftar periksa lewat master data, dan butir baru langsung berlaku tanpa program diubah | `RWI-RULE-018` |
| `RWI-AC-037` | Setiap penandaan butir daftar periksa menyimpan nama petugas dan waktu penandaannya | `RWI-RULE-018` |
| `RWI-AC-038` | Pasien yang masuk 12 Agustus pukul 22:40 dan pulang 15 Agustus pukul 09:00 menampilkan lama dirawat 3 hari | `RWI-RULE-019` |
| `RWI-AC-039` | Pasien yang masuk dan pulang pada tanggal yang sama menampilkan 1 hari, bukan 0 hari | `RWI-RULE-019` |
| `RWI-AC-040` | Untuk pasien yang masih dirawat, angka hari rawat bertambah satu setiap pergantian tanggal, bukan setiap genap 24 jam | `RWI-RULE-019` |
| `RWI-AC-041` | Hanya supervisor yang dapat membuka kembali episode `Closed`, dan reopen tanpa alasan ditolak | `RWI-RULE-020` |
| `RWI-AC-042` | Episode yang sedang dibuka untuk koreksi tidak muncul di census dan tidak menempati tempat tidur mana pun | `RWI-RULE-020` |
| `RWI-AC-043` | Lama dirawat sebuah episode tidak berubah setelah episode itu dibuka kembali lalu ditutup lagi | `RWI-RULE-020` |
| `RWI-AC-044` | Episode `Draft` yang tidak disentuh lebih dari 1 hari terbaca `Cancelled` pada pembacaan berikutnya, tanpa ada proses latar belakang yang dijalankan | `RWI-RULE-022` |
| `RWI-AC-045` | Kunjungan rawat inap yang dibuat untuk `Draft` yang gugur ikut ditandai batal dan tidak muncul pada laporan kunjungan | `RWI-RULE-022` |
| `RWI-AC-046` | Batas 1 hari dapat diubah admin dan nilai barunya langsung dipakai pada pembacaan berikutnya | `RWI-RULE-022` |
| `RWI-AC-047` | Dua catatan perkembangan dari dokter yang sama pada tanggal yang sama menghasilkan satu visite, dengan waktu mengikuti catatan pertama | `RWI-RULE-017` |
| `RWI-AC-048` | Catatan dari dua dokter berbeda pada tanggal yang sama menghasilkan dua visite | `RWI-RULE-017` |
| `RWI-AC-049` | Episode yang berstatus `DischargePending` lebih dari 4 jam muncul sebagai terlambat pada daftar penutupan tertunda, beserta lama keterlambatannya | `RWI-RULE-023` |
| `RWI-AC-050` | Ketiga ambang daftar pantau dapat diubah admin tanpa mengubah program | `RWI-RULE-023` |
| `RWI-AC-051` | Tidak ada daftar pantau yang menghalangi tindakan apa pun; ketiganya hanya memantau | `RWI-RULE-023` |
| `RWI-AC-052` | Resep yang ditandai obat pulang terkirim ke Farmasi dengan konteks encounter yang sama seperti resep harian | `RWI-RULE-024` |
| `RWI-AC-053` | Butir "obat pulang sudah diserahkan" dapat dinonaktifkan admin, dan setelah dinonaktifkan penutupan episode tidak lagi tertahan olehnya | `RWI-RULE-024` |
| `RWI-AC-054` | Perawat dapat menyimpan pengkajian awal untuk pasien rawat inap tanpa mengisi nomor antrean, dan pengkajian itu terbaca menempel pada kunjungan pasien tersebut | `RWI-RULE-026` |
| `RWI-AC-055` | Dokter dapat menyimpan catatan pemeriksaan pada hari pertama dan hari kedua untuk satu pasien rawat inap yang sama, dan keduanya tersimpan sebagai dua catatan terpisah | `RWI-RULE-026` |
| `RWI-AC-056` | Dokter dapat menyimpan resep pada hari pertama dan hari kedua untuk satu pasien rawat inap yang sama, dan resep hari kedua tidak ditolak walaupun resep hari pertama sudah diserahkan | `RWI-RULE-026` |
| `RWI-AC-057` | Untuk kunjungan bertipe rawat jalan, permintaan membuat konsultasi kedua tetap ditolak dengan pesan yang sama seperti sebelumnya, sehingga perilaku poliklinik tidak berubah | `RWI-RULE-026` |
| `RWI-AC-058` | Pasien rawat inap tidak muncul pada daftar antrean poliklinik mana pun, dan tidak ada baris antrean yang dibuat saat admisi diaktifkan | `RWI-RULE-026` |
| `RWI-AC-059` | Setelah pasien ditempatkan, sistem dapat menjawab siapa yang menempati bed tersebut dan sejak jam berapa, tanpa membaca berkas log | `RWI-RULE-027` |
| `RWI-AC-060` | Percobaan menyetel `BedStatus` menjadi `Occupied` atau `Reserved` lewat menu master data ditolak dengan pesan yang menjelaskan bahwa status itu hanya lahir dari tindakan Rawat Inap | `RWI-RULE-027` |
| `RWI-AC-061` | Menyetel `BedStatus` menjadi `Maintenance` lewat menu master data tetap berhasil, sehingga wewenang admin atas keadaan non-pasien tidak berkurang | `RWI-RULE-027` |
| `RWI-AC-062` | Bila penulisan catatan penempatan gagal, kolom `BedStatus` juga tidak berubah; tidak pernah ada keadaan hanya salah satu yang tersimpan | `RWI-RULE-027` |
| `RWI-AC-063` | Laporan selisih menampilkan bed yang kolom statusnya tidak cocok dengan catatan penempatannya, lengkap dengan nama pasien dan waktu mulai penempatan | `RWI-RULE-027` |
| `RWI-AC-064` | Setelah episode ditutup, bed yang tadinya ditempati terbaca `Available` pada pencarian bed kosong pada pembacaan berikutnya | `RWI-RULE-027` |
| `RWI-AC-065` | Episode yang baru dibuat berstatus kelayakan keuangan `Pending`, dan penutupannya ditolak selama masih `Pending` | `RWI-RULE-028` |
| `RWI-AC-066` | Petugas kasir dapat menandai episode menjadi `Cleared`, dan setelah itu penutupan episode berhasil tanpa perlu jalan keluar supervisor | `RWI-RULE-028` |
| `RWI-AC-067` | Percobaan menandai kelayakan keuangan oleh petugas admisi, perawat, atau dokter ditolak dengan kode 403 | `RWI-RULE-028` |
| `RWI-AC-068` | Penandaan tanpa catatan ditolak, dan penandaan yang berhasil menyimpan nama penandai beserta waktunya | `RWI-RULE-028` |
| `RWI-AC-069` | Layar episode dan laporan menampilkan bahwa status kelayakan keuangan berasal dari penandaan manual, bukan dari tagihan | `RWI-RULE-028` |
| `RWI-AC-070` | Episode berstatus `Blocked` tetap tidak dapat ditutup petugas admisi, dan hanya dapat ditutup supervisor dengan alasan wajib sesuai `RWI-RULE-009` | `RWI-RULE-028` |
| `RWI-AC-071` | Setelah disposisi `RANAP` dijalankan dan admisi diselesaikan, kunjungan IGD pasien tersebut terbaca sudah ditutup dan ada satu kunjungan baru bertipe rawat inap atas nama pasien yang sama | `RWI-RULE-029` |
| `RWI-AC-072` | Kunjungan IGD dan kunjungan rawat inap hasil serah terima terbaca sebagai satu rangkaian kedatangan, sehingga riwayat pasien dapat ditelusuri dari IGD sampai pulang | `RWI-RULE-029` |
| `RWI-AC-073` | Kunjungan rawat inap hasil serah terima membawa unit layanan, kelas pasien, dan DPJP sesuai keputusan admisi, bukan warisan dari IGD | `RWI-RULE-029` |
| `RWI-AC-074` | Bila penempatan tempat tidur ditolak karena bed sudah diambil pasien lain, kunjungan IGD tetap terbuka dan tidak ada kunjungan rawat inap yang terbentuk | `RWI-RULE-029` |
| `RWI-AC-075` | Catatan klinis yang ditulis selama pasien di IGD tetap terbaca menempel pada kunjungan IGD, tidak berpindah dan tidak tersalin ke kunjungan rawat inap | `RWI-RULE-029` |
| `RWI-AC-076` | Karena kunjungan jangkar bertipe rawat inap, dokter dapat menulis catatan perkembangan hari kedua dan seterusnya untuk pasien yang masuk lewat IGD | `RWI-RULE-026`, `RWI-RULE-029` |
| `RWI-AC-077` | Untuk disposisi selain `RANAP`, misalnya `PULANG` atau `RUJUK`, tidak ada kunjungan rawat inap yang dibuat | `RWI-RULE-029` |
| `RWI-AC-078` | Sistem dapat menjawab siapa DPJP sebuah episode pada tanggal tertentu, bukan hanya siapa DPJP yang terakhir | `RWI-RULE-030` |
| `RWI-AC-079` | Permintaan perpindahan pasien oleh dokter yang bukan DPJP aktif episode itu ditolak, dan penolakannya tidak dapat dilewati dengan mengisi kolom keterangan apa pun | `RWI-RULE-030` |
| `RWI-AC-080` | Permintaan perpindahan oleh DPJP aktif diterima selama alasan medis diisi | `RWI-RULE-030` |
| `RWI-AC-081` | Pengalihan DPJP tanpa alasan ditolak; pengalihan yang berhasil menyimpan pengalih, waktu, dan alasannya | `RWI-RULE-030` |
| `RWI-AC-082` | Setelah DPJP dialihkan, baris DPJP sebelumnya tetap terbaca lengkap dengan masa berlakunya dan tidak tertimpa | `RWI-RULE-030` |
| `RWI-AC-083` | Dokter yang tanggung jawabnya sudah berakhir tidak lagi dapat meminta perpindahan pasien tersebut | `RWI-RULE-030` |
| `RWI-AC-084` | Sebuah episode aktif tidak pernah berada dalam keadaan tanpa DPJP aktif maupun dengan dua DPJP aktif sekaligus | `RWI-RULE-030` |
| `RWI-AC-085` | Setelah episode berjalan dari `Draft` sampai `Closed`, seluruh perpindahan statusnya terbaca urut lengkap dengan pelaku, waktu, dan alasan | `RWI-RULE-031` |
| `RWI-AC-086` | Bila penulisan baris riwayat gagal, status episode juga tidak berubah; tidak pernah ada status yang berpindah tanpa baris riwayat | `RWI-RULE-031` |
| `RWI-AC-087` | Baris riwayat yang sudah tersimpan tidak dapat diubah maupun dihapus lewat endpoint mana pun | `RWI-RULE-031` |
| `RWI-AC-088` | Pemesanan bed yang gugur karena lewat 2 jam meninggalkan baris riwayat bertanda dilakukan sistem, tanpa nama orang | `RWI-RULE-031` |
| `RWI-AC-089` | Episode `Draft` yang batal sendiri setelah 1 hari meninggalkan baris riwayat bertanda dilakukan sistem | `RWI-RULE-031` |
| `RWI-AC-090` | Laporan penutupan tanpa kelayakan keuangan dapat disusun dari tabel riwayat ini tanpa membaca berkas log | `RWI-RULE-031` |
| `RWI-AC-091` | Pembatalan admisi pada episode `Admitted` ditolak bila riwayat menunjukkan sudah ada catatan klinis, sesuai `RWI-RULE-004` | `RWI-RULE-031` |
| `RWI-AC-092` | Satu episode hanya dapat punya satu resume pulang; percobaan membuat resume kedua ditolak | `RWI-RULE-032` |
| `RWI-AC-093` | Resume pulang menampilkan DPJP beserta periodenya secara otomatis dari catatan DPJP, tanpa diketik ulang petugas | `RWI-RULE-032` |
| `RWI-AC-094` | Penutupan episode ditolak selama resume pulang belum ditandatangani DPJP | `RWI-RULE-032` |
| `RWI-AC-095` | Untuk cara pulang meninggal, resume tidak meminta instruksi kontrol dan obat pulang, tetapi mewajibkan waktu dan sebab kematian | `RWI-RULE-032` |
| `RWI-AC-096` | Untuk cara pulang dirujuk, resume menolak disimpan bila tujuan rujukan belum diisi | `RWI-RULE-032` |
| `RWI-AC-097` | Setelah episode ditutup, resume tidak dapat diubah; percobaan mengubahnya ditolak dengan pesan yang mengarahkan pada pembukaan kembali episode | `RWI-RULE-032` |
| `RWI-AC-098` | Resep yang ditandai obat pulang terbaca sebagai obat pulang pada layar Farmasi, berbeda tampilannya dari resep harian | `RWI-RULE-024` |
| `RWI-AC-099` | Setelah Farmasi menyerahkan obat pulang, butir "obat pulang sudah diserahkan" pada daftar periksa administrasi tertutup otomatis tanpa petugas admisi menandainya manual | `RWI-RULE-024` |
| `RWI-AC-100` | Resep rawat jalan tidak terpengaruh penanda ini dan tetap berperilaku seperti sebelumnya | `RWI-RULE-024` |
| `RWI-AC-101` | Kepala ruangan dapat menugaskan perawat penanggung jawab pada satu episode, dan census menampilkan nama perawat itu | `RWI-RULE-033` |
| `RWI-AC-102` | Percobaan menugaskan perawat oleh peran selain kepala ruangan ditolak dengan kode 403 | `RWI-RULE-033` |
| `RWI-AC-103` | Penggantian perawat menutup baris lama dengan waktu berakhir dan membuka baris baru; baris lama tetap terbaca | `RWI-RULE-033` |
| `RWI-AC-104` | Episode yang belum punya perawat penanggung jawab tetap dapat menerima pengkajian, catatan, dan perpindahan tanpa tertahan | `RWI-RULE-033` |
| `RWI-AC-105` | Episode tanpa perawat penanggung jawab muncul pada daftar pantau kepala ruangan | `RWI-RULE-033` |
| `RWI-AC-106` | Seeder data master contoh menolak dijalankan pada lingkungan produksi | `RWI-DEC-048` |
| `RWI-AC-107` | Admin dapat menambah kamar dan tempat tidur baru lewat layar master yang sudah ada, tanpa perintah database | `RWI-DEC-048` |
| `RWI-AC-108` | Tombol nonaktifkan pada halaman detail tempat tidur berhasil menonaktifkan bed, dan bed itu hilang dari hasil pencarian bed kosong | `RWI-DEC-049` |
| `RWI-AC-109` | Tombol aktifkan pada halaman detail tempat tidur berhasil mengaktifkan kembali bed yang sebelumnya dinonaktifkan | `RWI-DEC-049` |
| `RWI-AC-110` | Admin dapat mengubah batas pemesanan tempat tidur dari 2 jam menjadi 3 jam lewat layar pengaturan, dan nilai baru berlaku pada pembacaan berikutnya tanpa aplikasi dinyalakan ulang | `RWI-RULE-034` |
| `RWI-AC-111` | Kelima angka pada `RWI-RULE-034` aturan nomor 2 dapat diubah dari satu layar yang sama | `RWI-RULE-034` |
| `RWI-AC-112` | Setiap perubahan nilai pengaturan menyimpan nama pengubah dan waktunya | `RWI-RULE-034` |
| `RWI-AC-113` | Butir daftar periksa administrasi tetap dikelola dari layar tersendiri, bukan dari layar pengaturan | `RWI-RULE-034` |
| `RWI-AC-114` | Setiap task yang menyentuh modul Klinis, Farmasi, Master Data, atau IGD membawa test regresi untuk jalur lama yang disentuhnya, dan task tidak dianggap selesai tanpa test itu | `RWI-DEC-051` |
| `RWI-AC-115` | Penjaga kewenangan DPJP pada `RWI-RULE-030` punya test yang membuktikan dokter bukan DPJP ditolak | `RWI-DEC-051` |
| `RWI-AC-116` | Menempatkan pasien yang sudah punya episode berstatus `Admitted` ditolak, disertai pesan yang menyebut nomor episode dan lokasi yang sedang ditempati | `RWI-RULE-035` |
| `RWI-AC-117` | Membuka admisi untuk pasien yang punya episode `Draft` lain tetap berhasil, dan sistem menampilkan peringatan tentang admisi yang sedang disiapkan itu | `RWI-RULE-035` |
| `RWI-AC-118` | Mencatat kepergian fisik pasien melepas tempat tidur seketika, dan tempat tidur itu muncul pada pencarian tempat tidur kosong pada pembacaan berikutnya | `RWI-RULE-036` |
| `RWI-AC-119` | Setelah kepergian dicatat, episode tetap berstatus `DischargePending` dan tetap wajib ditutup | `RWI-RULE-036` |
| `RWI-AC-120` | Pasien yang sudah dicatat pergi tidak lagi muncul pada census dan tidak dapat dipindahkan | `RWI-RULE-036` |
| `RWI-AC-121` | Menutup episode tanpa mencatat kepergian lebih dulu tetap berhasil, dan tempat tidur dilepas saat penutupan | `RWI-RULE-036` |
| `RWI-AC-122` | Episode bayi dapat menyimpan rujukan ke episode ibunya, dan sistem dapat menjawab bayi siapa yang berada di boks kamar tertentu | `RWI-DEC-056` |
| `RWI-AC-123` | Menutup episode ibu tidak menutup episode bayinya dan tidak melepas boks bayi | `RWI-DEC-056` |
| `RWI-AC-124` | Menyunting resume yang belum ditandatangani tidak membuat versi baru | `RWI-DEC-057` |
| `RWI-AC-125` | Mengubah resume yang sudah ditandatangani lewat sesi koreksi menyimpan salinan versi sebelumnya | `RWI-DEC-057` |
| `RWI-AC-126` | Versi resume yang tersimpan tidak dapat diubah maupun dihapus | `RWI-DEC-057` |
| `RWI-AC-127` | Riwayat lokasi satu episode terbaca lengkap dari catatan penempatan milik Rawat Inap, tanpa perlu membaca tabel milik modul Registrasi | `RWI-DEC-053` |
| `RWI-AC-128` | Penempatan pasien perempuan ke tempat tidur yang hanya menerima laki-laki ditolak | `RWI-RULE-012` |
| `RWI-AC-129` | Pasien yang jenis kelaminnya belum tercatat hanya dapat ditempatkan pada tempat tidur yang menerima keduanya, dan hanya ke kamar yang belum ada penghuninya | `RWI-RULE-012` |
| `RWI-AC-130` | Penempatan ke kamar yang sudah dihuni pasien berjenis kelamin berbeda ditolak, disertai pesan yang menyebut kamarnya | `RWI-RULE-012` |
| `RWI-AC-131` | Bayi pada boks bayi tidak menghalangi penempatan pasien lain di kamar yang sama, apa pun jenis kelaminnya | `RWI-RULE-012` |
| `RWI-AC-132` | Penempatan bayi ke boks bayi di kamar ibunya berhasil walaupun jenis kelamin bayi berbeda dari ibunya | `RWI-RULE-012` |
| `RWI-AC-133` | Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda ditolak, sama seperti penempatan | `RWI-RULE-012` |
| `RWI-AC-134` | Pasien yang ditandai membutuhkan isolasi hanya dapat ditempatkan pada tempat tidur isolasi | `RWI-RULE-012` |
| `RWI-AC-135` | Pasien yang tidak membutuhkan isolasi ditolak saat ditempatkan pada tempat tidur isolasi | `RWI-RULE-012` |
| `RWI-AC-136` | Petugas admisi dapat merekam kebutuhan isolasi saat episode masih `Draft`, dan nilainya tertandai sebagai catatan awal | `RWI-RULE-012` |
| `RWI-AC-137` | Setelah episode aktif, hanya DPJP aktif yang dapat mengubah kebutuhan isolasi, dan perubahannya tertandai sebagai keputusan klinis | `RWI-RULE-012` |
| `RWI-AC-138` | Mengubah kebutuhan isolasi menjadi ya saat pasien berada di tempat tidur biasa tetap diterima, dan episode itu muncul pada daftar pantau penempatan tidak sesuai | `RWI-RULE-012` |
| `RWI-AC-139` | Percobaan mengubah kebutuhan isolasi oleh dokter yang bukan DPJP aktif ditolak | `RWI-RULE-012` |
| `RWI-AC-140` | Perawat dapat menyimpan pengkajian untuk pasien pada kunjungan bertipe IGD tanpa satu pun baris antrean dibuat | `RWI-RULE-026` |
| `RWI-AC-141` | Dokter jaga shift kedua dapat menulis catatan konsultasi kedua pada satu kunjungan IGD yang sama, dan keduanya tersimpan sebagai dua catatan terpisah | `RWI-RULE-026` |
| `RWI-AC-142` | Resep kedua pada satu kunjungan IGD tidak ditolak walaupun resep pertama masih aktif | `RWI-RULE-026` |
| `RWI-AC-143` | Untuk kunjungan rawat jalan dan medical check-up, permintaan tanpa antrean tetap ditolak dengan kode dan pesan **sama persis** seperti sebelum perubahan | `RWI-RULE-026` |
| `RWI-AC-144` | Pasien IGD tidak muncul pada daftar antrean poliklinik mana pun, sebelum maupun sesudah perubahan | `RWI-RULE-026` |
| `RWI-AC-145` | Penempatan tempat tidur untuk pasien asal IGD ditolak selama event `Tiba` belum tercatat, dengan pesan yang menyebutkan sebabnya | `RWI-RULE-029` |
| `RWI-AC-146` | `InpBedPlacement.StartDateTime` untuk pasien asal IGD sama persis dengan waktu `Tiba` pada catatan kepergian IGD, bukan waktu penempatan dibuat | `RWI-RULE-029` |
| `RWI-AC-147` | Untuk jalur datang langsung dan poliklinik, `StartDateTime` tetap waktu penempatan dibuat dan tidak menunggu apa pun | `RWI-RULE-029` |
| `RWI-AC-148` | Kunjungan rawat inap hasil serah terima menyimpan Id kunjungan IGD pada `OriginEncounterId`, sehingga riwayat pasien terbaca sebagai satu rangkaian | `RWI-RULE-029` |
| `RWI-AC-149` | Kunjungan yang tidak berasal dari kunjungan lain menyimpan `OriginEncounterId` kosong, dan seluruh kunjungan lama tetap terbaca tanpa diubah | `RWI-RULE-029` |

---

## Open Questions dan Blocker

### Dibawa dari PRD bagian 22

| ID PRD | Pertanyaan | Memblokir | Status |
|---|---|---|---|
| OQ-RI-001 | Apakah admisi wajib selalu berasal dari rujukan atau encounter sebelumnya | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-011` |
| OQ-RI-002 | Apakah tempat tidur perlu status `Reserved` sebelum `Occupied` | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-007` |
| OQ-RI-003 | Berapa lama pemesanan tempat tidur boleh aktif | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-008` |
| OQ-RI-004 | Siapa yang punya kewenangan final atas transfer | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-012` |
| OQ-RI-005 | Apakah unit tujuan wajib menerima transfer lebih dulu | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-012` |
| OQ-RI-006 | Siapa yang mengeksekusi penutupan episode | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-016` |
| OQ-RI-007 | Apa daftar periksa wajib sebelum pasien pulang | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-026` |
| OQ-RI-008 | Apakah kelayakan keuangan memblokir atau hanya memberi peringatan | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-015` |
| OQ-RI-009 | Persetujuan umum apa saja yang wajib saat masuk | `LATER SLICE` | `DIJAWAB` oleh `RWI-DEC-035`, menunggu pemilik privasi |
| OQ-RI-010 | Apakah bayi baru lahir, ICU, dan isolasi masuk MVP pertama | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-020` |
| OQ-RI-011 | Apakah rencana asuhan keperawatan SDKI wajib pada MVP pertama | `LATER SLICE` | `TERTUTUP` oleh `RWI-DEC-034`, turunan `RWI-DEC-004` |
| OQ-RI-012 | Siapa yang berhak membuka kembali episode yang sudah ditutup | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-028` |

### Tambahan dari agent

| ID | Pertanyaan | Memblokir | Status |
|---|---|---|---|
| `RWI-OQ-013` | Cara pulang apa saja yang diakui selain pulang atas izin DPJP | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-017` |
| `RWI-OQ-014` | Bagaimana pasien meninggal diperlakukan | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-017` |
| `RWI-OQ-015` | Apakah pindah kelas perawatan termasuk MVP dan bagaimana dicatat | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-013` |
| `RWI-OQ-016` | Apakah pasien titipan termasuk MVP | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-019` — di luar MVP |
| `RWI-OQ-017` | Apakah pemisahan jenis kelamin dan isolasi adalah aturan keras | `DESIGN` | `DIJAWAB` oleh `RWI-DEC-018`, menunggu pemilik klinis dan privasi |
| `RWI-OQ-018` | Batas waktu pengkajian awal dan aturan verifikasi CPPT oleh DPJP | `IMPLEMENTATION` | `DIJAWAB` oleh `RWI-DEC-029`, menunggu pemilik klinis |
| `RWI-OQ-019` | Apa yang dihitung sebagai satu visite dokter | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-025` |
| `RWI-OQ-020` | Bagaimana lama dirawat dihitung | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-027` |
| `RWI-OQ-021` | Bagaimana obat pulang diperlakukan | `LATER SLICE` | `TERTUTUP` oleh `RWI-DEC-033` |
| `RWI-OQ-022` | Siapa yang boleh membatalkan admisi dan apa akibatnya pada tempat tidur | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-010` |
| `RWI-OQ-023` | Siapa pemilik keputusan dan siapa yang berwenang approve modul ini | `DESIGN` | `TERBUKA` — tindakan organisasi, tidak dapat diselesaikan wawancara. Lihat `RWI-DEC-037` |
| `RWI-OQ-024` | Apa yang terjadi bila petugas menyelesaikan admisi setelah pemesanan tempat tidurnya gugur dan tempat tidur itu sudah diambil pasien lain | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-021` |
| `RWI-OQ-025` | Apa isi "sesuai SOP" untuk Dokter/DPJP pada baris Transfer tabel kewenangan PRD bagian 14 | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-022` |
| `RWI-OQ-026` | Apakah perpindahan yang mengubah kelas perawatan dikecualikan dari kewenangan perawat pelaksana | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-013` |
| `RWI-OQ-027` | Bentuk daftar pantau episode yang sudah boleh pulang tetapi belum ditutup: isinya apa, siapa yang memantau, dan berapa lama dianggap terlalu lama menggantung | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-032` |
| `RWI-OQ-028` | Bagaimana episode `Draft` yang ditinggalkan berhari-hari dibersihkan, dan siapa yang membersihkannya | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-030` |
| `RWI-OQ-029` | Apakah "koordinasi dengan DPJP terkait" harus meninggalkan jejak di sistem, atau cukup diselesaikan antar dokter di luar sistem | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-024` |
| `RWI-OQ-030` | Bila dokter menulis dua catatan perkembangan dalam satu hari, apakah dihitung satu visite atau dua | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-031` |
| `RWI-OQ-031` | Bentuk daftar pantau kepatuhan pengkajian awal dan verifikasi CPPT: isinya apa dan siapa yang menindaklanjuti | `IMPLEMENTATION` | `TERTUTUP` oleh `RWI-DEC-032` |
| `RWI-OQ-032` | Siapa pemilik modul `ClinicalManagement` dan `PharmacyManagement`, dan apakah mereka menyetujui pelonggaran yang dituntut `RWI-RULE-026` | `IMPLEMENTATION` | `TERBUKA` — tindakan organisasi. Desain boleh berjalan, implementasi tidak boleh dimulai sebelum persetujuan ini ada |
| `RWI-OQ-036` | Siapa nama orang atau unit yang bertanggung jawab mengisi data master tempat tidur, kamar, unit layanan, dan kelas pasien sebelum modul dipakai, dan kapan batas waktunya | `IMPLEMENTATION` | `TERBUKA` — tindakan organisasi. Aturannya sudah dikunci `RWI-DEC-048`, tetapi nama penanggung jawabnya tidak dapat dikarang agent |
| `RWI-OQ-035` | Berapa lama baris riwayat perubahan status episode wajib disimpan sebelum boleh diarsipkan, dan siapa yang berwenang menyetujuinya | `LATER SLICE` | `DIJAWAB` oleh `RWI-DEC-060` — sampai angkanya ditetapkan, tidak ada pengarsipan maupun penghapusan otomatis sama sekali. Menunggu pemilik hukum untuk menaikkannya ke `approved` |
| `RWI-OQ-034` | Siapa pemilik modul `EmergencyInstallationManagement`, dan apakah pemilik itu menyetujui serah terima IGD ke rawat inap yang dituntut `RWI-RULE-029`, termasuk menjalankan penanda `ClosesEmergencyVisit` yang selama ini tidak pernah dipakai | `IMPLEMENTATION` | `TERBUKA` — bagian **siapa** sudah terjawab: pemiliknya **Rizki Gunawan**, ditetapkan `RWI-DEC-069` 2026-08-24. Bagian **persetujuan** menunggu `IGD-DEC-067` dinaikkan dari `draft` ke `approved` atas nama Rizki. Desain boleh berjalan, implementasi `INP-S09` tidak boleh dimulai sebelum itu |
| `RWI-OQ-033` | Siapa pemilik modul `MasterData` HealthServices, dan apakah pemilik itu menyetujui pembatasan endpoint `/beds/{id}/availability` yang dituntut `RWI-RULE-027` | `IMPLEMENTATION` | `TERBUKA` — tindakan organisasi. Desain boleh berjalan, implementasi tidak boleh dimulai sebelum persetujuan ini ada |
| `RWI-OQ-037` | Siapa pemilik pengiriman data rawat inap ke SATUSEHAT, data apa yang wajib dikirim, kapan dipicu, dan di mana riwayat lokasi pasien disimpan | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-053` untuk bagian **kepemilikan data**: riwayat lokasi tetap milik Rawat Inap. Bagian isi kiriman dan pemicunya tetap di luar MVP dan menunggu pemilik integrasi |
| `RWI-OQ-038` | Apakah serah terima klinis antar shift keperawatan wajib direkam sistem, dan apa isi minimalnya | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-058` untuk bagian **scope**: tidak masuk MVP. Bagian **isi** serah terima tetap terbuka dan menunggu pemilik klinis |
| `RWI-OQ-039` | Aturan klinis pasien meninggal dan pasien kabur: siapa mencatat, dokumen wajib, apakah resume tetap wajib, kapan bed dilepas, dan pelaporannya | `DESIGN` | `DIJAWAB` oleh `RWI-DEC-059` dan dirinci pada `RWI-RULE-037`. **Tetap di luar MVP.** Menunggu pemilik klinis untuk menaikkannya ke `approved` |
| `RWI-OQ-040` | Apakah sistem perlu menyimpan penanda bahwa seorang bayi dirawat gabung dengan ibunya | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-056` — ya, berupa satu rujukan opsional pada episode bayi |
| `RWI-OQ-041` | Apakah kepergian fisik pasien dicatat sebagai kejadian tersendiri sehingga tempat tidur dapat dilepas lebih awal | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-055` — ya, sebagai tindakan tersendiri yang melepas tempat tidur seketika. Dirinci pada `RWI-RULE-036` |
| `RWI-OQ-042` | Apakah satu pasien boleh punya dua episode rawat inap aktif sekaligus | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-054` — dilarang untuk status `Admitted` dan `DischargePending`; `Draft` hanya diberi peringatan. Dirinci pada `RWI-RULE-035` |
| `RWI-OQ-044` | Di mana kebutuhan isolasi dicatat dan siapa menetapkannya; bagaimana membedakan kamar yang boleh ditempati campur | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-065` dan `RWI-DEC-066` — kebutuhan isolasi menjadi atribut episode; seluruh kamar dianggap tidak boleh campur tanpa kolom baru |
| `RWI-OQ-043` | Apakah resume pulang perlu menyimpan riwayat versi | `LATER SLICE` | `TERTUTUP` oleh `RWI-DEC-057` — ya, tetapi hanya versi yang sudah ditandatangani |

### Blocker desain saat ini

**Diperbarui 2026-08-21 setelah Closure Pass.** Tidak ada lagi blocker yang menghalangi
penyusunan desain. Yang tersisa adalah blocker implementasi dan blocker sign-off.

**Tidak ada blocker desain.** Seluruh 17 pertanyaan penutup capability map sudah tertutup, dan
keempat butir yang tadinya memblokir desain sudah diputuskan:

| Butir yang tadinya memblokir desain | Ditutup oleh |
|---|---|
| Ketergantungan dokumentasi klinis pada antrean dan konsultasi | `RWI-DEC-038`, `RWI-RULE-026` |
| Sumber kebenaran penghunian tempat tidur | `RWI-DEC-039`, `RWI-RULE-027` |
| Gerbang keuangan tanpa Billing operasional | `RWI-DEC-040`, `RWI-RULE-028` |
| Nasib kunjungan IGD saat pasien naik ke bangsal | `RWI-DEC-041`, `RWI-RULE-029` |

**Blocker implementasi — desain boleh disusun, source code belum boleh ditulis:**

1. Persetujuan pemilik empat modul tetangga berstatus `ACTIVE` yang akan disentuh, yaitu
   `ClinicalManagement`, `PharmacyManagement`, `MasterData`, dan `EmergencyInstallationManagement`.
   Lihat `RWI-OQ-032`, `RWI-OQ-033`, dan `RWI-OQ-034`. **Diperbarui 2026-08-24:** `RWI-OQ-032` dan
   `RWI-OQ-033` tertutup oleh `RWI-DEC-062`. `RWI-OQ-034` tetap terbuka — `EmergencyInstallationManagement`
   dimiliki Rizki Gunawan, bukan pemilik `RWI-DEC-061`, sesuai `RWI-DEC-069`. Yang tertahan olehnya
   hanya `INP-S09`, yang memang di luar MVP.
2. Kesiapan data master tempat tidur, kamar, unit layanan, dan kelas pasien. Aturannya sudah
   dikunci `RWI-DEC-048`, penanggung jawabnya belum. Lihat `RWI-OQ-036`.
3. Perbaikan tombol aktifkan dan nonaktifkan tempat tidur yang hari ini selalu gagal. Lihat
   `RWI-DEC-049`.
4. Modul masih berstatus `PLANNED` pada registry, sehingga belum ada izin implementasi.

**Blocker sign-off — modul tidak boleh melayani pasien sungguhan:**

5. Pemilik klinis dan pemilik keamanan/privasi belum ditunjuk. `RWI-DEC-018`, `RWI-DEC-029`, dan
   `RWI-DEC-035` sudah dijawab tetapi **tidak dapat naik ke `approved`**, dan `RWI-DEC-045`
   bagian daftar isi resume juga menunggu peninjauan klinis.
6. `RWI-OQ-023` belum terjawab: siapa nama orang atau komite yang berwenang menyetujui modul ini.
   Lihat `RWI-DEC-037`.

**Risiko yang sudah diterima secara sadar dan perlu dipantau:**

| ID | Risiko | Penurunannya |
|---|---|---|
| `RWI-RISK-001` | Perawat pelaksana dapat memindahkan pasien sehingga kelas perawatan berubah tanpa persetujuan | Diterima sadar lewat `RWI-DEC-012` dan `RWI-DEC-013` |
| `RWI-RISK-002` | Pelonggaran mesin klinis menyentuh alur poliklinik yang tidak punya satu pun test | Diturunkan `RWI-DEC-051` yang mewajibkan test regresi per task |
| `RWI-RISK-003` | Petugas kasir dapat menandai `Cleared` tanpa tagihan yang sungguh ada | Hilang setelah `RWI-RULE-028` aturan nomor 7 dijalankan, yaitu saat Billing punya kemampuan transaksi |
| `RWI-RISK-004` | Penjaga kewenangan DPJP hanya bekerja bila dipanggil; endpoint baru yang lupa memanggilnya lolos diam-diam | Diturunkan `RWI-DEC-051` dan acceptance criteria `RWI-AC-115` |

Hal berikut **sudah bukan** blocker:

- Kemungkinan tumpang tindih dengan modul yang sudah ada, tertutup 2026-08-21 lewat capability map
  `01-existing-capability-map.md` revision `1.1`. Lihat `RWI-DEC-001`.

- Batas scope, tertutup 2026-08-20 lewat `RWI-DEC-004` dan `RWI-DEC-005`.
- Kebuntuan approval umum, tertutup 2026-08-20 lewat `RWI-DEC-006`; keputusan produk dan alur
  kerja sekarang boleh naik ke `approved`.
- Aturan pemesanan tempat tidur, tertutup lewat `RWI-DEC-007` dan `RWI-DEC-008`.
- Model status episode, tertutup lewat `RWI-DEC-009`.
- Aturan pembatalan admisi, tertutup lewat `RWI-DEC-010`.
- Jalur masuk admisi, tertutup lewat `RWI-DEC-011`.
- Kewenangan dan cara kerja perpindahan pasien, tertutup lewat `RWI-DEC-012`.
- Perlakuan pindah kelas perawatan, tertutup lewat `RWI-DEC-013`. Risiko `RWI-RISK-001`
  diterima secara sadar dan tidak lagi dihitung sebagai blocker.
- Perilaku saat perpindahan gagal, tertutup lewat `RWI-DEC-014`, sekaligus menegaskan INV-02.
- Sifat gerbang keuangan sebelum penutupan, tertutup lewat `RWI-DEC-015`.
- Kewenangan penutupan episode, tertutup lewat `RWI-DEC-016`.
- Cara pulang selain izin DPJP, tertutup lewat `RWI-DEC-017`, dengan catatan sisi klinisnya
  tetap menunggu pemilik klinis.
- Perilaku penempatan terkait jenis kelamin dan isolasi sudah **dijawab** lewat `RWI-DEC-018`,
  sehingga tidak lagi memblokir desain, tetapi tetap memblokir produksi.
- Nasib pasien titipan, tertutup lewat `RWI-DEC-019` sebagai di luar MVP.
- Perlakuan bayi baru lahir dan ICU, tertutup lewat `RWI-DEC-020`.
- Admisi yang diselesaikan setelah pemesanan gugur, tertutup lewat `RWI-DEC-021`.
- Pembagian kewenangan DPJP atas perpindahan, tertutup lewat `RWI-DEC-022`.
- Batas antara yang dijaga sistem dan yang menjadi pertimbangan DPJP, tertutup lewat
  `RWI-DEC-023`, sekaligus menyelamatkan `RWI-DEC-012` dari `superseded`.
- Perlakuan dokter yang bukan DPJP episode, tertutup lewat `RWI-DEC-024`.
- Definisi visite dokter, tertutup lewat `RWI-DEC-025`, sekaligus menutup satu kemampuan MUST
  yang sebelumnya sama sekali tidak punya bentuk.
- Isi clearance administrasi, tertutup lewat `RWI-DEC-026`, sekaligus menambal syarat ketiga
  `RWI-RULE-010` yang sebelumnya kosong.
- Cara menghitung lama dirawat, tertutup lewat `RWI-DEC-027`.
- Kewenangan dan batas membuka kembali episode, tertutup lewat `RWI-DEC-028`.
- Bentuk aturan batas waktu pengkajian dan verifikasi CPPT sudah **dijawab** lewat
  `RWI-DEC-029`, sehingga tidak lagi memblokir desain, tetapi tetap memblokir produksi.
- Pembersihan episode `Draft` yang ditinggalkan, tertutup lewat `RWI-DEC-030`.
- Cara menghitung jumlah visite, tertutup lewat `RWI-DEC-031`.
- Penanggung jawab dan ambang ketiga daftar pantau, tertutup lewat `RWI-DEC-032`.
- Perlakuan obat pulang, tertutup lewat `RWI-DEC-033`.
- Rencana asuhan keperawatan SDKI, tertutup lewat `RWI-DEC-034` sebagai turunan `RWI-DEC-004`.
- Bentuk persetujuan umum saat masuk sudah **dijawab** lewat `RWI-DEC-035`, sehingga tidak lagi
  memblokir desain, tetapi tetap memblokir produksi.
- **Seluruh tujuh konflik PRD (`RWI-CON-001` sampai `RWI-CON-007`) sudah tertutup** per
  2026-08-20.

---

## Riwayat Pass

| Tanggal | Pass | Ringkasan |
|---|---|---|
| 2026-08-27 | Pembahasan ulang arsitektur frontend, revision `0.4` | Lima pertanyaan diajukan dan dijawab pemilik setelah 18 task frontend selesai tetapi `FLOW-RI-MVP-001` ternyata tidak dapat dijalankan. Audit menemukan tiga cacat pada `03-frontend-architecture.md` revision `0.3`: daftar layar tidak pernah diadu dengan alur bisnis, keterjangkauan diserahkan penuh ke pelaksana, dan aksi disebut pada matriks peran tanpa layar pemilik — akibatnya **sembilan operasi HTTP** yang sudah jadi tidak pernah dipanggil dan layar sesi koreksi tidak dapat dicapai siapa pun. `RWI-DEC-075` mengubah admisi menjadi alur berlangkah dua jalur; `RWI-DEC-076` menetapkan tulisan bertahap dan memisahkan penempatan dari admisi; `RWI-DEC-077` memilih cetak persetujuan tanpa menyimpan sehingga `DEC-INP-003` tidak tersentuh; `RWI-DEC-078` menaikkan keterjangkauan menjadi wewenang blueprint lewat `IA-INP-01` s.d. `IA-INP-05`; `RWI-DEC-079` mengganti total layar admisi lama. Empat layar baru `FE-INP-16` s.d. `FE-INP-19`. Dua butir terbuka baru `RWI-OQ-045` dan `RWI-OQ-046`. **Nol perubahan backend, nol tabel baru, nol endpoint baru** — cacat penjamin tertutup karena alur baru memakai `POST /patient-encounters` milik Registrasi. Roadmap frontend naik ke revision `3` dengan 16 task baru; `backend-roadmap.md` tidak berubah |
| 2026-08-24 | Blueprint revision `4` dan sinkronisasi roadmap | Empat keputusan Amendment Pass diserap ke seluruh berkas blueprint lewat `/qv-design`: Kelayakan Penempatan tumbuh menjadi sembilan aturan, dua integrasi arah baca baru `INT-INP-06` dan `INT-INP-07`, asal `InpBedPlacement.StartDateTime` berubah untuk jalur serah terima, dan sepuluh acceptance criteria masuk matriks test. Seluruh kontrak naik ke `0.4.0`. Ditemukan bahwa **tidak satu pun task MVP tertahan**, karena aturan barunya hanya menyala pada `INP-S09`. Revision `4` disetujui Muhammad Hamzah lewat `RWI-DEC-074`, lalu ketiga roadmap disinkronkan ke `roadmap_revision` `2` |
| 2026-08-24 | Amendment Pass, tiga usulan lintas modul dari IGD | Empat pertanyaan diajukan dan dijawab pemilik. `RWI-DEC-070` memperluas pelonggaran `RWI-RULE-026` ke kunjungan `Emergency` mencakup aturan 3, 4, dan 5 — melampaui `IGD-DEC-068` yang hanya menyebut aturan 3 — dan merevisi aturan 6. `RWI-DEC-071` mempertahankan `RWI-DEC-041` tetapi menulis ulang justifikasinya, setelah ditemukan bahwa satu-satunya alasan yang tercatat gugur oleh keputusan sebelumnya. `RWI-DEC-072` menjadikan event `Tiba` milik IGD sebagai sumber kebenaran waktu tiba dan menambah `RWI-RULE-029` aturan 8. `RWI-DEC-073` menempatkan pekerjaan kolom `OriginEncounterId` di sisi IGD dan menulis ulang `compatibility_impact`. Sepuluh acceptance criteria baru `RWI-AC-140` s.d. `RWI-AC-149`. `RWI-DEC-038` di-`superseded` sebagian |
| 2026-08-24 | Koreksi kepemilikan modul tetangga | Pemilik `EmergencyInstallationManagement` diketahui bernama **Rizki Gunawan**, bukan pemilik `RWI-DEC-061`. `RWI-DEC-069` ditulis; `RWI-DEC-062` di-`superseded` sebagian pada bagian IGD; `RWI-OQ-034` dan `DEC-INP-002` terbuka kembali dengan pemilik yang bernama, menunggu `IGD-DEC-067` dinaikkan ke `approved` oleh Rizki. Gerbang “Persetujuan pemilik modul tetangga” menjadi terbuka sebagian dan hanya menahan `INP-S09`, yang memang di luar MVP. **Tidak ada isi desain yang berubah** |
| 2026-08-21 | Blueprint revision `3` | Ketiga keputusan `RWI-DEC-064` s.d. `RWI-DEC-066` diserap ke seluruh berkas blueprint. `EPIC RI-34` ditulis beserta `FR-RI-154` s.d. `FR-RI-162`, lima skenario UAT `UAT-29` s.d. `UAT-33`, dan bagian 2A pada matriks acceptance test berisi 26 skenario yang menutup seluruh `RWI-AC-128` s.d. `RWI-AC-139`. Kemampuan "penolakan penempatan karena isolasi atau jenis kelamin" berpindah dari daftar ditunda ke dalam MVP, dan `INP-S11` berpindah dari slice yang dihentikan menjadi slice yang dirancang. Satu gerbang keras sebelum produksi berubah bentuk: bukan lagi menunggu keputusan, melainkan menunggu `EPIC RI-34` lolos uji. Seluruh contract naik ke `0.3.0`; dua di antaranya — state transition dan integration — naik versi **tanpa** berubah isinya, disertai catatan kenapa |
| 2026-08-21 | Penutupan `RWI-OQ-044` | Dua sub-keputusan yang melengkapi aturan keras jenis kelamin dan isolasi ditutup. Kebutuhan isolasi ditetapkan sebagai atribut episode rawat inap, keputusan klinisnya milik DPJP dan dapat diperbarui selama perawatan, sedangkan petugas admisi hanya boleh merekam catatan awal berdasarkan keterangan dokter pengirim supaya penempatan tidak menunggu slice dokumentasi klinis (`RWI-DEC-065`). Seluruh kamar dianggap tidak boleh ditempati campur, diperiksa dari penghuni yang sedang ada dan bukan dari penanda master, tanpa menambah kolom apa pun pada `MstRoom`; penghuni boks bayi dikecualikan dari kedua sisi pemeriksaan (`RWI-DEC-066`). `RWI-RULE-012` ditulis ulang penuh menjadi dua bagian, 12 acceptance criteria baru ditulis, dan nama pemilik berwenang diisi menjadi Muhammad Hamzah menggantikan penanda akun |
| 2026-08-21 | Penutupan butir organisasi | Empat butir yang selama ini hanya dapat diselesaikan lewat tindakan organisasi ditutup. Pemilik berwenang ditunjuk menggantikan pemegang sementara (`RWI-DEC-061`), sehingga kata "sementara" pada `RWI-DEC-006` tidak berlaku lagi. Kepemilikan `ClinicalManagement`, `PharmacyManagement`, `MasterData`, dan `EmergencyInstallationManagement` dinyatakan berada pada pemilik yang sama beserta persetujuannya (`RWI-DEC-062`), mencabut tiga gerbang implementasi sekaligus. Penanggung jawab pengisian data master ditetapkan beserta target 22 Agustus 2026 (`RWI-DEC-063`). Pemisahan jenis kelamin dan isolasi **diubah menjadi aturan keras** yang menolak penempatan (`RWI-DEC-064`), men-`superseded` `RWI-DEC-018` dan menulis ulang `RWI-RULE-012`. Ditemukan bahwa bagian isolasi **belum dapat dijalankan** karena tidak ada satu pun kolom di source yang mencatat kebutuhan isolasi seorang pasien; dicatat sebagai `RWI-OQ-044`. Butir terbuka turun dari delapan menjadi empat, dan tidak satu pun memblokir desain maupun implementasi MVP |
| 2026-08-21 | Business module blueprint revision 2 | Empat keputusan Amendment Pass diserap ke blueprint. Satu tabel baru `InpDischargeSummaryRevision`, tiga kolom baru pada `InpEpisode` yaitu waktu kepergian, pencatat kepergian, dan rujukan episode ibu; satu nilai enum baru `PatientDeparted`; satu endpoint baru pencatatan kepergian; satu invariant baru `INV-INP-10` beserta unique index parsial keempat; dan `INV-INP-01` dilonggarkan untuk episode yang pasiennya sudah pergi. Enam functional requirement baru `FR-RI-148` s.d. `FR-RI-153`, lima skenario UAT baru, 23 skenario acceptance test baru. Tidak ada kemampuan `MUST HAVE` yang dicabut, tidak ada epic baru, dan tidak ada gelombang pengiriman yang bergeser. Seluruh contract naik ke `0.2.0`. Manifest naik ke revision `2` dan penanda STALE dicabut. Satu artefak hulu dicatat tertinggal, yaitu arsitektur domain revision `0.1` |
| 2026-08-21 | Amendment pass revision 3 | Delapan butir terbuka ditangani. Pemilik kebutuhan kembali mendelegasikan seluruh jawaban kepada rekomendasi agent (`RWI-DEC-052`), dan delegasi itu dinyatakan **tidak berlaku surut maupun ke pass berikutnya**. Lima butir tertutup penuh: riwayat lokasi tetap milik Rawat Inap sehingga blueprint tidak perlu dibongkar (`RWI-DEC-053`), satu pasien satu episode aktif (`RWI-DEC-054`), kepergian fisik pasien melepas tempat tidur lebih awal (`RWI-DEC-055`), penanda rawat gabung bayi (`RWI-DEC-056`), dan riwayat versi resume (`RWI-DEC-057`). Tiga butir dijawab tetapi **tidak dapat naik ke `approved`**: scope serah terima antar shift tertutup sedangkan isinya tetap klinis (`RWI-DEC-058`), aturan meninggal dan kabur (`RWI-DEC-059`), dan masa simpan riwayat (`RWI-DEC-060`). `RWI-RULE-035`, `RWI-RULE-036`, dan `RWI-RULE-037` ditulis; dua belas acceptance criteria baru; satu baris gerbang keras baru. **Empat keputusan mengubah blueprint**, sehingga blueprint wajib naik revision lewat `/qv-design` sebelum dipakai |
| 2026-08-21 | Business module blueprint | Blueprint modul disusun untuk sembilan slice yang lolos arsitektur domain, menghasilkan 13 berkas canonical pada folder ini beserta [`blueprint-manifest.md`](./blueprint-manifest.md) revision `1` berstatus `draft`. Isi utamanya: dua bounded context, satu aggregate root `InpEpisode`, dua belas tabel baru berawalan `Inp` dan `MstInpatient`, **nol perubahan kolom** pada tabel milik modul lain, 13 epic `EPIC RI-21` s.d. `RI-33`, 47 functional requirement `FR-RI-101` s.d. `FR-RI-147`, 23 skenario UAT, dan 82 skenario acceptance test yang 28 di antaranya jalur gagal. Satu-satunya perubahan pada modul lain bersifat perilaku, yaitu `PATCH /beds/{id}/availability` menolak nilai terisi dan dipesan. Urutan pengiriman disusun sebagai lima gelombang `MVP-0` s.d. `MVP-4`; delapan kemampuan yang ditunda seluruhnya berada di `POST-MVP` dan tidak satu pun masuk gelombang. Empat pertanyaan ditandai memblokir sebelum development lock |
| 2026-08-21 | Hospital domain architecture | Arsitektur domain disusun untuk sembilan slice yang lolos gerbang requirement, menghasilkan [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) revision `0.1` berstatus `draft`. Kesiapan `DOMAIN_ARCHITECTURE_PARTIAL`. Dua bounded context ditetapkan, 14 konsep dimiliki Rawat Inap, 10 konsep dipakai ulang tanpa duplikasi, 8 konsep ditolak beserta alasannya, dan sepuluh invariant dinyatakan. Dua ketegangan diselesaikan secara arsitektur tanpa mengubah keputusan bisnis: pemesanan tempat tidur yang gugur membuat episode `Draft` boleh hidup tanpa pemesanan (`INV-INP-09`), dan pembukaan kembali episode dimodelkan sebagai sesi koreksi supaya tidak melanggar `RWI-DEC-009` yang mengunci lima status. Tujuh gap arsitektur dicatat; empat di antaranya dikembalikan ke wawancara sebagai `RWI-OQ-040` s.d. `RWI-OQ-043` |
| 2026-08-21 | Requirement completeness gate | Gerbang kelengkapan requirement dijalankan dan menghasilkan [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) revision `1.0`. Modul dipecah menjadi 15 slice dan dinilai terhadap 18 dimensi kelengkapan, dibandingkan dengan baseline `indonesia-hospital-domain-reference` berkas `inpatient.md` yang berstatus `REFERENCE_ONLY`. Hasil keseluruhan `PARTIALLY_READY`: 8 slice siap dirancang, 2 sebagian, 5 berhenti. Tiga gap baru ditemukan yang tidak pernah muncul pada Scope Pass maupun Closure Pass — interoperabilitas SATUSEHAT beserta pemilik riwayat lokasi (`RWI-OQ-037`), serah terima klinis antar shift keperawatan (`RWI-OQ-038`), dan aturan klinis pasien meninggal serta kabur yang kini diberi ID tersendiri (`RWI-OQ-039`). Tujuh Decision ID `DEC-INP-001` s.d. `DEC-INP-007` dicatat sebagai pemblokir |
| 2026-08-21 | Closure pass revision 2, pertanyaan 7 s.d. 13 | Pemilik kebutuhan mendelegasikan sisa pertanyaan kepada rekomendasi agent (`RWI-DEC-044`), sejalan dengan `RWI-DEC-036` pada Scope Pass. Tujuh keputusan diambil sekaligus: resume pulang sebagai catatan resmi milik episode (`RWI-DEC-045`), penanda obat pulang disimpan di tabel resep milik Farmasi (`RWI-DEC-046`), perawat penanggung jawab ditugaskan per episode dengan riwayat (`RWI-DEC-047`), data master diisi lewat layar dan seeder hanya untuk pengembangan (`RWI-DEC-048`), cacat tombol bed diperbaiki di frontend dengan memanggil `/status` (`RWI-DEC-049`), seluruh angka admin disatukan dalam satu tabel pengaturan (`RWI-DEC-050`), dan test menjadi bagian pekerjaan Rawat Inap bukan pekerjaan terpisah (`RWI-DEC-051`). `RWI-RULE-032`, `RWI-RULE-033`, dan `RWI-RULE-034` ditulis, `RWI-RULE-024` dilengkapi. `RWI-TRQ-011` s.d. `RWI-TRQ-017` ditutup. 24 acceptance criteria baru ditulis. `RWI-OQ-036` ditambahkan. **Seluruh 17 pertanyaan penutup capability map tertutup** |
| 2026-08-21 | Closure pass revision 2, pertanyaan 6 | Pertanyaan 6 dijawab: Rawat Inap punya tabel riwayat perubahan status sendiri yang meniru bentuk `TrxWorkflowStatusHistory` tanpa menumpang padanya, ditulis satu transaksi dengan perubahan statusnya dan tidak dapat diubah (`RWI-DEC-043`). `RWI-RULE-031` ditulis, `RWI-TRQ-010` ditutup, tujuh acceptance criteria baru ditulis, `RWI-OQ-035` tentang masa simpan riwayat ditambahkan sebagai keputusan hukum yang menunggu pemilik privasi |
| 2026-08-21 | Closure pass revision 2, pertanyaan 5 | Pertanyaan 5 dijawab: episode punya catatan DPJP tersendiri berisi riwayat penugasan, dan permintaan perpindahan hanya diterima dari DPJP yang aktif saat itu (`RWI-DEC-042`). Penjaganya ditulis di dalam service Rawat Inap karena mesin hak akses hanya mengenal peran terhadap endpoint. `RWI-RULE-030` ditulis, `RWI-TRQ-009` ditutup, tujuh acceptance criteria baru ditulis, `RWI-RISK-004` dicatat |
| 2026-08-21 | Closure pass revision 2, pertanyaan 4 | Ditemukan tabrakan antara `RWI-RULE-026` aturan 6 (pelonggaran hanya untuk kunjungan bertipe rawat inap) dan `RWI-DEC-011` (kunjungan IGD dipakai apa adanya, bertipe `Emergency`): pasien jalur IGD justru tidak akan mendapat pelonggaran. Pertanyaan 4 dijawab: kunjungan IGD ditutup dan kunjungan baru bertipe rawat inap dibuat sebagai jangkar, keduanya dihubungkan sebagai satu rangkaian kedatangan (`RWI-DEC-041`). `RWI-RULE-029` ditulis; `RWI-DEC-011` dan tabel jalur masuk pada `RWI-RULE-005` ditandai `superseded` sebagian tanpa dihapus; `RWI-TRQ-007` dan `RWI-TRQ-008` ditutup; tujuh acceptance criteria baru ditulis; `RWI-OQ-034` ditambahkan sebagai blocker implementasi. **Keempat blocker desain dari capability map tertutup** |
| 2026-08-21 | Closure pass revision 2, pertanyaan 3 | Pertanyaan 3 dijawab: status kelayakan keuangan disimpan pada episode dan ditandai manual petugas kasir dengan pelaku, waktu, dan catatan wajib, sampai `BillingManagement` punya kemampuan transaksi (`RWI-DEC-040`). `RWI-DEC-015` dan `RWI-RULE-009` **tidak** dicabut; gerbang tetap memblokir dan jalan keluar supervisor tetap pengecualian. `RWI-RULE-028` ditulis, `RWI-TRQ-006` ditutup, enam acceptance criteria baru ditulis, `RWI-RISK-003` dicatat |
| 2026-08-21 | Closure pass revision 2, pertanyaan 2 | Pertanyaan 2 dijawab: catatan penempatan milik Rawat Inap menjadi sumber kebenaran penghunian tempat tidur, dan kolom `MstBed.BedStatus` turun kedudukan menjadi salinan yang ditulis dalam satu transaksi yang sama (`RWI-DEC-039`). Nilai `Reserved` dan `Occupied` dicabut dari wewenang admin master data, dan satu laporan selisih diwajibkan. `RWI-RULE-027` ditulis; `RWI-TRQ-004` dan `RWI-TRQ-005` ditutup; enam acceptance criteria baru ditulis; `RWI-OQ-033` ditambahkan sebagai blocker implementasi |
| 2026-08-21 | Closure pass revision 2, pertanyaan 1 | Ditemukan pembatas yang lebih keras daripada keharusan antrean: satu kunjungan hanya boleh punya satu konsultasi dokter, dan satu konsultasi hanya boleh punya satu resep aktif; penjaganya memeriksa `EncounterId` sehingga antrean semu tidak dapat melewatinya. Capability map dinaikkan ke revision `1.1`. `RWI-FACT-011` dan `RWI-FACT-012` ditulis. Pemilik kebutuhan memilih melonggarkan mesin klinis yang ada, bukan membuat entity tandingan dan bukan membuat antrean semu (`RWI-DEC-038`). `RWI-RULE-026` ditulis; `RWI-TRQ-001`, `RWI-TRQ-002`, dan `RWI-TRQ-003` ditutup; lima acceptance criteria baru ditulis; `RWI-OQ-032` dan `RWI-RISK-002` ditambahkan sebagai blocker implementasi |
| 2026-08-21 | Capability audit (`/qv-trace`) | Audit read-only backend `5afb54b` dan frontend `dec4fdeff` selesai; impact scan `45dcfa1` → `5afb54b` tidak menemukan perubahan source yang relevan. Sembilan butir `RWI-TRC-001` s.d. `RWI-TRC-009` terjawab. 44 kemampuan diklasifikasi: 10 `Ready to reuse`, 11 `Reuse with adapter`, 3 `Extend`, 18 `Missing`, 1 `Conflict`, 1 `Unknown`. Tiga hambatan besar dicatat: dokumentasi klinis terkunci pada antrean poliklinik, status tempat tidur tanpa catatan penghunian, dan kelayakan keuangan tanpa sumber data. Tiga konflik frontend–backend dikonfirmasi (`RWI-CON-TRC-001` s.d. `RWI-CON-TRC-003`). `RWI-DEC-001` ditutup. 17 pertanyaan penutup `RWI-TRQ-001` s.d. `RWI-TRQ-017` diteruskan ke `/grill-me`, empat di antaranya memblokir desain. Hasil ada di `01-existing-capability-map.md` revision `1.0` |
| 2026-08-20 | Scope pass revision 1 | Seed dari PRD Modul Rawat Inap. Batas scope disusun, 7 konflik dan 11 lubang cakupan dicatat, 9 butir diteruskan ke audit source, wawancara dimulai |
| 2026-08-20 | Scope pass revision 1, lanjutan (a) | Pertanyaan 4 dijawab: pemesanan tempat tidur berlaku 2 jam dengan satu angka seragam yang bisa diatur admin (`RWI-DEC-008`). Aturan `RWI-RULE-001` dan `RWI-RULE-002` ditulis, `OQ-RI-003` ditutup, `RWI-OQ-024` ditambahkan, daftar blocker dirapikan. Pertanyaan 5 diajukan untuk menutup `RWI-CON-001` |
| 2026-08-20 | Scope pass revision 1, lanjutan (b) | Pertanyaan 5 dijawab: model status episode `Draft` → `Admitted` → `DischargePending` → `Closed` plus `Cancelled`, dan `InCare` dibuang (`RWI-DEC-009`). `RWI-FACT-005` dan `RWI-RULE-003` ditulis, `RWI-CON-001` ditutup. Pertanyaan 6 diajukan untuk menutup `RWI-OQ-022` |
| 2026-08-20 | Scope pass revision 1, lanjutan (c) | Pertanyaan 6 dijawab: pembatalan admisi dibatasi sampai sebelum ada catatan klinis, dengan supervisor sebagai pemberi izin setelah `Admitted` (`RWI-DEC-010`). `RWI-RULE-004` ditulis, `RWI-OQ-022` dan `RWI-GAP-010` ditutup, tabel lubang cakupan diberi kolom status. Pertanyaan 7 diajukan untuk menutup `OQ-RI-001` |
| 2026-08-20 | Scope pass revision 1, lanjutan (d) | Pertanyaan 7 dijawab: setiap episode selalu menempel pada satu kunjungan, dan kunjungan rawat inap dibuat otomatis untuk pasien yang datang langsung (`RWI-DEC-011`). `RWI-RULE-005` ditulis, `OQ-RI-001` ditutup, `RWI-TRC-002` naik menjadi prasyarat audit. Pertanyaan 8 diajukan untuk menutup `RWI-CON-003`, `OQ-RI-004`, dan `OQ-RI-005` |
| 2026-08-20 | Scope pass revision 1, lanjutan (e) | Pertanyaan 8 dijawab: kewenangan transfer mengikuti tabel PRD apa adanya, perpindahan satu langkah tanpa penerimaan unit tujuan (`RWI-DEC-012`). `RWI-FACT-006` dan `RWI-RULE-006` ditulis; `RWI-CON-003`, `OQ-RI-004`, `OQ-RI-005` ditutup; risiko `RWI-RISK-001` dicatat; `RWI-OQ-025` dan `RWI-OQ-026` ditambahkan. Pertanyaan 9 diajukan untuk menutup `RWI-OQ-026` |
| 2026-08-20 | Scope pass revision 1, lanjutan (f) | Pertanyaan 9 dijawab: pindah kelas tidak dikecualikan dan kelas tagihan mengikuti kamar (`RWI-DEC-013`). `RWI-RULE-007` ditulis; `RWI-OQ-015`, `RWI-OQ-026`, dan `RWI-GAP-003` ditutup; `RWI-RISK-001` diterima secara sadar; `RWI-GAP-004` dinyatakan belum terjawab dan bergantung pada `RWI-OQ-016`. Pertanyaan 10 diajukan untuk menutup `RWI-CON-007` |
| 2026-08-20 | Scope pass revision 1, lanjutan (g) | Pertanyaan 10 dijawab: perpindahan bersifat utuh atau batal sama sekali, dan INV-02 berlaku setiap saat (`RWI-DEC-014`). `RWI-RULE-008` ditulis, `RWI-CON-007` ditutup, urutan EPIC RI-09 dinyatakan tidak berlaku. Pertanyaan 11 diajukan untuk menutup `RWI-CON-005` dan `OQ-RI-008` |
| 2026-08-20 | Scope pass revision 1, lanjutan (h) | Pertanyaan 11 dijawab: kelayakan keuangan memblokir penutupan, hanya `Cleared` yang lolos, dengan jalan keluar supervisor yang beralasan dan terekam (`RWI-DEC-015`). `RWI-FACT-007`, `RWI-FACT-008`, dan `RWI-RULE-009` ditulis; `RWI-CON-005` dan `OQ-RI-008` ditutup. Pertanyaan 12 diajukan untuk menutup `RWI-CON-004` dan `OQ-RI-006` |
| 2026-08-20 | Scope pass revision 1, lanjutan (i) | Pertanyaan 12 dijawab: keputusan pulang milik DPJP, penutupan episode dikerjakan petugas admisi atau supervisor dengan lima syarat yang diperiksa sistem (`RWI-DEC-016`). `RWI-RULE-010` ditulis; `RWI-CON-004` dan `OQ-RI-006` ditutup; `RWI-OQ-027` ditambahkan. **Seluruh tujuh konflik PRD tertutup.** Pertanyaan 13 diajukan untuk menutup `RWI-GAP-001` dan `RWI-GAP-002` |
| 2026-08-20 | Scope pass revision 1, penutup | Pemilik kebutuhan mendelegasikan sisa pertanyaan kepada rekomendasi agent (`RWI-DEC-036`). Pertanyaan 30 diputuskan lewat delegasi itu: wajib satu persetujuan umum rawat inap yang menahan penutupan, bukan admisi (`RWI-DEC-035`). `RWI-RULE-025` ditulis, `OQ-RI-009` dijawab, satu baris gerbang produksi baru ditambahkan. `RWI-OQ-023` **tidak** ditutup karena jawabannya adalah nama orang yang sungguh ada; dicatat sebagai `RWI-DEC-037`. Antrean pertanyaan wawancara habis |
| 2026-08-20 | Scope pass revision 1, lanjutan (z) | Pertanyaan 29 dijawab: obat pulang adalah jenis resep pada CAP-023 dan penyerahannya menjadi butir daftar periksa administrasi (`RWI-DEC-033`). `RWI-RULE-024` ditulis; `RWI-OQ-021` dan `RWI-GAP-009` ditutup; `OQ-RI-011` ditutup sebagai turunan `RWI-DEC-004` lewat `RWI-DEC-034`; dua acceptance criteria baru ditulis. Pertanyaan 30 diajukan untuk menutup `OQ-RI-009` |
| 2026-08-20 | Scope pass revision 1, lanjutan (y) | Pertanyaan 28 dijawab: tiap daftar pantau punya satu penanggung jawab dengan ambang yang diatur admin (`RWI-DEC-032`). `RWI-RULE-023` ditulis, `RWI-OQ-027` dan `RWI-OQ-031` ditutup, `RWI-FE-002` ditambahkan, tiga acceptance criteria baru ditulis. Pertanyaan 29 diajukan untuk menutup `RWI-GAP-009` |
| 2026-08-20 | Scope pass revision 1, lanjutan (x) | Pertanyaan 27 dijawab: visite dihitung satu per dokter per tanggal (`RWI-DEC-031`). `RWI-RULE-017` dilengkapi, `RWI-OQ-030` ditutup, dua acceptance criteria baru ditulis. Pertanyaan 28 diajukan untuk menutup `RWI-OQ-027` dan `RWI-OQ-031` |
| 2026-08-20 | Scope pass revision 1, lanjutan (w) | Pertanyaan 26 dijawab: episode `Draft` yang ditinggalkan 1 hari batal sendiri, dihitung saat dibaca, dan kunjungannya ikut dibatalkan (`RWI-DEC-030`). `RWI-RULE-022` ditulis, `RWI-OQ-028` ditutup, tiga acceptance criteria baru ditulis. Pertanyaan 27 diajukan untuk menutup `RWI-OQ-030` |
| 2026-08-20 | Scope pass revision 1, lanjutan (v) | Pertanyaan 25 dijawab: pengkajian awal dan verifikasi CPPT ditargetkan 24 jam dan hanya dipantau, tidak menahan apa pun (`RWI-DEC-029`). `RWI-RULE-021` ditulis dan `RWI-OQ-031` ditambahkan. Keputusan ini **tidak dinaikkan ke `approved`** karena aturan klinis dan akreditasi, dan satu baris gerbang produksi baru ditambahkan. Pertanyaan 26 diajukan untuk menutup `RWI-OQ-028` |
| 2026-08-20 | Scope pass revision 1, lanjutan (u) | Pertanyaan 24 dijawab: reopen hanya untuk memperbaiki catatan, oleh supervisor, tanpa mengembalikan tempat tidur dan tanpa menambah hari rawat (`RWI-DEC-028`). `RWI-RULE-020` ditulis, `OQ-RI-012` ditutup, tiga acceptance criteria baru ditulis. Pertanyaan 25 diajukan untuk menutup `RWI-OQ-018` |
| 2026-08-20 | Scope pass revision 1, lanjutan (t) | Pertanyaan 23 dijawab: lama dirawat dihitung dari selisih tanggal dengan minimum 1 hari (`RWI-DEC-027`). `RWI-RULE-019` ditulis, `RWI-OQ-020` dan `RWI-GAP-008` ditutup, bagian Frontend Decision Authority dibuka dengan `RWI-FE-001`, tiga acceptance criteria baru ditulis. Pertanyaan 24 diajukan untuk menutup `OQ-RI-012` |
| 2026-08-20 | Scope pass revision 1, lanjutan (s) | Pertanyaan 22 dijawab: clearance administrasi berbentuk daftar periksa yang diatur admin dan bersifat menahan (`RWI-DEC-026`). `RWI-RULE-018` ditulis, syarat ketiga `RWI-RULE-010` ditambal, `OQ-RI-007` ditutup, tiga acceptance criteria baru ditulis. Pertanyaan 23 diajukan untuk menutup `RWI-OQ-020` |
| 2026-08-20 | Scope pass revision 1, lanjutan (r) | Pertanyaan 21 dijawab: visite tercatat dari catatan perkembangan dokter, tanpa formulir tersendiri (`RWI-DEC-025`). `RWI-FACT-010` dan `RWI-RULE-017` ditulis; `RWI-OQ-019` dan `RWI-GAP-007` ditutup; `RWI-OQ-030` ditambahkan; tiga acceptance criteria baru ditulis. Pertanyaan 22 diajukan untuk menutup `OQ-RI-007` |
| 2026-08-20 | Scope pass revision 1, lanjutan (q) | Pertanyaan 20 dijawab: koordinasi antar DPJP tidak direkam sistem, dan dokter bukan DPJP selalu ditolak tanpa kolom pelolos (`RWI-DEC-024`). `RWI-OQ-029` ditutup dan tiga acceptance criteria baru ditulis. **Seluruh butir `DESIGN` tertutup kecuali penunjukan pemilik berwenang yang bersifat organisasi.** Pertanyaan 21 diajukan untuk menutup `RWI-OQ-019` |
| 2026-08-20 | Scope pass revision 1, lanjutan (p) | Pertanyaan 19 dijawab: "kesiapan unit tujuan" adalah pertimbangan profesional DPJP, bukan pemeriksaan sistem (`RWI-DEC-023`). `RWI-CON-008` ditutup, `RWI-DEC-012` tetap berlaku utuh tanpa bagian yang `superseded`, dan dua acceptance criteria baru ditulis. Pertanyaan 20 diajukan untuk menutup `RWI-OQ-029` |
| 2026-08-20 | Scope pass revision 1, lanjutan (o) | Pertanyaan 18 dijawab langsung oleh pemilik kebutuhan di luar pilihan yang ditawarkan: DPJP dapat menginisiasi dan menyetujui transfer pasien dalam tanggung jawab klinisnya, sedangkan pasien DPJP lain menuntut koordinasi atau pengalihan DPJP terdokumentasi (`RWI-DEC-022`). `RWI-RULE-016` ditulis dan `RWI-OQ-025` ditutup. Ditemukan konflik baru `RWI-CON-008` antara `RWI-DEC-022` dan `RWI-DEC-012`, serta pertanyaan baru `RWI-OQ-029`. Pertanyaan 19 diajukan untuk menutup `RWI-CON-008` |
| 2026-08-20 | Scope pass revision 1, lanjutan (n) | Pertanyaan 17 dijawab: keadaan tempat tidur diperiksa ulang saat admisi diaktifkan, dan isian admisi tidak hilang saat ditolak (`RWI-DEC-021`). `RWI-RULE-015` ditulis, `RWI-OQ-024` ditutup, `RWI-OQ-028` ditambahkan, lima acceptance criteria baru ditulis. Pertanyaan 18 diajukan untuk menutup `RWI-OQ-025` |
| 2026-08-20 | Scope pass revision 1, lanjutan (m) | Pertanyaan 16 dijawab: bayi baru lahir punya episode dan kunjungan sendiri dengan boks sebagai tempat tidur, ICU tidak dibedakan (`RWI-DEC-020`). `RWI-RULE-014` ditulis; `OQ-RI-010` dan `RWI-GAP-011` ditutup; `RWI-TRC-006` diperluas mencakup boks bayi. Pertanyaan 17 diajukan untuk menutup `RWI-OQ-024` |
| 2026-08-20 | Scope pass revision 1, lanjutan (l) | Pertanyaan 15 dijawab: pasien titipan tidak masuk MVP dan kelas tagihan tetap mengikuti kamar (`RWI-DEC-019`). `RWI-RULE-013` ditulis; `RWI-OQ-016` dan `RWI-GAP-004` ditutup sebagai di luar MVP; daftar Di luar scope bertambah satu baris. Pertanyaan 16 diajukan untuk menutup `OQ-RI-010` dan `RWI-GAP-011` |
| 2026-08-20 | Scope pass revision 1, lanjutan (k) | Pertanyaan 14 dijawab: pemisahan jenis kelamin dan isolasi tetap berupa penyaring pencarian, bukan aturan penolakan (`RWI-DEC-018`). `RWI-FACT-009` dan `RWI-RULE-012` ditulis. Keputusan ini **tidak dinaikkan ke `approved`** karena berada di area klinis dan privasi yang dikecualikan `RWI-DEC-006`, dan dua baris gerbang produksi baru ditambahkan. Pertanyaan 15 diajukan untuk menutup `RWI-OQ-016` |
| 2026-08-20 | Scope pass revision 1, lanjutan (j) | Pertanyaan 13 dijawab: diakui lima cara pulang dengan syarat berbeda per cara (`RWI-DEC-017`). `RWI-RULE-011` ditulis; `RWI-OQ-013`, `RWI-OQ-014`, `RWI-GAP-001`, dan `RWI-GAP-002` ditutup; sisi klinis untuk pasien meninggal dan kabur ditandai tetap terbuka |
