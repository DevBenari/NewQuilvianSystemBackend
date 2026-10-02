# FE-BKC-027 — Kategori Petty Cash (`FE-PC-06`–`08`) dan butir menu "Kategori Petty Cash"

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-027` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, fitur master data baru penuh (7 berkas + 2 registrasi, mengikuti `master-data-feature-standard.md`) |
| Task mode | `FRONTEND` (backend read-only — 9 endpoint `PettyCashCategoriesController` sudah dikunci dan diverifikasi `BE-BKC-035`, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-035` — `✅`, `dotnet build` LULUS, `dotnet test` 12/12 LULUS, terverifikasi 8 September 2026 |
| Status task | Source selesai untuk seluruh Scope task ini (7 berkas + 2 registrasi lengkap). **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — verifikasi akan dilakukan manual oleh pengguna sendiri. Belum di-commit |

## Ringkasan untuk pembaca umum

`FE-BKC-023`–`026` menyelesaikan sisi voucher dan anggaran kas kecil. Task ini adalah master data
terakhir dari rumpun `MVP-15` Petty Cash: layar Kategori Petty Cash (daftar, detail, tambah, ubah)
yang dipakai Finance untuk mengelola kategori pengeluaran (mis. Transportasi, Konsumsi) tanpa
perubahan kode aplikasi. Kategori ini sudah dikonsumsi sebagai saringan pada `FE-BKC-023`
(`getPettyCashCategoryOptions`) — task ini melengkapi sisi CRUD-nya.

