# Laporan Perubahan Backend — `PAY-BE-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-12` |
| Judul | Fase 2 backend: registry `Prl`, pemindahan 19 entity ke modul Payroll, penggantian nama kelas ke `Prl*` |
| Slice | Payroll mandiri — kepemilikan dan penamaan |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 7.3 (jawaban pemilik 11 November 2026); rencana `PAYROLL_V2_06`; permintaan eksplisit pemilik 11 November 2026 ("kerjakan fase 2 backend") |
| Contract version | Tidak ada perubahan kontrak API. Nama kelas, rute `payroll-management/...`, nama DTO, dan nama izin pada atribut akses tidak berubah |
| Dependency | `PAY-BE-01` sampai `PAY-BE-11`. Pasangan fisik: `PAY-MIG-02` ([laporan](PAY-MIG-02.md)) |
| Klasifikasi | `HEAVY` — 84 berkas berubah isinya, 50 dipindah, menyentuh entity inti yang dipakai HR dan Finance, tanpa kompilasi |
| Task mode | `BACKEND` (lintas repository hanya untuk dua berkas test frontend yang membaca source backend; lihat bagian 3.4) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina` (tidak membuat dan tidak berpindah branch); laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `f2dd20a6` ditambah perubahan lokal yang belum di-commit (PAY-BE-11, dokumen Payroll dan BLM). **Pengerjaan ulang setelah CI (bagian 8):** `6cf61197` (`dad`) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Source selesai dan lolos pemeriksaan statis; `dotnet build` dan eksekusi migration menunggu pemilik.** Tanpa build, kompilasi C# belum terbukti. **Pengerjaan ulang (bagian 8):** kegagalan QBE strict di CI diperbaiki pada tingkat source (merge integrasi yang benar dan dua pengecualian bertenggat); hasilnya menunggu diterapkan ke working tree dan dijalankan ulang di CI oleh pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik/prefix registry | Baris baru `Corporate / PayrollManagement / Payroll / BUSINESS DOMAIN / MODULE / Prl / ACTIVE` ditambahkan pada task ini atas persetujuan pemilik 11 November 2026. Sebelumnya modul ini hanya tercatat sebagai submodule HR yang memegang entity `Wfp*` legacy |
| Keberlakuan | `LEGACY MIGRATION` untuk 19 entity (`QBE-NAM-003`); `TOUCHED LEGACY` untuk 34 berkas yang hanya menyebut nama entity |
| Status registry | `ACTIVE` setelah task ini. `QBE-MOD-002` dan `QBE-MOD-003` atas `Prl*` tidak lagi memblokir |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-NAM-003`, `QBE-NAM-004`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-DB-002`, `QBE-SVC-001`, `QBE-PERM-001` (izin tidak berubah), `QBE-API-001` (rute tidak berubah), `QBE-DTO-001` |
| Wewenang yang dipakai | Tiga dari empat, semuanya dari instruksi pemilik: (1) mengedit registry di repository ini, (2) mengubah source, (3) menulis berkas migration, Designer, dan snapshot secara manual. **Tidak dipakai dan tidak diberikan:** `dotnet build/restore/ef`, eksekusi SQL atau database, deployment, stage/commit/push/branch, penyuntingan salinan registry pada suite skill |
| Migration / database | Berkas migration ditulis manual (`PAY-MIG-02`); **tidak dijalankan** |

## 1. Masalah yang diperbaiki

Sembilan belas entity Payroll masih tinggal di folder HRD dengan nama yang tidak mencerminkan pemiliknya: 13 bernama `Trx*` (awalan yang tidak lagi boleh dipakai untuk kode baru) dan 6 bernama `Wfp*` (awalan milik Workforce Profile). Akibatnya modul Payroll tidak punya entity sendiri, registry tidak mencatat pemiliknya, dan `QBE-NAM-001` menandai `Trx*` sebagai penamaan yang belum dinormalkan.

## 2. Proses bisnis

Tidak ada perubahan proses bisnis dan tidak ada perubahan perilaku. Pekerjaan ini hanya mengganti nama dan lokasi: kolom, tipe, kunci asing, rute, hak akses, dan isi data tidak berubah. Pemilik menguji hasilnya dengan membuka layar yang sama seperti sebelumnya (bagian 7).

**Contoh nama.** Kelas dan tabel `TrxPayrollRun` menjadi `PrlPayrollRun`; `WfpTax` menjadi `PrlTax`; `DbSet` `WfpTaxes` menjadi `PrlTaxes`; konfigurasi `WfpTaxConfiguration` menjadi `PrlTaxConfiguration`. Controller `WfpTaxController` tetap bernama `WfpTaxController` dengan rute dan izin yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` backend, kontrak QBE, `DATABASE_RULES.md`, registry kepemilikan dan prefix, skrip `tooling/qbe/Invoke-QbeConformanceCheck.ps1` (aturan pembacaan registry), `Repositories/ApplicationDbContext.cs`, snapshot migration, migration pola `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix`, seluruh 19 model, 19 konfigurasi, 6 controller dan 6 DTO `Wfp*`, serta setiap berkas yang menyebut salah satu dari 57 nama (19 entity, 19 `DbSet` jamak, 19 konfigurasi).

