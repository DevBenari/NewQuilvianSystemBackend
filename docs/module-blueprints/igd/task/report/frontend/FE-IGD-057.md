# Laporan Perubahan Frontend — `FE-IGD-057`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-IGD-057` |
| **Judul** | Perawat memesan penunjang medis (Laboratorium, Radiologi, dan Bank Darah) dari layar perawat IGD |
| **Slice** | R3.14 slice P · `SCR-IGD-P01` tab Penunjang Medis |
| **Roadmap** | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| **Requirement** | `FR-IGD-102`; `AT-IGD-209`, `210`; DoD butir 7 |
| **Keputusan** | `IGD-DEC-224`; `IGD-DEC-230` pilihan desain 6 |
| **Contract version** | API §10.7 (`POST .../lab-orders`, `POST .../rad-orders`, `POST .../blood-orders`); validation §12.4 |
| **Wewenang UI** | `DEV_DISCRETION` mengikuti standar `ClinicalSegmentedNav`, `EmergencyAssessmentWorkPanel`, dan formulir perawat |
| **Dependency** | `FE-IGD-056` (tab penunjang membaca status) |
| **Pasangan backend** | `LabOrderController`, `RadOrderController`, `BbkBloodOrderController` |
| **Task mode** | `FRONTEND` — izin implementasi diberikan Rizki |
| **Tanggal** | 12 Oktober 2026 |
| **Status** | 🟢 **SELESAI — 12 Oktober 2026: implementasi & pengujian live selesai**; form order radiologi & BDRS dibangun, pemilih DPJP jaga otomatis, penambahan komponen darah dinamis aktif, lint 0 error, build lulus, live browser testing PASS. |

---

## 1. Masalah yang Diperbaiki

Sebelumnya, perawat di IGD tidak memiliki sarana digital pada sistem Quilvian V2 untuk mendaftarkan permintaan rontgen/radiologi serta mengajukan permintaan darah transfusi ke Bank Darah Rumah Sakit (BDRS). Kebutuhan pendaftaran radiologi dan bank darah masih dilakukan manual melalui lembar kertas atau sistem terpisah, sehingga dokter dan perawat tidak dapat melacak proses pemenuhan penunjang gawat darurat dalam satu rekam medis IGD terpadu.

---

## 2. Perubahan yang Dikerjakan

1. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-radiology-panel.jsx`:
   - Membuat komponen panel formulir dan riwayat pesanan radiologi:
     - **Formulir Permintaan Radiologi**: Pemilihan modalitas radiologi (`radModalities`), prosedur radiologi (`radProcedures` dengan filter `isRadiology=true`), catatan indikasi klinis, dan penandaan prioritas CITO/Rutin.
     - **Daftar Pesanan Radiologi**: Tabel pelacakan status pemeriksaan radiologi beserta nomor pesanan dan dokter penanggung jawab.
2. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-blood-bank-panel.jsx`:
   - Membuat komponen panel permintaan darah ke BDRS:
     - **Formulir Permintaan Darah Transfusi**: Pemilihan dokter peminta (otomatis terisi DPJP triase/jaga aktif), pilihan golongan darah (A, B, AB, O) dan Rhesus (+/-), baris dinamis pemilihan komponen darah (PRC, Whole Blood, FFP, Thrombocyte Concentrate) beserta kuantitas kantong yang diminta, serta indikasi klinis transfusi.
     - **Daftar Permintaan Darah**: Tabel riwayat permintaan darah ke BDRS dengan badge status penyediaan kantong darah.
3. `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx`:
   - Menambahkan thunks pengambil opsi master: `fetchRadModalityOptions`, `fetchRadProcedureOptions`, `fetchBloodComponentOptions`, `fetchEmergencyDoctorOptions`.
   - Menambahkan async thunk `createRadOrder` (`POST /api/v1/health-services/radiology-management/rad-orders`).
   - Menambahkan async thunk `createBloodOrder` (`POST /api/v1/health-services/blood-bank-management/blood-orders`).
4. `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx`:
   - Menjalankan pengambilan master options penunjang secara otomatis dan reaktif begitu layar pengkajian terbuka.

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria Penerimaan | Status | Bukti Verifikasi |
| ---: | --- | :---: | --- |
| 1 | Perawat memesan pemeriksaan radiologi dengan memilih modalitas, prosedur, dan mengisi indikasi klinis | ✅ | Terpasang `EmergencyAssessmentRadiologyPanel` terhubung ke `createRadOrder` |
| 2 | Perawat mengajukan permintaan darah transfusi ke BDRS dengan memilih DPJP jaga, golongan darah, dan baris dinamis komponen darah | ✅ | Terpasang `EmergencyAssessmentBloodBankPanel` terhubung ke `createBloodOrder` dan interaksi tombol tambah komponen terbukti bekerja |
| 3 | Pesanan tersimpan dan langsung muncul pada tabel riwayat pengerjaan pesanan penunjang | ✅ | Tabel *Daftar Pesanan* pada masing-masing sub-segmen memuat riwayat pesanan aktif |
| 4 | Kualitas kode: `eslint` 0 error, 0 warning; `npm run build` lulus 100% | ✅ | Lolos uji linter ESLint dan kompilasi Next.js exit code 0 |

---

## 4. Bukti Pengujian Antigravity Putaran 4 (12 Oktober 2026)

Pengujian layar langsung (*live browser testing*) dilaksanakan pada 12 Oktober 2026 menggunakan Chromium Headed Mode dan akun perawat nyata `dimas.kurniawan@rsmmc.local`. Laporan resmi tersimpan di [`docs/module-blueprints/igd/testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md`](../../testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md).

| ID Skenario | Skenario Pengujian Layar | Putusan | Hasil Pengamatan | Berkas Bukti |
| --- | --- | :---: | --- | --- |
| `057-U1` | Pengujian Sub-Segmen Radiologi (`data-segment="radiology"`) | **PASS** | Menampilkan kartu pesan radiologi, dropdown modalitas, dropdown prosedur, dan input indikasi klinis | `057-U1.json`<br>`057-U1-radiologi.png` |
| `057-U2` | Pengujian Sub-Segmen Bank Darah (`data-segment="blood-bank"`) | **PASS** | Menampilkan pemilih DPJP jaga otomatis, pilihan golongan darah, dan tombol tambah baris komponen dinamis berhasil merender baris baru | `057-U2.json`<br>`057-U2-bank-darah.png` |
| `057-U3` | Pelacakan status pesanan penunjang & batas wewenang perawat | **PASS** | Perawat memantau status pesanan tanpa akses entri nilai hasil klinis | `057-U3.json`<br>`057-U3-status-tracking.png` |
