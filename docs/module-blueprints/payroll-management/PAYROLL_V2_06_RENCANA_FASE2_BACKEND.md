# PAYROLL V2 — 06. Rencana Fase 2 Backend (untuk ditinjau pemilik)

| Hal | Isi |
| --- | --- |
| Tanggal | 11 November 2026 |
| Status | **DIKERJAKAN secara statis pada 11 November 2026 (🟡: `dotnet build` dan eksekusi migration menunggu pemilik).** Bagian 1 sampai 10 adalah rencana asli dan **dibiarkan apa adanya sebagai riwayat** (tulisan "belum disentuh" di dalamnya benar pada saat ditulis); hasil dan selisihnya dari rencana ada di **bagian 11**. Semula dokumen ini berhenti pada rencana atas pilihan pemilik "Catat keputusan + kerjakan frontend; backend jadi rencana"; pemilik kemudian memerintahkan "kerjakan fase 2 backend". Laporan: [`PAY-BE-12`](task/report/backend/PAY-BE-12.md) dan [`PAY-MIG-02`](task/report/backend/PAY-MIG-02.md) |
| Task | `PAY-BE-12` (registry dan pemindahan source), `PAY-MIG-02` (migration rename) |
| Dasar keputusan | `PAYROLL_V2_01` bagian 7.3 (jawaban pemilik 11 November 2026) |
| Label bukti | `REPOSITORY FACT` = dibaca dari source atau dihitung dari repository pada 11 November 2026; `TARGET PROPOSAL` = usulan, belum diterapkan |
| Pembuktian | Semua hitungan di bagian 3 berasal dari pembacaan dan pencarian teks, bukan dari kompilasi. `dotnet build` dan eksekusi database tidak dijalankan oleh agen |

---

## 1. Ringkasan untuk pemilik

Keputusan Anda: 19 entity Payroll yang sekarang berada di folder HRD (13 `Trx*` dan 6 `Wfp*`) dipindah ke modul Payroll, **semuanya diganti nama ke awalan `Prl`** (nama kelas dan nama tabel), lewat migration yang hanya mengganti nama (tidak menghapus data). Modul Payroll didaftarkan di registry dengan awalan `Prl` berstatus `ACTIVE`.

Hasil pemeriksaan dampak membawa **satu hal yang perlu Anda putuskan ulang sebelum eksekusi** (bagian 3.2): lima dari enam entity `Wfp*` bukan sekadar data Payroll. Mereka adalah bagian dari agregat profil pegawai milik HR (`MstWorkforceProfile`), sehingga mengganti namanya ikut mengubah entity inti HR dan beberapa controller HR. Pilihan Anda tetap tercatat apa adanya; bagian 9 memuat cara membatalkannya bila Anda berubah pikiran.

Di luar itu, penggantian nama 13 entity `Trx*` relatif sempit dan mengikuti pola yang sudah dipakai pada 11 migration rename di repository ini (IGD, Rekam Medis, Laboratorium, Farmasi, Registrasi, Gizi, dan lain-lain).

## 2. Keputusan yang menjadi dasar

| No | Keputusan | Sumber |
| ---: | --- | --- |
| 1 | Registry: `Corporate / PayrollManagement / Payroll`, awalan `Prl`, `ACTIVE` | `PAYROLL_V2_01` 7.3 butir 1 |
| 2 | 13 entity `Trx*` diganti ke `Prl*`, kelas dan tabel, lewat migration rename | butir 2 |
| 3 | 6 entity `Wfp*` ikut pindah dan diganti ke `Prl*`, kelas dan tabel | butir 3 |
| 4 | Backend dikerjakan sebagai rencana yang ditinjau pemilik dulu | butir 5 |

## 3. Temuan dampak (`REPOSITORY FACT`)

### 3.1 Ukuran perubahan

| Hal | Jumlah |
| --- | ---: |
| Entity yang pindah dan diganti nama | 19 (13 `Trx*`, 6 `Wfp*`) |
| Berkas model yang dipindah (`Areas/Corporate/HumanResource/PayrollManagement/Models/`) | 19 |
| Berkas konfigurasi EF yang ikut diganti nama (`Repositories/Configurations/Corporate/HumanResource/PayrollManagement/`) | 19 |
| `DbSet` pada `Repositories/ApplicationDbContext.cs` | 19 properti |
| Berkas non-migrasi **lain** yang menyebut salah satu entity (akan diubah namanya di dalamnya) | 54, termasuk 19 konfigurasi, `ApplicationDbContext.cs`, dan `ApplicationDbContextModelSnapshot.cs` |
| Berkas migration lama yang menyebut nama entity di bagian Designer-nya | sekitar 270 sampai 340 per entity (riwayat). Tidak diubah: riwayat migration tidak ditulis ulang |

