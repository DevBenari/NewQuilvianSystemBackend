# Roadmap Delivery Backend — Integrasi Rawat Inap ↔ Kasir / Billing

> ## Berkas Baru — Sub-modul `integrasi-billing` (Slice `INP-S22`)
>
> Berkas ini adalah register kerja pengiriman (*delivery roadmap*) backend untuk **Integrasi Rawat Inap ↔ Kasir / Billing**, mencakup 6 kemampuan integrasi kanonik (`INT-CAP-01` s.d. `INT-CAP-06` / `RANAP-INT-001` s.d. `006`).
>
> | Hal | Keterangan |
> |---|---|
> | Letak berkas | `docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md` |
> | Hubungan dengan roadmap lain | **Terpisah dan independen.** Tidak menimpa atau mengganggu `episode-rawat-inap/`, `dokter-rawat-inap/`, atau `keperawatan/`. |
> | Rentang Task ID | **`BE-RWI-127` s.d. `BE-RWI-134`** (8 task vertical slice terfokus) |
> | ID Bebas Berikutnya | **`BE-RWI-135`** |
> | Governance & Test Policy | Mematuhi `rules/backend/engineering/` dan `rules/backend/TEST_POLICY.md`. |

---

## Metadata

```yaml
module_id: rawat-inap
submodule: integrasi-billing
slice_id: INP-S22
blueprint_id: RWI-BP-001-INT-BIL
blueprint_revision: 1.0.0
roadmap_file: docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
approval_gate: APPROVED
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-17"
approval_decision: RWI-DEC-162
primary_source: "docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md"
contract_version: 1.0.0
decision_source: "docs/module-blueprints/rawat-inap/00-interview-decisions.md revision 26 (RWI-DEC-156 s.d. 162)"
backend_source_sha: fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344
frontend_source_sha: 2c00758832f834cff0288bef4f0d2fcf1161fb52
task_id_range: BE-RWI-127..BE-RWI-134
task_id_next_free: BE-RWI-135
waves: [INT-BE-1, INT-BE-2, INT-BE-3, INT-BE-4]
write_authority: "TIDAK diberikan di sini. Wewenang tulis source, migration, database, dan deployment dinyatakan terpisah per task"
```

## Legenda Status Task

| Tanda | Arti Status |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done terbukti penuh, laporan task tracked tersedia |
| 🟡 | Source code telah dibuat namun kriteria verifikasi belum tuntas 100% |
| ⛔ | Prasyarat dependency belum terpenuhi (tertahan) |
| tanpa tanda | Belum dikerjakan (Ready for execution setelah approval) |

---

## Grafik Urutan Dependency Backend

```text
BE-RWI-127 (Fondasi Outbox & Occupancy Tracking)
    │
    ├──► BE-RWI-128 (Service Outbox & Enqueue Helper)
    │        │
    │        ├──► BE-RWI-129 (Background Worker & Backoff)
    │        │
    │        └──► BE-RWI-130 (Event Admisi & Room Charge Fisik)
    │                 │
    │                 └──► BE-RWI-131 (Mutasi & Koreksi saat Billing OPEN)
    │
    └──► BE-RWI-132 (Kueri Status Kasir Bangsal Bebas Rupiah)
             │
             └──► BE-RWI-133 (Webhook Clearance Kasir & Auto-Reblock)
                      │
                      └──► BE-RWI-134 (Supervisor Override & Pelepasan Fisik Pasien)
```

### Tabel Gelombang Eksekusi

| Gelombang | Boleh Mulai Setelah | Task ID | Lingkup Pekerjaan |
|---:|---|---|---|
| **INT-BE-1** | Approval Blueprint | `BE-RWI-127` | Migrasi tabel `InpIntegrationOutboxes` dan tracking fields `InpBedPlacement`. |
| **INT-BE-2** | `BE-RWI-127` | `BE-RWI-128`, `BE-RWI-132` | Service outbox transaksional dan endpoint kueri status kasir bangsal (bebas rupiah). |
| **INT-BE-3** | `BE-RWI-128`, `BE-RWI-132` | `BE-RWI-129`, `BE-RWI-130`, `BE-RWI-133` | Worker background outbox, event admisi & bed occupied, serta webhook auto-reblock. |
| **INT-BE-4** | `BE-RWI-130`, `BE-RWI-133` | `BE-RWI-131`, `BE-RWI-134` | Mutasi kamar berizin saat billing OPEN, supervisor override darurat, dan event pelepasan bed. |

