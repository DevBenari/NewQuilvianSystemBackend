# Laporan Perubahan Backend — `BE-FIN-101`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-101` |
| Judul | Perbaiki dua nama kolom SQL salah dan urutan baca reader/koneksi pada Laporan AR |
| Slice | Perbaikan bug pasca-laporan pemilik, di luar gelombang REV-18 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "Perbaikan `ISSUE-FIN-001` — Laporan AR dan Report AR Created" |
| Trace | `ISSUE-FIN-001` (`ISS-FIN-001-01`, `ISS-FIN-001-02`, `ISS-FIN-001-T1`), `PLAN-REPAIR-FIN-001` (`FIX-FIN-001-01`, `FIX-FIN-001-03`) |
| Contract version | Tidak berubah — endpoint, DTO, dan kontrak respons `BillingArReportPagedResponse`/`BillingArReportSummaryResponse` tetap sama persis |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — dua baris nama kolom pada string SQL, dan satu restrukturisasi urutan baris tanpa logika baru, pada satu berkas |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs`; dokumen blueprint `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `845dabf918f28c6326aecdbcb89a1e2297367265` (branch `Yasmina`) |
| Tanggal | 2026-10-11 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

---

## 1. Masalah yang diperbaiki

Setiap kali petugas Finance membuka menu Laporan AR, sistem menampilkan "Terjadi kesalahan pada
server" dan laporan sama sekali tidak terbuka — bukan kosong, bukan sebagian, melainkan gagal
total. Log server mencatat `PostgresException 42703: column intake2.CreatedAt does not exist`.
Penyebabnya: dua `ORDER BY` pada query SQL mentah di balik laporan ini memanggil kolom bernama
`"CreatedAt"` pada tabel `FinBillingHandoffIntake` dan `FinReceivableInvoiceBatch`, padahal kolom
jejak pembuatan yang sebenarnya ada pada kedua tabel itu bernama `"CreateDateTime"`. Ditemukan
pula satu risiko tambahan yang belum pernah benar-benar terjadi di lapangan (karena bug di atas
selalu menggagalkan query lebih dulu): bila petugas menyaring laporan dengan satu Penjamin
tertentu, kode yang mengambil nama penjamin dijalankan selagi pembaca data mentah (`reader`) pada
koneksi database yang sama masih terbuka — begitu bug utama diperbaiki, ini berisiko memunculkan
galat baru yang berbeda untuk kasus saringan tersebut.

---

## 2. Proses bisnis

Petugas Finance membuka menu **Finance → Transaksi A/R → Laporan & Monitoring → Laporan AR**.
Sistem menjalankan satu query gabungan yang menelusuri setiap invoice Billing, lalu mencari di
`LATERAL JOIN`: (1) baris handoff Finance AR yang paling baru untuk invoice itu
(`FinBillingHandoffIntake`), untuk mengetahui status integrasinya ke Finance; dan (2) baris batch
Invoice AR yang paling aktif untuk piutang yang terbentuk darinya (`FinReceivableInvoiceBatch`),
untuk mengetahui status pembuatan dokumen tagihan resminya. Kedua pencarian "ambil satu baris
terbaru" ini memakai `ORDER BY ... DESC LIMIT 1`, dan nama kolom tanggal yang dipakai sebagai
pembanding "terbaru" itulah yang salah ketik.

