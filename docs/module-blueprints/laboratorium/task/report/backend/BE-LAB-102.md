# Laporan Perubahan Backend — `BE-LAB-102`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-102` |
| Judul | Aksi jadwal ulang, aturan kelayakan, riwayat, izin, dan seeder |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6av.2 |
| Trace | `FR-21.2`..`FR-21.7`, `FR-21.9`, `FR-21.13`; `LAB-DEC-252`, `254`, `256`, `257`, `261`, `262`, `264`, `268`, `270`..`276`; `02-backend-architecture.md` 33.1, 33.4, 33.10 no. 2–4, 33.11; `INV-61`..`INV-67`, `INV-69` |
| Contract version | `LAB-API-v1` **`r50`** 45.2; `LAB-PERM-v1` **revision 18** bagian 20; `LAB-VAL-v1` **`r22`** `VAL-156`..`VAL-165`; `LAB-STATE-v1` **`r12`** 14.1 — keempatnya **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Dependency | `BE-LAB-101` — ✅ selesai 2026-11-25 (kode sudah masuk commit `4d65ce0f`) |
| Klasifikasi | `HEAVY` — repository 0, berkas diperiksa 2 (±30), berkas diubah 1 (6), logika 2 (lima penjaga lintas modul berurutan), kontrak API 2 (dua endpoint baru), database 1 (persistence yang ada, nol schema), keamanan 2 (aksi izin baru + seeder kebijakan), UI 0 = **10** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Lab, satu baris `Program.cs`; laporan, roadmap, dan traceability modul. **Tanpa** migration, **tanpa** menulis basis data bersama, **tanpa** commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `cce1dbf5` (branch `yoga`) — roadmap disusun pada `4d2c0162`; satu-satunya commit sesudahnya yang menyentuh source Lab adalah `4d65ce0f` (kode `BE-LAB-101` sendiri) |
| Tanggal | 2026-11-26 |
| Status | ✅ **Selesai** — build lulus; harness SQLite **144/144**; kontrol negatif 4 mutan tertangkap seluruhnya. HTTP pada aplikasi berjalan **belum** — menunggu migration `BE-LAB-101` diterapkan ke devYoga (langkah rilis `MVP-16e` no. 1) |

### Gerbang masukan

| Butir | Hasil |
| --- | --- |
| Status task | `SIAP DIKERJAKAN` — `BE-LAB-101` selesai |
| Kontrak | Empat kontrak `approved` 2026-11-25; nol perubahan kontrak pada task ini |
| Hash masukan | 13 berkas pada tabel *Hash masukan* traceability `MVP-16` dihitung ulang (`tr -d '\r' \| sha256sum`) — **13/13 sama** |
| Source | `git log 4d2c0162..HEAD` atas source Lab: hanya `4d65ce0f` (`BE-LAB-101`) — tidak basi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Governance terbaca | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` pada suite Skill (`TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE`) |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix | `Lab` — **`ACTIVE`** |
| Keberlakuan | `LabRescheduleEligibility`, `LabOrderReschedulePermissionSeeder`, `RescheduleAsync`, `GetRescheduleHistoryAsync`, dua endpoint, dua DTO = **`NEW CODE`**; `LabOrderService`, `LabOrderController`, `LabOrderDtos`, `Program.cs` = **`TOUCHED LEGACY`** |
| QBE yang berlaku | `QBE-SVC-001` (controller → service, nol akses context di controller), `QBE-API-001` (`ApiResponse`, pemetaan `404/409/422` pola Lab), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]`), `QBE-DTO-001` (DTO, bukan entity), `QBE-VAL-001` (`VAL-156`..`165`), `QBE-TXN-001` (satu `SaveChangesAsync`, penolakan sebelum tulis), `QBE-LOG-001` (log perubahan dengan pelaku), `QBE-AUD-001` (riwayat di tabel domain, log aplikasi terpisah) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*` — nol entity/konfigurasi baru (dibuat `BE-LAB-101`); `QBE-CODE-*` — nol nomor bisnis |
| Pengecualian | Nol |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-101`, setiap pesanan punya tanggal rencana, tetapi **tidak ada cara memindahkannya**. Pasien yang
datang belum puasa hanya bisa dibatalkan lalu dipesan ulang — nomor pesanan berganti, jejak permintaan dokter terputus,
dan Laporan Pembatalan menghitung pembatalan yang sebenarnya hanya penundaan (BR-157).

