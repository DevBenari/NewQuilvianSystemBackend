# Laporan Perubahan Frontend — `FE-HRD-FIN-REC-001` s.d. `FE-HRD-FIN-REC-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `FE-HRD-FIN-REC-001` s.d. `FE-HRD-FIN-REC-027` (Amandemen Finishing Menu 03 — Rekrutmen & Seleksi) |
| Branch & HEAD SHA | `HamzahV2` @ `7dfae4cdb` (*uncommitted working tree*; tanpa `commit`/`push`/`merge`) |
| Submenu target | Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi (`9` tautan sidebar `menu-items.jsx` tetap utuh; **0 submenu baru**) |
| Dependency BE | [`FIN-HRD-REC-FIN-BE-01-20.md`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/task/report/backend/FIN-HRD-REC-FIN-BE-01-20.md) (`BE-HRD-FIN-REC-001..020`, `NO MIGRATION`) |
| Rujukan desain | [`INSTRUKSI_AGENT_HRD_MENU_03_REKRUTMEN_DAN_SELEKSI.md`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/instruksi/INSTRUKSI_AGENT_HRD_MENU_03_REKRUTMEN_DAN_SELEKSI.md) (Skema Target Gambar 1–3) |
| Tanggal | 26 November 2026 |
| Status | 🟡 **IMPLEMENTED (Non-Schema UI Scope Verified; Gate Schema/Policy Pertahankan Status Partial untuk `FE-003` & `FE-014`)** — `check-css-usage.mjs` (`0` temuan / `70` `.jsx`), `eslint --quiet` scoped `0` error (`17` warning pre-existing/hook), `17/17` unit test rekrutmen lulus, `npm run build` (`942/942` halaman + standalone siap), dan `12/12` Chrome CDP UAT `PASS` |

---

## 1. Tujuan & Restrukturisasi Halaman Menu 03

Pembaruan frontend ini merapikan seluruh alur kerja Menu 03 agar pengguna rumah sakit (Recruiter HRD, Asesor Klinis, Kepala Unit, Dokter K3RS, dan Manajemen) memproses transaksi utama pada **halaman dedicated App Router** (bukan menumpuk form panjang di `ConfirmModal`) dengan tampilan mengikuti tiga skema target visual tanpa membuat komponen global baru maupun menyentuh geometri `ConfirmModal` global:

1. **Candidate Workspace (`Gambar 1` — `/hr/recruitment/candidates/[id]`)**:
   - Hero ringkas berisi lingkaran inisial (`AP`), nama, kode kandidat, NIK ter-masking (`maskNik`), serta aksi cepat (`Kembali`, `Edit Biodata`, `Daftarkan ke Lowongan`).
   - Pemilih lamaran aktif tersinkron ke URL (`?applicationId=&stage=&tab=`) dengan `AbortController` anti-*race condition*, stepper 6 tahap, 5 tab ARIA (`Ringkasan | Biodata | Lamaran | Dokumen | Riwayat`), serta panel kanan sticky `Panduan Berikutnya` dari `AllowedActions` & `NextAction`.
   - Sub-transaksi kandidat dipisahkan ke halaman penuh `/edit` (NIK terkunci + pemulihan `409`), `/apply`, dan `/documents/create`.
2. **Worklist & Pemeriksaan Seleksi Administrasi (`Gambar 2` — `/screenings` & `/screenings/[id]`)**:
   - Worklist `/screenings` bawaan memfilter `Pending`, memuat `SummaryGrid` agregat & banner rekonsiliasi read-only, serta menampilkan `clinicalScreeningResultLabel` (`"Menunggu Pemeriksaan STR"`) & jumlah berkas.
   - Halaman `/screenings/[id]` menampilkan `Checklist Dokumen` (`KTP`, `CV`, `Ijazah`, `STR`, kompetensi opsional) dengan indikator ketersediaan, elemen `<progress>`, cek kualifikasi, catatan pemeriksaan, serta 4 kontrol (`Simpan Pemeriksaan`, `Minta Kelengkapan`, `Gugurkan`, `Lolos Administrasi`) dengan *hard gate* dokumen wajib + STR klinis.
3. **Evaluasi & Rubrik Berbobot (`Gambar 3` — `/evaluations`, `/evaluations/hr/[id]`, `/evaluations/clinical/[id]`, `/evaluations/rubrics`)**:
   - Worklist evaluasi menampilkan tab **Antrean Siap Dinilai** (dari `/evaluations/eligible-applications`) di samping daftar sesi terjadwal dan tautan ke `/evaluations/rubrics`.
   - `CandidateEvaluationWorkspace` menampilkan tabel indikator nilai `1–5`, ring skor SVG real-time (`87 dari 100` saat `[4,5,4,4,4,5]`), penyimpanan draft lokal `sessionStorage` per lamaran & jenis evaluasi, dropdown `Rekomendasi` native, finalisasi ke server, serta peringatan Veto Mutlak untuk `ClinicalAssessment`.
   - Halaman `/evaluations/rubrics` menampilkan struktur rubrik dan catatan eksplisit `BLOCKED_SCHEMA_AND_POLICY_GATE` (`BE-011`).
