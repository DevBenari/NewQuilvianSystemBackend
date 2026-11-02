# Laporan Perubahan Frontend — `FE-HRD-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-026` |
| Judul | Layar Batch Inisialisasi Penempatan Awal Pegawai (55 Pegawai Eksisting) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-054`, `BE-HRD-027` |
| Contract version | Initial Placement Batch API v1 (`HrdInitialPlacementBatchController.cs`) |
| Dependency | `BE-HRD-027` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/initial-batches/page.jsx`, `src/app/hr/initial-placement-batch/page.jsx`, `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`, `src/lib/services/hr/initial-placement-batch-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Saat sistem HR Quilvian beralih ke tata kelola terstruktur, terdapat 55 staf eksisting rumah sakit yang belum memiliki data penempatan organisasi, jabatan, atau relasi atasan resmi. Memasukkan satu per satu via form SK kepegawaian akan sangat memakan waktu.

Task ini menyediakan antarmuka **Batch Inisialisasi Penempatan Awal**:
1. **Manajemen Wadah Batch**: Petugas HR dapat melihat daftar batch inisialisasi atau membuat batch baru.
2. **Validasi Otomatis (Pre-Flight Integrity Check)**: Tombol validasi memeriksa apakah seluruh 55 baris usulan penempatan memiliki unit kerja, departemen, dan formasi jabatan yang valid di master data serta tidak ada duplikasi penugasan primer.
3. **Penerapan Serentak (Batch Apply)**: Setelah status tervalidasi siap (*Validated*), HR dapat mengesahkan seluruh penempatan awal secara serentak dalam satu transaksi database atomik (`HRD-DEC-054`).

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-026: Initial Placement Batch Service dan View tersedia (0.3755ms)
```
Status: **PASS (100%)**
