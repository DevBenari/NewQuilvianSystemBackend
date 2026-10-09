# Laporan Perubahan Backend — `BE-LAB-92`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-92` |
| Judul | Ringkasan hari ini dan pesanan terbaru Beranda |
| Slice | `MVP-13a` — `EPIC-LAB-19` Beranda Lab mengikuti susunan v1 (BR-140) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6as.1 |
| Trace | `LAB-DEC-205`, `LAB-DEC-208`, `LAB-DEC-209`, `LAB-DEC-211`; `LAB-REQ-021` butir 1–6, 9, 10; `FR-19.2`, `FR-19.3`, `FR-19.6`, `FR-19.8`, `FR-19.9`; `02-backend-architecture.md` bagian 28 |
| Contract version | `LAB-API-v1` `r44` (bagian 39.2, 39.3, 39.5–39.7) dan `LAB-PERM-v1` revision 16 (bagian 18.2) — **`approved`** 2026-10-08 oleh Yoga Aji Pratama lewat `LAB-REQ-021`. `LAB-VAL-v1` `r20` tidak dipakai task ini (`VAL-154` milik `BE-LAB-93`) |
| Dependency | Nol task pendahulu |
| Klasifikasi | `MEDIUM` — satu service baru, dua endpoint baca, satu pengangkatan kueri pada service yang dipakai layar lain; nol skema, nol izin baru |
| Task mode | `BACKEND` — wewenang tulis dari pemilik modul (*"lanjut"* 2026-10-08 atas tawaran `/build-module-backend BE-LAB-92`) |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, `Program.cs`, dan dokumen blueprint `laboratorium`. Frontend read-only |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `38c8a4f4` (branch `yoga`, upstream `origin/yoga`) + working tree |
| Tanggal | 2026-10-08 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — harness 34/34, HTTP baca-saja 18/18, build 0 error, validator lolos. **Batas:** `403` belum diamati di runtime (bagian 5) |

---

## 1. Masalah yang diperbaiki

Beranda Lab hari ini hanya punya rekap per status untuk rentang bebas (`GET /lab-orders/summary`). Rekap itu memakai
waktu baris dibuat, tidak mengenal hari WIB, tidak menghitung CITO, tidak tahu berapa hasil menunggu validasi, dan
tidak memuat daftar pesanan. Akibatnya layar tidak dapat menampilkan kartu *hari ini*, *fokus operasional*, maupun
tabel *pesanan terbaru* yang diputuskan pada putaran 25.

