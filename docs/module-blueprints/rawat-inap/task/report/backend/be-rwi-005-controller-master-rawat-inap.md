# Laporan Perubahan Backend — `BE-RWI-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-005` |
| Judul | Admin dapat mengubah pengaturan dan butir administrasi lewat layar |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`; api contract `0.4.0` bagian Inpatient Setting dan Inpatient Clearance Item; permission matrix bagian 1.1 dan 2.5; `RWI-AC-003`, `RWI-AC-110` |
| Contract version | API `0.4.0` — **bentuknya tidak berubah**. Delapan endpoint yang sebelumnya berstatus "Rencana (belum tersedia)" kini ada di dalam kode |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `0869775` |
| Tanggal pengerjaan | 24 Agustus 2026 |
| Jenis perubahan | Penambahan dua controller, dua service master, dan dua berkas DTO. Tidak ada endpoint modul lain yang tersentuh |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Lihat bagian 6.1 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. `dotnet build` dan `dotnet test` **tidak dijalankan** pada sesi
> ini. Acceptance criteria nomor 6 — penolakan 403 tanpa hak akses — bahkan **tidak dapat**
> dibuktikan lewat test satuan; ia memerlukan aplikasi berjalan beserta basis datanya. Task
> ini belum boleh ditandai selesai. Lihat bagian 6.1 dan 9.

---

## 1. Apa yang dibangun, dan kenapa

### 1.1 Keadaan sebelum perubahan

Dua tabel master Rawat Inap sudah ada (`BE-RWI-001`) dan sudah dapat diisi seeder
(`BE-RWI-002`), tetapi **tidak ada satu pun cara mengubah isinya selain lewat perintah
database langsung**.

> **Contoh akibatnya.** Kepala ruangan meminta batas pemesanan tempat tidur diturunkan dari
> 2 jam menjadi 90 menit, karena ruangan sedang padat dan tempat tidur yang dipesan terlalu
> lama menganggur. Tanpa layar pengaturan, permintaan itu berubah menjadi tiket untuk tim
> Backend, yang harus menjalankan perintah `UPDATE` terhadap basis data produksi. Tidak ada
> jejak siapa yang meminta, tidak ada jejak siapa yang menyetujui, dan tidak ada cara
> mengembalikannya selain perintah `UPDATE` berikutnya.

### 1.2 Yang dibuka task ini

Delapan endpoint: dua untuk pengaturan, enam untuk butir administrasi. Sejak task ini, angka
batas waktu dan daftar butir administrasi dapat diubah admin dari layar, jejaknya tercatat pada
kolom audit dan pada log, dan nilai barunya berlaku pada pembacaan berikutnya.

---

## 2. Proses bisnis

### 2.1 Tujuan

Batas waktu pemesanan, ambang daftar pantau, dan daftar butir administrasi dapat diubah admin
tanpa satu baris kode pun disentuh, dan nilai barunya berlaku pada pembacaan berikutnya.

### 2.2 Pelaku

| Pelaku | Perannya | Hak akses |
| --- | --- | --- |
| Admin Master Data | Mengubah angka pengaturan; menambah, mengubah, menonaktifkan, dan menghapus butir administrasi | `InpatientSetting : Read/Update`, `InpatientClearanceItem : Read/Create/Update/Delete` |
| Petugas ruangan | Tidak menyentuh layar ini. Merasakan akibatnya lewat aturan yang dijalankan sistem | — |
| Auditor | Membaca jejak perubahan pada kolom audit dan log | — |

### 2.3 Pemicu

Admin membuka layar pengaturan Rawat Inap atau layar butir administrasi, lalu menyimpan
perubahan.

### 2.4 Prasyarat

| Prasyarat | Alasan |
| --- | --- |
| Kedua tabel master sudah ada | `BE-RWI-001` |
| Baris pengaturan sudah terisi | `BE-RWI-002`. Bila belum, `GET` pengaturan membalas 404 beserta penjelasan cara mengisinya |
| Enam service Rawat Inap terdaftar | `BE-RWI-004` |
| Peran pengguna sudah diberi butir hak akses | Admin melakukannya lewat layar Role Access yang sudah ada |

### 2.5 Langkah utama — mengubah angka pengaturan

1. Admin membuka layar pengaturan. Layar memanggil `GET /inpatient-settings`.
2. Sistem mengembalikan baris pengaturan yang berlaku beserta `Id`-nya.
3. Admin mengubah satu atau beberapa angka, lalu menekan Simpan.
4. Layar memanggil `PUT /inpatient-settings/{id}` dengan seluruh nilai barunya.
5. Sistem memeriksa kewajaran setiap angka. Bila ada yang di luar rentang, permintaan ditolak
   400 beserta kalimat yang menyebut angka mana yang salah.
6. Sistem memeriksa satu aturan tambahan: baris pengaturan terakhir yang masih aktif tidak
   boleh dinonaktifkan. Lihat bagian 2.7.
7. Bila lolos, nilai disimpan beserta jejak siapa yang mengubah dan kapan.
8. Perubahan dicatat pada log aplikasi.
9. Pembacaan berikutnya oleh service Rawat Inap mana pun memakai nilai baru.

### 2.6 Langkah utama — menambah butir administrasi

1. Admin menekan Tambah pada layar butir administrasi.
2. Layar memanggil `POST /inpatient-clearance-items`.
3. Sistem membakukan kode butir menjadi huruf besar tanpa spasi di ujung.
4. Sistem memeriksa apakah kode itu sudah dipakai butir lain. Bila sudah, permintaan ditolak
   409 beserta kalimat yang menyebut kodenya.
5. Bila belum, butir disimpan beserta jejak audit.
6. Bila dua admin menyimpan kode yang sama pada saat hampir bersamaan sehingga keduanya lolos
   langkah 4, index unik di database menolak salah satunya, dan sistem tetap membalas 409 —
   bukan galat 500.

### 2.7 Aturan bisnis yang melekat

| Aturan | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | ---: |
| Angka pengaturan wajib wajar | `BedReservationMinutes` di luar 1–1440 menit | "Lama pemesanan tempat tidur harus antara 1 dan 1440 menit." | 400 |
| Idem | Keempat angka jam di luar 1–720 jam | Kalimat serupa yang menyebut angka mana | 400 |
| Nama dan awalan wajib diisi | Kosong atau hanya spasi | "Nama pengaturan wajib diisi." / "Awalan nomor episode wajib diisi." | 400 |
| Pengaturan terakhir tidak boleh dinonaktifkan | `IsActive` dikirim `false` dan tidak ada baris aktif lain | "Pengaturan ini satu-satunya yang masih aktif, sehingga tidak dapat dinonaktifkan. Tanpa pengaturan aktif, modul Rawat Inap kembali memakai angka bawaan tanpa ada yang memberitahu petugas." | 400 |
| Kode butir tidak boleh kembar | Kode sudah dipakai butir lain yang belum terhapus | "Kode butir ADM-DOC sudah dipakai butir administrasi lain." | 409 |
| Kode dan nama butir wajib diisi | Kosong atau hanya spasi | "Kode butir wajib diisi." / "Nama butir wajib diisi." | 400 |
| Data tidak ditemukan | `Id` tidak ada atau sudah terhapus | "Butir administrasi tidak ditemukan." | 404 |
| Tanpa hak akses | Peran pengguna tidak punya butir haknya | Ditolak `AccessPermissionFilter` | 403 |

> **Contoh aturan pengaturan terakhir.** Admin membuka layar pengaturan lalu mematikan tanda
> aktif, mengira itu hanya menyembunyikan barisnya dari daftar. Sejak saat itu modul Rawat
> Inap tidak menemukan baris pengaturan mana pun, sehingga ia diam-diam kembali memakai angka
> bawaan: pemesanan 120 menit, walaupun rumah sakit sudah menyetelnya 90 menit sebulan
> sebelumnya. Tidak ada satu pun layar yang menampilkan hal itu sebagai kesalahan — pemesanan
> hanya "terasa" lebih longgar. Karena itu penonaktifan baris terakhir ditolak.

### 2.8 Perubahan status butir administrasi

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Aktif | Nonaktifkan | Nonaktif | Admin Master Data | Tidak ada. Butir wajib pun boleh dinonaktifkan |
| Nonaktif | Aktifkan kembali | Aktif | Admin Master Data | Tidak ada |
| Aktif atau Nonaktif | Hapus | Terhapus | Admin Master Data | Tidak ada. Penghapusan bersifat lunak — baris tetap tersimpan, hanya ditandai terhapus |
| Terhapus | — | — | — | Tidak ada jalan kembali lewat layar. Kodenya bebas dipakai butir baru |

**Yang tidak berubah pada ketiga tindakan itu:** penandaan yang sudah ada pada episode lama.

> **Contoh.** Butir `DISCHARGE-MED` sudah ditandai selesai pada episode Ny. Sari yang ditutup
> bulan lalu. Hari ini admin menonaktifkan butir itu karena penyerahan obat pulang pindah ke
> modul Farmasi. Penandaan pada episode Ny. Sari tetap ada apa adanya, sehingga riwayat
> penutupan episodenya tetap dapat dibaca utuh oleh auditor. Kalau penandaannya ikut terhapus,
> riwayat episode lama akan berbohong: ia akan tampak ditutup tanpa obat pulang pernah
> diserahkan, padahal diserahkan.

### 2.9 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Master pengaturan belum terisi | `GET` membalas 404 beserta kalimat yang menyebut cara mengisinya |
| Dua admin menyimpan kode butir yang sama bersamaan | Salah satu berhasil, yang lain menerima 409 — bukan 500 |
| Butir yang sudah terhapus dicoba diubah | 404 |
| Kode butir dikirim dengan huruf kecil dan spasi | Dibakukan menjadi huruf besar tanpa spasi sebelum diperiksa dan disimpan |

### 2.10 Hasil akhir

Delapan endpoint tersedia. Angka pengaturan dan daftar butir administrasi dapat diubah dari
layar, jejaknya tercatat, dan modul Rawat Inap membaca nilai barunya pada pembacaan berikutnya.

---

## 3. Grup endpoint

### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca pengaturan Rawat Inap yang berlaku | `InpatientSetting : Read` | – | `ApiResponse<InpatientSettingResponse>` |
| `PUT` | `/{id}` | Mengubah nilai pengaturan | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` | `ApiResponse<InpatientSettingResponse>` |

