# Laporan Perubahan Backend — `BE-LAB-98`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-98` |
| Judul | Statistik Pesanan beserta unduhan `.xlsx`, pencatat cetak, dan izinnya |
| Slice | `S16b-2` Statistik Pesanan Laboratorium; gelombang `MVP-15c` |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 6au.3 |
| Trace | `FR-20.5`..`FR-20.9`; `LAB-DEC-231`, `233`, `241`..`246`, `248`; `INV-59`; BR-145, BR-152, BR-153, BR-155; `AC-330` (sisi Statistik), `AC-333`..`AC-337` |
| Contract version | `LAB-API-v1` `r49` 44.3, 44.4; `LAB-PERM-v1` revision 17 bagian 19; `LAB-VAL-v1` `r21` 23.2 — **`approved` 2026-10-20** (Yoga Aji Pratama) |
| Dependency | `BE-LAB-96` — selesai |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2 (>20), berkas diubah 2 (12), logika bisnis 1, kontrak API 2 (endpoint baru), database 1 (kueri baca baru), keamanan/auth 1 (resource izin baru), UI 0 |
| Task mode | `BACKEND` — backend target tulis, frontend referensi read-only |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/{Controllers,Services,DTOs}`, `Program.cs`, laporan ini, baris status roadmap/traceability `MVP-15` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) + working tree `BE-LAB-96`, `BE-LAB-97` |
| Tanggal | 2026-10-20 |
| Status | ✅ **Selesai** — `403` non-superadmin terbukti 2026-10-20 dengan akun dr. Bima, bertahap tanpa izin → `Read` → `Export` (bagian 8). Semula *batas verifikasi* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — Laboratory, **`ACTIVE`** |
| Keberlakuan | `NEW CODE` (controller, service laporan, penulis `.xlsx`, pencatat keluaran, DTO); `TOUCHED LEGACY` (`LabOrderDemandQuery`, `LabFilterMetadataFactory`, `LabOperationalReportDtos`, `Program.cs`) |
| QBE yang berlaku | `QBE-SVC-001` (controller → service; nol `DbContext` di controller), `QBE-API-001` (`ApiResponse`, `400`/`422` pola `S16a`), `QBE-PERM-001` (`[AccessController]`/`[AccessAction]`/`[AccessPermission]` berpasangan), `QBE-DTO-001` (nol entity di kontrak), `QBE-VAL-001` (`VAL-147`..`149`), `QBE-LOG-001` (log keluaran beraktor), `QBE-OPT-001` (metadata dikonsumsi layar `FE-LAB-54`), `QBE-MOD-001`, `QBE-NAM-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — nol entity, configuration, migration; `QBE-CODE-*` — nol nomor bisnis |
| Branch | `yoga` |

---

## 1. Masalah yang diperbaiki

Kepala instalasi belum punya cara melihat **beban permintaan** laboratorium — berapa pesanan dan pemeriksaan yang diminta
dari Rawat Jalan, IGD, Rawat Inap, dan unit mana — kecuali menghitung manual dari daftar pesanan. Artifact *Menu Laporan
v2* meminta *Statistik Pesanan* (`LAB-EVD-014` §5); halaman v1-nya hanya placeholder.

## 2. Proses bisnis

1. Pemegang izin `LabOrderStatisticsReport : Read` membuka laporan dan memilih **periode** (wajib, paling panjang 366
   hari), lalu bila perlu **jenis kunjungan**, **unit layanan**, dan **disiplin**.
2. Backend menghitung, menurut **waktu pesanan diminta** dalam WIB:
   - **Pesanan** — bukan batal, bukan `Draft`;
   - **Pemeriksaan** — bukan batal, bukan gugur, pada pesanan yang tidak batal, **ditambah** permintaan yang belum masuk
     wadah — persis definisi Beranda (`INV-59`), dari class bersama `LabOrderDemandQuery` (`BE-LAB-96`).
3. Hasilnya disusun **Jenis kunjungan → Unit layanan → disiplin** dengan *Sub Total* per jenis kunjungan dan *Total*.
   Nama unit apa adanya dari data induk — *LABOR* tampil *Kamar Bersalin*. Pemeriksaan tanpa disiplin masuk kolom
   *Belum terklasifikasi*; kunjungan yang unitnya tidak ditemukan tampil *Unit tidak tercatat*.
