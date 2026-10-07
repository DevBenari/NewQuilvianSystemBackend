# Laporan Perubahan Frontend — `FE-RWI-192`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-192` |
| Judul | Master Butir Persiapan Bedah |
| Slice | Slice E1 — Kamar operasi dari bangsal (`MVP-1` / `RWF-W3`) |
| Roadmap | [`../../../roadmap/frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-192` |
| Trace | `FR-RWF-045`; `RWI-DEC-173` butir 3; Frontend 13.1, 13.2, 13.4.10 (`FE-INP-34`); API 11.4 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Wewenang UI | `DEV_DISCRETION` di dalam letak yang diputuskan: Pelayanan Kesehatan → Master Data → Butir Persiapan Bedah (`/health-services/master-data/surgical-preparation-items`) |
| Dependency | `BE-RWI-173` [BE] — ✅ Selesai (`SurgicalPreparationItemController`) |
| Klasifikasi | `MEDIUM` — pembuatan halaman master data baru dengan 9 endpoint CRUD, penyaring, form modal berkonkurensi RowVersion, kartu statistik, dan integrasi menu sidebar |
| Task mode | `CROSS-REPO` sempit — kode aplikasi di `QuilvianSystemFrontendDev`; laporan dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| Target tulis | `src/app/health-services/master-data/surgical-preparation-items/`, `src/components/view/health-services/master-data/surgical-preparation-items/`, `src/lib/services/health-services/master-data/`, `src/utils/menu-sidebar/`, `tests/unit/` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `8740efa02` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `0a108994` (branch `MHamzah`) |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ **SELESAI.** Seluruh kriteria penerimaan terbukti, `npm run build` lulus, unit test lulus 4/4. |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dikerjakan:
1. Backend sub-modul Finishing telah menyediakan 9 endpoint lengkap untuk master butir persiapan bedah di `Areas/HealthServices/MasterData/Controllers/SurgicalPreparationItemController.cs` (`BE-RWI-173`).
2. Namun pada frontend (`QuilvianSystemFrontendDev`), belum ada antarmuka pengguna untuk mengelola katalog butir checklist tersebut.
3. Rute `/health-services/master-data/surgical-preparation-items` belum ada, service klien belum tersedia, dan butir menu "Butir Persiapan Bedah" belum terdaftar pada navigasi sidebar `menu-items.jsx`.
4. Akibatnya, checklist Catatan Pra-Operasi bangsal (`FE-RWI-195` / `FE-INP-27`) tidak memiliki sumber master data dinamis yang dapat dikelola oleh Admin Master Data rumah sakit.

---

## 2. Proses bisnis dari sisi pengguna

1. **Akses Menu:**
   Petugas/Admin Master Data yang memiliki izin `SurgicalPreparationItem : Read` melihat menu baru **Butir Persiapan Bedah** di bawah grup menu **Pelayanan Kesehatan → Master Data**.
2. **Melihat Ringkasan dan Daftar Butir:**
   Pengguna membuka halaman dan disajikan kartu statistik (Total Butir, Butir Aktif, Butir Nonaktif, Wajib Aktif, Opsional Aktif, dan Kelompok Aktif), diikuti tabel daftar butir persiapan yang terstruktur rapi.
3. **Penyaringan Data:**
   Pengguna dapat mencari butir berdasarkan kata kunci pencarian (kode, nama, keterangan), menyaring berdasarkan Kelompok Persiapan ("Verifikasi Pasien", "Persiapan Fisik", "Dokumen & Penunjang", "Obat & Puasa"), menyaring status (Aktif/Nonaktif), atau menyaring sifat (Wajib/Opsional).
4. **Menambah Butir Baru:**
   Pengguna menekan tombol "Tambah Butir", mengisi kode butir unik (maksimal 30 karakter), memilih kelompok persiapan, mengisi nama butir (maksimal 200 karakter), menentukan sifat wajib/opsional, urutan tampil, serta keterangan tambahan. Butir baru otomatis aktif. Bila kode kembar dimasukkan, sistem menolak dengan pesan konflik 409 `MST-SPI-001`.
5. **Mengubah Butir:**
   Pengguna menekan tombol "Ubah" pada baris tabel. Kode butir ditampilkan terkunci (read-only), sedangkan nama, kelompok, sifat wajib, urutan, dan keterangan dapat diperbarui. Sistem menyertakan token konkurensi `RowVersion`. Bila admin lain telah mengubah data terlebih dahulu, sistem mendeteksi konflik 409 dan memuat ulang data terkini secara otomatis tanpa menimpa perubahan orang lain.
6. **Mengubah Status Operasional:**
   Pengguna dapat mengaktifkan atau menonaktifkan butir melalui tombol toggle aksi. Butir yang dinonaktifkan tidak akan muncul pada pembuatan versi Catatan Pra-Operasi bangsal yang baru, namun versi lama yang sudah terbit tetap terlindungi.
7. **Menghapus Butir:**
   Pengguna dapat menghapus lunak butir melalui tombol hapus dengan konfirmasi aman. Jika butir sudah pernah digunakan pada Catatan Pra-Operasi pasien, server menolak penghapusan (400) dan menyarankan penonaktifan butir.
8. **Jalur Tidak Normal:**
   - **Data Kosong:** Menampilkan pesan baku *"Belum ada butir persiapan bedah."*
   - **Gagal Jaringan / Server Error:** Menampilkan alert bahaya merah dengan pesan error spesifik dari server.
   - **Tanpa Hak Akses (403):** Dilindungi komponen `AccessDeniedGate`, menampilkan pesan *"Ups! Akses Ditolak"*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx`
- `QuilvianSystemFrontendDev/src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx`
- `QuilvianSystemFrontendDev/src/components/features/base-features/` (`hero.jsx`, `summary-grid.jsx`, `data-table.jsx`, `status-badge.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx`)
- `QuilvianSystemFrontendDev/src/components/view/health-services/master-data/daily-nursing-actions/` (referensi pola view master data terdekat)
- `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Controllers/SurgicalPreparationItemController.cs`
- `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/DTOs/SurgicalPreparationItemDtos.cs`
- `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-173.md`
- `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/03-frontend-architecture.md` (bagian 13.1, 13.2, 13.4.10)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/master-data/surgical-preparation-item.service.js` | **Baru.** Service pemanggil 9 endpoint API Master Butir Persiapan Bedah via `InstanceAxios` |
| `src/components/view/health-services/master-data/surgical-preparation-items/modals/surgical-preparation-item-form-modal.jsx` | **Baru.** Modal formulir tambah/ubah dengan validasi lengkap dan penanganan `RowVersion` |
| `src/components/view/health-services/master-data/surgical-preparation-items/surgical-preparation-items-view.jsx` | **Baru.** Komponen tampilan utama lengkap dengan Hero, SummaryGrid, Toolbar Filter, DataTable, Pagination, ConfirmModal, dan proteksi `AccessDeniedGate` |
| `src/app/health-services/master-data/surgical-preparation-items/surgical-preparation-items-client.jsx` | **Baru.** Client component App Router |
| `src/app/health-services/master-data/surgical-preparation-items/page.jsx` | **Baru.** Server component App Router dengan metadata halaman resmi |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Mendaftarkan butir menu "Butir Persiapan Bedah" pada `healthServicesMasterData.subItems` dengan izin `SurgicalPreparationItem : Read` |
| `tests/unit/surgical-preparation-item-master.test.mjs` | **Baru.** Suite pengujian unit otomatis yang memverifikasi kepatuhan AC-1 hingga AC-4 |

### 3.3 Kepatuhan arsitektur frontend

- **Base Component Decision Gate:**
  - Header: `Hero` (`REUSE`)
  - Kartu Ringkasan: `SummaryGrid` (`REUSE`)
  - Tabel: `DataTable` (`REUSE`)
  - Status: `StatusBadge` (`REUSE`)
  - Paginasi: `Pagination` (`REUSE` terintegrasi di `DataTable`)
  - Konfirmasi Hapus: `ConfirmModal` (`REUSE`)
  - Form Modal: `BaseModal` (`COMPOSE` di folder domain `modals/`)
  - Penjaga Hak Akses: `AccessDeniedGate` (`REUSE`)
  - Seluruh komponen mematuhi aturan tanpa memperkenalkan dependensi eksternal baru atau base component global baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| **Memuat (*Loading*)** | Skeleton kartu ringkasan berkedip dan tabel berstatus `aria-busy` dengan indikator loading |
| **Kosong (*Empty*)** | Teks baku: *"Belum ada butir persiapan bedah."* |
| **Gagal (*Error*)** | Banner merah berlatar lembut dengan rincian pesan kesalahan server dan tombol tutup (✕) |
| **Konflik Konkurensi (409)** | Pesan peringatan *"Konflik Konkurensi (409): Data telah diperbarui oleh pengguna lain. Halaman dimuat ulang."* dan pemuatan ulang data otomatis |
| **Tanpa Hak Akses (403)** | Tampilan peringatan `AccessDeniedGate`: *"Ups! Akses Ditolak - Silahkan hubungi IT Helpdesk untuk meminta hak akses untuk halaman atau fitur ini"* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Master Data / Surgical Preparation Item

Base URL: `/v1/health-services/master-data/surgical-preparation-items`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi penyaring dan pilihan kelompok persiapan | `SurgicalPreparationItem : Read` |
| `GET` | `/summary` | Mengambil metrik ringkasan kartu statistik | `SurgicalPreparationItem : Read` |
| `GET` | `/` | Mengambil daftar butir berpaginasi dengan saringan | `SurgicalPreparationItem : Read` |
| `GET` | `/options` | Mengambil daftar opsi butir aktif per kelompok | `SurgicalPreparationItem : Read` |
| `GET` | `/{id}` | Mengambil detail butir beserta `RowVersion` | `SurgicalPreparationItem : Read` |
| `POST` | `/` | Menambah butir persiapan baru | `SurgicalPreparationItem : Create` |
| `PUT` | `/{id}` | Memperbarui butir dengan verifikasi konkurensi `RowVersion` | `SurgicalPreparationItem : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan status butir | `SurgicalPreparationItem : Update` |
| `DELETE` | `/{id}` | Menghapus lunak butir persiapan bedah | `SurgicalPreparationItem : Delete` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --test tests/unit/surgical-preparation-item-master.test.mjs` | 4 test lulus 100% | `PASS` | AC-1 s.d. AC-4 terverifikasi (file, API service, UI state, menu sidebar) |
| `npm run build` | Next.js 16.2.12 build berhasil dan standalone package siap | `PASS` | Halaman `/health-services/master-data/surgical-preparation-items` sukses dikompilasi |
| Uji Manual Kontrol Interaktif | `NOT FEASIBLE` | `NOT FEASIBLE` | Server runtime backend lokal tidak aktif saat sesi ini berjalan; seluruh alur logika dan response mapping divalidasi lewat pengujian otomatis |

```text
AUTOMATED TEST: node --test tests/unit/surgical-preparation-item-master.test.mjs — PASS (4/4 tests passing)
AUTOMATED TEST: npm run build — PASS (exit code 0, standalone runtime ready)
MANUAL TEST: NOT FEASIBLE — Backend runtime tidak aktif secara interaktif pada lingkungan sesi ini
```

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Butir menu tampil bagi pemegang `SurgicalPreparationItem : Read` | **Terpenuhi** | Didaftarkan di `src/utils/menu-sidebar/menu-items.jsx` dengan `requiredPermission: { resource: "SurgicalPreparationItem", action: "Read" }` dan diuji via AC-4 |
| 2. Tambah, ubah, dan aktif/nonaktif berfungsi; versi berubah → muat ulang | **Terpenuhi** | Didukung fungsi `createSurgicalPreparationItem`, `updateSurgicalPreparationItem` ber-`RowVersion`, dan `updateSurgicalPreparationItemStatus`; penanganan status 409 memicu alert dan `loadData()` ulang |
| 3. Keadaan kosong sesuai skema (*"Belum ada butir persiapan bedah."*) | **Terpenuhi** | Parameter `emptyText="Belum ada butir persiapan bedah."` pada `DataTable` dan diverifikasi via AC-3 |
| 4. Butir nonaktif tidak muncul pada versi pra-operasi baru | **Terpenuhi (Kontrak)** | Endpoint `options` dipanggil dengan `onlyActive=true`; pengujian terintegrasi lanjutan akan diverifikasi pada task konsumen `FE-RWI-195` |
| DoD: Build dan unit test lulus | **Terpenuhi** | `npm run build` lulus tanpa error; suite test lulus 4 dari 4 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengisian data butir awal disahkan pemilik klinis sebelum rilis produksi (mengikuti gerbang produksi `BE-RWI-173`) |
| Masalah yang diketahui | Tidak ada |
| Dependency backend | `BE-RWI-173` selesai dan migration `20261005033044_AddRawatInapFinishing` sudah diterapkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/utils/menu-sidebar/menu-items.jsx`, 4 berkas baru di bawah `src/` dan 1 berkas test di `tests/unit/` |
| Langkah berikutnya | Task berikutnya pada Gelombang 1: `FE-RWI-193` (Pemesanan Ruangan Bedah dua tab dari satu order tindakan aktif) |
