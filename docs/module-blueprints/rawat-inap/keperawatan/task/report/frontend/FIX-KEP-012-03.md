# Laporan Perubahan Frontend — `FIX-KEP-012-03`

## Metadata

| Field | Nilai |
|---|---|
| **Task ID** | `FIX-KEP-012-03` |
| **Judul** | Modal Pemesanan Radiologi Khusus Perawat via Delegasi DPJP (`NursingRadiologyOrderModal`) |
| **Isu Terkait** | [`../../docs/issue/issue-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md`](../../docs/issue/issue-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md) — `ISS-KEP-012-02` |
| **Plan Repair** | [`../../docs/plan-repair/plan-repair-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md`](../../docs/plan-repair/plan-repair-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md) |
| **Roadmap Terkait** | [`../../roadmap/frontend-roadmap-finishing.md`](../../roadmap/frontend-roadmap-finishing.md) — `FE-RWI-089` (AC-6), integrasi `BE-RWI-104` |
| **Traceability** | `FR-DOK-106`; `RWI-DEC-153`; `BE-RWI-104`; `VAL-DOK-46`, `VAL-DOK-47`; api-contract 0.6.0 bagian 12.12 |
| **Contract Version** | `1.0.0` |
| **Dependency** | `BE-RWI-104` (Kolom instruksi pada pesanan Lab & Radiologi), `useAncillaryOrderRequester` |
| **Klasifikasi** | `FEATURE / INTEGRATION` — Membuka pemesanan pencitraan radiologi bagi perawat rawat inap dengan penegakan tata kelola delegasi instruksi dokter DPJP aktif |
| **Task Mode** | `CROSS-REPO` — Source code di `QuilvianSystemFrontendDev`; laporan dan plan-repair di `NewQuilvianSystemBackend` |
| **Tanggal** | 7 Oktober 2026 |
| **Status** | ✅ **SELESAI.** Seluruh kriteria penerimaan terbukti penuh. Automated unit test 5/5 PASS pada `tests/unit/nursing-radiology-order.test.mjs`. ESLint 0 error 0 warning. |

---

## 1. Masalah yang Diselesaikan

Pada menu **Penunjang Medis** (`NursingAncillarySection`) sub-tab **Radiologi**, tombol **`+ Pesan Radiologi`** sebelumnya terkunci berwarna abu-abu (*disabled*) dengan teks: *"Pemesanan dari sisi perawat sedang disiapkan. Hasil pemeriksaan tetap dapat dibaca di bawah."*

Penonaktifan tombol tersebut merupakan pembatas keselamatan klinis (*clinical safety guard*) sesuai `AC-6` karena perawat rawat inap tidak berwenang meresepkan modalitas pencitraan beradiasi ion / kontras tanpa mandat dokter spesialis/DPJP. Setelah backend `BE-RWI-104` menyediakan kolom `InstructingDoctorId` dan penanda verifikasi `Pending` pada tabel `RadOrder`, antarmuka formulir pemesanan khusus perawat (`NursingRadiologyOrderModal`) harus diwujudkan agar alur delegasi instruksi radiologi dapat berjalan secara digital.

---

## 2. Perubahan yang Dilakukan

1. **Pembuatan Komponen Modal `NursingRadiologyOrderModal`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-radiology-order-modal.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-radiology-order-modal.jsx)
   - Antarmuka modal lapang (`size="lg"`) dengan kartu konteks pasien rawat inap terkunci (Nama Pasien, No. RM, Bed & Kamar, DPJP Pasien).
   - Memanfaatkan hook [`useAncillaryOrderRequester`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-ancillary-order-requester.js) untuk memuat daftar dokter aktif yang bertugas pada episode pasien rawat inap.
   - Pilihan wajib **Dokter Pemberi Instruksi \*** dengan validasi inline client-side (pesanan tidak dapat dikirim bila dokter belum dipilih).
   - Dropdown pemilihan **Modalitas Radiologi \*** (X-Ray, USG, CT-Scan, MRI) dan **Prosedur / Pemeriksaan Radiologi \***.
   - Sakelar prioritas urgensi (**Rutin** vs **CITO / Segera**) disertai *ClinicalSafetyAlert* saat CITO dipilih.
   - Panel catatan persiapan klinis dan kewaspadaan khusus keselamatan radiasi (riwayat alergi kontras, puasa pra-tindakan).
   - Kotak catatan indikasi klinis dengan tombol pilihan cepat (*preset*) evaluasi infiltrat pneumonia, konfirmasi posisi NGT/CVC, nyeri akut abdomen, dsb.
   - Banner edukasi tata kelola klinis (`BE-RWI-104`) yang menjelaskan bahwa pesanan akan berstatus `Pending` menunggu verifikasi DPJP.

