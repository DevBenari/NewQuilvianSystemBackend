# Integration Contract — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | Mengikuti set kontrak pada [manifest](../blueprint-manifest.md), amandemen Bed Management draft. Riwayat metadata sebelumnya: **`0.11.0`** — bagian 10 Workspace PPRI (`INT-RWA-01` s.d. `14`), `approved` 2026-10-08 (`RWI-DEC-265`). Sebelumnya `0.10.0` — bagian 9, `approved` (`RWI-DEC-221`); `0.9.0` — bagian 8; isi sebelumnya `last_changed_in` `0.4.0` |
| Status | **`draft`** — amandemen Bed Management belum disetujui. Riwayat metadata sebelumnya: **`draft`** |
| Owner | Produk/domain/API Muhammad Hamzah; keamanan/privasi OPEN; frontend sesuai DEC-292. Riwayat metadata sebelumnya: Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | Decision 46; gate 1.12 / BM-RCG-20261010-01; audit BM-AUD-20261010-01 rev1. Riwayat metadata sebelumnya: `evidence/03-hospital-domain-architecture.md` revision `0.1` bagian J; `00-interview-decisions.md` revision `6` |
| Backend SHA | Bed Management: d4e1eca06fb28c05934c68c1e51a4dca01935a10. Riwayat metadata: `5afb54b` |
| Dampak kompatibilitas | Bed Management: GET aditif, mutation diperketat (expected version/key/category/reason); cutover seluruh konsumen wajib. Riwayat metadata sebelumnya: Satu arah tulis lintas modul yang baru. Tidak ada kontrak eksternal yang berubah |

> **`0.3.0` sengaja tidak menambah satu integrasi pun.** `RWI-DEC-066` menolak menambah kolom
> "boleh campur" pada `MstRoom`, sehingga aturan pencampuran kamar dijalankan dengan **membaca**
> penghuni yang sedang ada — data milik Rawat Inap sendiri. Tidak ada arah tulis baru ke modul lain,
> dan janji "nol perubahan kolom pada tabel modul lain" tetap utuh. Yang naik hanyalah
> `contract_version`, supaya seluruh kontrak tetap sebaris.

**Modul ini tidak memanggil satu pun sistem di luar aplikasi Quilvian pada revisi ini.** Seluruh
integrasi yang dibahas di sini bersifat internal, yaitu antar modul di dalam satu aplikasi dan satu
database. Alasannya ada di bagian 4.

---

## 1. Integrasi internal — arah baca

| ID | Produsen | Konsumen | Tujuan bisnis | Sumber kebenaran | Sifat |
| --- | --- | --- | --- | --- | --- |
| `INT-INP-01` | `RegistrationManagement` | Rawat Inap | Kunjungan sebagai jangkar episode | Registrasi | Sinkron, baca langsung lewat `ApplicationDbContext` |
| `INT-INP-02` | `MasterData` HealthServices | Rawat Inap | Tempat tidur, kamar, unit layanan, kelas pasien | Master Data | Sinkron, baca langsung |
| `INT-INP-04` | `Corporate/HumanResource` | Rawat Inap | Dokter untuk DPJP, pegawai untuk perawat | HR Workforce | Sinkron, baca langsung |
| `INT-INP-05` | `PatientManagement` | Rawat Inap | Identitas pasien untuk census dan resume | Patient Management | Sinkron, baca lewat kunjungan |
| `INT-INP-06` | `EmergencyInstallationManagement` | Rawat Inap | Waktu pasien tiba di bangsal, dari event `Tiba` pada catatan kepergian IGD | **IGD** | Sinkron, baca langsung. Hanya pada jalur serah terima IGD |
| `INT-INP-07` | `RegistrationManagement` | Rawat Inap | Rangkaian kedatangan, lewat `TrxPatientEncounter.OriginEncounterId` | Registrasi | Sinkron, baca langsung. Kolomnya dibuat dan diisi modul IGD |

**Dua integrasi terakhir ditambahkan pada `0.4.0`** lewat `RWI-DEC-072` dan `RWI-DEC-073`.
Keduanya arah **baca**, keduanya hanya menyala pada jalur serah terima IGD, dan jalur itu adalah
`INP-S09` yang di luar scope revisi ini. Tidak satu pun kolom milik modul lain ditulis atau
dibuat oleh Rawat Inap karena keduanya.

Keenamnya **tidak** menyalin data. Yang disimpan modul ini hanya Id-nya, dan nama ditampilkan
lewat `Include` atau projection saat query.

Satu pengecualian yang disengaja: `InpBedPlacement` menyimpan salinan `RoomId`, `ServiceUnitId`,
dan `PatientClassId` **pada saat penempatan dibuat**. Ini bukan duplikasi master, melainkan
**rekaman keadaan pada satu titik waktu**. Kalau kamar dipindahkan ke kelas lain tahun depan,
riwayat tahun ini tetap menunjukkan kelas yang benar-benar berlaku saat itu — dan itulah yang
membuat `RWI-RULE-007` dapat dijalankan.

---

## 2. Integrasi internal — arah tulis

