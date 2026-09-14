# Laporan Perubahan Frontend — `FE-ACC-P2-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-014` |
| Judul | Tombol Periode Akuntansi mengikuti hak akses |
| Slice | `HARDENING` — batch 14 September 2026 |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-014` (revisi 4) |
| Trace | `ACC-GAP-010`; `ACC-DEC-080`; acceptance (4) `FE-ACC-004` pada [`frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Contract version | `ACC-API-0.10` grup Accounting Period — **tidak berubah**; `ACC-PERMISSION-0.5` `AccountingPeriod : Create/Close/Reopen` |
| Wewenang UI | Kartu roadmap: tombol dimatikan beserta keterangan, bukan disembunyikan. Warna, jarak, dan letak keterangan `DEV_DISCRETION` |
| Dependency | — (tanpa perubahan backend) |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1, berkas diubah 1 (4 berkas), logika 0, kontrak API 0, database 0, keamanan 1 (tampilan hak akses, bukan penegakan), UI 1 (satu layar) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `RizkiV2` — layar Periode Akuntansi; laporan dan tautan bukti di repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `f6b1498fe` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b3ab542e` (branch `rizkiG`) — `AccountingPeriodController` |
| Tanggal | 14 September 2026 |
| Status | **🟡 SEBAGIAN** — 6 dari 6 acceptance terpetakan ke source; ESLint `0 error, 0 warning`; tinggal `npm run build` oleh owner. `IMPLEMENTATION COMPLETE`, `READY FOR UAT` |

---

## 1. Keadaan yang ditemukan di awal

Layar `/corporate/accounting/periods` sejak `FE-ACC-004` (4 September 2026) menampilkan tombol
**Bangkitkan Setahun** serta butir menu **Tutup Sementara**, **Tutup Permanen**, dan **Buka Kembali**
kepada **semua** pengguna yang dapat membuka layar, apa pun haknya. Bukti di
`accounting-period-view.jsx` sebelum perubahan: ketiga tombol hanya dimatikan oleh `actionLoading`
dan status periode; tidak ada satu pun pemeriksaan hak.

Akibatnya: petugas yang hanya punya `AccountingPeriod : Read` menekan Buka Kembali, mengisi alasan
panjang, menekan konfirmasi — lalu ditolak `403` dari backend. Pekerjaannya sia-sia, dan pesannya
tidak menjelaskan bahwa masalahnya hak akses.

Waktu itu repository belum punya mekanisme hak sisi klien, sehingga acceptance (4) `FE-ACC-004`
dicatat `ACC-GAP-010`. Hook `usePermission` kini sudah ada dan dipakai layar Accounting lain
(Daftar Periksa Penutupan, Tutup Tahun, Jurnal Berulang, Pengaturan Akuntansi). `ACC-DEC-080`
(14 September 2026) memutuskan celah ini ditutup di frontend lewat hook itu, tanpa `AvailableActions`
di backend.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Pengguna berhak penuh

Tidak ada yang berubah. Tombol dan butir menu menyala seperti sebelumnya, dan tidak ada keterangan
tambahan.

### 2.2 Pengguna tanpa sebagian hak

Contoh: Sari, staf akuntansi dengan hak `AccountingPeriod : Read` dan `AccountingPeriod : Close`,
tanpa `Create` dan `Reopen`.

| Langkah | Yang dilihat Sari |
| ---: | --- |
| 1 | Membuka Periode Akuntansi. Tombol **Bangkitkan Setahun tampil tetapi mati**; di bawah kartu konteks tertulis "Anda tidak memiliki hak Bangkitkan Periode." |
| 2 | Membuka menu Tindakan pada periode `2026-08` berstatus Tutup Sementara. **Tutup Permanen menyala** (ia punya hak `Close`). **Buka Kembali tampil tetapi mati**, dan di bawahnya tertulis "Anda tidak memiliki hak Buka Kembali Periode." |
| 3 | Membuka menu pada periode `2026-09` berstatus Terbuka. Tutup Sementara dan Tutup Permanen menyala; tidak ada keterangan karena tidak ada butir yang mati |

Keterangan ditulis **terlihat**, bukan hanya sebagai tooltip, karena sebagian browser tidak
menampilkan `title` pada tombol yang `disabled`. `title` tetap dipasang sebagai tambahan.

### 2.3 Jalur tidak normal

| Keadaan | Perilaku |
| --- | --- |
| Daftar hak **belum termuat** atau **gagal dimuat** | Tombol **menyala** — `usePermission` menjawab boleh selama belum diketahui (acceptance 5). Bila pengguna ternyata tidak berhak, backend menolak `403` dan pesannya tampil sebagai toast "Tindakan Ditolak" |
| SuperAdmin | Semua menyala — `selectHasPermission` menjawab benar untuk SuperAdmin |
| Tombol mati tetapi dialog dipanggil dari jalur lain | `openGenerateDialog`, `openCloseDialog`, dan `openReopenDialog` ikut menolak membuka dialog bila haknya tidak ada |
| Pengguna tanpa `AccountingPeriod : Read` | Tidak berubah: `AccessDeniedGate` menampilkan penolakan akses dari respons daftar |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; kartu `FE-ACC-P2-014` dan `FE-ACC-004`; `ACC-DEC-080`;
`AccountingPeriodController.cs` pada `rizkiG` (atribut `[AccessAction]`/`[AccessPermission]`);
`use-permission.jsx`; `permission-slice.jsx`; `accounting-period-view.jsx`;
`use-accounting-period.jsx`; `accounting-period-constants.jsx`;
`accounting-period-view.module.css`; `period-closing-view.jsx` dan
`period-closing-constants.jsx` (kalimat "Anda tidak memiliki hak …"); `use-recurring-journal.jsx`
dan `recurring-journal-view.jsx` (pola `title` + paragraf keterangan); `base-button.jsx` (props
`disabled` dan `title` diteruskan); `src/app/globals.css` (token `--color-text-muted`, `--space-1`,
`--space-2`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/accounting-period/accounting-period-constants.jsx` | `ACCOUNTING_PERIOD_PERMISSION` (resource dan tiga aksi, disalin dari controller) dan `ACCOUNTING_PERIOD_PERMISSION_COPY` (tiga kalimat keterangan) |
| `src/lib/hooks/corporate/accounting/accounting-period/use-accounting-period.jsx` | Tiga `usePermission`; `generateBlockedReason`, `closeBlockedReason`, `reopenBlockedReason` diekspos; ketiga pembuka dialog menolak bila haknya tidak ada |
| `src/components/view/corporate/accounting/accounting-period/accounting-period-view.jsx` | Tombol Bangkitkan Setahun dan tiga butir menu memakai `disabled` + `title` dari alasan hook; paragraf keterangan di bawah kartu konteks dan di dalam menu Tindakan |
| `src/style/corporate/accounting/accounting-period-view.module.css` | Dua kelas baru, `.permissionHint` dan `.actionMenuHint`, hanya memakai token — tanpa warna literal, tanpa typography |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap view → hook layar → hook bersama `usePermission` → `permission-slice`. Teks
berada di konstanta layar, logika di hook, view hanya menampilkan. Nol komponen baru, nol slice
baru, nol perubahan `globals.css`, nol `style={{ }}`.

