# FE-BKC-021 — Layar Pengecualian Finansial untuk Selisih yang Tidak Dapat Ditagihkan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-021` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, perluasan alur yang sudah ada (bukan layar baru — reuse penuh `FE-BKC-008`) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API — field sudah dikirim `BE-BKC-029`) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-029` — `DONE`, `dotnet build`/`test` dikonfirmasi lulus pengguna 6 September 2026. Lihat `requirement-traceability.md` § amendment 4 September 2026 |
| Status task | Source selesai **untuk acceptance 1–6**. Acceptance 7 **TIDAK diimplementasikan** — diblokir `BKC-GAP-01` (lihat § Scope yang dikecualikan). **`npm run lint:errors`/`test:unit`/`build` dijalankan dan LULUS.** Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Sejak `BE-BKC-028`/`030`, satu invoice bisa punya "selisih tidak dapat ditagihkan" — nominal yang
kontrak penjamin larang dibebankan ke pasien maupun ke asuransi. `BE-BKC-029` menambahkan kategori
`NON_BILLABLE_RESIDUAL` pada write-off khusus untuk menuntaskan selisih ini (terpisah dari kategori
lama `PATIENT_AR` yang menuntaskan piutang pasien biasa), dengan plafon yang sama sekali berbeda:
dibatasi sisa selisihnya sendiri, **bukan** saldo tagihan pasien. Sampai task ini, layar Pengecualian
Finansial (hasil `FE-BKC-008`) sama sekali tidak tahu field ini ada — petugas keuangan tidak bisa
melihat berapa selisih yang belum ditanggung, dan formulir pengajuan write-off tidak punya cara
memilih kategori maupun tahu nominal berapa yang wajar diajukan.

Task ini murni menyambungkan field yang **sudah** dikirim backend ke layar yang **sudah** ada:

1. Panel Pengecualian Finansial sekarang menampilkan "Selisih tidak dapat ditagihkan yang belum
   ditanggung: Rp 60.000" (atau keterangan biru bahwa tidak ada selisih, bila nol) — dibaca langsung
   dari server, tidak dijumlahkan ulang dari daftar case.
2. Formulir pengajuan write-off mendapat pemilih kategori (Piutang Pasien / Selisih Tidak Dapat
   Ditagihkan). Bila ada selisih yang belum ditanggung, kategori dan nominal **sudah terisi** begitu
   formulir dibuka — petugas tinggal menuliskan alasan dan mengirim.
3. Tabel daftar case menampilkan kolom Kategori untuk baris write-off, supaya atasan yang menyetujui
   tahu plafon mana yang sedang diperiksa.

Nominal yang melebihi plafon (baik piutang pasien maupun selisih) tetap ditolak **oleh server**,
dengan pesan yang sudah menyebut plafon yang relevan — layar ini **tidak** menduplikasi perhitungan
plafon di sisi klien, persis peringatan eksplisit pada `frontend-roadmap.md`: menghitung ulang plafon
di layar akan menyimpang dari yang dijaga server dan membuat penolakan terlihat tanpa alasan.

## Keputusan yang mengunci scope (ringkas)

Trace `BKC-DES-023`, `BKC-DES-024`; kontrak `BIL-API-0.7` (`nonBillableResidualRemaining` pada `GET
.../financial-exceptions/invoices/{id}`; `category` pada `CreateWriteOffRequest`/`WriteOffResponse`)
— `approved`, dikirim backend sejak `BE-BKC-029`. `roadmap/frontend-roadmap.md` § `FE-BKC-021`.

**Keputusan desain kecil, dibuat pada task ini sendiri, didokumentasikan di sini:**

