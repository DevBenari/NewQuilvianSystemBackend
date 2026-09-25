# Laporan Isu: Kegagalan Penyimpanan Evaluasi Awal MPP Akibat Ketidakcocokan Resolusi ID Versi Checklist

**ID Isu:** `ISSUE-KEP-007`  
**Modul:** Rawat Inap — Keperawatan (*Inpatient Nursing Workspace*)  
**Submodul:** Evaluasi Awal Manajer Pelayanan Pasien (MPP / *Case Management Evaluation*, `tab=initial-eval`)  
**Tingkat Keparahan:** 🔴 **Tinggi / Ketidaksesuaian Properti Kontrak DTO (API Property Contract Mismatch)**  
**Tanggal Temuan:** 24 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Antigravity (*Playwright End-to-End Live Testing*)  
**Status Isu:** 🟢 **TERATASI (Resolved & Verified)**  

---

## 1. Ringkasan Masalah

Pada saat Manajer Pelayanan Pasien (MPP) atau Perawat yang bertugas membuka subtab **Evaluasi Awal (`initial-eval`)**, mengisi 8 seksi checklist evaluasi awal pasien rawat inap sesuai standar PRD 33, lalu menekan tombol **"Simpan Konsep"**, permintaan gagal dengan status **HTTP 409 Conflict**:

```json
{
  "success": false,
  "statusCode": 409,
  "message": "Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar.",
  "data": null,
  "errors": {
    "code": "INSTRUMENT_VERSION_CHANGED"
  }
}
```

Kegagalan ini memblokir staf MPP dari menyimpan formulir konsep dan melanjutkan finalisasi dokumen rekam medis evaluasi awal pasien.

---

## 2. Akar Masalah Teknis (*Root Cause Analysis*)

Pemeriksaan alur jaringan dan kode sumber menemukan perbedaan penamaan properti antara respon endpoint resolusi checklist dengan pembacaan properti di hook orkestrasi frontend:

### A. Endpoint Resolusi Checklist Backend
Endpoint yang mengembalikan checklist aktif adalah:
- **Metode & Rute:** `GET /api/v1/health-services/clinical-management/case-management-evaluations/checklist/resolve?episodeId={episodeId}`
- **Tag Swagger:** `[Tags("Health Services / Clinical Management / Case Management Evaluation")]`
- **Tipe Data Respon:** `ApiResponse<ResolvedInstrumentResponse>`

Struktur JSON respon aktual dari backend adalah:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Instrumen yang berlaku berhasil diambil.",
  "data": {
    "instrumentId": "c1a1f000-0107-4a01-9b01-000000000008",
    "instrumentCode": "CASE_MANAGEMENT_CHECKLIST",
    "instrumentName": "Checklist Evaluasi Awal MPP (draft)",
    "instrumentKind": 6,
    "versionId": "c1a1f000-0107-4a01-9b02-000000000008",
    "versionNumber": 1,
    "definition": { ... },
    "isApproved": true
  }
}
```
Perhatikan bahwa properti ID versi instrumen pada `ResolvedInstrumentResponse` bernama **`versionId`**.

### B. Pembacaan Properti pada Hook Frontend
Pada berkas [`use-case-management-evaluation.js`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-case-management-evaluation.js):
```javascript
// SEBELUM PERBAIKAN:
const versionId =
  evaluation?.checklist?.instrumentVersionId ||
  checklist?.instrumentVersionId;
```
Ketika dokumen evaluasi awal belum pernah disimpan (`evaluation` bernilai `null`), hook mencoba membaca dari `checklist?.instrumentVersionId`. Karena properti sebenarnya pada objek checklist adalah `checklist.versionId`, variabel `versionId` bernilai `undefined`, yang kemudian dikirimkan sebagai `null` dalam payload POST:

```json
{
  "episodeId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
  "clinicalDateTime": "2026-09-23T21:26:00.000Z",
  "instrumentVersionId": null,
  "responses": { ... }
}
```

### C. Validasi Backend pada `CaseManagementEvaluationService`
Pada backend [`CaseManagementEvaluationService.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/CaseManagementEvaluationService.cs#L399):
```csharp
if (!versionId.HasValue || versionId.Value != berlaku.Value!.VersionId)
    return Conflict("Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar.", "INSTRUMENT_VERSION_CHANGED");
```
Karena `instrumentVersionId` bernilai `null` (`!versionId.HasValue`), backend menolak permintaan pembuatan konsep dengan HTTP 409 `INSTRUMENT_VERSION_CHANGED`.

---

## 3. Langkah Perbaikan Terpasang (*Remediation*)

Perbaikan dilakukan pada lapisan hook frontend dan komponen presentasi:

### A. Koreksi Resolusi Versi pada `use-case-management-evaluation.js`
Menambahkan resolusi fallback ke `checklist?.versionId`:
```javascript
// SESUDAH PERBAIKAN:
const versionId =
  evaluation?.checklist?.instrumentVersionId ||
  checklist?.versionId ||
  checklist?.instrumentVersionId;
```
Dengan perubahan ini, payload pembuatan konsep baru selalu menyertakan GUID versi aktif yang sah (contoh: `c1a1f000-0107-4a01-9b02-000000000008`), sehingga lolos validasi versi backend.

### B. Perbaikan Fallback Label Versi pada `initial-evaluation-section.jsx`
Memperbaiki header kartu checklist agar menampilkan nama instrumen dinamis:
```jsx
// SESUDAH PERBAIKAN:
<span className={styles.checklistVersionLabel} data-testid="checklist-version-label">
  Instrumen: {checklist?.instrumentVersionLabel || checklist?.instrumentName || "Case Management Checklist V1 (8 Bagian)"}
</span>
```

---

## 4. Spesifikasi Endpoint Bergaya Swagger

| Atribut | Keterangan |
| :--- | :--- |
| **Grup Tag** | `[Tags("Health Services / Clinical Management / Case Management Evaluation")]` |
| **Metode HTTP** | `POST` |
| **Rute API** | `/api/v1/health-services/clinical-management/case-management-evaluations` |
| **Otorisasi** | Bearer Token / Cookie Sesi (`CaseManagementEvaluation : Create`) |
| **Kontrak Request Body** | `CreateCaseManagementEvaluationRequest` |
| **Respon Sukses** | `HTTP 201 Created` — `ApiResponse<CaseManagementEvaluationResponse>` |
| **Contoh Skenario** | Pasien Tn. Indra Gunawan (RM: `00-00-00-16`, DHF Grade II) dievaluasi oleh MPP Ns. Mira Safitri dengan pengisian 8 bagian perencanaan asuhan terpadu. |

---

## 5. Bukti Verifikasi Pasca Perbaikan

1. **Pengujian Simpan Konsep**: Payload POST kini mengirimkan `instrumentVersionId: "c1a1f000-0107-4a01-9b02-000000000008"`.
2. **Respon Backend**: Berhasil menerima `HTTP 201 Created` dengan penerbitan nomor dokumen Evaluasi Awal otomatis `#MPP-YYYYMMDD-XXXX`.
3. **Status Isu**: Dinyatakan **🟢 TERATASI (Resolved & Verified)**.
