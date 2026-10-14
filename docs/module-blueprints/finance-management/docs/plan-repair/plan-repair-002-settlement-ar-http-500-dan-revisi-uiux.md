# PLAN-REPAIR-002 — Settlement AR: perbaikan HTTP 500, duplikasi data, dan revisi UI/UX

```yaml
plan_id: PLAN-REPAIR-FIN-002
issue: ../issue/issue-002-settlement-ar-http-500-dan-revisi-uiux.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-10-11"
diputuskan_oleh: "pemilik modul (chat 2026-10-11: \"Kerjakan semua lakukan perbaikan melalui build-module-backend dan build-module-frontend\")"
tanggal_keputusan: "2026-10-11"
basis_source_backend: "845dabf9 (Yasmina)"
basis_source_frontend: "46a1549dc (yasmina)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-FIN-002-01` | `ISS-FIN-002-01` | Perbaiki akar HTTP 500 — translasi EF gagal pada `debtorResolvedName` di `ApplySorting`/filter `Search` | BE | 1 | `BE-FIN-102` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/backend/BE-FIN-102.md` |
| `FIX-FIN-002-02` | `ISS-FIN-002-02` | Tambahkan `!IsDelete` pada `batchSubquery` agar tidak menduplikasi baris | BE | 1 | `BE-FIN-102` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/backend/BE-FIN-102.md` |
| `FIX-FIN-002-03` | `ISS-FIN-002-03` | ~~Samakan id/class kontainer scroll~~ — **tidak diperlukan**, lihat koreksi di bawah | FE | 2 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-04` | `ISS-FIN-002-04`, `ISS-FIN-002-05` | Revisi filter 2 kolom + 3 information card + label saldo | FE | 2 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-05` | `ISS-FIN-002-04` | Revisi tabel 7 kolom proporsional, tanpa scroll horizontal desktop | FE | 2 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-06` | `ISS-FIN-002-04` | Kondisi error list/summary (state gagal + tombol coba lagi, tanpa menghapus filter) | FE | 2 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-07` | `ISS-FIN-002-04` | Navigasi Detail Settlement AR mempertahankan filter/halaman/sorting saat kembali | FE | 3 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-08` | `ISS-FIN-002-05` | Label dinamis "Nilai Awal Piutang" vs periode pada card Beginning Balance | FE | 3 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-09` | `ISS-FIN-002-06` | Revisi dokumen blueprint agar mencerminkan `FinanceSettlementService`/halaman dedicated sebagai desain resmi | Dokumentasi | 1 | — | ✅ SELESAI | `FIN-DEC-203` (`00-interview-decisions.md`), `03-frontend-architecture.md` §17.2, `02-backend-architecture.md` bagian Q, `contracts/api-contract.md` bagian Q.1 |
| `FIX-FIN-002-10` | `ISS-FIN-002-07` | Audit menu/sidebar untuk menentukan route aktif di antara dua route "Settlement AR"; hapus/redirect yang tidak dipakai | FE | 3 | `FE-FIN-044` | ✅ SELESAI (bagian ini) — route `/finance/settlement` dihapus, dikonfirmasi tanpa rujukan | `../../task/report/frontend/FE-FIN-044.md` |
| `FIX-FIN-002-11` | `ISS-FIN-002-08` | ~~Tambahkan opsi urut `totalPiutang`~~ — **premis keliru**, lihat koreksi di bawah | FE (+ BE bila K-02 = Ya) | 3 | `FE-FIN-044` | ✅ SELESAI (Diverifikasi penuh oleh pengguna) | `../../task/report/frontend/FE-FIN-044.md` |

**Ringkasan: 7 dari 11 perbaikan berstatus Sebagian (source selesai, `npm run build`/uji
peramban menunggu konfirmasi pemilik), 1 Selesai secara source (`-10`), 1 belum dikerjakan
(`-09`, dokumentasi).** K-01 diperlakukan sebagai disetujui Opsi (a) atas instruksi langsung
pemilik 2026-10-11 ("kerjakan semua"); P-02 terjawab dari log aplikasi asli (lihat
`ISS-FIN-002-01`). `FIX-FIN-002-01`+`-02` digabung satu task backend (`BE-FIN-102`, berkas
sama); `FIX-FIN-002-03` s/d `-08`, `-10`, `-11` digabung satu task frontend (`FE-FIN-044`, satu
layar yang sama; task ID berubah dari `FE-FIN-043` karena sesi lain memakai nomor itu untuk
issue berbeda hampir bersamaan). `FIX-FIN-002-09` dikerjakan langsung sebagai revisi dokumen
blueprint, bukan lewat task roadmap BE/FE.

**Koreksi atas dua dugaan awal** (ditemukan saat implementasi `FE-FIN-044`, lihat laporan
bagian 1):

- `FIX-FIN-002-03` (footer): dugaan "kontainer halaman tidak match `#content-page`" **tidak
  terbukti** — `src/components/ui/content-page.jsx` membungkus SEMUA halaman normal (termasuk
  Settlement AR) dengan `id="content-page"` di level shell aplikasi (`root-layout-shell.jsx`).
  Mekanisme *safe space* footer sudah bekerja benar tanpa perubahan apa pun.
- `FIX-FIN-002-11` (opsi sort): dugaan "opsi kurang lengkap" **keliru** — `SETTLEMENT_SORT_OPTIONS`
  terbukti kode mati, tidak dipakai kontrol UI mana pun. Tidak ada opsi yang "hilang" karena
  tidak ada dropdown sort yang pernah dirender. Membangun kontrol sort baru tidak diminta
  eksplisit pada laporan manapun — tidak dikerjakan, dicatat untuk keputusan pemilik terpisah.

---

## 2. Solusi terpilih per temuan

### ISS-FIN-002-01 — HTTP 500 saat memuat daftar Settlement AR

