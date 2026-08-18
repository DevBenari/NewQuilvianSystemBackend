# Format Registry Sistem Quilvian

| Field | Nilai |
| --- | --- |
| Status | Canonical format |
| Berlaku untuk | Seluruh dokumen di `docs/system-registry/` |
| Ditulis oleh | `/scan-system-registry` (pintasan `/qv-scan`) |
| Dibaca oleh | `/grill-me`, `/trace-existing-capabilities`, `/design-business-module`, `/build-module-backend` |
| Aturan gaya | Tunduk pada [rule-output](../rule-output/aturan-output-dokumentasi.md) |

Dokumen ini menetapkan **bentuk baku hasil scanning project**. Formatnya merupakan
penyempurnaan dari format audit arsitektur yang sudah dipakai di lingkungan Quilvian, dengan
tujuh perbaikan yang dijelaskan pada bagian 1.

---

## 1. Tujuh perbaikan dari format sebelumnya

Format audit sebelumnya sudah benar arahnya: ia memisahkan yang sudah ada dari yang belum, dan
memberi legenda status. Tujuh hal berikut diperbaiki agar dokumen tetap dapat dipercaya ketika
sistem berisi ratusan tabel.

| No | Masalah pada format lama | Perbaikan |
| ---: | --- | --- |
| 1 | Satu konsep punya empat penulisan status: `[SUDAH]`, `[SUDAH-IDENTITY]`, `[SUDAH/SESUAIKAN]`, `[SUDAH-MENDUKUNG]` | Satu sumbu status saja, lima tingkat `L0`–`L4`, tanpa varian |
| 2 | Fakta dan usulan tercampur dalam satu pohon, sehingga `[BARU-WAJIB]` terbaca seolah sudah diputuskan | Registry hanya memuat fakta. Usulan pindah ke blueprint modul setelah owner memutuskan |
| 3 | `[SUDAH]` hanya berarti "terdaftar di `ApplicationDbContext`", padahal pembaca mengiranya "siap dipakai" | Kesiapan dinyatakan berlapis: model, configuration, migration, controller, consumer |
| 4 | Tidak ada bukti lokasi, sehingga klaim tidak bisa diperiksa | Setiap baris membawa path dan commit SHA |
| 5 | Tidak ada kolom pemilik modul, padahal itu penyebab utama konflik | Kolom pemilik wajib; yang tidak jelas ditulis `Belum ditentukan` dan masuk zona konflik |
| 6 | Pohon ASCII tidak terbaca untuk 445 entity dan tidak bisa disaring atau dibandingkan | Tabel per area, ditambah indeks abjad |
| 7 | Tidak ada penanda kapan dokumen berlaku, sehingga cepat basi tanpa ketahuan | Manifest memuat SHA dan status kesegaran |

Pohon ASCII tetap boleh dipakai, tetapi hanya untuk gambaran satu area, bukan untuk memuat
seluruh sistem.

---

## 2. Legenda status — satu sumbu, lima tingkat

Status menjawab satu pertanyaan saja: **sejauh mana kemampuan ini benar-benar sudah jadi.**

| Tingkat | Nama | Syarat yang harus terbukti | Artinya bagi developer |
| --- | --- | --- | --- |
| `L0` | Tidak ada | Tidak ditemukan di source mana pun | Belum ada apa-apa |
| `L1` | Terdaftar | Model ada, `DbSet` terdaftar di `ApplicationDbContext` | Baru berupa kelas. Belum tentu ada tabelnya di database |
| `L2` | Berskema | Tingkat `L1` + `IEntityTypeConfiguration` + migration yang membuat tabelnya | Tabel sudah nyata dan punya relasi serta index |
| `L3` | Berlayanan | Tingkat `L2` + controller atau service yang dapat dipanggil | Sudah bisa dipakai lewat API |
| `L4` | Terpakai | Tingkat `L3` + ada pemakai nyata: layar frontend atau modul backend lain | Terbukti dipakai, bukan sekadar tersedia |

Ditambah satu penanda yang berdiri sendiri:

| Penanda | Arti | Contoh |
| --- | --- | --- |
| `⚠ Bermasalah` | Ada lapisan yang melompat atau hilang | Ada controller tetapi tidak ada migration, sehingga endpoint pasti gagal saat dijalankan di luar komputer developer |

### Contoh pembacaan

> `MstPatient` bertingkat `L4 Terpakai`. Artinya modelnya ada, tabelnya sudah dibuat lewat
> migration, endpoint-nya tersedia, dan ada layar frontend yang benar-benar memanggilnya. Modul
> baru **tidak boleh** membuat tabel pasien sendiri.
>
> `MstBillingItemCategory` bertingkat `L1 Terdaftar`. Artinya kelasnya ada dan sudah didaftarkan,
> tetapi tabelnya belum tentu ada di database dan belum ada endpoint-nya. Modul yang
> membutuhkannya harus memperhitungkan pekerjaan tambahan, bukan menganggapnya siap.

Perbedaan dua contoh di atas tidak terlihat pada format lama, karena keduanya sama-sama ditulis
`[SUDAH]`.

### Yang bukan status

Kata seperti `wajib`, `prioritas`, `sprint 1`, atau `perlu disesuaikan` **dilarang** muncul di
registry. Itu keputusan manusia. Registry hanya menyatakan apa adanya.

---

## 3. Berkas 0 — `registry-manifest.md`

Halaman identitas. Wajib menjadi berkas pertama yang dibaca skill lain.

```markdown
# Registry Sistem Quilvian

| Field | Nilai |
| --- | --- |
| Versi registry | 3 |
| Mode pemindaian | `full` |
| Tanggal pemindaian | 2026-08-14 |
| Backend | `NewQuilvianSystemBackend` cabang `MHamzah` commit `dd09806` |
| Frontend | `QuilvianSystemFrontendDev` cabang `HamzahV2` commit `08c84d371` |
| Status kesegaran | `SEGAR` |
| Batas berlaku scan penuh | 2026-09-13 |
| Cakupan yang tidak diperiksa | Database runtime, service eksternal, environment produksi |

## Ringkasan angka

| Yang dihitung | Jumlah | Cara menghitung |
| --- | ---: | --- |
| Area | 5 | Folder tingkat satu di `Areas/` ditambah `Models/` bersama |
| Modul | 38 | Folder modul di dalam setiap area |
| `DbSet` terdaftar | 445 | Baris `public DbSet<...>` pada `Repositories/ApplicationDbContext.cs` |
| File EF configuration | 452 | Berkas `*Configuration.cs` di `Repositories/` |
| Controller | 246 | Berkas `*Controller.cs` di `Areas/` dan `Controllers/` |
| Migration | 81 | Berkas migration di `Migrations/`, di luar Designer dan Snapshot |
| Grup Swagger `[Tags(...)]` | <n> | Nilai unik atribut `[Tags(...)]` |
| Zona konflik terbuka | <n> | Baris berstatus terbuka pada berkas 05 |

## Sebaran tingkat kesiapan

| Tingkat | Jumlah entity | Bagian |
| --- | ---: | ---: |
| `L4 Terpakai` | <n> | <n>% |
| `L3 Berlayanan` | <n> | <n>% |
| `L2 Berskema` | <n> | <n>% |
| `L1 Terdaftar` | <n> | <n>% |
| `⚠ Bermasalah` | <n> | <n>% |

Angka ini adalah gambaran kasar kesehatan sistem. Banyaknya entity `L1` berarti banyak kelas
yang sudah ditulis tetapi belum menjadi layanan yang dapat dipakai.

## Cara menghasilkan ulang

Jalankan `/qv-scan full` untuk pemindaian menyeluruh, atau `/qv-scan refresh` untuk memperbarui
hanya bagian yang berubah sejak commit di atas.
```

Selisih antara 445 `DbSet` dan 452 file configuration pada contoh di atas bukan kesalahan
otomatis, tetapi wajib dijelaskan: bisa berarti ada configuration untuk tipe turunan, atau ada
configuration yang entity-nya sudah tidak terdaftar. Selisih yang tidak dapat dijelaskan masuk
zona konflik.

---

## 4. Berkas 1 — `01-peta-area-dan-modul.md`

