# Laporan Perubahan Frontend — `FE-OPT-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-022` |
| Judul | Penjualan langsung |
| Slice | Layar `FE-OPT-N07` mode langsung, `N08` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `FR-OPT-050`; `OPT-DEC-020`, `046`, `069`; `OPT-AC-010`, `029`; `UAT-10`, `11`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 3, 5.3, 5.7, 5.8 |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `OPT-VAL-042`, `049`, `050` — `approved`, terkunci hash; tanpa endpoint baru (`BE-OPT-042`) |
| Wewenang UI | Screen brief `approved` untuk route, mode form, dan aksi; urutan bagian dan isi keterangan `DEV_DISCRETION` |
| Dependency | `FE-OPT-021` ✅, `BE-OPT-042` ✅ |
| Klasifikasi | `SMALL` — perluasan form dan aturan pesanan; tanpa route baru; 1 berkas uji E2E baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`021` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Alur tulis asli belum dapat dijalankan karena devYoga belum punya katalog barang Optik. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Route `orders/new?kind=direct` sudah ada tetapi ditahan pesan *"Penjualan langsung belum tersedia."* Aksi draf tertagih sudah disiapkan `FE-OPT-020`, dan modal Serahkan sudah dibuat `FE-OPT-021`. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Resep hanya wajib bila ada lensa kontak | `OptOrderService.ValidateContentAsync` | Bagian Resep hanya muncul sesudah lensa kontak dipilih |
| Lensa kacamata ditolak pada penjualan langsung | `DirectSaleSpectacleLensMessage` | Tombol Tambah nonaktif dengan alasannya; kategorinya tidak ditawarkan |
| Katalog hanya menerima satu kategori dan tidak bisa mengecualikan kategori | `GET reference-data/item-catalog` | Baris lensa kacamata tetap tampil; menyaring di layar akan merusak paginasi |
| `PUT orders/{id}` menolak bila jenis berbeda dari pesanan | `OptOrderService` baris 318 | Halaman Ubah membaca jenis dari pesanan; sebelumnya form selalu mengirim jenis berpengerjaan |
| Resep dikunci hanya saat kirim ke pengerjaan | `OptOrderService` baris 521 | Keterangan *"Resep akan terkunci…"* tidak tampil pada penjualan langsung |
| Tagihkan membuat pesanan tetap `Draft`, minimum uang muka 0% | `BE-OPT-042` | Status bayar tampil tanpa baris uang muka |

---

## 2. Proses bisnis dari sisi pengguna

**Contoh (`UAT-10`):** Pak Joko (samaran) membeli 1 botol cairan lensa kontak Rp 85.000.