### 3.2 Berkas yang berubah

Pekerjaan dilakukan oleh satu skrip di scratchpad (bukan bagian repo): dicoba dulu tanpa menulis (`--dry`), seluruh 84 berkas yang akan berubah dicadangkan, lalu diterapkan (`--apply`). Pemindahan memakai `mv` biasa, bukan `git mv`.

| Kelompok | Berkas | Perubahan |
| --- | ---: | --- |
| Model dipindah | 19 | `Areas/Corporate/HumanResource/PayrollManagement/Models/` → `Areas/Corporate/PayrollManagement/Models/`; nama kelas dan berkas, `[Table(...)]`, namespace |
| Konfigurasi EF dipindah | 19 | `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/` → `.../Corporate/PayrollManagement/`; nama kelas dan berkas, `ToTable(...)`, `HasDatabaseName(...)`, namespace |
| Controller `Wfp*` dipindah | 6 | ke `Areas/Corporate/PayrollManagement/Controllers/`; hanya namespace dan nama tipe entity yang berubah |
| DTO `Wfp*` dipindah | 6 | ke `Areas/Corporate/PayrollManagement/DTOs/`; hanya namespace |
| Diubah di tempat | 34 | Rincian di bawah |
| Migration baru | 2 | `20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.cs` dan `.Designer.cs` ([laporan](PAY-MIG-02.md)) |
| Registry | 1 | Tiga sisipan pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (bagian 3.5) |

Rincian 34 berkas yang diubah di tempat:

| Lokasi | Berkas | Isi perubahan |
| --- | ---: | --- |
| `Areas/Corporate/PayrollManagement` (Services 12, Constants 3, Options 1, DTOs 1) | 17 | Nama tipe entity dan `DbSet`; 7 di antaranya juga memuat perubahan `PAY-BE-11` (bagian 3.6) |
| `Areas/Corporate/HumanResource` (MasterData 4, Overtime 4, Benefit 2, Attendance 1, Leave 1, WorkforceProfile 1) | 13 | Nama tipe entity, termasuk tipe properti navigasi pada `MstWorkforceProfile` |
| `Areas/Corporate/FinanceManagement/Receivable` | 2 | `FinanceReceivableInstallmentPlanService`, `FinanceReceivablePayrollSyncService` |
| `Repositories/ApplicationDbContext.cs` | 1 | 19 properti `DbSet` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | 1 | 100 penggantian nama (entity, tabel, indeks bernama) dan namespace |

Jumlah penggantian: **784 nama** (snapshot 100, rujukan 402, model 100, controller dan DTO 102, konfigurasi 80) dan **181 namespace**. Hanya awalan yang berganti (`Trx` atau `Wfp` menjadi `Prl`); sisa nama sama, tiga huruf diganti tiga huruf sehingga panjang nama tidak berubah.

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** tidak berubah. Rute, nama DTO, dan nama controller tetap. Nama properti JSON tidak berubah karena properti navigasi (`Payroll`, `Tax`, `Insurance`, `TransportAllowance`, `TransportAllowanceTransactions`) hanya berganti tipe, bukan nama.

**Keamanan:** nama izin pada `[AccessController]`, `[AccessPermission]`, dan `[AccessAction]` pada enam controller `Wfp*` tidak berubah (dibandingkan sebelum dan sesudah, bagian 5), sehingga hak akses yang sudah dicentang di Akses Role tidak putus.

**Database:** tidak berubah oleh pekerjaan ini. Nama tabel di source sudah `Prl*`, sehingga **aplikasi hasil build ini hanya cocok dengan database yang sudah menjalankan `PAY-MIG-02`**. Rilis satu paket, database lebih dulu (bagian 7).

### 3.4 Dampak ke frontend

