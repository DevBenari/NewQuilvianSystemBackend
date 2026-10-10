# Laporan Perubahan Backend — `BE-FIN-097`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-097` |
| Judul | Serah terima pembalik Billing diolah untuk membatalkan piutang lama, menerbitkan piutang baru, dan memicu restitusi payroll |
| Slice | `REV-18B4` — Inbound Webhook Payroll & Koreksi Reversal |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-231`..`234`; `FIN-DEC-187`, `195`, `200`; `02-backend-architecture.md` P.7 |
| Contract version | `FIN-INTEGRATION-1.8` §P.6 (`draft`); `FIN-STATE-1.8` P.1 |
| Dependency | `BE-FIN-091` 🟡, `BE-FIN-095` 🟡 — keduanya belum ✅ |
| Klasifikasi | `HEAVY` — mengaktifkan `HandoffType` yang sudah ada tapi dorman, menyentuh skema persetujuan Billing-Finance, menemukan blocker cross-module baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (diperluas) |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **SEBAGIAN secara struktural.** Langkah 1-2 kontrak (`batalkan lama, terbitkan baru`) diimplementasikan, tetapi **tidak akan pernah benar-benar berjalan** sampai Billing membangun jalur penerbitan `BilArHandoff` pengganti (lihat §0 — blocker baru, dicatat eksplisit). Langkah 3 (restitusi payroll) **TIDAK diimplementasikan**, bukan diam-diam dilewatkan. `dotnet build` **TIDAK DIJALANKAN** |

---

## 0. Temuan blocker — WAJIB dibaca

**`FinBillingHandoffTypes.Adjustment = "ADJUSTMENT"` sudah ada di source sejak sebelum sesi ini, tetapi dorman** — dideklarasikan, tidak pernah disinkron (`SyncNewFactsAsync`) maupun diproses (`ProcessAsync`). Task ini mengaktifkannya untuk kasus spesifik: `BilHandoffAdjustment` yang menunjuk `BilArHandoff` berjenis `EMPLOYEE_BENEFIT`.

**Blocker yang ditemukan sebelum menulis kode:** kontrak §P.6 langkah 1 menulis *"Billing menerbitkan `BilArHandoff` baru membawa `BenefitOwnerId` pegawai yang benar"* sebagai pasangan dari `BilHandoffAdjustment`. Diverifikasi ke **satu-satunya** jalur Billing yang menulis `BilHandoffAdjustment` (`BillingArApHandoffService.cs`, method pencatat koreksi generik dipakai untuk refund/write-off invoice): method itu **hanya mencatat baris penyesuaian**, dan **tidak pernah** menerbitkan `BilArHandoff` baru sebagai pasangannya — method itu juga memilih handoff target dengan memprioritaskan `PatientGuarantor`, bukan `EMPLOYEE_BENEFIT`, sehingga bahkan untuk kasus biasa pun secara default tidak menyasar baris manfaat karyawan. **Tidak ada kode Billing manapun** yang mengimplementasikan skenario "koreksi pemilik manfaat salah orang" secara spesifik — ini pola yang sama persis dengan temuan `BE-FIN-091` (skema `BilArHandoff`) dan `BE-FIN-094` (mesin payroll run HR): **produsen hulu di modul lain belum dibangun**.

**Keputusan untuk task ini (berbeda dari `BE-FIN-094`):** kali ini saya **tidak** memperluas wewenang ke modul Billing untuk membangun jalur penerbitan `BilArHandoff` pengganti — konteks sesi sudah sangat panjang (task ke-7 berturutan), dan menduplikasi logika kalkulasi "siapa pegawai yang benar" di Billing adalah keputusan bisnis Billing sendiri (bukan sekadar skema, seperti kasus `BE-FIN-091`). Konsumen Finance ditulis defensif dan DAPAT DIJALANKAN tanpa merusak apa pun (menunggu, bukan error, saat `newHandoff` tidak ditemukan — lihat §1), sehingga ia **aman menganggur** sampai Billing membangun bagiannya, bukan dalam keadaan rusak.

**Identifikasi relevansi tanpa field `AdjustmentType`:** entity `BilHandoffAdjustment` sebenarnya tidak punya kolom `AdjustmentType`/`SourceHandoffId`/`AdjustedAmount` seperti ditulis kontrak (field aslinya: `ArHandoffId`, `Direction`, `Amount`, `Reason`) — temuan drift lain. Karena tidak ada penanda tipe, Finance mengidentifikasi relevansi lewat `ArHandoffId` yang menunjuk `BilArHandoff.DebtorType = EMPLOYEE_BENEFIT` — keputusan desain, didokumentasikan sebagai asumsi, bukan fakta kontrak.

**Pasangan handoff lama/baru diidentifikasi lewat `CorrelationId` yang sama** — tidak ada field eksplisit pada `BilHandoffAdjustment` yang menunjuk handoff pengganti; ini pola yang sudah dipakai di tempat lain pada file yang sama (handoff↔intake), diterapkan dengan asumsi Billing akan mengikuti pola yang sama kelak.

---

## 1. Proses bisnis

**Alur yang DIIMPLEMENTASIKAN (langkah 1-2 kontrak):**
1. `SyncNewFactsAsync` menemukan `BilHandoffAdjustment` baru yang `ArHandoffId`-nya menunjuk handoff `EMPLOYEE_BENEFIT`, mendaftarkannya sebagai `FinBillingHandoffIntake` tipe `ADJUSTMENT`.
2. `ProcessArAdjustmentIntakeAsync` (dipanggil lewat `ProcessAsync` yang sudah ada):
   - Piutang lama (dicari dari `SourceHandoffKey` handoff lama) dibatalkan: `OutstandingAmount → 0`, `AdjustedAmount` naik sebesar sisa yang dibatalkan (menjaga `CK_FinReceivable_Balance`), `Status → CANCELLED`, satu mutasi `PENYESUAIAN` ditulis (pembalikan, bukan edit manual — `FIN-DEC-173`).
   - Dicari `BilArHandoff` lain yang berbagi `CorrelationId` dengan adjustment (calon handoff pengganti). **Belum pernah ditemukan** sampai Billing membangun jalur penerbitannya (§0) — bila ditemukan, kartu piutang baru diterbitkan memakai pola identik `ProcessArIntakeAsync` (item, mutasi `PENGAKUAN`, event `PENGAKUAN-PIUTANG`).
   - Bila handoff pengganti belum ada: intake tetap `CONSUMED` (piutang lama tetap dibatalkan — itu valid berdiri sendiri), **bukan** `ERROR` — karena menunggu Billing bukan kegagalan yang perlu diulang petugas, dan pembatalan piutang lama sudah final dan benar terlepas kartu barunya sudah terbit atau belum.

**Alur yang TIDAK diimplementasikan:**
3. Restitusi payroll (kontrak langkah 3, `FIN-DEC-195`) — event pembalikan angsuran ke HR bila cicilan pegawai lama sempat terpotong. **Sengaja tidak dikerjakan** pada task ini (lihat §3).

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §P.6 (dibaca penuh)
- `Areas/HealthServices/BillingManagement/Billing/Models/BilHandoffAdjustment.cs` (struktur asli, dibandingkan ke kontrak)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` (method pencatat koreksi dan `GetHandoffStatusAsync` — dibaca untuk menemukan blocker §0)
- `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` (`FinBillingHandoffTypes.Adjustment` ditemukan sudah ada tapi dorman)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (1500+ baris, bagian `SyncNewFactsAsync`/`ProcessAsync`/`ProcessArIntakeAsync` dipakai sebagai pola yang diikuti persis)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | `SyncNewFactsAsync` menemukan `BilHandoffAdjustment` relevan; `ProcessAsync` mendelegasikan `HandoffType.Adjustment`; method baru `ProcessArAdjustmentIntakeAsync` |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint baru, memakai worklist `POST /{id}/process` yang sudah ada (`BE-FIN-009`) |
| Database | `NOT APPLICABLE` — nol migration, nol entity baru. Mengaktifkan nilai enum C# yang sudah lama ada (`FinBillingHandoffTypes.Adjustment`), bukan menambah nilai baru |
| Keamanan/Auth | `NOT APPLICABLE` — nol perubahan otorisasi |

