# Laporan Perubahan Frontend — `FE-HRD-049`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-049` |
| Judul | Layar Detail Pelamar & Penapisan STR Klinis Otomatis |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-12` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 S-D2 |
| Trace | `HRD-DEC-076`, `FR-REC-04`, `VR-REC-07`, `VR-REC-08`, `AT-HRD-D2-05`, `UAT-REC-02`, `BE-HRD-055` |
| Contract version | API Contract v8 bagian 14.4 grup `Candidate Screening` |
| Dependency | `FE-HRD-048` (✅ Selesai), `BE-HRD-055` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/candidates/[id]/page.jsx`, `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penapisan berkas kualifikasi nakes (Surat Tanda Registrasi / STR) di lingkungan rumah sakit adalah gerbang keselamatan pasien mutlak. Kandidat klinis dengan STR yang kedaluwarsa atau tidak terdaftar wajib gugur secara otomatis (*auto-reject*) tanpa intervensi manual.

Task ini mengimplementasikan antarmuka penapisan STR:
1. **Profil Rincian Kandidat & Berkas Unggahan (`/hr/recruitment/candidates/[id]`)**:
   - Menampilkan biodata lengkap, kontak, status seleksi, dan daftar seluruh dokumen pendukung (CV, STR, Ijazah, KTP, Sertifikat ACLS/BTCLS).
2. **Penegakan Gerbang STR Kedaluwarsa (`HRD-DEC-076`)**:
   - Sistem secara otomatis membandingkan tanggal akhir masa berlaku STR (`validUntil`) terhadap tanggal hari ini.
   - Jika STR telah kedaluwarsa, layar menampilkan banner merah tegas: *"PERINGATAN GUGUR OTOMATIS (HRD-DEC-076): Masa berlaku STR pelamar telah kedaluwarsa. Posisi klinis otomatis ditolak oleh sistem dan tombol penjadwalan wawancara/uji klinis dinonaktifkan permanen"*.
3. **Modal Unggah Dokumen / STR Baru**:
   - Memungkinkan pengunggahan berkas digital dengan pencatatan nomor registrasi dan tanggal masa berlaku.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan berkas lamaran, nomor dokumen, dan tanggal kedaluwarsa STR | ✅ PASS | Tabel dokumen pada `candidate-detail-screening-view.jsx` menyajikan detail tipe, nomor, masa berlaku, dan status verifikasi. |
| 2 | Menampilkan banner merah dan menonaktifkan aksi lanjut jika STR kedaluwarsa (`HRD-DEC-076`) | ✅ PASS | Banner `vetoAlertBanner` tampil otomatis dan tombol *"1. Formulir Wawancara HR"* serta *"2. Formulir Uji Klinis"* terpasang `disabled={isStrExpired}`. |
| 3 | Modal pengunggahan dokumen mendukung tipe STR, CV, KTP, dan Ijazah | ✅ PASS | Form modal `docForm` terhubung ke `RecruitmentService.uploadDocument`. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `StatusBadge`, `DataTable`, `Hero`, `BaseFormControl`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` (Komponen antarmuka detail & penapisan STR)
- `src/app/hr/recruitment/candidates/[id]/page.jsx` (Rute dinamis Next.js App Router)
