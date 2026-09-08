# Laporan Perubahan Backend — `BE-LAB-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-10` |
| Judul | Penanda cito dan duplo per pemeriksaan |
| Slice | `S1a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `FR-01.1` .. `FR-01.4`; `LAB-DEC-013`, `LAB-DEC-026`; `AC-18`, `AC-39`, `AC-40`; `VAL-03`, `VAL-04` |
| Contract version | `LAB-API-v1` r3 — `PUT /lab-examinations/{id}/urgency` dan `PUT /lab-examinations/{id}/duplo`; `LAB-STATE-v1` r2; `LAB-VAL-v1` r3 |
| Dependency | `BE-LAB-09` **`SELESAI`**; `BE-LAB-16` **`SELESAI`** menyediakan grup endpointnya |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, migration, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Keempat butir DoD terpenuhi; migration aditifnya terbukti dua arah pada dev pemilik |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE`. Entri registry 2026-09-02 dan 2026-09-03 memberi wewenang source, pembuatan migration, dan eksekusi ke dev pemilik |
| Keberlakuan | `TOUCHED LEGACY` untuk `LabTransitionHistory` dan grup endpoint yang sudah ada; `NEW CODE` untuk kedua endpoint, kedua DTO, migration, dan berkas ujinya |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-DTO-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001`, `QBE-ENUM-001`, `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, `QBE-MOD-003` — tidak ada entity maupun modul baru. Seluruh `QBE-CODE-*` — tidak ada nomor bisnis. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION` |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Kesegeraan melekat pada **pesanan**, sehingga tidak dapat dibedakan per pemeriksaan.

> Seorang dokter memesan Kalium dan Kolesterol sekaligus. Kalium dibutuhkan sekarang — hasilnya
> menentukan tindakan dalam hitungan menit. Kolesterol dapat menunggu sampai besok. Dengan
> penanda pada tingkat pesanan, dokter hanya punya dua pilihan: menandai **keduanya** cito, atau
> **tidak sama sekali**.

Menandai keduanya cito membuat laboratorium mendahulukan pekerjaan yang tidak mendesak, dan
lama-lama penanda cito berhenti berarti apa-apa. Tidak menandainya sama sekali menunda
pemeriksaan yang benar-benar mendesak.

---

## 2. Proses bisnis

### 2.1 Contoh berangka — `AC-39`

| | Sebelum | Sesudah |
| --- | --- | --- |
| Satuan penanda | Pesanan | **Pemeriksaan** |
| Kalium | Cito hanya bila seluruh pesanan cito | **Cito** |
| Kolesterol | Ikut cito, mau tidak mau | **Biasa** |
| Yang naik ke urutan atas daftar kerja | Keduanya, atau tidak sama sekali | **Hanya Kalium** |

### 2.2 Siapa yang boleh menandai

| Penanda | Yang boleh | Alasan |
| --- | --- | --- |
| Cito | **Dokter pemesan pesanan itu** (`VAL-03`) | Kesegeraan adalah penilaian klinis atas pasiennya sendiri, bukan keputusan administratif |
| Duplo | Petugas berwenang lewat permission `LabExamination : Update` | Duplo adalah keputusan pelaksanaan laboratorium, bukan penilaian klinis dokter pemesan |

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Dokter lain menandai cito | Ditolak `403` `VAL-03`; **tidak ada satu ruas pun yang berubah** |
| Pesanan sudah `Completed` atau `Cancelled` | Ditolak `409` `VAL-04` — tidak ada lagi pekerjaan yang dapat didahulukan |
| Wadah penopangnya sudah ditolak | Penanda duplo ditolak `409` — bahan yang tidak layak tidak dikerjakan sekali pun, apalagi dua kali |
| Pemeriksaan sudah gugur atau dibatalkan | Kedua penanda ditolak `409` |
| Menyetel nilai yang sudah berlaku | Diterima, tetapi **tidak** menambah baris riwayat — itu bukan perpindahan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Pertentangan dokumen yang harus diselesaikan lebih dulu

DoD task ini menuntut riwayat terbentuk pada setiap penandaan, dan
`contracts/state-transition-matrix.md` menyebut riwayat itu **berlingkup `LabExamination`**.
Keduanya tidak dapat dipenuhi: `LabTransitionScope` tidak punya nilai `LabExamination`, dan
`LabTransitionHistory` tidak punya kolom yang menunjuk pemeriksaan.

Sumber yang tersedia saling bertentangan, dan pertentangannya sudah dicatat terbuka sejak
`BE-LAB-09`:

| Sumber | Yang dinyatakannya |
| --- | --- |
| `erd/data-dictionary.md` bagian 4 | `LabTransitionHistory` bertambah `LabExaminationId`; `LabTransitionScope` bertambah nilai `LabExamination` |
| `roadmap/backend-roadmap.md` bagian 6 | `LabTransitionHistory` — `Diperbarui`, **tambah** `LabExaminationId`, tambah kolom aman |
| `roadmap/backend-roadmap.md` bagian 8.3 | `TrxLabTransitionHistory` — "Sudah ada, dipakai apa adanya. **Tidak ada pekerjaan struktur**" |

Dua dari tiga sepakat, dan yang menyendiri masih menyebut nama tabel sebelum `BE-LAB-19`
mengganti namanya — tanda bahwa baris itulah yang tertinggal. Kamus data dan bagian 6 yang
dipakai; bagian 8.3 diperbaiki mengikutinya dan pertentangannya ditutup.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Enums/LaboratoryEnums.cs` | `LabTransitionScope` bertambah `LabExamination = 3`. Nilai lama tidak bergeser |
| `.../Models/LabTransitionHistory.cs` | Bertambah `LabExaminationId` dan navigasinya |
| `.../LabTransitionHistoryConfiguration.cs` | Index dan foreign key `Restrict` untuk kolom baru |
| `.../DTOs/LabExaminationDtos.cs` | `SetLabExaminationUrgencyRequest` dan `SetLabExaminationDuploRequest` — **baru** |
| `.../Services/LabExaminationService.cs` | `SetUrgencyAsync`, `SetDuploAsync`, penjaga keadaan bersama, pemuat pesanan, dan `AppendHistory` berlingkup pemeriksaan. `LabExaminationForbiddenException` baru |
| `.../Controllers/LabExaminationController.cs` | Dua endpoint `PUT` dan pemetaan `403` |
| `Migrations/20260904035620_AddLabExaminationIdToLabTransitionHistory.cs` | **Baru.** Aditif |
| `Migrations/scripts/20260904035620_...sql` dan `README.md` | Skrip idempotent beserta barisnya pada daftar |
| `Tests/.../LabExaminationUrgencyTests.cs` | **Baru.** Empat belas uji |
| `Tests/.../LabExaminationEndpointTests.cs` | Kawat pemicu jumlah endpoint disesuaikan dari empat menjadi enam |
| `.../Services/LabSpecimenService.cs` | `MoveExaminationsAsync` menerima pesanan dan nama tindakan, lalu menulis riwayat per pemeriksaan yang berpindah; `AppendExaminationHistory` baru |
| `Tests/.../LabExaminationAuditTrailTests.cs` | **Baru.** Enam uji atas keenam kejadian berlingkup pemeriksaan |
| `contracts/permission-audit-matrix.md` | Dua baris audit untuk penandaan cito dan duplo |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Dua endpoint baru sesuai `LAB-API-v1` r3; keduanya sudah tertulis di `contracts/api-contract.md` sebagai `Rencana (belum tersedia)` dan kini tersedia. Tidak ada endpoint lama yang berubah bentuk |
| Kontrak integrasi | `NOT APPLICABLE`. Penanda cito dan duplo **tidak** menyentuh salinan tarif maupun muatan fakta ke Billing — dampaknya pada tarif masih `LAB-OPEN-013` |
| Database | **Aditif.** Satu kolom nullable, satu index, satu foreign key `Restrict`. Dijalankan dua arah pada dev pemilik |
| Keamanan/Auth | Kedua endpoint memakai `LabExamination : Update` sesuai `contracts/permission-audit-matrix.md`. **Satu aturan tambahan ditulis sebagai kode**: `VAL-03` membandingkan pelaku dengan dokter pemesan, dan `CAP-16` sudah membuktikan sistem permission tidak dapat melakukannya |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **`VAL-04` didahulukan atas `VAL-03`** | Pesanan yang sudah selesai tidak dapat diubah oleh siapa pun, termasuk dokter pemesannya. Menjawab `409` lebih benar daripada menjawab `403` kepada orang yang sebenarnya berwenang — jawaban `403` akan membuatnya mengira haknya dicabut, padahal yang habis adalah waktunya. Ada uji khusus yang mengunci urutan ini |
| 2 | **`VAL-03` tidak berlaku bagi duplo** | Matriks state menyebut yang boleh menandai duplo adalah "petugas berwenang menetapkan kelayakan atau analis", bukan dokter pemesan. Menyalin `VAL-03` ke sana akan mengarang aturan yang tidak diputuskan siapa pun. Ada uji yang mengunci ketiadaan aturan itu, supaya penyalinan tidak terjadi diam-diam di kemudian hari |
| 3 | **Menyetel nilai yang sama tidak menambah riwayat** | DoD menuntut riwayat pada setiap penandaan. Menyetel cito pada pemeriksaan yang sudah cito bukan penandaan melainkan pengulangan, dan riwayat mencatat perpindahan keadaan, bukan jumlah kali tombol ditekan. Permintaannya tetap dijawab `200` dengan keadaan terkini |
| 4 | **Dua baris audit ditambahkan ke matriks** | `contracts/permission-audit-matrix.md` bagian 4 belum memuat baris untuk `Examination.SetUrgency` dan `Examination.SetDuplo`. Keduanya ditambahkan atas instruksi pemilik modul pada sesi yang sama, mengikuti pola `Order.SetUrgency` yang sudah ada di sana |
| 5 | **Empat jejak audit yang hilang ikut ditutup** | Matriks audit menuntut `Examination.Add`, `Examination.ChargeEligible`, `Examination.Void`, dan `Examination.Cancel` masing-masing meninggalkan satu baris riwayat. **Tidak satu pun pernah ditulis** — bukan karena terlupa, melainkan karena kolom penunjuk dan nilai enum lingkupnya baru ada sejak task ini. Keempatnya ditutup atas instruksi pemilik modul; lihat bagian 3.5 |

