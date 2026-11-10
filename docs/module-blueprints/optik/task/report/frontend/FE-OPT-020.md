# Laporan Perubahan Frontend — `FE-OPT-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-020` |
| Judul | Buat pesanan, katalog, tagihkan, ajukan diskon, status bayar, kirim ke pengerjaan, cetak bukti pesanan |
| Slice | Layar pesanan (`FE-OPT-N07`, `N08`); prasyarat `FE-OPT-021` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-010`..`032` |
| Trace | `FR-OPT-040`, `041`, `043`, `082`; `OPT-DEC-008`, `009`, `027`, `028`, `035`, `044`, `064`, `066`, `073`, `075`, `076`; `OPT-AC-005`, `006`, `019`, `027`, `031`; `UAT-07`, `08`, `09`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 3, 5.7, 5.8, 6 |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` bagian 4 — `approved`, terkunci hash; delta `OPT-API-v1.1` dari `BE-OPT-054`; Billing `POST invoices/{id}/discounts` as-is (`INT-OPT-11`) |
| Wewenang UI | Screen brief `approved` untuk route, form, modal tindakan, dan isi cetak; warna dan tata letak isian `DEV_DISCRETION` |
| Dependency | `FE-OPT-012` ✅, `003` ✅, `BE-OPT-032` ✅, `034` ✅, `054` ✅ |
| Klasifikasi | `MEDIUM` — 3 route, 5 berkas view, 2 hook, 1 konstanta, 1 aturan, perubahan kecil kunjungan, 2 berkas uji baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`012` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal), dengan satu selisih terhadap brief pada Ajukan Diskon (bagian 8). Alur tulis asli belum dapat dijalankan. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Lapisan data pesanan sudah ada dari `FE-OPT-003`. Tab Pesanan pada detail kunjungan menampilkan daftar, dengan tombol nonaktif. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| `prescriptions/options` tidak memuat id versi, padahal `POST orders` meminta `PrescriptionVersionId` | `OptPrescriptionOptionResponse`, `OptOrderCreateRequest` | Sesudah resep dipilih, versi terkininya dibaca dari `GET prescriptions/{id}` |
| Jenis resep ditentukan barang: lensa kontak butuh resep lensa kontak, selain itu resep kacamata | `ValidateContentAsync` | Pilihan resep disaring menurut barang; resep yang tidak sesuai terlepas otomatis |
| `payment-status` `409` sebelum ditagihkan | `OptOrderPaymentGate` | Status bayar hanya diminta sesudah `billedAt` terisi |
| Billing menentukan nominal diskon dari kebijakan dan menolak nominal manual, kecuali diskon dokter | `BillingDiscountService.ResolveApplicationAsync` | Modal Ajukan Diskon tanpa isian nominal (bagian 8) |
| Diskon dokter butuh memo dan jasa dokter pada baris | `BillingDiscountService` | Modal hanya menawarkan kebijakan `PROMO_ITEM` |
| Pilihan pemasok memakai izin `Supplier : Read` | `SupplierController` | Tanpa izin, pesan server tampil di daftar pilihan |

---

## 2. Proses bisnis dari sisi pengguna

