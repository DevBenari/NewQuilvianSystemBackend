# Laporan Perubahan Frontend — `FE-FIN-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-FIX-002` |
| Judul | Saringan jenis debitur pada Buku Piutang dan tiga laporan AR memakai nilai yang dipahami backend (`PATIENT_GUARANTOR`) |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap. Mengikuti pola `FIX` yang sudah dipakai `FE-FIN-FIX-001` |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pemilik, 5 Oktober 2026 |
| Trace | `FIN-CQ-11` dan `FIN-CAP-083` (`01-existing-capability-map.md` bagian 21.2 dan 21.4) |
| Contract version | **Tidak ada perubahan kontrak.** Nilai yang dikirim frontend justru dibuat **sesuai** kontrak backend yang sudah berjalan (`FinReceivableDebtorTypes`) |
| Dependency | Tidak ada. Backend tidak perlu diubah |
| Wewenang UI | **Tidak dipakai.** Nol perubahan tampilan: menu, route, tata letak, warna, tipografi, dan **seluruh label yang dibaca pengguna** tetap sama |
| Klasifikasi | `LIGHT` — 1 berkas konstanta diubah, 1 berkas test ditambah; nol komponen, nol view, nol hook, nol style |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `0ed37b5c4` (branch `yasmina`). Working tree **tidak bersih** sebelum task: pekerjaan `FE-FIN-FIX-001` belum di-commit |
| Tanggal | 2026-10-05 |
| Status | ✅ **SELESAI DAN TERVERIFIKASI DI BROWSER.** Lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Backend mengenal tiga jenis debitur piutang, dan membandingkannya **apa adanya**
(`x.DebtorType == request.DebtorType` pada `FinanceReceivableService.cs:63`):

| Backend (`FinReceivableDebtorTypes`) | Frontend sebelum perbaikan |
| --- | --- |
| `PAYER` | `PAYER` ✅ |
| `PATIENT_GUARANTOR` | **`PATIENT`** ❌ |
| `EMPLOYEE_BENEFIT` | `EMPLOYEE_BENEFIT` ✅ |

Nilai tengahnya tidak pernah cocok. Dua akibat yang dirasakan pengguna:

1. **Saringan "Pasien Umum" selalu kosong.** Petugas memilih "Pasien Umum", sistem mengirim
   `debtorType=PATIENT`, backend tidak menemukan satu baris pun, dan tabel menampilkan keadaan kosong —
   **tanpa pesan galat apa pun**, sehingga terbaca seolah memang tidak ada piutang pasien umum. Berlaku
   pada empat layar: Buku Piutang, Report AR, Report AR Created, dan Report Closed Billing.
2. **Label baris tidak menemukan padanan.** Baris piutang yang jenis debiturnya `PATIENT_GUARANTOR` tidak
   punya kunci pada tabel label, sehingga kolomnya memperlihatkan nilai mentah `PATIENT_GUARANTOR`
   kepada pengguna, bukan "Pasien Umum".

---

## 2. Proses bisnis

**Alur normal: menyaring Buku Piutang per jenis debitur.**

1. Petugas membuka Finance → Transaksi A/R → Buku Piutang.
2. Petugas membuka pilihan "Semua Tipe Debitur" dan memilih salah satu jenis.
3. Sistem mengirim jenis itu ke backend sebagai saringan.
4. Backend memulangkan hanya piutang yang jenis debiturnya sama persis.
5. Kolom "Tipe Debitur" pada setiap baris menampilkan label Bahasa Indonesia-nya.

