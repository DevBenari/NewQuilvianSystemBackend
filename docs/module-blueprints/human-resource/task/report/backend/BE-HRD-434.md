# Laporan Perubahan Backend — `BE-HRD-434`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-434` |
| Judul | Co-signature terikat order dan emergency override |
| Slice | `S-FIN-M09` / Masa Berlaku Kewenangan (alias `FIN-HRD-09-BE-009`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-434` |
| Trace | M09-P0-10; instruksi Menu 09 `FIN-HRD-09-BE-009` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-433` ✅ |
| Klasifikasi | `MEDIUM` — sebagian, menunggu kontrak lintas modul |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `ClinicalPrivilegeGateService`, `ClinicalPrivilegeGateController`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | 🟡 Sebagian — penjagaan sisi HR ditambahkan; kontrak co-sign order-scoped dengan pemilik EMR belum ada (gerbang "Kontrak co-sign EMR") |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Co-sign menulis teks `OrderId`/`EncounterId` ke `WfpClinicalPrivilege.ApprovalNotes` dan **mengganti** `SupervisorUserId` kewenangan (perubahan permanen); co-sign tanpa order diterima; kewenangan yang tidak memerlukan supervisi atau sudah tidak berlaku tetap bisa di-co-sign; galat bisnis jatuh ke `500` karena controller hanya menangkap `KeyNotFoundException`.

## 2. Proses bisnis

1. Co-sign (`POST /api/v1/corporate/hrd/clinical-privileges/co-sign`) wajib menyebut `OrderId` dan `EncounterId`.
2. Kewenangan harus aktif, dalam masa berlaku (waktu server), tidak diblokir, dan berstatus supervisi.
3. Bila kewenangan mencatat supervisor penanggung jawab, hanya supervisor itu yang boleh co-sign (`403`).
4. Supervisor penanggung jawab tidak lagi diganti oleh co-sign.
5. **Belum:** catatan co-sign per order (aktor, batas waktu, idempotensi) di tabel milik modul klinis — menunggu kontrak EMR. Sementara jejak tetap ditambahkan pada `ApprovalNotes`.
6. Override darurat tetap tercatat terpisah ke KMKP dan tidak mengubah kewenangan permanen.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ClinicalPrivilegeGateService.cs` (`ProcessCoSignatureAsync`, `RecordLifeSavingOverrideAsync`), `ClinicalPrivilegeGateController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/ClinicalPrivilegeGateService.cs` | Wajib order+encounter; cek status/masa berlaku/supervisi; hanya supervisor tercatat; tidak mengganti supervisor |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/ClinicalPrivilegeGateController.cs` | Co-sign: `403`/`422` (sebelumnya `500`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | DTO tetap; `OrderId` dan `EncounterId` kini wajib secara bisnis (`422` bila kosong) |
| Database | Tanpa migration |
| Keamanan/Auth | Hak existing `ClinicalPrivilegeGate : CoSign` |

## 4. Dokumentasi endpoint

#### HR Credentialing - Clinical Privilege Gate

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/hrd/clinical-privileges/co-sign` | Co-signature supervisor | `ClinicalPrivilegeGate : CoSign` |
| `POST` | `/api/v1/corporate/hrd/clinical-privileges/emergency-override` | Override darurat | `ClinicalPrivilegeGate : EmergencyOverride` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: co-sign tanpa order | `422` | `PASS` | validasi baru |
| Penelusuran: supervisor lain | `403` | `PASS` | cek `SupervisorUserId` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Supervisor tidak bisa menyetujui order lain | Sebagian | co-sign wajib order dan supervisor tercatat; keterikatan ke data order nyata menunggu kontrak EMR |
| (2) Override tidak mengubah privilege permanen | Terpenuhi | override tercatat terpisah; co-sign tidak lagi mengganti supervisor |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Blocker | Kontrak co-sign order-scoped dengan pemilik modul EMR/Health Services (gerbang 28.0.1) |
| Pemilik | Pemilik modul EMR/Health Services |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
