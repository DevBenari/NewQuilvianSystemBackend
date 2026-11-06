# Laporan Perubahan Frontend — `FE-NV-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-008` |
| Judul | Daftar dan form Tablet Bed beserta butir menu |
| Slice | `MVP-6` — `EPIC NV-05` pengelolaan tablet bed (gelombang frontend 1, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-008` |
| Trace | `FR-NV-033`, `034`, `035`; `NV-DEC-035`; `AD-01`–`04`; `VAL-NV-17`–`25`; `UAT-12` sisi admin, `UAT-13`; layar `FE-NVW-05`, `FE-NVW-06` (`03-frontend-architecture.md` R2.1–R2.3, R2.9); flowchart `06` |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`), api §5 |
| Wewenang UI | Isi dan tombol dikunci skema `FE-NVW-05`/`06`; nama dan urutan butir menu, konfirmasi modal, tata letak form `DEV_DISCRETION` |
| Dependency | `BE-NV-009` ✅ (6 Nov 2026, uji HTTP 29/29) |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika sedang (1), kontrak yang ada (1), auth berkaitan — kata sandi tablet (1), workflow terbatas (1) |
| Task mode | `FRONTEND` — perintah Rizki 6 Nov 2026 ("mulai sekarang BE-NV-010; FE-NV-008 dan FE-NV-015") |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan ini dan baris status roadmap/traceability di backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `3ab8e3215` (branch `RizkiV2`, sejajar `origin/RizkiV2`); perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `13fa1c4d` (backend yang berjalan saat uji) |
| Tanggal | 2026-11-06 |
| Status | ✅ **SELESAI 6 November 2026** — AC 6/6 terpetakan ke source; `npx eslint` 0 error; uji layar 18/18 cek `PASS` (bagian `FE-NV-008` 14 cek) sesudah satu perbaikan yang ditemukan uji (§6). AC-5 (`VAL-NV-20`) bukti runtime-nya menunggu `BE-NV-011`, sesuai kartu. Verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

API tablet bed (`BE-NV-009`) sudah ada, tetapi belum ada layar. Admin IT hanya bisa mendaftarkan
tablet, membuat login, dan menonaktifkannya lewat Swagger.

## 2. Proses bisnis dari sisi pengguna

1. Admin IT membuka **Administrator › Master Data › Tablet Bed Nurse View**. Butir menu ini hanya
   tampil bagi pemegang `NurseViewBedTablet : Read`.
2. Daftar tablet tampil dengan kolom kode, nama, bed (nama bed · kamar), lantai, status, dan online.
   Admin bisa mencari (kode, nama, bed, letak) dan menyaring lantai serta status.
3. Admin menekan **+ Tambah Tablet Bed** (hanya tampil bagi pemegang `Create`). Di form, isian Bed
   terkunci sampai Lantai dipilih. Sesudah lantai dipilih, daftar bed lantai itu dimuat; bed yang sudah
   punya tablet tampil "… · sudah punya tablet" dan tidak bisa dipilih. Lantai tanpa bed aktif menampilkan
   "Lantai ini belum punya bed aktif."
4. Sesudah simpan, toast hijau tampil, lalu halaman berpindah ke rincian: status "Belum ada login",
   Panggilan Pasien "Tidak ada".
5. **Buat Login** → konfirmasi → modal "Catat Login Tablet" berisi nama pengguna dan kata sandi, dengan
   tombol Salin. Kata sandi hanya ada di modal itu.
6. **Atur Ulang Login** memberi kata sandi baru dan mengaktifkan kembali tablet nonaktif.
7. **Perbarui** mengubah nama, bed, letak, atau keterangan. Bed tablet itu sendiri tetap bisa dipilih.
8. **Nonaktifkan** meminta alasan 5–500 karakter; login tablet langsung dikunci.

**Jalur tidak normal yang ditangani layar:**

