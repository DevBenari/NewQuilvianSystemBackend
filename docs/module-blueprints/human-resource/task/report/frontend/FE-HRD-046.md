# Laporan Perubahan Frontend — `FE-HRD-046`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-046` |
| Judul | Layar Pengelolaan Lowongan Kerja (Job Vacancies) |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-11` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 S-D2 |
| Trace | `HRD-DEC-073`, `HRD-DEC-074`, `FR-REC-02`, `VR-REC-04`, `BE-HRD-053` |
| Contract version | API Contract v8 bagian 14.2 grup `Job Vacancy` |
| Dependency | `FE-HRD-044` (✅ Selesai), `BE-HRD-053` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/vacancies/page.jsx`, `src/components/view/hr/recruitment/vacancies-list-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Lowongan pekerjaan rumah sakit memerlukan penegakan ketat mengenai kategori posisi: apakah posisi tersebut merupakan tenaga klinis/medis (dokter, perawat, apoteker, analis medis) yang mensyaratkan kepemilikan STR aktif, atau posisi non-klinis (administrasi, TI, keuangan).

Task ini mengimplementasikan antarmuka pengelolaan lowongan:
1. **Daftar Lowongan Kerja (`/hr/recruitment/vacancies`)**:
   - Menampilkan nomor lowongan, judul posisi, penanda kategori klinis (`IsClinical`), kuota kursi, status tayang, dan periode pendaftaran.
   - Pembeda visual StatusBadge merah *"Klinis / Medis (Wajib STR)"* vs badge biru *"Non-Klinis"*.
   - Filter segmented pencarian dan penyaringan kategori peran medis.
2. **Modal Pembukaan Lowongan Baru**:
   - Formulir input pemilihan Requisition approved, judul posisi, kuota kursi, checkbox penanda klinis, tanggal penutupan, dan deskripsi kualifikasi.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tabel lowongan menyajikan Nomor, Judul, Kuota, Kategori Klinis, Status, dan Tanggal Berakhir | ✅ PASS | Terbukti pada `vacancies-list-view.jsx` dengan kolom `vacancyNumber`, `title`, `isClinical`, `quotaSeats`, `status`, dan `dates`. |
| 2 | Kolom peran menampilkan badge visual pembeda posisi medis/klinis (`IsClinical`) vs non-klinis | ✅ PASS | StatusBadge merah/biru `row.isClinical ? "danger" : "info"` terbukti pada kolom `isClinical`. |
| 3 | Tombol aksi terhubung ke endpoint API backend | ✅ PASS | Terhubung via `RecruitmentService.createVacancy` dan navigasi rute ke detail lowongan. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `StatusBadge`, `DataTable`, `Hero`, `BaseFormControl`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/vacancies-list-view.jsx` (Komponen antarmuka list)
- `src/app/hr/recruitment/vacancies/page.jsx` (Rute Next.js App Router)
- `src/utils/menu-sidebar/menu-items.jsx` (Butir menu `Lowongan Kerja`)
