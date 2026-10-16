# Laporan Perubahan Frontend — `FE-IGD-064`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-064` |
| Judul | Tampilan dan Penanganan Status Form Dibatalkan Admisi di Layar IGD |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.16 · `SCR-IGD-P01` (tab *Tindak Lanjut*), `P02` (panel form), `P03` (dialog/rincian) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-119`; `AT-IGD-238`; DoD 15 |
| Keputusan | `IGD-DEC-309` |
| Contract version | API **`0.19.0`** §12.8; validation **`0.18.0`** §14.9 (aturan 30); state **`0.13.0`** §11.6; `03-frontend-architecture.md` §17.12 |
| Pasangan backend | `BE-IGD-077` (build 0 error oleh Rizki Gunawan) |
| Dependency | `BE-IGD-077`, `FE-IGD-061` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): 5 berkas IGD; laporan ini, roadmap, traceability |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **SELESAI / DONE — Build & Live Browser Testing Lulus.** Implementasi 5 berkas frontend selesai: penambahan konstanta kejadian `ADMISSION_REJECTED = 8`, tone status lencana merah/danger untuk `CancelledByAdmission`, pemaparan alasan penolakan admisi pada utilitas ringkasan dan detail, lencana dan teks deskripsi penolakan admisi pada panel form perawat IGD, aktivasi kembali tombol pembuatan form baru saat form ditolak admisi, serta penonaktifan tombol *Jalankan* dengan pesan panduan Aturan 30. ESLint 0 error; `npm run build` lulus. Terbukti lulus 100% pada Live Browser Testing Putaran B3 (`2026-10-16-laporan-uji-putaran-b3-transfer-igd.md`). |

### Frontend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Modul | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik | IGD, Product/Domain Owner: Rizki Gunawan |
| Komponen Pendaftaran IGD | **Tidak disentuh** (`emergency-registration` milik Rizki tetap utuh) |
| Konfigurasi Bersama | Nol baris pada `.env`, `package.json`, `next.config.js`, `globals.css` |
| Design System & Base Components | Menggunakan `StatusBadge`, `BaseButton`, `InformationAlert`, dan `ReadOnlyGroup`/`ReadOnlyItem` standar tanpa hardcoded custom CSS baru |
| Linter & Format | `npx eslint` 0 error; CRLF tanpa BOM; nol komentar baru |

---

## 1. Masalah yang Diselesaikan

Berdasarkan keputusan `IGD-DEC-309`, saat petugas admisi menolak/membatalkan form permintaan rawat inap dari antrean transfer rawat inap:
1. Panel perawat IGD (`SCR-IGD-P02`) sebelumnya belum menampilkan lencana status khusus `Dibatalkan admisi — {alasan}` dengan tone peringatan tegas (merah/danger).
2. Catatan alasan penolakan yang diinput oleh petugas admisi belum ditampilkan secara eksplisit pada rincian status form di tab Tindak Lanjut IGD.
3. Tombol *Buat Form Permintaan Rawat Inap* perlu dipastikan aktif kembali (*re-enabled*) setelah pembatalan admisi terjadi, sehingga perawat dapat segera menyusun draf form baru setelah berkoordinasi ulang dengan dokter penanggung jawab (DPJP IGD).
4. Tombol *Jalankan* pada baris tindak lanjut rawat inap harus tetap nonaktif dan menyajikan teks bantuan (*helper text*) panduan operasional yang spesifik sesuai Aturan 30 matriks validasi:
   *"Form permintaan rawat inap ditolak oleh petugas admisi ({alasan}). Buat form permintaan baru sebelum pasien diproses."*

---

## 2. Proses Bisnis & Alur Kerja Antarmuka

| Alur | Trigger | Aktor | Tampilan di Antarmuka IGD |
| ---: | --- | --- | --- |
| 1 | Petugas admisi menolak form transfer dari Antrean Transfer Rawat Inap | Petugas Admisi Rawat Inap | Form transfer berpindah status menjadi `CancelledByAdmission`. Alasan penolakan dicatat dalam kejadian `AdmissionRejected (8)`. |
| 2 | Perawat membuka tab *Tindak Lanjut* pasien IGD | Perawat IGD | - Panel Form Permintaan Rawat Inap menampilkan lencana `Dibatalkan admisi — {alasan}` dengan status `danger` (merah tegas).<br>- Teks deskripsi panel menampilkan: *"Ditolak oleh petugas admisi dengan alasan: {alasan}"*.<br>- Tombol **"Buat Form Baru"** aktif dan dapat ditekan untuk membuka draf baru.<br>- Tombol **"Lihat Form"** tetap dapat dibuka untuk melihat histori form yang ditolak. |
| 3 | Perawat memeriksa baris tindakan pada tindak lanjut Rawat Inap | Perawat IGD | - Tombol *Jalankan* tetap dinonaktifkan (*disabled*).<br>- Di bawah tombol aksi tampil teks keterangan helper:<br>   *"Form permintaan rawat inap ditolak oleh petugas admisi ({alasan}). Buat form permintaan baru sebelum pasien diproses."* |
| 4 | Perawat menekan tombol *Lihat Form* | Perawat IGD | Form terbuka dalam mode baca-saja. Pada kelompok *Isian form*, baris *Alasan penolakan admisi* menyajikan alasan penolakan admisi. Pada seksi *Riwayat*, tercatat kejadian *"Ditolak petugas admisi"* beserta waktu dan nama petugas admisi. |
| 5 | Perawat menekan tombol *Buat Form Baru* | Perawat IGD | Draf formulir baru dibuat. Tombol *Jalankan* beralih menampilkan panduan penyelesaian draf dan permintaan tanda tangan DPJP IGD. |

---

## 3. Rincian Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx` | 1. Menambahkan `ADMISSION_REJECTED: 8` ke dalam `ADMISSION_REQUEST_EVENT_TYPE`.<br>2. Menambahkan label `[ADMISSION_REQUEST_EVENT_TYPE.ADMISSION_REJECTED]: "Ditolak petugas admisi"` ke dalam `ADMISSION_REQUEST_EVENT_LABELS`.<br>3. Mengubah tone `[ADMISSION_REQUEST_DISPLAY_STATUS.CANCELLED_BY_ADMISSION]` menjadi `"danger"`.<br>4. Menambahkan fungsi helper pesan `executeRejectedByAdmission` pada `ADMISSION_REQUEST_TEXT`. |
| `src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js` | 1. Memetakan properti `admissionRejectionReason` pada `normalizeAdmissionRequestSummary` dan `normalizeAdmissionRequestDetail`.<br>2. Memperbarui `resolveAdmissionExecuteBlock` agar memeriksa apakah form berstatus `CANCELLED_BY_ADMISSION` dan mengembalikan pesan `executeRejectedByAdmission`.<br>3. Memperbarui `resolveAdmissionDisplayLabel` agar memformat lencana `Dibatalkan admisi — {alasan}` saat status adalah `CANCELLED_BY_ADMISSION`. |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-panel.jsx` | 1. Menambahkan penanganan status `CANCELLED_BY_ADMISSION` pada `describeLatest` dengan pesan alasan tolak admisi.<br>2. Meneruskan objek `latest` ke `resolveAdmissionDisplayLabel` agar alasan penolakan muncul pada lencana status.<br>3. Menyesuaikan label tombol menjadi `"Buat Form Baru"` ketika terdapat form sebelumnya yang dibatalkan. |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx` | 1. Mengimpor `ADMISSION_REQUEST_DISPLAY_STATUS`.<br>2. Meneruskan objek `detail` ke `resolveAdmissionDisplayLabel` pada lencana header form.<br>3. Menampilkan label *"Alasan penolakan admisi"* pada `ReadOnlyItem` ketika form berstatus `CANCELLED_BY_ADMISSION`. |
| `src/components/view/health-services/emergency-installation-management/doctor-emergency/emergency-admission-signature-dialog.jsx` | Meneruskan objek `detail` ke `resolveAdmissionDisplayLabel` pada ringkasan tanda tangan dokter. |

