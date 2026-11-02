# Laporan Perubahan Frontend — `FE-HRD-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-016` |
| Judul | Penyediaan rute dan layar tinjauan kinerja berkala dan evaluasi KPI staf |
| Slice | `S-C3` (Evaluasi Kinerja & KPI Staf RS) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-013`, `FE-HRD-016` |
| Contract version | Performance Management API v1 |
| Dependency | `FE-HRD-002` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/performance/reviews/`, `src/components/view/hr/performance/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penilaian kinerja berkala (tahunan atau semesteran) untuk tenaga medis, paramedis, dan tenaga non-medis rumah sakit sangat penting untuk menentukan kelayakan kenaikan jenjang karier klinis (*clinical ladder*), pemberian insentif remunerasi, serta perpanjangan kontrak kerja.

Task ini menyediakan antarmuka evaluasi kinerja di alamat `/hr/performance/reviews`:
1. **Dasbor Ringkasan Penilaian**: Metrik total evaluasi yang telah diselesaikan, rata-rata skor capaian KPI, dan evaluasi yang masih dalam proses (*in progress*).
2. **Tabel Tinjauan Kinerja Staf**: Menampilkan daftar nama pegawai, unit penempatan, periode evaluasi, skor capaian KPI (0-100), predikat kinerja (*Sangat Baik*, *Baik*, *Cukup*, *Kurang*), serta status penyelesaian review.
3. **Modal Pembuatan Evaluasi Kinerja Baru**: Dialog interaktif bagi penilai (atasan/HR) untuk memulai lembar penilaian staf baru dengan menetapkan target KPI dan periode review.

---

## 1. Wewenang UI yang Dipakai

Rute evaluasi kinerja HR: `src/app/hr/performance/reviews/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan metrik skor | `SummaryCards` | `REUSE` | Rata-rata skor KPI dan total review |
| Filter & pencarian | `DataFilter` | `REUSE` | Filter nama staf atau unit kerja |
| Tabel evaluasi kinerja | `DataTable` | `REUSE` | Kolom ID, Pegawai, Unit, Periode, Skor KPI, Predikat, Status |
| Predikat & status | `StatusBadge` | `REUSE` | Badge Sangat Baik / Baik / Draft |
| Modal inisiasi review baru | `ConfirmModal` | `REUSE` | Form pembukaan evaluasi kinerja baru |
| Notifikasi feedback | `ToastStack` | `REUSE` | Umpan balik notifikasi pengguna |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/performance-management-service.js`: Layanan pemanggilan API data tinjauan kinerja dan pembuatan review baru.
* `src/components/view/hr/performance/performance-review-view.jsx`: Tampilan dasbor evaluasi kinerja staf HR terintegrasi modal.
* `src/app/hr/performance/reviews/page.jsx`: Rute Next.js untuk halaman Tinjauan Kinerja Staf.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service performance |
| Tampilan Skor & Predikat | `PASS` | Menghitung dan menyajikan skor numerik serta predikat kualitatif |
| Komunikasi API Backend | `PASS` | Terhubung ke controller performance management backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar monitoring dan evaluasi kinerja staf berkala | Terpenuhi | Halaman `/hr/performance/reviews` aktif |
| 2. Menampilkan rekapitulasi capaian skor KPI dan predikat kerja staf | Terpenuhi | Tersaji pada DataTable dan SummaryCards |
| 3. Penilai dapat membuka sesi evaluasi kinerja baru | Terpenuhi | Modal ConfirmModal terhubung ke `createPerformanceReview` |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-016` selesai secara tuntas dan telah menyediakan instrumen penilaian kinerja yang obyektif bagi manajemen rumah sakit.
