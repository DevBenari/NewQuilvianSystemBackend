# Laporan Perubahan Frontend — `FE-HRD-052`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-052` |
| Judul | Layar Direktori Penawaran Kerja (Job Offers) & Deteksi Plafon Gaji |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-12` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-079`, `FR-REC-07`, `VR-REC-13`, `VR-REC-14`, `BE-HRD-058` |
| Contract version | API Contract v8 bagian 14.6 grup `Job Offers` |
| Dependency | `FE-HRD-048` (✅ Selesai), `BE-HRD-058` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/offers/page.jsx`, `src/components/view/hr/recruitment/job-offers-list-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penawaran kerja formal (*Job Offer Letter*) mengatur besaran remunerasi pokok, tunjangan keahlian, tanggal mulai bertugas, dan batas waktu konfirmasi kandidat. 

Task ini mengimplementasikan direktori penawaran kerja beserta kepatuhan plafon gaji:
1. **Daftar Penawaran Kerja Terintegrasi (`/hr/recruitment/offers`)**:
   - Menampilkan nomor surat penawaran, nama kandidat, formasi lowongan, grade gaji, take home pay bulanan, dan status persetujuan.
   - Filter status penawaran: `DRAFT`, `PENDING_APPROVAL`, `APPROVED`, `OFFER_RELEASED`, `ACCEPTED`, `DECLINED`, `EXPIRED`.
2. **Peringatan Otomatis Pelanggaran Plafon Gaji (`HRD-DEC-079`)**:
   - Jika nilai gaji pokok atau benefit yang ditawarkan melampaui batas atas (*ceiling*) struktur skala upah rumah sakit, kartu dan baris penawaran ditandai dengan peringatan visual (Peringatan Plafon Gaji Terlampaui).
   - Menampilkan eskalasi jalur persetujuan dari Tier 13 (Direktur SDM) menjadi Tier 14 (Direktur Keuangan & Direktur Utama).
3. **Modal Pembuatan Draft Penawaran Baru**:
   - Form pembuatan draft penawaran baru dengan kalkulasi penyesuaian tier otomatis saat input angka gaji.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Direktori penawaran kerja menyajikan tabel data lengkap dengan filter status dan pencarian | ✅ PASS | Terbukti pada `job-offers-list-view.jsx` menggunakan `DataTable` dan Redux state `offers`. |
| 2 | Peringatan plafon gaji (`HRD-DEC-079`) menyala ketika penawaran melampaui batas grade | ✅ PASS | Peringatan visual dan badge tier persetujuan Tier 14 otomatis aktif jika nominal melebihi plafon. |
| 3 | Modal pembuatan draft penawaran terhubung ke API backend | ✅ PASS | Terhubung via `RecruitmentService.createOffer` dengan payload ID kandidat, formasi, dan komponen gaji. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `DataTable`, `InformationAlert`, `BaseFormControl`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/job-offers-list-view.jsx` (Komponen direktori dan modal draft penawaran kerja)
- `src/app/hr/recruitment/offers/page.jsx` (Rute Next.js App Router)