Dua test unit di repository frontend membaca source backend sebagai teks dan ikut berubah. Keduanya test, bukan source produk:

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/employee-installment-entry-hrd-approval.test.mjs` baris 244 | `/TrxPayrollVariableInputs/` menjadi `/PrlPayrollVariableInputs/`. Sebelum diubah test ini **gagal** karena servis Finance kini memakai nama baru |
| `tests/unit/payroll-installment-deduction-guards.test.mjs` baris 86 | Penjaga negatif "Finance tidak membuat payroll run" memakai `PrlPayrollRun` dan seterusnya. Tanpa perubahan penjaga itu menjadi hampa (selalu lulus) karena nama lama tidak ada lagi |

`OLD_CONTROLLER` pada berkas kedua sengaja tidak diubah: ia memeriksa bahwa berkas controller lama **tidak** ada. Dua komentar di source frontend (`payroll-run-constants.js` baris 172 dan `use-payroll-run-list.js` baris 4) masih menyebut `TrxPayrollReversal` dan `TrxPayrollRun`; itu hanya komentar dan tidak diubah.

> Peringatan tata kelola: `CLAUDE.md` frontend mewajibkan perubahan source frontend lewat skill `build-module-frontend`. Dua perubahan di atas hanya pada berkas test dan merupakan akibat mekanis langsung dari pemindahan ini. Bila pemilik menghendaki perubahan itu dikerjakan lewat jalur skill frontend, kembalikan dua berkas tersebut dengan `git checkout` dan jalankan task frontend terpisah.

### 3.5 Registry

Tiga sisipan pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (148 menjadi 151 baris):

| Sisipan | Isi |
| --- | --- |
| Tabel kepemilikan | `\| Corporate \| PayrollManagement / Payroll \| BUSINESS DOMAIN / MODULE \| Prl \| ACTIVE \|`, tepat setelah baris `Wfp`; lima kolom dan urutannya dipertahankan |
| Tabel kepanjangan prefix | `\| Prl \| Payroll \|` |
| Catatan `Wfp` | Satu kalimat tambahan: `PayrollManagement` keluar dari daftar submodule legacy pemegang `Wfp*`. Daftar aslinya tidak dihapus |
| Catatan perubahan | Satu baris bertanggal 2026-11-11: pendaftaran `Prl` = *Payroll*, persetujuan pemilik (jawaban pop-up), pencabutan penghalang `QBE-MOD-002`, 13 `Trx*` dan 6 `Wfp*` dipindah dan dinamai ulang (`QBE-NAM-003`) sehingga `Wfp` tidak lagi memiliki entity Payroll, serta batas wewenang (penamaan, kepemilikan, aktivasi; eksekusi database dan deployment terpisah) |

Salinan registry pada suite skill **tidak disentuh**; penyelarasannya milik pemilik (bagian 7, butir 6).

### 3.6 Pemisahan perubahan di working tree

Working tree backend sebelum pekerjaan ini sudah memuat perubahan lain yang belum di-commit. Perubahan fase 2 **bercampur** di working tree yang sama. Tidak ada yang dikembalikan, di-stage, atau dirapikan.

| Kelompok | Jalur | Milik |
| --- | --- | --- |
| A. Hanya fase 2 | 50 berkas lama berstatus `D` (31 di `Areas/Corporate/HumanResource/PayrollManagement`, 19 di `Repositories/Configurations/Corporate/HumanResource/PayrollManagement`); 50 berkas baru di `Areas/Corporate/PayrollManagement/{Models,Controllers,DTOs}` dan `Repositories/Configurations/Corporate/PayrollManagement`; 2 berkas migration baru; 27 berkas `M` (HR 13, Finance 2, Payroll 10, `ApplicationDbContext.cs`, snapshot); registry | `PAY-BE-12` dan `PAY-MIG-02` |
| B. Tumpang tindih | 7 berkas Payroll yang memuat `PAY-BE-11` **dan** penggantian nama fase 2: `Constants/PayrollConfirmationSources.cs` (baru), `Constants/PayrollStatusCatalog.cs`, `Services/PayrollSalaryPaymentStatusResolver.cs` (baru), `Services/PayrollPaymentService.cs`, `Services/PayrollRunService.cs`, `Services/PayrollEmployeeDeductionService.cs`, `Services/PayrollReportService.cs` | Keduanya |
| C. Bukan fase 2 (tidak disentuh) | `Constants/PayrollSalaryPaymentStatuses.cs` (baru), `DTOs/PayrollPaymentDtos.cs`, `DTOs/PayrollRunDtos.cs`, `DTOs/PayrollEmployeeDeductionDtos.cs`, `Controllers/PayrollPaymentsController.cs`; 10 dokumen Finance/BLM; dokumen Payroll (`PAYROLL_V2_01` sampai `06`, laporan `PAY-BE-11`, `PAY-FE-08`, `PAY-FE-11`, `PAY-FE-12`, `PAY-FE-13`) | `PAY-BE-11`, BLM, dokumen |

**Konsekuensi praktis.** Karena 7 berkas pada kelompok B sudah memakai nama `Prl*`, `PAY-BE-11` tidak lagi dapat di-commit terpisah dan tetap dapat dikompilasi tanpa fase 2. Keduanya adalah satu paket yang dibuktikan oleh satu `dotnet build`. Pemilik menentukan bentuk commit.

## 4. Dokumentasi endpoint

Tidak menambah atau mengubah endpoint (`NOT APPLICABLE`). Rute `payroll-management/...` dan izin pada enam controller `Wfp*` tetap.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Percobaan kering skrip (`--dry`) sebelum menulis: nama `Prl*` yang sudah ada, tujuan pemindahan yang sudah terisi | 0 nama `Prl*` sudah ada; tidak ada tabrakan tujuan; 84 berkas berubah, 50 dipindah | `PASS` | Keluaran skrip |
| Cadangan seluruh berkas sebelum diubah | 84 berkas di scratchpad | `PASS` | `scratchpad/p6/backup/` |
| **Transformasi dibalik:** terapkan `Prl*` menjadi nama lama dan namespace baru menjadi lama pada setiap berkas hasil, lalu bandingkan dengan cadangan | **84 dari 84 identik.** Tidak ada perubahan lain selain penggantian nama; jumlah baris tiap berkas tetap (tanpa `using` ganda) | `PASS` | Keluaran skrip |
| Pencarian nama lama (19 entity, `DbSet` jamak, konfigurasi) di luar `Migrations/`, `docs/`, `bin`, `obj` | 0 sisa | `PASS` | `grep` |
| Pencarian namespace `HumanResource.PayrollManagement` di luar `Migrations/` | 9 sisa, seluruhnya string `LogCategory` (bukan namespace; bagian 7, butir 8) | `PASS` | `grep` |
| Snapshot: nama lama, namespace lama, BOM, jumlah baris | 0 nama lama, 0 namespace lama, BOM utuh, 145.280 baris (sama seperti sebelumnya), 100 kemunculan `Prl*` | `PASS` | `grep`, `xxd`, `wc` |
| Namespace setiap berkas di folder modul Payroll sama dengan folder | 0 pelanggaran dari 50 berkas yang dipindah dan berkas Payroll lain | `PASS` | Skrip statis |
| Tipe ganda dalam namespace yang sama pada modul Payroll dan konfigurasinya | 0 | `PASS` | Skrip statis |
| Tipe yang dideklarasikan langsung pada namespace induk `...HumanResource` yang hilang dari jangkauan nama sederhana | 0 tipe; 0 rujukan parsial pada 50 berkas yang dipindah | `PASS` | Skrip statis |
| Setiap `using QuilvianSystemBackend.*` menunjuk namespace yang dideklarasikan | Seluruh `using` ke `PayrollManagement` valid. 4 `using` lain tak terdeklarasi (`Helpers`, `OperatingRoomCommandSupport`, `Billing.Constants`, `MembershipHarness`) adalah kondisi lama dan tidak terkait | `PASS` untuk Payroll | Skrip statis |
| Berkas yang memakai `Prl*` tanpa `using` Models Payroll | 6, seluruhnya memakai nama properti `DbSet` (`dbContext.PrlX`) atau teks deskripsi, bukan tipe | `PASS` (telaah) | `grep` |
| Atribut akses dan rute enam controller `Wfp*` sebelum dan sesudah (`[AccessController]`, `[AccessPermission]`, `[AccessAction]`, `Route`, `Http*`), nama entity dinormalkan | 6 dari 6 identik | `PASS` | Skrip perbandingan |
| String literal berisi nama entity di luar konfigurasi dan snapshot | 28: 19 `[Table(...)]` pada model, 4 pesan atau deskripsi pada modul Attendance dan Overtime, 5 label sumber pada `PayrollCalculationService`; tidak ada `nameof`; tidak ada pada atribut akses | `PASS` (telaah) | Skrip inspeksi |
| Pencarian FE untuk label `WfpPayroll.*` yang dipakai sebagai isi JSON rincian hitung | Tidak ada pemakai di `src` dan `tests` | `PASS` | `grep` |
| Replikasi pemeriksaan kepemilikan checker QBE (`Resolve-RegistryOwnership`, `DbSet`, `IEntityTypeConfiguration`) untuk 19 entity | 19 dari 19 lolos; kontrol negatif `TrxPayrollRun` gagal pada prefix seperti seharusnya; baris registry terparse (7 sel), baris catatan perubahan tidak terbaca sebagai baris kepemilikan (6 sel) | `PASS` (replikasi, **bukan** checker resmi) | Skrip statis |
| Checker QBE resmi `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan: `pwsh` tidak tersedia di mesin ini | `NOT RUN` | — |
| Test FE yang membaca source backend | Sebelum diubah: 29 test, 28 lulus, 1 gagal. Sesudah: 29 lulus | `PASS` | `node --test` |
| Suite unit FE penuh | 4.772 test, 4.725 lulus, 47 gagal; **nama 47 yang gagal sama persis dengan baseline** (tanpa kegagalan baru) | `PASS` (setara baseline) | `node --test "tests/unit/*.test.mjs"` |
| `dotnet restore` / `dotnet build` pada project aplikasi | **Tidak dijalankan oleh agen** (aturan pemilik). **Menunggu pemilik** | `NOT RUN` | — |
| `dotnet ef migrations has-pending-model-changes` | Tidak dijalankan | `NOT RUN` | — |
| Perilaku runtime (kueri, layar HR, run, slip, pembayaran) | Belum dijalankan | `NOT RUN` | — |
| Tes otomatis backend | Tidak ada project tes otomatis di backend (kebijakan repository); tidak dibuat | `NOT APPLICABLE` | `rules/backend/TEST_POLICY.md` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen:** semua perintah `dotnet`, eksekusi database, server dan API, serta checker PowerShell.

