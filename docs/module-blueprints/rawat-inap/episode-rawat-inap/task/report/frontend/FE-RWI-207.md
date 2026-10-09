# Laporan Perubahan Frontend — `FE-RWI-207`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-207` |
| Judul | Tombol "Simpan & Lanjut ke Pembayaran" selalu dapat ditekan; ringkasan dan fokus ke isian yang belum lengkap |
| Slice | Perubahan desain pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-003` butir `ISS-EPS-003-07`; `PLAN-REPAIR-EPS-003` perbaikan `FIX-EPS-003-07`, `FIX-EPS-003-10`; `RWI-DEC-223` |
| Contract version | `0.10.0` — tidak disentuh. Urutan `POST /patients` → `POST /patient-identity-documents` → `POST /patient-emergency-contacts` tidak berubah |
| Dependency | `FIX-EPS-003-10` bagian (a) — skema 3.3 dan 4.2 sudah direvisi ke revision `0.6` |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | `RWI-DEC-223` — keputusan pemilik pada laporan 6 Oktober 2026 butir 7; skema tampilan 3.3 revision `0.6` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b010ffb9722f236160477c95c931c22467550200` (`HamzahV2`), perubahan belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

Tombol "Simpan & Lanjut ke Pembayaran" mati sampai seluruh isian wajib terisi (`disabled=
{!canSubmitNewPatient}`), tanpa memberi tahu isian mana yang kurang. Formulir memakai `mode: "onBlur"`,
sehingga isian yang belum disentuh tidak menampilkan pesan apa pun. Perilaku itu **sesuai** skema 3.3
yang disetujui; pemilik meminta diubah, sehingga ini perubahan desain (`RWI-DEC-223`).

## 2. Proses bisnis

1. Tombol "Simpan & Lanjut ke Pembayaran" dapat ditekan kapan saja, kecuali selama penyimpanan berjalan.
2. Ditekan saat isian belum lengkap: **tidak ada request ke server**. Di atas formulir tampil ringkasan
   peringatan, misalnya *"3 isian belum lengkap"* beserta daftar "Tempat Lahir — Identitas Pasien",
   "Kecamatan — Alamat Pasien", "Hubungan — Kontak Darurat".
3. Halaman bergulir ke isian pertama yang kosong menurut urutan layar, dan kursor masuk ke dalamnya. Bila
   isian itu berada di bagian "Data tambahan pasien" yang tertutup, bagian itu dibuka dulu.
