# Laporan Perubahan Backend — `BE-BD-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-014` |
| Judul | Lokasi penyimpanan darah dapat dikelola, termasuk dinonaktifkan |
| Slice | `MVP-0` — fondasi master Bank Darah |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md` §D.1 |
| Trace | `DEC-BD-035`, `DEC-BD-037`, `BD-DOM-24` · `INV-BD-025`, `INV-BD-027`, `INV-BD-028` · `contracts/api-contract.md` §Blood Storage Location · `contracts/validation-matrix.md` (`VAL-BD-067`, `VAL-BD-068`) · `data/data-dictionary.md` §`MstBloodStorageLocation` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` approval ✅ · `G2a`/`G2b` registry ✅. Tidak ada task pendahulu |
| Klasifikasi | `MEDIUM` — satu entity master baru, sembilan endpoint, satu migration, satu seeder, nol perubahan pada modul lain |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData/**`, `Repositories/**`, `Migrations/**`, `QuilvianSystemBackend.Tests/**`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e9e5903` cabang `sukmagp` |
| Tanggal | `2026-09-03` |
| Status | **`SELESAI`** untuk scope master. Penegakan `VAL-BD-060` dan pencacahan `VAL-BD-068` menjadi milik `BE-BD-015`; lihat bagian 6 dan 8 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, Quilvian tidak punya tempat untuk mencatat **di mana darah disimpan**.
Akibatnya bukan sekadar data yang hilang: `INV-BD-025` menetapkan kantong darah **tidak dapat
dialokasikan sebelum memiliki lokasi penyimpanan**, sehingga tanpa master ini seluruh alur Bank
Darah berhenti sebelum dimulai.

Ada masalah kedua yang lebih halus. `MstDrugStorageLocation` sudah ada, punya tipe `ColdStorage`
lengkap dengan rentang suhu dan kode rak — sangat menggoda untuk dipakai ulang. `DEC-BD-035`
menolaknya secara sadar: master itu berorientasi Farmasi, dimiliki tim lain, dan membawa atribut
obat yang tidak berlaku untuk darah. Tanpa master sendiri, tekanan untuk menyambungkannya akan
muncul lagi di setiap task berikutnya.

**Contoh.** Kulkas Besar rusak Selasa siang. Tanpa master lokasi, tidak ada cara menyatakan bahwa
kulkas itu tidak boleh dipakai lagi — petugas hanya bisa saling memberi tahu secara lisan, dan
kantong baru tetap dapat "disimpan" di sana menurut sistem.

---

## 2. Proses bisnis

**Tujuan.** Memberi BDRS satu tempat resmi untuk mendaftarkan kulkas darah yang benar-benar ada,
dan untuk menutup pemakaian lokasi yang sedang tidak layak.

**Pelaku.** Admin master data Bank Darah, lewat butir hak akses `BloodStorageLocation : *`.

**Pemicu.** Penyiapan modul Bank Darah sebelum kantong pertama diterima; atau kerusakan kulkas
yang menuntut penonaktifan.

**Langkah pada jalur normal:**

1. Admin membuka layar master lokasi penyimpanan darah. Kartu statistik menampilkan jumlah lokasi
   aktif — dan **menandai tegas** bila angkanya nol, karena keadaan itu menghentikan seluruh modul.
2. Admin menambah lokasi lewat `POST /`, mengisi kode dan nama yang dikenali petugas.
3. Layar kantong darah kelak memanggil `GET /options`, yang **hanya** mengembalikan lokasi aktif.
4. Ketika sebuah kulkas rusak, admin menonaktifkannya lewat `PATCH /{id}/status`.

**Aturan yang berlaku:**

| Aturan | Perilakunya |
| --- | --- |
| Kode dan nama sama-sama tunggal | `VAL-BD-067` menahan keduanya. Dua kulkas bernama sama membuat petugas tidak dapat membedakannya saat memilih lokasi |
| Penonaktifan tidak pernah ditolak | `VAL-BD-068`. Menonaktifkan lokasi justru dilakukan ketika kulkasnya rusak; menolaknya akan memaksa petugas memindahkan kantong ke lokasi yang sedang rusak |
| Penonaktifan tidak memindahkan apa pun | `DEC-BD-037`. Kantong tetap tercatat di sana dengan status yang sama persis. Yang tertutup hanya gerbang ke depan |
| Kotak pilihan tidak pernah menawarkan lokasi nonaktif | Penyaringan di backend dan **tidak dapat dimatikan pemanggil**, sehingga layar tidak dapat menawarkan lokasi nonaktif walaupun penulis layarnya lupa menyaring |
| Master kosong menghentikan modul | `INV-BD-025`. Ditandai `IsBloodBankHaltedByEmptyActiveLocation` pada ringkasan |

**Contoh berangka.** Kulkas Besar berisi dua belas kantong dinonaktifkan Selasa siang. Setelah
penonaktifan: dua belas kantong itu **tetap** tercatat di Kulkas Besar, **status kantongnya tidak
berubah**, dan Kulkas Besar **hilang** dari kotak pilihan penyimpanan. Yang tertahan hanya alokasi
keduabelas kantong tersebut, sampai petugas memindahkannya ke kulkas aktif.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Kode sudah dipakai lokasi lain | `409` — "Kode lokasi penyimpanan itu sudah dipakai. Gunakan kode lain." |
| Nama sudah dipakai lokasi lain | `409` — "Nama lokasi penyimpanan itu sudah dipakai. Gunakan nama lain." |
| Kode atau nama kosong | `400` dengan sebab yang disebut |
| Lokasi tidak ada atau sudah dihapus | `404` |
| Seluruh lokasi nonaktif, lalu `GET /options` dipanggil | `200` dengan daftar kosong, disertai keterangan bahwa kantong tidak dapat disimpan, dialokasikan, maupun diberikan |

**Hasil akhir.** BDRS dapat mengelola daftar kulkas darahnya sendiri, dan keadaan berbahaya
"tidak ada lokasi aktif" terbaca di halaman index sebelum ada pasien yang menunggu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` | QBE ID yang berlaku, termasuk larangan `SortOrder` generik |
| `rules/backend/engineering/QBE_EXCEPTIONS.json` | Memeriksa apakah sudah ada pengecualian terdaftar — **kosong** |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix `Mst` terdaftar dan `ACTIVE` |
| `rules/backend/master-data-endpoint-standard.md` | Sembilan endpoint baseline |
| `rules/backend/role-access-rules.md` | Pasangan `[AccessAction]`/`[AccessPermission]` |
| `docs/module-blueprints/bank-darah/data/data-dictionary.md` §`MstBloodStorageLocation` | Kontrak kolom |
| `docs/module-blueprints/bank-darah/contracts/api-contract.md` §Blood Storage Location | Kontrak endpoint |
| `docs/module-blueprints/bank-darah/contracts/validation-matrix.md` | `VAL-BD-067`, `VAL-BD-068` dan alasan `VAL-BD-068` meloloskan |
| `Areas/HealthServices/MasterData/**/BloodComponent*` | Pola rumah yang baru dibuat `BE-BD-001` — dijadikan acuan agar kedua master Bank Darah konsisten |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstBloodStorageLocation.cs` | **Baru.** Entity master, mewarisi `IdentityModel` |
| `Repositories/Configurations/HealthServices/MasterData/MstBloodStorageLocationConfiguration.cs` | **Baru.** Index unik `StorageLocationCode`, index `IsActive` |
| `Areas/HealthServices/MasterData/DTOs/BloodStorageLocationDtos.cs` | **Baru.** Response, option, create/update, status, ringkasan, metadata |
| `Areas/HealthServices/MasterData/Services/BloodStorageLocationService.cs` | **Baru.** Pemilik seluruh pembacaan dan perubahan master |
| `Areas/HealthServices/MasterData/Controllers/BloodStorageLocationController.cs` | **Baru.** Sembilan endpoint |
| `Areas/HealthServices/MasterData/Seeders/BloodStorageLocationSeeder.cs` | **Baru.** Dua lokasi minimum yang aktif |
| `Repositories/ApplicationDbContext.cs` | Menambah `DbSet<MstBloodStorageLocation>` |
| `Program.cs` | Mendaftarkan `BloodStorageLocationService` sebagai `Scoped` |
| `Migrations/20260903083142_AddMstBloodStorageLocation.cs` beserta `.Designer.cs` | **Baru.** Migration pembuatan tabel |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis oleh `dotnet ef` |
| `QuilvianSystemBackend.Tests/HealthServices/MasterData/BloodStorageLocationServiceTests.cs` | **Baru.** 25 pengujian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sembilan endpoint baru pada `api/v1/health-services/master-data/blood-storage-locations`. Kontrak `v4` menuliskan enam; tiga sisanya delta yang dituntut standar master data — lihat bagian 4 |
| Database | Satu tabel baru `public."MstBloodStorageLocation"`, satu index unik, satu index `IsActive`. **Migration sudah dibuat tetapi BELUM dijalankan** |
| Keamanan/Auth | Empat butir hak akses baru pada resource `BloodStorageLocation`: `Read`, `Create`, `Update`, `Delete`. Argumen pertama `[AccessPermission]` sama persis dengan `ControllerName`. Nol pemakaian `IsInRole`, nama peran, departemen, jabatan, maupun `UserType` |