1. **Satu tombol "+ Ajukan Write-off", bukan tombol terpisah untuk selisih.** Alur bisnis (§ proses
   bisnis roadmap, langkah 5) menggambarkan satu tombol pengajuan yang, ketika ditekan, formulirnya
   sudah terisi. Menambah tombol kedua ("Ajukan Write-off atas Selisih") akan menduplikasi alur yang
   sudah ada di `FE-BKC-008` — bertentangan dengan instruksi reuse eksplisit pada roadmap ("beserta
   seluruh alur pengajuan... apa adanya"). Tombol yang sama membuka formulir yang sama; hanya nilai
   awalnya yang bercabang mengikuti ada/tidaknya selisih (lihat `buildWriteOffForm` pada hook).
2. **`IsFullSettlement` sengaja tidak diberi elemen UI apa pun.** Field ini hanya relevan untuk satu
   validasi backend (`BIL-VAL-041`: kategori `NON_BILLABLE_RESIDUAL` yang ditandai pelunasan penuh
   ditolak) dan untuk kategori `PATIENT_AR` sama sekali diabaikan server (nilainya diturunkan ulang
   dari outstanding saat approval). Tidak ada alasan sah bagi layar mengirim `true` untuk kategori
   manapun — field ini tidak pernah dikirim sama sekali dari sini (bawaan server: `false`), yang
   sekaligus memenuhi acceptance 5 secara konstruktif: kombinasi yang ditolak backend memang tidak
   pernah bisa dibentuk dari layar ini.
3. **Mengganti kategori pada formulir mengosongkan nominal.** Bukan diminta acceptance criteria
   secara eksplisit, tapi konsekuensi langsung dari menambahkan pemilih kategori: kedua kategori
   punya plafon berbeda sama sekali, mempertahankan nominal lama lintas kategori akan membuat
   petugas mengirim nominal yang dibatasi plafon yang salah. Perilaku ini bagian dari menambahkan
   kategori dengan aman, bukan scope tambahan yang berdiri sendiri.
4. **Kolom "Kategori" ditambahkan pada tabel daftar case** (khusus baris write-off). Tidak diminta
   eksplisit oleh acceptance criteria, tapi langsung melayani proses bisnis langkah 5(e): "atasan
   membaca dan menyetujui" — atasan tidak dapat memeriksa pengajuan dengan bermakna tanpa tahu
   kategori mana yang sedang diperiksa, mengingat plafonnya berbeda sama sekali.

## Scope yang dikecualikan — acceptance 7

**Acceptance criterion 7 ("Layar finalisasi menampilkan peringatan bila masih ada selisih yang belum
ditanggung, tetapi tidak memblokir finalisasi") TIDAK diimplementasikan pada task ini.**

`requirement-traceability.md` § `BKC-GAP-01` mencatat eksplisit: keputusan bisnisnya (`BKC-DEC-090`)
sudah `approved`, tetapi **desainnya belum ditulis**, dan baris yang sama menyatakan tegas bahwa
`FE-BKC-021` acceptance 7 "belum punya rujukan desain". Bagian 4 dokumen yang sama menjelaskan
kenapa ini disengaja: *"Membuatkan task untuk keputusan yang desainnya belum ada berarti pelaksana
yang akan merancangnya sambil menulis kode, dan itu persis cara keputusan bisnis terbentuk tanpa
pemiliknya."* Mengarang letak/isi/kondisi tampil peringatan ini di layar finalisasi tanpa desain
yang disetujui berarti melanggar instruksi eksplisit itu — build-module-frontend § langkah 1 juga
mengharuskan task berhenti bila masukan yang dibutuhkan belum disetujui, dan desain acceptance 7
justru berstatus demikian.

**Tidak ada kode ditulis untuk acceptance 7.** `BillingFinalizationPanel`/`FinalizeInvoiceModal`
tidak disentuh sama sekali oleh task ini. Acceptance 1–6 diimplementasikan penuh; acceptance 7 tetap
tercatat sebagai gap terbuka (`BKC-GAP-01`), menunggu desainnya ditulis pemilik blueprint.

## Proses bisnis

| Aspek | Penjelasan |
| --- | --- |
| Pemicu | Petugas keuangan membuka Menu Pembayaran, melihat panel Pengecualian Finansial pada tagihan yang memuat selisih |
| Aturan bisnis | Nominal write-off kategori `NON_BILLABLE_RESIDUAL` dibatasi sisa selisihnya sendiri, **bukan** sisa tagihan pasien; kategori `PATIENT_AR` tidak berubah perilakunya sama sekali |
| Perubahan status | Case write-off berpindah `SUBMITTED` → `POSTED` saat disetujui. Status tagihan **tidak berpindah** — tidak disentuh task ini, sudah demikian sejak `FE-BKC-008` |
| Jalur tidak normal | Pengajuan melebihi plafon (baik kategori) ditolak **oleh server**; pesannya menyebut plafon yang relevan (bukan digeneralisasi) dan disurfacekan lewat `actionError`/`InformationAlert` yang sudah ada — tidak ada penanganan galat baru yang perlu ditulis |
| Hasil akhir | Sisa selisih menjadi nol setelah write-off `NON_BILLABLE_RESIDUAL` disetujui; sisa tagihan pasien (dibaca terpisah di blok Ringkasan Pembayaran) **tetap seperti semula** — kedua angka berasal dari sumber server yang berbeda, task ini tidak mencampurnya |

## Base Component Decision Gate

`UI GATE: 1 elemen baru — REUSE mayoritas`

- Pemilih kategori memakai elemen `<select>` polos — pola **identik** dengan pemilih "Arah" pada
  `CreateAdjustmentModal` yang sudah ada (bukan komponen baru, hanya opsi baru).
- Tidak ada modal, tombol, atau alur baru — `CreateWriteOffModal`, `BillingFinancialExceptionPanel`,
  dan `useBillingFinancialException` seluruhnya komponen/hook yang **sudah ada** sejak `FE-BKC-008`,
  hanya diperluas propsnya.
- Satu kolom tabel baru ("Kategori") pada `DataTable` yang sudah ada — pola sama dengan kolom
  "Arah" yang sudah ada di tabel yang sama (render kondisional per `kind`).

## Endpoint yang dikonsumsi

Tidak ada endpoint baru. Task ini membaca/mengirim field tambahan pada endpoint yang **sudah**
dikonsumsi `use-billing-financial-exception.js` sejak `FE-BKC-008`:

| Field | Endpoint | Arah | Sejak task backend |
| --- | --- | --- | --- |
| `nonBillableResidualRemaining` | `GET .../financial-exceptions/invoices/{invoiceId}` | Baca | `BE-BKC-029` |
| `category` (request) | `POST .../financial-exceptions/write-offs` | Kirim | `BE-BKC-029` |
| `category` (response) | `POST .../financial-exceptions/write-offs` (dan replay-nya) | Baca | `BE-BKC-029` |

`isFullSettlement` pada `CreateWriteOffRequest` **tidak pernah dikirim** dari layar ini (lihat §
Keputusan butir 2) — bergantung pada bawaan server (`false`).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../billing-financial-exception-slice.jsx` | `createBillingWriteOff` mengirim `category`; state baru `nonBillableResidualRemaining` + normalizer + selector, diisi dari response `getFinancialExceptionsByInvoice` yang sama dengan daftar case |
| `src/lib/hooks/.../use-billing-financial-exception.js` | `buildWriteOffForm` menerima nilai residual (kategori+nominal bercabang mengikuti ada/tidaknya selisih); `openWriteOff` membaca `nonBillableResidualRemaining` terkini; `confirmWriteOff` mengirim `category`; `handleWriteOffChange` mengosongkan nominal saat kategori diganti manual; `normalizeCaseRow` (write-off) ikut membaca `category`; `nonBillableResidualRemaining` diekspor |
| `src/lib/hooks/.../billing-financial-exception-constants.js` | `WRITE_OFF_CATEGORY_OPTIONS` baru (dua kategori tetap dari backend) |
| `src/components/view/.../detail/create-write-off-modal.jsx` | Pemilih kategori baru (pola identik `CreateAdjustmentModal`); teks `InformationAlert` diperbarui menyebut plafon bercabang kategori |
| `src/components/view/.../detail/billing-financial-exception-panel.jsx` | `InformationAlert` baru menampilkan sisa selisih (atau keterangan tidak ada selisih); kolom tabel "Kategori" baru untuk baris write-off |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | Satu prop baru diteruskan ke `BillingFinancialExceptionPanel` (`nonBillableResidualRemaining`) |

Total: **6 berkas berubah**, tidak ada berkas baru.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit 0, `eslint . --quiet` tanpa output |
| `npm run test:unit` | **PASS** | 440/440 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru (perluasan hook/komponen murni, tidak ada logika bercabang cukup kompleks untuk butuh test unit terpisah) |
| `npm run build` | **PASS** | `✓ Compiled successfully in 74s`; `postbuild`/`prepare-standalone.mjs` sukses; tidak ada error/warning baru |
| Verifikasi statis: acceptance 1 (sisa selisih dari server, tidak dihitung ulang) | **LULUS** | `nonBillableResidualRemaining` dibaca langsung dari field response yang sama dengan daftar case (`normalizeNonBillableResidualRemaining`), bukan dijumlahkan dari `trackedCases`/`writeOffs` |
| Verifikasi statis: acceptance 2 (kategori + nominal terisi awal) | **LULUS** | `buildWriteOffForm(nonBillableResidualRemaining)` — kategori `NON_BILLABLE_RESIDUAL` dan nominal `String(residual)` saat residual > 0 |
| Verifikasi statis: acceptance 3 (penolakan melebihi plafon menyebut selisihnya, bukan tagihan pasien) | **SUDAH TERPENUHI backend** | `BillingFinancialExceptionService.CreateWriteOffAsync` (baris ~300–309 di backend): pesan kategori `NON_BILLABLE_RESIDUAL` eksplisit "melebihi selisih yang tidak dapat ditagihkan", terpisah dari pesan `PATIENT_AR` ("melebihi saldo outstanding"). Layar hanya mem-passthrough pesan itu lewat `actionError`/`fieldErrors` yang sudah ada — tidak ada logika baru yang perlu diverifikasi di sisi layar |
| Verifikasi statis: acceptance 4 (pengaju tidak dapat menyetujui sendiri) | **SUDAH TERPENUHI sebelum task ini** | `canApprove`/`disabled={isSelfRequested}` pada panel sudah ada sejak `FE-BKC-008`, tidak disentuh task ini — grep konfirmasi tidak ada perubahan pada blok itu |
| Verifikasi statis: acceptance 5 (kategori selisih ditandai pelunasan penuh ditolak) | **LULUS (dengan konstruksi, lihat § Keputusan butir 2)** | Grep `isFullSettlement`/`IsFullSettlement` pada seluruh berkas frontend yang diubah: nol kecocokan — field itu tidak pernah dikirim, sehingga kombinasi yang ditolak backend tidak pernah bisa terbentuk dari layar ini |
| Verifikasi statis: acceptance 6 (sisa tagihan pasien & status tagihan tidak berubah) | **SUDAH TERPENUHI backend+existing** | Approval write-off (`ApproveWriteOffAsync`) tidak pernah menulis `BilInvoice.Status`; `refreshAfterMutation()` pada hook hanya me-refresh daftar case/kredit, tidak menyentuh state kalkulasi/Ringkasan Pembayaran yang terpisah |
| Verifikasi statis: acceptance 7 (peringatan finalisasi, tidak memblokir) | **TIDAK DIIMPLEMENTASIKAN — diblokir `BKC-GAP-01`** | Lihat § Scope yang dikecualikan |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini |
| Verifikasi manual ter-autentikasi (tagihan dengan selisih nyata, pengajuan write-off kedua kategori, approval oleh pengguna kedua) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder; membutuhkan data tagihan dengan `NonBillableResidualAmount` > 0 dan dua akun pengguna berbeda untuk maker-checker |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test/build lulus bersih dan acceptance
1–6 terverifikasi secara statis/tinjauan kode, tetapi ini **bukan** pengganti verifikasi manual
ter-autentikasi dengan data nyata (per instruksi `build-module-frontend`: "Lint/test/build yang PASS
saja bukan bukti"). Acceptance 7 sengaja tidak dikerjakan — lihat § Scope yang dikecualikan.

## Risiko yang tersisa

1. **Belum diverifikasi dengan data selisih nyata.** Seluruh bukti acceptance 1–6 adalah tinjauan
   kode/statis. Sebelum ditandai selesai, satu tagihan dengan `NonBillableResidualAmount` > 0
   **wajib** diperiksa langsung di browser — termasuk mengonfirmasi pesan penolakan server saat
   nominal melebihi plafon benar-benar menyebut selisihnya, bukan tagihan pasien.
2. **Acceptance 7 tertunda oleh gap desain, bukan oleh keterbatasan teknis.** `BKC-GAP-01` perlu
   pass desain tersendiri (pemilik blueprint) sebelum bisa dikerjakan — lihat § Scope yang
   dikecualikan. Task lanjutan untuk acceptance 7 sebaiknya menunggu desain itu, bukan menebaknya.
3. **Keputusan tidak menambah tombol kedua untuk pengajuan selisih (§ Keputusan butir 1) adalah
   judgment call.** Bila produk menginginkan jalur yang lebih eksplisit/terpisah secara visual dari
   write-off piutang pasien biasa, itu perubahan desain terpisah — dicatat di sini supaya tidak
   disalahartikan sebagai kelalaian.
4. **Temuan di luar scope, tidak diperbaiki di sini (diwariskan dari `FE-BKC-019`/`FE-BKC-020`).**
   `getItemCoverageStatus` pada Menu Pembayaran masih belum mengenali `nonBillableResidual` untuk
   badge per baris item — tidak tersentuh oleh task ini (berkas berbeda, fungsi berbeda), tetap
   direkomendasikan sebagai task tersendiri.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran Finance/AR dan peran atasan (dua akun berbeda untuk maker-checker), buka
   tagihan dengan selisih tidak dapat ditagihkan nyata, verifikasi: nominal sisa selisih tampil
   benar, formulir pengajuan write-off terisi awal (kategori + nominal), pengajuan melebihi plafon
   ditolak dengan pesan yang menyebut selisihnya, dan sesudah disetujui sisa tagihan pasien di blok
   Ringkasan Pembayaran tidak berubah.
2. Verifikasi tagihan tanpa selisih (residual = 0) — pastikan keterangan biru "tidak ada selisih"
   tampil dan formulir write-off kembali ke bawaan lama (kategori Piutang Pasien, nominal kosong).
3. Ajukan pass desain untuk `BKC-GAP-01` (`BKC-DEC-090` — peringatan finalisasi) ke pemilik
   blueprint, lalu lanjutkan acceptance 7 sebagai task terpisah setelah desainnya ada.
4. Pertimbangkan task perbaikan terpisah untuk `getItemCoverageStatus` (§ Risiko butir 4) — di luar
   scope task ini maupun `FE-BKC-019`/`FE-BKC-020`.
