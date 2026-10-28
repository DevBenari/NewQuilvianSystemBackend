# Laporan Perubahan Frontend — `FE-HRD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-006` |
| Judul | Penyediaan rute dan layar pengajuan koreksi absensi mandiri (ESS) |
| Slice | `S-A5` (Employee Self-Service Koreksi Absensi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-006`, `FE-HRD-006` |
| Contract version | Employee Self-Service Attendance Correction API v1 |
| Dependency | `BE-HRD-008` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/self-services/human-resource/attendance-correction/`, `src/components/view/self-services/human-resource/attendance-correction/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Ketika mesin absensi biometrik atau sistem tap kartu rumah sakit mengalami kendala teknis (mati lampu, jaringan putus), atau staf medis lupa melakukan absensi saat bertugas menangani pasien darurat, staf memerlukan jalur resmi untuk mengajukan perbaikan jam kehadiran (*attendance correction*).

Task ini menyediakan antarmuka mandiri di rute `/self-services/human-resource/attendance-correction` yang memfasilitasi:
1. **Daftar Pengajuan Koreksi**: Tabel riwayat pengajuan koreksi jam masuk/keluar pegawai beserta status persetujuannya.
2. **Formulir Pengajuan Koreksi Baru**: Modal interaktif untuk memilih tanggal absensi yang bermasalah, jenis koreksi (Jam Masuk / Jam Pulang / Keduanya), jam kehadiran yang diusulkan, alasan perbaikan, serta unggahan bukti dukung.

---

## 1. Wewenang UI yang Dipakai

Rute mandiri pegawai: `src/app/self-services/human-resource/attendance-correction/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Kartu ringkasan koreksi | `SummaryCards` | `REUSE` | Menampilkan total permohonan koreksi |
| Tabel riwayat koreksi | `DataTable` | `REUSE` | Kolom tanggal absensi, jenis perbaikan, waktu lama, waktu baru, alasan, status |
| Penanda status verifikasi | `StatusBadge` | `REUSE` | Status pending / disetujui / ditolak |
| Modal pengajuan koreksi | `ConfirmModal` | `REUSE` | Form input interaktif permohonan koreksi kehadiran |
| Notifikasi feedback | `ToastStack` | `REUSE` | Toast notifikasi saat pengajuan terkirim |

---

## 3. Berkas yang Berubah

### Ditambahkan / Diperbarui
* `src/lib/services/hr/employee-self-service.js`: Ditambahkan metode pemanggilan API: `getAttendanceCorrections`, `createAttendanceCorrection`, dan `getCorrectionFieldOptions`.
* `src/components/view/self-services/human-resource/attendance-correction/attendance-correction-employee-view.jsx`: Komponen layar mandiri koreksi absensi pegawai terintegrasi dengan modal dan tabel riwayat.
* `src/app/self-services/human-resource/attendance-correction/page.jsx`: Rute Next.js untuk halaman ESS Koreksi Absensi Mandiri.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service koreksi absensi |
| Validasi Form Koreksi | `PASS` | Memastikan tanggal dan jam koreksi terisi lengkap dengan alasan |
| Komunikasi API Backend | `PASS` | Terhubung ke endpoint `BE-HRD-008` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia portal mandiri pengajuan koreksi absensi di area self-services | Terpenuhi | Halaman `/self-services/human-resource/attendance-correction` aktif |
| 2. Pegawai dapat mengajukan koreksi jam masuk/pulang beserta alasannya | Terpenuhi | Modal interaktif terhubung ke `createAttendanceCorrection` |
| 3. Riwayat koreksi tersaji dalam tabel dengan status yang jelas | Terpenuhi | Menggunakan DataTable dan StatusBadge |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-006` selesai secara tuntas dan telah memberikan solusi resmi bagi pegawai rumah sakit untuk mengajukan perbaikan jam kehadiran secara transparan.
