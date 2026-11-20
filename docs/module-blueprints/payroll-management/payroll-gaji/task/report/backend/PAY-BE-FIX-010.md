# Laporan Perbaikan Backend — `PAY-BE-FIX-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-010` (ad-hoc lanjutan seri `PAY-BE-FIX-001..009`) |
| Judul | Status persetujuan penetapan gaji: pisahkan persetujuan aktual dari kelayakan Payroll, sehingga penetapan yang sah otomatis tidak lagi terbaca "Belum Disetujui" |
| Slice | Corporate / Payroll Management (`Prl`) dan Human Resource / Workforce Core (`Wfp`), bagian Penetapan Gaji |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc `PAY-BE-FIX`; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Trace | Keputusan "peniadaan approval" 19 November 2026 (komentar `PayrollPolicyOptions.cs`); `PAY-BE-FIX-008` (aturan tunggal "disetujui" dan sakelar kebijakan) |
| Contract version | Tidak ada kontrak bertversi; semua perubahan **additive** pada respons yang sudah ada |
| Dependency | `PAY-FE-FIX-010` (frontend) membaca kontrak ini; source-nya sudah dikerjakan (laporan: `../frontend/PAY-FE-FIX-010.md`, status 🟡) |
| Klasifikasi | `MEDIUM` — skor 6 (berkas diperiksa 9–20: 1; berkas diubah 4–8: 1; logika sedang: 1; kontrak API additive: 1; database: 0; keamanan/auth berkaitan: 1; UI/workflow backend: 1). Satu repository untuk task ini |
| Task mode | `CROSS-REPO` dengan urutan backend lebih dulu; task ini hanya menulis di backend |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `d642a9df9de35801ed8332adc0cdcd6127f96353` (branch `Yasmina`, kerja bersih pada awal task) |
| Tanggal | 20 November 2026 |
| Status | 🟡 **Source selesai; belum divalidasi lewat build.** `dotnet build` `NOT RUN` karena dilarang pada task ini. Tidak ada klaim runtime |

**Backend Governance Preflight.** Area `Corporate`, Module `HumanResource/WorkforceCore` (prefix `Wfp`) dan `PayrollManagement` (prefix `Prl`, legacy). Keberlakuan `TOUCHED LEGACY`. Tidak ada entity, tabel, configuration, atau prefix baru, sehingga QBE-ENT, QBE-CFG, QBE-NAM, QBE-MOD-002/003 tidak berlaku. QBE ID yang berlaku: QBE-API-001 (respons tetap dibungkus `ApiResponse<T>`/`PagedResult<T>`), QBE-PERM-001 (atribut Access tidak diubah), QBE-DTO-001 (hanya DTO), QBE-VAL-001 (nilai filter baru divalidasi, 400 bila tidak dikenal).

---

## 1. Masalah yang diperbaiki

Halaman `/payroll/workforce-compensations/<WorkforceProfileId>`, bagian **Riwayat Penetapan Gaji**, menampilkan penetapan yang aktif, utama, dan sedang berlaku dengan badge **"Belum Disetujui"** sekaligus keterangan **"Belum disyaratkan Payroll"**. Dua keterangan itu saling bertentangan, dan petugas tidak tahu apakah gajinya akan dibayar atau tertahan.

Penyebab di backend: respons riwayat hanya membawa **fakta persetujuan manual** (`IsApproved`, `ApprovalStatus`, `ApprovedAt`) dan satu penanda kebijakan (`ApprovalRequired`). Tidak ada field yang menyatakan "penetapan ini sah dipakai Payroll menurut kebijakan". Akibatnya `ApprovalStatus` bernilai `PendingApproval` untuk setiap penetapan tanpa `ApprovedAt`, walau kebijakan memang tidak mensyaratkannya, dan frontend terpaksa menebak.

Contoh (data pada screenshot, kebijakan bawaan `false`): Gaji Pokok Dokter Tetap, 30 Juni 2026 sampai sekarang, Utama, Aktif, `ApprovedAt` kosong.

---

## 2. Proses bisnis

**Tujuan.** Petugas melihat dengan benar, per penetapan: apakah sah dipakai Payroll, dan karena apa (persetujuan manual atau kebijakan).

