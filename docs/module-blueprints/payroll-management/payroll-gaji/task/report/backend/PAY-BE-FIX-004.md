# Laporan Perubahan Backend — `PAY-BE-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-004` (ad-hoc, bukan kartu roadmap) |
| Judul | Konfigurasi PPh dan iuran dikelola HRD (Profil Pajak, Profil Iuran) dan menjadi sumber perhitungan Payroll Run; status konfigurasi eksplisit menggantikan "keduanya nol" |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap, traceability, dan kontrak `PGJ-C1` tidak disentuh (preseden `PAY-BE-FIX-001..003`) |
| Trace | Perintah pemilik 2026-11-18 (master prompt "Finishing Konfigurasi PPh dan Iuran HRD"), DEC-PRL-TAX-001 s.d. 007 |
| Contract version | Tidak tercakup `PGJ-C1`. Kontrak baru bersifat additive; satu aksi izin baru (`WorkforceTaxProfile : ViewAmount`) |
| Klasifikasi | `HIGH` — mesin hitung gaji, kolom baru pada entity yang dibaca banyak layanan, jalur uang |
| Task mode | `BACKEND` (Frontend lewat `PAY-FE-FIX-004`) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-18 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan.** `dotnet build` **NOT RUN** (dilarang; kode belum pernah dikompilasi). Migration **tidak dibuat** (dilarang): **10 kolom baru wajib dibuat lewat DDL pada bagian 7 sebelum Backend dijalankan**, jika tidak setiap query `PrlTax`/`PrlInsurance` gagal. Skema PPh otomatis **BLOCKED** (bagian 4). Uji API dan runtime NOT RUN |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry: `Corporate | PayrollManagement / Payroll | Prl | ACTIVE`). Tidak ada entity baru; kolom baru pada `PrlTax` dan `PrlInsurance` |
| Keberlakuan | `TOUCHED LEGACY` (controller, DTO, `PayrollCalculationService`, `PayrollRunService`, readiness, simulasi); `NEW CODE` untuk `PayrollTaxInsuranceCatalog` dan `PayrollTaxInsuranceRules` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001` (field additive, tanpa endpoint baru selain gerbang izin), `QBE-PERM-001` (tanpa hardcode peran), `QBE-LOG-001/AUD-001` (audit ditambahkan pada tulis profil), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-DB-001/002` (kolom dicatat sebagai kebutuhan DDL terpisah) |
| Checker QBE | **NOT RUN** |
| Hak akses | `WorkforceTaxProfile` (Read/Create/Update/Delete tidak berubah) + aksi baru `ViewAmount` (gerbang nominal PPh manual, pola `PayrollCompensation`). `WorkforceInsuranceProfile` tidak berubah. Payroll Read atas konfigurasi tetap lewat izin yang sudah ada; `PayrollRun.Calculate` **tidak** memberi hak ubah konfigurasi |

---

## 1. Akar masalah

