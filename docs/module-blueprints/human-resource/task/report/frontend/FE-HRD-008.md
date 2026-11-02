# Laporan Perubahan Frontend — `FE-HRD-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-008` |
| Judul | Penyediaan rute dan layar kotak masuk persetujuan terpadu (Unified Approval Inbox) |
| Slice | `S-A7` (Unified Approval Inbox Atasan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-007`, `FE-HRD-008` |
| Contract version | Corporate Workflow Approval API v1 |
| Dependency | `BE-HRD-010` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workflow/approval-inbox/`, `src/components/view/hr/workflow/`, `src/lib/services/hr/`, `src/utils/menu-sidebar/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Atasan langsung di rumah sakit (Kepala Ruangan, Kepala Instalasi, Manajer Medis, Direktur) setiap hari menerima berbagai macam permohonan dari staf bawahannya: permohonan cuti, rencana lembur, klaim lembur, tukar jadwal jaga, hingga perbaikan absensi. Jika atasan harus membuka 5 aplikasi atau halaman yang berbeda untuk menyetujui masing-masing permohonan, proses administrasi menjadi lambat dan rawan terlewat.

Task ini menyediakan **Kotak Masuk Persetujuan Terpadu (*Unified Approval Inbox*)** di alamat `/hr/workflow/approval-inbox`:
1. **Daftar Permohonan Terpadu**: Mengumpulkan seluruh antrean permohonan staf dalam satu tabel, lengkap dengan filter tipe dokumen (Cuti, Lembur, Jadwal, Absensi).
2. **Detail Permohonan**: Informasi lengkap staf pemohon, unit kerja, tanggal, durasi, dan alasan permohonan.
3. **Aksi Persetujuan Cepat**: Tombol *Setujui* (*Approve*), *Tolak* (*Reject*), dan *Minta Revisi* (*Revision*) yang dilengkapi modal konfirmasi catatan/alasan keputusan.
4. **Entri Menu Sidebar**: Terdaftar resmi pada menu navigasi samping di bawah grup modul HR.

---

## 1. Wewenang UI yang Dipakai

Rute alur persetujuan: `src/app/hr/workflow/approval-inbox/page.jsx`.
Menu sidebar: `src/utils/menu-sidebar/menu-items.jsx` (`corporateApprovalInbox`).

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Kartu metrik antrean | `SummaryCards` | `REUSE` | Total pending, disetujui, ditolak, butuh revisi |
| Penyaringan tipe dokumen | `DataFilter` | `REUSE` | Filter berdasarkan kategori permohonan |
| Tabel antrean permohonan | `DataTable` | `REUSE` | Kolom ID, Pemohon, Jenis, Ringkasan, Tanggal, Status, Aksi |
| Status permohonan | `StatusBadge` | `REUSE` | Penanda status pengajuan |
| Tombol aksi persetujuan | `BaseButton` | `REUSE` | Tombol Setujui, Tolak, dan Revisi |
| Modal konfirmasi keputusan | `ConfirmModal` | `REUSE` | Modal pengisian alasan/catatan persetujuan |
| Notifikasi hasil aksi | `ToastStack` | `REUSE` | Toast notifikasi saat persetujuan berhasil diproses |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/approval-inbox-service.js`: Layanan pemanggilan API daftar antrean dan eksekusi persetujuan atasan.
* `src/components/view/hr/workflow/approval-inbox-view.jsx`: Tampilan kotak masuk persetujuan terpadu dengan modal aksi.
* `src/app/hr/workflow/approval-inbox/page.jsx`: Rute Next.js untuk halaman Unified Approval Inbox.

### Diperbarui
* `src/utils/menu-sidebar/menu-items.jsx`: Menambahkan entri menu `corporateApprovalInbox` ("Kotak Masuk Persetujuan").

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, service, dan menu-items |
| Modal Aksi Persetujuan | `PASS` | Modal terbuka dengan benar saat tombol Setujui/Tolak/Revisi ditekan |
| Komunikasi API Backend | `PASS` | Terhubung ke controller workflow approval `BE-HRD-010` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar unified approval inbox untuk atasan | Terpenuhi | Halaman `/hr/workflow/approval-inbox` aktif |
| 2. Mampu menyajikan antrean dari beragam tipe dokumen permohonan staf | Terpenuhi | Filter dokumen dan tabel dinamis |
| 3. Menyediakan aksi Approve, Reject, dan Revision dengan konfirmasi catatan | Terpenuhi | Terintegrasi dengan ConfirmModal dan API `processDecision` |
| 4. Menu terdaftar pada sidebar navigasi | Terpenuhi | Entri `corporateApprovalInbox` di `menu-items.jsx` |
| 5. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-008` selesai secara tuntas dan telah menghadirkan pusat persetujuan terpadu bagi para kepala ruangan dan pimpinan unit rumah sakit.
