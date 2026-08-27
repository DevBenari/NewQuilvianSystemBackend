# FE-BKC-007 — Operasi Shift Kasir

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-007` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice, **halaman baru** (bukan bagian invoice detail) |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` mengikuti presedens task sebelumnya |
| Branch frontend | `yasmina` |
| Route baru | `/health-services/billing-management/cashier/shifts` — menu "Shift Kasir" ditambahkan ke blok "Billing dan Kasir" |
| Status task | Source selesai, lulus lint/build/`test:unit`. Belum di-commit, belum diverifikasi manual. |

## Ringkasan untuk pembaca umum

Halaman baru "Shift Kasir" untuk kasir membuka, menyerahterimakan, dan menutup shift kerjanya, serta
untuk Kepala Kasir meninjau selisih kas dan membuka kembali shift yang sudah ditutup.

**Temuan penting yang membatasi seberapa mulus fitur ini bisa dipakai saat ini**: backend
`CashierShiftsController` hanya punya satu endpoint baca — `GET /current`, yang HANYA
mengembalikan shift AKTIF milik pengguna yang sedang login. **Tidak ada endpoint untuk mencari
atau melihat shift milik kasir lain** (baik shift yang sedang menunggu handover kepada Anda,
maupun shift `CLOSED_WITH_VARIANCE` yang perlu direview Kepala Kasir). **`RegisterId` (identitas
meja/konter kasir) juga belum punya master data** — backend hanya menerima GUID apa pun tanpa
validasi keberadaannya.

Karena keterbatasan ini, tiga aksi yang menyasar shift **milik orang lain** (konfirmasi terima
handover, review variance, dan reopen) dibangun memakai input manual Shift ID + Row Version, yang
harus disampaikan secara langsung (verbal/chat) oleh pihak yang mengetahuinya — bukan dicari
otomatis dari dalam aplikasi. Ini didokumentasikan secara eksplisit sebagai keterbatasan backend,
bukan kekurangan implementasi frontend.

## Proses bisnis

### Proses 1 — Membuka dan Menutup Shift (shift milik sendiri)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Kasir mencatat mulai dan selesainya masa kerja kasir, termasuk kas pembukaan dan kas fisik saat penutupan. |
| Pelaku | Kasir dengan hak `CashierShift : Create` (buka) dan `CashierShift : Close` (tutup). |
| Pemicu | Kasir menekan "+ Buka Shift" di awal kerja, "Tutup Shift" di akhir kerja. |
| Prasyarat buka | Kasir belum punya shift aktif lain, dan register yang dipilih belum dipakai shift aktif lain. |
| Prasyarat tutup | Shift berstatus `OPEN`/`REOPENED`, tidak ada handover yang masih menunggu konfirmasi. |
| Langkah utama | 1) Kasir membuka "+ Buka Shift", mengisi ID Register dan kas pembukaan. 2) Sistem membuat shift baru berstatus `OPEN`. 3) Selama shift berjalan, kas sistem (`systemCash`) bertambah otomatis setiap ada penerimaan tunai (top-up deposit, tender tunai — dari `FE-BKC-005`/`006`). 4) Di akhir kerja, kasir menghitung kas fisik sungguhan dan mengisi form "Tutup Shift". 5) Sistem menghitung variance = kas fisik − (kas pembukaan + kas sistem). |
| Aturan bisnis | Variance nol → shift langsung `CLOSED`. Variance tidak nol (lebih atau kurang) → `CLOSED_WITH_VARIANCE`, wajib direview Kepala Kasir sebelum dianggap selesai — selisih **tidak pernah dihilangkan diam-diam**. |
| Contoh konkret | Kas pembukaan Rp500.000, kas sistem (dari penerimaan tunai selama shift) Rp2.300.000 → kas diharapkan Rp2.800.000. Kasir menghitung kas fisik Rp2.795.000 → variance −Rp5.000 (kurang). Shift menjadi `CLOSED_WITH_VARIANCE`, menunggu review Kepala Kasir. |
| Perubahan status | Tidak ada → `OPEN` (buka). `OPEN`/`REOPENED` → `CLOSED` (variance nol) atau `CLOSED_WITH_VARIANCE` (ada selisih). |
| Jalur tidak normal | • Kasir sudah punya shift aktif → ditolak `409`. • Register sedang dipakai shift aktif lain → ditolak `409`. • Ada handover tertunda saat mencoba tutup → ditolak dengan pesan jelas. |
| Hasil akhir | Riwayat shift tercatat lengkap (kas pembukaan/sistem/fisik/variance, waktu buka-tutup) untuk audit keuangan. |

