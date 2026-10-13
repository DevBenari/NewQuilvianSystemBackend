# Laporan Perubahan Frontend — `FE-FIN-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-044` |
| Judul | Settlement AR — revisi UI/UX (kategori tab, tabel 7 kolom, error state, navigasi detail) |
| Slice | Perbaikan bug + revisi UI/UX pasca-laporan pemilik, di luar gelombang REV-18 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "Perbaikan `ISSUE-FIN-002` — Settlement AR" |
| Trace | `ISSUE-FIN-002` (`ISS-FIN-002-03`, `ISS-FIN-002-04`, `ISS-FIN-002-05`, `ISS-FIN-002-07`, `ISS-FIN-002-08`), `PLAN-REPAIR-FIN-002` (`FIX-FIN-002-04`, `-05`, `-06`, `-07`, `-08`, `-10`) |
| Kontrak | Tidak berubah — endpoint, bentuk request/response, dan nama field tetap sama persis. Tidak ada field baru dari backend yang dikonsumsi |
| Wewenang UI | `PLAN-REPAIR-FIN-002` bagian 4, disetujui pemilik 11 Oktober 2026 ("kerjakan semua") — K-01 diperlakukan disetujui Opsi (a) |
| Dependency | `BE-FIN-102` (perbaikan HTTP 500) — tidak memblokir pekerjaan FE ini karena murni perubahan tampilan/komposisi, bukan konsumsi field baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/components/view/finance/receivable/settlement/`, `src/lib/hooks/finance/receivable/use-settlement-ar.jsx`, `src/components/view/finance/receivable/movements/receivable-movements-view.jsx`; dihapus: `src/app/finance/settlement/page.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `46a1549dc` (branch `yasmina`) |
| Tanggal | 2026-10-11 |
| Status | 🟡 **Sebagian** — source selesai dan lolos lint; `npm run build` dan uji visual peramban **NOT RUN** |

---

## 1. Masalah yang diperbaiki dan temuan yang terkoreksi

Bagian ini sekaligus mengoreksi tiga dugaan pada `ISSUE-FIN-002` yang ternyata, setelah membaca
source sebenarnya (bukan hanya ringkasan audit awal), **tidak membutuhkan perubahan di titik yang
semula diduga** — dicatat jujur sesuai aturan `diagnose-module-issue` bagian "jangan mengarang
atau membiarkan dugaan keliru tanpa koreksi".

1. **Footer menimpa tabel (`ISS-FIN-002-03`) — dugaan semula tidak terbukti.** Audit awal menduga
   penyebabnya adalah pembungkus halaman Settlement AR (`finance-settlement-page`) tidak
   memiliki id/class `content-page` yang dibaca `footer.jsx` untuk memberi *safe space*.
   Pembacaan ulang `src/components/ui/content-page.jsx` dan
   `src/components/providers/root-layout-shell.jsx` membuktikan **setiap halaman normal** (bukan
   kiosk/nurse-view/login/error) — termasuk Settlement AR — sudah otomatis dibungkus
   `<div id="content-page" className="content-page">` oleh shell aplikasi, di level yang lebih
   tinggi dari halaman manapun. Mekanisme *safe space* footer karena itu **sudah bekerja dengan
   benar** untuk halaman ini tanpa perubahan apa pun. **Tidak ada perubahan source yang
   diterapkan untuk temuan ini.** Dampak visual yang dilaporkan pemilik (dari screenshot yang
   tidak ikut terlampir pada percakapan ini) kemungkinan besar bersumber dari hal lain — lihat
   bagian 7.
2. **Opsi urut hilang untuk kolom nominal (`ISS-FIN-002-08`) — premis semula keliru.** Audit awal
   menduga `SETTLEMENT_SORT_OPTIONS` "kurang lengkap" karena tidak mencakup `totalPiutang`/
   `beginningBalance`/`endingBalance`. Pembacaan ulang membuktikan konstanta ini **sama sekali
   tidak dipakai** di mana pun pada aplikasi (`grep` nol hasil selain deklarasinya sendiri) — tidak
   ada dropdown/kontrol urut yang dirender untuk Settlement AR sama sekali. Ini bukan "opsi yang
   kurang lengkap", melainkan kode mati yang disiapkan untuk kontrol yang belum pernah dibangun.
   Menambah entity baru ke dalamnya tidak berdampak apa pun selama tidak ada UI yang
   memakainya — dan membangun kontrol dropdown urut baru adalah penambahan fitur yang **tidak**
   diminta eksplisit pada laporan manapun (laporan hanya meminta tabel bisa diklik header-nya
   untuk sort, bukan dropdown terpisah). **Tidak dikerjakan** — dicatat sebagai temuan untuk
   keputusan pemilik terpisah bila kontrol urut memang diinginkan ke depan.
