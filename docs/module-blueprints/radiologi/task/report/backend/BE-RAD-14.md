# Laporan Perubahan Backend — `BE-RAD-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-14` |
| Judul | Uji kontrak hak akses radiologi |
| Slice | Lintas — seluruh slice |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 6 |
| Trace | `RAD-DEC-003`, `RAD-DEC-005`, `RAD-DEC-015`; `RAD-PERM-001` bagian 9; menutup `RAD-CAP-025` |
| Contract version | `RAD-PERM-001` **revision 5** `approved` |
| Dependency | Seluruh controller radiologi — terpenuhi. Dijalankan ulang setiap kali endpoint bertambah |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 0, logika bisnis 1, kontrak API 1, database 0, keamanan 2, workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** Build lulus 0 error; 106 uji radiologi lulus, 18 di antaranya baru. **Tidak ditemukan satu pun cacat hak akses** pada kelima controller radiologi |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Keberlakuan | `NEW CODE` — berkas uji baru; **tidak ada source aplikasi yang diubah** |
| QBE ID yang berlaku | `QBE-PERM-001` memakai metadata Access yang berlaku — dibuktikan, bukan diubah |
| QBE ID yang **tidak** berlaku | Seluruh aturan entity, configuration, penamaan, API, dan database |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Radiologi tidak punya uji kontrak hak akses, sementara dua modul sebanding punya —
Laboratorium lewat `LaboratoryAuthorityTests`, Bank Darah lewat
`BloodBankRoleAccessContractTests`. Itulah isi `RAD-CAP-025`, dan `DEC-RAD-006` mengusulkan
mewajibkannya sebelum Rilis 1.

**Yang membuat ketiadaannya berbahaya bukan teorinya, melainkan kejadian nyata pada modul
lain.** Sembilan endpoint Rawat Inap pernah memeriksa pasangan hak akses yang tidak pernah
didaftarkan `AccessMenuSeeder`. Akibatnya bukan galat yang terlihat:

> `403` permanen yang **tidak dapat diperbaiki dari layar mana pun** — baris untuk dicentang
> admin memang tidak pernah ada.

Cacat itu lolos berbulan-bulan karena buktinya diambil lewat Swagger sebagai SuperAdmin, dan
`AccessPermissionService.HasAccessAsync` memulangkan `true` untuk SuperAdmin **sebelum satu
baris hak akses pun dibaca**.

Radiologi kini punya dua puluh enam endpoint di lima controller, termasuk yang menyangga
pemisahan wewenang paling penting di modul ini: siapa boleh menyusun aturan keselamatan, dan
siapa boleh mengesahkannya.

---

## 2. Proses bisnis

**Tujuan.** Membuat pergeseran hak akses menjadi kegagalan build, bukan temuan lapangan.

**Yang dijaga.** Pemisahan antara admin Radiologi yang menyusun aturan keselamatan dan
penanggung jawab klinis yang mengesahkannya. Pemisahan itu dapat bergeser diam-diam lewat tiga
jalan, dan **tidak satu pun menghasilkan galat yang terlihat**:

| Jalan bergesernya | Akibatnya bagi pengguna |
| --- | --- |
| Endpoint lupa diberi `[AccessPermission]` | Siapa pun yang berhasil login dapat memanggilnya |
| Endpoint memeriksa pasangan yang tidak terdaftar seeder | `403` permanen bagi semua peran, tidak dapat diperbaiki dari layar |
| Aksi disembunyikan dari layar Akses Role | Tidak dapat diberikan kepada siapa pun — sama saja endpointnya mati |

