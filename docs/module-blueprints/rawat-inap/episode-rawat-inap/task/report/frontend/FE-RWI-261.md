# Laporan Perubahan Frontend — `FE-RWI-261`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-261` |
| Judul | Filter Otomatis dan Seleksi Bed Khusus ODC (`isOdcBed=true`) pada Langkah 6 Wizard Penempatan Bed |
| Modul / Sub-modul | `rawat-inap` / `episode-rawat-inap` |
| Kontrak Target | `0.15.0` (`APPROVED_DESIGN_PLANNED`) |
| Delivery Lock | `ODC-DEL-20261017-01` |
| Tanggal | 17 Oktober 2026 |
| Status | **Selesai (Source Code Updated)** |

## Hasil Implementasi

1. **Parameter Query & Normalisasi Bed ODC (`inpatient-bed-utils.jsx`)**:
   - Memperbarui `normalizeBedBoard` dan `normalizeAvailableBeds` untuk membaca properti `isOdcBed` dari respons server.
   - Memperbarui `buildBedBoardQuery` dan `buildAvailableBedQuery` agar secara otomatis menyertakan parameter `isOdcBed: true` saat pasien bertipe ODC.

2. **Pengendalian Filter & Proteksi Seleksi Bed ODC (`use-inpatient-bed-board.jsx` & `use-inpatient-admission-bed.jsx`)**:
   - Menerima opsi `patientType` dan `isOdcBed` pada `useInpatientBedBoard`.
   - Mengalirkan filter `isOdcBed: true` ke kueri server `bed-board` dan `available-beds`.
   - Pada method `selectBed` di `useInpatientAdmissionBed`, menambahkan proteksi validasi: apabila pasien bertipe ODC namun tempat tidur yang diklik bukan tempat tidur ODC (`!bed.isOdcBed`), pemilihan langsung ditolak dengan pesan: *"Pasien ODC hanya dapat ditempatkan pada tempat tidur khusus One Day Care (ODC)."*

3. **Indikator Visual & Banner Informasi (`InpatientBedBoard` & `InpatientAdmissionBedSelectionStep`)**:
   - Pada papan bed (`InpatientBedBoard`), setiap kartu tempat tidur ODC (`bed.isOdcBed`) kini dilengkapi lencana status pill berwarna amber/warning bertuliskan `"Khusus ODC"`.
   - Pada Langkah 6 Wizard Pemilihan Bed (`InpatientAdmissionBedSelectionStep`), banner informasi biru (`InformationAlert`) ditampilkan di bagian atas dengan pesan: *"Menampilkan tempat tidur khusus perawatan One Day Care (ODC)"*.

## Bukti dan Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Filter Ketersediaan Bed ODC | Parameter `isOdcBed=true` otomatis disertakan pada endpoint ketersediaan bed |
| Lencana Status Tempat Tidur | `StatusBadge` "Khusus ODC" berhasil terpasang pada kartu bed yang memiliki penanda ODC |
| Penegakan Proteksi Seleksi | `selectBed` menolak pemilihan bed non-ODC untuk pasien ODC |
| Banner Informasi Langkah 6 | Banner `InformationAlert` aktif menampilkan informasi filter ODC |
| Kontrak & Linting | Bebas syntax error; mematuhi konvensi UI komponen Quilvian |
| Production Build | Build frontend mandiri oleh user sesuai kesepakatan |

## Batas Perubahan

- Berkas yang diubah:
  1. `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx`
  2. `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx`
  3. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx`
  4. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`
  5. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx`
  6. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