*Contoh.* Gula Darah Puasa dr. Arif direncanakan Senin 2 Desember. Senin 07.40 pasien mengaku sudah sarapan. Sebelum
task ini Gilang harus membatalkan pesanan. Sesudah task ini Gilang memindahkannya ke Selasa 3 Desember dengan alasan
*pasien belum puasa*: pesanan, nomor, dan statusnya tetap; satu baris riwayat tercatat.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Memindahkan tanggal rencana pemeriksaan yang belum diambil sampelnya, beralasan, tanpa membatalkan pesanan |
| Pelaku | Pemegang `LabOrder : Reschedule` — bawaan: jabatan pemegang `LabOrder : Hold` (di devYoga: *Analis Laboratorium*); Kepala Instalasi lewat langkah rilis |
| Pemicu | `PUT /lab-orders/{id}/reschedule` |
| Prasyarat | Pesanan masih sebelum sampel diambil, bukan bagian MCU, kunjungannya belum selesai, tanpa CITO aktif |

**Langkah (urutan penjaga).**

1. Pesanan tidak ada atau terhapus → `404`.
2. Pemanggil tidak memegang `LabOrder : Reschedule` → `403` (ditolak filter izin sebelum service berjalan).
3. Badan permintaan cacat: `plannedDate` tidak ada atau bukan tanggal, alasan > 1000 karakter → `400` (`VAL-156`).
4. `version` dikirim dan berbeda dengan versi pesanan → `409` *"Pesanan ini baru saja diubah petugas lain. Muat ulang
   lalu coba lagi."* (`VAL-165`).
5. Kelayakan — **satu rumus** `LabRescheduleEligibility.Evaluate`, urutan tetap; alasan pertama yang kena dikembalikan
   `409`:

| Urutan | Syarat menolak | Pesan | Aturan |
| ---: | --- | --- | --- |
| 1 | `CoveringMcuOrderId` terisi **atau** ada komponen MCU tidak terhapus yang menautkan pesanan | *Pesanan ini bagian MCU; jadwal ulang dilakukan petugas MCU.* | `VAL-164` |
| 2 | Status bukan `Requested`/`Confirmed`, dan bukan `OnHold` dari `Requested`/`Confirmed` | *Pesanan berstatus {status} tidak dapat dijadwalkan ulang.* | `VAL-160` |
| 3 | Ada wadah (selain `Cancelled`/`RecollectionRequired`; `OnHold` dinilai menurut status sebelum ditahan) yang bukan `Planned` | *Sampel sudah diambil; batalkan wadah atau ambil ulang lebih dulu.* — bila seluruh wadah penghalang `Rejected`: *Sampel ditolak; minta ambil ulang lebih dulu.* | `VAL-161` |
| 4 | Kunjungan `Completed`/`Cancelled`/`NoShow` | *Kunjungan pesanan ini sudah selesai; pesan ulang pada kunjungan baru.* | `VAL-162` |
| 5 | Ada permintaan `Ordered` ber-CITO **atau** pemeriksaan ber-CITO yang tidak gugur/batal | *Pesanan memuat pemeriksaan CITO; minta dokter mencabut tanda CITO lebih dulu, atau batalkan lalu pesan ulang.* | `VAL-163` |

6. Isi: tanggal sebelum hari ini WIB → `422` *"Tanggal rencana tidak boleh sebelum hari ini."* (`VAL-157`); sama dengan
   tanggal saat ini → `422` *"Tanggal baru sama dengan tanggal rencana saat ini."* (`VAL-158`); alasan kosong/spasi →
   `422` *"Alasan penjadwalan ulang wajib diisi."* (`VAL-159`).
7. Lolos semua → tanggal rencana berganti, `Version` naik, satu baris `LabOrderReschedule` (tanggal lama, baru, alasan
   ter-*trim*, pelaku, waktu UTC) — **satu** `SaveChangesAsync`. Status pesanan, wadah, permintaan, dan pemeriksaan
   **tidak** disentuh; riwayat perpindahan status **tidak** ditulis.
8. Satu baris log: *"Pesanan LAB-… dijadwalkan ulang dari 2026-12-02 ke 2026-12-03 oleh pengguna {id}."* — tanpa alasan,
   tanpa identitas pasien.
