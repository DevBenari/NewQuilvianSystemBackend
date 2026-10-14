# Laporan Perubahan Backend — `RJ-REG-REV-BE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-BE-003` |
| Judul | Detail dan edit data pendaftaran di Daftar Pasien RJ |
| Slice | Batch 1 — `RJ-REG-REV-002` (REV-08) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `3` |
| Trace | `RJ-REG-DEC-005`, `RJ-REG-DEC-006`; kontrak cakupan `RJ-DOC-ENCLIST-001@1.0.0` |
| Contract version | Delta aditif pada `OutpatientEncounterController` |
| Dependency | `RJ-REG-REV-BE-002` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `CROSS-REPO MODE` (`RJ-REG-DEC-001`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a52b3c98` (`sukmagp`), belum di-commit |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

**Backend Governance Preflight:** Area `HealthServices`, Module `RegistrationManagement`, prefix `Reg` (ACTIVE),
`NEW CODE` (service + DTO baru), controller tetap tanpa DbContext (`QBE-SVC-001`). QBE: `QBE-SVC-001`,
`QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`.

## 1. Masalah yang diperbaiki

Daftar Pasien RJ hanya menampilkan baris dan aksi batal/rujukan. Petugas tidak dapat membuka detail kunjungan
maupun memperbaiki No. HP/alamat pasien yang salah tanpa pindah ke master pasien (yang memakai izin dan cakupan
lebih luas).

## 2. Proses bisnis

1. Petugas membuka **Detail** dari baris. Server memakai cakupan yang **sama** dengan daftar
   (`OutpatientEncounterListService.BuildScopedQuery`): dokter hanya pasiennya, perawat hanya poli cluster-nya,
   pemegang `ReadAll` semua. Di luar cakupan → `404` (keberadaan kunjungan tidak bocor).
2. Detail memuat kunjungan, antrean, pembayaran, rujukan, pasien, `canEdit`, dan `editBlockedReason`.
3. **Edit** hanya bila sesi memegang `OutpatientEncounter : Update` **dan** status < `Sedang Konsultasi`, belum
   batal/selesai/tidak hadir. Field: No. HP, WhatsApp, email, alamat, kewarganegaraan (ke master pasien) dan
   Catatan (kunjungan). Nama, No. RM, NIK, tanggal lahir, status, poli, dokter **tidak** ada di request.
4. Baris kunjungan dikunci `FOR UPDATE`; status diperiksa ulang di dalam transaksi.
5. Audit (`LoggerService.AuditAsync`) mencatat **nama** field yang berubah dan aktor, tanpa nilai PII.

Contoh: kunjungan `ENC-RSMMC-00197` berstatus *Sedang Konsultasi* → `canEdit=false`, PATCH ditolak `400`
"Kunjungan dengan status Sedang Konsultasi tidak dapat diubah dari Daftar Pasien Rawat Jalan."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientEncounterController.cs`, `OutpatientEncounterListService.cs`, `ClinicalActorScopeService.cs`,
`OutpatientEncounterExplicitPermissions.cs`, `RegPatientEncounter.cs`, `EncounterReferralService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterDetailService.cs` | Baru — `GetDetailAsync`, `UpdateRegistrationDataAsync`, `GetEditBlockedReason` |
| `…/DTOS/OutpatientEncounterDetailDtos.cs` | Baru — response detail, pasien, request edit |
| `…/Controllers/OutpatientEncounterController.cs` | `GET /{id}`, `PATCH /{id}/registration-data`; deskripsi controller |
| `Program.cs` | Registrasi `OutpatientEncounterDetailService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru, aditif |
| Database | `NOT APPLICABLE` (tanpa migration) |
| Keamanan/Auth | Action baru `OutpatientEncounter : Update` — **perlu dicentang admin di Akses Role** untuk petugas. Cakupan server-side dipertahankan; tidak memakai endpoint `PatientEncounter` yang lebih luas |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Outpatient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/outpatient-encounters/{id}` | Detail kunjungan + pasien dalam cakupan | `OutpatientEncounter : Read` |
| `PATCH` | `/outpatient-encounters/{id}/registration-data` | Ubah kontak, kewarganegaraan, catatan | `OutpatientEncounter : Update` |

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build Release / EF / QBE Strict | `0 Error` / no changes / `PASS` | `PASS` |
| D1 detail dalam cakupan `200`, `canEdit=true` | sesuai | `PASS` |
| D2 tidak ada `404`; D3 tanpa login `401` | sesuai | `PASS` |
| D4 edit tersimpan (email dinormalkan huruf kecil) | sesuai | `PASS` |
| D5 terbaca ulang; nama/No. RM tetap | sesuai | `PASS` |
| D6 field identitas di body diabaikan | sesuai | `PASS` |
| D7 email/HP salah, WNA tanpa negara, catatan > 500 → `400` | sesuai | `PASS` |
| D8/D9 status ≥ 6 → `canEdit=false`, PATCH `400` | sesuai | `PASS` |
| D10 pemulihan data uji | `200` | `PASS` |
| Layar L1–L5 (`RJ-REG-REV-FE-002`) | 5/5 | `PASS` |
| `403` tanpa `OutpatientEncounter : Update` | — | `NOT RUN` — akun uji SuperAdmin; mencopot role butuh SQL dengan izin terpisah. Dibuktikan lewat kode (`[AccessPermission]` + `canEdit`) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Detail dalam cakupan `200`; luar cakupan/tidak ada `404`; tanpa izin `403` | Terpenuhi sebagian uji: `200`/`404`/`401` diuji; `403` lewat kode | D1–D3 |
| 2. Edit tersimpan dan terbaca ulang | Terpenuhi | D4, D5, L5 |
| 3. Status ≥ 6/batal/selesai → `400` | Terpenuhi | D9 |
| 4. Identitas tidak dapat diubah | Terpenuhi | D6 |
| 5. Audit tanpa PII | Terpenuhi | kode `AuditAsync` (`ChangedFields`) |
| 6. `canEdit` sesuai izin + status | Terpenuhi | D1, D8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Data kontak satu kunjungan uji (`ENC-RSMMC-00282`) dikembalikan, tetapi kewarganegaraannya kini `WNI` (sebelumnya `null`) |
| Risiko tersisa | Kontak diubah pada master pasien → berlaku ke kunjungan lain pasien itu (disengaja, satu sumber kebenaran) |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` |
| Langkah berikutnya | Admin mencentang `OutpatientEncounter : Update` untuk role petugas pendaftaran |
