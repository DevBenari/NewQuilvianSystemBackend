# Laporan Perubahan Frontend — `FE-HRD-051`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-051` |
| Judul | Layar Formulir Uji Klinis / Kredensial Medis & Hak Veto Unit Kerja |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-12` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-077`, `FR-REC-06`, `VR-REC-11`, `VR-REC-12`, `BE-HRD-057` |
| Contract version | API Contract v8 bagian 14.5 grup `Candidate Evaluation` |
| Dependency | `FE-HRD-049` (✅ Selesai), `BE-HRD-057` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/evaluations/clinical/[id]/page.jsx`, `src/components/view/hr/recruitment/candidate-clinical-exam-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penilaian kompetensi klinis (dokter, perawat, apoteker, penunjang medis) dipimpin langsung oleh Komite Medis/Keperawatan dan Kepala Instalasi terkait. Keselamatan pasien menuntut standar kompetensi yang tidak dapat ditawar.

Task ini mengimplementasikan formulir uji klinis dan penegakan hak veto:
1. **Formulir Uji Klinis & Kredensialing (`/hr/recruitment/evaluations/clinical/[id]`)**:
   - Rubrik penilaian 3 pilar kompetensi:
     - Keterampilan Klinis & Prosedur Medis (skor 0-100)
     - Pemahaman Patient Safety & PPI (skor 0-100)
     - Kesiapan Kredensialing & Etika Profesi (skor 0-100)
   - Kalkulasi otomatis rata-rata skor klinis.
2. **Penegakan Hak Veto Mutlak Unit Kerja (`HRD-DEC-077`)**:
   - Jika Kepala Ruangan/Komite Medis memilih keputusan `NOT_RECOMMENDED`, sistem mengaktifkan status Hak Veto Mutlak unit kerja.
   - Kolom "Alasan Penolakan Klinis / Veto" menjadi wajib diisi (mandatory validation).
   - Dialog konfirmasi veto muncul untuk memastikan keputusan penolakan klinis tidak dapat dianulir oleh bagian rekrutmen non-medis.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Form penilaian uji klinis menyajikan rubrik skor 3 pilar klinis dan rata-rata skor otomatis | ✅ PASS | Terbukti pada `candidate-clinical-exam-view.jsx` dengan state `clinicalSkillsScore`, `patientSafetyScore`, `credentialingScore` dan kalkulator `averageScore`. |
| 2 | Hak Veto Mutlak Unit Kerja (`HRD-DEC-077`) diwajibkan menyertakan alasan penolakan | ✅ PASS | Validasi interaktif memeriksa kolom `vetoReason` jika status `NOT_RECOMMENDED` dipilih sebelum payload dikirimkan. |
| 3 | Terhubung ke endpoint API backend `submitClinicalEvaluation` | ✅ PASS | Terhubung via `RecruitmentService.submitClinicalEvaluation` dengan payload lengkap ke `/v1/corporate/human-resource/recruitment/evaluations/clinical`. |
| 4 | Kepatuhan Desain Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `BaseFormControl`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/candidate-clinical-exam-view.jsx` (Komponen formulir uji klinis dan penegakan veto unit)
- `src/app/hr/recruitment/evaluations/clinical/[id]/page.jsx` (Rute dinamis Next.js App Router)
