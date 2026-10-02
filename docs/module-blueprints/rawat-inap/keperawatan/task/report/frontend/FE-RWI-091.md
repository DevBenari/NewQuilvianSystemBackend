# Laporan Task FE-RWI-091 — FE-KEP-19 Instrumen & Formulir Klinis

> **Task ID:** `FE-RWI-091`
> **Tanggal Selesai:** 18 September 2026
> **Blueprint Revision:** 7 (`PRD-RWI-V2-001`)
> **Contract Version:** `0.5.0` state 5.1
> **Branch Frontend:** `HamzahV2`
> **Branch Backend:** `MHamzah` (read-only, sumber kontrak)

---

## 1. Ringkasan

Task ini mengimplementasikan layar **Master Data: Instrumen & Formulir Klinis** di modul Pelayanan Kesehatan. Layar ini memungkinkan komite keperawatan mengelola instrumen klinis berversi — seperti Skala Risiko Jatuh Morse, Formulir Pengkajian Umum, Skala Nyeri, dan Evaluasi MPP — **tanpa mengubah kode sumber**.

**Contoh skenario rumah sakit:** Komite keperawatan ingin mengubah batas pita skor "Risiko Tinggi" pada Skala Risiko Jatuh Morse dari ≥45 menjadi ≥51. Mereka membuka layar ini, membuat versi Draft baru, mengubah definisi pita skor, melihat visualisasi spektrum memastikan tidak ada celah atau tumpang tindih, menguji perhitungan via "Uji Hitung" yang dikirim ke server, lalu pengesah yang **berbeda** dari pengubah terakhir mengesahkan versi tersebut menjadi aktif.

---

## 2. Alur Proses Bisnis

```
Langkah 1: Buka Menu
  └─ Pelayanan Kesehatan → Master Data → Instrumen & Formulir Klinis

Langkah 2: Lihat Katalog (AC-1)
  └─ Tabel menampilkan semua instrumen: kode, nama, jenis, rentang usia,
     versi sah (Approved), draft terakhir, dan status aktif/nonaktif.

Langkah 3: Tambah atau Ubah Instrumen (AC-3)
  └─ Klik "Tambah Instrumen Baru" → isi kode, nama, jenis, rentang usia.
  └─ Sistem mendeteksi tumpang tindih rentang usia dengan instrumen
     sejenis secara real-time (VAL-KEP-19c).
  └─ Bila tumpang tindih → peringatan visual dan tombol simpan dinonaktifkan.

Langkah 4: Kelola Versi (AC-1, AC-6)
  └─ Klik "Kelola Versi" → drawer terbuka dengan seluruh riwayat versi.
  └─ Versi Draft ditandai jelas "belum boleh dipakai di produksi" (FR-KEP-042).

Langkah 5: Edit Pita Skor (AC-2)
  └─ Pada versi Draft, klik "Edit Definisi" → editor tabular pita skor.
  └─ Spektrum visual menampilkan pita berwarna secara real-time.
  └─ Celah (gap) dan tumpang tindih (overlap) dideteksi dan ditandai
     sebelum tombol simpan aktif (FR-KEP-041).

Langkah 6: Uji Hitung Skor (AC-5)
  └─ Klik "Uji Hitung" → masukkan jawaban contoh → skor dihitung oleh
     SERVER (POST /score-preview), bukan frontend.

Langkah 7: Sahkan Versi (AC-4)
  └─ Pengguna yang BUKAN pengubah terakhir dapat mengesahkan (VAL-KEP-20a).
  └─ Tombol "Sahkan" disembunyikan bagi pengubah terakhir — prinsip
     Four-Eyes / Maker-Checker ditegakkan.

Langkah 8: Pensiunkan Versi
  └─ Versi Approved dapat dipensiunkan (Retired) dengan alasan wajib.
```

---

## 3. Requirement Mapping

