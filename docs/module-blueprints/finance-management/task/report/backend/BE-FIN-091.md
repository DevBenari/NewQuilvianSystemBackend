# Laporan Perubahan Backend — `BE-FIN-091`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-091` |
| Judul | Serah terima tagihan manfaat karyawan 2 baris dari Billing dikonsumsi dengan aman ke piutang pegawai dan piutang penjamin internal |
| Slice | `REV-18B1` — Fondasi Skema & Intake Serah Terima |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-220`..`222`; `FIN-DEC-183`..`186`; `FIN-DES-100`; `FIN-VAL-247` |
| Contract version | `FIN-INTEGRATION-1.8` §P.1 — `draft` (menunggu persetujuan pemilik); `FIN-VAL-1.11` `FIN-VAL-247` — `approved` |
| Dependency | Tidak ada task backend lain yang menjadi prasyarat (roadmap: `—`). Prasyarat eksekusi REV-18 butir 2 ("Migration Billing `AddEmployeeBenefitColumnsToBilArHandoff`") **ditutup oleh task ini sendiri**, atas wewenang cross-module eksplisit yang diberikan pengguna pada sesi ini (lihat bagian 0) |
| Klasifikasi | `MEDIUM` — perluasan skema pada 1 entity existing (`BilArHandoff`, di luar module Finance), 1 migration, dan perluasan logika pada 1 method existing (`ProcessArIntakeAsync`). Nol endpoint baru, nol DTO baru |
| Task mode | `BACKEND`, diperluas eksplisit ke cross-module (Finance menulis `BillingManagement/Billing` **hanya untuk skema**: model, configuration, migration — bukan logika bisnis/emisi) atas konfirmasi pengguna pada sesi ini |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/BillingIntake/`, `Areas/HealthServices/BillingManagement/Billing/Models/`, `Repositories/Configurations/HealthServices/BillingManagement/Billing/`, `Migrations/` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source dan migration selesai ditulis.** `dotnet build` dan `dotnet ef database update` **TIDAK DIJALANKAN** — instruksi eksplisit pengguna ("tanpa build automatis", dipertegas kembali: "jangan jalankan build migration, saya akan jalankan sendiri"). Migration **belum diterapkan** ke database. Uji manual **NOT FEASIBLE** (lihat bagian 5) |

---

## 0. Backend Governance Preflight dan keputusan scope

