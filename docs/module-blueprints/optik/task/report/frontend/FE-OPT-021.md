# Laporan Perubahan Frontend — `FE-OPT-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-021` |
| Judul | Daftar pesanan, tandai siap, serahkan, ajukan/putuskan batal, tutup tidak diambil |
| Slice | Layar `FE-OPT-N09`..`N11`; prasyarat `FE-OPT-022`, `030`, `032` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `FR-OPT-060`, `061`, `062`; `OPT-DEC-010`, `018`, `038`, `041`, `043`, `048`, `049`, `067`; `OPT-AC-007`, `009`, `017`, `021`, `031`, `034`; `UAT-12`, `13`, `14`, `15`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 5.8, 5.10 |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` — `approved`, terkunci hash; delta `OPT-API-v1.1` dari `BE-OPT-054` untuk dua penyaring dan empat tanda baris |
| Wewenang UI | Screen brief `approved` untuk route, kolom, kartu, dan modal; warna dan lebar kolom `DEV_DISCRETION` |
| Dependency | `FE-OPT-020` ✅, `BE-OPT-040` ✅, `041` ✅, `054` ✅ |
| Klasifikasi | `MEDIUM` — 1 route, 2 berkas view baru, 2 hook baru, 1 berkas aturan baru, perubahan pada detail pesanan dan menu, 2 berkas uji baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`020` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Alur tulis asli belum dapat dijalankan karena devYoga belum punya pesanan. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Thunk seluruh tindakan sudah ada dari `FE-OPT-003`, dan tombolnya sudah disiapkan `FE-OPT-020` dalam keadaan nonaktif. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Tidak ada endpoint ringkasan pesanan | `OptOrderController` | Angka kartu dibaca dari `totalData` empat `GET orders?<penanda>=true&pageSize=1` |
| Respons pesanan menyimpan masa garansi sebagai snapshot jumlah hari dan bulan, bukan tanggal | `OptOrderResponse`, `OPT-DEC-041` | Bagian garansi menampilkan masa apa adanya, tanpa menghitung tanggal sendiri (bagian 8) |
| Akibat persetujuan batal per baris ditentukan `TetapDitagih` | `OptOrderService` | Modal Putuskan menampilkan cermin aturan itu sebagai keterangan |
| `isLensCut` hanya bermakna untuk lensa stok aktif | `OptOrderService`, `OPT-VAL-054` | Pilihan sudah/belum dipotong hanya tampil dan terkirim untuk baris itu |
| Serah mengembalikan `invoiceFinalized` dan dapat membawa `billingMessage` | `OptOrderDtos`, `AfterProgressAsync` | Toast memberi tahu kunjungan tertutup; peringatan tagihan tampil di detail |
| `.region-confirm-modal p` memusatkan paragraf selebar 330 px dan isi modal tanpa ikon menempel pada judul | `modal-confirm.css` | Isi modal memakai `mt-3` dan `div`; berlaku juga untuk modal `FE-OPT-020` di halaman yang sama |

---

## 2. Proses bisnis dari sisi pengguna

1. **Dedi membuka Pesanan Optik.** Empat kartu menunjukkan yang perlu ditindaklanjuti. **Contoh:** Menunggu Persetujuan Batal 1, Perlu Dihubungi 2, Terlambat 3, Lewat Batas Ambil 4. Klik kartu menyaring tabel, klik sekali lagi melepasnya. Baris menampilkan tanda seperti *Terlambat 4 hari*.
2. **Pesanan selesai dikerjakan.** Dedi menekan **Tandai Siap** dan mengonfirmasi. Status menjadi *Siap Diambil*.
3. **Pasien mengambil.** Dedi membuka **Serahkan**, mencocokkan identitas, lalu mencentangnya. Tombol Serahkan baru aktif sesudah dicentang, sedangkan Batal selalu bisa. Bila diwakilkan, nama dan hubungan wajib berpasangan. Bila tagihan bersisa, modal menampilkan pesan server, mis. *"Tagihan belum lunas. Sisa Rp 550.000."*, dan status tidak berubah (`UAT-13`).
4. **Sesudah diserahkan.** Bagian *Penyerahan dan Garansi* menampilkan waktu, pengambil, catatan fitting, dan masa garansi, mis. ukuran 30 hari, cacat lensa 6 bulan. Bila ini pesanan terakhir yang lunas, toast memberi tahu tagihan final dan kunjungan tertutup (`UAT-12`).
5. **Pasien membatalkan sesudah pengerjaan.** Dedi membuka **Ajukan Batal**, menulis alasan, dan menandai setiap lensa stok sudah atau belum dipotong. Status pesanan tidak berubah; aksi lain tertahan sampai diputuskan.
6. **Kepala Optik memutuskan.** Modal **Putuskan** menampilkan alasan dan akibat per baris. **Contoh:** Frame A kembali ke stok dan barisnya batal; Lensa Custom tetap ditagih. Tolak wajib memakai catatan; Setujui tidak.
7. **Pesanan tidak diambil.** Kepala Optik membuka **Tutup Tidak Diambil** dengan alasan wajib dan keterangan *"Tagihan tetap penuh; kacamata disimpan atas nama pasien."* Bila belum waktunya, server menolak, mis. *"Pesanan baru bisa ditutup sesudah 90 hari sejak siap diambil."* Pesanan yang sudah ditutup tetap dapat diserahkan menyusul.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Slice dan service pesanan `FE-OPT-003`; `use-optical-visit-list`, `use-optical-order-detail`, `confirm-modal`, `modal-confirm.css`, `data-filter`, `summary-grid`, `base-form-control`. Backend (baca-saja): `OptOrderController`, `OptOrderService`, `OptOrderDtos`; laporan `BE-OPT-040`, `041`, `054`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/orders/page.jsx` | **Baru.** Route daftar |
| `src/components/view/health-services/optical-management/orders/optical-order-list-view.jsx` | **Baru.** Kartu, penyaring, tabel |
| `src/components/view/health-services/optical-management/orders/optical-order-fulfillment-modals.jsx` | **Baru.** Lima modal tindakan |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-list.jsx` | **Baru.** Controller daftar dan angka kartu |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-fulfillment.jsx` | **Baru.** Isian, validasi, dan kiriman lima modal |
| `src/utils/health-services/optical-management/optical-order-fulfillment-rules.js` | **Baru.** Query, kartu, tanda baris, validasi, kiriman, akibat batal, baris garansi |
| `src/lib/constants/health-services/optical-management/optical-order-constants.js` | **Diperbarui.** Teks daftar dan tindakan |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `orderFulfillment` menyala |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-detail.jsx` | **Diperbarui.** Mendelegasikan lima aksi; toast dapat membawa keterangan; tombol kembali ke Pesanan Optik |
| `src/components/view/health-services/optical-management/orders/optical-order-detail-view.jsx` | **Diperbarui.** Bagian Pengajuan Batal, Penyerahan dan Garansi |
| `src/components/view/health-services/optical-management/orders/optical-order-action-modals.jsx` | **Diperbarui.** Jarak isi modal |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diperbarui.** Butir Pesanan Optik, `OptOrder : Read` |
| `tests/unit/optical-order-fulfillment-rules.test.mjs` | **Baru.** 11 uji |
| `tests/e2e/optical-order-fulfillment-screen.spec.mjs` | **Baru.** 15 skenario API tiruan |
| `tests/e2e/optical-order-screen.spec.mjs` | **Diperbarui.** Tandai Siap kini aktif |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-003` tanpa perubahan. Satu pengecualian yang disengaja: angka kartu memanggil service langsung dan disimpan di state lokal hook, agar empat permintaan kecil itu tidak menimpa `list` dan `pagination` di slice. Tidak ada CSS baru.

