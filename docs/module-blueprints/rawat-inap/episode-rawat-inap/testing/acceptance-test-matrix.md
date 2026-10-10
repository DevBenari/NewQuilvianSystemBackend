# Acceptance Test Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | Mengikuti set pada manifest sub-modul; Bed Management draft, last_changed_in 0.12.0. Riwayat metadata: **`0.11.0`** — bagian 21 Workspace PPRI, `approved` 2026-10-08 (`RWI-DEC-265`). Sebelumnya `0.10.0` — bagian 20, `approved` (`RWI-DEC-221`); `0.9.0` — bagian 18 |
| `last_changed_in` | **`0.12.0`** — Bed Management. Riwayat metadata: **`0.11.0`** — bagian 21. Sebelumnya `0.10.0` — bagian 20; `0.9.0` — bagian 18; `0.8.0` — bagian 2A.1 dan 4.1 |
| Status | **`draft`** — Bed Management belum disetujui. Riwayat metadata: **`draft`** untuk `0.9.0`. `0.8.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Masukan | Decision46, gate1.12/BM-RCG-20261010-01, audit BM-AUD-20261010-01 rev1. Riwayat metadata: `00-interview-decisions.md` revision `6` (149 acceptance criteria); `contracts/api-contract.md`, `contracts/validation-matrix.md`, dan `contracts/permission-audit-matrix.md` revision `0.3.0`; kontrak lain revision `0.2.0` |
| Backend SHA | Bed Management: d4e1eca06fb28c05934c68c1e51a4dca01935a10. Riwayat metadata: `5afb54b` |
| Frontend SHA | Bed Management: 969acfcc04cdf31074a1911e9827c31d25ddadd0. Riwayat metadata: `dec4fdeff` |

Matriks ini memuat **jalur berhasil dan jalur gagal**. Jalur gagal justru yang paling membuktikan
aturan bisnis benar-benar ditegakkan.

Keadaan awal saat matriks ini disusun: backend hanya punya **satu** berkas test
(`QuilvianSystemBackend.Tests/BillingManagement/BillingModuleFoundationTests.cs`) dan frontend punya
**empat**. Tidak satu pun menyentuh tempat tidur, kunjungan, atau dokumentasi klinis. Karena itu
`RWI-DEC-051` mewajibkan test menjadi bagian pekerjaan, bukan pekerjaan terpisah.

> **Catatan Perkembangan Prasarana Uji (Status 23 September 2026)**:
> Sesuai mandat `RWI-DEC-051`, prasarana uji telah diperluas secara komprehensif:
> - **Frontend Unit Testing**: Memiliki **172+ berkas unit test suite** di `QuilvianSystemFrontendDev/tests/unit/`, mencakup puluhan test suite khusus siklus rawat inap (`inpatient-admission-*`, `inpatient-bed-board`, `inpatient-placement`, `inpatient-transfer-*`, `inpatient-departure`, `inpatient-discharge*`, `inpatient-closure*`, `inpatient-census`, `inpatient-clinical-instrument-renderer`, dll).
> - **Live E2E Verification**: Telah terdokumentasi **5 laporan pengujian operasional langsung** di direktori ini (`laporan-testing-admisi-penempatan-episode.md`, `laporan-testing-semua-tipe-pasien.md`, `laporan-testing-fase-3-tim-medis-isolasi.md`, `laporan-testing-fase-4-perpindahan-tempat-tidur.md`, dan `laporan-testing-fase-5-keputusan-pulang-resume-medis.md`).


---

## 1. Admisi dan pemesanan tempat tidur

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-001` | Tempat tidur berstatus `Reserved` tidak muncul pada pencarian tempat tidur kosong | Integrasi | `GET /available-beds` tidak memuat tempat tidur yang sedang dipesan |
| `RWI-AC-002` | Pemesanan pukul 09:15 masih mengunci pada pembacaan 11:14, dan sudah `Available` pada pembacaan 11:16, **tanpa proses latar belakang** | Integrasi | Dua pembacaan dengan waktu berbeda menghasilkan keadaan berbeda; tidak ada penjadwal yang dijalankan |
| `RWI-AC-003` | Batas 2 jam dapat diubah admin dan nilai barunya langsung dipakai | Integrasi | Ubah `BedReservationMinutes`, pemesanan berikutnya memakai nilai baru |
| **Gagal** | Memesan tempat tidur yang sudah dipesan episode lain | Integrasi | 409 dengan pesan "Tempat tidur ini sudah dipesan untuk pasien lain" |
| **Gagal** | Memesan tempat tidur berstatus `Maintenance` | Integrasi | 422 dengan pesan yang menyebut keadaan tempat tidur |
| **Gagal** | Membuka admisi pada kunjungan yang sudah punya episode | Integrasi | 409, dan `INV-INP-04` tidak dilanggar |
| **Gagal** | Membuka admisi tanpa DPJP | Integrasi | 400, dan `INV-INP-03` tidak dilanggar |
| `RWI-AC-116` | Menempatkan pasien yang sudah punya episode `Admitted` | Integrasi | 409 disertai nomor episode dan lokasi yang sedang ditempati. Membuktikan `INV-INP-10` |
| `RWI-AC-117` | Membuka admisi untuk pasien yang punya episode `Draft` lain | Integrasi | Berhasil, disertai peringatan. **Bukan** penolakan |
| **Gagal** | Menempatkan pasien yang episode lamanya `DischargePending` dan kepergiannya belum dicatat | Integrasi | 409 |
| — | Menempatkan pasien yang episode lamanya `DischargePending` tetapi kepergiannya **sudah** dicatat | Integrasi | **Berhasil.** Membuktikan batasnya kepergian fisik, bukan penutupan |

## 2. Penempatan pasien dan pencegahan tempat tidur ganda

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-059` | Setelah pasien ditempatkan, sistem menjawab siapa yang menempati dan sejak jam berapa | Integrasi | `GET /placements/by-episode/{id}` memuat baris dengan `StartDateTime` terisi |
| `RWI-AC-004` | Status episode yang tersedia hanya lima nilai. `InCare` ditolak | Unit | Enum hanya memuat lima nilai; nilai di luar itu ditolak |
| **Gagal — paling penting** | **Dua petugas merebut tempat tidur yang sama pada waktu hampir bersamaan** | Integrasi dengan dua transaksi bersamaan | Satu berhasil, satu ditolak 409. **Tepat satu** baris `InpBedPlacement` aktif untuk tempat tidur itu. Tidak ada penempatan ganda yang tersimpan |
| `RWI-AC-062` | Bila penulisan catatan penempatan gagal, `MstBed.BedStatus` juga tidak berubah | Integrasi | Paksa kegagalan di tengah transaksi; kedua tabel kembali ke keadaan semula |
| **Gagal** | Menempatkan pasien pada episode yang sudah `Admitted` | Integrasi | 409 |
| `RWI-AC-147` | Untuk jalur datang langsung dan poliklinik, waktu mulai penempatan tetap waktu penempatan dibuat dan tidak menunggu apa pun | Integrasi | `StartDateTime` sama dengan waktu server saat endpoint dipanggil; tidak ada pembacaan ke modul IGD |
| `RWI-AC-063` | Laporan selisih menampilkan tempat tidur yang statusnya tidak cocok dengan penghuninya | Integrasi | Buat selisih secara sengaja lewat perubahan langsung di database uji; laporan menampilkannya |

## 2A. Kelayakan penempatan — jenis kelamin dan isolasi

Bagian ini lahir pada `contract_version` `0.3.0`. Sebelumnya kelompok ini justru tercatat pada
bagian 14 sebagai **yang tidak diuji**, karena keputusannya belum turun.

Kedelapan aturan Kelayakan Penempatan dipanggil dari dua tindakan. Karena itu setiap skenario di
bawah wajib dijalankan **dua kali** — sekali lewat penempatan, sekali lewat perpindahan — kecuali
yang memang hanya masuk akal pada salah satunya. Test yang hanya menutup jalur penempatan akan
meloloskan pelanggaran lewat perpindahan, dan itu justru jalur yang paling sering dipakai petugas
yang sedang terburu-buru.

### 2A.1 Pemisahan jenis kelamin

**Ditulis ulang pada `0.8.0`, 11 September 2026.** `RWI-DEC-101` mencabut aturan jenis kelamin
tingkat kamar, sehingga empat baris di bawah ini **berbalik arah**: skenario yang dulu wajib
ditolak kini wajib berhasil. Baris lama dipertahankan dengan coretan supaya pembaca tahu
pembalikannya disengaja, bukan test yang kendur.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-128` | Pasien perempuan ditempatkan pada tempat tidur bertanda hanya laki-laki | Integrasi | 422 `BED_GENDER_MISMATCH` dengan pesan "Tempat tidur ini hanya untuk pasien laki-laki". **Tidak berubah** |
| ~~`RWI-AC-130`~~ **BERBALIK** | Kamar sudah dihuni pasien perempuan, pasien laki-laki masuk tempat tidur lain di kamar yang sama, dan tempat tidur itu menerima keduanya | Integrasi | **Berhasil.** ~~Dulu 422 dengan pesan menyebut nama kamar~~. Membuktikan penghuni kamar **tidak lagi diperiksa** |
| `RWI-AC-130a` ★ baru | Response `ineligible` maupun penolakan **tidak pernah** memuat kode `ROOM_GENDER_MIXED` | Integrasi | Kode itu tidak ada lagi di seluruh jalur: pencarian, pemesanan, penempatan, dan perpindahan |
| — | Kamar yang sama, pasien berikutnya berjenis kelamin sama | Integrasi | **Berhasil.** Tetap berlaku, tetapi kini membuktikan hal yang berbeda: bahwa tidak ada aturan kamar sama sekali |
| — | Kamar berisi satu tempat tidur | Integrasi | Tetap berhasil. Nilainya turun karena aturan kamar sudah tidak ada; dipertahankan sebagai regresi murah |
| ~~`RWI-AC-129`~~ **DIPERSEMPIT** | Jenis kelamin pasien belum tercatat, tempat tidur menerima keduanya, **kamar sudah berpenghuni** | Integrasi | **Berhasil.** ~~Dulu mensyaratkan kamar belum berpenghuni~~ |
| ~~**Gagal**~~ **BERBALIK** | Jenis kelamin belum tercatat, kamar sudah berpenghuni, tempat tidur menerima keduanya | Integrasi | **Berhasil.** ~~Dulu 422~~. Ini `AC-MVP-003` pada PRD V2 |
| **Gagal** | Jenis kelamin belum tercatat, tempat tidur hanya menerima satu jenis kelamin | Integrasi | 422 `PATIENT_GENDER_UNKNOWN`. **Tidak berubah.** Membuktikan pencabutan tidak melonggarkan syarat tempat tidur |
| ~~`RWI-AC-133`~~ **BERBALIK** | Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda, tempat tidur tujuan menerima keduanya | Integrasi | **Berhasil.** ~~Dulu 422~~. Tetap membuktikan penempatan dan perpindahan memanggil pemeriksaan yang sama |
| `RWI-AC-133a` ★ baru | Isolasi tetap menolak setelah aturan kamar dicabut | Integrasi | `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED` **tetap** 422. Ini regresi terpenting pass ini, karena kedua aturan itu bertetangga di dalam method yang sama dengan aturan yang dihapus |
| `RWI-AC-133b` ★ baru | Pengecualian boks bayi tetap berlaku | Integrasi | Penempatan **ke** boks bayi tetap melewati kedua aturan jenis kelamin yang tersisa |

