# Laporan Perubahan Frontend — `RJ-REG-REV-FE-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-008` |
| Judul | Data Kunjungan tanpa Keluhan Utama + redesign Data Rujukan |
| Requirement | `RJ-REG-REV-011` (REV-11), `RJ-REG-REV-012` (REV-12) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-003`; kontrak `RJ-DOC-REFERRAL-001@1.0.0` tidak berubah |
| Dependency | — |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

**REV-11.** Langkah 2 *Data Kunjungan* Pendaftaran RJ tidak lagi menampilkan Keluhan Utama; baris Keluhan Utama
di langkah Verifikasi juga dihapus. Payload mengirim `chiefComplaint: null` (tidak ada nilai palsu); backend
memang opsional. Kolom database, data historis, skrining perawat/dokter, SOAP, dan IGD tidak disentuh.

**REV-12.** Step Data Rujukan disusun ulang menjadi empat kelompok berjudul dan berketerangan:
A. **Sumber Rujukan** (fasilitas perujuk + alert mitra, dokter perujuk) · B. **Surat Rujukan** (No. rujukan,
tanggal & jam, berkas surat) · C. **Informasi Klinis** (diagnosa ICD-10, alasan rujukan, catatan diagnosa
berdampingan) · D. **Unit Tujuan & Jadwal** (unit tujuan, unit Laboratorium bersyarat, alert Radiologi/Lab,
kartu jadwal dokter bersyarat). Nama field, aturan wajib, dependency, dan payload sama persis dengan
`RJ-DOC-REV-FE-019`.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/outpatient-registration/outpatient-visit-step.jsx` | Field Keluhan Utama dihapus |
| `src/components/view/…/outpatient-registration/outpatient-verification-step.jsx` | Baris Keluhan Utama dihapus |
| `src/components/view/…/outpatient-registration/outpatient-referral-step.jsx` | Susunan A–D, `ReferralSectionHeader` |
| `src/style/…/outpatient-registration.module.css` | `.referralSectionHeader` |

## 3. Keputusan base component

`UI GATE: 1 elemen — COMPOSE 1, NEW 0` — field existing (`EmergencySelectField`, `EmergencyTextField`,
`FilterDatePicker`, `FilterTimePicker`) disusun ulang; judul kelompok memakai kelas judul existing.

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| U12 Keluhan Utama tidak tampil di langkah 2 | lulus | `PASS` |
| N10 Verifikasi tanpa Keluhan Utama | lulus | `PASS` |
| N11 pendaftaran selesai, `chiefComplaint=null` | lulus | `PASS` |
| N8 kelompok A–D tampil | lulus | `PASS` |
| N9 tablet 820 px tanpa scroll horizontal | lulus | `PASS` |
| R1–R3 isi seluruh kelompok A–D, unggah surat, selesaikan pendaftaran rujukan | 4/4 | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Keluhan Utama tidak tampil di langkah 2 RJ | Terpenuhi | U12 |
| 2. Pendaftaran tetap dapat diselesaikan | Terpenuhi | N11 |
| 3. IGD, skrining, SOAP tidak berubah | Terpenuhi lewat diff (berkas tersebut tidak disentuh) | `git status` |
| 4. Field wajib dan dependency rujukan tetap; payload sama | Terpenuhi lewat kode (field/rules tidak berubah) | diff |
| 5. Rujukan tersimpan dan terbaca ulang | Terpenuhi | `ui_ref.cjs` R1–R3: kunjungan rujukan `POST 200`, surat `200`, layar "Data rujukan tersimpan"; `GET …/{id}/referral` → `isComplete=true`, 1 surat, alasan sesuai. Kunjungan uji dibatalkan |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran |
