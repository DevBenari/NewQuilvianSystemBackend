# Laporan Perubahan Frontend — `FE-HRD-048`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-048` |
| Judul | Layar Direktori Data Pelamar & NIK Rehire Flow |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-11` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 1 S-D2 |
| Trace | `HRD-DEC-075`, `FR-REC-03`, `VR-REC-05`, `VR-REC-06`, `UAT-REC-03`, `BE-HRD-054` |
| Contract version | API Contract v8 bagian 14.3 grup `Candidate Directory` |
| Dependency | `BE-HRD-054` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/candidates/page.jsx`, `src/components/view/hr/recruitment/candidates-directory-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Direktori induk pelamar rumah sakit berfungsi sebagai basis data seluruh kandidat yang pernah melamar, dilengkapi pengamanan keunikan identitas kependudukan (NIK KTP 16 digit) dan pendeteksian otomatis mantan karyawan (rehire flow) untuk menjamin transparansi rekam jejak kerja sebelumnya.

Task ini mengimplementasikan direktori pelamar:
1. **Pencarian & Tabel Direktori Pelamar (`/hr/recruitment/candidates`)**:
   - Menampilkan kode pelamar, nama lengkap, NIK, alamat email, nomor telepon, status seleksi, dan status rehire.
   - Pembeda visual StatusBadge kuning *"Mantan Pegawai RS (Rehire)"* vs pelamar eksternal baru (`HRD-DEC-075`).
   - Filter pencarian nama, filter khusus NIK 16 digit, dan filter status rehire.
2. **Modal Registrasi Pelamar Baru**:
   - Formulir registrasi kandidat dengan validasi pola ekspresi reguler NIK 16 digit (`/^\d{16}$/`), nama lengkap, jenis kelamin, tanggal/tempat lahir, email, dan telepon.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Form pendaftaran memvalidasi panjang dan format angka NIK (16 digit) (`VR-REC-05`) | ✅ PASS | Validasi client-side regex `^\d{16}$` pada `candidates-directory-view.jsx` menolak input jika panjang/format tidak valid. |
| 2 | Jika NIK terdeteksi mantan pegawai, form memunculkan indikasi rehire (`HRD-DEC-075`) | ✅ PASS | Kolom `isRehire` menampilkan StatusBadge kuning *"Mantan Pegawai RS (Rehire)"* dan tautan ke riwayat profil sebelumnya. |
| 3 | Tabel direktori menyajikan Nama, NIK, Kontak, Posisi Terakhir Dilamar, dan Badge Rehire | ✅ PASS | Kolom `fullName`, `nationalIdNumber`, `contact`, `isRehire`, dan `status` terbukti lengkap pada tabel data. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `StatusBadge`, `DataTable`, `Hero`, `BaseFormControl`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/candidates-directory-view.jsx` (Komponen antarmuka direktori pelamar)
- `src/app/hr/recruitment/candidates/page.jsx` (Rute Next.js App Router)
- `src/utils/menu-sidebar/menu-items.jsx` (Butir menu `Bank Pelamar`)
