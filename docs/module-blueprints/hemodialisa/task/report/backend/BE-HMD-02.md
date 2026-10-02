# Laporan Perubahan Backend — `BE-HMD-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-02` |
| Judul | Integrasi Keutuhan Rekam Medis Dua Langkah dan Ekstensi Enum Lintas Modul |
| Slice | `MVP-0` — Fondasi, Model Data, dan Tata Kelola |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.1 |
| Trace | `CAP-07`, `CAP-08`, `CAP-18`, `HMD-DEC-010`, `FR-HMD-073`, `FR-HMD-074`, `NFR-008`, Temuan Kritis 1 (`01-existing-capability-map.md`) |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` ✅; koordinasi tertulis pemilik Rekam Medis — **diizinkan** Muhammad Hamzah pada sesi 22 September 2026 |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 0, logika 1, kontrak API 0, database 0, keamanan 2, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`, `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs`, `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule | `MedicalRecordManagement` (pemilik berkas) atas nama `HemodialysisManagement / Hemodialysis`; `MasterData` untuk `ServiceUnitType` |
| Pemilik / prefix registry | `Mrc` untuk berkas Rekam Medis; `Hmd` untuk pemanggil |
| Status registry | `Mrc` `ACTIVE`; `Hmd` `ACTIVE` sejak 22 September 2026 |
| Keberlakuan | `TOUCHED LEGACY` — ketiga berkas milik modul lain; perubahan bersifat aditif |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-ENUM-001`, `QBE-SVC-001`, `QBE-TXN-001` |

---

## 1. Masalah yang diperbaiki

Rekam Medis punya daftar tertutup jenis dokumen yang **benar-benar** dikunci setelah disahkan
(`JenisYangDitegakkan`). Jenis yang tidak ada di daftar itu dibiarkan lolos oleh
`EnsureMutableAsync` — aturan yang sengaja dibuat supaya alur lama tidak terblokir.

Akibatnya, bila catatan sesi hemodialisa hanya ditambahkan sebagai nilai enum tanpa masuk daftar
itu, sesi akan **terlihat** `Finalized` di layar tetapi tetap bisa diubah tanpa satu pun error.
Contoh: berat badan akhir pasien yang sudah disahkan dokter pukul 12.00 dapat diganti diam-diam
oleh siapa pun pukul 14.00. Ini **Temuan Kritis 1** blueprint.

---

## 2. Proses bisnis

1. Perawat menyelesaikan dokumentasi sesi; dokter penanggung jawab sesi mengesahkannya (`BE-HMD-17`).
2. Saat pengesahan, catatan sesi didaftarkan ke daftar keutuhan Rekam Medis dengan jenis
   `HemodialysisSession = 14`, penulisnya perawat dan penanda tangannya dokter.
3. Sejak detik itu setiap permintaan ubah terhadap sesi itu diperiksa di **dua tempat**:
   status sesi `Finalized` milik Hemodialisa, dan `EnsureMutableAsync` milik Rekam Medis.
   Keduanya menolak dengan `423 HMD-VAL-075`.
4. Koreksi hanya lewat addendum Rekam Medis, dan data asli tetap tersimpan.

