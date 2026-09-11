# Laporan Perubahan Backend — `BE-RWI-070`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-070` |
| Judul | Ambang tindak lanjut deposit dapat diubah admin |
| Slice | `S11` — Deposit dapat diterima dan ditelusuri ke episodenya; `EPIC RI-31` pengaturan yang dapat diubah admin |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-070` |
| Trace | `RWI-DEC-096`; `FR-RI-143`, `FR-RI-177`; `04-prd-to-mvp.md` `EPIC RI-31` |
| Contract version | API `0.6.1`. **Nol perubahan kontrak** — endpoint pengaturan yang sudah ada hanya bertambah satu field |
| Dependency | **Tidak ada.** Kartu task menuliskan `—` |
| Klasifikasi | `LIGHT` — satu kolom, satu migration, satu aturan validasi, tanpa endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData/**`, `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs`, `Repositories/Configurations/**`, `Migrations/`, project uji InMemory |
| Model | claude-opus-5 |
| Commit backend saat dikerjakan | `4c3a458dac3fd10dcac770adb938bfa2b4e0a7dd` |
| Tanggal | 10 September 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terbukti. Satu migration dibuat dan **belum diterapkan ke database mana pun**. Satu butir verifikasi — uji migration maju-mundur pada PostgreSQL — **dikecualikan atas keputusan pemilik**; lihat bagian 5.2 dan 6 |

---

## 1. Masalah yang diperbaiki

`RWI-DEC-096` memutuskan bahwa kekurangan uang muka pada perawatan panjang ditagih ulang secara
berkala. Berapa hari sekali penagihan itu diingatkan adalah **kebijakan rumah sakit**, bukan
keputusan pengembang — tetapi sebelum perubahan ini tidak ada satu tempat pun untuk menyimpannya.

Akibatnya, begitu daftar pantau kekurangan deposit dibangun nanti, angka tiga hari itu terpaksa
ditanam di dalam kode. Rumah sakit yang ingin menagih ulang seminggu sekali harus menunggu
pengembang mengubah kode, membangun ulang aplikasi, dan menerbitkan versi baru — untuk satu angka
yang sifatnya administratif.

**Contoh.** Rumah sakit menetapkan penagihan ulang setiap 7 hari, bukan 3. Sebelum perubahan ini,
permintaan sesederhana itu adalah permintaan perubahan kode. Sesudahnya, admin membukanya sendiri
di layar Pengaturan Rawat Inap, menyimpan, dan angka baru itu berlaku pada pembacaan berikutnya
tanpa aplikasi dinyalakan ulang.

---

## 2. Proses bisnis

**Tujuan.** Rumah sakit dapat mengubah sendiri jarak hari penagihan ulang kekurangan uang muka.

**Pelaku.** Admin pemegang `InpatientSetting : Update`.

**Pemicu.** Kebijakan penagihan rumah sakit berubah.

**Langkah yang berurutan.**

1. Admin membuka layar Pengaturan Rawat Inap dan membaca nilai yang berlaku lewat
   `GET /master-data/inpatient-settings`.
2. Admin mengubah **Ambang tindak lanjut kekurangan deposit** dari `3` menjadi angka lain.
3. Admin menyimpan lewat `PUT /master-data/inpatient-settings/{id}`.
4. Sistem memeriksa angkanya masuk akal, lalu menyimpannya beserta jejak siapa yang mengubah dan
   kapan.
5. Setiap pembacaan berikutnya oleh modul Rawat Inap memakai angka baru itu.

**Aturan yang berlaku.**

| Aturan | Nilai | Sebabnya |
| --- | --- | --- |
| Batas bawah | `1` hari | Ambang `0` berarti kekurangan ditagih ulang **tanpa jeda**. Setiap pembacaan daftar pantau akan memunculkan episode yang sama, dan penagihan berkala kehilangan artinya |
| Batas atas | `365` hari | Tidak ada episode rawat inap yang berjalan selama setahun, sehingga angka di atasnya hanya dapat berarti salah ketik yang **mematikan** pengingat tanpa siapa pun menyadarinya |
| Nilai bawaan | `3` hari | `RWI-DEC-096` |

**Status yang dihasilkan.** Tidak ada status baru. Kolom ini hanya menjadwalkan pengingat kerja.

**Jalur tidak normal pertama — baris pengaturan belum ada sama sekali.** Pada lingkungan baru yang
belum di-seed, modul Rawat Inap membaca nilai bawaan `3` dan **tetap menyala**, sambil menuliskan
peringatan pada log supaya keadaan itu tidak lolos diam-diam. Perilaku ini sudah menjadi pola
`InpSettingService` sejak `RWI-DEC-008`, dan kolom baru ini mengikutinya apa adanya.

