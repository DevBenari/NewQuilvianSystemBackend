# Laporan Perubahan Frontend — `FE-BD-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-011` |
| Judul | Lokasi penyimpanan darah dikelola, akibat penonaktifan terbaca |
| Slice | Roadmap frontend Bank Darah — layar `FE-BD-10` Setup master ketiga |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §3 |
| Trace | `DEC-BD-035` (master ketiga Setup), `DEC-BD-037` (penonaktifan tidak memindahkan kantong), `INV-BD-025`, `INV-BD-027` · `BD-CAP-021` |
| Contract version | `v4` — ✅ **`approved`** (`Sukmagp`, 2026-09-03) |
| Wewenang UI | Rupa layar `DEV_DISCRETION`. Yang dikunci roadmap hanya sumber data, hak akses, dan keadaan layar yang wajib ada |
| Dependency | `G1` ✅ · `BE-BD-014` ✅ **SELESAI** — 9 endpoint terverifikasi langsung di source backend |
| Klasifikasi | `MEDIUM` — satu fitur master data penuh, 14 berkas baru, nol komponen baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `V2QuilvianSystemFrontendDev` (source) + `docs/module-blueprints/bank-darah/` pada backend (laporan & bukti roadmap saja) |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `7e90e0477` cabang `sukmagpV2` |
| Commit backend yang dijadikan rujukan | `843430b` cabang `sukmagp` — kontrak `BE-BD-014` dibaca pada SHA ini. Backend `HEAD` bergerak ke `500db78` saat task berjalan, dan rentang `843430b..500db78` **docs-only** (3 berkas, seluruhnya `docs/`), sehingga kontrak yang dibaca tidak berubah |
| Tanggal | `2026-09-10` |
| Status | 🟡 **SELESAI SEBAGIAN.** Layar berdiri penuh dan tervalidasi, tetapi acceptance `FE-BD-015` **tidak dapat dipenuhi** karena data sumbernya belum ada di backend |

---

## 1. Keadaan yang ditemukan di awal

Modul Bank Darah sudah punya dua layar Setup master yang dibangun `FE-BD-001`: Katalog Komponen
Darah dan Daftar Alasan Terkendali. Keduanya lengkap dan mengikuti bentuk baku master data.

**Yang belum ada: layar ketiga.** Backend `BE-BD-014` sudah menyediakan seluruh permukaannya sejak
selesai, tetapi tidak ada satu pun layar yang memanggilnya. Akibatnya lokasi penyimpanan darah
hanya dapat dibuat lewat seeder atau langsung ke database.

**Kenapa itu berbahaya.** `INV-BD-025` menyatakan ketika nol lokasi aktif, **seluruh alur Bank
Darah berhenti**: tidak ada kantong yang dapat disimpan, dialokasikan, maupun diberikan. Tanpa
layar, keadaan sekritis itu tidak terlihat siapa pun sampai ada pasien yang menunggu.

**Bukti yang diperiksa, bukan diasumsikan.** Controller backend dibaca langsung dan kesembilan
endpoint baseline master data terbukti ada — termasuk `GET /summary` yang memulangkan
`IsBloodBankHaltedByEmptyActiveLocation`, sebuah penanda yang **dihitung server** supaya layar
tidak perlu menyimpulkannya sendiri dari angka nol.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya** petugas BDRS yang menyiapkan master Bank Darah sebelum modul dipakai melayani
pasien, dan penanggung jawab yang menutup sebuah lokasi ketika kulkasnya rusak atau dipindahkan.

### 2.1 Alur normal — menambah lokasi

1. Petugas membuka **Bank Darah → Setup → Lokasi Penyimpanan Darah**.
2. Layar menampilkan tiga kartu ringkasan: Total Lokasi, Lokasi Aktif, dan Nonaktif.
3. Petugas menekan **+ Tambah Lokasi Penyimpanan Darah**.
4. Petugas mengisi **Kode Lokasi** (contoh `KLK-BSR`, maksimal 30 karakter), **Nama Lokasi**
   (contoh `Kulkas Bank Darah Ruang Sentral`, maksimal 150 karakter), dan **Keterangan** yang
   bersifat opsional (maksimal 250 karakter).