---

## 4. Bukti Verifikasi

### 4.1 Linter Check (ESLint)
```powershell
npx eslint src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-panel.jsx src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx src/components/view/health-services/emergency-installation-management/doctor-emergency/emergency-admission-signature-dialog.jsx
```
**Hasil:**
`0 error, 0 warning (Exit Code 0)`

### 4.2 Format dan Konvensi Berkas
- **Akhiran Baris:** CRLF tanpa BOM pada seluruh 5 berkas yang disentuh (diverifikasi lewat Node.js byte buffer scan).
- **Komentar Baru:** 0 baris komentar baru (`//`, `/* */`, `{/* */}`).
- **Komponen Pendaftaran IGD:** Nol baris perubahan pada `emergency-registration`.

### 4.3 Build Lulus
```powershell
npm run build
```
Build Next.js App Router lulus (0 error).

---

## 5. Pemenuhan Acceptance Criteria

| # | Kriteria Acceptance | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Panel perawat IGD (`SCR-IGD-P02`) menampilkan lencana `Dibatalkan admisi — {alasan}` dengan status `CancelledByAdmission` saat form ditolak petugas admisi | ✅ LULUS | `resolveAdmissionDisplayLabel` memformat `Dibatalkan admisi — ${reason}`, `ADMISSION_REQUEST_DISPLAY_TONES` bernilai `"danger"`, diteruskan ke `StatusBadge` di panel |
| 2 | Catatan alasan penolakan dari petugas admisi tampil jelas pada rincian status form di tab Tindak Lanjut IGD | ✅ LULUS | Ditampilkan pada teks deskripsi panel (`describeLatest`) dan `ReadOnlyItem` label *"Alasan penolakan admisi"* di `EmergencyAssessmentAdmissionRequestForm` serta riwayat kejadian `AdmissionRejected` (*"Ditolak petugas admisi"*) |
| 3 | Tombol *Buat Form Permintaan Rawat Inap* aktif kembali untuk membuat draf form baru setelah koordinasi ulang dengan DPJP | ✅ LULUS | Form `Cancelled` tidak lagi dianggap form aktif oleh `hasActiveForm`; `canCreate` bernilai `true`; tombol `"Buat Form Baru"` aktif dan memicu `onCreate` |
| 4 | Tombol *Jalankan* pada baris tindak lanjut rawat inap tetap nonaktif dengan teks helper: *"Form permintaan rawat inap ditolak oleh petugas admisi ({alasan}). Buat form permintaan baru sebelum pasien diproses."* | ✅ LULUS | `resolveAdmissionExecuteBlock` mengembalikan format pesan Aturan 30; `executeHold` menonaktifkan tombol *Jalankan* dan menampilkan teks bantuan di baris tindak lanjut |
| 5 | Komponen pendaftaran IGD milik pemilik tidak disentuh; `eslint` 0 error; `npm run build` lulus | ✅ LULUS | Komponen `emergency-registration` tidak disentuh; ESLint 0 error; Next.js build sukses |
