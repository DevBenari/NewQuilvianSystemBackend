# Laporan Perubahan Frontend — `FE-HRD-050`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-050` |
| Judul | Layar Formulir Evaluasi Wawancara HR & Scorecard Penilaian |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-12` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-073`, `HRD-DEC-074`, `FR-REC-05`, `VR-REC-09`, `VR-REC-10`, `BE-HRD-056` |
| Contract version | API Contract v8 bagian 14.5 grup `Candidate Evaluation` |
| Dependency | `FE-HRD-049` (✅ Selesai), `BE-HRD-056` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/evaluations/hr/[id]/page.jsx`, `src/components/view/hr/recruitment/candidate-hr-evaluation-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Wawancara HR menilai kecocokan budaya kerja rumah sakit, integritas, orientasi pelayanan pasien, dan komitmen kedisiplinan calon karyawan.

Task ini mengimplementasikan formulir evaluasi wawancara HR:
1. **Scorecard Penilaian Terstruktur (`/hr/recruitment/evaluations/hr/[id]`)**:
   - Rubrik nilai terukur (skala 0-100) untuk 3 kriteria utama:
     - Komunikasi & Orientasi Layanan Pasien
     - Motivasi, Integritas, dan Resiliensi
     - Relevansi Pengalaman Kerja
   - Kalkulasi otomatis rata-rata skor gabungan dengan indikator visual.
2. **Penetapan Rekomendasi HR**:
   - Pilihan keputusan: `RECOMMENDED` (Lolos), `CONSIDERED` (Cadangan Talent Pool), atau `NOT_RECOMMENDED` (Gugur Wawancara).
   - Input catatan narasi observasi interviewer.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Form evaluasi menyajikan kriteria penilaian skor 0–100 dan kalkulasi skor rata-rata | ✅ PASS | Terbukti pada `candidate-hr-evaluation-view.jsx` dengan state `communicationScore`, `motivationScore`, `experienceScore` dan kalkulator `averageScore`. |
| 2 | Pilihan rekomendasi keputusan HR terhubung ke API backend | ✅ PASS | Terhubung via `RecruitmentService.submitHrEvaluation` dengan pemetaan rekomendasi 0, 1, 2. |
| 3 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `BaseFormControl`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/candidate-hr-evaluation-view.jsx` (Komponen formulir evaluasi HR)
- `src/app/hr/recruitment/evaluations/hr/[id]/page.jsx` (Rute dinamis Next.js App Router)
