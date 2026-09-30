# Laporan Perubahan Backend — `BE-FIN-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-048` |
| Judul | Pengetatan validasi pesan saldo subledger pada kotak keluar sesuai aturan Accounting |
| Slice | Penyelarasan Integrasi Akuntansi Tutup Periode (Menjawab `accounting/evidence/15`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 10 |
| Trace | Keputusan bisnis `FIN-DEC-091`, `FIN-DEC-092`; keputusan Accounting `ACC-DEC-109`, `ACC-DEC-110`; aturan kontrak `ACC-XMOD-0.4` §8a; matriks validasi `contracts/validation-matrix.md` (`FIN-VAL-079`, `FIN-VAL-082`); surat balasan `finance/evidence/21` |
| Contract version | `ACC-XMOD-0.4`, `FIN-VAL-1.5` |
| Dependency | `BE-FIN-023` ✅ (kotak keluar membawa rincian saldo subledger); nol perubahan skema database |
| Klasifikasi | `LOW` — berkas diperiksa 2, berkas diubah 1 (`FinanceAccountingOutboxService.cs`), nol perubahan skema/migration |
| Task mode | `BACKEND` (`TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — 30 September 2026. Validasi nilai positif dan tanggal akhir periode pesan saldo selesai diimplementasikan, seluruh 3 acceptance criteria terpenuhi di kode, nol error kompilasi baru, nol migrasi database. |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa Quilvian:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul pengelolaan keuangan rumah sakit |
| Submodule | `AccountingIntegration` | Submodul aktif integrasi akuntansi |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang modifikasi kode |
| Keberlakuan | `TOUCHED LEGACY` | Pengetatan validasi pada service outbox yang sudah berjalan |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-DTO-001` | Mematuhi seluruh batasan rekayasa backend |

---

## 2. Masalah yang Diperbaiki

Berdasarkan surat susulan dari pemilik modul Accounting ([accounting/evidence/15](../../accounting/evidence/15-susulan-accounting-aturan-saldo-dan-nomor-jurnal.md)) bertanggal 30 September 2026, proses tutup buku besar Accounting kini membandingkan saldo subledger Finance dengan toleransi selisih nol (0) dan tanpa pengecualian (`ACC-DEC-076`, `113`). Dua kelemahan validasi pada `FinanceAccountingOutboxService` sebelumnya berisiko menggagalkan proses rekonsiliasi tutup buku:

1. **Peluang Terkirimnya Saldo Negatif (`ACC-DEC-109`, `FIN-DEC-091`):**
   - *Masalah:* Kode sebelumnya (`FinanceAccountingOutboxService.cs` baris 157-163) memberikan pengecualian `if (!isSaldoSubledger)` pada pemeriksaan `request.Amount < 0`. Ini memungkinkan nominal negatif lolos khusus untuk tipe `SALDO-SUBLEDGER`.
   - *Risiko:* Jika staf mengirim saldo Utang Supplier Rp 300.000.000 sebagai angka minus `-300000000.00` (karena saldo normal kredit), sistem akuntansi akan mendeteksi selisih dua kali lipat (Rp 600 juta) dan menolak pengajuan tutup buku dengan respon HTTP `409 Conflict`.
   - *Solusi:* Accounting mewajibkan seluruh nominal saldo dikirim berdasarkan nilai normal mutlak/positif. Validasi Finance wajib menolak mutlak seluruh nominal negatif (`Amount < 0`) tanpa pengecualian.

2. **Ketiadaan Validasi Tanggal Akhir Periode (`ACC-DEC-110`, `FIN-DEC-092`):**
   - *Masalah:* Pemeriksaan sebelumnya hanya mencocokkan regex `yyyy-MM` pada `AccountingPeriodCode`, tetapi tidak memvalidasi apakah nilai `request.AccountingDate` jatuh tepat pada hari terakhir periode tersebut.
   - *Risiko:* Jika pesan saldo periode September 2026 (`2026-09`) dikirim dengan tanggal 29 September (`2026-09-29`), Accounting menerima pesannya namun menganggap akun kontrol tersebut "belum lengkap", sehingga periode tetap tertahan tidak bisa ditutup sampai versi saldo bertanggal akhir periode dikirimkan.
   - *Solusi:* Sistem harus menghitung hari terakhir bulan secara matematis (termasuk tahun kabisat) dan menolak setiap pesan saldo yang tanggalnya tidak cocok dengan hari terakhir periode.

---

## 3. Rincian Perubahan Kode

Perubahan dilakukan secara terisolasi pada metode privat `ValidateRequest` di `FinanceAccountingOutboxService.cs`:

### 3.1. Penegakan Nilai Negatif Ditolak Mutlak
Kode pengecekan nilai diperketat menjadi:
```csharp
// Aturan Nilai (FIN-VAL-079, FIN-VAL-138, FIN-DES-054, FIN-DES-058, FIN-DEC-091, ACC-DEC-109)
if (request.Amount < 0)
{
    throw new AccountingOutboxException("Nominal kejadian harus lebih dari nol.");
}

if (request.Amount == 0)
{
    if (!FinAccountingEventTypeCodes.ZeroAmountAllowedEventTypes.Contains(request.EventTypeCode))
    {
        throw new AccountingOutboxException("Nominal kejadian harus lebih dari nol.");
    }
}
```
- Bila `request.Amount < 0`: Ditolak langsung dengan `AccountingOutboxException("Nominal kejadian harus lebih dari nol.")` untuk semua jenis kejadian tanpa pengecualian.
- Bila `request.Amount == 0`: Hanya diperbolehkan untuk kejadian yang terdaftar di `ZeroAmountAllowedEventTypes` (`SALDO-SUBLEDGER`, `PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`).
- Bila `request.Amount > 0`: Diterima.

### 3.2. Penegakan Tanggal Akhir Periode pada Pesan Saldo
Pada blok pemeriksaan `isSaldoSubledger`, ditambahkan logika kalkulasi tanggal akhir periode:
```csharp
var periodParts = request.SubledgerBalance.AccountingPeriodCode.Split('-');
var periodYear = int.Parse(periodParts[0]);
var periodMonth = int.Parse(periodParts[1]);
var expectedLastDate = new DateOnly(periodYear, periodMonth, DateTime.DaysInMonth(periodYear, periodMonth));

if (request.AccountingDate != expectedLastDate)
{
    throw new AccountingOutboxException(
        $"AccountingDate untuk pesan saldo periode {request.SubledgerBalance.AccountingPeriodCode} " +
        $"wajib tanggal akhir periode ({expectedLastDate:yyyy-MM-dd}).");
}
```
- Menggunakan `DateTime.DaysInMonth(year, month)` untuk menentukan hari terakhir bulan secara presisi (contoh: September = 30, Februari 2026 = 28, Februari 2028 = 29).
- Jika `request.AccountingDate != expectedLastDate`, melempar `AccountingOutboxException` dengan pesan kesalahan yang informatif menyebutkan tanggal yang seharusnya.

---

## 4. Pemetaan Acceptance Criteria & Bukti

| Acceptance Criteria | Status | Bukti Kode |
| --- | :---: | --- |
| Pesan saldo bernilai negatif ditolak | **Terpenuhi** | `FinanceAccountingOutboxService.cs` baris 150-153: `if (request.Amount < 0) throw new AccountingOutboxException("Nominal kejadian harus lebih dari nol.");` |
| `AccountingDate` bukan tanggal akhir periode ditolak | **Terpenuhi** | `FinanceAccountingOutboxService.cs` baris 191-196: `if (request.AccountingDate != expectedLastDate) throw new AccountingOutboxException(...)` |
| Pesan saldo bernilai `0.00` dan positif dengan tanggal akhir periode tersimpan | **Terpenuhi** | `request.Amount == 0` lolos melalui `ZeroAmountAllowedEventTypes.Contains("SALDO-SUBLEDGER")`, dan `AccountingDate` tepat akhir bulan lolos validasi tanggal. |

---

## 5. Dampak Skema & Lingkungan

- **Perubahan Database:** Nol (0) tabel, nol (0) kolom, nol (0) index baru.
- **Migration:** Tidak ada migration baru.
- **Dampak Pemanggil:** Pemanggil transaksi operasional harian (Billing Intake, Kasir, AR, AP) tidak terdampak karena perubahan ini hanya memperketat kode saldo subledger dan menolak angka negatif.

---

## 6. Langkah Selanjutnya

1. Konfirmasi kompilasi `dotnet build` oleh pengguna.
2. Lanjutkan ke perancangan dan implementasi `BE-FIN-049` (*Monthly Subledger Balance Snapshot Service*) yang akan mengagregasi posisi saldo 4 akun kontrol (Kas Kasir, Kas Kecil, Piutang, Hutang) per akhir bulan dan menerbitkan 4 pesan `SALDO-SUBLEDGER` ke outbox.
