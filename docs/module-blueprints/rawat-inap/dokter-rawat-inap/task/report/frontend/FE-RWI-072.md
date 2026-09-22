# Laporan Perubahan Frontend — `FE-RWI-072`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-072` |
| Judul | Tab Resep: Rekonsiliasi Obat dan Sliding Scale (`FE-DOK-10` pada `FE-DOK-09`) |
| Slice | Gelombang 3 — `PRD-RWI-V2-001`, `EPIC DOK-13`, `EPIC DOK-14` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-072` |
| Trace | `FR-DOK-092`, `FR-DOK-093`, `FR-DOK-096`, `FR-DOK-097`, `FR-DOK-098`, `FR-DOK-099`; `RWI-DEC-132`, `RWI-DEC-133`, `RWI-DEC-134`, `RWI-DEC-146`; `VAL-DOK-52`, `VAL-DOK-52a`, `VAL-DOK-55`, `VAL-DOK-55a`, `VAL-DOK-55b`, `VAL-DOK-55c`, `VAL-DOK-55d`; `BE-RWI-101` [BE], `BE-RWI-103` [BE] |
| Contract version | `0.6.0` API rekonsiliasi obat bawaan dan API sliding scale order |
| Wewenang UI | Dua sub-tab tambahan pada layar `FE-DOK-10` (Tab Resep) di dalam kerangka `FE-DOK-09` |
| Dependency | `FE-RWI-071` ✅ selesai 17 September 2026; `BE-RWI-101` [BE] ✅ selesai 16 September 2026; `BE-RWI-103` [BE] ✅ selesai 17 September 2026 |
| Klasifikasi | `HEAVY` — pengelolaan terapi obat bawaan berisiko tinggi (Rekonsiliasi) dan protokol insulin berversi (Sliding Scale) |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-6) terbukti pada source code dan verifikasi build/test. |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum implementasi task ini diselesaikan:
1. **Ketiadaan Pengambilan Keputusan Obat Bawaan Pasien di Frontend (`AC-1`, `FR-DOK-092`):**
   Obat yang dibawa pasien dari rumah (misal: obat hipertensi rutin, antidiabetes oral) dicatat perawat saat admisi rawat inap, namun dokter tidak memiliki antarmuka untuk memutuskan nasib setiap obat: apakah **Lanjut Sama**, **Lanjut Ubah**, atau **Hentikan**. Ketiadaan keputusan ini berisiko menyebabkan terapi terputus atau dosis ganda.
2. **Ketiadaan Riwayat Jejak Keputusan Obat Bawaan (`AC-2`, `FR-DOK-093`):**
   Bila dokter mengganti keputusan (misal: semula obat Amlodipin diputuskan "Lanjut Sama", lalu diubah menjadi "Hentikan"), antarmuka lama hanya mampu melihat status terakhir. Riwayat dokter pemutus sebelumnya, waktu keputusan, dan catatan klinis penggantian hilang dari pengamatan medis.
3. **Celah Penanganan Galat `409 Conflict` Saat Resep Sudah Aktif (`AC-3`, `VAL-DOK-52a`):**
   Setelah draft resep hasil rekonsiliasi diselesaikan dan menjadi resep aktif di Instalasi Farmasi, dokter tidak boleh lagi mengubah keputusan rekonsiliasi secara sepihak. Backend mengembalikan status HTTP `409 Conflict`. Tanpa penanganan khusus di frontend, galat ini tampil sebagai error sistem umum yang membingungkan staf klinis.
4. **Ketiadaan Penyalinan Rentang Protokol ke Order Pasien (`AC-4`, `FR-DOK-097`):**
   Pada sistem lama, protokol sliding scale hanya menautkan ID template. Akibatnya, jika komite medis memperbarui rentang protokol sliding scale di tingkat rumah sakit (misal dari versi 1 ke versi 2), dosis insulin pasien yang sedang berjalan di bangsal diam-diam berubah tanpa instruksi dokter.
5. **Celah Penyesuaian Dosis Insulin Tanpa Alasan Klinis (`AC-5`, `VAL-DOK-55b`):**
   Bila dokter memodifikasi dosis insulin per rentang gula darah (misal memotong dosis separuh karena pasien sensitif insulin), sistem belum mewajibkan pengisian alasan klinis sebelum order disimpan.
6. **Ketiadaan Pratinjau Kalkulasi Dosis GDS Sebelum Disimpan (`AC-6`, `FR-DOK-096`):**
   Dokter tidak dapat mensimulasikan hasil dosis insulin untuk nilai gula darah tertentu secara interaktif sebelum menyimpan order, sehingga risiko salah perhitungan rentang dosis baru terdeteksi saat perawat hendak menyuntikkan insulin.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Skenario Rekonsiliasi Obat Bawaan Pasien (`AC-1`, `AC-2`, `AC-3`)
1. **Pencatatan Awal oleh Perawat:**
   Saat pasien Tn. Budi masuk rawat inap, Ns. Siti mencatat obat yang dibawa pasien dari rumah:
   - Amlodipin 10 mg (1×1 oral)
   - Metformin 500 mg (3×1 oral)
   Data tersimpan dengan status keputusan awal `Pending` ("Belum Diputuskan").
2. **Keputusan oleh DPJP (dr. Ahmad):**
   - dr. Ahmad membuka Tab **Resep** sub-segmen **Rekonsiliasi Obat**.
   - Untuk Amlodipin: dr. Ahmad menekan **"Lanjut Sama"**. Sistem membentuk butir obat ke dalam draf resep rawat inap Tn. Budi dengan dosis dan frekuensi yang identik.
   - Untuk Metformin: dr. Ahmad menekan **"Lanjut Ubah"**, memasukkan catatan: *"Dosis diturunkan menjadi 1x500 mg karena fungsi ginjal menurun"*. Butir masuk ke draf resep untuk disesuaikan aturan pakainya.
3. **Penggantian Keputusan & Riwayat Audit (`AC-2`):**
   - Sore harinya, dr. Ahmad memeriksa hasil lab fungsi ginjal terbaru dan memutuskan Metformin harus dihentikan total.
   - dr. Ahmad menekan **"Hentikan"** pada baris Metformin dan mengisi alasan: *"eGFR < 30 ml/menit, hentikan Metformin"*.
   - Saat tombol **"Riwayat Keputusan"** ditekan, modal menampilkan lini masa urutan keputusan:
     - *Keputusan #2: Hentikan (dr. Ahmad, 17:00, "eGFR < 30 ml/menit, hentikan Metformin") — Menggantikan keputusan sebelumnya.*
     - *Keputusan #1: Lanjut Ubah (dr. Ahmad, 10:00, "Dosis diturunkan menjadi 1x500 mg").*
4. **Proteksi Resep Aktif (`AC-3`):**
   - Jika resep rawat inap yang memuat Amlodipin telah difinalisasi dan diproses apotek, lalu dr. Ahmad mencoba menekan "Hentikan" pada rekonsiliasi, backend menolak dengan HTTP `409`.
   - Frontend menampilkan banner bahaya klinis:
     > *"Resep hasil keputusan sebelumnya sudah aktif. Ubah terapi lewat resep rawat inap atau hentikan obatnya melalui menu penghentian resep (VAL-DOK-52a)."*

### 2.2 Skenario Protokol & Order Sliding Scale Insulin (`AC-4`, `AC-5`, `AC-6`)
1. **Pemilihan Protokol Sah:**
   - dr. Ahmad membuka sub-segmen **Sliding Scale** dan menekan **"Pesan Protokol Sliding Scale"**.
   - dr. Ahmad memilih protokol *"SSI-DEWASA-REGULER (v2)"* dan jadwal cek *"Sebelum Makan & Sebelum Tidur (4× sehari)"*.
2. **Penyalinan Rentang Mandiri (`AC-4`):**
   - Sistem secara otomatis menyalin rentang dosis dari versi template ke formulir order pasien:
     - `< 150 mg/dL` → `0 unit` (Rutin)
     - `150 – 200 mg/dL` → `2 unit` (Rutin)
     - `200 – 250 mg/dL` → `4 unit` (Rutin)
     - `250 – 300 mg/dL` → `6 unit` (Rutin)
     - `≥ 300 mg/dL` → `8 unit` (⚠️ Wajib Lapor Dokter)
   - Rentang ini tersimpan sebagai data mandiri milik pasien. Perubahan template master di kemudian hari tidak akan memengaruhi terapi Tn. Budi.
3. **Penyesuaian Dosis & Kewajiban Alasan Klinis (`AC-5`):**
   - Mengetahui Tn. Budi memiliki riwayat hipoglikemia, dr. Ahmad menurunkan dosis rentang `250 – 300 mg/dL` dari 6 unit menjadi 3 unit.
   - Sistem mendeteksi `isOrderAdjusted = true` dan mengunci tombol konfirmasi:
     > *"Dosis disesuaikan dari template standar. Wajib mengisi alasan penyesuaian klinis (VAL-DOK-55b)."*
   - dr. Ahmad mengisi alasan: *"Pasien sangat sensitif insulin dan memiliki riwayat hipoglikemia nokturnal"*. Tombol simpan kini aktif.
4. **Simulasi Pratinjau Interaktif Sebelum Simpan (`AC-6`):**
   - Di bagian bawah form, kotak simulator interaktif langsung menampilkan pratinjau kalkulasi:
     > *"GDS 280 mg/dL → 3 unit (disesuaikan: Pasien sangat sensitif insulin dan memiliki riwayat hipoglikemia nokturnal)"*
   - dr. Ahmad memastikan simulasi telah tepat dan menekan **"Pesan Protokol"**. Nomor order bisnis diterbitkan: `SSO-2026-000001`.

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate`)