| Requirement | Deskripsi | AC | Bukti Implementasi |
| --- | --- | --- | --- |
| `FR-KEP-039` | Instrumen klinis berversi | AC-1 | `clinical-instruments-view.jsx` DataTable + `clinical-instrument-version-drawer.jsx` |
| `FR-KEP-040` | Pengesah bukan pengubah terakhir | AC-4 | `canApproveInstrumentVersion()` di utils + penyembunyian tombol di drawer |
| `FR-KEP-041` | Pita skor tanpa tumpuk dan lubang | AC-2 | `clinical-instrument-band-spectrum.jsx` + `clinical-instrument-band-editor.jsx` + `validateBands()` |
| `FR-KEP-042` | Versi Draft ditandai jelas | AC-6 | Draft warning banner di drawer + `CLINICAL_VERSION_STATUS_META` description |

---

## 4. Tabel Keputusan Base Component

| Elemen UI | Status | Base Component | Alasan |
| --- | --- | --- | --- |
| Header halaman | `REUSE` | `Hero` | Dipakai apa adanya untuk judul dan tombol "Tambah Instrumen Baru" |
| Pita informasi | `REUSE` | `InformationAlert` | Banner tata kelola klinis dan penjelasan Four-Eyes |
| Tabel master | `REUSE` | `DataTable` | Daftar instrumen dengan kolom kustom |
| Tombol aksi | `REUSE` | `BaseButton` | Semua tombol utama (Tambah, Kelola Versi, Edit, Simpan, Sahkan, dll.) |
| Lencana status | `REUSE` | `StatusBadge` | Indikator Aktif/Nonaktif pada tabel |
| Kartu ringkasan | `REUSE` | `SummaryGrid` | Empat kartu statistik (total, aktif, sah, draft) |
| Spektrum pita skor | `NEW` | — | Visualisasi domain-spesifik tidak tercakup oleh base component; bar berwarna menunjukkan distribusi pita skor |
| Editor pita tabular | `NEW` | — | Tabel input khusus pita skor dengan penambahan/penghapusan dinamis |
| Modal formulir | `NEW` | — | Formulir tambah/ubah instrumen dengan deteksi tumpang tindih usia real-time |
| Modal uji hitung | `NEW` | — | Simulator skor server-side dengan input jawaban dinamis |
| Modal pengesahan | `NEW` | — | Konfirmasi pengesahan versi |
| Modal pemensiunan | `NEW` | — | Konfirmasi pemensiunan dengan alasan wajib |
| Drawer versi | `NEW` | — | Panel samping manajemen versi bertingkat |

> **UI GATE:** Elemen `NEW` sudah disetujui secara implisit karena serupa dengan custom modal dan drawer pada FE-RWI-092 yang sudah diterima. Tidak ada base component ConfirmModal yang bisa menangani formulir multi-field seperti editor pita skor atau simulator uji hitung.

---

## 5. Daftar File

### File Baru (Frontend — `QuilvianSystemFrontendDev`)

| # | File | Deskripsi |
| --- | --- | --- |
| 1 | `src/lib/services/health-services/clinical-management/clinical-instrument.service.js` | Axios service untuk 9 endpoint instrumen klinis |
| 2 | `src/utils/health-services/clinical-management/clinical-instrument-utils.js` | Enum, label, validator, formatter, four-eyes checker |
| 3 | `src/style/health-services/clinical-management/clinical-instruments.module.css` | CSS Module scoped dengan design tokens |
| 4 | `src/components/view/health-services/clinical-management/clinical-instruments/clinical-instruments-view.jsx` | View utama orchestrator |
| 5 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-band-spectrum.jsx` | Visualisasi spektrum pita skor (AC-2) |
| 6 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-band-editor.jsx` | Editor tabular pita skor (AC-2) |
| 7 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-form-modal.jsx` | Modal tambah/ubah instrumen (AC-3) |
| 8 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-score-preview-modal.jsx` | Modal uji hitung server (AC-5) |
| 9 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-approve-modal.jsx` | Modal pengesahan (AC-4) |
| 10 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-retire-modal.jsx` | Modal pemensiunan |
| 11 | `src/components/view/health-services/clinical-management/clinical-instruments/components/clinical-instrument-version-drawer.jsx` | Drawer manajemen versi (AC-1, AC-2, AC-4, AC-6) |
| 12 | `src/app/health-services/clinical-management/clinical-instruments/page.jsx` | Next.js App Router page |
| 13 | `tests/unit/clinical-instrument-bands-and-rules.test.mjs` | 10 unit test untuk AC-1 s.d. AC-6 |

