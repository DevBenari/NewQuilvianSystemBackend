# Backend Roadmap — Platform Authorization & Access Control

`blueprint_id`: `SEC-BP-001` · Backend task prefix: `BE-SEC` · Frontend task prefix: `FE-SEC` (usulan)
· Revisi roadmap: `2` · Diperbarui 2 September 2026

---

## Kontrak kanonik yang berlaku

**Identitas otorisasi sebuah endpoint adalah pasangan `(resource, action)` pada
`[AccessPermission]`.** Seeder mendaftarkan pasangan itu apa adanya, sehingga kunci yang dicari saat
request masuk selalu ada di registry.

Kontrak ini ditetapkan `BE-SEC-001` dan **tidak berubah** oleh satu pun task di bawah. Business
Permission adalah lapisan **di atas** technical permission, bukan penggantinya.

Resolver transisional yang disetujui:

```
EffectiveTechnicalPermissions(user)
  = LegacyTechnicalPermissions(user)
    UNION
    TechnicalPermissions( EffectiveBusinessPermissions(user) )
```

Sumber Business Permission dapat dinyalakan dan dimatikan selama migrasi. `SysAccessPolicy` tidak
dihapus pada fase awal. Tidak boleh ada *silent privilege loss* maupun *privilege broadening*.

---

## Klasifikasi induk

`BE-SEC-002` diklasifikasikan **`EPIC`** menurut `rules/backend/TASK_CLASSIFICATION.md`: perancangan
ulang yang menyentuh seluruh arsitektur otorisasi dan berjalan multi-fase. Aturan eksekusi
menyatakan task `EPIC` tidak pernah dikerjakan langsung — `STOP → DECOMPOSE → klasifikasikan ulang
setiap fase`.

Dekomposisinya menghasilkan 11 task. Setiap task dapat direview sendiri, dites sendiri, di-rollback
sendiri, dan tidak menuntut migrasi *big-bang*.

---

## Task selesai

| Task ID | Outcome | Trace | Acceptance criteria/verifikasi | Status |
|---|---|---|---|---|
| `BE-SEC-001` | Integritas otorisasi existing pulih dan menjadi baseline yang sah | `SEC-REQ-001..012`, keputusan owner `D1`–`D4`, `N1`–`N3` | Source mismatch `89 → 0`; registry usang `59 → 0`; drift registry `0/0`; `SysAccessPolicy` tidak pernah dibuat seeder; 856 test otomatis lulus; 10 smoke test lulus dengan akun non-SuperAdmin | `COMPLETED` |
| `BE-SEC-002` | Arsitektur Business Permission dan Access Profile ditetapkan; seluruh keputusan owner ditutup; epic didekomposisi menjadi 11 task | `SEC-REQ-013..023`, keputusan owner `D-ARCH-1`–`D-ARCH-9` | Tiga dokumen evidence; 19 Business Permission terklasifikasi `A`/`B`/`C`/`D`; matriks pemecahan 7 → 28 identitas; audit collision prefix `Sec` nihil; audit semantik OR ASP.NET; impact query read-only pada database development; legacy parity matrix per Departemen × Posisi | **`CLOSED / READY FOR COMMIT`** — **bukan** task implementasi |

Bukti `BE-SEC-001`: [`../task/report/backend/BE-SEC-001.md`](../task/report/backend/BE-SEC-001.md)

Bukti `BE-SEC-002`:
[`../evidence/01-be-sec-002-audit-architecture.md`](../evidence/01-be-sec-002-audit-architecture.md),
[`../evidence/02-be-sec-002-decision-closure.md`](../evidence/02-be-sec-002-decision-closure.md),
[`../evidence/03-be-sec-003-pre-implementation-impact.md`](../evidence/03-be-sec-003-pre-implementation-impact.md)

Bukti planning `BE-SEC-003`:
[`../evidence/04-be-sec-003-implementation-plan.md`](../evidence/04-be-sec-003-implementation-plan.md)

Revalidasi dan koreksi `BE-SEC-003`:
[`../evidence/05-be-sec-003-current-head-revalidation.md`](../evidence/05-be-sec-003-current-head-revalidation.md),
[`../evidence/06-be-sec-003-readiness-correction.md`](../evidence/06-be-sec-003-readiness-correction.md)

---

## Task implementasi berikutnya

### 🟡 `BE-SEC-003` — Technical Permission Granularity Hardening (pilot Dokter Rawat Jalan)

