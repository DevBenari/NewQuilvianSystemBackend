# Laporan Perubahan Backend — `BE-FIN-051`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-051` |
| Judul | Header `Idempotency-Key` ditegakkan pada perintah uang rumpun Purchasing |
| Slice | `POST-MVP` — **task baru**, bukan hasil `/plan-module-delivery`. Diotorisasi eksplisit oleh pemilik repository setelah laporan `FE-FIN-008` mencatat backend sama sekali tidak membaca header ini di lima controller Purchasing |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task, baris `BE-FIN-051`, ditambahkan task ini) |
| Trace | `FIN-DES-006` (`api-contract.md` baris 26: "Perintah yang memindahkan uang — Wajib header `Idempotency-Key`"); `contracts/api-contract.md` §B.1-B.5 |
| Contract version | `FIN-API-1.1` — nol perubahan kontrak; kolom Request "Idempotency-Key" yang sudah tercantum sejak awal kini ditegakkan |
| Dependency | `BE-FIN-032`..`035` ✅ (kelima controller Purchasing sudah berjalan dan dipakai ulang polanya) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca (5 controller, kontrak, laporan `FE-FIN-008`, `PettyCashBudgetService` sebagai rujukan pola) (skor 1); 8 berkas diubah/dibuat (skor 1); logika bisnis sedang — cek-replay/simpan lintas 14 endpoint, desain ledger baru (skor 1); kontrak API — nol endpoint baru, field header yang sudah ada kini ditegakkan (skor 0); database — satu tabel baru, nol migration dieksekusi (skor 1); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — tidak ada (skor 0). Total 4 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/{Models,Services,Controllers}/**` (baru+diubah), `Repositories/ApplicationDbContext.cs` (`DbSet` baru), `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/**` (baru), `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI), `docs/module-blueprints/finance-management/roadmap/**` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `yasmina` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI 30 September 2026.** `dotnet build` PASS 0 error dikonfirmasi pengguna; migration `TableFinPurchasingIdempotencyRecord` dieksekusi ke database dan berjalan lancar (dikonfirmasi pengguna). Ledger idempotensi aktif untuk 14 aksi lintas 5 controller Purchasing |

---

## 1. Masalah yang diperbaiki

Laporan `FE-FIN-008` (frontend, sesi terpisah) mencatat bahwa kontrak `api-contract.md` mensyaratkan
header `Idempotency-Key` pada setiap "perintah yang memindahkan uang" (`FIN-DES-006`), tetapi
kelima controller Purchasing (`FinancePurchaseOrdersController`, `FinanceGoodsReceiptsController`,
`FinanceInvoiceExchangesController`, `FinancePurchasingInvoicesController`,
`FinanceSupplierReturnsController`) **nol referensi** ke header ini sama sekali — dikonfirmasi
lewat pencarian langsung di source sebelum task ini dimulai. Tanpa penegakan ini, klien yang
mengulang permintaan setelah timeout jaringan (padahal permintaan pertama sebenarnya berhasil)
akan memicu pemrosesan kedua kalinya — PO/GR/Tukar Faktur/Purchasing Invoice/Retur ganda, atau
`422` yang membingungkan karena entity sudah berpindah status.

**Contoh konkret.** Petugas AP menekan "Ajukan PO" tepat saat koneksi terputus. Klien tidak tahu
apakah permintaannya sampai. Tanpa `Idempotency-Key`, menekan ulang tombol akan memicu
`SubmitAsync` kedua kalinya — bila permintaan pertama sebenarnya sudah berhasil, permintaan kedua
akan ditolak `422` ("PO harus berstatus DRAFT untuk diajukan") karena PO sudah `PENDING_APPROVAL`,
membingungkan petugas yang mengira aksinya gagal total. Dengan task ini, permintaan kedua yang
membawa `Idempotency-Key` yang sama persis akan menerima kembali respons sukses pertama, apa
adanya.

---

## 2. Proses bisnis

**Pelaku:** Sistem (transparan bagi petugas AP — tidak ada UI baru; frontend yang mengirim header
ini adalah tindak lanjut terpisah, lihat bagian 7).

**Langkah normal (per permintaan):**

1. Klien mengirim `POST` beserta header `Idempotency-Key: <Guid>` pada salah satu dari 14 aksi
   (lihat bagian 4).
2. Controller memanggil `PurchasingIdempotencyService.TryReplayAsync` — mengecek apakah kunci ini
   sudah pernah dipakai (`FinPurchasingIdempotencyRecord`).
3. **Bila sudah pernah** (replay): controller langsung mengembalikan `ResponseBody` dan
   `ResponseStatusCode` yang tersimpan **persis** seperti permintaan pertama — nol pemrosesan
   ulang, nol pemanggilan service bisnis.
4. **Bila belum pernah**: controller memanggil service bisnis seperti biasa (nol perubahan pada
   lima service). Bila berhasil, controller memanggil `PurchasingIdempotencyService.SaveAsync`
   untuk merekam `(IdempotencyKey, EntityType, Action, EntityId, StatusCode, ResponseBody)`.

**Jalur tidak normal:**

- **Header tidak dikirim sama sekali:** `[ApiController]` menolak otomatis `400` (model binding
  gagal untuk parameter `Guid` wajib yang headernya tidak ada) — nol kode tambahan diperlukan.
- **Dua permintaan bersamaan ber-kunci sama** (race, misalnya double-click sebelum debounce klien
  bekerja): keduanya lolos `TryReplayAsync` (belum ada baris saat keduanya mengecek), keduanya
  memanggil service bisnis (di sinilah penjaga status entity yang sudah ada — mis. "PO harus
  DRAFT" — menjadi lapis pertahanan kedua), lalu keduanya mencoba `SaveAsync`; indeks unik
  `IdempotencyKey` membuat penulis kedua gagal `DbUpdateException`, yang **sengaja ditangkap dan
  diabaikan** (penulis pertama menang, baris itulah yang sah).
- **Aksi gagal** (validasi bisnis, konflik, dll.): baris ledger **tidak pernah ditulis** — hanya
  jalur sukses yang direkam, sehingga permintaan ulang dengan kunci yang sama setelah kegagalan
  akan diproses ulang dari awal (bukan me-replay kegagalan lama, yang mungkin sudah tidak relevan
  bila state entity berubah).

**Hasil akhir:** permintaan yang benar-benar diproses ulang (retry sah setelah timeout) tidak
pernah menghasilkan efek samping ganda pada aksi yang sama; permintaan dengan kunci berbeda tetap
diproses independen seperti biasa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/task/report/frontend/FE-FIN-008.md` bagian 1 "Lanjutan
  kedua" — analisis gap yang menjadi dasar task ini
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.1-B.5 — baris mana
  yang benar-benar mensyaratkan `Idempotency-Key` (Create/Submit/Approve/Cancel; **bukan**
  `Reject`/`PUT` Update)
- `Areas/Corporate/FinanceManagement/PettyCash/Services/PettyCashBudgetService.cs`,
  `Controllers/PettyCashBudgetController.cs` — pola nyata satu-satunya di repository yang sudah
  menegakkan `Idempotency-Key` (kolom pada baris "movement") — dibaca sebagai rujukan sintaks
  `[FromHeader]`, **tidak ditiru** untuk penyimpanan (lihat bagian 3.3)
- Kelima controller dan service Purchasing (`FinancePurchaseOrders(Service)`,
  `FinanceGoodsReceipts(Service)`, `FinanceInvoiceExchanges(Service)`,
  `FinancePurchasingInvoices(Service)`, `FinanceSupplierReturns(Service)`) — dibaca penuh untuk
  memastikan nol `[FromHeader]` sebelumnya, dan untuk menetapkan titik sisip yang tidak mengubah
  logika bisnisnya
- `Repositories/ApplicationDbContext.cs` — pola `DbSet` dan `ApplyConfigurationsFromAssembly`
  (auto-discovery, nol registrasi manual per konfigurasi)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
  — lokasi registrasi DI kelima service Purchasing yang sudah ada

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingIdempotencyRecord.cs` | **Baru.** Model ledger + `FinPurchasingIdempotencyEntityTypes`/`Actions` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingIdempotencyRecordConfiguration.cs` | **Baru.** Konfigurasi EF — unique index `IdempotencyKey`, check constraint `EntityType`/`Action` |
| `Repositories/ApplicationDbContext.cs` | Tambah `DbSet<FinPurchasingIdempotencyRecord> FinPurchasingIdempotencyRecords` |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/PurchasingIdempotencyService.cs` | **Baru.** `TryReplayAsync`, `SaveAsync` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Tambah `services.AddScoped<PurchasingIdempotencyService>();` |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchaseOrdersController.cs` | `Create`/`Submit`/`Approve`/`Cancel` diberi header + cek/simpan; `Reject`/`Update` **tidak disentuh** |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceGoodsReceiptsController.cs` | `Create`/`Cancel` diberi header + cek/simpan |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceInvoiceExchangesController.cs` | `Create`/`Cancel` diberi header + cek/simpan |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs` | `Create`/`Submit`/`Approve` diberi header + cek/simpan; `Reject`/`Update` **tidak disentuh** |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs` | `Create`/`Confirm`/`Cancel` diberi header + cek/simpan |

**Nol berkas service Purchasing (`FinancePurchaseOrderService.cs`, dst.) diubah** — seluruh
penyisipan terjadi di lapisan controller, persis desain bagian 3.3.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan kontrak.** Header `Idempotency-Key` sudah tercantum di `api-contract.md` §B.1-B.5 sejak `FIN-API-1.1` dikunci — task ini menegakkannya, bukan menambah kontrak baru. Response bentuknya identik seperti sebelumnya untuk permintaan baru; untuk replay, response persis salinan permintaan pertama |
| Database | **Satu tabel baru** `FinPurchasingIdempotencyRecord` (skema `public`), unique index pada `IdempotencyKey`, check constraint `EntityType`/`Action`. Migration `TableFinPurchasingIdempotencyRecord` (20260930080625) dieksekusi ke database — dikonfirmasi pengguna berjalan lancar |
| Keamanan/Auth | **Nol resource/action baru.** Endpoint yang disentuh memakai `[AccessPermission]` yang sudah ada persis seperti sebelumnya — header `Idempotency-Key` bukan mekanisme otorisasi, murni deduplikasi permintaan. Nol `IsInRole`/nama peran hardcode |

**Keputusan desain (diotorisasi eksplisit pemilik repository):** ledger idempotensi terpisah
(`FinPurchasingIdempotencyRecord`), **bukan** kolom `IdempotencyKey` per-aksi pada lima entity
Purchasing. Alasan: setiap entity punya beberapa aksi terpisah yang masing-masing butuh dedup
sendiri sepanjang siklus hidupnya (mis. PO: Create+Submit+Approve+Cancel — empat aksi, bukan
satu), sehingga satu kolom per entity (pola `PettyCashBudgetService`, yang menaruh kolom pada baris
"movement" per transaksi) tidak dapat ditiru langsung karena PO/GR/Tukar Faktur/Purchasing
Invoice/Supplier Return tidak punya tabel "movement" semacam itu. Satu tabel ledger bersama
menghindari penambahan 4-8 kolom nullable pada lima tabel yang sudah berjalan, dan sekaligus
menjadi pola siap pakai bila gap idempotensi yang sama (masih "Rencana (belum tersedia)" di
`api-contract.md`) suatu saat ditutup untuk rumpun Finance lain (Receipt, Receivable, Payment,
dst.) — meski itu eksplisit di luar cakupan task ini.

---

## 4. Dokumentasi endpoint

Nol endpoint baru — 14 endpoint yang sudah ada kini menegakkan header wajibnya.

#### Corporate / Finance Management / Purchasing / Purchase Order

| Method | Path | Idempotency-Key |
| --- | --- | --- |
| `POST` | `/` | Ditegakkan |
| `POST` | `/{id}/submit` | Ditegakkan |
| `POST` | `/{id}/approve` | Ditegakkan |
| `POST` | `/{id}/cancel` | Ditegakkan |

#### Corporate / Finance Management / Purchasing / Goods Receipt

| Method | Path | Idempotency-Key |
| --- | --- | --- |
| `POST` | `/` | Ditegakkan |
| `POST` | `/{id}/cancel` | Ditegakkan |

#### Corporate / Finance Management / Purchasing / Invoice Exchange

| Method | Path | Idempotency-Key |
| --- | --- | --- |
| `POST` | `/` | Ditegakkan |
| `POST` | `/{id}/cancel` | Ditegakkan |

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Idempotency-Key |
| --- | --- | --- |
| `POST` | `/` | Ditegakkan |
| `POST` | `/{id}/submit` | Ditegakkan |
| `POST` | `/{id}/approve` | Ditegakkan |

#### Corporate / Finance Management / Purchasing / Supplier Return

| Method | Path | Idempotency-Key |
| --- | --- | --- |
| `POST` | `/` | Ditegakkan |
| `POST` | `/{id}/confirm` | Ditegakkan |
| `POST` | `/{id}/cancel` | Ditegakkan |

**Sengaja tidak disentuh:** `POST /{id}/reject` (PO dan Purchasing Invoice) dan `PUT /{id}`
(Update, PO dan Purchasing Invoice) — `api-contract.md` §B.1/§B.4 tidak mencantumkan
`Idempotency-Key` pada baris ini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi langsung oleh pengguna 30 September 2026 |
| Migration `TableFinPurchasingIdempotencyRecord` dibuat pengguna, sinkron model diverifikasi | `Up`/`Down` persis mencerminkan model (`CreateTable`+2 index+2 check constraint); migrasi probe `ProbeSync` (`20260930080914`) yang dibuat sesudahnya untuk mengonfirmasi sinkron **kosong total** (`Up`/`Down` no-op) — bukti model dan snapshot sudah sinkron penuh tanpa drift. `ProbeSync` dihapus sesudahnya (izin eksplisit pengguna), `ApplicationDbContextModelSnapshot.cs` tetap identik | `PASS` (diverifikasi lewat `diff`, bukan tebakan) | `diff` byte-per-byte kedua `Designer.cs` — hanya beda nama kelas/atribut `[Migration(...)]`, isi `BuildTargetModel`/`BuildModel` identik |
| Eksekusi migration ke database | Berjalan lancar, tabel `FinPurchasingIdempotencyRecord` aktif di database | `PASS` | Dikonfirmasi langsung oleh pengguna 30 September 2026 |
| Replay: kunci sama dua kali pada aksi sama | Permintaan kedua mengembalikan `ResponseBody`/`StatusCode` tersimpan, nol pemanggilan service bisnis kedua kalinya | `NOT RUN` (review kode) | `PurchasingIdempotencyService.TryReplayAsync` dipanggil sebelum blok `try` di seluruh 14 aksi |
| Header tidak dikirim | `400` otomatis dari `[ApiController]` model binding | `NOT RUN` (review kode, perilaku bawaan ASP.NET Core) | Parameter `Guid idempotencyKey` non-nullable pada seluruh 14 aksi |
| Race dua permintaan ber-kunci sama bersamaan | Penulis kedua ledger gagal indeks unik, ditangkap `DbUpdateException`, diabaikan — penulis pertama menang | `NOT RUN` (review kode) | `PurchasingIdempotencyService.SaveAsync` blok `try/catch (DbUpdateException)` |
| Aksi gagal (validasi/konflik) | Nol baris ledger ditulis — `SaveAsync` hanya dipanggil di jalur sukses | `NOT RUN` (review kode) | Seluruh 14 pemanggilan `SaveAsync` berada setelah baris sukses, sebelum `return`, di dalam blok `try` yang sama |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi dijalankan (`dotnet run`) beserta permintaan HTTP
nyata (mis. lewat Postman/Swagger) terhadap data Purchasing hidup untuk menguji replay end-to-end;
belum dilakukan pada sesi ini meski build dan migration sudah beres.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** eksekusi runtime/manual (uji panggilan HTTP hidup) — menunggu instruksi
eksplisit terpisah bila dibutuhkan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Permintaan dengan `Idempotency-Key` yang sama dua kali pada aksi yang sama mengembalikan respons persis pertama kali (replay), bukan memproses ulang | Terpenuhi (source) | `TryReplayAsync` + `Replay()` helper di seluruh 5 controller |
| Permintaan tanpa header ditolak `400` otomatis | Terpenuhi (source, perilaku bawaan `[ApiController]`) | Parameter `Guid idempotencyKey` non-nullable |
| Dua request bersamaan ber-kunci sama tidak menghasilkan dua baris ledger | Terpenuhi (source) | Indeks unik `IdempotencyKey` + `catch (DbUpdateException)` |
| Nol perubahan pada lima service Purchasing yang sudah berjalan | Terpenuhi (source) — diverifikasi lewat diff: nol baris berubah pada kelima berkas `*Service.cs` | Bagian 3.2 |
| `dotnet build` berhasil | Terpenuhi | `PASS`, dikonfirmasi pengguna |
| Migration dibuat dan sinkron dengan model | Terpenuhi | `TableFinPurchasingIdempotencyRecord` dibuat pengguna, diverifikasi sinkron via probe kosong (bagian 5) |
| Migration dieksekusi ke database | Terpenuhi | Dikonfirmasi pengguna, berjalan lancar |

Seluruh acceptance criteria terpetakan ke source yang ada, validasi yang diminta task ini
(`dotnet build`, migration dibuat+sinkron+dieksekusi) sudah benar-benar dijalankan dan hasilnya
dikonfirmasi pengguna — task ini ditandai `✅`. Uji manual/runtime (panggilan HTTP hidup) tetap
belum dilakukan; dicatat sebagai langkah lanjutan opsional, bukan penahan status, konsisten dengan
pola task backend lain pada modul ini (mis. `BE-FIN-039`/`040`/`050`).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini **bukan** hasil `/plan-module-delivery` — task ID dan keputusan desain (ledger terpisah vs kolom per-entity) ditetapkan langsung oleh pemilik repository di tengah sesi kerja `FE-FIN-004`/`BE-FIN-050`, menutup gap yang sudah dicatat terbuka di laporan `FE-FIN-008.md` |
| Masalah yang diketahui | (1) **Jendela atomisitas**: baris ledger ditulis di `SaveChangesAsync` TERPISAH dari transaksi bisnis masing-masing service (yang membuka/menutup transaksinya sendiri) — bila proses mati tepat di antara commit bisnis dan commit ledger, permintaan ulang akan memproses ulang aksi bisnisnya. Diterima sebagai trade-off proporsional (nol perubahan pada 5 service yang sudah berjalan dan teruji) — penjaga status entity yang sudah ada (mis. "PO harus DRAFT untuk diajukan") tetap jadi lapis pertahanan kedua, jadi konsekuensinya bukan silent double-processing tanpa jejak, hanya kegagalan validasi bisnis yang normal. (2) `SupplierReturn.Confirm`/`Cancel` disertakan meski `api-contract.md` §B.5 tidak secara eksplisit mencantumkannya (delta kontrak yang sudah dicatat `FinanceSupplierReturnsController` sejak `BE-FIN-035`) — dimasukkan untuk konsistensi karena keduanya state-transition uang-relevan yang setara dengan aksi lain yang memang wajib header ini; jika pemilik kontrak berpendapat lain, ini mudah dicabut (satu baris per aksi) |
| Dependency backend | `BE-FIN-032`..`035` ✅ — kelima controller sudah berjalan, source dikonfirmasi lewat pembacaan langsung. Task ini murni menambah lapisan di atasnya, nol dependency baru yang belum selesai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup task ini: 2 berkas baru (`FinPurchasingIdempotencyRecord.cs`, `FinPurchasingIdempotencyRecordConfiguration.cs`), 1 berkas baru (`PurchasingIdempotencyService.cs`), 5 controller Purchasing diubah, `ApplicationDbContext.cs` diubah (1 baris `DbSet`), `BillingManagementServiceCollectionExtensions.cs` diubah (1 baris registrasi), migration baru `Migrations/20260930080625_TableFinPurchasingIdempotencyRecord.cs`+`.Designer.cs` (dibuat pengguna), `Migrations/ApplicationDbContextModelSnapshot.cs` diubah (regenerasi otomatis EF, konsisten). Migrasi probe `ProbeSync` (`20260930080914_ProbeSync.cs`/`.Designer.cs`, dibuat pengguna untuk verifikasi) sudah **dihapus** — kosong total, tidak diperlukan lagi (izin eksplisit pengguna). File lain yang terlihat pada `git status` (mis. `docs/module-blueprints/accounting/MODULE-STATUS.md`, laporan `FE-FIN-007.md`/`FE-FIN-015.md`) **bukan** bagian task ini — tidak disentuh, kemungkinan sesi/proses lain berjalan paralel pada repository yang sama |
| Langkah berikutnya | (1) Uji manual replay pada minimal satu aksi per entity bila dibutuhkan (panggilan HTTP hidup, misalnya lewat Swagger); (2) **tindak lanjut frontend terpisah**: `FE-FIN-008`/`009` perlu diperbarui untuk benar-benar MENGIRIM header `Idempotency-Key` pada 14 aksi ini — laporan `FE-FIN-008.md` mencatat frontend saat ini sengaja TIDAK mengirim header itu karena sebelumnya "tidak ada penerimanya"; kini backend membacanya dan tabelnya aktif di database, jadi frontend perlu menyusul (task terpisah, `FRONTEND MODE`) |