| # | Temuan | Bukti |
| --- | --- | --- |
| 1 | Peringatan "keduanya nol" muncul karena kelengkapan dinilai dari `PrlPayroll.TaxAmount == 0 && InsuranceAmount == 0` | `PayrollProfileRules.IsTaxInsuranceUnset` dipakai `PayrollReadinessEvaluator`, `PayrollCalculationService`, dan `GetEmployeeDetailAsync` |
| 2 | Mesin run aktif membaca nominal dari `PrlPayroll`, bukan dari Profil Pajak/Iuran yang diisi HRD | `PayrollCalculationService`: `Round(profile.TaxAmount)`, `Round(profile.InsuranceAmount)` |
| 3 | Profil Payroll disiapkan otomatis dengan nominal 0 dan tidak ada UI yang mengisinya | `PayrollProfileProvisioningService` |
| 4 | `PrlTax` tidak punya jenis PPh, kategori penerima, atau skema; `TaxMethod` (Gross/GrossUp/Net) dan PTKP adalah satu-satunya atribut. `PrlInsurance` tidak punya status kepesertaan eksplisit (semua flag false tidak dapat dibedakan dari "belum diisi") | model dan DTO |
| 5 | Dua engine pajak/iuran lama (modul "Modern": `PayrollTaxCalculationService`, `PayrollBpjsCalculationService`) masih dapat dipanggil lewat `PayrollRunModernController` pada run yang sama. `PayrollBpjsCalculationService` **menimpa `TotalDeduction` dengan hanya iuran BPJS** dan membuat komponen bertipe `Deduction` berkode `DED_BPJS_*`, yang oleh mesin resmi dibaca sebagai potongan manual: **iuran dapat terpotong dua kali** | `PayrollBpjsCalculationService.cs:398-401`; mesin resmi menghitung semua komponen `Deduction` non-mesin sebagai potongan manual |
| 6 | `PayrollTaxCalculationService` tidak dapat dipakai sebagai sumber kebijakan: tabel TER tertanam di kode tanpa versi atau tanggal berlaku (nilai **tidak diverifikasi** ke PMK 168/2023 pada task ini), kategori `K/I/*` jatuh diam-diam ke kategori A, masa Desember dihitung `bruto × 12` tanpa mengurangi PPh yang sudah dipotong Januari–November, dan `TotalDeduction` ditimpa tanpa iuran/cicilan | `PayrollTaxCalculationService.cs` |
| 7 | Tidak ada master kebijakan tarif PPh berversi di repository (pencarian model `Mst*/Prl*/Pol*` untuk tarif, PTKP, TER, lapisan Pasal 17). Satu-satunya master tarif iuran: `HrdPayrollSetting` (Pengaturan Penggajian), yang sudah dibekukan ke `PrlPayrollRun.ConfigurationSnapshotJson` saat run dibuka | pencarian source |

## 2. Perubahan

- **Model + EF**: `PrlTax` + `TaxArticle`, `RecipientTaxCategory`, `CalculationMode`, `PolicyVersion`, `ExemptionReason`, `DecisionBasis`, `ApprovedManualAmount`. `PrlInsurance` + `ParticipationStatus`, `ParticipationBasis`, `BpjsEmploymentPrograms`. Semua nullable (kosong = **belum dikonfigurasi**, bukan nol sah). `PrlTaxConfiguration`/`PrlInsuranceConfiguration` diperbarui.
- **`Constants/PayrollTaxInsuranceCatalog.cs`** (baru): kosakata kode (`TaxArticles`, `TaxRecipientCategories`, `TaxCalculationModes`, `TaxMethodKinds`, `PayrollConfigurationStatuses`, `PayrollComponentResultStatuses`, `InsuranceParticipationStatuses`, `InsurancePrograms`, `TaxTreatments`, kode komponen iuran, kode komponen jalur lama).
- **`Services/PayrollTaxInsuranceRules.cs`** (baru, murni, satu-satunya tempat aturan): validasi tulis (dipakai controller), penilaian per periode (dipakai mesin run), kesiapan (dipakai kelengkapan data dan Profil Penggajian), pembaca parameter iuran dari snapshot run, dan penjaga "satu mesin aktif".
- **`WfpTaxController`**: field baru pada request/response; validasi jenis PPh × kategori × metode × skema × status subjek pajak; `filters/metadata` mengirim `TaxArticleOptions`, `RecipientTaxCategoryOptions`, `CalculationModeOptions` (dengan `IsSupported`+`Note`); ringkasan `ConfiguredProfile`/`NotConfiguredProfile`; `IsEmployerBorneTax` **diturunkan Backend** dari metode Net; nominal manual hanya terlihat dan hanya dapat ditetapkan pemegang `ViewAmount` (tanpa izin: `null`, bukan 0, dan nominal yang ada **dipertahankan** saat ubah); endpoint gerbang izin `GET {id}/amount`; **audit** pada buat/ubah/ubah status/hapus (konfigurasi dan aktor, tanpa nominal, NPWP, atau data pribadi).
- **`WfpInsuranceController`**: `ParticipationStatus`/`ParticipationBasis`/`BpjsEmploymentPrograms`; validasi kepesertaan eksplisit; `filters/metadata` `ParticipationStatusOptions`, `EmploymentProgramOptions`; ringkasan; audit.
- **`PayrollCalculationService`**: sumber PPh dan iuran = konfigurasi HRD yang dinilai pada periode run; per penerima menulis `TaxSnapshotJson` dan `InsuranceSnapshotJson`; komponen iuran per program (`PAY_INS_*`; pegawai bertipe `Insurance`, pemberi kerja bertipe `EmployerContribution` dengan `EmployerAmount` dan tidak tampil di slip); `TotalEmployerInsuranceContribution` kini terisi dan **tidak** mengurangi gaji bersih; komponen jalur lama (`DED_PPH21`, `DED_BPJS_*`, `ER_BPJS_*`) dibuang dengan peringatan; penahan konfigurasi menjadi `CalculationResult.EmployeeBlockers`/`Blockers` (penerima tetap terhitung dan ditandai, run tidak dapat diajukan).
- **`PayrollRunService.GetEmployeeDetailAsync`** (tetap murni baca): membaca snapshot; `DataIncomplete` diturunkan dari status konfigurasi (bukan nominal nol); respons + `Tax` dan `Insurance`.
- **Kelengkapan data dan Profil Penggajian**: `PayrollReadinessEvaluator.AssessProfile` menerima kesiapan konfigurasi; `TaxInsuranceStatus` ("Sudah/Belum ditetapkan") diturunkan dari status eksplisit; DTO + `TaxConfigurationStatus`, `InsuranceConfigurationStatus`, `*Issues`. `PayrollProfileRules.IsTaxInsuranceUnset` **dihapus**.
- **`PayrollSimulationService`**: run sementara kini membawa snapshot Pengaturan Penggajian (sumber tarif sama dengan run resmi) dan menampilkan penahan konfigurasi sebagai peringatan.
- **Satu mesin aktif**: `PayrollTaxCalculationService` dan `PayrollBpjsCalculationService` menolak (`422`) run yang sudah dihitung mesin resmi.

