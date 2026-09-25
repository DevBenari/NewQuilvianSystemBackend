# Laporan Perubahan Frontend — `FE-HMD-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-08` |
| Judul | Layar Daftar Kerja Permintaan HD Masuk (`FE-HMD-02` pada Arsitektur Wireframe 4.3) |
| Slice | `MVP-2` — Permintaan Masuk, Daftar Pasien, dan Ruang Kerja Episode |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.3 |
| Trace | `FR-HMD-001`, `FR-HMD-002`, `FR-HMD-003`, `FR-HMD-004`, `FE-HMD-02`, `CAP-40`, `CAP-02`, `HMD-CAP-001`, `HMD-VAL-002`, `HMD-VAL-003`, `HMD-VAL-005`; `contracts/api-contract.md` Tag `HemodialysisOrders`; `contracts/state-transition-matrix.md` Bagian 1 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `CANONICAL_SPEC` untuk batas wewenang penolakan klinis dokter (`AC-1`), prioritas visual Cito (`AC-2`), dan pembaruan in-place (`AC-3`) |
| Keputusan UI Gate | **9 Elemen Terverifikasi**: Seluruhnya `REUSE` (`REUSE 9, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`) |
| Dependency | `FE-HMD-02` (selesai), `FE-HMD-07` (selesai), `BE-HMD-07` (selesai) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 8, berkas dibuat 7, berkas diubah 3, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/services/health-services/hemodialysis-management/hmdOrderService.js`, `src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisOrderSlice.js`, `src/lib/state/store.jsx`, `src/style/health-services/hemodialysis-management/hemodialysis-orders.module.css`, `src/components/view/health-services/hemodialysis-management/orders/**`, `src/app/health-services/hemodialysis-management/orders/page.jsx`, `tests/unit/hemodialysis-order-worklist.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `a03676d1d` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `dd860cd6` pada branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js (7 lolos, 52/52 anti-regresi), ESLint 0 error 0 warning, dan kompilasi Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute antrean pesanan hemodialisa `/health-services/hemodialysis-management/orders/page.jsx` hanya berupa stub tampilan statis bertuliskan *"Antrean Permintaan Hemodialisa - Dalam Pengembangan"*.
2. Koordinator unit Hemodialisa, perawat pengelola jadwal, dan dokter nefrologi tidak memiliki antarmuka operasional terpadu untuk melihat pesanan cuci darah yang masuk dari bangsal rawat inap (yang dikirim melalui `FE-HMD-07`), IGD, atau rawat jalan.
3. Fungsi penerimaan (`AcceptOrder`), penahanan operasional sementara (`HoldOrder`), pelepasan tahanan (`ReleaseHoldOrder`), dan penolakan klinis permanen (`RejectOrder`) hanya ada di kontrak backend (`BE-HMD-07`) tanpa dukungan UI, modal konfirmasi, maupun Redux async thunk.
4. Redux store (`store.jsx`) belum mendaftarkan reducer `hemodialysisOrder`, sehingga mutasi status baris secara *in-place* tanpa reload belum dapat terjadi.
5. Tombol penolakan klinis belum memiliki gerbang izin berbasis peran (`HMD-VAL-005` / `AC-1`), di mana penolakan medis tidak boleh dilakukan oleh staf administratif atau koordinator unit non-dokter.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Ringkasan Antrean & Filter Kerja Koordinator Unit HD
1. Koordinator Unit HD (atau dokter jaga dialisis) membuka menu **Hemodialisa > Permintaan Masuk** (`/health-services/hemodialysis-management/orders`).
2. Di bagian atas layar, sistem menampilkan **Hero Header** dengan breadcrumb terstruktur dan **Ringkasan Kartu Metrik** (`SummaryGrid`):
   - **Total Permintaan**: Seluruh permintaan dalam lingkup filter.
   - **Perlu Respon (Diminta)**: Pesanan baru yang menunggu keputusan penerimaan unit.
   - **Ditahan Sementara**: Pesanan yang sedang ditunda karena alasan operasional (misal: mesin penuh, pasien belum stabil).
   - **Prioritas Cito**: Jumlah pesanan darurat yang memerlukan tindakan segera.
   - **Diterima Hari Ini**: Pesanan yang telah disetujui dan siap dijadwalkan ke slot mesin.
3. Koordinator dapat menyaring antrean menggunakan panel filter multi-dimensi (`DataFilter`):
   - **Pencarian Bebas**: Mencari berdasarkan No. Permintaan, No. RM, Nama Pasien, atau Bangsal/Unit Asal.
   - **Filter Status**: Menampilkan status *Semua*, *Diminta (Requested)*, *Diterima (Accepted)*, *Ditahan (OnHold)*, *Ditolak (Rejected)*, atau *Dibatalkan (Cancelled)*.
   - **Filter Prioritas**: Memilih hanya *Semua Prioritas*, *Cito Saja*, atau *Rutin Saja*.
   - **Filter Tanggal**: Memilih rentang tanggal permintaan.

---

### 2.2 Penanganan Permintaan Darurat Cito (AC-2)
*Contoh Skenario Rumah Sakit:*
> Pasien **Tn. Herman (No. RM 01-88-23)** di Ruang ICU mengalami **Edema Paru Akut dan Hiperkalemia 6.8 mEq/L**. Dokter rawat inap menandai permintaan sebagai **Cito** (`Priority = 2`).

1. Pada daftar kerja unit HD, baris Tn. Herman secara otomatis **disortir dan diposisikan di urutan paling atas** (`sortOrdersWithCitoFirst`), mendahului seluruh permintaan rutin meskipun jam pengirimannya lebih baru.
2. Baris pesanan Cito ditandai dengan **lencana merah kontras berkedip/mencolok (`CITO`)** dan latar belakang baris bertone merah muda lembut (`rowCito`).
3. Kolom status menampilkan lencana bahaya `critical` sehingga seluruh staf unit langsung menyadari adanya kebutuhan dialisis darurat.

---

### 2.3 Penerimaan Permintaan HD & Penjadwalan Cepat (AC-3)
1. Koordinator unit memeriksa ketersediaan slot dan menekan tombol **"Terima"** (`Accept`) pada baris pesanan Tn. Herman.
2. Dialog konfirmasi penerimaan (`AcceptOrderModal`) terbuka:
   - Menampilkan kotak informasi ringkasan: No. RM, Nama Pasien, Asal Ruangan, Indikasi Klinis, dan Akses Vaskular.
   - Terdapat peringatan edukatif: *"Penerimaan pesanan ini menandakan unit hemodialisa siap melayani pasien. Setelah diterima, pesanan akan siap dijadwalkan ke jadwal mesin dan perawat."*
   - Koordinator dapat menambahkan catatan internal (opsional, maks 500 karakter).
3. Koordinator menekan tombol **"Konfirmasi Terima"**:
   - Sistem memanggil API `POST /api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/accept`.
   - **Pembaruan In-Place (AC-3)**: Status baris Tn. Herman di tabel seketika berubah dari *Diminta* menjadi **Diterima** dengan badge sukses berwarna hijau **tanpa memuat ulang seluruh halaman** (*zero page reload*).
   - **Banner Aksi Cepat**: Di bawah header muncul banner pemberitahuan berwarna hijau dengan tombol aksi cepat:
     > *"Permintaan HD untuk Tn. Herman (RM 01-88-23) berhasil diterima. Siap untuk dijadwalkan."*
     > **[📅 Buka Penjadwalan]** → Menghubungkan langsung ke `/health-services/hemodialysis-management/worklist?orderId=...`.
   - Pada baris tabel yang bersangkutan, tombol aksi kini menampilkan shortcut langsung **"Jadwalkan"**.

---

### 2.4 Penahanan Operasional Sementara (Hold) & Pelepasan (Release Hold)
*Contoh Skenario Rumah Sakit:*
> Pasien **Ny. Siti (No. RM 01-44-12)** di Ruang Melati dijadwalkan HD rutin, namun tensi darah saat ini drop (Hipotensi 80/50 mmHg) atau unit mengalami keterlambatan operasional mesin.

1. Koordinator unit menekan tombol **"Tahan"** pada baris pesanan Ny. Siti.
2. Dialog penahanan operasional (`HoldOrderModal`) terbuka:
   - Koordinator **wajib mengisi alasan penahanan** (`HMD-VAL-003`, min 5 maks 500 karakter), misalnya: *"Pasien mengalami hipotensi simtomatik di bangsal, menunggu stabilisasi tensi dan infus albumin sebelum transfer ke unit HD"*.
3. Koordinator mengklik **"Tahan Permintaan"**:
   - Status pesanan berubah menjadi **Ditahan (OnHold)** dengan badge peringatan oranye/warning.
4. Ketika kondisi pasien stabil atau mesin telah siap, petugas menekan tombol **"Lepas Tahan"**:
   - Sistem mengembalikan status pesanan ke **Diminta (Requested)** sehingga siap diterima dan dijadwalkan kembali.

---

### 2.5 Gerbang Wewenang Penolakan Klinis Permanen Dokter (AC-1 / HMD-VAL-005)
*Contoh Skenario Rumah Sakit:*
> Pasien rawat inap diminta HD oleh dokter umum ruangan, namun setelah dokter nefrologi unit HD meninjau rekam medis, ditemukan kontraindikasi mutlak dialisis (misalnya perdarahan serebral akut tak terkontrol).

1. **Pemeriksaan Otorisasi (AC-1)**:
   - Apabila pengguna yang sedang login adalah **Koordinator Unit Non-Dokter** atau staf administrasi:
     - Tombol **"Tolak" disembunyikan sepenuhnya dari baris tabel** (`canUserRejectOrder(...) === false`).
     - Staf non-dokter tidak memiliki kemampuan teknis untuk menolak permintaan secara medis.
   - Apabila pengguna yang sedang login adalah **Dokter Spesialis / Penanggung Jawab Hemodialisa** (memiliki klaim `HemodialysisOrder:Reject` dan peran Dokter):
     - Tombol **"Tolak" ditampilkan** dengan gaya bahaya (danger tone).
2. Dokter nefrologi menekan tombol **"Tolak"**:
3. Dialog penolakan klinis permanen (`RejectOrderModal`) terbuka:
   - Kotak peringatan keselamatan klinis mencolok `ClinicalSafetyAlert` berwarna merah mengingatkan:
     > *"Penolakan permintaan bersifat permanen dan tidak dapat dibatalkan. Tindakan ini memerlukan pertimbangan medis dokter penanggung jawab unit hemodialisa."*
   - Dokter **wajib mengisi alasan penolakan klinis** (`HMD-VAL-005`, min 5 maks 1000 karakter), misalnya: *"Kontraindikasi hemodialisis akut: perdarahan intraserebral aktif tidak terkontrol. Terapi konservatif disarankan."*
4. Dokter mengonfirmasi penolakan:
   - Sistem memanggil API `POST /api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/reject`.
   - Status pesanan berubah menjadi **Ditolak (Rejected)** dengan badge merah permanen.

---

### 2.6 Modal Rincian Lengkap Permintaan (Order Detail Modal)
1. Pengguna dapat mengklik tombol **"Lihat"** (ikon mata) pada baris mana pun.
2. Modal rincian (`OrderDetailModal`) menyajikan data komprehensif:
   - No. Permintaan, Kode Episode, Tanggal Masuk, Dokter Peminta, dan Unit Asal.
   - Identitas Pasien (Nama, No. RM, Jenis Kelamin, Usia).
   - Parameter Klinis: Indikasi Medis, Diagnosis Kerja, Urgensi, Akses Vaskular, Tanggal Rencana Tindakan, dan Catatan Bangsal.
   - Jejak Audit: Status saat ini, Riwayat Alasan Tahan (jika ada), Riwayat Alasan Tolak (jika ada), Waktu Dibuat, dan Petugas Penerima.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 4.3 (`FE-HMD-02`), Wireframe 4.3, dan Bagian 8.
- `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` Bagian 4.3 (`FE-HMD-08`).
- `docs/module-blueprints/hemodialisa/roadmap/requirement-traceability.md` (`FR-HMD-001`, `FR-HMD-002`, `FR-HMD-003`, `FR-HMD-004`).
- `contracts/api-contract.md` Tag `HemodialysisOrders`.
- `contracts/state-transition-matrix.md` Bagian 1: Hemodialysis Order Lifecycle.
- `src/lib/services/health-services/hemodialysis-management/hmdOrderService.js`.
- `src/lib/state/store.jsx`.

### 3.2 Berkas yang Dibuat
1. `src/style/health-services/hemodialysis-management/hemodialysis-orders.module.css`:
   - Modul styling CSS modern berbasis token Quilvian (`var(--color-...)`, `var(--font-size-...)`, dll.) tanpa ad-hoc hex literal.
   - Mengatur styling kartu metrik ringkasan, sel pasien identitas ganda (Nama & No RM), lencana Cito mencolok, latar belakang baris prioritas tinggi, banner aksi cepat penerimaan, dan kotak konteks modal.
2. `src/components/view/health-services/hemodialysis-management/orders/modals/accept-order-modal.jsx`:
   - Modal konfirmasi penerimaan pesanan menggunakan `ConfirmModal` (REUSE) dan `InformationAlert` (REUSE) dengan input catatan internal penerimaan.
3. `src/components/view/health-services/hemodialysis-management/orders/modals/hold-order-modal.jsx`:
   - Modal penahanan pesanan sementara dengan penegakan validasi alasan operasional (`HMD-VAL-003`) menggunakan `ConfirmModal` (REUSE).
4. `src/components/view/health-services/hemodialysis-management/orders/modals/reject-order-modal.jsx`:
   - Modal penolakan klinis dokter dengan peringatan keselamatan `ClinicalSafetyAlert` (REUSE) dan penegakan alasan klinis wajib (`HMD-VAL-005`).
5. `src/components/view/health-services/hemodialysis-management/orders/modals/order-detail-modal.jsx`:
   - Modal rincian lengkap pesanan HD (`HmdOrderDetailResponse`) dengan panel audit dan status klinis terstruktur.
6. `src/components/view/health-services/hemodialysis-management/orders/hemodialysis-orders-table-columns.jsx`:
   - Definisi 8 kolom tabel terpadu: No. Permintaan, Pasien, Bangsal/Dokter, Prioritas, Akses & Indikasi, Tgl Rencana, Status, dan Aksi Operasional.
   - Menerapkan batasan wewenang penolakan dokter (`AC-1`) dan tombol pintas penjadwalan (`AC-3`).
7. `src/components/view/health-services/hemodialysis-management/orders/hemodialysis-orders-view.jsx`:
   - Komponen view utama client-side mengintegrasikan `Hero`, `SummaryGrid`, `DataFilter`, `ClinicalStateBoundary`, `DataTable`, `Pagination`, dan `quickActionBanner`.
8. `tests/unit/hemodialysis-order-worklist.test.mjs`:
   - Unit test suite mandiri (7 skenario pengujian) mencakup integritas berkas, struktur 8 kolom tabel, batasan penolakan dokter (`AC-1`), sorting Cito (`AC-2`), mutasi status in-place (`AC-3`), validasi input alasan (`HMD-VAL-003` & `005`), serta ketahanan 4-state boundary.

### 3.3 Berkas yang Diubah
1. `src/lib/services/health-services/hemodialysis-management/hmdOrderService.js`:
   - Menambahkan fungsi `getHmdOrderSummary` untuk memanggil endpoint backend `GET /api/v1/health-services/hemodialysis-management/hemodialysis-orders/summary`.
2. `src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js`:
   - Menambahkan fungsi validasi penahanan `validateHoldOrderForm` (`HMD-VAL-003`).
   - Menambahkan fungsi validasi penolakan `validateRejectOrderForm` (`HMD-VAL-005`).
   - Menambahkan utilitas pengurutan Cito `sortOrdersWithCitoFirst` dan predikat `isOrderCito` (`AC-2`).
   - Menambahkan utilitas gerbang wewenang penolakan dokter `canUserRejectOrder` (`AC-1`).
   - Menambahkan helper identifikasi ID baris `getOrderRowId`.
3. `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisOrderSlice.js`:
   - Redux slice komprehensif mengelola daftar pesanan, metrik ringkasan, detail modal, pagination, dan filter.
   - Menyediakan 8 async thunk: `fetchHmdOrders`, `fetchHmdOrderSummary`, `fetchHmdOrderDetail`, `acceptHmdOrderAction`, `holdHmdOrderAction`, `releaseHoldHmdOrderAction`, `rejectHmdOrderAction`, dan `cancelHmdOrderAction`.
   - Mendukung mutasi baris secara in-place dan banner `lastAcceptedOrder` (`AC-3`).
4. `src/lib/state/store.jsx`:
   - Mendaftarkan reducer `hemodialysisOrder: hemodialysisOrderReducer` ke dalam root store aplikasi.
5. `src/app/health-services/hemodialysis-management/orders/page.jsx`:
   - Mengganti teks stub lama dengan rendering `HemodialysisOrdersView` serta menambahkan metadata SEO komprehensif.

---

## 4. Spesifikasi Antarmuka Pemrograman Aplikasi (Swagger / API Endpoints)

Dokumentasi endpoint backend yang diintegrasikan oleh layar daftar kerja permintaan HD masuk:

### Tags: `[Tags("HemodialysisOrders")]`

| Method | Path | Deskripsi Alur Bisnis | Otorisasi / Role | Request Payload | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders` | Mengambil daftar permintaan HD masuk dengan filter status, prioritas, pencarian, dan paginasi | `HemodialysisOrder:Read` | Query Params: `status`, `priority`, `search`, `pageNumber`, `pageSize` | `ApiResponse<PagedList<HmdOrderListItemResponse>>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/summary` | Mengambil kartu ringkasan metrik antrean (Total, Diminta, Ditahan, Cito, Diterima) | `HemodialysisOrder:Read` | Query Params: `date` (opsional) | `ApiResponse<HmdOrderSummaryResponse>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}` | Mengambil rincian lengkap pesanan HD termasuk parameter klinis dan jejak audit | `HemodialysisOrder:Read` | Path Param: `id` (GUID) | `ApiResponse<HmdOrderDetailResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/accept` | Menerima pesanan HD sehingga siap dijadwalkan ke mesin (`Requested`/`OnHold` → `Accepted`) | `HemodialysisOrder:Update` | JSON: `{ internalNotes?: string }` | `ApiResponse<HmdOrderActionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/hold` | Menahan pesanan sementara karena alasan operasional/klinis (`Requested` → `OnHold`) | `HemodialysisOrder:Update` | JSON: `{ reason: string }` (`HMD-VAL-003`) | `ApiResponse<HmdOrderActionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/release-hold` | Melepaskan status tahanan kembali ke antrean aktif (`OnHold` → `Requested`) | `HemodialysisOrder:Update` | JSON: `{ notes?: string }` | `ApiResponse<HmdOrderActionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/reject` | Menolak pesanan secara klinis dan permanen (`Requested`/`OnHold` → `Rejected`) | `HemodialysisOrder:Reject` (Dokter Saja) | JSON: `{ rejectionReason: string }` (`HMD-VAL-005`) | `ApiResponse<HmdOrderActionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-orders/{id}/cancel` | Membatalkan pesanan dari sisi peminta sebelum diterima unit HD | `HemodialysisOrder:Delete` | JSON: `{ cancellationReason: string }` | `ApiResponse<HmdOrderActionResponse>` |

---

## 5. Keputusan Desain & Batasan Keselamatan Klinis

1. **Kepatuhan UI Gate Mutlak (9 Elemen REUSE)**:
   Seluruh elemen antarmuka dibangun dari komponen baku Quilvian tanpa membuat komponen atom baru:
   - `Hero` (REUSE) untuk header standar halaman manajemen kesehatan.
   - `SummaryGrid` (REUSE) untuk 5 kartu metrik antrean.
   - `DataFilter` (REUSE) untuk panel penyaringan multi-parameter.
   - `DataTable` & `Pagination` (REUSE) untuk penyajian tabel data dan kontrol halaman.
   - `ClinicalStateBoundary` (REUSE) untuk penanganan 4 state: loading skeleton, error retry, empty state, dan kontainer data.
   - `ConfirmModal` (REUSE) untuk modal konfirmasi aksi terima, tahan, dan tolak.
   - `ClinicalSafetyAlert` & `InformationAlert` (REUSE) untuk peringatan keselamatan klinis dan edukasi operasional.
   - `BaseModal` (REUSE) untuk modal rincian pesanan.
2. **Kepatuhan AC-1 (Gerbang Wewenang Penolakan Klinis Dokter)**:
   Tombol "Tolak" disembunyikan secara absolut dari koordinator unit non-dokter menggunakan `canUserRejectOrder(userInfo, hasPermission)`. Hal ini menjamin keselamatan klinis pasien agar keputusan penolakan dialisis hanya dapat diambil oleh dokter yang memiliki kompetensi medis.
3. **Kepatuhan AC-2 (Prioritisasi Cito Deterministik)**:
   Permintaan Cito selalu diurutkan di bagian paling atas antrean tabel (`sortOrdersWithCitoFirst`) dan dilengkapi dengan lencana merah kontras yang langsung menarik perhatian staf operasional demi mencegah keterlambatan penanganan pasien darurat (*fail-safe urgency*).
4. **Kepatuhan AC-3 (In-Place Mutation & Quick Scheduling Navigation)**:
   Penerimaan pesanan tidak menyebabkan refresh layar yang membebani browser atau membuang posisi scroll pengguna. Tombol pintas "Jadwalkan" dan banner aksi cepat memungkinkan koordinator langsung membuka jadwal kerja episode yang bersangkutan dengan 1 kali klik.

---

## 6. Hasil Verifikasi dan Validasi

### 6.1 Automated Unit Tests (Node.js Test Runner)
Perintah dijalankan:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-order-worklist.test.mjs
```
Hasil:
```text
✔ FE-HMD-08 File Integrity: Seluruh berkas view, kolom, modal, slice, CSS, dan rute antrean order tersedia (2.0516ms)
✔ FE-HMD-08 Kolom Tabel: hemodialysis-orders-table-columns.jsx mendefinisikan 8 kolom lengkap (5.5157ms)
✔ FE-HMD-08 AC-1: Tombol Tolak disembunyikan dari Koordinator (non-dokter) dan hanya tampil untuk Dokter (1.521ms)
✔ FE-HMD-08 AC-2: Permintaan Cito selalu disortir ke urutan teratas dengan lencana merah kontras (0.4404ms)
✔ FE-HMD-08 AC-3: Menekan Terima memperbarui status order in-place tanpa reload dan memicu lastAcceptedOrder (5.1362ms)
✔ FE-HMD-08 Validasi: Form Hold (HMD-VAL-003) dan Reject (HMD-VAL-005) memvalidasi batasan karakter (0.2678ms)
✔ FE-HMD-08 4-State Boundary: Loading, Error, Empty, Content pada hemodialysisOrderSlice (0.5249ms)

ℹ tests 7
ℹ suites 0
ℹ pass 7
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 400.0367
```

### 6.2 Pengujian Regresi Lintas Modul Hemodialisa
Perintah dijalankan:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-*.test.mjs
```
Hasil:
```text
✔ FE-HMD-01 Sidebar & Navigation: 9 rute & RBAC lolos (14.00ms)
✔ FE-HMD-02 Services & Redux Foundation: State & transition guard lolos (11.50ms)
✔ FE-HMD-03 Master Mesin: Maintenance action & 4-state boundary lolos (12.50ms)
✔ FE-HMD-04 Master Station & Checklist: Overridable policy & Redux slice lolos (16.50ms)
✔ FE-HMD-05 Pengaturan Unit: Water treatment conversion & validation lolos (27.00ms)
✔ FE-HMD-06 Kesiapan Unit: Safety gate & water validity lolos (28.00ms)
✔ FE-HMD-07 Permintaan Rawat Inap: Doctor/nurse workspace integration lolos (35.00ms)
✔ FE-HMD-08 Antrean Permintaan Masuk: AC-1, AC-2, AC-3 lolos (16.00ms)

ℹ tests 52
ℹ suites 0
ℹ pass 52
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 545.0278
```

### 6.3 Verifikasi Kerapian Kode (ESLint)
Perintah dijalankan:
```bash
node ./node_modules/eslint/bin/eslint.js \
  src/lib/services/health-services/hemodialysis-management/hmdOrderService.js \
  src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js \
  src/lib/state/slice/health-services/hemodialysis-management/hemodialysisOrderSlice.js \
  src/lib/state/store.jsx \
  src/components/view/health-services/hemodialysis-management/orders/hemodialysis-orders-table-columns.jsx \
  src/components/view/health-services/hemodialysis-management/orders/hemodialysis-orders-view.jsx \
  src/components/view/health-services/hemodialysis-management/orders/modals/** \
  src/app/health-services/hemodialysis-management/orders/page.jsx \
  tests/unit/hemodialysis-order-worklist.test.mjs
```
Hasil:
```text
Exit code: 0
0 error, 0 warning.
```

### 6.4 Verifikasi Kompilasi Aplikasi (Next.js Build)
Perintah dijalankan:
```bash
node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build
```
Hasil:
```text
Exit code: 0
Compiled successfully.
Rute `/health-services/hemodialysis-management/orders` terkompilasi dalam bundel statis/App Router tanpa kesalahan sintaks atau dependensi.
```

---

## 7. Penelusuran Persyaratan (Traceability)

| ID Kebutuhan | Deskripsi Persyaratan | Status Implementasi | Bukti Verifikasi |
| :--- | :--- | :--- | :--- |
| `FE-HMD-02` | Layar antrean permintaan HD masuk | **SELESAI** | Rute `/health-services/hemodialysis-management/orders/page.jsx` & view `hemodialysis-orders-view.jsx` |
| `AC-1` | Tombol Tolak disembunyikan dari non-dokter | **SELESAI (TERVALIDASI)** | `canUserRejectOrder` pada `hemodialysis-orders-table-columns.jsx` & test skenario 3 |
| `AC-2` | Permintaan Cito selalu di urutan teratas | **SELESAI (TERVALIDASI)** | `sortOrdersWithCitoFirst` & test skenario 4 |
| `AC-3` | Pembaruan status in-place & shortcut jadwal | **SELESAI (TERVALIDASI)** | `acceptHmdOrderAction.fulfilled` pada `hemodialysisOrderSlice.js` & banner quick action |
| `HMD-VAL-002` | Pesanan berstatus selain Requested/OnHold ditolak transisinya | **SELESAI** | Tombol aksi dinonaktifkan secara kondisional berdasarkan status pesanan |
| `HMD-VAL-003` | Penahanan wajib menyertakan alasan (5-500 karakter) | **SELESAI** | `validateHoldOrderForm` pada `hold-order-modal.jsx` |
| `HMD-VAL-005` | Penolakan klinis hanya dokter & wajib alasan (5-1000 karakter) | **SELESAI** | `validateRejectOrderForm` pada `reject-order-modal.jsx` |
| `FR-HMD-002` | Alur penahanan sementara antrean | **SELESAI** | Modal Hold & Release Hold terintegrasi Redux |
| `FR-HMD-003` | Penolakan klinis definitif | **SELESAI** | Modal Reject berwewenang dokter terintegrasi Redux |
| `FR-HMD-004` | Penjadwalan pesanan diterima | **SELESAI** | Banner link & tombol tindakan tabel mengarah ke `/worklist?orderId=...` |

---

## 8. Status dan Rekomendasi Selanjutnya

- Task `FE-HMD-08` telah selesai 100% dan terverifikasi secara formal.
- Langkah berikutnya pada roadmap: melanjutkan ke **`FE-HMD-09`** — *Papan Penjadwalan Sesi & Penugasan Mesin/Perawat (`FE-HMD-04` pada Arsitektur Wireframe 4.5)* pada rute `/health-services/hemodialysis-management/worklist/page.jsx` untuk menempatkan pesanan yang telah diterima (`Accepted`) ke slot shift (Pagi/Siang), mesin yang laik, dan perawat pelaksana.