5. Setelah disimpan, layar berpindah ke halaman detail lokasi yang baru dibuat. Lokasi baru
   **selalu lahir aktif** — status aktif tidak ditawarkan saat menambah, karena membuat lokasi
   yang langsung nonaktif tidak punya kegunaan.

### 2.2 Alur menonaktifkan lokasi

1. Petugas membuka detail lokasi, lalu menekan **Nonaktifkan**.
2. Muncul konfirmasi yang **menyebut akibatnya lebih dulu**, bukan sekadar bertanya "yakin?":

   > Lokasi ini tidak lagi dapat dipilih untuk penyimpanan baru maupun perpindahan kantong.
   > Kantong yang masih tercatat di sana tidak berpindah dan tidak berubah status, tetapi belum
   > dapat dialokasikan sampai dipindahkan ke lokasi yang aktif.

3. Setelah dikonfirmasi, status berubah dan tombolnya berganti menjadi **Aktifkan**.

**Tombol Hapus sengaja tidak ada.** `DEC-BD-037` menetapkan penonaktifan sebagai jalur yang benar:
ia menutup gerbang tanpa memutus makna riwayat penempatan lama yang menyebut lokasi itu.

### 2.3 Jalur tidak normal — seluruh alur Bank Darah berhenti

Ketika **nol lokasi aktif**, layar menampilkan peringatan merah di atas kartu ringkasan:

> **Alur Bank Darah berhenti.** Tidak ada satu pun lokasi penyimpanan yang aktif, sehingga kantong
> darah tidak dapat disimpan, dialokasikan, maupun diberikan. Aktifkan kembali salah satu lokasi,
> atau tambahkan lokasi baru, sebelum ada pasien yang menunggu.

Peringatan itu **berbeda dari keadaan tabel kosong**. Tabel kosong berarti "belum ada data";
peringatan ini berarti "modul berhenti bekerja". Keduanya bisa muncul bersamaan pada instalasi
baru, dan bisa juga muncul sendiri-sendiri: daftar berisi sepuluh lokasi yang semuanya nonaktif
tetap memunculkan peringatan ini walau tabelnya penuh.

Peringatan hanya dirender setelah ringkasan selesai dimuat, supaya pesan sekeras itu tidak
berkedip setiap kali halaman dibuka.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` frontend · `rules/GLOBAL_RULES.md` · `rules/frontend/master-data-feature-standard.md` · `rules/frontend/ui-consistency-checklist.md` |
| Kontrak backend (read-only) | `Areas/HealthServices/MasterData/Controllers/BloodStorageLocationController.cs` · `DTOs/BloodStorageLocationDtos.cs` · `Services/BloodStorageLocationService.cs` |
| Modul rujukan terdekat | Seluruh 14 berkas `blood-bank-reasons` — modul yang sama, dibangun `FE-BD-001` |
| Rujukan otoritatif standar | `hr/master-data/job-level` — dipakai memutuskan pola `unwrapApiData` dan `formatNumber` |
| Base component | `base-detail-view.jsx`, `confirm-modal.jsx`, `information-alert.jsx`, dan katalog `base-features/` |
| Roadmap | `roadmap/frontend-roadmap.md` §3, `roadmap/backend-roadmap.md` §6.1, `03-frontend-architecture.md` §layar |

### 3.2 Berkas yang berubah

**Empat belas berkas baru** — bentuk baku master data, tujuh berkas source ditambah lima route:

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/blood-storage-locations/blood-storage-locations-constants.jsx` | **Baru.** Objek `BLOOD_STORAGE_LOCATIONS_CONFIG` tunggal |
| `src/lib/state/slice/health-services/master-data/master-data-blood-storage-locations-slice.jsx` | **Baru.** Sembilan thunk, state, reducer, selector |
| `src/utils/health-services/master-data/blood-storage-locations/blood-storage-locations-utils.jsx` | **Baru.** Fungsi murni: baca payload, form, validasi, detail rows, penanda modul berhenti |
| `src/lib/hooks/.../use-master-data-blood-storage-locations.jsx` | **Baru.** Controller halaman list |
| `src/lib/hooks/.../use-master-data-blood-storage-locations-detail.jsx` | **Baru.** Controller detail + aksi aktif/nonaktif |
| `src/lib/hooks/.../use-master-data-blood-storage-locations-editor.jsx` | **Baru.** Controller create dan update |
| `src/components/view/.../master-data-blood-storage-locations-view.jsx` | **Baru.** Layar daftar |
| `src/components/view/.../detail/blood-storage-locations-detail-view.jsx` | **Baru.** Layar detail |
| `src/components/view/.../add/blood-storage-locations-form-view.jsx` | **Baru.** Layar tambah dan perbarui |
| `src/app/health-services/master-data/blood-storage-locations/` | **Baru.** Lima route tipis: `page.jsx`, `<feature>-client.jsx`, `create/page.jsx`, `[slug]/page.jsx`, `[slug]/update/page.jsx` |

