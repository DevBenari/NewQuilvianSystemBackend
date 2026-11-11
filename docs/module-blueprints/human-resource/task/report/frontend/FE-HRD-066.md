# Laporan Perubahan Frontend — `FE-HRD-066`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-066` |
| Judul | Layar Daftar & Detail Masa Orientasi / Pendampingan Klinis (Preceptorship) (FE-HRD-130, FE-HRD-131) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-15` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-090, VR-ONB-60 s.d. VR-ONB-63, BE-HRD-073` |
| Contract version | API Contract v9 bagian 15.4 (clinical-preceptorships) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-073 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/clinical-preceptorships-list-view.jsx, src/components/view/hr/lifecycle/clinical-preceptorship-detail-view.jsx, src/app/hr/lifecycle/clinical-preceptorships/page.jsx, src/app/hr/lifecycle/clinical-preceptorships/[id]/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pemantauan program bimbingan klinis (*clinical preceptorship*) bagi tenaga perawat, bidan, dan nakes baru oleh pembimbing klinis senior sebelum dilepas memberikan asuhan mandiri.

Task ini mengimplementasikan:
1. **Layar Daftar Pendampingan Klinis (/hr/lifecycle/clinical-preceptorships)**:
   - Filter berdasarkan unit rawat inap/jalan/IGD/ICU, nama preceptor, nama preceptee, dan status kelulusan klinis.
2. **Layar Detail Pendampingan Klinis (/hr/lifecycle/clinical-preceptorships/[id])**:
   - Pemantauan logbook kompetensi klinis harian dan target prosedur medis yang harus diobservasi.
   - **Aksi Kelayakan Praktik Mandiri (Declare Independent Practice)**: Mewajibkan unggahan sertifikat atau lembar rekomendasi evaluasi preceptor yang telah ditandatangani Komite Keperawatan/Tenaga Kesehatan (VR-ONB-62).
   - **Aksi Pengalihan Pembimbing Klinis (Reassign Preceptor)**: Hanya dapat memilih staf klinis aktif di unit yang sama (VR-ONB-63) dengan alasan penggantian formal.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Pernyataan praktik mandiri mewajibkan bukti rekomendasi preceptor (VR-ONB-62) | ✅ PASS | Modal rilis praktik mandiri memvalidasi unggahan dokumen evaluasi klinis. |
| 2 | Pengalihan preceptor dibatasi pada staf klinis aktif di unit bersangkutan (VR-ONB-63) | ✅ PASS | Dropdown pemilihan preceptor baru terfilter berdasarkan unit penugasan. |
| 3 | Terhubung ke endpoint clinical preceptorships backend | ✅ PASS | Terhubung via LifecycleService.declareIndependentPractice dan reassignPreceptor. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/clinical-preceptorships-list-view.jsx (Tabel daftar preceptorship klinis)`
- `src/components/view/hr/lifecycle/clinical-preceptorship-detail-view.jsx (Detail kompetensi, modal rilis mandiri, & alih preceptor)`
- `src/app/hr/lifecycle/clinical-preceptorships/page.jsx (Rute App Router daftar preceptorship)`
- `src/app/hr/lifecycle/clinical-preceptorships/[id]/page.jsx (Rute App Router detail preceptorship)`