| Kebutuhan Antarmuka | Komponen Terpilih | Path Sumber Komponen | Status | Alasan & Rekomendasi |
| :--- | :--- | :--- | :---: | :--- |
| Sub-navigasi Tab Resep (Buat, Template, History, Harian, Rekonsiliasi, Sliding Scale) | `ClinicalSegmentedNav` | `src/components/ui/doctor-clinical-base` | `REUSE` | Menggunakan navigasi segmen baku dengan lencana jumlah item obat bawaan dan order aktif. |
| Banner Peringatan Klinis & Penolakan 409 | `ClinicalSafetyAlert` | `src/components/ui/doctor-clinical-base` | `REUSE` | Nada `danger` untuk penolakan 409 resep aktif dan penghentian order; nada `warning` untuk kewajiban alasan penyesuaian insulin. |
| Lencana Status Keputusan & Order | `ClinicalStatusBadge` | `src/components/ui/doctor-clinical-base` | `REUSE` | Menampilkan label status keputusan rekonsiliasi dan status aktif/dihentikan sliding scale. |
| Batas Status Asinkron (Loading, Empty, Error) | `ClinicalStateBoundary` | `src/components/ui/doctor-clinical-base` | `REUSE` | Mengelola pemuatan data rekonsiliasi dan order pasien secara terisolasi. |
| Penjaga Hak Akses Aksi Medis | `ClinicalActionGuard` | `src/components/ui/doctor-clinical-base` | `REUSE` | Mengunci tombol aksi keputusan dan order jika pengguna bukan dokter dengan penugasan aktif. |
| Modal Keputusan, Detail, dan Konfirmasi Penghentian | `ConfirmModal` | `src/components/features/base-features/confirm-modal` | `REUSE` | Dialog modal konfirmasi standar sistem Quilvian. |
| Tombol Aksi | `BaseButton` | `src/components/features/base-features/base-button` | `REUSE` | Tombol primer, outline, dan bahaya sesuai design token. |
| Kontrol Form (Input Nilai, Dropdown, Textarea) | `BaseTextField`, `BaseNativeSelectField`, `BaseTextAreaField` | `src/components/features/base-features/base-form-control` | `REUSE` | Seluruh kontrol input memakai komponen form baku. |
| Panel Tata Letak Rekonsiliasi & Sliding Scale | Panel Komposisi | Disesuaikan dengan tata letak ruang kerja dokter | `COMPOSE` | Merangkai komponen base di atas ke dalam layout responsif. |