Root cause sudah terbukti dari `Logs/quilvian-backend-20261011.json`:
`InvalidOperationException` karena `ApplySorting` dan filter `Search` merujuk ulang
`x.debtorResolvedName` — anggota hasil proyeksi `SettlementQueryRow` yang dihitung dari ternary
bersarang `comp→ins→emp→pat` — pada operator `OrderBy`/`Where` yang dipasang setelah `Select`
pada rantai 6 `LEFT JOIN`. EF Core tidak dapat menerjemahkan ulang referensi semacam ini.

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan `comp`/`ins` sebagai anggota `SettlementQueryRow`, lalu hitung ulang ternary yang sama langsung dari `x.pat`/`x.emp`/`x.comp`/`x.ins` di setiap titik pakai sebelum materialisasi (`ApplySorting`, filter `Search`) — bukan merujuk ke `debtorResolvedName` yang sudah diproyeksikan | Tidak memakai `.AsEnumerable()`/`.ToList()` sebagai workaround (dilarang eksplisit pemilik); mempertahankan filtering/sorting/pagination di database; perubahan terlokalisasi pada satu file | Menambah dua anggota pada record proyeksi; `debtorResolvedName` tetap dipertahankan untuk pemakaian setelah materialisasi (mapping baris di `GetSettlementPagedAsync:90-119`) agar tidak mengubah kontrak respons |
| B | Pindahkan seluruh query ke client evaluation dengan `.AsEnumerable()` sebelum sorting/filtering | Perbaikan sangat cepat | Dilarang eksplisit oleh pemilik ("Jangan menggunakan `AsEnumerable()` atau `ToList()` sebelum filtering/pagination sebagai workaround"); seluruh tabel `FinReceivable` yang cocok kategori akan ditarik ke memori sebelum difilter — berbahaya untuk data produksi |
| C | Hapus kemampuan urut berdasarkan nama debitur gabungan; urutkan hanya berdasarkan `x.pat.FullName` (abaikan debitur non-pasien) | Perubahan minimal | Mematahkan urutan untuk kategori Perusahaan/Karyawan (yang justru paling sering memakai `debtorResolvedName` karena `x.pat` selalu null) — tidak memenuhi kebutuhan laporan |

**Solusi terpilih: Opsi A.**

1. Memenuhi larangan eksplisit pemilik terhadap `.AsEnumerable()`/`.ToList()` sebagai workaround
   filtering/pagination (Bagian A §4 pada laporan awal).
2. Radius dampak terkecil — hanya menambah dua anggota record dan menyesuaikan dua titik pakai
   (`ApplySorting`, filter `Search`) pada file yang sama; tidak mengubah kontrak
   `FinanceSettlementRowResponse`/`FinanceSettlementSummaryResponse` maupun skema database.
3. Tetap mempertahankan `debtorResolvedName` apa adanya untuk dipakai di `GetSettlementPagedAsync`
   setelah `ToListAsync` (baris 90-119) — titik itu sudah aman karena berjalan di memori, bukan di
   dalam `IQueryable`.

**Kenapa bukan opsi lain.** Opsi B eksplisit dilarang pemilik dan berisiko kinerja/beban database
untuk tabel besar. Opsi C mengorbankan kebenaran fungsional (urutan nama debitur) demi kemudahan.

---

### ISS-FIN-002-02 — Duplikasi baris akibat `batchSubquery`

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan `!b.IsDelete` ke predikat `batchSubquery` | Perubahan satu baris, konsisten dengan pola soft-delete yang sudah dipakai di seluruh service ini (`!r.IsDelete`, `!m.IsDelete`) | Tidak ada — ini murni melengkapi filter yang sudah ada di tempat lain pada file yang sama |
| B | Perbaiki lewat migration: paksa `IsActiveMembership = false` setiap kali baris di-soft-delete | Menutup akar masalah pada sumbernya (data), bukan hanya titik baca ini | Menyentuh service penulisan (`FinanceReceivableInvoiceBatchService`) yang di luar cakupan read-only Settlement AR; berisiko melanggar batasan "jangan mengubah data pembayaran/ledger tanpa kebutuhan terbukti" pada laporan |

**Solusi terpilih: Opsi A.**

1. Radius dampak terkecil — satu baris predikat pada satu query baca.
2. Sejalan dengan pola yang sudah konsisten dipakai di seluruh `FinanceSettlementService`
   (`!r.IsDelete` baris 702, `!m.IsDelete` baris 72 dan 878) — hanya `batchSubquery` yang
   terlewat.
3. Tidak menyentuh data maupun lifecycle batch — murni query baca, sesuai batasan "Settlement AR
   adalah laporan read-only" pada laporan.

**Kenapa bukan opsi lain.** Opsi B mengubah perilaku penulisan data lifecycle batch — di luar
cakupan task ini dan berisiko melanggar batasan eksplisit pemilik untuk tidak mengubah
pencatatan/mutasi.

---

### ISS-FIN-002-03 — Footer menimpa konten Settlement AR

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan class/id `content-page` (atau turunan yang dibaca `footer.jsx`) pada kontainer scroll utama `settlement-ar-view.jsx` | Memakai mekanisme "ruang aman" yang sudah ada tanpa menyentuh `footer.jsx` sama sekali | Perlu memastikan class ini tidak bentrok dengan style existing halaman |
| B | Ubah `footer.jsx` agar mencari kontainer lewat selector yang lebih umum (mis. atribut data khusus) | Menutup akar masalah untuk seluruh halaman Finance yang mungkin punya masalah serupa | Radius dampak lebih besar — menyentuh komponen global yang dipakai seluruh aplikasi, bukan hanya Settlement AR |

**Solusi terpilih: Opsi A.**

1. Radius dampak paling kecil — hanya menyentuh halaman Settlement AR, sesuai cakupan task yang
   diminta pemilik.
2. `footer.jsx` adalah komponen global dipakai seluruh aplikasi (`FooterWrapper` merender di
   semua rute kecuali login/kiosk/nurse-display/error-page) — mengubahnya berisiko memengaruhi
   halaman lain yang saat ini sudah bekerja dengan benar.

