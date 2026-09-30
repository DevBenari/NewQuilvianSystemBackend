# FE-FIN-016 — Revisi urutan menu Master Data dan helper text field Mata Uang (Rekening Bank)

- TASK ID: FE-FIN-016
- SUMBER PERMINTAAN: **Instruksi langsung Yasmin (Product/Domain Owner Finance), 30 September 2026 — bukan dari chain `grill-me`/`design-business-module`/`plan-module-delivery`.** Revisi ini bukan kemampuan baru dan tidak mengubah requirement/kontrak apa pun — murni penataan ulang urutan tampilan menu yang sudah ada dan perubahan satu kalimat teks bantuan pada field yang sudah ada. Task ID dipakai untuk konsistensi jejak laporan modul ini saja (melanjutkan `FE-FIN-015`), bukan klaim bahwa ini berasal dari roadmap.
- TASK TYPE: TOUCHED LEGACY (dua perubahan kecil pada file yang sudah berjalan) — nol route, nol permission, nol logic, nol komponen baru
- COMPLEXITY: TRIVIAL
- MODEL: Claude Sonnet 5
- TASK MODE: FRONTEND
- WRITE TARGET: `QuilvianSystemFrontendDev` — `src/utils/menu-sidebar/menu-items.jsx` (urutan array), `src/lib/constants/finance/master-data/bank-account/bank-account-constants.jsx` (satu string `description`)

- FILES INSPECTED:
  - `src/utils/menu-sidebar/menu-items.jsx` — ditemukan blok `corporateFinanceMasterData` (baris ±676-699), `subItems` array berisi tiga entri (`Kategori Petty Cash`, `Rekening Bank`, `Mata Uang & Kurs`, dalam urutan itu sebelum diubah). Setiap entri hanya `label`/`key`/`icon`/`pathname` — tidak ada field urutan numerik terpisah (`sortOrder`), urutan tampil murni mengikuti urutan array
  - `src/app/finance/master-data/bank-account/create/page.jsx` — halaman "Tambah Rekening Bank" ternyata hanya pembungkus tipis (`metadata.title` + render `<BankAccountFormView mode="create" />`), rincian field TIDAK ada di sini
  - `src/components/view/finance/master-data/bank-account/add/bank-account-form-view.jsx` — form config-driven lewat `BaseEditorView` + hook `useMasterDataBankAccountEditor`, sumber field ada di `BANK_ACCOUNT_CONFIG`
  - `src/lib/constants/finance/master-data/bank-account/bank-account-constants.jsx` — ditemukan `BANK_ACCOUNT_CREATE_FIELDS` (baris 1-59, dipakai KHUSUS halaman create) berisi field `currencyCode` dengan `description: "Mata uang rekening. Bawaan Rupiah (IDR)."` (baris 53) — dikonfirmasi TERPISAH dari dua definisi `currencyCode` lain di berkas yang sama (baris ±192 kolom tabel daftar, baris ±239 kolom detail view read-only) — keduanya tidak punya `description` sama sekali dan tidak tersentuh task ini
  - `src/components/features/base-features/base-form-control.jsx` — dikonfirmasi `field.description` dirender lewat `<small className={formStyles.hint}>{description}</small>`, dipakai SERAGAM oleh seluruh tipe field (`select`, `text`, `textarea`, date, time, checkbox) di seluruh aplikasi — mengubah isi string TIDAK mengubah struktur/kelas render sama sekali, sehingga typography/spacing/warna otomatis identik dengan komponen existing tanpa perlu satu baris style pun disentuh

- FILES CHANGED:
  - `src/utils/menu-sidebar/menu-items.jsx` — tiga entri `subItems` pada blok `corporateFinanceMasterData` disusun ulang urutannya menjadi Kategori Petty Cash → Mata Uang & Kurs → Rekening Bank (menukar posisi dua entri terakhir). **Nol field diubah** pada tiap entri — `label`/`key`/`icon`/`pathname` seluruhnya identik dengan sebelumnya, hanya posisi dalam array yang berpindah
  - `src/lib/constants/finance/master-data/bank-account/bank-account-constants.jsx` — satu nilai string `description` pada field `currencyCode` milik `BANK_ACCOUNT_CREATE_FIELDS` diubah dari `"Mata uang rekening. Bawaan Rupiah (IDR)."` menjadi `"Tambahkan master mata uang dan kurs melalui menu Mata Uang & Kurs."`. **Nol properti lain pada field ini diubah** — `label`, `type`, `placeholder`, `required`, `optionResource`, `payloadType`, `defaultValue` seluruhnya identik dengan sebelumnya

