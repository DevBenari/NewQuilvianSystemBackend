# FE-BKC-004 — Diskon Promo dan Approval Dokter

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-004` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan — wewenang lintas-repo untuk write ini diberikan eksplisit oleh pemilik task pada sesi yang sama (mengikuti presedens `FE-BKC-003`) |
| Branch frontend | `yasmina` |
| Frontend snapshot (awal task) | `fac1b49c8` (belum berubah — task `FE-BKC-003` juga belum di-commit) |
| Backend snapshot (dibaca sebagai bukti) | `8e48237` |
| Status task | Source selesai ditulis, lulus lint dan build. Belum di-commit, belum diverifikasi manual. |

## Ringkasan untuk pembaca umum

Halaman detail invoice Billing sekarang punya satu bagian baru: **Diskon**. Petugas Billing/Kasir
bisa mengajukan diskon dari policy yang berlaku, dan dokter bisa menyetujui diskon jasa dokter
miliknya sendiri langsung dari halaman yang sama.

Ada tiga jenis diskon dengan perilaku berbeda:

1. **Promo Total** — potongan untuk keseluruhan porsi tagihan pasien, nilainya sudah ditentukan
   oleh policy (persentase atau nominal tetap), langsung efektif tanpa perlu persetujuan siapa
   pun.
2. **Promo Item** — sama seperti Promo Total, tapi menyasar satu item tagihan tertentu, juga
   langsung efektif.
3. **Diskon Dokter** — potongan dari jasa dokter pada satu item tertentu. Nominalnya diketik
   manual oleh kasir (bukan dari policy), dan **wajib disetujui oleh dokter yang memiliki jasa
   tersebut** sebelum ikut mengurangi tagihan. Bila nominal yang diajukan melebihi batas limit
   yang ditentukan policy, pengajuan otomatis dialihkan ke alur persetujuan Finance (di luar
   scope task ini — hanya perlu terlihat jelas statusnya).

Penting: mengajukan diskon **tidak otomatis mengubah total tagihan yang tampil**. Petugas perlu
menekan tombol "Hitung Ulang" (dari task `FE-BKC-003`) setelah diskon efektif, supaya kalkulasi
tagihan memperhitungkan diskon tersebut.

## Proses bisnis

### Proses 1 — Mengajukan Diskon Promo (Promo Total / Promo Item)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Menerapkan potongan harga standar (promo) pada invoice tanpa perlu persetujuan tambahan. |
| Pelaku | Petugas Billing/Kasir yang punya hak `BillingDiscount : Create`. |
| Pemicu | Petugas menekan "+ Ajukan Diskon" pada halaman detail invoice, memilih policy bertipe Promo Total/Promo Item. |
| Prasyarat | Invoice berstatus `OPEN`. Policy diskon aktif dan sedang berlaku (dalam periode efektif). Untuk Promo Item: item invoice yang dipilih masih aktif (belum dibatalkan) dan bukan biaya administrasi. |
| Langkah utama | 1) Petugas membuka dialog "Ajukan Diskon". 2) Memilih policy dari daftar policy aktif. 3) Untuk Promo Item, memilih item invoice target. 4) Mengisi alasan. 5) Menekan "Ajukan Diskon". 6) Sistem menghitung nominal diskon dari rumus policy (persentase atau nominal tetap, dibatasi limit bila ada) dan langsung menandainya efektif. 7) Baris diskon baru muncul di tabel Diskon dengan status "Disetujui". |
| Aturan bisnis | Nominal diskon **selalu dihitung backend**, tidak boleh diisi manual oleh petugas untuk tipe promo. Diskon tidak boleh menyasar item biaya administrasi. Policy yang sama tidak boleh diterapkan dua kali pada target (invoice/item) yang sama. |
| Contoh konkret | Policy "PROMO10" (Promo Item, persentase 10%, limit Rp100.000) diterapkan pada item senilai Rp800.000. Perhitungan: 10% × Rp800.000 = Rp80.000. Karena Rp80.000 masih di bawah limit Rp100.000, nominal yang tersimpan adalah Rp80.000, langsung berstatus "Disetujui". |
| Perubahan status | Diskon baru langsung berstatus `APPROVED` (Disetujui) — tidak ada status antara. |
| Jalur tidak normal | • Item yang dipilih adalah biaya administrasi → ditolak dengan pesan "Biaya administrasi tidak dapat didiskon." (frontend tidak memfilter kategori ini secara khusus untuk Promo Item — lihat catatan simplifikasi di bagian Temuan). • Policy yang sama sudah pernah diterapkan pada target yang sama → ditolak dengan kode `409` dan pesan "Policy diskon sudah diterapkan pada target invoice yang sama.". • Invoice bukan `OPEN` → ditolak, tombol otomatis nonaktif di frontend. • Data invoice sudah berubah sejak dimuat → `409`, toast "Data sudah berubah" dan reload otomatis (pola yang sama seperti `FE-BKC-003`). |
| Hasil akhir | Invoice punya satu baris diskon baru berstatus Disetujui. Total tagihan **belum berubah** sampai petugas menekan Hitung Ulang. |

### Proses 2 — Mengajukan dan Menyetujui Diskon Jasa Dokter

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memberi potongan pada bagian jasa dokter suatu item, dengan persetujuan eksplisit dari dokter pemilik jasa tersebut sebagai kontrol atas pendapatannya sendiri. |
| Pelaku | Pengaju: Petugas Billing/Kasir (`BillingDiscount : Create`). Penyetuju: Dokter penanggung jawab kunjungan pasien tersebut (`BillingDoctorDiscount : Approve`) — bukan dokter lain, dan bukan pengaju yang sama. |
| Pemicu | Pengajuan: petugas memilih policy bertipe Diskon Dokter. Persetujuan: dokter menekan "Setujui" pada baris diskon yang menunggu dirinya. |
| Prasyarat pengajuan | Invoice `OPEN`. Item target memiliki komponen jasa dokter (`DoctorShare` > 0). Nominal diskon diisi manual oleh kasir dan wajib lebih besar dari nol. |
| Prasyarat persetujuan | Diskon berstatus "Menunggu Dokter" (bukan "Menunggu Finance" — kasus itu ditolak dan diarahkan ke alur exception Finance, di luar scope task ini). Penyetuju adalah dokter yang sama dengan dokter penanggung jawab kunjungan pasien pada invoice tersebut. Penyetuju bukan orang yang mengajukan diskon ini. |
| Langkah utama | **Pengajuan:** 1) Petugas membuka "Ajukan Diskon", memilih policy Diskon Dokter. 2) Memilih item yang punya jasa dokter. 3) Mengetik nominal diskon. 4) Mengisi alasan. 5) Submit. 6) Sistem memeriksa nominal terhadap sisa jasa dokter yang tersedia dan limit policy, lalu menetapkan status "Menunggu Dokter" atau "Menunggu Finance". **Persetujuan:** 7) Dokter membuka invoice yang sama, melihat baris diskon berstatus "Menunggu Dokter" dengan tombol "Setujui". 8) Dokter mengisi alasan persetujuan dan menekan "Ya, Setujui". 9) Status berubah menjadi "Disetujui". |
| Aturan bisnis | Total diskon dokter pada satu item (gabungan semua pengajuan, apa pun statusnya) tidak boleh melebihi `DoctorShare` item tersebut. Bila nominal yang diajukan melebihi limit yang ditentukan policy (bukan melebihi `DoctorShare`), pengajuan otomatis berstatus "Menunggu Finance", bukan "Menunggu Dokter" — dokter **tidak bisa** menyetujui kasus ini dari halaman ini. |
| Contoh konkret | Item "Konsultasi Dokter Spesialis" (jasa dokter Rp500.000, limit policy Rp300.000). Kasir mengajukan diskon Rp200.000 dengan alasan "Pasien BPJS PBI" → di bawah limit, status "Menunggu Dokter". Dokter penanggung jawab menyetujui dengan alasan "Sesuai kebijakan BPJS PBI" → status "Disetujui". Bila kasir kemudian mengajukan diskon tambahan Rp250.000 pada item yang sama, sistem menolak karena Rp200.000 + Rp250.000 = Rp450.000 melebihi Rp500.000 (di halaman, hint "sisa jasa dokter" menampilkan Rp300.000 sebelum submit, sebagai bantuan tampilan saja). |
| Perubahan status | `PENDING_DOCTOR` → `APPROVED` (oleh dokter pemilik share) atau tetap `PENDING_DOCTOR` bila ditolak validasi. `PENDING_FINANCE` tidak dapat diubah dari halaman ini. |
| Jalur tidak normal | • Dokter mencoba menyetujui pengajuan yang statusnya "Menunggu Finance" → tombol Setujui tidak ditampilkan sama sekali untuk status ini (hanya "Menunggu Dokter" yang punya tombol aksi). • Pengaju mencoba menyetujui pengajuannya sendiri → tombol Setujui nonaktif dengan keterangan "Pengaju tidak dapat menyetujui pengajuannya sendiri.", backend menolak independen dengan `403`. • Pengguna yang bukan dokter (tidak punya identitas dokter) mencoba menyetujui → tombol nonaktif dengan keterangan "Hanya dokter pemilik share yang dapat menyetujui.". Bila tetap dipaksa lewat API langsung, backend menolak `403` dengan pesan "Diskon jasa dokter hanya dapat disetujui oleh dokter pemilik share." — **frontend tidak dapat memverifikasi dokter yang benar-benar tepat** (encounter belum tentu milik dokter yang sedang login) tanpa endpoint tambahan; ini bergantung penuh pada otorisasi backend, dicatat sebagai batasan yang disengaja (lihat Temuan). |
| Hasil akhir | Diskon jasa dokter berstatus Disetujui, siap ikut dihitung saat invoice di-Hitung Ulang. Riwayat pengajuan dan persetujuan (siapa, kapan, alasan) tersimpan permanen di baris diskon. |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/discounts` | Mengajukan diskon (promo langsung efektif; diskon dokter menunggu approval) | `BillingDiscount : Create` | `ApplyDiscountRequest` (`discountPolicyId`, `invoiceItemId?`, `requestedAmount?`, `expectedRowVersion`, `reason`) | `ApiResponse<DiscountResponse>` |
| `POST` | `/{id}/discounts/{discountId}/approve` | Dokter menyetujui diskon jasa dokter miliknya | `BillingDoctorDiscount : Approve` | `ApproveDiscountRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<DiscountResponse>` |