### Proses 2 — Handover Shift (dua aktor, lintas sesi login)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memindahkan tanggung jawab shift (dan kas yang sedang dipegang) dari satu kasir ke kasir lain tanpa menutup dan membuka shift terpisah, mis. pergantian jam kerja. |
| Pelaku | Kasir pemilik shift (mengajukan) dan kasir penerima (mengonfirmasi) — **wajib dua login berbeda**. |
| Pemicu | Kasir pemilik menekan "Ajukan Handover"; kasir penerima menekan "Konfirmasi Terima Shift" dari sesi login mereka sendiri. |
| Prasyarat | Shift pemilik `OPEN`/`REOPENED`, belum ada handover tertunda lain. Kasir penerima belum punya shift aktif sendiri. Kasir penerima harus berbeda dari kasir pemilik. |
| Langkah utama | 1) Kasir pemilik mengisi ID kasir penerima dan alasan, submit. 2) Sistem membuat catatan handover berstatus `PENDING`; shift pemilik tetap `OPEN` tapi kini bertanda "menunggu konfirmasi". 3) Halaman menampilkan Shift ID dan Row Version shift tersebut untuk disalin dan disampaikan ke kasir penerima. 4) Kasir penerima login, membuka "Konfirmasi Terima Shift", menempelkan Shift ID + Row Version yang diterima, mengisi alasan, submit. 5) Sistem menutup shift lama (`HANDED_OVER`) dan membuat shift baru untuk kasir penerima dengan kas pembukaan = kas pembukaan + kas sistem shift lama (kas berpindah utuh). |
| Aturan bisnis | Endpoint yang sama dipakai untuk mengajukan maupun mengonfirmasi — backend membedakan berdasarkan siapa yang login, bukan lewat aksi terpisah. Kasir pemilik tidak bisa mengonfirmasi handover-nya sendiri (harus orang lain yang login). |
| Contoh konkret | Kasir A (kas sistem Rp1.200.000, kas pembukaan Rp500.000) mengajukan handover ke Kasir B. Kasir B login, memasukkan Shift ID + Row Version yang diberikan Kasir A, konfirmasi. Shift Kasir A menjadi `HANDED_OVER`; Kasir B mendapat shift baru dengan kas pembukaan Rp1.700.000 (Rp500.000 + Rp1.200.000) — kas tidak hilang atau harus dihitung ulang dari nol. |
| Perubahan status | Shift asal: `OPEN` → (pending, tetap `OPEN`) → `HANDED_OVER`. Shift baru kasir penerima: tidak ada → `OPEN`. |
| Jalur tidak normal | • Kasir penerima masih punya shift aktif sendiri → ditolak `409`. • Orang selain kasir pemilik/penerima yang ditunjuk mencoba mengonfirmasi → `403`. • Row Version yang dimasukkan sudah usang (mis. Shift ID/Row Version salah salin) → `409`, pesan jelas. |
| Hasil akhir | Tanggung jawab dan kas shift berpindah utuh ke kasir baru, tercatat sebagai satu rangkaian dua command (ajukan + konfirmasi) di audit trail. |

### Proses 3 — Review Variance dan Reopen (Kepala Kasir, shift milik orang lain)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Kepala Kasir meninjau selisih kas yang ditemukan saat penutupan shift, dan bila perlu membuka kembali shift yang sudah ditutup untuk koreksi. |
| Pelaku | Kepala Kasir dengan hak `CashierShift : Review` (review) dan `CashierShift : Reopen` (reopen) — otoritas berbeda dari kasir biasa. |
| Pemicu | Kasir yang menutup shift dengan variance menyampaikan Shift ID + Row Version kepada Kepala Kasir (di luar aplikasi — lihat catatan keterbatasan). Kepala Kasir membuka "Review Variance" atau "Buka Kembali Shift". |
| Prasyarat review | Shift berstatus `CLOSED_WITH_VARIANCE`. | 
| Prasyarat reopen | Shift berstatus `CLOSED` atau `REVIEWED`. |
| Langkah utama | 1) Kepala Kasir memasukkan Shift ID + Row Version yang disampaikan. 2) Untuk review: mengisi resolusi (mis. "Selisih diterima, disebabkan pembulatan kembalian") dan alasan, submit → shift menjadi `REVIEWED`. 3) Untuk reopen: mengisi alasan, submit → shift menjadi `REOPENED` (bisa dipakai transaksi lagi oleh kasir aslinya). |
| Aturan bisnis | Review **tidak menghilangkan** nilai variance — hanya mencatat resolusi di atasnya, nilai selisih tetap tersimpan permanen untuk audit. Reopen memerlukan otoritas kebijakan eksplisit dan selalu tercatat dengan alasan di audit trail. |
| Contoh konkret | Shift dengan variance −Rp5.000 (dari Proses 1). Kepala Kasir mereview dengan resolusi "Selisih diterima, dalam batas toleransi pembulatan" — shift menjadi `REVIEWED`, variance −Rp5.000 tetap tercatat di riwayat, tidak diubah menjadi nol. |
| Perubahan status | `CLOSED_WITH_VARIANCE` → `REVIEWED` (review). `CLOSED`/`REVIEWED` → `REOPENED` (reopen). |
| Jalur tidak normal | • Shift bukan `CLOSED_WITH_VARIANCE` saat direview → ditolak. • Shift bukan `CLOSED`/`REVIEWED` saat reopen → ditolak. • Shift Kasir/Register sudah punya shift aktif lain saat reopen → `409`. |
| Hasil akhir | Selisih kas terdokumentasi dengan resolusi yang jelas untuk audit; shift bisa dibuka kembali secara terkontrol bila memang diperlukan koreksi. |