1. **Dedi membuat pesanan.** Dari tab Pesanan ia memilih resep berlaku dan barang dari katalog, yang dapat disaring menurut kategori, warna, ukuran, indeks, dan kata kunci. Stok depo tampil. Lensa kacamata wajib memilih pasokan stok atau custom. Form tidak menampilkan harga. **Simpan Draf** membuka detail pesanan.
2. **Dedi melihat perkiraan harga.** **Contoh:** frame Rp 450.000 + lensa Rp 600.000 → perkiraan total Rp 1.050.000 dari backend. Barang tanpa tarif ditandai dengan pesan server dan total tampil "—".
3. **Dedi menagihkan.** Modal mengingatkan *"Barang tidak bisa diubah sesudah ditagihkan."* Status menjadi *Menunggu Uang Muka*; status bayar dan harga baris dibaca dari tagihan.
4. **Kasir menerima uang muka.** Dedi menekan **Muat ulang status bayar**. **Contoh:** bagian pasien Rp 1.050.000, minimum 50% = Rp 525.000, terbayar Rp 525.000, *Terpenuhi*.
5. **Dedi mengirim ke pengerjaan** dengan tujuan dan perkiraan jadi; lab luar wajib memilih pemasok. Penolakan server tampil di dalam modal, mis. *"Uang muka belum memenuhi minimum Rp 525.000."* (`UAT-08`) atau *"Lunasi pesanan OPT-ORD-… dulu atau daftarkan kunjungan baru."* (`UAT-09`).
6. **Cetak Bukti Pesanan.** Berisi nomor pesanan, pasien, No. RM, kunjungan, barang, tujuan, perkiraan jadi, dan tanggal cetak, tanpa harga dan tanpa nilai resep.
7. **Ajukan Diskon.** Dedi memilih kebijakan, baris tagihan pesanan ini, dan alasan. Nominal dihitung Billing dari kebijakan, lalu tampil di kolom Diskon.
8. **Batal sebelum pengerjaan** dengan alasan wajib. **Ubah** tersedia selama draf belum ditagihkan, dan halaman Ubah ikut menampilkan perkiraan harga versi tersimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Slice dan service pesanan `FE-OPT-003`; `use-apply-billing-discount.js`, `billing-invoice-slice`, `menu-pembayaran-view`; `row-action-menu`, `confirm-modal`, `use-select-resource`. Backend (baca-saja): `OptOrderController`, `OptOrderService`, `OptOrderPaymentGate`, `OptOrderDtos`, `OptReferenceDataDtos`, `BillingDiscountService`, `BillingInvoiceDtos`, `SupplierController`; kontrak `state-transition-matrix`, `validation-matrix`, `permission-audit-matrix`, `integration-contract`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/visits/[encounterId]/orders/new/page.jsx` | **Baru.** Route buat pesanan; `?kind=` |
| `src/app/health-services/optical-management/orders/[orderId]/page.jsx` | **Baru.** Route detail |
| `src/app/health-services/optical-management/orders/[orderId]/edit/page.jsx` | **Baru.** Route ubah draf |
| `src/components/view/health-services/optical-management/orders/optical-order-form-view.jsx` | **Baru.** Resep, katalog, barang terpilih |
| `src/components/view/health-services/optical-management/orders/optical-order-detail-view.jsx` | **Baru.** Header, aksi, barang, status bayar, riwayat |
| `src/components/view/health-services/optical-management/orders/optical-order-action-modals.jsx` | **Baru.** Tagihkan, Kirim, Batal, Ajukan Diskon |
| `src/components/view/health-services/optical-management/orders/optical-order-price-preview.jsx` | **Baru.** Perkiraan harga |
| `src/components/view/health-services/optical-management/orders/optical-order-print-document.jsx` | **Baru.** Bukti pesanan |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-form.jsx` | **Baru.** Controller form |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-detail.jsx` | **Baru.** Controller detail; memakai `useApplyBillingDiscount` |
| `src/lib/constants/health-services/optical-management/optical-order-constants.js` | **Baru.** Enum, kalimat, teks |
| `src/utils/health-services/optical-management/optical-order-rules.js` | **Baru.** Validasi, kiriman, aksi per status, cetak |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `OPTICAL_SCREEN_READY`: `order` menyala; tambah `orderFulfillment`, `directSale` |
| `src/utils/health-services/optical-management/optical-visit-rules.js` | **Diperbarui.** Penjualan Langsung mengikuti `directSale` |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx`, `src/components/view/health-services/optical-management/visits/optical-visit-detail-view.jsx` | **Diperbarui.** Tombol pesanan dan klik baris pesanan |
| `tests/unit/optical-order-rules.test.mjs` | **Baru.** 15 uji |
| `tests/e2e/optical-order-screen.spec.mjs` | **Baru.** 17 skenario API tiruan |
| `tests/unit/optical-visit-rules.test.mjs`, `tests/e2e/optical-visit-screen.spec.mjs` | **Diperbarui.** Mengikuti tombol pesanan yang kini aktif |
| `tests/e2e/optical-refraction-exam-screen.spec.mjs`, `tests/e2e/optical-prescription-screen.spec.mjs` | **Diperbarui.** Pembantu klik menunggu posisi tombol stabil (bagian 6) |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-003` tanpa perubahan. Ajukan Diskon memakai hook Billing yang ada, dengan baris tagihan dibatasi pada baris pesanan ini, sehingga tidak ada logika diskon kedua. Cetak mengikuti pola `FE-OPT-012`. Tidak ada CSS baru.

Layar tidak menghitung uang: seluruh angka, termasuk total, minimum uang muka, dan sisa, dibaca dari backend.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Keterangan muat per bagian; tombol nonaktif selama aksi; katalog ber-*debounce* 300 ms |
| Kosong | Katalog, barang terpilih, dan resep berlaku punya keterangan masing-masing |
| Gagal | Pesan server tampil di modal atau di atas tombol, isian tidak hilang; perkiraan harga, status bayar, dan harga tagihan gagal per bagian tanpa menahan halaman |
| Tanpa hak akses | Tanpa `Create` atau `Update` → form ditolak; `403` detail → akses ditolak; `403` tagihan → *"Harga tagihan belum dapat ditampilkan: …"*; tombol mengikuti izin |
| Data basi | `409` `OPT-VAL-058` → modal tertutup, detail dan status bayar dimuat ulang, pesan server tampil; aksi berikutnya memakai versi terbaru |
| Nonaktif | Aksi `FE-OPT-021`, `030` nonaktif, *Fitur belum tersedia*; pengajuan batal menunggu → aksi lain nonaktif dengan keterangan; draf tertagih → form Ubah ditahan |
| Validasi | Barang, resep, pasokan, jumlah, tujuan pengerjaan; kalimat sama dengan backend |

Privasi: bukti pesanan tanpa nilai resep dan tanpa harga (ada skenario E2E).

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/orders?encounterId=` | Tab Pesanan | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}` | Detail | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}/price-preview` | Perkiraan harga draf | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}/payment-status` | Status bayar | `OptOrder : Read` |
| `POST` | `/v1/health-services/optical-management/orders` | Buat draf | `OptOrder : Create` |
| `PUT` | `/v1/health-services/optical-management/orders/{id}` | Ubah draf | `OptOrder : Update` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/bill` | Tagihkan | `OptOrder : Bill` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/send-to-work` | Kirim ke pengerjaan | `OptOrder : SendToWork` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancel` | Batal sebelum pengerjaan | `OptOrder : Cancel` |