---

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Blood Storage Location

Base URL: `api/v1/health-services/master-data/blood-storage-locations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi penyaring, pengurutan, dan isian form | `BloodStorageLocation : Read` |
| `GET` | `/summary` | Ringkasan jumlah, termasuk penanda modul berhenti karena tidak ada lokasi aktif | `BloodStorageLocation : Read` |
| `GET` | `/` | Daftar lokasi dengan pencarian, penyaringan, pengurutan, dan halaman | `BloodStorageLocation : Read` |
| `GET` | `/options` | Pilihan lokasi **aktif saja** untuk kotak isian layar kantong darah | `BloodStorageLocation : Read` |
| `GET` | `/{id}` | Detail satu lokasi | `BloodStorageLocation : Read` |
| `POST` | `/` | Menambah lokasi baru | `BloodStorageLocation : Create` |
| `PUT` | `/{id}` | Mengubah kode, nama, keterangan, dan status | `BloodStorageLocation : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan lokasi | `BloodStorageLocation : Update` |
| `DELETE` | `/{id}` | Menandai lokasi terhapus tanpa menghapus fisik | `BloodStorageLocation : Delete` |

Kode status: `200` berhasil · `400` isian tidak lengkap · `401` belum masuk · `403` tidak berhak ·
`404` tidak ditemukan atau sudah dihapus · `409` kode atau nama sudah dipakai (`VAL-BD-067`).

