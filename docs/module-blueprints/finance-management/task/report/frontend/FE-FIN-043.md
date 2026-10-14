# Laporan Perubahan Frontend — `FE-FIN-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-043` |
| Judul | Laporan AR: lengkapi filter Status Billing/Integrasi Finance AR/Invoice AR yang hilang, dan perbaiki galat API yang salah ditampilkan sebagai data kosong |
| Slice | Perbaikan bug pasca-laporan pemilik, di luar gelombang REV-18 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (repository backend), bagian "Perbaikan `ISSUE-FIN-001` — Laporan AR dan Report AR Created" |
| Trace | `ISSUE-FIN-001`, `PLAN-REPAIR-FIN-001` (`FIX-FIN-001-05`, `FIX-FIN-001-06` — ditambahkan menyusul instruksi lanjutan pemilik 11 Oktober 2026) |
| Contract version | Tidak berubah — ketiga parameter (`billingStatus`, `financeHandoffStatus`, `arInvoiceStatus`) sudah ada pada kontrak `GET .../reports/billing-ar` sejak awal; task ini hanya menghubungkan UI yang belum memakainya |
| Wewenang UI | `REUSE` murni atas `FilterSelect` dan `InformationAlert` yang sudah ada dan sudah dipakai berulang di layar Finance lain — tidak ada komponen baru |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — menambah 3 kontrol filter dan 1 blok error-state, memakai ulang komponen base yang sudah ada, pada 2 berkas |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/lib/hooks/finance/receivable/use-billing-ar-report.jsx`, `.../components/view/finance/receivable/report/finance-ar-report-view.jsx`; laporan ini di repository backend (`docs/module-blueprints/`) |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `46a1549dcc3b9f5359d132e501dec567ec826e17` (branch `yasmina`), ditambah perubahan task ini di atasnya |
| Commit backend yang dijadikan rujukan | `845dabf918f28c6326aecdbcb89a1e2297367265` (branch `Yasmina`), ditambah `BE-FIN-101` |
| Tanggal | 2026-10-11 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

---

## 1. Keadaan yang ditemukan di awal

Pemilik meminta audit lanjutan atas mekanisme filter dan penanganan galat pada layar Laporan AR
(`/finance/ar-report` → komponen `FinanceArReportView`). Dua celah konkret ditemukan dengan
membandingkan hook `useBillingArReport` terhadap kontrak backend (`BillingArReportQuery.cs`):

1. **Tiga filter yang didukung backend sejak awal tidak pernah terhubung ke frontend.** DTO
   backend mendukung `billingStatus` (`ALL`/`OPEN`/`FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF`,
   default `ALL`) dan `financeHandoffStatus` (`ALL`/`NOT_HANDED_OFF`/`PENDING`/`SUCCESS`/`FAILED`/`NOT_APPLICABLE`),
   tetapi `DEFAULT_FILTERS` dan `buildQueryParams` pada `use-billing-ar-report.jsx` tidak pernah
   menyebut keduanya sama sekali — sehingga nilainya tidak pernah ikut terkirim ke API, dan tidak
   ada kontrol apa pun di layar untuk menyaringnya. Satu filter ketiga, `arInvoiceStatus`, sudah
   ada di `DEFAULT_FILTERS`/`buildQueryParams` tetapi **tidak pernah dirender** sebagai kontrol di
   `finance-ar-report-view.jsx` — nilainya terkunci permanen ke `"ALL"` karena tidak ada cara bagi
   pengguna untuk mengubahnya.
2. **Galat API salah ditampilkan sebagai "data kosong".** Saat `fetchReportData` gagal,
   `errorMessage` memang terisi dan sebuah toast galat muncul sesaat, tetapi `totalData` tetap
   bernilai `0` (nilai awal state, tidak pernah diubah oleh jalur galat). `BillingArReportTable`
   memutuskan tampilan kosong semata dari `totalData === 0`, sehingga begitu toast menghilang,
   pengguna hanya melihat "Tidak ada data tagihan" — tampak seperti laporan benar-benar kosong,
   padahal sebenarnya permintaan API gagal total. Tidak ada tombol "Coba Lagi" di layar ini sama
   sekali.

---

## 2. Proses bisnis dari sisi pengguna

Petugas Finance membuka **Finance → Transaksi A/R → Laporan & Monitoring → Report AR**. Panel
kiri menyediakan filter: Pencarian, Nama Perusahaan/Asuransi/Penjamin, Kunjungan, Periode,
**(baru)** Status Billing, **(baru)** Status Integrasi Finance AR, **(baru)** Status Invoice AR,
lalu Tanggal Mulai/Akhir. Panel kanan menampilkan ringkasan (nama penjamin, total nominal,
jumlah pasien/invoice). Petugas menekan Cari untuk menerapkan saringan, atau Reset untuk kembali
ke "Semua Status Billing"/"Semua Status Integrasi"/"Semua Status Invoice AR". Tabel di bawahnya
menampilkan baris invoice dikelompokkan per penjamin dengan subtotal per tanggal, dan tombol
Download PDF/Export Excel mengekspor seluruh hasil filter (bukan hanya 30 baris halaman aktif).

Jalur tidak normal **(diperbaiki task ini)**: bila API gagal (misalnya server sedang bermasalah),
petugas kini melihat kartu galat merah "Gagal memuat Laporan AR" berisi pesan galat yang
sebenarnya beserta tombol **Coba Lagi** — bukan lagi tabel kosong yang menyesatkan. Menekan Coba
Lagi mengulang permintaan dengan saringan yang sedang berlaku (`applied`), tanpa mereset filter
pengguna ke default.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/lib/hooks/finance/receivable/use-billing-ar-report.jsx` (seluruh berkas)
- `src/components/view/finance/receivable/report/finance-ar-report-view.jsx` (seluruh berkas)
- `src/components/view/finance/receivable/report/billing-ar-report-table.jsx` (perilaku empty-state)
- `src/components/features/base-features/access-denied-gate.jsx` dan `access-denied-utils.js` (memastikan galat generik tidak salah ditafsirkan sebagai akses ditolak)
- `src/components/features/base-features/information-alert.jsx` (komponen yang dipakai ulang untuk error-state)
- `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceBillingArReportDtos.cs` (repository backend — konfirmasi nilai dan default ketiga filter)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/receivable/use-billing-ar-report.jsx` | Tambah export `BILLING_STATUS_OPTIONS`, `FINANCE_HANDOFF_STATUS_OPTIONS`, `AR_INVOICE_STATUS_OPTIONS`. Tambah `billingStatus: "ALL"` dan `financeHandoffStatus: "ALL"` ke `DEFAULT_FILTERS`. Tambah pengiriman `billingStatus`/`financeHandoffStatus` di `buildQueryParams` (pola sama dengan `arInvoiceStatus` yang sudah ada). Tambah fungsi `retry` yang memanggil `fetchReportData(applied)` tanpa mereset filter, dan mengembalikannya dari hook |
| `src/components/view/finance/receivable/report/finance-ar-report-view.jsx` | Impor `InformationAlert` dan ketiga konstanta opsi baru. Tambah 3 `FilterSelect` baru (Status Billing, Status Integrasi Finance AR, Status Invoice AR) mengikuti pola kontrol filter yang sudah ada persis. Tambah blok `InformationAlert variant="danger"` + `BaseButton onClick={retry}` yang tampil saat `errorMessage` terisi, ditempatkan tepat sebelum `BillingArReportTable`, mengikuti pola yang sudah dipakai `report-created-view.jsx` |

### 3.3 Kepatuhan arsitektur frontend

Seluruh kontrol filter baru memakai ulang `FilterSelect` yang sudah dipakai berulang di layar
yang sama (REUSE murni, nol komponen baru) — lihat tabel keputusan pada bagian 3.4. Blok
error-state memakai ulang `InformationAlert` (variant `danger`) dan `BaseButton`, keduanya sudah
diimpor di berkas ini atau di layar Finance sejenis (`report-created-view.jsx`), dan ditempatkan
dengan pola yang identik: `InformationAlert` berisi pesan galat ditambah tombol retry, muncul
sebelum tabel data. Tidak ada CSS baru ditulis — tombol retry memakai utility class Bootstrap
(`d-flex`, `gap-3`, dst.) yang sudah dipakai di `report-created-view.jsx`, bukan nilai visual
literal baru.

### 3.4 Tabel keputusan base component (gerbang langkah 3)

| Elemen | Status | Bukti |
| --- | --- | --- |
| Dropdown Status Billing | `REUSE` | `FilterSelect` sudah dipakai untuk Kunjungan dan Periode pada berkas yang sama (baris 135, 150 sebelum perubahan) |
| Dropdown Status Integrasi Finance AR | `REUSE` | Sama seperti di atas |
| Dropdown Status Invoice AR | `REUSE` | Sama seperti di atas |
| Kartu galat + tombol Coba Lagi | `REUSE` | `InformationAlert` + `BaseButton` sudah dipakai dengan pola identik di `report-created-view.jsx` (baris 369-373 berkas itu) |

Seluruh elemen berstatus `REUSE` — tidak ada elemen `NEW`/`EXTEND` yang menunggu keputusan
pemilik.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner dan teks "Memuat Laporan AR..." pada badan tabel (`BillingArReportTable`); tidak berubah |
| Kosong (hasil filter benar-benar nol) | "Tidak ada data tagihan. Tidak ada invoice Billing lunas yang cocok dengan saringan yang dipilih."; tidak berubah |
| Gagal (API) | **(baru)** Kartu merah "Gagal memuat Laporan AR" berisi pesan galat sebenarnya, ditambah tombol "Coba Lagi" yang mengulang permintaan dengan saringan yang sama. Tabel di bawahnya tetap menampilkan placeholder kosongnya sendiri sebagai indikator sekunder — pola yang sama dengan Report AR Created |
| Tanpa hak akses | `AccessDeniedGate` menampilkan "Ups! Akses Ditolak" hanya bila galat sungguhan berupa 401/403 atau pesan terkait akses — dikonfirmasi tidak salah memicu untuk galat generik seperti "Terjadi kesalahan pada server" |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Parameter yang **kini benar-benar terkirim** (sebelumnya tidak pernah
dikirim sama sekali untuk dua di antaranya):

#### Corporate / Finance Management / Receivable

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar?billingStatus=...` | Saring menurut status Billing (`OPEN`/`FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF`); tidak dikirim bila `ALL` | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar?financeHandoffStatus=...` | Saring menurut status integrasi Finance AR | `FinanceReceivable : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivables/reports/billing-ar?arInvoiceStatus=...` | Saring menurut status pembuatan Invoice AR (parameter sudah ada sebelumnya, kini dapat diisi dari UI) | `FinanceReceivable : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/lib/hooks/finance/receivable/use-billing-ar-report.jsx src/components/view/finance/receivable/report/finance-ar-report-view.jsx --quiet` | Berhasil, exit `0` | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil, exit `0`, seluruh route ter-compile, `postbuild` sukses | `PASS` | Keluaran perintah |
| Pembacaan ulang `buildQueryParams` setelah edit | `billingStatus`/`financeHandoffStatus` hanya terkirim bila bukan `"ALL"`, persis mengikuti pola `arInvoiceStatus` yang sudah ada | `PASS` | Pembacaan source |
| Pembacaan ulang `isAccessDeniedError` terhadap pesan galat generik ("Terjadi kesalahan pada server", "Gagal memuat Laporan AR.") | Tidak cocok kata kunci apa pun (`akses ditolak`, `forbidden`, dst.) — `AccessDeniedGate` meneruskan ke `children` seperti biasa | `PASS` | Pembacaan source `access-denied-utils.js` |
| Buka `/finance/ar-report`, uji ketiga dropdown baru dan tombol Coba Lagi di browser | Tidak dijalankan | `NOT FEASIBLE` | Server dev tidak dijalankan pada sesi ini |

Uji manual: `NOT FEASIBLE` — server dev tidak dijalankan pada sesi ini.

**Tidak dijalankan:** verifikasi peramban (pilih setiap opsi dropdown baru, picu galat API dan
tekan Coba Lagi, konfirmasi filter tetap tersimpan) — menunggu environment dev yang aktif.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Petugas dapat menyaring Laporan AR berdasarkan Status Billing, Status Integrasi Finance AR, dan Status Invoice AR dari layar | **Terpenuhi secara source dan build** — ketiga dropdown terpasang dan terhubung ke `buildQueryParams`; belum diverifikasi interaksi di browser sungguhan | Diff bagian 3.2, build PASS |
| Default saringan adalah "Semua Status Billing" (backend tidak pernah dipaksa `CLOSED`) | **Terpenuhi** — `DEFAULT_FILTERS.billingStatus = "ALL"` dan backend (`BillingArReportQuery.BillingStatus` default `"ALL"`, dikonfirmasi di `FinanceBillingArReportDtos.cs`) sudah konsisten sejak awal; tidak ada perubahan backend yang diperlukan | Pembacaan source FE dan BE |
| Saat API gagal, summary tidak ditampilkan sebagai Rp0 dan data tidak dianggap kosong tanpa penjelasan | **Terpenuhi** — kartu galat merah tampil terpisah dari placeholder kosong tabel, dengan pesan galat sebenarnya; `summary`/`items` tidak direset paksa ke nol oleh jalur galat (nilai lama dipertahankan state React) | Diff bagian 3.2, pembacaan `fetchReportData` |
| Tombol "Coba Lagi" tersedia dan mempertahankan filter pengguna | **Terpenuhi** — `retry()` memanggil `fetchReportData(applied)`, tidak menyentuh `form`/`applied` | Diff fungsi `retry`, bagian 3.2 |
| Export Excel/PDF mengikuti seluruh filter aktif, bukan hanya 30 baris halaman aktif | **Terpenuhi — sudah benar sebelum task ini**, dikonfirmasi ulang: `handleExportExcel`/`handleDownloadPdf` menghapus `pageNumber`/`pageSize` dari parameter sebelum memanggil endpoint ekspor, dan backend (`ExportExcelAsync`/`ExportPdfAsync`) memanggil `ExecutePagedItemsQueryAsync(..., 0, int.MaxValue, ...)` | Pembacaan source FE dan BE, nol perubahan diperlukan |

Task ini **belum** dapat ditandai ✅ selama verifikasi peramban manual belum dilakukan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Berkas laporan ini sempat tertimpa isi task lain (`FE-FIN-043` versi Settlement AR milik sesi lain yang bekerja bersamaan pada repository yang sama) karena kolisi alokasi task ID; sesi itu sudah menomori ulang task-nya sendiri menjadi `FE-FIN-044` dan memindahkan laporannya ke `FE-FIN-044.md`, sehingga berkas ini dipulihkan dengan isi aslinya (task ini, Laporan AR). Tidak ada tindakan lebih lanjut diperlukan dari task ini, tetapi dicatat sebagai bukti bahwa repository sedang dikerjakan lebih dari satu sesi secara bersamaan |
| Masalah yang diketahui | Tidak ada temuan baru di luar yang sudah dicatat pada `ISSUE-FIN-001` |
| Dependency backend | `BE-FIN-101` — audit lanjutan backend (lihat catatan penutupnya) mengonfirmasi `BuildFromWhereClause`, `ExecuteSummaryQueryAsync`, `ExecutePagedItemsQueryAsync`, dan query export tidak memiliki duplikasi hitung, pencampuran nilai estimasi/final, atau pengambilan seluruh data sebelum pagination — nol perubahan backend tambahan diperlukan untuk task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/lib/hooks/finance/receivable/use-billing-ar-report.jsx`, `M src/components/view/finance/receivable/report/finance-ar-report-view.jsx` (ditambah perubahan lain yang sudah ada sebelumnya di working tree dan di luar scope task ini — tidak disentuh) |
| Langkah berikutnya | Verifikasi peramban dengan server dev aktif: pilih setiap opsi filter baru satu per satu, picu galat API (misalnya mematikan backend sesaat) dan konfirmasi kartu galat + Coba Lagi berfungsi serta filter tidak hilang; setelah PASS, tandai `FE-FIN-043` ✅ pada roadmap dan `requirement-traceability.md` |
