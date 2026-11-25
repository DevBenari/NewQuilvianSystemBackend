# Backend Roadmap — Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Roadmap ID | `KSK-PO-RM-BE-001` |
| Revision | `1.1` — menambah `BE-KSK-PO-011` (revisi blueprint 2.1, `KSK-DEC-057`) |
| Status | `approved` — Sukma Giri Pratama, 2026-11-19 (`KSK-DEC-055`); r1.1 Sukma Giri Pratama, 2026-11-25 (`KSK-DEC-058`). Wewenang implementasi `GRANTED` untuk seluruh task; pembuatan dan penerapan migration tetap memerlukan konfirmasi terpisah |
| Blueprint | `KSK-BP-001` revisi `2`, `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), bentuk `SINGLE` |
| Cakupan | `EPIC KSK-PO-01` s.d. `KSK-PO-04` (sisi backend), rilis `MVP-0` s.d. `MVP-3` (`04-prd-to-mvp.md` PO-20). `POST-MVP` tidak direncanakan |
| SHA baseline | BE `5e777833` (`sukmagp`), FE `5f18e9f04` (`sukmagpV2`); r1.1: BE `356bf463`, FE `7bba5cd1c` |
| Kontrak masukan | `KSK-CONTRACT-v2` `approved`, bagian *Amendment PO*; r1.1: `KSK-CONTRACT-v2.1` `approved` (`KSK-DEC-058`, *Perubahan v2.1* pada `api-contract.md`, `KSK-VAL-PO-073`, `074`). Hash berkas sesudah approval: `api-contract.md` `1aca969a678d`, `state-transition-matrix.md` `2df4865bd7a1`, `validation-matrix.md` `f37f0f6b4746`, `integration-contract.md` `d8cab335cd07`, `permission-audit-matrix.md` `b031ae6f062d` |
| Masukan desain | `02-backend-architecture.md` PO.1–PO.15 (`0b8b3e654f26`), `data/data-dictionary.md` PO-D (`e5034ab703ae`), `04-prd-to-mvp.md` PO-10, PO-20 (`edccb4141345`), `testing/acceptance-test-matrix.md` PO-T (`d97bb3d86834`) |
| Owner | Sukma Giri Pratama |
| Nama berkas | `online-registration-backend-roadmap.md`, karena `roadmap/backend-roadmap.md` sudah dimiliki revisi 1 (`KSK-RM-BE-001`). Pola sama dengan `rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` |

> **Batas dokumen.**
> 1. Roadmap ini **bukan** izin menulis kode. Setiap task baru boleh dikerjakan lewat `build-module-backend` setelah roadmap disetujui dan task diminta secara eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan dokumen engineering canonical, bukan dari roadmap ini.
> 3. Migration `AddRegOnlineRegistration` di `BE-KSK-PO-001`: **pembuatan** dan **penerapan** ke `QuilvianNewDevSukma` masing-masing butuh konfirmasi terpisah dari pemilik (`CLAUDE.md`). `KSK-DEC-054` tidak mencakup izin itu.
> 4. Verifikasi memakai pola Bank Darah: build, `has-pending-model-changes`, QBE Strict, verifier otorisasi, runtime HTTP ke `QuilvianNewDevSukma` dengan data samaran yang dibuat lewat API (bukan eksekusi DB langsung). Tanpa folder `Tests/`. `dotnet test` ditulis `NOT RUN — tidak ada project test`.
> 5. Lingkungan dev memakai kunci uji resmi Cloudflare Turnstile dan SMTP uji lokal atau `Email:Smtp:Enabled = false`. Nilai rahasia tidak pernah ditulis ke berkas yang di-commit.
> 6. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-KSK-PO-001 ✅ ─> BE-KSK-PO-002 ✅ ─┬─> BE-KSK-PO-004 ✅ ─> BE-KSK-PO-005 ✅ ─┬─> BE-KSK-PO-006 ✅
                                      │                                         │
BE-KSK-PO-003 ✅ ─────────────────────┘                                         ├─> BE-KSK-PO-008 ✅
                                                                                │
                                                                                └─> BE-KSK-PO-009 ✅

BE-KSK-PO-006 ✅ ─> BE-KSK-PO-007 ✅

BE-KSK-PO-009 ✅ ─> BE-KSK-PO-010 ✅

BE-KSK-PO-005 ✅ ─> BE-KSK-PO-011 ✅
```

Blok kedua s.d. keempat menyambung rantai dari `BE-KSK-PO-006`, `BE-KSK-PO-009`, dan `BE-KSK-PO-005` pada blok pertama, karena satu baris tidak muat 100 karakter.

