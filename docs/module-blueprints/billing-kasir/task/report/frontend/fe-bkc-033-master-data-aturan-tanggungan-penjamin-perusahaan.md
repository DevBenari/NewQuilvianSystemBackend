# FE-BKC-033 — Master Data Aturan Tanggungan Penjamin Perusahaan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-033` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`); layar `FE-MPY-07` |
| Task type | Frontend, fitur master data CRUD baru penuh (7 berkas + 2 registrasi, sesuai `rules/frontend/master-data-feature-standard.md`), dibangun **semirip mungkin** dengan layar "Aturan Cakupan Asuransi" (`insurance-coverage-rules`) yang sudah ada — sesuai arahan eksplisit `frontend-roadmap.md` § `FE-BKC-033` |
| Task mode | `FRONTEND` (backend read-only — 9 endpoint, DTO, dan seluruh aturan validasi `CompanyGuarantorCoverageRuleController`/`Service` **sudah dibangun penuh sebelumnya**, dikonfirmasi ada persis sesuai kontrak, dibaca langsung dari source) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-043` ✅ (backend `CompanyGuarantorCoverageRuleController`/`Service`/DTO dikonfirmasi ada lengkap — controller, service dengan derivasi Urun Biaya server-side, dan metadata field `BuildCreateFields()`) |
| Status task | **Source selesai.** Seluruh 7 berkas + 2 registrasi dibuat/diubah. **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

## Ringkasan untuk pembaca umum

Layar Master Data baru di menu **Health Services › Master Data › Aturan Tanggungan Penjamin**,
tepat di sebelah layar "Aturan Cakupan Asuransi" yang sudah ada — sengaja dibuat memakai bentuk
visual dan pola interaksi yang sama persis, supaya admin master data yang sudah terbiasa dengan
layar asuransi langsung bisa memakai layar ini tanpa belajar pola baru. Bedanya: aturan di sini
berlaku untuk **perusahaan penjamin** (bukan penyedia asuransi), dan punya satu dimensi tambahan
khusus perusahaan penjamin — **Golongan Karyawan**.

Satu aturan isian mengikat dari dokumen arsitektur: isian **Persentase Urun Biaya (Co-Payment)
selalu hanya-baca dan terisi otomatis** mengikuti Persentase Tanggungan yang diketik admin (Urun
Biaya = 100% dikurangi Persentase Tanggungan) — karena nilainya sepenuhnya diturunkan dan
diautoritasi server, bukan sesuatu yang boleh diisi manual.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-064`, `FR-BKC-066`, `FR-BKC-067`; `MPY-DEC-008`. `frontend-roadmap.md` §
`FE-BKC-033`. Acceptance frontend `#69` (butir menu terdaftar dan terjangkau peran berwenang) —
isian Urun Biaya hanya-baca terbukti.

**Temuan yang wajib dilaporkan:**

1. **Backend fitur ini SUDAH selesai dibangun penuh sebelum task ini dimulai** —
   `CompanyGuarantorCoverageRuleController`, `CompanyGuarantorCoverageRuleService` (dengan
   derivasi Urun Biaya server-side yang authoritative, identik polanya dengan
   `InsuranceCoverageRuleController`), DTO lengkap, model EF, konfigurasi EF, migration, dan
   registrasi DI — semuanya dikonfirmasi ada dan dibaca langsung dari source. Task ini murni
   pekerjaan frontend terhadap kontrak yang sudah terkunci, tidak ada backend yang disentuh.