**Kapan dijalankan.** Setiap kali endpoint radiologi bertambah atau berganti nama. Uji ini
tidak menyentuh database maupun jaringan, sehingga murah dijalankan bersama uji lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/.../RadiologyManagement/RadiologyRoleAccessContractTests.cs` | **Baru.** 18 uji atas lima controller radiologi |

**Tidak ada source aplikasi yang diubah.** Itu hasil yang diharapkan: uji ini membuktikan
keadaan yang sudah benar, bukan memperbaiki yang salah.

### 3.2 Apa yang dibuktikan, butir demi butir

| Butir `RAD-PERM-001` bagian 9 | Cara membuktikannya |
| --- | --- |
| 1. Setiap endpoint memuat `[AccessPermission]` dengan string persis seperti tabel | Setiap pasangan yang diperiksa dibandingkan dengan pasangan yang **akan** dibuat `AccessMenuSeeder`, dihitung dengan aturan yang sama: `ControllerName` dari `[AccessController]` dipasangkan dengan argumen pertama `[AccessAction]` |
| 2. Tidak ada endpoint radiologi tanpa atribut hak akses | Refleksi atas seluruh method ber-`[HttpGet/Post/Put/Patch/Delete]` pada kelima controller |
| 3. Pemisahan wewenang benar-benar ditegakkan service | `RadSafetyPolicyService` dipanggil **langsung**, tanpa melewati satu pun atribut |
| 4. Tidak ada peran memegang `Deactivate` tanpa `Approve` — bagian 5.3 | Pemeriksa yang membaca susunan peran dari `SysAccessPolicy`; diuji dengan tiga susunan peran |

### 3.3 Keputusan rancangan

**Uji ditempatkan di project in-memory, bukan Postgres.** Preseden Laboratorium tinggal di
project integrasi Postgres yang **tidak dapat dijalankan** di lingkungan ini —
`QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi. Uji kontrak yang tidak pernah dijalankan tidak
menjaga apa pun. Seluruh pemeriksaannya memang tidak membutuhkan database sungguhan: tiga butir
pertama murni refleksi, dan butir keempat cukup memakai penyedia in-memory.

**Pemeriksa disiplin `Deactivate` ditulis sebagai fungsi publik yang menerima satu
`ApplicationDbContext`.** Dengan begitu ia dapat dijalankan juga terhadap database sungguhan
ketika susunan peran hendak diperiksa — bukan hanya di dalam uji. Yang diuji di sini adalah
pemeriksanya, dengan tiga susunan peran: melanggar, patuh, dan setengah-patuh.

