# Laporan Perubahan Frontend — `FE-HRD-072`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-072` |
| Judul | Penyelarasan Pilihan Jenis Kontrak & Durasi Maksimal 5 Tahun pada Form Penawaran Kerja (Job Offer) |
| Slice | `S-D2 · amandemen MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-095, VR-ONB-35 s.d. VR-ONB-37, BE-HRD-064, FE-HRD-053` |
| Contract version | API Contract v9 bagian 15.2 & modul penawaran kerja |
| Dependency | `FE-HRD-053 (✅ Selesai), BE-HRD-064 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/recruitment/job-offers-list-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penyelarasan jenis perjanjian kerja pada tahap rekrutmen penawaran kerja (*Job Offer*) agar selaras 100% dengan jenis kontrak yang didukung pada modul onboarding dan kontrak rumah sakit.

Task ini mengimplementasikan:
1. **Penyelarasan Dropdown Jenis Kontrak**:
   - Memastikan pilihan pada form penawaran mencakup: PKWT (Waktu Tertentu), PKWTT (Waktu Tidak Tertentu), Mitra Klinis Paruh Waktu, Konsultan Spesialis, dan Magang/Fellowship.
2. **Validasi Durasi Kontrak (1 s.d. 60 Bulan)**:
   - Sesuai PP No. 35/2021 dan aturan bisnis RS (VR-ONB-37), durasi awal penawaran kerja PKWT dibatasi maksimal 60 bulan (5 tahun). Input durasi divalidasi ketat dengan pesan error yang jelas jika pengguna memasukkan angka di luar rentang 1 s.d. 60 bulan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Pilihan jenis kontrak selaras dengan enum kontrak lifecycle (HRD-DEC-095) | ✅ PASS | JobOffersListView menyajikan opsi jenis kontrak yang kompatibel penuh. |
| 2 | Validasi durasi kontrak menolak nilai di atas 60 bulan (VR-ONB-37) | ✅ PASS | Pemeriksaan form menolak angka kurang dari 1 atau lebih dari 60 bulan. |
| 3 | Perhitungan tanggal berakhir kontrak otomatis dihitung dari durasi | ✅ PASS | Sistem memperbarui proyeksi tanggal akhir kontrak berdasarkan tanggal mulai + durasi. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/job-offers-list-view.jsx (Validasi pilihan jenis kontrak & batas 60 bulan)`
