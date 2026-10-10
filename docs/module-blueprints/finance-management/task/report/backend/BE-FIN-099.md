# Laporan Perubahan Backend — `BE-FIN-099`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-099` |
| Judul | Layanan penutupan berkala porsi benefit rumah sakit (hitung awal, draf, terbitkan atomic, batalkan) beroperasi penuh |
| Slice | `REV-18B2` — Perjanjian Cicilan, Clearance, Pelunasan & Migrasi |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-211`..`217`; `FIN-DEC-177`, `178`; `FIN-DES-102`, `103` |
| Contract version | `FIN-API-1.9` O.3 (`draft`); `FIN-VAL-1.10` `FIN-VAL-241`..`243` (`approved`); `FIN-STATE-1.8` O.3 |
| Dependency | `BE-FIN-091` 🟡, `BE-FIN-092` 🟡 — keduanya belum ✅ |
| Klasifikasi | `HEAVY` — 1 service baru (6 method publik, transaksi atomik, penguncian), 1 controller baru (5 endpoint) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,DTOs,Services}/`, `Program.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis.** `dotnet build` **TIDAK DIJALANKAN**. Task ke-9 berturutan tanpa build |

---

## 0. Backend Governance Preflight

**Area/Module:** `Corporate/FinanceManagement/Receivable`, prefix `Fin`, `ACTIVE`. **Applicability:** `NEW CODE` penuh. **QBE ID:** QBE-SVC-001, QBE-TXN-001 (transaksi atomik pada `PostAsync`/`CancelAsync`), QBE-PERM-001 (`preview` memakai hak `Read`, bukan `Create` — `L.5.13`).

**Penegak invarian `FIN-VAL-243` yang sesungguhnya.** `BE-FIN-092` §3 sudah mencatat: unique-terfilter lintas tabel pada `FinBenefitSettlementItem.ReceivableId` tidak dapat dibuat sebagai DB constraint Postgres (filter merujuk `Status` tabel induk). Task ini adalah **satu-satunya penegak** invarian itu — lewat `GetEligibleItemsAsync` yang secara eksplisit mengeluarkan piutang yang sudah dimiliki baris item pada pelunasan manapun yang **belum** dibatalkan, dipanggil ulang baik saat `CreateAsync` (snapshot) maupun `PostAsync` (validasi ulang sebelum atomic-close, `L.5.5`).

---

## 1. Proses bisnis

**Eligibility piutang** (kriteria yang saya tetapkan — kontrak tidak merincinya persis): `DebtorType = PAYER`, `DebtorReferenceId` = penjamin dipilih, `Status ∈ {OUTSTANDING, PARTIAL}`, `RecognizedAt` jatuh pada bulan `AccountingPeriodCode`, dan **belum** dimiliki item pelunasan lain yang tidak dibatalkan.

1. **Preview** (`POST /benefit-settlements/preview`) — murni baca, hak `Read` saja (`L.5.13`). Dipanggil 3× atas periode sama menghasilkan angka identik, nol perubahan (`L.5.1`). Periode kosong → `200` nol kartu, bukan `404` (`L.5.2`).
2. **Create** (`POST /benefit-settlements`) — snapshot piutang layak saat itu menjadi `FinBenefitSettlement` (`DRAF`) + N `FinBenefitSettlementItem`. Saldo piutang **belum** bergerak (`L.5.3`). Kosong → `422` (`FIN-VAL-242`). Periode+penjamin sudah ada pelunasan aktif → `409` (`FIN-VAL-241`, lapis service + unique index `BE-FIN-092`).
3. **Post** (`POST /{id}/post`) — validasi ULANG setiap item (masih `OUTSTANDING`/`PARTIAL`, belum diambil pelunasan lain) **sebelum** menyentuh satu pun baris. Satu saja tidak sah → `409`, **nol** kartu tertutup (`L.5.5`, atomicity sejati — bukan "tutup yang valid, lewati yang tidak"). Lolos semua → setiap piutang `OutstandingAmount → 0`, `Status → SETTLED`, satu mutasi `PELUNASAN-INTERNAL` per kartu, via `FinanceSubledgerMovementService` (`L.5.4`).
4. **Cancel** (`POST /{id}/cancel`) — dari `DRAF`: nol akibat saldo (`L.5.8`). Dari `DITERBITKAN`: setiap kartu dibuka kembali (`OutstandingAmount` pulih, `Status → OUTSTANDING/PARTIAL`), satu mutasi **pembalik** `PELUNASAN-INTERNAL` per kartu — baris mutasi asli **tetap ada**, tidak dihapus (`L.5.9`). Periode yang pelunasannya dibatalkan boleh ditutup ulang — unique index hanya mengikat yang tidak dibatalkan (`L.5.10`).
5. **Nol penghapusan buku** di jalur manapun — satu-satunya mekanisme penutupan adalah mutasi `PELUNASAN-INTERNAL` (`FIN-DEC-178`, `L.5.11`).
6. `AccountingEventId` sengaja tetap `null` selamanya pada task ini — kontraknya tertahan `FIN-OQ-103`, penutupan piutang tetap sah tanpanya (`L.5.12`).

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` O.3 (dibaca sebelumnya)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-241`..`243`
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` O.3, O.4
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `L.5.1`..`L.5.14`
- `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-092.md` §3 (temuan unique-terfilter lintas tabel — dasar desain `GetEligibleItemsAsync`)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableBillingDataService.cs` (pola join `MstPatients`/`BilInvoices` untuk `PatientName`/`InvoiceNumber`)
- `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` (`CompanyGuarantorName`)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceBenefitSettlementDtos.cs` | **Baru.** 10 class sesuai `FIN-API-1.9` O.3.1 |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceBenefitSettlementService.cs` | **Baru.** 6 method publik + infrastruktur transaksi/lock/mapping privat, 2 exception class |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceBenefitSettlementsController.cs` | **Baru.** 5 endpoint sesuai kontrak apa adanya |
| `Program.cs` | `AddScoped<FinanceBenefitSettlementService>()` |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 5 endpoint baru, persis `FIN-API-1.9` O.3 |
| Database | `NOT APPLICABLE` — nol migration, seluruh tabel sudah disediakan `BE-FIN-092` |
| Keamanan/Auth | Resource baru `FinanceBenefitSettlement` (`Read`, `Create`, `Post`, `Cancel`). `preview` sengaja memakai `Read`, bukan `Create` (`L.5.13`, dicocokkan argumen `[AccessPermission]` baris per baris terhadap `ControllerName`) |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review manual: `CK_FinReceivable_Balance` konsisten pada `PostAsync` (tutup) dan `CancelAsync` (buka kembali) | `AllocatedAmount` disesuaikan simetris pada kedua arah — pola yang sama diterapkan sejak `BE-FIN-095` | `PASS` (review manual) | `PostAsync`, `CancelAsync` |
| Review manual: bug sintaks `class X : Y;` (semicolon, bukan C# valid untuk badan kelas kosong) ditemukan dan diperbaiki sebelum laporan ditulis | `BenefitSettlementListResponse` diperbaiki jadi `{ }` | `PASS` (ditemukan sendiri sebelum build, bukan oleh compiler) | `FinanceBenefitSettlementDtos.cs` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile majemuk 9 task** (`BE-FIN-091`..`099`). Belum satu pun dikompilasi.

Uji manual: `NOT FEASIBLE`.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status |
| --- | --- |
| `L.5.1`, `L.5.2` Hitung awal non-mutasi, periode kosong `200` | **Terpenuhi struktural** |
| `L.5.3` Draf, saldo belum bergerak | **Terpenuhi struktural** |
| `L.5.4` Terbitkan menutup seluruhnya | **Terpenuhi struktural** |
| `L.5.5` Penerbitan atomic, satu tidak sah → nol tertutup | **Terpenuhi struktural** — validasi ulang penuh sebelum mutasi apa pun |
| `L.5.6` Pelunasan kedua ditolak unique index | **Terpenuhi struktural**, dua lapis |
| `L.5.7` Pelunasan kosong `422` | **Terpenuhi struktural**, dicek di `Create` dan `Post` |
| `L.5.8`, `L.5.9`, `L.5.10` Pembatalan draf/terbit, dibuka ulang | **Terpenuhi struktural** |
| `L.5.11` Nol penghapusan buku | **Terpenuhi by design** |
| `L.5.12` Penanda akuntansi kosong, tetap sah | **Terpenuhi by design** |
| `L.5.13`, `L.5.14` Hak akses preview vs post | **Terpenuhi struktural** |

**DoD roadmap**: **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan.

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Source belum pernah dikompilasi |
| Masalah yang diketahui | Kriteria eligibility piutang (`RecognizedAt` dalam bulan periode) adalah interpretasi saya — kontrak tidak merincinya secara eksplisit field mana yang menentukan "piutang milik periode X" |
| Risiko tersisa | Risiko compile 9 task majemuk |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 3 file baru (DTO, Service, Controller), `Program.cs` dimodifikasi |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` untuk `BE-FIN-091`..`099` sekaligus; konfirmasi field penentu "piutang milik periode akuntansi X" (`RecognizedAt` vs field lain) |