9. Jawaban `200` = detail pesanan terbaru.

**Jalur tidak normal.**

| Contoh | Hasil |
| --- | --- |
| Pesanan `Accepted` dikirim tanpa alasan | `409` status — **bukan** `422` alasan: keadaan dinilai lebih dulu (pola `VAL-75` → `VAL-74`) |
| Pesanan `Confirmed`, wadah A `RecollectionRequired`, pengganti B `Planned` | `200`; A dan B tidak berubah |
| Troponin CITO sudah dipesan, wadah belum dibuat | `409` CITO — CITO dibaca dari **permintaan**, bukan hanya pemeriksaan |
| HbA1c tambahan MCU di luar paket (tanpa `CoveringMcuOrderId`) | `409` MCU — dibaca dari `McuOrderComponent.LabOrderId` |
| Dua petugas memuat versi 4; A menyimpan, B menyimpan | A `200`; B `409` versi; satu riwayat |
| Dua penyimpanan berbalapan tanpa versi | Yang kalah `409` *"Data laboratorium sudah diubah oleh petugas lain…"*; perubahan tanggal **dan** baris riwayatnya batal bersama |
| Empat kali berturut-turut, lalu kembali ke hari ini | Seluruhnya `200`; tanpa batas jumlah maupun jarak (`LAB-DEC-264`) |

**Riwayat.** `GET /lab-orders/{id}/reschedules` (pemegang `LabOrder : Read`) — terbaru di atas, dengan nama pelaku
lewat jalur nama yang sama dengan nama pemesan. Pesanan tanpa riwayat → `200` daftar kosong.

