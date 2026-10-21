# Laporan Task Frontend — `RJ-REF-REV-FE-001`..`003` (Zoom 13 Okt 2026)

Definisi task: [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian 8.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Tanggal | 16 Oktober 2026 |
| Branch / baseline | `sukmagpV2` @ `321f987a`, working tree belum di-commit |
| Requirement | `RJ-REF-REV-001`..`005` |
| Status | `FE-001` 🟡, `FE-002` 🟡, `FE-003` 🟡 — implementasi, lint, build, unit selesai; uji layar `NOT RUN` |

## 2. Perubahan

| Task | Berkas | Isi |
| --- | --- | --- |
| `FE-001` | `outpatient-referral-step.jsx` | Kartu Sumber Rujukan; picker Surat Rujukan internal (memuat/galat/kosong/coba lagi); eksternal tanpa Informasi Klinis, nomor & berkas opsional |
| `FE-001` | `outpatient-referral.utils.js` | `isInternalReferralSource`, `validateOutpatientReferral` per sumber, payload create internal/eksternal, PUT Lab tanpa diagnosa |
| `FE-001` | `outpatient-registration.constants.js`, `outpatient-registration.service.js` | `REFERRAL_SOURCE_TYPE/OPTIONS`, pesan baru, endpoint + `fetchInternalReferralLetters` |
| `FE-001` | `use-outpatient-registration.js`, `outpatient-registration-page.jsx`, `outpatient-verification-step.jsx` | `handleClearReferralFiles`, `patientId` ke step, unggah hanya bila ada berkas, ringkasan verifikasi per sumber |
| `FE-002` | `doctor-certificate-tab.jsx`, `use-doctor-certificate.js`, `doctor-certificate.utils.js`, `doctor-certificate.constants.js`, `doctor-certificate.module.css` | Tujuan poliklinik saja; hapus Tanggal Rujukan & Kelas; tombol *Terbitkan Surat Rujukan*; muat surat terbit & kunci form; preview draft bercap *DRAFT — BELUM TERBIT*, Cetak nonaktif; `CertificatePreview`/`PreviewModal` diekspor untuk dipakai ulang |
| `FE-003` | `outpatient-encounter-referral-letters.jsx` (baru), `outpatient-encounter-detail-view.jsx`, `outpatient-encounter-detail-utils.jsx`, `outpatient-encounter.service.js` | Kelompok *Rujukan Masuk*; tabel *Surat Rujukan* + *Preview & Cetak* dari endpoint bercakupan |
| — | `tests/unit/outpatient-referral-revision.test.mjs` (baru) | 12 uji fungsi murni |

Keamanan: semua teks lewat `sanitizeDisplayText`/JSX (tanpa HTML mentah); surat resmi selalu diambil dari
backend, bukan dari draft lokal; berkas tetap divalidasi format/ukuran di layar dan ulang di backend.

## 3. Validasi

| Langkah | Hasil |
| --- | --- |
| ESLint berkas tersentuh | 0 error; warning sama dengan `HEAD` (tab 1, hook 5 — pola lama) |
| `npm run build` | Sukses. Catatan: build pertama gagal karena `@fontsource/barlow-semi-condensed` dan `react-zoom-pan-pinch` (modul floor-plan Rawat Inap) sudah ada di `package.json` tetapi belum terpasang; dipasang `--no-save`, `yarn.lock` dikembalikan ke `HEAD` |
| Unit `outpatient-referral-revision` + `doctor-certificate-referral` | 14/14 PASS |
| Unit seluruh suite | 2960/2987; 27 gagal di modul lain (Rawat Inap, Hemodialisa, Bank Darah, CRM, parity), tidak menyentuh berkas task ini |
| Uji layar Playwright | `NOT RUN` |

## 4. Belum terbukti

- Alur layar internal/eksternal, kunci form setelah terbit, cetak kertas surat (UAT).
- Tampilan pada akun non-SuperAdmin.
