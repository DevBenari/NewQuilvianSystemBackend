# Laporan Perubahan Backend — `BE-HRD-414`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-414` |
| Judul | Kontrak DTO uraian tugas, rantai atasan, dan jalur perubahan atasan mandiri |
| Slice | `S-FIN-M07` / Atasan Langsung (alias `FIN-HRD-07-BE-008`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-414` |
| Trace | `HRD-DEC-036`; instruksi Menu 07 `FIN-HRD-07-BE-008` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | `BE-HRD-410` ✅, `BE-HRD-413` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdPersonnelActionOrderService.cs`, `WorkforceCore/Controllers/WfpEmploymentHistoryController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

DTO uraian tugas (`jobDescription`, `versionHistory`) sudah konsisten. Yang belum: tidak ada jenis transaksi untuk perubahan atasan. Frontend terpaksa mengajukannya sebagai SK kategori `Rotation`, sehingga riwayat pegawai mencatat "Rotasi" padahal unit dan jabatannya tidak berubah — bertentangan dengan `HRD-DEC-036` (perubahan penetapan atasan adalah jenis transaksi tersendiri).

## 2. Proses bisnis

1. HR membuat SK kategori `ManagerChange` (Perubahan Atasan) dengan atasan baru dan tanggal efektif.
2. Validasi: atasan baru wajib; unit, departemen, dan jabatan target harus kosong (perubahan unit/jabatan tetap Mutasi/Rotasi/Promosi) → bila tidak, `400`.
3. Alur persetujuan sama dengan pola MVP `HRD-DEC-036`: scan wajib, pembuat ≠ penyetuju, cek siklus atasan, persetujuan tepat sekali (`BE-HRD-407`).
4. Saat berlaku, hanya penetapan atasan yang berubah; riwayat pegawai mencatat jenis `ManagerChange` berlabel "Perubahan atasan".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdPersonnelActionOrderService.cs` (metadata kategori, create/update draft, `ApplyOrderAssignmentsAsync`), `WfpEmploymentHistoryController.cs` (label jenis riwayat), DTO SK.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` | Kategori `ManagerChange` di metadata; `ValidateManagerChangeShape` pada create dan update draft |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpEmploymentHistoryController.cs` | Label riwayat "Perubahan atasan" |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nilai `ActionCategory` baru `ManagerChange` (aditif) |
| Database | Tanpa migration (`ActionCategory` teks) |
| Keamanan/Auth | Hak SK existing |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Personnel Action Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders` (`actionCategory = ManagerChange`) | Ajukan perubahan atasan | hak existing |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: SK `ManagerChange` tanpa atasan baru | `400` | `PASS` | `ValidateManagerChangeShape` |
| Penelusuran: SK `ManagerChange` dengan jabatan target | `400` | `PASS` | idem |
| Penelusuran: SK `ManagerChange` berlaku | Hanya atasan berubah; riwayat `ManagerChange` | `PASS` | `ApplyOrderAssignmentsAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kontrak JSON cocok dengan FE | Terpenuhi (backend) | DTO nested konsisten; frontend perlu memakai kategori `ManagerChange` |
| (2) Perubahan atasan tidak dicatat sebagai `Mutation`/`Rotation` | Terpenuhi | kategori dan jenis riwayat `ManagerChange` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak frontend | Form perubahan atasan perlu mengirim `actionCategory: "ManagerChange"` alih-alih `Rotation` (task frontend Menu 07) |
| Keputusan yang diambil | Perubahan atasan memakai mesin SK dengan jenis transaksi sendiri, sesuai pola persetujuan MVP `HRD-DEC-036`; definisi workflow terpisah di masa depan tetap dimungkinkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
