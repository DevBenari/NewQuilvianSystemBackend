# Laporan Perubahan Frontend — `FE-HRD-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-024` |
| Judul | Formulir Pembuatan Dokumen SK, Detail, Unggah Scan, dan Panel Persetujuan |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `HRD-DEC-031`, `BE-HRD-025` |
| Contract version | Personnel Action Order API v1 (`HrdPersonnelActionOrderController.cs`) |
| Dependency | `FE-HRD-022` (✅ Selesai), `FE-HRD-023` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/personnel-actions/create/page.jsx`, `src/app/hr/workforce-core/personnel-actions/[id]/page.jsx`, `src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx`, `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penerbitan Surat Keputusan (SK) mutasi dan promosi pegawai rumah sakit merupakan dokumen legal operasional yang memerlukan ketelitian dan tata kelola independen.

Task ini mengimplementasikan dua antarmuka utama:
1. **Formulir Pembuatan SK Baru (`/create`)**:
   - Memilih pegawai target, kategori tindakan kepegawaian, tanggal efektif, dan dasar pertimbangan hukum/operasional.
   - Mengisi formasi target: unit organisasi, departemen, jabatan struktural/fungsional, relasi atasan langsung, dan usulan gaji baru.
   - Validasi data dasar dan panduan tata kelola terkait larangan mutasi retroaktif pada periode absensi tertutup (`HRD-DEC-050`).
2. **Layar Detail, Unggah Scan, dan Panel Persetujuan (`/[id]`)**:
   - Menampilkan perbandingan status kedinasan dan linimasa tanggal efektif.
   - Form pengunggahan berkas scan fisik SK bertandatangan basah/elektronik (PDF, PNG, JPG) untuk pengarsipan permanen (`HRD-DEC-056`).
   - Panel tindakan alur kerja: Ajukan Persetujuan, Setujui & Terapkan SK, Tolak SK (dengan modal input alasan wajib), dan Batalkan SK.
   - Penegakan prinsip *Segregation of Duties* (`HRD-DEC-031`): Pembuat draf SK dilarang menyetujui dokumen buatannya sendiri.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-024: Formulir Pembuatan & Detail SK Kepegawaian mematuhi HRD-DEC-031 anti self-approval (0.442ms)
```
Status: **PASS (100%)**
