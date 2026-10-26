# Backend Roadmap — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Roadmap ID | `RJ-PRI-RM-BE-001` |
| Revision | `2` — menambah `BE-RJP-013` (`MVP-3`, ganti tanggal). Revisi `1` berisi `BE-RJP-001` s.d. `012` |
| Status | r1 `approved` — Sukma Giri, 2026-10-16 (`RJ-PRI-DEC-031`). r2 `approved` — Sukma Giri, 2026-10-21 (`RJ-PRI-DEC-039`: roadmap `MVP-3` disetujui karena tidak memuat keputusan bisnis baru). Wewenang implementasi `GRANTED` untuk seluruh task |
| Blueprint | `rawat-jalan` revisi `33`, `approved` (Sukma Giri, 2026-10-16, `RJ-PRI-DEC-030`), dan revisi `34`, `approved` (Sukma Giri, 2026-10-21, `RJ-PRI-DEC-039`), bentuk `SINGLE` |
| Cakupan | `MVP-0` s.d. `MVP-3` (`EPIC RP-01` s.d. `RP-09`, sisi backend). `POST-MVP` tidak direncanakan |
| SHA baseline | r1: BE `551e1cbb` (`sukmagp`), FE `1fd858322` (`sukmagpV2`). r2: BE `b0285ff1`, FE `037c975b7` |
| Kontrak masukan | `RJ-PRI-RESV-001@1.0.0` `approved`. Hash per berkas di `blueprint-manifest.md` Revisi 33 (`api-contract.md` `7932aaf7dc6c`, `state-transition-matrix.md` `76ce19d73d35`, `validation-matrix.md` `a52d2b2352ee`, `integration-contract.md` `91432bc56a4a`, `permission-audit-matrix.md` `26e9d859e114`). Untuk `BE-RJP-013`: `RJ-PRI-RESV-001@1.2.0` `approved`, hash di manifest Revisi 34 (`api-contract.md` `dc01eae85f3c`, `state-transition-matrix.md` `7422b45bbb5c`, `validation-matrix.md` `a1fd93c59867`, `integration-contract.md` `daeea91d43ca`, `permission-audit-matrix.md` `395c2e87b09b`) |
| Masukan desain | `02-backend-architecture.md` RP.1–RP.14 (`bd5586a3e478`), `data/data-dictionary.md` RP (`0e7737a60d3b`), `04-prd-to-mvp.md` RP-10, RP-20 (`a7962b2158b1`), `testing/acceptance-test-matrix.md` *Amendment RP* (`ce8039c0fb65`). Untuk `BE-RJP-013`: `02` GT.1–GT.14 (`e1df7b3c92b8`), data RP-GT (`4a2fe12c8fad`), `04` RP-GT (`bcb35d6aee59`), tes *Amendment RP-GT* (`e8c1e00d8f79`) |
| Owner | Sukma Giri |
| Nama berkas | `priority-reservation-backend-roadmap.md`, karena `roadmap/backend-roadmap.md` sudah dimiliki scope `RJ-BIL`. Pola yang sama dipakai `e2e-backend-roadmap.md` |

> **Batas dokumen.**
> 1. Roadmap ini **bukan** izin menulis kode. Setiap task baru boleh dikerjakan lewat `build-module-backend` dengan approval task yang eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/`, bukan dari roadmap ini.
> 3. Migration dibuat di `BE-RJP-005` dan `BE-RJP-013`. **Penerapannya** ke `QuilvianNewDevSukma` butuh izin terpisah per sesi. Untuk `BE-RJP-013`, izin itu sudah diberikan `RJ-PRI-DEC-039`.
> 4. Verifikasi memakai pola Bank Darah: build, `has-pending-model-changes`, QBE Strict, runtime HTTP. Tanpa folder `Tests/`. `dotnet test` ditulis `NOT RUN — tidak ada project test`.
> 5. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda

| Tanda | Arti |
|:---:|---|
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-RJP-001 ✅

BE-RJP-003 ✅

BE-RJP-004 ✅

BE-CRM-010 ✅ [CRM] ─> BE-RJP-002 ✅ ────────────────────┐
                                                         │
BE-RJP-005 ✅ ─┬─> BE-RJP-006 ✅ ─┬─> BE-RJP-007 ✅ ─┬───┴─> BE-RJP-008 ✅
               │                  │                  │
               │                  │                  ├─> BE-RJP-010 ✅ ─┬───────────────────┐
               │                  │                  │                  │                   │
               │                  │                  ├──────────────────┴─> BE-RJP-012 ✅   │
               │                  │                  │                                      │
               │                  │                  └──────────────────────────────────────┴─> BE-RJP-013 ✅
               │                  │
               │                  └─> BE-RJP-009 ✅
               │
               └─> BE-RJP-011 ✅
```