Sebaran 54 berkas menurut modul:

| Modul | Berkas |
| --- | ---: |
| `Repositories/Configurations` | 19 |
| `Areas/Corporate/PayrollManagement` (Services 9, Constants 2, Options 1, DTOs 1) | 13 |
| `Areas/Corporate/HumanResource/PayrollManagement` (enam controller `Wfp*` yang menyebut entity; enam DTO di folder itu hanya memuat nama DTO sendiri dan ikut pindah tanpa perubahan isi) | 6 |
| `Areas/Corporate/HumanResource/MasterData` | 4 |
| `Areas/Corporate/HumanResource/OvertimeManagement` | 3 |
| `Areas/Corporate/FinanceManagement/Receivable` | 2 |
| `Areas/Corporate/HumanResource/BenefitManagement` | 2 |
| `AttendanceManagement`, `LeaveManagement`, `WorkforceProfileManagement` (masing-masing 1) | 3 |
| `Repositories/ApplicationDbContext.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs` | 2 |

Tidak ditemukan: tabel lain yang namanya memakai awalan yang sama (`TrxPayroll*`, `TrxMedicalServiceFee*`, `WfpTax*`, `WfpInsurance*`, `WfpPayroll*`, `WfpTransportAllowance*` di luar 19 ini), nama yang sudah memakai awalan `Prl`, maupun skrip `.sql`, `.ps1`, `.sh`, atau `.json` di luar migration yang menyebut nama tabel lama.

### 3.2 Entity `Wfp*` menyentuh inti HR (perlu keputusan ulang)

`REPOSITORY FACT`: lima entity berikut adalah properti navigasi pada `MstWorkforceProfile` (profil pegawai), dan dipakai langsung oleh controller HR.

| Entity `Wfp*` | Dipakai oleh (di luar modul Payroll) |
| --- | --- |
| `WfpInsurance` | `MstWorkforceProfile`, `EmployeeController`, `WorkforceDetailController`, `TrxEmployeeInsuranceEnrollment` (BenefitManagement) |
| `WfpPayroll` | `MstWorkforceProfile`, `EmployeeController`, `WorkforceDetailController`, `PayrollPeriodController` dan `SalaryStructureController` (HR MasterData), `AttendancePayrollHandoffService` |
| `WfpTax` | `MstWorkforceProfile`, `EmployeeController`, `WorkforceDetailController` |
| `WfpTransportAllowance` | `MstWorkforceProfile`, `EmployeeController`, `WorkforceDetailController` |
| `WfpTransportAllowanceTransaction` | `MstWorkforceProfile`, `EmployeeController` |
| `WfpTransportAllowancePolicy` | hanya `ApplicationDbContext`, konfigurasi, dan controller/DTO Payroll sendiri |

Akibat bila `Wfp*` diganti ke `Prl*`:

1. Entity inti HR (`MstWorkforceProfile`, awalan `Mst`, pemilik HR) akan memuat properti bertipe `Prl*`, yaitu entity milik modul lain. Arah ketergantungan HR ke Payroll belum ada sebelumnya.
2. `EmployeeController` dan `WorkforceDetailController` (layar pegawai HR) ikut diubah.
3. Awalan `Wfp` terdaftar di registry sebagai milik `Corporate / WorkforceCore` (`ACTIVE / LEGACY`). Menghapus enam entity dari pemiliknya mengubah arti kepemilikan itu, dan registry perlu mencatatnya.
4. QBE tidak mewajibkan penggantian nama ini (`Wfp` adalah awalan terdaftar).

Pendapat agen, bukan keputusan: pindah folder dan ganti nama `Trx*` (13 entity) dikerjakan lebih dulu sebagai satu paket; enam `Wfp*` dikerjakan sebagai paket terpisah sesudah Anda menegaskan bahwa data pajak, iuran, dan tunjangan transport pegawai memang dimiliki Payroll, bukan profil pegawai HR. Pilihan itu bisa dijalankan tanpa mengubah rencana lain (bagian 9, butir 1).

