# Laporan Perubahan Backend — `BE-HRD-010`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-010` |
| Judul | Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi Penggajian |
| Slice | `S-B5` (Kalkulasi Payroll, Pemotongan Angsuran, & Restitusi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 5 |
| Trace | `HRD-DEC-009`, `IN-10`, `BE-FIN-110`, `BE-FIN-111` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-007` (Selesai ✅), `BE-HRD-008` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `PayrollRunInstallmentDeductionController.cs`, `PayrollInstallmentDeductionService.cs`, `PayrollRestitutionController.cs`, `PayrollRestitutionService.cs`, `AttendancePayrollHandoffController.cs`, `OvertimePayrollHandoffController.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Integrasi Data Presensi dan Lembur Sah ke Putaran Penggajian**:
   Sebelumnya, input presensi harian (potongan keterlambatan / ketidakhadiran) dan upah lembur berisiko dihitung dari data mentah yang belum selesai direkonsiliasi. Melalui gerbang serah terima `AttendancePayrollHandoff` dan `OvertimePayrollHandoff`, hanya data yang telah berstatus `Verified` yang dimasukkan ke dalam kalkulasi payroll.
2. **Pemotongan Cicilan Piutang RS pada Slip Gaji (`BE-FIN-110`)**:
   Staf rumah sakit yang memiliki pinjaman atau kasbon di bagian Keuangan dipotong cicilannya secara otomatis per putaran payroll run (`PayrollRunInstallmentDeductionController`). Pemotongan ini mengurangi saldo piutang pegawai di modul Finance secara langsung dan tercatat pada rincian potongan slip gaji.
3. **Mekanisme Restitusi Salah Potong Gaji ke Periode Berikutnya (`BE-FIN-111`)**:
   Apabila terjadi kekeliruan pemotongan pinjaman (misal cicilan terpotong melebihi sisa hutang atau ada penyesuaian bunga), `PayrollRestitutionController` mengeksekusi restitusi pengembalian dana yang dikreditkan kembali ke slip gaji periode berjalan/berikutnya secara idempoten.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Presensi Final Terverifikasi]      [Lembur Final SPKL Terverifikasi]
               │                                   │
               └─────────────────┬─────────────────┘
                                 │
                                 ▼
                     [Putaran Payroll Run HR]
                                 │
       ┌─────────────────────────┼─────────────────────────┐
       ▼                         ▼                         ▼
[Gaji Pokok & Tunjangan]  [Potongan Cicilan RS]    [Restitusi Salah Potong]
 (Workforce Core)          (BE-FIN-110)              (BE-FIN-111)
       │                         │                         │
       └─────────────────────────┼─────────────────────────┘
                                 │
                                 ▼
                      [Kalkulasi Take-Home Pay]
                                 │
                                 ▼
            [Penguncian Slip Gaji & Serah Terima ke Finance]
```

* **Tujuan**: Memastikan keakuratan perhitungan penghasilan bersih seluruh tenaga medis dan non-medis, penegakan kewajiban angsuran rumah sakit, serta perlindungan hak pegawai atas kekeliruan pemotongan gaji.
* **Pelaku**: HR Payroll Officer, Manager Keuangan / Finance, Pegawai Penerima Gaji.
* **Pemicu**: Siklus tutup buku penggajian bulanan (biasanya tanggal 25 setiap bulan).
* **Langkah & Aturan**:
  1. Data kehadiran dan lembur wajib berstatus final/terverifikasi sebelum batch payroll run dikunci.
  2. Pemotongan cicilan piutang RS mengecek saldo terbuka di Finance dan mencatat idempoten per baris slip gaji.
  3. Restitusi salah potong mengkreditkan kembali nilai restitusi pada periode berjalan tanpa perlu koreksi manual di luar sistem.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Payroll Management / Installment Deduction`
Tag: `[Tags("Corporate / Human Resource / Payroll Management / Installment Deduction")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/payroll-runs/{runId}/installment-deductions` | Menjalankan pemotongan cicilan piutang RS untuk seluruh staf pada payroll run | `PayrollRunInstallmentDeduction:ExecuteRunDeductions` | `runId` (Guid) | `ApiResponse<PayrollRunInstallmentDeductionSummaryResponse>` |

### Grup: `Corporate / Human Resource / Payroll Management / Restitution`
Tag: `[Tags("Corporate / Human Resource / Payroll Management / Restitution")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/payroll-restitutions` | Memproses restitusi salah potong cicilan ke slip gaji periode berikutnya | `PayrollRestitution:ProcessRestitution` | `PayrollRestitutionRequest` | `ApiResponse<PayrollRestitutionResponse>` |

---

## 4. Bukti implementasi kode

1. **Controller & Service Pemotongan Cicilan**:
   - Berkas: [PayrollRunInstallmentDeductionController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunInstallmentDeductionController.cs)
   - Berkas: [PayrollInstallmentDeductionService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollInstallmentDeductionService.cs)

2. **Controller & Service Restitusi Slip Gaji**:
   - Berkas: [PayrollRestitutionController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRestitutionController.cs)
   - Berkas: [PayrollRestitutionService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollRestitutionService.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Perhitungan memasukkan data sah dari presensi dan lembur yang sudah berstatus final -> **LULUS**.
* **Acceptance Criteria 2**: `PayrollRestitutionController` dan `PayrollRunInstallmentDeductionController` terintegrasi pada putaran payroll -> **LULUS**.
* **Acceptance Criteria 3**: Serah terima batch payroll ke modul Finance bersifat idempoten -> **LULUS**.
