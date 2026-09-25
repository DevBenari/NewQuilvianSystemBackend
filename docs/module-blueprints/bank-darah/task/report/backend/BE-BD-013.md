# Laporan Perubahan Backend — `BE-BD-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-013` |
| Judul | Penyaluran biaya tindakan Bank Darah ke Billing |
| Slice | Tindakan Bank Darah dan biayanya (`BD-SLICE-08`, bagian penyerahan biaya) |
| Roadmap | `roadmap/backend-roadmap.md` — bagian 3, 4, 5, 6, dan 7 |
| Trace | `DEC-BD-016` (disetujui), `DEC-BD-021`, `DEC-BD-034`, `INV-BD-024`, `BD-CAP-015`, `BD-DOM-19`; `integration-contract.md` §3; `state-transition-matrix.md` §5; `api-contract.md` grup Blood Bank Procedure |
| Contract version | Set kontrak `v4` `approved` (`Sukmagp`, 2026-09-03) dengan delta `DEC-BD-016` yang disetujui `Sukmagp` pada 2026-09-17 |
| Dependency | `BE-BD-012` ✅ (tindakan dan salinan tarif), `BE-BD-007` ✅ (pemberian kantong), `BE-BD-010` ✅ (koreksi dua tahap), `DEC-BD-016` ✅ disetujui |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Bank Darah, satu konstanta registri Billing, dan dokumen blueprint `docs/module-blueprints/bank-darah/**` yang disebut task |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `cb3076d1` cabang `sukmagp` (perubahan belum di-commit) |
| Tanggal | 17 September 2026 |
| Status | ✅ **SELESAI** — ketiga acceptance terbukti runtime, build `0 Error(s)`, nol perubahan model, QBE Strict `PASS` |

---

## 1. Masalah yang diperbaiki

Sampai hari ini tindakan Bank Darah hanya **dicatat dan diselesaikan**. Tidak ada satu rupiah pun yang
sampai ke Billing, karena pemilik belum menyetujui "konteks sumber" Bank Darah pada kontrak Billing
(`DEC-BD-016`). Akibatnya bagi rumah sakit: tindakan seperti uji silang serasi sudah dikerjakan dan
bertarif, tetapi kasir tidak pernah melihat tagihannya.

Pada 17 September 2026 pemilik, `Sukmagp`, menyetujui `DEC-BD-016`. Task ini menyambungkan penyelesaian
tindakan ke jalur Billing resmi yang sudah dipakai Laboratorium dan Radiologi — tanpa membuat mekanisme
Billing kedua.

**Contoh konkret.** Pasien menerima dua kantong darah dalam satu tindakan bertarif Rp121.000. Yang benar
adalah **satu** tagihan Rp121.000, bukan dua. Bila petugas menekan tombol kirim ulang karena jaringan
sempat putus, tagihan tetap satu. Bila kemudian salah satu pencatatan kantong dikoreksi, tagihan tindakan
tidak ikut dibatalkan oleh Bank Darah — peninjauannya milik Billing.

---

## 2. Proses bisnis

**Tujuan.** Setiap tindakan Bank Darah yang selesai menghasilkan tepat satu fakta biaya di Billing.

**Pelaku.** Petugas Bank Darah pemegang hak akses `BloodBankProcedure : Update`.

**Pemicu.** Tindakan berpindah dari `Recorded` ke `Completed`.

**Langkah normal, berurutan:**

1. Petugas menekan **Selesai** pada tindakan berstatus `Recorded`.
2. Backend memeriksa pelaku dari akun login dan status tindakan.
3. Status berpindah ke `Completed`; satu baris riwayat `Complete` ditulis. Keduanya **disimpan lebih dulu**.
4. Sesudah tersimpan — di luar transaksi apa pun — backend menyusun fakta biaya **dari data yang tersimpan**:
   order darah sebagai sumber, tindakan sebagai item, kunjungan dari order, waktu dari riwayat `Complete`,
   jumlah `1 Tindakan`, dan salinan tarif `BE-BD-012`.
5. Fakta diserahkan lewat `ClinicalMilestoneFactProducer` ke `BillingFolioService.RecognizeMilestoneAsync`.
6. Billing membuat satu charge line berstatus `PendingFinancialReview` pada folio kunjungan.
7. Jawaban `complete` membawa ringkasan `BillingHandoff` (`Emitted`).

**Aturan yang berlaku.**