**Izin bawaan.** Saat startup, `LabOrderReschedulePermissionSeeder` (sesudah `AccessMenuSeeder`) menyalin setiap
kebijakan **aktif** `LabOrder : Hold` menjadi `LabOrder : Reschedule` pada departemen–jabatan yang sama — **sekali**:
begitu `Reschedule` pernah punya baris kebijakan (termasuk yang dicabut), seeder tidak berbuat apa-apa lagi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Governance pada Preflight; roadmap 6av, traceability `MVP-16`, `LAB-API-v1` 45, `LAB-VAL-v1` 24, `LAB-PERM-v1` 20,
`LAB-STATE-v1` 14, `02-backend-architecture.md` 33, `testing/acceptance-test-matrix.md` (`T-S20-*`); laporan `BE-LAB-101`.
Source: `LabOrderService.cs` (`HoldAsync`, `ResumeAsync`, `CancelAsync`, `VerifyInstructionAsync`, `GetDetailAsync`,
`CreateByExaminationsCoreAsync` `VAL-67`, `SaveWithConcurrencyGuardAsync`, `ResolvePlannedDate`); `LabOrderController.cs`
(`Cancel`, `ExecuteAsync`, `SortOrder` aksi); `LabOrderDtos.cs`; `LaboratoryEnums.cs`; model `LabOrder`, `LabSpecimen`,
`LabOrderedProcedure`, `LabExamination`, `LabOrderReschedule`, `McuOrderComponent`; `LabSpecimenService`
(`TerminalSpecimenStatuses`); `LabOrderConfiguration` (token `Version`); `OperatingRoomHandoverPermissionSeeder`;
`Program.cs` (urutan seeder); `AccessPermissionService`, `AccessPermissionFilter`, `AccessPermissionAttribute`,
`PermissionRegistryValidator`/`Descriptor`; `LoggerService`; `EncounterStatus`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabRescheduleEligibility.cs` | **Baru** — `Evaluate` dan `IsOverdue` statis tanpa basis data; `LabRescheduleSnapshot`, `LabRescheduleSpecimenState`, `LabRescheduleVerdict`; kode alasan dan pesan sebagai konstanta. `public` supaya `BE-LAB-103` (daftar pantau) memakai rumus yang sama |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `RescheduleAsync` (urutan penjaga 24.1, satu `SaveChangesAsync`, log tanpa alasan), `GetRescheduleHistoryAsync`, `ReadRescheduleSnapshotAsync` (empat kueri baca: wadah, kunjungan, CITO permintaan/pemeriksaan, komponen MCU); konstanta pesan `VAL-158`, `159`, `165`. Komentar `PlannedDateBeforeTodayMessage` diperbarui (kini juga `VAL-157`) |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | `PUT {id:guid}/reschedule` (`[AccessAction("Reschedule", "Reschedule Lab Order", AccessType = Update, SortOrder = 7)]`, `[AccessPermission("LabOrder", "Reschedule")]`, lewat `ExecuteAsync` yang ada); `GET {id:guid}/reschedules` (`LabOrder : Read`, `404`). Tanpa `<summary>` pada action |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `RescheduleLabOrderRequest` (`[Required] DateOnly? PlannedDate`, `[MaxLength(1000)] string? Reason` tanpa `[Required]`, `int? Version`); `LabOrderRescheduleResponse` (tujuh ruas `r50`) |
| `Areas/HealthServices/LaboratoryManagement/Seeders/LabOrderReschedulePermissionSeeder.cs` | **Baru** — pola `OperatingRoomHandoverPermissionSeeder`: sekali jalan, salin kebijakan aktif `Hold` → `Reschedule` |
| `Program.cs` | Satu panggilan `RunStartupSeederAsync("LabOrderReschedulePermissionSeeder", …)` tepat sesudah seeder Kamar Operasi (sesudah `AccessMenuSeeder`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** sesuai `r50` 45.2: dua endpoint baru, dua DTO baru. Endpoint yang ada tidak berubah. Ruas detail `rescheduleCount`/`canReschedule`/`isOverdue` **bukan** task ini (`BE-LAB-103`) |
| Database | **Nol migration, nol perubahan schema.** Menulis `LabOrder` (kolom yang ada) dan `LabOrderReschedule` (tabel `BE-LAB-101`). Membaca `RegPatientEncounter` dan `McuOrderComponent` saja. Seeder menambah baris `SysAccessPolicies` saat startup — di devYoga terjadi pada startup pertama sesudah kode ini berjalan (langkah rilis `MVP-16e` no. 2) |
| Keamanan/Auth | Satu aksi izin baru `LabOrder : Reschedule` (`Update`). Penanda kelayakan tidak pernah menggantikan izin. Alasan (dapat memuat kondisi pasien) hanya tersimpan di tabel riwayat dan dibaca pemegang `LabOrder : Read`; **tidak** masuk log |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders` — **dua endpoint baru**.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/{id}/reschedule` | Memindahkan tanggal rencana pesanan, beralasan; status pesanan tidak berubah | `LabOrder : Reschedule` |
| `GET` | `/{id}/reschedules` | Riwayat jadwal ulang satu pesanan, terbaru di atas | `LabOrder : Read` |

**Contoh permintaan `PUT`.** `{"plannedDate":"2026-12-03","reason":"pasien belum puasa","version":4}`

**Contoh jawaban `GET`.**

```json
[{ "id": "…", "previousPlannedDate": "2026-12-02", "newPlannedDate": "2026-12-03",
   "reason": "pasien belum puasa", "actorUserId": "…", "actorName": "Gilang", "occurredAt": "2026-12-02T00:40:00Z" }]