**Kenapa bukan opsi lain.** Opsi B melanggar aturan cakupan ("jangan melakukan refactor terhadap
module yang tidak terkait" — `AGENTS.md` frontend §Aturan Cakupan Perubahan).

---

### ISS-FIN-002-04 — Layout filter, information card, dan label saldo

Karena `K-01` belum diputuskan, solusi berikut ditulis dengan asumsi Opsi (a) pada K-01
(tetapkan V2 sebagai desain resmi) — bila pemilik memilih opsi lain, bagian ini perlu ditulis
ulang.

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Susun ulang `settlement-ar-view.jsx` memakai grid 2 kolom (filter kiri ~33%, card kanan ~67%) di breakpoint desktop, tumpuk vertikal di bawahnya, pakai base component Finance existing (card, input, date-picker) | Konsisten dengan `rules/frontend/page-composition-patterns.md` dan base component catalog; tidak menambah dependency baru | Perlu audit base component searchable-dropdown mana yang sudah ada untuk filter entitas penjamin/pasien/karyawan |
| B | Buat ulang dari nol dengan component generik baru untuk grid 2 kolom | Fleksibel | Melanggar larangan "jangan membuat base component baru" tanpa kebutuhan jelas (`AGENTS.md` FE §Tanpa Abstraksi yang Tidak Diperlukan) |

**Solusi terpilih: Opsi A.**

1. Mengikuti instruksi `AGENTS.md` FE: *"Ikuti kode yang sudah ada. Jangan menciptakan arsitektur
   baru"* dan *"Gunakan kembali base component existing ketika sesuai."*
2. Tiga information card (Total Piutang, Beginning Balance, Ending Balance) sudah punya data
   sumbernya dari `FinanceSettlementSummaryResponse` — perubahan hanya pada tata letak dan teks
   label, bukan pada sumber data.

**Kenapa bukan opsi lain.** Opsi B melanggar aturan "tanpa abstraksi yang tidak diperlukan" pada
`AGENTS.md` frontend.

---

### ISS-FIN-002-05 — Tabel 7 kolom tanpa scroll horizontal

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Terapkan `table-layout: fixed; width: 100%; max-width: 100%` pada `DataTable`/`settlement-ar-table-columns.jsx`, set lebar kolom via persentase sesuai usulan pemilik (No 5%, Nama Pasien 21%, dst.), izinkan wrap pada Nama Pasien & No. Invoice saja | Sesuai permintaan eksplisit pemilik; `DataTable` base component sudah dipakai, tidak perlu komponen tabel baru | Perlu uji visual pada 6 breakpoint yang diminta (1920/1440/1366/1024/768/390) untuk memastikan tidak ada overlap |
| B | Pertahankan `overflow-x: auto` sebagai solusi utama | Perubahan minimal | Dilarang eksplisit oleh pemilik ("Jangan menggunakan overflow-x: auto sebagai solusi utama pada desktop") |

**Solusi terpilih: Opsi A.**

1. Memenuhi larangan eksplisit pemilik terhadap `overflow-x: auto` sebagai solusi utama.
2. `DataTable` (`@/components/features/base-features/data-table`) sudah dipakai di
   `settlement-ar-view.jsx:408-425` — perubahan cukup pada definisi kolom dan CSS, bukan
   mengganti komponen tabel.

**Kenapa bukan opsi lain.** Opsi B eksplisit dilarang pemilik pada laporan.

---

### ISS-FIN-002-06 — Implementasi tidak tertelusur di blueprint

Solusi bergantung sepenuhnya pada `K-01` — lihat bagian 6. Tidak ada solusi teknis yang dipilih
di sini karena ini murni keputusan tata kelola/arsitektur milik pemilik modul.

---

## 3. Skema tampilan sebelum → sesudah

> Catatan: rangka berikut disusun dari pembacaan `settlement-ar-view.jsx` dan
> `settlement-ar-table-columns.jsx` saat ini (SEBELUM), dipadukan dengan permintaan eksplisit
> pemilik pada laporan (SESUDAH) — **bukan** dari screenshot asli pemilik, karena screenshot
> tersebut belum benar-benar terlampir pada percakapan ini (lihat P-01). Proporsi dan susunan
> SESUDAH mengikuti usulan Bagian D/E pada laporan.

### SEBELUM (berdasarkan source saat ini, desktop)

```text
┌──────────────────────────────────────────────────────────────────┐
│ Settlement AR                                    [Ekspor Excel/PDF]│
│ Finance > Transaksi A/R > Pembayaran & Pelunasan > Settlement AR   │
├──────────────────────────────────────────────────────────────────┤
│ [Perusahaan] [Pasien Umum] [Karyawan]                              │
├──────────────────────────────────────────────────────────────────┤
│  (tata letak filter & card belum 2-kolom proporsional sesuai       │
│   permintaan — struktur persis belum dikonfirmasi dari screenshot) │
├──────────────────────────────────────────────────────────────────┤
│ NO | NAMA PASIEN | NO. INVOICE | BEG.BAL | END.BAL | TOTAL | AKSI  │
│  (lebar kolom tidak fixed — berpotensi scroll menyamping)          │
├──────────────────────────────────────────────────────────────────┤
│ [Pagination]                                                       │
└──────────────────────────────────────────────────────────────────┘
      ⚠ footer global (portal ke document.body) berpotensi menimpa
        baris/pagination terbawah bila kontainer bukan #content-page
```

### SESUDAH

```text
┌──────────────────────────────────────────────────────────────────┐
│ Settlement AR                                                      │
│ "Riwayat transaksi, pembayaran, dan saldo piutang."                │
│ Finance > Transaksi A/R > Pembayaran & Pelunasan > Settlement AR   │
├──────────────────────────────────────────────────────────────────┤
│ ( Perusahaan ) ( Pasien Umum ) ( Karyawan )     ← segmented tabs   │
├───────────────────────────┬──────────────────────────────────────┤
│ FILTER & PENCARIAN (~33%) │ INFORMATION CARDS (~67%)              │
│ [ Cari invoice/No.RM... ] │ ┌───────────┬───────────┬───────────┐ │
│ [ Perusahaan/Asuransi ▾ ] │ │Total      │Beginning  │Ending     │ │
│ [ Cari no. invoice...   ] │ │Piutang    │Balance    │Balance    │ │
│ [dd/mm/yyyy] [dd/mm/yyyy] │ │Rp X       │Rp Y       │Rp Z       │ │
│ [ Cari ]      ( Reset )   │ └───────────┴───────────┴───────────┘ │
├───────────────────────────┴──────────────────────────────────────┤
│ NO│ NAMA PASIEN      │ NO. INVOICE   │BEG.BAL│END.BAL│TOTAL│AKSI │
│ 5%│ 21% (bold+No.RM) │ 25% (teal,klik│  14%  │  14%  │ 15% │ 6%  │
│   │                  │  wrap)        │ rata kanan           │     │
├──────────────────────────────────────────────────────────────────┤
│ Menampilkan 1-10 dari Z data piutang            [‹ 1 2 3 ›]       │
└──────────────────────────────────────────────────────────────────┘
      (footer global tetap di bawah, tidak menimpa — ruang aman
       terpasang pada kontainer scroll halaman ini)
```

**Wilayah.**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Category tabs | Perusahaan / Pasien Umum / Karyawan | `query.Category` (state lokal `useSettlementAr`) | Segmented tab existing Finance (ditelusuri saat implementasi) |
| Filter kiri | Kata kunci, filter entitas (searchable), No. Invoice, rentang tanggal, tombol Cari/Reset | `FinanceSettlementQuery` (FE state) | Base component input/date-picker/searchable-dropdown existing |
| Information cards | Total Piutang, Beginning Balance, Ending Balance | `FinanceSettlementSummaryResponse` | Card existing Finance (lihat `rules/frontend/design-tokens.md` untuk padding/radius) |
| Tabel | 6 kolom data + Aksi | `FinanceSettlementRowResponse[]` | `DataTable` existing (`@/components/features/base-features/data-table`) |
| Pagination | "Menampilkan X-Y dari Z data piutang" + kontrol halaman | `PageNumber`/`PageSize`/`TotalData` | `Pagination` existing |

**Tombol.**

| Label | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cari | Primer (teal) | Selalu aktif | Terapkan filter, reset `PageNumber` ke 1, fetch ulang list+summary |
| Reset | Sekunder (outline) | Selalu aktif | Kembalikan seluruh filter ke default kategori aktif |
| Detail (per baris) / No. Invoice | Tautan/tombol | Selalu aktif selama baris punya `ReceivableId` | Navigasi ke Detail Settlement AR by `ReceivableId`, bukan nomor invoice |
| Ekspor Excel / PDF | Sekunder | Selalu aktif | Unduh file sesuai filter aktif, seluruh hasil filter |

**Keadaan.**

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Skeleton/spinner pada tabel dan card, filter tetap dapat diisi |
| Kosong | Tabel menampilkan pesan "Tidak ada data" tanpa menghilangkan filter yang sudah dipilih |
| Gagal (list) | State error pada section tabel + tombol "Coba Lagi"; filter tetap dipertahankan; tidak menampilkan "0 data" seolah berhasil |
| Gagal (summary) | Card menampilkan status "Gagal memuat ringkasan", bukan Rp0 tanpa label |
| Tidak berhak | Di luar cakupan — otorisasi `FinanceReceivable : Read` sudah ditegakkan backend (`[AccessPermission]` pada controller); FE tidak perlu menduplikasi logikanya |

---

## 4. Rincian satu perbaikan

### FIX-FIN-002-01 — Reproduksi dan perbaiki akar masalah HTTP 500

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-01` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (record proyeksi + dua titik pakai) |
| **Berkas yang diubah** | `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs` — `SettlementQueryRow` (baris 691-698), `BuildFilteredQueryable` (proyeksi 752-763, filter `Search` 782-793), `ApplySorting` (baris 798-827) |
| **Radius dampak** | `GetSettlement`, `GetSettlementSummary`, `ExportExcelAsync`, `ExportPdfAsync` — seluruhnya memanggil `BuildFilteredQueryable`/`ApplySorting` yang sama |
| **Bergantung pada** | Tidak ada — root cause sudah terbukti dari `Logs/quilvian-backend-20261011.json` |

**Langkah.**

1. Tambahkan `comp` (`MstCompanyGuarantor?`) dan `ins` (`MstInsuranceProvider?`) sebagai anggota
   baru pada record `SettlementQueryRow`, diisi dari join yang sudah ada (baris 746-749) —
   tidak perlu join baru.
2. Di `ApplySorting` cabang default (`patientname`), ganti referensi `x.debtorResolvedName`
   dengan ekspresi ternary yang sama persis namun dihitung langsung dari anggota record:
   `x.pat != null ? x.pat.FullName : x.comp != null ? x.comp.CompanyGuarantorName : x.ins != null ? x.ins.InsuranceProviderName : x.emp != null ? x.emp.FullName : null`.
3. Di filter `Search` (baris 792), ganti kondisi `x.debtorResolvedName != null && x.debtorResolvedName.ToUpper().Contains(search)`
   dengan kondisi setara yang memeriksa `x.comp`/`x.ins`/`x.emp`/`x.pat` langsung (masing-masing
   `!= null && ....Contains(search)`, digabung `||`), bukan merujuk `debtorResolvedName`.
4. Biarkan `debtorResolvedName` pada proyeksi tetap ada apa adanya — tetap dipakai di
   `GetSettlementPagedAsync` (baris 90-119) setelah `ToListAsync`, titik itu sudah aman.

**Acceptance criteria.**

1. Request `GET .../receivables/settlement?category=COMPANY&pageNumber=1&pageSize=10&sortBy=patientName&sortDirection=asc`
   mengembalikan HTTP 200 dengan `data` berisi `Items`/`Summary` (boleh kosong bila memang tidak
   ada data), bukan `success: false`.
2. Request yang sama dengan tambahan `search=<kata kunci>` juga mengembalikan HTTP 200 (filter
   `Search` memakai pola identik yang sebelumnya juga akan gagal).
3. `Logs/quilvian-backend-20261011.json` (atau log baru setelah perbaikan) tidak lagi mencatat
   `InvalidOperationException`/`could not be translated` untuk endpoint `receivables/settlement`
   setelah perbaikan diverifikasi berjalan.

**Verifikasi.** Review kode untuk memastikan ekspresi ternary pada langkah 2-3 identik secara
semantik dengan `debtorResolvedName` existing. Validasi build mengikuti permintaan eksplisit
pemilik: **jangan menjalankan `dotnet build`/`dotnet run` secara otomatis** — tunggu instruksi
eksplisit terpisah sebelum validasi build/test dijalankan; catat status build sebagai "belum
dijalankan (menunggu izin)" pada laporan task.

**Risiko.** Rendah — perubahan murni menghitung ulang ekspresi yang sudah ada di tempat lain
pada file yang sama (proyeksi `debtorResolvedName`), tanpa mengubah logika bisnis atau kontrak
respons. Risiko utama adalah salah menyalin urutan prioritas ternary (`comp→ins→emp→pat`) —
harus sama persis dengan proyeksi baris 758-761.

---

### FIX-FIN-002-02 — Tambahkan `!IsDelete` pada `batchSubquery`

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-02` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (satu baris predikat query) |
| **Berkas yang diubah** | `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceSettlementService.cs:734-735` |
| **Radius dampak** | Hanya `BuildFilteredQueryable` — dipakai oleh `GetSettlementPagedAsync`, `GetSettlementSummaryInternalAsync`, `ExportExcelAsync`, `ExportPdfAsync`, dan `GetSettlementDetailAsync` bila memakai query serupa |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Ubah predikat `batchSubquery` menjadi
   `.Where(b => !b.IsDelete && b.IsActiveMembership && b.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)`.

**Acceptance criteria.**

1. Untuk piutang pseudonim `REC-TEST-001` yang memiliki dua baris `FinReceivableInvoiceBatchItem`
   dengan `ReceivableId` sama — satu `IsDelete = true, IsActiveMembership = true` (sisa lifecycle
   lama) dan satu `IsDelete = false, IsActiveMembership = true` (aktif) — hasil
   `GetSettlementPagedAsync` menampilkan `REC-TEST-001` **tepat satu kali**.
2. `TotalData` pada response sama dengan `COUNT DISTINCT ReceivableId` yang cocok filter kategori
   aktif (validasi eksplisit yang diminta pemilik pada Bagian A §5).

**Verifikasi.** Review kode (satu baris) cukup untuk perubahan ini; validasi query penuh
membutuhkan data uji dengan membership batch ganda seperti di atas — catat sebagai kebutuhan
data uji pada `testing/acceptance-test-matrix.md` bila belum ada skenario ini.

**Risiko.** Tidak ada risiko regresi — hanya mempersempit (bukan memperluas) baris yang dianggap
"membership aktif", konsisten dengan makna `IsActiveMembership` yang dimaksud desain index
parsial pada `FinReceivableInvoiceBatchItemConfiguration.cs:38-41`.

---

### FIX-FIN-002-03 — Perbaiki ruang aman footer untuk Settlement AR

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (class/markup, bukan logika `footer.jsx`) |
| **Berkas yang diubah** | `src/components/view/finance/receivable/settlement/settlement-ar-view.jsx` (kontainer baris 100) |
| **Radius dampak** | Hanya halaman Settlement AR — tidak menyentuh `footer.jsx`/`footer-wrapper.jsx` yang dipakai seluruh aplikasi |
| **Bergantung pada** | P-01 (konfirmasi visual screenshot) disarankan sebelum menutup sebagai selesai, tetapi perbaikan teknis dapat dikerjakan lebih dulu |

**Langkah.**

1. Tambahkan id `content-page` atau class `.content-page` (sesuai yang benar-benar dibaca
   `footer.jsx:35-37` — perlu dikonfirmasi ulang saat implementasi mana yang dipakai: id atau
   class) pada kontainer scroll utama yang membungkus `finance-settlement-page`, tanpa mengubah
   struktur DOM yang dipakai bagian lain halaman.

**Acceptance criteria.**

1. Pada viewport 1366px dan 768px dengan data penuh 10 baris, baris terakhir tabel dan kontrol
   pagination terlihat utuh tanpa tertutup footer.
2. Tidak ada halaman Finance lain yang berubah perilaku footer-nya (karena `footer.jsx` sendiri
   tidak disentuh).

**Verifikasi.** Uji manual visual pada browser di breakpoint yang diminta pemilik (1920/1440/
1366/1024/768/390) — sesuai item testing FE §33 butir 15 pada laporan ("Footer tidak overlap").

**Risiko.** Bila class `content-page` sudah dipakai untuk kebutuhan styling lain pada halaman
Finance manapun, perlu dicek tidak ada bentrok aturan CSS sebelum diterapkan.

---

### FIX-FIN-002-04 — Revisi filter 2 kolom, information card, dan label saldo

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-04`, `ISS-FIN-002-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (layout + teks label) |
| **Berkas yang diubah** | `settlement-ar-view.jsx` (struktur filter & card, sekitar baris 100-408 saat ini) |
| **Radius dampak** | Hanya halaman Settlement AR |
| **Bergantung pada** | Tidak ada — K-01 diperlakukan disetujui Opsi (a), lihat catatan di bagian 2 |

**Langkah.**

1. Susun ulang kontainer filter+card menjadi grid 2 kolom (±33%/±67%) di breakpoint desktop,
   vertikal di tablet/mobile, memakai base component grid existing.
2. Perjelas label Beginning Balance card: saat `StartDate`/`EndDate` kosong, gunakan label
   "Nilai Awal Piutang" (bukan "Saldo Sebelum Awal Periode") sesuai §11 laporan; saat periode
   terisi, tampilkan tanggal posisi laporan.

**Acceptance criteria.**

1. Pada 1920px dan 1366px, filter menempati kolom kiri, tiga card berjajar sama tinggi di kolom
   kanan.
2. Pada 390px, filter dan card tersusun vertikal tanpa elemen terpotong.
3. Saat tidak ada `StartDate`/`EndDate` dipilih, card Beginning Balance berlabel "Nilai Awal
   Piutang"; saat ada, menampilkan tanggal posisi laporan sesuai periode.

**Verifikasi.** `npm run lint:errors` dan uji manual visual pada 6 breakpoint yang diminta
pemilik (§33 laporan: 1920/1440/1366/1024/768/390).

**Risiko.** Perubahan label card bisa berdampak ke ekspektasi pengguna yang sudah terbiasa
dengan tampilan sebelumnya — tidak mengubah nilai/angka, hanya teks label.

---

### FIX-FIN-002-05 — Tabel 7 kolom proporsional tanpa scroll horizontal

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (CSS kolom + definisi `buildSettlementTableColumns`) |
| **Berkas yang diubah** | `src/components/view/finance/receivable/settlement/settlement-ar-table-columns.jsx:20-145`; style terkait tabel |
| **Radius dampak** | Hanya tabel Settlement AR — `buildSettlementTableColumns` tidak dipakai modul lain (perlu konfirmasi ulang saat implementasi bila ternyata dipakai ulang) |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Terapkan `table-layout: fixed; width: 100%; max-width: 100%` pada tabel.
2. Set lebar kolom sesuai proporsi usulan pemilik (No 5%, Nama Pasien 21%, No. Invoice 25%,
   Beginning Balance 14%, Ending Balance 14%, Total Piutang 15%, Aksi 6%), disesuaikan bila
   pengujian visual menunjukkan kebutuhan readability berbeda.
3. Izinkan wrap pada kolom Nama Pasien dan No. Invoice saja; kolom nominal tetap satu baris,
   rata kanan, tidak terpotong.
4. Untuk mobile, ubah ke stacked-card layout yang tetap menampilkan seluruh 7 kolom informasi.

**Acceptance criteria.**

1. Pada 1920/1440/1366/1024px, seluruh 7 kolom terlihat tanpa scroll horizontal tingkat halaman.
2. Nominal pada kolom Beginning Balance/Ending Balance/Total Piutang tidak pernah terpotong untuk
   nilai hingga `Rp 999.999.999`.
3. Pada 768px dan 390px, data tetap lengkap (stacked-card atau compact mode), tidak ada data yang
   disembunyikan.

**Verifikasi.** Uji manual visual pada 6 breakpoint yang diminta pemilik; `npm run lint:errors`.

**Risiko.** Proporsi kolom usulan pemilik bersifat "acuan awal, boleh disesuaikan" (sesuai §23
laporan) — perlu uji dengan data nama pasien/invoice terpanjang yang realistis agar tidak
overlap.

---

### FIX-FIN-002-06 — Kondisi error list/summary

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`use-settlement-ar.jsx` state error + render kondisional di view) |
| **Berkas yang diubah** | `src/lib/hooks/finance/receivable/use-settlement-ar.jsx` (sekitar `fetchData` baris 245-289); `settlement-ar-view.jsx` |
| **Radius dampak** | Hanya halaman Settlement AR |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Pisahkan state gagal list vs gagal summary (saat ini `Promise.allSettled` pada baris 255-256
   sudah memisahkan kedua request — pastikan state error masing-masing dipakai terpisah di UI,
   bukan digabung menjadi satu pesan generik).