4. **Onboarding Handoff, Offer, MCU, & Talent Pool — Dedicated Pages Lintas Submenu**:
   - Seluruh formulir pembuatan & rincian utama kini menempati rute dedicated: `/requisitions/create`, `/vacancies/create`, `/vacancies/[id]/requirements` (pratinjau UI dengan gate `BE-007/008`), `/offers/create`, `/mcu-clearance/create`, `/mcu-clearance/[id]` (masking medis ketat K3RS), `/talent-pool/create`, `/talent-pool/[id]`, `/talent-pool/[id]/promote`, dan `/handoffs/create`.

**Contoh Skenario Konkret Rumah Sakit (RS MMC)**:
Recruiter membuka menu **Pelamar**, memilih baris *Aulia Rahma Pratama*, lalu mengklik `Workspace`. Layar memuat lamaran *Perawat IGD* pada tahap *Tes/Wawancara*. Ketika membuka halaman `/evaluations/hr/[id]`, recruiter mengubah nilai indikator ke `[4, 5, 4, 4, 4, 5]` dan ring skor langsung menampilkan `87`. Sementara saat pengguna non-medis membuka `/mcu-clearance/[id]`, catatan dokter dan tombol unduh sertifikat diganti teks `"Dirahasiakan (hanya pemeriksa berhak)"`.

---

## 2. Daftar Berkas Frontend yang Dibuat & Diubah (`QuilvianSystemFrontendDev`)

| Kelompok | Berkas Utama | Perubahan |
| --- | --- | --- |
| Route & Service Core | `src/lib/constants/hr/recruitment/recruitment-constants.js`, `src/lib/services/hr/recruitment-service.js`, `src/lib/hooks/hr/use-hr-remote-data.js` | Pembangun rute dedicated `RECRUITMENT_ROUTES` (termasuk `?applicationId=&stage=&tab=`), `SCREENING_DECISION_VALUES`, pembungkus API baru (`eligible-*`, `reconcile*`, `getScreeningByApplicationId`, `getScreeningSummary`, `getMcuById`, `getTalentPoolById`), serta `statusCode` pada hook error. |
| Styling & Utilitas | `src/style/hr/recruitment/recruitment-workspace.module.css`, `src/utils/hr/recruitment/recruitment-workspace-utils.js` | CSS Module token murni (`0` hardcoded hex, `0` Tailwind, `.avatarInitials`, `.progressNative`, `.tableCompactInput`, `.scoreRing*`) serta utilitas tab, inisial, stepper visual, checklist `StrCertificate`, dan kalkulator skor berbobot rubrik. |
| Workspace Pelamar & Dedicated Kandidat | `candidates-directory-view.jsx`, `candidate-detail-screening-view.jsx`, `candidate-workspace-tabs.jsx`, `wizard/candidate-wizard-step-panels.jsx`, `candidate-edit-view.jsx`, `candidate-apply-view.jsx`, `candidate-document-create-view.jsx` + App Router `src/app/hr/recruitment/candidates/[id]/{edit,apply,documents/create}/page.jsx` | Pemisahan `Status Pelamar` vs `Lamaran & Tahap Aktif`, NIK masking, Workspace Gambar 1 (`5` tab + `Panduan Berikutnya`), tautan wizard ke screening/workspace, serta form dedicated `/edit`, `/apply`, `/documents/create` tanpa modal. |
| Requisition & Vacancy | `requisition-form-view.jsx`, `requisitions-list-view.jsx`, `requisition-detail-view.jsx`, `vacancy-form-view.jsx`, `vacancies-list-view.jsx`, `vacancy-detail-view.jsx`, `vacancy-requirements-view.jsx` + `src/app/hr/recruitment/{requisitions/create,vacancies/create,vacancies/[id]/requirements}/page.jsx` | Form dedicated requisition & lowongan lengkap dengan sisa kuota dan pratinjau UI persyaratan (gate `BE-007/008`). |
| Seleksi Administrasi & Evaluasi | `screening-worklist-view.jsx`, `screening-detail-view.jsx`, `evaluation-worklist-view.jsx`, `evaluation-rubrics-view.jsx`, `CandidateEvaluationWorkspace.jsx` + `src/app/hr/recruitment/{screenings/[id],evaluations/rubrics}/page.jsx` | Worklist + detail checklist Gambar 2 (termasuk perbaikan kunci enum `SCREENING_DECISION_VALUES.Pending/Passed/Rejected` hasil temuan UAT), antrean evaluasi, rubrik Gambar 3 dengan ring skor SVG & dropdown `Rekomendasi` native, serta gerbang kebijakan `Master Rubrik`. |
| Offer, MCU, Talent Pool, Handoff | `job-offer-create-view.jsx`, `job-offers-list-view.jsx`, `job-offer-detail-view.jsx`, `mcu-create-view.jsx`, `mcu-detail-view.jsx`, `mcu-clearance-view.jsx`, `talent-pool-create-view.jsx`, `talent-pool-detail-view.jsx`, `talent-pool-promote-view.jsx`, `talent-pool-view.jsx`, `hiring-handoff-create-view.jsx`, `hiring-handoffs-view.jsx` + rute `page.jsx` terkait | Penggantian transaksi modal menjadi halaman penuh App Router, masking medis MCU K3RS di `/mcu-clearance/[id]`, validasi fast-track `/talent-pool/[id]/promote`, dan alur terpadu ke onboarding. |
| Unit Test Baru | `tests/unit/recruitment-workspace-utils.test.mjs` | `6` unit test murni: kesesuaian kunci `SCREENING_DECISION_VALUES`, `normalizeWorkspaceTab`, `getCandidateInitials`, `getWorkspaceStepVisual`, `buildScreeningDocumentChecklist`, dan `calculateWeightedRubricScore === 87`. |

