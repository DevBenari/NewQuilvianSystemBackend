# Rawat Inap — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RWI-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Interview mode | `Scope pass` — 30 pertanyaan, antrean pertanyaan habis per 2026-08-20 |
| Product/domain owner | Pemilik suite skill Quilvian sebagai **pemegang sementara** sesuai `RWI-DEC-006`; nama formal perlu diisi |
| Clinical governance owner | `OPEN` — menjadi syarat sebelum produksi |
| Security/privacy owner | `OPEN` — menjadi syarat sebelum produksi |
| Backend SHA | `45dcfa1` (branch `MHamzah`) |
| Frontend SHA | `dec4fdeff` |
| Capability map | **Belum ada.** Scope dikunci tanpa audit kemampuan existing, sehingga kemungkinan duplikasi dengan modul lain belum diperiksa |
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
| Aturan bisnis tertulis | 25 | `RWI-RULE-001` sampai `RWI-RULE-025`, seluruhnya disertai contoh berangka |
| — di antaranya **belum final** | 3 | `RWI-RULE-012`, `RWI-RULE-021`, dan `RWI-RULE-025`. Ketiganya menunggu pemilik klinis atau pemilik privasi |
| Keputusan tercatat | 37 | 30 berstatus `approved`, 7 berstatus `draft` |
| Fakta yang terbukti dari repository dan PRD | 10 | `RWI-FACT-001` sampai `RWI-FACT-010` |
| Acceptance criteria yang sudah dapat diuji | 53 | `RWI-AC-001` sampai `RWI-AC-053` |
| Keputusan yang didelegasikan ke pelaksana | 2 | `RWI-FE-001` dan `RWI-FE-002`, keduanya `DEV_DISCRETION` |
| Konflik | **8 dari 8 tertutup** | Tujuh berasal dari PRD, satu ditemukan antar keputusan di dokumen ini sendiri |
| Lubang cakupan | 9 dari 11 tertutup | Dua sisanya sudah dijawab tetapi menunggu pemilik klinis |
| Pertanyaan wawancara tersisa | **0** | Antrean habis. Yang tersisa bukan pertanyaan, melainkan tindakan organisasi |
| Gerbang sebelum produksi | 3 gerbang tata kelola + 4 baris aturan | Lihat bagian Gate Sebelum Produksi |

**Satu-satunya butir terbuka** adalah `RWI-OQ-023` / `RWI-DEC-037`: siapa nama orang atau
komite yang berwenang menyetujui modul ini. Butir itu tidak dapat diselesaikan lewat wawancara
maupun lewat delegasi, karena jawabannya adalah nama orang yang sungguh ada.

**Yang sudah boleh dikerjakan:** audit kemampuan existing lewat `/qv-trace`, memakai sembilan
butir `RWI-TRC-001` sampai `RWI-TRC-009` sebagai daftar periksanya.

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


### `RWI-RULE-012` — Pemisahan jenis kelamin dan isolasi — **BELUM FINAL**

Dasar keputusan: `RWI-DEC-018`. Menjawab `RWI-OQ-017` dan `RWI-GAP-005`, tetapi **belum**
berstatus `approved`. Baca peringatan di akhir bagian ini sebelum memakainya.

Perilaku yang dipilih pemilik kebutuhan: mengikuti FR-RI-010 dan FR-RI-011 apa adanya.

| Butir | Ketentuan |
|---|---|
| Jenis kelamin | **Bukan** aturan penolakan. Petugas boleh memasang penyaring jenis kelamin saat mencari tempat tidur, tetapi sistem tidak menolak penempatan yang mencampur laki-laki dan perempuan dalam satu kamar |
| Isolasi | Sama. Penanda isolasi hanya dipakai sebagai penyaring pencarian, bukan sebagai syarat penempatan |
| Yang benar-benar menolak penempatan | Hanya FR-RI-011, yaitu `IsActive = true` dan status ketersediaan tempat tidur |
| Siapa yang bertanggung jawab atas kecocokan | Petugas yang menempatkan, bukan sistem |

Contoh nyata:

> Pukul **23:40** IGD mengirim Ibu Sari untuk dirawat inap. Semua kamar perempuan penuh.
> Petugas admisi mencari tempat tidur **tanpa** memasang penyaring jenis kelamin, menemukan
> bed `MELATI-05` kosong di kamar yang sudah berisi dua pasien laki-laki, lalu menempatkan
> Ibu Sari di sana. Sistem menerimanya tanpa penolakan dan tanpa peringatan.

Akibat yang tercatat:

1. Sistem tidak mencegah pencampuran jenis kelamin dalam satu kamar.
2. Sistem tidak mencegah pasien yang membutuhkan isolasi ditempatkan di kamar biasa yang sudah
   berisi pasien lain. Kesalahan jenis kelamin masih bisa diperbaiki dengan memindahkan
   pasien; penularan yang sudah terjadi tidak bisa ditarik kembali.