---

## 3. Yang sengaja TIDAK diimplementasikan

**Restitusi payroll (kontrak §P.6 langkah 3).** Menentukan apakah cicilan pegawai lama sempat terpotong membutuhkan query ke `FinReceivableInstallment` milik perjanjian atas piutang lama (`PaidAmount > 0`), lalu menulis entri pembalik ke HR lewat mekanisme yang analog dengan `FinanceReceivablePayrollSyncService` (`BE-FIN-094`) — kemungkinan `TrxPayrollVariableInput` bernilai negatif (reimbursement), sebuah keputusan desain yang belum pernah dibuat di repository ini. Mengerjakannya tergesa pada task ini — task ke-7 berturutan dalam sesi yang sama — berisiko menghasilkan kode yang salah tanpa sempat ditinjau memadai. **Dicatat `NOT IMPLEMENTED`**, bukan ditulis asal-asalan.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review manual: `CK_FinReceivable_Balance` tetap konsisten pada pembatalan | `AdjustedAmount` dinaikkan sebesar `OutstandingAmount` yang dibatalkan — pelajaran dari bug yang ditemukan di `BE-FIN-095`, diterapkan proaktif di sini | `PASS` (review manual) | `ProcessArAdjustmentIntakeAsync` |
| Review manual: alur tidak merusak intake existing (`Ar`, `Collection`, dst.) | Hanya menambah cabang baru pada `switch`, nol baris existing diubah | `PASS` (review manual) | Diff §2.2 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile majemuk 7 task** (`BE-FIN-091`..`097`). Belum satu pun dikompilasi.