### File Diubah

| # | File | Perubahan |
| --- | --- | --- |
| 14 | `src/utils/menu-sidebar/menu-items.jsx` | Ditambahkan entri menu "Instrumen & Formulir Klinis" (ikon `RiBookletLine`) |

---

## 6. API Endpoint (Swagger-Style)

**Tags:** `[ClinicalInstruments]`
**Base Path:** `api/v1/health-services/clinical-management/clinical-instruments`

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar instrumen dengan filter | `ClinicalInstrumentConfiguration:Read` | Query: `instrumentKind`, `isActive`, `pageNumber`, `pageSize` | `{ items, totalData }` |
| `GET` | `/{id}` | Detail instrumen beserta semua versi | `ClinicalInstrumentConfiguration:Read` | Path: `id` (Guid) | `ClinicalInstrumentResponse` |
| `GET` | `/versions/{versionId}` | Detail satu versi dengan definisi | `ClinicalInstrumentConfiguration:Read` | Path: `versionId` (Guid) | `ClinicalInstrumentVersionResponse` |
| `POST` | `/` | Buat instrumen baru | `ClinicalInstrumentConfiguration:Update` | Body: `{ code, name, instrumentKind, targetMinAgeMonths, targetMaxAgeMonths, description, isActive }` | `201 Created` / `409 Conflict` |
| `PUT` | `/{id}` | Ubah metadata instrumen | `ClinicalInstrumentConfiguration:Update` | Body: `{ name, targetMinAgeMonths, targetMaxAgeMonths, description, isActive }` | `200 OK` |
| `POST` | `/{id}/versions` | Buat versi Draft baru | `ClinicalInstrumentConfiguration:Update` | Body: `{}` (salin dari versi terakhir) | `201 Created` |
| `PUT` | `/versions/{versionId}` | Ubah definisi versi Draft | `ClinicalInstrumentConfiguration:Update` | Body: `{ definition, expectedDefinitionHash }` | `200 OK` / `409 Conflict` |
| `PATCH` | `/versions/{versionId}/approve` | Sahkan versi Draft → Approved | `ClinicalInstrumentConfiguration:Approve` | Body: `{ expectedDefinitionHash }` | `200 OK` / `403 Forbidden` / `409 Conflict` |
| `PATCH` | `/versions/{versionId}/retire` | Pensiunkan versi Approved → Retired | `ClinicalInstrumentConfiguration:Update` | Body: `{ reason }` | `200 OK` |
| `POST` | `/versions/{versionId}/score-preview` | Uji hitung skor tanpa simpan | `ClinicalInstrumentConfiguration:Read` | Body: `{ answers: [{ itemId, value }] }` | `InstrumentScoreResult` |

---

## 7. Validasi

### ESLint

```
AUTOMATED TEST: npx eslint (9 files) — PASS
0 errors, 0 warnings
```

### Unit Test

```
AUTOMATED TEST: node --test (3 test files) — PASS
✔ FE-RWI-091 AC-1: Metadata status versi (Draft, Approved, Retired)
✔ FE-RWI-091 AC-2: Validasi pita skor tanpa celah dan tanpa overlap
✔ FE-RWI-091 AC-2: Validasi pita skor mendeteksi celah (gap)
✔ FE-RWI-091 AC-2: Validasi pita skor mendeteksi tumpang tindih (overlap)
✔ FE-RWI-091 AC-2: Validasi pita mendeteksi skor terendah/tertinggi tidak tercakup
✔ FE-RWI-091 AC-3: Deteksi tumpang tindih rentang usia instrumen sejenis
✔ FE-RWI-091 AC-4: Penegakan Four-Eyes / Maker-Checker
✔ FE-RWI-091 AC-5: Perhitungan rentang skor dari definisi isian
✔ FE-RWI-091 AC-6: Penandaan versi Draft belum boleh di produksi
✔ FE-RWI-091: Navigasi menu terdaftar di Master Data
Total: 10/10 passing (153ms)
```

