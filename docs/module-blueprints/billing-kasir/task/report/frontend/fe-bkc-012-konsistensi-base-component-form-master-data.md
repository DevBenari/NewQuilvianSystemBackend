# FE-BKC-012 — Konsistensi Base Component pada Form Master Data Billing

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-012` (task baru, perbaikan konsistensi UI atas scope yang sebelumnya dibangun `FE-BKC-002` "Workspace master policy" — bukan penggantian task itu, hanya bagian form create/update-nya) |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, perbaikan konsistensi UI (bukan perubahan bisnis/kontrak) |
| Task mode | `FRONTEND` |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Pemicu | Screenshot pengguna atas halaman "Tambah Tax Rule" menunjukkan form mentah (`<input>`/`<select>`/`<label>` tanpa styling) — bukan `BaseEditorView` yang jadi konvensi form create/update di seluruh aplikasi (mis. `administrator-bank-form-view.jsx`) |
| Status task | Source selesai ditulis untuk 5 halaman, lint dan build lulus bersih (`.next/server/app/health-services/billing-management/master-data/{tax-rule,discount-policy,administration-fee-policy,room-charge-policy,register}/create` dan `/[id]` terkonfirmasi ter-compile). Belum di-commit. Belum diverifikasi manual ter-autentikasi. |

## Ringkasan untuk pembaca umum

Lima halaman "Tambah"/"Perbarui" pada Master Data Billing (Tax Rule, Discount Policy,
Administration Fee Policy, Room Charge Policy, Register) sebelumnya dibangun dengan elemen form
HTML polos — `<input>`, `<select>`, `<label>` tanpa styling apa pun, hanya bagian header (Hero)
yang tampil sesuai desain aplikasi. Task ini mengganti kelima form itu memakai `BaseEditorView`
(komponen form create/update baku yang sudah dipakai di seluruh aplikasi, mis. Master Data Bank
di modul Administrator), sehingga tampilannya konsisten: label dan input bergaya seragam,
pesan error per field, tombol Simpan/Batal baku, dan panel preview di sisi form.

**Tidak ada perubahan perilaku bisnis** — validasi, payload yang dikirim ke backend, dan alur
create/update persis sama seperti sebelumnya. Yang berubah murni cara form dirender.

## Base Component Decision Gate

`UI GATE: 5 halaman x rata-rata 8 elemen field = REUSE penuh (BaseEditorView/BaseEditorField/BaseFormControl); 0 EXTEND, 0 COMPOSE, 0 WRAP, 0 NEW`

| Kebutuhan UI | Kandidat base | Bukti | Status | Catatan |
| --- | --- | --- | --- | --- |
| Layout form create/update (header, grid field, tombol aksi, panel preview) | `BaseEditorView` | `src/components/features/base-features/base-editor-view.jsx` — dipakai `administrator-bank-form-view.jsx` dan modul lain | REUSE | Dipakai apa adanya lewat props `config`/`fields`/`form`/`fieldErrors`/`onChange`/`onSubmit`/`onCancel`/`formProps`/`previewProps`, sama seperti pemakai lain. |
| Field teks, angka, tanggal, select, textarea, checkbox | `BaseEditorField` (routing ke `BaseTextField`/`BaseSelectField`/`BaseDateField`/`BaseTextAreaField`/`BaseCheckboxField` di `base-form-control.jsx`) | `src/components/features/base-features/base-editor-field.jsx`, `base-form-control.jsx` — dibaca langsung untuk memastikan kontrak `field.{name,label,type,options,required,maxLength,min,max,disabled,description}` | REUSE | Field config murni deklaratif di file `*-editor-config.jsx` baru per entity — tidak ada perubahan pada komponen base itu sendiri. |

**Temuan penting selama pemeriksaan source (bukan asumsi)**: field bertipe `type: "number"`
pada `BaseTextField` memakai `normalizeNumberInputValue` yang **membulatkan ke integer**
(`Math.trunc`, lihat `base-form-control.jsx:105-124`) — cocok untuk field integer (menit, nominal
Rupiah tanpa sen), tapi SALAH untuk field yang butuh desimal. Dua field terdampak:

- **Tax Rule → `rate`** (mis. 11.5%) — sebelumnya `<input type="number" step="0.01">`, benar-benar desimal.
- **Discount Policy → `value`/`limit`** (persentase atau nominal, `step="0.01"` di form lama).

Untuk keduanya dipakai `type: "text"` + `inputMode: "decimal"` (bukan `type: "number"`) supaya
nilai desimal tidak terpotong diam-diam — regresi fungsional yang akan lolos lint/build karena
secara sintaks tetap valid, hanya nilai yang salah saat runtime. Field integer murni (Administration
Fee Policy `amount`, Room Charge Policy `minimumMinutes`/`periodMinutes`) TETAP `type: "number"`
karena form lama memang `step="1"` (integer-only, konsisten dengan validasi `Number.isInteger` yang
sudah ada di `use-room-charge-policy-editor.js`).

## Files changed

| Entity | Hook (ditambah `config`/`fields`) | Config field baru | View (raw form -> `BaseEditorView`) |
| --- | --- | --- | --- |
| Tax Rule | `use-tax-rule-editor.js` | `tax-rule-editor-config.jsx` | `tax-rule-form-view.jsx` |
| Discount Policy | `use-discount-policy-editor.js` | `discount-policy-editor-config.jsx` | `discount-policy-form-view.jsx` |
| Administration Fee Policy | `use-administration-fee-policy-editor.js` | `administration-fee-policy-editor-config.jsx` | `administration-fee-policy-form-view.jsx` |
| Room Charge Policy | `use-room-charge-policy-editor.js` | `room-charge-policy-editor-config.jsx` | `room-charge-policy-form-view.jsx` |
| Register | `use-register-editor.js` | `register-editor-config.jsx` | `register-form-view.jsx` |

Pola perubahan pada tiap hook IDENTIK: tambah import config, tambah `const fields = useMemo(() =>
get<Entity>EditorFields(mode), [mode])`, tambah `config`/`fields`/`actionLoading` (alias dari
`submitting || actionLoading`, dibutuhkan `BaseEditorView`) pada objek yang di-return. Logika
`validate`/`buildPayload`/`handleSubmit`/state Redux TIDAK disentuh sama sekali di kelima hook —
murni penambahan, bukan penulisan ulang.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `eslint` (full severity) | **PASS** | `npx eslint <12 file berubah/baru>` → 0 error, 4 warning. Keempat warning (`react-hooks/set-state-in-effect`) berasal dari `useEffect` yang SAMA PERSIS di keempat hook (Administration Fee, Discount, Register, Room Charge) dan TIDAK disentuh diff task ini — dikonfirmasi lewat `git diff` bahwa baris warning ada di luar hunk perubahan. Pre-existing, bukan regresi. |
| `npm run build` | **PASS** | Sempat gagal 2x dengan `EBUSY: resource busy or locked, rmdir .next/standalone` (dua proses `node.exe` menahan lock — kemungkinan dev/prod server pengguna sendiri berjalan paralel; TIDAK di-kill, ditunggu sampai proses itu berhenti sendiri). Setelah lock hilang (`tasklist` mengonfirmasi nol proses node tersisa), build ulang exit 0, `postbuild` selesai normal. |
| Route hasil build | **PASS** | Kelimanya (`tax-rule`, `discount-policy`, `administration-fee-policy`, `room-charge-policy`, `register`) — baik `/create` maupun `/[id]` (update) — terkonfirmasi ada di `.next/server/app/health-services/billing-management/master-data/`. |
| Grep anti-regresi (checklist C/G) | **PASS** | Tidak ada `<button>`/`.btn` mentah, tidak ada `<table>` baru, tidak ada utility typography Bootstrap pada kelima view yang ditulis ulang — seluruhnya delegasi penuh ke `BaseEditorView`. |
| Perilaku bisnis (validasi, payload, redirect) | **TIDAK BERUBAH** | Dikonfirmasi dengan membandingkan `validate`/`buildPayload`/`handleSubmit` sebelum dan sesudah diff — nol perubahan pada fungsi-fungsi itu di kelima hook. |
| Test otomatis | `AUTOMATED TEST: SKIPPED (opsional) — repo tidak memakai Jest (test-policy.md); task murni migrasi tampilan tanpa logika baru` | — |
| Verifikasi manual (klik-coba create/update dengan data nyata di kelima form) | **NOT DONE** | Tidak ada kredensial login yang tersedia untuk builder; sengaja tidak diminta lewat chat untuk alasan keamanan. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint dan build sudah lulus bersih, tapi
klik-coba langsung (terutama field `rate`/`value`/`limit` yang sengaja diubah tipenya untuk
menghindari pembulatan integer) belum diverifikasi di browser nyata.

## Risiko yang tersisa

1. Field `type: "text"` + `inputMode: "decimal"` untuk `rate`/`value`/`limit` TIDAK memiliki
   pemblokiran karakter non-numerik di level browser seperti `type="number"` bawaan (`pattern` HTML
   hanya divalidasi saat submit form native, bukan mencegah ketikan) — pengguna masih bisa mengetik
   huruf, tapi `validate()` di hook tetap menolaknya saat submit (`Number.isNaN` check sudah ada
   sebelumnya, tidak berubah). Tidak ada regresi validasi, hanya UX pengetikan yang sedikit berbeda
   dari sebelumnya (browser tidak lagi menolak huruf saat diketik, feedback baru muncul saat submit).
2. Sama seperti `FE-BKC-011`: belum ada verifikasi manual ter-autentikasi sama sekali untuk kelima
   halaman ini.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya akses Master Data Billing, buka kelima halaman "Tambah" dan
   "Perbarui", isi termasuk nilai desimal pada Tax Rule (`rate`, mis. `11.5`) dan Discount Policy
   (`value`/`limit`), submit, lalu konfirmasi payload yang terkirim ke backend membawa angka desimal
   yang benar (bukan terpotong ke integer).
2. Konfirmasi tombol Batal, pesan error per field, dan redirect setelah submit berhasil masih
   berjalan sama seperti sebelum perubahan.
