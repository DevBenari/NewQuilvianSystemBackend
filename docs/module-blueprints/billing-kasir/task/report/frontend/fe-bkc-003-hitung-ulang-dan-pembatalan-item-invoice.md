# FE-BKC-003 — Hitung Ulang dan Pembatalan Item Invoice

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-003` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan — wewenang lintas-repo untuk write ini diberikan eksplisit oleh pemilik task pada sesi yang sama |
| Branch frontend | `yasmina` |
| Frontend snapshot (awal task) | `fac1b49c8` |
| Backend snapshot (dibaca sebagai bukti) | `8e48237` |
| Status task | Source selesai ditulis dan lulus lint. Belum di-commit, belum diverifikasi manual (lihat bagian *Definition of Done*) |

## Ringkasan untuk pembaca umum

Halaman detail invoice Billing (yang menampilkan daftar item tagihan sebuah kunjungan pasien)
sebelumnya hanya bisa **dilihat**. Task ini menambahkan dua tombol tindakan pada halaman yang
sama:

1. **Hitung Ulang** — memaksa sistem menghitung ulang total tagihan invoice dari item-item yang
   masih aktif saat ini, dan menyimpan hasilnya sebagai versi kalkulasi baru. Versi lama tetap
   tersimpan sebagai riwayat, tidak hilang.
2. **Batalkan** (per item) — membatalkan satu item tagihan yang salah/keliru dicatat, dengan
   alasan wajib diisi. Setelah item dibatalkan, sistem otomatis menghitung ulang total tagihan.

Kedua tombol ini hanya aktif selama invoice masih berstatus `OPEN` (belum difinalisasi). Invoice
yang sudah `FINAL`/`CLOSED` bersifat baca-saja — perubahan pada invoice yang sudah final harus
lewat mekanisme *adjustment* terpisah (di luar scope task ini).

## Proses bisnis

### Proses 1 — Hitung Ulang Invoice

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memastikan total tagihan invoice mencerminkan item-item yang aktif saat ini, misalnya setelah ada item yang berubah di sumbernya. |
| Pelaku | Petugas Billing/Kasir yang punya hak `BillingInvoice : Update`. |
| Pemicu | Petugas menekan tombol "Hitung Ulang" pada halaman detail invoice. |
| Prasyarat | Invoice berstatus `OPEN`. Petugas mengisi alasan (wajib, tidak boleh kosong). |
| Langkah utama | 1) Petugas membuka detail invoice. 2) Petugas menekan "Hitung Ulang". 3) Sistem menampilkan dialog konfirmasi yang meminta alasan. 4) Petugas mengisi alasan dan menekan "Ya, Hitung Ulang". 5) Sistem mengirim permintaan ke backend beserta versi data invoice yang sedang dilihat petugas (`rowVersion`) dan alasan. 6) Backend membuat versi kalkulasi baru dan mengembalikannya. 7) Halaman menampilkan versi kalkulasi baru sebagai kalkulasi berjalan; versi sebelumnya tetap tampil di riwayat. |
| Aturan bisnis | Total tagihan (`GrossAmount`), biaya admin, diskon, pajak, dan porsi pasien/penjamin dihitung ulang sepenuhnya oleh backend berdasarkan item yang masih aktif (item yang sudah dibatalkan tidak ikut dihitung). Frontend tidak melakukan perhitungan apa pun sendiri — hanya menampilkan hasil dari backend. |
| Contoh konkret | Invoice awalnya punya 3 item aktif dengan total kotor Rp1.500.000. Setelah satu item senilai Rp300.000 dibatalkan (Proses 2), petugas menekan "Hitung Ulang". Versi kalkulasi baru menampilkan total kotor Rp1.200.000 (Rp1.500.000 − Rp300.000), dan versi kalkulasi sebelumnya (Rp1.500.000) tetap terlihat di tabel "Riwayat Kalkulasi" untuk audit. |
| Perubahan status | Status invoice **tidak berubah** (tetap `OPEN`). Yang bertambah adalah nomor versi kalkulasi (`CurrentCalculationVersion`) dan penanda versi data (`RowVersion`) invoice, dipakai untuk mendeteksi bila ada petugas lain yang mengubah invoice bersamaan. |
| Jalur tidak normal | • Invoice sudah bukan `OPEN` (mis. sudah `FINAL`) → sistem menolak dengan pesan "Invoice final tidak dapat diedit; ajukan adjustment", tombol otomatis nonaktif di frontend. • Versi data invoice yang dipakai petugas sudah usang (petugas lain mengubah invoice ini lebih dulu) → sistem menolak dengan kode `409` dan pesan "Data telah berubah. Muat ulang sebelum melanjutkan."; frontend menampilkan pesan tersebut dan **otomatis memuat ulang** data invoice supaya petugas melihat versi terbaru. • Alasan tidak diisi → tombol konfirmasi tetap nonaktif sampai alasan diisi (dicegah di frontend), dan backend tetap menolak permintaan kosong sebagai jaring pengaman kedua. |
| Hasil akhir | Invoice punya satu versi kalkulasi baru yang menjadi acuan tagihan berjalan. Versi-versi sebelumnya tetap tersimpan sebagai riwayat yang tidak bisa dihapus. |

### Proses 2 — Membatalkan Item Invoice

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Menghapus (secara logis) satu item tagihan yang keliru dicatat, tanpa menghapus jejaknya dari sistem. |
| Pelaku | Petugas Billing/Kasir yang punya hak `BillingInvoice : Update`. |
| Pemicu | Petugas menekan tombol "Batalkan" pada baris item tertentu di tabel "Item Invoice". |
| Prasyarat | Invoice berstatus `OPEN`. Item yang dipilih belum berstatus `VOIDED`. Belum ada versi kalkulasi invoice ini yang "terkunci" (`IsLocked`) — kalau sudah terkunci berarti item tersebut sudah diproses lebih lanjut (misalnya sudah tercakup pembayaran), sehingga tidak boleh dibatalkan langsung. Petugas mengisi alasan pembatalan (wajib). |
| Langkah utama | 1) Petugas menekan "Batalkan" pada baris item yang salah. 2) Sistem menampilkan dialog konfirmasi berisi deskripsi item dan meminta alasan. 3) Petugas mengisi alasan dan menekan "Ya, Batalkan". 4) Sistem mengirim permintaan pembatalan ke backend beserta data asal item tersebut (informasi dari mana item ini berasal) dan alasan. 5) Backend menandai item sebagai `VOIDED`, mencatat alasannya, lalu otomatis membuat versi kalkulasi baru (sama seperti Proses 1) supaya total tagihan langsung sesuai. 6) Halaman menampilkan status item sebagai "Voided" beserta alasannya pada kolom "Keterangan", dan total tagihan yang sudah diperbarui. |
| Aturan bisnis | Item yang sudah dibatalkan tidak dihitung lagi ke total tagihan. Pembatalan tidak menghapus baris item dari tabel — baris tetap ada dengan status `VOIDED` dan alasan, untuk keperluan audit (item tidak pernah dihapus permanen dari sistem). |
| Contoh konkret | Item "Pemeriksaan Laboratorium" senilai Rp300.000 salah dicatat dua kali pada invoice yang sama. Petugas membatalkan salah satu baris dengan alasan "Item duplikat, sudah tercatat di baris lain". Baris tersebut berubah status menjadi "Voided" dengan keterangan "Item duplikat, sudah tercatat di baris lain", dan total tagihan invoice berkurang Rp300.000. |
| Perubahan status | Item: `ACTIVE` → `VOIDED` (satu arah, tidak bisa dikembalikan ke `ACTIVE` dari halaman ini). Invoice: status tetap `OPEN`, versi kalkulasi bertambah satu (sama seperti Proses 1). |
| Jalur tidak normal | • Item sudah `VOIDED` sebelumnya → tombol "Batalkan" otomatis nonaktif dengan keterangan "Item ini sudah dibatalkan.". • Invoice sudah bukan `OPEN`, atau ada versi kalkulasi yang sudah terkunci (item sudah diproses lebih lanjut, misal sudah masuk pembayaran) → backend menolak dengan pesan bisnis yang jelas ("Invoice final tidak dapat diedit..." atau "Item tidak dapat dibatalkan karena pelayanan atau pembayaran sudah diproses."); frontend menampilkan pesan tersebut. • Permintaan pembatalan yang sama terkirim dua kali (misalnya koneksi terputus lalu petugas mencoba lagi) → backend mendeteksi ini sebagai permintaan yang sama (bukan pembatalan ganda) dan mengembalikan hasil yang sama seperti percobaan pertama — item **tidak** dibatalkan dua kali. • Versi data invoice sudah usang saat petugas menekan konfirmasi → sama seperti Proses 1: kode `409`, pesan "Data telah berubah...", dan frontend otomatis memuat ulang. |
| Hasil akhir | Item berstatus `VOIDED` dengan alasan tercatat, tidak lagi ikut dihitung ke total tagihan, dan invoice punya versi kalkulasi baru yang mencerminkan hal itu. |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

Grup Swagger ini persis nilai atribut `[Tags(...)]` pada
`Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/recalculate` | Membuat versi kalkulasi baru dari item yang masih aktif | `BillingInvoice : Update` | `RecalculateInvoiceRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<CalculationResponse>` |
| `POST` | `/{id}/items/{itemId}/void` | Membatalkan satu item invoice yang eligible | `BillingInvoice : Update` | `VoidInvoiceItemRequest` (`expectedRowVersion`, `sourceVersion`, `sourceStatus`, `contractVersion`, `reason`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<InvoiceDetailResponse>` |

