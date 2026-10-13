# Laporan Task Backend — `RJ-DOC-REV-BE-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-017` |
| Judul | Skema rujukan + master Institusi/Dokter Perujuk |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Keputusan | `RJ-DOC-DEC-073`, `080`, `081`, `083`; desain revisi `31` |
| Kontrak | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Task mode | `CROSS-REPO MODE` — backend (`RJ-DOC-DEC-083`) |
| Branch / baseline | `sukmagp` @ `77caf434`, perubahan belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (rujukan), `HealthServices` / `MasterData` (perujuk) |
| Prefix registry | `Reg` — Registration, `ACTIVE / LEGACY`; `Mst` — Master / Reference / MasterData, `ACTIVE` |
| Keberlakuan | `NEW CODE` (3 entity `Reg*`, enum, service CRUD, endpoint baru); `TOUCHED LEGACY` (`MstReferralInstitution`, dua controller, `ReferralMasterDataService`, `ApplicationDbContext`) |
| QBE berlaku | QBE-ENT-001, NAM-001/002, CFG-001, MOD-001, SVC-001, API-001, PERM-001, LOG-001, VAL-001, DTO-001, PAGE-001, DEL-001, CODE-004 (unik kode institusi), AUD-001 |
| Jenis capability | Master data (perujuk) — standar sembilan endpoint; entity rujukan baru belum punya endpoint (dibuat `BE-018`/`BE-019`) |

## 2. Yang dikerjakan

1. **Skema rujukan.** `RegEncounterReferral`, `RegEncounterReferralDocument`, `RegEncounterReferralRevision` (warisan `IdentityModel`), enum `ReferralTargetUnitType`, `ReferralCaptureSource`, `ReferralRevisionType`, tiga `IEntityTypeConfiguration`, dan tiga `DbSet`. Unique `PatientEncounterId` difilter `IsDelete = false`; FK Restrict ke kunjungan, service unit, klinik, dan diagnosis; Cascade untuk surat dan revisi; `RowVersion` sebagai concurrency token; `jsonb` untuk nilai revisi.
2. **`MstReferralInstitution.IsPartner`** (`bool`, bawaan `false`).
3. **Master Institusi dan Dokter Perujuk.** Masing-masing sembilan endpoint standar (`filters/metadata`, `summary`, list, `options`, detail, create, update, `status`, delete), ditambah `kiosk/options` dengan policy `KioskRead`. Seluruh logika ada di `ReferralMasterDataService` (QBE-SVC-001); controller hanya memetakan hasil.
4. **Migration** `20261008093837_AddEncounterReferralAndReferralInstitutionPartner`: tiga tabel + satu kolom + index. Diterapkan ke `QuilvianNewDevSukma` lewat script idempoten satu migration (`dotnet ef migrations script <AQ> <PM-B> --idempotent`).

### Berkas

| Berkas | Status |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/ReferralTargetUnitType.cs`, `ReferralCaptureSource.cs`, `ReferralRevisionType.cs` | Baru |
| `Areas/HealthServices/RegistrationManagement/Models/RegEncounterReferral.cs`, `RegEncounterReferralDocument.cs`, `RegEncounterReferralRevision.cs` | Baru |
| `Repositories/Configurations/HealthServices/RegEncounterReferralConfiguration.cs` | Baru |
| `Repositories/ApplicationDbContext.cs` | Diperbarui — 3 `DbSet` |
| `Areas/HealthServices/MasterData/Models/MstReferralInstitution.cs` + `MstReferralInstitutionConfiguration.cs` | Diperbarui — `IsPartner` |
| `Areas/HealthServices/MasterData/DTOs/ReferralMasterDataDtos.cs` | Diperbarui — DTO CRUD, metadata, `isPartner` pada option |
| `Areas/HealthServices/MasterData/Services/ReferralMasterDataService.cs` | Diperbarui — CRUD |
| `Areas/HealthServices/MasterData/Controllers/ReferralInstitutionController.cs`, `ReferralDoctorController.cs` | Diperbarui — 9 endpoint + `kiosk/options` |
| `Migrations/20261008093837_AddEncounterReferralAndReferralInstitutionPartner.cs` (+ Designer, snapshot) | Baru |

## 3. Endpoint

#### Health Services / Master Data / Referral Institution

| Method | Path | Hak akses |
| --- | --- | --- |
| `GET` | `/api/v1/health-services/master-data/referral-institutions/filters/metadata` | `ReferralInstitution : Read` |
| `GET` | `…/summary` | `ReferralInstitution : Read` |
| `GET` | `…/` (filter `search`, `isActive`, `isPartner`) | `ReferralInstitution : Read` |
| `GET` | `…/options` (sudah ada; + `isPartner`) | `ReferralInstitution : Read` |
| `GET` | `…/kiosk/options` | Policy `KioskRead` |
| `GET` | `…/{id}` | `ReferralInstitution : Read` |
| `POST` | `…/` | `ReferralInstitution : Create` |
| `PUT` | `…/{id}` | `ReferralInstitution : Update` |
| `PATCH` | `…/{id}/status` | `ReferralInstitution : Update` |
| `DELETE` | `…/{id}` | `ReferralInstitution : Delete` |

#### Health Services / Master Data / Referral Doctor

Sama, base `/api/v1/health-services/master-data/referral-doctors`, resource `ReferralDoctor`; list menerima `referralInstitutionId`.

## 4. Delta terhadap kontrak dan standar

| Delta | Alasan |
| --- | --- |
| Kode institusi diisi pengguna, bukan dibuat backend | Kontrak yang disetujui (`api-contract` *PM-B*) menerima `institutionCode`; kode institusi perujuk lazim berupa kode faskes resmi. Sama dengan pola master terbaru `BloodStorageLocation`. Keunikan tetap dijaga index unik database (QBE-CODE-004). Menyimpang dari standar master data §2.6 — dicatat, bukan diam-diam |
| Hapus institusi ditolak `409` juga bila masih punya dokter perujuk (selain bila dipakai kunjungan) | Mencegah dokter yatim; pesan mengarahkan menghapus dokter atau menonaktifkan institusi |
| Nama dokter unik per institusi (`409`) | Mencegah dobel pilihan pada layar pendaftaran |
| `kiosk/options` tanpa `[AccessAction]`/`[AccessPermission]` | Mengikuti `KioskPatientLookupController`: policy `KioskRead` adalah otorisasi alternatif yang disetujui. Akun SuperAdmin juga lolos policy itu (R19) |

## 5. Verifikasi

| Perintah / skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false -o <scratchpad>` | 0 Error, 239 warning (tidak ada di berkas yang disentuh) | `PASS` |
| `dotnet ef migrations add … --configuration Release` | Migration hanya 3 tabel + 1 kolom + index; `Down()` lengkap | `PASS` |
| Script idempoten ke `QuilvianNewDevSukma` | `APPLIED`; history memuat `20261008093837_…`; 3 tabel ada; `IsPartner` default `false` | `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 17 berkas, VIOLATION 0, REVIEW 0 | `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test (pola Bank Darah) | — |
| Runtime HTTP ke backend uji `https://localhost:7185` | **21/21 PASS** | `PASS` |

