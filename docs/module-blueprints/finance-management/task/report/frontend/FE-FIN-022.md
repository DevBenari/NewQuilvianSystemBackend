# Laporan Perubahan Frontend — `FE-FIN-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-022` |
| Judul | Petugas AR mencatat, melunasi, menghapus, dan membatalkan tagihan sewa parkir dan tenant — Layar Tagihan Sewa dan Rincian |
| Slice | `REV-13D` — `EPIC FIN-19` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-099`..`106`; `FIN-DES-075`, `076`; `03-frontend-architecture.md` Bagian 18 (`FIN-LYR-AR-21`, `FIN-LYR-AR-22`) |
| Contract version | `FIN-API-1.4` §E.1, `FIN-STATE-1.5` §E.1, `FIN-VAL-1.6` (`FIN-VAL-154`..`164`), `FIN-PERM-1.6` §F.1 — seluruhnya `approved` 1 Oktober 2026 |
| Wewenang UI | `FIN-DEC-105` (butir menu "Tagihan Sewa" dan rute `/finance/non-patient-receivables`); `FIN-DEC-103` (ketiadaan approval pada Hapus & Batalkan **mengikat**); `FIN-DES-075` (pernyataan pelunasan belum masuk kas harian **mengikat** selama `FIN-OQ-044` terbuka); `FIN-STATE-1.5` (pelunasan bernilai minus terbaca jelas sebagai pembatalan **mengikat**). Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/R"); `BE-FIN-057` [BE] ✅ (layanan dan 10 endpoint piutang sewa non-pasien, `dotnet build` PASS dikonfirmasi pengguna) |
| Klasifikasi | `HIGH` — satu repository frontend (skor 0); 7 berkas baru + 1 berkas diubah (skor 1); 2 hook baru + 2 view baru + 1 columns + 1 modal set (skor 2); 6 endpoint dikonsumsi (skor 2); database — tidak relevan frontend (skor 0); keamanan/auth — 1 resource baru (`FinanceNonPatientReceivable`) dengan 3 action (`Read`/`Create`/`Update`) (skor 1); UI/workflow — alur transaksi sewa, konfirmasi non-approval, dan alert integrasi kas (skor 1). Total 7 → `HIGH` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO dan kontrak, serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/non-patient-receivables/**`, `src/components/view/finance/receivable/non-patient/**`, `src/lib/hooks/finance/receivable/{use-non-patient-receivable-list,use-non-patient-receivable-detail}.jsx`, `src/lib/constants/finance/receivable/non-patient-receivable-constants.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `fc8a9fef` — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai 1 Oktober 2026.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS** (exit code 0), `npm run build` **PASS** (exit code 0, TurboPack compiled standalone). |

---

## 1. Masalah yang diperbaiki

Sebelumnya, piutang rumah sakit yang dikelola modul Keuangan terbatas pada piutang pelayanan pasien (`FinReceivable`), yang bersumber dari serah terima billing rumah sakit (`SourceHandoffKey`, `InvoiceId`). Tagihan atas transaksi non-pasien seperti **sewa lahan/kantong parkir** dan **sewa tenant (kantin, ATM center, minimarket, optik, dsb.)** tidak dapat dicatat ke dalam sistem karena model data pasien menuntut data penjamin (`DebtorType`), registrasi, dan episode pasien (`FIN-OQ-043`).

Akibatnya, pencatatan penagihan sewa masih dilakukan secara manual dan di luar pengawasan subledger piutang resmi. Selain itu:
1. Tidak ada antarmuka bagi staf AR untuk menerbitkan tagihan sewa berkala beserta denda keterlambatannya.
2. Tidak ada sarana pencatatan pelunasan pembayaran yang diterima dari pihak penyewa.
3. Sesuai keputusan bisnis `FIN-DEC-103`, penagihan sewa **tidak melewati jenjang persetujuan supervisor/manajer** (karena bersifat penerimaan rutin atas kontrak kerja sama), sehingga antarmuka harus memberikan peringatan eksplisit saat petugas melakukan tindakan berisiko tinggi seperti penghapusan buku (*write-off*) atau pembatalan (*cancel*).
4. Belum adanya integrasi otomatis antara pelunasan sewa dengan kas harian bendahara (`FIN-OQ-044`), sehingga antarmuka **wajib** mencantumkan pernyataan transparan agar petugas tidak berasumsi bahwa uang sewa sudah otomatis tercatat di kas bendahara.

Task **`FE-FIN-022`** menghadirkan dua layar terpadu:
- **`FIN-LYR-AR-21` (Layar Daftar & Pencatatan Tagihan Sewa):** Rute `/finance/non-patient-receivables`, menyediakan pencarian, ringkasan saldo, filter kategori (Parkir/Tenant), filter status, modal pencatatan baru, modal koreksi, modal pelunasan, serta modal konfirmasi hapus buku dan pembatalan tagihan.
- **`FIN-LYR-AR-22` (Layar Rincian Tagihan Sewa & Riwayat Pelunasan):** Rute `/finance/non-patient-receivables/[slug]`, menampilkan kartu identitas objek/penyewa, ringkasan saldo tagihan, riwayat pelunasan bertahap (di mana pelunasan bernilai minus secara visual ditandai sebagai pembatalan/koreksi), dan kontrol aksi yang diaktifkan/dinonaktifkan secara dinamis mengikuti aturan mesin status backend.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Staf Piutang AR (Finance AR Staff) dan Auditor Keuangan.

**Pemicu:** Periode sewa tiba (bulanan/tahunan) untuk objek parkir atau tenant rumah sakit.

### Alur Langkah Normal

1. **Pencatatan Tagihan Sewa Baru:**
   - Petugas membuka menu **Keuangan > Transaksi A/R > Tagihan Sewa**.
   - Petugas mengklik tombol **"+ Catat Tagihan Sewa"** (memerlukan hak akses `FinanceNonPatientReceivable : Create`).
   - Petugas mengisi formulir: Kategori (`Parkir` / `Tenant`), Nama Penyewa, Objek Sewa, Periode Sewa (Awal & Akhir), Tanggal Jatuh Tempo, Nominal Tagihan Pokok, Denda Keterlambatan (opsional), dan Catatan.
   - Setelah menekan tombol "Catat Tagihan", sistem memanggil `POST /v1/corporate/finance-management/non-patient-receivables`. Tagihan terbit dengan status `OUTSTANDING` ("Belum Lunas").
2. **Koreksi Tagihan Sewa:**
   - Bila terdapat kesalahan penginputan nama objek atau nominal sebelum ada pembayaran, petugas menekan tombol **"Koreksi"** (`PUT /{id}`).
   - Sistem memverifikasi bahwa tagihan **belum pernah menerima pembayaran sama sekali** (`Settlements.Count == 0`, `FIN-VAL-159`). Bila sudah ada pembayaran, tombol otomatis dinonaktifkan (`disabled`).
3. **Pencatatan Pelunasan Sewa:**
   - Ketika penyewa membayar sewa (baik sebagian maupun lunas), petugas menekan tombol **"Catat Pelunasan"** (`POST /{id}/settlements`).
   - Modal menampilkan peringatan wajib `FIN-OQ-044` bahwa pelunasan ini belum masuk kas harian bendahara.
   - Petugas memasukkan tanggal, nominal, metode pembayaran (Transfer Bank, Tunai, Giro, QRIS), dan nomor bukti transaksi.
   - Jika pembayaran melunasi seluruh saldo, status otomatis berpindah menjadi `SETTLED` ("Lunas"). Jika baru sebagian, berpindah menjadi `PARTIALLY_SETTLED` ("Sebagian").
   - *Koreksi Pelunasan Negatif:* Jika terdapat kesalahan penginputan kas masuk, petugas dapat mencatat pelunasan bernilai minus untuk mengembalikan saldo piutang ke keadaan sebelumnya (`FIN-VAL-162`).
4. **Hapus Buku Piutang Sewa (Write-Off):**
   - Jika penyewa wanprestasi atau bangkrut dan piutang tidak tertagih, petugas menekan tombol **"Hapus Piutang"** (`POST /{id}/write-off`).
   - Dialog konfirmasi muncul dengan peringatan `FIN-DEC-103` bahwa tindakan ini **tidak memerlukan persetujuan siapa pun**. Petugas **wajib** mengisi alasan penghapusan buku.
   - Status berpindah menjadi `WRITTEN_OFF` ("Dihapusbukukan") dan saldo piutang menjadi Rp 0.
5. **Pembatalan Tagihan Sewa (Cancel):**
   - Jika tagihan keliru diterbitkan (misalnya objek sewa batal digunakan), petugas menekan tombol **"Batalkan"** (`POST /{id}/cancel`).
   - Aksi ini hanya diizinkan jika tagihan berstatus `OUTSTANDING` dan belum ada pembayaran (`FIN-VAL-160`).
   - Dialog konfirmasi muncul dengan peringatan `FIN-DEC-103`. Petugas **wajib** mengisi alasan pembatalan. Status berpindah menjadi `CANCELLED` ("Dibatalkan").

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar

### 3.1 Hierarki Wewenang UI

| Hal | Wewenang | Rujukan / Bukti | Status Kepatuhan |
|---|---|---|---|
| Keberadaan butir menu "Tagihan Sewa" | **Mengikat** | `FIN-DEC-105`, `03-frontend-architecture.md` §18.1 baris 818 | Dipatuhi — terdaftar di bawah `Transaksi A/R` |
| Rute halaman `/finance/non-patient-receivables` | **Mengikat** | `03-frontend-architecture.md` §18.1 baris 810 | Dipatuhi — didukung rute list dan detail |
| Peringatan non-approval pada Hapus & Batalkan | **Mengikat** | `FIN-DEC-103`, `03-frontend-architecture.md` §18.2 baris 873 | Dipatuhi — teks peringatan eksplisit dan alasan wajib diisi |
| Peringatan integrasi kas belum aktif | **Mengikat** | `FIN-OQ-044`, `FIN-DES-075`, `03-frontend-architecture.md` §18.2 baris 876 | Dipatuhi — alert banner ditampilkan di halaman list, detail, dan modal pelunasan |
| Pelunasan minus terbaca sebagai pembatalan | **Mengikat** | `FIN-STATE-1.5`, `03-frontend-architecture.md` §18.2 baris 887 | Dipatuhi — baris pelunasan minus berlatar merah dengan badge "Pembatalan / Koreksi Negatif" |
| Aksi tidak sah dinonaktifkan, bukan disembunyikan | **Mengikat** | `03-frontend-architecture.md` §18.2 baris 869 | Dipatuhi — tombol aksi berstatus `disabled` dengan tooltip penjelas bila status tidak memenuhi syarat |
| Tata letak, warna, ikon, tipografi | `DEV_DISCRETION` | Design token Quilvian & CSS Modules | Dipatuhi |

### 3.2 Tabel Keputusan Base Component (UI Gate)

| No | Elemen Layar | Status Keputusan | Komponen Sumber | Rekomendasi & Justifikasi |
|:---:|---|:---:|---|---|
| 1 | Header Halaman & Breadcrumb | `REUSE` | `@/components/features/base-features/hero` & `FinanceBreadcrumb` | Menggunakan komponen standar `Hero` dan breadcrumb hirarkis navigasi Keuangan. |
| 2 | Kartu Ringkasan Portofolio (5 Kartu) | `REUSE` | `@/components/features/base-features/summary-grid` | Menggunakan `SummaryCards` yang menampilkan Total Tagihan, Belum Lunas, Sebagian, Lunas, dan Dihapus/Batal. |
| 3 | Filter Pencarian & Kategori | `REUSE` | `@/components/features/base-features/data-filter` | Menggunakan `DataFilter` standar dengan input pencarian, dropdown kategori sewa, dropdown status, dan tanggal jatuh tempo. |
| 4 | Tabel Daftar Tagihan Sewa | `REUSE` | `@/components/features/base-features/data-table` | Menggunakan `DataTable` lengkap dengan pagination, status badge, sorting, dan aksi baris. |
| 5 | Badge Status Tagihan & Kategori | `REUSE` | `@/components/features/base-features/status-badge` | Menggunakan `StatusBadge` dengan pemetaan warna semantik status (`warning`, `info`, `success`, `secondary`, `danger`). |
| 6 | Modal Form Tagihan (Catat/Koreksi) | `COMPOSE` | `@/components/features/base-features/confirm-modal` | Dirangkai menggunakan `ConfirmModal` sebagai kerangka modal dialog yang menampung form isian tagihan sewa. |
| 7 | Modal Catat Pelunasan | `COMPOSE` | `@/components/features/base-features/confirm-modal` | Dirangkai menggunakan `ConfirmModal` dengan penambahan panel ringkasan sisa piutang dan alert peringatan `FIN-OQ-044`. |
| 8 | Modal Konfirmasi Hapus Piutang (Write-off) | `REUSE` | `@/components/features/base-features/confirm-modal` | Memanfaatkan kapabilitas bawaan `ConfirmModal` dengan `requireReason={true}` dan pesan non-approval `FIN-DEC-103`. |
| 9 | Modal Konfirmasi Pembatalan Tagihan (Cancel) | `REUSE` | `@/components/features/base-features/confirm-modal` | Memanfaatkan kapabilitas bawaan `ConfirmModal` dengan `requireReason={true}` dan varian `danger`. |
| 10 | Banner Peringatan Integrasi Kas (`FIN-OQ-044`) | `REUSE` | Komponen Alert Bootstrap dengan ikon Bootstrap/Remix standard | Alert banner semantik `alert-warning` yang menonjol dan informatif. |
| 11 | Tabel Riwayat Pelunasan di Detail | `COMPOSE` | HTML Table responsif (`data-flat-table="true"`) | Tabel riwayat pelunasan dengan styling semantik untuk membedakan pembayaran normal (hijau) dan pembatalan/koreksi negatif (merah). |

> **Ringkasan UI Gate:** 11 elemen dievaluasi: **8 REUSE (72.7%)**, **3 COMPOSE (27.3%)**, 0 EXTEND, 0 WRAP, 0 NEW. Seluruh elemen memanfaatkan pustaka komponen dasar Quilvian tanpa membuat komponen visual baru.

---

## 4. Spesifikasi Endpoint Bergaya Swagger

Dokumentasi endpoint backend yang dikonsumsi oleh layar `FIN-LYR-AR-21` dan `FIN-LYR-AR-22`:

`[Tags("Corporate / Finance Management / Non Patient Receivable")]`

| Method | Path | Deskripsi | Hak Akses (Auth) | Request Body / Query | Response Sukses (200 / 201) |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/non-patient-receivables` | Mengambil daftar piutang sewa berpaginasi dengan filter kategori, status, tanggal jatuh tempo, dan kata kunci pencarian. | `FinanceNonPatientReceivable : Read` | `NonPatientReceivableQuery` (query string) | `ApiResponse<PagedResult<NonPatientReceivableResponse>>` |
| `GET` | `/api/v1/corporate/finance-management/non-patient-receivables/summary` | Mengambil ringkasan nominal dan jumlah tagihan per status untuk kategori sewa tertentu. | `FinanceNonPatientReceivable : Read` | `NonPatientReceivableSummaryQuery` (query string) | `ApiResponse<NonPatientReceivableSummaryResponse>` |
| `GET` | `/api/v1/corporate/finance-management/non-patient-receivables/{id}` | Mengambil detail lengkap satu tagihan sewa beserta riwayat seluruh transaksi pelunasannya. | `FinanceNonPatientReceivable : Read` | Path parameter `{id}` | `ApiResponse<NonPatientReceivableDetailResponse>` |
| `POST` | `/api/v1/corporate/finance-management/non-patient-receivables` | Mencatat penerbitan tagihan sewa non-pasien baru untuk objek sewa dan periode tertentu. | `FinanceNonPatientReceivable : Create` | `CreateNonPatientReceivableRequest` (JSON) | `201 Created` — `ApiResponse<NonPatientReceivableResponse>` |
| `PUT` | `/api/v1/corporate/finance-management/non-patient-receivables/{id}` | Mengoreksi data tagihan sewa yang belum pernah menerima pembayaran sama sekali. | `FinanceNonPatientReceivable : Update` | `UpdateNonPatientReceivableRequest` (JSON) | `ApiResponse<NonPatientReceivableResponse>` |
| `POST` | `/api/v1/corporate/finance-management/non-patient-receivables/{id}/settlements` | Mencatat transaksi pelunasan pembayaran sewa (mendukung nominal positif maupun negatif). | `FinanceNonPatientReceivable : Update` | `CreateNonPatientReceivableSettlementRequest` (JSON) | `201 Created` — `ApiResponse<NonPatientReceivableResponse>` |
| `POST` | `/api/v1/corporate/finance-management/non-patient-receivables/{id}/write-off` | Menghapusbukukan saldo piutang sewa yang macet tanpa jenjang persetujuan (alasan wajib diisi). | `FinanceNonPatientReceivable : Update` | `WriteOffNonPatientReceivableRequest` (JSON) | `ApiResponse<NonPatientReceivableResponse>` |
| `POST` | `/api/v1/corporate/finance-management/non-patient-receivables/{id}/cancel` | Membatalkan penerbitan tagihan sewa yang belum menerima pembayaran (alasan wajib diisi). | `FinanceNonPatientReceivable : Update` | `CancelNonPatientReceivableRequest` (JSON) | `ApiResponse<NonPatientReceivableResponse>` |

