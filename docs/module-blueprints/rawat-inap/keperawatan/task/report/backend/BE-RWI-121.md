# Laporan Perubahan Backend — `BE-RWI-121`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-121` |
| Judul | Tanda vital per episode |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-121` |
| Trace | `FR-KEP-056`; `RWI-DEC-118`; kamus data 11.2; api-contract 7.4; `RLN3-CAP-12` |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-110` ✅ 17 September 2026 (kolom `InpEpisodeId` lahir di K3) |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — empat kriteria terpetakan; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tabel legacy `TrxPatientVitalSign` |
| Keberlakuan | `TOUCHED LEGACY` (controller) + `NEW CODE` (`InpatientVitalSignService`) |
| QBE relevan | `QBE-SVC-001` (endpoint baru tanpa `DbContext` di controller), `QBE-API-001`, `QBE-PAGE-001` |
| Database | Nol migration tambahan — kolom dan index `(InpEpisodeId, ObservationDateTime)` lahir di K3 |

## 1. Masalah yang diperbaiki

Tanda vital tidak menyimpan episode, sehingga perkembangan pasien sepanjang rawat inap hanya dapat dibaca
dengan menebak kunjungan dan antreannya — grafik perburukan tidak dapat disusun per perawatan.

## 2. Proses bisnis

1. Perawat mencatat tanda vital Budi → `POST /patient-vital-signs`; server mengisi `InpEpisodeId` dari episode
   berjalan kunjungannya (`BE-RWI-110`).
2. Menu Vital Sign dan grafik → `GET /patient-vital-signs/episodes/{episodeId}?from=&to=`.
   Tanpa rentang: 24 jam terakhir. Contoh: dibuka Rabu 10.00 → Selasa 10.00–Rabu 10.00, terurut waktu observasi.
3. Setiap titik membawa MAP dan EWS yang sudah dihitung server — layar tidak menghitung ulang. Baris dibatalkan
   atau salah catat tetap dikirim dengan `IsExcludedFromChart = true`.
4. **Jalur tidak normal:** rentang lebih dari 7 hari → `400`; awal setelah akhir → `400`; episode tidak ada → `404`.
5. Tanda vital lama tanpa episode tetap terbaca lewat `GET /`, `active-by-encounter`, dan `active-by-queue`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientVitalSignController.cs`, `PatientVitalSignDtos.cs`, kamus data 11.2, api-contract 7.4.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientVitalSignService.cs` | `GetSeriesAsync` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientVitalSignDtos.cs` | `PatientVitalSignSeriesItem` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` | `GET episodes/{episodeId}` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai 7.4 |
| Database | `NOT APPLICABLE` (kolom dari K3) |
| Keamanan/Auth | `PatientVitalSign : Read` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Deret tanda vital satu episode untuk tabel dan grafik | `PatientVitalSign : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–4 | Terpetakan | `PASS` | Bagian 6 |
| `dotnet build`, verifikasi skema | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `TrxPatientVitalSign` menyimpan episode | Terpenuhi | Kolom `InpEpisodeId` (K3); diisi `CreateVitalSign` |
| 2. Deret per episode terurut waktu | Terpenuhi | `GetSeriesAsync` — `OrderBy(ObservationDateTime)` |
| 3. Data grafik dari kontrak yang sama | Terpenuhi | MAP/EWS/kritis pada item deret |
| 4. Tanda vital lama tanpa episode tetap terbaca di jalur lama | Terpenuhi | Endpoint lama tidak diubah |
| DoD: `dotnet build`, verifikasi skema | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tanda vital rawat inap lama yang tercatat sebelum K3 tidak punya episode dan tidak muncul pada deret baru; pengisian mundur tidak dikerjakan karena tidak diminta kartu |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` controller dan DTO; service baru `??` |
| Langkah berikutnya | Frontend grafik tanda vital |
