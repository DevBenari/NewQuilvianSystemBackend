# Laporan Perubahan Backend — `BE-HMD-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-03` |
| Judul | Pendaftaran Dependency Injection Service, Definisi Hak Akses, dan Pengisian Data Master Awal |
| Slice | `MVP-0` — Fondasi, Model Data, dan Tata Kelola |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.1 |
| Trace | `HMD-DEC-007`, `HMD-DEC-013`, `FR-HMD-041`, `FR-HMD-051`, `NFR-011`; `02-backend-architecture.md` bagian 4, 5, 9; `contracts/permission-audit-matrix.md` bagian 2 dan 3 |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` ✅, `BE-HMD-02` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 0, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Program.cs`, `Areas/HealthServices/HemodialysisManagement/Constants/`, `.../Seeders/`, `.../Services/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source dan data di DB pribadi |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule | `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` sejak 22 September 2026 |
| Keberlakuan | `NEW CODE` (konstanta, seeder, service); `TOUCHED LEGACY` (`Program.cs`, baris data di `MstServiceUnit` dan `MstProcedure`) |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-CODE-004`, `QBE-CODE-005`, `QBE-ENUM-001` |

---

## 1. Masalah yang diperbaiki

Tabel `Hmd*` sudah ada, tetapi aplikasi belum bisa memakainya: service Hemodialisa belum
terdaftar sehingga controller mana pun yang memintanya akan gagal saat request pertama, nama hak
akses belum terdefinisi, dan unit HD belum punya data awal. Tanpa 12 butir checklist Pra-HD,
misalnya, perawat tidak punya apa pun untuk dicentang dan sesi pertama tidak akan pernah bisa
dinyatakan siap.

---

## 2. Proses bisnis

1. Saat aplikasi mulai, 10 service Hemodialisa didaftarkan sebagai *scoped* (satu instance per
   request).
2. Seeder `HemodialysisMasterDataSeeder` berjalan setelah seeder Radiologi, **hanya bila**
   `SeedDefaultData:Enabled` tidak dimatikan, dan hanya menambah baris yang belum ada:
   - 1 unit layanan `SU-HMD-001` "Unit Hemodialisa" bertipe `Hemodialysis`;
   - 1 tindakan `PR-HMD-001` "Hemodialisis";
   - 12 butir checklist Pra-HD, **seluruhnya `IsOverridable = false`**:
     `IDENTITY`, `ENCOUNTER`, `EPISODE`, `PRESCRIPTION`, `CONSENT`, `ALLERGY`,
     `VASCULAR_ACCESS`, `ISOLATION`, `MACHINE`, `STATION`, `WATER`, `SUPPLY`;
   - 5 butir kesiapan unit: `MACHINE`, `STATION`, `WATER` (wajib tanggal hasil), `SUPPLY`, `STAFF`;
   - 1 baris pengaturan unit.
3. Pengaturan bawaan dan artinya bagi unit:

| Pengaturan | Nilai | Artinya |
| --- | --- | --- |
| `MaxPatientsPerNurse` | 3 | Batas pasien per perawat per shift |
| `EnforceNurseRatio` | `false` | Melewati batas hanya memunculkan peringatan |
| `WaterResultValidityHours` | 720 | Hasil uji air berlaku 30 hari |
| `EnforceCompetencyCheck` | `false` | Kewenangan yang belum dapat diperiksa tidak memblokir penugasan |
| `AllowMultipleActiveEpisodePerPatient` | `false` | Satu pasien satu program aktif |
| `SessionStartGraceMinutes` | 60 | Toleransi mulai sesi 60 menit |
| `RequireDifferentSigner` | `true` | Pengesah wajib berbeda dari penyusun dokumentasi |
| `ProcedureId` | `PR-HMD-001` | Tindakan yang diterbitkan saat sesi dimulai |

Jalur tidak normal: bila seeder gagal, `RunStartupSeederAsync` mencatat kegagalannya tanpa
menghentikan aplikasi — pola yang sama dengan seeder lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Program.cs` (pendaftaran service dan urutan seeder)
- Seeder domain lain: `RadiologyMasterDataSeeder`, seeder Bank Darah
- `AccessMenuSeeder`, `AccessControllerAttribute`, `AccessActionAttribute`, `AccessPermissionAttribute`
- `contracts/permission-audit-matrix.md` bagian 2–3

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Program.cs` | `using` modul Hemodialisa; 10 `AddScoped<Hmd*Service>()` (baris 599–608); `RunStartupSeederAsync("HemodialysisMasterDataSeeder", …)` (baris 1416) |
| `Areas/HealthServices/HemodialysisManagement/Constants/HemodialysisPermissions.cs` | Baru. `ModuleCode = HEALTH_SERVICE_HEMODIALYSIS_MANAGEMENT`; 18 Resource beserta aksinya persis seperti matriks otorisasi |
| `Areas/HealthServices/HemodialysisManagement/Seeders/HemodialysisMasterDataSeeder.cs` | Baru. Idempoten berdasarkan Id dan kode; menghormati `SeedDefaultData:Enabled` |
| `Areas/HealthServices/HemodialysisManagement/Services/*.cs` | Kerangka 10 service beserta `HmdOperationResult.cs` (hasil operasi, kode error, teks pesan persis `validation-matrix.md`) dan `HmdServiceSupport.cs` (zona waktu Asia/Jakarta, kunci advisory, alokasi nomor lewat `NumberSeriesAllocator`) |