**Preflight Billing** (`Areas/HealthServices/BillingManagement/Billing/`): Area `HealthServices`, Module `BillingManagement/Billing`, prefix `Bil`, status registry `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27). Applicability: `TOUCHED LEGACY` (menambah kolom pada entity existing `BilArHandoff`, bukan entity baru) — QBE-MOD-002/003 tidak memblokir karena module sudah terdaftar.

**Preflight Finance** (`Areas/Corporate/FinanceManagement/BillingIntake/`): Area `Corporate`, Module `FinanceManagement/BillingIntake`, prefix `Fin`, status registry `ACTIVE` (baris 14). Applicability: `NEW CODE` untuk baris yang ditambahkan ke `ProcessArIntakeAsync` (validasi + penyalinan field baru); kode existing di sekitarnya tetap `UNTOUCHED LEGACY`.

**Blocker yang ditemukan dan ditutup pada task ini.** Roadmap REV-18 Backend sendiri mencatat Prasyarat Eksekusi #2: *"Migration Billing `AddEmployeeBenefitColumnsToBilArHandoff`: 🟡 Menahan eksekusi runtime `BE-FIN-091`."* Diverifikasi ke source: `BilArHandoff.cs` belum punya kolom `BenefitOwnerId`/`BenefitRelationship` maupun nilai `DebtorType = EMPLOYEE_BENEFIT`, dan `docs/module-blueprints/billing-kasir/` (module Billing) tidak punya task/roadmap entry apa pun untuk perluasan ini — meskipun `FIN-DEC-184` sudah disetujui "Pemilik Billing" sejak 6 Oktober 2026. Pengguna dikonfirmasi lewat pertanyaan eksplisit pada sesi ini dan memilih **memperluas scope task ini** untuk menutup gap administratif tersebut dengan syarat ketat:

- **Yang dikerjakan di sisi Billing:** model (`BilArHandoff.cs`), configuration (`BilArHandoffConfiguration.cs`), migration (file, **bukan** eksekusi).
- **Yang TIDAK dikerjakan di sisi Billing:** logika bisnis `BillingArApHandoffService` yang **menerbitkan** 2 baris serah terima saat finalisasi invoice (kalkulasi porsi pegawai vs RS Benefit). Ini di luar wewenang yang diberikan pengguna pada sesi ini, tetap milik module Billing, dan menuntut task/otorisasi terpisah.

Konsekuensi langsung: `M.1.1` ("Tagihan manfaat karyawan menghasilkan 2 baris serah terima") **tidak dapat dibuktikan end-to-end** oleh task ini — lihat bagian 6.

---

## 1. Proses bisnis

**Tujuan.** Billing menyerahterimakan tagihan layanan RS atas pegawai/keluarga ke Finance dalam dua baris terpisah per invoice: satu baris `PAYER` (porsi ditanggung RS, penjamin "RS Benefit") dan satu baris `EMPLOYEE_BENEFIT` (porsi kelebihan di atas plafon, ditanggung pegawai). Finance mengonsumsi kedua baris menjadi kartu piutang (`FinReceivable`) tanpa pernah menghitung ulang nominal atau plafon — murni menyalin apa adanya (`FIN-DEC-185`).

**Pelaku.** Petugas Finance (memicu `POST /{id}/process` pada layar fakta masuk Billing) atau proses ulang baris `ERROR`.

**Langkah (sebelum task ini, sudah berjalan untuk `PAYER`/`PATIENT_GUARANTOR`):**
1. `SyncNewFactsAsync` membaca `BilArHandoff` berstatus `CREATED` dan membuat baris `FinBillingHandoffIntake` berstatus `NEW`.
2. Petugas memicu `ProcessAsync` → `ProcessArIntakeAsync` untuk satu baris intake.
3. Service mengambil `BilArHandoff` sumber, memastikan belum pernah diproses (`IX_FinReceivable_SourceHandoffKey`).
4. Service membuat `FinReceivable` + 1 `FinReceivableItem`, mutasi subledger PENGAKUAN, kejadian akuntansi PENGAKUAN-PIUTANG — seluruhnya dalam satu transaksi `Serializable`.
5. `BilArHandoff.Status` diubah ke `ACKNOWLEDGED` (ACK balik ke Billing), baris intake ditandai `ACKNOWLEDGED`.

**Langkah baru pada task ini (baris 3.5, sisip di antara langkah 3 dan 4):**
- Bila `handoff.DebtorType == 'EMPLOYEE_BENEFIT'` dan `handoff.BenefitOwnerId` kosong → request ditolak dengan `BillingIntakeValidationException` → controller memetakannya ke **`422`** (pola existing, tidak diubah).
- Bila lolos, `FinReceivable.BenefitOwnerId` dan `BenefitRelationship` disalin apa adanya dari `handoff` (berlaku untuk seluruh `DebtorType` — `NULL` untuk `PAYER`/`PATIENT_GUARANTOR`, ditegakkan constraint database di kedua ujung).

**Jalur tidak normal:**
- `EMPLOYEE_BENEFIT` tanpa `BenefitOwnerId` → `422` di layer service, dan **tidak akan pernah tersimpan** di database karena `CK_BilArHandoff_BenefitOwner` (sumber) dan `CK_FinReceivable_BenefitOwner` (tujuan, sudah ada sejak `BE-FIN-079`) menolak kombinasi itu sebagai hard constraint.
- Baris intake yang gagal diproses karena sebab lain tetap mengikuti jalur existing: `MarkErrorAsync` menandai `ERROR`, bisa diulang.

**Hasil akhir.** Piutang `EMPLOYEE_BENEFIT` terbit dengan `OutstandingAmount = Amount` (dari handoff) dan status `OUTSTANDING` — sama persis dengan jalur `PAYER`/`PATIENT_GUARANTOR` existing, hanya menambah dua field sensitif yang disalin.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/00-interview-decisions.md` (`FIN-DEC-183`..`189`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (bagian P, baris 6251, 6253, 6311)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (P.1 `INT-FIN-BIL-001`)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-247`)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` (`M.1.1`..`M.1.5`)
- `docs/module-blueprints/finance-management/data/data-dictionary.md` (baris 221-222)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (prefix `Bil`, `Fin`)
- `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`
- `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilArHandoffConfiguration.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` dan `FinReceivableConfiguration.cs` (sudah siap sejak `BE-FIN-079`, tidak diubah)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (1369 baris, dibaca penuh)
- `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` (pemetaan `BillingIntakeValidationException` → `422`)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs` | Tambah `Guid? BenefitOwnerId`, `string? BenefitRelationship` (MaxLength 50), dan konstanta `BillingArDebtorTypes.EmployeeBenefit = "EMPLOYEE_BENEFIT"` |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilArHandoffConfiguration.cs` | Perluas `CK_BilArHandoff_DebtorType` menambah `'EMPLOYEE_BENEFIT'`; tambah `entity.Property(BenefitRelationship).HasMaxLength(50)`; tambah check constraint baru `CK_BilArHandoff_BenefitOwner` (pola sama persis `CK_FinReceivable_BenefitOwner`) |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | `ProcessArIntakeAsync`: tambah validasi 422 (`FIN-VAL-247`) sebelum insert; salin `BenefitOwnerId`/`BenefitRelationship` ke `FinReceivable` baru; perbarui komentar yang sudah usang (sebelumnya menyatakan EMPLOYEE_BENEFIT "OPEN DECISION, tidak pernah dihasilkan di sini") |
| `Migrations/20261010022402_AddEmployeeBenefitColumnsToBilArHandoff.cs` + `.Designer.cs` | Migration baru, dihasilkan `dotnet ef migrations add` (bukan ditulis tangan) — 2 kolom nullable + 2 check constraint |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model diperbarui otomatis oleh EF tooling, diverifikasi lewat `git diff`: hanya menyentuh bagian `BilArHandoff`, tidak ada drift lain |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint baru/berubah. Endpoint existing `POST /api/v1/.../billing-intakes/{id}/process` sudah mencakup jalur ini; metadata `[AccessAction]`/`[AccessPermission]` tidak disentuh |
| Database | **Migration dibuat, BELUM diterapkan.** 2 kolom nullable (`BenefitOwnerId` uuid, `BenefitRelationship` varchar(50)) pada tabel `BilArHandoff`; 1 check constraint diperluas (`CK_BilArHandoff_DebtorType`), 1 check constraint baru (`CK_BilArHandoff_BenefitOwner`). Nullable, tanpa downtime, nol backfill data lama (konsisten `FIN-DEC-184`/`186`) |
| Keamanan/Auth | `NOT APPLICABLE` untuk authorization (tidak ada perubahan `[Authorize]`/role). **Data sensitif:** `BenefitOwnerId`/`BenefitRelationship` adalah data pegawai — sudah ditandai `<summary>Sensitif</summary>` di `FinReceivable.cs` sejak `BE-FIN-079`; komentar yang sama ditambahkan pada `BilArHandoff.cs`. Tidak ada log yang mencetak nilainya |

---

## 3. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint. Endpoint existing yang menjalankan jalur baru ini (`FinanceBillingIntake` — `Process`) tidak berubah kontraknya.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet ef migrations add AddEmployeeBenefitColumnsToBilArHandoff` | `Build succeeded.` → `Done.` | `PASS` | Output command tersimpan pada sesi; file migration + Designer + snapshot update ada di `git status --short` |
| `git diff Migrations/ApplicationDbContextModelSnapshot.cs` | Delta **hanya** pada entity `BilArHandoff` (2 property, 1 constraint diperluas, 1 constraint baru) + 1 baris BOM kosmetik | `PASS` | Diperiksa manual baris per baris, dikutip pada bagian 0 |
| `dotnet build` (project aplikasi) | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna pada awal task ("Tanpa build automatis") dan dipertegas ulang pertengahan sesi ("jangan jalankan build migration, saya akan jalankan sendiri") — proses `dotnet ef migrations has-pending-model-changes` yang sedang berjalan untuk verifikasi tambahan dihentikan paksa atas instruksi ini |
| `dotnet ef database update` | **TIDAK DIJALANKAN** | `NOT RUN` | Di luar wewenang task — eksekusi database adalah hak eksklusif pemilik repository (`AGENTS.md` bagian *Wewenang migration dan eksekusi*), dan pengguna eksplisit akan menjalankannya sendiri |

**Tidak dijalankan:** `dotnet ef migrations has-pending-model-changes` (verifikasi tambahan yang saya inisiasi sendiri untuk double-check) — dihentikan di tengah jalan atas instruksi eksplisit pengguna, bukan karena gagal. Kecukupan migration sudah diverifikasi lewat pembacaan manual diff snapshot (baris di atas), bukan lewat command ini.

Uji manual: **`NOT FEASIBLE`** — memerlukan database dengan migration ter-apply dan baris `BilArHandoff` bertipe `EMPLOYEE_BENEFIT` sungguhan untuk dikonsumsi; keduanya tidak tersedia pada sesi ini (migration sengaja belum diterapkan, dan producer baris `EMPLOYEE_BENEFIT` — `BillingArApHandoffService` — belum dibangun, lihat bagian 0).

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.1.1` — Tagihan manfaat karyawan menghasilkan 2 baris serah terima saat finalisasi invoice | **Belum terpenuhi — di luar scope task ini.** Ini tanggung jawab `BillingArApHandoffService` (emisi, sisi Billing), yang tidak diberi wewenang pada sesi ini (lihat bagian 0). Skema penampungnya (unique index `(InvoiceId, DebtorType)`, kolom baru) sudah siap menampung 2 baris itu | `BilArHandoffConfiguration.cs` baris 37 (index existing, tidak diubah) |
| `M.1.2` — Baris `EMPLOYEE_BENEFIT` memuat `BenefitOwnerId`/`BenefitRelationship` valid ke `MstEmployee` | **Belum terpenuhi — di luar scope task ini.** Validasi ke `MstEmployee` adalah tanggung jawab Billing saat emisi (`FIN-DEC-199`), bukan Finance saat konsumsi (`FIN-DEC-185`) | — |
| `M.1.3` — Nominal piutang pegawai = kelebihan di atas plafon, disalin apa adanya | **Terpenuhi struktural.** `OriginalAmount = handoff.Amount` sudah type-agnostic sejak awal (tidak diubah task ini); kini berlaku juga untuk `EMPLOYEE_BENEFIT` | `FinanceBillingIntakeService.cs` baris ~460-462 (tidak diubah, sudah benar) |
| `M.1.4` — Serah terima `EMPLOYEE_BENEFIT` tanpa `BenefitOwnerId` ditolak `422` + check constraint keras | **Terpenuhi.** Validasi service eksplisit + `CK_BilArHandoff_BenefitOwner` (baru) + `CK_FinReceivable_BenefitOwner` (existing) sebagai backstop ganda | Diff bagian 2.2; migration bagian 2.2 |
| `M.1.5` — Konsumsi serah terima menerbitkan kartu piutang pegawai via `FinanceBillingIntakeService` (bukan `FinanceReceivableIntakeService` seperti ditulis roadmap — nama class tidak cocok source, diikuti source yang sebenarnya per `AGENTS.md`) | **Terpenuhi struktural, belum terverifikasi runtime.** `FinReceivable` dengan `DebtorType = EMPLOYEE_BENEFIT`, `OutstandingAmount = Amount`, `Status = OUTSTANDING` — jalur kode sama persis dengan `PAYER`, hanya field tambahan disalin | Diff bagian 2.2; verifikasi runtime `NOT FEASIBLE` (bagian 4) |

Catatan: roadmap task row kolom "Reuse" menyebut `FinanceReceivableIntakeService` dan `CK_FinReceivable_EmployeeBenefit_BenefitOwnerId` — keduanya **tidak cocok nama source sebenarnya** (`FinanceBillingIntakeService.ProcessArIntakeAsync` dan `CK_FinReceivable_BenefitOwner`). Diikuti source yang ada per `AGENTS.md` ("Ikuti kode yang sudah ada"), dicatat sebagai temuan drift dokumentasi, bukan diperbaiki pada blueprint (di luar scope implementasi).

**DoD roadmap** ("Build PASS, unit test PASS, laporan task tracked"): **belum terpenuhi penuh** — `dotnet build` sengaja tidak dijalankan atas instruksi eksplisit pengguna; repository ini tidak memelihara automated test project (`rules/backend/TEST_POLICY.md`) sehingga "unit test PASS" pada DoD lama sudah tidak berlaku.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `M.1.1` dan `M.1.2` tidak dapat dibuktikan oleh task ini — keduanya milik `BillingArApHandoffService` (emisi, Billing), bukan konsumsi (Finance). Task terpisah dengan otorisasi eksplisit dibutuhkan untuk menutupnya |
| Masalah yang diketahui | (1) `dotnet build` belum pernah dijalankan atas kode ini — risiko error kompilasi belum sepenuhnya nol meski `dotnet ef migrations add` sudah membuktikan seluruh solution (termasuk file yang diubah) berhasil compile pada saat itu. (2) `BenefitRelationship` pada `FinReceivable` (tujuan salin) ber-MaxLength 30, sedangkan `BilArHandoff` (sumber, baru) dan keputusan `FIN-DEC-184` menetapkan 50 — pre-existing dari `BE-FIN-079`, tidak disentuh (di luar scope), secara praktis aman karena nilai yang dipakai (`SELF`/`SPOUSE`/`CHILD`/`PARENT`/`OTHER`) jauh di bawah 30 karakter |
| Risiko tersisa | Migration belum diterapkan — jalur `EMPLOYEE_BENEFIT` akan gagal di runtime (kolom tidak ada di database fisik) sampai pemilik menjalankan `dotnet ef database update`. Jalur `PAYER`/`PATIENT_GUARANTOR` existing **tidak terdampak** sama sekali oleh perubahan ini (backward compatible, kolom baru nullable) |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna menghentikan verifikasi tambahan (`dotnet ef migrations has-pending-model-changes`) di tengah jalan dan meminta hanya membaca file migration/snapshot yang sudah ada. Dipatuhi — proses dihentikan, tidak ada command build/EF lain dijalankan sesudahnya |
| Status Git | `git status --short`: `M Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs`, `M Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs`, `M Repositories/Configurations/HealthServices/BillingManagement/Billing/BilArHandoffConfiguration.cs`, `?? Migrations/20261010022402_AddEmployeeBenefitColumnsToBilArHandoff.Designer.cs`, `?? Migrations/20261010022402_AddEmployeeBenefitColumnsToBilArHandoff.cs` |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build` lalu `dotnet ef database update` secara manual. (2) Otorisasi terpisah untuk `BillingArApHandoffService` (emisi 2 baris saat finalisasi invoice) agar `M.1.1`/`M.1.2` dapat ditutup dan jalur `EMPLOYEE_BENEFIT` dapat diuji end-to-end. (3) Setelah build dikonfirmasi PASS oleh pemilik, perbarui baris status roadmap `BE-FIN-091` dari 🟡 ke status yang sesuai |
