# Laporan Perubahan Backend — `BE-RAD-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-04` |
| Judul | Kelola alat pencitraan |
| Slice | `S13` — Data induk alat pencitraan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-001` butir 14, `RAD-DEC-002`, `RJ-BIL-DEC-014`; API `RAD-API-001` grup *Master Data / Rad Modality*; validation `RAD-VAL-001` bagian 3; permission `RAD-PERM-001` bagian 4; kemampuan `RAD-CAP-001` |
| Contract version | `RAD-API-001` **revision 4** dan `RAD-PERM-001` **revision 4**, keduanya `approved` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` — memuat `BE-RAD-03` beserta amandemen kontrak revision 3 |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** Build lulus 0 error; 61 uji radiologi lulus, 19 di antaranya baru. Alat pencitraan kini dapat didaftarkan lewat aplikasi tanpa menyentuh database langsung |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Master Data |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` sejak 2026-09-10 |
| Keberlakuan | `NEW CODE` untuk `RadModalityController`, `RadModalityService`, dan DTO-nya; `TOUCHED LEGACY` untuk `RadOperationResult` dan `RadiologyDtos` yang diperluas |
| QBE ID yang berlaku | `QBE-MOD-001`; `QBE-SVC-001` orkestrasi dan akses data dimiliki service, controller tidak menyentuh `ApplicationDbContext`; `QBE-API-001`; `QBE-DTO-001`; `QBE-PERM-001`; `QBE-PAGE-001`; `QBE-VAL-001`; `QBE-DEL-001` lifecycle delete dihormati beserta audit aktornya |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — tidak ada entity, configuration, penamaan fisik, maupun migration; `QBE-CODE-*` — kode alat diisi pengguna mengikuti kode DICOM, bukan dialokasikan sistem |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Alat pencitraan hanya dapat didaftarkan dengan menulis langsung ke tabel `MstRadModality`.
Akibatnya penambahan satu alat baru — CT kedua, USG di poli, pesawat rontgen pengganti —
menuntut seorang programmer menjalankan perintah `INSERT`, tanpa jejak siapa mendaftarkan dan
tanpa satu pun pemeriksaan.

Yang lebih berbahaya adalah arah sebaliknya. Tanpa endpoint, **tidak ada yang menahan alat
dinonaktifkan sementara aturan keselamatannya masih berlaku**.

> **Contoh nyata.** Rumah sakit mengganti CT lama dengan CT baru. Admin menandai CT lama tidak
> aktif langsung di database. Aturan "skrining kehamilan wajib untuk CT" tetap tersimpan,
> menggantung pada alat yang sudah tidak ada — sementara CT baru belum punya aturan apa pun dan
> menolak seluruh pemeriksaannya. Dua-duanya tidak terlihat sampai pasien pertama ditolak di
> depan alat.

Task ini menutup keduanya: pendaftaran lewat aplikasi dengan jejak lengkap, dan penjagaan yang
menolak memensiunkan alat yang aturannya masih berlaku.

---

## 2. Proses bisnis

**Tujuan.** Memberi admin Radiologi cara mengelola daftar alat pencitraan sendiri, tanpa
meninggalkan aturan keselamatan yang menggantung.

**Pelaku.** Admin Radiologi — memegang `RadModality : Read`, `Create`, `Update`, dan `Delete`.

**Langkah berurutan pada layar.**

1. Layar dibuka. `GET /filters/metadata` memberi pilihan penyaring dan pengurutan;
   `GET /summary` mengisi kartu statistik, termasuk **berapa alat aktif yang belum punya aturan
   keselamatan**; `GET /` mengisi tabel.
2. Admin menekan Tambah, mengisi kode dan nama alat, lalu `POST /` menyimpannya.
3. Perubahan data alat lewat `PUT /{id}`; menyalakan atau mematikan alat lewat
   `PATCH /{id}/status`; menghapus lewat `DELETE /{id}`.
4. Form lain yang perlu memilih alat — misalnya penyusunan aturan keselamatan — memakai
   `GET /options`, yang bawaannya hanya memuat alat yang masih dipakai.

**Aturan yang berlaku, dengan contohnya.**

| Aturan | Contoh |
| --- | --- |
| Kode alat wajib diisi | Kode kosong ditolak `400` |
| Nama alat wajib diisi | Nama kosong ditolak `400` |
| Kode alat unik | Sudah ada `CT`; mendaftarkan `CT` lagi ditolak `409` |
| Huruf besar-kecil tidak membedakan | Sudah ada `CT`; mendaftarkan `ct` **tetap** ditolak `409`, karena bagi manusia keduanya alat yang sama |
| Alat yang masih dipakai tidak dapat dipensiunkan | CT punya aturan "skrining kehamilan" yang berlaku; menonaktifkan atau menghapus CT ditolak `409` |
| Kode alat yang sudah dihapus dapat dipakai ulang | Alat `RF` dihapus; mendaftarkan `RF` baru diterima |

**Urutan yang benar saat mengganti alat.**

1. Hentikan aturan keselamatan yang berlaku pada alat lama —
   `POST /rad-safety-rules/{id}/deactivate`, wewenang penanggung jawab klinis.
2. Baru pensiunkan alatnya — `PATCH /rad-modalities/{id}/status`.
3. Daftarkan alat baru, lalu susun dan sahkan aturan keselamatannya.

Urutan itu sengaja tidak dapat dibalik. Aturan keselamatan adalah kebijakan klinis; mencabutnya
sebagai efek samping mematikan sebuah alat berarti mengubah kebijakan tanpa sepengetahuan
penanggung jawabnya.

**Jalur tidak normal.**

| Keadaan | Kode | Yang terjadi |
| --- | :---: | --- |
| Alat tidak ditemukan atau sudah dihapus | `404` | "Alat pencitraan tidak ditemukan atau sudah dihapus." |
| Kode kembar | `409` | "Kode alat sudah dipakai. Gunakan kode lain." |
| Alat masih dipakai aturan berlaku | `409` | "Alat ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan aturannya lebih dulu." |
| Dua pendaftaran kode sama berjalan bersamaan | `409` | Index unik pada database menolak yang kedua; pesannya sama, bukan galat database mentah |

**Hasil akhir.** `RAD-CAP-001` berpindah dari `Repair` menjadi lengkap di sisi backend.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan standar.** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`;
`rules/backend/REVIEW_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`;
`rules/backend/master-data-endpoint-standard.md`; `rules/rule-output/`.

