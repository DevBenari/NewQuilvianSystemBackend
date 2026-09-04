# Laporan Perubahan Backend — `BE-LAB-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-14` |
| Judul | Daftar kerja dan pemantauan keterlambatan cito |
| Slice | `S7` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6, gelombang `MVP-3` |
| Trace | `FR-04.1` .. `FR-04.4`; `LAB-DEC-013`; `AC-10`, `AC-17`, `AC-39`; `VAL-39` |
| Contract version | `LAB-API-v1` r3 grup Lab Worklist — `GET /pending` dan `GET /cito-overdue` |
| Dependency | `BE-LAB-10` **`SELESAI`**, `BE-LAB-12` **`SELESAI`**, batas waktu cito dari `BE-LAB-02` **`SELESAI`** |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Keempat butir DoD terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` — service, controller, dan DTO grup Lab Worklist seluruhnya baru |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — **tidak ada entity, configuration, maupun migration**, dan ketiadaan itu justru yang dituntut `FR-04.4`. Seluruh `QBE-CODE-*` — tidak ada nomor bisnis. `QBE-LOG-001`, `QBE-AUD-001`, `QBE-VAL-001`, `QBE-DEL-001` — grup ini hanya membaca dan tidak mengubah state apa pun |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Petugas laboratorium tidak punya cara melihat pekerjaannya, dan kepala instalasi tidak punya
cara melihat yang terlambat.

> Kalium cito yang wadahnya dinyatakan layak pukul 09.00 dengan janji 60 menit seharusnya
> selesai pukul 10.00. Pukul 10.20 ia masih menganggur, dan **tidak ada satu layar pun** yang
> menunjukkannya. Yang mengetahuinya hanya dokter yang menunggu hasilnya — biasanya setelah
> menelepon.

Penanda cito yang dibangun `BE-LAB-10` juga belum berarti apa-apa tanpa daftar yang benar-benar
mendahulukannya.

---

## 2. Proses bisnis

### 2.1 Urutan daftar kerja — `AC-10`

| Yang masuk | Waktu | Kesegeraan | Posisi |
| --- | --- | --- | ---: |
| 14 pesanan | 10.00 | Biasa | 2 sampai 15 |
| 1 pesanan | 10.05 | **Cito** | **1** |

Urutannya tiga tingkat: kesegeraan, lalu waktu pesanan masuk, lalu waktu pemeriksaan dibuat.
Tingkat kedua itulah yang membuat dua pesanan cito tetap urut di antara mereka sendiri, bukan
teracak.

### 2.2 Keterlambatan cito — `AC-17`

| Keadaan | Hasil |
| --- | --- |
| Layak 09.00, batas 60 menit, belum selesai 10.20 | **Muncul**, kelebihan 20 menit |
| Layak 09.00, batas 60 menit, pesanan selesai 09.45 | **Tidak muncul** |
| Layak 09.00, batas 60 menit, sekarang 09.59 | Tidak muncul — belum lewat |
| Wadah **belum** dinyatakan layak, pesanan masuk 5 jam lalu | Tidak muncul — hitungannya belum mulai |
| Cito, jenis pemeriksaannya belum punya batas waktu | **Muncul**, tetapi **tidak** dianggap terlambat (`VAL-39`) |

Titik mulainya adalah saat wadah dinyatakan layak (`FR-04.3`). Sebelum bahannya layak,
laboratorium belum punya apa pun untuk dikerjakan dan tidak adil dihitung terlambat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabWorklistDtos.cs` | **Baru.** `LabWorklistPagedQuery`, `LabWorklistItemResponse`, `LabCitoOverdueResponse` |
| `.../Services/LabWorklistService.cs` | **Baru.** `GetPendingAsync` dan `GetCitoOverdueAsync`; hanya membaca |
| `.../Controllers/LabWorklistController.cs` | **Baru.** Dua endpoint `GET`, nol jalur tulis |
| `Program.cs` | Satu baris pendaftaran `LabWorklistService` |
| `Tests/.../LabWorklistTests.cs` | **Baru.** Empat belas uji |

**Tidak ada entity, configuration, maupun migration** — dan itu bukan kelalaian melainkan
`FR-04.4`.

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Dua endpoint yang sudah tertulis pada `contracts/api-contract.md` sebagai `Rencana (belum tersedia)` kini tersedia. Tidak ada endpoint lama yang tersentuh |
| Kontrak integrasi | `NOT APPLICABLE`. Grup ini tidak menerbitkan fakta apa pun |
| Database | **Tidak ada dampak sama sekali.** Nol tabel, nol kolom, nol migration |
| Keamanan/Auth | Kedua endpoint memakai `LabWorklist : Read` sesuai `contracts/permission-audit-matrix.md`. Karena hanya membaca, tidak ada jejak audit yang dituntut |