### 2A.2 Pengecualian boks bayi — dua arah

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-132` | Bayi laki-laki ditempatkan pada boks bayi di kamar ibunya | Integrasi | **Berhasil.** Aturan jenis kelamin dan pencampuran dilewati untuk penempatan **ke** boks bayi |
| `RWI-AC-131` | Kamar berisi ibu dan bayi laki-lakinya; pasien perempuan lain hendak masuk | Integrasi | **Berhasil.** Penghuni boks bayi tidak dihitung saat memeriksa pencampuran |
| — | Kamar berisi **hanya** bayi laki-laki di boks, tanpa penghuni dewasa; pasien perempuan hendak masuk | Integrasi | **Berhasil.** Membuktikan pengecualian berlaku juga saat bayi adalah satu-satunya penghuni |
| — | Pasien dewasa ditempatkan pada tempat tidur bertanda boks bayi | Integrasi | Perilakunya mengikuti penanda master; bila master menolak, 422. **Skenario ini menguji batas pengecualian**, bukan melebarkannya |

### 2A.3 Kebutuhan isolasi — penempatan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-134` | Pasien bertanda membutuhkan isolasi ditempatkan pada tempat tidur bukan isolasi | Integrasi | 422 dengan pesan "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi" |
| — | Pasien bertanda membutuhkan isolasi ditempatkan pada tempat tidur isolasi | Integrasi | **Berhasil** |
| `RWI-AC-135` | Pasien tidak membutuhkan isolasi ditempatkan pada tempat tidur isolasi | Integrasi | 422 dengan pesan "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi". Membuktikan penjagaannya **dua arah**, bukan satu arah |
| — | Hasil pencarian tempat tidur kosong untuk pasien yang membutuhkan isolasi | Integrasi | `GET /available-beds` hanya memuat tempat tidur isolasi. Penyaring dan penolak menghasilkan jawaban yang **sama**, bukan berbeda |

### 2A.4 Kebutuhan isolasi — siapa yang boleh menetapkan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-136` | Petugas admisi menyalakan kebutuhan isolasi selagi episode `Draft` | Integrasi | Berhasil; `IsolationSource = AdmissionRecord`, `IsolationSetByUserId` terisi, `IsolationSetByDoctorId` **kosong** |
| `RWI-AC-137` | DPJP aktif mengubah kebutuhan isolasi setelah episode `Admitted` | Integrasi | Berhasil; `IsolationSource = ClinicalDecision`, `IsolationSetByDoctorId` terisi |
| `RWI-AC-139` | Dokter yang **bukan** DPJP aktif mengubah kebutuhan isolasi | Integrasi | 403. Membuktikan `GUARD-INP-04`, penjaga keempat yang tidak dapat dikerjakan mesin hak akses |
| **Gagal** | Petugas admisi mengubah kebutuhan isolasi setelah episode `Admitted` | Integrasi | 403. Membuktikan wewenangnya berhenti begitu episode aktif, bukan berlaku selamanya |
| **Gagal** | Menyalakan kebutuhan isolasi tanpa mengisi keterangan | Integrasi | 400 dengan pesan "Tuliskan alasan atau keterangan kebutuhan isolasi" |
| **Gagal** | Peran di luar admisi dan dokter memanggil endpoint kebutuhan isolasi | Integrasi | 403 dari mesin hak akses, sebelum service dijalankan |

### 2A.5 Perubahan isolasi tidak pernah ditahan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-138` | DPJP menyalakan kebutuhan isolasi sementara pasien berada di tempat tidur biasa | Integrasi | **Berhasil, tidak ditolak.** Nilai tersimpan, dan episode muncul pada `GET /monitoring/isolation-mismatch` |
| — | Pasien pada daftar pantau dipindahkan ke tempat tidur isolasi | Integrasi | Perpindahan berhasil, dan episode **hilang** dari daftar pantau pada pembacaan berikutnya |
| — | Kebalikannya: DPJP mematikan kebutuhan isolasi sementara pasien berada di tempat tidur isolasi | Integrasi | Berhasil, dan episode muncul pada daftar pantau. Membuktikan daftar pantau bekerja dua arah |
| — | Daftar pantau dibaca ketika tidak ada satu pun ketidaksesuaian | Integrasi | Daftar kosong, bukan galat |

---

## 3. Perpindahan pasien

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Perpindahan berhasil menutup penempatan lama dan membuka yang baru | Integrasi | Dua baris `InpBedPlacement`; yang lama punya `EndDateTime` dan `EndReason = Transfer` |
| — | Kelas yang ditagihkan mengikuti kamar yang ditempati | Integrasi | Baris kedua membawa `PatientClassId` kamar tujuan, bukan kamar asal |
| **Gagal — invariant** | Perpindahan gagal di tengah jalan | Integrasi | Pasien tetap pada tempat tidur semula. **Tidak pernah** ada keadaan pasien tanpa tempat tidur. Membuktikan `INV-INP-07` |
| `RWI-AC-079` | Dokter yang bukan DPJP aktif meminta perpindahan | Integrasi | 403 dengan pesan yang menyebut alasannya. Membuktikan `GUARD-INP-01` |
| `RWI-AC-080` | DPJP aktif meminta perpindahan disertai alasan | Integrasi | Berhasil |
| `RWI-AC-083` | Dokter yang tanggung jawabnya sudah berakhir meminta perpindahan | Integrasi | 403 |
| **Gagal** | Perpindahan tanpa alasan | Integrasi | 400 |
| **Gagal** | Perpindahan ke tempat tidur yang sedang ditempati | Integrasi | 409 |
| **Gagal** | Memindahkan pasien yang kepergiannya sudah dicatat | Integrasi | 422 |

## 4. Penanggung jawab

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-078` | Sistem menjawab siapa DPJP pada tanggal tertentu | Integrasi | Dua baris penugasan berperiode; query per tanggal mengembalikan yang benar |
| `RWI-AC-081` | Pengalihan DPJP tanpa alasan ditolak | Integrasi | 400 |
| `RWI-AC-082` | Setelah dialihkan, baris DPJP sebelumnya tetap terbaca dan tidak tertimpa | Integrasi | Baris lama masih ada dengan `EndDateTime` terisi |
| `RWI-AC-084` | Episode aktif tidak pernah tanpa DPJP aktif maupun dengan dua DPJP aktif | Integrasi | Unique index parsial menolak baris kedua yang `EndDateTime` kosong |
| `RWI-AC-101` | Kepala ruangan menugaskan perawat, dan census menampilkan namanya | Integrasi | Census memuat nama perawat |
| `RWI-AC-102` | Peran selain kepala ruangan menugaskan perawat | Integrasi | 403 |
| `RWI-AC-104` | Episode tanpa perawat tetap dapat menerima perpindahan | Integrasi | Perpindahan berhasil walaupun perawat belum ditugaskan |
| `RWI-AC-105` | Episode tanpa perawat muncul pada daftar pantau | Integrasi | `GET /monitoring/unassigned-nurse-episodes` memuat episode itu |

### 4.1 Peran penugasan dokter — lahir `0.8.0`

Menyerap `RWI-DEC-099`. Satu baris lama **berubah maknanya** dan wajib dibaca ulang sebelum
test-nya ditulis.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| ~~`RWI-AC-084`~~ **DIPERSEMPIT** | Episode aktif tidak pernah punya dua **DPJP** aktif | Integrasi | Index unik menolak baris `Dpjp` kedua yang `EndDateTime` kosong. ~~Dulu menolak penugasan terbuka kedua apa pun perannya~~ |
| `RWI-AC-084a` ★ baru | Satu episode punya satu DPJP, dua konsulen, dan satu dokter jaga aktif bersamaan | Integrasi | **Keempatnya tersimpan.** Membuktikan filter index memandang peran, bukan sekadar `EndDateTime` |
| `RWI-AC-084b` ★ baru | Migration mengisi seluruh baris lama menjadi `Dpjp` | Migration | Nol baris berperan kosong; jumlah baris sebelum dan sesudah sama persis |
| `RWI-AC-084c` ★ baru | Urutan migration dijalankan terbalik, index diganti sebelum kolom terisi | Migration | **Gagal terkendali**, bukan diam-diam lolos. Membuktikan urutan tiga langkah memang mengikat |
| `RWI-AC-084d` ★ baru | Konsulen mencoba mengubah perannya sendiri menjadi `Dpjp` pada baris yang sama | Integrasi | Ditolak. Peran diubah dengan menutup baris lalu membuka baru, bukan menimpa |
| `RWI-AC-084e` ★ baru | Pengalihan DPJP menutup baris lama dan membuka baris baru dalam satu transaksi | Integrasi | Tidak pernah ada saat ketika dua baris `Dpjp` terbuka, dan tidak pernah ada saat tanpa DPJP |
| `RWI-AC-084f` ★ baru | Dokter berperan `Consultant` meminta keputusan pulang | Integrasi | 403. Ini keadaan fail-closed `OPEN-MVP-004`, bukan kebijakan yang sudah diputuskan |
| `RWI-AC-084g` ★ baru | Dokter berperan `OnCallDoctor` menandatangani resume | Integrasi | 403 |
| `RWI-AC-084h` ★ baru | Rollback migration dijalankan setelah satu baris `Consultant` tersimpan | Migration | **Gagal terkendali.** Membuktikan batas rollback yang tertulis pada `data/data-dictionary.md` bagian 2.1 memang nyata |

## 4A. Kepergian fisik pasien

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-118` | Mencatat kepergian melepas tempat tidur seketika | Integrasi | Baris penempatan tertutup dengan `EndReason = PatientDeparted`; `MstBed.BedStatus` menjadi `Available`; tempat tidur muncul pada pencarian berikutnya |
| `RWI-AC-119` | Setelah kepergian dicatat, episode tetap `DischargePending` | Integrasi | Status tidak berubah, dan episode tetap muncul pada daftar pantau penutupan tertunda |
| `RWI-AC-120` | Pasien yang sudah pergi tidak muncul di census dan tidak dapat dipindahkan | Integrasi | Census tidak memuatnya; perpindahan ditolak 422 |
| `RWI-AC-121` | Menutup episode tanpa mencatat kepergian tetap berhasil | Integrasi | Tempat tidur dilepas saat penutupan, `EndReason = EpisodeClosed` |
| — | Kepergian **tidak** menulis baris riwayat status | Integrasi | Jumlah baris `InpStatusHistory` tidak bertambah |
| **Gagal** | Mencatat kepergian pada episode `Admitted` | Integrasi | 422 |
| **Gagal** | Mencatat kepergian dua kali | Integrasi | 409 |
| **Gagal** | Waktu kepergian mendahului keputusan pulang | Integrasi | 400 |
| **Gagal** | Kepergian dicatat peran yang tidak berwenang | Integrasi | 403 |
| **Gagal** | Pelepasan tempat tidur gagal di tengah jalan | Integrasi | Kolom kepergian pada episode juga tidak terisi |

## 5. Census dan lama dirawat

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Census menampilkan pasien `Admitted` dan `DischargePending`, tidak menampilkan `Draft`, `Closed`, `Cancelled` | Integrasi | Lima episode berstatus berbeda; census memuat tepat dua |
| — | Lama dirawat dihitung dari selisih tanggal dengan hasil paling sedikit 1 hari | Unit | Masuk 21 Sept 22:30, pulang 22 Sept 06:00 menghasilkan **1 hari**, bukan 0 |
| — | Lama dirawat bertambah pada pergantian tanggal, bukan setiap genap 24 jam | Unit | Masuk 21 Sept 22:30; pada 22 Sept 00:30 sudah bernilai 1 |
| `RWI-AC-064` | Setelah episode ditutup, tempat tidur terbaca `Available` pada pencarian berikutnya | Integrasi | `GET /available-beds` memuat tempat tidur itu |

## 6. Pembatalan admisi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Pembatalan `Draft` oleh petugas admisi disertai alasan | Integrasi | Episode `Cancelled`, pemesanan `Cancelled`, tempat tidur `Available` |
| — | Pembatalan `Admitted` oleh supervisor | Integrasi | Episode `Cancelled`, penempatan ditutup dengan `EndReason = AdmissionCancelled` |
| **Gagal** | Pembatalan `Admitted` oleh petugas admisi | Integrasi | 403 |
| **Gagal** | Pembatalan tanpa alasan | Integrasi | 400 |
| **Gagal** | Pelepasan tempat tidur gagal saat pembatalan | Integrasi | Seluruh pembatalan dibatalkan; episode tetap `Admitted` |

