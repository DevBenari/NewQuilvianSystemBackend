# Laporan Perubahan Backend — `BE-FIN-116`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-116` |
| Judul | Finance dapat melihat dan mengekspor jadwal potongan per periode, termasuk tunggakan terbawa, sebagai templat unggah |
| Slice | `REV-21` (Slice D: Jembatan Manual & Integrasi Mesin Payroll) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-21` |
| Trace | `FIN-DEC-219`; `FR-FIN-253`; `FIN-DES-105`; `FIN-INTEGRATION-1.10` §`S.1`; `FIN-API-1.11` §`S.1` (`/schedule`, ekspor) |
| Contract version | `FIN-INTEGRATION-1.10` §`S.1`; `FIN-API-1.11` §`S.1` |
| Dependency | `BE-FIN-093` 🟡 |
| Klasifikasi | `MEDIUM` — pembuatan layanan query jadwal potongan per periode `FinancePayrollDeductionBatchService`, generator ekspor Excel templat ClosedXML, generator ekspor PDF serah terima QuestPDF `DeductionScheduleReportDocuments`, controller `FinancePayrollDeductionBatchController` berhak akses `FinancePayrollDeductionBatch : Read`, DTO kontrak `FIN-API-1.11` §`S.1.1`, dan registrasi DI di `Program.cs` |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("kerjakan BE-FIN-116 tanpa build dotnet automatis /build-module-backend", 13 Oktober 2026) |
| Target tulis | Lihat §3.2 |
| Model | Claude Sonnet 3.7 / Antigravity |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh implementasi DTO kontrak, query jadwal angsuran dengan penyaringan status dan perjanjian disetujui, ekspor Excel dengan kolom kosong sesuai templat, ekspor PDF serah terima, controller berhak akses resmi, dan DI service selesai di source; `dotnet build` **NOT RUN** oleh agent (instruksi pemilik: "tanpa build dotnet automatis") |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | Corporate / FinanceManagement / Receivable |
| Registry | `FinanceManagement — Fin — ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` (`FinancePayrollDeductionBatchDtos.cs`, `DeductionScheduleReportDocuments.cs`, `FinancePayrollDeductionBatchService.cs`, `FinancePayrollDeductionBatchController.cs`); `TOUCHED CODE` (`Program.cs`, `01-backend-roadmap.md`) |
| Entity operasional baru | Menggunakan model EF Core existing: `FinReceivableInstallment`, `FinReceivableInstallmentPlan`, `FinReceivable`, `MstEmployee`, `MstPayrollPeriod`; tidak membuat entitas atau tabel duplikat |
| QBE yang berlaku | `QBE-SVC-001` (authoritative service logic), `QBE-API-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-NAM-001` |

---

## 1. Masalah yang Diperbaiki

Sebelum fase jembatan manual potong gaji dibangun:
1. **Ketiadaan Visibilitas Jadwal Potongan bagi Penggajian (HR)**: Pada masa transisi jembatan manual, pemotongan gaji pegawai untuk cicilan piutang RS dilakukan di luar sistem oleh tim penggajian. Tanpa jadwal resmi dari Finance, petugas penggajian tidak mengetahui siapa saja pegawai yang memiliki cicilan jatuh tempo pada periode berjalan (`FIN-DEC-219`, `FR-FIN-253`).
2. **Ketiadaan Pelimpahan Akumulasi Tunggakan pada Jadwal**: Bila seorang pegawai gagal terpotong pada bulan-bulan sebelumnya (misal Juli dan Agustus), sisa kewajibannya seharusnya diakumulasikan ke dalam nominal terutang bulan berjalan (`CarriedOverAmount`), tanpa memunculkan kembali baris historis yang sudah berstatus `TERTUNGGAK`.
3. **Ketiadaan Templat Terstandarisasi untuk Unggah Hasil**: Tim penggajian memerlukan templat berkas Excel yang memuat identitas pegawai, kode angsuran, dan nominal terutang, dengan kolom status potongan dan nominal terpotong yang sengaja dikosongkan agar dapat diisi dan diunggah kembali ke sistem pada saat proses batch hasil potongan (`BE-FIN-117`).

---

## 2. Proses Bisnis & Pemenuhan Acceptance Criteria

### 2.1 Pemenuhan Acceptance Criteria

1. **AC 1 — Cicilan Gagal Dilimpahkan; Baris Historis Tidak Muncul**:
   - Query pada `FinancePayrollDeductionBatchService.GetScheduleAsync` secara tegas memfilter:
     ```csharp
     inst.DeductionPeriod == period && inst.Status == FinReceivableInstallmentStatuses.Dijadwalkan
     ```
   - Contoh kasus: Budi memiliki cicilan Rp 1.000.000 per bulan. Pada Juli dan Agustus potongan gagal sehingga status kedua angsuran tersebut berubah menjadi `TERTUNGGAK`. Sisa tunggakan Rp 2.000.000 telah dilimpahkan ke `CarriedOverAmount` angsuran September.
   - Saat jadwal September ditarik (`period = "2026-09"`), baris Juli dan Agustus **tidak muncul** karena statusnya bukan `DIJADWALKAN` dan periodenya bukan September. Baris September menampilkan:
     - `ScheduledAmount` = Rp 1.000.000
     - `CarriedOverAmount` = Rp 2.000.000
     - `DueAmount` (`OutstandingAmount`) = **Rp 3.000.000**.
2. **AC 2 — Perjanjian Status `MENUNGGU` Tidak Muncul**:
   - Query melakukan join ke `FinReceivableInstallmentPlan` dan memfilter:
     ```csharp
     plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui
     ```
   - Angsuran dari perjanjian yang masih berstatus `MENUNGGU`, `DITOLAK`, `DIBATALKAN`, atau `SELESAI` tidak akan masuk ke dalam jadwal potongan.
3. **AC 3 — Ekspor Excel Memuat Kolom Status dan Nominal Terpotong Kosong**:
   - Metode `ExportScheduleExcelAsync` menggunakan ClosedXML untuk menyusun lembar kerja Excel berformat `.xlsx`.
   - Kolom yang dihasilkan:
     1. `No`
     2. `Kode Angsuran` (`{PlanNumber}-{InstallmentNumber}`)
     3. `NIP`
     4. `Nama Pegawai`
     5. `Unit Kerja`
     6. `No. Piutang`
     7. `Angsuran Ke`
     8. `Nominal Terjadwal`
     9. `Tunggakan Terbawa`
     10. `Nominal Terutang`
     11. `Status Potongan` (KOSONG — untuk diisi `BERHASIL`, `SEBAGIAN`, atau `GAGAL`)
     12. `Nominal Terpotong` (KOSONG — untuk diisi angka nominal yang berhasil dipotong)
   - Kolom 11 dan 12 sengaja dikosongkan pada setiap baris data sehingga berkas Excel ini sekaligus berfungsi sebagai templat unggah hasil batch (`UploadDeductionBatchRequest`) pada `BE-FIN-117`.
   - Header dibekukan (*freeze row* 1), lebar kolom disesuaikan otomatis (*auto-fit*), dan format angka numerik (`#,##0`) dipasang pada kolom nominal.

