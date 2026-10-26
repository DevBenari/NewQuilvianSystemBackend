# Laporan Task `RJ-REG-REV-FE-011` — General Consent seperti RI, khusus pasien baru

| Field | Isi |
| --- | --- |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian 9.2 |
| Requirement | `RJ-REG-REV-015` |
| Keputusan | `RJ-REG-DEC-012` (amendemen 23 Okt 2026), wewenang `RJ-REG-DEC-001` |
| Dependency | `RJ-REG-REV-BE-007` ✅ |
| Kontrak | `PUT`/`GET …/patient-encounters/{encounterId}/general-consent` sesuai [laporan BE-007](../backend/RJ-REG-REV-BE-007.md) |
| Branch frontend | `sukmagpV2` (upstream `origin/sukmagpV2`), belum di-commit |
| Tanggal | 23 Oktober 2026 |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Status | ✅ `TESTED` / `UAT_PENDING` |

## 1. Hasil untuk pengguna

Step General Consent di Pendaftaran Rawat Jalan kini memakai susunan General Consent Rawat Inap:

1. **Pihak Penanda Tangan / Penanggung Jawab**: Pasien Sendiri, Orang Tua, Wali yang Sah, Suami / Istri, atau Keluarga Lainnya. Isiannya Nama Lengkap, NIK/Paspor, Hubungan, No. HP (angka, maks 13), dan Alamat. *Pasien Sendiri* terisi otomatis dari data pasien baru. Jenis lain mengosongkan nama dan NIK supaya identitas pasien tidak terbawa.
2. **Pernyataan Persetujuan Umum (12 Butir Klausul)**: teks bilingual RS MMC, semua tercentang lebih dulu, lencana *n dari 12 Disetujui*, dan tombol *Pilih Semua / Batalkan Semua*. Butir 1–11 sama persis dengan Rawat Inap. Butir 12 menyebut *pasien rawat jalan* dan *Brosur Panduan Rawat Jalan*.
3. **Tanda Tangan**: bagian tanda tangan digital Rawat Jalan yang lama tetap, yaitu kanvas dan tombol *Clear*.

Isian lama versi demo (Hubungan dropdown, Jenis Pasien, Poliklinik, Penerimaan Panduan, Keterangan) dihapus.

Step ini **hanya muncul pada pendaftaran pasien baru**:

| Pasien | Urutan step (Umum) |
| --- | --- |
| Baru | Cari/Input Pasien → Jenis Kunjungan → Data Kunjungan → Metode Pembayaran → **General Consent** → Verifikasi & Konfirmasi → Selesai |
| Lama | Cari/Input Pasien → Jenis Kunjungan → Data Kunjungan → Metode Pembayaran → Verifikasi & Konfirmasi → Selesai |

Pada pasien lama, tombol Pembayaran berbunyi *Lanjut ke Verifikasi*, dan *Kembali* dari Verifikasi membuka Pembayaran. Jenis kunjungan Rujukan mengikuti aturan yang sama.

Consent kini **disimpan ke backend**. Sesudah kunjungan terbentuk (poliklinik maupun rujukan Laboratorium), layar mengirim isian ke `BE-007`. Layar Selesai lalu menampilkan:

- *Menyimpan General Consent...*
- *General Consent tersimpan*
- *General Consent belum tersimpan* dengan tombol **Coba lagi**. Kunjungan tetap terdaftar, dan Coba lagi mengirim isian yang sama.