2. Tambahkan tombol "Coba Lagi" pada state gagal list yang memanggil ulang `fetchData` tanpa
   mereset filter yang sudah dipilih pengguna.
3. Saat summary gagal, tampilkan status "Gagal memuat ringkasan" pada card, bukan `Rp0` tanpa
   keterangan.

**Acceptance criteria.**

1. Saat endpoint list gagal (disimulasikan lewat mock/intercept), tabel menampilkan state error +
   tombol "Coba Lagi"; filter yang sudah diisi pengguna tetap terlihat terisi.
2. Saat endpoint summary gagal sementara list berhasil, card menampilkan "Gagal memuat ringkasan"
   dan bukan `Rp0`.

**Verifikasi.** Uji manual dengan memblokir salah satu endpoint lewat devtools network
throttling/block request; `npm run lint:errors`.

**Risiko.** Rendah — perubahan terisolasi pada state error, tidak mengubah alur data sukses.

---

### FIX-FIN-002-07 — Navigasi Detail mempertahankan filter

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (query string/state saat navigasi) |
| **Berkas yang diubah** | `use-settlement-ar.jsx` (`navigateToDetail`, baris 381-398); halaman detail terkait (`src/app/finance/receivable/[slug]/movements/page.jsx`) |
| **Radius dampak** | Settlement AR list ↔ Detail Settlement AR (movements view) |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Sertakan seluruh state filter aktif (`Category`, `Search`, filter entitas, periode,
   `PageNumber`, `SortBy`/`SortDirection`) sebagai query string saat `navigateToDetail` memanggil
   rute movements.
