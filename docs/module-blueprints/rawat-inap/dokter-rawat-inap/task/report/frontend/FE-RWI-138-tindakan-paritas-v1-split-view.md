# Laporan Perubahan Frontend & Backend — `FE-RWI-138-tindakan-paritas-v1-split-view`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-138` |
| Judul | Modernisasi Penuh Menu Tindakan Dokter Rawat Inap — Paritas V1 Split-View Inline, Tanpa Modal Pop-up, FOC, Riwayat Multi-Filter, Modal Detail & Koreksi |
| Modul | Rawat Inap — Dokter Rawat Inap, Tab **Tindakan** |
| Rencana kerja | [`rencana-kerja/tindakan/tindakan.md`](../../../roadmap/rencana-kerja/tindakan/tindakan.md) — Tahap 4 (implementasi tuntas) |
| Bukti V1 | `QuilvianV1/QuilvianSystemFrontendDev/captures/dokter-rawat-inap/06-tindakan/01-form-tindakan.png`, `02-riwayat-tindakan.png` |
| Trace | `FE-RWI-073`, `FE-DOK-11`, `FR-DOK-100`, `FR-DOK-101`, `FR-DOK-102`; `BE-RWI-051`, `BE-RWI-052`, `BE-RWI-053`; `SKP 2` (Verifikasi Lisan) |
| Target tulis | Backend: `NewQuilvianSystemBackend` (`PatientProcedureDtos.cs`, `PatientProcedureOrderService.cs`, `PatientProcedureController.cs`); Frontend: `QuilvianSystemFrontendDev` (`procedure-form-panel.jsx`, `procedure-history-panel.jsx`, `use-inpatient-procedure-tab.jsx`, `inpatient-procedure-tab.jsx`, `patient-procedure.service.js`, `physician-procedure.module.css`, modals, unit test) |
| Status | ✅ Selesai diimplementasikan secara tuntas — Backend Build 0 Error(s), Frontend Unit Tests 9/9 PASS (0 fail) |

---

## 1. Ringkasan Perubahan

Sesuai permintaan modernisasi V1 (`/modernisasi-menu-v1`), menu Tindakan Dokter Rawat Inap dirombak total dari model dialog pop-up menjadi tata letak **Split-View 2-Kolom Inline** persis V1:

1. **Form Tindakan Medis (Paritas 100% V1 `01-form-tindakan.png`)**:
   - Menghilangkan dialog pop-up katalog (`DoctorProcedureCatalogModal`).
   - **Header Banner**: "Form Tindakan Medis - Rawat Inap" dengan gradient teal/cyan khas Quilvian V1. Kotak konteks pasien internal dihilangkan karena informasi pasien (nama, no RM, kelas, ruangan) sudah tersedia secara permanen di header workspace rawat inap.
   - **Split-View 2-Kolom**:
     - **Kolom Kiri (Daftar Tindakan Medis)**: Live search katalog, tabel kode, nama tindakan, tarif sesuai kelas pasien, badge penjamin (`Ditanggung`), dan tombol aksi `[+]` untuk memilih tindakan ke form konfigurasi.
     - **Kolom Kanan (Form Tindakan Medis)**: Form konfigurasi tindakan aktif mencakup Kode, Nama, Tarif Satuan, Jumlah (*required*), Disposisi Pasien (*textarea*), Switch FOC (*Free of Charge* / bebas biaya dengan validasi alasan FOC), Indikasi Klinis/Keterangan, dan flag klinis rawat inap (Tindakan Utama, Cito). Aksi: `[ Batal ]` dan `[ Tambahkan ]`.
   - **Tabel Staging Bawah (Tindakan yang Dipilih)**: Keranjang staging tindakan sebelum dikirim. Menyediakan kolom No, Kode, Nama Tindakan, Tarif Satuan, Jumlah, Subtotal, Status Biaya (FOC/Berbayar), Keterangan, dan tombol Hapus baris. Footer menyediakan tombol `[ Reset Semua ]`, kalkulasi otomatis **Total Biaya**, dan tombol batch `[ Simpan Tindakan ]`.