**Jalur tidak normal kedua — baris warisan yang kolomnya bernilai `0`.** Baris pengaturan yang
dibuat sebelum kolom ini lahir mendapat nilai `3` langsung dari database saat migration dijalankan.
Sebagai lapis kedua, pembacaan modul juga mengembalikan `3` bila nilainya kurang dari `1`. Tanpa
dua lapis itu, satu baris warisan akan membuat pengingat menyala setiap hari.

**Jalur tidak normal ketiga — angka di luar batas.** Nilai `0`, negatif, atau di atas `365`
ditolak dengan pesan yang menyebut rentangnya, dan baris yang tersimpan **tidak bergeser sama
sekali**.

**Hasil akhir.** Jarak hari penagihan ulang menjadi setelan rumah sakit, bukan angka di dalam kode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-070` | Scope, kelima acceptance criteria, dan verifikasi yang diminta |
| `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs` | Tempat kolomnya, dan pola nilai bawaan kolom lain |
| `Areas/HealthServices/MasterData/Services/InpatientSettingService.cs` | Pola validasi dan penyimpanan yang sudah ada lewat `BE-RWI-005` |
| `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs` | Pola nilai bawaan `InpatientSettingValues.Defaults` |
| `Areas/HealthServices/MasterData/Seeders/InpatientMasterDataSeeder.cs` | Angka yang di-seed supaya tidak berbeda dari bawaan |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Status kepemilikan `InPatientManagement / Inp` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs` | Kolom `DepositFollowUpIntervalDays` bernilai bawaan `3` |
| `Repositories/Configurations/HealthServices/MasterData/MstInpatientSettingConfiguration.cs` | `HasDefaultValue(3)` supaya baris yang sudah ada di database ikut bernilai `3`, bukan `0` |
| `Areas/HealthServices/MasterData/DTOs/InpatientSettingDtos.cs` | Field baru pada DTO baca; field baru pada DTO ubah beserta `[Range(1, 365)]` |
| `Areas/HealthServices/MasterData/Services/InpatientSettingService.cs` | Menyimpan nilai baru, dan menolak nilai di luar rentang beserta pesannya |
| `Areas/HealthServices/MasterData/Controllers/InpatientSettingController.cs` | Field baru ikut pada jawaban `ToResponse` |
| `Areas/HealthServices/MasterData/Seeders/InpatientMasterDataSeeder.cs` | Baris `DEFAULT` di-seed dengan `3` |
| `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs` | `InpatientSettingValues` membawa field baru; `Defaults` bernilai `3`; pembacaan memulihkan `3` bila nilai tersimpan kurang dari `1` |
| `Migrations/20260910041922_AddDepositFollowUpIntervalToInpatientSetting.cs` | **Migration baru.** Menambah satu kolom `integer` dengan `defaultValue: 3` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model ikut memuat kolom baru |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/InPatientManagement/InpatientSettingServiceTests.cs` | Empat test baru untuk kelima kriteria |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Nol endpoint baru. `InpatientSettingResponse` bertambah satu field, dan `UpdateInpatientSettingRequest` menerima satu field baru yang bernilai bawaan `3` bila tidak dikirim, sehingga pemanggil lama tidak rusak |
| Database | **Satu migration**, `20260910041922_AddDepositFollowUpIntervalToInpatientSetting`. Satu kolom `integer NOT NULL DEFAULT 3` pada `public."MstInpatientSetting"`. **Belum diterapkan** ke database dev pemilik, database bersama, maupun database target mana pun — lihat bagian 5.2 |
| Keamanan/Auth | **Nol perubahan.** Memakai `InpatientSetting : Read` dan `InpatientSetting : Update` yang sudah ada. `[AccessController(ControllerName = "InpatientSetting")]` sejajar dengan `[AccessPermission("InpatientSetting", …)]` dan `[AccessAction(…)]` pada kedua method. Nol hardcode peran |

---

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Inpatient Setting

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Membaca pengaturan Rawat Inap yang berlaku, kini termasuk ambang tindak lanjut kekurangan deposit | `InpatientSetting : Read` |
| `PUT` | `/{id}` | Mengubah nilai pengaturan, kini termasuk ambang tindak lanjut kekurangan deposit | `InpatientSetting : Update` |

Route lengkapnya `api/v1/health-services/master-data/inpatient-settings`.

**Field yang bertambah.**

| Nama | Tipe | Bawaan | Rentang | Arti |
| --- | --- | --- | --- | --- |
| `depositFollowUpIntervalDays` | `int` | `3` | `1`–`365` | Berapa hari sekali kekurangan uang muka ditagih ulang selama episode berjalan |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` seluruh solution | **`0 error CS`**, 197 warning | `PASS` | Kompilasi C# bersih. Dua error `MSB3021`/`MSB3027` bukan kesalahan kode — lihat bagian 5.1 |
| Kriteria 1 — tanpa baris pengaturan, ambang terbaca `3` dan modul tetap menyala | Lulus | `PASS` | `InpatientSettingServiceTests.Kriteria1_TanpaBarisPengaturan_AmbangTindakLanjutTerbacaTigaHari` |
| Kriteria 2 dan 4 — admin mengubahnya lewat endpoint yang sudah ada, dan berlaku pada pembacaan berikutnya | Lulus. Dibaca `3`, diubah menjadi `7`, dibaca ulang `7` tanpa aplikasi dinyalakan ulang | `PASS` | `…Kriteria2Dan4_AmbangDiubahAdmin_BerlakuPadaPembacaanBerikutnya` |
| Kriteria 3 — nilai `0`, `-1`, dan `366` ditolak | Lulus untuk ketiganya, dan baris tersimpan tetap `3` | `PASS` | `…Kriteria3_AmbangDiLuarRentangWajar_Ditolak`, tiga `InlineData` |
| Baris warisan bernilai `0` tetap terbaca `3` | Lulus | `PASS` | `…BarisWarisanBerambangNol_TetapTerbacaTigaHari` |
| Uji migration **maju-mundur** pada PostgreSQL Docker sekali pakai | **Tidak dijalankan sampai selesai** | `NOT RUN` | Dicoba dan dihentikan; sebabnya pada bagian 5.2 |
| `dotnet test` kelas uji ini beserta kelas uji `BE-RWI-069` | `Failed: 0, Passed: 21` | `PASS` | Keluaran perintah |
| `dotnet test` project uji InMemory **penuh** | `Failed: 17, Passed: 1023, Total: 1040` | `EXISTING / ENVIRONMENT ISSUE` | Ketujuh belas kegagalan seluruhnya milik `BillingManagement` dan sudah ada sebelum task ini |
| Garis dasar pada `HEAD` tanpa perubahan apa pun | `Failed: 17, Passed: 1012, Total: 1029` | `PASS` | Perubahan di-`stash`, suite dijalankan, lalu dikembalikan. **Angka gagalnya sama persis: 17** |