2. **"Semirip mungkin" diterapkan pada bentuk VISUAL dan pola interaksi (bagian form
   berkelompok/`BaseGroupedEditorView`, gaya kolom tabel, badge status), bukan menyalin seluruh
   implementasi internal Insurance Coverage Rule apa adanya.** Ditemukan dan SENGAJA TIDAK ditiru
   beberapa hal pada implementasi Insurance yang sudah ada:
   - Field `isCovered`/`isExcluded` pada form Insurance adalah **field mati** — tidak ada properti
     yang cocok sama sekali pada `CreateInsuranceCoverageRuleRequest` maupun
     `CreateCompanyGuarantorCoverageRuleRequest`; keduanya tidak ditambahkan di sini.
   - Field "Kelas Pasien" pada form Insurance adalah **teks bebas** (`patientClassName`) padahal
     backend keduanya (Insurance maupun Company Guarantor) punya `PatientClassId` (Guid FK) yang
     benar-benar divalidasi (keberadaan + status aktif) dan endpoint opsi resminya sendiri
     (`/api/v1/health-services/master-data/patient-classes/options`, sesuai
     `BuildCreateFields()` backend). Di sini `patientClassId` diimplementasikan sebagai select
     asli, bukan teks bebas.
   - Tipe item **"TariffCategory"**, bukan **"ServiceCategory"** seperti pada Insurance —
     dikonfirmasi `CompanyGuarantorCoverageRuleService.NormalizeItemType` mengubah
     "ServiceCategory" menjadi "TariffCategory" sebelum disimpan, dan `ItemTypeOptions` metadata
     backend sendiri mendaftarkan "TariffCategory".
   - `maxQuantityPerVisit`/`maxQuantityPerMonth` diperlakukan sebagai **desimal**, bukan bilangan
     bulat seperti pada Insurance — dikonfirmasi kedua kolom itu `decimal?` pada
     `CreateCompanyGuarantorCoverageRuleRequest` (berbeda dari `int?` milik Insurance).
   - `effectiveStartDate`/`effectiveEndDate` **tidak diwajibkan** — metadata field backend sendiri
     menandai keduanya `RequiredType="optional"`, berbeda dari form Insurance yang mewajibkan
     `effectiveStartDate` walau DTO backend-nya sendiri nullable/opsional.
   - Mekanisme penebakan identitas pengguna sisi klien milik Insurance
     (`resolveCurrentUserAudit` — mendekode JWT dari localStorage/sessionStorage, mencoba ~30
     nama field kandidat) dan pemindaian DOM untuk alasan hapus (`readDeleteReasonFromDom`)
     **tidak ditiru sama sekali** — keduanya tidak diperlukan: `CreateBy`/`UpdateBy` sepenuhnya
     ditentukan server dari klaim JWT (`GetCurrentUserId()`), `CreateByName`/`UpdateByName` sudah
     diresolusi penuh di response (`MapToResponse`), dan modal konfirmasi hapus
     (`confirm-modal.jsx`) memang sudah memanggil `onConfirm(event, cleanReason)` — alasan hapus
     didapat langsung dari parameter kedua, tanpa perlu memindai DOM.
3. **Select relasi (`companyGuarantorId`, `tariffId`, `drugId`, `drugCategoryId`, `procedureId`,
   `tariffCategoryId`, `patientClassId`) SEMUANYA memakai `select-resource-registry.js`** —
   dikonfirmasi ketujuh resource ini sudah terdaftar (`administratorCompanyGuarantors` di
   `administrator-select-resources.js`; enam lainnya di `health-service-select-resources.js`),
   masing-masing dengan endpoint yang cocok persis dengan `OptionsSource` pada `BuildCreateFields()`
   backend. Karena itu, hook editor **tidak memerlukan** thunk multi-endpoint bergaya
   `getInsuranceCoverageRuleMasterOptions` (`Promise.allSettled` ke 6+ endpoint,
   `buildMasterOptionMap`/`mergeOptionMaps`) seperti Insurance — `BaseGroupedEditorField`/
   `useSelectResource` memuat opsi ketujuh field itu sendiri (fetch, cari, muat lagi) begitu
   `field.optionResource` cocok dengan key registry. `itemType`/`coverageStatus` (enum lokal dari
   `/filters/metadata`, bukan relasi) tetap memakai `optionMap` biasa dengan fallback statis.
4. **Metadata `BuildCreateFields()` backend hanya mengumumkan 25 field, padahal DTO
   Create/Update punya 33 field yang benar-benar berfungsi dan dipersist ke database.** Delapan
   field yang tidak diumumkan (`coPaymentPercent`, `coPaymentAmount`, `benefitPlanName`,
   `maxQuantityPerVisit`, `maxQuantityPerMonth`, `maxAmountPerVisit`, `maxAmountPerMonth`,
   `sortOrder`) tetap disertakan di form ini — terutama `coPaymentPercent` yang justru menjadi
   *satu-satunya field yang secara eksplisit diminta task ini* (isian hanya-baca, terisi otomatis).
   Kemungkinan celah dokumentasi metadata backend, sama pola temuannya dengan `FE-BKC-032`.
