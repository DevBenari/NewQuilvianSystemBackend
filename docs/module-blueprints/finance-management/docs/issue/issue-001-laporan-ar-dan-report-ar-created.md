# ISSUE-001 — Laporan AR Gagal Total Karena Nama Kolom SQL Salah, Report AR Created Gagal Dimuat Karena Import React yang Hilang

```yaml
issue_id: ISSUE-FIN-001
module_id: finance-management
layar: "Finance → Transaksi A/R → Laporan & Monitoring → Laporan AR (GET /api/v1/corporate/finance-management/receivables/reports/billing-ar) dan Laporan Pembuatan AR / Report AR Created (/finance/ar-report/created)"
sumber_laporan: "Laporan pemilik 11-10-2026: dua gejala — Laporan AR menampilkan 'Terjadi kesalahan pada server' (PostgresException 42703), dan Report AR Created menampilkan 'This page couldn't load'"
tanggal_issue: "2026-10-11"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "845dabf918f28c6326aecdbcb89a1e2297367265 (Yasmina)"
source_sha_frontend: "46a1549dcc3b9f5359d132e501dec567ec826e17 (yasmina)"
rencana_perbaikan: ../plan-repair/plan-repair-001-laporan-ar-dan-report-ar-created.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Dua layar pada menu Finance → Transaksi A/R → Laporan & Monitoring sama-sama tidak dapat
digunakan sejak sumbernya pada kondisi laporan ini. **Laporan AR** gagal total dengan pesan
"Terjadi kesalahan pada server" karena query SQL mentah di backend memakai nama kolom yang
tidak pernah ada di database (`CreatedAt`, padahal kolomnya bernama `CreateDateTime`) pada dua
tempat `ORDER BY` di dalam `LATERAL JOIN`. **Report AR Created** gagal dimuat sama sekali
("This page couldn't load") karena hook data frontend-nya memanggil `useMemo` tanpa pernah
mengimpornya dari `react`, sehingga React melempar galat saat halaman pertama kali dirender.
Keduanya murni cacat teknis (`BUG`), bukan desain yang disengaja, dan keduanya **Blocker** karena
seluruh alur layar tidak dapat diselesaikan sama sekali — bukan sekadar terganggu. Satu temuan
tambahan (laten, belum pernah terpicu karena bug utama menghalanginya lebih dulu) ditemukan pada
cara Laporan AR membaca nama penjamin saat saringan penjamin tertentu dipakai. Satu temuan
dokumentasi tambahan juga dicatat: dokumen desain frontend masih menyebut kontrak API lama untuk
Report AR Created, padahal source sudah pindah ke endpoint detail resmi yang lebih baru.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (diringkas) | Lampiran |
| :---: | --- | --- |
| 1 | Laporan AR menampilkan "Terjadi kesalahan pada server" saat diakses; log backend menunjukkan `PostgresException 42703: column intake2.CreatedAt does not exist` pada `FinanceBillingArReportService.ExecuteSummaryQueryAsync` | Potongan log error dari pelapor (disertakan dalam teks laporan) |
| 2 | Report AR Created (`/finance/ar-report/created`) menampilkan "This page couldn't load" | Tidak ada tangkapan layar; gejala dilaporkan sebagai teks |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-FIN-001-01` | 1 | `ORDER BY intake2."CreatedAt"` memakai nama kolom yang tidak ada di tabel `FinBillingHandoffIntake` | `BUG` | Backend | Blocker | SUDAH-VERIFIKASI | `FIX-FIN-001-01` |
| `ISS-FIN-001-02` | 1 | `ORDER BY batch2."CreatedAt"` memakai nama kolom yang tidak ada di tabel `FinReceivableInvoiceBatch` | `BUG` | Backend | Blocker | SUDAH-VERIFIKASI | `FIX-FIN-001-01` |
| `ISS-FIN-001-03` | 2 | Hook `useReportArCreated` memanggil `useMemo` tanpa mengimpornya dari `react` | `BUG` | Frontend | Blocker | SUDAH-VERIFIKASI | `FIX-FIN-001-02` |
| `ISS-FIN-001-T1` | — | `ResolvePayerNameAsync` menjalankan query EF Core pada koneksi yang sama selagi `NpgsqlDataReader` ringkasan masih terbuka | `BUG` (laten) | Backend | High | SUDAH-VERIFIKASI | `FIX-FIN-001-03` |
| `ISS-FIN-001-T2` | — | `03-frontend-architecture.md` §17 masih mendokumentasikan kontrak lama `GET /receivables` untuk Report AR Created, padahal source sudah memakai endpoint detail resmi `GET /receivable-invoice-batches/reports/created` | `DESIGN_CHANGE` (dokumen tertinggal dari source) | Dokumentasi | Low | SUDAH-VERIFIKASI | `FIX-FIN-001-04` |

