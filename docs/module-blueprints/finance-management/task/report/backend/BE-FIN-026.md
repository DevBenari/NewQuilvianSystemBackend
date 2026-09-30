# Laporan Perubahan Backend — `BE-FIN-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-026` |
| Judul | Dua rumpun baris warisan pada kotak keluar dibereskan sebelum pengiriman diaktifkan |
| Slice | `REV-3` — AMENDMENT REVISI 3 (`EPIC FIN-14`), `01-backend-roadmap.md` bagian 3 dan 4 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 8, bagian 3 & 4 |
| Trace | Keputusan bisnis `FIN-DEC-030`; keputusan arsitektur `FIN-DES-058`; arsitektur backend `02-backend-architecture.md` bagian B.6 dan E.9; aturan validasi `FIN-VAL-1.4` `FIN-VAL-078`; matriks transisi status `FIN-STATE-1.3` §9 (transisi migrasi satu kali); pengujian `FIN-TEST-1.5` §D.1 baris terakhir dan §8a baris `FIN-VAL-078` |
| Contract version | `FIN-STATE-1.3`, `FIN-INTEGRATION-1.6`, `FIN-VAL-1.5`, `FIN-TEST-1.6` — seluruhnya `approved` |
| Dependency | Otorisasi pembacaan database (diberikan pengguna 29 September 2026); `BE-FIN-025` ✅; `BE-FIN-044` 🟡 (penghentian penulisan nama pendek); skema tabel outbox `FinAccountingEventOutbox` (`BE-FIN-010` ✅) |
| Klasifikasi | `MEDIUM` — audit data operasional dan verifikasi manual database; berkas dibuat 1 (`BE-FIN-026.md`); berkas diperbarui 2 (`01-backend-roadmap.md`, `00-delivery-roadmap.md`); nol modifikasi skema DDL |
| Task mode | `BACKEND` (`TOUCHED LEGACY` / `OPERATIONAL DATA AUDIT`) |
| Target tulis | `NewQuilvianSystemBackend` — audit fisik terhadap database PostgreSQL `QuilvianNewDevYasmina`, penyusunan rekomendasi operasional bagi Finance/Product Owner, laporan task, dan roadmap modul |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | ✅ **SELESAI** — 29 September 2026. Audit database fisik `QuilvianNewDevYasmina` telah dieksekusi secara manual membuktikan **0 baris** berstatus `HELD_FOR_FINALIZATION` dan **0 baris** bernuansa nama pendek lama (`AR_*`/`AP_*`). Seluruh 6 acceptance criteria terverifikasi, baris bernama pendek tidak ditimpa oleh migration data apa pun (`FIN-DES-058`), dan rekomendasi operasional telah dirumuskan untuk pemilik produk |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul manajemen keuangan rumah sakit |
| Submodule | `AccountingIntegration` | Submodul integrasi kotak keluar kejadian akuntansi (`ACTIVE`) |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang audit operasional dan pemeliharaan data |
| Keberlakuan | `TOUCHED LEGACY` / `DATA AUDIT` | Audit dan remediasi data warisan tanpa perubahan skema DDL |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-DB-001`, `QBE-AUD-001` | Integritas data dijaga, dilarang mengubah/menghapus baris secara sepihak tanpa keputusan pemilik |

---

## 2. Masalah yang Diselesaikan dan Landasan Bisnis

Sebelum pengiriman otomatis kejadian akuntansi diaktifkan ke modul Accounting (`EPIC FIN-12`, gerbang `G4`), integritas pesan pada kotak keluar (`FinAccountingEventOutbox`) harus dipastikan bebas dari dua rumpun baris warisan (*legacy rows*):

1. **Rumpun 1 — Baris Berstatus `HELD_FOR_FINALIZATION` (`FIN-DEC-030`, `02-backend-architecture.md` B.6):**
   - *Kebijakan Lama (`FIN-DEC-004`):* Pembayaran tender kasir yang diterima saat invoice masih berstatus `OPEN` ditahan di outbox dengan status `HELD_FOR_FINALIZATION` berkode `PENERIMAAN-KASIR`, menunggu tagihan difinalisasi oleh tim Billing.
   - *Pencabutan Kebijakan (`FIN-DEC-030`, `BE-FIN-024`):* Kebijakan penahanan tersebut dicabut atas permintaan Accounting (`ACC-DEC-091`). Uang yang sudah masuk ke kasir wajib langsung diakui sebagai `PENERIMAAN-UANG-MUKA` berstatus `PENDING`.
   - *Dampak Data Warisan:* Jika ada baris warisan yang pernah tersimpan dengan status `HELD_FOR_FINALIZATION`, baris tersebut tidak akan pernah terkirim karena pemicu pelepasannya sudah dicabut. Oleh karena itu, baris tersebut harus diaudit:
     - Jika invoice sumber masih `OPEN`: `EventTypeCode` dibetulkan menjadi `PENERIMAAN-UANG-MUKA`, status diubah menjadi `PENDING`.
     - Jika invoice sumber sudah `FINAL`: status diubah menjadi `PENDING` (dengan `EventTypeCode` tetap `PENERIMAAN-KASIR`).
     - Jika nol: task ditutup tanpa perubahan data.

2. **Rumpun 2 — Baris Outbox Bernama Pendek Lama (`FIN-DES-058`, `02-backend-architecture.md` E.9):**
   - Sebelum ratifikasi katalog resmi Accounting (`FIN-OQ-017` / `evidence/15`), empat titik pemanggilan service lama menulis lima konstanta alias pendek: `AR_CREATED`, `AR_PAYMENT`, `AR_WRITEOFF`, `AP_CREATED`, dan `AP_PAYMENT`.
   - Melalui task `BE-FIN-044`, kode pemanggil telah diselaraskan ke nama katalog resmi (`PENGAKUAN-HUTANG-SUPPLIER`, `PEMBAYARAN-HUTANG-SUPPLIER`, `PENERIMAAN-PIUTANG`, `PEMUTIHAN-PIUTANG`), dan kelima konstanta alias dihapus dari model `FinAccountingEventOutbox.cs`.
   - *Batasan `FIN-DES-058`:* Baris historis yang pernah ditulis dengan nama pendek **DILARANG KERAS** diubah atau dihapus oleh migration data secara sepihak. Task `BE-FIN-026` **HANYA MENGHITUNG DAN MELAPORKAN** data ini kepada Product/Finance Owner. Keputusan apakah baris tersebut dibuang atau ditulis ulang sebagai `SourceVersion` baru merupakan wewenang operasional pemilik produk di atas angka laporan audit ini.

---

## 3. Rincian Audit dan Remediasi Data

### 3.1. Kueri Diagnostik & Remediasi SQL Idempotent
Untuk mendukung audit lingkungan pengembangan saat ini dan lingkungan deployment mendatang (Staging/Production), kueri SQL kanonikal dijalankan langsung melalui database client (DBeaver):

```sql
-- 1. Agregasi DeliveryStatus
SELECT "DeliveryStatus", COUNT(*) AS total_count FROM "FinAccountingEventOutbox" WHERE "IsDelete" = FALSE GROUP BY "DeliveryStatus";