## 2. Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../outpatient-registration.constants.js` | URL `encounterGeneralConsent`; `OUTPATIENT_CONSENT_SIGNER_TYPE(S)` (dari `ADMISSION_CONSENT_SIGNER_TYPES` RI); `OUTPATIENT_CONSENT_ITEMS` (dari `INPATIENT_MMC_CONSENT_ITEMS`, butir 12 diganti); nilai bawaan form consent baru; `CONSENT_FOLLOW_UP_STATUS`. Opsi demo lama dihapus |
| `src/utils/.../outpatient-general-consent.utils.js` | Baru. Fungsi murni: prefill penanda tangan, toggle klausul, validasi, payload `BE-007` |
| `src/utils/.../outpatient-referral.utils.js` | `getOutpatientRegistrationSteps(isReferral, { includeConsent })` dan `getNextOutpatientStep`. Tanpa opsi, perilaku lama tetap |
| `src/lib/services/.../outpatient-registration.service.js` | `saveOutpatientGeneralConsent` (`PUT`) |
| `src/lib/state/slice/.../outpatient-registration-slice.jsx` | Thunk `completeOutpatientConsentFollowUp`, state `consentFollowUp` (hanya status, tanpa isian) |
| `src/lib/hooks/.../use-outpatient-registration.js` | `requiresConsent` (pasien baru); bar step tanpa consent untuk pasien lama; Pembayaran → step berikutnya pada bar; validasi consent; simpan consent sesudah kunjungan terbentuk (poli dan Lab); `handleRetryConsentFollowUp` |
| `src/components/view/.../outpatient-general-consent-step.jsx` | Ditulis ulang: penanda tangan, 12 klausul, tanda tangan RJ lama |
| `src/components/view/.../outpatient-registration-page.jsx` | Label tombol Pembayaran dinamis; step consent hanya pasien baru; `ConsentFollowUpNotice` di layar Selesai |
| `src/style/.../outpatient-registration.module.css` | `.consentFieldFull`, `.consentSignerGrid`, `.consentClauseHeader`, `.consentClauseActions`, `.consentClauseGrid`, `.consentClauseSpanFull` (token `--space-*`) |

Delta kontrak: tidak ada terhadap `BE-007`. Waktu tanda tangan dan petugas tidak dikirim klien.

## 3. Keputusan base component

Modul referensi visual: langkah Formulir Persetujuan Admisi Rawat Inap (`inpatient-admission-consent-form-step.jsx`) untuk isi, dan step Pendaftaran RJ/IGD (`stepUnifiedCard`) untuk kerangka.

`UI GATE: 9 elemen — REUSE 7, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kerangka step, judul section, tombol Kembali/Lanjut | Kelas step RJ/IGD | `stepUnifiedCard`, `consentSection` yang sudah ada | REUSE | Tetap |
| Pilihan jenis penanda tangan | `BaseCheckboxCard` pola radio | Dipakai `inpatient-admission-visit-category-step`, `journal-reversal-dialog` | REUSE | `variant="compact"`, `role="radio"` |
| Isian penanda tangan | `EmergencyTextField` → `BaseTextField` | `numberOnly` + `maxLength` seperti No. HP pasien baru | REUSE | Tetap |
| Kartu klausul | `BaseCheckboxCard` | Props `title`, `description`, `bullets` | REUSE | Butir 3 dan 12 selebar baris, sama dengan RI |
| Lencana hitungan | `StatusBadge` | Sama dengan RI | REUSE | Tetap |
| Pilih Semua, Clear | `BaseButton` | — | REUSE | Tetap |
| Peringatan dan notice | `EmergencyInlineAlert` | Sama dengan notice rujukan | REUSE | Tetap |
| Grid penanda tangan dan klausul | Kelas fitur | `outpatient-registration.module.css` | COMPOSE | Kelas baru bertoken |
| Tanda tangan digital | Kanvas RJ lama | `react-signature-canvas` | COMPOSE | Dipertahankan sesuai permintaan |

Pilihan untuk elemen COMPOSE dan tampilan klausul:

- **A. `BaseCheckboxCard` untuk penanda tangan dan klausul — Rekomendasi, dipakai.** Isi, urutan, dan perilaku sama dengan RI, dan komponennya base design system. Tidak ada risiko regresi ke modul lain. Bedanya hanya bentuk visual: kartu bercentang, bukan chip dan kartu khusus RI.
- **B. Menyalin markup RI dan memakai kelas CSS `inpatient-admission.module.css`.** Tampilan persis RI, tetapi gaya Rawat Jalan bergantung pada stylesheet modul lain. Perubahan stylesheet RI akan ikut mengubah RJ diam-diam.

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` 8 berkas tersentuh | exit 0 — `PASS` |
| `npm run lint:errors` | exit 0 — `PASS` |
| `npm run build` | exit 0, "Compiled successfully", 516/516 halaman — `PASS` |
| Uji fungsi `node --test` (skrip scratchpad, `tests/helpers/register.mjs`) | 10/10 — `PASS` |
| `npm run test:unit` | Command gagal di Node 20 karena glob `tests/unit/*.test.mjs` tidak diekspansi. Dijalankan setara dengan glob dari shell: 3405 test, 3363 pass, **42 fail**. Kedua kegagalan di berkas test yang mengimpor modul tersentuh (`FE-CRM-007`, `FE-RWI-058`) memeriksa source yang tidak diubah task ini (`new-patient-form.jsx` IGD, langkah deposit RI). Kegagalan lain berada di modul RI, HMD, CRM, Lab, dan Bank Darah — `UNRELATED EXISTING ISSUE` |
| Grep anti-regresi (warna literal, `font-*`, `!important`, `<button>`/`<table>`/inline style baru) | kosong — `PASS`. Tiga `<button>` di step consent adalah tombol Kembali/Lanjut lama, sama dengan step RJ/IGD lain |

