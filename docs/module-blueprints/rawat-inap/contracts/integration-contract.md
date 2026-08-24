# Integration Contract — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.3.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | `evidence/03-hospital-domain-architecture.md` revision `0.1` bagian J; `00-interview-decisions.md` revision `5` |
| Backend SHA | `5afb54b` |
| Dampak kompatibilitas | Satu arah tulis lintas modul yang baru. Tidak ada kontrak eksternal yang berubah |

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

Keempatnya **tidak** menyalin data. Yang disimpan modul ini hanya Id-nya, dan nama ditampilkan
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
| Persetujuan yang dibutuhkan | Pemilik `MasterData`, tercatat `RWI-OQ-033`, **belum ada** |

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
| `EmergencyInstallationManagement` | Serah terima IGD ke rawat inap di luar scope | `DEC-INP-002` |
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