Delapan belas Resource hak akses: `HemodialysisOrder`, `HemodialysisEpisode`,
`HemodialysisEligibility`, `HemodialysisVascularAccess`, `HemodialysisSerology`,
`HemodialysisIsolation`, `HemodialysisPrescription`, `HemodialysisSchedule`,
`HemodialysisSession`, `HemodialysisObservation`, `HemodialysisMedication`,
`HemodialysisComplication`, `HemodialysisRecord`, `HemodialysisUnitReadiness`,
`HemodialysisMachine`, `HemodialysisStation`, `HemodialysisSetting`, `HemodialysisChecklistItem`.

Nomor bisnis dialokasikan `NumberSeriesAllocator` dengan koneksi sendiri, reset `NEVER`, 8 digit:
`HMD_ORDER`/`HD-ORD`, `HMD_EPISODE`/`HD-EP`, `HMD_SESSION`/`HD-SES`, `HMD_VITAL_SIGN`/`VTS-HD`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — konstanta dipakai controller pada task berikutnya |
| Database | Tidak ada perubahan schema. Baris data master ditulis ke DB pribadi `QuilvianNewDevHamzah` pada 22 September 2026 |
| Keamanan/Auth | 18 Resource dan aksinya menjadi butir yang dapat dicentang admin di layar Pengaturan → Manajemen Role → Akses Role. Tidak ada role yang di-hardcode |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Seeder dijalankan lewat `HemodialysisMasterDataSeeder.SeedAsync` dari DLL hasil build, dua kali berturut-turut | Setelah pengisian pertama, jalan ulang menghasilkan `unit=0 tindakan=0 checklist=0 kesiapan=0 pengaturan=0` — tidak ada baris ganda | `PASS` | Skrip `dotnet fsi` pada DB `QuilvianNewDevHamzah`, diulang 22 September 2026 |
| Isi data di DB | `SU-HMD-001 \| Unit Hemodialisa \| 10`; `PR-HMD-001 \| Hemodialisis`; checklist `12 \| 0 overridable \| 12 wajib`; kesiapan `5 \| 1 wajib tanggal hasil`; pengaturan `3 \| False \| 720 \| False \| False \| 60 \| True \| ProcedureId terisi` | `PASS` | Kueri baca-saja |
| Resolusi DI secara statis | Setiap parameter konstruktor 10 service terdaftar *scoped*: `ApplicationDbContext`, `NumberSeriesAllocator`, `InpatientClinicalContextService`, `ClinicalDocumentIntegrityService`, `ClinicalMilestoneFactProducer`, `DrugUsageService`, dan sesama service `Hmd*`. Tidak ada siklus (`Finalization → BillingHandoff`, `Schedule → CompetencyGate`, `Session → UnitReadiness`) dan tidak ada singleton yang memakai scoped | `PASS` | Pemindaian konstruktor terhadap `Program.cs` |
| Startup aplikasi | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna sedang berjalan dari build lama (`bin/` terkunci); instance kedua akan menjalankan hosted service dan seeder ganda pada DB yang sama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `DependencyInjectionResolutionTests` dan seeder test **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT APPLICABLE` — task tanpa endpoint.

**Tidak dijalankan:** startup aplikasi, dengan alasan di atas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Unit HD memiliki 12 butir checklist; seluruhnya `IsOverridable = false` | Terpenuhi | Kueri DB: 12 butir, 0 overridable |
| 2. `HmdSetting` termuat dengan toleransi mulai 60 menit dan masa berlaku air 720 jam | Terpenuhi | Kueri DB: `SessionStartGraceMinutes = 60`, `WaterResultValidityHours = 720` |
| 3. Seluruh 10 service dapat di-*resolve* tanpa dependency hilang atau siklik | Terpenuhi (bukti statis) | Seluruh dependency terdaftar, tanpa siklus. Pembuktian runtime `NOT RUN` |
| DoD: startup backend lulus tanpa error resolusi | **Belum dibuktikan runtime** — dicatat `NOT RUN`, tidak diklaim lulus. Dikecualikan menurut keputusan tetap pemilik 10 September 2026: verifikasi berat dijalankan pemilik sendiri | Menunggu pemilik menjalankan build terbaru |
| DoD: seeder lengkap dan idempoten | Terpenuhi | Jalan ulang menambah 0 baris |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kombinasi `AllowMultipleActiveEpisodePerPatient = true` tidak dapat berlaku karena unique index bersyarat dari kamus data tetap menolak episode aktif kedua; lihat `BE-HMD-08` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Data master baru ada di DB pribadi. Lingkungan lain terisi saat aplikasi pertama kali mulai dengan `SeedDefaultData:Enabled` menyala |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Pemilik menjalankan aplikasi dari build terbaru dan memastikan log startup tanpa error DI; admin mencentang butir hak akses Hemodialisa pada layar Akses Role |
