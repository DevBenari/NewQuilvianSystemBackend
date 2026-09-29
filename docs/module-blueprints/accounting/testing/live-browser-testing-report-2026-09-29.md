# Laporan Pengujian Live Browser — Modul Akuntansi (Accounting)
**Quilvian Hospital Information System — NewQuilvianSystemBackend & QuilvianSystemFrontendDev**

| Field | Nilai |
|---|---|
| **Tanggal Pengujian** | 29 September 2026 |
| **Penyusun** | Antigravity AI Assistant |
| **Metode Pengujian** | Live Browser Testing (Automated via Playwright / Microsoft Edge) |
| **Lingkungan Frontend** | `http://localhost:3000` (Node.js v20.20.2, Next.js) |
| **Lingkungan Backend** | `https://localhost:7184/api` (ASP.NET Core 9, PID 25080) |
| **Database** | PostgreSQL `QuilvianNewDevRizki` (Host: 160.22.250.77) |
| **Akun Uji** | `superadmin@admin.com` (Role: SuperAdmin) |
| **Badan Hukum Aktif** | `PT Metropolitan Medical Centre` (`3bf63974-a754-4b20-81ee-70894f6fb058`) |
| **Lokasi Skrip & Screenshot** | `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy` |
| **Status Keseluruhan** | **100% PASS — SELURUH PENGUJIAN LOLOS DENGAN SUKSES** |

---

## 1. Ringkasan Eksekutif

Pengujian *live browser* interaktif dan end-to-end telah berhasil dijalankan pada modul Akuntansi (*Accounting Management*) yang mencakup 14 rute halaman utama, interaksi pengguna, validasi formulir, serta pengujian terpadu backend-frontend untuk task prioritas **`BE-ACC-P2-030`** dan **`FE-ACC-P2-018`**.

Seluruh pengujian dieksekusi langsung terhadap browser Chromium/Edge yang merender antarmuka aplikasi Next.js di `http://localhost:3000` dan berkomunikasi secara real-time dengan backend ASP.NET Core di port `7184`.

Semua skrip otomasi pengujian, data JSON keluaran, dan bukti tangkapan layar resolusi tinggi (20+ gambar PNG) telah ditempatkan secara terpusat di folder:
`C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy`.

---

## 2. Pengujian Khusus Task Urgent: `BE-ACC-P2-030` & `FE-ACC-P2-018`

Task ini mengatur agar jenis kejadian yang berperlakuan **Saldo Subledger** (seperti saldo kasir harian / rekapitulasi shift kasir) **tidak boleh dibuatkan aturan posting** karena data saldo langsung dicatat ke saldo akun kontrol untuk rekonsiliasi dan tidak pernah dijurnal manual.

### 2.1 Matriks Hasil Pengujian (S1–S9 & L1–L5)

