# Laporan Perubahan Backend — `BE-RWI-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-001` |
| Judul | Dua tabel master Rawat Inap ada di database |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`; `02-backend-architecture.md` §4.12, §4.13, §7.1 langkah 1; `erd/02-inpatient-configuration.md` |
| Contract version | API `0.3.0` — **tidak berubah**. Task ini tidak menyentuh satu pun endpoint |
| Commit backend | `8a96aa03a9a2c0bc1c4d257ff0e8d339d544b4d9` (model, konfigurasi, `DbSet`) pada branch `MHamzah`. Berkas migration **belum di-commit** — lihat bagian 6.2 |
| Tanggal verifikasi | 24 Agustus 2026 |
| Jenis perubahan | Penambahan dua tabel master baru. Tidak ada tabel modul lain yang tersentuh |
| Status | **SELESAI** — keempat acceptance criteria dan seluruh butir DoD terbukti |

---

## 1. Apa yang dibangun, dan kenapa ini task pertama

### 1.1 Keadaan sebelum perubahan

Folder `Areas/HealthServices/InPatientManagement/` tidak ada sama sekali. Modul Rawat Inap belum
punya satu baris pun kode. Yang lebih menentukan urutan: modul ini nanti memakai **tujuh angka
batas waktu** — berapa lama pemesanan tempat tidur mengunci, berapa lama episode `Draft` boleh
telantar, berapa jam target pengkajian awal, dan seterusnya.

Angka seperti itu punya dua tempat tinggal yang mungkin. Ditanam di dalam kode, atau disimpan di
tabel master yang dapat diubah admin. `RWI-DEC-008` memilih yang kedua, dan alasannya bukan soal
kerapian:

> **Contoh:** rumah sakit memutuskan pemesanan tempat tidur cukup mengunci 90 menit, bukan 120.
> Bila angka `120` ditanam di `InpBedReservationService`, perubahan itu berarti mengubah kode,
> membangun ulang, dan menurunkan aplikasi sebentar. Bila angka itu satu baris di
> `MstInpatientSetting`, admin mengubahnya sendiri dari layar pengaturan dan berlaku pada
> pembacaan berikutnya.

Hal yang sama berlaku pada daftar butir administrasi yang menahan penutupan episode
(`RWI-DEC-026`). Butirnya **daftar baris**, bukan satu nilai, sehingga sengaja dipisah menjadi
tabelnya sendiri dan tidak disatukan ke tabel pengaturan.

### 1.2 Kenapa dikerjakan paling awal

`BE-RWI-002` (seeder) mengisi kedua tabel ini. `BE-RWI-004` (enam service) membaca angkanya.
Seluruh task S1 ke atas memakai angka itu untuk memutuskan sesuatu. Selama tabelnya belum ada,
tidak satu pun dari ketiganya dapat dikerjakan tanpa menanam angka di kode — yang justru
dilarang `RWI-DEC-008`.

Task ini juga **tidak** membuat tabel transaksi. Kesebelas tabel `Inp*` adalah `BE-RWI-003`,
task terpisah. Pemisahan itu disengaja supaya kegagalan pada salah satunya tidak menyeret yang lain.

---

## 2. Proses bisnis

### 2.1 Tujuan

Sistem punya tempat menyimpan angka batas waktu dan daftar butir administrasi, sehingga tidak ada
satu pun angka modul Rawat Inap yang perlu ditanam di kode.

### 2.2 Pelaku

| Pelaku | Perannya |
| --- | --- |
| Admin Rawat Inap | Mengubah angka batas waktu; menambah dan menonaktifkan butir administrasi |
| Petugas ruangan | Tidak menyentuh tabel ini langsung. Merasakan akibatnya lewat aturan yang dijalankan sistem |
| Tim Backend/API | Pemilik perubahan ini |

### 2.3 Aturan bisnis yang melekat pada bentuk datanya

| Aturan | Diwujudkan sebagai | Dasar |
| --- | --- | --- |
| Hanya ada satu baris pengaturan yang dipakai | `Code` unique + `IsDefault` | `RWI-DEC-008` |
| Pemesanan tempat tidur mengunci 2 jam | `BedReservationMinutes` bawaan `120` | `RWI-RULE-002` |
| Episode `Draft` telantar gugur setelah 1 hari | `DraftEpisodeExpiryHours` bawaan `24` | `RWI-RULE-022` |
| Nomor episode berawalan `RI` | `EpisodeNumberPrefix` | Konvensi, mengikuti `IGD` |
| Satu butir administrasi tidak boleh terdaftar dua kali | `ItemCode` unique | `RWI-DEC-026` |
| Butir tidak wajib tidak menahan penutupan | `IsMandatory` | `RWI-RULE-018`, `RWI-RULE-024` |
| Butir yang tidak berlaku lagi dinonaktifkan, bukan dihapus | `IsActive` + soft delete `IdentityModel` | `RWI-DEC-032` |

### 2.4 Hasil akhir

Kedua tabel berdiri kosong di database. Isinya menyusul lewat `BE-RWI-002`. Tidak ada satu pun
perilaku aplikasi yang berubah hari ini — task ini murni menyiapkan tempat.

---

## 3. File yang diubah

| Berkas | Jenis | Isi |
| --- | --- | --- |
| `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs` | Baru | 12 properti bisnis + audit `IdentityModel` |
| `Areas/HealthServices/MasterData/Models/MstInpatientClearanceItem.cs` | Baru | 7 properti bisnis + audit `IdentityModel` |
| `Repositories/Configurations/HealthServices/MasterData/MstInpatientSettingConfiguration.cs` | Baru | Panjang kolom, unique `Code`, index `(IsActive, IsDefault)` |
| `Repositories/Configurations/HealthServices/MasterData/MstInpatientClearanceItemConfiguration.cs` | Baru | Panjang kolom, unique `ItemCode`, index `IsMandatory` dan `IsActive` |
| `Repositories/ApplicationDbContext.cs` | Disunting | 2 baris `DbSet` (baris 547–548) |
| `Migrations/20260824053651_CreateInpatientMasterTables.cs` | Baru | Dihasilkan tooling EF, bukan tulisan tangan |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disunting | Snapshot dua entity baru |

Kedua konfigurasi terjaring otomatis oleh `ApplyConfigurationsFromAssembly` pada
`ApplicationDbContext.cs:621`, sehingga tidak perlu didaftarkan satu per satu.

Pola yang diikuti: `MstEmergencySetting` milik modul IGD — bentuk kolom audit, soft delete, dan
cara konfigurasi EF disalin apa adanya, sesuai kolom *Reuse* pada roadmap.

### 3.1 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Seeder isi kedua tabel | `BE-RWI-002`, task terpisah |
| Controller dan service master | `BE-RWI-004` dan `BE-RWI-005` |
| Sebelas tabel `Inp*` | `BE-RWI-003` |
| Relasi `MstInpatientClearanceItem` → `InpClearanceMark` | Tabel lawannya belum ada; dibentuk pada `BE-RWI-003` |

---

## 4. Verifikasi

Seluruh bukti di bawah dijalankan 24 Agustus 2026.

### 4.1 Pemeriksaan statis

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build Release | `dotnet build -c Release` | **Lulus** — dipastikan pemilik pekerjaan sebelum verifikasi ini dijalankan |
| Snapshot sinkron dengan model | `dotnet ef migrations has-pending-model-changes` | **Lulus** — `No changes have been made to the model since the last migration` |
| SQL arah maju terbentuk | `dotnet ef migrations script <sebelumnya> <migration ini>` | 2 `CREATE TABLE`, 5 index, 1 baris `__EFMigrationsHistory` |
| SQL arah mundur terbentuk | perintah sama, urutan dibalik | 2 `DROP TABLE` + hapus baris history. `Down` **bukan** badan kosong |
| Cakupan sentuhan | Pembacaan kedua skrip | Tidak satu pun nama tabel selain `MstInpatientSetting` dan `MstInpatientClearanceItem` muncul |

### 4.2 Bukti eksekusi pada database lokal

Migration dijalankan pada **PostgreSQL 16.15 sekali pakai** di dalam container Docker
(`postgres:16`, port lokal `55432`, database `qvmigtest`), dinyalakan khusus untuk uji ini lalu
dihapus setelah selesai. Connection string diberikan lewat `--connection`, sehingga konfigurasi
project tidak disentuh sama sekali. **Database dev bersama tidak dikenai perubahan apa pun** —
lihat bagian 6.1.

| Langkah | Hasil |
| --- | --- |
| Arah maju dari database kosong | **Lulus**, exit `0`. Ke-86 migration terpasang, 462 tabel terbentuk |
| Kedua tabel terbentuk | `MstInpatientSetting` dan `MstInpatientClearanceItem` ada di schema `public` |
| Bentuk kolom `MstInpatientSetting` | 22 kolom — 12 kolom bisnis + 10 kolom audit. **Cocok kolom demi kolom** dengan `erd/data-dictionary.md` bagian 12 |
| Bentuk kolom `MstInpatientClearanceItem` | 7 kolom bisnis + audit. **Cocok** dengan bagian 13 |
| Index terbentuk | 7 index: 2 PK, `IX_MstInpatientSetting_Code` **unique**, `IX_MstInpatientSetting_IsActive_IsDefault`, `IX_MstInpatientClearanceItem_ItemCode` **unique**, `IX_..._IsMandatory`, `IX_..._IsActive` |
| Unique `ItemCode` benar-benar menegakkan | Baris pertama `ADM-DOC` masuk; baris kedua `ADM-DOC` **ditolak database**: `duplicate key value violates unique constraint "IX_MstInpatientClearanceItem_ItemCode"` |
| Arah mundur | **Lulus**, exit `0` |
| Akibat arah mundur | 462 → **460** tabel. Tepat dua tabel itu yang hilang, tidak ada tabel modul lain yang ikut terhapus. `__EFMigrationsHistory` mundur ke `20260818084734_AddTriageSlaBreachMarker` |

Bahwa arah maju dijalankan dari database **benar-benar kosong** memberi bukti tambahan yang tidak
diminta task ini: rantai 86 migration project ini utuh dan dapat dibangun ulang dari nol.

### 4.3 Acceptance criteria

| # | Kriteria | Status | Bukti |
| :---: | --- | :---: | --- |
| 1 | `MstInpatientSetting` memuat kedelapan kolom pada arsitektur §8.1 dengan tipe sesuai kamus data | ✅ | Kedelapan kolom ada (`Code`, `Name`, `BedReservationMinutes`, `DraftEpisodeExpiryHours`, `InitialAssessmentTargetHours`, `ProgressNoteVerificationTargetHours`, `PendingClosureThresholdHours`, `EpisodeNumberPrefix`), ditambah `IsDefault`, `IsActive`, `Notes` yang memang tercantum pada kamus data bagian 12. Tipe dan panjang cocok |
| 2 | `MstInpatientClearanceItem` memuat `ItemCode` unik dan `IsMandatory` | ✅ | Unique index terbentuk **dan terbukti menolak** duplikat pada database sungguhan, bukan sekadar terdaftar di konfigurasi |
| 3 | Migration maju dan mundur berhasil pada database lokal | ✅ | Keduanya exit `0` pada PostgreSQL 16 lokal; keadaan sebelum dan sesudah diperiksa lewat `information_schema` |
| 4 | Tidak ada tabel modul lain yang tersentuh | ✅ | Skrip SQL kedua arah hanya menyebut dua tabel itu; rollback menghapus tepat 2 dari 462 tabel |

### 4.4 Definition of Done

| Butir DoD | Status |
| --- | :---: |
| Dua model | ✅ |
| Dua konfigurasi EF | ✅ |
| Dua `DbSet` | ✅ |
| Satu migration | ✅ |
| Uji maju-mundur lulus | ✅ |
| Build lulus | ✅ |
| Laporan menyatakan migration belum diterapkan di luar lokal | ✅ — bagian 6.1 |
| Kamus data dan kenyataan database cocok kolom demi kolom | ✅ — bagian 4.2 |

---

## 5. Penyimpangan terhadap roadmap

**Tidak ada penyimpangan pada bentuk data.** Satu catatan penempatan berkas:

Roadmap tidak menyebut folder konfigurasi untuk task ini. Kedua konfigurasi ditempatkan di
`Repositories/Configurations/HealthServices/MasterData/`, sedangkan puluhan konfigurasi master
lain (`MstBedConfiguration.cs`, `MstRoomConfiguration.cs`, dan seterusnya) berada satu tingkat di
atasnya, langsung di `HealthServices/`. Penempatan baru ini **lebih rapi**, bukan lebih kacau, dan
sejalan dengan arah perapian `BE-IGD-013`. Yang perlu diputuskan: apakah konfigurasi master lain
menyusul dipindahkan ke sana, atau kedua berkas ini yang dikembalikan sejajar. Selama belum
diputuskan tidak ada akibat teknis apa pun — `ApplyConfigurationsFromAssembly` menjaring keduanya
di mana pun letaknya.

---

## 6. Yang perlu perhatian pemilik pekerjaan

### 6.1 Tidak ada connection string lokal di project ini

Temuan ini muncul saat menyiapkan uji migration, dan berlaku jauh melampaui task ini.

| Berkas | Isi `ConnectionStrings` |
| --- | --- |
| `appsettings.Development.json` | `DefaultConnection` mengarah ke **server dev bersama**, database `QuilvianNewDevTim01` |
| `appsettings.json` | **Tidak punya blok `ConnectionStrings` sama sekali** |

Akibatnya `dotnet ef database update` yang dijalankan apa adanya — tanpa `--connection` —
**mengenai database dev bersama**, bukan database lokal. Padahal kolom *Risk/blocker*
`BE-RWI-001` berbunyi: migration tidak boleh diterapkan ke database mana pun selain lokal tanpa
izin tertulis.

Uji pada laporan ini menghindarinya dengan menyalakan Postgres sekali pakai dan memberikan
connection string lewat `--connection`. **Migration `CreateInpatientMasterTables` belum pernah
diterapkan ke `QuilvianNewDevTim01` maupun database lain mana pun.**

> **Saran, bukan bagian dari task ini:** sediakan cara baku menjalankan migration terhadap
> database lokal — misalnya profil `appsettings.Local.json` yang tidak ikut terlacak Git, atau
> satu skrip pembungkus. Tanpa itu, setiap task bermigration berikutnya menghadapi jebakan yang
> sama. `BE-RWI-003` membawa sebelas tabel dan empat unique index parsial; sekali saja lupa
> mengetik `--connection`, kesebelasnya mendarat di database bersama.

### 6.2 Berkas migration belum di-commit

Commit `8a96aa0` memuat lima berkas: dua model, dua konfigurasi, dan `ApplicationDbContext.cs`.
Ketiga berkas migration — `20260824053651_CreateInpatientMasterTables.cs`, `.Designer.cs`, dan
`ApplicationDbContextModelSnapshot.cs` — masih berada di working tree dan **belum masuk commit**.

Ini bukan kesalahan implementasi, tetapi perlu diselesaikan: rekan yang menarik branch `MHamzah`
hari ini mendapat model dan konfigurasi tanpa migration-nya, sehingga
`has-pending-model-changes` di mesin mereka akan melaporkan ada perubahan model yang belum
bermigration.

Sesuai kesepakatan kerja, `add`, `commit`, dan `push` dijalankan sendiri oleh pemilik pekerjaan.

---

## 7. Langkah berikutnya

| Urutan | Task | Keterangan |
| :---: | --- | --- |
| 1 | Commit ketiga berkas migration | Lihat bagian 6.2 |
| 2 | `BE-RWI-002` | Seeder mengisi satu baris `DEFAULT` dan tiga butir administrasi. Dependency-nya (task ini) sudah terpenuhi |
| 3 | `BE-RWI-003` | Sebelas tabel transaksi. Baca dulu bagian 6.1 sebelum menjalankan migration-nya |

---

## 8. Risiko tersisa

| Risiko | Sifat | Pemilik |
| --- | --- | --- |
| `InitialAssessmentTargetHours` dan `ProgressNoteVerificationTargetHours` bersumber dari `RWI-RULE-021` yang **belum final secara klinis** | Nilai bawaan `24` sudah tertanam sebagai default properti model. Dapat diubah admin, jadi tidak menahan MVP — tetapi menahan pemakaian untuk pasien sungguhan | Product/Domain bersama pemilik klinis |
| Tidak ada berkas test Rawat Inap di repository | Task ini tidak mensyaratkannya — kolom *Verification* meminta uji migration, bukan unit test. Tetapi `RWI-DEC-051` mewajibkan test menempel pada tiap task, sehingga utang ini terbawa ke `BE-RWI-033` | Backend/API |
| Jebakan connection string pada bagian 6.1 | Berlaku pada setiap task bermigration berikutnya | Backend/API |