> **Contoh.** Terdapat tiga piutang: satu `PAYER` (PT Asuransi Sehat), satu `PATIENT_GUARANTOR`
> (Tn. Umum), dan satu `EMPLOYEE_BENEFIT` (Budi).
>
> | Petugas memilih | Dikirim sebelum | Hasil sebelum | Dikirim sesudah | Hasil sesudah |
> |---|---|---|---|---|
> | Pasien Umum | `PATIENT` | **0 baris** | `PATIENT_GUARANTOR` | 1 baris (Tn. Umum) |
> | Penjamin / Asuransi | `PAYER` | 1 baris | `PAYER` | 1 baris (tidak berubah) |
> | Manfaat Karyawan | `EMPLOYEE_BENEFIT` | 0 baris* | `EMPLOYEE_BENEFIT` | 0 baris* |
>
> \* Nilainya memang sudah benar sejak awal; hasilnya nol karena piutang karyawan belum pernah ada
> (`EPIC FIN-04`, `FIN-DEC-006`), bukan karena cacat ini.

**Jalur tidak normal.**

| Kejadian | Hasil |
| --- | --- |
| Petugas memilih "Semua Tipe Debitur" | Saringan tidak dikirim; seluruh jenis tampil. Tidak berubah |
| Petugas menekan "Atur ulang filter" | Saringan kembali kosong dan seluruh baris tampil. Tidak berubah |
| Backend memulangkan jenis debitur yang tidak dikenal frontend | Nilai mentahnya ditampilkan apa adanya lewat `sanitizeText`, bukan kosong. Tidak berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Mengapa diperiksa |
| --- | --- |
| `src/lib/constants/finance/receivable/receivable-constants.jsx` | Tempat `DEBTOR_TYPES` didefinisikan |
| `src/lib/state/slice/finance/receivable/finance-receivable-slice.jsx` | Memastikan saringan dikirim sebagai query param apa adanya, tanpa pemetaan ulang |
| `src/lib/hooks/finance/receivable/use-finance-receivable.jsx`, `use-ar-report-receivable-list.jsx`, `use-created-receivable-list.jsx`, `use-closed-billing-receivable-list.jsx` | Keempatnya mengirim `debtorType`; memastikan tidak ada yang menerjemahkan nilainya |
| `src/lib/hooks/finance/receivable/use-corporate-receivable-list.jsx` | Memakai `DEBTOR_TYPES.PAYER` sebagai saringan terkunci — memastikan tidak terdampak |
| `src/components/view/finance/receivable/finance-receivable-table-columns.jsx`, `src/utils/finance/receivable/receivable-utils.jsx` | Dua pembaca `DEBTOR_TYPE_LABELS` |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` (backend, baca saja) | Sumber kebenaran nilai yang sah |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` dan `FinanceReceivablesController.cs` (backend, baca saja) | Memastikan **kedua** controller memakai `FinanceReceivableService.GetPagedAsync` yang sama, sehingga perbandingannya identik |
| `src/lib/constants/finance/subledger-setup/subledger-control-account-constants.jsx` dan `src/lib/hooks/health-services/billing-management/billing-invoices/billing-finalization-constants.js` | Dua tempat lain yang sudah memakai `PATIENT_GUARANTOR` dengan benar — memastikan tidak perlu diubah |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/receivable/receivable-constants.jsx` | Kunci `PATIENT: "PATIENT"` menjadi `PATIENT_GUARANTOR: "PATIENT_GUARANTOR"`, diikuti pembaruan rujukannya pada `DEBTOR_TYPE_LABELS` dan `DEBTOR_TYPE_OPTIONS`. Ditambah blok komentar beralasan yang menyebut `FIN-CQ-11` dan mewajibkan nilainya mengikuti backend |
| `tests/unit/finance-receivable-debtor-types.test.mjs` | **Berkas baru.** Tiga test pengaman |

**Label yang dibaca pengguna tidak diubah.** "Pasien Umum" tetap "Pasien Umum" — yang diperbaiki hanya
nilai yang dikirim ke backend. Nol berkas view, komponen, hook, atau style disentuh.

### 3.3 `UI GATE`

```
UI GATE: N/A — task hanya menyentuh berkas constant dan test; nol JSX dan nol CSS ditulis.
```

Sesuai `base-component-decision-gate.md`: gerbang dilewati untuk task yang hanya menyentuh constant,
utility, hook, atau dokumentasi.

---

## 4. Checklist konsistensi UI dan grep anti-regresi

Dijalankan pada berkas yang diubah, sesuai `ui-consistency-checklist.md` bagian G.

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal (`#`, `rgb(`) | **Kosong** |
| Typography yang menimpa komponen shared (`font-size`, `font-weight`, `line-height`) | **Kosong** |
| Tombol non-base (`<button`, `btn-primary`) | **Kosong** |
| Tabel mentah (`<table`) | **Kosong** |
| Bootstrap utility typography (`fw-bold`, `fs-[0-9]`) | **Kosong** |
| `!important` baru | **Kosong** |
| Blok `prefers-color-scheme: dark` baru | **Kosong** |

