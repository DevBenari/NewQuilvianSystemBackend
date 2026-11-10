# Laporan Perubahan Frontend — `FE-HRD-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-023` |
| Judul | Antarmuka Daftar Dokumen SK Kepegawaian (Personnel Action Order List) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `HRD-DEC-050`, `HRD-DEC-056`, `BE-HRD-025` |
| Contract version | Personnel Action Order API v1 (`HrdPersonnelActionOrderController.cs`) |
| Dependency | `FE-HRD-022` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/personnel-actions/page.jsx`, `src/app/hr/personnel-action-orders/page.jsx`, `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Petugas tata usaha dan administrasi HR rumah sakit membutuhkan satu layar terpadu untuk memantau seluruh Surat Keputusan (SK) mutasi, promosi, rotasi, demosi, dan penugasan sementara bagi seluruh tenaga medis dan staf.

Layar ini menyediakan:
1. **Penyaringan & Pencarian**: Filter kategori SK, filter status persetujuan, dan pencarian cepat nomor SK atau nama/NIP pegawai.
2. **Kartu Ringkasan Metrik**: Total SK, Menunggu Persetujuan, Diterapkan, dan Draf.
3. **Tabel Data Terstruktur**: Nomor SK resmi, identitas pegawai, kategori tindakan, tujuan penempatan, tanggal efektif berlakunya penugasan, status badge, dan indikator ketersediaan berkas fisik SK hasil pindai.
4. **Navigasi Cepat**: Tombol pembuatan SK baru dan tombol rincian per baris dokumen.

---

## 1. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header & Deskripsi Halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan Metrik | `SummaryGrid` | `REUSE` | Standar metrik Quilvian |
| Filter & Search Bar | `DataFilter`, `FilterSelect` | `REUSE` | Standar filter Quilvian |
| Tabel Data & Paginasi | `DataTable`, `RegionPagination` | `REUSE` | Paginasi tunggal standar |
| Indikator Status Dokumen | `StatusBadge` | `REUSE` | Varian info, warning, active, danger |
| Peringatan Galat Server | `InformationAlert` | `REUSE` | Pesan ramah pengguna dengan tombol Coba Lagi |

---

## 2. Bukti Pengujian Unit

```text
✔ FE-HRD-023: Rute dan View Daftar SK Kepegawaian tersedia (0.4334ms)
```
Status: **PASS (100%)**
