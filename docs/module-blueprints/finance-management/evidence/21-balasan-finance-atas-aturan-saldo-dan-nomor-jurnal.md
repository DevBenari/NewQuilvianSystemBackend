# Balasan Finance atas Aturan Saldo Subledger dan Nomor Jurnal pada Tanda Terima

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 30 September 2026 |
| Menjawab | `docs/module-blueprints/accounting/evidence/15-susulan-accounting-aturan-saldo-dan-nomor-jurnal.md` (30 September 2026) |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`: `FIN-DEC-090` sampai `FIN-DEC-093` (30 September 2026, sesi `/grill-me`) |
| Kontrak yang berlaku | `ACC-XMOD-0.4` (`cross-module-contract.md` bagian 4b dan 8a) |

---

## 1. Ringkasan Eksekutif

Terima kasih atas surat susulan `evidence/15`. Seluruh empat aturan pesan saldo subledger (`ACC-XMOD-0.4` bagian 8a) dan penjelasan mengenai perilaku nomor jurnal pada tanda terima (`ACC-DEC-116` sampai `119`) **kami terima dan sanggupi sepenuhnya**.

Dokumen ini memuat jawaban tegas Finance atas empat butir kebutuhan Accounting (bagian 4 `evidence/15`), penyesuaian validasi di sisi Finance (`BE-FIN-048`), serta penambahan task penyedia kalkulasi snapshot saldo subledger akhir bulan (`BE-FIN-049`) pada roadmap Finance untuk menjamin rekonsiliasi tutup bulan Accounting dapat berjalan lancar tanpa kendala toleransi selisih.

---

## 2. Jawaban Resmi atas Empat Butir Kebutuhan Accounting

Berikut adalah tanggapan resmi dan komitmen Finance untuk setiap butir pada Bagian 4 `evidence/15`:

| # | Butir Kebutuhan Accounting | Jawaban & Komitmen Finance | Dasar Keputusan |
|---:|---|---|---|
| **15.1** | **Kesanggupan mengirim saldo untuk SETIAP control account aktif tiap periode, termasuk yang bersaldo `0.00` (Aturan 2, `ACC-DEC-108`)** | **SANGGUP PENUH.** Finance berkomitmen menerbitkan pesan saldo subledger untuk **seluruh 4 control account** (Kas Kasir, Kas Kecil, Piutang, dan Utang Supplier) setiap penutupan periode bulanan. Jika satu akun kontrol tidak memiliki transaksi atau bersaldo nihil pada periode tersebut (misalnya kas kecil tidak terpakai), Finance tetap menerbitkan kejadian `SALDO-SUBLEDGER` dengan `Amount = 0.00`. Finance menambahkan task `BE-FIN-049` (*Monthly Subledger Balance Snapshot Service*) untuk mengotomatiskan penerbitan 4 baris kejadian ini. | `FIN-DEC-090` |
| **15.2** | **Kesanggupan mengirim `Amount` menurut saldo normal akun (Aturan 3, `ACC-DEC-109`)** | **SANGGUP PENUH.** Seluruh nilai nominal saldo dikirim dalam **angka positif (atau nol)** mengikuti saldo normal akun masing-masing. Utang Supplier bersaldo kredit Rp 300.000.000 dikirim `300000000.00` (bukan minus). Finance memperketat aturan validasi pada `FinanceAccountingOutboxService` (`BE-FIN-048`) sehingga nominal negatif pada `SALDO-SUBLEDGER` **ditolak tanpa pengecualian**. | `FIN-DEC-091` |
| **15.3** | **Jadwal terbit saldo akhir bulan dan kepatuhan `AccountingDate` = tanggal akhir periode (Aturan 4, `ACC-DEC-110`)** | **SANGGUP & TERJADWAL.**<br>1. **Validasi Tanggal:** `FinanceAccountingOutboxService` mengunci bahwa untuk kejadian `SALDO-SUBLEDGER`, `AccountingDate` wajib sama dengan hari terakhir periode pada `AccountingPeriodCode` (contoh periode `2026-09` wajib `2026-09-30`).<br>2. **Jadwal Operasional:** Kalkulasi snapshot saldo akhir bulan dan penerbitan 4 kejadian outbox dijadwalkan otomatis setiap **tanggal 1 bulan berikutnya pukul 00:05 dini hari WIB**, mengunci posisi saldo per hari terakhir bulan sebelumnya sebelum proses tutup buku Accounting dimulai. | `FIN-DEC-092` |
| **15.4** | **Konfirmasi rujukan tetap `AccountingEventId`, penerimaan status `Gagal`/`Diabaikan`, dan kebutuhan nomor jurnal terkini (Bagian 3, `ACC-DEC-116`..`119`)** | **DIKONFIRMASI.**<br>1. Finance menegaskan bahwa `AccountingEventId` (yang disimpan sebagai `AccountingReceiptNumber`) adalah **satu-satunya rujukan tetap (immutable identity)**.<br>2. `AccountingJournalNumber` diperlakukan murni sebagai informasi pencatatan saat tanda terima pertama, bukan kunci rujukan unik.<br>3. Finance **tidak membutuhkan** jalur query/sinkronisasi berkala untuk nomor jurnal terkini, karena siklus hidup operasional Finance tidak bergantung pada nomor jurnal GL.<br>4. Respon tanda terima berstatus `Gagal` atau `Diabaikan` diterima sebagai status tanda terima yang sah (bukan galat koneksi jaringan). | `FIN-DEC-093` |

---

## 3. Detail Dampak Arsitektur & Tindak Lanjut Teknis di Finance

Untuk memastikan komitmen di atas terlaksana di level kode dan arsitektur, Finance menetapkan dua langkah kerja backend berikut:

### 3.1 Pengetatan Validasi Kotak Keluar (`BE-FIN-048`)
Pada berkas `FinanceAccountingOutboxService.cs`:
1. **Penegakan Nilai Positif / Nol:**
   Menghapus toleransi nilai negatif pada `SALDO-SUBLEDGER`. Pengecekan `request.Amount < 0` kini melempar `AccountingOutboxException("Nominal kejadian harus lebih dari atau sama dengan nol.")` secara mutlak, sehingga nilai utang Rp 300.000.000 wajib bernilai `300000000.00`.
2. **Penegakan Tanggal Akhir Periode:**
   Menambahkan validasi logika tanggal:
   ```csharp
   // Periode YYYY-MM diurai menjadi tahun dan bulan
   var year = int.Parse(request.SubledgerBalance.AccountingPeriodCode.Substring(0, 4));
   var month = int.Parse(request.SubledgerBalance.AccountingPeriodCode.Substring(5, 2));
   var daysInMonth = DateTime.DaysInMonth(year, month);
   var expectedDate = new DateOnly(year, month, daysInMonth);

   if (request.AccountingDate != expectedDate)
   {
       throw new AccountingOutboxException(
           $"AccountingDate untuk pesan saldo periode {request.SubledgerBalance.AccountingPeriodCode} " +
           $"wajib tanggal akhir periode ({expectedDate:yyyy-MM-dd}).");
   }
   ```

### 3.2 Layanan Snapshot Saldo Subledger Bulanan (`BE-FIN-049`)
Finance menambahkan satu service khusus `FinanceSubledgerSnapshotService` yang berjalan pada penutupan bulan untuk mengagregasi posisi saldo berjalan 4 akun kontrol:
1. **Kas Kasir (`KAS-KASIR`):** Total fisik kas dari seluruh shift kasir yang berstatus `CLOSED` / `REVIEWED` pada tanggal akhir periode.
2. **Kas Kecil (`KAS-KECIL`):** Sisa kas fisik di brankas kas kecil per tanggal akhir periode (`CurrentBalance`).
3. **Piutang (`PIUTANG`):** Total sisa piutang penjamin dan pasien (`OutstandingAmount`) yang belum teralokasi per tanggal akhir periode.
4. **Utang Supplier (`UTANG-SUPPLIER`):** Total sisa tagihan utang supplier (`OutstandingAmount`) yang belum dibayar per tanggal akhir periode (dikirim dalam angka positif).

Setiap akun kontrol tersebut menghasilkan satu baris kejadian di `FinAccountingEventOutbox` dengan jenis `SALDO-SUBLEDGER` dan `AccountingDate` hari terakhir bulan.

---

## 4. Penutup

Dengan jawaban dan penyesuaian di atas, gerbang **G4** dan koordinasi jadwal operasional tutup bulan antara Finance dan Accounting dinyatakan **siap dan selaras**. Kami menunggu daftar kode akun kontrol definitif dari Accounting begitu Bagan Akun Rumah Sakit (COA) disahkan pada gerbang G2.
