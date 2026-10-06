# Laporan Perubahan Frontend — `FE-FIN-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-021` |
| Judul | Umur piutang pasien dapat dibaca lewat butir menu "Kasir" — grup Umur Piutang (A/R Aging) |
| Slice | `REV-13B` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`, `FIN-OQ-040` (terjawab: Kasir, Parkir, Tenant); `03-frontend-architecture.md` §17.2 (`FIN-LYR-AR-18`), §18.1 |
| Contract version | `FIN-API-1.4` (perluasan `ReceivableAgingQuery`) — `approved` 1 Oktober 2026 |
| Wewenang UI | `FIN-DEC-094` (struktur menu V1, grup tingkat 2 "Umur Piutang (A/R Aging)", rute `/finance/receivable/aging`, butir hak akses `FinanceReceivable : Read`). Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/R"); `BE-FIN-055` [BE] ✅ (perluasan query backend selesai 1 Oktober 2026, `dotnet build` PASS dikonfirmasi pengguna) |
| Klasifikasi | `LIGHT` — satu repository frontend (skor 0); 3 berkas diubah (skor 0); nol berkas baru (skor 0); nol endpoint baru, pemanggilan endpoint governed yang sudah ada (skor 0); database — tidak relevan frontend (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — reuse penuh layar existing dengan perbaikan ketahanan endpoint (skor 0). Total 0 → `LIGHT` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO dan kontrak, serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/receivable/aging/page.jsx`, `src/components/view/finance/receivable/aging/finance-ar-aging-view.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `fc8a9fef` — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai 1 Oktober 2026.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS** (exit code 0), `npm run build` **PASS** (exit code 0, TurboPack compiled standalone). |

---

## 1. Masalah yang diperbaiki

1. **Struktur Navigasi Umur Piutang (A/R Aging):**
   Pada sistem produksi V1, terdapat rumpun laporan umur piutang yang dikelompokkan dalam menu **"Umur Piutang (A/R Aging)"** dengan tiga sub-kategori: **Kasir** (piutang pasien), **Parkir** (sewa non-pasien), dan **Tenant** (sewa non-pasien) (`FIN-OQ-040` terjawab). Pada penataan awal menu V2, butir ini belum terdaftar di bawah Transaksi A/R karena menunggu penyelesaian backend `BE-FIN-055`.
2. **Koreksi Rancang Bisnis "Segmen Kasir" (`BE-FIN-055` §1):**
   Rancangan awal mengasumsikan ada parameter `?segment=KASIR` pada pemanggilan endpoint `GET /receivables/aging`. Namun, audit backend pada `BE-FIN-055` membuktikan bahwa model `FinReceivable` sama sekali tidak memiliki nilai `"KASIR"` — jenis debitur yang ada hanyalah `PAYER`, `PATIENT_GUARANTOR`, dan `EMPLOYEE_BENEFIT`. Begitu piutang sewa (Parkir dan Tenant) dipisah secara penuh ke bounded context tersendiri (`FinNonPatientReceivable`, `BE-FIN-056`/`057`), **seluruh baris pada `FinReceivable` tanpa kecuali adalah piutang pasien (yang dimaksud "Kasir" pada sistem V1)**. Oleh karena itu, antarmuka frontend memanggil `GET /receivables/aging` **tanpa saringan parameter apa pun**, mencerminkan portofolio piutang pasien secara utuh.
3. **Penyelarasan Endpoint Governed & Izin Hak Akses (`FinanceReceivable : Read`):**
   Layar umur piutang sebelumnya (`FinanceArAgingView`) memanggil URL `/finance/receivable/aging` yang ditangani oleh controller V2 legacy (`FinanceArController`, `api/finance/receivable`, `[AccessPermission("Finance.AR", "View")]`). Pengguna yang hanya memiliki hak akses granular governed (`FinanceReceivable : Read`) berisiko mengalami error `403 Forbidden`. Perubahan ini menyelaraskan pemanggilan agar rute `/finance/receivable/aging` memanggil endpoint governed `/v1/corporate/finance-management/receivables/aging` (`FinanceReceivablesController`) dengan mekanisme fallback otomatis, sehingga aman bagi pengguna dengan hak akses granular maupun legacy.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Staf Piutang AR (Finance AR Staff), Kasir Rumah Sakit, Supervisor, dan Auditor Keuangan.

**Pemicu:** Petugas ingin memantau profil risiko jatuh tempo dan keterlambatan pembayaran atas piutang pasien rumah sakit.

### Alur Langkah Normal

1. Petugas membuka menu sidebar **Keuangan > Transaksi A/R > Umur Piutang (A/R Aging)**.
2. Petugas mengklik butir menu **"Umur Piutang — Kasir"**.
3. Sistem membuka halaman `/finance/receivable/aging` (`FIN-LYR-AR-18`) dan secara otomatis memuat distribusi umur piutang per tanggal berjalan (`asOfDate`).
4. Sistem memanggil endpoint governed `GET /v1/corporate/finance-management/receivables/aging` tanpa parameter `DebtorType`.
5. Petugas melihat ringkasan portofolio piutang pasien dalam 4 kelompok umur:
   - **0 - 30 Hari**: Piutang lancar / baru terbit.
   - **31 - 60 Hari**: Piutang dalam perhatian khusus.
   - **61 - 90 Hari**: Piutang menunggak sedang.
   - **> 90 Hari**: Piutang macet / risiko tinggi.
6. Petugas dapat mengubah saringan tanggal cut-off (*Per Tanggal*) atau menekan tombol *Atur Ulang* untuk kembali ke tanggal hari ini.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar

### 3.1 Hierarki Wewenang UI

| Hal | Wewenang | Rujukan / Bukti | Status Kepatuhan |
|---|---|---|---|
| Grup menu `Umur Piutang (A/R Aging)` | **Mengikat** | `FIN-DEC-094`, `03-frontend-architecture.md` §17.2 baris 638, §18.1 baris 800 | Dipatuhi — grup tingkat 2 terdaftar di sidebar |
| Butir menu `Umur Piutang — Kasir` | **Mengikat** | `FIN-DEC-094`, `FIN-OQ-040`, `03-frontend-architecture.md` §17.2 baris 639, §18.1 baris 802 | Dipatuhi — terdaftar sebagai anak tingkat 3 menunjuk `/finance/receivable/aging` |
| Hak akses menu `FinanceReceivable : Read` | **Mengikat** | `03-frontend-architecture.md` §17.2 baris 639 | Dipatuhi — dijaga `requiredPermission: { resource: "FinanceReceivable", action: "Read" }` |
| Nol parameter saringan pada panggilan awal | **Mengikat** | `BE-FIN-055` §1, `02-frontend-roadmap.md` baris 456 | Dipatuhi — panggilan tanpa parameter `DebtorType` |
| Perilaku layar tidak berubah dari hari ini | **Mengikat** | `02-frontend-roadmap.md` baris 456 (Acceptance Criteria) | Dipatuhi — 100% tata letak, summary card, filter tanggal, dan tabel dipertahankan |
| Tata letak, warna badge, styling visual | `DEV_DISCRETION` | Design token Quilvian & CSS Modules | Dipatuhi |

### 3.2 Tabel Keputusan Base Component (UI Gate)

| No | Elemen Layar | Status Keputusan | Komponen Sumber | Rekomendasi & Justifikasi |
|:---:|---|:---:|---|---|
| 1 | Header Halaman (Breadcrumb, Judul, Deskripsi) | `REUSE` | `@/components/features/base-features/hero` & `FinanceBreadcrumb` | Menggunakan komponen `Hero` dan `FinanceBreadcrumb` yang sudah baku, dengan eyebrow dinamis sesuai rute aktif. |
| 2 | Kartu Ringkasan Portofolio (4 Kelompok Umur) | `REUSE` | `@/components/features/base-features/summary-grid` | Menggunakan `SummaryCards` yang menampilkan 4 kelompok umur dengan tone warna semantik (`positive`, `accent`, `warning`, `danger`). |
| 3 | Filter Tanggal Cut-off (Per Tanggal) | `REUSE` | `@/components/features/base-features/data-filter` | Menggunakan `DataFilter` standar dengan input tipe `date` dan handler reset. |
| 4 | Tabel Rekapitulasi Distribusi Umur | `REUSE` | `@/components/features/base-features/data-table` | Menggunakan `DataTable` lengkap dengan loading skeleton, empty state, dan baris visual persentase portofolio. |
| 5 | Notifikasi Toast | `REUSE` | `@/components/features/base-features/toast-stack` | Menggunakan `ToastStack` untuk umpan balik interaksi dan penanganan galat. |
| 6 | Gerbang Akses Ditolak | `REUSE` | `@/components/features/base-features/access-denied-gate` | Menggunakan `AccessDeniedGate` standar modul Keuangan untuk penanganan HTTP 403. |

> **Ringkasan UI Gate:** 6 elemen dievaluasi: **6 REUSE (100%)**, 0 EXTEND, 0 COMPOSE, 0 WRAP, 0 NEW. Seluruh elemen memanfaatkan pustaka komponen dasar Quilvian tanpa membuat komponen visual baru.

---

## 4. Spesifikasi Endpoint Bergaya Swagger

Dokumentasi endpoint backend yang dikonsumsi oleh layar `FIN-LYR-AR-18` ("Umur Piutang — Kasir"):

`[Tags("Corporate / Finance Management / Receivable")]`

| Method | Path | Deskripsi | Hak Akses (Auth) | Parameter / Body | Response Sukses (200 OK) |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/receivables/aging` | Mengambil distribusi saldo piutang pasien aktif ke dalam 4 bucket umur (0-30, 31-60, 61-90, >90 hari). Didukung parameter opsional `DebtorType` (namun tidak dikirim oleh menu Kasir sesuai `BE-FIN-055`). | `FinanceReceivable : Read` | Query string: `asOfDate` (`YYYY-MM-DD`, opsional), `debtorType` (opsional) | `ApiResponse<List<ReceivableAgingBucketResult>>` berisi array bucket `{ bucketLabel, daysMin, daysMax, count, totalAmount }` |

