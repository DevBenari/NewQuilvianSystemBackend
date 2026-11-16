# Laporan Penyelesaian Task: BE-HRD-338 Pensiun: Policy, Approval Gate, Handoff Aman dan Nomor Sah

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-338`
* **Nama Task**: Pensiun: Policy, Approval Gate, Handoff Aman dan Nomor Sah
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 2: Pensiun)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penegakan gerbang persetujuan (*Approval Gate*) pada `RetirementService.HandoffAsync`: pemanggilan handoff ke serah terima wajib menolak (`InvalidOperationException`) jika status kasus pensiun belum berstatus `Approved`.
  - Penghapusan nomor acak berisiko tabrakan `new Random().Next(...)` pada pembentukan nomor referensi pensiun, separation, dan clearance.
  - Perhitungan kandidat pensiun secara dinamis berbasis tanggal lahir dari profil master pegawai di database (menghilangkan batasan umur hardcoded 58 tahun di backend).
  - Pembentukan entitas `TrxEmployeeSeparation` dan `TrxOffboardingHandoverChecklist` secara atomik dan idempoten saat pensiun disetujui.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `BUSINESS RULE HARDENING & COMPLIANCE`
* **Wewenang**: Penguatan aturan bisnis proses pensiun pegawai rumah sakit.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Pensiun dokter senior, perawat utama, atau kepala instalasi di rumah sakit merupakan tonggak strategis:
1. **Approval Gate Mutlak Sebelum Handoff**: Kasus pensiun yang masih berstatus `Planned` atau `Draft` tidak boleh langsung dialihkan ke proses handoff atau pemutusan akses SIMRS. Kasus harus mendapatkan otorisasi Direksi/HRD terlebih dahulu agar penyesuaian hak purnabakti dan uang pensiun disetujui.
2. **Kandidat Pensiun Berbasis Database Nyata**: Batas usia pensiun dokter pendidik/spesialis seringkali berbeda dengan staf administrasi umum (misal 60 vs 58 tahun). backend mengomputasi kandidat dari tanggal lahir dan kategori jabatan yang tersimpan pada tabel master, bukan filter angka statis.
3. **Legalitas Dokumen Pensiun**: Nomor SK Pensiun harus rapi, berurutan, dan dapat dilacak oleh auditor pemerintah/BPJS Ketenagakerjaan.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
Pada `Areas/Corporate/HumanResource/LifecycleManagement/Services/RetirementService.cs`:
```csharp
// VALIDASI APPROVAL GATE PADA HANDOFF:
if (!string.Equals(retirement.RetirementStatus, "Approved", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"Kasus pensiun {retirement.RetirementCaseNumber} tidak dapat dialihkan ke serah terima karena berstatus '{retirement.RetirementStatus}'. Status wajib 'Approved' terlebih dahulu.");
}

// PENOMORAN AMAN:
var now = DateTime.UtcNow;
var caseNumber = $"RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 40)];
var separationNumber = $"SEP-RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 45)];
var clearanceNumber = $"EXC-RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 45)];
```

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload / Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/lifecycle/retirements` | `[Tags("HR Lifecycle - Retirement")]` | Ambil daftar kasus pensiun aktif | `Hrd.Lifecycle.Retirement.View` | `page`, `pageSize`, `status` | `ApiResponse<PagedResult<RetirementCaseDto>>` |
| `GET` | `/api/corporate/human-resource/lifecycle/retirements/eligible-candidates` | `[Tags("HR Lifecycle - Retirement")]` | Ambil kandidat pegawai yang memenuhi syarat pensiun | `Hrd.Lifecycle.Retirement.View` | - | `ApiResponse<List<RetirementEligibleCandidateDto>>` |
| `POST` | `/api/corporate/human-resource/lifecycle/retirements` | `[Tags("HR Lifecycle - Retirement")]` | Inisiasi pengajuan pensiun baru | `Hrd.Lifecycle.Retirement.Create` | `InitiateRetirementRequest` | `ApiResponse<RetirementCaseDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/retirements/{id}/approve` | `[Tags("HR Lifecycle - Retirement")]` | Berikan persetujuan resmi SK pensiun | `Hrd.Lifecycle.Retirement.Manage` | `ApproveRetirementRequest` | `ApiResponse<RetirementCaseDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/retirements/{id}/handoff` | `[Tags("HR Lifecycle - Retirement")]` | Alihkan ke serah terima (wajib status Approved) | `Hrd.Lifecycle.Retirement.Manage` | - | `ApiResponse<RetirementCaseDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Uji Guard Gate**: Handoff dari status selain `Approved` berhasil ditolak dengan exception deskriptif.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
