# Laporan Perubahan Frontend — `FE-HRD-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-013` |
| Judul | Penyediaan rute dan layar monitoring penggajian restitusi & potongan cicilan piutang |
| Slice | `S-B5` (Penggajian Restitusi & Potongan Cicilan Piutang) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-011`, `FE-HRD-013` |
| Contract version | Payroll Restitution & Deduction Integration API v1 |
| Dependency | `BE-HRD-013` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/payroll/restitution-installment/`, `src/components/view/hr/payroll/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Dalam siklus penggajian bulanan rumah sakit (*payroll cycle*), komponen gaji staf tidak hanya terdiri dari gaji pokok dan tunjangan. Terdapat dua komponen penyesuaian keuangan penting:
1. **Penambahan Dana (Restitusi)**: Klaim penggantian biaya pengobatan atau pengeluaran operasional pegawai yang disetujui untuk ditambahkan ke slip gaji.
2. **Pengurangan Dana (Potongan Cicilan Piutang)**: Potongan pembayaran cicilan kasbon/pinjaman pegawai kepada rumah sakit atau koperasi yang wajib dipotong dari gaji.

Task ini menyediakan antarmuka terintegrasi di alamat `/hr/payroll/restitution-installment`:
- **Peralihan Tampilan (Dual-Tab)**: Tab khusus Restitusi Pegawai dan Tab khusus Potongan Cicilan Piutang.
- **Kartu Statistik Finansial**: Metrik total nominal restitusi yang disetujui dan total nominal potongan cicilan aktif.
- **Tabel Rekapitulasi Rinci**: Menampilkan nama pegawai, nomor referensi keuangan, jumlah nominal uang, bulan periode gaji, dan status kelayakan pemotongan/penambahan.

---

## 1. Wewenang UI yang Dipakai

Rute integrasi penggajian HR: `src/app/hr/payroll/restitution-installment/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Tab navigasi (Restitusi vs Potongan) | Segmented Button Group | `REUSE` | Tombol tab navigasi kategori finansial |
| Kartu metrik nominal | `SummaryCards` | `REUSE` | Total nominal restitusi dan cicilan |
| Filter & pencarian | `DataFilter` | `REUSE` | Filter nama pegawai atau nomor dokumen |
| Tabel penyajian data finansial | `DataTable` | `REUSE` | Kolom ID, Pegawai, Referensi, Nominal, Periode, Status |
| Status kelayakan | `StatusBadge` | `REUSE` | Badge Disetujui / Terpotong / Aktif |
| Notifikasi feedback | `ToastStack` | `REUSE` | Umpan balik notifikasi pengguna |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/payroll-integration-service.js`: Layanan pemanggilan API data restitusi pegawai dan potongan cicilan piutang.
* `src/components/view/hr/payroll/payroll-restitution-installment-view.jsx`: Tampilan manajemen restitusi dan cicilan penggajian HR dengan dual-tab.
* `src/app/hr/payroll/restitution-installment/page.jsx`: Rute Next.js untuk halaman Penggajian Restitusi & Cicilan.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service payroll |
| Format Nominal Mata Uang | `PASS` | Seluruh angka rupiah diformat rapi (IDR) |
| Komunikasi API Backend | `PASS` | Terhubung ke controller payroll integration `BE-HRD-013` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar monitoring restitusi dan cicilan piutang untuk payroll | Terpenuhi | Halaman `/hr/payroll/restitution-installment` aktif |
| 2. Memisahkan tampilan restitusi dan potongan cicilan dalam tab dinamis | Terpenuhi | Dual-tab terintegrasi dengan filter pencarian |
| 3. Kartu ringkasan finansial menyajikan total nominal secara transparan | Terpenuhi | Menggunakan SummaryCards |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-013` selesai secara tuntas dan telah menyediakan antarmuka verifikasi komponen penyesuaian slip gaji staf rumah sakit.
