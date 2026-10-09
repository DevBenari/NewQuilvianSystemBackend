# Laporan Perubahan Frontend — `FIX-KEP-012-02`

## Metadata

| Field | Nilai |
|---|---|
| **Task ID** | `FIX-KEP-012-02` |
| **Judul** | Modal Pemesanan Laboratorium Khusus Perawat via Delegasi DPJP (`NursingLabOrderModal`) |
| **Isu Terkait** | [`../../docs/issue/issue-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md`](../../docs/issue/issue-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md) — `ISS-KEP-012-02` |
| **Plan Repair** | [`../../docs/plan-repair/plan-repair-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md`](../../docs/plan-repair/plan-repair-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md) |
| **Roadmap Terkait** | [`../../roadmap/frontend-roadmap-finishing.md`](../../roadmap/frontend-roadmap-finishing.md) — `FE-RWI-089` (AC-6), integrasi `BE-RWI-104` |
| **Traceability** | `FR-DOK-106`; `RWI-DEC-153`; `BE-RWI-104`; `VAL-DOK-46`, `VAL-DOK-47`; api-contract 0.6.0 bagian 12.12 |
| **Contract Version** | `1.0.0` |
| **Dependency** | `BE-RWI-104` (Kolom instruksi pada pesanan Lab & Radiologi), `useAncillaryOrderRequester` |
| **Klasifikasi** | `FEATURE / INTEGRATION` — Membuka pemesanan laboratorium bagi perawat rawat inap dengan penegakan tata kelola delegasi instruksi dokter DPJP aktif |
| **Task Mode** | `CROSS-REPO` — Source code di `QuilvianSystemFrontendDev`; laporan dan plan-repair di `NewQuilvianSystemBackend` |
| **Tanggal** | 7 Oktober 2026 |
| **Status** | ✅ **SELESAI.** Seluruh kriteria penerimaan terbukti penuh. Automated unit test 5/5 PASS pada `tests/unit/nursing-lab-order.test.mjs`. ESLint 0 error 0 warning. |

---

## 1. Masalah yang Diselesaikan

Pada menu **Penunjang Medis** (`NursingAncillarySection`) sub-tab **Laboratorium**, tombol **`+ Pesan Laboratorium`** sebelumnya dinonaktifkan (*disabled*) dengan catatan: *"Pemesanan dari sisi perawat sedang disiapkan. Hasil pemeriksaan tetap dapat dibaca di bawah."*

Penonaktifan tersebut awalnya merupakan gerbang keselamatan klinis (*clinical safety guard*) sesuai `AC-6` sebelum backend `BE-RWI-104` selesai diimplementasikan, karena perawat dilarang meresepkan tes penunjang mandiri tanpa instruksi DPJP. Setelah backend `BE-RWI-104` selesai dan menyediakan kolom `InstructingDoctorId` serta status verifikasi `Pending`, antarmuka formulir pemesanan khusus perawat (`NursingLabOrderModal`) perlu dibangun agar perawat dapat menginput pesanan atas instruksi lisan DPJP secara resmi ke dalam sistem.

---

## 2. Perubahan yang Dilakukan

1. **Pembuatan Komponen Modal `NursingLabOrderModal`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-lab-order-modal.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-lab-order-modal.jsx)
   - Antarmuka modal lapang (`size="lg"`) dengan kartu konteks pasien rawat inap terkunci (Nama Pasien, No. RM, Bed & Kamar, DPJP Pasien).
   - Memanfaatkan hook [`useAncillaryOrderRequester`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-ancillary-order-requester.js) untuk memuat daftar dokter aktif yang bertugas pada episode pasien rawat inap.
   - Pilihan wajib **Dokter Pemberi Instruksi \*** dengan validasi inline client-side (pesanan tidak dapat dikirim bila dokter belum dipilih).
   - Dropdown pemilihan pemeriksaan laboratorium (dari katalog `labCatalog` atau fallback pemeriksaan rawat inap standar).
   - Sakelar prioritas urgensi (**Rutin** vs **CITO / Segera**) disertai *ClinicalSafetyAlert* saat CITO dipilih.
   - Kotak catatan indikasi klinis dengan tombol pilihan cepat (*preset*) evaluasi Hb post-transfusi, febris hari ke-3, skrining pra-operasi, dsb.
   - Banner edukasi tata kelola klinis (`BE-RWI-104`) yang menjelaskan bahwa pesanan akan berstatus `Pending` menunggu verifikasi DPJP.

