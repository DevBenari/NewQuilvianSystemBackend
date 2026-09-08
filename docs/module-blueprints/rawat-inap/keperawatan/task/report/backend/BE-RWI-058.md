# Laporan Perubahan Backend — `BE-RWI-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-058` |
| Judul | Perkembangan nyeri, risiko jatuh, dan gizi terbaca sebagai satu garis waktu |
| Slice | Gelombang `KEP-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-007`, `FR-KEP-010`; PRD 16.2 aturan 6 dan 11; `AC-CAP012-02`, `AC-CAP012-04`; `VAL-KEP-17` |
| Contract version | API `0.3.0` `GET /episodes/{episodeId}/timeline` dan `GET /episodes/{episodeId}/due-status` |
| Dependency | `BE-RWI-055` selesai; `BE-RWI-056` 🟡 sebagian — tidak menahan task ini |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah 4–8 (1), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), hanya perilaku query yang sudah ada (1), keamanan tidak ada (0), workflow terbatas (1). Total **5** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Kelima acceptance criteria terbukti. **Nol tabel baru dan migration kosong** |

---

## 1. Masalah yang diperbaiki

Endpoint yang sudah ada mengembalikan **daftar** pengkajian satu perawatan. Daftar menjawab
"pengkajian apa saja yang ada"; ia tidak menjawab pertanyaan yang benar-benar dipakai perawat dan
DPJP di depan pasien:

> **Apakah nyeri pasien ini membaik atau memburuk?**

Nilai terakhir saja tidak cukup. Skala nyeri 5 berarti hal yang sangat berbeda bila kemarin 8
dibanding bila kemarin 2. Yang dibutuhkan adalah **seluruh** pengukuran, terurut waktu.

Masalah kedua: keadaan tenggat tidak terbaca sama sekali. Kolom `DueAt` sudah terisi sejak
`BE-RWI-054` dan `BE-RWI-055`, tetapi tidak ada satu pun endpoint yang menerjemahkannya menjadi
jawaban yang dapat dibaca manusia — apalagi yang **membedakan** "belum dipantau" dari "tepat waktu"
dan dari "terlambat".

---

## 2. Proses bisnis

**Tujuan.** Ruang kerja keperawatan menampilkan perkembangan pasien dan keadaan tenggatnya, tanpa
pernah mengubah penilaian pengkajian yang lalu.

**Pelaku.** Perawat pelaksana, kepala ruangan, dan DPJP — ketiganya membaca, tidak menulis.

**Langkah yang berurutan.**

1. Perawat membuka satu perawatan pada ruang kerja keperawatan.
2. Layar meminta lini masa pengkajian perawatan itu.
3. Sistem mengembalikan **seluruh** pengkajian keperawatan terurut waktu, ditambah tiga deret
   pengukuran: nyeri, risiko jatuh, dan skrining gizi.
4. Setiap baris membawa keadaan tenggatnya, dihitung dari tenggat yang **sudah tersimpan** pada
   pengkajian itu — bukan dari kebijakan yang berlaku hari ini.
5. Baris yang pernah dikoreksi membawa banyaknya koreksi beserta nomor urut terakhirnya; isi
   aslinya tetap tampil apa adanya.

**Empat keadaan tenggat, dan artinya bagi pengguna.**

| Keadaan | Artinya |
| --- | --- |
| `NotMonitored` | Belum ada kebijakan batas waktu yang berlaku. **Bukan** tepat waktu, dan **bukan** terlambat |
| `Pending` | Belum selesai, tenggatnya belum lewat |
| `OnTime` | Sudah selesai, dan selesainya tidak melewati tenggat |
| `Late` | Melewati tenggat: selesai terlambat, atau belum selesai sampai tenggat lewat |