### 3.3 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Satuannya pemeriksaan, bukan pesanan** | Sejak `LAB-DEC-026` kesegeraan melekat pada pemeriksaan. Menyusun daftar kerja per pesanan akan membuat `AC-39` mustahil: satu pesanan berisi Kalium cito dan Kolesterol biasa hanya punya satu posisi, sehingga salah satunya pasti salah tempat |
| 2 | **Baris `LabValueBound` mana yang menentukan batas waktu** | `LabValueBound` dipecah menurut jenis kelamin dan kelompok umur untuk keperluan batas nilai, sementara batas waktu cito adalah janji layanan yang tidak bergantung pada keduanya. Blueprint tidak menyebut baris mana yang berlaku. Yang dipakai: baris umum — `All` tanpa kelompok umur — dan bila baris itu tidak mengisinya, **nilai terkecil** di antara baris aktif lainnya. Memilih yang terkecil berarti memilih janji yang paling ketat. **Menegaskannya adalah utang pemilik blueprint** |
| 3 | **Perhitungan keterlambatan dilakukan di memori** | Batas waktu berbeda-beda per jenis pemeriksaan, sehingga penjumlahan waktu di dalam SQL menjadi aritmetika tanggal yang berbeda bentuk pada setiap provider. Yang ditarik hanya pemeriksaan cito yang wadahnya sudah layak dan pekerjaannya belum selesai — himpunan yang secara wajar berukuran kecil. Penyaring disiplin dan pencarian tetap dikerjakan database |
| 4 | **"Selesai" diturunkan dari status pesanan** | `AC-17` menyebut "belum dirilis". Perilisan hasil adalah slice hasil yang masih tertahan `LAB-SIGN-001` dan belum ada. Yang dipakai sebagai penggantinya adalah keadaan yang benar-benar tersedia: pesanan berstatus `Completed` atau `Cancelled`, dan pemeriksaan berstatus `Voided` atau `Cancelled`. Begitu slice hasil dibangun, definisi ini perlu ditinjau ulang |
| 5 | **Waktu "sekarang" dapat disuntikkan** | `GetCitoOverdueAsync` menerima `asOf` opsional. Controller selalu mengirim `null`, sehingga perilaku produksinya memakai jam server. Yang memakainya adalah uji, supaya bukti keterlambatan tidak bergantung pada jam mesin yang menjalankannya |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-worklists`

| Verb | Path | Permission | Request | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/pending` | `LabWorklist : Read` | `LabWorklistPagedQuery` | `ApiResponse<PagedResult<LabWorklistItemResponse>>` |
| `GET` | `/cito-overdue` | `LabWorklist : Read` | `LabWorklistPagedQuery` | `ApiResponse<PagedResult<LabCitoOverdueResponse>>` |

`LabWorklistPagedQuery`: `pageNumber`, `pageSize` (dibatasi 1..100), `discipline`, `onlyCito`,
`search` — pencarian bebas pada kode dan nama pemeriksaan serta barcode wadah.

`LabCitoOverdueResponse` membawa `citoTurnaroundMinutes`, `deadlineAt`, `overdueMinutes`,
`hasCitoTurnaround`, dan `note`. Ketiga yang pertama kosong pada baris `VAL-39`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 243, Total: 243` | `PASS` | Naik dari 229; empat belas uji baru |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `ENVIRONMENT` | Seluruhnya `BLOCKED_BY_TEST_DB_CONFIGURATION`; angkanya tidak berubah |
| Checker QBE `Strict` atas 5 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **`AC-10`** — 14 biasa pukul 10.00, 1 cito pukul 10.05 | Cito di urutan **pertama**, bukan kelima belas | `PASS` | `AC10_SatuCitoPukul1005_BeradaDiUrutanPertamaDiAtasEmpatBelasPesananBiasa` |
| **`AC-10`** — dua cito berbeda waktu masuk | Keduanya di atas yang biasa; di antara keduanya urut menurut waktu masuk, bukan urutan pembuatan baris | `PASS` | `AC10_DuaCitoBerbedaWaktuMasuk_KeduanyaDiAtasYangBiasaDanUrutMenurutWaktuMasuk` |
| **`AC-39`** — satu pesanan, Kalium cito dan Kolesterol biasa | Hanya Kalium naik ke urutan pertama; Kolesterol tetap di antrean biasa | `PASS` | `AC39_SatuPesananBerisiKaliumCitoDanKolesterolBiasa_HanyaKaliumNaikKeAtas` |
| Pesanan selesai | Tidak muncul pada daftar kerja | `PASS` | `PesananYangSudahSelesai_TidakMunculPadaDaftarKerja` |
| **`AC-17`** — layak 09.00, batas 60 menit, sekarang 10.20 | Muncul dengan `overdueMinutes` **20**, `deadlineAt` 10.00 | `PASS` | `AC17_KaliumCito60Menit_LayakPukul0900_BelumSelesaiPukul1020_TerlambatDuaPuluhMenit` |
| **`AC-17`** — selesai pukul 09.45 | **Tidak** muncul | `PASS` | `AC17_PekerjaanSelesaiPukul0945_TidakMunculPadaDaftarPantau` |
| Belum melewati batas waktu | Tidak muncul | `PASS` | `CitoYangBelumMelewatiBatasWaktunya_TidakMuncul` |
| **`FR-04.3`** — wadah belum dinyatakan layak | Tidak dihitung terlambat walaupun pesanan masuk lima jam lalu | `PASS` | `CitoYangWadahnyaBelumDinyatakanLayak_TidakDihitungTerlambat` |
| **`VAL-39`** — cito tanpa batas waktu | Muncul, `hasCitoTurnaround` salah, `overdueMinutes` kosong, disertai keterangan | `PASS` | `VAL39_CitoTanpaBatasWaktu_TetapDitampilkanTetapiTidakDianggapTerlambat` |
| Urutan daftar pantau | Keterlambatan yang sesungguhnya di atas baris `VAL-39` | `PASS` | `BarisTanpaBatasWaktu_BeradaDiBawahKeterlambatanYangSesungguhnya` |
| Daftar pantau hanya cito | Pekerjaan biasa yang jauh melewati batas tetap tidak muncul | `PASS` | `DaftarPantau_HanyaMemuatCito` |
| **`FR-04.4`** — tidak ada tabel daftar kerja | Nol entity ber-nama `Worklist` pada model, dan nol jalur tulis pada controllernya | `PASS` | `TidakAdaTabelDaftarKerja_YangDibuat` |
| Bentuk kontrak kedua endpoint | `GET`, route, dan permission sesuai `LAB-API-v1` r3 | `PASS` | `KeduaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Kenapa waktu disuntikkan, bukan dibaca dari jam mesin