**Kontrak.** `contracts/api-contract.md` grup *Rad Modality*; `contracts/validation-matrix.md`
bagian 3; `contracts/permission-audit-matrix.md` bagian 4; `roadmap/backend-roadmap.md`;
`roadmap/requirement-traceability.md`; laporan `BE-RAD-03.md`.

**Source pembanding.** `Models/MstRadModality.cs`;
`Repositories/Configurations/.../MstRadModalityConfiguration.cs`;
`Services/RadSafetyPolicyService.cs` dan `Controllers/RadSafetyRuleController.cs` sebagai pola
terdekat yang baru dibuat; `MasterData/Controllers/ServiceUnitController.cs` sebagai rujukan
bentuk kontrak master data; `Responses/PagedResult.cs`; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Controllers/RadModalityController.cs` | **Baru.** Sembilan endpoint baseline master data beserta seluruh metadata Access dan `[ProducesResponseType]` |
| `Areas/.../Services/RadModalityService.cs` | **Baru.** Metadata, rekap, daftar berhalaman, pilihan dropdown, rincian, tambah, ubah, ubah status, hapus |
| `Areas/.../DTOs/RadModalityDtos.cs` | **Baru.** Request, response rincian, pilihan, metadata, rekap, dan penyaring daftar |
| `Areas/.../DTOs/RadiologyDtos.cs` | `RadModalityResponse` ditambah `Description` dan `SortOrder` — additive, tidak merusak pemakainya yang sudah ada |
| `Areas/.../Services/RadOperationResult.cs` | Dua kode galat baru: `RAD_MODALITY_CODE_ALREADY_USED` dan `RAD_MODALITY_STILL_IN_USE` |
| `Program.cs` | Pendaftaran `RadModalityService` |
| `Tests/.../RadiologyManagement/RadModalityServiceTests.cs` | **Baru.** 19 uji tanpa database |

### 3.3 Keputusan rancangan yang perlu diketahui

**Kode alat disimpan dalam huruf besar.** `ct` dan `CT` adalah alat yang sama bagi manusia.
Membiarkan keduanya berdampingan membuat aturan keselamatan terpasang pada salah satunya saja —
dan alat kembarannya berjalan tanpa penjagaan.

**Penghapusan memakai soft delete.** Alat yang pernah dipakai memeriksa pasien tidak boleh
hilang dari jejak. Kode alat yang sudah dihapus boleh dipakai ulang, mengikuti pola master data
yang sudah berjalan di repository ini.

**Penanda kesiapan gerbang ditulis sebagai subquery, bukan fungsi C#.** Percobaan pertama
menuliskannya sebagai method biasa yang menerima satu alat. Entity Framework tidak dapat
menerjemahkan method semacam itu menjadi SQL, dan memaksakannya membuat seluruh tabel ditarik ke
memori lebih dulu. Pada tabel alat hal itu tidak terasa, tetapi kebiasaannya menular ke tempat
yang jauh lebih besar. Penyaringnya sama persis dengan yang dipakai gerbang keselamatan sejak
`BE-RAD-06`.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint yang direncanakan `RAD-API-001` kini tersedia, ditambah empat endpoint baseline standar master data. `RadModalityResponse` bertambah dua field — additive, konsumen lama tidak perlu berubah |
| Database | Tidak ada perubahan schema, entity, index, maupun migration. **Tidak ada satu pun perintah database yang dijalankan.** Tabel `MstRadModality` beserta index uniknya sudah ada sejak migration awal modul |
| Keamanan/Auth | Setiap endpoint memuat `[AccessPermission("RadModality", ...)]` dengan string persis seperti `RAD-PERM-001` bagian 4. Tidak ada atribut otorisasi yang dilemahkan |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/radiology-management/master-data/rad-modalities`