### 3.3 Entity `Trx*` dipakai modul lain

Delapan berkas di luar Payroll memakai entity `Trx*` Payroll dan harus ikut diubah namanya di dalamnya:

| Berkas | Entity |
| --- | --- |
| `FinanceManagement/Receivable/Services/FinanceReceivableInstallmentPlanService.cs` | `TrxPayrollVariableInput` |
| `FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs` | `TrxPayrollRun`, `TrxPayrollRunEmployee`, `TrxPayrollVariableInput` |
| `HumanResource/AttendanceManagement/Services/AttendancePayrollHandoffService.cs` | `TrxPayrollAttendanceInput`, `TrxPayrollRun`, `TrxPayrollRunEmployee` |
| `HumanResource/BenefitManagement/Models/TrxEmployeeLoanInstallment.cs` | `TrxPayrollRunEmployee` (properti navigasi) |
| `HumanResource/LeaveManagement/Services/LeavePayrollIntegrationService.cs` | `TrxPayrollAttendanceInput`, `TrxPayrollRun`, `TrxPayrollRunEmployee`, `TrxPayrollVariableInput` |
| `HumanResource/OvertimeManagement/Controllers/OvertimePayrollHandoffController.cs` | `TrxPayrollOvertimeInput` |
| `HumanResource/OvertimeManagement/Services/OvertimePayrollHandoffQueryService.cs` | `TrxPayrollOvertimeInput`, `TrxPayrollRun`, `TrxPayrollRunEmployee` |
| `HumanResource/OvertimeManagement/Services/OvertimePayrollHandoffService.cs` | `TrxPayrollOvertimeInput`, `TrxPayrollRun`, `TrxPayrollRunEmployee` |

Tidak ada perubahan perilaku; hanya nama tipe.

## 4. Pemetaan nama (`TARGET PROPOSAL`)

Aturan usulan: **ganti awalan saja** (`Trx` atau `Wfp` menjadi `Prl`), karena ketiganya tiga huruf sehingga panjang nama tidak berubah dan tidak ada nama yang tiba-tiba terpotong batas 63 karakter PostgreSQL. Menyingkat nama (misalnya `PrlRun` alih-alih `PrlPayrollRun`, seperti `TrxMedicalRecordAccessLog` menjadi `MrcAccessLog`) adalah keputusan penamaan terpisah dan bukan syarat QBE; tidak diusulkan sekarang (bagian 9, butir 2).

| Kelas dan tabel lama | Kelas dan tabel baru | `DbSet` baru | Konfigurasi baru |
| --- | --- | --- | --- |
| `TrxMedicalServiceFeeCalculation` | `PrlMedicalServiceFeeCalculation` | `PrlMedicalServiceFeeCalculations` | `PrlMedicalServiceFeeCalculationConfiguration` |
| `TrxMedicalServiceFeePayment` | `PrlMedicalServiceFeePayment` | `PrlMedicalServiceFeePayments` | `PrlMedicalServiceFeePaymentConfiguration` |
| `TrxPayrollAdjustment` | `PrlPayrollAdjustment` | `PrlPayrollAdjustments` | `PrlPayrollAdjustmentConfiguration` |
| `TrxPayrollApproval` | `PrlPayrollApproval` | `PrlPayrollApprovals` | `PrlPayrollApprovalConfiguration` |
| `TrxPayrollAttendanceInput` | `PrlPayrollAttendanceInput` | `PrlPayrollAttendanceInputs` | `PrlPayrollAttendanceInputConfiguration` |
| `TrxPayrollEmployeeComponent` | `PrlPayrollEmployeeComponent` | `PrlPayrollEmployeeComponents` | `PrlPayrollEmployeeComponentConfiguration` |
| `TrxPayrollOvertimeInput` | `PrlPayrollOvertimeInput` | `PrlPayrollOvertimeInputs` | `PrlPayrollOvertimeInputConfiguration` |
| `TrxPayrollPayment` | `PrlPayrollPayment` | `PrlPayrollPayments` | `PrlPayrollPaymentConfiguration` |
| `TrxPayrollPayslip` | `PrlPayrollPayslip` | `PrlPayrollPayslips` | `PrlPayrollPayslipConfiguration` |
| `TrxPayrollReversal` | `PrlPayrollReversal` | `PrlPayrollReversals` | `PrlPayrollReversalConfiguration` |
| `TrxPayrollRun` | `PrlPayrollRun` | `PrlPayrollRuns` | `PrlPayrollRunConfiguration` |
| `TrxPayrollRunEmployee` | `PrlPayrollRunEmployee` | `PrlPayrollRunEmployees` | `PrlPayrollRunEmployeeConfiguration` |
| `TrxPayrollVariableInput` | `PrlPayrollVariableInput` | `PrlPayrollVariableInputs` | `PrlPayrollVariableInputConfiguration` |
| `WfpInsurance` | `PrlInsurance` | `PrlInsurances` | `PrlInsuranceConfiguration` |
| `WfpPayroll` | `PrlPayroll` | `PrlPayrolls` | `PrlPayrollConfiguration` |
| `WfpTax` | `PrlTax` | `PrlTaxes` | `PrlTaxConfiguration` |
| `WfpTransportAllowance` | `PrlTransportAllowance` | `PrlTransportAllowances` | `PrlTransportAllowanceConfiguration` |
| `WfpTransportAllowancePolicy` | `PrlTransportAllowancePolicy` | `PrlTransportAllowancePolicies` | `PrlTransportAllowancePolicyConfiguration` |
| `WfpTransportAllowanceTransaction` | `PrlTransportAllowanceTransaction` | `PrlTransportAllowanceTransactions` | `PrlTransportAllowanceTransactionConfiguration` |