### `INT-INP-03` — Menuliskan salinan status ketersediaan tempat tidur

Ini **satu-satunya** arah tulis modul ini ke luar batasnya sendiri.

| Aspek | Ketetapannya |
| --- | --- |
| Produsen | Rawat Inap |
| Konsumen | `MasterData` HealthServices, kolom `MstBed.BedStatus` |
| Tujuan bisnis | Menjaga agar seluruh pembaca lama — daftar bed, ringkasan, isian pilihan, dan layar master di frontend — tetap bekerja tanpa diubah |
| Sumber kebenaran | Rawat Inap untuk **maknanya**; Master Data untuk **kolomnya** |
| Nilai yang boleh ditulis | Hanya `Available`, `Reserved`, dan `Occupied` |
| Nilai yang **tidak** boleh disentuh Rawat Inap | `Cleaning`, `Maintenance`, `Blocked`, `Inactive` — tetap wewenang admin master data |
| Sifat | Sinkron, **di dalam transaksi yang sama** dengan perubahan catatan penempatan |
| Idempotency | Penulisan bersifat menetapkan nilai, bukan menambah. Mengulang operasi yang sama menghasilkan keadaan yang sama |
| Bila gagal | Seluruh transaksi dibatalkan. Tidak ada keadaan catatan penempatan berubah tetapi kolom status tidak, atau sebaliknya |
| Rekonsiliasi | Laporan selisih `GET /monitoring/bed-drift`, lihat bagian 3 |
| Persetujuan | Pemilik `MasterData` HealthServices, tercatat sebagai `RWI-OQ-033` — **sudah diberikan** 21 Agustus 2026 lewat `RWI-DEC-062`. Diterapkan `BE-RWI-006` pada 1 September 2026 |

**Kapan penulisan terjadi:**

| Tindakan Rawat Inap | `MstBed.BedStatus` menjadi |
| --- | --- |
| Pemesanan dibuat | `Reserved` |
| Pemesanan dibatalkan | `Available` |
| Pemesanan gugur, terbaca saat query | `Available` |
| **Kepergian fisik pasien dicatat** | `Available` |
| Pasien ditempatkan | `Occupied` |
| Pasien pindah — tempat tidur lama | `Available` |
| Pasien pindah — tempat tidur tujuan | `Occupied` |
| Admisi dibatalkan | `Available` |
| Episode ditutup | `Available` |

**Satu hal yang tidak dilakukan modul ini:** menimpa `Cleaning`, `Maintenance`, atau `Blocked`
menjadi `Available`. Bila tempat tidur dilepas sementara statusnya sedang `Maintenance` karena
disetel admin, nilai itu **dibiarkan apa adanya** dan tidak dikembalikan ke `Available`. Tempat
tidur yang sedang diperbaiki memang tidak boleh langsung dipakai pasien berikutnya.

---

## 3. Rekonsiliasi — laporan selisih tempat tidur

Karena salinan dan sumbernya berada di dua modul yang berbeda, selisih tetap mungkin terjadi —
misalnya bila kelak ada jalur lain yang menyetel kolom itu, atau bila data lama sudah terlanjur
salah sebelum modul ini dipasang.

Laporan selisih adalah **bagian dari kontrak integrasi**, bukan fitur tambahan yang boleh ditunda.

| Jenis selisih | Cara mengenalinya | Artinya bagi pengguna |
| --- | --- | --- |
| Tempat tidur terbaca kosong padahal ada penghuni | `BedStatus = Available` tetapi ada `InpBedPlacement` dengan `EndDateTime` kosong | Berbahaya. Tempat tidur bisa diberikan ke pasien kedua |
| Tempat tidur terbaca terisi padahal kosong | `BedStatus = Occupied` tetapi tidak ada penempatan aktif | Merugikan. Kamar terlihat penuh padahal tersedia |
| Tempat tidur terbaca dipesan padahal tidak ada pemesanan | `BedStatus = Reserved` tetapi tidak ada pemesanan berstatus `Active` | Merugikan, sama seperti di atas |

Contoh baris laporan:

> Tempat tidur `BD-RSMMC-00042` tertulis Tersedia, tetapi masih ada penempatan aktif atas nama
> Tn. Budi sejak 21 Agustus 2026 pukul 10:40. Episode `RI-2026-08-000123`.

**Yang harus diperiksa sebelum modul dipakai pertama kali:** bila di database sudah ada baris
`MstBed` yang terlanjur berstatus `Reserved` atau `Occupied` — padahal belum pernah ada modul rawat
inap — baris itu wajib dikembalikan ke `Available` lebih dulu. Kalau tidak, laporan selisih akan
langsung menampilkan seluruh baris itu. Ini sudah tercatat pada rencana migration bagian 7.2.

---

## 4. Integrasi eksternal

**Tidak ada satu pun integrasi eksternal yang dirancang pada revisi ini.**

Ini keadaan yang disengaja, bukan bagian yang belum ditulis. Alasannya:

| Hal | Keterangan |
| --- | --- |
| Yang seharusnya ada | Pengiriman data rawat inap ke SATUSEHAT: identitas encounter, riwayat lokasi, diagnosis, tindakan, dan data terkait pemulangan |
| Buktinya dibutuhkan | PRD Modul Rawat Inap baris 814; baseline `ID-INP-INT-001` sampai `ID-INP-INT-005`, seluruhnya dengan `integration_relevance: HIGH` |
| Kenapa tidak dirancang | Keputusannya belum ada. Tercatat sebagai `DEC-INP-005` dan `RWI-OQ-037` |
| Apa yang belum diputuskan | Siapa pemiliknya, data apa yang wajib dikirim, kapan dipicu, dan **di mana riwayat lokasi disimpan** — pada catatan penempatan milik Rawat Inap, atau pada kunjungan milik Registrasi |
| Kenapa itu mahal bila salah | Bila jawabannya "pada kunjungan", pemilik data riwayat lokasi berpindah dari Rawat Inap ke Registrasi |
| **Sudah diputuskan pada 2026-08-21** | `RWI-DEC-053` menetapkan riwayat lokasi **tetap dimiliki Rawat Inap**. Pengiriman dibangun sebagai kemampuan tersendiri yang membacanya. Yang masih terbuka hanya isi kiriman, pemicunya, dan siapa pemiliknya |

### 4.1 Yang sudah disiapkan supaya keputusan itu tidak mahal

Walaupun kontraknya belum dirancang, seluruh bahan yang dibutuhkan pengiriman **sudah tersimpan
dalam bentuk yang dapat dibaca ulang**:

| Bahan yang dibutuhkan SATUSEHAT | Sudah tersedia di |
| --- | --- |
| Identitas encounter | `InpEpisode.EncounterId` |
| Riwayat lokasi beserta periodenya | `InpBedPlacement`, berbentuk baris berperiode |
| Waktu pasien meninggalkan ruangan | `InpEpisode.PhysicallyLeftAt` |
| Versi resume sebelumnya, bila resume pernah diamandemen | `InpDischargeSummaryRevision` |
| Riwayat penanggung jawab | `InpDoctorAssignment`, berbentuk baris berperiode |
| Perubahan status episode beserta waktunya | `InpStatusHistory` |
| Data terkait pemulangan | `InpDischargeSummary` |
| Kelas layanan yang berlaku per periode | `InpBedPlacement.PatientClassId` |

Karena semuanya berbentuk riwayat dan bukan penanda keadaan terakhir, pengiriman kelak tinggal
membaca — tidak ada yang perlu dibongkar.

---

## 5. Kejadian bisnis

Daftar berikut adalah **fakta bisnis**, bukan rancangan mekanisme pengiriman pesan.

| ID | Kejadian | Kapan terjadi | Konsumen yang mungkin peduli |
| --- | --- | --- | --- |
| `EVT-INP-01` | Episode diaktifkan | Episode menjadi `Admitted` | Billing, interoperabilitas, census |
| `EVT-INP-02` | Tempat tidur dipesan | Pemesanan dibuat | Papan ketersediaan |
| `EVT-INP-03` | Pemesanan gugur | Terbaca saat query | Papan ketersediaan |
| `EVT-INP-04` | Pasien menempati tempat tidur | Penempatan dibuka | Billing untuk charge kamar, interoperabilitas |
| `EVT-INP-05` | Pasien berpindah tempat tidur | Perpindahan berhasil | Billing bila kelas berubah, interoperabilitas |
| `EVT-INP-06` | DPJP dialihkan | Pengalihan berhasil | Interoperabilitas, laporan |
| `EVT-INP-07` | Pasien diputuskan boleh pulang | Episode menjadi `DischargePending` | Farmasi untuk obat pulang, kasir |
| `EVT-INP-12` | Pasien meninggalkan ruangan | Kepergian fisik dicatat | Papan ketersediaan, kasir, kebersihan |
| `EVT-INP-08` | Resume pulang ditandatangani | Penandatanganan | Interoperabilitas, rekam medis |
| `EVT-INP-09` | Episode ditutup | Episode menjadi `Closed` | Billing, interoperabilitas, papan ketersediaan |
| `EVT-INP-10` | Episode dibatalkan | Episode menjadi `Cancelled` | Papan ketersediaan, kunjungan |
| `EVT-INP-11` | Episode ditutup menembus gerbang keuangan | Penutupan oleh supervisor | Laporan pengecualian |

### 5.1 Cara mewujudkannya pada MVP

Capability map **tidak menemukan satu pun** sarana antrean pesan atau kotak keluar di dalam
source pada SHA `5afb54b`. Karena itu:

| Hal | Ketetapannya pada MVP |
| --- | --- |
| Bentuk pengiriman | Pemanggilan langsung di dalam service, bukan pesan asinkron |
| Kenapa memadai | Seluruh konsumen yang ada hari ini berada di dalam satu aplikasi dan satu database |
| Kapan menjadi tidak memadai | Begitu `DEC-INP-005` terjawab dan pengiriman ke luar aplikasi dibutuhkan |
| Dicatat sebagai | `ARCH-GAP-006` pada arsitektur domain |