| Kejadian | Yang dilihat admin |
| --- | --- |
| Bed sudah dipakai tablet lain ketika dipilih | Opsi bed terkunci dengan label "· sudah punya tablet" |
| Bed direbut tablet lain di antara memilih dan menyimpan | Toast merah dan pesan di bawah isian Bed "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu."; pilihan bed dikosongkan; daftar bed dimuat ulang dan bed itu kini terkunci |
| `409` lain (versi berubah `VAL-NV-21`, panggilan aktif `VAL-NV-20`) | Toast merah dengan pesan backend; rincian dan daftar bed dimuat ulang |
| Alasan nonaktif < 5 karakter | Toast "Alasan Belum Valid"; tidak ada request |
| Tanpa hak akses | Layar akses ditolak (`AccessDeniedGate`); butir menu tersembunyi |
| Daftar gagal dimuat | Alert "Data tablet gagal dimuat." beserta pesan backend dan tombol **Coba lagi** |
| Pilihan lantai gagal dimuat | Saringan lantai disembunyikan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; suite skill `frontend-architecture.md`,
`base-component-decision-gate.md`, `ui-consistency-checklist.md`, `master-data-feature-standard.md`;
modul rujukan visual **Display Nurse View** (`FE-NV-001`/`002`: constants, slice, utils, tiga hook, tiga
view, lima route); `base-editor-form.jsx` (`dependsOn`, `remountKey`, `optionLoadingMap`,
`optionHandlerMap`), `base-editor-field.jsx` (registry select remote, `field.remote`),
`base-form-control.jsx`, `filter-select.jsx`, `information-alert.jsx`, `use-permission.jsx`,
`select-resource-registry.js`, `left-sidebar-items-virtualized.jsx` (`requiredPermission`),
`store.jsx`, `menu-items.jsx`; backend `NvwBedTabletController`/`Service`/`Dtos` (kontrak as-is).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/administrator/master-data/nurse-view-bed-tablet/nurse-view-bed-tablet-constants.jsx` | **Baru.** `NURSE_VIEW_BED_TABLET_CONFIG`: endpoint, route, token scope, resource `NurseViewBedTablet`, salinan teks, kolom, field form (Lantai `submit: false`; Bed `dependsOn: "floorCode"`; keduanya `remote: false`), baris detail, pesan |
| `src/lib/state/slice/administrator/master-data/master-data-nurse-view-bed-tablet-slice.jsx` | **Baru.** Sembilan thunk untuk sembilan endpoint api §5, termasuk `bed-options`. Tidak ada state yang menyimpan respons login; galat membawa `statusCode` dan `code` (`errors.code` backend) |
| `src/utils/administrator/master-data/nurse-view-bed-tablet/nurse-view-bed-tablet-utils.jsx` | **Baru.** Status/badge, online, label bed, opsi lantai, opsi bed (terkunci bila `hasTabletInUse`, kecuali bed tablet itu sendiri), form, validasi, payload, baris detail. Helper umum diimpor dari utils perangkat TV, mengikuti preseden `FE-NV-004` |
| `src/lib/hooks/administrator/master-data/nurse-view-bed-tablet/use-master-data-nurse-view-bed-tablet.jsx` | **Baru.** Controller daftar; `usePermission(NurseViewBedTablet, Create)` untuk tombol Tambah |
| `…/use-master-data-nurse-view-bed-tablet-editor.jsx` | **Baru.** Create/update; lantai → `bed-options`; bed dikosongkan saat lantai berganti; `remountKey` lantai + versi muat ulang; redirect tertunda 1,5 detik (pola `FE-NV-002`); `409` → muat ulang daftar bed dan rincian; `VAL-NV-19` → isian Bed dikosongkan dan diberi pesan |
| `…/use-master-data-nurse-view-bed-tablet-detail.jsx` | **Baru.** Rincian; aksi = `AvailableActions` **dan** `usePermission(NurseViewBedTablet, Update)`; buat/atur ulang login; kredensial hanya di state lokal; nonaktif beralasan; `409` → muat ulang |
| `src/components/view/administrator/master-data/nurse-view-bed-tablet/master-data-nurse-view-bed-tablet-view.jsx` | **Baru.** Halaman daftar |
| `…/add/nurse-view-bed-tablet-form-view.jsx` | **Baru.** `BaseEditorView` dengan `optionLoadingMap` dan `optionHandlerMap` |
| `…/detail/nurse-view-bed-tablet-detail-view.jsx` | **Baru.** `BaseDetailView` + tiga `ConfirmModal`; deskripsi "Status: … · Ada panggilan aktif" bila ada |
| `src/app/administrator/master-data/nurse-view-bed-tablets/page.jsx`, `nurse-view-bed-tablet-client.jsx`, `create/page.jsx`, `[slug]/page.jsx`, `[slug]/update/page.jsx` | **Baru.** Route tipis; token kosong/cadangan/> 180 karakter → `notFound()` |
| `src/lib/state/store.jsx` | +2 baris: import dan `masterDataNurseViewBedTablet` (pola registrasi setiap modul, sama dengan `FE-NV-001`) |
| `src/utils/menu-sidebar/menu-items.jsx` | +6 baris: butir "Tablet Bed Nurse View" sesudah "Interval TTV Nurse View", `requiredPermission: { resource: "NurseViewBedTablet", action: "Read" }` (berkas bersama, restu `NV-DEC-056`) |

**Diff ringkas berkas bersama `menu-items.jsx`** (gabungan dengan `FE-NV-015`, +12/−0):

```diff
       {
         label: "Interval TTV Nurse View",
         key: "administratorNurseViewVitalSignInterval",
         pathname: "/administrator/master-data/nurse-view-vital-sign-intervals",
       },