`[CRM]` = task backend pada `crm-membership/roadmap/backend-roadmap.md`, cermin baca-saja (resolver member aktif).

Pasangan prasyarat → task: 12 (`CRM-010→002`, `005→006`, `005→011`, `006→007`, `006→009`, `007→008`, `002→008`,
`007→010`, `007→012`, `010→012`, `007→013`, `010→013`), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — (`BE-RJP-002` menunggu `BE-CRM-010` ✅ yang sudah selesai) | `BE-RJP-001`, `BE-RJP-002`, `BE-RJP-003`, `BE-RJP-004`, `BE-RJP-005` — boleh paralel |
| 2 | `BE-RJP-005` | `BE-RJP-006`, `BE-RJP-011` |
| 3 | `BE-RJP-006` | `BE-RJP-007`, `BE-RJP-009` |
| 4 | `BE-RJP-007` (+ `BE-RJP-002` untuk `008`) | `BE-RJP-008`, `BE-RJP-010` |
| 5 | `BE-RJP-007`, `BE-RJP-010` | `BE-RJP-012`, `BE-RJP-013` |

**Gelombang rilis produk** (`04-prd-to-mvp.md` RP-20) berbeda dari gelombang teknis di atas:

| Rilis | Task | Catatan |
|---|---|---|
| `MVP-0` | `BE-RJP-001`, `002`, `003`, `004` | `BE-RJP-004` dirilis bersama `FE-RJP-001` |
| `MVP-1` | `BE-RJP-005` s.d. `011` | Secara teknis `BE-RJP-005` tidak menunggu `MVP-0`, sehingga boleh dikerjakan paralel bila owner mengizinkan. Rilisnya tetap sesudah `MVP-0` |
| `MVP-2` | `BE-RJP-012` | Development boleh jalan. Go-live menunggu `RJ-PRI-OQ-003` (domain publik dan IP asli) |
| `MVP-3` | `BE-RJP-013` | Ganti tanggal (`04` RP-GT-7). Aditif dan kompatibel mundur, sehingga boleh dirilis sebelum atau bersama `FE-RJP-009`. Migration diterapkan sebelum kode berjalan |

## Ringkasan Task

