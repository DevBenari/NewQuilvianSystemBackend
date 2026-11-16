# Laporan Penyelesaian Task: BE-HRD-343 Pemeriksaan API/DB via Service, Concurrency, Keselarasan 6 Alur, Report & Retest

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-343`
* **Nama Task**: Pemeriksaan API/DB via Service, Concurrency, Keselarasan 6 Alur, Report & Retest
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Verifikasi Menyeluruh Backend Menu 12)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Audit komprehensif seluruh controller, service, dan DTO pada 6 Submenu Pemberhentian & Pensiun.
  - Verifikasi kompilasi `dotnet build QuilvianSystemBackend.csproj --no-restore` berhasil dengan **0 Error(s)**.
  - Verifikasi background runner backend `dotnet run --no-build` aktif dan listening pada `https://localhost:7184/api`.
  - Penyusunan *API-to-UI Contract Sheet* lengkap sebagai acuan penguncian kontrak bagi lapisan frontend (menghapus seluruh mock data dan fake toasts).
  - Pemutakhiran register status `backend-roadmap.md` menandai seluruh 10 task `BE-HRD-334` s.d. `BE-HRD-343` dengan status `✅ Selesai`.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` & `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx` / `Mst`
* **Klasifikasi Kode**: `END-TO-END VERIFICATION & AUDIT`
* **Wewenang**: Verifikasi kepatuhan arsitektur backend Menu 12 sesuai instruksi master.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Matriks Kontrak Resmi API-ke-Frontend (API-to-UI Contract Sheet)

### 3.1 Submenu 1: Pengunduran Diri (`/hr/separation/resignations`)
* `GET /api/corporate/human-resource/employee-relations/resignations?pageNumber=1&pageSize=10&status={status}`
  - Response: `ApiResponse<PagedResult<ResignationDto>>` (`items: []` saat zero data, bukan demo list).
* `POST /api/corporate/human-resource/employee-relations/resignations`
  - Payload: `{ employeeId: Guid, submissionDate: DateTime, requestedLastDate: DateTime, reasonCategory: string, reasonDetails: string }`
* `POST /api/corporate/human-resource/employee-relations/resignations/{id}/approve`
  - Payload: `{ notes: string, approvedLastDate: DateTime }`
* `POST /api/corporate/human-resource/employee-relations/resignations/{id}/reject`
  - Payload: `{ rejectionReason: string }`
* `POST /api/corporate/human-resource/employee-relations/resignations/{id}/handoff`
  - Trigger handoff ke proses pemisahan dan checklist serah terima.

### 3.2 Submenu 2: Pensiun (`/hr/separation/retirements`)
* `GET /api/corporate/human-resource/lifecycle/retirements?page=1&pageSize=10&status={status}`
  - Response: `ApiResponse<PagedResult<RetirementCaseDto>>` (`items: []` saat zero data).
* `GET /api/corporate/human-resource/lifecycle/retirements/eligible-candidates`
  - Response: `ApiResponse<List<RetirementEligibleCandidateDto>>` (dibaca dari tanggal lahir di master pegawai, bukan filter umur hardcoded).
* `POST /api/corporate/human-resource/lifecycle/retirements`
  - Payload: `{ employeeId: Guid, plannedRetirementDate: DateTime, notes: string }`
* `POST /api/corporate/human-resource/lifecycle/retirements/{id}/approve`
  - Payload: `{ notes: string, approvedRetirementDate: DateTime }`
* `POST /api/corporate/human-resource/lifecycle/retirements/{id}/handoff`
  - Memvalidasi status wajib `Approved` sebelum handoff diizinkan.

### 3.3 Submenu 3: Akhir Kontrak (`/hr/separation/contract-expiry`)
* `GET /api/corporate/human-resource/lifecycle/contracts?page=1&pageSize=10&expiringInDays={30|60|90}&status={status}`
  - Response: `ApiResponse<PagedResult<EmploymentContractDto>>` (menyaring kontrak yang berakhir dalam horizon N hari).
* `POST /api/corporate/human-resource/lifecycle/contracts/{id}/decide`
  - Payload: `{ decision: "Renew"|"Non-Renew", notes: string, effectiveDate: DateTime }`

### 3.4 Submenu 4: Pemberhentian (`/hr/separation/terminations`)
* `GET /api/corporate/human-resource/employee-relations/separations?pageNumber=1&pageSize=10&separationType=Termination`
  - Response: `ApiResponse<PagedResult<EmployeeSeparationDto>>` (`items: []` saat zero data).
* `POST /api/corporate/human-resource/employee-relations/separations`
  - Inisiasi kasus PHK dengan pencegahan duplikasi kasus aktif.
* `POST /api/corporate/human-resource/employee-relations/separations/calculate-severance`
  - Hitung simulasi pesangon authoritative dari engine backend (bukan rumus client).
* `POST /api/corporate/human-resource/employee-relations/separations/{id}/approve-finance`
  - Persetujuan resmi nominal pesangon oleh Finance.

### 3.5 Submenu 5: Serah Terima (`/hr/separation/handovers`)
* `GET /api/corporate/human-resource/lifecycle/handovers?page=1&pageSize=10&status={status}`
  - Response: `ApiResponse<PagedResult<OffboardingHandoverDto>>` (`items: []` saat zero data).
* `POST /api/corporate/human-resource/lifecycle/handovers/{id}/tasks`
  - Tambah butir tugas serah terima dengan kode tugas unik `TSK-yyyyMMddHHmmss-N`.
* `POST /api/corporate/human-resource/lifecycle/handovers/{id}/tasks/{taskId}/submit`
  - Penyerahan butir tugas (menolak submit ulang jika sudah Completed).
* `POST /api/corporate/human-resource/lifecycle/handovers/{id}/tasks/{taskId}/verify`
  - Verifikasi penerimaan butir tugas (hanya menerima item berstatus Submitted).
* `POST /api/corporate/human-resource/lifecycle/handovers/{id}/complete`
  - Selesaikan checklist (menolak jika memiliki 0 tugas atau ada tugas belum selesai).

### 3.6 Submenu 6: Clearense (`/hr/separation/exit-clearance`)
* `GET /api/corporate/human-resource/lifecycle/exit-clearance?page=1&pageSize=10&status={status}`
  - Response: `ApiResponse<PagedResult<ExitClearanceItemDto>>` (`items: []` saat zero data).
* `GET /api/corporate/human-resource/employee-relations/exit-clearances/{id}`
  - Detail status 5 departemen (Direct Supervisor, IT, HR, General Affair, Finance).
* `POST /api/corporate/human-resource/employee-relations/exit-clearances/{id}/items/{itemId}/signoff`
  - Tanda tangani checklist departemen dengan verifikasi identitas server fail-closed.
* `POST /api/corporate/human-resource/employee-relations/exit-clearances/{id}/finalize`
  - Finalisasi kliring dan pembukaan kunci hak payroll akhir (wajib seluruh 5 departemen Approved).

---

## 4. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Runtime Backend**: Server ASP.NET Core berjalan pada background task ID `c4dedf65-362b-45de-b26b-640c0ad51c86/task-412`.
* **Kesiapan Frontend**: Lapisan frontend siap diimplementasikan murni berbasis kontrak di atas tanpa mock/fallback data.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