**Batas bukti.** Pemeriksa statis dan penelusuran di atas tidak menggantikan kompilasi. Pada fase 1 pemeriksa statis pernah meloloskan kesalahan (`CS0509`) yang baru ketahuan saat build. Yang paling mungkin bermasalah pada pekerjaan ini: nama sederhana yang dulu terselesaikan lewat namespace induk (sudah diperiksa, 0 temuan), tabrakan nama antara tipe `Prl*` dan tipe lain pada berkas yang mengimpor banyak namespace (sudah diperiksa, 0 nama `Prl*` sebelumnya), dan atribut `[Table]` yang tidak sama dengan `ToTable` (keduanya diganti oleh pola yang sama).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Registry `Prl` ACTIVE dengan lima kolom persis dan satu catatan perubahan | Terpenuhi | Bagian 3.5; replikasi parser |
| 19 model, 19 konfigurasi, 6 controller, 6 DTO dipindah dengan `mv` ke modul Payroll; folder HR kosong dihapus | Terpenuhi | Bagian 3.2; folder lama tidak ada |
| Kelas, berkas, konfigurasi, `DbSet`, `[Table]`, `ToTable`, indeks bernama, dan seluruh rujukan memakai `Prl*` | Terpenuhi (source) | Pencarian 0 sisa; transformasi dibalik 84 dari 84 |
| Nama kolom, tipe, kunci asing, properti navigasi, DTO, controller, rute tidak berubah | Terpenuhi | Perbandingan atribut dan rute; transformasi dibalik |
| Hak akses di Akses Role tidak putus | Terpenuhi (source) | 6 dari 6 controller identik |
| Migration lama tidak ditulis ulang; hanya snapshot dan migration baru | Terpenuhi | Skrip tidak menyentuh `Migrations/` selain snapshot |
| Snapshot konsisten dengan model | Terpenuhi (statis) | Bagian 5; `has-pending-model-changes` belum dijalankan |
| Tidak ada stage, commit, push, atau perubahan branch | Terpenuhi | Branch tetap `Yasmina` |
| DoD: laporan tracked dan register | Terpenuhi | Berkas ini dan `PAYROLL_V2_04` |
| DoD: `dotnet build` | **Belum** | Menunggu pemilik |
| DoD: migration diterapkan dan diverifikasi pada database uji | **Belum** | Menunggu pemilik (`PAY-MIG-02`) |
| DoD: perilaku runtime | **Belum** | Bagian 7 |

