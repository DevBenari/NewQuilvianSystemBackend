# Laporan Perubahan Backend — `BE-RWI-133`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-133` |
| Judul | Webhook Clearance Kasir & Auto-Reblock |
| Slice | Gelombang INT-BE-3 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-133` |
| Trace | `FR-INT-014`, `FR-INT-015`; `RWI-DEC-158`, `RWI-AC-238`; API §2, State §1 |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-132` |
| Klasifikasi | `HIGH` — Gerbang kepulangan berbasis webhook sinyal kasir dan penguncian instan Auto-Reblock |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/DTOs/`, `Areas/HealthServices/InPatientManagement/Services/`, `Areas/HealthServices/InPatientManagement/Controllers/`, `Program.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Endpoint webhook penerima clearance kasir dan mekanisme Auto-Reblock terpasang lengkap. |

---

## 1. Masalah yang Diselesaikan
Ketika kasir telah menerbitkan kelayakan pembayaran (*Clearance Approved*), sering kali muncul tagihan susulan mendadak (misalnya dari farmasi atau laboratorium yang baru menginput hasil). Jika perawat bangsal memulangkan pasien tanpa mengetahui penambahan tagihan ini, rumah sakit mengalami piutang macet (*loss revenue*). Diperlukan mekanisme sinyal balik otomatis (*Auto-Reblock*) yang seketika mengunci tombol kepulangan di bangsal saat kasir mencabut persetujuan (*Clearance Revoked*).

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **DTO Webhook `ClearanceSignalWebhookDto`**:
   - Dibuat di `Areas/HealthServices/InPatientManagement/DTOs/InpatientClearanceSignalDtos.cs`.
   - Menerima `EncounterId`, `Action`, `Reason`, `RevokedByCashierName`, dan `TimestampUtc`.
2. **Implementasi `HandleClearanceSignalAsync`**:
   - Pada `InpatientClearanceGateService`, menangani dua jenis sinyal utama dari kasir:
     - **`CLEARANCE_APPROVED`**: Mengubah `episode.ClearanceStatus = BillingClearanceStatus.Cleared`, membersihkan `ClearanceRevokedReason`, dan mengaktifkan izin kepulangan fisik di layar perawat.
     - **`CLEARANCE_REVOKED` (Auto-Reblock)**: Seketika mengubah `episode.ClearanceStatus = BillingClearanceStatus.Revoked`, menyimpan alasan pencabutan kasir pada `ClearanceRevokedReason`, dan mereset status override darurat untuk memastikan tombol pelepasan fisik pasien langsung terkunci kembali.
3. **Controller `InpatientDischargeClearanceController`**:
   - Swagger Tag: `[Tags("Inpatient Discharge Clearance")]`.
   - Endpoint webhook internal: `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/discharge-clearance/webhook`.
   - Menghubungkan pemanggilan dari sistem kasir ke `_clearanceGateService.HandleClearanceSignalAsync` dan mencatat jejak audit via `_loggerService`.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClearanceSignalDtos.cs` | Baru — Payload webhook sinyal kelayakan kasir |
| `Areas/HealthServices/InPatientManagement/Services/IInpatientClearanceGateService.cs` | Baru — Kontrak interface gerbang clearance |
| `Areas/HealthServices/InPatientManagement/Services/InpatientClearanceGateService.cs` | Baru — Implementasi logika webhook sinyal & Auto-Reblock |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs` | Baru — Controller Swagger untuk endpoint webhook |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Clearance Approved):** Sinyal disetujui kasir memperbarui status episode menjadi `Cleared` dan membuka izin pelepasan fisik.
- **AC-2 (Auto-Reblock Aktif):** Sinyal dicabut kasir mengubah status menjadi `Revoked`, mengunci tombol pulang seketika, dan menyimpan alasan penolakan.
- **AC-3 (Penyimpanan Alasan Kasir):** Alasan pencabutan (`Reason`) tersimpan presisi di database untuk ditampilkan ke perawat.
