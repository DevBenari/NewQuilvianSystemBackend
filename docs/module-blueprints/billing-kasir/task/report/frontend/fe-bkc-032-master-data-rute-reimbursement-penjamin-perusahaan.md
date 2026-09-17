# FE-BKC-032 — Master Data Rute Reimbursement Penjamin Perusahaan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-032` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`); layar `FE-MPY-06` |
| Task type | Frontend, fitur master data CRUD baru penuh (7 berkas + 2 registrasi, sesuai `rules/frontend/master-data-feature-standard.md`) |
| Task mode | `FRONTEND` (backend read-only — 9 endpoint, DTO, dan seluruh aturan validasi `CompanyGuarantorReimbursementRouteController`/`Service` dibaca langsung dari source, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-042` ✅ (9 endpoint `CompanyGuarantorReimbursementRouteController` dikonfirmasi ada persis sesuai kontrak, dibaca langsung dari controller/DTO/service) |
| Status task | **Source selesai.** Seluruh 7 berkas + 2 registrasi dibuat/diubah. **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

## Ringkasan untuk pembaca umum

Layar Master Data baru di menu **Administrator › Master Data › Rute Reimbursement Penjamin**.
Admin master data memakainya untuk menentukan, per perusahaan penjamin, apakah tagihan
kunjungannya **ditanggung sendiri oleh perusahaan itu** atau **diteruskan ke sebuah asuransi
mitra** — beserta prioritas pencocokan, status rute bawaan, dan masa berlaku. Bentuknya mengikuti
pola daftar+formulir master data baku yang sudah dipakai puluhan fitur lain di repository ini,
sehingga tidak ada tampilan baru yang perlu dipelajari ulang oleh pengguna.

Satu aturan isian mengikat dari dokumen arsitektur: begitu pengguna memilih tipe rute
"Menanggung Sendiri (SELF)", isian **Asuransi Mitra langsung hilang dari formulir** dan nilainya
dikosongkan — bukan sekadar berubah abu-abu. Ini mencegah data asuransi lama tersimpan diam-diam
padahal rutenya sudah bukan tipe asuransi.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-083`; `MPY-DEC-008`. `frontend-roadmap.md` § `FE-BKC-032`.
`03-frontend-architecture.md` § "Peta butir menu" dan § "Skema fitur — `FE-MPY-06`". Acceptance
frontend `#69` (butir menu terdaftar dan terjangkau peran berwenang).

**Temuan yang wajib dilaporkan:**

1. **Entitas ini tidak punya field kode bisnis sama sekali** — berbeda dari setiap fitur master
   data pendahulunya (`categoryCode`, `itemCode`, dst). Dikonfirmasi dari
   `CompanyGuarantorReimbursementRouteResponse`/`CreateCompanyGuarantorReimbursementRouteRequest`:
   tidak ada properti kode apa pun. `codeField`/`codeKey` pada config diset `null`, `codeKeys: []`,
   dan identitas tampilan memakai `companyGuarantorName` (dianggap sebagai `nameField`) — bukan
   sebuah workaround, murni mengikuti bentuk data yang memang tidak punya kode.
2. **`companyGuarantorId` HANYA ada pada form create, sama sekali tidak ada pada form update** —
   bukan sekadar `disabled`. Dikonfirmasi dari `UpdateCompanyGuarantorReimbursementRouteRequest`
   (tidak punya properti `CompanyGuarantorId` sama sekali) dan
   `CompanyGuarantorReimbursementRouteService.BuildUpdateFields()` (daftar field metadata backend
   sendiri juga tidak menyebutkan field ini untuk mode update). Karena pola template yang ada
   (`petty-cash-category`) mengasumsikan `updateFields` adalah superset dari `createFields` (field
   yang berbeda hanya dikunci lewat `updateOnly`), pola itu **tidak bisa dipakai apa adanya** di
   sini — `companyGuarantorId` justru hilang total di arah sebaliknya (ada di create, tidak ada di
   update). Solusinya: dua daftar field literal terpisah persis mengikuti kedua `BuildCreateFields`/
   `BuildUpdateFields` backend, dan `utils.getVisibleFields(mode)` (pola diambil dari
   `lab-rejection-reason-utils.jsx`, fitur lain yang sudah menghadapi masalah identik lewat flag
   `createOnly`) dipakai di `buildInitialForm`/`validateForm`/`buildPayload` — bukan sebuah
   `formFields` tunggal yang diasumsikan superset.
3. **`isActive` tampil pada form CREATE, bukan `updateOnly` seperti pola mayoritas fitur lain.**
   Dikonfirmasi `CreateCompanyGuarantorReimbursementRouteRequest.IsActive` ada dengan bawaan
   `true`, dan `BuildCreateFields()` backend eksplisit menyertakan `isActive` (`SortOrder = 9`).
   Kalau field ini disalin sebagai `updateOnly: true` mengikuti pola `petty-cash-category` tanpa
   diperiksa ulang, admin master data kehilangan kemampuan menonaktifkan rute langsung saat
   membuatnya.
4. **Select `companyGuarantorId`/`insuranceProviderId` TIDAK memakai pola manual
   `optionMap`+dispatch thunk dari `rules/frontend/master-data-feature-standard.md` bagian 8** —
   sempat diimplementasikan begitu, lalu diganti setelah ditemukan
   `lib/hooks/select/select-resource-registry.js`: kedua relasi ini **sudah terdaftar** di sana
   (`administratorCompanyGuarantors` dan `insuranceProviders`, lengkap dengan `endpoint`,
   `valueKey`, `labelKeys`, `filterKeys`). `BaseEditorField`/`useSelectResource` otomatis memuat,
   mencari, dan memuat-lagi opsi select ini sendiri begitu `field.optionResource` cocok dengan key
   registry — tanpa perlu dispatch thunk manual maupun membangun `optionMap` di hook editor sama
   sekali. Memakai jalur manual di sini akan **menduplikasi fetch** (registry tetap jalan sendiri
   di `BaseEditorField` terlepas dari `optionMap` yang disuplai, dan opsi dari registry
   diprioritaskan saat keduanya ada) tanpa manfaat. **Ini kemungkinan celah dokumentasi**: bagian 8
   `master-data-feature-standard.md` hanya mendokumentasikan jalur manual dan tidak menyebutkan
   `select-resource-registry.js` sama sekali padahal registry itu jauh lebih luas dipakai (~80
   resource terdaftar) — layak diperiksa apakah dokumen itu perlu diperbarui supaya task
   berikutnya tidak mengulang jalur manual yang sama untuk relasi yang sebetulnya sudah terdaftar.
   Field `routeType` (enum SELF/INSURANCE_PROVIDER dari `/filters/metadata`, bukan relasi ke
   master lain) tetap memakai jalur `optionMap`/`buildMetadataOptionMap` manual seperti biasa,
   karena memang bukan kasus yang dicakup registry.
5. **Aturan sembunyikan-dan-kosongkan (`FE-MPY-06`, wajib mengikat)** diimplementasikan di dua
   tempat yang saling melengkapi: `use-...-editor.jsx` memetakan ulang `fields` dengan `useMemo`
   yang bergantung `form.routeType` (menyetel `hidden`/`required` dinamis pada field
   `insuranceProviderId`, mekanisme `field.hidden` dikonfirmasi ada dan dihormati
   `base-editor-form.jsx: getVisibleFields`), dan `handleChange` mengosongkan
   `insuranceProviderId` begitu `routeType` berubah keluar dari `INSURANCE_PROVIDER`. `buildPayload`
   punya penjagaan kedua: memaksa `insuranceProviderId` menjadi `null` di payload kapan pun
   `routeType` bukan `INSURANCE_PROVIDER`, terlepas dari isi form — meniru persis bagaimana
   `CreateRouteAsync`/`UpdateRouteAsync` backend sendiri memaksa `null` (baris
   `InsuranceProviderId = normalizedRouteType == "INSURANCE_PROVIDER" ? request.InsuranceProviderId
   : null`).
6. **Tiga aturan validasi backend (`BIL-VAL-087`, `088`, `091`) ditiru di `validateForm`** supaya
   pengguna dapat umpan balik sebelum mengirim: SELF+asuransi terisi ditolak, INSURANCE_PROVIDER
   tanpa asuransi ditolak, tanggal akhir mendahului tanggal mulai ditolak. `BIL-VAL-089`
   (perusahaan asuransi tidak aktif) dan `BIL-VAL-090` (rute bawaan ganda per perusahaan) **sengaja
   tidak ditiru di klien** — keduanya butuh pengecekan basis data (`AnyAsync` ke tabel rute lain)
   yang tidak bisa dan tidak semestinya ditiru di frontend; pesan `422` backend apa adanya yang
   menjadi wasit akhir untuk keduanya.
7. **Filter baris pada daftar hanya menyediakan SATU select dari metadata (`isActive`)**, walau
   endpoint `GET /` backend sebenarnya menerima `companyGuarantorId`, `insuranceProviderId`,
   `routeType`, dan `isDefault` sebagai query parameter tambahan. Ini **bukan kekurangan** —
   `master-data-feature-standard.md` § 7.1 mengunci "Satu select filter dari metadata saja" untuk
   SEMUA layar master data (dikonfirmasi silang ke `inpatient-clearance-item`, fitur lain yang
   punya lebih dari satu kandidat select namun tetap hanya merender satu). `startDate`/`endDate`/
   `customPeriod` sebaliknya benar-benar dikirim ke backend (bukan sekadar tampil) karena
   `GetRoutes` memang menerima ketiganya sebagai parameter nyata.

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND, seluruh elemen REUSE`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Halaman daftar (`Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `Pagination`) | `REUSE` | Struktur identik `petty-cash-category`/`inpatient-clearance-item`, tanpa satu pun komponen baru |
| Halaman detail (`BaseDetailView`) | `REUSE` | Hanya Kembali/Perbarui/Hapus dirender, sesuai § 7.2 standar |
| Halaman formulir (`BaseEditorView`/`BaseEditorForm`/`BaseEditorField`) | `REUSE` | `grouped: false` — tidak memakai `BaseGroupedEditorView`, sesuai arahan "cukup dirujuk, tidak perlu digambar ulang" pada `03-frontend-architecture.md` |
| Select relasi (`companyGuarantorId`, `insuranceProviderId`) | `REUSE` | `select-resource-registry.js` — resource sudah terdaftar sebelumnya, tidak menulis fetch baru |
| Select enum (`routeType`) | `REUSE` | Pola `optionMap`/`buildMetadataOptionMap` identik `inpatient-clearance-item` |
| Isian tanggal (`effectiveStartDate/EndDate`), switch (`isDefault`/`isActive`), angka (`priority`) | `REUSE` | `BaseDateField`/`BaseCheckboxField`/`BaseTextField` bawaan `BaseEditorField` |