1. **Dedi membuka Penjualan Langsung** dari tab Pesanan kunjungan.
2. **Dedi memilih barang.** Form dibuka dengan bagian Barang, tanpa resep. Baris lensa kacamata tidak dapat ditambahkan, dan penunjuk pada tombolnya menjelaskan alasannya. **Simpan Draf** membuka detail dengan perkiraan harga Rp 85.000.
3. **Dedi menagihkan.** Status tetap *Draf*, status bayar muncul tanpa uang muka, dan aksi utama berubah menjadi **Serahkan**. Tidak ada Kirim ke Pengerjaan.
4. **Kasir menerima Rp 85.000.** Dedi menekan **Muat ulang status bayar**.
5. **Dedi menyerahkan** sesudah mencocokkan identitas. Bila masih bersisa, modal menampilkan *"Tagihan belum lunas. Sisa Rp 85.000."* Bila stok habis, modal menampilkan *"Stok aksesoris & cairan CAIRAN-CL di depo Optik tidak cukup."* dan status tidak berubah.
6. **Lensa kontak (`UAT-11`).** Begitu lensa kontak ditambahkan, bagian **2. Resep** muncul dan hanya menawarkan resep lensa kontak yang berlaku. Tanpa resep, Simpan Draf ditolak dengan *"Lensa kontak wajib memakai resep lensa kontak yang masih berlaku."* Bila lensa kontak dihapus, bagian Resep hilang dan resep yang tadi dipilih tidak ikut terkirim.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-optical-order-form`, `optical-order-form-view`, `optical-order-rules`, `optical-order-print-document`, `use-optical-visit-detail`. Backend (baca-saja): `OptOrderService`, `OptOrderPaymentGate`; laporan `BE-OPT-042`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/optical-management/optical-order-rules.js` | **Diperbarui.** Kebutuhan resep menurut jenis, barang yang dapat dipilih, pilihan kategori, validasi, kiriman, penanda bukti |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-form.jsx` | **Diperbarui.** Jenis pesanan dari alamat atau pesanan; resep hanya dibaca bila dibutuhkan |
| `src/components/view/health-services/optical-management/orders/optical-order-form-view.jsx` | **Diperbarui.** Urutan bagian mengikuti mode, kolom Pasokan disembunyikan, alasan pada tombol nonaktif |
| `src/components/view/health-services/optical-management/orders/optical-order-detail-view.jsx` | **Diperbarui.** Status bayar tanpa baris uang muka |
| `src/components/view/health-services/optical-management/orders/optical-order-print-document.jsx` | **Diperbarui.** Bukti tanpa tujuan pengerjaan dan perkiraan jadi |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx` | **Diperbarui.** Tombol Penjualan Langsung membuka form |
| `src/lib/constants/health-services/optical-management/optical-order-constants.js` | **Diperbarui.** Teks mode langsung; pesan penahan dihapus |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `directSale` menyala |
| `tests/unit/optical-order-rules.test.mjs` | **Diperbarui.** 5 uji baru |
| `tests/e2e/optical-direct-sale-screen.spec.mjs` | **Baru.** 10 skenario API tiruan |
| `tests/e2e/optical-order-screen.spec.mjs`, `tests/e2e/optical-visit-screen.spec.mjs` | **Diperbarui.** Mengikuti tombol yang kini aktif |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice dan service tanpa perubahan. Form berkacamata dan penjualan langsung tetap satu form sesuai `OPT-DEC-069`. Tidak ada CSS baru.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Sama dengan form pesanan; *"Membaca versi resep..."* menahan Simpan Draf |
| Kosong | Katalog dan barang terpilih punya keterangan; pasien tanpa resep lensa kontak melihat keterangan di daftar pilihan |
| Gagal | Penolakan server tampil di atas tombol dan isian tidak hilang; penolakan serah tampil di modal |
| Tanpa hak akses | Tombol Penjualan Langsung mengikuti `OptOrder : Create`; form ditolak tanpa izin |
| Data basi | Sama dengan pesanan: `409` memuat ulang isian terbaru |
| Nonaktif | Tambah nonaktif untuk lensa kacamata dengan alasan pada `title`; barang nonaktif tetap tidak dapat dipilih |
| Validasi | Barang, jumlah, dan resep lensa kontak; kalimat sama dengan backend |

Privasi: bukti tanpa harga, sesuai `OPT-DEC-073` (ada skenario E2E). Penjualan barang bebas resep tidak membaca resep pasien sama sekali (ada skenario E2E).

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Perilaku yang dipakai pada grup Opt Order:

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/optical-management/orders` | Buat, `orderKind = 2` | `OptOrder : Create` |
| `PUT` | `/v1/health-services/optical-management/orders/{id}` | Ubah draf, jenis mengikuti pesanan | `OptOrder : Update` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/bill` | Tagihkan, tetap `Draft` | `OptOrder : Bill` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/hand-over` | Serahkan dari `Draft` tertagih | `OptOrder : HandOver` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancel` | Batal | `OptOrder : Cancel` |
| `GET` | `/v1/health-services/optical-management/prescriptions/options?patientId=` | Hanya bila ada lensa kontak | `OptPrescription : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Seluruh uji unit layar pesanan | `optical-order-rules` 20 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi 8 aturan: kebutuhan resep, lensa kacamata, barang nonaktif, kategori, validasi, resep sisa, bukti | Kedelapannya terdeteksi; berkas dipulihkan dan terbukti identik | `PASS` | Keluaran skrip |
| E2E penjualan langsung, 3 putaran | 30 lulus, 0 gagal | `PASS` | Keluaran Playwright |
| E2E seluruh layar Optik atas build produksi | 103 lulus, 0 gagal (10 + 15 + 17 + 20 + 19 + 22) | `PASS` | Keluaran Playwright |
| Form `UAT-10` | Tanpa bagian resep dan tanpa permintaan resep; Tambah lensa kacamata nonaktif dengan alasan; kategori tanpa lensa kacamata; tanpa kolom Pasokan; bentuk kiriman persis | `PASS` | Skenario form |
| Form `UAT-11` | Bagian Resep muncul, hanya resep lensa kontak, wajib, tanpa keterangan kunci; versi resep terkirim; hapus lensa kontak → resep sisa tidak terkirim | `PASS` | Skenario form |
| Ubah dan penolakan | Jenis dari pesanan, bukan alamat; `PUT` membawa jenis; penolakan server tidak menghilangkan isian | `PASS` | Skenario ubah, form |
| Detail `UAT-10` | Tagihkan tetap Draf; status bayar tanpa uang muka; tanpa Kirim dan Tandai Siap; serah menutup kunjungan | `PASS` | Skenario detail |
| Serah dan batal | Belum lunas dan stok kurang ditolak di modal; draf tertagih dapat dibatalkan | `PASS` | Skenario serah, batal |
| Cetak | Isi iframe cetak tanpa tujuan pengerjaan, perkiraan jadi, dan harga | `PASS` | Skenario cetak |
| Backend lokal: `POST` tanpa resep, `POST` dengan resep, `PUT` bentuk layar ke id acak | Seluruhnya `404` sesudah lolos binder | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: kontrol negatif jenis dan jumlah berupa teks | Keduanya `400`, membuktikan pemeriksaan di atas tidak lolos palsu | `PASS` | Skrip kontrak |
| Backend lokal: metadata; opsi resep; daftar | `200`; lima kategori terbaca; jumlah pesanan tetap 0 | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: form dengan lensa kontak, detail draf tertagih | Susunan sesuai brief | `PASS` | Tangkapan layar di scratchpad |
| Pengukuran Simpan Draf terhadap footer pada 1280, 1366, dan 1920 px, form dua mode | Saat digulir habis, tombol sekitar 8 px di atas footer dan terkena klik | `PASS` | Skrip ukur di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.365 lulus, 72 gagal dari 4.441; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-021` (`diff` kosong), tanpa berkas Optik | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (tidak ada server lokal menyala) | `Compiled successfully in 58s`; exit 0 | `PASS` | Keluaran build |

