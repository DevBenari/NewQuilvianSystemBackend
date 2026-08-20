# Rawat Inap — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RWI-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Interview mode | `Scope pass` |
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
| `RWI-FACT-006` | Baris `Transfer` pada tabel kewenangan PRD bagian 14 berbunyi: Admisi kosong, Kepala Perawat centang, Perawat centang, Dokter/DPJP ditulis "sesuai SOP", Billing kosong, Supervisor centang. Perawat pelaksana mendapat centang penuh, sedangkan Dokter/DPJP tidak | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 767 |
| `RWI-FACT-005` | Kata `InCare` hanya muncul dua kali di seluruh PRD, yaitu baris 613 dan baris 1088. Keduanya hanya berupa kotak pada diagram alur. Tidak ada satu pun Functional Requirement, definisi, pemicu perpindahan, maupun baris kewenangan yang menyebut `InCare` | Pencarian kata pada `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` |

---

## Konflik yang Ditemukan pada PRD

Konflik berikut ditemukan agent saat membaca PRD. Konflik ini **tidak boleh diputus sendiri
oleh agent**; semuanya masuk antrean wawancara atau antrean audit source.

| ID | Konflik | Letak | Status |
|---|---|---|---|
| `RWI-CON-001` | Model status episode tidak konsisten. Bagian 10 menulis `Draft → Admitted → InCare → DischargePending → Closed`, sedangkan bagian 24 Contract A menulis `Admission → InCare → DischargePending → Closed` tanpa `Draft` dan dengan nama berbeda | PRD baris 608-618 vs 1086-1091 | `TERTUTUP` oleh `RWI-DEC-009` |
| `RWI-CON-002` | Status tempat tidur tidak konsisten. Bagian 11 mewajibkan `Reserved` sebelum `Occupied`, tetapi OQ-RI-002 justru masih menanyakan apakah `Reserved` diperlukan. FR-RI-013 dan FR-RI-014 hanya menyebut `Occupied` dan `Available` | PRD baris 634-640, 1052, 311-318 | `TERTUTUP` oleh `RWI-DEC-007` |
| `RWI-CON-003` | Kewenangan transfer bertabrakan. Tabel kewenangan bagian 14 sudah memberi tanda centang kepada Kepala Perawat dan Perawat, tetapi OQ-RI-004 masih menanyakan siapa yang berwenang final | PRD baris 767 vs 1054 | `TERTUTUP` oleh `RWI-DEC-012` |
| `RWI-CON-004` | Kewenangan penutupan episode bertabrakan. Tabel bagian 14 menulis "sesuai SOP" untuk Admisi dan Dokter serta centang untuk Supervisor, tetapi OQ-RI-006 masih menanyakan siapa yang mengeksekusi penutupan | PRD baris 770 vs 1056 | `TERBUKA` |
| `RWI-CON-005` | Gerbang keuangan ambigu. EPIC RI-10 hanya menuntut "status financial clearance tersedia", yang secara harfiah tetap lolos walaupun statusnya `Blocked`. OQ-RI-008 masih menanyakan apakah gerbang ini memblokir atau sekadar peringatan | PRD baris 570-571 vs 1058 | `TERBUKA` |
| `RWI-CON-006` | Nama entity usulan PRD (`InpatientEpisode`, `InpatientBedAssignment`, dan seterusnya) tidak mengikuti aturan penamaan registry yang mewajibkan prefix `Inp`. PRD sendiri menyatakan nama final mengikuti governance backend, jadi registry yang menang | PRD baris 691-700 vs `RWI-FACT-003` | `TERTUTUP` oleh `RWI-DEC-002` |
| `RWI-CON-007` | Urutan transfer atomik pada EPIC RI-09 menutup penempatan bed lama sebelum menempati bed baru, sedangkan invariant INV-02 mensyaratkan satu episode selalu punya satu bed aktif. Titik tengah transaksi berpotensi melanggar invariant bila dibaca harfiah | PRD baris 512-530 vs 656 | `TERBUKA` |