### 3.5 Empat jejak audit yang hilang, ditutup pada sesi yang sama

Setelah kolom `LabExaminationId` dan nilai `LabTransitionScope.LabExamination` ada, pemeriksaan
ulang menunjukkan bahwa **tidak satu pun** kejadian berlingkup pemeriksaan pernah menulis baris
riwayat. Yang ada selama ini hanya catatan `LoggerService` — catatan aplikasi, bukan jejak audit
permanen, dan `QBE-AUD-001` menuntut keduanya dipisahkan.

| Kejadian | Sebelum | Sesudah |
| --- | --- | --- |
| `Examination.Add` | Hanya log aplikasi | Satu baris riwayat, `ToStatus` `Ordered` |
| `Examination.Cancel` | Hanya log aplikasi | Satu baris riwayat **beserta alasannya** |
| `Examination.ChargeEligible` | Tidak ada apa pun | Satu baris **per pemeriksaan** yang benar-benar berpindah |
| `Examination.Void` | Tidak ada apa pun | Satu baris per pemeriksaan, membawa catatan penolakan wadahnya |

Dua yang terakhir ditulis dari `LabSpecimenService.MoveExaminationsAsync`, yang kini menerima
pesanan dan nama tindakannya. Pemeriksaan yang sudah berada pada status tujuan tidak
menghasilkan baris apa pun, sehingga menyatakan wadah layak dua kali tetap meninggalkan dua
baris — satu untuk setiap pemeriksaan — bukan empat.

