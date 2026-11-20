# Laporan Penyelesaian Task: BE-HRD-368 Pensiun: Policy, Approval Gate, Handoff Aman dan Nomor Sah

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-368`
* **Nama Task**: Pensiun: Policy, Approval Gate, Handoff Aman dan Nomor Sah
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 2: Pensiun)
* **Status**: `✅ Selesai (2026-11-20) — dikerjakan ulang sesudah diturunkan ke 🟡 pada 19 November 2026`
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

Gerbang `Approved` sebelum handoff sudah benar. Yang belum: (1) usia pensiun tertanam (`AddYears(-55)`, `minAge = 55`, `AddYears(58)`), padahal kebijakan usia pensiun adalah keputusan rumah sakit; (2) nomor `RET-`, `SEP-RET-`, `EXC-RET-` dari cap waktu + GUID, bukan number series.

### Proses bisnis sesudah perbaikan

1. HR menetapkan **usia pensiun normal** per rumah sakit pada Pengaturan Siklus Kepegawaian (`NormalRetirementAge`, 45–70 tahun). Tidak dikirim saat menyimpan pengaturan = nilai tersimpan dipertahankan.
2. Belum ditetapkan → `GET …/retirements/eligible-employees` membalas `409 POLICY_NOT_CONFIGURED`; ringkasan menampilkan `RetirementPolicyStatus = POLICY_NOT_CONFIGURED` dan hitungan mendekati pensiun 0. Kasus pensiun yang sudah ada tetap dapat dibuka, disetujui, dan di-handoff.
3. Sudah ditetapkan → kandidat dihitung dari master pegawai aktif: tanggal pensiun proyeksi = tanggal lahir + usia pensiun RS penempatan utamanya (bila penempatan tak diketahui dan hanya ada satu nilai kebijakan, nilai itu dipakai); kandidat tampil bila tanggal proyeksi ≤ hari ini + 3 tahun (rentang tampilan yang sama dengan perilaku lama). `minAge` kini penyaring opsional, bukan usia pensiun.
4. Membuat kasus: nomor `RET-YYYYMM-NNNN` dari number series; usia pensiun kasus = nilai isian petugas atau kebijakan RS bila kosong.
5. Handoff: nomor separation dari deret `SEP` bersama (`HRD_ER_SEPARATION`, `BE-HRD-366`) dan nomor clearance `EXC-YYYYMM-NNNN`.

**Contoh berangka.** Kebijakan RS 56 tahun. Perawat lahir 10 Mei 1971 → pensiun proyeksi 10 Mei 2027 (≤ 20 November 2029) → tampil sebagai kandidat, sisa 6 bulan.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `LifecycleManagement/Models/HrdEmploymentLifecycleSetting.cs`, konfigurasinya | Kolom `NormalRetirementAge` (nullable) |
| `Migrations/20261120090000_AddHrdRetirementPolicyAge.cs` (baru) | `ALTER TABLE … ADD COLUMN IF NOT EXISTS` + `CHECK 45–70`; ditulis manual, **belum dijalankan** |
| `LifecycleManagement/DTOs/HrdEmploymentLifecycleSettingDtos.cs`, `Services/HrdEmploymentLifecycleSettingService.cs` | Baca/simpan `NormalRetirementAge` |
| `LifecycleManagement/Services/RetirementService.cs` | Kebijakan dari pengaturan; `POLICY_NOT_CONFIGURED`; kandidat per RS; nomor dari `NumberSeriesAllocator` |
| `LifecycleManagement/DTOs/RetirementDtos.cs` | Ringkasan + `RetirementPolicyStatus`, `NormalRetirementAge` |
| `LifecycleManagement/Controllers/RetirementController.cs` | `minAge` opsional; `409 POLICY_NOT_CONFIGURED` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Handoff ditolak jika status belum `Approved` | Terpenuhi | Gerbang lama dipertahankan |
| Nomor kasus unik | Terpenuhi | `NumberSeriesAllocator` + index unik `RetirementNumber` |
| Kandidat pensiun dihitung dari master pegawai | Terpenuhi | `ComputeApproachingAsync` atas `MstEmployees` aktif dengan usia kebijakan RS |
| Persistensi transaksi utuh | Terpenuhi | Handoff tetap satu transaksi; nomor dari alokator boleh hangus bila transaksi batal |

### Catatan

| Hal | Isi |
| --- | --- |
| **Wajib sebelum dipakai** | Jalankan migration `20261120090000_AddHrdRetirementPolicyAge`. Tanpa kolom ini, query EF ke `HrdEmploymentLifecycleSetting` (pengaturan siklus, pembentukan masa percobaan) gagal `42703` |
| Masalah yang diketahui | `ResignationLifecycleHandoffService` masih memakai pembangkit nomor `SEP`/`EXC` lamanya (di luar kartu ini) |
| Perubahan sampingan | `NONE` |

### Verifikasi pengerjaan ulang

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning baru dari build pertama — field penjaga belum diisi di konstruktor serah terima dan tag dokumentasi — diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas berkas slice Menu 12 | 17 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (skrip `dotnet fsi` di luar repository) | `IsValid=True`, 2.646 identitas (bertambah 11 penanda Menu 12); build pertama sempat ditolak validator karena penanda `ExitClearance : Clear…` menunjuk modul yang salah, diperbaiki ke `HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE` | `PASS` | Keluaran skrip |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal pengguna sedang berjalan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