`AUTOMATED TEST: node --test <scratchpad>/fe-unit/general-consent.test.mjs — PASS` (10/10, tidak ditambahkan ke `tests/`).

| ID | Uji fungsi | Hasil |
| --- | --- | --- |
| U1 | Pasien baru Umum: consent di antara Pembayaran dan Verifikasi | `PASS` |
| U2 | Pasien lama: tanpa consent; Pembayaran → Verifikasi; Kembali → Pembayaran | `PASS` |
| U3 | Rujukan: pasien baru dengan consent, pasien lama tanpa | `PASS` |
| U4 | Tanpa opsi = perilaku lama | `PASS` |
| U5 | Butir 1–11 identik dengan RI; butir 12 rawat jalan | `PASS` |
| U6 | Nilai bawaan: Pasien Sendiri, 12 tercentang, field demo hilang | `PASS` |
| U7 | Prefill Pasien Sendiri; jenis lain mengosongkan identitas | `PASS` |
| U8 | Validasi jenis, nama, klausul, tanda tangan | `PASS` |
| U9 | Toggle klausul tetap urut | `PASS` |
| U10 | Payload `BE-007` (trim, kosong → `undefined`, HP angka, tanpa `signedAt`) | `PASS` |

Uji layar Playwright: FE dev `localhost:3000`, API `7184` dialihkan ke backend uji `https://localhost:7185` (build `BE-007`), DB `QuilvianNewDevSukma`, akun SuperAdmin, pasien uji `UJI GC PASIEN DAA8`.