+      {
+        label: "Tablet Bed Nurse View",
+        key: "administratorNurseViewBedTablet",
+        pathname: "/administrator/master-data/nurse-view-bed-tablets",
+        requiredPermission: { resource: "NurseViewBedTablet", action: "Read" },
+      },
+      { …butir "Ambang Panggilan Nurse View" milik FE-NV-015… },
```

Nol komentar pada berkas baru; nol CSS baru; seluruh berkas CRLF.

### 3.3 Kepatuhan arsitektur dan privasi

| Aturan | Keadaan |
| --- | --- |
| Bentuk berkas master data (rujukan sebaudara `nurse-view-device`) | Diikuti: constants, slice, utils, tiga hook, tiga view, lima route, registrasi store dan menu |
| Sembilan thunk master data | **Delta disengaja**, sama dengan `FE-NV-001`: kontrak api §5 memakai aksi bernama dan tidak punya `summary`, `options`, `PATCH status`, `DELETE`. Slice memetakan sembilan endpoint yang ada |
| Definisi kolom di berkas `-table-columns.jsx` terpisah (checklist B) | **Tidak**, mengikuti modul sebaudara `nurse-view-device` yang menaruh kolom di config + view |
| Kata sandi tidak masuk Redux, storage, maupun log | Hanya di state lokal hook detail selama modal terbuka; slice tidak menyimpan respons login; nol `console`; uji L5–L6: tidak ada di `localStorage`/`sessionStorage`; tangkapan layar modal disamarkan |
| Hak akses di UI | Tombol Tambah (`Create`) dan tombol aksi rincian (`Update` + `AvailableActions`) mengikuti `usePermission`; butir menu `requiredPermission`. Backend tetap penentu akhir |
| Kirim ganda | Tombol dan modal terkunci selama `actionLoading`; form terkunci selama jeda redirect |

**Tabel keputusan base component (UI GATE):**

| Elemen | Base component | Status |
| --- | --- | --- |
| Kepala halaman daftar | `Hero` | `REUSE` |
| Saringan + pencarian | `DataFilter` + `FilterSelect` ×3 | `REUSE` |
| Tabel + paginasi | `DataTable` + `RegionPagination` | `REUSE` |
| Status dan online | `StatusBadge` | `REUSE` |
| Alert gagal + Coba lagi | `InformationAlert` (`children`) + `BaseButton` | `REUSE` |
| Akses ditolak | `AccessDeniedGate` | `REUSE` |
| Form | `BaseEditorView` | `REUSE` |
| Pemilih lantai → bed, bed terkunci, teks bed kosong | `BaseSelectField` lewat props yang sudah ada: `dependsOn`, `remountKey`, `option.disabled`, `optionLoadingMap`, `optionHandlerMap.emptyText`, `field.remote: false`, `field.description` | `REUSE` |
| Halaman rincian | `BaseDetailView` | `REUSE` |
| Konfirmasi login, modal kredensial, nonaktif beralasan | `ConfirmModal` (`warning`, `success` + `hideCancel`, `danger` + `requireReason`) | `REUSE` |
| Toast | `ToastStack` milik view dasar | `REUSE` |

`UI GATE: PASS — seluruh elemen REUSE, tidak ada keputusan user yang dibutuhkan.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat daftar | "Mengambil data tablet bed..." |
| Kosong | "Belum ada tablet bed." |
| Gagal | Alert "Data tablet gagal dimuat." + pesan backend + Coba lagi |
| Tanpa hak akses | `AccessDeniedGate` dengan pesan 403 backend |
| Memuat rincian / tidak ditemukan / tautan rusak | "Mengambil detail tablet bed..." / "Data tablet bed tidak ditemukan." / "Tautan detail tablet tidak valid…" |
| Form belum lengkap | Pesan per isian + toast "Form Belum Lengkap" |
| Simpan berhasil | Toast hijau, tombol terkunci, ±1,5 detik kemudian ke rincian |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Bed Tablet

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar, saringan, paginasi | `NurseViewBedTablet : Read` |
| `GET` | `/floors` | Saringan lantai dan isian Lantai | `NurseViewBedTablet : Read` |
| `GET` | `/bed-options?floorCode=` | Isian Bed beserta tanda sudah bertablet | `NurseViewBedTablet : Read` |
| `GET` | `/{id}` | Rincian dan isi form perbarui | `NurseViewBedTablet : Read` |
| `POST` | `/` | Simpan tablet baru | `NurseViewBedTablet : Create` |
| `PUT` | `/{id}` | Perbarui / pindah bed | `NurseViewBedTablet : Update` |
| `POST` | `/{id}/login` | Buat Login | `NurseViewBedTablet : Update` |
| `POST` | `/{id}/login/reset` | Atur Ulang Login | `NurseViewBedTablet : Update` |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan | `NurseViewBedTablet : Update` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 14 berkas baru + `store.jsx` + `menu-items.jsx` (gabungan dengan `FE-NV-015`: 22 berkas) | 0 error, 2 warning `react-hooks/set-state-in-effect`; untuk task ini 1 warning di hook rincian, pola resolusi token yang sama dengan hook `nurse-view-device` (hook rujukan menghasilkan 2 warning identik) | `PASS` (0 error); warning `EXISTING WARNING` | Keluaran perintah 6 Nov |
| Grep anti-regresi (tombol mentah, `<table>`, utilitas tipografi, style inline, warna literal, storage/console, komentar) | Nol temuan | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Port 3000 dipakai `next dev` milik Rizki; build menulis `.next` yang sama dan akan merusak dev server (pelajaran 3 Okt). Kartu meminta build hanya bila diminta |
| Uji layar (Playwright Edge headless, `next dev` 3000 + backend `13fa1c4d`) | 18/18 cek `PASS` sesudah perbaikan | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-008-015-20261106/run-2026-11-06T07-52-06-902Z/` (`hasil.json`, `jaringan.json`, 13 PNG) |

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan owner tanpa unit test; rules/frontend/test-policy.md menjadikan penulisan test opsional`.