## 7. Keputusan pulang dan resume

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-093` | Resume menampilkan DPJP beserta periodenya secara otomatis | Integrasi | Resume memuat dua DPJP bila episode berganti DPJP |
| `RWI-AC-092` | Membuat resume kedua untuk episode yang sama | Integrasi | 409. Membuktikan `INV-INP-05` |
| `RWI-AC-096` | Cara pulang `Referred` tetapi tujuan rujukan kosong | Integrasi | 400 |
| `RWI-AC-097` | Mengubah resume setelah episode ditutup tanpa sesi koreksi | Integrasi | 409 dengan pesan yang mengarahkan pada sesi koreksi |
| `RWI-AC-124` | Menyunting resume yang belum ditandatangani | Integrasi | Isi berubah, **tidak** ada baris versi yang dibuat |
| `RWI-AC-125` | Mengubah resume yang sudah ditandatangani lewat sesi koreksi | Integrasi | Satu baris `InpDischargeSummaryRevision` berisi salinan isi lama, penandatangan lama, dan sesi koreksi yang menyebabkannya |
| `RWI-AC-126` | Mengubah atau menghapus salinan versi resume | Integrasi | Tidak ada endpoint; percobaan menghasilkan 404 |
| **Gagal** | Dokter bukan DPJP aktif menyatakan pasien boleh pulang | Integrasi | 403. Membuktikan `GUARD-INP-02` |
| **Gagal** | Dokter bukan DPJP aktif menandatangani resume | Integrasi | 403. Membuktikan `GUARD-INP-03` |
| **Gagal** | Menandatangani resume tanpa diagnosis utama | Integrasi | 400 |

## 8. Kelayakan keuangan dan penutupan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-065` | Episode baru berstatus kelayakan `Pending`, dan penutupan ditolak | Integrasi | 422 menyebut kelayakan keuangan |
| `RWI-AC-066` | Setelah kasir menandai `Cleared`, penutupan berhasil tanpa supervisor | Integrasi | Episode `Closed` |
| `RWI-AC-067` | Petugas admisi, perawat, atau dokter menandai kelayakan keuangan | Integrasi | 403 |
| `RWI-AC-068` | Penandaan tanpa catatan ditolak; yang berhasil menyimpan nama dan waktu | Integrasi | 400 pada kasus pertama; kolom terisi pada kasus kedua |
| `RWI-AC-069` | Layar dan laporan menampilkan bahwa kelayakan berasal dari penandaan manual | e2e | `IsManualMarking` terbaca pada jawaban dan tampil di layar |
| `RWI-AC-070` | Episode `Blocked` tidak dapat ditutup petugas admisi | Integrasi | 422 |
| — | Supervisor menutup menembus gerbang keuangan disertai alasan | Integrasi | Episode `Closed`, `IsClosedWithoutFinancialClearance = true`, muncul pada laporan pengecualian |
| **Gagal** | Supervisor menembus gerbang sementara resume belum ditandatangani | Integrasi | 422. Membuktikan jalan keluar **hanya** menembus syarat keuangan |
| **Gagal** | Menutup episode berstatus `Admitted` | Integrasi | 422 dengan pesan yang menyebut keputusan pulang |
| — | Jawaban `closure-readiness` memuat kelima syarat beserta tanda sudah atau belum | Integrasi | Lima baris, bukan satu nilai boleh atau tidak |

## 9. Daftar periksa administrasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-053` | Butir "obat pulang sudah diserahkan" dinonaktifkan admin, lalu penutupan tidak lagi tertahan olehnya | Integrasi | Butir nonaktif tidak muncul pada syarat penutupan |
| — | Butir wajib yang belum ditandai menahan penutupan | Integrasi | 422 menyebut nama butirnya |
| **Gagal** | Menandai butir pada episode yang sudah `Closed` | Integrasi | 409 |

## 10. Riwayat status dan sesi koreksi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-085` | Seluruh perpindahan status terbaca urut lengkap dengan pelaku, waktu, dan alasan | Integrasi | Episode dari `Draft` sampai `Closed` meninggalkan empat baris berurutan |
| `RWI-AC-086` | Bila penulisan riwayat gagal, status episode juga tidak berubah | Integrasi | Paksa kegagalan; kedua tabel kembali semula |
| `RWI-AC-087` | Baris riwayat tidak dapat diubah maupun dihapus lewat endpoint mana pun | Integrasi | Tidak ada endpoint update atau delete; percobaan menghasilkan 404 |
| `RWI-AC-088` | Pemesanan yang gugur meninggalkan baris riwayat bertanda dilakukan sistem | Integrasi | Baris dengan `ActorType = System` dan `ChangedByUserId` kosong |
| `RWI-AC-089` | Episode `Draft` yang batal sendiri meninggalkan baris bertanda sistem | Integrasi | Sama seperti di atas |
| `RWI-AC-090` | Laporan penutupan tanpa kelayakan keuangan disusun dari tabel riwayat tanpa membaca berkas log | Integrasi | Laporan benar walaupun berkas log dikosongkan |
| — | Sesi koreksi dibuka, cara pulang diubah, sesi ditutup | Integrasi | Status episode **tetap** `Closed` sepanjang sesi; tempat tidur tidak berubah; lama dirawat tidak bertambah |
| **Gagal** | Membuka sesi koreksi kedua sementara yang pertama belum ditutup | Integrasi | 409 |
| **Gagal** | Menutup sesi koreksi tanpa daftar perubahan | Integrasi | 400 |
| **Gagal** | Peran selain supervisor membuka sesi koreksi | Integrasi | 403 |

## 11. Pengaturan admin

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-110` | Batas pemesanan diubah dari 2 jam menjadi 3 jam, dan nilai baru berlaku pada pembacaan berikutnya tanpa aplikasi dinyalakan ulang | Integrasi | Pemesanan baru memakai 3 jam |
| `RWI-AC-111` | Kelima angka dapat diubah dari satu layar yang sama | e2e | Satu layar memuat kelimanya |
| `RWI-AC-112` | Setiap perubahan menyimpan nama pengubah dan waktunya | Integrasi | Kolom audit terisi |
| `RWI-AC-113` | Butir daftar periksa dikelola dari layar tersendiri | e2e | Layar terpisah |
| — | Pemesanan yang sudah berjalan **tidak** ikut berubah ketika pengaturan diubah | Integrasi | `ExpiresAt` pemesanan lama tetap memakai nilai lama |

## 12. Perbaikan dan regresi modul lain

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-108` | Tombol nonaktifkan tempat tidur berhasil, dan tempat tidur hilang dari pencarian | e2e | Tidak ada 404 |
| `RWI-AC-109` | Tombol aktifkan berhasil mengaktifkan kembali | e2e | Tidak ada 404 |
| `RWI-AC-060` | Menyetel `BedStatus` menjadi `Occupied` lewat menu master data ditolak | Integrasi | 422 dengan pesan yang mengarahkan ke modul Rawat Inap |
| `RWI-AC-061` | Menyetel `BedStatus` menjadi `Maintenance` lewat menu master data tetap berhasil | Integrasi | 200. Membuktikan wewenang admin tidak berkurang |
| `RWI-AC-114` | Setiap task yang menyentuh Master Data membawa test regresi jalur lama | Integrasi | Endpoint bed lain tetap berperilaku sama |
| `RWI-AC-115` | Penjaga kewenangan DPJP punya test yang membuktikan dokter bukan DPJP ditolak | Integrasi | Sudah tercakup pada bagian 3 dan 7 |

## 12A. Bayi baru lahir dan hubungan dengan ibu

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-122` | Episode bayi menyimpan rujukan ke episode ibunya | Integrasi | Detail episode bayi memuat episode ibu; pencarian bayi yang dirawat gabung di kamar tertentu berhasil |
| `RWI-AC-123` | Menutup episode ibu tidak menutup episode bayi | Integrasi | Episode bayi tetap `Admitted`, boks bayi tetap terisi |
| `RWI-AC-127` | Riwayat lokasi satu episode terbaca lengkap dari catatan penempatan milik Rawat Inap | Integrasi | Tidak ada query ke tabel milik modul Registrasi |
| **Gagal** | Episode bayi menunjuk episode milik pasien yang sama | Integrasi | 422 |
| **Gagal** | Episode menunjuk dirinya sendiri sebagai episode ibu | Integrasi | 400 |
| **Gagal** | Episode ibu yang ditunjuk sudah `Closed` | Integrasi | 422 |

## 13. Data master awal

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-106` | Seeder menolak berjalan pada lingkungan produksi | Unit | Seeder mengembalikan penolakan bila lingkungan produksi |
| `RWI-AC-107` | Admin menambah kamar dan tempat tidur lewat layar, tanpa perintah database | e2e | Kamar dan tempat tidur baru muncul pada pencarian |
| — | Modul tetap berjalan bila `MstInpatientSetting` belum terisi | Integrasi | Nilai bawaan dipakai, dan peringatan tercatat |

---

## 14. Yang **tidak** diuji pada revisi ini

| Yang tidak diuji | Alasan | Decision ID |
| --- | --- | --- |
| Pengkajian, catatan dokter, tindakan, resep untuk pasien rawat inap | Slice di luar scope | `DEC-INP-001` |
| Serah terima IGD ke rawat inap | Slice di luar scope | `DEC-INP-002` |
| Persetujuan umum rawat inap | Slice di luar scope | `DEC-INP-003` |
| Pengiriman SATUSEHAT | Slice di luar scope | `DEC-INP-005` |
| Cara pulang meninggal dan kabur | Slice di luar scope | `DEC-INP-007` |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice yang di luar scope | `DEC-INP-001` |

**Sembilan acceptance criteria baru dari Amendment Pass 2026-08-24 juga belum dapat diuji di
sini,** dan alasannya sama: keduanya milik slice yang di luar scope. Didaftar supaya tidak
hilang, bukan supaya dilupakan.

| Requirement | Kenapa belum diuji | Milik siapa |
| --- | --- | --- |
| `RWI-AC-140` s.d. `RWI-AC-144` | Pelonggaran mesin klinis untuk kunjungan `Emergency` dan penjagaan agar rawat jalan tidak berubah | `DEC-INP-001`; pelaksananya modul IGD lewat `IGD-DEC-068` |
| `RWI-AC-145`, `RWI-AC-146` | Penempatan menunggu event `Tiba` dan waktu mulai dibaca dari IGD | `DEC-INP-002`; jalur `INP-S09` |
| `RWI-AC-148`, `RWI-AC-149` | Rangkaian kedatangan lewat `OriginEncounterId` | `DEC-INP-002`; kolomnya dikerjakan modul IGD lewat `IGD-DEC-075` |

`RWI-AC-147` **tidak** ada pada daftar ini. Ia justru dapat diuji sekarang, dan sudah masuk
bagian 2 sebagai penjaga agar jalur datang langsung tidak ikut berubah.

Ketiadaan test untuk **keenam** butir itu adalah **keadaan yang disengaja**, bukan cakupan yang
terlupa.

**Satu baris keluar dari daftar ini pada `0.3.0`.** "Penolakan penempatan karena isolasi atau jenis
kelamin" kini justru menjadi bagian 2A dengan 26 skenario, setelah `DEC-INP-004` turun lewat
`RWI-DEC-064` sampai `RWI-DEC-066`.

---

## 15. Ringkasan cakupan

