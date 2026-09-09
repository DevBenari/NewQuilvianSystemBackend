# Laporan Perubahan Backend — `BE-EXT-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-01` |
| Judul | [Master Data] Kolom disiplin pada `MstProcedure` |
| Slice | `S14` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-0` |
| Trace | `LAB-DEC-036`, `LAB-COORD-005` — disetujui 2026-09-01; `AC-51` bergantung padanya |
| Contract version | `erd/data-dictionary.md` bagian 9b.1 |
| Dependency | Tidak ada. **Bukan milik Laboratorium** — dikerjakan atas instruksi pemilik modul yang juga kontributor `master-data` |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData`, migration, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Pengisian nilai untuk katalog yang sudah ada sengaja tidak dilakukan — lihat bagian 3.3 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `Master / Reference` |
| Pemilik dan prefix registry | Prefix `Mst`, lifecycle `ACTIVE`. Persetujuan menambah kolom diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001` (`LAB-COORD-005`) |
| Keberlakuan | `TOUCHED LEGACY` — `MstProcedure` sudah ada; satu kolom ditambahkan |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-ENT-003`, `QBE-CFG-002`, `QBE-ENUM-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` — tidak ada entity baru. Seluruh `QBE-CODE-*`, `QBE-API-001`, `QBE-PERM-001` — tidak ada endpoint maupun nomor bisnis |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Katalog tindakan tidak dapat membedakan ketiga disiplin laboratorium.

> `MstProcedure` sudah punya `IsLaboratory`, `IsRadiology`, `IsSurgery`, dan `IsTherapy` —
> tetapi tidak ada pembeda antara Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Yang
> tersedia hanya `ProcedureGroupName` dan `ProcedureCategoryName` berupa **teks bebas**.

Akibatnya sistem tidak punya cara mengetahui bahwa Hemoglobin adalah pemeriksaan Patologi
Klinik. Petugas dapat membuat pesanan Mikrobiologi lalu menambahkan Hemoglobin ke dalamnya, dan
tidak ada yang menahannya (`INV-22`).

---

## 2. Proses bisnis

Tidak ada perilaku yang berubah pada rilis ini. Yang ditambahkan adalah **tempat** menyimpan
penggolongan, sehingga `BE-LAB-07` dapat menyaring katalog per disiplin dan `INV-22` punya dasar
untuk menolak pemeriksaan yang tidak sesuai disiplin pesanannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../MasterData/Models/MstProcedure.cs` | Bertambah `LabDiscipline?` |
| `.../Configurations/HealthServices/MstProcedureConfiguration.cs` | Enum disimpan `int`; index bersyarat `"LabDiscipline" IS NOT NULL` |
| `Migrations/20260904065309_AddLabDisciplineAndReferralMasterData.cs` | **Baru**, bersama `BE-EXT-02` |
| `Tests/.../MasterData/ReferralMasterDataTests.cs` | **Baru**, empat uji untuk task ini |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Tidak ada endpoint yang ditambah maupun berubah bentuk |
| Database | **Aditif.** Satu kolom `integer` boleh kosong, satu index bersyarat. Dijalankan dua arah pada `QuilvianNewDevYoga` |
| Keamanan/Auth | `NOT APPLICABLE` |

### 3.3 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Nilainya tidak diisi, dan itu keputusan** | DoD menyebut "nilainya terisi". Menggolongkan setiap pemeriksaan yang sudah ada ke salah satu dari tiga disiplin adalah **keputusan klinis**, bukan turunan teknis: tidak ada aturan pada blueprint maupun pada data yang memberitahu bahwa Kultur darah adalah Mikrobiologi sementara Hemoglobin adalah Patologi Klinik. Menebaknya akan menghasilkan katalog yang tampak lengkap tetapi salah golong — dan `INV-22` kemudian menolak pesanan yang sebenarnya sah, dengan pesan yang membingungkan. Yang dibutuhkan adalah daftar penggolongan dari pihak laboratorium |
| 2 | **Index-nya bersyarat** | Tindakan non-laboratorium tidak punya disiplin, dan itu mayoritas isi tabel. Index difilter `"LabDiscipline" IS NOT NULL` supaya hanya memuat baris yang benar-benar bermakna |
| 3 | **Satu-satunya tambahan Laboratorium** | `LAB-DEC-036` menegaskan batasnya. Ada uji yang menahan satuan hasil, batas nilai, jenis wadah, dan batas waktu cito agar tidak menyelinap ke `MstProcedure` di kemudian hari |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menyentuh satu pun endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 271, Total: 271` | `PASS` | Naik dari 259 |
| Checker QBE `Strict` | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| Kolom ada, boleh kosong, disimpan `int` | Terbukti pada model relasional | `PASS` | `MstProcedure_MemilikiKolomDisiplinYangBolehKosongDanDisimpanSebagaiInt` |
| Index bersyarat | Filter `"LabDiscipline" IS NOT NULL` terpasang | `PASS` | `MstProcedure_MemilikiIndexDisiplinYangHanyaMemuatBarisBermakna` |
| Batas `LAB-DEC-036` | Nol atribut operasional laboratorium pada `MstProcedure` | `PASS` | `MstProcedure_TidakBertambahAtributOperasionalLaboratorium` |
| Penyaringan katalog per disiplin | Empat tindakan digolongkan; penyaring Mikrobiologi mengembalikan tepat satu; satu tindakan non-lab tetap tanpa disiplin | `PASS` | `JenisPemeriksaan_DapatDigolongkanKeDisiplinnya` |
| Migration maju, mundur, maju | `Done.` ketiganya; daftar migration bersih | `PASS` | `dotnet ef database update` terhadap `QuilvianNewDevYoga` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Pengisian nilai disiplin untuk katalog yang sudah ada | Butuh daftar penggolongan dari pihak laboratorium — lihat bagian 3.3 butir 1 |
| Penyaringan lewat endpoint katalog | Endpointnya adalah cakupan `BE-LAB-07`, yang penahannya baru dicabut task ini |

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kolom ada | **Terpenuhi** |
| Nilainya terisi | **Belum** — butuh penggolongan dari pihak klinis; lihat bagian 3.3 butir 1 |
| Penyaringan katalog per disiplin terbukti bekerja | **Terpenuhi** pada tingkat data dan kueri |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | Selama nilainya belum diisi, penyaringan katalog per disiplin akan mengembalikan daftar kosong pada lingkungan mana pun. Itu perilaku yang benar, tetapi mudah disalahartikan sebagai kerusakan |
| Risiko tersisa | **Rendah.** Kolom aditif dan boleh kosong; tidak ada jalur lama yang berubah |
| Perubahan sampingan | `NONE` untuk task ini. Dua perbaikan tata kelola dicatat pada laporan `BE-EXT-02` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. Meminta daftar penggolongan disiplin dari pihak laboratorium, lalu mengisinya. 2. `BE-LAB-07` — katalog laboratorium; penahannya sudah dicabut |
