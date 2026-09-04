# Laporan Perubahan Backend — `BE-RWI-002`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> Yang **belum** berubah: acceptance criteria dan DoD task ini tetap belum terbukti penuh —
> build hijau bukan tanda selesai — sehingga tandanya pada roadmap tetap 🟡.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-002` |
| Judul | Data master awal terisi tanpa menebak isi khas rumah sakit |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `RWI-DEC-048`; `02-backend-architecture.md` §8.1, §8.2, §8.4; `RWI-AC-106` |
| Contract version | API `0.4.0` — **tidak berubah**. Task ini tidak menyentuh satu pun endpoint |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `0869775` |
| Tanggal pengerjaan | 24 Agustus 2026 |
| Jenis perubahan | Penambahan satu seeder baru beserta pendaftarannya. Tidak ada tabel maupun endpoint modul lain yang tersentuh |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Lihat bagian 4.1 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. Karena itu `dotnet build` dan `dotnet test` **tidak dijalankan**
> pada sesi ini. Seluruh baris "lulus" yang biasanya ada pada bagian Verifikasi diganti
> menjadi **belum dijalankan**, dan task ini **belum boleh ditandai selesai** sebelum
> keduanya benar-benar dijalankan. Lihat bagian 4.1 dan 7.

---

## 1. Apa yang dibangun, dan kenapa

### 1.1 Keadaan sebelum perubahan

`BE-RWI-001` sudah membuat dua tabel master Rawat Inap, tetapi keduanya **berdiri kosong**:

| Tabel | Isinya sebelum task ini |
| --- | --- |
| `MstInpatientSetting` | 0 baris |
| `MstInpatientClearanceItem` | 0 baris |

Modul dengan tabel master kosong tidak dapat dipakai sama sekali. Layar pengaturan menampilkan
daftar kosong, dan setiap service yang membaca angka batas waktu tidak menemukan apa pun.

### 1.2 Kenapa isinya di-seed, bukan diketik ulang tiap lingkungan

Ada delapan angka dan tiga butir administrasi yang nilainya sudah ditetapkan pada blueprint.
Meminta setiap pengembang mengetiknya ulang di lingkungannya sendiri menghasilkan tiga akibat
yang semuanya buruk: nilainya berbeda antar-mesin, kesalahan ketik tidak ketahuan, dan
lingkungan uji tidak dapat dipercaya sebagai gambaran lingkungan lain.

Seeder menghapus ketiganya sekaligus.

### 1.3 Dua hal yang sengaja **tidak** dilakukan seeder ini

Ini bagian yang paling menentukan bentuk task ini, dan keduanya berasal dari `RWI-DEC-048`.

**Pertama, seeder menolak berjalan di lingkungan produksi.**

> **Contoh.** Admin rumah sakit sudah menyetel batas pemesanan tempat tidur menjadi 90 menit
> pada bulan Juli, karena ruangan mereka padat. Bulan Agustus aplikasi dinaikkan versinya dan
> dinyalakan ulang. Kalau seeder ikut berjalan di produksi dan menimpa nilai, batasnya kembali
> menjadi 120 menit tanpa ada satu pun orang yang memutuskannya. Tidak ada layar yang
> menampilkan hal itu sebagai kesalahan — pemesanan hanya "terasa" lebih longgar, dan tidak
> ada yang tahu kenapa.
>
> Seeder ini bahkan tidak sampai ke pertanyaan menimpa atau tidak: di produksi ia berhenti
> sebelum membaca tabel mana pun.

**Kedua, seeder tidak pernah membuat kamar maupun tempat tidur.**

> **Contoh.** Seeder yang "membantu" dengan membuat kamar `MELATI-01` sampai `MELATI-10` dan
> tempat tidur `A` sampai `D` di tiap kamar terlihat rapi di layar. Masalahnya muncul dua
> minggu kemudian: rumah sakit ini sebenarnya memakai penamaan `3A-101`, dan kamar Melati
> mereka hanya ada empat. Petugas terlanjur menempatkan pasien di tempat tidur karangan itu,
> dan sekarang ada catatan penempatan yang menunjuk kamar yang tidak ada wujudnya.
>
> Susunan kamar dan tempat tidur khas tiap rumah sakit. Yang mengisinya adalah Admin Master
> Data lewat layar, tercatat sebagai `RWI-AC-107`.

---

## 2. Proses bisnis

### 2.1 Tujuan

Modul Rawat Inap dapat dinyalakan di lingkungan pengembangan dan pengujian tanpa satu pun layar
menampilkan daftar pilihan kosong, dan tanpa seeder ikut mengarang data yang seharusnya
ditetapkan tiap rumah sakit sendiri.

### 2.2 Pelaku

| Pelaku | Perannya |
| --- | --- |
| Tim Backend/API | Menyalakan seeder lewat konfigurasi saat menyiapkan lingkungan pengembangan |
| Admin Rawat Inap | Mengubah angka hasil seeder lewat layar pengaturan bila perlu. Nilai hasil seeder adalah titik awal, bukan keputusan akhir |
| Admin Master Data | Mengisi kamar dan tempat tidur. **Bukan** pekerjaan seeder ini |
| Pemilik klinis | Wajib meninjau dua angka yang belum final. Lihat bagian 8 |

### 2.3 Pemicu

Aplikasi dinyalakan dengan konfigurasi `Seeders:RunInpatientMasterDataSeed` bernilai `true`.
Bawaannya `false`, sehingga menjalankan aplikasi apa adanya **tidak** menulis apa pun.

### 2.4 Prasyarat

| Prasyarat | Alasan |
| --- | --- |
| Migration `CreateInpatientMasterTables` sudah diterapkan | Tabelnya harus ada lebih dulu — `BE-RWI-001` |
| Ada akun `SuperAdmin` di database | Seeder mencatat siapa pembuat barisnya pada kolom audit `CreateBy` |
| Lingkungan **bukan** produksi | `RWI-DEC-048` |

### 2.5 Langkah utama

1. Aplikasi menyala dan membaca konfigurasi `Seeders:RunInpatientMasterDataSeed`.
2. Bila bernilai `false`, seeder dilewati sepenuhnya. Selesai.
3. Bila bernilai `true`, aplikasi mencari akun `SuperAdmin`. Bila tidak ada, aplikasi berhenti
   dengan pesan yang menyebut alasannya.
4. Seeder memeriksa nama lingkungan. Bila produksi, seeder berhenti, menulis satu peringatan
   pada log, dan tidak menyentuh tabel mana pun.
5. Seeder memeriksa apakah baris pengaturan berkode `DEFAULT` sudah ada. Bila sudah, bagian ini
   dilewati beserta alasannya.
6. Bila belum ada, satu baris pengaturan ditambahkan dengan delapan nilai pada bagian 3.2.
7. Seeder membaca seluruh kode butir administrasi yang sudah ada di tabel.
8. Untuk setiap butir bawaan, butir yang kodenya sudah ada dilewati; yang belum ada ditambahkan.
9. Seeder mengembalikan ringkasan berisi berapa baris yang benar-benar ditambahkan dan bagian
   mana yang dilewati beserta alasannya. Ringkasan itu ditulis ke log.

### 2.6 Aturan bisnis yang melekat

| Aturan | Diwujudkan sebagai | Dasar |
| --- | --- | --- |
| Seeder tidak berjalan di produksi | Pemeriksaan nama lingkungan sebelum menyentuh tabel | `RWI-DEC-048` |
| Seeder tidak pernah menimpa data yang sudah ada | Pemeriksaan `Code` dan `ItemCode` sebelum menyisipkan | `RWI-DEC-048` |
| Menjalankan seeder dua kali aman | Idempotensi yang sama seperti di atas | Acceptance criteria 3 |
| Seeder tidak membuat kamar dan tempat tidur | Tidak ada satu baris pun kode yang menyentuh `MstRoom` atau `MstBed` | `RWI-DEC-048` |
| Hanya ada satu baris pengaturan | Kode `DEFAULT` + unique index `IX_MstInpatientSetting_Code` | `RWI-DEC-008` |
| Satu butir tidak terdaftar dua kali | `ItemCode` unik | `RWI-DEC-026` |

### 2.7 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Lingkungan produksi | Seeder berhenti, menulis peringatan, tidak menulis apa pun |
| Tidak ada akun `SuperAdmin` | Aplikasi berhenti menyala dengan pesan `Seeder master data Rawat Inap membutuhkan akun SuperAdmin.` Ini disengaja: baris tanpa jejak siapa pembuatnya tidak dapat diaudit |
| Baris `DEFAULT` sudah ada | Bagian pengaturan dilewati; alasannya ditulis sebagai peringatan pada log |
| Sebagian butir sudah ada | Hanya yang belum ada yang ditambahkan; jumlah yang dilewati ditulis pada log |
| Baris `DEFAULT` pernah dihapus lunak | Tetap dianggap ada. Kodenya masih menempati index unik di database, sehingga menyisipkan baris kedua akan menghentikan aplikasi saat menyala |

### 2.8 Hasil akhir

Setelah seeder berjalan sekali di lingkungan pengembangan:

| Tabel | Isinya |
| --- | --- |
| `MstInpatientSetting` | 1 baris berkode `DEFAULT` |
| `MstInpatientClearanceItem` | 3 baris: `ADM-DOC`, `RETURN-ITEM`, `DISCHARGE-MED` |
| `MstRoom`, `MstBed` | **Tidak berubah** |

---

## 3. File yang diubah

| Berkas | Jenis | Isi |
| --- | --- | --- |
| `Areas/HealthServices/MasterData/Seeders/InpatientMasterDataSeeder.cs` | Baru | Seeder beserta kelas hasil `InpatientMasterDataSeedResult` |
| `Program.cs` | Disunting | Fungsi lokal `SeedInpatientMasterDataAsync`, gerbang konfigurasi, dan satu `using` |
| `appsettings.json` | Disunting | Satu kunci konfigurasi `Seeders:RunInpatientMasterDataSeed`, bernilai `false` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientMasterDataSeederTests.cs` | Baru | 7 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/IsolatedInpatientDbContextFactory.cs` | Baru | Pembuat `ApplicationDbContext` di memori, dipakai bersama task lain |

Pola yang diikuti: `EmergencyMasterDataSeeder` milik modul IGD — bentuk kelas statis, kelas
hasil berisi ringkasan, cara pendaftaran pada `Program.cs`, dan gerbang konfigurasi yang
bawaannya mati. Kolom *Reuse* pada roadmap menunjuk seeder Farmasi; `EmergencyMasterDataSeeder`
dipilih karena ia keturunan langsung pola itu dan berada di folder yang sama dengan berkas ini.

### 3.1 Isi yang di-seed, apa adanya

**`MstInpatientSetting` — satu baris:**

| Kolom | Nilai | Sumber |
| --- | --- | --- |
| `Code` | `DEFAULT` | Konvensi, mengikuti `MstEmergencySetting` |
| `Name` | `Pengaturan Rawat Inap Default` | Konvensi |
| `BedReservationMinutes` | `120` | `RWI-RULE-002` — 2 jam |
| `DraftEpisodeExpiryHours` | `24` | `RWI-RULE-022` — 1 hari |
| `InitialAssessmentTargetHours` | `24` | `RWI-RULE-021` — **belum final secara klinis** |
| `ProgressNoteVerificationTargetHours` | `24` | `RWI-RULE-021` — **belum final secara klinis** |
| `PendingClosureThresholdHours` | `4` | `RWI-RULE-023` |
| `EpisodeNumberPrefix` | `RI` | Konvensi, mengikuti `IGD` |
| `IsDefault`, `IsActive` | `true` | — |
| `Notes` | Catatan yang menyebutkan dua angka di atas belum final secara klinis | Ditambahkan supaya peringatannya ikut terbawa ke database, bukan hanya hidup di laporan ini |

**`MstInpatientClearanceItem` — tiga baris:**

| `ItemCode` | `ItemName` | `IsMandatory` | `SortOrder` |
| --- | --- | :---: | ---: |
| `ADM-DOC` | Berkas administrasi pasien lengkap | Ya | 10 |
| `RETURN-ITEM` | Barang milik pasien dan barang rumah sakit sudah diselesaikan | Ya | 20 |
| `DISCHARGE-MED` | Obat pulang sudah diserahkan | **Tidak** | 30 |

> **Kenapa `DISCHARGE-MED` tidak wajib.** Penyerahan obat pulang diselesaikan modul Farmasi,
> dan modul Farmasi berada di luar scope MVP ini. Kalau butir itu dijadikan wajib, setiap
> episode akan tertahan menunggu penandaan yang tidak ada satu pun cara menyelesaikannya dari
> dalam modul Rawat Inap. Petugas akan menandainya asal-asalan supaya episode dapat ditutup —
> dan sejak saat itu daftar periksa administrasi kehilangan artinya. `RWI-RULE-024`.

### 3.2 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Baris `MstRoom` dan `MstBed` | `RWI-DEC-048` — khas tiap rumah sakit |
| Baris `MstServiceUnit` dan `MstPatientClass` | Dimiliki modul Master Data. Kesiapannya adalah gerbang `RWI-DEC-063`, bukan pekerjaan seeder ini |
| Endpoint untuk memanggil seeder | Seeder adalah alat penyiapan lingkungan, bukan fitur. Membuatnya dapat dipanggil lewat HTTP berarti membuat cara baru menulis master dari luar |
| Penimpaan nilai yang sudah ada | Seeder tidak pernah menimpa. Lihat contoh pada bagian 1.3 |

---

## 4. Verifikasi

### 4.1 Yang **belum** dijalankan, dan kenapa

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build Release | `dotnet build -c Release` | ✅ **PASS** — Build succeeded, 0 Error(s), 26 Agustus 2026 |
| Test | `dotnet test` | ✅ **PASS** — Passed! Failed 0, Passed 255, Total 255 |

Akibatnya, **tidak satu pun** acceptance criteria pada bagian 4.3 dapat dinyatakan terbukti
hari ini. Yang dapat dinyatakan hanyalah bahwa kodenya sudah ditulis dan test-nya sudah
disiapkan untuk membuktikannya. Bagian 7 memuat perintah yang perlu dijalankan.

### 4.2 Yang diperiksa lewat pembacaan kode

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Seeder tidak menyentuh `MstRoom` dan `MstBed` | Pembacaan seluruh isi berkas seeder | Tidak ada satu pun rujukan ke kedua tipe itu |
| Tidak ada tabel modul lain yang disentuh | Pembacaan yang sama | Hanya `MstInpatientSetting` dan `MstInpatientClearanceItem` |
| Diff hanya berisi penambahan | `git diff --stat` | `Program.cs` +114, `appsettings.json` +1, tanpa satu pun baris terhapus |
| Tidak ada nilai rahasia | Pembacaan diff | Tidak ada connection string, token, maupun kredensial |

### 4.3 Acceptance criteria

| # | Kriteria | Status | Bukti yang disiapkan |
| :---: | --- | :---: | --- |
| 1 | `MstInpatientSetting` berisi tepat satu baris berkode `DEFAULT` dengan kedelapan nilai §8.1 | ⏳ Belum diuji | `Seeder_MengisiSatuBarisPengaturanBerkodeDefault` memeriksa kedelapan nilai satu per satu |
| 2 | Tiga butir `ADM-DOC`, `RETURN-ITEM`, `DISCHARGE-MED`, dengan `DISCHARGE-MED` tidak wajib | ⏳ Belum diuji | `Seeder_MengisiTigaButirAdministrasiDenganDischargeMedTidakWajib` |
| 3 | Menjalankan seeder dua kali tidak menghasilkan data ganda | ⏳ Belum diuji | `Seeder_DijalankanDuaKali_TidakMenghasilkanDataGanda` — menghitung baris setelah dua kali jalan |
| 4 | Seeder menolak berjalan di lingkungan produksi | ⏳ Belum diuji | `Seeder_MenolakBerjalanDiLingkunganProduksi`, ditambah dua `Theory` untuk empat penulisan nama lingkungan |
| 5 | Seeder tidak pernah membuat baris `MstRoom` maupun `MstBed` | ⏳ Belum diuji (sudah terbukti lewat pembacaan kode, bagian 4.2) | `Seeder_TidakPernahMembuatKamarMaupunTempatTidur` |

### 4.4 Definition of Done

| Butir DoD | Status |
| --- | :---: |
| Seeder idempotent | ✅ Ditulis; ⏳ belum diuji |
| Seeder menolak produksi | ✅ Ditulis; ⏳ belum diuji |
| Ketiga test lulus | ⏳ **Belum dijalankan** |
| Laporan mencantumkan isi yang di-seed apa adanya | ✅ — bagian 3.1 |
| Laporan menyebut dua angka yang belum final | ✅ — bagian 3.1 dan 8 |

---

## 5. Penyimpangan terhadap roadmap dan blueprint

| Yang tertulis | Yang dikerjakan | Alasan |
| --- | --- | --- |
| Roadmap: seeder di `Areas/HealthServices/MasterData/Seeders/` | Sesuai roadmap | — |
| Arsitektur §5: seeder di `Areas/HealthServices/InPatientManagement/Seeders/` | **Tidak diikuti** | Roadmap revision `2` lebih baru dan merupakan wewenang delivery. Letak yang dipilih juga sejalan dengan `EmergencyMasterDataSeeder` yang berada di folder yang sama, dan seeder ini mengisi tabel `Mst*` milik Master Data — bukan tabel `Inp*`. Perbedaan ini perlu dirapikan pada salah satu dokumen; keputusannya milik pemilik arsitektur |
| Roadmap *Reuse*: `PrescriptionReviewCriterionSeeder.cs` dan `Icd10DiagnosisSeeder.cs` | Pola `EmergencyMasterDataSeeder` yang diikuti | `Icd10DiagnosisSeeder` berada di `Seeders/` akar, bukan di folder Farmasi seperti tertulis pada roadmap. `EmergencyMasterDataSeeder` adalah keturunan langsung pola yang sama dan berada di folder tujuan berkas ini |

Tidak ada penyimpangan pada isi yang di-seed. Kedelapan nilai dan ketiga butir sama persis
dengan §8.1 dan §8.2.

---

## 6. Yang perlu perhatian pemilik pekerjaan

### 6.1 Gerbang konfigurasi bawaannya mati

`Seeders:RunInpatientMasterDataSeed` bernilai `false` pada `appsettings.json`. Menyalakan
aplikasi apa adanya tidak menulis apa pun. Ini disengaja, mengikuti `RunEmergencyMasterDataSeed`
— basis data pengembangan di project ini adalah **basis data bersama**, dan seeder yang menyala
sendiri berarti setiap orang yang menjalankan aplikasi ikut menulis ke sana.

Untuk mengisinya, setel kunci itu menjadi `true` pada lingkungan yang dituju, jalankan
aplikasi sekali, lalu kembalikan ke `false`.

### 6.2 Dua lapis penjagaan produksi, bukan satu

Perlu dicatat supaya tidak salah paham saat membaca kode:

| Lapis | Letak | Yang dijaga |
| --- | --- | --- |
| Gerbang konfigurasi | `Program.cs` | Seeder tidak berjalan kecuali sengaja dinyalakan |
| Penolakan produksi | Di dalam seeder | Sekalipun seseorang menyalakan konfigurasi di produksi, seeder tetap tidak menulis apa pun |

Lapis kedua yang diminta `RWI-DEC-048`. Lapis pertama adalah kebiasaan project yang sudah ada.

### 6.3 Nama lingkungan yang dianggap produksi

Seeder membandingkan nama lingkungan dengan `Production`, mengabaikan besar kecil huruf dan
spasi di ujung. Ini mengikuti cara ASP.NET Core sendiri membaca `ASPNETCORE_ENVIRONMENT`.

**Yang perlu diperiksa pemilik pekerjaan:** bila lingkungan produksi rumah sakit ini memakai
nama lain — misalnya `Prod`, `Live`, atau `RSMMC-Prod` — penjagaan ini **tidak berlaku** di
sana. Nama lingkungan produksi yang sebenarnya belum diperiksa pada sesi ini karena
memerlukan konfigurasi deployment, yang di luar wewenang task ini.

---

## 7. Langkah berikutnya

| Urutan | Langkah | Perintah atau keterangan |
| :---: | --- | --- |
| 1 | Jalankan build | `dotnet build -c Release` |
| 2 | Jalankan test | `dotnet test --filter FullyQualifiedName~InpatientMasterDataSeederTests` |
| 3 | Perbarui bagian 4.3 dan 4.4 laporan ini dengan hasil sebenarnya | Wajib sebelum task ditandai selesai |
| 4 | Tandai `BE-RWI-002` selesai pada roadmap dan `requirement-traceability.md` | Hanya bila langkah 1–3 lulus |
| 5 | Periksa nama lingkungan produksi yang sebenarnya | Bagian 6.3 |
| 6 | `add`, `commit`, `push` | Dijalankan sendiri oleh pemilik pekerjaan |

---

## 8. Risiko tersisa

| Risiko | Sifat | Pemilik |
| --- | --- | --- |
| `InitialAssessmentTargetHours` dan `ProgressNoteVerificationTargetHours` bernilai `24` jam yang **belum disahkan pemilik klinis** (`RWI-RULE-021`) | Angka ini menentukan apakah sebuah episode dianggap terlambat dikaji. Salah angka berarti daftar pantau kepatuhan menunjuk episode yang sebenarnya tidak terlambat, atau sebaliknya. Dapat diubah admin, jadi tidak menahan MVP — tetapi **menahan pemakaian untuk pasien sungguhan** | Product/Domain bersama pemilik klinis |
| Build dan test belum dijalankan | Seluruh acceptance criteria belum terbukti | Backend/API |
| Penjagaan produksi bergantung pada nama lingkungan | Lihat bagian 6.3 | Backend/API bersama pemilik deployment |
| Seeder belum pernah dijalankan terhadap PostgreSQL sungguhan | Test memakai provider InMemory yang tidak menegakkan index unik. Idempotensi terbukti dari kode, bukan dari database | Backend/API |