`MANUAL TEST: PASS` — lewat uji layar otomatis di atas (bukan UAT).

**Pelaksanaan uji.** Aktor berhak `QUILVIAN_PERAWAT_RUANGAN_*` dengan izin sementara
`NurseViewBedTablet` R/C/U + `NurseViewPatientCallWaitThreshold` R/U (salin → tambah → pulihkan lewat
API Akses Role; policy asli 20 butir dipulihkan identik); konteks kedua dengan izin baca ambang saja;
`QUILVIAN_LOKET_*` sebagai akun tanpa izin. Login lewat UI dengan pola aman-hydration. Kata sandi tablet
hanya di memori skrip dan disamarkan di tangkapan layar; pemindaian bukti: nol token, nol pola sandi.

| No | Langkah | Hasil sebenarnya | Status |
| ---: | --- | --- | --- |
| L1 | Cari "Nurse View" di sidebar (aktor berhak) | Butir "Tablet Bed Nurse View" dan "Ambang Panggilan Nurse View" tampil | `PASS` |
| L2 | Buka daftar | `GET` 200; baris tampil; tombol "+ Tambah Tablet Bed" ada | `PASS` |
| L3 | Saring status Nonaktif | `GET ?status=3` 200; hanya baris Nonaktif | `PASS` |
| L4 | Tambah: Bed terkunci sebelum Lantai; Lantai 3 → `bed-options?floorCode=3`; pilih 330-A; Simpan | Bed terkunci; `POST` 201 `NVT-000009`; payload `deviceName`, `bedId`, `locationNote`, `description` (tanpa `floorCode`); toast "…berhasil didaftarkan."; pindah ke rincian, Panggilan Pasien "Tidak ada" | `PASS` |
| L5 | Buat Login | `POST /login` 200; modal menampilkan `nvt-000009` + sandi; sandi tidak di storage; status Aktif; tombol Atur Ulang Login tampil | `PASS` |
| L6 | Atur Ulang Login | `POST /login/reset` 200; sandi baru berbeda; tidak di storage | `PASS` |
| L7 | Tambah lagi, Lantai 3 | Opsi "330-A · KELAS I 330 · sudah punya tablet" terkunci (`UAT-13`) | `PASS` |
| L8 | Pilih 330-B, lalu tablet lain direbutkan ke 330-B lewat API, lalu Simpan | `POST` 409; toast dan pesan di bawah Bed "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu."; isian Bed kembali "Pilih bed"; `bed-options` dimuat ulang; 330-B kini terkunci | `PASS` (sesudah perbaikan) |
| L9 | Perbarui tablet: bed sendiri tetap bisa dipilih; pindah ke 329-A | `PUT` 200 dengan `version` 2; rincian menampilkan 329-A | `PASS` |
| L10 | Nonaktifkan dengan alasan "abc", lalu alasan sah | Alasan pendek: nol `PATCH`; alasan sah: `PATCH` 200, toast "Login tablet langsung dikunci.", status Nonaktif, tombol Nonaktifkan hilang, Atur Ulang Login tampil | `PASS` |
| L16 | Aktor dengan izin baca ambang saja | Butir "Tablet Bed Nurse View" tersembunyi | `PASS` |
| L17 | Aktor yang sama membuka daftar tablet | `GET` 403; layar akses ditolak; tombol Tambah tidak ada | `PASS` |
| L18 | LOKET | Kedua butir menu tersembunyi | `PASS` |

