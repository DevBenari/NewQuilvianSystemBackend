# Laporan Perubahan Frontend — `FE-RWI-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-040` |
| Judul | Butir Administrasi Rawat Inap mengikuti Standar Master Data (Tanpa Modal, Berbasis Halaman Dedicated Table, Detail, Create, Update) |
| Slice | `F12 — Repair layar existing` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), task `FE-RWI-040` |
| Trace | Bukti runtime pemilik 28 Agustus 2026 & revisi 1 September 2026; standar arsitektur `Master Data Service Unit`; `FE-INP-13`; skema §18 dan §24.1; `FR-RI-142` s.d. `FR-RI-144` |
| Contract version | API Master Data `0.4.0`; enam endpoint clearance item (GET list, GET detail, POST, PUT, PATCH status, DELETE) berstatus tersedia. Source backend pada commit `44fd6ccba` menjadi bukti runtime as-is |
| Wewenang UI | Standar UI Master Data Health Services (seperti `Master Data Service Unit`): tabel utama tanpa modal, halaman detail dedicated berbasis `HealthServicesMasterDataDetailView`, formulir tambah/ubah berbasis `HealthServicesMasterDataEditorView` |
| Dependency | `BE-RWI-005` (endpoint clearance items) ✅ tersedia; `FE-RWI-033` (pemindahan menu ke Master Data) ✅ selesai. `RWI-UI-GAP-007` membatasi data awal environment, tetapi penambahan butir dari keadaan kosong terbukti berjalan |
| Klasifikasi | `MEDIUM` — Arsitektur modul diselaraskan 100% dengan standar Master Data Service Unit; tanpa perubahan backend, skema database, atau penanaman data tiruan |
| Task mode | `FRONTEND` — source backend strict read-only; wewenang lintas repository hanya berkas laporan ini, roadmap, dan traceability modul Rawat Inap |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; folder laporan, roadmap, dan traceability Rawat Inap pada `NewQuilvianSystemBackend` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Standar Master Data Service Unit diterapkan penuh: tabel tanpa modal, link tambah ke `/create`, klik ganda baris tabel membuka `/clearance-items/[slug]`, halaman detail menyediakan aksi Ubah (`/[slug]/update`), toggle status, dan Hapus (dengan konfirmasi alasan). `npm run lint` lulus dengan `0 errors` serta 573 warning existing; `npm run build` berhasil (`✓ Compiled successfully`, 246 static routes); unit test `tests/unit/inpatient-clearance-item.test.mjs` lulus 18/18 tes (`0 fails`). |

---

## 1. Keadaan yang ditemukan & Arahan Revisi Pemilik

Berdasarkan tinjauan standar master data sistem Quilvian (merujuk pada implementasi kanonikal `Master Data Service Unit`), modul master data **tidak menggunakan dialog modal** untuk formulir tambah/ubah maupun tampilan detail ringkasan. Setiap entitas master data memiliki 4 halaman navigasi terstruktur:
1. **Daftar Tabel (`/clearance-items`)**: Menampilkan ringkasan data, filter, pencarian, paginasi, dan tombol tambah berupa tautan ke halaman buat baru.
2. **Formulir Tambah (`/clearance-items/create`)**: Halaman dedicated dengan `HealthServicesMasterDataEditorView` dan live preview parameter.
3. **Detail Data (`/clearance-items/[slug]`)**: Halaman dedicated dengan `HealthServicesMasterDataDetailView` yang menyajikan detail lengkap, riwayat audit, tombol Ubah, tombol Status, dan tombol Hapus.
4. **Formulir Ubah (`/clearance-items/[slug]/update`)**: Halaman dedicated dengan `HealthServicesMasterDataEditorView` yang memuat data existing untuk diperbarui.

---

## 2. Proses Bisnis & Alur Penggunaan

### 2.1 Alur Penggunaan Standar Master Data

1. **Melihat Daftar Butir**:
   - Pengguna membuka menu `Pelayanan Kesehatan → Master Data → Butir Administrasi Rawat Inap` (`/health-services/inpatient-management/clearance-items`).
   - Tabel menampilkan kolom: `No`, `Tanggal Dibuat`, `Kode`, `Nama Butir Administrasi`, `Wajib`, `Urutan`, dan `Status`.
   - Jika data kosong dan pengguna memiliki izin `Create`, area filter tetap menyediakan tombol **`+ Tambah Butir Administrasi`** (`Link` ke `/create`).
2. **Menambah Butir Baru**:
   - Pengguna mengeklik **`+ Tambah Butir Administrasi`**, peramban berpindah ke `/health-services/inpatient-management/clearance-items/create`.
   - Formulir menampilkan isian: Kode Butir, Nama Butir, Keterangan, Switch Wajib, Urutan Tampil, dan Switch Aktif.
   - Sisi kanan menampilkan live preview. Pengguna menekan tombol Simpan, data dikirim via `POST /inpatient-clearance-items`, toast sukses muncul, dan peramban kembali ke daftar utama.
3. **Membuka Detail Butir**:
   - Pengguna mengeklik dua kali (*double click*) pada baris tabel yang diinginkan.
   - Peramban bernavigasi ke halaman detail aman `/health-services/inpatient-management/clearance-items/[slug]` (menggunakan token rute privat terdaftar).
   - Halaman detail menampilkan ringkasan data, sifat wajib, status, tanggal dibuat, tanggal diperbarui, serta pembuat/pengubah.
4. **Mengubah Butir**:
   - Dari halaman detail, pengguna menekan tombol **Ubah**, peramban berpindah ke `/health-services/inpatient-management/clearance-items/[slug]/update`.
   - Formulir memuat data yang ada dari server (`GET /inpatient-clearance-items/{id}`). Pengguna melakukan perubahan lalu menekan Simpan (`PUT /inpatient-clearance-items/{id}`).