**Pelaku.** Petugas HR membuat/mengubah penetapan; pengesah menyetujui bila kebijakan mensyaratkan; petugas Payroll membaca. Backend tidak mengubah siapa yang berwenang.

**Dua hal yang dinilai terpisah.**

| Dimensi | Pertanyaan | Field | Nilai |
| --- | --- | --- | --- |
| Persetujuan aktual (tidak berubah) | Adakah persetujuan manual yang tercatat? | `IsApproved`, `ApprovalStatus`, `ApprovedAt`, `ApprovedByUserName` | `Approved` / `PendingApproval` |
| Asal wewenang (baru) | Dari mana penetapan ini boleh dipakai Payroll? | `AuthorizationStatus` | `ManualApproved` / `AutoByPolicy` / `PendingApproval` |
| Keadaan operasional (baru) | Apakah sedang berlaku? | `OperationalStatusCode` | `Current` / `Scheduled` / `Expired` / `Inactive` / `NotPrimary` |
| Putusan (baru) | Boleh dipakai Payroll hari ini? | `IsPayrollUsable` | `true` bila `Current` dan bukan `PendingApproval` |

**Aturan penentuan** (satu tempat: `WfpSalaryAssignmentRules`):

1. `AuthorizationStatus`: `PendingApproval` bila kebijakan mensyaratkan persetujuan dan belum tercatat; `ManualApproved` bila persetujuan manual tercatat (kebijakan apa pun); selain itu `AutoByPolicy`.
2. `OperationalStatusCode`, berurutan: tidak aktif → `Inactive`; tanggal akhir lewat → `Expired`; tanggal mulai belum tiba → `Scheduled`; bukan penetapan utama → `NotPrimary`; selain itu `Current`.
3. `AutoByPolicy` **bukan persetujuan oleh seseorang**. Ia tidak mengisi `ApprovedAt`/`ApprovedByUserId`, `IsApproved` tetap `false`, dan tidak ada backfill.

**Contoh berangka (kebijakan `RequireApprovedSalaryAssignment` = `false`, tanggal acuan 20 Nov 2026):**

| Penetapan | Aktif | Utama | Masa berlaku | `ApprovedAt` | `AuthorizationStatus` | `OperationalStatusCode` | `IsPayrollUsable` |
| --- | :---: | :---: | --- | --- | --- | --- | :---: |
| Pada screenshot | Ya | Ya | 30 Jun 2026 – sekarang | kosong | `AutoByPolicy` | `Current` | `true` |
| Dijadwalkan | Ya | Ya | 1 Jan 2027 – sekarang | kosong | `AutoByPolicy` | `Scheduled` | `false` |
| Dinonaktifkan | Tidak | Ya | 1 Jan 2026 – sekarang | kosong | `AutoByPolicy` | `Inactive` | `false` |
| Disetujui manual lalu berakhir | Ya | Ya | 1 Jan 2025 – 31 Des 2025 | terisi | `ManualApproved` | `Expired` | `false` |
| Aktif tetapi bukan utama | Ya | Tidak | 1 Jan 2026 – sekarang | kosong | `AutoByPolicy` | `NotPrimary` | `false` |

Dengan kebijakan `true`, baris pertama menjadi `PendingApproval` / `Current` / `IsPayrollUsable = false` (tertahan), sedangkan baris yang `ApprovedAt`-nya terisi menjadi `ManualApproved` / `Current` / `true`.

