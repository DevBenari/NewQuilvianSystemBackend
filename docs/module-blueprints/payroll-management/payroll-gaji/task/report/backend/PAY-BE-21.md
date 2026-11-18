# Laporan Perubahan Backend — `PAY-BE-21`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-21` |
| Judul | Simulasi |
| Slice | `MVP-1` — `EPIC PGJ-05` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-21` |
| Trace | FR-PGJ-040, 041, 042; PRL-DEC-32; `../../../02-backend-architecture.md` bagian 5.5; `../../../contracts/api-contract.md` (`POST /simulations`, `PayrollSimulationRequest`/`Response`); `../../../contracts/validation-matrix.md` `SIM-01`–`SIM-03`; `../../../contracts/permission-audit-matrix.md` (`PayrollRun : Simulate`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-20` 🟡 (aturan hitung input manual sudah final di mesin hitung) |
| Klasifikasi | `HIGH` — skor 7: repository 0, berkas diperiksa 2 (jalur impor HR, Finance, logger), berkas diubah 1 (2 baru + 3 diubah), logika bisnis 1, kontrak API 2 (endpoint baru dan delta isi), database 1 (transaksi yang selalu dibatalkan), keamanan 0 (action baru jenis Read), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `PayrollSimulationService.cs`, `PayrollSimulationDtos.cs` (Baru); `PayrollRunsController.cs`, `PayrollCalculationService.cs`, `Program.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`20` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai dengan cakupan yang **dipersempit atas keputusan pemilik** (tanpa impor HR); kriteria 2–4 terbukti statis; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji runtime hitung baris menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); run sementara memakai `PrlPayrollRun` yang sudah ada |
| Keberlakuan | `NEW CODE` (service, DTO), `TOUCHED LEGACY` (`PayrollRunsController`, `PayrollCalculationService`) |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service), `QBE-CODE-002/003/006` (tidak ada nomor bisnis yang diterbitkan; nomor run sementara dari GUID dan tidak pernah tersimpan), `QBE-NAM-001` |
| Hak akses | Action baru `Simulate` pada `ControllerName = "PayrollRun"`, `AccessType = Read` (matriks hak akses). Tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Petugas Payroll tidak punya cara melihat hasil hitung gaji sebelum membuka run resmi. Satu-satunya
cara adalah membuka run, sehingga run percobaan, nomor, dan data impor tertinggal.

## 2. Proses bisnis

**Tujuan.** Petugas melihat hasil hitung percobaan satu periode untuk sekelompok pegawai, tanpa
satu pun data tersimpan (PRL-DEC-32).

**Langkah:**

1. Petugas memilih periode dan, bila mau, pegawai tertentu (paling banyak `SimulationMaxEmployees`,
   bawaan 50).
2. Sistem mengambil pegawai layak hitung dengan aturan yang sama dengan run resmi: profil payroll
   aktif, eligible, berstatus `Active`, berlaku di periode, dan punya identitas pegawai atau dokter.
3. Sistem membuka transaksi, membuat run sementara berjenis `Simulation`, lalu menjalankan mesin
   hitung yang sama dengan run resmi.
4. Sistem membaca hasilnya: komponen, bruto, potongan, dan bersih per pegawai, beserta peringatan
   dan galat per pegawai.
5. Transaksi **selalu dibatalkan**, baik hitungnya berhasil maupun gagal.

**Temuan saat eksekusi dan keputusan pemilik (2026-11-12).** Bagian 5.5 arsitektur menganggap
impor HR boleh dipakai di dalam simulasi. Penelusuran source membuktikan sebaliknya:

| Service | Temuan | Akibat bila dipakai |
| --- | --- | --- |
| `AttendancePayrollHandoffService.ExecuteAsync` | Membuka transaksi sendiri (baris 315) dan mengunci absensi harian (`daily.IsLocked = true`, baris 437 dan 480) | Di dalam transaksi simulasi: EF menolak transaksi bertingkat. Di luar: absensi HR terkunci permanen |
| `LeavePayrollIntegrationService.ExecuteAsync` | Membuka transaksi sendiri (baris 428) | Sama |
| `OvertimePayrollHandoffService.PostAsync` | Membuka transaksi sendiri (baris 60) | Sama |
| `PayrollInstallmentDeductionService` | Bergantung pada `FinanceReceivablePayrollSyncService` | Dilarang kriteria 4 |

Pemilik memilih **simulasi tanpa impor HR**. Simulasi menghitung gaji pokok, tunjangan dari
struktur atau cadangan profil, transport bulanan tetap, serta pajak dan iuran tetapan profil.
Kehadiran, lembur, cuti, penghasilan variabel, dan potongan cicilan tidak ikut. Ketiganya
disebut terang di `notes` respons.

**Contoh:** petugas mensimulasikan November 2026 untuk 3 pegawai. Pegawai A bergaji pokok
Rp 6.000.000 dengan struktur berisi Tunjangan Jabatan Rp 750.000 dan transport Rp 400.000.
Simulasi menampilkan tiga komponen penghasilan, pajak dan iuran dari profil, serta bersih
sebelum cicilan. Sesudahnya, jumlah baris run di database tetap sama.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Periode tidak ada atau terhapus | 404 `SIM-01` "Periode penggajian tidak ditemukan." |
| Lebih dari 50 pegawai dipilih | 400 `SIM-02` "Simulasi paling banyak 50 pegawai sekali jalan." |
| Tidak ada pegawai layak hitung | 400 `SIM-03` "Tidak ada pegawai dengan penetapan gaji yang berlaku di periode ini." |
| Pegawai tidak dipilih dan yang layak lebih dari 50 | Diambil 50 pertama menurut nama; dijelaskan di `notes` |
| Pegawai yang dipilih tidak punya profil aktif | Dilewati; jumlahnya dijelaskan di `notes` |
| Satu pegawai bergalat (mis. penetapan gaji belum disetujui) | Pegawai itu tampil dengan `errors[]` dan tidak ikut dijumlah; pegawai lain tetap tampil |
| Mesin hitung melempar galat tak terduga | Transaksi tetap dibatalkan (`finally`), lalu galat diteruskan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollRunService.cs` (`CreateAsync`, `SeedEmployeesAsync`, `CalculateAsync`, nomor run dari GUID),
`PayrollHrdImportService.cs`, `AttendancePayrollHandoffService.cs`, `LeavePayrollIntegrationService.cs`,
`OvertimePayrollHandoffService.cs`, `OvertimePayrollHandoffQueryService.cs`,
`PayrollInstallmentDeductionService.cs`, `PayrollCalculationService.cs`, `PrlPayrollRunConfiguration.cs`
(index unik hanya untuk `Regular`, tanpa check constraint jenis run), `PrlPayrollEmployeeComponent.cs`,
`MstPayrollPeriod.cs`, `PayrollPolicyOptions.cs`, `LoggerService.cs` (menulis lewat `ILogger`, bukan
database), `Program.cs` (tidak ada `SaveChangesInterceptor` pada aplikasi), `PayrollRunsController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollSimulationService.cs` | Baru | Pemilihan pegawai, `SIM-01`–`SIM-03`, batas pegawai, run sementara di transaksi yang selalu dibatalkan, pembacaan hasil, log tanpa nominal |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollSimulationDtos.cs` | Baru | `PayrollSimulationRequest`, `PayrollSimulationResponse`, `PayrollSimulationEmployeeResponse`, `PayrollSimulationComponentResponse` |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollRunsController.cs` | Diperbarui | `POST /simulations` (`PayrollRun : Simulate`); injeksi `PayrollSimulationService` |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | Diperbarui (tambahan) | `CalculationResult.EmployeeWarnings` dan `EmployeeErrors` per baris pegawai. Perilaku hitung tidak berubah. Di luar daftar berkas kartu; dibutuhkan agar `warnings[]`/`errors[]` per pegawai tidak bergantung pada nama yang bisa kembar |
| `Program.cs` | Diperbarui | `AddScoped<PayrollSimulationService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru. Delta isi di bagian 4 |
| Database | Tidak ada perubahan skema. Run sementara, baris pegawai, dan komponen ditulis lalu **selalu di-rollback**; status periode tidak disentuh (dibaca `AsNoTracking`) |
| Keamanan/Auth | Action baru `PayrollRun : Simulate` (Read). Admin harus memberikannya di Akses Role |

## 4. Dokumentasi endpoint

### `[Tags("Corporate / Payroll Management / Payroll Run")]`

Base URL: `api/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
|---|---|---|---|---|---|---|
| `POST` | `/simulations` | Hitung percobaan satu periode tanpa menyimpan apa pun | `PayrollRun : Simulate` (Read) | `PayrollSimulationRequest` | `PayrollSimulationResponse` | 200, 400 (`SIM-02`, `SIM-03`), 403, 404 (`SIM-01`) |

