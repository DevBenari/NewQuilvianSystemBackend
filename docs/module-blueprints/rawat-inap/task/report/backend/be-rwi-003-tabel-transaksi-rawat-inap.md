# Laporan Perubahan Backend — `BE-RWI-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-003` |
| Judul | Sebelas tabel transaksi beserta empat penjaga keunikannya |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | [`02-backend-architecture.md`](../../../02-backend-architecture.md) revision `0.4` §3, §4.1 s.d. §4.11, §6, §7.1; `INV-INP-01` s.d. `INV-INP-10`; [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) revision `0.4`; `RWI-DEC-054` s.d. `RWI-DEC-057`, `RWI-DEC-065`, `RWI-DEC-073` |
| Contract version | API `0.4.0` — **tidak berubah**. Task ini tidak menyentuh satu pun endpoint |
| Commit backend | **Belum di-commit.** Seluruh berkas masih di working tree — lihat bagian 6.3 |
| Tanggal verifikasi | 24 Agustus 2026 |
| Jenis perubahan | Penambahan sebelas tabel transaksi baru. **Nol kolom pada tabel modul lain yang berubah** |
| Status | **SELESAI** — kelima acceptance criteria dan seluruh butir DoD terbukti |

---

## 1. Apa yang dibangun

### 1.1 Keadaan sebelum perubahan

`BE-RWI-001` sudah membuat dua tabel master, tetapi modul belum punya satu pun tabel transaksi.
Artinya belum ada tempat menyimpan episode, penempatan tempat tidur, penugasan DPJP, resume
pulang, maupun riwayat status. Seluruh task S1 ke atas — `BE-RWI-004` dan sesudahnya — menunggu
tabel-tabel ini berdiri.

### 1.2 Kenapa penjaga keunikannya yang menentukan

Task ini bukan sekadar membuat sebelas tabel. Yang membuatnya penting adalah **empat keadaan
mustahil** yang harus benar-benar dijadikan mustahil oleh database, bukan oleh kode:

| Keadaan yang harus mustahil | Penjaganya | Invariant |
| --- | --- | --- |
| Dua pasien menempati satu tempat tidur | `IX_InpBedPlacement_BedId_Active` | `INV-INP-02` |
| Dua episode memesan satu tempat tidur | `IX_InpBedReservation_BedId_Active` | `INV-INP-02` |
| Satu episode punya dua DPJP aktif | `IX_InpDoctorAssignment_EpisodeId_Active` | `INV-INP-03` |
| Satu pasien tercatat dirawat di dua tempat | `IX_InpEpisode_PatientId_Present` | `INV-INP-10` |

Kolom *Risk/blocker* pada roadmap menyatakannya tegas: pemeriksaan "tempat tidur kosong" di
dalam kode **tidak cukup**, karena dua transaksi dapat sama-sama lolos pemeriksaan sebelum salah
satunya menyimpan. Penjaga sebenarnya adalah unique index parsial di tingkat database.

Karena itu bagian 4.3 laporan ini tidak berhenti pada "index terbentuk", melainkan membuktikan
setiap index **benar-benar menolak** baris kedua pada PostgreSQL sungguhan.

---

## 2. Proses bisnis

### 2.1 Tujuan

Sistem punya tempat menyimpan seluruh perjalanan seorang pasien rawat inap — dari admisi dibuka,
tempat tidur dipesan dan ditempati, DPJP dan perawat ditugaskan, sampai resume pulang
ditandatangani dan episode ditutup — beserta riwayat perpindahan statusnya.

### 2.2 Pelaku

| Pelaku | Perannya |
| --- | --- |
| Petugas admisi | Membuka episode, memesan dan menempatkan tempat tidur |
| Kepala ruangan | Menugaskan perawat |
| DPJP | Menandatangani resume pulang |
| Petugas kasir | Menandai kelayakan keuangan |
| Supervisor | Membuka sesi koreksi |
| Tim Backend/API | Pemilik perubahan ini |

Tidak satu pun pelaku di atas menyentuh tabel ini pada task ini — belum ada endpoint. Yang
dibangun adalah tempat datanya.

### 2.3 Aturan bisnis yang melekat pada bentuk datanya

| Aturan | Diwujudkan sebagai | Dasar |
| --- | --- | --- |
| Satu episode menempel pada tepat satu kunjungan | `EncounterId` unique | `INV-INP-04` |
| Satu tempat tidur paling banyak satu penempatan aktif | Unique parsial `WHERE "EndDateTime" IS NULL` | `INV-INP-02` |
| Satu tempat tidur paling banyak satu pemesanan aktif | Unique parsial `WHERE "ReservationStatus" = 1` | `INV-INP-02` |
| Satu episode paling banyak satu DPJP aktif | Unique parsial `WHERE "EndDateTime" IS NULL` | `INV-INP-03` |
| Satu pasien paling banyak satu episode yang benar-benar hadir | Unique parsial atas `Admitted`, dan `DischargePending` yang kepergiannya belum dicatat | `INV-INP-10`, `RWI-DEC-054`, `RWI-DEC-055` |
| Satu episode paling banyak satu resume pulang | `EpisodeId` unique pada `InpDischargeSummary` | `INV-INP-05` |
| Kelas yang ditagihkan adalah kelas saat penempatan dibuat | `RoomId`, `ServiceUnitId`, `PatientClassId` sebagai **salinan** pada `InpBedPlacement` | `RWI-RULE-007` |
| Riwayat lokasi tidak pernah hilang | `InpBedPlacement` berbentuk berperiode, bukan kolom lokasi pada episode | `RWI-DEC-053` |
| Versi resume yang digantikan tidak boleh hilang | Tabel `InpDischargeSummaryRevision` | `RWI-DEC-057` |
| Data tidak dihapus, hanya dinonaktifkan | `IsActive` + soft delete `IdentityModel` | Konvensi project |

### 2.4 Hasil akhir

Sebelas tabel berdiri, empat keadaan mustahil benar-benar mustahil, dan kesebelasnya dapat
dibangun ulang dari database kosong maupun dibongkar kembali tanpa menyentuh tabel modul lain.

---

## 3. File yang diubah

| Berkas | Jenis | Isi |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/` | Baru | **Tujuh** enum: `InpEpisodeStatus`, `InpDischargeType`, `InpBedReservationStatus`, `InpBedPlacementEndReason`, `InpFinancialClearanceStatus`, `InpIsolationSource`, `InpStatusChangeActorType` |
| `Areas/HealthServices/InPatientManagement/Models/` | Baru | **Sebelas** model `Inp*` |
| `Repositories/Configurations/HealthServices/InPatientManagement/` | Baru | **Sebelas** konfigurasi EF |
| `Repositories/ApplicationDbContext.cs` | Disunting | 1 `using` + 11 baris `DbSet` |
| `Migrations/20260824095353_CreateInpatientTransactionTables.cs` | Baru | Dihasilkan tooling EF. 11 `CreateTable`, 78 `CreateIndex` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disunting | Snapshot sebelas entity baru |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientEnumFoundationTests.cs` | Baru | Enam test bentuk enum |
| `Program.cs` | Disunting | **Satu baris `using`, di luar scope task ini** — lihat bagian 5.2 |

Kesebelas konfigurasi terjaring otomatis oleh `ApplyConfigurationsFromAssembly` pada
`ApplicationDbContext.cs:624`, sehingga tidak perlu didaftarkan satu per satu.

Pola yang diikuti: `TrxEmergencyDisposition` beserta konfigurasinya milik modul IGD — bentuk
kolom audit, `HasConversion<int>()` untuk enum, `DeleteBehavior.Restrict` pada seluruh foreign
key, dan navigation property yang menunjuk master tanpa menyalinnya.

### 3.1 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Service, controller, DTO | `BE-RWI-004` dan sesudahnya |
| Seeder isi tabel transaksi | Tabel transaksi memang tidak di-seed |
| Kolom `OriginEncounterId` pada `TrxPatientEncounter` | **Pekerjaan modul IGD**, bukan modul ini — `RWI-DEC-073` |
| Pemeriksaan waktu tiba dari IGD | Aturan 9 Kelayakan Penempatan; jalur `INP-S09` di luar MVP — `RWI-DEC-072` |
| Navigation `MstInpatientClearanceItem` → `InpClearanceMark` | Dibentuk lewat `WithMany()` tanpa koleksi, supaya model milik MasterData tidak perlu disunting |

---

## 4. Verifikasi