**Jalur tidak normal.** Nilai filter status yang tidak dikenal ditolak dengan kode 400 beserta daftar nilai yang sah. Tanpa izin `ViewAmount`, nominal tetap `null` (tidak berubah).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpSalaryAssignmentRules.cs`, `WfpSalaryAssignmentController.cs`, `WorkforceCoreCrossEmployeeController.cs`, `WorkforceCoreCrossEmployeeQueryService.cs`, `PayrollCompensationService.cs`, `PayrollHrdSourceReader.cs`, `PayrollCalculationService.cs` (bagian penetapan gaji), `PayrollReadinessEvaluator.cs`, `PayrollHrdImportService.cs`, `PayrollPolicyOptions.cs`, `WfpSalaryAssignmentConfiguration.cs`, migration `20260726161839_initializeBigModulHRD2.cs` (indeks), `appsettings.json`/`appsettings.Development.json` (hanya blok `Payroll:Policy`), DTO `WfpSalaryAssignmentDtos`, `WorkforceCoreCrossEmployeeDtos`, `PayrollCompensationDtos`, serta laporan `PAY-BE-FIX-008` dan `PAY-BE-FIX-009`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/WfpSalaryAssignmentRules.cs` | Menambah `AuthorizationStatusOf`, `OperationalStatusOf`, `StandingOf`, normalisasi nilai filter, tiga ekspresi query EF (`AuthorizationStatusExpression`, `OperationalStatusExpression`, `PayrollUsableExpression`), konstanta `WfpSalaryAuthorizationStatuses` dan `WfpSalaryOperationalStatuses`, serta `SalaryStanding`. Aturan lama (`IsApproved`, `StatusOf`, ekspresi persetujuan) tidak diubah |
| `Areas/Corporate/PayrollManagement/Services/PayrollCompensationService.cs` | `SalaryHistory` kini membawa `AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable`, dan `ApprovedByUserName` (hanya untuk persetujuan yang benar-benar tercatat). Ini titik yang menghasilkan baris pada screenshot |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollCompensationDtos.cs` | Field baru pada `PayrollCompensationSalaryHistoryResponse` |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpSalaryAssignmentController.cs` | `MapResponse` memakai satu aturan untuk teks `OperationalStatus` lama (hasil identik) dan kode baru; ringkasan menambah hitungan sesuai kebijakan; komentar Create yang menyatakan "selalu menunggu persetujuan" dikoreksi |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/WfpSalaryAssignmentDtos.cs` | Field baru pada `WfpSalaryAssignmentResponse` dan `WfpSalaryAssignmentSummaryResponse` |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/WorkforceCoreCrossEmployeeQueryService.cs` | Rekap lintas pegawai: filter `authorizationStatus` dan `operationalStatus`, serta field status pada setiap baris |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforceCoreCrossEmployeeController.cs` | Dua parameter query baru, validasi nilai (400), diteruskan ke service |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/WorkforceCoreCrossEmployeeDtos.cs` | Field baru, termasuk `ApprovalStatus` yang sebelumnya tidak ada pada rekap |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Semua **additive**. Field lama (`IsApproved`, `ApprovalStatus`, `ApprovalRequired`, `ApprovedAt`, `IsActive`, `IsPrimary`, `OperationalStatus`) dipertahankan dengan makna yang sama. Filter `isApproved` tetap berarti persetujuan manual tercatat. Parameter query baru bersifat opsional |
| Database | **Tidak ada** perubahan skema, migration, atau data. Tidak ada backfill `ApprovedAt`/`ApprovedByUserId`. Status diturunkan saat dibaca |
| Keamanan/Auth | Atribut `[AccessAction]`/`[AccessPermission]` tidak diubah. `BaseSalary` tetap `null` tanpa `ViewAmount`. Nama pengesah dimuat hanya untuk persetujuan yang tercatat dan hanya pada endpoint detail yang sudah berizin `PayrollCompensation : ReadDetail` |
| Kebijakan Payroll | Tidak diubah. Pembaca Payroll (`PayrollHrdSourceReader`, `PayrollCalculationService`, `PayrollHrdImportService`) tidak disentuh; kodenya sudah memakai `RequireApprovedSalaryAssignment`. `IsPayrollUsable` dirancang sama dengan syarat pembaca itu (aktif, utama, berlaku, dan bila disyaratkan, disetujui) |

### 3.4 Keputusan rancangan yang menyimpang dari usulan prompt