## 3. Aturan yang dijaga server

1. **Empat konsep terpisah**: jenis PPh (`PPH21`/`PPH26`), kategori penerima (`PERMANENT_EMPLOYEE`/`NON_PERMANENT_EMPLOYEE`/`NON_EMPLOYEE_EXPERT`), metode pajak (`Gross`/`GrossUp`/`Net`), skema (`AUTO_REGULATORY`/`APPROVED_MANUAL_AMOUNT`/`EXEMPT_OR_NOT_APPLICABLE`). Ketiganya yang baru kosong bersamaan = belum dikonfigurasi (kompatibel dengan klien lama); bila salah satu diisi, ketiganya wajib.
2. `PPH26` hanya untuk subjek pajak luar negeri (`IsTaxResident = false`); `PPH21` hanya untuk dalam negeri. Kewarganegaraan asing **bukan** dasar otomatis.
3. Kategori penerima tidak diturunkan dari jabatan atau jenis SDM; dokter tidak otomatis pegawai tetap. PTKP hanya relevan untuk `PPH21` + kategori pegawai.
4. `APPROVED_MANUAL_AMOUNT`: nominal ≥ 0 dan dasar penetapan wajib; metode `GrossUp` ditolak. `EXEMPT_OR_NOT_APPLICABLE`: alasan dan dasar aturan wajib, tanpa nominal. Pengecualian **tidak** diturunkan dari hasil hitung nol.
5. Metode `Gross`: PPh dipotong dari pegawai. `Net`: PPh ditanggung pemberi kerja, **tidak** menjadi potongan dan tidak mengurangi gaji bersih (nominalnya ada di snapshot dan respons). `GrossUp`: blocker, tidak ada nilai fiktif.
6. Iuran: status eksplisit (`ACTIVE`/`NOT_PARTICIPATING`/`NOT_APPLICABLE`); selain `ACTIVE` wajib dasar dan semua program harus nonaktif; `ACTIVE` wajib memilih program. Semua flag false tidak diperlakukan sebagai bukti tidak wajib.
7. Tarif dan batas BPJS dari **Pengaturan Penggajian pada snapshot run** (satu sumber kebijakan, berversi lewat snapshot). Tarif per pegawai pada `PrlInsurance` **diabaikan** (peringatan); override tanpa workflow tidak diizinkan.
8. Nilai status hasil: `NOT_CONFIGURED`, `CALCULATED`, `VALID_ZERO`, `EXEMPT`, `NOT_APPLICABLE`, `ERROR`. Hanya tiga yang pertama setelah `CALCULATED` adalah nol yang sah.
9. Cadangan kompatibilitas: bila HRD **belum** mengonfigurasi dan `PrlPayroll.TaxAmount`/`InsuranceAmount` > 0, nilai itu dipakai dan ditandai (`Source = LEGACY_PROFILE_AMOUNT`, status konfigurasi tetap `NOT_CONFIGURED`, peringatan). Cadangan tidak pernah mengalahkan konfigurasi HRD.
10. Snapshot PPh/iuran disimpan saat hitung dan ikut dibekukan bersama run; run yang disahkan/terkunci tidak dapat dihitung ulang (guard existing), sehingga perubahan HRD berikutnya tidak mengubahnya.