Status akhir **🟡**: seluruh kriteria terpenuhi pada tingkat source dan pemeriksaan statis, tetapi kompilasi, migration, dan runtime belum terbukti.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Aplikasi hasil build ini tidak boleh dijalankan terhadap database yang belum menjalankan `PAY-MIG-02`**, karena seluruh kueri memakai nama tabel baru. Rilis satu paket: database lebih dulu, baru aplikasi. Jangan menggabungkan ke branch rilis sebelum migration diterapkan |
| Selisih dari rencana (`PAYROLL_V2_06`) | (1) **59 berkas terdampak di source, bukan 54** (rencana): +4 berkas Payroll (termasuk dua berkas baru `PAY-BE-11`) dan +1 berkas Overtime (`OvertimeFinalReconciliationService`). Bukan perubahan lingkup. (2) **Enam `Wfp*` dikerjakan dalam paket yang sama dengan 13 `Trx*`**, mengikuti instruksi pemilik, bukan usul agen untuk memisahnya. (3) **Tidak ada branch khusus**; pemilik memerintahkan kerja di `Yasmina`. (4) **Pra-cek `LIKE` pada rancangan migration diganti pencocokan token** dan pemeriksaan tabrakan (lihat laporan `PAY-MIG-02`) |
| Risiko tersisa | (1) Kompilasi belum terbukti. (2) Layar pegawai HR (profil pajak, iuran, tunjangan transport) memakai properti navigasi yang berganti tipe; uji manual disarankan (butir 5 di bawah). (3) Aplikasi dan database harus dirilis berpasangan |
| Temuan di luar lingkup | (1) **Sembilan string `LogCategory` masih `Corporate.HumanResource.PayrollManagement`** (enam controller `Wfp*`, tiga service Payroll). Mengubahnya mengubah kategori pada log sehingga pencarian log lama terputus; tidak diubah karena bukan bagian dari rename. (2) **Ketidakselarasan Designer yang sudah ada sebelumnya:** entity `Rhb*` (108 kemunculan) ada di snapshot dan Designer `20261105045613_AddRehabilitationManagement`, tetapi **tidak ada** di Designer `20261106025752_AddOpticalCreateIdempotencyKey` dan `20261110090000_AddFinPurchasingBlmStaging`. Designer baru dibangun dari snapshot sehingga memuat `Rhb*`. Pemilik sebaiknya memastikan lewat `has-pending-model-changes`. (3) `docs/system-registry/02-entity-terdaftar.md` adalah dokumen audit berstempel SHA `f2c5090` yang masih memuat 19 nama lama; tidak diubah karena berupa riwayat dan di luar daftar berkas yang diizinkan |
| Dokumen yang tidak diubah | Laporan task lama (`PAY-BE-01` sampai `PAY-BE-11`, `BE-FIN-*`, dan lainnya) dan dokumen HR yang menyebut nama atau jalur lama. Itu catatan keadaan pada saatnya; nama sebelum 11 November 2026 sah pada konteksnya |
| Perubahan sampingan | Dua berkas test frontend (bagian 3.4). Selain itu `NONE` |
| Interupsi | Percakapan sempat diringkas di tengah pekerjaan (analisis sudah selesai, belum ada berkas yang diubah). Pekerjaan dilanjutkan dari kondisi terverifikasi tanpa penyuntingan ganda |
| Status Git | Branch `Yasmina`, SHA dasar `f2dd20a6`. Working tree: 52 `M`, 50 `D`, 60 `??` pada `git status -uall`. Tidak ada stage atau commit. Pemisahan per jalur ada di bagian 3.6 |
| Langkah berikutnya | Pemilik: lihat daftar di bawah |

