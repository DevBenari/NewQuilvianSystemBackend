# Laporan Perubahan Backend — `RJ-REG-REV-BE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-BE-002` |
| Judul | Kewarganegaraan pasien |
| Slice | Batch 1 — `RJ-REG-REV-003` (REV-09) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `3` |
| Trace | `RJ-REG-DEC-001`, `RJ-REG-DEC-006` |
| Contract version | Delta aditif DTO pasien (tanpa versi kontrak terpisah) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — entity + migration + DTO + validasi |
| Task mode | `CROSS-REPO MODE` (`RJ-REG-DEC-001`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a52b3c98` (`sukmagp`), belum di-commit |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

**Backend Governance Preflight:** Area `HealthServices`, Module `PatientManagement / MasterData`, prefix `Mst`,
keberlakuan `TOUCHED LEGACY` untuk `MstPatient`/`PatientController`, `NEW CODE` untuk enum `PatientCitizenship`.
QBE: `QBE-ENT-002`, `QBE-CFG-002`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`.

## 1. Masalah yang diperbaiki

Master pasien tidak punya kewarganegaraan. `CountryId` yang ada adalah negara **alamat**, bukan kewarganegaraan,
sehingga pasien WNA tidak dapat dicatat.

## 2. Proses bisnis

Pendaftaran pasien baru memilih WNI (bawaan) atau WNA. WNA wajib menunjuk negara asal dari master `MstCountry`;
WNI tidak menyimpan negara (tidak ada ID negara yang di-hardcode). Pemanggil lama (Kiosk, IGD) tidak mengirim
field ini → data lama `null`. `PUT` tanpa `citizenship` mempertahankan nilai tersimpan; `PUT` ke WNI
mengosongkan negara asal.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstPatient.cs`, `MstPatientConfiguration.cs`, `PatientDtos.cs`, `PatientController.cs` (create, update,
validasi, mapper), `MstCountry.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Enums/PatientCitizenship.cs` | Baru — `Wni = 1`, `Wna = 2` |
| `…/MasterData/Models/MstPatient.cs` | `Citizenship`, `NationalityCountryId`, navigasi `NationalityCountry` |
| `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs` | Konversi int, FK `Restrict`, index `NationalityCountryId` |
| `…/MasterData/DTOs/PatientDtos.cs` | Request: `Citizenship`, `NationalityCountryId`; response: + `CitizenshipName`, `NationalityCountryName` |
| `…/MasterData/Controllers/PatientController.cs` | Set saat create; pertahankan saat `PUT` tanpa nilai; `ValidateCitizenshipAsync`; label `WNI`/`WNA`; include negara |
| `Migrations/20261014044508_AddPatientCitizenship.cs` (+ Designer, snapshot) | Dua kolom nullable + index + FK |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada create/update/list/detail pasien |
| Database | Migration `20261014044508_AddPatientCitizenship` **diterapkan ke `QuilvianNewDevSukma`** (izin `RJ-REG-DEC-001`); hanya migration ini yang pending saat itu. `Down()` bersih |
| Keamanan/Auth | Tidak berubah (`Patient : Create/Update`) |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/patients/admin` | + `citizenship`, `nationalityCountryId` | `Patient : Create` |
| `PUT` | `/patients/admin/{id}` | Sama; `null` = pertahankan | `Patient : Update` |
| `GET` | `/patients/admin/{id}` | + `citizenship`, `citizenshipName`, `nationalityCountryId`, `nationalityCountryName` | `Patient : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Build Release | `0 Error` | `PASS` | — |
| EF `has-pending-model-changes` | No changes | `PASS` | — |
| QBE Strict | `VIOLATION 0` | `PASS` | — |
| `dotnet ef database update` (Sukma) | Applied | `PASS` | — |
| C1 create WNI | `200` | `PASS` | `rt_batch1.py` |
| C2 WNA tanpa negara | `400` | `PASS` | idem |
| C3 negara tidak ada | `400` | `PASS` | idem |
| C4 WNA + negara | `200` | `PASS` | idem |
| C5 detail mengembalikan `citizenship=2`, `WNA`, nama negara | sesuai (sesudah perbaikan label) | `PASS` | run ke-2 |
| C6 `PUT` tanpa field mempertahankan | sesuai | `PASS` | idem |
| C7 `PUT` ke WNI mengosongkan negara | sesuai | `PASS` | idem |
| Layar: pasien baru WNA tersimpan & terbaca ulang | `200`, `citizenship=2` | `PASS` | `ui_new.cjs` N7 + GET API |

Total runtime Batch 1: **29/29 PASS** (run awal 28/29: label `Wna` diperbaiki menjadi `WNA`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Create WNI tanpa negara | Terpenuhi | C1 |
| 2. WNA tanpa negara `400`; dengan negara tersimpan | Terpenuhi | C2, C4 |
| 3. Negara tidak ada/nonaktif `400` | Terpenuhi | C3 |
| 4. Detail mengembalikan 4 field | Terpenuhi | C5 |
| 5. `PUT` tanpa field mempertahankan | Terpenuhi | C6 |
| 6. Migration hanya dua kolom + FK + index | Terpenuhi | berkas migration |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Master `MstCountry` di `QuilvianNewDevSukma` hanya berisi `Indonesia`.** WNA secara teknis berfungsi, tetapi negara asing perlu diisi lewat Master Wilayah (data, bukan kode) |
| Masalah yang diketahui | — |
| Risiko tersisa | Server dev 7184 milik user masih biner lama; kolom nullable tidak memutusnya |
| Perubahan sampingan | `bin/Release`, `obj/Release` (ter-ignore) |
| Interupsi | `NONE` |
| Status Git | Berkas di atas `M`/`??` |
| Langkah berikutnya | Isi master negara; terapkan migration ke DB master saat rilis |
