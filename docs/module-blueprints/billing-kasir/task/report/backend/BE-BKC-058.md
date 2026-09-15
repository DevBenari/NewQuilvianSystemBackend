# Laporan Perubahan Backend — `BE-BKC-058`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-058` |
| **Judul** | Ringkasan halaman gabungan dan daftar periode |
| **Slice** | `MVP-23` (halaman gabungan + aktivasi) — eksekusi gelombang 3 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-058` |
| **Trace** | `FR-BKC-104`; `PC-DES-025` |
| **Contract version** | `BIL-API-1.1` — `approved` 15 September 2026 |
| **Dependency** | `BE-BKC-054` (periode anggaran beserta plafon dan sisanya). **Diverifikasi lewat pembacaan source pada sesi ini**: `CreatePeriodAsync`, `ActivatePeriodAsync`, `ClosePeriodAsync` sudah ada di `PettyCashBudgetService`, dan `PettyCashBudgetResponse` sudah memuat `PeriodStart`/`PeriodEnd`/`BudgetAmount`/`RemainingBudgetAmount`/`Status`. **Catatan**: `BE-BKC-054` sendiri belum memiliki laporan tracked dan roadmap belum ditandai — gap pra-existing yang sama seperti dicatat pada laporan `BE-BKC-057`, di luar scope task ini |
| **Klasifikasi** | `LIGHT` (skor 2 — repository backend saja → 0; berkas diperiksa sekitar 10 dokumen/source → 1; berkas diubah 3 → 0; logika bisnis murni agregasi baca → 0; kontrak API memakai kontrak yang sudah dikunci → 1; database tidak ada dampak schema/migration → 0; keamanan memakai permission `Read` yang sudah ada, tidak ada permission baru → 0; UI/workflow tidak ada → 0) |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Source backend (`Areas/HealthServices/BillingManagement/PettyCash/**`), laporan task ini, dan baris status pada roadmap serta `requirement-traceability.md` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `0ca85ba4610f2745b761d5e092b495bfc35396b0` (branch `Yasmina`) |
| **Tanggal** | 15 September 2026 |
| **Status** | 🟡 **Sebagian.** Source endpoint `GET /budget/overview` sudah lengkap dan direview manual, tetapi `dotnet build` **tidak dijalankan pada sesi ini atas instruksi eksplisit pengguna** ("tanpa build otomatis") — DoD roadmap mewajibkannya lulus. Acceptance criteria (bentuk `PettyCashOverviewResponse` mengembalikan keenam angka dalam satu panggilan) belum dapat diverifikasi lewat request HTTP sungguhan karena migration `BE-BKC-053` masih dalam proses regenerasi terpisah dan belum diterapkan ke database manapun |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash` (Petty Cash Budget) |
| **Owner / Prefix Registry** | Prefix `Bil` — `HealthServices / BillingManagement / Billing`, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE` — sudah terdaftar, tidak ada modul/entity baru pada task ini |
| **Keberlakuan** | `TOUCHED LEGACY` — satu endpoint baca baru pada controller yang sudah ada (`PettyCashBudgetController`), memakai DTO baru murni agregasi tanpa entity/tabel baru |
| **QBE ID yang berlaku** | `QBE-API-001` (endpoint baru mengikuti pola `GET` baca existing pada controller yang sama), `QBE-PERM-001` (reuse permission `PettyCashBudget : Read` yang sudah ada — **tidak** ada permission baru dibuat), `QBE-DTO-001` (DTO baru di folder `Dtos/` domain pemilik) |
| **Pengecualian / Temuan** | `dotnet build` **tidak dijalankan** atas instruksi eksplisit pengguna pada sesi ini — dicatat `NOT RUN` pada bagian 5, bukan `PASS`. Satu ketidakcocokan kecil ditemukan pada dokumen kontrak sendiri: `api-contract.md` baris 829 menyebut "kelima angka", sedangkan kartu roadmap task ini menyebut DoD "keenam angka" — keduanya merujuk bentuk `PettyCashOverviewResponse` yang sama; diimplementasikan sesuai daftar field eksplisit yang tertulis (`activeBudget`, `pendingEvidenceCount`, `pendingDisbursementCount`, `totalDisbursedThisPeriod`), selisih penghitungan angka dicatat sebagai temuan dokumentasi, bukan diperbaiki di sini karena bukan wewenang task ini menyunting `contracts/**` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, layar Petty Cash yang ingin menampilkan kartu ringkasan gabungan (anggaran periode aktif, saldo berjalan, total pemakaian, sisa anggaran, jumlah voucher menunggu bukti, dan jumlah voucher menunggu pencairan) harus memanggil **tiga endpoint terpisah** (`GET /budget/current`, `GET /vouchers/summary`, dan menghitung sendiri sebagian angka dari daftar voucher) lalu menjumlahkannya sendiri di frontend. Ini melanggar prinsip yang sudah ditegakkan di seluruh modul ini (`PC-DES-005`, `PC-DES-025`): angka finansial **MUST** dihitung server, bukan diturunkan/dijumlahkan layar — risiko konkretnya, dua layar yang menjumlahkan dengan cara berbeda dapat menampilkan angka "sisa anggaran" yang saling tidak konsisten untuk periode yang sama.

---

## 2. Proses bisnis

**Pelaku.** Siapa pun yang memegang hak akses `PettyCashBudget : Read` (Kasir, Finance, Kepala Kasir/Finance Operations — lihat `permission-audit-matrix.md`).

**Pemicu.** Layar Petty Cash dibuka atau disegarkan.

**Langkah utama (`GET /budget/overview`):**

1. Sistem mengambil periode anggaran `ACTIVE` beserta `budgetAmount`, `currentBalance`, `remainingBudgetAmount`, dan `totalDisbursedAmount`-nya lewat `PettyCashBudgetService.GetCurrentAsync` yang sudah ada (`BE-BKC-054`).
2. Sistem menghitung jumlah voucher berstatus `CASH_RECEIVED` (`pendingEvidenceCount`, "menunggu bukti") dan `REQUESTED` (`pendingDisbursementCount`, "menunggu pencairan") langsung dari tabel voucher.
3. Kelima angka dan satu objek anggaran dikembalikan dalam **satu** response `PettyCashOverviewResponse`.

**Jalur tidak normal.** Belum ada periode anggaran `ACTIVE` sama sekali → `404`, perilaku yang sama seperti `GET /budget/current` yang sudah ada, karena `GetOverviewAsync` memanggil ulang method yang sama.

**Hasil akhir.** Layar Petty Cash dapat menyajikan seluruh kartu ringkasan dari satu panggilan jaringan, dengan angka yang **dijamin konsisten** karena berasal dari satu perhitungan server yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-058`: scope, dependency, acceptance, DoD |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md` baris 787–831 | Bentuk `PettyCashOverviewResponse` dan endpoint `GET /budget/overview` yang terkunci |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Verifikasi `GetCurrentAsync`, `CreatePeriodAsync`, `ActivatePeriodAsync`, `ClosePeriodAsync` (bukti `BE-BKC-054` sudah terimplementasi), pola query hitungan status |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` (`GetSummaryAsync`) | Pola reuse query hitungan status per roadmap kolom "Reuse" |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` | Konvensi action `GET` baca existing dan pemetaan exception |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs` | Bentuk `PettyCashBudgetResponse` existing yang dijadikan `ActiveBudget` |
| `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md` | Konfirmasi tidak perlu permission baru — `PettyCashBudget : Read` sudah ada |
| `git status --short`, `git log -1` pada repository backend | State Git saat ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Tambah `GetOverviewAsync` — reuse `GetCurrentAsync` untuk anggaran/saldo/sisa, ditambah dua `CountAsync` untuk status voucher, langsung lewat `ApplicationDbContext` (tanpa menyuntikkan `PettyCashVoucherService`, supaya tidak membentuk circular dependency dengan arah ketergantungan yang sudah ada) |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` | Tambah action `GetOverview` (`GET /overview`), memakai permission `PettyCashBudget : Read` yang sudah ada |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs` | Tambah `PettyCashOverviewResponse` (`ActiveBudget`, `PendingEvidenceCount`, `PendingDisbursementCount`, `TotalDisbursedThisPeriod`) |

Berkas lain yang tampak `M` pada `git status --short` sudah berubah sebelum sesi ini dimulai — bagian pekerjaan `BE-BKC-053`–`057` yang belum seluruhnya dilaporkan (lihat laporan `BE-BKC-057` bagian 7). Task ini tidak menyentuhnya.

**Catatan scope roadmap.** Kolom "Scope" kartu task ini juga menyebut "filter `budgetId` pada riwayat pergerakan" — pemeriksaan pada sesi ini menemukan filter itu **sudah ada** di `PettyCashBudgetMovementQuery`/`GetMovementsAsync` sejak `BE-BKC-054` (bukti: `api-contract.md` baris 792 sudah menandainya "Tersedia — filter `budgetId` baru"). Tidak ada perubahan tambahan yang diperlukan untuk butir ini pada task ini.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /budget/overview` diimplementasikan sesuai `BIL-API-1.1` |
| Database | `NOT APPLICABLE` — murni query baca, tidak ada perubahan schema/migration |
| Keamanan/Auth | `NOT APPLICABLE` — reuse permission `PettyCashBudget : Read` yang sudah ada, tidak ada permission baru |

---

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Billing Management / Petty Cash / Budget")]`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/overview` | Satu panggilan untuk seluruh kartu ringkasan halaman gabungan: anggaran periode aktif, saldo, total pemakaian, sisa anggaran, jumlah menunggu bukti, dan jumlah menunggu pencairan | `PettyCashBudget : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pengguna pada sesi ini: "tanpa build otomatis" |
| Review manual `GetOverviewAsync`, `GetOverview`, dan `PettyCashOverviewResponse` terhadap sintaks C# dan kecocokan tipe | Tidak ditemukan kesalahan | `PASS` (pengganti sementara untuk `dotnet build` yang tidak dijalankan — **bukan** pengganti penuh) | Pembacaan ulang `PettyCashBudgetService.cs` baris 29–58, `PettyCashBudgetController.cs` baris 37–49, `PettyCashBudgetDtos.cs` baris 116–131 pada sesi ini |
| Verifikasi kontrak API terhadap `api-contract.md` baris 790 dan 829 | Field `activeBudget`, `pendingEvidenceCount`, `pendingDisbursementCount`, `totalDisbursedThisPeriod` cocok nama dan tipe | `PASS` | Perbandingan manual DTO terhadap teks kontrak |
| Verifikasi proses bisnis: sisa anggaran dihitung server, bukan diturunkan layar | `RemainingBudgetAmount` dihitung di `PettyCashBudgetService.Map` (`BudgetAmount − TotalDisbursedAmount`), bukan di endpoint overview maupun frontend | `PASS` | Pembacaan `PettyCashBudgetService.cs` method `Map` |
| Permintaan HTTP sungguhan ke `GET /budget/overview` | Tidak dijalankan | `NOT RUN` | Migration `BE-BKC-053` belum diterapkan ke database manapun (`Up()` masih kosong), sehingga tidak ada environment yang dapat menjalankan aplikasi dengan skema lengkap |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi berjalan dan database dengan migration `BE-BKC-053` sudah diterapkan; keduanya belum tersedia pada sesi ini.

**Tidak dijalankan:** `dotnet build` (instruksi eksplisit pengguna) dan permintaan HTTP sungguhan (migration belum diterapkan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bentuk `PettyCashOverviewResponse` — keenam angka tersedia dalam satu panggilan | **Source ada, belum terbukti lewat request HTTP** | DTO dan service cocok kontrak (bagian 5), tetapi belum dijalankan sebagai request sungguhan |
| DoD: endpoint berjalan dan mengembalikan keenam angka | **Belum terbukti** | Sama seperti di atas |
| DoD: tidak ada angka kartu yang harus dijumlahkan layar | **Terpenuhi secara desain** | Seluruh angka dihitung `GetOverviewAsync`/`Map` di server; layar hanya menampilkan field response apa adanya |
| DoD: `dotnet build` lulus | **Belum terpenuhi** | Tidak dijalankan atas instruksi eksplisit pengguna |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Lihat bagian 7 |

Task ini **tidak dapat** ditandai `✅` pada roadmap. Blocker yang sama seperti `BE-BKC-057`: `dotnet build` belum dijalankan, dan migration `BE-BKC-053` belum diterapkan sehingga tidak ada database untuk menjalankan permintaan HTTP sungguhan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` belum pernah dijalankan untuk perubahan pada task ini — ada kemungkinan kecil kesalahan sintaks yang tidak tertangkap review manual |
| Masalah yang diketahui | Sama seperti dicatat pada laporan `BE-BKC-057`: `BE-BKC-053`/`054`/`055` sudah terimplementasi di source tetapi belum punya laporan tracked maupun tanda roadmap. Ditambah satu temuan baru pada task ini: `api-contract.md` menyebut "kelima angka" sedangkan DoD roadmap menyebut "keenam angka" untuk bentuk respons yang sama — selisih penghitungan, bukan selisih field (dicatat pada Backend Governance Preflight) |
| Risiko tersisa | Selama migration belum diterapkan dan `dotnet build` belum lulus, endpoint ini **tidak dapat dipakai** — risiko murni "belum bisa dipakai", bukan risiko data salah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` — pengguna memang meminta sejak awal task ini dikerjakan tanpa build otomatis |
| Status Git | Ditambahkan pada task ini: 3 berkas source (lihat bagian 3.2) plus laporan ini. Berkas lain yang tampak `M` sudah berubah sebelum sesi ini (lihat laporan `BE-BKC-057`) |
| Langkah berikutnya | (1) Selesaikan regenerasi migration `BE-BKC-053`; (2) jalankan `dotnet build` dan catat hasilnya menggantikan baris `NOT RUN`; (3) terapkan migration ke database pengembangan dan jalankan `GET /budget/overview` sebagai request HTTP sungguhan; (4) setelah lulus, roadmap `BE-BKC-058` boleh ditandai `✅`; (5) laporkan ke pemilik blueprint selisih "kelima" vs "keenam angka" pada `api-contract.md` untuk diperjelas; (6) backfill laporan tracked `BE-BKC-053`–`055` |
