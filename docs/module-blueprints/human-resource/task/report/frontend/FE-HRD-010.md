# Laporan Perubahan Frontend — `FE-HRD-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-010` |
| Judul | Penyediaan rute dan layar alokasi dan penyesuaian saldo cuti HR |
| Slice | `S-B2` (Alokasi & Penyesuaian Saldo Cuti HR) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-003`, `FE-HRD-010` |
| Contract version | Leave Administration API v1 |
| Dependency | `FE-HRD-003` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/leave/adjustments/`, `src/components/view/hr/leave/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Setiap awal tahun kerja, saat pengangkatan pegawai tetap, atau ketika ada kebijakan khusus dari direksi rumah sakit (misalnya perpanjangan hak cuti karena masa tanggap darurat), bagian HR perlu mengalokasikan kuota cuti tahunan dan melakukan penyesuaian saldo cuti pegawai (*leave balance adjustment*).

Task ini menyediakan antarmuka di alamat `/hr/leave/adjustments`:
1. **Daftar Saldo Cuti Pegawai**: Menampilkan rekapitulasi kuota awal, hari terpakai, dan sisa hari cuti per jenis cuti untuk seluruh pegawai.
2. **Modal Penyesuaian Saldo Cuti**: Dialog interaktif untuk menambah atau mengurangi hari saldo cuti secara manual, disertai pilihan jenis operasi (Tambah / Kurang) dan pencatatan alasan audit yang jelas.

---

## 1. Wewenang UI yang Dipakai

Rute penyesuaian cuti HR: `src/app/hr/leave/adjustments/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Kartu metrik saldo | `SummaryCards` | `REUSE` | Total staf dengan saldo aktif dan terpakai |
| Filter pencarian staf | `DataFilter` | `REUSE` | Filter nama pegawai dan tipe cuti |
| Tabel saldo cuti | `DataTable` | `REUSE` | Kolom ID, Pegawai, Tipe Cuti, Alokasi, Terpakai, Sisa, Aksi |
| Status saldo | `StatusBadge` | `REUSE` | Badge penanda ketersediaan saldo |
| Modal penyesuaian cuti | `ConfirmModal` | `REUSE` | Form dialog penyesuaian saldo |
| Notifikasi sistem | `ToastStack` | `REUSE` | Notifikasi saat saldo berhasil disesuaikan |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/leave-admin-service.js`: Layanan pemanggilan API daftar saldo dan eksekusi penyesuaian saldo cuti.
* `src/components/view/hr/leave/leave-admin-view.jsx`: Tampilan manajemen saldo cuti HR terintegrasi modal penyesuaian.
* `src/app/hr/leave/adjustments/page.jsx`: Rute Next.js untuk halaman Alokasi Saldo Cuti HR.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service cuti admin |
| Modal Form Penyesuaian | `PASS` | Memvalidasi input angka penyesuaian dan alasan perubahan |
| Komunikasi API Backend | `PASS` | Terhubung ke controller leave administration backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar penyesuaian dan monitoring saldo cuti untuk HR | Terpenuhi | Halaman `/hr/leave/adjustments` aktif |
| 2. HR dapat melakukan penyesuaian saldo cuti pegawai dengan alasan audit | Terpenuhi | Modal ConfirmModal terhubung ke `adjustLeaveBalance` |
| 3. Rekapitulasi saldo cuti tersaji dalam tabel informatif | Terpenuhi | Menggunakan SummaryCards dan DataTable |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-010` selesai secara tuntas dan telah menyediakan instrumen administrasi kuota cuti bagi staf HR rumah sakit.
