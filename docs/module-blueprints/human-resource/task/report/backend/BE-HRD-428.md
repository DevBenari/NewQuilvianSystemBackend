# Laporan Perubahan Backend — `BE-HRD-428`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-428` |
| Judul | Sertifikasi kompetensi dan integritas verifikasi |
| Slice | `S-FIN-M09` / Sertifikasi Kompetensi (alias `FIN-HRD-09-BE-003`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-428` |
| Trace | M09-P0-03; instruksi Menu 09 `FIN-HRD-09-BE-003` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-426` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WfpCertificationController`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Create sudah memaksa `IsVerified=false` dan ada rute verify terpisah, tetapi `PUT` sertifikasi masih menyalin `IsVerified`/`VerificationStatus` dari form — pemilik dapat memverifikasi sendiri lewat edit biasa. PATCH verify juga bisa mengirim `IsVerified=false` dengan status `Verified`, dan syarat dokumen wajib hanya dicek bila body menyatakan terverifikasi.

## 2. Proses bisnis

1. Create → selalu `Pending`.
2. Edit (`PUT`) tidak dapat mengubah status verifikasi; perubahan data substantif (jenis, nama, penerbit, nomor, tanggal, seumur hidup, berkas) pada sertifikat terverifikasi mengembalikannya ke `Pending`.
3. Verifikasi (`PATCH …/verify`) oleh petugas lain: dokumen wajib harus ada (menurut master jenis), sertifikat kedaluwarsa tidak bisa valid, status `Verified` tidak boleh dipasangkan dengan `IsVerified=false`.
4. Berkas tersimpan di `App_Data` (privat) dan hanya diunduh lewat endpoint berizin.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpCertificationController.cs`, `WfpCertificationFileStorageService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/WfpCertificationController.cs` | PUT mengabaikan status verifikasi + reset bila data substantif berubah (`HasSubstantiveCertificationChange`); PATCH verify: dokumen wajib dari master, cegah status terbalik |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `IsVerified`/`VerificationStatus` pada form PUT diabaikan |
| Database | Tanpa migration |
| Keamanan/Auth | Self-verification lewat PUT tertutup |

## 4. Dokumentasi endpoint

#### Workforce Certification

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications/{id}` | Ubah data (tanpa status verifikasi) | `WorkforceCertification : Update` |
| `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications/{id}/verify` | Verifikasi/tolak | `WorkforceCertification : Update` |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications/{id}/file` | Unduh berkas privat | `WorkforceCertification : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: PUT `IsVerified=true` | Status tidak berubah | `PASS` | assignment dihapus |
| Penelusuran: verify tanpa berkas (master mewajibkan) | `400` | `PASS` | cek `RequiresDocument` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tidak ada sertifikat terverifikasi dari create atau update biasa | Terpenuhi | create `Pending`; PUT tidak menyentuh status |
| (2) Akses berkas terbatas | Terpenuhi | berkas di `App_Data`, diunduh lewat endpoint berizin |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak frontend | Form edit tidak lagi mengirim status verifikasi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
