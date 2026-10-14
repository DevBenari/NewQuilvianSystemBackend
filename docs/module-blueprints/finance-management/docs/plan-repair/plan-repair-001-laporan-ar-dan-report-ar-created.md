# PLAN-REPAIR-001 — Perbaikan Nama Kolom SQL Laporan AR dan Impor React Report AR Created

```yaml
plan_id: PLAN-REPAIR-FIN-001
issue: ../issue/issue-001-laporan-ar-dan-report-ar-created.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-10-11"
diputuskan_oleh: "Pemilik — K-01 (FIX-FIN-001-01, 02, 03, 05, 06) disetujui lewat instruksi chat langsung \"lakukan perbaikan melalui /quilvian-engineering-skills:build-module-backend dan /quilvian-engineering-skills:build-module-frontend\"; K-02 (FIX-FIN-001-04) disetujui lewat instruksi chat langsung \"kerjakan FIX-FIN-001-04\""
tanggal_keputusan: "2026-10-11"
basis_source_backend: "845dabf918f28c6326aecdbcb89a1e2297367265 (Yasmina)"
basis_source_frontend: "46a1549dcc3b9f5359d132e501dec567ec826e17 (yasmina)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | --- | --- | --- |
| `FIX-FIN-001-01` | `ISS-FIN-001-01`, `ISS-FIN-001-02` | Ganti `intake2."CreatedAt"` dan `batch2."CreatedAt"` menjadi `"CreateDateTime"` pada `BuildFromWhereClause` | Backend | 1 | `BE-FIN-101` | 🟡 SEBAGIAN — source selesai, `dotnet build` belum dikonfirmasi (2/3 acceptance criteria terpenuhi secara source; runtime belum diverifikasi) | [laporan BE-FIN-101](../../task/report/backend/BE-FIN-101.md) |
| `FIX-FIN-001-02` | `ISS-FIN-001-03` | Tambahkan `useMemo` ke impor `react` pada `use-report-ar-created.jsx` | Frontend | 1 | `FE-FIN-042` | 🟡 SEBAGIAN — source selesai, lint PASS, `npm run build` **PASS**; verifikasi peramban manual belum dikonfirmasi (2/3 acceptance criteria terpenuhi) | [laporan FE-FIN-042](../../task/report/frontend/FE-FIN-042.md) |
| `FIX-FIN-001-03` | `ISS-FIN-001-T1` | Pindahkan `ResolvePayerNameAsync` agar dipanggil setelah `NpgsqlDataReader` ringkasan selesai dan tertutup | Backend | 2 | `BE-FIN-101` | 🟡 SEBAGIAN — source selesai (satu task gabungan dengan `FIX-FIN-001-01`), `dotnet build`/uji manual saringan penjamin belum dikonfirmasi | [laporan BE-FIN-101](../../task/report/backend/BE-FIN-101.md) |
| `FIX-FIN-001-04` | `ISS-FIN-001-T2` | Perbarui kontrak `FIN-LYR-AR-12` pada `03-frontend-architecture.md` agar menyebut endpoint V2 yang sebenarnya dipakai | Dokumentasi | 3 | — | ✅ SELESAI | [`03-frontend-architecture.md` §23](../../03-frontend-architecture.md) |
| `FIX-FIN-001-05` | Temuan lanjutan (instruksi pemilik 11 Oktober 2026, sesi kedua) | Hubungkan filter Status Billing, Status Integrasi Finance AR, dan Status Invoice AR (sudah didukung backend, belum pernah terhubung ke UI Laporan AR) | Frontend | 2 | `FE-FIN-043` | 🟡 SEBAGIAN — source selesai, lint PASS, `npm run build` **PASS**; verifikasi peramban manual belum dikonfirmasi | [laporan FE-FIN-043](../../task/report/frontend/FE-FIN-043.md) |
| `FIX-FIN-001-06` | Temuan lanjutan (instruksi pemilik 11 Oktober 2026, sesi kedua) | Laporan AR: galat API tidak lagi salah ditampilkan sebagai data kosong — tambahkan kartu error-state dan tombol Coba Lagi yang mempertahankan filter | Frontend | 2 | `FE-FIN-043` | 🟡 SEBAGIAN — source selesai, lint PASS, `npm run build` **PASS**; verifikasi peramban manual belum dikonfirmasi | [laporan FE-FIN-043](../../task/report/frontend/FE-FIN-043.md) |

**Ringkasan: 1 dari 6 perbaikan berstatus ✅ SELESAI (`FIX-FIN-001-04`, revisi dokumen murni). 5 berstatus 🟡 SEBAGIAN (source selesai, lint dan build frontend PASS; `dotnet build` backend dan seluruh verifikasi peramban manual masih menunggu).**

**Ringkasan: 0 dari 4 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-FIN-001-01 & ISS-FIN-001-02 — Nama Kolom SQL Salah pada Dua `ORDER BY`

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Ganti literal `"CreatedAt"` menjadi `"CreateDateTime"` pada kedua baris, tanpa mengubah struktur `LATERAL JOIN`, filter, atau logika lain di sekitarnya | Perbaikan minimal, tepat sasaran akar masalah, nol risiko terhadap logika perhitungan/agregasi yang sudah benar, konsisten dengan nama kolom asli di `IdentityModel` dan migration snapshot | Tidak ada |
| B | Tambah kolom `CreatedAt` baru lewat migration pada `FinBillingHandoffIntake`/`FinReceivableInvoiceBatch` agar query lama tetap valid | Query SQL tidak perlu disentuh | Melanggar batasan eksplisit pelapor ("jangan membuat migration untuk menambah `CreatedAt` tanpa alasan schema yang sah"); menambah kolom duplikat yang tidak pernah dipakai di tempat lain; menyesatkan pembaca source di masa depan |

**Solusi terpilih: Opsi A.**
1. `CreateDateTime` adalah satu-satunya kolom jejak pembuatan yang benar-benar ada pada kedua
   tabel ini (`IdentityModel`, dikonfirmasi pada migration snapshot) — mengganti nama kolom di
   query adalah perbaikan pada sisi yang salah, bukan pada sisi yang benar.
2. Radius dampak nol terhadap baris kode lain: kedua `ORDER BY ... LIMIT 1` ini hanya menentukan
   *baris mana* yang dipilih sebagai "terbaru" di dalam `LATERAL JOIN`-nya sendiri; mengganti nama
   kolom pembanding tidak mengubah kolom apa pun yang dibaca atau dikembalikan ke caller.

**Kenapa bukan opsi B.** Opsi B secara langsung melanggar batasan yang pelapor tetapkan sendiri
dan tidak memperbaiki akar masalah (kesalahan ada di query, bukan di skema).

**Pengerasan tambahan (bukan perbaikan bug, murni determinisme).** Brief asli menyarankan
"ordering deterministik, misalnya tanggal pembuatan diikuti ID jika diperlukan". Karena beberapa
baris pada tabel ini bisa memiliki `CreateDateTime` yang identik (misalnya hasil proses batch),
`FIX-FIN-001-01` menambahkan `, "Id" DESC` sebagai pembanding kedua pada kedua `ORDER BY`, supaya
baris mana yang dipilih saat terjadi dasi tidak bergantung pada urutan fisik tabel yang tidak
terjamin. Ini **tidak** mengubah hasil pada kasus normal (tanpa dasi waktu) dan **tidak**
mengubah kontrak respons apa pun.

---

### ISS-FIN-001-03 — `useMemo` Tidak Diimpor

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Tambahkan `useMemo` ke daftar impor `react` yang sudah ada di baris 8 | Perbaikan satu baris, tepat sasaran akar masalah, nol risiko | Tidak ada |
| B | Hapus pemakaian `useMemo` dan ganti `debtorIdsToResolve` menjadi dihitung ulang setiap render tanpa memoisasi | Tetap memperbaiki galat | Menghapus optimisasi yang sengaja ditulis (mencegah `useCompanyGuarantorNameResolver` memanggil ulang resolve setiap render); mengubah kode lebih dari yang diperlukan |

**Solusi terpilih: Opsi A.**
1. Satu-satunya yang salah adalah impornya hilang — logika `useMemo` itu sendiri benar dan
   konsisten dengan pola `useMemo` yang sudah dipakai di `report-created-view.jsx` dan
   `report-created-table.jsx` (keduanya mengimpor `useMemo` dengan benar).
2. Perbaikan satu baris ini cukup untuk menghilangkan `ReferenceError` yang memblokir seluruh
   halaman.

**Kenapa bukan opsi B.** Mengubah logika memoisasi adalah perubahan yang lebih besar dari yang
dibutuhkan untuk perbaikan bug ini, dan berisiko mengubah perilaku pemanggilan
`useCompanyGuarantorNameResolver`.

---

### ISS-FIN-001-T1 — `ResolvePayerNameAsync` Berebut Koneksi dengan Reader yang Masih Terbuka

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Simpan seluruh nilai kolom dari `reader` ke variabel lokal di dalam blok `if (await reader.ReadAsync(...))`, lalu panggil `ResolvePayerNameAsync` **setelah** blok `await using var reader` berakhir (reader sudah tertutup) | Urutan baca-lalu-tutup-lalu-resolve eksplisit dan mudah diverifikasi; tidak menambah round-trip database baru; tidak mengubah hasil perhitungan apa pun | Method `ExecuteSummaryQueryAsync` sedikit lebih panjang (satu level nesting lebih sedikit, bukan lebih banyak) |
| B | Buka koneksi/command kedua yang terpisah (`NpgsqlConnection` baru) khusus untuk `ResolvePayerNameAsync` | Tidak perlu merestrukturisasi method | Membuka koneksi database kedua hanya untuk satu lookup nama adalah pemborosan resource dan pola yang tidak konsisten dengan penggunaan `_dbContext` yang sudah ada di tempat lain pada service ini |
| C | Resolve nama penjamin lebih dulu, sebelum membuka reader ringkasan sama sekali | Menghindari masalah urutan sama sekali | Query nama penjamin akan tetap dijalankan meski ternyata `reader.ReadAsync()` nanti mengembalikan `false` (kasus langka, kolom `COUNT(*)` selalu mengembalikan satu baris bahkan untuk 0 hasil, sehingga kasus ini sebenarnya tidak pernah terjadi) — secara fungsional setara dengan Opsi A tetapi mengubah urutan baca yang sudah ada tanpa alasan tambahan |

**Solusi terpilih: Opsi A.**
1. Radius dampak terkecil: hanya menata ulang urutan baris di dalam satu method, tanpa menambah
   resource baru (opsi B) atau memindahkan logika ke urutan yang tidak perlu (opsi C).
2. Mengikuti pola yang sudah diminta pelapor sendiri di brief awal: "Eksekusi summary → Baca dan
   simpan hasil → Dispose DataReader dan command → Resolve nama penjamin → Bentuk response DTO."

**Kenapa bukan opsi lain.** Opsi B boros resource tanpa manfaat tambahan. Opsi C secara
fungsional setara tetapi tidak mengikuti urutan yang sudah diminta pelapor dan sedikit mengubah
kapan query penjamin dieksekusi relatif terhadap query ringkasan tanpa alasan yang lebih kuat
dari Opsi A.

---

### ISS-FIN-001-T2 — Dokumen Desain Tertinggal dari Source

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Perbarui baris kontrak `FIN-LYR-AR-12` pada `03-frontend-architecture.md` agar menyebut endpoint V2 (`GET .../receivable-invoice-batches/reports/created` dan turunannya) | Dokumen kembali sinkron dengan source; menindaklanjuti peringatan yang sudah ditulis sendiri di `FE-FIN-017.md:288` | Tidak ada |
| B | Biarkan dokumen seperti sekarang, cukup catat gap-nya di issue ini | Tidak menyentuh dokumen blueprint | Risiko yang sudah diperingatkan `FE-FIN-017.md` ("menyesatkan task `FE-FIN-019`+") tetap terbuka |

**Solusi terpilih: Opsi A.** Revisi dokumen murni tanpa menyentuh source atau perilaku aplikasi,
dan sudah diminta sendiri oleh laporan task sebelumnya.

**Kenapa bukan opsi B.** Membiarkan gap yang sudah teridentifikasi tanpa menutupnya hanya
menunda masalah yang pasti berulang ke task frontend berikutnya yang membaca tabel kontrak ini.

---

## 3. Skema Tampilan Sebelum → Sesudah

Keempat perbaikan **tidak mengubah tata letak, komponen, atau elemen visual apa pun** pada
kedua layar — seluruhnya adalah perbaikan di balik layar (nama kolom SQL, satu baris impor
JavaScript, urutan eksekusi backend, dan teks dokumen). Tata letak Laporan AR dan Report AR
Created setelah perbaikan identik dengan rancangan yang sudah ada; perbedaannya murni pada
**apakah layar berhasil dimuat sama sekali**.

### Laporan AR — Sebelum (Sesuai Laporan Pengguna)

```text
+-----------------------------------------------------------------+
|  Finance > Transaksi A/R > Laporan & Monitoring > Laporan AR    |
+-----------------------------------------------------------------+
|                                                                 |
|   [!] Terjadi kesalahan pada server.                           |
|                                                                 |
|       (Tidak ada data, tidak ada ringkasan, tidak ada tabel —   |
|        seluruh permintaan GET /reports/billing-ar gagal 500)    |
|                                                                 |
+-----------------------------------------------------------------+
```

### Laporan AR — Sesudah (Rencana Perbaikan)

```text
+-----------------------------------------------------------------+
|  Finance > Transaksi A/R > Laporan & Monitoring > Laporan AR    |
+-----------------------------------------------------------------+
|  Filter: [Penjamin ▾] [Jenis Kunjungan ▾] [Periode ▾] [Cari]    |
+-----------------------------------------------------------------+
|  Ringkasan: 128 Invoice (97 Pasien) | Billing On Process: 40     |
|  Handoff Berhasil: 88 | Invoice AR Terbit: 65                   |
|  Estimasi: Rp 120.000.000 | Final: Rp 340.000.000               |
+-----------------------------------------------------------------+
|  PT ASURANSI CONTOH                                              |
|  No | Tgl | No.Reg | No.Bill | Pasien | ... | Status Inv AR     |
|  1  | ... | ...    | ...     | ...    | ... | Sudah Dibuat      |
|  Total PT ASURANSI CONTOH: Rp 45.000.000                         |
+-----------------------------------------------------------------+
|  [Ekspor Excel]  [Ekspor PDF]           Halaman 1 / 5            |
+-----------------------------------------------------------------+
```

### Report AR Created — Sebelum (Sesuai Laporan Pengguna)

```text
+-----------------------------------------------------------------+
|  This page couldn't load                                       |
|                                                                 |
|  (Galat JavaScript pada render pertama — tidak ada breadcrumb, |
|   tidak ada filter, tidak ada tabel yang sempat tampil)         |
+-----------------------------------------------------------------+
```

### Report AR Created — Sesudah (Rencana Perbaikan)

```text
+-----------------------------------------------------------------+
|  Finance > Laporan & Monitoring > Laporan Pembuatan AR           |
+-----------------------------------------------------------------+
| Filter Data (kiri)            | Ringkasan Laporan (kanan)        |
| - Kata Kunci                  | Penjamin Terpilih: Semua         |
| - Penjamin/Perusahaan         | Total Tagihan Terfilter: Rp ...  |
| - Jenis Kunjungan / Dasar Tgl | Jumlah Invoice AR: 65 Dokumen     |
| - Periode / Rentang Tanggal   | Ringkasan per Jenis Kunjungan     |
| [Terapkan Filter] [Reset]     |                                   |
| [Cetak] [Excel] [PDF]         |                                   |
+-----------------------------------------------------------------+
|  Tabel 9 kolom, dikelompokkan per Penjamin, subtotal per tanggal |
+-----------------------------------------------------------------+
```

### Tabel Wilayah (Report AR Created, tidak berubah dari rancangan existing)

| Wilayah | Isi | Sumber Data | Komponen |
| :--- | :--- | :--- | :--- |
| Filter Data | Kata kunci, penjamin, jenis kunjungan, dasar tanggal, periode | `filters` (state hook) | `ResourceFilterSelect`, `select`, `input` |
| Ringkasan Laporan | Nama penjamin terpilih, total tagihan, jumlah invoice AR, ringkasan jenis kunjungan | `summary` (dari `GET .../reports/created/summary`) | kartu ringkasan (`canceledStyles.summary*`) |
| Tabel Detail | Baris detail tagihan per Invoice AR, dikelompokkan per penjamin, subtotal per tanggal | `items`/`groups` (dari `GET .../reports/created`) | `ReportCreatedTable` |

### Tabel Keadaan (tidak berubah dari rancangan existing — dipulihkan, bukan diciptakan)

| Keadaan | Penanganan pada Tampilan |
| :--- | :--- |
| Memuat | `loading`/`summaryLoading` true, label berganti menjadi "..." pada kartu ringkasan |
| Gagal (API) | `errorMessage` terisi, `InformationAlert variant="danger"` tampil, toast galat muncul |
| Kosong | `totalData === 0`, tombol ekspor nonaktif, tabel menampilkan keadaan kosong bawaan `ReportCreatedTable` |
| Berisi | Tabel dan ringkasan terisi sesuai respons |
| Galat render (sebelum perbaikan) | Tidak tertangani sama sekali — inilah `ISS-FIN-001-03` yang diperbaiki `FIX-FIN-001-02` |

---

## 4. Rincian Perbaikan

### FIX-FIN-001-01 — Perbaiki Dua Nama Kolom SQL Salah pada `BuildFromWhereClause`

| | |
| :--- | :--- |
| **Menutup** | `ISS-FIN-001-01`, `ISS-FIN-001-02` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (raw SQL string) |
| **Berkas yang diubah** | `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs` (baris 581 dan 613, di dalam method `BuildFromWhereClause`) |
| **Radius dampak** | Seluruh permukaan Laporan AR yang memanggil `BuildFromWhereClause`: `GetReportAsync` (daftar + ringkasan halaman), `ExportExcelAsync`, `ExportPdfAsync`. **Tidak** berdampak ke `GetPayerOptionsAsync` (LINQ EF Core murni, tidak memakai `BuildFromWhereClause`). **Tidak** berdampak ke modul lain — kedua tabel ini juga dipakai service lain (`FinanceReceivableInvoiceBatchService`), tetapi service tersebut memakai LINQ (`BuildReportArCreatedBaseQuery`), bukan SQL mentah, sehingga tidak membawa typo yang sama. |
| **Bergantung pada** | Tidak ada |

**Langkah rinci:**
1. Baris 581, ganti:
   ```sql
   ORDER BY intake2."CreatedAt" DESC
   ```
   menjadi:
   ```sql
   ORDER BY intake2."CreateDateTime" DESC, intake2."Id" DESC
   ```
2. Baris 613, ganti:
   ```sql
   ORDER BY bi2."IsActiveMembership" DESC, batch2."CreatedAt" DESC
   ```
   menjadi:
   ```sql
   ORDER BY bi2."IsActiveMembership" DESC, batch2."CreateDateTime" DESC, batch2."Id" DESC
   ```
3. Tidak ada perubahan lain pada method ini — seluruh `JOIN`, `WHERE`, filter, dan kolom `SELECT`
   di method lain (`ExecuteSummaryQueryAsync`, `ExecutePagedItemsQueryAsync`) tetap seperti
   sekarang karena tidak mengandung nama kolom yang salah.

**Acceptance criteria:**
1. `GET /api/v1/corporate/finance-management/receivables/reports/billing-ar` (tanpa saringan apa
   pun) mengembalikan HTTP 200 dengan `data.items`, `data.groups`, dan `data.summary` terisi
   sesuai data yang ada — bukan HTTP 500.
2. Query tidak lagi melempar `PostgresException 42703` untuk kolom `CreatedAt` pada tabel
   `FinBillingHandoffIntake` maupun `FinReceivableInvoiceBatch`.
3. Nilai `summary.InvoiceCount`, `summary.TotalAmount`, dan status per baris (`FinanceHandoffStatus`, `ArInvoiceStatus`) **identik** dengan yang akan dihasilkan seandainya kolom sejak awal bernama benar — perbaikan ini tidak mengubah logika penghitungan apa pun, hanya nama kolom pembanding `ORDER BY ... LIMIT 1`.
4. `GET .../reports/billing-ar/export/excel` dan `.../export/pdf` berhasil mengembalikan berkas (bukan HTTP 500) untuk saringan yang sama.

**Verifikasi.** Tidak dijalankan otomatis oleh skill ini (bukan kewenangannya) dan **tidak**
dijalankan `dotnet build`/`dotnet run` secara otomatis oleh agent mana pun tanpa instruksi
eksplisit terpisah dari pemilik — sesuai permintaan pemilik saat sesi ini berjalan. Saat perbaikan
dieksekusi lewat `build-module-backend`, verifikasi yang diharapkan: `dotnet build` (hanya setelah
pemilik mengonfirmasi secara eksplisit), lalu uji manual/`curl` keempat endpoint pada bagian 6
laporan asli dengan kombinasi saringan pada bagian 14 laporan asli (tanpa saringan, per penjamin,
per jenis kunjungan, per periode, per status Billing/handoff/Invoice AR).

**Risiko.** Sangat rendah — mengganti nama kolom literal di dalam string SQL tidak dapat
memengaruhi baris kode C# lain. Risiko utama adalah salah ketik ulang; mitigasinya adalah
mencocokkan persis dengan nama kolom pada migration snapshot (`"CreateDateTime"`, huruf besar di
awal setiap kata, tanpa spasi).

---

### FIX-FIN-001-02 — Tambahkan Impor `useMemo` yang Hilang

| | |
| :--- | :--- |
| **Menutup** | `ISS-FIN-001-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (satu baris impor) |
| **Berkas yang diubah** | `src/lib/hooks/finance/receivable/use-report-ar-created.jsx` (baris 8) |
| **Radius dampak** | Report AR Created (`/finance/ar-report/created`) — satu-satunya pemakai `useReportArCreated`. Dipakai juga secara tidak langsung lewat `use-created-receivable-list.jsx` (wrapper kompatibilitas) — keduanya akan ikut berfungsi normal begitu impor ini diperbaiki, karena keduanya memanggil fungsi yang sama. |
| **Bergantung pada** | Tidak ada |

