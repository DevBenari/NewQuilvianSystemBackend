# Laporan Perubahan Backend — `BE-RWI-056`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-056` |
| Judul | Pengkajian awal dan pengkajian ulang tidak lagi saling menimpa |
| Slice | Gelombang `KEP-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-005`, `FR-KEP-006`; PRD 16.2 aturan 3; `AC-CAP012-02`; `VAL-KEP-08`, `VAL-KEP-11`; `UAT-KEP-06`, `UAT-KEP-07` |
| Contract version | State transition `0.3.0` bagian 1; validation `0.3.0` — `approved` 3 September 2026 lewat `RWI-DEC-092` |
| Dependency | `BE-RWI-054` — selesai, lihat laporannya |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah ≤ 3 (0), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), dampak schema/index/migration (2), keamanan tidak ada (0), workflow terbatas (1). Total **5** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, migration, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026 |
| Status | 🟡 **Sebagian.** Tiga dari empat acceptance criteria terbukti penuh; kriteria 4 terbukti sebagian. Rinciannya pada bagian 6 dan 7 |

---

## 1. Masalah yang diperbaiki

Pengkajian keperawatan tidak dikerjakan sekali lalu selesai. Perawat mengisi pengkajian awal saat
pasien masuk kamar, lalu mengisi pengkajian ulang setiap hari sesudahnya. Sebelum perubahan ini,
tidak ada satu pun aturan yang menjaga bahwa keduanya tersimpan sebagai catatan yang berbeda pada
satu perawatan — dan tidak ada yang menghalangi lahirnya **dua pengkajian awal** untuk satu pasien.

Akibatnya dua-duanya berbahaya:

1. **Riwayat menjadi kabur.** Dua pengkajian awal pada satu perawatan membuat pertanyaan "berapa
   nilai nyeri pasien saat ia baru masuk" punya dua jawaban.
2. **Perawat tidak diarahkan.** Ketika perawat keliru memilih "pengkajian awal" pada hari ketiga,
   sistem menerimanya diam-diam alih-alih mengarahkannya ke pengkajian ulang.

> **Contoh nyatanya.** Ns. Sari mengisi pengkajian awal Tn. Budi pada hari Senin dengan skala nyeri
> 7. Hari Selasa Ns. Dewi membuka layar yang sama dan — karena tidak diarahkan apa pun — memilih
> "pengkajian awal" lagi, lalu mengisi skala nyeri 3. Rekam medis kini memuat dua "pengkajian awal"
> dengan angka berbeda, dan tidak ada yang tahu mana yang benar-benar awal.

---

## 2. Proses bisnis

**Tujuan.** Satu perawatan memiliki tepat satu pengkajian awal yang hidup; sisanya adalah
pengkajian ulang. Nilai pengkajian awal tidak boleh berubah karena pengkajian berikutnya dibuat.

**Pelaku.** Perawat pelaksana dan kepala ruangan.

**Langkah yang berurutan.**

1. Perawat membuat pengkajian awal saat pasien baru masuk kamar. Tersimpan.
2. Esok harinya perawat membuat pengkajian ulang harian. Tersimpan sebagai **catatan tersendiri**;
   pengkajian awal tidak tersentuh.
3. Saat perawat hendak menyelesaikan pengkajian, sistem memeriksa bagian penilaian yang wajib
   terisi. Bila ada yang kosong, penolakannya menyebut bagiannya satu per satu.

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Perawat membuat pengkajian awal **kedua** pada perawatan yang sama | Ditolak: *"Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang."* | `409` |
| Pengkajian awal sebelumnya **dibatalkan**, lalu perawat membuat yang baru | **Diterima.** Pembatalan memang jalan keluar dari pengkajian yang salah | `200` |
| Menyelesaikan pengkajian rawat inap yang skrining gizinya belum diisi | Ditolak: *"Pengkajian belum dapat diselesaikan. Bagian berikut masih kosong: skrining gizi."* | `400` |
| Pengkajian poliklinik, medical check-up, atau IGD | **Tidak tersentuh aturan mana pun di atas** | — |

