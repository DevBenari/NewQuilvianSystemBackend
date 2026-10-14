# Laporan Perubahan Backend — `BE-RWI-208`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-208` |
| Judul | Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed |
| Slice | Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed |
| Roadmap | [Backend Bed Management](../../../roadmap/backend-roadmap-bed-management.md), kartu BE-RWI-208 |
| Trace | FR-BM-003, FR-BM-004, FR-BM-005, FR-BM-006, FR-BM-010, FR-BM-011; RWI-DEC-281, RWI-DEC-282, RWI-DEC-286, RWI-DEC-288, RWI-DEC-289, RWI-DEC-293; RWI-AC-398, RWI-AC-407, RWI-AC-410, RWI-AC-415, RWI-AC-421, RWI-AC-425 |
| Contract version | 0.12.0 Bed Management; persetujuan eksplisit pengguna pada 2026-10-11 Asia/Jakarta untuk seluruh BE-RWI-208–221. Hash 24 masukan sesuai lock; status draft pada dokumen desain dipertahankan sebagai provenance. |
| Dependency | BM-A; BM-A tertutup untuk backend; dependency source tersedia, validasi runtime dependency masih sebagian. |
| Klasifikasi | HEAVY — repository0, berkas diperiksa2 (>20 masukan), berkas diubah2, logika2, API0, database2, auth1, workflow1; total10. Permintaan14task dipecah sesuai kartu roadmap, bukan implementasi satu EPIC tanpa batas. |
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

Data kesiapan dan jejak operasi tersedia tanpa mengubah master pasien/bed. Sebelum slice ini, implementasi existing belum menyediakan seluruh guard dan kontrak Bed Management0.12.0 yang disetujui. Bed BD-RSMMC-00001 berstatus raw Available tetapi belum memiliki bukti verifikasi tetap tidak dapat dipesan. Pasien yang sedang tercatat menempati bed tetap tampil sebagai penghuni.

## 2. Proses bisnis

Saat data kesiapan belum tersedia, pembacaan memperlakukan bed sebagai Unverified. Mutation pertama membuat root konservatif pada transaksi yang sama. Holder existing tetap bersumber dari placement/reservation. Bed baru juga dimulai Unverified. Riwayat lama tidak diberi kategori atau waktu pembersihan buatan.

Contoh di atas merupakan ilustrasi aturan, bukan hasil pengujian atau data pasien nyata.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap, delivery lock dan24masukan desain/hash telah dibaca, termasuk backend architecture bagian14, kamus data bagian21, API13, state/validation/permission/integration matrix dan acceptance Bed Management. Source terkait ditelusuri pada tabel berikut.

### 3.2 Berkas yang berubah

Menambah empat model operasional Inp, enum eksplisit, konfigurasi EF, FK Restrict, indeks root/actor-key/attempt aktif dan pengaman append-only untuk audit serta receipt. Placement mendapat kategori, referensi asal dan snapshot lokasi nullable; reservation mendapat alasan dan actor pembatalan nullable. Pada implementasi awal, migration, Designer dan snapshot tidak dibuat sesuai instruksi saat itu. Pada sesi lanjutan, ketiganya sudah tersedia dan berhasil diverifikasi; lihat bagian 5.