| Aturan | Isi | Contoh |
| --- | --- | --- |
| Satuan biaya | Satu fakta per tindakan, bukan per kantong (`DEC-BD-021`) | 2 kantong → 1 fakta, `Quantity = 1` |
| Nominal | Salinan `TariffAmountSnapshot`; `MstTariff` tidak dibaca ulang | Tarif induk naik ke Rp300.000 → fakta tetap Rp121.000 |
| Waktu kejadian | Waktu penyelesaian tersimpan, bukan waktu kirim | Kirim ulang besok → `OccurredAt` tetap waktu selesai hari ini |
| Konteks sumber | Konstanta server `BloodBank` / `BloodBankCharge` | Body client yang menyebut `Laboratory` diabaikan |
| Kepemilikan finansial | Billing | Bank Darah tidak menandai tagihan dibayar, ditolak, atau dibatalkan |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Billing menolak, tak terjangkau, atau hasilnya tidak pasti | Tindakan **tetap** `Completed`; `complete` tetap `200` dengan pesan "penyerahan fakta biaya ke Billing memerlukan tinjauan" dan `BillingHandoff` berisi jenis hasilnya |
| Galat tak terduga di jalur penyerahan | Dilaporkan sebagai `OutcomeUnknown` dengan kode `BBK_COST_FACT_HANDOFF_UNCONFIRMED`, bukan `500`; perubahan klinis tidak tersentuh |
| Tindakan yang sudah `Completed` diselesaikan lagi | **Ditolak `422`** — perilaku `BE-BD-012` (`AC-BD-101`) dipertahankan |
| Fakta perlu dikirim ulang | `POST /{id}/resend-cost-fact` — tanpa perpindahan status, tanpa riwayat baru; Billing tidak membuat charge kedua |
| Kirim ulang atas tindakan yang belum selesai | **Ditolak `422`** |
| Kantong dikoreksi sesudah fakta terkirim | Koreksi berjalan seperti `BE-BD-010`; **tidak** ada fakta pembatalan, refund, void, maupun penyesuaian |

**Hasil akhir.** Satu tindakan selesai = satu fakta di ledger `CliClinicalMilestoneFact` = satu charge line
di Billing, berapa pun kantong dan berapa kali pun dikirim ulang.

### 2.1 Kenapa kirim ulang memakai endpoint terpisah, bukan `complete` kedua

Task mengizinkan dua bentuk jalur kirim ulang. Bentuk "panggil `complete` lagi" **ditolak** karena
bertabrakan dengan `AC-BD-101` jalur gagal milik `BE-BD-012`: *"Tindakan yang sudah `Completed` diselesaikan
lagi → **Ditolak** secara terkendali; status dan audit tidak bergerak"*. Mengubah jawaban itu menjadi `200`
akan membalik perilaku yang sudah terbukti. Karena itu kirim ulang menjadi aksi tersendiri,
`resend-cost-fact`, dengan pola `LabSpecimenService.AcceptAsync` — mengirim ulang fakta tanpa menyentuh
keadaan. Ini **pengiriman ulang idempotent**, bukan penyelesaian kedua: nol perpindahan status, nol baris
riwayat, nol salinan tarif baru.

### 2.2 Kenapa `Quantity`/`Unit` diisi

`BillingFolioService.RecognizeMilestoneAsync` hanya membuat `BilChargeComponent` — satu-satunya tempat
`TariffSnapshot` tersimpan di Billing — **bila `Quantity` terisi** (baris 350–368). Tanpa `Quantity`, salinan
tarif tidak pernah sampai ke Billing. Pola yang sama dipakai Laboratorium (`Quantity = 1`,
`Unit = "Pemeriksaan"`) dan Radiologi (`Quantity = 1`, `Unit = "Examination"`). Bank Darah memakai
`Quantity = 1`, `Unit = "Tindakan"` — menyebut tindakan, **bukan** jumlah kantong.

### 2.3 Kenapa `CausationId` tidak diisi

Producer Laboratorium, Radiologi, dan tindakan klinis tidak mengisinya, dan tidak ada identitas domain
yang lazim dipakai untuk itu. `CorrelationId` diisi `BloodOrderId`, mengikuti pola Laboratorium yang
mengisinya dengan id pesanan.

---

## 3. Perubahan yang dikerjakan