**Tiga berkas existing disunting:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Registrasi reducer `masterDataBloodStorageLocation` — 2 baris |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua entri menu: pada kelompok master data Health Services, dan pada **Bank Darah → Setup** sejajar dua master yang sudah ada — 10 baris |
| `src/utils/health-services/master-data/blood-bank-reasons/blood-bank-reasons-utils.jsx` | **Perbaikan lintas-task atas persetujuan eksplisit pengguna.** Menambahkan ekspor `formatNumber` yang hilang. Lihat §8 |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya lurus dan searah, sama dengan modul rujukan:

```text
route (app/) -> view (components/view/) -> hook (lib/hooks/) -> slice (lib/state/slice/)
                          |                      |                        |
                          +----------------------+------------------------+
                                                 v
                              constants (CONFIG tunggal) + utils (fungsi murni)
```

**Nol arsitektur baru.** Tidak ada factory, generator, hook master-data generik, maupun lapisan
HTTP tandingan. Struktur disalin dari modul rujukan lalu diisi ulang, persis seperti yang
diwajibkan `AGENTS.md` frontend.

**Duplikasi helper antar slice master data dipertahankan** — itu keputusan yang sudah diambil
standar, bukan technical debt yang dirapikan sambil lalu.

### 3.4 Gerbang keputusan base component

Layar dipecah menjadi elemen, lalu tiap elemen ditetapkan statusnya. **Seluruhnya `REUSE`; nol
`NEW`, nol `EXTEND`**, sehingga gerbang ini tidak menuntut keputusan pengguna.

| Elemen layar | Status | Base component yang dipakai |
| --- | --- | --- |
| Gerbang hak akses | `REUSE` | `access-denied-gate` |
| Kepala halaman + tombol tambah | `REUSE` | `hero` + `base-button` |
| Kartu ringkasan | `REUSE` | `summary-grid` |
| **Peringatan modul berhenti** | `REUSE` | `information-alert` varian `danger` |
| Baris filter, pencarian, reset | `REUSE` | `data-filter` + `filter-select` + `filter-date-picker` |
| Tabel daftar | `REUSE` | `data-table` |
| Lencana status | `REUSE` | `status-badge` |
| Paginasi | `REUSE` | `pagination` |
| Halaman detail | `REUSE` | `base-detail-view` |
| **Konfirmasi aktif/nonaktif** | `REUSE` | `confirm-modal` lewat slot konfirmasi `base-detail-view`, varian `warning`/`success` |
| Form tambah dan perbarui | `REUSE` | `base-editor-view` |
| Notifikasi | `REUSE` | `toast-stack` bawaan kedua base view |

`UI GATE: PASSED — 12 elemen, 12 REUSE, 0 NEW, 0 EXTEND, 0 menunggu keputusan pengguna.`