Pasangan prasyarat → task: 10 (`001→002`, `002→004`, `003→004`, `004→005`, `005→006`, `005→008`, `005→009`, `006→007`, `009→010`, `005→011`), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-KSK-PO-001`, `BE-KSK-PO-003` — boleh paralel |
| 2 | `BE-KSK-PO-001` | `BE-KSK-PO-002` |
| 3 | `BE-KSK-PO-002`, `BE-KSK-PO-003` | `BE-KSK-PO-004` |
| 4 | `BE-KSK-PO-004` | `BE-KSK-PO-005` |
| 5 | `BE-KSK-PO-005` | `BE-KSK-PO-006`, `BE-KSK-PO-008`, `BE-KSK-PO-009`, `BE-KSK-PO-011` (r1.1) — boleh paralel |
| 6 | `BE-KSK-PO-006` / `BE-KSK-PO-009` | `BE-KSK-PO-007`, `BE-KSK-PO-010` |

**Gelombang rilis produk** (`04-prd-to-mvp.md` PO-20) berbeda dari gelombang teknis:

| Rilis | Task | Catatan |
| --- | --- | --- |
| `MVP-0` | `BE-KSK-PO-001`, `002`, `008` | `008` secara teknis menunggu `005`, karena data uji penutupan hanya boleh dibuat lewat API pendaftaran. Rilisnya bersama `MVP-1` |
| `MVP-1` | `BE-KSK-PO-003`, `004`, `005` | Development boleh jalan. Go-live menunggu `KSK-PO-OQ-010`, `015`, `016` |
| `MVP-2` | `BE-KSK-PO-006`, `007` | — |
| `MVP-3` | `BE-KSK-PO-009`, `010` | Boleh dikerjakan paralel dengan `MVP-2` sesudah `005` |
| `MVP-1` (r1.1) | `BE-KSK-PO-011` | Perubahan perilaku halaman publik (`KSK-DEC-057`); dirilis bersama `FE-KSK-PO-009` karena query `visitDate` wajib |

`BE-KSK-PO-002`, `007`, dan `008` sama-sama mengubah `PatientEncounterController.CreateEncounterCoreAsync`. Kerjakan berurutan atau pada branch yang sama agar tidak terjadi konflik di blok transaksi yang sama.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-KSK-PO-001` | Skema pendaftaran online dan migration | `FR-KSK-PO-001`; `KSK-DSN-PO-001`, `002`, `003`, `014` | PO-D.2–D.4, D.6 | `IdentityModel`, pola `RegPriorityReservation` | Model, enum, configuration, `DbSet`, opsi, `appsettings.json`, migration | — | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-002` | Kuota menghitung pendaftaran online dan dikunci per jadwal | `FR-KSK-PO-002`; `INV-PO-02`; `KSK-DSN-PO-009` | PO.4.3 | `DoctorScheduleQuotaService`, pola `pg_advisory_xact_lock` | `DoctorScheduleQuotaService`, `PatientEncounterController` | `BE-KSK-PO-001` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-003` | Pemeriksa captcha dan pengirim email | `FR-KSK-PO-013`; `KSK-DSN-PO-006`, `007` | `INT-PO-01`, `INT-PO-03` | `IHttpClientFactory`, `System.Net.Mail` | `Services/Security`, `Services/Notification`, `Program.cs`, `appsettings.json` | — | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-004` | Opsi publik dan pencocokan pasien lama | `FR-KSK-PO-010`, `011`, `014`; `KSK-DEC-031`, `KSK-DSN-PO-005`, `013` | Grup *Online Registration Public* (`GET options/*`, `POST patient-match`) | `PriorityReservationArrivalController`, `KioskPatientLookupService`, `MaskPatientName` | Controller publik, `OnlineRegistrationPublicService`, `PatientNameMasker`, dua policy rate limit | `BE-KSK-PO-002`, `BE-KSK-PO-003` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-005` | Kirim pendaftaran online | `FR-KSK-PO-012`; `KSK-DEC-030`, `034`, `035`, `038`, `042`, `043`, `047` | Grup publik `POST` | `NumberSeriesAllocator`, helper hash RP, `ReferralDocumentStorageService` (root dan validasi berkas) | `OnlineRegistrationPublicService`, controller publik, policy submit | `BE-KSK-PO-004` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-006` | Verifikasi check-in kiosk | `FR-KSK-PO-020`; `KSK-DEC-033`, `040` | Grup *Online Registration Check-In* | Pola `KioskPatientLookupController` | `OnlineRegistrationCheckInController`, `OnlineRegistrationCheckInService`, policy check-in | `BE-KSK-PO-005` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-007` | Check-in menjadi kunjungan | `FR-KSK-PO-021`; `KSK-DEC-036`, `039`, `046`, `048`; `KSK-DSN-PO-008`, `010` | `POST patient-encounters/kiosk` (diperluas) | Hook RP `PrepareCompletionAsync`/`CompleteInTransactionAsync` | `PatientEncounterController`, `PatientEncounterCreateRequest`, `OnlineRegistrationCheckInService` | `BE-KSK-PO-006` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-008` | Penutupan Jalur Lain dan Tidak Datang | `FR-KSK-PO-003`, `004`; `KSK-DEC-037`, `045`; `KSK-DSN-PO-015` | PO-S.1 | `CloseForEncounterAsync` RP, `OutpatientPriorityReservationClosureHostedService` | `OnlineRegistrationService`, hosted service, `PatientEncounterController`, `Program.cs` | `BE-KSK-PO-005` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-009` | Daftar, ringkasan, detail, berkas untuk petugas | `FR-KSK-PO-030`, `032` (Resource); `KSK-DEC-050`; `KSK-DSN-PO-017` | Grup *Online Registration* (`GET`) | `OutpatientPriorityReservationController#GetAll`, `summary`, detail | `OnlineRegistrationController`, `OnlineRegistrationService` | `BE-KSK-PO-005` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-010` | Ubah jadwal, ubah kontak, batal | `FR-KSK-PO-031`, `032`; `KSK-DEC-051`, `052`; `KSK-DSN-PO-016` | Grup *Online Registration* (`PUT`, `POST cancel`) | `CancelAsync` RP, `RowVersion` | `OnlineRegistrationController`, `OnlineRegistrationService` | `BE-KSK-PO-009` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `BE-KSK-PO-011` | Faskes perujuk mitra di halaman publik | `FR-KSK-PO-015`; `KSK-DEC-057`; `KSK-DSN-PO-018` | `v2.1`: `GET /public/options/referral-institutions` (`visitDate` wajib), `POST /public`; `KSK-VAL-PO-073`, `074` | `ReferralPartnerEligibility.IsEligibleOn`/`FindEligibleAsync` | `OnlineRegistrationPublicService` (opsi faskes, validasi kirim), controller publik (query), DTO opsi bila perlu | `BE-KSK-PO-005` | Tiga baris uji `KSK-DEC-057` di PO-T.1 | Runtime HTTP | Data perjanjian mitra uji. Pemilik: implementer | Kriteria terbukti; tanpa migration |

## Kartu task

### ✅ `BE-KSK-PO-001` — Skema pendaftaran online dan migration

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-19 — build PASS (0 error), `has-pending-model-changes` bersih, migration `20261119134155_AddRegOnlineRegistration` (3 tabel, 15 index, 11 FK `Restrict`, 1 check constraint) dibuat dan **diterapkan** ke `QuilvianNewDevSukma` atas izin terpisah (0 Pending sesudahnya), QBE Strict PASS (24 berkas). Delta: configuration di `Repositories/Configurations/HealthServices/`, section `HealthServices:Registration:OnlineRegistration`, 6 index FK otomatis — [laporan](../task/report/backend/BE-KSK-PO-001.md) |
| Outcome | `RegOnlineRegistration`, `RegOnlineRegistrationReferralDocument`, `RegOnlineRegistrationHistory`; lima enum baru; `EncounterRegistrationSource.OnlineRegistration = 7`, `ReferralCaptureSource.OnlineRegistration = 3`; tiga configuration; tiga `DbSet`; `OnlineRegistrationOptions` beserta kunci `Registration:OnlineRegistration`, `Turnstile`, dan `Email:Smtp` tanpa nilai rahasia; migration `AddRegOnlineRegistration` |
| Requirement/decision | `FR-KSK-PO-001`; `INV-PO-03`, `05`, `07`, `08`; `KSK-DSN-PO-001`, `002`, `003`, `014` |
| Kontrak | `data/data-dictionary.md` PO-D.2–D.4 dan DDL PO-D.6; `02-backend-architecture.md` PO.6, PO.9–PO.11 |
| Reuse | `IdentityModel`; pola configuration dan `RowVersion` dari `RegPriorityReservation` |
| Cakupan | `Areas/HealthServices/RegistrationManagement/Models/`, `Enums/`, `Repositories/Configurations/HealthServices/RegistrationManagement/`, `Repositories/ApplicationDbContext.cs`, `Services/OnlineRegistrationClosureOptions.cs` atau berkas opsi setara, `appsettings.json`, satu migration |
| Dependency | — |
| Acceptance criteria | (1) Kolom, tipe, panjang, bawaan, index (termasuk unique bersyarat `CodeHash` saat `Status = 1` dan `EncounterId` bukan null), check constraint kontak, dan FK `Restrict` persis kamus data. (2) `Down` menghapus ketiga tabel. (3) `has-pending-model-changes` bersih sesudah migration. (4) Build lulus. (5) `appsettings.json` tidak memuat `SecretKey` maupun `Password` berisi |
| Verifikasi | `dotnet ef migrations script` untuk migration ini dibandingkan dengan DDL PO-D.6; `migrations list` menunjukkan satu migration baru `(Pending)`; QBE Strict |
| Gerbang | Konfirmasi pemilik **sebelum** membuat migration dan **sebelum** menerapkannya |
| Risiko/pemilik | Migration modul lain dengan timestamp lebih awal (memori *pending migration tidak berurutan*). Pemilik: implementer + Sukma Giri Pratama |
| DoD | Kriteria 1–5 terbukti; penerapan ke DB hanya dengan izin |

### ✅ `BE-KSK-PO-002` — Kuota menghitung pendaftaran online dan dikunci per jadwal

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — AC 2 runtime terbukti lewat `BE-KSK-PO-005`: satu pendaftaran online sungguhan menaikkan `usedAppointment` 1→2 dan `usedTotal` 1→2, sisa kuota opsi publik 5→4, dan 34 kiriman yang ditolak tidak mengubah hitungan. Sebelumnya 🟡 2026-11-19 — build PASS, QBE Strict PASS; R1 regresi kuota lama vs baru identik (63 dan 65 jadwal, termasuk 6 slot terpakai); R4 5 permintaan bersamaan pada kuota 1: 1 berhasil, 4 ditolak (backend lama: 5/5 berhasil); harness in-memory 9/9 PASS — [laporan](../task/report/backend/BE-KSK-PO-002.md) |
| Outcome | `GetUsageAsync` menambah pendaftaran Menunggu Check-in ke `Appointment` dan `Total`; `AcquireQuotaLockAsync(scheduleId, date)`; `CreateEncounterCoreAsync` mengambil kunci kuota sesudah kunci pasien bila ada `DoctorScheduleId` |
| Requirement/decision | `FR-KSK-PO-002`; `INV-PO-02`; `KSK-DEC-030`; `KSK-DSN-PO-009`; `GATE-PO-17` |
| Kontrak | `02-backend-architecture.md` PO.4.3; `contracts/api-contract.md` *Patient Encounter (diperluas)* (`doctor-schedule-quotas` perilaku diperbarui) |
| Reuse | `DoctorScheduleQuotaService`; pola kunci `REG_ENCOUNTER_ACTIVE_*` |
| Cakupan | `Services/DoctorScheduleQuotaService.cs`, `Controllers/PatientEncounterController.cs` (bagian transaksi) |
| Dependency | `BE-KSK-PO-001` |
| Acceptance criteria | (1) Tanpa pendaftaran online, angka kuota sama persis dengan sebelum perubahan (regresi jalur petugas, kiosk, IGD). (2) Satu pendaftaran Menunggu Check-in mengurangi sisa appointment dan total sebesar 1; status lain tidak. (3) Urutan kunci pasien → kuota sama di semua jalur. (4) Dua permintaan kunjungan bersamaan pada slot terakhir: tepat satu berhasil |
| Verifikasi | Runtime HTTP: `doctor-schedule-quotas` sebelum/sesudah; dua `POST patient-encounters` paralel pada jadwal berkuota 1 |
| Risiko/pemilik | Jalur bersama RJ; regresi kuota. Catat titik sentuh ke blueprint `rawat-jalan` (`GATE-PO-06`). Pemilik: implementer + Sukma Giri Pratama |
| DoD | Kriteria 1–4 terbukti; build, QBE Strict PASS |

### ✅ `BE-KSK-PO-003` — Pemeriksa captcha dan pengirim email

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-19 — build PASS (0 error, 0 warning di berkas baru), QBE Strict PASS (5 berkas, 0 pelanggaran), harness 17/17 PASS (Turnstile kunci uji lolos/gagal, timeout 5,00 s, SMTP terkirim, SMTP diam 10,01 s, log bersih). Runtime aplikasi penuh `NOT RUN` karena belum ada endpoint pemakai — [laporan](../task/report/backend/BE-KSK-PO-003.md) |
| Outcome | `TurnstileVerificationService` (klien bernama `Turnstile`, 5 detik, tanpa retry, fail-closed, `Enabled = false` selalu lolos dengan peringatan saat start); `SmtpEmailSender` (`System.Net.Mail`, 10 detik, satu percobaan); binding opsi dan DI |
| Requirement/decision | `FR-KSK-PO-013`; `KSK-DEC-042`, `044`; `KSK-DSN-PO-006`, `007` |
| Kontrak | `contracts/integration-contract.md` `INT-PO-01`, `INT-PO-03` |
| Reuse | `IHttpClientFactory` (bawaan ASP.NET Core); tanpa paket NuGet baru |
| Cakupan | `Services/Security/TurnstileVerificationService.cs`, `Services/Notification/SmtpEmailSender.cs`, `Program.cs`, `appsettings.json` |
| Dependency | — |
| Acceptance criteria | (1) Kunci uji Cloudflare "selalu lolos" → lolos; "selalu gagal" → gagal. (2) Host Turnstile tidak terjangkau → `Unavailable` dalam ≤ 5 detik. (3) Email terkirim ke SMTP uji; SMTP mati → hasil gagal dalam ≤ 10 detik tanpa exception yang lolos. (4) Log tidak memuat secret, token, sandi, maupun alamat email |
| Verifikasi | Build; uji runtime lewat endpoint `BE-KSK-PO-004` setelah tersedia, atau harness sementara di scratchpad yang tidak di-commit |
| Risiko/pemilik | HTTP keluar pertama di backend; firewall server. Pemilik: implementer; akun nyata IT (`KSK-PO-OQ-015`, `016`) |
| DoD | Kriteria 1–4 terbukti |

### ✅ `BE-KSK-PO-004` — Opsi publik dan pencocokan pasien lama

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-19 — build PASS, QBE Strict PASS, verifier otorisasi PASS (0 endpoint telanjang baru, 24/24 identitas); runtime 26/26 PASS di 7185 (tanpa token, jadwal tersaring appointment + kiosk termasuk 3 jadwal uji, field minimum, 404 identik untuk 3 jenis salah, captcha kosong 400, Cloudflare tak terjangkau 503, 429 tepat di permintaan ke-21, no-store, kartu tersamar); penyamar nama identik dengan kode lama (14 contoh); log tanpa NIK/tanggal lahir — [laporan](../task/report/backend/BE-KSK-PO-004.md) |
| Outcome | `OnlineRegistrationPublicController` (`[AllowAnonymous]`, tanpa `[AccessController]`, `no-store`, path tersamar di log) dengan `GET options/clinics`, `options/doctor-schedules`, `options/insurance-providers`, `options/referral-institutions`, dan `POST patient-match`; `PatientNameMasker` di `Helpers/` dipakai juga oleh `QueueDisplayRuntimeController`; policy `OnlineRegistrationPublicOptions` dan `OnlineRegistrationPublicMatch` |
| Requirement/decision | `FR-KSK-PO-010`, `011`, `014`; `KSK-DEC-031`, `038`; `KSK-PO-FE-002`; `KSK-DSN-PO-005`, `013` |
| Kontrak | `api-contract.md` grup *Online Registration Public* (empat `GET` dan `patient-match`); `KSK-VAL-PO-001` s.d. `004`, `010` s.d. `012` |
| Reuse | `PriorityReservationArrivalController`, `KioskPatientLookupService` (normalisasi, pasien gabungan, non-aktif), `QueueDisplayRuntimeController#MaskPatientName` |
| Cakupan | Controller publik, `OnlineRegistrationPublicService` (bagian opsi dan pencocokan), `Helpers/PatientNameMasker.cs`, `QueueDisplayRuntimeController.cs`, `Program.cs`, DTO |
| Dependency | `BE-KSK-PO-002`, `BE-KSK-PO-003` |
| Acceptance criteria | (1) Endpoint dapat dipanggil tanpa token. (2) Jadwal hanya yang `IsAllowAppointment` dan `IsAllowKioskRegistration`, dengan sisa kuota dari `BE-KSK-PO-002`. (3) Respons hanya field minimum kontrak. (4) Pencocokan benar → nama dan nomor kartu tersamar; salah/non-aktif/gabung → `404` dengan isi identik. (5) Captcha gagal `400`; Cloudflare mati `503`. (6) Batas 20/jam → `429`. (7) Display antrean tetap menyamarkan nama persis seperti sebelumnya |
| Verifikasi | Runtime HTTP tanpa token; perbandingan byte respons `404`; uji `429`; verifier otorisasi (cara pendaftaran sama dengan `PriorityReservationArrivalController`) |
| Risiko/pemilik | Membuka master ke internet. Pemilik: implementer + Sukma Giri Pratama |
| DoD | Kriteria 1–7 terbukti; QBE Strict, verifier PASS |

### ✅ `BE-KSK-PO-005` — Kirim pendaftaran online

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — build PASS, QBE Strict PASS (37 berkas, 0 temuan), verifier otorisasi PASS (0 endpoint telanjang baru, 24/24 identitas); runtime di 7185: utama 50/50 (T4b dijalankan ulang sesudah perbaikan skrip uji), SMTP mati 2/2, captcha gagal + batas laju 3/3, Cloudflare mati 1/1, termasuk kirim ganda bersamaan `[200, 409]`, 34 validasi persis matriks, `429` tepat di kiriman ke-11, berkas 8 MB dan `.exe` ditolak; kueri baca-saja DB 11/11 PASS dengan izin pemilik (hash kode, `PatientId` kosong, consent, status email, riwayat, berkas); log 4 run tanpa data sensitif. Delta: nomor `PO-yyyyMMdd-nnnn` (format alokator), bukan `PO-yyMMdd-nnnn` — [laporan](../task/report/backend/BE-KSK-PO-005.md) |
| Outcome | `POST online-registrations/public` (`multipart/form-data`): validasi seluruh langkah, pencocokan ulang pasien lama, captcha, kunci pasien lalu kunci kuota, kode 8 karakter + hash, nomor `PO-yyMMdd-nnnn`, berkas rujukan ke `online-registration-referrals/{id}/` (kompensasi hapus bila transaksi gagal), consent pasien baru, riwayat `SUBMITTED`, email sesudah transaksi; policy `OnlineRegistrationPublicSubmit` |
| Requirement/decision | `FR-KSK-PO-012`; `KSK-DEC-030`, `032`, `034`, `035`, `038`, `042`, `043`, `047`; `INV-PO-01` s.d. `06`; `KSK-DSN-PO-002`, `004`, `011`, `012` |
| Kontrak | `api-contract.md` grup publik `POST`; `KSK-VAL-PO-005` s.d. `034`; PO-S.1 baris pertama |
| Reuse | `NumberSeriesAllocator`, helper hash SHA-256 RP, validasi isi berkas dan root privat `ReferralDocumentStorageService`, aturan No. HP kiosk (`KSK-DEC-022`) |
| Cakupan | `OnlineRegistrationPublicService`, controller publik, DTO, `Program.cs` |
| Dependency | `BE-KSK-PO-004` |
| Acceptance criteria | AC 3, 4, 5 (email), 6, 14, 15, 16 sesuai `testing/acceptance-test-matrix.md` PO-T.1. Tambahan: (1) kode hanya muncul di respons ini dan tidak tersimpan asli; (2) email tidak memuat NIK/No. RM/penjamin; (3) SMTP mati → `200` dengan `emailStatus = 2`; (4) kirim ganda bersamaan → satu `200`, satu `409` |
| Verifikasi | Runtime HTTP dengan kunci uji Turnstile; cek data lewat endpoint detail `BE-KSK-PO-009` bila sudah ada; bila belum, kueri baca-saja ke DB hanya dengan izin pemilik; berkas ada di root privat |
| Risiko/pemilik | Unggah berkas tanpa login. Pemilik: implementer + Sukma Giri Pratama |
| DoD | Seluruh kriteria terbukti; log tanpa data sensitif |

### ✅ `BE-KSK-PO-006` — Verifikasi check-in kiosk

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — build PASS, QBE Strict PASS, verifier otorisasi PASS (2026-11-20); runtime penolakan 17/17 PASS (2026-11-20); harness 26/26 PASS; runtime hari H 2026-11-25 6/6 PASS (data uji dipindah ke tanggal hari ini dengan izin pemilik (satu `UPDATE` 9 baris)): tanggal lahir salah `404` seragam, kode + tanggal lahir `200` token 43 karakter, paket pasien lama tanpa kode/NIK/kontak dengan nama tersamar, Lupa Kode NIK dan No. RM berformat `200`, paket pasien baru dengan `identityNumberForScan`, kontak, rujukan 2 berkas, consent. DoD dikecualikan: `dotnet test` (tanpa project test) — [laporan](../task/report/backend/BE-KSK-PO-006.md) |
| Outcome | `POST online-registrations/kiosk/check-in/verify` (`KioskRead`, tanpa `[AccessAction]`): mode kode atau identitas + tanggal lahir; status dan tanggal hari ini (WIB); token 256 bit, hash disimpan, berlaku `CheckInTokenMinutes`; paket check-in; policy `OnlineRegistrationCheckIn` per akun kiosk |
| Requirement/decision | `FR-KSK-PO-020`; `KSK-DEC-033`, `040`; `KSK-DSN-PO-008` |
| Kontrak | Grup *Online Registration Check-In*; `KSK-VAL-PO-040` s.d. `045` |
| Reuse | Pola `KioskPatientLookupController` (policy, rate limit per akun) |
| Cakupan | `OnlineRegistrationCheckInController`, `OnlineRegistrationCheckInService#VerifyAsync`, DTO, `Program.cs` |
| Dependency | `BE-KSK-PO-005` |
| Acceptance criteria | AC 8, 10. Tambahan: (1) kode yang sudah status akhir → `404` identik dengan kode salah; (2) belum hari H → `409` menyebut tanggal; (3) paket tidak memuat kode; `identityNumberForScan` hanya untuk pasien baru |
| Verifikasi | Runtime HTTP dengan akun kiosk; data dibuat lewat `BE-KSK-PO-005` |
| Risiko/pemilik | Tebak kode di kiosk; ditopang tanggal lahir, hari H, dan rate limit. Pemilik: implementer |
| DoD | Kriteria terbukti; verifier otorisasi PASS (aturan `KSK-DSN-006`) |

### ✅ `BE-KSK-PO-007` — Check-in menjadi kunjungan

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — build PASS, QBE Strict PASS (4 berkas, 0 temuan), verifier otorisasi PASS (2026-11-20); runtime penolakan dan regresi tiga kanal 14/14 PASS (2026-11-20); harness 44/44 PASS; runtime hari H 2026-11-25 16/16 PASS (data uji dipindah ke tanggal hari ini dengan izin pemilik (satu `UPDATE` 9 baris)): pasien lama `200` sumber 7 appointment kiosk, status 2 + riwayat `CHECKED_IN`, token dipakai ulang `409` `055`, kode sekali pakai `404`, token lama `046`, poli diganti `048`; pasien baru dengan master dari `POST patients/kiosk` `200`, rujukan 2 dokumen `CaptureSource` 3 dapat dibuka, consent `SignedAt` asli; NIK master berbeda `052`; jadwal dinonaktifkan `409` `053`; kuota penuh `409` `054`; kuota tepat penuh oleh pendaftaran sendiri `200` tanpa kuota ganda. Delta: `POST ''` alias kiosk, penolakan `047` di `/admin`. Keputusan pemilik terbuka (bukan syarat task): faskes perujuk non-mitra di halaman publik. DoD dikecualikan: `dotnet test` (tanpa project test) — [laporan](../task/report/backend/BE-KSK-PO-007.md) |
| Outcome | `PatientEncounterCreateRequest` + `OnlineRegistrationId`, `OnlineRegistrationCheckInToken`; tolak dari kanal petugas; `PrepareCheckInAsync` sebelum transaksi (token, status, hari H, poli/jadwal/dokter, `IsAppointment`, pasien lama `PatientId`, pasien baru NIK master, pembayaran, kartu, faskes); di dalam transaksi `CompleteCheckInInTransactionAsync` (status Menjadi Kunjungan sebelum validasi kuota kunjungan, `EncounterId`, `RegPatientGeneralConsent` dari consent tampungan dengan `SignedAt` asli, `RegEncounterReferralDocument` menunjuk `StoragePath` yang sama, hapus hash token, riwayat `CHECKED_IN`); `RegistrationSource = 7`, `CaptureSource = 3` |
| Requirement/decision | `FR-KSK-PO-021`; `KSK-DEC-034`, `036`, `039`, `046`, `048`; `INV-PO-02`, `06`; `KSK-DSN-PO-008`, `010`, `014` |
| Kontrak | `api-contract.md` *Patient Encounter (diperluas)*; `KSK-VAL-PO-046` s.d. `055`; PO-S.1 baris ketiga |
| Reuse | Hook RP `PrepareCompletionAsync`/`CompleteInTransactionAsync` di `PatientEncounterController.cs:966-990`, `:1181-1200`; validasi jadwal/kuota jalur kunjungan |
| Cakupan | `PatientEncounterController`, `PatientEncounterDtos.cs`, `OnlineRegistrationCheckInService` |
| Dependency | `BE-KSK-PO-006` |
| Acceptance criteria | AC 7, 9, 13, 14, 17 (sisi backend) dan baris PO-T.2 terkait. Tambahan: (1) check-in saat kuota appointment tepat penuh oleh pendaftaran itu sendiri berhasil; (2) kegagalan apa pun di transaksi membatalkan kunjungan dan status tetap 1; (3) kunjungan petugas/kiosk biasa tanpa field baru berperilaku sama seperti sebelumnya |
| Verifikasi | Runtime HTTP: pasien lama, pasien baru (master baru dan NIK lama), rujukan dengan berkas, consent; uji negatif per `KSK-VAL-PO-046` s.d. `055` |
| Risiko/pemilik | Jalur bersama kunjungan. Pemilik: implementer + Sukma Giri Pratama |
| DoD | Kriteria terbukti; regresi kunjungan RJ/kiosk/IGD dibuktikan dengan satu kunjungan per kanal |

### ✅ `BE-KSK-PO-008` — Penutupan Jalur Lain dan Tidak Datang

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-23 — build PASS, QBE Strict PASS (10 berkas, 0 temuan), verifier otorisasi PASS (fallback 68 tetap); runtime di 7185 8/8 PASS untuk Jalur Lain (pasien lama di jadwal berkuota 1 yang penuh oleh pendaftarannya sendiri tetap diterima dan pendaftaran status 3 + nomor kunjungan; kuota tidak ganda; kode ditolak kiosk `404`; pasien lain ditolak kuota penuh; pasien baru lewat NIK master status 3; pendaftaran lain tidak tersentuh); harness jam palsu 11/11 PASS untuk Tidak Datang (2 hari tertinggal, idempoten, kuota kembali, berjalan saat start) dan aturan pencocokan; layanan harian tercatat berjalan saat start di runtime. 2026-11-23: runtime Tidak Datang 5/5 PASS dengan data nyata (3 pendaftaran uji 21 Nov ditutup saat backend menyala untuk hari 22 Nov yang terlewat: status 4, tepat satu riwayat `NO_SHOW` sumber Sistem 1→4, `closedAt` terisi, aksi petugas tertutup; 0 Menunggu Check-in bertanggal lewat; 6 kode 21 Nov ditolak kiosk `404` seragam; kuota jadwal 21 Nov 0; backend uji kedua menyala → 0 ditutup, riwayat tetap satu). Tanpa perubahan source — [laporan](../task/report/backend/BE-KSK-PO-008.md) |
| Outcome | `OnlineRegistrationService.CloseForEncounterAsync` dipanggil di samping penutupan Reservasi Prioritas untuk kunjungan RJ berpoli tanpa `OnlineRegistrationId` (pasien lama lewat `PatientId`, pasien baru lewat NIK master); `CloseExpiredAsync` + `OnlineRegistrationClosureHostedService` (00:05 WIB, mengejar hari tertinggal, idempoten) |
| Requirement/decision | `FR-KSK-PO-003`, `004`; `KSK-DEC-037`, `045`; `GATE-PO-04`; `KSK-DSN-PO-015` |
| Kontrak | PO-S.1 baris keempat dan kelima; PO-I.3 |
| Reuse | `OutpatientPriorityReservationService#CloseForEncounterAsync`, `#CloseExpiredAsync`, `OutpatientPriorityReservationClosureHostedService` |
| Cakupan | `OnlineRegistrationService`, `OnlineRegistrationClosureHostedService`, `PatientEncounterController`, `Program.cs` |
| Dependency | `BE-KSK-PO-005` |
| Acceptance criteria | AC 11, 12 dan baris PO-T.3 terkait |
| Verifikasi | Runtime HTTP: pendaftaran lewat API, lalu kunjungan lewat kiosk/petugas; penutupan harian dipicu dengan jam lokal uji atau opsi `RunAtLocalTime` dev |
| Risiko/pemilik | Menutup pendaftaran pasien lain bila pencocokan NIK salah. Pemilik: implementer |
| DoD | Kriteria terbukti; menjalankan ulang tidak menambah riwayat |

### ✅ `BE-KSK-PO-009` — Daftar, ringkasan, detail, dan berkas untuk petugas

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — build PASS, QBE Strict PASS (44 berkas, 0 temuan), verifier otorisasi PASS (fallback 68 tetap, 0 endpoint telanjang baru); runtime di 7185 32/32 PASS dengan akun fixture API ber-hak dan tanpa hak (`200`/`403`/`401`; pencarian kode lewat hash 6/6, nomor/nama/No. RM/NIK sebagian; `400` `KSK-VAL-PO-068`/`069`; paginasi; ringkasan; detail + riwayat; berkas PDF utuh, lintas pendaftaran `404`); kode tidak pernah keluar. Aksi `Read`, `Update`, `Cancel` di Akses Role terbukti bersama endpoint `BE-KSK-PO-010` (A1). Sebelumnya 🟡 — [laporan](../task/report/backend/BE-KSK-PO-009.md) |
| Outcome | `OnlineRegistrationController` (`[AccessController(... "Online Registration", ControllerName = "OnlineRegistration")]`) dengan `GET`, `GET summary`, `GET {id}`, `GET {id}/referral-documents/{documentId}`; pencarian kode lewat hash persis, lainnya sebagian; detail beserta riwayat |
| Requirement/decision | `FR-KSK-PO-030`, `032` (Resource); `KSK-DEC-050`; `KSK-PO-AS-002`; `KSK-DSN-PO-017` |
| Kontrak | Grup *Online Registration* `GET`; `KSK-VAL-PO-068`, `069` |
| Reuse | `OutpatientPriorityReservationService#GetPagedAsync`, ringkasan, pengisian nama audit; `ReferralDocumentStorageService#OpenAsync` (pembukaan berkas) |
| Cakupan | Controller petugas, `OnlineRegistrationService` (kueri), DTO |
| Dependency | `BE-KSK-PO-005` |
| Acceptance criteria | AC 18. Tambahan: (1) Resource `OnlineRegistration` dengan aksi `Read`, `Update`, `Cancel` muncul di layar Akses Role setelah seeder berjalan; (2) kode tidak pernah dikembalikan; (3) tanpa hak `403` |
| Verifikasi | Runtime HTTP dengan akun ber-hak dan tanpa hak; verifier otorisasi |
| Risiko/pemilik | Data pribadi lengkap di layar petugas. Pemilik: implementer |
| DoD | Kriteria terbukti; QBE Strict, verifier PASS |

### ✅ `BE-KSK-PO-010` — Ubah jadwal, ubah kontak, batal

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — build PASS, QBE Strict PASS (6 berkas diff, 0 temuan), verifier otorisasi PASS (fallback 68 tetap, 0 endpoint telanjang baru); runtime di 7185 33/33 PASS: pindah tanggal + dokter lain di poli sama (kuota lama 5→4, baru 0→1, riwayat lama/baru, kode lama di kiosk menyebut 23 Nov), jadwal baru penuh `409` tanpa perubahan, poli lain `400`, bentrok tanggal `409`, versi lama `409`; ubah kontak dengan riwayat dan tanpa kirim ulang; batal: status 5, kuota kembali, kode ditolak kiosk `404`, batal ulang `409`, alasan pendek `400`; akun hanya `Read` `403`; log tanpa kontak — [laporan](../task/report/backend/BE-KSK-PO-010.md) |
| Outcome | `PUT {id}/schedule` (poli sama, H+1 s.d. H+30, kuota baru dengan kunci kuota jadwal baru, aturan satu per tanggal, kode tetap), `PUT {id}/contact`, `POST {id}/cancel` (alasan wajib, status Dibatalkan); semuanya dengan `expectedRowVersion` dan riwayat nilai lama/baru |
| Requirement/decision | `FR-KSK-PO-031`, `032`; `KSK-DEC-051`, `052`; `KSK-PO-AS-003`; `KSK-DSN-PO-016`; `GATE-PO-16` |
| Kontrak | Grup *Online Registration* `PUT`/`POST`; `KSK-VAL-PO-060` s.d. `072`; PO-S.1 baris enam s.d. delapan, PO-S.2 |
| Reuse | `OutpatientPriorityReservationService#CancelAsync`, pola `RowVersion` |
| Cakupan | Controller petugas, `OnlineRegistrationService` (perintah), DTO |
| Dependency | `BE-KSK-PO-009` |
| Acceptance criteria | AC 19, 20 dan baris PO-T.4 terkait |
| Verifikasi | Runtime HTTP; kuota sebelum/sesudah lewat `doctor-schedule-quotas`; kode lama dicoba di verifikasi kiosk sesudah pindah tanggal |
| Risiko/pemilik | Pertukaran kuota. Pemilik: implementer |
| DoD | Kriteria terbukti; log tanpa kontak |

### ✅ `BE-KSK-PO-011` — Faskes perujuk mitra di halaman publik

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — build PASS (0 error, 0 warning di berkas yang diubah), QBE Strict PASS (3 berkas, 0 temuan), verifier otorisasi PASS (fallback 68 tetap, 0 endpoint telanjang baru); runtime di 7185 10/10 PASS: tanpa/di luar H+1..H+30 `visitDate` → `400` `KSK-VAL-PO-074`, mitra berlaku muncul, mitra uji s.d. 5 Des muncul pada 5 Des dan tidak pada 6 Des, mitra kedaluwarsa/nonaktif tidak muncul, kirim dengan perjanjian berakhir sebelum tanggal kunjungan `400` `KSK-VAL-PO-073` tanpa tersimpan, mitra berlaku `200`. Dikecualikan: faskes `IsPartner = false` (API master selalu menyimpan mitra; tidak ada di DB dev), `dotnet test` (tanpa project test). Tanpa migration — [laporan](../task/report/backend/BE-KSK-PO-011.md) |
| Outcome | `GET /public/options/referral-institutions` mewajibkan `visitDate` (H+1 s.d. H+30, `400` `KSK-VAL-PO-074` bila tidak sah) dan hanya mengembalikan faskes aktif, mitra, dengan perjanjian yang berlaku pada tanggal itu (`ReferralPartnerEligibility.IsEligibleOn`). `POST /public` menolak faskes yang bukan mitra pada tanggal kunjungan pendaftaran (`400` `KSK-VAL-PO-073`). Check-in tidak berubah (`RJ-VAL-PM-17` tetap) |
| Requirement/decision | `FR-KSK-PO-015`; `KSK-DEC-057`, `058`; `KSK-DSN-PO-018`; `DEC-FRJ-001` |
| Kontrak | `KSK-CONTRACT-v2.1` — *Perubahan v2.1* pada `api-contract.md`; `KSK-VAL-PO-073`, `074` |
| Reuse | `ReferralPartnerEligibility` (`Areas/HealthServices/MasterData/Services`), pola saringan `partner-options` RJ |
| Cakupan | `OnlineRegistrationPublicService` (opsi faskes, validasi kirim), `OnlineRegistrationPublicController` (query `visitDate`) |
| Dependency | `BE-KSK-PO-005` |
| Acceptance criteria | Tiga baris uji `KSK-DEC-057` di PO-T.1: (1) mitra muncul hanya pada tanggal perjanjian berlaku, non-mitra tidak pernah muncul, tanpa `visitDate` → `400` `074`; (2) kirim dengan faskes non-mitra → `400` `073`, tidak ada pendaftaran tersimpan. Tambahan: pendaftaran dengan faskes mitra tetap `200`; batas laju dan validasi `search` tidak berubah |
| Verifikasi | Runtime HTTP ke backend uji; data uji faskes mitra dan non-mitra dibaca dari master yang ada (tanpa eksekusi DB langsung); build, QBE Strict, verifier otorisasi |
| Risiko/pemilik | Query wajib baru memutus konsumen lama; konsumen tunggal halaman publik dirilis bersama `FE-KSK-PO-009`. Pemilik: implementer |
| DoD | Kriteria terbukti; tanpa migration; log tanpa data pasien |

## Tindak lanjut di luar task

| Tindak lanjut | Pemilik | Kapan |
| --- | --- | --- |
| Catat titik sentuh kuota dan penutupan di decision log `rawat-jalan` (`GATE-PO-06`) | Sukma Giri Pratama | Sebelum `BE-KSK-PO-002` ✅ dirilis. Termasuk temuan: pindah jadwal kunjungan (`OutpatientEncounterScheduleService`) belum memakai kunci kuota |
| Centang `OnlineRegistration : Read/Update/Cancel` di Akses Role | Admin | Go-live `MVP-3` |
| `KSK-PO-OQ-010`, `015`, `016` | IT/infrastruktur | Go-live `MVP-1` |

## Coverage gap

Tidak ada acceptance criteria Amendment PO yang tanpa task backend. AC 1 (urutan langkah halaman) dan bagian tampilan AC 2, 5, 17, 18 dibuktikan task frontend (`online-registration-frontend-roadmap.md`).
