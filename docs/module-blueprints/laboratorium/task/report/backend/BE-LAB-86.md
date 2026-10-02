# Laporan Perubahan Backend — `BE-LAB-86`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-86` |
| Judul | Unduhan CSV ketiga laporan dan pencatatannya |
| Slice | Gelombang `MVP-11a` — **penutup** |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6an.5** |
| Trace | `FR-17.7`, `FR-17.8`, `FR-17.9`; `LAB-DEC-160`; A7.9; 23.10 butir 1, 2, 7 |
| Contract version | `LAB-API-v1` **`r37`** 32.2-32.3; `LAB-PERM-v1` **revision 12** 14.2, 14.3, 14.5 — **`approved` 2026-09-28** |
| Dependency | `BE-LAB-83` ✅, `BE-LAB-84` ✅, `BE-LAB-85` ✅ |
| Klasifikasi | `MEDIUM` — skor 9: repository 0, berkas diperiksa 1 (±10), berkas diubah 2 (3), logika bisnis 1, kontrak API **2** (tiga endpoint berkas), database 0, keamanan/auth **2** (aksi izin baru; satu-satunya jalur data keluar sistem), UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7ff35b8c` (branch `yoga`), di atas perubahan `BE-LAB-84`/`85` yang belum ter-commit |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — tiga unduhan berjalan **lewat HTTP terhadap PostgreSQL**: `200 text/csv`, tiga byte pertama `EF BB BF`, nama berkas memuat jenis dan periode; berkas penolakan memuat angka sungguhan dev *Patologi Klinik;5;1;20,0*. **Tiga unduhan → tepat tiga baris log** `LabOperationalReport.Export` dengan jenis, periode, disiplin, jumlah baris, dan pelaku — **nol** data pasien; membuka laporan dan penolakan periode **tidak** menambah log. Registri **1576 → 1577** (aksi `Export`, `AccessType` `Read`). Harness **19/19**; regresi utuh; build 0 error; **nol pustaka baru**; nol migration |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `LabReportCsvWriter`, `LabCsvFile`, tiga endpoint unduh. `TOUCHED LEGACY` — `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (laporan dibentuk service yang sama dengan layar; controller hanya menyusun berkas dan mencatat, tanpa context), `QBE-API-001` (kode status dan pesan sama dengan layar), `QBE-VAL-001` (`ResolvePeriod` bersama), `QBE-PERM-001` (`[AccessAction("Export")]` berpasangan `[AccessPermission("LabOperationalReport", "Export")]`), `QBE-AUD-001` (log unduhan lewat pencatat aplikasi, bukan tabel audit data). **Tidak berlaku:** `QBE-ENT-*`/`QBE-DB-*` (nol entity, nol migration), `QBE-DTO-*` (respons berkas) |
| Governance yang dibaca | Sama dengan `BE-LAB-83`..`85`; ditambah `Services/Logging/LoggerService.cs` (perilaku pencatat, lihat 3.2) |

---

## 1. Masalah yang diperbaiki

Ketiga laporan hanya dapat dibaca di layar. Kepala instalasi dan manajemen perlu membawanya ke rapat
dan mengolahnya di Excel — dan karena mengunduh membawa data keluar sistem, setiap unduhan harus tercatat
dan hanya terbuka bagi yang diberi izin mengunduh.

---

## 2. Proses bisnis

**Berkas yang terbuka benar di Excel berbahasa Indonesia** — pemisah titik koma, desimal koma, huruf
Indonesia terbaca (UTF-8 dengan BOM). Contoh sungguhan dari database dev:

```text
Periode;1 September 2026 - 30 September 2026
Disiplin;Wadah diputuskan;Wadah ditolak;Angka penolakan (%)
Patologi Klinik;5;1;20,0
Patologi Anatomi;0;0;
Mikrobiologi;0;0;

Disiplin;Kode alasan;Alasan penolakan;Jumlah
Patologi Klinik;INSUFFICIENT_QUANTITY;Jumlah sampel tidak mencukupi;1
```

**Isi berkas = isi layar.** Berkas dibentuk dari respons laporan yang sama, bukan dari kueri kedua.

**Izin mengunduh terpisah dari izin melihat** (23.10 butir 2). Pemegang `LabOperationalReport : Read`
saja dapat membuka ketiga laporan tetapi **tidak** dapat mengunduh.

