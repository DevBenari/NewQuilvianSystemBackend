# Roadmap Backend — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `6` — `approved` 19 Oktober 2026 (`PAT-DEC-069`–`077`). Riwayat: `5` (`PAT-DEC-068`), `4` (`PAT-DEC-067`), `3` (`PAT-DEC-033`), `2` untuk `BE-PAT-MIG-001` |
| `roadmap_revision` | **`6`** — 19 Oktober 2026: kartu task prasyarat `BE-PAT-MIG-003A` (`PAT-DEC-069`–`074`) ditambahkan atas instruksi tertulis pemilik; `BE-PAT-MIG-003` ✅ (deploy dan `PAT-GATE-005` generasi 1 `PASS`); `BE-PAT-MIG-002` tetap ⛔ dengan prasyarat baru dan amandemen kontrak `1.3.0` (`PAT-DEC-075`–`077`). Riwayat `5`: 16 Oktober 2026: `PAT-DEC-068` pada kolom Reuse dan rencana berkas `BE-PAT-MIG-003` (kunci dua bagian tetap). Riwayat `4`: 16 Oktober 2026: amandemen `PAT-DEC-067` pada `PAT-MIG3-AC-07`, `08`, `12`, `14`, `PAT-MIG2-AC-46`, dan `PAT-RSK-036`. Riwayat `3`: 14 Oktober 2026: menambah kartu `BE-PAT-MIG-002` berstatus ⛔ draft; diamandemen 15 Oktober 2026 dengan bukti Step 16 dan keputusan pemilik `PAT-DEC-034`–`043`; amandemen kedua 15 Oktober 2026 dengan keputusan `PAT-DEC-044`–`062` dan kartu usulan task prasyarat `BE-PAT-MIG-003`; **difinalkan 15 Oktober 2026, sesi ketiga**, dengan keputusan `PAT-DEC-063`–`066`, hasil `requirement-completeness-gate` `PASS`, dan persetujuan `PAT-DEC-033`. Riwayat: revision `1` (8 Oktober 2026) dengan butir `PAT-OQ-001`–`004` berstatus `PROPOSED`; revision `2` (8 Oktober 2026) mencatat keputusan final review dan `PAT-OQ-005`–`007` |
| Status roadmap | `BE-PAT-MIG-001`: **`approved`** — 8 Oktober 2026. `BE-PAT-MIG-003` dan `BE-PAT-MIG-002`: **`approved`** — kartu dan acceptance criteria disetujui 15 Oktober 2026 (`PAT-DEC-033`). `BE-PAT-MIG-003` **boleh dimulai** dalam sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend`. `BE-PAT-MIG-003A`: **`approved`** — 19 Oktober 2026 (instruksi pemilik, `PAT-DEC-069`). `BE-PAT-MIG-002` ⛔ sampai `BE-PAT-MIG-003A` ✅ dan `PAT-GATE-005` generasi 2 `PASS`; `BE-PAT-MIG-003` ✅ sudah terpenuhi. Persetujuan kartu **tidak** memberi wewenang *commit*, deployment, eksekusi database, pemanggilan endpoint, atau runtime Canary-A |
| Area / Module / Prefix | `HealthServices` / `PatientManagement` / `Pat` — registry `ACTIVE` |
| Snapshot source backend | `BE-PAT-MIG-001`: branch `QuilvianStaDeploy`, commit `103b45ccd5f0d9e2cbacff588540d8fe3d52e706`. `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`: `QuilvianIntegrationBackend` @ `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923`; `BE-PAT-MIG-003A`: `QuilvianIntegrationBackend` @ `c6537ee45dd774e6b77efd739f4410c6778a228e` (HEAD = `origin/QuilvianIntegrationBackend`, 19 Oktober 2026) — HEAD masih sama pada finalisasi 15 Oktober 2026 |
| Snapshot source frontend | Tidak ada task frontend (`PAT-DEC-015`, `PAT-DEC-060`). `QuilvianSystemFrontendDev` @ `2831ea635d83c0eca3f2cfbb0ae3be383e3ce204` dibaca read-only hanya untuk konsumen QR |
| Masukan | `BE-PAT-MIG-001`: `00-interview-decisions.md` revision `2`, `sha256:fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885`. `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`: `00-interview-decisions.md` revision `3` (`approved`) dan `requirement-completeness-assessment.md` revision `1` — hash pada manifest |
| Kontrak | `BE-PAT-MIG-001`: `contracts/api-contract.md` versi `1.1.0`, `sha256:cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e`. `BE-PAT-MIG-002`, `BE-PAT-MIG-003`, dan `BE-PAT-MIG-003A`: versi **`1.3.0`** `approved` (19 Oktober 2026; riwayat `1.2.2` 16 Oktober 2026) — Bagian B, B14, B15; hash pada manifest |
| Traceability | [requirement-traceability.md](requirement-traceability.md) |

**Catatan hash.** Revision `3` menambah bagian baru pada `00-interview-decisions.md` dan
`api-contract.md`, sehingga hash kedua berkas berubah. Isi yang dirujuk `BE-PAT-MIG-001` — bagian
1–5 keputusan dan bagian 1–9 kontrak — **tidak berubah**, kecuali baris kepala dokumen. Hash lama
di atas tetap sah sebagai bukti masukan `BE-PAT-MIG-001` pada saat task itu dibangun.

Hash dihitung dengan `sha256sum` terhadap berkas apa adanya di disk, cara yang sama dengan
manifest `rekam-medis`.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-PAT-MIG-001 ✅ ─────────────────────────┬─> BE-PAT-MIG-002 ⛔
                                           │
BE-PAT-MIG-003 ✅ ──> BE-PAT-MIG-003A 🟡 ──┤
                                           │
{PAT-GATE-005 generasi 2 ⛔} ──────────────┘
```

*Grafik revision `5` — jejak:* `BE-PAT-MIG-001 ✅`, `BE-PAT-MIG-003 🟡`, dan `{PAT-GATE-005 ⛔}` →
`BE-PAT-MIG-002 ⛔`. Node `BE-PAT-MIG-003A` dan generasi 2 gerbang ditambahkan 19 Oktober 2026 atas
instruksi pemilik (`PAT-DEC-069`); `PAT-GATE-005` generasi 1 sudah `PASS` (`PAT-FACT-090`).

`BE-PAT-MIG-002` menunggu empat hal:
- `BE-PAT-MIG-001` ✅, karena rekonsiliasi Pilot wajib lebih dulu (`PAT-DEC-010`) dan komponennya dipakai ulang;
- `BE-PAT-MIG-003` ✅, karena gerbang `NORMAL_ALLOCATION_GUARD` memakai pemeriksa baca-saja allocator milik task itu (kontrak B14.2);
- `BE-PAT-MIG-003A`, karena jalur pendaftaran bayi melewati lantai reservasi sampai task itu dideploy (`PAT-SRC-031`, `PAT-DEC-069`);
- `{PAT-GATE-005 generasi 2}` — deployment Staging dan verifikasi runtime `BE-PAT-MIG-003A`: pemeriksa
  membuktikan pendaftaran normal **dan** bayi terlindungi (`PAT-DEC-072`). Urutan wajib pemilik
  menempatkan implementasi `BE-PAT-MIG-002` **sesudah** guard dideploy dan diverifikasi di Staging
  (`00-interview-decisions.md` bagian 13.3 dan 15).

`{PAT-GATE-005}` adalah node gerbang, bukan task. Gerbang itu sendiri baru dapat diminta sesudah
`BE-PAT-MIG-003` ✅ untuk source dan direview leader, dengan otorisasi deployment terpisah.

`BE-PAT-MIG-003` tidak menunggu task lain. Persetujuan desain `PAT-DEC-033` sudah `approved`,
sehingga node `{PAT-DEC-033}` dari grafik amandemen kedua tidak digambar lagi.

Prasyarat data — tabel `migration.rsmmc_*` dan tabel cadangan di Staging — bukan task, sehingga
tidak digambar. Gerbang runtime Canary-A `PAT-GATE-002` terjadi sesudah `BE-PAT-MIG-002` ✅ dan tidak
digambar, karena bukan prasyarat build task mana pun.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-PAT-MIG-001` ✅ |
| 1 | — | `BE-PAT-MIG-003` ✅ |
| 2 | `BE-PAT-MIG-003` ✅ | `BE-PAT-MIG-003A` 🟡 |
| ⛔ menunggu `{PAT-GATE-005 generasi 2}` | `BE-PAT-MIG-001` ✅, `BE-PAT-MIG-003` ✅, `BE-PAT-MIG-003A`, `{PAT-GATE-005 generasi 2}` | `BE-PAT-MIG-002` |

*Grafik amandemen kedua — jejak:* `{PAT-DEC-033 ⛔}` → `BE-PAT-MIG-003 ⛔` → `BE-PAT-MIG-002 ⛔`,
ditambah `BE-PAT-MIG-001 ✅` → `BE-PAT-MIG-002`.

---

## Slice

| Slice | Hasil yang dapat diperiksa | Task | Status |
| --- | --- | --- | --- |
| **S1 — Mekanisme rekonsiliasi MRN Pilot** | Endpoint admin sementara yang dapat mensimulasikan dan menjalankan pemindahan MRN 715 pasien Pilot, lengkap dengan test terfokus | `BE-PAT-MIG-001` | ✅ selesai 9 Oktober 2026 untuk implementasi source — **SOURCE IMPLEMENTATION: APPROVED** (review source leader `APPROVED` 9 Oktober 2026); **RUNTIME: PENDING** — rekonsiliasi belum dijalankan, `PAT-GATE-001` tetap terbuka. Riwayat: 🟡 sebagian 9 Oktober 2026 — source diport ke baseline `QuilvianIntegrationBackend` @ `179aea3f` (branch `BE-PAT-MIG-001`); test `73/73`, build `0` error, QBE `Strict` `PASS`; menunggu review source oleh pengguna/leader. Rekonsiliasi runtime belum dijalankan (`PAT-GATE-001`) |

| **S2 — Worker migrasi massal pasien RSMMC, Canary-A** | Endpoint admin baru yang dapat mensimulasikan dan menjalankan pembuatan pasien dari target preview untuk manifest eksekusi Canary-A (`RSMMC-CANARY-A` v1, 25 `legacy_pid`, `PAT-DEC-056`), aman diulang dan dapat dilanjutkan | `BE-PAT-MIG-002` | ⛔ **`approved`** 15 Oktober 2026 — desain dikunci; ketiga isu P0 diputuskan. Menunggu `BE-PAT-MIG-003` ✅ dan `PAT-GATE-005` `PASS`. Belum ada source |
| **S3 — Guard reservasi nomor pasien normal** | Di Staging, pendaftaran pasien normal — rute biasa, kiosk, dan admin — memakai `PatientCode` ≥ 705.250 dan MRN ≥ `00-89-01-88`, sehingga tidak pernah memakai `PatientCode` 742–705.249 maupun MRN akhir `FINALIZED`; pemeriksa baca-saja tersedia untuk gerbang worker | `BE-PAT-MIG-003` | ✅ **SELESAI** 19 Oktober 2026 — source di-*merge* pemilik ke Integration (PR #290: `a9f2fda2` dokumen, `ab45dd3c` source, merge `3c457ab4`); dideploy ke DEV dan STAGING dengan `RsmmcMigrationReservationGuard__Enabled = true`, health `PASS`; **`PAT-GATE-005` generasi 1 `PASS`** (dilaporkan pemilik, `PAT-FACT-090`): `guardState = ACTIVE`, `PAT-RSMMC-705250`, `00-89-01-88`, `overallGuardReady = true` — memenuhi `PAT-MIG3-AC-12`. Bukti build 17 Oktober 2026: `dotnet build` `0` error; test `143` lulus / `0` gagal / `7` dilewati; PostgreSQL 15.15 provider-real `10`/`10` PASS (dijalankan pemilik); QBE `Strict` `PASS`; tanpa EF migration. **Catatan:** generasi 1 hanya membuktikan `CreatePatient`; jalur bayi `POST /newborns` (upstream `3a0189c2`) ditutup task terpisah `BE-PAT-MIG-003A`. *Riwayat: 🟡 siap review pemilik 17 Oktober 2026* [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| **S4 — Guard reservasi nomor pendaftaran bayi** | Di Staging persis, pendaftaran bayi `POST /newborns` memakai lantai dan kunci yang sama dengan pendaftaran normal atau gagal tertutup; pemeriksa `PAT-GATE-005` membuktikan kedua jalur | `BE-PAT-MIG-003A` | 🟡 **SOURCE SIAP — MENUNGGU BUKTI PROVIDER-REAL DAN REVIEW PEMILIK** 19 Oktober 2026 — di atas Integration `c6537ee4` (belum di-*commit*); `dotnet build -c Release` `0` error, `0` warning dari berkas yang diubah (298 warning lama di berkas lain); `dotnet test` PatientManagementTests `189` lulus / `0` gagal / `16` dilewati — `46` kasus baru `BE-PAT-MIG-003A` lulus, `70` regresi `BE-PAT-MIG-003` dan `73` regresi `BE-PAT-MIG-001` lulus, `16` dilewati = test provider-real (7 lama + 9 baru) tanpa server; QBE `Strict` `PASS`; `has-pending-model-changes` tanpa perubahan. **Belum:** uji PostgreSQL 15 provider-real `PAT-MIG3A-AC-09` (server tidak tersedia di sesi agent — dijalankan pemilik); review source; `PAT-MIG3A-AC-12` milik runtime `PAT-GATE-005` generasi 2. Laporan: [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |

Menjalankan rekonsiliasi pada data staging **bukan** bagian slice S1. Itu gerbang terpisah
`PAT-GATE-001`. Menjalankan Canary-A **bukan** bagian slice S2. Itu gerbang terpisah
`PAT-GATE-002`.

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-PAT-MIG-001` | 715 pasien Pilot V1 dapat dipindahkan dengan aman ke MRN kanonik RSMMC, termasuk QR baru, tanpa mengubah identitas maupun relasi | `PAT-DEC-001`–`PAT-DEC-016`; `PAT-OQ-001`–`PAT-OQ-007`; `PAT-MIG-AC-01`–`PAT-MIG-AC-28` | `api-contract.md` `1.1.0` | `PatientController` (route, tag, izin `Patient : Update`, logika QR dengan pemeriksaan sebelum helper); `IdentityModel`; pola project test di `Tests/` | Endpoint, service, DTO, dan test terfokus di bawah PatientManagement | — | 28 butir, lihat kartu | Lihat kartu | `PAT-RSK-001`–`PAT-RSK-008`; pemilik: pemilik/leader PatientManagement | Lihat kartu |
| `BE-PAT-MIG-002` ⛔ | Pasien RSMMC kanonik yang belum ada di Quilvian dapat dibuat persis dari target preview yang diaudit, aman diulang dan dapat dilanjutkan, dimulai dari manifest eksekusi `RSMMC-CANARY-A` v1 | `PAT-DEC-017`–`PAT-DEC-066`; `PAT-OQ-008`–`PAT-OQ-037` (0 `BLOCKING`); `PAT-GAP-009`–`016`; `PAT-MIG2-AC-01`–`PAT-MIG2-AC-51` (`AC-44`/`45` dipindah ke `BE-PAT-MIG-003`) | `api-contract.md` `1.2.2` `approved` — Bagian B, B15 | `RsmmcPilotEnvironmentGate`, `RsmmcPilotQrArtifactStore` dan factory QR `PatientController` apa adanya; pola kunci transaksi PostgreSQL; pola izin endpoint Bagian A; harness PostgreSQL provider-real dari `BE-PAT-MIG-003` | Endpoint, service, DTO, pembaca/penulis migrasi bertipe, manifest Canary-A, dan profil batch di bawah PatientManagement | `BE-PAT-MIG-001` ✅; `BE-PAT-MIG-003` ✅; `BE-PAT-MIG-003A`; gerbang `{PAT-GATE-005 generasi 2}` | 49 butir aktif dari 51 ID, lihat kartu | Lihat kartu | `PAT-RSK-009`–`PAT-RSK-038`; pemilik: pemilik/leader PatientManagement | Lihat kartu |
| `BE-PAT-MIG-003` ✅ | Di Staging, pendaftaran pasien normal tidak lagi dapat memakai nomor yang dicadangkan migrasi RSMMC; Produksi tidak berubah | `PAT-DEC-042`, `054`, `055`, `057`, `064`, `065`, `066`, `067`, `068`; `PAT-GAP-002`–`008`; `PAT-MIG3-AC-01`–`PAT-MIG3-AC-14` | `api-contract.md` `1.2.2` `approved` — B14–B14.3, B15; bentuk request/response pendaftaran normal tidak berubah, ditambah `503` khusus Staging dan endpoint pemeriksa baca-saja khusus Staging | Kunci transaksi PostgreSQL dua bagian tetap (`PAT-DEC-068`); transaksi `CreatePatient` yang sudah ada (`PAT-SRC-028`); satu titik `CreatePatient` untuk tiga rute (`PAT-SRC-023`) | Module service allocator, profil reservasi, pemeriksa baca-saja; perubahan terbatas pada `CreatePatient` | — | 14 butir, lihat kartu | Lihat kartu | `PAT-RSK-013`, `029`, `030`, `032`, `034`, `036`, `038`; pemilik: pemilik/leader PatientManagement | Lihat kartu |
| `BE-PAT-MIG-003A` 🟡 | Di Staging, pendaftaran bayi tidak lagi dapat memakai nomor yang dicadangkan migrasi RSMMC; di luar Staging persis `NumberSeries` bayi tidak berubah | `PAT-DEC-069`–`074`; `PAT-SRC-031`; `PAT-MIG3A-AC-01`–`PAT-MIG3A-AC-12` | `api-contract.md` `1.3.0` `approved` — B14.2 (aditif), B14.4, B15 | `PatientIdentifierAllocationService.AllocateReservedAsync` dan kunci `(PATI, NORM)` milik `BE-PAT-MIG-003`; transaksi dan kunci idempotency/kasus `NewbornPatientRegistrationService` yang sudah ada; harness provider-real | Cabang guard pada service bayi, method alokasi berlantai bersama, pemeriksa dua jalur | `BE-PAT-MIG-003` ✅ | 12 butir, lihat kartu | Lihat kartu | `PAT-RSK-039`–`042`; pemilik: pemilik/leader PatientManagement | Lihat kartu |