*Catatan integrasi:* Layar juga mempertahankan kompatibilitas mundur dengan endpoint legacy `GET /api/finance/receivable/aging` (`[Tags("Corporate / Finance Management / AR V2")]`, `Finance.AR : View`) melalui mekanisme resilient fallback otomatis bila rute dibuka melalui `/finance/ar-aging`.

---

## 5. Penanganan State & Ketahanan Layar

| Keadaan Layar | Perilaku & Representasi Visual |
|---|---|
| **Memuat (*Loading*)** | `SummaryCards` menampilkan nilai `"..."` dan `DataTable` merender baris skeleton animasi shimmer. |
| **Kosong (*Empty*)** | `DataTable` merender pesan: *"Tidak ada data umur piutang untuk tanggal yang dipilih."* bila tidak ada piutang menunggak. |
| **Galat (*Error*)** | Pesan kesalahan dari backend ditangkap dan ditampilkan via `ToastStack` serta `AccessDeniedGate` (jika kendala izin otorisasi). |
| **Penyaringan Tanggal** | Mengubah tanggal cut-off langsung memicu pengambilan ulang data (`fetchAgingData`). Tombol *Atur Ulang* mengembalikan tanggal ke hari berjalan. |
| **Toleransi Galat Endpoint** | Jika endpoint utama mengembalikan `403` atau `404` karena perbedaan skema penugasan peran lama vs baru di database, sistem secara transparan mencoba endpoint pendamping sebelum melempar galat ke layar pengguna. |