Tidak ada tabrakan: tidak ada nama `Prl*` yang sudah ada di source maupun snapshot, dan tidak ada dua nama lama yang menjadi nama baru yang sama.

Entity lain yang berhubungan dan **tidak** ikut: `Mst*` data induk Payroll (awalan `Mst` sah untuk master/referensi) dan entity milik modul lain yang hanya memiliki kunci asing ke tabel ini.

Berkas lain yang ikut berganti nama: 6 controller `Wfp*Controller` dan 6 `Wfp*Dtos.cs` di `Areas/Corporate/HumanResource/PayrollManagement/` pindah ke `Areas/Corporate/PayrollManagement/`. Nama kelas controller dan DTO, rute `payroll-management/...`, serta `ControllerName` pada atribut akses **tidak berubah** supaya hak akses di Akses Role tidak putus (`role-access-rules`: argumen `AccessController` harus sama persis dengan yang tersimpan). Penggantian nama controller adalah keputusan terpisah dan tidak diusulkan.

## 5. Langkah eksekusi (urutan)

Siapa yang melakukan: **[Pemilik]** hanya Anda; **[Agen]** saya, setelah wewenang dan rencana disetujui.

| No | Langkah | Pelaku | Keluaran | Berhenti bila |
| ---: | --- | --- | --- | --- |
| 0 | Branch khusus fase 2, dibuat pemilik. Cadangan database uji. `dotnet build` baseline hijau dilaporkan pemilik | [Pemilik] | Nama branch, bukti build | Build baseline merah (kesalahan lama harus beres lebih dulu) |
| 1 | Pastikan `20261104071700_AddPayrollIntegrityConstraints` sudah dijalankan atau belum, dan jalankan lebih dulu bila belum | [Pemilik] | Jawaban | Migration itu belum diterapkan dan Anda ingin melompatinya (lihat bagian 9, butir 4) |
| 2 | Tambah baris registry `Corporate | PayrollManagement / Payroll | BUSINESS DOMAIN / MODULE | Prl | ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` beserta satu baris catatan perubahan (tanggal, persetujuan pemilik, "Prl = Payroll", pencabutan penghalang `QBE-MOD-002`). Salinan registry pada suite skill disamakan oleh pemilik supaya kedua salinan tidak berselisih | [Agen] untuk salinan repository; [Pemilik] untuk salinan suite | Dua baris registry identik | Pemilik belum menyetujui baris persis ini |
| 3 | Pindahkan 19 berkas model, 19 konfigurasi, 6 controller, 6 DTO ke `Areas/Corporate/PayrollManagement/` dan `Repositories/Configurations/Corporate/PayrollManagement/` memakai `mv` biasa; ganti namespace | [Agen] | Struktur folder | — |
| 4 | Ganti nama kelas, berkas, konfigurasi, `DbSet`, `ToTable(...)`, dan seluruh rujukan (54 berkas, bagian 3.1) sesuai pemetaan bagian 4. Nama kolom, tipe, kunci asing, dan nama properti navigasi **tidak** berubah | [Agen] | Source yang konsisten | Pencarian menemukan nama lama tersisa |
| 5 | Tulis migration `RenamePayrollTrxAndWfpTablesToPrlPrefix` (manual, pola bagian 6) beserta `.Designer.cs` dan perbarui `ApplicationDbContextModelSnapshot.cs` secara manual | [Agen], hanya setelah wewenang eksplisit pemilik untuk menulis berkas migration | Tiga berkas migration | Wewenang belum diberikan |
| 6 | `dotnet build`, pemeriksaan QBE conformance mode `GitRange` lokal, dan pemeriksaan "tidak ada perubahan model tertunda" | [Pemilik] | Keluaran build | Ada kesalahan; perbaikan oleh agen lalu ulangi |
| 7 | Terapkan migration ke database uji, jalankan pemeriksaan SQL bagian 7, lalu uji `Down` dan `Up` ulang | [Pemilik] | Hasil SQL | Jumlah baris atau nama constraint tidak cocok |
| 8 | Perbarui `PAYROLL_V2_05` (SQL pra-cek dan skrip uji memakai nama tabel baru) dan laporan task | [Agen] | Dokumen | — |
| 9 | Rilis satu paket: database dulu (migration), lalu aplikasi (aturan yang sama dengan `FIN-DES-138`) | [Pemilik] | — | — |

