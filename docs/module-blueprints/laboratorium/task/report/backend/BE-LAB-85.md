# Laporan Perubahan Backend — `BE-LAB-85`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-85` |
| Judul | Laporan waktu penyelesaian |
| Slice | Gelombang `MVP-11a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6an.4** |
| Trace | `FR-17.3`, `FR-17.4`, `FR-17.5`, `FR-17.9` bagian laporan ini; `LAB-DEC-159` butir 4; `AC-17`; `INV-57`; `ARCH-GAP-LAB-13`; 23.10 butir 5 |
| Contract version | `LAB-API-v1` **`r37`** 32.2-32.3 beserta contoh respons `/turnaround-time` — **`approved` 2026-09-28** |
| Dependency | `BE-LAB-84` ✅ (urutan berkas), `BE-LAB-82` ✅ (`LabCitoTurnaroundPolicy`, `LabReleasableDisciplines`), `BE-LAB-77` ✅ (daftar pantau tanpa hasil dirilis) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±10), berkas diubah 2 (3), logika bisnis **2** (TAT dan terlambat satu rumus dengan daftar pantau), kontrak API 1 (endpoint aditif), database 1 (kueri baca), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7ff35b8c` (branch `yoga`), di atas perubahan `BE-LAB-84` yang belum ter-commit |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — `GET /turnaround-time` berjalan **lewat HTTP terhadap PostgreSQL** dengan seluruh kode status kontrak; kueri diterjemahkan Npgsql (sesi read-only). **`AC-252` terbukti dua arah** pada harness terhadap **daftar pantau sungguhan** (`LabWorklistService.GetCitoOverdueAsync`), termasuk pergantian batas 60 → 90 yang mengubah laporan **dan** daftar pantau bersamaan. Harness **21/21**; regresi `69`, `72`..`77`, `81`..`84` utuh; build 0 error tanpa warning baru; **nol pembacaan `LabValueBound`** di service laporan; registri tetap 1576; nol migration. Angka berisi lewat HTTP belum teramati — dev nol punya hasil dirilis |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `GetTurnaroundTimeAsync`, `LabTurnaroundTimeReportResponse`, `LabTurnaroundTimeRow`, endpoint baru. `TOUCHED LEGACY` — nol |
| QBE yang berlaku | `QBE-SVC-001` (kebijakan batas cito diinjeksi ke service; controller tanpa context), `QBE-API-001`, `QBE-VAL-001` (`ResolvePeriod` bersama), `QBE-DTO-001` (DTO persis 23.4), `QBE-PERM-001` (kunci `LabOperationalReport : Read` yang sudah ada). **Tidak berlaku:** `QBE-LOG-001` (baca saja), `QBE-ENT-*`/`QBE-DB-*` (nol entity, nol migration) |
| Governance yang dibaca | Sama dengan `BE-LAB-83`/`84` pada sesi yang sama |

---

## 1. Masalah yang diperbaiki

Kepala instalasi tidak dapat melihat **seberapa cepat hasil keluar**. Yang lebih berbahaya: tanpa satu sumber
batas, laporan waktu penyelesaian yang dibangun terpisah dapat menyebut sebuah Kalium *tepat waktu*
sementara daftar pantau menyebutnya *terlambat* — dua jawaban untuk satu pertanyaan (`INV-57`).

---

## 2. Proses bisnis

**Waktu penyelesaian** dihitung dari **wadah dinyatakan layak** sampai **hasil dirilis**, per disiplin dan
per kesegeraan (*cito* dan *rutin*).