5. **Menghapus Butir**:
   - Dari halaman detail, pengguna menekan tombol **Hapus**.
   - Modal konfirmasi merah meminta alasan penghapusan. Setelah dikonfirmasi, sistem mengirim `DELETE /inpatient-clearance-items/{id}`, toast sukses muncul, dan peramban kembali ke halaman daftar.
6. **Penanganan Eksepsi & Konflik (409 Conflict)**:
   - Jika kode butir kembar ditolak oleh server, form **mempertahankan seluruh isian** dan menampilkan notifikasi kesalahan spesifik tanpa kehilangan data yang sudah diketik pengguna.

---

## 3. Struktur Berkas & Komponen

### 3.1 Berkas Frontend yang Diubah & Dibuat

| Berkas | Peran / Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-clearance-item-view.jsx` | Tampilan tabel utama master data (mengikuti `master-data-service-unit-view.jsx` tanpa modal form/detail) |
| `src/components/view/health-services/inpatient-management/detail/inpatient-clearance-item-detail-view.jsx` | Halaman detail dedicated berbasis `HealthServicesMasterDataDetailView` |
| `src/components/view/health-services/inpatient-management/add/inpatient-clearance-item-form-view.jsx` | Halaman formulir tambah/ubah berbasis `HealthServicesMasterDataEditorView` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-items.jsx` | Hook tabel daftar master data (filter, sorting, paginasi, navigasi `openCreate` dan `openDetail`) |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-item-detail.jsx` | Hook halaman detail (pembacaan detail, aksi navigasi update, delete dengan alasan, toggle status) |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-item-editor.jsx` | Hook formulir tambah/ubah (state form, validasi, submit create/update, retensi form saat konflik 409) |
| `src/lib/constants/health-services/inpatient-management/inpatient-clearance-item-constants.jsx` | Metadata tabel, editor config, token scope, dan opsi filter |
| `src/utils/health-services/inpatient-management/inpatient-clearance-item-utils.jsx` | Helper format tanggal, detail rows, route token resolver, dan permission resolver |
| `src/app/health-services/inpatient-management/clearance-items/page.jsx` | Rute App Router untuk tabel master data |
| `src/app/health-services/inpatient-management/clearance-items/create/page.jsx` | Rute App Router untuk halaman tambah |
| `src/app/health-services/inpatient-management/clearance-items/[slug]/page.jsx` | Rute App Router untuk halaman detail |
| `src/app/health-services/inpatient-management/clearance-items/[slug]/update/page.jsx` | Rute App Router untuk halaman ubah |

### 3.2 Gerbang Keputusan Base Component (UI Gate)

| Kebutuhan UI | Base Component | Keputusan |
| :--- | :--- | :--- |
| Shell Halaman Master | `Hero` + shell token Administrator | `REUSE` |
| Penyaring & Pencarian | `DataFilter` + `FilterSelect` | `REUSE` |
| Tabel & Paginasi | `DataTable` + `RegionPagination` | `REUSE` |
| Badge Status | `StatusBadge` | `REUSE` |
| Detail Master Data | `HealthServicesMasterDataDetailView` | `REUSE` |
| Editor Master Data | `HealthServicesMasterDataEditorView` | `REUSE` |
| Modal Konfirmasi Hapus | `ConfirmModal` (pada halaman detail) | `REUSE` |
| Notifikasi Toast | `ToastStack` | `REUSE` |

`UI GATE: 8 elemen — REUSE 8, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0.`

---

## 4. Endpoint yang Dikonsumsi

Tag: `Health Services / Master Data / Inpatient Clearance Item`  
Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Deskripsi | Hak Akses | Request | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/` | Mengambil daftar master data butir | `InpatientClearanceItem : Read` | Query params (search, mandatory, active, page, size) | `ApiResponse<PagedResult<InpatientClearanceItemResponse>>` |
| `GET` | `/{id}` | Mengambil detail lengkap butir | `InpatientClearanceItem : Read` | Path param `id` (GUID) | `ApiResponse<InpatientClearanceItemResponse>` |
| `POST` | `/` | Menambah butir baru | `InpatientClearanceItem : Create` | Body `CreateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `PUT` | `/{id}` | Memperbarui data butir | `InpatientClearanceItem : Update` | Body `UpdateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `PATCH` | `/{id}/status` | Mengaktifkan/menonaktifkan butir | `InpatientClearanceItem : Update` | Body `UpdateInpatientClearanceItemStatusRequest` | `ApiResponse<InpatientClearanceItemResponse>` |
| `DELETE` | `/{id}` | Menghapus butir (soft delete) | `InpatientClearanceItem : Delete` | Body `DeleteInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` |

---

## 5. Hasil Verifikasi

| Skenario / Perintah | Hasil | Klasifikasi |
| :--- | :--- | :--- |
| `npm.cmd run lint` | Exit `0`; `0 errors`, 573 warning baseline | `PASS` |
| `npm.cmd run build` | Next.js build sukses; 246 route terkompilasi | `PASS` |
| `node --test tests/unit/inpatient-clearance-item.test.mjs` | 18 test pass, 0 fail (164 ms) | `PASS` |
| Ketiadaan Modal pada Halaman Tabel | Terverifikasi bebas dari modal form/detail | `PASS` |
| Navigasi Dedicated Page | `/create`, `/[slug]`, `/[slug]/update` siap dan terdaftar | `PASS` |

Semua kriteria dan standar arsitektur master data telah terpenuhi secara menyeluruh.
