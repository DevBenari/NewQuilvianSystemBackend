# Laporan Perubahan Frontend: Task FE-FIN-007

## 1. Ringkasan Eksekutif
- **Task ID**: `FE-FIN-007`
- **Nama Modul**: `Finance Management`
- **Nama Task**: Penyempurnaan Layar Pemantauan Integrasi (Katalog Final Kejadian, Intake Error, & Status Legacy)
- **Tanggal Selesai**: 30 September 2026
- **Status Akhir**: ✅ **SELESAI (DONE)**
- **Wewenang Keputusan Bisnis**: Product Owner (`FIN-DEC-002`, `FIN-DEC-030`, `FIN-DEC-039`, `FIN-DEC-064`, `FIN-DEC-072`, `FIN-DEC-074`)
- **Kontrak Terkunci**: `FIN-API-1.0` (Billing Intake & Accounting Events), `FIN-PERM-1.0`, `FIN-STATE-1.3`, `FIN-VAL-1.4` (`FIN-VAL-141`, `FIN-VAL-142`), `03-frontend-architecture.md` §3.5, §14.1, §14.2, §14.3
- **Repositori Target**: `QuilvianSystemFrontendDev`
- **Branch**: `yasmina`

---

## 2. Latar Belakang & Kebutuhan Bisnis

Pada implementasi dasar pemantauan integrasi (`FE-FIN-006`), layar telah mampu menyajikan daftar fakta masuk dari Billing (*Intake*) dan antrean outbox kejadian subledger ke Accounting. Namun, seiring dengan pematangan keputusan bisnis keuangan, perbaikan arsitektur tata kelola kasir/billing (`FIN-DEC-030`), dan perluasan katalog kejadian akuntansi (`FIN-DEC-064`, `072`, `074`), ditemukan sejumlah kebutuhan operasional rumah sakit yang wajib disempurnakan pada `FE-FIN-007`:

1. **Dinamisasi Katalog Final Kejadian Akuntansi Tanpa Hardcode (`03-frontend-architecture.md` §14.1)**:
   Pilihan tipe kejadian pada penyaring (*filter*) tidak boleh ditulis tangan secara statis di sisi klien (*browser*). Seluruh kode kejadian katalog final (seperti pemecahan `SELISIH-KAS-KURANG` / `SELISIH-KAS-LEBIH`, penanda shift `PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, dan snapshot `SALDO-SUBLEDGER`) harus diambil secara dinamis dari response metadata backend (`GET /accounting-events/filters/metadata`) serta adaptif terhadap riwayat kejadian yang ada.

2. **Dukungan Kejadian Non-Moneter / Penanda Status (Nominal Rp 0 Sah)**:
   Dalam operasional rumah sakit, beberapa transaksi integrasi tidak melibatkan perpindahan uang tunai langsung melainkan berfungsi sebagai penanda status operasional (misalnya penutupan shift kasir pada akhir giliran kerja kasir IGD atau penutupan periode subledger). Kejadian ini secara sah bernilai Rp 0,00 dan **tidak boleh** ditampilkan sebagai galat atau transaksi bermasalah.

3. **Status `ACKNOWLEDGED` Tanpa Nomor Jurnal Bukan Kegagalan**:
   Pada beberapa kejadian tertentu, sistem Accounting dapat mengakui dan membukukan kejadian (*status ACKNOWLEDGED*) tanpa menerbitkan nomor jurnal umum secara langsung (misalnya kejadian penanda batas atau rekonsiliasi internal). Layar pemantauan wajib menampilkan baris tersebut sebagai pengakuan sah (*positive state*), bukan kegagalan integrasi.

4. **Penegasan Status `HELD_FOR_FINALIZATION` sebagai Peninggalan Kebijakan Lama (`FIN-DEC-030`)**:
   Sebelum dikeluarkannya keputusan bisnis `FIN-DEC-030`, status `HELD_FOR_FINALIZATION` dianggap sebagai antrean wajar yang menunggu kasir melakukan finalisasi tagihan. Melalui `FIN-DEC-030`, kebijakan tersebut telah dicabut: fakta tagihan yang masuk ke Finance hanya yang sudah final. Oleh karena itu, baris historis berstatus `HELD_FOR_FINALIZATION` harus diperlakukan secara tegas sebagai peninggalan kebijakan lama (*legacy*) yang memerlukan tindakan koreksi/penyesuaian data operasional, bukan antrean normal yang sedang menunggu modul Billing.

5. **Penanganan Baris Intake `ERROR` Tanpa Akses Database & Larangan Tombol Ulangi untuk Galat Kebijakan (`FIN-VAL-141`, `FIN-VAL-142`, `03-frontend-architecture.md` §14.3)**:
   Petugas Finance harus dapat melihat rincian penyebab kegagalan pengolahan fakta tagihan langsung dari antarmuka pengguna tanpa perlu meminta staf IT melakukan kueri ke database. Khusus untuk fakta tagihan yang mengalami galat akibat batasan kebijakan rumah sakit atau inkonsistensi mutasi kasir (misalnya pengembalian dana pasien rujukan rawat jalan `REFERRED_OUTPATIENT_ADMIN` sesuai `FIN-VAL-141`, atau pembalikan tender top-up deposit tanpa mutasi pembalik kasir sesuai `FIN-VAL-142`), sistem **secara tegas melarang penawaran tombol "Ulangi" (*retry*)**, karena tindakan coba ulang otomatis tidak akan pernah berhasil tanpa adanya intervensi perbaikan data di modul Billing atau penyesuaian kebijakan administratif.

---

## 3. Alur Proses Bisnis Terpadu

```
[Modul Billing / Kasir RS]
           │
           ▼
[Penerimaan Fakta Masuk (Intake)] ───► Apakah terjadi galat validasi bisnis/kebijakan?
           │                                 │
           │                                 ├───► Ya (Galat Kebijakan: FIN-VAL-141 / FIN-VAL-142)
           │                                 │     - Tampilkan pesan galat detail
           │                                 │     - Badge: "Perlu Kebijakan"
           │                                 │     - Tombol "Ulangi" DINONAKTIFKAN / DILARANG
           │                                 │
           │                                 └───► Ya (Galat Teknis Biasa)
           │                                       - Tampilkan pesan galat
           │                                       - Tersedia tombol "Ulangi"
           ▼
[Transaksi Subledger Finance] (Piutang, Kasir, Setoran, Saldo)
           │
           ▼
[Antrean Outbox ke Accounting] ───► Pemantauan Status:
                                         ├── PENDING: Menunggu pengiriman worker
                                         ├── HELD: Menunggu pembukaan periode akuntansi
                                         ├── HELD_FOR_FINALIZATION: Peninggalan kebijakan lama (Koreksi Data)
                                         ├── SENT: Terkirim, menunggu pengakuan Accounting
                                         ├── ACKNOWLEDGED: Berhasil diakui (meski No. Jurnal kosong)
                                         └── FAILED: Gagal kirim teknis (Investigasi TI/Finance)
```

1. **Pemantauan Fakta Billing Masuk**:
   - Petugas Finance membuka tab *Fakta Billing Masuk (Intake)* untuk melihat aliran klaim/tagihan pasien dari rawat jalan, rawat inap, IGD, dan farmasi.
   - Fakta yang berstatus `NEW` dapat disinkronkan langsung atau diolah menjadi piutang.
   - Fakta yang berstatus `ERROR` diperiksa rincian pesan galatnya. Bila galat disebabkan oleh kebijakan administratif rumah sakit (`FIN-VAL-141` atau `FIN-VAL-142`), antarmuka mengunci tombol pengulangan dan memberikan instruksi bahwa kasus tersebut memerlukan koordinasi manajerial/koreksi data sumber.
2. **Pemantauan Antrean Kejadian Accounting**:
   - Petugas memeriksa outbox kejadian akuntansi pada tab *Antrean Kejadian Accounting (Outbox)*.
   - Tipe kejadian disaring berdasarkan katalog lengkap yang diambil secara dinamis dari backend.
   - Kejadian bernilai nominal Rp 0,00 yang sah (seperti serah terima shift kasir) diberi label penanda khusus dan tidak diperlakukan sebagai galat.
   - Status `HELD_FOR_FINALIZATION` diberikan penanda peringatan oranye (*Amber*) dengan panduan operasional bahwa status tersebut adalah data historis warisan kebijakan terdahulu.

---

## 4. Komponen dan Berkas yang Dimodifikasi

| No | Berkas | Perubahan yang Dilakukan |
|---|---|---|
| 1 | `src/lib/constants/finance/monitoring/monitoring-constants.jsx` | - Mengubah konfigurasi `HELD_FOR_FINALIZATION` menjadi *"Tertahan: Kebijakan Lama"* dengan pesan panduan operasional yang jelas.<br>- Menghapus opsi hardcoded tipe kejadian akuntansi statis dan menetapkan baseline dinamis `DEFAULT_ACCOUNTING_EVENT_TYPE_OPTIONS`. |
| 2 | `src/utils/finance/monitoring/monitoring-utils.jsx` | - Menambahkan fungsi helper `isNonRetryableIntakeError(data)` untuk mendeteksi error kebijakan (`FIN-VAL-141`, `FIN-VAL-142`, `REFERRED_OUTPATIENT_ADMIN`, `FIN-OQ-031`, `FIN-OQ-034`).<br>- Menambahkan helper `isZeroAmountAllowedEventType(eventTypeCode)` untuk mengenali tipe kejadian yang sah bernilai Rp 0 (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, `SALDO-SUBLEDGER`).<br>- Menambahkan pemetaan teks ramah pengguna untuk status `HELD_FOR_FINALIZATION` dan `ACKNOWLEDGED`. |
| 3 | `src/components/view/finance/monitoring/accounting-events/accounting-events-table-columns.jsx` | - Menampilkan `eventTypeCode` katalog final apa adanya tanpa pemotongan string.<br>- Menambahkan badge *"Penanda Status (Rp 0 Sah)"* pada kolom nominal untuk kejadian non-moneter yang sah bernilai Rp 0.<br>- Menampilkan label ramah *"Diakui (Tanpa No. Jurnal)"* pada baris `ACKNOWLEDGED` tanpa nomor jurnal akuntansi tanpa menampilkan warna merah/kegagalan. |
| 4 | `src/components/view/finance/monitoring/accounting-events/accounting-event-detail-modal.jsx` | - Menambahkan banner peringatan khusus untuk kejadian berstatus `HELD_FOR_FINALIZATION` yang menerangkan status warisan kebijakan lama.<br>- Menampilkan catatan penjelas pada informasi nominal untuk tipe kejadian non-moneter bernilai Rp 0.<br>- Menyajikan status pengakuan akuntansi tanpa nomor jurnal secara positif dan bersih. |
| 5 | `src/components/view/finance/monitoring/billing-intake/billing-intake-table-columns.jsx` | - Menghilangkan tombol aksi *"Ulangi"* pada baris intake `ERROR` yang tergolong galat kebijakan (`isNonRetryableIntakeError`), menggantinya dengan badge informatif *"Perlu Kebijakan"*, sehingga mencegah pengulangan sia-sia.<br>- Menyoroti pesan error kebijakan pada kolom rincian galat. |
| 6 | `src/components/view/finance/monitoring/billing-intake/billing-intake-detail-modal.jsx` | - Menambahkan banner informatif khusus batasan kebijakan rumah sakit (`FIN-VAL-141` / `FIN-VAL-142`) pada modal detail rincian fakta intake.<br>- Menyembunyikan tombol proses/ulangi di footer modal detail bila fakta mengalami galat kebijakan yang tidak dapat diulang. |
| 7 | `src/components/view/finance/monitoring/finance-monitoring-view.jsx` | - Mengintegrasikan opsi penyaring tipe kejadian dinamis (`eventTypeOptions`) yang disusun adaptif dari `accountingEvents.metadata.eventTypeCodeOptions` dan kode kejadian pada daftar aktif.<br>- Memperbarui kartu metrik ringkasan `held-finalization` menjadi *"Tertahan: Kebijakan Lama"* dengan subjudul *"Peninggalan kebijakan lama (perlu dibetulkan)"*.<br>- Mengoptimalkan memoization React hooks untuk kepatuhan penuh linter dan performa rendering. |

---

## 5. Gerbang Keputusan Base Component (`UI Gate`)

Sesuai aturan rekayasa frontend Quilvian, seluruh elemen antarmuka memanfaatkan base component yang telah terstandarisasi:

| Komponen / Elemen UI | Sumber Base Component | Status | Rekomendasi & Catatan |
|---|---|---|---|
| Filter Tipe Kejadian Dinamis | `src/components/features/base-features/filter-select.jsx` | `REUSE` | Digunakan untuk menampilkan pilihan dinamis dari backend metadata. |
| Ringkasan Status Outbox & Intake | `src/components/features/base-features/summary-grid.jsx` | `REUSE` | Memperbarui kartu metrik untuk mencerminkan status kebijakan lama secara akurat. |
| Tabel Fakta Masuk & Outbox | `src/components/features/base-features/data-table.jsx` | `REUSE` | Menampilkan baris data dengan styling badge status dan proteksi aksi. |
| Modal Detail Dialog | Accessible Dialog Modal Component | `REUSE` | Memperkaya modal dengan banner kontekstual kebijakan dan detail JSON terformat. |

---

## 6. Spesifikasi Endpoint API (Gaya Swagger)

### Tag: `[Tags("Corporate / Finance Management / Accounting Events")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Query | Format Response |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/accounting-events/filters/metadata` | Mengambil metadata opsi filter (daftar kode tipe kejadian final, status, pengurutan) | `FinanceAccountingEvent : Read` | — | `ApiResponse<AccountingEventFilterMetadataResponse>` |
| `GET` | `/api/v1/corporate/finance-management/accounting-events/summary` | Mengambil ringkasan jumlah antrean outbox (termasuk heldForFinalizationCount) | `FinanceAccountingEvent : Read` | — | `ApiResponse<AccountingEventSummaryResponse>` |
| `GET` | `/api/v1/corporate/finance-management/accounting-events` | Mengambil daftar berpaginasi antrean kejadian subledger outbox | `FinanceAccountingEvent : Read` | `AccountingEventQuery` (deliveryStatus, eventTypeCode, search, pagination) | `ApiResponse<PagedResult<AccountingEventResponse>>` |
| `GET` | `/api/v1/corporate/finance-management/accounting-events/{id}` | Mengambil detail lengkap kejadian outbox beserta muatan payload JSON dan riwayat percobaan | `FinanceAccountingEvent : Read` | Route `id` (Guid) | `ApiResponse<AccountingEventDetailResponse>` |

### Tag: `[Tags("Corporate / Finance Management / Billing Intake")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Query | Format Response |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/billing-intake` | Mengambil daftar fakta tagihan masuk dari Billing beserta status dan error | `FinanceBillingIntake : Read` | Query (status, pagination) | `ApiResponse<PagedResult<BillingIntakeResponse>>` |
| `GET` | `/api/v1/corporate/finance-management/billing-intake/{id}` | Mengambil rincian satu fakta tagihan masuk dan pesan galat validasi | `FinanceBillingIntake : Read` | Route `id` (Guid) | `ApiResponse<BillingIntakeDetailResponse>` |
| `POST` | `/api/v1/corporate/finance-management/billing-intake/sync` | Memicu sinkronisasi penarikan fakta tagihan baru dari modul Billing | `FinanceBillingIntake : Sync` | — | `ApiResponse<BillingIntakeSyncResponse>` |
| `POST` | `/api/v1/corporate/finance-management/billing-intake/{id}/process` | Mengolah kembali fakta masuk (hanya untuk galat teknis non-kebijakan) | `FinanceBillingIntake : Process` | Route `id` (Guid) | `ApiResponse<BillingIntakeResponse>` |

---

## 7. Bukti Validasi

### 7.1 Validasi Linter (`ESLint`)
- **Perintah**: `npm run lint:errors` & `npx eslint src/components/view/finance/monitoring src/lib/constants/finance/monitoring src/utils/finance/monitoring`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (0 errors, 0 warnings)**

### 7.2 Validasi Production Build Next.js
- **Perintah**: `npm run build`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (Exit code 0)**
  - Mengompilasi 419 halaman statis Next.js dengan Turbopack secara sukses.
  - Skrip `scripts/prepare-standalone.mjs` berhasil mengekspor aset statis dan publik untuk runtime mandiri (*standalone*).

### 7.3 Verifikasi Kepatuhan Kriteria Penerimaan (*Acceptance Criteria*)
1. **Katalog Final Kejadian Akuntansi**:
   - Seluruh kode tipe kejadian dari backend (`eventTypeCodeOptions`) disajikan dalam dropdown filter tanpa hardcode di klien.
   - Kode kejadian ditampilkan apa adanya pada tabel dan modal detail.
   - Kejadian non-moneter yang sah bernilai Rp 0 (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, `SALDO-SUBLEDGER`) ditandai dengan badge khusus dan tidak memicu pesan error validasi nominal.
2. **Status `ACKNOWLEDGED` Tanpa Nomor Jurnal**:
   - Baris berstatus `ACKNOWLEDGED` tanpa nomor jurnal akuntansi (`accountingJournalNumber = null/kosong`) ditampilkan sebagai status sukses (`positive`), tidak ditampilkan sebagai kegagalan integrasi, dan tidak memicu tombol kirim ulang.
3. **Penyajian Status `HELD_FOR_FINALIZATION`**:
   - Ditampilkan sebagai *"Tertahan: Kebijakan Lama"* pada kartu ringkasan, tabel, dan modal detail.
   - Modal detail menyajikan banner peringatan bahwa status ini merupakan peninggalan kebijakan lama yang memerlukan pembetulan data operasional, bukan antrean normal yang menunggu modul Billing.
4. **Proteksi Baris Intake `ERROR`**:
   - Rincian sebab kegagalan ditampilkan langsung pada antarmuka dari atribut `errorMessage` backend tanpa memerlukan kueri database manual.
   - Fakta tagihan yang mengalami galat akibat kebijakan atau inkonsistensi mutasi Billing (`FIN-VAL-141` / `FIN-VAL-142`) diproteksi dengan menyembunyikan/meniadakan tombol *"Ulangi"*, mencegah perulangan sia-sia.
5. **Batasan Wewenang & Komputasi**:
   - Nol tombol kirim/publish manual ke Accounting (wewenang tetap pada `EPIC FIN-12`).
   - Nol kalkulasi angka uang atau saldo di sisi browser klien (seluruh angka disajikan murni dari backend).

---

## 8. Kesimpulan & Penandaan Roadmap

Task `FE-FIN-007` telah diimplementasikan secara tuntas dengan kepatuhan penuh terhadap seluruh aturan konstitusi modul keuangan Quilvian, arsitektur frontend §14, dan gerbang kelengkapan validasi.

Status task pada registri dan roadmap diperbarui menjadi:
- `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`: **✅ SELESAI (DONE)**
- `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md`: **✅ SELESAI (DONE)**
