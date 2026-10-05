# Laporan Perubahan Backend — `BE-RWI-154`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-154` |
| Judul | Koreksi salah catat penempatan |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-154` |
| Trace | `FR-RWF-019`; `RWI-DEC-157`, `192` (g); `RSK-RWF-02`; `INV-RWF-07`; `INT-RWF-03`; `UAT-RWF-25`; API 3.4; state 5.4; `VAL-RWF-07` s.d. `12` |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-151` ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `BillingCalculationService.cs`, `InpatientClearanceService.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`; bacaan Billing lewat adapter |
| Prefix registry | `Inp`, `Bil` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (correction service, adapter); `TOUCHED LEGACY` (`InpBedOccupancyService`, controller, `BillingCalculationService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Salah catat kamar, bed, kelas, atau waktu penempatan tidak dapat diperbaiki tanpa memalsukan
transfer, dan penanda `IsSuperseded` dipakai bersama transfer dan koreksi sehingga tarif kamar
berisiko dobel atau hilang.

## 2. Proses bisnis

1. Pemegang `InpatientBedOccupancy : Correct` mengirim koreksi dengan alasan wajib, minimal satu
   field (bed, kelas, waktu mulai/selesai), dan `ExpectedVersion`.
2. Status invoice dibaca langsung dari Billing lewat `IInpBillingClearanceAdapter`. Tak terbaca →
   422 `INP-COR-002`; bukan `OPEN`/`NONE` → 422 `INP-COR-001` "tagihan sudah difinalkan".
3. Koreksi membuat baris baru (`CorrectsPlacementId`), baris lama berakhir `CorrectedEntry` dan
   diberi `SupersededByCorrectionId`. Versi berubah → 409 `INP-COR-003`; bed tujuan tidak layak →
   422 `INP-COR-004`. Event `OCCUPANCY_CORRECTED` diantrekan.
4. Transfer biasa kini menerbitkan `BED_OCCUPIED`; `OCCUPANCY_CORRECTED` hanya milik koreksi.
5. Billing mengecualikan baris yang `SupersededByCorrectionId`-nya terisi; `IsSuperseded` (diakhiri
   transfer) **tidak** dipakai sebagai saringan.
6. Contoh `RSK-RWF-02` (hitungan dari source): Budi kelas 2 sejak 1 Okt 08.00, pindah ke kamar
   kelas 1 pada 3 Okt 08.00, lalu kepala ruangan mengoreksi baris kelas 1 menjadi VIP. Hasil:
   1–3 Okt dihitung kelas 2 (2 hari, baris diakhiri transfer tetap tertagih), 3 Okt dan seterusnya
   dihitung VIP; baris kelas 1 tersimpan sebagai jejak tetapi tidak tertagih — tidak dobel, tidak
   kurang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedOccupancyService.cs` (transfer, linimasa), `InpatientBedOccupancyController.cs`,
`BillingCalculationService.cs` (`CalculateRoomChargeAsync`), `InpatientClearanceService.cs`,
`InpBillingDepositAdapter.cs` (pola adapter), kontrak API 3.4.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Services/InpPlacementCorrectionService.cs` | Baru — validasi, gerbang invoice, kode `INP-COR-001` s.d. `004` |
| `InPatientManagement/Services/InpBedOccupancyService.cs` | `ApplyPlacementCorrectionAsync`; transfer menerbitkan `BED_OCCUPIED` |
| `InPatientManagement/Services/IInpBillingClearanceAdapter.cs`, `InpBillingClearanceAdapter.cs` | Baru — pembaca status kasir/invoice gagal-tertutup |
| `BillingManagement/Billing/Services/IInpatientClearanceService.cs`, `InpatientClearanceService.cs`, `Dtos/InpatientClearanceDtos.cs` | `GetLatestStatusAsync` → `InpatientClearanceStatusView` (status, kendala, `InvoiceStatus`) |
| `InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | `POST placements/{placementId}/corrections` |
| `InPatientManagement/DTOs/InpatientCorrectionDtos.cs`, `InpatientBedOccupancyDtos.cs` | `CorrectPlacementRequest`; `placements/by-episode` + `CorrectsPlacementId`, `SupersededByCorrectionId`, `IsCorrection` |
| `InPatientManagement/Enums/InpBedPlacementEndReason.cs` | `CorrectedEntry = 5` |
| `BillingManagement/Billing/Services/BillingCalculationService.cs` | Saringan `SupersededByCorrectionId == null` |
| `Program.cs` | Registrasi adapter dan correction service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru API 3.4; tiga field aditif pada `placements/by-episode` |
| Database | Memakai kolom `I1` |
| Keamanan/Auth | Permission baru `InpatientBedOccupancy : Correct` |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/inpatient-management/bed-occupancies/placements/{placementId}/corrections` | Koreksi salah catat selama invoice `OPEN`/belum terbentuk | `InpatientBedOccupancy : Correct` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review atribut `[AccessAction("Correct")]`/`[AccessPermission("InpatientBedOccupancy", "Correct")]` | Cocok | `PASS` | Controller |
| Review saringan tarif kamar | Hanya `SupersededByCorrectionId` | `PASS` | `CalculateRoomChargeAsync` |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Skenario `RSK-RWF-02` dengan data | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Invoice `FINAL` → 422 "tagihan sudah difinalkan" | Terpenuhi (source) | `INP-COR-001` |
| 2. Baris baru, baris lama ditandai, alasan wajib | Terpenuhi (source) | `ApplyPlacementCorrectionAsync`, validasi alasan |
| 3. Transfer dan koreksi dapat dibedakan | Terpenuhi (source) | `IsCorrection`, `EndReason` `Transfer` vs `CorrectedEntry` |
| 4. `RSK-RWF-02` tidak dobel, tidak kurang | Terpenuhi (source) | Contoh hitungan bagian 2 butir 6 |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Adapter status kasir dibuat di sini untuk gerbang invoice; cakupan penuh `BE-RWI-152` (endpoint `billing-status`, penghapusan `billing-details`) **belum** dikerjakan |
| Masalah yang diketahui | Invoice belum terbentuk (`NONE`) dianggap aman dikoreksi |
| Risiko tersisa | `IsSuperseded` tetap dipakai transfer; pembaca lain harus memakai `SupersededByCorrectionId` untuk koreksi |
| Perubahan sampingan | `InpatientClearanceService` bertambah `GetLatestStatusAsync` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??`. `InpBedOccupancyService.cs` kini juga diubah `BE-RWI-183` (belum di-commit) |
| Langkah berikutnya | Admin memberikan `InpatientBedOccupancy : Correct`; uji skenario `RSK-RWF-02` |
