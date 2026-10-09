# Laporan Implementasi Task FE-RWI-201: Laporan Rawat Inap & Laporan Transfer Ruangan (P2)

## Metadata Task
- **Task ID**: `FE-RWI-201`
- **Modul**: Rawat Inap (`inpatient-management`) & Pelaporan (`inpatient-reports`)
- **Status**: Selesai
- **Requirement / Keputusan**: `FR-RWF-087`, `RWI-DEC-205`, `RWI-DEC-214`, `RWI-DEC-215`, `RWI-DEC-220` butir 4, `IA-INP-05`, `AC-RWF-086`, `AC-RWF-100`, `RWI-AC-340`, `UAT-RWF-20`, `UAT-RWF-44`
- **Kontrak**: Frontend 13.2, 13.4.8, 13.7 (`FE-INP-32`), API 11.7, `02-module-map.md` 7.3
- **Pemilik**: Muhammad Hamzah

---

## 1. Ringkasan Perubahan
1. **API Service (`inpatient-report.service.js`)**:
   - Membangun service untuk endpoint pelaporan rawat inap:
     - `getRoomTransferReport(params)` (`GET /v1/health-services/inpatient-management/reports/room-transfers`): mengambil baris laporan transfer ruangan dan koreksi penempatan dengan pagination.
     - `exportRoomTransferReport(params)` (`GET /v1/health-services/inpatient-management/reports/room-transfers/export`): mengunduh berkas `.xlsx` hasil ekspor data.
2. **Komponen View Laporan (`inpatient-room-transfer-report-view.jsx`)**:
   - Filter Periode & Validasi Rentang Waktu:
     - Tanggal awal dan akhir wajib diisi.
     - Rentang waktu maksimal ≤ 31 hari. Melebihi 31 hari memunculkan peringatan error `VAL-RWF-90` ("Rentang waktu laporan tidak boleh lebih dari 31 hari.").
     - Opsi centang "Sertakan Koreksi Penempatan" (`includeCorrections`, default true).
   - Kolom Tabel & Pembedaan Jenis:
     - Menampilkan Waktu, Pasien & RM, Asal Ruangan & Bed, Tujuan Ruangan & Bed, Alasan Medis, dan Petugas.
     - Kolom Jenis membedakan secara tegas antara perpindahan mutasi (`Transfer`, badge active) dan koreksi salah catat (`Koreksi`, badge warning) sesuai skenario `UAT-RWF-20`.
     - Teks keadaan kosong: "Tidak ada transfer pada periode ini."
   - Kontrol Wewenang & Ekspor Excel:
     - Halaman dilindungi permission `InpatientReport : ReadRoomTransfer`.
     - Tombol "Ekspor Excel" hanya aktif bagi pemegang `InpatientReport : ExportRoomTransfer`.
3. **Route & Menu Sidebar**:
   - Membuat rute halaman di `src/app/health-services/inpatient-management/reports/page.jsx`.
   - Mendaftarkan butir menu ke-10 "Laporan Rawat Inap" (`/health-services/inpatient-management/reports`) pada submenu Rawat Inap di `src/utils/menu-sidebar/menu-items.jsx` dengan konfigurasi `requiredPermission: { resource: "InpatientReport", action: "ReadRoomTransfer" }` (`UAT-RWF-44`, `RWI-AC-340`).

---

## 2. Bukti Verifikasi
- **Unit Test**: `tests/unit/inpatient-room-transfer-report.test.mjs`
  - Hasil: `3 passed, 0 failed` (100% lulus).
  - Menguji keutuhan fungsi service API 11.7, validasi batas waktu 31 hari VAL-RWF-90, pembedaan tanda baris Koreksi UAT-RWF-20, proteksi permission ekspor dan baca, serta registrasi butir menu sidebar.
