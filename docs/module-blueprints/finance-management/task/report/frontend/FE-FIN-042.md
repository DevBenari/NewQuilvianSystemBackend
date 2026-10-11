# Laporan Perubahan Frontend — `FE-FIN-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-042` |
| Judul | Report AR Created: tambahkan impor `useMemo` yang hilang pada `useReportArCreated` |
| Slice | Perbaikan bug pasca-laporan pemilik, di luar gelombang REV-18 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (repository backend), bagian "Perbaikan `ISSUE-FIN-001` — Laporan AR dan Report AR Created" |
| Trace | `ISSUE-FIN-001` (`ISS-FIN-001-03`), `PLAN-REPAIR-FIN-001` (`FIX-FIN-001-02`) |
| Contract version | Tidak berubah — endpoint yang dikonsumsi (`GET .../receivable-invoice-batches/reports/created` dan turunannya) tetap sama persis |
| Wewenang UI | Tidak ada perubahan tampilan apa pun — perbaikan satu baris impor JavaScript pada hook data, nol komponen/JSX disentuh |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — satu baris impor pada satu berkas |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/lib/hooks/finance/receivable/use-report-ar-created.jsx`; laporan ini di repository backend (`docs/module-blueprints/`) |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `46a1549dcc3b9f5359d132e501dec567ec826e17` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `845dabf918f28c6326aecdbcb89a1e2297367265` (branch `Yasmina`) |
| Tanggal | 2026-10-11 |
| Status | 🟡 **Sebagian** — source selesai, lint PASS, `npm run build` **PASS** (dijalankan atas instruksi eksplisit pemilik pada permintaan lanjutan); verifikasi peramban manual tetap **NOT FEASIBLE** (server dev tidak dijalankan) |

---

## 1. Keadaan yang ditemukan di awal

Halaman Report AR Created (`/finance/ar-report/created`) menampilkan "This page couldn't load"
setiap kali dibuka — bukan galat API, bukan tabel kosong, melainkan seluruh halaman gagal
dirender. Penyebabnya ditemukan di `src/lib/hooks/finance/receivable/use-report-ar-created.jsx`:
hook `useReportArCreated` memanggil `useMemo` pada baris 126 untuk menghitung
`debtorIdsToResolve`, tetapi baris impor `react` (baris 8) hanya menyebut
`useCallback, useEffect, useState` — `useMemo` tidak pernah diimpor. Setiap kali komponen
`ReportCreatedView` dirender, baris ini melempar `ReferenceError: useMemo is not defined`, dan
Next.js App Router menangkapnya sebagai galat generik di lapisan paling luar.

Pemeriksaan juga memastikan hook ini **sudah** memanggil endpoint V2 yang benar
(`GET .../receivable-invoice-batches/reports/created` dan turunannya) — laporan awal pelapor
sempat menduga halaman ini masih memakai endpoint generik `/receivables`, tetapi itu sudah tidak
akurat untuk working tree saat ini; `use-created-receivable-list.jsx` kini hanyalah *compatibility
wrapper* ke `useReportArCreated`. Celah yang sebenarnya menahan halaman ini hanyalah satu baris
impor yang hilang.

---

## 2. Proses bisnis dari sisi pengguna

Petugas Finance membuka menu **Finance → Transaksi A/R → Laporan & Monitoring → Laporan Pembuatan
AR** (Report AR Created), baik lewat navigasi sidebar maupun langsung di URL
`/finance/ar-report/created`. Halaman menampilkan panel Filter Data (kata kunci, penjamin, jenis
kunjungan, dasar tanggal, periode) di kiri dan Ringkasan Laporan (penjamin terpilih, total
tagihan, jumlah Invoice AR, ringkasan per jenis kunjungan) di kanan, lalu tabel detail tagihan di
bawahnya, dikelompokkan per penjamin dengan subtotal per tanggal. Petugas dapat menerapkan
filter, mereset, berpindah halaman, serta mencetak/mengekspor ke Excel/PDF. Jalur tidak normal:
memuat (label "..." pada kartu ringkasan), gagal memuat data dari API (pesan galat beserta toast,
filter tetap tersimpan), dan kosong (tombol ekspor nonaktif, tabel menampilkan keadaan kosong).
Sebelum perbaikan ini, jalur yang benar-benar terjadi adalah **galat render React** yang tidak
tertangani sama sekali oleh keempat state di atas — halaman gagal sebelum sempat memanggil API.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/lib/hooks/finance/receivable/use-report-ar-created.jsx` (seluruh berkas)
- `src/lib/hooks/finance/receivable/use-created-receivable-list.jsx` (wrapper kompatibilitas)
- `src/components/view/finance/ar-report/report-created-view.jsx`
- `src/components/view/finance/ar-report/report-created-table.jsx`
- `src/lib/hooks/finance/receivable/use-company-guarantor-name-resolver.jsx`
- `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx` (konfirmasi endpoint V2 sudah benar)
- `docs/module-blueprints/finance-management/docs/issue/issue-001-laporan-ar-dan-report-ar-created.md` dan `docs/plan-repair/plan-repair-001-laporan-ar-dan-report-ar-created.md` (repository backend)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/receivable/use-report-ar-created.jsx` | Baris 8: `import { useCallback, useEffect, useState } from "react";` → `import { useCallback, useEffect, useMemo, useState } from "react";`. Tidak ada baris lain yang diubah |

### 3.3 Kepatuhan arsitektur frontend

Perbaikan ini tidak menambah pola baru, folder baru, atau komponen baru — murni melengkapi impor
yang hilang pada hook data yang sudah ada di lokasi yang benar
(`src/lib/hooks/finance/receivable/`). Pola `useMemo` yang dipanggil pada baris 126 sendiri sudah
konsisten dengan pola yang sama di `report-created-view.jsx` dan `report-created-table.jsx`
(keduanya mengimpor `useMemo` dengan benar), serta `use-company-guarantor-name-resolver.jsx`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Label "..." pada kartu Ringkasan Laporan; tidak berubah oleh perbaikan ini |
| Kosong | `totalData === 0`, tombol ekspor nonaktif, tabel menampilkan keadaan kosong bawaan `ReportCreatedTable`; tidak berubah |
| Gagal (API) | `errorMessage` terisi, `InformationAlert variant="danger"` tampil, toast galat muncul; tidak berubah |
| Tanpa hak akses | `AccessDeniedGate` membungkus seluruh halaman; tidak berubah |
| Galat render (sebelum perbaikan) | Seluruh halaman gagal dengan "This page couldn't load" — **ini yang diperbaiki task ini**. Setelah perbaikan, halaman merender normal dan keempat state di atas kembali berfungsi sebagaimana mestinya |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint yang berubah. Endpoint yang **kembali dapat diakses** (karena halaman yang
memanggilnya kini berhasil dirender) adalah endpoint V2 yang sudah ada:

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches/reports/created` | Daftar detail tagihan Laporan Pembuatan AR terpaginasi | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches/reports/created/summary` | Ringkasan Laporan Pembuatan AR | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches/reports/created/export/excel` | Ekspor Excel | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches/reports/created/export/pdf` | Ekspor PDF | `FinanceReceivableInvoiceBatch : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/lib/hooks/finance/receivable/use-report-ar-created.jsx --quiet` | Berhasil, exit `0` | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil, exit `0`, seluruh route (termasuk `/finance/ar-report/created`) ter-compile, `postbuild` (`prepare-standalone.mjs`) sukses | `PASS` | Keluaran perintah |
| Pembacaan ulang baris 8 dan 126 setelah edit | Impor `useMemo` ada, dipakai tepat satu kali, logika `useMemo` tidak berubah | `PASS` | Pembacaan source |
| Buka `/finance/ar-report/created` di browser | Tidak dijalankan | `NOT FEASIBLE` | Server dev (`npm run dev`) tidak dijalankan pada sesi ini |

Uji manual: `NOT FEASIBLE` — server dev tidak dijalankan pada sesi ini.

**Tidak dijalankan:** verifikasi peramban (buka halaman, filter, paginasi, ekspor) — menunggu
environment dev yang aktif. `npm run build` sudah dijalankan dan PASS (lihat bagian 6).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `/finance/ar-report/created` tidak lagi menampilkan "This page couldn't load" | **Terpenuhi secara build** — `npm run build` berhasil mengkompilasi route ini tanpa galat; belum diverifikasi di browser sungguhan | `npm run build` PASS, bagian 6 |
| Tidak ada `ReferenceError`/galat React lain di console | **Terpenuhi secara source dan build** (akar penyebabnya, impor yang hilang, sudah ditambahkan); belum diverifikasi di browser sungguhan | Lint PASS, build PASS, pembacaan source |
| `debtorIdsToResolve` tetap hanya dihitung ulang saat `appliedFilters.debtorReferenceId`/`items` berubah | **Terpenuhi** — logika `useMemo` tidak disentuh, hanya impornya yang ditambahkan | Diff baris 8, bagian 3.2 |

Task ini **belum** dapat ditandai ✅ selama verifikasi peramban manual belum dilakukan (server dev
tidak dijalankan pada sesi ini) — build dan lint sudah PASS.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemilik awalnya meminta agar build tidak dijalankan otomatis (dinyatakan untuk `dotnet build`). Permintaan lanjutan pemilik secara eksplisit meminta `npm run build` dijalankan — dilakukan dan PASS. Batasan `dotnet build` (backend) tetap berlaku terpisah |
| Masalah yang diketahui | `FIX-FIN-001-04` (revisi `03-frontend-architecture.md` agar menyebut endpoint V2 ini) belum dikerjakan — task dokumentasi terpisah, belum diminta eksplisit pada sesi ini |
| Dependency backend | `BE-FIN-101` (perbaikan Laporan AR) berjalan independen — Report AR Created memakai endpoint yang berbeda (`receivable-invoice-batches/reports/created`, bukan `receivables/reports/billing-ar`) dan tidak terdampak bug `ISSUE-FIN-001` sisi backend |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/lib/hooks/finance/receivable/use-report-ar-created.jsx` (ditambah perubahan lain yang sudah ada sebelumnya di working tree dan di luar scope task ini: `pagination.jsx`, `finance-ar-aging-detail-view.jsx`, `finance-ar-aging-payers-view.jsx`, `base-features/pagination.jsx` — tidak disentuh) |
| Langkah berikutnya | Verifikasi peramban (buka halaman, filter, paginasi, ekspor) dengan server dev aktif sesuai `PLAN-REPAIR-FIN-001` bagian 8; setelah PASS, tandai `FE-FIN-042` ✅ pada roadmap dan `requirement-traceability.md` |