**Kegagalan berselang yang ditemukan.** Skenario `UAT-11` lulus pada putaran pertama, lalu gagal pada putaran kedua. Tangkapan layar menunjukkan Simpan Draf di balik footer mengambang, sehingga klik jatuh ke footer. Pengukuran membuktikan pengguna tidak terhalang saat digulir habis; penyebabnya pembantu klik di spec yang menekan tombol saat bagian Resep masih menambah tinggi halaman. Pembantu kini memastikan tombol yang terkena klik, lalu spec diulang tiga putaran tanpa gagal.

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya: devYoga belum punya katalog barang Optik dan akun petugas penjualan. Perilaku tulis backend terbukti di harness `BE-OPT-042`.

**Tidak dijalankan:** pembayaran dengan kasir asli; potong stok asli; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N07` mode langsung dan brief §5.7 | Terpenuhi, dengan selisih bagian 8 | Skenario form |
| `UAT-10`, `OPT-AC-010`: tanpa resep, tagih, lunas, serah tanpa pengerjaan | Terpenuhi di E2E API tiruan; potong stok dan finalisasi di harness | Skenario form, detail |
| `UAT-11`, `OPT-AC-029`: lensa kontak wajib resep lensa kontak berlaku | Terpenuhi di E2E API tiruan; penolakan resep kedaluwarsa di harness | Skenario form |
| `OPT-DEC-046`: lensa kacamata tidak dapat dipilih | Terpenuhi | Skenario form, uji unit |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Resep lensa kontak pada penjualan langsung tidak terkunci.** Backend mengunci resep hanya saat kirim ke pengerjaan, yang tidak dilalui penjualan langsung. Pesanan tetap menyimpan versi yang dipakai, tetapi resepnya masih dapat direvisi sesudah terjual. Layar mengikuti perilaku ini dan tidak menampilkan keterangan kunci |
| Peringatan | **Draf tertagih berlabel sama dengan draf biasa.** Status keduanya *Draf*; pembedanya aksi Serahkan, bagian Status Bayar, dan riwayat *Ditagihkan*. Label mengikuti server |
| Delta brief | (1) Mode langsung mendahulukan **Barang**, dengan Resep sesudahnya, karena bagian Resep baru muncul setelah barang dipilih. (2) Baris lensa kacamata tetap tampil di katalog dalam keadaan nonaktif; endpoint tidak bisa mengecualikan kategori. (3) Baris uang muka disembunyikan pada status bayar, dan tujuan pengerjaan serta perkiraan jadi disembunyikan pada bukti, karena tidak berlaku bagi penjualan langsung |
| Dependency backend | `NONE` |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Delapan mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`..`021`: `??` `tests/e2e/optical-direct-sale-screen.spec.mjs`; perubahan lain berada pada berkas Optik yang sudah `??` |
| Langkah berikutnya | `FE-OPT-030` (keluhan garansi). `FE-OPT-031` dan `032` juga siap dikerjakan |
