# Laporan Perubahan Backend — `BE-BKC-FIX-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-022` |
| Judul | Tambahkan `REEVALUATED_BY_CASHIER` ke constraint kode alasan surat kelayakan kepulangan rawat inap (`CK_BilInpatientClearanceHandoff_ReasonCode`) |
| Sumber | [`PLAN-REPAIR-BKC-002`](../../docs/plan-repair/plan-repair-002-evaluasi-ulang-kelayakan-ditolak.md) `FIX-BKC-002-01`; cacat [`ISSUE-BKC-002`](../../docs/issue/issue-002-evaluasi-ulang-kelayakan-ditolak.md) / `ISS-BKC-002-01` |
| Keputusan | Opsi A (Perluas daftar literal pada berkas konfigurasi EF Core, buat satu migration mandiri, lengkapi metadata designer, dan perbarui model snapshot) |
| Target tulis | `NewQuilvianSystemBackend` |
| Tanggal | 2026-10-14 |
| Status | 🟡 **Source, migration, dan snapshot selesai; migration sudah diterapkan pemilik ke basis data dev.** Belum tercatat: hasil `dotnet build` (dijalankan pemilik, tidak dijalankan agent) dan uji tombol Evaluasi Ulang di layar |

---

## 1. Masalah

Pada modul Billing dan Kasir alur Rawat Inap, petugas kasir memiliki wewenang untuk melakukan evaluasi ulang kelayakan kepulangan pasien rawat inap melalui tombol **Evaluasi Ulang** pada daftar surat kelayakan modul konsumen (`BIL-SCR-41`). Tombol ini memanggil endpoint:
`POST /api/v1/health-services/billing-management/billing/inpatient-clearance/reevaluate`

Action `ReevaluateClearance` pada `InpatientClearanceController` memanggil `InpatientClearanceService.EvaluateClearanceAsync` dengan kode alasan:
`InpatientClearanceReasonCodes.ReevaluatedByCashier` bernilai string `"REEVALUATED_BY_CASHIER"`.

Namun pada skema basis data PostgreSQL yang diterapkan sebelumnya (migrasi `20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff`), batasan pemeriksaan skema (*CHECK constraint*):
`CK_BilInpatientClearanceHandoff_ReasonCode`
hanya mengizinkan 6 nilai:
```sql
"ReasonCode" IN (
    'INVOICE_SETTLED',
    'GUARANTOR_APPROVED',
    'DISCHARGE_ORDER_INITIATED',
    'LATE_CHARGE_POSTED',
    'PAYMENT_REVERSED',
    'CORRECTION_APPLIED'
)
```

Nilai `'REEVALUATED_BY_CASHIER'` tidak terdaftar pada constraint tersebut. Akibatnya, setiap kali kasir menekan tombol Evaluasi Ulang, penyimpanan entitas `BilInpatientClearanceHandoff` versi baru selalu ditolak oleh basis data dengan *PostgresException: 23514 (check constraint violation)* dan menghasilkan status respon HTTP 500 (Galat Server), meskipun kondisi finansial pasien sudah lunas dan layak pulang.

---

## 2. Proses Bisnis Sesudah Perbaikan

1. Pasien rawat inap (contoh: Ny. Sari Mandasari) yang sebelumnya memiliki surat kelayakan berstatus `BLOCKED` karena masih memiliki sisa kewajiban pembayaran, telah menyelesaikan seluruh tagihannya di kasir.
2. Petugas kasir membuka layar **Surat ke Modul Konsumen** (`BIL-SCR-41`) dan menekan tombol **Evaluasi Ulang** pada baris surat kepulangan pasien yang bersangkutan.
3. Kasir mengonfirmasi tindakan pada dialog konfirmasi evaluasi ulang.
4. Sistem backend menjalankan verifikasi kelayakan finansial:
   - Memastikan sisa tagihan pasien bernilai nol atau dijamin penuh penjamin.
   - Mengambil versi finansial terakhir dari handoff kepulangan pasien.
   - Menerbitkan record handoff kelayakan baru dengan `FinancialVersion` bertambah 1, `ClearanceStatus = 'CLEARED'`, dan `ReasonCode = 'REEVALUATED_BY_CASHIER'`.
