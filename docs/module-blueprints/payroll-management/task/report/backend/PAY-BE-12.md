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
| Commit backend saat dikerjakan | `f2dd20a6` ditambah perubahan lokal yang belum di-commit (PAY-BE-11, dokumen Payroll dan BLM) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Source selesai dan lolos pemeriksaan statis; `dotnet build` dan eksekusi migration menunggu pemilik.** Tanpa build, kompilasi C# belum terbukti |

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
