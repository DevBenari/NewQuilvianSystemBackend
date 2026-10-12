# Laporan Perubahan Backend — `BE-RWI-209`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-209` |
| Judul | Hak dan gate operasi dievaluasi dari akun serta scope yang sah |
| Slice | Hak dan gate operasi dievaluasi dari akun serta scope yang sah |
| Roadmap | [Backend Bed Management](../../../roadmap/backend-roadmap-bed-management.md), kartu BE-RWI-209 |
| Trace | FR-BM-001, FR-BM-002, FR-BM-005, FR-BM-006, FR-BM-009, FR-BM-011; RWI-DEC-278, RWI-DEC-280, RWI-DEC-287, RWI-DEC-291, RWI-DEC-293; RWI-AC-416, RWI-AC-417, RWI-AC-423, RWI-AC-425 |
| Contract version | 0.12.0 Bed Management; persetujuan eksplisit pengguna pada 2026-10-11 Asia/Jakarta untuk seluruh BE-RWI-208–221. Hash 24 masukan sesuai lock; status draft pada dokumen desain dipertahankan sebagai provenance. |
| Dependency | BM-A; BM-A tertutup untuk backend; dependency source tersedia, validasi runtime dependency masih sebagian. |
| Klasifikasi | HEAVY — repository0, berkas diperiksa2 (>20 masukan), berkas diubah1, logika2, API0, database1, auth2, workflow1; total9. Permintaan14task dipecah sesuai kartu roadmap, bukan implementasi satu EPIC tanpa batas. |
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

Hak dan gate operasi dievaluasi dari akun serta scope yang sah. Sebelum slice ini, implementasi existing belum menyediakan seluruh guard dan kontrak Bed Management0.12.0 yang disetujui. Petugas yang diberi Read pada unit A tidak menerima bed unit B. Petugas OperationalOnly melihat lokasi dan status, tetapi tidak menerima identitas pasien atau riwayat hunian.

## 2. Proses bisnis

Admin memberikan hak melalui manajemen akses existing, lalu menyediakan assignment unit yang dapat ditelusuri. Server mengecek akun aktif, permission, unit/bed/episode dan gate tindakan. Tanpa bukti SOP atau verifier, tindakan pembersihan/verifikasi ditolak. Nama role atau isian browser tidak memberi kewenangan.

Contoh di atas merupakan ilustrasi aturan, bukan hasil pengujian atau data pasien nyata.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap, delivery lock dan24masukan desain/hash telah dibaca, termasuk backend architecture bagian14, kamus data bagian21, API13, state/validation/permission/integration matrix dan acceptance Bed Management. Source terkait ditelusuri pada tabel berikut.

### 3.2 Berkas yang berubah

Binding BedManagement menggunakan typed options dan validasi startup. AccessPermissionService existing tetap menentukan grant; assignment pengguna/unit/operasi menentukan scope server. OperationalOnly membatasi identitas, history, transfer dan verifikasi. Workflow default nonaktif, daftar assignment kosong dan bukti kelas kosong.