### 3.0 Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` (pemilik perubahan); satu konstanta pada `BillingManagement/Operational` |
| Registry | `BloodBankManagement / Blood Bank` / `Bbk` / `ACTIVE`; `BillingManagement / Billing` / `Bil` / `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` pada `BillingSourceContract.cs`; `NEW CODE` untuk method, DTO, dan endpoint baru di Bank Darah |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa DbContext, orkestrasi di service), `QBE-API-001` (`ApiResponse<T>`, status yang mapan), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]`), `QBE-LOG-001` (log aksi beserta aktor), `QBE-DTO-001` (DTO, bukan entity), `QBE-VAL-001` (status dan aktor divalidasi), `QBE-TXN-001` (klinis ter-commit sebelum Billing), `QBE-MOD-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` — nol entity, configuration, nomor bisnis, maupun migration |
| Governance terbaca | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` (identik dengan salinan suite), `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/*` suite 1.18.0 |
| Selisih wewenang | Aturan build skill membatasi tulis dokumen pada `task/report` dan baris status roadmap. Task ini **secara eksplisit** memerintahkan pencatatan `DEC-BD-016` dan pembaruan kontrak, sehingga wewenang task (presedensi 1) yang berlaku |

### 3.1 Berkas yang diperiksa

`AGENTS.md`; aturan `rules/backend/*` dan `rules/rule-output/*`; `BillingSourceContract.cs`;
`ClinicalMilestoneFactProducer.cs`; `ClinicalMilestoneFactDtos.cs`; `ClinicalMilestoneFactEnums.cs`;
`BillingFolioService.cs`; `RadStudyService.cs`; `LabSpecimenService.cs`; `LabOrderService.cs`
(`MapHandoff`); `LabSpecimenDtos.cs`; `PatientProcedureController.cs`; `ConsultationFinalizationService.cs`;
`PrescriptionController.cs`; `BbkBloodBankProcedureService.cs`; `BbkBloodBankProcedureController.cs`;
`BloodBankProcedureDtos.cs`; `BbkBloodBankProcedure.cs`; `BbkTransitionHistory.cs`; `BbkProcedureStatus.cs`;
`BbkBloodUnitService.cs` (jalur koreksi, pemberian, gerbang); `BbkBloodOrderService.cs`;
`BbkEncounterStatusReader.cs`; `AccessMenuSeeder.cs`; `RoleAccessController.cs`; `LoggerService.cs`;
`Program.cs`; `ApplicationDbContextModelSnapshot.cs`; seluruh dokumen blueprint Bank Darah yang menyebut
`DEC-BD-016`; laporan `BE-BD-007`, `009`, `010`, `012`, `016`; frontend `V2QuilvianSystemFrontendDev`
(baca saja — nol pemakai endpoint tindakan).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | Konstanta `BloodBankSourceContext = "BloodBank"` dan `BloodBankChargeEffectType = "BloodBankCharge"`, didaftarkan pada `AllowedEffectTypes`. Lima pemetaan lain tidak disentuh |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodBankProcedureService.cs` | Injeksi `ClinicalMilestoneFactProducer` dan `LoggerService`; `CompleteAsync` menyerahkan fakta **sesudah** `SaveChangesAsync`; method baru `ResendCostFactAsync`, `HandOffCostFactAsync`, `BuildCostFactRequestAsync`; `BloodBankProcedureResult` mendapat parameter opsional `BillingHandoff` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodBankProcedureController.cs` | `complete` membawa `BillingHandoff`; endpoint baru `POST /{id}/resend-cost-fact`; log aksi berisi id dan kode saja |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodBankProcedureDtos.cs` | Isian nullable `BillingHandoff` pada `BloodBankProcedureDetailDto`; kelas baru `BloodBankProcedureBillingHandoffDto` mengikuti `LabBillingHandoffResponse` |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkProcedureStatus.cs` | Komentar saja — pernyataan "tidak memicu apa pun di luar Bank Darah" sudah tidak benar |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodBankProcedure.cs` | Komentar saja — nol property, nol atribut; model EF tidak berubah |
| `docs/module-blueprints/bank-darah/00-interview-decisions.md` | Bagian 8.31 baru: `DEC-BD-016` `approved` `Sukmagp` 2026-09-17; baris register dan Open Question ditutup dengan riwayat; catatan gugurnya batas `AC-BD-102` |
| `docs/module-blueprints/bank-darah/contracts/integration-contract.md` | Bagian 3 menjadi kontrak berlaku: bentuk fakta, aturan urutan, idempotency, koreksi; keadaan lama sebagai riwayat |
| `docs/module-blueprints/bank-darah/contracts/api-contract.md` | Baris `complete` diperbarui, baris `resend-cost-fact` baru, delta `BillingHandoff` |
| `docs/module-blueprints/bank-darah/contracts/state-transition-matrix.md` | §5: efek sesudah `Completed` dan aksi kirim ulang tanpa perpindahan status |
| `docs/module-blueprints/bank-darah/04-prd-to-mvp.md` | Pernyataan "tanpa charge Billing"/`OPEN DECISION` diberi status baru beserta riwayat |
| `docs/module-blueprints/bank-darah/02-backend-architecture.md` | Tujuh pernyataan "tertahan `DEC-BD-016`" diberi status baru beserta riwayat |
| `docs/module-blueprints/bank-darah/data/data-dictionary.md` | Satu baris `BbkBloodBankProcedure` |
| `docs/module-blueprints/bank-darah/testing/acceptance-test-matrix.md` | `AC-BD-026/027/058` ditandai terbukti; penanda tertunda dipertahankan sebagai riwayat |
| `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` | Tanda status dan bukti `BE-BD-013` |
| `docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md` | Baris requirement dan acceptance `BE-BD-013` |
| `docs/module-blueprints/bank-darah/MODULE-STATUS.md` | Keadaan current, hitungan backend, fase, blocker |
| `docs/module-blueprints/bank-darah/task/report/backend/BE-BD-013.md` | **Baru** — laporan ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu endpoint baru `POST /{id}/resend-cost-fact`; satu isian nullable `BillingHandoff` pada `BloodBankProcedureDetailDto` (terisi hanya pada `complete` dan `resend-cost-fact`, `null` pada `GET`). Nol endpoint, isian, atau kode status yang dihapus maupun diganti nama. `AvailableActions` tidak berubah. Frontend tidak punya pemakai endpoint tindakan (hanya berkas hasil build `.next`) |
| Database | **Migration: TIDAK.** Nol entity, kolom, index, maupun configuration. `has-pending-model-changes`: *"No changes have been made to the model since the last migration."* Penulisan runtime hanya ke tabel yang sudah ada: `CliClinicalMilestoneFact` (ledger producer), `BilFolio`, `BilChargeLine`, `BilChargeComponent`, `BilProcessingEffect` (milik Billing). Tujuh migration modul lain yang tertunda **tidak** diterapkan, disunting, maupun dibuat ulang |
| Keamanan/Auth | Pelaku dari klaim akun login. `SourceContext`, `EffectType`, `EncounterId`, identitas fakta, dan nominal seluruhnya diturunkan server. Kedua endpoint tanpa body. `resend-cost-fact` memakai `BloodBankProcedure : Update` yang sama dengan `complete` — butir hak akses tidak bertambah, dan teks `[AccessAction]`-nya sengaja sama persis supaya baris layar Akses Role tidak berubah. Registri Billing tetap fail-closed (perbandingan ordinal) |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Bank Procedure

Base URL: `api/v1/health-services/blood-bank-management/blood-bank-procedures`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/complete` | Menyatakan tindakan selesai; sesudah tersimpan, menyerahkan satu fakta biaya ke Billing dan melaporkan hasilnya pada `BillingHandoff` | `BloodBankProcedure : Update` |
| `POST` | `/{id}/resend-cost-fact` | **Baru.** Mengirim ulang fakta biaya tindakan yang sudah selesai tanpa perpindahan status; aman diulang | `BloodBankProcedure : Update` |

