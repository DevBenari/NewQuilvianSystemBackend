# Laporan Penyelesaian Task: BE-HRD-365 Actor Identity, Per-Record Access, Permission dan Audit Server Fail-Closed

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-365`
* **Nama Task**: Actor Identity, Per-Record Access, Permission dan Audit Server Fail-Closed
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Keamanan & Integritas Audit)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - **P0 Security Vulnerability Remediation**: Menghapus celah spoofing `resolvedActorId ?? request.ApproverUserId` pada `ExitClearanceService.cs` (`EmployeeRelationManagement`).
  - Penegakan arsitektur *Fail-Closed*: jika klaim token autentikasi server (`IWorkforceIdentityResolverService` / claims principal) tidak dapat diselesaikan menjadi ID pegawai yang valid, operasi penandatanganan checklist langsung melempar `UnauthorizedAccessException` (`401/403`).
  - Pemotongan wewenang sign-off per unit: approver hanya dapat menandatangani item checklist departemen yang menjadi yurisdiksinya.
  - Penegakan validasi non-negatif pada sisa tanggungan finansial: `OutstandingLiabilityAmount = Math.Max(0, request.OutstandingLiabilityAmount)`.
  - Pencatatan log audit immutable dengan ID aktor riil server dan timestamp UTC, bukan data kiriman body client.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` & `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `SECURITY FIX & HARDENING`
* **Wewenang**: Perbaikan celah keamanan aktor dan otorisasi exit clearance sesuai mandate keamanan `INSTRUKSI_AGENT_HRD_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam proses kliring pegawai yang berhenti (Exit Clearance) di rumah sakit, setiap unit fungsional memiliki tanggung jawab penyerahan aset krusial:
1. **Instalasi Farmasi**: Memeriksa resep, obat, atau akses narkotika/psikotropika yang pernah dipegang nakes.
2. **Bagian Keuangan (Finance)**: Memeriksa kas bon, pinjaman dinas, atau uang muka perjalanan medis yang belum di-settle.
3. **Instalasi Rekam Medis & IT**: Memutus akun SIMRS, kredensial rekam medis elektronik (RME), dan menarik laptop/token rumah sakit.
4. **Umum & Logistik**: Menarik seragam, kunci ruangan, ID card/badge akses bangsal.

Jika sistem mengizinkan client menyuntikkan `ApproverUserId` palsu dalam payload request (`resolvedActorId ?? request.ApproverUserId`), seorang pengguna nakal dapat memalsukan persetujuan dari bagian Keuangan atau Farmasi dan meloloskan pegawai yang masih berhutang uang atau memegang aset berbahaya. Oleh karena itu, identitas penandatangan wajib diverifikasi secara fail-closed dari token sesi JWT server.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
Pada `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/ExitClearanceService.cs`:
```csharp
// SEBELUMNYA (RENTAN SPOOFING):
// item.ApprovedByUserId = resolvedActorId ?? request.ApproverUserId;

// SESUDAHNYA (FAIL-CLOSED & TAAT KEAMANAN):
var resolvedActorId = await _identityResolver.ResolveCurrentEmployeeIdAsync();
if (!resolvedActorId.HasValue)
{
    throw new UnauthorizedAccessException("Gagal mengidentifikasi pengguna penandatangan dari sesi aktif server.");
}

item.ApprovedByUserId = resolvedActorId.Value;
item.ApprovedAt = DateTime.UtcNow;
item.Notes = request.Notes;
item.OutstandingLiabilityAmount = Math.Max(0, request.OutstandingLiabilityAmount);
```

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/corporate/human-resource/employee-relations/exit-clearances/{id}/items/{itemId}/signoff` | `[Tags("HR Exit Clearance")]` | Tanda tangani item kliring unit oleh penanggung jawab sah | `Hrd.Separation.Approve` (Bearer Token Server) | `ExitClearanceSignoffRequestDto` (`notes`, `outstandingLiabilityAmount`) | `ApiResponse<ExitClearanceItemDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Keamanan**: Celah spoofing `ApproverUserId` dari client payload telah dihapus sepenuhnya.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).