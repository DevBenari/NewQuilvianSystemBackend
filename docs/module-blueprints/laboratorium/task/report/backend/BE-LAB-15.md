# Laporan Perubahan Backend — `BE-LAB-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-15` |
| Judul | Monitoring tiga disiplin |
| Slice | `S15` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6, gelombang `MVP-3` |
| Trace | `FR-10.1` .. `FR-10.3`; `LAB-DEC-025`; `AC-41`, `AC-42`, `AC-19` |
| Contract version | `LAB-API-v1` r3 grup Lab Monitoring, ditambah `GET /lab-orders/by-discipline/{discipline}` |
| Dependency | `BE-LAB-01` **`SELESAI`**, `BE-LAB-14` **`SELESAI`** |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Ketiga butir DoD terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk grup Lab Monitoring; `TOUCHED LEGACY` untuk `LabOrderController` yang bertambah satu jalur baca |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-MOD-001`, `QBE-ENUM-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — tidak ada entity, configuration, maupun migration. Seluruh `QBE-CODE-*` — tidak ada nomor bisnis. `QBE-LOG-001`, `QBE-AUD-001`, `QBE-DEL-001` — grup ini hanya membaca |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Ketiga disiplin laboratorium berbagi satu daftar, padahal petugasnya berbeda orang.

> Petugas Patologi Anatomi membuka daftar pesanan dan melihat pekerjaan Mikrobiologi dan
> Patologi Klinik bercampur di dalamnya. Untuk menemukan miliknya, ia harus memilih penyaring
> disiplin lebih dahulu — **setiap kali membuka layar**, dan setiap kali dengan kemungkinan
> salah pilih.

Bukti lapangan menunjukkan laboratorium memakai tiga daftar sejajar sebagai tiga menu berbeda.
`LAB-DEC-025` mengikuti kenyataan itu, bukan sebaliknya.

---

## 2. Proses bisnis

### 2.1 Tiga menu, bukan satu penyaring — `AC-41`

| Yang dibuka | Yang tampil |
| --- | --- |
| `/clinical-pathology` | Hanya pesanan Patologi Klinik |
| `/anatomic-pathology` | Hanya pesanan Patologi Anatomi |
| `/microbiology` | Hanya pesanan Mikrobiologi |

Disiplin ditentukan **jalur yang dipanggil**, bukan ruas yang dikirim. Tidak ada cara memanggil
jalur Patologi Klinik lalu memperoleh pesanan Mikrobiologi, dan tidak ada pula cara membuka
salah satunya tanpa disiplin sama sekali.

### 2.2 Penyaring yang sama bagi ketiganya

Pasien, nomor rekam medis, nomor kunjungan, periode, jenis kunjungan, kunjungan baru atau lama,
unit layanan, ruangan, jenis penjamin, status pesanan, status wadah, dan penanda cito.

Status wadah menyaring dari **wadahnya**, bukan dari status pesanan: satu pesanan dapat memiliki
beberapa wadah berstatus berbeda, dan yang dicari adalah keberadaan salah satunya.

### 2.3 Dua hal yang sengaja tidak dilayani