Layar mengikuti bentuk baku `master-data-feature-standard.md`: kartu ringkasan (Total/Aktif/
Nonaktif), saringan (termasuk tanggal/periode yang tetap tampil walau belum didukung backend),
tabel dengan status badge, halaman detail dengan tiga aksi (Kembali/Perbarui/Hapus), dan form
tambah/ubah dengan `categoryCode` (wajib, unik, **diketik Finance saat tambah, terkunci saat
ubah**), `categoryName`, `description` (opsional), `isActive`.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-061`–`063`. Kontrak 9 endpoint `Master Data / Petty Cash Category`
(`PettyCashCategoriesController.cs`, terkunci `BE-BKC-035`). `frontend-roadmap.md` § `FE-BKC-027`.

**Temuan riset wajib dilaporkan sebelum implementasi (bukan `DEV_DISCRETION` — bukti dari source,
diungkap transparan karena menyimpang dari referensi yang dituliskan kartu task):**

1. **Referensi yang dituliskan kartu task (Tax Rule, Discount Policy, Room Charge Policy) ternyata
   TIDAK lengkap menurut standar acuannya sendiri.** `master-data-feature-standard.md` § 13
   secara eksplisit mendaftar *"Fitur master data tanpa halaman detail atau tanpa `/summary`"*
   sebagai **"Belum lengkap — Laporkan sebagai kekurangan, jangan jadikan contoh"**, dan menetapkan
   modul rujukan otoritatif sebagai `hr/master-data/job-level` (generasi terbaru), bukan modul
   master data generasi lama. Diperiksa langsung ke source: kelima fitur master data Billing
   Management yang ada (`administration-fee-policy`, `discount-policy`, `room-charge-policy`,
   `tax-rule`, `register`) **sama-sama** tidak punya hook detail (`use-*-detail.js`), view detail,
   maupun folder `add/` — seluruhnya hanya list + form (folder `form/`), pola generasi lama.
   **Task ini adalah fitur master data PERTAMA di Billing Management yang mengikuti bentuk penuh
   tujuh-berkas** (termasuk halaman detail), memakai `hr/master-data/job-level` sebagai template
   struktural untuk bagian yang hilang tadi (hook detail, view detail, `utils.jsx` lengkap,
   folder `add/`).
2. **Kode kategori (`categoryCode`) memakai `isCodeUserInput: true`, berbeda dari `job-level`
   (`isCodeUserInput: false`, kode dialokasikan backend).** Ini **bukan** penyimpangan dari
   standar — § 2.4 dokumen standar sendiri mendefinisikan `isCodeUserInput` sebagai dua nilai sah,
   dan Scope kartu task eksplisit meminta `categoryCode` *"wajib, unik, diketik Finance"*, sama
   seperti pola `MstTaxRule.Code` yang disebut kartu task. Karena `job-level` (rujukan struktural
   utama) tidak mendemonstrasikan kasus `isCodeUserInput: true`, pola perilaku field kode diambil
   dari `tax-rule` (satu-satunya fitur Billing Management yang sudah mendemonstrasikannya): field
   `categoryCode` dirender wajib diisi saat tambah, dan **`disabled: true`** (dikunci, bukan
   disembunyikan) saat ubah — dikonfirmasi bekerja karena `base-editor-field.jsx:105`
   (`isDisabled = Boolean(disabled || field.disabled || field.readOnly)`) benar-benar membaca
   `field.disabled` per field.
3. **Lokasi berkas constants/hooks memakai konvensi lokal modul Billing Management
   (`src/lib/hooks/health-services/billing-management/master-data/<feature>/`), bukan lokasi
   `src/lib/constants/<domain>/master-data/<feature>/` yang dituliskan standar § 1.**
   Presedensi dokumen standar sendiri (baris "Presedensi" di kepala dokumen): *"Bila source
   frontend berbeda, source yang berlaku dan selisihnya dilaporkan."* Diperiksa: **seluruh lima**
   fitur master data Billing Management yang ada memakai lokasi `lib/hooks/.../master-data/
   <feature>/` secara seragam — bukan anomali satu fitur, melainkan konvensi modul yang sudah
   mapan. Delta ini murni lokasi berkas, bukan isi/arsitektur (isi tetap mengikuti kontrak
   sembilan-thunk penuh dari standar, bukan pola lama `getApiBaseUrl`/`buildUrl` yang memang
   ditandai `Legacy` pada § 13). Lokasi slice (`src/lib/state/slice/health-services/billing-
   management/master-data-petty-cash-category-slice.jsx`, flat tanpa sub-folder `master-data/`)
   juga dipertahankan karena berkas ini **sudah ada** sejak `FE-BKC-023` (diperluas, bukan
   dipindah) dan sudah diimpor `use-petty-cash-vouchers.js` — memindah lokasinya akan memaksa
   perubahan berkas di luar scope task ini.
4. **`PettyCashCategoryResponse` backend tidak punya `CreateByName`/`UpdateByName`** (berbeda dari
   `JobLevelResponse`) — baris "Dibuat Oleh"/"Diperbarui Oleh" pada kolom tabel dan detail
   `job-level` **sengaja tidak disalin**; grup audit detail dibatasi pada `createDateTime`,
   `updateDateTime`, `isActive` yang benar-benar ada di response.

## Proses bisnis

Finance menambah kategori baru (kode + nama, opsional deskripsi), mengubah nama/deskripsi/status
kategori yang ada (kode terkunci setelah dibuat), dan menghapus kategori yang belum pernah dipakai
voucher mana pun. Percobaan menghapus kategori yang sudah dipakai voucher ditolak server
(`PettyCashCategoryInUseException` → `400`) dan pesannya diteruskan apa adanya lewat `ConfirmModal`/
toast galat pada halaman detail — layar tidak menduga-duga pemakaian kategori sendiri sebelum
mengirim permintaan hapus.

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND yang mengubah perilaku default, seluruh elemen REUSE — tujuh berkas source adalah pengisian ulang pola job-level/tax-rule, bukan komponen baru`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Kerangka halaman list (`Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `AccessDeniedGate`) | `REUSE` | Pola identik `master-data-job-level-view.jsx` |
| Saringan tanggal/periode/status/jumlah baris | `REUSE` | `FilterDatePicker` × 2, `FilterSelect` × 2 — pola identik `job-level`; tanggal/periode tetap tampil+aktif walau backend belum mendukungnya (§ Keputusan, tidak ada di `supportedFilterKeys`) |
| Status badge kolom Status | `REUSE` | `StatusBadge` dengan `label`/`pill`, pola identik `job-level` |
| Halaman detail (`Kembali`/`Perbarui`/`Hapus`, tanpa aksi status) | `REUSE` | `BaseDetailView` + `BaseButton` — pola identik `job-level-detail-view.jsx`; aksi status ada di hook tapi sengaja tidak dipasang tombol (§ 7.2 standar) |
| Form tambah/ubah | `REUSE` | `BaseEditorView` — pola identik `job-level-form-view.jsx` |
| Field `categoryCode` terkunci saat ubah | `REUSE` | `field.disabled: true` dibaca `base-editor-field.jsx` — dikonfirmasi bukan `EXTEND` (tidak mengubah kode komponen, murni prop yang sudah didukung); pola perilaku field diambil dari `tax-rule` (§ Keputusan butir 2) |
| Field `isActive` (switch, hanya saat ubah) | `REUSE` | Tipe `"switch"` sudah didukung `base-editor-field.jsx` (`CHECKBOX_TYPES`); pola identik `job-level` |
| Field `description` (textarea, opsional) | `REUSE` | Tipe `"textarea"`, pola identik `job-level` |
| Token route privat (detail/update) | `REUSE` | `registerPrivateRouteToken`/`resolvePrivateRouteToken` apa adanya, tanpa perubahan |
| Route tipis (`page.jsx`, `[slug]/page.jsx`, dst.) | `REUSE` | Disalin persis dari `job-level`, termasuk daftar token rute cadangan (`add`/`create`/`edit`/`new`/`update`) dan batas panjang token |
| Butir menu sidebar | `REUSE` | Format objek identik entri lain `menu-items.jsx`; ikon `RiSettingsLine` — konsisten dengan **seluruh** entri lain grup Master Data Billing Management yang memang memakai ikon seragam ini (beda dari entri `subMenu` tingkat atas yang memakai ikon berbeda-beda) |

Tidak ada satu pun elemen `NEW` atau `EXTEND` yang mengubah perilaku default komponen — seluruh
tujuh berkas source murni mengisi ulang `<FEATURE>_CONFIG`, thunk, dan JSX komposisi sesuai pola
`job-level`/`tax-rule` yang sudah terbukti jalan pada fitur lain. Tidak ada gerbang yang menunggu
keputusan pengguna sebelum implementasi bisa lanjut.

## Endpoint yang dikonsumsi

| Endpoint | Method | Dipakai untuk | Sejak task backend |
| --- | --- | --- | --- |
| `.../petty-cash-categories/filters/metadata` | `GET` | Metadata filter (default filter, page size options) | `BE-BKC-035` |
| `.../petty-cash-categories/summary` | `GET` | Tiga kartu ringkasan (Total/Aktif/Nonaktif) | `BE-BKC-035` |
| `.../petty-cash-categories` | `GET` | Tabel daftar kategori | `BE-BKC-035` |
| `.../petty-cash-categories/options` | `GET` | Saringan Kategori `FE-BKC-023` (sudah dikonsumsi sejak sebelumnya, dipertahankan apa adanya) | `BE-BKC-035` |
| `.../petty-cash-categories/{id}` | `GET` | Halaman detail | `BE-BKC-035` |
| `.../petty-cash-categories` | `POST` | Form tambah | `BE-BKC-035` |
| `.../petty-cash-categories/{id}` | `PUT` | Form ubah | `BE-BKC-035` |
| `.../petty-cash-categories/{id}/status` | `PATCH` | Tersedia di slice/hook (thunk `updatePettyCashCategoryStatus`), **tidak dipasang ke tombol mana pun** — sesuai § 7.2 standar (aksi status tidak tampil di halaman detail) | `BE-BKC-035` |
| `.../petty-cash-categories/{id}` | `DELETE` | Tombol Hapus di halaman detail (tanpa body — `requireDeleteReason: false`, dikonfirmasi dari `Delete(Guid id, ...)` controller tanpa `[FromBody]`) | `BE-BKC-035` |

Sembilan dari sembilan endpoint baseline dikonsumsi. Tidak ada delta kontrak — tidak ada endpoint
yang seharusnya ada tapi tidak dipakai, dan tidak ada endpoint baru yang diminta backend.

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/hooks/.../master-data/petty-cash-category/petty-cash-category-constants.js` | **Baru.** `PETTY_CASH_CATEGORY_CONFIG` tunggal — identitas, routing, `idKeys`/`codeKeys`/`nameKeys`, `isCodeUserInput: true`, `filterDefaults`/`supportedFilterKeys`/`unsupportedFilterKeys`, `sortOptions`, `summaryConfig` (3 kartu, field asli `PettyCashCategorySummaryResponse`), `listColumns` (5 kolom), `formFields`/`editorConfig` (`createFields` 3 field, `updateFields` 4 field termasuk `categoryCode` terkunci + `isActive`), `detailFields` (tanpa `createByName`/`updateByName` — § Keputusan butir 4), `messages` |
| `src/utils/.../master-data/petty-cash-category/petty-cash-category-utils.jsx` | **Baru.** Fungsi murni: `getId`/`getCode`/`getName`/`getIsActive`, `buildInitialForm`/`mapToForm`, `validateForm`, `buildPayload`, `buildDetailRows`, `safeString` (menyembunyikan UUID), `createToast`, `getErrorMessage`. Pola karakter kontrol dibangun dari `String.fromCharCode` (bukan literal `\u00XX`) untuk menghindari byte kontrol mentah di source |
| `src/lib/state/slice/.../master-data-petty-cash-category-slice.jsx` | **Diubah** (milik `FE-BKC-023`, sebelumnya hanya `getPettyCashCategoryOptions`). Diperluas jadi 9 thunk penuh (`getPettyCashCategoryFilterMetadata`/`Summary`/`List`/`Options` (dipertahankan)/`Detail`, `createPettyCashCategory`, `updatePettyCashCategory`, `updatePettyCashCategoryStatus`, `deletePettyCashCategory`), state 9-loading/9-error penuh, `sanitizeQueryParams`/`sanitizeMutationPayload`/`assertValidId`. **Export `getPettyCashCategoryOptions`/`selectPettyCashCategoryOptions`/`selectPettyCashCategoryOptionsLoading` dipertahankan persis** (nama, signature, perilaku) — `use-petty-cash-vouchers.js` (`FE-BKC-023`) tidak tersentuh |
| `src/lib/hooks/.../master-data/petty-cash-category/use-master-data-petty-cash-category.jsx` | **Baru.** Controller halaman list — filter+paginasi, hidrasi `DefaultFilter` sekali (`useRef`), `cleanRequestParams` hanya kirim key yang didukung backend |
| `.../use-master-data-petty-cash-category-detail.jsx` | **Baru.** Controller halaman detail — resolusi token, `goBack`/`goUpdate`/`openDeleteConfirm`/`handleDelete`; `openStatusConfirm`/`handleStatusChange` disediakan tapi tidak dipasang ke tombol (§ 7.2 standar) |
| `.../use-master-data-petty-cash-category-editor.jsx` | **Baru.** Controller create+update — `handleChange` mengunci `categoryCode` saat mode update; `handleSubmit` validasi dulu, baru kirim, lalu daftar token + redirect detail |
| `src/components/view/.../master-data/petty-cash-category/master-data-petty-cash-category-view.jsx` | **Baru.** View list |
| `.../detail/petty-cash-category-detail-view.jsx` | **Baru.** View detail — tiga aksi saja |
| `.../add/petty-cash-category-form-view.jsx` | **Baru.** View form tambah/ubah |
| `src/app/health-services/billing-management/master-data/petty-cash-category/page.jsx` | **Baru.** Route list |
| `.../petty-cash-category-client.jsx` | **Baru.** Client wrapper |
| `.../create/page.jsx` | **Baru.** Route tambah |
| `.../[slug]/page.jsx` | **Baru.** Route detail (token privat, menolak token cadangan) |
| `.../[slug]/update/page.jsx` | **Baru.** Route ubah |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu entri menu "Kategori Petty Cash" ditambahkan ke `subItems` grup Master Data Billing Management (bukan `subMenu` tingkat atas — sesuai Scope kartu task) |

Total: **13 berkas baru, 2 berkas diubah**. Tidak ada berkas milik `FE-BKC-022`–`026` lain yang
tersentuh selain perluasan `master-data-petty-cash-category-slice.jsx` yang memang tercatat di atas
(dan sudah diverifikasi tidak mengubah export yang dipakai `FE-BKC-023`).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint .` | **SKIPPED** — atas permintaan eksplisit pengguna ("tanpa test... karena saya akan lakukan secara manual") | Tidak dijalankan sesi ini |
| `npm run test:unit` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| `npm run build` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| Sembilan thunk memetakan tepat ke sembilan endpoint baseline | **LULUS (tinjauan kode)** | Dicocokkan satu-satu terhadap `PettyCashCategoriesController.cs` — lihat tabel Endpoint di atas |
| `categoryCode` tidak dapat diubah saat mode update | **LULUS (tinjauan kode)** | `PETTY_CASH_CATEGORY_UPDATE_FIELDS[0].disabled === true`; `use-master-data-petty-cash-category-editor.jsx` `handleChange` juga menolak perubahan terprogram (`name === config.codeField && safeMode === "update"`) sebagai penjagaan berlapis |
| Halaman detail hanya menampilkan Kembali/Perbarui/Hapus | **LULUS (tinjauan kode)** | `petty-cash-category-detail-view.jsx` `renderDetailActions` hanya tiga `BaseButton`; `openStatusConfirm`/`handleStatusChange` dari hook tidak direferensikan di view sama sekali |
| Filter tanggal/periode tetap tampil dan aktif walau tidak dikirim ke backend | **LULUS (tinjauan kode)** | `master-data-petty-cash-category-view.jsx` merender `FilterDatePicker` × 2 + `FilterSelect` periode tanpa `disabled` bersyarat; `cleanRequestParams` (hook list) tidak menyertakan `startDate`/`endDate`/`customPeriod` |
| Tidak ada UUID tampil di UI | **LULUS (tinjauan kode)** | `safeString` (utils) menolak nilai berbentuk UUID via `isUuidValue`; navigasi detail/update memakai `registerPrivateRouteToken`/`resolvePrivateRouteToken`, bukan id mentah di URL |
| Export `FE-BKC-023` pada slice tidak berubah | **LULUS (tinjauan kode)** | `getPettyCashCategoryOptions`/`selectPettyCashCategoryOptions`/`selectPettyCashCategoryOptionsLoading` ada persis dengan nama dan endpoint yang sama; `grep` dikonfirmasi tidak ada pemakaian lain yang bergantung pada string tipe action Redux internal |
| Tidak ada byte kontrol mentah tertulis di source (regression check internal sesi ini) | **LULUS** | `grep -P '[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]'` pada seluruh 14 berkas baru — nol match |
| Pesan "kategori sudah dipakai voucher tidak dapat dihapus" | **BELUM DIVERIFIKASI** — perlu voucher nyata berkategori terpasang | Jalur kode meneruskan `error.message` dari respons `400` `PettyCashCategoryInUseException` apa adanya lewat toast; tidak ada component test ditulis (`tanpa test`) |
| Verifikasi manual (browser: tambah, ubah kode terkunci, nonaktifkan, hapus terpakai vs tidak terpakai, saringan tanggal/periode/status) | **NOT FEASIBLE (sesi ini) / akan dilakukan pengguna** | Tidak ada kredensial login pada sesi ini; sesuai permintaan eksplisit, pengguna akan menjalankan verifikasi manual sendiri |

**Task ini belum bisa ditandai selesai.** Tidak ada lint/test/build yang dijalankan sesi ini (atas
permintaan eksplisit pengguna) dan tidak ada verifikasi manual — seluruh bukti di atas murni
tinjauan kode/statis, termasuk pemeriksaan literal byte kontrol yang memang dijalankan sesi ini.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — akan dilakukan pengguna sendiri (permintaan eksplisit)
- AUTOMATED TEST: SKIPPED (atas permintaan eksplisit pengguna) — lint/test:unit/build tidak dijalankan

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat tooling maupun browser pada sesi ini** — sama seperti
   `FE-BKC-024`–`026`, murni tinjauan kode statis.
2. **Fitur master data pertama di Billing Management yang mengikuti bentuk penuh tujuh-berkas.**
   Lima fitur sejenis lain (`administration-fee-policy`, `discount-policy`, `room-charge-policy`,
   `tax-rule`, `register`) masih memakai pola lama (tanpa detail, folder `form/`) — bukan
   diperbaiki task ini (di luar scope), dicatat di sini sebagai temuan agar task lanjutan yang
   menyentuhnya sadar akan kesenjangan ini, bukan menganggapnya preseden yang benar.
3. **`master-data-petty-cash-category-slice.jsx` kini dipakai dua task (`FE-BKC-023` dan
   `FE-BKC-027`).** Perubahan pada slice ini (8 thunk baru, state penuh) tidak menyentuh export
   yang dipakai `FE-BKC-023`, tetapi risiko regresi silang tetap ada sampai keduanya diverifikasi
   lint/build bersamaan.
4. **`sortOptions`/`sortableFields` dari `/filters/metadata` belum bisa dipetakan 1:1 ke label
   Bahasa Indonesia** — backend hanya mengirim `List<string> SortableFields` (nama field mentah),
   bukan `{value,label}[]` seperti asumsi `readSortOptions`; hook tetap memakai fallback statis
   `config.sortOptions` karena `normalizeSelectOptions` terhadap `SortableFields` mentah akan
   menghasilkan label yang sama dengan value-nya (tidak diterjemahkan). Tidak berdampak fungsional
   (select sortir memang sengaja tidak dirender di UI per pola `job-level`), dicatat sebagai
   kesenjangan data metadata murni.
5. **Aktivitas paralel `FE-BKC-022` tetap tidak tersentuh** (lihat laporan `FE-BKC-023` § Risiko
   butir 5) — tidak berubah pada task ini.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint`/`test:unit`/`build` dan verifikasi manual (tambah kategori,
   coba ubah kode yang terkunci, nonaktifkan, hapus kategori tak terpakai, coba hapus kategori
   yang sudah dipakai voucher dan pastikan pesan galat muncul, saringan tanggal/periode/status)
   sendiri.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-027`) dan
   `requirement-traceability.md` menautkan laporan ini, tandai `✅`.
3. Pertimbangkan task tersendiri (di luar `MVP-15`) untuk menyamakan lima fitur master data
   Billing Management lain ke bentuk penuh tujuh-berkas (§ Risiko butir 2) — tidak mendesak,
   tidak memblokir rumpun Petty Cash.