Yang **tidak** dilakukan: membangun sarana antrean pesan sekarang hanya karena mungkin dibutuhkan
kelak. Itu pekerjaan yang belum ada pemintanya.

---

## 6. Modul yang sengaja belum terhubung

| Modul | Kenapa belum | Decision ID |
| --- | --- | --- |
| `ClinicalManagement` | Dokumentasi klinis rawat inap di luar scope | `DEC-INP-001` |
| `PharmacyManagement` | Resep dan obat pulang di luar scope | `DEC-INP-001` |
| `EmergencyInstallationManagement` | Serah terima IGD ke rawat inap di luar scope. **Sejak `0.4.0` arah bacanya sudah dirancang** lewat `INT-INP-06`, tetapi belum terhubung karena `INP-S09` belum dikerjakan. Pemiliknya bernama sejak `RWI-DEC-069`: Rizki Gunawan | `DEC-INP-002` |
| `BillingManagement` | Belum punya kemampuan transaksi. Digantikan penandaan manual sesuai `RWI-RULE-028` | — |

Untuk `BillingManagement`, perlu ditegaskan: modul Rawat Inap **tidak** membangun faktur, tagihan
berjalan, tarif, atau perhitungan biaya sendiri. Yang disimpan hanyalah **pernyataan kelayakan**
berupa `Pending`, `Cleared`, atau `Blocked`, dan pernyataan itu ditandai manual petugas kasir. Ini
bukan sistem billing mini; ia hanya gerbang.

Ketika `BillingManagement` operasional, sumber nilai berpindah dari penandaan manual menjadi bacaan
dari Billing. **Aturan penutupannya tidak berubah** — hanya sumber datanya. Ini sudah dikunci pada
`RWI-RULE-028` aturan 7.

---

## 7. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 1 | `RWI-RULE-005`, `RWI-RULE-007` |
| 2 | `RWI-RULE-027`, `RWI-DEC-039`, `RWI-OQ-033` |
| 3 | `RWI-RULE-027` aturan 6 |
| 4 | `DEC-INP-005`, `RWI-OQ-037`, baseline `ID-INP-INT-001` s.d. `005` |
| 5 | Arsitektur domain bagian J.3, `ARCH-GAP-006` |
| 6 | `RWI-RULE-028`, `DEC-INP-001`, `DEC-INP-002` |

---

## 8. Perubahan pada `contract_version` `0.9.0` — amandemen terbatas ★ 15 September 2026

**Status `draft`.** Seluruhnya internal, satu `ApplicationDbContext`.

| ID | Arah | Modul lawan | Pola | Pasangan | Keadaan |
| --- | --- | --- | --- | --- | --- |
| `INT-INP-08` | Tulis, dipicu penutupan | `MedicalRecordManagement` | Sinkron, satu transaksi | `INT-DOK-13` | `Missing` — `RLN3-CAP-38` |
| `INT-INP-09` | Tulis, dipicu penutupan | `ClinicalManagement` pesanan tindakan | Sinkron, satu transaksi | `INT-DOK-13` | `Missing` |
| `INT-INP-10` | Tulis, dipicu penutupan | `PharmacyManagement` dosis obat | Sinkron, satu transaksi | `INT-KEP-15` | `Missing` |
| `INT-INP-11` | Baca | `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement` | Sinkron, hanya baca | `INT-DOK-20` | `Missing` — usulan isian resume |
| `INT-INP-12` | Baca | `ClinicalManagement` | Sinkron, hanya baca | `INT-DOK-11` | `Missing` — `NeedsReviewCount` census |
| `INT-INP-13` | Dibaca | `ClinicalManagement` penjaga penulis klinis | Sinkron | `INT-DOK-12` | `Extend` — `AssignmentPurpose` |

### 8.1 `INT-INP-08` s.d. `INT-INP-10` — langkah akibat penutupan

| Hal | Isinya |
| --- | --- |
| Urutan | Setelah status `Closed` diterapkan dan sebelum `SaveChanges`: (4) kunci konsep, (5) batalkan pesanan tertunda, (6) batalkan dosis masa depan |
| Kenapa urutan itu | Penguncian tidak bergantung dua lainnya; pembatalan pesanan dan dosis tidak saling bergantung. Urutan dibuat tetap supaya log dan test dapat dibandingkan |
| Kegagalan | Galat pada langkah mana pun → rollback seluruh transaksi, termasuk pengembalian tempat tidur dan penutupan penugasan |
| Idempotensi | Menjalankan ulang pada episode `Closed` ditolak oleh penjaga status yang sudah ada; ketiga langkah juga hanya menyentuh keadaan awal yang benar |
| Selama service tujuan belum dibangun | Langkah yang service-nya belum ada **tidak dipasang**; penutupan berjalan seperti hari ini. Tiap langkah dipasang pada gelombang pemiliknya — 11.8 |
| Pemberitahuan | `INT-INP-08` adalah titik sentuh `MedicalRecordManagement` — **wajib diberitahukan kepada Yoga Aji Pratama**, satu paket dengan `INT-DOK-13` dan `INT-KEP-12` |
| Contoh | Joko ditutup 13.00 → 1 konsep terkunci, 1 pesanan batal, 1 dosis 20.00 batal; response `SideEffects` menampilkan ketiga angka |

