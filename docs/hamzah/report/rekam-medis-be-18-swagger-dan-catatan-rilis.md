# Rekam Medis — `BE-18` Swagger dan catatan rilis

| | |
|---|---|
| Tanggal | 2026-08-27 |
| Task ID | `BE-18` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `yoga` (repository backend, tidak ada operasi Git write) |
| Trace | api-contract bagian 8; manifest bagian dampak kompatibilitas |
| Migration | **Tidak ada** |
| Endpoint baru | **Tidak ada** |
| Bukti | `dotnet test` → `Failed: 0, Passed: 132`. 3 uji baru, seluruhnya lulus |
| Breaking change | **Tidak** — hanya keterangan dan pengaturan build |

---

## 1. Masalah yang diselesaikan

Tiga perubahan perilaku pada endpoint yang sudah berjalan **tidak terlihat** dari bentuk
permintaan maupun responsnya. Dua di antaranya paling menjebak:

> Klien yang mengirim `ProviderUserId` atau `IsReadOnlyGenerated` pada permintaan ubah CPPT
> **tidak menerima galat** — tetapi nilainya diabaikan.

Mengabaikan alih-alih menolak adalah pilihan sadar: menolak akan memutus frontend yang sedang
berjalan. Tetapi **mengabaikan kiriman klien tanpa pemberitahuan juga bukan praktik yang baik**.
Task ini yang menutupnya.

## 2. Temuan yang mengubah bentuk pekerjaan

Acceptance criteria nomor 1 berbunyi "Swagger menyebut...". Saat memeriksa cara memenuhinya,
ditemukan hal yang menentukan:

**Swagger pada aplikasi ini tidak membaca komentar keterangan sama sekali.**

| Yang diperiksa | Keadaan sebelum task ini |
|---|---|
| `GenerateDocumentationFile` pada `.csproj` | **Tidak ada** |
| `IncludeXmlComments` pada `AddSwaggerGen` | **Tidak ada** |
| Paket `Swashbuckle.AspNetCore.Annotations` | **Tidak dirujuk** — metapackage `Swashbuckle.AspNetCore` tidak memuatnya |

Artinya seluruh komentar keterangan yang ditulis sepanjang `BE-01` sampai `BE-17` **tidak pernah
tampil** di halaman Swagger.

**Konsekuensinya bagi task ini:** menuliskan keterangan saja tidak akan pernah memenuhi
acceptance criteria nomor 1, sebanyak apa pun komentar ditambahkan. Penyalaan dokumentasi XML
karena itu menjadi bagian task ini, bukan pekerjaan terpisah.

### Kenapa dokumentasi XML, bukan cara lain

| Pilihan | Penilaian |
|---|---|
| **Mengaktifkan dokumentasi XML** | **Dipilih.** Mekanisme baku, dan langsung memunculkan seluruh keterangan yang sudah ditulis di modul ini maupun modul lain |
| Menambah paket `Swashbuckle.AspNetCore.Annotations` | Menambah dependency baru hanya untuk satu endpoint |
| Menulis `IOperationFilter` sendiri | Mesin buatan sendiri padahal mekanisme bakunya ada, dan tidak memunculkan keterangan lain |
| Atribut `[EndpointDescription]` | Tidak dibaca Swashbuckle 6.5.0 yang dipakai di sini |

### Peringatan `CS1591` disenyapkan dengan sadar

Menyalakan `GenerateDocumentationFile` menyalakan pula peringatan `CS1591`, yang menuntut
komentar XML pada **setiap** anggota publik di seluruh aplikasi. Pada basis kode sebesar ini,
peringatan itu akan berjumlah ribuan dan menenggelamkan peringatan sungguhan — tanpa membuat
dokumentasinya lebih baik.

`<NoWarn>$(NoWarn);1591</NoWarn>` ditambahkan beserta alasannya di dalam `.csproj`. Terbukti pada
hasil build: **0 peringatan `CS1591`**.