| Field | Isi |
|---|---|
| **Status** | 🟡 **`PARTIAL`** — 11 September 2026. **Fase A** (pemecahan 22 identitas di source, commit `85fcc3fd` 4 September) dan **Fase A′** (koreksi `DoctorConsultation.WriteSoap` menjadi identitas ke-23, perbaikan paritas `IsCancel`, regression test, skrip diagnostik baca-saja) **selesai**. **Fase B** (perluasan `SysAccessPolicy`) dan **Fase C** (penyempitan audio antrean) **belum dijalankan**. **2 dari 10 acceptance criteria terpenuhi** (nomor 1 dan 3); nomor 2, 4, 5, 6, 9 menunggu Fase B/C; nomor 7, 8, 10 tidak dapat dibuktikan karena gerbang test mati. **Blocker:** proyek test `UnitTests.InMemory` gagal dikompilasi — 14 galat pada dua berkas milik modul lain (`BillingDepositServiceTests.cs` 11 galat, `PatientEncounterCompanyGuarantorTests.cs` 3 galat) — sehingga seluruh test keamanan ikut mati. `dotnet build` proyek utama **berhasil**, dan keempat berkas yang diubah task ini kompilasi bersih; keadaan database belum terukur. Laporan: [`../task/report/backend/BE-SEC-003.md`](../task/report/backend/BE-SEC-003.md) |
| **Klasifikasi** | `HEAVY` (skor 11) |
| **Planning evidence** | [`../evidence/04-be-sec-003-implementation-plan.md`](../evidence/04-be-sec-003-implementation-plan.md) — scope, split matrix, audit lifecycle seeder, deployment order, rollback, test plan, database impact, known limitations |
| **Impact evidence** | [`../evidence/03-be-sec-003-pre-implementation-impact.md`](../evidence/03-be-sec-003-pre-implementation-impact.md) — hasil query read-only database development |
| **Outcome** | Tujuh identitas technical permission yang terlalu kasar dipecah menjadi 28 identitas, tanpa satu pun Departemen × Posisi kehilangan atau memperoleh kemampuan |
| **Trace** | `SEC-REQ-013`, `SEC-REQ-014`, `SEC-REQ-021`; keputusan `D-ARCH-3`, `D-ARCH-6`, `D-ARCH-7`, `D-ARCH-8`, `D-ARCH-10`, `O-1` |
| **Scope** | Pemecahan pada 6 controller pilot; identitas `QueueVoice.PlayAudio` beserta otorisasi OR; penutupan identitas lama tanpa hard delete; perluasan `SysAccessPolicy` ke *exact historical capability set*; pembaruan test terkunci |
| **Di luar scope** | Business Permission, Access Profile, resolver, registry prefix, `MedicalCertificate`, penyempitan hak siapa pun, pemecahan platform-wide |
| **Dependency** | **`BE-SEC-002` `CLOSED`** — arsitektur, klasifikasi, dan keputusan owner berasal dari sana. Impact report `evidence/03` sudah ditinjau. Wewenang migrasi data `CONDITIONALLY APPROVED` untuk development |
| **Database** | Tidak ada perubahan skema, tidak ada EF migration. Migrasi **data**: `SysActionAccess` aktif 1.076 → 1.097 (28 baru, 7 ditutup); `SysAccessPolicy` fisik 498 → 537 (39 dibuat, nol dihapus); efektif 469 → 500, lalu 508 setelah langkah audio |
| **Acceptance criteria** | **1. Technical permission split selesai** — 28 identitas terdaftar dan terbaca layar Akses Role; 7 identitas lama tertutup tanpa hard delete; `PatientProcedure.Create` tetap aktif dan `PatientProcedure.Select` dibuat. **2. Legacy parity terverifikasi** — untuk setiap Departemen × Posisi, himpunan endpoint yang dapat dijangkau identik sebelum dan sesudah; jumlah pasangan tetap 11. **3. Tidak ada privilege broadening** — nol Departemen × Posisi memperoleh endpoint baru, di luar penyempitan audio yang diputuskan `O-1`. **4. Tidak ada silent privilege loss** — nol Departemen × Posisi kehilangan endpoint; 6 pengguna terdampak diverifikasi satu per satu. **5. Migrasi teruji** — mode laporan dijalankan dan ditinjau lebih dulu; hasil mode tulis sama dengan laporannya; perluasan idempoten. **6. Rollback teruji** — skrip balik dijalankan pada database uji dan mengembalikan `SysAccessPolicy` efektif ke 469 serta pasangan Departemen × Posisi ke 11. **7.** `ReconcileNeverCreatesAccessPolicy` tetap hijau. **8.** `CompatibilityFallbackMatchesApprovedLegacySetExactly` diperbarui secara sadar — jumlah tetap 69. **9.** Otorisasi audio terbukti **OR**, bukan AND. **10.** Tiga test SuperAdmin tetap hijau; seluruh test suite lulus; `has-pending-model-changes` bersih; smoke test akun non-SuperAdmin |
| **Larangan implementasi** | 1. `AllowAnonymous` pada endpoint audio. 2. Menjadikan `QueueDisplayRuntimeRead` sebagai permission user dokter/perawat. 3. Dua atribut otorisasi yang runtime-nya menghasilkan AND. 4. Menyentuh `MedicalCertificate`. 5. Memperbarui `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. 6. Menyempitkan hak siapa pun di luar langkah audio yang tercatat. 7. Menaruh perluasan policy di dalam `AccessMenuSeeder`. 8. Pre-seeding identitas baru sebelum deploy |
| **Risiko/DoD** | Risiko tertinggi di rangkaian ini. DoD: source + migrasi data + test + laporan tracked + bukti parity per Departemen × Posisi |
| **Rollback** | Mandiri, tanpa titik tanpa kembali. Balikkan source; seeder mendaftarkan ulang identitas lama; skrip balik mengaktifkan 8 policy lama dan menonaktifkan 39 policy baru; snapshot tersedia sebagai jaring pengaman |

---

## Task berikutnya, berurutan

| Task ID | Judul | Klasifikasi | Dependency | Perubahan database | Batas rollback | Status |
|---|---|---|---|---|---|---|
| `BE-SEC-004` | Registry prefix `Sec` dan skema katalog Business Permission | `HEAVY` | `BE-SEC-003` | Migration **aditif**: 3 tabel baru | `Down` menghapus 3 tabel kosong; baris registry dicabut | `NOT STARTED` |
| `BE-SEC-005` | Isi katalog Business Permission pilot | `MEDIUM` | `BE-SEC-004` | Data katalog, idempoten | Hapus baris katalog; tabel tetap | `NOT STARTED` |
| `BE-SEC-006` | Skema Access Profile dan penetapan organisasi | `HEAVY` | `BE-SEC-005` | Migration **aditif**: 4 tabel baru | `Down` menghapus 4 tabel | `NOT STARTED` |
| `BE-SEC-007` | Resolver Business Permission mode bayangan | `MEDIUM` | `BE-SEC-006` | Tidak ada — query baca | Hapus service dan registrasinya | `NOT STARTED` |
| `BE-SEC-008` | Aktivasi sumber izin kedua | `HEAVY` | `BE-SEC-007` + laporan bayangan ditinjau | Tidak ada | **Matikan sakelar** | `NOT STARTED` |
| `BE-SEC-009` | API admin Business Access | `HEAVY` | `BE-SEC-008` | Tidak ada skema baru | Cabut controller | `NOT STARTED` |
| `BE-SEC-010` | `GET /api/v1/access/me` | `MEDIUM` | `BE-SEC-008` | Tidak ada | Cabut endpoint | `NOT STARTED` |
| `BE-SEC-011` | Baseline Self Service otomatis | `TBD` | `BE-SEC-008` + keputusan HR | Belum diketahui | Belum dirancang | `NOT DECOMPOSED` |
| `FE-SEC-001` | State izin frontend dan perbaikan guard | `HEAVY` | `BE-SEC-010` + frontend authority | Tidak ada | Kembalikan berkas frontend | `NOT STARTED` |
| `FE-SEC-002` | Layar Manajemen Hak Akses business-oriented | `HEAVY` | `BE-SEC-009`, `FE-SEC-001` | Tidak ada | Cabut rute view baru | `NOT STARTED` |
| `FE-SEC-003` | Guard tab dan tombol Dokter Rawat Jalan | `MEDIUM` | `FE-SEC-001` | Tidak ada | Kembalikan berkas | `NOT STARTED` |

Scope, acceptance criteria, dan berkas yang diperkirakan berubah untuk setiap task ada pada
[`../evidence/02-be-sec-002-decision-closure.md`](../evidence/02-be-sec-002-decision-closure.md)
bagian L.

### Rantai dependency

```
BE-SEC-003 🟡 (hardening identitas)
   └── BE-SEC-004  (registry + skema katalog)
          └── BE-SEC-005  (isi katalog)
                 └── BE-SEC-006  (skema Access Profile)
                        └── BE-SEC-007  (resolver bayangan)
                               └── BE-SEC-008  (aktivasi bersakelar)  ← perilaku berubah di sini
                                      ├── BE-SEC-009  (API admin)
                                      │      └── FE-SEC-002
                                      ├── BE-SEC-010  (/api/access/me)
                                      │      └── FE-SEC-001
                                      │             ├── FE-SEC-002
                                      │             └── FE-SEC-003
                                      └── BE-SEC-011  (Self Service)