**Langkah rinci:**
1. Baris 8, ganti:
   ```jsx
   import { useCallback, useEffect, useState } from "react";
   ```
   menjadi:
   ```jsx
   import { useCallback, useEffect, useMemo, useState } from "react";
   ```
2. Tidak ada baris lain yang perlu diubah — fungsi `useMemo` pada baris 126 sudah ditulis benar
   dan tidak perlu disentuh.

**Acceptance criteria:**
1. Membuka `/finance/ar-report/created` tidak lagi menampilkan "This page couldn't load";
   halaman merender breadcrumb, panel Filter Data, panel Ringkasan Laporan, dan tabel (kosong
   atau berisi sesuai data).
2. Tidak ada `ReferenceError` atau galat React lain di console browser saat halaman dimuat
   pertama kali, saat menerapkan filter, maupun saat berganti halaman paginasi.
3. `debtorIdsToResolve` tetap hanya dihitung ulang ketika `appliedFilters.debtorReferenceId` atau
   `items` berubah (perilaku memoisasi yang sudah ditulis, tidak berubah oleh perbaikan ini).

**Verifikasi.** Tidak dijalankan otomatis oleh skill ini. Saat dieksekusi lewat
`build-module-frontend`: `npm run lint:errors` (harus PASS — aturan ini seharusnya menangkap
`no-undef` untuk kasus seperti ini), lalu `npm run build` **hanya setelah pemilik mengonfirmasi
secara eksplisit** bahwa build boleh dijalankan, lalu uji manual membuka halaman di browser.