### 8.2 `INT-INP-11` — usulan isian resume

| Hal | Isinya |
| --- | --- |
| Yang dibaca | Diagnosis encounter; tindakan `Completed`; butir resep pulang; hasil laboratorium/radiologi final kritis atau abnormal; Assesment Edukasi selesai |
| Batas tunggu | 5 detik per sumber; sumber lambat → `SourceStatus = Unavailable`, sumber lain tetap dikembalikan |
| Privasi | Hanya dikirim kepada pemegang `InpatientDischarge : Read`; tidak dicatat ke custom logger |
| Yang tidak dilakukan | Tidak menyimpan; tidak memperbarui resume otomatis |

### 8.3 `INT-INP-12` — angka "Perlu Review"

| Hal | Isinya |
| --- | --- |
| Yang dibaca | Jumlah entri CPPT menunggu verifikasi yang boleh diverifikasi dokter login (`RWI-DEC-125`, `126`) dan jumlah pesanan menunggu verifikasi instruksinya |
| Kegagalan | `NeedsReviewCount = null` beserta penanda; angka lain tetap tampil |

### 8.4 `INT-INP-13` — tujuan penugasan dibaca penjaga penulis klinis

`InpatientClinicalContextService` membaca `AssignmentPurpose` untuk menegakkan `RWI-DEC-130` butir (5): dokter
berpenugasan `LateDocumentation` tidak dapat memverifikasi CPPT. Keputusan pulang, tanda tangan resume, perpindahan, dan
isolasi sudah tertutup bagi `OnCallDoctor` oleh `GUARD-INP-01` s.d. `04`.

---

## 9. Perubahan pada `contract_version` `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.10.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Pemilik pihak lain | Kamar Operasi — Ikbal Yulianto (**disetujui Ikbal Yulianto, `RWI-DEC-208`**); Billing — `RWI-DEC-192`, `196`; Clinical — Muhammad Hamzah |
| Keputusan yang belum ada | **Tidak ada.** ~~`DEC-INP-018`~~ ditutup `RWI-DEC-207`. `INT-RWF-22` tetap mengirim dengan `EncounterId` kasus OK; Billing menautkan kunjungan itu ke invoice `RANAP` (`INT-RWF-25`) |

Seluruh integrasi di bawah adalah panggilan **dalam proses yang sama** (satu aplikasi, satu database), bukan HTTP antarmodul, mengikuti pola `InpBillingDepositAdapter`.

| ID | Dari → ke | Pemicu | Isi | Sinkron | Idempotensi | Bila gagal | Rekonsiliasi |
|---|---|---|---|---|---|---|---|
| `INT-RWF-19` | Rawat Inap → OK | Pesan ruang bedah dari bangsal | `PatientId`, `EncounterId` episode, satu `PatientProcedureId`, jenis layanan, rencana anestesi, isian pesanan, akun penginput | Ya | `Idempotency-Key` diteruskan ke `CreateOprCaseRequest.IdempotencyKey` | Pesanan tidak tersimpan; pengguna mencoba lagi | Tidak perlu |
| `INT-RWF-20` | OK → Rawat Inap (baca) | Penerimaan serah terima | "Apakah pasien X menempati bed aktif di unit Y" (`InpPatientLocationQuery`) | Ya | Baca | Gagal baca → penerimaan **ditolak** (gagal tertutup) | — |
| `INT-RWF-21` | OK → Clinical | Kasus `Completed` | Selesaikan setiap order tindakan yang dirujuk kasus: pelaksana dokter operator, waktu = kasus selesai; fakta tagih tindakan lahir dari jalur order tindakan yang sudah ada | Sesudah commit kasus | Order sudah `Completed` dilewati | Dicatat; dicoba ulang lewat rekonsiliasi delivery OK | `GET …/integration/reconciliation` menampilkan order yang belum selesai |
| `INT-RWF-22` | OK → Billing | Kasus `Completed` | Komponen `ANESTHESIA` (bila catatan anestesi final), `OR_RENT` (durasi menit kamar operasi), `MATERIAL-{usageId}` (pemakaian `Used`); `SourceContext = OPERATING_ROOM`, `EncounterId` kasus | Sesudah commit (outbox `OprIntegrationDelivery`) | `case:charge:component:revision` | Delivery `Failed`, dicoba ulang | Rekonsiliasi OK yang sudah ada; Billing menolak tarif tak ada → baris "tarif belum ada" |
| `INT-RWF-23` | OK → Rawat Inap | Simpan kamar pulih `Inpatient`/`Icu`, pasien tanpa episode hadir | `PatientId`, `SourceEncounterId`, `OprCaseId`, dokter operator, tingkat perawatan, catatan keputusan | Ya, dalam transaksi OK | Satu `Pending` per kasus (unique parsial) | Transaksi kamar pulih gagal seluruhnya; petugas menyimpan ulang | Tidak perlu — disetujui `RWI-DEC-208` |
| `INT-RWF-24` | OK → Rawat Inap | Keputusan kamar pulih berubah dari `Inpatient`/`Icu`, atau pasien boleh pulang | Batalkan permintaan `Pending` beralasan | Ya, dalam transaksi OK | Permintaan bukan `Pending` dilewati | Sama | — |
| `INT-RWF-25` | Rawat Inap → Billing | Admisi dari permintaan selesai | Ketukan pintu `ADMISSION_CONFIRMED` seperti admisi biasa — **isi pesan tetap daftar putih** (`INV-RWF-05`); Billing membaca `InpAdmissionReferral.SourceEncounterId` dari sumbernya dan menautkan kunjungan asal ke invoice `RANAP` (`integrasi-billing` `INT-RWF-29`, `RWI-DEC-207`) | Outbox (`integrasi-billing` `1.1.0`) | Sama dengan `INT-RWF-01`; tautan unik per invoice dan kunjungan | Sama | Putar ulang `I5` |
| `INT-RWF-26` | Rawat Inap → Clinical | Transfer antarunit tersimpan | `InpEpisodeId`, unit asal, unit tujuan, penempatan asal dan tujuan | Sesudah commit transfer | Satu dokumen per penempatan tujuan | Dicatat; transfer tetap sah | Daftar Pantau menampilkan transfer antarunit tanpa dokumen; dibuat ulang saat dibuka |
| `INT-RWF-27` | Rawat Inap/Clinical/OK → OK (baca) | Bangsal membuka ringkasan operasi | `GET cases/{id}/post-operative-summary` | Ya | Baca | Gagal → "Ringkasan operasi tidak dapat dimuat"; tidak ada salinan | — |
| `INT-RWF-28` | OK → Clinical (baca) | Kirim pra-operasi | Tanda vital terakhir episode; skor nyeri terakhir | Ya | Baca | Tanpa tanda vital → `OPR-WPO-005` | — |

