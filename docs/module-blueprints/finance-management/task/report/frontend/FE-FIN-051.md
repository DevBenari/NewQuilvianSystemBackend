# Laporan Perubahan Frontend — `FE-FIN-051`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-051` |
| Judul | Pejabat Finance melihat dan menetapkan periode peralihan ke mesin payroll |
| Slice | `REV-21` (Slice D: Jembatan Manual & Integrasi Mesin Payroll, `EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` baris 1230 |
| Trace | `FR-FIN-258`; `FIN-DEC-222`; `FIN-API-1.11` §`S.2`; `FIN-VAL-1.13` §`FIN-VAL-286`; `FIN-PERM-1.12` |
| Contract version | `FIN-API-1.11` §`S.2` (`approved 2026-10-13`) |
| Reuse | Layar master `DirectPaymentThreshold` (`FE-FIN-031`/`033`/`034`, pola nilai aktif berversi) |
| Dependency | `BE-FIN-115` 🟡 [BE] (`MstPayrollDeductionCutover`, service, controller `PayrollDeductionCutoverController`) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak dan ikon menu; mengikat untuk label "Peralihan Potong Gaji", penolakan alasan kosong di layar, dan tampilan penolakan mundur dengan pesan backend |
| Klasifikasi | `MEDIUM` — 1 route App Router, 1 view, 1 custom hook, 1 Redux slice, 1 berkas konstanta, 1 berkas utilitas, 1 CSS Module, 1 registrasi store, 1 pendaftaran butir menu sidebar, 1 berkas unit test |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only; laporan tracked dan penandaan roadmap ditulis pada `NewQuilvianSystemBackend` |
| Tanggal | 14 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Seluruh implementasi source frontend, registrasi store, butir menu sidebar, dan unit test telah selesai. `npx eslint` scope PASS (0 error, 0 warning), `node --test` unit test PASS (9/9 pass). Verifikasi runtime manual live browser berstatus `MANUAL TEST: NOT FEASIBLE` karena server backend belum terpasang migration cutover aktif di database lokal |

---

## 1. Masalah yang Diperbaiki

Sebelum task `FE-FIN-051` diimplementasikan:
1. **Ketiadaan Layar Pengaturan Periode Peralihan**: Rumah sakit sedang berada pada masa transisi pemotongan cicilan piutang pegawai secara manual (jembatan berkas payroll). Pejabat Finance tidak memiliki antarmuka untuk melihat apakah periode peralihan ke mesin payroll sudah aktif atau belum (`FR-FIN-258`, `FIN-DEC-222`).
2. **Ketiadaan Visibilitas Status Jembatan Manual**: Tanpa layar ini, staf tidak dapat memastikan kapan sistem mulai menolak unggahan batch manual dan mengalihkan penanganan pemotongan cicilan sepenuhnya ke mesin payroll otomatis HR (`FIN-LYR-PG-03`).
3. **Pencegahan Human Error Saat Penetapan Periode**: Penetapan periode peralihan memerlukan alasan tertulis wajib dan konfirmasi bertingkat agar tidak terjadi pemunduran periode ke bulan yang telah memiliki batch manual disahkan (`FIN-VAL-286`).

---

## 2. Proses Bisnis & Alur Pengguna

1. **Akses Menu**: Pengguna membuka menu **Keuangan** → **Master Data** → **Peralihan Potong Gaji** (`/finance/master-data/payroll-deduction-cutover`). Menu ini dilindungi oleh hak akses `PayrollDeductionCutover : Read`.
2. **Kondisi Belum Ditetapkan (AC 1)**: Bila backend mengembalikan `null` pada endpoint `/current` (belum ada cutover aktif), layar menampilkan banner informatif berwarna oranye: *"Seluruh periode memakai pencatatan manual"*.
3. **Kondisi Periode Aktif**: Bila cutover sudah pernah ditetapkan, kartu *Periode Peralihan Aktif* menampilkan periode aktif (format terbaca ramah: e.g. "Januari 2027 (2027-01)"), alasan penetapan, nama pejabat yang menetapkan, dan waktu penetapan audit.
4. **Riwayat Penetapan**: Tabel *Riwayat Penetapan Periode Peralihan* memuat histori lengkap seluruh penetapan masa lalu yang diambil dari endpoint `GET /` dengan badge status `Aktif` / `Nonaktif`.
5. **Form Penetapan Baru**:
   - Hanya tampil bagi pemegang hak `PayrollDeductionCutover : Create`. Pengguna yang hanya memiliki hak `Read` melihat informasi bahwa perubahan nilai memerlukan hak Create.
   - Kolom *Periode Pertama Mesin Payroll*: input teks dengan format validasi `YYYY-MM` (bulan 01–12).
   - Kolom *Alasan Penetapan / Perubahan*: textarea wajib diisi maksimal 500 karakter.
6. **Validasi & Konfirmasi (AC 2)**:
   - Alasan kosong dicegah langsung di layar sebelum permintaan dikirim.
   - Mengklik *Tetapkan Periode Peralihan* memunculkan `ConfirmModal` yang merangkum periode peralihan baru, periode sebelumnya, alasan perubahan, dan catatan implikasi cutover.
7. **Penanganan Penolakan Backend (AC 3)**:
   - Bila backend menolak pemunduran periode peralihan (HTTP 422 `PayrollDeductionCutoverValidationException`), pesan domain backend (misal: "Tidak dapat memundurkan periode peralihan ke YYYY-MM karena sudah terdapat batch pemotongan manual berstatus DISAHKAN...") ditampilkan secara persis di banner alert galat.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

```text
UI GATE: 7 elemen — REUSE 6, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| No | Elemen UI | Kandidat Base | Status | Bukti / Catatan |
|:--:|---|---|:--:|---|
| 1 | Header halaman | `Hero` | `REUSE` | `src/components/features/base-features/hero.jsx` — eyebrow, title, description |
| 2 | Pembatas izin | `AccessDeniedGate` | `REUSE` | `src/components/features/base-features/access-denied-gate.jsx` — proteksi `PayrollDeductionCutover:Read` |
| 3 | Banner alert & notifikasi | `InformationAlert` | `REUSE` | `src/components/features/base-features/information-alert.jsx` — varian info, warning, danger, success |
| 4 | Input form & textarea | `BaseTextField`, `BaseTextAreaField` | `REUSE` | `src/components/features/base-features/base-form-control.jsx` — validasi inline, deskripsi, karakter counter |
| 5 | Tombol aksi | `BaseButton` | `REUSE` | `src/components/features/base-features/base-button.jsx` — varian primary dan secondary |
| 6 | Modal konfirmasi penetapan | `ConfirmModal` | `REUSE` | `src/components/features/base-features/confirm-modal.jsx` — dialog konfirmasi aksi berkontrol |
| 7 | Riwayat penetapan cutover | `DataTable`, `StatusBadge` | `REUSE` | `src/components/features/base-features/data-table.jsx`, `status-badge.jsx` — kolom No, Periode, Status, Alasan, Audit |
| 8 | Kartu ringkasan periode aktif | Kartu token CSS Module | `COMPOSE` | Rangkai semantik kartu HTML dengan token `--base-*` konsisten dengan pola `direct-payment-threshold-view.jsx` |

Nol komponen baru (`NEW: 0`). Nol pembungkus generik (`WRAP: 0`). Seluruh styling mematuhi token desain baku.

---

## 4. Peta Endpoint API & Kontrak (`FIN-API-1.11` §`S.2`)

Base URL: `/api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers`

| Method | Endpoint | Hak Akses | Handler Thunk | Keterangan |
|---|---|---|---|---|
| `GET` | `/current` | `PayrollDeductionCutover:Read` | `fetchCurrentCutover` | Nilai peralihan aktif. Payload data `null` bila belum pernah ditetapkan (fail-open ke jembatan manual) |
| `GET` | `/` | `PayrollDeductionCutover:Read` | `fetchCutoverHistory` | Riwayat seluruh versi penetapan periode peralihan |
| `POST` | `/` | `PayrollDeductionCutover:Create` | `setPayrollDeductionCutover` | Menetapkan periode baru; versi lama dinonaktifkan otomatis di backend |

Request Payload `POST`:
```json
{
  "automaticFromPeriod": "2027-01",
  "changeReason": "Dasar penetapan mesin payroll HR siap",
  "expectedRowVersion": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## 5. Pemenuhan Acceptance Criteria

1. **AC 1 — Tanpa nilai: tampil "Seluruh periode memakai pencatatan manual"**:
   - Terpenuhi. Saat `current` bernilai `null` (`notSet === true`), komponen me-render `<InformationAlert variant="warning" title="Seluruh periode memakai pencatatan manual">` lengkap dengan uraian bahwa seluruh cicilan dikelola lewat pencatatan manual.
2. **AC 2 — Alasan kosong tidak dapat dikirim**:
   - Terpenuhi. Fungsi `validateCutoverForm` dan `handleSubmitRequest` mengecek `changeReason.trim()`. Bila kosong, permintaan dicegah di sisi klien, pesan *"Alasan penetapan periode peralihan wajib diisi."* muncul di bawah textarea, dan dialog konfirmasi tidak dibuka.
3. **AC 3 — Penolakan mundur ditampilkan dengan pesan backend**:
   - Terpenuhi. Pada `normalizeCutoverError`, status HTTP `422` meneruskan pesan asli dari backend (`rawMessage`), bukan pesan fallback generik. Ketika pejabat mencoba memundurkan periode cutover ke bulan yang memiliki batch manual `DISAHKAN`, pesan penolakan backend tertampil jelas di banner galat halaman dan modal konfirmasi.

---

## 6. Checklist Konsistensi UI & Grep Anti-Regresi

Hasil audit terhadap file perubahan:
1. **Warna literal**: `grep -nEi "#[0-9a-f]{3,8}\b|rgba?\("` pada CSS Module → **0 kecocokan** (PASS).
2. **Typography shared component**: `grep -nE "font-size|font-weight|line-height"` pada CSS Module → **0 penimpaan terhadap komponen shared** (hanya kelas card internal yang memakai token resmi) (PASS).
3. **Tombol non-base**: `grep -nE "<button|className=\"btn|btn-primary"` pada JSX view → **0 kecocokan**; seluruh tombol memakai `BaseButton` (PASS).
4. **Tabel mentah**: `grep -n "<table"` pada JSX view → **0 kecocokan**; tabel riwayat memakai `DataTable` (PASS).
5. **Bootstrap typography utility**: `grep -nE "fw-(bold|semibold|light)|fs-[0-9]"` pada JSX view → **0 kecocokan** (PASS).
6. **!important**: `grep -n "!important"` pada CSS Module → **0 kecocokan** (PASS).

---

## 7. Berkas yang Diperiksa, Dibuat, dan Diubah

### Repository Target: `QuilvianSystemFrontendDev`
1. `src/lib/constants/finance/master-data/payroll-deduction-cutover/payroll-deduction-cutover-constants.jsx` (**Baru**)
2. `src/utils/finance/master-data/payroll-deduction-cutover/payroll-deduction-cutover-utils.jsx` (**Baru**)
3. `src/lib/state/slice/finance/master-data/master-data-payroll-deduction-cutover-slice.jsx` (**Baru**)
4. `src/lib/hooks/finance/master-data/payroll-deduction-cutover/use-payroll-deduction-cutover.jsx` (**Baru**)
5. `src/style/corporate/finance/master-data/payroll-deduction-cutover-view.module.css` (**Baru**)
6. `src/components/view/finance/master-data/payroll-deduction-cutover/payroll-deduction-cutover-view.jsx` (**Baru**)
7. `src/app/finance/master-data/payroll-deduction-cutover/page.jsx` (**Baru**)
8. `src/app/finance/master-data/payroll-deduction-cutover/payroll-deduction-cutover-client.jsx` (**Baru**)
9. `src/lib/state/store.jsx` (**Diperbarui** — registrasi slice `financePayrollDeductionCutover`)
10. `src/utils/menu-sidebar/menu-items.jsx` (**Diperbarui** — penambahan butir menu "Peralihan Potong Gaji")
11. `tests/unit/payroll-deduction-cutover-utils.test.mjs` (**Baru** — 9 pengujian unit terotomasi)

### Repository Blueprint: `NewQuilvianSystemBackend`
1. `docs/module-blueprints/finance-management/task/report/frontend/FE-FIN-051.md` (**Baru** — laporan tracked ini)
2. `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (**Diperbarui** — penandaan status task `FE-FIN-051` menjadi `🟡`)

---

## 8. Verifikasi & Bukti Uji

| Validasi | Perintah | Hasil | Keterangan |
|---|---|---|---|
| Linter | `npx eslint <touched_files>` | **PASS** | 0 error, 0 warning pada seluruh file baru dan file yang diubah |
| Unit Test | `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-deduction-cutover-utils.test.mjs` | **PASS** | 9 test suites pass (AC 2 alasan kosong, format periode, payload, normalisasi camel/pascal, filter history, error classifier, AC 3 penolakan mundur 422, format periode/waktu) |
| Unit Test Regresi | `node --import ./tests/helpers/register.mjs --test tests/unit/direct-payment-threshold-utils.test.mjs` | **PASS** | 13 test suites pass tanpa regresi pada modul acuan |
| Verifikasi Manual | Browser runtime | **NOT FEASIBLE** | Server backend belum menjalankan migrasi database tabel cutover di lokal; verifikasi runtime dialihkan ke pengujian integrasi live |

---

## 9. Kesimpulan & Penandaan Status Roadmap

Task `FE-FIN-051` telah selesai diimplementasikan pada source code frontend dengan struktur modul yang bersih, mematuhi panduan desain token Quilvian, serta lolos pengujian unit dan linting.

Status task ditandai 🟡 **SEBAGIAN** pada roadmap (menunggu pengujian integrasi runtime end-to-end dengan backend).
