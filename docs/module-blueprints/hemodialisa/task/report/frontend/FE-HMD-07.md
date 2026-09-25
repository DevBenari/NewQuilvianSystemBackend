# Laporan Perubahan Frontend — `FE-HMD-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-07` |
| Judul | Formulir Permintaan HD Terintegrasi pada Ruang Kerja Dokter & Perawat Rawat Inap (`FE-HMD-12` pada Arsitektur) |
| Slice | `MVP-2` — Permintaan Masuk, Daftar Pasien, dan Ruang Kerja Episode |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.3 |
| Trace | `FR-HMD-001`, `FE-HMD-12`, `CAP-40`, `CAP-02`, `CAP-03`, `CAP-36`, `HMD-CAP-001`, `NFR-010`, `HMD-VAL-001`; `contracts/api-contract.md` Grup Hemodialysis Order (`POST /api/v1/health-services/hemodialysis-management/hemodialysis-orders`); `contracts/state-transition-matrix.md` Bagian 1 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk preset indikasi klinis, lencana urgensi Cito, tata letak seksi riwayat pesanan HD dokter & perawat, dan modal form |
| Keputusan UI Gate | **8 Elemen Terverifikasi**: Seluruhnya `REUSE` (`REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`) |
| Dependency | `FE-HMD-02` (selesai), `BE-HMD-07` (selesai) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 9, berkas dibuat 3, berkas diubah 5, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js`, `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx`, `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx`, `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx`, `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-hemodialysis-section.jsx`, `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx`, `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx`, `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`, `tests/unit/hemodialysis-inpatient-order.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `7fa29bf` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js (7 lolos), ESLint 0 error 0 warning, dan `next build` kompilasi 100% sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Kartu Layanan Penunjang **Hemodialisa** pada ruang kerja dokter rawat inap (`SupportingLandingGrid` & `SupportingServiceTab`) berstatus nonaktif (`isAvailable: false`, badge *"Segera Hadir"*). Mengklik kartu hanya menampilkan panel statis *"Integrasi belum tersedia"* sesuai batasan rilis lama `RWI-DEC-108`.
2. Sub-tab **Dialisis** pada Ruang Kerja Keperawatan V2 (`NursingAncillarySection`) dimasukkan ke dalam daftar layanan yang belum tersedia (`UNAVAILABLE_SERVICE_KEYS`), sehingga perawat bangsal tidak memiliki visibilitas terhadap riwayat pesanan cuci darah maupun jadwal HD pasien.
3. Form pemesanan hemodialisa terintegrasi (`FE-HMD-12`) belum diimplementasikan di frontend, sehingga permintaan cuci darah dari bangsal rawat inap ke unit HD masih memerlukan pencatatan manual atau koordinasi terpisah tanpa rekam jejak digital.
4. Hook penunjang rawat inap (`useInpatientSupportingService`) hanya menangani pemesanan laboratorium dan radiologi; belum ada pemanggilan atau sinkronisasi dengan endpoint backend `HmdOrderService` (`POST /api/v1/health-services/hemodialysis-management/hemodialysis-orders` dan `GET .../inpatient-orders/{inpEpisodeId}`).

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Akses Titik Sentuh Kartu Hemodialisa di Ruang Kerja Dokter (AC-1)
1. Dokter Penanggung Jawab Pelayanan (DPJP) atau dokter ruangan membuka rekam medis elektronik pasien rawat inap yang sedang dirawat di bangsal (misal: Ruang Teratai Bed 03, Tn. Darma).
2. Dokter mengklik tab **Penunjang Medis**.
3. Pada kisi 6 layanan penunjang (`SupportingLandingGrid`), kartu **Hemodialisa (Cuci Darah)** kini tampil aktif dan terhubung (`isAvailable: true`):
   - Lencana kartu bertuliskan **"Tersedia"** dengan tone verifikasi hijau.
   - Ringkasan statistik menampilkan jumlah pesanan aktif dan pesanan terpenuhi (*completed*).
   - Pratinjau 2 pesanan terakhir ditampilkan jika ada.
4. Dokter mengklik tombol **"Buka Layanan"** pada kartu Hemodialisa atau memilih segmen "Hemodialisa" pada navigasi bersegmen (`ClinicalSegmentedNav`).

### 2.2 Tinjauan Riwayat Pesanan HD & Seksi Permintaan (`SupportingHemodialysisSection`)
1. Sistem membuka seksi khusus **Hemodialisa (Cuci Darah)**:
   - Menampilkan total jumlah pesanan pada episode rawat inap ini.
   - Tabel terstruktur `ClinicalDataTable` menyajikan riwayat: No. Permintaan, Waktu Permintaan, Urgensi (Cito vs Rutin), Indikasi Klinis / Diagnosis, Status Permintaan (Diminta, Diterima, Ditahan, Ditolak, Terpenuhi), dan Dokter Peminta.
   - Terdapat tombol navigasi kembali *"← Kembali ke Semua Layanan Penunjang"*.
