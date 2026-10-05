# Laporan Perubahan Backend — `BE-FIN-073`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-073` |
| Judul | Penanda shift terbit tanpa seseorang menekan tombol |
| Slice | `REV-14C` — `EPIC FIN-22`, jalur pengiriman |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14C — `EPIC FIN-22`, jalur pengiriman" |
| Trace | `FR-FIN-156`; `FIN-DEC-118`; `FIN-DES-078` |
| Contract version | `FIN-INTEGRATION-1.7` §5.12.6. Bagian ini juga menegaskan ketiga kode penanda shift (`PENUTUPAN-`, `PEMBALIKAN-PENUTUPAN-`, `PEMBUKAAN-SHIFT-KASIR`) tetap **dilewati** oleh `FinanceAccountingDispatchWorker` (`BE-FIN-071`, `GatedEventTypeCodes`) sampai Accounting menyatakan G6 siap — task ini hanya menerbitkan baris ke outbox Finance, **bukan** mengirimkannya ke Accounting. Kedua gerbang itu independen sesuai desain `FIN-DES-078` |
| Dependency | `BE-FIN-070` ✅ (selesai) |
| Klasifikasi | `LIGHT` — repo 0 + berkas diperiksa 9-20 (1) + berkas diubah ≤3 (0) + logika sederhana (0) + kontrak API tidak ada (0) + database tidak ada (0) + auth tidak ada (0) + UI tidak ada (0) = 1. Konsisten dengan catatan roadmap sendiri: "risiko rendah; task terkecil REV-14C" |
| Task mode | `BACKEND` (target tulis backend; frontend tidak disentuh — tidak ada task frontend pasangan pada baris `FR-FIN-156`) |
| Target tulis | `NewQuilvianSystemBackend`, terbatas pada `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceCashierShiftMarkerSchedulerHostedService.cs` (baru), `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceCashierShiftMarkerSchedulerOptions.cs` (baru), `Program.cs`, ditambah laporan tracked ini dan pembaruan register roadmap |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C sebelumnya — lihat bagian 7 "Status Git") |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap dan seluruh acceptance criteria yang dapat diverifikasi lewat pembacaan kode sudah terpenuhi; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, penanda status shift kasir (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`,
`PEMBUKAAN-SHIFT-KASIR`) **hanya** dapat diterbitkan lewat endpoint manual
`POST /billing-intake/cashier-shift-closure-markers/sync` (`BE-FIN-045`, diperluas `BE-FIN-070`).
Temuan `FIN-CAP-052` pada `01-existing-capability-map.md` baris 974 mencatatnya eksplisit:
**"Missing pemicu otomatis — penanda hanya terbit bila seseorang menekan sinkronisasi — Accounting
tidak akan pernah melihat shift tertutup tanpa tindakan manual."**

Akibatnya, bila tidak ada operator yang mengingat menekan tombol sinkronisasi, Accounting tidak akan
pernah tahu shift kasir mana yang sudah tertutup (sehingga tidak dapat menutup bulan dengan percaya
diri) maupun shift mana yang masih terbuka (sehingga tutup bulan bisa saja dilakukan padahal ada kas
yang belum direkonsiliasi).

---

## 2. Proses bisnis

**Tujuan.** Menjadikan penerbitan ketiga penanda shift berjalan otomatis secara berkala, tanpa
menunggu seseorang menekan tombol sinkronisasi manual.

**Pelaku.** `FinanceCashierShiftMarkerSchedulerHostedService` (baru) sebagai pemicu berkala,
`FinanceBillingIntakeService.SyncCashierShiftClosureMarkersAsync` (sudah ada sejak `BE-FIN-045`,
diperluas `BE-FIN-070`, **tidak diubah sama sekali** oleh task ini) sebagai pelaksana.

**Pemicu dan langkah berurutan:**

1. Penjadwal berjalan sebagai `BackgroundService` yang hidup selama aplikasi berjalan (bila
   `Finance:CashierShiftMarkerScheduler:Enabled = true`). Setiap `PollIntervalSeconds` (bawaan 60
   detik), ia membuka scope baru dan memanggil `SyncCashierShiftClosureMarkersAsync` dengan actor
   sistem dari konfigurasi (bawaan `Guid.Empty`).
2. Di dalam method yang **sudah ada dan tidak disentuh**: seluruh shift kasir dibaca (`AsNoTracking`,
   nol tulisan ke tabel `Bil*`), lalu untuk setiap shift yang `CLOSED`/`REVIEWED` diterbitkan penanda
   penutupan (dan mutasi kas `KAS-SHIFT` bila relevan — bagian yang sudah ada sejak `BE-FIN-062`),
   shift yang dibuka kembali setelah tertutup diterbitkan penanda pembalik, dan shift berstatus
   **belum final** (`OPEN`, `HANDED_OVER`, `REOPENED`, `CLOSED_WITH_VARIANCE`, `PERLU_TINDAK_LANJUT`)
   diterbitkan penanda pembukaan (`BE-FIN-070`, `FIN-DEC-121`).
3. Idempotensi sepenuhnya diwarisi dari method yang sudah ada: shift yang penandanya sudah pernah
   terbit untuk siklus yang sama **tidak** diterbitkan ulang. Penjadwal ini **tidak menambah**
   mekanisme idempotensi baru apa pun — ia murni pemicu berkala.
4. Baris yang diterbitkan masuk ke `FinAccountingEventOutbox` berstatus `PENDING`, tetapi **tetap
   dilewati** `FinanceAccountingDispatchWorker` (`BE-FIN-071`) karena ketiga kode ini ada di dalam
   `GatedEventTypeCodes`-nya — sehingga Accounting belum benar-benar menerimanya sampai G6 disiapkan.
   Task ini **hanya** menjawab "apakah baris sudah ada di outbox Finance", bukan "apakah sudah sampai
   ke Accounting" — dua pertanyaan yang sengaja dipisah `FIN-DES-078`.

**Aturan yang berlaku.** `FIN-DEC-118` (membuka kembali pembangunan jalur pengiriman sebagai tiga
hosted service terpisah), `FIN-DES-078` (hosted service dibangun mati, gerbang konfigurasi dan gerbang
kode terpisah per hosted service).

**Jalur tidak normal.** Kegagalan tak terduga pada satu siklus (mis. masalah konektivitas database
sesaat) dicatat sebagai galat lewat `_logger.LogError`, dan **tidak menghentikan** hosted service —
siklus berikutnya mencoba lagi sesuai jeda yang dikonfigurasi. `SyncCashierShiftClosureMarkersAsync`
sendiri tidak mendefinisikan exception bisnis khusus (dikonfirmasi: jalur manual yang sudah berjalan
pada `FinanceBillingIntakeController` juga tidak membungkusnya dengan try/catch spesifik), sehingga
penjadwal ini menangkap `Exception` umum per siklus — pola yang sama dengan
`AccAccountingEventSchedulerHostedService` dan `FinanceAccountingDispatchWorker`.

**Hasil akhir.** Ketiga penanda shift terbit ke outbox Finance secara berkala tanpa tindakan manusia,
begitu pemilik modul mengaktifkan konfigurasinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (baris task `BE-FIN-073`, `FIN-DES-078`)
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` (`FIN-CAP-052`)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§5.12.6 — gerbang dispatch worker, dipastikan independen dari task ini)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (method `SyncCashierShiftClosureMarkersAsync` dan record `CashierShiftClosureMarkerSyncResult` — dibaca sebagai bukti, **tidak diubah**)
- `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` (endpoint manual `POST cashier-shift-closure-markers/sync` — dipastikan pola pemanggilan dan penanganan exception)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (dipastikan `FinanceBillingIntakeService` sudah terdaftar `AddScoped`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingDispatchWorker.cs` (dipastikan ketiga kode penanda shift sudah ada di `GatedEventTypeCodes` — bukti bahwa task ini aman diaktifkan independen dari gerbang G3/G6)
- `Program.cs` (blok `runBackgroundJobs`, pola registrasi `FinanceAccountingDispatchWorker`/`FinanceSubledgerSnapshotSchedulerHostedService`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceCashierShiftMarkerSchedulerOptions.cs` (baru) | Kelas options: `Enabled` (bawaan `false` — dibangun mati), `PollIntervalSeconds` (bawaan 60), `SystemActorUserId` (nullable, bawaan `Guid.Empty`) |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceCashierShiftMarkerSchedulerHostedService.cs` (baru) | `BackgroundService` baru — hosted service **paling sederhana** dari tiga yang dibangun `FIN-DES-078` (interval polling murni, tanpa jam spesifik, tanpa panggilan HTTP keluar). Memanggil `FinanceBillingIntakeService.SyncCashierShiftClosureMarkersAsync` (**tidak diubah**) tiap siklus lewat scope baru; menangkap `Exception` umum per siklus tanpa menghentikan hosted service |
| `Program.cs` | Menambah `using` untuk namespace `BillingIntake.Services`; menambah `builder.Services.Configure<FinanceCashierShiftMarkerSchedulerOptions>(builder.Configuration.GetSection("Finance:CashierShiftMarkerScheduler"))` dan `builder.Services.AddHostedService<FinanceCashierShiftMarkerSchedulerHostedService>()` di dalam blok `runBackgroundJobs`, tepat sesudah registrasi `FinanceSubledgerSnapshotSchedulerHostedService` (BE-FIN-072) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **NOT APPLICABLE.** Task ini tidak menambah maupun mengubah endpoint HTTP apa pun — cakupannya murni "1 hosted service + options; 1 `AddHostedService`" sesuai roadmap. Endpoint manual `POST cashier-shift-closure-markers/sync` yang sudah ada **tidak disentuh** |
| Database | **NOT APPLICABLE.** Nol model baru, nol kolom baru, nol migration — penjadwal murni memanggil service yang sudah ada dan sudah divalidasi pada `BE-FIN-045`/`BE-FIN-070` |
| Keamanan/Auth | **NOT APPLICABLE.** Tidak ada endpoint, `[AccessController]`/`[AccessAction]`/`[AccessPermission]` baru. Hosted service berjalan di luar konteks HTTP/user yang terautentikasi, konsisten dengan seluruh hosted service lain pada aplikasi ini |

---

## 4. Dokumentasi endpoint

**NOT APPLICABLE.** Task ini tidak membuat maupun mengubah endpoint HTTP.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna pada task ini: "jangan lakukan build backend secara automatis". Sesuai `rules/backend/TEST_POLICY.md` dan `status-task-roadmap.md` §2.1, ketiadaan hasil build sebenarnya menahan status `✅` — task ini karena itu ditandai `🟡`, bukan diklaim `PASS` tanpa dijalankan |
| Pembacaan kode: hosted service dibangun mati | Terverifikasi — `Enabled` bawaan `false`; `ExecuteAsync` kembali setelah satu baris log bila mati | `PASS` | `FinanceCashierShiftMarkerSchedulerOptions.cs`, `FinanceCashierShiftMarkerSchedulerHostedService.cs` |
| Pembacaan kode: terbit berkala tanpa tindakan pengguna | Terverifikasi — `ExecuteAsync` memicu `SyncCashierShiftClosureMarkersAsync` sendiri tiap `PollIntervalSeconds`, tanpa menunggu panggilan HTTP apa pun | `PASS` | `FinanceCashierShiftMarkerSchedulerHostedService.cs`, `JalankanSatuSiklusAsync` |
| Pembacaan kode: nol tulisan ke tabel `Bil*` | Terverifikasi — diwarisi dari `SyncCashierShiftClosureMarkersAsync` yang **tidak diubah**; seluruh akses `BilCashierShifts` memakai `AsNoTracking()` (dikonfirmasi pada komentar kelas method itu sendiri: "Shift kasir DIBACA SAJA... nol tulisan ke tabel Bil* mana pun") | `PASS` | `FinanceBillingIntakeService.cs` baris 1059, 1073-1082 |
| Pembacaan kode: idempotensi tidak terganggu | Terverifikasi — penjadwal tidak menambah mekanisme idempotensi apa pun; seluruhnya diwarisi dari method yang sudah terbukti pada laporan `BE-FIN-045`/`BE-FIN-070` | `PASS` (lewat pewarisan kontrak task sebelumnya, bukan diuji ulang pada task ini) | Laporan [BE-FIN-070](BE-FIN-070.md) |
| Pembacaan kode: penanda shift tetap tertahan di gerbang pengiriman G6 | Terverifikasi — ketiga kode penanda (`PenutupanShiftKasir`, `PembalikanPenutupanShiftKasir`, `PembukaanShiftKasir`) sudah terdaftar di `FinanceAccountingDispatchWorker.GatedEventTypeCodes`, dibangun pada `BE-FIN-071` sebelum task ini — mengaktifkan penjadwal ini **tidak** membuka pengiriman penanda shift ke Accounting secara tidak sengaja | `PASS` | `FinanceAccountingDispatchWorker.cs` baris 51-66 |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan runtime aplikasi aktif dan database aktif
dengan data shift kasir sungguhan, di luar cakupan perubahan source tanpa eksekusi aplikasi pada
task ini.

**Tidak dijalankan:** `dotnet build` (lihat tabel di atas), uji manual end-to-end (`NOT FEASIBLE`,
memerlukan lingkungan runtime), dan `dotnet test` (backend tidak memelihara project automated test —
`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Terbit berkala tanpa tindakan pengguna | Terpenuhi (source) | Bagian 2, bagian 5 |
| Mati secara bawaan | Terpenuhi | `FinanceCashierShiftMarkerSchedulerOptions.Enabled = false` |
| Nol tulisan ke tabel `Bil*` | Terpenuhi (diwarisi, tidak diubah) | Bagian 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna pada task ini | Bagian 5 |
| Nol migration | Terpenuhi | Bagian 3.3 |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna pada task ini ("tapi jangan lakukan build backend secara automatis"). Ini **bukan**
pengecualian DoD yang melepas butir tersebut secara permanen (lihat `status-task-roadmap.md` §2.2) —
build tetap wajib dijalankan sebelum task ini dapat dinaikkan ke `✅`, hanya saja bukan pada sesi ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` belum pernah dijalankan terhadap perubahan ini. Risiko compile error (mis. namespace `using` baru pada `Program.cs` yang bentrok nama tipe) belum tersingkirkan oleh bukti build — hanya oleh pembacaan source yang cermat |
| Masalah yang diketahui | `NONE` — tidak ditemukan gap kontrak baru pada task ini. Gerbang pengiriman sesungguhnya ke Accounting (G6, `FIN-OQ-035`/`047`) tetap menjadi urusan `BE-FIN-071`, bukan task ini |
| Risiko tersisa | Penjadwal belum pernah diuji berjalan pada runtime sungguhan dengan data shift aktif. Risiko tertinggi task ini (sesuai roadmap: "risiko rendah; task terkecil REV-14C") tetap lebih rendah dibanding kedua hosted service Finance lainnya, karena tidak ada panggilan HTTP keluar maupun kredensial yang terlibat |
| Perubahan sampingan | `NONE` — ketiga berkas yang disentuh murni penambahan untuk task ini; `Program.cs` yang sudah membawa perubahan tidak ter-commit dari task-task REV-14 sebelumnya tidak dipulihkan maupun diubah bagian lamanya |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir task menunjukkan dua berkas baru (`FinanceCashierShiftMarkerSchedulerHostedService.cs`, `FinanceCashierShiftMarkerSchedulerOptions.cs`) dan satu berkas termodifikasi (`Program.cs`) milik task ini, tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`072` (REV-14A/14B/14C) yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh oleh task ini |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` dan mengonfirmasi hasilnya, lalu laporan ini dan roadmap dinaikkan ke `✅`; (2) pemilik modul mengisi `Finance:CashierShiftMarkerScheduler:Enabled = true` pada konfigurasi lingkungan kapan pun siap, tanpa perlu menunggu G3/G6 — gerbang pengiriman sesungguhnya tetap terjaga `FinanceAccountingDispatchWorker` |
