# Laporan Perubahan Backend — `BE-FIN-102`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-102` |
| Judul | Perbaiki HTTP 500 Settlement AR (translasi EF pada `debtorResolvedName`) dan duplikasi baris invoice batch |
| Slice | Perbaikan bug pasca-laporan pemilik, di luar gelombang REV-18 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "Perbaikan `ISSUE-FIN-002` — Settlement AR" |
| Trace | `ISSUE-FIN-002` (`ISS-FIN-002-01`, `ISS-FIN-002-02`), `PLAN-REPAIR-FIN-002` (`FIX-FIN-002-01`, `FIX-FIN-002-02`) |
| Contract version | Tidak berubah — endpoint, DTO, dan kontrak respons `FinanceSettlementPagedResponse`/`FinanceSettlementRowResponse`/`FinanceSettlementSummaryResponse` tetap sama persis |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — perubahan satu record proyeksi (tambah 2 anggota) dan dua titik pakai pada satu method, plus satu baris predikat filter, seluruhnya pada satu berkas |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs`; dokumen blueprint `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `845dabf9` (branch `Yasmina`) |
| Tanggal | 2026-10-11 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

---

## 1. Masalah yang diperbaiki

Petugas Finance yang membuka Settlement AR (kategori Perusahaan, dan pada dasarnya seluruh
kategori karena `sortBy=patientName` adalah default) mendapat "Terjadi kesalahan pada server".
Log aplikasi asli (`Logs/quilvian-backend-20261011.json`, entri berulang sejak 10:53:45 UTC hari
ini) membuktikan akar masalahnya: `System.InvalidOperationException: The LINQ expression
'...SettlementQueryRow(...).debtorResolvedName)' could not be translated.` — EF Core tidak bisa
menerjemahkan ulang referensi ke `x.debtorResolvedName` (anggota hasil proyeksi, dihitung dari
ternary bersarang `comp→ins→emp→pat`) ketika dipakai lagi pada `OrderBy` (`ApplySorting`, cabang
default `patientName`) yang dipasang setelah `Select` pada rantai 6 `LEFT JOIN`. Filter `Search`
memakai pola identik dan akan gagal dengan cara yang sama begitu parameter itu diisi.

Temuan terpisah: `batchSubquery` pada `BuildFilteredQueryable` tidak menyaring `!IsDelete`,
sehingga piutang dengan membership batch ganda (satu baris soft-delete tersisa, satu baris aktif
baru) bisa tampil dua kali pada daftar/summary, menggelembungkan `TotalData` dan nominal total.

---

## 2. Proses bisnis