| Task ID | Outcome | Epic / FR | Dependency | Rilis | Status |
|---|---|---|---|---|---|
| `BE-RJP-001` | Nomor prioritas kelipatan N | `RP-01` / `FR-RP-001`, `002` | — | `MVP-0` | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-01/02 6/6 + UAT-RP-02 PASS — [laporan](../task/report/backend/BE-RJP-001.md) |
| `BE-RJP-002` | Klasifikasi antrean memakai resolver dan hari pendaftaran (memenuhi `BE-CRM-022`) | `RP-03` / `FR-RP-020` s.d. `022` | `BE-CRM-010` ✅ [CRM] | `MVP-0` | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 8/8 PASS — [laporan](../task/report/backend/BE-RJP-002.md) |
| `BE-RJP-003` | Display API tanpa antrean prioritas | `RP-02` / `FR-RP-010` | — | `MVP-0` | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-03 3/3 PASS — [laporan](../task/report/backend/BE-RJP-003.md) |
| `BE-RJP-004` | Group realtime display dengan payload ringkas | `RP-02` / `FR-RP-011`, `012` | — | `MVP-0` (rilis bersama `FE-RJP-001`) | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-04/05 5/5 PASS; rilis bersama FE-RJP-001 — [laporan](../task/report/backend/BE-RJP-004.md) |
| `BE-RJP-005` | Skema reservasi dan migration | `RP-04` / `FR-RP-030` (data) | — | `MVP-1` | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS (10 berkas), skema = kamus data; migration diterapkan ke QuilvianNewDevSukma atas izin. Rework 2026-10-21 (`PublicArrivalBaseUrl` bawaan server dev, lokal lewat override): JSON valid, nilai efektif terbukti, build PASS (0 error); staging/production disetel lewat environment variable server — [laporan](../task/report/backend/BE-RJP-005.md) |
| `BE-RJP-006` | Buat reservasi dan kueri | `RP-04` / `FR-RP-030` s.d. `032`, `FR-RP-062` | `BE-RJP-005` | `MVP-1` | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 26/26 PASS (R-RP-08/09 paralel/10/23 kolom, UAT-RP-07..09); 403 tidak diuji (hanya akun SuperAdmin). Rework 2026-10-19 (RJ-PRI-DEC-032, kontrak 1.1.0): build PASS (0 error), EF bersih, QBE Strict PASS, runtime 11/11 PASS (kolom audit daftar, ringkasan rentang, kompatibel mundur) — [laporan](../task/report/backend/BE-RJP-006.md) |
| `BE-RJP-007` | Perintah petugas: tiba, kembalikan, skrining, kirim ulang, batal | `RP-04`, `RP-06` / `FR-RP-033` s.d. `035`, `037`, `FR-RP-052` | `BE-RJP-006` | `MVP-1` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 18/18 PASS (R-RP-15/19/23 kirim ulang/24/26); link publik menyusul BE-RJP-012, lengkapi 403 menyusul BE-RJP-008 — [laporan](../task/report/backend/BE-RJP-007.md) |
| `BE-RJP-008` | Pelengkapan lewat pendaftaran kunjungan dan jalur lain | `RP-05` / `FR-RP-040` s.d. `043` | `BE-RJP-007`, `BE-RJP-002` | `MVP-1` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 16/16 + 2/2 PASS (R-RP-16/17/19 lengkapi/20/21 penutupan/22/27/30, UAT-RP-11..13), tanpa regresi — [laporan](../task/report/backend/BE-RJP-008.md) |
| `BE-RJP-009` | Penutupan Tidak Datang akhir hari | `RP-04` / `FR-RP-036` | `BE-RJP-006` | `MVP-1` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 5/5 dua kali (R-RP-18 idempoten, UAT-RP-10) + peran Web tanpa proses PASS — [laporan](../task/report/backend/BE-RJP-009.md) |
| `BE-RJP-010` | Realtime daftar reservasi untuk admisi dan skrining | `RP-04` / `FR-RP-038` | `BE-RJP-007` | `MVP-1` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime SignalR 9/9 PASS (R-RP-14 sisi backend, payload tanpa nama) + regresi display — [laporan](../task/report/backend/BE-RJP-010.md) |
| `BE-RJP-011` | Nama ruang tunggu prioritas di respons antrean | `RP-07` / `FR-RP-060`, `061` (data) | `BE-RJP-005` | `MVP-1` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 3/3 dua kali (R-RP-29 sisi API, nama dari konfigurasi; Reguler null) — [laporan](../task/report/backend/BE-RJP-011.md) |
| `BE-RJP-012` | Endpoint publik link konfirmasi | `RP-06` / `FR-RP-050`, `051`, `053` | `BE-RJP-007`, `BE-RJP-010` | `MVP-2` | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 12/12 PASS (R-RP-11/12/13/25, UAT-RP-14/15, kode tidak masuk log); go-live menunggu RJ-PRI-OQ-003 dan tinjauan keamanan — [laporan](../task/report/backend/BE-RJP-012.md) |
| `BE-RJP-013` | Ganti tanggal reservasi sebelum hari kunjungan | `RP-09` / `FR-RP-090` s.d. `096` | `BE-RJP-007`, `BE-RJP-010` | `MVP-3` | ✅ 2026-10-21 — build PASS (0 error), EF bersih, QBE Strict PASS (8 berkas), migration diterapkan ke QuilvianNewDevSukma, runtime 23/23 PASS (R-RP-31..42 sisi API/SignalR, 403 dengan akun non-SuperAdmin); sisi layar R-RP-41 di FE-RJP-009 — [laporan](../task/report/backend/BE-RJP-013.md) |

## Rincian Task

