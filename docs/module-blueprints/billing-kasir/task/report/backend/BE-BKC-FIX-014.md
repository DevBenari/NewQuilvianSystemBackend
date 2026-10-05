# Laporan Perubahan Backend — `BE-BKC-FIX-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-014` |
| Judul | Mempercepat daftar Running Invoice (get all) dan detail invoice (get by id) agar tetap cepat saat data sudah banyak |
| Slice | Perbaikan performa ad-hoc, di luar penomoran roadmap asli — mengikuti pola `BE-BKC-FIX-XXX` yang sudah dipakai modul ini (`BE-BKC-FIX-001`–`013`) |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pengguna pada sesi 5 Oktober 2026 ("Mengapa query get data running invoice … agak lama?", dilanjutkan "lakukan semuanya") |
| Trace | Kontrak `BIL-API-1.0` (`GET /invoices` dan `GET /invoices/{id}`); `API_RULES.md`; `DATABASE_RULES.md` |
| Contract version | `BIL-API-1.0` tidak naik versi. Bentuk request dan response tidak berubah. Satu perubahan isi: `calculationVersions` pada `GET /{id}` kini berisi **satu** versi (yang sedang berlaku), bukan seluruh riwayat — lihat bagian 3.3 |
| Dependency | Tidak ada |
| Klasifikasi | `HEAVY` — skor 9: repository 0; berkas diperiksa >20 → 2; berkas diubah 8 → 1; logika bisnis sedang → 1; kontrak API (isi respons berubah) → 2; database (kolom, indeks, migration) → 2; keamanan/auth 0; UI/workflow satu halaman terdampak tanpa perubahan kode frontend → 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` saja. Frontend hanya dibaca sebagai rujukan konsumen |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `1a3b9299d4fe0fbe47a30124fa1d71e8b21e4ce6` (branch `Yasmina`, upstream `origin/Yasmina`, working tree bersih sebelum task) |
| Tanggal | 2026-10-05 |
| Status | 🟡 **KODE DAN MIGRATION SELESAI DITULIS, VALIDASI BELUM DIJALANKAN.** `dotnet build` `NOT RUN` dan migration belum diterapkan ke database mana pun. Task ini belum boleh disebut selesai (`✅`) sebelum build dan pemeriksaan model migration lulus — lihat bagian 5 dan 7 |

---

## 1. Masalah yang diperbaiki

Layar Running Invoice dan Menu Pembayaran terasa lambat, dan akan makin lambat seiring data bertambah.

**Detail invoice (`GET /{id}`).**

- Satu query memuat tiga daftar sekaligus: item tagihan, penerapan diskon, dan seluruh riwayat versi kalkulasi. Database mengembalikan hasil perkalian ketiganya.
  > **Ilustrasi:** invoice dengan 50 item, 3 diskon, dan 10 versi kalkulasi menghasilkan 50 × 3 × 10 = 1.500 baris.
- Setiap versi kalkulasi membawa `BreakdownSnapshot` (JSON besar, isinya rincian per item). Salinan JSON itu ikut terulang di setiap baris hasil perkalian, lalu seluruhnya dibaca dan diurai satu per satu.
- Riwayat versi bertambah setiap kali tagihan dihitung ulang, jadi invoice rawat inap yang berjalan lama menjadi yang paling berat — padahal layar kasir hanya memakai satu versi: yang sedang berlaku.
- Ringkasan pasien/kunjungan dimuat lewat sekitar delapan query berurutan, masing-masing satu kali bolak-balik ke database.

**Daftar invoice (`GET /`).**

- Tabel invoice tidak punya indeks untuk status, urutan terbaru, maupun tanggal. Setiap halaman mengurutkan seluruh hasil saringan, jadi biayanya mengikuti total data, bukan ukuran halaman.
- Saringan tanggal memakai `CASE` yang menggabungkan kolom dari dua tabel (kunjungan dan invoice), sehingga tidak ada indeks yang bisa dipakai. Saringan bawaan layar ini (periode "Hari ini" + status `OPEN`) selalu melewati jalur tersebut.
- Pencarian memakai `UPPER(kolom) LIKE '%teks%'`. Wildcard di depan membuat indeks `FullName` dan `MedicalRecordNumber` yang sudah ada tidak terpakai.
- Total data (`COUNT`) dihitung dengan menjalankan seluruh gabungan tabel setiap kali, walaupun tidak ada pencarian.
- Jumlah dan nilai kotor item dihitung dengan dua subquery per baris halaman, masing-masing memindai item invoice yang sama.

Semua temuan di atas berasal dari pembacaan kode. Belum ada pengukuran waktu atau `EXPLAIN ANALYZE` pada data nyata (lihat bagian 5).

---

## 2. Proses bisnis

Tidak ada aturan bisnis yang berubah. Yang berubah hanya cara data dicari dan dibaca.

**Membuka daftar Running Invoice.**

1. Kasir membuka layar. Frontend mengirim `status=OPEN` dan `period=today` (bawaan layar), ditambah ketikan pencarian bila ada.
2. Backend menyaring invoice menurut status, jenis layanan, dan rentang tanggal kunjungan. Rentang tanggal kini dibaca dari kolom `VisitDate` milik invoice sendiri.
3. Bila kasir mengetik pencarian, backend mencari nomor invoice, nama pasien, dan nomor rekam medis dengan `ILIKE` (tidak peka huruf besar-kecil, mengandung teks).
4. Backend menghitung total data. Tanpa pencarian, total dihitung dari tabel invoice saja.
5. Backend mengambil satu halaman (urut terbaru), lalu melengkapinya dengan nama pasien, poliklinik, penjamin, serta jumlah dan nilai kotor item aktif.

**Aturan `VisitDate` dan contoh.**

`VisitDate` adalah salinan tanggal kunjungan pada saat invoice dibuat. Bila kunjungannya tidak terbaca, dipakai `InvoiceDate`, lalu `CreateDateTime` — aturan yang sama dengan query lama.

> **Contoh:** pasien datang 5 Oktober 2026 pukul 09.10 WIB, invoice dibuat pukul 09.15. `VisitDate` = 09.10. Saringan "Hari ini" menyertakannya. Invoice lama tanpa kunjungan yang terbaca, dengan `InvoiceDate` kosong dan `CreateDateTime` 3 Oktober, memakai 3 Oktober dan tidak masuk "Hari ini".

`EncounterDate` tidak pernah diubah setelah kunjungan dibuat (pencarian `\.EncounterDate =` pada seluruh `Areas/` tidak menemukan penulisan ulang), sehingga salinan ini tidak bisa menyimpang dari sumbernya.

**Jalur tidak normal.**

| Kejadian | Hasil |
| --- | --- |
| Pencarian berisi `%`, `_`, atau `\` | Diperlakukan sebagai huruf biasa. Mencari `50%` hanya menemukan teks yang benar-benar memuat `50%` |
| Invoice tidak punya item aktif | `runningGrossAmount` = 0 dan `activeItemCount` = 0, sama seperti sebelumnya |
| Invoice tidak punya kunjungan yang terbaca | Tetap muncul di daftar dengan nama pasien kosong, sama seperti sebelumnya |
| Id invoice tidak ada | `GET /{id}` mengembalikan 404 "Invoice Billing tidak ditemukan." |
| Versi kalkulasi yang berlaku tidak ada barisnya | Versi dengan nomor tertinggi yang dikirim — sama dengan cadangan yang dipakai frontend |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingInvoicesController.cs`, `BillingInvoiceService.cs`, `BillingInvoiceDtos.cs`, `BillingCalculationService.cs` (`MapResponse`, `DeserializeBreakdown`, `PreviewCalculationAsync`), `BillingPayerEditService.cs`, `BilInvoice.cs`, `BilCalculationVersion.cs`, `BilInvoiceConfiguration.cs`, `BilInvoiceItemConfiguration.cs`, `BilCalculationVersionConfiguration.cs`, `MstPatientConfiguration.cs`, `RegPatientEncounterConfiguration.cs`, `Program.cs` (pengaturan `UseNpgsql`), `QuilvianSystemBackend.csproj` (aturan kompilasi migration), `Migrations/*` (pola migration, Designer, snapshot, `MigrationMetadata.g.*`), `docs/engineering/*`, `AGENTS.md`, `CLAUDE.md`. Frontend (rujukan konsumen): `use-billing-invoices.js`, `use-billing-invoice-detail.js`, `use-menu-pembayaran.js`, `billing-invoice-calculation-breakdown.js`, `billing-invoice-slice.jsx`, `data-filter.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs` | Properti baru `VisitDate` (`DateTime`) beserta penjelasan |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceConfiguration.cs` | Mapping `VisitDate` (`timestamp with time zone`, bawaan `CURRENT_TIMESTAMP`); indeks `IX_BilInvoice_Status_CreateDateTime` (menurun pada `CreateDateTime`, parsial `IsDelete = false`) dan `IX_BilInvoice_VisitDate` (parsial) |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` | `GetPagedAsync`: saringan tanggal pada `VisitDate`, pencarian `ILIKE` dengan escape, `COUNT` tanpa join bila tanpa pencarian, jumlah/nilai item lewat satu `GROUP BY`. `GetDetailAsync`: `AsSplitQuery`, hanya versi kalkulasi yang berlaku. `LoadPatientSummaryAsync`: dari sekitar 8 query menjadi 2. `UpsertChargeAsync`: mengisi `VisitDate` saat invoice dibuat. Helper `BuildContainsPattern` |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs` | Komentar pada `InvoiceDetailResponse.CalculationVersions` tentang isinya per jalur |
| `Migrations/20261005090000_OptimizeRunningInvoiceQueries.cs` | **Baru.** Kolom `VisitDate` + pengisian baris lama, dan dua indeks btree `BilInvoice`. **Tidak memerlukan ekstensi.** `Down` membalik semuanya |
| `Migrations/20261005090000_OptimizeRunningInvoiceQueries.Designer.cs` | **Baru.** Model target = snapshot terkini |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Properti `VisitDate` dan dua indeks `BilInvoice` (12 baris ditambahkan, tidak ada yang dihapus) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /{id}`: field `calculationVersions` tetap ada, isinya satu elemen (versi yang berlaku). Konsumen frontend memilih versi lewat `currentCalculationVersion` (`use-menu-pembayaran.js:75`, `billing-invoice-calculation-breakdown.js:124`), sehingga tidak perlu diubah. Jalur tulis yang memakai `InvoiceDetailResponse` yang sama (`from-source`, `void`, `catalog-charges`, `other-charges`) **tidak diubah** dan tetap membawa seluruh riwayat. `GET /` tidak berubah bentuk |
| Database | Satu migration, `OptimizeRunningInvoiceQueries`: kolom `BilInvoice.VisitDate` (NOT NULL, bawaan `CURRENT_TIMESTAMP`) dan dua indeks btree. **Tidak memerlukan ekstensi.** Indeks trigram (`pg_trgm`) **tidak dibuat** atas keputusan pengguna (bagian 7). **Belum berhasil diterapkan ke database mana pun**: dua percobaan pengguna sebelumnya memakai versi migration yang masih memasang `pg_trgm` (bagian 5) |
| Keamanan/Auth | `NOT APPLICABLE`. Atribut `[AccessAction]`/`[AccessPermission]` pada controller tidak berubah. Tidak ada hak akses baru, tidak ada `IsInRole`/`UserType` |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar Running Invoice berhalaman. Lebih cepat; bentuk respons tidak berubah | `BillingInvoice : Read` |
| `GET` | `/{id}` | Detail satu invoice. Lebih cepat; `calculationVersions` hanya memuat versi yang berlaku | `BillingInvoice : Read` |