Jawaban `resend-cost-fact`:

| HTTP | Kapan |
| --- | --- |
| `200` | `BillingHandoff.Kind` `Emitted` atau `Replayed` |
| `409` | `OutcomeUnknown` atau `ReconciliationRequired` — ringkasan pada `errors` |
| `422` | `RejectedByBilling`, `Invalid`, atau tindakan belum `Completed` |
| `404` | Tindakan tidak ada atau dihapus |
| `403` | Tanpa `BloodBankProcedure : Update` |

Bentuk `BillingHandoff`: `Kind`, `IsClinicallySafe`, `MilestoneFactId`, `MilestoneFactVersion`,
`DispatchStatus`, `Code`, `Message`. Keterangan proses, **bukan** status pembayaran.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `git diff --check` | Bersih | `PASS` | Exit `0` |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -nodeReuse:false -p:BuildInParallel=false -p:UseSharedCompilation=false` (pertama) | `0 Error(s)`, `217 Warning(s)`; **3** warning `CS1573` dari tag `<param>` baru pada record `BloodBankProcedureResult` | `NEW ERROR` (warning) — diperbaiki | Tag dipindah ke `<remarks>` |
| Build yang sama sesudah perbaikan | **`Build succeeded`, `0 Error(s)`, `214 Warning(s)`**, `01:01:01` | `PASS` | **Nol** warning dari berkas `BE-BD-013`. 214 seluruhnya di berkas modul lain; pembanding laporan `BE-BD-010` adalah 198 pada HEAD lebih lama, sebelum merge Integration `cb3076d1` |
| `dotnet ef migrations has-pending-model-changes --context ApplicationDbContext --no-build` | *No changes have been made to the model since the last migration.* | `PASS` | Exit `0` |
| Verifikator registri (skrip `dotnet fsi` di luar repository, memuat DLL hasil build) | **14/14** | `PASS` | `IsKnownSourceContext("BloodBank")` `true`; `IsAllowedEffectType("BloodBank","BloodBankCharge")` `true`; `("BloodBank","LaboratoryCharge")`, `("Laboratory","BloodBankCharge")`, `("BloodBank","BloodBankCancellation")`, `("bloodbank")`, `("BloodBank", null)` seluruhnya `false`; kelima pemetaan lama tetap `true` |
| QBE Strict `WorkingTree` (`tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`) | 6 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | `Final result: PASS`, exit `0` |
| Runtime `AC-BD-026`, `AC-BD-027`, `AC-BD-058` | Lihat bagian 5.1–5.3 | `PASS` | API sungguhan terhadap `QuilvianNewDevSukma` + `SELECT` baca-saja |
| Keterpisahan fixture dan migration | 188 migration terterapkan sebelum dan sesudah; nol baris `TEST-BD006`..`016` bergeser | `PASS` | Bagian 5.4 |
| Galat scheduler absensi saat aplikasi berjalan | FK `HrdAttendanceProcessingRun` gagal | `EXISTING / ENVIRONMENT ISSUE` | Sudah dicatat `BE-BD-006`; tidak menyentuh jalur Bank Darah maupun Billing |

Uji manual: `PASS` — dijalankan lewat klien HTTP terkendali, bukan peramban.

**Tidak dijalankan:** `dotnet test` — repository tidak memiliki project test, dan kebijakan roadmap revisi 11
bagian 0.1 melarang membuatnya. Build ulang ketiga tidak dijalankan karena source tidak berubah sesudah build
kedua.

### 5.1 `AC-BD-026` — satu tindakan dengan 2 kantong, satu fakta

Aplikasi dijalankan dari hasil build (`http://localhost:5107`, `Development`) **tanpa** menerapkan migration
apa pun. Fixture `TEST-BD013-20260917193327` dibuat seluruhnya lewat API oleh SuperAdmin.

