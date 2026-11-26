# Laporan Penyelesaian Task: BE-HRD-365 Actor Identity, Per-Record Access, Permission dan Audit Server Fail-Closed

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-365`
* **Nama Task**: Actor Identity, Per-Record Access, Permission dan Audit Server Fail-Closed
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Keamanan & Integritas Audit)
* **Status**: `✅ Selesai (2026-11-20) — dikerjakan ulang sesudah diturunkan ke 🟡 pada 19 November 2026`
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

---

## Pengerjaan ulang 20 November 2026

| Field | Nilai |
| --- | --- |
| Pemicu | Audit source 19 November 2026 menurunkan task ini ke 🟡 (lihat kartu roadmap bagian 26.2) |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: kerjakan seluruh task backend roadmap HR) |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), perubahan belum di-commit |
| Model | Claude Opus 5.5 |
| Governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0; keberlakuan `TOUCHED LEGACY` + `NEW CODE` (penanda hak akses) |
| Catatan baris lama | Baris verifikasi pengerjaan 16 November 2026 di atas dipertahankan sebagai riwayat (`rules/backend/TEST_POLICY.md` bagian 6) |

### Masalah yang diperbaiki

Resolusi aktor fail-closed sudah benar sejak 16 November. Yang belum: (1) penanda tangan kliring tidak diperiksa haknya atas **departemen** item — petugas IT dapat menandatangani pengembalian obat Farmasi; (2) tugas serah terima dapat diserahkan siapa pun yang punya hak endpoint dan diverifikasi siapa pun, termasuk penyerahnya sendiri.

### Proses bisnis sesudah perbaikan

1. **Kliring lima departemen** (`PUT /api/v1/hrd/exit-clearance/checklists/{checklistId}/signoff`): aktor dari token; kliring yang sudah `Completed` ditolak `422`; penanda tangan wajib memegang hak departemen item (`MultiDeptExitClearance : SignoffWorkUnitHandover`, `SignoffGeneralAffairsAsset`, `SignoffPharmacyMedicalDevice`, `SignoffFinanceCooperative`, `SignoffInformationTechnology`) yang dicentang admin di Akses Role; item serah terima unit kerja juga boleh ditandatangani atasan utama aktif pegawai yang keluar. Selain itu `403` tanpa perubahan data.
2. **Serah terima** (`PUT …/handovers/{id}/tasks/{taskId}/submit`): hanya penanggung jawab tugas (`AssignedToUserId` atau profil `AssignedToWorkforceProfileId`); tugas tanpa penanggung jawab diserahkan pegawai yang keluar. Selain itu `403`.
3. **Verifikasi serah terima** (`…/verify`): penyerah tidak boleh memverifikasi; verifikator adalah atasan utama aktif pegawai yang keluar atau pemegang `OffboardingHandoverAdministration : VerifyAllTasks`. Selain itu `403`.

**Contoh.** Petugas IT (hanya hak `SignoffInformationTechnology`) menandatangani item `PharmacyMedicalDevice` → `403`, item tetap `Pending`.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `EmployeeRelationManagement/MultiDeptExitClearanceExplicitPermissions.cs` (baru) | Lima penanda hak per departemen |
| `EmployeeRelationManagement/Services/IExitClearanceService.cs`, `ExitClearanceService.cs` | `SignoffChecklistAsync` menerima `ClaimsPrincipal`; cek hak departemen, atasan langsung, status kliring |
| `EmployeeRelationManagement/Controllers/MultiDeptExitClearanceController.cs` | Meneruskan `User`; `UnauthorizedAccessException` → `403`, `InvalidOperationException` → `422` |
| `LifecycleManagement/LifecycleExitExplicitPermissions.cs` (baru) | Penanda `VerifyAllTasks` (+ penanda aspek untuk `BE-HRD-372`) |
| `LifecycleManagement/Services/OffboardingHandoverService.cs`, `Controllers/OffboardingHandoverController.cs` | Cek pemilik tugas saat serah, larangan verifikasi diri, atasan langsung/`VerifyAllTasks` saat verifikasi; `403`/`409` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Verifikasi actor gagal langsung menghasilkan `401/403` tanpa efek samping pada database | Terpenuhi | Aktor dari token, penolakan sebelum perubahan entity; controller memetakan `403` |
| Penandatanganan item unit lain ditolak | Terpenuhi | Hak per departemen + atasan untuk unit kerja; serah terima per pemilik/atasan |

### Catatan

| Hal | Isi |
| --- | --- |
| Peringatan untuk admin | Lima hak `Signoff…` dan `Verify All Handover Tasks` perlu dicentang untuk peran petugas departemen/HR; tanpa itu hanya atasan langsung (unit kerja/serah terima) dan SuperAdmin yang dapat menandatangani |
| Perubahan sampingan | `NONE` |

### Verifikasi pengerjaan ulang

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning baru dari build pertama — field penjaga belum diisi di konstruktor serah terima dan tag dokumentasi — diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas berkas slice Menu 12 | 17 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (skrip `dotnet fsi` di luar repository) | `IsValid=True`, 2.646 identitas (bertambah 11 penanda Menu 12); build pertama sempat ditolak validator karena penanda `ExitClearance : Clear…` menunjuk modul yang salah, diperbaiki ke `HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE` | `PASS` | Keluaran skrip |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal pengguna sedang berjalan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
