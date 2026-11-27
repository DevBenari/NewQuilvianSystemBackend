# Laporan Perubahan Backend — `BE-LAB-101`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-101` |
| Judul | Tanggal rencana pada pesanan, tabel riwayat, dan migration |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6av.1 |
| Trace | `FR-21.1`; `LAB-DEC-252`, `253`, `263`, `269`; `02-backend-architecture.md` 33.4, 33.7, 33.10 no. 1 dan no. 5; `INV-68`; data dictionary bagian 24 |
| Contract version | `LAB-API-v1` **`r50`** 45.3 (ruas `plannedDate` detail) dan 45.4; `LAB-VAL-v1` **`r22`** `VAL-166` — keduanya **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Dependency | Nol |
| Klasifikasi | `MEDIUM` — repository 0, berkas diperiksa 1 (±15), berkas diubah 2 (11), logika 1, kontrak API 2 (ruas baru), database 2 (kolom + tabel + migration), keamanan 0, UI 0 = **8** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Lab, konfigurasi EF, `ApplicationDbContext`, **berkas** migration; laporan dan roadmap modul. **Tanpa** menerapkan migration ke basis data, **tanpa** commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d2c0162` (branch `yoga`) |
| Tanggal | 2026-11-25 |
| Status | ✅ **Selesai** — kode, harness 25/25, dan bukti SQL migration di PostgreSQL 15 sungguhan. Migration **belum** diterapkan ke devYoga (langkah rilis `MVP-16e` no. 1, atas instruksi pemilik) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix | `Lab` — **`ACTIVE`** pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `LabOrderReschedule` + konfigurasinya = **`NEW CODE`**; `LabOrder`, `LabOrderService`, `LabOrderController`, `LabOrderDtos`, `LabOrderConfiguration`, `ApplicationDbContext` = **`TOUCHED LEGACY`** |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-NAM-001`/`002` (`Lab*`, tanpa `Trx`), satu paket nama `LabOrderReschedule` / `LabOrderRescheduleConfiguration` / `LabOrderReschedules` / `public."LabOrderReschedule"`, `QBE-CFG-001`, `QBE-MOD-002`/`003` (modul terdaftar), `QBE-API-001` (`ApiResponse`, `422` pola Lab), `QBE-DTO-001`, `QBE-VAL-001` (`VAL-166`), `QBE-TXN-001` (penolakan sebelum transaksi dibuka) |
| Pengecualian | Nol. **Utang teknis dipertahankan, tidak dirapikan:** `LabOrderConfiguration.cs` tinggal di `Repositories/Configurations/HealthServices/` dengan namespace `…Configurations.HealthService` (tunggal), berbeda dari konfigurasi Lab lain — `02-backend-architecture.md` 33.4 |

---

## 1. Masalah yang diperbaiki

Pesanan laboratorium tidak punya **tanggal rencana pemeriksaan**. Setiap pesanan dianggap dikerjakan pada hari
diminta, sehingga pemeriksaan yang memang direncanakan untuk hari lain tidak bisa dibedakan dari yang mendesak hari itu,
dan tidak ada tempat untuk mencatat penjadwalan ulang (BR-157).

*Contoh.* dr. Arif memesan Profil Lipid pada Kamis untuk dikerjakan Senin. Sebelum task ini, pesanan itu hanya membawa
waktu diminta Kamis. Sesudah task ini, pesanan bisa lahir dengan tanggal rencana Senin; pesanan yang tidak menyebut
tanggal tetap bertanggal hari dibuat.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Setiap pesanan Lab punya hari rencana pemeriksaan yang jelas — dasar daftar kerja harian dan penjadwalan ulang (`BE-LAB-102`, `103`) |
| Pelaku | Pemesan: dokter poli/IGD/Rawat Inap, petugas Lab, dan jalur MCU — lewat endpoint pembuatan yang sudah ada |
| Pemicu | Pesanan dibuat lewat `POST /lab-orders` atau `POST /lab-orders/by-examinations` |
| Prasyarat | Tidak ada yang baru; aturan pembuatan pesanan yang sudah ada tetap berlaku |

**Langkah.**

1. Pemanggil mengirim permintaan pembuatan pesanan, dengan atau tanpa `plannedDate`.
2. Sistem menghitung **hari ini menurut WIB**, bukan UTC.
3. Bila `plannedDate` kosong → tanggal rencana = hari ini.
4. Bila `plannedDate` ≥ hari ini → dipakai apa adanya, tanpa batas atas.
5. Bila `plannedDate` < hari ini → permintaan **ditolak `422`** *"Tanggal rencana tidak boleh sebelum hari ini."* sebelum
   nomor pesanan dialokasikan dan sebelum apa pun ditulis (`VAL-166`).