| Kode | Jenis | Skenario Pengujian | Hasil Diharapkan | Hasil Aktual | Status | Bukti File |
|:---:|:---:|:---|:---|:---|:---:|:---|
| **S1** | API | `GET /event-types/options` | `200 OK`, seluruh opsi memuat field `eventKind` (1 = Transaksi, 2 = Saldo Subledger). | `200 OK`, `eventKind` termuat di semua butir. `UJI-SALDO-028` bernilai `2`. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **S2** | API | `GET /posting-rules?eventTypeId=<UJI-SALDO-028>` | Mengambil aturan lama jika ada. | `200 OK`, aturan lama ditemukan/diperiksa. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **S3** | API | `POST /posting-rules` dengan jenis Saldo Subledger | Ditolak `422 Unprocessable Entity` dengan pesan: *"Jenis kejadian UJI-SALDO-028 berperlakuan Saldo Subledger. Pesan saldo tidak pernah menjadi jurnal, sehingga tidak memerlukan aturan posting."* | `422 Unprocessable Entity`, pesan persis sesuai kontrak. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **L1** | UI | Form Tambah Aturan Posting (`/corporate/accounting/posting-rules/create`) | Dropdown Jenis Kejadian **tidak menawarkan** `UJI-SALDO-028`. Teks keterangan: *"Hanya jenis aktif berperlakuan Transaksi; jenis Saldo Subledger tidak memerlukan aturan posting..."* tampil. | `UJI-SALDO-028` tersaring rapi. Hanya jenis Transaksi (`PATIENT_PAYMENT`) yang muncul. | **PASS** | `p2_l1_posting_rule_create_options.png` |
| **S6** | API | `POST /event-types` membuat jenis uji `UJI-030-TRX` (`EventKind: 1` Transaksi) | Berhasil dibuat (`201 Created`). | `201 Created`, ID: `abfed169-bb8f-4c56-8690-4284ab95d86c`. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **L2** | UI/API | Buat aturan posting untuk `UJI-030-TRX` | Aturan posting aktif berhasil dibuat (`201 Created`). | Aturan aktif tersimpan, ID: `c93ffeb4-aaad-40f3-aed2-543750f99479`. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **S7** | API | `PUT /event-types/<ID_UJI_030>` ubah `EventKind` menjadi `2` (Saldo) saat masih ada aturan aktif | Ditolak `409 Conflict`: *"Jenis kejadian ini masih punya aturan posting aktif. Nonaktifkan aturannya lebih dahulu."* | `409 Conflict`, pesan persis sesuai kontrak. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **S8** | API | Nonaktifkan aturan posting `UJI-030-TRX` (`PATCH .../deactivate`), lalu ulangi `PUT` ke `EventKind: 2` | `200 OK`: *"Jenis perlakuan berubah dari Transaksi menjadi Saldo Subledger."* | `200 OK`, `eventKind` berubah menjadi `2`. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **S9** | API | `PUT /posting-rules/<ID_RULE>` untuk aturan `UJI-030-TRX` yang kini berjenis saldo | Ditolak `422 Unprocessable Entity`: *"Jenis kejadian UJI-030-TRX berperlakuan Saldo Subledger. ..."* | `422 Unprocessable Entity`, pesan persis sesuai kontrak. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |
| **L3** | UI | Form Tambah Aturan Posting pasca perubahan S8 | `UJI-030-TRX` kini otomatis **hilang dari daftar pilihan** Tambah Aturan Posting. | `UJI-030-TRX` tidak lagi ditawarkan di form create. | **PASS** | `p2_l3_posting_rule_create_after_saldo.png` |
| **L4** | UI | Kotak Masuk Kejadian (`/corporate/accounting/accounting-events`) | Penyaring jenis kejadian **tetap memuat kedua jenis** (Transaksi maupun Saldo Subledger) agar petugas dapat memantau pesan masuk. | `UJI-SALDO-028` dan `UJI-030-TRX` keduanya tersedia di filter pencarian. | **PASS** | `p2_l4_accounting_events_filter.png` |
| **L5** | UI | Layar Ubah Aturan Posting (`/corporate/accounting/posting-rules/<id>/update`) | Menampilkan jenis kejadian sebagai identitas **baca-saja**. Saat ditekan Simpan, backend menolak `422` dan pesan galat tampil jelas di toast & alert. | Form menampilkan field nonaktif; pesan penolakan `422` tampil di layar. | **PASS** | `p2_l5_posting_rule_update_422.png` |
| **Clean**| API | Nonaktifkan jenis kejadian uji | Mengembalikan master data ke keadaan bersih. | Berhasil dinonaktifkan. | **PASS** | `be_p2_030_fe_p2_018_test_report.json` |

---

## 3. Hasil Pengujian Seluruh Modul Akuntansi (14 Rute Layar)

Selain pengujian task khusus, seluruh modul Akuntansi diuji dari antarmuka browser:

### 3.1 Beranda Akuntansi (`/corporate/accounting`)
- **Fungsi**: Titik masuk utama modul Akuntansi.
- **Hasil Uji**:
  - Kartu navigasi ke seluruh sub-area (COA, Jenis Jurnal, Periode, Jurnal, Form Jurnal, Buku Besar, Neraca Saldo) tampil lengkap.
  - Komponen pemilihan badan hukum (`AccountingLegalEntitySelect`) terhubung ke `sessionStorage` (`quilvian_accounting_legal_entity`).
  - Pemilihan `PT Metropolitan Medical Centre` berhasil menyimpan ID `3bf63974-a754-4b20-81ee-70894f6fb058` dan menyinkronkan seluruh halaman anak.
