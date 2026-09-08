# FE-BKC-020 — Peringatan Anomali Data dan Penanda per Baris

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-020` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, penambahan tampilan pada komponen yang sudah ada (bukan layar baru) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API — field sudah dikirim `BE-BKC-025`) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-025` — `DONE`, `dotnet build`/`test` dikonfirmasi lulus pengguna. Lihat `requirement-traceability.md` § amendment 4 September 2026 |
| Status task | Source selesai. **`npm run lint:errors`/`test:unit`/`build` dijalankan dan LULUS.** Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Sejak `BE-BKC-025`, satu invoice bisa punya keputusan coverage berstatus **anomali data** — bukan
karena rule asuransinya menolak, tapi karena data pendaftarannya sendiri bermasalah (penjamin
tidak eligible, polis tidak aktif, provider asuransi tidak diisi, atau kunjungan tidak ditemukan).
Pada kasus ini seluruh nominal item yang bisa ditagihkan (`BillingCoverageAdapter.Anomaly()`)
sudah jatuh ke pasien **sementara**, sambil menunggu petugas Pendaftaran membetulkan datanya —
tapi sampai task ini, layar Menu Pembayaran sama sekali tidak memberi tahu kasir bahwa nominal itu
statusnya masih menggantung. Kasir melihat tagihan yang tampak normal (badge "Tunai" di semua
baris, tidak ada peringatan apa pun), padahal sebagian nominalnya bisa berubah begitu data
pendaftaran diperbaiki dan rule asuransi yang sebenarnya bisa dievaluasi ulang.

Task ini menambahkan dua hal, keduanya murni presentasi dari field yang **sudah** dikirim backend:

1. **Peringatan kuning** di atas Ringkasan Pembayaran, satu per kalimat dari
   `breakdown.coverage.anomalyMessages` — teksnya persis dari server, layar tidak mengarang
   kalimatnya sendiri.
2. **Badge "Anomali Data"** pada baris item yang terdampak (menggantikan "Tunai" yang menyesatkan),
   dengan urutan prioritas status per baris yang sekarang memeriksa anomali data **lebih dulu**
   daripada "Menunggu Verifikasi"/"Penjamin"/"Tunai".

Nominal anomali (`dataAnomalyAmount`) **tidak** ditambahkan sebagai baris subtotal baru mana pun di
Ringkasan Pembayaran — sesuai `BKC-DES-011`, nominal itu sudah melebur ke `Harus Dibayar Pasien`
sejak sebelumnya, memisahkannya jadi baris sendiri justru akan membuat total menjumlah dua kali.

## Keputusan yang mengunci scope (ringkas)

Trace `BKC-DES-010`, `BKC-DES-011`; kontrak `BIL-API-0.6` (`hasDataAnomaly`, `anomalyCodes`,
`anomalyMessages`, `dataAnomalyAmount` pada `breakdown.coverage`; `itemDataAnomalyAmount`/
`taxDataAnomalyAmount` per item; `dataAnomalyAmount` pada `administrationFee`/`roomCharge`) —
`approved`, dikirim backend sejak `BE-BKC-025`. `roadmap/frontend-roadmap.md` § `FE-BKC-020`.

Tidak ada keputusan bisnis baru yang perlu ditutup pada task ini. Satu keputusan desain kecil,
di luar dokumen governance yang ada, dibuat pada task ini sendiri dan didokumentasikan di sini:

**Warna badge "Anomali Data".** Sistem status badge (`status-badge.css`) hanya punya empat kelas
warna (`active`=hijau, `warning`=kuning, `pending`=biru, `inactive`=merah), dan dua di antaranya
sudah dipakai status lain (`warning`→"Penjamin", `pending`→"Menunggu Verifikasi"). `inactive`
(merah) tidak dipakai karena `BKC-DES-011` eksplisit melarang anomali disajikan sebagai galat
merah — larangan itu ditujukan ke peringatan di atas, tapi semangatnya (anomali data bukan
kegagalan/kesalahan, hanya status yang menunggu tindak lanjut) berlaku sama untuk badge baris.
Dipilih **reuse `region-status-pending`** (biru, sama dengan "Menunggu Verifikasi") — keduanya
memang satu kategori semantik yang sama ("belum pasti sampai ditindaklanjuti pihak lain"), warna
yang sama tidak membingungkan karena **teks label tetap berbeda** ("Anomali Data" vs "Menunggu
Verifikasi"), yang juga sekaligus memenuhi acceptance criterion "tidak disampaikan lewat warna
saja". Tidak menambah kelas CSS baru — sesuai `AGENTS.md`, "jangan menambahkan pola/elemen baru
hanya karena terlihat lebih modern" bila yang ada masih bisa dipakai.

## Proses bisnis

| Aspek | Penjelasan |
| --- | --- |
| Pemicu | Kasir membuka Menu Pembayaran satu invoice — peringatan dan badge tampil otomatis begitu kalkulasi memuat `hasDataAnomaly`/`anomalyMessages`, tidak ada aksi tambahan dari kasir |
| Aturan bisnis | Peringatan hanya tampil bila server benar-benar mengirim `anomalyMessages` (array kosong/absen → tidak ada peringatan sama sekali, bukan peringatan kosong). Badge "Anomali Data" hanya pada item yang porsi item/pajaknya (`itemDataAnomalyAmount`/`taxDataAnomalyAmount`) > 0 |
| Perubahan status | Tidak ada — task ini murni presentasi, tidak menulis apa pun |
| Jalur tidak normal | Tagihan tunai murni atau tagihan yang seluruhnya tercover normal (tidak ada anomali sama sekali): tidak ada peringatan yang dirender, dan seluruh badge tetap "Tunai"/"Penjamin" seperti sebelumnya — behavior ini tidak disentuh oleh task ini (lihat § Definition of Done, acceptance 5) |
| Hasil akhir | Kasir melihat kalimat penyebab anomali persis dari server di atas Ringkasan Pembayaran, dan tahu persis baris item mana yang nominalnya masih berstatus sementara |

**Kenapa anomali data diperiksa sebelum "Menunggu Verifikasi" di `getItemCoverageStatus`, walau
keduanya secara bisnis tidak akan pernah tumpang tindih pada baris yang sama.** Satu invoice hanya
bisa punya SATU jenis keputusan coverage — `BillingCoverageAdapter.Anomaly()` mengembalikan seluruh
keputusan invoice itu sebagai anomali (`ItemDataAnomalyAmount`/`TaxDataAnomalyAmount` terisi,
`ItemUnresolvedAmount`/`TaxUnresolvedAmount` selalu nol), sedangkan jalur rule normal yang
menghasilkan unresolved tidak pernah mengisi field anomali. Jadi `dataAnomalyTotal > 0` dan
`unresolvedTotal > 0` secara matematis tidak pernah keduanya benar pada satu baris — urutan
pemeriksaan tidak mengubah hasil apa pun, tapi ditulis eksplisit lebih dulu supaya pembaca kode
melihat urutan sebab-akibatnya (anomali data → menunggu verifikasi → penjamin → tunai) tanpa perlu
menelusuri backend untuk tahu kenapa urutannya aman.

## Base Component Decision Gate

`UI GATE: 0 elemen baru — REUSE penuh`

- Peringatan memakai `InformationAlert` yang sudah ada, `variant="warning"` — pola yang sama persis
  dipakai `paymentBlockedReason` di berkas yang sama.
- Badge memakai `StatusBadge`/`BILLING_ITEM_COVERAGE_BADGE_CONFIG` yang sudah ada
  (`FE-BKC-FIX-006`/`FIX-008`) — hanya menambah satu entry baru pada config, tidak mengubah
  komponennya.
- Tidak ada kelas CSS baru (lihat § Keputusan di atas).

## Endpoint yang dikonsumsi

Tidak ada endpoint baru. Task ini membaca field tambahan dari response `GET
.../calculation-preview`/`POST .../recalculate` yang **sudah** dikonsumsi `menu-pembayaran-view.jsx`
sejak sebelumnya:

| Field | Level | Sejak task backend |
| --- | --- | --- |
| `breakdown.coverage.anomalyMessages` | Invoice (breakdown) | `BE-BKC-025` |
| `breakdown.items[].itemDataAnomalyAmount`, `.taxDataAnomalyAmount` | Per item | `BE-BKC-025` |

`breakdown.coverage.hasDataAnomaly`/`anomalyCodes` dan `dataAnomalyAmount` pada
`administrationFee`/`roomCharge` **tidak** dibaca layar ini — `anomalyMessages` sudah cukup untuk
peringatan (kalimatnya sudah final dari server, tidak perlu di-switch dari kode), dan biaya
administrasi/kamar tidak punya baris/badge per komponen di tabel item (hanya item yang punya baris
sendiri di UI ini).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | `itemOutcomeById` menyimpan dua field baru (`itemDataAnomaly`, `taxDataAnomaly`); `getItemCoverageStatus` menambah cabang `"anomali_data"` sebelum cabang `"belum_terverifikasi"`; variabel baru `dataAnomalyMessages` (dibaca dari `breakdown.coverage.anomalyMessages`); satu blok render baru — `InformationAlert variant="warning"` per kalimat, ditaruh sebagai child pertama di dalam blok invoice-loaded (sebelum kartu pasien, sehingga selalu di atas Ringkasan Pembayaran) |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | `BILLING_ITEM_COVERAGE_BADGE_CONFIG` menambah entry `anomali_data: { label: "Anomali Data", className: "region-status-pending" }` |

Total: **2 berkas berubah**.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit 0, `eslint . --quiet` tanpa output |
| `npm run test:unit` | **PASS** | 440/440 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru (penambahan presentasi murni, tidak ada logika bercabang yang cukup kompleks untuk butuh test unit terpisah) |
| `npm run build` | **PASS** | Build Next.js selesai tanpa error/warning baru; `postbuild`/`prepare-standalone.mjs` sukses |
| Verifikasi statis: acceptance 1 (peringatan di atas Ringkasan Pembayaran) | **LULUS** | Blok `dataAnomalyMessages.map(...)` ditaruh sebagai child pertama di dalam `{!loading && invoice ? (<>...`, sebelum kartu pasien dan seluruh isi lain — struktural selalu di atas |
| Verifikasi statis: acceptance 2 (kalimat dari server, bukan dikarang layar) | **LULUS** | `message={message}` merender string `anomalyMessages[i]` apa adanya, tidak ada template/formatting tambahan pada teksnya |
| Verifikasi statis: acceptance 3 (nominal anomali tidak jadi baris subtotal) | **LULUS** | Grep `dataAnomalyAmount` pada berkas ini sesudah edit: nol kecocokan di luar komentar — tidak ada `<dl>`/`<dd>` baru yang merender nominal itu |
| Verifikasi statis: acceptance 4 (tidak disampaikan lewat warna saja) | **LULUS** | `InformationAlert` selalu merender teks penuh terlepas dari `variant`; badge "Anomali Data" punya label teks sendiri yang berbeda dari "Menunggu Verifikasi" walau warnanya sama (lihat § Keputusan) |
| Verifikasi statis: acceptance 5 (tagihan tunai/tercover normal tidak berubah) | **LULUS (tinjauan kode)** | `dataAnomalyMessages` kosong → tidak ada `InformationAlert` yang dirender (`.map` atas array kosong); `getItemCoverageStatus`: cabang baru hanya aktif bila `dataAnomalyTotal > 0`, kalau tidak fallthrough ke logika lama yang tidak diubah sama sekali |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini |
| Verifikasi manual ter-autentikasi (tagihan dengan anomali data nyata — payer tidak eligible, polis tidak aktif, dst.) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder; membutuhkan data pendaftaran yang sengaja dibuat bermasalah untuk memicu `BillingCoverageAdapter.Anomaly()` |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test/build lulus bersih dan seluruh
lima acceptance criteria terverifikasi secara statis/tinjauan kode, tetapi ini **bukan** pengganti
verifikasi manual ter-autentikasi dengan data nyata (per instruksi `build-module-frontend`:
"Lint/test/build yang PASS saja bukan bukti").

## Risiko yang tersisa

1. **Belum diverifikasi dengan data anomali nyata.** Seluruh bukti acceptance criteria pada task
   ini adalah tinjauan kode/statis, bukan observasi runtime terhadap invoice yang benar-benar
   memicu `Anomaly()` di backend. Sebelum ditandai selesai, satu contoh invoice dengan penjamin
   tidak eligible atau polis tidak aktif **wajib** diperiksa langsung di browser.
2. **Keputusan warna badge (biru, reuse "pending") adalah judgment call, bukan spesifikasi
   eksplisit.** Palet status badge saat ini tidak punya warna kelima yang netral/informational
   selain yang sudah dipakai `pending`; dicatat di § Keputusan bila desainer produk ingin
   memisahkannya lebih jelas nanti (misalnya menambah kelas CSS baru), itu perubahan desain sistem
   terpisah, bukan bug pada task ini.
3. **Temuan di luar scope, tidak diperbaiki di sini (dicatat sesuai `AGENTS.md`, diwariskan dari
   `FE-BKC-019`).** `getItemCoverageStatus` masih belum mengenali `nonBillableResidual` (item yang
   seluruh nilainya masuk jalur non-billable-residual salah dilabeli "Tunai") — gap ini **tidak**
   diperbaiki secara oportunis pada task ini walau fungsi yang sama disentuh untuk menambah cabang
   anomali data, sesuai disiplin scope `AGENTS.md` ("jangan melakukan cleanup yang tidak terkait").
   Tetap direkomendasikan sebagai task/perbaikan tersendiri.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya akses Menu Pembayaran, buka tagihan dengan data pendaftaran yang
   sengaja dibuat bermasalah (payer tidak eligible, polis tidak aktif, provider asuransi kosong,
   atau kunjungan tidak ditemukan), verifikasi: peringatan kuning muncul di atas Ringkasan
   Pembayaran dengan kalimat yang sesuai penyebabnya, badge "Anomali Data" muncul pada baris item
   yang terdampak, dan tidak ada baris subtotal baru untuk nominal anomali.
2. Verifikasi tagihan tunai murni dan tagihan tercover normal (tanpa anomali) — pastikan tidak ada
   peringatan yang muncul dan seluruh badge tetap seperti sebelum task ini.
3. Pertimbangkan task perbaikan terpisah untuk `getItemCoverageStatus` (§ Risiko butir 3, gap
   `nonBillableResidual`) — di luar scope task ini maupun `FE-BKC-019`.
4. Lanjutkan `FE-BKC-021` (Layar Pengecualian Finansial untuk selisih yang tidak dapat ditagihkan).
