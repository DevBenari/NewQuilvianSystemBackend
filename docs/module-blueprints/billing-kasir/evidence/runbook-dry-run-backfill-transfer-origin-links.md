# Runbook Eksekusi Dry-Run Baca-Saja Backfill Tautan Transfer Lama

## 1. Identifikasi Dokumen dan Tujuan

| Metadata | Keterangan |
| :--- | :--- |
| Task ID | `BE-BKC-086` |
| Judul | Skrip dry-run baca-saja backfill tautan transfer lama |
| Modul / Submodul | `HealthServices / BillingManagement / Billing` |
| Dokumen Acuan | `docs/module-blueprints/billing-kasir/02-backend-architecture.md` amendment `2.3` Bagian 8 |
| Dasar Keputusan | `BKC-DEC-160`, `BKC-DEC-163`, `BKC-DES-088`, `BIL-INT-022`, `BIL-AT-214` |
| Lokasi Skrip SQL | [`docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql) |
| Sifat Operasi | **Murni Baca-Saja (Read-Only)** — Nol operasi `INSERT`, `UPDATE`, `DELETE`, `DROP`, atau `ALTER` |
| Sasaran Pengguna | Database Administrator (DBA), Backend Engineer, dan Pemilik Bisnis Modul Billing |

### Tujuan Runbook:
Memberikan panduan langkah demi langkah yang aman bagi pemilik sistem dan tim teknis untuk mengeksekusi skrip dry-run sebelum migration data `BE-BKC-087` dijalankan. Hasil dari dry-run ini adalah prasyarat mutlak (*hard prerequisite*) untuk memperoleh otorisasi eksplisit pemilik `{OTORISASI-MIG-2}`.

---

## 2. Prosedur Eksekusi Aman

### 2.1 Lingkungan Eksekusi yang Direkomendasikan
1. **Development / Local Environment:**
   Dijalankan pertama kali untuk memvalidasi skenario uji data buatan (`BIL-AT-214`).
2. **Staging / QA Environment:**
   Dijalankan terhadap replika data riil untuk mengukur volume kandidat sebenarnya.
3. **Production (Pra-Rilis):**
   Dijalankan sesaat sebelum jadwal pemeliharaan migrasi `BE-BKC-087` untuk mengunci jumlah baris target yang akan ditulis.

### 2.2 Cara Menjalankan Menggunakan Client Database (DBeaver / pgAdmin / psql)
1. Buka koneksi database PostgreSQL target dengan role yang memiliki izin `SELECT` pada schema `public`.
2. Buka berkas skrip [`dry-run-backfill-transfer-origin-links.sql`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/evidence/dry-run-backfill-transfer-origin-links.sql).
3. Jalankan query satu per satu secara berurutan (Query 1 sampai Query 6) agar masing-masing hasil tabel dapat dicatat dan dianalisis secara terpisah.

---

## 3. Panduan Pembacaan Hasil Query

### Query 1: Ringkasan Total Kandidat per Tipe Sumber
- **Yang Ditampilkan:** Jumlah kunjungan rawat inap transfer yang memenuhi seluruh 5 syarat kriteria penautan resmi, dikelompokkan berdasarkan `AdmissionTransferSourceType` (`Emergency` dan `Outpatient`).
- **Makna Operasional:** Ini adalah jumlah baris baru yang akan ditulis ke tabel `BilInvoiceEncounterLink` saat migrasi `BE-BKC-087` dijalankan.

### Query 2: Klasifikasi Seluruh Transfer Rawat Inap (Kandidat vs Dilewati)
- **Yang Ditampilkan:** Pengelompokan seluruh data transfer rawat inap yang memiliki `SourceEncounterId` ke dalam 6 status evaluasi:
  1. `CANDIDATE_READY`: Memenuhi seluruh kriteria, siap ditautkan oleh migration `BE-BKC-087`.
  2. `ALREADY_LINKED`: Kunjungan sudah memiliki tautan `BilInvoiceEncounterLink` aktif (misal lewat event runtime `BE-BKC-082` atau rujukan operasi). Tidak perlu ditautkan ulang.
  3. `PATIENT_MISMATCH`: Kunjungan rawat inap dan kunjungan asal tercatat untuk pasien yang berbeda. Anomali integritas data Registrasi. Dilewati demi keselamatan data finansial.
  4. `SOURCE_DELETED_OR_CANCELLED`: Kunjungan asal telah dihapus atau berstatus dibatalkan (`IsCancel = true`). Dilewati.
  5. `NO_RANAP_INVOICE`: Kunjungan rawat inap belum memiliki invoice tagihan `RANAP` aktif. Dilewati.
  6. `NO_ADMISSION_RECEIPT`: Invoice ranap ada, tetapi tanda terima event `ADMISSION_CONFIRMED` yang sah belum tercatat pada `BilInpatientEventReceipt`. Dilewati.

### Query 3: Deteksi Anomali `TYPE_MISMATCH`
- **Yang Ditampilkan:** Daftar kandidat yang memiliki ketidakcocokan tipe antara `AdmissionTransferSourceType` di rawat inap dengan `EncounterType` kunjungan asal di Registrasi.
  - Contoh: `AdmissionTransferSourceType = 'Emergency'`, tetapi `EncounterType = 1` (Poliklinik/Outpatient).
- **Tindakan yang Diambil:** Sesuai kriteria `BKC-DES-088`, data ini **tetap ditautkan** (karena kriteria backfill harus 100% konsisten dengan penulis runtime `BE-BKC-082`), namun daftarnya diserahkan ke bagian Rekam Medis / Admisi Registrasi untuk pembersihan data registrasi historis.

### Query 4: Sebaran Status Invoice Rawat Inap Kandidat
- **Yang Ditampilkan:** Status invoice rawat inap (`OPEN`, `FINAL`, `CLOSED`, dll.) dari seluruh kandidat yang akan ditautkan.
- **Dampak Finansial:**
  - **Invoice `OPEN`:** Begitu ditautkan oleh migration, saat kasir membuka tagihan rawat inap dan kalkulasi dijalankan, tagihan rawat inap akan langsung mengkreditkan biaya administrasi poliklinik asal (`BE-BKC-084`).
  - **Invoice `FINAL` / `CLOSED`:** Tagihan yang sudah difinalisasi atau ditutup **tidak akan berubah nominalnya secara otomatis**. Namun status penautan ini akan mencegah tagihan tersebut dilaporkan sebagai anomali `UNLINKED_TRANSFER_ORIGIN` saat validasi `MVP-37` berjalan.

### Query 5: Pemeriksaan Kredit Liar `REFERRED_OUTPATIENT_ADMIN` (`BKC-CQ-16`)
- **Yang Ditampilkan:** Jumlah kredit refund `REFERRED_OUTPATIENT_ADMIN` yang ada di database.
- **Ambang Batas Keamanan:** **Harus bernilai 0**. Jika ditemukan angka > 0 sebelum migration `BE-BKC-085`/`087` berjalan di lingkungan tersebut, hal itu menandakan anomali integritas skema yang harus diinvestigasi terlebih dahulu.

### Query 6: Proyeksi Nilai Kolom yang Akan Ditulis
- **Yang Ditampilkan:** Simulasi tepat dari setiap kolom yang akan di-insert:
  - `ReceiptId`: Mengambil `Id` tanda terima `ADMISSION_CONFIRMED` yang paling awal (`earliest received`).
  - `LinkedAt`: Mengambil `ReceivedAtUtc` tanda terima paling awal (bukan waktu migrasi dijalankan).
  - `CreateBy`: Menggunakan penanda sistem backfill `7b1f0163-0000-4000-8000-000000000163`.

---

## 4. Validasi Matriks Skenario Uji Sesuai `BIL-AT-214`

Pengujian acceptance criteria `BIL-AT-214` dilakukan dengan menyiapkan set data uji spesifik yang memuat 5 skenario transfer:

| ID Skenario | Deskripsi Kasus Uji Data | Hasil Klasifikasi Query 2 | Verifikasi Kepatuhan |
| :--- | :--- | :--- | :---: |
| Kasus 1 | Satu transfer rawat inap sah dengan invoice ranap, tanda terima admisi valid, belum tertaut | `CANDIDATE_READY` = 1 | ✅ Lolos |
| Kasus 2 | Satu transfer rawat inap di mana `S.PatientId` berbeda dengan `E.PatientId` | `PATIENT_MISMATCH` = 1 | ✅ Lolos |
| Kasus 3 | Satu transfer rawat inap di mana kunjungan asal dibatalkan (`IsCancel = true`) | `SOURCE_DELETED_OR_CANCELLED` = 1 | ✅ Lolos |
| Kasus 4 | Satu transfer rawat inap yang belum memiliki invoice ranap aktif | `NO_RANAP_INVOICE` = 1 | ✅ Lolos |
| Kasus 5 | Satu transfer rawat inap yang sudah memiliki tautan aktif di `BilInvoiceEncounterLink` | `ALREADY_LINKED` = 1 | ✅ Lolos |
| **Integritas** | **Verifikasi Efek Samping Database** | **Nol baris berubah / tertulis** | ✅ Lolos |

---

## 5. Gerbang Otorisasi `{OTORISASI-MIG-2}`

Setelah eksekusi dry-run dijalankan oleh tim DBA/pemilik:
1. Hasil output Query 1 sampai Query 5 dicatat pada notulen rilis atau lampiran evidence.
2. Pemilik Modul Billing menandatangani persetujuan `{OTORISASI-MIG-2}` berdasarkan angka kandidat yang terbukti.
3. Migrasi data `BE-BKC-087` baru diizinkan untuk di-generate dan diaplikasikan ke database.
