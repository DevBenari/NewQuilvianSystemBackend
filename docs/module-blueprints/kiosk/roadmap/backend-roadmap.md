# Backend Roadmap — Modul Kiosk

| Field | Nilai |
| --- | --- |
| Roadmap ID | `KSK-RM-BE-001` |
| Revision | `1` |
| Status | `approved` — Sukma Giri Pratama, 2026-09-30 |
| Blueprint ID | `KSK-BP-001` r1, status `approved` (Sukma Giri Pratama, 2026-09-30) |
| SHA baseline | BE `419b910f` (branch `sukmagp`), FE `4ec51b0b` |
| Kontrak masukan | `KSK-CONTRACT-v1` — `approved` (hash per berkas di `blueprint-manifest.md#artifact_hashes`) |
| Masukan desain | `02-backend-architecture.md`, `04-prd-to-mvp.md` §10, §20 |
| Owner | Sukma Giri Pratama |

> **Batas dokumen.**
> 1. Roadmap ini memecah desain menjadi task kecil. Roadmap ini **bukan** izin menulis kode. Setiap task baru boleh dikerjakan lewat `build-module-backend` dengan `TASK MODE: BACKEND` yang dinyatakan eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan dokumen engineering canonical (`rules/backend/engineering/`), bukan dari roadmap ini.
> 3. **Tidak ada migration** di seluruh roadmap. Bila sebuah task ternyata membutuhkan migration, task itu berhenti dan kembali ke desain.
> 4. Verifikasi mengikuti pola Bank Darah: tanpa folder `Tests/`, `dotnet test` dicatat `NOT RUN — tidak ada project test`. Uji runtime memakai DB `QuilvianNewDevSukma` dengan data samaran dan izin eksekusi per sesi.
> 5. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan/gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-KSK-001 ✅ ─> BE-KSK-002 ✅

{KSK-OQ-005 ✅} ─> BE-KSK-003
```

`{KSK-OQ-005}` = pencatatan amendment `RWI-ENC-PAYER-001` v1.1.0 di blueprint rawat-inap (keputusannya `KSK-DEC-013` sudah `approved`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-KSK-001` |
| 2 | `BE-KSK-001` | `BE-KSK-002` |
| — | ⛔ menunggu `KSK-OQ-005` | `BE-KSK-003` |