## 6. Rancangan migration (`TARGET PROPOSAL`)

Memakai **pola yang sudah dipakai di repository ini**: `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` (satu dari 11 migration rename di `Migrations/`; yang lain untuk IGD, Laboratorium, Farmasi, Registrasi, Gizi, dst.). Isinya satu blok PostgreSQL yang:

1. Mengganti nama tabel (`ALTER TABLE ... RENAME TO`) hanya bila tabelnya ada.
2. Mengganti nama setiap constraint yang namanya memuat nama tabel lama, termasuk kunci asing milik tabel lain yang menunjuk ke sini; mengganti nama constraint PK atau unique sekaligus mengganti nama indeks penopangnya.
3. Mengganti nama indeks sisa yang tidak ditopang constraint (mencakup `UX_TrxPayrollPayment_LiveInstruction`, `UX_TrxPayrollRun_RegularPeriod`, dan `UX_TrxPayrollVariableInput_MedicalFeeClaim` dari `PAY-MIG-01`).

Nama constraint dan indeks **dicari dari katalog PostgreSQL**, tidak diketik satu per satu, karena sebagian nama buatan EF terpotong pada 63 karakter dan berakhiran `~`; menuliskannya manual mengundang salah ketik yang baru ketahuan saat dijalankan (alasan yang sama ditulis di migration Rekam Medis).

| Sifat | Rancangan |
| --- | --- |
| Destruktif | Tidak. Seluruhnya `RENAME`; tidak ada `DROP` maupun `CREATE` (`QBE-DB-002`). Isi tabel utuh, termasuk riwayat run, slip, dan pembayaran |
| `Down` | Memakai peta terbalik (`Prl*` kembali ke nama lama) dengan blok yang sama |
| Jumlah pasangan | 19 |
| Panjang nama | Sama (tiga huruf diganti tiga huruf), sehingga langkah "rapikan nama terpotong" pada migration Rekam Medis tidak diperlukan |
| Idempoten | Tahap rename tabel melewati tabel yang tidak ada, jadi aman diulang pada database yang sudah memakai nama baru |
| Pra-cek wajib | Sebelum rename, jalankan kueri yang mencantumkan setiap constraint dan indeks yang namanya memuat salah satu nama lama **dan bukan milik** 19 tabel ini atau tabel yang punya kunci asing ke sana. Hasil harus kosong atau sudah dijelaskan: penggantian dilakukan lewat `LIKE '%nama%'`, jadi nama lain yang memuat potongan yang sama ikut berubah |
| Skrip SQL idempoten | Pola migration sebelumnya menyimpan skrip di `Migrations/scripts/`. Pembuatannya (`dotnet ef migrations script`) dilakukan pemilik |
| Eksekusi | Hanya pemilik, pada database uji dahulu. Agen tidak menjalankan `dotnet ef` maupun SQL |