**Risiko.** Sangat rendah — menambah satu nama ke daftar impor yang sudah ada tidak dapat
memengaruhi perilaku lain pada berkas ini.

---

### FIX-FIN-001-03 — Tunda `ResolvePayerNameAsync` Sampai Reader Ringkasan Tertutup

| | |
| :--- | :--- |
| **Menutup** | `ISS-FIN-001-T1` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (restrukturisasi urutan baris, bukan logika) |
| **Berkas yang diubah** | `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBillingArReportService.cs` (method `ExecuteSummaryQueryAsync`, baris 734-823) |
| **Radius dampak** | Hanya memengaruhi jalur `GetReportAsync`, `ExportExcelAsync`, `ExportPdfAsync` ketika `query.PayerId` diisi dengan nilai spesifik (bukan "Semua"). Tidak berdampak pada jalur tanpa saringan penjamin. |
| **Bergantung pada** | `FIX-FIN-001-01` (baris ini baru benar-benar tereksekusi untuk pertama kalinya setelah query tidak lagi gagal di `BuildFromWhereClause`) |

**Langkah rinci:**
1. Di dalam blok `if (await reader.ReadAsync(cancellationToken))`, simpan **seluruh** 12 nilai
   kolom (`invCount` s/d `totAmount`) ke variabel lokal seperti sekarang — bagian ini sudah benar
   dan tidak berubah.
