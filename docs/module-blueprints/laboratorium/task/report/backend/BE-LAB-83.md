# Laporan Perubahan Backend — `BE-LAB-83`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-83` |
| Judul | Laporan jumlah pemeriksaan, penyaring periode, dan izin laporan |
| Slice | Gelombang `MVP-11a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6an.2** |
| Trace | `FR-17.1`, `FR-17.5`, `FR-17.6`, `FR-17.7`, `FR-17.9`; `LAB-DEC-159` butir 2, `LAB-DEC-155`, `LAB-DEC-160`, `LAB-DEC-071`; `INV-55`; 23.10 butir 2-4 dan 6 |
| Contract version | `LAB-API-v1` **`r37`** 32.2-32.3; `LAB-VAL-v1` **`r15`** `VAL-147`..`VAL-149`; `LAB-PERM-v1` **revision 12** 14.2-14.3 — ketiganya **`approved` 2026-09-28** |
| Dependency | `BE-LAB-82` ✅ (`LabReleasableDisciplines`), `BE-LAB-70` ✅ (`ReleasedAt`) |
| Klasifikasi | `MEDIUM` — skor 9: repository 0, berkas diperiksa 1 (±14), berkas diubah 2 (6), logika bisnis 1, kontrak API **2** (grup endpoint baru), database 1 (kueri baca berkelompok), keamanan/auth **2** (resource izin baru), UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`82` yang belum ter-commit. Rancangan gelombang pada `4a94628a`; impact scan dijalankan saat `BE-LAB-82` — nol perubahan source relevan |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — kedua endpoint berjalan **lewat HTTP terhadap PostgreSQL** (`200`, `VAL-147` ×3, `VAL-148` dengan bunyi yang sama dengan daftar Pemeriksaan, `VAL-149` 366 → `200` dan 367 → `422`, disiplin tak dikenal `400`, `403` bagi akun tanpa izin laporan, `401`). Pengelompokan dibuktikan terjadi di **SQL** (`GROUP BY` pada log perintah Npgsql, sesi read-only), dan batas periode WIB terlihat pada parameter yang dikirim ke PostgreSQL. Harness **25/25**; regresi `69`, `72`..`77`, `81`, `82` utuh; build 0 error tanpa warning baru; registri **1575 → 1576**. Isi angka pada data sungguhan belum teramati — dev nol punya hasil dirilis |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `LabOperationalReportController`, `LabOperationalReportService`, `LabOperationalReportDtos`. `TOUCHED LEGACY` — `LabFilterMetadataFactory` (metode baru), `LabQueryDateRange` (konstanta pesan), `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (controller → service; nol akses context di controller), `QBE-API-001` (`ApiResponse<T>`, kode status kontrak), `QBE-VAL-001` (`VAL-147`..`VAL-149` di service, satu aturan bagi laporan dan unduhannya kelak), `QBE-DTO-001` (DTO persis 23.4; nol entity terekspos), `QBE-PERM-001` (`[AccessController]`, `[AccessAction]` berpasangan `[AccessPermission]`), `QBE-OPT-001` (metadata dikonsumsi layar `FE-LAB-44`). **Tidak berlaku:** `QBE-LOG-001` (nol perubahan state; membuka laporan sengaja tidak dicatat, PERM 14.3), `QBE-ENT-*`/`QBE-DB-*` (nol entity, nol migration), `QBE-PAGE-001` (laporan ringkasan, bukan daftar) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris `Lab` `ACTIVE`) |

---

## 1. Masalah yang diperbaiki

Kepala instalasi belum punya satu pun angka ringkas tentang **berapa banyak hasil yang keluar** dari
laboratorium. Satu-satunya cara adalah menghitung dari daftar pesanan, yang memuat pesanan batal,
hasil yang belum disahkan, dan pesanan yang tanggalnya terbaca per UTC.

---

## 2. Proses bisnis

**Laporan jumlah pemeriksaan** menjawab: *berapa hasil yang dirilis pada periode ini*, per disiplin dan
per jenis pemeriksaan.

| Aturan | Contoh |
| --- | --- |
| Yang dihitung hanya hasil **dirilis** (`LAB-DEC-155`) | Hemoglobin tervalidasi 20 September yang belum dirilis **tidak** dihitung |
| Tanggalnya **tanggal rilis WIB** | Kalium dirilis 1 Oktober 06.30 WIB masuk **Oktober**, walau di basis data tercatat 30 September 23.30 UTC. Hasil dirilis 1 September 00.10 WIB masuk **September** |
| Disiplin yang belum punya jalur rilis ditulis **belum dapat dihitung** — bukan 0 | Patologi Anatomi: *"Rilis hasil Patologi Anatomi belum tersedia."* — angka kosong |
| Order lama tanpa disiplin tidak hilang | Dikelompokkan **Belum tergolong** dan ikut dijumlahkan |
| Nama pemeriksaan = nama **saat dipesan** | Katalog diubah menjadi *Kalium Serum* — laporan tetap menulis *Kalium* |