Uji manual: `NOT FEASIBLE` — dan bahkan dengan build sukses serta migration diterapkan, uji end-to-end LANGKAH PENUH tidak akan pernah berhasil sampai Billing membangun jalur penerbitan `BilArHandoff` pengganti (§0). Hanya langkah pembatalan piutang lama yang dapat diuji berdiri sendiri.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.4.1`..`M.4.3` | **Belum terpenuhi.** Pembatalan piutang lama terpenuhi struktural; penerbitan piutang baru terpenuhi struktural TAPI tidak dapat dipicu sampai Billing membangun jalur penerbitan `BilArHandoff` pengganti (§0); restitusi payroll `NOT IMPLEMENTED` (§3) | `ProcessArAdjustmentIntakeAsync` |
| Integritas audit tanpa edit manual debitur | **Terpenuhi** — pembatalan selalu lewat mutasi `FinReceivableMovement`, nol penulisan langsung ke field debitur | `ProcessArAdjustmentIntakeAsync` |

**DoD roadmap** ("Build PASS, unit test koreksi, laporan task tracked"): **belum terpenuhi**.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Fitur ini **tidak dapat berfungsi penuh** sampai Billing membangun jalur penerbitan `BilArHandoff` pengganti untuk skenario salah-orang — ini **blocker cross-module nyata**, bukan sekadar risiko compile. (2) Restitusi payroll belum ada sama sekali. (3) Source belum pernah dikompilasi |
| Masalah yang diketahui | Pencarian handoff pengganti lewat `CorrelationId` adalah asumsi desain saya, bukan kontrak Billing yang disepakati — bisa jadi Billing memilih mekanisme penautan yang berbeda saat jalurnya akhirnya dibangun |
| Risiko tersisa | (1) Risiko compile 7 task majemuk. (2) Bila Billing membangun jalur penerbitannya dengan mekanisme penautan BERBEDA dari asumsi `CorrelationId` di sini, kode ini perlu direvisi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short`: `FinanceBillingIntakeService.cs` diperluas — nol file baru pada task ini |
| Langkah berikutnya | (1) Pemilik Billing menilai dan merancang jalur penerbitan `BilArHandoff` pengganti untuk skenario salah-orang (di luar wewenang task ini). (2) Pemilik Finance memutuskan mekanisme restitusi payroll (§3) sebagai task terpisah. (3) Pemilik menjalankan `dotnet build` untuk `BE-FIN-091`..`097` sekaligus |