Kode `404` muncul pada `GET /{id}` bila invoice tidak ada atau sudah ditandai terhapus. Kode `403` bila pengguna tidak punya hak `BillingInvoice : Read`.

Delta kontrak terhadap `BIL-API-1.0`: hanya isi `calculationVersions` pada `GET /{id}` (bagian 3.3). Tidak ada endpoint baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` | Tidak dijalankan | `NOT RUN` | Tidak ada paket atau berkas proyek yang diubah |
| `dotnet build` | **Tidak dijalankan.** Pengguna meminta agar `dotnet build` dan perintah berat lain hanya dijalankan bila diminta. Akibatnya **tidak ada bukti bahwa kode ini dapat dikompilasi** | `NOT RUN` | — |
| `dotnet ef migrations has-pending-model-changes` | Tidak dijalankan. Perintah ini yang membuktikan snapshot sama dengan model | `NOT RUN` | — |
| Review diff `git diff` pada 6 berkas termodifikasi | Hanya perubahan yang dimaksud; tidak ada perubahan di luar scope | `PASS` | `git status --short`: 6 berkas `M`, 2 berkas `??` (migration dan Designer) |
| Penyuntingan snapshot lewat skrip | Setiap penanda ditemukan tepat sekali pada blok entity yang benar sebelum menulis; `git diff` snapshot = 24 baris tambahan, tanpa baris dihapus | `PASS` | Skrip menolak menulis bila penanda tidak unik |
| Designer baru vs snapshot | Isi `BuildTargetModel` = isi snapshot baru; header memuat `[Migration("20261005090000_OptimizeRunningInvoiceQueries")]` | `PASS` (pembacaan) | Pemeriksaan awal dan akhir berkas |
| Kecocokan nama indeks dan filter di konfigurasi, migration, snapshot | Cocok: `IX_BilInvoice_Status_CreateDateTime`, `IX_BilInvoice_VisitDate`; filter `"IsDelete" = false` | `PASS` (pembacaan) | Perbandingan tiga berkas |
| Titik pembuatan `BilInvoice` | Hanya satu (`UpsertChargeAsync`), sudah mengisi `VisitDate` | `PASS` (pencarian) | Pencarian `new BilInvoice` dan `BilInvoices.Add` |
| `EncounterDate` tidak diubah setelah dibuat | Tidak ada penulisan ulang pada `Areas/` | `PASS` (pencarian) | Pencarian `\.EncounterDate\s*=` |
| Verifikasi proses bisnis (pencarian, saringan tanggal, versi kalkulasi) | Ditelusuri pada kode; belum diuji dengan data | `PASS` (pembacaan) | Bagian 2 |
| `EXPLAIN (ANALYZE, BUFFERS)` sebelum/sesudah pada data mendekati produksi | Tidak dijalankan. **Klaim "lebih cepat" belum terukur** | `NOT RUN` | — |
| Percobaan 1: `dotnet ef database update --no-build --configuration Release` (dijalankan **pengguna**, bukan agent), migration versi pertama yang masih satu berkas | **Gagal pada perintah pertama**: `CREATE EXTENSION IF NOT EXISTS pg_trgm;` → `42501: permission denied for language c`. Tidak ada perubahan tertulis (transaksi dibatalkan, migration tidak masuk riwayat). Log juga menunjukkan EF menemukan migration `20261005090000_OptimizeRunningInvoiceQueries`, artinya build Release yang memuat Designer-nya ada; ini kesimpulan dari log, bukan hasil `dotnet build` yang saya jalankan | `NEW ERROR` (migration mengasumsikan akun berhak memasang ekstensi) | Log pengguna 5 Oktober 2026 |
| Percobaan 2: perintah yang sama setelah migration diberi pemeriksaan ekstensi | **Gagal sesuai rancangan**: `P0001` dengan pesan "Ekstensi pg_trgm belum terpasang dan akun migration tidak berhak memasangnya…". Tidak ada perubahan tertulis. `pg_trgm` memang belum terpasang pada database itu | `EXISTING / ENVIRONMENT ISSUE` (prasyarat server, bukan cacat kode) | Log pengguna 5 Oktober 2026 |
| Migration trigram dihapus atas permintaan pengguna (tidak punya akses superuser untuk memasang `pg_trgm`) | Dua berkas migration trigram (yang belum pernah diterapkan) dihapus. Indeks trigram dibuang dari `BilInvoiceConfiguration` dan `MstPatientConfiguration` (berkas ini kembali identik dengan `HEAD`) serta dari snapshot. Skrip memverifikasi isi Designer migration yang tersisa **identik** dengan snapshot, dan diff snapshot terhadap `HEAD` kini hanya 12 baris tambahan: kolom `VisitDate` dan dua indeks btree | `PASS` (pembacaan dan skrip) | Skrip bantu di scratchpad sesi; `git diff --numstat` |
| Penerapan migration yang tersisa | Belum dijalankan | `NOT RUN` | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada database dengan migration terpasang pada sesi ini.

**Tidak dijalankan:** build, restore, pemeriksaan model migration, `EXPLAIN ANALYZE`, penerapan migration, dan uji manual. Alasannya: instruksi pengguna agar perintah berat hanya dijalankan atas permintaan, ditambah batas wewenang eksekusi database pada `AGENTS.md`.

---

## 6. Acceptance criteria dan Definition of Done

Task ini ad-hoc dan tidak punya acceptance criteria roadmap. Kriteria di bawah diambil dari cakupan yang diminta pengguna.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Detail: `AsSplitQuery` | Terpenuhi di kode | `GetDetailAsync` |
| Detail: hanya versi kalkulasi yang berlaku | Terpenuhi di kode | `GetDetailAsync`; field tetap ada, isi satu elemen |
| Detail: ringkasan pasien jadi 1-2 query | Terpenuhi di kode | `LoadPatientSummaryAsync` (2 query, sebelumnya sekitar 8) |
| List: indeks `(Status, CreateDateTime menurun)` parsial | Terpenuhi di kode dan migration | Konfigurasi, migration, snapshot |
| List: indeks trigram | **Tidak dikerjakan atas keputusan pengguna (5 Oktober 2026)**: memasang `pg_trgm` memerlukan akun superuser yang belum dimiliki pengguna. Query tetap memakai `ILIKE` dengan escape, sehingga indeks trigram bisa ditambahkan kelak tanpa mengubah query. Pencarian teks **belum dipercepat** | `GetPagedAsync`, `BuildContainsPattern` |
| List: snapshot `VisitDate` dengan pengisian baris lama dan indeks | Terpenuhi di kode dan migration | Model, konfigurasi, `UpsertChargeAsync`, migration |
| List: `COUNT` tanpa join bila tanpa pencarian | Terpenuhi di kode | `GetPagedAsync` |
| List: `Sum`/`Count` item digabung | Terpenuhi di kode | `GROUP BY` pada pass 2 |
| Kode dapat dikompilasi (`dotnet build`) | **Belum terpenuhi** | `NOT RUN` |
| Snapshot sama dengan model (`has-pending-model-changes`) | **Belum terpenuhi** | `NOT RUN` |
| Migration diterapkan dan indeks terpakai (`EXPLAIN ANALYZE`) | **Belum terpenuhi** | `NOT RUN` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Migration, Designer, dan snapshot **disusun tangan**, bukan dihasilkan `dotnet ef migrations add`, karena build tidak dijalankan. Repo ini pernah gagal start akibat snapshot tidak cocok dengan model (`PendingModelChangesWarning`, lihat komentar pada `20260930120000_AddMasterDailyNursingAction.cs`). **Jalankan `dotnet ef migrations has-pending-model-changes` sebelum migration dipakai.** Bila melaporkan perubahan tertunda, hapus migration beserta Designer-nya (dua berkas) dan kembalikan snapshot, lalu hasilkan ulang dengan `dotnet ef migrations add` |
| Masalah yang diketahui | **Pencarian teks belum dipercepat** (nomor invoice, nama pasien, nomor RM): indeks trigram tidak dibuat karena `pg_trgm` belum bisa dipasang. Biayanya sama seperti sebelumnya, yaitu memindai himpunan yang tersisa setelah saringan status dan tanggal. Karena saringan itu kini ber-indeks, pencarian dengan periode "Hari ini" + `OPEN` diperkirakan memindai himpunan kecil (belum diukur), sedangkan pencarian pada semua periode dan semua status tetap akan melambat saat data besar. Cara menambahkannya kelak, bila ada akses superuser: DBA menjalankan `CREATE EXTENSION pg_trgm;` pada database aplikasi, lalu migration baru berisi tiga indeks GIN (`gin_trgm_ops`) pada `BilInvoice.InvoiceNumber`, `MstPatient.FullName`, dan `MstPatient.MedicalRecordNumber`; dapat disusun ulang atas permintaan. Indeks trigram baru terpakai bila ketikan minimal 3 karakter |
| Risiko tersisa | (1) `CREATE INDEX` tanpa `CONCURRENTLY`: penulisan ke `BilInvoice` tertahan selama kedua indeks dibangun; jalankan di luar jam layanan bila tabel invoice sudah besar. (2) `UPDATE` pengisian `VisitDate` menulis ulang seluruh baris `BilInvoice` satu kali. (3) Ringkasan pasien kini memakai subquery skalar; hasilnya sama, tetapi belum dibandingkan pada data nyata. (4) Riwayat: dua percobaan pengguna gagal karena akun migration tidak berhak memasang `pg_trgm` (`42501`, lalu pesan pemeriksaan `P0001`). Migration yang tersisa tidak lagi bergantung pada ekstensi |
| Temuan di luar scope (tidak diubah) | (a) Jalur tulis (`LoadInvoiceByItemAsync`, `VoidItemAsync`, tambah biaya) masih memuat seluruh versi kalkulasi dengan pola `Include` yang sama; mereka mengembalikan seluruh riwayat. (b) Tiga pencarian lain di `BillingInvoiceService` masih memakai `ToUpper().Contains` — riwayat pembayaran, ikhtisar kasir, dan opsi kunjungan aktif (`GetPaymentHistoryAsync`, `GetCashierOverviewAsync`, `GetActiveEncounterOptionsAsync`). Bila indeks trigram kelak dibuat, ketiganya bisa dialihkan ke `BuildContainsPattern` + `ILike`. (c) Dua Designer terbaru (`20261003090000`, `20261004090000`) memuat entity `CliDoctorCertificate` yang **tidak ada** di snapshot maupun di source mana pun (hanya di Designer; sisa model dari branch lain). Tidak berdampak pada task ini, tetapi perlu dibersihkan pemilik migration. (d) Kini ada 64 Designer penuh di `Migrations/` (termasuk satu dari task ini); `tooling/migrations/Update-MigrationHistory.ps1` belum dijalankan untuk mengarsipkan yang lebih lama |
| Kesesuaian QBE | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing`, prefix `Bil` (registry: `ACTIVE`). Keberlakuan: `TOUCHED LEGACY` (tidak ada entity baru, jadi `QBE-MOD-002`/`QBE-MOD-003` tidak terpicu). Aturan yang berlaku: `QBE-CFG-002` (indeks pada configuration yang disentuh), `QBE-API-001` dan `QBE-DTO-001` (kontrak `ApiResponse`/`PagedResult` dan DTO tidak berubah; entity EF tidak diekspos), `QBE-PERM-001` (metadata Access tidak berubah). `QBE-ENT-003` (larangan field persisted yang murni kebutuhan presentasi): `VisitDate` adalah kunci saringan dan indeks, bukan kebutuhan tampilan, tetapi ini keputusan desain yang **perlu dikonfirmasi pemilik modul** karena menambah kolom salinan. `QBE-DB-001`/`QBE-DB-002` tidak berlaku (bukan `LEGACY MIGRATION`) |
| Selisih governance | Salinan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` pada `docs/engineering/` (repository ini) berbeda dari salinan pada suite Skill (baris Finance dan prefix `Gzi`/`Gz`). `BACKEND_ENGINEERING_CONTRACT.md` identik. Baris `Bil` sama di keduanya. Salinan repository yang berlaku; tidak memengaruhi task ini |
| Roadmap dan traceability | Tidak ada kartu roadmap untuk task `FIX` ad-hoc (sama seperti `BE-BKC-FIX-013`), sehingga tidak ada tanda status yang dapat ditulis pada `roadmap/**`. `requirement-traceability.md` tidak diubah karena tidak ada requirement baru yang dijawab |
| Perubahan sampingan | `NONE`. Skrip bantu penyuntingan snapshot ditulis di folder scratchpad sesi, bukan di repository |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir pekerjaan: `M` BillingInvoiceDtos.cs, BilInvoice.cs, BillingInvoiceService.cs, ApplicationDbContextModelSnapshot.cs, BilInvoiceConfiguration.cs; `??` `20261005090000_OptimizeRunningInvoiceQueries.cs` dan `.Designer.cs`; ditambah laporan ini. `MstPatientConfiguration.cs` tidak lagi berubah. Tidak ada stage, commit, push, atau perubahan branch |
| Langkah berikutnya | 1) `dotnet build -c Release` (wajib diulang karena `--no-build` memakai DLL lama). 2) `dotnet ef migrations has-pending-model-changes`. 3) `dotnet ef database update --no-build --configuration Release` di luar jam layanan; hanya satu migration baru yang akan diterapkan. 4) `EXPLAIN (ANALYZE, BUFFERS)` untuk `GET /` (periode hari ini + `OPEN`, serta dengan pencarian) dan `GET /{id}` sebelum/sesudah. 5) Putuskan tindak lanjut temuan (a)-(d). 6) Setelah 1-3 lulus, ubah status task ini menjadi `✅`. 7) Bila kelak ada akses superuser, minta migration indeks trigram (lihat Masalah yang diketahui) |
