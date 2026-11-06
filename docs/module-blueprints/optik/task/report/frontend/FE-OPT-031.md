# Laporan Perubahan Frontend — `FE-OPT-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-031` |
| Judul | Katalog/profil barang dan pengaturan (Admin Optik) |
| Slice | Layar `FE-OPT-N13`, `N14` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `FR-OPT-110`, `111`; `OPT-DEC-009`, `027`, `031`, `040`, `074`; `OPT-AC-006`, `020`; `UAT-24`, `25`, `26`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 1, 3, 5.12 |
| Contract version | `OPT-API-v1` grup Opt Reference Data, `OPT-VAL-v1` `OPT-VAL-070`..`073` — `approved`, terkunci hash |
| Wewenang UI | Screen brief `approved` untuk menu, route, kolom, dan isian; warna dan susunan isian `DEV_DISCRETION`. **Satu aksi di luar brief:** Daftarkan Barang (bagian 8), menunggu konfirmasi pemilik |
| Dependency | `FE-OPT-000` ✅, `001` ✅, `003` ✅, `BE-OPT-005` ✅ |
| Klasifikasi | `MEDIUM` — 2 route, 3 berkas view, 2 hook, 1 konstanta, 1 aturan, menu; 2 berkas uji baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`030` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Penyimpanan asli belum dijalankan, agar tidak menulis ke devYoga; devYoga juga belum punya barang Optik. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Lapisan data katalog dan pengaturan sudah ada dari `FE-OPT-003`. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Katalog hanya memuat barang yang sudah berprofil, dan `PUT item-profiles/{drugId}` membuat profil bila belum ada | `GetItemCatalogAsync`, `UpsertItemProfileAsync`, `BE-OPT-005` langkah 1 | Dengan Ubah Profil saja, katalog tidak pernah terisi. Ditambahkan **Daftarkan Barang** (bagian 8) |
| `GET drugs/options` memakai `isPrescribable = true` secara bawaan, sedangkan barang Optik `IsPrescribable = false` | `DrugController`, `FR-OPT-110` | Pilihan barang wajib mengirim `isPrescribable=false`; resource bawaan `drugs` tidak dipakai |
| Backend menyimpan seluruh ruas profil saat upsert | `UpsertItemProfileAsync` | Isian di luar kategori terpilih dikirim kosong, agar ganti kategori tidak meninggalkan atribut lama |
| Merk tidak disimpan di profil, melainkan dari `MstDrug.BrandName` | `OptItemProfileResponse` | Merk tampil baca-saja di modal |
| Ruas angka pengaturan bertipe nilai, sehingga ruas kosong terbaca 0 | `OptSettingUpdateRequest` | Isian kosong ditahan di layar, karena garansi kosong akan tersimpan sebagai 0 tanpa peringatan |
| Riwayat menyimpan nama ruas teknis, tanpa nama pelaku | `ChangedFieldsSummary`, `ChangedBy` | Nama ruas diterjemahkan ke label; pelaku belum tampil (bagian 8) |

---

## 2. Proses bisnis dari sisi pengguna