2. **Hapus** pemanggilan `ResolvePayerNameAsync` dari dalam blok `if` tersebut. Simpan hasil
   `query.PayerId.HasValue && query.PayerId.Value != Guid.Empty` sebagai flag boolean lokal,
   bersama nilai `query.PayerId.Value` bila ada, tanpa memanggil method apa pun yang menyentuh
   `_dbContext` selagi masih di dalam cakupan `await using var reader`/`await using var cmd`.
3. Pindahkan konstruksi `return new BillingArReportSummaryResponse { ... }` ke **luar** blok
   `try`/`finally` yang membuka `reader` dan `cmd` — yaitu setelah `await conn.CloseAsync()` pada
   baris `finally` selesai (atau, lebih sederhana, setelah seluruh blok `try { ... } finally { ... }`
   method ini berakhir), sehingga `reader` dan `cmd` sudah pasti di-dispose sebelum
   `ResolvePayerNameAsync` dipanggil.
4. Panggil `ResolvePayerNameAsync` di titik itu — setelah reader tertutup — menggunakan flag dan
   nilai `PayerId` yang disimpan di langkah 2, lalu bentuk dan kembalikan
   `BillingArReportSummaryResponse` menggunakan seluruh nilai yang sudah disimpan di langkah 1.
5. Jalur "tidak ada baris" (`else` dari `if (await reader.ReadAsync(...))`, baris 813-817) tetap
   mengembalikan `BillingArReportSummaryResponse` kosong seperti sekarang — tidak berubah, karena
   jalur ini tidak pernah memanggil `ResolvePayerNameAsync`.