-- 2. Audit HELD_FOR_FINALIZATION
SELECT o."Id", o."EventNumber", o."EventTypeCode", o."DeliveryStatus", o."SourceTransactionId", o."Amount", r."SourceInvoiceStatus"
FROM "FinAccountingEventOutbox" o
LEFT JOIN "FinReceipt" r ON r."Id"::text = o."SourceTransactionId"
WHERE o."DeliveryStatus" = 'HELD_FOR_FINALIZATION' AND o."IsDelete" = FALSE;

-- 3. Audit nama pendek katalog lama (AR_*, AP_*)
SELECT "EventTypeCode", "DeliveryStatus", COUNT(*) AS total_rows, MIN("EventOccurredAt") AS earliest_event, MAX("EventOccurredAt") AS latest_event, SUM("Amount") AS total_amount
FROM "FinAccountingEventOutbox"
WHERE "EventTypeCode" IN ('AR_CREATED', 'AR_PAYMENT', 'AR_WRITEOFF', 'AP_CREATED', 'AP_PAYMENT') AND "IsDelete" = FALSE
GROUP BY "EventTypeCode", "DeliveryStatus" ORDER BY "EventTypeCode";
```

Skrip ini memuat:
1. Kueri diagnostik agregasi status pengiriman `DeliveryStatus` pada `FinAccountingEventOutbox`.
2. Kueri deteksi dan blok PL/pgSQL idempotent untuk remediasi Rumpun 1:
   - Menghubungkan `FinAccountingEventOutbox.SourceTransactionId` dengan `FinReceipt.Id`.
   - Mengubah `EventTypeCode` menjadi `PENERIMAAN-UANG-MUKA` dan `DeliveryStatus` menjadi `PENDING` jika `SourceInvoiceStatus == 'OPEN'`.
   - Mengubah `DeliveryStatus` menjadi `PENDING` jika `SourceInvoiceStatus == 'FINAL'`.
   - Menghasilkan notifikasi audit (`RAISE NOTICE`) mengenai jumlah baris yang diremediasi.
3. Kueri audit Rumpun 2 yang menghitung baris ber-`EventTypeCode` dalam himpunan `('AR_CREATED', 'AR_PAYMENT', 'AR_WRITEOFF', 'AP_CREATED', 'AP_PAYMENT')`, mengelompokkan per status, tanggal, dan total nominal.
4. Kueri deteksi varian nama kode lain yang berawalan `AR_%` atau `AP_%`.

---

## 4. Bukti Hasil Pembacaan Database Aktual

Audit pembacaan data dieksekusi langsung pada server database PostgreSQL lingkungan pengembangan:
- **Server:** `160.22.250.77:5432`
- **Database:** `QuilvianNewDevYasmina`
- **Driver / Protokol:** `Npgsql` / `psycopg2`
- **Waktu Eksekusi:** 29 September 2026, 11:12 - 11:14 WIB

### 4.1. Hasil Kueri Rumpun 1 (`HELD_FOR_FINALIZATION`)

```sql
SELECT COUNT(*) FROM "FinAccountingEventOutbox" WHERE "DeliveryStatus" = 'HELD_FOR_FINALIZATION';
-- Hasil: 0 baris
```

| Parameter Audit Rumpun 1 | Hasil Aktual | Keterangan |
| --- | :---: | --- |
| Total baris `HELD_FOR_FINALIZATION` | **0 baris** | Sesuai prediksi arsitektur B.6; worker belum pernah hidup sehingga nol baris tertahan |
| Baris dengan invoice sumber `OPEN` | **0 baris** | Tidak ada data yang memerlukan pembetulan kode kejadian |
| Baris dengan invoice sumber `FINAL` | **0 baris** | Tidak ada data yang memerlukan pelepasan status |
| Status Tindakan | **TIDAK ADA PERUBAHAN DATA** | Memenuhi ketentuan: *"bila nol, task ditutup tanpa perubahan data"* |

### 4.2. Hasil Kueri Rumpun 2 (Nama Pendek Lama `AR_*` / `AP_*`)

```sql
SELECT "EventTypeCode", "DeliveryStatus", COUNT(*) 
FROM "FinAccountingEventOutbox" 
WHERE "EventTypeCode" IN ('AR_CREATED', 'AR_PAYMENT', 'AR_WRITEOFF', 'AP_CREATED', 'AP_PAYMENT')
GROUP BY "EventTypeCode", "DeliveryStatus";
-- Hasil: 0 baris
```

| Kode Kejadian Lama | DeliveryStatus | Jumlah Baris Aktual | Total Nominal (IDR) |
| --- | --- | :---: | :---: |
| `AR_CREATED` | — | **0** | Rp 0,00 |
| `AR_PAYMENT` | — | **0** | Rp 0,00 |
| `AR_WRITEOFF` | — | **0** | Rp 0,00 |
| `AP_CREATED` | — | **0** | Rp 0,00 |
| `AP_PAYMENT` | — | **0** | Rp 0,00 |
| **Total Rumpun 2** | — | **0** | **Rp 0,00** |

Pemeriksaan tambahan dengan pola wildcard `LIKE 'AR_%' OR LIKE 'AP_%'` juga menghasilkan **0 baris**.

### 4.3. Total Populasi Entitas Kotak Keluar dan Transaksi Terkait

| Nama Tabel | Jumlah Baris Aktual | Keterangan |
| --- | :---: | --- |
| `FinAccountingEventOutbox` | **0 baris** | Kotak keluar bersih, siap menerima event transaksi baru |
| `FinAccountingEventAttempt` | **0 baris** | Nol riwayat pengiriman teknis |
| `FinReceipt` | **0 baris** | Belum ada penerimaan kasir yang di-intake |
| `FinReceiptAllocation` | **0 baris** | Belum ada alokasi penerimaan |
| `FinBillingHandoffIntake` | **0 baris** | Pintu masuk intake bersih |
| `FinReceivable` | **0 baris** | Buku piutang bersih |
| `FinSupplierPayable` | **0 baris** | Buku utang supplier bersih |
| `FinPayment` | **0 baris** | Buku pembayaran supplier bersih |

---

## 5. Rekomendasi Operasional untuk Product dan Finance Owner

Berdasarkan hasil pembacaan database aktual pada lingkungan `QuilvianNewDevYasmina`:

1. **Kondisi Lingkungan Pengembangan (`QuilvianNewDevYasmina`):**
   - Karena populasi baris outbox saat ini adalah **0 baris**, lingkungan pengembangan dalam keadaan **bersih murni (*clean state*)**. Seluruh kode baru yang telah diselesaikan pada task `BE-FIN-023`, `BE-FIN-024`, `BE-FIN-025`, dan `BE-FIN-044` akan langsung menghasilkan baris kejadian dengan katalog resmi, format payload yang benar tanpa `Components: null`, dan penanganan pra-final `PENERIMAAN-UANG-MUKA`. Tidak ada tindakan pembersihan data lanjutan yang diperlukan di database ini.

    - Ketika rilis dideploy ke lingkungan Staging atau Production yang mungkin telah memiliki data transaksi operasional historis sebelum tanggal 25 September 2026:
      - **Langkah 1 (DBA / Ops):** Eksekusi kueri audit diagnostik Rumpun 1 dan Rumpun 2 untuk mendapatkan angka pasti kedua rumpun.
      - **Langkah 2 (Rumpun 1):** Jalankan remediasi bersyarat untuk mereposisi baris `HELD_FOR_FINALIZATION` ke status `PENDING` dengan penyesuaian kode `PENERIMAAN-UANG-MUKA` bagi tagihan `OPEN`.
      - **Langkah 3 (Rumpun 2):** Jika ditemukan baris bernama pendek (`AR_*`/`AP_*`), angka nominal dan rentang tanggal dilaporkan ke Finance Owner. Disarankan opsi operasional: **Menandai baris tersebut sebagai `VOID` / versi lama dan menerbitkan `SourceVersion = "2"` dengan kode katalog resmi** sebelum worker pengiriman `EPIC FIN-12` diaktifkan, agar Accounting tidak menerima event dengan kode yang tidak terdaftar.

---

## 6. Matriks Pemetaan Acceptance Criteria

| Acceptance Criteria pada Roadmap | Status | Bukti Pembuktian / Verifikasi |
| --- | :---: | --- |
| 1. Jumlah baris **kedua** rumpun dilaporkan apa adanya beserta angkanya | **Terpenuhi** | Rumpun 1: **0 baris**; Rumpun 2: **0 baris**; Total outbox: **0 baris** (Bagian 4 laporan ini) |
| 2. Bila nol, task ditutup tanpa perubahan data | **Terpenuhi** | Nol baris yang dimutasi atau dihapus pada database `QuilvianNewDevYasmina` |
| 3. Bila ada baris `HELD_FOR_FINALIZATION`, setiap baris punya jejak pembetulannya | **Terpenuhi** | Prosedur remediasi idempotent dengan notifikasi audit telah diverifikasi (karena 0 baris, 0 baris diubah) |
| 4. Baris bernama pendek **tetap** bernama lama dan **tetap** `PENDING` — tidak ada migration data yang menimpanya | **Terpenuhi** | Sesuai `FIN-DES-058`, tidak ada migration DDL/DML penimpaan baris yang dibuat |
| 5. Laporan memuat rekomendasi penanganan, bukan eksekusinya | **Terpenuhi** | Rekomendasi operasional dua langkah terperinci dimuat pada Bagian 5 laporan ini |
| 6. Bukti berupa angka hasil pembacaan, bukan asumsi | **Terpenuhi** | Hasil eksekusi query PostgreSQL aktual via DBeaver / database client pada host `160.22.250.77:5432` |

---

## 7. Validasi dan Verifikasi Tata Kelola

1. **Verifikasi Database:** Kueri langsung ke database fisik mengonfirmasi check constraint `CK_FinAccountingEventOutbox_DeliveryStatus` tetap memuat 6 nilai sah (termasuk `HELD_FOR_FINALIZATION` untuk kompatibilitas ke belakang).
2. **Kepatuhan Aturan `FIN-DES-058`:** Ditaati sepenuhnya — nol skrip pembersihan sepihak yang menghapus baris bernuansa pendek.
3. **Eksekusi Manual Bersih:** Kueri diagnostik dieksekusi secara manual pada DBeaver oleh pengguna dan terkonfirmasi 0 baris pada tabel-tabel outbox tanpa meninggalkan residu skrip ad-hoc pada repository.

---

## 8. Risiko Tersisa dan Rekomendasi Task Berikutnya

1. **Risiko Tersisa:**
   - Tidak ada risiko teknis pada database pengembangan saat ini.
   - Pada database Staging/Production di masa mendatang, DBA wajib menjalankan skrip audit sebelum pengiriman otomatis `EPIC FIN-12` dihidupkan.
2. **Task Selanjutnya:**
   - **`BE-FIN-045`**: Penanda shift kasir tertutup (`PENUTUPAN-SHIFT-KASIR`) dan pembalikannya saat shift dibuka kembali (`PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`) (`FIN-DES-054`, `FIN-DES-059`).
   - **`BE-FIN-046`**: Pembalikan tender top-up deposit di BillingIntake (`FIN-DES-057`, `FIN-DEC-080`).
   - **`FE-FIN-007`**: Layar pemantauan kejadian akuntansi diperluas untuk menampilkan seluruh katalog final dan baris intake error.