## Endpoint yang dikonsumsi

### Administrator / Master Data / Company Guarantor Reimbursement Route

Base URL: `api/v1/administrator/master-data/company-guarantor-reimbursement-routes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter, opsi `RouteTypeOptions`, `SortOptions` | `CompanyGuarantorReimbursementRoute : Read` |
| `GET` | `/summary` | 6 kartu ringkasan (total/aktif/nonaktif/SELF/INSURANCE_PROVIDER/bawaan) | idem |
| `GET` | `/` | Daftar berpaginasi | idem |
| `GET` | `/options` | Opsi ringkas (tidak dikonsumsi task ini — disediakan untuk fitur lain yang merujuk rute ini) | idem |
| `GET` | `/{id}` | Detail | idem |
| `POST` | `/` | Tambah | `CompanyGuarantorReimbursementRoute : Create` |
| `PUT` | `/{id}` | Perbarui | `CompanyGuarantorReimbursementRoute : Update` |
| `PATCH` | `/{id}/status` | Ubah status aktif (thunk tersedia, tidak dipasang ke tombol manapun — sesuai § 7.2) | idem |
| `DELETE` | `/{id}` | Hapus (soft delete, tanpa alasan) | `CompanyGuarantorReimbursementRoute : Delete` |

Relasi yang dikonsumsi lewat `select-resource-registry.js` (bukan endpoint baru yang ditulis task
ini): `GET /v1/administrator/master-data/company-guarantors/options` (resource
`administratorCompanyGuarantors`) dan `GET /v1/administrator/master-data/insurance-providers/options`
(resource `insuranceProviders`).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `.../lib/constants/administrator/master-data/company-guarantor-reimbursement-route/company-guarantor-reimbursement-route-constants.jsx` | **Baru.** `COMPANY_GUARANTOR_REIMBURSEMENT_ROUTE_CONFIG` tunggal — identitas, filter, kolom, ringkasan, field create/update terpisah, detail rows, `metadataOptionSources` |
| `.../lib/state/slice/administrator/master-data/master-data-company-guarantor-reimbursement-route-slice.jsx` | **Baru.** 9 thunk (metadata/summary/list/options/detail/create/update/status/delete), state, reducer, selector |
| `.../utils/administrator/master-data/company-guarantor-reimbursement-route/company-guarantor-reimbursement-route-utils.jsx` | **Baru.** Fungsi murni: `getVisibleFields(mode)`, `buildInitialForm`, `validateForm` (+ 3 aturan silang field), `buildPayload`, `buildDetailRows` |
| `.../lib/hooks/.../company-guarantor-reimbursement-route/use-master-data-company-guarantor-reimbursement-route.jsx` | **Baru.** Controller halaman daftar |
| `.../use-master-data-company-guarantor-reimbursement-route-detail.jsx` | **Baru.** Controller halaman detail |
| `.../use-master-data-company-guarantor-reimbursement-route-editor.jsx` | **Baru.** Controller create/update — `fields` dinamis (hidden/required `insuranceProviderId`), `handleChange` mengosongkan asuransi saat keluar dari `INSURANCE_PROVIDER` |
| `.../components/view/administrator/master-data/company-guarantor-reimbursement-route/master-data-company-guarantor-reimbursement-route-view.jsx` | **Baru.** Halaman daftar |
| `.../detail/company-guarantor-reimbursement-route-detail-view.jsx` | **Baru.** Halaman detail |
| `.../add/company-guarantor-reimbursement-route-form-view.jsx` | **Baru.** Halaman create/update |
| `.../app/administrator/master-data/company-guarantor-reimbursement-routes/{page.jsx, company-guarantor-reimbursement-route-client.jsx, create/page.jsx, [slug]/page.jsx, [slug]/update/page.jsx}` | **Baru.** 5 berkas route tipis. Path URL **jamak** (`-routes`) sesuai kontrak terkunci `03-frontend-architecture.md`; slug internal berkas tetap tunggal mengikuti konvensi `<feature>` fitur master data lain |
| `src/lib/state/store.jsx` | **Diubah.** Tambah import + registrasi reducer `masterDataCompanyGuarantorReimbursementRoute`, ditempatkan di grup Administrator. Entri lain tidak diubah |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Tambah entri menu "Rute Reimbursement Penjamin" di grup Administrator › Master Data, tepat setelah "Perusahaan Penjamin" — memenuhi Acceptance `#69` |

