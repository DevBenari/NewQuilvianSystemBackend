# Laporan Perubahan Frontend — `FE-HRD-065`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-065` |
| Judul | Layar Daftar & Detail Evaluasi Masa Percobaan (Probation) Pegawai Rumah Sakit (FE-HRD-128, FE-HRD-129) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-17` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-094, VR-ONB-55 s.d. VR-ONB-59, BE-HRD-079` |
| Contract version | API Contract v9 bagian 15.3 (probations) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-079 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/probations-list-view.jsx, src/components/view/hr/lifecycle/probation-detail-view.jsx, src/app/hr/lifecycle/probations/page.jsx, src/app/hr/lifecycle/probations/[id]/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pemantauan masa percobaan calon staf tetap rumah sakit dengan ketetapan durasi maksimal 3 bulan tanpa opsi perpanjangan sesuai hukum ketenagakerjaan.

Task ini mengimplementasikan:
1. **Layar Daftar Masa Percobaan (/hr/lifecycle/probations)**:
   - Menampilkan daftar staf yang sedang dalam masa percobaan, tanggal awal dan akhir (tepat 3 bulan), skor evaluasi tengah dan akhir, serta status persetujuan.
2. **Layar Detail & Penilaian Evaluasi Masa Percobaan (/hr/lifecycle/probations/[id])**:
   - Rubrik penilaian kinerja mencakup kompetensi teknis, kepatuhan SPO RS, etika kerja, kedisiplinan, dan orientasi pelayanan pasien.
   - Pilihan keputusan akhir:
     - **LULUS (Passed)**: Mengusulkan penerbitan SK Pegawai Tetap.
     - **TIDAK LULUS (Failed)**: Mewajibkan unggahan berkas berita acara penilaian tidak lulus (VR-ONB-59) sebelum tindakan pengakhiran hubungan kerja disahkan.
   - Penolakan perpanjangan masa percobaan (VR-ONB-57): Sistem tidak menyediakan tombol perpanjangan waktu karena masa percobaan dilarang melebihi 3 bulan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Durasi masa percobaan dikunci tepat 3 bulan tanpa perpanjangan (VR-ONB-55 s.d. VR-ONB-57) | ✅ PASS | Tampilan tanggal dan kalkulasi periode terkunci pada ProbationDetailView. |
| 2 | Keputusan tidak lulus mewajibkan lampiran bukti evaluasi (VR-ONB-59) | ✅ PASS | Modal keputusan tidak lulus memvalidasi kewajiban upload berkas bukti sebelum submit. |
| 3 | Terhubung ke endpoint evaluasi probation backend | ✅ PASS | Terhubung via LifecycleService.evaluateProbation. |
| 4 | Bebas Tailwind dan menerapkan design token Quilvian | ✅ PASS | Menggunakan layout CSS Module terstruktur dengan status badge resmi. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/probations-list-view.jsx (Tabel daftar probation & filter status)`
- `src/components/view/hr/lifecycle/probation-detail-view.jsx (Detail penilaian & modal keputusan hasil)`
- `src/app/hr/lifecycle/probations/page.jsx (Rute App Router daftar probation)`
- `src/app/hr/lifecycle/probations/[id]/page.jsx (Rute App Router detail probation)`