| Berkas | Peran pada slice |
| --- | --- |
| [BedManagementOptions.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [BedManagementOptionsValidator.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedAccessService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedOperationException.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationException.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [InpBedOperationFilter.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationFilter.cs) | `??` — diubah/ditambah; bukti source untuk slice ini. |
| [Program.cs](../../../../../../../Program.cs) | `M` — diubah/ditambah; bukti source untuk slice ini. |

### 3.3 Dampak kontrak API, database dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak memiliki endpoint utama; fondasi dipakai endpoint pada slice turunannya. |
| Database | Tidak menambah tabel akun, role, assignment atau seed. |
| Keamanan/Auth | Permission existing + assignment/unit server; OperationalOnly dan izin episode membatasi identitas; tanpa seed role, grant atau orang. |

Delta teknis: opsi ReservationExpiryActorUserId dan ReservationExpiryAssignmentReference memungkinkan audit expiry memakai akun sistem nyata yang ditetapkan admin. Keduanya tidak diisi agent; expiry yang membutuhkan actor tanpa bukti ditolak. Detail aktivasi dan digest kelas ada di akhir laporan.

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
| dotnet build / restore | Tidak dijalankan | NOT RUN | Build dilarang pengguna; tidak ada perubahan project/dependency aplikasi dan restore tidak dijalankan. |
| Migration / database / seed | Tidak dibuat atau dijalankan | NOT RUN | Diserahkan pengguna; folder Migrations dan snapshot existing tidak diubah. |
| API / PostgreSQL / concurrency / rollback / proses bisnis | Seluruh skenario runtime kartu belum dijalankan | NOT RUN | Tidak menjalankan aplikasi, koneksi database atau mutasi fixture. Source review bukan bukti eksekusi. |
| AUTOMATED TEST | NOT APPLICABLE — backend tidak memelihara project test otomatis | NOT APPLICABLE | rules/backend/TEST_POLICY.md; tidak membuat project, dependency atau runner uji aplikasi. |

Uji manual: **REQUIRED / NOT RUN**. BM-AT-416, BM-AT-417, BM-AT-423, BM-AT-425 masih menunggu bukti runtime yang relevan. Keluaran pemeriksaan statis berada di TEMP mesin kerja; hasil numeriknya dicatat di laporan ini, tanpa membuat laporan task kedua.

## 6. Acceptance criteria dan Definition of Done

| Kriteria persis seperti roadmap | Status | Bukti dan batas |
| --- | --- | --- |
| Unknown grant/scope ditolak dan OperationalOnly tidak mendapat identitas atau akses riwayat pasien. | Belum terbukti penuh — NOT_RUN | [BedManagementOptions.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs), [BedManagementOptionsValidator.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs), [InpBedAccessService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs), [InpBedOperationException.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationException.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Gate spesifik: tanpa proof urutan lintas kelas ditahan, same valid class ID tetap dapat dibandingkan; tanpa SOP/assignment aksi cleaning ditahan. | Belum terbukti penuh — NOT_RUN | [BedManagementOptions.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs), [BedManagementOptionsValidator.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs), [InpBedAccessService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs), [InpBedOperationException.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationException.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Mengubah nama role di UI tidak memberi hak server. | Belum terbukti penuh — NOT_RUN | [BedManagementOptions.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs), [BedManagementOptionsValidator.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs), [InpBedAccessService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs), [InpBedOperationException.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationException.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |
| Default kosong/tidak terverifikasi tidak dianggap bukti aktivasi; SOP/arah ClassLevel tidak dikarang. | Belum terbukti penuh — NOT_RUN | [BedManagementOptions.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs), [BedManagementOptionsValidator.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs), [InpBedAccessService.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs), [InpBedOperationException.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOperationException.cs). Alur dan guard ditelusuri secara statis; skenario positif/negatif belum dijalankan. |

Pemetaan ID acceptance: RWI-AC-416, RWI-AC-417, RWI-AC-423, RWI-AC-425. ID tersebut scoped ke Bed Management/decision46, bukan approval atau hasil roadmap IGD historis yang memakai nomor serupa.

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
| Masalah yang diketahui | Delta teknis: opsi ReservationExpiryActorUserId dan ReservationExpiryAssignmentReference memungkinkan audit expiry memakai akun sistem nyata yang ditetapkan admin. Keduanya tidak diisi agent; expiry yang membutuhkan actor tanpa bukti ditolak. Detail aktivasi dan digest kelas ada di akhir laporan. |
| Risiko tersisa | Kompilasi lulus. Schema sudah diterapkan dan riwayat EF diverifikasi (0 pending). Acceptance alur bisnis belum terbukti penuh. Consumer lama tanpa field/key baru perlu disesuaikan; transaksi/locking/privacy/integrasi perlu PostgreSQL runtime. |
| Perubahan sampingan | Tidak menulis frontend/project/solution/package/CI/migration. Perubahan pengguna existing dipertahankan; tidak stage/commit/push/deploy. |
| Interupsi implementasi awal (historis) | Sesi dihentikan pengguna lalu dilanjutkan dengan “continue”; status/diff dan artefak validasi diperiksa sebelum melanjutkan. Tidak ada build, migration atau mutation database yang diulang. |
| Status Git | `git status --short --untracked-files=all`: flag `M`/`??` untuk source terkait dicatat di tabel3.2; laporan ini `??`. Perubahan uncommitted. Dua manifest backend dan empat dokumen planning sudah berubah/untracked sebelum tugas; frontend tetap dua lockfile existing. |
| Langkah berikutnya | Lanjutkan verifikasi acceptance API/manual PostgreSQL, concurrency, rollback dan backfill sesuai kartu; tutup gate aktivasi dengan bukti nyata. Build dan penerapan migration sudah selesai.  |

## 8. Konfigurasi aktivasi yang harus dipenuhi pemilik

Section `BedManagement` terikat di Program.cs. Default `EnableWorkflow=false`, `UnitAssignments=[]` dan `ClassOrder` tanpa bukti menahan tindakan. Setiap assignment memuat UserId aktif, ServiceUnitId, daftar Operations yang diizinkan validator, AssignmentReference nyata, serta SopReference untuk pekerjaan cleaning. VerifyReadiness memerlukan IsReadinessVerifier=true dan OperationalOnly=false. Hak tetap harus dicentang pada manajemen akses existing; config tidak memberi grant.

Untuk expiry, admin menunjuk ReservationExpiryActorUserId berupa akun sistem nyata yang aktif dan ReservationExpiryAssignmentReference. Tanpa kedua bukti, mutation expiry ditahan. Tidak ada seed actor atau ID contoh produksi.

Untuk lintas kelas, MasterData menyediakan Direction (1 HigherValueIsHigherGrade atau2 LowerValueIsHigherGrade), EvidenceReference, EquivalencePairs resmi dan ApprovedSourceDigest SHA256. Source digest memakai kelas aktif/tidak dihapus: tiap baris `Id:D|ClassLevel`, diurutkan Guid; LF di antara baris. Tambahkan LF lalu pasangan ekuivalensi `A:D|B:D` yang masing-masing dinormalisasi A<B, diurutkan A lalu B dan dipisahkan LF. Tidak ada trailing LF setelah pasangan terakhir; bila daftar pasangan kosong, bagian kelas diikuti satu LF. Hash UTF-8 ditulis hex lowercase. Perubahan class/rank/ekuivalensi membuat proof lama tidak cocok. Harga/nama kelas tidak berpengaruh.

Contoh: class X dan Y bernilai0 tetap Unknown untuk beda ID. Class X ke class X aktif menghasilkan SameGrade. Untuk X ke Y yang berbeda, hanya arah dan digest data yang disahkan pemilik yang membuka UpGrade/DownGrade/SameGrade sesuai aturan; contoh ini tidak menetapkan arah rumah sakit.
