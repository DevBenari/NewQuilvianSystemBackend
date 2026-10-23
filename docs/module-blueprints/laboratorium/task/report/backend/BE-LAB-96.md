# Laporan Perubahan Backend — `BE-LAB-96`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-96` |
| Judul | Definisi hitung Beranda dipindah ke satu tempat bersama |
| Slice | `S16b-2` Statistik Pesanan — fondasi `INV-59`; gelombang `MVP-15a` |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 6au.1 |
| Trace | `FR-20.6`; `LAB-DEC-242`, `LAB-DEC-246`; `INV-59`; `ARCH-GAP-LAB-15`, `ARCH-GAP-LAB-16`; `02-backend-architecture.md` bagian 32.4 (`LabOrderDemandQuery`) |
| Contract version | `LAB-API-v1` `r49` 44.5 — **`approved` 2026-10-20** (Yoga Aji Pratama, *"Setuju keenam butir"*). Baris `GET /lab-orders/dashboard/yearly`: *nol perubahan perilaku* |
| Dependency | Nol |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1 (±12), berkas diubah 0 (3), logika bisnis 1 (pemindahan kueri hitung), kontrak API 1 (memakai kontrak yang ada), database 1 (perilaku kueri yang ada), keamanan 0, UI 0 |
| Task mode | `BACKEND` — backend target tulis, frontend referensi read-only |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/Services/`, `Program.cs`, laporan ini, dan baris status roadmap/traceability `MVP-15` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) |
| Tanggal | 2026-10-20 |
| Status | ✅ **Selesai** — uji karakterisasi identik, build lulus, aplikasi menyala, Beranda terbaca dengan akun superadmin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — Laboratory, **`ACTIVE`** (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 29, 57, 117) |
| Keberlakuan | `NEW CODE` (`LabOrderDemandQuery.cs`); `TOUCHED LEGACY` (`LabDashboardService.cs`, `Program.cs`) |
| QBE yang berlaku | `QBE-MOD-001` (capability di modul pemilik), `QBE-SVC-001` (service memiliki kueri; controller tidak berubah), `QBE-NAM-001` (nol `Trx*`), `QBE-API-001` (nol perubahan API) |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-MOD-002/003` — nol entity, nol configuration; `QBE-CODE-*` — nol nomor bisnis; `QBE-DB-*` — nol migration |
| Branch | `yoga` — branch pemegang modul Laboratorium, sama dengan seluruh task `BE-LAB-*` sebelumnya |

---

## 1. Masalah yang diperbaiki

Beranda Laboratorium menghitung **berapa pesanan dan pemeriksaan yang diminta** dengan aturan yang tertulis langsung di
dalam `LabDashboardService.GetYearlyAsync`. Laporan *Statistik Pesanan* yang akan dibangun (`BE-LAB-98`) dijanjikan
menampilkan angka **yang sama** dengan Beranda (`LAB-DEC-242`, `AC-334`). Bila aturan itu disalin ke laporan, dua salinan
akan bercabang diam-diam suatu hari — persis yang terjadi ketika fakta F30-20 ditulis dari potongan kode dan lupa bahwa
Beranda tidak menghitung pemeriksaan gugur.

**Contoh akibatnya bila disalin:** seseorang kelak menambahkan *"order Draft tidak dihitung"* di laporan saja. Beranda
menulis 156 pemeriksaan Patologi Klinik bulan Oktober, laporan menulis 154 — dua layar resmi, dua angka, tanpa galat apa pun.

## 2. Proses bisnis

Tidak ada proses bisnis yang berubah. Kepala instalasi dan petugas tetap membuka **Beranda Lab** dan memilih tahun; grafik
pemeriksaan per disiplin, sebaran, *Jenis laboratorium*, dan tren pesanan bulanan dihitung dengan aturan yang sama
seperti sebelumnya:

1. **Pesanan** dihitung menurut **waktu diminta** (cadangan: waktu dibuat) dalam WIB; pesanan **dibatalkan** dan **Draft**
   tidak dihitung.
