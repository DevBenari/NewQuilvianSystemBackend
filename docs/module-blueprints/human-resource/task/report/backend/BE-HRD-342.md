# Laporan Penyelesaian Task: BE-HRD-342 Clearense: Canonical Owner, Signoff Per Unit, Kewajiban Finance, Lock Integration

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-342`
* **Nama Task**: Clearense: Canonical Owner, Signoff Per Unit, Kewajiban Finance, Lock Integration
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 6: Clearense)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penegakan model kepemilikan tunggal (*Canonical Owner*) untuk proses exit clearance pegawai keluar.
  - Sign-off bertahap per unit operasional rumah sakit (Atasan Langsung, IT/SIMRS, HRD/Kepegawaian, Fasilitas/Umum, dan Keuangan/Finance) dengan audit penandatangan berbasis server fail-closed.
  - Penegakan gembok kewajiban Finance (*Finance Liability Lock*): jika terdapat sisa tanggungan pinjaman/ganti rugi (`OutstandingLiabilityAmount > 0`), proses kliring tidak dapat diselesaikan secara final sampai diselesaikan atau diperhitungkan dalam pesangon.
  - Penutupan otomatis akses pengguna SIMRS dan pembukaan gembok pencairan payroll akhir hanya jika seluruh unit berstatus `Approved`.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` & `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `INTEGRATION & ACCESS CONTROL`
* **Wewenang**: Penguatan verifikasi matriks clearance lintas unit dan integrasi gembok payroll akhir.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Exit clearance (Clearense) adalah benteng pertahanan terakhir pengamanan aset dan data rekam medis rumah sakit:
1. **Penonaktifan Akses Rekam Medis Elektronik (RME)**: Dokter atau perawat yang sudah tidak aktif wajib dicabut hak aksesnya terhadap SIMRS dan berkas rekam medis pasien guna mematuhi UU Pelindungan Data Pribadi (UU PDP) dan Permenkes RME.
2. **Pengembalian Aset Medis & Fasilitas**: Stetoskop, tensimeter, kartu akses radiologi, kunci locker perawat, dan laptop RS wajib diserahkan ke bagian Umum/IT.
3. **Penyelesaian Kewajiban Keuangan**: Piutang kasbon, kelebihan klaim rawat jalan, atau uang muka operasional wajib diverifikasi oleh Finance. Status payroll akhir tetap terkunci rapat hingga exit clearance mendapatkan persetujuan penuh 100%.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
1. Pada `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/ExitClearanceService.cs`:
   - Validasi nilai sisa kewajiban keuangan `OutstandingLiabilityAmount = Math.Max(0, request.OutstandingLiabilityAmount)`.
   - Identitas penandatangan unit dikonfirmasi dari sesi JWT server aktif (bebas celah impersonasi).
2. Pada `Areas/Corporate/HumanResource/LifecycleManagement/Services/ExitClearanceService.cs`:
   - Penyelarasan paging query `query.PageNumber ?? query.Page` dan status filter kliring.

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload / Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/lifecycle/exit-clearance` | `[Tags("HR Lifecycle - Exit Clearance")]` | Ambil matriks exit clearance terpaginasi | `Hrd.Lifecycle.ExitClearance.View` | `page`, `pageNumber`, `pageSize`, `status` | `ApiResponse<PagedResult<ExitClearanceItemDto>>` |
| `GET` | `/api/corporate/human-resource/employee-relations/exit-clearances/{id}` | `[Tags("HR Exit Clearance")]` | Ambil rincian lengkap checklist 5 unit | `Hrd.Separation.View` | - | `ApiResponse<ExitClearanceDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/exit-clearances/{id}/items/{itemId}/signoff` | `[Tags("HR Exit Clearance")]` | Berikan tanda tangan persetujuan unit | `Hrd.Separation.Approve` | `ExitClearanceSignoffRequestDto` | `ApiResponse<ExitClearanceItemDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/exit-clearances/{id}/finalize` | `[Tags("HR Exit Clearance")]` | Finalisasi clearance setelah seluruh 5 unit approved | `Hrd.Separation.Manage` | - | `ApiResponse<ExitClearanceDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Gembok**: Finalisasi kliring terkunci hingga seluruh unit menandatangani dan tanggungan lunas.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
