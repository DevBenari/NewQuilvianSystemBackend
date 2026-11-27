# Laporan Perubahan Backend — `BE-LAB-103`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-103` |
| Judul | Penanda kelayakan, *Terlewat*, daftar pantau, dan kiosk |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16c` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6av.3 |
| Trace | `FR-21.7`, `FR-21.8`, `FR-21.10`, `FR-21.11`; `LAB-DEC-258`, `259`, `260`, `266`, `273`, `277`; `02-backend-architecture.md` 33.4, 33.10 no. 6; `INV-67`, `INV-68`; `LAB-DC-068`, `LAB-DC-069` |
| Contract version | `LAB-API-v1` **`r50`** 45.3, 45.5, 45.6 — **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Dependency | `BE-LAB-102` — ✅ selesai 2026-11-26 (working tree) |
| Klasifikasi | `HEAVY` — repository 0, berkas diperiksa 2 (±20), berkas diubah 2 (8), logika 2 (penyaring gabungan + satu rumus di tiga pembaca), kontrak API 2 (ruas dan nilai enum baru), database 1 (kueri baca saja), keamanan 1 (kiosk tanpa alasan), UI 0 = **10** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Lab; laporan, roadmap, dan traceability modul. **Tanpa** migration, **tanpa** menulis basis data bersama, **tanpa** commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `cce1dbf5` (branch `yoga`), di atas working tree `BE-LAB-101`/`102` |
| Tanggal | 2026-11-26 |
| Status | ✅ **Selesai** — build lulus; harness SQLite **126/126**; karakterisasi **23/23 identik**; regresi `BE-LAB-102` **144/144**; kontrol negatif **5/5 mutan tertangkap**. HTTP pada aplikasi berjalan **belum** — menunggu migration `BE-LAB-101` di devYoga |

### Gerbang masukan

| Butir | Hasil |
| --- | --- |
| Status task | `SIAP DIKERJAKAN` — `BE-LAB-102` selesai |
| Kontrak | `r50` `approved`; nol perubahan kontrak |
| Hash masukan | 13 berkas tabel *Hash masukan* traceability `MVP-16` — **13/13 sama** |
| Source | HEAD tetap `cce1dbf5`; satu-satunya perubahan Lab di atasnya adalah `BE-LAB-101`/`102` sendiri |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Governance terbaca | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill |
| Area / Module | `HealthServices` / `LaboratoryManagement` — prefix `Lab` **`ACTIVE`** |
| Keberlakuan | `LabRescheduleSnapshotReader`, `OverdueDays`, `OverdueExpression`, `TerapkanTanggalRencana` = **`NEW CODE`**; `LabMonitoringService`, `LabOrderService`, `LabMonitoringDtos`, `LabOrderDtos`, `LabDateCategory` = **`TOUCHED LEGACY`** |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; controller tidak berubah), `QBE-API-001` (`PagedResult`/`ApiResponse` yang ada), `QBE-DTO-001` (ruas DTO, bukan entity), `QBE-ENUM-001` (nilai enum milik modul), `QBE-PAGE-001` (paging daftar pantau tetap), `QBE-ENT-003` (nol kolom presentasi — *Terlewat*/kelayakan dihitung saat dibaca) |
| Tidak berlaku | `QBE-ENT-001/002`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-TXN-001`, `QBE-PERM-001` — nol entity, konfigurasi, nomor, tulis, atau izin baru |
| Pengecualian | Nol |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-102` petugas sudah bisa memindahkan tanggal rencana, tetapi layar **tidak tahu** pesanan mana yang boleh
dipindah, mana yang tanggalnya sudah lewat, dan Daftar Pasien Lab tidak dapat menyaring menurut tanggal rencana. Akibatnya
tombol *Jadwal Ulang* hanya bisa ditebak, lalu ditolak `409`; pesanan yang pasiennya tidak datang Senin hilang dari daftar
Selasa.

*Contoh.* Rabu 4 Desember, Gilang membuka daftar kerja hari ini. Sebelum task ini daftar hanya menyaring menurut waktu
pesanan dibuat. Sesudahnya daftar memuat pesanan bertanggal rencana 4 Desember **ditambah** Profil Lipid bertanggal
2 Desember yang sampelnya belum diambil — bertanda *Terlewat 2 hari* — dan setiap baris membawa `canReschedule` beserta
alasannya bila tidak layak.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Layar dapat menampilkan tanggal rencana, *Terlewat*, dan kelayakan jadwal ulang tanpa menebak; daftar kerja harian memuat rencana hari ini dan yang terlewat |
| Pelaku | Pembaca detail pesanan (`LabOrder : Read`), daftar pantau (`LabMonitoring : Read`), kiosk (`KioskReadPolicy`) — izin **tidak berubah** |
| Pemicu | `GET /lab-orders/{id}`, `GET /lab-monitoring/{disiplin}`, `GET /lab-orders/kiosk/pending-by-patient/{patientId}` |

**Aturan penanda — satu rumus.** Ketiga pembaca menyusun potret lewat `LabRescheduleSnapshotReader` (yang juga dipakai
penjaga `PUT reschedule` sejak task ini), lalu menilainya dengan `LabRescheduleEligibility`:

| Ruas | Aturan | Contoh (hari ini Rabu) |
| --- | --- | --- |
| `canReschedule` + kode + alasan | `Evaluate` — urutan MCU → status → wadah → kunjungan → CITO; alasan sama persis dengan pesan `409` | Kolesterol wadah `Collected` → `false`, `SampleTaken`, *"Sampel sudah diambil; …"* |
| `isOverdue` | Tanggal rencana < hari ini **dan** status masih sebelum sampel **dan** wadah efektif masih `Planned` — MCU, CITO, dan kunjungan **tidak** ikut (pesanan MCU/CITO yang tidak datang tetap perlu terlihat) | Rencana Senin, belum ada wadah diambil → `true`; HbA1c MCU rencana Senin → tetap `true` meskipun `canReschedule=false` |
| `overdueDays` | Hari sejak tanggal rencana bila terlewat, selain itu `0` | Senin → Rabu = `2` |
| `rescheduleCount` (detail) | Jumlah baris riwayat jadwal ulang — dihitung, tidak disimpan | Dua kali dipindah → `2` |
| `isEarly` (kiosk) | Tanggal rencana > hari ini WIB | Rencana Jumat, pasien datang Rabu → `true`; kiosk **tidak menolak** |

**Daftar pantau `dateCategory=PlannedDate`.**

1. `startDate`/`endDate` dibandingkan dengan **bagian tanggal**-nya terhadap tanggal rencana, keduanya inklusif
   (`endDate=2026-12-04T23:59` = 4 Desember).
2. `includeOverdue=true` **dan** ada `startDate` → hasil ditambah pesanan *Terlewat* bertanggal rencana sebelum `startDate`.
   Syaratnya diambil dari `LabRescheduleEligibility.OverdueExpression` — bentuk kueri dari rumus `IsOverdue` di berkas yang
   sama, dibuktikan setara pada 19 kasus.
3. `includeOverdue` tanpa `startDate`, atau pada kategori lain → **diabaikan**, tidak ditolak.
4. `dateCategory` kosong → **tetap `OrderDate`** (`LAB-DEC-277`): menu Hasil dan Riwayat tidak berubah — dibuktikan
   karakterisasi byte-identik.

| Contoh (hari ini 26 Nov) | Hasil |
| --- | --- |
| `PlannedDate`, 26–26, `includeOverdue` | Rencana 26 + terlewat 24 (2 hari) + terlewat 25 (1 hari); pesanan `Accepted` bertanggal 24 **tidak** |
| `PlannedDate`, 26–26 | Rencana 26 saja |
| `PlannedDate`, 27–27, `includeOverdue` | Rencana 27 + terlewat 24, 25; rencana 26 **bukan** terlewat |
| Tanpa `dateCategory`, periode jam ini | Seluruh pesanan yang **diminta** jam ini, berapa pun tanggal rencananya |

**Tanggal rencana bukan penghalang** (`LAB-DEC-259`): wadah boleh direncanakan dan diambil sebelum tanggal rencana; tanggal
rencana tetap, pesanannya kini `SampleTaken`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap 6av.3, traceability `MVP-16`, `LAB-API-v1` 45.3/45.5/45.6, `02-backend-architecture.md` 33.4 dan 33.10, uji
`T-S20-12`/`13`/`16`/`17`; `LabMonitoringService.cs` (seluruhnya), `LabMonitoringDtos.cs`, `LabDateCategory.cs`,
`LabMonitoringController.cs`; `LabOrderService.cs` (`GetDetailAsync`, `GetKioskPendingByPatientAsync`, `RescheduleAsync`,
`MapDetailResponse` dan pemakainya); `LabOrderDtos.cs`; `LabSpecimenService` (`PlanAsync`, `CollectAsync`);
`LabOrderResultProgressRules` (pola baca per halaman); model `LabSpecimen`, `MstPatient`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabRescheduleSnapshotReader.cs` | **Baru** — `internal static ReadAsync(db, orderIds)`: potret kelayakan sekumpulan pesanan dengan **dua kueri** (proyeksi pesanan + daftar wadah berkelompok), pola `LabOrderResultProgressRules.ReadAsync` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabRescheduleEligibility.cs` | `OverdueDays`; `OverdueExpression(today)` — bentuk kueri `IsOverdue` untuk penyaring, di berkas yang sama dengan rumusnya |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Detail: `RescheduleCount` dalam proyeksi + penanda dari potret; kiosk: `PlannedDate`, `IsEarly`, `IsOverdue` (satu baca potret per panggilan); `ReadRescheduleSnapshotAsync` (`BE-LAB-102`) kini meneruskan ke pembaca bersama — empat kueri per-pesanan miliknya dihapus |
| `Areas/HealthServices/LaboratoryManagement/Services/LabMonitoringService.cs` | `PlannedDate` dalam proyeksi; penanda per halaman dari pembaca bersama; cabang `PlannedDate` + `includeOverdue` (`TerapkanTanggalRencana`, penggabung ekspresi) |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabMonitoringDtos.cs` | `LabMonitoringQuery.IncludeOverdue`; enam ruas baru `LabMonitoringItemResponse` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | Enam ruas baru `LabOrderDetailResponse` (selain `PlannedDate` dari `BE-LAB-101`); tiga ruas baru `LabKioskPendingOrderItemResponse` |
| `Areas/HealthServices/LaboratoryManagement/Enums/LabDateCategory.cs` | `PlannedDate = 4` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** sesuai `r50`: ruas respons baru (detail, daftar pantau, kiosk), nilai enum `PlannedDate = 4`, parameter `includeOverdue` (bawaan `false`). Pemanggil lama tidak berubah — karakterisasi 23/23 identik |
| Database | **Nol migration, nol tulis.** Daftar pantau kini menjalankan **7 perintah SQL per halaman, tetap** berapa pun jumlah baris (sebelumnya 5; +2 untuk potret). Detail +2 kueri, kiosk +2 kueri |
| Keamanan/Auth | Izin tidak berubah. Kiosk menerima tanggal dan dua penanda saja — **tanpa** alasan, kode alasan, maupun riwayat (`LAB-DEC-266`). `canReschedule` bukan izin |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders` — **endpoint yang sudah ada, diperluas**.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}` | Rincian pesanan; kini memuat `rescheduleCount`, `isOverdue`, `overdueDays`, `canReschedule`, `rescheduleBlockReasonCode`, `rescheduleBlockReason` | `LabOrder : Read` |
| `GET` | `/kiosk/pending-by-patient/{patientId}` | Pesanan aktif pasien di kiosk; setiap item kini memuat `plannedDate`, `isEarly`, `isOverdue` | `KioskReadPolicy` |