Kode status yang mungkin muncul:

| Kode | Artinya bagi pengguna |
| ---: | --- |
| 200 | Berhasil |
| 400 | Ada angka yang di luar rentang wajar, atau pengaturan terakhir dicoba dinonaktifkan |
| 403 | Peran pengguna tidak punya hak untuk tindakan ini |
| 404 | Pengaturan belum terisi, atau `Id` yang dikirim tidak ada |

### Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar butir administrasi | `InpatientClearanceItem : Read` | Query | `ApiResponse<InpatientClearanceItemPagedResult>` |
| `GET` | `/{id}` | Detail satu butir | `InpatientClearanceItem : Read` | – | `ApiResponse<InpatientClearanceItemResponse>` |
| `POST` | `/` | Menambah butir baru | `InpatientClearanceItem : Create` | `CreateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `PUT` | `/{id}` | Mengubah butir | `InpatientClearanceItem : Update` | `UpdateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan butir | `InpatientClearanceItem : Update` | `UpdateInpatientClearanceItemStatusRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `DELETE` | `/{id}` | Menandai butir terhapus | `InpatientClearanceItem : Delete` | `DeleteInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |

Penyaring yang tersedia pada `GET /`: `search`, `isMandatory`, `isActive`, `sortBy`,
`sortDirection`, `pageNumber`, `pageSize`. Urutan bawaannya menurut `sortOrder` naik, yaitu
urutan tampil butir pada daftar periksa yang dikerjakan petugas. Ukuran halaman bawaan 25,
paling besar 100 — sama seperti master data lain.