---

## Lubang Cakupan yang Ditemukan Agent

Hal berikut tidak dibahas PRD sama sekali, padahal berada di dalam batas scope yang diusulkan
dan berpotensi memblokir desain.

| ID | Lubang | Kenapa penting | Status |
|---|---|---|---|
| `RWI-GAP-001` | PRD hanya mengenal satu cara pulang, yaitu pasien diizinkan pulang oleh DPJP. Tidak ada pulang atas permintaan sendiri, dirujuk ke rumah sakit lain, pasien meninggal, dan pasien kabur | Keempat kondisi itu tetap harus melepas tempat tidur dan menutup episode, tetapi gerbangnya berbeda. Tanpa ini petugas akan mencari jalan pintas | `TERBUKA` |
| `RWI-GAP-002` | Tidak ada aturan pasien meninggal: siapa yang mencatat, apakah resume pulang tetap wajib, dan kapan bed dilepas | Menyangkut rekam medis dan pelaporan wajib | `TERBUKA` |
| `RWI-GAP-003` | Tidak ada aturan pindah kelas perawatan (naik kelas atau turun kelas), padahal transfer hanya digambarkan sebagai pindah bed | Pindah kelas punya akibat biaya; datanya harus tercatat sebagai perubahan kelas, bukan sekadar pindah bed | `TERTUTUP` oleh `RWI-DEC-013` |
| `RWI-GAP-004` | Tidak ada aturan pasien titipan, yaitu pasien yang dirawat di kelas atau ruang yang bukan haknya karena kamar penuh | Sangat lazim di rumah sakit Indonesia dan memengaruhi census, kelas, dan biaya | `TERBUKA` |
| `RWI-GAP-005` | Aturan pemisahan jenis kelamin dan isolasi hanya ditulis sebagai penyaring pencarian yang opsional ("gender compatibility jika digunakan"), bukan sebagai aturan keras | Bila hanya penyaring, sistem tetap mengizinkan laki-laki dan perempuan satu kamar | `TERBUKA` |
| `RWI-GAP-006` | Tidak ada batas waktu pengkajian awal keperawatan dan tidak ada aturan verifikasi CPPT oleh DPJP | Keduanya kewajiban akreditasi dan biasanya diaudit | `TERBUKA` |
| `RWI-GAP-007` | "Visite dokter" masuk daftar MUST (CAP-025) tetapi tidak punya satu pun Functional Requirement yang mendefinisikannya | Tidak jelas apa yang dianggap satu visite dan siapa yang mencatatnya | `TERBUKA` |
| `RWI-GAP-008` | Cara menghitung lama dirawat (LOS) hanya disebut "berdasarkan admission time" | Hitungan hari rawat berbeda antara selisih jam dan hitungan hari kalender; angkanya dipakai pihak lain | `TERBUKA` |
| `RWI-GAP-009` | Tidak ada aturan obat pulang | Resep obat pulang berbeda perlakuan dari resep harian dan biasanya menjadi bagian gerbang pulang | `TERBUKA` |
| `RWI-GAP-010` | Pembatalan admisi (`Draft/Admitted → Cancelled`) disebut ada, tetapi tidak ada aturan siapa yang boleh membatalkan, apakah alasan wajib, dan apa yang terjadi pada bed | Pembatalan yang tidak melepas bed membuat kamar terlihat penuh padahal kosong | `TERTUTUP` oleh `RWI-DEC-010` |
| `RWI-GAP-011` | Tidak ada aturan bayi baru lahir yang dirawat gabung dengan ibunya, walaupun OQ-RI-010 menyinggungnya | Bayi biasanya perlu episode sendiri tetapi menempati boks di kamar ibu | `TERBUKA` |

---

## Pertanyaan yang Harus Dijawab Source Code, Bukan Manusia

Butir berikut sengaja **tidak** ditanyakan kepada pemilik kebutuhan. Semuanya diteruskan ke
`/qv-trace` (`/trace-existing-capabilities`).

