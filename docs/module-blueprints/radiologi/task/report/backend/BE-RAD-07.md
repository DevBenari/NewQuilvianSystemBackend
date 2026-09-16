# Laporan Perubahan Backend — `BE-RAD-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-07` |
| Judul | Model hasil bacaan berversi |
| Slice | `S9` — Hasil bacaan radiolog |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 4, gelombang `MVP-2` |
| Trace | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003`; `RAD-ARCH-BE-001` bagian 4.9 dan 4.10; `RAD-ERD-REP-001`; `RAD-ERD-DICT-001` bagian 1, 2, dan 7 |
| Contract version | `RAD-ERD-REP-001` dan `RAD-ERD-DICT-001` — `approved` |
| Dependency | Registry `Rad` `ACTIVE` — terpenuhi |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 0, database **2**, keamanan 1, workflow 0; ditambah satu tingkat karena migration menyentuh schema |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Repositories/`, `Migrations/`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai untuk source dan migration.** Build lulus 0 error; 121 uji radiologi lulus, 15 di antaranya baru. Migration `AddRadReport` **dibuat, tidak dijalankan** — eksekusi database adalah wewenang terpisah yang belum diberikan |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE`. Entri riwayat registry 2026-09-10 menyebut `RadReport` dan `RadReportVersion` secara **eksplisit** sebagai entity yang penghalang `QBE-MOD-002`-nya sudah dicabut |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-ENT-001` entity mewarisi `IdentityModel`; `QBE-ENT-002` Guid, field, navigasi, dan nullability mengikuti semantik domain; `QBE-NAM-002` memakai prefix `Rad` yang disetujui registry; `QBE-CFG-001` menyediakan `IEntityTypeConfiguration` beserta mapping, key, index, dan relasi; `QBE-MOD-001`; `QBE-ENUM-001` enum dimiliki modul |
| QBE ID yang **tidak** berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001` — service dan endpoint milik `BE-RAD-08` dan `BE-RAD-09`; `QBE-CODE-*` — penomoran bacaan belum dialokasikan task ini |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Hasil bacaan radiolog **tidak punya tempat penyimpanan sama sekali**. `RAD-CAP-010` menandainya
`Missing`: citra dapat diambil dan dinyatakan layak, tetapi tidak ada satu pun tabel untuk
menuliskan apa yang dilihat radiolog di dalamnya.

Yang lebih menentukan daripada sekadar "ada tabelnya" adalah **bentuknya**. Bacaan radiologi
dapat dikoreksi berbulan-bulan setelah dirilis, dan koreksi itu tidak boleh menghapus apa yang
sudah dibaca orang.

> **Contoh nyata.** Tn. B menjalani CT kepala pada Senin. Bacaannya dirilis: tidak ada
> perdarahan. Dokter jaga memulangkan pasien atas dasar itu. Kamis, radiolog lain meninjau
> ulang dan menemukan perdarahan kecil yang terlewat, lalu mengoreksi bacaannya.
>
> Kalau koreksi itu menimpa isi yang lama, pertanyaan **"apa yang dibaca dokter jaga hari
> Senin"** kehilangan jawabannya — padahal itulah pertanyaan pertama yang diajukan ketika
> keputusan memulangkan pasien ditinjau.

Task ini menyiapkan bentuk yang membuat kedua jawaban tetap ada.

---

## 2. Proses bisnis

**Tujuan.** Menyediakan tempat penyimpanan hasil bacaan yang riwayatnya tidak dapat hilang.

**Dua tabel, dengan pembagian yang disengaja.**

| Tabel | Isinya | Mengapa dipisah |
| --- | --- | --- |
| `RadReport` | Identitas dan status bacaan atas satu study: nomor bacaan, status, nomor versi yang berlaku, kapan pertama dan terakhir dirilis | Ini yang dicari dan disaring. Isinya tidak pernah berubah oleh koreksi |
| `RadReportVersion` | Isi bacaan per versi: temuan, kesimpulan, saran, penulis, pengesah, alasan koreksi | Setiap koreksi menambah baris baru, **tidak pernah menimpa** baris lama |

**Alur yang akan berjalan di atasnya** — dikerjakan `BE-RAD-08` dan seterusnya:

1. Study dinyatakan mutunya diterima. Bacaan lahir berstatus `Pending`, belum punya versi.
2. Radiolog, residen, radiografer, atau bantuan AI menulis draf. Lahir versi `1`, dan **peran
   penulisnya dibekukan** pada baris itu.
3. Dokter radiolog mengesahkan, lalu merilis. Versi `1` menjadi `Released` dan isinya beku.
4. Koreksi membuat versi `2` yang menunjuk versi `1` sebagai pendahulunya. Versi `1` berpindah
   menjadi `Superseded` — **isinya tetap utuh**.
5. Koreksi atas koreksi membuat versi `3`, dan seterusnya. Tidak ada status terminal.

**Tiga aturan yang dijaga database, bukan hanya kode.**

| Aturan | Penjaganya | Bila tidak dijaga |
| --- | --- | --- |
| Satu study paling banyak satu bacaan | Index unik pada `RadStudyId`, difilter `IsDelete = false` | Dua permintaan bersamaan menghasilkan dua bacaan atas satu study, dan dokter pengirim melihat dua kesimpulan berbeda |
| Nomor versi tidak kembar dalam satu bacaan | Index unik pada `RadReportId` + `VersionNumber` | Dua amandemen bersamaan sama-sama menjadi "versi 2"; riwayatnya bercabang tanpa ada yang tahu mana yang berlaku |
| Nomor bacaan tidak kembar | Index unik pada `ReportNumber`, difilter | Dua bacaan berbeda dirujuk dengan nomor yang sama pada surat dan rekam medis |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Enums/RadiologyEnums.cs` | Tiga enum baru: `RadReportStatus`, `RadReportVersionStatus`, `RadReportAuthorRole` |
| `Areas/.../Models/RadReport.cs` | **Baru.** Wadah identitas dan status |
| `Areas/.../Models/RadReportVersion.cs` | **Baru.** Isi bacaan per versi |
| `Repositories/Configurations/.../RadReportConfiguration.cs` | **Baru.** Mapping, index, dan relasi |
| `Repositories/Configurations/.../RadReportVersionConfiguration.cs` | **Baru.** Mapping, index unik versi, dan rantai koreksi |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` baru |
| `Migrations/20260911025734_AddRadReport.cs` beserta `.Designer.cs` | **Baru.** Dibuat, **belum dijalankan** |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bertambah dua entity |
| `Tests/.../RadiologyManagement/RadReportModelContractTests.cs` | **Baru.** 15 uji atas bentuk model |

### 3.2 Tiga bentuk yang ditetapkan ERD, dan mengapa

**Isi bacaan tidak disimpan di `RadReport`.** Kalau disimpan di sana, setiap koreksi menimpa isi
sebelumnya. Riwayat klinis yang tertimpa tidak dapat dikembalikan, dan pertanyaan "apa yang
dibaca dokter itu waktu itu" kehilangan jawabannya.

**Tidak ada kunci asing dari induk ke versi yang berlaku.** `RAD-ERD-REP-001` menyebut ini
eksplisit sebagai jebakan, dan roadmap mengulangnya sebagai risiko: relasi semacam itu
melingkar — induk menunjuk versi, versi menunjuk induk — sehingga penyisipan versi pertama
mustahil dilakukan dalam satu transaksi tanpa kolom sementara yang kosong. Yang disimpan hanya
`CurrentVersionNumber`, sebuah angka.

**`AuthorRoleSnapshot` dibekukan, bukan dibaca ulang.** Seorang residen yang kemudian menjadi
dokter radiolog tetap tidak boleh mengesahkan draf yang ia tulis semasa menjadi residen. Yang
dinilai adalah keadaan saat bacaan itu disusun, bukan jabatan hari ini — `RAD-DEC-003`.
Penegakannya milik `BE-RAD-08`; kolomnya disiapkan di sini.

**`EncounterId` sengaja tanpa kunci asing.** Pemilik kunjungan adalah Registration Management;
kolom ini salinan untuk pencarian tanpa penggabungan tabel, sebagaimana `RAD-ERD-DICT-001`
bagian 1.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint. Grup *Rad Report* pada `RAD-API-001` tetap berlabel rencana sampai `BE-RAD-09` |
| Database | **Dua tabel baru.** Migration `20260911025734_AddRadReport` dibuat dan diperiksa, **tidak dijalankan ke database mana pun**. Tidak ada tabel lain yang tersentuh |
| Keamanan/Auth | `NOT APPLICABLE` untuk authorization. Empat kolom ditandai **sensitif** dan haram masuk application log: `Findings`, `Impression`, `Recommendation`, `AmendmentReason` — `RAD-PERM-001` bagian 7. Penegakannya milik `BE-RAD-08` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah endpoint.

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, **0 error**, 4 menit 28 detik | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddRadReport` | Berhasil, `Done.` | `PASS` | `Migrations/20260911025734_AddRadReport.cs` |
| Isi `Up()` | Dua `CreateTable`; delapan `CreateIndex`, tiga di antaranya unik dan dua difilter `"IsDelete" = false` | `PASS` | Pembacaan langsung berkas migration |
| Cakupan migration | **Hanya** tabel `RadReport` dan `RadReportVersion` yang disentuh | `PASS` | Penyaringan seluruh nama tabel pada migration |
| Isi `Down()` | Kedua tabel di-drop dengan urutan yang benar — versi lebih dulu, baru induknya | `PASS` | Pembacaan langsung |
| Kesehatan snapshot | Entity unik bertambah **tepat dua**; **tidak ada** entity yang hilang | `PASS` | Perbandingan `comm` antara `HEAD` dan working copy |
| `dotnet build` project uji | Berhasil, 0 error, 3 menit 53 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build` | **121 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| Satu study paling banyak satu bacaan | Index `RadStudyId` unik dan difilter `"IsDelete" = false` | `PASS` | `SatuStudyPalingBanyakSatuBacaan_DijagaIndexUnik` |
| Nomor bacaan tidak kembar | Index `ReportNumber` unik dan difilter | `PASS` | `NomorBacaanTidakBolehKembar` |
| Nomor versi tidak kembar dalam satu bacaan | Index `RadReportId` + `VersionNumber` unik | `PASS` | `NomorVersiTidakBolehKembarDalamSatuBacaan` |
| Rantai koreksi dapat ditelusuri | Index pada `PreviousVersionId`, `EncounterId`, dan `ReportStatus` ada | `PASS` | `RantaiKoreksiDapatDitelusuri` |
| **Tidak ada kunci asing dari induk ke versi berlaku** | Nol kunci asing dari `RadReport` ke `RadReportVersion`; `CurrentVersionNumber` ada | `PASS` | `TidakAdaKunciAsingDariIndukKeVersiBerlaku` |
| `EncounterId` tidak punya kunci asing | Nol | `PASS` | `EncounterIdTidakPunyaKunciAsing` |
| Seluruh relasi memakai `Restrict` | Seluruh kunci asing pada kedua tabel | `PASS` | `SeluruhRelasiMemakaiRestrict` |
| Kesimpulan wajib, temuan dan saran boleh kosong | Sesuai kamus data | `PASS` | `KesimpulanWajibDiisi_TemuanDanSaranBoleh` |
| Panjang kolom isi sesuai kamus data | 8000, 4000, 2000, 1000 | `PASS` | `PanjangKolomIsiSesuaiKamusData`, 4 kasus |
| Nomor bacaan panjangnya 64 dan wajib | Sesuai kamus data | `PASS` | `NomorBacaanPanjangnyaSesuaiKamusData` |
| Seluruh enum disimpan sebagai angka | Ketiganya `int` | `PASS` | `SeluruhEnumDisimpanSebagaiAngka` |
| Kedua tabel terpetakan ke schema `public` | Sesuai pola modul | `PASS` | `KeduaTabelTerpetakanKeSchemaPublic` |
| Regresi 106 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | Tujuh berkas uji radiologi sebelumnya |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT APPLICABLE` — tidak ada endpoint maupun layar.