---

## Kartu task

### ✅ `BE-PAT-MIG-001` — RSMMC Pilot MRN Reconciliation

| Field | Isi |
| --- | --- |
| **Task ID** | `BE-PAT-MIG-001` |
| **Judul** | RSMMC Pilot MRN Reconciliation |
| **Status** | ✅ **SELESAI 9 Oktober 2026 untuk cakupan task ini — implementasi source.** **SOURCE IMPLEMENTATION: APPROVED** — review source leader `APPROVED` 9 Oktober 2026 atas port di branch `BE-PAT-MIG-001` (`QuilvianIntegrationBackend` @ `179aea3f`); 28 dari 28 acceptance criteria terpetakan ke source dan terverifikasi (test `73/73`, build `0` error, QBE `Strict` *working tree* `PASS`); seluruh butir DoD fase implementasi terpenuhi. **RUNTIME: PENDING** — rekonsiliasi **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION**; `PAT-GATE-001` tetap **terbuka**; tanda ini **bukan** berarti 715 pasien sudah direkonsiliasi, dan migrasi RSMMC secara keseluruhan belum selesai. Bukti: [laporan bagian 10](../task/report/backend/BE-PAT-MIG-001.md). **Riwayat 🟡 SEBAGIAN 9 Oktober 2026, sesudah port semantik ke Integration.** Implementasi yang sudah direview dipindahkan maknanya ke branch `BE-PAT-MIG-001` di atas `QuilvianIntegrationBackend` @ `179aea3f`: enam berkas source baru identik dengan rujukan, kontrak SQL `IDENTICAL`, perubahan `PatientController`/`Program.cs`/`.gitignore` sama maknanya tanpa menimpa perubahan Integration. Validasi ulang di branch itu: `dotnet test` project `QuilvianSystemBackend.PatientManagementTests` `73` lulus, `0` gagal, `0` dilewati (72 test rujukan + 1 test delta `PatientQrPayloadBuilder`); `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)` — `256 Warning(s)`, nol dari berkas baru; QBE `Strict` *working tree* 16 berkas `VIOLATION: 0`, `PASS`. Butir DoD "source sudah direview pengguna/leader" **belum terpenuhi**. Rekonsiliasi runtime **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION** (`PAT-GATE-001`). Bukti: [laporan bagian 9](../task/report/backend/BE-PAT-MIG-001.md). **Riwayat 8 Oktober 2026, sesudah koreksi owner review:** ke-28 acceptance criteria terpetakan ke source dan test. Dua koreksi pemilik selesai: bukti kepemilikan QR memakai penanda + panjang + SHA-256 dan *commit* tidak pasti tidak pernah menghapus QR; positif palsu `QBE-CODE-002` ditutup dengan rename `GenerateQrCodePngBytes` → `RenderQrCodePngBytes`. `dotnet test` project `QuilvianSystemBackend.PatientManagementTests` `72` lulus, `0` gagal, `0` dilewati; `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)` — `242 Warning(s)`, nol dari berkas baru; QBE `Strict` *working tree* `VIOLATION: 0`, `PASS`. Butir DoD "source sudah direview pengguna/leader" **belum terpenuhi**. Rekonsiliasi runtime **NOT EXECUTED** — menunggu `PAT-GATE-001`. Riwayat: disetujui pemilik 8 Oktober 2026 sebagai `SIAP DIKERJAKAN`; pengerjaan pertama 66/66 test dengan satu positif palsu QBE `ReportOnly`. Bukti: [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| **Klasifikasi** | `LEGACY MIGRATION` — migrasi data pasien lama. Lihat `00-interview-decisions.md` bagian 5 tentang perbedaannya dengan arti `LEGACY MIGRATION` pada kontrak QBE |
| **Area / Module / Prefix** | `HealthServices` / `PatientManagement` / `Pat` |
| **Outcome** | 715 pasien Pilot V1 yang sudah ada dapat dipindahkan dengan aman dari MRN sementara buatan Quilvian ke rencana MRN RSMMC berstatus `FINALIZED`, termasuk pembuatan QR baru, sambil mempertahankan identitas pasien dan seluruh relasi yang ada |
| **Asal** | Pilot V1 memakai API pembuatan pasien biasa, sehingga MRN 715 pasien dibuat Quilvian dan tidak cocok dengan rencana kanonik. MRN itu dibutuhkan pasien RSMMC lain, sehingga rekonsiliasi wajib mendahului migrasi sisa pasien (`PAT-DEC-010`) |
| **Requirement/decision** | `PAT-DEC-001` sampai `PAT-DEC-016`; keputusan review `PAT-OQ-001` sampai `PAT-OQ-007`; requirement `PAT-MIG-AC-01` sampai `PAT-MIG-AC-28` |
| **Kontrak** | [`contracts/api-contract.md`](../contracts/api-contract.md) versi `1.1.0` — `approved` final; endpoint berstatus **Rencana (belum tersedia)** |
| **Reuse** | `REUSE`: base route dan `[Tags]` `PatientController.cs` baris 42 dan 52; konvensi izin `Patient : Update` baris 771–778 dan 912–919; `IdentityModel` untuk field audit. `REUSE WITH ADAPTER`: `SavePatientQrCodeFile` baris 1046–1172, karena baris 1144 menimpa file diam-diam dan bertentangan dengan `AC-13`. Path QR tujuan wajib diperiksa **sebelum** helper ini dipanggil; bila artefaknya sudah ada, helper tidak dipanggil (`PAT-OQ-005`). `REUSE`: `BuildPatientQrPayload` baris 1386–1401 untuk isi QR. Pola project test: `Tests/QuilvianSystemBackend.PharmacyTests/` (`PAT-OQ-001`) |
| **Larangan pakai ulang** | `GenerateMedicalRecordNumberAsync` baris 1403 (`AC-16`); `GeneratePatientCodeAsync` baris 2468 (`AC-17`); pola `!environment.IsProduction()` pada `Services/Security/AccessPermissionService.cs` baris 38 sebagai satu-satunya pemeriksaan, karena gagal terbuka dan melanggar `AC-05` (`PAT-OQ-006`); `IHostEnvironment.IsStaging()` saja, karena tidak membedakan huruf besar-kecil sedangkan `PAT-OQ-002` menuntut nama persis `Staging` |
| **Cakupan** | Satu endpoint `POST /admin/migration/rsmmc-pilot-mrn/reconcile` di bawah base route Patient; service rekonsiliasi beserta DTO request/response di dalam `Areas/HealthServices/PatientManagement/`; pemeriksaan path QR tujuan sebelum helper QR dipanggil; gerbang environment berbasis daftar izin positif berisi `Staging` saja; test terfokus sesuai `AC-27` di project xUnit khusus `Tests/QuilvianSystemBackend.PatientManagementTests/`, tanpa entri `.sln` (`PAT-OQ-001`). Penempatan persis berkas source mengikuti pola terdekat dan diputuskan saat build |
| **Governance guard test** | Sebelum membuat project test, agent build memeriksa ulang `rules/backend/TEST_POLICY.md` dan aturan `build-module-backend`. Penilaian perencanaan: **diizinkan** oleh `TEST_POLICY` bagian 3 (`00-interview-decisions.md` bagian 4.1). Bila pemeriksaan saat build menyatakan sebaliknya: **jangan di-*bypass***, hentikan bagian pembuatan test, laporkan aturan persis yang memblokir beserta alternatif canonical, lalu minta keputusan. Dilarang menaruh test ini di `PharmacyTests`, `NutritionTests`, atau `OperatingRoomTests`. Bagian source endpoint tidak ikut berhenti |
| **Di luar cakupan** | Membuat atau menerapkan EF migration; perintah migration database; SQL langsung yang mengubah data; memanggil endpoint; menjalankan rekonsiliasi 715 pasien; deployment; eksekusi produksi; 704.508 pasien yang belum dibuat (`AC-23`); perubahan pada tabel `migration.rsmmc_*`; menghapus QR lama; frontend; `appsettings*`; `Program.cs` kecuali registrasi dependency yang terbukti dibutuhkan dan dicatat |
| **Dependency** | Tidak ada task prasyarat. **Prasyarat data, bukan task, dilaporkan pengguna dan tidak diverifikasi agent:** tabel `migration.rsmmc_patient_mrn_plan`, `migration.rsmmc_patient_crosswalk`, dan `migration.rsmmc_patient_pilot_reconcile_backup` sudah ada di staging dengan isi sesuai `PAT-FACT-001`–`PAT-FACT-026` |
| **Batas wewenang database/migration** | Task ini **hanya** memberi wewenang implementasi source (`PAT-DEC-012`). Tidak ada pembuatan EF migration, penerapan EF migration, eksekusi database, SQL langsung, pemanggilan endpoint, eksekusi rekonsiliasi, deployment, atau eksekusi produksi. Tabel `migration.rsmmc_*` belum dipetakan di source, sehingga pembacaannya wajib tidak menimbulkan kebutuhan EF migration dan tidak mengubah skema (`PAT-OQ-007`). Mekanisme bacanya dipilih saat build dengan nilai masukan berparameter. Eksekusi runtime menunggu `PAT-GATE-001` |
| **Keputusan yang melekat** | MRN tujuan hanya dari perencana (`PAT-DEC-005`). `Id`, `PatientCode`, dan relasi tetap (`PAT-DEC-007`). QR lama tidak dihapus (`PAT-DEC-008`). Satu pasien satu transaksi; berhenti pada kegagalan tak terduga pertama (`PAT-DEC-011`). Konflik MRN tujuan dan konflik artefak QR tujuan diperlakukan sebagai kegagalan tak terduga yang menghentikan proses, karena audit mencatat nol konflik (`api-contract.md` bagian 4.2); konflik artefak QR dilaporkan berstatus `CONFLICT` (`PAT-OQ-005`). Hanya environment bernama persis `Staging` yang diizinkan, untuk kedua mode (`PAT-OQ-002`, `PAT-OQ-006`). `dryRun` yang tidak dikirim berarti `true` (`PAT-OQ-003`). `limit` hanya menghitung pasien layak; `ALREADY_RECONCILED` dan `QR_INCONSISTENT` dilaporkan untuk seluruh set Pilot, tidak dihitung dalam `limit`, dan `QR_INCONSISTENT` tidak diperbaiki (`PAT-OQ-004`) |
| **QBE** | QBE preflight dan kesesuaian engineering **diselesaikan saat build**, dari `AGENTS.md` backend dan `docs/engineering/`. Perkiraan awal: berkas baru berstatus `NEW CODE`, dan `PatientController` menjadi `TOUCHED LEGACY` bila ikut diubah. `QBE-NAM-003`, `QBE-DB-001`, dan `QBE-DB-002` mengatur *rename* dan kemungkinan besar tidak berlaku, karena task ini tidak me-*rename* apa pun. Ini dipastikan pada preflight build |
| **Laporan** | [`../task/report/backend/BE-PAT-MIG-001.md`](../task/report/backend/BE-PAT-MIG-001.md) — ditulis `build-module-backend` 8 Oktober 2026; diperbarui 9 Oktober 2026 dengan bukti port ke Integration (bagian 9) |

#### Acceptance criteria

1. **`AC-01`** — Rekonsiliasi hanya memilih baris yang memenuhi seluruh syarat berikut:
   `batch_id` perencana sama dengan permintaan; `plan_status` perencana = `FINALIZED`;
   `legacy_pid` crosswalk sama dengan `legacy_pid` perencana; `quilvian_patient_id` crosswalk
   terisi; `MstPatient.Id` sama dengan `quilvian_patient_id`; dan `MstPatient.MedicalRecordNumber`
   sekarang berbeda dengan `planner.final_medical_record_number`.
2. **`AC-02`** — MRN tujuan kanonik hanya diambil dari
   `rsmmc_patient_mrn_plan.final_medical_record_number`. `crosswalk.normalized_mrn` tidak boleh
   dipakai sebagai MRN tujuan.
3. **`AC-03`** — `dryRun=true` tidak melakukan satu pun perubahan database permanen dan tidak
   menulis satu pun file QR.
4. **`AC-04`** — `dryRun` mengembalikan baris yang deterministik, diurutkan menurut `legacy_pid`,
   dan minimal memuat: `legacyPid`, `patientId`, `PatientCode`, `FullName`, MRN sekarang, MRN
   akhir, path QR sekarang, path QR rencana, dan status.
5. **`AC-05`** — `dryRun=false` hanya diizinkan di environment yang terbukti bukan produksi.
   Eksekusi di produksi selalu ditolak. Environment yang tidak dikenal wajib ditolak (*fail
   closed*).
6. **`AC-06`** — Endpoint wajib login, dan memakai konvensi otorisasi Patient Update yang sudah
   ada atau izin lain yang sudah ada dan setara atau lebih ketat.
7. **`AC-07`** — Untuk setiap pasien yang direkonsiliasi, yang dipertahankan: `MstPatient.Id`,
   `PatientCode`, seluruh data demografi, seluruh relasi/FK yang ada, `CreateDateTime`, dan
   `CreateBy`.
8. **`AC-08`** — Hanya field rekonsiliasi berikut yang boleh berubah: `MedicalRecordNumber`,
   `QrCodePath`, `UpdateDateTime`, dan `UpdateBy`.
9. **`AC-09`** — QR baru wajib dibuat dari MRN akhir kanonik. Isi QR serta path/folder-nya wajib
   sesuai MRN akhir itu.
10. **`AC-10`** — File/folder QR Pilot lama wajib tidak tersentuh selama rekonsiliasi.
11. **`AC-11`** — Bila pembuatan QR gagal, `MstPatient` wajib tetap tidak berubah.
12. **`AC-12`** — Bila `SaveChanges`/*commit* database gagal sesudah QR baru dibuat, hanya artefak
    QR yang baru dibuat yang boleh dibersihkan secara *best-effort*. QR lama tidak boleh dihapus.
13. **`AC-13`** — Artefak QR tujuan yang tak terduga sudah ada untuk MRN akhir tidak boleh
    ditimpa diam-diam. Keadaan itu wajib menghasilkan konflik yang aman atau penanganan eksplisit
    yang setara.
14. **`AC-14`** — Operasi wajib dapat dilanjutkan dan idempoten, artinya aman dipanggil berulang
    tanpa hasil ganda. Bila MRN sudah kanonik, pasien diklasifikasikan `ALREADY_RECONCILED`.
15. **`AC-15`** — Bila MRN sudah kanonik tetapi `QrCodePath` tidak sesuai MRN kanonik, laporkan
    `QR_INCONSISTENT`, bukan menebak perbaikannya diam-diam.
16. **`AC-16`** — Pembangkit MRN normal tidak boleh dipanggil untuk pasien Pilot ini.
17. **`AC-17`** — Tidak boleh ada `PatientCode` baru yang dibangkitkan.
18. **`AC-18`** — Tidak boleh ada `MstPatient` baru yang dibuat untuk 715 pasien Pilot.
19. **`AC-19`** — Tidak boleh ada `MstPatient` yang dihapus lalu dibuat ulang.
20. **`AC-20`** — Crosswalk tidak boleh diubah.
21. **`AC-21`** — Tabel perencana MRN kanonik tidak boleh diubah.
22. **`AC-22`** — Tabel cadangan *rollback* tidak boleh diubah.
23. **`AC-23`** — 704.508 pasien yang belum dibuat berada di luar cakupan task ini.
24. **`AC-24`** — Eksekusi dibatasi paling banyak `request.limit` baris, dengan urutan
    `legacy_pid` yang deterministik.
25. **`AC-25`** — Pemrosesan memakai satu transaksi per pasien, sehingga kegagalan tidak
    membatalkan pasien yang sudah selesai sebelumnya.
26. **`AC-26`** — Pada perilaku eksekusi awal, proses berhenti pada kegagalan tak terduga yang
    pertama, lalu mengembalikan/melaporkan pasien yang gagal secara aman.
27. **`AC-27`** — Test terfokus wajib mencakup: *dry run*; gerbang otorisasi/environment;
    validasi `limit`; konflik MRN; pelestarian identitas; pelestarian `PatientCode`; perilaku QR;
    idempotensi; kegagalan QR; kegagalan database sesudah QR; keutuhan crosswalk; keutuhan
    perencana; keutuhan cadangan; serta perilaku `limit` dan urutan yang deterministik.
28. **`AC-28`** — Validasi build wajib mencakup `dotnet build -p:RunAnalyzers=false` dan test
    terfokus yang sesuai.

Penomoran `AC-xx` di atas adalah penomoran pengguna. Pada traceability, butir yang sama ditulis
`PAT-MIG-AC-xx` supaya unik di seluruh repository.

#### Verifikasi

| No | Bukti | Bentuk yang dicatat pada laporan task |
| ---: | --- | --- |
| V1 | Kepatuhan governance dan aturan | QBE preflight: Area, Module, prefix, kelas keberlakuan, dan QBE ID yang benar-benar berlaku; kecocokan `[AccessAction]` dan `[AccessPermission]` |
| V2 | Inspeksi source terhadap `QuilvianStaDeploy` terkini | SHA saat build; bila berbeda dari `103b45cc`, kutipan baris pada kontrak diperiksa ulang lebih dulu |
| V3 | Test terfokus lulus | Perintah test yang dijalankan beserta jumlah lulus/gagal apa adanya, untuk seluruh topik `AC-27` |
| V4 | Build backend lulus | Keluaran `dotnet build -p:RunAnalyzers=false`, jumlah error dan warning apa adanya |
| V5 | Inspeksi `git diff` | Daftar berkas berubah beserta alasannya; tidak ada perubahan di luar cakupan |
| V6 | Tidak ada artefak QR masuk Git | `git status --short` tidak memuat berkas di bawah `patient-qrcodes/` maupun file `.png` hasil runtime |
| V7 | Tidak ada eksekusi database selama implementasi | Pernyataan eksplisit: tidak ada EF migration dibuat/diterapkan, tidak ada perintah database |
| V8 | Tidak ada pemanggilan endpoint selama implementasi | Pernyataan eksplisit pada laporan |
| V9 | Tidak ada commit, push, atau deploy tanpa otorisasi terpisah | `git status --short` dan pernyataan eksplisit pada laporan |
| V10 | Verifikasi kontrak API dan proses bisnis | Method, path, request/response, kode status, dan izin yang benar-benar terpasang; penelusuran langkah `api-contract.md` bagian 4.2 pada source, termasuk default `dryRun=true`, pencocokan persis `Staging`, dan pemeriksaan QR sebelum helper |
| V11 | Model EF tidak berubah (`PAT-OQ-007`) | Review diff: tidak ada berkas di `Migrations/` yang berubah, dan tidak ada pemetaan ke `migration.rsmmc_*` yang membuat model menuntut EF migration. Perintah EF tooling apa pun, termasuk yang hanya memeriksa, tidak dijalankan tanpa izin eksplisit |

Baris status automated test pada laporan ditulis sebagai `AUTOMATED TEST: <perintah> — PASS/FAIL`,
karena pemilik memintanya secara eksplisit (`PAT-DEC-014`).

#### Definition of Done — fase implementasi

- [x] Endpoint diimplementasikan sesuai kontrak `1.1.0`
- [x] Test terfokus lulus
- [x] Build backend lulus
- [x] Tidak ada perubahan berkas yang tidak dimaksud
- [x] Laporan task implementasi lengkap di `../task/report/backend/BE-PAT-MIG-001.md`
- [x] Source sudah direview pengguna/leader
- [x] **Belum ada** rekonsiliasi runtime yang dijalankan

Butir "source sudah direview pengguna/leader" adalah tindakan manusia. Selama butir itu belum
terpenuhi, task paling tinggi berstatus 🟡, walaupun build dan test sudah lulus.

---

### ⛔ `BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient Migration Worker

> **Kartu ini `approved`** — 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`). Cakupan, rencana berkas,
> acceptance criteria, dan Definition of Done di bawah dikunci. Task ini **belum boleh dimulai**
> sampai `BE-PAT-MIG-003` ✅, `BE-PAT-MIG-003A` ✅, dan `PAT-GATE-005` generasi 2 `PASS` (urutan wajib
> pemilik, `00-interview-decisions.md` bagian 13.3 dan 15; `PAT-DEC-069`). Desainnya diamandemen kontrak
> `1.3.0`: `PERSIST_IS_MEMBER` dihapus (`PAT-DEC-075`), lima kolom baru `NULL` (`PAT-DEC-076`), dan
> `PATIENT_BASELINE` memakai baseline tetap (`PAT-DEC-077`).
>
> *Banner lama — jejak:* "Kartu ini `draft` dan belum disetujui … WAITING FOR OWNER APPROVAL —
> BE-PAT-MIG-002 DESIGN".

| Field | Isi |
| --- | --- |
| **Task ID** | `BE-PAT-MIG-002` |
| **Judul** | RSMMC Resumable Bulk Patient Migration Worker |
| **Status** | ⛔ **`approved` — menunggu prasyarat.** Desain dikunci 15 Oktober 2026 (`PAT-DEC-033`); 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, 0 `BLOCKING`. Penghalang mulai build: `BE-PAT-MIG-003` ✅ (terpenuhi 19 Oktober 2026), `BE-PAT-MIG-003A` ✅, dan `PAT-GATE-005` generasi 2 `PASS` (`PAT-DEC-069`). Impact scan 19 Oktober 2026: `BLOCKED` (`PAT-FACT-091`). Pemilik: pemilik/leader PatientManagement. Belum ada source. *Riwayat: draft terblokir sampai sesi kedua — menunggu `PAT-OQ-019`, `031`, `036`, `037` dan `PAT-DEC-033`* |
| **Klasifikasi bisnis** | `LEGACY MIGRATION` — migrasi data pasien (`PAT-DEC-017`; arti istilah pada `00-interview-decisions.md` bagian 5) |
| **Perkiraan klasifikasi build** | `HEAVY` — perkiraan skor 10: berkas diperiksa > 20 (2), berkas diubah > 8 bila test diminta (2), logika bisnis kompleks (2), menambah kontrak API (2), penulisan persistence lewat SQL berparameter (1), berkaitan dengan keamanan (1), satu repository (0), tanpa UI (0). Ditetapkan ulang saat build |
| **Area / Module / Prefix** | `HealthServices` / `PatientManagement` / `Pat`, Submodule `MasterData` — registry `ACTIVE` baris 24. Tidak ada entity persisted baru, sehingga tidak ada kebutuhan registry baru |
| **Outcome** | Pasien RSMMC kanonik yang belum ada di Quilvian dapat dibuat persis dari `migration.rsmmc_patient_target_preview` batch kanonik — dengan `PatientCode` preview, MRN akhir perencana, dan QR baru — lewat endpoint admin khusus Staging yang aman diulang, dapat dilanjutkan, dan pada build pertama hanya memproses manifest eksekusi Canary-A — `RSMMC-CANARY-A` v1, 25 pasien dalam urutan yang disetujui (`PAT-DEC-056`) |
| **Asal** | Rekonsiliasi Pilot `CLOSED/PASS` (`PAT-GATE-001`, dilaporkan pemilik). 704.506 pasien siap dan sudah diaudit pada target preview (`PAT-FACT-040`–`049`). API pembuatan pasien normal tidak dapat dipakai karena membangkitkan kode dan MRN sendiri (`PAT-SRC-006`, `PAT-SRC-016`) |
| **Requirement/decision** | `PAT-DEC-017`–`PAT-DEC-066`; pertanyaan `PAT-OQ-008`–`PAT-OQ-037`, seluruhnya diputuskan atau `SUPERSEDED` (`00-interview-decisions.md` bagian 10.1); usulan standar `PAT-GAP-009`–`016` (`requirement-completeness-assessment.md`); requirement `PAT-MIG2-AC-01`–`PAT-MIG2-AC-51`, dengan `AC-44` dan `AC-45` dipindah ke `BE-PAT-MIG-003` |
| **Kontrak** | [`contracts/api-contract.md`](../contracts/api-contract.md) `1.2.2` — **`approved`**: Bagian B dan B15; endpoint **Rencana (belum tersedia)** |
| **Dependency** | Task: `BE-PAT-MIG-001` ✅, `BE-PAT-MIG-003A` — guard jalur bayi (`PAT-DEC-069`), dan `BE-PAT-MIG-003` ✅ — pemeriksa allocator untuk `NORMAL_ALLOCATION_GUARD` (kontrak B14.2) dan harness PostgreSQL provider-real. Gerbang: `{PAT-GATE-005}` — urutan wajib pemilik. Keputusan `PAT-DEC-033`: `approved`. **Prasyarat data, bukan task, dilaporkan pemilik dan tidak diverifikasi agent:** `PAT-FACT-030`–`PAT-FACT-089`. Prasyarat runtime: `PAT-GATE-001` `CLOSED/PASS`; `PAT-GATE-005` `PASS` |
| **Batas wewenang database/migration** | Task ini kelak **hanya** memberi wewenang implementasi source. Tidak ada pembuatan atau penerapan EF migration, perubahan skema, eksekusi database, SQL langsung di luar kode aplikasi, pemanggilan endpoint, pembuatan QR sungguhan, deployment, maupun eksekusi produksi. Tabel `migration.rsmmc_*` tetap tidak dipetakan ke model EF. Eksekusi runtime menunggu `PAT-GATE-002` |
| **Keputusan yang melekat** | Preview adalah snapshot masukan yang disetujui; kode dan MRN tidak dibangkitkan (`PAT-DEC-020`, `PAT-DEC-021`). Identitas migrasi `legacy_pid`, tanpa pencocokan NIK (`PAT-DEC-022`). Satu pasien satu transaksi, kunci database per pasien, baca ulang sesudah kunci (`PAT-DEC-023`). Keselamatan QR mengikuti `BE-PAT-MIG-001` (`PAT-DEC-024`). Manifest Canary-A beku, tanpa penggantian (`PAT-DEC-026`). Canary-B dan 285 baris historis di luar build ini (`PAT-DEC-027`, `PAT-DEC-031`). Endpoint baru terpisah (`PAT-DEC-028`). Ditambah 15 Oktober 2026: provenance `MIGRATED_BULK_WORKER` (`PAT-DEC-034`); preview immutable (`PAT-DEC-035`); eksekusi Canary-A berhenti pada `CONFLICT`/`FAILED` tak terduga pertama (`PAT-DEC-036`); test wajib (`PAT-DEC-037`); `legacy_pid` Int64 (`PAT-DEC-038`); Pilot dan QR lama immutable byte demi byte (`PAT-DEC-039`); daftar 25 hanya kandidat (`PAT-DEC-040`); `IsMember = false` wajib tersimpan (`PAT-DEC-041`); perlindungan reservasi kode dan MRN wajib tahan lama (`PAT-DEC-042`); tabrakan QR adalah P0 dan tidak boleh diselesaikan dengan adopsi diam-diam (`PAT-DEC-043`). Catatan: "manifest Canary-A beku" pada `PAT-DEC-026` kini kandidat draft. Ditambah amandemen kedua 15 Oktober 2026: route dan request (`PAT-DEC-044`); `CANARY_A` saja, `limit` 1–25, urutan manifest tanpa sort ulang (`PAT-DEC-045`); simulasi membaca filesystem tanpa menulis (`PAT-DEC-046`); kolom crosswalk dari preview, `normalized_nik` hanya bukti (`PAT-DEC-047`); kunci dua bagian dan `CONCURRENT_RUN` (`PAT-DEC-048`); data pribadi (`PAT-DEC-049`); `PHONE_ALREADY_USED` (`PAT-DEC-050`); `TARGET_CREATED` (`PAT-DEC-051`); path QR `rsmmc-bulk` untuk seluruh pasien (`PAT-DEC-052`); persistensi `IsMember` dengan bukti provider-real (`PAT-DEC-053`); gerbang menuntut `BE-PAT-MIG-003` aktif dan terverifikasi (`PAT-DEC-055`); manifest eksekusi = 25 kandidat dalam urutan yang sama (`PAT-DEC-056`). Ditambah sesi ketiga 15 Oktober 2026: SQL bertipe sesuai skema fisik, tanpa fallback `::text` (`PAT-DEC-063`); lantai MRN normal 890.188 yang menjadi syarat `NORMAL_ALLOCATION_GUARD` (`PAT-DEC-064`); bukti provider-real PostgreSQL 15 sekali pakai (`PAT-DEC-065`); guard hanya di `Staging` persis dengan `Enabled == true` (`PAT-DEC-066`) |
| **QBE** | QBE preflight dan kesesuaian engineering **diselesaikan saat build**, dari `AGENTS.md` backend dan `docs/engineering/`. Perkiraan: berkas baru `NEW CODE`; `PatientController.cs` dan `Program.cs` `TOUCHED LEGACY`. ID yang kemungkinan berlaku: `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-AUD-001`, `QBE-CODE-003`. `QBE-CODE-001`/`004` kemungkinan tidak berlaku karena tidak ada alokasi nomor bisnis. Hindari nama method berpola `Generate…Code`/`Generate…Number` pada kode baru — pola itu memicu positif palsu `QBE-CODE-002` pada `BE-PAT-MIG-001` |
| **Laporan** | `../task/report/backend/BE-PAT-MIG-002.md` — dicadangkan, **belum dibuat**. Ditulis `build-module-backend` sesudah build dan validasi |

#### Komponen yang dipakai ulang

| Komponen | Keputusan | Alasan |
| --- | --- | --- |
| `RsmmcPilotEnvironmentGate` | `REUSE` apa adanya | Sudah memenuhi `Staging` persis, ordinal, gagal tertutup (`PAT-SRC-004`) |
| `RsmmcPilotQrArtifactStore`, `RsmmcPilotCreatedQrArtifact`, `TryDeleteCreated` | `REUSE` apa adanya — dengan layout folder dasar terpisah `patient-qrcodes/rsmmc-bulk` (`PAT-DEC-052`) | `CreateNew` dan bukti kepemilikan penanda + panjang + SHA-256 sudah sesuai `PAT-DEC-024` (`PAT-SRC-005`); folder dasar diambil dari layout (`PAT-SRC-022`) |
| Factory `CreateRsmmcPilotQrArtifactStore()` beserta `PatientQrPayloadBuilder`, `SanitizePathSegment`, `CombineUrlPath`, `RenderQrCodePngBytes` | `REUSE` lewat factory | Isi, folder, dan path QR satu sumber dengan pembuatan pasien |
| Base route, `[Tags]`, `[Authorize]`, `[AccessController]` `PatientController` | `REUSE` | `PAT-SRC-002` |
| Pola izin `[AccessAction("Update", …)]` + `[AccessPermission("Patient", "Update")]` | `REUSE` | `PAT-SRC-003` |
| Pola kunci transaksi PostgreSQL dengan pemeriksaan Npgsql | `REUSE WITH ADAPTER` — bentuk dua kunci `pg_try_advisory_xact_lock(key1, key2)`, bukan `hashtext` satu kunci (`PAT-DEC-048`) | `PAT-SRC-014`, `PAT-SRC-026` |
| Pola `Database.SqlQuery<T>` + `FormattableString` berparameter | `REUSE WITH ADAPTER` | Parameter bertipe `bigint`/`uuid` supaya indeks terpakai, bukan cast `::text` (`PAT-SRC-010`) |
| Struktur transaksi per pasien dan perlakuan `COMMIT_OUTCOME_UNCERTAIN` pada service Pilot | `REUSE` pola, bukan class | Semantik yang sudah direview leader |
| `MstPatient`, `MstPatientConfiguration`, `IdentityModel`, enum pasien | `REUSE` apa adanya — **tidak diubah** | `PAT-SRC-009`; `PAT-OQ-017` pilihan A tidak menyentuhnya |
| `LoggerService` | `REUSE` | Pola log service Pilot |
| `Tests/QuilvianSystemBackend.PatientManagementTests/` beserta `TestDatabase` | `REUSE` — **wajib** (`PAT-DEC-037`) | `PAT-SRC-018`. `TestDatabase` membentuk skema dari konfigurasi EF, sehingga default `IsMember = true` ikut ada dan cacat `PAT-SRC-008` dapat direproduksi |
| Harness PostgreSQL 15 sekali pakai yang dibangun `BE-PAT-MIG-003` | `REUSE` — **wajib** (`PAT-DEC-065`, kontrak B15) | Satu cara membuat dan menghancurkan basis data bukti untuk kedua task; tanpa dependency baru |

#### Larangan pakai ulang apa adanya

| Komponen | Alasan |
| --- | --- |
| `RsmmcPilotMigrationSource` / `IRsmmcPilotMigrationSource` | Sengaja hanya-baca dan dijaga test (`PAT-SRC-013`); SQL-nya khusus Pilot dan meng-cast ke `text` (`PAT-SRC-010`). Tidak boleh ditambah operasi tulis |
| `RsmmcPilotApprovedBatchProfile` | Ekspektasi khusus Pilot 715/715 (`PAT-SRC-012`) |
| `RsmmcPilotMrnReconciliationService` | Mengubah pasien yang sudah ada, memuat seluruh set ke memori, dan memakai `LegacyPidComparer` yang salah mengurutkan angka negatif (`PAT-SRC-011`) |
| DTO `RsmmcPilotMrnReconcile*` | Semantik Pilot: `limit` 1–100, `TotalPilot`, status rekonsiliasi |
| Endpoint `POST …/rsmmc-pilot-mrn/reconcile` | Dilarang diperluas atau dialihfungsikan (`PAT-DEC-028`) |
| `CreatePatient`, `CreatePatientForAdmin`, `GeneratePatientCodeAsync`, `GenerateRunningCodeAsync`, `GenerateMedicalRecordNumberAsync` | Membangkitkan kode dan MRN sendiri (`PAT-DEC-020`, `PAT-SRC-006`) |
| `SavePatientQrCodeFile`, `DeletePhysicalFileIfExists` | Menimpa berkas dan menghapus tanpa bukti kepemilikan (`PAT-SRC-016`) |
| `GetFileStoragePaths()` pada simulasi | Membuat folder (`PAT-SRC-015`) |
| `!environment.IsProduction()` atau `IsStaging()` saja | Gagal terbuka / tidak membedakan huruf besar-kecil (`PAT-OQ-006`) |
| Insert EF `IsMember` tanpa penulisan eksplisit dan verifikasi | `false` tersimpan `true` (`PAT-SRC-008`); mengenai seluruh 704.506 baris (`PAT-FACT-074`). *Teks lama juga menyebut `IsActive`; seluruh preview `IsActive = true`, sehingga tidak terdampak* |
| Path QR bawaan `patient-qrcodes/<MRN>/` untuk pasien bulk | 715 path itu berisi QR historis Pilot (`PAT-FACT-070`) — dilarang (`PAT-DEC-052`) |
| Mengadopsi berkas QR yang sudah ada hanya karena path-nya memuat MRN target | `PAT-DEC-043`; adopsi QR historis tidak dipakai sama sekali (`PAT-DEC-052`) |
| Kunci `hashtext(…)` satu kunci 32-bit untuk worker | Dicabut `PAT-DEC-048`: dua `legacy_pid` berbeda dapat jatuh ke kunci yang sama |
| Sort ulang manifest `CANARY_A` berdasarkan `legacy_pid` | `PAT-DEC-045`: urutan eksekusi = urutan manifest |
| Mencabut izin `Patient : Create` sebagai satu-satunya bukti pembekuan | Hanya menutup `POST /admin` (`PAT-SRC-027`) |

#### Rencana berkas source — usulan, nama dan letak saja

Tidak ada implementasi pada task perencanaan ini. Nama berkas boleh disesuaikan saat build dan
dicatat pada laporan task.

| Berkas | Status | Keberlakuan QBE perkiraan | Isi |
| --- | --- | --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcBulkPatientMigrationDtos.cs` | Baru | `NEW CODE` | Request, response ringkasan, baris pasien, gerbang, konstanta status dan jenis konflik |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkPatientMigrationService.cs` | Baru | `NEW CODE` | Orkestrasi: validasi, gerbang B4.2, klasifikasi B3.2, transaksi per pasien B4.3, QR, log |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkMigrationStore.cs` | Baru | `NEW CODE` | Antarmuka dan implementasi SQL berparameter bertipe: baca preview, perencana, crosswalk, batch, cadangan; hitungan gerbang termasuk `PREVIEW_SCHEMA_SIGNATURE` dari `information_schema`; kunci dua bagian per pasien (`PAT-DEC-048`); **satu-satunya** operasi tulis: `INSERT` crosswalk |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkCanaryManifest.cs` | Baru | `NEW CODE` | Manifest eksekusi `RSMMC-CANARY-A` versi `1`: 25 `legacy_pid` dalam urutan yang disetujui, sebaran kelompok sumber, sidik jari berurutan `c43207eb…79ad` (`PAT-DEC-044`, `045`, `056`) |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkApprovedBatchProfile.cs` | Baru | `NEW CODE` | Angka gerbang B4.2 |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkPatientMapper.cs` | Baru | `NEW CODE` | Pemetaan preview → `MstPatient` (B10) dan pembanding baca ulang (`PAT-OQ-017`) |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcQrDestinationProbe.cs` | Baru (`PAT-DEC-046`) | `NEW CODE` | Memeriksa keberadaan berkas QR tujuan tanpa membuat folder |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Diperbarui | `TOUCHED LEGACY` (dibatasi) | Satu aksi baru sesudah aksi `Update` yang ada; factory QR kedua yang memakai `RsmmcPilotQrArtifactStore` dengan folder dasar `patient-qrcodes/rsmmc-bulk` (`PAT-DEC-052`); penentu lokasi baca-saja untuk probe. Aksi baru tidak mengakses `DbContext` |
| `Program.cs` | Diperbarui | `TOUCHED LEGACY` | Registrasi DI saja |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MigrasiMassal/*.cs` dan `Infrastructure/TiruanStoreMigrasiMassal.cs` | Baru — **wajib** (`PAT-DEC-037`, `065`) | Lingkup test | Test terfokus, termasuk test persistensi `IsMember = false` di SQLite **dan** test provider-real PostgreSQL 15 sekali pakai memakai harness `BE-PAT-MIG-003` (`PAT-MIG2-AC-47`), tanpa entri `.sln` |
| ~~Allocator nomor pasien normal yang sadar reservasi beserta perubahan `CreatePatient`~~ | **Dipindah** ke kartu `BE-PAT-MIG-003` (`PAT-DEC-057`) | — | — |
| `docs/module-blueprints/patient-management/task/report/backend/BE-PAT-MIG-002.md` | Baru | — | Laporan, ditulis `build-module-backend` |

**Tidak berubah:** `Areas/**/Models/**` termasuk `MstPatient.cs`;
`Repositories/Configurations/HealthServices/MstPatientConfiguration.cs`;
`Repositories/ApplicationDbContext.cs`; `Migrations/**`; `appsettings*`; seluruh berkas
`RsmmcPilot*` dan `RsmmcPilotMrnReconciliationDtos.cs`; `QuilvianSystemBackend.sln`.

#### Acceptance criteria

Penomoran `PAT-MIG2-AC-xx` dipakai di kartu dan traceability supaya tidak tertukar dengan
`AC-xx` milik `BE-PAT-MIG-001`. Butir bertanda **(runtime)** dibuktikan pada `PAT-GATE-002`,
bukan pada build.

**Gerbang dan keadaan batch**

1. **`PAT-MIG2-AC-01`** — Hanya environment bernama persis `Staging` (ordinal) yang diizinkan,
   untuk simulasi dan eksekusi; selain itu ditolak `403` sebelum tabel apa pun dibaca.
2. **`PAT-MIG2-AC-02`** — Eksekusi hanya untuk batch kanonik
   `45eeefba-dd39-6831-1bd2-d986851f7c1f`, dan batch itu wajib ada di
   `migration.rsmmc_patient_batch`. Batch lain ditolak `409`.
3. **`PAT-MIG2-AC-03`** — Gerbang preview kanonik: 704.506 baris, dan `legacy_pid`, `PatientCode`,
   serta MRN usulan masing-masing unik 704.506.
4. **`PAT-MIG2-AC-04`** — Gerbang perencana: 705.223 baris `FINALIZED` pada batch kanonik.
5. **`PAT-MIG2-AC-05`** — Himpunan tertunda tepat `{280143, 805596}`, dan `PAT-RSMMC-101416`,
   `PAT-RSMMC-619077`, serta MRN akhir kedua pasien itu tidak dipakai `MstPatient` mana pun.
6. **`PAT-MIG2-AC-06`** — Deteksi pergeseran baseline: jumlah `MstPatient` wajib sama dengan
   741 + jumlah crosswalk hasil worker; selisih menolak eksekusi.
7. **`PAT-MIG2-AC-07`** — Deteksi tabrakan `PatientCode`: gerbang "kode preview dipakai pasien
   tak terpetakan" = 0, dan pemeriksaan per pasien sesudah kunci, termasuk pasien bertanda
   terhapus.
8. **`PAT-MIG2-AC-08`** — Deteksi tabrakan MRN akhir dengan aturan yang sama dengan
   `PAT-MIG2-AC-07`.
9. **`PAT-MIG2-AC-09`** — Manifest eksekusi `RSMMC-CANARY-A` versi `1` eksplisit di server: 25
   `legacy_pid` kandidat (`00-interview-decisions.md` bagian 11) dalam urutan yang sama, menjadi
   manifest eksekusi sesudah QR A1 dan `BE-PAT-MIG-003` selesai (`PAT-DEC-056`). Urutan eksekusi
   persis urutan manifest, tanpa sort ulang (`PAT-DEC-045`). Respons menyebut `manifestId`,
   `manifestVersion`, dan sidik jari berurutan `c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad`
   (`PAT-DEC-044`). Sebaran kelompok sumber 7/6/6/6 diperiksa. Pasien manifest yang berubah keadaan
   diklasifikasikan, **tidak pernah** diganti. *Diamandemen kedua 15 Oktober 2026; teks sebelumnya
   memakai sidik jari himpunan `3b5affd9…434aa` dan belum menetapkan urutan.*

**Idempotensi dan konkurensi**

10. **`PAT-MIG2-AC-10`** — Baris crosswalk dibuat di transaksi yang sama dengan `MstPatient`,
    berkunci utama `legacy_pid`. Panggilan ulang sesudah sukses menghasilkan `ALREADY_MIGRATED`,
    bukan pasien kedua.
11. **`PAT-MIG2-AC-11`** — Kunci per pasien `pg_try_advisory_xact_lock(key1, key2)` dua bagian
    (`PAT-DEC-048`): key1 = namespace tetap worker/batch kanonik, key2 = `legacy_pid` dengan
    konversi Int32 yang divalidasi; di luar Int32 → `FAILED` / `LOCK_KEY_OUT_OF_RANGE`; kunci
    dipegang → `CONFLICT` / `CONCURRENT_RUN` tanpa menunggu. Preview, perencana, crosswalk, dan
    kepemilikan dibaca ulang **sesudah** kunci didapat. Tanpa kunci memori proses sebagai satu-satunya
    pengaman; tanpa Count+1 atau Max+1. *Diamandemen kedua 15 Oktober 2026.*
12. **`PAT-MIG2-AC-12`** — Kasus A: crosswalk belum ada → `MstPatient` dan crosswalk dibuat
    atomik.
13. **`PAT-MIG2-AC-13`** — Kasus B pada Canary-A: crosswalk ada tanpa target → `CONFLICT` /
    `CROSSWALK_EXISTS_WITHOUT_TARGET`; `match_status`, `migration_status`, `review_reason`, dan
    `last_batch_id` tidak berubah.
14. **`PAT-MIG2-AC-14`** — Kasus C+D: pemetaan cocok → `ALREADY_MIGRATED`, tidak menghabiskan
    `limit`, tidak menghentikan proses, tanpa perubahan; `QrCodePath` tidak sesuai →
    `MIGRATED_QR_INCONSISTENT`, dilaporkan, tidak diperbaiki.
15. **`PAT-MIG2-AC-15`** — Kasus E: target tidak ada → `CONFLICT` / `ORPHANED_CROSSWALK`;
    pasien tidak dibuat ulang.
16. **`PAT-MIG2-AC-16`** — Kasus F: kode atau MRN pasien target berbeda → `CONFLICT` /
    `TARGET_MAPPING_MISMATCH`; `quilvian_patient_id` dipetakan lebih dari satu `legacy_pid` →
    `CONFLICT` / `DUPLICATE_TARGET_MAPPING`.

**QR dan hasil yang tidak pasti**

17. **`PAT-MIG2-AC-17`** — Kasus G: path `patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png` diperiksa,
    penulisan memakai `FileMode.CreateNew`, berkas yang ada tidak pernah ditimpa atau diadopsi;
    konflik membatalkan transaksi pasien itu. Isi QR dari MRN akhir dengan format pembuatan pasien
    (`PAT-DEC-052`). *Diamandemen kedua 15 Oktober 2026.*
18. **`PAT-MIG2-AC-18`** — Kasus I: QR baru dihapus hanya bila kepemilikan terbukti lewat
    penanda pemanggilan + panjang + SHA-256; selain itu dipertahankan dan dilaporkan.
19. **`PAT-MIG2-AC-19`** — Kasus J: galat `CommitAsync` → `FAILED` / `COMMIT_OUTCOME_UNCERTAIN`,
    QR dipertahankan, tanpa *retry* otomatis, proses berhenti; panggilan berikutnya
    menyelesaikannya lewat crosswalk.
20. **`PAT-MIG2-AC-20`** — Kasus H: hasil penulisan QR tidak pasti → transaksi tidak pernah
    di-*commit*, berkas sebagian dipertahankan, `FAILED` / `CREATE_QR`.

**Mutasi, transaksi, dan log**

21. **`PAT-MIG2-AC-21`** — Simulasi tanpa mutasi: tidak ada `INSERT`/`UPDATE`/`DELETE` pada tabel
    mana pun, dan tidak ada folder atau berkas yang dibuat, diubah, atau dihapus. Simulasi boleh
    membaca keberadaan, metadata, dan SHA-256 berkas (`PAT-DEC-046`). `dryRun` yang tidak dikirim
    berarti simulasi.
22. **`PAT-MIG2-AC-22`** — Satu transaksi per pasien; kegagalan tidak membatalkan pasien yang
    sudah *commit*.
23. **`PAT-MIG2-AC-23`** — Setiap hasil pasien pada eksekusi dan ringkasan simulasi dicatat
    dengan `ActorUserId`; nama, NIK, telepon, dan alamat tidak masuk log.

**Larangan**

24. **`PAT-MIG2-AC-24`** — `PatientCode` = preview apa adanya; pembangkit `PatientCode` normal
    tidak dipanggil.
25. **`PAT-MIG2-AC-25`** — MRN = MRN akhir perencana = MRN usulan preview; pembangkit MRN normal
    tidak dipanggil.
26. **`PAT-MIG2-AC-26`** — `identity_number` tidak pernah dipakai untuk memilih, mencocokkan, atau
    menggabungkan pasien; tabrakan → `CONFLICT`.
27. **`PAT-MIG2-AC-27`** — `full_name` kosong → `CONFLICT` / `PREVIEW_VALUE_INVALID`; tidak ada
    nama pengganti.
28. **`PAT-MIG2-AC-28`** — `280143` dan `805596` tidak ada di manifest dan ditolak eksplisit oleh
    engine.
29. **`PAT-MIG2-AC-29`** — Pilot 715 tidak tersentuh: tidak ada `UPDATE`/`DELETE` pada
    `MstPatient`, tidak ada perubahan crosswalk Pilot; gerbang `PILOT_CROSSWALK_INTACT`,
    `PILOT_PREVIEW_ROWS`, dan `BACKUP_ROWS` lulus.
30. **`PAT-MIG2-AC-30`** — QR Pilot lama tidak pernah dihapus, dipindah, atau ditimpa; jalur
    pembersihan hanya menerima artefak buatan pemanggilan itu sendiri.
31. **`PAT-MIG2-AC-31`** — Produksi mustahil: daftar izin positif `Staging`, tanpa pengaturan
    konfigurasi yang melonggarkannya, tanpa jalur pintas.
32. **`PAT-MIG2-AC-32`** — Preview, perencana, tabel cadangan, dan crosswalk historis tidak
    diubah (`PAT-OQ-009` pilihan A).
33. **`PAT-MIG2-AC-33`** — Nilai tersimpan sama dengan preview: dibaca ulang sebelum *commit*,
    termasuk `IsMember = false` dan `IsActive = true`; selisih → `FAILED` / `VERIFY_PERSISTED`.
    *Dikoreksi 15 Oktober 2026 — tidak ada kebutuhan `IsActive = false`.*
34. **`PAT-MIG2-AC-34`** — Tidak ada EF migration, perubahan skema, atau perubahan
    model/konfigurasi `MstPatient`; tabel `migration.rsmmc_*` tidak dipetakan ke model EF; seluruh
    SQL berparameter.

**Bukti runtime dan batas gelombang**

35. **`PAT-MIG2-AC-35`** **(runtime)** — Eksekusi ulang Canary-A yang sama menghasilkan
    `ALREADY_MIGRATED` untuk seluruh baris manifest eksekusi; jumlah `MstPatient` dan crosswalk tidak
    bertambah. *Diamandemen 15 Oktober 2026; teks lama menyebut angka tetap 25.*
36. **`PAT-MIG2-AC-36`** **(runtime)** — Audit sesudah Canary-A: `MstPatient` = 741 + jumlah pasien
    manifest eksekusi; satu baris crosswalk `MIGRATED_BULK_WORKER` per pasien, 1:1, tanpa target
    ganda; preview tetap 704.506 `PENDING`; perencana 705.223; cadangan 715; pemetaan Pilot 715
    utuh; kode cadangan tertunda tidak dipakai; `IsMember = false` dan `IsActive = true` tersimpan
    fisik pada setiap pasien canary; QR baru ada di path strategi dengan isi MRN masing-masing; QR
    historis Pilot utuh — SHA-256 sebelum dan sesudah sama; nilai berikut pembangkit normal bukan
    nomor cadangan. *Diamandemen 15 Oktober 2026; teks lama menyebut angka tetap 766 dan 25.*
37. **`PAT-MIG2-AC-37`** — Build hanya mengenal `CANARY_A`; mode lain ditolak `400`; tidak ada
    *hosted service* atau perulangan otomatis atas 704 ribu pasien.

**Kontrak, izin, dan validasi**

38. **`PAT-MIG2-AC-38`** — Respons memuat ringkasan, gerbang, baris per pasien, dan sidik jari
    manifest sesuai Bagian B2.2; DTO tanpa entity EF.
39. **`PAT-MIG2-AC-39`** — `[Authorize]`, `[AccessAction("Update", …)]` +
    `[AccessPermission("Patient", "Update")]`; eksekusi menolak aktor yang tidak dikenali; tanpa
    `IsInRole`.
40. **`PAT-MIG2-AC-40`** — `batchId` dan `selectionMode` wajib; `limit` 1–25 bawaan 25;
    `dryRun` bawaan `true`.
41. **`PAT-MIG2-AC-41`** — `dotnet build -p:RunAnalyzers=false` `0` error dan QBE `Strict` `PASS`.
    Automated test terfokus **wajib** (`PAT-DEC-037`); topiknya mencakup kasus A–J, gerbang B4.2
    termasuk `NORMAL_ALLOCATION_GUARD`, manifest eksekusi dan urutannya yang **tidak** disort ulang,
    simulasi tanpa mutasi, konversi kunci Int32 termasuk `legacy_pid` negatif, kunci sibuk, path
    `rsmmc-bulk`, kolom crosswalk `PAT-DEC-047`, dan persistensi `IsMember = false`
    (`PAT-MIG2-AC-47`). *Diamandemen kedua 15 Oktober 2026.*
42. **`PAT-MIG2-AC-42`** — Endpoint dan perilaku `BE-PAT-MIG-001` tidak berubah; berkas
    `RsmmcPilot*` tidak diubah.

**Ditambahkan 15 Oktober 2026 — bukti Step 16**

43. **`PAT-MIG2-AC-43`** — Tidak satu pun dari 715 QR historis Pilot dihapus, ditimpa, dipindah,
    atau diadopsi. QR pasien bulk dibuat di path `rsmmc-bulk` (`PAT-DEC-052`), sehingga 715
    pasien bertabrakan tidak terblokir permanen oleh `CONFLICT` generik. Dibuktikan dengan test
    path strategi dan, saat runtime, SHA-256 QR historis yang tidak berubah.
44. **`PAT-MIG2-AC-44`** — *Dipindah ke `BE-PAT-MIG-003` (`PAT-DEC-057`) sebagai `PAT-MIG3-AC-03`–`05`.
    Teks semula: perlindungan reservasi `PatientCode` — pembuatan pasien normal tidak dapat
    menghasilkan `PatientCode` 742–705.249, termasuk `PAT-RSMMC-101416` dan `PAT-RSMMC-619077`.*
45. **`PAT-MIG2-AC-45`** — *Dipindah ke `BE-PAT-MIG-003` (`PAT-DEC-057`) sebagai `PAT-MIG3-AC-06`.
    Teks semula: perlindungan reservasi MRN — pembuatan pasien normal tidak dapat menghasilkan MRN
    akhir `FINALIZED` mana pun, termasuk MRN dua pasien tertunda.*
46. **`PAT-MIG2-AC-46`** — Gerbang `NORMAL_ALLOCATION_GUARD`: eksekusi ditolak `409` kecuali
    `BE-PAT-MIG-003` aktif dan terverifikasi — pemeriksa baca-saja melaporkan `overallGuardReady = true`,
    yaitu guard berjalan dengan konfigurasi sah, `PatientCode` berikutnya ≥ `PAT-RSMMC-705250`, dan MRN
    berikutnya ≥ `00-89-01-88` (`PAT-DEC-055`, `064`, `066`, `067`; kontrak B14.2). Pembekuan
    operasional saja **tidak** meluluskan gerbang; simulasi melaporkan hasilnya. *Diamandemen 16 Oktober
    2026 (nama field `1.2.1`); teks sesi ketiga memakai `guardActive`/`configurationValid`; teks
    amandemen kedua: "MRN berikutnya bukan MRN akhir `FINALIZED`"; teks sebelumnya menerima "guard
    aktif atau pembekuan aktif".*
47. **`PAT-MIG2-AC-47`** — Persistensi `IsMember = false` (`PAT-DEC-053`): sesudah insert EF, di
    transaksi yang sama, update eksplisit hanya baris pasien baru, baca ulang `AsNoTracking`,
    verifikasi `IsMember = false`, `IsActive = true`, dan seluruh kolom preview; rollback bila
    berbeda. Konfigurasi EF global tidak diubah. Preview tidak diubah. Test persistensi terfokus wajib,
    **ditambah bukti provider-real** pada PostgreSQL 15 sekali pakai (`PAT-DEC-065`, kontrak B15) yang
    membuktikan: `IsMember = false` tersimpan fisik `false`; `IsActive = true` tetap `true`; koreksi
    eksplisit di transaksi yang sama berhasil; baca ulang `AsNoTracking` cocok dengan preview; rollback
    tidak meninggalkan pasien maupun crosswalk; percobaan ulang idempoten tidak membuat target ganda;
    kegagalan sebelum *commit* tidak meninggalkan pemetaan pasien ter-*commit*. Bukti SQLite saja tidak
    cukup. *Diamandemen sesi ketiga 15 Oktober 2026; teks sebelumnya: "caranya `PAT-OQ-036`".*
48. **`PAT-MIG2-AC-48`** — Kandidat 25 tidak dieksekusi sebelum syarat `PAT-DEC-056` terpenuhi.
    Sesudahnya, 25 nilai yang sama dalam urutan yang sama menjadi `RSMMC-CANARY-A` v1 tanpa
    penggantian: 10 menguji informasi path bawaan historis, 15 menguji path bawaan bersih, dan
    seluruh 25 menulis ke `rsmmc-bulk`. *Diamandemen kedua 15 Oktober 2026.*

**Ditambahkan pada amandemen kedua 15 Oktober 2026**

49. **`PAT-MIG2-AC-49`** — Baris crosswalk baru berisi tepat kolom `PAT-DEC-047`: `legacy_pid`,
    `legacy_old_rm` = `preview.legacy_old_rm`, `normalized_mrn` = `preview.legacy_normalized_mrn`,
    `normalized_nik` = `preview.legacy_normalized_nik`, `quilvian_patient_id` = `Id` baru,
    `match_status = TARGET_CREATED`, `migration_status = MIGRATED_BULK_WORKER`, `last_batch_id` =
    batch kanonik, `created_at` = waktu database; `canonical_legacy_pid`, `review_reason`,
    `updated_at` NULL. `normalized_nik` dan `normalized_mrn` tidak pernah **dibaca** untuk mencocokkan
    pasien atau menentukan MRN tujuan, dan tidak masuk respons maupun log.
50. **`PAT-MIG2-AC-50`** — Telepon preview berklasifikasi `SAFE_TO_STORE` yang sudah dipakai
    `MstPatient` lain menghasilkan `CONFLICT` / `PHONE_ALREADY_USED`; pasien tidak dibuat; nilai
    telepon preview tidak dibuang atau diubah untuk "meloloskan" pasien (`PAT-DEC-050`).

**Ditambahkan sesi ketiga 15 Oktober 2026**

51. **`PAT-MIG2-AC-51`** — SQL bertipe dan tanda tangan skema (`PAT-DEC-063`; kontrak B4.2, B9.1,
    B10): seluruh SQL migrasi berparameter bertipe — `batch_id` uuid, `legacy_pid` bigint, enum
    integer, boolean — tanpa cast `::text`. Gerbang `PREVIEW_SCHEMA_SIGNATURE` dievaluasi pertama dan
    menolak panggilan, sebelum satu pasien pun diproses, bila kolom yang dipakai worker hilang atau
    tipenya berbeda — termasuk `full_name` (text/varchar) dan `birth_date` (date). Kolom enum wajib
    atau boolean bernilai NULL, serta nilai yang bukan anggota enum — termasuk 0 untuk
    `PatientType`/`PatientStatus` — → `CONFLICT` / `PREVIEW_VALUE_INVALID`; `gender` NULL → `Gender`
    NULL; tidak ada pemetaan otomatis ke `Unknown`.

#### Verifikasi

| No | Bukti | Bentuk yang dicatat pada laporan task |
| ---: | --- | --- |
| V1 | QBE preflight | Area, Module, prefix, keberlakuan, dan QBE ID yang benar-benar berlaku |
| V2 | Inspeksi source terkini | SHA saat build; bila berbeda dari `a52b3c98`, kutipan baris pada kontrak Bagian B dan `00-interview-decisions.md` bagian 9 diperiksa ulang |
| V3 | Build | Keluaran `dotnet build -p:RunAnalyzers=false`, jumlah error dan warning apa adanya |
| V4 | QBE `Strict` | Keluaran checker cakupan *working tree* |
| V5 | Review diff dan cakupan | Berkas berubah beserta alasannya; tidak ada berkas pada daftar "Tidak berubah" yang tersentuh |
| V6 | Tidak ada artefak QR di Git | `git status --short` tanpa `patient-qrcodes/` maupun `.png` runtime |
| V7 | Tidak ada eksekusi database dan EF migration | Pernyataan eksplisit; diff tidak menyentuh `Migrations/`, `Repositories/`, `Models/` |
| V8 | Tidak ada pemanggilan endpoint | Pernyataan eksplisit |
| V9 | Tidak ada commit, push, atau deploy tanpa otorisasi | `git status --short` dan pernyataan eksplisit |
| V10 | Verifikasi kontrak dan proses bisnis | Method, path, request/response, kode status, dan izin yang terpasang; penelusuran urutan B4.3, gerbang B4.2, dan kasus A–J pada source, dengan contoh berangka |
| V11 | Automated test dan bukti provider-real | **Wajib** (`PAT-DEC-037`, `065`): perintah dan jumlah lulus/gagal apa adanya, untuk test SQLite **dan** test PostgreSQL 15 sekali pakai (kontrak B15). Bukti SQLite saja tidak cukup (`PAT-DEC-053`). Baris laporan: `AUTOMATED TEST: dotnet test <project> — PASS/FAIL`, ditambah baris terpisah untuk provider-real |
| V12 | Runtime | **Bukan** bagian build: `PAT-MIG2-AC-35` dan `36` dibuktikan pada `PAT-GATE-002` |

#### Definition of Done — fase implementasi

- [x] `PAT-DEC-033` disetujui — 15 Oktober 2026, sesi ketiga
- [ ] `BE-PAT-MIG-003` ✅ dan `PAT-GATE-005` `PASS` **sebelum** build dimulai (urutan wajib pemilik)
- [ ] Endpoint sesuai kontrak Bagian B yang sudah `approved`
- [ ] Build backend lulus
- [ ] QBE `Strict` lulus
- [ ] Test terfokus lulus — **wajib** (`PAT-DEC-037`), termasuk persistensi `IsMember = false`
- [ ] Bukti persistensi provider-real `IsMember = false` pada PostgreSQL 15 sekali pakai tersedia (`PAT-DEC-065`, `PAT-MIG2-AC-47`)
- [ ] Tidak ada berkas di luar cakupan yang berubah; perilaku `BE-PAT-MIG-001` tetap
- [ ] Laporan task lengkap di `../task/report/backend/BE-PAT-MIG-002.md`
- [ ] Source sudah direview leader
- [ ] **Belum ada** eksekusi Canary-A

#### Definition of Done — runtime Canary-A (`PAT-GATE-002`, terpisah dari build)

- [ ] Otorisasi (a): deployment Staging dan simulasi
- [ ] Bukti simulasi ditinjau pemilik
- [ ] Otorisasi (b): eksekusi `dryRun=false`
- [ ] Audit database dan QR sesudah eksekusi lulus (`PAT-MIG2-AC-36`)
- [ ] Eksekusi ulang membuktikan idempotensi (`PAT-MIG2-AC-35`)
- [ ] `PAT-GATE-005` `PASS` — `BE-PAT-MIG-003` dideploy dan terverifikasi sebelum (b)
- [ ] Bukti pembekuan pendaftaran normal yang menutup `POST /`, `POST /kiosk`, dan `POST /admin` (`PAT-DEC-055`, `PAT-SRC-027`)
- [ ] Inventaris filesystem read-only `patient-qrcodes/` dan `patient-qrcodes/rsmmc-bulk/` sebelum otorisasi (`PAT-DEC-062`)

---

### ✅ `BE-PAT-MIG-003` — RSMMC Normal Patient Identifier Reservation Guard

> **Kartu ini `approved`** — 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`). Task ini adalah task
> prasyarat `BE-PAT-MIG-002` (`PAT-DEC-057`) dan **boleh dimulai** dalam sesi `TASK MODE: BACKEND`
> tersendiri lewat `build-module-backend`, dengan branch yang ditetapkan pemilik. Belum ada source.
>
> *Banner lama — jejak:* "Kartu ini usulan dan belum disetujui … Task ini tidak boleh dimulai sampai
> `PAT-DEC-033` disetujui."

| Field | Isi |
| --- | --- |
| **Task ID** | `BE-PAT-MIG-003` |
| **Judul** | RSMMC Normal Patient Identifier Reservation Guard |
| **Status** | ✅ **SELESAI** 19 Oktober 2026 — source di-*merge* pemilik ke Integration (PR #290: `a9f2fda2` dokumen, `ab45dd3c` source, merge `3c457ab4`); dideploy ke DEV dan STAGING dengan `RsmmcMigrationReservationGuard__Enabled = true`, health `PASS`; **`PAT-GATE-005` generasi 1 `PASS`** (dilaporkan pemilik, `PAT-FACT-090`): `guardState = ACTIVE`, `PAT-RSMMC-705250`, `00-89-01-88`, `overallGuardReady = true` — memenuhi `PAT-MIG3-AC-12`. Bukti build 17 Oktober 2026: `dotnet build` `0` error; test `143` lulus / `0` gagal / `7` dilewati; PostgreSQL 15.15 provider-real `10`/`10` PASS (dijalankan pemilik); QBE `Strict` `PASS`; tanpa EF migration. **Catatan:** generasi 1 hanya membuktikan `CreatePatient`; jalur bayi `POST /newborns` (upstream `3a0189c2`) ditutup task terpisah `BE-PAT-MIG-003A`. *Riwayat: 🟡 siap review pemilik 17 Oktober 2026* Laporan: [laporan](../task/report/backend/BE-PAT-MIG-003.md). *Riwayat: `approved` — belum dikerjakan (15 Oktober 2026); draft terblokir sampai sesi kedua* |
| **Klasifikasi bisnis** | Perubahan alur pendaftaran pasien normal untuk melindungi reservasi nomor migrasi. **Bukan** migrasi data |
| **Perkiraan klasifikasi build** | `HEAVY` — perkiraan skor 10: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika bisnis kompleks (2), memakai endpoint yang ada dengan perubahan perilaku (1), perilaku persistence dan serialisasi (1), berkaitan dengan keamanan (1), satu repository (0), alur pendaftaran kiosk dan admin (2). Ditetapkan ulang saat build |
| **Area / Module / Prefix** | `HealthServices` / `PatientManagement` / `Pat`, Submodule `MasterData`. Tidak ada entity persisted baru |
| **Outcome** | Di Staging persis, pendaftaran pasien normal — `POST /`, `POST /kiosk`, `POST /admin` — memakai `PatientCode` ≥ `PAT-RSMMC-705250` dan MRN ≥ `00-89-01-88`, sehingga tidak pernah memakai `PatientCode` 742–705.249 maupun MRN akhir `FINALIZED`; reservasi migrasi RSMMC dan folder QR historis Pilot terlindungi; di Staging persis tanpa `Enabled = true` pendaftaran gagal tertutup (`PAT-DEC-067`); pembekuan pendaftaran Staging dapat dicabut sesudah `PAT-GATE-005`. Di Produksi dan environment lain penomoran tidak berubah |
| **Asal** | Step 16: pembangkit normal memilih `PAT-RSMMC-00742` dan `00-00-00-27`, keduanya dicadangkan (`PAT-FACT-072`, `073`); keputusan `PAT-DEC-054` dan `PAT-DEC-057` |
| **Requirement/decision** | `PAT-DEC-042`, `054`, `055`, `057`, `064`, `065`, `066`, `067`, `068`; usulan standar `PAT-GAP-002`–`008` (`requirement-completeness-assessment.md`); requirement `PAT-MIG3-AC-01`–`PAT-MIG3-AC-14`, menggantikan `PAT-MIG2-AC-44`/`45` |
| **Kontrak** | [`contracts/api-contract.md`](../contracts/api-contract.md) `1.2.2` — **`approved`**: B14–B14.3 dan B15. Bentuk request/response ketiga endpoint pendaftaran **tidak** berubah; cara memilih `PatientCode` dan MRN berubah di Staging persis; ditambah penolakan `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` khusus Staging persis dan endpoint baca-saja `GET /admin/migration/rsmmc-reservation-guard` (`PAT-DEC-067`); kunci serialisasi dua bagian tetap (`PAT-DEC-068`) |
| **Dependency** | Tidak menunggu task lain. Keputusan `PAT-DEC-033`: `approved`. Prasyarat runtime **sesudah** task ini: `PAT-GATE-005` |
| **Batas wewenang database/migration** | Hanya implementasi source. Lantai 705.250 dan 890.188 disimpan sebagai konstanta di profil reservasi (`PAT-GAP-003`), sehingga **tidak** membutuhkan EF migration atau perubahan skema. `appsettings*` tidak diubah; konfigurasi `RsmmcMigrationReservationGuard:Enabled` diatur pemilik saat deployment. Satu-satunya eksekusi database yang diizinkan adalah test provider-real terhadap PostgreSQL 15 sekali pakai (`PAT-DEC-065`) — tidak pernah Staging bersama atau Produksi. Tidak ada pemanggilan endpoint, deployment, maupun perubahan Produksi |
| **Di luar cakupan** | Konfigurasi EF `IsMember` (`PAT-DEC-053`); cara penulisan QR pendaftaran normal (`File.WriteAllBytes` tetap — dicatat sebagai temuan, bukan diperbaiki); worker migrasi; seeder demo Operating Room yang memakai kode `DEMO-OPR-*` |
| **QBE** | Diselesaikan saat build. Perkiraan: allocator baru `NEW CODE`; `PatientController.cs` dan `Program.cs` `TOUCHED LEGACY`. ID yang kemungkinan berlaku: `QBE-SVC-001`, `QBE-CODE-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-004` (indeks unik `PatientCode` dan `MedicalRecordNumber` sudah ada), `QBE-TXN-001`, `QBE-LOG-001` |
| **Laporan** | [laporan](../task/report/backend/BE-PAT-MIG-003.md) — ditulis `build-module-backend` 16 Oktober 2026 |

#### Rencana berkas source — usulan, nama dan letak saja

| Berkas | Status | Keberlakuan QBE perkiraan | Isi |
| --- | --- | --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientIdentifierAllocator.cs` | Baru | `NEW CODE` | Saat guard aktif: alokasi `PatientCode` (lantai 705.250) dan MRN (lantai 890.188) di bawah `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap (`PAT-DEC-068`) — dalam transaksi `CreatePatient`. Saat guard tidak aktif: algoritma nomor kosong terkecil yang sama dengan sekarang, tanpa kunci baru. Pemeriksa baca-saja (kontrak B14.2) |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcIdentifierReservationProfile.cs` | Baru | `NEW CODE` | Konstanta lantai `705250` dan `890188`, awalan `PAT-RSMMC-`, pembacaan konfigurasi `RsmmcMigrationReservationGuard:Enabled`, dan aturan environment kontrak B14.3 |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Diperbarui | `TOUCHED LEGACY` (dibatasi) | `CreatePatient` memakai allocator; `GeneratePatientCodeAsync` dan `GenerateMedicalRecordNumberAsync` tidak lagi dipanggil jalur pendaftaran |
| `Program.cs` | Diperbarui | `TOUCHED LEGACY` | Registrasi DI saja |
| `Tests/QuilvianSystemBackend.PatientManagementTests/ReservasiNomor/*.cs` | Baru — wajib (test terfokus, `PAT-DEC-057`) | Lingkup test | Lantai, rentang cadangan, ketiga rute, pemeriksa baca-saja, aturan environment dan konfigurasi |
| `Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/` — harness PostgreSQL sekali pakai, beserta test provider-real allocator | Baru — wajib (`PAT-DEC-065`) | Lingkup test | Membuat dan menghapus basis data acak dari connection string environment variable; dipakai ulang `BE-PAT-MIG-002` (kontrak B15) |
| `docs/module-blueprints/patient-management/task/report/backend/BE-PAT-MIG-003.md` | Baru | — | Laporan, ditulis `build-module-backend` |

#### Acceptance criteria

1. **`PAT-MIG3-AC-01`** — Allocator `PatientCode` dan MRN normal berada di module service; controller
   tidak lagi mengalokasikan nomor bisnis (`QBE-SVC-001`, `QBE-CODE-002`), dan pembangkit "kosong
   terkecil" lama tidak lagi dipanggil jalur pendaftaran mana pun.
2. **`PAT-MIG3-AC-02`** — `POST /`, `POST /kiosk`, dan `POST /admin` memakai allocator yang sama
   lewat `CreatePatient` (`PAT-SRC-023`).
3. **`PAT-MIG3-AC-03`** — Lantai tahan lama: suffix `PatientCode` yang dipilih adalah suffix pertama
   yang belum dipakai dan ≥ 705.250. *Contoh:* belum ada `PAT-RSMMC-` ≥ 705.250 → `PAT-RSMMC-705250`;
   705.250 dan 705.251 sudah dipakai → `PAT-RSMMC-705252`.
4. **`PAT-MIG3-AC-04`** — Allocator tidak pernah menghasilkan suffix 742–705.249, termasuk
   `PAT-RSMMC-101416` dan `PAT-RSMMC-619077`.
5. **`PAT-MIG3-AC-05`** — Alokasi diserialkan di tingkat database dengan kunci ber-namespace di dalam
   transaksi pembuatan pasien; indeks unik `PatientCode` tetap pengaman terakhir; tidak ada kunci
   memori proses sebagai satu-satunya pengaman dan tidak ada Count+1/Max+1 tanpa proteksi
   (`QBE-CODE-003`).
6. **`PAT-MIG3-AC-06`** — Lantai MRN (`PAT-DEC-064`): saat guard aktif, MRN yang dipilih adalah MRN
   numerik pertama yang belum dipakai dan ≥ 890.188, berformat `00-00-00-00`; tidak pernah di bawah
   890.188, sehingga tidak pernah MRN akhir `FINALIZED` (tertinggi 890.187), termasuk MRN dua pasien
   tertunda, dan pendaftaran normal tidak pernah menulis QR ke folder 715 MRN historis Pilot.
   *Contoh:* belum ada MRN ≥ 890.188 → `00-89-01-88`; `00-89-01-88` sudah dipakai → `00-89-01-89`.
   Allocator tidak membaca tabel `migration.*`. *Diamandemen sesi ketiga; teks lama: "Mekanismenya
   mengikuti `PAT-OQ-031` (`BLOCKING`)".*
7. **`PAT-MIG3-AC-07`** — Pemeriksa baca-saja (kontrak `1.2.1` B14.2) mengembalikan `environment`,
   `guardState`, `guardEnabled`, `guardConfigurationValid`, kedua lantai, `nextPatientCodeNumeric`/
   `Formatted`, `nextMrnNumeric`/`Formatted`, `patientCodeFloorSatisfied`, `mrnFloorSatisfied`, dan
   `overallGuardReady` tanpa mengalokasikan, tanpa kunci, dan tanpa menulis — tidak membuat pasien, QR,
   folder, maupun mengubah tabel migrasi. Tersedia sebagai service untuk `NORMAL_ALLOCATION_GUARD`
   (`PAT-MIG2-AC-46`) dan sebagai `GET /admin/migration/rsmmc-reservation-guard` dengan
   `[AccessPermission("Patient", "Update")]`, khusus `Staging` persis (`403` di luar itu). *Diamandemen
   16 Oktober 2026 (`PAT-DEC-067`).*
8. **`PAT-MIG3-AC-08`** — Di `EnvironmentName == "Staging"` persis (ordinal) allocator aman reservasi
   wajib dipakai dan hanya berjalan bila `RsmmcMigrationReservationGuard.Enabled == true`
   (`PAT-DEC-066`, `067`). Di luar Staging persis, penomoran memakai algoritma nomor kosong terkecil
   yang sama dengan sekarang, tanpa kunci baru dan tanpa membaca konfigurasi guard, sehingga Produksi
   tidak berubah. *Diamandemen 16 Oktober 2026; teks sesi ketiga: "Saat tidak aktif, penomoran memakai
   algoritma … pemeriksa melaporkan `guardActive = false`".*
9. **`PAT-MIG3-AC-09`** — Bentuk request/response ketiga endpoint pendaftaran tidak berubah; perilaku
   `IsMember` (`PAT-DEC-053`) dan cara pembuatan QR pendaftaran normal tidak berubah.
10. **`PAT-MIG3-AC-10`** — Test terfokus wajib: lantai, rentang cadangan termasuk dua kode tertunda,
    ketiga rute, pemeriksa baca-saja, dan keenam baris tabel environment/konfigurasi kontrak B14.3.
    Perilaku bersamaan dibuktikan pada `PAT-MIG3-AC-13`, karena SQLite tidak dapat membuktikan kunci
    PostgreSQL. *Diamandemen sesi ketiga.*
11. **`PAT-MIG3-AC-11`** — `dotnet build -p:RunAnalyzers=false` `0` error dan QBE `Strict` `PASS`.
12. **`PAT-MIG3-AC-12`** **(runtime, `PAT-GATE-005`)** — Di Staging sesudah deploy dengan
    `Enabled = true`, `GET /admin/migration/rsmmc-reservation-guard` melaporkan `overallGuardReady = true`,
    `nextPatientCodeFormatted` ≥ `PAT-RSMMC-705250`, dan `nextMrnFormatted` ≥ `00-89-01-88`. Baru sesudah
    itu pemilik boleh mencabut pembekuan pendaftaran.
13. **`PAT-MIG3-AC-13`** — Uji PostgreSQL provider-real (`PAT-DEC-065`, kontrak B15) terhadap
    PostgreSQL 15 sekali pakai yang dihancurkan sesudah test: lantai `PatientCode` dan MRN; kode
    742–705.249 dan MRN ≤ 890.187 tidak pernah diberikan; dua pendaftaran bersamaan memperoleh nomor
    berbeda lewat kunci, yang kedua menunggu; mode tanpa guard memberi hasil yang sama dengan algoritma
    lama; konfigurasi hilang di `Staging` persis → gagal tertutup. Server tidak tersedia saat bukti
    diminta → test **gagal**, tidak dilewati. Tidak pernah memakai Staging bersama atau Produksi.
14. **`PAT-MIG3-AC-14`** — Gagal tertutup (`PAT-DEC-066`, `067`; `PAT-GAP-002`): di `Staging` persis,
    `Enabled = false`, bagian `RsmmcMigrationReservationGuard` yang tidak ada, `Enabled` yang tidak ada,
    atau nilai yang bukan boolean / tidak terbaca membuat ketiga rute pendaftaran ditolak `503` /
    `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` dengan log peringatan tanpa data pribadi, **sebelum**
    insert pasien, penulisan QR, dan penulisan foto, dan tidak pernah jatuh ke penomoran lama. Lantai
    tidak dapat diubah lewat konfigurasi. *Diamandemen 16 Oktober 2026; teks sesi ketiga: "`Enabled = false`
    yang ditulis eksplisit mematikan guard".*

#### Verifikasi

| No | Bukti | Bentuk yang dicatat pada laporan task |
| ---: | --- | --- |
| V1 | QBE preflight | Area, Module, prefix, keberlakuan, QBE ID yang berlaku |
| V2 | Inspeksi source terkini | SHA saat build; kutipan baris `PAT-SRC-006`, `023`, `027` diperiksa ulang |
| V3 | Build | Keluaran `dotnet build -p:RunAnalyzers=false` apa adanya |
| V4 | QBE `Strict` | Keluaran checker |
| V5 | Review diff dan cakupan | Hanya allocator, profil, `CreatePatient`, DI, dan test yang berubah |
| V6 | Automated test dan bukti provider-real | Perintah dan jumlah lulus/gagal apa adanya, untuk test SQLite **dan** test PostgreSQL 15 sekali pakai (`PAT-MIG3-AC-13`) |
| V7 | Tidak ada eksekusi database, EF migration, pemanggilan endpoint, commit, atau deploy | Pernyataan eksplisit dan `git status --short` |
| V8 | Verifikasi proses bisnis | Penelusuran allocator dengan contoh berangka `PAT-MIG3-AC-03` |
| V9 | Runtime | **Bukan** bagian build: `PAT-MIG3-AC-12` pada `PAT-GATE-005` |

#### Definition of Done — fase implementasi

- [x] `PAT-DEC-033`, `PAT-OQ-031`, dan `PAT-OQ-037` diputuskan — 15 Oktober 2026, sesi ketiga
- [x] Allocator, pemeriksa, dan aturan environment/konfigurasi sesuai kontrak B14–B14.3
- [x] Build backend lulus dan QBE `Strict` lulus
- [x] Test terfokus lulus
- [x] Uji PostgreSQL provider-real lulus (`PAT-MIG3-AC-13`) — `10`/`10`, dijalankan pemilik 17 Oktober 2026
- [x] Tidak ada berkas di luar cakupan yang berubah
- [x] Laporan task lengkap di `../task/report/backend/BE-PAT-MIG-003.md`
- [x] Source sudah direview pemilik — di-*merge* lewat PR #290
- [x] `PAT-GATE-005` diminta terpisah sesudah review — generasi 1 `PASS` 19 Oktober 2026 (`PAT-FACT-090`)

---

### 🟡 `BE-PAT-MIG-003A` — Newborn Identifier Reservation Guard

> **Kartu ini ditambahkan atas instruksi tertulis pemilik 19 Oktober 2026** (`TASK: BE-PAT-MIG-003A`,
> bagian 1 dan 10.6; `PAT-DEC-069`). Task prasyarat pengerasan: menutup celah yang ditemukan impact scan
> `BE-PAT-MIG-002` (`PAT-SRC-031`). `BE-PAT-MIG-002` tetap ⛔ sampai task ini selesai, diuji, dideploy ke
> Staging, dan `PAT-GATE-005` generasi 2 `PASS`.

| Field | Isi |
| --- | --- |
| **Task ID** | `BE-PAT-MIG-003A` |
| **Judul** | Newborn Identifier Reservation Guard |
| **Status** | 🟡 **SOURCE SIAP — MENUNGGU BUKTI PROVIDER-REAL DAN REVIEW PEMILIK** 19 Oktober 2026 — di atas Integration `c6537ee4` (belum di-*commit*); `dotnet build -c Release` `0` error, `0` warning dari berkas yang diubah (298 warning lama di berkas lain); `dotnet test` PatientManagementTests `189` lulus / `0` gagal / `16` dilewati — `46` kasus baru `BE-PAT-MIG-003A` lulus, `70` regresi `BE-PAT-MIG-003` dan `73` regresi `BE-PAT-MIG-001` lulus, `16` dilewati = test provider-real (7 lama + 9 baru) tanpa server; QBE `Strict` `PASS`; `has-pending-model-changes` tanpa perubahan. **Belum:** uji PostgreSQL 15 provider-real `PAT-MIG3A-AC-09` (server tidak tersedia di sesi agent — dijalankan pemilik); review source; `PAT-MIG3A-AC-12` milik runtime `PAT-GATE-005` generasi 2. Laporan: [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| **Klasifikasi bisnis** | Perubahan alur pendaftaran bayi untuk melindungi reservasi nomor migrasi. **Bukan** migrasi data |
| **Klasifikasi build** | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas diubah > 8 termasuk test dan dokumen (2), logika bisnis kompleks — konkurensi dan urutan kunci (2), mengubah kontrak aditif pemeriksa (2), perilaku persistence/transaksi yang sudah ada (1), berkaitan dengan keamanan (1), satu repository (0), tanpa UI (0) |
| **Area / Module / Prefix** | `HealthServices` / `PatientManagement` / `Pat`, Submodule `MasterData` — registry `ACTIVE` baris 24. Tidak ada entity persisted baru |
| **Outcome** | Di `Staging` persis, pendaftaran bayi `POST /newborns` tidak dapat memakai `PatientCode` maupun MRN yang dicadangkan migrasi RSMMC: guard aktif → nomor pertama yang belum dipakai ≥ `PAT-RSMMC-705250` / `00-89-01-88` lewat allocator dan kunci yang sama dengan pendaftaran normal; guard tidak sah → gagal tertutup sebelum alokasi dan mutasi. Di environment lain perilaku `NumberSeries` bayi tidak berubah. Pemeriksa `PAT-GATE-005` membuktikan kedua jalur |
| **Asal** | Impact scan `BE-PAT-MIG-002` 19 Oktober 2026: `BLOCKED` (`PAT-FACT-091`, `PAT-SRC-031`) |
| **Requirement/decision** | `PAT-DEC-069`–`PAT-DEC-074`; requirement `PAT-MIG3A-AC-01`–`PAT-MIG3A-AC-12` |
| **Kontrak** | [`contracts/api-contract.md`](../contracts/api-contract.md) `1.3.0` — **`approved`**: B14.2 (keluaran diperluas, aditif) dan B14.4 (baru). Bentuk request/response `POST /newborns` tidak berubah; ditambah `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` khusus Staging persis |
| **Dependency** | `BE-PAT-MIG-003` ✅ (allocator, profil, pemeriksa, harness provider-real). Prasyarat runtime **sesudah** task ini: `PAT-GATE-005` generasi 2 |
| **Batas wewenang database/migration** | Hanya implementasi source dan test. Tidak ada EF migration baru (`PAT-DEC-074`); migration `20261016080745_ReviseNewbornAdmissionFlow` milik fitur bayi. Tidak ada eksekusi database selain test provider-real terhadap PostgreSQL 15 sekali pakai, pemanggilan endpoint, deployment, maupun perubahan Produksi |
| **Di luar cakupan** | Implementasi `BE-PAT-MIG-002`; Canary-A; data migrasi; seeder demo Operating Room; QR runtime yang ter-*commit* (`P-09` impact scan); perilaku penomoran Produksi |
| **QBE** | `NEW CODE`: method baru pada `PatientIdentifierAllocationService`, `RsmmcIdentifierReservationProfile`, test. `TOUCHED LEGACY` (dibatasi): `NewbornPatientRegistrationService`, `PatientController`, DTO pemeriksa. ID yang berlaku: `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-CODE-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-004`, `QBE-CODE-006`, `QBE-TXN-001` |
| **Laporan** | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) — ditulis `build-module-backend` 19 Oktober 2026 |

#### Rencana berkas source

| Berkas | Status | Keberlakuan QBE | Isi |
| --- | --- | --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcIdentifierReservationProfile.cs` | Diperbarui | `NEW CODE` (penambahan) | `RsmmcIdentifierAllocationMode`, aturan tunggal `ResolveAllocationMode`, `IsMigrationReservationSafe`, `IsGuardReady` dua jalur |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientIdentifierAllocationService.cs` | Diperbarui | `NEW CODE` (penambahan) | `AllocateReservedAsync` publik — algoritma berlantai yang sudah ada, dipakai bersama; `CheckAsync` melaporkan kedua jalur |
| `Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs` | Diperbarui | `TOUCHED LEGACY` | Guard dinilai di awal; gagal tertutup; cabang berlantai di Staging persis; cabang `NumberSeries` tidak diubah |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Diperbarui | `TOUCHED LEGACY` | `503` untuk status penolakan guard jalur bayi; helper penolakan dipakai bersama; log pemeriksa |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcReservationGuardDtos.cs` | Diperbarui | `TOUCHED LEGACY` | Empat field aditif |
| `Tests/QuilvianSystemBackend.PatientManagementTests/**` | Baru/diperbarui | Lingkup test (diminta eksplisit pemilik) | `BayiUji.cs`, `PendaftaranBayiReservasiTests.cs`, `PendaftaranBayiPostgresSungguhanTests.cs`; parameter opsional layanan bayi pada `ReservasiHarness.BuatController` |

#### Acceptance criteria

1. **`PAT-MIG3A-AC-01`** — Di `Staging` persis dengan `Enabled = true`, `PatientCode` dan MRN bayi adalah
   nomor pertama yang belum dipakai ≥ `PAT-RSMMC-705250` / `00-89-01-88`, lewat
   `PatientIdentifierAllocationService.AllocateReservedAsync`; deret `NumberSeries` bayi tidak dibaca
   maupun dinaikkan. *Contoh:* ibu `PAT-RSMMC-00741`, nomor cadangan `PAT-RSMMC-705249` ada → bayi
   `PAT-RSMMC-705250` / `00-89-01-88`. (T6)
2. **`PAT-MIG3A-AC-02`** — Di `Staging` persis, `Enabled = false`, konfigurasi hilang, salah bentuk, atau
   tidak terbaca → `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` dengan `guardState`, sebelum basis
   data dibaca, dikunci, atau ditulis; tidak pernah jatuh ke `NumberSeries`. (T3, T4, T5)
3. **`PAT-MIG3A-AC-03`** — Di luar `Staging` persis, perilaku `NumberSeries` bayi sama dengan sebelum
   task, konfigurasi guard tidak dibaca, dan kunci `(PATI, NORM)` tidak diambil. (T1, T2)
4. **`PAT-MIG3A-AC-04`** — Kunci `pg_advisory_xact_lock(1346458697, 1313821261)` diambil di transaksi
   bayi sesudah kunci idempotency dan kasus, dilepas saat transaksi selesai; tidak ada jalur yang
   mengambil kunci itu lalu kunci bayi, sehingga tidak ada siklus *deadlock*. (T10, audit kunci)
5. **`PAT-MIG3A-AC-05`** — Normal↔normal, normal↔bayi, dan bayi↔bayi bersamaan tidak pernah
   menghasilkan nomor ganda maupun nomor di bawah lantai. (T7, T8, T9)
6. **`PAT-MIG3A-AC-06`** — Semantik idempotency bayi tidak berubah: kunci sama + data sama → hasil
   sebelumnya tanpa nomor baru; kunci sama + data berbeda → `409`. (T15)
7. **`PAT-MIG3A-AC-07`** — Semantik urutan kelahiran per kasus tidak berubah. (T16)
8. **`PAT-MIG3A-AC-08`** — Pemeriksa melaporkan `normalPatientCreateProtected`,
   `newbornPatientCreateProtected`, dan mode alokasi kedua jalur; `overallGuardReady` hanya `true` bila
   guard aktif, konfigurasi sah, kedua jalur `RESERVED_FLOOR`, dan kedua lantai terpenuhi; tetap
   baca-saja, khusus Staging, `Patient : Update`, tanpa data pribadi. (T11, T12, T13, T14)
9. **`PAT-MIG3A-AC-09`** — Bukti provider-real PostgreSQL 15 sekali pakai untuk AC-01–08, kontrak B15.
   Server tidak tersedia saat bukti diminta → test gagal, tidak dilewati.
10. **`PAT-MIG3A-AC-10`** — Seluruh test `BE-PAT-MIG-003` dan `BE-PAT-MIG-001` tetap lulus (T17); build
    Release `0` error; QBE `Strict` `PASS`.
11. **`PAT-MIG3A-AC-11`** — Tidak ada EF migration baru; `has-pending-model-changes` tanpa perubahan.
12. **`PAT-MIG3A-AC-12`** **(runtime, `PAT-GATE-005` generasi 2)** — Sesudah deploy ke Staging dengan
    `Enabled = true`, pemeriksa melaporkan `environment = Staging`, `guardState = ACTIVE`,
    `guardEnabled = true`, `guardConfigurationValid = true`, `normalPatientCreateProtected = true`,
    `newbornPatientCreateProtected = true`, kedua lantai terpenuhi, dan `overallGuardReady = true`.

#### Definition of Done — fase implementasi

- [x] Keputusan `PAT-DEC-069`–`074` dikunci pemilik — 19 Oktober 2026
- [x] Source sesuai kontrak `1.3.0` B14.2 dan B14.4
- [x] Build Release `0` error, QBE `Strict` `PASS`, test SQLite `189` lulus / `0` gagal / `16` dilewati (19 Oktober 2026)
- [ ] Uji PostgreSQL provider-real lulus (`PAT-MIG3A-AC-09`)
- [x] Tidak ada EF migration baru
- [x] Laporan task di `../task/report/backend/BE-PAT-MIG-003A.md`
- [ ] Source direview pemilik/leader
- [ ] `PAT-GATE-005` generasi 2 diminta terpisah sesudah review dan deploy

---

## Register status

| Task | Judul | Status | Laporan |
| --- | --- | --- | --- |
| `BE-PAT-MIG-001` | RSMMC Pilot MRN Reconciliation | ✅ 9 Oktober 2026 — **SOURCE IMPLEMENTATION: APPROVED** (review source leader `APPROVED`); **RUNTIME: PENDING** (`PAT-GATE-001` terbuka). Riwayat: 🟡 9 Oktober 2026 — 28 dari 28 AC terbukti di source/test pada baseline Integration `179aea3f` (test `73/73`, build `0` error, QBE `Strict` `PASS`); review source belum; runtime belum (`PAT-GATE-001`). **Catatan 14 Oktober 2026:** pemilik melaporkan rekonsiliasi Pilot `CLOSED/PASS`; source ada di Integration lewat commit `ef3adbe4`. Rincian bukti runtime belum tercatat di laporan — lihat `PAT-STALE-002` pada manifest | [BE-PAT-MIG-001](../task/report/backend/BE-PAT-MIG-001.md) |
| `BE-PAT-MIG-002` | RSMMC Resumable Bulk Patient Migration Worker | ⛔ **`approved`** 15 Oktober 2026 (`PAT-DEC-033`), diamandemen kontrak `1.3.0` 19 Oktober 2026 — menunggu `BE-PAT-MIG-003A` ✅ dan `PAT-GATE-005` generasi 2 `PASS` (`PAT-DEC-069`); `BE-PAT-MIG-003` ✅ sudah terpenuhi. Belum ada source. Riwayat: draft 14 Oktober 2026, diamandemen dua kali 15 Oktober 2026 | Belum ada |
| `BE-PAT-MIG-003` | RSMMC Normal Patient Identifier Reservation Guard | ✅ **SELESAI** 19 Oktober 2026 — source di-*merge* pemilik ke Integration (PR #290: `a9f2fda2` dokumen, `ab45dd3c` source, merge `3c457ab4`); dideploy ke DEV dan STAGING dengan `RsmmcMigrationReservationGuard__Enabled = true`, health `PASS`; **`PAT-GATE-005` generasi 1 `PASS`** (dilaporkan pemilik, `PAT-FACT-090`): `guardState = ACTIVE`, `PAT-RSMMC-705250`, `00-89-01-88`, `overallGuardReady = true` — memenuhi `PAT-MIG3-AC-12`. Bukti build 17 Oktober 2026: `dotnet build` `0` error; test `143` lulus / `0` gagal / `7` dilewati; PostgreSQL 15.15 provider-real `10`/`10` PASS (dijalankan pemilik); QBE `Strict` `PASS`; tanpa EF migration. **Catatan:** generasi 1 hanya membuktikan `CreatePatient`; jalur bayi `POST /newborns` (upstream `3a0189c2`) ditutup task terpisah `BE-PAT-MIG-003A`. *Riwayat: 🟡 siap review pemilik 17 Oktober 2026* Riwayat: `approved` 15 Oktober 2026 (`PAT-DEC-033`), amandemen `PAT-DEC-067` 16 Oktober 2026 | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `BE-PAT-MIG-003A` | Newborn Identifier Reservation Guard | 🟡 **SOURCE SIAP — MENUNGGU BUKTI PROVIDER-REAL DAN REVIEW PEMILIK** 19 Oktober 2026 — di atas Integration `c6537ee4` (belum di-*commit*); `dotnet build -c Release` `0` error, `0` warning dari berkas yang diubah (298 warning lama di berkas lain); `dotnet test` PatientManagementTests `189` lulus / `0` gagal / `16` dilewati — `46` kasus baru `BE-PAT-MIG-003A` lulus, `70` regresi `BE-PAT-MIG-003` dan `73` regresi `BE-PAT-MIG-001` lulus, `16` dilewati = test provider-real (7 lama + 9 baru) tanpa server; QBE `Strict` `PASS`; `has-pending-model-changes` tanpa perubahan. **Belum:** uji PostgreSQL 15 provider-real `PAT-MIG3A-AC-09` (server tidak tersedia di sesi agent — dijalankan pemilik); review source; `PAT-MIG3A-AC-12` milik runtime `PAT-GATE-005` generasi 2. Laporan: [laporan](../task/report/backend/BE-PAT-MIG-003A.md) | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |

---

## Gerbang dan butir terbuka

### Keputusan review — seluruhnya `APPROVED` 8 Oktober 2026

Pada `roadmap_revision` `1`, `PAT-OQ-001` sampai `PAT-OQ-004` masih `PROPOSED`. Seluruhnya kini
final, dan tiga keputusan baru ditambahkan. **Jumlah pertanyaan terbuka: 0.**

| ID | Keputusan final | Menahan apa |
| --- | --- | --- |
| `PAT-OQ-001` | Project xUnit khusus `Tests/QuilvianSystemBackend.PatientManagementTests/`, tanpa entri `.sln`, dengan governance guard | Tidak menahan. Bersyarat pada pemeriksaan `TEST_POLICY` saat build; bila ditolak, hanya bagian test yang berhenti dan keputusan diminta |
| `PAT-OQ-002` | Hanya `Staging` persis, untuk kedua mode `dryRun` | Tidak menahan |
| `PAT-OQ-003` | `dryRun` yang tidak dikirim = `true`; tidak ada default implisit `false` | Tidak menahan |
| `PAT-OQ-004` | Pasien kanonik tidak dihitung dalam `limit`; `QR_INCONSISTENT` dilaporkan, tidak diperbaiki | Tidak menahan |
| `PAT-OQ-005` | Path QR tujuan diperiksa sebelum helper; bila ada, `CONFLICT` tanpa menimpa | Tidak menahan |
| `PAT-OQ-006` | Daftar izin positif yang gagal tertutup; bukan `!environment.IsProduction()` saja | Tidak menahan |
| `PAT-OQ-007` | Tidak ada EF migration atau perubahan skema untuk membaca `migration.rsmmc_*` | Tidak menahan |

### Gerbang sesudah task

| ID | Gerbang | Pemilik | Keadaan |
| --- | --- | --- | --- |
| `PAT-GATE-001` | Otorisasi eksplisit untuk deployment staging, pemanggilan endpoint, dan eksekusi rekonsiliasi 715 pasien | Pemilik/leader PatientManagement | ~~**Terbuka.** Baru boleh diminta sesudah `BE-PAT-MIG-001` lulus review source, build, dan test. Pembaruan 9 Oktober 2026: prasyarat itu kini terpenuhi (review source leader `APPROVED`), tetapi otorisasi eksekusinya **belum** diberikan — runtime tetap `PENDING`~~ → **`CLOSED/PASS`** — dilaporkan pemilik 14 Oktober 2026 pada sesi perencanaan `BE-PAT-MIG-002`. Agent tidak memverifikasinya; rincian bukti runtime belum tercatat (`PAT-STALE-002`) |
| `PAT-GATE-002` | Runtime Canary-A `BE-PAT-MIG-002`: (a) deployment Staging + simulasi; (b) eksekusi `dryRun=false`, audit, dan eksekusi ulang | Pemilik/leader PatientManagement | **Belum dapat diminta.** Prasyarat: `PAT-DEC-033`, `BE-PAT-MIG-002` ✅ untuk source, review source leader, **`PAT-GATE-005` generasi 2 `PASS`**, baseline `MstPatient` tetap yang diukur read-only dan disetujui (`PAT-DEC-077`), manifest eksekusi `RSMMC-CANARY-A` v1 (`PAT-DEC-056`), bukti pembekuan yang menutup keempat jalur pembuatan pasien — normal, admin, kiosk, dan bayi (`PAT-DEC-055`, `PAT-DEC-073`, `PAT-SRC-027`), inventaris filesystem read-only (`PAT-DEC-062`), dan bukti persistensi provider-real PostgreSQL 15 sekali pakai (`PAT-DEC-065`, dari build) sebelum (b). Urutan 12 langkah: `00-interview-decisions.md` bagian 12; urutan lintas task: bagian 13.3 |
| `PAT-GATE-003` | Desain dan runtime Canary-B (285 baris crosswalk historis) | Pemilik/leader PatientManagement | **Dicadangkan** — dirancang sesudah `PAT-GATE-002` `PASS`; bergantung `PAT-OQ-013`, `PAT-OQ-014` |
| `PAT-GATE-004` | Gelombang yang lebih besar dari canary | Pemilik/leader PatientManagement | **Dicadangkan** — sesudah `PAT-GATE-002` dan `PAT-GATE-003` `PASS`; bergantung `PAT-OQ-015`, `PAT-OQ-026` |
| `PAT-GATE-005` | Deployment Staging dan verifikasi runtime guard reservasi nomor: generasi 1 untuk `BE-PAT-MIG-003`, generasi 2 untuk `BE-PAT-MIG-003A` | Pemilik/leader PatientManagement | **Generasi 1 `PASS`** — 19 Oktober 2026, dilaporkan pemilik (`PAT-FACT-090`): dideploy ke DEV dan STAGING, `Enabled = true`, health `PASS`, `guardState = ACTIVE`, `PAT-RSMMC-705250` / `00-89-01-88`, `overallGuardReady = true`. Generasi 1 hanya membuktikan `CreatePatient`. **Generasi 2 — belum dapat diminta.** Prasyarat: `BE-PAT-MIG-003A` ✅ untuk source — termasuk uji provider-real `PAT-MIG3A-AC-09` — dan review pemilik; otorisasi deployment terpisah; migration `20261016080745_ReviseNewbornAdmissionFlow` sudah diterapkan di Staging. Lulus bila pemeriksa di Staging persis melaporkan `environment = Staging`, `guardState = ACTIVE`, `guardEnabled = true`, `guardConfigurationValid = true`, `normalPatientCreateProtected = true`, `newbornPatientCreateProtected = true`, kedua lantai terpenuhi, dan `overallGuardReady = true` (`PAT-MIG3A-AC-12`, `PAT-DEC-072`) — tanpa membuat pasien untuk pengujian. Pembuatan pasien Staging tetap dibekukan selama persiapan dan jendela Canary-A (`PAT-DEC-073`); sesudah generasi 2 `PASS`, baseline `MstPatient` baru diukur read-only (`PAT-DEC-077`). Gerbang ini juga prasyarat **build** `BE-PAT-MIG-002` (urutan wajib pemilik). *Keadaan lama — jejak: "Belum dapat diminta. Keadaan 16 Oktober 2026: `BE-PAT-MIG-003` 🟡 …"* |

### Keputusan `BE-PAT-MIG-002` dan `BE-PAT-MIG-003` — keadaan per sesi ketiga 15 Oktober 2026

Rinciannya ada di `00-interview-decisions.md` bagian 10. **Jumlah pertanyaan `BLOCKING`: 0.**

| ID | Topik | Keadaan |
| --- | --- | --- |
| `PAT-OQ-008`–`014`, `016`, `018`, `021`–`030`, `032`–`035` | 23 pertanyaan, termasuk P0 strategi QR (`028`) dan reservasi `PatientCode` (`030`) | `OWNER_APPROVED` — `PAT-DEC-034`–`037`, `PAT-DEC-044`–`062` |
| `PAT-OQ-019` | Skema fisik tabel migrasi | `OWNER_APPROVED` — `PAT-DEC-063`. *Riwayat: `BLOCKING` sampai sesi kedua* |
| `PAT-OQ-031` | **P0** — perlindungan reservasi MRN | `OWNER_APPROVED` — `PAT-DEC-064`, lantai 890.188. *Riwayat: `BLOCKING` sampai sesi kedua* |
| `PAT-OQ-036` | Cara bukti persistensi provider-real `IsMember = false` | `OWNER_APPROVED` — `PAT-DEC-065`, PostgreSQL 15 sekali pakai. *Riwayat: `BLOCKING` sesi kedua* |
| `PAT-OQ-037` | Cakupan environment `BE-PAT-MIG-003` | `OWNER_APPROVED` — `PAT-DEC-066`, `Staging` persis dan `Enabled == true`. *Riwayat: `BLOCKING` sesi kedua* |
| `PAT-OQ-015`, `017`, `020` | QR tujuan terisi, `IsMember`/`IsActive`, pengaman pembangkit normal | `SUPERSEDED` |

Hasil `requirement-completeness-gate`: `PASS` untuk `BE-PAT-MIG-003` lalu `BE-PAT-MIG-002`
(`requirement-completeness-assessment.md`). Usulan standar `PAT-GAP-002`–`016` dikunci lewat
`PAT-DEC-033`.

### Gap dokumen

| ID | Gap | Keadaan |
| --- | --- | --- |
| `PAT-GAP-001` | Blueprint ini hanya memuat berkas minimum untuk satu work item. 11 dari 14 berkas pasti bentuk `SINGLE` belum dibuat — rinciannya di `blueprint-manifest.md` | **Terbuka**, sengaja. Tidak menahan `BE-PAT-MIG-001`, tetapi `verify-module-readiness` untuk modul PatientManagement secara utuh akan mencatatnya. Revision `3` meneruskan paket minimum untuk `BE-PAT-MIG-002`: arsitektur, alur, dan kamus data dicatat di kontrak Bagian B dan kartu task |

---

## Risiko

| ID | Risiko | Penanganan yang diwajibkan | Pemilik |
| --- | --- | --- | --- |
| `PAT-RSK-001` | Tabel `migration.rsmmc_*` belum dipetakan di source. Menambah entity EF biasa dapat memunculkan perubahan model yang menuntut EF migration | Diputuskan `PAT-OQ-007`: tidak ada EF migration dan tidak ada perubahan skema. Mekanisme baca berparameter dipilih saat build; bukti V11 | Developer build |
| `PAT-RSK-002` | `SavePatientQrCodeFile` menimpa file yang sudah ada (baris 1144) | Diputuskan `PAT-OQ-005`: periksa path tujuan sebelum helper dipanggil; bila ada, `CONFLICT`. Sisa risiko: jeda singkat antara pemeriksaan dan penulisan — cara menutupnya `DEV_DISCRETION`, dicatat pada laporan | Developer build |
| `PAT-RSK-003` | Pola `!environment.IsProduction()` di source gagal terbuka | Diputuskan `PAT-OQ-006`: daftar izin positif berisi `Staging` saja (`AC-05`, `PAT-OQ-002`) | Developer build |
| `PAT-RSK-004` | Test berbasis EF Sqlite belum tentu dapat meniru skema `migration` PostgreSQL atau query khusus PostgreSQL | Desain test dan batasannya dicatat apa adanya pada laporan. Topik `AC-27` yang tidak dapat dibuktikan disebut terbuka, bukan ditandai lulus | Developer build |
| `PAT-RSK-005` | Pilihan A pada `api-contract.md` bagian 7 membuka kemampuan ubah MRN massal bagi setiap pemegang izin `Patient : Update` di staging | Rekomendasi pilihan B; pilihan akhir dicatat pada laporan | Pemilik/leader |
| `PAT-RSK-006` | Dua admin memanggil endpoint pada waktu hampir bersamaan, sehingga pasien yang sama berpotensi diproses dua kali | Syarat `AC-01` diperiksa ulang di dalam transaksi per pasien sebelum mengubah data | Developer build |
| `PAT-RSK-007` | `TEST_POLICY` menyatakan folder `Tests/` sudah dihapus, padahal branch ini masih men-*track* 40 berkas di tiga project test yang tidak terdaftar di `.sln` | Dicatat sebagai selisih governance. Tidak diubah oleh task ini. Project test baru dibuat hanya lewat governance guard `PAT-OQ-001` | Pemilik suite Skill |
| `PAT-RSK-008` | Nama environment pada server staging yang berjalan tidak diperiksa. Bila ejaannya bukan persis `Staging` — misalnya `staging` — endpoint akan menolak di sana | Gagal tertutup, sehingga aman. Terlihat pada simulasi pertama sesudah `PAT-GATE-001`. Perbaikannya ada di konfigurasi deployment, **bukan** dengan melonggarkan pencocokan | Pemilik/leader, saat `PAT-GATE-001` |

### Risiko `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`

| ID | Risiko | Penanganan yang diusulkan | Pemilik |
| --- | --- | --- | --- |
| `PAT-RSK-009` | **Pembuatan ganda karena balapan.** Dua admin atau dua instance memproses pasien yang sama hampir bersamaan | Tiga lapis B5: kunci database per pasien + baca ulang sesudah kunci; kunci utama crosswalk; indeks unik `PatientCode`/MRN. Kunci dua bagian tanpa menunggu (`PAT-DEC-048`) | Developer build |
| `PAT-RSK-010` | **Filesystem dan database tidak atomik.** Berkas QR dan *commit* tidak dapat dijadikan satu transaksi | Urutan B4.3: QR dibuat paling akhir sebelum *commit*; tidak pernah *commit* bila QR tidak pasti; pembersihan hanya dengan bukti kepemilikan; QR yatim dikenali panggilan berikutnya sebagai kasus G | Developer build |
| `PAT-RSK-011` | **Hasil *commit* tidak pasti** | `COMMIT_OUTCOME_UNCERTAIN`: QR dipertahankan, tanpa *retry*, berhenti; panggilan berikutnya menyelesaikan lewat crosswalk | Developer build; operator |
| `PAT-RSK-012` | **Preview basi.** Preview diaudit pada satu titik waktu, sedangkan `MstPatient` dan perencana dapat berubah sesudahnya | Gerbang B4.2 di setiap panggilan, baca ulang per pasien sesudah kunci, `PLANNER_MISMATCH`/`PREVIEW_STATE_CHANGED` | Developer build |
| `PAT-RSK-013` | **P0 — pergeseran target oleh pembangkit normal, terbukti.** Pendaftaran pasien biasa di Staging memakai kode/MRN "kosong terkecil" yang dicadangkan preview: berikutnya `PAT-RSMMC-00742` dan `00-00-00-27` (`PAT-FACT-072`, `073`), dan sesudah migrasi penuh `PAT-RSMMC-101416` milik pasien tertunda (`PAT-SRC-006`) | Lantai `PatientCode` 705.250 (`PAT-DEC-054`) dan lantai MRN 890.188 (`PAT-DEC-064`) di `BE-PAT-MIG-003`, aktif di `Staging` persis dengan `Enabled == true` (`PAT-DEC-066`); pembekuan pendaftaran Staging berlaku (`PAT-DEC-055`) dan harus menutup ketiga jalur (`PAT-RSK-032`); gerbang `NORMAL_ALLOCATION_GUARD` menuntut `BE-PAT-MIG-003` terverifikasi | Pemilik/leader |
| `PAT-RSK-014` | **285 baris crosswalk historis tertimpa** | Build pertama tidak punya jalur `UPDATE` crosswalk; kasus B menjadi `CONFLICT`; Canary-B dirancang terpisah (`PAT-DEC-027`, `PAT-OQ-013`) | Developer build; pemilik |
| `PAT-RSK-015` | **`quilvian_patient_id` tidak unik di database** (`PAT-FACT-053`) | Gerbang `CROSSWALK_DUPLICATE_TARGET` dan pemeriksaan per pasien `DUPLICATE_TARGET_MAPPING` | Developer build |
| `PAT-RSK-016` | **Percobaan ulang sesudah proses mati** | Keadaan dibaca ulang dari crosswalk dan `MstPatient`, bukan dari memori; pasien yang sudah *commit* menjadi `ALREADY_MIGRATED`; QR yatim menjadi kasus G | Developer build; operator |
| `PAT-RSK-017` | **Populasi sangat besar (704.506).** Waktu proses, ±704 ribu folder QR, biaya gerbang per panggilan, batas waktu HTTP | Build pertama hanya Canary-A; tidak ada pemuatan seluruh preview ke memori; mekanisme gelombang diputuskan di `PAT-GATE-004` (`PAT-DEC-061`) | Pemilik/leader |
| `PAT-RSK-018` | **P0 — 715 tabrakan namespace QR historis, terbukti** (`PAT-FACT-070`); 10 di antaranya pada kandidat Canary-A (`PAT-FACT-071`). `CONFLICT` generik memblokir 715 pasien sah; menimpa melanggar `PAT-DEC-039` | Namespace `patient-qrcodes/rsmmc-bulk/<MRN>/` untuk seluruh pasien bulk (`PAT-DEC-052`, didukung scan konsumen `PAT-SRC-019`–`022`); simulasi membaca path rencana dan path historis (`PAT-DEC-046`); inventaris folder wajib (`PAT-DEC-062`). *Penanganan lama "`CONFLICT` tanpa adopsi" (`PAT-OQ-015`) digantikan* | Pemilik/leader |
| `PAT-RSK-019` | **`IsMember = false` tersimpan `true`** lewat EF (`PAT-SRC-008`) — mengenai **seluruh** 704.506 baris (`PAT-FACT-074`). *Dikoreksi 15 Oktober 2026: `IsActive` tidak terdampak karena seluruh preview `IsActive = true`* | Penulisan eksplisit dan verifikasi baca ulang (`PAT-DEC-053`); bukti provider-real PostgreSQL 15 sekali pakai (`PAT-DEC-065`, kontrak B15); test persistensi wajib (`PAT-MIG2-AC-47`); audit fisik runtime | Developer build; pemilik |
| `PAT-RSK-020` | **SQL tidak memakai indeks dan urutan negatif salah** bila komponen Pilot dipakai ulang (`PAT-SRC-010`, `PAT-SRC-011`) | Parameter bertipe sesuai skema fisik `PAT-FACT-079`–`083`; urutan `bigint` numerik; gerbang `PREVIEW_SCHEMA_SIGNATURE` (`PAT-DEC-063`, `PAT-MIG2-AC-51`) | Developer build |
| `PAT-RSK-021` | **Kebocoran data pribadi** — preview memuat NIK, telepon, dan alamat | Tidak ada di log; respons hanya tanda ada/tidak (`PAT-DEC-049`) | Developer build |
| `PAT-RSK-022` | ~~**Tabrakan angka `hashtext`** — dua kunci berbeda jatuh ke angka 32-bit yang sama~~ | **Dicabut** amandemen kedua 15 Oktober 2026: kunci dua bagian `PAT-DEC-048` memakai `legacy_pid` persis sebagai key2, sehingga tidak ada tabrakan di dalam namespace worker. Sisa risikonya dicatat sebagai `PAT-RSK-035` | — |
| `PAT-RSK-023` | **Pasien ganda pada 22 baris historis berisiko** (`PAT-FACT-055`) | Ditahan dari Canary-B (`PAT-DEC-059`) | Pemilik proses identitas pasien |
| `PAT-RSK-024` | **Ejaan environment Staging** — sama dengan `PAT-RSK-008` | Gagal tertutup; terlihat pada simulasi pertama | Pemilik/leader |
| `PAT-RSK-025` | **Keterbatasan test SQLite** — tidak dapat membuktikan advisory lock, SQL PostgreSQL, atau perilaku default kolom yang sebenarnya | Bukti provider-real PostgreSQL 15 sekali pakai untuk `IsMember` dan kunci allocator (`PAT-DEC-065`, kontrak B15); perilaku lain yang tidak terbukti di SQLite dicatat apa adanya dan dibuktikan pertama kali pada simulasi Canary-A | Developer build |
| `PAT-RSK-026` | **P0 — pendaftaran normal dapat menimpa QR historis Pilot.** Bila `PAT-INF-001` benar, MRN normal berikutnya `00-00-00-27` adalah MRN sementara Pilot, dan `SavePatientQrCodeFile` menulis dengan `File.WriteAllBytes` (`PAT-SRC-016`) — menimpa bukti yang wajib utuh (`PAT-DEC-039`) | Pembekuan pendaftaran di Staging dinyatakan berlaku (`PAT-DEC-055`) dan harus menutup ketiga jalur (`PAT-RSK-032`); lantai MRN 890.188 (`PAT-DEC-064`). `PAT-INF-001` terkonfirmasi (`PAT-FACT-089`) | Pemilik/leader |
| `PAT-RSK-027` | Seeder demo Operating Room menambah satu `MstPatient` bila `Seeders:RunOperatingRoomDemoSeed` aktif (`PAT-SRC-023`), sehingga `PATIENT_BASELINE` gagal walau tidak ada pelanggaran reservasi | Pengaturan itu dipastikan mati di Staging selama jendela migrasi, atau rumus baseline mencatat pasien demo secara eksplisit | Pemilik/leader |
| `PAT-RSK-028` | `PAT-OQ-029` pilihan B mengubah perilaku pendaftaran normal: pasien yang didaftarkan tanpa `isMember` kini tersimpan `true`, sesudahnya `false` (`PAT-SRC-024`) | **Dihindari:** pemilik memilih pilihan A (`PAT-DEC-053`); konfigurasi EF global tidak diubah | Pemilik/leader |
| `PAT-RSK-029` | Guard reservasi menyentuh alur pendaftaran normal — kiosk, admin, dan rute biasa — sehingga dampaknya meluas ke luar migrasi | Task dan review terpisah (`PAT-DEC-057`, `BE-PAT-MIG-003`); jalur insert hanya satu (`PAT-SRC-023`) | Pemilik/leader |
| `PAT-RSK-030` | Allocator sadar reservasi yang membaca tabel `migration.*` membuat pendaftaran normal bergantung pada tabel migrasi — di environment tanpa tabel itu, atau sesudah tabelnya dibersihkan | **Dihindari:** allocator memakai lantai dan tidak membaca tabel `migration.*` saat runtime — `PAT-DEC-054` untuk `PatientCode`, `PAT-DEC-064` untuk MRN | Developer build |
| `PAT-RSK-031` | Path QR pasien bulk (`patient-qrcodes/rsmmc-bulk/<MRN>/`) berbeda dari pola pasien normal. Skrip operasional atau cadangan di luar repository yang mengandaikan pola lama tidak diperiksa | Konsumen di dalam repository terbukti memakai path tersimpan (`PAT-SRC-019`, `020`); pemilik memeriksa skrip operasionalnya | Pemilik/leader |
| `PAT-RSK-032` | **Pembekuan yang tidak menutup jalur kiosk.** `POST /` dan `POST /kiosk` hanya dijaga policy `KioskRead`; mencabut izin `Patient : Create` hanya menutup `POST /admin` (`PAT-SRC-027`). Bila pembekuan saat ini dilakukan dengan cara itu, reservasi `PAT-RSMMC-00742` dan `00-00-00-27` masih terbuka lewat kiosk | Pemilik memastikan ketiga jalur tertutup — misalnya menonaktifkan akun kiosk dan membatasi peran yang lolos `KioskRead` — dan menyerahkan buktinya sebelum `PAT-GATE-002` | Pemilik/leader |
| `PAT-RSK-033` | **NIK tersalin ke crosswalk.** `normalized_nik` diisi dari preview (`PAT-DEC-047`), sehingga data sensitif kini ada di dua tabel migrasi | Tidak masuk respons maupun log; tidak dibaca untuk mencocokkan pasien (`PAT-MIG2-AC-49`); akses skema `migration` dibatasi pemilik | Pemilik/leader; developer build |
| `PAT-RSK-034` | **Perubahan penomoran Produksi.** Bila guard `BE-PAT-MIG-003` aktif global, deploy ke Produksi mengubah nomor pasien normal Produksi menjadi mulai `PAT-RSMMC-705250`, dan guard MRN sadar reservasi bergantung pada tabel migrasi yang mungkin tidak ada di sana | **Dihindari:** guard hanya aktif di `Staging` persis dengan `Enabled == true` (`PAT-DEC-066`); di luar itu algoritma lama tanpa perubahan hasil (`PAT-MIG3-AC-08`) | Pemilik/leader |
| `PAT-RSK-035` | **key1 kunci worker dipakai pihak lain kelak.** Nilai `1173286842` (`0x45EEEFBA`) dicadangkan untuk worker; bila pemakai kunci dua bagian lain di masa depan memilih key1 yang sama, keduanya saling menahan | Nilai key1 dicatat di kontrak B4.3 dan diperiksa pada review source setiap pemakai kunci dua bagian baru; saat ini tidak ada pemakai lain (`PAT-SRC-026`) | Developer build; reviewer |
| `PAT-RSK-036` | ~~**Guard dimatikan di Staging.** `Enabled = false` yang ditulis eksplisit mematikan guard (`PAT-DEC-066`), sehingga pendaftaran normal kembali ke nomor kosong terkecil dan dapat memakai reservasi lagi~~ | **Dihindari** 16 Oktober 2026: `PAT-DEC-067` — `Enabled = false` di Staging persis gagal tertutup. Sisa risiko: konfigurasi yang belum diisi sesudah deploy menolak seluruh pendaftaran normal Staging (`503`) sampai pemilik mengisinya — disengaja, dan pembekuan `PAT-DEC-055` masih berlaku sampai `PAT-GATE-005` | Pemilik/leader |
| `PAT-RSK-037` | **Nilai preview baru terlihat saat runtime.** Tipe `full_name`/`birth_date` yang berbeda, atau NULL/nilai tidak dikenal pada kolom enum dan boolean, baru terlihat ketika SQL bertipe membaca Staging | Gerbang `PREVIEW_SCHEMA_SIGNATURE` gagal tertutup sebelum pasien mana pun diproses; `PREVIEW_VALUE_INVALID` pada simulasi Canary-A, yang tidak pernah berhenti; tidak ada pemetaan otomatis; perubahan perlakuan lewat revisi blueprint (`PAT-MIG2-AC-51`) | Developer build; pemilik |
| `PAT-RSK-038` | **Server PostgreSQL 15 sekali pakai tidak tersedia saat build.** Bukti provider-real wajib tidak dapat dihasilkan | Test gagal, tidak dilewati; task tidak ditandai ✅ (tetap 🟡) sampai bukti tersedia; **tidak** ada fallback ke Staging bersama, Produksi, atau SQLite saja (`PAT-DEC-065`) | Pemilik; developer build |
| `PAT-RSK-039` | **Bukti provider-real `BE-PAT-MIG-003A` belum dijalankan di sesi build.** Server PostgreSQL 15 sekali pakai tidak tersedia di sesi agent 19 Oktober 2026; kunci baru dan konkurensi normal↔bayi baru terbukti lewat test SQLite (cabang, lantai, urutan) dan penelusuran source | Task tetap 🟡 sampai pemilik menjalankan `--filter Kategori=ProviderReal` dan hasilnya dicatat (`PAT-MIG3A-AC-09`); sama dengan pola `BE-PAT-MIG-003` | Pemilik |
| `PAT-RSK-040` | **Di luar Staging persis, pendaftaran normal dan bayi tetap tidak saling diserialkan.** Pendaftaran normal memakai nomor kosong terkecil, pendaftaran bayi `NumberSeries` — seperti sebelum task; tabrakan ditangkap indeks unik (`409` pada jalur bayi) | Dipertahankan sengaja (`PAT-DEC-070` b): kampanye migrasi tidak boleh mengubah perilaku Produksi. Penyatuan penomoran adalah keputusan bisnis terpisah | Pemilik/leader PatientManagement |
| `PAT-RSK-041` | **Jalur pembuatan pasien baru di masa depan.** Pemeriksa melaporkan keputusan cabang dua jalur yang dikenal; jalur insert `MstPatient` lain yang ditambahkan kelak tidak otomatis terlihat | Inventaris jalur insert diulang pada setiap impact scan sebelum `BE-PAT-MIG-002` dan sebelum setiap gelombang (`PAT-SRC-031`) | Pemilik/leader PatientManagement |
| `PAT-RSK-042` | **Pendaftaran bayi di Staging sebelum `BE-PAT-MIG-003A` dideploy.** Bila build yang memuat `3a0189c2` sudah berjalan di Staging, bayi yang sempat didaftarkan dapat memakai nomor cadangan (misalnya `PAT-RSMMC-00742`) | Pemeriksaan read-only pemilik sebelum Canary-A atas `MstPatient` dengan `NewbornRegistrationKeyHash IS NOT NULL`; gerbang `PREVIEW_CODE_UNMAPPED` / `PREVIEW_MRN_UNMAPPED` gagal tertutup bila itu terjadi | Pemilik |