**Periode** wajib diisi, tanggal awal tidak boleh sesudah tanggal akhir, dan paling panjang **366 hari**
— batas yang melindungi basis data yang dipakai bersama.

**Izin tersendiri** (`LAB-DEC-160`): laporan merangkum seluruh pasien dan seluruh petugas, sehingga hak
baca daftar Laboratorium **tidak** membukanya. Sampai admin memberi `LabOperationalReport : Read`
kepada jabatan kepala instalasi dan manajemen (langkah rilis `MVP-11c`), setiap pengguna menerima
`403`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6an, 6an.2 | Cakupan, empat jebakan, verifikasi, DoD |
| `contracts/api-contract.md` bagian 32 | Endpoint, kode status, bentuk permintaan dan respons |
| `contracts/validation-matrix.md` `VAL-147`..`VAL-149` | Teks dan kode |
| `contracts/permission-audit-matrix.md` bagian 14 | Resource, aksi, pencatatan |
| `02-backend-architecture.md` 23.3, 23.4, 23.10, 23.11 | Kelas, DTO, keputusan, privasi |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-28 | Skenario `AC-250`, periode, izin, privasi |
| `Services/LabQueryDateRange.cs`, `Helpers/AppDateTimeHelper.cs` | Tanggal polos dibaca Asia/Jakarta; akhir rentang dinaikkan ke penghabisan hari |
| `Controllers/LabMonitoringController.cs` | Pesan `VAL-148` yang wajib sama persis |
| `Services/LabFilterMetadataFactory.cs`, `DTOs/LabFilterAndSummaryDtos.cs` | Pola metadata penyaring |
| `Controllers/LabFourEyesExceptionReasonController.cs` | Pola `[AccessController]` dan deskripsi aksi bersama |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/LabOperationalReportController.cs` | **Baru.** Route, tag, dan `[AccessController]` resource `LabOperationalReport`. `GET /filters/metadata` dan `GET /examination-count`, masing-masing `[AccessAction("Read", …, AccessType = Read)]` berpasangan `[AccessPermission("LabOperationalReport", "Read")]`. **Aksi `Export` belum dipasang** — milik `BE-LAB-86` |
| `Services/LabOperationalReportService.cs` | **Baru.** `GetExaminationCountAsync` — satu kueri `AsNoTracking` berkelompok (disiplin order × jenis pemeriksaan × nama tersimpan). `ResolvePeriod` — `VAL-147`..`VAL-149`, dipakai bersama laporan berikutnya. `LabOperationalReportValidationException` membawa kode status |
| `DTOs/LabOperationalReportDtos.cs` | **Baru.** `LabOperationalReportQuery`, `LabReportPeriodResponse`, `LabExaminationCountReportResponse`, `LabExaminationCountRow` persis 23.4; `LabExaminationCountProcedureRow` (rincian per jenis); `LabOperationalReportFilterMetadataResponse` (nama `r37` 32.2) |
| `Services/LabFilterMetadataFactory.cs` | `LabOperationalReport()` — tiga disiplin, tiga parameter (`startDate`, `endDate` wajib), `maxPeriodDays` 366 |
| `Services/LabQueryDateRange.cs` | Konstanta `InvertedRangeMessage` — bunyi `VAL-148` satu sumber bagi laporan |
| `Program.cs` | `AddScoped<LabOperationalReportService>()` |

**Nol entity, nol migration.** Satu kunci hak akses baru: `LabOperationalReport : Read`.

**Keempat jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Membandingkan `ReleasedAt` dengan tanggal UTC mentah | Periode lewat `LabQueryDateRange.Normalize` — tanggal polos dibaca WIB, akhir dinaikkan ke penghabisan hari | Parameter yang dikirim ke PostgreSQL untuk September: `2026-08-31T17:00:00Z` .. `2026-09-30T16:59:59.9999999Z`; harness: 1 Okt 06.30 WIB → Oktober, 1 Sep 00.10 WIB → September |
| Menulis 0 bagi disiplin yang belum dapat dihitung | Disiplin di luar `LabReleasableDisciplines` → `isCountable = false`, `total` kosong, alasan terisi | HTTP: PA dan Mikrobiologi `total: null` |
| Membuang order berdisiplin kosong | Dikelompokkan *Belum tergolong* | Harness |
| Menghitung dari `FinalizedAt` atau status | Satu-satunya dasar `ReleasedAt` | Harness: tervalidasi belum dirilis tidak dihitung |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Validasi periode di service, bukan controller | Tiga endpoint unduh `BE-LAB-86` wajib menolak periode yang sama persis; satu `ResolvePeriod` mencegah dua salinan aturan. Pesannya tetap sama persis dengan kontrak |
| Satu jenis pemeriksaan dengan dua nama tersimpan (katalog diubah di antara dua pemesanan) | **Satu baris**, nama dari rilis **terakhir** dalam periode. Kontrak meminta rincian per jenis pemeriksaan dengan nama tersimpan; dua baris bernama beda untuk satu jenis membingungkan pembaca |
| Disiplin yang dapat dihitung tanpa rilis pada periode itu | Ditulis **0** — di sini 0 memang benar: jalurnya ada, rilisnya nihil |
| *Belum tergolong* | Hanya tanpa penyaring disiplin, dan hanya bila ada yang dirilis — penyaring memilih disiplin **order** |
| Pemeriksaan atau order terhapus | Tidak dihitung |
| Status pemeriksaan (gugur/batal) | **Tidak disaring** — yang dihitung adalah fakta rilis. Rilis sesudah batal ditolak `VAL-143`, jadi kombinasi itu tidak lahir lewat aplikasi |
| Pesan `VAL-148` | Konstanta baru di `LabQueryDateRange`; enam controller lama tetap memakai literal dengan bunyi yang sama — tidak disentuh di task ini |
| Deskripsi aksi `Read` | Satu teks bagi kedua endpoint — registri menampilkan satu baris per kunci |
| `SortOrder` controller | 24 — sesudah `LabFourEyesExceptionReason` (23) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — dua endpoint baru `r37` 32.2. Empat endpoint lain grup ini milik `BE-LAB-84`..`86` |
| Database | **Baca saja**, satu kueri `GROUP BY` per laporan; nol migration. Periode dibatasi 366 hari (`VAL-149`) |
| Keamanan/Auth | Resource baru `LabOperationalReport : Read`. **Nol kebijakan diberikan** — setiap pengguna selain superadmin `403` sampai langkah rilis `MVP-11c`. Respons tanpa nama pasien, No. RM, nilai hasil, maupun nama petugas (23.11) |
| Pencatatan | Membuka laporan **tidak** dicatat (PERM 14.3). Log yang muncul hanya log keamanan platform untuk penolakan `403` |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Operational Report

Base URL: `api/v1/health-services/laboratory-management/lab-operational-reports`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring — periode dan disiplin | `LabOperationalReport : Read` | - | `ApiResponse<LabOperationalReportFilterMetadataResponse>` |
| `GET` | `/examination-count` | Jumlah pemeriksaan yang dirilis, per disiplin dan per jenis | `LabOperationalReport : Read` | `LabOperationalReportQuery` | `ApiResponse<LabExaminationCountReportResponse>` |

**`LabOperationalReportQuery`:** `startDate` (wajib, `YYYY-MM-DD`, WIB, inklusif), `endDate` (wajib),
`discipline` (`ClinicalPathology` / `AnatomicalPathology` / `Microbiology`; kosong = seluruhnya).

| Kode | Kapan | Pesan |
| --- | --- | --- |
| `200` | Laporan terbentuk, termasuk periode tanpa data | *"Laporan jumlah pemeriksaan berhasil dibentuk."* |
| `400` | `VAL-147` | *"Periode laporan wajib diisi."* |
| `400` | `VAL-148` | *"Tanggal awal tidak boleh melewati tanggal akhir."* |
| `400` | Disiplin tak dikenal | Jawaban validasi model bawaan ASP.NET Core |
| `401` | Tanpa token | — |
| `403` | Tanpa `LabOperationalReport : Read` | *"Anda tidak memiliki akses ke menu atau fitur ini."* |
| `422` | `VAL-149` | *"Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya."* |

Contoh respons September 2026 — **tangkapan sungguhan** dari database dev:

```json
{
  "period": { "startDate": "2026-09-01", "endDate": "2026-09-30", "generatedAt": "2026-09-30T03:08:06Z" },
  "rows": [
    { "discipline": "ClinicalPathology", "disciplineName": "Patologi Klinik", "isCountable": true, "notCountableReason": null, "total": 0, "procedures": [] },
    { "discipline": "AnatomicalPathology", "disciplineName": "Patologi Anatomi", "isCountable": false, "notCountableReason": "Rilis hasil Patologi Anatomi belum tersedia.", "total": null, "procedures": [] },
    { "discipline": "Microbiology", "disciplineName": "Mikrobiologi", "isCountable": false, "notCountableReason": "Rilis hasil Mikrobiologi belum tersedia.", "total": null, "procedures": [] }
  ],
  "totalCountable": 0
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 86 detik | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-83` | **25 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| **SQL terhadap PostgreSQL** — sesi `default_transaction_read_only = on` | Satu perintah per laporan: `SELECT … count(*)::int, max(…) … GROUP BY "Discipline", "ProcedureId", COALESCE(snapshot, nama katalog)` — **pengelompokan di basis data**. Parameter September: `2026-08-31T17:00:00Z` .. `2026-09-30T16:59:59.9999999Z` | `PASS` | Log `RelationalEventId.CommandExecuted` |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21, `81` 25/25, `82` karakterisasi 12/12 dan unit 13/13 | `PASS` | Harness yang sama |
| **`PermissionRegistryValidator`** | Aplikasi menyala; registri **1575 → 1576** — satu kunci baru `LabOperationalReport : Read` | `PASS` | Log startup |
| Swagger | Dua endpoint di tag *Lab Operational Report*; `examination-count` memuat `StartDate`, `EndDate`, `Discipline` dan respons `200`/`400`/`422`; tanpa teks deskripsi | `PASS` | `/swagger/health-services/swagger.json` |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |
| Log baca | **Nol** baris log domain untuk pembukaan laporan yang berhasil | `PASS` | Log aplikasi |

**Rincian HTTP** — aplikasi sungguhan, database dev bersama, nol penulisan.

| Panggilan | Akun | Hasil |
| --- | --- | --- |
| `GET /filters/metadata` | Superadmin | `200` — tiga disiplin, `startDate`/`endDate` wajib, `maxPeriodDays` 366 |
| `GET /examination-count?startDate=2026-09-01&endDate=2026-09-30` | Superadmin | `200` — PK `total 0`, PA dan Mikrobiologi *belum dapat dihitung* (`total: null`) |
| `… &discipline=ClinicalPathology` | Superadmin | `200` — hanya baris PK |
| Tanpa `startDate`; tanpa `endDate`; tanpa keduanya | Superadmin | `400` ×3 — *"Periode laporan wajib diisi."* |
| `startDate=2026-09-30&endDate=2026-09-01` | Superadmin | `400` — *"Tanggal awal tidak boleh melewati tanggal akhir."* |
| Pembanding: `GET /lab-monitoring/clinical-pathology` dengan periode terbalik yang sama | Superadmin | `400` — bunyi **sama persis** |
| `2026-01-01 .. 2027-01-01` (366 hari) | Superadmin | `200` |
| `2026-01-01 .. 2027-01-02` (367 hari) | Superadmin | `422` — bunyi `VAL-149` |
| `discipline=Hematology` | Superadmin | `400` — validasi model |
| Kedua endpoint | Kepala Instalasi — jabatannya tidak memegang `LabExamination`, `LabWorklist`, `LabMonitoring`, maupun `LabOperationalReport` (kueri baca-saja) | `403` ×2 |
| Tanpa token | — | `401` |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-250`** — Kalium A 12 Sep, Kalium B 1 Okt 06.30 WIB, Glukosa C batal | September: Kalium hanya dari A (dan satu Kalium bernama lama); Oktober: Kalium B; Glukosa tidak di mana pun |
| `AC-250` batas awal — 1 Sep 00.10 WIB; pembanding 31 Agu 23.50 WIB | Masuk September; yang 31 Agustus tidak |
| `AC-250` — tervalidasi belum dirilis | Tidak dihitung |
| `AC-250` — nama tersimpan | *Kalium*, bukan nama katalog *Kalium Serum (nama katalog baru)*; satu baris per jenis |
| Pemeriksaan terhapus; order terhapus | Tidak dihitung |
| **`ARCH-GAP-LAB-11`** — PA dengan laporan Final pada periode itu | `isCountable = false`, `total` kosong, alasan terisi |
| Order tanpa disiplin | *Belum tergolong*, `total 1`, ikut `totalCountable` |
| Urutan baris | PK, PA, Mikrobiologi, Belum tergolong |
| Periode satu hari (`LAB-DEC-071`) — 31 Agustus | Hasil 23.50 WIB masuk; 1 Sep 00.10 WIB tidak |
| Penyaring `ClinicalPathology`; `AnatomicalPathology` | Hanya PK (tanpa *Belum tergolong*); hanya PA *belum dapat dihitung* |
| Periode tanpa data | `200`; PK `0`; tanpa *Belum tergolong* |
| `VAL-147` ×3, `VAL-148`, `VAL-149` 366/367 | Kode dan bunyi tepat; penolakan periode tanpa satu kueri pun |
| Jumlah kueri laporan | **1** |
| Metadata | Tiga disiplin, tiga parameter, 366 hari |
| **Privasi respons** | Ruas: `Discipline`, `DisciplineName`, `EndDate`, `GeneratedAt`, `IsCountable`, `NotCountableReason`, `Period`, `ProcedureId`, `ProcedureName`, `Procedures`, `Rows`, `StartDate`, `Total`, `TotalCountable` — nol ruas pasien, No. RM, nilai hasil, atau petugas |

**Tidak dijalankan:**

- Angka **berisi** lewat HTTP — dev nol punya hasil dirilis (jalur rilis menunggu izin dan penunjukan
  Human Resource; lihat `BE-LAB-74`). Batas WIB terbukti pada parameter PostgreSQL dan pada harness.
- **`AC-253` dengan persona tepatnya** — pemegang `LabExamination : Read` dan `LabWorklist : Read` tanpa
  izin laporan. Kredensial akun seperti itu tidak tersedia di sesi ini. Penolakan `403` dibuktikan pada
  akun tanpa izin laporan (Kepala Instalasi). Karena penegakannya filter hak akses platform yang sama
  bagi setiap resource, hak baca lain tidak ikut membuka laporan.
- *`Read` tanpa `Export`* — `Export` lahir di `BE-LAB-86`.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-250` beserta empat barisnya | ✅ **Terpenuhi** pada harness; batas WIB juga pada parameter PostgreSQL | — |
| `ARCH-GAP-LAB-11` jumlah | ✅ **Terpenuhi** — harness dan HTTP | — |
| Order tanpa disiplin | ✅ **Terpenuhi** pada harness | — |
| Penyaring disiplin | ✅ **Terpenuhi** — harness dan HTTP | — |
| `VAL-147`..`VAL-149` | ✅ **Terpenuhi** — harness dan HTTP | — |
| Disiplin tak dikenal → `400` | ✅ **Terpenuhi** — HTTP | — |
| `AC-253` untuk dua endpoint ini | ✅ **Terpenuhi sebagian** — `403` lewat HTTP bagi akun tanpa izin laporan; persona pemegang hak baca Lab belum dijalankan | 5 |
| `PermissionRegistryValidator` | ✅ **Terpenuhi** — aplikasi menyala, 1576 | — |
| Privasi respons | ✅ **Terpenuhi** | — |
| Verifikasi roadmap — log kueri: pengelompokan di SQL | ✅ **Terpenuhi** — `GROUP BY` di PostgreSQL | — |
| DoD — dua endpoint; tiga aturan periode; resource terdaftar; nol migration; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Kebijakan `LabOperationalReport : Read` belum diberikan kepada jabatan mana pun — sesuai rancangan, milik langkah rilis `MVP-11c`, yang langkah 2-nya `BLOCKED` sampai jabatan *manajemen* ditetapkan. **Larangan PERM 14.6:** kebijakan laporan tidak boleh disalin dari pemegang `LabExamination : Read`. **2.** Enam controller lama masih menulis pesan `VAL-148` sebagai literal; bunyinya sama, dan beralih ke konstanta cukup dikerjakan saat berkas itu disentuh |
| Risiko tersisa | **Rendah.** Baca saja; periode dibatasi; izin tertutup secara bawaan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-83`: `??` `Controllers/LabOperationalReportController.cs`, `Services/LabOperationalReportService.cs`, `DTOs/LabOperationalReportDtos.cs`, laporan ini; ` M` `Services/LabFilterMetadataFactory.cs`, `Services/LabQueryDateRange.cs`, `Program.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`82` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-84` (laporan penolakan wadah) naik menjadi `SIAP DIKERJAKAN` — **task itu membawa satu-satunya migration gelombang ini** (`AddLabSpecimenDecidedAtIndex`). Pembuatan migration adalah wewenang terpisah (`CLAUDE.md`), sehingga saat dikerjakan butuh izin eksplisit untuk **membuat** migration; **menerapkannya** ke database dev adalah izin lain lagi. **2.** `BE-LAB-78` tetap `SIAP DIKERJAKAN` |
