# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-003` |
| Judul | SOAP |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 2a–2f |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.3` |
| Trace | `RJ-DOC-DEC-007`; `RJ-DOC-REV-BE-003`, `RJ-DOC-REV-BE-006` |
| Contract version | `PATCH /doctor-consultations/{id}/soap` (tidak berubah); field kelompok DTD pada `master-options` (`RJ-DOC-REV-BE-006`) |
| Wewenang UI | `DEV_DISCRETION` dalam batas base component dan design token; tidak ada komponen `NEW` |
| Dependency | `RJ-DOC-REV-BE-003` 🟡 (endpoint berfungsi), `RJ-DOC-REV-BE-006` ✅ |
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

- Header SOAP menampilkan nama pasien, No. RM, dan status "SOAP siap diedit." (dilingkari merah di UAT).
- Subjective berisi gabungan banyak riwayat asesmen; Objective berisi tanda vital, kesadaran, GCS, EWS, dan catatan.
- Data diambil dari catatan aktif antrean, bukan yang terakhir tersimpan.
- Hasil pencarian ICD-10 berupa daftar datar.

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab SOAP. Subjective terisi `Keluhan utama: <keluhan>` dari asesmen **final terakhir** pada kunjungan ini, termasuk skrining ulang dokter. Objective terisi tanda vital dasar terakhir: TD, nadi, napas, suhu, SpO₂, BB, TB, BMI.
2. Isian yang diketik dokter tidak ditimpa; teks hasil generate lama diganti otomatis.
3. Dokter mencari ICD-10; hasil dikelompokkan per kelompok ICD Diagnosa. Contoh: judul `001.0 Kolera` di atas `A00`, `A00.0`, `A00.1`, `A00.9`. Mengetik nama kelompok (mis. `kolera`) juga berhasil.
4. Assessment terisi dari diagnosis terpilih (perilaku yang sudah ada).
5. Autosave tetap berjalan. Status hanya muncul saat menyimpan, ada perubahan, atau gagal.

---

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/registration-management/doctor-queues/tabs/soap/doctor-soap-tab.jsx` | Badge nama/RM/"siap diedit" dihapus; modal ICD dikelompokkan; placeholder disesuaikan |
| `src/lib/hooks/health-services/clinical-management/use-doctor-soap.js` | Ambil asesmen dan tanda vital final terakhir per kunjungan; builder baru |
| `src/utils/health-services/clinical-management/doctor-soap-utils.js` | `buildSubjectiveFromChiefComplaint`, `buildObjectiveFromBasicVitalSign` |
| `src/components/ui/doctor-clinical-base/DoctorDiagnosisSearchModal.jsx` | Prop opsional `groupByDiagnosisGroup` (default `false`) |
| `src/components/ui/doctor-clinical-base/doctor-soap.module.css` | `.diagnosisGroupHeading` bertoken |
| `tests/unit/doctor-soap-revision.test.mjs` (baru) | 3 test |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view → hook → service → InstanceAxios`. View tidak memanggil Axios langsung; normalisasi ada di `utils`. Tidak ada state global, provider, atau abstraksi HTTP baru.

```
UI GATE: 3 elemen — REUSE 2, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Header SOAP | `DoctorSoapHeader` | REUSE | — |
| Kelompok ICD pada pencarian | `DoctorDiagnosisSearchModal` | EXTEND | **A (dipilih)**: prop opsional default `false`, layar rawat inap tidak berubah. B: ubah default — regresi rawat inap. C: modal baru — duplikasi kontrak |
| Textarea S/O/A/P | `DoctorSoapField` | REUSE | — |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat SOAP..." |
| Gagal simpan | Badge "Autosave gagal." dan pesan galat |
| ICD tanpa kelompok | Judul "Tanpa kelompok" |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/doctor-consultations/{id}/soap` | Autosave | `DoctorConsultation : WriteSoap` |

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/patient-diagnoses/master-options` | Pencarian ICD dan kelompok DTD | `PatientDiagnosis : Read` |

Ditambah `GET /patient-assessments` dan `GET /patient-vital-signs` (`?patientId=&encounterId=`) untuk data terakhir tersimpan.

---

## 6. Verifikasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint <berkas task>` | `0` error; peringatan yang muncul berasal dari pola lama di berkas yang sama (`react-hooks/exhaustive-deps`, `set-state-in-effect`) | `PASS` |
| `node --test tests/unit/doctor-soap-revision.test.mjs` | `3/3` lulus | `PASS` |
| `npm run test:unit` (seluruh suite) | `2175` test: `2169` lulus, `6` gagal. Keenamnya di `hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`, `petty-cash-finance-separation` — tidak satu pun membaca berkas task ini | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | `✓ Compiled successfully in 52s`, postbuild standalone siap, exit `0` | `PASS` |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/doctor-soap-revision.test.mjs — PASS

MANUAL TEST: NOT FEASIBLE — sesi agen tidak memiliki peramban untuk mengklik layar. Sebagai gantinya, kontrak data yang dikonsumsi layar diuji lewat HTTP sungguhan ke aplikasi backend scratchpad (`localhost:5217`, `QuilvianNewDevSukma`): autosave `PATCH /soap` `200`; `master-options?search=kolera` mengembalikan kelompok `001.0 Kolera`. Pemilik perlu mengklik ulang alur pada bagian 2 di lingkungan dev.

### Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Warna/spacing/typography baru memakai token (`var(--color-*)`, `var(--space-*)`, `var(--font-size-*)`) | Ya — `var(--space-*)`, `var(--color-*)`, `var(--font-size-*)`, `var(--font-weight-semibold)` |
| Tidak ada `<button>`/`<table>`/input mentah baru | Ya — memakai `BaseButton`, `DataTable`, `FilterSelect`, `ResourceFilterSelect`, `BaseTextField` |
| Typography komponen shared tidak di-override | Ya |
| State memuat/kosong/gagal tersedia | Ya — lihat bagian 4 |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| a. Badge yang dilingkari tidak ditampilkan | Terpenuhi | `doctor-soap-tab.jsx` |
| b. Subjective = keluhan utama asesmen terakhir | Terpenuhi | Unit test; `use-doctor-soap.js` |
| c. Objective = vital sign dasar terakhir | Terpenuhi | Unit test |
| d. Grouping ICD-10 per ICD Diagnosa | Terpenuhi | `RJ-DOC-REV-BE-006` R3–R4; modal |
| e. Assessment dari ICD terpilih | Terpenuhi (perilaku lama dipertahankan) | `buildAssessmentFromDiagnoses` |
| f. Simpan SOAP | Terpenuhi pada `HEAD` | `RJ-DOC-REV-BE-003` R1–R5 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Penyebab gagal simpan UAT `28/9` belum terbukti (`RJ-DOC-REV-BE-003` 🟡). Bila masih terjadi, periksa migration dan hak akses `WriteSoap` di lingkungan UAT |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `git status --short` frontend pada laporan ringkas sesi; seluruh perubahan belum di-commit |