Menjawab: **sistem ini terdiri dari apa saja, dan siapa pemilik tiap bagian.**

```markdown
## Peta area

| Area | Isi singkat | Jumlah modul | Jumlah entity | Pemilik proses bisnis |
| --- | --- | ---: | ---: | --- |
| `Administrator` | Pengaturan sistem, hak akses, data induk administratif | 2 | <n> | Manajer Sistem Informasi |
| `Corporate` | Kepegawaian, penggajian, kehadiran, pengembangan pegawai | 19 | <n> | Human Resource |
| `HealthServices` | Pelayanan pasien dari pendaftaran sampai farmasi dan billing | 7 | <n> | Direktur Pelayanan Medis |
| `SelfServices` | Layanan mandiri pegawai dan biometrik | 3 | <n> | Human Resource |
| `Global/Shared` | Data induk lintas area seperti wilayah, bank, perangkat | — | <n> | Manajer Sistem Informasi |

## Modul di dalam area HealthServices

| Modul | Folder | Prefix entity | Entity | Controller | Pemilik data | Tingkat rata-rata |
| --- | --- | --- | ---: | ---: | --- | --- |
| Master Data | `Areas/HealthServices/MasterData/` | `Mst` | <n> | <n> | Rekam Medis | `L3` |
| Patient Management | `Areas/HealthServices/PatientManagement/` | `Mst`, `Trx` | <n> | <n> | Rekam Medis | `L4` |
| Registration Management | `Areas/HealthServices/RegistrationManagement/` | `Trx` | <n> | <n> | Pendaftaran | `L4` |
| Clinical Management | `Areas/HealthServices/ClinicalManagement/` | `Trx` | <n> | <n> | Komite Medis | `L3` |
| Emergency Installation | `Areas/HealthServices/EmergencyInstallationManagement/` | `Trx`, `Mst` | <n> | <n> | Kepala IGD | `L3` |
| Pharmacy Management | `Areas/HealthServices/PharmacyManagement/` | `Mst`, `Trx` | <n> | <n> | Instalasi Farmasi | `L3` |
| Billing Management | `Areas/HealthServices/BillingManagement/` | `Mst` | <n> | <n> | Keuangan | `L1` |

Kolom **Pemilik data** menjawab pertanyaan "siapa yang berhak mengubah aturan di sini".
Kolom **Tingkat rata-rata** memberi gambaran cepat modul mana yang masih kerangka.

## Catatan struktur

| Temuan | Lokasi | Keterangan |
| --- | --- | --- |
| Folder `Controller` (tunggal) | `Areas/HealthServices/EmergencyInstallationManagement/` | Area lain memakai `Controllers`. Utang teknis, jangan ditiru modul baru |
| Folder `DTOS` (huruf besar) | `Areas/HealthServices/RegistrationManagement/` | Area lain memakai `DTOs` |
```

Bagian catatan struktur penting untuk modul baru: developer baru cenderung meniru folder
terdekat, sehingga penyimpangan menyebar bila tidak ditandai.

---

## 5. Berkas 2 — `02-entity-terdaftar.md`

Inti registry. Menjawab: **apa saja yang sudah dibuat, dan sejauh mana jadinya.**

Satu tabel per modul. Jangan satu tabel raksasa untuk 445 entity.

```markdown
### HealthServices / Registration Management

Pemilik data: Pendaftaran. Prefix yang dipakai: `Trx`.

| Entity | Jenis | Model | Config | Migration | API | Consumer | Tingkat | Bukti |
| --- | --- | :---: | :---: | :---: | :---: | :---: | --- | --- |
| `TrxPatientEncounter` | Transaksi | ✓ | ✓ | ✓ | ✓ | ✓ | `L4` | `Areas/.../Models/TrxPatientEncounter.cs` @ `dd09806` |
| `TrxQueue` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/.../Models/TrxQueue.cs` @ `dd09806` |
| `TrxKioskScanSession` | Transaksi | ✓ | ✓ | ✓ | ✓ | — | `L3` | `Areas/.../Models/TrxKioskScanSession.cs` @ `dd09806` |
| `TrxPatientEncounterGuarantor` | Transaksi | ✓ | ✓ | ✓ | — | — | `L2` | `Areas/.../Models/TrxPatientEncounterGuarantor.cs` @ `dd09806` |