---

## 4. Rincian per Temuan

### ISS-FIN-001-01 — `ORDER BY intake2."CreatedAt"` Memakai Kolom yang Tidak Ada

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI — dibaca langsung di source dan di snapshot migration EF Core terakhir |
| **Perbaikan** | `FIX-FIN-001-01` |

**Apa yang terjadi.** Setiap kali petugas Finance membuka Laporan AR, PostgreSQL menolak query
dan server mengembalikan "Terjadi kesalahan pada server". Laporan sama sekali tidak dapat
dibuka — bukan tampil kosong, bukan tampil sebagian.

**Kenapa terjadi.** Pada `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs:581`, di dalam `LATERAL JOIN` yang mengambil baris handoff Finance AR terbaru:

```sql
-- FinanceBillingArReportService.cs:577-583
LEFT JOIN LATERAL (
    SELECT intake2."Id", intake2."Status", intake2."ErrorMessage"
    FROM "FinBillingHandoffIntake" intake2
    WHERE intake2."SourceHandoffId" = h."Id" AND intake2."HandoffType" = 'AR' AND NOT intake2."IsDelete"
    ORDER BY intake2."CreatedAt" DESC
    LIMIT 1
) intake ON true
```

Kolom `"CreatedAt"` tidak pernah ada pada tabel `FinBillingHandoffIntake`. Model `FinBillingHandoffIntake` (`Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs:19`) mewarisi `IdentityModel` (`Models/IdentityModel.cs:17`), yang menamai kolom jejak pembuatannya `CreateDateTime` — dikonfirmasi juga pada snapshot migration terbaru `Migrations/20261010115612_AddFinanceReceivableInstallmentAndBenefitSettlement.Designer.cs:4815` yang memetakan `FinBillingHandoffIntake` ke kolom `"CreateDateTime"`, bukan `"CreatedAt"`. Query mentah ini ditulis belum lama dan tidak pernah diuji terhadap database sungguhan, sehingga typo nama kolom ini lolos.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan desain mana pun yang meminta kolom bernama `CreatedAt`; ini murni kesalahan pengetikan terhadap skema yang sudah ada.

**Dampak nyata.** Petugas Finance tidak dapat membuka Laporan AR sama sekali untuk memantau siklus tagihan Billing → Piutang → Invoice AR. Karena query ini dipakai bersama oleh ringkasan, daftar halaman, ekspor Excel, dan ekspor PDF (lihat bagian 6 kontrak), **seluruh** permukaan Laporan AR terdampak sekaligus.

**Rekomendasi.** Ganti `intake2."CreatedAt"` menjadi `intake2."CreateDateTime"`. Rincian opsi dan langkah ada di plan repair.

---

### ISS-FIN-001-02 — `ORDER BY batch2."CreatedAt"` Memakai Kolom yang Tidak Ada

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-001-01` |

**Apa yang terjadi.** Sama seperti `ISS-FIN-001-01` — bagian dari query yang sama yang membuat seluruh Laporan AR gagal total.

**Kenapa terjadi.** Pada baris `FinanceBillingArReportService.cs:613`, di dalam `LATERAL JOIN` yang mengambil batch Invoice AR teraktif milik satu piutang:

```sql
-- FinanceBillingArReportService.cs:608-615
LEFT JOIN LATERAL (
    SELECT bi2."BatchId", batch2."Status" AS "BatchStatus", batch2."BatchNumber"
    FROM "FinReceivableInvoiceBatchItem" bi2
    JOIN "FinReceivableInvoiceBatch" batch2 ON bi2."BatchId" = batch2."Id" AND NOT batch2."IsDelete"
    WHERE bi2."ReceivableId" = r."Id" AND NOT bi2."IsDelete"
    ORDER BY bi2."IsActiveMembership" DESC, batch2."CreatedAt" DESC
    LIMIT 1
) batch_info ON true
```

Sama seperti di atas, `FinReceivableInvoiceBatch` (`Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs:16`) mewarisi `IdentityModel`, dikonfirmasi pada snapshot migration `Migrations/20261010115612_...Designer.cs:9858` memetakan ke kolom `"CreateDateTime"`, bukan `"CreatedAt"`.

**Apakah ini menyimpang dari desain?** `BUG`, dengan sebab yang sama persis dengan `ISS-FIN-001-01`.

**Dampak nyata.** Sama dengan `ISS-FIN-001-01` — keduanya berada di satu `FROM` clause yang sama, sehingga PostgreSQL akan menolak keseluruhan query begitu salah satu `LATERAL JOIN` dievaluasi, terlepas urutan mana yang dibaca optimizer lebih dulu. Memperbaiki hanya satu dari keduanya **tidak** akan membuat Laporan AR berfungsi.

**Rekomendasi.** Ganti `batch2."CreatedAt"` menjadi `batch2."CreateDateTime"`.

---

### ISS-FIN-001-03 — Hook `useReportArCreated` Memanggil `useMemo` Tanpa Impor

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-001-02` |

