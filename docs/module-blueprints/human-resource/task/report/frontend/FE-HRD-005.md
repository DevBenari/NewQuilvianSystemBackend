# Laporan Perubahan Frontend — `FE-HRD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-005` |
| Judul | Penyediaan rute dan layar permohonan ubah jadwal dan tukar shift pegawai (ESS) |
| Slice | `S-A4` (Employee Self-Service Penjadwalan & Tukar Shift) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-005`, `FE-HRD-005` |
| Contract version | Employee Self-Service Schedule & Shift Swap API v1 |
| Dependency | `BE-HRD-007` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/self-services/human-resource/schedule/`, `src/components/view/self-services/human-resource/schedule/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Tenaga medis dan staf keperawatan rumah sakit sering kali perlu melakukan pertukaran jadwal jaga (shift swap) dengan rekan sejawat, atau mengajukan permohonan penyesuaian jadwal dinas karena tugas belajar atau keperluan darurat lainnya.

Task ini menyediakan antarmuka mandiri di rute `/self-services/human-resource/schedule` yang memfasilitasi:
1. **Jadwal Dinas Saya**: Melihat daftar jadwal penugasan shift dinas aktif (Pagi, Siang, Malam, Libur).
2. **Permohonan Perubahan Jadwal**: Formulir permohonan pergantian hari atau jam dinas perorangan.
3. **Permohonan Tukar Shift**: Formulir pertukaran jadwal jaga dengan pegawai pengganti, lengkap dengan tanggal jadwal asal, tanggal jadwal tujuan, dan alasan pertukaran agar kepala ruangan atau atasan dapat memberikan persetujuan.

---

## 1. Wewenang UI yang Dipakai

Rute mandiri pegawai: `src/app/self-services/human-resource/schedule/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Kartu metrik jadwal | `SummaryCards` | `REUSE` | Menampilkan total penugasan shift |
| Tabel jadwal dinas | `DataTable` | `REUSE` | Kolom tanggal dinas, nama shift, jam kerja, unit/ruangan, status |
| Status jadwal | `StatusBadge` | `REUSE` | Badge penugasan |
| Modal pengajuan perubahan jadwal | `ConfirmModal` | `REUSE` | Form input permohonan perubahan jadwal |
| Modal tukar shift rekan sejawat | `ConfirmModal` | `REUSE` | Form input pertukaran shift antarstaf |
| Notifikasi sistem | `ToastStack` | `REUSE` | Umpan balik setelah pengajuan terkirim |

---

## 3. Berkas yang Berubah

### Ditambahkan / Diperbarui
* `src/lib/services/hr/employee-self-service.js`: Ditambahkan metode pemanggilan API: `getMyScheduleAssignments`, `createScheduleChangeRequest`, dan `createShiftSwapRequest`.
* `src/components/view/self-services/human-resource/schedule/schedule-employee-view.jsx`: Komponen layar mandiri jadwal dinas pegawai dengan modal pengajuan perubahan dan modal tukar shift.
* `src/app/self-services/human-resource/schedule/page.jsx`: Rute Next.js untuk halaman ESS Jadwal & Tukar Shift.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service jadwal |
| Validasi Modal Tukar Shift | `PASS` | Memvalidasi identitas rekan pengganti dan tanggal shift |
| Komunikasi API Backend | `PASS` | Terhubung ke endpoint `BE-HRD-007` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia portal mandiri jadwal dinas pegawai di area self-services | Terpenuhi | Halaman `/self-services/human-resource/schedule` aktif |
| 2. Pegawai dapat mengajukan perubahan jadwal dinas | Terpenuhi | Modal ubah jadwal terhubung ke backend |
| 3. Pegawai dapat mengajukan tukar shift dengan rekan kerja | Terpenuhi | Modal tukar shift terhubung ke backend |
| 4. Memakai base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-005` selesai secara tuntas dan telah menyediakan alur pertukaran shift yang tertib dan tercatat rapi di rumah sakit.
