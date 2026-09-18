# Laporan Perubahan Frontend — `BE-BKC-FIX-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-010` (pasangan frontend dari task backend dengan ID yang sama — ad-hoc, di luar roadmap, permintaan langsung pengguna lewat `/grill-me` → `/build-module-backend` → `/build-module-frontend`) |
| Judul | Hapus field "Kategori Kena Pajak"/`taxableCategory` dari CRUD Tax Rule (form create/update, kolom list, teks deskripsi) |
| Slice | Perbaikan atas fitur existing (`FE-BKC-002` — Workspace master policy), bukan fitur baru |
| Roadmap | `NOT APPLICABLE` — perbaikan ad-hoc, dikunci lewat amendment decision log modul, bukan task roadmap baru |
| Trace | `BKC-DEC-098`, `BKC-DEC-099` (`NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/00-interview-decisions.md`) |
| Contract version | `BIL-API-1.0` — mengikuti breaking change backend yang sudah dieksekusi (`BE-BKC-FIX-010` sisi backend): field `taxableCategory` sudah tidak ada lagi di request/response `TaxRulesController` maupun query param `GET /options` |
| Wewenang UI | `REMOVE` murni — menghapus satu field dari form/tabel/teks yang sudah ada, tidak ada base component baru maupun perubahan layout selain hilangnya satu field/kolom |
| Dependency | Backend `BE-BKC-FIX-010` — source selesai, `dotnet build` **belum dijalankan pengguna** (lihat laporan backend). Frontend task ini tetap aman dikerjakan lebih dulu karena hanya menghapus field yang sudah tidak dikirim/diterima backend, bukan menambah field baru yang butuh kontrak backend hidup |
| Klasifikasi | `LOW` — penghapusan field murni, nol endpoint/route baru, nol base component baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — lima file (lihat § 2). Backend **tidak disentuh** (sudah selesai di task backend terpisah) |
| Model | Claude Sonnet 5 |
| Tanggal | 16 September 2026 |
| Status | Source selesai. `npm run lint:errors`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna — validasi frontend dijalankan manual). Belum diverifikasi manual di browser |

---

## 1. Konteks

Backend sudah menghapus kolom `TaxableCategory` dari `MstTaxRule` (entity, DTO, service,
controller) atas dasar `BKC-DEC-098`/`099` — lihat
`docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-FIX-010.md`. Task ini adalah
pasangan frontendnya: menghapus field "Kategori Kena Pajak" dari seluruh permukaan CRUD Tax Rule
di UI, supaya form tidak lagi menampilkan/mewajibkan field yang sudah tidak diterima backend.

Audit `grep` atas `taxableCategory`/`TaxableCategory`/"kategori kena pajak" (case-insensitive) di
seluruh `src/` menemukan **lima file** — tidak ada Redux slice, service Axios, maupun filter
dropdown yang memakai field ini (dikonfirmasi: field murni tampilan/form, tidak pernah dipakai
sebagai parameter query terpisah di frontend).

## 2. Perubahan yang dikerjakan

### 2.1 `use-tax-rule-editor.js`

