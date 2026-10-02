# Laporan Perubahan Backend — `BE-RWI-123`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-123` |
| Judul | Pelaksanaan sliding scale (migration K7) |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-123` |
| Trace | `FR-KEP-072` s.d. `FR-KEP-076`; `RWI-DEC-146`, `RWI-DEC-148`, `RWI-DEC-150` (`G-22`), `RWI-DEC-155`; `VAL-KEP-27`, `VAL-KEP-28`, `VAL-KEP-29a`–`f`; `INT-KEP-11`, `INT-DOK-18`; state matrix 5.6; kamus data 11.15; api-contract 7.12 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-119` ✅ 17 September 2026; `BE-RWI-103` [BE-DOK] — order sliding scale sudah ada di source |
| Klasifikasi | `HEAVY` — **task paling berbahaya pada roadmap ini** |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — tujuh kriteria terpetakan; `dotnet build` lolos; migration K7 **diterapkan** ke `QuilvianNewDevHamzah`, skema terverifikasi dari katalog; verifikasi proses bisnis dan uji galat buatan `NOT RUN`; **`RWI-OQ-097` masih terbuka** |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement` |
| Registry / prefix | `Phm` — `ACTIVE`; entity baru `PhmSlidingScaleExecution` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-CODE-006`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001` |
| Hak akses baru | `SlidingScaleExecution : Read`, `: Create`; menyimpan juga memeriksa `MedicationAdministration : Create` lewat `AccessPermissionService` |
| Nomor bisnis | `ExecutionNumber` deret `PHM_SLIDING_SCALE_EXECUTION`, awalan `SSX`, reset harian, 4 digit; dosis baru memakai deret MAR |
| Database | `20260917105000_AddSlidingScaleExecution` (K7, sesudah `R6` `dokter-rawat-inap`); `Down` menolak bila berisi data; **diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |

## 1. Masalah yang diperbaiki

Perawat mengukur gula darah lalu menghitung dosis insulin dari protokol dokter secara manual. Salah membaca rentang, salah
satuan, atau menyimpan dosis tanpa GDS-nya berarti dosis insulin yang salah — dan tidak ada jejak yang bisa diperiksa.

## 2. Proses bisnis

1. Order Budi v2 disesuaikan separuh "pasien sensitif insulin". Pukul 11.00 perawat mengetik GDS 280 mg/dL →
   `POST sliding-scale-executions/preview` → rentang 250–300, `ComputedDoseUnits = 3`, instruksi rentang, `IsHighAlert`.
   Tidak ada yang disimpan.
2. Simpan → `POST sliding-scale-executions` dengan `Idempotency-Key` wajib dan `ExpectedOrderVersionNumber = 2`. Di dalam
   **satu transaksi**: GDS ditulis lewat `DailyMonitoringService.AddGlucoseReading` (atau GDS bangsal terpilih dikunci dan
   diperiksa ulang), satuan dibandingkan tanpa konversi, rentang dicocokkan dari versi order berlaku, dosis MAR
   `SlidingScale` 3 unit dicatat (slot `Due` bila dikirim, jika tidak baris baru), dan `PhmSlidingScaleExecution` ditulis.
3. Insulin high-alert → dosis `Due` + `Pending` cek ganda; pelaksanaan tetap `Recorded`.
4. Tombol tertekan dua kali dengan kunci yang sama → `200` pelaksanaan dan dosis yang sama.
5. GDS 60 mg/dL jatuh pada rentang 0 unit → dosis `Held` "GDS di bawah rentang pemberian" (`G-22`).
6. **Jalur tidak normal:**
   - Order `Stopped`, butir dihentikan, atau tidak ada order aktif → `409` "Tidak ada protokol sliding scale aktif untuk
     pasien ini. Hubungi dokter." (`VAL-KEP-27`).
   - GDS rujukan bukan GDS bangsal aktif episode ini — termasuk id hasil laboratorium → `422` (`VAL-KEP-28`).
   - GDS mmol/L pada protokol mg/dL → `409` "Satuan gula darah (mmol/L) berbeda dari satuan protokol (mg/dL). Periksa
     ulang; sistem tidak mengonversi." (`VAL-KEP-29a`).
   - GDS sudah dipakai pelaksanaan tercatat → `409` "Gula darah ini sudah dipakai pukul 11.02. Ukur ulang…" (`29b`).
   - Dokter menyesuaikan order ke v3 sebelum simpan → `409` "Dokter sudah menyesuaikan protokol. Periksa dosis baru
     sebelum memberi." (`29c`).
   - Dosis aktual ≠ dosis hitung tanpa alasan → `400` (`29d`); tanpa `Idempotency-Key` → `400` (`29e`).
   - Nilai tidak masuk tepat satu rentang → `422`.
   - Galat simpan pada langkah mana pun → transaksi dibatalkan: nol GDS, nol dosis, nol pelaksanaan; jawaban "Pencatatan
     sliding scale gagal disimpan. Belum ada yang tercatat; ulangi." (`RWI-DEC-148` c).
7. Riwayat: `GET /episodes/{episodeId}` menampilkan jam, GDS, rentang, dosis hitung dan aktual, pelaksana, status cek
   ganda, serta tanda GDS yang dikoreksi sesudahnya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PhmSlidingScaleOrder*.cs`, `PhmSlidingScaleRange.cs`, `PhmSlidingScaleTemplateVersion.cs`, `SlidingScaleOrderService.cs`,
