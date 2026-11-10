# Laporan Perubahan Frontend — `FE-HRD-053`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-053` |
| Judul | Layar Rincian Penawaran Kerja, Persetujuan Bertingkat (T13/T14), & Respon Kandidat |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-13` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-079`, `FR-REC-07`, `VR-REC-15`, `VR-REC-16`, `BE-HRD-058` |
| Contract version | API Contract v8 bagian 14.6 grup `Job Offers` |
| Dependency | `FE-HRD-052` (✅ Selesai), `BE-HRD-058` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/offers/[id]/page.jsx`, `src/components/view/hr/recruitment/job-offer-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penawaran kerja yang disetujui perlu diproses melalui otorisasi struktural direksi sebelum surat resmi dirilis ke kandidat, dilanjutkan pencatatan penerimaan (*acceptance*) atau penolakan (*decline*).

Task ini mengimplementasikan layar rincian penawaran kerja komprehensif:
1. **Rincian Komponen Remunerasi & Syarat Kerja (`/hr/recruitment/offers/[id]`)**:
   - Rincian gaji pokok, tunjangan profesi medik/keperawatan, tunjangan jabatan, fasilitas kesehatan (BPJS & Asuransi Tambahan), dan tanggal mulai efektif bekerja.
2. **Alur Otorisasi Bertingkat (Tier 13 vs Tier 14)**:
   - Menampilkan tahapan approval pejabat rumah sakit:
     - Tier 13 (Standar): Kepala HRD dan Direktur SDM/Umum.
     - Tier 14 (Eskalasi Over-Ceiling): Direktur SDM, Direktur Keuangan, dan Direktur Utama.
   - Tombol aksi persetujuan (*Approve Offer*) atau pengembalian revisi (*Reject / Request Revision*).
3. **Penerbitan Surat Resmi & Rekam Respon Pelamar**:
   - Tombol Rilis Surat Penawaran (*Release Offer Letter*) yang menghasilkan tautan dan dokumen penawaran resmi.
   - Tombol pencatatan respon kandidat: Diterima (*Accepted with Signature Confirmation*) atau Ditolak (*Declined* beserta alasan kandidat).

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Rincian lengkap penawaran kerja disajikan dengan breakdown komponen remunerasi | ✅ PASS | Terbukti pada `job-offer-detail-view.jsx` menampilkan tabel komponen upah dan total estimasi kompensasi. |
| 2 | Menampilkan alur persetujuan bertingkat Tier 13 / Tier 14 berdasarkan nominal plafon | ✅ PASS | Tampilan alur otorisasi mendeteksi flag `overCeiling` dan menyajikan status reviewer terkait. |
| 3 | Aksi Approve, Release Offer, dan Record Response terhubung ke API backend | ✅ PASS | Terhubung via `RecruitmentService.approveOffer`, `releaseOffer`, dan `recordOfferResponse`. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `StatusBadge`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/job-offer-detail-view.jsx` (Komponen rincian penawaran kerja, alur persetujuan, dan pencatatan respon pelamar)
- `src/app/hr/recruitment/offers/[id]/page.jsx` (Rute dinamis Next.js App Router)