### 2.2 Ekspor Dokumen PDF Serah Terima

- Metode `ExportSchedulePdfAsync` memanggil `DeductionScheduleReportDocuments.BuildSchedulePdf` yang dibangun dengan QuestPDF.
- Dokumen dibuat dalam orientasi *A4 Landscape* dengan header resmi, garis batas, kotak ringkasan periode dan total terutang, tabel rincian angsuran yang rapi dengan perataan kanan pada nominal uang, serta penomoran halaman dinamis di footer.

---

## 3. Perubahan Kode Sumber

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (`BE-FIN-116`)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§`S.1`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (§`S.1`, §`S.1.1`)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (§`S.2`, §`S.3`)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallment.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallmentPlan.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs`
- `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs`
- `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstPayrollPeriod.cs`

### 3.2 Berkas yang Dibuat / Diperbarui

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinancePayrollDeductionBatchDtos.cs` | **Baru** | DTO kontrak `DeductionScheduleResponse`, `DeductionScheduleRowResponse`, serta kerangka DTO batch `UploadDeductionBatchRequest`, `SubmitDeductionBatchRequest`, `DeductionBatchResponse`, dll. sesuai `FIN-API-1.11` §`S.1.1`. |
| `Areas/Corporate/FinanceManagement/Receivable/Services/DeductionScheduleReportDocuments.cs` | **Baru** | Generator dokumen PDF jadwal potongan pegawai berbasis QuestPDF dengan layout A4 landscape, header ringkasan, tabel data, dan penomoran halaman. |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinancePayrollDeductionBatchService.cs` | **Baru** | Service logika query jadwal potongan dengan validasi format periode regex `^\d{4}-(0[1-9]|1[0-2])$`, resolusi `PayrollPeriodId` HR, pemuatan nama dan departemen pegawai, serta penyusunan berkas Excel ClosedXML (kolom status dan nominal kosong) dan PDF QuestPDF. |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinancePayrollDeductionBatchController.cs` | **Baru** | Controller API `api/v1/corporate/finance-management/payroll-deduction` dengan endpoint `GET /schedule`, `GET /schedule/export/excel`, dan `GET /schedule/export/pdf`, dilengkapi atribut `[AccessController]`, `[Tags]`, `[AccessAction]`, dan `[AccessPermission]` (`FinancePayrollDeductionBatch : Read`). |
| `Program.cs` | Diperbarui | Registrasi dependensi `builder.Services.AddScoped<FinancePayrollDeductionBatchService>();`. |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Diperbarui | Penandaan status task `BE-FIN-116` menjadi `🟡` (sebagian). |

