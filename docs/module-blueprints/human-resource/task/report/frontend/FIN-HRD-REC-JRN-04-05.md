# Laporan Perubahan Frontend — `FIN-HRD-REC-JRN-04` & `FIN-HRD-REC-JRN-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `FIN-HRD-REC-JRN-04` (Komponen & Workspace Journey di Detail Kandidat), `FIN-HRD-REC-JRN-05` (Integrasi Konteks Deep-Link, URL Sync, Unit Test, & Aksesibilitas) |
| Judul | Recruitment Journey Terintegrasi pada `/hr/recruitment/candidates/[id]` |
| Submenu target | Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi → **Pelamar** (`/hr/recruitment/candidates/[id]`, **tanpa** menambah sub-menu sidebar baru; tetap `9` tautan `/hr/recruitment/*` di `menu-items.jsx`) |
| Dependency BE | `FIN-HRD-REC-JRN-01` s.d. `03` ✅ ([laporan BE](file:///C:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/task/report/backend/FIN-HRD-REC-JRN-01-03.md)) |
| Target tulis | `QuilvianSystemFrontendDev/{src,tests}/**`, `test-with-agy/**`, laporan ini |
| Tanggal | 25 November 2026 |
| Status | ✅ **IMPLEMENTED** — `check-css-usage.mjs` (`0` temuan / `11` berkas), `eslint --max-warnings=0` (`0` error & warning / `15` berkas), HR unit test `19/19` (`5/5` helper journey baru), `npm run build` (`✓ Compiled successfully in 91s`, `935/935` halaman statis & standalone siap), serta `11/11` grup UAT Chrome headed `PASS` |

---

## 1. Tujuan & Alur Pengguna di Layar

Pemantauan perjalanan seleksi pelamar diintegrasikan langsung pada halaman detail pelamar `/hr/recruitment/candidates/[id]` agar HR Recruiter, Komite Klinis, Dokter K3RS, dan HR Lifecycle melihat perkembangan 6 tahap seorang kandidat tanpa berpindah ke menu tambahan maupun kehilangan konteks lamaran ketika membuka sub-layar wawancara, asesmen klinis, atau penawaran.

**Urutan interaksi utama di layar (`CandidateJourneyPanel`)**:
1. **Sinkronisasi URL & Pemilih Multi-Lamaran**: Parameter `?applicationId=&stage=` dibaca lewat `useSearchParams()` (halaman dibungkus `<Suspense fallback={null}>`). Bila parameter `applicationId` tidak valid atau sudah usang, layar menampilkan `InformationAlert` peringatan ramah lalu jatuh aman ke lamaran terbaru. Bila pelamar memiliki >1 lamaran, pemilih `Lamaran Aktif` memperbarui URL via `router.replace(..., { scroll: false })` dan hook `useHrRemoteData` membatalkan permintaan terdahulu (*AbortController*) + mengecek `journeyRemote.data?.applicationId === selectedApplicationId` agar data tidak tertukar akibat *race condition*.
2. **Header Ringkasan & Peringatan Blocker**: Menampilkan lowongan sasaran, penanda *Klinis (Wajib STR & Veto)* / *Non-Klinis*, NIK ter-masking pada Hero maupun subjudul (`maskNik`), badge `Outcome`, `Tahap Aktif`, `Kelengkapan Data` (`Complete`/`Partial`/`Restricted`), indikasi `Fast-Track Talent Pool`, `Kandidat Rehire`, dan tumpukan blocker (`BLOCKING` / `WARNING`) beserta saran resolusi.
3. **Stepper 6 Tahap Interaktif**: Membedakan tegas antara tahap inspeksi yang sedang dibuka (`aria-current="step"`) dengan posisi aktual lamaran di server (`Tahap Saat Ini`), serta menerapkan warna tepi atas berbasis CSS tokens (`--color-success`, `--color-primary`, `--color-warning`, `--color-danger`, `--color-disabled`) — responsif menjadi 3 kolom pada tablet dan 1 kolom vertikal pada ponsel (`390px`).
4. **Rincian Tahap & Kronologi (`CandidateJourneyStageDetails`)**:
   - *Pendaftaran*: kelengkapan & verifikasi dokumen, STR, fast-track, serta ringkasan peringkat Talent Pool bila ada.
   - *Seleksi Administrasi*: status seleksi, wajib STR, nomor & masa berlaku STR, catatan pemeriksa.
   - *Tes & Wawancara*: status HR & klinis, sesi dalam `DataTable`, serta pemberitahuan eksplisit bila sesi penilai lain disembunyikan (`VR-PKB-07`).
   - *Surat Penawaran*: kontrak, plafon grade, status `Tier13Standard`/`Tier14Exceptional`, penyamaran nominal (`Dirahasiakan`) ketika `isSalaryMasked = true`, serta riwayat persetujuan.
   - *MCU Pra-Kerja*: nomor rujukan, fasilitas, kesimpulan kelaikan, penahanan `FitWithRestrictions`, serta penyamaran `DoctorNotes` + tombol unduh sertifikat (`isStoredAttachmentKey`) bila `isMedicalDetailsMasked = true`.
   - *Serah Terima*: nomor handoff, kode `WorkforceProfile`, penanda *Profil Pegawai Lama Dipakai Ulang*, kursi headcount, alert *Pemulihan Kasus Onboarding Diperlukan* (`NeedsOnboardingRecovery`), dan tautan langsung ke `/hr/lifecycle/onboarding-cases/{id}`.
5. **Panel Samping Langkah Berikutnya & Aksi Kontekstual**: Menjalankan tindakan inline aman (`INITIATE_SCREENING`, `EVALUATE_SCREENING`, `RESPOND_CONDITIONAL_OFFER`, `RESPOND_FINAL_OFFER`, `CONFIRM_HANDOFF_TRANSFER`, `RECOVER_ONBOARDING_CASE`) melalui `ConfirmModal` pre-terikat tanpa ketik UUID, atau menavigasi deep-link ke `/hr/recruitment/evaluations/{hr|clinical}/[id]?applicationId=...` dan `/hr/recruitment/offers/[id]?applicationId=...&candidateId=...` lengkap dengan tombol *Kembali ke Journey*.

---

## 2. Berkas Frontend yang Dibuat & Diubah (`QuilvianSystemFrontendDev`)

| Berkas | Status | Peran |
| --- | --- | --- |
| `src/utils/hr/recruitment/recruitment-journey-utils.js` | Baru | Fungsi murni: `resolveSelectedApplication`, `resolveSelectedStageKey`, `buildCandidateJourneyQueryString`, `buildJourneyDeepLink`, pemeta badge status/outcome/completeness, masker tampilan gaji/medis, & `isStoredAttachmentKey`. |
| `src/style/hr/recruitment/candidate-journey.module.css` | Baru | CSS Module murni berbasis design token (`--color-*`, `--space-*`, `--radius-*`, `--focus-ring`, responsif `<=1180px` & `<=680px`, `0` literal hex/rgb, `0` Tailwind). |
| `src/components/view/hr/recruitment/candidate-journey-stage-details.jsx` | Baru | Renderer detail spesifik tiap 6 tahap lengkap dengan masker RBAC & blocker Recovery. |
| `src/components/view/hr/recruitment/candidate-journey-panel.jsx` | Baru | Kontainer state, URL sync, stepper 6 tahap, kronologi collapsible, panel `Panduan Langkah Berikutnya`, dan `ConfirmModal` aksi inline. |
| `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` | Diubah | Menyematkan `<CandidateJourneyPanel>`, tombol *Buka Journey* di tabel lamaran, deep-link `?applicationId=`, serta masking NIK Hero (`maskNik`). |
| `src/components/features/hr/recruitment/CandidateEvaluationWorkspace.jsx` | Diubah | Membaca `?applicationId=` lewat `useSearchParams()` untuk prapilih lamaran tepat sasaran. |
| `src/components/view/hr/recruitment/candidate-hr-evaluation-view.jsx` & `candidate-clinical-exam-view.jsx` | Diubah | Tombol *Kembali ke Journey* mengarah balik ke `/candidates/{id}?applicationId=...&stage=evaluation`. |
| `src/components/view/hr/recruitment/job-offer-detail-view.jsx` | Diubah | Tombol *Kembali ke Journey* menuju `/candidates/{id}?applicationId=...&stage=job_offer`. |
| `src/app/hr/recruitment/{candidates,evaluations/hr,evaluations/clinical,offers}/[id]/page.jsx` | Diubah | Pembungkus `<Suspense fallback={null}>` agar Next App Router bebas galat CSR bailout pada `useSearchParams()`. |
| `src/lib/constants/hr/recruitment/recruitment-constants.js` & `src/lib/services/hr/recruitment-service.js` | Diubah | Menambah `RECRUITMENT_ROUTES.candidateJourney` dan `RecruitmentService.getApplicationJourney(applicationId, signal)` (mempertahankan baris pre-existing pada line 208). |
| `tests/unit/recruitment-journey-utils.test.mjs` | Baru | `5` unit test murni utilitas Journey (query fallback, URL sync, pemetaan badge `StatusBadge`, masker, & validasi key sertifikat MCU). |

### Base Component Decision Gate

| Elemen UI | Pilihan | Keputusan | Catatan |
| --- | --- | --- | --- |
| Stepper 6 tahap, grid 2 kolom detail, timeline kronologi | `BaseDetailSection` + `BaseDetailSidePanel` + CSS tokens `candidate-journey.module.css` | `COMPOSE` + `REUSE` | Memakai design token & base detail section/side panel yang ada |
| Badge status tahap, outcome, kelengkapan, & tombol | `StatusBadge`, `HrStatusBadge`, `BaseButton` | `REUSE` | 100% varian & status sah sesuai audit `check-css-usage.mjs` |
| Tabel sesi evaluasi, riwayat persetujuan, grid info | `DataTable`, `HrInfoGrid`, `InformationAlert`, `ToastStack` | `REUSE` | Konsisten seluruh layar HRD (`sortLatestFirst={false}`) |
| Aksi inline screening/respon offer/pemulihan handoff & pilih lamaran | `ConfirmModal`, `BaseSelectField`, `BaseCheckboxField`, `BaseTextAreaField`, `CandidateScreeningEvaluationFields` | `REUSE` | Tanpa input UUID manual |

`UI GATE: PASS — 0 Tailwind utilities, 0 raw colors, 0 temuan check-css-usage.mjs pada 11 JSX`

---

## 3. Verifikasi & Status Akhir

| Pemeriksaan | Hasil | Status |
| --- | --- | --- |
| `node .../check-css-usage.mjs` (11 `.jsx`) | `OK — 11 berkas diperiksa, 0 temuan.` | `PASS` |
| `npx eslint --max-warnings=0` (15 berkas berubah/baru) | `0 error`, `0 warning` | `PASS` |
| `npm run lint:errors` (lint seluruh repo FE sebelum polesan CSS modal) | Exit `0` | `PASS` |
| HR unit test suite (`recruitment-journey-utils`, `candidate-registration-wizard-utils`, `workforce-core-target-state`) | `19/19 PASS` (`5/5` unit test baru `recruitment-journey-utils.test.mjs`) | `PASS` |
| Full `npm run test:unit` (dijalankan sebelumnya di `task-804`) | `5552 tests: 5403 pass, 149 fail` — seluruh `149` gagal adalah pre-existing di modul non-rekrutmen (`workforce-salary-assignment.test.mjs` ENOENT, `hemodialysis-*`, `inpatient-*`); **0 gagal di Recruitment** | `PASS (Scoped)` |
| `npm run build` + `postbuild` standalone | `✓ Compiled successfully in 91s`; `935/935` halaman statis; standalone runtime siap | `PASS` |
| Live Browser Chrome Headed (`jrn-1125-uat-browser.mjs`) | **`11/11` grup UAT PASS** (`0` HTTP 5xx, `0` pageError) | `PASS` |
