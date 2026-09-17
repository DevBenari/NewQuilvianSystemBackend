# Laporan Perubahan Backend — `BE-RWI-109`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-109` |
| Judul | Risiko jatuh memakai instrumen berversi (migration K2) |
| Slice | Gelombang 3 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-109` |
| Trace | `FR-KEP-043`, `FR-KEP-044`; mencabut `RWI-FACT-036`; `RWI-DEC-124`, `RWI-DEC-136`, **`RWI-DEC-152`**; `INT-KEP-16`; api-contract 0.5.0 bagian 7.0 dan 7.1; state matrix 5.2; `VAL-KEP-21a`–`c` |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-107` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — **perubahan perilaku** jalur klinis yang sudah berjalan |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — lima kriteria terpetakan; `dotnet build` **NOT RUN**; **regresi poliklinik NOT RUN** — keduanya atas keputusan pemilik 17 September 2026 |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` (`NursingAssessmentDocumentService`) + `TOUCHED LEGACY` (`PatientAssessmentController`) |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Pemberitahuan pemilik `rawat-jalan` | **Terpenuhi** — Sukma GP menyetujui `R7`, `R9`, `K2` pada 16 September 2026, `RWI-DEC-152` |
| Database | K2 tanpa perubahan bentuk data |

## 1. Masalah yang diperbaiki

Risiko jatuh rawat inap V2 dihitung dari dua centang (`HasAtaxia` +1, `HasPosturalInstability` +1) dengan
batas `≥ 2` Tinggi yang tertanam di controller (`RWI-FACT-036`). Pasien dengan riwayat jatuh dan infus —
skor Morse 45 — dapat tersimpan "tidak berisiko" karena kedua centang itu tidak menanyakannya.

## 2. Proses bisnis

1. Formulir Resiko Jatuh memanggil `GET /patient-assessments/instruments/resolve` dan menerima versi berlaku.
2. Ns. Siti menyimpan dokumen `AssessmentType = FallRisk` dengan `InstrumentResponses[]` →
   `NursingAssessmentDocumentService.ApplyDraftAsync`:
   - versi yang dikirim wajib versi yang berlaku bagi usia pasien, bila tidak `409` (`VAL-KEP-21c`);
   - server menghitung skor dan pita dari definisi versi: riwayat jatuh 25 + diagnosis sekunder 15 + infus 20
     = **60** → pita `[45, null)` "Tinggi" → `FallRiskStatus = HighRisk`, `FallRiskScore = 60`;
   - baris `CliAssessmentInstrumentResponse` menyimpan versi, hash definisi, jawaban, skor, pita.
3. Isian berskor yang belum dijawab → skor **kosong**, kategori `Unknown` — bukan 0/Rendah.
4. Menyelesaikan → versi `Approved` di produksi (`422` bila konsep), isian wajib versi, seluruh isian berskor
   terjawab. Setelah `Completed`, `PUT` ditolak sehingga **skor tidak dihitung ulang**.
5. **Poliklinik, medical check-up, IGD tidak berubah:** `CalculateFallRiskScore`/`CalculateFallRiskStatus` tetap
   dipakai jalur tanpa episode dengan hasil sama persis (tanpa penanda → `NoRisk`; skor 2 → `HighRisk`; 1 →
   `MediumRisk`; 0 → `LowRisk`).
6. Pada jalur rawat inap, `HasAtaxia` dan `HasPosturalInstability` diabaikan (api-contract 7.0).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientAssessmentController.cs` (perhitungan risiko jatuh, `BagianPengkajianKeperawatanYangKosong`,
`NormalizeAssessmentData`), `01-existing-capability-map.md` `RLN3-CAP-06`, `RLN3-CAP-20`, `RLN3-CON-01`, state matrix 5.2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentDocumentService.cs` | Baru — penerapan jawaban instrumen, skor server, penjaga penyelesaian |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Jalur rawat inap tidak lagi menghitung risiko jatuh dari angka di kode; `BagianPengkajianKeperawatanYangKosong` (daftar isian wajib tetap) **dihapus**; buat/ubah/selesaikan memanggil service; detail membawa `InstrumentResults`; `NormalizeAssessmentData` tidak merapikan kategori dokumen V2 |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | `InstrumentResponses` pada request; `InstrumentResults` pada response buat dan detail |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada payload; **perubahan perilaku** penyelesaian pengkajian keperawatan rawat inap (isian wajib dari versi instrumen, bukan daftar tetap risiko jatuh + gizi) |
| Database | `NOT APPLICABLE` (K2) |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menyimpan dokumen V2 beserta jawaban instrumen; skor dihitung server | `PatientAssessment : Create` |
| `PUT` | `/{id}` | Menyimpan ulang konsep; skor dihitung ulang dari versi berlaku | `PatientAssessment : Update` |
| `PATCH` | `/{id}/complete` | Menyelesaikan menurut versi instrumen | `PatientAssessment : Update` |
| `GET` | `/{id}` | Detail beserta `InstrumentResults` | `PatientAssessment : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pencarian angka batas risiko jatuh pada jalur rawat inap | Jalur rawat inap tidak memanggil `CalculateFallRiskScore`/`Status`; metode itu hanya di cabang `!pengkajianRawatInap` | `PASS` | `CalculateAssessmentValues` (dua overload) |
| Pencarian daftar isian wajib tetap | `BagianPengkajianKeperawatanYangKosong` tidak ada lagi di source | `PASS` | `grep` sesi ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| **Regresi poliklinik** (pengkajian antrean poli dengan centang ataksia) | Tidak dijalankan | `NOT RUN` | Kartu menyebut "tetap wajib dijalankan sungguhan" — **butir DoD ini belum terpenuhi** dan dikecualikan hanya atas keputusan pemilik 17 September 2026 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Skor dan pita dihitung server dan disimpan bersama versinya | Terpenuhi | `ApplyDraftAsync` → `ClinicalInstrumentDefinitionEngine.Score`; kolom `InstrumentVersionId`, `DefinitionHashSnapshot`, `TotalScore`, `BandCode` |
| 2. Skor tidak dihitung ulang setelah selesai | Terpenuhi | `UpdateAssessment` menolak `Completed`; `EnsureCanCompleteAsync` tidak menulis skor |
| 3. Source tanpa angka batas/skor butir risiko jatuh rawat inap dan tanpa daftar isian wajib tetap | Terpenuhi | Cabang rawat inap `fallRiskStatus = Unknown`; kategori dari `MappedFallRiskStatus` pita; daftar tetap dihapus |
| 4. Non-rawat-inap tetap memakai perhitungan lama tanpa perubahan hasil | Terpenuhi (statis) | `CalculateFallRiskStatus(hasFallRisk, score)` — tabel hasil pada bagian 2 butir 5 identik dengan sebelum task |
| 5. Pengkajian lama yang menyimpan versi tetap terbaca | Terpenuhi | `EnrichDetailAsync` membaca baris jawaban apa pun status versinya |
| DoD: laporan merujuk `RWI-DEC-152` | Terpenuhi | Preflight |
| DoD: regresi poliklinik hijau | **Belum dijalankan — dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Mundur dari K2 wajib dicatat sebagai **kejadian keselamatan** (kartu roadmap) |
| Masalah yang diketahui | Pengkajian `DailyReassessment` rawat inap (tidak dipakai jalur V2) kini tidak punya pemeriksaan isian wajib tetap |
| Risiko tersisa | Tanpa versi instrumen sah (`RWI-OQ-056`) Resiko Jatuh tidak dapat diselesaikan di produksi; layar lama yang masih mengirim dua centang akan melihat kategori `Unknown` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` controller dan DTO; service baru `??` |
| Langkah berikutnya | Regresi poliklinik oleh pemilik; frontend Resiko Jatuh memakai `resolve` |