Uji keterlambatan yang memakai `DateTime.UtcNow` akan berubah hasilnya menurut kapan ia
dijalankan, dan yang paling buruk: ia lulus hari ini dan gagal tengah malam nanti tanpa ada yang
mengubah satu baris kode pun. Karena itu `GetCitoOverdueAsync` menerima `asOf`, dan seluruh
angka pada bagian 5 — 20 menit, 09.45, 09.59 — adalah angka tetap yang dapat diperiksa siapa
pun.

Controller tidak meneruskan parameter itu; produksi selalu memakai jam server.

### 5.2 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Uji integrasi terhadap PostgreSQL sungguhan | 52 uji `IntegrationTests.Postgres` terhalang `QUILVIAN_BILLING_TEST_DB`; akun aplikasi tidak memiliki hak `CREATEDB`. Kedua endpoint ini murni baca dan tidak menyentuh invariant yang ditegakkan database, sehingga provider InMemory memadai |
| Perintah database apa pun | Task ini tidak menyentuh schema |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-10` — cito di urutan atas, dan sesama cito urut menurut waktu masuk | **Terpenuhi** | Dua uji |
| `AC-17` — keterlambatan cito muncul, yang sudah selesai tidak | **Terpenuhi** | Empat uji, termasuk kedua jalur gagal |
| `AC-39` — hanya pemeriksaan cito yang naik, bukan seluruh pesanannya | **Terpenuhi** | `AC39_SatuPesananBerisiKaliumCitoDanKolesterolBiasa_HanyaKaliumNaikKeAtas` |

| Butir DoD | Status |
| --- | --- |
| Dua endpoint tersedia | **Terpenuhi** |
| Urutan cito terbukti | **Terpenuhi** |
| Perhitungan keterlambatan terbukti pada kedua jalur | **Terpenuhi** — muncul, dan tidak muncul |
| Tidak ada tabel daftar kerja yang dibuat | **Terpenuhi**, beserta ujinya |

**Keempat butir DoD terpenuhi.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **(a)** Baris `LabValueBound` mana yang menentukan batas waktu cito belum ditegaskan blueprint; turunan yang dipakai ada pada bagian 3.3 butir 2. **(b)** Definisi "selesai" masih diturunkan dari status pesanan karena slice hasil tertahan `LAB-SIGN-001`; perlu ditinjau ulang begitu perilisan hasil dibangun. **(c)** `LabValueBound` masih kosong di dev pemilik, sehingga daftar pantau di sana akan menampilkan seluruh cito sebagai baris `VAL-39` sampai batas waktunya diisi — itu perilaku yang benar, tetapi mudah disalahartikan sebagai kesalahan |
| Risiko tersisa | **Rendah.** Grup ini hanya membaca, tidak menyentuh schema, dan tidak mengubah satu pun jalur yang sudah ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. `BE-LAB-15` — monitoring tiga disiplin; penahannya `BE-LAB-14` baru saja dicabut. 2. Mengisi `LabValueBound` beserta batas waktu citonya agar daftar pantau berarti. 3. Menegaskan baris `LabValueBound` mana yang menentukan batas waktu cito |