2. Saat tombol "Kembali" ditekan dari Detail, baca query string tersebut untuk mengembalikan
   state filter list, bukan selalu mereset ke halaman 1.

**Acceptance criteria.**

1. Dari halaman 3 dengan filter kategori Karyawan dan kata kunci tertentu, membuka Detail lalu
   menekan Kembali mengembalikan pengguna ke halaman 3 dengan kategori dan kata kunci yang sama.

**Verifikasi.** Uji manual navigasi pulang-pergi; `npm run lint:errors`.

**Risiko.** Rendah — murni penambahan parameter navigasi, tidak mengubah data yang ditampilkan.

---

### FIX-FIN-002-08 — Label semantik Beginning Balance tanpa periode

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-05` |
| **Area** | Backend (opsional, bila label dibutuhkan dari response) + Frontend |
| **Jenis perubahan** | Penambahan label/teks, bukan perubahan nilai |
| **Berkas yang diubah** | FE: komponen card ringkasan di `settlement-ar-view.jsx`. BE (opsional): `FinanceSettlementSummaryResponse`/`FinanceSettlementRowResponse` bila diputuskan label dikirim dari backend, bukan diturunkan di FE dari ada/tidaknya `StartDate`/`EndDate` |
| **Radius dampak** | Hanya tampilan card Beginning Balance |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Di FE, turunkan label card dari ada/tidaknya `query.StartDate`/`query.EndDate` yang sedang
   aktif (data ini sudah tersedia di state filter FE) — tidak perlu perubahan backend.

**Acceptance criteria.**

1. Tanpa periode dipilih: card Beginning Balance berlabel "Nilai Awal Piutang".
2. Dengan periode dipilih: card Beginning Balance berlabel seperti biasa dengan tanggal posisi
   laporan ditampilkan.

**Verifikasi.** Uji manual toggle filter periode on/off dan amati label berubah sesuai.

**Risiko.** Tidak ada — perubahan teks murni, nilai nominal tidak berubah.

---

### FIX-FIN-002-09 — Revisi dokumen blueprint agar mencerminkan implementasi V2

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-06` |
| **Area** | Dokumentasi blueprint |
| **Jenis perubahan** | Revisi dokumen desain (bukan source) |
| **Berkas yang diubah** | `03-frontend-architecture.md` §17.2 (baris 631); `02-backend-architecture.md` (tambahkan bagian baru untuk `FinanceSettlementService`); `contracts/api-contract.md` (tambahkan kontrak `receivables/settlement*`) |
| **Radius dampak** | Dokumentasi saja — tidak menyentuh source |
| **Bergantung pada** | Tidak ada — K-01 diperlakukan disetujui Opsi (a) |