5. **Aturan hanya-baca Urun Biaya diimplementasikan identik dengan pola
   `use-master-data-insurance-coverage-rule-editor.jsx` yang sudah terbukti benar**: `handleChange`
   menghitung ulang `coPaymentPercent` setiap `coveragePercent` berubah, `mapToForm` menurunkan
   ulang nilainya saat data lama dimuat (menjaga tampilan tetap konsisten walau data tersimpan
   sempat tidak sinkron), `getFieldDisabled`/`getDisabledReason` mengunci field itu di
   `BaseGroupedEditorForm` dengan alasan yang dijelaskan ke pengguna, dan `buildPayload` menghitung
   ulang nilainya sekali lagi sebagai penjagaan kedua sebelum dikirim. Backend tetap wasit akhir —
   `CreateRuleAsync`/`UpdateRuleAsync` mengabaikan apa pun yang dikirim klien untuk field ini dan
   menurunkannya sendiri (`Math.Clamp(100m - coveragePercent, 0m, 100m)`).
6. **`companyGuarantorId` TETAP ada dan dapat diedit pada form update** (berbeda dari
   `FE-BKC-032`) — dikonfirmasi `UpdateCompanyGuarantorCoverageRuleRequest : Create...Request`
   tanpa override apa pun, dan `UpdateRuleAsync` benar-benar menerapkan
   `entity.CompanyGuarantorId = request.CompanyGuarantorId`. Karena itu satu daftar field
   (`COMPANY_GUARANTOR_COVERAGE_RULE_FORM_FIELDS`) melayani kedua mode tanpa perlu pemisahan
   create/update seperti `FE-BKC-032` — `BuildUpdateFields()` backend sendiri hanya
   mengembalikan `BuildCreateFields()` apa adanya, mengonfirmasi keduanya identik.
7. **Alasan hapus bersifat opsional di kontrak backend** (`DeleteCompanyGuarantorCoverageRuleRequest.
   DeleteReason` adalah `string?` tanpa `[Required]`), berbeda dari asumsi hard-required pada
   thunk `deleteInsuranceCoverageRule` milik Insurance. Dialog konfirmasi hapus tetap memakai
   `requireReason: true` (bawaan `HealthServicesMasterDataDetailView`, tidak ditimpa) sebagai
   praktik akuntabilitas yang wajar untuk penghapusan data master — bukan karena backend
   mewajibkannya. Catatan tambahan: bila diisi, `DeleteRuleAsync` backend menimpa `Description`
   milik baris dengan alasan hapus tersebut sebelum soft-delete (`entity.Description =
   NormalizeNullableText(deleteReason)`) — perilaku backend apa adanya, tidak diubah di sini.
8. **Filter baris pada daftar hanya menyediakan SATU select dari metadata (`isActive`)**, walau
   endpoint `GET /` backend menerima `companyGuarantorId`, `itemType`, `coverageStatus`,
   `benefitPlanCode`, `employeeGrade` sebagai query parameter tambahan — konsisten dengan
   `master-data-feature-standard.md` § 7.1 ("Satu select filter dari metadata saja"), sama seperti
   keputusan yang sudah diambil pada `FE-BKC-032`.

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND, seluruh elemen REUSE`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Halaman formulir berkelompok (`BaseGroupedEditorView`/`BaseGroupedEditorForm`) | `REUSE` | Komponen dan pola yang identik dipakai `insurance-coverage-rules-form-view.jsx` — dipilih (bukan `BaseEditorView` datar milik `FE-BKC-032`) justru untuk memenuhi arahan "semirip mungkin" |
| Halaman daftar (`Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `Pagination`) | `REUSE` | Struktur dan gaya kolom (badge status pil, sel dua baris) identik `master-data-insurance-coverage-rules-view.jsx` |
| Halaman detail (`HealthServicesMasterDataDetailView` → `BaseDetailView`) | `REUSE` | Komponen domain yang sama dipakai Insurance Coverage Rule |
| Select relasi (7 field) | `REUSE` | `select-resource-registry.js` — seluruh resource sudah terdaftar sebelumnya |
| Select enum (`itemType`, `coverageStatus`) | `REUSE` | Pola `optionMap` + fallback statis identik Insurance |
| Isian tanggal, switch, angka, textarea | `REUSE` | `BaseGroupedEditorField` bawaan |
| Penguncian field hanya-baca (`coPaymentPercent`) | `REUSE` | `getFieldDisabled`/`getDisabledReason` — mekanisme yang sama persis sudah ada di `BaseGroupedEditorForm` dan dipakai Insurance |

## Endpoint yang dikonsumsi

### Health Services / Master Data / Company Guarantor Coverage Rule

