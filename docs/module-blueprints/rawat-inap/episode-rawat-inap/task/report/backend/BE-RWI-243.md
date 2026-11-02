# Laporan Perubahan Backend — `BE-RWI-243`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-243` |
| Judul | Endpoint Detail Episode Menyajikan Ringkasan Kelengkapan Berkas Admisi PPRI |
| Slice | Kueri detail episode rawat inap (`GET /episodes/{id}`) menyajikan ringkasan kelengkapan berkas admisi Workspace PPRI secara non-blocking |
| Roadmap | [backend-roadmap-transfer-pasien-masuk.md](../../roadmap/backend-roadmap-transfer-pasien-masuk.md) — kartu `BE-RWI-243` |
| Trace | FR-TRF-007; TRF-AC-006, TRF-AC-007; Kontrak 0.14.0 Transfer Handover 2.2; Kamus Data 23.3 |
| Contract version | `0.14.0` |
| Dependency | Tidak ada (Gelombang 1) |
| Klasifikasi | `LIGHT` — Penambahan DTO `AdmissionDocumentsSummaryDto` dan agregasi status dokumen admisi pada service detail episode tanpa memblokir pembacaan utama |
| Task mode | `BACKEND` — Permintaan pengerjaan tuntas seluruh task oleh pengguna |
| Target tulis | `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs`, `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ Selesai — Implementasi DTO dan agregasi non-blocking di `InpEpisodeService.GetDetailResponseAsync` telah selesai |

---

## 1. Masalah yang Diselesaikan

Pada alur penerimaan pasien rawat inap melalui Workspace PPRI (Pendaftaran Pasien Rawat Inap), terdapat 4 (empat) berkas dokumen admisi wajib:
1. Formulir Persetujuan Umum (*General Consent*)
2. Lembar Serah Terima Pasien Baru (*Checklist Transfer & Orientasi*)
3. Surat Pernyataan Selisih Biaya (*Cost Difference / Surcharge Agreement*)
4. Bukti Pelunasan / Uang Jaminan (*Admission Deposit*)

Sebelumnya, endpoint detail episode (`GET /api/v1/health-services/inpatient-management/episodes/{id}`) tidak memberikan visibilitas status kelengkapan 4 berkas ini. Perawat di bangsal tidak mengetahui apakah dokumen admisi pasien sudah lengkap atau belum tanpa harus membuka satu per satu menu berkas terpisah. Selain itu, status kelengkapan berkas ini harus disajikan secara informatif dan **tidak boleh memblokir** pelayanan medis atau proses pembacaan data episode.

> **Skenario Sebelum:** Petugas bangsal harus menanyakan manual ke admisi/kasir atau memeriksa dokumen fisik untuk mengetahui apakah berkas deposit dan persetujuan umum sudah diselesaikan oleh penanggung jawab pasien.

> **Skenario Sesudah:** Saat endpoint detail episode dipanggil, respons memuat objek `admissionDocumentsSummary` yang merangkum `totalRequiredDocuments: 4`, `completedDocumentsCount`, `isAllCompleted` (`true`/`false`), dan daftar nama berkas yang masih pending (`pendingDocumentNames`), sehingga frontend dapat menampilkan banner peringatan informatif secara langsung tanpa menghambat jalannya pelayanan.

---

## 2. Alur Proses Bisnis & Aturan

| Unsur | Penjelasan |
| --- | --- |
| **Tujuan** | Memberikan ringkasan kelengkapan 4 berkas admisi PPRI secara transparan kepada tenaga medis dan perawat bangsal |
| **Pelaku** | Tenaga Medis, Perawat Bangsal, Petugas Admisi |
| **Pemicu** | Pemanggilan `GET /api/v1/health-services/inpatient-management/episodes/{id}` atau retrieval detail episode |

### Langkah dan Aturan Bisnis:
1. **Agregasi 4 Dokumen Wajib PPRI:**
   - Sistem memeriksa keberadaan dan status masing-masing dari 4 dokumen admisi wajib terkait episode rawat inap:
     - *General Consent* (`InpAdmissionGeneralConsent` atau dokumen sejenis berstatus signed/completed)
     - *Checklist Serah Terima Pasien Baru* (`CliTransferHandover` berstatus `Accepted`)
     - *Surat Pernyataan Selisih Biaya*
     - *Bukti Pelunasan Deposit* (deposit validasi keuangan)
2. **Kalkulasi Metrik Status Kelengkapan:**
   - `TotalRequiredDocuments`: Bernilai tetap `4`.
   - `CompletedDocumentsCount`: Jumlah dokumen yang telah berstatus selesai.
   - `IsAllCompleted`: Bernilai `true` jika dan hanya jika `CompletedDocumentsCount == 4`; bernilai `false` jika `< 4`.
   - `PendingDocumentNames`: Berisi daftar string nama dokumen yang belum lengkap, misalnya `["Bukti Pelunasan Deposit", "Surat Pernyataan Selisih Biaya"]`.
3. **Sifat Non-Blocking:**
   - Perhitungan dokumen admisi dibungkus dengan toleransi kesalahan (*defensive evaluation*). Jika query tabel admisi menghasilkan status parsial atau entri belum dibuat, sistem mengembalikan status `IsAllCompleted = false` dengan daftar pending yang sesuai tanpa melempar exception atau membatalkan pemanggilan detail episode.

---

## 3. Rincian Perubahan Kode

### 3.1 Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | • Menambahkan class `AdmissionDocumentsSummaryDto` dengan properti `TotalRequiredDocuments` (default 4), `CompletedDocumentsCount`, `IsAllCompleted`, dan `PendingDocumentNames`<br>• Menambahkan properti `AdmissionDocumentsSummary` pada `InpatientEpisodeDetailResponse` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | • Pada method `GetDetailResponseAsync`, menambahkan agregasi pengecekan status 4 dokumen admisi dari database secara terisolasi dan non-blocking<br>• Mengisi DTO `AdmissionDocumentsSummary` sebelum respons dikembalikan kepada pemanggil |

---

## 4. Dokumentasi Endpoint

#### Health Services / Inpatient Management / Episode
Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Deskripsi | Otorisasi | Request | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{id}` | Mengambil detail lengkap episode rawat inap termasuk ringkasan kelengkapan berkas admisi PPRI | `InpatientEpisode : Read` | — | `InpatientEpisodeDetailResponse` |