| Usulan prompt | Yang dikerjakan | Alasan berbukti |
| --- | --- | --- |
| Status `Conflict` | **Tidak ditambah** | Indeks unik `IX_WfpSalaryAssignment_WorkforceProfileId` (satu penetapan utama aktif per pegawai) dibuat oleh migration awal tabel (`Migrations/20260726161839_initializeBigModulHRD2.cs:46090-46096`), dan penulisan yang beririsan ditolak `FindOverlappingPrimaryAsync`. Keadaan itu tidak dapat muncul dari data, sehingga status itu akan menjadi state mati. Bila indeks kelak dilonggarkan (sudah dicatat sebagai dependency pada komentar `ClearPrimaryAsync`), `Conflict` perlu ditambahkan saat itu |
| `NotEffective` (satu nilai) | `Scheduled` dan `Expired` terpisah | Frontend harus menampilkan "Terjadwal" dan "Berakhir" sebagai dua label berbeda; satu nilai gabungan memaksa frontend menebak dari tanggal |
| `IsPrimary` sebagai syarat "Disetujui Otomatis" | Dipertahankan, lewat `NotPrimary` | `PayrollCalculationService.cs:546` hanya membayar penetapan utama (baris non-utama hanya menjadi peringatan), sama dengan `PayrollHrdSourceReader.cs:144`. Menampilkan baris non-utama sebagai "otomatis sah" akan menyesatkan |

---

## 4. Dokumentasi endpoint

#### Corporate / Payroll Management / Workforce Compensation

Base URL: `api/v1/corporate/payroll-management/workforce-compensations`

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/{workforceProfileId}` | Detail profil penggajian beserta `SalaryHistory` | `PayrollCompensation : ReadDetail` | `SalaryHistory[]` + `AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable`, `ApprovedByUserName` |

#### Corporate / Payroll Management / Salary Assignment

Base URL: `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/salary-assignments` (juga tersedia di `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/salary-assignments`)

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar penetapan satu pegawai | `WfpSalaryAssignment : Read` | Item + `AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable` |
| `GET` | `/{id}` | Detail satu penetapan | `WfpSalaryAssignment : Read` | Field yang sama |
| `GET` | `/summary` | Ringkasan jumlah | `WfpSalaryAssignment : Read` | + `ApprovalRequired`, `AutoByPolicyData`, `AwaitingApprovalData`, `PayrollUsableData`; `ApprovedData` dan `PendingApprovalData` tidak berubah makna |

#### Corporate / Human Resource / Workforce Core / Cross-Employee Administration

Base URL: `api/v1/corporate/human-resource/workforce-core`

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/salary-assignments` | Rekap penetapan gaji seluruh pegawai (tanpa nominal) | `WfpSalaryAssignment : ReadAll` | Query baru `authorizationStatus`, `operationalStatus`; item + `ApprovalStatus`, `AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable` |

