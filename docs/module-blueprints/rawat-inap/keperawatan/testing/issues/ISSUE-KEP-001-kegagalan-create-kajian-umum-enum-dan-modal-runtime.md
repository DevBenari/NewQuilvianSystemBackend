# Laporan Isu: Kegagalan Pembuatan (Create) & Finalisasi Kajian Umum Keperawatan Rawat Inap

**ID Isu:** `ISSUE-KEP-001`  
**Modul:** Rawat Inap — Keperawatan (`Inpatient Nursing Workspace`)  
**Submodul:** Pengkajian Pasien (`Assessment Section`) — Tab Kajian Umum (`activeTab="general"`)  
**Tingkat Keparahan:** 🔴 **Tinggi / Critical Blocker** (Menghentikan alur pencatatan rekam medis rawat inap)  
**Tanggal Temuan:** 23 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Live In-Browser Antigravity (Playwright)  
**Status Isu:** Open / Menunggu Perbaikan Kode  

---

## 1. Ringkasan Masalah

Pada saat perawat melakukan pencatatan **Kajian Umum Keperawatan** untuk pasien rawat inap baru (skenario pasien Tn. Indra Gunawan, No. RM `00-00-00-16`, Episode `c3fe1370-18f0-42fb-8d9f-01449212828e`), sistem mengalami dua kegagalan fatal:

1. **Aksi "Simpan Konsep" (Draft) Gagal:** Permintaan penyimpanan ditolak oleh server dengan status **`HTTP 400 Bad Request`**. Penyebabnya adalah data tingkat kesadaran (`consciousnessStatus`) dan beberapa status klinis lainnya terkirim bernilai `null` ke backend akibat kesalahan konversi tipe data teks menjadi numerik pada frontend.
2. **Aksi "Selesaikan Pengkajian" (Complete) Gagal:** Ketika tombol *Selesaikan Pengkajian* ditekan, modal konfirmasi penyelesaian tidak terbuka sama sekali karena terjadi galat pemrograman JavaScript (**`ReferenceError: setModalErrorMessage is not defined`**) pada kode antarmuka.

---

## 2. Dampak Klinis dan Proses Bisnis Rumah Sakit

* **Kendala Pelayanan Pasien:** Perawat ruangan tidak dapat mendokumentasikan hasil pengkajian awal pasien masuk ke dalam rekam medis elektronik. Hal ini memaksa perawat mengulang pencatatan atau mencatat manual di kertas.
* **Risiko Akreditasi Rumah Sakit (KARS / JCI):** Standar pelayanan rumah sakit mewajibkan pengkajian awal keperawatan diselesaikan maksimal 24 jam sejak pasien masuk ruang perawatan. Kegagalan fungsi simpan ini berisiko menyebabkan keterlambatan pencatatan (*overdue assessment*).
* **Kerapuhan Alur Kerja Selanjutnya:** Rencana asuhan keperawatan (SDKI/SLKI/SIKI) dan pemantauan tanda vital tidak dapat ditautkan secara akurat jika dokumen kajian awal gagal tersimpan di basis data.

---

## 3. Rincian Teknis Temuan Masalah

### Temuan 1: Eror Konversi Nilai Enum pada Penyimpanan Draf (`HTTP 400 Bad Request`)

