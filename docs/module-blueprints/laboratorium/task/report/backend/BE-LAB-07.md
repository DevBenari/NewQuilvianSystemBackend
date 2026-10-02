# Laporan Perubahan Backend — `BE-LAB-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-07` |
| Judul | Katalog, harga, dan cakupan penjamin — baca saja |
| Slice | `S14` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-0` |
| Trace | `FR-09.1` .. `FR-09.5`; `LAB-DEC-033`, `LAB-DEC-036`; `AC-43`, `AC-47`, `AC-48`, `AC-51`; `VAL-46`, `VAL-50`; `INV-22` |
| Contract version | `LAB-API-v1` r3 grup Lab Catalog; `LAB-INT-v1` r3 `INT-06` |
| Dependency | `BE-LAB-01` **`SELESAI`**, `BE-EXT-01` **`SELESAI`** — penahannya dicabut pada hari yang sama |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `3029af9`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Ketiga butir DoD terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk grup Lab Catalog; `TOUCHED LEGACY` untuk `LabExaminationService` yang bertambah satu aturan validasi |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-VAL-001`, `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — **tidak ada entity, configuration, maupun migration**, dan ketiadaan itu justru yang dituntut `AC-47`. Seluruh `QBE-CODE-*` — tidak ada nomor bisnis. `QBE-LOG-001`, `QBE-AUD-001` — grup katalog hanya membaca |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Dua hal sekaligus, dan keduanya berakar pada satu sebab: harga dan penggolongan tidak pernah
sampai ke tangan petugas laboratorium.

> **Pertama**, petugas tidak dapat melihat harga pemeriksaan sebelum memesannya. Pasien
> bertanya "berapa?", dan jawabannya baru muncul di kasir.
>
> **Kedua**, sistem tidak dapat menolak pemeriksaan yang salah disiplin. Petugas membuat pesanan
> Mikrobiologi lalu menambahkan Hemoglobin ke dalamnya, dan tidak ada yang menahannya. Pesanan
> campur aduk itu kemudian masuk ke daftar kerja disiplin yang salah.

---

## 2. Proses bisnis

### 2.1 Melihat harga bukan memesan — `AC-43`

| Yang dilakukan | Yang terbentuk |
| --- | --- |
| Membuka katalog dan memilih tiga pemeriksaan | **Tidak ada apa pun** |
| Melihat harga satuan 35.000, 30.000, dan 40.000 | **Tidak ada apa pun** |
| Melihat total 105.000 | **Tidak ada apa pun** |

Baris tagihan baru terbentuk ketika wadah dinyatakan layak, dan itu jauh di hilir. Batas ini
dijaga uji yang menghitung `BilChargeLine`, `CliClinicalMilestoneFact`, dan `LabExamination`
setelah ketiga harga dibaca — ketiganya nol.

### 2.2 Cakupan penjamin

| Keadaan | Yang ditampilkan |
| --- | --- |
| Penjamin tidak dikirim | Harga rumah sakit saja |
| Penjamin punya kontrak | Harga rumah sakit **dan** harga kontraknya |
| Penjamin tanpa kontrak | Harga rumah sakit, ditandai **tidak tercakup** beserta keterangannya |

Tidak tercakup **bukan berarti gratis**, dan bukan pula berarti pasien pasti membayar sendiri.
Yang pasti hanya: penjamin itu tidak punya harga kontrak untuk pemeriksaan tersebut. Keputusan
finansialnya tetap milik Billing.

### 2.3 `INV-22` — disiplin pesanan mengikat isinya

> Petugas membuat pesanan berdisiplin Mikrobiologi, lalu mencoba menambahkan Hemoglobin.
> Hemoglobin bertanda Patologi Klinik pada katalog, sehingga permintaan ditolak `422` dengan
> pesan `VAL-46`: *"Pemeriksaan ini bukan bagian dari Microbiology. Buat pesanan terpisah untuk
> disiplin yang sesuai."*

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabCatalogDtos.cs` | **Baru.** Tiga penyaring dan tiga bentuk jawaban |
| `.../Services/LabCatalogService.cs` | **Baru.** Tiga jalur baca; nol jalur ubah |
| `.../Controllers/LabCatalogController.cs` | **Baru.** Tiga endpoint `GET` |
| `.../Services/LabExaminationService.cs` | `AddAsync` menegakkan `INV-22` / `VAL-46` |
| `Program.cs` | Satu baris pendaftaran `LabCatalogService` |
| `Tests/.../LabCatalogTests.cs` | **Baru.** Tujuh belas uji |