Layar tidak menentukan hasil. Status terlambat dan ketiga penanda lain dibaca dari server; layar hanya menambah jumlah hari keterlambatan dari tanggal kalender.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Kartu dan tabel punya keadaan muat sendiri; tombol modal nonaktif selama mengirim |
| Kosong | *"Tidak ada pesanan Optik pada penyaring ini."* |
| Gagal | Daftar gagal → pesan server + **Coba lagi**; satu kartu gagal → *—* tanpa menahan kartu lain; penolakan tindakan → pesan server di dalam modal, isian tidak hilang |
| Tanpa hak akses | `403` daftar → akses ditolak; menu mengikuti `OptOrder : Read`; tombol mengikuti izin tiap aksi |
| Data basi | `409` `OPT-VAL-058` → modal tertutup, detail dimuat ulang, pesan server tampil dengan judul *Data sudah berubah* |
| Nonaktif | Serahkan nonaktif sampai identitas dicentang; pengajuan batal menunggu → aksi lain nonaktif; **Catat Keluhan** nonaktif sampai `FE-OPT-030` |
| Validasi | Identitas, pasangan nama dan hubungan, alasan batal, tanda lensa, keputusan, catatan tolak, alasan tutup; kalimat sama dengan backend |

Privasi: daftar tidak memuat harga dan nilai resep (ada skenario E2E).

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/orders` | Daftar dan angka kartu | `OptOrder : Read` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/mark-ready` | Tandai Siap | `OptOrder : MarkReady` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/hand-over` | Serahkan, termasuk susulan | `OptOrder : HandOver` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancellation-requests` | Ajukan Batal | `OptOrder : RequestCancel` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancellation-requests/{requestId}/decision` | Putuskan Batal | `OptOrder : DecideCancel` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/close-not-collected` | Tutup Tidak Diambil | `OptOrder : CloseNotCollected` |