## 4. Blocker per jenis (kebijakan yang sah belum ada)

| Jenis | Status | Alasan |
| --- | --- | --- |
| `AUTO_REGULATORY` untuk semua kombinasi jenis × kategori | **BLOCKED** | Tidak ada master kebijakan tarif PPh berversi (TER, PTKP, lapisan Pasal 17) dan tidak ada akumulasi masa pajak (YTD). `IsAutoRegulatorySupported(...) => false`; skema ditolak saat menyimpan dan, bila ada data lama, menghasilkan penahan (bukan Rp0, bukan formula kategori lain). Tabel TER tertanam di `PayrollTaxCalculationService` **tidak dipakai** |
| PPh 21 pegawai tetap: masa pajak selain terakhir (TER) dan masa pajak terakhir (selisih Pasal 17 atas penghasilan setahun) | **BLOCKED** | Butuh tabel TER berversi dan akumulasi penghasilan/PPh setahun; masa pajak terakhir tidak selalu Desember (berhenti kerja, dll.) |
| PPh 26 dan tarif P3B/treaty | **BLOCKED** | Butuh status P3B dan dokumen pendukung |
| Metode `GrossUp` | **BLOCKED** | Belum ada formula yang sah |
| Dokter bukan pegawai / tenaga ahli (formula) | **BLOCKED** | Formula jenis penerima ini belum ada; kategori dapat dicatat dan PTKP diabaikan |
| `APPROVED_MANUAL_AMOUNT` (Gross dan Net), `EXEMPT_OR_NOT_APPLICABLE` | **IMPLEMENTED — CODE ONLY** | Berdasar keputusan HRD yang tercatat, bukan rumus |
| Iuran BPJS Kesehatan, JHT, JP, JKK, JKM, asuransi swasta | **IMPLEMENTED — CODE ONLY** | Tarif/batas dari Pengaturan Penggajian; dasar upah **NEEDS CONFIRMATION** (bagian 6) |

Akibat praktis: **PPh otomatis tidak tersedia**. Sampai kebijakan tarif berversi disahkan dan dimasukkan, HRD hanya dapat menetapkan nominal manual yang disetujui atau pengecualian berdasar. Penerima tanpa konfigurasi (dan tanpa tetapan lama > 0) **menahan pengajuan run**.

## 5. Kontrak API (additive)