**Apa yang terjadi.** Begitu petugas membuka menu Report AR Created, seluruh halaman gagal
tampil dan browser menunjukkan pesan generik "This page couldn't load" — bukan pesan galat
Laporan yang biasa (bukan toast merah, bukan tabel kosong), karena ini bukan galat dari respons
API, melainkan galat JavaScript yang terjadi sebelum sempat memanggil API sama sekali.

**Kenapa terjadi.** Pada `src/lib/hooks/finance/receivable/use-report-ar-created.jsx:8`, impor dari
`react` hanya menyebut tiga hook:

```jsx
// use-report-ar-created.jsx:8
import { useCallback, useEffect, useState } from "react";
```

Namun pada baris 126, fungsi hook yang sama memanggil `useMemo`:

```jsx
// use-report-ar-created.jsx:126
const debtorIdsToResolve = useMemo(() => {
  const ids = [];
  if (appliedFilters.debtorReferenceId) ids.push(appliedFilters.debtorReferenceId);
  items.forEach((it) => {
    if (it?.companyId) ids.push(it.companyId);
  });
  return ids;
}, [appliedFilters.debtorReferenceId, items]);
```

Karena `useMemo` tidak pernah diimpor, baris ini melempar `ReferenceError: useMemo is not
defined` setiap kali hook dipanggil — yaitu setiap kali komponen `ReportCreatedView` dirender.
React tidak memiliki cara untuk menampilkan tampilan galat kustom untuk `ReferenceError` yang
terjadi di luar `try/catch` komponen; Next.js App Router menangkapnya di lapisan paling luar dan
menunjukkan pesan generik "This page couldn't load". Berkas ini adalah berkas baru pada komit
`46a1549dc` ("feat: add Settlement AR hook and related styles for billing reports") — satu-satunya
komit yang pernah menyentuhnya — sehingga ini adalah cacat yang terbawa sejak penulisan pertama,
bukan regresi dari perubahan setelahnya.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada keputusan desain yang terlibat; ini murni
kesalahan impor JavaScript.

**Dampak nyata.** Report AR Created (Laporan Pembuatan AR) sama sekali tidak dapat dipakai oleh
siapa pun yang membukanya — bukan hanya saat memakai saringan tertentu.

**Rekomendasi.** Tambahkan `useMemo` ke daftar impor dari `react` pada baris 8.

---

### ISS-FIN-001-T1 — `ResolvePayerNameAsync` Berpotensi Berebut Koneksi dengan `NpgsqlDataReader` yang Masih Terbuka

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan, diminta ditelusuri pelapor: "pastikan filter penjamin tertentu tidak menimbulkan error tambahan") |
| **Jenis** | `BUG` (laten — belum pernah benar-benar terpicu di lapangan) |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI pada source; belum pernah terbukti di lapangan karena `ISS-FIN-001-01`/`02` selalu menggagalkan query lebih dulu, sehingga baris ini tidak pernah tercapai saat ini |
| **Perbaikan** | `FIX-FIN-001-03` |
| **Kenapa dicantumkan** | Begitu `ISS-FIN-001-01`/`02` diperbaiki, jalur kode ini akan mulai benar-benar tereksekusi untuk pertama kalinya saat petugas menyaring laporan dengan satu Penjamin tertentu — risiko ini wajib ditutup bersamaan, bukan ditemukan nanti sebagai bug baru. |