Petugas Finance membuka **Finance → Transaksi A/R → Pembayaran & Pelunasan → Settlement AR**,
memilih kategori (Perusahaan/Pasien Umum/Karyawan), dan sistem menampilkan daftar piutang
beserta saldo awal/akhir per baris dan ringkasan total. Setelah perbaikan ini, alurnya kembali
normal: memilih kategori → daftar dan ringkasan tampil → mengurutkan berdasarkan nama pasien
(default) atau kolom lain tidak lagi gagal → mencari dengan kata kunci bebas (nama penjamin,
nama pasien, No. RM, nomor invoice) juga tidak lagi berisiko gagal dengan galat yang sama →
piutang yang pernah mengalami pembatalan/penerbitan ulang batch invoice tidak lagi muncul dua
kali di daftar maupun ringkasan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs` (seluruh berkas)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` (aksi `GetSettlement`/`GetSettlementSummary`)
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchItemConfiguration.cs` (konfirmasi index parsial unik `IsDelete=false AND IsActiveMembership=true`)
- `Logs/quilvian-backend-20261011.json` (bukti exception asli)
- `docs/module-blueprints/finance-management/docs/issue/issue-002-settlement-ar-http-500-dan-revisi-uiux.md` dan `docs/plan-repair/plan-repair-002-settlement-ar-http-500-dan-revisi-uiux.md`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (konfirmasi prefix `Fin`, status `ACTIVE`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs` | (1) `SettlementQueryRow` (baris 691-700): tambah anggota `MstCompanyGuarantor? comp` dan `MstInsuranceProvider? ins`. (2) Proyeksi pada `BuildFilteredQueryable` (baris 754-767): sertakan `comp`/`ins` sebagai anggota record (sumbernya sudah ada dari join existing, tidak ada join baru). (3) Filter `Search` (baris 789-802): ganti kondisi yang merujuk `x.debtorResolvedName` dengan pemeriksaan langsung `x.comp`/`x.ins`/`x.emp` (selain `x.pat` yang sudah ada), disertai komentar penjelas kenapa `debtorResolvedName` sengaja tidak dipakai di titik ini. (4) `ApplySorting` cabang default `patientName` (baris 832-842): hitung ulang ternary yang sama (`pat→comp→ins→emp`) langsung dari anggota record, bukan merujuk `x.debtorResolvedName`. (5) `batchSubquery` (baris 736-737): tambah `!b.IsDelete` ke predikat `Where`. `debtorResolvedName` tetap dipertahankan apa adanya untuk dipakai setelah `ToListAsync` (pemetaan baris `GetSettlementPagedAsync`, `GetSettlementSummaryInternalAsync`, export Excel/PDF) — titik-titik itu sudah aman karena berjalan di memori |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tambah bagian "Perbaikan `ISSUE-FIN-002`" beserta kartu task `BE-FIN-102` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint, nol bentuk request/response yang berubah. `GET .../receivables/settlement`, `.../summary`, `.../{id}`, `.../export/excel`, `.../export/pdf` kembali berfungsi dengan kontrak yang sama persis |
| Database | `NOT APPLICABLE` — nol migration, nol perubahan schema. `comp`/`ins` yang ditambahkan pada `SettlementQueryRow` adalah entity yang sudah di-join sebelumnya, hanya belum dibawa sebagai anggota record terpisah |
| Keamanan/Auth | `NOT APPLICABLE` — nol perubahan pada `[AccessAction]`/`[AccessPermission]`, nol endpoint baru |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah bentuk. Endpoint yang **diperbaiki perilakunya** (dari gagal
total menjadi berfungsi, untuk sorting default dan ke depannya untuk `Search`) adalah endpoint
yang sudah terdaftar:

#### Corporate / Finance Management / Receivable

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/receivables/settlement` | Daftar Settlement AR terpaginasi | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/settlement/summary` | Ringkasan global Settlement AR | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/settlement/{id}` | Detail satu Settlement AR | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/settlement/export/excel` | Ekspor seluruh hasil filter ke Excel | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/settlement/export/pdf` | Ekspor seluruh hasil filter ke PDF | `FinanceReceivable : Read` |

`GetSettlementDetailAsync` (endpoint `.../{id}`) **tidak terdampak** perbaikan translasi — method
itu memakai proyeksi anonymous type terpisah dengan `FirstOrDefaultAsync` langsung tanpa
`Where`/`OrderBy` tambahan setelahnya, sehingga tidak mengalami pola kegagalan yang sama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` / `dotnet run` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik pada sesi ini: "jangan lakukan build dotnet secara automatis" |
| Pembacaan ulang `SettlementQueryRow`, proyeksi, filter `Search`, `ApplySorting` setelah edit | Seluruh referensi ke `x.debtorResolvedName` pada klausa yang berjalan sebelum materialisasi (`Where`/`OrderBy`) sudah diganti menjadi penghitungan ulang dari `x.pat`/`x.comp`/`x.ins`/`x.emp`; `debtorResolvedName` masih dipakai apa adanya di titik-titik setelah `ToListAsync` | `PASS` | `grep -n debtorResolvedName FinanceSettlementService.cs` — seluruh kecocokan di luar deklarasi record dan komentar berada pada baris 99/104/200/205/317/319/452/454, semuanya di dalam `foreach`/mapping pasca-materialisasi |
| Pembacaan ulang `batchSubquery` setelah edit | Predikat kini `!b.IsDelete && b.IsActiveMembership && ...`, konsisten dengan pola `!r.IsDelete`/`!m.IsDelete` yang sudah dipakai di tempat lain pada file yang sama | `PASS` | Pembacaan source baris 736-737 |
| Tidak ada penggunaan `.AsEnumerable()`/`.ToList()` baru sebagai workaround sebelum filtering/pagination | Tidak ditemukan — seluruh perbaikan tetap dalam bentuk `IQueryable` yang diterjemahkan ke SQL | `PASS` | Pembacaan source menyeluruh method yang diubah |
| `GET .../receivables/settlement?category=COMPANY&pageNumber=1&pageSize=10&sortBy=patientName&sortDirection=asc` terhadap database sungguhan | Tidak dijalankan | `NOT RUN` | Memerlukan `dotnet build`/`dotnet run` — menunggu konfirmasi pemilik |
| `GET .../receivables/settlement?search=<kata kunci>` terhadap database sungguhan | Tidak dijalankan | `NOT RUN` | Sama seperti di atas |
| Uji duplikasi baris (piutang dengan membership batch ganda, satu soft-delete satu aktif) | Tidak dijalankan | `NOT RUN` | Memerlukan data uji dan environment database — menunggu konfirmasi pemilik |