2. **Pembaruan Utilitas & Hook Supporting Service**:
   - Berkas: [`src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx)
     - `buildLabOrderCreatePayload`: menambahkan dukungan penerusan `instructingDoctorId`.
     - `normalizeLabOrder`: memetakan `orderNumber`, `instructingDoctorId`, `instructingDoctorName`, dan `instructionVerificationStatus`.
   - Berkas: [`src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx)
     - `orderLab`: menambahkan parameter opsional `instructingDoctorId` dan menyalurkannya ke payload pembuatan order.

3. **Pengaktifan Tombol Pesan & Integrasi Modal di `NursingAncillarySection`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx)
   - Mengaktifkan tombol **`+ Pesan Laboratorium`** (warna primer) saat `canWrite = true`, yang memicu pembukaan `NursingLabOrderModal`.
   - Mendestrukturisasi `labCatalog`, `orderLab`, dan `creatingLab` dari `useInpatientSupportingService`.

4. **Pembaruan Tabel Riwayat Pesanan `NursingAncillaryOrderTable`**:
   - Berkas: [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-order-table.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-order-table.jsx)
   - Menambahkan kolom **Verifikasi DPJP** untuk pemeriksaan laboratorium.
   - Menampilkan badge status instruksi: `⏳ Menunggu DPJP` (warning) atau `✓ Terverifikasi DPJP` (success) lengkap dengan nama dokter pemberi instruksi.

---

## 3. Hasil Validasi

### 3.1 Automated Tests
```text
AUTOMATED TEST: node --test tests/unit/nursing-lab-order.test.mjs — PASS
✔ FIX-KEP-012-02 AC-1 & BE-RWI-104: buildLabOrderCreatePayload menyertakan instructingDoctorId (8.15ms)
✔ FIX-KEP-012-02 AC-2: normalizeLabOrder memuat instructingDoctorId, instructingDoctorName, dan status verifikasi (1.46ms)
✔ FIX-KEP-012-02 AC-3: NursingLabOrderModal memuat alur delegasi DPJP, prioritas, dan validasi form (1.74ms)
✔ FIX-KEP-012-02 AC-4: NursingAncillarySection mengaktifkan tombol Pesan Laboratorium dan menghubungkan modal (2.74ms)
✔ FIX-KEP-012-02 AC-5: NursingAncillaryOrderTable memuat kolom Verifikasi DPJP dan badge status instruksi (1.73ms)
tests: 5, pass: 5, fail: 0 (100% PASS)
```

### 3.2 ESLint Validation
```text
AUTOMATED TEST: node ./node_modules/eslint/bin/eslint.js (5 files) — PASS
0 errors, 0 warnings
```

---

## 4. Evaluasi Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti |
|---|:---:|---|
| 1. Tombol `+ Pesan Laboratorium` aktif dan membuka jendela modal pemesanan | ✅ Terpenuhi | `NursingAncillarySection` merender tombol aktif saat `canWrite` dan memicu `NursingLabOrderModal` |
| 2. Pemesanan gagal simpan di sisi client bila dokter pemberi instruksi belum dipilih | ✅ Terpenuhi | Validasi inline di `NursingLabOrderModal` memeriksa `requester.selectedDoctorId` sebelum submit |
| 3. Pesanan yang berhasil dikirim tercatat dengan status verifikasi `Pending` dan muncul di tabel riwayat laboratorium | ✅ Terpenuhi | Payload membawa `instructingDoctorId` ke endpoint `BE-RWI-104`, dan tabel merender badge `Menunggu DPJP` |
