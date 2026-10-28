# Laporan Perubahan Frontend — `FE-HRD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-002` |
| Judul | Penyediaan rute dan layar administrasi lintas-pegawai untuk Workforce Core |
| Slice | `S-A1` (Workforce Core Cross-Employee Administration) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-001`, `HRD-DEC-002`, `FE-HRD-002` |
| Contract version | Workforce Core Management API v1 |
| Dependency | `BE-HRD-003` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/`, `src/components/view/hr/workforce-core/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Petugas administrasi kepegawaian rumah sakit membutuhkan kemampuan untuk melihat rekapitulasi data seluruh staf dalam satu layar terpadu, tidak hanya perorangan saat membuka profil satu per satu. 

Task ini menyediakan 6 menu dan halaman administrasi lintas-pegawai di area `/hr/workforce-core/`:
1. **Perubahan Profil Pegawai**: Rekapitulasi permohonan perubahan data identitas atau kontak pegawai.
2. **Penempatan Organisasi**: Rekapitulasi penempatan unit, instalasi, dan divisi staf di rumah sakit.
3. **Penugasan Jabatan**: Rekapitulasi formasi jabatan struktural dan fungsional.
4. **Penetapan Manajer / Atasan**: Matriks atasan langsung pegawai untuk jalur persetujuan (*approval chain*).
5. **Riwayat Kepegawaian**: Rekam jejak perjalanan karier dan status kerja pegawai.
6. **Penetapan Gaji Pokok**: Rekapitulasi besaran gaji dasar staf.

Setiap layar dilengkapi dengan kartu statistik (*SummaryCards*), kotak pencarian dan penyaringan data (*DataFilter*), tabel data responsif (*DataTable*), status badge (*StatusBadge*), dan paginasi (*Pagination*).

---

## 1. Wewenang UI yang Dipakai

Menggunakan pola layout dashboard operasional Quilvian standar di bawah rute Next.js App Router: `src/app/hr/workforce-core/*`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman & breadcrumb | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan metrik data | `SummaryCards` | `REUSE` | Kartu metrik total data & filter aktif |
| Filter & pencarian | `DataFilter` | `REUSE` | Input pencarian kata kunci terintegrasi |
| Tabel penyajian data | `DataTable` | `REUSE` | Menampilkan kolom ID, Pegawai, Info, Waktu, Status |
| Penanda status data | `StatusBadge` | `REUSE` | Menampilkan badge status Aktif / Terverifikasi / Pending |
| Paginasi halaman | `Pagination` | `REUSE` | Navigasi halaman data |
| Peringatan & pesan kosong | `InformationAlert` | `REUSE` | Alert informasi dan penanganan error |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/workforce-core-cross-employee-service.js`: Layanan pemanggilan API backend untuk 6 domain lintas-pegawai.
* `src/components/view/hr/workforce-core/workforce-core-cross-employee-list-view.jsx`: Komponen tampilan daftar bersama yang reusable dengan standar base feature.
* `src/components/view/hr/workforce-core/employee-profile-changes-view.jsx`: Tampilan rekapitulasi perubahan profil.
* `src/components/view/hr/workforce-core/organization-assignments-view.jsx`: Tampilan rekapitulasi penempatan organisasi.
* `src/components/view/hr/workforce-core/position-assignments-view.jsx`: Tampilan rekapitulasi penugasan jabatan.
* `src/components/view/hr/workforce-core/manager-assignments-view.jsx`: Tampilan rekapitulasi penetapan atasan/manajer.
* `src/components/view/hr/workforce-core/employment-histories-view.jsx`: Tampilan rekapitulasi riwayat kerja.
* `src/components/view/hr/workforce-core/salary-assignments-view.jsx`: Tampilan rekapitulasi penetapan gaji pokok.
* `src/app/hr/workforce-core/employee-profile-changes/page.jsx`: Rute Next.js untuk perubahan profil.
* `src/app/hr/workforce-core/organization-assignments/page.jsx`: Rute Next.js untuk penempatan organisasi.
* `src/app/hr/workforce-core/position-assignments/page.jsx`: Rute Next.js untuk penugasan jabatan.
* `src/app/hr/workforce-core/manager-assignments/page.jsx`: Rute Next.js untuk penetapan manajer.
* `src/app/hr/workforce-core/employment-histories/page.jsx`: Rute Next.js untuk riwayat kepegawaian.
* `src/app/hr/workforce-core/salary-assignments/page.jsx`: Rute Next.js untuk penetapan gaji pokok.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada seluruh berkas view, page, dan service |
| Komunikasi API Backend | `PASS` | Seluruh endpoint terhubung ke controller `BE-HRD-003` di backend |
| Penanganan State | `PASS` | Mendukung state memuat (*loading*), data kosong (*empty*), dan kegagalan jaringan (*error*) |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia 6 rute dan layar administrasi kepegawaian lintas-pegawai | Terpenuhi | 6 route di bawah `/hr/workforce-core/` |
| 2. Panggilan API terhubung ke endpoint `BE-HRD-003` | Terpenuhi | `WorkforceCoreCrossEmployeeService` memanggil endpoint backend yang sah |
| 3. Seluruh elemen UI menggunakan base component baku Quilvian | Terpenuhi | Menggunakan Hero, SummaryCards, DataFilter, DataTable, StatusBadge, Pagination |
| 4. Memiliki penanganan loading, error, dan pagination yang benar | Terpenuhi | Implementasi lengkap di `workforce-core-cross-employee-list-view.jsx` |

---

## 6. Catatan Penutup

Task `FE-HRD-002` selesai secara tuntas dan telah menyediakan antarmuka administrasi kepegawaian lintas-pegawai yang terstruktur bagi petugas HR rumah sakit.
