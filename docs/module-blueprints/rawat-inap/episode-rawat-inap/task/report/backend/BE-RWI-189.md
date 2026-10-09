# Laporan Perubahan Backend — `BE-RWI-189`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-189` |
| Judul | Bacaan Clinical: konteks penjamin, surat pengantar, alergi aktif |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-189` |
| Trace | `FR-RWA-003`, `051`, `070`; `RWI-DEC-253`, `254`, `262`, `264`; `RWI-FACT-066`, `067`; backend 13.6, 13.8.4; `INT-RWA-03` s.d. `05` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — konteks penjamin dipakai perhitungan harga; isian hanya ditambah |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/{Services,Controllers}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API dan pembacaan data samaran **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Prefix registry | `Cli` — `ACTIVE`; tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` (`EncounterInsuranceService`, `DoctorCertificateService`, `PatientAllergyController`), `NEW CODE` (`PatientAllergyQueryService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Label pasien butuh nomor kartu penjamin (`RWI-DEC-253`), IPD butuh diagnosis dan dokter dari surat
pengantar terbit (`RWI-DEC-254`), dan header ruang kerja butuh alergi aktif. Semuanya dibaca lewat
service Clinical, bukan query Rawat Inap.

## 2. Proses bisnis

1. `EncounterInsuranceContext` bertambah `CardNumber` dan `MemberNumber` dari snapshot sumber
   pembayaran, hanya pada cabang asuransi dan perusahaan; tunai kosong. Pemakai lama (perkiraan harga)
   tidak membaca isian ini, dan konteks tidak dikirim langsung lewat endpoint mana pun.
2. `DoctorCertificateService.GetLatestIssuedInpatientReferralAsync(encounterId)` mengembalikan surat
   `InpatientReferral` berstatus `Issued` terbaru: nomor, dokter (nama snapshot), tanggal terbit,
   diagnosis, alasan. Surat `Cancelled` diabaikan; tanpa surat → kosong. Tidak ada data pasien maupun
   gambar tanda tangan.
3. `PatientAllergyQueryService.GetActiveAlertsAsync` memuat query `active-alerts` yang sebelumnya ada di
   controller. Controller kini memanggil service dengan token pembatalan permintaan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EncounterInsuranceService.GetContextAsync` dan seluruh pemakainya, `DoctorCertificateService`,
`PatientAllergyController.GetActiveAlerts`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `ClinicalManagement/Services/EncounterInsuranceService.cs` | `CardNumber`, `MemberNumber` pada konteks |
| `ClinicalManagement/Services/DoctorCertificateService.cs` | `GetLatestIssuedInpatientReferralAsync`; kelas `InpatientReferralLetterSummary` |
| `ClinicalManagement/Services/PatientAllergyQueryService.cs` | Baru — `GetActiveAlertsAsync` (query sama dengan controller lama) |
| `ClinicalManagement/Controllers/PatientAllergyController.cs` | `active-alerts` memanggil service |
| `Program.cs` | `AddScoped<PatientAllergyQueryService>` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru; `GET patient-allergies/active-alerts` tetap bentuknya |
| Database | Tidak ada |
| Keamanan/Auth | Tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Allergy

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/patient-allergies/active-alerts` | Query pindah ke `PatientAllergyQueryService`; respons tetap | `PatientAllergy : Read` (tetap) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Controller tanpa query; service pemilik | `PASS` | Review source |
| Pemakai `EncounterInsuranceContext` | Tidak ada endpoint yang mengirim konteks utuh; isian aditif | `PASS` | Pencarian pemakai |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| API dan pembacaan data samaran | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Asuransi/perusahaan memuat nomor kartu dan peserta; tunai kosong; pemakai lama tidak berubah | Terpenuhi (source) | Dua cabang konteks; isian tidak dibaca perhitungan harga |
| 2. Surat `Issued` terbaru; `Cancelled` diabaikan; tanpa surat → kosong; tanpa data pasien/gambar | Terpenuhi (source) | `GetLatestIssuedInpatientReferralAsync` |
| 3. `active-alerts` sama sebelum dan sesudah | Terpenuhi (review) | Query dipindah utuh; uji runtime dikecualikan |
| 4. Tidak ada endpoint baru | Terpenuhi | Diff |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `IssuedDate` surat disimpan tengah malam UTC; pemakai memformat sebagai tanggal saja |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` `PatientAllergyQueryService.cs`; `M` `EncounterInsuranceService.cs`, `DoctorCertificateService.cs`, `PatientAllergyController.cs`, `Program.cs`. `InsuranceCoverageService.cs` juga `M` (rename variabel lokal pukul 11.08), **bukan** dari task ini |
| Langkah berikutnya | Uji pengguna: pasien asuransi, tunai, kunjungan dengan surat `Issued` dan `Cancelled` |