Total: **14 berkas baru, 2 berkas diubah**.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` (via salinan `.mjs` sementara) pada `company-guarantor-reimbursement-route-constants.jsx`, `-utils.jsx`, `-slice.jsx`, dan ketiga hook (`list`/`detail`/`editor`) — keenam `exit 0`, termasuk setelah revisi penghapusan dispatch manual |
| Sintaks berkas JSX baru (3 view + 5 route) | **BELUM DIVERIFIKASI ALAT** | Sama seperti task master data/edit-tagihan sebelumnya — diverifikasi manual lewat pembacaan ulang, mengikuti struktur `petty-cash-category` yang sudah terbukti jalan |
| Butir menu terdaftar dan dapat dijangkau peran berwenang (Acceptance `#69`) | **LULUS (tinjauan kode)** | `menu-items.jsx`: entri baru punya `label`/`key`/`icon`/`pathname` lengkap, format identik entri sekelilingnya; akses ditegakkan reaktif oleh `AccessDeniedGate` di view saat backend menolak `403` |
| Asuransi mitra disembunyikan DAN dikosongkan saat SELF dipilih (aturan mengikat `FE-MPY-06`) | **LULUS (tinjauan kode)** | `fields` di editor hook menyetel `hidden: true` saat `routeType !== "INSURANCE_PROVIDER"` (bukan `disabled`); `handleChange` mengosongkan `insuranceProviderId` di state form; `buildPayload` memaksa `null` di payload sebagai penjagaan kedua |
| `companyGuarantorId` tidak terkirim pada `PUT` (kontrak backend tidak punya field ini) | **LULUS (tinjauan kode)** | `getVisibleFields("update")` mengembalikan `COMPANY_GUARANTOR_REIMBURSEMENT_ROUTE_UPDATE_FIELDS`, yang sama sekali tidak menyertakan field `companyGuarantorId` — `buildPayload` tidak pernah mengiterasinya di mode update |
| 3 aturan validasi client (`BIL-VAL-087/088/091`) menghasilkan pesan sama dengan backend | **LULUS (tinjauan kode)** | Pesan `validateForm` disalin kata-demi-kata dari `CompanyGuarantorReimbursementRouteService.cs` |
| Select relasi memuat opsi tanpa dispatch manual | **LULUS (tinjauan kode)** | `optionResource: "administratorCompanyGuarantors"`/`"insuranceProviders"` dikonfirmasi cocok persis dengan key terdaftar `administrator-select-resources.js`; editor hook tidak lagi mengimpor thunk/slice fitur lain apa pun |
| Verifikasi manual (klik-coba: tambah SELF, tambah INSURANCE_PROVIDER, ubah, hapus, cek `422` asuransi tidak aktif/rute bawaan ganda, cek menu sidebar) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai.** Source selesai dan ditinjau kode menyeluruh terhadap
kontrak backend nyata (controller, DTO, service, dan metadata field yang dipublikasikan backend
sendiri), tetapi lint/test/build serta klik-coba ter-autentikasi belum dijalankan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini — termasuk belum ada konfirmasi bahwa 3 berkas JSX view dan 5 berkas route benar-benar
   valid secara sintaks (hanya ditinjau manual).