L11–L15 milik `FE-NV-015` (laporannya terpisah).

**Temuan uji dan perbaikannya (6 Nov, run 1–3 → run 4):**

1. **Isian Bed memanggil resource master `beds` sendiri.** `optionResource: "beds"` dikenali registry
   select global (`resolveFieldResourceName`) sebagai master bed, sehingga isian Bed ikut memanggil
   `GET /api/v1/health-services/master-data/beds/options` setiap kali dibuka (403 untuk aktor uji). Bagi
   admin yang punya izin master bed, daftar seluruh bed tanpa tanda terkunci akan menimpa daftar lantai
   terpilih dan `AD-01` di layar tidak berlaku. **Perbaikan:** `remote: false` pada isian Lantai dan Bed
   (opsi resmi `BaseEditorField`). Sesudahnya tidak ada lagi panggilan `beds/options`.
2. **Bed yang direbut tetap tampak terpilih.** `FilterSelect` mendahulukan opsi terpilih yang tersimpan
   (label lama, aktif) di atas opsi hasil muat ulang, sehingga admin bisa mengirim bed yang sama lagi dan
   mendapat `409` berulang. **Perbaikan:** slice meneruskan `errors.code`; pada `VAL-NV-19` isian Bed
   dikosongkan dan diberi pesan; `remountKey` ikut versi muat ulang sehingga isian dipasang ulang.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 skema `FE-NVW-05` dan `FE-NVW-06` lengkap, termasuk ketiga keadaan daftar | Terpenuhi | L2–L10; memuat/kosong/gagal di source (`DataTable` teks baku, `InformationAlert` + Coba lagi) |
