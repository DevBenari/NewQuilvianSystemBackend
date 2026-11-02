# Laporan Perubahan Frontend — `FE-HRD-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-018` |
| Judul | Penyediaan rute dan layar pencatatan kasus disiplin dan surat peringatan (SP) |
| Slice | `S-C5` (Kasus Disiplin & Surat Peringatan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-015`, `FE-HRD-018` |
| Contract version | Disciplinary Action Management API v1 |
| Dependency | `BE-HRD-015` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/employee-relations/disciplinary-actions/`, `src/components/view/hr/employee-relations/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penegakan tata tertib dan etika kerja di rumah sakit sangat penting demi menjamin mutu pelayanan dan keselamatan pasien. Pelanggaran kedisiplinan (seperti sering terlambat dinas, mangkir tugas, atau pelanggaran SOP tindakan) memerlukan penanganan formal yang tercatat rapi, mulai dari teguran lisan, Surat Peringatan Pertama (SP 1), SP 2, hingga SP 3 / Pemutusan Hubungan Kerja (PHK).

Task ini menyediakan antarmuka terpusat di alamat `/hr/employee-relations/disciplinary-actions`:
1. **Dasbor Kasus Disiplin**: Metrik jumlah kasus aktif, jumlah SP yang diterbitkan, dan kasus yang sedang dalam tahap investigasi/rehabilitasi.
2. **Tabel Rekapitulasi Kasus & SP**: Menampilkan nama pegawai, nomor surat peringatan, jenis pelanggaran, tanggal kejadian, tanggal berlaku sanksi, dan status status sanksi.
3. **Modal Penerbitan Sanksi Disiplin Baru**: Dialog interaktif bagi tim HR / Komite Etik untuk mencatat kasus pelanggaran baru dan menerbitkan surat tindakan disiplin.

---

## 1. Wewenang UI yang Dipakai

Rute hubungan kepegawaian & disiplin: `src/app/hr/employee-relations/disciplinary-actions/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan metrik kasus | `SummaryCards` | `REUSE` | Total kasus aktif dan terbit |
| Filter & pencarian | `DataFilter` | `REUSE` | Filter nama staf atau jenis sanksi |
| Tabel tindakan disiplin | `DataTable` | `REUSE` | Kolom ID, Pegawai, Kasus/SP, Tanggal Terbit, Masa Berlaku, Status |
| Status sanksi | `StatusBadge` | `REUSE` | Badge SP 1 / SP 2 / SP 3 / Selesai |
| Modal terbit sanksi baru | `ConfirmModal` | `REUSE` | Form input kasus disiplin baru |
| Notifikasi feedback | `ToastStack` | `REUSE` | Umpan balik notifikasi pengguna |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/employee-relations-service.js`: Layanan pemanggilan API daftar kasus disiplin dan penerbitan tindakan sanksi baru.
* `src/components/view/hr/employee-relations/disciplinary-action-view.jsx`: Tampilan dasbor kasus disiplin HR terintegrasi modal sanksi.
* `src/app/hr/employee-relations/disciplinary-actions/page.jsx`: Rute Next.js untuk halaman Kasus Disiplin & SP.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service disiplin |
| Validasi Modal Sanksi | `PASS` | Memastikan data pegawai, nomor SP, dan masa berlaku terisi |
| Komunikasi API Backend | `PASS` | Terhubung ke controller employee relations `BE-HRD-015` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar pencatatan kasus disiplin dan surat peringatan staf | Terpenuhi | Halaman `/hr/employee-relations/disciplinary-actions` aktif |
| 2. Menampilkan informasi riwayat tindakan disiplin dan masa berlaku sanksi | Terpenuhi | Tersaji pada DataTable |
| 3. Petugas HR/Komite dapat menerbitkan tindakan disiplin baru | Terpenuhi | Modal ConfirmModal terhubung ke `createDisciplinaryAction` |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-018` selesai secara tuntas dan telah menyediakan instrumen pencatatan kepatuhan etika dan kedisiplinan kerja staf rumah sakit.