## 7. Pembuktian yang diminta (oleh pemilik, pada database uji)

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Sebelum: hitung baris tiap dari 19 tabel lama | Dicatat |
| 2 | Sesudah: hitung baris tiap dari 19 tabel baru | Sama dengan nomor 1 per tabel |
| 3 | Tabel lama sudah tidak ada | 0 tabel dengan nama lama di skema `public` |
| 4 | Tidak ada constraint dan indeks yang masih memuat `TrxPayroll`, `TrxMedicalServiceFee`, `WfpTax`, `WfpInsurance`, `WfpPayroll`, `WfpTransportAllowance` | 0 baris |
| 5 | Kunci asing dari tabel lain (mis. `TrxEmployeeLoanInstallment` ke `PrlPayrollRunEmployee`) tetap ada dan valid | Semua ada, `convalidated = true` |
| 6 | `Down` lalu `Up` ulang | Nomor 2 sampai 5 tetap cocok |
| 7 | Aplikasi: run, slip, pembayaran, dan layar pegawai HR (profil pajak, iuran, tunjangan transport) memuat data lama | Cocok dengan sebelum migration |

## 8. Risiko dan mitigasi

| No | Risiko | Mitigasi |
| ---: | --- | --- |
| 1 | Aplikasi versi lama gagal total setelah tabel diganti nama (semua kueri ke nama lama) | Rilis satu paket, database dulu lalu aplikasi; jangan gabung ke branch rilis sebelum migration diterapkan |
| 2 | `LIKE` pada nama constraint mengganti nama milik tabel lain | Pra-cek di bagian 6; hasil dibaca pemilik sebelum migration dijalankan |
| 3 | Kesalahan ketik atau nama terlewat pada 54 berkas (tanpa build oleh agen) | Pencarian teks nama lama harus nol sisa; build dan QBE conformance oleh pemilik; agen tidak mengklaim kompilasi hijau |
| 4 | Snapshot migration ditulis manual dan menyimpang dari model | Pemeriksaan "perubahan model tertunda" oleh pemilik; migration lain di repository ini dibuat dengan cara sama |
| 5 | Layar pegawai HR rusak karena properti navigasi `Wfp*` berganti tipe | Hanya relevan bila butir `Wfp*` dijalankan; uji layar pegawai HR dimasukkan ke bagian 7 |
| 6 | Hak akses putus bila nama controller berubah | Nama controller dan atribut akses tidak diubah (bagian 4) |
| 7 | Dokumen dan SQL pra-cek `PAYROLL_V2_05` memakai nama lama | Langkah 8 |

## 9. Pertanyaan terbuka untuk pemilik

| No | Pertanyaan | Usulan agen |
| ---: | --- | --- |
| 1 | Apakah enam entity `Wfp*` tetap diganti ke `Prl*` setelah melihat bagian 3.2 (menyentuh `MstWorkforceProfile`, `EmployeeController`, `WorkforceDetailController`)? | Kerjakan 13 `Trx*` lebih dulu; putuskan `Wfp*` sesudah yang pertama terbukti di database uji. Bila tetap diganti, lakukan sebagai paket kedua |
| 2 | Pola nama: ganti awalan saja (`PrlPayrollRun`) atau singkat (`PrlRun`)? | Ganti awalan saja; penyingkatan bisa batch terpisah |
| 3 | Baris registry persis seperti langkah 2 sudah disetujui? Siapa yang menyamakan salinan registry pada suite skill? | Setuju, salinan suite oleh pemilik registry |
| 4 | Apakah migration `AddPayrollIntegrityConstraints` (`PAY-MIG-01`) sudah dijalankan di database uji? | Jalankan lebih dulu; rename menyusul sesudahnya |
| 5 | Nama branch khusus fase 2 dan jadwal jendela rilis (database dulu, lalu aplikasi) | Ditentukan pemilik |
| 6 | Wewenang menulis berkas migration manual (langkah 5) diberikan kapan? | Saat langkah 3 dan 4 selesai dan Anda siap |