| Aturan | Contoh |
| --- | --- |
| Titik mulai = wadah layak, bukan pengambilan atau pemesanan | Waktu tunggu sebelum sampel sampai di laboratorium tidak dibebankan ke laboratorium |
| *Terlambat* hanya bila selang **melebihi** batas cito | Kalium cito layak 08.00, dirilis 09.10, batas 60 → **70 menit, terlambat**. Dirilis tepat 09.00 → **tidak** terlambat |
| Batas dan perbandingan **sama persis** dengan daftar pantau | Kalium layak 08.00 belum dirilis tampil *terlambat 5 menit* di daftar pantau pukul 09.05, lalu dirilis 09.10 → terlambat di laporan. Kalium kedua dirilis 08.55 → **tidak** tampil di daftar pantau **dan** tidak terlambat di laporan |
| Batas yang berlaku **saat laporan dibuka** | Batas Kalium diubah 60 → 90: laporan dan daftar pantau **bersama-sama** berhenti menyebut Kalium 70 menit terlambat |
| Cito tanpa batas | Tidak dinilai terlambat; dihitung pada *tanpa batas*; tetap masuk rata-rata |
| Pengambilan ulang | Dihitung dari wadah **pengganti** — wadah ditolak 08.00, pengganti layak 09.00, hasil 09.40 → **40 menit** |
| Rutin | Masuk rata-rata rutin; kolom *terlambat* dan *tanpa batas* kosong pada baris rutin |
| Disiplin tanpa jalur rilis | *Belum dapat dihitung* — seluruh angka kosong, bukan 0 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6an.4 | Cakupan, tiga jebakan, verifikasi, DoD |
| `contracts/api-contract.md` 32.2-32.3 | Contoh respons `/turnaround-time`, arti setiap ruas |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-28 | `AC-252` beserta batas persis, *dan sebaliknya*, rutin, cito tanpa batas; `INV-57` pengambilan ulang; batas satu sumber; `ARCH-GAP-LAB-11` TAT |
| `Services/LabWorklistService.cs` — `GetCitoOverdueAsync` | Perbandingan *terlambat* yang wajib ditiru persis |
| `Services/LabCitoTurnaroundPolicy.cs`, `Constants/LabReleasableDisciplines.cs` | Sumber batas dan disiplin |
| `Repositories/Configurations/…/LabExaminationConfiguration.cs` | Index `ChargeEligibleAt` dan `Urgency` sudah ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabOperationalReportService.cs` | Konstruktor menerima `LabCitoTurnaroundPolicy`. `GetTurnaroundTimeAsync` — proyeksi lima kolom (disiplin order, kesegeraan, jenis, `ChargeEligibleAt`, `ReleasedAt`) untuk pemeriksaan dirilis pada periode itu yang wadahnya pernah layak; batas cito sekali baca lewat kebijakan, **dilewati** bila tidak ada rilis cito; `BarisSelang` — rata-rata satu desimal, *terlambat* dengan `tenggat = layak + batas; terlambat bila dirilis > tenggat` |
| `DTOs/LabOperationalReportDtos.cs` | `LabTurnaroundTimeReportResponse`, `LabTurnaroundTimeRow` — ruas persis 23.4 |
| `Controllers/LabOperationalReportController.cs` | `GET /turnaround-time`, `[AccessPermission("LabOperationalReport", "Read")]` |

**Nol entity, nol migration, nol aksi hak akses baru.**

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Membaca `LabValueBound` langsung — rumus kedua | Batas **hanya** dari `LabCitoTurnaroundPolicy.GetLimitsAsync` | `grep LabValueBound` pada service laporan: **0**; pergantian 60 → 90 mengubah laporan dan daftar pantau bersamaan |
| `>=` untuk *terlambat* | `ReleasedAt > ChargeEligibleAt.AddMinutes(batas)` — bentuk yang sama dengan `sekarang > tenggat` daftar pantau | Rilis tepat 60 menit tidak terlambat |
| Memulai dari `CollectedAt` atau `RequestedAt` | Hanya `ChargeEligibleAt`; tanpa `ChargeEligibleAt` tidak dihitung | Harness: pengambilan ulang 40 menit; rilis tanpa waktu layak tidak masuk |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Selisih waktu dihitung di memori | Batas cito per jenis pemeriksaan dan *terlambat* dinilai per baris — kontrak mengizinkannya, dan jumlah baris dibatasi periode 366 hari (`VAL-149`). Proyeksinya lima kolom, tanpa data pasien |
| Baris per disiplin | **Dua** baris — *Cito* lalu *Rutin* — bagi setiap disiplin, termasuk yang belum dapat dihitung, supaya bentuk respons tetap. Contoh respons kontrak memperlihatkan PA dengan satu baris saja; kontrak tidak mewajibkan jumlah baris, dan dua baris menjaga layar dari kasus khusus |
| Cito tanpa rilis | `overdueCount` dan `withoutLimitCount` bernilai **0**, bukan kosong — kolom itu milik baris cito dan nilainya benar nol; `averageMinutes` kosong karena pembaginya nol |
| *Belum tergolong* | Dua baris bila ada rilis dari order berdisiplin kosong, hanya tanpa penyaring — sama dengan kedua laporan lain |
| Pemeriksaan atau order terhapus | Tidak dihitung |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — `GET /turnaround-time` sesuai `r37` 32.2-32.3. Seluruh endpoint baca grup laporan kini berdiri; unduhan milik `BE-LAB-86` |
| Database | **Baca saja**: satu kueri proyeksi, ditambah satu kueri batas bila ada rilis cito. Index `ChargeEligibleAt` dan `Urgency` sudah ada; penyaringnya pada `ReleasedAt` (index `BE-LAB-70`) |
| Keamanan/Auth | Kunci `LabOperationalReport : Read` yang sama — registri tetap 1576. Respons tanpa pasien, jenis pemeriksaan, maupun petugas |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Operational Report

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/turnaround-time` | Waktu penyelesaian dari wadah layak sampai hasil dirilis, per disiplin dan kesegeraan | `LabOperationalReport : Read` | `LabOperationalReportQuery` | `ApiResponse<LabTurnaroundTimeReportResponse>` |