#### Lokasi Kode Sumber:
* **Frontend:** [`src/lib/hooks/health-services/inpatient-management/use-clinical-instrument-form.js` (Baris 272–284)](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-clinical-instrument-form.js#L272-L284)
* **Backend:** [`Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` (Baris 346–354)](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs#L346-L354)

#### Akar Masalah (Root Cause):
1. Pada instrumen formulir dinamis Kajian Umum, opsi jawaban radio button memiliki kode berupa teks (*string*), misalnya:
   * `KU_CONSCIOUSNESS`: `"ComposMentis"`
   * `KU_OXYGEN_TYPE`: `"NasalCannula"`
   * `NUT_APPETITE`: `"Normal"`
   * `NUT_RISK`: `"NoRisk"`
   * `FUNC_STATUS`: `"Independent"`
2. Pada saat fungsi `buildSavePayload` dijalankan, nilai kolom terikat (*bound columns*) dipaksa menjadi angka dengan perintah:
   ```javascript
   consciousnessStatus: boundColumns.ConsciousnessStatus ? Number(boundColumns.ConsciousnessStatus) : ...
   ```
3. Di JavaScript, fungsi `Number("ComposMentis")` menghasilkan nilai bukan angka (**`NaN`**).
4. Saat objek JavaScript diubah menjadi teks JSON (`JSON.stringify`), standar JSON mengubah nilai `NaN` menjadi `null`:
   ```json
   {
     "consciousnessStatus": null,
     "oxygenSupportType": null,
     "appetiteStatus": null,
     "nutritionRiskStatus": null,
     "functionalStatus": null
   }
   ```
5. Pada backend ASP.NET Core, kelas DTO `CreatePatientAssessmentRequest` mendefinisikan properti enum tersebut sebagai tipe data non-nullable:
   ```csharp
   public ConsciousnessStatus ConsciousnessStatus { get; set; } = ConsciousnessStatus.Unknown;
   ```
6. Serializer .NET menolak nilai `null` untuk enum non-nullable dan membatalkan pemrosesan request dengan pesan eror:
   ```text
   The JSON value could not be converted to QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums.ConsciousnessStatus. Path: $.consciousnessStatus
   ```

---

### Temuan 2: Eror Runtime Pemanggilan Fungsi State Modal Selesai (`ReferenceError`)

#### Lokasi Kode Sumber:
* **Frontend:** [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx` (Baris 300–313)](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx#L300-L313)

#### Akar Masalah (Root Cause):
Pada fungsi `handleOpenCompleteModal` terdapat baris kode berikut:
```javascript
const handleOpenCompleteModal = () => {
  setFeedback(null);
  setModalErrorMessage(null); // <--- KESALAHAN: Fungsi ini tidak pernah didefinisikan!
  ...
  setShowCompleteModal(true);
};
```
Sementara itu, pada deklarasi state komponen di baris 83:
```javascript
const [modalError, setModalError] = useState(null);
```
Fungsi setter yang dideklarasikan adalah `setModalError`, bukan `setModalErrorMessage`. Akibatnya, saat perawat menekan tombol **"Selesaikan Pengkajian"**, peramban web melempar kesalahan `ReferenceError` dan menghentikan alur pembukaan modal konfirmasi.

---

## 4. Langkah-Langkah Reproduksi Masalah

1. Masuk ke aplikasi web Quilvian sebagai pengguna perawat atau superadmin (`superadmin@admin.com`).
2. Buka menu rawat inap pasien:
   `http://localhost:3000/health-services/inpatient-management/episodes/c3fe1370-18f0-42fb-8d9f-01449212828e/nursing?section=assessment&tab=general`
3. Pilih opsi **Pengkajian Awal**.
4. Lengkapi isian formulir:
   * Isi catatan keluhan utama pada textarea.
   * Pilih tingkat kesadaran **Compos Mentis**.
   * Pilih status nutrisi **Normal**, risiko malnutrisi **Tidak Berisiko**, dan fungsional **Mandiri**.
5. Klik tombol **"Simpan Konsep"** di bagian bawah formulir:
   * **Hasil:** Permintaan `POST` menghasilkan respons `HTTP 400 Bad Request`.
6. Klik tombol **"Selesaikan Pengkajian"**:
   * **Hasil:** Tidak ada respons pada layar, konsol peramban menampilkan `Uncaught ReferenceError: setModalErrorMessage is not defined`.

---

## 5. Bukti Data Jaringan & Log Kesalahan

### Payload Permintaan yang Terkirim (`POST` Request)
```json
{
  "encounterId": "d0f70f24-5232-43f1-aee4-256308b2bf95",
  "inpEpisodeId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
  "assessmentType": 0,
  "instrumentResponses": [
    {
      "instrumentVersionId": "c1a1f000-0107-4a01-9b02-000000000004",
      "responses": {
        "KU_CHIEF_COMPLAINT": "Pasien dalam observasi klinis...",
        "KU_CONSCIOUSNESS": "ComposMentis",
        "KU_OXYGEN": true,
        "KU_OXYGEN_TYPE": "NasalCannula",
        "NUT_APPETITE": "Normal",
        "NUT_RISK": "NoRisk",
        "FUNC_STATUS": "Independent"
      }
    }
  ],
  "vitalSignId": null,
  "painAssessmentState": 0,
  "completeImmediately": false,
  "chiefComplaint": "Pasien dalam observasi klinis...",
  "consciousnessStatus": null,
  "isUsingOxygen": true,
  "oxygenSupportType": null,
  "oxygenFlowRate": null,
  "appetiteStatus": null,
  "nutritionRiskStatus": null,
  "functionalStatus": null
}
```

### Respons Eror dari Server (`HTTP 400 Bad Request`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "request": [
      "The request field is required."
    ],
    "$.consciousnessStatus": [
      "The JSON value could not be converted to QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums.ConsciousnessStatus. Path: $.consciousnessStatus | LineNumber: 0 | BytePositionInLine: 2311."
    ]
  },
  "traceId": "00-3075c1aedaba9da1497e8755f3b3ec48-01ab541a2daec1e2-00"
}
```

---

## 6. Spesifikasi Kontrak API

### `[Tags("Health Services / Clinical Management / Patient Assessment")]`

| Properti | Nilai |
| :--- | :--- |
| **Metode** | `POST` |
| **Path** | `/api/v1/health-services/clinical-management/patient-assessments` |
| **Deskripsi** | Pembuatan dokumen pengkajian klinis pasien baru |
| **Otorisasi** | Bearer Token (JWT) — `PatientAssessment.Create` |
| **Status Sukses** | `201 Created` |
| **Status Gagal** | `400 Bad Request` (Validasi isian gagal), `403 Forbidden` (Bukan perawat/pegawai aktif), `409 Conflict` (Pengkajian awal ganda) |

---

## 7. Rencana Solusi & Tindakan Perbaikan

Untuk menyelesaikan isu ini, diperlukan 2 langkah perbaikan pada kode frontend:

### Perbaikan 1: Pemetaan Enum yang Aman pada `use-clinical-instrument-form.js`
Gunakan kamus konstanta untuk memetakan nilai kode teks (*string*) ke angka integer enum sebelum dimasukkan ke dalam payload:

```javascript
// Kamus pemetaan aman untuk enum backend
const CONSCIOUSNESS_MAP = {
  Unknown: 0,
  ComposMentis: 1,
  Apatis: 2,
  Somnolen: 3,
  Sopor: 4,
  Coma: 5,
};