**Acceptance criteria:**
1. Menyaring Laporan AR dengan satu Penjamin tertentu (`payerId` terisi) mengembalikan HTTP 200
   dengan `summary.PayerName` berisi nama penjamin yang benar — bukan galat
   `NpgsqlOperationInProgressException` atau sejenisnya.
2. Menyaring tanpa Penjamin tertentu tetap mengembalikan `summary.PayerName = "Semua Perusahaan / Asuransi"` seperti sekarang — tidak berubah.
3. Seluruh 12 nilai ringkasan lain (`InvoiceCount`, `PatientCount`, `TotalAmount`, dst.) nilainya
   **identik** dengan sebelum perbaikan untuk saringan yang sama — perbaikan ini murni menata
   ulang *kapan* `ResolvePayerNameAsync` dipanggil, tidak mengubah nilai apa pun yang dihitung.

**Verifikasi.** Tidak dijalankan otomatis oleh skill ini. Saat dieksekusi lewat
`build-module-backend`: `dotnet build` hanya setelah konfirmasi eksplisit pemilik, lalu uji manual
`GET .../reports/billing-ar?payerId=<guid-penjamin-valid>` dibandingkan dengan hasil tanpa
`payerId` untuk memastikan hanya `PayerName` yang berbeda.

**Risiko.** Rendah. Risiko utama adalah salah memindahkan variabel sehingga nilai yang dibaca dari
`reader` tidak lengkap tersimpan sebelum `reader` didispose — mitigasinya adalah memastikan
langkah 1 (simpan semua 12 nilai) terjadi **sebelum** langkah 2 dieksekusi, persis seperti urutan
yang sudah ada sekarang, hanya memindahkan pemanggilan `ResolvePayerNameAsync` ke titik yang lebih
belakang.