5. Basis data PostgreSQL menerima nilai `'REEVALUATED_BY_CASHIER'` tanpa pelanggaran batasan (*check constraint*).
6. Kasir menerima pemberitahuan hijau: *"Evaluasi Berhasil: Evaluasi ulang kelayakan kepulangan rawat inap selesai. Status clearance: CLEARED"*.
7. Status kelayakan pasien di modul Rawat Inap/Perawat langsung terbarukan sehingga proses discharge fisik pasien dapat dilanjutkan.

> **Contoh Skenario Rumah Sakit:**  
> Pasien Ny. Sari Mandasari memiliki surat kelayakan awal versi 1 dengan status `BLOCKED` karena terdapat deposit yang belum diaplikasikan ke tagihan kamar. Setelah kasir mengalokasikan deposit dan tagihan menjadi lunas Rp 0, kasir menekan tombol **Evaluasi Ulang**. Sistem mencatat surat kelayakan versi 2 dengan alasan `REEVALUATED_BY_CASHIER` dan status `CLEARED`. Pasien dapat dipulangkan oleh perawat ruangan tanpa kendala sistem.

---

## 3. Rincian Perubahan Kode dan Berkas

| Berkas | Jenis Perubahan | Deskripsi |
| --- | --- | --- |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInpatientClearanceHandoffConfiguration.cs` | Modifikasi | Menambahkan `'REEVALUATED_BY_CASHIER'` ke definisi `CK_BilInpatientClearanceHandoff_ReasonCode` (baris 24) sehingga total terdapat 7 kode alasan yang sah. |
| `Migrations/20261014100000_FixClearanceReasonCodeReevaluatedByCashier.cs` | Berkas Baru | Migration EF Core mandiri: metode `Up()` melakukan `DropCheckConstraint` lalu `AddCheckConstraint` dengan 7 nilai. Metode `Down()` mengembalikan constraint ke 6 nilai awal. |
| `Migrations/20261014100000_FixClearanceReasonCodeReevaluatedByCashier.Designer.cs` | Berkas Baru | Metadata dan snapshot model migrasi untuk EF Core yang memuat atribut `[DbContext(typeof(ApplicationDbContext))]`, `[Migration("20261014100000_FixClearanceReasonCodeReevaluatedByCashier")]`, serta implementasi penuh method `BuildTargetModel` yang diselaraskan dengan `ApplicationDbContextModelSnapshot.cs`. |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Modifikasi | Memperbarui constraint `CK_BilInpatientClearanceHandoff_ReasonCode` pada snapshot model EF Core agar menyertakan `'REEVALUATED_BY_CASHIER'` (baris 63457). |

### Spesifikasi Endpoint Bergaya Swagger

#### Health Services / Billing Management / Billing / Inpatient Clearance

Grup Endpoint: `[Tags("Health Services / Billing Management / Billing / Inpatient Clearance")]`  
Base URL: `/api/v1/health-services/billing-management/billing/inpatient-clearance`

| Method | Path | Deskripsi | Auth / Permission | Request Body | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/reevaluate` | Menilai ulang kelayakan kepulangan rawat inap dan menerbitkan handoff clearance versi baru | `BillingInpatient : Clearance` | `ReevaluateInpatientClearanceRequestDto` `{ encounterId, notes }` | `200 OK` (`InpatientClearanceHandoffDto`), `400 Bad Request`, `404 Not Found` |

---

## 4. Acceptance Criteria (`FIX-BKC-002-01`)

