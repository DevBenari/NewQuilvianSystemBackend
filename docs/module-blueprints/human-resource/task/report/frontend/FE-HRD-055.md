# Laporan Perubahan Frontend — `FE-HRD-055`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-055` |
| Judul | Layar Direktori Talent Pool & Jalur Cepat (Fast-Track) Promosi Pelamar Potensial |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-13` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `FR-REC-09`, `VR-REC-19`, `BE-HRD-060` |
| Contract version | API Contract v8 bagian 14.8 grup `Talent Pool` |
| Dependency | `FE-HRD-048` (✅ Selesai), `BE-HRD-060` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/talent-pool/page.jsx`, `src/components/view/hr/recruitment/talent-pool-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Kandidat yang memiliki kompetensi baik namun kuota formasi lowongan telah terpenuhi disimpan ke dalam *Talent Pool* cadangan agar dapat direkrut cepat (*fast-track*) di masa mendatang tanpa perlu mengulang seleksi administrasi awal.

Task ini mengimplementasikan direktori talent pool rumah sakit:
1. **Bank Bakat Calon Tenaga Kesehatan & Non-Kesehatan (`/hr/recruitment/talent-pool`)**:
   - Menampilkan daftar pelamar potensial yang masuk kategori cadangan (*Considered for Talent Pool*).
   - Pencarian berdasarkan nama, keahlian khusus, pengalaman kerja, dan unit peminatan.
   - Tag keahlian (*Skill Tags*) dan indikator masa berlaku data kandidat di talent pool (misal: 6 bulan s.d. 1 tahun).
2. **Jalur Cepat Promosi Pelamar ke Lowongan Baru (*Fast-Track Re-engagement*)**:
   - Tombol aksi cepat mempromosikan talenta langsung ke tahapan wawancara user atau penawaran kerja saat ada lowongan darurat atau ekspansi bangsal baru.
   - Rekam alasan pemanggilan kembali dan penugasan ke nomor formasi lowongan aktif yang membutuhkan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Direktori talent pool menampilkan profil kandidat potensial dengan tag keahlian dan status cadangan | ✅ PASS | Terbukti pada `talent-pool-view.jsx` dengan `DataTable`, badge keahlian, dan filter kualifikasi. |
| 2 | Aksi jalur cepat (Fast-Track) menghubungkan kandidat ke lowongan aktif baru | ✅ PASS | Terhubung via `RecruitmentService.activateTalentPoolCandidate` dengan pemilihan target lowongan. |
| 3 | Redux state terintegrasi untuk pembacaan dan pembaruan data talent pool | ✅ PASS | Menggunakan `recruitment-slice.js` dengan thunk `fetchTalentPoolCandidates` dan pembaruan instan. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `DataTable`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/talent-pool-view.jsx` (Komponen direktori talent pool dan modal re-engagement cepat)
- `src/app/hr/recruitment/talent-pool/page.jsx` (Rute Next.js App Router)