6. Pada `by-examinations`, satu tanggal yang sama berlaku bagi **setiap** pesanan hasil pemecahan per disiplin.

| Contoh (hari ini 25 November 2026 WIB) | Hasil |
| --- | --- |
| Tanpa `plannedDate` | Tanggal rencana `2026-11-25` |
| `plannedDate: "2026-11-25"` | Diterima — batas bawah inklusif |
| `plannedDate: "2026-11-28"` | Diterima, tersimpan `2026-11-28` |
| `plannedDate: "2027-12-30"` (+400 hari) | Diterima — tidak ada batas atas (`LAB-DEC-264`) |
| `plannedDate: "2026-11-24"` | `422`, nol pesanan dan nol permintaan pemeriksaan tertulis |

**Status:** tidak ada status baru. **Pesanan lama** diisi oleh migration: tanggal rencana = tanggal waktu diminta **menurut
WIB** (atau waktu dibuat bila waktu diminta kosong). *Contoh:* diminta `2026-10-08 18:30 UTC` (= 9 Oktober 01.30 WIB) →
`2026-10-09`. Pengisian ini **tidak** menulis riwayat jadwal ulang. **Hasil akhir:** setiap pesanan, lama maupun baru,
punya tanggal rencana; tabel riwayat jadwal ulang siap dipakai `BE-LAB-102`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `DATABASE_RULES`, `API_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`);
`Models/LabOrder.cs`; `LabOrderConfiguration.cs`; `LabTransitionHistoryConfiguration.cs` (pola); `ApplicationDbContext.cs`;
`LabOrderService.cs` (`CreateAsync`, `CreateByExaminationsCoreAsync`, `GetDetailAsync`, `MapDetailResponse`);
`LabOrderController.cs` (`Create`, `CreateByExaminations`); `LabOrderDtos.cs`; `LabOrderNumberService` (sadar-provider);
`Helpers/AppDateTimeHelper.cs`; pola `DateOnly` MCU (`McuExaminationService.cs:1574`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | `DateOnly PlannedDate` (wajib); navigasi `ICollection<LabOrderReschedule> Reschedules` |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrderReschedule.cs` | **Baru** — `[Table("LabOrderReschedule", Schema = "public")]`, mewarisi `IdentityModel`: `LabOrderId`, `PreviousPlannedDate`, `NewPlannedDate`, `Reason` (≤ 1000), `ActorUserId`, `OccurredAt` |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | `PlannedDate` → `date`, wajib; index `IX_LabOrder_PlannedDate`. **Tidak dipindah** (utang teknis) |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabOrderRescheduleConfiguration.cs` | **Baru** — kolom `date`, `Reason` `varchar(1000)` wajib, `OccurredAt` `timestamptz`, index `(LabOrderId, OccurredAt)`, FK → `LabOrder` **`Restrict`** |
| `Repositories/ApplicationDbContext.cs` | `DbSet<LabOrderReschedule> LabOrderReschedules` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `CreateLabOrderRequest.PlannedDate` dan `CreateLabOrderByExaminationsRequest.PlannedDate` (`DateOnly?`, opsional); `LabOrderDetailResponse.PlannedDate` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `ResolvePlannedDate` (bawaan hari ini, `VAL-166`) dipanggil di **awal** `CreateAsync` dan `CreateByExaminationsCoreAsync` — jalur MCU ikut lewat inti yang sama; `PlannedDate` diisi pada kedua pembentuk entity; detail dan `MapDetailResponse` membawa `PlannedDate`; `TodayPlannedDate()` dan `PlannedDateBeforeTodayMessage` `internal` untuk dipakai ulang `BE-LAB-102` (`VAL-157`) |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | `Create` (`POST /lab-orders`) kini menangkap `LabOrderValidationException` → **`422`** dan mendeklarasikan `ProducesResponseType` `422`. Sebelumnya endpoint ini **tidak punya jalur `422`** — tanpa tangkapan ini `VAL-166` akan menjadi `500` |
| `Migrations/20261125074224_AddLabOrderPlannedDateAndReschedule.cs` (+ `.Designer.cs`) | **Baru.** `Up` **disunting tangan** dari hasil pembangkitan: EF mengusulkan `NOT NULL` dengan bawaan `0001-01-01`; diganti lima langkah 33.7 — kolom nullable → `UPDATE` WIB → `NOT NULL` → tabel → index. `Down` dari EF apa adanya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Dibangkitkan EF — hanya `PlannedDate`, index, dan `LabOrderReschedule` (diperiksa: nol perubahan modul lain) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** sesuai `r50`: ruas opsional `plannedDate` pada dua permintaan pembuatan; ruas `plannedDate` pada detail pesanan; `POST /lab-orders` memperoleh jawaban `422`. Pemanggil lama (layar dokter poli/IGD/Rawat Inap, MCU, layar Lab) **tidak berubah perilaku**: tanpa ruas = hari ini |
| Database | Satu kolom `LabOrder.PlannedDate` `date NOT NULL` (diisi dari data lama), satu index, satu tabel `LabOrderReschedule` dengan FK `RESTRICT` dan satu index. Berkas migration **ada di working tree**, **belum diterapkan** ke basis data mana pun selain PostgreSQL uji sekali pakai |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint, izin, atau data pribadi baru. `Reason` pada tabel baru ditandai sensitif (dapat menyebut kondisi pasien) untuk `BE-LAB-102` |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders` — **endpoint yang sudah ada, diperluas**.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat satu pesanan; kini menerima `plannedDate` opsional | `LabOrder : Create` |
| `POST` | `/by-examinations` | Membuat pesanan per disiplin; `plannedDate` opsional berlaku bagi setiap pesanan hasil pemecahan | `LabOrder : Create` |
| `GET` | `/{id}` | Rincian pesanan; kini memuat `plannedDate` | `LabOrder : Read` |

| Kode | Arti bagi pengguna |
| --- | --- |
| `201` | Pesanan terbentuk dengan tanggal rencana |
| `422` | *"Tanggal rencana tidak boleh sebelum hari ini."* — tidak ada pesanan yang dibuat |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (dan `--no-incremental` sesudah migration) | `Build succeeded`, **0 error**, 616 peringatan | `PASS` | Keluaran build |
| Peringatan pada berkas yang disentuh | Empat peringatan (`CS1574` `LabOrder.cs:24,28`; `CS8602` `LabOrderService.cs:2023,2053`) — **seluruhnya di luar baris yang diubah** | `EXISTING / ENVIRONMENT ISSUE` | Dicocokkan dengan rentang `git diff -U0` |
| `dotnet ef migrations add AddLabOrderPlannedDateAndReschedule` | Migration + Designer + snapshot; snapshot diperiksa hanya berisi perubahan Lab | `PASS` | `git diff` snapshot |
| Harness SQLite in-memory `be101harness` (scratchpad, model EF penuh) — metadata model: `PlannedDate` wajib `date` + index; `LabOrderReschedule` di `public`, FK `Restrict`, `Reason` 1000 wajib, index `(LabOrderId, OccurredAt)`, tanggal `date`, `DbSet` | 9/9 | `PASS` | Keluaran harness |
| `CreateAsync` tanpa tanggal → hari ini WIB (respons, tersimpan, dan `GetDetailAsync`); nol riwayat jadwal ulang | 4/4 | `PASS` | Keluaran harness — "hari ini WIB" dihitung mandiri dari `TimeZoneInfo`, bukan dari `AppDateTimeHelper` |
| `CreateAsync` +3 hari, +400 hari, = hari ini → diterima; kemarin → `LabOrderValidationException`, jumlah pesanan 4 → 4 | 5/5 | `PASS` | Keluaran harness |
| `CreateByExaminationsAsync` dua disiplin: tanpa tanggal → keduanya hari ini; +2 → keduanya sama; kemarin → ditolak, nol pesanan **dan** nol permintaan | 4/4 | `PASS` | Keluaran harness |
| `LabOrderController.Create` kemarin → **`422`**; tanpa tanggal → `201`; `CreateByExaminations` kemarin → `422` | 3/3 | `PASS` | Keluaran harness — **ringkasan 25 lulus, 0 gagal** |
| **SQL migration di PostgreSQL 15.15 sungguhan** (binari portabel, port 55432, basis data sekali pakai; skrip `dotnet ef migrations script` naik dan turun) — pengisian data lama | `18:30 UTC` → `2026-10-09`; `16:59:59 UTC` → `2026-10-08`; `RequestedAt` kosong + dibuat `17:00 UTC` → `2026-09-02`; `02:00 UTC` → `2026-11-25`; sisa NULL **0** | `PASS` | Keluaran `psql` |
| Kolom `NOT NULL` `date`; kedua index; FK `ON DELETE RESTRICT` (`confdeltype = r`); menghapus pesanan bertaut riwayat ditolak; menyisipkan pesanan tanpa `PlannedDate` ditolak `not-null constraint` | Seluruhnya sesuai | `PASS` | Keluaran `psql` |
| Skrip **turun** lalu **naik** lagi | Turun: tabel, index, kolom, baris riwayat migration hilang; 4 pesanan utuh. Naik ulang: bersih | `PASS` | Keluaran `psql`; server dimatikan dan data dihapus sesudahnya |

Uji manual: `NOT FEASIBLE` — lihat di bawah.

**Tidak dijalankan:**

- **HTTP pada aplikasi berjalan** (`NOT RUN`): aplikasi lokal memakai devYoga; tanpa migration diterapkan, setiap kueri
  `LabOrder` akan gagal karena kolom `PlannedDate` belum ada. Penerapan migration ke devYoga adalah langkah rilis
  `MVP-16e` no. 1 dan **butuh instruksi pemilik** — tidak dilakukan.
- **Tengah malam WIB secara harfiah** (00.30 WIB): jam sistem tidak dapat dipalsukan dalam harness; logika "hari ini"
  memakai `AppDateTimeHelper.OperationalDate()` (WIB) dan dibandingkan dengan perhitungan `TimeZoneInfo` mandiri. Konversi
  zona waktu di SQL dibuktikan di PostgreSQL dengan nilai lintas tengah malam.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-339` / `T-S20-01` — pembuatan tanpa tanggal = hari ini WIB | Terpenuhi (harness) | Bagian 5 |
| `AC-339` / `T-S20-02` — isi awal data lama = tanggal WIB, nol riwayat; `POST` kemarin → `422` `VAL-166`, nol pesanan | Terpenuhi (PostgreSQL + harness) | Bagian 5 |
| DoD — migration + Designer + snapshot di working tree, **tidak** diterapkan otomatis | Terpenuhi | `git status` |
| DoD — laporan `BE-LAB-101.md` | Terpenuhi | Berkas ini |
| Bukti HTTP dengan aplikasi berjalan | **Belum** — menunggu langkah rilis `MVP-16e` no. 1 | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Rilis bersama.** Kolom `PlannedDate` wajib: bila migration diterapkan ke sebuah basis data **tanpa** kode ini, setiap pembuatan pesanan Lab gagal. Terapkan migration dan kode `BE-LAB-101` bersama |
| Masalah yang diketahui | `POST /lab-orders` (`CreateAsync`) tetap tanpa `VAL-67` — `LAB-OPEN-056`, di luar task |
| Risiko tersisa | `Down` membuang riwayat jadwal ulang — jalankan hanya sebelum fitur dipakai (`02-backend-architecture.md` 33.7). `UPDATE` pengisian menyentuh setiap baris `LabOrder`; di produksi jalankan di luar jam sibuk Lab |
| Perubahan sampingan | `NONE`. Working tree juga memuat perubahan modul SDM dan berkas uji SDM milik **sesi lain** yang berjalan bersamaan — **tidak disentuh** |
| Interupsi | Build dan pembangkitan migration **ditunda** dua kali karena sesi lain sedang menjalankan `dotnet build`/`dotnet test` atas proyek yang sama (berkas `obj` terkunci) — dilanjutkan sesudah prosesnya selesai, tanpa menghentikannya. Binari PostgreSQL portabel lama tidak utuh (nol DLL) → diunduh ulang zip resmi EDB 15.15 ke folder tersendiri di scratchpad |
| Status Git | Berkas task: ` M` `LabOrderController.cs`, `LabOrderDtos.cs`, `Models/LabOrder.cs`, `LabOrderService.cs`, `ApplicationDbContextModelSnapshot.cs`, `ApplicationDbContext.cs`, `LabOrderConfiguration.cs`; `??` `Models/LabOrderReschedule.cs`, `LabOrderRescheduleConfiguration.cs`, `Migrations/20261125074224_AddLabOrderPlannedDateAndReschedule.cs` dan `.Designer.cs`. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `BE-LAB-102` (aksi jadwal ulang, aturan kelayakan, riwayat, izin, seeder) — kini tidak lagi menunggu. Penerapan migration ke devYoga menunggu instruksi pemilik |