---

## 5. Penanganan State & Ketahanan Layar

| Keadaan Layar | Perilaku & Representasi Visual |
|---|---|
| **Memuat (*Loading*)** | Kartu ringkasan menampilkan `"..."`, tabel menampilkan skeleton loading shimmer, dan tombol aksi menampilkan spinner selama mutasi berlangsung. |
| **Kosong (*Empty*)** | `DataTable` menampilkan pesan *"Belum ada tagihan sewa pada saringan ini."* bila tidak ada baris yang cocok. Pada riwayat pelunasan detail, menampilkan pesan *"Belum ada riwayat pelunasan untuk tagihan sewa ini."* |
| **Galat (*Error*)** | Pesan penolakan validasi bisnis backend (`FIN-VAL-154`..`164`) ditangkap dan ditampilkan via `ToastStack`. Kendala otorisasi ditangani oleh `AccessDeniedGate`. |
| **Aksi Tidak Sah (*Disabled State*)** | Tombol aksi pada baris tabel maupun header detail dinonaktifkan (`disabled`) dengan atribut `title` yang menjelaskan alasan penonaktifan: <br>- *Koreksi:* nonaktif jika sudah ada pelunasan.<br>- *Pelunasan:* nonaktif jika tagihan sudah `WRITTEN_OFF` atau `CANCELLED`.<br>- *Hapus Piutang:* nonaktif jika tagihan sudah `SETTLED`, `WRITTEN_OFF`, atau `CANCELLED`.<br>- *Batalkan:* nonaktif jika sudah ada pelunasan atau status bukan `OUTSTANDING`. |
| **Transparansi Non-Approval (`FIN-DEC-103`)** | Modal Hapus Piutang dan Batalkan menyajikan kotak peringatan merah tebal bahwa tindakan ini bersifat final tanpa persetujuan, dan tombol konfirmasi dinonaktifkan sampai kolom alasan diisi. |
| **Transparansi Kas (`FIN-OQ-044`)** | Banner peringatan emas/kuning ditampilkan secara permanen pada header halaman dan modal pelunasan. |
| **Pelunasan Bernilai Negatif** | Baris pelunasan bernilai minus ditampilkan dengan latar merah lembut, teks angka merah, dan badge *"Pembatalan / Koreksi Negatif"*. |

