# ISSUE-RWI-TINDAKAN-001 — Tampilan Tarif Rp 0 pada Katalog Formulir Pemesanan Tindakan Medis Rawat Inap

```yaml
issue_id: ISSUE-RWI-TINDAKAN-001
module_id: rawat-inap
submodule: tindakan-rawat-inap (dokter-rawat-inap & keperawatan-rawat-inap)
blueprint_id: RWI-BP-001
contract_version: 0.6.0
tanggal_issue: "2026-10-02"
pelapor: Tim Engineering / QA Quilvian
lingkungan_terdampak:
  - QuilvianNewDevMaster (Database Master)
  - QuilvianNewDevHamzah (Database Pengembangan Hamzah)
  - QuilvianNewDevSukma (Database Pengembangan Sukma)
status: TERIDENTIFIKASI / MENUNGGU PERBAIKAN
tingkat_keparahan: SEDANG (Major UI Anomaly — Data billing riil di backend aman)
komponen_terkait:
  backend_controller: Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs
  backend_dto: Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs
  backend_service: Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs
  frontend_hook: src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx
  frontend_view: src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx
```

---

## 1. Ringkasan Eksekutif

Pada pengujian antarmuka pengguna (**Frontend**) formulir pemesanan tindakan medis rawat inap (*Physician Workspace* & *Nursing Workspace*), seluruh item pada tabel **Daftar Tindakan Medis** (kolom kiri) menampilkan nominal tarif **`Rp 0`** dengan label kelas pasien di bawahnya (misalnya **`SUITE`**), serta badge status penjaminan **`Ditanggung`**.

Kondisi ini menimbulkan kebingungan bagi staf medis (dokter dan perawat) karena tindakan medis terkesan tidak dikenakan biaya (gratis). 

Berdasarkan audit komprehensif terhadap database **`QuilvianNewDevMaster`** dan **`QuilvianNewDevHamzah`**, data tarif di tabel master (`MstTariff`) sebenarnya **tersedia lengkap dengan harga normal rumah sakit yang positif (> Rp 0)**. Anomali tampilan `Rp 0` ini terjadi karena **endpoint backend katalog tidak menyertakan data tarif**, sehingga frontend menggunakan nilai *fallback default* `Rp 0`.

---

## 2. Gejala dan Bukti Visual

### 2.1 Tangkapan Layar
Pada layar pemesanan tindakan rawat inap:
- **Header**: *Form Tindakan Medis - Rawat Inap*
- **Tabel Kiri**: *Daftar Tindakan Medis (100 tindakan tersedia)*
- **Kolom Tarif**:
  ```text
  Angkat Jahitan (Aff Hecting)           -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  Defibrilasi / Kardioversi Darurat      -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  Ekstraksi Kuku (Rosser Plasty)         -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  Injeksi Intramuskular (IM Injection)   -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  Injeksi Intravena (IV Bolus / Push)    -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  Injeksi Subkutan (SC / Insulin)        -> Tarif: Rp 0 | SUITE | Status: Ditanggung
  ```

---

## 3. Analisis Akar Masalah (Root Cause Analysis)

Investigasi penelusuran dari database, lapisan logika backend, hingga rendering frontend menunjukkan tiga akar masalah yang saling terkait:

### 3.1 Akar Masalah 1: Endpoint Backend `GET /master-options` Tidak Mengambil Tarif

Endpoint yang dipanggil oleh antarmuka katalog tindakan adalah:
`GET /api/v1/health-services/clinical-management/patient-procedures/master-options?take=100`

