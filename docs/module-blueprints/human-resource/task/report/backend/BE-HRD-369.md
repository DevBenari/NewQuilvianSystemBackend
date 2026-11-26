# Laporan Penyelesaian Task: BE-HRD-369 Akhir Kontrak: List/Filter Riil, Keputusan T16, Nonrenewal Handoff

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-369`
* **Nama Task**: Akhir Kontrak: List/Filter Riil, Keputusan T16, Nonrenewal Handoff
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 3: Akhir Kontrak)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penambahan parameter `ExpiringInDays` pada `EmploymentContractQuery` di `Areas/Corporate/HumanResource/LifecycleManagement/DTOs/EmploymentContractDtos.cs`.
  - Implementasi penyaringan horizon jatuh tempo kontrak kerja (30 hari, 60 hari, 90 hari) pada `HrdEmploymentContractService.GetPagedAsync` berbasis perhitungan tanggal `EndDate` di database PostgreSQL.
  - Dukungan evaluasi kepatuhan PKWT PP 35/2021 (batas kumulatif 5 tahun / batas perpanjangan) dari data riil.
  - Alur pencatatan keputusan perpanjangan/pengakhiran kontrak T16 yang persisten di database.
  - Integrasi otomatis pengalihan ke handoff pemisahan hanya jika keputusan adalah `Non-Renew` (tidak diperpanjang).

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `EXTENSION & FILTER QUERY`
* **Wewenang**: Penambahan kemampuan filter horizon kontrak dan pengelolaan keputusan T16.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Pengelolaan tenaga kerja kontrak (PKWT) seperti perawat kontrak, radiografer kontrak, atau staf administrasi penunjang memiliki tenggat waktu hukum yang ketat:
1. **Pemberitahuan Minimal 30 Hari Sebelum Berakhir**: Sesuai PP 35/2021, pemberitahuan kelanjutan atau pengakhiran kontrak wajib diberikan minimal 30 hari sebelum masa kontrak habis. HRD RS membutuhkan filter cepat untuk memantau kontrak yang habis dalam 30, 60, dan 90 hari ke depan.
2. **Kompensasi PKWT**: Kontrak yang berakhir dan tidak diperpanjang mewajibkan rumah sakit membayar uang kompensasi PKWT proporsional sesuai masa kerja.
3. **Pemisahan Jalur Keputusan (T16)**:
   - Jika diputuskan **Perpanjang (Renew)**: Kontrak baru dibuat, tidak ada proses exit clearance aset.
   - Jika diputuskan **Tidak Diperpanjang (Non-Renew)**: Otomatis dialihkan ke proses exit clearance untuk pengembalian inventaris medis dan penutupan akun.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
1. **`Areas/Corporate/HumanResource/LifecycleManagement/DTOs/EmploymentContractDtos.cs`**:
   ```csharp
   public class EmploymentContractQuery
   {
       public int? Page { get; set; }
       public int? PageSize { get; set; }
       public string? Status { get; set; }
       public int? ExpiringInDays { get; set; }
       public string? Search { get; set; }
   }
   ```
2. **`Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs`**:
   ```csharp
   if (query.ExpiringInDays.HasValue && query.ExpiringInDays.Value > 0)
   {
       var targetHorizon = today.AddDays(query.ExpiringInDays.Value);
       contractsQuery = contractsQuery.Where(c => c.EndDate >= today && c.EndDate <= targetHorizon);
   }
   ```

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload / Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/lifecycle/contracts` | `[Tags("HR Lifecycle - Contract")]` | Ambil daftar kontrak dengan filter horizon hari | `Hrd.Lifecycle.Contract.View` | `page`, `pageSize`, `expiringInDays`, `status` | `ApiResponse<PagedResult<EmploymentContractDto>>` |
| `POST` | `/api/corporate/human-resource/lifecycle/contracts/{id}/decide` | `[Tags("HR Lifecycle - Contract")]` | Rekam keputusan kelanjutan kontrak T16 | `Hrd.Lifecycle.Contract.Manage` | `ContractDecisionRequestDto` | `ApiResponse<EmploymentContractDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Filter Horizon**: Filter `expiringInDays=30`, `60`, `90` berhasil mengevaluasi rentang `EndDate` di EF Core.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).