# Laporan Perubahan Frontend — `FE-ACC-P2-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-007` |
| Judul | Penanda control account pada layar COA |
| Slice | `P2-CTRL` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-007` |
| Trace | `ACC-DEC-064`, `ACC-DEC-072`, `ACC-DEC-073`; `FR-P2-035`, `FR-P2-036` |
| Contract version | `ACC-API-0.8` grup Chart of Account — `approved` — untuk acceptance (1) dan (2). Acceptance (3) memakai `IsControlAccount` pada `/options`, yang baru **diusulkan `ACC-API-0.11`** dan baru ada di source `BE-ACC-P2-012` 🟡 |
| Wewenang UI | Satu kotak centang pada Form Akun, satu kolom penanda pada tabel COA, satu baris pada rincian akun, dan keterangan pada Form Jurnal. **Bukan layar baru** |
| Tambahan disetujui | Perbaikan cacat `{ id, data }` pada `use-journal-editor.jsx` (Form Jurnal MVP, `FE-ACC-006`) — disetujui Rizki 11 September 2026 |
| Dependency | `BE-ACC-P2-011` ✅ untuk (1) dan (2); `BE-ACC-P2-012` 🟡 untuk (3) — source ada, belum dibangun maupun diuji |
| Klasifikasi | `MEDIUM` — tiga layar yang sudah ada disentuh, satu hook bersama, nol komponen base baru |
| Task mode | `CROSS-REPO` — source di frontend `RizkiV2`, laporan di backend `rizkiG` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, laporan ini, serta tanda status dan tautan bukti pada roadmap dan traceability |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `460f717a0` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `968841e` (branch `rizkiG`) ditambah perubahan `BE-ACC-P2-012` yang belum di-commit |
| Tanggal | 11 September 2026 |
| Status | **✅ `SELESAI` — 11 September 2026, lanjutan.** `IMPLEMENTATION COMPLETE` · developer verification `PASS` · `READY FOR UAT` (UAT belum dijalankan). Lihat bagian 9. *Riwayat: 🟡 `SEBAGIAN` pada awal hari yang sama — ketiga acceptance terpetakan ke source; (3) baru bekerja setelah `BE-ACC-P2-012` berjalan; build dan uji peramban `NOT RUN`.* |

---

## 1. Keadaan yang ditemukan di awal

| Yang diperiksa | Hasil |
| --- | --- |
| Layar COA | Belum menampilkan maupun mengisi `isControlAccount`, walau `BE-ACC-P2-011` ✅ sudah mengirimnya pada daftar dan rincian sejak 9 September 2026 |
| Form Jurnal | Pemilih akun menampilkan seluruh akun dari `/options`, termasuk Kas Kasir. Petugas baru tahu akunnya terlarang sesudah ditolak `422` |
| `/options` | Source backend `BE-ACC-P2-012` (hari ini) menambahkan `IsControlAccount` dan sengaja **tidak** menyaring akun control |
| Komponen dasar | `BaseEditorField` **sudah** menghormati `field.disabled` (baris 105); `normalizeOptionList` **sudah** meneruskan `option.disabled` ke `FilterSelect`. Keduanya dipakai apa adanya |

### Dua cacat yang ditemukan di layar yang sama

**1. `{ id, data }` pada Form Jurnal** — sudah diketahui sejak 11 September pagi, diperbaiki atas
persetujuan Rizki. `use-journal-editor.jsx` memanggil `updateJournal({ id, data: payload })`,
sedangkan `master-data-resource-slice-factory.jsx` hanya membaca `{ id, payload }`. Akibatnya
**menyimpan ubahan draft jurnal mengirim `PUT` berbadan kosong**: ubahan tidak pernah sampai ke
backend. Satu-satunya hook di repository yang memakai `data:` untuk factory itu.

**2. Layar Ubah Akun tidak pernah dapat disimpan** — baru ditemukan. `validateForm` mewajibkan
Jenis Akun dan Saldo Normal **tanpa melihat mode**, padahal form ubah tidak memuat kedua isian itu
(`UpdateChartOfAccountRequest` memang tidak menerimanya). Menurut jalur kodenya, setiap tekan
Simpan pada form ubah berhenti di toast "Isian Belum Lengkap", dengan galat yang menempel pada
isian yang tidak dirender. Cacat ini **menghalangi acceptance (1)** — menandai akun lama sebagai
control account justru lewat form ubah — sehingga diperbaiki di task ini. Belum dibuktikan di
peramban; buktinya uji unit dan pembacaan jalur `handleSubmit`.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Menandai akun

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pemilik proses akuntansi | Membuka COA, klik dua kali Kas Kasir, lalu Perbarui |
| 2 | Layar | Kotak centang **Control Account** tampil tercentang atau tidak sesuai data tersimpan |
| 3 | Pemilik proses akuntansi | Mencentang, lalu Perbarui Akun |
| 4 | Layar | Tabel COA menampilkan badge **Control** pada baris Kas Kasir; akun lain bertanda `-` |

**Pengguna tanpa hak `ChartOfAccount : Update`** melihat kotak centang itu **mati**, dengan
keterangan "Anda tidak memiliki hak mengubah akun, sehingga penanda control account tidak dapat
diubah dari akun ini." Aturannya sama di form tambah: pengguna yang hanya berhak menambah akun
dapat membuat akun biasa, tetapi tidak dapat membuatnya sebagai control account.

### 2.2 Menyusun jurnal manual

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Staff akuntansi | Membuka Tambah Jurnal |
| 2 | Layar | Kartu Baris Jurnal menampilkan keterangan: akun control account tetap tampil tetapi dimatikan, dan pencatatannya hanya lewat kejadian akuntansi |
| 3 | Staff akuntansi | Membuka pemilih akun. "1-1002 — Kas Kasir (control account)" tampil **tidak dapat diklik**; akun lain seperti biasa |

**Draft lama yang terlanjur memakai akun control** — misalnya disimpan sebelum akunnya ditandai —
tetap menampilkan nama akunnya, bukan kotak kosong. Keterangannya berubah menjadi peringatan:
"Baris 2 masih menunjuk akun control account dan akan ditolak saat disimpan. Ganti akunnya lebih
dahulu." Petugas tahu sebelum menekan Simpan, bukan sesudah ditolak.

**Kenapa dimatikan, bukan disembunyikan.** Kartu task mengizinkan keduanya. Disembunyikan membuat
draft lama menampilkan kotak akun kosong, dan petugas tidak pernah tahu bahwa Kas Kasir ada tetapi
terlarang — ia akan mengira akunnya belum dibuat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; suite skill `rules/frontend/frontend-architecture.md`,
`base-component-decision-gate.md`, `ui-consistency-checklist.md`, `test-policy.md`,
`REPORT_TEMPLATE.md`; `base-editor-view.jsx`, `base-editor-form.jsx`, `base-editor-field.jsx`,
`base-form-control.jsx`, `base-ui-utils.jsx`, `filter-select.jsx`, `status-badge.jsx`,
`use-permission.jsx`, `master-data-resource-slice-factory.jsx`, `accounting-journal-slice.jsx`;
layar dan hook COA serta Form Jurnal; tiga uji unit Accounting yang membaca source kedua hook.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/corporate/accounting/chart-of-account/chart-of-account-utils.jsx` | `getIsControlAccount` baru; `buildPayload` selalu mengirim `isControlAccount` tegas; **`validateForm` hanya menuntut jenis akun dan saldo normal pada mode tambah** |
| `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | Kotak centang di form tambah dan ubah, kolom `Control` di tabel, baris di rincian, `CONTROL_ACCOUNT_BADGE`, `CONTROL_ACCOUNT_LOCKED_DESCRIPTION` |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-editor.jsx` | `usePermission("ChartOfAccount", "Update")`; field dimatikan dan keterangannya diganti bila tidak berhak; form ubah memuat nilai tersimpan |
| `src/components/view/corporate/accounting/chart-of-account/chart-of-account-view.jsx` | Format kolom `controlAccount` → `StatusBadge` |
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | `CONTROL_ACCOUNT_OPTION_SUFFIX`, `CONTROL_ACCOUNT_NOTICE`, `describeControlAccountNotice` |
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | Pilihan akun control **dimatikan**, tidak disaring; `controlAccountNotice`; **perbaikan `{ id, data }` → `{ id, payload }`** |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | `InformationAlert` di kartu Baris Jurnal |
| `tests/unit/accounting-control-account.test.mjs` (baru) | 11 uji |