### `BE-RJP-001` ✅ — Nomor prioritas kelipatan N

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-01/02 6/6 + UAT-RP-02 PASS — [laporan](../task/report/backend/BE-RJP-001.md) |
| Outcome | Prioritas mendapat kelipatan N terkecil yang belum terpakai, dan Reguler mendapat nomor terkecil yang bukan kelipatan N. `ReservedPriorityNumbers` diganti `PriorityNumberMultiple` (bawaan 5, minimal 2, divalidasi saat start) |
| Requirement/decision | `FR-RP-001`, `FR-RP-002`; `RJ-PRI-DEC-004` (menggantikan `RJ-DOC-DEC-056`); `INV-RP-09` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0` — `validation-matrix.md` `VAL-RP-21`; `02` RP.4.1 |
| Reuse | `OutpatientQueueNumberAllocator` (kunci, cakupan, larangan pakai ulang tetap) |
| Cakupan | `Services/OutpatientQueueNumberAllocator.cs`, `appsettings.json` (`HealthServices:Registration:QueueNumber`), validasi opsi di `Program.cs` |
| Dependency | — |
| Acceptance criteria | `R-RP-01`, `R-RP-02`, `UAT-RP-01`, `UAT-RP-02` |
| Verifikasi | Build, QBE Strict, runtime HTTP pendaftaran petugas dan kiosk, kueri `RegQueue` |
| Risiko/pemilik | Lingkungan lain yang mengisi `ReservedPriorityNumbers` perlu diberi tahu. Rilis di luar jam layanan (`GATE-RP-02`). Pemilik: Sukma Giri |
| DoD | Keempat kriteria terbukti dan laporan task tertulis |

### `BE-RJP-002` ✅ — Klasifikasi antrean dengan resolver (memenuhi `BE-CRM-022`)

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 8/8 PASS — [laporan](../task/report/backend/BE-RJP-002.md) |
| Outcome | `OutpatientQueueClassificationService` memakai `PatientMembershipStatusResolver`. Tanggal penilaian = hari pendaftaran (WIB). `FromReservationSnapshot` tersedia untuk `BE-RJP-008`. `PatientEncounterController` mengambil service dari DI |
| Requirement/decision | `FR-RP-020` s.d. `022`; `RJ-PRI-DEC-006`, `007`; `CRM-DEC-032`; `crm-membership` `FR-CRM-042`, `AT-36` |
| Kontrak | `integration-contract.md` `INT-RP-01`; `02` RP.4.2 |
| Reuse | Resolver (`BE-CRM-010` ✅), kebijakan tingkat yang sudah dibaca |
| Cakupan | `Services/OutpatientQueueClassificationService.cs`, `Controllers/PatientEncounterController.cs` (konstruktor dan pemanggilan `ClassifyAsync`), `Program.cs` |
| Dependency | `BE-CRM-010` ✅ [CRM] |
| Acceptance criteria | `R-RP-06`, `R-RP-07`, `UAT-RP-05`, `UAT-RP-06`. Pasien yang memenuhi aturan CRM mendapat klasifikasi identik dengan sebelumnya |
| Verifikasi | Runtime HTTP sebelum/sesudah, kueri snapshot `RegQueue` |
| Risiko/pemilik | Booking mingguan yang sudah berjalan berubah hasil klasifikasinya (`CAP-RP-05`). Petugas diberi tahu sebelum rilis. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti, laporan task tertulis, dan baris `BE-CRM-022` di roadmap `crm-membership` ditandai dipenuhi |

### `BE-RJP-003` ✅ — Display API tanpa antrean prioritas

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-03 3/3 PASS — [laporan](../task/report/backend/BE-RJP-003.md) |
| Outcome | `BuildDisplayQueueQuery` menyaring `IsPriorityQueue = false`, sehingga `summary`, `items`, `called`, dan suara tidak memuat antrean prioritas |
| Requirement/decision | `FR-RP-010`; `RJ-PRI-DEC-005`; `INV-RP-08` |
| Kontrak | `api-contract.md` Queue Display Runtime (diperbarui) |
| Reuse | Penyaring `QueueAudienceMode` yang sudah ada |
| Cakupan | `Controllers/QueueDisplayRuntimeController.cs` |
| Dependency | — |
| Acceptance criteria | `R-RP-03`, `UAT-RP-03` (sisi API) |
| Verifikasi | Runtime HTTP dengan akun display |
| Risiko/pemilik | Display `MemberOnly` menjadi kosong untuk tingkat prioritas (`GATE-RP-01`). Admin perangkat diberi tahu. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `BE-RJP-004` ✅ — Group realtime display

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS, EF bersih, QBE Strict PASS, runtime R-RP-04/05 5/5 PASS; rilis bersama FE-RJP-001 — [laporan](../task/report/backend/BE-RJP-004.md) |
| Outcome | `QueueHub.JoinQueueDisplayCluster` untuk akun display. Akun display ditolak di group perawat/dokter. `QueueRealtimeService` mengirim `QueueDisplayRealtimeEventResponse` (tanpa `PatientId`/`EncounterId`) ke group display, hanya untuk antrean non-prioritas |
| Requirement/decision | `FR-RP-011`, `FR-RP-012`; `RJ-PRI-DEC-025`; `RJ-DOC-DEC-057`; `CONF-RP-01` |
| Kontrak | `api-contract.md` hub `/hubs/queues`; `integration-contract.md` `INT-RP-06` |
| Reuse | `QueueHub`, klaim cluster perangkat display, `QueueRealtimeService` |
| Cakupan | `Hubs/QueueHub.cs`, `Services/QueueRealtimeService.cs`, `DTOS/QueueRealtimeDtos.cs` |
| Dependency | — |
| Acceptance criteria | `R-RP-04`, `R-RP-05`, `UAT-RP-04` |
| Verifikasi | Klien SignalR uji (skrip di scratchpad) dengan akun display dan akun perawat |
| Risiko/pemilik | **Rilis wajib bersama `FE-RJP-001`.** Bila backend lebih dulu, display berhenti menerima realtime sampai frontend menyusul (tetap memuat ulang berkala). Pemilik: Sukma Giri |
| DoD | Kriteria terbukti; rilis bersama `FE-RJP-001` tercatat |

### `BE-RJP-005` ✅ — Skema reservasi dan migration

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS (10 berkas), skema = kamus data; migration diterapkan ke QuilvianNewDevSukma atas izin. Rework 2026-10-21 (`PublicArrivalBaseUrl` bawaan server dev, lokal lewat override): JSON valid, nilai efektif terbukti, build PASS (0 error); staging/production disetel lewat environment variable server — [laporan](../task/report/backend/BE-RJP-005.md) |
| Outcome | `RegPriorityReservation`, `RegPriorityReservationHistory`, tiga enum baru, `EncounterRegistrationSource.PriorityReservation = 6`, dua configuration, dua `DbSet`, `PriorityReservationOptions` beserta kunci konfigurasi, dan migration `AddOutpatientPriorityReservation` |
| Requirement/decision | `FR-RP-030` (data); `RJ-PRI-DEC-009`, `012`, `019`, `021`, `024`; `INV-RP-01`, `04`, `05` |
| Kontrak | `data/data-dictionary.md` RP-2, RP-3, RP-5 |
| Reuse | `IdentityModel`, pola `RowVersion` dari `crm-membership` |
| Cakupan | `Models/`, `Enums/`, `Repositories/Configurations/HealthServices/`, `ApplicationDbContext`, `Services/PriorityReservationOptions.cs`, `appsettings.json`, satu migration |
| Dependency | — |
| Acceptance criteria | (1) Kolom, tipe, bawaan, index (enam pada reservasi), dan FK `Restrict` persis kamus data. (2) `Down` menghapus kedua tabel. (3) `has-pending-model-changes` bersih sesudah migration. (4) Build lulus |
| Verifikasi | `dotnet ef migrations script` untuk migration ini dibandingkan dengan DDL kamus data. `migrations list` menunjukkan satu migration baru `(Pending)` |
| Risiko/pemilik | Migration modul lain masuk dengan timestamp lebih awal (lihat memori pending migration). Pemilik: implementer + Sukma Giri |
| DoD | Kriteria 1–4 terbukti. Penerapan ke DB hanya dengan izin |

### `BE-RJP-006` ✅ — Buat reservasi dan kueri

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 26/26 PASS (R-RP-08/09 paralel/10/23 kolom, UAT-RP-07..09); 403 tidak diuji (hanya akun SuperAdmin). Rework 2026-10-19 (RJ-PRI-DEC-032, kontrak 1.1.0): build PASS (0 error), EF bersih, QBE Strict PASS, runtime 11/11 PASS (kolom audit daftar, ringkasan rentang, kompatibel mundur) — [laporan](../task/report/backend/BE-RJP-006.md) |
| Outcome | `OutpatientPriorityReservationController` dengan `GET /`, `/summary`, `/{id}`, `/eligibility`, dan `POST /`. Service membuat nomor `RSV` lewat `NumberSeriesAllocator`, kode 32 byte acak dengan hash SHA-256, snapshot klasifikasi, dan riwayat `CREATED`, di bawah kunci per pasien. Respons buat memuat link sekali, teks WA tanpa poli/dokter, dan No. HP pasien |
| Requirement/decision | `FR-RP-030` s.d. `032`, `FR-RP-062`; `RJ-PRI-DEC-007`, `008`, `011`, `012`, `021`; `VAL-RP-01` s.d. `06` |
| Kontrak | `api-contract.md` Outpatient Priority Reservation; `permission-audit-matrix.md` RP-1 (`Read`, `Create`) |
| Reuse | `NumberSeriesAllocator`, resolver, `FindActiveEncounterAsync`/`BlockActiveEncounter`, pola kunci `RJ-DOC-REV-BE-007`, pola pencarian pasien |
| Cakupan | Controller baru, `OutpatientPriorityReservationService` (perintah buat + kueri), `DTOs/OutpatientPriorityReservationDtos.cs`, registrasi DI, deret nomor (seed bila allocator membutuhkannya) |
| Dependency | `BE-RJP-005` |
| Acceptance criteria | `R-RP-08`, `R-RP-09` (termasuk paralel), `R-RP-10`, `R-RP-23` (kolom hanya hash), `UAT-RP-07` s.d. `09` |
| Verifikasi | Runtime HTTP; uji paralel dua permintaan bersamaan |
| Risiko/pemilik | Kode link bocor ke log bila salah tulis. Periksa log sesudah run. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti; log tidak memuat kode, hash, atau No. HP |

### `BE-RJP-007` ✅ — Perintah petugas

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 18/18 PASS (R-RP-15/19/23 kirim ulang/24/26); link publik menyusul BE-RJP-012, lengkapi 403 menyusul BE-RJP-008 — [laporan](../task/report/backend/BE-RJP-007.md) |
| Outcome | `POST /{id}/arrival`, `/{id}/arrival-reversal`, `PUT /{id}/screening`, `POST /{id}/arrival-code`, `POST /{id}/cancel`, dengan `RowVersion`, riwayat per kejadian, dan matriks status RP-B/RP-C |
| Requirement/decision | `FR-RP-033` s.d. `035`, `037`, `FR-RP-052`; `RJ-PRI-DEC-012`, `019`, `022`, `023`, `028`, `029`; `VAL-RP-10` s.d. `14` |
| Kontrak | `api-contract.md`; `state-transition-matrix.md` RP-B, RP-C; `permission-audit-matrix.md` RP-1 (`Complete`, `Screen`, `Create`, `Cancel`) |
| Reuse | `OutpatientPriorityReservationService` dari `006` |
| Cakupan | Lima endpoint dan perintah service |
| Dependency | `BE-RJP-006` |
| Acceptance criteria | `R-RP-15`, `R-RP-19`, `R-RP-23` (kirim ulang), `R-RP-24`, `R-RP-26` |
| Verifikasi | Runtime HTTP dengan akun admisi dan akun petugas skrining (`Read` + `Screen`) |
| Risiko/pemilik | Peran uji harus diatur lewat Akses Role (izin user untuk SQL bila perlu). Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `BE-RJP-008` ✅ — Pelengkapan dan jalur lain

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 16/16 + 2/2 PASS (R-RP-16/17/19 lengkapi/20/21 penutupan/22/27/30, UAT-RP-11..13), tanpa regresi — [laporan](../task/report/backend/BE-RJP-008.md) |
| Outcome | `POST /patient-encounters/admin` menerima `priorityReservationId`. Service memeriksa status Sudah Tiba, pasien, tanggal, dan izin `Complete`, lalu memakai snapshot klasifikasi (Reguler bila membership snapshot Dibatalkan). Kunjungan dan reservasi `Converted` tersimpan dalam satu transaksi, dengan `RegistrationSource = 6`. Kunjungan RJ berpoliklinik lain pada tanggal sama menutup reservasi terbuka sebagai `RegisteredElsewhere`. Kiosk menolak field ini |
| Requirement/decision | `FR-RP-040` s.d. `043`; `RJ-PRI-DEC-009`, `010`, `016`, `024`, `027`; `RJ-PRI-DEC-030` usulan (1); `VAL-RP-15` s.d. `20` |
| Kontrak | `api-contract.md` Patient Encounter (diperbarui); `integration-contract.md` `INT-RP-03`, `04` |
| Reuse | `CreateEncounterCoreAsync` (jadwal, kuota, penjamin, antrean), `AccessPermissionService` |
| Cakupan | `PatientEncounterController.cs` (alur pembuatan), `PatientEncounterDtos.cs`, service reservasi (`PrepareCompletionAsync`, `CompleteInTransactionAsync`, `CloseForEncounterAsync`) |
| Dependency | `BE-RJP-007`, `BE-RJP-002` |
| Acceptance criteria | `R-RP-16`, `R-RP-17`, `R-RP-20`, `R-RP-21` (penutupan), `R-RP-22`, `R-RP-27`, `R-RP-30`, `UAT-RP-11` s.d. `13` |
| Verifikasi | Runtime HTTP petugas dan kiosk, termasuk regresi pendaftaran Reguler, IGD, dan Laboratorium langsung |
| Risiko/pemilik | Menyentuh jalur pendaftaran yang dipakai kiosk, petugas, dan IGD. Wajib uji regresi `R-RP-30`. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti, tanpa regresi |

### `BE-RJP-009` ✅ — Penutupan Tidak Datang

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 5/5 dua kali (R-RP-18 idempoten, UAT-RP-10) + peran Web tanpa proses PASS — [laporan](../task/report/backend/BE-RJP-009.md) |
| Outcome | `OutpatientPriorityReservationClosureHostedService` saat start dan pukul 00.05 WIB, per batch 200, idempoten, hanya pada `Runtime:Role` `Worker`/`All` |
| Requirement/decision | `FR-RP-036`; `RJ-PRI-DEC-013`; `INT-RP-08` |
| Kontrak | `state-transition-matrix.md` RP-B |
| Reuse | Pola `PatientMembershipLifecycleHostedService` |
| Cakupan | Hosted service, perintah sistem di service, registrasi `Program.cs` |
| Dependency | `BE-RJP-006` |
| Acceptance criteria | `R-RP-18`, `UAT-RP-10` |
| Verifikasi | Runtime dengan data reservasi bertanggal kemarin (dibuat lewat API lalu tanggalnya dimundurkan dengan izin) |
| Risiko/pemilik | Memundurkan tanggal butuh SQL langsung dengan izin user. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `BE-RJP-010` ✅ — Realtime daftar reservasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime SignalR 9/9 PASS (R-RP-14 sisi backend, payload tanpa nama) + regresi display — [laporan](../task/report/backend/BE-RJP-010.md) |
| Outcome | `QueueHub.JoinPriorityReservationBoard` (memeriksa `OutpatientPriorityReservation : Read`). `NotifyPriorityReservationChangedAsync` dikirim dari setiap perintah petugas dan konfirmasi |
| Requirement/decision | `FR-RP-038`; `RJ-PRI-DEC-014`, `026`; `INT-RP-07` |
| Kontrak | `api-contract.md` hub; `permission-audit-matrix.md` RP-1 (`Read`) |
| Reuse | `QueueHub`, `QueueRealtimeService`, `AccessPermissionService` |
| Cakupan | `Hubs/QueueHub.cs`, `Services/QueueRealtimeService.cs`, `DTOS/QueueRealtimeDtos.cs`, pemanggilan dari service reservasi |
| Dependency | `BE-RJP-007` |
| Acceptance criteria | `R-RP-14` (sisi backend) |
| Verifikasi | Klien SignalR uji dengan akun petugas skrining |
| Risiko/pemilik | Menyentuh berkas yang sama dengan `BE-RJP-004`. Kerjakan berurutan. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti; payload tanpa nama pasien |

### `BE-RJP-011` ✅ — Nama ruang tunggu prioritas

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 3/3 dua kali (R-RP-29 sisi API, nama dari konfigurasi; Reguler null) — [laporan](../task/report/backend/BE-RJP-011.md) |
| Outcome | Field `PriorityWaitingLocationName` pada item Nurse Station, Doctor Queue, dan `QueueClassification` pembuatan kunjungan, terisi dari konfigurasi bila prioritas |
| Requirement/decision | `FR-RP-060`, `061`; `RJ-PRI-DEC-017`, `018` |
| Kontrak | `api-contract.md` Nurse Station Queue dan Doctor Queue (diperbarui), Patient Encounter |
| Reuse | DTO yang ada, `PriorityReservationOptions` dari `005` |
| Cakupan | `DTOs/NurseStationQueueDtos.cs`, `DTOs/DoctorQueueDtos.cs`, `DTOs/PatientEncounterDtos.cs`, controller antrean terkait |
| Dependency | `BE-RJP-005` |
| Acceptance criteria | `R-RP-29` (sisi API); antrean Reguler `null` |
| Verifikasi | Runtime HTTP |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `BE-RJP-012` ✅ — Endpoint publik link konfirmasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — build PASS (0 error), EF bersih, QBE Strict PASS, runtime 12/12 PASS (R-RP-11/12/13/25, UAT-RP-14/15, kode tidak masuk log); go-live menunggu RJ-PRI-OQ-003 dan tinjauan keamanan — [laporan](../task/report/backend/BE-RJP-012.md) |
| Outcome | `PriorityReservationArrivalController` (`[AllowAnonymous]`) dengan `GET /{code}` dan `POST /{code}/confirm`, policy `PriorityReservationArrivalPublic` 20/menit/IP, respons tanpa data pribadi, 404 seragam, dan `NOT_YET_OPEN` sebelum hari H. Konfirmasi memicu realtime `BE-RJP-010` |
| Requirement/decision | `FR-RP-050`, `051`, `053`; `RJ-PRI-DEC-008`, `012`, `028`; `VAL-RP-07` s.d. `09`; `NFR-RP-007` |
| Kontrak | `api-contract.md` Priority Reservation Arrival; `permission-audit-matrix.md` RP-3 |
| Reuse | `AddRateLimiter` (pola `KioskPatientLookupController`) |
| Cakupan | Controller publik, perintah konfirmasi di service, policy di `Program.cs` |
| Dependency | `BE-RJP-007`, `BE-RJP-010` |
| Acceptance criteria | `R-RP-11` s.d. `13`, `R-RP-25`, `UAT-RP-14`, `UAT-RP-15` |
| Verifikasi | Runtime HTTP tanpa cookie login |
| Risiko/pemilik | Satu-satunya endpoint bisnis tanpa login. Tinjauan keamanan sebelum go-live. **Go-live menunggu `RJ-PRI-OQ-003`** (IT/infrastruktur). Pemilik: Sukma Giri |
| DoD | Kriteria terbukti. Go-live tercatat terpisah setelah `RJ-PRI-OQ-003` dijawab |

### `BE-RJP-013` ✅ — Ganti tanggal reservasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-21 — build PASS (0 error), EF bersih, QBE Strict PASS (8 berkas), migration diterapkan ke QuilvianNewDevSukma, runtime 23/23 PASS (R-RP-31..42 sisi API/SignalR, 403 dengan akun non-SuperAdmin); sisi layar R-RP-41 di FE-RJP-009 — [laporan](../task/report/backend/BE-RJP-013.md) |
| Outcome | `POST /{id}/reschedule` mengganti tanggal reservasi Menunggu Kedatangan bila hari ini (WIB) < tanggal kunjungan. Nomor tetap, status member dinilai ulang, snapshot diganti, link baru terbit dan link lama mati, riwayat `RESCHEDULED` mencatat tanggal lama dan baru, lalu event realtime membawa `previousVisitDate`. Bila gagal, tidak ada yang berubah |
| Requirement/decision | `FR-RP-090` s.d. `096`; `RJ-PRI-DEC-034` s.d. `039`; `INV-RP-01`, `03` (revisi `34`), `05`, `06`, `10`; `VAL-RP-03` s.d. `06`, `11`, `12`, `14`, `22` s.d. `25` |
| Kontrak | `RJ-PRI-RESV-001@1.2.0`: `api-contract.md` *Amendment RP-GT*, `state-transition-matrix.md` RP-GT-A/B, `validation-matrix.md` RP-GT, `integration-contract.md` RP-GT, `permission-audit-matrix.md` RP-GT |
| Reuse | `LockPatientAsync`, `FindOpenReservationAsync`, `HasActiveOutpatientEncounterOnAsync`, `ValidateVisitDate`, `NewArrivalCode`/`HashArrivalCode`, `OutpatientQueueClassificationService.ClassifyAsync`, `BuildIssuedResponseAsync`, pola controller `ReissueArrivalCode` (log, realtime, respons terbit) |
| Cakupan | (1) Model dan konfigurasi `RegPriorityReservationHistory`: `EventRescheduled`, `FromVisitDate`, `ToVisitDate` (`date`, nullable). (2) Migration `AddPriorityReservationHistoryVisitDates` beserta Designer dan snapshot, lalu diterapkan ke `QuilvianNewDevSukma` setelah memeriksa migration modul lain yang `(Pending)`. (3) `RescheduleAsync` dengan urutan pemeriksaan `02` GT.4. (4) DTO `ReschedulePriorityReservationRequest`; `canReschedule`; `fromVisitDate`/`toVisitDate` di riwayat; `previousVisitDate` di event realtime; `PreviousVisitDate` di hasil perintah. (5) Teks WA versi ganti tanggal dan nama kejadian "Tanggal kunjungan diganti". (6) Action controller `Reschedule` dengan `OutpatientPriorityReservation : Create`, dan deskripsi `[AccessAction("Create")]` yang diseragamkan |
| Dependency | `BE-RJP-007` ✅ (perintah petugas, `RowVersion`, riwayat), `BE-RJP-010` ✅ (board realtime) |
| Acceptance criteria | `R-RP-31` s.d. `R-RP-42` (`testing/acceptance-test-matrix.md` *Amendment RP-GT*), sisi API dan SignalR. `R-RP-41` bagian layar menjadi milik `FE-RJP-009` |
| Verifikasi | Pola Bank Darah: build `-p:UseSharedCompilation=false`, `has-pending-model-changes`, QBE Strict, runtime HTTP ke `7185` terhadap `QuilvianNewDevSukma` dengan data samaran (tanggal relatif `H+n`), klien SignalR untuk event. `dotnet test`: `NOT RUN — tidak ada project test` |
| Risiko/pemilik | (1) Kunci pasien wajib dipasang di dalam transaksi sebelum memeriksa tanggal baru. Bila tidak, dua permintaan paralel dapat lolos, dan hanya unique index yang menahannya. (2) Akun uji non-SuperAdmin dibutuhkan untuk 403 (`R-RP-38`). Bila tidak tersedia, 403 dicatat belum diuji, seperti `BE-RJP-006`. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti; migration diterapkan ke DB uji; laporan `task/report/backend/BE-RJP-013.md`; status roadmap dan traceability diperbarui |
| Handoff | QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend dan `docs/engineering/` |

## Di luar roadmap ini

| Butir | Penyebab | Selama MVP |
|---|---|---|
| WA otomatis, lonceng notifikasi | `RJ-PRI-OQ-004` | Tombol `wa.me` manual |
| Diskon member di tagihan | `RJ-PRI-OQ-005` (`DEC-CRM-001` q5) | Tanpa diskon |
| Peringatan lama tunggu, laporan rekap | `GATE-RP-03`, gate dimensi 18 | Daftar dengan saringan |
| Ekstraksi `CreateEncounterCoreAsync` ke service | `02` RP.14 | Tetap di controller legacy |
| Isi master tingkat `PriorityQueue = true` | `RJ-PRI-OQ-006` (Yoga/Marketing) | Data go-live, bukan task kode |
| Centang Akses Role | Operasional | Daftar periksa go-live di traceability |
| Peringatan reservasi terbuka di tanggal lain saat membuat reservasi | `RJ-PRI-OQ-014` (`LATER SLICE`) | Admisi memakai Ganti Tanggal (`BE-RJP-013`) alih-alih membuat reservasi baru |