Base URL: `api/v1/health-services/master-data/company-guarantor-coverage-rules`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter, `ItemTypeOptions`, `CoverageStatusOptions` | `CompanyGuarantorCoverageRule : Read` |
| `GET` | `/summary` | Kartu ringkasan (total/aktif/nonaktif/covered/notCovered/partialCovered/needApproval) | idem |
| `GET` | `/` | Daftar berpaginasi | idem |
| `GET` | `/options` | Opsi ringkas (tidak dikonsumsi task ini) | idem |
| `GET` | `/{id}` | Detail | idem |
| `POST` | `/` | Tambah (201 Created) | `CompanyGuarantorCoverageRule : Create` |
| `PUT` | `/{id}` | Perbarui | `CompanyGuarantorCoverageRule : Update` |
| `PATCH` | `/{id}/status` | Ubah status aktif (thunk tersedia, tidak dipasang ke tombol manapun — sesuai § 7.2) | idem |
| `DELETE` | `/{id}` | Hapus (soft delete, alasan opsional di backend) | `CompanyGuarantorCoverageRule : Delete` |

Relasi yang dikonsumsi lewat `select-resource-registry.js` (bukan endpoint baru yang ditulis task
ini): `company-guarantors`, `tariffs`, `drugs`, `drug-categories`, `procedures`,
`tariff-categories`, `patient-classes` — masing-masing `GET .../options`.

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `.../lib/constants/health-services/master-data/company-guarantor-coverage-rule-constants.jsx` | **Baru.** 33 field form (25 dari `BuildCreateFields()` + 8 field DTO nyata yang tidak diumumkan metadata), 8 grup form bergaya Insurance, kolom tabel, filter, editor config |
| `.../lib/state/slice/health-services/master-data/master-data-company-guarantor-coverage-rule-slice.jsx` | **Baru.** 9 thunk (metadata/summary/list/options/detail/create/update/status/delete), state, reducer, selector |
| `.../utils/health-services/master-data/company-guarantor-coverage-rule/company-guarantor-coverage-rule-utils.jsx` | **Baru.** Satu berkas utils (konsisten `master-data-feature-standard.md`, bukan 3 berkas terpisah seperti Insurance): `mapToForm`, `validateForm`, `buildPayload`, `buildDetailRows`, `deriveCoPaymentPercent`, token route privat, formatter tampilan |
| `.../lib/hooks/.../company-guarantor-coverage-rules/use-master-data-company-guarantor-coverage-rule.jsx` | **Baru.** Controller halaman daftar |
| `.../use-master-data-company-guarantor-coverage-rule-detail.jsx` | **Baru.** Controller halaman detail — `handleDelete(event, reason)` membaca alasan langsung dari parameter `ConfirmModal`, tanpa pemindaian DOM |
| `.../use-master-data-company-guarantor-coverage-rule-editor.jsx` | **Baru.** Controller create/update — derivasi+kunci `coPaymentPercent`, pengosongan field item saat `itemType` berganti |
| `.../components/view/.../company-guarantor-coverage-rules/master-data-company-guarantor-coverage-rules-view.jsx` | **Baru.** Halaman daftar |
| `.../company-guarantor-coverage-rule-table-columns.jsx` | **Baru.** Definisi kolom (badge status, sel dua baris) bergaya Insurance |
| `.../detail/company-guarantor-coverage-rules-detail-view.jsx` | **Baru.** Halaman detail |
| `.../add/company-guarantor-coverage-rules-form-view.jsx` | **Baru.** Halaman create/update (`BaseGroupedEditorView`) |
| `.../app/health-services/master-data/company-guarantor-coverage-rules/{page.jsx, company-guarantor-coverage-rules-client.jsx, create/page.jsx, [slug]/page.jsx, [slug]/update/page.jsx}` | **Baru.** 5 berkas route tipis, dengan penolakan token cadangan (`add/create/edit/new/update`) sesuai `master-data-feature-standard.md` § 7.4 |
| `src/lib/state/store.jsx` | **Diubah.** Tambah import + registrasi reducer `masterDataCompanyGuarantorCoverageRule`, ditempatkan tepat setelah `masterDataInsuranceCoverageRule`. Entri lain tidak diubah |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Tambah entri menu "Aturan Tanggungan Penjamin" di grup Health Services › Master Data, tepat setelah "Aturan Cakupan Asuransi" — memenuhi Acceptance `#69` |

