# Laporan Perubahan Backend — `BE-FIN-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-022` |
| Judul | Empat jenis fakta Billing baru menjadi nilai `HandoffType` yang sah |
| Slice | `REV-3` — AMENDMENT REVISI 3 (`EPIC FIN-14`), `01-backend-roadmap.md` bagian 3 dan 4 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 2, bagian 3 |
| Trace | Keputusan bisnis `FIN-DEC-040`..`044`; keputusan arsitektur `FIN-DES-029`; kontrak `FIN-STATE-1.1` bagian 1, `FIN-VAL-1.1`; kamus data `erd/data-dictionary.md` baris `HandoffType` |
| Contract version | `FIN-STATE-1.1` dan `FIN-VAL-1.1` — keduanya `locked` 25 September 2026, `approved` owner |
| Dependency | Tidak ada task yang menahan. Tabel `FinBillingHandoffIntake` sudah ada sejak `BE-FIN-005` ✅ |
| Klasifikasi | `MEDIUM` — skor 4: repository tunggal (0), berkas diperiksa 12 (1), berkas diubah 5 (1), logika bisnis sederhana (0), kontrak API tidak tersentuh (0), dampak schema/migration (2), keamanan tidak tersentuh (0), UI tidak tersentuh (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi dan berkas migration, ditambah berkas laporan serta roadmap modul ini |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852`, branch `Yasmina` |
| Tanggal | 25 September 2026 |
| Status | 🟡 **SEBAGIAN** — source lengkap dan `dotnet build` `0 Error`, tetapi kemampuan yang dijanjikan outcome **belum berlaku di database** karena eksekusi migration berada di luar wewenang sesi ini. Lihat bagian 6 |

---

## 1. Masalah yang diperbaiki

Modul Finance sudah punya satu pintu masuk untuk seluruh fakta dari Billing, yaitu tabel
`FinBillingHandoffIntake`. Sampai sebelum perubahan ini, pintu itu hanya mengenali **empat**
jenis fakta: piutang (`AR`), utang (`AP`), penerimaan kasir (`COLLECTION`), dan koreksi
(`ADJUSTMENT`).

Amendment revisi 3 blueprint menambahkan empat jenis fakta baru yang harus masuk lewat pintu yang
sama: mutasi deposit pasien, pengakuan kelebihan bayar, pengembalian uang ke pasien, dan
pengesahan selisih kas shift kasir.

> **Akibat nyata bila dibiarkan.** Petugas menaruh deposit Rp 5.000.000 untuk pasien rawat inap.
> Billing mencatat mutasinya. Ketika Finance mencoba mencatat fakta itu di pintu masuknya dengan
> jenis `DEPOSIT_MOVEMENT`, database **menolak** penyimpanannya karena aturan pemeriksaan nilai
> (*check constraint*) `CK_FinBillingHandoffIntake_HandoffType` hanya mengizinkan empat nilai
> lama. Fakta keuangan yang nyata gagal masuk, dan tidak ada jalan lain yang sah — memaksakannya
> dengan memakai jenis lama (misalnya `COLLECTION`) justru membuat deposit terbaca sebagai
> penerimaan kasir, yang lawan jurnalnya berbeda.

---

## 2. Proses bisnis

**Tujuan.** Membuat empat jenis fakta baru dapat disimpan di pintu masuk Finance, tanpa membuat
tabel baru dan tanpa mengubah arti empat jenis yang sudah ada.

**Pelaku.** Tidak ada pelaku manusia baru. Yang memakai nilai-nilai ini adalah sistem, saat
menyinkronkan fakta dari Billing. Petugas Finance tidak pernah mengetik nilai `HandoffType`
secara manual.

**Pemicu.** Task ini bersifat fondasi: ia tidak dipicu kejadian bisnis, melainkan menyiapkan
tempat bagi jalur sinkronisasi yang dibangun task berikutnya (`BE-FIN-025`).

**Prasyarat.** Tabel `FinBillingHandoffIntake` sudah ada (dibangun `BE-FIN-005`).

**Langkah utama.**

1. Empat nilai baru didaftarkan sebagai konstanta pada `FinBillingHandoffTypes`, sehingga kode
   pemanggil tidak pernah menulis teksnya secara bebas.
2. Aturan pemeriksaan nilai pada database diperluas dari empat menjadi delapan nilai yang sah.
3. Berkas migration dibuat untuk membawa perubahan aturan itu ke database.

**Aturan bisnis yang berlaku.**

| Aturan | Isi |
| --- | --- |
| Empat nilai lama tidak berubah | `AR`, `AP`, `COLLECTION`, `ADJUSTMENT` tetap sah dan tetap berarti hal yang sama. Tidak ada baris lama yang perlu diisi ulang |
| Satu fakta hanya boleh masuk sekali | Dijaga index unik `(HandoffType, SourceHandoffKey)` yang **sudah ada** dan **tidak disentuh** task ini |
| Kunci idempotensi diambil dari sumbernya | `DEPOSIT_MOVEMENT` dan `REFUND_CASE` memakai `IdempotencyKey` milik Billing; `REFUNDABLE_CREDIT` dan `CASH_VARIANCE_REVIEW` memakai `Id` baris sumbernya, karena sumbernya tidak punya kunci sendiri (`FIN-DES-029`) |
| Empat jenis baru berhenti di `CONSUMED` | Sumbernya tidak punya kolom status handoff yang bisa ditandai, dan Finance dilarang menulis ke tabel Billing (`FIN-STATE-1.1` bagian 1) |

**Perubahan status.** Task ini **tidak** menambah satu pun status baru maupun transisi baru.
Siklus `NEW → CONSUMED → ACKNOWLEDGED` beserta `ERROR` tetap seperti sebelumnya; yang bertambah
hanya nilai kolom pembeda jenisnya.

**Jalur tidak normal.**

| Kejadian | Yang terjadi |
| --- | --- |
| Nilai `HandoffType` di luar delapan nilai sah | Database menolak penyimpanan lewat aturan pemeriksaan nilai — bukan lewat pemeriksaan di kode yang bisa terlewat |
| Fakta yang sama disinkronkan dua kali | Baris kedua ditolak index unik; fakta dianggap sudah masuk |
| Migrasi dimundurkan sementara sudah ada baris bernilai baru | `Down()` **gagal**. Ini disengaja dan sudah ditulis pada ringkasan berkas migration: baris itu harus dipindah atau dihapus lebih dulu |

**Hasil akhir.** Setelah migration dijalankan, keempat jenis fakta baru dapat disimpan di pintu
masuk Finance. Sebelum migration dijalankan, source sudah siap tetapi database masih menolaknya —
lihat bagian 6.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `AGENTS.md` | Konstitusi repository; gerbang wewenang dan keselamatan database |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Memastikan `Finance`/`Fin` berstatus `ACTIVE` |
| `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` | Tempat konstanta jenis fakta |
| `Repositories/Configurations/.../FinBillingHandoffIntakeConfiguration.cs` | Tempat aturan pemeriksaan nilai dan index unik |
| `Migrations/20260821020839_AllowOutOfQueueScaleTriageLevel.cs` | Preseden pola mengubah aturan pemeriksaan nilai |
| `Migrations/20260915074405_RevisiTablePettyCash.cs` | Preseden kedua, untuk perubahan berlapis |
| `Migrations/MigrationMetadata.g.cs` | Memastikan cara repository mendaftarkan migration |
| `Migrations/20260924083000_...Designer.cs` | Rujukan bentuk berkas Designer terbaru |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model terkini |
| `02-backend-architecture.md` bagian B.2, B.5, B.6 | Keputusan arsitektur `FIN-DES-029` beserta rencana migration |
| `contracts/state-transition-matrix.md` bagian 1 | Daftar empat jenis baru beserta sumber kunci idempotensinya |
| `roadmap/01-backend-roadmap.md` bagian 3 | Cakupan, acceptance criteria, dan batas task |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` | `FinBillingHandoffTypes` bertambah empat konstanta: `DepositMovement`, `RefundableCredit`, `RefundCase`, `CashVarianceReview`. Ringkasan kelas diperbarui agar tidak menyesatkan pembaca berikutnya. Empat konstanta lama tidak disentuh |
| `Repositories/Configurations/Corporate/FinanceManagement/BillingIntake/FinBillingHandoffIntakeConfiguration.cs` | Aturan pemeriksaan nilai `CK_FinBillingHandoffIntake_HandoffType` diperluas dari empat menjadi delapan nilai |
| `Migrations/20260925090000_AlterFinBillingHandoffIntakeHandoffTypeCheck.cs` | **Baru.** `Up()` menghapus lalu memasang ulang aturan pemeriksaan nilai dengan delapan nilai; `Down()` mengembalikannya ke empat |
| `Migrations/20260925090000_AlterFinBillingHandoffIntakeHandoffTypeCheck.Designer.cs` | **Baru.** Mengikuti pola repository: atribut `[DbContext]` dan `[Migration]` beserta `BuildTargetModel`, disalin dari Designer migration terakhir lalu disesuaikan pada tiga titik (nama migration, nama kelas, dan aturan pemeriksaan nilai) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Satu baris: aturan pemeriksaan nilai yang sama diperbarui agar snapshot mencerminkan model terkini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak menyentuh satu pun controller, endpoint, DTO, maupun route. `FIN-API-1.0` dan `FIN-PERM-1.0` sengaja tidak bergerak pada amendment ini |
| Database | **Ada.** Satu aturan pemeriksaan nilai diubah pada tabel `FinBillingHandoffIntake`. **Nol tabel baru, nol kolom baru, nol index baru.** Berkas migration sudah dibuat; **eksekusinya BELUM dijalankan** — lihat bagian 6 |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint, tidak ada permission, tidak ada pemeriksaan kewenangan yang tersentuh |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `FinanceManagement` |
| Submodule | `BillingIntake` |
| Pemilik/prefix pada registry | `Finance` / `Fin` / `ACTIVE` — baris 13 `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `TOUCHED LEGACY` **tidak** berlaku. Berkas yang disentuh adalah kode yang dibangun modul ini sendiri pada `BE-FIN-005` dan sudah patuh QBE, sehingga standar `NEW CODE` berlaku penuh |
| Status registry | Terdaftar sebelum berkas model pertama ditulis (`BE-FIN-001` ✅) — `QBE-MOD-002` dan `QBE-MOD-003` terpenuhi, bukan dilewati |
| QBE ID yang benar-benar berlaku | `QBE-ENT-001` (entity mewarisi `IdentityModel` — tidak berubah, tetap terpenuhi), `QBE-CFG-001` (configuration memegang mapping, key, index, constraint), `QBE-NAM-002` (prefix `Fin` dari registry yang disetujui), `QBE-CODE-004` (kode bisnis unik punya constraint database — index unik `(HandoffType, SourceHandoffKey)` tetap terpasang) |
| QBE ID yang **tidak** berlaku | `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — ketiganya untuk `LEGACY MIGRATION`, dan task ini bukan itu. `QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001`, `QBE-DTO-001` — tidak ada permukaan API yang tersentuh |
| Sisa `agents/rules/` di repository target | Tidak ditemukan — sudah dicabut sebagaimana mestinya |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah, mengubah, maupun menghapus endpoint. Permukaan API
Finance tidak bergerak sama sekali.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` project aplikasi | Berhasil — **`0 Error`, `229 Warning`**, waktu 2 menit 49 detik | `PASS` | Keluaran perintah. Seluruh 229 warning adalah `CS1573`/`CS1734` (komentar XML kurang tag parameter) pada berkas yang **tidak berkaitan** dengan task ini — Medical Record, Pharmacy, Operating Room, Filters — dan sudah ada sebelum perubahan ini |
| Review diff/scope | Hanya lima berkas berubah, seluruhnya di dalam cakupan task | `PASS` | `git diff --stat` atas ketiga berkas source: 29 baris ditambah, 4 dihapus. `ModelSnapshot` hanya **satu** baris |
| Verifikasi proses bisnis — empat nilai lama tetap sah | Keempatnya masih ada di dalam aturan pemeriksaan nilai yang baru | `PASS` | `FinBillingHandoffIntakeConfiguration.cs`: `IN ('AR','AP','COLLECTION','ADJUSTMENT','DEPOSIT_MOVEMENT','REFUNDABLE_CREDIT','REFUND_CASE','CASH_VARIANCE_REVIEW')` — empat nilai lama berada di posisi pertama, tidak berubah |
| Verifikasi proses bisnis — index unik tidak tersentuh | `IX_FinBillingHandoffIntake_Identity` tetap `(HandoffType, SourceHandoffKey)` unik dengan filter `IsDelete = false` | `PASS` | Berkas configuration yang sama, blok `HasIndex` tidak ikut berubah pada diff |
| Verifikasi konsistensi Designer terhadap rujukan | Tiga substitusi terverifikasi tepat satu kemunculan masing-masing; ukuran berkas 4.977.166 byte vs rujukan 4.977.111 byte (selisih 55 byte = perubahan nama dan empat nilai baru) | `PASS` | Pemeriksaan terprogram atas jumlah kemunculan sebelum penulisan |
| Verifikasi batas task — berkas yang MUST NOT disentuh | `FinanceBillingIntakeService`, `FinanceAccountingOutboxService`, `FinanceReceiptService`, dan seluruh tabel `Bil*` **tidak** muncul pada diff | `PASS` | `git status --short` dan `git diff --stat` |
| Eksekusi migration ke database | **Tidak dijalankan** | `NOT RUN` | Di luar wewenang sesi ini — owner secara eksplisit membatasi wewenang pada pembuatan berkas migration saja |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — perilaku yang dijanjikan (menyimpan baris ber-jenis baru) baru dapat
dicoba setelah migration dijalankan, dan itu di luar wewenang sesi ini.

**Tidak dijalankan, beserta alasannya:**

| Pemeriksaan | Alasan |
| --- | --- |
| `dotnet ef database update` | **Dilarang pada sesi ini.** Owner memberi wewenang membuat berkas migration, bukan menjalankannya |
| `dotnet restore` | Tidak ada dependency maupun berkas project yang tersentuh |
| Pengulangan `dotnet build` sesudah penghapusan BOM | Owner meminta `dotnet build` tidak dijalankan otomatis. Penghapusan BOM tidak mengubah satu pun token C#, hanya tiga byte penanda di awal berkas — lihat bagian 7 |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis seperti roadmap) | Status | Bukti |
| --- | --- | --- |
| Baris ber-`HandoffType` baru dapat disimpan | **Belum terpenuhi di database** | Source lengkap: konstanta, configuration, dan berkas migration ada. Tetapi aturan pemeriksaan nilai **di database masih empat nilai** sampai migration dijalankan, sehingga penyimpanan baris ber-jenis baru masih akan ditolak. Ini satu-satunya kriteria yang menahan status ✅ |
| Baris ber-nilai lama tetap sah | Terpenuhi | Empat nilai lama tetap berada di dalam aturan pemeriksaan nilai yang baru; `Up()` tidak menyentuh baris data apa pun |
| Index unik tetap menolak fakta yang sama dua kali | Terpenuhi | `HasIndex(HandoffType, SourceHandoffKey).IsUnique()` tidak tersentuh diff |

| Definition of Done (roadmap) | Status | Bukti |
| --- | --- | --- |
| Nol tabel dan nol kolom baru | Terpenuhi | Migration hanya memuat `DropCheckConstraint` dan `AddCheckConstraint` |
| `Down()` mengembalikan constraint ke empat nilai | Terpenuhi | Berkas migration bagian `Down()` |
| Batas keamanan `Down()` dinyatakan | Terpenuhi | Ringkasan berkas migration menyatakan `Down()` akan gagal bila sudah ada baris bernilai baru, beserta langkah yang harus dilakukan lebih dulu |

**Kesimpulan status: 🟡 SEBAGIAN.** Seluruh pekerjaan source selesai dan build membuktikannya,
tetapi outcome task — "empat jenis fakta Billing baru **menjadi nilai yang sah**" — belum berlaku
sampai migration dijalankan. Menandainya ✅ akan menyesatkan pembaca berikutnya, yang bisa
menyangka `BE-FIN-025` sudah punya fondasi yang siap dipakai di database.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 229 warning `dotnet build`, seluruhnya `CS1573`/`CS1734` pada berkas yang tidak berkaitan dan sudah ada sebelum perubahan ini. Nol warning baru dari lima berkas yang disentuh |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | (a) Selama migration belum dijalankan, `BE-FIN-025` **tidak boleh** dimulai — ia akan menulis baris ber-jenis baru yang pasti ditolak database. (b) `Down()` akan gagal bila kelak sudah ada baris bernilai baru; ini disengaja dan sudah tertulis di berkas migration |
| Perubahan sampingan | **Satu, dan sudah dipulihkan.** Penulisan awal menambahkan penanda BOM ke `ApplicationDbContextModelSnapshot.cs` dan ke Designer baru, padahal berkas rujukan repository tidak memakainya. BOM dihapus, sehingga diff `ModelSnapshot` kembali menjadi tepat satu baris. Tidak ada pekerjaan user lain yang tersentuh |
| Interupsi | Satu instruksi owner masuk di tengah pengerjaan: `dotnet build` tidak boleh dijalankan otomatis. Build sudah berjalan sekali **sebelum** instruksi itu masuk, hasilnya dipakai apa adanya dan tidak diulang. Instruksi itu disimpan sebagai preferensi tetap |
| Status Git | `M Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs`, `M Repositories/Configurations/Corporate/FinanceManagement/BillingIntake/FinBillingHandoffIntakeConfiguration.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs`, `?? Migrations/20260925090000_AlterFinBillingHandoffIntakeHandoffTypeCheck.cs`, `?? Migrations/20260925090000_AlterFinBillingHandoffIntakeHandoffTypeCheck.Designer.cs`. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | (1) Minta wewenang eksekusi migration dari owner, jalankan `AlterFinBillingHandoffIntakeHandoffTypeCheck`, lalu naikkan status task ini menjadi ✅. (2) `BE-FIN-023`..`026` tetap ⛔ menunggu ratifikasi owner Accounting atas tujuh kode kejadian (`FIN-OQ-017`) — bukan menunggu task ini |