2. **Pemeriksaan** dihitung bila tidak **gugur** dan tidak **batal**, pada pesanan yang tidak dibatalkan; ditambah
   **permintaan** yang belum masuk wadah.
3. **Disiplin** dibaca dari pesanan, lalu dari katalog; kosong → *belum terklasifikasi*.

Yang berubah hanya **letak** aturan itu: kini satu class bersama yang akan dipakai juga oleh Statistik Pesanan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`;
`Areas/HealthServices/LaboratoryManagement/Services/LabDashboardService.cs` (utuh); `Program.cs` (registrasi Lab);
`Models/LabExamination.cs`, `Models/LabOrderedProcedure.cs`, `Models/IdentityModel.cs`; harness `be94harness` sesi lalu
(pola InMemory); `backend-roadmap.md` 6au.1; `02-backend-architecture.md` 32.4.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderDemandQuery.cs` | **Baru.** `CountedOrders`, `CountedExaminations`, `PendingOrderedProcedures` (pembentuk kueri atas rentang UTC) dan dua ekspresi disiplin `ExaminationDiscipline`, `OrderedProcedureDiscipline`. Isinya **dipindah apa adanya** dari `GetYearlyAsync`, termasuk penyaringan `Draft` yang tidak simetris (`ARCH-GAP-LAB-15`) |
| `Areas/HealthServices/LaboratoryManagement/Services/LabDashboardService.cs` | Konstruktor menerima `LabOrderDemandQuery`; ketiga kueri `GetYearlyAsync` memanggilnya; salinan kueri lama **dihapus** (−36 baris). `GetTodayAsync`, `GetRecentOrdersAsync`, `DenganCito` tidak disentuh |
| `Program.cs` | `builder.Services.AddScoped<LabOrderDemandQuery>();` tepat sebelum `LabDashboardService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — `GET /lab-orders/dashboard/yearly` tidak berubah (route, izin `LabOrder : Read`, bentuk respons, `VAL-154`) |
| Database | Nol tabel, nol kolom, nol index, nol migration. Kueri SQL yang dibentuk sama — predikat dan pengelompokan dipindah tanpa diubah |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah. Endpoint yang perilakunya dijaga:

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/dashboard/yearly` | Grafik pemeriksaan per disiplin, sebaran, dan tren pesanan bulanan satu tahun | `LabOrder : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji karakterisasi **sebelum** perubahan — harness InMemory `be96harness`, 13 pesanan: status Requested/Accepted/Cancelled/Draft/Completed/Confirmed/InProcess/OnHold, satu pesanan dihapus, pemeriksaan Ordered/ChargeEligible/Voided/Cancelled/dihapus, permintaan Ordered/Fulfilled/Cancelled/dihapus, disiplin pesanan kosong (jatuh ke katalog), prosedur tanpa disiplin, `RequestedAt` kosong (cadangan `CreateDateTime`), batas tahun WIB 1 Januari 00.30 dan 31 Desember 23.30/23.59; tahun 2026, 2025, kosong, 1999, 2027 | Acuan direkam; angka dicocokkan manual dengan data uji (mis. PK 2026 = 6, MB 2026 = 6 termasuk pesanan `Draft`, belum terklasifikasi = 2, tren Jan/Mar/Mei/Jun/Des = 1/2/1/1/1) | `PASS` | `scratchpad/be96harness/acuan-sebelum.json` |
| Uji karakterisasi **sesudah** perubahan — berkas harness **sama** (sha256 `91c85944…`), `LabOrderDemandQuery` terdeteksi lewat refleksi | **Identik byte demi byte** dengan acuan | `PASS` | `cmp` → *byte-identik*; keluaran *"IDENTIK dengan acuan"*, exit 0 |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (server BE/FE dimatikan lebih dulu) | `Build succeeded`, 0 error, 1 menit 44 detik | `PASS` | Keluaran build |
| Aplikasi menyala, `ASPNETCORE_ENVIRONMENT=Development` (validasi DI saat startup) | `https://localhost:7184/health` → `200` | `PASS` | Log `scratchpad/be.log` |
| Login superadmin lewat layar FE `http://localhost:3000` (Playwright, tulis diblokir) | Berhasil → `/` | `PASS` | `scratchpad/be96-layar.mjs` |
| `GET /dashboard/yearly`, tanpa tahun dan `?year=2026` pada devYoga | `200` — PK 10, PA 5, Mikro 5, belum terklasifikasi 0; tren September 17, Oktober 6 = **23**, cocok dengan 23 pesanan aktif yang terbaca langsung dari basis data pada capability map rev 9 | `PASS` | Keluaran skrip |
| `?year=1999` | `422` *"Tahun tidak sah. Pilih tahun 2000 sampai tahun berjalan."* | `PASS` | Keluaran skrip |
| Beranda Lab di peramban | Terbuka utuh; grafik 10/5/5 (total 20), tren 2026 23 pesanan; nol galat API; nol tulis | `PASS` | `scratchpad/shots/be96-beranda-lab.png` |