| No | Kriteria Penerimaan | Status | Bukti |
| :---: | --- | :---: | --- |
| 1 | Daftar `CK_BilInpatientClearanceHandoff_ReasonCode` pada `BilInpatientClearanceHandoffConfiguration.cs` memuat 7 nilai termasuk `REEVALUATED_BY_CASHIER`. | ✅ LULUS | `BilInpatientClearanceHandoffConfiguration.cs` baris 24 memuat 7 nilai. |
| 2 | `Up()` migration baru berisi tepat satu perubahan: penggantian constraint itu. `Down()` mengembalikan enam nilai semula. | ✅ LULUS | `20261014100000_FixClearanceReasonCodeReevaluatedByCashier.cs` Up() dan Down() terisolasi hanya pada constraint terkait. |
| 3 | Model snapshot ikut memuat 7 nilai. | ✅ LULUS | `ApplicationDbContextModelSnapshot.cs` baris 63457 memuat `'REEVALUATED_BY_CASHIER'`. |
| 4 | Empat jalur penulis lain (`INVOICE_SETTLED`, `PAYMENT_REVERSED`, `LATE_CHARGE_POSTED`, `DISCHARGE_ORDER_INITIATED`) tidak berubah perilakunya. | ✅ LULUS | Skema hanya memperlonggar whitelist nilai yang diizinkan, tidak mengubah kode atau relasi yang sudah ada. |
| 5 | Migration tidak diterapkan oleh builder ke database; pemilik menjalankan `dotnet ef database update` sendiri. | ✅ LULUS | Builder tidak mengeksekusi perintah database update runtime. |

---

## 5. Verifikasi dan Pengujian

- **Verifikasi Kompilasi (Build):** Sesuai instruksi langsung dari pemilik sistem (*"jangan lakukan build dotnet automatis. biar saya build sendiri"*), proses `dotnet build` tidak dijalankan secara otomatis oleh agen dan didelegasikan kepada pemilik sistem.
- **Pemeriksaan Statis:** Berkas konfigurasi, migrasi, designer, dan snapshot telah diverifikasi keselarasan sintaksis dan penamaan tipe C#.

---

### Catatan verifikasi tambahan, 14 Oktober 2026

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| Constraint di basis data dev sesudah migration diterapkan | `CK_BilInpatientClearanceHandoff_ReasonCode` memuat `REEVALUATED_BY_CASHIER` (query read-only) | `PASS` |
| Migration `20261014100000_FixClearanceReasonCodeReevaluatedByCashier` tercatat di `__EFMigrationsHistory` dev | Ada | `PASS` |
| `Up()` hanya mengganti satu constraint; `Down()` mengembalikan enam nilai | Dibaca langsung pada berkas | `PASS` |
| Selisih `ApplicationDbContextModelSnapshot.cs` terhadap cadangan sebelum task | Satu baris constraint (+ penanda BOM baris 1); perubahan payroll dan employee benefit milik pemilik tidak terpengaruh | `PASS` |
| Source tumpuan (`InpatientClearanceService`, `InpatientClearanceController`, konfigurasi settlement/invoice) tidak berubah sejak `d166cf4f` | Nol selisih | `PASS` |
| `dotnet build` | Dijalankan pemilik; hasilnya belum dilaporkan kepada agent | `NOT RUN` oleh agent |
| Tombol Evaluasi Ulang berhasil menyimpan surat versi baru | Tabel `BilInpatientClearanceHandoff` di dev **masih kosong**, sehingga belum ada bukti tombol itu pernah berhasil | `NOT RUN` |

Kriteria roadmap 1–5 terpenuhi pada tingkat source dan migration. Yang menahan tanda `✅`: hasil build
dan uji tombol di layar.

## 6. Peringatan untuk Gelombang Berikutnya

> [!WARNING]
> Pada perancangan `MVP-37` (desain revisi `2.1`), constraint `CK_BilInpatientClearanceHandoff_ReasonCode` akan diperluas kembali menjadi 11 nilai. Tim pengembang atau agen yang mengerjakan `MVP-37` **WAJIB** memastikan bahwa nilai `'REEVALUATED_BY_CASHIER'` tetap dipertahankan dalam daftar nilai constraint tersebut, agar perbaikan ini tidak terhapus kembali.