| Kelompok | Skenario berhasil | Skenario gagal |
| --- | ---: | ---: |
| Admisi dan pemesanan | 5 | 6 |
| Penempatan dan tempat tidur ganda | 4 | 2 |
| **Kelayakan penempatan — jenis kelamin dan isolasi** | 15 | 11 |
| Perpindahan | 3 | 6 |
| Penanggung jawab | 6 | 2 |
| **Kepergian fisik pasien** | 5 | 5 |
| Census dan lama dirawat | 4 | 0 |
| Pembatalan | 2 | 3 |
| Pulang dan resume | 6 | 4 |
| Kelayakan dan penutupan | 6 | 4 |
| Daftar periksa | 2 | 1 |
| Riwayat dan koreksi | 7 | 3 |
| Pengaturan | 5 | 0 |
| Perbaikan dan regresi | 6 | 0 |
| Bayi dan hubungan dengan ibu | 3 | 3 |
| Data master | 3 | 0 |
| **Total** | **82** | **49** |

Delapan skenario gagal yang paling penting, dan tidak boleh dilewati:

1. **Dua petugas merebut tempat tidur yang sama** — membuktikan `INV-INP-02`.
2. **Perpindahan gagal di tengah jalan** — membuktikan `INV-INP-07`.
3. **Dokter bukan DPJP meminta perpindahan** — membuktikan `GUARD-INP-01`, satu-satunya kewenangan
   yang tidak dijaga mesin hak akses.
4. **Supervisor menembus gerbang keuangan sementara resume belum ditandatangani** — membuktikan
   jalan keluar itu benar-benar sempit.
5. **Menempatkan pasien yang sudah punya episode aktif** — membuktikan `INV-INP-10`.
6. **Menempatkan pasien yang episode lamanya sudah ditinggalkan secara fisik** — membuktikan
   batasnya kepergian, bukan penutupan. Ini kebalikan nomor 5 dan sama pentingnya: yang pertama
   mencegah data ganda, yang kedua mencegah pasien tertahan oleh urusan administrasi.
7. **Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda** — membuktikan kedua tindakan
   memanggil pemeriksaan yang sama. Test yang hanya menutup jalur penempatan meloloskan pelanggaran
   lewat jalur yang justru paling sering dipakai.
8. **Pasien biasa menempati tempat tidur isolasi** — membuktikan penjagaannya dua arah. Arah ini
   yang paling sering terlupa, karena tidak terasa berbahaya bagi pasien yang bersangkutan; yang
   dirugikan adalah pasien berikutnya yang benar-benar membutuhkan isolasi.

---

## 16. Perubahan pada `contract_version` `0.3.0`

| Yang ditambahkan | Dasar |
| --- | --- |
| Bagian 2A kelayakan penempatan, 26 skenario dalam lima kelompok | `RWI-DEC-064`, `RWI-DEC-065`, `RWI-DEC-066` |
| Dua skenario gagal wajib baru pada ringkasan cakupan | `RWI-RULE-012` A.6 dan B.7 |

| Yang dihapus | Alasan |
| --- | --- |
| Baris "penolakan penempatan karena isolasi atau jenis kelamin" pada bagian 14 | Bukan lagi di luar scope; keputusannya turun 2026-08-21 |

Dua belas acceptance criteria baru `RWI-AC-128` sampai `RWI-AC-139` seluruhnya tercakup.

---

## 17. Perubahan pada `contract_version` `0.2.0`

| Yang ditambahkan | Dasar |
| --- | --- |
| Bagian 4A kepergian fisik pasien, 10 skenario | `RWI-DEC-055` |
| Empat skenario satu pasien satu episode pada bagian 1 | `RWI-DEC-054` |
| Tiga skenario versi resume pada bagian 7 | `RWI-DEC-057` |
| Bagian 12A hubungan bayi dan ibu, 6 skenario | `RWI-DEC-056` |
| Satu skenario kepemilikan riwayat lokasi pada bagian 12A | `RWI-DEC-053` |

Tidak ada skenario yang dihapus pada revision ini.

---

## 18. Perubahan pada `contract_version` `0.9.0` — amandemen terbatas ★ 15 September 2026

**Status `draft`.** Seluruh skenario negatif memakai peran nyata non-SuperAdmin.

### 18.1 Census dokter — `RWI-DEC-111`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `FR-RI-191` | dr. Ahmad DPJP Budi, konsulen Sari, 120 pasien lain; `assignedToMe=true` | Integrasi | Dua baris; `TotalCount = 2`; baris Sari `MyAssignmentRole = Consultant` |
| `FR-RI-191` | dr. Yoga jaga Joko 22.00–07.00; query 06.59 dan 07.01 | Integrasi | Joko ada pada 06.59, tidak ada pada 07.01, tanpa proses latar |
| **Gagal** `FR-RI-191` | dr. Ahmad mengirim `DoctorId` dr. Rina bersama `assignedToMe=true` | Integrasi | Hasil tetap milik dr. Ahmad |
| **Gagal** `FR-RI-192` | Akun tanpa data dokter | Integrasi | Daftar kosong beserta pesan; bukan `403` |
| Regresi | Census tanpa `assignedToMe` oleh kepala ruangan | Integrasi | Hasil sama seperti sebelum perubahan |

### 18.2 Penugasan pendukung — `RWI-DEC-099`, `RWI-DEC-130`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `FR-RI-193` | Kepala ruangan melibatkan dr. Ahmad sebagai konsulen | Integrasi | `201`; dr. Ahmad muncul pada census dirinya |
| `FR-RI-194`, `RWI-AC-189` | Penugasan singkat dr. Rina Kamis 10.00–11.00 beralasan | Integrasi | `201`; peran `OnCallDoctor`; DPJP aktif tetap dr. Ahmad |
| **Gagal** `RWI-AC-189` | Penugasan singkat tanpa waktu selesai | Integrasi | `400` `VAL-INP-01`; nol baris |
| **Gagal** `FR-RI-194` | Penugasan singkat berperan konsulen | Integrasi | `400` `VAL-INP-01` |
| **Gagal** `FR-RI-194` | Penugasan singkat berwaktu mulai Selasa | Integrasi | `400` `VAL-INP-08` |
| **Gagal** `FR-RI-193` | Perawat pelaksana pemegang `InpatientEpisode : Update` membuat penugasan | Integrasi | `403` `GUARD-INP-09` |
| **Gagal** `RWI-AC-190` | Selama penugasan singkat aktif, dr. Rina memverifikasi CPPT | Integrasi | `403` |
| `FR-RI-195` | Kepala ruangan mengakhiri penugasan konsulen | Integrasi | `EndDateTime` terisi; dr. Ahmad hilang dari census dirinya |
| **Gagal** `FR-RI-195` | Mengakhiri penugasan DPJP lewat `…/end` | Integrasi | `409` |
| Database | Menyisipkan baris `LateDocumentation` tanpa waktu selesai langsung ke database | Integrasi PostgreSQL | Check constraint menolak |

### 18.3 Resume — `RWI-DEC-112`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `FR-RI-196` | dr. Rina menyimpan draf dengan tiga isian baru lalu menandatangani | Integrasi | Tiga isian tersimpan; episode **tidak** tertutup |
| `FR-RI-196` | Resume bertanda tangan dikoreksi lewat sesi koreksi | Integrasi | Versi lama menyimpan tiga isian lamanya |
| `FR-RI-197` | `summary-prefill` untuk Budi | Integrasi | Diagnosis, tindakan, obat pulang, hasil kritis, edukasi terisi beserta sumber; nol baris resume berubah |
| **Gagal** `FR-RI-197` | Modul laboratorium tidak menjawab | Integrasi | `ImportantFindingsSummary.SourceStatus = Unavailable`; isian lain tetap |
| **Gagal** `FR-RI-196` | Edukasi 2.500 karakter | Integrasi | `400` `VAL-INP-12` |

### 18.4 Penutupan episode — `RWI-DEC-138`, `RWI-DEC-143`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `FR-RI-198`, `RWI-AC-199` | Joko: satu konsep SOAP dr. Yoga; ditutup 13.00 | Integrasi | Konsep `LockedUnsigned`; `SideEffects.LockedDraftCount = 1` |
| `FR-RI-199`, `RWI-AC-213` | Pesanan cek GDS Ns. Siti belum dilaksanakan | Integrasi | `Cancelled` "episode ditutup sebelum dilaksanakan" |
| `FR-RI-199` | Pesanan tertunda sudah ditagih | Integrasi | Tidak dibatalkan; muncul pada `billed-pending-procedure-orders`; penutupan tidak tertahan |
| `FR-RI-200`, `AC-KEP-113` | Dosis 08.00 belum dicatat, dosis 20.00 | Integrasi | 20.00 `Cancelled`; 08.00 tetap `Due` |
| `FR-RI-201` | `closure-readiness` 12.55 | Integrasi | `CanClose = true`; tiga peringatan |
| **Gagal** `INV-INP-11` | Galat buatan pada langkah penguncian | Integrasi PostgreSQL | Episode tetap `DischargePending`; tempat tidur tetap terisi; pesanan dan dosis tidak berubah |
| **Gagal** `RWI-AC-201` | Episode dibuka kembali lewat sesi koreksi | Integrasi | Konsep tetap `LockedUnsigned`; pesanan dan dosis tetap batal |
| `RWI-AC-214` | Setelah penutupan | Integrasi | Nol pesanan tindakan berstatus terkunci "Tidak Ditandatangani" |

### 18.5 Status Ketergantungan Pengujian pada `0.9.0` (Pemutakhiran 23 September 2026)

| Skenario | Ketergantungan Awal | Status Implementasi Terkini | Kesiapan Pengujian |
| --- | --- | --- | --- |
| **Langkah 5 penutupan** (pembatalan pesanan tindakan belum jalan) | `DOK-V2-1` (`BE-RWI-091` s.d. `BE-RWI-098`) | ✅ **SELESAI** (17 Sept 2026). Logika pembatalan pesanan tindakan telah terpasang pada `InpDischargeService.Closure.cs:1072-1077` (`BE-RWI-086`) | Siap diuji pada verifikasi Fase 8 (Penutupan) |
| **Langkah 6 penutupan** (pembatalan dosis berjadwal belum jalan) | `KEP-V2-2` (`BE-RWI-114` s.d. `BE-RWI-123`) | ✅ **SELESAI** (17 Sept 2026). Logika pembatalan dosis obat berjadwal telah terpasang memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync` pada `InpDischargeService.Closure.cs:1081-1086` (`BE-RWI-087`) | Siap diuji pada verifikasi Fase 8 (Penutupan) |
| **Bagian Lab/Rad `NeedsReviewCount`** | Modul Penunjang Lab/Rad | Menunggu verifikasi sinkronisasi lintas modul | Pengujian terpisah |

---

## 19. Registri Keterlacakan Laporan Pengujian Operasional Langsung (*Live E2E Reports*)

Untuk mematuhi aturan keterlacakan (*traceability*) dan membuktikan penegakan aturan bisnis secara nyata di lingkungan operasional, laporan hasil pengujian langsung dikelola dalam direktori ini:

| No | Fase Uji | Berkas Laporan | Cakupan Skenario | Status Hasil |
| :---: | :--- | :--- | :--- | :---: |
| 1 | **Fase 1 & Fase 2** | [`laporan-testing-admisi-penempatan-episode.md`](laporan-testing-admisi-penempatan-episode.md) | Pendaftaran pasien lama, penentuan DPJP & kelas, draf episode, reservasi bed (`POST /reservations`), dan penempatan pasien (`POST /placements`). | 🟢 **100% LULUS** |
| 2 | **Langkah 3 Admisi** | [`laporan-testing-semua-tipe-pasien.md`](laporan-testing-semua-tipe-pasien.md) | Validasi perilaku antarmuka untuk 6 kategori tipe pasien: Umum, Ibu, Bayi Baru Lahir, Anak, Pegawai, Korporat. | ⚠️ **5/6 LULUS**<br>(Bayi Baru Lahir `BLOCKED` `ISSUE-002`) |
| 3 | **Fase 3** | [`laporan-testing-fase-3-tim-medis-isolasi.md`](laporan-testing-fase-3-tim-medis-isolasi.md) | Alih rawat DPJP utama (`POST /doctor-assignments`), penugasan PPJA (`POST /nurse-assignments`), penegakan guard isolasi (`RWI-RULE-004`), dan sinkronisasi ke sensus bangsal. | 🟢 **100% LULUS** |
| 4 | **Fase 4** | [`laporan-testing-fase-4-perpindahan-tempat-tidur.md`](laporan-testing-fase-4-perpindahan-tempat-tidur.md) | Perpindahan bed transaksi atomik (`POST /placements/transfer`), penegakan alasan medis wajib, dialog konfirmasi dua arah, dan riwayat penempatan. | 🟢 **100% LULUS** |
| 5 | **Fase 5** | [`laporan-testing-fase-5-keputusan-pulang-resume-medis.md`](laporan-testing-fase-5-keputusan-pulang-resume-medis.md) | Penegakan guard eksklusif DPJP aktif (`GUARD-INP-02`), keputusan pemulangan klinis (`POST /discharges/{id}/decide`), penyusunan resume medis 8 bagian klinis, dan tanda tangan digital. | 🟢 **100% LULUS** |
| 6 | **Fase 6 s.d. 8** | [`laporan-testing-fase-6-clearance-penutupan-episode.md`](test-by-agy/laporan-testing-fase-6-clearance-penutupan-episode.md) | Kliring finansial kasir (`POST /financial-clearance`), pencatatan kepergian fisik (`POST /record-departure`), butir administrasi, evaluasi 5 syarat kesiapan, dan penutupan resmi episode (`POST /close`). | 🟢 **100% LULUS** |
| 7 | **Siklus Lengkap (End-to-End)** | [`laporan-testing-siklus-lengkap-episode-rawat-inap.md`](test-by-agy/laporan-testing-siklus-lengkap-episode-rawat-inap.md) | Verifikasi live siklus utuh dari pendaftaran pasien lama, admisi, draf episode, reservasi bed, penempatan, tim medis PPJA, isolasi, alih rawat tempat tidur, keputusan pulang DPJP, resume medis digital, kliring kasir, kepergian fisik, hingga penutupan resmi dan verifikasi sensus bersih. | 🟢 **100% LULUS** |

---

## 20. Amandemen kontrak `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