| Langkah | Request | HTTP | Keadaan database sesudahnya |
| --- | --- | --- | --- |
| Rantai kantong | order `ORD-00000089` (1 baris, 2 kantong) → PMI `PMI-00000044` → penerimaan `-01`, `-02` → simpan → alokasi → bukti kecocokan → pemberian | `200` seluruhnya | Kedua kantong `Issued` (status 4) pada order yang sama; alokasi aktif 2 |
| Catat tindakan | `POST /` `TND-00000066` | `200` | `Recorded`; **0** fakta, **0** charge line, **0** folio |
| Kirim ulang sebelum selesai | `POST /{id}/resend-cost-fact` | **`422`** "Fakta biaya hanya terbit untuk tindakan yang sudah selesai; tindakan ini berstatus Dicatat." | Tetap 0 fakta |
| **Selesaikan** | `POST /{id}/complete` | **`200`** — `BillingHandoff.Kind = Emitted`, `DispatchStatus = Dispatched`, `MilestoneFactVersion = 1` | **Tepat 1** `CliClinicalMilestoneFact`, **tepat 1** `BilChargeLine`, 1 `BilChargeComponent`, 1 `BilProcessingEffect`, 1 folio baru |
| Selesaikan lagi | `POST /{id}/complete` | **`422`** "Tindakan berstatus Selesai tidak dapat dinyatakan selesai lagi." | Baris riwayat `Complete` tetap **1** — `AC-BD-101` utuh |

Isi fakta yang tersimpan, dibandingkan dengan sumbernya:

| Unsur | Nilai tersimpan | Sesuai sumber |
| --- | --- | --- |
| `SourceContext` / `EffectType` / `MilestoneKind` | `BloodBank` / `BloodBankCharge` / `ChargeEligibility` | ✅ |
| `SourceAggregateId` | Id order `ORD-00000089` | ✅ |
| `SourceItemId` | Id tindakan `TND-00000066` | ✅ |
| `EncounterId` | Kunjungan order | ✅ |
| `OccurredAt` | `2026-09-17 13:23:36.275897+00` | ✅ sama persis dengan `OccurredAt` riwayat `Complete` |
| `Quantity` / `Unit` | `1.000000` / `Tindakan` | ✅ bukan 2 |
| `TariffSnapshot.unitPrice` | `121000.00` | ✅ sama dengan `TariffAmountSnapshot`; `tariffId` sama dengan tarif terpilih `BE-BD-012` |
| `CorrelationId` | Id order | ✅ |
| Charge line | `CalculationStatus = PendingFinancialReview`, `CalculatedAmount = null` | ✅ keputusan jumlah milik Billing |
| Folio | `ReviewRequired`, versi 1 | ✅ |
| Seluruh database | Fakta `BloodBank` total **1**, charge line `BloodBank` total **1** | ✅ |