| Endpoint | Perubahan |
| --- | --- |
| `GET/POST/PUT .../workforce-profiles/{id}/tax-profile[/{id}]` | + `taxArticle`, `recipientTaxCategory`, `calculationMode`, `policyVersion`, `exemptionReason`, `decisionBasis`, `approvedManualAmount` (request); respons + `amountsVisible`, `hasApprovedManualAmount`, `approvedManualAmount` (null tanpa `ViewAmount`), `configurationStatus`, `configurationIssues`, `isPtkpRelevant` |
| `GET .../tax-profile/{id}/amount` | **baru**, gerbang izin `WorkforceTaxProfile : ViewAmount` |
| `GET .../tax-profile/filters/metadata` | + `taxArticleOptions`, `recipientTaxCategoryOptions`, `calculationModeOptions`; opsi memuat `isSupported` dan `note` |
| `GET .../tax-profile/summary` | + `configuredProfile`, `notConfiguredProfile` |
| `.../insurance-profile` | + `participationStatus`, `participationBasis`, `bpjsEmploymentPrograms`, `configurationStatus`, `configurationIssues`, `hasIgnoredPerEmployeeRates`; metadata + `participationStatusOptions`, `employmentProgramOptions`; ringkasan + `configuredProfile`, `notConfiguredProfile` |
| `GET payroll-runs/{runId}/employees/{runEmployeeId}` | + `tax` (`PayrollRunTaxResultResponse`) dan `insurance` (`PayrollRunInsuranceResultResponse`, `lines[]` per program, `paidBy` Employee/Employer); `taxCalculationStatus` dipertahankan (kini `Configured` hanya bila konfigurasi PPh dan iuran lengkap) |
| `GET payroll-data-readiness`, `GET payroll-compensations` | + `taxConfigurationStatus`, `insuranceConfigurationStatus`, `taxConfigurationIssues`, `insuranceConfigurationIssues`; `taxInsuranceStatus` diturunkan dari status eksplisit |

Kode: `400` validasi (kombinasi tidak sah, dasar kosong), `403` (menulis nominal tanpa `ViewAmount`), `404`, `422` (jalur lama pada run yang dihitung mesin resmi).

## 6. Keputusan yang dibutuhkan dan risiko

1. **Sumber kebijakan tarif PPh berversi** (TER per kategori A/B/C, PTKP, lapisan Pasal 17, dasar pemotongan, akumulasi masa pajak, masa pajak terakhir, tarif PPh 26) — **NEEDS BUSINESS/TAX DECISION**. Tanpanya `AUTO_REGULATORY` tetap BLOCKED. Referensi regulasi (PP 58/2023, PMK 168/2023) **tidak diverifikasi** pada task ini karena tidak dipakai sebagai tarif.
2. **Dasar upah iuran BPJS**: dipakai gaji pokok + tunjangan tetap + transportasi + tunjangan khusus pegawai (sama dengan jalur lama). Apakah transportasi/tunjangan khusus termasuk upah — **NEEDS CONFIRMATION**; tercatat di `PolicySnapshotJson.insuranceWageBase`.
3. **Pengaturan Penggajian**: nilai bawaan tarif/batas (`PayrollSettingService.GetOrCreateDefaultSettingAsync`, mis. JKK 0,89%) sudah ada sebelum task ini dan **tidak diverifikasi**; tarif JKK bergantung kelas risiko. Perlu ditinjau pemilik kebijakan.
4. **Dampak operasional**: seluruh pegawai tanpa konfigurasi HRD dan tanpa tetapan lama > 0 kini **menahan pengajuan run**. Ini disengaja (permintaan pemilik) tetapi akan terlihat segera pada run berikutnya.
5. **Satu baris konfigurasi per pegawai** (indeks unik) dan ubah menimpa baris: histori konfigurasi ada pada audit dan snapshot run, bukan baris riwayat. Versi/riwayat konfigurasi bila dibutuhkan = keputusan terpisah.
6. **Persetujuan nominal manual**: satu langkah (HRD dengan `Update` + `ViewAmount`); alur validator Finance/Tax tidak dibangun (tidak ada workflow existing).
7. **Jalur "Modern" lama** tetap ada (hanya ditolak pada run yang sudah dihitung mesin resmi). Run yang dihitung jalur lama **sebelum** hitung resmi pertama: komponen lamanya dibuang saat hitung resmi. Penghentian penuh jalur itu = keputusan terpisah.
8. **Slip self-service lama** (`PayrollPayslipDistributionService`) memakai label tertanam "PPh 21 TER PMK 168/2023" dan perkiraan `TotalDeduction × 0,6` untuk iuran bila baris pegawai tidak ada; **tidak diubah**. Slip resmi (`PayrollPayslipService`) memakai komponen run: baris iuran per program dan nama komponen PPh mengikuti konfigurasi; kontribusi pemberi kerja (Amount 0) tidak tampil.
9. Run yang dihitung sebelum task ini tidak punya snapshot PPh/iuran; rincian menampilkan "hitung ulang untuk mendapatkan status konfigurasi" (tidak menuduh run yang sudah dibekukan).
10. Backend tidak membangun pengecekan "PPh sah Rp 0" dari rumus; nol sah hanya dari keputusan HRD (manual 0, pengecualian) atau kepesertaan tidak berlaku.

