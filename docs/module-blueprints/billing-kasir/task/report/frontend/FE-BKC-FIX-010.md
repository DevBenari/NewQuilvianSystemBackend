# Laporan Perubahan Frontend — `FE-BKC-FIX-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-010` (ad-hoc, di luar roadmap, dependency frontend yang dicatat di `BE-BKC-FIX-006`) |
| Judul | Field "Persentase Co-Payment" pada form master data Insurance Coverage Rule kini read-only, otomatis mengikuti "Persentase Coverage" (100 - CoveragePercent) |
| Slice | Lanjutan `BE-BKC-FIX-006` (backend) |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan payload/skema; field `coPaymentPercent` tetap dikirim di payload, isinya sekarang selalu turunan (backend sudah mengabaikannya sejak `BE-BKC-FIX-006`, ini murni mencerminkan itu di UI) |
| Wewenang UI | `REUSE` murni — `BaseGroupedEditorForm` sudah mendukung override `getFieldDisabled`/`getDisabledReason` (dipakai via `formProps`, mekanisme yang SUDAH dipakai `insurance-coverage-rules-form-view.jsx`), tidak ada base component baru/extend |
| Dependency | `BE-BKC-FIX-006` (backend, sudah live) |
| Klasifikasi | `LOW` — murni penguncian satu field form + kalkulasi tampilan, tidak mengubah alur submit/validasi inti |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `use-master-data-insurance-coverage-rule-editor.jsx`, `insurance-coverage-rules-form-view.jsx`, `insurance-coverage-rule-constants.jsx` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai, lint bersih. **Belum diverifikasi hidup** — dev server (Turbopack HMR) belum memuat ulang perubahan (pola staleness yang sama seperti task-task sebelumnya di sesi ini) |

---

## 1. Keadaan yang ditemukan di awal

`BE-BKC-FIX-006` mengubah backend supaya `CoPaymentPercent` SELALU diturunkan server-side dari
`CoveragePercent` (`100 - CoveragePercent`), mengabaikan apa pun yang dikirim client. Tapi form
master data Insurance Coverage Rule (create/update) MASIH menampilkan field "Persentase Co-Payment"
sebagai input bebas yang bisa diisi manual — pengguna bisa mengetik nilai di situ, form-nya sukses
disimpan, TAPI nilai itu diam-diam diabaikan backend. Ini berpotensi membingungkan (pengguna
mengira sudah mengatur co-payment secara independen, padahal tidak berpengaruh apa pun).

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: admin yang mengelola master data Insurance Coverage Rule.

**Sebelum**: "Persentase Co-Payment (%)" adalah field bebas, terpisah dari "Persentase Coverage (%)"
— bisa diisi angka apa pun 0-100 tanpa keterkaitan ke Coverage, walau backend sudah tidak
memakainya untuk kalkulasi apa pun (`BE-BKC-FIX-006`).

**Sesudah**: field "Persentase Co-Payment" terkunci (read-only, tidak bisa diketik), nilainya
otomatis mengikuti `100 - Persentase Coverage` — berubah LIVE begitu pengguna mengetik di field
"Persentase Coverage". Saat membuka rule LAMA yang mungkin datanya sudah tidak konsisten (dibuat
sebelum `BE-BKC-FIX-006`), form langsung menghitung ulang nilai tampilannya dari `CoveragePercent`
yang tersimpan, bukan menampilkan angka `CoPaymentPercent` mentah yang mungkin sudah usang.

**Jalur tidak normal**: field lain yang (di masa depan) memakai `field.dependsOn` tetap mengikuti
perilaku disabled bawaan (`isFilled` pada field dependency) — override ini HANYA menambah kasus
khusus untuk `coPaymentPercent`, tidak mengganti seluruh mekanisme disabled form.

---

## 3. Perubahan yang dikerjakan

### 3.1 `use-master-data-insurance-coverage-rule-editor.jsx`