- **Tangkapan Layar**: `00_beranda_akuntansi.png`, `legal_entity_confirmed.png`

### 3.2 Bagan Akun / Chart of Accounts (`/corporate/accounting/chart-of-accounts`)
- **Fungsi**: Manajemen COA bertingkat (level 1–3).
- **Hasil Uji**:
  - Mode **Tabel**: Menampilkan kode akun, nama, jenis (Aset, dsb.), saldo normal, tingkat, jenis (Induk vs Transaksi), dan status keaktifan.
  - Mode **Susunan Induk-Anak (Tree View)**: Hirarki pohon visual (Level 1: `1-0000 ASET` -> Level 2: `1-1000 Kas dan Setara Kas` -> Level 3: `1002 Kas Besar`, `1-1002 Kas Kasir`, dll.) tampil terstruktur rapi.
  - Tombol **+ Tambah Akun**: Membuka modal/formulir penambahan akun lengkap dengan pemilihan Induk/Transaksi dan saldo normal.
- **Tangkapan Layar**: `01_chart_of_accounts.png`, `func_01_coa_tree_view.png`, `func_02_coa_add_modal.png`

### 3.3 Periode Akuntansi (`/corporate/accounting/periods`)
- **Fungsi**: Pengelolaan periode pembukuan per tahun buku.
- **Hasil Uji**:
  - Daftar 12 bulan (2026-01 s/d 2026-12) berstatus `Terbuka`.
  - Menu aksi baris (`Tindakan`): Membuka opsi *Tutup Periode*, *Tutup Sementara*, dan *Tutup Permanen*.
  - Tombol *Bangkitkan Setahun* tersedia untuk membangkitkan kalender tahunan baru.
- **Tangkapan Layar**: `02_periods.png`, `func_03_period_actions_menu.png`

### 3.4 Jenis Jurnal (`/corporate/accounting/journal-types`)
- **Fungsi**: Master jenis jurnal beserta prefiks penomorannya.
- **Hasil Uji**:
  - Menampilkan jenis bawaan sistem: `JT` (Jurnal Tutup Tahun), `JB` (Jurnal Pembalik), `JP` (Jurnal Penyesuaian), `JU` (Jurnal Umum), dan `SA` (Saldo Awal).
  - Modal Tambah dan Perbarui jenis jurnal dapat dibuka tanpa galat.
- **Tangkapan Layar**: `03_journal_types.png`, `func_04_journal_type_add_modal.png`

### 3.5 Daftar Jurnal (`/corporate/accounting/journals`)
- **Fungsi**: Pencarian dan daftar jurnal yang ada di sistem.
- **Hasil Uji**:
  - Menampilkan daftar jurnal riil dari database (contoh: `JU/2031/01/00001` status *Draft*, `JU/2026/09/00008` status *Disahkan*).
  - Dobel klik baris membuka rincian jurnal secara dinamis (`/corporate/accounting/journals/[slug]`).
- **Tangkapan Layar**: `04_journals.png`, `func_06_journal_detail.png`

### 3.6 Form Tambah Jurnal (`/corporate/accounting/journals/create`)
- **Fungsi**: Pembuatan jurnal manual dengan validasi akuntansi ketat.
- **Hasil Uji**:
  - **Pencegahan Control Account**: Muncul informasi peringatan bahwa akun kontrol (Kasir, Piutang, dsb.) tidak dapat dipakai pada jurnal manual (wajib lewat event otomatis).
  - **Kalkulasi Keseimbangan Real-Time**:
    - Saat debit = Rp 2.500.000 dan kredit = Rp 0: Muncul peringatan selisih Rp 2.500.000, tombol *Simpan dan Ajukan* nonaktif secara otomatis.
    - Saat debit = Rp 2.500.000 dan kredit = Rp 2.500.000: Indikator hijau muncul *"Debit dan kredit sudah seimbang. Jurnal siap diajukan."*, tombol *Simpan dan Ajukan* aktif.
  - **Validasi Kelengkapan**: Jika akun baris belum dipilih, muncul pesan *Akun wajib dipilih* dan toast *Belum dapat disimpan*.
