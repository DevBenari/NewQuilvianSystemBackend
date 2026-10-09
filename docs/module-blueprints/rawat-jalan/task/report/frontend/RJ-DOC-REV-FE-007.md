# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-007` |
| Judul | Penunjang Medis — order Lab dan Radiologi dari dokter |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 7 |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.3` |
| Trace | `RJ-DOC-DEC-007`, `RJ-DOC-DEC-008` (`RJ-DOC-OQ-003` dibuka ulang); `RJ-DOC-DEC-004` |
| Contract version | `LabOrder` `POST /by-examinations`, `RadOrder` `POST /` (sudah ada, tidak berubah) |
| Wewenang UI | `DEV_DISCRETION` dalam batas base component dan design token; tidak ada komponen `NEW` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — 3 berkas baru, kontrak yang sudah ada |
| Task mode | `FRONTEND` (+ laporan lintas repository di blueprint backend) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7969f959` cabang `sukmagpV2` (worktree bersih saat mulai) |
| Commit backend yang dijadikan rujukan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001..006` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — verifikasi klik manual oleh pemilik masih diperlukan |

---

## 1. Keadaan yang ditemukan di awal

Tab *Penunjang Medis* masih `WorkInProgressTab`. Endpoint order Lab dan Radiologi sudah ada dan dipakai layar modul Lab/Radiologi, tetapi dokter harus pindah modul dan memilih ulang kunjungan.

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab *Penunjang Medis* pada pasien aktif. Kunjungan otomatis kunjungan pasien itu.
2. **Laboratorium**: pilih pemeriksaan dari katalog Lab (harga tampil), lalu klik *Order Laboratorium*. Backend memecah pesanan per disiplin. Daftar order Lab kunjungan ini tampil di bawahnya.
3. **Radiologi**: pilih alat pencitraan, pemeriksaan radiologi (dapat dicari), isi indikasi klinis, centang Cito bila perlu, lalu klik *Order Radiologi*. Daftar order Radiologi tampil.
4. Pengerjaan dan hasil tetap dikerjakan modul Lab/Radiologi. Konsultasi boleh selesai walau order belum dikerjakan (`RJ-DOC-DEC-004`).

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../doctor-queues/tabs/supporting/doctor-supporting-exam-tab.jsx` (baru) | Tab order penunjang |
| `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-supporting-orders.js` (baru) | Muat dan buat order Lab/Radiologi per kunjungan |
| `.../doctor-queues/doctor-queue-view.jsx` | Render tab `supportingExam` |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → hook → service → InstanceAxios`. View tidak memanggil Axios langsung; normalisasi ada di `utils`. Tidak ada state global, provider, atau abstraksi HTTP baru.

```
UI GATE: 6 elemen — REUSE 5, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Status |
| --- | --- | --- |
| Pemilih pemeriksaan Lab | `LabCatalogPicker` (modul Lab) | REUSE |
| Alat pencitraan | `FilterSelect` | REUSE |
| Pemeriksaan radiologi | `ResourceFilterSelect` + `useSelectResource("procedures")` | REUSE |
| Indikasi dan cito | `BaseTextAreaField`, `BaseCheckboxField` | REUSE |
| Daftar order | `DataTable` | REUSE |
| Panel tab | dirangkai dari panel `rad-order.module.css` dan komponen di atas | COMPOSE — tanpa komponen baru |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat order laboratorium/radiologi..." |
| Kosong | "Belum ada order … pada kunjungan ini." |
| Validasi | "Alat pencitraan wajib dipilih.", "Pemeriksaan radiologi wajib dipilih.", "Pilih sekurang-kurangnya satu pemeriksaan laboratorium." |
| Gagal simpan | Pesan merah `InformationAlert` dari backend |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `POST` | `/laboratory-management/lab-orders/by-examinations` | Buat order Lab |
| `GET` | `/laboratory-management/lab-orders?encounterId=` | Daftar order Lab |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `POST` | `/radiology-management/rad-orders` | Buat order Radiologi |
| `GET` | `/radiology-management/rad-orders?encounterId=` | Daftar order Radiologi |
| `GET` | `/radiology-management/master-data/rad-modalities/options` | Alat pencitraan |

---

## 6. Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint <berkas task>` | `0` error; peringatan yang muncul berasal dari pola lama di berkas yang sama (`react-hooks/exhaustive-deps`, `set-state-in-effect`) | `PASS` |
| `npm run test:unit` (seluruh suite) | `2175` test: `2169` lulus, `6` gagal. Keenamnya di `hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation` — tidak satu pun membaca berkas task ini | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | `✓ Compiled successfully in 52s`, postbuild standalone siap, exit `0` | `PASS` |

AUTOMATED TEST: SKIPPED (opsional) — perubahan berupa komposisi view dan wiring hook.

MANUAL TEST: NOT FEASIBLE — sesi agen tidak memiliki peramban untuk mengklik layar. Sebagai gantinya, kontrak data yang dikonsumsi layar diuji lewat HTTP sungguhan ke aplikasi backend scratchpad (`localhost:5217`, `QuilvianNewDevSukma`): R1 `POST /lab-orders/by-examinations` `201`; R2 `POST /rad-orders` `201`; R3 daftar Lab per kunjungan `LAB-RSMMC-000008 Requested`; R4 daftar Radiologi `THORAX / Computed Radiography / Requested`. Pemilik perlu mengklik ulang alur pada bagian 2 di lingkungan dev.

### Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Warna/spacing/typography baru memakai token (`var(--color-*)`, `var(--space-*)`, `var(--font-size-*)`) | Tidak ada CSS baru |
| Tidak ada `<button>`/`<table>`/input mentah baru | Ya — memakai `BaseButton`, `DataTable`, `FilterSelect`, `ResourceFilterSelect`, `BaseTextField` |
| Typography komponen shared tidak di-override | Ya |
| State memuat/kosong/gagal tersedia | Ya — lihat bagian 4 |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Dokter dapat membuat dan melihat order Lab dan Radiologi kunjungan ini memakai endpoint yang sudah ada | Terpenuhi | Runtime R1–R4 (bagian 6) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Posisi dokter perlu hak akses `LabOrder : Create/Read`, `RadOrder : Create/Read`, `LabCatalog : Read`, `RadModality : Read` di Akses Role |
| Data uji | Order Lab `LAB-RSMMC-000008` (*A1 GAMBARAN DARAH TEPI*) dan Radiologi *THORAX* (indikasi `TEST-REVFE007`) pada kunjungan antrean `G002` |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `git status --short` frontend pada laporan ringkas sesi; seluruh perubahan belum di-commit |