Jumlah pasangan prasyarat → task: 2 (`BE-KSK-001→002`, `KSK-OQ-005→003`).

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-KSK-001` | Endpoint lookup pasien menjawab 4 hasil dengan data minimal | EPIC KSK-01; `FR-KSK-001..005, 007, 008, 009`; `KSK-DEC-006/007/017/018/019`; `KSK-DSN-001..004, 006, 008` | `KSK-CONTRACT-v1` api §Kiosk Patient Lookup, validation §1 | `MstPatient` (+ identity document, insurance, membership) baca-saja; `ApiResponse<T>`; policy `KioskRead`; pola `FindPatientAsync` | Enum ×2, DTO, service, controller, DI | — | Lihat kartu | Build, EF, QBE, verifier, runtime R0..R9 | Normalisasi HP di query tidak diterjemahkan EF → fallback SQL berparameter / Sukma | Kartu |
| ✅ `BE-KSK-002` | Lookup dibatasi 10/menit/akun perangkat | EPIC KSK-01; `FR-KSK-006`; `KSK-DEC-011`; `KSK-DSN-005` | api §Kiosk Patient Lookup (kode `429`) | Middleware rate limiting bawaan `net9.0` | `Program.cs`, `appsettings.json`, atribut endpoint | `BE-KSK-001` | Lihat kartu | Build, verifier, runtime R0..R4 | Salah urutan middleware membuat partisi jatuh ke IP / Sukma | Kartu |
| `BE-KSK-003` | Route kiosk menerima Penjamin Perusahaan | EPIC KSK-04; `FR-KSK-030/031`; `KSK-DEC-013` | api §Patient Encounter | `CreateEncounterCoreAsync`, `LoadValidPatientCompanyGuarantorAsync` | Satu argumen + komentar di `PatientEncounterController` | `KSK-OQ-005` | Lihat kartu | Build, verifier, runtime R0..R4 | Melebarkan wewenang kiosk; dijaga validasi existing / Sukma + Muhammad Hamzah | Kartu |

## Kartu task

### ✅ `BE-KSK-001` — Layanan Lookup No. RM

| Aspek | Isi |
| --- | --- |
| Status | ✅ SELESAI — 30 September 2026. Build PASS (0 error), EF tanpa perubahan model, QBE Strict 6/6 berkas PASS, authorization verifier PASS (fallback 69 tetap), runtime 20/20 skenario PASS (termasuk R13 `403` akun non-kiosk) + log tersamar PASS; AC 1–11 terpenuhi; label `api-contract.md` diperbarui. `dotnet test` NOT RUN — tidak ada project test. Laporan: [task/report/backend/BE-KSK-001.md](../task/report/backend/BE-KSK-001.md) |
| Gelombang | `MVP-0`, gelombang eksekusi 1 |
| Outcome | `POST api/v1/health-services/registration-management/kiosk-patient-lookups` menjawab `Found` / `NotFound` / `MultipleMatch` / `ContactStaff` dengan respons Kartu Pasien minimal |
| File | Baru: `Areas/HealthServices/RegistrationManagement/Enums/KioskPatientLookupSearchType.cs`, `.../Enums/KioskPatientLookupResult.cs`, `.../DTOS/KioskPatientLookupDtos.cs`, `.../Services/KioskPatientLookupService.cs`, `.../Controllers/KioskPatientLookupController.cs`. Diperbarui: `Program.cs` (`AddScoped<KioskPatientLookupService>()` saja) |
| Tidak termasuk | Rate limit (`BE-KSK-002`); perubahan `PatientController`, `KioskScanSessionController`, model, configuration, migration |
| Aturan wajib | Controller tanpa `ApplicationDbContext`; endpoint `[Authorize(Policy = AuthorizationPolicies.KioskRead)]` **tanpa** `[AccessAction]`/`[AccessPermission]` (`KSK-DSN-006`); `[Tags("Health Services / Registration Management / Kiosk Patient Lookup")]`; query `AsNoTracking` dan berparameter; enum dikirim sebagai angka; log hanya 4 digit terakhir |

**Acceptance criteria**

1. KTP 16 digit milik satu pasien Aktif → `200`, `result = 1`, `nextAction = EXISTING_PATIENT_REGISTRATION`, dan `patient` berisi tepat `patientId, medicalRecordNumber, patientCode, fullName, patientTypeName, genderName, bloodTypeName`.
2. KTP yang hanya ada di `MstPatientIdentityDocument` aktif → ditemukan.
3. HP `0812-3456-7890` cocok dengan tersimpan `+6281234567890`; `021 555 1234` cocok dengan `+62215551234`.
4. HP yang dipakai dua pasien → `result = 3`, `patient = null`, `nextAction = USE_IDENTITY_NUMBER_OR_CONTACT_STAFF`.
5. Pasien `IsDeceased`, `Inactive`, atau `Blacklisted` → `result = 4`, `patient = null`.
6. Pasien A `Merged` → B Aktif → `result = 1` dengan `patientId = B`; rantai berputar → `result = 4`, tanpa hang.
7. Nomor tidak terdaftar → `result = 2`, `nextAction = NEW_PATIENT_REGISTRATION`.
8. KTP 15 digit / huruf, HP `12345`, nilai berisi `<` atau karakter kontrol, `searchType` di luar 1–4 → `400` dengan pesan `validation-matrix.md` §1.
9. `searchType = 3/4` menemukan pasien dari kartu asuransi / nomor member aktif.
10. Log aplikasi untuk lookup memuat `searchType`, `result`, dan `****` + 4 digit terakhir saja.
11. Tanpa token → `401`; token pegawai biasa tanpa role Kiosk/Admin → `403`.

**Verifikasi**

| Langkah | Bukti |
| --- | --- |
| Build | `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>` — 0 error |
| EF | `dotnet ef migrations has-pending-model-changes --no-build` → tidak ada perubahan |
| QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` |
| Otorisasi | `bash tools/authorization-verifier/verify-authorization.sh` → exit `0`; `approved-compatibility-fallback.txt` tidak berubah |
| Runtime | App dari scratchpad terhadap `QuilvianNewDevSukma`; R0 kondisi awal (tanpa data diubah), R1–R9 = AC 1–9 memakai pasien samaran yang disiapkan dengan izin, dan keadaan DB sesudah run dicatat |
| `dotnet test` | `NOT RUN — tidak ada project test` |

**DoD:** AC 1–11 terbukti di laporan `task/report/backend/BE-KSK-001.md`; lima langkah verifikasi lulus; tidak ada migration; `api-contract.md` label berubah dari "Rencana" menjadi "Tersedia" oleh task ini.

### ✅ `BE-KSK-002` — Rate limit lookup

