# Laporan Perubahan Frontend — `FE-HRD-061`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-061` |
| Judul | Aksi Checklist Orientasi 3-Lapis, Rekam Kehadiran Hari Pertama, & Unggah Bukti Valid |
| Slice | `S-C4-ONB` (Onboarding dan Kontrak Kerja Pegawai RS) · `MVP-15` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-086`, `HRD-DEC-087`, `HRD-DEC-088`, `VR-ONB-03` s.d. `VR-ONB-10`, `VR-ONB-17`, `BE-HRD-075` |
| Contract version | API Contract v9 bagian 15.1 (checklist actions, attendance) |
| Dependency | `FE-HRD-060` (✅ Selesai), `BE-HRD-075` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pelaksanaan orientasi menuntut bukti akuntabel bahwa setiap instruksi kerja, fasilitas kartu akses, dan pelatihan SPO keselamatan pasien benar-benar telah diselesaikan oleh staf bersangkutan.

Task ini mengimplementasikan aksi interaktif pada layar kasus onboarding:
1. **Konfirmasi Kehadiran Hari Pertama**:
   - Tombol "Konfirmasi Kehadiran Hari Pertama" mencatat jam kedatangan pegawai di bagian HRD dan mengubah status kasus dari `PREPARATION` menjadi `FIRST_DAY_ATTENDED`.
2. **Penyelesaian Butir Checklist & Unggah Bukti Sah (`VR-ONB-09`)**:
   - Butir yang mensyaratkan bukti lampiran (`requiresProof = true`) membuka modal unggah berkas.
   - Sistem memvalidasi format berkas (hanya PDF, JPG, PNG) dan membatasi ukuran maksimal 5 MB sebelum payload dikirimkan.
   - Pengecekan izin per lapis: Butir HRD membutuhkan izin `CompleteItem`, butir unit membutuhkan izin `CompleteUnitItem`.
3. **Penyelarasan Pengalihan Preceptor & Atasan**:
   - Pemilihan atasan langsung dan preceptor unit hanya menampilkan daftar staf klinis aktif di unit tersebut.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Butir wajib bukti menolak dicentang sebelum berkas dipilih (`VR-ONB-09`) | ✅ PASS | Terbukti pada fungsi `handleToggleItem` dan `handleSubmitProof` di `onboarding-case-detail-view.jsx`. |
| 2 | Format dan ukuran berkas divalidasi ketat (PDF/JPG/PNG <= 5MB) | ✅ PASS | Validasi interaktif menolak tipe file lain atau ukuran melebihi 5MB. |
| 3 | Aksi centang/batal centang terhubung ke endpoint API backend | ✅ PASS | Terhubung via `LifecycleService.completeChecklistItem` dan `uncheckChecklistItem`. |
| 4 | Kepatuhan Desain Bebas Tailwind | ✅ PASS | Tampilan form modal dan checklist 100% menggunakan CSS Module variabel token Quilvian. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx` (Komponen interaktif aksi checklist dan upload bukti)