3. **Dua route untuk Settlement AR (`ISS-FIN-002-07`, bagian audit) — terbukti dan diperbaiki.**
   `src/app/finance/settlement/page.jsx` dan `src/app/finance/receivable/settlement/page.jsx`
   sama-sama merender `SettlementArView` yang identik. Konfigurasi menu
   (`src/utils/menu-sidebar/menu-items.jsx:793`) hanya menautkan `/finance/receivable/settlement`.
   Route `/finance/settlement` terbukti **tanpa satu pun rujukan** di seluruh codebase selain
   dirinya sendiri — dihapus.

Masalah yang benar-benar diperbaiki (`BUG`/`DESIGN_CHANGE` terverifikasi):

- Tab kategori (Perusahaan/Pasien Umum/Karyawan) berada di dalam kolom kanan (sejajar card),
  bukan di atas seluruh area filter+card seperti diminta Bagian D §18 laporan.
- Label card Beginning Balance selalu berbunyi "Saldo sebelum awal periode" walau tidak ada
  periode dipilih — semestinya "Nilai Awal Piutang" saat tanpa periode (Bagian B §11 laporan).
- Saat endpoint ringkasan (`summary`) gagal sementara daftar berhasil, ketiga card diam-diam
  menampilkan `Rp 0` tanpa keterangan apa pun — bukan sesuai permintaan eksplisit pemilik untuk
  tidak menampilkan `Rp0` otomatis atau angka lama tanpa label (Bagian D §21).
- Tidak ada tombol "Coba Lagi" saat daftar gagal dimuat, hanya pesan error pasif.
- Navigasi ke Detail Settlement AR dan kembali sudah mempertahankan kategori/filter/halaman,
  **kecuali** `sortBy`/`sortDirection` yang tidak ikut terbawa (gap sempit, bukan kegagalan total).
- Lebar kolom tabel (`minWidth` 60-200px per kolom, total ±1000px) berisiko memicu scroll
  horizontal pada viewport 1024px sesuai permintaan "tanpa scroll horizontal pada desktop/laptop".
- `column.headerAlign` pada definisi kolom tidak pernah dibaca `DataTable` (yang membaca
  `column.align`), sehingga header kolom nominal tidak benar-benar rata kanan meski kodenya
  terlihat seperti sudah mengatur itu.

---

## 2. Proses bisnis