Akibat di modul lain (Kamar Operasi, Billing, Clinical) lulus hanya bila terbukti di modul penerima (`RWI-DEC-168`). Skenario bertanda **OK** disaksikan pemilik modul OK; persetujuan OK atas `RWI-OQ-114` sudah tercatat (`RWI-DEC-208`).

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWF-041` / `AC-RWF-040` | Perawat memesan bedah untuk pasien samaran yang punya order "Appendektomi" | Integrasi dengan OK sungguhan + E2E | Kasus `Requested` di daftar Kasus Operasi; dokter operator dari order; penginput akun perawat; tepat satu `OprCaseProcedure` |
| `FR-RWF-041` / `AC-RWF-048` | Pesan tanpa order, dengan order milik kunjungan lain, dan dengan order yang dibatalkan | Integrasi | Ketiganya `422 INP-SRG-001`; tidak ada kasus OK |
| `FR-RWF-043` | Pesan dari tab Bedah Obgyn dengan isian `SurgicalServiceType = General` dari klien | Integrasi | Kasus tersimpan `Obstetric` |
| `FR-RWF-044` / `AC-RWF-041` | OK menjadwalkan kasus, bangsal memuat ulang | E2E | Label "Terjadwal" beserta tanggal dan jam |
| `FR-RWF-045` / `AC-RWF-042` | Pra-operasi dengan butir wajib belum dikonfirmasi penerima | Integrasi | Kesiapan OK memuat `WARD_PRE_OP_INCOMPLETE`; "Siap" ditolak |
| `FR-RWF-048` / `AC-RWF-045` | Kasus sisi Kanan, penandaan Kiri | Integrasi | `422 OPR-WPO-001` saat kirim dan saat konfirmasi |
| `AC-RWF-046` | Akun pengirim mencoba konfirmasi pra-operasi; akun pengirim serah terima mencoba menerima | Integrasi | `OPR-WPO-002`; `OPR-HO-002` |
| `FR-RWF-090` / `AC-RWF-093` | Pra-operasi `Confirmed`, kasus ditunda | Integrasi | Versi 1 `NeedsUpdate` dalam transaksi tunda; versi 1 tetap terbaca; "Siap" ditolak `WARD_PRE_OP_NEEDS_UPDATE` |
| `FR-RWF-090` / `AC-RWF-094` | Dijadwalkan ulang; TD baru dicatat; versi 2 dikirim lalu dikonfirmasi akun OK lain | Integrasi + E2E | Potret versi 2 = TD terbaru; versi 1 `Superseded`; "Siap" diterima |
| `FR-RWF-046` / `AC-RWF-043` | Perawat bangsal pemegang `Receive` menerima serah terima | Integrasi | `Accepted`, penerima dan waktu dari akun login |
| `FR-RWF-046` / `AC-RWF-047` | Serah terima ke ICU saat pasien masih di Melati; lalu transfer; lalu terima | E2E **OK** | `422 OPR-HO-001`, kemudian berhasil; bed tidak berpindah oleh penerimaan |
| `INV-RWF-28` | Pemeriksaan lokasi bed gagal (layanan baca dimatikan di lingkungan uji) | Integrasi | Penerimaan ditolak, bukan diloloskan |
| `FR-RWF-049` / `AC-RWF-049` | Kasus `InProgress` 08.00–11.00 | Integrasi dengan Billing | Bed asal tetap terisi; tarif kamar berjalan |
| `FR-RWF-047` / `AC-RWF-044` | Kasus `Completed` dengan catatan anestesi final dan 2 bahan `Used`; kasus lain dibatalkan sebelum selesai | Integrasi dengan Clinical dan Billing **OK** | Kasus selesai: tepat satu baris tindakan (dari order), satu anestesi, satu sewa kamar, dua bahan; kasus batal: nol baris |
| `INV-RWF-29` | Jalankan `OperatingRoomCompletionEffects` dua kali untuk kasus yang sama | Integrasi | Tidak ada baris ganda; order tindakan yang sudah `Completed` dilewati |
| `AC-RWF-092` | Bahan tertagih lewat OK lalu diretur dan lolos pemeriksaan | Integrasi dengan Billing | Tidak ada tagihan Farmasi untuk bahan itu; retur membatalkan tagihan OK |
| `FR-RWF-047` | Tarif sewa kamar operasi tidak ada di master | Integrasi dengan Billing | Baris "tarif belum ada"; invoice tidak dapat difinalkan (`VAL-RWF-17`) |
| `FR-RWF-086` / `AC-RWF-085` | Tolak kasus Diminta tanpa alasan, lalu dengan alasan | Integrasi + E2E **OK** | `400`, lalu `Rejected` dengan penolak dan waktu; bangsal melihat label merah dan alasannya |
| `AC-RWF-099` | Kasus `Rejected`: jadwalkan, ubah, batal, tolak lagi; bangsal pesan ulang order yang sama | Integrasi | Semua `422 OPR-CASE-REJ-002`/`001`; kasus baru `Requested`; invoice tanpa biaya kasus ditolak |
| `FR-RWF-086` | Laporan Operasi periode berisi satu kasus ditolak dan satu dibatalkan | Integrasi | `RejectedCount = 1`, `CancelledCount = 1` |
| `FR-RWF-080` / `AC-RWF-080` | Pasien samaran dari poliklinik; kamar pulih menyimpan keputusan `Inpatient` | Integrasi **OK** | Permintaan `Pending`; tidak ada episode |
| `AC-RWF-088` | Admisi dari permintaan; bed ditempati; serah terima diterima | E2E | Permintaan `Completed` dengan episode; kasus OK `Completed`; tarif kamar sejak bed ditempati |
| `FR-RWF-089` / `AC-RWF-089` | Keputusan `Inpatient` untuk pasien yang sudah punya episode; OK membatalkan permintaan beralasan; admisi biasa untuk pasien yang punya permintaan `Pending` | Integrasi | `AdmissionReferralState = NotNeeded` tanpa baris baru; permintaan `Cancelled` hilang dari daftar; `409 INP-ADM-REF-001` |
| `INV-RWF-32` | Dua penyimpanan kamar pulih bersamaan untuk kasus yang sama | Integrasi konkurensi | Satu permintaan `Pending` (unique parsial) |
| `FR-RWF-081`, `082` / `AC-RWF-081`, `082` | Bangsal membuka ringkasan laporan final, lalu laporan draft | Integrasi + E2E | Isian klinis tampil tanpa tombol ubah; draft → "Laporan operasi belum final" |
| `FR-RWF-088` / `AC-RWF-087` | Serah terima belum diterima 61 menit dengan ambang 60 | Integrasi | Tampil di daftar pantau bangsal dan di daftar serah terima OK `overdueOnly` |
| `FR-RWF-087` / `AC-RWF-086` | Laporan 1–7 Okt berisi satu transfer dan satu koreksi; ekspor | Integrasi + E2E | Baris transfer dan baris bertanda "Koreksi"; audit ekspor tercatat tanpa nama pasien |
| `AC-RWF-100` | Pengguna berperan "Kepala Ruangan" tanpa permission laporan | Integrasi | `403` |
| `FR-RWF-087` | Periode 45 hari | Integrasi | `400` `VAL-RWF-90` |
| `FR-RWF-071` / `AC-RWF-071` | Transfer Melati → ICU; transfer di dalam Melati | Integrasi + E2E | Satu dokumen `NotSent` hanya untuk transfer antarunit; banner tampil di kedua unit |
| `FR-RWF-071` / `AC-RWF-072` | Perawat ICU menolak tanpa alasan, lalu dengan alasan; pengirim mengirim ulang | Integrasi | `400`; `Rejected`; `Sent` kembali |
| `INV-RWF-33` | Pembuatan dokumen serah terima gagal sesudah transfer | Integrasi | Transfer tetap tersimpan; Daftar Pantau menampilkan transfer tanpa dokumen |
| Regresi OK | Kasus dibuat petugas OK dengan tiga tindakan; alur OK lama tanpa pra-operasi bangsal | Regresi | `POST cases` tetap menerima banyak tindakan; kasus darurat tetap dapat memakai bypass darurat yang sudah ada |
| Regresi permission | Peran yang dulu memegang `OperatingRoomHandover : Update` | Regresi | Sesudah `E8`, peran itu memegang `Send`, **tidak** `Receive` |
| `RWI-AC-330` / `RWI-DEC-207` | Admisi dari permintaan untuk pasien Poli Bedah | Integrasi dengan Billing | Pesan `ADMISSION_CONFIRMED` hanya berisi field daftar putih; Billing membuat satu tautan kunjungan asal |
| `RWI-AC-331` / `RWI-DEC-220` butir 1 | Admisi biasa untuk pasien dengan permintaan `Pending` | Integrasi | `409 INP-ADM-REF-001` |
| `RWI-AC-332` / butir 2 | Pesan ruang bedah untuk episode `DischargePending` | Integrasi | `422 INP-SRG-002` |
| `RWI-AC-333` / butir 3 | Peran pemegang `OperatingRoomHandover : Update` lama sesudah `E8` | Regresi | Kirim boleh; terima `403` |
| `RWI-AC-334` / butir 4 | Laporan transfer 32 hari | Integrasi | `400` `VAL-RWF-90` |
| `RWI-AC-336` / butir 6 | Ambang daftar pantau diubah dari 60 ke 120 menit | Integrasi | Serah terima 61 menit tidak lagi tampil sampai lewat 120 menit |
| `RWI-AC-338` / `RWI-DEC-219` | Pemesanan Ruangan Bedah | E2E | Perkiraan tarif tindakan dari order dan keterangan komponen OK |
| `RWI-AC-340` / `RWI-DEC-214`, `215` | Sidebar Rawat Inap dengan dan tanpa permission laporan | E2E | Butir Laporan Rawat Inap hanya bagi pemegang permission; paling banyak sepuluh butir |

---

## 21. Amandemen kontrak `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.11.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`) |
| Data uji samaran | PRD bagian 18: Tn. Budi Santoso (RM `00-12-34-56`, asuransi PT Asuransi Sehat Sentosa, kartu `7788-0012-3456`, kelas 2, Melati 03 bed B), istri Ny. Rina Santoso; Ny. Wati (tunai); By. Ny. Rina (3 hari). Petugas: Sari (admisi), Dewi (CRO), Andi (perawat), Maya (kepala ruangan), Yudi (kasir), Hendra (supervisor) |
| Prasyarat lingkungan | Peran dan hak `RWI-OQ-124`; situs rumah sakit `IsMainSite`; kebijakan deposit asuransi kelas 2 minimum Rp 5.000.000, interval 3 hari; butir serah terima `STPB-*` |

Akibat di modul lain lulus hanya bila terbukti di modul penerima (`RWI-DEC-168`). Baris bertanda **gerbang** baru dapat dijalankan setelah persetujuan pemilik modul yang disebut.

### 21.1 Ruang kerja, header, dan kelengkapan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWA-001` / `RWI-AC-343` | Sari dan Andi membuka Detail Episode Budi; pengguna Gizi tanpa hak membuka Detail Episode lalu mengetik alamat ruang kerja | E2E | Tombol "Workspace PPRI" tepat sesudah Workspace Dokter bagi Sari dan Andi; tidak ada bagi pengguna Gizi; alamat langsung → "Akses Tidak Tersedia"; panggilan server `403` |
| `RWI-AC-344` | Membuka Workspace PPRI | E2E + pemantauan jaringan | Navigasi tanpa Assessment Edukasi dan tanpa MP Benefit; tidak ada panggilan `patient-assessments` |
| `FR-RWA-005` / `RWI-AC-345` | Budi (asuransi, kurang deposit, tanpa operasi) dan Wati (tunai, deposit cukup) | Integrasi + E2E | "0 dari 6" dan "0 dari 4"; General Consent, Privasi, Label tidak dihitung |
| `RWI-DEC-250` / `RWI-AC-371` | Budi punya kasus OK `Completed` tanpa Estimasi; Wati kasus OK `Rejected` tanpa penanda manual — dijalankan **saat `EPIC-RWA-09` dikirim** | Integrasi | Budi: "Estimasi Biaya belum dibuat"; Wati: Estimasi tidak wajib |
| `FR-RWA-006` / `RWI-AC-346` | Dokter memutuskan Budi pulang saat Selisih Biaya belum ditandatangani | Integrasi + E2E | Keputusan pulang tersimpan; Detail Episode "Dokumen admisi belum lengkap: 1 (Selisih Biaya)" |
| `FR-RWA-007` / `RWI-AC-358` | Episode Wati `Closed`; Sari mengubah Nilai Kepercayaan lewat panggilan langsung; mencetak ulang | Integrasi | `409 INP-ADM-DOC-001`; cetak ulang tanpa alasan `422 INP-ADM-PRT-001`, dengan alasan berhasil |
| G-30 | Episode `Draft` (admisi belum dikonfirmasi) | Integrasi | `summary.Availability = NotYetAdmitted`; tulis `409 INP-ADM-DOC-002`; `/letterhead` tetap `200` |
| `FR-RWA-008` | Layanan baca pasien dimatikan di lingkungan uji | E2E | "DATA PASIEN TIDAK DAPAT DIMUAT" + Coba Muat Ulang; tidak ada form |
| `RWI-AC-378` | Akun yang hanya memegang `InpatientAdmissionDocument : Read`, tanpa `Patient`, `PatientEncounter`, `BillingDeposit`, `HospitalSite : Read` | E2E + pemantauan jaringan | Nol penolakan `403`; header dan cetakan lengkap; respons calon penanda tangan hanya nama, hubungan, alamat |
| `RWI-AC-379` | Andi (tanpa `ViewAmount`) dan Sari | E2E | Andi: "Deposit: lihat kasir", tombol cetak Pelunasan Deposit tidak tampil, `/summary/amounts` `403`; Sari: "Deposit kurang Rp 3.000.000" |
| `RWI-DEC-260` / `RWI-AC-381` | Surat jatuh tempo Sabtu 10 Oktober 2026 11.00; waktu uji 11.01 dengan kekurangan masih ada | Integrasi (jam dipalsukan) | Peringatan di header (berupiah bagi `ViewAmount`) dan Detail Episode (tanpa rupiah); daftar pantau deposit tetap memakai ambang `DepositFollowUpIntervalDays` |
| `INV-RWA-14` | Telaah source | Statis | Tidak ada query Workspace PPRI ke `MstPatient*`, `RegPatientEncounter*`, `BilDeposit*`, `MstHospitalSite`, `CliDoctorCertificate`; seluruh bacaan lewat `InpAdmissionSourceReader` |