#### Health Services / Laboratory Management / Lab Monitoring

Base URL: `api/v1/health-services/laboratory-management/lab-monitoring` — **endpoint yang sudah ada, diperluas**.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/clinical-pathology`, `/anatomic-pathology`, `/microbiology` | Daftar pantau; query `dateCategory=PlannedDate` dan `includeOverdue`; baris memuat `plannedDate`, `isOverdue`, `overdueDays`, `canReschedule`, kode dan alasan | Tidak berubah |

*Contoh:* `GET /lab-monitoring/clinical-pathology?dateCategory=PlannedDate&startDate=2026-12-04&endDate=2026-12-04&includeOverdue=true`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | `Build succeeded`, **0 error**, nol peringatan pada berkas yang disentuh | `PASS` | Keluaran build |
| **Karakterisasi** — `be103char` (data deterministik: 9 pesanan, dua disiplin, semua status penting): 11 kueri daftar pantau PK (bawaan, periode tanpa kategori, `OrderDate`, `SamplingDate`, `ExaminationDate`, `OnlyCito`, status, halaman 2, pencarian, status wadah, hanya awal) + 1 MB, 9 detail, 2 kiosk — dijalankan **sebelum** perubahan (dua kali, identik) dan **sesudah**; ruas baru `r50` dibuang | **23/23 kunci identik**; ruas yang bertambah persis delapan ruas `r50` | `PASS` | `char-sebelum.json` vs `char-sesudah.json` |
| `T-S20-12` — 20 kasus `T-S20-04`..`08` (status, wadah efektif, kunjungan, CITO permintaan/pemeriksaan/gugur, MCU paket/di luar paket): `canReschedule`, kode, alasan pada **detail dan baris** dibandingkan dengan hasil `PUT` nyata | 40/40 — selalu sama; alasan = pesan `409` | `PASS` | Harness `be103harness` |
| *Terlewat* — 19 kasus tanggal lampau/hari ini/besok (termasuk `OnHold` tanpa status sebelum, wadah terhapus, MCU, CITO, kunjungan selesai): detail `isOverdue`/`overdueDays`, baris = detail, **penyaring SQL `includeOverdue` = rumus** | 57/57 | `PASS` | Harness |
| `T-S20-13` — `PlannedDate` hari ini + `includeOverdue` (rencana hari ini + terlewat, `overdueDays` 2 dan 1); tanpa `includeOverdue`; `endDate` 23.59 inklusif; periode lampau; periode besok + `includeOverdue`; hanya `endDate`; tanpa periode; tanpa `dateCategory` = `OrderDate`; `includeOverdue` diabaikan pada bawaan dan `OrderDate`; ditambah satu pemeriksaan bentuk enam ruas baris | 10/10 + 1 | `PASS` | Harness |
| N+1 — perintah SQL halaman 2 baris = 5 baris = 5 baris + `includeOverdue` | 7 = 7 = 7 | `PASS` | `DbCommandInterceptor` harness |
| Detail — `rescheduleCount` 0 → 2; jawaban `PUT` membawa `rescheduleCount` 3 dan penanda terbaru; bentuk tujuh ruas `r50` 45.3 | Sesuai | `PASS` | Harness |
| `T-S20-16` — kiosk: ke depan `isEarly`; lampau sebelum sampel `isOverdue`; hari ini keduanya `false`; lampau sampel diterima bukan terlewat; keempatnya tetap tampil; JSON item **persis delapan ruas** (tanpa alasan/riwayat); **nol** perintah tulis | Sesuai | `PASS` | Harness |
| `T-S20-17` — rencana tiga hari lagi: `PlanAsync` lalu `CollectAsync` hari ini berhasil; tanggal rencana tetap; detail kini `SampleTaken`, bukan terlewat | Sesuai | `PASS` | Harness — **ringkasan 126 lulus, 0 gagal** |
| Regresi `BE-LAB-102` — penjaga `PUT` kini lewat pembaca bersama | **144/144** | `PASS` | `be102harness` |
| **Kontrol negatif** — lima mutan: (a) penyaring SQL menyimpang dari rumus, (b) bawaan `DateCategory` menjadi `PlannedDate`, (c) potret dibaca per baris, (d) *Terlewat* ikut menyaring MCU/CITO/kunjungan, (e) kiosk `isEarly` memasukkan hari ini | **5/5 tertangkap**: (a) `Terlewat [RecollectionRequired + Planned]` SQL ≠ rumus; (b) dua uji `T-S20-13` bawaan `OrderDate`; (c) dua uji N+1 (11 vs 17 perintah); (d) empat uji *Terlewat* MCU/CITO/kunjungan; (e) kiosk hari ini. Sesudah pemulihan: hash ketiga berkas sama, build ulang, harness **126/126**, regresi **144/144**, karakterisasi **23/23** | `PASS` | `mut103-hasil.log` |
| HTTP pada aplikasi berjalan | — | `NOT RUN` | Lihat di bawah |

**Tidak dijalankan:**

- **HTTP pada aplikasi berjalan** (`NOT RUN`): devYoga belum memiliki kolom `LabOrder.PlannedDate` (migration `BE-LAB-101`
  menunggu instruksi pemilik — langkah rilis `MVP-16e` no. 1); setiap kueri pesanan Lab akan gagal sebelum itu.
- **PostgreSQL sungguhan**: kueri baru hanya perbandingan `date`, enum, dan `EXISTS` — tidak ada fungsi khas PostgreSQL.
  Semantik `NULL` pada `StatusBeforeHold` (wadah `OnHold` tanpa status sebelum) diuji di SQLite dan setara dengan rumus.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-353` / `T-S20-12` — penanda detail dan baris = hasil `PUT`, satu rumus | Terpenuhi (harness) | Bagian 5 |