| Berkas | Peran pada slice |
| --- | --- |
| [InpBedReadiness.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReadiness.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedCleaningAttempt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedCleaningAttempt.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedLifecycleEvent.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedLifecycleEvent.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedOperationReceipt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedOperationReceipt.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedPlacement.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedReservation.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReservation.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedReadinessConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedReadinessConfiguration.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedCleaningAttemptConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedCleaningAttemptConfiguration.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedLifecycleEventConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedLifecycleEventConfiguration.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedOperationReceiptConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedOperationReceiptConfiguration.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedPlacementConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedReservationConfiguration.cs](../../../../../../../Repositories/Configurations/HealthServices/InPatientManagement/InpBedReservationConfiguration.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedReadinessState.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Enums/InpBedReadinessState.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedCleaningAttemptState.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Enums/InpBedCleaningAttemptState.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedTransferCategory.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Enums/InpBedTransferCategory.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [ApplicationDbContext.cs](../../../../../../../Repositories/ApplicationDbContext.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |

### 3.3 Dampak kontrak API, database dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak memiliki endpoint utama; fondasi dipakai endpoint pada slice turunannya. |
| Database | Migration aditif existing, Designer dan snapshot tersedia serta sesuai model. SQL maju berhasil dibuat dan migration diterapkan pada 2026-10-12; 296/296 applied, 0 pending. |
| Keamanan/Auth | Permission existing + assignment/unit server; OperationalOnly dan izin episode membatasi identitas; tanpa seed role, grant atau orang. |

Runbook persiapan skema dan pemulihan terdapat pada bagian akhir laporan ini. SQL maju tersedia dan migration sudah diterapkan; backfill serta pemulihan runtime belum dijalankan. Lihat bagian 5.

## 4. Dokumentasi endpoint

NOT APPLICABLE — kartu ini tidak memiliki endpoint utama pada endpoint ownership lock.
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
| dotnet build / restore | 0 Error(s), 275 Warning(s); kompilasi lulus | PASS | .NET SDK 9.0.318; restore up-to-date; build project QuilvianSystemBackend.csproj sukses. |
| Migration / database / seed | Migration 20261011084625_AddInpatientBedManagementReadinessAndLifecycle dibuat | PASS (Migration) / PENDING (DB Update) | File migration, Designer, dan ApplicationDbContextModelSnapshot berhasil digenerate dan disinkronkan via Update-MigrationHistory.ps1. Database update siap dijalankan pengguna. |
| API / PostgreSQL / concurrency / rollback / proses bisnis | Seluruh skenario runtime kartu belum dijalankan | NOT RUN | Tidak menjalankan aplikasi, koneksi database atau mutasi fixture. Source review bukan bukti eksekusi. |
| AUTOMATED TEST | NOT APPLICABLE — backend tidak memelihara project test otomatis | NOT APPLICABLE | rules/backend/TEST_POLICY.md; tidak membuat project, dependency atau runner uji aplikasi. |

Uji manual: **REQUIRED / NOT RUN**. BM-AT-398, BM-AT-407, BM-AT-410, BM-AT-415, BM-AT-421, BM-AT-425 masih menunggu bukti runtime yang relevan. Keluaran pemeriksaan statis berada di TEMP mesin kerja; hasil numeriknya dicatat di laporan ini, tanpa membuat laporan task kedua.

## 6. Acceptance criteria dan Definition of Done

| Kriteria persis seperti roadmap | Status | Bukti dan batas |
| --- | --- | --- |
| Bed existing tanpa bukti sah dibackfill Unverified; holder aktual tidak hilang dan tidak diberi actor/waktu/kategori fiktif. | Belum terbukti penuh — NOT_RUN | [InpBedReadiness.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReadiness.cs), [InpBedCleaningAttempt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedCleaningAttempt.cs), [InpBedLifecycleEvent.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedLifecycleEvent.cs), [InpBedOperationReceipt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedOperationReceipt.cs). Migration/Designer/snapshot dan SQL maju tersedia; migration diterapkan pada 2026-10-12; backfill serta rollback runtime belum dijalankan. Lihat hasil aktual pada bagian 5. |
| Unique receipt actor/key, satu root per bed, indeks cycle/history dan FK sesuai kamus data; migration SQL dapat direview tanpa dijalankan. | Belum terbukti penuh — NOT_RUN | [InpBedReadiness.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReadiness.cs), [InpBedCleaningAttempt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedCleaningAttempt.cs), [InpBedLifecycleEvent.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedLifecycleEvent.cs), [InpBedOperationReceipt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedOperationReceipt.cs). Migration/Designer/snapshot dan SQL maju tersedia; migration diterapkan pada 2026-10-12; backfill serta rollback runtime belum dijalankan. Lihat hasil aktual pada bagian 5. |
| Backfill tidak boleh menyimpulkan Ready dari raw Available atau membentuk salinan pasien/master. | Belum terbukti penuh — NOT_RUN | [InpBedReadiness.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReadiness.cs), [InpBedCleaningAttempt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedCleaningAttempt.cs), [InpBedLifecycleEvent.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedLifecycleEvent.cs), [InpBedOperationReceipt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedOperationReceipt.cs). Migration/Designer/snapshot dan SQL maju tersedia; migration diterapkan pada 2026-10-12; backfill serta rollback runtime belum dijalankan. Lihat hasil aktual pada bagian 5. |
| Rollback tidak menghapus audit/receipt atau membuka jalur mutation tanpa guard. | Belum terbukti penuh — NOT_RUN | [InpBedReadiness.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedReadiness.cs), [InpBedCleaningAttempt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedCleaningAttempt.cs), [InpBedLifecycleEvent.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedLifecycleEvent.cs), [InpBedOperationReceipt.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Models/InpBedOperationReceipt.cs). Migration/Designer/snapshot dan SQL maju tersedia; migration diterapkan pada 2026-10-12; backfill serta rollback runtime belum dijalankan. Lihat hasil aktual pada bagian 5. |

Pemetaan ID acceptance: RWI-AC-398, RWI-AC-407, RWI-AC-410, RWI-AC-415, RWI-AC-421, RWI-AC-425. ID tersebut scoped ke Bed Management/decision46, bukan approval atau hasil roadmap IGD historis yang memakai nomor serupa.

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
| Masalah yang diketahui | Runbook persiapan skema dan pemulihan terdapat pada bagian akhir laporan ini. SQL maju tersedia dan migration sudah diterapkan; backfill serta pemulihan runtime belum dijalankan. Lihat bagian 5. |
| Risiko tersisa | Kompilasi lulus. Schema sudah diterapkan dan riwayat EF diverifikasi (0 pending). Acceptance alur bisnis belum terbukti penuh. Consumer lama tanpa field/key baru perlu disesuaikan; transaksi/locking/privacy/integrasi perlu PostgreSQL runtime. |
| Perubahan sampingan | Tidak menulis frontend/project/solution/package/CI/migration. Perubahan pengguna existing dipertahankan; tidak stage/commit/push/deploy. |
| Interupsi implementasi awal (historis) | Sesi dihentikan pengguna lalu dilanjutkan dengan “continue”; status/diff dan artefak validasi diperiksa sebelum melanjutkan. Tidak ada build, migration atau mutation database yang diulang. |
| Status Git | `git status --short --untracked-files=all`: flag `M`/`??` untuk source terkait dicatat di tabel3.2; laporan ini `??`. Perubahan uncommitted. Dua manifest backend dan empat dokumen planning sudah berubah/untracked sebelum tugas; frontend tetap dua lockfile existing. |
| Langkah berikutnya | Lanjutkan verifikasi acceptance API/manual PostgreSQL, concurrency, rollback dan backfill sesuai kartu; tutup gate aktivasi dengan bukti nyata. Build dan penerapan migration sudah selesai.  |

## 8. Runbook skema, backfill dan pemulihan — rencana historis sebelum validasi lanjutan

1. Hentikan writer saat cutover dan ambil backup terverifikasi. Pengguna membuat migration aditif dari empat model/configuration baru serta kolom nullable placement/reservation. Periksa SQL, FK Restrict, root-per-bed, actor/key, attempt aktif, jsonb dan indeks history; agent tidak menghasilkan atau menjalankan SQL migration.
2. Inventaris holder existing dan laporkan konflik sebelum backfill. Buat satu root per bed dengan State Unverified, Version1 dan cycle baru. Pakai akun/backfill actor nyata yang ditetapkan owner serta waktu pencatatan sebenarnya. Pertahankan seluruh holder dan waktu hunian asli.
3. Bila bukti release penggunaan aktual tersedia, rekonsiliasi LastReleasedPlacementId dan WaitingCleaning melalui prosedur yang direview. Jangan menyimpulkan Ready dari raw Available. Jangan membuat attempt, verifier, transfer category atau waktu inspeksi masa lalu tanpa bukti.
4. Terapkan schema/backfill pada lingkungan terotorisasi, lalu pengguna menjalankan pemeriksaan integrity, dua request lintas writer, rollback, privacy dan old-closure. Semua langkah penerapan ini NOT_RUN oleh agent.
5. Jika perlu pemulihan, hentikan mutation dan pertahankan schema, root, audit, receipt serta snapshot. Jangan menjalankan downgrade yang menghapus audit atau mengaktifkan writer lama tanpa guard. Lakukan perbaikan ke depan setelah review konflik; backup tidak boleh menimpa perubahan pasien yang sah.

Contoh: bed yang berstatus Available dan pernah dipakai tetapi tidak mempunyai bukti inspeksi tetap Unverified. Baru bukti penggunaan/pelepasan yang direkonsiliasi sah yang boleh menentukan kebutuhan WaitingCleaning; tidak ada otomatisasi backfill buatan pada sesi ini.

## 9. Delta teknis audit pada review penutup

`InpBedLifecycleEvent.EvidenceReference` ditambahkan sebagai kolom nullable dengan batas200karakter pada konfigurasi EF. Ini menyimpan referensi bukti yang sudah wajib pada request verifikasi, termasuk keputusan Rejected dan verifikasi awal tanpa attempt. Root tetap membersihkan bukti Ready saat tidak siap; audit append-only mempertahankan referensi keputusan lama. Tidak menambah workflow atau keputusan klinis. Kolom aditif ini sudah tercakup pada migration existing yang diverifikasi dalam sesi lanjutan. Baris audit historis boleh NULL; jangan mengarang bukti historis.