```

| Kode (`PUT`) | Arti bagi pengguna |
| --- | --- |
| `200` | Tanggal berganti; jawaban = detail pesanan terbaru |
| `400` | Permintaan cacat: tanggal tidak ada/bukan tanggal, alasan > 1000 karakter |
| `403` | Tidak memegang `LabOrder : Reschedule` |
| `404` | *Order laboratorium tidak ditemukan.* |
| `409` | Versi basi; pesanan bagian MCU; status di luar jendela; sampel sudah diambil/ditolak; kunjungan selesai; CITO aktif; balapan simpan |
| `422` | Tanggal lampau; tanggal sama; alasan kosong |

| Kode (`GET`) | Arti |
| --- | --- |
| `200` | Daftar riwayat (boleh kosong) |
| `403` | Tidak memegang `LabOrder : Read` |
| `404` | Pesanan tidak ada |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (sesudah server lokal dipastikan mati dan uji mutasi sesi lain selesai) | `Build succeeded`, **0 error** | `PASS` | Keluaran build |
| Peringatan pada berkas yang disentuh | `CS1573` baru pada record potret → diperbaiki (tag `param` dilengkapi; build ulang 0 kemunculan). Tersisa `CS8602` `LabOrderService.cs:2023,2053` — di `CancelAsync`, sudah tercatat pada laporan `BE-LAB-101` | `PASS` / `EXISTING / ENVIRONMENT ISSUE` | Keluaran build |
| **Uji murni `Evaluate`/`IsOverdue`** tanpa basis data — setiap cabang, setiap status pesanan, setiap status wadah, kunjungan, CITO, MCU, urutan alasan (MCU > status > wadah > kunjungan > CITO), *Terlewat* hanya syarat 2 dan 3 | 41/41 | `PASS` | Harness `be102harness` bagian A |
| `T-S20-03` — `Confirmed` + wadah `Planned`: `200`, tanggal + versi baru, `UpdateBy` pelaku, status pesanan/wadah/kunjungan tetap, **tepat satu** riwayat (lama, baru, alasan ter-*trim*, pelaku, UTC) | Seluruhnya sesuai | `PASS` | Harness bagian B |
| `T-S20-04` — `Requested` tanpa wadah `200`; `OnHold` dari `Confirmed` `200` dan tetap `OnHold`/`StatusBeforeHold` utuh; `OnHold` dari `InProcess`, `Accepted`, `InProcess`, `Completed`, `Cancelled`, `Draft`, `CancelRequested` → `409` | Seluruhnya sesuai | `PASS` | Harness |
| **Uji urutan** — `Accepted` tanpa alasan → `409` (bukan `422`); `Accepted` + tanggal kemarin → `409` | Sesuai | `PASS` | Harness |
| `T-S20-05` — `Cancelled`+`Planned` `200`; `OnHold` dari `Planned` `200`; `OnHold` dari `Collected` `409`; `Rejected` `409` pesan ambil ulang; `RecollectionRequired`+`Planned` `200` (keduanya tidak berubah); `Collected` `409`; wadah terhapus diabaikan | Seluruhnya sesuai | `PASS` | Harness |
| `T-S20-06` — kunjungan `Completed`, `Cancelled`, `NoShow` → `409` `VAL-162` | 3/3 | `PASS` | Harness |
| `T-S20-07` — permintaan `Ordered` ber-CITO tanpa wadah `409`; pemeriksaan ber-CITO `409`; sesudah dicabut `200`; pemeriksaan CITO gugur/batal `200`; permintaan CITO dibatalkan / `Fulfilled` / terhapus `200` | Seluruhnya sesuai | `PASS` | Harness |
| `T-S20-08` — `CoveringMcuOrderId` `409`; komponen MCU di luar paket `409`; komponen MCU terhapus `200`; pesanan biasa `200` | 4/4 | `PASS` | Harness |
| `T-S20-09` — kemarin `422` `VAL-157`; sama `422` `VAL-158`; alasan spasi/tab/baris baru dan `null` `422` `VAL-159`; +90 hari `200`; empat kali berturut-turut `200` (4 riwayat); kembali ke hari ini `200` | Seluruhnya sesuai | `PASS` | Harness |
| **Setiap penolakan** di atas: nol riwayat baru, tanggal **dan** versi tetap | Diperiksa pada setiap kasus `409`/`422` | `PASS` | Fungsi `Tolak` harness |
| `T-S20-14` — A versi 4 `200`; B versi 4 `409` `VAL-165`, satu riwayat; versi basi mendahului kelayakan; tanpa versi tetap diterima | Sesuai | `PASS` | Harness |
| Balapan simpan (entity dimuat lebih dulu, petugas lain menyimpan) → `LabConcurrencyException`; **nol** riwayat, tanggal milik pemenang (`INV-69`) | Sesuai | `PASS` | Harness |
| `404` — `PUT` pesanan tidak ada dan terhapus; `GET` pesanan tidak ada | Sesuai | `PASS` | Harness |
| `VAL-156` — `plannedDate` kosong tidak sah; alasan 1001 karakter tidak sah, 1000 sah; tanggal `03-12-2026` → `JsonException`; contoh `r50` terbaca | Sesuai | `PASS` | Harness (`Validator` DataAnnotations + `System.Text.Json` Web) |
| `T-S20-15` — `GET reschedules` 200, lima peristiwa, terbaru di atas, rantai tanggal bersambung, nama pelaku jalur `r48`; daftar kosong untuk pesanan tanpa riwayat; bentuk JSON tujuh ruas camelCase | Sesuai | `PASS` | Harness |
| `T-S20-15` — Laporan Pembatalan periode yang sama: **kontrol** pesanan yang dibatalkan muncul; 17 pesanan yang hanya dijadwal ulang **tidak** muncul; jejak `LabTransitionHistory` tidak bertambah oleh jadwal ulang | Sesuai | `PASS` | Harness (`LabCancellationReportService.GetListAsync` di SQLite) |
| Log (33.10 no. 4) — satu baris memuat nomor pesanan, tanggal lama/baru, pelaku; **tidak** memuat alasan, `PatientId`, atau nomor kunjungan | Sesuai | `PASS` | Logger penangkap pada harness |
| Metadata endpoint — template route, `AccessAction` `Reschedule`/`Update`/`SortOrder 7` (tunggal di antara aksi `LabOrder`), `AccessPermission`, `ProducesResponseType` 200/400/403/404/409/422; `GET` → `LabOrder : Read` | Sesuai | `PASS` | Refleksi harness |
| **Validator izin startup** — `PermissionRegistryValidator.Validate(BuildFromAssembly(...))` | `IsValid`; `LabOrder.Reschedule` terdaftar pada modul Lab | `PASS` | Harness |
| `T-S20-10` — `AccessPermissionFilter` + `AccessPermissionService` + `UserManager` sungguhan (SQLite): pemegang `Read` saja `403`; pemegang `Update` saja `403`; Analis sebelum seeder `403`; sesudah seeder lolos; sesudah dicabut `403` | Sesuai | `PASS` | Harness |
| `T-S20-11` — seeder startup pertama: 1 kebijakan, hanya jabatan pemegang `Hold` aktif; jabatan dengan `Hold` dicabut tidak; Kepala (pemegang `Update`) tidak; kebijakan `Hold` utuh. Startup kedua no-op; cabut lalu startup ulang → tidak diberikan kembali; aksi `Reschedule` belum hidup → no-op | Sesuai | `PASS` | Harness — **ringkasan 144 lulus, 0 gagal** |
| **Kontrol negatif** — empat mutan pada jebakan roadmap (a) CITO permintaan tidak dibaca, (b) MCU hanya `CoveringMcuOrderId`, (c) `RecollectionRequired` ikut menolak, (f) alasan ikut ke log; source dipulihkan lewat `writeFileSync` lalu hash dicocokkan | **4/4 tertangkap**: (a) `T-S20-07` CITO permintaan → `200` padahal harus `409`; (b) `T-S20-08` komponen MCU di luar paket → `200`; (c) uji murni + `T-S20-05` `RecollectionRequired`+`Planned` → `409`; (f) uji log *tanpa alasan*. Sesudah pemulihan: hash kedua berkas sama dengan sebelum mutasi, build ulang, harness kembali **144/144** | `PASS` | `mut102-hasil.log` |
| HTTP pada aplikasi berjalan (`200`/`403` dengan akun asli) | — | `NOT RUN` | Lihat di bawah |

**Tidak dijalankan:**

- **HTTP pada aplikasi berjalan** (`NOT RUN`): aplikasi lokal memakai devYoga, yang belum memiliki kolom
  `LabOrder.PlannedDate` dan tabel `LabOrderReschedule` (migration `BE-LAB-101` belum diterapkan — langkah rilis
  `MVP-16e` no. 1, atas instruksi pemilik). Menyalakan aplikasi juga akan menjalankan `AccessMenuSeeder` dan seeder baru
  ini terhadap basis data bersama — itu langkah rilis no. 2, bukan uji. Jalur izin dibuktikan dengan filter, service izin,
  dan `UserManager` sungguhan di harness; HTTP `403` dengan akun asli (sandi diminta ke pemilik) dikerjakan sesudah
  langkah rilis 1–2.
- **Uji pada PostgreSQL sungguhan**: nol SQL baru yang khas PostgreSQL — kueri penjaga hanya `Any`/`Where` atas kolom
  enum dan Guid. Harness SQLite memadai; tanggal `DateOnly` sudah dibuktikan di PostgreSQL oleh `BE-LAB-101`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-338`, `AC-343` / `T-S20-03` — jadwal ulang berhasil, satu riwayat, status tetap | Terpenuhi (harness) | Bagian 5 |