**Contoh request:**

```json
{ "payrollPeriodId": "8f0c…", "workforceProfileIds": ["1a2b…", "3c4d…"] }
```

**Contoh respons (dipotong, data samaran):**

```json
{
  "periodCode": "2026-11",
  "policyVersion": "PAYROLL-POLICY-2026.11-DRAFT",
  "employeeCount": 2,
  "totalGrossPay": 14300000.00,
  "totalDeduction": 1100000.00,
  "totalNetPay": 13200000.00,
  "notes": [
    "Simulasi tidak mengimpor kehadiran, lembur, dan cuti dari HR, sehingga komponen dari sumber itu tidak muncul.",
    "Simulasi tidak memuat penghasilan variabel dan potongan cicilan piutang; potongan yang tampil adalah potongan sebelum cicilan.",
    "Hasil simulasi tidak disimpan."
  ],
  "employees": [
    {
      "workforceProfileId": "1a2b…",
      "employeeNumber": "EMP-0001",
      "employeeName": "Pegawai Contoh A",
      "grossPay": 7150000.00,
      "totalDeduction": 550000.00,
      "netPay": 6600000.00,
      "components": [
        { "code": "PAY_BASE_SALARY", "name": "Gaji Pokok", "type": "Earning", "amount": 6000000.00, "isTaxable": true },
        { "code": "PAY_STRUCTURE_ALLOWANCE", "name": "Tunjangan Jabatan", "type": "Earning", "amount": 750000.00, "isTaxable": true },
        { "code": "PAY_TRANSPORT_ALLOWANCE", "name": "Tunjangan Transportasi", "type": "Earning", "amount": 400000.00, "isTaxable": true }
      ],
      "warnings": [],
      "errors": []
    }
  ]
}
```

