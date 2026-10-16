# Laporan Perubahan Backend — `RJ-REG-REV-BE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-BE-001` |
| Judul | Filter tanggal lahir pasien |
| Slice | Batch 1 — `RJ-REG-REV-001` (REV-01) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `3` |
| Trace | `RJ-REG-DEC-001`, `RJ-REG-DEC-002` |
| Contract version | Delta aditif pada `GET /patients` (tanpa versi kontrak terpisah) |
| Dependency | — |
| Klasifikasi | `LIGHT` — satu parameter query, satu berkas |
| Task mode | `CROSS-REPO MODE` (`RJ-REG-DEC-001`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a52b3c98` (`sukmagp`), belum di-commit |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

**Backend Governance Preflight:** Area `HealthServices`, Module `PatientManagement / MasterData`, prefix `Mst`
(BUSINESS DOMAIN / MASTER), keberlakuan `TOUCHED LEGACY` (controller lama mengakses DbContext langsung, tidak
direfactor). QBE: `QBE-API-001`, `QBE-VAL-001`, `QBE-PERM-001`.

## 1. Masalah yang diperbaiki

Petugas Pendaftaran Rawat Jalan hanya dapat mencari pasien dengan No. RM, NIK, No. HP, atau nama. Pasien dengan
nama pasaran (mis. "SITI") tidak dapat dipersempit tanpa tanggal lahir.

## 2. Proses bisnis

Petugas mengisi tanggal lahir (boleh tanpa isian lain). Frontend mengirim `birthDate=yyyy-MM-dd`. Server
mencocokkan kolom `BirthDate` (tipe `date`) persis, lalu menggabungkannya dengan `search` bila ada. Format salah
dijawab `400` dengan pesan berbahasa Indonesia. Contoh: `birthDate=1990-07-21` hanya mengembalikan pasien lahir
21 Juli 1990; `21-07-1990` ditolak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientController.cs` (`GetPatients`, `ApplyStandardFilter`, wrapper kiosk), `MstPatientConfiguration.cs`
(`BirthDate` = `date`), pemanggil frontend `emergency-registration.service.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Parameter opsional `birthDate` (string `yyyy-MM-dd`) di akhir `GetPatients`, `TryParseBirthDateFilter`, filter `BirthDate ==` nilai `DateTimeKind.Unspecified` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `birthDate` opsional. Wrapper Kiosk tidak meneruskannya (perilaku Kiosk identik) |
| Database | `NOT APPLICABLE` — memakai index `(FullName, BirthDate, Gender)` yang ada |
| Keamanan/Auth | Tetap `[AccessPermission("Patient","Read")]`; tidak ada data baru yang terekspos |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/patients?birthDate=yyyy-MM-dd&search=` | Cari pasien berdasarkan tanggal lahir, dapat digabung dengan kata kunci | `Patient : Read` |
| `GET` | `/patients/admin?birthDate=…` | Sama, rute admin | `Patient : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | `0 Error` | `PASS` | build ke scratchpad |
| `dotnet ef migrations has-pending-model-changes` | No changes | `PASS` | — |
| QBE Strict (18 berkas batch BE) | `VIOLATION 0` | `PASS` | `Invoke-QbeConformanceCheck.ps1 -Mode Strict` |
| B1 `birthDate` saja | hanya pasien bertanggal lahir itu | `PASS` | runtime 7185 `rt_batch1.py` |
| B2 `birthDate` + `search` | irisan (1 pasien) | `PASS` | idem |
| B3 tidak cocok | kosong | `PASS` | idem |
| B4 format `21-07-1990`, `1990-13-01`, `abc` | `400` | `PASS` | idem |
| B5 rute `/admin` | `200` | `PASS` | idem |
| B6 tanpa `birthDate` | perilaku lama | `PASS` | idem |
| B7 tanpa login | `401` | `PASS` | idem |
| `dotnet test` | — | `NOT RUN` | tidak ada project test (pola Bank Darah) |

Uji manual: `PASS` — layar Pendaftaran RJ (`RJ-REG-REV-FE-001`, U2: query `birthDate=1979-05-17`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `birthDate` saja → hanya pasien bertanggal lahir itu | Terpenuhi | B1 |
| 2. `birthDate` + `search` → irisan | Terpenuhi | B2, B3 |
| 3. Format salah → `400` | Terpenuhi | B4 |
| 4. Tanpa `birthDate` perilaku lama identik | Terpenuhi | B6 |
| 5. Izin `Patient : Read` tetap | Terpenuhi | Kode + B7 (`401`); `403` tanpa izin `NOT FEASIBLE` (akun uji SuperAdmin) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | Pencarian `search` lama tetap `Contains` lintas banyak kolom (bukan task ini) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan batch; berkas di atas `M` |
| Langkah berikutnya | UAT petugas pendaftaran |