> **Contoh berangka.** Batas pengkajian awal 1440 menit. Pengkajian Tn. Budi dibuat pukul 09.00
> tanggal 1, tenggatnya pukul 09.00 tanggal 2, dan ia selesai pukul 05.00 tanggal 2 — jadi
> `OnTime`. Bila esok harinya rumah sakit memperketat batasnya menjadi 480 menit, pengkajian Tn.
> Budi **tetap** `OnTime`, karena tenggat miliknya sudah tersimpan sejak ia dibuat.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Perawatan tidak ditemukan | Dijawab `404` |
| Master kebijakan kosong | Seluruh baris `NotMonitored`; pesannya *"Batas waktu pengkajian belum ditetapkan."* |
| Perawatan belum punya pengkajian sama sekali | Lini masa kosong; keadaan tenggat pengkajian awal tetap dihitung dari saat pasien masuk kamar |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` bagian 1 | Bentuk kedua endpoint |
| `contracts/validation-matrix.md` bagian 5 | `VAL-KEP-17` dan `VAL-KEP-18` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `GET /episodes/{episodeId}` sebagai acuan bentuk |
| `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs`, `MrcClinicalNoteAddendum.cs` | Sumber jumlah dan nomor urut koreksi |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` | Konteks perawatan dan `AdmittedAt` |
| `rules/backend/transaction-endpoint-standard.md` bagian 2.4 | Bentuk permukaan monitoring read-only |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/AssessmentDueState.cs` | **Baru.** Empat keadaan tenggat beserta alasan kenapa `NotMonitored` tidak boleh disamakan dengan `OnTime` |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingAssessmentMonitoringDtos.cs` | **Baru.** Bentuk lini masa, deret pengukuran, titik pengukuran, dan keadaan tenggat |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentMonitoringService.cs` | **Baru.** Pemilik seluruh pembacaan pemantauan; `GetTimelineAsync`, `GetDueStatusAsync`, dan `HitungKeadaan` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Dua endpoint baca baru yang meneruskan ke service itu |
| `Program.cs` | Pendaftaran `NursingAssessmentMonitoringService` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentMonitoringTests.cs` | **Berkas baru**, dipakai bersama `BE-RWI-064`. Sepuluh uji di antaranya milik task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baca baru** pada grup Patient Assessment. Nol endpoint lama berubah |
| Database | **`NOT APPLICABLE`.** Nol tabel, nol kolom, nol index. **Migration task ini kosong.** Seluruhnya membaca kolom yang sudah ada |
| Keamanan/Auth | Kedua endpoint memakai `PatientAssessment : Read` yang sudah ada. Nol Resource baru, nol action baru |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/timeline` | Lini masa pengkajian keperawatan satu perawatan: seluruh pengukuran nyeri, risiko jatuh, dan skrining gizi terurut waktu | `PatientAssessment : Read` |
| `GET` | `/episodes/{episodeId}/due-status` | Keadaan tenggat seluruh pengkajian keperawatan pada satu perawatan, beserta kalimat siap tampil | `PatientAssessment : Read` |

**Isi balasan lini masa.**

| Bagian | Isinya |
| --- | --- |
| `Entries` | Satu baris per pengkajian: nomor, jenis, waktu, status, tenggat, keadaan tenggat, selisih keterlambatan, penulis, jumlah koreksi, nomor koreksi terakhir, dan nilai ketiga pengukuran |
| `Series` | Tiga deret — `pain`, `fallRisk`, `nutrition` — masing-masing memuat **seluruh** titik pengukuran terurut waktu |
| `IsPolicyMasterEmpty` | Penanda supaya layar menulis "batas waktu belum ditetapkan", bukan "terlambat" |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| **Skenario tiga pengukuran nyeri berurutan**: `LiniMasa_MenampilkanSeluruhPengukuranTerurutWaktu` | Tiga titik dengan nilai `[8, 5, 2]` terurut naik menurut waktu; ketiganya tampil, bukan hanya yang terakhir | `PASS` | Uji |
| `LiniMasa_MemuatTigaDeretPengukuran` | Deret `pain`, `fallRisk`, dan `nutrition` seluruhnya ada | `PASS` | Uji |
| **Skenario kebijakan berubah di tengah**: `PenilaianPengkajianLama_TidakBerubahSaatKebijakanDiperketat` | Tetap `OnTime`; tenggat dan kode kebijakannya tidak bergerak sesudah kebijakan baru dibuat | `PASS` | Uji |
| `PengkajianSelesaiMelewatiTenggat_TerbacaTerlambatBesertaSelisihnya` | `Late`; selisih 120 menit; pesannya menyebut "melewati tenggat" | `PASS` | Uji |
| **Skenario master kosong**: `MasterKebijakanKosong_MenghasilkanTidakDipantau` | `NotMonitored`; pesannya *"Batas waktu pengkajian belum ditetapkan."*; `LateCount` nol | `PASS` | Uji |
| **Skenario pengkajian yang sudah dikoreksi**: `BarisYangPernahDikoreksi_MembawaNomorUrutDanIsiAslinyaTetapTampil` | Jumlah koreksi 1, nomor urut terakhir 1; skala nyeri asli tetap 3 pada baris maupun deret | `PASS` | Uji |
| `LiniMasaPerawatanTidakDikenal_Kosong` | Kosong, bukan galat; controller menerjemahkannya menjadi `404` | `PASS` | Uji |
| `HitungKeadaan_MengikutiUrutanYangMengikat` (3 jalur) | Tanpa tenggat → `NotMonitored`; selesai sebelum tenggat → `OnTime`; belum selesai dan tenggat belum lewat → `Pending` | `PASS` | Uji |
| Pemeriksaan migration task ini kosong | Nol berkas migration dibuat untuk task ini | `PASS` | Bagian 3.3 |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lini masa menampilkan seluruh pengukuran terurut waktu, bukan hanya yang terakhir | Terpenuhi | Uji `LiniMasa_MenampilkanSeluruhPengukuranTerurutWaktu` dan `LiniMasa_MemuatTigaDeretPengukuran` |
| 2. Nilai lama tidak pernah ditimpa | Terpenuhi | Uji yang sama membuktikan ketiga nilai tersimpan sebagai tiga baris berbeda; ditambah uji `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` milik `BE-RWI-056` |
| 3. Keadaan tenggat dihitung dari kebijakan yang aktif saat pengkajian dibuat; mengubah kebijakan tidak mengubah penilaian yang lalu | Terpenuhi | Uji `PenilaianPengkajianLama_TidakBerubahSaatKebijakanDiperketat` |
| 4. Master kebijakan kosong menghasilkan "tidak dipantau", bukan "terlambat" | Terpenuhi | Uji `MasterKebijakanKosong_MenghasilkanTidakDipantau` |
| 5. Baris yang pernah dikoreksi membawa nomor urut addendum-nya, dan isi aslinya tetap tampil | Terpenuhi | Uji `BarisYangPernahDikoreksi_MembawaNomorUrutDanIsiAslinyaTetapTampil` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Dua endpoint | Terpenuhi |
| Proyeksi lini masa tiga jenis pengukuran | Terpenuhi |
| Perhitungan tenggat berversi | Terpenuhi |
| Kelima acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Preflight QBE | Area `HealthServices`, Module `ClinicalManagement`, prefix `Cli`, Lifecycle `ACTIVE`. Keberlakuan `NEW CODE` untuk service dan DTO baru. `QBE-SVC-001` ditegakkan: seluruh pembacaan ketiga permukaan baru dimiliki `NursingAssessmentMonitoringService`, dan controller tidak menyentuh `ApplicationDbContext` untuknya. `QBE-DTO-001`, `QBE-PAGE-001`, dan `QBE-API-001` terpenuhi. Nol entity persisted dibuat, sehingga `QBE-MOD-002` dan `QBE-MOD-003` tidak berlaku |
| Keputusan bentuk | Keadaan tenggat **dihitung saat dibaca** dari `DueAt` yang sudah tersimpan, bukan dipersistensi sebagai kolom. Menyimpannya berarti menambah kolom yang harus dijaga tetap selaras dengan `CompletedAt`, dan `QBE-ENT-003` melarang kolom yang tidak menambah kebenaran |
| Delta kontrak | Kajian medis milik DPJP **sengaja tidak ikut** pada lini masa ini. Kontrak menempatkannya pada ruang kerja dokter (`CAP-022`); lini masa keperawatan menyaring empat jenis pengkajian keperawatan saja |
| Risiko tersisa | Lini masa mengambil seluruh pengkajian satu perawatan **tanpa halaman**. Untuk perawatan yang sangat panjang, balasannya ikut membesar. Kontrak `0.3.0` tidak meminta pagination pada endpoint ini, dan pemenggalan lini masa justru mematikan kegunaannya — perkembangan hanya terbaca bila seluruh titiknya tampil. Bila kelak terbukti berat, jalan keluarnya penyaring rentang tanggal, bukan halaman |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini sebagian sudah ikut tercommit pemilik repository pada `0a63358`; sisanya masih di working tree. **Agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `BE-RWI-064` memakai perhitungan yang sama untuk daftar pantau kepatuhan; `FE-RWI-053` membangun layarnya |