| ID | Yang harus dibuktikan dari source |
|---|---|
| `RWI-TRC-001` | Apakah `MstBed` benar sudah punya `BedStatus`, `IsReservable`, penyaring room/service unit/patient class, dan ringkasan Available/Occupied seperti klaim EPIC RI-02 |
| `RWI-TRC-002` | Apakah `PatientEncounterController` benar memaksa kelas pasien `"RAWAT JALAN"` seperti klaim PRD bagian 2. **Naik menjadi prasyarat** sejak `RWI-DEC-011`, karena jalur pasien datang langsung menuntut pembuatan kunjungan bertipe rawat inap |
| `RWI-TRC-003` | Bentuk nyata `TrxPatientEncounter`: status, relasi lokasi, dan apakah sudah menyimpan riwayat lokasi |
| `RWI-TRC-004` | Apakah `PatientAssessment`, `PatientVitalSign`, `PatientDiagnosis`, `PatientProcedure`, CPPT, `PatientConsent`, dan `Prescription` benar sudah terhubung ke `EncounterId` |
| `RWI-TRC-005` | Apakah `BillingManagement` benar baru punya `MasterData` saja |
| `RWI-TRC-006` | Apakah master bed, room, service unit, dan kelas pasien sudah terisi data, karena Definition of Done melarang manipulasi database manual |
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


---

## Decision Log