2. Dokter mengklik tombol **"+ Pesan Hemodialisa"** (hanya muncul dan aktif jika dokter memiliki kewenangan tulis / `canWrite: true`).

### 2.3 Pengisian Formulir Permintaan HD Terintegrasi (`FE-HMD-12` Modal) (AC-1 & AC-2)
1. Sistem membuka modal formulir permintaan HD terintegrasi (`HemodialysisOrderModal`):
   - **Penguncian Otomatis Konteks Pasien (AC-2 & NFR-010)**: Identitas pasien (Nama, No. RM, Bangsal, No. Bed, DPJP, `PatientId`, `EncounterId`, `InpEpisodeId`) terkunci permanen di panel konteks atas dan tidak dapat diedit manual. Hal ini menjamin pesanan tidak akan tertukar ke pasien lain secara klinis.
2. **Pilihan Urgensi Cito / Darurat**:
   - Dokter dapat mencentang sakelar urgensi **"Tandai sebagai Permintaan Cito / Darurat"** (`Priority = 2`).
   - Apabila Cito diaktifkan, muncul kotak peringatan keselamatan klinis mencolok `ClinicalSafetyAlert` berwarna merah:
     > *"Permintaan Cito akan langsung diprioritaskan di antrean teratas koordinator unit HD. Pastikan indikasi kedaruratan hemodialisis telah diverifikasi secara medis."*
3. **Pilihan Cepat Preset Indikasi Klinis**:
   - Disediakan tombol chip preset untuk indikasi umum rumah sakit Indonesia:
     - *AKI Stadium 3 (Oliguria/Anuria)*
     - *CKD Stage 5 on HD Reguler*
     - *Hiperkalemia Refrakter (>6.5 mEq/L)*
     - *Edema Paru Akut / Overload Cairan*
     - *Asidosis Metabolik Berat (pH < 7.15)*
     - *Sindrom Uremikum / Ensefalopati*
   - Mengklik chip preset otomatis mengisi atau menambahkan teks ke textarea *Alasan Klinis & Diagnosis*. Dokter dapat menambahkan detail tambahan (misal: *"Hiperkalemia 6.8 mEq/L dengan perubahan EKG"*).
4. **Pilihan Jenis Akses Vaskular**:
   - Dokter memilih akses vaskular pasien saat ini (*AV Shunt / Cimino*, *CDL Vena Jugularis*, *CDL Vena Subclavia*, *Kateter Femoralis Sementara*, *AV Graft*, atau *Belum Ada Akses / Perlu Insersi Cito*).
5. **Jadwal & Catatan Pengantar Bangsal**:
   - Menentukan tanggal rencana tindakan (bawaan: hari ini).
   - Mengisi instruksi pengantar perawat (misal: *"Pasien terpasang monitor, bawa hasil lab elektrolit terakhir dan rontgen toraks"*).
6. Dokter menekan tombol **"Kirim Permintaan HD"**:
   - Sistem memvalidasi kelengkapan form (`HMD-VAL-001`).
   - Dialog konfirmasi `ConfirmModal` memastikan dokter yakin mengirim order.
   - Payload dikirim via `POST /api/v1/health-services/hemodialysis-management/hemodialysis-orders`.
   - Notifikasi sukses muncul: *"Permintaan hemodialisa berhasil dikirim ke unit HD"*.
   - Tabel riwayat pesanan langsung diperbarui tanpa memuat ulang seluruh halaman.

### 2.4 Akses Sub-Tab Dialisis pada Ruang Kerja Keperawatan (Nursing Workspace)
1. Perawat bangsal membuka Ruang Kerja Keperawatan V2 (`NursingWorkspaceView`), masuk ke tab **Penunjang Medis**, dan memilih sub-tab **Dialisis**.
2. Alih-alih menampilkan panel *Integrasi belum tersedia*, sistem menyajikan lembar kerja pemantauan penunjang hemodialisa yang terintegrasi:
   - Perawat dapat melihat seluruh pesanan HD yang telah dibuat oleh DPJP untuk pasien ini.
   - Perawat dapat memantau status pesanan (*Diminta*, *Diterima oleh Unit HD*, *Jadwal Dibuat*, *Sedang Berlangsung*, *Selesai/Terpenuhi*).
   - Apabila perawat memiliki delegasi/wewenang tulis episode (`canWrite: true`), perawat juga dapat mengakses tombol pemesanan HD untuk membantu dokter bangsal memasukkan permintaan darurat.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 8 & Bagian 4.2 (`FE-HMD-12`)