`JournalLineRow` **tidak diubah**. Ia dipakai tiga layar, dan penandaannya dikerjakan di hook
editor jurnal saja.

### 3.3 Kepatuhan arsitektur frontend

Alurnya tetap `view → hook → slice → InstanceAxios`. Nol thunk, nol endpoint, nol slice baru.
Konfigurasi dan salinan teks di `constants`, fungsi murni di `utils`, orkestrasi di hook, dan view
hanya merender. Hak akses memakai `usePermission` yang sudah dipakai Pengaturan Akuntansi, Tutup
Tahun, dan Jurnal Berulang — tidak ada hardcode peran.

### 3.4 Gerbang keputusan base component

`UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kotak centang Control Account | `BaseEditorView` → `BaseEditorField` → `BaseCheckboxField` | `CHECKBOX_TYPES` di `base-editor-field.jsx`; preseden `isPostable` di form yang sama | REUSE | Field `type: "checkbox"` pada config |
| Kotak centang mati bagi yang tidak berhak | `BaseEditorField` `field.disabled` | `base-editor-field.jsx` baris 105 | REUSE | Hook menyetel `disabled` dan mengganti `description` |
| Kolom penanda di tabel | `DataTable` + `StatusBadge` | pola `format` di `chart-of-account-view.jsx`; nada `info` | REUSE | Kolom config `format: "controlAccount"` |
| Baris di rincian akun | `BaseDetailView` lewat `detailFields` | `format: "boolean"` di `use-chart-of-account-detail.jsx` | REUSE | Entri config saja |
| Pilihan akun mati di Form Jurnal | `BaseSelectField` → `FilterSelect` | `normalizeOptionList` meneruskan `disabled`; `filter-select.jsx` baris 93, 379, 539 | REUSE | `disabled: true` pada opsi |
| Keterangan kenapa | `InformationAlert` | sudah dipakai layar ini untuk saldo awal | REUSE | Di dalam kartu Baris Jurnal |

`renderField` pada `BaseEditorView` sempat dipertimbangkan untuk mematikan kotak centang, lalu
dibatalkan sesudah `field.disabled` terbukti sudah didukung.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah — pola masing-masing layar |
| Kosong | Tabel COA: akun tanpa penanda bertanda `-`. Form Jurnal: keterangan **tidak muncul** bila tidak ada satu pun akun control pada pilihan |
| Gagal | Pesan backend ditampilkan apa adanya, termasuk `422` "Akun … hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual." |
| Tanpa hak akses | Kotak centang mati beserta alasannya; seluruh layar tetap dijaga `AccessDeniedGate` seperti sebelumnya |
| Backend belum membawa `isControlAccount` di `/options` | Semua akun dianggap biasa; tidak ada yang dimatikan dan keterangan tidak muncul. Backend tetap penegak terakhir |

---

## 5. Endpoint yang dikonsumsi

Nol endpoint baru. Yang berubah hanya bidang yang dibaca atau dikirim.

#### Corporate / Accounting / Master Data / Chart of Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts` | Kolom `Control` — membaca `isControlAccount` | `ChartOfAccount : Read` |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts/{id}` | Baris rincian dan nilai awal form ubah | `ChartOfAccount : Read` |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts/options` | Menandai pilihan akun di Form Jurnal — **usulan `ACC-API-0.11`** | `ChartOfAccount : Read` |
| `POST` | `/v1/corporate/accounting/master-data/chart-of-accounts` | Mengirim `isControlAccount` | `ChartOfAccount : Create` |
| `PUT` | `/v1/corporate/accounting/master-data/chart-of-accounts/{id}` | Mengirim `isControlAccount` tegas | `ChartOfAccount : Update` |

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/corporate/accounting/journals/{id}` | Simpan ubahan draft — **kini benar-benar membawa badan permintaan** | `Journal : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 8 berkas yang diubah | `0 errors`, 4 warning | `PASS` | Keempatnya `react-hooks/set-state-in-effect` pada efek yang sudah ada di `HEAD` (`setRouteError`, dua `setForm`, `setJournalId`); tidak ada `setState` baru di dalam efek. Pembanding otomatis lint `HEAD` lewat `--stdin` gagal dijalankan (exit 2), jadi klaim "nol warning baru" bersandar pada pembacaan kode |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **651 lulus, 0 gagal** (sebelumnya 640) | `PASS` | Keluaran perintah |
| Grep anti-regresi pada baris yang ditambahkan | Nol `<button`, nol `<table`, nol warna literal, nol `!important`, nol `style=` | `PASS` | Satu temuan `fs-4` berada di `menu-items.jsx` milik pekerjaan `FE-ACC-P2-003`/`004` yang belum di-commit, bukan baris task ini |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi owner: "tanpa build" |
| Uji peramban | Tidak dijalankan | `NOT RUN` | Lihat di bawah |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`

`MANUAL TEST: NOT FEASIBLE` — sesi ini tidak menjalankan peramban, dan acceptance (3) menuntut
backend `BE-ACC-P2-012` yang belum dibangun. Skenario untuk Rizki:

| No | Langkah | Yang diharapkan |
| ---: | --- | --- |
| 1 | COA → rincian Kas Kasir → Perbarui | Kotak centang Control Account tercentang; **Perbarui Akun berhasil** — sebelumnya tertahan "Isian Belum Lengkap" |
| 2 | Hapus centang, simpan, lalu centang lagi dan simpan | Badge `Control` pada tabel hilang lalu muncul kembali |
| 3 | Masuk sebagai pengguna tanpa `ChartOfAccount : Update` | Kotak centang mati beserta keterangannya |
| 4 | Tambah Jurnal → pilih akun | Akun bertanda tampil dengan akhiran "(control account)" dan tidak dapat diklik; keterangan biru di kartu Baris Jurnal |
| 5 | Ubah draft jurnal yang sudah ada, ganti nominal, Simpan Draft, muat ulang | **Nominal baru tersimpan** — sebelumnya `PUT` berbadan kosong |

**Tidak dijalankan:** `npm run build` (instruksi owner), uji peramban (lihat di atas), `test:e2e`
(repository tidak punya `playwright.config`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kotak centang hanya menyala bagi yang berhak mengubah akun | Terpetakan ke source, belum diuji di peramban | `use-chart-of-account-editor.jsx` — `usePermission("ChartOfAccount", "Update")`, `field.disabled`; uji `kotak centang dimatikan bagi yang tidak berhak mengubah akun` |
| (2) Tabel COA menampilkan penandanya sehingga terbaca sekilas | Terpetakan ke source, belum diuji di peramban | Kolom `Control` + `StatusBadge`; data dari `ChartOfAccountListResponse` (`BE-ACC-P2-011` ✅) |
| (3) Form Jurnal menyembunyikan atau mematikan akun control dari pemilihnya, disertai keterangan kenapa | Terpetakan ke source, **baru bekerja setelah `BE-ACC-P2-012` berjalan** | `use-journal-editor.jsx` — `disabled: isControlAccount`, `controlAccountNotice`; `journal-form-view.jsx` — `InformationAlert` |
| DoD — lint hijau | Terpenuhi | `0 errors` |
| DoD — build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi owner |
| DoD — laporan task tertulis | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak `/options` yang dikonsumsi acceptance (3) masih **usulan** `ACC-API-0.11`. Bila ratifikasinya mengubah nama bidang, hanya `getIsControlAccount` yang perlu disesuaikan |
| Masalah yang diketahui | **Dialog penyesuaian jurnal dan Form Jurnal Berulang belum mematikan akun control.** Keduanya kini ditolak backend (`ACC-DEC-072`, `ACC-DEC-073`), tetapi petugas baru tahu sesudah menyimpan. Di luar acceptance kartu ini, yang hanya menyebut Form Jurnal. Disarankan task lanjutan dengan pola hook yang sama |
| Temuan backend | `POST` akun menerima `IsControlAccount` dari pengguna yang hanya berhak `Create`; backend tidak memeriksanya terpisah. Layar mendahului dengan menuntut `Update`, tetapi penegak terakhirnya belum ada. Dicatat, tidak diubah |
| Dependency backend | `BE-ACC-P2-012` 🟡 — sampai dibangun dan berjalan, `/options` tidak membawa `isControlAccount` dan Form Jurnal berperilaku seperti sebelumnya |
| Perubahan sampingan | Dua perbaikan cacat MVP di atas: `{ id, data }` (disetujui) dan `validateForm` mode ubah (menghalangi acceptance (1)) |
| Interupsi | Skrip suntingan pertama gagal di-parse Bash sebelum menulis apa pun; dijalankan ulang dari berkas skrip. Nol suntingan ganda |
| Nama berkas | Mengikuti pola laporan Phase 2 lain (`fe-acc-p2-0xx-<judul>.md`), bukan `FE-ACC-P2-007.md` persis seperti aturan suite, supaya satu folder tidak memakai dua pola |
| Status Git | Frontend: 7 source `M` milik task ini dan 1 uji `??`; `store.jsx`, `menu-items.jsx`, serta seluruh berkas `recurring-journal` berasal dari `FE-ACC-P2-003`/`004` yang belum di-commit dan tidak disentuh. Tidak ada stage maupun commit |
| Langkah berikutnya | (1) Rizki membangun backend dan frontend, lalu menjalankan skenario bagian 6. (2) Ratifikasi `ACC-API-0.11`. (3) `FE-ACC-P2-008` sisi buku besar |

---

## 9. Penyelesaian — 11 September 2026, lanjutan

Atas instruksi owner, `FE-ACC-P2-007` dituntaskan dalam sesi yang sama dengan `FE-ACC-P2-008`.
Status dipisah menurut alur kerja owner: development yang selesai tidak menunggu UAT.

```text
IMPLEMENTATION STATUS          : IMPLEMENTATION COMPLETE
DEVELOPER VERIFICATION STATUS  : PASS
UAT STATUS                     : READY FOR UAT — belum diuji tim UAT
```

### 9.1 Yang berubah sejak laporan pagi

| Hal | Sebelum | Sesudah |
| --- | --- | --- |
| Backend `BE-ACC-P2-012` | Source ada, belum berjalan | Berjalan di `rizkiG` `cca0957`; `/options` terbukti mengirim `isControlAccount = true` untuk keenam control account (uji kontrak read-only 11 Sep 2026) |
| Dialog penyesuaian jurnal | Akun control dapat dipilih dan baru ditolak `422` sesudah dikirim | Dimatikan pada pemilih akun baris selisih, disertai keterangan; tombol kirim ditahan bila ada baris ber-akun control (`ACC-DEC-072`) |
| Form Jurnal Berulang | Akun control baru ditolak saat disimpan | Dimatikan, disertai keterangan; template lama yang barisnya menunjuk akun control disebut nomor barisnya dan tombol Simpan ditahan (`ACC-DEC-073`) |
| Aturan pilihan akun | Ditulis di dalam `use-journal-editor.jsx` saja | Satu util bersama `journal-account-option-utils.jsx`, dipakai **ketiga** layar yang memakai `JournalLineRow` |
| `npm run build` | `NOT RUN` | `✓ Compiled successfully` 45 dtk |

**Pembalikan penuh tidak disentuh.** Ia tidak memakai pemilih akun — barisnya diturunkan backend
dari jurnal asal — dan `ACC-DEC-072` membolehkannya.

### 9.2 Berkas yang berubah pada lanjutan ini

| Berkas | Perubahan |
| --- | --- |
| `src/utils/corporate/accounting/journal/journal-account-option-utils.jsx` (baru) | `buildJournalAccountOption`, `findControlAccountLineNumbers`, `hasControlAccountOption` |
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | `ADJUSTMENT_CONTROL_ACCOUNT_NOTICE`, `RECURRING_CONTROL_ACCOUNT_NOTICE`; `describeControlAccountNotice` menerima salinan teks per layar, bawaannya tetap Form Jurnal |
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | Memakai util bersama; perilaku tidak berubah |
| `src/lib/hooks/corporate/accounting/journal/use-journal-reversal.jsx` | Pilihan akun lewat util; `controlAccountNotice`; `canSubmit` dan `blockingMessage` menahan baris ber-akun control. `toCents` dan hitungan sen **tidak diubah** — dikunci `accounting-journal-reversal.test.mjs` |
| `src/components/view/corporate/accounting/journal/detail/journal-reversal-dialog.jsx` | `InformationAlert` di bagian baris selisih |
| `src/lib/hooks/corporate/accounting/recurring-journal/use-recurring-journal-editor.jsx` | Pilihan akun lewat util; `controlAccountNotice`; `saveBlockedReason` menyebut baris ber-akun control |
| `src/components/view/corporate/accounting/recurring-journal/form/recurring-journal-form-view.jsx` | `InformationAlert` di kartu Baris Template |
| `tests/unit/accounting-control-account.test.mjs` | 11 → 15 uji |

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` — opsi `disabled` pada
`FilterSelect` lewat `JournalLineRow`, `InformationAlert` di dialog, `InformationAlert` di form
template. `JournalLineRow` tidak diubah.