| `AC-344`, `AC-346`, `AC-356` / `T-S20-13` — daftar `PlannedDate` + `includeOverdue`, `overdueDays` benar, bawaan tetap `OrderDate` | Terpenuhi (harness + karakterisasi) | Bagian 5 |
| `AC-351` / `T-S20-16` — kiosk tanggal, `isEarly`, `isOverdue`, tanpa alasan, nol tulis | Terpenuhi (harness) | Bagian 5 |
| `AC-345` / `T-S20-17` — tanggal rencana bukan penghalang | Terpenuhi (harness) | Bagian 5 |
| Karakterisasi tanpa `dateCategory` byte-identik | Terpenuhi | 23/23 |
| Jumlah kueri per halaman tidak bertambah per baris | Terpenuhi | 7 = 7 = 7 |
| DoD — nol migration | Terpenuhi | `git status` |
| DoD — laporan `BE-LAB-103.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap rancangan (dicatat, kontrak tidak diubah) | (1) **Berkas baru `LabRescheduleSnapshotReader.cs`** di luar daftar cakupan: satu penyusun potret bagi `PUT`, detail, daftar, dan kiosk — lebih kuat daripada menyusun potret di tiap service (`INV-67`). Akibatnya metode potret `BE-LAB-102` diganti penerusan; regresi 144/144. (2) **`OverdueExpression`** — bentuk kueri `IsOverdue` untuk penyaring; rancangan tidak menyebut bentuknya. Kesetaraannya dengan rumus dibuktikan 19 kasus dan mutan (a). (3) **Jawaban `POST /` dan `POST /by-examinations`** tidak diisi penanda baru (bawaan `false`/`0`) — `r50` 45.3 memperluas `GET /{id}`; jawaban perpindahan status dan `PUT reschedule` yang memakai detail ikut terisi. (4) `includeOverdue` tanpa `startDate` diabaikan (tidak ada "sebelum periode") |
| Catatan bagi `FE-LAB-57` | Untuk `dateCategory=PlannedDate`, kirim `startDate`/`endDate` sebagai **tanggal** (`yyyy-MM-dd`), bukan waktu ISO berzona: backend memakai bagian tanggal apa adanya, sehingga `2026-12-03T17:00:00Z` (= 4 Desember 00.00 WIB) terbaca 3 Desember |
| Masalah yang diketahui | Tidak ada yang baru |
| Risiko tersisa | Uji HTTP akun asli dan data uji nyata (`MVP-16e` no. 4–5) menunggu langkah rilis |
| Perubahan sampingan | `NONE`. Perubahan SDM di working tree milik sesi lain — tidak disentuh |
| Interupsi | Kontrol negatif ditunda sampai uji mutasi HR sesi lain (`mut468.cjs`) selesai, supaya kedua build tidak berebut `obj` — ditunggu, tidak dihentikan |
| Status Git | Berkas task: ` M` `LabMonitoringService.cs`, `LabOrderService.cs`, `LabMonitoringDtos.cs`, `LabOrderDtos.cs`, `LabDateCategory.cs`; `??` `LabRescheduleSnapshotReader.cs`, laporan ini; `LabRescheduleEligibility.cs` (baru sejak `BE-LAB-102`) diperluas; ` M` roadmap dan traceability. Tidak ada stage, commit, atau push |
| Langkah berikutnya | Backend `MVP-16` selesai pada kode. Berikutnya frontend `FE-LAB-57`..`59` (`MVP-16d`), dan langkah rilis `MVP-16e` no. 1–3 atas instruksi pemilik |