Keterangan kolom:

- **Jenis** — `Master` untuk data induk, `Transaksi` untuk data kejadian, `Sistem` untuk
  kebutuhan teknis, `Identitas` untuk pengguna dan hak akses.
- **Model** — kelas entity ditemukan.
- **Config** — ada `IEntityTypeConfiguration` yang mengatur relasi dan index.
- **Migration** — ada migration yang benar-benar membuat atau mengubah tabelnya.
- **API** — ada controller atau service yang memakainya.
- **Consumer** — ada layar frontend atau modul backend lain yang memanggilnya.
- **Bukti** — path relatif ditambah commit SHA yang diaudit.

Isi tanda `✓` bila terbukti, `—` bila tidak ditemukan, dan `?` bila tidak dapat diperiksa
karena batas akses. Jangan mengisi `✓` berdasarkan kemiripan nama berkas.
```

Aturan pengisian yang mengikat:

1. Satu baris satu entity. Jangan menggabungkan beberapa entity dalam satu baris.
2. Tanda `✓` pada kolom Migration hanya boleh diberikan bila migration-nya ditemukan, bukan
   karena "biasanya ada".
3. Entity yang punya `API` tetapi tidak punya `Migration` wajib ditandai `⚠ Bermasalah` dan
   masuk zona konflik.
4. Bukti wajib memuat commit SHA. Bukti tanpa SHA dianggap belum ditulis.

---

## 6. Berkas 3 — `03-kepemilikan-data-bersama.md`

Berkas paling menentukan untuk mencegah konflik antar modul. Menjawab: **data ini milik siapa,
dan siapa yang boleh mengubahnya.**

```markdown
## Data bersama dan pemiliknya

| Konsep | Entity canonical | Modul pemilik | Boleh menulis | Hanya boleh membaca | Dilarang dibuat ulang sebagai |
| --- | --- | --- | --- | --- | --- |
| Pasien | `MstPatient` | Patient Management | Patient Management | Seluruh modul pelayanan | `PatientIGD`, `PatientLab`, `PasienRujukan` |
| Dokter | `MstDoctor` | Human Resource | Human Resource | Seluruh modul pelayanan | `DokterPoli`, `DoctorLab` |
| Pegawai | `MstEmployee` | Human Resource | Human Resource | Seluruh modul | Salinan pegawai per area |
| Episode pelayanan | `TrxPatientEncounter` | Registration Management | Registration Management | Clinical, Pharmacy, Billing | `Kunjungan`, `VisitIGD` |
| Poli | `MstClinic` | Health Services Master Data | Master Data | Seluruh modul pelayanan | `PoliRujukan` |
| Tarif | `MstTariff` | Health Services Master Data | Master Data | Billing, Order, Pharmacy | Tabel tarif per modul |
| Obat | `MstDrug` | Pharmacy Management | Pharmacy Management | Clinical, Billing | `ObatIGD` |
| Kamar dan tempat tidur | `MstRoom`, `MstBed` | Health Services Master Data | Master Data | Rawat inap, IGD | Salinan per unit |

## Aturan pemakaian data bersama

1. Modul yang membutuhkan data bersama menyimpan **penunjuknya** saja, yaitu `Id` entity
   pemilik. Jangan menyalin nama, alamat, atau nomor identitas ke tabel sendiri.
2. Pengecualian yang sah adalah *snapshot* transaksi, yaitu penyalinan nilai pada saat
   transaksi terjadi karena nilainya memang harus dibekukan.

   > **Contoh:** tagihan menyimpan `HargaSaatTransaksi` sebesar Rp 150.000, walaupun tarif
   > induknya kemudian berubah menjadi Rp 175.000. Tagihan lama tidak boleh ikut berubah.
   > Sebaliknya, menyalin nama pasien ke tabel antrean **bukan** snapshot yang sah, karena
   > nama pasien tidak perlu dibekukan.
3. Modul yang merasa data bersama kurang lengkap tidak boleh membuat tabel tandingan. Ia
   mengajukan penambahan kolom kepada modul pemilik.