**Tidak ada entity, configuration, maupun migration** — dan itu bukan kelalaian melainkan
`AC-47`.

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** untuk grup Lab Catalog: tiga endpoint yang sudah tertulis sebagai `Rencana (belum tersedia)` kini tersedia. **Satu perubahan perilaku** pada `POST /lab-examinations/by-order/{labOrderId}`: permintaan yang disiplinnya tidak sesuai kini ditolak `422`. Bentuk permintaan dan jawabannya tidak berubah |
| Kontrak integrasi | `INT-06` **Laboratorium membaca katalog, harga, dan cakupan penjamin** kini benar-benar terpakai. Baca saja, tanpa penyalinan tetap |
| Database | **Tidak ada dampak sama sekali.** Nol tabel, nol kolom, nol migration |
| Keamanan/Auth | Ketiga endpoint memakai `LabCatalog : Read`. Tidak ada permission tulis pada grup ini, karena tidak ada jalur tulisnya |

### 3.3 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **`VAL-50` ditegakkan lewat ketiadaan, bukan lewat penjaga** | Aturan berbunyi "percobaan mengubah data lewat modul Laboratorium ditolak `403`". Yang dibangun bukan penjaga yang menjawab `403`, melainkan grup endpoint yang **tidak punya satu pun jalur ubah** — sehingga percobaan itu berakhir `404`/`405` di lapisan routing, jauh sebelum menyentuh kode. Itu lebih kuat daripada penjaga: penjaga dapat dilupakan pada endpoint berikutnya, sedangkan jalur yang tidak ada tidak dapat dilupakan. Ada uji yang menjaga ketiadaan itu |
| 2 | **`INV-22` menuntut kedua disiplin diketahui** | Pesanan peninggalan sebelum kolom disiplin ada, dan katalog yang belum digolongkan, keduanya bernilai kosong. Menolak permintaan ketika salah satunya kosong akan mematikan pemesanan pada rumah sakit yang data induknya belum lengkap — padahal yang belum lengkap adalah data induknya, bukan permintaannya. Tiga uji mengunci ketiga kombinasi kosong itu |
| 3 | **Aturan tarif berlaku disamakan dengan jalur pemesanan** | `TarifBerlakuAsync` memakai aturan yang sama persis dengan `ResolveTariffAsync` pada `LabSpecimenService`: aktif, belum dihapus, dalam rentang berlaku, dan yang paling akhir mulai berlaku yang menang. Disengaja — harga yang dilihat petugas saat memesan harus sama dengan harga yang kelak disalin ke baris pemeriksaan. Bila keduanya berbeda aturan, pasien melihat satu angka dan ditagih angka lain |
| 4 | **Katalog yang belum digolongkan tetap tampil** | Tanpa penyaring disiplin, pemeriksaan yang `LabDiscipline`-nya kosong ikut tampil. Menyembunyikannya akan membuat katalog tampak kosong pada rumah sakit yang penggolongannya belum diisi, dan petugas menyimpulkan sistemnya rusak. Dengan penyaring disiplin, ia memang tidak ikut — karena disiplinnya belum diketahui |
| 5 | **Kelas perawatan diperlakukan sebagai penyaring opsional** | Kontrak penjamin yang tidak menyebut kelas berlaku untuk semua kelas. Bila keduanya ada, yang menyebut kelas didahulukan, lalu `Priority` yang lebih kecil. Aturan ini turunan dari bentuk `MstInsuranceTariff`; bila Master Data punya aturan pemilihan sendiri yang berbeda, ini perlu diselaraskan |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-catalog`

| Verb | Path | Permission | Request | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/examinations` | `LabCatalog : Read` | `LabCatalogQuery` | `ApiResponse<PagedResult<LabCatalogItemResponse>>` |
| `GET` | `/examinations/{procedureId}/price` | `LabCatalog : Read` | `LabPriceQuery` | `ApiResponse<LabPriceResponse>` |
| `GET` | `/tariffs` | `LabCatalog : Read` | `LabTariffQuery` | `ApiResponse<PagedResult<LabTariffViewResponse>>` |