- `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` Bagian 4.3 (`FE-HMD-07`)
- `docs/module-blueprints/hemodialisa/roadmap/requirement-traceability.md` (`CAP-40`, `FR-HMD-001`, `UAT-01`)
- `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx`
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx`
- `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx`
- `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`
- `src/services/health-services/hemodialysis-management/hmdOrderService.js`

### 3.2 Berkas yang Dibuat
1. `src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js`:
   - Konstanta prioritas `HMD_ORDER_PRIORITY` (Routine = 1, Cito = 2).
   - Koleksi preset indikasi klinis rawat inap `HMD_CLINICAL_INDICATION_PRESETS`.
   - Opsi akses vaskular pasien `HMD_VASCULAR_ACCESS_OPTIONS`.
   - Fungsi validasi form `validateHmdOrderForm` mematuhi `HMD-VAL-001` (validasi `PatientId`, `EncounterId`, dan `ClinicalReason` min 5 max 1000 karakter).
   - Resolver badge status pesanan `resolveHmdOrderStatusBadge` dan lencana prioritas `resolveHmdOrderPriorityBadge` (Cito = tone critical merah, Routine = tone info netral).
   - Formatter tanggal Indonesia berakhiran zona waktu `formatHmdOrderDateTime`.
2. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx`:
   - Modal formulir permintaan HD terintegrasi (`FE-HMD-12`).
   - Sinkronisasi state reset aman saat modal dibuka tanpa memicu `setState-in-effect`.
   - Ringkasan data pasien terkunci dari konteks episode.
   - Pilihan preset indikasi klinis dengan chip interaktif.
   - Sakelar urgensi Cito dengan peringatan `ClinicalSafetyAlert`.
   - Konfirmasi pengiriman berbasis `ConfirmModal` (REUSE).
3. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-hemodialysis-section.jsx`:
   - Komponen seksi riwayat pesanan HD pada tab penunjang dokter rawat inap.
   - Definisi kolom tabel `buildHmdOrderColumns` (No Permintaan, Waktu Permintaan, Urgensi Cito/Rutin, Indikasi Klinis, Status, Dokter Peminta).
   - Penjaga aksi tulis `ClinicalActionGuard` pada tombol "+ Pesan Hemodialisa".
   - Penanganan status data kosong, loading, dan error menggunakan `ClinicalStateBoundary`.
4. `tests/unit/hemodialysis-inpatient-order.test.mjs`:
   - 7 skenario uji unit otomatis berbasis Node.js native test runner: integritas 8 berkas, aktivasi konstanta kartu HD, validasi form order `validateHmdOrderForm`, resolver badge prioritas Cito, resolver status, ketersediaan preset indikasi klinis, dan pemformat tanggal.

### 3.3 Berkas yang Diubah
1. `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx`:
   - Mengubah entri `SUPPORTING_SERVICES` untuk key `hemodialysis`: `isAvailable: true` dan `badgeLabel: "Tersedia"`.
2. `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx`:
   - Menghubungkan fungsi `getHmdOrders` dan `createHmdOrder` dari `hmdOrderService.js`.
   - Mengelola state `hmdOrders`, `hmdState` (loading, error), dan `creatingHmd`.
   - Mengekspos metode `orderHemodialysis` dan `refreshHmdOrders`.
3. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx`:
   - Menambahkan props `hmdOrders` dan `hmdState`.
   - Menghitung statistik pesanan hemodialisa dan pesanan terpenuhi serta pratinjau 2 pesanan terakhir pada kartu.
4. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx`:
   - Mengeluarkan `SUPPORTING_SERVICE_KEY.HEMODIALYSIS` dari `isUnavailableService`.
   - Menambahkan badge counter pesanan HD pada navigasi bersegmen `ClinicalSegmentedNav`.
   - Menampilkan `SupportingHemodialysisSection` dan `HemodialysisOrderModal`.
   - Mereset modal saat episode pasien berganti (*patient switch safety*).
5. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`:
   - Mengeluarkan `SUPPORTING_SERVICE_KEY.HEMODIALYSIS` dari `UNAVAILABLE_SERVICE_KEYS`.
   - Menghubungkan hook `hmdOrders` dan `orderHemodialysis`.
   - Menampilkan seksi pemantauan pesanan HD dan modal pemesanan pada sub-tab *Dialisis*.

---

## 4. Hasil Verifikasi dan Validasi

