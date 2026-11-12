# Laporan Perubahan Frontend — `FE-HRD-045`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-045` |
| Judul | Layar Detail & Draf Requisition Formasi |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-11` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 S-D2 |
| Trace | `HRD-DEC-073`, `HRD-DEC-074`, `FR-REC-01`, `VR-REC-03`, `BE-HRD-052` |
| Contract version | API Contract v8 bagian 14.1 grup `Job Requisition` |
| Dependency | `FE-HRD-044` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/requisitions/[id]/page.jsx`, `src/components/view/hr/recruitment/requisition-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Dokumen requisition yang telah dibuat membutuhkan peninjauan rincian formasi serta alur persetujuan atau pembatalan sebelum lowongan pekerjaan dibuka.

Task ini mengimplementasikan layar rincian requisition:
1. **Rincian Formasi & Alokasi Kursi (`/hr/recruitment/requisitions/[id]`)**:
   - Menampilkan nomor requisition, nomor headcount WFP terkait, jabatan, unit organisasi kerja, fasilitas rumah sakit, dan tanggal pengajuan.
   - Panel ringkasan kuota alokasi kursi: Kursi Diminta, Kursi Disetujui, dan Kursi Terpenuhi.
2. **Aksi Alur Kerja Kondisional**:
   - Tombol *"Ajukan Persetujuan"* aktif saat dokumen berstatus `DRAFT`.
   - Tombol *"Batalkan Requisition"* aktif untuk dokumen `DRAFT` atau `SUBMITTED`, memunculkan modal input alasan pembatalan wajib dan menegakkan validasi `VR-REC-03` (menolak jika lowongan kerja aktif telah dibuka).

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan detail lengkap requisition beserta tautan dokumen WFP | ✅ PASS | Terbukti pada `requisition-detail-view.jsx` dengan grid detail `requisitionNumber`, `headcountRequestNumber`, `positionName`, `organizationUnitName`, dan alokasi kursi. |
| 2 | Tombol "Ajukan" dan "Batalkan" aktif secara kondisional berdasarkan status dokumen (`DRAFT`) | ✅ PASS | Logika kondisional `isDraft` dan `isSubmitted` pada `requisition-detail-view.jsx` mengontrol visibilitas tombol aksi. |
| 3 | Konfirmasi pembatalan requisition memvalidasi ketiadaan lowongan aktif (`VR-REC-03`) | ✅ PASS | Modal pembatalan `showCancelModal` mewajibkan input alasan dan menangkap respon validasi `VR-REC-03` dari backend secara transparan. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Menggunakan token CSS variables dan base component Quilvian (`BaseButton`, `StatusBadge`, `Hero`, `InformationAlert`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/recruitment/requisition-detail-view.jsx` (Komponen antarmuka detail)
- `src/app/hr/recruitment/requisitions/[id]/page.jsx` (Rute dinamis Next.js App Router)
