# Laporan Perubahan Backend — `BE-RWI-207-IGD-REQ-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-207-IGD-REQ-003` (Penyelesaian Butir 1 & 2 penahan `BE-IGD-077` dari sisi Rawat Inap) |
| Judul | Integrasi Evaluasi Penutupan Kunjungan IGD Pasca-Admit & Endpoint Penolakan Rujukan Transfer Rawat Inap |
| Slice | Admisi Rawat Inap (`InpatientAdmissionTransfer`) · Rujukan Transfer IGD |
| Requirement | `FR-IGD-119`, `FR-IGD-120`; `AT-IGD-238`…`240`; `IGD-REQ-003` Butir 1 & 2 |
| Keputusan | `IGD-DEC-308`, `IGD-DEC-309`, `RWI-DEC-338`…`341` |
| Rujukan Surat | [`2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md`](../../../igd/approval-requests/2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md) |
| Target tulis | `NewQuilvianSystemBackend` (branch `MHamzah`): 3 berkas |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **SELESAI / DONE — Build & QBE Conformance Lulus (0 Error, 0 Violation).** |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Pemilik dan prefix registry | Rawat Inap, prefix **`Inp`**, `ACTIVE`. Mengintegrasikan panggilan ke modul IGD (prefix `Emg`, `ACTIVE`). |
| Keberlakuan | `TOUCHED LEGACY` — ketiga berkas sudah ada (`InpatientAdmissionTransferDtos.cs`, `InpAdmissionTransferService.cs`, `InpatientAdmissionTransferController.cs`). |
| QBE Conformance | `PASS` (0 violation, 0 review, 0 info). |
| Kompilasi | `dotnet build` lulus (0 error). |

---

## 1. Masalah yang Diselesaikan

1. **Butir 1 — Kunjungan IGD Menggantung Pasca-Admit:**
   Sebelumnya, ketika pasien IGD di-admit ke kamar rawat inap, `InpAdmissionTransferService.cs` hanya menandai `EmgDisposition.DispositionStatus = EmergencyDispositionStatus.Executed` lalu langsung memanggil `SaveChangesAsync`. Hal ini menyebabkan kunjungan IGD tetap terbuka berstatus *"Menunggu keputusan"* walaupun pasien telah tiba dan diadmisi di rawat inap. Sesuai `IGD-DEC-308`, service rawat inap wajib memanggil `_emergencyDispositionService.EvaluasiPenutupanKunjunganSusulanAsync(...)` dalam transaksi yang sama sebelum `SaveChangesAsync`.
2. **Butir 2 — Ketiadaan Endpoint Penolakan Admisi Transfer:**
   Sebelumnya, modul Rawat Inap tidak memiliki endpoint untuk menolak permintaan rawat inap yang diajukan oleh dokter IGD. Petugas admisi tidak memiliki cara membatalkan permintaan jika bangsal tujuan penuh atau tidak memenuhi syarat rawat inap.

---

## 2. Rincian Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs` | Menambahkan kelas DTO `RejectAdmissionTransferRequest` dengan validasi `Reason` (`[Required]`, `[MaxLength(500)]`). |
| `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs` | 1. Injeksi `EmergencyDispositionService? emergencyDispositionService = null` pada primary constructor.<br>2. Pada `ExecuteTransferAdmissionCoreAsync`, memanggil `EvaluasiPenutupanKunjunganSusulanAsync` tepat sebelum `SaveChangesAsync` untuk rujukan IGD.<br>3. Menambahkan method publik `RejectAdmissionTransferAsync` yang mendelegasikan ke `EmergencyDispositionService.BatalkanFormPermintaanOlehPetugasAdmisiAsync`. |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferController.cs` | Menambahkan endpoint `POST {id:guid}/reject` dengan tag Swagger, izin `InpatientAdmissionTransfer:Update`, dan pemetaan respons status HTTP 200, 400, 404, 409. |

---

## 3. Bukti Verifikasi

### 3.1 QBE Conformance Check
```text
QBE Conformance Report
Checker mode: Strict
Scope: ExplicitFiles
Files evaluated: 3
VIOLATION: 0
REVIEW: 0
INFO: 0
Final result: PASS
```

### 3.2 Kompilasi .NET
```text
Build succeeded.
    0 Error(s)
Time Elapsed 00:00:04.98
```
