# Laporan Perubahan Backend — `BE-RWI-217`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-217` |
| Judul | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak |
| Slice | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak |
| Roadmap | [Backend Bed Management](../../../roadmap/backend-roadmap-bed-management.md), kartu BE-RWI-217 |
| Trace | FR-BM-007, FR-BM-008, FR-BM-009; RWI-DEC-277, RWI-DEC-285, RWI-DEC-289, RWI-DEC-291; RWI-AC-405, RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424 |
| Contract version | 0.12.0 Bed Management; persetujuan eksplisit pengguna pada 2026-10-11 Asia/Jakarta untuk seluruh BE-RWI-208–221. Hash 24 masukan sesuai lock; status draft pada dokumen desain dipertahankan sebagai provenance. |
| Dependency | BE-RWI-216; BM-A tertutup untuk backend; dependency source tersedia, validasi runtime dependency masih sebagian. |
| Klasifikasi | HEAVY — repository0, berkas diperiksa2 (>20 masukan), berkas diubah2, logika2, API2, database1, auth1, workflow1; total11. Permintaan14task dipecah sesuai kartu roadmap, bukan implementasi satu EPIC tanpa batas. |
| Task mode | BACKEND; implementasi awal tanpa build/migration sesuai instruksi saat itu; sesi lanjutan build/migration diminta eksplisit pengguna. |
| Target tulis | NewQuilvianSystemBackend; source terkait kartu, laporan dan status/tautan bukti blueprint. |
| Model | Codex (GPT-6); tanpa delegasi sub-agent. |
| Commit backend saat dikerjakan | db85bede83c0ddbc9aaa412c576f2c3fd7447492; branch MHamzah, upstream origin/MHamzah; HEAD tidak dipindahkan. |
| Tanggal | 2026-10-11 Asia/Jakarta |
| Validasi terakhir | 2026-10-12 Asia/Jakarta — konfigurasi, build, SQL dan penerapan database |
| Status | 🟡 SEBAGIAN — 4/4 butir acceptance kartu ditelusuri ke source/rencana; 0/4 dinyatakan terbukti penuh. Build PASS (0 error, 10 warning); migration existing sesuai model dan SQL berhasil dibuat; database update PASS 2026-10-12 (296/296 applied, 0 pending); acceptance runtime NOT_RUN.  |

## Backend Governance Preflight

| Hal | Bukti |
| --- | --- |
| Aturan dibaca | AGENTS.md backend; docs/engineering/BACKEND_ENGINEERING_CONTRACT.md dan MODULE_OWNERSHIP_PREFIX_REGISTRY.md; global rules/backend TASK_RULES, TASK_CLASSIFICATION, CROSS_REPO_RULES, REVIEW_RULES, REPORT_TEMPLATE, API_RULES, DATABASE_RULES dan TEST_POLICY; aturan output dokumentasi. |
| Area / module / prefix | HealthServices / InPatientManagement / Inp ACTIVE; adapter HealthServices / MasterData / Mst ACTIVE; sub-modul blueprint episode-rawat-inap. |
| Keberlakuan | NEW CODE untuk tambahan; TOUCHED LEGACY untuk callsite/DTO/controller existing. Tidak ada LEGACY MIGRATION atau rename entity/tabel lama. |
| QBE yang berlaku | QBE-ENT-001; QBE-NAM-001; QBE-CFG-001; QBE-MOD-002; QBE-SVC-001; aturan canonical endpoint, permission, transaksi, DTO, audit dan validasi yang relevan; QBE-CODE-002/003 pada alokasi nomor master Bed. Pemeriksa otomatis hanya menilai subset ID yang diimplementasikan tooling. |
| Approval / batas | User: “saya setujui semua roadmap tersebut”, “tanpa melakukan dotnet build dan migration”, dan build/migration dilakukan mandiri. Instruksi user mengesampingkan kewajiban build/migration skill, tidak memberi bukti runtime. |
| Snapshot / perubahan existing | 24/24 hash input sesuai; HEAD BE/FE cocok lock. Dua manifest backend dan dua lockfile frontend sudah berubah sebelum pekerjaan; dipertahankan. Source frontend tidak ditulis. |

## 1. Masalah yang diperbaiki

