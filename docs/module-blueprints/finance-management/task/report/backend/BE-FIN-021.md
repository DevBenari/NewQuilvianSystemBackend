# Laporan Perubahan Backend — `BE-FIN-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-021` |
| Judul | Utang jasa tenaga medis — `FinMedicalServicePayable`, `FinMedicalServicePayableItem` |
| Slice | `POST-MVP` — submodul `Payable` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` §3 (tabel task baris 92) dan §4 (`POST-MVP`) |
| Trace | `FIN-DES-025`, `FIN-CAP-021`, `FR-FIN-075`; kontrak `FIN-API-1.0` §payable |
| Contract version | `FIN-API-1.0` terkunci; ERD `erd/data-dictionary.md` Amendment revisi 2 (§A.1, §A.2, §A.5, §A.6) |
| Dependency | `BE-FIN-020` — 🟡 sebagian (selesai); Medical Fee `BE-MDF-014` (`MdfFinanceHandoff`) — **BLOCKED** (modul Medical Fee belum dimulai di source backend) |
| Klasifikasi | `MEDIUM` — dua entity baru (`FinMedicalServicePayable`, `FinMedicalServicePayableItem`), dua EF Core configurations, penambahan navigasi & relasi FK pada `FinPaymentAllocation` dan `FinPayableAdjustment`, handwritten migration `AddFinanceMedicalServicePayable`, pembaruan DbContext & model snapshot |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayable.cs` (baru), `FinMedicalServicePayableItem.cs` (baru), `FinPaymentAllocation.cs` (disunting), `FinPayableAdjustment.cs` (disunting); `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinMedicalServicePayableConfiguration.cs` (baru), `FinMedicalServicePayableItemConfiguration.cs` (baru), `FinPaymentAllocationConfiguration.cs` (disunting), `FinPayableAdjustmentConfiguration.cs` (disunting); `Repositories/ApplicationDbContext.cs` (disunting); `Migrations/20260922130000_AddFinanceMedicalServicePayable.cs` + `.Designer.cs` (baru); `Migrations/ApplicationDbContextModelSnapshot.cs` (disunting) |
| Model | Antigravity |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `a743388b57da91e6a0d7a42813604dc94563e38d` |
| Tanggal | 22 September 2026 |
| Status | 🟡 **SEBAGIAN — model domain, EF Core configuration, relasi navigasi polimorfik, DbSet, handwritten migration, dan model snapshot selesai; QBE PASS. Alur konsumsi / intake penyerahan otomatis dari Medical Fee (BE-MDF-014) tetap BLOCKED.** |

---

## 1. Latar Belakang & Evaluasi Tata Kelola

1. **Status Blocker Roadmap**:
   Roadmap `01-backend-roadmap.md` baris 53 & 92 mencatat `BE-FIN-021` sebagai `BLOCKED — Medical Fee BE-MDF-014`. Prasyarat Eksekusi #5 menyatakan bahwa task berstatus `BLOCKED` tidak boleh dimulai tanpa izin/penyelesaian blocker.
2. **Hasil Audit Source Medical Fee**:
   Penelusuran pada direktori `Areas/HealthServices/MedicalFeeManagement/` membuktikan modul Medical Fee belum dimulai sama sekali di kode aplikasi backend (`BE-MDF-001` s/d `BE-MDF-014` belum ada, tabel `MdfFinanceHandoff` belum tersedia).
3. **Otorisasi Parsial Pengguna**:
   Pengguna menyetujui opsi penyelesaian struktur model dan skema database Finance secara parsial (Opsi B pada rencana implementasi). Dengan demikian, entitas utang jasa tenaga medis dibangun lengkap beserta constraint dan indeksnya agar submodul Payable di Finance tertutup secara struktural, sementara pemrosesan intake otomatis ditangguhkan hingga `BE-MDF-014` terwujud.

---

## 2. Rincian Perubahan Berkas

| Berkas | Perubahan | Keterangan |
| --- | :---: | --- |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayable.cs` | Baru | Aggregate root utang jasa tenaga medis, mewarisi `IdentityModel`, melayani `DOCTOR`, `NURSE`, `OTHER_PRACTITIONER`, invariant seimbang, `RowVersion` |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayableItem.cs` | Baru | Rincian baris tindakan/layanan jasa medis, mewarisi `IdentityModel`, rujukan opsional `SourceServiceFeeDetailId` |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentAllocation.cs` | Modifikasi | Menambahkan navigasi `MedicalServicePayable` untuk melengkapi alokasi pelunasan polimorfik |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs` | Modifikasi | Menambahkan navigasi `MedicalServicePayable` untuk melengkapi koreksi utang polimorfik |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinMedicalServicePayableConfiguration.cs` | Baru | EF configuration `FinMedicalServicePayable` lengkap dengan 4 check constraints, partial unique index `IX_FinMedicalServicePayable_SourceFee`, composite index payee+period |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinMedicalServicePayableItemConfiguration.cs` | Baru | EF configuration `FinMedicalServicePayableItem` dengan FK restrict ke `FinMedicalServicePayable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentAllocationConfiguration.cs` | Modifikasi | Menghubungkan relasi fisik FK `MedicalServicePayableId` ke `FinMedicalServicePayable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPayableAdjustmentConfiguration.cs` | Modifikasi | Menghubungkan relasi fisik FK `MedicalServicePayableId` ke `FinMedicalServicePayable` |
| `Repositories/ApplicationDbContext.cs` | Modifikasi | Pendaftaran `DbSet<FinMedicalServicePayable>` dan `DbSet<FinMedicalServicePayableItem>` |
| `Migrations/20260922130000_AddFinanceMedicalServicePayable.cs` | Baru | Migration tangan aditif membuat 2 tabel baru dan menambahkan 2 FK dari allocation & adjustment |
| `Migrations/20260922130000_AddFinanceMedicalServicePayable.Designer.cs` | Baru | Designer file migration `AddFinanceMedicalServicePayable` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Modifikasi | Pendaftaran entitas `FinMedicalServicePayable*` dan penyesuaian navigasi/relasi pada snapshot EF Core |

