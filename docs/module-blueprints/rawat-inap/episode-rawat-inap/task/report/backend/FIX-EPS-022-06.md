# Laporan Perubahan Backend — FIX-EPS-022-06

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Predikat tunggal kelayakan service unit registrasi rawat inap |
| Plan | PLAN-REPAIR-EPS-022 |
| Target | `NewQuilvianSystemBackend`, branch `MHamzah`, basis `ca09cae909597442385ce4513be9c803549be10e` |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE; database/migration tidak diperlukan |

## QBE preflight

| Item | Hasil |
| --- | --- |
| Area/module | HealthServices / InPatientManagement + validasi RegistrationManagement |
| Prefix/registry | `Inp`, modul aktif; tidak menambah entity/prefix/schema |
| Capability | Repair query ketersediaan bed dan validasi encounter existing |
| Endpoint | Tidak ada endpoint baru atau perubahan route/payload |
| Security | Permission, scope unit, dan audit existing dipertahankan |
| Database | Tidak ada perubahan model/migration/eksekusi database |
| QBE | QBE-SVC-001, QBE-VAL-001, QBE-API-001; tidak ada pengecualian |

## Implementasi

`ServiceUnitRegistrationRules.InpatientRegistrationEligible` menjadi expression bersama dengan empat syarat: aktif, tidak dihapus, tersedia untuk registrasi, dan bertipe `Inpatient`. Expression dipakai oleh:

1. default unit rawat inap pada pembentukan encounter;
2. validasi unit encounter rawat inap; dan
3. subquery unit yang membatasi `SearchAvailableBedsAsync`.

Scope unit petugas dari `InpBedAccessService` tetap diterapkan bersama predicate ini. Validasi saat menulis tetap dipertahankan untuk menolak perubahan master data setelah daftar bed dimuat.

## Berkas

- `Areas/HealthServices/MasterData/Services/ServiceUnitRegistrationRules.cs`
- `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
- `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`

## Acceptance dan verifikasi

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Unit nonaktif/terhapus/tidak tersedia/bukan inpatient tidak ditawarkan | Terpenuhi source | Query bed memakai expression empat syarat |
| Encounter tetap menolak unit stale | Terpenuhi source | Validasi write memakai expression yang sama |
| Tidak membuat endpoint/schema baru | Terpenuhi | Diff source |
| Build backend | PASS | Build terpisah ke `artifacts/validation/issue-022`, 0 error; build output default hanya terhalang executable localhost yang sedang dikunci PID 16608 |

Peringatan build tersisa berasal dari advisory `SixLabors.ImageSharp` yang sudah ada. Automated backend test baru tidak dibuat karena task ini tidak memberikan otorisasi pembuatan test backend; validasi dilakukan dengan build dan inspeksi query/kontrak.