**Delta terhadap kontrak `PGJ-C1`:**

| Delta | Alasan |
| --- | --- |
| Simulasi **tidak mengimpor HR** dan tidak memuat penghasilan variabel serta cicilan | Keputusan pemilik 2026-11-12; jalur impor HR meninggalkan jejak (bagian 2). Bagian 5.5 arsitektur perlu direvisi lewat `design-business-module` |
| Respons menambah `notes[]` dan `employees[].employeeNumber` | `notes` menjelaskan sumber yang tidak ikut, supaya hasil tidak disangka lengkap; nomor pegawai memudahkan petugas mencocokkan |
| `totalDeduction`/`netPay` adalah nilai sebelum cicilan piutang | Cicilan butuh sinkronisasi Finance, yang dilarang kriteria 4 |
| Total hanya menjumlah pegawai yang berhasil dihitung | Pegawai bergalat bisa membawa angka setengah jadi |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kriteria 4: rujukan number series, sinkronisasi Finance, impor HR, service cicilan, `IDbContextFactory`, transaksi lain di jalur simulasi | 0 di `PayrollCalculationService`; di `PayrollSimulationService` hanya transaksinya sendiri dan rollback | `PASS` | `grep` |
| Kriteria 2: penulisan hanya di dalam transaksi yang selalu dibatalkan | Dua `SaveChangesAsync` di dalam `try`; `RollbackAsync` dan `ChangeTracker.Clear()` di `finally`; periode hanya dibaca | `PASS` | Review source |
| Tidak ada penulis ke database di luar `DbContext` | Tanpa `SaveChangesInterceptor`; `LoggerService` menulis ke `ILogger` | `PASS` | Review `Program.cs`, `LoggerService.cs` |
| Index unik run tidak berbenturan | Filter hanya `RunType = 'Regular'`; tanpa check constraint jenis run | `PASS` | `PrlPayrollRunConfiguration.cs` |
| Kriteria 3: lebih dari batas pegawai | 400 `SIM-02` sebelum query apa pun selain periode | `PASS` | Review source |
| Kriteria 1: mesin hitung yang sama | `PayrollCalculationService.CalculateRunAsync` yang sama dengan run resmi; pegawai dipilih dengan aturan `SeedEmployeesAsync` | `PASS` (statis, cakupan dipersempit) | Review source |
| Atribut hak akses `PayrollRunsController` | 21/21 endpoint konsisten | `PASS` | Skrip pemeriksa |
| Bentrok rute | `simulations` satu-satunya `POST` tanpa id | `PASS` | `grep` |
| Keseimbangan kurung | 0 selisih di 4 berkas | `PASS` | Skrip penghitung |
| Uji runtime hitung baris sebelum dan sesudah | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build dan setelah admin memberi hak
`PayrollRun : Simulate`.