Kode status sama dengan dua laporan lain: `200`, `400` (`VAL-147`, `VAL-148`, disiplin tak dikenal),
`401`, `403`, `422` (`VAL-149`).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` | **0 error, 230 warning**; nol dari berkas yang disentuh | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-85` | **21 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21, `81` 25/25, `82` 12/12 dan 13/13, `83` 25/25, `84` 15/15 | `PASS` | Harness `83`/`84` disesuaikan satu baris — konstruktor service laporan kini dua parameter |
| **SQL terhadap PostgreSQL** (sesi read-only) | `SELECT l0."Discipline", l."Urgency", l."ProcedureId", l."ChargeEligibleAt", l."ReleasedAt" … WHERE … "ReleasedAt" >= @start AND "ReleasedAt" <= @end AND "ChargeEligibleAt" IS NOT NULL …`; parameter September `2026-08-31T17:00Z`..`2026-09-30T16:59:59.9999999Z` | `PASS` | Log perintah Npgsql |
| Tinjauan kode — `LabValueBound` di `LabOperationalReportService` | **0** | `PASS` | `grep` |
| Startup, registri, Swagger | Registri **1576**; `turnaround-time` di Swagger (`StartDate`, `EndDate`, `Discipline`; `200`/`400`/`422`; tanpa deskripsi); nol galat tak dikenal | `PASS` | Log dan Swagger |

**Rincian HTTP** — superadmin kecuali disebut lain; nol penulisan.

| Panggilan | Hasil |
| --- | --- |
| `GET /turnaround-time?startDate=2026-09-01&endDate=2026-09-30` | `200` — PK cito dan rutin `releasedCount 0`, `averageMinutes null`, cito `overdueCount 0`/`withoutLimitCount 0`, rutin keduanya `null`; PA dan Mikrobiologi *belum dapat dihitung* |
| `discipline=AnatomicalPathology` | `200` — dua baris PA *belum dapat dihitung* |
| Tanpa `startDate` / terbalik / 367 hari / 366 hari | `400` / `400` / `422` / `200` |
| `discipline=Hematology` | `400` |
| Akun Kepala Instalasi (tanpa izin laporan) | `403` |
| Tanpa token | `401` |
| Regresi `/examination-count`, `/specimen-rejection`, `/lab-worklists/cito-overdue` | `200` ketiganya |