Slot konfirmasi `base-detail-view` bernama `deleteConfirm`, tetapi isinya murni pass-through ke
`confirm-modal`. Mengisinya dengan judul, pesan, label, dan varian penonaktifan adalah pemakaian
lewat props — bukan perubahan perilaku default komponennya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan keadaan memuat bawaan `data-table`; kartu ringkasan menampilkan keadaan memuatnya sendiri. Peringatan modul berhenti **ditahan** sampai ringkasan selesai, supaya tidak berkedip |
| Kosong | `Belum ada data lokasi penyimpanan darah yang tersimpan.` Bila nol lokasi aktif, peringatan merah modul berhenti muncul **terpisah** di atasnya |
| Gagal | Pesan gagal dari backend ditampilkan lewat `information-alert` varian `danger`. Sembilan pesan gagal masing-masing punya kalimatnya sendiri, contoh `Gagal mengambil daftar lokasi penyimpanan darah.` Permintaan yang dibatalkan tidak dianggap gagal |
| Tanpa hak akses | `access-denied-gate` menutup seluruh layar dengan aksi `BloodStorageLocation:Read` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Master Data / Blood Storage Location

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-storage-locations/filters/metadata` | Nilai awal filter, pilihan urutan, dan pilihan jumlah baris | `BloodStorageLocation : Read` |
| `GET` | `/v1/health-services/master-data/blood-storage-locations/summary` | Tiga kartu ringkasan **dan** penanda modul berhenti | `BloodStorageLocation : Read` |
| `GET` | `/v1/health-services/master-data/blood-storage-locations` | Tabel daftar beserta paginasinya | `BloodStorageLocation : Read` |
| `GET` | `/v1/health-services/master-data/blood-storage-locations/options` | Tersedia di slice, **belum dipakai layar ini** — feed untuk layar kantong darah nanti | `BloodStorageLocation : Read` |
| `GET` | `/v1/health-services/master-data/blood-storage-locations/{id}` | Halaman detail dan pengisian form perbarui | `BloodStorageLocation : Read` |
| `POST` | `/v1/health-services/master-data/blood-storage-locations` | Menambah lokasi baru | `BloodStorageLocation : Create` |
| `PUT` | `/v1/health-services/master-data/blood-storage-locations/{id}` | Mengubah kode, nama, keterangan, dan status | `BloodStorageLocation : Update` |
| `PATCH` | `/v1/health-services/master-data/blood-storage-locations/{id}/status` | Mengaktifkan dan menonaktifkan dari halaman detail | `BloodStorageLocation : Update` |
| `DELETE` | `/v1/health-services/master-data/blood-storage-locations/{id}` | Tersedia di slice, **sengaja tidak dipasang tombolnya** sesuai DoD | `BloodStorageLocation : Delete` |

**Filter yang tidak dikirim.** Kontrol Tanggal Mulai, Tanggal Akhir, dan Periode tetap dirender
sesuai kontrak baku master data, tetapi nilainya hidup di state UI saja. Backend `GetAll` hanya
menerima `search`, `isActive`, `sortBy`, `sortDirection`, `pageNumber`, dan `pageSize` —
diverifikasi langsung dari tanda tangan controller. Ketiganya terdaftar sebagai
`unsupportedFilterKeys` beserta alasannya sebagai komentar di constants.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 17 berkas task ini | **Keluaran kosong** — `0 error, 0 warning` | `PASS` | Dijalankan terpisah untuk memisahkan kontribusi task ini dari garis dasar |
| `npm run lint` seluruh repository | **`608 problems (0 errors, 608 warnings)`** | `PASS` | **Nol** di antaranya berasal dari berkas task ini, dibuktikan baris di atas |
| `npm run build` — **sebelum** perbaikan | **GAGAL**, `Turbopack build failed with 2 errors` | `EXISTING / ENVIRONMENT ISSUE` | Keduanya `The export formatNumber was not found` pada `blood-bank-reasons-utils.jsx`. **Nol error menyebut `blood-storage-locations`** |
| `npm run build` — **sesudah** perbaikan | **`✓ Compiled successfully in 27.1s`** | `PASS` | Keempat route task ini terdaftar; `postbuild` standalone juga berhasil |
| Route terdaftar di keluaran build | 4 route | `PASS` | `/blood-storage-locations` (static), `/[slug]`, `/[slug]/update` (dynamic), `/create` (static) |
| `node --test tests/unit` | **`pass 434, fail 0`** | `PASS` | 434 kasus uji existing; nol regresi |
| Grep anti-regresi UI — warna literal | Nihil | `PASS` | `#hex` dan `rgba()` nol pada 15 berkas JSX |
| Grep anti-regresi UI — typography literal | Nihil | `PASS` | `font-size`, `font-weight`, `line-height` nol |
| Grep anti-regresi UI — button mentah | Nihil | `PASS` | `<button`, `btn-primary`, `btn-secondary` nol |
| Grep anti-regresi UI — tabel mentah | Nihil | `PASS` | `<table` nol; tabel memakai `data-table` |
| Grep anti-regresi UI — utility `fw-`/`fs-` | Nihil pada berkas fitur | `PASS` | `fs-4` hanya pada ikon menu, mengikuti seluruh 40+ entri existing |
| Grep anti-regresi UI — `!important` | Nihil | `PASS` | Nol berkas CSS baru dibuat |