Total: **14 berkas baru, 2 berkas diubah**.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` (via salinan `.mjs` sementara) pada constants, utils, slice, dan ketiga hook — keenam `exit 0` |
| Sintaks berkas JSX baru (3 view + table-columns + 5 route) | **BELUM DIVERIFIKASI ALAT** | Diverifikasi manual lewat pembacaan ulang, mengikuti struktur `insurance-coverage-rules` yang sudah terbukti jalan |
| Butir menu terdaftar dan dapat dijangkau peran berwenang (Acceptance `#69`) | **LULUS (tinjauan kode)** | `menu-items.jsx`: entri baru punya `label`/`key`/`icon`/`pathname` lengkap, format identik entri Insurance Coverage Rule di sebelahnya |
| Urun Biaya (Co-Payment) terbukti hanya-baca dan terisi otomatis | **LULUS (tinjauan kode)** | `getFieldDisabled` mengembalikan `true` untuk `coPaymentPercent` tanpa syarat; `handleChange`/`mapToForm`/`buildPayload` ketiganya menghitung ulang nilainya dari `coveragePercent` — tidak ada jalur yang membiarkan nilai manual pengguna lolos ke payload |
| `patientClassId` sebagai select asli (bukan teks bebas seperti Insurance) | **LULUS (tinjauan kode)** | Field terdaftar `optionResource: "patientClasses"`, cocok dengan resource terdaftar `health-service-select-resources.js` dan `OptionsSource` metadata backend |
| Tipe item `TariffCategory` (bukan `ServiceCategory`) | **LULUS (tinjauan kode)** | `COMPANY_GUARANTOR_COVERAGE_ITEM_TYPE_OPTIONS` dan `ITEM_FIELD_BY_TYPE` keduanya memakai `TariffCategory`, cocok `NormalizeItemType`/`ItemTypeOptions` backend |
| `companyGuarantorId` dapat diedit pada mode update | **LULUS (tinjauan kode)** | Satu daftar field dipakai kedua mode; tidak ada logika yang menyembunyikan/menonaktifkan field ini saat `mode === "update"` |
| Verifikasi manual (klik-coba: tambah tiap tipe item, ubah `companyGuarantorId` pada update, cek Urun Biaya ikut berubah live, hapus dengan/tanpa alasan, cek menu sidebar) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai.** Source selesai dan ditinjau kode menyeluruh terhadap
kontrak backend nyata (controller, service, DTO, dan metadata field backend), termasuk terhadap
implementasi Insurance Coverage Rule yang menjadi rujukan visual "semirip mungkin", tetapi
lint/test/build serta klik-coba ter-autentikasi belum dijalankan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini — termasuk belum ada konfirmasi bahwa berkas JSX (3 view + table-columns + 5 route)
   benar-benar valid secara sintaks (hanya ditinjau manual).
2. **`BaseGroupedEditorView`/`BaseGroupedEditorForm` belum pernah dipakai penulis laporan ini
   sebelum task ini** (`FE-BKC-032` memakai `BaseEditorView` yang datar) — dipelajari dari
   `insurance-coverage-rules-form-view.jsx` dan kode sumber kedua komponen tersebut, tetapi belum
   diklik-coba sungguhan untuk memastikan 8 grup/33 field benar-benar merender dan tervalidasi
   sesuai harapan pada browser nyata.
3. **Celah dokumentasi metadata backend berulang** (lihat temuan butir 4) — kedua fitur
   (`CompanyGuarantorReimbursementRoute` pada `FE-BKC-032` dan `CompanyGuarantorCoverageRule` di
   sini) sama-sama punya field DTO nyata yang tidak diumumkan `BuildCreateFields()`/
   `BuildUpdateFields()`. Pola berulang ini layak diperiksa pemilik backend secara terpisah dari
   task ini.
4. **Endpoint `/options` milik fitur ini sendiri belum punya konsumen** — disediakan sebagai
   bagian dari 9 thunk baku, tetapi tidak ada fitur lain pada task ini yang merujuknya.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build` untuk fitur ini.
2. Klik-coba manual: buka menu Health Services › Master Data › Aturan Tanggungan Penjamin; tambah
   aturan untuk tiap tipe item (Tariff/Drug/DrugCategory/Procedure/TariffCategory); ubah Persentase
   Tanggungan dan pastikan Urun Biaya ikut berubah live serta tetap terkunci; ubah
   `companyGuarantorId` pada form update; hapus satu data dengan dan tanpa mengisi alasan; pastikan
   menu sidebar tampil tepat di sebelah "Aturan Cakupan Asuransi".
3. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-033`) dan
   `requirement-traceability.md` (baris `FR-BKC-064`, `066`, `067`), tandai `✅`, tautkan laporan
   ini.
4. `FE-BKC-034` (Aksesibilitas, privasi, dan regresi lintas layar) adalah task berikutnya pada
   roadmap — bergantung pada `FE-BKC-029`, `030`, `031`, bukan pada task ini.