Contoh menemukan lokasi yang masih aktif:

```http
GET /api/v1/health-services/master-data/blood-storage-locations/options
```

**Tiga delta endpoint terhadap kontrak `v4`:**

| Endpoint | Sebab ditambahkan |
| --- | --- |
| `GET /filters/metadata` | Baseline wajib standar master data |
| `GET /summary` | Baseline wajib. Khusus master ini ia membawa `IsBloodBankHaltedByEmptyActiveLocation` — keadaan yang menghentikan seluruh modul (`INV-BD-025`) dan sebaiknya terlihat di halaman index |
| `DELETE /{id}` | Baseline wajib. Untuk keadaan sehari-hari **menonaktifkan tetap lebih tepat**, dan itu dinyatakan pada dokumentasi endpoint-nya. Bila pemilik menghendaki master ini nonaktif-saja seperti `MedicalRecordAccessPurpose`, endpoint ini dicabut dan `PATCH /{id}/status` menjadi satu-satunya jalan |

**Satu delta penamaan**, sama seperti `BE-BD-001`: blueprint menuliskan
`MstBloodStorageLocationController`/`Service`, berkasnya dibuat `BloodStorageLocationController`/
`Service` mengikuti 30 dari 30 controller master data yang tidak memakai prefix `Mst`. Nama entity,
configuration, DbSet, dan tabel tetap `MstBloodStorageLocation`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil — `0 Error(s)`, `186 Warning(s)` | `PASS` | Jumlah warning **identik** dengan sebelum task ini; nol warning baru |
| `dotnet ef migrations add AddMstBloodStorageLocation` | Migration terbentuk | `PASS` | `Migrations/20260903083142_AddMstBloodStorageLocation.cs` |
| Migration diadu dengan kamus data | Cocok, dengan satu selisih yang disengaja | `PASS` | Kolom, tipe, panjang, nullability, PK, index unik kode, dan index `IsActive` sesuai. Selisih `SortOrder` dijelaskan di bagian 8 |
| Snapshot model memuat entity baru | Terbukti | `PASS` | `Migrations/ApplicationDbContextModelSnapshot.cs` |
| 25 pengujian `BloodStorageLocationServiceTests` | `Failed: 0, Passed: 25` | `PASS` | Dijalankan bersama 34 pengujian task sebelumnya; total **59 lulus** |
| `dotnet test QuilvianSystemBackend.Tests` | **Tidak dapat dijalankan** | `EXISTING / ENVIRONMENT ISSUE` | Kerusakan pre-existing `PatientEncounterTestWorld.cs`, sama seperti dicatat `BE-BD-001.md`. **Masih belum diperbaiki pemiliknya** |