Petugas Finance membuka Settlement AR → memilih kategori lewat tab yang kini berada di atas,
terpisah dari filter dan ringkasan → mengisi filter (kata kunci, entitas, no. invoice, rentang
tanggal) → melihat tiga card ringkasan (Total Piutang, Beginning Balance, Ending Balance) yang
labelnya menyesuaikan ada/tidaknya periode dipilih → bila ringkasan gagal dimuat, card
menampilkan "Gagal memuat ringkasan" alih-alih angka nol yang menyesatkan → bila daftar gagal
dimuat, petugas melihat pesan error beserta tombol "Coba Lagi" tanpa kehilangan filter yang sudah
diisi → tabel menampilkan 7 kolom tanpa perlu scroll menyamping di laptop/desktop → mengklik
nomor invoice atau tombol Aksi membuka Detail Settlement AR, dan menekan Kembali mengembalikan
petugas ke posisi filter, halaman, dan urutan yang persis sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/view/finance/receivable/settlement/settlement-ar-view.jsx` (seluruh berkas)
- `src/components/view/finance/receivable/settlement/settlement-ar-table-columns.jsx` (seluruh berkas)
- `src/lib/hooks/finance/receivable/use-settlement-ar.jsx` (seluruh berkas)
- `src/components/view/finance/receivable/movements/receivable-movements-view.jsx` (bagian `backHref`)
- `src/lib/constants/finance/receivable/settlement-ar-constants.jsx` (audit `SETTLEMENT_SORT_OPTIONS`, tidak diubah)
- `src/components/features/footers/footer.jsx`, `src/style/components/features/footer.css`, `src/components/ui/content-page.jsx`, `src/components/providers/root-layout-shell.jsx` (audit mekanisme footer, tidak diubah)
- `src/components/features/base-features/data-table.jsx`, `src/style/components/features/base-features/base-data-components.module.css` (audit perilaku `overflow-x`/`align`, tidak diubah — komponen bersama lintas modul)
- `src/app/finance/settlement/page.jsx` vs `src/app/finance/receivable/settlement/page.jsx` (audit duplikasi)
- `src/utils/menu-sidebar/menu-items.jsx` (konfirmasi route aktif)
- `docs/module-blueprints/finance-management/docs/issue/issue-002-settlement-ar-http-500-dan-revisi-uiux.md`, `docs/plan-repair/plan-repair-002-settlement-ar-http-500-dan-revisi-uiux.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `settlement-ar-view.jsx` | Tab kategori dipindah ke atas blok filter+card (full width, sebelum `<Row>` 2 kolom), duplikatnya di kolom kanan dihapus. Blok error list kini menyertakan tombol "Coba Lagi" (`retryFetch`). Label card Beginning Balance dinamis (`hasPeriod`). Ketiga card menampilkan "Gagal memuat ringkasan" (merah) saat `summaryErrorMessage` terisi, alih-alih `Rp 0` diam-diam. Komentar kolom tabel diperbarui dari "6 Kolom V1" menjadi "7 Kolom" |
| `settlement-ar-table-columns.jsx` | `minWidth`/`width` tujuh kolom diperketat (total ±1000px → ±824px) agar tidak memicu scroll horizontal pada viewport 1024px. `headerAlign` (tidak pernah dibaca `DataTable`) diganti `align` pada 4 kolom (3 nominal + Aksi) agar header benar-benar rata kanan/tengah. Komentar header berkas diperbarui dari "6 kolom V1" menjadi "7 kolom" |
| `use-settlement-ar.jsx` | Tambah state `summaryErrorMessage` dan fungsi `retryFetch`. `fetchData` kini membedakan kegagalan ringkasan dari kegagalan daftar — tidak lagi membiarkan `summary` tetap `null`/stale tanpa penanda saat `summaryRes` gagal. `navigateToDetail` kini ikut menyertakan `sortBy`/`sortDirection` pada query string ke halaman Detail |
| `receivable-movements-view.jsx` | `backHref` kini ikut membaca & meneruskan `sortBy`/`sortDirection` dari query string saat kembali ke Settlement AR |
| `src/app/finance/settlement/page.jsx` | **Dihapus** — duplikat route yang merender komponen identik dengan `/finance/receivable/settlement`, nol rujukan di menu atau source lain |
| `docs/module-blueprints/finance-management/docs/issue/issue-002-...md`, `docs/plan-repair/plan-repair-002-...md` | Koreksi `ISS-FIN-002-03` dan `ISS-FIN-002-08` (lihat bagian 1); pembaruan register status `FIX-FIN-002-04` s/d `-08`, `-10` |
| `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` | Tambah bagian "Perbaikan `ISSUE-FIN-002`" beserta kartu task `FE-FIN-044` |

### 3.3 Dampak kontrak API dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol field baru dikonsumsi dari backend, nol perubahan bentuk request |
| Keamanan/Auth | `NOT APPLICABLE` — nol perubahan `AccessDeniedGate`/permission check |

---

## 4. Verifikasi