### 5.2 `AC-BD-027` — kirim ulang tidak membuat charge kedua

| Langkah | HTTP | `BillingHandoff` | Database |
| --- | --- | --- | --- |
| `POST /{id}/resend-cost-fact` | `200` | `Replayed`, `MilestoneFactId` sama, versi `1` | Fakta, charge line, dan processing effect tetap 1 |
| `POST /{id}/resend-cost-fact` dengan body palsu (`sourceContext: Laboratory`, `unitPrice: 1`, `quantity: 2`, `encounterId` lain) | `200` | `Replayed`, identitas sama | Body diabaikan; tetap 1 |
| `GET /{id}` | `200` | `null` | — |

Perbandingan penuh sebelum/sesudah kedua kiriman ulang: baris fakta **identik** (termasuk `PayloadFingerprint`,
`IdempotencyKey`, `DispatchAttemptCount`), charge line **identik**, baris tindakan **identik**
(`UpdateDateTime` tidak bergeser), baris riwayat `Complete` tetap 1, salinan tarif tidak berubah.

**Batas bukti.** Karena fakta sudah `Dispatched`, producer mengembalikan `Replayed` **sebelum** memanggil
Billing. Idempotency di tingkat Billing (`IdempotencyKey` yang sama pada fakta berstatus `Pending`) adalah
jalur producer yang sudah ada dan tidak dapat dipicu lewat API tanpa menulis database langsung; jalur itu
tidak diuji ulang di sini.

### 5.3 `AC-BD-058` — koreksi pemberian tidak membalik biaya

Aktor penyetuju sungguhan `test.bd013.approver.20260917193327@rsmmc.local`: `Employee`, nol role, pada
Department/Position `TEST-BD013` tersendiri, hanya memegang `BloodUnit : Read` dan
`BloodUnit : ApproveCorrection`. Pengaju adalah SuperAdmin — dua orang berbeda, sesuai `VAL-BD-073`.

| Langkah | HTTP | Hasil |
| --- | --- | --- |
| Potret database sebelum koreksi | — | 1 fakta (`ChargeEligibility` v1), 1 charge line, 1 effect, folio `ReviewRequired` |
| Ajukan koreksi kantong `-01` (SuperAdmin) | `200` | Koreksi `Requested` |
| Setujui koreksi (aktor penyetuju) | `200` | Koreksi `Approved`, `DecidedByUserId` = penyetuju; kantong tetap `Issued`; **pemenuhan order 2 → 1** |
| Potret database sesudah | — | Fakta **identik**; jenis fakta tetap hanya `ChargeEligibility` — **nol** `ClinicalCancellation`; charge line **identik** (`CalculationStatus`, `Version`, `IsCancel`, `IsDelete`, `UpdateDateTime`); processing effect **identik**; folio **identik**; seluruh Billing kunjungan itu **identik** |
| Kirim ulang sesudah koreksi | `200` | `Replayed`, identitas sama — koreksi tidak mengubah isi fakta |
| Kirim ulang oleh aktor penyetuju (tanpa `BloodBankProcedure : Update`) | **`403`** | Fakta identik — hak akses endpoint baru ditegakkan untuk non-SuperAdmin |

Bukti source pendukung: jalur koreksi `BbkBloodUnitService` tidak memiliki satu pun dependensi ke
`ClinicalMilestoneFactProducer`, `BillingFolioService`, maupun entity `Bil*`/`Cli*` (dicari dengan `grep`
pada seluruh `Areas/HealthServices/BloodBankManagement`), dan `BuildCostFactRequestAsync` tidak memuat
jumlah maupun identitas kantong, sehingga koreksi tidak dapat mengubah sidik jari fakta.

### 5.4 Keterpisahan

| Pemeriksaan | Hasil |
| --- | --- |
| `__EFMigrationsHistory` | 188 baris sebelum dan sesudah; terakhir `20260917055332_AddBbkIssuanceCorrection` — nol migration diterapkan |
| Kantong fixture lain (`TEST-BD0%` selain `TEST-BD013`) yang dibuat/diubah sesudah 13:20 UTC | **0** |
| Order lain yang dibuat/diubah sesudah 13:20 UTC | **0** |
| Baris riwayat Bank Darah di luar entity `TEST-BD013` sesudah 13:20 UTC | **0** |
| Unit pemesan `TEST-BD006` | Tidak dipakai dan tidak berubah — `TEST-BD013` membuat unit pemesan sendiri |
| Kunjungan yang dipakai | Kunjungan rawat jalan yang sudah ada, dibaca saja; dipilih karena **belum** punya folio Billing maupun order Bank Darah, supaya folio uji tidak tercampur data lain |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-026` — satu tindakan selesai dengan 2 kantong diberikan menghasilkan satu fakta Billing, bukan dua | Terpenuhi | Bagian 5.1 — 2 kantong `Issued`, tepat 1 fakta dan 1 charge line, `Quantity = 1` |
| `AC-BD-027` — fakta biaya tindakan yang sama dikirim ulang bersifat idempotent; Billing tidak membuat charge kedua | Terpenuhi | Bagian 5.2 — dua kiriman ulang `Replayed`, seluruh baris identik; batas bukti tingkat Billing dicatat |
| `AC-BD-058` — koreksi pemberian tidak membalik fakta biaya tindakan secara otomatis | Terpenuhi | Bagian 5.3 — koreksi `Approved` oleh aktor berbeda, nol fakta pembatalan, Billing identik |
| Perilaku `BE-BD-012` dipertahankan | Terpenuhi, **dengan satu pengecualian yang disengaja** | `AC-BD-101` jalur gagal tetap `422`; `AC-BD-098/099/100` tidak disentuh. `AC-BD-102` ("nol fakta biaya, nol pemanggilan Billing") berstatus **HISTORICAL / SUPERSEDED oleh `DEC-BD-016` + `BE-BD-013`** atas keputusan pemilik 18 September 2026: **tidak dihapus**, tetap bukti historis `BE-BD-012` yang sah saat task itu ditutup, dan **bukan** syarat saat ini. Tercatat pada `00-interview-decisions.md` §8.31 |
| Build `0 Error(s)` | Terpenuhi | Bagian 5 |
| Nol migration, nol perubahan model | Terpenuhi | `has-pending-model-changes` bersih |
| QBE Strict `PASS` | Terpenuhi | Bagian 5 |
| Laporan, roadmap, dan traceability diperbarui | Terpenuhi | Bagian 3.2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga warning `CS1573` sempat muncul dari perubahan ini dan sudah diperbaiki sebelum build akhir. Build Debug memory-safe butuh ±1 jam pada workstation ini |
| Masalah yang diketahui | (1) Karena `resend-cost-fact` berbagi baris `SysActionAccess` `Update` dengan `complete`, kolom tampilan `RoutePath`/`HttpMethod` baris itu mengikuti atribut yang terakhir dipindai seeder. Kolom itu hanya tampilan/pencarian di layar Akses Role, bukan penegakan — pola yang sama sudah ada pada baris `Read` bersama. (2) `LoggerService` tidak menulis isi objek data ke log terstruktur, sehingga kode `BillingHandoff` tidak terlihat di log aplikasi; status yang sah tetap ada di ledger `CliClinicalMilestoneFact` dan audit `ClinicalFact.Dispatch` |
| Risiko tersisa | (1) Idempotency tingkat Billing untuk fakta `Pending` tidak diuji ulang lewat API. (2) Dokumen snapshot bertanggal — `01-prerequisite-readiness.md`, `02-existing-capability-map.md`, `02-requirement-completeness-assessment.md`, `03-domain-architecture.md` — menyebut `DEC-BD-016` terbuka sebagai keadaan saat audit; atas keputusan pemilik 18 September 2026 **dipertahankan sebagai snapshot historis**, bukan risiko yang harus ditutup. (3) `roadmap/frontend-roadmap.md` masih menyebut `BE-BD-013` future scope; sinkronisasi frontend adalah fase terpisah sesudah commit backend. **Riwayat butir (4):** teks `AC-BD-102` menunggu pemilik roadmap — **diputuskan 18 September 2026**: tidak dihapus, berstatus HISTORICAL / SUPERSEDED |
| Perubahan sampingan | `NONE` pada repository. Di luar repository: berkas skrip verifikasi di scratchpad sesi; berkas kata sandi sementara aktor uji **dihapus** sesudah dipakai |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.2 |
| Langkah berikutnya | Review diff oleh pemilik, lalu commit atas wewenang terpisah. Sesudahnya: fase sinkronisasi frontend (termasuk `frontend-roadmap.md` dan layar kirim ulang bila dikehendaki). Dokumen snapshot audit tidak disegarkan, dan `AC-BD-102` sudah diputuskan — keduanya atas keputusan pemilik 18 September 2026 |

### 7.1 Fixture `TEST-BD013-20260917193327` — belum dibersihkan

Seluruhnya dibuat lewat API di `QuilvianNewDevSukma`. Cleanup belum dijalankan dan belum disetujui.

| Jenis | Identitas |
| --- | --- |
| Unit pemesan | `a6ec356f-da4d-4ed4-b67e-6b6434ab6089` (`IsAvailableForBloodOrder = true`) |
| Komponen darah | `TBD013-P193327` `da64ce8b-9619-4376-938f-98ff39680fde` (validity 24 jam) |
| Lokasi penyimpanan | `TBD013-LOC193327` `be05798d-d460-4409-b702-a4ff201dbdef` |
| Alasan koreksi | `TBD013-KOR193327` `c6fda511-5d90-410c-bb39-78f968d39f01` |
| Order | `ORD-00000089` `4fb9cf3b-df46-4319-bc33-73266c0ddc39`, baris `ecc43fc4-df39-4c54-a928-8e6e84b4bf5b` |
| Permintaan PMI | `PMI-00000044` `a9128dc6-6954-4c5b-9a4b-887de571c22d` |
| Kantong | `-01` `0991186d-f51d-4133-a665-8b29bcc37048`, `-02` `d64cc129-c138-43d3-a7bf-e6afe4d550d6` — keduanya `Issued` (terminal) |
| Penempatan / alokasi / bukti | `65cb2fb1-…`, `41f29373-…` / `7e127675-…`, `579bd692-…` / `94811eb4-…`, `52cd537f-…` |
| Tindakan | `TND-00000066` `0d4cb399-8541-4430-a601-f1272adb1b77` — `Completed` |
| Koreksi | `6b87dfe9-8657-4aa2-9627-64794d7562de` — `Approved` |
| Ledger klinis | `CliClinicalMilestoneFact` `0eb2e6a1-219e-4cd8-8fd3-04adec27999f` |
| Billing | Folio `c8f74642-a6c0-4954-8ddd-9227d180657d` · charge line `1c280ed7-f308-4d23-a6a8-5656d7d35f19` · komponen `71a4f8aa-40eb-4069-aba0-463eab4ca38d` · processing effect `d19e3c29-f1a3-47ac-81af-4b4079d27b20` |
| Identitas | Department `610ca80e-d23c-4122-86b0-f752c845d415` · Position `eaceb02f-6b34-4146-8dec-fb980bb60c46` · Employee `1483c432-0385-498a-9399-da33e7c01c73` · user `6119442a-7113-4a8a-a0d7-ba5e65950756` · dua `SysAccessPolicy` `4fb31b23-…`, `ab85b2d9-…` |
| Data yang ada, dibaca saja | Pasien dan kunjungan rawat jalan `1448a376-e60e-4291-88f3-31c32a386cb7` (kini memiliki folio uji di atas); tindakan data induk `ALAT_1967` dan tarifnya; satu dokter |

**Jangan membersihkan berdasarkan prefix saja.** Kantong `Issued` terminal, folio dan charge line milik
Billing, dan kebijakan akses perlu dicabut terpisah.

### 7.2 Status Git

Cabang `sukmagp`, `HEAD` `cb3076d1`. Nol commit, nol push. `git diff --check` bersih. Nol berkas `Migrations/`
maupun frontend berubah. Keluaran `git status --short` di akhir pekerjaan:

```text
 M Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs
 M Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodBankProcedureController.cs
 M Areas/HealthServices/BloodBankManagement/DTOs/BloodBankProcedureDtos.cs
 M Areas/HealthServices/BloodBankManagement/Enums/BbkProcedureStatus.cs
 M Areas/HealthServices/BloodBankManagement/Models/BbkBloodBankProcedure.cs
 M Areas/HealthServices/BloodBankManagement/Services/BbkBloodBankProcedureService.cs
 M docs/module-blueprints/bank-darah/00-interview-decisions.md
 M docs/module-blueprints/bank-darah/02-backend-architecture.md
 M docs/module-blueprints/bank-darah/04-prd-to-mvp.md
 M docs/module-blueprints/bank-darah/MODULE-STATUS.md
 M docs/module-blueprints/bank-darah/contracts/api-contract.md
 M docs/module-blueprints/bank-darah/contracts/integration-contract.md
 M docs/module-blueprints/bank-darah/contracts/state-transition-matrix.md
 M docs/module-blueprints/bank-darah/data/data-dictionary.md
 M docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md
 M docs/module-blueprints/bank-darah/testing/acceptance-test-matrix.md
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-013.md
```
