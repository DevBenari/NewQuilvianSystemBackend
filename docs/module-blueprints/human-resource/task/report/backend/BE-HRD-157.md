# Laporan Penyelesaian Backend Task: BE-HRD-157

## 1. Identitas Task
* **ID Task**: `BE-HRD-157`
* **Nama Task**: Engine alur otorisasi 3-jenjang, hierarki eskalasi & penegakan anti-self-approval HR Manager (`PayrollApprovalWorkflowService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-31: Integrasi Potongan Koperasi, Variance Guard, & Alur Otorisasi 3-Jenjang`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Penggajian rumah sakit melibatkan dana miliaran rupiah yang dicairkan ke ratusan tenaga medis dan staf penunjang. Mengingat tingginya risiko kecurangan dan benturan kepentingan, tata kelola korporasi rumah sakit mewajibkan pemisahan wewenang secara ketat (*3-Tier Segregation of Duties*):

1. **Tier 1 — Pengajuan oleh Payroll Officer (`Submitted`)**:
   - Payroll Officer mengkalkulasi komponen gaji, menyerap potongan koperasi, dan menyelesaikan justifikasi variansi.
   - Batch diajukan ke status `WaitingHrReview`. Sistem memvalidasi kelolosan gerbang variansi (`PAY-B3`).
2. **Tier 2 — Review & Verifikasi Manajerial oleh HR Manager (`Reviewed`)**:
   - HR Manager memeriksa keabsahan rekapitulasi, memverifikasi catatan justifikasi lonjakan variansi, dan meneruskan berkas ke status `WaitingFinalApproval`.
   - **Penegakan Keras Aturan Anti-Self-Approval (`PAY-B4`, `VR-PAY-08`)**: HR Manager dilarang menyetujui batch yang dia ajukan/submit sendiri. Tindakan approver mereview batch inisiasinya sendiri ditolak langsung dengan status **HTTP 403 Forbidden**.
3. **Tier 3 — Persetujuan Akhir Direksi oleh Direktur SDM / Keuangan (`ApprovedFinal`)**:
   - Direktur memeriksa kepatuhan anggaran dan memberikan persetujuan final.
   - Dampak persetujuan Tier 3:
     * Status batch berubah menjadi `Approved`.
     * Batch **dikunci secara permanen (`IsLocked = true`)** dan menjadi **immutable** (`PAY-B2`).
     * Seluruh operasi pengubahan komponen, penambahan pegawai, atau rekalkulasi diblokir permanen.
4. **Alur Penolakan (*Rejection Workflow*)**:
   - HR Manager atau Direktur berhak menolak batch yang terindikasi anomali.
   - Penolakan **wajib disertai catatan alasan resmi minimal 15 karakter (`PAY-B7`, `VR-PAY-16`)**.
   - Status batch dikembalikan ke `NeedCorrection`, gembok batch dibuka kembali secara terkendali, dan audit log penolakan direkam ke tabel `TrxPayrollApproval`.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollApprovalDtos.cs`
* DTO yang dibuat:
  - `SubmitPayrollRunCommand`: Payload pengajuan Tier 1.
  - `ReviewPayrollRunCommand`: Payload review manajerial Tier 2.
  - `ApprovePayrollRunCommand`: Payload persetujuan final Tier 3.
  - `RejectPayrollRunCommand`: Payload penolakan dengan validasi `[MinLength(15)]` pada `RejectionReason`.
  - `PayrollApprovalResultDto`: Hasil eksekusi alur otorisasi (status sebelum/sesudah, tier approval, aksi yang diambil, ID dan nama user approver, stempel waktu, status gembok batch, dan pesan).
  - `PayrollApprovalHistoryDto`: Rekam jejak audit trail seluruh tahapan persetujuan batch.

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollApprovalWorkflowService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollApprovalWorkflowService.cs`
* Logika Bisnis:
  1. Submit Tier 1 (`SubmitRunForReviewAsync`):
     - Memeriksa gerbang kelolosan variansi via `IPayrollVarianceResolutionService`.
     - Mengubah status ke `WaitingHrReview` dan mencatat entitas `TrxPayrollApproval` level `PayrollOfficer`.
  2. Review Tier 2 (`ReviewRunAsync`):
     - Mengecek `SubmittedByUserId == actorUserId` $\rightarrow$ Jika sama, lemparkan penolakan `HTTP 403 Forbidden` (`PAY-B4`).
     - Mengubah status ke `WaitingFinalApproval` dan mencatat entitas level `HrManager`.
  3. Approve Final Tier 3 (`ApproveRunFinalAsync`):
     - Memverifikasi anti-self-approval Direksi.
     - Mengubah status ke `Approved`, membekukan batch (`IsLocked = true`, `LockedAt = DateTime.UtcNow`, `ApprovedAt = DateTime.UtcNow`).
     - Mencatat entitas level `Director`.
  4. Penolakan (`RejectRunAsync`):
     - Memvalidasi panjang string alasan $\ge 15$ karakter.
     - Mengubah status ke `NeedCorrection` dan membuka gembok proteksi (`IsLocked = false`).
     - Mencatat riwayat penolakan berstatus `Rejected`.
  5. Audit Trail (`GetApprovalHistoryAsync`):
     - Mengambil seluruh log aksi approval berurutan berdasarkan `StepOrder` dan stempel waktu.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/submit` | Tier 1: Submit batch oleh Payroll Officer ke HR Manager | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/review` | Tier 2: Review batch oleh HR Manager (anti-self-approval) | 200 OK / 403 Forbidden / 422 | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/approve-final` | Tier 3: Persetujuan final Direktur & penguncian immutable | 200 OK / 403 Forbidden / 422 | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/reject` | Tolak batch kembali ke NeedCorrection (min. 15 karakter) | 200 OK / 400 Bad Request / 422 | Update |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/{id}/approvals` | Tampilkan riwayat audit trail persetujuan approval batch | 200 OK | Read |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Pencegahan Kolusi Internal (Uji Coba Self-Approval)**:
  - User `budi.hr` (Payroll Officer merangkap HR Manager) mengajukan batch penggajian periode berjalan.
  - Saat `budi.hr` mencoba menekan tombol Review (`/review`), sistem mendeteksi `SubmittedByUserId == actorUserId`.
  - Respon Sistem: Permintaan ditolak dengan `HTTP 403 Forbidden`:
    `"Pelanggaran Segregation of Duties (PAY-B4): Approver dilarang mereview batch penggajian yang disubmit oleh dirinya sendiri (VR-PAY-08). Wajib direview oleh pejabat HR yang berbeda."`
* **Persetujuan Sah Berjenjang**:
  - Langkah 1: Diajukan oleh Officer `siti.payroll` $\rightarrow$ Status: `WaitingHrReview`.
  - Langkah 2: Direview oleh HR Manager `dr. hendra.hrm` $\rightarrow$ Status: `WaitingFinalApproval`.
  - Langkah 3: Disetujui final oleh Direktur Keuangan `drg. maya.cfo` $\rightarrow$ Status: `Approved`, `IsLocked: true`.
  - Hasil: Seluruh data terkunci rapat dan siap untuk serah terima paket pencairan ke Finance (Fase MVP-32).

---

## 5. Verifikasi Acceptance Criteria
* [x] **Pemisahan Peran 3-Tier Berjenjang**: Menegakkan hierarki Officer $\rightarrow$ HR Manager $\rightarrow$ Direktur.
* [x] **Penegakan Anti-Self-Approval (`PAY-B4`, `VR-PAY-08`)**: Menolak approver yang merangkap inisiator dengan HTTP 403.
* [x] **Kewajiban Alasan Penolakan $\ge 15$ Karakter (`PAY-B7`, `VR-PAY-16`)**: Menolak input alasan penolakan pendek dengan HTTP 400.
* [x] **Penguncian Permanen Batch Pasca Tier 3 (`PAY-B2`)**: Menyematkan `IsLocked = true` dan menghentikan seluruh mutasi.
* [x] **Zero Warning Kompilasi Baru**: Solusi backend lulus `dotnet build` dengan **0 Error(s)**.