### Base Component Decision Gate

- **Keputusan**: `REUSE` komponen standar (`Hero`, `BaseButton`, `BaseInputField`, `BaseNativeSelectField`, `BaseSelectField`, `BaseDateField`, `BaseTextAreaField`, `DataTable`, `SummaryGrid`, `InformationAlert`, `StatusBadge`, `HrPanel`, `HrInfoGrid`, `HrStatusBadge`, `HrResourceField`, `HrAttachmentField`, `ConfirmModal`, `ToastStack`) + komposisi CSS Module token murni (`recruitment-workspace.module.css`).
- **Jumlah komponen base global baru**: **`0`**. Modifikasi `menu-items.jsx` / `ConfirmModal` global: **`0`**.
- **UI Gate**: `check-css-usage.mjs` pada **70 berkas `.jsx` Rekrutmen** → **`OK — 70 berkas diperiksa, 0 temuan`**.

---

## 3. Verifikasi & Catatan Jujur

| Pemeriksaan | Hasil Aktual | Status | Keterangan |
| --- | --- | --- | --- |
| `node .../check-css-usage.mjs` (70 berkas `.jsx` Rekrutmen) | `OK — 70 berkas diperiksa, 0 temuan.` | `PASS` | Dijalankan pasca-edit akhir FE menggunakan path literal sehingga seluruh `[id]` ikut teraudit. |
| `npx eslint --quiet` pada seluruh path Rekrutmen yang diubah | `0 error` (Exit `0`) | `PASS` | Catatan: `npx eslint --max-warnings=0` masih mencatat `17` warning `react-hooks/*` (`0` error); target CI `lint:errors` mensyaratkan `0` error. |
| Unit test Rekrutmen (`recruitment-workspace-utils`, `recruitment-journey-utils`, `candidate-registration-wizard-utils`) | **`tests 17, pass 17, fail 0`** (`183.3877 ms`) | `PASS` | Mencakup test baru `SCREENING_DECISION_VALUES` & kalkulasi skor `87`. |
| Baseline `npm run test:unit` (dijalankan pra-polesan label akhir) | `tests 5570, pass 5421, fail 149` | `PASS (Scoped)` | Seluruh `149` gagal berada di modul non-rekrutmen pre-existing (`payroll-*`, `workforce-salary-assignment.test.mjs`, `inpatient-*`, `lab-*`); **`0` kegagalan di Rekrutmen**. |
| `npm run build` (`prebuild` + `next build` + `postbuild`) | `frontendVersion 2.2.0 valid`, `✓ Compiled successfully in 93s`, `942/942` halaman statis, `[prepare-standalone] Standalone runtime siap dijalankan.` | `PASS` | Dijalankan sesudah edit akhir FE (`screening-detail-view.jsx`, `CandidateEvaluationWorkspace.jsx`, `recruitment-workspace-utils.test.mjs`). |
| Live Browser Chrome Headed CDP (`fin-rec-1126-uat-browser.mjs`) | **`12/12 PASS, 0 FAIL`**, `0 http5xx`, `0 pageErrors` | `PASS` | Rincian skenario, bug yang ditemukan saat UAT awal (`decision=undefined`), dan screenshot lengkap tersedia pada [Laporan Live UAT](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/testing/test-by-agy/laporan-live-testing-rekrutmen-dan-seleksi-finishing.md). |
