# Laporan Perubahan Frontend — `FE-HRD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-004` |
| Judul | Penyediaan rute dan layar rencana serta klaim lembur pegawai (ESS) |
| Slice | `S-A3` (Employee Self-Service Lembur Pegawai) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-004`, `FE-HRD-004` |
| Contract version | Employee Self-Service Overtime API v1 |
| Dependency | `BE-HRD-005` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/self-services/human-resource/overtime/`, `src/components/view/self-services/human-resource/overtime/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pelayanan rumah sakit yang beroperasi 24 jam sehari membutuhkan fleksibilitas penugasan lembur (misalnya tindakan operasi cito, penanganan darurat di IGD, atau penggantian shift mendadak). Agar perhitungan jam kerja dan kompensasi tertib, pegawai membutuhkan dua tahapan lembur:
1. **Rencana Lembur (Pre-Overtime)**: Pengajuan permohonan dinas lembur sebelum pekerjaan dilaksanakan.
2. **Klaim Realisasi Lembur (Post-Overtime)**: Pelaporan jam aktual pelaksanaan lembur beserta uraian tugas yang dikerjakan untuk diverifikasi atasan dan HR.

Task ini menyediakan antarmuka dua tab di rute `/self-services/human-resource/overtime` untuk melihat riwayat serta mengirim rencana lembur baru maupun klaim lembur baru.

---

## 1. Wewenang UI yang Dipakai

Rute mandiri pegawai: `src/app/self-services/human-resource/overtime/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Tab navigasi (Rencana vs Klaim) | Segmented Button Group | `REUSE` | Tombol tab navigasi rencana dan klaim |
| Kartu ringkasan jam lembur | `SummaryCards` | `REUSE` | Metrik jam rencana dan jam klaim |
| Tabel riwayat lembur | `DataTable` | `REUSE` | Kolom tanggal, jam mulai, jam selesai, durasi, alasan/uraian tugas, status |
| Status verifikasi lembur | `StatusBadge` | `REUSE` | Status persetujuan atasan |
| Modal pengajuan rencana / klaim | `ConfirmModal` | `REUSE` | Modal interaktif pengajuan lembur |
| Notifikasi sistem | `ToastStack` | `REUSE` | Toast notifikasi saat form berhasil dikirim |

---

## 3. Berkas yang Berubah

### Ditambahkan / Diperbarui
* `src/lib/services/hr/employee-self-service.js`: Ditambahkan metode pemanggilan API: `getOvertimePlans`, `createOvertimePlan`, `getOvertimeClaims`, dan `createOvertimeClaim`.
* `src/components/view/self-services/human-resource/overtime/overtime-employee-view.jsx`: Komponen layar mandiri lembur pegawai dengan fitur dual-tab (Rencana & Klaim).
* `src/app/self-services/human-resource/overtime/page.jsx`: Rute Next.js untuk halaman ESS Lembur Pegawai.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service lembur |
| Pengalihan Tab Rencana/Klaim | `PASS` | Tab berganti dinamis memuat tabel dan formulir terkait |
| Komunikasi API Backend | `PASS` | Terhubung ke endpoint `BE-HRD-005` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia halaman mandiri lembur pegawai di area self-services | Terpenuhi | Halaman `/self-services/human-resource/overtime` aktif |
| 2. Mendukung pemisahan alur rencana lembur dan klaim realisasi lembur | Terpenuhi | Tampilan dual-tab dengan fungsi create masing-masing |
| 3. Riwayat jam lembur dan status persetujuan tersaji jelas | Terpenuhi | Menggunakan SummaryCards, DataTable, dan StatusBadge |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-004` selesai secara tuntas dan telah menyediakan sistem pengajuan lembur transparan bagi pegawai dan manajemen rumah sakit.