---

## Tabel Rincian Task Backend

| Task ID | Status | Outcome | Requirement & Keputusan | Kontrak | Kemampuan Existing | Cakupan Pekerjaan | Dependency | Acceptance Criteria | Laporan Task |
|---|:---:|---|---|---|---|---|---|---|:---:|
| `BE-RWI-127` | ✅ | Tabel outbox dan field tracking occupancy terpasang di database | `FR-INT-017`; `RWI-DEC-161`, `RWI-AC-241` | `1.0.0` — Data Dict §2 & §3 | `InpBedPlacement` | Migrasi EF Core untuk `InpIntegrationOutboxes` (index unik `IdempotencyKey`, index filtered pending) dan kolom `PhysicallyLeftAt`, `Version`, `ChangeReason`, `IsSuperseded` pada `InpBedPlacement`. | — | AC-1: Migrasi sukses tanpa downtime.<br>AC-2: Index unik IdempotencyKey aktif.<br>AC-3: Data lama terisi default aman. | [Laporan BE-RWI-127](../task/report/backend/BE-RWI-127.md) |
| `BE-RWI-128` | ✅ | Penulisan event outbox terintegrasi transaksional dalam DbContext | `FR-INT-002`, `FR-INT-017`; `RWI-DEC-161` | `1.0.0` — Backend §3 & §7 | `ApplicationDbContext` | Implementasi `IInpIntegrationOutboxService` dan `InpIntegrationOutboxService`. Enqueue pesan JSON dalam transaksi yang sama dengan entitas bisnis. | `BE-RWI-127` | AC-1: Event tersimpan atomik saat SaveChanges.<br>AC-2: Rollback membatalkan outbox.<br>AC-3: Format IdempotencyKey tervalidasi. | [Laporan BE-RWI-128](../task/report/backend/BE-RWI-128.md) |
| `BE-RWI-129` | ✅ | Event outbox terkirim berkala dengan ketahanan exponential backoff | `FR-INT-018`; `RWI-DEC-161`, `RWI-AC-241` | `1.0.0` — Backend §7.2 | BackgroundService | Implementasi `InpatientIntegrationOutboxWorker` (polling interval 5 detik, batch 50 pesan, formula backoff, penanda DeadLetter setelah 10 kegagalan). | `BE-RWI-128` | AC-1: Pesan Pending berubah Published saat sukses.<br>AC-2: RetryCount bertambah saat gagal.<br>AC-3: NextRetryAtUtc dihitung presisi. | [Laporan BE-RWI-129](../task/report/backend/BE-RWI-129.md) |
| `BE-RWI-130` | ✅ | Sinyal admisi dan room charge fisik terbit tepat waktu | `FR-INT-001`, `FR-INT-004`; `RWI-DEC-156`, `RWI-AC-236` | `1.0.0` — Integration Contract §1.2 | `InpatientAdmissionService` | Hooking event `ADMISSION_CONFIRMED` saat status admisi `Admitted`, dan `BED_OCCUPIED` saat tempat tidur terisi fisik dengan mencatat `OccupancyStartAt`. | `BE-RWI-128` | AC-1: Pasien Admitted menerbitkan event admisi.<br>AC-2: Pasien menempati bed menerbitkan BED_OCCUPIED.<br>AC-3: Draft/Booking tidak menerbitkan tagihan. | [Laporan BE-RWI-130](../task/report/backend/BE-RWI-130.md) |
| `BE-RWI-131` | ✅ | Mutasi kamar aman divalidasi terhadap status billing OPEN | `FR-INT-007` s.d. `010`; `RWI-DEC-157`, `RWI-AC-237` | `1.0.0` — Validation `VAL-INT-002`, `003` | `InpatientBedTransferService` | Validasi status billing pasien ke modul kasir sebelum mutasi; jika `OPEN`, simpan versi baru (`IsSuperseded = true` pada data lama) dan kirim `OCCUPANCY_CORRECTED`; jika `CLOSED`, tolak mutasi. | `BE-RWI-130` | AC-1: Mutasi ditolak saat billing CLOSED.<br>AC-2: Alasan wajib minimal 10 karakter.<br>AC-3: Data lama tidak terhapus fisik.<br>AC-4: Event koreksi terbit. | [Laporan BE-RWI-131](../task/report/backend/BE-RWI-131.md) |
| `BE-RWI-132` | ✅ | Layar bangsal menerima status operasional kasir tanpa nominal rupiah | `FR-INT-011` s.d. `013`; `RWI-DEC-160`, `RWI-AC-240` | `1.0.0` — API §1, Validation `VAL-INT-006` | Controller baru | Endpoint `GET /episodes/{id}/billing-status` (steril rupiah untuk `InpatientNurse:Read`) dan `GET /episodes/{id}/billing-details` (nominal rupiah untuk `InpatientBilling:View`). | `BE-RWI-127` | AC-1: Endpoint status tidak memuat nominal rupiah.<br>AC-2: Endpoint detail menolak perawat biasa (403).<br>AC-3: Daftar kendala blocker tampil informatif. | [Laporan BE-RWI-132](../task/report/backend/BE-RWI-132.md) |
| `BE-RWI-133` | ✅ | Webhook kasir mengaktifkan tombol pulang atau mengunci auto-reblock | `FR-INT-014`, `FR-INT-015`; `RWI-DEC-158`, `RWI-AC-238` | `1.0.0` — API §2, State §1 | Controller webhook | Endpoint webhook `POST /episodes/{id}/discharge-clearance/webhook` untuk sinyal `ClearanceApproved` (buka tombol) dan `ClearanceRevoked` (eksekusi Auto-Reblock seketika). | `BE-RWI-132` | AC-1: ClearanceApproved mengaktifkan kelayakan pulang.<br>AC-2: ClearanceRevoked mengunci kembali (Auto-Reblock).<br>AC-3: Alasan pencabutan kasir tersimpan. | [Laporan BE-RWI-133](../task/report/backend/BE-RWI-133.md) |
| `BE-RWI-134` | ✅ | Supervisor override darurat klinis dan pelepasan fisik pasien final | `FR-INT-005`, `006`, `016`; `RWI-DEC-158`, `159`, `RWI-AC-238`, `239` | `1.0.0` — API §2, Validation `VAL-INT-001`, `004`, `005` | `InpatientDischargeService` | Endpoint `POST /episodes/{id}/supervisor-override` (alasan darurat >= 20 kar, verifikasi PIN, audit log) dan `POST /episodes/{id}/confirm-physical-discharge` (kunci `PhysicallyLeftAt`, terbitkan `BED_RELEASED`). | `BE-RWI-133` | AC-1: Pelepasan fisik ditolak jika clearance belum sah & tanpa override.<br>AC-2: Override sukses membuka pelepasan darurat.<br>AC-3: Event BED_RELEASED memuat jam fisik presisi.<br>AC-4: Audit log override tercatat permanen. | [Laporan BE-RWI-134](../task/report/backend/BE-RWI-134.md) |