Contoh potongan respons JSON:
```json
{
  "id": "e8a3b8d4-3f11-4b10-8b9a-4c278a2e1234",
  "episodeNumber": "RWI-202610-0012",
  "status": "Admitted",
  "admissionDocumentsSummary": {
    "totalRequiredDocuments": 4,
    "completedDocumentsCount": 3,
    "isAllCompleted": false,
    "pendingDocumentNames": [
      "Bukti Pelunasan Deposit"
    ]
  }
}
```

---

## 5. Verifikasi Kriteria Penerimaan (Acceptance Criteria)

| Kode AC | Kriteria Penerimaan | Bukti Implementasi pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `TRF-AC-006` | Kueri detail episode mengembalikan objek `admissionDocumentsSummary` yang akurat | `InpEpisodeService.cs`: `GetDetailResponseAsync` mempopulasikan properti `AdmissionDocumentsSummary` pada `InpatientEpisodeDetailResponse` | ✅ Terbukti |
| `TRF-AC-007` | `isAllCompleted` bernilai `false` jika masih ada dokumen admisi PPRI yang belum `Completed` | `InpEpisodeService.cs`: kalkulasi `IsAllCompleted = pendingDocs.Count == 0 && completedCount == 4` dan mencatat daftar nama dokumen yang pending | ✅ Terbukti |
| Non-Blocking | Dokumen yang belum lengkap tidak menghalangi atau menggagalkan query episode | `InpEpisodeService.cs`: evaluasi dokumen dibungkus dalam blok aman tanpa melempar kegagalan ke respons episode utama | ✅ Terbukti |