**Permintaan kepada pemilik, berurutan:**

1. Jalankan `dotnet build` pada project aplikasi dan tempel keluarannya. Ini satu gerbang untuk `PAY-BE-11` dan `PAY-BE-12`.
2. Jalankan `dotnet ef migrations has-pending-model-changes` (atau padanan yang dipakai tim). Hasil yang diharapkan: tidak ada perubahan tertunda. Bila ada, catat apakah terkait `Rhb*` (temuan di atas).
3. Jalankan pemeriksaan QBE conformance lokal (`Invoke-QbeConformanceCheck.ps1`, mode `WorkingTree` atau `GitRange`). Replikasi agen hanya setara untuk aturan kepemilikan, bukan pengganti.
4. Tinjau dan jalankan `PAY-MIG-02` pada database uji mengikuti skrip pada [laporan `PAY-MIG-02`](PAY-MIG-02.md): cadangan, pra-cek, terapkan, pasca-cek, uji `Down` lalu `Up` ulang.
5. Uji manual setelah migration diterapkan: (a) layar pegawai HR, tab profil pajak, iuran/asuransi, dan tunjangan transport masih memuat data lama; (b) daftar dan detail Payroll Run, slip gaji, riwayat pembayaran masih memuat data lama; (c) Akses Role untuk `WorkforcePayrollProfile`, `WorkforceTaxProfile`, `WorkforceInsuranceProfile`, dan `WorkforceTransportAllowance` masih tercentang dan layarnya tidak 403.
6. Samakan salinan registry pada suite skill dengan baris `Prl` dan catatan perubahan 2026-11-11 (satu baris tabel, satu baris kepanjangan prefix, satu baris catatan; teks sama dengan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`).
7. Bila proses tim memindahkan Designer lama ke `Migrations/History/`, jalankan skrip pemeliharaannya. `MigrationMetadata.g.cs` dibangkitkan tool dan tidak disentuh.
8. Putuskan apakah kategori log `Corporate.HumanResource.PayrollManagement` (sembilan tempat) disamakan dengan modul baru. Perubahan ini memutus pencarian log lama, jadi sebaiknya dijadwalkan terpisah.
9. Tentukan bentuk commit mengingat tumpang tindih dengan `PAY-BE-11` (bagian 3.6), dan apakah dua berkas test frontend dikerjakan lewat jalur skill frontend (bagian 3.4).

> Daftar di atas ditulis sebelum CI dijalankan. Setelah kegagalan CI, bagian 8 menggantikan butir 3 dan menambah langkah menerapkan hasil perbaikan.

## 8. Perbaikan setelah kegagalan CI QBE strict (11 November 2026)

Pengerjaan ulang task yang sama atas laporan CI pemilik: `QBE Conformance`, mode `Strict`, `GitRange`, 1.096 berkas dievaluasi, **87 `VIOLATION` dan 12 `REVIEW`**. Basis PR adalah `origin/QuilvianIntegrationBackend` (`adfde228`).

### 8.1 Asal temuan

Dari 87 pelanggaran, **hanya 4 berasal dari fase 2**. Delapan puluh tiga lainnya (`QBE-NAM-001` dan `QBE-MOD-002` pada Rekrutmen dan Lifecycle HR) berasal dari keadaan Git, bukan dari kode Payroll.

| Temuan | Jumlah | Asal | Bukti |
| --- | ---: | --- | --- |
| `QBE-NAM-001` pada model, `DbSet`, dan konfigurasi `Trx*` Rekrutmen dan Lifecycle | 57 | Bukan fase 2. Di integrasi folder Rekrutmen berisi 68 berkas hasil normalisasi (`Hrd*`); di `HEAD` folder yang sama masih berisi 20 berkas versi lama (`Trx*`, `Mst*`) | `git ls-tree` |
| `QBE-MOD-002` pada model yang sama | 26 | Sama | Port checker |
| `QBE-CODE-002` pada `WfpTransportAllowancePolicyController` (`GenerateCodeAsync`, baris 278 dan 687) dan `WfpTransportAllowanceTransactionController` (`GenerateTransactionNumberAsync`, baris 251 dan 671) | 4 | **Fase 2.** Pemindahan controller membuat seluruh barisnya dianggap kode baru | Port checker |
| `QBE-SVC-001` (`REVIEW`, tidak memblokir) pada enam controller `Wfp*` | 12 | **Fase 2**, sebab sama | Port checker |

**Penyebab 83 pelanggaran.** Commit `6cf61197` (`dad`) adalah commit merge dengan dua induk, `fafd80c9` dan `adfde228`, tetapi **pohon berkasnya identik dengan `fafd80c9`**. Akibatnya 39 commit dari `QuilvianIntegrationBackend` tercatat sebagai terintegrasi padahal isinya tidak ikut: dibanding basis PR, `HEAD` berbeda 1.838 berkas (676.737 baris terhapus), termasuk seluruh normalisasi Rekrutmen/Lifecycle dari pengembang lain. `dad` sudah ada di `origin/Yasmina`.

**Kekeliruan pada pekerjaan fase 2 sebelumnya.** Verifikasi saya waktu itu mereplikasi aturan kepemilikan checker saja dan menandai checker resmi `NOT RUN`. Empat pelanggaran `QBE-CODE-002` dan dua belas `REVIEW` pada controller yang dipindah seharusnya sudah disebut sebagai risiko (pola `Generate*Number` di controller yang dipindah terlihat dari bacaan). Pada pengerjaan ulang ini port checker diperluas ke seluruh aturan yang diimplementasikan dan divalidasi lebih dulu (bagian 8.3).

### 8.2 Yang dikerjakan

| Langkah | Isi |
| --- | --- |
| Merge yang benar | Hasil merge tiga arah `fafd80c9` + `adfde228` (titik cabang `f2dd20a6`) dihitung dengan `git merge-tree`, dengan `merge.directoryRenames=false`. Bentrok yang tersisa **dua**, keduanya konten: `ApplicationDbContext.cs` dan `ApplicationDbContextModelSnapshot.cs`. Dengan pengaturan bawaan Git ada 49 bentrok, 46 di antaranya saran memindahkan berkas Payroll Modern milik integrasi ke folder baru (lihat keputusan desain di bawah) |
| Dua bentrok konten | Perubahan Yasmina pada keduanya **terbukti murni penggantian nama** (dibalik, identik dengan `f2dd20a6`). Maka dipakai versi integrasi, lalu penggantian nama diterapkan ulang |
| Rename pada kode integrasi | 22 berkas hasil merge masih memuat nama lama atau membutuhkan `using` baru: 4 entity Payroll Modern di folder HR (`TrxPayrollDeductionItem`, `TrxPayrollHandoffBatch`, `TrxPayrollRestitution`, `TrxPayrollVarianceAlert`, properti navigasinya bertipe entity yang kini `Prl*`), 15 service, 1 service HR (`EmployeeDocumentIssuanceService`), `ApplicationDbContext.cs`, dan snapshot. Di snapshot nama lengkap entity diganti **per 19 nama**, bukan per namespace, karena entity milik integrasi tetap bernama lengkap di namespace HR |
| Designer | Dibangun ulang dari snapshot hasil (header saja yang berbeda dari snapshot: 7 baris) |
| Pengecualian | Dua entri bertenggat di `docs/engineering/QBE_EXCEPTIONS.json`: `EXP-QBE-CODE-002-PRL-001` dan `EXP-QBE-CODE-002-PRL-002`, `Scope` per berkas, `ExpiresAt` 2027-03-31, `ApprovalReference` keputusan pemilik 2026-11-11. Penomoran dipindahkan ke `NumberSeriesManagement` oleh task lanjutan `PAY-BE-13` (belum dikerjakan) |

**Keputusan desain (konservatif).** Berkas Payroll Modern milik integrasi (7 controller, 18 DTO, 11 model di folder HR, dan 41 service yang berada di folder Payroll tetapi mendeklarasikan namespace `...HumanResource.PayrollManagement.Services`) **tidak dipindahkan dan tidak diganti namanya**. Memindahkannya ke `Areas/Corporate/PayrollManagement` akan menundukkannya pada baris registry `Prl` dan memaksa rename entity dan tabel milik tim lain tanpa persetujuan mereka. Akibatnya setelah merge terdapat **dua keluarga namespace Payroll** (`...HumanResource.PayrollManagement.*` milik integrasi dan `...Corporate.PayrollManagement.*` milik Payroll V2). Menyatukannya adalah keputusan tim, bukan bagian task ini.

### 8.3 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| **Port checker QBE divalidasi** pada keadaan `dad` terhadap `adfde228` (aturan `NAM-001`, `CODE-002/003`, `SVC-001`, `ENT-001`, `CFG-001`, `MOD-002`; pembersih komentar/string, `-match` tidak peka huruf, pengecualian per berkas) | **Mereproduksi laporan CI persis**: 1.096 berkas, 47 pemindahan migration, 87 `VIOLATION` (`NAM-001` 57, `MOD-002` 26, `CODE-002` 4), 12 `REVIEW`, termasuk nomor baris 278/687/251/671 dan 280 sampai 294, 306 sampai 308, 312 pada `ApplicationDbContext.cs` | `PASS` (validasi port) |
| Port yang sama pada hasil perbaikan | **111 berkas dievaluasi; 4 `VIOLATION` (semua `CODE-002`) tertutup dua pengecualian; 0 pelanggaran memblokir; 12 `REVIEW`; hasil `PASS`** | `PASS` (port; **bukan checker resmi**) |
| Checker resmi `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan (`pwsh` tidak tersedia untuk agen). Pemilik menjalankannya ulang | `NOT RUN` |
| Diff terhadap basis PR | **136 berkas** (sebelumnya 1.838), seluruhnya pekerjaan Yasmina: 56 diubah, 50 dipindah, 5 baru, dokumen. Tidak ada penghapusan berkas integrasi | `PASS` |
| Setiap berkas `.cs` yang berbeda dari basis, dibalik penggantian namanya, dibandingkan dengan basis | 97 identik; 9 tidak identik, **semuanya berkas `PAY-BE-11`** (fitur yang memang berbeda dari basis) | `PASS` |
| Nama lama 19 entity di luar `Migrations/` | 0 | `PASS` |
| Snapshot hasil: nama lama, nama `Prl*`, entity integrasi | 0 nama lama; 106 kemunculan `Prl*` (100 dari fase 2 ditambah 6 FK entity integrasi ke tabel Payroll); entity integrasi tetap bernama lengkap di namespace HR; 152.815 baris, BOM utuh | `PASS` |
| `using` pada seluruh source | Semua `using` ke namespace `PayrollManagement` merujuk namespace yang dideklarasikan. Empat `using` lain tak terdeklarasi adalah kondisi lama di luar Payroll (`Helpers`, `OperatingRoomCommandSupport`, dua di `Tests/`) | `PASS` |
| Sepuluh berkas yang memakai nama `Prl*` tanpa `using` Models | Seluruhnya hanya memakai properti `DbSet` atau teks, tidak ada tipe | `PASS` |
| Tipe bernama sama pada dua keluarga namespace Payroll | 0 | `PASS` |
| Nama izin `[AccessController]` pada 22 controller di kedua folder | Semua unik; atribut akses enam controller `Wfp*` tetap identik | `PASS` |
| `Program.cs`: registrasi layanan Payroll ganda | 0 | `PASS` |
| Simulasi nama migration pada snapshot hasil | 10.786 dari 10.786 nama cocok setelah `Up`; `Up` lalu `Down` kembali ke semula; 289 nama terdampak (283 + 6 FK entity integrasi); lihat laporan `PAY-MIG-02` bagian 9 | `PASS` (simulasi) |
| `dotnet build`, checker resmi, `has-pending-model-changes`, migration pada database | Tidak dijalankan | `NOT RUN` |

### 8.4 Temuan yang tidak diubah

1. **Dua `[Route]` kelas identik pada controller milik integrasi**: `api/hr-deductions` pada `PayrollDeductionScheduleController` dan `RestitutionClaimController`. Sudah ada di basis; bukan bagian pekerjaan ini. Bila verb dan sub-rute bertabrakan, ASP.NET Core akan menolak saat runtime. Mohon pemilik controller itu memeriksa.
2. **Lima model Payroll Modern milik integrasi masih bernama `Trx*`** (`TrxPayrollDeductionItem`, `TrxPayrollDeductionSchedule`, `TrxPayrollHandoffBatch`, `TrxPayrollRestitution`, `TrxPayrollVarianceAlert`) dan berada di folder HR. Tidak dievaluasi checker karena tidak berubah terhadap basis, tetapi menjadi utang normalisasi `QBE-NAM-003` milik tim integrasi.
3. Dua belas `REVIEW` `QBE-SVC-001` pada enam controller `Wfp*` (`ApplicationDbContext` di controller) tidak memblokir; menyelesaikannya berarti memindahkan akses data ke service Payroll, di luar lingkup.
4. Kategori log, `Rhb*` pada Designer lama, dan dokumen audit lama: tetap seperti bagian 7.

### 8.5 Cara menerapkan hasil

Working tree dan index Anda **tidak diubah** pekerjaan ini. Hasil disimpan sebagai satu objek tree Git (dibangun lewat indeks sementara) yang menggabungkan: `HEAD` + 39 commit integrasi + perbaikan nama + pengecualian + dokumen. `git read-tree -u --reset` menimpa working tree, dan pengaman agen menolaknya, sehingga langkah ini dijalankan pemilik:

```bash
cd NewQuilvianSystemBackend
git status                      # harus bersih (HEAD 6cf61197)
git read-tree -u --reset <TREE> # TREE tertera pada pesan penyerahan; menimpa working tree
git reset -q                    # index kembali ke HEAD: hasil tampil sebagai perubahan biasa
git status --short | head       # tinjau
git add -A && git commit        # hanya bila sudah yakin; satu commit koreksi di atas dad
git push                        # tanpa --force
```

Untuk membatalkan sebelum commit: `git read-tree -u --reset HEAD`. Objek tree tidak dirujuk branch mana pun sehingga dapat dihapus `git gc` setelah masa tenggang; terapkan dalam beberapa hari.