`LabCatalogQuery`: `pageNumber`, `pageSize` (1..100), `discipline`, `search`,
`insuranceProviderId`, `patientClassId`.
`LabPriceQuery`: `insuranceProviderId`, `patientClassId`.
`LabTariffQuery`: `pageNumber`, `pageSize`, `procedureId`, `search`.

| Kode | Kapan |
| --- | --- |
| `200` | Berhasil |
| `404` | Jenis pemeriksaan tidak ditemukan |
| `422` | Tindakan yang diminta bukan pemeriksaan laboratorium |

**Tidak ada** `POST`, `PUT`, `PATCH`, maupun `DELETE` pada grup ini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Total: 288` | `PASS` | Naik dari 271; tujuh belas uji baru |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `ENVIRONMENT` | Seluruhnya `BLOCKED_BY_TEST_DB_CONFIGURATION`; angkanya tidak berubah |
| Checker QBE `Strict` atas 6 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **`AC-43`** — tiga pemeriksaan | Harga satuan 35.000, 30.000, 40.000; total 105.000; **nol** baris tagihan, fakta klinis, dan pemeriksaan | `PASS` | `AC43_MemilihTigaPemeriksaan_MenampilkanHargaSatuanDanTotalTanpaMembentukTagihan` |
| Tarif belum diatur | Harga kosong disertai keterangan, bukan nol yang menyesatkan | `PASS` | `TarifBelumDiatur_HargaKosongDanDisertaiKeterangan` |
| Penjamin berkontrak | Harga kontrak dan kodenya tampil; tidak ditandai tidak tercakup | `PASS` | `PenjaminBerkontrak_MenampilkanHargaKontraknya` |
| Penjamin tanpa kontrak | Ditandai tidak tercakup beserta keterangannya; harga rumah sakit tetap tampil | `PASS` | `PenjaminTanpaKontrak_DitandaiTidakTercakupDanBukanBerartiGratis` |
| **`AC-47`** — nol tabel tarif | Nol entity Laboratorium bernama `Tariff`/`Price`; kolom bertarif hanya `TariffId` dan `*Snapshot` | `PASS` | `Laboratorium_TidakMemilikiSatuPunTabelTarif` |
| **`AC-48`** dan **`VAL-50`** | Tepat tiga endpoint, seluruhnya `GET`; nol `POST`, `PUT`, `DELETE`, `PATCH` | `PASS` | `GrupKatalog_TidakMemilikiSatuPunJalurUbah` |
| Penyaring disiplin | Jalur Mikrobiologi hanya mengembalikan Kultur darah | `PASS` | `KatalogTersaringPerDisiplin_HanyaMenampilkanDisiplinYangDiminta` |
| Katalog belum digolongkan | Tetap tampil tanpa penyaring; tidak ikut muncul pada penyaring disiplin | `PASS` | `PemeriksaanBelumDigolongkan_TetapTampilTanpaPenyaringDisiplin` |
| Daftar tarif | Hanya tarif tindakan berpenanda `IsLaboratory`; tarif radiologi tidak ikut | `PASS` | `DaftarTarif_HanyaMemuatTarifPemeriksaanLaboratorium` |
| **`AC-51`** / **`VAL-46`** | Hemoglobin ke pesanan Mikrobiologi ditolak; pesan menyebut disiplin pesanannya; nol pemeriksaan tersimpan | `PASS` | `VAL46_MenambahkanHemoglobinKePesananMikrobiologi_Ditolak` |
| Disiplin sesuai | Pemeriksaan tetap dapat ditambahkan beserta salinan tarifnya | `PASS` | `DisiplinSesuai_PemeriksaanDapatDitambahkan` |
| Salah satu disiplin kosong | Ketiga kombinasi tidak ditolak | `PASS` | `SalahSatuDisiplinBelumDiketahui_TidakDitolak` |
| Bentuk kontrak ketiga endpoint | `GET`, route, dan permission sesuai `LAB-API-v1` r3 | `PASS` | `KetigaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Satu uji yang terlalu ketat, dan cara memperbaikinya

