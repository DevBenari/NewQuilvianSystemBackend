# Laporan Perubahan Frontend — `FE-HRD-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-015` |
| Judul | Penyediaan rute dan layar registrasi & monitoring pelatihan wajib staf RS |
| Slice | `S-C2` (Pelatihan Wajib & Sertifikasi Staf RS) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-009`, `FE-HRD-015` |
| Contract version | Learning & Certification Management API v1 |
| Dependency | `BE-HRD-012` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/learning/training-records/`, `src/components/view/hr/learning/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Sesuai standar akreditasi rumah sakit (KARS/JCI), seluruh tenaga kesehatan dan pegawai rumah sakit diwajibkan mengikuti pelatihan keselamatan dan kompetensi rutin secara berkala, seperti:
- Bantuan Hidup Dasar (BHD / Basic Life Support)
- Pencegahan dan Pengendalian Infeksi (PPI)
- Keselamatan Pasien Rumah Sakit (KPRS)
- Keselamatan dan Kesehatan Kerja Rumah Sakit (K3RS) / Penanggulangan Kebakaran

Task ini menyediakan antarmuka terpusat di alamat `/hr/learning/training-records`:
1. **Dasbor Kepatuhan Sertifikasi**: Menampilkan jumlah pegawai bersertifikat aktif, sertifikasi yang akan kedaluwarsa, dan kepatuhan pelatihan wajib.
2. **Tabel Rekam Pelatihan Staf**: Kolom nama staf, jenis pelatihan/sertifikasi, tanggal pelatihan, lembaga penyelenggara, tanggal kedaluwarsa sertifikat, dan status validitas.
3. **Modal Pendaftaran Pelatihan / Sertifikat Baru**: Dialog interaktif untuk mendaftarkan rekam keikutsertaan pelatihan staf baru beserta nomor sertifikatnya.

---

## 1. Wewenang UI yang Dipakai

Rute pelatihan & sertifikasi HR: `src/app/hr/learning/training-records/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan sertifikasi | `SummaryCards` | `REUSE` | Total pelatihan aktif dan kadaluwarsa |
| Filter & pencarian | `DataFilter` | `REUSE` | Filter nama staf atau jenis pelatihan |
| Tabel sertifikasi | `DataTable` | `REUSE` | Kolom ID, Pegawai, Pelatihan, Penyelenggara, Kedaluwarsa, Status |
| Status sertifikat | `StatusBadge` | `REUSE` | Badge Valid / Expiring Soon / Expired |
| Modal rekam pelatihan baru | `ConfirmModal` | `REUSE` | Form input rekam pelatihan dan sertifikasi baru |
| Notifikasi feedback | `ToastStack` | `REUSE` | Umpan balik notifikasi pengguna |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/learning-development-service.js`: Layanan pemanggilan API daftar rekam pelatihan dan pembuatan rekam pelatihan baru.
* `src/components/view/hr/learning/training-certification-view.jsx`: Tampilan dasbor pelatihan & sertifikasi HR terintegrasi modal.
* `src/app/hr/learning/training-records/page.jsx`: Rute Next.js untuk halaman Pelatihan Wajib & Sertifikasi Staf.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service learning |
| Validasi Modal Sertifikat | `PASS` | Memastikan tanggal sertifikat dan nama pelatihan terisi valid |
| Komunikasi API Backend | `PASS` | Terhubung ke controller learning & development `BE-HRD-012` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar monitoring pelatihan wajib dan sertifikasi staf RS | Terpenuhi | Halaman `/hr/learning/training-records` aktif |
| 2. Menampilkan informasi kedaluwarsa sertifikat kompetensi staf | Terpenuhi | Tersaji pada DataTable dan SummaryCards |
| 3. Petugas HR dapat mencatat keikutsertaan pelatihan staf baru | Terpenuhi | Modal ConfirmModal terhubung ke `createTrainingRecord` |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-015` selesai secara tuntas dan telah menyediakan instrumen pemantauan kepatuhan pelatihan akreditasi staf rumah sakit.
