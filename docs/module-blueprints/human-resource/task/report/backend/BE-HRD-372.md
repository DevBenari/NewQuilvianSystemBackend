# Laporan Penyelesaian Task: BE-HRD-372 Clearense: Canonical Owner, Signoff Per Unit, Kewajiban Finance, Lock Integration

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-372`
* **Nama Task**: Clearense: Canonical Owner, Signoff Per Unit, Kewajiban Finance, Lock Integration
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 6: Clearense)
* **Status**: `✅ Selesai (2026-11-20) — dikerjakan ulang sesudah diturunkan ke 🟡 pada 19 November 2026`
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

Kliring lima departemen (keluarga A, `HrdExitClearanceProcess`) sudah menahan finalisasi sampai kelima departemen `Approved` dan tanggungan Rp 0, lalu membuka gembok payroll akhir. Yang belum: (1) penandatanganan unit lain tidak ditolak (ditutup bersama `BE-HRD-365`); (2) keluarga B (`TrxExitClearance`, Lifecycle) mengizinkan siapa pun dengan hak `Update` mencentang aspek mana pun dan mengesahkan clearance hanya dengan syarat bebas piutang; (3) dua keluarga hidup berdampingan sehingga satu pemberhentian dapat memiliki dua clearance.

### Keputusan pemilik kanonik (diambil sebagai asumsi teknis, dapat dibalik pemilik)

| Jalur pemberhentian | Clearance kanonik |
| --- | --- |
| Inisiasi Hubungan Kerja (`EmployeeSeparationService.InitiateSeparationAsync`, Menu 12 Pemberhentian) | Keluarga A — kliring lima departemen + gembok payroll akhir |
| Handoff pengunduran diri, pensiun, akhir kontrak (`ResignationLifecycleHandoffService`, `RetirementService`) | Keluarga B — clearance Lifecycle + verifikasi piutang Finance |

Invarian baru: membuat clearance keluarga B untuk pemberhentian yang sudah memiliki kliring keluarga A ditolak `422`, sehingga satu pemberhentian hanya punya satu clearance.

### Proses bisnis keluarga B sesudah perbaikan

1. Setiap aspek (unit kerja, aset, payroll, akses IT, HR) hanya dapat diubah pemegang hak aspek itu (`ExitClearance : ClearDepartment`, `ClearAsset`, `ClearPayroll`, `ClearItAccess`, `ClearHr`); selain itu `403`.
2. `POST …/exit-clearances/{id}/complete` ditolak `422` bila masih ada aspek yang belum selesai, lalu tetap memeriksa piutang Finance secara langsung (sisa piutang > 0 → `422`).

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `LifecycleManagement/LifecycleExitExplicitPermissions.cs` (baru) | Lima penanda hak aspek |
| `LifecycleManagement/Services/ExitClearanceService.cs` | Cek hak aspek; syarat lima aspek sebelum selesai; invarian satu clearance per pemberhentian |
| `LifecycleManagement/Controllers/ExitClearanceController.cs` | Meneruskan `User`; `UnauthorizedAccessException` → `403` |
| Berkas keluarga A | Lihat laporan `BE-HRD-365` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Signoff unit lain ditolak | Terpenuhi | Keluarga A: hak per departemen; keluarga B: hak per aspek |
| Tanggungan belum lunas memblokir finalisasi | Terpenuhi | Keluarga A: total tanggungan > 0 → tolak; keluarga B: piutang Finance langsung |
| Gembok payroll akhir hanya terbuka setelah tuntas 100% | Terpenuhi | `FinalizeExitClearanceAsync` membuka `IsFinalPayrollUnlocked` hanya sesudah lima departemen `Approved` dan tanggungan 0 |

### Catatan

| Hal | Isi |
| --- | --- |
| Temuan | `IExitClearanceStatusProvider` (gembok payroll keluarga A) terdaftar di DI tetapi belum dibaca modul Payroll; penegakan di sisi Payroll di luar kartu ini |
| Peringatan untuk admin | Lima hak aspek `Clear…` perlu dicentang untuk petugas tiap aspek |
| Perubahan sampingan | `NONE` |

### Verifikasi pengerjaan ulang

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning baru dari build pertama — field penjaga belum diisi di konstruktor serah terima dan tag dokumentasi — diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas berkas slice Menu 12 | 17 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (skrip `dotnet fsi` di luar repository) | `IsValid=True`, 2.646 identitas (bertambah 11 penanda Menu 12); build pertama sempat ditolak validator karena penanda `ExitClearance : Clear…` menunjuk modul yang salah, diperbaiki ke `HUMAN_RESOURCE_LIFECYCLE_EXIT_CLEARANCE` | `PASS` | Keluaran skrip |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal pengguna sedang berjalan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