Uji `AC-47` mula-mula menuntut **setiap** kolom bertarif pada Laboratorium berakhiran
`Snapshot`. Ia gagal pada `LabExamination.TariffId`.

Kegagalan itu benar, dan yang keliru adalah ujinya. `TariffId` bukan salinan melainkan
**penunjuk** ke tarif milik Master Data — bentuk yang justru dituntut kamus data bagian 3.
Ujinya diperbaiki menjadi: kolom bertarif hanya boleh berupa `TariffId` atau berakhiran
`Snapshot`, dan tidak boleh ada satu pun **entity** tarif milik Laboratorium. Yang dilarang
`AC-47` adalah tabel tarif tersendiri, bukan tautan ke tarif orang lain.

### 5.2 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Uji integrasi terhadap PostgreSQL sungguhan | 52 uji `IntegrationTests.Postgres` terhalang `QUILVIAN_BILLING_TEST_DB`; akun aplikasi tanpa hak `CREATEDB`. Ketiga endpoint murni baca dan tidak menyentuh invariant yang ditegakkan database |
| Percobaan `403` `VAL-50` lewat HTTP | Tidak ada jalur ubah yang dapat dicoba — lihat bagian 3.3 butir 1 |
| Perintah database apa pun | Task ini tidak menyentuh schema |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-43` — harga satuan, subtotal, total, tanpa tagihan | **Terpenuhi** | `AC43_MemilihTigaPemeriksaan_...` |
| `AC-47` — tidak ada tabel tarif milik Laboratorium | **Terpenuhi** | `Laboratorium_TidakMemilikiSatuPunTabelTarif` |
| `AC-48` — tarif tidak dapat diubah dari modul Laboratorium | **Terpenuhi** | `GrupKatalog_TidakMemilikiSatuPunJalurUbah` |
| `AC-51` — pemeriksaan salah disiplin ditolak | **Terpenuhi** | `VAL46_MenambahkanHemoglobinKePesananMikrobiologi_Ditolak` |

| Butir DoD | Status |
| --- | --- |
| Tiga endpoint tersedia dan seluruhnya baca saja | **Terpenuhi** |
| `AC-47` dan `AC-48` terbukti | **Terpenuhi** |
| `VAL-46` terbukti setelah `BE-EXT-01` selesai | **Terpenuhi** — `BE-EXT-01` selesai pada hari yang sama |

**Ketiga butir DoD terpenuhi.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **(a)** Selama nilai `MstProcedure.LabDiscipline` belum diisi, penyaring disiplin mengembalikan daftar kosong dan `INV-22` tidak menolak apa pun. Keduanya perilaku yang benar, tetapi keduanya juga membuat fitur ini tampak tidak bekerja sampai penggolongan katalog dilakukan. **(b)** Aturan pemilihan kontrak penjamin — kelas didahulukan, lalu `Priority` — adalah turunan dari bentuk `MstInsuranceTariff`, bukan aturan yang tertulis di blueprint; bila Master Data punya aturan sendiri, ini perlu diselaraskan |
| Risiko tersisa | **Rendah.** Grup katalog hanya membaca dan tidak menyentuh schema. Satu perubahan perilaku ada pada jalur menambah pemeriksaan, dan ia hanya aktif ketika kedua disiplin diketahui |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. Mengisi nilai disiplin pada katalog, supaya penyaringan dan `INV-22` benar-benar berlaku. 2. `BE-LAB-08` — satu-satunya task backend yang tersisa; menunggu endpoint `INT-05` milik `registration-management` |