| Yang tidak ada | Alasan |
| --- | --- |
| Bank Darah (`AC-42`) | Alur, regulasi, dan penelusuran kantongnya berbeda jauh. Menaruhnya di bawah Laboratorium karena "sama-sama darah" akan menyembunyikan perbedaan itu |
| Stok, pembelian, dan pemakaian reagen (`AC-19`, `LAB-DEC-014`) | Persediaan adalah urusan logistik, bukan pemeriksaan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabMonitoringDtos.cs` | **Baru.** `LabMonitoringQuery` dan `LabMonitoringItemResponse` |
| `.../Services/LabMonitoringService.cs` | **Baru.** Satu `GetByDisciplineAsync` yang dipakai ketiga jalur; hanya membaca |
| `.../Controllers/LabMonitoringController.cs` | **Baru.** Tiga endpoint `GET`, nol jalur tulis |
| `.../Controllers/LabOrderController.cs` | Bertambah `GET /by-discipline/{discipline}` beserta pengurai nilai disiplinnya |
| `Program.cs` | Satu baris pendaftaran `LabMonitoringService` |
| `Tests/.../LabMonitoringTests.cs` | **Baru.** Dua belas uji |
| `Tests/.../LabScopeBoundaryTests.cs` | **Baru.** Empat uji penelusuran batas modul |

**Tidak ada entity, configuration, maupun migration.** Seluruh isinya diturunkan dari
`LabOrder.Discipline`.

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Empat endpoint yang sudah tertulis sebagai `Rencana (belum tersedia)` kini tersedia. Tidak ada endpoint lama yang berubah bentuk |
| Kontrak integrasi | `NOT APPLICABLE` |
| Database | **Tidak ada dampak sama sekali** |
| Keamanan/Auth | Grup monitoring memakai `LabMonitoring : Read`; jalur per disiplin pada grup pesanan memakai `LabOrder : Read`, sama dengan jalur daftar yang sudah ada |

### 3.3 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Tiga jalur, satu perilaku** | Penyaring, proyeksi, dan pengurutan ditulis satu kali di service dan dipakai ketiganya. Yang tiga adalah jalurnya di controller. Menyalin tiga implementasi akan membuat "penyaring identik" pada DoD benar hari ini dan tidak lagi benar enam bulan lagi, tanpa ada yang menyadarinya |
| 2 | **Disiplin bukan ruas penyaring** | `LabMonitoringQuery` sengaja tidak memilikinya. Ada uji yang menjaga ketiadaan itu: begitu disiplin menjadi ruas biasa, tiga menu terpisah kehilangan alasan keberadaannya dan `LAB-DEC-025` terbatalkan diam-diam |
| 3 | **Penyaring "nomor pesanan" tidak dapat dipenuhi** | Kontrak menyebutnya, tetapi `LabOrder` **tidak memiliki kolom nomor pesanan sama sekali** — yang ada hanya `Id` dan nomor kunjungan pada encounter. Yang disediakan adalah penyaring nomor kunjungan. Menambah nomor pesanan berarti kolom baru, format kode, dan keunikan database — perubahan kontrak dan schema tersendiri, bukan cakupan task ini |
| 4 | **"Penjamin" diterjemahkan menjadi jenis penjamin** | Satu kunjungan dapat membawa beberapa penjamin sekaligus pada `TrxPatientEncounterGuarantor`. Yang disediakan adalah penyaring `PaymentType` pada kunjungan — tunai, asuransi, perusahaan. Menyaring per penjamin tertentu memerlukan keputusan tentang penjamin mana yang mewakili sebuah kunjungan, dan itu bukan keputusan Laboratorium |
| 5 | **Disiplin yang tidak dikenal ditolak `400`** | Pada `GET /lab-orders/by-discipline/{discipline}`, nilai yang tidak dikenal dijawab `400`, bukan daftar kosong. Daftar kosong akan terbaca sebagai "belum ada pekerjaan", padahal yang terjadi adalah salah ketik. Pengurainya menerima nama enum maupun bentuk bertanda hubung, supaya kedua grup dapat dipanggil dengan istilah yang sama |
| 6 | **Pesanan tanpa disiplin tidak muncul di mana pun** | `Discipline` boleh kosong semata-mata karena pesanan yang terbentuk sebelum kolomnya ada memang tidak pernah punya. Baris seperti itu tidak muncul pada satu pun dari ketiga daftar, dan itu perilaku yang benar — memasukkannya ke salah satu daftar berarti menebak disiplinnya. Ada uji yang mengunci perilaku ini |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-monitoring`

| Verb | Path | Permission | Request | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/clinical-pathology` | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` |
| `GET` | `/anatomic-pathology` | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` |
| `GET` | `/microbiology` | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` |

Ditambah pada grup Lab Order:

| Verb | Path | Permission | Request | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/lab-orders/by-discipline/{discipline}` | `LabOrder : Read` | `LabOrderPagedQuery` | `ApiResponse<PagedResult<LabOrderListResponse>>` |

Nilai `{discipline}` yang diterima: `clinical-pathology`, `anatomical-pathology`, `microbiology`,
atau nama enumnya langsung. Selain itu dijawab `400`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 259, Total: 259` | `PASS` | Naik dari 243; enam belas uji baru |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `ENVIRONMENT` | Seluruhnya `BLOCKED_BY_TEST_DB_CONFIGURATION`; angkanya tidak berubah |
| Checker QBE `Strict` atas 7 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **`AC-41`** — tiga daftar, data campuran | Klinik 2 baris, Anatomi 1, Mikro 1; tidak satu baris pun menyeberang | `PASS` | `AC41_TigaDaftarDenganDataCampuran_MasingMasingHanyaMenampilkanDisiplinnya` |
| Isi baris daftar pantau | Nama pasien, nomor rekam medis, nomor kunjungan, rekap wadah dan pemeriksaan, penanda cito | `PASS` | `BarisDaftarPantau_MembawaIdentitasPasienDanRekapPekerjaannya` |
| Penyaring cito | Hanya pesanan yang memuat pemeriksaan cito | `PASS` | `PenyaringCito_HanyaMenampilkanPesananYangMemuatPemeriksaanCito` |
| Penyaring nomor rekam medis | Cocok sebagian, menemukan pasien yang dicari | `PASS` | `PenyaringNomorRekamMedis_MenemukanPesananPasienYangDicari` |
| Penyaring status wadah | Menyaring dari wadahnya, bukan dari status pesanan | `PASS` | `PenyaringStatusWadah_MenyaringDariWadahnya_BukanDariStatusPesanan` |
| Pesanan tanpa disiplin | Tidak muncul pada satu pun dari ketiga daftar | `PASS` | `PesananTanpaDisiplin_TidakMunculPadaSatuPunDaftarPantau` |
| Penyaring identik | Ketiga jalur menerima tipe permintaan yang sama, dan seluruhnya `GET` | `PASS` | `KetigaEndpoint_MemakaiBentukPenyaringYangSamaPersis` |
| Disiplin bukan penyaring | `LabMonitoringQuery` nol ruas disiplin; tiga belas ruas kontrak lainnya ada | `PASS` | `PenyaringDaftarPantau_TidakMemilikiRuasDisiplin` |
| Bentuk kontrak ketiga endpoint | `GET`, route, dan permission sesuai `LAB-API-v1` r3 | `PASS` | `KetigaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak` |
| Bentuk kontrak jalur per disiplin | `GET by-discipline/{discipline}`, `LabOrder : Read` | `PASS` | `EndpointPesananPerDisiplin_MemakaiRouteDanPermissionYangDikunciKontrak` |
| **`AC-42`** — tipe dan anggota | Nol temuan atas delapan istilah Bank Darah | `PASS` | `AC42_TidakSatuPunTipeAtauAnggotaLaboratorium_MelayaniBankDarah` |
| **`AC-19`** — tipe dan anggota | Nol temuan atas sepuluh istilah persediaan reagen | `PASS` | `AC19_TidakSatuPunTipeAtauAnggotaLaboratorium_MenyimpanStokPembelianAtauPemakaianReagen` |
| **`AC-42`** — entity tersimpan | Nol temuan pada seluruh entity Laboratorium beserta kolomnya | `PASS` | `AC42_TidakSatuPunEntityTersimpan_MelayaniBankDarahMaupunReagen` |
| **`AC-42`** — route | Nol temuan pada seluruh route controller Laboratorium | `PASS` | `AC42_TidakSatuPunRouteLaboratorium_MelayaniBankDarahMaupunReagen` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Uji yang harus dijaga agar tidak lulus secara palsu