Empat titik dihapus: nilai awal `taxableCategory: ""` (`buildEmptyForm`), pemetaan dari item saat
mode update (`buildFormFromItem`), validasi wajib-isi (`validate`: pesan "Kategori kena pajak
wajib diisi." dihapus), dan pengiriman ke payload (`buildPayload`).

### 2.2 `tax-rule-editor-config.jsx`

Entri field `{ name: "taxableCategory", label: "Kategori Kena Pajak", ... }` dihapus dari array
`getTaxRuleEditorFields`. `helperText` pada `TAX_RULE_EDITOR_CONFIG` yang menyebut "berdasarkan
kategori kena pajak" diringkas menghilangkan frasa itu.

### 2.3 `tax-rule-view.jsx` (halaman list)

Kolom tabel `{ key: "taxableCategory", header: "Kategori Kena Pajak", ... }` dihapus dari
`buildColumns`. Teks `description` pada `Hero` yang menyebut "berdasarkan kategori kena pajak"
diringkas.

### 2.4 `tax-rule-form-view.jsx` (halaman create/update, `BaseEditorView`)

Dua teks deskripsi yang menyebut "kategori kena pajak" diringkas: `description` utama
`BaseEditorView` dan `previewProps.description` (teks pratinjau sebelum submit). File ini tidak
memuat identifier `taxableCategory` — hanya prosa deskriptif, field form-nya sendiri sudah
dikendalikan lewat `tax-rule-editor-config.jsx` (§ 2.2).

**Yang sengaja TIDAK diubah**: field lain pada form (Kode, Nama, Rate, Mode Pembulatan, Aturan
Alokasi, Berlaku Sejak/Sampai, Aktif), kolom lain pada tabel list, tombol aksi (Perbarui/
Nonaktifkan/Aktifkan/Hapus), filter status pada `DataFilter`, dan seluruh Redux
slice/thunk/selector (`master-data-tax-rule-slice`) — tidak ada satu pun yang memakai
`taxableCategory` sejak awal.

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Grep ulang `taxableCategory`/`TaxableCategory`/"kategori kena pajak" di seluruh `src/` | Nol hasil setelah seluruh edit selesai | `PASS` (tinjauan kode) |
| Konsistensi `buildEmptyForm`/`buildFormFromItem`/`validate`/`buildPayload` di `use-tax-rule-editor.js` | Keempat fungsi diperiksa ulang — tidak ada satu pun yang masih mereferensikan `form.taxableCategory` atau `errors.taxableCategory` | `PASS` (tinjauan kode) |
| `getTaxRuleEditorFields` tidak lagi mengekspos field yang dihapus untuk mode `create` maupun `update` | Array field diperiksa — fungsi ini sama untuk kedua mode (tidak ada percabangan mode khusus untuk field ini sebelumnya), field terhapus untuk keduanya | `PASS` (tinjauan kode) |
| `buildColumns` (`tax-rule-view.jsx`) tidak lagi mereferensikan kolom yang dihapus | Diperiksa — fungsi `render` kolom lain tidak bergantung pada urutan/keberadaan kolom `taxableCategory` yang dihapus | `PASS` (tinjauan kode) |
| Redux slice `master-data-tax-rule-slice` tidak terdampak | Diperiksa: slice ini tidak diubah task ini, dan sebelumnya juga tidak pernah memetakan `taxableCategory` sebagai field state/selector terpisah — payload create/update diteruskan mentah dari `buildPayload` | `PASS` (tinjauan kode) |

**`AUTOMATED TEST: BLOCKED`** — `npm run lint:errors`/`test:unit`/`build` tidak dijalankan
(instruksi baku pengguna).
**`MANUAL TEST: NOT FEASIBLE` pada sesi ini** — tidak ada instruksi/kebutuhan konkret untuk
menjalankan `npm run dev` dan membuka browser; perubahan murni penghapusan field/kolom/teks yang
diverifikasi lewat tinjauan kode terhadap seluruh titik pakai yang ditemukan grep.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Seluruhnya tinjauan kode. Belum ada rebuild backend + restart dev server frontend + klik-coba form Tambah/Perbarui Tax Rule pasca-perubahan |
| Ketergantungan pada backend | Backend `BE-BKC-FIX-010` sudah source-complete tapi belum di-`dotnet build` oleh pengguna. Sampai backend benar-benar berjalan dengan kontrak baru, verifikasi hidup end-to-end frontend ini belum bisa dilakukan |
| Perubahan sampingan | `NONE` — nol file baru, nol route baru, nol base component baru/diubah |
| Status Git | Modified: `use-tax-rule-editor.js`, `tax-rule-editor-config.jsx`, `tax-rule-view.jsx`, `tax-rule-form-view.jsx`. **Catatan**: working tree frontend juga memuat perubahan lain yang **bukan** bagian task ini (`.env`, tiga file `menu-pembayaran`/`billing-invoice`, satu file baru `payment-allocation-row.jsx`) — sudah ada sebelum task ini dimulai, sengaja tidak disentuh/dilaporkan sebagai bagian task ini. Belum staged/commit |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` backend lalu `npm run lint:errors`/`test:unit`/`build` frontend; (2) verifikasi manual browser: buka halaman Tax Rule, konfirmasi form Tambah/Perbarui tidak lagi menampilkan field "Kategori Kena Pajak" dan submit berhasil tanpa field itu; (3) konfirmasi kolom "Kategori Kena Pajak" tidak lagi muncul di tabel daftar Tax Rule |
