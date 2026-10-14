# Laporan Perubahan Backend — `BE-FIN-094`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-094` |
| Judul | Jadwal angsuran yang disetujui otomatis terbit ke modul Payroll HR sebagai input variabel penggajian |
| Slice | `REV-18B3` — Sinkronisasi Outbound Payroll HR |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-224`; `FIN-DEC-190`; `FIN-DES-100` |
| Contract version | `FIN-INTEGRATION-1.8` §P.2 (`draft`, menunggu persetujuan pemilik) |
| Dependency | `BE-FIN-093` 🟡 — belum ✅, belum pernah dikompilasi. Dilanjutkan atas pola konfirmasi yang sama dengan task sebelumnya pada sesi ini |
| Klasifikasi | `HEAVY`, **cross-module** — menulis ke 3 tabel milik `Corporate/HumanResource/PayrollManagement` (`TrxPayrollRun`, `TrxPayrollRunEmployee`, `TrxPayrollVariableInput`), wewenang diberikan eksplisit pengguna pada sesi ini setelah blocker ditemukan dan didiskusikan |
| Task mode | `BACKEND`, diperluas eksplisit ke cross-module (Finance menulis ke `HumanResource/PayrollManagement` — **adapter sempit**, bukan mesin payroll run HR) atas konfirmasi pengguna |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Services/`, `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInstallmentPlanService.cs` (BE-FIN-093, diperluas), `Program.cs`, `appsettings.json` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis sebagai adapter sempit, bukan kontrak P.2 apa adanya** (kontrak asli secara struktural tidak dapat diimplementasikan — lihat §0). `dotnet build` **TIDAK DIJALANKAN**. Dibangun di atas `BE-FIN-093` yang juga belum ✅ — risiko compile majemuk 4 task |

---

## 0. Temuan blocker dan keputusan scope (WAJIB dibaca sebelum §1)

**Blocker yang ditemukan sebelum kode ditulis.** Kontrak `INT-FIN-HR-001` §P.2 menulis Finance memetakan `WorkforceProfileId` dan `PayrollPeriodId` langsung lalu menulis `TrxPayrollVariableInput` **di dalam transaksi persetujuan**. Diverifikasi ke source sebenarnya:

1. `TrxPayrollVariableInput` **tidak** punya kolom `WorkforceProfileId`/`PayrollPeriodId` — yang ada `PayrollRunEmployeeId` (FK ke `TrxPayrollRunEmployee`).
2. `TrxPayrollRunEmployee` terikat ke `TrxPayrollRun`, yang terikat ke periode. Baris ini **hanya pernah dibaca**, tidak pernah dibuat, oleh satu-satunya preseden cross-module existing (`LeavePayrollIntegrationService.cs`, HR/LeaveManagement) — service itu selalu mengasumsikan `TrxPayrollRunEmployee` sudah ada.
3. Diperiksa lebih lanjut: **folder `Corporate/HumanResource/PayrollManagement` sama sekali tidak punya Service atau Controller** untuk `TrxPayrollRun`/`TrxPayrollRunEmployee` — hanya model + configuration murni. Mesin pemrosesan payroll run (buka run, kumpulkan pegawai aktif, hitung, approve, bayar) **belum pernah dibangun** di repository ini.

Karena jadwal angsuran bisa mencakup periode berbulan-bulan ke depan, dan payroll run untuk periode yang jauh di depan nyaris pasti belum dibuka HR saat perjanjian disetujui, kontrak §P.2 **secara struktural tidak dapat diimplementasikan apa adanya**.

**Keputusan pengguna (sesi ini, dikonfirmasi lewat dua pertanyaan eksplisit):**
1. Lanjutkan, bukan berhenti — tutup gap ini sebagai bagian task.
2. Cakupannya **adapter sempit**: resolve-or-create `TrxPayrollRun`+`TrxPayrollRunEmployee` saat dibutuhkan. **Eksplisit BUKAN** membangun mesin payroll run HR yang sesungguhnya (itu epic tersendiri, butuh blueprint HR sendiri, jauh di luar satu task sore ini).