`SlidingScaleRangeValidator.cs`, `AccessPermissionService.cs`, integration contract 8.5, `dokter-rawat-inap` integration 12.8,
validation matrix 6.4, state matrix 5.6, api-contract 7.12.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Enums/SlidingScaleExecutionStatus.cs`, `Models/PhmSlidingScaleExecution.cs` | Baru |
| `Repositories/Configurations/HealthServices/PharmacyManagement/SlidingScaleExecutionConfiguration.cs` | Baru — unique kunci, unique dosis, unique GDS tercatat parsial, check constraint |
| `Repositories/ApplicationDbContext.cs`, `Migrations/20260917105000_AddSlidingScaleExecution.cs`, snapshot | K7 |
| `Areas/HealthServices/PharmacyManagement/DTOs/SlidingScaleExecutionDtos.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Services/SlidingScaleExecutionService.cs` | Baru — pratinjau, pelaksanaan satu transaksi, riwayat, detail |
| `Areas/HealthServices/PharmacyManagement/Controllers/SlidingScaleExecutionController.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.cs` | `AllocateNumberAsync` menjadi `internal` untuk nomor dosis sliding scale |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.Glucose.cs` | `AddGlucoseReading` tanpa simpan; koreksi GDS menandai pelaksanaan; pembatalan GDS terpakai ditolak |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Recording.cs` | Koreksi dosis sliding scale dibatasi `Administered`/`Cancelled` (`VAL-KEP-33b`); `Cancelled` ikut membatalkan pelaksanaan |
| `Program.cs` | Registrasi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru sesuai 7.12. Delta: `AdministeredAt` kosong dibaca "sekarang" dan `ActualRoute` kosong dibaca rute butir resep; dosis aktual 0 pada rentang bukan 0 wajib beralasan dan dicatat `Held` |
| Database | Satu tabel baru `PharmacyManagement` |
| Keamanan/Auth | Kewenangan ganda dicek service; `ExceptionReason` tidak masuk payload logger; nol pembacaan hasil laboratorium |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Sliding Scale Execution — `sliding-scale-executions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/preview` | Rentang dan dosis hitung tanpa menyimpan | `SlidingScaleExecution : Read` |
| `POST` | `/` | GDS, dosis MAR, pelaksanaan dalam satu transaksi | `SlidingScaleExecution : Create` + `MedicationAdministration : Create` |
| `GET` | `/episodes/{episodeId}` | Riwayat pelaksanaan (bawaan 7 hari) | `SlidingScaleExecution : Read` |
| `GET` | `/{id}` | Detail beserta dosis MAR | `SlidingScaleExecution : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran kriteria 1–7 | Terpetakan | `PASS` | Bagian 6 |
| Review urutan transaksi | Validasi baca → alokasi nomor → `BeginTransaction` → GDS/dosis/pelaksanaan → satu `SaveChanges` → `Commit`; `DbUpdateException` → `Rollback` | `PASS` | `ExecuteAsync` |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi skema | Dibaca dari katalog `QuilvianNewDevHamzah` setelah `database update` | `PASS` | Lampiran |
| Verifikasi proses bisnis ketujuh kriteria | Tidak dijalankan | `NOT RUN` | Build dan migration lolos; belum ada order aktif (`RWI-OQ-097`) |
| Uji galat buatan pada setiap langkah transaksi | Tidak dijalankan | `NOT RUN` | Docker tidak aktif |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ditolak tanpa order aktif | Terpenuhi | `LoadActiveOrderAsync` → `409` |
| 2. Hanya GDS bangsal bersatuan sama | Terpenuhi | `LoadWardReadingAsync` (`422`), `Compute` (`409`) |
| 3. Satuan berbeda ditolak tanpa konversi | Terpenuhi | Nol kode konversi satuan |
| 4. Satu transaksi; dosis tampil sekali | Terpenuhi | Satu `SaveChanges` dalam transaksi; unique `UX_PhmSlidingScaleExecution_MedicationAdministration` |
| 5. Idempoten | Terpenuhi | Kunci wajib + unique + pengembalian pemenang pada tabrakan |
| 6. Pratinjau sebelum simpan; pengecualian beralasan | Terpenuhi | `PreviewAsync`; `VAL-KEP-29d` |
| 7. Rentang 0 unit → `Held` beralasan | Terpenuhi | `AlasanRentangNol` |
| DoD: `dotnet build`, verifikasi skema dan proses bisnis, uji galat buatan | `dotnet build` lolos; skema terverifikasi dari katalog; **verifikasi proses bisnis, uji galat buatan dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Gerbang produksi | **`RWI-OQ-097` masih terbuka** — nama pengesah isi protokol belum ada. Pemakaian disetujui `RWI-DEC-155`, tetapi selama nama itu kosong tidak ada versi template yang dapat disahkan, tidak ada order aktif, dan setiap pelaksanaan pada pasien sungguhan ditolak `VAL-KEP-27` |
| Peringatan | Notifikasi ke dokter untuk rentang berinstruksi tidak dikirim sistem (gate `G-24`); instruksi hanya ditampilkan |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Pemilik klinis task ini belum ditunjuk (tabel task roadmap); verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` `ApplicationDbContext.cs`, snapshot, `Program.cs` |
| Langkah berikutnya | Penunjukan pengesah `RWI-OQ-097`; frontend pelaksanaan sliding scale |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |

Migration task ini: `20260917105000_AddSlidingScaleExecution` (K7) — diterapkan ke `QuilvianNewDevHamzah` 17 September 2026.

