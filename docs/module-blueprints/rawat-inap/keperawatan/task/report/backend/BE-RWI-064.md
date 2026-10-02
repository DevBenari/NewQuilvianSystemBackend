# Laporan Perubahan Backend — `BE-RWI-064`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-064` |
| Judul | Kepala ruangan melihat pengkajian mana yang belum dikerjakan |
| Slice | Gelombang `KEP-MVP-4` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; PRD 16.2 aturan 11; `INV-KEP-03`; `VAL-KEP-18` |
| Contract version | API `0.3.0` — endpoint daftar pantau kepatuhan; lihat delta kontrak pada bagian 7 |
| Dependency | `BE-RWI-058` — selesai, lihat laporannya |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah ≤ 3 (0), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), hanya perilaku query yang sudah ada (1), keamanan tidak ada (0), workflow terbatas (1). Total **4** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. **Nol tabel baru dan migration kosong** |

---

## 1. Masalah yang diperbaiki

Kepala ruangan tidak punya cara menemukan pengkajian yang tertinggal selain membuka pasien satu per
satu. Pada ruangan berisi dua puluh pasien, itu berarti dua puluh kali membuka layar untuk mencari
satu atau dua yang belum dikerjakan.

`RWI-RULE-023` sudah menyebut daftar pantau ini sejak awal, dan `InpatientMonitoringController`
mencatatnya sendiri sebagai **gap yang disengaja** sejak `BE-RWI-029`:

> *"Daftar pantau ketiga pada `RWI-RULE-023` sengaja tidak ada. Kepatuhan pengkajian awal dan
> verifikasi CPPT bergantung pada slice dokumentasi klinis yang masih menunggu `DEC-INP-001`."*

Slice itu kini ada. Task ini menutup gap-nya.

---

## 2. Proses bisnis

**Tujuan.** Kepala ruangan menemukan perawatan yang pengkajian awalnya belum ada atau sudah lewat
tenggat, tanpa membuka pasien satu per satu — dan **tanpa** menahan pekerjaan siapa pun.

**Pelaku.** Kepala ruangan.

**Langkah yang berurutan.**

1. Kepala ruangan membuka daftar pantau kepatuhan pengkajian awal.
2. Sistem mengambil seluruh perawatan yang masih berjalan — `Admitted` dan `DischargePending`.
3. Untuk setiap perawatan, sistem mencari pengkajian awalnya yang masih hidup.
4. Perawatan yang pengkajian awalnya **sudah selesai tepat waktu** tidak ditampilkan; daftar ini
   memuat pekerjaan yang tertinggal, bukan seluruh pasien.
5. Sisanya ditampilkan beserta keadaan tenggat dan selisih keterlambatannya, terurut dari yang
   paling terlambat.

**Tiga keadaan kosong yang berbeda artinya.**

| Keadaan | Pesan | Yang harus dilakukan kepala ruangan |
| --- | --- | --- |
| Tidak ada yang tertinggal | *"Seluruh pengkajian awal sudah tepat waktu."* | Tidak ada |
| Belum ada kebijakan batas waktu | *"Batas waktu pengkajian belum ditetapkan."* | Meminta clinical governance mengisi masternya |
| — | **Tidak pernah** berbunyi "tidak ada data" | — |

> **Kenapa pembedaan ini penting.** Keduanya menghasilkan daftar kosong yang terlihat sama persis
> di layar, tetapi menuntut tindakan yang sama sekali berbeda: yang pertama berarti pekerjaan
> beres, yang kedua berarti pemantauannya sendiri belum menyala — `FR-KEP-025`.

**Yang daftar ini tidak lakukan.**

| Hal | Ketetapannya |
| --- | --- |
| Menahan pencatatan | **Tidak pernah.** Perawat tetap dapat mencatat pada perawatan yang muncul di sini — `INV-KEP-03`, `VAL-KEP-18` |
| Menampilkan isi klinis | **Tidak.** Hanya nama pasien, lokasi, dan keterlambatan |

