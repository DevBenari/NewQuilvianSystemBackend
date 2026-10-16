# Laporan Perubahan Frontend — `FE-IGD-066`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-066` |
| Judul | Field Seleksi Tingkat Ketergantungan Pasien pada Form Permintaan Rawat Inap & Tampilan Cetak Surat Pengantar |
| Slice | `EPIC IGD-15` · Vertical Slice B4 / Amandemen Analis |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Requirement | `FR-IGD-121`; `AT-IGD-241`; DoD 15 |
| Keputusan | `IGD-DEC-312` (Penambahan Tingkat Ketergantungan Pasien pada Form Permintaan Ranap & Surat Pengantar) |
| Pasangan backend | `BE-IGD-079` (`EmergencyPatientDependencyLevel` & model migration) |
| Dependency | `emergency-admission-request-constant.jsx`, `emergency-admission-request.utils.js`, `use-emergency-admission-request.jsx`, `emergency-assessment-admission-request-form.jsx`, `emergency-assessment-admission-request-print.jsx`, `emergency-admission-signature-dialog.jsx` |
| Task mode | `FRONTEND` |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2` |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **DONE — 16 Oktober 2026** |

---

## 1. Masalah yang Diselesaikan

1. **Kebutuhan Identifikasi Tingkat Ketergantungan Pasien Rawat Inap:**
   Berdasarkan permintaan analis rumah sakit pada alur transfer pasien rawat inap IGD (`IGD-DEC-312`), perawat IGD perlu menetapkan tingkat ketergantungan pasien (*patient dependency level*) sebelum pasien dikirim ke ruang rawat inap. Data ini sangat krusial bagi kepala ruangan rawat inap untuk merencanakan alokasi perawat pelaksana, rasio beban asuhan, dan kebutuhan tempat tidur khusus.
2. **Ketiadaan Pilihan pada Form Permintaan Rawat Inap:**
   Sebelumnya, formulir permintaan rawat inap (`EmergencyAssessmentAdmissionRequestForm`) hanya memuat unit layanan, kelas perawatan, DPJP usulan, estimasi lama rawat, indikasi klinis, dan kebutuhan khusus (ICU, isolasi, oksigen, monitoring khusus) tanpa ada seleksi klasifikasi ketergantungan.
3. **Penyelarasan Gate Tanda Tangan & Cetakan Dokumen Fisik:**
   - Validasi gate tanda tangan (`isAdmissionRequestReadyForSignature`) wajib menahan tombol *Minta TTD* apabila perawat belum memilih salah satu tingkat ketergantungan.
   - Dokumen cetak fisik Surat Pengantar Rawat Inap (`EmergencyAssessmentAdmissionRequestPrint`) dan dialog pratinjau verifikasi DPJP IGD (`EmergencyAdmissionSignatureDialog`) harus mencantumkan tingkat ketergantungan ini secara jelas pada seksi *Rencana Penempatan*.

---

## 2. Rincian Perubahan Kode Frontend

### A. Konstanta: `emergency-admission-request-constant.jsx`
- Menambahkan daftar pilihan tingkat ketergantungan:
  ```javascript
  export const ADMISSION_REQUEST_DEPENDENCY_LEVELS = Object.freeze([
    Object.freeze({ value: 1, label: "Low Dependency" }),
    Object.freeze({ value: 2, label: "Moderate Dependency" }),
    Object.freeze({ value: 3, label: "High Dependency" }),
  ]);
  ```
- Menambahkan definisi field pada `ADMISSION_REQUEST_FIELDS`:
  ```javascript
  dependencyLevel: Object.freeze({
    name: "patientDependencyLevel",
    label: "Tingkat Ketergantungan",
    required: true,
    placeholder: "Pilih tingkat ketergantungan",
  }),
  ```
- Memperbarui teks hint `requestSignatureHint` agar menyebutkan tingkat ketergantungan.
- Menambahkan label cetak `dependencyLevelLabel: "Tingkat Ketergantungan"` pada `ADMISSION_PRINT_TEXT`.

### B. Utilitas & Validasi: `emergency-admission-request.utils.js`
- Mengimpor `ADMISSION_REQUEST_DEPENDENCY_LEVELS`.
- Memperbarui `normalizeAdmissionRequestDetail`: membaca `patientDependencyLevel` (number/null) dan `patientDependencyLevelName` (text).
- Memperbarui `normalizeAdmissionPrintData`: memproyeksikan `patientDependencyLevel` dan `patientDependencyLevelName`.
- Memperbarui `buildAdmissionRequestForm`: memetakan `patientDependencyLevel` dari detail ke state form sebagai string numerik (`"1"`, `"2"`, `"3"`) atau string kosong.
- Memperbarui `buildAdmissionRequestSelections`: mencari opsi berlabel dari `ADMISSION_REQUEST_DEPENDENCY_LEVELS` untuk mengisi kontrol `BaseSelectField`.
- Memperbarui `buildAdmissionRequestUpdatePayload`: memparsing `patientDependencyLevel` menjadi integer bernilai $> 0$ atau `null`.
- Memperbarui `isAdmissionRequestReadyForSignature`: mewajibkan `toText(form?.patientDependencyLevel)` sebelum формулир dianggap lengkap.

### C. Hook State: `use-emergency-admission-request.jsx`
- Menambahkan `ADMISSION_REQUEST_FIELDS.dependencyLevel.name` ke dalam array konstanta `SELECTION_FIELDS`, sehingga ketika perawat memilih opsi pada `BaseSelectField`, state `selections.patientDependencyLevel` diperbarui secara otomatis dan reaktif.

### D. Tampilan Form Permintaan Rawat Inap: `emergency-assessment-admission-request-form.jsx`
- Mengimpor `ADMISSION_REQUEST_DEPENDENCY_LEVELS`.
- Pada seksi formulir draf `<EmergencyAssessmentFormSection title="Rencana penempatan">`, menambahkan komponen:
  ```jsx
  <BaseSelectField
    field={ADMISSION_REQUEST_FIELDS.dependencyLevel}
    value={form.patientDependencyLevel}
    selectedOption={selections.patientDependencyLevel}
    options={ADMISSION_REQUEST_DEPENDENCY_LEVELS}
    disabled={!editable}
    searchPlaceholder="Pilih tingkat ketergantungan..."
    emptyText="Pilihan tidak ditemukan"
    onChange={setField}
  />
  ```
- Pada seksi *Isian form* dalam mode read-only (sesudah diminta tanda tangan atau dibekukan), menambahkan baris data:
  ```jsx
  <ReadOnlyItem label="Tingkat ketergantungan">{detail.patientDependencyLevelName || "-"}</ReadOnlyItem>
  ```

### E. Pratinjau Dokumen Cetak Fisik: `emergency-assessment-admission-request-print.jsx`
- Pada seksi `<PrintSection title={ADMISSION_PRINT_TEXT.sectionPlacement}>`, menambahkan baris cetak resmi:
  ```jsx
  <PrintRow label={ADMISSION_PRINT_TEXT.dependencyLevelLabel}>
    {content.patientDependencyLevelName || "-"}
  </PrintRow>
  ```

### F. Dialog Tanda Tangan DPJP IGD: `emergency-admission-signature-dialog.jsx`
- Pada komponen `SignatureSummary`, menambahkan baris verifikasi tingkat ketergantungan agar DPJP IGD dapat membaca klasifikasi ini sebelum menandatangani persetujuan rawat inap.

### G. Kebijakan Nol Gangguan & Nol Komentar (*Zero Disruption & Clean Code*)
- Seluruh berkas *base components* bersama (`@/components/features/base-features/`) tetap utuh tanpa modifikasi.
- Nol baris komentar baru ditambahkan ke seluruh source code aplikasi C# maupun JavaScript/JSX.

---

## 3. Hasil Pengujian & Verifikasi

1. **Linting (ESLint)**:
   - Perintah:
     ```bash
     npx eslint src/components/view/health-services/emergency-installation-management/doctor-emergency/emergency-admission-signature-dialog.jsx \
                src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx \
                src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-admission-request-print.jsx \
                src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx \
                src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-admission-request.jsx \
                src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js \
                tests/unit/emergency-admission-request-dependency-level.test.mjs
     ```
   - Hasil: **0 Error, 0 Warning** (Clean).
2. **Unit Testing Otomatis**:
   - Berkas: `tests/unit/emergency-admission-request-dependency-level.test.mjs`
   - Perintah: `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-admission-request-dependency-level.test.mjs`
   - Hasil: **6 dari 6 Subtest LULUS (100% PASS)**:
     - `ADMISSION_REQUEST_DEPENDENCY_LEVELS` memuat Low, Moderate, dan High Dependency.
     - `normalizeAdmissionRequestDetail` menormalisasi nilai numerik dan label nama tingkat ketergantungan.
     - `buildAdmissionRequestForm` dan `buildAdmissionRequestSelections` memetakan form dan objek dropdown dengan tepat.
     - `buildAdmissionRequestUpdatePayload` memvalidasi dan memformat payload PUT ke backend.
     - `isAdmissionRequestReadyForSignature` menolak validasi jika tingkat ketergantungan kosong dan meloloskan jika terisi.
     - `normalizeAdmissionPrintData` menyertakan data tingkat ketergantungan ke pratinjau cetak.
3. **Penyelarasan Backend (`BE-IGD-079`)**:
   - Backend ASP.NET Core port 7184 telah siap dengan kolom database `PatientDependencyLevel` (Migration `20261016064300_AddPatientDependencyLevelToAdmissionRequest`) dan DTO respons telah menyertakan enum serta nama terjemahan.