Kode status yang mungkin muncul:

| Kode | Artinya bagi pengguna |
| ---: | --- |
| 200 | Berhasil |
| 400 | Kode atau nama butir kosong, atau urutan tampil di luar 0–9999 |
| 403 | Peran pengguna tidak punya hak untuk tindakan ini |
| 404 | Butir tidak ditemukan atau sudah terhapus |
| 409 | Kode butir sudah dipakai butir lain |

---

## 4. File yang diubah

| Berkas | Jenis | Isi |
| --- | --- | --- |
| `Areas/HealthServices/MasterData/Controllers/InpatientSettingController.cs` | Baru | 2 endpoint |
| `Areas/HealthServices/MasterData/Controllers/InpatientClearanceItemController.cs` | Baru | 6 endpoint |
| `Areas/HealthServices/MasterData/DTOs/InpatientSettingDtos.cs` | Baru | `InpatientSettingResponse`, `UpdateInpatientSettingRequest` |
| `Areas/HealthServices/MasterData/DTOs/InpatientClearanceItemDtos.cs` | Baru | 5 DTO |
| `Areas/HealthServices/MasterData/Services/InpatientSettingService.cs` | Baru | Pembacaan dan perubahan baris pengaturan |
| `Areas/HealthServices/MasterData/Services/InpatientClearanceItemService.cs` | Baru | Seluruh CRUD butir administrasi |
| `Program.cs` | Disunting | Dua pendaftaran service (dilaporkan juga pada `BE-RWI-004`) |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientSettingServiceTests.cs` | Baru | 9 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientClearanceItemServiceTests.cs` | Baru | 10 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientMasterDataControllerContractTests.cs` | Baru | 7 test bentuk kontrak dan hak akses |

### 4.1 Butir hak akses tidak perlu didaftarkan satu per satu

Roadmap menyebut scope task ini mencakup "butir hak akses `InpatientSetting` dan
`InpatientClearanceItem` pada `AccessMenuSeeder`". Setelah `Seeders/AccessMenuSeeder.cs` dibaca,
ternyata **tidak ada satu baris pun yang perlu ditulis di sana**.

`AccessMenuSeeder` menyisir seluruh endpoint aplikasi saat menyala, membaca atribut
`[AccessController]` dan `[AccessAction]`, lalu membuat baris modul, controller, dan action di
database bila belum ada. Kedua controller baru sudah memakai atribut itu, sehingga butir
haknya muncul sendiri.

**Yang perlu diperhatikan pemilik pekerjaan:** karena butir hak baru lahir saat aplikasi
menyala, lingkungan uji yang tidak pernah menjalankan `AccessMenuSeeder` akan menolak seluruh
permintaan ke kedua controller ini — dan alasannya bukan kesalahan kode, melainkan butir haknya
memang belum ada. Ini persis risiko yang ditulis roadmap.

### 4.2 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| `POST /inpatient-settings` | Tabel pengaturan dipakai sebagai satu baris tunggal berkode `DEFAULT`. Api contract juga tidak memuatnya. Ketiadaannya dijaga satu test supaya tidak ditambahkan kelak tanpa menyadari akibatnya |
| `DELETE /inpatient-settings/{id}` | Alasan sama. Menghapus satu-satunya baris pengaturan membuat modul kembali memakai angka bawaan diam-diam |
| Kolom `Code` pada `UpdateInpatientSettingRequest` | Modul membaca baris ini lewat `Code`. Mengganti kodenya berarti seluruh modul kehilangan baris yang dibacanya |
| Kolom `IsDefault` pada request | Hanya ada satu baris; tandanya selalu benar. Menyediakan cara mematikannya hanya menambah cara baru untuk salah setel |
| `GET /filters/metadata` dan `GET /summary` | Beberapa master lain punya keduanya, tetapi api contract `0.4.0` tidak memuatnya untuk kedua grup ini. Menambahkan endpoint di luar kontrak yang terkunci bukan wewenang task ini |
| Penghapusan penandaan saat butir dinonaktifkan atau dihapus | Lihat contoh pada bagian 2.8 |

---

## 5. Penyimpangan terhadap roadmap dan blueprint

### 5.1 Controller tidak memakai `ApplicationDbContext` langsung — penyimpangan yang disengaja

| Yang tertulis | Yang dikerjakan |
| --- | --- |
| Roadmap kolom *Reuse*: "Tidak memakai service, sesuai konvensi project untuk CRUD sederhana" | Kedua controller memakai service; tidak satu pun menerima `ApplicationDbContext` |
| Arsitektur §4.20: "Tidak memakai service — CRUD sederhana, memakai `ApplicationDbContext` langsung sesuai konvensi" | Sama seperti di atas |

**Dasarnya `QBE-SVC-001`**, yang berbunyi: *MUST / NEW CODE: Module Service owns domain
CRUD/orchestration; controller does not direct-context it.*

Urutan wewenang pada `AGENTS.md` menempatkan `BACKEND_ENGINEERING_CONTRACT.md` di atas pola
source yang sudah ada. Kalimatnya tegas: keberadaan pola lama yang memakai `ApplicationDbContext`
langsung di controller **bukan** alasan menirunya pada kode baru. Kedua kalimat blueprint di
atas menyandarkan diri pada "konvensi", yaitu pola source yang sudah ada — tingkat wewenang
paling rendah.

Penyimpangan ini **menambah dua berkas** dibanding scope roadmap, dan tidak mengubah satu pun
bentuk endpoint, hak akses, maupun pesan bagi pengguna. Yang perlu diputuskan pemilik
arsitektur: apakah §4.20 dan kolom *Reuse* roadmap disesuaikan, atau pengecualian QBE dicatat
pada `QBE_EXCEPTIONS.json`. Selama belum diputuskan tidak ada akibat teknis apa pun.

### 5.2 Nama DTO status dan hapus

| Yang tertulis pada api contract | Yang dipakai |
| --- | --- |
| `UpdateStatusRequest` | `UpdateInpatientClearanceItemStatusRequest` |
| `DeleteRequest` | `DeleteInpatientClearanceItemRequest` |

Nama kelas DTO tidak terlihat oleh pemanggil — yang terlihat adalah bentuk JSON-nya, dan
bentuknya sama persis: `{ "isActive": true }` dan `{ "deleteReason": "..." }`. Penamaan
per-entity mengikuti konvensi seluruh master data project ini, misalnya
`UpdateAgeCategoryStatusRequest` dan `DeleteAgeCategoryRequest`. Nama generik pada api contract
dibaca sebagai penyebutan singkat, bukan nama kelas yang mengikat.

### 5.3 Bagian validation matrix yang dirujuk roadmap tidak ada

Kolom *Trace* pada roadmap menyebut "validation matrix bagian pengaturan".
`contracts/validation-matrix.md` versi `0.4.0` **tidak punya bagian itu** — kedua belas
bagiannya seluruhnya tentang alur episode, dan tidak satu pun tentang master pengaturan atau
butir administrasi.

Akibatnya, aturan validasi kedua layar ini **tidak terkunci kontrak**. Yang dipakai sebagai
dasar: acceptance criteria pada roadmap, pola master data terdekat, dan akibat nyata dari
angka yang salah. Seluruhnya ditulis apa adanya pada bagian 2.7 supaya dapat ditinjau pemilik
proses bisnis.

Satu aturan pada bagian 2.7 **tidak** berasal dari roadmap maupun kontrak: penolakan
menonaktifkan baris pengaturan terakhir. Alasannya ada pada contoh di bagian 2.7. Aturan ini
perlu disahkan atau dicabut pemilik proses bisnis.

### 5.4 Nama service master dan service modul mirip

`InpatientSettingService` (Master Data) dan `InpSettingService` (Rawat Inap) adalah dua kelas
berbeda dengan tugas berbeda:

| Kelas | Pemilik | Tugas |
| --- | --- | --- |
| `InpatientSettingService` | Master Data | Melayani layar admin: membaca satu baris pengaturan dan mengubah nilainya |
| `InpSettingService` | Rawat Inap | Melayani service lain: membaca angka yang berlaku, menyediakan nilai bawaan bila master belum terisi |

Penamaannya mengikuti konvensi masing-masing modul — `Inpatient` untuk berkas Master Data
seperti `EmergencySettingService`, dan awalan `Inp` untuk berkas modul Rawat Inap sesuai
registry. Perbedaannya ditulis sebagai catatan pada kedua kelas supaya tidak tertukar.

---

## 6. Verifikasi

### 6.1 Yang **belum** dijalankan, dan kenapa

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build Release | `dotnet build -c Release` | **BELUM DIJALANKAN** — pemilik pekerjaan meminta pengerjaan tanpa build |
| Test | `dotnet test` | **BELUM DIJALANKAN** — alasan sama |
| Pemanggilan endpoint sungguhan | — | **BELUM DIJALANKAN** — memerlukan aplikasi berjalan beserta basis datanya |

### 6.2 Yang diperiksa lewat pembacaan kode

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Kedua controller tidak menerima `ApplicationDbContext` | Pembacaan ctor | Benar — hanya service dan `LoggerService` |
| Setiap endpoint punya `[AccessAction]` dan `[AccessPermission]` | Pembacaan seluruh aksi | Kedelapan endpoint punya keduanya, dan nilainya cocok permission matrix bagian 2.5 |
| Route dan grup Swagger cocok kontrak | Pembacaan atribut | Cocok |
| Tidak ada endpoint modul lain yang tersentuh | `git status --short` | Hanya berkas baru; `Program.cs` dan `appsettings.json` yang disunting, keduanya hanya penambahan |
| Tidak ada nilai rahasia | Pembacaan diff | Tidak ada |

### 6.3 Acceptance criteria

| # | Kriteria | Status | Bukti yang disiapkan |
| :---: | --- | :---: | --- |
| 1 | Kedelapan endpoint sesuai api contract bentuk dan hak aksesnya | ⏳ Belum diuji | `InpatientMasterDataControllerContractTests` — 7 test yang memeriksa route, grup Swagger, jumlah endpoint per verb, dan kelengkapan atribut hak akses |
| 2 | Mengubah `BedReservationMinutes` membuat pemesanan **berikutnya** memakai nilai baru; pemesanan yang sudah berjalan tidak berubah | ⏳ Belum diuji, dan **sebagian belum dapat diuji** | `MengubahBatasPemesanan_BerlakuPadaPembacaanBerikutnya` membuktikan separuh pertamanya. Separuh kedua — pemesanan yang sudah berjalan tidak berubah — **belum dapat diuji sampai `BE-RWI-010` ada**, karena belum ada satu pun pemesanan di dalam sistem |
| 3 | Menambah baris pengaturan kedua ditolak | ⏳ Belum diuji | Dijaga tiga lapis: tidak ada endpoint `POST`, dijaga test `InpatientSettingController_TidakPunyaEndpointMenambahBaris`; seeder idempotent (`BE-RWI-002`); dan unique index `IX_MstInpatientSetting_Code` di database |
| 4 | Menonaktifkan butir wajib tidak menghapus penandaan yang sudah ada pada episode lama | ⏳ Belum diuji | `MenonaktifkanButirWajib_TidakMenghapusPenandaanPadaEpisodeLama` dan `MenghapusButir_BersifatLunakDanTidakMenyentuhPenandaan` |
| 5 | Butir dengan `ItemCode` kembar ditolak | ⏳ Belum diuji | `ButirDenganKodeKembar_Ditolak` dan `MengubahButirMenjadiKodeMilikButirLain_Ditolak` |
| 6 | Tanpa hak akses, ditolak 403 | ⏳ **Tidak dapat diuji lewat test satuan** | Yang dapat diuji: seluruh endpoint memang diberi atribut hak akses yang benar (`SetiapEndpointDiberiAtributHakAkses`). Penolakan 403-nya sendiri dijalankan `Filters/AccessPermissionFilter.cs` saat permintaan masuk, dan pembuktiannya memerlukan aplikasi berjalan |

### 6.4 Definition of Done

| Butir DoD | Status |
| --- | :---: |
| Dua controller | ✅ |
| Delapan endpoint | ✅ |
| Hak akses terdaftar | ✅ Otomatis lewat `AccessMenuSeeder` — lihat bagian 4.1. ⏳ Belum terbukti karena aplikasi belum dinyalakan |
| Keenam kriteria lulus | ⏳ **Belum dijalankan**; kriteria 2 dan 6 hanya sebagian yang dapat diuji |
| Api contract diperbarui dari "Rencana" menjadi tersedia | ⛔ **Belum dilakukan** — lihat bagian 7 |

---

## 7. Api contract belum diperbarui, dan itu disengaja

Butir DoD terakhir meminta `contracts/api-contract.md` diperbarui: kedelapan baris yang
sekarang bertuliskan **Rencana (belum tersedia)** menjadi tersedia.

Pembaruan itu **tidak dilakukan** pada sesi ini, karena dua hal:

1. `contracts/**` adalah wewenang tulis skill blueprint, bukan skill implementasi. Aturan
   `lokasi-laporan-task.md` menetapkan skill build hanya menulis di `task/report/<lapisan>/`
   ditambah pembaruan bukti pada roadmap dan `requirement-traceability.md`.
2. Menandai endpoint "tersedia" sebelum build dan test dijalankan berarti menuliskan klaim yang
   belum ada buktinya. Endpoint yang tidak dapat dibangun bukan endpoint yang tersedia.

Pembaruan api contract karena itu menjadi langkah lanjutan setelah validasi dijalankan, dan
tercatat pada bagian 9.

---

## 8. Kesesuaian QBE

| ID | Berlaku pada | Kepatuhan |
| --- | --- | --- |
| `QBE-MOD-001` | Seluruh berkas | Kedua controller, DTO, dan service berada di bawah `Areas/HealthServices/MasterData/`, modul pemilik tabelnya |
| `QBE-MOD-002` | — | Tidak ada entity baru pada task ini |
| `QBE-NAM-001` | Seluruh berkas | Tidak ada nama berawalan `Trx` |
| `QBE-NAM-002` | — | Tabelnya sudah ada, berawalan `Mst` sesuai registry |
| `QBE-SVC-001` | Kedua controller | **Dipatuhi, menyimpang dari blueprint.** Lihat bagian 5.1 |
| `QBE-API-001` | Kedelapan endpoint | Memakai `[ApiController]`, `ControllerBase`, route `api/v1/...`, `ApiResponse<T>`, `PagedResult<T>`, dan kode status yang sudah mapan |
| `QBE-PERM-001` | Kedelapan endpoint | `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]` sesuai permission matrix bagian 1.1 dan 2.5 |
| `QBE-LOG-001` | Lima endpoint yang mengubah data | Setiap perubahan menulis catatan lewat `LoggerService` beserta `EntityId` dan pelakunya. Kedua `GET` tidak dicatat, sesuai permission matrix |
| `QBE-VAL-001` | Kedua service | Validasi bentuk lewat data annotation pada DTO, dan validasi aturan bisnis di dalam service |
| `QBE-DTO-001` | Kedelapan endpoint | Tidak ada entity EF yang dikembalikan sebagai balasan; seluruhnya lewat DTO |
| `QBE-PAGE-001` | `GET /inpatient-clearance-items` | `PagedResult<T>`, ukuran halaman bawaan 25, paling besar 100 — sama seperti master data lain |
| `QBE-OPT-001` | — | Tidak ada endpoint pilihan yang dibuat, karena belum ada layar yang memakainya |
| `QBE-DEL-001` | `DELETE` dan `PATCH /status` | Penghapusan bersifat lunak beserta jejak pelakunya; penandaan pada episode lama tidak tersentuh |
| `QBE-ENT-003` | `MstInpatientClearanceItem.SortOrder` | **Temuan di luar scope.** Kolom ini lahir pada `BE-RWI-001`. Ia dipakai sebagai urutan butir pada daftar periksa yang dikerjakan petugas — urutan bisnis, bukan urutan tampilan semata — sehingga tidak dianggap melanggar. Dicatat di sini supaya terlihat saat ditinjau, dan **tidak diubah** karena di luar scope task ini |

---

## 9. Langkah berikutnya

| Urutan | Langkah | Perintah atau keterangan |
| :---: | --- | --- |
| 1 | Jalankan build | `dotnet build -c Release` |
| 2 | Jalankan test | `dotnet test --filter FullyQualifiedName~InPatientManagement` |
| 3 | Nyalakan aplikasi, pastikan `AccessMenuSeeder` membuat butir hak `InpatientSetting` dan `InpatientClearanceItem` | Membuktikan DoD hak akses |
| 4 | Uji kedelapan endpoint lewat Swagger, termasuk sekali tanpa hak akses untuk membuktikan 403 | Membuktikan acceptance criteria 1 dan 6 |
| 5 | Perbarui bagian 6.3 dan 6.4 laporan ini dengan hasil sebenarnya | Wajib sebelum task ditandai selesai |
| 6 | Perbarui `contracts/api-contract.md` lewat skill blueprint | Lihat bagian 7 |
| 7 | Tandai `BE-RWI-005` selesai pada roadmap dan `requirement-traceability.md` | Hanya bila langkah 1–4 lulus |
| 8 | Putuskan penyimpangan `QBE-SVC-001` versus blueprint §4.20 | Lihat bagian 5.1 |
| 9 | Sahkan atau cabut aturan "pengaturan terakhir tidak boleh dinonaktifkan" | Lihat bagian 5.3 |
| 10 | `add`, `commit`, `push` | Dijalankan sendiri oleh pemilik pekerjaan |

---

## 10. Risiko tersisa

| Risiko | Sifat | Pemilik |
| --- | --- | --- |
| Build, test, dan pemanggilan endpoint belum dijalankan | Kelima acceptance criteria pertama belum terbukti; kriteria 6 memang tidak dapat dibuktikan tanpa aplikasi berjalan | Backend/API |
| Butir hak akses lahir saat aplikasi menyala | Lingkungan uji yang tidak menjalankan `AccessMenuSeeder` akan menolak seluruh permintaan ke kedua controller ini, dan alasannya akan tampak seperti kesalahan kode padahal bukan | Backend/API |
| Aturan "pengaturan terakhir tidak boleh dinonaktifkan" belum disahkan | Aturan ini menutup satu jalan yang mungkin memang dibutuhkan admin. Belum ada dasarnya pada kontrak | Product/Domain |
| Acceptance criteria 2 baru separuh yang dapat diuji | Separuh keduanya menunggu `BE-RWI-010` | Backend/API |
| Penyimpangan `QBE-SVC-001` versus blueprint §4.20 belum diputuskan | Dokumentasi, bukan teknis. Selama belum diputuskan, blueprint dan kode berbeda pada satu kalimat | Pemilik arsitektur backend |
| Api contract masih menyatakan kedelapan endpoint "Rencana (belum tersedia)" | Dokumentasi. Wajib diperbarui lewat skill blueprint setelah validasi lulus | Backend/API bersama pemilik blueprint |
| Provider InMemory tidak menegakkan index unik | Test kode kembar membuktikan bahwa **service** menolaknya. Penjagaan lapis kedua — index unik `IX_MstInpatientClearanceItem_ItemCode` — sudah terbukti menolak pada database sungguhan saat `BE-RWI-001` | Backend/API |