**Rincian 25 pengujian:**

| Kelompok | Jumlah | Yang dibuktikan |
| --- | ---: | --- |
| Pengelolaan dasar | 5 | Normalisasi kode, kode kembar ditolak, **nama kembar ditolak**, kode salah ketik dapat dibetulkan, isian kosong ditolak |
| Penonaktifan | 3 | `VAL-BD-068` selalu berhasil; `DEC-BD-037` tidak menyentuh kolom lain; dapat diaktifkan kembali |
| Kotak pilihan | 2 | Lokasi nonaktif dan lokasi terhapus tidak pernah ditawarkan |
| Ringkasan & daftar | 3 | `INV-BD-025` penanda modul berhenti, master kosong, halaman dihitung backend |
| Batas scope MVP | 3 | `AC-BD-064` nol kolom suhu/kapasitas/rak/hierarki; daftar kolom terkunci; bawaan `IsActive` aktif |
| Penghapusan | 1 | Penandaan, bukan penghapusan fisik |
| Seeder | 8 | Dua lokasi aktif, modul tidak lagi berhenti, dapat diulang, tidak menimpa, menolak produksi |

Uji manual: `NOT FEASIBLE` — menuntut database PostgreSQL yang sudah dimigrasikan; eksekusi
database adalah wewenang terpisah.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Eksekusi migration ke database | Wewenang terpisah |
| Pemeriksaan index unik fisik | Provider InMemory tidak menegakkannya; menjadi bagian verifikasi migration |
| Smoke test endpoint lewat HTTP | Menuntut aplikasi berjalan dengan database yang sudah dimigrasikan |
| `dotnet test` seluruh solusi | Terhalang kerusakan pre-existing |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-064` — sistem diminta mencatat suhu atau kapasitas: tidak ada kolom maupun endpoint | **Terpenuhi** | `Entity_TidakPunyaKolomSuhuKapasitasRakMaupunHierarki` dan `Entity_HanyaPunyaKolomYangDitetapkanKamusData` |
| `VAL-BD-067` — kode atau nama sudah dipakai | **Terpenuhi** | Dua pengujian terpisah untuk kode dan nama |
| `VAL-BD-068` — penonaktifan meloloskan, bukan menolak | **Terpenuhi sebagian** | Penonaktifan selalu berhasil dan pesannya menjelaskan akibatnya. **Jumlah kantong belum dapat disebut** — lihat bagian 8 |
| DoD — lokasi nonaktif hilang dari pilihan | **Terpenuhi** | `Pilihan_TidakPernahMenawarkanLokasiNonaktif` |
| DoD — penonaktifan **tidak** memindahkan kantong | **Terpenuhi secara struktural** | `MenonaktifkanLokasi_TidakMenyentuhKolomLain`. Tidak ada satu baris kode pun di task ini yang menyentuh kantong, dan kantong memang belum ada |
| DoD — minimal satu lokasi aktif terseed | **Terpenuhi** | Seeder mengisi dua lokasi aktif; `Seeder_MembuatModulTidakLagiBerhenti` |
| DoD — `MstDrugStorageLocation` tidak disentuh | **Terpenuhi** | Nol berkas Farmasi berubah; penolakannya dicatat pada komentar entity |
| `AC-BD-062`, `AC-BD-065`, `AC-BD-066` — lokasi nonaktif ditolak untuk penyimpanan dan perpindahan (`VAL-BD-060`) | **Belum terpenuhi — bukan milik task ini** | Penolakan terjadi di jalur penyimpanan kantong, dan `BbkBloodUnitPlacement` belum ada. Menjadi acceptance `BE-BD-015` |
| `AC-BD-067` — penonaktifan berhasil dan peringatan menyebut jumlah kantong | **Belum terpenuhi — bukan milik task ini** | Bagian "berhasil" terpenuhi; bagian "menyebut jumlah" menuntut tabel penempatan |

### Kenapa empat acceptance itu tidak dipaksakan masuk

Keempatnya menuntut **penempatan kantong**, yang secara eksplisit dikeluarkan dari scope task ini.
Menuliskan pengujian tiruan yang seolah-olah membuktikannya akan menghasilkan bukti palsu.

Yang task ini jamin adalah prasyaratnya: ketika `BE-BD-015` menegakkan `VAL-BD-060`, master
lokasinya sudah ada, penanda aktifnya sudah bekerja, dan kotak pilihannya sudah menyaring sendiri.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 186 warning build, jumlahnya **identik** dengan sebelum task ini. Nol warning baru |
| Masalah yang diketahui | Tiga hal, seluruhnya dirinci pada bagian 8: selisih `SortOrder` terhadap kamus data, pemeriksaan nama tanpa index unik database, dan pencacahan kantong pada `VAL-BD-068` |
| Risiko tersisa | Migration **belum dijalankan**. Seeder mengisi dua lokasi bernama "Kulkas Besar" dan "Kulkas Kecil" yang **wajib disesuaikan BDRS** dengan kulkas yang benar-benar ada sebelum dipakai sungguhan — seeder menolak berjalan di produksi justru untuk itu |
| Perubahan sampingan | `Migrations/ApplicationDbContextModelSnapshot.cs` berubah otomatis oleh `dotnet ef migrations add` |
| Interupsi | **Ada.** Sesi terputus setelah entity, configuration, DTO, service, controller, dan seeder selesai dibuat. Pemulihan dilakukan dengan memeriksa `git status` dan isi berkas: registrasi `DbSet` dan DI ternyata **sudah** terpasang sebelum terputus, sehingga tidak dikerjakan ulang. Pekerjaan dilanjutkan dari migration. Nol penyuntingan ganda |
| Status Git | Lihat di bawah |
| Langkah berikutnya | 1. `BE-BD-016` seeder hak akses — task terakhir `MVP-0`. 2. Selesaikan sisa `BE-BD-001` bagian `MstBloodBankReason`. 3. Jalankan ketiga migration `MVP-0` lewat wewenang eksekusi database terpisah. 4. Pemilik proses memutuskan tiga gap pada bagian 8. 5. Pemilik Registration Management memperbaiki `PatientEncounterTestWorld.cs` |

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md
?? Areas/HealthServices/MasterData/Controllers/BloodStorageLocationController.cs
?? Areas/HealthServices/MasterData/DTOs/BloodStorageLocationDtos.cs
?? Areas/HealthServices/MasterData/Models/MstBloodStorageLocation.cs
?? Areas/HealthServices/MasterData/Seeders/BloodStorageLocationSeeder.cs
?? Areas/HealthServices/MasterData/Services/BloodStorageLocationService.cs
?? Migrations/20260903083142_AddMstBloodStorageLocation.Designer.cs
?? Migrations/20260903083142_AddMstBloodStorageLocation.cs
?? QuilvianSystemBackend.Tests/HealthServices/MasterData/BloodStorageLocationServiceTests.cs
?? Repositories/Configurations/HealthServices/MasterData/MstBloodStorageLocationConfiguration.cs
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-014.md
```

