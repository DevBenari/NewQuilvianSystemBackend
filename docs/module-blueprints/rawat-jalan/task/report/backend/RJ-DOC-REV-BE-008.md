# Laporan Perubahan Backend — `RJ-DOC-REV-BE-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-008` |
| Judul | Aturan kunjungan RJ berklinik dan pelonggaran pemblokir pendaftaran |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `11.1` |
| Trace | `RJ-DOC-DEC-012`, `016`, `017`, `019`, `021`, `022`, `023`; desain `02-backend-architecture.md` DP.3.1, DP.3.2, DP.3.4, DP.5 |
| Kontrak | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — bagian *Perubahan perilaku endpoint yang sudah ada* |
| Wewenang | `RJ-DOC-DEC-025` — source + runtime `QuilvianNewDevSukma`; tanpa migration, commit, push |
| Task mode | `BACKEND` |
| Baseline | BE `245f0464` (`sukmagp`); working tree hanya berisi dokumen blueprint sesi ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI — AT-DP-15, 16, 17, 19 terbukti runtime; AT-DP-18 terbukti lewat diff |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (prefix `Reg`, `ACTIVE / LEGACY` di registry) |
| Keberlakuan | `NEW CODE` untuk `OutpatientEncounterRules.cs`; `TOUCHED LEGACY` untuk `PatientEncounterController.cs` dan `MedicalRecordAccessAuditService.cs` (komentar saja) |
| Registry | Tidak ada entity baru, sehingga `QBE-MOD-002`/`003` tidak berlaku |
| QBE yang berlaku | `QBE-MOD-001` (berkas baru di Submodule pemilik), `QBE-SVC-001` (aturan berada di lapisan service), Conformance Strict pada berkas yang disentuh |
| Hak akses | Endpoint tidak ditambah atau diubah atributnya. Tanpa hardcode role |
| Migration / DB | Tidak ada |

## 2. Perubahan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterRules.cs` | Baru | Kelas statis satu sumber aturan: `WhereOutpatientClinicEncounter` (Outpatient, `ClinicId` terisi, tanpa `EmgVisit`, belum dihapus), `WhereBlocksRegistration` (ditambah belum batal, `CompletedAt` kosong, status ≤ `InConsultation`), dan expression `IsCancellableStatus` untuk `BE-010` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui | `FindActiveEncounterAsync` memakai `WhereBlocksRegistration(_dbContext)` menggantikan `KunjunganMasihBerjalan`. Dipanggil di validasi awal **dan** cek ulang di dalam transaction create (advisory lock `RJ-DOC-REV-BE-007` tetap). Endpoint lain tidak disentuh |
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordAccessAuditService.cs` | Diperbarui (komentar) | Catatan bahwa pendaftaran tidak lagi memakai `KunjunganMasihBerjalan`. Logika tidak berubah |

Diff: 2 berkas diubah (+10 / −6) dan 1 berkas baru.

**Delta terhadap desain:** DP.5 menulis aturan sebagai tiga `Expression`. Dua di antaranya dibuat
sebagai extension method `IQueryable` yang menerima `ApplicationDbContext`, karena syarat "tanpa
`EmgVisit`" membutuhkan subquery ke `DbSet<EmgVisit>` — `RegPatientEncounter` tidak punya navigasi
ke `EmgVisit`. Aturannya identik dengan DP.3.1/DP.3.2. `IsCancellableStatus` tetap `Expression`.

## 3. Validasi

| Perintah / bukti | Hasil | Status |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -nodeReuse:false -p:UseSharedCompilation=false` | Exit 0, `0 Error(s)`, `244 Warning(s)` (sama dengan baseline `RJ-DOC-REV-BE-007`), 1 m 34 s. Satu-satunya warning di berkas task: `CS8619` existing pada `PatientEncounterController.cs:2423` (bergeser satu baris) | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` × 3 | Setiap berkas `VIOLATION 0`, `REVIEW 0`, `Final result: PASS` | `PASS` |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah, tanpa project test | — |

### Uji runtime

Lingkungan: `bin/Release/net9.0`, `Development`, `Runtime__Role=Web`, `http://localhost:5199`, DB
`QuilvianNewDevSukma`. Akun seed SuperAdmin; cookie `quilvian_access_token` (bertanda `secure`)
dikirim manual karena uji memakai HTTP lokal. Kredensial tidak dicetak. Pasien uji
`KSKTEST-RM-07`; jadwal dokter Jumat `1a117688-…` (00.00–23.55, walk-in); unit penunjang
`fddbe4ae-…` tanpa klinik. Keluhan uji `UJI-RJDP-BE008`.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| R0 | Login | `200` | `PASS` |
| R1 | Buat kunjungan penunjang tanpa klinik (`ENC-RSMMC-00182`) | `200` | `PASS` |
| R2 | **AT-DP-17** — pasien punya kunjungan penunjang aktif, daftar ke poli | `200` (`ENC-RSMMC-00183`). Sebelum task ini akan `400` | `PASS` |
| R3 | **AT-DP-16** — daftar lagi saat `00183` berstatus < 7 | `400` "Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00183 tanggal 02 Okt 2026. …" | `PASS` |
| R4 | `PATCH /admin/{00183}/status` → `7` | `200` | `PASS` |
| R5 | **AT-DP-15** — daftar saat `00183` berstatus 7 | `200` (`ENC-RSMMC-00184`) | `PASS` |
| R6 | **AT-DP-19** — `PATCH /admin/{00183}/cancel` lama pada status 7 | `200`, perilaku lama tetap | `PASS` |
| R7 | `POST /admin` tanpa autentikasi | `401` | `PASS` |