- IMPLEMENTATION: Kedua perubahan adalah edit minimal pada data/konfigurasi yang sudah ada, bukan penulisan ulang struktur. Urutan menu murni reordering tiga object dalam satu array JS (tidak ada mekanisme "sortOrder" terpisah untuk diubah). Helper text field murni penggantian nilai string pada satu baris config-driven form — komponen render (`BaseEditorView` → `base-form-control.jsx`) sudah sepenuhnya generic dan tidak disentuh, sehingga style otomatis konsisten tanpa risiko drift visual.

- WEWENANG UI: Instruksi eksplisit user mencakup kedua perubahan persis seperti diminta (teks lama dan teks baru dikutip verbatim oleh user, urutan menu didiktekan eksplisit) — tidak ada keputusan desain yang didelegasikan ke `DEV_DISCRETION` pada task ini.
- DEPENDENCY BACKEND: Nol. Kedua perubahan murni presentasi frontend, tidak menyentuh payload request/response, endpoint, atau kontrak API apa pun.
- API CONTRACT IMPACT: Nol.
- VISUAL REFERENCE: NOT APPLICABLE — tidak ada mockup/referensi visual yang diberikan; instruksi user sudah cukup presisi (teks lama/baru dikutip persis, urutan menu didaftar eksplisit dengan nomor).
- VALIDASI:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `npm run build`/`dev`/`lint`/`test` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna — pengguna memverifikasi sendiri secara visual |
  | Urutan menu baru sesuai permintaan | Kategori Petty Cash → Mata Uang & Kurs → Rekening Bank, dibaca ulang dari array hasil edit | PASS (review) | Baca ulang berkas pasca-edit |
  | Route/permission/key/icon menu tidak berubah | Ketiga entri: `pathname`, `key`, `icon` byte-identik dengan sebelum edit, hanya urutan array yang berbeda | PASS (review) | Diff manual sebelum/sesudah |
  | Helper text baru sesuai permintaan (persis kutipan user) | `description` field `currencyCode` pada `BANK_ACCOUNT_CREATE_FIELDS` cocok persis | PASS (review) | Baca ulang berkas pasca-edit |
  | Perubahan tidak menyentuh field `currencyCode` lain (tabel daftar, detail view) | Dua definisi lain (`BANK_ACCOUNT_TABLE_COLUMNS`-setara, detail-view fields) tetap utuh, tidak punya `description` sama sekali baik sebelum maupun sesudah | PASS (review) | Baca ulang seluruh berkas, `grep` seluruh kemunculan `currencyCode` |
  | Style helper text mengikuti component existing | Dikonfirmasi lewat pembacaan `base-form-control.jsx` — `description` dirender `<small className={formStyles.hint}>` yang sudah dipakai seragam untuk semua field, tidak diubah sama sekali oleh task ini | PASS (review, STRUKTURAL — bukan visual, lihat MANUAL TEST) | Baca ulang komponen render |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED, bukan diverifikasi visual di browser.**

- MANUAL TEST: **NOT FEASIBLE** dari sesi ini — memerlukan `npm run dev` dan pengecekan visual di browser (tampilan sidebar dan halaman Tambah Rekening Bank), yang sengaja tidak dijalankan sesuai instruksi pengguna. Pengguna disarankan memeriksa: (1) sidebar Finance > Master Data menampilkan tiga menu dalam urutan baru, (2) field Mata Uang pada Tambah Rekening Bank menampilkan teks bantuan baru dengan tampilan yang identik (ukuran, warna, jarak) dengan helper text field lain pada form yang sama.
- WARNINGS: NONE
- KNOWN ISSUES: NONE
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  src/utils/menu-sidebar/menu-items.jsx
  M  src/lib/constants/finance/master-data/bank-account/bank-account-constants.jsx
  ```

- NEXT RECOMMENDED STEP: Pengguna menjalankan `npm run dev` (atau setara) dan memeriksa kedua perubahan secara visual di browser sebagai bukti akhir — task ini tidak memerlukan task lanjutan lain.