---

## Kartu Task Backend Detil

### Kartu `BE-RWI-127`: Migrasi Data & Outbox Table
- **Status:** ✅ Selesai (Laporan: [BE-RWI-127.md](../task/report/backend/BE-RWI-127.md))
- **Outcome:** Skema tabel `InpIntegrationOutboxes` dan kolom tracking hunian tempat tidur terpasang di database.
- **Kriteria Penerimaan (AC):**
  1. File migrasi EF Core `AddInpatientBillingIntegrationOutbox` ter-generate dengan benar di `Migrations/`.
  2. Tabel `InpIntegrationOutboxes` memiliki Primary Key `Id` (UUID), unique index pada `IdempotencyKey`, dan filtered index pada `Status`.
  3. Tabel `InpBedPlacements` memiliki kolom `PhysicallyLeftAt`, `Version` (default 1), `ChangeReason`, dan `IsSuperseded` (default false).
  4. Migrasi dapat di-rollback (`Down()`) secara bersih tanpa error SQL.
- **Definition of Done (DoD):** `dotnet ef migrations add` sukses, script SQL diverifikasi, `dotnet build` mandiri oleh user.

### Kartu `BE-RWI-128`: Service Transactional Outbox
- **Status:** ✅ Selesai (Laporan: [BE-RWI-128.md](../task/report/backend/BE-RWI-128.md))
- **Outcome:** Service `InpIntegrationOutboxService` dapat mendaftarkan event outbox secara transaksional di DbContext.
- **Kriteria Penerimaan (AC):**
  1. Method `EnqueueEventAsync` memvalidasi kelengkapan argumen dan format compound key `IdempotencyKey`.
  2. Data pesan JSON tersimpan di database lokal saat `SaveChangesAsync()` dipanggil.
  3. Jika transaksi bisnis di-rollback, pesan outbox ikut ter-rollback secara atomik.
