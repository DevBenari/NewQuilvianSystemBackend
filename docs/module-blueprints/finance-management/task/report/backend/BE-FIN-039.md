# Laporan Perubahan Backend — `BE-FIN-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-039` |
| Judul | Beberapa piutang satu penjamin dapat diterbitkan sebagai satu dokumen tagihan resmi |
| Slice | `REV-4` — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian "REV-4 — AR Invoice Agregat dan Potongan AR" |
| Trace | `FIN-DEC-048`, `054`; `FR-FIN-089`..`092`; `FIN-API-1.1` §B.7; `FIN-PERM-1.1` §B.5; `FIN-STATE-1.2` §B.7; `FIN-VAL-1.2` `114`..`117` |
| Contract version | `FIN-API-1.1` (`locked` 25 September 2026) |
| Dependency | `BE-FIN-038` 🟡 (source & berkas migration selesai 29 September 2026, eksekusi migration tertunda — dipakai sebagai bukti skema `FinReceivableInvoiceBatch(Item)`, bukan blocker implementasi source) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); ≤8 berkas diperiksa domain terdekat (skor 0), tapi >8 total dibaca lintas kontrak dan modul Billing (skor 1); 4 berkas diubah/dibuat (skor 1); logika bisnis sedang — agregasi lintas piutang + panggilan lintas domain (skor 1); kontrak API — 7 endpoint baru memakai pola yang sudah ada (skor 1); database — nol perubahan skema, murni baca tabel `BE-FIN-038` (skor 0); keamanan/auth — resource+action baru mengikuti pola kanonikal (skor 1); UI/workflow — tidak ada (skor 0). Total 5 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,Dtos,Services}/**` (baru), `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI), `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — lihat Status Git bagian 7 (working tree `Yasmina`, HEAD `7811c048`) |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **sengaja tidak dijalankan** — instruksi eksplisit pengguna pada task ini ("tanpa build automatis"). Nol migration disentuh — task ini murni membaca tabel yang sudah dibangun `BE-FIN-038` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, meski skema Batch Tagihan AR sudah tersedia (`BE-FIN-038`), tidak ada layanan
atau endpoint yang benar-benar memakainya. Petugas AR tidak dapat menggabungkan beberapa piutang
satu penjamin menjadi satu dokumen tagihan resmi — setiap piutang harus ditagihkan terpisah,
padahal penjamin yang sama biasanya ingin menerima satu dokumen gabungan per periode.

**Contoh konkret.** PT Contoh Sejahtera (penjamin perusahaan) punya tiga piutang bulan September:
Rp 5.000.000, Rp 3.200.000, dan Rp 1.800.000. Sebelum task ini, RS mengirim tiga dokumen tagihan
terpisah. Sesudah task ini, petugas AR memilih ketiganya lewat `GET /eligible-receivables`,
membuat satu Batch Tagihan AR (`POST /`), menerbitkannya (`POST /{id}/issue`), lalu mengunduh satu
dokumen gabungan (`GET /{id}/document`) senilai total Rp 10.000.000 berisi rincian ketiga invoice
dari layanan Billing.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR (membuat, menerbitkan, membatalkan batch; mengunduh dokumen).

**Langkah normal:**

1. Petugas AR memeriksa piutang yang layak digabung untuk satu penjamin
   (`GET /eligible-receivables?debtorReferenceId=...`) — hanya menampilkan piutang `PAYER` milik
   penjamin itu, belum tergabung batch aktif lain, dan masih `OUTSTANDING`/`PARTIAL`.
2. Petugas AR membuat batch (`POST /`) dari piutang terpilih beserta periode tagihannya. Batch
   tersimpan `DRAFT`, `TotalAmount` dihitung backend dari jumlah `OriginalAmount` anggotanya.
3. Petugas AR menerbitkan batch (`POST /{id}/issue`). Status berpindah ke `ISSUED` — anggotanya
   terkunci, tidak dapat ditambah/dikurangi lagi.
4. Petugas AR (atau siapa pun yang berhak) mengunduh dokumen tagihan gabungan
   (`GET /{id}/document`) — rincian per invoice diambil dari
   `BillingCompanyGuarantorInvoiceDocumentService` milik Billing, dipanggil sekali per anggota.
5. Seiring piutang anggotanya dilunasi lewat jalur penerimaan yang sudah ada (`BE-FIN-018`,
   di luar task ini), status batch **otomatis mengikuti**: `ISSUED` → `PARTIALLY_PAID` (sebagian
   anggota `SETTLED`) → `PAID` (seluruh anggota `SETTLED`). Ini dihitung ulang setiap batch dibaca
   satu-satu (`GET /{id}`), **bukan** lewat perubahan pada `FinanceReceivableService`.

**Jalur tidak normal:**

- Piutang bukan `PAYER` disertakan → ditolak `400` (batas skema `FIN-DEC-048`: batch hanya
  menerima `DebtorType = PAYER`).
- Piutang dari dua penjamin berbeda dicampur dalam satu batch → ditolak `400` (`FIN-VAL-115`).
- Piutang yang sudah tergabung batch aktif lain disertakan lagi → ditolak `409` (`FIN-VAL-114`).
- Batch kosong (nol anggota) diterbitkan → ditolak `422` (`FIN-VAL-116`).
- Batch yang bukan `DRAFT` diminta diterbitkan atau dibatalkan → ditolak `422` menyebut status
  sebenarnya (mencegah pelanggaran "anggota `ISSUED` terkunci").
- `RowVersion` usang pada `issue`/`cancel` → `409`, klien diminta memuat ulang.

**Hasil akhir:** batch `ISSUED`/`PARTIALLY_PAID`/`PAID` selalu mencerminkan status anggotanya
apa adanya; `FinReceivable.OutstandingAmount` **tidak pernah** disentuh service ini —
`FinanceReceivableService` tetap satu-satunya penulis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu task
  `BE-FIN-039`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.7 (7 endpoint),
  `permission-audit-matrix.md` §B.5 (resource `FinanceReceivableInvoiceBatch`, action
  `Read`/`Create`/`Issue`/`Update`), `state-transition-matrix.md` §B.7, `validation-matrix.md`
  `FIN-VAL-114`..`117`
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch(Item).cs`,
  `FinReceivable.cs` (`DebtorType`/`DebtorReferenceId`, `FIN-CAP-031`) — dibaca, **tidak diubah**
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` — pola
  `GetPagedAsync` (paging+sort), exception class yang dipakai ulang
  (`ReceivableBadRequestException` dkk. **tidak** dipakai ulang — dibuat set baru
  `ReceivableInvoiceBatch*Exception` agar tidak tercampur dengan exception piutang individual)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` —
  pola controller submodul ini; **disimpangi secara sengaja** pada penamaan resource hak akses
  (lihat 3.3)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingCompanyGuarantorInvoiceDocumentService.cs`
  (`FIN-CAP-030`) — **dipanggil per anggota lewat `InvoiceId`**, tidak disalin logikanya
- `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingCompanyGuarantorInvoiceDtos.cs` —
  bentuk `CompanyGuarantorInvoiceDocumentResponse` yang dibungkus jadi dokumen gabungan
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
  — lokasi registrasi DI dan konfirmasi `BillingCompanyGuarantorInvoiceDocumentService` sudah
  scoped, siap diinjeksi lintas domain

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Dtos/FinanceReceivableInvoiceBatchDtos.cs` | **Baru.** Query, response, request DTO untuk 7 endpoint |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInvoiceBatchService.cs` | **Baru.** `GetPagedAsync`, `GetByIdAsync` (+ `RefreshStatusAsync` lazy), `GetEligibleReceivablesAsync`, `CreateAsync` (`FIN-VAL-114`/`115`), `IssueAsync` (`FIN-VAL-116`), `CancelAsync`, `GetDocumentAsync` (agregasi lintas domain) |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs` | **Baru.** 7 endpoint sesuai `api-contract.md` §B.7 |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Tambah `services.AddScoped<FinanceReceivableInvoiceBatchService>();` |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-039` → 🟡 di seluruh titik kemunculan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Ketujuh endpoint** `api-contract.md` §B.7 dibuat: `GET /`, `GET /{id}`, `GET /eligible-receivables`, `POST /`, `POST /{id}/issue`, `GET /{id}/document`, `POST /{id}/cancel`. Nol delta kontrak — kontrak sudah lengkap sejak `locked` |
| Database | **`NOT APPLICABLE`.** Task ini murni membaca tabel yang sudah dibangun `BE-FIN-038` (`FinReceivableInvoiceBatch`, `...Item`) dan `FinReceivable` yang sudah berjalan. Nol tabel/kolom baru, nol migration |
| Keamanan/Auth | Resource baru `FinanceReceivableInvoiceBatch` (nama **kanonikal penuh**, persis `permission-audit-matrix.md`). Action: `Read`, `Create`, `Issue`, `Update` (dipakai untuk `cancel`, mengikuti pola `FinanceInvoiceExchangesController`/`FinancePurchasingInvoicesController` yang memetakan `cancel` ke `Update`). **Keputusan sengaja:** controller ini **tidak** mengikuti pola nama pendek `FinanceReceivablesController` (`[AccessPermission("Receivable", ...)]`) — itu utang legacy yang sedang diluruskan `BE-FIN-042`/`FIN-DES-062` (`FIN-CQ-08`). Controller **baru** ini langsung memakai nama kanonikal sejak awal, konsisten dengan seluruh controller Purchasing yang dibangun sejak `BE-FIN-032`. Nol `IsInRole`/nama peran/departemen/`UserType` hardcode |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar batch, disaring penjamin (`debtorReferenceId`) dan status | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/eligible-receivables` | Daftar `FinReceivable` yang memenuhi syarat digabung untuk satu penjamin | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/{id:guid}` | Rincian batch beserta daftar `FinReceivable` anggota; status disegarkan sebelum dikembalikan | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/{id:guid}/document` | Dokumen tagihan gabungan — rincian per invoice dari layanan Billing | `FinanceReceivableInvoiceBatch : Read` |
| `POST` | `/` | Membuat batch `DRAFT` dari daftar `FinReceivable` terpilih; `TotalAmount` dihitung backend | `FinanceReceivableInvoiceBatch : Create` |
| `POST` | `/{id:guid}/issue` | Menerbitkan batch, mengunci daftar anggotanya | `FinanceReceivableInvoiceBatch : Issue` |
| `POST` | `/{id:guid}/cancel` | Membatalkan batch `DRAFT` yang belum diterbitkan | `FinanceReceivableInvoiceBatch : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Sengaja tidak dijalankan — instruksi eksplisit pengguna pada task ini |
| Campur dua penjamin dalam satu `CreateAsync` | Ditolak `ReceivableInvoiceBatchBadRequestException` → `400` | `NOT RUN` (review kode) | `CreateAsync` — `debtorReferenceIds.Count > 1` |
| Piutang di batch aktif lain disertakan | Ditolak `ReceivableInvoiceBatchConflictException` → `409` | `NOT RUN` (review kode) | `CreateAsync` — query `FinReceivableInvoiceBatchItems` filter `Batch.Status <> CANCELLED` |
| Batch kosong diterbitkan | Ditolak `422` | `NOT RUN` (review kode) | `IssueAsync` — `Items.Count(x => !x.IsDelete) == 0` |
| Batch `ISSUED` diminta dibatalkan | Ditolak `422` menyebut status sebenarnya | `NOT RUN` (review kode) | `CancelAsync` |
| Batch `ISSUED` dengan sebagian anggota `SETTLED` dibaca | Status berubah jadi `PARTIALLY_PAID`, tersimpan | `NOT RUN` (review kode) | `RefreshStatusAsync` |
| Dokumen batch beranggota 3 invoice | 3 pemanggilan `BillingCompanyGuarantorInvoiceDocumentService.GetDocumentAsync`, `GrandTotalCoveredAmount` = jumlah `Totals.TotalCoveredAmount` ketiganya | `NOT RUN` (review kode) | `GetDocumentAsync` |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi berjalan (`dotnet run`) dan database dengan
migration `BE-FIN-038` sudah dieksekusi, keduanya belum diotorisasi/dijalankan.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh eksekusi runtime/manual, dan
`Invoke-QbeConformanceCheck.ps1` — ketiganya menunggu instruksi eksplisit terpisah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Campur dua penjamin ditolak `400` | Terpenuhi (source) | `CreateAsync` |
| Piutang di batch aktif lain ditolak `409` | Terpenuhi (source) | `CreateAsync` |
| Batch kosong tidak dapat terbit | Terpenuhi (source) | `IssueAsync` — `FIN-VAL-116` |
| Dokumen batch memuat rincian per invoice dari layanan Billing | Terpenuhi (source) | `GetDocumentAsync` memanggil `BillingCompanyGuarantorInvoiceDocumentService` |
| Status `PARTIALLY_PAID`/`PAID` mengikuti status `FinReceivable` anggota | Terpenuhi (source) — lazy refresh saat `GetByIdAsync` | `RefreshStatusAsync` |
| Service ini **MUST NOT** menulis `FinReceivable.OutstandingAmount` | Terpenuhi — nol baris kode menulis kolom itu; seluruh akses `FinReceivable` bersifat baca (`AsNoTracking` atau baca lewat `Include` tanpa mutasi) | Review `FinanceReceivableInvoiceBatchService.cs` menyeluruh |
| `dotnet build` berhasil | **Belum terpenuhi** | Sengaja `NOT RUN` |
| `FIN-TEST-1.2` §B.5 | **Belum dijalankan** — memerlukan aplikasi berjalan | — |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | (1) `GET /eligible-receivables` dan filter "layak digabung" (`PAYER` + belum tergabung batch aktif + `OUTSTANDING`/`PARTIAL`) adalah **definisi teknis**, bukan `FIN-VAL` bernomor — kontrak hanya menyebut namanya, tidak merinci kriterianya; dicatat di sini sebagai keputusan implementasi yang dapat ditinjau ulang pemilik kontrak. (2) `PeriodStart`/`PeriodEnd` tidak divalidasi `Start <= End` — tidak ada `FIN-VAL` yang memintanya. (3) `GenerateBatchNumber` belum memakai provider number-series atomik (`QBE-CODE-001`..`006`), pola yang sama dengan seluruh generator nomor rumpun ini |
| Risiko tersisa | `RefreshStatusAsync` hanya dipanggil dari `GetByIdAsync` — daftar (`GetPagedAsync`) menampilkan status hasil refresh **terakhir kali batch itu dibuka satu-satu**, bukan status yang selalu real-time. Ini keputusan cakupan sengaja (menghindari menulis N baris sekaligus saat memuat daftar), dicatat sebagai risiko tampilan, bukan risiko data — `OutstandingAmount` piutang yang sebenarnya tidak pernah salah, hanya label ringkasan batch yang bisa sedikit tertinggal sampai batch itu dibuka |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir pekerjaan mencakup task ini: `M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`, `?? Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs`, `?? Areas/Corporate/FinanceManagement/Receivable/Dtos/FinanceReceivableInvoiceBatchDtos.cs`, `?? Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInvoiceBatchService.cs`, plus `?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-039.md`. Berkas model/configuration `FinReceivableInvoiceBatch(Item)` dari `BE-FIN-038` juga masih `??` karena belum di-commit siapa pun — **tidak disentuh ulang** task ini. Sejumlah besar perubahan milik sesi lain juga terlihat pada `git status` repository — tidak disentuh, tidak dilaporkan di sini karena bukan bagian task ini |
| Langkah berikutnya | (1) Pemilik repository menjalankan `dotnet build`; (2) otorisasi dan eksekusi migration `AddArInvoiceBatchAndReceiptDeduction` (`BE-FIN-038`) ke lingkungan pengembangan — endpoint task ini tidak dapat diuji runtime tanpanya; (3) `BE-FIN-040` (potongan penerimaan) dapat dikerjakan paralel — tidak bergantung pada task ini |