Uji manual: `NOT FEASIBLE` pada sesi ini — server backend tidak dijalankan, sesuai batasan
"jangan lakukan build dotnet secara automatis" dari pemilik.

**Tidak dijalankan:** `dotnet build`, `dotnet run`, seluruh uji manual terhadap API sungguhan —
semuanya menunggu konfirmasi eksplisit pemilik untuk setiap langkah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Request yang dilaporkan (`category=COMPANY`, `sortBy=patientName`) mengembalikan HTTP 200 | Belum terpenuhi — menunggu `dotnet build` dan uji manual | Perbaikan source menghilangkan konstruksi yang terbukti gagal translasi (lihat bagian 5), tetapi belum dijalankan terhadap database sungguhan |
| Request dengan `search=<kata kunci>` juga mengembalikan HTTP 200 | Belum terpenuhi — menunggu `dotnet build` dan uji manual | Sama seperti di atas; filter `Search` kini memakai pola yang sama dengan `ApplySorting` yang sudah diperbaiki |
| Log tidak lagi mencatat `InvalidOperationException`/`could not be translated` untuk endpoint ini | Belum dapat dipastikan — menunggu reload aplikasi dan reproduksi ulang | Perbaikan source sudah menghilangkan konstruksi pemicunya |
| Piutang dengan membership batch ganda tampil tepat satu kali, `TotalData` = `COUNT DISTINCT ReceivableId` | Belum terpenuhi — menunggu data uji dan environment database | Perbaikan source (`!b.IsDelete`) sudah benar secara statis (lihat bagian 5) |

Task ini **belum** dapat ditandai ✅ selama `dotnet build` dan uji manual terhadap database
sungguhan belum dikonfirmasi pemilik — konsisten dengan `status-task-roadmap.md`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemilik secara eksplisit meminta agar `dotnet build`/`dotnet run` tidak dijalankan otomatis pada sesi ini — dicatat sebagai pengecualian verifikasi, bukan diabaikan |
| Masalah yang diketahui | `FIX-FIN-002-08` (bagian backend, label semantik saldo) dan `FIX-FIN-002-09` (revisi dokumen blueprint) belum termasuk task ini — ditangani terpisah |
| Risiko tersisa | Rendah untuk kebenaran logika (perubahan murni menghitung ulang ekspresi yang sudah ada di file yang sama, tanpa mengubah aturan bisnis). Risiko administratif: kebenaran runtime belum dibuktikan dengan `dotnet build`/uji manual |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs`, `M docs/module-blueprints/finance-management/docs/issue/`, `M docs/module-blueprints/finance-management/docs/plan-repair/`, `M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, `?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-102.md` |
| Langkah berikutnya | Pemilik mengonfirmasi `dotnet build`/`dotnet run`, lalu uji manual request yang dilaporkan beserta variasi `search` dan data uji membership batch ganda pada `PLAN-REPAIR-FIN-002` bagian 8; setelah PASS, tandai `BE-FIN-102` ✅ pada roadmap dan `requirement-traceability.md` |