- Helper baru `deriveCoPaymentPercent(coveragePercent)` — `100 - Math.Clamp(Number(coveragePercent), 0, 100)` (fallback 0 bila bukan angka valid).
- `handleChange`: saat `name === "coveragePercent"`, `next.coPaymentPercent` ikut dihitung ulang via helper itu (side-effect live, sama pola dengan penanganan `itemType` yang sudah ada di function yang sama).
- `useEffect` pemetaan `detail → form` (mode update): `coPaymentPercent` hasil `mapInsuranceCoverageRuleToForm` DITIMPA dengan hasil `deriveCoPaymentPercent(mapped.coveragePercent)` — supaya data lama yang mungkin tidak konsisten (dari sebelum `BE-BKC-FIX-006`) tidak menampilkan angka usang.
- `getFieldDisabled`/`getDisabledReason` baru diekspor dari hook — replikasi PERSIS logika default `BaseGroupedEditorForm` (`field.dependsOn` + `isFilled`, diimpor dari `@/utils/ui/base-ui-utils`, util yang sama dipakai base component itu sendiri) DITAMBAH kasus khusus: `field.name === "coPaymentPercent"` selalu `disabled: true` dengan alasan "Otomatis dihitung dari Persentase Coverage (100% - Persentase Coverage)".

### 3.2 `insurance-coverage-rules-form-view.jsx`

`formProps` (sudah ada, dipakai untuk `title`/`submitLabel`/dst) ditambah `getFieldDisabled:
editor.getFieldDisabled` dan `getDisabledReason: editor.getDisabledReason` — diteruskan ke
`BaseGroupedEditorForm` lewat mekanisme spread `{...formProps}` yang SUDAH ADA di
`BaseGroupedEditorView` (`base-grouped-editor-view.jsx` baris 176), TIDAK perlu mengubah base
component apa pun.

### 3.3 `insurance-coverage-rule-constants.jsx`

Deskripsi kedua field diperbarui supaya konsisten dengan perilaku baru:
- `coveragePercent`: "...Persentase Co-Payment akan otomatis mengikuti (100% dikurangi nilai ini)."
- `coPaymentPercent`: "Otomatis dihitung dari Persentase Coverage (100% - Persentase Coverage) - tidak bisa diisi manual."

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Field "Persentase Co-Payment" read-only | `REUSE` | `BaseGroupedEditorForm` sudah punya prop override `getFieldDisabled`/`getDisabledReason` (dipakai lewat `formProps` yang sudah ada) - tidak ada base component baru/extend, murni mengisi hook yang sudah disediakan base component |

**`UI GATE`**: tidak ada — murni REUSE lewat hook override yang sudah disediakan base component.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` atas ketiga file | Berhasil tanpa error | `PASS` |
| Uji hidup (Playwright, form update rule "Coverage Allianz Kategori Radiologi", CoveragePercent=75) | `coPaymentPercent` value tampil 25 (BENAR, data lama sudah konsisten) TAPI `disabled` masih `false` dan mengetik 60 di CoveragePercent TIDAK mengubah CoPaymentPercent — dicek ulang, HTML halaman TIDAK mengandung teks deskripsi baru yang baru saja ditambahkan ("Otomatis dihitung dari Persentase Coverage"), mengonfirmasi ini staleness dev server (Turbopack belum reload), BUKAN bug source | `MANUAL TEST: BLOCKED (dev server stale)` |

**Tidak dijalankan:** component test (instruksi eksplisit pengguna sepanjang sesi ini — tanpa file
test).

---

## 5. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Menunggu dev server (Turbopack) reload perubahan — pola staleness yang sama seperti beberapa task sebelumnya di sesi ini, bukan indikasi bug |
| Data lama | Rule yang datanya sudah tidak konsisten (`CoveragePercent+CoPaymentPercent != 100`, dari sebelum `BE-BKC-FIX-006`) akan langsung menampilkan nilai `CoPaymentPercent` yang BENAR (turunan) begitu form dibuka — pengguna tinggal klik simpan (tanpa perlu mengubah apa pun) untuk mempersiskan data tersimpan, sesuai catatan `BE-BKC-FIX-006` § "Langkah berikutnya" |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Status Git | Modified: `use-master-data-insurance-coverage-rule-editor.jsx`, `insurance-coverage-rules-form-view.jsx`, `insurance-coverage-rule-constants.jsx`. Belum staged/commit |
| Langkah berikutnya | Restart/reload frontend dev server, verifikasi ulang: field "Persentase Co-Payment" terkunci abu-abu/tidak bisa diketik, dan mengetik di "Persentase Coverage" langsung mengubah angkanya secara live |