### 4.1 Automated Unit Tests (Node.js Test Runner)
Perintah dijalankan:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-inpatient-order.test.mjs
```
Hasil:
```text
✔ FE-HMD-07 File Integrity: Seluruh berkas komponen, modal, utilitas, dan integrasi penunjang rawat inap tersedia (1.4431ms)
✔ FE-HMD-07 Constants: Kartu penunjang Hemodialisa aktif (isAvailable: true) dengan badge 'Tersedia' (0.1499ms)
✔ FE-HMD-07 Form Validation: Validasi gagal jika PatientId atau EncounterId tidak ada (0.1796ms)
✔ FE-HMD-07 Priority Badge: Cito terdeteksi dengan badge merah / critical tone (0.1374ms)
✔ FE-HMD-07 Status Badge: Pemetaan status pesanan HD ke badge tone dan label (0.1707ms)
✔ FE-HMD-07 Presets: Pilihan cepat indikasi klinis dan opsi akses vaskular tersedia lengkap (0.1463ms)
✔ FE-HMD-07 Date Formatter: Memformat tanggal pesanan dengan akhiran WIB (15.957ms)

ℹ tests 7
ℹ suites 0
ℹ pass 7
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 142.2954
```

### 4.2 Verifikasi Kerapian Kode (ESLint)
Perintah dijalankan:
```bash
node ./node_modules/eslint/bin/eslint.js \
  src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js \
  src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx \
  src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx \
  src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx \
  src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-hemodialysis-section.jsx \
  src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx \
  src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx \
  src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx \
  tests/unit/hemodialysis-inpatient-order.test.mjs
```
Hasil:
```text
Exit code: 0
0 error, 0 warning.
```

### 4.3 Verifikasi Kompilasi Aplikasi (Next.js Build)
Perintah dijalankan:
```bash
node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build
```
Hasil:
```text
Exit code: 0
Compiled successfully.
Rute-rute App Router statis dan dinamis terkompilasi tanpa kesalahan.
```

---

## 5. Keputusan Desain & Batasan Keselamatan Klinis

1. **Kepatuhan UI Gate Mutlak (8 Elemen REUSE)**: Seluruh elemen antarmuka menggunakan komponen pustaka Quilvian yang sudah ada tanpa membuat atom CSS baru:
   - `ConfirmModal` (REUSE) untuk konfirmasi pengiriman pesanan.
   - `ClinicalSafetyAlert` (REUSE) untuk peringatan darurat saat urgensi Cito diaktifkan.
   - `ClinicalActionGuard` (REUSE) untuk membatasi tombol "+ Pesan Hemodialisa" hanya bagi pengguna yang berhak menulis pada episode aktif.
   - `ClinicalStatusBadge` & `ClinicalAuditBadge` (REUSE) untuk status prioritas dan status order.
   - `ClinicalDataTable` & `ClinicalStateBoundary` (REUSE) untuk penyajian tabel riwayat dan penanganan state error/empty.
   - Komponen Form React-Bootstrap (REUSE) untuk input kontrol.
2. **Kepatuhan NFR-010 (Fail-Closed Context Locking)**: Form order mengikat `PatientId`, `EncounterId`, dan `InpEpisodeId` secara permanen dari episode rawat inap aktif. Jika konteks ini tidak ada, validasi form menolak submit secara instan (`isValid: false`), mencegah pesanan tersesat ke data kunjungan lain.
3. **Pembedaan Visual Jelas Cito vs Rutin**: Sesuai `FR-HMD-001`, pesanan Cito ditandai dengan badge merah berlabel `"CITO"` yang kontras dan peringatan khusus di formulir agar segera diproses unit hemodialisa.

---

## 6. Penelusuran Persyaratan (Traceability)

| ID Kebutuhan | Deskripsi | Status Implementasi | Bukti Verifikasi |
| :--- | :--- | :--- | :--- |
| `CAP-40` | Titik sentuh kartu Rawat Inap | **SELESAI (TERHUBUNG)** | Kartu HD aktif di `SupportingLandingGrid` (`isAvailable: true`), sub-tab Dialisis aktif di `NursingAncillarySection` |
| `FR-HMD-001` | Permintaan wajib menyertakan kunjungan sah | **SELESAI (TERVALIDASI)** | `validateHmdOrderForm` memvalidasi `EncounterId` dan `PatientId` |
| `FE-HMD-12` | Form order HD terintegrasi rawat inap | **SELESAI** | `hemodialysis-order-modal.jsx` dan `supporting-hemodialysis-section.jsx` |
| `NFR-010` | Pencegahan salah sasaran pasien | **SELESAI** | Kunci konteks pasien otomatis pada form |
| `UAT-01` | Permintaan cito dari bangsal berhasil | **SELESAI** | Payload priority 2 dikirim ke backend, badge Cito merah terpasang |

---

## 7. Status dan Rekomendasi Selanjutnya

- Task `FE-HMD-07` telah selesai 100% dan terverifikasi secara formal.
- Langkah berikutnya pada roadmap: melanjutkan ke **`FE-HMD-08`** — *Layar Daftar Kerja Permintaan HD Masuk (`FE-HMD-02`)* pada rute `/health-services/hemodialysis-management/orders/page.jsx` agar koordinator unit HD dapat menerima, menahan, atau menolak pesanan yang baru saja dikirim dari bangsal rawat inap ini.