Seluruh bukti di bawah dijalankan 24 Agustus 2026.

### 4.1 Pemeriksaan statis

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build Release | `dotnet build -c Release` | **Lulus**, 0 error — setelah perbaikan bagian 5.2 |
| Snapshot sinkron dengan model | `dotnet ef migrations has-pending-model-changes` | **Lulus** — `No changes have been made to the model since the last migration` |
| SQL arah maju terbentuk | `dotnet ef migrations script` | 11 `CREATE TABLE`, 78 index |
| SQL arah mundur terbentuk | perintah sama, urutan dibalik | 11 `DROP TABLE`. `Down` **bukan** badan kosong |
| **Cakupan sentuhan** | Pembacaan skrip arah maju | **Nol `ALTER TABLE`.** Tabel modul lain hanya **dirujuk** foreign key inline, tidak diubah |

Baris terakhir itulah bukti terkuat untuk acceptance criteria nomor 5. Sebelas tabel baru
merujuk `MstBed`, `MstRoom`, `MstServiceUnit`, `MstPatientClass`, `MstPatient`, `MstDoctor`,
`MstEmployee`, `MstInpatientClearanceItem`, dan `TrxPatientEncounter` — tetapi tidak satu pun
dari kesembilan tabel itu menerima kolom baru atau perubahan bentuk.

### 4.2 Bukti eksekusi pada database lokal

Migration dijalankan pada **PostgreSQL 16 sekali pakai** di dalam container Docker
(`postgres:16`, port lokal `55433`, database `qvmigtest`), dinyalakan khusus untuk uji ini lalu
**dihapus setelah selesai**. Connection string diberikan lewat `--connection`, sehingga
konfigurasi project tidak disentuh sama sekali. **Database dev bersama tidak dikenai perubahan
apa pun** — lihat bagian 6.1.

| Langkah | Hasil |
| --- | --- |
| Arah maju dari database kosong | **Lulus**, exit `0`. 474 tabel terbentuk |
| Kesebelas tabel terbentuk | Seluruhnya ada di schema `public` |
| Bentuk kolom | **251 kolom** dibandingkan satu per satu dengan kamus data — 141 kolom bisnis + 110 kolom audit. **Nol ketidakcocokan** pada nama, tipe, panjang, dan boleh-kosong. Lihat bagian 4.4 |
| Index parsial terbentuk | **Enam**, dan definisinya cocok kata demi kata dengan DDL kamus data bagian 16 |
| Arah mundur | **Lulus**, exit `0` |
| Akibat arah mundur | 474 → **463** tabel. Tepat sebelas tabel itu yang hilang |
| Tabel `BE-RWI-001` selamat | `MstInpatientSetting` dan `MstInpatientClearanceItem` masih ada sesudah rollback |
| Riwayat migration | Mundur ke `20260824053651_CreateInpatientMasterTables` |

### 4.3 Uji penolakan — bagian yang paling menentukan

Setiap index parsial diuji dengan menyisipkan baris pertama yang sah, lalu baris kedua yang
melanggar. Foreign key dimatikan sementara lewat `SET session_replication_role = replica`
supaya baris uji tidak memerlukan induk lengkap; **unique index tidak dijalankan trigger**,
sehingga tetap berlaku penuh selama uji.

| # | Yang diuji | Hasil |
| :---: | --- | --- |
| 1 | Penempatan aktif kedua pada satu tempat tidur | **DITOLAK** — `duplicate key value violates unique constraint "IX_InpBedPlacement_BedId_Active"` |
| 1b | Penempatan baru **setelah** yang lama ditutup | **DITERIMA** — index parsial tidak menghalangi perpindahan yang sah |
| 2 | Pemesanan aktif kedua pada satu tempat tidur | **DITOLAK** — `"IX_InpBedReservation_BedId_Active"` |
| 3 | DPJP aktif kedua pada satu episode | **DITOLAK** — `"IX_InpDoctorAssignment_EpisodeId_Active"` |
| 4 | Episode `Admitted` kedua untuk satu pasien | **DITOLAK** — `"IX_InpEpisode_PatientId_Present"` |
| 4b | Episode baru setelah episode lama `Closed` | **DITERIMA** — pasien yang sudah pulang boleh dirawat lagi |
| 4c | Episode baru saat episode lama `DischargePending` dan kepergiannya **sudah dicatat** | **DITERIMA** — persis pelonggaran yang dituntut `RWI-DEC-055` |
| 5 | Perawat aktif kedua pada satu episode | **DITOLAK** — `"IX_InpNurseAssignment_EpisodeId_Active"` |
| 6 | Sesi koreksi terbuka kedua pada satu episode | **DITOLAK** — `"IX_InpCorrectionSession_EpisodeId_Open"` |
| 7 | `SequenceNumber` ganda dalam satu episode | **DITOLAK** — `"IX_InpBedPlacement_EpisodeId_SequenceNumber"` |