**Uji manual: `NOT FEASIBLE`.** Alasannya konkret dan berlapis: tabel `MstBloodStorageLocation`
**belum ada di lingkungan mana pun** karena migration `20260903083142_AddMstBloodStorageLocation`
belum dijalankan, dan menjalankan migration adalah wewenang terpisah yang tidak diberikan task ini.
Tanpa tabelnya, kesembilan endpoint memulangkan galat, sehingga alur layar tidak dapat ditelusuri
di peramban. Yang **dapat** dibuktikan tanpa database sudah dibuktikan: layar ter-compile, keempat
route terdaftar, dan nol regresi pada 434 kasus uji.

**Tidak dijalukan:** `npm run test:e2e` — repository memakai Playwright, dan menurut
`rules/frontend/test-policy.md` test otomatis baru bersifat opsional dan bukan gerbang selesai.
Menjalankan e2e juga menuntut aplikasi berjalan beserta databasenya, yang tidak tersedia.

**Catatan tentang `npm run test:unit`.** Script bawaan gagal di lingkungan Windows ini
(`Could not find 'tests\unit\**\*.test.mjs'`) karena glob-nya tidak diekspansi shell. Suite-nya
sendiri **ada dan sehat** — 37 berkas, 434 kasus uji, seluruhnya lulus ketika dijalankan sebagai
`node --import ./tests/helpers/register.mjs --test tests/unit`. Ini temuan tooling di luar cakupan
task ini, dilaporkan tanpa diubah.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FE-BD-014` keadaan kosong menyatakan **modul berhenti**, bukan sekadar "tidak ada data" | ✅ **Terpenuhi** | Peringatan `information-alert` varian `danger` dirender dari `IsBloodBankHaltedByEmptyActiveLocation` yang **dihitung backend**, terpisah dan berbeda kalimat dari pesan tabel kosong. Cadangan sisi klien (`activeCount === 0`) hanya dipakai bila penanda itu tidak dikirim |
| `FE-BD-015` konfirmasi penonaktifan **menyebut jumlah kantong tertahan** | ⛔ **BELUM terpenuhi** | **Angkanya tidak ada di backend.** Entity penempatan kantong `BbkBloodUnitPlacement` belum ada di source — dinyatakan sendiri oleh `BloodStorageLocationService.cs:288` — dan `PATCH /{id}/status` hanya memulangkan `BloodStorageLocationResponse` tanpa angka apa pun. Yang **sudah** dikerjakan: konfirmasi menyebut konsekuensinya sebagai kalimat, selaras dengan pesan backend sendiri. Angka menyusul bersama `BE-BD-015` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Tombol hapus **tidak** disediakan | ✅ **Terpenuhi** — halaman detail hanya punya Kembali, Perbarui, dan Nonaktifkan/Aktifkan |
| Penonaktifan memakai `PATCH /{id}/status` | ✅ **Terpenuhi** — `updateBloodStorageLocationsStatus` |
| Penonaktifan **tidak** memindahkan kantong (`DEC-BD-037`) | ✅ **Terpenuhi** — layar tidak memanggil endpoint pemindahan apa pun, dan kalimat konfirmasinya menyatakan hal itu secara eksplisit |
| Bentuk baku master data diikuti | ✅ **Terpenuhi** — 7 berkas source + 5 route + 2 registrasi |

**Satu dari dua acceptance criteria terpenuhi.** Karena itu task ini 🟡 **SEBAGIAN**, bukan ✅.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` memulangkan 608 warning pada seluruh repository, **nol** di antaranya dari berkas task ini |
| Masalah yang diketahui | `FE-BD-015` belum terpenuhi — lihat §7. Angka kantong tertahan menunggu `BE-BD-015` |
| **Temuan di luar cakupan — dua cacat `FE-BD-001`** | **(1) Build rusak.** `master-data-blood-bank-reasons-view.jsx` meng-import `formatNumber` yang tidak pernah diekspor utils-nya, sehingga `npm run build` **gagal sejak commit `7e90e0477`** — padahal `FE-BD-001` bertanda ✅ SELESAI. **Diperbaiki atas persetujuan eksplisit pengguna** dengan menambahkan ekspor tersebut, bentuknya disamakan dengan modul rujukan `job-level`. **(2) Belum diperbaiki:** editor hook `blood-bank-reasons` memanggil `utils.unwrapApiData(...)`, sedangkan `unwrapApiData` **tidak ada** di default export — setiap simpan yang **berhasil** akan melempar TypeError lalu menampilkan toast "Gagal Menyimpan". Rujukan `job-level` memakai named import; fitur baru ini sudah memakai pola yang benar. Cacat kedua **sengaja dibiarkan** karena melampaui perbaikan yang disetujui, dan sebaiknya menjadi task perbaikan tersendiri milik `FE-BD-001` |
| Dependency backend | `BE-BD-014` ✅ selesai. **Migration `20260903083142_AddMstBloodStorageLocation` belum dijalankan**, sehingga layar belum dapat dipakai walau kodenya berdiri — wewenang terpisah. `BE-BD-015` ⛔ masih tertahan `G4`, dan itulah yang menahan `FE-BD-015` |
| Perubahan sampingan | `NONE` — nol berkas tergenerasi ikut tertinggal. Folder `.next/` hasil build tidak di-track |
| Interupsi | `NONE` |
| Status Git | Lihat §9 |
| Langkah berikutnya | **(1)** Jalankan migration Bank Darah agar layar dapat diuji manual di peramban. **(2)** Jadwalkan perbaikan cacat kedua `FE-BD-001`. **(3)** Lengkapi `FE-BD-015` setelah `BE-BD-015` menyediakan angka kantong tertahan. **(4)** `FE-BD-006` dan `FE-BD-009` masih menunggu — `FE-BD-009` terblokir dasar layar `FE-BD-06` yang dibangun `FE-BD-005` |

---

## 9. Status Git

```text
 M src/lib/state/store.jsx
 M src/utils/health-services/master-data/blood-bank-reasons/blood-bank-reasons-utils.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/master-data/blood-storage-locations/
?? src/components/view/health-services/master-data/blood-storage-locations/
?? src/lib/constants/health-services/master-data/blood-storage-locations/
?? src/lib/hooks/health-services/master-data/blood-storage-locations/
?? src/lib/state/slice/health-services/master-data/master-data-blood-storage-locations-slice.jsx
?? src/utils/health-services/master-data/blood-storage-locations/
```

Frontend `HEAD` tetap `7e90e0477` cabang `sukmagpV2`. Backend **tidak disentuh sama sekali** di
luar berkas laporan ini beserta bukti roadmap dan traceability. `HEAD` backend berada di `500db78`
ketika laporan ini ditutup; pergerakan dari `843430b` itu dilakukan pengguna dan isinya docs-only.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. Nol perintah database dijalankan.