| Perintah/skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint settlement-ar-view.jsx settlement-ar-table-columns.jsx use-settlement-ar.jsx receivable-movements-view.jsx --quiet` | Exit `0`, nol error | `PASS` | Output command, kode keluar `0` |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik pada sesi ini (diterapkan juga ke `npm run build` sampai dikonfirmasi terpisah, mengikuti preseden `FE-FIN-042`) |
| Uji visual peramban (6 breakpoint: 1920/1440/1366/1024/768/390) | Tidak dijalankan | `NOT FEASIBLE` | Server dev tidak dijalankan pada sesi ini |
| Pembacaan ulang `backHref`/`navigateToDetail` setelah edit | `sortBy`/`sortDirection` konsisten ikut dikirim dan dibaca kedua arah | `PASS` | Pembacaan source kedua berkas |
| Pembacaan ulang route `/finance/settlement` | Terhapus, `/finance/receivable/settlement` tetap utuh dan satu-satunya yang ditautkan menu | `PASS` | `ls src/app/finance/` tidak lagi memuat `settlement/` |

**Tidak dijalankan:** `npm run build`, `npm run test:unit`, seluruh uji visual peramban pada 6
breakpoint yang diminta pemilik — menunggu konfirmasi eksplisit untuk menjalankan dev
server/build pada sesi berikutnya.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tab kategori berada di atas, bukan di kolom kanan | **Terpenuhi** | `settlement-ar-view.jsx`, blok tab dipindah sebelum `<Row>` filter+card |
| Label Beginning Balance dinamis sesuai ada/tidaknya periode | **Terpenuhi** | Kondisi `hasPeriod` pada card 2 |
| Ringkasan gagal tidak menampilkan `Rp0`/angka lama tanpa label | **Terpenuhi** | `summaryErrorMessage` pada ketiga card |
| Tombol "Coba Lagi" tersedia saat daftar gagal, filter tidak hilang | **Terpenuhi** | Blok error list + `retryFetch`; `appliedFilters` tidak direset oleh fungsi ini |
| Sorting ikut terbawa saat ke Detail dan kembali | **Terpenuhi** | `navigateToDetail` dan `backHref` |
| 7 kolom tidak memicu scroll horizontal pada 1920/1440/1366/1024px | Belum dapat dipastikan visual — perubahan lebar sudah diterapkan (±824px total) | Menunggu uji peramban sungguhan |
| Route duplikat Settlement AR dibereskan | **Terpenuhi** | `/finance/settlement` dihapus |
| Footer tidak menimpa tabel | **Sudah benar tanpa perubahan** — mekanisme `content-page` sudah bekerja di level shell aplikasi | Lihat bagian 1 butir 1 |

Task ini **belum** dapat ditandai ✅ selama uji visual peramban pada 6 breakpoint dan `npm run
build` belum dikonfirmasi pemilik.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` tidak dijalankan otomatis pada sesi ini, mengikuti instruksi eksplisit pemilik yang semula ditujukan untuk `dotnet build` dan diperluas berhati-hati ke `npm run build` |
| Masalah yang diketahui | `FIX-FIN-002-09` (revisi dokumen blueprint) dan bagian backend `FIX-FIN-002-11` (dukungan sort Beginning/Ending Balance, menunggu K-02) belum termasuk task ini |
| Temuan di luar cakupan | `SETTLEMENT_SORT_OPTIONS` adalah kode mati (dead code) — tidak dihapus karena di luar permintaan laporan manapun, dicatat untuk keputusan pemilik bila kontrol urut akan dibangun |
| Risiko tersisa | Rendah untuk kebenaran logika (murni tampilan/komposisi, nol logika bisnis baru). Risiko visual: proporsi lebar kolom baru belum diuji di peramban sungguhan pada 6 breakpoint yang diminta |
| Perubahan sampingan | `NONE` selain penghapusan route duplikat yang sudah dikonfirmasi tanpa rujukan |
| Interupsi | `NONE` |
| Status Git | `M src/components/view/finance/receivable/settlement/settlement-ar-view.jsx`, `M src/components/view/finance/receivable/settlement/settlement-ar-table-columns.jsx`, `M src/lib/hooks/finance/receivable/use-settlement-ar.jsx`, `M src/components/view/finance/receivable/movements/receivable-movements-view.jsx`, `D src/app/finance/settlement/page.jsx` (di repo frontend); `M docs/module-blueprints/finance-management/docs/issue/`, `M docs/module-blueprints/finance-management/docs/plan-repair/`, `M docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, `?? docs/module-blueprints/finance-management/task/report/frontend/FE-FIN-044.md` (di repo backend, host blueprint) |
| Langkah berikutnya | Pemilik mengonfirmasi `npm run build` dan uji visual 6 breakpoint pada `PLAN-REPAIR-FIN-002` bagian 8; setelah PASS, tandai `FE-FIN-044` ✅ pada roadmap dan `requirement-traceability.md`. `FIX-FIN-002-09` (revisi blueprint) dan K-02 (sort balance) masih terbuka |