4. Kepemilikan yang belum jelas ditulis `Belum ditentukan` dan otomatis menjadi zona konflik.
```

---

## 7. Berkas 4 — `04-kavling-nama-dan-endpoint.md`

Mencegah dua modul mengambil nama atau alamat yang sama.

```markdown
## Aturan prefix entity

| Prefix | Untuk apa | Contoh | Jumlah terpakai |
| --- | --- | --- | ---: |
| `Mst` | Data induk yang jarang berubah | `MstClinic` | <n> |
| `Trx` | Data kejadian atau transaksi | `TrxPatientEncounter` | <n> |
| `Wfp` | Data profil dan siklus kerja pegawai | `WfpPayroll` | <n> |
| `Sys` | Kebutuhan teknis dan pengaturan sistem | `SysAccessPolicy` | <n> |
| `Emp` | Data kehadiran pegawai | `EmpAttendance` | <n> |

Prefix salah pakai wajib dilaporkan. Contohnya data induk yang diberi nama `Trx...` akan
menyesatkan pembaca berikutnya.

## Nama yang sudah dipakai

Sebelum menetapkan nama entity baru, periksa daftar ini. Nama yang sudah dipakai tidak boleh
diambil ulang, walaupun berada di area berbeda.

| Nama | Modul pemilik | Area |
| --- | --- | --- |
| `TrxPatientReferral` | belum ada | — |
| `TrxLabOrder` | belum ada | — |

Baris "belum ada" sengaja dicantumkan untuk nama yang sedang direncanakan modul lain, agar dua
modul tidak memesan nama yang sama pada waktu bersamaan.

## Grup Swagger yang sudah terdaftar

Judul ditulis persis seperti nilai `[Tags(...)]` pada controller.

### Corporate / Human Resource / Master Data / Allowance Type

Base URL: `api/v1/corporate/human-resource/master-data/allowance-types`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Menampilkan daftar jenis tunjangan | `AllowanceType : Read` |

Registry cukup memuat **daftar grup dan base URL**-nya, tidak perlu seluruh endpoint. Rincian
endpoint per modul tetap menjadi tugas `/trace-existing-capabilities`.

## Base URL yang sudah dipakai

| Awalan | Area | Keterangan |
| --- | --- | --- |
| `api/v1/health-services/...` | HealthServices | <n> controller |
| `api/v1/corporate/human-resource/...` | Corporate | <n> controller |

Modul baru wajib memakai awalan yang konsisten dengan areanya.
```

---

## 8. Berkas 5 — `05-zona-konflik.md`

Menjawab: **di mana saja titik yang berpotensi membuat dua modul bertabrakan.**

```markdown
## Jenis konflik

| Kode | Jenis | Cara mendeteksi |
| --- | --- | --- |
| `KF-1` | Nama kembar | Dua entity bernama sangat mirip di area berbeda |
| `KF-2` | Kandidat duplikasi konsep | Dua entity berbeda nama tetapi menyimpan konsep yang sama |
| `KF-3` | Entity tanpa pemilik | Tidak jelas modul mana yang berwenang menulis |
| `KF-4` | Skema tidak lengkap | Ada `DbSet` tanpa configuration, atau ada API tanpa migration |
| `KF-5` | Alamat endpoint bentrok | Dua controller memakai grup `[Tags(...)]` atau base URL yang sama |
| `KF-6` | Enum ganda | Enum dengan makna sama didefinisikan di dua area |
| `KF-7` | Prefix tidak sesuai | Data induk diberi prefix `Trx`, atau sebaliknya |

## Daftar temuan

| ID | Jenis | Temuan | Modul terdampak | Risiko nyata bila diabaikan | Status |
| --- | --- | --- | --- | --- | --- |
| `KF-001` | `KF-4` | `MstBillingItemCategory` terdaftar sebagai `DbSet` tetapi belum ditemukan migration-nya | Billing Management | Endpoint yang memakainya akan gagal di lingkungan uji, walaupun berhasil di komputer developer | Terbuka |
| `KF-002` | `KF-2` | Konsep penjamin muncul di `TrxPatientEncounterGuarantor` dan `MstPatientCompanyGuarantor` | Registration, Patient Management | Dua modul menghitung penjamin dengan cara berbeda, sehingga tagihan bisa berbeda untuk pasien yang sama | Terbuka |

