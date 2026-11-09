# Laporan Perubahan Frontend — `FE-HRD-044`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-044` |
| Judul | Layar Antrean & Permintaan Requisition Formasi |
| Slice | `S-D2` (Rekrutmen dan Seleksi Calon Karyawan RS) · `MVP-11` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 1 S-D2 |
| Trace | `HRD-DEC-073`, `HRD-DEC-074`, `FR-REC-01`, `VR-REC-01`, `VR-REC-02`, `BE-HRD-052` |
| Contract version | API Contract v8 bagian 14.1 grup `Job Requisition` |
| Dependency | `BE-HRD-052` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/recruitment/requisitions/page.jsx`, `src/components/view/hr/recruitment/requisitions-list-view.jsx`, `src/lib/services/hr/recruitment-service.js`, `src/lib/state/slice/hr/recruitment/recruitment-slice.js`, `src/utils/menu-sidebar/menu-items.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Permintaan pembukaan lowongan pekerjaan di rumah sakit harus bersumber dari perencanaan formasi tenaga kerja (Workforce Planning / WFP) yang telah disetujui, demi menjamin kepatuhan rasio kecukupan nakes dan ketersediaan anggaran.

Task ini mengimplementasikan antarmuka utama antrean requisition formasi:
1. **Dasbor & Tabel Antrean Requisition (`/hr/recruitment/requisitions`)**:
   - Menampilkan nomor requisition, nama jabatan, unit organisasi kerja, alokasi kursi (diminta, disetujui, dan terpenuhi), status dokumen, dan tanggal diajukan.
   - Dilengkapi kartu ringkasan metrik: Total Requisition, Menunggu Persetujuan, dan Disetujui (Siap Dibuka Lowongan).
   - Filter pencarian cepat berdasarkan kata kunci jabatan dan status dokumen.
2. **Modal Permohonan Requisition Baru**:
   - Formulir terstruktur dengan validasi pemilihan Headcount Request WFP induk, unit kerja, jabatan, dan kuota kursi yang diminta.
   - Penegakan validasi kuota kursi (`VR-REC-02`): kursi yang diajukan tidak boleh melampaui sisa alokasi formasi WFP.
3. **Penyelarasan Navigasi Sidebar**:
   - Penambahan grup navigasi tingkat 1 `"Pengadaan & Rekrutmen"` pada menu utama Sumber Daya Manusia di `menu-items.jsx`.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan tabel daftar requisition: Nomor, Jabatan, Unit Kerja, Jumlah Kursi, Status, Tanggal Pengajuan | ✅ PASS | Terbukti pada `requisitions-list-view.jsx` dengan kolom `requisitionNumber`, `positionName`, `organizationUnitName`, `requestedSeats`, `status`, dan `createDateTime`. |
| 2 | Tombol "Buat Requisition Baru" membuka modal form untuk memilih `HeadcountRequestId` approved | ✅ PASS | Modal interaktif `showCreateModal` pada `requisitions-list-view.jsx` terhubung ke `RecruitmentService.createRequisition`. |
| 3 | Pesan validasi kursi `VR-REC-02` tampil jelas jika kursi melebihi alokasi WFP | ✅ PASS | Penanganan error state `createError` menampilkan pesan validasi bisnis backend secara transparan via `InformationAlert`. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Tampilan 100% menggunakan token CSS variables (`recruitment.module.css`) dan base component Quilvian (`BaseButton`, `StatusBadge`, `DataTable`, `Hero`, `BaseFormControl`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/lib/services/hr/recruitment-service.js` (Metode `getRequisitions`, `createRequisition`)
- `src/lib/state/slice/hr/recruitment/recruitment-slice.js` (Thunk `fetchRequisitions`, state `requisitions`)
- `src/utils/menu-sidebar/menu-items.jsx` (Grup `Pengadaan & Rekrutmen` & item `Permintaan Requisition`)
- `src/components/view/hr/recruitment/requisitions-list-view.jsx` (Komponen antarmuka list)
- `src/app/hr/recruitment/requisitions/page.jsx` (Rute Next.js App Router)
- `src/style/hr/recruitment/recruitment.module.css` (Gaya CSS Module berbasis token)