---

## 8. Gap yang ditemukan dan sengaja tidak diimplementasikan

### 8.1 ⚠️ `SortOrder` — kontrak `v4` menuntutnya, kontrak engineering melarangnya

**Ini butuh keputusan pemilik, dan saya tidak mengambilnya sendiri secara diam-diam.**

`data/data-dictionary.md` §`MstBloodStorageLocation` menetapkan kolom `SortOrder` (`int`, wajib,
bawaan `0`, keterangan "Urutan tampil pilihan"), dan `contracts/api-contract.md` menuliskan
`PUT /{id}` "Ubah kode, nama, **urutan**, keterangan".

`BACKEND_ENGINEERING_CONTRACT.md` menyatakan sebaliknya, dan kalimatnya mengikat:

> `SortOrder` presentasi yang dipersistensi secara generik **dilarang untuk kode baru**; urutan
> bisnis yang sesungguhnya memakai field semantik. `SortOrder` pada DTO, form, permission, dan UI
> tetap sah.

`QBE_EXCEPTIONS.json` **kosong**, jadi tidak ada pengecualian terdaftar yang menutupi ini.

**Yang saya kerjakan: mengikuti kontrak engineering — kolom `SortOrder` tidak dibuat.** Tiga alasan:

1. Perintah task ini berbunyi "Ikuti Backend Engineering Contract" secara eksplisit.
2. Urutan penggantinya semantik dan deterministik: `StorageLocationCode` lalu `StorageLocationName`.
   Kulkas memang lebih wajar diurutkan menurut kodenya daripada menurut angka yang diketik admin.