**Yang tidak diintegrasikan.** Biaya operasi tidak lewat ketukan pintu Rawat Inap — OK mengirim langsung ke Billing (`INT-RWF-22`) karena kasus OK juga melayani pasien non-rawat inap. Bahan OK yang tertagih lewat `INT-RWF-22` **tidak** ditagih lagi oleh Farmasi (`RWI-DEC-196`): delivery bahan memakai sumber `OPERATING_ROOM`, dan pengeluaran stok OK (`inventory/dispatch`) tetap tanpa tagihan.

**Penyelarasan decision log revision `31` ★ 2 Oktober 2026.** `INT-RWF-23` dan `INT-RWF-24` disetujui OK (`RWI-DEC-208`). `INT-RWF-25` dikoreksi: versi sebelumnya menulis pesan "ditambah `SourceEncounterId`", yang melanggar `INV-RWF-05`.

---

## 10. Perubahan pada `contract_version` `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.11.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`) |
| Pemilik pihak lain | `ClinicalManagement` — Muhammad Hamzah (disetujui lewat `RWI-DEC-264`); `PatientManagement` — **disetujui `RWI-DEC-266`**; HR Master Data — **disetujui `RWI-DEC-266`**; `RegistrationManagement` — **menunggu `RWI-OQ-128`**; Billing — Yasmina, service deposit yang ada dipakai apa adanya, method tarif kamar **menunggu `RWI-OQ-129`**; Kamar Operasi — service yang ada dipakai apa adanya |
| Prinsip | Seluruh integrasi adalah **bacaan** dalam proses yang sama (satu aplikasi, satu database), bukan HTTP antarmodul. Workspace PPRI **tidak menulis** ke modul lain mana pun dan **tidak** menerbitkan kejadian ke Billing, karena dokumen admisi tidak menimbulkan biaya (`RWI-DEC-257`, `264`) |

Satu-satunya pemanggil sisi Rawat Inap adalah `InpAdmissionSourceReader` (`INV-RWA-14`). "Bila gagal" ditulis dari sudut pandang petugas.