Uji penelusuran seperti `AC-42` mudah lulus karena alasan yang salah: bila penelusurannya
kebetulan tidak menemukan **satu pun** tipe Laboratorium, ia tetap melaporkan nol temuan dan
tampak hijau.

Karena itu setiap uji penelusuran pada `LabScopeBoundaryTests` lebih dahulu menuntut himpunan
yang ditelusurinya tidak kosong — ada tipe, ada entity, ada route. Baru sesudah itu ia menuntut
nol temuan.

### 5.2 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Uji integrasi terhadap PostgreSQL sungguhan | 52 uji `IntegrationTests.Postgres` terhalang `QUILVIAN_BILLING_TEST_DB`; akun aplikasi tidak memiliki hak `CREATEDB`. Keempat endpoint murni baca dan tidak menyentuh invariant yang ditegakkan database |
| Penyaring nomor pesanan | Kolomnya tidak ada — lihat bagian 3.3 butir 3 |
| Perintah database apa pun | Task ini tidak menyentuh schema |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-41` — tiga daftar, masing-masing hanya disiplinnya | **Terpenuhi** | `AC41_TigaDaftarDenganDataCampuran_...` |
| `AC-42` — tidak ada yang melayani Bank Darah | **Terpenuhi** | Tiga uji penelusuran: tipe dan anggota, entity, route |
| `AC-19` — tidak ada yang menyimpan stok, pembelian, maupun pemakaian reagen | **Terpenuhi** | Dua uji penelusuran |

| Butir DoD | Status |
| --- | --- |
| Tiga endpoint tersedia | **Terpenuhi**, ditambah `GET /lab-orders/by-discipline/{discipline}` |
| Penyaring identik | **Terpenuhi** — satu bentuk permintaan dan satu implementasi bagi ketiganya |
| `AC-41` terbukti | **Terpenuhi** |
| `AC-42` terbukti | **Terpenuhi**, sekaligus `AC-19` |

**Keempat butir DoD terpenuhi.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **(a)** Penyaring "nomor pesanan" yang disebut kontrak tidak dapat dipenuhi karena `LabOrder` tidak memiliki kolomnya; keputusan menambahnya milik pemilik blueprint. **(b)** "Penjamin" diterjemahkan menjadi jenis penjamin pada kunjungan, bukan penjamin tertentu — lihat bagian 3.3 butir 4. **(c)** Pesanan peninggalan yang `Discipline`-nya kosong tidak muncul pada satu pun daftar pantau; bila jumlahnya banyak di suatu lingkungan, pengisian mundurnya adalah pekerjaan data tersendiri |
| Risiko tersisa | **Rendah.** Keempat endpoint hanya membaca, tidak menyentuh schema, dan tidak mengubah jalur yang sudah ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. `BE-LAB-07` dan `BE-LAB-08` menunggu dependency eksternal `BE-EXT-01` .. `BE-EXT-03` dari `master-data` dan `registration-management` — itulah yang tersisa pada backend Laboratorium. 2. Memutuskan nasib penyaring nomor pesanan. 3. Meminta DBA menyediakan database test beserta hak `CREATEDB` |