**Satu selisih yang tidak ditutup.** Matriks menandai `Examination.Cancel` sebagai kejadian yang
alasannya **wajib**, tetapi tidak ada aturan `VAL-*` yang menuntutnya dan
`CancelLabExaminationRequest.Reason` tetap boleh kosong — sama seperti pembatalan wadah. Alasan
yang diberikan kini tersimpan pada barisnya; menjadikannya wajib berarti menolak permintaan yang
selama ini diterima, dan itu keputusan kontrak milik pemilik blueprint.

---

## 4. Dokumentasi endpoint

| Verb | Route | Permission | Request | Response |
| --- | --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/laboratory-management/lab-examinations/{id}/urgency` | `LabExamination : Update` | `SetLabExaminationUrgencyRequest` — `{ "isCito": true }` | `ApiResponse<LabExaminationResponse>` |
| `PUT` | `/api/v1/health-services/laboratory-management/lab-examinations/{id}/duplo` | `LabExamination : Update` | `SetLabExaminationDuploRequest` — `{ "isDuplo": true }` | `ApiResponse<LabExaminationResponse>` |

| Kode | Kapan |
| --- | --- |
| `200` | Berhasil, termasuk ketika nilainya memang sudah berlaku |
| `403` | `VAL-03` — pelaku bukan dokter pemesan (hanya pada `urgency`) |
| `404` | Pemeriksaan atau pesanannya tidak ditemukan |
| `409` | `VAL-04`, wadah sudah ditolak, atau pemeriksaan sudah gugur maupun dibatalkan |

Waktu dan pelaku penandaan **tidak** diterima dari pemanggil; keduanya diisi backend.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 229, Total: 229` | `PASS` | Naik dari 209; dua puluh uji baru |
| `Examination.Add` meninggalkan riwayat | Satu baris, `ToStatus` `Ordered` | `PASS` | `MenambahPemeriksaan_MeninggalkanSatuBarisExaminationAdd` |
| `Examination.Cancel` meninggalkan riwayat | Satu baris beserta alasannya | `PASS` | `MembatalkanPemeriksaan_MeninggalkanSatuBarisExaminationCancelBesertaAlasannya` |
| `Examination.ChargeEligible` per pemeriksaan | Dua baris untuk wadah dua pemeriksaan | `PASS` | `MenyatakanWadahLayak_MeninggalkanSatuBarisChargeEligiblePerPemeriksaan` |
| Menyatakan layak dua kali | Tetap dua baris, bukan empat | `PASS` | `MenyatakanLayakDuaKali_TidakMenggandakanBarisRiwayat` |
| `Examination.Void` per pemeriksaan | Dua baris, masing-masing membawa catatan penolakan | `PASS` | `MenolakWadah_MeninggalkanSatuBarisVoidPerPemeriksaanBesertaCatatannya` |
| Kedua lingkup tidak saling menggantikan | Baris pemeriksaan menunjuk pemeriksaan dan bukan wadah; sebaliknya juga | `PASS` | `BarisBerlingkupPemeriksaan_TidakMenunjukWadahDanSebaliknya` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `ENVIRONMENT` | Seluruhnya `BLOCKED_BY_TEST_DB_CONFIGURATION`; angkanya tidak berubah |
| Checker QBE `Strict` atas 8 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **`AC-18`** — dokter pemesan menandai cito | `Urgency` menjadi `Cito`, `UrgencyMarkedAt` dan `UrgencyMarkedByUserId` terisi, satu baris riwayat berlingkup `LabExamination` | `PASS` | `AC18_DokterPemesanMenandaiCito_MenyimpanWaktuPelakuDanSatuBarisRiwayat` |
| **`AC-18`** — mengembalikan menjadi biasa | Riwayat bertambah menjadi dua baris, arahnya `Cito` ke `Routine` | `PASS` | `AC18_MengembalikanCitoMenjadiBiasa_MenambahSatuBarisRiwayatLagi` |
| **`VAL-03`** — dokter lain menandai cito | `403`; `Urgency`, `UrgencyMarkedAt`, `UrgencyMarkedByUserId`, dan riwayat **seluruhnya tidak berubah** | `PASS` | `VAL03_DokterLainMenandaiCito_Ditolak403DanTidakMengubahApaPun` |
| **`VAL-04`** — pesanan sudah `Completed` | `409` | `PASS` | `VAL04_PesananSudahSelesai_Ditolak409` |
| Urutan `VAL-04` sebelum `VAL-03` | Pesanan `Cancelled` dan pelaku bukan pemesan tetap dijawab `409` | `PASS` | `PesananSelesaiDanPelakuBukanPemesan_Menjawab409BukanNya403` |
| **`AC-39`** — cito dan biasa berdampingan | Kalium `Cito`, Kolesterol `Routine` tanpa jejak penandaan | `PASS` | `AC39_SatuPesananMemuatKaliumCitoDanKolesterolBiasaSekaligus` |
| **`AC-40`** — duplo hanya mengenai barisnya | `IsDuplo` benar pada Kalium, tetap salah pada Kolesterol | `PASS` | `AC40_MenandaiDuplo_HanyaMengenaiBarisYangDitandai` |
| **`AC-40`** — tidak ada kesegeraan pada pesanan | Nol route memuat `urgency`, `cito`, maupun `duplo` pada `LabOrderController` | `PASS` | `GrupLabOrder_TidakMemilikiEndpointKesegeraanSamaSekali` |
| Pengulangan tidak menambah riwayat | Dua kali cito berturut-turut tetap satu baris | `PASS` | `MenyetelKesegeraanYangSudahBerlaku_TidakMenambahRiwayat` |
| Wadah ditolak | Penanda duplo `409` | `PASS` | `WadahSudahDitolak_PenandaDuploDitolak409` |
| Pemeriksaan dibatalkan | Kedua penanda `409` | `PASS` | `PemeriksaanYangSudahDibatalkan_TidakDapatDitandaiCitoMaupunDuplo` |
| Bentuk kontrak kedua endpoint | `PUT`, route, dan permission sesuai `LAB-API-v1` r3 | `PASS` | `KeduaEndpoint_MemakaiPutDanPermissionYangDikunciKontrak` |
| **Migration maju** ke `QuilvianNewDevYoga` | `Done.` | `PASS` | `dotnet ef database update`, 2026-09-04 |
| **Migration mundur** | `Done.`; daftar kembali menampilkan `(Pending)` | `PASS` | Kolom, index, dan foreign key-nya terlepas |
| **Migration maju kedua** | `Done.`; tidak ada lagi `(Pending)` | `PASS` | Database ditinggalkan pada keadaan target |

