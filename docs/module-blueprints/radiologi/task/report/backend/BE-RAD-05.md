# Laporan Perubahan Backend — `BE-RAD-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-05` |
| Judul | Kelola butir keselamatan |
| Slice | `S13` — Data induk alat pencitraan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-005`; API `RAD-API-001` grup *Master Data / Rad Safety Requirement*; validation `RAD-VAL-001` bagian 3; permission `RAD-PERM-001` bagian 4; kemampuan `RAD-CAP-002` |
| Contract version | `RAD-API-001` **revision 5** dan `RAD-PERM-001` **revision 5**, keduanya `approved` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** Build lulus 0 error; 78 uji radiologi lulus, 17 di antaranya baru |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Master Data |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` sejak 2026-09-10 |
| Keberlakuan | `NEW CODE` untuk `RadSafetyRequirementController`, `RadSafetyRequirementService`, dan DTO-nya; `TOUCHED LEGACY` untuk `RadOperationResult` |
| QBE ID yang berlaku | `QBE-MOD-001`; `QBE-SVC-001`; `QBE-API-001`; `QBE-DTO-001`; `QBE-PERM-001`; `QBE-PAGE-001`; `QBE-VAL-001`; `QBE-DEL-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*`, `QBE-CODE-*` |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Butir keselamatan — "skrining kehamilan", "skrining implan logam", "alergi kontras" — hanya
dapat ditambahkan dengan menulis langsung ke tabel `MstRadSafetyRequirement`. Padahal daftar
itulah kosakata yang dipakai admin ketika menyusun aturan keselamatan pada `BE-RAD-03`.

Yang lebih berbahaya, sama seperti pada alat pencitraan: **tidak ada yang menahan sebuah butir
dihapus sementara aturan yang memakainya masih berlaku**.

> **Contoh nyata.** Admin merapikan daftar butir dan menghapus "skrining kehamilan" karena
> dianggap kembar dengan butir lain. Aturan "skrining kehamilan wajib untuk CT-Scan" tetap
> berjalan, tetapi pertanyaannya kehilangan rumusan. Layar tetap menampilkan satu baris
> pemeriksaan yang harus dijawab — dengan nama yang kosong atau menyesatkan. Petugas tetap
> mencentangnya, dan jejaknya tetap tercatat sebagai "dijawab aman".

Pertanyaan keselamatan yang kehilangan rumusannya lebih buruk daripada pertanyaan yang tidak
pernah ada, karena ia tetap terlihat dijawab.

---

## 2. Proses bisnis

**Tujuan.** Memberi admin Radiologi cara mengelola kosakata butir keselamatan, tanpa merusak
aturan yang sedang berjalan.

**Yang perlu dipahami lebih dulu: ini kosakata, bukan kebijakan.** Menambah butir di sini
**tidak** membuatnya berlaku bagi seorang pasien pun. Ia hanya menjadi pilihan yang tersedia
ketika admin menyusun aturan keselamatan untuk sebuah alat — dan aturan itulah yang wajib
disahkan penanggung jawab klinis sebelum mengikat.

**Pelaku.** Admin Radiologi — `RadSafetyRequirement : Read`, `Create`, `Update`, `Delete`.

**Langkah berurutan.**

1. `GET /filters/metadata` memberi pilihan penyaring, termasuk **daftar kelompok butir yang
   benar-benar dipakai data saat ini**; `GET /summary` mengisi kartu statistik; `GET /` mengisi
   tabel.
2. Admin menambah butir lewat `POST /`, mengubahnya lewat `PUT /{id}`.
3. Form penyusunan aturan keselamatan memakai `GET /options` untuk daftar pilihannya.
4. Butir yang sudah tidak dipakai dipensiunkan lewat `PATCH /{id}/status` atau dihapus lewat
   `DELETE /{id}` — keduanya ditolak bila butirnya masih dipakai aturan berlaku.

**Aturan yang berlaku, dengan contohnya.**

| Aturan | Contoh |
| --- | --- |
| Kode butir wajib diisi | Kode kosong ditolak `400` |
| Nama butir wajib diisi | Nama kosong ditolak `400` |
| Kode butir unik | Sudah ada `METAL_IMPLANT`; menambah `METAL_IMPLANT` lagi ditolak `409` |
| Huruf besar-kecil tidak membedakan | Sudah ada `CONTRAST_ALLERGY`; menambah `contrast_allergy` **tetap** ditolak `409` |
| Butir yang masih dipakai tidak dapat hilang | "Skrining kehamilan" dipakai aturan CT yang berlaku; menonaktifkan atau menghapusnya ditolak `409` |
| Butir yang hanya dipakai draf **boleh** dipensiunkan | Draf belum menanyakan apa pun kepada pasien |
| Kode butir yang sudah dihapus dapat dipakai ulang | Butir `OLD_CODE` dihapus; menambah `OLD_CODE` baru diterima |