**Langkah.** Lihat bagian 8 (Dokumen hulu yang ikut direvisi) — pekerjaan aktual dilakukan lewat
jalur blueprint berwenang (`manage-module-blueprint`/`design-business-module`), bukan oleh
`diagnose-module-issue` itu sendiri.

**Acceptance criteria.** Tertulis pada bagian 8.

**Verifikasi.** Tidak berlaku — dokumentasi, bukan source.

**Risiko.** Tanpa perbaikan ini, perbaikan UI/UX pada `FIX-FIN-002-04`/`05`/`06`/`07` akan terus
berjalan "di luar radar" blueprint, berulang pada siklus audit berikutnya.

---

### FIX-FIN-002-10 — Audit dua route Settlement AR

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-07` |
| **Area** | Frontend |
| **Jenis perubahan** | Investigasi lebih dulu; kemungkinan hapus/redirect satu route |
| **Berkas yang diubah** | Belum ditentukan — tergantung hasil audit konfigurasi menu/sidebar |
| **Radius dampak** | Routing Settlement AR |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Telusuri konfigurasi menu/sidebar untuk menemukan route mana yang benar-benar ditautkan ke
   menu "Settlement AR".
2. Tentukan apakah `src/app/finance/settlement/page.jsx` adalah kode mati, alias redirect, atau
   dipakai menu lain yang namanya kebetulan sama.

**Acceptance criteria.**

1. Satu dokumentasi singkat (pada laporan task) menyebutkan status masing-masing route dan
   tindakan yang diambil (dipertahankan dengan alasan / dihapus / diarahkan ulang).

**Verifikasi.** `npm run lint:errors`, `npm run build` (sesuai jadwal validasi yang disetujui
pemilik — tidak dijalankan otomatis untuk backend, FE tetap mengikuti `AGENTS.md` FE kecuali
pemilik meminta sebaliknya).

**Risiko.** Menghapus route yang ternyata masih dipakai akan memutus akses pengguna — audit menu
harus tuntas sebelum penghapusan.

---

### FIX-FIN-002-11 — Tambahkan opsi urut Total Piutang

| | |
| --- | --- |
| **Menutup** | `ISS-FIN-002-08` |
| **Area** | Frontend (+ Backend bila K-02 = Ya) |
| **Jenis perubahan** | Source frontend (daftar opsi dropdown urut) |
| **Berkas yang diubah** | `src/lib/constants/finance/receivable/settlement-ar-constants.jsx:45-54` |
| **Radius dampak** | Hanya kontrol urut pada tabel Settlement AR |
| **Bergantung pada** | `K-02` untuk cakupan `beginningBalance`/`endingBalance` |

**Langkah.**

1. Tambahkan entri `totalPiutang` ke `SETTLEMENT_SORT_OPTIONS` (backend `ApplySorting` sudah
   mendukung `"totalpiutang"`).
2. Bila `K-02` = Ya, ajukan task backend terpisah untuk menambah dukungan
   `beginningBalance`/`endingBalance` pada `ApplySorting` sebelum menambah opsi ini di FE.

**Acceptance criteria.**

1. Dropdown urut menampilkan opsi "Total Piutang" dan hasil tabel terurut sesuai saat dipilih.

**Verifikasi.** Uji manual memilih opsi urut baru dan membandingkan urutan baris dengan nilai
`TotalPiutang`.

**Risiko.** Rendah — backend sudah mendukung nilai sort ini, perubahan FE murni menambah opsi.

---

## 5. Urutan pengerjaan

```text
BE-FIN-102 (FIX-FIN-002-01 + FIX-FIN-002-02)   tanpa dependency — root cause terbukti, boleh dikerjakan sekarang
FE-FIN-044 (FIX-FIN-002-03/04/05/06/07/08/10/11-FE)   tanpa dependency — K-01 diperlakukan disetujui Opsi (a)
FIX-FIN-002-09   tanpa dependency — dikerjakan langsung sebagai revisi dokumen blueprint
P-01 (jawaban pelapor: screenshot asli — opsional, mempercepat verifikasi visual)
└── FIX-FIN-002-03   (perbaikan teknis sudah dikerjakan di FE-FIN-044 tanpa menunggu P-01; verifikasi visual akhir menunggu)
K-02 (keputusan pemilik: sort Beginning/Ending Balance)
└── FIX-FIN-002-11 (bagian backend)   bagian totalPiutang FE sudah termasuk FE-FIN-044, tidak menunggu K-02
```

---

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari dokumen issue bagian 6:

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bagaimana memperlakukan implementasi `FinanceSettlementService`/halaman dedicated yang tidak tertelusur di blueprint? | (a) Tetapkan sebagai desain resmi baru; (b) Bongkar, kembalikan ke `/finance/receipts`; (c) Tunda revisi UI/UX, selesaikan hanya bug 500+duplikasi dulu | **(a) — diperlakukan disetujui 2026-10-11** atas instruksi langsung pemilik ("kerjakan semua") | ~~`FIX-FIN-002-04`, `-05`, `-06`, `-07`, `-09`~~ tidak lagi tertahan |
| K-02 | Apakah Beginning Balance/Ending Balance perlu bisa diurutkan dari tabel? | Ya (butuh kerja backend tambahan) / Tidak | Tidak (bukan bagian permintaan laporan manapun) | Bagian backend `FIX-FIN-002-11` — **masih tertahan**, belum dijawab |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | §17.2, baris 631 (baris "Settlement AR") | `/finance/receipts` \| `FIN-LYR-AR-09` \| `FinanceReceipt : Read` \| Sudah ada (`FE-FIN-004`) | Rute dedicated `/finance/receivable/settlement` (atau rute final hasil `FIX-FIN-002-10`), hak akses `FinanceReceivable : Read`, menunjuk ke implementasi V2 (`FinanceSettlementService`), beserta rujukan bagian skema tampilan baru yang memuat layout filter/card/tabel hasil `FIX-FIN-002-04`/`-05` |
| `02-backend-architecture.md` | Bagian baru (belum ada bagian Settlement AR sama sekali) | Tidak ada | Bagian baru yang mendokumentasikan `FinanceSettlementService`: entitas yang digabung, logika `BeginningBalance`/`EndingBalance`, logika membership invoice batch aktif (termasuk perbaikan `FIX-FIN-002-02`) |
| `contracts/api-contract.md` | Bagian baru | Tidak memuat endpoint `receivables/settlement*` sama sekali | Tambahkan lima endpoint (list, summary, detail, export excel, export pdf) beserta bentuk `FinanceSettlementQuery`/`FinanceSettlementRowResponse`/`FinanceSettlementSummaryResponse` |
| `00-interview-decisions.md` | Dekat `FIN-DEC-107` (baris 1223) | Menyatakan Settlement AR = reuse `/finance/receipts` | Tambahkan catatan revisi keputusan bertanggal 2026-10-11 yang menyatakan pemilik memutuskan membangun layar dedicated V2, menggantikan pendekatan reuse untuk item menu ini saja (Ayat Silang tetap memakai `/finance/receipts` sesuai `FIN-DEC-096`) |

Revisi dokumen desain ini **tidak dikerjakan oleh `diagnose-module-issue`** — dikerjakan lewat
`manage-module-blueprint`/`design-business-module` setelah `K-01` dijawab, lalu baris pada tabel
ini ditandai selesai.

---

## 8. Verifikasi menyeluruh

Setelah seluruh `FIX` di atas disetujui dan dikerjakan:

1. Request `GET .../receivables/settlement?category=COMPANY&pageNumber=1&pageSize=10&sortBy=patientName&sortDirection=asc`
   mengembalikan HTTP 200.
2. Piutang dengan membership batch ganda (satu soft-delete, satu aktif) muncul tepat satu kali
   dan `TotalData` cocok `COUNT DISTINCT ReceivableId`.
3. Pada 1920/1440/1366/1024px tidak ada scroll horizontal tingkat halaman pada tabel; pada
   768/390px seluruh data tetap terlihat dalam bentuk compact/stacked.
4. Footer tidak menimpa baris tabel/pagination terbawah pada seluruh breakpoint di atas.
5. Navigasi Detail → Kembali mempertahankan kategori, filter, periode, halaman, dan sorting yang
   sama persis.
6. Export Excel/PDF menghasilkan seluruh baris hasil filter (bukan hanya 10 baris halaman aktif)
   dengan nominal numeric pada Excel.
7. Dokumen blueprint (`03-frontend-architecture.md`, `02-backend-architecture.md`,
   `contracts/api-contract.md`) sudah direvisi sesuai bagian 7 dan tidak lagi membantah source.
8. Validasi otomatis: FE — `npm run lint:errors` dan `npm run test:unit` (minimum sesuai
   `AGENTS.md` FE); BE — **tidak dijalankan otomatis** (`dotnet build`/`dotnet test` menunggu
   instruksi eksplisit terpisah dari pemilik, sesuai arahan yang diberikan selama sesi ini).

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-11 | Rencana dibuat, 11 perbaikan, 2 pertanyaan pelapor (P-01/P-02) dan 2 keputusan pemilik (K-01/K-02) menahan sebagian perbaikan | `diagnose-module-issue` |
| 2026-10-11 | P-02 terjawab dari `Logs/quilvian-backend-20261011.json` (bukan dari pelapor) — root cause `FIX-FIN-002-01` dipastikan: `InvalidOperationException` pada translasi `debtorResolvedName`. Pemilik menginstruksikan "kerjakan semua" via `build-module-backend`/`build-module-frontend`; K-01 diperlakukan disetujui Opsi (a). Task diregistrasi: `BE-FIN-102` (FIX-01+02), `FE-FIN-044` (FIX-03/04/05/06/07/08/10/11-FE). K-02 tetap tertahan | agent pemanggil (sesi ini) |
| 2026-10-11 | Task ID FE semula `FE-FIN-043` diganti `FE-FIN-044` — sesi lain memakai `FE-FIN-043` untuk issue berbeda (Laporan AR) hampir bersamaan. `BE-FIN-102` dan `FE-FIN-044` dikerjakan (source selesai, build/uji peramban belum). Koreksi dua dugaan: `FIX-FIN-002-03` (footer) ternyata sudah benar tanpa perubahan — mekanisme `content-page` sudah bekerja di level shell aplikasi; `FIX-FIN-002-11` (opsi sort) premisnya keliru — `SETTLEMENT_SORT_OPTIONS` adalah kode mati, tidak ada kontrol sort yang pernah dibangun. `FIX-FIN-002-10` (hapus route duplikat `/finance/settlement`) selesai. `FIX-FIN-002-09` (revisi blueprint) dan K-02 masih terbuka | agent pemanggil (sesi ini) |
