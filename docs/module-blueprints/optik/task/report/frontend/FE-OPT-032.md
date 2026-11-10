# Laporan Perubahan Frontend — `FE-OPT-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-032` |
| Judul | Empat laporan (satu halaman, 4 tab) |
| Slice | Layar `FE-OPT-N15`; prasyarat `FE-OPT-033` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `FR-OPT-100`; `OPT-DEC-025`, `070`; `UAT-22`, `23`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 1, 3, 5.11 |
| Contract version | `OPT-API-v1` grup Opt Report — `approved`, terkunci hash |
| Wewenang UI | Screen brief `approved` untuk menu, route, tab, dan penyaring; susunan kolom dan kartu `DEV_DISCRETION` |
| Dependency | `FE-OPT-021` ✅, `BE-OPT-052` ✅ |
| Klasifikasi | `SMALL` — 1 route, 2 berkas view, 1 hook, 1 konstanta, 1 aturan, menu; 2 berkas uji baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`031` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji baca dengan backend lokal). Laporan berisi data belum dapat dilihat dengan backend asli, karena devYoga belum punya pesanan dan pemeriksaan. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Lapisan data laporan sudah ada dari `FE-OPT-003`, dengan satu bagian state per tab. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Backend menghitung seluruh total, dan menjawab periode yang dipakai | `OptReportDtos` | Layar tidak menjumlah; keterangan periode dibaca dari jawaban, bukan dari isian |
| Periode terbalik atau lebih dari 366 hari ditolak `400` | `ResolvePeriod` | Pesan server tampil apa adanya, tanpa **Coba lagi** karena mengulang tidak akan berhasil |
| Tanggal kosong memakai bawaan server | `OptReportPeriodQuery` | Tanggal yang dihapus tidak dikirim |
| Baris pesanan terbuka tidak membawa label jenis | `OptOpenOrderRow` | Label dibaca dari `metadata.orderKinds` |
| `daysSinceReady` terisi untuk semua pesanan siap, walau belum perlu dihubungi | `OptOpenOrderRow` | Tanda mengikuti `needsContact`, bukan keberadaan jumlah hari |

---

## 2. Proses bisnis dari sisi pengguna

Kepala Optik membuka **Laporan Optik**. Periode bawaan awal bulan berjalan s.d. hari ini, dan dipakai bersama tiga tab.

1. **Kunjungan per Pemeriksa.** Total kunjungan dan pemeriksaan, lalu per pemeriksa. **Contoh:** Rina, RO: 7 kunjungan, 6 pemeriksaan, 1 direvisi.
2. **Penjualan.** Layar menegaskan *"Penjualan bruto dari baris tagihan, sebelum diskon tagihan."* **Contoh:** total Rp 1.125.000, terbagi per jenis barang (frame, lensa, pemeriksaan refraksi) dan per cara bayar.
3. **Pesanan Terbuka (`UAT-22`).** Memakai *per tanggal*, bukan periode. **Contoh:** pesanan siap 35 hari tampil dengan tanda *Perlu dihubungi 35 hari*, dan pesanan yang melewati perkiraan jadi bertanda *Terlambat 4 hari*.
4. **Keluhan & Garansi.** Delapan angka: keluhan, dalam dan di luar garansi, menunggu, disetujui, ditolak, remake dibuat, remake diserahkan, lalu rincian per jenis keluhan.

**Jalur tidak normal:** periode lebih dari 366 hari menampilkan *"Periode laporan paling panjang 366 hari."* Petugas tanpa `OptReport : Read` tidak melihat menunya, dan membuka alamatnya ditolak (`UAT-23`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`optical-report-slice`, `optical-order.service`, `data-filter`, `filter-date-picker`, `summary-grid`, `use-optical-visit-detail`. Backend (baca-saja): `OptReportController`, `OptReportService`, `OptReportDtos`; laporan `BE-OPT-052`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/reports/page.jsx` | **Baru.** Route; `?tab=` |
| `src/components/view/health-services/optical-management/reports/optical-report-view.jsx` | **Baru.** Tab, penyaring, keadaan |
| `src/components/view/health-services/optical-management/reports/optical-report-panels.jsx` | **Baru.** Isi empat tab |
| `src/lib/hooks/health-services/optical-management/reports/use-optical-report.jsx` | **Baru.** Controller |
| `src/lib/constants/health-services/optical-management/optical-report-constants.js` | **Baru.** Tab dan teks |
| `src/utils/health-services/optical-management/optical-report-rules.js` | **Baru.** Periode bawaan, query, kartu, tanda |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diperbarui.** Butir Laporan Optik, `OptReport : Read`, sebelum Master Data |
| `tests/unit/optical-report-rules.test.mjs` | **Baru.** 8 uji |
| `tests/e2e/optical-report-screen.spec.mjs` | **Baru.** 9 skenario API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-003` tanpa perubahan. Tab memakai `DataFilter`, sama dengan detail kunjungan. Tidak ada CSS baru. Layar tidak menghitung uang.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Hanya tab aktif yang meminta data; tabel menampilkan keterangan muat; atur ulang nonaktif |
| Kosong | Keterangan per tabel; angka nol tampil sebagai `0`, bukan `—` |
| Gagal | `400` → pesan server tanpa Coba lagi; gangguan lain → pesan server + **Coba lagi**; tabel disembunyikan agar data lama tidak terbaca sebagai hasil penyaring baru |
| Tanpa hak akses | `403` → akses ditolak; menu mengikuti `OptReport : Read` |

Privasi: Pesanan Terbuka memuat nama dan No. RM, sesuai kontrak, tanpa nilai resep dan tanpa harga.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reports/visits-by-examiner` | Kunjungan per Pemeriksa; `from`, `to` | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/sales` | Penjualan; `from`, `to` | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/open-orders` | Pesanan Terbuka; `asOf` | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/warranty` | Keluhan & Garansi; `from`, `to` | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Label jenis pesanan | `OptReferenceData : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-report-rules.test.mjs` | 8 lulus, 0 gagal, termasuk dalam `tests/unit/` di bawah | `PASS` | Keluaran perintah |
| Uji mutasi 12 aturan: tab, periode bawaan, query, nol, format uang, tanda, koma, keterangan rentang | Putaran pertama satu lolos (tanda *Perlu dihubungi* mengikuti jumlah hari); uji ditambah, putaran kedua kedua belasnya terdeteksi; berkas dipulihkan dan terbukti identik | `PASS` | Keluaran skrip |
| E2E seluruh layar Optik atas build produksi akhir | 137 lulus, 0 gagal (9 + 10 + 15 + 10 + 15 + 17 + 22 + 20 + 19) | `PASS` | Keluaran Playwright |
| Keempat tab | Angka dan tabel dari server; hanya tab aktif yang meminta; `?tab=` terbawa di alamat; tab tak dikenal ke tab pertama; keterangan bruto; jumlah berkoma | `PASS` | Skenario tab |
| `UAT-22` | Per tanggal; tanda dan hari dari server; label jenis dari metadata; batas pengingat pada kartu | `PASS` | Skenario pesanan terbuka |
| `UAT-23` | `403` ditolak; menu hilang tanpa izin | `PASS` | Skenario izin |
| Penyaring | Periode bawaan; ganti dan hapus tanggal mengubah query; periode dipakai bersama antartab; tanggal acuan terpisah; atur ulang mengembalikan bawaan | `PASS` | Skenario penyaring |
| Keadaan | `400` tanpa Coba lagi; `500` dengan Coba lagi; kosong; nol tampil `0` | `PASS` | Skenario penyaring, kosong |
| Backend lokal: keempat laporan dengan query layar, tanpa tanggal, dan hanya tanggal akhir | `200`; nama ruas cocok dengan yang dibaca layar; periode terjawab sesuai | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: batas periode | Terbalik `400`; 366 hari `200`; 367 hari `400`, kalimat sama | `PASS` | Skrip kontrak |
| Backend lokal: kontrol negatif `asOf=zzz` | `400` | `PASS` | Skrip kontrak |
| Backend lokal: kontrol negatif `from=05-11-2026` | **Tidak ditolak:** `200`, dibaca sebagai 11 Mei 2026. Layar tidak terdampak, karena selalu mengirim `YYYY-MM-DD` | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: keempat tab | Tiga cacat ditemukan dan diperbaiki (catatan di bawah). Sesudahnya sesuai brief; tabel Pesanan Terbuka meluap 1 px | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.394 lulus, 72 gagal dari 4.470; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-031` (`diff` kosong), tanpa berkas Optik | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (tidak ada server lokal menyala) | `Compiled successfully in 60s`; exit 0 | `PASS` | Keluaran build |

**Cacat yang ditemukan lewat tangkapan layar.** Seluruh E2E sudah lulus ketika ketiganya terlihat: (1) kolom Tanda pada Pesanan Terbuka terpotong pada 1366 px; (2) dua pemilih tanggal bertumpuk, karena `DataFilter` menghitung Fragment sebagai satu kontrol; (3) delapan kartu garansi tersusun 6 + 2. Perbaikannya: No. RM dan nama digabung menjadi kolom Pasien, pemilih tanggal dikirim sebagai array, dan lebar kartu diatur menjadi 4 + 4.

Uji manual: `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan, dan untuk seluruh `GET` lewat backend lokal. `NOT FEASIBLE` untuk melihat laporan berisi dengan data asli: devYoga hanya punya satu kunjungan, tanpa pemeriksaan, pesanan, tagihan Optik, dan keluhan. Kebenaran angka terbukti di harness `BE-OPT-052`.

**Tidak dijalankan:** `403` dengan akun petugas asli, karena belum ada akun berizin Optik dan superadmin menutupi penolakan; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N15` dan brief §5.11 | Terpenuhi, dengan selisih bagian 8 | Skenario tab |
| `OPT-DEC-070`: satu halaman, 4 tab, satu menu | Terpenuhi | Skenario tab, izin |
| `UAT-22`: pesanan siap lama tampil *perlu dihubungi* | Terpenuhi di E2E API tiruan; aturan di harness | Skenario pesanan terbuka |
| `UAT-23`: petugas penjualan ditolak | Terpenuhi di E2E API tiruan; filter izin di harness | Skenario izin |
| Layar tidak menghitung uang | Terpenuhi: total dan nilai dari server | Kode; uji unit |
| Ekspor tidak termasuk MVP | Tidak dibuat, sesuai brief | Kode |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Izin laporan memuat data pasien.** Pesanan Terbuka menampilkan nama dan No. RM, sehingga `OptReport : Read` sebaiknya hanya untuk Kepala Optik dan Admin Optik, sesuai catatan `BE-OPT-052` |
| Peringatan | **Backend menerima tanggal berformat lain tanpa menolak.** `from=05-11-2026` dibaca sebagai 11 Mei 2026. Layar tidak terdampak; ini perlu diingat bila endpoint dipanggil dari tempat lain |
| Delta brief | (1) No. RM dan nama digabung menjadi kolom Pasien pada Pesanan Terbuka, agar kolom Tanda muat pada 1366 px. (2) Baris Pesanan Terbuka tidak dapat diklik, karena brief hanya meminta isi tabel mengikuti respons; tautan ke detail mudah ditambahkan karena `orderId` tersedia |
| Dependency backend | `NONE` |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Dua belas mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`..`031`: `??` `optical-report-constants.js`, `optical-report-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook; ` M` `menu-items.jsx` |
| Langkah berikutnya | `FE-OPT-033` (E2E per UAT `UAT-01`..`26`) |