Perubahan kamar, unit dan kelas ikut guard bed yang terdampak. Sebelum slice ini, implementasi existing belum menyediakan seluruh guard dan kontrak Bed Management0.12.0 yang disetujui. Kamar berisi tiga bed. Client hanya mengirim dua versi, atau satu bed direservasi setelah detail dimuat: server menolak perubahan parent, tanpa menonaktifkan sebagian bed.

## 2. Proses bisnis

Admin memuat parent dan seluruh versi child bed. Server membaca ulang set bed setelah lock. Jika ada child held atau set/versi berubah, seluruh mutation ditolak. Perubahan parent yang aman tetap mencatat audit dan menginvalidasi kesiapan child yang terdampak.

Contoh di atas merupakan ilustrasi aturan, bukan hasil pengujian atau data pasien nyata.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap, delivery lock dan24masukan desain/hash telah dibaca, termasuk backend architecture bagian14, kamus data bagian21, API13, state/validation/permission/integration matrix dan acceptance Bed Management. Source terkait ditelusuri pada tabel berikut.

### 3.2 Berkas yang berubah

Writer hierarchy memakai adapter transaksi yang sama, menginventaris seluruh child bed dan memeriksa AffectedBedVersions persis. Hierarchy lama/tujuan dikunci terurut; holder menghalangi perubahan availability. Detail parent menyediakan snapshot versi bed tanpa pasien. Perubahan kelas menyebabkan digest proof tidak cocok.