---

## 6. Dampak Keamanan & Aksesibilitas

1. **Keamanan RBAC:**
   - Resource baru: `FinanceNonPatientReceivable`.
   - Tindakan melihat daftar dan rincian tagihan dijaga oleh action `Read`.
   - Tindakan mencatat tagihan baru dijaga oleh action `Create`.
   - Tindakan mengoreksi, melunasi, menghapusbuku, dan membatalkan tagihan dijaga oleh action `Update`. Tombol disembunyikan bagi pengguna yang tidak memiliki hak akses ini.
2. **Aksesibilitas (A11y):**
   - Seluruh input form pada modal dialog memiliki elemen `<label>` yang terikat secara semantik.
   - Tombol-tombol aksi baris memiliki atribut `title` dan aria label yang jelas untuk pembaca layar.

---

## 7. Bukti Verifikasi Kualitas

### 7.1 Linting Check (`npm run lint:errors`)
- **Perintah:** `npm run lint:errors`
- **Hasil:** `PASS`
- **Output:** Exit code `0`, 0 error, 0 warning.

### 7.2 Build Check (`npm run build`)
- **Perintah:** `npm run build`
- **Hasil:** `PASS`
- **Output:** Turbopack compiled standalone berhasil. Rute `/finance/non-patient-receivables` dan `/finance/non-patient-receivables/[slug]` berhasil dikompilasi ke bundle produksi tanpa kendala sintaksis atau referensi impor.

