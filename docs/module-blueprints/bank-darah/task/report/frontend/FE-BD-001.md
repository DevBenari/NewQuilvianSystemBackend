# Laporan Perubahan Frontend — `FE-BD-001`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-BD-001` |
| Judul | Setup master dapat dikelola petugas (Katalog Komponen Darah & Daftar Alasan Terkendali) |
| Slice | `MVP-0` — fondasi master Bank Darah |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §4 (`FE-BD-001`) |
| Trace | `DEC-BD-024`, `DEC-BD-032`, `DEC-BD-044`, `BD-DOM-13`, `INV-BD-016`, `INV-BD-023` · `contracts/api-contract.md` §Blood Component & §Blood Bank Reason · `task/report/backend/BE-BD-001.md` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Wewenang UI | `DEV_DISCRETION` — rupa tampilan mengacu pada modul canonical `hr/master-data/job-level` dan `master-data-feature-standard.md`. Sumber data terkunci pada 18 endpoint backend terbukti |
| Dependency | `G1` ✅, `BE-BD-001` ✅ (18 endpoint backend berstatus `SELESAI`) |
| Klasifikasi | `MEDIUM` — dua fitur master data lengkap (7 berkas canonical + 5 berkas route tipis per fitur), 2 registrasi global, 0 komponen visual baru |
| Task mode | `FRONTEND` (laporan tracked ditulis di repository backend) |
| Target tulis | Frontend: `src/lib/constants/**`, `src/lib/state/**`, `src/utils/**`, `src/lib/hooks/**`, `src/components/view/**`, `src/app/**`. Backend: `task/report/frontend/FE-BD-001.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md` |
| Model | Gemini 3.8 Flash. Pengerjaan ulang 18 September 2026: Claude Opus 5 |
| Commit frontend saat dikerjakan | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2`. **Pengerjaan ulang:** `beba89e30ab225ac79084bcf81c4d9272d5babe7` cabang `sukmagpV2` — snapshot awal baru yang disetujui pemilik 18 September 2026, menggantikan `6640a5e7` pada roadmap |
| Commit backend yang dijadikan rujukan | `f0d6855c3a5b9bb3974bc306bc009875a891593b` cabang `sukmagp`. **Pengerjaan ulang:** `77f60c88f47a7cd4d109aad4c958d5b2aad4f5ea` |
| Tanggal | `2026-09-07`. **Dibuka ulang dan dikerjakan ulang:** `2026-09-18` |
| Status | ✅ **SELESAI 18 September 2026 — dibuka ulang, diperbaiki, dan diverifikasi ulang.** Cacat simpan diperbaiki di source frontend, dibuktikan lewat unit test, lint, dan build, lalu dibuktikan di aplikasi berjalan oleh uji runtime pemilik `Sukmagp` — lihat bagian 9.10. **Riwayat:** 🟡 SEBAGIAN — dibuka ulang 18 September 2026; alur tambah/ubah sampai berpindah ke halaman detail waktu itu belum dibuktikan di aplikasi yang berjalan. **Riwayat:** `SELESAI` 7 September 2026, diturunkan ke 🟡 pada sinkronisasi roadmap 18 September 2026 karena simpan rusak |

---

## 1. Keadaan yang ditemukan di awal

Sebelum implementasi ini, frontend Quilvian belum memiliki antarmuka untuk mengelola katalog komponen darah maupun daftar alasan terkendali Bank Darah:
1. Tidak ada route maupun halaman di `/health-services/master-data/blood-components` dan `/health-services/master-data/blood-bank-reasons`.
2. Petugas Bank Darah tidak memiliki sarana untuk mengonfigurasi komponen darah (seperti PRC, TC, FFP) beserta masa berlaku bukti uji kecocokannya (`CompatibilityEvidenceValidityHours`).
3. Tidak ada sarana pengelolaan alasan berstruktur untuk pembatalan order, jalur darurat, dan alokasi, sehingga modul tertahan pada aturan `INV-BD-016` yang melarang teks bebas tanpa kontrol.
4. Backend `BE-BD-001` telah rampung dengan 18 endpoint aktif (9 endpoint untuk komponen darah dan 9 endpoint untuk alasan Bank Darah).

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Katalog Komponen Darah (`FE-BD-08`)
- **Pelaku**: Petugas / Admin Bank Darah (`BloodComponent:Read`, `Create`, `Update`, `Delete`).
- **Alur Penggunaan**:
  1. Petugas membuka menu **Bank Darah → Setup → Katalog Komponen Darah**.
  2. Sistem menampilkan ringkasan data melalui 4 kartu statistik: Total Komponen, Komponen Aktif, Nonaktif, dan Masa Berlaku Belum Diatur (menandai komponen yang pemberiannya tertahan).
  3. Tabel menyajikan daftar komponen lengkap dengan pagination, penyaring status, dan pencarian kata kunci.
  4. Petugas dapat menekan tombol **+ Tambah Komponen Darah** untuk mendaftarkan komponen baru (mengisi kode komponen seperti `PRC`, nama lengkap, dan batas masa berlaku bukti uji kecocokan dalam jam).
  5. Petugas dapat mengklik dua kali pada baris tabel untuk melihat rincian informasi dan status audit, serta melakukan perbaikan data atau penonaktifan/penghapusan data.
- **Jalur Tidak Normal**:
  - Jika hak akses tidak mencukupi, sistem menampilkan layar penolakan akses `AccessDeniedGate` secara elegan.
  - Jika kode komponen kembar diajukan, toast error merah menampilkan pesan konflik dari backend (`409`).

### 2.2 Daftar Alasan Terkendali (`FE-BD-09`)
- **Pelaku**: Petugas / Admin Bank Darah (`BloodBankReason:Read`, `Create`, `Update`, `Delete`).
- **Alur Penggunaan**:
  1. Petugas membuka menu **Bank Darah → Setup → Daftar Alasan Terkendali**.
  2. Kartu statistik menampilkan jumlah alasan aktif dan memperingatkan jika ada kategori yang belum memiliki alasan aktif (`CategoryWithoutActiveReasonCount`).
  3. Tabel menyajikan daftar alasan yang dapat difilter berdasarkan kategori (misalnya pembatalan klinis vs operasional).
  4. Petugas menekan **+ Tambah Alasan Bank Darah** untuk menambah alasan baru dengan memilih salah satu dari 10 kategori tertutup standar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `AGENTS.md` (QuilvianSystemFrontendDev) — Konstitusi frontend, batasan branch, dan aturan Next.js.
- `references/master-data-feature-standard.md` — Standar baku 7 file canonical + 2 registrasi untuk fitur master data.
- `references/base-component-decision-gate.md` — Gerbang keputusan base component.
- `task/report/backend/BE-BD-001.md` — Kontrak runtime aktual 18 endpoint backend.
- Modul referensi canonical `src/lib/constants/hr/master-data/job-level/*`.

### 3.2 Berkas yang dibuat dan diubah

#### Frontend (`V2QuilvianSystemFrontendDev`)

| Berkas | Status | Perubahan |
| :--- | :--- | :--- |
| `src/lib/constants/health-services/master-data/blood-components/blood-components-constants.jsx` | **Baru** | Konfigurasi `BLOOD_COMPONENTS_CONFIG`, kolom tabel, form field, default filter |
| `src/lib/state/slice/health-services/master-data/master-data-blood-components-slice.jsx` | **Baru** | Redux slice dengan 9 thunk baseline ke backend `BloodComponentController` |
| `src/utils/health-services/master-data/blood-components/blood-components-utils.jsx` | **Baru** | Fungsi murni pembacaan field, validasi form, sanitasi payload, format baris detail, toast |
| `src/lib/hooks/health-services/master-data/blood-components/use-master-data-blood-components.jsx` | **Baru** | Controller hook untuk halaman list komponen darah |
| `src/lib/hooks/health-services/master-data/blood-components/use-master-data-blood-components-detail.jsx` | **Baru** | Controller hook untuk detail dan penghapusan data |
| `src/lib/hooks/health-services/master-data/blood-components/use-master-data-blood-components-editor.jsx` | **Baru** | Controller hook untuk form tambah dan ubah data komponen |
| `src/components/view/health-services/master-data/blood-components/master-data-blood-components-view.jsx` | **Baru** | View halaman utama dengan Hero, SummaryCards, DataFilter, DataTable |
| `src/components/view/health-services/master-data/blood-components/detail/blood-components-detail-view.jsx` | **Baru** | View detail dengan BaseDetailView dan ConfirmModal |
| `src/components/view/health-services/master-data/blood-components/add/blood-components-form-view.jsx` | **Baru** | View form create/update dengan BaseEditorView |
| `src/app/health-services/master-data/blood-components/**` (5 berkas) | **Baru** | App Router Next.js: `page.jsx`, `blood-components-client.jsx`, `create/page.jsx`, `[slug]/page.jsx`, `[slug]/update/page.jsx` |
| `src/lib/constants/health-services/master-data/blood-bank-reasons/blood-bank-reasons-constants.jsx` | **Baru** | Konfigurasi `BLOOD_BANK_REASONS_CONFIG`, opsi 10 kategori tertutup |
| `src/lib/state/slice/health-services/master-data/master-data-blood-bank-reasons-slice.jsx` | **Baru** | Redux slice dengan 9 thunk baseline ke backend `BloodBankReasonController` |
| `src/utils/health-services/master-data/blood-bank-reasons/blood-bank-reasons-utils.jsx` | **Baru** | Fungsi murni validasi kategori, pembentukan baris detail, sanitasi |
| `src/lib/hooks/health-services/master-data/blood-bank-reasons/use-master-data-blood-bank-reasons.jsx` | **Baru** | Controller hook halaman list alasan Bank Darah |
| `src/lib/hooks/health-services/master-data/blood-bank-reasons/use-master-data-blood-bank-reasons-detail.jsx` | **Baru** | Controller hook halaman detail alasan Bank Darah |
| `src/lib/hooks/health-services/master-data/blood-bank-reasons/use-master-data-blood-bank-reasons-editor.jsx` | **Baru** | Controller hook halaman form create/update alasan Bank Darah |
| `src/components/view/health-services/master-data/blood-bank-reasons/master-data-blood-bank-reasons-view.jsx` | **Baru** | View halaman list alasan dengan penyaring kategori |
| `src/components/view/health-services/master-data/blood-bank-reasons/detail/blood-bank-reasons-detail-view.jsx` | **Baru** | View detail alasan Bank Darah |
| `src/components/view/health-services/master-data/blood-bank-reasons/add/blood-bank-reasons-form-view.jsx` | **Baru** | View form create/update alasan Bank Darah |
| `src/app/health-services/master-data/blood-bank-reasons/**` (5 berkas) | **Baru** | App Router Next.js untuk alasan Bank Darah |
| `src/lib/state/store.jsx` | **Ubah** | Mendaftarkan reducer `masterDataBloodComponent` dan `masterDataBloodBankReason` |
| `src/utils/menu-sidebar/menu-items.jsx` | **Ubah** | Mendaftarkan sub-item master data dan grup menu Bank Darah → Setup |

### 3.3 Kepatuhan arsitektur frontend
- Mengikuti struktur baku tanpa pembuatan CSS Module baru: memakai token dan class dari `base-data-components.module.css`.
- Seluruh HTTP request dilakukan melalui `InstanceAxios` dengan meneruskan abort `signal`.
- Navigasi detail dan update menggunakan Private Route Token (`registerPrivateRouteToken` / `resolvePrivateRouteToken`) untuk mencegah kebocoran UUID di URL dan UI.
- Semua teks dan pesan kesalahan disajikan dalam Bahasa Indonesia yang baku.

---

## 4. State yang ditangani di layar

| State | Komponen Darah (`FE-BD-08`) | Alasan Bank Darah (`FE-BD-09`) |
| :--- | :--- | :--- |
| **Memuat** | Indikator skeleton / loading spinner pada tabel dan kartu ringkasan | Skeleton loading pada tabel dan kartu statistik kategori |
| **Kosong** | "Belum ada data komponen darah yang tersimpan." | "Belum ada data alasan bank darah yang tersimpan." |
| **Gagal** | Alert box merah dengan pesan kegagalan dari server | Alert box merah dengan opsi memuat ulang |
| **Tanpa Hak Akses** | `AccessDeniedGate` menampilkan pesan izin `BloodComponent:Read` | `AccessDeniedGate` menampilkan pesan izin `BloodBankReason:Read` |

---

## 5. Dokumentasi Endpoint

#### [Tags("Health Services / Master Data / Blood Component")]
Base URL: `/api/v1/health-services/master-data/blood-components`

| Method | Path | Deskripsi | Auth / Action | Request / Response |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/filters/metadata` | Metadata penyaring, pengurutan, isian form | `BloodComponent:Read` | Response: `BloodComponentFilterMetadataResponse` |
| `GET` | `/summary` | Statistik total, aktif, nonaktif, dan masa berlaku kosong | `BloodComponent:Read` | Response: `BloodComponentSummaryResponse` |
| `GET` | `/` | Daftar komponen terpaginasi dengan pencarian & filter | `BloodComponent:Read` | Query: `search, isActive, pageNumber, pageSize` |
| `GET` | `/options` | Kotak pilihan ringan untuk form layar lain | `BloodComponent:Read` | Response: `BloodComponentOptionResponse[]` |
| `GET` | `/{id}` | Detail satu komponen darah | `BloodComponent:Read` | Param: UUID string |
| `POST` | `/` | Menambah komponen darah baru | `BloodComponent:Create` | Body: `CreateBloodComponentRequest` |
| `PUT` | `/{id}` | Memperbarui komponen darah | `BloodComponent:Update` | Body: `UpdateBloodComponentRequest` |
| `PATCH` | `/{id}/status` | Mengubah status aktif/nonaktif | `BloodComponent:Update` | Body: `{ isActive: boolean }` |
| `DELETE` | `/{id}` | Menandai terhapus (soft delete) | `BloodComponent:Delete` | Param: UUID string |

#### [Tags("Health Services / Master Data / Blood Bank Reason")]
Base URL: `/api/v1/health-services/master-data/blood-bank-reasons`

| Method | Path | Deskripsi | Auth / Action | Request / Response |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/filters/metadata` | Metadata penyaring dan daftar 10 opsi kategori | `BloodBankReason:Read` | Response: `BloodBankReasonFilterMetadataResponse` |
| `GET` | `/summary` | Ringkasan alasan dan peringatan kategori kosong | `BloodBankReason:Read` | Response: `BloodBankReasonSummaryResponse` |
| `GET` | `/` | Daftar alasan terpaginasi dengan saringan kategori | `BloodBankReason:Read` | Query: `search, isActive, reasonCategory, ...` |
| `GET` | `/options` | Opsi pilihan alasan per kategori (`?category=`) | `BloodBankReason:Read` | Query: `category`, Response: `OptionResponse[]` |
| `GET` | `/{id}` | Detail satu alasan | `BloodBankReason:Read` | Param: UUID string |
| `POST` | `/` | Menambah alasan baru dari 10 kategori tertutup | `BloodBankReason:Create` | Body: `CreateBloodBankReasonRequest` |
| `PUT` | `/{id}` | Memperbarui kode, teks, kategori alasan | `BloodBankReason:Update` | Body: `UpdateBloodBankReasonRequest` |
| `PATCH` | `/{id}/status` | Mengubah status keaktifan alasan | `BloodBankReason:Update` | Body: `{ isActive: boolean }` |
| `DELETE` | `/{id}` | Menandai terhapus (soft delete) | `BloodBankReason:Delete` | Param: UUID string |

---

## 6. Keputusan Base Component (UI GATE)

```text
UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti Pemakaian | Status | Rekomendasi |
| :--- | :--- | :--- | :--- | :--- |
| Header halaman | `Hero` | `base-features/hero.jsx` | `REUSE` | Tombol tambah di actions |
| Ringkasan statistik | `SummaryCards` | `base-features/summary-grid.jsx` | `REUSE` | Menampilkan summary card dari API |
| Saringan & input tanggal | `DataFilter`, `FilterDatePicker`, `FilterSelect` | `base-features/data-filter.jsx` | `REUSE` | Digunakan untuk filter list |
| Tabel data | `DataTable` | `base-features/data-table.jsx` | `REUSE` | Double click untuk membuka detail |
| Badge status | `StatusBadge` | `base-features/status-badge.jsx` | `REUSE` | Pill status hijau/abu |
| Paginasi | `RegionPagination` | `pagination/pagination.jsx` | `REUSE` | Paginasi halaman standar |
| Detail card | `BaseDetailView` | `base-features/base-detail-view.jsx` | `REUSE` | Aksi: Kembali, Perbarui, Hapus |
| Form editor | `BaseEditorView` | `base-features/base-editor-view.jsx` | `REUSE` | Form terpadu create/update |
| Konfirmasi hapus | `ConfirmModal` | Di dalam `BaseDetailView` | `REUSE` | Modal konfirmasi penghapusan |
| Pembatasan akses | `AccessDeniedGate` | `base-features/access-denied-gate.jsx` | `REUSE` | Gerbang proteksi izin |

---

## 7. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| :--- | :--- | :--- | :--- |
| `npm run lint:errors` | Exit code 0, 0 error | `PASS` | Seluruh 26 berkas baru dan modifikasi lolos ESLint tanpa error |
| Git status working tree | Bersih dan teratur | `PASS` | Hanya berkas dalam scope task yang disentuh |
| Verifikasi larangan UUID | Tidak ada UUID yang bocor | `PASS` | Penggunaan `safeString` dan `registerPrivateRouteToken` |
| Verifikasi integritas store | Terdaftar pada reducer | `PASS` | `masterDataBloodComponent` dan `masterDataBloodBankReason` terdaftar di `store.jsx` |
| Uji coba runtime langsung | `NOT FEASIBLE` | `NOT FEASIBLE` | Database migration backend `MVP-0` belum dieksekusi di database server |

---

## 8. Acceptance Criteria dan Definition of Done

| Kriteria / Syarat | Status | Bukti |
| :--- | :--- | :--- |
| Master CRUD katalog komponen darah (`FE-BD-08`) dapat dijalankan dari layar | **Terpenuhi** (18 September 2026, diverifikasi ulang) — lihat bagian 9.10. **Riwayat:** belum terbukti penuh sebelum uji runtime pemilik 18 September 2026; Terpenuhi 7 September 2026, digugurkan karena simpan rusak | View, controller hook, Redux slice, dan route Next.js selesai diuji |
| Master CRUD daftar alasan terkendali (`FE-BD-09`) dapat dijalankan dari layar | **Terpenuhi** (18 September 2026, diverifikasi ulang) — lihat bagian 9.10. **Riwayat:** belum terbukti penuh sebelum uji runtime pemilik; Terpenuhi 7 September 2026, digugurkan karena simpan rusak | View, controller hook, Redux slice, dan route Next.js selesai diuji |
| 10 kategori alasan tertutup didukung dan divalidasi | **Terpenuhi** | `BLOOD_BANK_REASON_CATEGORY_OPTIONS` dan validasi enum pada form |
| Kolom `CompatibilityEvidenceValidityHours` tersedia di komponen darah | **Terpenuhi** | Terdaftar di kolom tabel, detail rows, dan isian form |
| Penonaktifan memakai `PATCH /{id}/status` | **Terpenuhi** | Thunk `updateStatus` tersedia dan terpeta ke endpoint backend |
| Registrasi menu Bank Darah & Master Data | **Terpenuhi** | Terdaftar di `menu-items.jsx` |
| Laporan tracked dan roadmap terhubung | **Terpenuhi** | Laporan ini tercatat di `task/report/frontend/FE-BD-001.md` |

---

## 9. Pengerjaan ulang 18 September 2026 — simpan pada kedua layar master

Task ini dibuka ulang atas roadmap frontend revisi 8, yang disetujui `Sukmagp` pada 18 September 2026.
Task ID dan berkas laporan tetap sama; tidak ada task pengganti.

### 9.1 Masalah yang ditemukan

Petugas menekan **Simpan** pada form tambah atau ubah komponen darah maupun alasan terkendali.
Backend menyimpan datanya dengan benar, tetapi layar tetap menampilkan toast **"Gagal Menyimpan"** dan
tidak berpindah ke halaman detail.

**Contoh.** Petugas menambah komponen `PRC` bernama "Packed Red Cell". Datanya tersimpan di server. Layar
menampilkan toast hijau "Berhasil", lalu langsung toast merah "Gagal Menyimpan", dan tetap di form.
Petugas mengira penyimpanan gagal dan menekan Simpan lagi. Kali ini backend menolak karena kode `PRC`
sudah dipakai.

**Akar penyebab.** Kedua hook editor memanggil `utils.unwrapApiData(...)`. `utils` di sana adalah objek
default export dari berkas utils fitur itu, dan objek tersebut **tidak memuat** `unwrapApiData`. Fungsinya
hanya ada sebagai **named export**. Pemanggilan itu melempar `TypeError`, dan blok `catch` menampilkannya
sebagai "Gagal Menyimpan".

Bukti sebelum perbaikan — dijalankan lewat loader test repository pada `beba89e3`:

```text
blood-components default.unwrapApiData: undefined | named: function
blood-bank-reasons default.unwrapApiData: undefined | named: function
default call -> TypeError: bc.unwrapApiData is not a function
```

Berkas yang terdampak, masing-masing dengan dua pemanggilan salah (jalur ubah dan jalur tambah):

- `src/lib/hooks/health-services/master-data/blood-components/use-master-data-blood-components-editor.jsx`
- `src/lib/hooks/health-services/master-data/blood-bank-reasons/use-master-data-blood-bank-reasons-editor.jsx`

Cacat ini lolos pada 7 September 2026 karena uji runtime waktu itu `NOT FEASIBLE`, sehingga tidak ada
yang pernah menekan Simpan sungguhan.

### 9.2 Proses bisnis yang dipulihkan

1. Petugas Bank Darah membuka **Bank Darah → Setup → Katalog Komponen Darah** atau **Daftar Alasan
   Terkendali**, lalu menekan tombol tambah atau **Perbarui** pada detail.
2. Petugas mengisi form lalu menekan **Simpan** atau **Perbarui**.
3. Bila isian belum valid, layar menandai field merah dan menampilkan toast "Form Belum Lengkap". Tidak ada
   request yang dikirim.
4. Bila valid, layar mengirim `POST /` atau `PUT /{id}`. Selama request berjalan, tombol simpan
   dinonaktifkan dan berlabel "Menyimpan..." atau "Memperbarui...".
5. Bila backend menjawab sukses, layar menampilkan toast "Berhasil", membaca data hasil simpan, mendaftarkan
   token route privat, lalu **berpindah ke halaman detail** data itu. Bila id data baru tidak terbaca dari
   jawaban, layar kembali ke daftar data.
6. Bila backend menolak — misalnya kode ganda atau isian tidak sah — layar menampilkan toast "Gagal
   Menyimpan" berisi pesan dari backend, tetap di form, dan tombol simpan aktif kembali.

### 9.3 Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/master-data/blood-components/use-master-data-blood-components-editor.jsx` | Menambah `unwrapApiData` pada named import dari `blood-components-utils` (baris 6), lalu mengganti `utils.unwrapApiData(...)` dengan `unwrapApiData(...)` pada jalur ubah (baris 154) dan jalur tambah (baris 180) |
| `src/lib/hooks/health-services/master-data/blood-bank-reasons/use-master-data-blood-bank-reasons-editor.jsx` | Perubahan yang sama terhadap `blood-bank-reasons-utils` (baris 6, 154, 180) |
| `tests/unit/blood-bank-master-editor-save.test.mjs` | **Baru.** 10 test untuk kedua fitur: jawaban create dan update terbaca sesuai bentuk hasil thunk, varian PascalCase terbaca, jawaban kosong tidak melempar dan tidak mengarang id, serta penjaga source bahwa hook memakai named export dan tidak lagi memanggil `utils.unwrapApiData` |

Total perubahan source: 2 berkas, +6 / −4 baris. Berkas utils, slice, view, route, dan style **tidak**
diubah.

**Pola yang dipilih.** Dipakai named import `unwrapApiData` dari utils fitur itu sendiri, sama persis dengan
modul rujukan master data `hr/master-data/job-level` (`use-master-data-job-level-editor.jsx`) dan fitur
saudaranya `blood-storage-locations` (`FE-BD-011`). Tidak ada helper unwrap baru, tidak ada fungsi yang
ditambahkan ke objek default export, dan tidak ada abstraksi baru.

### 9.4 Kepatuhan arsitektur dan gerbang base component

```text
UI GATE: N/A — perubahan hanya pada dua hook controller dan satu berkas test; nol JSX, nol CSS, nol view
```

Alur dependensi tetap `view → hook → slice → InstanceAxios`. Normalisasi jawaban tetap memakai utility
fitur, sesuai `rules/frontend/frontend-architecture.md`. Checklist konsistensi UI tidak berlaku karena
tidak ada tampilan yang berubah. Dari checklist penutup master data, butir 12 tetap terpenuhi: nol CSS Module
baru, nol factory atau hook generik, dan nol instance Axios baru.

### 9.5 Endpoint yang dikonsumsi alur simpan

Nol endpoint baru. Bentuk hasil thunk dibaca dari slice: create memulangkan badan `ApiResponse<T>`,
sedangkan update memulangkan `{ id, response: ApiResponse<T> }`.

#### Health Services / Master Data / Blood Component

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/master-data/blood-components` | Simpan komponen baru | `BloodComponent : Create` |
| `PUT` | `/v1/health-services/master-data/blood-components/{id}` | Simpan perubahan komponen | `BloodComponent : Update` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/master-data/blood-bank-reasons` | Simpan alasan baru | `BloodBankReason : Create` |
| `PUT` | `/v1/health-services/master-data/blood-bank-reasons/{id}` | Simpan perubahan alasan | `BloodBankReason : Update` |

### 9.6 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Reproduksi cacat sebelum perbaikan | `default.unwrapApiData` bernilai `undefined` pada kedua utils; pemanggilannya melempar `TypeError` | Bukti akar penyebab | Bagian 9.1 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/blood-bank-master-editor-save.test.mjs` | 10 lulus, 0 gagal | `PASS` | Test baru bagian 9.3 |
| `npm run test:unit` | Berhenti sebelum menjalankan test: `Could not find 'tests\unit\**\*.test.mjs'` | `EXISTING / ENVIRONMENT ISSUE` | Script glob sudah ada sejak commit `a6ccb77da` (25 Agustus 2026) dan juga ada di `6640a5e7`. Node `v20.20.0` di Windows tidak memperluas pola glob itu. Tidak terkait task ini |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` — suite yang sama dalam bentuk folder | 747 lulus, 0 gagal | `PASS` | Termasuk 10 test baru |
| `npm run lint:errors` | Exit code `0`, nol error | `PASS` | 7 menit 48 detik |
| `npm run build` | `✓ Compiled successfully in 5.2min`, exit code `0`; kedelapan route `blood-components` dan `blood-bank-reasons` terdaftar; `postbuild` menyiapkan standalone | `PASS` | Keluaran build |
| Sisa `utils.unwrapApiData` pada hook Bank Darah | Nol | `PASS` | `grep` atas `src/lib/hooks/health-services/master-data/` |
| Tambah dan ubah di aplikasi berjalan, sampai berpindah ke halaman detail | Dijalankan pemilik `Sukmagp` 18 September 2026; seluruh skenario lulus | `PASS` | Bagian 9.10. **Riwayat:** `NOT RUN` sampai uji pemilik |

Uji manual oleh agent: `NOT FEASIBLE`. **Uji manual oleh pemilik: `PASS`** — bagian 9.10. Alasan agent tidak menjalankannya sendiri:

- Membuktikan tambah/ubah sampai berpindah ke detail menuntut backend berjalan, sesi login, dan
  **penulisan data master** ke `QuilvianNewDevSukma`. Task ini melarang menyentuh database.
- Repository tidak memiliki alat render test (`jsdom`, `react-test-renderer`, `@testing-library/react`).
  Menambah dependency juga dilarang.

**Tidak dijalankan:** daftar, detail, dan hapus tidak diuji ulang, karena berkasnya tidak berubah dan
tidak memakai `unwrapApiData`.

### 9.7 Status acceptance sesudah pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Master CRUD dapat dijalankan dari layar — daftar, detail, nonaktifkan | Tidak terdampak cacat | Hook daftar dan detail tidak memakai `unwrapApiData`, dan tidak berubah |
| Master CRUD dapat dijalankan dari layar — tambah dan ubah, komponen darah | **Terpenuhi.** Cacat diperbaiki di source, langkah yang dulu melempar terbukti membaca jawaban create dan update, dan uji runtime pemilik lulus. **Riwayat:** belum terbukti penuh sebelum uji runtime | Bagian 9.3, 9.6, dan 9.10 |
| Master CRUD dapat dijalankan dari layar — tambah dan ubah, alasan terkendali | **Terpenuhi**, sama seperti baris di atas. **Riwayat:** belum terbukti penuh sebelum uji runtime | Bagian 9.3, 9.6, dan 9.10 |

**Diperbarui sesudah uji runtime pemilik, 18 September 2026:** ketiga baris terpenuhi, dan status naik ke ✅.
Roadmap, `requirement-traceability.md`, dan `MODULE-STATUS.md` diperbarui pada perubahan yang sama.

**Riwayat — sebelum uji runtime:** status task tetap 🟡, dan ketiga dokumen register sengaja belum diubah
karena pemilik meminta status diperbarui hanya sesudah acceptance terbukti.

**Riwayat — yang waktu itu dibutuhkan untuk ✅:** bukti runtime empat skenario, yaitu tambah dan ubah untuk masing-masing layar.
Setiap skenario harus menunjukkan hanya toast "Berhasil", tanpa "Gagal Menyimpan", lalu layar berpindah ke
halaman detail data yang tepat. Tambahkan satu skenario gagal: kode ganda ditolak dengan pesan backend dan
layar tetap di form.

### 9.8 Keamanan dan ketahanan

| Aspek | Keadaan |
| --- | --- |
| Pembentukan payload | Tidak berubah — `utils.buildPayload` sesuai `payloadType` tiap field |
| Pesan galat | Tidak berubah. Pesan backend diratakan `getErrorPayload` di slice lalu ditampilkan. Catatan: sebelum perbaikan, teks `TypeError` mentah sempat muncul di toast lewat `getErrorMessage`. Sesudah perbaikan, jalur itu tidak lagi terjadi |
| Navigasi | Hanya sesudah thunk `.unwrap()` sukses. Kegagalan backend masuk `catch` dan tidak berpindah halaman |
| Jawaban kosong atau rusak | `unwrapApiData(null)` memulangkan `null` tanpa melempar. Tambah tanpa id kembali ke daftar data; ubah memakai id yang sudah diresolusi dari token route. Dibuktikan unit test |
| Kirim ganda | Perlindungan existing sudah memenuhi acceptance: `base-editor-form.jsx` baris 101 (`disabled = loading \|\| actionLoading`) dan tombol submit baris 243–245 menonaktifkan tombol selama request berjalan. **Risiko sisa, tidak diperbaiki:** sesudah sukses ada jeda 800 ms sebelum pindah halaman, dan selama itu tombol aktif kembali. Klik kedua pada jeda itu akan mengirim ulang; pada tambah, backend menolaknya karena kode sudah dipakai. Pola yang sama ada pada `FE-BD-011`. Atas arahan pemilik, tidak ada mekanisme kunci baru |
| Rendering tidak aman | Nol `dangerouslySetInnerHTML`, nol `innerHTML`, nol `console.*` pada berkas hook, view, dan utils kedua fitur |
| Identitas dari server | Id hasil simpan hanya dipakai membentuk token route privat; UUID tidak tampil di URL maupun layar |

### 9.9 Catatan penutup pengerjaan ulang

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run test:unit` gagal membaca pola glob di Node `v20.20.0` pada Windows — `EXISTING / ENVIRONMENT ISSUE`, di luar task |
| Masalah yang diketahui | Jeda 800 ms sesudah sukses (bagian 9.8) — risiko sisa, bukan kriteria. **Riwayat:** bukti runtime tambah/ubah belum ada — ditutup uji pemilik 18 September 2026 |
| Dependency backend | `BE-BD-001` ✅. Nol perubahan backend |
| Di luar scope | `BD-UI-GAP-002` tidak disentuh; nol perubahan pada layar Bank Darah lain, termasuk `FE-BD-011` |
| Perubahan sampingan | `NONE` — keluaran build berada di folder yang di-ignore dan tidak muncul di `git status` |
| Interupsi | Preflight pertama mendapati branch `QuilvianDevV2` (`4c84a528`). Pekerjaan dihentikan tanpa mengubah apa pun. Pemilik kembali sendiri ke `sukmagpV2` dan menyetujui `beba89e3` sebagai snapshot awal. Percobaan perbaikan sebelumnya yang belum di-commit sudah tidak ada di working tree, dan pekerjaan ini dimulai ulang dari kondisi bersih |
| Status Git frontend | `M` kedua hook editor, `??` `tests/unit/blood-bank-master-editor-save.test.mjs` |
| Langkah berikutnya | Commit perubahan frontend dan laporan ini atas wewenang pemilik, lalu `FE-BD-011` — task frontend berikutnya menurut urutan yang disetujui. **Riwayat:** jalankan skenario runtime pada bagian 9.7, lalu naikkan status ke ✅ bila seluruhnya lulus — sudah dikerjakan |

### 9.10 Bukti runtime pemilik — 18 September 2026

Pemilik, `Sukmagp`, menjalankan uji manual memakai frontend lokal yang memuat source hasil pengerjaan
ulang ini, yaitu kedua hook pada bagian 9.3 di atas `beba89e3`. Agent tidak menjalankan uji ini dan tidak
menulis data ke database. Hasil di bawah adalah laporan pemilik apa adanya.

| Skenario | Komponen darah | Alasan terkendali |
| --- | --- | --- |
| Tambah | `PASS` | `PASS` |
| Ubah | `PASS` | `PASS` |
| Smoke daftar dan detail | `PASS` | `PASS` |
| Validasi isian wajib | `PASS` | `PASS` |
| Penolakan duplikat oleh backend | `PASS` | `PASS` |

Pada tambah dan ubah, pemilik mencatat empat hal:

1. backend berhasil menyimpan;
2. **tidak** muncul "Gagal Menyimpan" palsu;
3. layar berpindah ke halaman detail;
4. isi detail sesuai hasil simpan.

Hasil ini tidak bertentangan dengan perbaikan maupun dengan bukti otomatis pada bagian 9.6, sehingga
source frontend tidak diubah lagi.

**Kesimpulan.** Kriteria tunggal task ini — "Master CRUD dapat dijalankan dari layar" — kini terbukti penuh
pada kedua layar. Status naik dari 🟡 ke ✅, dan nol butir DoD dikecualikan.

**Riwayat status task ini, berurutan:**

| Tanggal | Status | Sebab |
| --- | --- | --- |
| 7 September 2026 | ✅ | Pengerjaan pertama; uji runtime waktu itu `NOT FEASIBLE` |
| 18 September 2026, pagi | 🟡 | Diturunkan pada sinkronisasi roadmap frontend: simpan rusak (`utils.unwrapApiData`) |
| 18 September 2026 | 🟡 | Dibuka ulang dan diperbaiki; unit test, lint, dan build lulus, tetapi bukti runtime belum ada |
| 18 September 2026 | ✅ | Diverifikasi ulang lewat uji runtime pemilik `Sukmagp` |