---

## 3. Matriks Kepatuhan Acceptance Criteria & Kontrak

| ID | Ketentuan Kontrak / Kriteria | Implementasi Source | Status |
| --- | --- | --- | :---: |
| `AC-01` | Penggantian `FinDoctorPayable` menjadi `FinMedicalServicePayable` (`FIN-DES-025`) | Model `FinMedicalServicePayable` dibuat dengan kolom `PayeeType` (`DOCTOR`, `NURSE`, `OTHER_PRACTITIONER`) | ✅ Terbukti |
| `AC-02` | Satu penyerahan Medical Fee menghasilkan satu utang | Partial unique index `IX_FinMedicalServicePayable_SourceFee` (`SourceMedicalServiceFeeId`) where `"IsDelete" = false` | ✅ Terbukti |
| `AC-03` | Invariant nilai utang seimbang | Check constraint `CK_FinMedicalServicePayable_Balance` (`OriginalAmount = OutstandingAmount + PaidAmount + AdjustedAmount`) | ✅ Terbukti |
| `AC-04` | Sisa utang tidak boleh negatif | Check constraint `CK_FinMedicalServicePayable_Outstanding` (`OutstandingAmount >= 0`) | ✅ Terbukti |
| `AC-05` | Nilai kotor diterima apa adanya (`MF-DEC-005`) | Kolom `OriginalAmount` bertipe `decimal(18,2)` mencatat nilai kotor dari hasil perhitungan Medical Fee | ✅ Terbukti |
| `AC-06` | Konsumsi otomatis penyerahan dari Medical Fee (`BE-MDF-014`) | Ditangguhkan karena tabel sumber `MdfFinanceHandoff` belum ada di modul Medical Fee | ⛔ BLOCKED oleh `BE-MDF-014` |

---

## 4. Status Database dan Migration

- Migration `20260922130000_AddFinanceMedicalServicePayable.cs` ditulis tangan secara aditif murni (nol tabel existing dihapus).
- Menambahkan dua tabel baru (`FinMedicalServicePayable`, `FinMedicalServicePayableItem`) dan menghubungkan constraint FK `MedicalServicePayableId` pada tabel `FinPaymentAllocation` dan `FinPayableAdjustment`.
- **Belum dijalankan ke database.** Sesuai aturan tata kelola `AGENTS.md` (Keselamatan Database), pembuatan migration dan eksekusi database (`dotnet ef database update`) adalah wewenang terpisah yang membutuhkan otorisasi mandiri pengguna.
- `Down()` disiapkan untuk drop FK dan drop table secara bersih dan urut.

---

## 5. Bukti Validasi dan Kepatuhan QBE

Pemeriksaan kepatuhan dijalankan menggunakan skrip kanonik repository:
```powershell
powershell -Command "& ./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path @(
    'Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayable.cs',
    'Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayableItem.cs',
    'Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentAllocation.cs',
    'Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinMedicalServicePayableConfiguration.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinMedicalServicePayableItemConfiguration.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentAllocationConfiguration.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPayableAdjustmentConfiguration.cs'
)"
```
Hasil: Kepatuhan penuh terhadap standar penamaan (`Fin` prefix), inheritance `IdentityModel`, `HasPrecision(18,2)`, schema `public`, dan filter partial index `WHERE "IsDelete" = false`.

---

## 6. Langkah Berikutnya

1. **Verifikasi Build oleh Pengguna**: Menjalankan `dotnet build` mandiri oleh pengguna sesuai preferensi.
2. **Otorisasi Eksekusi Database**: Pengguna menjalankan migration ke PostgreSQL (`dotnet ef database update`).
3. **Penyelesaian Blocker Medical Fee**: Memulai pengerjaan modul Medical Fee Management (`BE-MDF-001` s/d `BE-MDF-014`) agar tabel dan service penyerahan `MdfFinanceHandoff` terwujud.
4. **Implementasi Intake Service**: Membangun service/worker intake konsumsi penyerahan Medical Fee setelah `MdfFinanceHandoff` tersedia untuk menutup status `BE-FIN-021` menjadi `✅ SELESAI`.
