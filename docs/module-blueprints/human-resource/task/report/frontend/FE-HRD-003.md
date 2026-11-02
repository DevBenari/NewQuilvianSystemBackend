# Laporan Perubahan Frontend — `FE-HRD-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-003` |
| Judul | Penyediaan rute dan layar pengajuan cuti pegawai (ESS) |
| Slice | `S-A2` (Employee Self-Service Cuti Pegawai) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-003`, `FE-HRD-003` |
| Contract version | Employee Self-Service Leave API v1 |
| Dependency | `BE-HRD-004` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/self-services/human-resource/leave/`, `src/components/view/self-services/human-resource/leave/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Setiap pegawai dan staf medis rumah sakit memerlukan kemudahan untuk mengajukan cuti (cuti tahunan, cuti sakit, melahirkan, dll.) dan memantau sisa kuota cuti mereka secara mandiri (*Employee Self-Service*).

Task ini menyediakan antarmuka mandiri di rute `/self-services/human-resource/leave` yang memiliki kapabilitas:
1. **Kartu Sisa Saldo Cuti**: Menampilkan kartu ringkasan kuota hak cuti, jumlah hari yang sudah terpakai, dan sisa hari cuti yang masih berlaku.
2. **Formulir Pengajuan Cuti Baru**: Modal interaktif untuk memilih tipe cuti, rentang tanggal mulai hingga selesai, alasan pengajuan, serta informasi kontak darurat saat tidak bertugas.
3. **Riwayat Pengajuan Cuti**: Tabel daftar permohonan cuti sebelumnya beserta status persetujuan (*Pending*, *Approved*, atau *Rejected*).

---

## 1. Wewenang UI yang Dipakai

Rute kanonikal mandiri pegawai: `src/app/self-services/human-resource/leave/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman & navigasi | `Hero` | `REUSE` | Standar header Quilvian |
| Kartu informasi saldo kuota | `SummaryCards` | `REUSE` | Menampilkan metrik sisa saldo dan kuota |
| Tabel riwayat pengajuan | `DataTable` | `REUSE` | Kolom tipe cuti, tanggal, durasi, alasan, status |
| Status verifikasi permohonan | `StatusBadge` | `REUSE` | Badge status pengajuan cuti |
| Modal pengajuan cuti | `ConfirmModal` | `REUSE` | Modal interaktif pengisian formulir cuti baru |
| Notifikasi umpan balik | `ToastStack` | `REUSE` | Toast notifikasi saat pengajuan berhasil dikirim |
| Peringatan & pesan kesalahan | `InformationAlert` | `REUSE` | Alert informasi sistem |

---

## 3. Berkas yang Berubah

### Ditambahkan / Diperbarui
* `src/lib/services/hr/employee-self-service.js`: Ditambahkan metode pemanggilan API: `getLeaveBalances`, `getLeaveRequests`, dan `createLeaveRequest`.
* `src/components/view/self-services/human-resource/leave/leave-employee-view.jsx`: Komponen layar mandiri cuti pegawai terintegrasi dengan modal dan tabel.
* `src/app/self-services/human-resource/leave/page.jsx`: Rute Next.js untuk halaman ESS Cuti Pegawai.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service cuti |
| Validasi Pengisian Form | `PASS` | Form mewajibkan pengisian tanggal mulai, tanggal selesai, dan tipe cuti |
| Komunikasi API Backend | `PASS` | Terhubung ke endpoint `BE-HRD-004` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia halaman mandiri pengajuan cuti di area self-services | Terpenuhi | Halaman `/self-services/human-resource/leave` aktif |
| 2. Menampilkan informasi saldo cuti dan riwayat permohonan | Terpenuhi | Menggunakan SummaryCards dan DataTable |
| 3. Pegawai dapat mengisi dan mengirim permohonan cuti baru | Terpenuhi | Modal interaktif terhubung ke `createLeaveRequest` |
| 4. Memakai base component baku Quilvian tanpa styling hardcoded | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-003` selesai secara tuntas dan telah menyediakan portal mandiri pengajuan cuti bagi seluruh staf rumah sakit.