## 10. Yang sengaja tidak dikerjakan

| Hal | Alasan |
| --- | --- |
| Mengubah source backend, registry, migration, atau `QBE_EXCEPTIONS.json` | Pilihan pemilik: backend jadi rencana dulu |
| Pengecualian QBE per berkas untuk `Trx*` | Tidak diperlukan karena seluruh entity diganti nama |
| Mengganti nama controller, rute alias, atau atribut akses | Menjaga hak akses; keputusan terpisah |
| Mengganti nama kolom atau properti navigasi | Di luar keputusan; tidak diperlukan oleh QBE |
| Menjalankan `dotnet build`, `dotnet ef`, atau SQL | Wewenang pemilik |

> Bagian 10 adalah daftar "tidak dikerjakan" **pada saat rencana ditulis**. Sebagian kini sudah dikerjakan (bagian 11); yang tetap tidak dikerjakan agen: `dotnet build`, `dotnet ef`, dan SQL.

---

## 11. Hasil pengerjaan (11 November 2026)

Status: 🟡 selesai secara statis. Pemilik memerintahkan "kerjakan fase 2 backend" (task `PAY-BE-12` dan `PAY-MIG-02`).

### 11.1 Langkah pada bagian 5

| No | Langkah | Hasil |
| ---: | --- | --- |
| 0 | Branch khusus, cadangan database, build baseline | Pemilik. **Tidak ada branch khusus**: pemilik memerintahkan kerja di `Yasmina`. Build baseline belum dilaporkan |
| 1 | `AddPayrollIntegrityConstraints` sudah dijalankan atau belum | Pemilik; belum dijawab. Migration rename tidak bergantung pada urutannya (laporan `PAY-MIG-02` bagian 6.1) |
| 2 | Baris registry `Prl` dan catatan perubahan | **Selesai** (salinan repository). Salinan suite skill menunggu disamakan pemilik |
| 3 | Pindahkan 19 model, 19 konfigurasi, 6 controller, 6 DTO | **Selesai.** 50 berkas dengan `mv`; folder HR yang kosong dihapus |
| 4 | Ganti nama dan rujukan | **Selesai.** 784 penggantian nama dan 181 namespace di 84 berkas; 0 nama lama tersisa; transformasi dibalik identik dengan cadangan pada 84 dari 84 berkas |
| 5 | Migration, Designer, snapshot | **Selesai (ditulis).** Wewenang menulis diberikan oleh instruksi pemilik; tidak dijalankan |
| 6 | `dotnet build`, QBE conformance, perubahan model tertunda | **Pemilik; belum.** `pwsh` tidak tersedia untuk agen, sehingga aturan kepemilikan checker hanya direplikasi (19 dari 19 lolos) |
| 7 | Terapkan ke database uji dan pemeriksaan bagian 7 | **Pemilik; belum.** Skrip siap pakai ada di laporan `PAY-MIG-02` bagian 6 |
| 8 | Perbarui `PAYROLL_V2_05` dan laporan | **Selesai.** `PAYROLL_V2_05` bagian 9; SQL pra-cek lama dibiarkan dengan catatan |
| 9 | Rilis satu paket | Pemilik |

### 11.2 Selisih dari rencana

| Rencana | Kenyataan |
| --- | --- |
| 54 berkas non-migrasi menyebut entity (bagian 3.1) | **59 berkas** (19 konfigurasi, 17 Payroll, 13 HR, 6 controller `Wfp*` yang dipindah, 2 Finance, `ApplicationDbContext.cs`, snapshot). Selisih +5: +4 berkas Payroll (termasuk dua berkas baru `PAY-BE-11`) dan +1 berkas Overtime (`OvertimeFinalReconciliationService`). Bukan perubahan lingkup |
| Usul agen: kerjakan 13 `Trx*` lebih dulu, `Wfp*` paket kedua (bagian 3.2 dan 9 butir 1) | **Satu paket.** Pemilik memerintahkan seluruh 19 pindah; konfirmasi ulang yang disarankan tidak diminta |
| Pra-cek `LIKE '%nama%'` pada migration (bagian 6) | **Pencocokan token** `(^|_)token(?=_|~|$)` ditambah pemeriksaan tabrakan nama tabel. Alasan: pada arah `Down`, `PrlPayroll` adalah awalan `PrlPayrollRun`; substring bebas salah memetakannya. Pra-cek kolateral menjadi tidak diperlukan |
| Branch khusus (langkah 0) | Tidak dibuat; kerja di `Yasmina` |
| Langkah "rapikan nama terpotong" tidak diperlukan karena panjang nama sama (bagian 6) | **Terbukti pada data model:** dari 283 nama terdampak, 67 sudah terpotong `~`, dan tidak ada satu pun yang token tabelnya terpotong di tengah; penggantian awalan tiga huruf tidak menggeser titik potong |
| Frontend tidak disebut | Dua berkas test FE yang membaca source backend ikut diubah (laporan `PAY-BE-12` bagian 3.4) |