| Decision ID | Type | Keputusan atau pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RWI-DEC-001` | Fact | Modul ini dikerjakan sebagai Scope Pass tanpa capability map. Risiko duplikasi dengan modul existing belum diperiksa | Agent | `draft` | — | Tidak ada `01-existing-capability-map.md` |
| `RWI-DEC-002` | Fact | Prefix entity operasional modul ini adalah `Inp`, bukan `Inpatient` | Backend governance | `draft` | — | `RWI-FACT-003` |
| `RWI-DEC-003` | Fact | Status registry `PLANNED` berarti belum ada izin implementasi, migration, atau database | Backend governance | `draft` | — | `RWI-FACT-002` |
| `RWI-DEC-004` | Decision | Batas scope MVP dikunci pada 18 kemampuan MUST milik PRD. Daftar Di dalam scope dan Di luar scope disetujui apa adanya | Product/domain owner | `draft` | Belum di-approve; owner berwenang belum ditetapkan | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-005` | Decision | Sebelas lubang cakupan diselesaikan sebagai aturan di dalam kemampuan yang sudah masuk scope, bukan sebagai item MUST baru. Jumlah MUST tetap 18 | Product/domain owner | `draft` | Belum di-approve; owner berwenang belum ditetapkan | Wawancara pertanyaan 1, 2026-08-20 |
| `RWI-DEC-006` | Decision | Pemilik suite skill Quilvian ditetapkan sebagai Product/Domain Owner sementara. Keputusan produk dan alur kerja boleh naik ke `approved`. Keputusan klinis dan keputusan keamanan/privasi tetap ditandai terbuka dan menjadi syarat sebelum produksi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 — **nama orang belum diisi** | Wawancara pertanyaan 2, 2026-08-20 |
| `RWI-DEC-007` | Decision | Tempat tidur memakai status `Reserved` sebelum `Occupied`. Pemesanan gugur sendiri setelah lewat batas waktu, dan kedaluwarsa dihitung saat data dibaca sehingga tidak memerlukan program penjadwal | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 3; menutup OQ-RI-002 dan `RWI-CON-002` |
| `RWI-DEC-008` | Decision | Pemesanan tempat tidur berlaku 2 jam sejak dibuat. Satu angka yang sama berlaku untuk semua unit dan semua asal pemesanan, dan angka itu disimpan sebagai parameter yang boleh diubah admin tanpa mengubah program | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 4; menutup OQ-RI-003; dirinci pada `RWI-RULE-002` |
| `RWI-DEC-009` | Decision | Model status episode adalah `Draft` → `Admitted` → `DischargePending` → `Closed`, ditambah `Cancelled`. `InCare` dibuang karena tidak punya definisi maupun pemicu di PRD, dan informasinya sudah tersimpan pada catatan pengkajian serta catatan visite. Status episode tidak dipakai sebagai syarat sebelum dokumentasi klinis boleh ditulis | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 5; menutup `RWI-CON-001`; dasar `RWI-FACT-005`; dirinci pada `RWI-RULE-003` |
| `RWI-DEC-010` | Decision | Pembatalan boleh oleh petugas admisi selagi episode `Draft`, dan oleh supervisor atau kepala ruangan selagi `Admitted` **selama episode belum punya satu pun catatan klinis**. Setelah ada catatan klinis, pembatalan tertutup. Alasan wajib diisi, dan pelepasan tempat tidur menjadi satu kesatuan dengan pembatalan | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 6; menutup `RWI-OQ-022` dan `RWI-GAP-010`; dirinci pada `RWI-RULE-004` |
| `RWI-DEC-011` | Decision | Setiap episode rawat inap selalu menempel pada tepat satu kunjungan. Kunjungan IGD atau poliklinik yang sudah ada dipakai apa adanya; untuk pasien yang datang langsung, sistem membuat kunjungan bertipe rawat inap secara otomatis di dalam proses admisi | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 7; menutup `OQ-RI-001`; dirinci pada `RWI-RULE-005`; bergantung pada `RWI-TRC-002` |
| `RWI-DEC-012` | Decision | Kewenangan transfer mengikuti tabel PRD bagian 14 apa adanya: Kepala Perawat, Perawat pelaksana, dan Supervisor boleh memindahkan pasien. Perpindahan berjalan satu langkah tanpa penerimaan unit tujuan. Risiko pindah kelas tanpa persetujuan diterima secara sadar, tercatat sebagai `RWI-RISK-001` | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 8; menutup `RWI-CON-003`, `OQ-RI-004`, dan `OQ-RI-005`; dirinci pada `RWI-RULE-006` |
| `RWI-DEC-013` | Decision | Pindah kelas tidak dikecualikan. Kewenangannya sama dengan pindah tempat tidur biasa, dan kelas yang ditagihkan selalu mengikuti kamar yang ditempati. Perubahannya disimpan sebagai riwayat. `RWI-RISK-001` diterima secara sadar, dan `RWI-GAP-004` pasien titipan dinyatakan belum terjawab | Product/domain owner sementara | `approved` | Pemegang sementara, 2026-08-20 | Wawancara pertanyaan 9; menutup `RWI-OQ-026` dan `RWI-OQ-015`; dirinci pada `RWI-RULE-007` |
| `RWI-DEC-014` | Open Question | Apa yang terjadi bila perpindahan pasien gagal di tengah jalan, dan apakah pasien boleh tercatat sesaat tanpa tempat tidur | Product/domain owner sementara | `draft` | — | Wawancara pertanyaan 10; menutup `RWI-CON-007` |

---

## Gate Sebelum Produksi