#### Gerbang keputusan base component

`UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol Bangkitkan Setahun mati beserta tooltip | `BaseButton` | `base-features/base-button.jsx` meneruskan `...props` termasuk `title`; dipakai sama di `recurring-journal-view.jsx` baris 180–187 | REUSE | `disabled` + `title` |
| Butir menu Tutup Sementara, Tutup Permanen, Buka Kembali mati | `BaseButton` | Butir menu yang sudah ada memakai `BaseButton` | REUSE | `disabled` + `title` |
| Keterangan terlihat di bawah kartu konteks | Pola paragraf `.muted` | `recurring-journal-view.jsx` baris 216–218, `period-closing-view.jsx` `.actionHint` | REUSE | `<p>` ber-kelas CSS Module layar sendiri |
| Keterangan di dalam menu Tindakan | Pola paragraf yang sama | Idem | REUSE | `<p>` di bawah butir |
| Penentu hak | `usePermission` | `lib/hooks/auth/use-permission.jsx`, dipakai 12 tempat | REUSE | Apa adanya |

Tidak ada elemen bukan `REUSE`, sehingga tidak ada pilihan yang perlu diputuskan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah — `DataTable` "Mengambil periode akuntansi..." |
| Kosong | Tidak berubah — "Periode belum dibangkitkan." |
| Gagal | Tidak berubah — pesan galat di atas tabel |
| Tanpa hak akses (tombol) | Tombol tampil mati; "Anda tidak memiliki hak Bangkitkan Periode.", "Anda tidak memiliki hak Tutup Periode.", "Anda tidak memiliki hak Buka Kembali Periode." |
| Tanpa hak baca | Tidak berubah — `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Hak yang diperiksa dan endpoint yang dijaganya:

