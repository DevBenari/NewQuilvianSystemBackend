# Laporan Task Backend — `RJ-DOC-REV-BE-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-020` |
| Judul | Validasi scan kartu asuransi |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Keputusan | `RJ-DOC-DEC-068`, `069`, `070`, `083` |
| Kontrak | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) — *Patient Insurance* |
| Task mode | `CROSS-REPO MODE` — backend |
| Branch / baseline | `sukmagp` @ `77caf434`, belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` / `MasterData` |
| Keberlakuan | `NEW CODE` (`InsuranceCardScanMatcher`, DTO `PatientInsuranceCardScanRequest`); `TOUCHED LEGACY` (`PatientInsuranceController`, `CreatePatientInsuranceRequest`, `Program.cs`) |
| QBE berlaku | QBE-SVC-001 (pembacaan asuransi dan aturan ada di service, bukan controller), QBE-VAL-001, QBE-DTO-001, QBE-API-001 |
| Temuan legacy | `PatientInsuranceController` memakai `ApplicationDbContext` langsung. Tidak dirapikan (di luar wewenang); kode baru tidak menambah akses context di controller |

## 2. Yang dikerjakan

- `InsuranceCardScanMatcher` (service): `ValidateAsync` membaca nama, kode, dan grup asuransi terpilih lalu menerapkan aturan `RJ-DOC-DEC-069`. `IsMatch`, `NormalizePolicy`, dan `NormalizeName` adalah fungsi murni.
- `CreatePatientInsuranceRequest.CardScan` opsional (`ScannedProviderName`, `ScannedPolicyNumber`).
- Create penjamin (`POST`, `/kiosk`, `/admin`) memanggil matcher sesudah validasi lama dan **sebelum** gambar kartu disimpan. Tidak cocok → `400 RJ-VAL-PM-01`, tidak lengkap → `400 RJ-VAL-PM-02`, tanpa `CardScan` → perilaku lama.
- Registrasi DI di `Program.cs`.

| Berkas | Status |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/InsuranceCardScanMatcher.cs` | Baru |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientInsuranceDtos.cs` | Diperbarui |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientInsuranceController.cs` | Diperbarui |
| `Program.cs` | Diperbarui — `AddScoped<InsuranceCardScanMatcher>` |

## 3. Delta

| Delta | Alasan |
| --- | --- |
| `UpdatePatientInsuranceRequest` mewarisi ruas `CardScan` tetapi update tidak memeriksanya | Keputusan membatasi pemeriksaan pada penjamin **baru** (`RJ-DOC-DEC-069`); ruas diabaikan pada update |
| Pesan diawali kode `RJ-VAL-PM-0x:` | Layar dan Kiosk dapat mengenali penolakan tanpa mencocokkan kalimat |

## 4. Verifikasi

| Perintah / skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release` ke scratchpad | 0 Error | `PASS` |
| EF | Tanpa perubahan model (tidak ada kolom baru) | `PASS` |
| QBE Strict | VIOLATION 0, REVIEW 0 | `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test | — |
| Runtime HTTP ke backend uji 7185 | **8/8 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| C1 | Prudential, polis `PMT-… 01`; scan `PT PRUDENTIAL INDONESIA LIFE` / `pmt…01` → `200` | PASS |
| C2 | Polis hasil scan beda → `400 RJ-VAL-PM-01` | PASS |
| C3 | Nama hasil scan `PT ASURANSI ALLIANZ LIFE` → `400 RJ-VAL-PM-01` | PASS |
| C4 | Scan tanpa No. polis → `400 RJ-VAL-PM-02` | PASS |
| C5 | Cocok lewat kode `INS-INHEALTH` → `200` | PASS |
| C6 | Tanpa `cardScan` → `200` (perilaku lama) | PASS |
| C7 | Penjamin yang ditolak tidak tersimpan (tepat 3 penjamin uji) | PASS |
| C8 | Jalur `/kiosk` dengan akun perangkat Kiosk → `400 RJ-VAL-PM-01` | PASS |

Data uji: tiga penjamin `PMT-…` milik pasien `00-00-00-15`, dihapus lewat `DELETE …/admin/{id}` (`200`).

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `RJ-AC-PM-01` cocok → `200` | Terpenuhi | C1, C5 |
| 2. `RJ-AC-PM-02` No. polis/nama beda → `400 RJ-VAL-PM-01`, tidak tersimpan | Terpenuhi | C2, C3, C7, C8 |
| 3. `cardScan` tidak lengkap → `RJ-VAL-PM-02` | Terpenuhi | C4 |
| 4. Tanpa `cardScan` perilaku lama identik | Terpenuhi | C6 |
| 5. `cardScan` tidak tersimpan | Terpenuhi — tidak ada kolom atau log yang menerimanya | Source |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration | Tidak ada |
| Risiko | Pencocokan sungguhan menunggu agent Plustek (`RJ-DOC-OQ-PM-01`) |
| Task berikutnya | `RJ-DOC-REV-FE-021`, `FE-KSK-016` |
| Perubahan sampingan | `NONE` |