Persetujuan pemegang sementara **tidak** menutup gerbang berikut. Ketiganya harus terpenuhi
sebelum modul ini boleh dipakai melayani pasien sungguhan.

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk. Semua aturan klinis pada dokumen ini memakai praktik umum dan regulasi sebagai dasar, bukan persetujuan komite klinis. Termasuk batas waktu pengkajian awal, verifikasi CPPT oleh DPJP, aturan pasien meninggal, dan syarat pasien boleh pulang |
| Security/privacy owner | Belum ditunjuk. Hak akses ke rekam medis rawat inap, penelusuran audit, dan aturan koreksi data harus disetujui pemiliknya |
| Registry lifecycle | Modul `InPatientManagement` masih `PLANNED`. Sesuai `RWI-FACT-002`, status ini hanya memberi hak penamaan, belum memberi izin implementasi, migration, atau database |

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
| OQ-RI-006 | Siapa yang mengeksekusi penutupan episode | `DESIGN` | `TERBUKA` |
| OQ-RI-007 | Apa daftar periksa wajib sebelum pasien pulang | `IMPLEMENTATION` | `TERBUKA` |
| OQ-RI-008 | Apakah kelayakan keuangan memblokir atau hanya memberi peringatan | `DESIGN` | `TERBUKA` |
| OQ-RI-009 | Persetujuan umum apa saja yang wajib saat masuk | `LATER SLICE` | `TERBUKA` |
| OQ-RI-010 | Apakah bayi baru lahir, ICU, dan isolasi masuk MVP pertama | `DESIGN` | `TERBUKA` |
| OQ-RI-011 | Apakah rencana asuhan keperawatan SDKI wajib pada MVP pertama | `LATER SLICE` | `TERBUKA` |
| OQ-RI-012 | Siapa yang berhak membuka kembali episode yang sudah ditutup | `IMPLEMENTATION` | `TERBUKA` |

### Tambahan dari agent

| ID | Pertanyaan | Memblokir | Status |
|---|---|---|---|
| `RWI-OQ-013` | Cara pulang apa saja yang diakui selain pulang atas izin DPJP | `DESIGN` | `TERBUKA` |
| `RWI-OQ-014` | Bagaimana pasien meninggal diperlakukan | `DESIGN` | `TERBUKA` |
| `RWI-OQ-015` | Apakah pindah kelas perawatan termasuk MVP dan bagaimana dicatat | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-013` |
| `RWI-OQ-016` | Apakah pasien titipan termasuk MVP | `DESIGN` | `TERBUKA` |
| `RWI-OQ-017` | Apakah pemisahan jenis kelamin dan isolasi adalah aturan keras | `DESIGN` | `TERBUKA` |
| `RWI-OQ-018` | Batas waktu pengkajian awal dan aturan verifikasi CPPT oleh DPJP | `IMPLEMENTATION` | `TERBUKA` |
| `RWI-OQ-019` | Apa yang dihitung sebagai satu visite dokter | `IMPLEMENTATION` | `TERBUKA` |
| `RWI-OQ-020` | Bagaimana lama dirawat dihitung | `IMPLEMENTATION` | `TERBUKA` |
| `RWI-OQ-021` | Bagaimana obat pulang diperlakukan | `LATER SLICE` | `TERBUKA` |
| `RWI-OQ-022` | Siapa yang boleh membatalkan admisi dan apa akibatnya pada tempat tidur | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-010` |
| `RWI-OQ-023` | Siapa pemilik keputusan dan siapa yang berwenang approve modul ini | `DESIGN` | `TERBUKA` |
| `RWI-OQ-024` | Apa yang terjadi bila petugas menyelesaikan admisi setelah pemesanan tempat tidurnya gugur dan tempat tidur itu sudah diambil pasien lain | `DESIGN` | `TERBUKA` — muncul dari `RWI-DEC-008` |
| `RWI-OQ-025` | Apa isi "sesuai SOP" untuk Dokter/DPJP pada baris Transfer tabel kewenangan PRD bagian 14 | `DESIGN` | `TERBUKA` — muncul dari `RWI-DEC-012` |
| `RWI-OQ-026` | Apakah perpindahan yang mengubah kelas perawatan dikecualikan dari kewenangan perawat pelaksana | `DESIGN` | `TERTUTUP` oleh `RWI-DEC-013` |

### Blocker desain saat ini

1. Tiga dari tujuh konflik PRD masih terbuka: `RWI-CON-004`, `RWI-CON-005`, dan
   `RWI-CON-007`. Yang sudah tertutup adalah `RWI-CON-002` lewat `RWI-DEC-007`, `RWI-CON-006`
   lewat `RWI-DEC-002`, `RWI-CON-001` lewat `RWI-DEC-009`, dan `RWI-CON-003` lewat
   `RWI-DEC-012`.
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
