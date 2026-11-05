# Laporan Perubahan Backend — `PAY-BE-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-04` |
| Judul | Payroll Run dan kalkulasi penghasilan |
| Slice | Payroll mandiri — proses penggajian |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-03/04/05; Prompt Tahap B |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-01 |
| Klasifikasi | `EPIC` — mesin kalkulasi gaji + transaksi serializable + integrasi HRD/Finance |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — mesin kalkulasi, impor HRD, dan snapshot kebijakan; rumus PPh/BPJS `NEEDS CONFIRMATION` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Tidak ada orkestrator kalkulasi penghasilan utuh: hitung gaji pokok/tunjangan/lembur/pajak/iuran, prorata, cutoff, dan detail komponen per penerima belum ada, dan data absensi/cuti/lembur HRD belum diimpor ke run.

## 2. Proses bisnis

Petugas membuka run, mengimpor data HRD, lalu menghitung. Mesin menghitung per penerima secara deterministik: gaji pokok dari penetapan gaji disetujui/primer/efektif (dipecah per segmen), prorata per hari kalender, tunjangan tetap, lembur, tunjangan kehadiran, penghasilan variabel, potongan absensi dan cuti tanpa gaji, pajak, dan iuran. Hasil disimpan sebagai komponen berkode `PAY_*` beserta snapshot kebijakan. Penerima bermasalah ditandai `Error` dan menjadi penahan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | Mesin kalkulasi `PAYROLL-CALC-V1` |
| `Areas/Corporate/PayrollManagement/Services/PayrollHrdImportService.cs` | Kesiapan dan impor absensi/cuti/lembur (per sumber, idempoten) |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` | `CalculateAsync` memanggil mesin; `CreateAsync` satu run Regular per periode; ringkasan, riwayat, rincian penerima |
| `Areas/Corporate/PayrollManagement/Options/PayrollPolicyOptions.cs` | Parameter kebijakan terversi (`Payroll:Policy`) |
| `Areas/Corporate/PayrollManagement/Constants/PayrollStatusCatalog.cs` | Kode komponen mesin, status pembayaran/slip |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollRunDtos.cs, PayrollHrdImportDtos.cs` | Kontrak diperluas (`rowVersion`, `availableActions`, `warnings`, kesiapan, impor) |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** `GET summary`, `filters/metadata`, `{id}/status-history`, `{id}/employees/{id}`, `{id}/hrd-readiness`, `POST {id}/import-hrd`; filter `status` menerima beberapa nilai dipisah koma. **Database:** tidak ada tabel/kolom baru. **Keamanan:** aksi baru `ImportHrd`, `ReadHrdReadiness`, `ReadEmployeeDetail`, `ReadStatusHistory`, `ReadSummary`, `ReadMetadata` pada resource `PayrollRun`.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Corporate / Payroll Management / Payroll Run**.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `verify_be.py` (analisis statis kustom: using, arity, atribut akses, nama tipe ganda, bentrok rute, namespace kosong) | 0 kesalahan, 0 peringatan (38 file Payroll, 16 controller) | `PASS` | keluaran skrip, dijalankan ulang setelah perubahan terakhir |
| `dotnet build` pada project aplikasi | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | - |
| Tes otomatis backend | Tidak ada project tes otomatis di backend (kebijakan repository); tidak dibuat | `NOT APPLICABLE` | `rules/backend/TEST_POLICY.md` |
| Penjaga statis dari sisi FE membaca source BE (`payroll-installment-deduction-guards`, `payroll-run-screens`, `payroll-module-contract`) | 6 + 11 + 12 uji lulus | `PASS` | `node --test` pada berkas tes |

**Tidak dijalankan:** `dotnet restore/build/test`, `dotnet ef ...`, eksekusi database, server/API — dilarang pemilik. Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Deterministik dan snapshot efektif | Terpenuhi (statis) | Urutan hitung tetap; `PolicySnapshotJson` berisi versi mesin dan kebijakan |
| Period cutoff, detail komponen, prorata | Terpenuhi (statis) | Jendela hari = irisan periode, masa kerja, profil; cutoff diteruskan ke impor HRD |
| Error item | Terpenuhi | Status `Error` + pesan per penerima menjadi penahan pengajuan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Rumus PPh/BPJS memakai tetapan profil (tidak ada tarif tertanam) dan prorata/cuti tanpa gaji adalah asumsi `NEEDS CONFIRMATION`. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Sahkan rumus pajak/iuran dan kebijakan prorata; build dan uji kalkulasi oleh pihak berwenang. |