Setelah diperbaiki, alurnya: petugas membuka laporan (boleh tanpa saringan apa pun, atau dengan
saringan penjamin/jenis kunjungan/periode/status) → sistem menghitung ringkasan (jumlah invoice,
nominal, status) dan daftar baris halaman aktif → bila petugas menyaring dengan satu Penjamin
tertentu, sistem juga mencari nama penjamin tersebut untuk ditampilkan di ringkasan → laporan
tampil lengkap dengan tabel dikelompokkan per penjamin dan subtotal per tanggal. Jalur tidak
normal yang ditutup task ini: saringan penjamin tertentu yang sebelumnya berisiko gagal dengan
galat database kedua begitu bug utama hilang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs` (seluruh berkas, 1196 baris)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInvoiceBatchService.cs` (audit raw SQL lain — tidak ada typo kolom sejenis)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` dan `FinanceReceivableInvoiceBatchesController.cs`
- `Models/IdentityModel.cs`, `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs`, `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs`
- `Migrations/20261010115612_AddFinanceReceivableInstallmentAndBenefitSettlement.Designer.cs` (snapshot schema terbaru, untuk mengonfirmasi nama kolom sebenarnya pada database)
- `docs/module-blueprints/finance-management/docs/issue/issue-001-laporan-ar-dan-report-ar-created.md` dan `docs/plan-repair/plan-repair-001-laporan-ar-dan-report-ar-created.md`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (konfirmasi prefix `Fin`, status `ACTIVE`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs` | Baris 581 (`BuildFromWhereClause`): `ORDER BY intake2."CreatedAt" DESC` → `ORDER BY intake2."CreateDateTime" DESC, intake2."Id" DESC`. Baris 613: `ORDER BY bi2."IsActiveMembership" DESC, batch2."CreatedAt" DESC` → `... batch2."CreateDateTime" DESC, batch2."Id" DESC`. Method `ExecuteSummaryQueryAsync` (baris 734-839) direstrukturisasi: 12 nilai kolom dari `reader` disimpan ke variabel lokal di dalam cakupan eksplisit yang membuka `cmd`/`reader`; pemanggilan `ResolvePayerNameAsync` dipindah ke setelah cakupan itu berakhir (reader sudah dipastikan di-dispose), sebelum membentuk `BillingArReportSummaryResponse` |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tambah bagian "Perbaikan `ISSUE-FIN-001`" beserta kartu task `BE-FIN-101` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint, nol bentuk request/response yang berubah. `GET .../reports/billing-ar`, `.../export/excel`, `.../export/pdf` kembali berfungsi dengan kontrak yang sama persis seperti yang sudah didokumentasikan |
| Database | `NOT APPLICABLE` — nol migration, nol perubahan schema. Perbaikan murni menyamakan nama kolom pada string SQL dengan nama kolom yang sudah ada di database sejak awal |
| Keamanan/Auth | `NOT APPLICABLE` — nol perubahan pada `[AccessAction]`/`[AccessPermission]`, nol endpoint baru |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah bentuk. Endpoint yang **diperbaiki perilakunya** (dari gagal
total menjadi berfungsi) adalah endpoint yang sudah terdaftar:

#### Corporate / Finance Management / Receivable

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar` | Daftar dan ringkasan Laporan AR terpaginasi | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar/export/excel` | Ekspor seluruh hasil filter ke Excel | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar/export/pdf` | Ekspor seluruh hasil filter ke PDF | `FinanceReceivable : Read` |

`GET .../reports/billing-ar/payer-options` **tidak terdampak** — endpoint ini memakai LINQ EF Core
murni ke `MstInsuranceProviders`/`MstCompanyGuarantors`, tidak memanggil `BuildFromWhereClause`
atau `ExecuteSummaryQueryAsync` yang diperbaiki di sini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik pada sesi ini: "jangan lakukan build dotnet secara automatis" |
| Pembacaan ulang kedua baris `ORDER BY` setelah edit | Keduanya memakai `"CreateDateTime"`, cocok dengan kolom pada migration snapshot terbaru | `PASS` | `grep -n 'CreateDateTime' FinanceBillingArReportService.cs` → baris 581 dan 613 |
| Pembacaan ulang `ExecuteSummaryQueryAsync` setelah restrukturisasi | `reader`/`cmd` berada dalam cakupan `{ }` eksplisit yang berakhir sebelum `ResolvePayerNameAsync` dipanggil; 12 nilai tersimpan ke variabel lokal sebelum cakupan itu berakhir | `PASS` | Pembacaan source baris 761-839 |
| `GET .../reports/billing-ar` terhadap database sungguhan (tanpa saringan, per penjamin, per jenis kunjungan, per periode, per status) | Tidak dijalankan | `NOT RUN` | Memerlukan `dotnet build`/`dotnet run` dan environment database — menunggu konfirmasi pemilik |
| `GET .../export/excel`, `.../export/pdf` | Tidak dijalankan | `NOT RUN` | Sama seperti di atas |

Uji manual: `NOT FEASIBLE` pada sesi ini — server backend tidak dijalankan, sesuai batasan "jangan
lakukan build dotnet secara automatis" dari pemilik.

**Tidak dijalankan:** `dotnet build`, `dotnet run`, seluruh uji manual terhadap API sungguhan —
semuanya menunggu konfirmasi eksplisit pemilik untuk setiap langkah, sesuai instruksi yang
diberikan pada sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `GET .../reports/billing-ar` (tanpa saringan) mengembalikan HTTP 200, bukan 500/42703 | Belum terpenuhi — menunggu `dotnet build` dan uji manual | Perbaikan source sudah benar secara statis (lihat bagian 5), tetapi belum dijalankan terhadap database sungguhan |
| `ORDER BY` memakai `"CreateDateTime"`, bukan `"CreatedAt"` | **Terpenuhi** | `grep` baris 581, 613 |
| Saringan `payerId` tertentu tidak lagi berisiko galat koneksi/reader ganda | **Terpenuhi secara source** (restrukturisasi urutan baris); belum diverifikasi runtime | Pembacaan source bagian 5 |
| Nilai ringkasan dan daftar lain tidak berubah dari logika sebelumnya | **Terpenuhi** — perbaikan tidak menyentuh rumus atau kolom `SELECT` apa pun, hanya nama kolom pembanding `ORDER BY` dan urutan baris pemanggilan method | Diff baris demi baris, bagian 3.2 |

Task ini **belum** dapat ditandai ✅ selama `dotnet build` dan uji manual terhadap database
sungguhan belum dikonfirmasi pemilik — konsisten dengan `status-task-roadmap.md`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemilik secara eksplisit meminta agar `dotnet build` tidak dijalankan otomatis pada sesi ini — dicatat sebagai pengecualian verifikasi, bukan diabaikan |
| Masalah yang diketahui | `FIX-FIN-001-04` (revisi dokumen `03-frontend-architecture.md` agar menyebut endpoint V2 Report AR Created) belum dikerjakan — di luar scope task ini (dokumentasi frontend-architecture, bukan source backend) |
| Audit lanjutan (instruksi pemilik 11 Oktober 2026, sesi kedua) | Diminta memeriksa ulang: (1) default Status Billing tidak boleh dipaksa `CLOSED` — **sudah benar sejak awal**, `BillingArReportQuery.BillingStatus` default `"ALL"` (`FinanceBillingArReportDtos.cs:30`) dan `BuildFromWhereClause` hanya menambah `AND i."Status" = @billingStatus` bila bukan `ALL`; (2) summary tidak boleh double count akibat JOIN — **sudah benar**, seluruh `LATERAL JOIN` pada `BuildFromWhereClause` memakai `LIMIT 1` sehingga setiap invoice `i` menghasilkan tepat satu baris, `COUNT(*)` pada `ExecuteSummaryQueryAsync` tidak mungkin menduplikasi; (3) pagination tidak boleh mengambil seluruh data dulu — **sudah benar**, `ExecutePagedItemsQueryAsync` memakai `LIMIT`/`OFFSET` di SQL, bukan memuat semua baris lalu memotong di memori; (4) export harus mencakup seluruh hasil filter, bukan 30 baris aktif — **sudah benar**, `ExportExcelAsync`/`ExportPdfAsync` memanggil `ExecutePagedItemsQueryAsync(..., 0, int.MaxValue, ...)`. **Nol perubahan source backend tambahan diperlukan** untuk keempat butir ini — temuan dicatat di sini sebagai bukti audit, bukan sebagai perbaikan baru |
| Risiko tersisa | Rendah untuk kebenaran logika (perbaikan nama kolom literal dan penataan ulang urutan baris, nol logika bisnis disentuh). Risiko administratif: kebenaran runtime belum dibuktikan dengan `dotnet build`/uji manual |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs`, `?? docs/module-blueprints/finance-management/docs/issue/`, `?? docs/module-blueprints/finance-management/docs/plan-repair/`, `M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, `?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-101.md` |
| Langkah berikutnya | Pemilik mengonfirmasi `dotnet build`, lalu uji manual keempat endpoint dengan kombinasi saringan pada `PLAN-REPAIR-FIN-001` bagian 8; setelah PASS, tandai `BE-FIN-101` ✅ pada roadmap dan `requirement-traceability.md` |
