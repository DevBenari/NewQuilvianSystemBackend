# Laporan Perubahan Frontend — `FE-HRD-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-025` |
| Judul | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-053`, `HRD-DEC-033`, `BE-HRD-026` |
| Contract version | Employee 360 API v1 (`HrdEmployee360Controller.cs`) |
| Dependency | `BE-HRD-026` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/employee-360/[id]/page.jsx`, `src/app/hr/employee-360/[id]/page.jsx`, `src/components/view/hr/workforce-core/employee-360-view.jsx`, `src/lib/services/hr/employee-360-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Rumah sakit membutuhkan satu layar terpadu 360 derajat untuk meninjau seluruh aspek staf medis dan karyawan (kedinasan, riwayat SK, sertifikasi STR/SIP, kompensasi, dan catatan disiplin), namun data konfidensial wajib dilindungi agar tidak bocor ke pihak yang tidak berhak.

Task ini mengimplementasikan layar **Employee 360 Dual-View**:
1. **Mode HR Administrator (Akses Penuh)**: Menampilkan seluruh tab informasi secara komprehensif, mencakup struktur gaji pokok, grade kompensasi, nomor rekening payroll, NIK KTP, tanggal lahir, susunan keluarga, dan riwayat tindakan disiplin.
2. **Mode Atasan Langsung (Akses Terbatas Kedinasan)**: Jika layar dibuka oleh manajer/kepala ruangan bawahan, sistem secara otomatis menyembunyikan informasi gaji, rekening, identitas sensitif, dan catatan sanksi, menggantinya dengan banner edukasi privasi (`HRD-DEC-053` & `HRD-DEC-033`). Atasan hanya berhak melihat unit penugasan, jabatan, atasan, histori SK, dan masa berlaku sertifikasi kompetensi.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-025: Employee 360 Dual-View Service dan View membatasi data sensitif (1.1043ms)
```
Status: **PASS (100%)**