Uji manual: `NOT FEASIBLE`.

### 5.1 Kawat pemicu yang menyala, dan itu memang gunanya

`BE-LAB-16` meninggalkan uji yang menuntut grup pemeriksaan punya **tepat empat** endpoint,
dengan komentar yang menyebut dua sisanya adalah cakupan `BE-LAB-10`. Uji itu gagal begitu
kedua endpoint ini dipasang.

Kegagalannya benar dan disengaja: ia memaksa penambahan endpoint pada grup ini disadari, bukan
lewat begitu saja. Angkanya diperbarui menjadi enam beserta komentar yang menjelaskan bahwa ia
tetap kawat pemicu, bukan sekadar penghitung.

### 5.2 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Uji integrasi terhadap PostgreSQL sungguhan | 52 uji `IntegrationTests.Postgres` terhalang `QUILVIAN_BILLING_TEST_DB`; akun aplikasi tidak memiliki hak `CREATEDB`. Bukti aturan di sini dihasilkan lewat provider InMemory, dan bentuk schema-nya lewat migration yang benar-benar dijalankan |
| `AC-39` pada urutan daftar kerja | Kartu task menyebut "hanya Kalium naik ke urutan atas daftar kerja". Daftar kerja itu sendiri adalah cakupan `BE-LAB-14`, dan belum ada. Yang dibuktikan di sini adalah datanya: satu pesanan memuat kedua tingkat kesegeraan sekaligus |
| Eksekusi migration di luar dev pemilik | Wewenang terpisah |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-18` — penandaan menyimpan waktu, pelaku, dan riwayat; jalur gagalnya `403` dan `409` | **Terpenuhi** | Empat uji, lihat bagian 5 |
| `AC-39` — satu pesanan memuat cito dan biasa sekaligus | **Terpenuhi** | `AC39_SatuPesananMemuatKaliumCitoDanKolesterolBiasaSekaligus` |
| `AC-40` — penanda berada pada pemeriksaan, dan tidak ada endpoint kesegeraan pada pesanan | **Terpenuhi** | Dua uji, keduanya arah berbeda |

| Butir DoD | Status |
| --- | --- |
| Dua endpoint tersedia | **Terpenuhi** |
| `VAL-03` dan `VAL-04` terbukti | **Terpenuhi** |
| `AC-40` terbukti | **Terpenuhi** |
| Riwayat terbentuk pada setiap penandaan | **Terpenuhi** — berlingkup `LabExamination` dan menunjuk pemeriksaannya |

**Keempat butir DoD terpenuhi.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas task ini. Satu `xUnit2029` yang sudah ada pada `LabValueBoundServiceTests.cs` tidak disentuh |
| Masalah yang diketahui | **(a)** Matriks audit menandai `Examination.Cancel` sebagai kejadian yang alasannya wajib, sementara tidak ada `VAL-*` yang menuntutnya dan ruas alasannya tetap boleh kosong — lihat bagian 3.5. **(b)** Dampak cito dan duplo terhadap tarif masih `LAB-OPEN-013`; keduanya sengaja tidak menyentuh salinan tarif |
| Risiko tersisa | **Rendah.** Perubahan schema-nya aditif, kedua endpoint baru, dan tidak ada jalur lama yang berubah perilakunya |
| Perubahan sampingan | Dua, keduanya disengaja. **(a)** Kawat pemicu jumlah endpoint pada `LabExaminationEndpointTests.cs` disesuaikan dari empat menjadi enam — lihat bagian 5.1. **(b)** Empat jejak audit berlingkup pemeriksaan yang tidak pernah ditulis siapa pun ditutup atas instruksi pemilik modul — lihat bagian 3.5 |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. `BE-LAB-14` — daftar kerja, yang akan memakai penanda cito ini untuk urutannya. 2. Memutuskan apakah alasan pembatalan pemeriksaan menjadi wajib, sesuai matriks audit. 3. Meminta DBA menyediakan database test beserta hak `CREATEDB` |