3. Karena aturan ini tidak melarang apa pun, **tidak ada acceptance criteria** yang bisa
   ditulis untuk mengujinya. Itu sebabnya bagian Acceptance Criteria tidak memuat baris untuk
   `RWI-RULE-012`.
4. Sebagai efek samping, bayi baru lahir yang dirawat gabung dengan ibunya (`RWI-GAP-011`)
   tidak akan terbentur aturan jenis kelamin.

> **PERINGATAN — aturan ini belum boleh dipakai melayani pasien sungguhan.** Isi keputusan ini
> berada di dua area yang secara tegas dikecualikan oleh `RWI-DEC-006`, yaitu keputusan klinis
> (pengendalian infeksi) dan keputusan privasi (pencampuran jenis kelamin). Pemegang sementara
> **tidak berwenang** menutupnya. Status keputusan tetap `draft` sampai pemilik klinis dan
> pemilik keamanan/privasi ditunjuk dan meninjau baris ini. Lihat Gate Sebelum Produksi.


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


---

## Frontend Decision Authority

Baris di sini adalah keputusan yang **sengaja didelegasikan** kepada pelaksana. Agent tidak
menetapkan menu, route, tab, modal, warna, maupun tata letak berdasarkan seleranya sendiri.

| Decision ID | Area | Owner | Status | Batas yang diizinkan | Dasar |
|---|---|---|---|---|---|
| `RWI-FE-001` | Kata yang dipakai untuk menamai angka hari rawat pada census | Pelaksana frontend | `DEV_DISCRETION` | Wajib menyebut dengan jelas bahwa angka itu hitungan hari rawat, bukan lama waktu sebenarnya. Bentuk kalimat, singkatan, penempatan, dan gaya tampilan bebas | `RWI-RULE-019` |
| `RWI-FE-002` | Bentuk tampilan tiga daftar pantau | Pelaksana frontend | `DEV_DISCRETION` | Boleh satu halaman gabungan atau tiga halaman terpisah. Urutan kolom, cara menandai keterlambatan, dan penempatan menu bebas. Yang wajib: lama keterlambatan terbaca, dan daftar tidak boleh menghalangi tindakan apa pun | `RWI-RULE-023` |

---

## Decision Log

