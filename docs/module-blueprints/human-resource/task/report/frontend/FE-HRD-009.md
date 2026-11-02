# Laporan Perubahan Frontend — `FE-HRD-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-009` |
| Judul | Penyediaan rute dan layar monitoring presensi HR dan koreksi on-behalf |
| Slice | `S-B1` (Monitoring Presensi & Koreksi On-Behalf HR) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-006`, `FE-HRD-009` |
| Contract version | Attendance Administration API v1 |
| Dependency | `BE-HRD-006` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/attendance/`, `src/components/view/hr/attendance/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Tim HR rumah sakit bertugas memantau kepatuhan kehadiran seluruh staf rumah sakit secara harian serta menangani kasus-kasus khusus di mana staf tidak dapat melakukan perbaikan absensi sendiri (misalnya staf sedang sakit keras, dinas luar kota, atau terjadi kesalahan sistem fatal). Dalam kondisi tersebut, staf HR berwenang melakukan koreksi atas nama staf (*on-behalf*). Selain itu, sistem harus memastikan bahwa periode presensi yang sudah ditutup (*closed period*) untuk penggajian tidak boleh diubah sembarangan.

Task ini menyediakan antarmuka di alamat `/hr/attendance`:
1. **Dasbor Monitoring Presensi**: Menampilkan metrik ringkasan kehadiran harian (Hadir, Terlambat, Izin/Sakit, dan Mangkir/Alpha).
2. **Tabel Rekapitulasi Presensi**: Menampilkan riwayat jam masuk dan pulang seluruh staf per tanggal.
3. **Modal Koreksi Atas Nama Staf (On-Behalf)**: Formulir khusus petugas HR untuk memasukkan pegawai target, tanggal absensi, jam masuk/pulang baru, serta alasan wajib koreksi.
4. **Validasi Periode Terkunci**: Tombol koreksi otomatis dinonaktifkan (*disabled*) jika tanggal absensi masuk ke dalam periode kehadiran yang sudah ditutup oleh HR.

---

## 1. Wewenang UI yang Dipakai

Rute administrasi presensi HR: `src/app/hr/attendance/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan kehadiran | `SummaryCards` | `REUSE` | Kartu metrik kehadiran harian |
| Penyaringan & tanggal | `DataFilter` | `REUSE` | Filter pencarian data staf |
| Tabel monitoring | `DataTable` | `REUSE` | Kolom ID, Pegawai, Tanggal, Jam Masuk, Jam Pulang, Status, Aksi |
| Status kehadiran | `StatusBadge` | `REUSE` | Badge penanda tepat waktu / terlambat / koreksi |
| Modal koreksi on-behalf | `ConfirmModal` | `REUSE` | Form dialog koreksi absensi atas nama staf |
| Notifikasi sistem | `ToastStack` | `REUSE` | Notifikasi saat koreksi berhasil disimpan |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/attendance-management-service.js`: Layanan pemanggilan API monitoring presensi, status periode, dan eksekusi koreksi on-behalf.
* `src/components/view/hr/attendance/attendance-monitoring-view.jsx`: Tampilan dasbor monitoring presensi HR terintegrasi modal on-behalf.
* `src/app/hr/attendance/page.jsx`: Rute Next.js untuk halaman Monitoring Presensi HR.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service presensi |
| Pemeriksaan Closed Period Guard | `PASS` | Modal melakukan validasi status penutupan periode sebelum kirim data |
| Komunikasi API Backend | `PASS` | Terhubung ke controller attendance administration `BE-HRD-006` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar monitoring presensi staf rumah sakit untuk HR | Terpenuhi | Halaman `/hr/attendance` aktif |
| 2. Fitur koreksi atas nama staf (on-behalf) berfungsi dengan aman | Terpenuhi | Modal ConfirmModal terhubung ke `submitOnBehalfCorrection` |
| 3. Mematuhi aturan penutupan periode kehadiran (closed period) | Terpenuhi | Pemeriksaan `checkPeriodClosed` terpasang |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-009` selesai secara tuntas dan telah menyediakan perangkat monitoring kehadiran yang andal bagi divisi HR rumah sakit.