| Berkas | Peran pada slice |
| --- | --- |
| [BedMutationService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/BedMutationService.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [RoomMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [RoomController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/RoomController.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [RoomDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/RoomDtos.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [ServiceUnitMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/ServiceUnitMasterWriteService.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [ServiceUnitController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [ServiceUnitDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [PatientClassMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/PatientClassMasterWriteService.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [PatientClassController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/PatientClassController.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [PatientClassDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/PatientClassDtos.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [Program.cs](../../../../../../../Program.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |

### 3.3 Dampak kontrak API, database dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 12 endpoint utama dipetakan di bawah. Mutation terkait meminta Idempotency-Key dan versi; payload lama perlu penyesuaian consumer frontend. |
| Database | Tidak redesign master atau menambah bulk API. Adapter shared mengunci unit/kamar/kelas/bed existing. |
| Keamanan/Auth | Permission existing + assignment/unit server; OperationalOnly dan izin episode membatasi identitas; tanpa seed role, grant atau orang. |

Write core update/status/delete yang memengaruhi bed diekstrak ke RoomMasterWriteService, ServiceUnitMasterWriteService dan PatientClassMasterWriteService. Create parent existing tidak termasuk ownership endpoint slice ini dan dipertahankan. Validator/mapper writer dipertahankan dari source existing; perubahan hierarchy vs reserve/transfer dan perubahan rank perlu verifikasi PostgreSQL.

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Room

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/master-data/rooms/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `Room : Update` |
| `PATCH` | `/api/v1/health-services/master-data/rooms/{id}/status` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `Room : Update` |
| `DELETE` | `/api/v1/health-services/master-data/rooms/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `Room : Delete` |
| `GET` | `/api/v1/health-services/master-data/rooms/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `Room : Read` |

#### Health Services / Master Data / Service Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/master-data/service-units/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `ServiceUnit : Update` |
| `PATCH` | `/api/v1/health-services/master-data/service-units/{id}/status` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `ServiceUnit : Update` |
| `DELETE` | `/api/v1/health-services/master-data/service-units/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `ServiceUnit : Delete` |
| `GET` | `/api/v1/health-services/master-data/service-units/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `ServiceUnit : Read` |

#### Health Services / Master Data / Patient Class

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/master-data/patient-classes/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `PatientClass : Update` |
| `PATCH` | `/api/v1/health-services/master-data/patient-classes/{id}/status` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `PatientClass : Update` |
| `DELETE` | `/api/v1/health-services/master-data/patient-classes/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `PatientClass : Delete` |
| `GET` | `/api/v1/health-services/master-data/patient-classes/{id}` | Perubahan kamar, unit dan kelas ikut guard bed yang terdampak | `PatientClass : Read` |

## 5. Verifikasi

### Penerapan migration — 12 Oktober 2026 Asia/Jakarta

Pengguna menyediakan `appsettings.Development.json`, lalu menyetujui secara eksplisit penerapan kedua migration pada database Development tersebut. File konfigurasi diabaikan Git; nilainya tidak disalin ke laporan. Command dijalankan dengan environment Development dan koneksi dari file itu hanya pada proses command. Tidak menjalankan web runtime atau seeder aplikasi.

| Pemeriksaan atau command aktual | Hasil | Klasifikasi |
| --- | --- | --- |
| Parsing konfigurasi dan koneksi EF | JSON valid; DefaultConnection terbaca; koneksi database berhasil | PASS |
| `dotnet build QuilvianSystemBackend.sln --nologo --verbosity quiet` | Sukses; 0 error, 10 warning NU1902/NU1903 terkait ImageSharp 3.1.11; SDK 9.0.318 | PASS dengan warning |
| `dotnet ef migrations has-pending-model-changes --no-build --context ApplicationDbContext` | Tidak ada perubahan model di luar migration terakhir | PASS |
| `dotnet ef migrations list --no-build --context ApplicationDbContext --json` sebelum update | 296 migration dikenali; 294 applied, 2 pending: Finance dan Bed Management | PASS |
| Review SQL maju kedua migration | Finance: 4 kolom nullable. Bed Management: 4 tabel, 5 kolom nullable, 18 indeks. Tidak ada DROP TABLE, DROP COLUMN, TRUNCATE atau DELETE FROM pada SQL maju | PASS |
| `dotnet ef database update 20261011084625_AddInpatientBedManagementReadinessAndLifecycle --no-build --context ApplicationDbContext` | Exit 0; kedua migration diterapkan; keluaran `Done.` | PASS |
| `dotnet ef migrations list --no-build --context ApplicationDbContext --json` setelah update | 296/296 applied; 0 pending; kedua ID di bawah berstatus applied=true | PASS |
| Acceptance API, concurrency, rollback, backfill dan UAT | Belum dijalankan; schema terpasang belum membuktikan seluruh perilaku bisnis. BM-G04 tetap terbuka | NOT RUN |

Migration yang diterapkan:

- `20261006150000_AddInvoiceDateAndTermsToFinReceivableInvoiceBatch`: menambah InvoiceDate, DueDate, PaymentTermDays dan Note, semuanya nullable, pada FinReceivableInvoiceBatch. Tambahan scope Finance disetujui pengguna sebelum eksekusi.
- `20261011084625_AddInpatientBedManagementReadinessAndLifecycle`: fondasi schema Bed Management yang sebelumnya sudah disiapkan.

SQL dan log yang sudah disamarkan disimpan di TEMP. SHA256 SQL Finance: `061197f94021564ccd9b9a8d8d977d4117b746edbdf7fd99f635bd0dc223f0e7`; SHA256 SQL Bed Management: `5be716528d445f7fad87cb5bc80fe90715ece923861cc191d5394dc9bfebdfa7`. Source migration, Designer dan snapshot tidak diubah pada sesi ini. Contoh batas bukti: bed dengan raw status Available tetap memerlukan bukti kesiapan yang sah; migration tidak otomatis melakukan backfill atau menyatakan bed itu Ready.

Riwayat 11 Oktober 2026 di bawah dipertahankan; hambatan konfigurasi pada riwayat tersebut sudah tertutup melalui pemeriksaan dan update pada 12 Oktober 2026.

### Validasi lanjutan — 11 Oktober 2026 Asia/Jakarta

Pengguna meminta agent menjalankan build dan migration pada sesi lanjutan ini. Migration `20261011084625_AddInpatientBedManagementReadinessAndLifecycle`, Designer dan snapshot sudah tersedia sebelum sesi; ketiganya digunakan tanpa membuat migration ganda. Pemeriksaan EF offline memakai konfigurasi sementara hanya pada proses command; tidak menghubungi database dan tidak mengubah konfigurasi proyek.

| Pemeriksaan aktual | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --nologo --verbosity quiet` | Sukses; 0 error, 10 warning NU1902/NU1903 terkait ImageSharp 3.1.11; SDK 9.0.318 | PASS dengan warning |
| EF memakai konfigurasi default | Gagal membuat DbContext: `Connection string 'DefaultConnection' tidak ditemukan.` | EXISTING / ENVIRONMENT ISSUE — konfigurasi koneksi tidak tersedia |
| `dotnet ef migrations has-pending-model-changes --no-build --context ApplicationDbContext` (offline) | `No changes have been made to the model since the last migration.` | PASS |
| `dotnet ef migrations list --no-build --no-connect --context ApplicationDbContext` (offline) | Migration target terdeteksi; status applied/pending database tidak dapat ditentukan pada mode offline | PASS untuk discovery |
| `dotnet ef migrations script 20261009100000_BackfillLabExaminationUrgencyFromOrderedProcedure 20261011084625_AddInpatientBedManagementReadinessAndLifecycle --no-build --context ApplicationDbContext --output <TEMP>/quilvian-bed-management-20261011.sql` (offline) | SQL maju berhasil dibuat: 4 tabel, 5 kolom nullable, 18 indeks; tidak ada DROP TABLE, DROP COLUMN, TRUNCATE atau DELETE FROM; FK memakai RESTRICT | PASS untuk generation dan review SQL |
| `dotnet ef database update 20261011084625_AddInpatientBedManagementReadinessAndLifecycle --no-build --context ApplicationDbContext` | Belum dijalankan; perlu konfigurasi koneksi dan target database yang jelas | NOT RUN — environment |
| API, concurrency, rollback, backfill dan UAT | Tidak dijalankan pada sesi build/migration ini; BM-G04 tetap terbuka | NOT RUN |

SHA256 SQL: `5be716528d445f7fad87cb5bc80fe90715ece923861cc191d5394dc9bfebdfa7`. SQL disimpan sementara di TEMP; command di atas dapat menghasilkan ulang berkasnya. Migration ini tidak mengisi root readiness existing melalui backfill. Contoh: bed dengan raw status Available tetap memerlukan bukti kesiapan yang sah; pembuatan schema tidak membuktikan bahwa bed itu Ready.

### Riwayat verifikasi sebelum sesi lanjutan

Tabel berikut dipertahankan sebagai bukti historis. Untuk hasil build/migration terkini, gunakan tabel validasi lanjutan di atas.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Hash input lock | 24 cocok, 0 mismatch | PASS | SHA256 raw-byte diperiksa ulang sebelum menulis laporan. |
| Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly | 61 file; VIOLATION0, REVIEW0, INFO0 | PASS | Tooling repo; pemeriksaan akhir setelah alokasi kode Bed dipindah ke service. ReportOnly bukan enforcement semua aturan. |
| Parser C# + pemeriksaan initializer eksplisit | 61 file; 1029 kelas diindeks; 0 error terdeteksi | PASS | @ast-grep/lang-csharp melalui skrip di TEMP; bukan compiler/type checking penuh. |
| Pemetaan method/path dan metadata permission | 39/39 endpoint sesuai; 0 mismatch | PASS | Endpoint ownership lock dibandingkan deklarasi controller. Tidak menjalankan Swagger/API. |
| git diff --check | 0 whitespace error | PASS | Git read-only. Peringatan LF/CRLF adalah pengaturan working copy. |
| dotnet build / restore | Tidak dijalankan | NOT RUN | Build dilarang pengguna; tidak ada perubahan project/dependency aplikasi dan restore tidak dijalankan. |
| Migration / database / seed | Tidak dibuat atau dijalankan | NOT RUN | Diserahkan pengguna; folder Migrations dan snapshot existing tidak diubah. |
| API / PostgreSQL / concurrency / rollback / proses bisnis | Seluruh skenario runtime kartu belum dijalankan | NOT RUN | Tidak menjalankan aplikasi, koneksi database atau mutasi fixture. Source review bukan bukti eksekusi. |
| AUTOMATED TEST | NOT APPLICABLE — backend tidak memelihara project test otomatis | NOT APPLICABLE | rules/backend/TEST_POLICY.md; tidak membuat project, dependency atau runner uji aplikasi. |

Uji manual: **REQUIRED / NOT RUN**. BM-AT-405, BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 masih menunggu bukti runtime yang relevan. Keluaran pemeriksaan statis berada di TEMP mesin kerja; hasil numeriknya dicatat di laporan ini, tanpa membuat laporan task kedua.

## 6. Acceptance criteria dan Definition of Done

| Kriteria persis seperti roadmap | Status | Bukti dan batas |
| --- | --- | --- |
| Held child bed menolak disable/move/delete parent; snapshot AffectedBedVersions dapat dibaca tanpa holder/pasien. | Belum terbukti penuh — NOT_RUN | [BedMutationService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/BedMutationService.cs), [RoomMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs), [RoomController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/RoomController.cs), [RoomDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/RoomDtos.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Race parent update melawan reserve/transfer tidak melewati guard; perubahan rank membuat proof stale. | Belum terbukti penuh — NOT_RUN | [BedMutationService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/BedMutationService.cs), [RoomMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs), [RoomController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/RoomController.cs), [RoomDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/RoomDtos.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Client tidak dapat mengirim set bed parsial/extra atau memalsukan expected version. | Belum terbukti penuh — NOT_RUN | [BedMutationService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/BedMutationService.cs), [RoomMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs), [RoomController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/RoomController.cs), [RoomDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/RoomDtos.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Tidak menebak arah ClassLevel; tidak membuat data master atau operasi bulk baru. | Belum terbukti penuh — NOT_RUN | [BedMutationService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/BedMutationService.cs), [RoomMasterWriteService.cs](../../../../../../../Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs), [RoomController.cs](../../../../../../../Areas/HealthServices/MasterData/Controllers/RoomController.cs), [RoomDtos.cs](../../../../../../../Areas/HealthServices/MasterData/DTOs/RoomDtos.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |

Pemetaan ID acceptance: RWI-AC-405, RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424. ID tersebut scoped ke Bed Management/decision46, bukan approval atau hasil roadmap IGD historis yang memakai nomor serupa.

| Butir DoD | Status |
| --- | --- |
| Source dan keterlacakan | Tersedia; 4/4 butir kartu ditelusuri, 0/4 terbukti penuh. |
| Governance, pemeriksaan statis dan laporan tracked | Selesai untuk cakupan statis yang dicatat; bukan verifikasi runtime. |
| Build dan migration | Build PASS: 0 error, 10 warning. Migration/Designer/snapshot existing sesuai model; SQL maju tersedia. Kedua migration diterapkan pada 2026-10-12; 296/296 applied, 0 pending. Acceptance runtime tetap NOT_RUN. |
| Runtime, PostgreSQL, Swagger/proses bisnis/integrasi | Schema PostgreSQL terpasang; acceptance API, concurrency, rollback dan integrasi belum dijalankan; BM-G04 tetap terbuka. |
| Aktivasi BM-G01/02/03 | Tetap terbuka sesuai scope; proof kelas/SOP/assignment/grant nyata tidak dibuat agent. |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Peringatan LF/CRLF dari Git. Pemanggilan PowerShell QBE awal memerlukan ExecutionPolicy Bypass hanya pada proses; kebijakan sistem tidak diubah. Temuan QBE alokasi kode controller sudah diperbaiki. |
| Masalah yang diketahui | Write core update/status/delete yang memengaruhi bed diekstrak ke RoomMasterWriteService, ServiceUnitMasterWriteService dan PatientClassMasterWriteService. Create parent existing tidak termasuk ownership endpoint slice ini dan dipertahankan. Validator/mapper writer dipertahankan dari source existing; perubahan hierarchy vs reserve/transfer dan perubahan rank perlu verifikasi PostgreSQL. |
| Risiko tersisa | Kompilasi lulus. Schema sudah diterapkan dan riwayat EF diverifikasi (0 pending). Acceptance alur bisnis belum terbukti penuh. Consumer lama tanpa field/key baru perlu disesuaikan; transaksi/locking/privacy/integrasi perlu PostgreSQL runtime. |
| Perubahan sampingan | Tidak menulis frontend/project/solution/package/CI/migration. Perubahan pengguna existing dipertahankan; tidak stage/commit/push/deploy. |
| Interupsi implementasi awal (historis) | Sesi dihentikan pengguna lalu dilanjutkan dengan “continue”; status/diff dan artefak validasi diperiksa sebelum melanjutkan. Tidak ada build, migration atau mutation database yang diulang. |
| Status Git | `git status --short --untracked-files=all`: flag `M`/`??` untuk source terkait dicatat di tabel3.2; laporan ini `??`. Perubahan uncommitted. Dua manifest backend dan empat dokumen planning sudah berubah/untracked sebelum tugas; frontend tetap dua lockfile existing. |
| Langkah berikutnya | Lanjutkan verifikasi acceptance API/manual PostgreSQL, concurrency, rollback dan backfill sesuai kartu; tutup gate aktivasi dengan bukti nyata. Build dan penerapan migration sudah selesai.  |