| AC-2 `UAT-12` sisi admin: daftar tablet → kode `NVT-…`; Buat Login → nama pengguna dan sandi tampil sekali | Terpenuhi | L4–L5 (kode `NVT-000009` karena nomor 1–8 terpakai uji backend dan uji sebelumnya) |
| AC-3 `UAT-13`: bed yang sudah bertablet tidak bisa dipilih; bila permintaan tetap terkirim, pesan `VAL-NV-19` tampil | Terpenuhi | L7, L8 |
| AC-4 kata sandi tidak di Redux, `localStorage`, `sessionStorage`, maupun `console` | Terpenuhi | L5–L6 (storage); source: slice tanpa field respons login, nol `console` |
| AC-5 pindah bed saat bed lama punya panggilan aktif → pesan `VAL-NV-20` | Terpetakan ke source; runtime menunggu `BE-NV-011` sesuai kartu | Editor menampilkan pesan backend pada toast dan memuat ulang (`409`); jalur pindah sukses terbukti L9 |
| AC-6 butir menu terjangkau bagi pemegang izin dan tersembunyi bagi yang tidak | Terpenuhi | L1, L16, L18 |
| DoD — lint `0 errors` | Terpenuhi | §6 |
| DoD — rekaman alur buat → login → atur ulang → nonaktif | Terpenuhi | L4–L6, L10 (JSON jaringan + PNG) |
| DoD — laporan | Terpenuhi | Berkas ini |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Daftar Bed memakai pencarian dan batas tampilan bawaan `FilterSelect` (30 opsi pertama; sisanya lewat kotak cari) karena tidak lagi memakai mode remote |
| Masalah yang diketahui | (1) Pesan `400` dari anotasi DTO backend berbentuk ProblemDetails (`errors.DeviceName[]`); slice menggabungkannya menjadi teks, jadi tetap terbaca. (2) Warning `set-state-in-effect` diwarisi pola hook sebaudara. (3) Kolom tabel tidak di berkas `-table-columns.jsx` terpisah, mengikuti modul sebaudara |
| Dependency backend | `BE-NV-009` ✅. `BE-NV-010` (login tablet) 🟡 tidak dibutuhkan layar ini |
| Data uji di DB dev | `NVT-000003`–`000010` dari uji layar (empat run), semuanya **nonaktif**; akun `nvt-000003`, `000005`, `000007`, `000009` terkunci. Seluruh tablet `NVT-000001`–`000010` kini nonaktif. Policy jabatan uji dipulihkan (20 baris aktif) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Selama sesi, tiga berkas IGD `emergency-assessment-*` di working tree frontend berubah oleh pengguna; tidak disentuh task ini |
| Status Git | Frontend task ini: `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx` (gabungan dengan `FE-NV-015`), `?? src/app/administrator/master-data/nurse-view-bed-tablets/`, `?? src/components/view/administrator/master-data/nurse-view-bed-tablet/`, `?? src/lib/constants/administrator/master-data/nurse-view-bed-tablet/`, `?? src/lib/hooks/administrator/master-data/nurse-view-bed-tablet/`, `?? src/lib/state/slice/administrator/master-data/master-data-nurse-view-bed-tablet-slice.jsx`, `?? src/utils/administrator/master-data/nurse-view-bed-tablet/`. Belum di-commit |
| Langkah berikutnya | `FE-NV-009` (login tablet ke layar tablet bed) sesudah `BE-NV-010` ✅ |
