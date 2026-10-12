# Laporan Task Backend — `RJ-DOC-REV-BE-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-019` |
| Judul | Surat rujukan privat |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Keputusan | `RJ-DOC-DEC-077`, `078`, `081`, `082`, `083` |
| Kontrak | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Dependency | `RJ-DOC-REV-BE-018` ✅ |
| Task mode | `CROSS-REPO MODE` — backend |
| Branch / baseline | `sukmagp` @ `77caf434`, belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (`Reg`) |
| Keberlakuan | `NEW CODE` (`ReferralDocumentStorageService`); `TOUCHED LEGACY` (`PatientEncounterController`, `Program.cs`, `appsettings.json`, `.gitignore`) |
| QBE berlaku | QBE-SVC-001, QBE-VAL-001, QBE-PERM-001, QBE-LOG-001 (log hanya jumlah berkas dan ID), QBE-AUD-001, QBE-DEL-001 (hapus lunak + revisi) |

## 2. Yang dikerjakan

- `ReferralDocumentStorageService`:
  - Menulis ke `FileStorage:PrivateRootPath/referral-documents/{encounterId}/{documentId}.{ext}`.
  - Menolak bila root privat kosong atau berada di dalam folder publik (`UploadRootPath` atau `wwwroot`).
  - Memeriksa format dari *magic bytes* (PDF/JPEG/PNG) dan membatasi 5 MB per berkas serta 10 berkas aktif.
  - Membersihkan nama berkas dari path.
  - Menghitung ulang kelengkapan dan menulis revisi `Completed`, `DocumentAdded`, atau `DocumentRemoved`.
  - Bila metadata gagal tersimpan, berkas fisik yang sudah ditulis dihapus.
- Endpoint di `PatientEncounterController`:
  - Unggah admin (multipart, `files`).
  - Unggah Kiosk (`KioskRead` + batas `RJ-VAL-PM-14`; respons tanpa rincian).
  - Unduh (`inline`, `Cache-Control: no-store`, `nosniff`).
  - Hapus lunak.
- Konfigurasi `FileStorage:PrivateRootPath = Storage/private`, `FileStorage:MaxReferralDocumentSizeMb = 5`; registrasi DI.
- `.gitignore`: `/Storage/private/`.

| Berkas | Status |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/ReferralDocumentStorageService.cs` | Baru |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui |
| `Program.cs`, `appsettings.json`, `.gitignore` | Diperbarui |

## 3. Endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Hak akses |
| --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/{encounterId}/referral/documents` | `PatientEncounter : Update` |
| `POST` | `…/patient-encounters/kiosk/{encounterId}/referral/documents` | Policy `KioskRead` |
| `GET` | `…/patient-encounters/{encounterId}/referral/documents/{documentId}/content` | `PatientEncounter : Read` |
| `DELETE` | `…/patient-encounters/{encounterId}/referral/documents/{documentId}` | `PatientEncounter : Update` |

## 4. Delta terhadap desain

| Delta | Alasan |
| --- | --- |
| Batas Kiosk: status ≤ *Menunggu Dokter* (5), bukan ≤ *Masuk Antrean* (2) | Kunjungan Kiosk langsung menjadi *Menunggu Perawat* (3) begitu antrean dibuat, sehingga batas 2 akan selalu menolak. Tetap ≤ 30 menit dan hanya kunjungan `IsFromKiosk` |
| Endpoint di `PatientEncounterController` | Sama dengan `BE-018` (aturan `ControllerName`) |
| Service diambil dari `RequestServices` di controller | Konstruktor controller tetap kompatibel dengan harness uji lama |
| `.gitignore` disentuh | Surat pasien adalah data kesehatan dan tidak boleh ikut ter-commit |

## 5. Verifikasi

| Perintah / skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release` | 0 Error | `PASS` |
| EF | Tanpa perubahan model | `PASS` |
| QBE Strict | VIOLATION 0, REVIEW 0 | `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test | — |
| Runtime HTTP backend uji 7185 | **16/16 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| D0 | Sebelum unggah: `missingFields = ["documents"]` | PASS |
| D1 | `.jpg` berisi teks → `400 PM-12` | PASS |
| D2 | Berkas > 5 MB → `400 PM-13` | PASS |
| D3 | 11 berkas → `400 PM-13` | PASS |
| D4 | PDF + PNG → `200`, rujukan **lengkap**, nama `../../surat rujukan.pdf` dibersihkan, urutan halaman 1–2 | PASS |
| D5 | Revisi `Completed` tercatat | PASS |
| D6 | Unduh berizin → PDF, `inline`, `no-store` | PASS |
| D7 | Path yang sama lewat `/uploads/…` → `404`; berkas ada di folder privat | PASS |
| D8 | Unduh tanpa login → `401` | PASS |
| D9 | Akun Kiosk mengunduh → `403` | PASS |
| D10 | Kiosk unggah ke kunjungan petugas → `403 PM-14` | PASS |
| D11 | Kiosk unggah 2 halaman ke kunjungan Kiosk → `200`, respons tanpa rincian; rujukan tetap belum lengkap (diagnosa/alasan) | PASS |
| D12–D13 | Hapus satu surat → tetap lengkap; hapus terakhir → belum lengkap, revisi `DocumentRemoved` | PASS |
| D14 | Surat terhapus → `404` | PASS |
| D15 | Unggah ke kunjungan status 6 → `409 PM-09` | PASS |

Data uji: dua kunjungan dibatalkan. Berkas uji tersimpan di folder privat build scratchpad, bukan di repository.

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Unggah PDF/JPG/PNG ≤ 5 MB `200`; format palsu `PM-12`; > 5 MB atau > 10 `PM-13` | Terpenuhi | D1–D4, D11 |
| 2. Berkas tidak dapat dibuka lewat `/uploads/...` (`404`) | Terpenuhi | D7 |
| 3. Unduh tanpa `PatientEncounter : Read` `403` | Terpenuhi | D9 (akun tanpa izin), D8 (tanpa login `401`) |
| 4. Kiosk ke kunjungan non-Kiosk / > 30 menit `403 PM-14` | Terpenuhi — cabang non-Kiosk runtime (D10); cabang > 30 menit terbukti lewat kode | D10 |
| 5. Unggah melengkapi → `true`; hapus terakhir → `false`; tercatat | Terpenuhi | D4, D5, D13 |
| 6. `PrivateRootPath` kosong → unggah ditolak dengan pesan jelas | Terpenuhi lewat kode (`TryResolvePrivateRoot`), tidak diuji runtime karena memerlukan konfigurasi berbeda | Source |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration | Tidak ada |
| Temuan legacy | `Storage/uploads/` (foto kartu penjamin, foto pasien, QR) sudah ter-track di git. Di luar wewenang task ini; disarankan ditinjau pemilik |
| Konfigurasi lingkungan | Setiap lingkungan wajib mengisi `FileStorage:PrivateRootPath` di luar folder publik |
| Task berikutnya | `RJ-DOC-REV-FE-019`, `RJ-DOC-REV-FE-020`, `FE-KSK-014` |
| Perubahan sampingan | `.gitignore` (lihat delta) |