4. Pemegang `Export` dapat **mengunduh** `.xlsx` — dibentuk dari respons yang sama dengan layar — dan **mencatat cetak**
   (`POST /print-log`) sebelum layar membuka dialog cetak. Keduanya tercatat di log: laporan, periode, penyaring, jumlah
   baris, pelaku. **Tidak ada data pasien di mana pun.**

**Contoh** (devYoga, superadmin, September–Oktober 2026): *Rawat Jalan → Laboratorium Klinik 6 pesanan / 8 pemeriksaan;
Medical Check Up 1/0; Rawat Jalan 16/12 — Sub Total 23/20*. Total pesanan 23 = tren Beranda September + Oktober (17 + 6).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/*`; `rules/backend/API_RULES.md`, `role-access-rules.md`, `REPORT_TEMPLATE.md`;
`LabOperationalReportController.cs`, `LabOperationalReportService.cs` (`ResolvePeriod`, `ReportPeriod`),
`LabOperationalReportDtos.cs`, `LabFilterMetadataFactory.cs`, `LabReportCsvWriter.cs`, `LabDashboardService.cs`,
`LabOrderDemandQuery.cs`; `Models/LabOrder.cs`; `RegistrationManagement/Enums/EncounterType.cs`;
`MasterData/Models/MstServiceUnit.cs`; `Services/Logging/LoggerService.cs`; `Attributes/AccessPermissionAttribute.cs`,
`AccessActionAttribute.cs`; `FinanceReceivableAgingReportService.cs` (pola `ClosedXML`); `Program.cs` (JSON options, DI);
kontrak `r49` 44.3–44.6, `LAB-PERM-v1` 19, `LAB-VAL-v1` 23; `02-backend-architecture.md` 32.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/LabOrderStatisticsReportController.cs` | **Baru.** Route `.../lab-order-statistics-reports`, resource `LabOrderStatisticsReport` (`SortOrder` 25). `GET /filters/metadata`, `GET /` (`Read`); `GET /export`, `POST /print-log` (`Export`). Nol akses `DbContext` |
| `Services/LabOrderStatisticsReportService.cs` | **Baru.** `GetFilterMetadataAsync` (unit aktif dari data induk), `GetAsync` (tiga kueri `GROUP BY` di basis data lewat `LabOrderDemandQuery`, susunan di memori), `DescribeFiltersAsync` (teks penyaring untuk log) |
| `Services/LabReportXlsxWriter.cs` | **Baru.** `ClosedXML`: judul, periode Indonesia, keterangan titik hitung, judul kolom dua tingkat, baris unit, *Sub Total*, *Total*; angka sebagai angka. `LabXlsxFile` |
| `Services/LabReportOutputLogger.cs` | **Baru.** `LogExportAsync`, `LogPrintAsync` → `AuditAsync` `<Resource>.Export`/`.Print`; isi di teks pesan, nol payload |
| `DTOs/LabOrderStatisticsReportDtos.cs` | **Baru.** Query (mewarisi `LabOperationalReportQuery`), print request, respons, sel, total, metadata, `LabServiceUnitOptionResponse`, `LabReportPrintLogResponse` |
| `Services/LabOrderDemandQuery.cs` | **Diperbarui.** `OrderDiscipline`; proyeksi `ExaminationDemand`, `OrderedProcedureDemand`, `OrderDemand` **disusun dari ekspresi disiplin yang sama** (penggantian parameter ekspresi) — aturan disiplin tetap satu; `LabDemandRow`. Kueri Beranda tidak berubah |
| `Services/LabFilterMetadataFactory.cs` | **Diperbarui.** `LabelDisiplin` menjadi `internal` (dipakai laporan); `LabelJenisKunjungan` baru; `LabOrderStatisticsReport()` (bagian statis metadata) |
| `DTOs/LabOperationalReportDtos.cs` | **Diperbarui.** `[JsonConverter(typeof(JsonStringEnumConverter))]` pada `Discipline` — lihat bagian 7 |
| `Program.cs` | `AddScoped` `LabOrderStatisticsReportService`, `LabReportXlsxWriter`, `LabReportOutputLogger` |

**Selisih dengan rancangan 32.4 (dicatat, bukan diubah diam-diam):**

| Selisih | Alasan |
| --- | --- |
| `LabReportPrintLogResponse` ditaruh di `LabOrderStatisticsReportDtos.cs`, bukan `LabCancellationReportDtos.cs` | Berkas itu baru lahir di `BE-LAB-99`; `BE-LAB-100` memakai ulang tipe ini |
| `LabOrderDemandQuery` menerima proyeksi baris permintaan | Pengelompokan jenis kunjungan × unit × disiplin butuh satu proyeksi; disusun dari ekspresi disiplin yang ada supaya `INV-59` tetap satu aturan |
| Konverter enum teks pada dua ruas query | Ditemukan uji HTTP: tanpa konverter, isi JSON `print-log` dengan `"encounterType":"Outpatient"` ditolak `400`, padahal `r49` 44.4 menetapkan ruas yang sama dengan query (nama) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — satu grup endpoint baru persis `r49` 44.3/44.4. Endpoint `S16a` tidak berubah (konverter menerima nama dan angka; query string tidak terpengaruh) |
| Database | Nol tabel, kolom, index, migration. Tiga kueri baca `AsNoTracking` + satu baca nama unit |
| Keamanan/Auth | Resource baru `LabOrderStatisticsReport` (`Read`, `Export`) — tercatat registri saat startup. **Nol kebijakan tersemai**: sebelum admin memberi, setiap pengguna non-superadmin `403` (`LAB-PERM-v1` 19.4). Tanpa identitas pasien |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order Statistics Report

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-order-statistics-reports/filters/metadata` | Pilihan penyaring: jenis kunjungan, unit layanan aktif, disiplin, batas 366 hari | `LabOrderStatisticsReport : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-order-statistics-reports` | Statistik pesanan per jenis kunjungan → unit → disiplin | `LabOrderStatisticsReport : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-order-statistics-reports/export` | Unduhan `.xlsx` | `LabOrderStatisticsReport : Export` |
| `POST` | `/api/v1/health-services/laboratory-management/lab-order-statistics-reports/print-log` | Mencatat cetak sebelum dialog cetak dibuka | `LabOrderStatisticsReport : Export` |

| Kode | Kapan | Pesan |
| --- | --- | --- |
| `200` | Laporan terbentuk / berkas terkirim / cetak tercatat | — |
| `400` | Periode kosong (`VAL-147`), awal sesudah akhir (`VAL-148`), enum tidak dikenal | *"Periode laporan wajib diisi."* / *"Tanggal awal tidak boleh melewati tanggal akhir."* |
| `403` | Tanpa `Read`; tanpa `Export` saat mengunduh atau mencatat cetak | Pesan izin platform |
| `422` | Periode lebih dari 366 hari (`VAL-149`) | *"Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya."* |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (server dimatikan lebih dulu) — dua kali, sebelum dan sesudah perbaikan konverter | 0 error | `PASS` | Keluaran build |
| Harness InMemory `be98harness` — 9 pesanan berbagai jenis kunjungan, unit, status, disiplin; batas WIB 30 Sep 23.30 / 1 Okt 00.10 | **29/29** | `PASS` | Keluaran harness |
| ↳ `AC-333`: pesanan Oktober dan September = tren Beranda | 6 = 6; 1 = 1 | `PASS` | Harness |
| ↳ `AC-334`/`AC-336`: pemeriksaan per disiplin setahun = grafik Beranda; gugur dan batal tidak; permintaan `Ordered` ya | PK 8, PA 2, MB 4, belum terklasifikasi 1 — sama | `PASS` | Harness |
| ↳ `AC-335`: *Belum terklasifikasi*, *Unit tidak tercatat*; respons dan `.xlsx` tanpa nama/No. RM pasien (string rahasia dicari) | Lulus | `PASS` | Harness |
| ↳ Penyaring jenis kunjungan, unit, disiplin; *Sub Total* = jumlah unit; *Total* = jumlah *Sub Total* | Lulus | `PASS` | Harness |
| ↳ `VAL-147`/`148` → `400`; 366 hari `200`, 367 hari `422` | Lulus | `PASS` | Harness |
| ↳ Berkas `.xlsx`: judul, *Periode: 1 Oktober 2026 – 31 Oktober 2026*, judul kolom, *Total* sama dengan layar dan bertipe angka | Lulus | `PASS` | Harness (dibaca ulang dengan `ClosedXML`) |
| ↳ Log unduhan dan cetak: isi di teks pesan, tanpa pasien; print-log tanpa periode `400` dan nol log | Lulus | `PASS` | Pencatat penangkap |
| ↳ Atribut izin: `[AccessPermission]` = `ControllerName` + argumen pertama `[AccessAction]` pada keempat endpoint | Cocok huruf demi huruf | `PASS` | Refleksi |
| ↳ Isi JSON print-log: nama enum, angka, kosong | Diterima | `PASS` | Harness |
| Regresi Beranda — harness `be96harness` (berkas tidak disunting) sesudah `LabOrderDemandQuery` diperluas | **Identik** dengan acuan | `PASS` | Dua kali |
| Startup Development | *"Permission registry valid. 1755 identitas kanonik dari 1755 kemampuan terdaftar."*; `/health` `200` | `PASS` | `scratchpad/be.log` |
| HTTP superadmin (devYoga): metadata `200` (19 unit, 5 jenis kunjungan); laporan Sep–Okt `200`; total pesanan 23 = tren Beranda; PK/PA/MB setahun = Beranda | Lulus | `PASS` | `scratchpad/be98-http.mjs` |
| HTTP: respons tanpa `medicalRecordNumber`/`patientName`; periode kosong/terbalik `400`, 367 hari `422`, `encounterType=Bogus` `400` | Lulus | `PASS` | Skrip yang sama |
| HTTP: `GET /export` → `200`, `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, `statistik-pesanan_2026-09-01_2026-10-31.xlsx`, berkas zip sah | Lulus | `PASS` | Skrip yang sama |
| HTTP: `POST /print-log` isi JSON nama enum → `200` (sebelum perbaikan konverter: `400` — **NEW ERROR** yang diperbaiki dalam task ini); tanpa periode `400` | Lulus | `PASS` | Skrip yang sama |
| Log aplikasi `Logs/quilvian-backend-20261020.json` | Dua baris `.Export`, satu `.Print`, isi lengkap di pesan, pelaku tercatat, tanpa pasien | `PASS` | Berkas log |
| `403` bagi akun non-superadmin | Terbukti 2026-10-20 — tanpa izin: keempat endpoint `403`; `Read` saja: unduh dan print-log `403` | `PASS` | Bagian 8 |

Uji manual: `NOT APPLICABLE` — layar Statistik Pesanan adalah `FE-LAB-54`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Empat endpoint berjalan persis `r49` 44.3/44.4 | **Terpenuhi** | HTTP |
| `AC-333`, `AC-334`, `AC-336` — sama dengan Beranda; gugur dan batal tidak dihitung | **Terpenuhi** | Harness + HTTP devYoga |
| `AC-335` — *Belum terklasifikasi*, *Unit tidak tercatat*, tanpa identitas pasien di respons dan `.xlsx` | **Terpenuhi** | Harness + HTTP |
| `AC-337` (sisi Statistik) — print-log `200` dan satu log; gagal validasi nol log | **Terpenuhi** sisi backend — `403` bagi pemegang `Read` saja terbukti (bagian 8); fail-closed layar milik `FE-LAB-54` | Harness + HTTP |
| `AC-330` (sisi Statistik) — `403` bagi `LabOrder : Read` saja dan `LabOperationalReport : Read` saja | **Terpenuhi** — dr. Bima memegang `LabOrder : Read` dan `LabOperationalReport : Read`/`Export` → `403` (bagian 8) | HTTP |
| Dua aksi izin terdaftar; `PermissionRegistryValidator` menerima | **Terpenuhi** | Log startup |
| `.xlsx` terbuka dengan angka sebagai angka | **Terpenuhi** | Harness |
| Log sesuai 19.5 | **Terpenuhi** | Log aplikasi |
| Nol rumus hitung kedua (`INV-59`) | **Terpenuhi** | Proyeksi disusun dari ekspresi bersama; regresi Beranda identik |
| Nol migration | **Terpenuhi** | — |
| Laporan `BE-LAB-98.md` | **Terpenuhi** | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server lokal BE 7184 dan FE 3000 dimatikan untuk dua kali build dan dinyalakan ulang; berjalan sebagai proses latar sesi ini. Konverter enum teks kini juga berlaku pada `Discipline` query `S16a` (hanya dipakai lewat query string — perilakunya tetap) |
| Masalah yang diketahui | `AC-330` runtime menunggu akun non-superadmin. devYoga hanya memuat kunjungan Rawat Jalan — pengelompokan Rawat Inap/IGD/Kamar Bersalin terbukti di harness saja (`T-S16B-16`, langkah rilis `MVP-15e` butir 3) |
| Risiko tersisa | Rendah-sedang. Kueri `GROUP BY` atas proyeksi dengan navigasi kunjungan (LEFT JOIN) — kinerja pada data besar belum diukur; periode dibatasi 366 hari |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

**Status Git akhir** (`git status --short`, di luar `docs/`): task ini menambah `LabOrderStatisticsReportController.cs`,
`LabOrderStatisticsReportService.cs`, `LabReportXlsxWriter.cs`, `LabReportOutputLogger.cs`, `LabOrderStatisticsReportDtos.cs`
dan mengubah `LabOrderDemandQuery.cs`, `LabFilterMetadataFactory.cs`, `LabOperationalReportDtos.cs`, `Program.cs`. Berkas
lain milik `BE-LAB-96`/`97` atau pekerjaan Radiologi. Nol stage, commit, atau push.

**Task berikutnya:** `BE-LAB-99` — Laporan Pembatalan (daftar, detail, penyaring, index, jejak baca). Untuk menutup
`AC-330`, pemilik modul dapat memberikan sandi satu akun non-superadmin, atau menguji sendiri sesudah izin diberikan.

## 8. Addendum verifikasi — 2026-10-20 (langkah rilis `MVP-15e` butir 2, 4)

Atas instruksi pemilik modul — *"lakukan migrasi tersebut"* beserta sandi akun dr. Bima untuk bukti validasi — dan
pilihannya *"Bertahap: Read → uji → Export"*. Sandi tidak disimpan di berkas mana pun.

| Langkah | Hasil | Bukti |
| --- | --- | --- |
| Tahap 0 — akun dr. Bima (Kepala Instalasi: memegang `LabOperationalReport : Read`/`Export` dan `LabOrder : Read`, nol izin laporan baru) | Kesepuluh endpoint kedua laporan **`403`**; kontrol `LabOperationalReport` `200`; log hanya `SECURITY_ACCESSDENIED`, nol baris log laporan | `scratchpad/bima-403.mjs` |
| Tahap 1 — jabatan diberi `Read` kedua laporan (set 41 → 43; nol izin hilang; nol izin tersembunyi sebelum kirim ulang) | Metadata, data laporan, pilihan petugas `200`; detail id acak `404`; **unduh dan print-log `403`** pada kedua laporan; satu log baca, nol log unduh/cetak | idem + log aplikasi |
| Tahap 2 — ditambah `Export` (set 43 → 45) | Seluruh endpoint `200`; tepat satu baris log per peristiwa (`.Read`, `.Export`, `.Print`), tanpa data pasien | Log aplikasi |

Jabatan *Kepala Instalasi Laboratorium* kini memegang keempat izin laporan `S16b` — izin bawaan `LAB-DEC-239` untuk kepala
instalasi terpasang di devYoga. Jabatan manajemen **ditunda** atas keputusan pemilik modul 2026-10-21 — master jabatan belum
punya jabatan manajemen pelayanan (hanya *Manajer Finance* dan *Manajer HR*, yang tidak diberi laporan berisi data pasien);
diberikan admin begitu jabatannya dibuat. Akun analis (Gilang) diuji 2026-10-21:
kontrol positif `lab-orders/filters/metadata` `200`, kesepuluh endpoint `S16b` kedua laporan `403`.