#### Corporate / Accounting / Accounting Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/accounting/periods/generate` | Bangkitkan Setahun | `AccountingPeriod : Create` |
| `POST` | `/v1/corporate/accounting/periods/{id}/close` | Tutup Sementara, Tutup Permanen | `AccountingPeriod : Close` |
| `POST` | `/v1/corporate/accounting/periods/{id}/reopen` | Buka Kembali | `AccountingPeriod : Reopen` |

Daftar hak pengguna dibaca dari `GET /v1/auth/permissions` lewat `permission-slice`, sekali per sesi.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 3 berkas JSX yang berubah | Exit `0`, keluaran kosong — **0 error, 0 warning** | `PASS` | Terminal agent, 14 Sep 2026 |
| Grep anti-regresi — baris CSS yang ditambahkan | Nol hex/`rgb`, nol `font-size`/`font-weight`/`line-height`, nol `!important` | `PASS` | `git diff -U0` disaring |
| Grep anti-regresi — baris JSX yang ditambahkan | Nol `<button` mentah, nol `<table`, nol `style={{`, nol utility `fw-`/`fs-` | `PASS` | Idem |
| Pemeriksaan source — ejaan hak | `Create`, `Close`, `Reopen` pada resource `AccountingPeriod` cocok dengan `AccountingPeriodController` baris 67–68, 81–82, 95–96 | `PASS` | Kedua berkas |
| Uji unit yang memuat berkas ini | Tidak ada — grep `tests/` nol kecocokan | `NOT APPLICABLE` | — |
| `npm run build` | Dijalankan owner — belum | `NOT RUN` | DoD kartu: "Build dijalankan owner" |

`AUTOMATED TEST: SKIPPED (opsional) — ACC-DEC-081; tidak ada uji yang memuat ketiga berkas.`

Uji manual: `NOT FEASIBLE` — repository tidak punya `playwright.config` dan agent tidak punya
peramban; skenario menuntut akun non-SuperAdmin yang sengaja tidak diberi `Create`/`Reopen`.
Skenario bagian 2.2 diserahkan ke tim UAT (`UAT-08`, `UAT-09`).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Tombol Bangkitkan Periode mati bagi pengguna tanpa `AccountingPeriod : Create` | **Terpenuhi** | `disabled={… \|\| Boolean(generateBlockedReason)}` |
| 2 | Tombol Tutup mati bagi pengguna tanpa `AccountingPeriod : Close` | **Terpenuhi** | Tutup Sementara dan Tutup Permanen memakai `closeBlockedReason` |
| 3 | Tombol Buka Kembali mati bagi pengguna tanpa `AccountingPeriod : Reopen` | **Terpenuhi** | `reopenBlockedReason` |
| 4 | Ketiganya **tetap tampil**, dan keterangan kenapa mati terbaca pengguna | **Terpenuhi** | Syarat tampil tetap hanya status periode; keterangan sebagai `title` dan paragraf terlihat |
| 5 | Selama daftar hak belum dimuat, perilaku mengikuti `usePermission` apa adanya | **Terpenuhi** | `allowed` dipakai langsung, tanpa logika tambahan |
| 6 | Nol perubahan backend, `globals.css` tidak disentuh, tanpa `style={{ }}` | **Terpenuhi** | `git status` frontend: 4 berkas, tidak ada `globals.css`; nol berkas source backend |

| Butir DoD | Hasil |
| --- | --- |
| Lint hijau | **Ya** — 0 error, 0 warning pada berkas yang berubah |
| Laporan task tertulis | **Ya** — berkas ini |
| Build dijalankan owner | **Belum** — satu-satunya sisa |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Butir **Tutup Sementara** masih ditawarkan untuk periode Terbuka, padahal sejak `BE-ACC-P2-006` (9 Sep 2026) backend menolak `Open` → `SoftClosed` dengan `409`. Di luar cakupan task ini — dicatat, tidak diubah |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M src/components/view/corporate/accounting/accounting-period/accounting-period-view.jsx`<br>` M src/lib/constants/corporate/accounting/accounting-period/accounting-period-constants.jsx`<br>` M src/lib/hooks/corporate/accounting/accounting-period/use-accounting-period.jsx`<br>` M src/style/corporate/accounting/accounting-period-view.module.css`<br>Nol commit, push, stage, atau merge |
| Langkah berikutnya | Owner menjalankan `npm run build`; bila hijau, task naik ke ✅. Tim UAT menjalankan skenario bagian 2.2 dengan akun tanpa `Create`/`Reopen` |