```

---

## Task terpisah di luar rantai

### `BE-SEC-012` ✅ — Remediasi *naked endpoint* otorisasi dan invarian verifier

| Field | Nilai |
|---|---|
| **Status** | ✅ **Selesai** 16 September 2026 — source diperbaiki dan invarian ditegakkan. Build `Release` `0 Error(s)`; authorization verifier `PASS` dengan invarian baru `[5]` = 0 endpoint telanjang baru, 8 utang baseline; `MetadataGaps` = 0; identitas wajib `BE-SEC-003` tetap 24/24; fallback tetap 69; `SysActionAccess` source 1.286 → 1.296 (+10). **Penerapan ke lingkungan masih ditahan** [`evidence/13`](../evidence/13-workschedule-dormant-grant-deployment-blocker.md). Laporan: [`BE-SEC-012.md`](../task/report/backend/BE-SEC-012.md) |
| **Sebab** | Audit orphan [`evidence/12`](../evidence/12-authorization-orphan-audit.md) menemukan 20 endpoint tulis HR master data hanya dilindungi `[Authorize]` |
| **Scope** | `WorkSchedule`, `Shift`, `ShiftGroup`, `ShiftPattern`, `WorkCalendar` — masing-masing `GET /{id}`, `PUT`, `PATCH /{id}/status`, `DELETE` |
| **Database** | Tidak ada perubahan skema, tidak ada EF migration, tidak ada eksekusi database |
| **Gerbang terbuka** | Keputusan pemilik modul HR atas hak tertidur `WorkSchedule.Update`/`Delete` — [`evidence/13`](../evidence/13-workschedule-dormant-grant-deployment-blocker.md) |

---

### `BE-SEC-013` ✅ — Penegakan otorisasi `WorkScheduleAssignment` dan pengosongan baseline

| Field | Nilai |
|---|---|
| **Status** | ✅ **Selesai** 16 September 2026 — 8 endpoint `WfpWorkScheduleAssignmentController` diberi `[AccessAction]` + `[AccessPermission]`, baseline naked dikosongkan. Build `Release` `0 Error(s)`; authorization verifier `PASS` dengan **naked = 0 baru, 0 utang baseline**; `MetadataGaps` = 0; identitas wajib `BE-SEC-003` tetap 24/24; fallback tetap 69; registry source Resource 339 → 340 (+1), Action 1.296 → 1.300 (+4). Laporan: [`BE-SEC-013.md`](../task/report/backend/BE-SEC-013.md) |
| **Sebab** | Utang yang dibekukan `BE-SEC-012` — seluruh endpoint controller ini hanya dilindungi `[Authorize]`, dan resource-nya tidak pernah terdaftar sehingga admin tidak dapat membatasinya |
| **Scope** | `GET filters/metadata`, `GET summary`, `GET /`, `GET /{id}`, `POST`, `PUT /{id}`, `PATCH /{id}/status`, `DELETE /{id}` |
| **Database** | Tidak ada perubahan skema, tidak ada EF migration, tidak ada eksekusi database. **Nol hak tertidur** — identitas `WorkScheduleAssignment` tidak pernah ada di registry, sehingga tidak ada `SysAccessPolicy` yang dapat hidup kembali |
| **Gerbang terbuka** | Tidak menambah blocker baru. Penerapan tetap ditahan [`evidence/13`](../evidence/13-workschedule-dormant-grant-deployment-blocker.md) milik `BE-SEC-012` |

---

### `BE-SEC-014` ✅ — Matriks hak akses pemilik dan persiapan penerapan yang aman

| Field | Nilai |
|---|---|
| **Status** | ✅ **Selesai** 16 September 2026 — matriks pemilik ditetapkan, tiga skrip database disiapkan (tidak satu pun dijalankan), baseline pemeliharaan diperbarui. Build `Release` `0 Error(s)`; authorization verifier `PASS`; registry source **1.300 / 340 / 48**; metadata gap 0; fallback 69; `BE-SEC-003` 24/24; naked 0 baru, 0 baseline. Laporan: [`BE-SEC-014.md`](../task/report/backend/BE-SEC-014.md) |
| **Sebab** | `BE-SEC-012` dan `BE-SEC-013` selesai di source tetapi belum boleh diterapkan: penerapan tanpa persiapan menghidupkan hak Finance yang tidak pernah disetujui, dan membuat layar HR kosong bagi semua orang |
| **Keputusan pemilik** | Master data mengikuti kepemilikan departemen. Manajer: Read/Create/Update/Delete. Staff: Read/Create. Finance **tidak** mempertahankan `WorkSchedule.Update`/`Delete` |
| **Artefak** | [`evidence/14`](../evidence/14-owner-policy-matrix-and-deployment-preparation.md) + tiga skrip pada `Migrations/scripts/be-sec-014-*.sql` |
| **Database** | **Tidak ada eksekusi.** Dua skrip tulis berakhir `ROLLBACK`; satu skrip baca-saja. Tidak ada EF migration |
| **Gerbang terbuka** | ~~Daftar posisi staff HR belum disetujui pemilik~~ → ✅ ditutup `BE-SEC-016` 17 September 2026 (`Staff HR`, Read/Create). Urutan sepuluh langkah penerapan **masih** belum dijalankan |

---

### `BE-SEC-015` ✅ — Pengerasan skrip policy `BE-SEC-003B` dan varian eksekusi DBeaver

| Field | Nilai |
|---|---|
| **Status** | ✅ **Selesai** 17 September 2026 — tiga cacat skrip penerapan ditutup, varian DBeaver resmi dibuat, parity dibuktikan mekanis. Build `Release` `0 Error(s)`; authorization verifier `PASS`; registry source **1.300 / 340 / 48**; metadata gap 0; fallback 69; `BE-SEC-003` 24/24; naked 0 baru, 0 baseline. Laporan: [`BE-SEC-015.md`](../task/report/backend/BE-SEC-015.md) |
| **Sebab** | Ditemukan saat validasi `BE-SEC-014`. **1.** Kedua `INSERT` Tahap 1 menghilangkan `UpdateBy`/`DeleteBy`/`CancelBy` — `uuid NOT NULL` tanpa default database — sehingga Tahap 1 akan gagal `23502` saat dijalankan. **2.** Tahap 2 hanya `UPDATE` telanjang tanpa gerbang apa pun, padahal ia satu-satunya tahap yang **mencabut** hak. **3.** Operator memakai DBeaver, sedangkan skrip menuntut psql |
| **Scope** | `Migrations/scripts/be-sec-003b-policy-expansion.sql` + varian `-dbeaver.sql` baru. **Nol berkas source aplikasi** |
| **Pengerasan Tahap 2** | Sasaran dibekukan dari kunci bisnis, lalu ditegaskan **tepat 4** (`PatientProcedure.Update` = 1, `DoctorQueue.Update` = 3) sebelum menulis, jumlah baris yang berubah ditegaskan lewat `RETURNING`, dan dipastikan tidak ada policy di luar sasaran yang tersentuh |
| **Parity** | Dibuktikan lewat diff ternormalisasi: peta 23, identitas wajib 24, aturan `Amend`, sasaran Tahap 1/2, sasaran rollback, kunci alami, kolom `NOT NULL`, dan seluruh kardinalitas **identik**. Yang berbeda hanya cangkang eksekusi dan gerbang operator tambahan |
| **Database** | **Tidak ada eksekusi.** Kedua tahap tulis tetap berakhir `ROLLBACK`. `BE-SEC-003B` Tahap 1 dan Tahap 2 tetap `PAUSED`. Tidak ada EF migration |
| **Gerbang terbuka** | Angka 4 pada gerbang Tahap 2 berasal dari kontrak pemilik dan **belum diukur** pada database; bagian 1.5 wajib dijalankan lebih dulu. Kedua varian belum pernah diuji terhadap database mana pun |

---

### `BE-SEC-016` ✅ — Penutupan matriks pemilik HR (`Staff HR`) dan penegakan 24 + 12 = 36

| Field | Nilai |
|---|---|
| **Status** | ✅ **Selesai** 17 September 2026 — gerbang terbuka `BE-SEC-014` ditutup, matriks pemilik dikodekan dan ditegakkan berlapis. Build `Release` `0 Error(s)`; authorization verifier `PASS`; registry source **1.300 / 340 / 48**; metadata gap 0; fallback 69; `BE-SEC-003` 24/24; naked 0 baru, 0 baseline. Laporan: [`BE-SEC-016.md`](../task/report/backend/BE-SEC-016.md) |
| **Sebab** | `BE-SEC-014` sengaja membiarkan daftar posisi staff HR kosong karena belum disetujui pemilik, sehingga seluruh staf HR tidak memperoleh akses apa pun — termasuk `Read` |
| **Keputusan pemilik** | **FINAL.** Departemen `Human Resource` hanya memiliki dua posisi. `Manajer HR` → Read/Create/Update/Delete. `Staff HR` → Read/Create saja, **tanpa** Update dan Delete. **Finance tidak diputuskan di sini** — satu-satunya pembatasan Finance yang disetujui tetap `Manajer Finance` tidak mempertahankan `WorkSchedule.Update`/`Delete` (`BE-SEC-014`); izin Finance lain berlaku apa adanya |
| **Kardinalitas** | `Manajer HR` 6×4 = **24**, `Staff HR` 6×2 = **12**, total cakupan akhir **36** kunci alami. 36 adalah cakupan akhir, **bukan** jumlah `INSERT` — skrip tetap idempoten |
| **Penegakan** | Sebelum tulis: tepat-satu departemen/posisi, 24/12/36, dan nol sasaran `Update`/`Delete` untuk staff. Sesudah tulis (masih di dalam transaksi): cakupan 24/24 dan 12/12, staff `Update`/`Delete` = 0 dibaca dari database, `Finance` × `Manajer Finance` `WorkSchedule.Update`/`Delete` = 0 (**hanya pasangan itu**, nama departemen dicocokkan persis), nol duplikat, dan nol baris di luar matriks 36 kunci — diperiksa dari daftar `RETURNING` |
| **Kunci bisnis** | Ditinjau ulang: `DepartmentCode` dan `(DepartmentId, PositionCode)` **unik**, nama **tidak**. Pencarian berbasis nama dipertahankan karena itulah bahasa keputusan pemilik, ketidakunikannya ditutup penegasan tepat-satu, dan code dibaca sebagai silang-periksa. Tidak ada konvensi kunci baru |
| **Artefak** | [`evidence/14`](../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian H + `Migrations/scripts/be-sec-014-post-seeder-hr-initial-grants.sql` |
| **Database** | **Tidak ada eksekusi.** Skrip tetap berakhir `ROLLBACK`. Tidak ada EF migration |
| **Gerbang terbuka** | Urutan sepuluh langkah penerapan `evidence/14` bagian E belum dijalankan. Skrip belum pernah diuji terhadap database |

---

| Pekerjaan | Repository | Alasan terpisah |
|---|---|---|
| Perbaikan route tab Surat Dokter: `doctor-certificates` → `medical-certificates` | Frontend | Cacat kontrak yang sudah ada. **Dilarang** diselipkan ke task `BE-SEC` mana pun |
| Pemecahan `MedicalCertificate.Update` (7 endpoint: `issue`, `verify`, `approve`, `reject`, `revoke`, `cancel`, `PUT`) | Backend | Menunggu perbaikan route di atas |
| Tombol "Tidak Hadir" memakai `InstanceAxios`, bukan `fetch` mentah | Frontend | Kebersihan arsitektur |
| Otorisasi SignalR hub `/hubs/queues` | Backend | Belum diaudit apakah `[Authorize]` saja memadai |
| ✅ 8 endpoint `WfpWorkScheduleAssignmentController` tanpa penegakan otorisasi | Backend | Ditemukan invarian naked endpoint `BE-SEC-012`; **ditutup `BE-SEC-013`** 16 September 2026. Baseline `KnownUnenforcedBusinessEndpoints` kini kosong |

---

## Keputusan owner yang sudah ditutup

| Butir | Keputusan | Tanggal |
|---|---|---|
| Runtime architecture | Approach A — lihat kontrak kanonik di atas | 2 September 2026 |
| Prefix entity | `Sec` = *Security*; registry diperbarui pada `BE-SEC-004`, bukan `BE-SEC-003` | 2 September 2026 |
| Access Profile | Bundel dapat dipakai ulang; override hanya `ADDITIVE GRANT`; tanpa DENY | 2 September 2026 |
| Procedure approve/execute/cancel | Tiga permission terpisah; `Approve` sensitif dan fail closed | 2 September 2026 |
| Consultation write vs complete | Capability berbeda; `Complete` adalah transisi workflow | 2 September 2026 |
| Queue audio — mekanisme | `QueueVoice.PlayAudio` **OR** `QueueDisplayRuntimeRead`. `AllowAnonymous` ditolak. `QueueDisplayRuntimeRead` **tidak boleh** menjadi permission user dokter/perawat. Implementasi wajib memakai satu mekanisme yang benar-benar menghasilkan OR | 2 September 2026 |
| Queue audio — penerima `QueueVoice.PlayAudio` (`O-1`) | Delapan Departemen × Posisi pemegang izin antrean: Medis × Dokter Umum, Dokter Spesialis, Dokter IGD; Keperawatan × Perawat Rawat Jalan, Perawat Rawat Inap, Perawat IGD, Kepala Keperawatan, Kepala Ruangan. Mencakup 17 pengguna aktif | 2 September 2026 |
| Medical Certificate | Tetap `BROKEN_DEPENDENCY`; task terpisah | 2 September 2026 |
| Impact query read-only pada database development | Disetujui; sudah dijalankan, hasil pada `evidence/03` | 2 September 2026 |
| Migrasi data development | **Conditionally approved** — boleh dijalankan dalam `BE-SEC-003` bila dry-run membuktikan enam syarat parity | 2 September 2026 |

---

## Keputusan owner yang masih terbuka

| Butir | Menahan task | Sifat |
|---|---|---|
| Daftar Departemen × Posisi yang berwenang **menyetujui** tindakan pasien | `BE-SEC-006` | Kewenangan klinis dan finansial. Default fail closed |
| Daftar Departemen × Posisi yang berwenang **melaksanakan** tindakan pasien | `BE-SEC-006` | Kewenangan klinis |
| **`P-1`** — Topologi deployment untuk lingkungan selain development | Penerapan `BE-SEC-003` **di luar development** | Bila instance lama masih melayani traffic saat instance baru menyala, seeder instance baru menutup identitas lama di database bersama dan instance lama seketika menolak 6 pengguna. Dibutuhkan konfirmasi pola **hentikan-dulu-baru-nyalakan**. Tidak ditemukan setting replica pada source; topologi produksi tidak dapat disimpulkan. Lihat `evidence/04` bagian E.3 dan I.1 |
| Apakah Keperawatan × Bidan termasuk actor pemanggil pasien | Tidak menahan | Bidan dikecualikan dari penerima `QueueVoice.PlayAudio` karena **tidak memegang satu pun izin antrean**. Bila pemilik sistem menganggapnya actor pemanggil, penambahannya satu baris pada langkah audio `BE-SEC-003` |
| Penetapan frontend authority dan prefix `FE-SEC` | `FE-SEC-001` | Kepemilikan modul |
| Siapa yang berwenang menandatangani surat dokter | Task terpisah | Ditunda sampai route diperbaiki |
| Definisi "pegawai aktif" untuk baseline Self Service | `BE-SEC-011` | Keputusan HR |
| 17 policy inert warisan `BE-SEC-001` (5 `SEMANTIC_CHANGED`, 12 `REMOVED_CAPABILITY`) | — | Sengaja fail closed |
| Dua baris proyeksi legacy-unresolved | — | Sengaja dipertahankan |
| Penerapan ke lingkungan selain development | — | Operasional |
| **Hak tertidur `WorkSchedule.Update` / `WorkSchedule.Delete`** | Penerapan `BE-SEC-012` | ✅ **Diputuskan** 16 September 2026 — Finance **dicabut**, Human Resource × Manajer HR **dipertahankan**. Skrip pencabutan siap, belum dijalankan: [`evidence/14`](../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian C.2 |
| Daftar posisi staff Human Resource | Pemberian hak awal `BE-SEC-014` | ✅ **DITUTUP `BE-SEC-016`** 17 September 2026 — audit baca-saja dijalankan, pemilik menetapkan `Staff HR` dengan Read/Create saja. Matriks final 24 + 12 = 36 kunci alami: [`evidence/14`](../evidence/14-owner-policy-matrix-and-deployment-preparation.md) bagian H |