- **Definition of Done (DoD):** Interface dan implementasi terdaftar di DI `Program.cs`.

### Kartu `BE-RWI-129`: Background Worker Outbox
- **Status:** ✅ Selesai (Laporan: [BE-RWI-129.md](../task/report/backend/BE-RWI-129.md))
- **Outcome:** Worker background mengambil pesan pending dan mengirimkan ke broker/Billing dengan exponential backoff.
- **Kriteria Penerimaan (AC):**
  1. Worker mengambil pesan outbox yang berstatus `Pending` atau `Failed` dengan batas waktu retry telah jatuh tempo.
  2. Pengiriman sukses mengubah status menjadi `Published` dan mencatat `PublishedAtUtc`.
  3. Pengiriman gagal mencatat `LastError`, menambah `RetryCount`, dan menjadwalkan `NextRetryAtUtc` sesuai formula eksponensial.
  4. Pesan yang gagal >= 10 kali ditandai sebagai `DeadLetter`.
- **Definition of Done (DoD):** Hosted service terdaftar di `Program.cs`.

### Kartu `BE-RWI-130`: Integrasi Admisi & Room Charge Fisik
- **Status:** ✅ Selesai (Laporan: [BE-RWI-130.md](../task/report/backend/BE-RWI-130.md))
- **Outcome:** Event `ADMISSION_CONFIRMED` dan `BED_OCCUPIED` diterbitkan secara asinkron ke Billing.
- **Kriteria Penerimaan (AC):**
  1. Pengesahan admisi menjadi `Admitted` memicu event outbox `ADMISSION_CONFIRMED`.
  2. Penempatan bed fisik oleh perawat mencatat `OccupancyStartAt` dan memicu event outbox `BED_OCCUPIED`.
  3. Pasien berstatus Booking atau Draft tidak menerbitkan event tagihan aktif.
- **Definition of Done (DoD):** Alur admisi terhubung ke outbox service, verifikasi data event JSON valid.

### Kartu `BE-RWI-131`: Mutasi Kamar & Validasi Billing OPEN
- **Status:** ✅ Selesai (Laporan: [BE-RWI-131.md](../task/report/backend/BE-RWI-131.md))
- **Outcome:** Mutasi kamar dan koreksi kelas/kamar aman divalidasi terhadap status billing kasir.
- **Kriteria Penerimaan (AC):**
  1. Sistem melakukan pengecekan status billing kasir sebelum mutasi dieksekusi.
  2. Jika billing kasir berstatus `CLOSED`, sistem menolak aksi mutasi dengan kode HTTP 422 (`VAL-INT-002`).
  3. Jika billing `OPEN`, sistem menandai baris penempatan lama `IsSuperseded = true`, membuat baris baru dengan versi increment, dan menerbitkan event outbox `OCCUPANCY_CORRECTED`.
  4. Alasan mutasi wajib diisi minimal 10 karakter (`VAL-INT-003`).