Uji manual: `NOT FEASIBLE` — layar Pengaturan Rawat Inap adalah pekerjaan repository frontend, dan
task frontend untuk field ini belum ada.

**Kriteria 5 — nol tabel dan nol kolom milik modul lain tersentuh.** Dibuktikan dari daftar berkas
yang berubah: satu-satunya entity yang disentuh adalah `MstInpatientSetting`, dan migration yang
dihasilkan hanya memuat satu `AddColumn` pada tabel itu. Nol tabel `Bil*` dan nol tabel milik modul
lain muncul pada migration maupun pada snapshot.

### 5.1 Dua error build yang bukan kesalahan kode

`dotnet build` melaporkan `2 Error(s)`, keduanya `MSB3021` dan `MSB3027`: berkas di `bin/` tidak
dapat ditimpa karena aplikasi backend **sedang berjalan** di komputer ini sejak 09:42 dan
menguncinya. Kompilasi C# sendiri berhasil sepenuhnya dengan `0 error CS`.

Proses milik pemilik **tidak dihentikan**. Seluruh build dan test dijalankan dengan
`-p:BaseOutputPath` ke folder sementara, sehingga nol berkas milik pemilik ditimpa dan test tetap
berjalan terhadap assembly yang baru, bukan yang basi.

### 5.2 Status migration

| Hal | Keadaan |
| --- | --- |
| Migration dibuat | Ya — `20260910041922_AddDepositFollowUpIntervalToInpatientSetting` |
| Diterapkan ke database dev pemilik | **Belum** |
| Diterapkan ke database bersama atau target | **Belum**, dan itu wewenang terpisah |
| Uji maju-mundur pada container PostgreSQL sekali pakai | **`NOT RUN`** — dicoba, lalu dihentikan |

**Kenapa uji maju-mundur tidak selesai dijalankan.** Container `postgres:15.15` sekali pakai
memang dinyalakan dan siap menerima perintah, tetapi penerapan migration tidak pernah sampai
membuat satu tabel pun. Sebabnya kehabisan memori pada komputer ini: dari 32 GB, sisa memori bebas
turun sampai **401 MB** karena aplikasi backend yang sedang berjalan, layanan bahasa editor, mesin
Docker, dan proses build .NET berebut memori pada saat bersamaan — sampai satu perintah
pemeriksaan pun gagal dengan `The paging file is too small for this operation to complete`.
Percobaannya **dihentikan dengan sengaja** supaya tidak mengganggu aplikasi milik pemilik yang
sedang berjalan; container uji dan mesin Docker sudah dimatikan kembali sesudahnya.