### Catatan tentang cara migration dibuat

`--no-build` **sengaja tidak dipakai**. Laporan `BE-RAD-01` mencatat flag itu menghasilkan
migration kosong sekaligus meregenerasi snapshot dari assembly lama. Build dijalankan penuh
lebih dulu, lalu `dotnet ef` membangun ulang sendiri.

### Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Index unik benar-benar menolak baris kembar | Penyedia in-memory tidak menegakkan index. Yang terbukti adalah **deklarasinya** pada model — bahwa aturannya ada dan tidak hilang saat configuration disunting kemudian |
| Migration dapat dijalankan dan dimundurkan | Menjalankannya menuntut database. `Down()` ada, terbaca benar, dan terkompilasi, tetapi **belum pernah dijalankan** |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef database update` ke database mana pun | **Wewenang terpisah dan belum diberikan.** `AGENTS.md`: perubahan model tidak dengan sendirinya memberi wewenang menjalankan migration |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-18 — riwayat versi utuh dan dapat ditelusuri | **Terpenuhi untuk bentuknya** | Rantai `PreviousVersionId` beserta index-nya ada; penegakan perilakunya milik `BE-RAD-10` |
| Index unik satu bacaan per study berlaku | **Terpenuhi pada deklarasi** | Uji model membuktikan index unik dan filternya; penegakan sesungguhnya menunggu migration dijalankan |
| Migration dapat dimundurkan | **Belum terbukti** | `Down()` ada dan benar, tetapi belum pernah dijalankan — sama seperti `BE-RAD-01` |
| Kedua tabel terbentuk sesuai DDL pada kamus data | **Terpenuhi pada model dan migration** | Sembilan uji kolom dan index; migration diperiksa baris demi baris |
| Tabel lain tidak tersentuh | **Terpenuhi** | Migration hanya memuat `RadReport` dan `RadReportVersion`; snapshot tidak kehilangan satu entity pun |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | Penomoran bacaan (`ReportNumber`) belum punya pengalokasi. Kolom dan index uniknya sudah ada; cara membangkitkannya menjadi bagian `BE-RAD-08`, dan wajib mengikuti `QBE-CODE-003` — **bukan** `Count+1` |
| Risiko tersisa | **Pertama**, migration belum dijalankan ke satu database pun, termasuk `devYoga`. Selama itu, tabelnya belum ada dan `BE-RAD-08` tidak dapat diuji terhadap database sungguhan. **Kedua**, index unik baru terbukti pada deklarasi, bukan pada penegakannya. **Ketiga**, migration ini menambah satu berkas Designer berukuran ±4 MB — lihat catatan di bawah |
| Temuan yang berkaitan dengan investigasi RAM | Migration ini menyalin ulang seluruh model 1.309 entity ke dalam satu berkas Designer baru. Folder `Migrations` yang sudah 400 MB dan 9,9 juta baris bertambah lagi. Setiap migration berikutnya akan melakukan hal yang sama. Ini memperkuat usulan **squash migration** yang dilaporkan sebelumnya |
| Perubahan sampingan | `NONE`. Snapshot berubah karena memang harus, dan perubahannya diperiksa |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-08` — service penulisan dan pengesahan bacaan. **Inti keselamatan modul**: pemeriksaan `ActAsRadiologist` dan penjagaan pengesahan sendiri wajib di service, bukan hanya di atribut endpoint. Sebelum itu, pemilik modul perlu memutuskan kapan migration `AddRadReport` dijalankan |

### Status Git pada akhir pekerjaan

```text
 M Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
?? Areas/HealthServices/RadiologyManagement/Models/RadReport.cs
?? Areas/HealthServices/RadiologyManagement/Models/RadReportVersion.cs
?? Migrations/20260911025734_AddRadReport.Designer.cs
?? Migrations/20260911025734_AddRadReport.cs
?? Repositories/Configurations/HealthServices/RadiologyManagement/RadReportConfiguration.cs
?? Repositories/Configurations/HealthServices/RadiologyManagement/RadReportVersionConfiguration.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadReportModelContractTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-07.md
```

Ditambah berkas `BE-RAD-04`, `BE-RAD-05`, `BE-RAD-14`, dan `BE-RAD-15` yang belum di-commit,
beserta dokumen kontrak dan roadmap yang ikut diperbarui. Empat berkas Laboratorium
(`LabPatientRegistration*`) **bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
