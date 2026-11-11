# Laporan Perubahan Frontend — `FE-HRD-073`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-073` |
| Judul | Integrasi Tipe Rujukan Persetujuan Kontrak, Evaluasi Masa Percobaan, dan Batch Data Awal (T15-T18) pada Kotak Masuk Persetujuan |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-17` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-091, HRD-DEC-094, HRD-DEC-096, VR-ONB-38, BE-HRD-068` |
| Contract version | API Contract v9 bagian 15.9 (approval-inbox reference mappings) |
| Dependency | `FE-HRD-064 (✅ Selesai), FE-HRD-065 (✅ Selesai), FE-HRD-067 (✅ Selesai), BE-HRD-068 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/workflow/approval-inbox-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penyatuan seluruh alur persetujuan (*workflow approvals*) modul kepegawaian ke dalam satu Kotak Masuk Persetujuan Terpadu (*Unified Approval Inbox*) untuk kemudahan atasan dan direksi rumah sakit.

Task ini mengimplementasikan:
1. **Pemetaan 4 Jenis Tugas Baru (T15 s.d. T18)**:
   - **T15**: Persetujuan Penerbitan & Penandatanganan Kontrak Kerja Pegawai Baru / Perpanjangan.
   - **T16**: Persetujuan Pengangkatan Staf Tetap (PKWTT) / Pengakhiran Masa Percobaan.
   - **T17**: Pengesahan Hasil Kelayakan Mandiri Preceptorship Klinis Keperawatan/Medis.
   - **T18**: Persetujuan Eksekusi Komitmen Batch Data Awal Kontrak Kerja.
2. **Penerapan Aturan Anti-Self-Approval**:
   - Jika pengguna aktif adalah pegawai yang menjadi subjek persetujuan atau permohonan, tombol aksi Setuju/Tolak dinonaktifkan secara visual dan diberikan lencana "Anti-Self-Approval Terkunci" demi akuntabilitas audit tata kelola rumah sakit.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tipe rujukan T15 s.d. T18 dipetakan ke label dan rute yang benar | ✅ PASS | ApprovalInboxView menyajikan badge tipe dan navigasi detail sesuai rujukan. |
| 2 | Aturan anti-self-approval mengunci aksi persetujuan sendiri | ✅ PASS | Pemeriksaan ID pemohon mencegah persetujuan mandiri secara visual dan logic. |
| 3 | Aksi persetujuan dan penolakan memanggil endpoint workflow API yang sah | ✅ PASS | Terhubung ke service workflow persetujuan terintegrasi. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/workflow/approval-inbox-view.jsx (Pemetaan tipe rujukan T15-T18 & validasi anti-self-approval)`