| ID | Dari → ke | Pemicu | Isi yang dibaca | Sinkron | Idempotensi | Bila gagal atau belum tersedia | Rekonsiliasi |
|---|---|---|---|---|---|---|---|
| `INT-RWA-01` | Rawat Inap → `PatientManagement` | Membuka ruang kerja; kunci dokumen; cetak gelang, label, IPD | Identitas pasien dan isi QR No. RM (`PatientProfileQueryService.GetIdentityAsync`, `PatientQrPayloadBuilder`) | Ya | Baca | "DATA PASIEN TIDAK DAPAT DIMUAT"; tidak ada form; kunci dan cetak ditolak (`FR-RWA-008`, `VAL-RWA-27`) | — |
| `INT-RWA-02` | Rawat Inap → `PatientManagement` | Isian bawaan GC V1, Data Wali, penanggung jawab IPD | Relasi terstruktur dan kontak darurat: nama, hubungan, alamat, telepon, penanda penanggung jawab (`GetPartyCandidatesAsync`) | Ya | Baca | Daftar pilihan kosong dengan pesan; petugas mengisi manual | — |
| `INT-RWA-03` | Rawat Inap → Clinical | Header | Nama alergi aktif (`PatientAllergyQueryService`) | Ya | Baca | "Alergi tidak dapat dimuat"; form lain tetap jalan | — |
| `INT-RWA-04` | Rawat Inap → Clinical | Header, label, IPD, kelengkapan Selisih Biaya, salinan beku | Jenis penjamin, nama, kelas, polis, **nomor kartu, nomor peserta** (`EncounterInsuranceService.GetContextAsync` yang diperluas) | Ya | Baca | Kelengkapan Selisih Biaya "tidak dapat dihitung"; kunci Selisih Biaya ditolak; label tanpa baris No. Kartu | — |
| `INT-RWA-05` | Rawat Inap → Clinical | IPD; saran butir 1 Serah Terima | Surat Pengantar Rawat Inap `Issued` terbaru pada kunjungan episode: nomor, dokter, tanggal, diagnosis, alasan (`DoctorCertificateService.GetLatestIssuedInpatientReferralAsync`) | Ya | Baca | Garis kosong; tanpa saran | — |
| `INT-RWA-06` | Rawat Inap → Registration | IPD pasien tanpa surat pengantar | Nama dokter perujuk luar dan institusinya (`EncounterReferralQueryService`) | Ya | Baca | Garis kosong (`RWI-AC-386`) | — |
| `INT-RWA-07` | Rawat Inap → Billing | Header (`ViewAmount`), simpan dan kunci Pelunasan Deposit, kelengkapan, peringatan jatuh tempo | `IsPolicyRequired`, minimum, diterima, kekurangan, interval tindak lanjut (`BillingDepositService.GetEpisodeDepositSummaryAsync`) | Ya | Baca | Angka tidak tampil; simpan dan kunci Pelunasan Deposit ditolak `VAL-RWA-12`; kelengkapan deposit "?" | Tidak perlu — angka beku menyimpan `AmountsReadAt` |
| `INT-RWA-08` | Rawat Inap → Billing, lalu Clinical | IPD "Rencana @ Kamar (Rp)"; baris kamar Estimasi | Tarif kamar per hari menurut unit dan kelas (`BillingCalculationService.GetDailyRoomRateAsync`), lalu harga menurut penjamin (`InsuranceCoverageService.ResolveTariffAsync`) | Ya | Baca | "lihat kasir" (`RWI-AC-387`) | — |
| `INT-RWA-09` | Rawat Inap → Clinical | Simpan dan kunci Estimasi Biaya (di luar gelombang) | Harga tindakan menurut penjamin dan kelas (`InsuranceCoverageService.ResolveProcedureAsync`) — tidak bergantung hak memesan (`RWI-DEC-258` butir 1) | Ya | Baca | Baris "Tarif belum tersedia" | — |
| `INT-RWA-10` | Rawat Inap → Billing | Kunci Estimasi Biaya (di luar gelombang) | Kebijakan biaya administrasi aktif rawat inap (`AdministrationFeePolicyService`) | Ya | Baca | Catatan biaya admin tidak dicetak | — |
| `INT-RWA-11` | Rawat Inap → Kamar Operasi | Kelengkapan (aturan wajib Estimasi); isian kepala Estimasi | Kasus pada kunjungan episode berstatus selain `Cancelled`/`Rejected` (`OperatingRoomCaseService.GetPagedAsync` dengan `EncounterId`) | Ya | Baca | Kewajiban Estimasi "?" | — |
| `INT-RWA-12` | Rawat Inap → HR Master Data | Seluruh cetakan; kode label; zona waktu | Profil situs `IsMainSite` aktif (`HospitalSiteProfileQueryService.GetMainSiteProfileAsync`) | Ya | Baca | Kop berbaris kosong, **tidak pernah** nilai bawaan yang ditanam; kunci dokumen ditolak `VAL-RWA-27` | — |
| `INT-RWA-13` | Rawat Inap (dalam modul) | Membuka Detail Episode | Kelengkapan tanpa rupiah dan jatuh tempo terlewati (`InpAdmissionCompletenessEvaluator` dari `InpEpisodeService`) | Ya | Baca | Detail episode tetap tampil; peringatan "Kelengkapan dokumen admisi tidak dapat dihitung" | — |
| `INT-RWA-14` | Modul lain → Rawat Inap | Kelak: Workspace Keperawatan, Workspace Dokter (amandemen terpisah, PRD bagian 8) | Ringkasan Nilai Kepercayaan dan Privasi `Completed` (`GET …/admission-workspace/patient-rights`) | Ya | Baca | Konsumen menampilkan "Ringkasan hak pasien tidak dapat dimuat" | — |

**Yang sengaja tidak diintegrasikan.**