### 9.3 Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada 18 berkas yang disentuh `FE-ACC-P2-007` dan `008` | `0 errors`; 2 warning `react-hooks/set-state-in-effect` — `setJournalId` (`use-journal-editor.jsx`) dan `setCorrectionType` (`use-journal-reversal.jsx`), keduanya efek yang sudah ada sebelum task ini | `PASS` / `EXISTING WARNING` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **677 lulus, 0 gagal** (651 → 677) | `PASS` |
| `npm run build` | `✓ Compiled successfully in 45s`, exit 0 | `PASS` |
| Uji kontrak `/options`, read-only | `200`, 21 akun, keenam control account `isControlAccount = true` | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`

`MANUAL TEST: NOT FEASIBLE` — sesi ini tidak menjalankan peramban; uji fungsional dan UAT
diserahkan owner kepada tim UAT terpisah.

### 9.4 Acceptance dan DoD, dinilai ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kotak centang hanya menyala bagi yang berhak mengubah akun | Terpenuhi di source | Bagian 7 |
| (2) Tabel COA menampilkan penandanya sehingga terbaca sekilas | Terpenuhi di source | Bagian 7 |
| (3) Form Jurnal mematikan akun control disertai keterangan kenapa | Terpenuhi — kini bersandar pada `/options` yang terbukti di runtime, dan diperluas ke dialog penyesuaian serta Form Jurnal Berulang | 9.1; uji `ketiga layar baris jurnal memakai aturan pilihan akun yang sama` |
| DoD — lint hijau | Terpenuhi | `0 errors` |
| DoD — build hijau | **Terpenuhi** | `✓ Compiled successfully` |
| DoD — laporan task tertulis | Terpenuhi | Berkas ini |
| UAT peramban | **Belum dijalankan** — diserahkan ke tim UAT | Keputusan owner 11 Sep 2026; bukan `UAT PASS` |

### 9.5 Yang perlu diuji tim UAT

Skenario 1–5 pada bagian 6, ditambah:

| No | Langkah | Yang diharapkan |
| ---: | --- | --- |
| 6 | Rincian jurnal disahkan → Koreksi → Jurnal Penyesuaian → buka pemilih akun baris selisih | Akun control berakhiran "(control account)" dan tidak dapat diklik; keterangan di atas tabel baris selisih menyarankan pembalikan penuh |
| 7 | Dialog yang sama → Pembalikan Penuh pada jurnal yang menyentuh Kas Kecil | Tidak ada keterangan control account; kiriman diterima (`ACC-DEC-072`) |
| 8 | Tambah Template Jurnal Berulang → pilih akun | Akun control dimatikan; keterangan menyebut penolakan saat disimpan maupun diaktifkan |
| 9 | Ubah template lama yang barisnya menunjuk akun yang kemudian ditandai control | Nomor barisnya disebut, tombol Simpan mati beserta alasannya |
| 10 | Pengguna tanpa `ChartOfAccount : Update` membuka Form Akun | Kotak centang Control Account mati beserta keterangannya |

### 9.6 Status Git lanjutan

Nol commit, stage, push, merge, atau rebase. Keluaran `git status --short` lengkap ada pada
laporan [`fe-acc-p2-008`](fe-acc-p2-008-layar-rekonsiliasi-control-account.md), karena kedua task
dikerjakan dalam satu working tree.