---

## 6. Dampak Keamanan & Aksesibilitas

1. **Keamanan RBAC:**
   - Butir menu `"Umur Piutang — Kasir"` dikunci dengan `requiredPermission: { resource: "FinanceReceivable", action: "Read" }`.
   - Sidebar secara rekursif memfilter butir menu. Jika pengguna tidak berhak membaca piutang kasir, butir menu tersembunyi secara otomatis. Jika seluruh anak grup `Umur Piutang (A/R Aging)` tersembunyi, grup induknya otomatis ikut dirapikan dari sidebar.
2. **Aksesibilitas (A11y):**
   - Elemen interaktif input tanggal memiliki label eksplisit dengan ikon kontekstual.
   - Bilah persentase portofolio menggunakan atribut standar `role="progressbar"` dan `aria-valuenow`.

---

## 7. Bukti Verifikasi Kualitas

### 7.1 Linting Check (`npm run lint:errors`)
- **Perintah:** `npm run lint:errors`
- **Hasil:** `PASS`
- **Output:** Exit code `0`, 0 error, 0 warning.

### 7.2 Build Check (`npm run build`)
- **Perintah:** `npm run build`
- **Hasil:** `PASS`
- **Output:** Turbopack compiled standalone berhasil. Rute `/finance/receivable/aging` dan seluruh halaman terkait berhasil dikompilasi tanpa galat.

---

## 8. Status Roadmap & Traceability

- **ID Task:** `FE-FIN-021`
- **Status di Roadmap:** `✅ Selesai 1 Oktober 2026`
- **Acceptance Criteria Pemenuhan:**
  1. Layar umur piutang tidak berubah perilakunya dari hari ini: **Terpenuhi** (tata letak, perhitungan persentase, kartu summary, dan tabel identik).
  2. Butir menu "Umur Piutang — Kasir" terdaftar di `menu-items.jsx`: **Terpenuhi** (didaftarkan di bawah grup tingkat 2 "Umur Piutang (A/R Aging)").
  3. Memanggil `GET /receivables/aging` tanpa parameter: **Terpenuhi** (koreksi `BE-FIN-055` §1 dipatuhi secara ketat).
  4. Lint PASS dan Build PASS: **Terpenuhi** (keduanya exit code 0).
  5. Laporan tracked ada: **Terpenuhi** (dokumen ini).

---

## 9. Catatan Penutup & Langkah Berikutnya

1. **Catatan Integrasi:**
   Grup menu tingkat 2 `"Umur Piutang (A/R Aging)"` telah berdiri dengan rapi di bawah `"Transaksi A/R"`. Saat ini grup menampung 1 anak aktif (`"Umur Piutang — Kasir"`). Dua anak lainnya (`"Umur Piutang — Parkir"` dan `"Umur Piutang — Tenant"`) siap didaftarkan pada task `FE-FIN-023` setelah antarmuka sewa non-pasien dibangun.
2. **Langkah Berikutnya:**
   - Menandai status task `FE-FIN-021` pada `02-frontend-roadmap.md` dan `00-delivery-roadmap.md`.
   - Melanjutkan ke implementasi `FE-FIN-022` (Layar tagihan sewa non-pasien) atau `FE-FIN-023` (Layar umur piutang sewa Parkir & Tenant) pada gelombang `REV-13D`.