Implementasi pada [`PatientProcedureController.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#L167-L178):

```csharp
var data = await query
    .OrderBy(x => x.SortOrder)
    .ThenBy(x => x.ProcedureName)
    .Take(take)
    .Select(x => new PatientProcedureMasterOptionResponse
    {
        Id = x.Id,
        ProcedureCode = x.ProcedureCode,
        ProcedureName = x.ProcedureName,
        ProcedureGroupName = x.ProcedureGroupName,
        ProcedureCategoryName = x.ProcedureCategoryName,
        ProcedureType = x.ProcedureType,
        IsNeedApproval = x.IsNeedApproval,
        IsSurgery = x.IsSurgery,
        EstimatedDurationMinutes = x.EstimatedDurationMinutes
        // ⚠️ ANOMALI: Tidak ada join ke MstTariff dan tidak ada field Tariff / TotalPrice
    })
    .ToListAsync();
```

DTO [`PatientProcedureMasterOptionResponse`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs#L155-L166) hanya memuat definisi klinis prosedur:

```csharp
public class PatientProcedureMasterOptionResponse
{
    public Guid Id { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public string? ProcedureGroupName { get; set; }
    public string? ProcedureCategoryName { get; set; }
    public string ProcedureType { get; set; } = string.Empty;
    public bool IsNeedApproval { get; set; }
    public bool IsSurgery { get; set; }
    public int EstimatedDurationMinutes { get; set; }
}
```

Endpoint ini tidak pernah membaca tabel `MstTariff`. Akibatnya, payload JSON respons API tidak memiliki atribut tarif sama sekali:
```json
{
  "id": "1051a622-67fb-4c34-b2d5-0891b18c3956",
  "procedureCode": "PR-RSMMC-00024",
  "procedureName": "Penjahitan Luka Sedang (Hecting 6-10 Jahitan)",
  "procedureGroupName": "Bedah Minor",
  "procedureCategoryName": "Tindakan Bedah Minor",
  "procedureType": "DoctorAction",
  "isNeedApproval": false,
  "isSurgery": true,
  "estimatedDurationMinutes": 45
}
```

---

### 3.2 Akar Masalah 2: Penanganan Fallback Tampilan di Frontend (`ProcedureFormPanel.jsx`)

Pada hook frontend [`use-inpatient-procedure-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx#L114-L123):
```javascript
const mapped = items.map((opt) => ({
  value: opt.id || opt.Id,
  label: `${opt.procedureCode ? `[${opt.procedureCode}] ` : ""}${opt.procedureName || opt.name || ""}`,
  procedureCode: opt.procedureCode,
  procedureName: opt.procedureName,
  category: opt.procedureCategoryName,
  group: opt.procedureGroupName,
  tariff: opt.totalPrice || opt.tariff || null,       // Nilai selalu null
  coverageStatus: opt.coverageStatus || null,         // Nilai selalu null
}));
```

Pada komponen tampilan [`procedure-form-panel.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx#L261-L271):
```jsx
<td>
  <div>{formatCurrency(item.tariff || item.totalPrice)}</div>
  <small style={{ color: "#00acc1", fontWeight: 600 }}>
    {episode?.className || episode?.patientClassName || "Tarif"}
  </small>
</td>
<td>
  <Badge bg="success" pill style={{ fontSize: "0.75rem" }}>
    {item.coverageStatus || "Ditanggung"}
  </Badge>
</td>
```

1. Karena `item.tariff` bernilai `undefined`, fungsi `formatCurrency(undefined)` menghasilkan teks **`"Rp 0"`**.
2. Label di bawah harga membaca nama kelas perawatan pasien yang sedang aktif di workspace (`episode?.className`), yang pada episode pasien uji bernilai **`"SUITE"`**.
3. Status penjaminan menggunakan fallback `item.coverageStatus || "Ditanggung"` sehingga muncul badge hijau **`Ditanggung`**.

---

### 3.3 Akar Masalah 3: Struktur Data `MstTariff` pada Database Master & Hamzah

Pemeriksaan silang pada database `QuilvianNewDevMaster` dan `QuilvianNewDevHamzah` membuktikan bahwa data tarif di database **sebenarnya bernilai positif dan tidak nol**:

```sql
SELECT p."ProcedureCode", p."ProcedureName", t."TariffCode", t."NormalPrice", t."PatientClassId"
FROM "MstProcedure" p
JOIN "MstTariff" t ON t."ProcedureId" = p."Id"
WHERE t."TariffCategoryId" = '416d7c40-0922-4ac0-866e-ad932b2fb7d0' AND t."IsDelete" = false;
```

**Bukti Data Riil di Database:**
- `Defibrilasi / Kardioversi Darurat` : **Rp 400.000**
- `Penjahitan Luka Sedang (Hecting 6-10)` : **Rp 275.000**
- `Ekstraksi Kuku (Rosser Plasty)` : **Rp 250.000**
- `Rawat Luka Kompleks / Debridement` : **Rp 250.000**
- `Pungsi Lumbal Diagnostik` : **Rp 500.000**
- `Pemasangan Infus Dewasa` : **Rp 75.000**
- `Nebulisasi Dewasa` : **Rp 150.000**
- `Angkat Jahitan (Aff Hecting)` : **Rp 55.000**

**Karakteristik Khusus Kategori Tindakan di Master & Hamzah:**
Seluruh 32 tarif tindakan memiliki nilai `PatientClassId = NULL` (Tarif Universal / Berlaku untuk Semua Kelas). 

Sebagai perbandingan, kategori master lain di database Master memiliki matriks tarif khusus per kelas:
- **Obat (Drug)**: 56.411 baris (100% memiliki `PatientClassId`)
- **Laboratorium**: 23.180 baris (100% memiliki `PatientClassId`)
- **Kamar Rawat**: 25.325 baris (100% memiliki `PatientClassId`)
- **Tindakan (Procedure)**: 32 baris (**100% `PatientClassId = NULL` / Universal**)

---

## 4. Dampak Bisnis dan Integritas Transaksi

| Aspek | Status Dampak | Keterangan |
| :--- | :---: | :--- |
| **Pengalaman Pengguna (UX Dokter & Perawat)** | **Terganggu** | Dokter dan perawat mengira tindakan medis tidak memiliki tarif atau sistem billing rusak. |
| **Integritas Transaksi Simpan Tindakan** | **AMAN** | Saat pesanan dibuat via `POST /inpatient-orders`, `PatientProcedureOrderService` memanggil `InsuranceCoverageService.ResolveProcedureAsync`. Layanan ini membaca `MstTariff` riil, sehingga catatan transaksi billing yang tersimpan tetap memiliki harga riil (bukan nol). |
| **Klaim Asuransi** | **AMAN** | Di database Master & Hamzah, seluruh 160 aturan penjaminan asuransi (`MstInsuranceCoverageRule`) terhubung aktif dengan 5 penjamin utama (BPJS, Mandiri Inhealth, Prudential, Allianz, Sinarmas) dengan cakupan 100%. |

---

## 5. Rencana Perbaikan (To-Be Solution)

### 5.1 Perbaikan Backend: Penyesuaian Endpoint `master-options`

Perbarui endpoint `GET /api/v1/health-services/clinical-management/patient-procedures/master-options` agar:
1. Menerima query parameter opsional: `patientClassId` (Guid?) dan `serviceUnitId` (Guid?).
2. Memperkaya DTO `PatientProcedureMasterOptionResponse` dengan field:
   - `Tariff` / `NormalPrice` (`decimal`): Harga normal rumah sakit.
   - `TotalPrice` (`decimal`): Total tarif yang berlaku.
   - `CoverageStatus` (`string`): Status jaminan (misal `"Ditanggung"`).
3. Melakukan pencarian tarif dari `MstTariff` dengan aturan:
   - Cari tarif yang cocok dengan `ProcedureId`.
   - Prioritaskan tarif yang cocok dengan `patientClassId` jika ada; jika tidak ada, gunakan tarif dasar universal (`PatientClassId IS NULL`).

#### Spesifikasi Kontrak API Target (Swagger-Style)

```yaml
Tags:
  - Health Services / Clinical Management / Patient Procedure
Endpoint:
  Method: GET
  Path: /api/v1/health-services/clinical-management/patient-procedures/master-options
  Deskripsi: Mengambil katalog master tindakan klinis lengkap dengan nominal tarif berlaku dan status penjaminan
  Auth: Bearer JWT (Wewenang: PatientProcedure:Read)
  QueryParams:
    - search (string, opsional): Kata kunci nama atau kode tindakan
    - patientClassId (guid, opsional): ID kelas rawat pasien untuk resolusi tarif spesifik
    - serviceUnitId (guid, opsional): ID unit layanan rawat inap
    - take (int, default: 50, max: 100): Batas jumlah data
  Response (200 OK):
    Body: ApiResponse<List<PatientProcedureMasterOptionResponse>>
```

**Definisi DTO Target:**
```csharp
public class PatientProcedureMasterOptionResponse
{
    public Guid Id { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public string? ProcedureGroupName { get; set; }
    public string? ProcedureCategoryName { get; set; }
    public string ProcedureType { get; set; } = string.Empty;
    public bool IsNeedApproval { get; set; }
    public bool IsSurgery { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    
    // Field Baru untuk Tampilan Antarmuka:
    public decimal Tariff { get; set; }
    public decimal TotalPrice { get; set; }
    public string CoverageStatus { get; set; } = "Ditanggung";
}
```

---

### 5.2 Perbaikan Frontend: Meneruskan Konteks Pasien

Pada file [`use-inpatient-procedure-tab.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx):
1. Teruskan parameter `patientClassId: episode?.patientClassId` saat memanggil `getPatientProcedureMasterOptions`.
2. Petakan field `tariff` dan `coverageStatus` dari payload respons API backend ke dalam state pilihan tindakan.

---

### 5.3 Opsi Kebijakan Master Data Tarif Rumah Sakit

Pihak manajemen rumah sakit / tim analis bisnis dapat memilih salah satu kebijakan berikut:
- **Kebijakan A (Tarif Universal / Flat)**: Tindakan medis berbiaya sama untuk semua kelas kamar (misalnya EKG tetap Rp 80.000 baik di Kelas III maupun di SUITE). Backend membaca tarif dengan `PatientClassId IS NULL`.
- **Kebijakan B (Tarif Berjenjang per Kelas)**: Biaya tindakan medis memiliki disparitas antar kelas (misalnya SUITE dikenakan koefisien 1.5x dari Kelas I). Jika memilih kebijakan ini, seeder tarif perlu menduplikasi baris `MstTariff` per `PatientClassId`.

---

## 6. Kesimpulan & Rekomendasi Tindak Lanjut

1. **Status Isu**: Anomali ini **bukan disebabkan oleh kerusakan database**, melainkan karena endpoint katalog master opsi (`GET /master-options`) belum dilengkapi fungsi penyajian tarif.
2. **Prioritas Penanganan**: **Tinggi (P1 UI Polish)** untuk menghindari kebingungan dokter dan perawat saat menginput tindakan pasien rawat inap.
3. **Langkah Berikutnya**: Implementasi penambahan field tarif pada `PatientProcedureController.GetMasterProcedureOptions` dan pembaruan mapping di frontend `useInpatientProcedureTab`.