Kolom **Risiko nyata** wajib menjelaskan akibatnya bagi pengguna atau data, bukan menyebut
istilah teknis saja. Tulis "tagihan pasien bisa berbeda", bukan "terjadi inkonsistensi data".

## Status temuan

| Status | Arti |
| --- | --- |
| Terbuka | Belum dibahas siapa pun |
| Dibahas | Sudah masuk wawancara suatu modul, keputusan belum ada |
| Diputuskan | Sudah ada keputusan owner, lengkap dengan `decision_id` |
| Selesai | Sudah ditutup di kode, dibuktikan pada commit tertentu |

Temuan tidak boleh dihapus. Temuan yang sudah selesai tetap tinggal beserta bukti penutupnya,
agar tidak ditemukan ulang sebagai "masalah baru" enam bulan kemudian.
```

---

## 9. Berkas 6 — `06-indeks-entity.md`

Daftar abjad seluruh entity, satu baris satu entity. Fungsinya satu: mencari cepat.

```markdown
| Entity | Area | Modul | Tingkat | Berkas rinci |
| --- | --- | --- | --- | --- |
| `EmpAttendance` | Corporate | Attendance Management | `L4` | [02](02-entity-terdaftar.md#corporate--attendance-management) |
| `MstBed` | HealthServices | Master Data | `L3` | [02](02-entity-terdaftar.md#healthservices--master-data) |
| `MstClinic` | HealthServices | Master Data | `L4` | [02](02-entity-terdaftar.md#healthservices--master-data) |
```

---

## 10. Yang dilarang masuk registry

| Dilarang | Alasan | Tempat yang benar |
| --- | --- | --- |
| Usulan entity baru | Registry adalah fakta, bukan rencana | `docs/module-blueprints/<module>/02-backend-architecture.md` |
| Kata `wajib`, `prioritas`, `sprint` | Itu keputusan owner, bukan temuan pemindaian | Roadmap modul |
| Rekomendasi index dan constraint | Itu desain | Blueprint modul |
| Urutan implementasi | Itu perencanaan | `docs/module-blueprints/<module>/roadmap/` |
| Alur proses bisnis target | Itu hasil wawancara dan desain | `00-interview-decisions.md` dan blueprint |
| Data pasien atau pegawai asli | Privasi | Tidak di mana pun; gunakan data samaran |

Format audit sebelumnya memuat bagian "entity prioritas", "urutan implementasi", dan
"rekomendasi index". Isinya berguna, tetapi tempatnya bukan di dokumen hasil pemindaian.
Menyatukan keduanya membuat pembaca tidak dapat membedakan mana yang sudah ada dan mana yang
baru diusulkan seseorang — dan itulah sumber konflik antar modul yang paling sering terjadi.

Bila sebuah dokumen usulan sudah terlanjur ditulis dengan format lama, perlakukan seperti ini:

1. Bagian yang menyatakan keadaan sekarang dipindahkan menjadi bahan registry.
2. Bagian yang menyatakan usulan dijadikan **masukan** untuk `/grill-me` modul terkait.
3. Dokumen aslinya disimpan sebagai arsip, tidak dipakai sebagai sumber kebenaran.

---

## 11. Checklist sebelum registry dianggap selesai

1. Manifest memuat SHA kedua repository dan status kesegaran.
2. Ringkasan angka terisi, dan cara menghitungnya disebutkan.
3. Setiap entity punya satu tingkat `L0`–`L4`, tanpa status campuran.
4. Setiap baris entity punya bukti berisi path dan commit SHA.
5. Setiap entity punya modul pemilik, atau ditulis `Belum ditentukan`.
6. Seluruh data bersama tercantum di berkas 3 beserta larangan duplikasinya.
7. Setiap `⚠ Bermasalah` punya baris padanan di zona konflik.
8. Tidak ada kata `wajib`, `prioritas`, atau usulan entity baru.
9. Seluruh narasi berbahasa Indonesia dan dapat dipahami pembaca non-teknis.