Uji 1b, 4b, dan 4c sengaja dimasukkan. Index yang menolak segalanya mudah dibuat; yang sulit
adalah index yang menolak tepat pada keadaan yang salah dan **membiarkan** yang benar. Ketiga
uji itu membuktikan perpindahan tempat tidur, admisi ulang, dan pelonggaran `RWI-DEC-055`
tidak ikut terhalang.

### 4.4 Perbandingan kamus data dengan kenyataan database

| Tabel | Kolom di kamus | Kolom di database | Hasil |
| --- | ---: | ---: | :---: |
| `InpEpisode` | 25 | 35 | COCOK |
| `InpDoctorAssignment` | 9 | 19 | COCOK |
| `InpNurseAssignment` | 8 | 18 | COCOK |
| `InpBedReservation` | 9 | 19 | COCOK |
| `InpBedPlacement` | 14 | 24 | COCOK |
| `InpDischargeSummary` | 12 | 22 | COCOK |
| `InpDischargeSummaryRevision` | 17 | 27 | COCOK |
| `InpClearanceMark` | 7 | 17 | COCOK |
| `InpFinancialClearance` | 9 | 19 | COCOK |
| `InpStatusHistory` | 11 | 21 | COCOK |
| `InpCorrectionSession` | 10 | 20 | COCOK |

Selisih tetap **sepuluh kolom** pada setiap tabel adalah kolom audit `IdentityModel`, yang
memang sengaja tidak diulang pada kamus data. Perbandingan dilakukan otomatis atas nama kolom,
tipe PostgreSQL, panjang `varchar`, dan boleh-kosong.

### 4.5 Test

| Test | Jumlah | Hasil |
| --- | ---: | --- |
| `InpatientEnumFoundationTests` | 6 | Lulus |
| `BillingModuleFoundationTests` (regresi) | 2 | Lulus |
| **Total** | **8** | `Failed: 0, Passed: 8` |

Test enum menjaga hal yang tidak terlihat saat membaca kode: angka enum tersimpan sebagai `int`
di database, sehingga menyisipkan nilai baru di tengah akan menggeser arti baris yang sudah
tersimpan. Termasuk yang dijaga: `InpEpisodeStatus` tepat lima nilai tanpa `InCare`, dan
`InpDischargeType` yang **menyisakan** angka 4 dan 5 untuk cara pulang meninggal dan kabur.

### 4.6 Acceptance criteria

| # | Kriteria | Status | Bukti |
| :---: | --- | :---: | --- |
| 1 | Kesebelas tabel terbentuk sesuai kamus data, termasuk enam kolom kebutuhan isolasi pada `InpEpisode` | ✅ | Bagian 4.4 — 251 kolom cocok, nol ketidakcocokan. Keenam kolom isolasi (`RequiresIsolation`, `IsolationSource`, `IsolationSetByUserId`, `IsolationSetByDoctorId`, `IsolationSetAt`, `IsolationNote`) ada dan bertipe benar |
| 2 | Empat unique index parsial terbentuk | ✅ | Bagian 4.3 — keempatnya terbentuk **dan terbukti menolak** pada database sungguhan. Dua index parsial tambahan ikut dibuat; lihat bagian 5.1 |
| 3 | `InpEpisodeStatus` memuat tepat lima nilai; `InCare` tidak ada | ✅ | Bagian 4.5, tiga test terpisah |
| 4 | Migration maju dan mundur berhasil | ✅ | Bagian 4.2 — keduanya exit `0`; 474 → 463 tabel saat mundur |
| 5 | Tidak ada kolom tabel modul lain yang berubah | ✅ | Bagian 4.1 — **nol `ALTER TABLE`** pada skrip arah maju |

### 4.7 Definition of Done

| Butir DoD | Status |
| --- | :---: |
| Sebelas tabel | ✅ |
| Empat index parsial | ✅ — enam terbentuk, empat di antaranya yang diminta |
| Satu migration | ✅ |
| Keempat test penolakan lulus | ✅ — sepuluh uji, tujuh penolakan dan tiga penerimaan |
| Uji maju-mundur lulus | ✅ |
| Kamus data dan kenyataan database cocok kolom demi kolom | ✅ — bagian 4.4 |

---

## 5. Penyimpangan terhadap roadmap

### 5.1 Enam index parsial, bukan empat

Acceptance criteria menyebut **empat** unique index parsial. Kamus data menuntut **enam**:

| Index | Diminta acceptance criteria? | Diminta kamus data? |
| --- | :---: | :---: |
| `IX_InpBedPlacement_BedId_Active` | Ya | Ya |
| `IX_InpBedReservation_BedId_Active` | Ya | Ya |
| `IX_InpDoctorAssignment_EpisodeId_Active` | Ya | Ya |
| `IX_InpEpisode_PatientId_Present` | Ya | Ya |
| `IX_InpNurseAssignment_EpisodeId_Active` | **Tidak** | Ya — bagian 3, kolom `EndDateTime` |
| `IX_InpCorrectionSession_EpisodeId_Open` | **Tidak** | Ya — bagian 11, kolom `ClosedAt` |

Keduanya dibuat, karena kamus data menyebutnya tegas dan DDL bagian 16 menyatakan enam tabel
sisanya mengikuti "unique index sesuai kolom Index pada tabel kamus data di atas". Angka empat
pada acceptance criteria menghitung index yang menjaga invariant bernomor; dua sisanya menjaga
aturan yang tidak diberi nomor invariant.

**Yang perlu diperhatikan pemilik pekerjaan:** `IX_InpNurseAssignment_EpisodeId_Active` berarti
satu episode hanya boleh punya **satu perawat aktif pada satu waktu**. Bila kenyataannya satu
pasien dapat dipegang lebih dari satu perawat sekaligus — misalnya perawat penanggung jawab dan
perawat pendamping — aturan itu keliru dan harus dicabut lewat keputusan, bukan lewat kode.
Kamus data yang menetapkannya, jadi perubahannya kembali ke `/qv-design`.

### 5.2 Satu perbaikan di luar scope: `Program.cs` tidak dapat dibangun

Build gagal pada **keadaan awal**, sebelum satu baris pun kode task ini ditulis:

```
Program.cs(273,32): error CS0246: The type or namespace name 'LabOrderService'
could not be found (are you missing a using directive or an assembly reference?)
```

`LabOrderService` ada di `Areas/HealthServices/LaboratoryManagement/Services/`, tetapi
`Program.cs` memanggilnya tanpa `using` namespace itu. Menurut `git log`, keduanya masuk lewat
commit `f69e9e4`, jauh sebelum task ini — bukan akibat perubahan di sini.

Satu baris `using` ditambahkan supaya build berjalan, karena tanpa build yang lulus **task ini
mustahil diverifikasi sama sekali**. Perbaikan ini di luar scope `BE-RWI-003` dan sengaja
dilaporkan terpisah supaya pemilik pekerjaan dapat memutuskan: dibiarkan menempel pada commit
task ini, atau dipisah menjadi commit perbaikan tersendiri atas nama pemilik `LaboratoryManagement`.

### 5.3 Penempatan folder konfigurasi

Roadmap menulis `Repositories/Configurations/HealthService/InPatientManagement/` — **tanpa `s`**.
Folder yang benar-benar dipakai project adalah `HealthServices/` (jamak), sejalan dengan
`HealthServices/EmergencyInstallationManagement/` dan dengan penempatan `BE-RWI-001`. Kesebelas
konfigurasi ditempatkan di folder jamak. Ini salah ketik pada roadmap, bukan keputusan desain.

---

## 6. Yang perlu perhatian pemilik pekerjaan

### 6.1 Jebakan connection string masih ada

Peringatan bagian 6.1 laporan `BE-RWI-001` **masih berlaku dan terbukti relevan**:
`appsettings.json` tidak punya blok `ConnectionStrings`, sedangkan
`appsettings.Development.json` mengarah ke server dev bersama. `dotnet ef database update` yang
dijalankan tanpa `--connection` akan mengenai database bersama.