## Temuan — keterbatasan backend yang memengaruhi UX (bukan celah UI)

1. **Tidak ada endpoint pencarian/daftar shift**. `GET /current` hanya mengembalikan shift aktif
   milik pemanggil sendiri. Tidak ada `GET /{id}` untuk melihat shift lain, dan tidak ada daftar
   "shift menunggu handover ke saya" atau "shift menunggu review variance". Akibatnya, tiga aksi
   yang menyasar shift orang lain (konfirmasi handover, review variance, reopen) dibangun dengan
   input manual Shift ID + Row Version — bekerja, tapi mengharuskan koordinasi di luar aplikasi
   (pesan langsung/verbal) dan rawan salah salin. **Rekomendasi**: tambahkan `GET
   /shifts/{id}` (baca saja, dengan otorisasi role yang sesuai) pada backend supaya frontend bisa
   membangun alur "masukkan ID → lihat pratinjau → konfirmasi" yang jauh lebih aman.
2. **Tidak ada master data Register**. `RegisterId` pada `OpenShiftRequest` adalah GUID bebas
   tanpa validasi referensial di backend. Form "Buka Shift" memakai input teks bebas dengan
   catatan agar ID dikoordinasikan konsisten oleh tim operasional. Menambahkan master data
   Register (dengan GET `/options`, seperti pola `PaymentMethod`/`BillingItemCategory`) akan
   memungkinkan picker yang layak.
3. Tidak ada picker untuk memilih "kasir penerima" pada handover — input teks ID pengguna,
   dengan asumsi koordinasi antar-kasir dilakukan di luar aplikasi (lazim di operasional rumah
   sakit, tapi tetap dicatat sebagai gap UX).

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Cashier / Shifts