#### Health Services / Radiology Management / Master Data / Rad Modality

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring dan pengurutan layar daftar alat | `RadModality : Read` |
| `GET` | `/summary` | Rekap jumlah alat, termasuk yang belum punya aturan keselamatan | `RadModality : Read` |
| `GET` | `/` | Daftar alat dengan pencarian, penyaringan, pengurutan, dan halaman | `RadModality : Read` |
| `GET` | `/options` | Pilihan ringan untuk dropdown pada form lain | `RadModality : Read` |
| `GET` | `/{id}` | Rincian satu alat beserta jejak audit dan jumlah aturan berlakunya | `RadModality : Read` |
| `POST` | `/` | Mendaftarkan alat baru | `RadModality : Create` |
| `PUT` | `/{id}` | Mengubah data alat | `RadModality : Update` |
| `PATCH` | `/{id}/status` | Menyalakan atau mematikan alat | `RadModality : Update` |
| `DELETE` | `/{id}` | Menandai alat terhapus tanpa menghapus fisik | `RadModality : Delete` |

Contoh pemanggilan daftar alat yang **belum siap dipakai memeriksa**:

```http
GET /api/v1/health-services/radiology-management/master-data/rad-modalities
    ?isActive=true&hasActiveSafetyRule=false&sortBy=sortOrder&sortDirection=asc
```

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan** sesuai permintaan pemilik modul.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -m:1 -p:RunAnalyzers=false -p:EnforceCodeStyleInBuild=false` | Berhasil, **0 error**, 2 menit 40 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build` — percobaan pertama | **2 gagal, 59 lulus** | `NEW ERROR`, sudah diperbaiki | Perancah uji, bukan service. Lihat penjelasan di bawah |
| `dotnet test --no-build` — setelah perbaikan | **61 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| Alat baru dapat didaftarkan | Kode tersimpan huruf besar; `CreateBy` terisi; belum punya aturan keselamatan | `PASS` | `AlatBaruDapatDidaftarkanTanpaMenyentuhDatabaseLangsung` |
| **AC-5** — kode alat kembar | Ditolak `409` dengan `RAD_MODALITY_CODE_ALREADY_USED` | `PASS` | `KodeAlatKembar_Ditolak409` |
| Kode kembar berbeda huruf besar-kecil | Tetap ditolak | `PASS` | `KodeAlatKembarBerbedaHurufBesarKecil_TetapDitolak` |
| Kode atau nama kosong | Ditolak `400` dengan pesan yang menyebut field-nya | `PASS` | `KodeAtauNamaKosong_Ditolak400` |
| Mengubah alat tanpa mengubah kodenya | Tidak dianggap kembar dengan dirinya sendiri | `PASS` | `MengubahAlat_TidakMenganggapKodenyaSendiriKembar` |
| Mengubah alat memakai kode milik alat lain | Ditolak `409` | `PASS` | `MengubahAlatMemakaiKodeMilikAlatLain_Ditolak409` |
| **AC-5** — menonaktifkan alat yang masih dipakai aturan berlaku | Ditolak `409`; alat **tetap aktif** | `PASS` | `MenonaktifkanAlatYangMasihDipakaiAturanBerlaku_Ditolak409` |
| Menghapus alat yang masih dipakai aturan berlaku | Ditolak `409`; baris **tidak** ditandai terhapus | `PASS` | `MenghapusAlatYangMasihDipakaiAturanBerlaku_Ditolak409` |
| Alat dapat dipensiunkan setelah aturannya dihentikan | Berhasil | `PASS` | `AlatDapatDipensiunkanSetelahAturannyaDihentikan` |
| Aturan yang masih draf tidak menahan penonaktifan | Berhasil — draf belum menjaga siapa pun | `PASS` | `AturanYangMasihDrafTidakMenahanPenonaktifan` |
| Alat yang sudah dihapus | Hilang dari daftar dan rincian; barisnya tetap ada di database | `PASS` | `AlatYangSudahDihapus_TidakMunculLagi` |
| Kode alat yang sudah dihapus dapat dipakai ulang | Berhasil | `PASS` | `KodeAlatYangSudahDihapus_DapatDipakaiUlang` |
| Daftar menyaring kesiapan gerbang | `true` memuat CT, `false` memuat USG | `PASS` | `DaftarMenyaringKesiapanGerbang` |
| Pencarian pada kode, nama, dan keterangan | Ketiganya menemukan | `PASS` | `DaftarMencariPadaKodeNamaDanKeterangan` |
| Permintaan halaman tidak masuk akal | `pageNumber` 0 menjadi 1; `pageSize` 0 menjadi 25; 500 dibatasi 100 | `PASS` | `DaftarMenormalkanPermintaanHalamanYangTidakMasukAkal` |
| Dropdown hanya memuat alat yang masih dipakai | Bawaannya satu; dengan `onlyActive=false` menjadi dua | `PASS` | `PilihanDropdownHanyaMemuatAlatYangMasihDipakai` |
| Rekap menghitung alat yang belum punya aturan | Hanya USG; alat yang sudah dipensiunkan tidak dihitung | `PASS` | `RekapMenghitungAlatYangBelumPunyaAturanKeselamatan` |
| Metadata tidak menjanjikan penyaring yang tidak diproses | Setiap parameter yang disebut memang dilayani query | `PASS` | `MetadataTidakMenjanjikanPenyaringYangTidakDiproses` |
| Regresi 42 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | Tiga berkas uji radiologi sebelumnya |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT FEASIBLE` — layar `FE-RAD-02` belum dibuat.

### Mengapa dua uji gagal pada percobaan pertama

Kegagalannya berbunyi "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan",
dan penyebabnya **bukan** cacat pada service.

`HttpContextAccessor` menyimpan context pada `AsyncLocal` yang dipakai bersama seluruh
instansnya, dan setternya **mengosongkan context milik accessor sebelumnya**. Dua uji itu
menyimpan satu service, lalu memanggil helper yang membuat service lain untuk menyahkan aturan
keselamatan, lalu memakai service pertamanya kembali — yang identitasnya sudah terhapus helper
tadi.

Di runtime nyata hal ini tidak terjadi: setiap request punya scope dan `HttpContext` sendiri.
Yang diperbaiki karena itu adalah perancah ujinya — service dibuat ulang pada titik pemakaian —
dan alasannya dicatat pada komentar helper `Service()` supaya tidak terulang. Penolakan
service terhadap identitas kosong justru perilaku yang benar dan sengaja tidak diubah.

### Batas verifikasi yang perlu diketahui

Seluruh uji berjalan di atas penyedia **in-memory**, bukan PostgreSQL. Yang **belum** terbukti:
terjemahan SQL untuk pencarian `Contains` dan subquery kesiapan gerbang, serta index unik
`ModalityCode` sebagai penjaga terakhir kode kembar. Pemeriksaan itu menunggu database test
dikonfigurasi.

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Migration | Tidak ada perubahan schema; tabel dan index-nya sudah ada sejak migration awal modul |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi; mengisinya wewenang terpisah |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-5 — kode alat kembar ditolak `409` | **Terpenuhi** | Dua uji, termasuk kembar yang berbeda huruf besar-kecil |
| AC-5 — menonaktifkan alat yang masih dipakai aturan aktif ditolak `409` | **Terpenuhi** | Dua uji: penonaktifan dan penghapusan, keduanya ditahan |
| Alat baru dapat didaftarkan tanpa menyentuh database langsung | **Terpenuhi** | `POST /` beserta seluruh jejak audit aktornya |

Tidak ada butir Definition of Done `BE-RAD-04` yang belum terpenuhi.

---

## 7. Delta terhadap roadmap

Roadmap menuliskan "CRUD sederhana, memakai `ApplicationDbContext` langsung sesuai pola
project". **Tidak diikuti**, dan alasannya perlu dicatat terbuka.

`AGENTS.md` menetapkan urutan wewenang: kontrak rekayasa backend mengalahkan pola source yang
sudah ada, dan menyatakannya eksplisit — keberadaan controller yang mengakses `DbContext`
langsung "tidak memberi wewenang untuk menggunakan pola tersebut dalam `NEW CODE`". `QBE-SVC-001`
menuntut orkestrasi domain dimiliki Module Service.

Karena itu akses datanya ditempatkan di `RadModalityService`, sama seperti `BE-RAD-03`. Hasilnya
juga lebih baik untuk task ini sendiri: penjagaan "alat masih dipakai aturan berlaku" perlu
membaca tabel aturan keselamatan, dan aturan sepenting itu lebih layak tinggal di service yang
dapat diuji langsung daripada di dalam action controller.

Roadmap juga menyebut lima endpoint; yang dibuat sembilan. Empat tambahannya —
`filters/metadata`, `summary`, `options`, dan `PATCH /{id}/status` — adalah baseline wajib
`master-data-endpoint-standard.md` untuk setiap capability master data.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | **Pertama**, penerjemahan query ke PostgreSQL belum diuji. **Kedua**, string hak akses belum dibuktikan cocok dengan baris izin di database — menunggu `BE-RAD-14`. **Ketiga**, alat yang sudah terdaftar tetap menolak pemeriksaan sampai aturan keselamatannya disusun dan disahkan; `GET /summary` dan `GET /rad-safety-rules/coverage` sama-sama menampilkan angka itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-05` — kelola butir keselamatan, bentuknya sejajar dengan task ini. Sesudahnya `BE-RAD-15` mengisi data master awal sampai `GET /coverage` mengembalikan daftar kosong |

### Status Git pada akhir pekerjaan

Berkas yang berubah karena `BE-RAD-04`:

```text
 M Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs
 M Program.cs
?? Areas/HealthServices/RadiologyManagement/Controllers/RadModalityController.cs
?? Areas/HealthServices/RadiologyManagement/DTOs/RadModalityDtos.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadModalityService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadModalityServiceTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-04.md
```

Berkas dokumentasi yang ikut diperbarui: `contracts/api-contract.md`,
`contracts/permission-audit-matrix.md`, `roadmap/backend-roadmap.md`, dan
`roadmap/requirement-traceability.md`.

Empat berkas Laboratorium (`LabPatientRegistration*`) yang berubah di worktree **bukan** hasil
pekerjaan ini dan sengaja dibiarkan utuh.

Tidak ada `git add`, commit, maupun push yang dilakukan.