> **Keputusan Base Component:** Seluruh elemen berstatus `REUSE` dan `COMPOSE`. **Nol (0) komponen berstatus `NEW`.**

---

## 4. Berkas yang dibuat dan dimodifikasi

### 4.1 Frontend Repository (`QuilvianSystemFrontendDev`)

| Berkas | Jenis Perubahan | Keterangan |
| --- | :---: | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-prescription-constants.jsx` | Modifikasi | Mendaftarkan sub-segmen `RECONCILIATION` dan `SLIDING_SCALE` ke `INPATIENT_PRESCRIPTION_SEGMENTS`; mendefinisikan enum keputusan rekonsiliasi, rute obat bawaan, status order sliding scale, satuan gula darah, dan pesan penolakan `409`. |
| `src/lib/services/health-services/pharmacy-management/medication-reconciliation.service.js` | **Baru** | Service Axios untuk modul Rekonsiliasi: `getReconciliationsByEpisode`, `recordHomeMedication`, `cancelReconciliationItem`, `decideReconciliation`, dan `getReconciliationDecisions`. |
| `src/lib/services/health-services/pharmacy-management/sliding-scale.service.js` | **Baru** | Service Axios untuk modul Sliding Scale: `getSlidingScaleTemplates`, `getSlidingScaleTemplateById`, `getSlidingScaleOrdersByEpisode`, `getSlidingScaleOrderById`, `createSlidingScaleOrder`, `adjustSlidingScaleOrder`, dan `stopSlidingScaleOrder`. |
| `src/utils/health-services/inpatient-management/inpatient-reconciliation-utils.jsx` | **Baru** | Utilitas normalisasi daftar obat bawaan, pemetaan format rute obat, pengurutan riwayat keputusan, dan penanganan ramah HTTP `409 Conflict` (`VAL-DOK-52a`). |
| `src/utils/health-services/inpatient-management/inpatient-sliding-scale-utils.jsx` | **Baru** | Utilitas kalkulasi dosis insulin dari nilai GDS, format batas rentang, deteksi penyesuaian dosis dari template (`isAdjusted`), validasi alasan klinis (`VAL-DOK-55b`), dan generator format simulasi pratinjau (`FR-DOK-096`). |
| `tests/unit/inpatient-sliding-scale-utils.test.mjs` | **Baru** | Suite test unit otomatis (6 pengujian lolos) untuk kalkulasi dosis, pencocokan rentang, validasi alasan penyesuaian, dan format error 409. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-reconciliation.jsx` | **Baru** | Hook terpadu pengelola state obat bawaan episode, pembukaan modal keputusan, penyimpanan keputusan dokter, dan pembacaan riwayat keputusan ber-audit trail. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-sliding-scale.jsx` | **Baru** | Hook terpadu pengelola daftar order sliding scale episode, pemilihan template sah, penyalinan rentang, kalkulasi simulasi interaktif GDS, penyesuaian versi order, dan penghentian order. |
| `src/style/health-services/inpatient-management/physician-prescription.module.css` | Modifikasi | Menambahkan styling untuk kartu obat bawaan, linimasa riwayat keputusan, tabel rentang sliding scale, dan kotak simulator pratinjau GDS. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-reconciliation-panel.jsx` | **Baru** | Panel tampilan Rekonsiliasi Obat Bawaan dengan aksi Lanjut Sama, Lanjut Ubah, Hentikan, modal keputusan dokter, dan modal riwayat keputusan (`AC-1`, `AC-2`, `AC-3`). |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-sliding-scale-panel.jsx` | **Baru** | Panel tampilan Protokol dan Order Sliding Scale dengan tabel rentang tersalin, validasi alasan penyesuaian, simulator kalkulasi GDS sebelum simpan, dan modal penyesuaian/penghentian order (`AC-4`, `AC-5`, `AC-6`). |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/inpatient-prescription-tab.jsx` | Modifikasi | Menghubungkan hook dan merender panel Rekonsiliasi dan Sliding Scale saat sub-tab aktif, serta menghitung counter badge masing-masing sub-segmen. |

