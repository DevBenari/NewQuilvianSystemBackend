# Laporan Perubahan Frontend — `FE-IGD-056`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-IGD-056` |
| **Judul** | Tab Penunjang di layar perawat membaca status penunjang per kunjungan |
| **Slice** | R3.14 slice P · `SCR-IGD-P01` tab Penunjang Medis |
| **Roadmap** | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| **Requirement** | `FR-IGD-101` (sisi baca perawat); DoD butir 7 |
| **Keputusan** | `IGD-DEC-224`; `IGD-DEC-230` pilihan desain 6 (tata kelola penunjang di layar perawat) |
| **Contract version** | API §10.7 (pembacaan pesanan Laboratorium, Radiologi, Bank Darah berbasis encounter) |
| **Wewenang UI** | `DEV_DISCRETION` mengikuti standar `ClinicalSegmentedNav` dan kartu kerja perawat IGD |
| **Dependency** | `FE-IGD-051` (tab tindakan perawat), master opsi penunjang |
| **Pasangan backend** | `LabOrderController`, `RadOrderController`, `BbkBloodOrderController` |
| **Task mode** | `FRONTEND` — izin implementasi diberikan Rizki |
| **Tanggal** | 12 Oktober 2026 |
| **Status** | ✅ **SELESAI — 19 Oktober 2026 atas penilaian pemilik**: tiga sub-segmen baca-status (Laboratorium, Radiologi, Bank Darah) terpasang dan berjalan, badge counter aktif, lint 0 error. Pemilik menyatakan layar yang dilayani `npm run dev` sah sebagai bukti selama modul masih dalam pengembangan, sehingga `IGD-DEC-296` tidak diberlakukan di kartu ini. **Hemodialisa dikeluarkan dari lingkup** — diperiksa di source 19 Oktober 2026: `emergency-assessment-diagnostic-support-tab.jsx` hanya memuat segmen `laboratory`, `radiology`, dan `blood-bank`, dan di backend tidak ada controller `HemodialysisManagement` yang menerima `encounterId`, sehingga pembacaan pesanan hemodialisa per kunjungan belum mungkin dibuat dari frontend. Jenis keempat menunggu endpoint backend dan dicatat sebagai langkah operasional, bukan kartu baru. *Sebelumnya:* 🟢 **SELESAI — 12 Oktober 2026: implementasi & pengujian live selesai**; sub-segmen terintegrasi, badge counter aktif, lint 0 error, build lulus, live browser testing PASS. |

---

## 1. Masalah yang Diperbaiki

Sebelum perubahan ini, tab Penunjang Medis pada layar pengkajian pasien IGD perawat (`SCR-IGD-P01` / `emergency-assessment-detail-view.jsx`) masih menampilkan banner pemberitahuan statis *"Fitur Belum Tersedia"* dan belum terhubung ke layanan penunjang SIMRS selain laboratorium dasar. Perawat IGD tidak dapat memantau status pesanan radiologi dan permintaan darah yang dipesan untuk pasien IGD tersebut, sehingga perawat harus bertanya secara manual ke instalasi penunjang.

---

## 2. Perubahan yang Dikerjakan

1. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-diagnostic-support-tab.jsx`:
   - Memasang navigasi kapsul segmen `ClinicalSegmentedNav` yang membagi tiga sub-layanan:
     - 🧪 **Laboratorium** (`data-segment="laboratory"`) dengan badge jumlah pesanan aktif.
     - 🩻 **Radiologi** (`data-segment="radiology"`) dengan badge jumlah pesanan aktif.
     - 🩸 **Bank Darah (BDRS)** (`data-segment="blood-bank"`) dengan badge jumlah pesanan aktif.
   - Menghapus blok statis peringatan *"Fitur Belum Tersedia"*.
2. `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx`:
   - Menambahkan endpoint constants: `RAD_ORDER_URL`, `BLOOD_ORDER_URL`.
   - Menambahkan thunk `fetchRadOrders` untuk mengambil riwayat pesanan radiologi berdasarkan `encounterId`.
   - Menambahkan thunk `fetchBloodOrders` untuk mengambil riwayat permintaan darah berdasarkan `encounterId`.
   - Mengelola state `radOrders` dan `bloodOrders` pada reducer.
3. `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx`:
   - Mengonfigurasi `navLoaders["diagnostic-support"]` agar memuat data `fetchLabOrders`, `fetchRadOrders`, dan `fetchBloodOrders` secara bersamaan ketika tab Penunjang Medis dibuka.
   - Mengekspos data `radOrders` dan `bloodOrders` pada properti `sections`.
4. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx`:
   - Meneruskan data `sections` (pesanan lab, radiologi, bank darah) serta data penugasan aktif (`activeAssignment`) ke komponen tab penunjang.

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria Penerimaan | Status | Bukti Verifikasi |
| ---: | --- | :---: | --- |
| 1 | Tab Penunjang perawat membaca status pesanan laboratorium, radiologi, dan bank darah untuk kunjungan IGD aktif | ✅ | Terpasang thunk `fetchLabOrders`, `fetchRadOrders`, `fetchBloodOrders` dan dihubungkan ke tabel riwayat di masing-masing panel |
| 2 | Menampilkan badge jumlah pesanan aktif pada masing-masing kapsul segmen penunjang | ✅ | `ClinicalSegmentedNav` merender badge dinamis dari panjang item pesanan |
| 3 | Pemesanan lab perawat yang sudah ada tetap bekerja dan terintegrasi mulus | ✅ | Panel `EmergencyAssessmentLabPanel` dipertahankan dengan formulir pemesanan dan tabel riwayat |
| 4 | Kualitas kode: `eslint` 0 error, 0 warning; `npm run build` lulus 100% | ✅ | Lolos uji linter ESLint dan kompilasi Next.js exit code 0 |

---

## 4. Bukti Pengujian Antigravity Putaran 4 (12 Oktober 2026)

Pengujian layar langsung (*live browser testing*) dilaksanakan pada 12 Oktober 2026 menggunakan Chromium Headed Mode dan akun perawat nyata `dimas.kurniawan@rsmmc.local`. Laporan resmi tersimpan di [`docs/module-blueprints/igd/testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md`](../../testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md).

| ID Skenario | Skenario Pengujian Layar | Putusan | Hasil Pengamatan | Berkas Bukti |
| --- | --- | :---: | --- | --- |
| `056-U1` | Membuka tab Penunjang Medis di ruang kerja perawat IGD (`SCR-IGD-P01`) | **PASS** | Tab Penunjang terbuka tanpa crash/blank, identitas pasien dan navigasi segmen aktif termuat | `056-U1.json`<br>`056-U1-tab-penunjang.png` |
| `056-U2` | Sub-segmen Laboratorium (Formulir & Tab Riwayat Pesanan) | **PASS** | Menampilkan kartu pesan lab, input catatan, dan tab *Daftar Pesanan* | `056-U2.json`<br>`056-U2-order-lab.png` |
| `057-U3` | Pelacakan status pesanan penunjang & batas wewenang perawat | **PASS** | Perawat memantau status pesanan tanpa akses entri nilai hasil klinis | `057-U3.json`<br>`057-U3-status-tracking.png` |