4. Setiap nama isian pada ringkasan dapat ditekan untuk menuju isiannya.
5. Begitu isian diperbaiki, pesan merah dan baris ringkasannya hilang sendiri.
6. Formulir lengkap: satu tekan menjalankan tiga penyimpanan berurutan seperti sebelumnya, lalu maju ke
   Pembayaran. Tekan dua kali dengan cepat tetap menghasilkan satu pasien.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` | Tombol hanya `disabled={savingPatient}`. Komponen `ValidationSummary` membaca `useFormState` — tampil setelah `submitCount > 0`, berisi tombol per isian. `focusNewPatientField` mencari isian lewat `data-field-name` atau `name`, membuka `<details>` yang tertutup, menggulir ke tengah layar, lalu memfokuskan elemen fokusabel pertama; dijalankan pada frame berikutnya |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | `canSubmitNewPatient` dan `useWatch` seluruh formulir dihapus. `handleSaveNewPatient({ onInvalid })` memakai `handleSubmit` agar formulir tercatat sudah dikirim — react-hook-form lalu memvalidasi ulang setiap perubahan. Penulisan dipisah ke `persistNewPatient`, yang juga menolak klik kedua selama penyimpanan berjalan |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-fields.jsx` | Pembungkus `EmergencySelectField`, `EmergencyDateField`, dan `EmergencyTimeField` memuat `data-field-name`, karena `FilterSelect`/`FilterDatePicker` tidak meneruskan `ref` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_NEW_PATIENT_FIELD_ORDER` — 23 isian menurut urutan tampil, beserta label dan bagiannya |
| `src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx` | `getInpatientNewPatientErrorFields(errors)` — isian bergalat menurut urutan layar; isian di luar daftar ditaruh di akhir dengan pesan galat sebagai label, bukan nama teknis |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | `.validationSummaryList` dan `.validationSummarySection` — hanya token desain |

### 3.2 Radius dampak

| Pemakai | Dampak |
| --- | --- |
| Admisi rawat inap Langkah 2 | Perilaku tombol dan validasi berubah sesuai `RWI-DEC-223` |
| Pendaftaran IGD | Hanya atribut `data-field-name` pada pembungkus isian; tidak ada perubahan perilaku. Tombol "Simpan Pasien & Lanjut" IGD memang sejak awal tidak dimatikan oleh kelengkapan isian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada request ketika isian belum valid; urutan request ketika valid tidak berubah |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Tombol simpan | `REUSE` | `BaseButton` varian `primary` dengan `loading` |
| Ringkasan isian | `REUSE` | `InformationAlert` varian `warning` dengan children, pola yang sama dengan peringatan "Pasien sudah terdaftar" pada langkah ini |
| Tautan per isian pada ringkasan | `REUSE` | `BaseButton` varian `ghost` ukuran `sm`. Versi awal memakai `<button>` mentah dengan kelas CSS sendiri; diganti setelah grep anti-regresi menandainya |
| Daftar ringkasan | `COMPOSE` | `ul` dengan dua kelas baru berbasis token (`--space-1`, `--color-text-muted`). Opsi lain — memakai `nextStepList` yang sudah ada — ditolak karena jarak antarbutirnya `--space-2` untuk kartu langkah berikutnya, terlalu longgar untuk daftar di dalam alert |

`UI GATE: PASS` — tidak ada `NEW`, tidak ada perubahan perilaku bawaan base component.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Unit test `inpatient-admission-registration-issue-003.test.mjs` | 9 dari 9 lulus; untuk task ini: ringkasan mengikuti urutan layar (Tempat Lahir → Kecamatan → Hubungan), bukan urutan objek `errors` | `PASS` |
| Suite unit penuh `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` (pengganti `npm run test:unit` yang gagal oleh sebab lingkungan) | 2510 test: 2502 lulus, 8 gagal. Kedelapan kegagalan ada di `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation`, `outpatient-prescription-parity`, `inpatient-procedure-parity`, dan `inpatient-setting` — tidak satu pun membaca atau mengimpor berkas yang diubah rencana ini | `PASS` untuk cakupan task / `UNRELATED EXISTING ISSUE` |
| `npx eslint` berkas yang diubah | 0 error, 0 warning baru | `PASS` |
| `npx eslint src --quiet` | Exit `0` | `PASS` |
| `npm run build` | Exit `0`; kompilasi 72 detik; 474 halaman; standalone siap | `PASS` |
| Grep anti-regresi UI pada baris baru | Nol warna/ukuran literal, nol `!important`, nol `<button>` mentah | `PASS` |
| Fokus dan gulir di peramban, termasuk isian daftar pilihan dan bagian tertutup | Belum dijalankan | `NOT RUN` |

`MANUAL TEST: NOT RUN` — dikecualikan atas keputusan pemilik 1 September 2026 dan 10 September 2026.

`AUTOMATED TEST: node --test tests/unit/inpatient-admission-registration-issue-003.test.mjs — PASS (9/9)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tombol dapat ditekan walau formulir kosong | Terpenuhi | `disabled={savingPatient}` |
| Tempat Lahir dan Kecamatan kosong → tanpa request; ringkasan "2 isian belum lengkap"; fokus ke Tempat Lahir; pesan merah pada keduanya | Terpenuhi pada source | `handleSubmit` memanggil `onInvalid` tanpa menjalankan `persistNewPatient`; `ValidationSummary`; `focusNewPatientField` |
| Isian pertama berupa daftar pilihan atau tanggal tetap menerima fokus | Terpenuhi pada source | `data-field-name` pada pembungkus; fokus ke elemen fokusabel pertama |
| Isian kosong di bagian tertutup → bagian terbuka | Terpenuhi pada source | `target.closest("details")` |
| Klik nama isian pada ringkasan memindahkan fokus | Terpenuhi | Tombol `ghost` per isian |
| Formulir lengkap → tiga panggilan berurutan; dua klik cepat tetap satu pasien | Terpenuhi | `persistNewPatient` tidak berubah urutannya; `savePatientInFlight` diperiksa di awal penyimpanan |
| Selama menyimpan, tombol terkunci dan bertuliskan "Menyimpan..." | Terpenuhi | `loading` + `loadingLabel` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dokumen desain | `05-skema-tampilan.md` naik ke revision `0.6`: baris tombol 3.3, dua baris Keadaan baru, dan pengecualian baris Fokus 4.2. `RWI-DEC-223` dan `RWI-DEC-224` dicatat pada `00-interview-decisions.md` revision `34` |
| Risiko tersisa | Verifikasi peramban belum dijalankan; perilaku fokus pada `FilterSelect` bergantung pada elemen fokusabel pertama di dalam pembungkusnya |
| Perubahan sampingan | `NONE` |
| Status Git frontend | Berkas `PLAN-REPAIR-EPS-003` (`FE-RWI-203` s.d. `207`): `M` `plustek-scan-panel.jsx`, `new-patient-form.jsx`, `emergency-registration-fields.jsx`, `inpatient-admission-registration-step.jsx`, `use-plustek-ktp-scanner.js`, `use-emergency-registration.js`, `use-inpatient-admission-patient.jsx`, `emergency-region.service.js`, `emergency-registration.constants.js`, `inpatient-admission-flow-constants.jsx`, `inpatient-admission-patient-utils.jsx`, `emergency-registration.utils.js`, `inpatient-admission.module.css`; `??` `tests/unit/inpatient-admission-registration-issue-003.test.mjs`. Enam berkas `nursing-workspace` yang juga berubah di working tree bukan milik rencana ini dan tidak disentuh |