const OXYGEN_TYPE_MAP = {
  None: 0,
  NasalCannula: 1,
  SimpleMask: 2,
  NonRebreathingMask: 3,
  VenturiMask: 4,
  Other: 99,
};

const APPETITE_MAP = {
  Unknown: 0,
  Normal: 1,
  Decreased: 2,
  Increased: 3,
  Poor: 4,
};

const NUTRITION_RISK_MAP = {
  Unknown: 0,
  NoRisk: 1,
  LowRisk: 2,
  MediumRisk: 3,
  HighRisk: 4,
};

const FUNCTIONAL_MAP = {
  Unknown: 0,
  Independent: 1,
  NeedPartialAssistance: 2,
  FullyDependent: 3,
};

const resolveEnumValue = (value, map, fallback = 0) => {
  if (value === undefined || value === null) return fallback;
  if (typeof value === "number") return value;
  const parsed = Number(value);
  if (!Number.isNaN(parsed)) return parsed;
  return map[value] !== undefined ? map[value] : fallback;
};
```

Kemudian terapkan pada fungsi `buildSavePayload`:
```javascript
consciousnessStatus: resolveEnumValue(boundColumns.ConsciousnessStatus, CONSCIOUSNESS_MAP, existingAssessment?.consciousnessStatus ?? 1),
oxygenSupportType: resolveEnumValue(boundColumns.OxygenSupportType, OXYGEN_TYPE_MAP, existingAssessment?.oxygenSupportType ?? 0),
appetiteStatus: resolveEnumValue(boundColumns.AppetiteStatus, APPETITE_MAP, existingAssessment?.appetiteStatus ?? 1),
nutritionRiskStatus: resolveEnumValue(boundColumns.NutritionRiskStatus, NUTRITION_RISK_MAP, existingAssessment?.nutritionRiskStatus ?? 0),
functionalStatus: resolveEnumValue(boundColumns.FunctionalStatus, FUNCTIONAL_MAP, existingAssessment?.functionalStatus ?? 1),
```

### Perbaikan 2: Perbaikan Nama Setter State pada `assessment-section.jsx`
Ubah baris 302 pada file `assessment-section.jsx`:
```diff
  const handleOpenCompleteModal = () => {
    setFeedback(null);
-   setModalErrorMessage(null);
+   setModalError(null);

    const validation = validateForComplete();
```