---

## 4. Verifikasi & Bukti

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pemilik ("tanpa build dotnet automatis") |
| AC 1: Cicilan gagal dilimpahkan; baris historis tidak muncul | `PASS` | Review alur kueri | Terbukti pada kueri EF Core: filter `inst.DeductionPeriod == period && inst.Status == "DIJADWALKAN"`. Baris bulan sebelumnya yang berstatus `TERTUNGGAK` tidak terambil; nominal terutang baris periode aktif mencakup `ScheduledAmount + CarriedOverAmount`. |
| AC 2: Perjanjian `MENUNGGU` tidak muncul | `PASS` | Review alur kueri | Terbukti pada join ke `FinReceivableInstallmentPlan`: filter eksplisit `plan.Status == "DISETUJUI"`. Perjanjian `MENUNGGU` dikeluarkan dari hasil. |
| AC 3: Excel memuat kolom status dan nominal kosong | `PASS` | Review generator Excel | Terbukti pada `ExportScheduleExcelAsync`: header menyertakan 12 kolom; kolom 11 (`Status Potongan`) dan kolom 12 (`Nominal Terpotong`) diisi `string.Empty` pada seluruh baris data sebagai templat unggah. |
| Validasi parameter `period` | `PASS` | Review kontroler & service | Regex `^\d{4}-(0[1-9]|1[0-2])$` memeriksa format; input tidak sah ditolak dengan HTTP `400 Bad Request`. |
| Ekspor PDF serah terima | `PASS` | Review generator PDF | Dokumen QuestPDF terstruktur landscape dengan header ringkasan, tabel rincian angsuran, dan footer penomoran halaman. |
| Hak akses RBAC resmi terpasang | `PASS` | Review konfigurasi | `[AccessController]` dengan ControllerName `"FinancePayrollDeductionBatch"`, `[AccessAction]` Read, dipasangkan identik dengan `[AccessPermission]`. |

---

## 5. Kesimpulan & Status

Task `BE-FIN-116` telah selesai diimplementasikan pada source code backend. Seluruh kriteria penerimaan (AC 1–3) terpenuhi, menyediakan API query jadwal potongan, ekspor Excel sebagai templat unggah hasil, serta ekspor PDF sebagai dokumen serah terima resmi.

Status task ditandai 🟡 **Sebagian** (menunggu verifikasi kompilasi dan eksekusi bersama pemilik sistem sesuai instruksi no-auto-build).