---

## 8. Status Roadmap & Traceability

- **ID Task:** `FE-FIN-022`
- **Status di Roadmap:** `✅ Selesai 1 Oktober 2026`
- **Acceptance Criteria Pemenuhan:**
  1. Konfirmasi Hapus dan Batalkan MUST menyebut terang ketiadaan persetujuan dan alasan wajib diisi: **Terpenuhi** (`NonPatientReceivableWriteOffModal` & `NonPatientReceivableCancelModal` mewajibkan alasan dan menampilkan teks `FIN-DEC-103`).
  2. Layar MUST menyatakan pelunasan belum masuk kas harian selama `FIN-OQ-044` terbuka: **Terpenuhi** (alert banner `FIN-OQ-044` hadir di list, detail, dan modal pelunasan).
  3. Pelunasan bernilai minus terbaca jelas sebagai pembatalan: **Terpenuhi** (tabel riwayat pelunasan menandai baris minus dengan badge pembatalan dan warna merah).
  4. Butir menu "Tagihan Sewa" terdaftar di `menu-items.jsx`: **Terpenuhi** (didaftarkan di bawah `Transaksi A/R`).
  5. Lint PASS dan Build PASS: **Terpenuhi** (keduanya exit code 0).
  6. Laporan task tracked ada: **Terpenuhi** (dokumen ini).

---

## 9. Catatan Penutup & Langkah Berikutnya

1. **Catatan Integrasi:**
   Seluruh kapabilitas pengelolaan piutang sewa non-pasien (`FIN-LYR-AR-21` dan `FIN-LYR-AR-22`) kini telah operasional di sisi frontend, melengkapi 10 endpoint backend dari `BE-FIN-057`.
2. **Langkah Berikutnya:**
   - Menandai status task `FE-FIN-022` pada `02-frontend-roadmap.md` dan `00-delivery-roadmap.md`.
   - Melanjutkan ke implementasi `FE-FIN-023` (Layar umur piutang sewa Parkir dan Tenant beserta dua butir menunya di bawah grup "Umur Piutang (A/R Aging)").