| `AC-341` / `T-S20-04` — jendela status | Terpenuhi (harness) | Bagian 5 |
| `AC-341`, `AC-355` / `T-S20-05` — wadah efektif | Terpenuhi (harness) | Bagian 5 |
| `AC-347` / `T-S20-06` — kunjungan selesai | Terpenuhi (harness) | Bagian 5 |
| `AC-348` / `T-S20-07` — CITO permintaan dan pemeriksaan | Terpenuhi (harness + mutan a) | Bagian 5 |
| `AC-350` / `T-S20-08` — MCU termasuk komponen di luar paket | Terpenuhi (harness + mutan b) | Bagian 5 |
| `AC-342`, `AC-349` / `T-S20-09` — isi dan tanpa batas | Terpenuhi (harness) | Bagian 5 |
| `AC-340` / `T-S20-10` — izin; validator startup lulus | Terpenuhi (harness, filter sungguhan). HTTP akun asli menunggu langkah rilis | Bagian 5 |
| `AC-354` / `T-S20-11` — seeder sekali jalan, Kepala tidak dari seeder | Terpenuhi (harness) | Bagian 5 |
| `T-S20-14` — layar basi | Terpenuhi (harness) | Bagian 5 |
| `AC-352` / `T-S20-15` — riwayat terbaca, bukan pembatalan | Terpenuhi (harness, dengan kontrol laporan) | Bagian 5 |
| Uji urutan: `Accepted` tanpa alasan → `409` | Terpenuhi | Bagian 5 |
| Nol baris riwayat dan tanggal tetap pada setiap penolakan | Terpenuhi | Bagian 5 |
| DoD — nol migration baru | Terpenuhi | `git status` — tidak ada berkas `Migrations/` |
| DoD — laporan `BE-LAB-102.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap rancangan (dicatat, kontrak tidak diubah) | (1) **`RescheduleLabOrderRequest.PlannedDate` bertipe `DateOnly?` ber-`[Required]`**, bukan `DateOnly` seperti tertulis di roadmap: ruas `DateOnly` biasa yang tidak dikirim terisi diam-diam `0001-01-01` dan berakhir `422` `VAL-157`, padahal kontrak menetapkan `400` `VAL-156`. Bentuk JSON (`"yyyy-MM-dd"`, wajib) sama dengan `r50`. (2) **`403` mendahului `404`** pada runtime: filter izin berjalan sebelum service, sehingga pengguna tanpa izin menerima `403` juga untuk pesanan yang tidak ada — perilaku seluruh endpoint Lab, tidak diubah. (3) **Wadah campuran** `Rejected` + `Collected`: pesan *sampel sudah diambil* (lebih umum); pesan *ambil ulang* hanya bila setiap wadah penghalang `Rejected` — kontrak tidak menyebut kasus campuran. (4) **Balapan simpan** memakai pesan `LabConcurrencyException` yang ada (*"Data laboratorium sudah diubah oleh petugas lain. Muat ulang lalu ulangi tindakan Anda."*); pesan `VAL-165` dipakai untuk versi yang dikirim. Keduanya `409`. (5) Badan `409` kelayakan membawa **pesan** saja; kode alasan (`McuLinked`, …) tersedia di `LabRescheduleVerdict` dan akan tampil pada penanda detail/daftar `BE-LAB-103` |
| Peringatan | Sesudah kode ini berjalan di devYoga, **startup pertama** memberi `LabOrder : Reschedule` kepada pemegang `LabOrder : Hold` (langkah rilis no. 2) dan tidak lagi sesudahnya. Kepala Instalasi diberi lewat Role Access (no. 3) — baca set jabatan dulu, POST menimpa seluruh set |
| Masalah yang diketahui | Harness SQLite kini perlu mencabut CHECK constraint ber-`btrim` (Payroll, Procurement — dari merge `cce1dbf5`) sebelum `EnsureCreated`; artefak harness, bukan aturan Lab |
| Risiko tersisa | Kelayakan detail/daftar belum tampil — `BE-LAB-103`. Uji HTTP akun asli dan data uji nyata (`MVP-16e` no. 4–5) menunggu langkah rilis |
| Perubahan sampingan | `NONE`. Working tree juga memuat perubahan SDM (`OfficialDuty*`, `HrAttachmentStorage`, `Tests/…HumanResourceTests`, `BE-HRD-467.md`) milik **sesi lain** yang berjalan bersamaan — tidak disentuh |
| Interupsi | Build ditunda ±15 menit: sesi lain menjalankan uji mutasi `BE-HRD-467` (`dotnet test` per mutan) atas proyek yang sama — ditunggu sampai `SELESAI`, tanpa dihentikan. Saat kontrol negatif saya berjalan, sesi itu memulai `dotnet test` HR penuh; mutan saya hanya menyentuh dua berkas Lab dan tetap terkompilasi, sehingga build HR mereka mungkin memuat satu mutan Lab sementara — tidak memengaruhi uji HR. Source sudah dipulihkan dan dibangun ulang sesudahnya |
| Status Git | Berkas task: ` M` `LabOrderController.cs`, `LabOrderDtos.cs`, `LabOrderService.cs`, `Program.cs`; `??` `Services/LabRescheduleEligibility.cs`, `Seeders/LabOrderReschedulePermissionSeeder.cs`, laporan ini; ` M` roadmap dan traceability. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `BE-LAB-103` (penanda kelayakan, *Terlewat*, daftar pantau, kiosk) — kini tidak lagi menunggu; memakai `LabRescheduleEligibility` apa adanya. Penerapan migration ke devYoga menunggu instruksi pemilik |

---

## Susulan 2026-11-27 — label status Indonesia pada `VAL-160`

| Butir | Isi |
| --- | --- |
| Pemicu | UAT `MVP-16e` (`backend-roadmap.md` 6av.4): alasan `StatusNotEligible` di devYoga berbunyi *Pesanan berstatus **InProcess** …* — nama enum terbaca petugas di tombol nonaktif dan respons `409`. Instruksi pemilik modul (Yoga Aji Pratama, 2026-11-27): *ganti label status jadi bahasa Indonesia* |
| Perubahan | `Services/LabRescheduleEligibility.cs` `StatusNotEligibleMessage`: `{status}` kini `LabFilterMetadataFactory.LabelOrderStatus(status)` — label yang sama dengan layar, penyaring, dan berkas Lab (`LAB-DEC-250`). Satu berkas, satu baris logika; detail, baris daftar pantau, dan `409` ikut berubah karena memakai rumus yang sama (`INV-67`) |
| Contoh | `InProcess` → *Sedang Dikerjakan*; `Completed` → *Selesai*; `Accepted` → *Diterima*; `OnHold` → *Ditahan*; `Draft` → *Draf*; `CancelRequested` → *Pembatalan Diminta*; `Cancelled` → *Dibatalkan* |
| Kontrak | `LAB-VAL-v1` `r22` `VAL-160` menulis templat *"Pesanan berstatus {status} tidak dapat dijadwalkan ulang."* tanpa menetapkan bentuk `{status}` — perubahan tetap di dalam templat. Artefak kontrak terkunci hash **tidak** disunting; **penyegaran untuk pass blueprint:** tegaskan `{status}` = label `LAB-DEC-250` pada `VAL-160` (dan contoh `02-backend-architecture.md` 33.4 bila memuat nama enum) |
| Preflight | Sama dengan bagian atas laporan ini — `HealthServices` / `LaboratoryManagement`, prefix `Lab` `ACTIVE`; berkas `NEW CODE` (`BE-LAB-102`); QBE yang berlaku: `QBE-API-001` (bentuk respons tidak berubah). Nol entity, nol migration, nol izin |
| Build | `dotnet build -p:RunAnalyzers=False` sesudah server lokal BE/FE dimatikan — lulus, 2 menit 35 detik |
| Harness `BE-LAB-102` | Salinan dengan ekspektasi **literal** (kamus label ditulis tangan, bukan memanggil kode produksi) + 6 uji *tanpa nama enum* → **150/150** |
| Kontrol negatif | Harness lama (ekspektasi Inggris) atas build baru → 128 lulus, **16 gagal — seluruhnya pemeriksaan teks pesan**; kode `409` dan *riwayat 0→0* tetap lulus |
| Harness `BE-LAB-103` | **126/126** — `T-S20-12` detail = baris = `PUT`, mis. *Pesanan berstatus Ditahan …* dan *… Diterima …* |
| Frontend | Nol perubahan — layar menampilkan alasan backend apa adanya; tidak ada spec yang mengunci teks lama |
| Migration / database | Nol migration; nol tulis ke devYoga |
| Status Git | `??` `Services/LabRescheduleEligibility.cs` (berkas baru `BE-LAB-102`, belum ter-track; kini memuat susulan). Tanpa stage/commit |