**Kelompok butir diturunkan dari isinya, bukan daftar tetap.** Kategori adalah teks bebas —
`Radiation`, `Contrast`, `Implant`, `Sedation`, dan seterusnya. Menuliskannya sebagai daftar
tetap di kode akan basi begitu rumah sakit memakai kelompok baru, sehingga
`GET /filters/metadata` menurunkannya dari data yang benar-benar ada.

**Hasil akhir.** `RAD-CAP-002` berpindah dari `Repair` menjadi lengkap di sisi backend.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan standar.** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`rules/backend/master-data-endpoint-standard.md`; `rules/backend/REPORT_TEMPLATE.md`;
`rules/rule-output/`.

**Kontrak.** `contracts/api-contract.md` grup *Rad Safety Requirement*;
`contracts/validation-matrix.md` bagian 3; `contracts/permission-audit-matrix.md` bagian 4;
`roadmap/backend-roadmap.md`; `roadmap/requirement-traceability.md`; laporan `BE-RAD-04.md`.

**Source pembanding.** `Models/MstRadSafetyRequirement.cs`;
`Repositories/Configurations/.../MstRadSafetyRequirementConfiguration.cs`;
`Services/RadModalityService.cs` dan `Controllers/RadModalityController.cs` sebagai pola sejajar
yang baru dibuat; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Controllers/RadSafetyRequirementController.cs` | **Baru.** Sembilan endpoint baseline master data |
| `Areas/.../Services/RadSafetyRequirementService.cs` | **Baru.** Metadata, rekap, daftar berhalaman, pilihan dropdown, rincian, tambah, ubah, ubah status, hapus |
| `Areas/.../DTOs/RadSafetyRequirementDtos.cs` | **Baru.** Request, response rincian, pilihan, metadata, rekap, penyaring daftar |
| `Areas/.../Services/RadOperationResult.cs` | Dua kode galat baru: `RAD_SAFETY_REQUIREMENT_CODE_ALREADY_USED` dan `RAD_SAFETY_REQUIREMENT_STILL_IN_USE` |
| `Program.cs` | Pendaftaran `RadSafetyRequirementService` |
| `Tests/.../RadiologyManagement/RadSafetyRequirementServiceTests.cs` | **Baru.** 17 uji tanpa database |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint yang direncanakan kini tersedia, ditambah empat baseline standar master data |
| Database | Tidak ada perubahan schema, entity, index, maupun migration. **Tidak ada perintah database yang dijalankan** |
| Keamanan/Auth | Setiap endpoint memuat `[AccessPermission("RadSafetyRequirement", ...)]` persis seperti `RAD-PERM-001` bagian 4 |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-requirements`

