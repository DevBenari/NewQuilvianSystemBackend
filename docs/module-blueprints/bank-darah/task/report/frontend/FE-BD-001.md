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
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` |
| Commit backend yang dijadikan rujukan | `f0d6855c3a5b9bb3974bc306bc009875a891593b` cabang `sukmagp` |
| Tanggal | `2026-09-07` |
| Status | **`SELESAI`** — seluruh acceptance criteria terpenuhi penuh |

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
| Master CRUD katalog komponen darah (`FE-BD-08`) dapat dijalankan dari layar | **Terpenuhi** | View, controller hook, Redux slice, dan route Next.js selesai diuji |
| Master CRUD daftar alasan terkendali (`FE-BD-09`) dapat dijalankan dari layar | **Terpenuhi** | View, controller hook, Redux slice, dan route Next.js selesai diuji |
| 10 kategori alasan tertutup didukung dan divalidasi | **Terpenuhi** | `BLOOD_BANK_REASON_CATEGORY_OPTIONS` dan validasi enum pada form |
| Kolom `CompatibilityEvidenceValidityHours` tersedia di komponen darah | **Terpenuhi** | Terdaftar di kolom tabel, detail rows, dan isian form |
| Penonaktifan memakai `PATCH /{id}/status` | **Terpenuhi** | Thunk `updateStatus` tersedia dan terpeta ke endpoint backend |
| Registrasi menu Bank Darah & Master Data | **Terpenuhi** | Terdaftar di `menu-items.jsx` |
| Laporan tracked dan roadmap terhubung | **Terpenuhi** | Laporan ini tercatat di `task/report/frontend/FE-BD-001.md` |