2. **Endpoint `/options` milik fitur ini sendiri belum punya konsumen** — disediakan sebagai bagian
   dari 9 thunk baku, tetapi tidak ada fitur lain pada task ini yang merujuknya. Wajar untuk task
   pertama yang memperkenalkan sebuah master data baru.
3. **Celah dokumentasi `master-data-feature-standard.md` § 8** (lihat temuan butir 4) — bagian itu
   hanya mendeskripsikan jalur `optionMap`/dispatch manual dan tidak menyebutkan
   `select-resource-registry.js` yang ternyata sudah menjadi jalur yang jauh lebih umum dipakai
   untuk relasi antar master data di repository ini. Disarankan pemilik dokumen memeriksa dan
   menambahkan bagian tentang registry ini supaya task master data berikutnya tidak salah memilih
   pola untuk relasi yang kebetulan sudah terdaftar.
4. **Label opsi select relasi bergantung sepenuhnya pada `labelKeys` yang sudah dikonfigurasi
   registry** (`companyGuarantorName`/`insuranceProviderName`, dst) — tidak diverifikasi ulang di
   sesi ini apakah kedua resource itu benar-benar mengembalikan data dengan field tersebut saat
   dipanggil sungguhan (hanya dibaca dari definisi registry dan DTO backend, belum diklik-coba).

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build` untuk fitur ini.
2. Klik-coba manual: buka menu Administrator › Master Data › Rute Reimbursement Penjamin; tambah
   satu rute SELF dan satu rute INSURANCE_PROVIDER; ubah salah satu; coba tandai dua rute sebagai
   bawaan untuk perusahaan yang sama (harus ditolak `422`); hapus satu; pastikan select asuransi
   mitra benar-benar hilang begitu tipe rute diganti ke SELF pada form yang sudah terisi.
3. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-032`) dan
   `requirement-traceability.md` (baris `FR-BKC-083`), tandai `✅`, tautkan laporan ini.
4. Pertimbangkan menindaklanjuti temuan butir 4 (celah dokumentasi `select-resource-registry.js`
   di `master-data-feature-standard.md`) secara terpisah dari task ini.
5. `FE-BKC-033` (Master Data Aturan Tanggungan Penjamin Perusahaan, `FE-MPY-07`) adalah task
   berikutnya pada roadmap — bergantung pada `BE-BKC-043`, bukan pada task ini.