| ID | Skenario | Hasil |
| --- | --- | --- |
| N0 | Pasien baru tersimpan | `PASS` |
| C1 | Pasien baru: bar step memuat General Consent (langkah 5 dari 7) | `PASS` |
| C2 | Pasien Sendiri terpilih; nama, NIK, hubungan terisi dari pasien baru | `PASS` |
| C3 | 12 butir tampil dan tercentang; *12 dari 12 Disetujui* | `PASS` |
| C4 | Butir 12 menyebut rawat jalan, bukan rawat inap | `PASS` |
| C5 | Isian demo lama tidak tampil | `PASS` |
| C6 | Tanda tangan RJ (kanvas + Clear) tetap | `PASS` |
| C7 | Butir 5 dilepas → *11 dari 12*, lanjut ditahan | `PASS` |
| C8 | *Pilih Semua* → 12 dari 12 | `PASS` |
| C9 | Orang Tua: nama/NIK kosong, hubungan *Orang Tua* | `PASS` |
| C10 | Nama kosong ditahan | `PASS` |
| C11 | No. HP hanya angka (`0812abc3456` → `08123456`) | `PASS` |
| C12 | Isian lengkap → Verifikasi | `PASS` |
| C13 | Kembali dari Verifikasi: isian, pilihan, tanda tangan, klausul bertahan | `PASS` |
| C14 | Layar 390 px tanpa scroll horizontal | `PASS` |
| S1 | Submit → kunjungan `ENC-RSMMC-00374` terdaftar | `PASS` |
| S2 | `PUT` consent pertama digagalkan (500 tiruan) → notice + *Coba lagi*, kunjungan tetap | `PASS` |
| S3 | *Coba lagi* → `200`, *General Consent tersimpan* | `PASS` |
| S4 | Payload sesuai kontrak `BE-007` | `PASS` |
| S5 | Coba lagi mengirim isian yang sama | `PASS` |
| E1 | Pasien lama (NIK pasien uji): bar step tanpa General Consent (6 step) | `PASS` |
| E2 | Tombol Pembayaran *Lanjut ke Verifikasi* | `PASS` |
| E3 | Pasien lama langsung ke Verifikasi | `PASS` |
| E4 | Kembali dari Verifikasi membuka Pembayaran | `PASS` |
| PE | Tidak ada galat JavaScript halaman | `PASS` |

Total 26/26 `PASS`. Baca ulang via API: `GET …/{ENC-RSMMC-00374}/general-consent` → `200`, `Parent`, "UJI GC Ibu Rina", 12 butir, PNG, petugas "SuperAdmin", `signedAt` server.

Data uji yang tertinggal di `QuilvianNewDevSukma`: pasien `UJI GC PASIEN DAA8`, kunjungan `ENC-RSMMC-00374`, dan consent-nya. Uji pasien lama berhenti di Verifikasi, sehingga tidak ada kunjungan kedua.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien baru dengan consent; pasien lama tanpa, tombol *Lanjut ke Verifikasi*, Kembali → Pembayaran | Terpenuhi | C1, E1–E4, U1–U3 |
| 2. Ditahan bila klausul < 12, nama < 3, tanda tangan kosong | Terpenuhi | C7, C10, U8 |
| 3. Isian dan tanda tangan bertahan saat Kembali lalu Lanjut | Terpenuhi | C13 |
| 4. Tersimpan sesudah submit (poli dan Lab) dan terbaca ulang via API | Jalur poli terpenuhi (S1, S3, baca ulang API). Jalur Lab memanggil fungsi yang sama (`saveConsentForEncounter`) sesudah follow-up rujukan, tetapi belum diuji di layar | S1–S4, kode |
| 5. Gagal simpan → *Coba lagi* tanpa mendaftar ulang | Terpenuhi | S2, S3, S5 |
| 6. IGD dan RI tidak berubah | Terpenuhi lewat diff: tidak ada berkas IGD/RI yang diubah; konstanta RI hanya diimpor | `git status` |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Jalur rujukan Laboratorium + pasien baru belum diuji di layar. Belum ada layar baca/cetak consent RJ di luar pendaftaran. Kartu penanda tangan memakai `BaseCheckboxCard`, sehingga bentuknya sedikit berbeda dari chip RI (isi dan perilaku sama). `npm run test:unit` tidak jalan di Node 20 karena pola glob |
| Wewenang UI | `RJ-REG-DEC-012` (isi = RI, tanda tangan RJ dipertahankan); bentuk visual kartu `DEV_DISCRETION` dengan rekomendasi A |
| Perubahan sampingan | `NONE` |
| Status Git | 8 berkas `M`, 1 berkas `??` di frontend, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran: pasien baru (poli dan Lab), pasien lama |