**Yang perlu diperiksa pemilik saat menjalankannya nanti.** Berkas migration-nya hanya memuat satu
`AddColumn` dengan `defaultValue: 3` dan satu `DropColumn` sebagai kebalikannya, sehingga langkah
mundurnya simetris dan tidak menyentuh tabel, index, maupun constraint lain. Yang layak dibuktikan
adalah tiga hal: kolomnya hadir sesudah maju, hilang sesudah mundur, dan baris pengaturan yang
sudah ada bernilai `3` — bukan `0` — sesudah maju.

Penerapan migration ke database mana pun di luar container uji sekali pakai adalah **wewenang
tersendiri** dan tidak dijalankan pada task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Nilai bawaan `3` terbaca pada lingkungan yang barisan pengaturannya belum diisi, tanpa aplikasi gagal menyala | **Terpenuhi** | Test kriteria 1 |
| 2. Admin dapat mengubahnya lewat endpoint pengaturan yang sudah ada | **Terpenuhi** | Test kriteria 2 dan 4, lewat `PUT /{id}` yang sudah ada |
| 3. Nilai `0` atau negatif ditolak | **Terpenuhi** | Test kriteria 3, ditambah batas atas `366` |
| 4. Perubahan berlaku pada pembacaan berikutnya | **Terpenuhi** | Test kriteria 2 dan 4 |
| 5. Nol tabel dan nol kolom milik modul lain tersentuh | **Terpenuhi** | Migration memuat satu `AddColumn` pada `MstInpatientSetting` saja |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Kolom ada | ✅ Terpenuhi |
| Migration ada | ✅ Terpenuhi |
| Validasi ada | ✅ Terpenuhi |
| Test ada | ✅ Terpenuhi — empat test baru |
| Kelima kriteria lulus | ✅ Terpenuhi |
| Build lulus | ✅ Terpenuhi untuk kompilasi C# — `0 error CS` |
| Laporan menyatakan migration belum diterapkan di luar lokal | ✅ Terpenuhi — bagian 5.2 |

**Satu butir verifikasi dikecualikan atas keputusan pemilik.** Uji migration maju-mundur pada
container PostgreSQL sekali pakai **tidak dijalankan sampai selesai**, dan pemilik menyatakan pada
10 September 2026 bahwa build serta verifikasi sisa dijalankan sendiri olehnya. Butir ini bukan
salah satu dari kelima acceptance criteria; kelimanya tetap terbukti penuh lewat test yang
benar-benar dijalankan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kolom ini **hanya menjadwalkan pengingat**. Ia tidak menahan perawatan, tidak menahan penutupan episode, dan tidak menolak tindakan apa pun. Kehati-hatian itu disebut kartu task dan sengaja dicatat ulang di sini supaya tidak bergeser artinya kelak |
| Masalah yang diketahui | Kolomnya sudah ada, tetapi **belum ada satu pun pembacanya**. Pemakainya adalah `BE-RWI-071` daftar pantau kekurangan deposit, yang masih ⛔ terblokir menunggu `BE-BKC-040`. Sampai itu selesai, angka ini tersimpan dan dapat diubah admin tetapi belum memengaruhi perilaku apa pun |
| Risiko tersisa | Rendah. Perubahan bersifat aditif ke satu tabel master yang berisi satu baris |
| Perubahan sampingan | Dua hal di luar source, keduanya disebut apa adanya. **Pertama**, perubahan sempat di-`stash` dan dikembalikan utuh untuk mengukur garis dasar test; pemulihannya dikonfirmasi. **Kedua**, container Docker `quilvian_rwi041_test` sempat dibuat untuk uji migration. Mesin Docker mati sendiri karena memori habis sebelum container itu dapat dihapus, sehingga ia **mungkin muncul kembali sebagai container berhenti** ketika Docker dinyalakan lagi. Container itu tidak dipakai apa pun dan aman dihapus dengan `docker rm -f quilvian_rwi041_test` |
| Interupsi | `NONE` |
| Status Git | Berkas berubah dan bertambah; nol operasi `add`, `commit`, `push`, `merge`, maupun `rebase` dijalankan |
| Langkah berikutnya | Terapkan migration ke database dev pemilik bila diinginkan — wewenang terpisah. Pemakaian angkanya menunggu `BE-RWI-071`, yang menunggu `BE-BKC-040` pada roadmap `billing-kasir` |
