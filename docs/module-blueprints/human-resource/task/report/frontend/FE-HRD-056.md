# Laporan Perubahan Frontend — `FE-HRD-056`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-056` |
| Judul | Layar Finalisasi & Serah Terima Calon Karyawan ke Modul Onboarding (Hiring Handoff) |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-13` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-080`, `FR-REC-10`, `VR-REC-20`, `BE-HRD-061` |
| Contract version | API Contract v8 bagian 14.9 grup `Hiring Handoffs` |
| Dependency | `FE-HRD-053` (✅ Selesai), `FE-HRD-054` (✅ Selesai), `BE-HRD-061` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/handoffs/page.jsx`, `src/components/view/hr/recruitment/hiring-handoffs-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Setelah pelamar menyetujui penawaran kerja dan dinyatakan sehat oleh dokter K3RS, proses rekrutmen ditutup dengan penyerahan berkas calon pegawai ke tim Personalia & Onboarding (*Hiring Handoff*) untuk pembuatan NIP dan penyiapan orientasi kerja.

Task ini mengimplementasikan layar gerbang serah terima onboarding:
1. **Daftar Calon Karyawan Siap Orientasi (`/hr/recruitment/handoffs`)**:
   - Menampilkan kandidat yang telah menyelesaikan seluruh tahapan seleksi beserta status kesiapan serah terima.
2. **Penegakan Gerbang Ketat Serah Terima (`VR-REC-20` & `HRD-DEC-080`)**:
   - Sistem secara ketat memeriksa 2 prasyarat mutlak:
     1. Status Penawaran Kerja: **Wajib `ACCEPTED`** (Surat penawaran telah ditandatangani).
     2. Status Kelaikan Medis: **Wajib `FIT` atau `FIT_WITH_RESTRICTIONS`** (Lulus MCU K3RS).
   - Bila salah satu prasyarat belum terpenuhi, tombol serah terima (*Hand Off*) otomatis dinonaktifkan dengan pesan peringatan persyaratan belum lengkap.
3. **Penyusunan Paket Integrasi Karyawan Baru**:
   - Form penentuan tanggal hari pertama masuk kerja (Start Date), unit kerja penempatan, mentor orientasi, dan opsi pembuatan akun NIP otomatis ke modul Onboarding.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan daftar calon pegawai yang siap diserahterimakan dengan indikator status Offer dan MCU | ✅ PASS | Terbukti pada `hiring-handoffs-view.jsx` menyajikan kolom status surat penawaran dan status kelaikan medis. |
| 2 | Penegakan aturan gerbang serah terima (`VR-REC-20`) mencegah handoff jika syarat belum lengkap | ✅ PASS | Tombol handoff dikunci disabled dan menyajikan tooltip/peringatan jika Offer != ACCEPTED atau MCU != FIT. |
| 3 | Eksekusi serah terima terhubung ke endpoint API backend `handoffs` | ✅ PASS | Terhubung via `RecruitmentService.createHandoff` dengan payload ID kandidat, tanggal orientasi, dan catatan penempatan. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `DataTable`, `InformationAlert`, `StatusBadge`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/hiring-handoffs-view.jsx` (Komponen serah terima rekrutmen ke onboarding)
- `src/app/hr/recruitment/handoffs/page.jsx` (Rute Next.js App Router)
