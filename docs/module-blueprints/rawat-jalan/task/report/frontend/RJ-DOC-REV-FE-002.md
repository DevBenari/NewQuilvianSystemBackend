# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-002` |
| Judul | Hasil Skrining dokter dan header pasien |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 1a–1d |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.3` |
| Trace | `RJ-DOC-DEC-007`; `RJ-DOC-REV-BE-001`, `RJ-DOC-REV-BE-002` |
| Contract version | Field identitas `RJ-DOC-REV-BE-001` (non-breaking) |
| Wewenang UI | `DEV_DISCRETION` dalam batas base component dan design token; tidak ada komponen `NEW` |
| Dependency | `RJ-DOC-REV-BE-001` ✅, `RJ-DOC-REV-BE-002` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` (+ laporan lintas repository di blueprint backend) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7969f959` cabang `sukmagpV2` (worktree bersih saat mulai) |
| Commit backend yang dijadikan rujukan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001..006` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — verifikasi klik manual oleh pemilik masih diperlukan |

---

## 1. Keadaan yang ditemukan di awal

- Kolom *Risiko Jatuh* menampilkan angka enum `fallRiskStatus` (`1` = Tidak berisiko), sehingga terbaca "skor 1" (`RJ-DOC-REV-BE-002`).
- `formatDateTime` menghasilkan `01Okt2026, 09:30` tanpa spasi.
- Header pasien hanya berisi No. RM, nama, jenis pembayaran, dan total kunjungan.

## 2. Proses bisnis dari sisi pengguna

1. Dokter memanggil pasien. Header menampilkan No. RM, nama, **jenis kelamin**, **umur**, **Asuransi/ Penjamin** (penjamin utama), **alergi** (merah bila ada), **Foto KTP/Asuransi**, dan total kunjungan.
2. Foto KTP/kartu tampil sebagai tautan bila berkasnya tersimpan di server. Hasil scan KIOS-K yang masih berupa path lokal mesin kiosk ditulis `KTP: belum terunggah`.
3. Tab Hasil Skrining: tanggal `01 Okt 2026, 09:30`; Risiko Jatuh `Skor 0 · Tidak berisiko` bila tidak dipilih.

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/doctor-queue-features/DoctorPatientContext.jsx` | Header: jenis kelamin, umur, alergi, foto KTP/kartu, label `Asuransi/ Penjamin` |
| `src/utils/health-services/registration-management/doctor-queue/patient-info.utils.js` | `getPatientGenderLabel`, `getPatientAllergyText`, `hasPatientAllergy`, `getPatientDocumentLinks` |
| `src/utils/health-services/registration-management/doctor-queue/doctor-queue-display-utils.js` | `formatDateTime` berspasi |
| `src/utils/health-services/registration-management/doctor-queue/screening-history.utils.js` | `getFallRiskDisplay` |
| `src/components/features/health-services/doctor-queue-features/AssessmentHistoryTable.jsx` | Kolom Risiko Jatuh memakai skor dan label |
| `src/style/health-services/registration-management/doctor-queues/doctor-queue-view.module.css` | `.contextAllergyAlert` (`var(--color-danger)`), gaya tautan |
| `tests/unit/doctor-screening-display.test.mjs` (baru) | 5 test |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → hook → service → InstanceAxios`. View tidak memanggil Axios langsung; normalisasi ada di `utils`. Tidak ada state global, provider, atau abstraksi HTTP baru.

```
UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Status |
| --- | --- | --- |
| Item header baru | sel `contextInfoGrid` yang sudah ada | REUSE |
| Kolom risiko jatuh | tabel riwayat asesmen yang sudah ada | REUSE |
| Tautan dokumen | `resolvePublicFileUrl` (`utils/shared`) | REUSE |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Data belum ada | `-` |
| Path dokumen tidak dapat dibuka | `KTP: belum terunggah` |
| Alergi | `Tidak ada`, atau daftar alergi berwarna bahaya |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/doctor-queues` | Field identitas dan penjamin | `DoctorQueue : Read` |

---

## 6. Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint <berkas task>` | `0` error; peringatan yang muncul berasal dari pola lama di berkas yang sama (`react-hooks/exhaustive-deps`, `set-state-in-effect`) | `PASS` |
| `node --test tests/unit/doctor-screening-display.test.mjs` | `5/5` lulus | `PASS` |
| `npm run test:unit` (seluruh suite) | `2175` test: `2169` lulus, `6` gagal. Keenamnya di `hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation` — tidak satu pun membaca berkas task ini | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | `✓ Compiled successfully in 52s`, postbuild standalone siap, exit `0` | `PASS` |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/doctor-screening-display.test.mjs — PASS

MANUAL TEST: NOT FEASIBLE — sesi agen tidak memiliki peramban untuk mengklik layar. Sebagai gantinya, kontrak data yang dikonsumsi layar diuji lewat HTTP sungguhan ke aplikasi backend scratchpad (`localhost:5217`, `QuilvianNewDevSukma`): antrean dokter `G001` mengembalikan `Perempuan`, `31 tahun 4 bulan 1 hari`, alergi uji, dan path dokumen. Pemilik perlu mengklik ulang alur pada bagian 2 di lingkungan dev.

### Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Warna/spacing/typography baru memakai token (`var(--color-*)`, `var(--space-*)`, `var(--font-size-*)`) | Ya — `var(--color-danger)`; tidak ada nilai literal baru |
| Tidak ada `<button>`/`<table>`/input mentah baru | Ya — memakai `BaseButton`, `DataTable`, `FilterSelect`, `ResourceFilterSelect`, `BaseTextField` |
| Typography komponen shared tidak di-override | Ya |
| State memuat/kosong/gagal tersedia | Ya — lihat bagian 4 |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Risiko jatuh tidak dipilih tampil `0`/`-` | Terpenuhi | Unit test |
| 2. Tanggal skrining berspasi | Terpenuhi | Unit test `01 Okt 2026, 09:05` |
| 3. Header menampilkan jenis kelamin, umur, alergi, foto KTP/kartu | Terpenuhi | `DoctorPatientContext.jsx`; data `RJ-DOC-REV-BE-001` R2–R3 |
| 4. Label `Asuransi/ Penjamin` | Terpenuhi | `DoctorPatientContext.jsx` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Seluruh dokumen identitas di DB dev berupa path lokal mesin kiosk; tautan KTP belum dapat terbuka sampai KIOS-K mengunggah berkasnya ke server |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `git status --short` frontend pada laporan ringkas sesi; seluruh perubahan belum di-commit |