> **Kenapa pengkajian awal yang dibatalkan tidak menghalangi.** Perawat yang salah memilih pasien
> membatalkan pengkajiannya, lalu membuat yang benar. Bila baris yang dibatalkan ikut dihitung,
> perawatan itu tidak akan pernah bisa punya pengkajian awal lagi — dan pasiennya kehilangan
> dokumen yang paling menentukan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/state-transition-matrix.md` bagian 1 dan `contracts/validation-matrix.md` | Mesin status pengkajian, `VAL-KEP-08` dan `VAL-KEP-11` |
| `04-prd-to-mvp.md` bagian `EPIC KEP-02` beserta `UAT-KEP-06` dan `UAT-KEP-07` | Skenario yang wajib terbukti |
| `02-backend-architecture.md` bagian 4.1 | Pencabutan unique index |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Penjagaan pembuatan dan penyelesaian yang sudah ada, termasuk `ValidateMedicalAssessmentRuleAsync` sebagai pola |
| `Areas/HealthServices/ClinicalManagement/Enums/FallRiskStatus.cs`, `NutritionRiskStatus.cs` | Bentuk nilai "belum diisi" pada kedua penilaian risiko |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Index yang sudah ada, termasuk milik `BE-RWI-040` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `ValidateSingleInitialNursingAssessmentAsync` — penjaga satu pengkajian awal aktif per perawatan, dijawab `409`; `BagianPengkajianKeperawatanYangKosong` — pemeriksaan isian wajib saat penyelesaian, khusus pengkajian yang menempel pada perawatan rawat inap; konstanta kalimat `VAL-KEP-11` |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Index parsial **non-unique** `IX_TrxPatientAssessment_Episode_Type_Active` pada `(InpEpisodeId, AssessmentType)` dengan penyaring `"AssessmentType" = 0 AND "IsDelete" = false` |
| `Migrations/20260906122534_AddInitialNursingAssessmentPartialIndex.cs` beserta `.Designer.cs` | Migration index parsial; `Down` menghapusnya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentSeparationTests.cs` | **Berkas baru.** 7 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua sebab penolakan baru** pada endpoint yang sudah ada, keduanya hanya berlaku bagi pengkajian rawat inap: `409` pengkajian awal kedua, dan `400` isian wajib kosong. Nol endpoint baru, nol field berubah. Jalur poliklinik, medical check-up, dan IGD tidak menerima satu pun aturan baru |
| Database | Satu index parsial non-unique. Nol kolom, nol tabel. Migration `20260906122534_AddInitialNursingAssessmentPartialIndex` **dibuat, belum diterapkan** ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE` — nol atribut hak akses ditambah atau diubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. Kini menolak pengkajian awal kedua pada satu perawatan dengan `409` | `PatientAssessment : Create` |
| `PATCH` | `/{id}/complete` | Menyelesaikan pengkajian. Kini menolak `400` bila bagian penilaian wajib masih kosong pada pengkajian rawat inap | `PatientAssessment : Update` |

**Nol endpoint baru dibuat task ini.**

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| Skenario **dua pengkajian berurutan**: `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` | Dua baris; skala nyeri pengkajian pertama tetap 7 sebelum dan sesudah pengkajian kedua dibuat | `PASS` | Uji |
| Skenario **pengkajian awal kedua**: `PengkajianAwalKedua_Ditolak409DenganArahan` | `409` beserta kalimat `VAL-KEP-11` apa adanya; tetap satu baris | `PASS` | Uji |
| Skenario **pengkajian awal dibatalkan lalu diulang**: `PengkajianAwalDibatalkan_PengkajianAwalBerikutnyaDiterima` | Diterima; dua baris, satu di antaranya `Cancelled` | `PASS` | Uji |
| Skenario isian wajib kosong: `MenyelesaikanPengkajianTanpaSkriningGizi_Ditolak400` | `400`; pesannya menyebut "skrining gizi"; status tidak menjadi `Completed` | `PASS` | Uji |
| Regresi: `PoliklinikBerantre_TidakTersentuhAturanSatuPengkajianAwal` | Dua pengkajian awal pada dua kunjungan rawat jalan tetap diterima | `PASS` | Uji |
| Regresi: `PengkajianPoliklinik_TetapDapatDiselesaikanTanpaSkriningGizi` | `200`; status menjadi `Completed` seperti sebelumnya | `PASS` | Uji |
| Pemeriksaan **index memang parsial**: `IndexParsialPerawatanJenis_TerbentukDanNonUnique` | Index bernama benar, non-unique, penyaringnya menyebut `AssessmentType` dan `IsDelete`; index pencarian milik `BE-RWI-040` tetap ada tanpa penyaring | `PASS` | Uji |
| Penolakan penyelesaian karena **risiko jatuh** belum diisi (`UAT-KEP-07`) | Tidak dapat dibuktikan | `NOT FEASIBLE` | Perhitungan lama pada `CalculateFallRiskStatus` mengubah "belum diisi" menjadi `NoRisk`. Rinciannya pada bagian 7 |
| Uji migration maju dan mundur terhadap PostgreSQL sungguhan | Tidak dijalankan | `NOT RUN` | Wewenang eksekusi database tidak diberikan task ini |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pengkajian awal dan ulang tersimpan sebagai record terpisah; nilai record pertama sama persis sebelum dan sesudah record kedua dibuat | Terpenuhi | Uji `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` |
| 2. Pengkajian awal kedua pada satu perawatan ditolak `409` dengan pesan yang benar | Terpenuhi | Uji `PengkajianAwalKedua_Ditolak409DenganArahan`; kalimatnya dibandingkan persis dengan `VAL-KEP-11` |
| 3. Pengkajian awal yang dibatalkan tidak menghalangi pembuatan berikutnya | Terpenuhi | Uji `PengkajianAwalDibatalkan_PengkajianAwalBerikutnyaDiterima` |
| 4. Menyelesaikan pengkajian dengan isian wajib kosong ditolak `400` dan menyebut bagian yang kosong satu per satu | **Terpenuhi sebagian** | Mekanismenya ada dan terbukti untuk **skrining gizi** (uji `MenyelesaikanPengkajianTanpaSkriningGizi_Ditolak400`). Bagian **risiko jatuh** — yang justru disebut `UAT-KEP-07` — **tidak dapat ditegakkan** pada bentuk data hari ini. Lihat bagian 7 |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Aturan pembuatan pada service | Terpenuhi |
| Satu index parsial **non-unique** | Terpenuhi |
| Satu migration | Terpenuhi — `20260906122534_AddInitialNursingAssessmentPartialIndex` |
| Uji empat skenario lulus | **Tiga dari empat** — skenario "risiko jatuh belum terisi" tidak dapat dibuktikan; penggantinya skenario skrining gizi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Kekurangan yang menahan status** | `UAT-KEP-07` menuntut penyelesaian pengkajian ditolak ketika **risiko jatuh** belum terisi. Itu **tidak dapat ditegakkan hari ini**, dan sebabnya ada pada bentuk data yang sudah berjalan: `CreatePatientAssessmentRequest.HasFallRisk` bertipe `bool` yang bawaannya `false`, dan `CalculateFallRiskStatus` mengubah `false` menjadi `FallRiskStatus.NoRisk`. Perawat yang **tidak mengisi** bagian risiko jatuh karena itu tersimpan sebagai pernyataan klinis "tidak berisiko" — pernyataan yang tidak pernah ia buat. Membedakan keduanya menuntut `HasFallRisk` menjadi nullable, dan itu perubahan yang merusak kontrak permintaan bersama poliklinik, medical check-up, dan IGD. **Dicatat sebagai temuan, tidak diperbaiki tanpa wewenang** (`API_RULES.md`) |
| Keputusan yang diambil, beserta buktinya | Bagian yang ditegakkan dipilih dari bukti yang sudah disetujui, bukan dikarang: `UAT-KEP-07` menyebut risiko jatuh, dan `FR-KEP-007` menempatkan nyeri, risiko jatuh, serta gizi sebagai tiga pengukuran yang wajib terbaca sebagai perkembangan. Nyeri **sengaja tidak** ikut diperiksa karena `HasPain` selalu bernilai benar atau salah sehingga tidak pernah dapat "kosong"; memaksakannya akan menolak pengkajian pasien yang memang tidak nyeri |
| Butir terbuka | `VAL-KEP-08` berbunyi "isian wajib **menurut kebijakan aktif**", sedangkan `MstClinicalAssessmentPolicy` yang dibangun `BE-RWI-055` hanya menyimpan batas waktu. Daftar isian wajib yang dapat diatur admin **belum ada kontraknya**. Menambahkannya berarti mengarang kebijakan klinis, dan itu dilarang. Butir ini diangkat ke pemilik klinis lewat `/qv-grill` |
| Delta kontrak | `IX_TrxPatientAssessment_Episode_Type_Active` dibuat sebagai index **kedua** pada pasangan kolom yang sama, bukan mengubah index milik `BE-RWI-040`. Index lama dipakai `dokter-rawat-inap` untuk mencari kajian menurut jenisnya; menempelkan penyaring `AssessmentType = 0` padanya akan mematikan kegunaan itu |
| Risiko tersisa | Aturan satu pengkajian awal dijaga **service**, bukan unique index. Dua permintaan yang tiba benar-benar bersamaan karena itu masih dapat lolos berdua. Unique index sengaja tidak dipakai — `02-backend-architecture.md` `0.3` bagian 4.1 mencabutnya karena akan ikut menghitung baris yang dibatalkan. Bila kelak lomba itu terbukti terjadi, jalan keluarnya unique index **parsial yang juga menyaring status batal**, dan itu keputusan bentuk data yang perlu diputuskan terpisah |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini masih di working tree; **agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `BE-RWI-065` dan `BE-RWI-057` melanjutkan dari sini. Butir isian wajib yang dapat diatur admin dibawa ke `/qv-grill` bersama pemilik klinis |
