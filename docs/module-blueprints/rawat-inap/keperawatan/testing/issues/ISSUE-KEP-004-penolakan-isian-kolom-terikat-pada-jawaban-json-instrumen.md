# Laporan Isu: Penolakan Isian Kolom Terikat (Bound Column) pada Jawaban JSON Instrumen Pengkajian

**ID Isu:** `ISSUE-KEP-004`  
**Modul:** Rawat Inap — Keperawatan (`Inpatient Nursing Workspace`)  
**Submodul:** Pengkajian Pasien (`Assessment Section`) — Tab Monitoring Nyeri (`activeTab="pain"`), Kajian Umum (`activeTab="general"`), dan Assesment Edukasi (`activeTab="education"`)  
**Tingkat Keparahan:** 🔴 **Tinggi / Integrasi & Contract Invariant Blocker** (Menyebabkan penolakan HTTP 400 Bad Request saat perawat menyimpan konsep atau menyelesaikan pengkajian)  
**Tanggal Temuan:** 23 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Live In-Browser Antigravity (Playwright)  
**Status Isu:** ✅ **RESOLVED / SELESAI DIPERBAIKI** (Pemisahan kolom terikat dan respons JSON diimplementasikan secara otomatis pada hook formulir instrumen klinis)  

---

## 1. Ringkasan Masalah

Pada saat perawat melakukan penyimpanan konsep (*Simpan Konsep*) pada formulir **Monitoring Nyeri (*Pain Scale*)**, server backend ASP.NET Core menolak permintaan dengan status **`HTTP 400 Bad Request`** dan pesan galat spesifik:

```text
"Pita instrumen tidak dapat dihitung dari jawaban: 
Isian PAIN_SCALE disimpan pada kolom PainScale, bukan pada jawaban JSON. 
Isian PAIN_LOCATION disimpan pada kolom PainLocation, bukan pada jawaban JSON. 
Isian PAIN_QUALITY disimpan pada kolom PainQuality, bukan pada jawaban JSON. 
Isian PAIN_TRIGGER disimpan pada kolom PainTrigger, bukan pada jawaban JSON. 
Isian PAIN_INTERVENTION disimpan pada kolom PainManagement, bukan pada jawaban JSON. 
Isian PAIN_NOTE disimpan pada kolom PainNote, bukan pada jawaban JSON."
```

Hal yang sama sebelumnya juga tercatat pada **Kajian Umum (*General Nursing Assessment*)**, di mana parameter seperti `ChiefComplaint`, `CurrentIllnessHistory`, `MedicationHistory`, `AllergyNote`, dan `FunctionalNote` ditolak jika dikirimkan di dalam dictionary JSON jawaban instrumen.

---

## 2. Dampak Klinis dan Bisnis Rumah Sakit

1. **Kegagalan Pencatatan Evaluasi Nyeri:**  
   Perawat tidak dapat menyimpan draf maupun memfinalisasi evaluasi derajat nyeri pasien rawat inap pasca-tindakan medis atau operasi.
2. **Kepatuhan Penanganan Nyeri Pasien Terhambat:**  
   Standar Akreditasi Rumah Sakit (KARS) dan Sasaran Keselamatan Pasien mewajibkan dokumentasi monitoring nyeri berkala. Kegagalan simpan dokumen ini berpotensi menyebabkan ketidaksesuaian prosedur operasional standar (SOP).
3. **Pengalaman Pengguna Buruk (*Bad UX*):**  
   Perawat yang telah meluangkan waktu mengisi seluruh isian formulir klinis melihat formulir gagal disimpan tanpa pesan panduan antarmuka yang ramah pengguna.

---

## 3. Rincian Temuan & Analisis Teknis