Kedua endpoint ini **sudah diimplementasikan backend** (bukan "Rencana (belum tersedia)") —
lihat bagian *Temuan* di bawah soal dokumen kontrak yang belum mencerminkan ini.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil. Halaman menampilkan hasil terbaru. |
| `404` | Invoice atau item tidak ditemukan (mis. tautan/ID tidak valid). |
| `409` | Data invoice sudah berubah sejak terakhir dimuat oleh petugas lain. Halaman menampilkan pesan dan memuat ulang datanya secara otomatis. |
| `422` | Aturan bisnis tidak terpenuhi — misalnya invoice sudah final, alasan kosong, atau item sudah diproses lebih lanjut sehingga tidak boleh dibatalkan. Pesannya ditampilkan apa adanya dari backend (sudah dalam Bahasa Indonesia). |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs:89-153` (endpoint `recalculate` dan `items/{itemId}/void`), commit `22bf9cf` ("Add module backend billing dan kasir part 2").
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:234-352` (`VoidItemAsync`, termasuk pengecekan status `OPEN`, `RowVersion`, kalkulasi terkunci, dan pemanggilan `RecalculateAsync` otomatis setelah void), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:467-491` (`ValidateVoidRequest` — daftar lengkap field wajib), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs:38-48` (`RecalculateAsync` — validasi `ExpectedRowVersion` dan `Reason` wajib), commit `22bf9cf`.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx:129-186` (thunk `recalculateBillingInvoice`, `voidBillingInvoiceItem`) — belum di-commit, working tree branch `yasmina`.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoice-detail.js` (state dialog, idempotency key per sesi dialog, penanganan `409`) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx` (tombol "Hitung Ulang", kolom "Aksi"/"Keterangan", dua `ConfirmModal`) — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-003`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Double-submit aman | **Terpenuhi (source)** | `Idempotency-Key`/`correlationId`/`causationId` dibuat sekali saat dialog dibuka, dipakai ulang untuk setiap percobaan konfirmasi pada dialog yang sama; backend mendeteksi payload identik sebagai replay (`BillingInvoiceService.cs:265-273`). Tombol konfirmasi nonaktif selama proses berjalan (`confirm-modal.jsx`). Belum diverifikasi manual end-to-end. |
| `409` meminta reload | **Terpenuhi (source)** | `use-billing-invoice-detail.js` fungsi `handleActionError` mendeteksi `statusCode === 409` dan memanggil ulang `getBillingInvoiceById`. Belum diverifikasi manual. |
| Invoice final read-only | **Terpenuhi (source)** | Tombol "Hitung Ulang" dan "Batalkan" nonaktif ketika `invoice.status !== "OPEN"`; backend tetap menolak independen (`BillingInvoiceService.cs:275-277`, `BillingCalculationService.cs` — validasi serupa). Belum diverifikasi manual. |
| Reason wajib | **Terpenuhi (source)** | `ConfirmModal` dengan `requireReason` menonaktifkan tombol konfirmasi sampai alasan diisi; backend menolak independen bila kosong (`ValidateVoidRequest`, `RecalculateAsync`). |
| Item sensitif dimask | **Tidak berubah dari scope task** | Task ini tidak menambah tampilan data pasien baru di luar yang sudah ada sejak `FE-BKC-001` (nomor sumber, deskripsi item). Tidak ada field sensitif baru yang ditampilkan. |
| Provenance source minimal | **Terpenuhi** | Kolom "Sumber" (sudah ada dari `FE-BKC-001`) tetap satu-satunya info asal item yang ditampilkan; field provenance lain (`sourceVersion`/`sourceStatus`/`contractVersion`) dipakai untuk mengisi request void secara otomatis, tidak ditampilkan sebagai form yang bisa diedit pengguna. |