Kedua endpoint ini sudah diimplementasikan backend sejak commit `22bf9cf` ("Add module backend
billing dan kasir part 2") — dokumen `contracts/api-contract.md` masih menandainya "Rencana
(belum tersedia)"; temuan yang sama seperti dilaporkan pada `FE-BKC-003`, tidak diulang di sini.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil. Baris diskon baru/terbaru langsung tampil di tabel Diskon. |
| `403` | Tidak berhak — pengaju mencoba menyetujui sendiri, atau penyetuju bukan dokter pemilik share. |
| `404` | Invoice, policy diskon, atau pengajuan diskon tidak ditemukan. |
| `409` | Data invoice sudah berubah sejak dimuat, atau policy yang sama sudah diterapkan pada target yang sama. Halaman menampilkan pesan dan memuat ulang otomatis. |
| `422` | Aturan bisnis tidak terpenuhi — contoh: item biaya administrasi, nominal melebihi jasa dokter, pengajuan tidak lagi menunggu dokter, atau nominal promo diisi manual padahal seharusnya dari policy. |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs:155-176` (endpoint `discounts` dan `discounts/{discountId}/approve`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingDiscountService.cs:25-121` (`ApplyAsync` — validasi admin fee, duplikasi, batas nominal terhadap gross item), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingDiscountService.cs:123-205` (`ApproveDoctorAsync` — self-approval forbidden, pencocokan dokter pemilik encounter, status `PENDING_FINANCE` tidak bisa disetujui dari sini), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingDiscountService.cs:239-299` (`ResolveApplicationAsync` — rumus nominal promo, batas jasa dokter, eskalasi ke Finance saat melebihi limit policy), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/MasterData/DTOs/DiscountPolicyDtos.cs:1-23` (`DiscountPolicyValues` — enum `PROMO_TOTAL`/`PROMO_ITEM`/`DOCTOR`), commit `22bf9cf` (master data dari part 1, enum tetap sama).
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` (thunk `applyBillingDiscount`, `approveBillingDoctorDiscount`, merge hasil ke `invoiceDetail.discounts`) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-apply-billing-discount.js` (form pengajuan diskon, field dinamis per tipe policy, hint sisa jasa dokter) — belum di-commit, file baru.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoice-detail.js` (alur persetujuan dokter, identitas `userId`/`doctorId` dari cookie sesi) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/apply-discount-modal.jsx` (modal form) — belum di-commit, file baru.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx` (kolom Aksi/Alasan pada tabel Diskon, tombol "+ Ajukan Diskon") — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-004`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Admin fee tidak selectable | **Sebagian** | Untuk tipe Diskon Dokter, filter `doctorShare > 0` otomatis menyingkirkan item admin fee (nilainya selalu 0). Untuk Promo Item, **tidak difilter di frontend** — kategori item tidak tersedia di response invoice tanpa endpoint master data tambahan (`BillingItemCategory`). Backend tetap menolak dengan pesan jelas. Simplifikasi disengaja, lihat Temuan. |
| Master promo tanpa approval | **Terpenuhi** | `ResolveApplicationAsync` menetapkan status `APPROVED` langsung untuk `PROMO_TOTAL`/`PROMO_ITEM`; frontend tidak menampilkan alur approval untuk tipe ini. |
| Doctor approval actor benar | **Terpenuhi (source), bergantung backend** | Tombol Setujui disembunyikan untuk pengaju sendiri dan untuk pengguna tanpa identitas dokter (cookie `doctorId`). Kecocokan dokter-vs-encounter yang sebenarnya sepenuhnya diverifikasi backend (`ApproveDoctorAsync`) karena frontend tidak punya data itu. |
| Finance exception terlihat | **Terpenuhi** | Status "Menunggu Finance" tampil sebagai badge terpisah di tabel Diskon; tidak ada tombol aksi untuk status ini di halaman ini (sesuai scope — alur approval Finance adalah `FE-BKC-008`). |
| Component/security tests | **Belum ada** | Sama seperti `FE-BKC-003`, belum ditulis unit/component test baru. |

## Temuan (bukan diperbaiki task ini — di luar wewenang tulis)

1. `contracts/api-contract.md` masih menandai endpoint discount sebagai "Rencana (belum
   tersedia)" — sama seperti temuan `FE-BKC-003`, belum diperbarui pemilik kontrak.
2. Frontend tidak dapat memverifikasi item mana yang berkategori biaya administrasi untuk tipe
   Promo Item (lihat Proses 1). Master data `BillingItemCategory` (existing, direuse) punya
   endpoint `GET /options` dengan field `isAdministrationFee`, tapi belum ada service/hook
   frontend yang mengonsumsinya di modul mana pun. Menambahkannya khusus untuk satu filter UI
   dianggap di luar proporsi task ini; backend tetap menjadi jaring pengaman.
3. Frontend tidak dapat memverifikasi dokter penanggung jawab encounter yang sebenarnya sebelum
   submit approve (lihat Proses 2) — `InvoiceDetailResponse` tidak membawa field dokter encounter.
   Backend adalah satu-satunya penegak aturan ini; UI hanya menyembunyikan tombol untuk kasus yang
   jelas salah (pengaju sendiri, bukan akun dokter).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit code 0, tanpa output, seluruh repo. |
| Lint severity penuh pada 6 file yang diubah/baru | **PASS** | `npx eslint <6 file>` tanpa `--quiet` → nol error, nol warning. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 84s`, termasuk route invoice detail yang diubah. `postbuild` sukses. |
| `npm run test:unit` | **PASS (harness); 1 kegagalan pre-existing tidak terkait** | 34 test, 33 lulus, 1 gagal (`auth-security.test.mjs`, sama seperti dilaporkan di `FE-BKC-003`, `ISSUE-FE-005`). Tidak ada test yang menguji kode diskon. |
| Smoke-test browser headless tanpa login | **PARTIAL PASS** | Route detail invoice (kini memuat modal diskon, kolom aksi baru) tetap 0 exception JS saat mount, redirect ke `/login` bersih setelah `401`. |
| Verifikasi manual ter-autentikasi (ajukan diskon, dokter approve) | **NOT DONE** | Sama seperti `FE-BKC-003` — tidak ada kredensial. Dev server masih berjalan di `localhost:3000`. |

**Task ini belum bisa ditandai selesai sepenuhnya** — `lint`, `build`, dan harness `test:unit`
lulus bersih, tapi klik-coba langsung (ajukan diskon promo, ajukan diskon dokter, approve oleh
akun dokter, kasus melebihi limit ke Finance) belum dijalankan dengan data dan akun sungguhan.

## Langkah berikutnya yang direkomendasikan

1. Login di `localhost:3000` dengan akun Billing (`BillingDiscount:Create`) dan akun dokter
   (`BillingDoctorDiscount:Approve`) pada encounter yang sama, lalu verifikasi manual kelima
   acceptance criteria di atas — termasuk kasus nominal melebihi limit policy (harus jadi
   "Menunggu Finance", bukan "Menunggu Dokter").
2. Tambahkan unit/component test untuk `use-apply-billing-discount.js` (logika field dinamis per
   tipe policy dan perhitungan hint sisa jasa dokter).
3. Setelah verifikasi manual dan test selesai, lanjut ke `FE-BKC-005` (deposit), `FE-BKC-008`
   (financial exception workbench — pasangan alami untuk kasus "Menunggu Finance" di atas), atau
   navigasi menu part 1 yang masih tertunda.