---

### FIX-FIN-001-04 — Perbarui Kontrak `FIN-LYR-AR-12` pada Dokumen Arsitektur Frontend

| | |
| :--- | :--- |
| **Menutup** | `ISS-FIN-001-T2` |
| **Area** | Dokumentasi |
| **Jenis perubahan** | Revisi dokumen blueprint (bukan source) |
| **Berkas yang diubah** | `docs/module-blueprints/finance-management/03-frontend-architecture.md` (baris 741, dan baris 634 bila perlu disesuaikan agar konsisten) |
| **Radius dampak** | Dokumentasi saja. Tidak menyentuh source backend maupun frontend. |
| **Bergantung pada** | Tidak ada — dapat dikerjakan kapan saja, tidak tergantung tiga `FIX` lain |

**Langkah rinci:**
1. Baris 741, ganti:
   ```text
   | `FIN-LYR-AR-12` Report AR Created | `GET /receivables` | diurutkan tanggal terbit |
   ```
   menjadi referensi endpoint V2 yang sebenarnya dipakai saat ini:
   ```text
   | `FIN-LYR-AR-12` Report AR Created | `GET /receivable-invoice-batches/reports/created`, `.../reports/created/summary`, `.../reports/created/export/{excel,pdf}` | detail per baris tagihan (`FinReceivableInvoiceBatchItem`), dikelompokkan per penjamin dengan subtotal tanggal |
   ```