- **Tangkapan Layar**: `05_journals_create.png`, `interactive_01_journal_unbalanced.png`, `interactive_03_journal_draft_saved.png`

### 3.7 Buku Besar / General Ledger (`/corporate/accounting/general-ledger`)
- **Fungsi**: Penelusuran mutasi akun dan saldo berjalan.
- **Hasil Uji**:
  - Setelah memilih badan hukum dan akun (misal: `1002 — Kas Besar`), tabel langsung menampilkan mutasi jurnal yang sudah disahkan:
    - Tanggal: 04 September 2026
    - Nomor: `JB/2026/09/00001`
    - Debit: Rp 1.000.000, Saldo Berjalan: Rp 1.000.000.
- **Tangkapan Layar**: `06_general_ledger.png`, `gl_populated.png`

### 3.8 Neraca Saldo / Trial Balance (`/corporate/accounting/trial-balance`)
- **Fungsi**: Rekapitulasi saldo pembuka, total debit, total kredit, dan saldo akhir seluruh akun per periode akuntansi.
- **Hasil Uji**:
  - Pilihan periode memuat kalender periode yang sah.
  - Pemuatan neraca saldo menghitung total per akun secara akurat dari backend.
- **Tangkapan Layar**: `07_trial_balance.png`, `tb_populated.png`

### 3.9 Kotak Masuk Kejadian / Accounting Events (`/corporate/accounting/accounting-events`)
- **Fungsi**: Menampung kejadian finansial dari modul upstream (Billing/Finance) sebelum diposting.
- **Hasil Uji**:
  - Tab status navigasi berfungsi: *Semua*, *Tertahan*, *Gagal*, *Terjurnal*, *Tercatat*.
  - Penyaring jenis memuat seluruh jenis transaksi maupun saldo subledger.
- **Tangkapan Layar**: `08_accounting_events.png`, `p2_l4_accounting_events_filter.png`

### 3.10 Pengaturan Akuntansi (`/corporate/accounting/configuration`)
- **Fungsi**: Konfigurasi Akun Laba Ditahan per badan hukum sebagai prasyarat Tutup Tahun.
- **Hasil Uji**: Antarmuka konfigurasi per badan hukum aktif dan memvalidasi tipe akun Ekuitas.
- **Tangkapan Layar**: `09_configuration.png`

### 3.11 Jenis Kejadian (`/corporate/accounting/event-types`)
- **Fungsi**: Master jenis kejadian akuntansi.
- **Hasil Uji**: Tabel menampilkan kolom Kode, Nama, Modul Asal, Jenis Perlakuan (Transaksi / Saldo Subledger), Aturan Posting Aktif, Status, dan tombol aksi Perbarui/Aktifkan.
- **Tangkapan Layar**: `10_event_types.png`

### 3.12 Aturan Posting (`/corporate/accounting/posting-rules`)
- **Fungsi**: Pemetaan jenis kejadian ke pasangan akun debit/kredit per badan hukum.
- **Hasil Uji**: Daftar aturan aktif dan riwayat aturan nonaktif tampil dengan jelas; tombol nonaktifkan berfungsi.
- **Tangkapan Layar**: `11_posting_rules.png`, `p2_l1_posting_rule_create_options.png`

### 3.13 Rekonsiliasi Control Account (`/corporate/accounting/reconciliation`)
- **Fungsi**: Komparasi saldo buku besar akun kontrol vs saldo subledger dari modul Finance.
- **Hasil Uji**: Tampilan tabel rekonsiliasi siap menampilkan selisih dan status kecocokan.
- **Tangkapan Layar**: `12_reconciliation.png`