1. **Admin Optik mendaftarkan barang.** Barangnya sudah dibuat di master barang Farmasi dan ditandai tidak dapat diresepkan. **Contoh:** Admin mencari `FRM-A-HTM-52`, memilih kategori Frame, mengisi warna Hitam dan ukuran 52-18-140, lalu menyimpan. Barang tampil di katalog dan dapat dipilih petugas pada form pesanan.
2. **Admin mengubah profil.** Isian mengikuti kategori: frame dan kacamata jadi berisi warna, ukuran, bahan; lensa berisi indeks, desain, lapisan; lensa kontak berisi BC, DIA, masa pakai; aksesoris tanpa atribut. Bila kategori diganti, atribut kategori lama ikut dikosongkan.
3. **Admin mengubah pengaturan (`UAT-24`).** **Contoh:** pengingat ambil 30 → 21 hari. Layar menegaskan *"Berlaku untuk pesanan sesudah disimpan."* Riwayat menampilkan *"Pengingat ambil (hari): 30 → 21"*.
4. **Nilai yang salah ditahan (`UAT-25`).** **Contoh:** batas tutup 20 hari dengan pengingat 30 hari ditolak dengan *"Batas tutup harus lebih lama dari batas pengingat."*, sebelum terkirim.
5. **Dua admin mengubah bersamaan.** Yang kedua menerima *"Pengaturan sudah diubah petugas lain. Muat ulang halaman."*, dan isian berganti ke nilai terbaru.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`optical-reference-data-slice`, `optical-order.service`, `select-resource-registry`, `health-service-select-resources`, `optical-prescription-rules`, `menu-items`. Backend (baca-saja): `OptReferenceDataController`, `OptReferenceDataService`, `OptReferenceDataDtos`, `DrugController`, `DrugDtos`; laporan `BE-OPT-005`; `OPT-DEC-031`, `074`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/master-data/item-catalog/page.jsx` | **Baru.** Route katalog |
| `src/app/health-services/optical-management/master-data/settings/page.jsx` | **Baru.** Route pengaturan |
| `src/components/view/health-services/optical-management/master-data/optical-item-catalog-view.jsx` | **Baru.** Daftar katalog |
| `src/components/view/health-services/optical-management/master-data/optical-item-profile-modal.jsx` | **Baru.** Modal Ubah Profil dan Daftarkan Barang |
| `src/components/view/health-services/optical-management/master-data/optical-settings-view.jsx` | **Baru.** Form dan riwayat |
| `src/lib/hooks/health-services/optical-management/master-data/use-optical-item-catalog.jsx` | **Baru.** Controller katalog dan modal |
| `src/lib/hooks/health-services/optical-management/master-data/use-optical-settings.jsx` | **Baru.** Controller pengaturan |
| `src/lib/constants/health-services/optical-management/optical-master-data-constants.js` | **Baru.** Resource pilihan barang, kalimat, teks, label riwayat |
| `src/utils/health-services/optical-management/optical-master-data-rules.js` | **Baru.** Query, isian per kategori, validasi, kiriman, riwayat |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diperbarui.** Grup Master Data dengan dua butir, `OptReferenceData : Read` |
| `tests/unit/optical-master-data-rules.test.mjs` | **Baru.** 11 uji |
| `tests/e2e/optical-master-data-screen.spec.mjs` | **Baru.** 10 skenario API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-003` tanpa perubahan. Pilihan barang memakai `useSelectResource` dengan resource Optik sendiri, pola yang sama dengan pilihan pasien pada pendaftaran kunjungan. Isian desimal memakai `OPTICAL_DECIMAL_FIELD_PROPS`, bukan `type: "number"`. Tidak ada CSS baru.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Keterangan muat katalog dan pengaturan; tombol nonaktif selama menyimpan |
| Kosong | Katalog menjelaskan cara mendaftarkan barang; pilihan barang menjelaskan syarat tidak dapat diresepkan; riwayat punya keterangan |
| Gagal | Katalog dan pengaturan gagal → pesan server + **Coba lagi**; `404` pengaturan → *"Pengaturan Optik belum tersedia…"*; penolakan simpan tampil di modal atau di atas tombol, isian tidak hilang |
| Tanpa hak akses | `403` → akses ditolak; tanpa `Update` tombol profil tidak tampil; tanpa `UpdateSetting` isian nonaktif dan keterangan baca-saja; menu mengikuti izin |
| Data basi | `409` → pengaturan dimuat ulang, isian berganti nilai terbaru, pesan tampil dengan judul *Data sudah berubah*; kiriman berikutnya memakai versi baru |
| Validasi | Barang, kategori, rentang desimal, seluruh aturan `OPT-VAL-070`..`073`; kalimat pengaturan sama dengan backend |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Label kategori dan desain | `OptReferenceData : Read` |
| `GET` | `/v1/health-services/optical-management/reference-data/item-catalog` | Daftar katalog | `OptReferenceData : Read` |
| `PUT` | `/v1/health-services/optical-management/reference-data/item-profiles/{drugId}` | Ubah profil, daftarkan barang | `OptReferenceData : Update` |
| `GET` | `/v1/health-services/optical-management/reference-data/settings` | Pengaturan + riwayat | `OptReferenceData : Read` |
| `PUT` | `/v1/health-services/optical-management/reference-data/settings` | Simpan pengaturan | `OptReferenceData : UpdateSetting` |

