# Laporan Perubahan Backend — `BE-FIN-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-006` |
| Judul | Entity buku piutang |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-1`) |
| Trace | `FIN-DES-010`..`014`, `024`; `FR-FIN-020`..`024`; `FIN-VAL-011` (invariant seimbang), `FIN-VAL-010`/`014`/`016`; `FIN-STATE-1.0` §2/§3 |
| Contract version | `FIN-VAL-1.0` §piutang (draft) — dipatuhi penuh untuk bentuk kolom dan check constraint; perilaku state-machine (`FIN-STATE-1.0`) belum ditegakkan karena belum ada service |
| Dependency | `BE-FIN-005` — 🟡 sebagian 21 September 2026 (entity+configuration selesai), lihat [laporan](BE-FIN-005.md) |
| Klasifikasi | `MEDIUM` — satu repository; 10 berkas dibuat + 1 berkas registrasi diubah; tidak ada logika bisnis (murni entity+configuration, 5 entity sekaligus); database berdampak pada bentuk skema (migration terpisah); tidak ada endpoint/keamanan baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Models/`, `Repositories/Configurations/Corporate/FinanceManagement/Receivable/`, `Repositories/ApplicationDbContext.cs` (registrasi `DbSet` saja) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Cakupan task ini (entity+configuration) terpenuhi penuh. Invariant nilai piutang seimbang (AC roadmap) terpasang sebagai check constraint database; perilaku state-machine dan penulisan `OutstandingAmount` yang sebelumnya menunggu pemilik task sudah dibangun `BE-FIN-008` (`FinanceReceivableService`). `dotnet build` PASS dan migration diterapkan — dikonfirmasi pengguna 23 September 2026, lihat Pembaruan bagian 7 |

---

## 1. Ringkasan keputusan cakupan

Roadmap mencantumkan Cakupan `BE-FIN-006` persis lima entity: `FinReceivable`, `FinReceivableItem`,
`FinReceivableDocument`, `FinReceivableAdjustment`, `FinReceivableWriteOff` — tanpa service maupun
controller (`FinanceReceivableService`/`FinanceReceivablesController` ada di arsitektur tetapi
Cakupan-nya belum ditugaskan ke task mana pun, temuan yang sudah dilaporkan di `BE-FIN-005`).
Migration (`AddFinanceReceivableAndCollection`, digabung dengan tabel penerimaan) adalah
`BE-FIN-007`, task terpisah.

Acceptance criteria roadmap — "Invariant nilai piutang seimbang terpasang sebagai check
constraint" — **sepenuhnya tercapai** pada slice ini, karena inilah persis yang diminta: sebuah
check constraint database, bukan perilaku service. Ini berbeda dari `BE-FIN-005`, yang AC-nya
menuntut perilaku runtime yang tidak tercapai pada entity+configuration saja.

**Naming collision check**: digrep seluruh `Areas/` untuk `FinReceivable`, `Fin{Receivable,Payable,Receipt}*`,
dan `class\s+\w*Receivable\w*` — nol hasil source code (hanya disebut di komentar Indonesia pada
modul Accounting, mis. `AccChartOfAccount.cs` yang menjelaskan sifat akun `Piutang` sebagai
`Asset`, bukan entity). Konsisten dengan `FIN-CAP-009` (`01-existing-capability-map.md`): "Seluruh
entity ini harus dibangun dari nol". Tidak ada tabrakan seperti `MstBank` pada `BE-FIN-002`.

---

## 2. Proses bisnis

`NOT APPLICABLE` secara langsung — tidak ada service/endpoint yang berjalan pada task ini.
Ringkasan proses bisnis yang **akan** dilayani skema ini (konteks pembaca):

1. Fakta AR dari Billing (lewat `FinBillingHandoffIntake`, `BE-FIN-005`) diolah menjadi satu
   `FinReceivable` sebesar **sisa tanggungan setelah dikurangi bayar tunai** (`FR-FIN-020`) —
   contoh: tagihan Rp 5.000.000, pasien bayar Rp 1.500.000 di kasir, piutang yang terbentuk
   Rp 3.500.000 (bukan Rp 5.000.000).
2. Nilai piutang **selalu seimbang** (`FR-FIN-021`, `FIN-VAL-011`):
   `OriginalAmount = OutstandingAmount + AllocatedAmount + AdjustedAmount + WrittenOffAmount`.
   Contoh: piutang Rp 3.500.000, sudah dilunasi Rp 1.000.000, dikoreksi turun Rp 500.000 → sisa
   Rp 2.000.000, dan `2.000.000 + 1.000.000 + 500.000 = 3.500.000` selalu sama dengan nilai asli.
3. Kelengkapan berkas klaim (`FinReceivableDocument`) **tidak pernah** menahan pengakuan piutang
   (`FR-FIN-023`) — piutang tetap terbentuk penuh sejak hari pertama walau belum ada satu berkas
   pun, hanya `ClaimStatus` yang menandai `INCOMPLETE`.
4. Koreksi (`FinReceivableAdjustment`) dan penghapusan buku (`FinReceivableWriteOff`) memakai
   maker-checker dua lapis: pengaju (`RequestedBy`) tidak boleh menjadi penyetuju
   (`ApprovedBy`) — ditegakkan check constraint database **dan** (kelak) service (`FIN-DEC-012`).
5. `FinReceivableItem` merinci piutang ke pasien/kunjungan/tagihan; `PatientId`/`EncounterId`
   sensitif dan **MUST NOT** ikut payload Accounting nanti.

**Belum dibangun pada task ini**: alur penerapan aturan-aturan di atas (itu tanggung jawab
`FinanceReceivableService`, satu-satunya penulis `OutstandingAmount` per `FIN-DES-011`), termasuk
mesin status `FIN-STATE-1.0` §2/§3 (`OUTSTANDING → PARTIAL/SETTLED`, `REQUESTED → APPROVED/REJECTED`, dst).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §2 (kolom kelima entity) dan §9.1 (DDL lengkap, termasuk catatan "DDL adalah dokumentasi bentuk tabel, bukan skrip yang dijalankan")
- `docs/module-blueprints/finance-management/erd/receivable-collection.md` §2–§5 (ERD mermaid, tabel status entity, catatan salah-baca)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §1.2 (kepemilikan data, baris "Piutang... Ya — baru. Tidak ada pemilik lain"), §2.3–2.4 (`FIN-DES-010`..`014`, `024`), §3.2 (class diagram), §4.2–4.6 (rincian tiap class), §5 (file-tree), §9 (tabel invariant)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` §2 (`FIN-VAL-010`..`016`) dan §3 (`FIN-VAL-020`..`025`)
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §2 (`FinReceivable`) dan §3 (`FinReceivableAdjustment`/`WriteOff`, matriks sama)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `EPIC FIN-03`, `FR-FIN-020`..`024`, `UAT-08`..`012`
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` — `FIN-CAP-009` (Missing, dibangun dari nol)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — baris `FinanceManagement / Receivable / Piutang` (`Fin`, `ACTIVE`) sudah terdaftar `BE-FIN-001`
- `Areas/Corporate/FinanceManagement/BillingIntake/{Models,Configurations}/FinBillingHandoffIntake*` (`BE-FIN-005`) — pola entity+configuration terdekat yang ditiru langsung
- `Repositories/Configurations/Corporate/FinanceManagement/PettyCash/FinPettyCashBudgetConfiguration.cs` — pola `RowVersion.IsConcurrencyToken()`, `table.HasCheckConstraint(...)`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` | **Baru.** Aggregate root; `FinReceivableStatuses`/`FinReceivableDebtorTypes`/`FinReceivableClaimStatuses` |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableItem.cs` | **Baru.** Anak, tanpa `RowVersion` (bukan aggregate root sendiri) |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableDocument.cs` | **Baru.** Anak, tanpa `RowVersion` |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableAdjustment.cs` | **Baru.** Maker-checker, `RowVersion`; `FinReceivableAdjustmentDirections`, `FinReceivableApprovalStatuses` (dipakai bersama `WriteOff`) |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableWriteOff.cs` | **Baru.** Maker-checker, `RowVersion`; memakai ulang `FinReceivableApprovalStatuses` |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs` | **Baru.** 5 check constraint (`DebtorType`, `Status`, `Outstanding>=0`, **Balance** — invariant AC roadmap, `BenefitOwner`), 2 unique index, 1 index biasa |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableItemConfiguration.cs` | **Baru.** FK `Restrict` ke `FinReceivable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableDocumentConfiguration.cs` | **Baru.** FK `Restrict` ke `FinReceivable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableAdjustmentConfiguration.cs` | **Baru.** FK `Restrict`, 4 check constraint termasuk maker-checker, unique index nomor |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableWriteOffConfiguration.cs` | **Baru.** FK `Restrict`, 3 check constraint (tanpa `Direction`), unique index nomor — lihat catatan ambiguitas bagian 3.3 |
| `Repositories/ApplicationDbContext.cs` | Registrasi 5 `DbSet` baru + using untuk namespace `Receivable.Models` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | 5 entity baru siap-migration. **Belum ada migration** (`BE-FIN-007`, digabung 7 tabel piutang+penerimaan) |
| Keamanan/Auth | `NOT APPLICABLE`. Catatan privasi untuk task mendatang: `FinReceivable.BenefitOwnerId`/`BenefitRelationship` dan `FinReceivableItem.PatientId`/`EncounterId` bertanda **Sensitif** — MUST NOT ikut payload Accounting maupun log |

**Ambiguitas kontrak yang dicatat, bukan diputuskan sepihak** (mengikuti preseden `BE-BKC-033`):
1. `data-dictionary.md` menandai `SourceHandoffId`/`InvoiceId`/`DebtorReferenceId`/`BenefitOwnerId`/`CorrelationId` pada `FinReceivable`, dan beberapa kolom rujukan pada `FinReceivableItem`/`Adjustment`, dengan anotasi kolom "Index" — tetapi DDL §9.1 hanya mendefinisikan **tiga** index bernama untuk `FinReceivable` (`ReceivableNumber`, `SourceHandoffKey` unik; `Status`+`DueDate`) dan **satu** untuk `FinReceivableAdjustment` (`AdjustmentNumber` unik). Daftar DDL bernama yang diikuti, sama seperti `BE-FIN-005`.
2. **`IX_FinReceivableWriteOff_Number`** — nama index ini **tidak tertulis eksplisit** di DDL manapun; dokumen hanya menyatakan "`FinReceivableWriteOff` memakai bentuk yang sama dengan `FinReceivableAdjustment`... dengan check constraint maker-checker yang identik". Ditambahkan by analogy langsung ke `IX_FinReceivableAdjustment_Number` (nama, filter, keunikan identik), konsisten dengan kolom `WriteOffNumber` yang ditandai `UK` pada tabel kolom. Begitu pula check constraint `CK_FinReceivableWriteOff_Status`/`Amount`/`MakerChecker` — didasarkan pada pernyataan eksplisit "bentuknya sama persis... kecuali Direction dan SourceHandoffAdjustmentId", bukan dikarang.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi eksplisit pengguna sejak `BE-FIN-002`. **Catatan**: `git diff` pada `Repositories/ApplicationDbContext.cs` menunjukkan pengguna sendiri sudah menambahkan `#pragma warning disable CS8618` di berkas ini secara independen — indikasi pengguna kemungkinan sudah mulai membangun/meninjau sendiri, sesuai anjuran laporan sebelumnya |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Sempat melebihi timeout 120 detik (kemungkinan proses lain berjalan bersamaan, mis. `dotnet build` pengguna), lalu diselesaikan di background: `Files evaluated: 29`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan di bawah |
| Review manual: kolom/tipe/default/check constraint/index pada seluruh 5 configuration dicocokkan satu-per-satu terhadap DDL `data-dictionary.md` §9.1 | Cocok, kecuali dua ambiguitas yang dicatat eksplisit pada bagian 3.3 (bukan kekurangan, melainkan area yang didokumentasikan) | `PASS` | Perbandingan manual pada sesi ini |
| Review manual: invariant "Balance" (AC roadmap) benar-benar berbentuk check constraint | `CK_FinReceivable_Balance CHECK ("OriginalAmount" = "OutstandingAmount" + "AllocatedAmount" + "AdjustedAmount" + "WrittenOffAmount")` — persis `FIN-VAL-011` | `PASS` | `FinReceivableConfiguration.cs` |
| Review manual: maker-checker pada `Adjustment` dan `WriteOff` | `CK_..._MakerChecker CHECK ("ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy")` pada keduanya, sesuai `FIN-DES-014` | `PASS` | Kedua berkas configuration |
| Review manual: `FinReceivableItem`/`FinReceivableDocument` tanpa `RowVersion` (bukan aggregate root) | Dikonfirmasi — hanya `FinReceivable`, `FinReceivableAdjustment`, `FinReceivableWriteOff` yang punya `RowVersion`, sesuai DDL §9.1 yang tidak mencantumkan kolom itu pada dua entity anak tersebut | `PASS` | Model dan DDL |
| Review governance: submodule `Receivable` sudah terdaftar registry sebelum file model ditulis | Terpenuhi — baris `FinanceManagement / Receivable / Piutang` sudah ada sejak `BE-FIN-001` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 29
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis.

Uji manual: `NOT APPLICABLE` — tidak ada endpoint/UI pada task entity+configuration ini. "Uji unit
invariant" pada kolom Verifikasi roadmap ditafsirkan sebagai bukti bahwa check constraint
`CK_FinReceivable_Balance` terpasang di database (mekanisme penegakan invariant tanpa test
otomatis) — bukan sebagai wewenang membuat project test baru, yang dilarang `TEST_POLICY.md`
kecuali diminta eksplisit.

**Tidak dijalankan:** `dotnet build` (lihat tabel), migration/eksekusi database (`NOT APPLICABLE`,
migration adalah `BE-FIN-007` terpisah).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Invariant nilai piutang seimbang terpasang sebagai check constraint | ✅ **Terpenuhi** | `CK_FinReceivable_Balance` pada `FinReceivableConfiguration.cs` |
| DoD: Prefix `Fin` | Terpenuhi — seluruh 5 entity `Fin*` | Nama class dan `[Table(...)]` |
| DoD: `Guid RowVersion` pada aggregate root | Terpenuhi — `FinReceivable`, `FinReceivableAdjustment`, `FinReceivableWriteOff` (ketiganya aggregate root/semi-independen sesuai DDL); `FinReceivableItem`/`Document` sengaja tanpa `RowVersion` karena bukan aggregate root sendiri, konsisten data dictionary | Model `*.cs` |

Kriteria yang secara literal menjadi tanggung jawab slice ini **seluruhnya terpenuhi**. Perilaku
runtime (state machine, penulisan `OutstandingAmount`, aging 4 kelompok `FR-FIN-022`) tetap
menunggu `FinanceReceivableService` sesuai catatan `BE-FIN-005`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi `dotnet build` PASS dan migration sudah diterapkan ke database. Gap `FinanceReceivableService` pada baris Peringatan di bawah sudah ditutup `BE-FIN-008`. Status task dinaikkan menjadi ✅ SELESAI |
| Peringatan | Gap roadmap `FinanceBillingIntakeService`/`FinanceReceivableService` tanpa task pemilik eksplisit (dilaporkan pertama kali di `BE-FIN-005`) makin relevan sekarang: `BE-FIN-006` menyelesaikan skema piutang, tetapi tidak ada satu pun task berikutnya yang secara eksplisit menyebut `FinanceReceivableService` pada Cakupan-nya kecuali tersirat di `BE-FIN-008` ("Layanan piutang: umur, koreksi, penghapusan") — ini task yang tepat, jadi gap ini **kemungkinan sudah tertutup untuk sisi Receivable** (berbeda dari sisi Billing Intake yang masih terbuka) |
| Masalah yang diketahui | Dua ambiguitas kontrak dicatat eksplisit pada bagian 3.3 (index kolom vs DDL bernama; nama index `WriteOff` by analogy) |
| Risiko tersisa | Rendah untuk task ini sendiri (aditif, sudah dicocokkan manual terhadap DDL). Risiko build tetap bertumpuk — lihat tabel Verifikasi; QBE checker sempat timeout pada sesi ini, kemungkinan proses lain berjalan bersamaan |
| Perubahan sampingan | `NONE` dari saya. Terdeteksi perubahan **milik pengguna** pada `Repositories/ApplicationDbContext.cs` (penambahan `#pragma warning disable CS8618` dan satu baris whitespace) — **tidak disentuh/dibatalkan**, dibiarkan apa adanya sesuai `TASK_RULES` |
| Interupsi | `NONE` pada task ini |
| Status Git | 10 berkas baru (5 model, 5 configuration) + `ApplicationDbContext.cs` berubah untuk task ini, di atas berkas `BE-FIN-002`..`005` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` mencakup `BE-FIN-002`..`006`. (2) Lanjut `BE-FIN-007` (migration gabungan, perlu otorisasi terpisah) atau `BE-FIN-008` (`FinanceReceivableService`) — keduanya dependency-nya `BE-FIN-006`, yang sudah terpenuhi untuk bagian yang menjadi tanggung jawabnya |