**Batas yang saya tetapkan sendiri di dalam adapter sempit itu** (dicatat sebagai keputusan desain, karena kontrak tidak menjawabnya eksplisit):

| Entity | Perlakuan | Alasan |
| --- | --- | --- |
| `MstPayrollPeriod` (kalender periode) | **Find-only.** Tidak pernah dibuat | Master data/konfigurasi kalender HR — bukan wewenang Finance |
| `MstPayrollComponent` (definisi komponen potongan) | **Find-only**, dicari lewat kode yang dikonfigurasi (`FinanceReceivablePayrollSyncOptions.PayrollComponentCode`), pola identik `LeavePayrollIntegrationOptions` | Konfigurasi bisnis payroll (perlakuan pajak dsb.) — Finance tidak berwenang mengarang |
| `TrxPayrollRun` | **Resolve-or-create**, status `Draft` minimal | Transaksional (`Trx*`), bukan master data — dianggap wajar dibuat sebagai efek samping, bukan keputusan bisnis |
| `TrxPayrollRunEmployee` | **Resolve-or-create**, snapshot nama/NIP dari `MstEmployee` | Sama alasannya dengan `TrxPayrollRun` |
| Kegagalan sinkron (periode/komponen belum ada) | **TIDAK membatalkan persetujuan perjanjian Finance.** Dicatat sebagai gap (`AuditAsync("Plan.PayrollSyncGap", ...)`), bukan exception yang di-throw | Finance MUST tetap bisa menyetujui perjanjian cicilannya sendiri — kegagalan konfigurasi HR bukan alasan memblokir hak Finance. Ini **penyimpangan dari kontrak** ("ditulis otomatis di dalam transaksi persetujuan" dibaca kontrak sebagai hard requirement); dicatat di sini, bukan diam-diam |

---

## 1. Proses bisnis

**Pemicu:** `POST /receivable-installment-plans/{id}/approve` (endpoint `BE-FIN-093`, sudah ada).

**Alur (baru, disisipkan ke `ApproveAsync` yang sudah ada):**
1. Seluruh baris `FinReceivableInstallment` dibangkitkan seperti biasa (perilaku `BE-FIN-093`, tidak berubah).
2. **Baru:** bila `receivable.BenefitOwnerId` terisi, `FinanceReceivablePayrollSyncService.SyncAsync` dipanggil di dalam transaksi yang sama:
   - Pegawai (`MstEmployee`) dicari dari `BenefitOwnerId`.
   - Komponen payroll dicari dari kode yang dikonfigurasi.
   - Untuk setiap baris angsuran: periode dicari (`MstPayrollPeriod`); bila ada, run dan run-pegawai di-resolve-or-create; baris `TrxPayrollVariableInput` di-upsert (idempoten, berbasis `PayrollRunEmployeeId`+`PayrollComponentId`+`SourceType`+`SourceId`) dengan `Amount = ScheduledAmount + CarriedOverAmount` (`FIN-DEC-190`).
   - Baris yang periodenya/komponennya tidak ditemukan **dilewati**, dicatat sebagai gap.
