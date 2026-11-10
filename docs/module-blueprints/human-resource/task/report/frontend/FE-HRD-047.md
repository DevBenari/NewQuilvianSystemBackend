# Laporan Perubahan Frontend — `FE-HRD-047`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-047` |
| Judul | Layar Detail Lowongan & Pelamar Masuk |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-11` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-073`, `HRD-DEC-074`, `FR-REC-02`, `BE-HRD-053`, `BE-HRD-054` |
| Contract version | API Contract v8 bagian 14.2 & 14.3 |
| Dependency | `FE-HRD-046` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/vacancies/[id]/page.jsx`, `src/components/view/hr/recruitment/vacancy-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pengelola rekrutmen membutuhkan layar untuk memantau detail kualifikasi suatu lowongan kerja, mengontrol status publikasi (terbitkan / tutup lowongan), dan meninjau daftar pelamar yang masuk secara bertahap.

Task ini mengimplementasikan layar rincian lowongan:
1. **Profil Kualifikasi Lowongan (`/hr/recruitment/vacancies/[id]`)**:
   - Menampilkan nomor dokumen lowongan, nomor requisition WFP induk, kuota kursi, kategori klinis, tanggal tayang, batas penutupan, dan uraian kualifikasi.
   - Aksi publikasi draf lowongan (*"Publikasikan Lowongan"*) dan aksi penutupan (*"Tutup Lowongan"* dengan modal alasan penutupan opsional).
2. **Sub-Tabel Segmented Pelamar Masuk**:
   - Menampilkan daftar kandidat pelamar yang masuk untuk posisi ini.
   - Tab segmented filter: *Semua Pelamar*, *STR Valid*, dan *STR Gugur / Kedaluwarsa*.
   - Tombol tautan *"Lihat Berkas STR"* yang langsung membuka profil penapisan dokumen kandidat (`/hr/recruitment/candidates/[id]`).

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan profil lowongan, kuota formasi, dan tanggal tayang | ✅ PASS | Terbukti pada `vacancy-detail-view.jsx` dengan panel profil lengkap. |
| 2 | Menampilkan sub-tabel pelamar dengan status tahapan saat ini | ✅ PASS | Sub-tabel `applicantColumns` menyajikan nama kandidat, nomor aplikasi, hasil penapisan STR, masa berlaku STR, dan tanggal penapisan. |
| 3 | Tautan klik pada baris pelamar langsung membuka detail profil pelamar terkait | ✅ PASS | Tombol *"Lihat Berkas STR"* mengarahkan ke `/hr/recruitment/candidates/[candidateId]`. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `StatusBadge`, `DataTable`, `Hero`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/vacancy-detail-view.jsx` (Komponen antarmuka detail lowongan & pelamar)
- `src/app/hr/recruitment/vacancies/[id]/page.jsx` (Rute dinamis Next.js App Router)