| Aspek | Isi |
| --- | --- |
| Status | ✅ SELESAI — 30 September 2026. Build PASS (0 error), EF tanpa perubahan model, QBE Strict 2/2 PASS, authorization verifier PASS (fallback 69 tetap), runtime R1–R6 PASS (10×`200` lalu `429` + `Retry-After: 60`; akun B `200`; endpoint lain `200`; pulih setelah 62 detik; `PermitPerMinute=3` → ke-4 `429`). `dotnet test` NOT RUN — tidak ada project test. Laporan: [task/report/backend/BE-KSK-002.md](../task/report/backend/BE-KSK-002.md) |
| Gelombang | `MVP-0`, gelombang eksekusi 2 |
| Outcome | Lookup ke-11 dalam satu menit dari satu akun perangkat dijawab `429`; perangkat lain dan endpoint lain tidak terdampak |
| File | Diperbarui: `Program.cs` (`AddRateLimiter` policy `KioskPatientLookup`, `UseRateLimiter` setelah `UseAuthentication`/`UseAuthorization`), `appsettings.json` (`KioskPatientLookup:PermitPerMinute = 10`), `KioskPatientLookupController.cs` (`[EnableRateLimiting("KioskPatientLookup")]`) |
| Aturan wajib | Tanpa `GlobalLimiter`; partisi `ClaimTypes.NameIdentifier`, fallback IP; `QueueLimit = 0`; body `429` = `ApiResponse<object>.Fail(429, "Terlalu banyak percobaan. Silakan coba lagi sebentar.")` |

**Acceptance criteria**

1. 10 lookup berturut-turut dari akun perangkat A → seluruhnya `200`; yang ke-11 → `429` dengan body di atas.
2. Pada menit yang sama akun perangkat B → `200`.
3. Endpoint lain (misalnya `GET patients/kiosk/{id}`) dari akun A pada menit itu tetap `200`.
4. Setelah jendela 1 menit berlalu, akun A kembali `200`.
5. Mengubah `PermitPerMinute` menjadi `3` di konfigurasi → permintaan ke-4 `429`.

**Verifikasi:** build, verifier (exit `0`), runtime R0–R4 (dua akun perangkat samaran), `dotnet test` NOT RUN.

**DoD:** AC 1–5 terbukti di `task/report/backend/BE-KSK-002.md`.

### `BE-KSK-003` ⛔ — Route kiosk menerima Penjamin Perusahaan

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-3`; ⛔ menunggu `KSK-OQ-005` |
| Blocker | Amendment `RWI-ENC-PAYER-001` v1.0.0 → v1.1.0 belum tercatat di `rawat-inap/episode-rawat-inap/contracts/encounter-company-guarantor-contract.md` §7. Keputusan bisnisnya (`KSK-DEC-013`, Muhammad Hamzah) sudah `approved`; yang kurang hanya pencatatan formal di blueprint pemilik kontrak. Pemilik: Sukma / Muhammad Hamzah. |
| Yang tetap bisa jalan | `BE-KSK-001`, `BE-KSK-002`, seluruh task FE kecuali `FE-KSK-008` |
| Outcome | `POST patient-encounters/kiosk` dengan `paymentType = 3` + `patientCompanyGuarantorId` sah → `200` |
| File | Diperbarui: `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` — `CreateEncounterForKiosk` memanggil `CreateEncounterCoreAsync(..., allowCompanyGuarantor: true, ...)`; komentar merujuk `KSK-DEC-013` / `RWI-ENC-PAYER-001` v1.1.0 |
| Tidak termasuk | Route `/admin`; tipe pembayaran baru; perubahan validasi perusahaan |

**Acceptance criteria**

1. Kiosk: `paymentType = 3` + relasi perusahaan aktif/eligible/dalam masa berlaku → `200`; `RegPatientEncounter.PaymentType = 3`, `RegPatientEncounterGuarantor.PatientCompanyGuarantorId` terisi.
2. Relasi kedaluwarsa atau tidak eligible → `400` dengan pesan existing; tidak ada baris kunjungan tersimpan.
3. `paymentType = 3` tanpa `patientCompanyGuarantorId` → `400` "PatientCompanyGuarantorId wajib diisi…".
4. Regresi: Tunai dan Asuransi lewat route kiosk tetap `200`; route `/admin` tidak berubah.
5. Verifier exit `0`; `PatientEncounter.CreateEncounterForKiosk` tetap di himpunan fallback (nama method tidak berubah).

**Verifikasi:** build, EF (tidak ada perubahan), QBE, verifier, runtime R0–R4 dengan pasien samaran yang punya relasi perusahaan, `dotnet test` NOT RUN.

**DoD:** AC 1–5 terbukti di `task/report/backend/BE-KSK-003.md`; `KSK-OQ-005` tercatat tertutup.

## Coverage gap

| Requirement | Status |
| --- | --- |
| `FR-KSK-001..009` | Tercakup `BE-KSK-001/002` |
| `FR-KSK-030/031` | Tercakup `BE-KSK-003` |
| `NFR-002` (p95 < 500 ms di produksi) | **Gap** — tidak dapat dibuktikan di DB dev 16 pasien; diukur pasca-rilis oleh pemilik |
| Seluruh FR frontend | Di `frontend-roadmap.md` |