**Ditambahkan uji untuk arah sebaliknya.** Selain membuktikan penyusun **tidak** dapat
mengesahkan, ada uji yang membuktikan orang kedua **dapat**. Tanpa itu, sebuah bug yang menolak
siapa pun akan terbaca sebagai "pengaman bekerja" — dan modul berhenti tanpa ada yang tahu
sebabnya.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint ditambah atau diubah |
| Database | `NOT APPLICABLE` — tidak ada schema, migration, maupun perintah database |
| Keamanan/Auth | **Inti task ini.** Tidak ada atribut yang diubah; yang bertambah adalah pembuktiannya |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah endpoint.

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -m:1 -p:RunAnalyzers=false` | Berhasil, **0 error**, 4 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build` | **106 lulus, 0 gagal**, 11 detik | `PASS` | Keluaran perintah |
| **Setiap endpoint radiologi punya `[AccessPermission]` dan `[AccessAction]`** | Tidak ada satu pun yang kurang, atas 5 controller | `PASS` | `SetiapEndpointRadiologi_PunyaAtributHakAkses` |
| **Setiap pasangan ada sebagai baris yang dapat dicentang** | Tidak ada satu pun pasangan yang tidak terdaftar | `PASS` | `SetiapPasanganHakAkses_AdaSebagaiBarisYangDapatDicentang` |
| Sembilan pasangan penyangga pemisahan wewenang tidak hilang | Seluruhnya ditemukan | `PASS` | `PasanganYangMenyanggaPemisahanWewenang_TidakBolehHilang`, 9 kasus |
| Setiap aksi muncul dan dapat diberikan di layar Akses Role | `AccessType` sah; tidak ada yang `IsSystemOnly` atau disembunyikan | `PASS` | `SetiapAksiRadiologi_MunculDanDapatDiberikanDiLayarAksesRole` |
| Kelima controller terdaftar pada modul yang sama | `HEALTH_SERVICE_RADIOLOGY_MANAGEMENT`, area `HealthServices` | `PASS` | `SetiapControllerRadiologi_TerdaftarPadaModulYangSama` |
| **Penyusun tidak dapat mengesahkan walau hak aksesnya lengkap** | Ditolak `Forbidden`; aturannya **tidak** menjadi `Active` | `PASS` | `PenyusunTidakDapatMengesahkan_WalauSeluruhHakAksesnyaLengkap` |
| **Orang kedua dapat mengesahkan** | Berhasil, status menjadi `Active` | `PASS` | `OrangKeduaDapatMengesahkan_SehinggaPemisahannyaBukanJalanBuntu` |
| Peran memegang `Deactivate` tanpa `Approve` terdeteksi | Satu pelanggaran dilaporkan beserta sebabnya | `PASS` | `PeranYangMemegangDeactivateTanpaApprove_Terdeteksi` |
| Peran memegang keduanya tidak dianggap melanggar | Kosong | `PASS` | `PeranYangMemegangKeduanya_TidakDianggapMelanggar` |
| Peran hanya memegang `Approve` tidak dianggap melanggar | Kosong — yang dilarang hanya arah sebaliknya | `PASS` | `PeranYangHanyaMemegangApprove_TidakDianggapMelanggar` |
| Regresi 88 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | Enam berkas uji radiologi sebelumnya |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT APPLICABLE` — seluruh pemeriksaan bersifat otomatis.

### Hasil yang perlu disebut terpisah

**Tidak ditemukan satu pun cacat.** Kelima controller radiologi — 26 endpoint — seluruhnya
memuat atribut yang benar, dan setiap pasangan hak akses yang diperiksa benar-benar
didaftarkan `AccessMenuSeeder`. Cacat `403` permanen yang menimpa sembilan endpoint Rawat Inap
**tidak terjadi** di Radiologi.

Itu bukan kebetulan: seluruh controller radiologi ditulis dengan `[AccessController]`,
`[AccessAction]`, dan `[AccessPermission]` yang konsisten sejak awal, dan `RAD-PERM-001`
menyediakan tabel string yang tinggal disalin. Uji ini memastikan keadaan itu **tetap** benar
ketika endpoint berikutnya ditambahkan.

### Batas verifikasi

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Susunan peran pada database sungguhan patuh bagian 5.3 | Pemeriksanya sudah ada dan teruji, tetapi datanya hanya ada di database yang berjalan. Perlu dijalankan pemilik modul terhadap lingkungan yang sebenarnya |
| Jalur otorisasi penuh dengan pengguna bukan SuperAdmin | Preseden Rawat Inap melakukannya dengan membangun service provider ber-Identity. Tidak dikerjakan di sini karena ketiga butir pertama sudah menutup kelas cacat yang sama tanpa perancah seberat itu; dicatat sebagai perluasan yang mungkin |

**Tidak dijalankan:** migration, perintah database, dan uji integrasi Postgres — tidak satu pun
dibutuhkan task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Butir 1 — string `[AccessPermission]` persis seperti tabel | **Terpenuhi** | `SetiapPasanganHakAkses_AdaSebagaiBarisYangDapatDicentang` |
| Butir 2 — tidak ada endpoint tanpa atribut | **Terpenuhi** | `SetiapEndpointRadiologi_PunyaAtributHakAkses` |
| Butir 3 — pemisahan wewenang ditegakkan service | **Terpenuhi** | Dua uji yang memanggil service langsung |
| Butir 4 — tidak ada peran `Deactivate` tanpa `Approve` | **Terpenuhi untuk pemeriksanya** | Tiga uji; penerapannya pada database sungguhan menunggu pemilik modul |
| Definition of Done — uji lulus dan dijalankan pada setiap gelombang | **Terpenuhi** | 106 uji radiologi lulus; uji ini ikut berjalan pada setiap `dotnet test` modul |

`RAD-CAP-025` — "Uji kontrak hak akses" — berpindah dari `Missing` menjadi **ada**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | **Pertama**, uji ini menjaga *bentuk* hak akses, bukan *pemberiannya*. Peran yang diberi terlalu banyak tetap lolos — yang dijaga hanya bahwa pemberiannya mungkin dan tercatat. **Kedua**, disiplin bagian 5.3 baru terjaga pada pemeriksanya; susunan peran sungguhan perlu diperiksa pemilik modul |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | **Seluruh task backend radiologi pada roadmap kini selesai**, kecuali dua hal yang bukan pekerjaan backend: penutupan `DEC-RAD-005` oleh tata kelola klinis, dan seluruh task frontend `FE-RAD-01` s/d `FE-RAD-13`. Gelombang `MVP-2` dan berikutnya — hasil bacaan radiolog `BE-RAD-07` s/d `BE-RAD-13` — belum dikerjakan dan menunggu keputusan urutan dari pemilik modul |

### Status Git pada akhir pekerjaan

```text
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadiologyRoleAccessContractTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-14.md
```

Ditambah berkas `BE-RAD-04`, `BE-RAD-05`, dan `BE-RAD-15` yang belum di-commit, beserta dokumen
kontrak dan roadmap yang ikut diperbarui. Empat berkas Laboratorium (`LabPatientRegistration*`)
**bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan.