## 3. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `QuilvianSystemBackend.csproj` | Diperbarui | `GenerateDocumentationFile` + `NoWarn` 1591 |
| `Program.cs` | Diperbarui | `IncludeXmlComments` pada `AddSwaggerGen`, dengan `includeControllerXmlComments: false` — lihat bagian 6 |
| `Areas/.../ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs` | Diperbarui | Keterangan pada `ProviderUserId` dan `IsReadOnlyGenerated` |
| `Areas/.../ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Diperbarui | Keterangan pada endpoint ubah CPPT |
| `docs/module-blueprints/rekam-medis/catatan-rilis.md` | Baru | Catatan rilis modul |
| `tests/.../SwaggerDocumentationTests.cs` | Baru | 3 uji |

Tidak ada perubahan logika. Seluruh perubahan pada source berupa keterangan dan pengaturan build.

## 4. Acceptance criteria

### 1) Swagger menyebut dua kolom yang diabaikan — **terpenuhi**

Keterangan dipasang pada dua tempat sekaligus: pada kolomnya sendiri, dan pada keterangan
endpoint ubah CPPT. Keduanya menyatakan dua hal yang sama pentingnya — bahwa nilainya diabaikan,
**dan** bahwa permintaannya tidak ditolak. Menyebut "diabaikan" tanpa menyebut "tidak ditolak"
justru mudah disalahpahami sebagai galat yang tidak muncul.

### 2) Catatan rilis memuat empat perubahan perilaku — **terpenuhi**

`catatan-rilis.md` memuat keempatnya beserta apa yang harus dilakukan klien:

| No | Perubahan |
|---:|---|
| 1 | `PUT` CPPT menolak catatan yang sudah terkunci |
| 2 | `ProviderUserId` diabaikan pada permintaan ubah |
| 3 | `IsReadOnlyGenerated` diabaikan pada permintaan ubah |
| 4 | `PATCH` status kunjungan mengunci dokumen terbuka, sehingga penutupan kunjungan dapat gagal |

Catatan rilis juga memuat cakupan aturan keutuhan, endpoint baru beserta kode statusnya,
perubahan bentuk balasan `/timeline`, hak akses baru beserta dua yang menuntut kehati-hatian
khusus, dan daftar hal yang wajib disiapkan sebelum modul dipakai.

### 3) Keterangan bahwa baru CPPT yang tunduk aturan keutuhan — **terpenuhi**

Dinyatakan pada keterangan endpoint ubah CPPT dan pada bagian 2 catatan rilis, lengkap dengan
akibatnya bagi dua belas jenis dokumen lain: aturan penguncian belum berlaku, dan dokumennya
masih dapat diubah bebas.

## 5. Verifikasi

```powershell
dotnet test tests\QuilvianSystemBackend.Tests\QuilvianSystemBackend.Tests.csproj
```

| Hasil | Angka |
|---|---|
| Kompilasi | **0 error**, **0 peringatan `CS1591`** |
| Uji seluruh suite | **Failed: 0, Passed: 132, Skipped: 0** — naik dari 129 |
| Uji `BE-18` | 3 uji, seluruhnya lulus |
| Durasi | 2 menit 8 detik |

### Kenapa diuji otomatis padahal verifikasinya manual

Roadmap menyebut verifikasi `BE-18` berupa pemeriksaan manual halaman Swagger. Masalahnya,
**pemeriksaan manual hanya berlaku pada hari ia dilakukan** — keterangannya dapat terhapus pada
perubahan berikutnya tanpa ada yang menyadari.

Uji memeriksa berkas dokumentasi XML hasil build, yaitu sumber yang dipakai Swagger untuk
menampilkan keterangan. Bila keterangannya hilang, atau `GenerateDocumentationFile` dimatikan
seseorang, uji gagal beserta pesan yang menjelaskan sebabnya.

Uji ini sekaligus membuktikan bahwa berkas dokumentasinya **benar-benar dihasilkan** — hal yang
tidak dapat dipastikan hanya dengan membaca `.csproj`.

## 6. Koreksi setelah halaman Swagger dibuka

Laporan ini semula mencatat "tampilan halaman Swagger" sebagai hal yang belum diverifikasi dan
perlu dilihat sekali dengan mata. Pemeriksaan itu dilakukan pada hari yang sama, dan **menemukan
cacat**.

### Cacat: dua belas judul grup kosong

`IncludeXmlComments` semula dipanggil dengan `includeControllerXmlComments: true`. Parameter itu
membuat Swashbuckle menambahkan satu tag tingkat dokumen untuk **tiap controller**, dinamai
menurut **nama kelas controller**, dengan `<summary>` kelas sebagai deskripsinya.

Project ini tidak mengelompokkan endpoint memakai nama controller. Ia memakai atribut
`[Tags(...)]` yang isinya kalimat panjang, misalnya
`Health Services / Medical Record Management / Medical Record Access Log`.

Akibatnya kedua nama itu tidak pernah bertemu. Terbukti dari `swagger.json` aplikasi yang sedang
berjalan:

| Yang diperiksa | Angka |
|---|---:|
| Tag tingkat dokumen bernama menurut kelas controller | 12 |
| Di antaranya yang dipakai endpoint mana pun | **0** |

Dua belas tag yatim itu tetap dirender Swagger UI karena dideklarasikan di dokumen, sehingga
tampil sebagai judul grup besar berisi deskripsi panjang tetapi **kosong isinya**.

### Perbaikan

`includeControllerXmlComments` diubah menjadi `false`, beserta keterangan alasannya di dalam
`Program.cs` supaya tidak dinyalakan lagi tanpa sengaja.

Yang hilang: keterangan tingkat kelas controller. Yang tetap ada: keterangan pada endpoint,
parameter, dan schema — **di situlah seluruh keterangan yang dituntut acceptance criteria
diletakkan**, sehingga tidak satu pun acceptance criteria terganggu.

Terverifikasi pada `swagger.json` sebelum perbaikan, teks wajibnya memang menempel di operation
dan schema, bukan di tag controller:

| Teks | Kemunculan |
|---|---:|
| `DIABAIKAN` | 3 |
| `belum ditegakkan` | 1 |

### Pilihan yang tidak diambil

Menulis document filter yang memetakan tag nama-kelas ke nilai `[Tags(...)]`-nya akan membuat
deskripsi modul tampil di kepala tiap grup — lebih bagus, tetapi menambah mesin buatan sendiri.
Ditolak demi menjaga tampilan Swagger tetap seragam dengan seluruh modul lain yang sudah ada.

## 7. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Ukuran keluaran build | Bertambah satu berkas XML. Tidak diukur |
| Isi keterangan modul lain | Keterangan endpoint modul lain kini ikut tampil di Swagger. Isinya belum ditinjau; kemungkinan ada yang usang |

Butir kedua patut diperhatikan pemilik API: penyalaan ini memunculkan keterangan yang selama ini
tidak terlihat, termasuk keterangan yang mungkin sudah tidak sesuai keadaan.

## 8. Persetujuan — tertutup pada hari yang sama

Saat laporan ini pertama ditulis, Definition of Done belum lengkap: catatan rilis menuntut
persetujuan pemilik API, sementara `api_authority` masih `OPEN`.

**Keadaan itu berubah pada hari yang sama.** Yoga Aji Pratama ditetapkan pula sebagai pemilik
frontend dan pemilik API, dicatat pada `RM-DEC-028`. Kontrak API `0.1.0` naik dari `draft`
menjadi `approved`, dan catatan rilis disahkan bersamanya.

Dua hal yang ikut disahkan:

1. Perubahan bentuk balasan `/timeline`, dari `PagedResult` langsung menjadi selubung yang memuat
   halaman beserta keterangan kelengkapannya.
2. Field `access` yang ditambahkan pada seluruh balasan endpoint berkas rekam medis.

`BE-18` karena itu dicatat **`SELESAI`** pada roadmap, dengan DoD lengkap.

**Akibat yang lebih besar daripada task ini sendiri:** pengesahan kontrak membuka gerbang paralel
frontend. Sepuluh task `FE-00` sampai `FE-09` tidak lagi `TERTAHAN KONTRAK`.

## 9. Keadaan modul setelah task ini

**Seluruh pekerjaan kode backend modul Rekam Medis selesai.** Tidak ada lagi task yang menunggu
ditulis.

| Keadaan | Jumlah |
|---|---|
| `SELESAI` | 16 |
| `SELESAI` sebagian | 2 — `BE-09`, `BE-15` |
| `SIAP DIJALANKAN` | 1 — `BE-08` |
| `SIAP` | 0 |

Tiga butir yang tersisa seluruhnya bukan pekerjaan kode:

| Butir | Task | Menunggu |
|---|---|---|
| Isi awal master keperluan akses | `BE-09` | SOP rekam medis rumah sakit |
| Penjalanan pengisian data lama | `BE-08` | Penelaahan pada salinan data nyata |
| Pemberitahuan penulis CPPT | `BE-15` | Pemilik modul menjalankan penyampaian |

Ditambah satu pemeriksaan data yang disarankan: jumlah pasien yang `MergedToPatientId`-nya sudah
terisi. Lihat laporan `BE-16`.

## 10. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 11. Langkah berikutnya

Karena seluruh task backend sudah dikerjakan, langkah yang sesuai adalah **audit kesiapan
menyeluruh** — memeriksa modul ini secara netral terhadap keputusan, blueprint, kontrak, source,
dan buktinya, bukan menambah task baru.

Pekerjaan frontend masih tertahan pada gerbang yang sama sejak awal: kontrak API belum disahkan
karena pemiliknya belum ditunjuk.