### Aturan Invariant pada Backend ASP.NET Core:
**Berkas:** [`Areas/HealthServices/ClinicalManagement/Services/ClinicalInstrumentDefinitionEngine.cs` (Baris 268–270)](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/ClinicalInstrumentDefinitionEngine.cs#L268-L270)

```csharp
foreach (var (kode, nilai) in responses)
{
    if (!isianMenurutKode.TryGetValue(kode, out var isian))
    {
        hasil.Errors.Add($"Isian {kode} tidak ada pada versi instrumen ini.");
        continue;
    }

    // ATURAN KETAT: Jika butir memiliki atribut 'Binding', butir tersebut DILARANG berada di dictionary responses JSON!
    if (!string.IsNullOrWhiteSpace(isian.Binding))
        hasil.Errors.Add($"Isian {kode} disimpan pada kolom {isian.Binding}, bukan pada jawaban JSON.");

    var galat = CheckShape(isian, nilai);
    if (galat != null)
        hasil.Errors.Add(galat);
}
```

### Penyebab di Sisi Frontend:
**Berkas:** [`src/lib/hooks/health-services/inpatient-management/use-clinical-instrument-form.js`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-clinical-instrument-form.js)

Sebelum perbaikan, fungsi `buildSavePayload` mengirimkan seluruh objek state `responses` secara mentah ke dalam array `instrumentResponses`:

```javascript
// SEBELUM (BERMASALAH):
const instrumentResponses = resolvedInstrument?.versionId
  ? [
      {
        instrumentVersionId: resolvedInstrument.versionId,
        responses, // <--- Memuat butir ber-binding seperti PAIN_SCALE, PAIN_LOCATION, dll.
      },
    ]
  : [];
```

Karena instrumen `PAIN_MONITORING` memiliki atribut `binding` pada seluruh butirnya (`PainAssessmentState`, `PainScale`, `PainLocation`, `PainQuality`, `PainTrigger`, `PainManagement`, `PainNote`), server menganggap butir-butir tersebut diduplikasi di JSON, padahal tempat penyimpanannya adalah kolom tabel `TrxPatientAssessment`.

---

## 4. Langkah-Langkah Reproduksi Masalah

1. Buka halaman pengkajian nyeri pasien:
   `http://localhost:3000/health-services/inpatient-management/episodes/c3fe1370-18f0-42fb-8d9f-01449212828e/nursing?section=assessment&tab=pain`
2. Klik tombol **"Ada Nyeri"**.
3. Isi skala nyeri: `4`, lokasi: `Perut kanan bawah`, karakter: `Tertusuk-tusuk`, faktor pencetus: `Bergerak`, intervensi: `Relaksasi`, catatan: `Pasien kooperatif`.
4. Klik tombol **"Simpan Konsep"**.
5. **Hasil:** Konsol jaringan menampilkan penolakan `HTTP 400 Bad Request` dengan pesan error invariant binding di atas.

---

## 5. Tindakan Penyelesaian yang Telah Diterapkan (*Resolution*)

Memanfaatkan fungsi utilitas `filterUnboundResponses` yang mengekstrak metadata `definition.sections[].items[].binding`. Seluruh butir yang memiliki atribut `binding` disaring keluar dari dictionary `responses` JSON, dan hanya dipetakan ke properti *top-level* payload DTO (`painScale`, `painLocation`, `chiefComplaint`, dll.):

```javascript
// SESUDAH (SOLUSI RESMI):
export const filterUnboundResponses = (definition, responses) => {
  if (!responses || typeof responses !== "object") return {};
  const boundItemCodes = new Set();
  (definition?.sections || []).forEach((sec) => {
    (sec.items || []).forEach((it) => {
      if (it.code && it.binding) {
        boundItemCodes.add(it.code);
      }
    });
  });

  const unbound = {};
  Object.entries(responses).forEach(([code, val]) => {
    if (!boundItemCodes.has(code) && val !== undefined && val !== null && val !== "") {
      unbound[code] = val;
    }
  });
  return unbound;
};

// Pada buildSavePayload:
const unboundResponses = filterUnboundResponses(
  resolvedInstrument?.definition,
  responses
);

const instrumentResponses = resolvedInstrument?.versionId
  ? [
      {
        instrumentVersionId: resolvedInstrument.versionId,
        responses: unboundResponses, // <--- Hanya memuat butir murni instrumen tanpa kolom terikat
      },
    ]
  : [];
```

---

## 6. Bukti Data Jaringan & Hasil Verifikasi

### Muatan Permintaan (*Request Payload*) yang Berhasil:
```json
{
  "encounterId": "d0f70f24-5232-43f1-aee4-256308b2bf95",
  "inpEpisodeId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
  "assessmentType": 7,
  "instrumentResponses": [
    {
      "instrumentVersionId": "c1a1f000-0107-4a01-9b02-000000000005",
      "responses": {}
    }
  ],
  "vitalSignId": null,
  "painAssessmentState": 2,
  "completeImmediately": false,
  "painScale": 4,
  "painLocation": "Perut kanan bawah",
  "painQuality": "Tertusuk-tusuk dan hilang timbul",
  "painTrigger": "Meningkat saat bergerak atau batuk",
  "painManagement": "Teknik relaksasi nafas dalam & kompres hangat",
  "painNote": "Pasien kooperatif, skala nyeri dilaporkan berkurang setelah relaksasi"
}
```

### Hasil Eksekusi Live:
1. **Monitoring Nyeri (`POST` Create Draf):**
   ```text
   [HTTP RES] POST .../patient-assessments -> Status: 201 Created
   [HTTP RES] PUT .../patient-assessments/{id} -> Status: 200 OK
   [HTTP RES] PATCH .../complete -> Status: 200 OK
   ```
   Dokumen resmi selesai `#ASM-20260923-00004`, progres pengkajian naik menjadi 60%.
2. **Assesment Edukasi (`POST` Create Draf):**
   ```text
   [HTTP RES] POST .../patient-assessments -> Status: 201 Created
   [HTTP RES] PUT .../patient-assessments/{id} -> Status: 200 OK
   [HTTP RES] PATCH .../complete -> Status: 200 OK
   ```
   Butir umum (`EDU_RECIPIENT`, `EDU_NEEDS`, dll.) tersimpan di `responses` JSON dan `educationNote` tersimpan di kolom entitas tanpa penolakan. Dokumen resmi selesai `#ASM-20260923-00005`, progres pengkajian naik menjadi 80%.

Status isu dinyatakan **SELESAI (RESOLVED)**.
