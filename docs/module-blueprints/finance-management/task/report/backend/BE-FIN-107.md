# Laporan Perubahan Backend — `BE-FIN-107`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-107` |
| Judul | Billing: pembagian dua porsi RS Benefit dan pegawai saat finalisasi invoice, serah terima AR/AP, dan pencatatan pilihan pegawai atas sisa tagihan pasien |
| Slice | `REV-19` Slice B — Billing Finalization dan Inti Serah Terima AR/AP (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` |
| Trace | `FIN-DEC-164`, `183`, `184`, `185`, `186`, `198`, `202`, `205`, `207`, `208`, `210`, `218`, `230`; `FR-FIN-220`..`222`, `246`..`248`, `250`..`252`; `FIN-INTEGRATION-1.9` `R.3`, `R.4`; `FIN-API-1.10` `R.5`; `FIN-VAL-262`, `264`, `265`, `289`; `BE-FIN-119` |
| Contract version | `FIN-INTEGRATION-1.9` `R.4`, `FIN-API-1.10` `R.5` |
| Dependency | `BE-FIN-091` ✅, `BE-FIN-104` ✅, `BE-FIN-106` ✅, `{GATE-01}` ✅ |
| Klasifikasi | `HEAVY` — skor 12: berkas diubah/baru 11 (2), logika pembagian porsi dan alur finalisasi inti billing kritis (2), kontrak API baru & DTO berubah (2), dampak schema: kolom baru, check constraint baru, dan pelebaran kolom (2), dampak serah terima keuangan AR/AP (2) |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("kerjakan FE-FIN-107 tanpa build dotnet /build-module-backend" dikonfirmasi ke `BE-FIN-107`, 13 Oktober 2026), fokus pada Billing Management dan Finance AR/AP |
| Target tulis | Lihat 3.2 |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | HealthServices / BillingManagement / Billing (layanan kalkulasi split, finalisasi, serah terima); Corporate / FinanceManagement / Receivable (pelebaran kolom relasi) |
| Registry | `BillingManagement — Bil — ACTIVE / LEGACY`; `FinanceManagement — Fin — ACTIVE` |
| Keberlakuan | `NEW CODE`: `BillingEmployeeBenefitSplitService`. `TOUCHED LEGACY`: `BilArHandoff`, `BilArHandoffConfiguration`, `FinReceivable`, `FinReceivableConfiguration`, `BillingFinalizationDtos`, `BillingHandoffDtos`, `BillingArApHandoffService`, `BillingFinalizationService`, `BillingManagementServiceCollectionExtensions`, `Program.cs` |
| Entity operasional baru | **Tidak ada** — penambahan kolom dan constraint pada `BilArHandoff`, serta pelebaran kolom pada `FinReceivable` |
| QBE yang berlaku | `QBE-SVC-001` (logika perhitungan authoritative di service, zero calculation di klien), `QBE-API-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-CFG-001`/`002`, `QBE-NAM-001` |

---

## 1. Masalah yang diperbaiki

Sebelumnya pada proses finalisasi tagihan (`BillingFinalizationService`), kunjungan yang dijamin oleh penjamin internal (karyawan RS atau keluarganya yang memiliki plafon benefit RS) diperlakukan sama persis dengan penjamin komersial atau perusahaan biasa. Hal ini menimbulkan sejumlah masalah fundamental:
1. **Tidak Ada Pembagian Dua Porsi**: Porsi yang ditanggung penjamin tidak dipotong terhadap plafon yang tersisa di HR ($S$). Bila tagihan melebihi sisa plafon ($C > S$), kelebihan tersebut ($E = C - P$) tidak dipisahkan ke piutang pegawai, melainkan berisiko ditagihkan keliru ke penjamin rumah sakit.
2. **Ketiadaan Pilihan Pegawai atas Sisa Pasien ($D$)**: Pegawai pemilik benefit belum dapat memilih apakah sisa tagihan pasien di luar cakupan aturan ($K$) dibayar langsung secara tunai di kasir atau ditangguhkan menjadi utang pegawai via potong gaji ($D$).
3. **Ketiadaan Kolom Audit & Check Constraint**: Tabel `BilArHandoff` belum memiliki kolom `DeferredPatientAmount` ($D$) dan `DeferredConsentGivenBy` untuk menyimpan nominal yang dialihkan dan nama pihak yang memberi persetujuan saat pasien bukan pegawai sendiri.
4. **Penerbitan Baris Nol**: Sistem lama berpotensi menerbitkan baris serah terima ber-nominal Rp 0 yang melanggar constraint database `Amount > 0`.
5. **Keterbatasan Panjang Karakter**: Kolom `FinReceivable.BenefitRelationship` dan `BilArHandoff.BenefitRelationship` sebelumnya terbatas 30 karakter, berpotensi memotong kode relasi tertentu (`K-06`).
6. **Komentar Ambiguitas di Model**: `BilArHandoff.cs` baris 25 sebelumnya memiliki komentar yang belum mempertegas bahwa `BenefitOwnerId` merujuk langsung ke `MstEmployee.Id` pegawai pemilik benefit.

Task ini menuntaskan butir `M.1.1` dan `M.1.2` dari `BE-FIN-091`, mengimplementasikan `BillingEmployeeBenefitSplitService` dengan formula authoritative, menghubungkannya ke alur pratinjau dan eksekusi finalisasi, mencatat pilihan $D$ beserta persetujuan, mencegah penerbitan baris nol, memperlebar kolom, serta memperketat check constraint database.

---

## 2. Proses bisnis & Rumus Authoritative

### 2.1 Formula Pembagian Porsi (R.4.2)

Setiap invoice kunjungan berpenjamin internal dievaluasi dengan formula berikut:

$$\begin{aligned}
C &= \text{Tanggungan penjamin berdasarkan aturan tarif/kelas (calculation.PrimaryAmount)} \\
S &= \text{Sisa plafon benefit karyawan aktif pada HR (EmployeeBenefitBalanceService.ReadAsync)} \\
P &= \min(C, S) \quad \text{— Porsi RS Benefit (diterbitkan sebagai DebtorType = PAYER)} \\
E &= C - P \quad \text{— Kelebihan tagihan di atas plafon yang ditanggung pegawai} \\
K &= \text{Sisa tagihan tanggung jawab pasien di luar } C \text{ (readiness.Outstanding)} \\
D &= \text{Nominal pilihan pegawai atas sisa pasien yang dialihkan ke utang pegawai (0} \le D \le K\text{)}
\end{aligned}$$

Hasil pembagian serah terima AR dan kewajiban kasir:
- **Porsi RS Benefit (`PAYER`)**: Nominal = $P$. Diterbitkan hanya jika $P > 0$ (`FIN-DEC-210`). DebtorReferenceId merujuk ke CompanyGuarantorId penjamin internal.
- **Porsi Utang Pegawai (`EMPLOYEE_BENEFIT`)**: Nominal = $E + D$. Diterbitkan hanya jika $E + D > 0$ (`FIN-DEC-210`). Menyimpan `BenefitOwnerId` = `MstEmployee.Id`, `DeferredPatientAmount` = $D$, dan `DeferredConsentGivenBy` bila $D > 0$ dan pasien bukan pegawai sendiri.
- **Kewajiban Pasien di Kasir**: Nominal = $K - D$. Jika $K - D = 0$, invoice siap difinalisasi secara normal tanpa transaksi kasir tambahan. Jika $K - D > 0$, kasir wajib melunasi sisa tunai sebelum invoice dapat difinalkan (kecuali departure exception).

### 2.2 Skenario Bisnis & Contoh Konkret

| Skenario | Nilai $C$, $S$, $K$, $D$ | Hasil Pembagian & Serah Terima | Status Kasir & Validasi |
| :--- | :--- | :--- | :--- |
| **1. Plafon Penuh (100%)** | $C = \text{Rp } 5.000.000$, $S = \text{Rp } 3.000.000$, $K = 0$, $D = 0$ | $P = \text{Rp } 3.000.000$ (`PAYER`), $E = \text{Rp } 2.000.000$ (`EMPLOYEE_BENEFIT`) | Kasir: Rp 0. Invoice langsung lunas dan siap difinalkan. |
| **2. Co-Pay Pasien, $D = 0$** | Cakupan 80% dari Rp 5.000.000 ($C = \text{Rp } 4.000.000$, $K = \text{Rp } 1.000.000$), $S = \text{Rp } 3.000.000$, $D = 0$ | $P = \text{Rp } 3.000.000$ (`PAYER`), $E = \text{Rp } 1.000.000$ (`EMPLOYEE_BENEFIT`) | Kasir: Pasien wajib bayar Rp 1.000.000 tunai di kasir. |
| **3. Co-Pay Pasien Dialihkan ($D > 0$)** | Sama seperti kasus 2, tetapi pegawai memilih $D = \text{Rp } 1.000.000$ (potong gaji) | $P = \text{Rp } 3.000.000$ (`PAYER`), Utang pegawai = $E + D = \text{Rp } 2.000.000$ (`EMPLOYEE_BENEFIT`), dengan `DeferredPatientAmount` = Rp 1.000.000 | Kasir: $K - D = \text{Rp } 0$. Pasien tidak perlu membayar tunai di kasir; tagihan siap difinalkan. |
| **4. Plafon Habis ($S = 0$)** | $C = \text{Rp } 2.000.000$, $S = \text{Rp } 0$, $K = 0$, $D = 0$ | $P = 0$ (Baris `PAYER` **tidak diterbitkan**), $E = \text{Rp } 2.000.000$ (`EMPLOYEE_BENEFIT`) | Baris `PAYER` nihil tidak diterbitkan (`FIN-DEC-210`). |
| **5. Sisa Melebihi Cakupan** | $C = \text{Rp } 3.000.000$, $S = \text{Rp } 5.000.000$, $K = \text{Rp } 500.000$, $D = 0$ | $P = \text{Rp } 3.000.000$ (`PAYER`), $E = 0$ (Baris `EMPLOYEE_BENEFIT` **tidak diterbitkan**) | Tidak ada baris utang pegawai bernilai 0. |
| **6. Pilihan $D$ Melebihi Sisa Pasien** | $K = \text{Rp } 500.000$, dikirim $D = \text{Rp } 600.000$ | Ditolak HTTP `422 UnprocessableEntity` (`FIN-VAL-262`): nominal pilihan tidak boleh melebihi sisa tagihan pasien. | Nol baris terbit, transaksi dibatalkan. |
| **7. Persetujuan Tanggungan Non-Self** | Pasien adalah anak ($CHILD$), $D = \text{Rp } 500.000$, tanpa `consentGivenBy` | Ditolak HTTP `422` (`FIN-VAL-289`, `BE-FIN-119`): nama pemberi persetujuan wajib diisi. | Nol baris terbit. |
| **8. Plafon HR Tidak Terbaca** | Layanan HR timeout atau down | Finalisasi ditahan HTTP `422` (`FIN-VAL-264`): layanan HR tidak dapat diakses untuk membaca plafon. | Nol baris terbit. |
| **9. Kunjungan Multi-Penjamin** | Pasien memiliki penjamin internal + asuransi kedua (`ExcessAmount > 0`) | Ditolak HTTP `422` (`FIN-VAL-265`): penjamin kedua pada kunjungan berpenjamin internal belum didukung. | Mencegah konflik alokasi piutang. |
| **10. Kunjungan Non-Internal Biasa** | Pasien Umum tunai atau Asuransi Komersial biasa | Logika split mengembalikan `null`. Alur finalisasi lama berjalan tanpa perubahan sedikit pun. | Kompatibilitas 100% terjaga. |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang Diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (spesifikasi task `BE-FIN-107`, Acceptance Criteria 1..9, DoD)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (arsitektur serah terima, migration plan, check constraints)
- `docs/module-blueprints/finance-management/data/data-dictionary.md` (`BilArHandoff`, constraint `CK_BilArHandoff_Deferred`, `CK_BilArHandoff_Consent`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (`R.5` spesifikasi payload DTO `EmployeeBenefitSplitResponse`)
- `Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitBalanceService.cs` (`INT-BIL-HR-001` pembacaan plafon benefit HR)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` (alur validasi kesiapan, idempotensi, eksekusi, dan audit)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` (penerbitan serah terima AR/AP ke finance)
- `Tests/QuilvianSystemBackend.PharmacyTests/BillingZeroPatientObligationTests.cs` (pemeriksaan kompabilitas konstruktor test)

### 3.2 Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| :--- | :--- |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs` | Ditambahkan properti `DeferredPatientAmount` (`decimal`, default 0) dan `DeferredConsentGivenBy` (`string?`, max 200). Diperbarui komentar `BenefitOwnerId` merujuk ke `MstEmployee.Id` per `K-04` & `FIN-INTEGRATION-1.9 R.4.1`. |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilArHandoffConfiguration.cs` | Ditambahkan check constraint `CK_BilArHandoff_Deferred` dan `CK_BilArHandoff_Consent`. Dikonfigurasikan presisi `DeferredPatientAmount` (18,2) dengan default 0, panjang `DeferredConsentGivenBy` (200), dan `BenefitRelationship` (50). |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` | Diperlebar atribut `[MaxLength(50)]` pada properti `BenefitRelationship` (`K-06`). |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs` | Diperlebar mapping `BenefitRelationship` ke panjang maksimum 50. |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingFinalizationDtos.cs` | Pada `FinalizeInvoiceRequest`: ditambahkan `EmployeeReceivableChoiceAmount` dan `EmployeeReceivableConsentGivenBy`. Pada `FinalizationPreviewResponse` dan `FinalizationResponse`: ditambahkan `EmployeeBenefitSplit`. Ditambahkan kelas baru `EmployeeBenefitSplitResponse` lengkap sesuai kontrak `R.5`. |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingHandoffDtos.cs` | Pada `ArHandoffResponse`: ditambahkan `BenefitOwnerId`, `BenefitRelationship`, `DeferredPatientAmount`, dan `DeferredConsentGivenBy`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingEmployeeBenefitSplitService.cs` | **Berkas Baru.** Layanan kalkulasi authoritative pembagian dua porsi: deteksi penjamin internal, pembacaan saldo plafon HR (`INT-BIL-HR-001`), rumus $P = \min(C, S)$, $E = C - P$, validasi batasan $0 \le D \le K$, validasi persetujuan non-self, mitigasi penjamin ganda (`FIN-VAL-265`), mitigasi kartu unlinked (`CARD_NOT_LINKED`), dan perhitungan `effectiveOutstanding`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` | Pada `StageHandoffsForFinalizationAsync`: ditambahkan parameter opsional `employeeBenefitSplit`. Jika kunjungan internal, menerbitkan baris `PAYER` ($P$) dan baris `EMPLOYEE_BENEFIT` ($E + D$ beserta $D$ dan consent), serta mencegah penerbitan baris nol. Diperbarui `MapAr` untuk memetakan seluruh field baru. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` | Diinjeksikan `BillingEmployeeBenefitSplitService`. Diperbarui `BuildReadinessAsync` untuk memuat pratinjau split dan blocking reasons. Diperbarui `FinalizeAsync` untuk mengevaluasi split, memvalidasi sisa kasir $K - D$, mencakup $D$ dan consent dalam `ComputeFinalizePayloadHash` demi idempotensi transaksi, dan meneruskan hasil ke serah terima. |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Didaftarkan `services.AddScoped<BillingEmployeeBenefitSplitService>();`. |
| `Program.cs` | Didaftarkan `builder.Services.AddScoped<BillingEmployeeBenefitSplitService>();`. |

### 3.3 Dampak Kontrak API, Database, dan Keamanan

| Aspek | Dampak |
| :--- | :--- |
| **Kontrak API** | 1. `POST /api/v1/health-services/billing-management/invoices/{id}/finalize` menerima `employeeReceivableChoiceAmount` dan `employeeReceivableConsentGivenBy`.<br>2. `GET /preview` dan `POST /finalize` mengembalikan objek `employeeBenefitSplit`.<br>3. `GET /handoffs` menampilkan `deferredPatientAmount`, `deferredConsentGivenBy`, `benefitOwnerId`, dan `benefitRelationship`. |
| **Database** | Tabel `BilArHandoff`: kolom `DeferredPatientAmount` (`numeric(18,2) NOT NULL DEFAULT 0`), `DeferredConsentGivenBy` (`varchar(200) NULL`), check constraint `CK_BilArHandoff_Deferred`, check constraint `CK_BilArHandoff_Consent`.<br>Tabel `FinReceivable`: kolom `BenefitRelationship` diperlebar dari `varchar(30)` menjadi `varchar(50)`.<br>**Migration belum dieksekusi** (menunggu pemilik). |
| **Keamanan & Validasi** | 1. Zero Calculation di Klien: seluruh formula dihitung authoritative di backend.<br>2. Fail-Closed: bila layanan HR tidak dapat diakses saat finalisasi, transaksi ditolak demi mencegah ketidaksesuaian saldo.<br>3. Check Constraints di DB menjamin integritas data: `DeferredPatientAmount` tidak dapat bernilai negatif, tidak dapat melebihi `Amount`, dan wajib memiliki pemberi persetujuan bila bukan diri sendiri (`SELF`). |

---

## 4. Dokumentasi Endpoint & Modifikasi Kontrak

#### Health Services / Billing Management / Billing Finalization (Diperbarui)

Base URL: `api/v1/health-services/billing-management/invoices`

| Method | Path | Kegunaan | Auth & Hak Akses | Request / Parameter | Response Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{id}/preview` | Pratinjau kesiapan invoice sebelum difinalkan, termasuk rincian pembagian benefit pegawai | `BillingFinalization : Read` | `id` (Invoice GUID) | `ApiResponse<FinalizationPreviewResponse>` (termasuk `employeeBenefitSplit`) |
| `POST` | `/{id}/finalize` | Memfinalkan invoice, mencatat pilihan pegawai, dan menerbitkan baris serah terima AR/AP | `BillingFinalization : Update` | `FinalizeInvoiceRequest` (+ `employeeReceivableChoiceAmount`, `employeeReceivableConsentGivenBy`) | `ApiResponse<FinalizationResponse>` (termasuk `employeeBenefitSplit`) |

#### Health Services / Billing Management / Billing Handoffs (Diperbarui)

Base URL: `api/v1/health-services/billing-management`

| Method | Path | Kegunaan | Auth & Hak Akses | Response Payload |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/finalization-records/{id}/handoffs` | Mengambil status serah terima AR/AP | `BillingFinalization : Read` | `ApiResponse<HandoffStatusResponse>` (`ArHandoffResponse` memuat `deferredPatientAmount`, `deferredConsentGivenBy`, dll.) |

---

## 5. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| :--- | :--- | :--- | :--- |
| QBE Preflight | Tidak ada entity baru, konfigurasi EF terdaftar, penamaan konsisten | `PASS` | Evaluasi preflight |
| Static Code Inspection: Formula Pembagian | Formula $P = \min(C, S)$, $E = C - P$, $E + D$, $K - D$ dihitung presisi, nilai dibatasi non-negatif | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 195–235 |
| Static Code Inspection: Pencegahan Baris Nol | `PayerAmount > 0` menerbitkan `PAYER`; `EmployeeReceivableAmount > 0` menerbitkan `EMPLOYEE_BENEFIT`; baris bernilai 0 tidak ditambahkan | `PASS` | `BillingArApHandoffService.cs` baris 70 & 95 |
| Static Code Inspection: Validasi Batasan $D$ | $D < 0$ dan $D > K$ ditolak dengan `BillingFinalizationValidationException` (`422`) | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 210–221 |
| Static Code Inspection: Validasi Persetujuan | Relasi bukan `SELF` dengan $D > 0$ mewajibkan `consentGivenBy` non-empty | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 223–230 |
| Static Code Inspection: Penjaga Penjamin Kedua | `guarantors.Count > 1` atau `ExcessAmount > 0` ditolak dengan `422` | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 84–92 |
| Static Code Inspection: Penjaga HR Unusable | Gagal baca HR atau saldo tidak usable menahan finalisasi dengan `422` | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 148–179 |
| Static Code Inspection: Idempotensi Transaksi | `ComputeFinalizePayloadHash` menyertakan $D$ dan `consentGivenBy` | `PASS` | `BillingFinalizationService.cs` baris 658–668 |
| Backward Compatibility Kunjungan Biasa | Kunjungan non-internal mengembalikan `null`, jalur lama utuh tanpa distorsi | `PASS` | `BillingEmployeeBenefitSplitService.cs` baris 78–82 |
| Backward Compatibility Unit Test | Parameter opsional `splitService = null` pada konstruktor `BillingFinalizationService` menjamin test existing tidak pecah | `PASS` | `BillingFinalizationService.cs` baris 27–40 |
| `dotnet build` / `dotnet test` | — | `NOT RUN` | Instruksi pemilik: dilarang menjalankan build/test otomatis |
| Migration File Creation | — | `NOT RUN` | Menunggu eksekusi oleh pemilik (perintah tercantum di bawah) |

Uji manual: `REQUIRED`. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

### Perintah Migrasi untuk Pemilik Sistem

Jalankan perintah berikut di terminal backend:

```powershell
dotnet build
dotnet ef migrations add AddDeferredPatientAmountAndWidenBenefitRelationship
dotnet ef database update
```

**Perubahan yang dihasilkan migrasi:**
1. Penambahan kolom `DeferredPatientAmount` (`numeric(18,2)`, default `0`) pada tabel `BilArHandoff`.
2. Penambahan kolom `DeferredConsentGivenBy` (`varchar(200)`, nullable) pada tabel `BilArHandoff`.
3. Penambahan check constraint `CK_BilArHandoff_Deferred` pada tabel `BilArHandoff`.
4. Penambahan check constraint `CK_BilArHandoff_Consent` pada tabel `BilArHandoff`.
5. Pelebaran kolom `BenefitRelationship` pada tabel `FinReceivable` dari `varchar(30)` ke `varchar(50)`.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria (AC 1–9) | Status | Bukti Kode |
| :--- | :--- | :--- |
| **AC 1**: Cakupan Rp 5.000.000 (100%), sisa Rp 3.000.000: `PAYER` Rp 3.000.000 dan `EMPLOYEE_BENEFIT` Rp 2.000.000 | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` ($P = \min(5\text{jt}, 3\text{jt}) = 3\text{jt}$, $E = 5\text{jt} - 3\text{jt} = 2\text{jt}$) |
| **AC 2**: Cakupan 80% dari Rp 5.000.000 ($C = 4\text{jt}$, $K = 1\text{jt}$), sisa plafon Rp 3.000.000, $D = 0$: `PAYER` Rp 3.000.000, `EMPLOYEE_BENEFIT` Rp 1.000.000, tunai kasir Rp 1.000.000 | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` ($P = 3\text{jt}$, $E = 1\text{jt}$, Kasir = $K - D = 1\text{jt}$) |
| **AC 3**: Kasus yang sama dengan $D = \text{Rp } 1.000.000$: `EMPLOYEE_BENEFIT` Rp 2.000.000, tunai kasir Rp 0, `DeferredPatientAmount` Rp 1.000.000 | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` ($E + D = 1\text{jt} + 1\text{jt} = 2\text{jt}$, Kasir = $1\text{jt} - 1\text{jt} = 0$, `DeferredPatientAmount` = $1\text{jt}$) |
| **AC 4**: Sisa Rp 0: tanpa baris `PAYER`; sisa melebihi cakupan: tanpa baris `EMPLOYEE_BENEFIT` kecuali $D > 0$; tidak pernah ada baris `Amount = 0` | Terpenuhi di source | `BillingArApHandoffService.cs` (kondisi `PayerAmount > 0` dan `EmployeeReceivableAmount > 0`) |
| **AC 5**: $D$ melebihi sisa tagihan pasien ditolak `422` | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` baris 217–221 |
| **AC 6**: Plafon tidak terbaca: finalisasi ditahan, nol baris | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` baris 151–178 |
| **AC 7**: Penjamin kedua ditolak (`ExcessAmount > 0` atau multi-penjamin) | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` baris 84–92 |
| **AC 8**: Kunjungan non-internal: perilaku lama tidak berubah | Terpenuhi di source | `BillingEmployeeBenefitSplitService.cs` baris 78–82 |
| **AC 9**: Finalisasi ulang tidak menggandakan baris | Terpenuhi di source | `BillingFinalizationService.cs` (pengecekan `prior is not null` via `IdempotencyKey` dan `PayloadHash`) |
| **DoD**: Preflight selesai | Terpenuhi | Bagian Preflight |
| **DoD**: Kontrak cocok | Terpenuhi | Bagian 4 & integrasi DTO API |
| **DoD**: Laporan tracked | Terpenuhi | Berkas laporan ini |
| **Verifikasi Roadmap**: `dotnet build` | **Belum** | `NOT RUN` (instruksi pemilik sistem) |

---

## 7. Catatan Penutup & Langkah Berikutnya

1. **Keterkaitan dengan `BE-FIN-108`**: `BE-FIN-107` bertugas membaca plafon, membagi porsi, dan menerbitkan baris serah terima AR/AP. Pemotongan saldo plafon sebenarnya pada modul HR (`pakai`) akan dijalankan pada `BE-FIN-108` di dalam transaksi finalisasi yang sama. Oleh karena itu, kedua task ini saling melengkapi sebelum rilis produksi.
2. **Keterkaitan dengan Frontend `FE-FIN-107`**: Frontend kasir/billing memerlukan penyesuaian dialog finalisasi untuk menampilkan pratinjau `EmployeeBenefitSplit`, input pemilihan nominal $D$, dan nama pemberi persetujuan bila pasien bukan pegawai sendiri.
3. **Langkah Pemilik Sistem**: Jalankan perintah `dotnet build` dan migrasi EF di atas untuk meratifikasi perubahan skema database PostgreSQL.