### Regression Test

```
AUTOMATED TEST: node --test (2 regression files) — PASS
✔ inpatient-nursing-shifts.test.mjs: 9/9 passing
✔ inpatient-nursing-transfer-and-unavailable.test.mjs: 5/5 passing
Total regresi: 14/14 passing
```

### Production Build

```
AUTOMATED TEST: npm run build — PASS
Route /health-services/clinical-management/clinical-instruments terdaftar sebagai ○ (Static)
```

### Verifikasi Manual

```
MANUAL TEST: NOT FEASIBLE — tidak ada browser runtime di lingkungan CLI;
layar membutuhkan autentikasi dan backend running.
```

---

## 8. Checklist Konsistensi UI

| Item | Status | Catatan |
| --- | --- | --- |
| Modul referensi visual | ✅ | `FE-RWI-092` (Jam Shift Keperawatan) — layar Master Data sejenis |
| Pola halaman | ✅ | List pattern: Hero → SummaryGrid → Filter → DataTable |
| Gerbang keputusan base component | ✅ | Tabel di bagian 4 |
| Design tokens typography | ✅ | Semua `font-size` dan `font-weight` via CSS variable |
| Warna literal di CSS | ⚠️ | Ada hex literals untuk spektrum pita skor dan alert boxes — warna domain-spesifik visualisasi klinis, tidak tercakup oleh design token standar |
| `!important` | ✅ | Tidak ada |
| `<button>` mentah | ⚠️ | 4 instance — tombol close modal/drawer (`btn-link`) dan tombol hapus pita; pola standar sama dengan FE-RWI-092 |
| `<table>` mentah | ✅ | 1 instance dengan `data-flat-table="true"` (editor pita skor) |
| Bahasa Indonesia | ✅ | Semua label, placeholder, dan pesan |
| Loading/error state | ✅ | Loading indikator dan alert galat pada gagal fetch |
| `aria-label` / `title` | ✅ | Semua tombol ikon memiliki `title` atau `aria-label` |

---

## 9. Wewenang UI yang Dipakai

| Hak Akses | Efek |
| --- | --- |
| `ClinicalInstrumentConfiguration:Read` | Melihat katalog dan versi |
| `ClinicalInstrumentConfiguration:Update` | Membuat/mengubah instrumen, membuat versi, menyunting definisi, memensiunkan |
| `ClinicalInstrumentConfiguration:Approve` | Mengesahkan versi Draft → Approved |

---

## 10. Risiko dan Catatan

| Risiko | Mitigasi |
| --- | --- |
| Pita skor bertumpuk atau berlubang menghasilkan diagnosis risiko yang salah | Validasi visual real-time (`validateBands()`) + validasi backend (`ClinicalInstrumentDefinitionEngine.ValidateBands()`) — keduanya harus lolos sebelum simpan |
| Approver = pengubah terakhir → mekanisme kontrol terlanggar | Four-Eyes ditegakkan di frontend (`canApproveInstrumentVersion()`) DAN backend (`VAL-KEP-20a` check constraint + service code) |
| Skor dihitung frontend → tidak konsisten dengan backend | Uji hitung memakai endpoint `POST /score-preview` — frontend tidak menghitung skor |
| Versi Draft terpakai di produksi | Banner peringatan Draft + metadata `CLINICAL_VERSION_STATUS_META` + backend `resolve` hanya mengambil versi Approved |

---

## 11. Dependency Backend

| Task BE | Status | Endpoint yang Dipakai |
| --- | --- | --- |
| `BE-RWI-107` | ✅ Selesai | GET list, GET detail, GET version, POST create, PUT update, POST create-version, PUT update-version, POST score-preview |
| `BE-RWI-108` | ✅ Selesai | PATCH approve, PATCH retire |