3. Persetujuan plan **tetap berhasil** apa pun hasil sinkron — lihat §0.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/integration-contract.md` P.2 (dibaca penuh, dibandingkan ke source)
- `Areas/Corporate/HumanResource/PayrollManagement/Models/{TrxPayrollRun,TrxPayrollRunEmployee,TrxPayrollVariableInput}.cs` (dibaca penuh)
- `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs`, `.../PayrollAndBenefit/Models/{MstPayrollPeriod,MstPayrollComponent}.cs`
- `Areas/Corporate/HumanResource/LeaveManagement/Services/LeavePayrollIntegrationService.cs` (1843 baris — dibaca method `UpsertVariableInputAsync`, `LoadContextAsync`, `GenerateVariableInputNumber` sebagai satu-satunya preseden cross-module existing)
- `Areas/Corporate/HumanResource/LeaveManagement/Services/LeavePayrollIntegrationOptions.cs` (pola konfigurasi kode komponen)
- `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/{TrxPayrollRun,TrxPayrollRunEmployee}Configuration.cs`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — tidak diperiksa ulang untuk prefix `Trx`/HR karena task ini tidak membuat entity baru di sana, hanya menulis baris ke tabel existing

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncOptions.cs` | **Baru.** `Enabled`, `PayrollComponentCode`, `CurrencyCode` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs` | **Baru.** `SyncAsync` + 4 method privat (resolve period/run/run-employee, upsert variable input) + 2 generator nomor |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInstallmentPlanService.cs` | `ApproveAsync` (BE-FIN-093) diperluas: constructor menerima `FinanceReceivablePayrollSyncService`; panggilan `SyncAsync` disisipkan sebelum `SaveChangesAsync` tunggal |
| `Program.cs` | `Configure<FinanceReceivablePayrollSyncOptions>` + `AddScoped<FinanceReceivablePayrollSyncService>()` |
| `appsettings.json` | Seksi baru `Finance:ReceivablePayrollSync` (`Enabled: true`, `PayrollComponentCode: "FINANCE_INSTALLMENT_DEDUCTION"`, `CurrencyCode: "IDR"`) |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint baru. Perilaku `POST .../approve` (BE-FIN-093) berubah **secara internal** (kini juga menulis ke HR), kontrak responsnya tidak berubah |
| Database | `NOT APPLICABLE` — nol migration. Tiga tabel HR existing (`TrxPayrollRun`, `TrxPayrollRunEmployee`, `TrxPayrollVariableInput`) menerima baris baru lewat EF, skema tidak disentuh |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint baru, nol perubahan `[Authorize]`/`[AccessAction]`. **Lintas modul:** Finance kini menulis langsung ke tabel HR lewat `ApplicationDbContext` yang sama (monolith satu database) — pola yang sama dengan Finance menulis `BilArHandoff.Status` (Billing) sejak `FinanceBillingIntakeService` lama |

---

## 3. Temuan lain yang dicatat, bukan didiamkan

**A. Penyimpangan dari kontrak P.2 field mapping.** Kontrak menyebut `FinReceivableInstallmentPlan.WorkforceProfileId` dan `FinReceivableInstallment.TargetPayrollPeriodId` — **tidak satu pun field ini ada** pada entity `BE-FIN-092`. Diikuti struktur source yang sebenarnya: `WorkforceProfileId` diresolusi dari `FinReceivable.BenefitOwnerId` → `MstEmployee.Id` → `MstEmployee.WorkforceProfileId` (rantai 2 lompatan), dan periode tetap memakai `FinReceivableInstallment.DeductionPeriod` (string `YYYY-MM`) yang memang sudah ada — tidak ada kolom Guid periode yang perlu ditambahkan.

**B. `RunNumber` pada adapter ini dibuat per-periode, bukan per-pegawai.** Satu `TrxPayrollRun` dipakai bersama oleh seluruh pegawai yang kebetulan punya angsuran pada periode yang sama (dicari lewat `PayrollPeriodId` + `RunType = "Regular"`, run pertama yang ditemukan dipakai ulang) — mendekati makna aslinya ("satu run per periode mencakup banyak pegawai"), tetapi `LegalEntityId`/`HospitalSiteId` run diwarisi dari `MstPayrollPeriod`, bukan dari pegawainya sendiri. Bila HR punya run terpisah per legal entity/site untuk periode yang sama, adapter ini **tidak** menyaringnya — berpotensi menulis ke run yang salah cakupannya. Dicatat sebagai keterbatasan, bukan ditebak lebih jauh karena data cakupan legal entity per pegawai tidak tersedia di sisi Finance.