### 11.3 Hal yang ditemukan dan tidak diubah

1. Sembilan string `LogCategory` masih `Corporate.HumanResource.PayrollManagement` (enam controller `Wfp*`, tiga service Payroll). Mengubahnya memutus pencarian log lama; keputusan pemilik.
2. Entity `Rhb*` ada pada snapshot dan Designer Rehabilitasi tetapi tidak pada Designer Optik (`20261106`) dan BLM (`20261110`). Sudah ada sebelum pekerjaan ini; Designer baru dibangun dari snapshot (laporan `PAY-MIG-02` bagian 3.3).
3. `docs/system-registry/02-entity-terdaftar.md` (audit berstempel SHA `f2c5090`) masih memuat nama lama; tidak diubah.
4. Perubahan fase 2 bercampur dengan `PAY-BE-11` pada tujuh berkas Payroll; keduanya satu paket pembuktian build (laporan `PAY-BE-12` bagian 3.6).

### 11.4 Permintaan kepada pemilik

Daftar bernomor ada pada laporan `PAY-BE-12` bagian 7 (build, `has-pending-model-changes`, QBE conformance, penerapan dan pasca-cek migration, uji layar HR dan Payroll, penyelarasan salinan registry suite, kategori log, bentuk commit).

### 11.5 Perbaikan setelah kegagalan CI QBE (11 November 2026)

CI QBE strict pada PR `Yasmina` menghasilkan 87 pelanggaran dan 12 `REVIEW`. **Hanya 4 pelanggaran (`QBE-CODE-002`) berasal dari fase 2**; 83 lainnya muncul karena commit merge `6cf61197` tidak membawa 39 commit `QuilvianIntegrationBackend` (tree identik dengan induk pertama), sehingga normalisasi Rekrutmen/Lifecycle HR milik pengembang lain tampak sebagai pelanggaran baru. Ringkasan perbaikan (rinci di `PAY-BE-12` bagian 8):

| Hal | Isi |
| --- | --- |
| Merge | Dihitung ulang dengan benar (`fafd80c9` + `adfde228`), `merge.directoryRenames=false`; dua bentrok konten (snapshot dan `ApplicationDbContext.cs`), keduanya terbukti hanya rename di sisi Yasmina. Diff terhadap basis PR: 1.838 → 136 berkas |
| Keputusan desain | Berkas Payroll Modern milik integrasi **tidak dipindah dan tidak diganti nama**; hanya rujukannya ke 19 entity fase 2 yang diperbarui. Hasilnya dua keluarga namespace Payroll (`HumanResource.PayrollManagement.*` milik integrasi, `Corporate.PayrollManagement.*` milik Payroll V2); menyatukannya adalah keputusan tim |
| `QBE-CODE-002` | Dua pengecualian bertenggat (sampai 2027-03-31) untuk `WfpTransportAllowancePolicyController` dan `WfpTransportAllowanceTransactionController`; tindak lanjut `PAY-BE-13` memindahkan penomoran ke `NumberSeriesManagement` |
| Bukti | Port checker tervalidasi mereproduksi laporan CI persis (1.096 berkas, 87/12), lalu pada hasil perbaikan: 111 berkas, 0 pelanggaran memblokir. Checker resmi dan `dotnet build` tetap oleh pemilik |
| Selisih dari rencana | Rencana awal tidak memperkirakan dua hal: integrasi menaruh berkas Payroll Modern baru di folder HR yang dipindahkan fase 2 (46 saran pemindahan Git), dan pemindahan controller memicu `QBE-CODE-002` pada dua controller yang memuat pola `Generate*` |