Kode status: `200` berhasil; `400` nilai `authorizationStatus`/`operationalStatus` tidak dikenal (pesan memuat nilai yang sah); `401` belum masuk; `403` tidak berhak; `404` pegawai tidak ditemukan (endpoint per pegawai).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini; **kompilasi belum dibuktikan** |
| QBE preflight/conformance | Dicatat di bagian Metadata | `PASS` | Tidak ada entity/prefix baru |
| Review diff/scope | **8 berkas source milik task ini**, semuanya dalam scope (daftar pada bagian 3.2); tidak ada migration, dependency, atau konfigurasi berubah. Setelah itu pihak lain (`PAY-BE-FIX-011`) menambah perubahan pada dua berkas yang sama (`WfpSalaryAssignmentController.cs`, `WfpSalaryAssignmentDtos.cs`) dan satu berkas lain (`PayrollSalaryRangeRuleService.cs`); angka `git diff --stat` kedua berkas itu kini bercampur dan tidak lagi mencerminkan task ini saja | `PASS` untuk cakupan task ini | Pengecekan: field `AuthorizationStatus`, `OperationalStatusCode`, `IsPayrollUsable` pada kedua berkas itu masih utuh; tambahan pihak lain hanya field gaji dasar dan keabsahan master pada bagian lain |
| Akhir baris | Tidak ada berkas dengan akhir baris campuran: setiap berkas yang diubah murni LF atau murni CRLF. **Koreksi 20 Nov 2026:** versi awal baris ini menyatakan "seluruh baris CRLF, sama dengan HEAD", yang tidak tepat. Blob di `HEAD` adalah LF, sedangkan sebagian working copy CRLF karena `core.autocrlf=true`; keduanya normal dan git menormalkannya saat diff. Metode ukur awal (`grep -c $'\r'`) tidak andal dan diganti `tr -cd '\r' \| wc -c` | `PASS` | Hitung CR per berkas dibanding jumlah baris |
| Pemeriksaan statis kompilasi | Setiap simbol baru punya definisi dan pemakaian; tidak ada bentrok nama pada `GetDetailAsync`; tipe `_dbContext.Users` dan properti `DisplayName/UserName/Email/UserCode` sama dengan pemakaian yang sudah ada | `PASS` (hanya bacaan kode) | Pencarian rujukan |
| Diagnostik IDE pada file yang diubah | Tidak ada severity `Error`/`Warning`; hanya saran gaya yang sudah ada sebelumnya | `PASS` (hanya satu file diperiksa) | Hook editor |
| Teks `OperationalStatus` lama tetap identik | Urutan nonaktif → menunggu persetujuan (hanya bila disyaratkan) → terjadwal → berakhir → berlaku dipertahankan | `PASS` (penelusuran kode) | `WfpSalaryAssignmentController.MapResponse` |
| Verifikasi kontrak API | Method, path, hak akses, dan parameter baru tercatat pada bagian 4 | `PASS` (dari source) | Controller |
| Verifikasi runtime / Swagger | Tidak dilakukan | `NOT RUN` | Server dev tidak boleh dijalankan; tidak ada akses terotorisasi ke data pegawai pada screenshot |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` (build dan server dev dilarang).

**Nilai kebijakan runtime.** `appsettings.json` dan `appsettings.Development.json` hanya memuat `Payroll:Policy:EnableBpjsInPayroll`; kunci `RequireApprovedSalaryAssignment` tidak ada, jadi nilai bawaan kode (`false`) berlaku. Override lewat environment variable di server **tidak dapat diperiksa** dari sini.

**Tidak dijalankan:** build, restore, runtime, dan pembacaan data `WorkforceProfileId 33793d1d-c34f-43e5-9940-c727f754e45c` dari database. Klasifikasi baris itu hanya dapat dibuktikan dengan penelusuran aturan di atas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Backend membedakan persetujuan aktual dari kelayakan Payroll | Terpenuhi pada source | Bagian 2 dan 3.2 |
| `RequireApprovedSalaryAssignment=false` dan penetapan valid → status otomatis, bukan `PendingApproval` | Terpenuhi pada source | `AuthorizationStatusOf`; tabel contoh |
| `RequireApprovedSalaryAssignment=true` dan belum disetujui → `PendingApproval`, tidak dapat dipakai | Terpenuhi pada source | `IsPayrollUsable` = `false`; pembaca Payroll tidak diubah |
| `ApprovedAt`/`ApprovedByUserId` tidak diisi atau dipalsukan | Terpenuhi | Tidak ada penulisan; `AutoByPolicy` hanya label turunan |
| Kompatibilitas field lama | Terpenuhi | Hanya penambahan field; `OperationalStatus` lama identik |
| Konsisten pada Create, Update, Detail, List, Summary | Sebagian | List, Detail, Summary, riwayat, dan rekap memakai aturan yang sama. Respons `Create`/`Update` hanya membawa pesan dan `ApprovalStatus` (tidak membawa objek penetapan), sehingga tidak ada status baru di sana |
| Perubahan nominal, tanggal efektif, histori tidak terjadi | Terpenuhi | Tidak ada penulisan data |
| Status konsisten dengan Readiness dan Payroll Run | Terpenuhi pada source | Pembaca tidak diubah dan sudah mengikuti kebijakan |
| Build berhasil | **Belum terpenuhi** | `NOT RUN` |
| Hasil runtime pada data pegawai contoh | **Belum terpenuhi** | `NOT RUN` |

Skenario UAT sisi backend, **hanya penelusuran kode, belum dieksekusi**: UAT-01 (`AutoByPolicy`/`Current`/`true`), UAT-02 (`PendingApproval`/`false`), UAT-03 (`ManualApproved` dengan `ApprovedAt` dari data), UAT-04 (`Scheduled`), UAT-05 (`Inactive`), UAT-06 (data lama tanpa `ApprovedAt` terbaca `AutoByPolicy` tanpa penulisan), UAT-08 (riwayat, rekap, detail memakai satu fungsi), UAT-09 (filter `isApproved` tetap persetujuan manual; filter baru terpisah), UAT-10 (`BaseSalary` tidak berubah), UAT-11 (penolakan irisan utama tidak tersentuh), UAT-12 (tidak ada akses ke snapshot Payroll Run). UAT-07 mengikuti aturan siklus hidup `Update` yang tidak diubah (`HasFinancialChange` membatalkan persetujuan).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tanggal acuan memakai `DateTime.UtcNow.Date`, sama dengan pembacaan yang sudah ada. Pada pukul 00.00–07.00 WIB, hari itu masih dihitung sebagai hari sebelumnya; ini perilaku lama yang tidak diubah |
| Masalah yang diketahui | **Pra-ada, tidak diubah:** membuat penetapan utama baru yang tidak beririsan membuat `ClearPrimaryAsync` menurunkan penetapan utama lama menjadi non-utama walau masa berlakunya masih berjalan (dipaksa indeks unik). Selama itu Payroll tidak membayar baris tersebut (hanya penetapan utama yang dibayar). Layar kini menampilkannya sebagai `NotPrimary`, bukan "otomatis sah". Pelonggaran memerlukan perubahan skema dan keputusan pemilik |
| Risiko tersisa | Kompilasi belum dibuktikan. Nilai kebijakan runtime (environment variable) belum dibuktikan. Filter `authorizationStatus`/`operationalStatus` pada rekap diterjemahkan ke SQL dan belum pernah dijalankan terhadap Npgsql |
| Perubahan sampingan | `NONE` oleh task ini. **Dicatat, milik pihak lain dan tidak disentuh:** (1) `docs/.../frontend/PAY-FE-FIX-008.md` berubah (+25 baris, "Putaran 3") pada 14:20; (2) `PAY-BE-FIX-011` menambah `PayrollSalaryRangeRuleService.ResolveSalaryAssignmentBasisAsync` dan mengubah `WfpSalaryAssignmentController.cs` serta `WfpSalaryAssignmentDtos.cs` (gaji dasar ditentukan server, `MasterValidityStatus`). Karena controller itu dipakai bersama, kompilasi gabungan kedua pekerjaan baru dapat dibuktikan setelah `dotnet build` oleh pemilik |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah. Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` (gabungan dengan `PAY-BE-FIX-011`), lalu membuka layar Riwayat dan Rekap Penetapan Gaji dengan data pegawai contoh untuk membuktikan `PAY-FE-FIX-010` |

