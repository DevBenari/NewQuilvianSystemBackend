# Laporan Isu: Kegagalan Payload Kontrak DTO Pencatatan Tanda Vital pada Pengawasan Harian Pasien

**ID Isu:** `ISSUE-KEP-006`  
**Modul:** Rawat Inap — Keperawatan (`Inpatient Nursing Workspace`)  
**Submodul:** Pengawasan Harian Pasien (`use-daily-monitoring.js` & `daily-monitoring-section.jsx`)  
**Tingkat Keparahan:** 🔴 **Tinggi / Cacat Integrasi Kontrak API (API Data Contract Mismatch & Missing Required Fields)**  
**Tanggal Temuan:** 23 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Antigravity (Playwright End-to-End Live Testing)  
**Status Isu:** 🟢 **TERATASI (Resolved & Verified)**  

---

## 1. Ringkasan Masalah

Pada saat perawat mengklik tombol **"+ Catat TTV"** pada layar **Pengawasan Harian Pasien (*Daily Monitoring*, `tab=daily-monitoring`)**, mengisi formulir tanda vital (Tekanan Darah, Nadi, Laju Napas, Suhu Tubuh, Saturasi Oksigen), lalu menekan tombol **"Simpan Tanda Vital"**, permintaan gagal dengan status **HTTP 400 Bad Request**:

```json
{
  "success": false,
  "statusCode": 400,
  "message": "PatientId wajib diisi.",
  "data": null
}
```

Kegagalan ini memblokir perawat rawat inap dari mendokumentasikan tanda-tanda vital harian pasien secara langsung dari lembar kerja pengawasan harian.

---

## 2. Akar Masalah Teknis (*Root Cause Analysis*)

Pemeriksaan mendalam terhadap kode sumber mengungkap adanya ketidaksesuaian kontrak DTO antara antarmuka frontend dengan endpoint backend:

### A. Endpoint Target dan Kontrak DTO Backend
Endpoint pembuatan tanda vital adalah:
- **Metode & Rute:** `POST /api/v1/health-services/clinical-management/patient-vital-signs`
- **Controller:** [`PatientVitalSignController.CreateVitalSign`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs#L457)
- **DTO Request:** [`CreatePatientVitalSignRequest`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/DTOs/PatientVitalSignDtos.cs#L250)

Kontrak DTO backend mensyaratkan:
1. `[Required] PatientId (Guid)`: Identitas pasien di `MstPatient`. Wajib diisi.
2. `EncounterId (Guid?)`: Kunjungan pasien yang kemudian dipakai oleh `InpatientVitalSignService.FindInpatientEpisodeIdAsync` untuk mengaitkan episode rawat inap berjalan (`InpEpisodeId`).
3. Nama-nama properti numerik tanda vital:
   - `BloodPressureSystolic (int?)`
   - `BloodPressureDiastolic (int?)`
   - `PulseRate (int?)`
   - `RespiratoryRate (int?)`
   - `Temperature (decimal?)`
   - `OxygenSaturation (decimal?)`
4. Aturan Keselamatan Klinis (`VAL-KEP-22c` / `INV-KEP-04`):
   - `HasPain = false`, `PainScale = null`, `PainLocation = null`, `PainNote = null` (isian nyeri dilarang pada tanda vital rawat inap, wajib lewat tab Monitoring Nyeri).

### B. Implementasi Awal Hook Frontend
Pada [`use-daily-monitoring.js`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-daily-monitoring.js):
1. Parameter inisialisasi hook hanya mendestrukturisasi `{ episodeId }`, tanpa menerima `patientId` dan `encounterId`.
2. Fungsi `recordVitalSign` mengirimkan objek payload dengan struktur:
   ```javascript
   // SEBELUM PERBAIKAN:
   const payload = {
     inpEpisodeId: episodeId,       // Ditolak / diabaikan backend DTO
     encounterId: formData.encounterId || null, // Selalu null
     systolic: formData.systolic ? Number(formData.systolic) : null, // Mismatch nama properti
     diastolic: formData.diastolic ? Number(formData.diastolic) : null,
     heartRate: formData.heartRate ? Number(formData.heartRate) : null,
     respiratoryRate: formData.respiratoryRate ? Number(formData.respiratoryRate) : null,
     bodyTemperature: formData.bodyTemperature ? Number(formData.bodyTemperature) : null,
     oxygenSaturation: formData.oxygenSaturation ? Number(formData.oxygenSaturation) : null,
     observationDateTime: formData.observationDateTime || new Date().toISOString(),
   };
   // Ketiadaan PatientId memicu Http 400 "PatientId wajib diisi."
   ```

---

## 3. Langkah Perbaikan Terpasang (*Remediation*)

Perbaikan dilakukan secara terpadu pada dua berkas frontend:

### A. Pengkabelan Konteks Ruang Rawat pada Section (`daily-monitoring-section.jsx`)
Menyediakan `patientId` dan `encounterId` dari objek konteks episode rawat inap yang aktif ke `useDailyMonitoring`:

```javascript
// src/components/view/health-services/inpatient-management/nursing-workspace/sections/daily-monitoring/daily-monitoring-section.jsx
const {
  selectedDate,
  summary,
  // ...
  recordVitalSign,
} = useDailyMonitoring({
  episodeId,
  patientId: workspace?.episode?.patientId,
  encounterId: workspace?.episode?.encounterId,
});
```

### B. Penyelarasan DTO pada Hook (`use-daily-monitoring.js`)
Menerima `patientId` dan `encounterId` serta memetakan seluruh properti formulir ke format `CreatePatientVitalSignRequest`:

```javascript
// src/lib/hooks/health-services/inpatient-management/use-daily-monitoring.js
export function useDailyMonitoring({
  episodeId,
  patientId = null,
  encounterId = null,
}) {
  // ...
  const recordVitalSign = useCallback(
    async (formData) => {
      if (!episodeId) throw new Error("Episode ID tidak tersedia.");
      const resolvedPatientId = formData.patientId || patientId;
      if (!resolvedPatientId) {
        throw new Error("Patient ID tidak ditemukan. Tanda vital memerlukan identitas pasien.");
      }
      setSubmitting(true);
      setActionFeedback(null);
      try {
        const payload = {
          patientId: resolvedPatientId,
          encounterId: formData.encounterId || encounterId || null,
          observationDateTime: formData.observationDateTime || new Date().toISOString(),
          bloodPressureSystolic:
            formData.systolic !== undefined && formData.systolic !== null && formData.systolic !== ""
              ? Number(formData.systolic)
              : null,
          bloodPressureDiastolic:
            formData.diastolic !== undefined && formData.diastolic !== null && formData.diastolic !== ""
              ? Number(formData.diastolic)
              : null,
          pulseRate:
            formData.heartRate !== undefined && formData.heartRate !== null && formData.heartRate !== ""
              ? Number(formData.heartRate)
              : null,
          respiratoryRate:
            formData.respiratoryRate !== undefined && formData.respiratoryRate !== null && formData.respiratoryRate !== ""
              ? Number(formData.respiratoryRate)
              : null,
          temperature:
            formData.bodyTemperature !== undefined && formData.bodyTemperature !== null && formData.bodyTemperature !== ""
              ? Number(formData.bodyTemperature)
              : null,
          oxygenSaturation:
            formData.oxygenSaturation !== undefined && formData.oxygenSaturation !== null && formData.oxygenSaturation !== ""
              ? Number(formData.oxygenSaturation)
              : null,
          hasPain: false,
          painScale: null,
          painLocation: null,
          painNote: null,
        };
        const created = await createPatientVitalSign(payload);
        setActionFeedback({
          type: "success",
          message: "Tanda vital berhasil dicatat.",
        });
        await loadSummary(selectedDate);
        return created;
      } catch (err) {
        const errorMsg =
          err.response?.data?.message || err.message || "Gagal mencatat tanda vital.";
        setActionFeedback({ type: "error", message: errorMsg });
        throw err;
      } finally {
        setSubmitting(false);
      }
    },
    [episodeId, patientId, encounterId, selectedDate, loadSummary]
  );
```

---

## 4. Hasil Verifikasi Lapangan (*Evidence of Resolution*)

Pengujian live end-to-end dengan Playwright (`test-pengawasan-harian-full-cycle.mjs`) mengonfirmasi bahwa:
1. Permintaan `POST /api/v1/health-services/clinical-management/patient-vital-signs` berhasil dieksekusi dengan status **HTTP 200 OK**.
2. Respons ringkasan harian `GET /api/v1/health-services/clinical-management/daily-monitoring/episodes/{episodeId}/summary?date=2026-09-23` segera tersegarkan secara reaktif.
3. Nilai tanda vital (TD 120/80 mmHg, Nadi 82x/m, RR 18x/m, Suhu 36.6°C, SpO2 98%) tampil langsung pada kartu pemantauan harian pasien di UI.
4. Tangkapan layar bukti tersimpan di `test-with-agy/screenshots/pengawasan-harian-testing/03-ttv-recorded.png`.