3. Belum ada konsumen yang rusak. `FE-BD-011` belum dibangun, dan menambah kolom ini kelak adalah
   migration aditif satu baris bila pemilik memutuskan sebaliknya.

**Yang pemilik perlu putuskan** — salah satu dari dua:

| Pilihan | Tindak lanjut |
| --- | --- |
| Urutan kode/nama cukup | Amandemen kamus data dan `api-contract.md` untuk mencabut `SortOrder`. Nol perubahan source |
| Urutan manual memang dibutuhkan | Daftarkan pengecualian pada `QBE_EXCEPTIONS.json` beserta QBE ID, alasan, dan cakupannya; lalu satu migration aditif menambahkan kolomnya |

Catatan pembanding: `MstMedicalRecordAccessPurpose` — kode baru yang belum lama ini masuk —
**memiliki** `SortOrder` yang dipersistensi. Jadi preseden rumah tidak seragam, dan itu justru
alasan tambahan untuk menyelesaikannya lewat keputusan tertulis, bukan lewat kebiasaan.

### 8.2 Nama lokasi tunggal tanpa index unik database

`VAL-BD-067` menahan **kode atau nama** yang sudah dipakai. Kamus data hanya menetapkan index unik
untuk **kode**.

Yang dikerjakan: kode dijaga index unik database, nama dijaga pemeriksaan di service. Akibatnya
nama menyisakan celah balapan yang sangat sempit — dua petugas yang menyimpan nama sama pada saat
hampir bersamaan dapat lolos berdua.

Tidak ditambahkan sendiri karena menambah index unik di luar kamus data adalah perubahan kontrak
database. **Untuk pemilik:** bila nama memang wajib tunggal mutlak, tambahkan index uniknya pada
kamus data, lalu satu migration aditif menegakkannya.

### 8.3 `VAL-BD-068` belum dapat menyebut jumlah kantong

Pesan `VAL-BD-068` pada kontrak berbunyi "Ada **N** kantong yang masih tercatat di sana". Menghitung
N menuntut `BbkBloodUnitPlacement`, yang belum ada.

Yang dikerjakan: penonaktifan berhasil dan pesannya menjelaskan akibatnya secara benar, tetapi
**tanpa angka**. Pencacahan ditambahkan pada `BE-BD-015` yang membuat tabel penempatannya.

### 8.4 Kebutuhan di luar scope yang tidak disentuh sama sekali

