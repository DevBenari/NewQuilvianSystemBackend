# Laporan Perubahan Frontend — `DEC-FRJ-001-FE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `DEC-FRJ-001-FE` (belum ada task roadmap; ID diturunkan dari keputusan `DEC-FRJ-001`) |
| Judul | Halaman Master Fasilitas Perujuk + integrasi Kiosk dan Pendaftaran Rawat Jalan |
| Kontrak | Delta backend pada `task/report/backend/DEC-FRJ-001-BE.md` |
| Target tulis | `QuilvianSystemFrontendDev`, branch `sukmagpV2` |
| Commit frontend saat dikerjakan | `6dbd184b` |
| Tanggal | 2026-10-09 |
| Status | Selesai; lint, build, dan uji layar dijalankan (lihat bagian 4) |

## 1. Ringkasan

- Halaman list mengikuti layout Manajemen Perangkat Kiosk. Hero (`Master Data` / `Fasilitas Perujuk`) tanpa tombol. SummaryGrid 4 indikator dari backend. Tombol `+ Tambah Fasilitas` di header DataFilter.
- Urutan filter: Tanggal Awal, Tanggal Akhir, Periode, Jenis Fasilitas, Status Kerja Sama, Search, Jumlah Baris, Refresh. Semua filter dikirim ke backend, dan setiap perubahan filter kembali ke halaman 1.
- Tabel tepat 7 kolom: No, Tanggal Dibuat, Kode, Nama Fasilitas, Masa Kerja Sama, Dibuat Oleh, Status. Klik dua kali membuka detail. Nama fasilitas bisa difokus lalu dibuka dengan Enter/Spasi.
- Form memakai `BaseGroupedEditorView` dengan 5 kelompok. Provinsi dan kota memakai select remote; kota dikosongkan saat provinsi diganti. Ada pencegahan submit ganda, dan update mengirim `expectedRowVersion`.
- Detail menampilkan histori perjanjian dan audit. Aksi baru: Perpanjang Kerja Sama (modal `ConfirmModal` + `BaseInputField`/`BaseDateField`) serta Aktifkan/Nonaktifkan.
- Rawat Jalan (step rujukan dan modal koreksi) memakai resource `referralPartnerInstitutions` dengan `serviceDate` = tanggal kunjungan. Jalur RJ→Laboratorium mengirim `requirePartnerEligibility: true`. Bila backend menolak dengan `RJ-VAL-PM-17`, pilihan fasilitas dan dokter dikosongkan.
- Kiosk memakai `kiosk/options` (kini hanya mitra layak). Bila submit ditolak `RJ-VAL-PM-17`, pilihan dikosongkan. Pasien baru dikembalikan ke step Data Rujukan; pasien lama melihat pesan untuk kembali memilih.

## 2. Keputusan base component

| Elemen | Status | Komponen |
| --- | --- | --- |
| Hero, SummaryGrid, DataFilter, DataTable, FilterDatePicker, FilterSelect, StatusBadge, BaseButton | `REUSE` | base-features |
| Form 5 kelompok | `REUSE` | `BaseGroupedEditorView` (`BaseEditorView` tidak mendukung pengelompokan) |
| Detail | `REUSE` | `BaseDetailView` |
| Modal perpanjangan | `COMPOSE` | `ConfirmModal` + `BaseInputField` + `BaseDateField` |
| Akses keyboard ke detail | `COMPOSE` | Elemen nama dengan `role="link"` + `tabIndex`; tidak mengubah DataTable |

UI GATE: tidak ada komponen `NEW`; tidak ada perubahan CSS global maupun base component.

## 3. Berkas yang berubah

Master: view list, `add/referral-institutions-form-view.jsx`, `detail/referral-institutions-detail-view.jsx`, hook list/editor/detail, `referral-institutions-constants.jsx`, `referral-institutions-utils.jsx`, `master-data-referral-institutions-slice.jsx`.
Rawat Jalan: `outpatient-referral-step.jsx`, `outpatient-encounter-referral-modal.jsx`, `use-outpatient-registration.js`, `outpatient-referral.utils.js`, `health-service-select-resources.js`.
Kiosk: `kiosk-old-patient-step-confirm.jsx`, `kiosk-old-patient-view.jsx`, `use-kiosk-new-patient-registration.jsx`, `kiosk-referral.constants.js`, `kiosk-referral-utils.jsx`.

## 4. Verifikasi

| Perintah atau skenario | Hasil |
| --- | --- |
| `npx eslint` pada berkas yang berubah | `PASS` — 0 error, 6 warning pola lama |
| `npm run build` | `PASS` |
| AUTOMATED TEST | `SKIPPED (opsional)` — repo tidak memakai framework test |
| Uji layar Playwright (FE 3000, API 7184→7185) | 15/15 `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| U1–U4 | Hero tanpa tombol; 4 indikator; tombol Tambah di header filter; 7 kolom berurutan | PASS |
| U5 | Refresh memuat ulang dengan filter aktif (`customPeriod=last30days`) | PASS |
| U6–U7 | Form 5 kelompok; submit kosong menampilkan validasi wajib | PASS |
| V1 | Fokus nama + Enter membuka detail | PASS |
| V2 | Detail: histori PKS, status, wilayah, audit | PASS |
| V3 | Modal perpanjangan menolak isian kosong | PASS |
| V4 | Toggle Nonaktifkan/Aktifkan | PASS |
| V5–V6 | Form update terisi (termasuk label provinsi/kota); simpan kembali ke detail | PASS |

Uji layar step rujukan RJ dan Kiosk tidak dijalankan. Kontrak dan penolakan `RJ-VAL-PM-17` pada jalur itu diverifikasi lewat HTTP (laporan backend I1–I10).

## 5. Catatan

| Hal | Isi |
| --- | --- |
| Tampilan telepon | Field grouped existing memformat nomor menjadi `+62 …`; nilai itu lolos validasi backend |
| Roadmap | Tidak diperbarui: task ini tidak tercantum di roadmap |