**Apa yang terjadi.** Ketika petugas menyaring Laporan AR berdasarkan satu Penjamin tertentu
(bukan "Semua Perusahaan/Asuransi"), ringkasan laporan berisiko gagal dengan galat database
tambahan — terpisah dari galat `ISS-FIN-001-01`/`02`.

**Kenapa terjadi.** Pada `FinanceBillingArReportService.cs:757-791` (`ExecuteSummaryQueryAsync`):

```csharp
// FinanceBillingArReportService.cs:770-790
await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
if (await reader.ReadAsync(cancellationToken))
{
    var invCount = reader.GetInt32(0);
    // ... baca 12 kolom dari reader ...

    string payerDisplayName = "Semua Perusahaan / Asuransi";
    if (query.PayerId.HasValue && query.PayerId.Value != Guid.Empty)
    {
        payerDisplayName = await ResolvePayerNameAsync(query.PayerId.Value, cancellationToken);
    }
    // reader BELUM di-dispose di titik ini — await using baru dieksekusi
    // setelah method mengembalikan nilai di bawahnya
    ...
}
```

`ResolvePayerNameAsync` (baris 1079) memakai `_dbContext.MstInsuranceProviders`/`MstCompanyGuarantors` — yaitu query EF Core biasa lewat `ApplicationDbContext` yang sama. `ApplicationDbContext` dan `cmd`/`reader` di atasnya memakai **koneksi ADO.NET yang sama** (`_dbContext.Database.GetDbConnection()`, baris 757). Npgsql (seperti kebanyakan provider ADO.NET tanpa MARS) tidak mengizinkan membuka command/query kedua pada satu koneksi selagi `NpgsqlDataReader` dari command pertama masih terbuka. Karena `ResolvePayerNameAsync` dipanggil **di dalam** blok `if (await reader.ReadAsync(...))`, sebelum `reader` sempat didispose oleh `await using`, baris ini berisiko melempar `NpgsqlOperationInProgressException` ("A command is already in progress").

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ada desain yang meminta dua command aktif
sekaligus pada satu koneksi; ini pola penulisan yang keliru, konsisten dengan kekhawatiran yang
sudah ditulis pelapor sendiri di laporan asli (Bagian A §5 brief awal).

**Dampak nyata.** Begitu `ISS-FIN-001-01`/`02` diperbaiki, siapa pun yang menyaring Laporan AR
dengan satu Penjamin spesifik (bukan "Semua") berisiko mendapat "Terjadi kesalahan pada server"
lagi — kali ini dari sumber yang berbeda, dan akan terlihat seperti bug baru jika tidak ditutup
sekarang.

**Rekomendasi.** Pindahkan `ResolvePayerNameAsync` agar dipanggil **setelah** `reader` selesai
dibaca dan dipastikan tertutup (misalnya setelah blok `await using` berakhir, dengan hasil
pembacaan disimpan ke variabel lokal lebih dulu), atau resolve nama penjamin lebih dulu sebelum
membuka `reader` ringkasan sama sekali. Rincian langkah di plan repair.

---