**Setiap unduhan tercatat** — contoh sungguhan: *"specimen-rejection — 2026-09-01..2026-09-30 — seluruh
disiplin — 3 baris"*, beserta pelaku dan waktunya. Membuka laporan di layar tidak dicatat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6an.5 | Cakupan, tiga jebakan, verifikasi, DoD |
| `contracts/api-contract.md` 32.2-32.3 | Tiga endpoint unduh, bentuk CSV |
| `contracts/permission-audit-matrix.md` 14.2, 14.3, 14.5 | Aksi `Export`, pemetaan endpoint, isi log dan yang dilarang |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-28 | `AC-253` `Read` tanpa `Export`, format CSV, log unduhan, log baca, privasi |
| `Services/Logging/LoggerService.cs` | Cara pencatat menulis — lihat 3.2 |
| `Services/LabDisciplineSettingService.cs` | Pola `AuditAsync` dan `LogCategory` Laboratorium |
| `Attributes/AccessActionAttribute.cs`, `AccessPermissionAttribute.cs` | Metadata akses |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabReportCsvWriter.cs` | **Baru.** Tiga `Write` — satu per laporan — tanpa pustaka baru: BOM UTF-8, `;`, desimal koma (`0,0`), baris periode dengan nama bulan Indonesia, judul kolom Bahasa Indonesia, nilai kosong ditulis kosong, escape `;`/tanda kutip/pindah baris, baris `CRLF`. `LabCsvFile(Content, RowCount)` |
| `Controllers/LabOperationalReportController.cs` | Menerima `LabReportCsvWriter` dan `LoggerService`. Tiga `GET …/export` dengan `[AccessAction("Export", …, AccessType = AccessTypes.Read, SortOrder = 2)]` dan `[AccessPermission("LabOperationalReport", "Export")]`. Satu jalur `UnduhAsync`: bentuk laporan lewat fungsi layar → tulis CSV → **satu** `AuditAsync` sesudah berkas jadi → `File(…, "text/csv", "<jenis>_<awal>_<akhir>.csv")` |
| `Program.cs` | `AddScoped<LabReportCsvWriter>()` |

**Temuan pada pencatat platform — dan bagaimana ditangani.** `LoggerService.WriteAsync` **tidak
menyerialisasi** objek `data` ke log. Objek itu hanya dibaca untuk menimpa ruas tertentu — `Id`/`UserId`,
`Name`/`UserName`, `Path`, `Ip`, `Email`. Payload `{ jenis, periode, … }` saja akan menghasilkan log tanpa
satu pun isi yang diwajibkan 14.5. Karena itu:

- isi wajib ditulis di **teks pesan**, persis bentuk contoh 14.5;
- payload tetap dikirim (jenis, awal, akhir, disiplin, jumlah baris) dengan nama ruas yang **tidak** menimpa
  pelaku maupun alamat permintaan.

Harness dan HTTP membuktikan pelaku di log adalah pengguna yang mengunduh.

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Berkas dari kueri terpisah | `UnduhAsync` memanggil fungsi service yang sama dengan endpoint layar | Harness: setiap baris berkas sama dengan respons layar |
| `Read` pada endpoint unduh | Ketiganya `LabOperationalReport : Export` | Refleksi: 3 unduh `Export`, 4 baca `Read`; `SysActionAccess` memuat dua aksi terpisah |
| Seluruh respons sebagai payload log | Log hanya jenis, periode, disiplin, jumlah baris | Log sungguhan: nol kode alasan, nama alasan, barcode, nilai |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Jumlah baris di log | Baris **tabel utama** laporan (satu per disiplin, atau disiplin × kesegeraan) — sesuai contoh 14.5, yang menulis *3 baris* untuk laporan penolakan seluruh disiplin |
| Rincian di berkas | Bagian kedua sesudah satu baris kosong: rincian per jenis pemeriksaan (jumlah) dan per alasan (penolakan). Kontrak hanya memperlihatkan tabel utama; rincian ikut supaya berkas memuat isi layar selengkapnya |
| Kesegeraan di berkas | *Cito* / *Rutin* — judul dan isi Bahasa Indonesia |
| Bulan di baris periode | Nama bulan Indonesia ditulis sendiri, tidak bergantung pada data budaya sistem operasi server |
| Pencatatan penolakan | Unduhan yang ditolak penjaga periode **tidak** dicatat — tidak ada data yang keluar |
| Swagger unduhan | Respons `200` tampil tanpa jenis konten `text/csv` — `ProducesResponseType` tidak dapat memaksakannya tanpa mengubah jenis konten respons galat. Keterangan kosmetik |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — tiga endpoint `r37` 32.2. Grup laporan kini lengkap tujuh endpoint |
| Database | **Baca saja** — kueri yang sama dengan laporan layar; nol migration |
| Keamanan/Auth | Aksi baru `LabOperationalReport : Export` (registri 1577). **Nol kebijakan diberikan** — pemberian kepada kepala instalasi dan manajemen milik langkah rilis `MVP-11c`; **larangan PERM 14.6**: jangan disalin dari pemegang `LabExamination : Read` |
| Pencatatan | Satu baris `LabOperationalReport.Export` per unduhan berhasil |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Operational Report

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/examination-count/export` | Unduh laporan jumlah pemeriksaan | `LabOperationalReport : Export` | `LabOperationalReportQuery` | `text/csv` — `laporan-jumlah-pemeriksaan_<awal>_<akhir>.csv` |
| `GET` | `/specimen-rejection/export` | Unduh laporan penolakan wadah | `LabOperationalReport : Export` | `LabOperationalReportQuery` | `text/csv` — `laporan-penolakan-wadah_<awal>_<akhir>.csv` |
| `GET` | `/turnaround-time/export` | Unduh laporan waktu penyelesaian | `LabOperationalReport : Export` | `LabOperationalReportQuery` | `text/csv` — `laporan-waktu-penyelesaian_<awal>_<akhir>.csv` |

