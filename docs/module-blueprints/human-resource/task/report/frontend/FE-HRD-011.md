# Laporan Perubahan Frontend — `FE-HRD-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-011` |
| Judul | Penyediaan rute dan layar rekonsiliasi jam lembur HR |
| Slice | `S-B3` (Rekonsiliasi Jam Lembur HR) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-004`, `FE-HRD-011` |
| Contract version | Overtime Reconciliation API v1 |
| Dependency | `FE-HRD-004` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/overtime/reconciliation/`, `src/components/view/hr/overtime/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Sebelum gaji dan uang lembur dibayarkan kepada para perawat, dokter jaga, maupun staf penunjang medis lainnya, tim HR perlu membandingkan antara jam lembur yang direncanakan dengan jam aktual yang dilaporkan pada mesin absensi. Proses ini disebut **Rekonsiliasi Jam Lembur**.

Task ini menyediakan antarmuka di alamat `/hr/overtime/reconciliation`:
1. **Dasbor Ringkasan Lembur**: Metrik total jam lembur yang diajukan, total jam lembur yang telah disetujui, dan total jam yang masih tertunda rekonsiliasinya.
2. **Tabel Rekonsiliasi Lembur**: Menampilkan rincian tanggal, nama pegawai, jam mulai/selesai yang diajukan, jam aktual mesin presensi, durasi jam yang diakui, serta status rekonsiliasi.
3. **Penyaringan & Pencarian**: Filter cepat untuk mencari berdasarkan nama staf atau unit kerja tertentu.

---

## 1. Wewenang UI yang Dipakai

Rute rekonsiliasi lembur HR: `src/app/hr/overtime/reconciliation/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan jam lembur | `SummaryCards` | `REUSE` | Metrik jam lembur terverifikasi |
| Pencarian & filter staf | `DataFilter` | `REUSE` | Filter pencarian data lembur |
| Tabel rekonsiliasi | `DataTable` | `REUSE` | Kolom ID, Pegawai, Tanggal, Jam Rencana, Jam Aktual, Jam Diakui, Status |
| Status verifikasi | `StatusBadge` | `REUSE` | Badge penanda status rekonsiliasi |
| Notifikasi feedback | `ToastStack` | `REUSE` | Umpan balik notifikasi pengguna |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/overtime-admin-service.js`: Layanan pemanggilan API daftar rekonsiliasi jam lembur dan pemrosesan audit lembur.
* `src/components/view/hr/overtime/overtime-admin-view.jsx`: Tampilan dasbor rekonsiliasi lembur HR terintegrasi dengan tabel dan metrik.
* `src/app/hr/overtime/reconciliation/page.jsx`: Rute Next.js untuk halaman Rekonsiliasi Jam Lembur HR.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service lembur admin |
| Penyajian Data Komparasi | `PASS` | Tabel menampilkan perbandingan jam rencana vs jam aktual secara jelas |
| Komunikasi API Backend | `PASS` | Terhubung ke controller overtime reconciliation backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar rekonsiliasi jam lembur untuk divisi HR | Terpenuhi | Halaman `/hr/overtime/reconciliation` aktif |
| 2. Menampilkan perbandingan jam lembur rencana dan jam aktual | Terpenuhi | Tersaji pada DataTable |
| 3. Ringkasan total jam lembur tersaji dalam kartu metrik | Terpenuhi | Menggunakan SummaryCards |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-011` selesai secara tuntas dan telah menyediakan instrumen verifikasi akurasi jam lembur sebelum diteruskan ke perhitungan payroll.