### 4.2 Backend Repository (`NewQuilvianSystemBackend`)

| Berkas | Jenis Perubahan | Keterangan |
| --- | :---: | --- |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/frontend/FE-RWI-072.md` | **Baru** | Laporan resmi tracked implementasi frontend untuk task `FE-RWI-072`. |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` | Modifikasi | Memperbarui kartu `FE-RWI-072` menjadi selesai (`✅ Selesai 17 September 2026`). |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/requirement-traceability-v2.md` | Modifikasi | Memperbarui status requirement `FR-DOK-092`, `FR-DOK-093`, `FR-DOK-096` s.d. `FR-DOK-099` dengan tautan laporan task `FE-RWI-072`. |

---

## 5. Dokumentasi Spesifikasi API (Swagger-Style)

### 5.1 Health Services / Pharmacy Management / Medication Reconciliation

`[Tags("Health Services / Pharmacy Management / Medication Reconciliation")]`

| Method | Path | Deskripsi & Kegunaan | Auth / Hak Akses | Request Body / Params | Response Body (Data) |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/pharmacy-management/medication-reconciliations/episodes/{episodeId}` | Mengambil seluruh obat bawaan pasien dalam satu episode beserta keputusan terakhir. | Bearer Token (`MedicationReconciliation:Read`) | Path: `episodeId` (GUID) | `ApiResponse<List<ReconciliationItemResponse>>` |
| `POST` | `/api/v1/health-services/pharmacy-management/medication-reconciliations/{id}/decisions` | Dokter memutuskan nasib obat: Lanjut Sama (1), Lanjut Ubah (2), Hentikan (3). "Lanjut" mengisi butir draft resep rawat inap. | Bearer Token (`MedicationReconciliation:Decide`) | Path: `id` (GUID)<br>Body: `{ decisionType, decisionNote, targetDraftPrescriptionId }` | `ApiResponse<ReconciliationDecisionResponse>` (HTTP 201 Created; HTTP 409 Conflict jika resep hasil sudah aktif) |
| `GET` | `/api/v1/health-services/pharmacy-management/medication-reconciliations/{id}/decisions` | Mengambil seluruh riwayat keputusan dokter untuk satu obat bawaan secara kronologis. | Bearer Token (`MedicationReconciliation:Read`) | Path: `id` (GUID) | `ApiResponse<List<ReconciliationDecisionResponse>>` |