### ISS-FIN-001-T2 — Dokumen Desain Frontend Masih Menyebut Kontrak API Lama untuk Report AR Created

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `DESIGN_CHANGE` (dokumen tertinggal dari source yang sudah lebih maju) |
| **Area** | Dokumentasi (Frontend) |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-001-04` |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri apakah frontend memakai endpoint Report AR Created yang benar — relevan langsung dengan `ISS-FIN-001-03`. |

**Apa yang terjadi.** Dokumen arsitektur frontend finance-management mencatat kontrak API untuk
Report AR Created sebagai endpoint generik lama, padahal implementasi yang berjalan sekarang
sudah memakai endpoint detail resmi yang lebih baru dan lebih lengkap.

**Kenapa terjadi.** `docs/module-blueprints/finance-management/03-frontend-architecture.md:741`
masih menulis:

```text
| `FIN-LYR-AR-12` Report AR Created | `GET /receivables` | diurutkan tanggal terbit |
```

Baris ini berasal dari implementasi V1 (`FE-FIN-017`, laporan tracked-nya ada di
`task/report/frontend/FE-FIN-017.md:227`, yang reuse `buildReceivableColumns()` di atas
`GET /receivables` generik). Working tree saat ini sudah memakai implementasi V2: hook
`use-created-receivable-list.jsx` kini hanyalah *compatibility wrapper* yang mengarahkan ke
`useReportArCreated` (`src/lib/hooks/finance/receivable/use-report-ar-created.jsx`), yang
memanggil endpoint detail resmi `GET /api/v1/corporate/finance-management/receivable-invoice-batches/reports/created`
beserta `.../reports/created/summary` dan `.../reports/created/export/{excel|pdf}` — didukung
method service backend khusus `GetReportArCreatedPagedAsync`/`GetReportArCreatedSummaryAsync`
(`FinanceReceivableInvoiceBatchService.cs:3448-3774`) yang mengembalikan detail per-baris tagihan
(`FinReceivableInvoiceBatchItem`), bukan daftar piutang generik. Implementasi V2 ini tidak
tercatat di roadmap manapun (`grep` atas `roadmap/*.md` untuk "reports/created" dan "Laporan
Pembuatan AR" tidak menemukan entri task), sehingga kemungkinan dikerjakan di luar proses task
yang terlacak, pada komit yang sama dengan `ISS-FIN-001-03` (`46a1549dc`).

**Apakah ini menyimpang dari desain?** `DESIGN_CHANGE` — bukan berarti source salah; justru
source sudah lebih maju dari dokumen. `FE-FIN-017.md:288` baris "Masalah yang diketahui" poin (2)
bahkan **sudah memperingatkan sendiri** bahwa `03-frontend-architecture.md` §17.2 harus
diperbarui agar tidak menyesatkan task lanjutan — peringatan itu belum pernah ditindaklanjuti.

**Dampak nyata.** Belum ada dampak fungsional (halaman tetap memakai kontrak yang benar).
Risikonya murni ke depan: agent atau pengembang lain yang membaca `03-frontend-architecture.md`
tanpa membaca source akan mengira Report AR Created masih memakai `GET /receivables`, lalu
"memperbaiki" source agar kembali cocok dengan dokumen yang sudah usang.

**Rekomendasi.** Perbarui baris kontrak `FIN-LYR-AR-12` pada `03-frontend-architecture.md` agar
menyebut endpoint V2 yang sebenarnya dipakai. Revisi dokumen ini **tidak** termasuk kewenangan
`diagnose-module-issue` (lihat kontrak plan repair bagian 8) — dicatat sebagai dokumen hulu yang
perlu direvisi lewat jalur blueprint yang berwenang.

---

## 5. Tanya-Jawab Pelapor

Laporan ini tidak memuat pertanyaan eksplisit dari pelapor (brief berbentuk instruksi perbaikan
terstruktur, bukan pertanyaan). Tidak ada yang perlu dijawab pada bagian ini.

---

## 6. Temuan Tambahan

Diuraikan penuh pada `ISS-FIN-001-T1` dan `ISS-FIN-001-T2` di bagian 4. Keduanya berada di modul
yang sama (`finance-management`), sehingga perbaikannya dimasukkan ke rencana yang sama.

Satu pengamatan tambahan yang **tidak** dijadikan temuan terpisah (karena bukan cacat, hanya
konteks): keempat endpoint Laporan AR (`billing-ar`, `billing-ar/payer-options`,
`billing-ar/export/excel`, `billing-ar/export/pdf`) seluruhnya memanggil `BuildFromWhereClause`,
`ExecuteSummaryQueryAsync`, dan/atau `ExecutePagedItemsQueryAsync` yang sama — kecuali
`payer-options`, yang memakai LINQ EF Core murni ke `MstInsuranceProviders`/`MstCompanyGuarantors`
dan **tidak terdampak** `ISS-FIN-001-01`/`02`/`T1` sama sekali. Audit juga memastikan
`FinanceReceivableInvoiceBatchService.cs` (termasuk seluruh query di balik Report AR Created) dan
`FinanceReceivableInvoiceBatchService.cs:2112` (satu-satunya `ExecuteSqlRawAsync` lain di area ini,
sebuah `pg_advisory_xact_lock` untuk locking, tidak terkait kolom tabel manapun) tidak memiliki
typo kolom sejenis.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik (Product Owner / Pelapor)

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| :---: | --- | --- | --- | --- |
| K-01 | Mengesahkan perbaikan `FIX-FIN-001-01` (dua nama kolom salah), `FIX-FIN-001-02` (impor `useMemo` hilang), dan `FIX-FIN-001-03` (urutan baca reader/koneksi) untuk segera dieksekusi lewat `build-module-backend`/`build-module-frontend` | Setuju / Tunda / Tolak | **Setuju** — ketiganya murni perbaikan teknis (nama kolom, impor, urutan eksekusi), tidak mengubah business logic, hasil perhitungan, nominal, atau kontrak API yang sudah benar, sesuai batasan yang pelapor sendiri tetapkan di laporan asli | `FIX-FIN-001-01`, `FIX-FIN-001-02`, `FIX-FIN-001-03` |
| K-02 | Mengesahkan revisi dokumentasi `FIX-FIN-001-04` (`03-frontend-architecture.md` §17 kontrak `FIN-LYR-AR-12`) agar sesuai source V2 yang berjalan saat ini | Setuju / Tunda / Tolak | **Setuju** — revisi dokumen murni, nol dampak ke source/perilaku aplikasi | `FIX-FIN-001-04` |

Catatan: laporan asli pelapor sudah menyatakan secara eksplisit prioritas P0 untuk perbaikan nama
kolom SQL dan error runtime Report AR Created, serta batasan "jangan mengubah business logic,
hasil perhitungan, nominal transaksi, status pembayaran, atau kontrak API yang sudah benar" —
ketiga `FIX` teknis di atas berada tepat di dalam batasan tersebut. Dokumen ini tetap menunggu
konfirmasi tertulis pemilik sebelum rencana berstatus `DISETUJUI`, karena persetujuan rencana
adalah langkah yang dikerjakan manusia, bukan bagian dari skill ini.

---

## 8. Catatan Pola

1. **Query mentah tanpa uji terhadap database sungguhan lolos ke produksi.** `ISS-FIN-001-01`/`02`
   adalah typo nama kolom yang seharusnya langsung gagal pada percobaan eksekusi pertama kalinya
   terhadap database manapun — ini menunjukkan `FinanceBillingArReportService` belum pernah
   benar-benar dieksekusi terhadap PostgreSQL sejak ditulis. Pencegahan: setiap service yang
   memakai SQL mentah (`CreateCommand`/`ExecuteReaderAsync`) wajib diuji lewat setidaknya satu
   pemanggilan nyata (manual atau integration test) terhadap database bermigrasi, bukan hanya
   `dotnet build`, karena query string tidak diperiksa compiler.
2. **Build frontend tidak pernah dijalankan untuk fitur yang memperkenalkan hook baru.** Laporan
   task `FE-FIN-017.md:244-245` secara eksplisit mencatat `npm run build` **NOT RUN** untuk
   generasi pertama rumpun layar laporan A/R ini. Generasi V2 (`use-report-ar-created.jsx`, komit
   `46a1549dc`) tampaknya mengalami hal yang sama — `ISS-FIN-001-03` adalah persis jenis galat
   (`ReferenceError` pada impor yang hilang) yang akan tertangkap langsung oleh `npm run build`
   atau bahkan `npm run lint:errors` (aturan `no-undef`/`react-hooks/rules-of-hooks` ESLint),
   tetapi lolos karena keduanya tidak pernah dijalankan untuk berkas ini.
3. **Implementasi V2 dibangun di luar proses task yang terlacak.** `use-report-ar-created.jsx` dan
   endpoint backend `reports/created` yang menyertainya tidak memiliki entri roadmap maupun
   laporan task tracked manapun (lihat `ISS-FIN-001-T2`). Ini menjelaskan kenapa dokumen desain
   (`03-frontend-architecture.md`) tidak ikut diperbarui — revisi dokumen biasanya terjadi sebagai
   bagian dari menutup sebuah task, dan di sini tidak ada task yang ditutup.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-11 | Issue dibuat dari laporan 2 gejala (Laporan AR gagal total, Report AR Created gagal dimuat); 3 temuan utama dan 2 temuan tambahan dirinci dengan status TERBUKA, menunggu persetujuan pemilik atas plan repair. | `diagnose-module-issue` (Claude) |
| 2026-10-11 | Pemilik menyetujui K-01 dan memerintahkan eksekusi via `build-module-backend`/`build-module-frontend`. `ISS-FIN-001-01`, `-02`, dan `-T1` ditutup source-nya oleh `BE-FIN-101`; `ISS-FIN-001-03` ditutup source-nya oleh `FE-FIN-042` — keduanya 🟡 SEBAGIAN, menunggu konfirmasi build/verifikasi runtime. Status issue: `DALAM_PERBAIKAN`. `ISS-FIN-001-T2` (K-02) belum dieksekusi. | `build-module-backend` + `build-module-frontend` (Claude) |