**Rincian harness** — tanggal September 2026, jam WIB.

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-252` dan sebaliknya** — Kalium X layak 08.00 belum dirilis; daftar pantau pukul 09.05 | Kalium X **tampil** terlambat 5 menit |
| … Kalium Y dirilis 08.55, order terbuka; daftar pantau pukul 09.05 | Kalium Y **tidak** tampil |
| **`AC-252`** — Kalium X lalu dirilis 09.10 | Terlambat di laporan; **hanya satu** terlambat pada PK cito |
| **Batas persis** — rilis tepat 60 menit | Tidak terlambat |
| Kalium Y di laporan (55 menit) | Tidak terlambat |
| **Cito tanpa batas** (120 menit) | `withoutLimitCount 1`; tidak terlambat; masuk rata-rata |
| **`INV-57` pengambilan ulang** — pemeriksaan lama gugur; pengganti layak 09.00, rilis 09.40 | 40 menit; yang gugur tidak ikut |
| Rata-rata PK cito | (70 + 55 + 60 + 120 + 40) / 5 = **69,0** |
| **Rutin** — 300 dan 30 menit | Rata-rata **165,0**; `overdueCount` dan `withoutLimitCount` kosong |
| Rilis tanpa `ChargeEligibleAt`; rilis 1 Okt 06.30 WIB | Tidak masuk September |
| Oktober — layak 30 Sep 22.00 WIB, rilis 1 Okt 06.30 WIB | Masuk Oktober, 510 menit, terlambat |
| **`ARCH-GAP-LAB-11` TAT** | PA dan Mikrobiologi `isCountable = false`, seluruh angka kosong |
| Order tanpa disiplin | *Belum tergolong* cito 45 menit |
| Urutan baris | PK cito, PK rutin, PA ×2, Mikrobiologi ×2, *Belum tergolong* ×2 |
| Jumlah kueri | **2** (pemeriksaan, batas); **1** bila tidak ada rilis cito |
| Periode tanpa rilis; penyaring PK; `VAL-147`/`148`/`149` | Angka kosong/0 sesuai kolom; dua baris PK; `400`, `400`, `422` |
| **Batas satu sumber — Kalium 60 → 90** | Sebelum: Kalium Z (65 menit, belum dirilis) tampil terlambat di daftar pantau. Sesudah: laporan menyebut Kalium X **tidak** terlambat (`overdueCount 0`) **dan** daftar pantau **tidak** lagi menampilkan Kalium Z |
| Privasi | Ruas: `AverageMinutes`, `Discipline`, `DisciplineName`, `IsCountable`, `NotCountableReason`, `OverdueCount`, `Period`, `ReleasedCount`, `Rows`, `Urgency`, `WithoutLimitCount` |

**Tidak dijalankan:**

- Angka berisi lewat HTTP — dev nol punya hasil dirilis. `AC-252` dibuktikan pada harness terhadap service
  daftar pantau yang sama dengan yang dipanggil endpoint `/cito-overdue`.
- *Batas satu sumber* lewat HTTP — mengubah batas cito berarti menulis data induk bersama.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-252` — 70 menit, terlambat | ✅ **Terpenuhi** pada harness | — |
| `AC-252` batas persis | ✅ **Terpenuhi** pada harness | — |
| `AC-252` *dan sebaliknya* | ✅ **Terpenuhi** pada harness — terhadap daftar pantau sungguhan | — |
| `AC-252` rutin; cito tanpa batas | ✅ **Terpenuhi** pada harness; bentuk kolom rutin juga lewat HTTP | — |
| `INV-57` pengambilan ulang | ✅ **Terpenuhi** pada harness | — |
| Batas cito satu sumber | ✅ **Terpenuhi** pada harness — 60 → 90 mengubah keduanya | — |
| `ARCH-GAP-LAB-11` TAT | ✅ **Terpenuhi** — harness dan HTTP | — |
| DoD — endpoint berjalan; `AC-252` dua arah; nol pembacaan batas di luar kebijakan; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** `averageMinutes` di JSON ditulis tanpa desimal bila bulat (`69`, bukan `69.0`) — nilainya sama; bentuk tampil milik layar dan CSV. **2.** Aturan `FirstOrDefault` baris batas umum yang ganda (catatan `BE-LAB-82`) ikut berlaku di sini — satu sumber, satu perilaku |
| Risiko tersisa | **Rendah.** Baca saja; periode dibatasi; batas dan perbandingan terbukti sama dengan daftar pantau |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M` `Controllers/LabOperationalReportController.cs`, `DTOs/LabOperationalReportDtos.cs`, `Services/LabOperationalReportService.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-84` (termasuk migration) yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-86` (unduhan CSV ketiga laporan dan pencatatannya; aksi `Export`) naik menjadi `SIAP DIKERJAKAN` — penutup `MVP-11a`. **2.** `BE-LAB-78` tetap `SIAP DIKERJAKAN` |
