# Laporan Perubahan Frontend — `FE-HRD-054`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-054` |
| Judul | Layar Verifikasi Kelaikan Medis & K3RS (MCU Clearance) dengan Proteksi Kerahasiaan Medis |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-13` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-D2 |
| Trace | `HRD-DEC-078`, `FR-REC-08`, `VR-REC-17`, `VR-REC-18`, `BE-HRD-059` |
| Contract version | API Contract v8 bagian 14.7 grup `Medical Check Up Clearance` |
| Dependency | `FE-HRD-048` (✅ Selesai), `BE-HRD-059` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/mcu-clearance/page.jsx`, `src/components/view/hr/recruitment/mcu-clearance-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pemeriksaan Kesehatan Kerja (Medical Check Up / MCU) bagi calon staf rumah sakit wajib memenuhi standar K3RS (Keselamatan dan Kesehatan Kerja Rumah Sakit) untuk melindungi pasien dan lingkungan kerja, dengan tetap menjaga kerahasiaan data rekam medis pribadi.

Task ini mengimplementasikan verifikasi kelaikan kerja medis dengan perlindungan privasi:
1. **Direktori Hasil Kelaikan Medis Calon Karyawan (`/hr/recruitment/mcu-clearance`)**:
   - Menampilkan daftar calon karyawan yang menjalani pemeriksaan kesehatan pra-kerja.
   - Status kelaikan kerja K3RS: `PENDING_EXAMINATION`, `FIT`, `FIT_WITH_RESTRICTIONS`, `TEMPORARY_UNFIT`, `UNFIT`.
2. **Proteksi & Penyamaran Kerahasiaan Medis (`HRD-DEC-078`)**:
   - Staf HR non-medis **TIDAK DAPAT** melihat catatan diagnosis spesifik laboratorium atau radiologi (dilindungi dengan banner "Diagnosis Medis Dilindungi Hak Privasi Medis K3RS").
   - Yang ditampilkan kepada HR hanyalah status kelaikan kerja umum dan restriksi tugas (misalnya: "Boleh bertugas, hindari angkat beban > 15 kg selama 1 bulan" atau "Wajib memakai APD respiratori level 2").
3. **Formulir Rekam Kelaikan Kerja oleh Dokter K3RS**:
   - Modal input keputusan status kelaikan kerja, batas waktu berlaku, catatan restriksi, dan nomor SIP dokter pemeriksa K3RS.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Direktori MCU menyajikan status kelaikan kerja K3RS calon karyawan secara jelas | ✅ PASS | Terbukti pada `mcu-clearance-view.jsx` menampilkan tabel data calon karyawan dengan badge status kelaikan kerja. |
| 2 | Perlindungan kerahasiaan diagnosis medis (`HRD-DEC-078`) aktif bagi staf HR umum | ✅ PASS | Terbukti diagnosis rahasia ditutup (*masked*) dan hanya memunculkan status fit/unfit beserta restriksi operasional. |
| 3 | Input rekam hasil kelaikan kerja terhubung ke endpoint API backend MCU | ✅ PASS | Terhubung via `RecruitmentService.recordMcuResult` dengan payload kelaikan kerja dan restriksi. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `Hero`, `DataTable`, `InformationAlert`, `StatusBadge`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/mcu-clearance-view.jsx` (Komponen direktori kelaikan medis dan modal input hasil MCU)
- `src/app/hr/recruitment/mcu-clearance/page.jsx` (Rute Next.js App Router)