### 21.2 Fondasi dokumen: siklus, versi, tanda tangan, konkurensi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `RWI-AC-384` | Cetak Privasi saat `Draft`, saat `AwaitingSignature`, dan saat `Completed` | Integrasi + E2E | Penanda "KONSEP — BELUM DITANDATANGANI"; "Lembar untuk ditandatangani — versi 1" tanpa tanda konsep, isi dari salinan beku; final dengan catatan tanda tangan kertas |
| `RWI-DEC-263` / `RWI-AC-385` | Pelunasan Deposit dikunci saat kurang Rp 3.000.000; kasir menerima Rp 1.000.000; tanda tangan dicatat; cetak ulang | Integrasi dengan Billing | Dokumen dan cetakan tetap Rp 3.000.000, `AmountsReadAt` waktu kunci; header Rp 2.000.000 |
| `RWI-AC-351` | Maya menandatangani Kepala Ruangan; Sari mencetak | E2E | Cetakan "Ditandatangani secara elektronik oleh Maya …, Kepala Ruangan …, *tanggal jam*", bukan nama Sari |
| `RWI-AC-352` | Andi (tanpa `SignAsHeadNurse`) memanggil `signatures/head-nurse` | Integrasi | `403` |
| `RWI-AC-353` / `INV-RWA-04` | Sari menandatangani Admission lalu CRO; dua permintaan bersamaan dari akun yang sama ke dua slot | Integrasi + konkurensi | `422 INP-ADM-DOC-033`; slot CRO kosong; unique index menolak salah satu permintaan bersamaan |
| `RWI-DEC-239` penegasan | Sari memverifikasi tanda tangan kertas Rina lalu menandatangani slot Petugas PPRI Selisih Biaya | Integrasi | Keduanya berhasil |
| `RWI-AC-355` | Ubah dokumen `Completed`; versi koreksi tanpa alasan; dengan alasan | Integrasi + E2E | `409 INP-ADM-DOC-005`; `400`; versi 1 `Superseded` dan versi 2 `Draft`, Riwayat menampilkan keduanya, kelengkapan tetap satu |
| `RWI-AC-356` | Andi membatalkan; Hendra membatalkan dengan "salah"; mencari `DELETE` | Integrasi + statis | `403`; `400` "Alasan pembatalan minimal 10 karakter"; tidak ada `DELETE` pada controller |
| `RWI-AC-357` / `INV-RWA-01` | Membuat Privasi kedua saat yang pertama `Draft`, `AwaitingSignature`, `Completed`; dua simpan konsep pertama bersamaan | Integrasi + konkurensi | `409 INP-ADM-DOC-003`; satu dokumen tersimpan (unique index bersaring) |
| `FR-RWA-128` / UAT-RWA-26 | Sari dan Hendra mengubah konsep yang sama; Hendra menyimpan dari layar lama | Integrasi + E2E | `409 INP-ADM-DOC-004`; perubahan Sari utuh |
| `FR-RWA-128` | Klik Simpan dua kali dengan `Idempotency-Key` sama; klik cetak dua kali | Integrasi | Satu dokumen dan satu log; kedua respons sama |
| `RWI-DEC-240` butir 2 | Buka kunci tanpa tanda tangan; buka kunci sesudah satu tanda tangan | Integrasi | Kembali `Draft` dengan salinan beku kosong; `409 INP-ADM-DOC-006` |
| `VAL-RWA-08` | Andi (pemegang `Update` tetapi bukan pembuat) membuang konsep Sari | Integrasi | `422 INP-ADM-DOC-008` |
| `RWI-AC-369` / `RWI-DEC-229` | Serah Terima sampai `Completed` | Integrasi + statis | Nol baris baru `MrcClinicalDocumentIntegrity`; enum `ClinicalDocumentKind` tidak berubah |
| `VAL-RWA-35` | Waktu tanda tangan kertas sebelum dokumen dikunci; 10 menit di masa depan | Integrasi | `400` |
| `RWI-AC-368` / `RWI-DEC-247` | Ganti kode formulir Selisih Biaya di `FE-INP-12`; cetak; cari source Workspace PPRI | Integrasi + statis | Cetakan berikutnya memakai kode baru; pencarian nama rumah sakit client, nama kota bawaan V1, dan pola kode formulir pada berkas service, controller, DTO, dan komponen cetak Workspace PPRI = nol hasil (seeder dikecualikan) |

### 21.3 Serah Terima Pasien Baru

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWA-030` / UAT-RWA-05 | Gelang dicetak 09.50, IPD 09.55; Sari mengonfirmasi 18 butir, kunci, tanda tangan 10.05; Dewi 10.20; Andi 10.40 sesudah Budi menempati bed 10.35 | E2E tiga akun | Saran tampil pada butir 9 dan 13; setiap tanda tangan tampil di layar lain ≤ 30 detik; `Completed` 10.40; kelengkapan naik satu |
| `RWI-AC-354` | Dewi membuka sebelum dikunci | E2E | Tanpa tombol tanda tangan; "Data serah terima belum dikirim oleh petugas admisi" |
| `RWI-AC-359` | Butir 5 belum dipilih dan butir 11 Belum tanpa keterangan; kunci | Integrasi + E2E | Satu penolakan `422` berisi dua pesan bernomor; status tetap `Draft` |
| `RWI-AC-360` | Admin mengganti nama butir 14 menjadi "Input Kartu Parkir"; buka dan cetak serah terima lama | Integrasi | Dokumen lama tetap "INPUT PARKIR" |
| `RWI-AC-361` / `INV-RWA-12` | Daftar periksa penutupan episode sesudah 18 butir serah terima aktif; tandai butir serah terima pada penutupan | Integrasi + regresi | Penutupan hanya memuat butir `EpisodeClosure`; penandaan butir `STPB-*` → `404`; episode dapat ditutup |
| `RWI-AC-376` / `RWI-DEC-255` | Andi menandatangani saat bed masih dipesan; sesudah penempatan | Integrasi + E2E | `422 INP-ADM-DOC-034` "Pasien belum menempati tempat tidur", lalu berhasil; tanda tangan CRO tidak terpengaruh |
| `RWI-AC-383` / `RWI-DEC-262` | Budi punya surat pengantar `Issued` dr. Andika; Wati tanpa surat; pasien lain hanya punya surat `Cancelled` | Integrasi | Butir 1 bersaran "Sudah — saran sistem (surat pengantar dr. Andika, 07-10-2026)" hanya untuk Budi; tetap wajib dipilih |
| `RWI-DEC-241` butir 2 | General Consent dicetak | Integrasi | Butir 12 **tanpa** saran selama *fail-closed* |

### 21.4 Gelang, label, IPD

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWA-050` / `RWI-AC-363` | Budi (pria 45 th); By. Ny. Rina (`IsNewborn`); anak 4 th; wanita 30 th status nikah `Unknown` | Integrasi | "BUDI SANTOSO, Tn." Gelang Dewasa; "BY. NY. RINA" Gelang Bayi + dua label kecil; Gelang Bayi bersapaan "An."; Gelang Dewasa tanpa sapaan |
| `RWI-AC-364`, `380` / `RWI-DEC-259` | Cetak gelang dan label; pasien lama tanpa berkas `QrCodePath` | Integrasi + pemindaian | QR = "00-12-34-56" sama dengan payload QR pasien; tanpa nama, tanggal lahir, ID acak; pasien lama tetap tercetak |
| `RWI-AC-374` / `RWI-DEC-253` | Label pasien asuransi dengan kartu kosong dan nomor peserta terisi; label Wati tunai | Integrasi | Baris "No. Kartu" kosong pada keduanya; tidak ada nomor acak |
| `FR-RWA-053` / UAT-RWA-12 | Cetak ulang gelang tanpa alasan, lalu "rusak" | Integrasi + E2E | `422 INP-ADM-PRT-001`; log "Cetakan ke-2, rusak, oleh Andi" |
| `VAL-RWA-45` | Catat cetak Gelang Dewasa untuk bayi baru lahir | Integrasi | `422 INP-ADM-PRT-005` |
| `RWI-AC-365` | Cetak IPD Budi | E2E | Garis kosong untuk pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat domisili, alamat kantor, no. mutasi, persetujuan direktur, perhatian khusus, kasir; tidak ada isian ketik |
| `RWI-AC-375` / `RWI-DEC-254` | IPD dengan surat `Issued`; dengan surat `Cancelled` saja; tanpa surat dan tanpa perujuk luar | Integrasi | Diagnosis, rencana, dokter dari surat; surat batal diabaikan; garis kosong |
| `RWI-AC-386` / **gerbang** `RWI-OQ-128` | IPD Wati yang punya dokter perujuk luar, sebelum dan sesudah service Registration tersedia | Integrasi | Sebelum: garis kosong, cetak tidak gagal. Sesudah: dokter perujuk dan institusinya |
| `RWI-AC-387` / **gerbang** `RWI-OQ-129` | IPD dibuka pemegang `ViewAmount` sebelum dan sesudah method tarif Billing tersedia | Integrasi | Sebelum: "lihat kasir" bagi semua. Sesudah: tarif kamar per hari menurut penjamin; tanpa `ViewAmount` tetap "lihat kasir" |
| `FR-RWA-071` / UAT-RWA-15 | Nilai Kepercayaan dan Privasi Budi `Completed`; cetak IPD | E2E | Isian terisi dari dokumen; log cetak bertambah; butir 9 serah terima bersaran |
| `FR-RWA-072` / UAT-RWA-16 | Layanan episode dimatikan sementara | E2E | Cetak nonaktif "Cetak ditahan sampai data wajib terbaca lengkap" + Coba Lagi |