| Tidak dibuat | Alasan |
|---|---|
| Tulis ke `TrxPatientConsent` | *Fail-closed* (`RWI-DEC-230`) |
| Tulis ke `MrcClinicalDocumentIntegrity` | `RWI-DEC-229` |
| Kejadian outbox ke Billing | Dokumen admisi tidak menimbulkan biaya; batas `integrasi-billing` tidak bergerak |
| Pemberitahuan jatuh tempo ke modul Billing | Ditunda (PRD bagian 8, G-46); peringatan hanya di Workspace PPRI dan Detail Episode |
| Penurunan kelas otomatis saat jatuh tempo terlewati | PRD bagian 15 butir 9; tetap transfer manual |
| Hub SignalR | Penyegaran berkala 30 detik di layar (G-34) |
| Integrasi eksternal | Tidak ada; tanda tangan elektronik tersertifikasi di luar MVP |

## 11. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

### 11.1 Dependency dan batas integrasi

| ID | Arah | Kontrak | Failure/ownership | Trace |
| --- | --- | --- | --- | --- |
| INT-BM-01 | MasterData → Bed operations | Baca master bed/room/unit/class live + verified class profile | MasterData tetap single source; tidak menghitung dari harga; invalid digest→Unknown | BM-G01, AC-405/423 |
| INT-BM-02 | Bed operations → MasterData bed copy | Update raw status di transaksi yang sama, melalui guard coordinator | Cleaning raw4 untuk internal waiting/inprogress; closed5..7 tidak ditimpa cleaning | AC-407/412/424 |
| INT-BM-03 | Departure/transfer → Readiness | Release placement aktual lalu root WaitingCleaning cycle baru | Closure episode lama tidak release bed baru; admission unused cancel tidak dirty | AC-407/408/426 |
| INT-BM-04 | Transfer → Clinical handover existing | Pertahankan alur sesudah commit; gagal callback dicatat/retry existing | Tidak gate penerimaan tujuan; tidak reverse transfer committed | DEC-284/290, AC-414/422 |
| INT-BM-05 | Placement/correction → Billing | Pertahankan BED_OCCUPIED untuk penempatan/transfer biasa dan OCCUPANCY_CORRECTED untuk koreksi; receiver requery timeline | Gunakan integrasi-billing contract approved 1.1.0 + outbox/ack existing; tidak mengubah event name/payload dari kontrak canonical | DEC-157/161/166, AC-415/418 |
| INT-BM-06 | Authorization platform → BedAccess | Permission existing + trusted scope assignment; deny bila proof hilang | Tidak role grants atau scope inferred; masking server | AC-416/417/425 |
| INT-BM-07 | Frontend → API committed outcome | Key stabil, expected versions, query own outcome; invalidate affected reads | Tidak optimistic success/offline overwrite; await refresh baru confirmation | AC-420–422 |
| INT-BM-08 | Master hierarchy writers → Bed guards | Perubahan availability unit/room/class serialized terhadap affected beds | Inventory source/path & implement adapter sebelum mixed-writer cutover | AC-419/424, BM-G04 |

### 11.2 Billing dan failure

Bed Management tidak membuat tarif, ledger, invoice, folio atau algoritma charge baru. ManualTransferCategory adalah catatan klasifikasi perpindahan, bukan price override. Kelas/waktu placement sah menjadi input yang dibaca Billing; correction tidak menghapus versi lama. Filter efektif mengikuti SupersededByCorrectionId, bukan IsSuperseded (yang juga true untuk transfer biasa).

Kontrak canonical Billing berada pada `../../integrasi-billing/contracts/integration-contract.md`; event schema/ack/idempotency receiver tetap di sana. Jangan menyalin schema event kedua atau menganggap event delivered sebelum receiver ack. Tambahan receipt API di bed berbeda dari dedup outbox receiver. Outbox ditulis bersama mutation bila event existing mensyaratkan; callback hanya sesudah commit; retry delivery tidak mengulang transfer.

Jika Billing guard atau scope dependency tidak dapat dipastikan saat commit, fail closed tanpa setengah data. Kepergian fisik existing tetap berbeda dari penutupan episode; tidak menambah gate kasir pada release bed. Gangguan network sesudah commit: own outcome lookup, refresh, retry same key jika perlu, tidak automatic reverse atau offline queue.

### 11.3 Batas scope

Tidak ada integrasi eksternal baru, worker expiry, reminder, offline workflow atau SLA baru. SOP rekonsiliasi dipenuhi dari BM-G02, bukan dibuat oleh AI. BM-DEP-01..05 pada gate tetap berlaku; dependencies non-blocking untuk desain bounded dapat menjadi blocker aktivasi operasi terkait.

### 11.4 Konsumen internal reserve — finalisasi 11 Oktober 2026

`InpAdmissionTransferService` juga memanggil reserve, sehingga ikut M3 dan guard INT-BM-02/07. Komposisi pembukaan episode, reserve, disposisi existing, dan receipt memakai satu transaksi caller sebagaimana backend14.14; layanan yang ikut tidak membuka transaksi bersarang. Tidak menambah integrasi eksternal, event Billing, atau aturan klinis IGD. Audit current dan fingerprint baru ada pada evidence/bed-management-impact-scan-20261011.md; arsip audit62 lama tidak tersedia untuk verifikasi ulang.