- **Definition of Done (DoD):** Logika validasi terpasang di `InpBedOccupancyService`.

### Kartu `BE-RWI-132`: Kueri Status Kasir Bangsal Bebas Rupiah
- **Status:** ✅ Selesai (Laporan: [BE-RWI-132.md](../task/report/backend/BE-RWI-132.md))
- **Outcome:** Endpoint REST status kasir untuk bangsal memisahkan pandangan operasional non-finansial dari rincian uang.
- **Kriteria Penerimaan (AC):**
  1. Endpoint `GET /episodes/{id}/billing-status` mengembalikan `operationalStatusText`, `statusColor`, `canPhysicallyDischarge`, dan `blockerReasons`.
  2. Field nominal uang (`totalCharges`, `depositBalance`, `outstanding`) bernilai null / tidak disertakan pada respons status operasional.
  3. Endpoint `GET /episodes/{id}/billing-details` mengembalikan breakdown angka rupiah dan memvalidasi izin `InpatientBilling:View`. Pengguna tanpa izin mendapat respons `403 Forbidden` (`VAL-INT-006`).
- **Definition of Done (DoD):** Controller terdaftar di Swagger dengan tag `[Tags("Inpatient Billing Operational")]`.

### Kartu `BE-RWI-133`: Webhook Clearance Kasir & Auto-Reblock
- **Status:** ✅ Selesai (Laporan: [BE-RWI-133.md](../task/report/backend/BE-RWI-133.md))
- **Outcome:** Sinyal clearance dari kasir diterima dan diproses untuk mengaktifkan pelepasan atau mengunci auto-reblock.
- **Kriteria Penerimaan (AC):**
  1. Webhook `POST /episodes/{id}/discharge-clearance/webhook` menerima payload sinyal clearance dari kasir.
  2. Sinyal `CLEARANCE_APPROVED` memperbarui status episode menjadi siap dipulangkan secara fisik.
  3. Sinyal `CLEARANCE_REVOKED` mengeksekusi *Auto-Reblock*: status kelayakan berubah menjadi `Revoked`, alasan pencabutan kasir tersimpan, dan tombol pelepasan fisik terkunci seketika.
- **Definition of Done (DoD):** Endpoint webhook beroperasi, auto-reblock aktif.

### Kartu `BE-RWI-134`: Supervisor Override & Pelepasan Fisik Pasien
- **Status:** ✅ Selesai (Laporan: [BE-RWI-134.md](../task/report/backend/BE-RWI-134.md))
- **Outcome:** Supervisor override kedaruratan klinis dan konfirmasi kepulangan fisik pasien beroperasi aman.
- **Kriteria Penerimaan (AC):**
  1. Endpoint `POST /episodes/{id}/supervisor-override` memvalidasi wewenang `InpatientSupervisor:Override`, PIN otorisasi, dan alasan klinis wajib minimal 20 karakter (`VAL-INT-004`).
  2. Override yang sah mencatat audit log permanen dan membuka izin pelepasan fisik darurat (`VAL-INT-005`).
  3. Endpoint `POST /episodes/{id}/confirm-physical-discharge` memvalidasi kelayakan (`Cleared` atau `Overridden`), mencatat `PhysicallyLeftAt`, menutup `OccupancyEndAt`, mengosongkan tempat tidur, dan menerbitkan event outbox `BED_RELEASED` ke kasir.
  4. Pelepasan fisik ditolak dengan kode 422 bila clearance masih `Pending`/`Revoked` tanpa override supervisor (`VAL-INT-001`), dan waktu keluar divalidasi (`VAL-INT-008`).
- **Definition of Done (DoD):** Controller terdaftar di Swagger dengan tag `[Tags("Inpatient Discharge Clearance")]`.