## 7. Kebutuhan skema (DDL terpisah — TIDAK dibuat atau dijalankan pada task ini)

Idempoten, wajib diterapkan **sebelum** Backend dan Frontend dirilis. Tanpanya semua query `PrlTax`/`PrlInsurance` (hitung, readiness, profil) gagal ("column does not exist").

```sql
ALTER TABLE public."PrlTax"
  ADD COLUMN IF NOT EXISTS "TaxArticle"           varchar(20)   NULL,
  ADD COLUMN IF NOT EXISTS "RecipientTaxCategory" varchar(40)   NULL,
  ADD COLUMN IF NOT EXISTS "CalculationMode"      varchar(40)   NULL,
  ADD COLUMN IF NOT EXISTS "PolicyVersion"        varchar(100)  NULL,
  ADD COLUMN IF NOT EXISTS "ExemptionReason"      varchar(500)  NULL,
  ADD COLUMN IF NOT EXISTS "DecisionBasis"        varchar(1000) NULL,
  ADD COLUMN IF NOT EXISTS "ApprovedManualAmount" numeric(18,2) NULL;

ALTER TABLE public."PrlInsurance"
  ADD COLUMN IF NOT EXISTS "ParticipationStatus"    varchar(30)  NULL,
  ADD COLUMN IF NOT EXISTS "ParticipationBasis"     varchar(500) NULL,
  ADD COLUMN IF NOT EXISTS "BpjsEmploymentPrograms" varchar(50)  NULL;
```

Tanpa `CHECK constraint` baru (nilai dijaga validasi Backend). `PrlPayrollRunEmployee.TaxSnapshotJson` dan `InsuranceSnapshotJson` **sudah ada**. Aksi izin `ViewAmount` pada `WorkforceTaxProfile` muncul di Akses Role setelah sinkronisasi atribut; peran HRD yang menetapkan nominal manual perlu diberi izin itu. Snapshot model EF (`ApplicationDbContextModelSnapshot`) tidak diperbarui (tanpa migration).

## 8. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` / `dotnet restore` | **NOT RUN** (dilarang) |
| `dotnet ef` / migration / DDL | **NOT RUN** — kebutuhan DDL didokumentasikan (bagian 7) |
| Automated test backend | `AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test (TEST_POLICY). Kontrak dikunci dari FE: `payroll-tax-config.test.mjs` mencocokkan kosakata, DTO, route, izin, dan penjaga "satu mesin" dengan source BE |
| Tinjauan kompilasi lewat pembacaan | Jumlah argumen record (20/14/13), nama properti entity, tipe nullable, dan konflik nama lokal diperiksa manual; satu nama parameter fungsi lokal diganti untuk menghindari bayangan. **Belum dikompilasi**, risiko galat kompilasi tetap ada |
| Uji hitung / API / runtime | **NOT RUN** |

TC-01..25: diperiksa **statis** (kode dan uji FE-BE); **tidak ada yang dibuktikan saat dijalankan**. TC-03, TC-04 (formula), TC-05 (formula) **BLOCKED** (bagian 4); TC-21 (persistensi) menunggu DDL.

`git status`: tidak ada yang di-stage atau di-commit. Perubahan tugas sebelumnya masih berada di working tree yang sama.