#### Lintas modul (as-is)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/drugs/options?isPrescribable=false` | Pilihan barang saat mendaftarkan | `Drug : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-master-data-rules.test.mjs` | 11 lulus, 0 gagal, termasuk dalam `tests/unit/` di bawah | `PASS` | Keluaran perintah |
| Uji mutasi 19 aturan: atribut kategori lain, kategori, barang, rentang desimal, seluruh aturan pengaturan, ruas kosong, versi, unit, label riwayat | Kesembilan belasnya terdeteksi; berkas dipulihkan dan terbukti identik | `PASS` | Keluaran skrip |
| E2E seluruh layar Optik atas build produksi | 128 lulus, 0 gagal (10 + 15 + 10 + 15 + 17 + 22 + 20 + 19) | `PASS` | Keluaran Playwright |
| Katalog | Kolom brief; atribut menurut kategori; stok; keaktifan; kategori, keaktifan, cari, reset, paginasi mengubah query | `PASS` | Skenario katalog |
| Ubah profil | Isian terisi; merk baca-saja; ganti kategori mengganti isian dan mengosongkan atribut lama; bentuk kiriman persis; rentang desimal; penolakan server di modal; daftar dimuat ulang | `PASS` | Skenario ubah profil |
| Daftarkan barang | Barang dan kategori wajib; pencarian terkirim dengan `isPrescribable=false`; id barang pada path; daftar dimuat ulang | `PASS` | Skenario daftarkan |
| Pengaturan `UAT-24`, `UAT-25` | Isian dari server; keterangan berlaku; kiriman persis; riwayat berlabel; versi naik; aturan ditahan; isian kosong ditahan; huruf terbuang; penolakan server tampil | `PASS` | Skenario pengaturan |
| Keadaan | Bentrok versi mengganti isian; baca-saja; `404` + Coba lagi; `403`; menu bertingkat mengikuti izin | `PASS` | Skenario keadaan |
| Backend lokal: `PUT` profil bentuk layar ke barang acak | `404` sesudah lolos binder | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: `PUT settings` dengan bentuk layar + versi salah | `409 OPT-CONCURRENCY`, kalimat sama | `PASS` | Skrip kontrak |
| Backend lokal: `UAT-25` | `400 OPT-VAL-071`, kalimat sama dengan layar | `PASS` | Skrip kontrak |
| Backend lokal: kontrol negatif indeks 200, kategori teks, hari desimal | Ketiganya `400`, membuktikan pemeriksaan di atas tidak lolos palsu | `PASS` | Skrip kontrak |
| Backend lokal: versi dan jumlah riwayat sebelum dan sesudah | Tetap versi 0, riwayat 0: tidak ada yang tertulis | `PASS` | Skrip kontrak |
| Backend lokal: `GET` metadata, katalog, pengaturan, pilihan barang | `200`; nama ruas cocok; bawaan `isPrescribable` terbukti membedakan hasil (7.862 vs 0) | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: katalog, dua modal, pengaturan | Susunan sesuai brief; tabel katalog muat tanpa meluap; menu bertingkat terbuka mengikuti halaman | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.386 lulus, 72 gagal dari 4.462; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-030` (`diff` kosong), tanpa berkas Optik | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (tidak ada server lokal menyala) | `Compiled successfully in 60s`; exit 0 | `PASS` | Keluaran build |

**Cacat yang ditemukan saat pengerjaan.** Pilihan barang semula memakai resource bawaan `drugs`, sehingga barang Optik tidak akan pernah muncul. API tiruan tidak dapat menangkap hal ini; temuannya berasal dari membaca parameter bawaan `DrugController`, lalu dikonfirmasi dengan backend lokal.

Uji manual: `NOT FEASIBLE` untuk penyimpanan dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya: menyimpan akan mengubah pengaturan dan profil di devYoga tanpa instruksi, dan belum ada barang `IsPrescribable = false` yang dapat didaftarkan. Perilaku simpan backend terbukti di harness `BE-OPT-005`.

**Tidak dijalankan:** `UAT-26` (barang Optik tidak tampil di peresepan), karena barangnya belum ada dan perilakunya milik data master; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N13`, `N14` dan brief §5.12 | Terpenuhi, dengan selisih bagian 8 | Skenario katalog, pengaturan |
| `UAT-24`: ubah pengaturan, riwayat tercatat | Terpenuhi di E2E API tiruan; pembekuan nilai di harness | Skenario pengaturan |
| `UAT-25`: batas tutup ditolak | Terpenuhi di layar dan backend lokal | Skenario pengaturan; skrip kontrak |
| `OPT-AC-006`: pengaturan berlaku ke depan | Keterangan tampil di layar; perilaku di harness | Skenario pengaturan |
| `OPT-DEC-027`: batas diskon tidak ada di Pengaturan Optik | Terpenuhi | Skenario pengaturan |
| `UAT-26`, `OPT-AC-020`: barang Optik tidak tampil di peresepan | **Belum dapat diuji** — butuh barang di master; layar hanya menawarkan barang `IsPrescribable = false` | Skenario daftarkan |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perlu konfirmasi pemilik | **Daftarkan Barang tidak tertulis di brief §5.12**, yang hanya menyebut Ubah Profil. Tanpa aksi ini katalog tidak pernah terisi, karena hanya menampilkan barang berprofil, dan seluruh alur pesanan tertahan. Aksinya memakai endpoint dan modal yang sama, sesuai `BE-OPT-005` langkah 1. Bila tidak disetujui, tombolnya mudah dilepas, tetapi perlu cara lain mengisi katalog |
| Peringatan | **Data prasyarat Farmasi.** devYoga belum punya satu pun barang `IsPrescribable = false`, sehingga pilihan Daftarkan Barang masih kosong. Pencarian "frame" pada barang yang dapat diresepkan juga 0. Frame, lensa, lensa kontak, dan cairan perlu dibuat di master barang lebih dulu. Barang yang keliru bertanda dapat diresepkan tidak dapat didaftarkan di sini |
| Peringatan | **Mendaftarkan barang yang sudah berprofil menimpa profilnya**, karena endpoint berupa upsert. Modal memberi tahu hal ini; layar tidak memeriksanya lebih dulu |
| Peringatan | **Izin prasyarat.** Admin Optik perlu `Drug : Read` untuk mendaftarkan barang. Tanpa izin itu, daftar pilihan menampilkan pesan server |
| Delta brief | (1) Merk tampil baca-saja, karena diambil dari master barang (`OptItemProfileResponse`), bukan isian seperti tertulis di brief. (2) Kacamata jadi memakai isian frame, karena brief tidak mengaturnya. (3) `OPT-DEC-074` menyebut penyaring stok, sedangkan brief §5.12 dan endpoint hanya punya kategori, cari, dan aktif; layar mengikuti brief. (4) Riwayat menampilkan waktu dan perubahan, tanpa nama pelaku, karena respons hanya memuat id; backend membatasi 20 riwayat terakhir |
| Dependency backend | `NONE` untuk layar ini. Nama pelaku riwayat perlu tambahan ruas bila diinginkan |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Sembilan belas mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`..`030`: `??` `optical-master-data-constants.js`, `optical-master-data-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook; ` M` `menu-items.jsx` |
| Langkah berikutnya | `FE-OPT-032` (empat laporan, satu halaman 4 tab) |