#### Health Services / Optical Management / Opt Reference Data dan Opt Prescription

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/item-catalog` | Katalog + stok depo | `OptReferenceData : Read` |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Label kategori, pasokan, tujuan | `OptReferenceData : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/options?patientId=` | Pilihan resep berlaku | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/{id}` | Id versi terkini | `OptPrescription : Read` |

#### Lintas modul (as-is)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/billing-management/billing/invoices/{id}` | Harga dan diskon baris | `BillingInvoice : Read` |
| `POST` | `/v1/health-services/billing-management/billing/invoices/{id}/discounts` | Ajukan diskon | `BillingDiscount : Create` |
| `GET` | `/v1/health-services/billing-management/master-data/discount-policies` | Kebijakan diskon | `DiscountPolicy : Read` |
| `GET` | `/v1/administrator/master-data/suppliers/options` | Pemasok lab luar | `Supplier : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-order-rules.test.mjs` | 15 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi: harga masuk bukti / pasokan terkirim semua barang / pemasok terkirim untuk bengkel sendiri / pengajuan batal tidak menahan / diskon batal ikut tampil | Uji terkait gagal pada kelimanya; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| E2E pesanan + resep + pemeriksaan + kunjungan atas build produksi | 78 lulus, 0 gagal, dua putaran berturut-turut (17 + 20 + 19 + 22) | `PASS` | Keluaran Playwright |
| Form | Penyaring katalog terkirim, atur ulang, paginasi; validasi; versi resep dibaca saat dipilih; pilihan resep disaring; bentuk kiriman persis; lensa kontak; campur lensa ditolak; hapus barang; tanpa harga | `PASS` | Skenario form |
| Ubah | Isian terisi, perkiraan harga tampil, `PUT` + versi; tertagih ditahan; bentrok memuat ulang | `PASS` | Skenario ubah |
| Draf dan tagihkan | Perkiraan dari backend, tarif kosong, `503` tidak menahan, Batal tidak mengirim, tanpa tarif ditolak apa adanya, harga dari tagihan sesudah sukses | `PASS` | Skenario draf, tagihkan |
| Status bayar | Angka backend, muat ulang meminta lagi, pesanan penahan, tagihan `403` tidak menahan, `503` tampil | `PASS` | Skenario status bayar |
| Kirim `UAT-08`, `UAT-09` | Wajib lengkap, lab luar butuh pemasok, penolakan di modal, sukses berubah status; bentrok menutup modal, kiriman berikutnya memakai versi baru | `PASS` | Skenario kirim |
| Batal dan diskon | Alasan wajib; hanya `PROMO_ITEM` dan baris pesanan ini; bentuk kiriman persis tanpa nominal; penolakan di modal; Diskon tampil di baris | `PASS` | Skenario batal, diskon |
| Cetak dan keadaan | Isi bukti dibaca dari iframe cetak, tanpa harga dan nilai resep; pengajuan batal menahan aksi; remake tanpa status bayar; baca-saja; `404`, `403` | `PASS` | Skenario cetak, keadaan |
| Backend lokal: `POST` buat, `PUT`, tagihkan, kirim, batal, diskon bentuk layar ke id acak | Seluruhnya `404` sesudah lolos binder; kontrol negatif jumlah teks dan tanggal salah → `400`; jumlah pesanan tetap 0 | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: `GET` metadata, katalog dengan seluruh penyaring, opsi resep, pemasok, kebijakan diskon | `200`; nama ruas pemasok dan metadata cocok | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: form dan detail | Susunan sesuai brief | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.349 lulus, 72 gagal dari 4.425; daftar berkas gagal **identik** dengan sesudah `FE-OPT-012` (`diff` kosong) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 62s`; 568 halaman statis; exit 0 | `PASS` | Keluaran build |

**Kegagalan berselang yang ditemukan.** Saat keempat spec dijalankan bersama, dua skenario lama sempat gagal. Tangkapan layar menunjukkan isian benar tetapi klik jatuh saat halaman masih bergulir halus sesudah `fill()`. Sumbernya pembantu klik di spec, bukan layar. Pembantu kini menunggu posisi tombol stabil.

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya: devYoga belum punya resep (kode profesi `OpticalManagement:Practitioner` kosong, lihat `FE-OPT-011`), katalog barang Optik kosong, kebijakan diskon kosong, dan belum ada akun petugas penjualan. Perilaku tulis backend terbukti di harness `BE-OPT-032`, `034`, `054`.

**Tidak dijalankan:** alur uang muka dengan kasir asli; cetak ke kertas sungguhan; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N07` dan brief §5.7 | Terpenuhi untuk pesanan berpengerjaan; mode `?kind=direct` milik `FE-OPT-022` | Skenario form |
| Layar sesuai `FE-OPT-N08` dan brief §5.8 | Terpenuhi untuk aksi `FE-OPT-020`; aksi lain nonaktif sampai `FE-OPT-021`, `030` | Skenario detail |
| `OPT-DEC-076`: form tanpa harga, pratinjau di detail dan Ubah | Terpenuhi | Skenario form, ubah, draf |
| `UAT-07`: kirim ke bengkel, status berubah | Terpenuhi di E2E API tiruan untuk layar; pemotongan stok di harness | Skenario kirim |
| `UAT-08`, `UAT-09`: penolakan uang muka dan pesanan penahan | Terpenuhi di E2E API tiruan; aturan di harness | Skenario kirim |
| Status bayar dari backend, tidak dihitung ulang (`OPT-DEC-028`, `044`) | Terpenuhi | Skenario status bayar |
| `OPT-DEC-073`: bukti tanpa nilai resep dan harga | Terpenuhi | Skenario cetak |
| `OPT-AC-016`, `UAT-19`: diskon di atas batas menunggu persetujuan | **Belum terpenuhi** — tertahan `OPT-APR-004` (`POST-MVP`), lihat bagian 8 | `BillingDiscountService` |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Ajukan Diskon tidak sama dengan brief §5.8.** Brief menyebut isian nominal dan *"Menunggu persetujuan Kepala Optik"* di atas batas. Billing as-is menolak nominal manual untuk diskon promo, memberi status disetujui langsung, dan persetujuan hanya ada untuk diskon dokter. Layar mengikuti perilaku nyata: pilih kebijakan, baris, dan alasan; besar diskon mengikuti kebijakan. Akibatnya setiap kebijakan `PROMO_ITEM` yang dipilih **langsung berlaku tanpa persetujuan**, dan `UAT-19` belum dapat dipenuhi. Ini sesuai catatan `OPT-APR-004` `POST-MVP`, tetapi perlu diketahui saat Billing menyiapkan kebijakan |
| Peringatan | **Kebijakan diskon tidak dapat disaring khusus Optik.** `MstDiscountPolicy` tidak punya penanda modul, sehingga seluruh kebijakan `PROMO_ITEM` aktif ikut tampil |
| Peringatan | **Data dan izin prasyarat.** devYoga belum punya katalog barang Optik, kebijakan diskon, dan akun petugas penjualan. Petugas juga butuh `BillingInvoice : Read`, `DiscountPolicy : Read`, dan `Supplier : Read`, selain `OPT-PERM-v1` |
| Delta brief | (1) Tombol kembali menuju tab Pesanan kunjungan, bukan *Pesanan Optik*, sampai daftarnya dibangun `FE-OPT-021`. (2) `GET prescriptions/{id}` ditambahkan karena `prescriptions/options` tidak memuat id versi. (3) Riwayat status tampil; bagian pengajuan batal dan garansi menyusul `FE-OPT-021`, kecuali peringatan pengajuan menunggu |
| Dependency backend | `OPT-APR-004` untuk diskon di atas batas (`POST-MVP`). Selebihnya `NONE` |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Pembantu klik pada spec `FE-OPT-010`..`012` diperbaiki (bagian 6). Lima mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`..`012`: `??` `optical-order-constants.js`, `optical-order-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook |
| Langkah berikutnya | `FE-OPT-021` (daftar pesanan, tandai siap, serahkan, ajukan/putuskan batal, tutup tidak diambil) |