**Contoh.** Pukul 09.15 WIB ada 12 pesanan sejak tengah malam. Satu di antaranya diminta pukul 06.30 WIB dan tersimpan
`2026-10-07T23:30Z`. Rekap lama yang membaca tanggal UTC mentah memasukkannya ke tanggal 7 Oktober, sehingga kartu
*hari ini* kehilangan satu pesanan tanpa satu galat pun.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Pelaku | Petugas pemegang `LabOrder : Read` — analis, kepala instalasi, dokter penanggung jawab |
| Pemicu | Membuka menu *Beranda* Laboratorium, atau menekan *Perbarui Data* |
| 1 | Layar meminta `GET /lab-orders/dashboard/today` |
| 2 | Server menentukan *hari ini* sebagai tanggal kalender WIB saat itu, lalu rentang UTC-nya: 2026-10-08 → `2026-10-07T17:00Z` sampai sebelum `2026-10-08T17:00Z` |
| 3 | Pesanan dihitung menurut **waktu diminta** (`RequestedAt`, atau waktu dibuat bagi pesanan lama) |
| 4 | *Menunggu* = belum *Selesai* dan tidak *Dibatalkan*; *Selesai* beserta persentase bulat (setengah ke atas, 0 bila belum ada pesanan) |
| 5 | *CITO* = pesanan yang punya **permintaan** pemeriksaan bertanda cito (dipilih saat memesan) **atau** **pemeriksaan** bertanda cito (*Tandai Cito*). Permintaan yang dibatalkan dan pemeriksaan yang gugur/batal tidak dihitung; pesanan yang dibatalkan tetap dihitung, sejalan dengan *Pesanan hari ini* |
| 6 | *Total pesanan tercatat* = seluruh pesanan sepanjang waktu; *Hasil menunggu validasi* = isi Antrean Validasi tahap *Menunggu Validasi* — **kueri yang sama**, bukan salinan |
| 7 | Layar meminta `GET /lab-orders/dashboard/recent-orders` → 10 pesanan terakhir diminta, seluruh status, beserta nama pasien, No. RM, dan nama pemeriksaannya |
| Jalur tidak normal | Belum masuk → `401`. Tanpa `LabOrder : Read` → `403`. Belum ada pesanan → semua angka 0 dan tabel kosong, tetap `200` |
| Hasil | Angka siap tampil; tidak ada data yang berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
(Lab `ACTIVE`, prefix `Lab`); `rules/backend/TASK_RULES.md`, `API_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`;
`LabOrderController.cs`; `LabOrderService.cs` (`GetSummaryAsync`, jalur pembuatan pesanan); `LabWorklistService.cs`
(`GetValidationQueueAsync`, `ResolveQueueDisciplines`); `LabMonitoringService.cs` (`hasCito`, proyeksi pasien);
`LabExaminationService.cs` (`HasResultStatus`); `LabSpecimenService.cs` (pembuatan pemeriksaan); `LabOrder.cs`,
`LabOrderedProcedure.cs`, `LabExamination.cs`, `LaboratoryEnums.cs`; `LabOrderConfiguration.cs`;
`Helpers/AppDateTimeHelper.cs`; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabDashboardDtos.cs` | Baru | `LabDashboardTodayResponse` (sembilan ruas) dan `LabDashboardRecentOrderResponse` (delapan ruas), persis `r44` 39.3 dan 39.5 |
| `Areas/HealthServices/LaboratoryManagement/Services/LabDashboardService.cs` | Baru | `GetTodayAsync(asOf?)` dan `GetRecentOrdersAsync()`; baca-saja, `AsNoTracking`, tanpa transaksi. Predikat CITO dua sumber (`DenganCito`) dan penentu tanggal WIB (`WibDateOf`) memakai zona `AppDateTimeHelper` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabWorklistService.cs` | Diperbarui | Syarat dasar `GetValidationQueueAsync` **diangkat tanpa perubahan bunyi** ke `ValidationQueueSource(stage, disciplines)`; method publik baru `CountAwaitingValidationAsync()` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Diperbarui | `LabDashboardService` disuntikkan; dua action `GET dashboard/today` dan `GET dashboard/recent-orders` |
| `Program.cs` | Diperbarui | `AddScoped<LabDashboardService>()` |
| Dokumen blueprint | Diperbarui | Laporan ini; kolom *Status* `r44` 39.2; status `BE-LAB-92` di roadmap; bukti di traceability |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baca baru sesuai `r44`; **aditif**. `summary`, antrean validasi, dan endpoint lain tidak berubah |
| Database | `NOT APPLICABLE` — nol tabel, kolom, index, migration. Tidak ada `database update` |
| Keamanan/Auth | `LabOrder : Read` dengan kunci aksi `Read` / `Read Lab Order` yang sudah ada — **nol aksi baru**. `recent-orders` membawa nama dan No. RM (setara Daftar Pasien Lab); hanya dua ruas `MstPatient` yang dibaca. GET tidak dicatat ke log audit |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL `api/v1/health-services/laboratory-management/lab-orders`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/dashboard/today` | Kartu hari ini dan fokus operasional Beranda | `LabOrder : Read` |
| `GET` | `/dashboard/recent-orders` | 10 pesanan terakhir diminta | `LabOrder : Read` |

Contoh jawaban asli `dashboard/today` di DB dev 2026-10-08 15.17 WIB:
`todayOrderCount` 2, `waitingOrderCount` 2, `completedOrderCount` 0, `completionPercent` 0, `citoOrderCount` 0,
`totalRecordedOrderCount` 23, `awaitingValidationCount` 0.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (server lokal BE/FE dimatikan dulu) | Berhasil, 0 error, nol warning dari berkas yang disentuh, 1 menit 51 detik | `PASS` | Log build sesi |
| Harness InMemory (`ApplicationDbContext` asli, nol tulis ke DB bersama) | **34/34** | `PASS` | Rincian di bawah |
| Startup Development + `PermissionRegistryValidator` | `/health` `200`; *"Permission registry valid. 1659 identitas kanonik dari 1659 kemampuan terdaftar"*; nol `fail:`/`crit:` di log | `PASS` | Log server |
| HTTP baca-saja — backend lokal + DB dev, superadmin | **18/18** | `PASS` | Rincian di bawah |
| `403` endpoint baru di runtime | — | `NOT RUN` | Superadmin melewati izin; akun lab yang sandinya tersedia memegang `LabOrder : Read`. Mekanisme atribut sama dengan `GetSummary` |

**Harness 34/34:**

- `AC-295`: 12 pesanan hari ini (termasuk tepat 00.00 WIB, 06.30 WIB, dan satu pesanan lama tanpa `RequestedAt`) →
  12 / Menunggu 8 / Selesai 3 / 25% / CITO 2. Pesanan 23.30 WIB kemarin, 00.00 WIB besok, dan pesanan terhapus
  tidak terhitung. CITO dari permintaan tanpa wadah dan dari *Tandai Cito* terhitung; permintaan cito yang dibatalkan
  dan pemeriksaan cito yang gugur tidak.
- Pembulatan: 1/3 → 33, 2/3 → 67, 1/8 → 13, 4/4 → 100, 0 pesanan → 0. Pesanan *Dibatalkan* ber-CITO ikut dihitung.
- Batas WIB: `2026-12-31T17:30Z` → 2027-01-01; `2026-12-31T16:59:59Z` → 2026-12-31.
- `AC-296`: 7 = 4 PK + 1 PK yang disiplinnya jatuh ke katalog + 2 Mikro; *Sementara*, PA, *Validated*, pesanan
  batal, dan pemeriksaan gugur tidak terhitung; **angka = `totalData` antrean**.
- Regresi antrean: tahap rilis 1, penyaring Mikro 2, urutan `FinalizedAt`, `VAL-139` tahap kosong tetap ditolak.
- `AC-299`: 10 baris, urutan sama dengan rumus kontrak, pesanan terhapus tidak muncul, pesanan *Dibatalkan* muncul,
  nama/No. RM dari kunjungan, `procedureNames` urut dibuat tanpa permintaan batal, jatuhan nama pemeriksaan utama,
  `requestedAt` jatuh ke waktu dibuat, kunjungan tanpa pasien → `null`.

Satu putaran pertama harness gagal di `AC-299` karena data uji tidak memberi pesanan baris kunjungan. InMemory
menerjemahkan navigasi wajib `Encounter` sebagai inner join. Di PostgreSQL FK `EncounterId` wajib dan `Restrict`,
tanpa query filter global, sehingga setiap pesanan selalu punya kunjungan. Pola proyeksinya sama dengan
`LabMonitoringService`. Data uji dibetulkan, kode tidak diubah.

**HTTP baca-saja 18/18:** tanpa masuk → `401` (dua endpoint); `today` `200` dengan tepat sembilan ruas;
`operationalDate` = tanggal WIB saat itu; persentase konsisten; **`awaitingValidationCount` = `totalData`
`GET /lab-worklists/validation-queue?stage=AwaitingValidation`** (0 = 0); `totalRecordedOrderCount` = `totalData`
`GET /lab-orders` (23 = 23); `recent-orders` `200`, 10 baris, tepat delapan ruas, urut menurun, baris teratas
(`LAB-RSMMC-000023`) cocok dengan `GET /{id}`; regresi `summary` `200`. Kueri berjalan di Npgsql tanpa `500`.
Nol permintaan tulis.

Uji manual: `NOT APPLICABLE` — task ini tanpa layar.

**Tidak dijalankan:** analyzer (`-p:RunAnalyzers=False`, sesuai kebiasaan repository untuk build lokal); `403`
runtime (di atas); `AC-295` dengan data CITO asli — DB dev hari ini nol pesanan cito, dan menulis data uji tidak
diberi wewenang. Uji otomatis tidak masuk repository (`LAB-RDY-C04`); harness tinggal di scratchpad sesi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-295` — 12/8/3/25%/2, batas WIB, CITO dua sumber | Terpenuhi (harness) | Bagian 5 |
| `AC-296` — sama dengan `totalData` antrean; *Sementara* dan PA tidak; nol kueri pengguna/SDM di jalur `today` | Terpenuhi (harness + HTTP) | Bagian 5; `LabDashboardService` hanya membaca `LabOrders`, `LabOrderedProcedures`, `LabExaminations` |
| `AC-299` — maks. 10 baris, urutan, termasuk *Dibatalkan*, `procedureNames` dan jatuhannya | Terpenuhi (harness + HTTP) | Bagian 5 |
| `AC-301` — `403` tanpa `LabOrder : Read` | **Sebagian** — atribut `[AccessPermission("LabOrder", "Read")]` terpasang dan registri lolos; `403` runtime belum diamati | Bagian 5 |
| Nol aksi baru di `SysActionAccess` | Terpenuhi — kunci aksi yang sama dengan `GetSummary`; jumlah kunci validator 1659 = 1659 | Log startup |
| Regresi antrean validasi | Terpenuhi (harness) | Bagian 5 |
| Build hijau, validator lolos | Terpenuhi | Bagian 5 |
| Laporan, kolom *Status* `r44` 39.2 diperbarui | Terpenuhi | Berkas ini; `contracts/api-contract.md` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Rumus CITO Beranda sengaja **berbeda** dari `hasCito` daftar pantau, yang hanya membaca pemeriksaan. Cacat as-is *CITO dari pemesanan hilang saat wadah dibuat* (`02-backend-architecture.md` 28.8, usulan `LAB-CONFLICT-019`) **tidak** diperbaiki di sini |
| Masalah yang diketahui | Tidak ada pada scope task |
| Risiko tersisa | Kueri hari ini memindai `LabOrder` tanpa index waktu (`LAB-REQ-021` butir 10). Volume dev 23 pesanan; diukur ulang pada `BE-LAB-93` untuk kueri tahunan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Server lokal BE (`dotnet run --no-build -p:RunAnalyzers=False`) dan FE (`npm run dev`) dimatikan sebelum build, lalu dinyalakan lagi |
| Status Git | Source: `M LabOrderController.cs`, `M LabWorklistService.cs`, `M Program.cs`, `?? LabDashboardDtos.cs`, `?? LabDashboardService.cs`; ditambah dokumen blueprint sesi ini. Tidak di-stage, tidak di-commit |
| Langkah berikutnya | `BE-LAB-93` (ringkasan tahunan, `VAL-154`) — berkas service dan DTO yang sama; atau `FE-LAB-52`, yang kini dapat diverifikasi |