Ringkasan: `8/8 PASS` (`rt_be008.log` di scratchpad sesi). Dua run sebelumnya gagal karena skrip:
run 1 cookie `secure` tidak terkirim (`401`, tanpa tulisan); run 2 jadwal dokter tidak berlaku
hari Jumat (`400`, satu kunjungan penunjang `00181` terbentuk lalu dibatalkan lewat API).

**AT-DP-18** (akses rekam medis tidak berubah): dibuktikan lewat diff — `KunjunganMasihBerjalan`
dan seluruh pemakainya di `MedicalRecordAccessAuditService` tidak berubah selain komentar. Tidak
diuji runtime.

**Bagian IGD dari AT-DP-17** tidak diuji runtime (membuat kunjungan IGD memerlukan alur IGD
tersendiri); dicakup aturan `EncounterType = Outpatient` dan tanpa `EmgVisit`.

Keadaan data sesudah uji (query read-only): `00181`–`00184` milik `KSKTEST-RM-07` seluruhnya
`IsCancel = true`; pasien uji tanpa kunjungan aktif. Pembersihan memakai endpoint aplikasi.
`bin/Release/net9.0/appsettings.Development.json` sudah ada sebelum uji dan ditimpa dengan salinan
yang sama dari root repository; berkas itu berada di folder build yang tidak di-track.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-DP-15` status 7 tidak memblokir | Terbukti | R5 |
| `AT-DP-16` status 0–6 tetap memblokir dengan pesan lama | Terbukti | R3 |
| `AT-DP-17` penunjang/IGD tidak memblokir | Terbukti untuk penunjang; IGD lewat aturan | R2 |
| `AT-DP-18` akses rekam medis tidak berubah | Terbukti lewat diff | §2 |
| `AT-DP-19` endpoint cancel lama tetap | Terbukti | R6 |

## 5. Risiko tersisa

1. Pemblokir kini lebih longgar: pasien dengan kunjungan status 7–8 yang tagihannya belum lunas
   dapat mendaftar ke poli lain. Ini keputusan pemilik (`RJ-DOC-DEC-019`).
2. Kunjungan penunjang tanpa klinik yang menggantung tidak lagi memblokir, tetapi juga tidak
   ditutup oleh siapa pun di luar `KioskEncounterClosureService` (kiosk saja).

## 6. Task berikutnya

`RJ-DOC-REV-BE-009` — daftar, summary, metadata bercakupan dan butir hak akses.