2. **Pembaruan Utilitas & Hook Supporting Service**:
   - Berkas: [`src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx)
     - `buildRadOrderCreatePayload`: menambahkan dukungan penerusan `isUrgent` dan `instructingDoctorId`.
     - `normalizeRadOrder`: memetakan `orderNumber`, `isUrgent`, `instructingDoctorId`, `instructingDoctorName`, dan `instructionVerificationStatus`.
   - Berkas: [`src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx)
     - `orderRadiology`: menambahkan parameter `isUrgent` dan `instructingDoctorId` dan menyalurkannya ke payload pembuatan order.

3. **Pengaktifan Tombol Pesan & Integrasi Modal di `NursingAncillarySection`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx)
   - Mengaktifkan tombol **`+ Pesan Radiologi`** (warna primer) saat `canWrite = true`, yang memicu pembukaan `NursingRadiologyOrderModal`.
   - Mendestrukturisasi `radProcedures`, `modalities`, `orderRadiology`, dan `creatingRad` dari `useInpatientSupportingService`.

4. **Penyelarasan Kolom Verifikasi pada `NursingAncillaryOrderTable`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-order-table.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-order-table.jsx)
   - Kolom **Verifikasi DPJP** aktif dan konsisten baik untuk pesanan laboratorium maupun radiologi.
   - Menampilkan badge status instruksi: `⏳ Menunggu DPJP` (warning) atau `✓ Terverifikasi DPJP` (success) lengkap dengan nama dokter pemberi instruksi.

---

## 3. Hasil Validasi

### 3.1 Automated Tests
```text
AUTOMATED TEST: node --test tests/unit/nursing-radiology-order.test.mjs — PASS
✔ FIX-KEP-012-03 AC-1 & BE-RWI-104: buildRadOrderCreatePayload menyertakan isUrgent dan instructingDoctorId (4.60ms)
✔ FIX-KEP-012-03 AC-2: normalizeRadOrder memuat orderNumber, isUrgent, dan instruksi DPJP (0.94ms)
✔ FIX-KEP-012-03 AC-3: NursingRadiologyOrderModal memuat alur delegasi DPJP, modalitas, prosedur, dan persiapan klinis (1.48ms)
✔ FIX-KEP-012-03 AC-4: NursingAncillarySection mengaktifkan tombol Pesan Radiologi dan menghubungkan modal (0.92ms)
✔ FIX-KEP-012-03 AC-5: NursingAncillaryOrderTable memuat kolom Verifikasi DPJP untuk Radiologi (0.91ms)
tests: 5, pass: 5, fail: 0 (100% PASS)
```

### 3.2 ESLint Validation
```text
AUTOMATED TEST: node ./node_modules/eslint/bin/eslint.js (6 files) — PASS
0 errors, 0 warnings
```

---

## 4. Evaluasi Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti |
|---|:---:|---|
| 1. Tombol `+ Pesan Radiologi` aktif dan membuka jendela modal formulir radiologi | ✅ Terpenuhi | `NursingAncillarySection` merender tombol aktif saat `canWrite` dan memicu `NursingRadiologyOrderModal` |
| 2. Pesanan tersimpan ke server dengan nomor order radiologi dan status verifikasi `Pending` | ✅ Terpenuhi | Payload membawa `instructingDoctorId` ke endpoint `POST /rad-orders` (`BE-RWI-104`), dan tabel merender badge `Menunggu DPJP` |
| 3. Hasil pesanan langsung tampil pada tabel riwayat penunjang radiologi | ✅ Terpenuhi | `refreshRadOrders` dipanggil saat pemesanan sukses, dan data ter-update di tabel `NursingAncillaryOrderTable` |