2. Tambahkan satu baris di Riwayat/catatan dokumen (sesuai format yang sudah dipakai dokumen ini)
   yang menyebutkan bahwa kontrak ini direvisi menyusul `ISSUE-FIN-001`/`PLAN-REPAIR-FIN-001`,
   menggantikan kontrak V1 (`GET /receivables`) yang didokumentasikan sebelumnya di `FE-FIN-017`.

**Acceptance criteria:**
1. `03-frontend-architecture.md` tidak lagi menyebut `GET /receivables` untuk `FIN-LYR-AR-12`.
2. Endpoint yang disebut pada dokumen cocok persis dengan konstanta
   `REPORT_AR_CREATED_ENDPOINT`/`REPORT_AR_CREATED_SUMMARY_ENDPOINT`/`reportArCreatedExportEndpoint`
   (`src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx:200-207`) dan
   route backend `FinanceReceivableInvoiceBatchesController.cs:162-192`.

**Verifikasi.** Tinjauan manual — bandingkan teks dokumen baru dengan dua berkas source yang
dirujuk di atas, baris demi baris.

**Risiko.** Tidak ada risiko ke aplikasi — ini perubahan teks dokumentasi.

---

## 5. Urutan Pengerjaan

```text
FIX-FIN-001-01   tanpa dependency — boleh dikerjakan sekarang (backend)
└── FIX-FIN-001-03   baru benar-benar teruji setelah FIX-01 membuat query berhasil berjalan
FIX-FIN-001-02   tanpa dependency — boleh dikerjakan sekarang (frontend), independen dari ketiga lainnya
FIX-FIN-001-04   tanpa dependency — boleh dikerjakan kapan saja (dokumentasi)
```

`FIX-FIN-001-01` dan `FIX-FIN-001-03` sebaiknya dikerjakan dalam satu task backend yang sama
(satu berkas, dua method yang saling berdekatan) agar verifikasi "filter penjamin tertentu tidak
menimbulkan error tambahan" dapat dilakukan sekali untuk keduanya. `FIX-FIN-001-02` adalah task
frontend yang berdiri sendiri. `FIX-FIN-001-04` dapat digabung ke task mana pun atau dikerjakan
terpisah sebagai task dokumentasi kecil.

---

## 6. Keputusan Pemilik yang Dibutuhkan

Disalin dari issue bagian 7:

| No | Keputusan | Status | Dampak bila ditolak |
| :---: | :--- | :---: | :--- |
| K-01 | Mengesahkan `FIX-FIN-001-01`, `FIX-FIN-001-02`, `FIX-FIN-001-03` untuk dieksekusi lewat `build-module-backend`/`build-module-frontend` | **Menunggu persetujuan tertulis pemilik** | Laporan AR dan Report AR Created tetap tidak dapat dipakai |
| K-02 | Mengesahkan `FIX-FIN-001-04` (revisi dokumen) | **Menunggu persetujuan tertulis pemilik** | Dokumen desain tetap menyesatkan pembaca berikutnya |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | §17, baris 741 (kontrak `FIN-LYR-AR-12`) | `GET /receivables`, diurutkan tanggal terbit | `GET /receivable-invoice-batches/reports/created` beserta `.../summary` dan `.../export/{excel,pdf}`, detail per baris tagihan dikelompokkan per penjamin |

Revisi dokumen ini dikerjakan sebagai `FIX-FIN-001-04` (bagian 4 di atas) — bukan oleh
`diagnose-module-issue` secara langsung, sesuai batas kewenangan skill ini.

---

## 8. Verifikasi Menyeluruh