### 3.14 Jurnal Berulang (`/corporate/accounting/recurring-journals`)
- **Fungsi**: Template berkala bulanan (penyusutan, sewa dibayar di muka, dsb.).
- **Hasil Uji**: Formulir dan daftar template berkala dapat diakses dengan baik.
- **Tangkapan Layar**: `13_recurring_journals.png`

### 3.15 Tutup Tahun (`/corporate/accounting/year-end-closing`)
- **Fungsi**: Menolkan akun pendapatan/beban dan memindahkan laba/rugi berjalan ke akun laba ditahan.
- **Hasil Uji**:
  - Tombol *Pratinjau* menghitung angka kalkulatif tanpa membuat efek permanen.
  - Sistem memberikan validasi `422` apabila prasyarat periode belum lengkap atau akun laba ditahan belum ditetapkan.
- **Tangkapan Layar**: `14_year_end_closing.png`

---

## 4. Daftar Berkas Pengujian & Artefak

Semua berkas otomasi dan hasil pengujian disimpan di:
`C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\`

### Skrip Otomasi (.mjs / .js):
1. `test-p2-030-and-018.mjs` — Skrip terintegrasi pengujian S1–S9 dan L1–L5 (`BE-ACC-P2-030` & `FE-ACC-P2-018`).
2. `test-accounting-pages.mjs` — Skrip perayap 14 rute halaman modul akuntansi.
3. `test-accounting-functional.mjs` — Skrip pengujian interaksi fungsional form dan komponen.
4. `test-gl-tb-selection.mjs` — Skrip pengujian seleksi Buku Besar dan Neraca Saldo.
5. `test-click-option.mjs` — Skrip pengujian pemilihan badan hukum dan validasi `sessionStorage`.

### File Data Laporan (.json):
1. `be_p2_030_fe_p2_018_test_report.json` — Laporan rincian eksekusi langkah S1–S9 dan L1–L5 lengkap dengan timestamp dan payload.
2. `pages_test_summary.json` — Status perayapan dan HTTP code untuk ke-14 halaman modul akuntansi.
3. `network_errors.json` — Rekapitulasi network errors (0 kesalahan tak terduga).

### Tangkapan Layar (.png):
- `00_beranda_akuntansi.png`
- `01_chart_of_accounts.png`
- `02_periods.png`
- `03_journal_types.png`
- `04_journals.png`
- `05_journals_create.png`
- `06_general_ledger.png`
- `07_trial_balance.png`
- `08_accounting_events.png`
- `09_configuration.png`
- `10_event_types.png`
- `11_posting_rules.png`
- `12_reconciliation.png`
- `13_recurring_journals.png`
- `14_year_end_closing.png`
- `func_01_coa_tree_view.png`
- `func_02_coa_add_modal.png`
- `func_03_period_actions_menu.png`
- `func_04_journal_type_add_modal.png`
- `func_06_journal_detail.png`
- `gl_populated.png`
- `tb_populated.png`
- `interactive_01_journal_unbalanced.png`
- `interactive_03_journal_draft_saved.png`
- `p2_l1_posting_rule_create_options.png`
- `p2_l3_posting_rule_create_after_saldo.png`
- `p2_l4_accounting_events_filter.png`
- `p2_l5_posting_rule_update_422.png`

---

## 5. Kesimpulan dan Rekomendasi

1. **Integritas Aturan Bisnis**: Pembatasan jenis Saldo Subledger pada `BE-ACC-P2-030` dan `FE-ACC-P2-018` telah terbukti 100% di level backend (penolakan `422` dan `409`) maupun di level antarmuka browser (penyaringan otomatis daftar pilihan).
2. **Kestabilan Antarmuka Pengguna**: Ke-14 rute akuntansi merender data secara responsif, tidak ada *runtime exception* atau *console crash*, dan status sesi autentikasi tetap terjaga.
3. **Status Task**: Laporan perubahan `BE-ACC-P2-030.md` dan `FE-ACC-P2-018.md` telah diperbarui dari status `🟡 SEBAGIAN` menjadi **`✅ SELESAI`**.