> **Kenapa tidak boleh menahan.** Daftar pantau yang memblokir pekerjaan klinis akan mendorong
> perawat mengakali sistem — mengisi seadanya asal lolos. `INV-KEP-03` adalah penjaga keselamatan,
> bukan kenyamanan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | Pola lima daftar pantau yang sudah ada, beserta catatan gap ketiganya |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`, `InpBedPlacement.cs` | Konteks perawatan dan lokasi pasien |
| `rules/backend/transaction-endpoint-standard.md` bagian 2.4 | Bentuk permukaan monitoring read-only dan penamaan path-nya |
| `contracts/api-contract.md` bagian 1 | Daftar endpoint grup Patient Assessment |
| `04-prd-to-mvp.md` `EPIC KEP-05` | `FR-KEP-024` s.d. `FR-KEP-026` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentMonitoringService.cs` | `GetInitialAssessmentComplianceAsync` dan `JelaskanDaftarPantauAsync`; perhitungan tenggat pengkajian awal bagi perawatan yang **belum punya** barisnya, dihitung dari saat pasien masuk kamar |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingAssessmentMonitoringDtos.cs` | `InitialAssessmentComplianceItemResponse` — sengaja tanpa satu pun properti isi klinis |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Satu endpoint baca berhalaman beserta penyaring unit dan penyaring "hanya yang terlambat" |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentMonitoringTests.cs` | **Berkas baru**, dipakai bersama `BE-RWI-058`. Enam uji di antaranya milik task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baca baru** pada grup Patient Assessment. Nol endpoint lama berubah |
| Database | **`NOT APPLICABLE`.** Nol tabel, nol kolom, nol index. **Migration task ini kosong** |
| Keamanan/Auth | Endpoint memakai `PatientAssessment : Read` yang sudah ada. Nol Resource baru, nol action baru. Isi balasannya sengaja tidak memuat data klinis, sehingga hak baca daftar pantau tidak menjadi jalan pintas membaca rekam medis |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/monitoring/initial-assessment-compliance` | Daftar perawatan berjalan yang pengkajian awalnya belum ada atau sudah lewat tenggat | `PatientAssessment : Read` |

**Query parameter.**

| Nama | Jenis | Kegunaan |
| --- | --- | --- |
| `serviceUnitId` | `guid?` | Menyaring menurut unit perawatan. Kosong berarti seluruh unit |
| `onlyLate` | `boolean` | Benar berarti hanya yang sudah melewati tenggat |
| `pageNumber` | `integer` | Halaman keberapa; bawaan `1` |
| `pageSize` | `integer` | Banyak baris per halaman; bawaan `25`, batas `100` |

**Isi setiap baris.** Nomor perawatan, nama pasien, nomor rekam medis, unit, kamar dan tempat
tidur, saat masuk kamar, apakah pengkajian awalnya sudah ada, tenggat, saat selesai, keadaan
tenggat, dan selisih keterlambatan. **Nol isi klinis.**

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| `DaftarPantau_MemuatPerawatanTanpaPengkajianAwal` | Perawatan yang belum punya pengkajian awal muncul, beserta nama pasien dan nama unitnya | `PASS` | Uji |
| **Skenario dua perawatan, satu terlambat**: `DaftarPantau_MemuatYangTerlambatDanMenyembunyikanYangTepatWaktu` | Hanya yang terlambat muncul; keadaannya `Late`; selisihnya 560 menit | `PASS` | Uji |
| **Skenario daftar kosong dan skenario kebijakan kosong**: `DaftarPantauKosong_DibedakanMenurutSebabnya` | Tanpa baris terlambat berbunyi *"Seluruh pengkajian awal sudah tepat waktu."*; tanpa kebijakan berbunyi *"Batas waktu pengkajian belum ditetapkan."* | `PASS` | Uji |
| **Pemeriksaan keterlambatan tidak memblokir pencatatan**: `Keterlambatan_TidakMenahanPencatatanBerikutnya` | Pencatatan pada perawatan yang sedang terlambat dijawab `200`; perawatan itu tetap muncul pada daftar pantau | `PASS` | Uji |
| `DaftarPantau_TidakMemuatIsiKlinis` | Bentuk balasan tidak memiliki properti keluhan, nyeri, gizi, risiko jatuh, maupun catatan apa pun | `PASS` | Uji refleksi |
| `DaftarPantau_MenyaringMenurutUnitPerawatan` | Dua perawatan tanpa penyaring, satu perawatan dengan penyaring unit | `PASS` | Uji |
| Pemeriksaan migration task ini kosong | Nol berkas migration dibuat untuk task ini | `PASS` | Bagian 3.3 |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar memuat perawatan yang pengkajian awalnya belum ada atau terlambat menurut kebijakan aktif | Terpenuhi | Uji `DaftarPantau_MemuatPerawatanTanpaPengkajianAwal` dan `DaftarPantau_MemuatYangTerlambatDanMenyembunyikanYangTepatWaktu` |
| 2. Daftar kosong dibedakan dari kebijakan kosong; tidak pernah berbunyi "tidak ada data" | Terpenuhi | Uji `DaftarPantauKosong_DibedakanMenurutSebabnya` — kedua kalimatnya dibandingkan persis |
| 3. Keterlambatan pengkajian tidak menahan satu pun tindakan lain | Terpenuhi **dengan batas yang dijelaskan** | Uji `Keterlambatan_TidakMenahanPencatatanBerikutnya`. Tabel tindakan keperawatan baru lahir pada `BE-RWI-061`, sehingga yang dibuktikan hari ini adalah pencatatan dokumen keperawatan berikutnya. Lihat bagian 7 |
| 4. Daftar tidak menampilkan isi klinis; hanya nama pasien, lokasi, dan keterlambatan | Terpenuhi | Uji `DaftarPantau_TidakMemuatIsiKlinis` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Satu endpoint | Terpenuhi |
| Penyaringan | Terpenuhi — unit perawatan dan keadaan tenggat |
| Nol tabel baru | Terpenuhi |
| Keempat acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Delta kontrak — route endpoint** | `api-contract.md` `0.3.0` bagian 1 **tidak memuat baris** untuk daftar pantau ini, padahal kartu task merujuk padanya. Route yang dipakai adalah `GET /patient-assessments/monitoring/initial-assessment-compliance`, dipilih dari dua batasan yang mengikat sekaligus: roadmap bagian 0.2 mewajibkan seluruh task menulis di `Areas/HealthServices/ClinicalManagement/`, dan `transaction-endpoint-standard.md` bagian 2.4 mewajibkan nama path menyebut **kondisi yang dipantau**. Alternatifnya — menaruhnya pada `InpatientMonitoringController` bersama lima daftar pantau lain — melanggar batasan pertama. **Diangkat sebagai delta kepada pemilik kontrak** |
| Batas bukti kriteria 3 | `INV-KEP-03` berbunyi "pembuatan tindakan pada episode yang terlambat tetap dijawab `201`". Tabel tindakan keperawatan belum ada; ia lahir pada `BE-RWI-061`. Yang dapat dibuktikan hari ini adalah bahwa daftar pantau **tidak memiliki satu pun jalur** yang menahan pencatatan: ia murni membaca, dan pencatatan dokumen keperawatan berikutnya pada perawatan yang sedang terlambat tetap diterima. Ketika `BE-RWI-061` mendarat, uji `INV-KEP-03` yang sesungguhnya wajib ditambahkan di sana |
| Keputusan bentuk | Perawatan yang **belum punya** pengkajian awal tetap ikut walaupun tenggatnya belum lewat, karena justru itu yang dicari kepala ruangan: pekerjaan yang belum dimulai. Penyaring `onlyLate` mempersempitnya menjadi yang benar-benar terlambat |
| Keputusan bentuk | Tenggat bagi perawatan yang belum punya pengkajian awal dihitung dari **saat pasien masuk kamar**, bukan dari saat daftar dibuka. Perawatan yang masih `Draft` tidak punya `AdmittedAt`, sehingga tidak punya tenggat yang masuk akal — dan memang tidak ikut, karena daftar ini hanya memuat perawatan berjalan |
| Risiko tersisa | Penyaringan dan pengurutan dilakukan **di dalam aplikasi**, bukan di database, karena keadaan tenggat menggabungkan tenggat tersimpan dengan tenggat yang dihitung dari kebijakan. Untuk rumah sakit dengan sangat banyak perawatan berjalan sekaligus, pembacaan ini akan terasa berat. Bila kelak terbukti, jalan keluarnya penyaring unit yang wajib diisi, bukan mengubah bentuk jawabannya |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini sebagian sudah ikut tercommit pemilik repository pada `0a63358`; sisanya masih di working tree. **Agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `FE-RWI-056` membangun layarnya; urutan daftar di dalam `FE-INP-09` wajib diputuskan bersama pemilik `episode-rawat-inap` |