Runtime (akun SuperAdmin + akun perangkat `kiosk-test`; kredensial tidak dicatat):

| ID | Skenario | Hasil |
| --- | --- | --- |
| R0 | Tanpa login → `401` | PASS |
| R1 | `filters/metadata` memuat field `isPartner` dan sort `isPartner` | PASS |
| R2 | Create institusi mitra; kode `pmtest-…` dinormalkan `PMTEST-RI-452613` | PASS |
| R3 | Kode ganda → `409` | PASS |
| R4 | Nama kosong → `400` | PASS |
| R5–R7 | List `isPartner=true`, summary `partnerReferralInstitution = 1`, `options` lama + `isPartner` | PASS |
| R8–R10 | Detail; `PUT` ubah `isPartner`; `PATCH status` nonaktif → hilang dari `options` | PASS |
| R11 | Dokter pada institusi nonaktif → `400` | PASS |
| R12–R15 | Create dokter; nama ganda → `409`; list disaring institusi; metadata + summary dokter | PASS |
| R16 | Hapus institusi yang masih punya dokter → `409` | PASS |
| R17–R18 | Akun Kiosk: `kiosk/options` institusi + dokter `200`; list admin → `403` | PASS |
| R19 | SuperAdmin ke `kiosk/options` → `200` (dicatat apa adanya) | PASS |
| R20 | Hapus dokter, lalu institusi → `200`; detail → `404` | PASS |

Data uji: institusi `PMTEST-RI-452613` dan satu dokter, keduanya sudah soft delete.

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Migration hanya menambah 3 tabel + 1 kolom; `Down()` bersih | Terpenuhi | Isi migration |
| 2. CRUD institusi/dokter sesuai kontrak; kode ganda `RJ-VAL-PM-16`; hapus institusi yang dipakai kunjungan `409` | Terpenuhi — cabang "dipakai kunjungan" terbukti lewat kode (belum ada kunjungan berujukan di DB uji); cabang "masih punya dokter" terbukti runtime (R16) | R2–R16 |
| 3. `options` lama kompatibel + `isPartner` | Terpenuhi | R7 |
| 4. `kiosk/options` akun Kiosk `200`, tanpa izin `403` | Terpenuhi — tanpa login `401` (R0 pola sama), akun Kiosk ke endpoint admin `403` (R18) | R17, R18 |
| 5. EF tanpa perubahan model tertunda | Terpenuhi | `has-pending-model-changes` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Dibuat dan diterapkan ke `QuilvianNewDevSukma` saja (`RJ-DOC-DEC-083`). Lingkungan lain belum |
| Risiko | Server dev Anda di 7184 belum memuat kode ini; restart diperlukan sebelum layar memakainya |
| Task berikutnya | `RJ-DOC-REV-BE-018`, `RJ-DOC-REV-BE-021`, `RJ-DOC-REV-FE-018` |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu: sesi terputus saat `ef migrations add` dengan build Debug (folder `bin/Debug` dikunci server 7184). Dilanjutkan dengan `--configuration Release`; tidak ada penyuntingan ganda |