### 21.5 Privasi, Nilai Kepercayaan, Selisih Biaya

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWA-061` / UAT-RWA-13 | Kerabat "Ny. Rina Santoso" dan "Sdr. Dimas, Jr."; permintaan khusus; transportasi Ya; tanda tangan kertas Budi; Maya; tutup dan buka lagi | E2E | Dokumen `Completed` tampil lagi; "Sdr. Dimas, Jr." satu baris; header ringkasan privasi |
| `VAL-RWA-15` | Kerabat keempat | Integrasi | `400` "Paling banyak 3 kerabat." |
| `FR-RWA-112` | Pengguna `InpatientEpisode : Read` membaca `/patient-rights` | Integrasi | Ringkasan Nilai Kepercayaan dan Privasi `Completed` saja; versi `Superseded` tidak ikut |
| `RWI-AC-362` / `RWI-DEC-242` | Episode baru Januari 2027; butir ke-6; kunci tanpa butir | Integrasi + E2E | Dua butir dari dokumen lalu sebagai `Draft`; `400` "Maksimal 5 butir"; `422` "Minimal satu hal yang bertentangan wajib diisi" |
| `FR-RWA-101` / UAT-RWA-21 | Subjek "istri saya", deklarer Rina, KTP, HP `081234567890`; Sari menandatangani Petugas PPRI | E2E | Data pasien hanya-baca; subjek tersimpan `Wife`; cetakan dwibahasa dengan kop dari profil |
| `RWI-AC-377` / `RWI-DEC-256` | Penjamin bertanda `IsAllowExcessPaymentByPatient = false` | Integrasi | Selisih Biaya tetap wajib; cetakan memuat ketiga butir |
| UAT-RWA-22 / `VAL-RWA-10`, `14` | Selisih Biaya untuk Wati; HP 14 digit | Integrasi + E2E | Lencana "Tidak diperlukan", `422 INP-ADM-DOC-010`; `400` "Nomor telepon maksimal 13 digit" |
| G-43 | Penjamin Budi berubah menjadi tunai sesudah Selisih Biaya `Completed` | Integrasi | Dokumen tetap tersimpan; kelengkapan tidak lagi mewajibkannya |

### 21.6 Pelunasan Deposit

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `RWI-AC-366` / UAT-RWA-17 | Minimum Rp 5.000.000, masuk Rp 2.000.000; surat Jumat 9 Oktober 2026 | Integrasi dengan Billing + E2E | "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)"; jatuh tempo bawaan Senin 12 Oktober 11.00 WIB |
| `RWI-AC-366` / UAT-RWA-18 | Wati deposit cukup | E2E | "Deposit episode ini sudah memenuhi kebijakan…", Simpan nonaktif; panggilan langsung `422 INP-ADM-DOC-011` |
| `RWI-AC-367`, `382` | Jatuh tempo 13 Oktober dengan interval 3; interval 1; surat Kamis 8 Oktober interval 3; interval 0 | Unit (`InpDepositDueDateCalculator`) + integrasi | `422 INP-ADM-DOC-019`; Sabtu 10 Oktober 11.00; Jumat 9 Oktober 11.00; tanggal surat |
| G-45 / `VAL-RWA-12` | Layanan deposit Billing dimatikan | Integrasi + E2E | Angka tidak tampil; simpan dan kunci `422 INP-ADM-DOC-012`; tidak ada isian angka |
| `RWI-DEC-252` / `RWI-AC-373` | Data Wali: pilih relasi `Spouse`; kontak darurat bertuliskan "isteri"; dua relasi `Child` | Integrasi + E2E | Relasi terisi otomatis; kontak darurat hanya di daftar pilihan; dua anak → petugas memilih |
| `AdmissionPartyInput` | Klien mengirim nama berbeda untuk sumber `PatientRelationship` | Integrasi | Server menyimpan nama dari service pemilik, bukan isian klien |

### 21.7 General Consent cetak saja dan tombol Cetak Persetujuan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `RWI-AC-347` | Membuka kedua tab dan mencetak | E2E + pemantauan jaringan | Lencana "Cetak saja"; tidak ada tombol Simpan; nol permintaan tulis |
| `RWI-AC-348`, `372` / `RWI-DEC-251` | Hubungan "Istri"; relasi tidak ditemukan; kamar "Melati Khusus" tanpa penanda; bed `IsIntensiveCareBed`; episode `RequiresIsolation`; kelas `IsForIntensiveCare` di bed biasa | Integrasi + E2E | Nama dan alamat dari relasi; isian terbuka berketerangan; "Umum"; "Khusus"; "Khusus"; "Umum" |
| `RWI-AC-349` / `RWI-DEC-246` | Sesudah `RWA-MVP-1`: Sari menekan Cetak Persetujuan; pengguna yang hanya `InpatientEpisode : Read`; tautan lama `consent-print` | E2E | Tiba di `FE-INP-36` tab Surat Persetujuan; tombol tidak tampil; tautan lama dialihkan |
| `PPRI-CAP-09` | Langkah 8 alur admisi untuk episode `Draft` | E2E | Kop dari profil rumah sakit, bukan nilai yang ditanam |

### 21.8 Master dan migration

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `E9` | Migration pada salinan basis data uji berisi butir penutupan lama | Migration | Butir lama `ChecklistType = 1`, tanpa induk, tanpa saran; pengaturan lama `InfantWristbandMaxAgeYears = 5`; penutupan episode lama tetap sama |
| `E9` mundur | Nonaktifkan butir `STPB-*`, mundur kode | Regresi | Penutupan episode tidak tertahan butir serah terima |
| `VAL-RWA-50` s.d. `53` | Induk berjenis lain; butir penutupan dengan sumber saran; ubah jenis butir terpakai; dua butir dengan sumber saran sama | Integrasi | `MST-ICI-001` s.d. `004` |
| `VAL-RWA-54` | Batas umur gelang bayi 17 | Integrasi | `400 MST-IST-001` |
| Registry hak akses | Aplikasi dinyalakan sesudah kode dirilis | Integrasi | `InpatientAdmissionDocument` beserta sepuluh aksinya terdaftar, aksi kustom terkelompok sesuai 13.9 |
| `RWI-DEC-048` | Seeder dijalankan dengan lingkungan `Production` | Unit | Seeder menolak; tidak ada butir `STPB-*` tertulis |

### 21.9 Yang belum diuji pada amandemen ini

| Butir | Sebab |
|---|---|
| Estimasi Biaya, penanda rencana tindakan, baris visit dokter | `EPIC-RWA-09` di luar gelombang (`DEC-INP-020`) |
| Penyimpanan General Consent, tanda tangan digital | `DEC-INP-003` |
| Peringatan nilai kepercayaan di Workspace Keperawatan dan Dokter | Amandemen terpisah (PRD bagian 8); gerbang produksi G-42 |
| Ukuran kertas gelang dan label pada printer rumah sakit | Diuji saat UAT di lokasi (G-37) |
| Masa simpan | Gerbang produksi G-35 |

## 22. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

### 22.1 Coverage seluruh acceptance criteria

Semua test di bawah **rancangan NOT_RUN**, bukan bukti hasil. Fixture actor/class/SOP adalah data uji terkontrol, bukan bukti role/order/SOP rumah sakit actual. AC source berada di decision log revision46; original audit 65 frontend tests passed hanya baseline as-is.

| Test ID | AC | Epic | Kasus | Jalur berhasil / expected | Jalur gagal / expected | Lapisan | Hasil target |
| --- | --- | --- | --- | --- | --- | --- | --- |
| BM-AT-396 | RWI-AC-396 | EPIC BM-01 | Menu rename | Sidebar menampilkan Bed Management di route lama | Nama lama/dua leaf → gagal | FE | NOT_RUN |
| BM-AT-397 | RWI-AC-397 | EPIC BM-01 | Tiga tab | Monitoring/Transfer/History reachable sesuai hak; child cleaning dari monitoring | Direct forbidden tab tidak fetch data | FE/API | NOT_RUN |
| BM-AT-398 | RWI-AC-398 | EPIC BM-01 | Enam status | Fixture enam keadaan menghasilkan tepat enam label/counts | Unverified/dirty tidak Available | Unit/API | NOT_RUN |
| BM-AT-399 | RWI-AC-399 | EPIC BM-05 | Origin auto | Episode A aktif di bed A, asal otomatis bed A | Client mengirim asal lama setelah transfer lain →409 | API/FE | NOT_RUN |
| BM-AT-400 | RWI-AC-400 | EPIC BM-05 | Target available | Bed B ready terpilih dan commit recheck | Target direbut reserved pasien lain sebelum commit →409 no transfer | PostgreSQL | NOT_RUN |
| BM-AT-401 | RWI-AC-401 | EPIC BM-05 | Category manual | Manual UpGrade disimpan pada destination snapshot | Field missing/default otomatis →400/UI gagal | API/FE | NOT_RUN |
| BM-AT-402 | RWI-AC-402 | EPIC BM-06 | No-transfer history | Episode tanpa transfer tetap satu segmen initial | History hanya transfer report → gagal | Query/API | NOT_RUN |
| BM-AT-403 | RWI-AC-403 | EPIC BM-05 | Same Grade | Pindah bed beda dengan valid class ID sama dan manual3 berhasil | Target same physical bed ditolak | API | NOT_RUN |
| BM-AT-404 | RWI-AC-404 | EPIC BM-05 | Mismatch | Pilihan sesuai official comparison diterima | Up secara resmi tapi pilih Down →422 source utuh | API/PostgreSQL | NOT_RUN |
| BM-AT-405 | RWI-AC-405 | EPIC BM-05 | Global official order | Fixture proof sah X di atas Y sama di dua unit | Harga/nama/unit mengubah classification →gagal | Unit/API | NOT_RUN |
| BM-AT-406 | RWI-AC-406 | EPIC BM-03 | HK start/finish | HK-TEST individu start/complete; actor/time per aksi | Akun tanpa scope/hak →403, no event | API | NOT_RUN |
| BM-AT-407 | RWI-AC-407 | EPIC BM-02 | Used release | A keluar10.00→Waiting, B pesan10.01 ditolak | Used release →Available langsung →gagal | Integration | NOT_RUN |
| BM-AT-408 | RWI-AC-408 | EPIC BM-02 | Unused cancel/expiry | Unused reservation lepas tanpa cleaning baru | Cancellation membuka closed/invalid bed →gagal | API | NOT_RUN |
| BM-AT-409 | RWI-AC-409 | EPIC BM-03 | Verifier | Selesai10.20 belum ready; perawat sah10.25 ready | HK complete langsung ready / unauthorized verify →gagal | API | NOT_RUN |
| BM-AT-410 | RWI-AC-410 | EPIC BM-03 | Subphase | AwaitingVerification label Dalam Pembersihan + Menunggu verifikasi | Status publik ketujuh atau available →gagal | Unit/FE | NOT_RUN |
| BM-AT-411 | RWI-AC-411 | EPIC BM-03 | Reject readiness | Reason ada→Waiting; attempt rejected retained | Reason blank→400/422 no state change | API | NOT_RUN |
| BM-AT-412 | RWI-AC-412 | EPIC BM-04 | All master paths | Empty close reason→Unavailable; reopen→not Ready | Occupied/reserved close via POST/PUT/status/availability/delete/activation adapter →409 | API/PostgreSQL | NOT_RUN |
| BM-AT-413 | RWI-AC-413 | EPIC BM-02 | TTL server | Default reserve120m, server evaluate at expiry | Browser timer release sendiri / worker/reminder baru →gagal | Clock/API/FE | NOT_RUN |
| BM-AT-414 | RWI-AC-414 | EPIC BM-05 | Atomic transfer | Origin released Waiting, dest occupied satu commit | Inject failure setelah satu write→full rollback; handover failure after commit no reverse | PostgreSQL | NOT_RUN |
| BM-AT-415 | RWI-AC-415 | EPIC BM-06 | History+snapshot+versions | Initial, transfer, ongoing nullend dan corrections tampil; master rename tidak relabel snapshot | Filter !IsSuperseded menghilangkan transfer biasa →gagal | Query/Billing integration | NOT_RUN |
| BM-AT-416 | RWI-AC-416 | EPIC BM-01 | HK privacy | Monitoring JSON HK hanya data operasional | Patient/episode/reservation identifier/diagnosis/history API terlihat→gagal | API security | NOT_RUN |
| BM-AT-417 | RWI-AC-417 | EPIC BM-01 | Permission+scope | Authorized scope actions visible and work | Unknown unit/grant/direct request →403; no counts leak | API/FE | NOT_RUN |
| BM-AT-418 | RWI-AC-418 | EPIC BM-06 | Reasons+correction | Cancel reason persisted; correction versioned Billing OPEN | Blank cancel or Billing closed correction →reject; no delete | API/Billing | NOT_RUN |
| BM-AT-419 | RWI-AC-419 | EPIC BM-02 | Concurrency | Dua request bersamaan hanya satu holder sah | Reserve vs placement/transfer/close/cleaning cross-table double holder→gagal | PostgreSQL concurrent | NOT_RUN |
| BM-AT-420 | RWI-AC-420 | EPIC BM-01 | Fresh confirmation/counts | Await refetch selesai; nonactive/nonreservable/invalid not count Available | Deferred stale response closure dipakai confirm atau stale commit lolos→gagal | FE/API | NOT_RUN |
| BM-AT-421 | RWI-AC-421 | EPIC BM-02 | Idempotency+cycle | Same key/payload single commit, receipt reused | Changed input samekey→409; old attempt verify siklus baru→409 | PostgreSQL/API | NOT_RUN |
| BM-AT-422 | RWI-AC-422 | EPIC BM-02 | Downtime/uncertain | Timeout aftercommit→lookup own receipt; no fake success; reconcile authorized audit | Offline cache overwrite/keybaru/auto reverse callback→gagal | Fault injection/FE | NOT_RUN |
| BM-AT-423 | RWI-AC-423 | EPIC BM-05 | Unverified class | Proof matching allows comparison; same valid class ID Same | Distinct IDs default0/equal numeric no proof →422 | Unit/API | NOT_RUN |
| BM-AT-424 | RWI-AC-424 | EPIC BM-04 | Data conflict | Active patient remains Occupied+conflict, invalid empty Unavailable | Mismatched raw/master treated empty Available →gagal | Query/API | NOT_RUN |
| BM-AT-425 | RWI-AC-425 | EPIC BM-03 | Activation scoped | Proof gates recorded; only dependent operations held | Unverified SOP/assignment/class treated production ready→gagal | Config/security/UAT | NOT_RUN |
| BM-AT-426 | RWI-AC-426 | EPIC BM-02 | Old closure | A leaves, clean+verify, B occupies, close A13.00 bed B untouched | Closure A releases B or version root changes incorrectly→gagal | Integration/PostgreSQL | NOT_RUN |

### 22.2 PostgreSQL transaction dan interleaving wajib

Gunakan PostgreSQL disposable yang sama provider/schema dengan target; EF InMemory/SQLite tidak membuktikan row lock/partial index. Jalankan minimal reserve-vs-reserve, reserve-vs-place, reserve-vs-transfer, place-vs-place, transfer-vs-transfer, transfer-vs-close, release-vs-reserve, cleaning-verify-vs-reserve/closure dan correction-vs-transfer. Barrier test sengaja menahan writer sesudah lock untuk memaksa interleaving. Assert di dua tabel: tidak ada holder beda episode bersamaan; master/root proyeksi konsisten; tidak ada event/receipt separuh; versi bertambah tepat untuk commit.

Inject failure setelah end source sebelum destination/create audit/outbox/receipt, lalu assert rollback seluruhnya. Timeout setelah commit lalu lookup/retry same key → receipt/result sama tanpa transfer/event kedua. Key sama hash beda→409; stale verification setelah release baru→409. Assertion pada audit actor/time, cycle dan immutable snapshot, bukan hanya HTTP200.

### 22.3 API/security/contract/frontend

Semua 38 route inventory diuji menurut status existing/new; metode unchanged tetap regression. Tambahan header/DTO pada old consumers yang belum diperbarui harus fail400 terkendali, bukan silent old behavior. Uji HK profile dengan broad permission fixture untuk memastikan OperationalOnly tetap menolak patient/episode/reservation fields dan history; jangan hanya memakai field-hide FE. Uji unknown assignment deny, unit counters scoped, denied direct query dan noPHI in logger.

FE deferred response test memaksa refetch pertama terlambat, memastikan konfirmasi memakai returned fresh context dan stale response diabaikan. Double click/timeout stable key; online recovery no cache overwrite; modal focus/keyboard/tab roles, ongoing end=NULL, wrong category preserves input. Consumer admission/detail/master/departure/correction ikut contract regression.

### 22.4 Migration, data dan integrasi

Review generated EF Configuration/migration sesuai data dictionary; PostgreSQL dry run fresh+legacy snapshots, orphan/duplicate/cross-holder report, no fake Ready from Available, no fake category/reasons/actors. Exercise cutover semua writer dan safe rollback retaining guards/schema/history. Outbox/receiver tests memakai Billing canonical1.1.0, ordinary transfer IsSuperseded=true tetap efektif bila SupersededByCorrectionId NULL. Handover callback fail tidak reverse; old episode closure setelah bed ditempati pasien baru tetap aman.

### 22.5 UAT

| ID | Epic | Jalur | Kondisi awal | Langkah | Hasil |
| --- | --- | --- | --- | --- | --- |
| UAT-701 | EPIC BM-01 | Berhasil | Fixture dua unit dengan enam status; actor berhak satu unit | Buka menu dan Monitoring, filter unit/kamar | Satu leaf, tiga tab, enam counts sesuai bed dalam scope; HK tidak menerima identitas |
| UAT-702 | EPIC BM-01 | Gagal | Actor hanya baca unit A; respons lama ditunda | Deep-link unit B/Transfer tanpa hak; buka confirm selama refetch | 403/akses ditolak tanpa data bocor; confirm pending disabled, no stale result |
| UAT-703 | EPIC BM-02 | Berhasil | Bed A Ready, episode sah; unused reservation lalu patient ditempatkan | Reserve120m/cancel beralasan; place; record departure | Cancel reason tersimpan tanpa dirty baru; sesudah used release Waiting, no immediate reserve |
| UAT-704 | EPIC BM-02 | Gagal | Dua petugas/sessions bersaing pada bed Ready | Reserve episode A versus place episode B; simulasi timeout lalu retry same key | Satu holder sah; unknown diperiksa, retry tidak menggandakan; old closure tidak lepas new patient |
| UAT-705 | EPIC BM-03 | Berhasil | Bed Waiting; HK dan perawat fixture punya proof assignment/SOP | HK mulai/selesai; perawat inspeksi dan sahkan | Selesai menunggu verifikasi; baru Ready setelah pengesahan, actor/time traced |
| UAT-706 | EPIC BM-03 | Gagal | Bed AwaitingVerification atau siklus sudah baru | HK coba sahkan; verifier reject tanpa alasan; attempt lama coba verify | HK403; blankreason reject; stalecycle409; rejection valid menjaga jejak dan kembali Waiting |
| UAT-707 | EPIC BM-04 | Berhasil | Bed kosong tanpa reservation | Admin close alasan, lalu reopen | Close Unavailable; reopen belum Available sampai readiness sah |
| UAT-708 | EPIC BM-04 | Gagal | Bed terisi atau reserved; master data raw konflik | Coba nonactive/status/PUT/delete/hierarchy availability writer | 409 no closure; pasien aktif tetap terlihat+flag; invalid tidak bookable |
| UAT-709 | EPIC BM-05 | Berhasil | Source current, destination Ready, official class proof fixture | Asal otomatis; manual Same/Up/Down sesuai fixture; confirm refreshed | Satu transfer commit, kategori+snapshot, asal Waiting, tujuan Occupied; handover aftercommit |
| UAT-710 | EPIC BM-05 | Gagal | Source current, tujuan diambil actor lain atau order proof tidak ada | Pilih wrongcategory/stale target/default0 bedaID; inject DB failure | 409/422, source/dest/history utuh; tidak infer harga/nama, rollback lengkap |
| UAT-711 | EPIC BM-06 | Berhasil | Satu episode tanpa transfer, satu transfer dengan correction valid | History bed/periode, buka versions; master rename kemudian refresh | Semua overlap segments termasuk initial/ongoing/correction; snapshot lama tetap; Billing flag tepat |
| UAT-712 | EPIC BM-06 | Gagal | HK OperationalOnly / actor noepisode rights; Billing CLOSED | Coba history/directAPI identity; coba koreksi/delete committed transfer | HK403; identity masked bagi viewer tanpa right; closed correction rejected; no history delete |

### 22.6 Bukti sign-off

Unit/API/PG/UI/UAT hasil target, schema migration applied evidence, master actual proof BM-G01, SOP/assignment BM-G02, security/privacy scope BM-G03 serta repairs BM-G04 semuanya belum tersedia. Laporan dokumentasi sendiri tidak menggantikan itu. Tidak menjalankan aplikasi/test target atau database pada desain ini.