## Temuan (bukan diperbaiki task ini — di luar wewenang tulis)

1. `contracts/api-contract.md` (`BIL-API-0.4`) masih menandai seluruh endpoint Billing Invoices,
   termasuk `recalculate` dan `items/{itemId}/void`, sebagai "Rencana (belum tersedia)". Bukti di
   atas menunjukkan keduanya sudah diimplementasikan sejak commit `22bf9cf`. Dokumen kontrak
   perlu diperbarui oleh skill pemilik kontrak (`design-business-module`/`plan-module-delivery`),
   bukan oleh laporan build ini.
2. Modul `billing-management` di frontend (invoices + 4 halaman master-data policy dari task
   sebelumnya) belum terdaftar di menu navigasi (`src/utils/menu-sidebar/menu-items.jsx`).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit code 0, tanpa output, di seluruh repository frontend. |
| Lint severity penuh pada 3 file yang diubah | **PASS** | `npx eslint <3 file>` tanpa `--quiet` → nol error, nol warning. |
| `npm run build` | **PASS** | Setelah Node lokal di-upgrade ke `v22.23.2`, `next build` selesai dengan `✓ Compiled successfully in 87s`, termasuk route `/health-services/billing-management/billing/invoices/[slug]` yang diubah task ini — tanpa error/warning. `postbuild` (`prepare-standalone.mjs`) juga selesai normal. Diulang dua kali dengan hasil identik. |
| `npm run test:unit` | **PASS (harness); 1 kegagalan pre-existing tidak terkait** | Pemilik task memperbaiki script `test:unit` di `package.json` (glob `tests/unit/**/*.test.mjs` menggantikan path direktori `tests/unit/`), menyelesaikan ketidakcocokan harness dengan Node 22.x. Hasil: 34 test ditemukan, 33 lulus, 1 gagal (`tests/unit/auth-security.test.mjs` — meng-`import` `src/utils/auth/base-login-utils.jsx` padahal file aslinya `.js`; bug pre-existing, tidak tersentuh task ini). **Tidak ada satu pun dari 34 test yang menguji kode billing-invoice** — task ini belum menambah unit/component test baru, meski acceptance roadmap menyebut "Component tests success/invalid/conflict/403". |
| Verifikasi manual (browser, tanpa login) | **PARTIAL** | `npm run dev` dijalankan lokal (terhubung ke API dev nyata `api-dev.quilvian-mmchospital.com` lewat `.env`). Smoke-test headless (Playwright, Chromium) pada 3 route: `/login` (render normal, 0 exception), daftar invoice (redirect bersih ke `/login` setelah `401`, 0 exception), **detail invoice yang diubah task ini** dengan ID palsu (redirect bersih ke `/login` setelah `401`, **0 exception JS** saat pohon komponen ter-mount — termasuk Hero action baru, kolom Aksi/Keterangan, dan dua `ConfirmModal`). Screenshot tersimpan lokal (tidak di-commit). |
| Verifikasi manual ter-autentikasi (klik tombol Hitung Ulang/Batalkan pada invoice nyata) | **NOT DONE** | Tidak ada kredensial login yang tersedia untuk builder; sengaja tidak diminta lewat chat untuk alasan keamanan. Dev server dibiarkan berjalan di `localhost:3000` untuk dicoba manual oleh pemilik task. |

**Task ini belum bisa ditandai selesai sepenuhnya** — `lint`, `build`, dan harness `test:unit`
sudah lulus bersih (evidence di atas), tapi klik-coba langsung dengan invoice nyata dan akun
sungguhan (kelima acceptance criteria pada tabel di atas) belum dijalankan, dan belum ada
unit/component test baru untuk kode billing-invoice.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya `BillingInvoice : Update` di `localhost:3000` (dev server sudah
   berjalan), buka satu invoice `OPEN`, lalu verifikasi manual kelima acceptance criteria pada
   tabel di atas (Hitung Ulang, Batalkan, konflik `409`, invoice final read-only, reason wajib).
2. Tambahkan unit/component test untuk `billing-invoice-slice.jsx`/`use-billing-invoice-detail.js`
   mengikuti pola `tests/unit/*.test.mjs` yang sudah ada, memenuhi acceptance "Component tests
   success/invalid/conflict/403" pada roadmap.
3. Setelah verifikasi manual dan test selesai dan dicatat, modul ini siap lanjut ke task berikutnya
   (`FE-BKC-004` diskon/approval dokter, atau task navigasi menu yang masih tertunda).