2. **Riwayat Tindakan Perawatan (Paritas 100% V1 `02-riwayat-tindakan.png`)**:
   - Filter bar multi-periode: Pencarian teks, dropdown periode (Semua, Hari Ini, 7 Hari Terakhir, 30 Hari Terakhir, Rentang Kustom), date picker tanggal mulai dan tanggal akhir, serta tombol reset filter.
   - Kolom tabel lengkap: Waktu tindakan, nama & kode tindakan, jumlah, total tarif / badge FOC, status klinis, tenaga penginput, status verifikasi instruksi (SKP 2), tenaga pelaksana, dan status billing kasir.
   - Integrasi Modal Pop-up:
     - **DetailTindakanModal**: Rincian lengkap identitas pasien, tarif, status penjaminan asuransi, FOC, instruksi, dan billing kasir.
     - **EditTindakanModal**: Formulir koreksi data tindakan (jumlah, disposisi, switch FOC + alasan, keterangan klinis, instruksi) yang memanggil `PUT /patient-procedures/{id}`.
     - **ConfirmModal**: Pembatalan pesanan tindakan dengan validasi alasan wajib (*mandatory cancel reason*).

3. **Backend Support**:
   - Memperluas DTO `CreateInpatientProcedureOrderRequest` dengan properti `IsFreeOfCharge`, `FreeOfChargeReason`, dan `DispositionNote`.
   - Memetakan field FOC pada `PatientProcedureOrderService.CreateInpatientOrderAsync`: Jika `IsFreeOfCharge = true`, maka `TotalPrice = 0`, `PatientPayAmount = 0`, dan `IsBillable = false`.
   - Memastikan respons list dan detail (`PatientProcedureResponse`) mengekspos `ClinicalNote`, `ClinicalReason`, `InstructionNote`, `DispositionNote`, `IsFreeOfCharge`, dan `FreeOfChargeReason`.

---

## 2. Bukti Verifikasi & Uji Teknis

### A. Backend Compilation
Perintah eksekusi:
```powershell
dotnet build QuilvianSystemBackend.csproj -c Debug --no-restore -v m
```
Hasil:
```text
QuilvianSystemBackend -> ...\QuilvianSystemBackend.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.23
```

### B. Frontend Unit Testing
Perintah eksekusi:
```powershell
cmd.exe /c node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-procedure-parity.test.mjs
```
Hasil:
```text
✔ Procedure Parity: extraction utilities return accurate procedure attributes (1.9219ms)
✔ Procedure Parity: formatProcedureMoney handles null, zero, and currency formatting (19.907ms)
✔ Procedure Parity: normalizeProcedurePresentationItem produces complete model (0.2401ms)
✔ Procedure Parity: doctor-clinical-base index exports all shared procedure components (5.6645ms)
✔ Procedure Parity: Outpatient doctor-procedure-tab reuses shared components without regression (1.0317ms)
✔ Procedure Parity: Inpatient procedure-form-panel follows V1 split-view without catalog modal (1.1326ms)
✔ Procedure Parity: Inpatient hook supports builder state, FOC, update, and patient switch safety (1.0629ms)
✔ Procedure Parity: Inpatient history panel provides V1 multi-filter and detail/edit modals (0.9991ms)
✔ Procedure Parity: Inpatient tab preserves all segmented capabilities (0.8833ms)
ℹ tests 9
ℹ suites 0
ℹ pass 9
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 182.7003
```

Uji regresi modul resep:
```powershell
cmd.exe /c node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-prescription-parity.test.mjs
```
Hasil:
```text
✔ 7 tests passed (0 fail)
```

---

## 3. Daftar File yang Diubah & Dibuat

### Backend (`NewQuilvianSystemBackend`)
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs`
- `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs`

### Frontend (`QuilvianSystemFrontendDev`)
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx` (Dibuat ulang menjadi V1 Split-View 2 Kolom)
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-history-panel.jsx` (Filter bar multi-periode + aksi modal)
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/inpatient-procedure-tab.jsx` (Wiring modal update & context)
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/modals/detail-modal-tindakan.jsx` (File baru: Modal Detail)
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/modals/edit-modal-tindakan.jsx` (File baru: Modal Koreksi Tindakan)
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx` (Staging, FOC, dan `handleUpdateProcedure`)
- `src/lib/services/health-services/clinical-management/patient-procedure.service.js` (`updatePatientProcedure`)
- `src/style/health-services/inpatient-management/physician-procedure.module.css` (Style V1 banner, split-view, tabel staging, dan filter bar)
- `tests/unit/inpatient-procedure-parity.test.mjs` (Unit test parity V1)