Jalur tidak normal: bila pendaftaran ke Rekam Medis gagal, pengesahan dibatalkan seluruhnya dan
sesi tetap `AwaitingFinalization` (`HMD-VAL-073`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `01-existing-capability-map.md` bagian Temuan Kritis 1; `02-backend-architecture.md` bagian 6 dan 7
- `ClinicalDocumentIntegrityService.cs`, `ClinicalNoteAddendumService.cs`, `ClinicalNoteAddendumController.cs`, `ClinicalDocumentIntegrityController.cs`, `MedicalRecordTimelineService.cs`
- `MrcClinicalDocumentIntegrity`, `ClinicalDocumentKind`, `ServiceUnitType`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs` | Nilai `Hemodialysis = 10` beserta keterangannya |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | Nilai `HemodialysisSession = 14` beserta keterangannya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | (a) `ClinicalDocumentKind.HemodialysisSession` masuk `JenisYangDitegakkan`, dengan catatan izin pemilik 22 September 2026. (b) Method aditif `RegisterCountersignedAsync` untuk dokumen yang penulis dan pengesahnya dua orang berbeda — dipakai `BE-HMD-17`. Method lama tidak diubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint Rekam Medis yang menerima `documentKind` kini mengenal nilai `14`. Tidak ada endpoint yang berubah bentuk |
| Database | Tidak ada perubahan schema — enum disimpan sebagai integer |
| Keamanan/Auth | Inti task: catatan sesi HD yang disahkan kini benar-benar terkunci. `RegisterCountersignedAsync` tidak menyimpan sendiri, sehingga ikut transaksi pemanggil |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)` | `PASS` | Warning pada `ClinicalDocumentIntegrityService.cs` hanya CS1573 milik method lama `LockOpenDocumentsForEncounterAsync`, yang tidak diubah |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Pemeriksaan source `DitegakkanUntuk` | `JenisYangDitegakkan` memuat `HemodialysisSession`; `DitegakkanUntuk(kind) => JenisYangDitegakkan.Contains(kind)` | `PASS` | `ClinicalDocumentIntegrityService.cs` baris 87–100 |
| Pemeriksaan source `EnsureMutableAsync` | Untuk jenis yang ditegakkan, dokumen berstatus terkunci menghasilkan `IsAllowed = false` | `PASS` | `ClinicalDocumentIntegrityService.cs` baris 312 dst. |
| Pemakaian dua tempat oleh Hemodialisa | `GuardWritableAsync` memeriksa status `Finalized` lalu `EnsureMutableAsync(HemodialysisSession, …)`; dipanggil 16 method penulis sesi | `PASS` | `HmdSessionService.cs` baris 1104–1111 |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | Test `HemodialysisDocumentIntegrityTests` pada roadmap tidak dibuat; **dikecualikan atas keputusan pengguna 22 September 2026** |
| Uji runtime `PUT` pada sesi final → `423` | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna sedang berjalan dari build lama; lihat bagian 7 |

Uji manual: `NOT FEASIBLE` pada sesi ini — lihat catatan `NOT RUN`.

**Tidak dijalankan:** uji runtime HTTP. Menjalankan instance kedua akan menjalankan hosted service
dan seeder dua kali terhadap database yang sama.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `DitegakkanUntuk(ClinicalDocumentKind.HemodialysisSession)` mengembalikan `true` | Terpenuhi | Nilai ada di himpunan tertutup `JenisYangDitegakkan` (baris 93) |
| 2. Dokumen sesi `Finalized` yang diperiksa `EnsureMutableAsync` ditolak | Terpenuhi | `EnsureMutableAsync` menolak jenis yang ditegakkan setelah terkunci; Hemodialisa memetakan penolakan itu menjadi `423 HMD-VAL-075` |
| DoD: kedua berkas di Rekam Medis diperbarui | Terpenuhi | `ClinicalDocumentKind.cs` dan `ClinicalDocumentIntegrityService.cs` |
| DoD: unit test integritas | **Dikecualikan atas keputusan pengguna 22 September 2026** ("Tanpa test"), sejalan dengan `TEST_POLICY.md` | Diganti pemeriksaan source di atas |
| DoD: `dotnet build` lulus bersih | Terpenuhi | `0 Error(s)` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Berkas milik Rekam Medis disentuh atas izin eksplisit pemilik modul pada sesi 22 September 2026 |
| Masalah yang diketahui | `MedicalRecordTimelineService` (`NamaJenis`, `AmbilIsiDokumenAsync`) belum mengenal `HemodialysisSession`: di linimasa Rekam Medis catatan sesi HD akan tampil dengan nama teknis `HemodialysisSession` dan isi kosong. Di luar scope roadmap; disarankan task lanjutan |
| Risiko tersisa | Bila kelak ada yang menghapus nilai dari `JenisYangDitegakkan`, penguncian lapis Rekam Medis hilang; lapis status sesi tetap menolak |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Sama dengan blok pada laporan `BE-HMD-01` |
| Langkah berikutnya | Uji runtime `423` setelah pemilik menjalankan build terbaru; task lanjutan linimasa Rekam Medis |