Setelah seluruh `FIX` disetujui dan dikerjakan oleh `build-module-backend`/`build-module-frontend`,
urutan verifikasi ujung-ke-ujung yang direkomendasikan (dijalankan oleh builder/pemilik, **bukan**
otomatis oleh agent manapun tanpa konfirmasi eksplisit untuk setiap langkah build/run):

1. **Build/lint** — `dotnet build` (backend) dan `npm run lint:errors` + `npm run build`
   (frontend), masing-masing hanya setelah pemilik mengonfirmasi secara eksplisit bahwa perintah
   tersebut boleh dijalankan pada sesi tersebut.
2. **Laporan AR — tanpa saringan.** `GET .../reports/billing-ar` mengembalikan HTTP 200, data
   terisi sesuai isi database, tidak ada `PostgresException`.
3. **Laporan AR — per Penjamin.** `GET .../reports/billing-ar?payerId=<guid>` mengembalikan HTTP
   200 dengan `summary.PayerName` berisi nama penjamin yang benar (membuktikan `FIX-FIN-001-03`
   menutup `ISS-FIN-001-T1`).
4. **Laporan AR — kombinasi saringan lain.** Jenis kunjungan, periode, status Billing, status
   handoff Finance AR, status Invoice AR — masing-masing dicoba satu per satu, dibandingkan dengan
   ekspektasi berdasarkan data yang diketahui di database uji.
5. **Laporan AR — paginasi dan ekspor.** Halaman pertama dan halaman berikutnya pada daftar;
   `GET .../export/excel` dan `.../export/pdf` berhasil mengembalikan berkas yang dapat dibuka.
6. **Report AR Created — buka langsung.** Navigasi ke `/finance/ar-report/created` dari sidebar
   dan lewat refresh browser langsung di URL tersebut — keduanya berhasil tampil tanpa "This page
   couldn't load".
7. **Report AR Created — filter dan paginasi.** Terapkan Filter, Reset, ganti halaman — tidak ada
   galat console, tidak ada permintaan API yang berulang tanpa henti.
8. **Report AR Created — ekspor.** Cetak, Excel, PDF masing-masing berhasil.
9. **Regresi.** Pastikan layar Finance lain yang memakai `FinanceBillingArReportService` atau
   `_dbContext` yang sama (tidak ada yang ditemukan selama audit ini di luar layar ini sendiri)
   tidak terdampak; dan pastikan `use-created-receivable-list.jsx` (wrapper kompatibilitas) masih
   berfungsi untuk pemanggil lain yang mungkin masih memakainya.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-11 | Rencana dibuat, 4 perbaikan (2 Blocker, 1 High laten, 1 Low dokumentasi), menunggu persetujuan pemilik. Pemilik secara eksplisit meminta agar `dotnet build` tidak dijalankan otomatis oleh agent — dicatat sebagai batasan verifikasi pada bagian 4 dan 8 dokumen ini. | `diagnose-module-issue` (Claude) |
| 2026-10-11 | Pemilik menyetujui K-01 lewat instruksi chat langsung dan memerintahkan eksekusi via `build-module-backend`/`build-module-frontend`. `FIX-FIN-001-01` dan `FIX-FIN-001-03` dikerjakan sebagai `BE-FIN-101` (satu task gabungan); `FIX-FIN-001-02` dikerjakan sebagai `FE-FIN-042`. Ketiganya 🟡 SEBAGIAN — source selesai, `dotnet build`/`npm run build`/verifikasi peramban belum dikonfirmasi pemilik. `FIX-FIN-001-04` (K-02) belum diminta dieksekusi, tetap BELUM DIKERJAKAN. Status rencana: `DISETUJUI_SEBAGIAN`. | `build-module-backend` + `build-module-frontend` (Claude) |
| 2026-10-11 | Instruksi lanjutan pemilik: audit ulang filter backend, summary/pagination, dan frontend Laporan AR, lalu eksekusi langsung. Audit backend (`BuildFromWhereClause`, `ExecuteSummaryQueryAsync`, `ExecutePagedItemsQueryAsync`, export) **tidak menemukan cacat baru** — default `billingStatus="ALL"`, nol double-count, pagination server-side, export seluruh hasil filter sudah benar sejak awal (dicatat di `BE-FIN-101`). Dua temuan nyata di frontend ditambahkan sebagai `FIX-FIN-001-05` (filter Status Billing/Integrasi Finance AR/Invoice AR tidak pernah terhubung ke UI) dan `FIX-FIN-001-06` (galat API salah tampil sebagai data kosong, nol tombol retry) — keduanya dikerjakan sebagai `FE-FIN-043`. Pemilik juga mengonfirmasi `npm run build` boleh dijalankan untuk frontend (terpisah dari batasan `dotnet build` backend yang tetap berlaku); hasilnya PASS untuk `FE-FIN-042` dan `FE-FIN-043`. | `build-module-backend` + `build-module-frontend` (Claude) |