```text
$ git status --short   (setelah `PAY-FE-FIX-010` selesai; HEAD d642a9df)
 M Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpSalaryAssignmentController.cs        <- task ini + PAY-BE-FIX-011
 M Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforceCoreCrossEmployeeController.cs <- task ini
 M Areas/Corporate/HumanResource/WorkforceCore/DTOs/WfpSalaryAssignmentDtos.cs                     <- task ini + PAY-BE-FIX-011
 M Areas/Corporate/HumanResource/WorkforceCore/DTOs/WorkforceCoreCrossEmployeeDtos.cs              <- task ini
 M Areas/Corporate/HumanResource/WorkforceCore/Services/WfpSalaryAssignmentRules.cs                <- task ini
 M Areas/Corporate/HumanResource/WorkforceCore/Services/WorkforceCoreCrossEmployeeQueryService.cs  <- task ini
 M Areas/Corporate/PayrollManagement/DTOs/PayrollCompensationDtos.cs                               <- task ini
 M Areas/Corporate/PayrollManagement/Services/PayrollCompensationService.cs                        <- task ini
 M Areas/Corporate/PayrollManagement/Services/PayrollSalaryRangeRuleService.cs                     <- PAY-BE-FIX-011 (bukan task ini)
 M docs/module-blueprints/payroll-management/payroll-gaji/task/report/frontend/PAY-FE-FIX-008.md   <- pihak lain (bukan task ini)
?? docs/module-blueprints/payroll-management/payroll-gaji/task/report/backend/PAY-BE-FIX-010.md    <- task ini
?? docs/module-blueprints/payroll-management/payroll-gaji/task/report/frontend/PAY-FE-FIX-010.md   <- task ini
```