Base URL: `api/v1/health-services/billing-management/cashier/shifts`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/open` | Membuka shift baru | `CashierShift : Create` | `OpenShiftRequest` (`registerId`, `openingCash`, `correlationId`, `causationId`) + `Idempotency-Key` | `ApiResponse<CashierShiftResponse>` |
| `GET` | `/current` | Shift aktif milik pengguna yang login | `CashierShift : Read` | — | `ApiResponse<CashierShiftResponse>` (`404` bila tidak ada — ditangani sebagai state kosong) |
| `POST` | `/{id}/handover` | Mengajukan ATAU mengonfirmasi handover (tergantung pelaku) | `CashierShift : Handover` | `HandoverShiftRequest` (`receivingCashierId`, `expectedRowVersion`, `reason`, `correlationId`, `causationId`) + `Idempotency-Key` | `ApiResponse<CashierShiftResponse>` |
| `POST` | `/{id}/close` | Menutup shift | `CashierShift : Close` | `CloseShiftRequest` (`physicalCash`, `expectedRowVersion`, `correlationId`, `causationId`) + `Idempotency-Key` | `ApiResponse<CashierShiftResponse>` |
| `POST` | `/{id}/variance-reviews` | Kepala Kasir mereview selisih | `CashierShift : Review` | `ReviewVarianceRequest` (`expectedRowVersion`, `resolution`, `reason`, `correlationId`, `causationId`) + `Idempotency-Key` | `ApiResponse<CashVarianceResponse>` |
| `POST` | `/{id}/reopen` | Membuka kembali shift | `CashierShift : Reopen` | `ReopenShiftRequest` (`expectedRowVersion`, `reason`, `correlationId`, `causationId`) + `Idempotency-Key` | `ApiResponse<CashierShiftResponse>` |

Seluruhnya sudah diimplementasikan backend sejak commit `22bf9cf`; `contracts/api-contract.md`
masih menandainya "Rencana (belum tersedia)" — temuan yang sama seperti task sebelumnya.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200`/`201` | Berhasil. |
| `403` | Bukan pihak yang berwenang — mis. bukan kasir penerima yang ditunjuk saat konfirmasi handover. |
| `404` | Tidak ada shift aktif (untuk `GET current`, ditampilkan sebagai state kosong) atau shift tidak ditemukan. |
| `409` | Kasir/register sudah punya shift aktif lain, ada handover tertunda, atau Shift ID/Row Version yang dimasukkan sudah usang/salah. |
| `422` | Aturan bisnis tidak terpenuhi — status shift tidak sesuai untuk aksi yang diminta, nominal tidak valid, dsb. |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Cashier/Controllers/CashierShiftsController.cs` (keenam endpoint), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs:35-134` (`OpenAsync`), `:154-347` (`HandoverAsync` — logika dua-aktor lewat satu endpoint), `:349-404` (`CloseAsync` — perhitungan variance), `:406-523` (`ReviewVarianceAsync`), `:525-574` (`ReopenAsync`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs:13` (`RegisterId` — bukti tidak ada foreign key/master data), commit `22bf9cf`.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/cashier-shift-slice.jsx` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/cashier-shift/use-cashier-shift.js`, `cashier-shift-constants.js` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/cashier-shift/*.jsx` (7 file, **baru** — view + 6 modal) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/app/health-services/billing-management/cashier/shifts/page.jsx`, `cashier-shifts-client.jsx` (**baru**, route baru) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/utils/menu-sidebar/menu-items.jsx` (entri "Shift Kasir" ditambahkan) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/state/store.jsx` (registrasi reducer baru) — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-007`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Dua aktor handover | **Terpenuhi (source), lewat relay manual** | Endpoint sama, dua login berbeda wajib — dipaksa backend. Frontend memfasilitasi lewat Shift ID + Row Version yang disalin manual (lihat Temuan #1). |
| Variance tetap terlihat | **Terpenuhi** | Header shift menampilkan kas fisik dan variance untuk shift berstatus closed/closed-with-variance/reviewed; review tidak menghilangkan nilainya. |
| Unauthorized action tersembunyi dan 403 aman | **Sebagian** | Aksi yang jelas tidak berlaku disembunyikan/nonaktif berdasarkan status shift yang terlihat (mis. Tutup Shift hanya untuk shift aktif sendiri). Untuk tiga aksi "shift lain", frontend **tidak bisa** menyembunyikan tombol berdasarkan kewenangan pengguna atas shift TERTENTU (karena tidak ada data shift itu untuk diperiksa sebelumnya) — mengandalkan backend menolak `403` dengan pesan jelas, ditampilkan apa adanya. |
| Late noncash tidak mengubah physical | **Terpenuhi (mengikuti backend)** | `ApplyCashReceiptAsync` hanya menambah `systemCash`, tidak pernah menyentuh `physicalCash` yang sudah dicatat kasir sendiri — frontend tidak menampilkan/mengedit field ini secara terpisah. |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint <14 file yang diubah/baru>` (severity penuh) | **PASS** | Tanpa output. |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 58s`, route baru `/health-services/billing-management/cashier/shifts` terkonfirmasi ter-compile. |
| `npm run test:unit` | **PASS** | 38 test, 38 pass, 0 fail. Tidak ada test yang menguji kode shift kasir. |
| Smoke-test browser headless tanpa login | **PARTIAL PASS** | 0 exception JS pada route baru, redirect ke `/login` bersih setelah `401`. |
| Verifikasi manual ter-autentikasi (dua akun kasir + satu akun Kepala Kasir) | **NOT DONE** | Tidak ada kredensial. Butuh minimal 2 akun kasir + 1 akun Kepala Kasir untuk menguji handover dan review secara penuh. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint/build/test:unit lulus bersih, tapi
klik-coba langsung (buka shift, handover dua aktor, tutup dengan variance, review, reopen) belum
dijalankan sama sekali.

## Langkah berikutnya yang direkomendasikan

1. Login dengan minimal 2 akun kasir (untuk handover) dan 1 akun Kepala Kasir (untuk review/reopen)
   di `localhost:3000`, verifikasi manual seluruh acceptance criteria di atas.
2. Sekarang `FE-BKC-006` (split tender) bisa diverifikasi penuh untuk skenario tunai, karena shift
   kasir sudah tersedia.
3. Pertimbangkan menambahkan `GET /shifts/{id}` dan master data Register di backend (temuan #1
   dan #2) sebagai task tooling terpisah untuk menghilangkan kebutuhan relay ID manual.
4. Lanjut ke `FE-BKC-008` (financial exception workbench) atau `FE-BKC-009` (finalisasi invoice).
