# Laporan Penyelesaian Task: BE-HRD-234 Layanan Diskresi Penyesuaian Tier Rekredensialing & Audit Rekomendasi Komite Medik

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-234`
* **Nama Task**: Pembangunan Layanan Diskresi Otoritas Penyesuaian Tier Kelayakan Rekredensialing oleh Ketua Komite Medik, Validasi Catatan Justifikasi Klinis, dan Perekaman Audit
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IRecredentialingCompositeProvider.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/RecredentialingQualityController.cs`

---

## 2. Latar Belakang & Wewenang Diskresi Klinis Ketua Komite
Meskipun sistem komputasi menghasilkan rekomendasi tier eligibilitas secara objektif dan deterministik (Tier Hijau, Kuning, Merah), dunia kedokteran rumah sakit mengenal prinsip diskresi pertimbangan klinis (*clinical judgment*) oleh Ketua Komite Medik:
1. **Hak Diskresi Bersyarat (`VR-OPP-19`, `AT-HRD-OPP-21`)**:
   - Ketua Komite Medik memiliki kewenangan sah untuk melakukan penyesuaian tier (misalnya menaikkan dokter dari Tier Kuning ke Tier Hijau jika keterlambatan dokumentasi terbukti akibat beban darurat bencana atau situasi khusus yang dapat diterima).
   - Penyesuaian tier secara sepihak tanpa dasar pertimbangan dilarang keras: Sistem memvalidasi kewajiban input nota justifikasi klinis tertulis **minimal 20 karakter**. Permintaan override tanpa justifikasi ditolak sistem dengan galat `400 Bad Request`.
2. **Perekaman Jejak Audit Permanen (`RecredentialingTierOverridden`)**:
   - Setiap aksi penyesuaian tier dicatat secara permanen ke jejak audit sistem: mencakup identitas dokter target, tier asal, tier tujuan baru, identitas Ketua Komite, alasan klinis, dan stempel waktu presisi.
   - Dokumen telaah rekredensialing akan menampilkan label `IsOverridden = TRUE` beserta nota pertimbangan Ketua Komite untuk menjamin transparansi akreditasi KARS STARKES.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}/override-tier` | Penyesuaian diskresi tier kelayakan oleh Ketua Komite Medik dengan justifikasi tertulis. | `[Authorize]` Ketua Komite Medik | `OverrideRecredTierRequestDto` | `200 OK` (`ApiResponse<RecredentialingScorecardDto>`) |

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario: Penyesuaian Diskresi dari Tier Kuning ke Tier Hijau
* **Latar Belakang**: dr. Robert Sp.BS memiliki komposit 78.50 (Tier Kuning akibat keterlambatan resume medis saat penanganan tanggap darurat bencana gempa bumi).
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/e1f2a3b4-5678-90ab-cdef-1234567890ab/override-tier`
* **Request Body**:
```json
{
  "newTier": "GreenFastTrack",
  "justificationReason": "Keterlambatan resume medis terjustifikasi secara resmi oleh Direktur Pelayanan akibat penugasan operasi darurat korban bencana massal. Kompetensi teknik bedah terbukti prima dan nihil komplikasi.",
  "approvedByCommitteeChairmanId": "f9a8b7c6-d5e4-3a2b-1c0d-ef9876543210"
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Diskresi penyesuaian tier kelayakan berhasil diterapkan dan dicatat ke log audit.",
  "data": {
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "doctorName": "dr. Robert Sp.BS",
    "doctorCode": "DOC-BS-009",
    "finalCompositeScore": 78.5,
    "calculatedTier": "YellowClarificationReview",
    "assignedTier": "GreenFastTrack",
    "tierDescription": "Tier Hijau - Rekomendasi Jalur Cepat (Fast-Track) Perpanjangan SPK Tanpa Hambatan",
    "isOverridden": true,
    "tierOverrideReason": "Keterlambatan resume medis terjustifikasi secara resmi oleh Direktur Pelayanan akibat penugasan operasi darurat korban bencana massal. Kompetensi teknik bedah terbukti prima dan nihil komplikasi.",
    "overriddenBy": "f9a8b7c6-d5e4-3a2b-1c0d-ef9876543210",
    "overriddenByName": "dr. Hendra Sp.BS(K)",
    "overriddenAt": "2026-11-09T14:56:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Kewajiban Catatan Justifikasi Klinis (`VR-OPP-19`, `AT-HRD-OPP-21`)**: ✅ Lulus. Permintaan diskresi tanpa alasan $\ge 20$ karakter ditolak seketika; justifikasi sah berhasil mengubah `AssignedTier`.
2. **Perekaman Jejak Audit Diskresi**: ✅ Lulus. Rincian pengubah, waktu, dan alasan tersimpan permanen pada metadata scorecard.