Seluruhnya kosong karena task ini memang tidak menulis markup maupun style.

---

## 5. Verifikasi

### 5.1 Validasi otomatis

| Bukti | Hasil |
| --- | --- |
| `npx eslint --quiet` pada 2 berkas yang diubah | **PASS** — exit 0, nol error dan nol warning |
| `node --test tests/unit/finance-receivable-debtor-types.test.mjs` | **PASS — 3/3** |
| `AUTOMATED TEST` | `AUTOMATED TEST: node --test tests/unit/finance-receivable-debtor-types.test.mjs — PASS (3/3)` |
| `npm run lint:errors` seluruh repo | `NOT RUN` — atas instruksi pemilik, perintah berat tidak dijalankan otomatis |
| `npm run test:unit` seluruh suite | `NOT RUN` — alasan sama |
| `npm run build` | `NOT RUN` — **instruksi eksplisit pemilik** ("tidak usah build secara automatis, biarkan saya yg build") |

**Isi test pengaman yang ditambahkan:**

1. Nilai `DEBTOR_TYPES` sama persis dengan tiga nilai canonical, dan `"PATIENT"` **tidak boleh** kembali.
2. Setiap jenis punya label dan tepat satu opsi saringan, dan opsi pertama adalah "semua" bernilai kosong.
3. Nilainya dibandingkan **langsung dengan source backend**: test membaca `FinReceivable.cs` pada repository
   backend yang bersebelahan, mengambil isi `FinReceivableDebtorTypes`, lalu menuntut keduanya identik.
   Bila repository backend tidak terjangkau, test ini dilewati alih-alih gagal palsu. **Pada sesi ini
   test itu benar-benar berjalan** (tidak dilewati), sehingga kecocokan dengan backend terbukti, bukan
   diasumsikan.

### 5.2 Verifikasi manual di browser

`MANUAL TEST: PASS — 9/9`. Dijalankan pada dev server `next dev -p 3100` memakai Chromium (Playwright),
dengan jawaban API disediakan tiruan yang **meniru perbandingan persis milik backend** (satu baris per
jenis debitur, disaring dengan `===`). Dev server milik pemilik di port 3000 tidak disentuh.

| # | Yang diverifikasi | Hasil |
| ---: | --- | --- |
| 1 | Buku Piutang terbuka tanpa galat | **PASS** |
| 2 | Ketiga baris tampil saat tanpa saringan | **PASS** |
| 3 | Label baris `PATIENT_GUARANTOR` terbaca "Pasien Umum", bukan nilai mentah | **PASS** |
| 4 | Memilih "Pasien Umum" mengirim `debtorType=PATIENT_GUARANTOR` | **PASS** — request tercatat: `/api/finance/receivable?debtorType=PATIENT_GUARANTOR&sortBy=dueDate&sortDirection=asc&pageNumber=1&pageSize=25` |
| 5 | Hasilnya hanya baris pasien umum, **bukan nol baris** seperti sebelum perbaikan | **PASS** |
| 6 | Tabel tidak menampilkan keadaan kosong | **PASS** |
| 7 | Memilih "Manfaat Karyawan" mengirim `EMPLOYEE_BENEFIT` | **PASS** |
| 8 | Hasil karyawan hanya baris karyawan | **PASS** |
| 9 | "Atur ulang filter" mengembalikan seluruh baris dan membuang saringan | **PASS** |

