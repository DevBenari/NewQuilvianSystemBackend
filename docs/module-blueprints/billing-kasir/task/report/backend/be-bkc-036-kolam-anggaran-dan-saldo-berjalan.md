# BE-BKC-036 — Kolam anggaran dan saldo berjalan Petty Cash

- TASK ID: `BE-BKC-036`
- TASK TYPE: Implementasi backend — service dan endpoint transaksional baru di atas tabel yang sudah ada
- COMPLEXITY: `HEAVY` (skor 10 — repository 0, berkas diperiksa >20 → 2, berkas diubah 8 → 2, logika bisnis kompleks/concurrency+ledger → 2, kontrak API memakai kontrak yang sudah dikunci → 1, database dampak perilaku persistence signifikan pada tabel existing → 1, keamanan mengaitkan dua permission baru mengikuti pola baku → 1, UI/workflow tidak ada → 0, dinaikkan satu tingkat karena dua faktor jatuh di tingkat berikutnya — logika bisnis dan berkas diubah)
- CLASSIFICATION SCORE: 10
- MODEL: Claude Sonnet 5
- TASK MODE: `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina`
- WRITE TARGET: Source backend (`Areas/HealthServices/BillingManagement/PettyCash/**`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`), test backend (`Tests/QuilvianSystemBackend.UnitTests.InMemory/**`), dan laporan task ini

## 1. Apa yang dikerjakan dan kenapa

**Tujuan bisnis.** Finance perlu bisa menambah dan mengoreksi saldo kas kecil beserta alasannya,
dan siapa pun yang berwenang perlu bisa membaca saldo berjalan kapan saja — angka yang tampil
sebagai kartu "TOTAL PETTY CASH" di layar. Setiap pergerakan saldo (nambah, koreksi, atau
menyerahkan uang voucher) harus meninggalkan satu baris riwayat yang menjelaskan **kenapa**
saldo berubah, sehingga pertanyaan "kenapa saldo berkurang Rp 300.000 bulan lalu" selalu
terjawab.

**Yang sudah ada sebelumnya.** Dari `BE-BKC-033`: tabel `BilPettyCashBudget` (satu baris
`HOSPITAL_MAIN`, saldo awal `0`) dan `BilPettyCashBudgetMovement` (ledger kosong). Dari
`BE-BKC-035`: kategori pengeluaran sudah bisa dikelola Finance. Tidak ada satu pun endpoint atau
service yang menyentuh saldo sebelum task ini.

**Yang ditambahkan task ini.** `PettyCashBudgetService` — **satu-satunya** kode yang boleh
mengubah `BilPettyCashBudget.CurrentBalance` — beserta empat endpoint publik dan satu method
internal yang akan dipanggil task berikutnya:

1. `GET /current` — melihat saldo berjalan, nominal yang sudah dijanjikan ke voucher yang
   disetujui (`reservedAmount`), dan sisa yang benar-benar bisa dipakai (`availableAmount`).
2. `GET /movements` — riwayat lengkap: kapan, berapa, kenapa, dan siapa pelakunya.
3. `POST /top-ups` — Finance menambah saldo beserta alasannya.
4. `POST /adjustments` — Finance mengoreksi saldo naik atau turun beserta alasannya, tanpa
   pernah menghapus riwayat lama.
5. `ApplyDisbursementAsync` (internal, **bukan** endpoint) — dipanggil task berikutnya
   (`BE-BKC-037`) saat kasir menekan "Uang Diberikan"; mengurangi saldo dan menulis baris ledger
   `DISBURSEMENT`. Ditulis sekarang karena `PettyCashBudgetService` adalah **satu-satunya**
   penulis saldo yang berwenang — voucher lifecycle (`BE-BKC-037`) hanya boleh memanggilnya, tidak
   boleh menulis saldo sendiri.

## 2. Proses bisnis

**Pelaku.** Finance (pengelola anggaran) — satu-satunya peran berwenang menambah dan mengoreksi
saldo (`contracts/permission-audit-matrix.md` § Resource `PettyCashBudget`). Kasir dan siapa pun
yang berwenang membaca voucher juga boleh **melihat** saldo (`Read`), tetapi tidak boleh
mengubahnya.

**Pemicu.** Finance menambah saldo saat kas kecil perlu diisi ulang (top-up), atau mengoreksi
saldo saat penghitungan fisik uang di laci berbeda dari catatan sistem (adjustment).

**Prasyarat.** Kolam anggaran `HOSPITAL_MAIN` sudah ada (dari `BE-BKC-033`). Task ini tidak
membuat kolam baru — pada rilis ini hanya ada tepat satu kolam untuk seluruh rumah sakit.

**Langkah utama — top-up:**

1. Finance mengisi nominal dan alasan penambahan (contoh: "Modal awal kas kecil bulan
   September").
2. Sistem menolak bila nominal kosong/nol/negatif atau alasan kosong, dengan pesan "Nominal dan
   alasan wajib diisi, dan nominal harus lebih besar dari nol." (kode `400`).
3. Sistem menolak bila data saldo yang dibaca layar sudah usang (petugas lain sudah mengubahnya
   lebih dulu) — muncul pesan "Data telah berubah. Muat ulang sebelum melanjutkan." (kode `409`).
4. Bila lolos, saldo bertambah, satu baris riwayat baru tercatat bertipe `TOP_UP`, dan angka
   `TotalTopUpAmount` (akumulasi untuk pelaporan) ikut bertambah.

**Langkah utama — koreksi (adjustment):**

1. Finance memilih arah (`INCREASE` menaikkan atau `DECREASE` menurunkan), mengisi nominal
   (selalu angka positif — arahnya ditentukan pilihan, bukan tanda minus), dan alasan.
2. Untuk koreksi **turun**, sistem memeriksa dua syarat sekaligus: hasil akhirnya tidak boleh di
   bawah nol, **dan** tidak boleh di bawah nominal yang sudah dijanjikan ke voucher yang sudah
   disetujui tetapi belum diserahkan uangnya (`reservedAmount`). Keduanya digabung supaya Finance
   tidak bisa diam-diam mengingkari janji yang sudah dibuat ke voucher yang sudah disetujui.
3. Bila salah satu syarat gagal, sistem menolak dengan pesan "Koreksi ini akan membuat saldo kas
   kecil tidak mencukupi untuk voucher yang sudah disetujui. Sisa yang sudah dijanjikan Rp
   {reservedAmount}." (kode `422`).
4. Koreksi **naik** tidak punya batas atas selain batas nominal maksimum sistem — menaikkan
   saldo tidak pernah mengingkari janji apa pun.

**Contoh berangka — koreksi ditolak (BIL-VAL-054).** Saldo kas kecil Rp 5.000.000. Satu voucher
Rp 4.800.000 sudah `Disetujui` (statusnya `APPROVED`) tetapi uangnya belum diserahkan, sehingga
`reservedAmount` Rp 4.800.000. Finance mencoba mengoreksi turun Rp 4.900.000 karena penghitungan
ulang fisik menemukan selisih. Hasilnya seharusnya Rp 100.000 — jauh di bawah Rp 4.800.000 yang
sudah dijanjikan ke voucher itu. Sistem **menolak** koreksi ini sepenuhnya; saldo tetap Rp
5.000.000 sampai Finance memasukkan nominal yang tidak melanggar janji tersebut.

**Perubahan status/nilai:**

| Aksi | Saldo bergerak? | Ledger yang ditulis | Siapa yang boleh |
| --- | :---: | --- | --- |
| Top-up | Ya, bertambah | `TOP_UP` | Finance |
| Koreksi naik | Ya, bertambah | `ADJUSTMENT` | Finance |
| Koreksi turun | Ya, berkurang — ditolak bila melanggar janji | `ADJUSTMENT` | Finance |
| Penyerahan uang voucher (`ApplyDisbursementAsync`, dipakai `BE-BKC-037`) | Ya, berkurang — ditolak bila saldo tidak cukup | `DISBURSEMENT` | Kasir (lewat endpoint disburse `BE-BKC-037`) |

**Jalur tidak normal.**

- Tombol top-up/koreksi tertekan dua kali dengan `Idempotency-Key` yang sama: permintaan kedua
  mengembalikan hasil permintaan pertama tanpa menambah saldo dua kali.
- `Idempotency-Key` kosong: ditolak `400` sebelum menyentuh database sama sekali.
- Penyerahan uang voucher yang sama dipanggil dua kali (`ApplyDisbursementAsync`): percobaan
  kedua ditolak `409` — satu voucher paling banyak satu pengurangan saldo, dijaga aplikasi
  **dan** oleh index database yang sudah dibuat `BE-BKC-033`.
- Penyerahan uang saat saldo sudah tidak cukup (misalnya saldo dikoreksi turun setelah voucher
  disetujui): ditolak `422`, voucher tetap berstatus `Disetujui` dan bisa dicoba lagi setelah
  saldo ditambah.

**Hasil akhir.** Saldo kas kecil selalu mencerminkan jumlah uang yang benar-benar ada, setiap
perubahannya punya alasan tertulis dan pelaku yang jelas, dan janji ke voucher yang sudah
disetujui tidak pernah bisa diingkari diam-diam lewat koreksi.

## 3. Health Services / Billing Management / Petty Cash / Budget

Base URL: `api/v1/health-services/billing-management/petty-cash/budget`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/current` | Saldo berjalan, nominal yang sudah dijanjikan, dan sisa yang bisa dipakai — sumber angka kartu "TOTAL PETTY CASH" | `PettyCashBudget : Read` | – | `ApiResponse<PettyCashBudgetResponse>` |
| `GET` | `/movements` | Riwayat pergerakan anggaran: penambahan, koreksi, dan pencairan | `PettyCashBudget : Read` | Query `movementType`, `startDate`, `endDate`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<PettyCashBudgetMovementResponse>>` |
| `POST` | `/top-ups` | Finance menambah anggaran kas kecil beserta alasannya | `PettyCashBudget : TopUp` | Header `Idempotency-Key`; body `PettyCashBudgetTopUpRequest` | `ApiResponse<PettyCashBudgetResponse>` |
| `POST` | `/adjustments` | Finance mengoreksi saldo naik/turun beserta alasannya | `PettyCashBudget : Adjust` | Header `Idempotency-Key`; body `PettyCashBudgetAdjustmentRequest` | `ApiResponse<PettyCashBudgetResponse>` |

**Kode status yang mungkin muncul:**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Permintaan berhasil |
| `400` | Nominal atau alasan tidak diisi/tidak valid, atau `Idempotency-Key` kosong (`BIL-VAL-055`) |
| `404` | Kolam anggaran aktif tidak ditemukan (seharusnya tidak pernah terjadi pada rilis ini — hanya satu kolam) |
| `409` | Saldo sudah diubah petugas lain; muat ulang sebelum melanjutkan |
| `422` | Koreksi turun akan membuat saldo negatif atau melanggar janji ke voucher yang sudah disetujui (`BIL-VAL-054`) |

Bentuk `PettyCashBudgetTopUpRequest`: `amount` (angka, wajib, lebih besar dari `0`, maksimal 2
desimal), `reason` (teks, wajib, 1–500 karakter), `expectedRowVersion` (nilai `rowVersion` yang
dibaca layar sebelumnya, wajib). `PettyCashBudgetAdjustmentRequest` berbentuk sama ditambah
`direction` (`INCREASE` atau `DECREASE`).

Bentuk `PettyCashBudgetResponse`: `poolCode`/`poolName` (`HOSPITAL_MAIN`), `currentBalance`
(saldo — "TOTAL PETTY CASH"), `reservedAmount` (dijanjikan ke voucher disetujui, dihitung
server), `availableAmount` (`currentBalance − reservedAmount`), `totalTopUpAmount`/
`totalDisbursedAmount` (akumulasi pelaporan), `lastMovementAt`, `rowVersion`.

## 4. Berkas yang diperiksa

Governance dan kontrak: `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`,
`rules/backend/REVIEW_RULES.md`, `rules/rule-output/lokasi-laporan-task.md`,
`rules/rule-output/aturan-output-dokumentasi.md`.

Blueprint modul: `roadmap/backend-roadmap.md` (kartu `BE-BKC-036`), `02-backend-architecture.md`
(bagian `PettyCashBudgetService`, "Perpindahan status — ringkasan bagi implementer", "Security,
privacy, exception, dan concurrency"), `contracts/api-contract.md` (§ Petty Cash / Budget),
`contracts/validation-matrix.md` (`BIL-VAL-054`, `BIL-VAL-055`, konteks `BIL-VAL-047`/`048`),
`contracts/permission-audit-matrix.md` (Resource `PettyCashBudget`), `data/data-dictionary.md`
(§ `BilPettyCashBudgetMovement` — kolom `Reason` Sensitif).

Model dan konfigurasi existing: `BilPettyCashBudget.cs`, `BilPettyCashBudgetMovement.cs`,
`BilPettyCashVoucher.cs`, `BilPettyCashBudgetConfiguration.cs` (constraint saldo `>= 0`, unique
singleton `Status = ACTIVE`), `BilPettyCashBudgetMovementConfiguration.cs` (unique index parsial
`(VoucherId)` untuk `DISBURSEMENT`, check constraint `Reason` wajib untuk `TOP_UP`/`ADJUSTMENT`),
`Repositories/ApplicationDbContext.cs` (nama `DbSet`).

Pola implementasi terdekat yang dipakai apa adanya: `BillingDepositService.cs` (transaction
`Serializable` + `pg_advisory_xact_lock` + replay `Idempotency-Key` + audit before/after —
diikuti persis, kecuali payload-hash mismatch detection yang **tidak** direplikasi karena
`BilPettyCashBudgetMovement` sengaja tidak punya kolom `PayloadHash`, berbeda dari
`BilDepositMovement`), `BillingDiscountService.cs` (pola resolusi nama pengguna terbaca dari
`ActorUserId` lewat tabel `Users`), `TaxRuleService.cs`/`RoomChargePolicyService.cs` (pola
exception dan `Normalize` helper).

## 5. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs` | **Baru.** Query, request, dan response DTO |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | **Baru.** `GetCurrentAsync`, `GetMovementsAsync`, `TopUpAsync`, `AdjustAsync`, `CalculateReservedAmountAsync`, `ApplyDisbursementAsync` |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` | **Baru.** Empat endpoint |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | **Diubah.** `using` baru + satu baris `services.AddScoped<PettyCashBudgetService>();` |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashBudgetServiceTests.cs` | **Baru.** 17 unit test domain |

Tidak ada model, migration, atau perubahan schema — task ini murni menambah service dan endpoint
di atas tabel `BilPettyCashBudget`/`BilPettyCashBudgetMovement` yang sudah dibuat `BE-BKC-033`.

## 6. Implementasi

- **Kunci penasihat.** `SELECT pg_advisory_xact_lock(hashtext('BIL_PETTY_CASH_BUDGET_HOSPITAL_MAIN'));`
  diambil di awal setiap transaction yang menyentuh saldo (`TopUpAsync`, `AdjustAsync`) maupun di
  awal `ApplyDisbursementAsync` — hanya dijalankan pada provider relational (`IsRelational()`);
  provider `InMemory` yang dipakai unit test melompatinya, sesuai pola `BillingNumberSeriesService`
  yang sudah ada.
- **`ApplyDisbursementAsync` sengaja TIDAK membuka transaction sendiri dan TIDAK memanggil
  `SaveChangesAsync`.** Ia hanya menandai perubahan pada entity yang sudah tracked dalam
  `ApplicationDbContext` yang sama; pemanggilnya (`PettyCashVoucherService` pada `BE-BKC-037`)
  bertanggung jawab membuka transaction dan menyimpannya bersama perubahan voucher dalam satu
  `SaveChangesAsync`. Ini murni mengikuti kontrak desain `PC-DES-004` — belum ada pemanggil nyata
  pada task ini karena `PettyCashVoucherService` belum ditulis.
- **Idempotensi tanpa payload hash.** `BilDepositMovement` (pola rujukan) punya kolom
  `PayloadHash` untuk mendeteksi permintaan idempotent yang isinya berbeda; `BilPettyCashBudgetMovement`
  **tidak** punya kolom itu (dikonfirmasi dari model dan `data/data-dictionary.md`). Karena itu
  `TopUpAsync`/`AdjustAsync` mendeteksi replay murni dari `IdempotencyKey` tanpa verifikasi
  kecocokan payload — proporsional dengan skema yang ada, **tidak** menambah kolom baru di luar
  wewenang task ini (perubahan schema adalah wewenang terpisah).
- **`CalculateReservedAmountAsync` publik** (bukan privat) supaya bisa dipanggil langsung dari
  test tanpa melalui HTTP, dan supaya `BE-BKC-037` (penjaga persetujuan `PC-DES-005`) dapat
  memanggilnya langsung tanpa duplikasi logika query.
- **`Reason` tidak masuk payload custom logger** (`AuditMovementAsync`) — kolom ini ditandai
  Sensitif pada `data/data-dictionary.md`. Yang masuk log: `BudgetId`, `PoolCode`, `MovementId`,
  `MovementType`, nominal, saldo sebelum/sesudah, `IdempotencyKey`, `CorrelationId`,
  `ActorUserId` — seluruhnya boleh sesuai `02-backend-architecture.md` § Privasi.
- **Nama pelaku terbaca manusia** (`ActorName`) pada `GetMovementsAsync` diresolusi lewat tabel
  `Users` (`DisplayName ?? UserName ?? Email ?? UserCode`), bukan menampilkan UUID mentah —
  mengikuti `BillingDiscountService.GetDoctorDiscountApprovalsAsync`.

## 7. Backend Governance Preflight

| Aspek | Nilai |
| --- | --- |
| Area | HealthServices |
| Module | BillingManagement / Billing |
| Submodule | PettyCash (mengikuti baris registry `HealthServices / BillingManagement / Billing` / `Bil` — sama seperti `BE-BKC-033`–`035`, lihat catatan `PC-OQ-003` pada `BE-BKC-033`) |
| Prefix entity | `Bil` — tidak ada entity baru dibuat task ini; `BilPettyCashBudget`/`BilPettyCashBudgetMovement` sudah ada dan bagian dari registry `ACTIVE` sejak `BE-BKC-033` |
| Keberlakuan | `NEW CODE` — service, controller, DTO baru di atas model yang sudah disetujui |
| Status registry | `ACTIVE` (baris `HealthServices / BillingManagement / Billing`); tidak ada `QBE-MOD-002` yang menahan task ini karena tidak ada model persisted baru |
| QBE ID yang berlaku | `QBE-SVC-001` (controller dilarang akses `ApplicationDbContext` langsung) — dipatuhi; `QBE-CODE-003` (dilarang mekanisme penomoran/ID karangan) — tidak relevan, task ini tidak membuat nomor apa pun, hanya `CorrelationId`/`IdempotencyKey` yang sudah jadi bagian skema |

## 8. Dampak kontrak API

Mengimplementasikan empat endpoint yang sudah dikunci di `contracts/api-contract.md` § Petty
Cash Budget (revisi kontrak `BIL-API-0.9`, status **DESIGN_APPROVED** menurut
`blueprint-manifest.md`). `ApplyDisbursementAsync` **bukan** endpoint — murni method internal
yang menunggu pemanggil dari `BE-BKC-037`. Tidak ada perubahan pada endpoint modul Billing lain.

## 9. Dampak database

Tidak ada migration baru, tidak ada perubahan schema. Task ini membaca dan menulis ke tabel
`BilPettyCashBudget` dan `BilPettyCashBudgetMovement` yang sudah dibuat migration
`20260907062238_AddTablePettyCashModule` milik `BE-BKC-033`, dan membaca (tanpa menulis) tabel
`BilPettyCashVoucher` untuk `CalculateReservedAmountAsync` dan penjaga `ApplyDisbursementAsync`.

## 10. Dampak keamanan

Resource `PettyCashBudget` dengan tiga permission (`Read`, `TopUp`, `Adjust`) — sesuai
`contracts/permission-audit-matrix.md`. `Reason` pada ledger ditandai Sensitif dan dikecualikan
dari custom logger (lihat § 6). Tidak ada data pasien yang tersentuh sama sekali pada rumpun
Petty Cash (`02-backend-architecture.md` § Privasi).

- VISUAL REFERENCE: NOT REQUIRED (tidak ada perubahan UI pada task backend ini)

## 11. Validasi

| Perintah/pemeriksaan | Hasil | Klasifikasi | Bukti/catatan |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan sesi ini | `NOT RUN` | Sesuai instruksi baku pengguna: build/test backend dijalankan manual oleh pengguna, bukan otomatis oleh sesi |
| `dotnet test --filter PettyCashBudgetServiceTests` | Tidak dijalankan sesi ini | `NOT RUN` | Sama seperti di atas — menunggu pengguna menjalankan dan melaporkan hasilnya |
| Review diff/scope | Dilakukan | `PASS` | `git status --short` menunjukkan hanya berkas dalam lingkup Petty Cash Budget (plus laporan/roadmap) yang tersentuh |
| Review kesesuaian QBE | Dilakukan | `PASS` | `QBE-SVC-001` dipatuhi; tidak ada model persisted baru sehingga `QBE-MOD-002`/`003` tidak berlaku |
| Pemeriksaan rahasia | Dilakukan | `PASS` | Tidak ada credential/token/connection string pada berkas yang berubah; `Reason` sensitif sengaja dikecualikan dari audit log (lihat § 6) |

17 unit test domain ditulis pada `PettyCashBudgetServiceTests.cs`, mencakup: `GetCurrentAsync`
(saldo/reserved/available), top-up (berhasil, `RowVersion` usang → `409`, replay
`Idempotency-Key` tidak dobel, nominal ≤ 0 → `400`, alasan kosong → `400`, `Idempotency-Key`
kosong → `400`), adjustment (naik, turun dalam batas, turun melanggar `reservedAmount` → `422`
dengan contoh berangka dari arsitektur, turun di bawah nol → `422`, arah tidak valid → `400`),
`CalculateReservedAmountAsync` (hanya voucher `APPROVED` aktif yang dihitung — `WAITING_APPROVAL`/
`COMPLETED`/dibatalkan dikecualikan), `ApplyDisbursementAsync` (berhasil mengurangi saldo dan
menulis `DISBURSEMENT`, saldo tidak cukup → `422`, voucher yang sudah dicairkan → `409`),
`GetMovementsAsync` (filter tipe pergerakan case-insensitive, urutan terbaru lebih dulu), dan
resolusi dependency injection `PettyCashBudgetService` lewat `AddBillingManagement()`. **Belum
ada satu pun yang benar-benar dieksekusi** — status di atas menunggu pengguna menjalankan
`dotnet test` secara manual. Kunci `pg_advisory_xact_lock` sendiri hanya dapat dibuktikan pada
provider PostgreSQL — di luar jangkauan unit test `InMemory`.

- MANUAL TEST: NOT APPLICABLE (task backend murni, tidak ada UI untuk diuji manual)

## 12. Peringatan dan risiko yang tersisa

- Test dan build **belum diverifikasi berjalan** sesi ini. Task ini **belum boleh ditandai
  selesai** sampai pengguna menjalankan `dotnet build`/`dotnet test` dan hasilnya dilaporkan
  kembali.
- **`ApplyDisbursementAsync` belum punya pemanggil nyata.** Method ini benar secara desain dan
  diuji lewat harness transaction buatan test (lihat § 11), tetapi kebenarannya di jalur produksi
  baru benar-benar terbukti setelah `BE-BKC-037` memanggilnya dari dalam transaction `DisburseAsync`
  miliknya sendiri sesuai `PC-DES-004`.
- **Idempotensi top-up/adjustment tanpa payload hash** (lihat § 6) berarti dua permintaan dengan
  `Idempotency-Key` sama tetapi **isi berbeda** akan diam-diam mengembalikan hasil permintaan
  pertama, bukan ditolak sebagai konflik — beda dari `BillingDepositService`. Ini keterbatasan
  skema yang ada, bukan bug; didokumentasikan eksplisit sebagai catatan terbuka, bukan
  diselesaikan sepihak dengan menambah kolom baru di luar wewenang task ini.
- `BE-BKC-033` (tabel dasar) masih 🟡 pada roadmap menyangkut bukti seed dan review Finance —
  tidak menghalangi source task ini, tetapi wajib tertutup sebelum gelombang `MVP-13`/`14`
  dinyatakan naik.

## 13. Perubahan sampingan

- INCIDENTAL CHANGES: NONE — seluruh berkas yang berubah berada dalam lingkup task ini.

## 14. Interupsi

- INTERRUPTIONS: NONE — task dikerjakan dalam satu sesi berkelanjutan tanpa interupsi.

## 15. Status Git

```
 M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
 M docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md
 M docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md
?? Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs
?? Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs
?? Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashBudgetServiceTests.cs
```

(Baris lain pada `git status --short` sesi ini berasal dari task `BE-BKC-035` yang belum
di-commit, bukan bagian task ini.) Belum di-stage maupun di-commit. Branch `Yasmina`.

## 16. Langkah berikutnya yang disarankan

1. Pengguna menjalankan `dotnet build` dan `dotnet test` secara manual untuk `BE-BKC-035` dan
   `BE-BKC-036` sekaligus, lalu melaporkan hasil sebenarnya (jumlah lulus/gagal).
2. Setelah build/test terbukti lulus, perbarui tabel status pada `roadmap/backend-roadmap.md`
   dan `roadmap/requirement-traceability.md`, menautkannya ke laporan ini.
3. Lanjutkan ke `BE-BKC-037` (siklus hidup voucher penuh) — task itu memanggil
   `AllocatePettyCashVoucherNumberAsync` (`BE-BKC-034`), `PettyCashCategoryService` (`BE-BKC-035`
   untuk validasi kategori aktif), dan `CalculateReservedAmountAsync`/`ApplyDisbursementAsync`
   (`BE-BKC-036`, task ini) — ketiganya sudah tersedia.

- KNOWN ISSUES: Idempotensi top-up/adjustment tanpa payload hash (§ 12); tidak ada yang lain
  ditemukan pada scope task ini.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