Galat sama dengan laporan layar: `400` (`VAL-147`, `VAL-148`, disiplin tak dikenal), `401`, `403`
(tanpa `Export` — termasuk pemegang `Read` saja), `422` (`VAL-149`), dalam bentuk `ApiResponse` JSON.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` | **0 error, 230 warning**; nol dari berkas yang disentuh | `PASS` | Keluaran build |
| `.csproj` | **Tidak berubah** — nol pustaka baru | `PASS` | `git diff` |
| Harness `BE-LAB-86` | **19 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `77` 21/21, `81` 25/25, `82` 12/12 dan 13/13, `83` 25/25, `84` 15/15, `85` 21/21 | `PASS` | Harness yang sama |
| **`PermissionRegistryValidator`** | Aplikasi menyala; registri **1576 → 1577**; `SysActionAccess`: `LabOperationalReport` — `Export` (`AccessType` `Read`, *"Mengunduh laporan operasional laboratorium sebagai berkas CSV"*) dan `Read` | `PASS` | Log startup; kueri baca-saja |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |
| Log aplikasi | Nol galat tak dikenal | `PASS` | `app.log` |

**Rincian HTTP** — superadmin kecuali disebut lain.

| Panggilan | Hasil |
| --- | --- |
| Ketiga `…/export?startDate=2026-09-01&endDate=2026-09-30` | `200`; `Content-Type: text/csv`; `Content-Disposition: attachment; filename=laporan-…_2026-09-01_2026-09-30.csv`; tiga byte pertama **`ef bb bf`** |
| Isi berkas penolakan | *Patologi Klinik;5;1;20,0* — angka sungguhan dev, cocok dengan laporan layar `BE-LAB-84` |
| Isi berkas jumlah dan waktu penyelesaian | Baris periode, judul Indonesia, PA/Mikrobiologi dengan keterangan *belum tersedia*, kolom kosong ditulis kosong |
| **Log** — sebelum / sesudah tiga unduhan | 0 → **3** baris `LabOperationalReport.Export` |
| Isi ketiga log | *examination-count — 2026-09-01..2026-09-30 — seluruh disiplin — 3 baris*; *specimen-rejection — … — 3 baris*; *turnaround-time — … — 6 baris*; `UserName=superadmin`, `UserId` pengguna, `Path` endpoint unduh; **nol** kode/nama alasan, barcode, nomor rekam medis, nilai hasil, isi berkas |
| Membuka ketiga laporan + metadata | Log tetap **3** |
| Unduh tanpa `endDate` / terbalik / 367 hari | `400` / `400` / `422` — log tetap **3** |
| Ketiga unduhan, akun Kepala Instalasi (tanpa izin laporan) | `403` ×3 |
| Tanpa token | `401` |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| Berkas penolakan 1-7 September (400/12) | `text/csv`, nama berkas tepat, BOM `EF-BB-BF` |
| Baris periode; judul; angka | *Periode;1 September 2026 - 7 September 2026*; judul Indonesia; *Patologi Klinik;400;12;3,0* |
| Nilai kosong | PA dan Mikrobiologi tanpa keputusan: angka penolakan **kosong** |
| Escape | Nama alasan `Label "rusak"; sobek` → `"Label ""rusak""; sobek"` |
| Isi = layar | Setiap baris disiplin sama dengan respons layar; ringkasan, total terhitung, dan rincian jumlah pemeriksaan sama; waktu penyelesaian *Cito;1;70,0;1;0* dan *Rutin;1;31,0;;;* |
| **Log unduhan** — tiga unduhan | **Tepat tiga** baris; isi 14.5; pelaku dari pencatat, tidak tertimpa payload |
| Log — larangan | Nol barcode, nomor order, nama alasan, nilai hasil, nama pemeriksaan, isi berkas |
| **Log baca** — tiga laporan + metadata | **Nol** baris |
| Penjaga periode pada unduhan | `400`, `400`, `422` — dan **tidak** dicatat |
| **`AC-253` `Read` tanpa `Export`** | Refleksi atribut: ketiga unduhan `LabOperationalReport:Export` (`AccessAction` `Export`/`Read`); keempat baca `LabOperationalReport:Read` — pemegang `Read` saja tidak memenuhi aksi `Export` |

**Tidak dijalankan:**

- **Membuka berkas di Excel berbahasa Indonesia** — tidak ada Excel pada lingkungan agen. Syarat yang
  membuatnya terbaca benar dibuktikan pada byte berkas: BOM, `;`, desimal koma. Mohon dicoba sekali oleh
  kepala instalasi saat langkah rilis `MVP-11c`.
- **`AC-253` dengan akun pemegang `Read` tanpa `Export`** — tidak ada kebijakan laporan di dev, dan
  memberinya berarti menulis kebijakan akses bersama. Pemisahan dibuktikan pada registri (`SysActionAccess`)
  dan atribut; penolakan `403` lewat HTTP bagi akun tanpa izin laporan.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Format CSV — BOM, periode, `;`, `3,0`, judul Indonesia | ✅ **Terpenuhi** — harness dan HTTP (angka sungguhan `20,0`) | — |
| Log unduhan — tiga baris berisi jenis, periode, disiplin, jumlah, pelaku; nol data pasien | ✅ **Terpenuhi** — harness dan HTTP | — |
| Log baca — nol baris | ✅ **Terpenuhi** — harness dan HTTP | — |
| Privasi | ✅ **Terpenuhi** — berkas memuat isi laporan yang sama (tanpa identitas pasien); log tanpa isi berkas | — |
| `AC-253` `Read` tanpa `Export` | ✅ **Terpenuhi pada registri dan atribut**; persona sungguhan belum | 5 |
| `PermissionRegistryValidator` menerima `Export` | ✅ **Terpenuhi** — 1577 | — |
| Verifikasi roadmap — dibuka di Excel berbahasa Indonesia | ⚠ **Belum dijalankan** — syarat bytenya terbukti | 5 |
| DoD — tiga unduhan; aksi `Export` terdaftar; log 14.5; nol pustaka baru; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** `LoggerService` tidak menyerialisasi payload — temuan platform, milik pemilik `Services/Logging`. Setiap modul yang mengandalkan payload untuk isi log audit kehilangan isinya tanpa galat. **2.** Swagger unduhan tidak menampilkan jenis konten `text/csv` pada `200` |
| Risiko tersisa | **Sedang-rendah.** Jalur data keluar sistem, tetapi tertutup secara bawaan (nol kebijakan) dan setiap unduhan tercatat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` `Services/LabReportCsvWriter.cs`, laporan ini; ` M` `Controllers/LabOperationalReportController.cs`, `Program.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-84`/`85` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** **`MVP-11a` selesai pada kode** (`BE-LAB-82`..`86`). **2.** Langkah rilis `MVP-11c` (6an.7): deploy, lalu admin memberi `Read` dan `Export` kepada kepala instalasi; langkah 2 (*manajemen*) tetap `BLOCKED` sampai jabatannya ditetapkan. **3.** Task backend Lab yang siap: `BE-LAB-78` (validasi dan rilis Mikrobiologi) |