Uji manual: `PASS` (Beranda Lab, superadmin).

**Tidak dijalankan:** `PermissionRegistryValidator` tersendiri — `NOT RUN`, sebab nol atribut izin berubah dan aplikasi
menyala normal. Uji pada basis data yang **menulis** — `NOT RUN`, sesuai larangan tulis ke basis data bersama.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `T-S16B-13` — `GET /lab-orders/dashboard/yearly` identik sebelum dan sesudah pada data uji yang sama (disiplin, `UnclassifiedExaminationCount`, dua belas bulan dua tahun) | **Terpenuhi** | Harness byte-identik |
| Uji karakterisasi ditulis dan lulus **sebelum** pemindahan, lalu lulus **tanpa disunting** sesudahnya | **Terpenuhi** | Satu berkas `Program.cs` harness, sha256 sama di kedua putaran |
| Satu class bersama; **nol salinan** kueri lama | **Terpenuhi** | Diff `LabDashboardService.cs`: tiga kueri diganti panggilan, −36 baris |
| `Draft` dipindah apa adanya (tidak dirapikan) | **Terpenuhi** | Harness: pesanan `Draft` tetap terhitung di pemeriksaan, tidak di tren |
| Nol migration | **Terpenuhi** | `git status` — nol berkas `Migrations/` |
| Build hijau | **Terpenuhi** | 0 error |
| Laporan `BE-LAB-96.md` | **Terpenuhi** | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server lokal BE 7184 dan FE 3000 yang dinyalakan **sesi lain** (pekerjaan Radiologi) dihentikan untuk build, **atas persetujuan pemilik modul 2026-10-20**, lalu dinyalakan ulang dengan pengaturan semula (BE Development, `https://localhost:7184;http://localhost:5107`; FE `npm run dev` dengan rahasia sesi dari spec e2e) dan login dibuktikan. Log keduanya kini di scratchpad sesi ini |
| Masalah yang diketahui | `ARCH-GAP-LAB-15` (penyaringan `Draft` tidak simetris) **sengaja** dipertahankan — teoretis, nol jalur yang menulis `Draft` |
| Risiko tersisa | Rendah. Pemanggil berikutnya (`BE-LAB-98`) wajib memakai class ini dan tidak menulis rumus kedua |
| Perubahan sampingan | `NONE`. Perubahan Radiologi di working tree milik pekerjaan lain dan tidak disentuh |
| Interupsi | `NONE` |

**Status Git akhir** (`git status --short`, di luar `docs/`): task ini mengubah `LabDashboardService.cs`, `Program.cs`, dan
menambah `LabOrderDemandQuery.cs`. Delapan berkas `RadiologyManagement` yang berubah dan dua berkas baru Radiologi sudah
ada sebelum task dimulai — bukan bagian task ini. Nol stage, commit, atau push.

**Task berikutnya:** `BE-LAB-97` (alasan wajib batal pemeriksaan/wadah, dapat sejajar) atau `BE-LAB-98` (Statistik
Pesanan, kini tidak tertahan).