Task ini membawa **sebelas tabel**. Seluruh perintah migration pada laporan ini memakai
`--connection` ke container sekali pakai, dan containernya sudah dihapus.
**Migration `CreateInpatientTransactionTables` belum pernah diterapkan ke `QuilvianNewDevTim01`
maupun database lain mana pun.**

Saran yang sama seperti sebelumnya masih menunggu keputusan: sediakan profil lokal atau skrip
pembungkus, supaya keselamatan ini tidak bergantung pada ingatan orang yang mengetik perintah.

### 6.2 Project Tests berada di dalam folder project web

Build sempat gagal berulang dengan pesan `MSB3030`, menunjuk jalur bersarang seperti
`QuilvianSystemBackend.Tests/bin/Release/net9.0/QuilvianSystemBackend.Tests/bin/Release/...`.

Sebabnya: `QuilvianSystemBackend.Tests/` berada **di dalam** folder `QuilvianSystemBackend`,
sedangkan project web menyapu isi foldernya sendiri sebagai `Content`. Setiap build menyalin
keluaran Tests ke dalam keluaran web, dan salinan itu bertumpuk sampai jalurnya melampaui batas.
Membersihkan `bin` dan `obj` milik Tests memulihkannya, tetapi masalahnya akan kembali.

Ini di luar scope task ini. Perbaikan yang wajar: pindahkan project Tests keluar dari folder
project web, atau tambahkan `Content Remove` untuk foldernya pada `QuilvianSystemBackend.csproj`.

### 6.3 Belum di-commit

Seluruh berkas masih berada di working tree:

| Keadaan | Berkas |
| --- | --- |
| Baru | `Areas/HealthServices/InPatientManagement/` (18 berkas), `Repositories/Configurations/HealthServices/InPatientManagement/` (11 berkas), dua berkas migration, satu berkas test |
| Disunting | `Repositories/ApplicationDbContext.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs`, `Program.cs` |

Sesuai kesepakatan kerja, `add`, `commit`, dan `push` dijalankan sendiri oleh pemilik pekerjaan.
Pertimbangkan memisahkan `Program.cs` menjadi commit tersendiri — lihat bagian 5.2.

---

## 7. Langkah berikutnya

| Urutan | Task | Keterangan |
| :---: | --- | --- |
| 1 | Commit berkas task ini | Lihat bagian 6.3 |
| 2 | `BE-RWI-002` | Seeder dua tabel master. Dependency-nya sudah terpenuhi sejak `BE-RWI-001` |
| 3 | `BE-RWI-004` | Enam service dan nomor episode. Dependency-nya baru terpenuhi oleh task ini |

---

## 8. Risiko tersisa

| Risiko | Sifat | Pemilik |
| --- | --- | --- |
| Index parsial **tidak** menyaring `IsDelete`, berbeda dari pola `TrxAttendance*` yang memakai `HasFilter("\"IsDelete\" = false")` | Mengikuti DDL kamus data apa adanya. Akibatnya penempatan yang di-soft-delete tetap menahan tempat tidurnya. Arahnya **aman** — gagal menolak lebih berbahaya daripada gagal menerima — tetapi bila ternyata tidak dikehendaki, perubahannya milik `/qv-design`, bukan implementasi | Product/Domain bersama Backend/API |
| `IX_InpNurseAssignment_EpisodeId_Active` membatasi satu perawat aktif per episode | Lihat bagian 5.1. Perlu dipastikan cocok dengan kenyataan ruangan | Product/Domain bersama pemilik klinis |
| Tidak ada test yang menjalankan index parsial di dalam rangkaian test otomatis | Bukti pada bagian 4.3 dijalankan sebagai SQL terhadap PostgreSQL sungguhan, bukan sebagai xunit — provider InMemory yang dipakai project **tidak menegakkan unique index sama sekali**, sehingga test xunit di atasnya akan lulus palsu. Membangun harness PostgreSQL untuk test adalah pekerjaan tersendiri | Backend/API, terkait `RWI-DEC-051` dan `BE-RWI-033` |
| `Program.cs` diperbaiki di luar scope | Lihat bagian 5.2 | Backend/API |
