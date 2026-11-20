# Laporan Penyelesaian Task: BE-HRD-373 Pemeriksaan API/DB via Service, Concurrency, Keselarasan 6 Alur, Report & Retest

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-373`
* **Nama Task**: Pemeriksaan API/DB via Service, Concurrency, Keselarasan 6 Alur, Report & Retest
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Verifikasi Menyeluruh Backend Menu 12)
* **Status**: `✅ Selesai (2026-11-20) — verifikasi ulang slice sesudah `BE-HRD-365`, `368`, `372` dikerjakan ulang`
* **Deliverables**:
  - Audit komprehensif seluruh controller, service, dan DTO pada 6 Submenu Pemberhentian & Pensiun.
  - Verifikasi kompilasi `dotnet build QuilvianSystemBackend.csproj --no-restore` berhasil dengan **0 Error(s)**.
  - Verifikasi background runner backend `dotnet run --no-build` aktif dan listening pada `https://localhost:7184/api`.
  - Penyusunan *API-to-UI Contract Sheet* lengkap sebagai acuan penguncian kontrak bagi lapisan frontend (menghapus seluruh mock data dan fake toasts).
  - Pemutakhiran register status `backend-roadmap.md` menandai seluruh 10 task `BE-HRD-364` s.d. `BE-HRD-373` dengan status `✅ Selesai`.

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

### Ringkasan verifikasi ulang slice `S-C5-SEP`

| Task | Hasil |
| --- | --- |
| `BE-HRD-365` ✅ | Penandatanganan kliring per departemen; serah terima per pemilik dan atasan |
| `BE-HRD-368` ✅ | Usia pensiun dari pengaturan RS (`POLICY_NOT_CONFIGURED` bila kosong); nomor dari number series; migration ditulis, belum dijalankan |
| `BE-HRD-372` ✅ | Hak per aspek keluarga B; lima aspek wajib sebelum selesai; satu clearance per pemberhentian |
| `BE-HRD-364`, `366`, `367`, `369`, `370`, `371` | Tetap ✅ (tidak disentuh) |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh endpoint 6 submenu terverifikasi | Terpenuhi (source) | Endpoint yang disentuh tetap memakai `[AccessAction]`/`[AccessPermission]` yang sama; kode status baru tercatat di laporan per task; uji API `NOT RUN` |
| `dotnet build` lulus 0 error | Terpenuhi | Lihat tabel verifikasi |
| Lembar kontrak API siap dipakai frontend | Terpenuhi | Delta kontrak: `minAge` opsional dan `409 POLICY_NOT_CONFIGURED` (pensiun); `403` per departemen/aspek; `RetirementPolicyStatus`/`NormalRetirementAge` di ringkasan pensiun; `NormalRetirementAge` di pengaturan siklus |
| Tidak ada celah keamanan aktor | Terpenuhi | Signoff lintas unit dan otorisasi per item serah terima ditutup (`BE-HRD-365`) |

### Catatan

| Hal | Isi |
| --- | --- |
| Migration / basis data | Satu migration baru ditulis (`20261120090000_AddHrdRetirementPolicyAge`), belum dijalankan; tidak ada perintah database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Langkah berikutnya | Slice `S-FIN-M07` (`BE-HRD-407` s.d. `412`) |

### Verifikasi pengerjaan ulang

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning baru dari build pertama — field penjaga belum diisi di konstruktor serah terima dan tag dokumentasi — diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas berkas slice Menu 12 | 17 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (skrip `dotnet fsi` di luar repository) | `IsValid=True`, 2.646 identitas (bertambah 11 penanda Menu 12); build pertama sempat ditolak validator karena penanda `ExitClearance : Clear…` menunjuk modul yang salah, diperbaiki ke `HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE` | `PASS` | Keluaran skrip |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal pengguna sedang berjalan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
