# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-001` |
| Judul | Skrining Pasien (perawat) |
| Slice | Revisi UAT `2026-09-28` — Skrining Pasien butir 1–3 |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.3` |
| Trace | `RJ-DOC-DEC-007`; `RJ-DOC-REV-BE-001` |
| Contract version | Field penjamin `RJ-DOC-REV-BE-001` (penambahan, non-breaking) |
| Wewenang UI | `DEV_DISCRETION` dalam batas base component dan design token; tidak ada komponen `NEW` |
| Dependency | `RJ-DOC-REV-BE-001` ✅ |
| Klasifikasi | `MEDIUM` — 1 layar, 5 berkas, logika validasi sedang |
| Task mode | `FRONTEND` (+ laporan lintas repository di blueprint backend) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7969f959` cabang `sukmagpV2` (worktree bersih saat mulai) |
| Commit backend yang dijadikan rujukan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001..006` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — verifikasi klik manual oleh pemilik masih diperlukan |

---

## 1. Keadaan yang ditemukan di awal

- Tab *Informasi Pasien* sudah membaca `primaryGuarantorNameSnapshot`, `primaryGuarantorTypeSnapshot`, `isInsurancePatient`, `isCompanyPatient`, tetapi backend tidak mengirimnya, sehingga tampil kosong atau `Tidak`. Ditutup di `RJ-DOC-REV-BE-001`.
- Baris Pembayaran Campuran, Eligibility Diperlukan, Eligibility Selesai, dan No. Eligibility tampil padahal baru diproses di kasir.
- Isian tanda vital di luar rentang wajar (mis. suhu `60`) baru ditolak backend saat Selesai Skrining, dengan **satu pesan global** di atas layar (`PatientVitalSignController.ValidateMeasurementValuesCore`). FE hanya punya validasi "wajib diisi".

## 2. Proses bisnis dari sisi pengguna

1. Perawat membuka pasien di Nurse Station, tab *Informasi Pasien*: penjamin utama, jenis penjamin, dan status pasien asuransi/perusahaan tampil sesuai pendaftaran KIOS-K. Eligibility dan pembayaran campuran tidak lagi tampil.
2. Tab *Tanda Vital*: field yang di luar rentang langsung diberi pesan di bawah field itu. Contoh: suhu `60` → *"Suhu tubuh harus 25–45 °C."*; diastolik `95` dengan sistolik `90` → *"Diastolik harus lebih kecil dari sistolik."*
3. Tombol Selesai Skrining tetap tertahan dan tab Tanda Vital ditandai tidak valid sampai semua field benar.

Rentang sama persis dengan backend: sistolik 40–300, diastolik 20–200, nadi 20–250, napas 5–80, suhu 25–45, SpO₂ 0–100, berat dan tinggi > 0, lingkar kepala 1–100, GCS E 1–4 / V 1–5 / M 1–6, aliran O₂ ≥ 0.

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/registration-management/nurse-station-management/screening-validation.utils.js` | `VITAL_RANGE_RULES`, `getVitalRangeErrors`, `getRangeFieldProps`; validasi selesai ikut memperhitungkan rentang; pesan rentang per field |
| `src/components/features/health-services/nurse-station-management/VitalSignTab.jsx` | Field opsional (BB, TB, lingkar kepala, aliran O₂, GCS) memakai `getRangeFieldProps` |
| `src/lib/hooks/health-services/registration-management/nurse-station-management-queue/useScreeningValidation.js` | Alasan tombol Selesai membedakan "belum lengkap" dan "di luar rentang" |
| `src/components/features/health-services/nurse-station-management/PatientInformationTab.jsx` | Hapus empat baris Pembayaran Campuran dan Eligibility |
| `tests/unit/nurse-screening-vital-range.test.mjs` (baru) | 5 test aturan rentang |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → hook → service → InstanceAxios`. View tidak memanggil Axios langsung; normalisasi ada di `utils`. Tidak ada state global, provider, atau abstraksi HTTP baru.

```
UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Status |
| --- | --- | --- |
| Pesan rentang per field | `BaseTextField` props `invalid`/`helperText` | REUSE |
| Baris info pembayaran | `PatientInfoRow` lokal | REUSE |
| Penanda tab tidak valid | `ScreeningTabs` `vitalValid` | REUSE |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Isian di luar rentang | Field merah dan pesan rentang di bawahnya |
| Field wajib kosong | "Wajib diisi sebelum selesai skrining." |
| Data penjamin belum ada | `-` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Nurse Station Queue

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/nurse-station-queues` | Field penjamin utama | `NurseStationQueue : Read` |

---

## 6. Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint <berkas task>` | `0` error; peringatan yang muncul berasal dari pola lama di berkas yang sama (`react-hooks/exhaustive-deps`, `set-state-in-effect`) | `PASS` |
| `node --test tests/unit/nurse-screening-vital-range.test.mjs` | `5/5` lulus | `PASS` |
| `npm run test:unit` (seluruh suite) | `2175` test: `2169` lulus, `6` gagal. Keenamnya di `hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation` — tidak satu pun membaca berkas task ini | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | `✓ Compiled successfully in 52s`, postbuild standalone siap, exit `0` | `PASS` |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/nurse-screening-vital-range.test.mjs — PASS

MANUAL TEST: NOT FEASIBLE — sesi agen tidak memiliki peramban untuk mengklik layar. Sebagai gantinya, kontrak data yang dikonsumsi layar diuji lewat HTTP sungguhan ke aplikasi backend scratchpad (`localhost:5217`, `QuilvianNewDevSukma`): antrean perawat `I001` `2026-08-24` mengembalikan `Prudential Indonesia` / `Asuransi` / `isInsurancePatient = true`. Pemilik perlu mengklik ulang alur pada bagian 2 di lingkungan dev.

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
| 1. Penjamin utama, jenis penjamin, pasien asuransi/perusahaan tampil benar | Terpenuhi | `RJ-DOC-REV-BE-001` R1 (data); layar membaca field yang sama |
| 2. Baris Pembayaran Campuran dan Eligibility dihapus | Terpenuhi | `PatientInformationTab.jsx` |
| 3. Alert isian tidak sesuai tampil di field masing-masing | Terpenuhi | Unit test 5/5; `VitalSignTab.jsx` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Bila backend mengubah rentang, `VITAL_RANGE_RULES` perlu ikut diubah |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `git status --short` frontend pada laporan ringkas sesi; seluruh perubahan belum di-commit |