| Kebutuhan | Keadaan |
| --- | --- |
| Pemantauan suhu dan rantai dingin | Dikeluarkan `DEC-BD-035` dari MVP. Nol kolom, nol endpoint — dan diuji `AC-BD-064` |
| Kapasitas dan pencacahan isi | Di luar MVP. Nol kolom |
| Rak, laci, hierarki gudang | Di luar MVP. Nol kolom |
| Integrasi IoT/sensor | Di luar MVP dan di luar batas modul |
| Penempatan kantong dan riwayat perpindahan | `BE-BD-015` |
| Penegakan `VAL-BD-060` | `BE-BD-015` |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MasterData` |
| Submodule | Tidak berlaku |
| Pemilik/prefix registry | `Administrator / HealthServices` · `Master / Reference` · prefix **`Mst`** · Lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` |
| Status registry | Terdaftar dan `ACTIVE`. Nol entri registry baru dibutuhkan |
| Catatan ownership | Master ini secara **bisnis** dimiliki BDRS (`DEC-BD-035`), tetapi secara **struktur** tinggal di `MasterData` dengan prefix `Mst`, sama seperti `MstBloodComponent`. Blueprint §D menetapkan lokasi berkasnya persis demikian |

**QBE ID yang berlaku dan cara pemenuhannya:**

| QBE ID | Pemenuhan |
| --- | --- |
| `QBE-ENT-001` | Mewarisi `IdentityModel` |
| `QBE-ENT-002` | `Guid` PK, nullability mengikuti semantik domain |
| `QBE-ENT-003` | **Nol field presentasi dipersistensi** — termasuk `SortOrder`; lihat bagian 8.1. `IsBloodBankHaltedByEmptyActiveLocation` **dihitung** pada DTO, tidak disimpan |
| `QBE-NAM-001` | Nol pemakaian `Trx*` |
| `QBE-NAM-002`, `QBE-NAM-004` | Prefix `Mst` dari registry |
| `QBE-CFG-001` | Configuration menyediakan mapping, key, dan dua index |
| `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` | Capability di bawah Area/Module pemiliknya yang sudah terdaftar |
| `QBE-SVC-001` | Controller **tidak** menyentuh `ApplicationDbContext` |
| `QBE-API-001` | Seluruh response terbungkus `ApiResponse<T>`; daftar memakai `PagedResult<T>` |
| `QBE-PERM-001` | `[AccessController]`, `[AccessAction]`, `[AccessPermission]` pada seluruh action |
| `QBE-LOG-001` | `LoggerService` mencatat Create, Update, UpdateStatus, Delete beserta pelakunya. `GET` tidak dicatat, mengikuti konvensi project |
| `QBE-VAL-001` | Validasi request dan `VAL-BD-067` di service |
| `QBE-DTO-001` | Entity EF tidak pernah dikembalikan sebagai kontrak API |
| `QBE-PAGE-001` | Paging, pencarian, pengurutan memakai pola yang sudah mapan |
| `QBE-OPT-001` | `/options` dan `/filters/metadata` disediakan karena layar master mengonsumsinya |
| `QBE-DEL-001` | Soft delete beserta `DeleteDateTime` dan `DeleteBy` |
| `QBE-AUD-001` | Audit database terpisah dari application logging |

**QBE ID yang TIDAK berlaku, beserta alasannya:**

| QBE ID | Alasan tidak berlaku |
| --- | --- |
| `QBE-CODE-001`..`006` | `StorageLocationCode` bukan nomor bisnis yang dialokasikan sistem. Ia penanda fisik yang ditulis pengguna — `KLK-BSR`, bukan nomor urut. `QBE-CODE-004` tetap dihormati semangatnya lewat index unik |
| `QBE-TXN-001` | Seluruh operasi menyentuh satu baris pada satu tabel |
| `QBE-ENUM-001` | Nol enum baru. Ketiadaan enum tipe lokasi disengaja — tipe kulkas adalah atribut Farmasi yang ditolak `DEC-BD-035` |
| `QBE-CFG-002` | Nol configuration legacy disentuh |
| `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` | Khusus `LEGACY MIGRATION`; task ini `NEW CODE` |
