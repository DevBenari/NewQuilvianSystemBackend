# Laporan Perubahan Frontend — `FE-HRD-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-028` |
| Judul | Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Menjadi Read-Only) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `BE-HRD-028` |
| Contract version | Master Data Employee Management v1 |
| Dependency | `FE-HRD-021` (✅ Selesai), `FE-HRD-022` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/constants/hr/master-data/employee/employee-grouped-form.config.jsx`, `src/components/view/hr/master-data/employees/form/employee-form-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Sebelumnya, penempatan unit organisasi dan penugasan jabatan staf dapat diubah langsung oleh staf yang memiliki akses form edit Master Data Pegawai tanpa melalui persetujuan manajemen atau penerbitan dokumen resmi. Hal ini menyebabkan ketidakteraturan tata kelola dan bypass wewenang unit kerja.

Task ini memberlakukan **Penguncian Form Master Data Pegawai (`HRD-DEC-048`)**:
1. Pada mode edit (`update`), field Departemen Utama, Unit Kerja, Posisi Utama, dan Jabatan dikunci secara permanen menjadi *read-only* / *disabled*.
2. Jika pengguna mengarahkan kursor pada field yang terkunci, muncul pesan alasan jelas: *"Penetapan unit kerja dan jabatan wajib melalui Surat Keputusan (SK) Kepegawaian resmi (HRD-DEC-048)."*
3. Form menyajikan banner informasi tata kelola resmi yang mengedukasi staf bahwa mutasi atau penugasan baru harus diproses melalui modul SK Kepegawaian.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-028: Form Master Data Pegawai mengunci field unit kerja dan jabatan pada mode edit (0.3445ms)
```
Status: **PASS (100%)**