**Langkah uji untuk pemilik** (DB uji, data samaran):

1. Catat sebelum: `SELECT COUNT(*) FROM "PrlPayrollRun"`, `"PrlPayrollRunEmployee"`,
   `"PrlPayrollEmployeeComponent"`, `"PrlPayrollVariableInput"`, dan `"IsLocked"` periode uji.
2. `POST /simulations` untuk periode uji dengan 3 pegawai. Harapan: 200, tiga pegawai beserta
   komponen, dan `notes` berisi tiga kalimat.
3. Ulangi langkah 1. Harapan: semua angka sama, dan tidak ada baris deret `NumNumberSeries` baru.
4. Kriteria 1: buka run resmi untuk periode yang sama tanpa impor HR, lalu hitung. Harapan: gaji
   pokok, tunjangan struktur, transport, pajak, dan iuran per pegawai sama dengan simulasi.
5. `POST /simulations` dengan 51 `workforceProfileIds`. Harapan: 400 `SIM-02`.
6. Periode yang tidak ada. Harapan: 404 `SIM-01`.
7. Akun tanpa hak `PayrollRun : Simulate`. Harapan: 403.

**Tidak dijalankan:** `dotnet build`, uji runtime, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Hasil per pegawai sama dengan hasil run resmi untuk data yang sama | **Dipersempit atas keputusan pemilik:** sama untuk komponen yang tidak bergantung pada impor HR, input variabel, dan cicilan (mesin hitung yang sama). Tidak berlaku untuk kehadiran, lembur, cuti, input variabel, dan cicilan | Bagian 2 dan 5 |
| 2. Jumlah baris run, input variabel, dan status periode sama sebelum dan sesudah | Terpenuhi (statis); uji runtime menunggu | Bagian 5 |
| 3. Lebih dari batas pegawai: 400 `SIM-02` | Terpenuhi (statis) | Bagian 5 |
| 4. Tidak ada pemanggilan number series atau sinkronisasi Finance | Terpenuhi (statis) | Bagian 5 |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: empat kriteria terbukti | Kriteria 2–4 statis; kriteria 1 dipersempit; uji runtime **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perlu tindak lanjut desain | Revisi `02-backend-architecture.md` bagian 5.5 dan kontrak `PayrollSimulationResponse` (tambahan `notes`, `employeeNumber`, cakupan tanpa impor HR) lewat `design-business-module` sebagai amandemen `PGJ-C1`. Builder tidak berwenang mengubah kontrak yang sudah disetujui |
| Pilihan lanjutan bila simulasi perlu lengkap | Membaca HR lewat method `Preview` baca-saja milik modul HR lalu memetakannya ke snapshot di memori. Pilihan ini menyalin logika pemetaan HR dan perlu dirancang bersama pemilik HR lebih dulu |
| Masalah yang diketahui | (1) Simulasi memakai koneksi database selama hitung berlangsung; 50 pegawai seharusnya cepat, tetapi transaksi terbuka menahan kunci baris yang dibaca mesin hitung sampai rollback. (2) Folder `Tests/` lama (`QuilvianSystemBackend.PatientManagementTests`) ada di repository; tidak disentuh task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu pertanyaan ke pemilik soal cakupan simulasi; dijawab "Tanpa impor HR" |
| Status Git | Task ini: `??` dua berkas baru; `M` `PayrollRunsController.cs`, `PayrollCalculationService.cs` (bersama `PAY-BE-17`/`18`/`20`), `Program.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`20` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build, beri hak `PayrollRun : Simulate`, langkah uji bagian 5. Task berikutnya: `PAY-BE-22` (laporan beban gaji, PPh/BPJS) atau `PAY-BE-23` (rekap lembur) |