| Decision ID | Type | Keputusan atau pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RWI-DEC-001` | Fact | Modul ini dikerjakan sebagai Scope Pass tanpa capability map. Risiko duplikasi dengan modul existing belum diperiksa | Agent | `draft` | — | Tidak ada `01-existing-capability-map.md` |
| `RWI-DEC-002` | Fact | Prefix entity operasional modul ini adalah `Inp`, bukan `Inpatient` | Backend governance | `draft` | — | `RWI-FACT-003` |
| `RWI-DEC-003` | Fact | Status registry `PLANNED` berarti belum ada izin implementasi, migration, atau database | Backend governance | `draft` | — | `RWI-FACT-002` |
| `RWI-DEC-004` | Decision | Batas scope MVP dikunci pada 18 kemampuan MUST milik PRD. Daftar Di dalam scope dan Di luar scope disetujui apa adanya | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — dinaikkan menyusul `RWI-DEC-006` | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-005` | Decision | Sebelas lubang cakupan diselesaikan sebagai aturan di dalam kemampuan yang sudah masuk scope, bukan sebagai item MUST baru. Jumlah MUST tetap 18 | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — dinaikkan menyusul `RWI-DEC-006` | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-006` | Decision | Pemilik suite skill Quilvian ditetapkan sebagai Product/Domain Owner sementara. Keputusan produk dan alur kerja boleh naik ke `approved`. Keputusan klinis dan keputusan keamanan/privasi tetap ditandai terbuka dan menjadi syarat sebelum produksi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — **nama orang belum diisi** | Wawancara pertanyaan 2, 2026-08-20 |
| `RWI-DEC-007` | Decision | Tempat tidur memakai status `Reserved` sebelum `Occupied`. Pemesanan gugur sendiri setelah lewat batas waktu, dan kedaluwarsa dihitung saat data dibaca sehingga tidak memerlukan program penjadwal | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 3; menutup OQ-RI-002 dan `RWI-CON-002` |
| `RWI-DEC-008` | Decision | Pemesanan tempat tidur berlaku 2 jam sejak dibuat. Satu angka yang sama berlaku untuk semua unit dan semua asal pemesanan, dan angka itu disimpan sebagai parameter yang boleh diubah admin tanpa mengubah program | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 4; menutup OQ-RI-003; dirinci pada `RWI-RULE-002` |
| `RWI-DEC-009` | Decision | Model status episode adalah `Draft` → `Admitted` → `DischargePending` → `Closed`, ditambah `Cancelled`. `InCare` dibuang karena tidak punya definisi maupun pemicu di PRD, dan informasinya sudah tersimpan pada catatan pengkajian serta catatan visite. Status episode tidak dipakai sebagai syarat sebelum dokumentasi klinis boleh ditulis | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 5; menutup `RWI-CON-001`; dasar `RWI-FACT-005`; dirinci pada `RWI-RULE-003` |
| `RWI-DEC-010` | Decision | Pembatalan boleh oleh petugas admisi selagi episode `Draft`, dan oleh supervisor atau kepala ruangan selagi `Admitted` **selama episode belum punya satu pun catatan klinis**. Setelah ada catatan klinis, pembatalan tertutup. Alasan wajib diisi, dan pelepasan tempat tidur menjadi satu kesatuan dengan pembatalan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 6; menutup `RWI-OQ-022` dan `RWI-GAP-010`; dirinci pada `RWI-RULE-004` |
| `RWI-DEC-011` | Decision | Setiap episode rawat inap selalu menempel pada tepat satu kunjungan. Kunjungan IGD atau poliklinik yang sudah ada dipakai apa adanya; untuk pasien yang datang langsung, sistem membuat kunjungan bertipe rawat inap secara otomatis di dalam proses admisi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 7; menutup `OQ-RI-001`; dirinci pada `RWI-RULE-005`; bergantung pada `RWI-TRC-002` |
| `RWI-DEC-012` | Decision | Kewenangan transfer mengikuti tabel PRD bagian 14 apa adanya: Kepala Perawat, Perawat pelaksana, dan Supervisor boleh memindahkan pasien. Perpindahan berjalan satu langkah tanpa penerimaan unit tujuan. Risiko pindah kelas tanpa persetujuan diterima secara sadar, tercatat sebagai `RWI-RISK-001` | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 8; menutup `RWI-CON-003`, `OQ-RI-004`, dan `OQ-RI-005`; dirinci pada `RWI-RULE-006` |
| `RWI-DEC-013` | Decision | Pindah kelas tidak dikecualikan. Kewenangannya sama dengan pindah tempat tidur biasa, dan kelas yang ditagihkan selalu mengikuti kamar yang ditempati. Perubahannya disimpan sebagai riwayat. `RWI-RISK-001` diterima secara sadar, dan `RWI-GAP-004` pasien titipan dinyatakan belum terjawab | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 9; menutup `RWI-OQ-026` dan `RWI-OQ-015`; dirinci pada `RWI-RULE-007` |
| `RWI-DEC-014` | Decision | Perpindahan adalah satu tindakan utuh: berhasil seluruhnya atau tidak ada yang berubah sama sekali. Pasien tidak pernah tercatat tanpa tempat tidur, sehingga INV-02 berlaku setiap saat. Urutan EPIC RI-09 dinyatakan tidak berlaku | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 10; menutup `RWI-CON-007`; dirinci pada `RWI-RULE-008` |
| `RWI-DEC-015` | Decision | Kelayakan keuangan **memblokir** penutupan episode. Hanya `Cleared` yang membuka penutupan; `Pending`, `Blocked`, dan status yang belum ada sama-sama menahan. Supervisor boleh menutup dengan alasan wajib, dan episode itu ditandai serta masuk laporan tersendiri. Rumusan "tersedia" pada EPIC RI-10 dinyatakan tidak berlaku | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 11; menutup `RWI-CON-005` dan `OQ-RI-008`; dasar `RWI-FACT-007` dan `RWI-FACT-008`; dirinci pada `RWI-RULE-009` |
| `RWI-DEC-016` | Decision | Keputusan pulang tetap milik DPJP sendiri. Penutupan episode dikerjakan petugas admisi atau Supervisor, dan hanya bisa berjalan bila kelima syarat penutupan terpenuhi. Frasa "sesuai SOP" pada baris `Close episode` diganti aturan tegas ini | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 12; menutup `RWI-CON-004` dan `OQ-RI-006`; dirinci pada `RWI-RULE-010` |
| `RWI-DEC-018` | Decision | Pemisahan jenis kelamin dan isolasi **tetap berupa penyaring pencarian**, bukan aturan yang menolak penempatan. Sistem mengizinkan pasien laki-laki dan perempuan sekamar, dan mengizinkan pasien yang butuh isolasi ditempatkan di kamar biasa | Product/domain owner sementara | `draft` — **tidak dapat naik ke `approved`** | Belum di-approve. Berada di area klinis dan privasi yang dikecualikan `RWI-DEC-006` | Wawancara pertanyaan 14; menjawab `RWI-OQ-017` dan `RWI-GAP-005`; dasar `RWI-FACT-009`; dirinci pada `RWI-RULE-012` |
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
| `RWI-DEC-017` | Decision | Diakui lima cara pulang: atas izin DPJP, atas permintaan sendiri, dirujuk, meninggal, dan kabur. Syarat penutupan menyesuaikan cara pulangnya, dan kelimanya sama-sama melepas tempat tidur. Baris meninggal dan kabur tetap **terbuka secara klinis** sesuai `RWI-DEC-006` | Product/domain owner sementara | `approved` untuk keputusan produk; **terbuka** untuk sisi klinis | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 13; menutup `RWI-OQ-013`, `RWI-OQ-014`, `RWI-GAP-001`, dan `RWI-GAP-002`; dirinci pada `RWI-RULE-011` |

---

## Gate Sebelum Produksi

Persetujuan pemegang sementara **tidak** menutup gerbang berikut. Seluruhnya harus terpenuhi
sebelum modul ini boleh dipakai melayani pasien sungguhan. Tiga baris pertama dan terakhir
adalah gerbang tata kelola; empat baris `RWI-RULE-*` adalah aturan yang isinya sudah dipilih
pemegang sementara tetapi berada di area klinis atau privasi yang dikecualikan `RWI-DEC-006`.

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk. Semua aturan klinis pada dokumen ini memakai praktik umum dan regulasi sebagai dasar, bukan persetujuan komite klinis. Termasuk batas waktu pengkajian awal, verifikasi CPPT oleh DPJP, aturan pasien meninggal, dan syarat pasien boleh pulang |
| `RWI-RULE-012` — isolasi | **Gerbang keras.** `RWI-DEC-018` memilih isolasi tetap berupa penyaring pencarian, sehingga sistem mengizinkan pasien yang butuh isolasi ditempatkan di kamar biasa berisi pasien lain. Ini keputusan pengendalian infeksi dan wajib ditinjau pemilik klinis sebelum modul dipakai melayani pasien sungguhan |
| `RWI-RULE-012` — jenis kelamin | **Gerbang keras.** `RWI-DEC-018` juga membuat sistem mengizinkan pasien laki-laki dan perempuan sekamar. Ini keputusan privasi dan wajib ditinjau pemilik keamanan/privasi |
| `RWI-RULE-025` — persetujuan umum | **Gerbang keras.** `RWI-DEC-035` mewajibkan satu persetujuan umum tetapi tidak menahan admisi, sehingga ada jeda ketika pasien dirawat tanpa persetujuan tertulis. Ini keputusan privasi dan hukum, wajib ditinjau pemilik keamanan/privasi sebelum modul dipakai melayani pasien sungguhan |
| `RWI-RULE-021` — batas waktu klinis | **Gerbang keras.** `RWI-DEC-029` menetapkan target 24 jam untuk pengkajian awal dan verifikasi CPPT, dan angka itu diambil dari praktik akreditasi yang lazim, bukan dari persetujuan komite klinis. Wajib ditinjau pemilik klinis sebelum modul dipakai melayani pasien sungguhan |
| Security/privacy owner | Belum ditunjuk. Hak akses ke rekam medis rawat inap, penelusuran audit, dan aturan koreksi data harus disetujui pemiliknya |
| Registry lifecycle | Modul `InPatientManagement` masih `PLANNED`. Sesuai `RWI-FACT-002`, status ini hanya memberi hak penamaan, belum memberi izin implementasi, migration, atau database |

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

### Blocker desain saat ini

1. **Wawancara sudah tidak punya pertanyaan tersisa.** Satu-satunya butir yang masih terbuka
   adalah `RWI-OQ-023`, yaitu penunjukan nama orang atau komite yang berwenang menyetujui
   modul ini. Butir itu adalah **tindakan organisasi**, bukan keputusan desain, dan tidak dapat
   diselesaikan lewat wawancara maupun lewat delegasi `RWI-DEC-036`. Lihat `RWI-DEC-037`.
2. `RWI-DEC-018` sudah dijawab tetapi **tidak dapat naik ke `approved`** karena berada di area
   klinis dan privasi. Desain penempatan tempat tidur boleh dilanjutkan, tetapi modul tidak
   boleh dipakai melayani pasien sungguhan sebelum kedua pemilik itu meninjau `RWI-RULE-012`.
2. Pemilik klinis dan pemilik keamanan/privasi belum ditunjuk. Keputusan yang menyangkut
   keselamatan pasien dan hak akses rekam medis tidak boleh naik ke `approved` oleh pemegang
   sementara.
3. Modul berstatus `PLANNED` pada registry, sehingga belum ada izin implementasi.
4. Belum ada capability map, sehingga kemungkinan tumpang tindih dengan modul yang sudah ada
   belum diperiksa. Ini yang dijawab `/qv-trace`, bukan wawancara.

Hal berikut **sudah bukan** blocker:

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