#### Health Services / Radiology Management / Master Data / Rad Safety Requirement

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring, termasuk kelompok butir yang benar-benar dipakai | `RadSafetyRequirement : Read` |
| `GET` | `/summary` | Rekap butir, termasuk yang dipakai aturan berlaku dan yang belum dipakai | `RadSafetyRequirement : Read` |
| `GET` | `/` | Daftar butir dengan pencarian, penyaringan, pengurutan, dan halaman | `RadSafetyRequirement : Read` |
| `GET` | `/options` | Pilihan ringan untuk form penyusunan aturan keselamatan | `RadSafetyRequirement : Read` |
| `GET` | `/{id}` | Rincian satu butir beserta jumlah aturan berlaku yang memakainya | `RadSafetyRequirement : Read` |
| `POST` | `/` | Menambah butir keselamatan | `RadSafetyRequirement : Create` |
| `PUT` | `/{id}` | Mengubah butir keselamatan | `RadSafetyRequirement : Update` |
| `PATCH` | `/{id}/status` | Menyalakan atau mematikan butir | `RadSafetyRequirement : Update` |
| `DELETE` | `/{id}` | Menandai butir terhapus tanpa menghapus fisik | `RadSafetyRequirement : Delete` |

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -m:1 -p:RunAnalyzers=false -p:EnforceCodeStyleInBuild=false` | Berhasil, **0 error**, 2 menit 17 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build` | **78 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| Butir baru belum mengikat siapa pun | `ActiveRuleCount` bernilai 0; kode tersimpan huruf besar | `PASS` | `ButirBaruDapatDitambahkanDanBelumMengikatSiapaPun` |
| Kode butir kembar | Ditolak `409` | `PASS` | `KodeButirKembar_Ditolak409` |
| Kode kembar berbeda huruf besar-kecil | Tetap ditolak | `PASS` | `KodeButirKembarBerbedaHurufBesarKecil_TetapDitolak` |
| Kode atau nama kosong | Ditolak `400` dengan pesan yang menyebut field-nya | `PASS` | `KodeAtauNamaKosong_Ditolak400` |
| Mengubah butir tanpa mengubah kodenya | Tidak dianggap kembar dengan dirinya sendiri | `PASS` | `MengubahButir_TidakMenganggapKodenyaSendiriKembar` |
| Butir tidak ditemukan | Ditolak `404` | `PASS` | `ButirYangTidakAda_Ditolak404` |
| **Menonaktifkan butir yang masih dipakai aturan berlaku** | Ditolak `409`; butir **tetap aktif** | `PASS` | `MenonaktifkanButirYangMasihDipakaiAturanBerlaku_Ditolak409` |
| **Menghapus butir yang masih dipakai aturan berlaku** | Ditolak `409`; baris **tidak** ditandai terhapus | `PASS` | `MenghapusButirYangMasihDipakaiAturanBerlaku_Ditolak409` |
| Butir dapat dipensiunkan setelah aturannya dihentikan | Berhasil | `PASS` | `ButirDapatDipensiunkanSetelahAturannyaDihentikan` |
| Butir yang hanya dipakai draf tetap dapat dipensiunkan | Berhasil — draf belum menanyakan apa pun | `PASS` | `ButirYangHanyaDipakaiDraf_TetapDapatDipensiunkan` |
| Butir yang sudah dihapus | Hilang dari daftar; kodenya dapat dipakai ulang | `PASS` | `ButirYangSudahDihapus_TidakMunculLagiDanKodenyaDapatDipakaiUlang` |
| Daftar menyaring butir yang sedang dipakai | `true` satu baris, `false` satu baris | `PASS` | `DaftarMenyaringButirYangSedangDipakai` |
| Daftar menyaring kelompok butir | Hanya kelompok yang diminta | `PASS` | `DaftarMenyaringKelompokButir` |
| Metadata menurunkan kelompok dari isinya | `Implant` dan `Radiation`, terurut | `PASS` | `MetadataMenurunkanKelompokDariIsinya` |
| Dropdown hanya memuat butir yang masih dipakai | Bawaannya satu; `onlyActive=false` menjadi dua | `PASS` | `PilihanDropdownHanyaMemuatButirYangMasihDipakai` |
| Rekap membedakan yang dipakai dari yang terlupakan | Total 3, dipakai 1, belum dipakai 2 | `PASS` | `RekapMembedakanButirYangDipakaiDariYangTerlupakan` |
| Permintaan halaman tidak masuk akal | Dinormalkan seperti task sebelumnya | `PASS` | `DaftarMenormalkanPermintaanHalamanYangTidakMasukAkal` |
| Regresi 61 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | Empat berkas uji radiologi sebelumnya |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT FEASIBLE` — layar `FE-RAD-03` belum dibuat.

### Batas verifikasi

Seluruh uji berjalan di atas penyedia **in-memory**. Yang belum terbukti: terjemahan SQL untuk
pencarian `Contains`, `Distinct` pada kategori, dan subquery pemakaian butir; serta index unik
`RequirementCode` sebagai penjaga terakhir.

**Tidak dijalankan:** migration — tidak ada perubahan schema; uji integrasi Postgres —
`QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kode butir kembar ditolak `409` | **Terpenuhi** | Dua uji, termasuk kembar berbeda huruf besar-kecil |
| Menonaktifkan butir yang masih dipakai ditolak `409` | **Terpenuhi** | Dua uji: penonaktifan dan penghapusan |
| Butir keselamatan dapat dikelola lewat endpoint | **Terpenuhi** | Sembilan endpoint tersedia |

Tidak ada butir Definition of Done `BE-RAD-05` yang belum terpenuhi.

---

## 7. Delta terhadap roadmap

Roadmap menyebut `RadSafetyRequirementController` — CRUD, tanpa menyebut jumlah endpoint.
Yang dibuat sembilan, mengikuti baseline wajib `master-data-endpoint-standard.md`. Akses datanya
ditempatkan di service sesuai `QBE-SVC-001`, sama seperti `BE-RAD-04`.

Kontrak juga menuliskan `DELETE /{id}` sebagai "menonaktifkan butir". Yang diimplementasikan
adalah **menandai terhapus**, dan penonaktifan diberi endpointnya sendiri
(`PATCH /{id}/status`) — keduanya perbuatan yang berbeda dan layak dibedakan. Kontrak
diperbarui mengikuti itu.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Penerjemahan query ke PostgreSQL belum diuji; string hak akses menunggu `BE-RAD-14` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | **Gelombang `MVP-0` selesai seluruhnya.** Berikutnya `BE-RAD-15` — pengisian data master awal sampai `GET /coverage` mengembalikan daftar kosong. Perlu diketahui: isi awalnya **belum ditetapkan klinis** (`DEC-RAD-005`), sehingga task itu menuntut keterlibatan penanggung jawab klinis, bukan hanya backend |

### Status Git pada akhir pekerjaan

```text
 M Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs
 M Program.cs
?? Areas/HealthServices/RadiologyManagement/Controllers/RadSafetyRequirementController.cs
?? Areas/HealthServices/RadiologyManagement/DTOs/RadSafetyRequirementDtos.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadSafetyRequirementService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadSafetyRequirementServiceTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-05.md
```

Ditambah berkas `BE-RAD-04` yang belum di-commit dan dokumen kontrak yang ikut diperbarui.
Empat berkas Laboratorium (`LabPatientRegistration*`) **bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan.