### 5.2 Health Services / Pharmacy Management / Sliding Scale Order

`[Tags("Health Services / Pharmacy Management / Sliding Scale Order")]`

| Method | Path | Deskripsi & Kegunaan | Auth / Hak Akses | Request Body / Params | Response Body (Data) |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/pharmacy-management/sliding-scale-templates` | Mengambil daftar master template protokol sliding scale yang berstatus aktif. | Bearer Token (`SlidingScaleTemplate:Read`) | Query: `isActive=true` | `ApiResponse<List<SlidingScaleTemplateListItem>>` |
| `GET` | `/api/v1/health-services/pharmacy-management/sliding-scale-templates/{id}` | Mengambil detail template protokol sliding scale beserta seluruh versi dan baris rentangnya. | Bearer Token (`SlidingScaleTemplate:Read`) | Path: `id` (GUID) | `ApiResponse<SlidingScaleTemplateResponse>` |
| `GET` | `/api/v1/health-services/pharmacy-management/sliding-scale-orders/episodes/{episodeId}` | Mengambil daftar order sliding scale per pasien untuk episode rawat inap. | Bearer Token (`SlidingScaleOrder:Read`) | Path: `episodeId` (GUID)<br>Query: `status` (opsional) | `ApiResponse<List<SlidingScaleOrderListItem>>` |
| `POST` | `/api/v1/health-services/pharmacy-management/sliding-scale-orders` | Dokter memesan protokol pada butir insulin draft resep. Rentang template disalin ke order pasien. | Bearer Token (`SlidingScaleOrder:Create`) | Body: `{ prescriptionItemId, templateVersionId, ranges, adjustmentReason, checkFrequencyCode }` | `ApiResponse<SlidingScaleOrderResponse>` (HTTP 201 Created; HTTP 400 jika alasan penyesuaian kosong) |
| `POST` | `/api/v1/health-services/pharmacy-management/sliding-scale-orders/{id}/versions` | Menyesuaikan order insulin berjalan → membentuk versi order baru (N+1). | Bearer Token (`SlidingScaleOrder:Update`) | Path: `id` (GUID)<br>Body: `{ expectedVersionNumber, ranges, adjustmentReason }` | `ApiResponse<SlidingScaleOrderResponse>` (HTTP 200 OK; HTTP 409 jika nomor versi basi) |
| `PATCH` | `/api/v1/health-services/pharmacy-management/sliding-scale-orders/{id}/stop` | Menghentikan order sliding scale pasien. Pelaksanaan berikutnya dihentikan. | Bearer Token (`SlidingScaleOrder:Update`) | Path: `id` (GUID)<br>Body: `{ reason }` | `ApiResponse<SlidingScaleOrderResponse>` (HTTP 200 OK) |

---

## 6. Bukti Verifikasi Otomatis

### 6.1 Test Unit Otomatis (Node.js Test Runner)
```bash
cmd.exe /c node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-sliding-scale-utils.test.mjs"
```
**Hasil:** `PASS`
```text
✔ FE-RWI-072 AC-6: kalkulasi dosis insulin dari GDS mencocokkan rentang dengan tepat (0.7881ms)
✔ FE-RWI-072 AC-4 & AC-5: deteksi penyesuaian dosis insulin dari template standar (0.1695ms)
✔ FE-RWI-072 AC-5: penyesuaian dosis mewajibkan alasan valid (VAL-DOK-55b) (0.1297ms)
✔ FE-RWI-072 AC-6: format pratinjau simulasi kalkulasi dosis tampil sebelum menyimpan (0.157ms)
✔ FE-RWI-072 AC-3: penanganan HTTP 409 Conflict saat resep hasil keputusan sebelumnya aktif (0.2425ms)
✔ FE-RWI-072: format batas rentang gula darah (0.154ms)
ℹ tests 6
ℹ suites 0
ℹ pass 6
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 121.5044
```

### 6.2 Linting (ESLint)
```bash
cmd.exe /c npm run lint
```
**Hasil:** `PASS` (0 error, 692 peringatan legacy tidak terdampak). Seluruh berkas baru dan modifikasi pada task ini lolos validasi tanpa galat.

### 6.3 Build Produksi (Next.js Turbopack)
```bash
cmd.exe /c npm run build
```
**Hasil:** `PASS` (Exit code 0). Seluruh route client dan server terkompilasi, dan postbuild `prepare-standalone` menyalin static/public assets dengan sukses.

---

## 7. Status Pembuktian Acceptance Criteria

| Kriteria | Status | Bukti Implementasi pada Source Code |
| :--- | :---: | :--- |
| **AC-1** (`FR-DOK-092`, PRD bag. 19) | ✅ | Setiap obat bawaan menampilkan pilihan keputusan: **Lanjut Sama** (`ContinueSame`), **Lanjut Ubah** (`ContinueModified`), dan **Hentikan** (`Stopped`) pada `PrescriptionReconciliationPanel`. Pilihan "Lanjut" memanggil endpoint backend yang membentuk butir ke dalam draft resep rawat inap. |
| **AC-2** (`FR-DOK-093`) | ✅ | Tombol "Riwayat Keputusan" membuka modal linimasa yang mengambil data via `getReconciliationDecisions(item.id)`. Menampilkan seluruh urutan keputusan (`sequenceNumber`), nama dokter pemutus, waktu keputusan, dan penanda `supersedesDecisionId`. |
| **AC-3** (`VAL-DOK-52a`) | ✅ | Fungsi `formatReconciliationError` pada `inpatient-reconciliation-utils.jsx` menangkap kode HTTP `409` saat resep hasil sudah aktif dan memunculkan banner bahaya klinis `INPATIENT_PRESCRIPTION_TEXT.conflict409ActivePrescriptionMessage`, bukan pesan galat umum atau crash. |
| **AC-4** (`FR-DOK-097`, `FR-DOK-098`) | ✅ | Pada `useInpatientSlidingScale.jsx`, pemilihan template menyalin baris rentang (`approvedVer.ranges.map`) ke dalam state mandiri `orderRanges`. Baris rentang tersalin ini yang dikirim ke backend, sehingga perubahan template di masa depan tidak mengubah order berjalan. |
| **AC-5** (`VAL-DOK-55b`) | ✅ | Fungsi `detectRangesAdjusted` mendeteksi perbedaan dosis/batas antara order dan template standar. Bila disesuaikan (`isOrderAdjusted = true`), fungsi `isAdjustmentReasonValid` mewajibkan pengisian `adjustmentReason` (minimal 3 karakter); tombol simpan dinonaktifkan bila alasan belum diisi. |
| **AC-6** (`FR-DOK-096`) | ✅ | Komponen `PrescriptionSlidingScalePanel` menyertakan simulator kalkulasi GDS interaktif sebelum menyimpan order. Fungsi `calculateDoseFromGlucose` dan `formatPreviewSimulation` menampilkan kalkulasi dinamis (contoh: *"GDS 280 mg/dL → 3 unit (disesuaikan: pasien sensitif insulin)"*). |