Nol galat pada console peramban dan nol exception. Jenis debitur yang benar-benar terkirim selama sesi
verifikasi: `""`, `PATIENT_GUARANTOR`, `EMPLOYEE_BENEFIT` — tidak ada lagi nilai `PATIENT`.

**Temuan sampingan yang berguna, dicatat apa adanya.** Buku Piutang memanggil
`GET /api/finance/receivable` (`FinanceArController`), **bukan**
`/api/v1/corporate/finance-management/receivables` (`FinanceReceivablesController`) seperti dugaan awal.
Keduanya sah dan memakai service serta DTO kueri yang sama, jadi perbaikan ini berlaku pada dua-duanya.
Hal ini **belum tercatat** di capability map dan ditambahkan pada bagian 21.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nilai jenis debitur frontend sama persis dengan backend | **Terpenuhi** | Test 1 dan 3, bagian 5.1 |
| Saringan "Pasien Umum" memulangkan baris, bukan nol | **Terpenuhi** | Verifikasi manual #4 dan #5 |
| Baris `PATIENT_GUARANTOR` menampilkan label, bukan nilai mentah | **Terpenuhi** | Verifikasi manual #3 |
| Saringan jenis lain tidak berubah perilakunya | **Terpenuhi** | Verifikasi manual #7 dan #8 |
| Atur ulang saringan tetap benar | **Terpenuhi** | Verifikasi manual #9 |
| Label yang dibaca pengguna tidak berubah | **Terpenuhi** | Bagian 3.2 |
| Nol perubahan tampilan, route, dan kontrak | **Terpenuhi** | Bagian 3.2 dan 4 |
| Lint pada berkas yang diubah bersih | **Terpenuhi** | Bagian 5.1 |

**Definition of Done terpenuhi**, karena itu statusnya ✅. Validasi seluruh repo (`lint:errors`,
`test:unit`, `build`) sengaja tidak dijalankan atas instruksi pemilik, dan **bukan** bagian Definition of
Done untuk task sekecil ini; cakupan yang relevan sudah divalidasi pada bagian 5.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko yang tersisa | **Rendah.** Perubahan hanya memperbesar kemungkinan baris ditemukan; tidak ada jalur yang kehilangan kemampuan. Satu-satunya yang perlu diingat: bila ada kode di luar daftar bagian 3.1 yang menyimpan nilai `"PATIENT"` secara literal (bukan lewat `DEBTOR_TYPES`), kode itu tetap salah — pencarian `PATIENT_GUARANTOR` dan `DEBTOR_TYPES` pada seluruh `src` dan `tests` tidak menemukan pemakaian seperti itu |
| Temuan di luar cakupan (tidak diubah) | (a) Satu nilai yang sama memiliki **tiga** label berbeda di frontend: "Pasien Umum" (Finance), "Penjamin Pasien Pribadi" (Subledger), dan "Pasien/Penjamin" (Billing). Menyatukannya adalah keputusan UI milik pemilik, bukan perbaikan cacat. (b) Finance memakai dua base URL untuk piutang (`/finance/receivable` dan `/v1/corporate/finance-management/receivables`), masing-masing dengan controller sendiri tetapi service yang sama |
| Dampak pada dokumen blueprint | `FIN-CQ-11` pada `01-existing-capability-map.md` ditandai **CLOSED**; `FIN-CAP-083` diperbarui dari `Repair` menjadi `Ready to reuse`; bagian 21.4 diperbarui agar tabel ketidakcocokan tidak lagi menyesatkan |
| Git | Nol stage, commit, push, pull, merge, rebase, dan deploy. Working tree memuat pula pekerjaan `FE-FIN-FIX-001` yang belum di-commit dan **tidak** disentuh task ini |
| Satu langkah berikutnya | Putuskan apakah tiga label berbeda untuk `PATIENT_GUARANTOR` disatukan; bila ya, itu task UI tersendiri |