**C. `MstPayrollComponent` dengan kode `FINANCE_INSTALLMENT_DEDUCTION` (default) hampir pasti belum ada.** Sama seperti `LEAVE_ALLOWANCE`/`LEAVE_ENCASHMENT` pada `LeavePayrollIntegrationOptions`, baris master data ini **MUST** dibuat HR lewat layar konfigurasi komponen payroll (jenis `Deduction`) sebelum sinkron pertama kali berhasil. Sampai saat itu, `SyncAsync` akan melewati **seluruh** baris dengan pesan jelas di `SkipReasons`, dan persetujuan perjanjian tetap berhasil (§0).

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review diff manual (service baru, perluasan `ApproveAsync`, DI, konfigurasi) | Dicocokkan terhadap struktur `TrxPayrollRun`/`TrxPayrollRunEmployee`/`TrxPayrollVariableInput` yang sebenarnya, bukan asumsi kontrak | `PASS` (review manual, bukan compiler) | §0-§3 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile majemuk 4 task** (`BE-FIN-091`, `092`, `093`, `094`) — belum satu pun dikompilasi. Task ini menambah risiko lebih jauh karena menyentuh namespace HR yang belum pernah dipakai Finance sebelumnya (kemungkinan nama class/property salah ketik lebih tinggi daripada task sebelumnya yang murni di dalam domain Finance sendiri).

Uji manual: `NOT FEASIBLE` — butuh build sukses, migration `BE-FIN-092` diterapkan, data piutang `EMPLOYEE_BENEFIT` nyata, **dan** baris `MstPayrollComponent`/`MstPayrollPeriod` HR yang valid; tak satu pun tersedia pada sesi ini.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.2.1` Baris jadwal tercatat akurat di payroll run periode terkait | **Terpenuhi struktural dengan keterbatasan dicatat (§3.B), belum terverifikasi runtime.** `SourceType`/`SourceId` tertaut balik ke `FinReceivableInstallment.Id` sesuai `FIN-DEC-190` | `FinanceReceivablePayrollSyncService.UpsertVariableInputAsync` |

**DoD roadmap** ("Build PASS, verifikasi penulisan data variabel, laporan task"): **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan, verifikasi penulisan data butuh database nyata.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Kontrak `INT-FIN-HR-001` §P.2 **tidak diimplementasikan apa adanya** — diganti adapter sempit atas keputusan pengguna, didokumentasikan §0. Pemilik `HR-Owner` MUST meninjau dan meresmikan penyimpangan ini (termasuk keputusan "gagal sinkron tidak membatalkan approve"), bukan menganggapnya sudah disetujui lewat laporan task ini saja. (2) Source belum pernah dikompilasi |
| Masalah yang diketahui | (1) `MstPayrollComponent` dengan kode default hampir pasti belum ada — sinkron pertama **akan** melewati semua baris sampai HR membuatnya. (2) Resolusi run tidak menyaring legal entity/hospital site per pegawai (§3.B) |
| Risiko tersisa | (1) Risiko compile 4 task majemuk. (2) `FinanceReceivablePayrollSyncService` BUKAN mesin payroll run HR — bila HR kelak membangun mesin run sungguhan, kemungkinan perlu rekonsiliasi dengan run `Draft` "yatim" yang dibuat adapter ini. (3) Field mapping kontrak P.2 perlu direvisi resmi di blueprint (temuan §3.A) supaya dokumentasi tidak lagi menyebut field yang tidak ada di source |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini. Dua `AskUserQuestion` dijawab pengguna sebelum implementasi dimulai (lanjutkan vs berhenti; adapter sempit vs mesin payroll run penuh) — keduanya dicatat §0, bukan interupsi sesi |
| Status Git | `git status --short`: 2 file baru (`FinanceReceivablePayrollSyncOptions.cs`, `FinanceReceivablePayrollSyncService.cs`), `FinanceReceivableInstallmentPlanService.cs` (BE-FIN-093) diperluas, `Program.cs` + `appsettings.json` dimodifikasi |
| Langkah berikutnya | (1) Pemilik HR meresmikan atau mengoreksi keputusan adapter sempit §0 — idealnya jadi amandemen kontrak `INT-FIN-HR-001` resmi, bukan penyimpangan senyap. (2) HR membuat baris `MstPayrollComponent` kode `FINANCE_INSTALLMENT_DEDUCTION` (atau kode lain, lalu `appsettings.json` disesuaikan). (3) Pemilik menjalankan `dotnet build` untuk memvalidasi `BE-FIN-091`..`094` sekaligus. (4) Setelah build bersih dan migration diterapkan, uji end-to-end satu perjanjian disetujui dan verifikasi baris `TrxPayrollVariableInput` yang terbit |
