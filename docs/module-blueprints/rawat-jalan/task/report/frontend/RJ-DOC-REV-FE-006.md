# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-006` |
| Judul | Surat Dokter |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 6a–6d |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.3` |
| Trace | `RJ-DOC-DEC-007`; `RJ-DOC-REV-BE-004` |
| Contract version | `doctor-certificates` (`RJ-DOC-REV-BE-004`) |
| Wewenang UI | `DEV_DISCRETION` dalam batas base component dan design token; tidak ada komponen `NEW` |
| Dependency | `RJ-DOC-REV-BE-004` ✅ |
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

- Endpoint surat dokter tidak ada di backend (ditutup `RJ-DOC-REV-BE-004`).
- `doctor-queue-view.jsx` merender `DoctorCertificateTab` **tanpa** `resolveConsultationContext` dan `onRegisterBeforeFinalize`, sehingga surat tidak pernah ikut tersimpan saat Selesai Konsultasi.
- Diagnosa diketik manual; tidak ada riwayat; tujuan rujukan teks bebas; label "Dokter Pemeriksa".

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka Surat Dokter. Diagnosa (surat sakit) dan Diagnosis/Indikasi Rujukan terisi dari **Assessment SOAP terakhir** bila masih kosong.
2. Surat rujukan: *Unit / Tujuan Rujukan* dipilih dari daftar unit layanan dan klinik yang dapat dicari.
3. Label dokter menjadi **Dokter Penanggung Jawab** pada form dan cetakan.
4. Dokter menekan *Preview Surat*. Saat Selesai Konsultasi, surat disimpan; nomor resmi (`SKS-000004`, dst.) dari backend tampil di field Nomor Surat (read-only).
5. Bagian **Riwayat Surat Dokter** menampilkan semua surat pasien, termasuk yang dibatalkan, terbaru di atas.
6. Bila antrean sudah punya surat terbit, surat itu dilanjutkan (diubah), bukan dibuat ganda.

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../tabs/certificate/doctor-certificate-tab.jsx` | Pilihan tujuan rujukan (`FilterSelect` server-side), riwayat (`DataTable`), label Dokter Penanggung Jawab, nomor read-only |
| `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-certificate.js` | Muat Assessment SOAP dan surat aktif, riwayat, pencarian tujuan rujukan |
| `src/lib/services/health-services/registration-management/doctor-queue.service.js` | `getDoctorCertificateHistory`, `getDoctorCertificateReferralTargets` |
| `src/utils/health-services/registration-management/doctor-queue/doctor-certificate.utils.js` | `buildReferralTargetKey`, `parseReferralTargetKey`; payload membawa id master |
| `src/lib/constants/.../doctor-certificate.constants.js` | Field `targetServiceUnitKey` |
| `.../doctor-queues/doctor-queue-view.jsx` | Tab surat menerima `resolveConsultationContext` dan `onRegisterBeforeFinalize` |
| `tests/unit/doctor-certificate-referral.test.mjs` (baru) | 2 test |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → hook → service → InstanceAxios`. View tidak memanggil Axios langsung; normalisasi ada di `utils`. Tidak ada state global, provider, atau abstraksi HTTP baru.

```
UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Status |
| --- | --- | --- |
| Tujuan rujukan dapat dicari | `FilterSelect` (`serverSide`, `onSearchChange`) | REUSE |
| Tabel riwayat | `DataTable` (`pagination={false}`) | REUSE |
| Diagnosa otomatis | `BaseTextField` | REUSE |
| Label dokter | `SelectField` lokal | REUSE |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Riwayat memuat | "Memuat riwayat surat dokter..." |
| Riwayat kosong | "Belum ada surat dokter untuk pasien ini." |
| Riwayat gagal | Pesan galat di tempat judul kosong |
| Simpan gagal | Pesan merah di footer; finalisasi konsultasi tidak terblokir |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Doctor Certificate

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/doctor-certificates?patientId=` | Riwayat | `DoctorCertificate : Read` |
| `GET` | `/doctor-certificates/referral-targets?search=` | Pilihan tujuan rujukan | `DoctorCertificate : Read` |
| `GET` | `/doctor-certificates/active-by-queue/{queueId}` | Lanjutkan surat aktif | `DoctorCertificate : Read` |
| `POST` / `PUT` | `/doctor-certificates`, `/{id}` | Simpan | `DoctorCertificate : Create/Update` |

---

## 6. Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint <berkas task>` | `0` error; peringatan yang muncul berasal dari pola lama di berkas yang sama (`react-hooks/exhaustive-deps`, `set-state-in-effect`) | `PASS` |
| `node --test tests/unit/doctor-certificate-referral.test.mjs` | `2/2` lulus | `PASS` |
| `npm run test:unit` (seluruh suite) | `2175` test: `2169` lulus, `6` gagal. Keenamnya di `hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation` — tidak satu pun membaca berkas task ini | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | `✓ Compiled successfully in 52s`, postbuild standalone siap, exit `0` | `PASS` |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/doctor-certificate-referral.test.mjs — PASS

MANUAL TEST: NOT FEASIBLE — sesi agen tidak memiliki peramban untuk mengklik layar. Sebagai gantinya, kontrak data yang dikonsumsi layar diuji lewat HTTP sungguhan ke aplikasi backend scratchpad (`localhost:5217`, `QuilvianNewDevSukma`): seluruh endpoint di bagian 5 diuji pada `RJ-DOC-REV-BE-004` R0–R13. Pemilik perlu mengklik ulang alur pada bagian 2 di lingkungan dev.

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
| a. Diagnosa dari Assessment SOAP terakhir | Terpenuhi | `use-doctor-certificate.js` |
| b. Riwayat surat dokter | Terpenuhi | `CertificateHistory`; backend R9 |
| c. Unit/Tujuan Rujukan berupa pilihan yang dapat dicari | Terpenuhi | Unit test; backend R1, R5 |
| d. Dokter Pemeriksa → Dokter Penanggung Jawab | Terpenuhi | Form dan cetakan; backend menyimpan DPJP (R2) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Hak akses `DoctorCertificate` sudah diberikan di DB Sukma lewat `grant-doctor-certificate-access.sql`; lingkungan lain perlu menjalankannya juga |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `git status --short` frontend pada laporan ringkas sesi; seluruh perubahan belum di-commit |