#### Health Services / Optical Management / Opt Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Pilihan Status | `OptReferenceData : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-order-fulfillment-rules.test.mjs` | 11 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Seluruh uji unit Optik atas kode akhir | 78 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi 13 aturan: akibat batal, tanda lensa, catatan tolak, identitas, pasangan pengambil, kartu, hari terlambat, garansi, alasan tutup | Ketiga belasnya terdeteksi; berkas dipulihkan dan terbukti identik | `PASS` | Keluaran skrip |
| E2E seluruh layar Optik atas build produksi | 93 lulus, 0 gagal (15 + 17 + 20 + 19 + 22) | `PASS` | Keluaran Playwright |
| Daftar | Kolom, tanda, angka kartu, tanpa harga; kartu eksklusif dan dapat dilepas; status, cari, reset, dan paginasi mengubah query; kembali dari detail mempertahankan penyaring; kartu gagal, Coba lagi, kosong, `403`; menu mengikuti izin | `PASS` | Skenario daftar, menu |
| Tandai Siap | Batal tidak mengirim; versi terkirim; status berubah; bentrok menutup modal | `PASS` | Skenario tandai siap |
| Serahkan `UAT-12`, `UAT-13` | Identitas mengunci tombol, Batal tetap bisa; pasangan pengambil; bentuk kiriman persis; belum lunas tampil di modal; finalisasi dan pesan tagihan tampil; serah susulan | `PASS` | Skenario serahkan |
| Ajukan Batal `UAT-14` | Alasan dan tanda wajib; hanya lensa stok terkirim; aksi lain tertahan; pengajuan ganda ditolak di modal | `PASS` | Skenario ajukan batal |
| Putuskan `UAT-15` | Akibat per baris; keputusan wajib; Tolak butuh catatan; `requestId` dan kiriman persis; Setujui tanpa catatan | `PASS` | Skenario putuskan |
| Tutup Tidak Diambil | Keterangan tagihan; alasan wajib; batas hari dari server; kiriman persis | `PASS` | Skenario tutup |
| Izin | Tombol mengikuti izin tiap aksi; Catat Keluhan nonaktif | `PASS` | Skenario izin |
| Backend lokal: lima `POST` tindakan dengan bentuk kiriman layar ke id acak | Seluruhnya `404` sesudah lolos binder | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: kontrol negatif `isLensCut`, `approve`, `status`, dan `expectedVersion` berupa teks | Keempatnya `400`, membuktikan pemeriksaan di atas tidak lolos palsu | `PASS` | Skrip kontrak |
| Backend lokal: `GET orders` dengan seluruh penyaring layar dan empat permintaan kartu; metadata | `200`; label `orderStatuses` terbaca; jumlah pesanan tetap 0 | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: daftar, lima modal, detail sesudah diserahkan | Tiga cacat tampilan ditemukan dan diperbaiki (bagian 6, catatan). Sesudahnya susunan sesuai brief; tabel meluap 6 px dan bergulir di dalam kartunya | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.360 lulus, 72 gagal dari 4.436; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-020` (`diff` kosong), tanpa berkas Optik. Dijalankan sebelum penyesuaian terakhir lebar kolom, yang tidak menyentuh aturan | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (tidak ada server lokal menyala) | `Compiled successfully in 62s`; exit 0 | `PASS` | Keluaran build |

**Cacat yang ditemukan lewat tangkapan layar.** Seluruh E2E sudah lulus ketika ketiganya terlihat: (1) kolom Tanda terdorong keluar layar pada 1366 px, (2) isi modal menempel pada judul, (3) paragraf keterangan modal menjorok. Lebar kolom disesuaikan sampai kolom Tanda terbaca utuh dan nomor pesanan tidak terlipat.

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya: devYoga belum punya pesanan, karena pembuatan pesanan tertahan resep (kode profesi `OpticalManagement:Practitioner` kosong), katalog barang, dan akun petugas. Perilaku tulis backend terbukti di harness `BE-OPT-040`, `041`, `054`.

**Tidak dijalankan:** kredit baris tagihan sesudah batal disetujui dengan Billing asli; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N09`..`N11` dan brief §5.8, §5.10 | Terpenuhi, dengan selisih bagian 8 | Skenario daftar, tindakan |
| `UAT-12`, `OPT-AC-009`, `OPT-AC-025` di layar: serah, belum lunas ditolak, finalisasi diteruskan | Terpenuhi di E2E API tiruan; aturan di harness | Skenario serahkan |
| `UAT-14`, `UAT-15`, `OPT-AC-007`, `017`, `021` di layar: pengajuan, keputusan, akibat per baris | Terpenuhi di E2E API tiruan; aturan di harness; kredit baris menunggu persetujuan Billing | Skenario ajukan, putuskan |
| `OPT-DEC-049`: tutup, tagihan tetap, serah susulan | Terpenuhi di E2E API tiruan; aturan di harness | Skenario tutup, serah susulan |
| `OPT-DEC-067`, `OPT-AC-031`, `034`: empat kartu dan kolom Tanda | Terpenuhi | Skenario daftar; backend lokal |
| Tombol mengikuti izin; server tetap penentu | Terpenuhi | Skenario izin |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Modal Putuskan menyalin aturan `TetapDitagih` untuk keterangan.** Bila aturan backend berubah, keterangan ini wajib ikut diubah. Yang dijalankan tetap keputusan server |
| Peringatan | **Angka kartu tidak selalu seketika.** Angka dimuat saat halaman dibuka dan lewat **Coba lagi**, bukan tiap penyaring berubah. Kartu selalu menghitung seluruh pesanan, tanpa terpengaruh Status dan Cari |
| Peringatan | **Data dan izin prasyarat.** Kartu tampil bagi siapa pun yang punya `OptOrder : Read`, sesuai brief. Akun Kepala Optik perlu `DecideCancel` dan `CloseNotCollected`; devYoga belum punya akun petugas Optik |
| Delta brief | (1) Bagian garansi menampilkan **masa** dari snapshot, bukan tanggal batas seperti tertulis di brief, karena respons tidak memuat tanggal; batas untuk keluhan tertentu dihitung server saat keluhan dicatat. (2) Kartu bersifat eksklusif, satu penanda aktif sekali waktu, walau backend menerima gabungan, agar angka kartu dan isi tabel berarti sama. (3) Pilihan Setujui/Tolak dan keterangan akibat per baris ditambahkan pada modal Putuskan. (4) Tautan pesanan remake ke pesanan asal belum ada: respons pesanan hanya memuat `isRemake`; ditinjau di `FE-OPT-030`. Delta tombol kembali dari `FE-OPT-020` kini tertutup |
| Dependency backend | `NONE` untuk task ini |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Jarak isi empat modal `FE-OPT-020` diperbaiki karena satu halaman dengan modal baru. Tiga belas mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | Sesi terpotong sekali sebelum kode ditulis; dilanjutkan dari rencana tanpa pengulangan. Satu pengambilan tangkapan layar berebut port dengan E2E dan diulang sendirian |
| Status Git | Di luar berkas `FE-OPT-010`..`020`: `??` `optical-order-fulfillment-rules.js` dan dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook; ` M` `menu-items.jsx` |
| Langkah berikutnya | `FE-OPT-022` (penjualan langsung) |
