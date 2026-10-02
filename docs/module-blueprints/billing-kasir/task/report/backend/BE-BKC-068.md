# BE-BKC-068 — Permukaan Pemeriksaan Ulang Keadaan Clearance

## Ringkasan untuk Pembaca Umum

Pada sistem rumah sakit, surat pemberitahuan pelunasan atau penjaminan resep obat yang dikirimkan secara otomatis dari modul kasir/Billing ke modul Farmasi dapat mengalami kendala teknis sementara di sisi penerima (misalnya gangguan layanan, aplikasi konsumen sempat mati, atau data terlewat). Apabila peristiwa tersebut terjadi, resep obat yang sebenarnya sudah lunas dibayar pasien dapat tertahan terus-menerus tanpa pernah disiapkan oleh apoteker, karena sistem Farmasi tidak mengetahui bahwa uang pembayaran telah berhasil diterima kasir.

Task ini membangun permukaan pemeriksaan ulang keadaan clearance resep (`ReadPrescriptionClearanceAsync`) di dalam modul Billing. Tanpa harus menunggu transaksi pembayaran berikutnya, instalasi farmasi dapat secara proaktif menanyakan status clearance resep kapan saja. Jika resep tersebut sudah memiliki riwayat surat pembayaran di Billing, sistem akan langsung menjawab persis sesuai data surat terakhir yang sah (apakah sudah lunas atau dijamin asuransi). Namun, apabila resep tersebut belum pernah memiliki surat clearance sama sekali di sistem Billing, sistem secara aman menjawab "belum diketahui" (`UNKNOWN`) dan obat belum boleh diserahkan (`fail-closed`). Permukaan ini berupa fungsi pembacaan murni dalam aplikasi tanpa membuka celah lalu lintas jaringan eksternal (nol endpoint HTTP), sehingga sangat cepat, aman, dan tidak memiliki efek samping.

---

- TASK ID: BE-BKC-068
- TASK TYPE: Fitur (Permukaan pemeriksaan ulang keadaan clearance resep in-process untuk modul Farmasi via `BilConsumerHandoffService.ReadPrescriptionClearanceAsync`)
- COMPLEXITY: LOW
- CLASSIFICATION SCORE: 2 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 3 → 1 + logika bisnis baca rekonsiliasi → 1 + kontrak in-process tanpa endpoint HTTP → 0 + database read-only → 0 + keamanan fail-closed → 0; total score 2)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (`BKC-DES-040`, line 3422, 3474, 3600)
  - `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INT-014`, baris 318–338)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-117`)
  - `docs/module-blueprints/billing-kasir/testing/acceptance-test-matrix.md` (`BIL-AT-141`, `BIL-AT-141-F`)
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilPrescriptionClearanceHandoff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Models/BilPrescriptionClearanceHandoff.cs` (menambahkan konstanta `PrescriptionClearanceStatuses.Unknown = "UNKNOWN"`)
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingConsumerHandoffDtos.cs` (model respon `PrescriptionClearanceStatusResponse`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs` (menambahkan method `ReadPrescriptionClearanceAsync`)
- IMPLEMENTATION:
  1. **Konstanta Status `UNKNOWN`**:
     Menambahkan `Unknown = "UNKNOWN"` pada `PrescriptionClearanceStatuses` sebagai representasi resmi untuk resep yang belum pernah memiliki surat clearance.
  2. **Model Respon DTO `PrescriptionClearanceStatusResponse`**:
     Menampung field: `PrescriptionId`, `ClearanceStatus`, `FinancialOutcome`, `ReasonCode`, `FinancialVersion`, `EffectiveAt`, `IsKnown`, dan `IsCleared`.
  3. **Method Pemeriksaan `ReadPrescriptionClearanceAsync`**:
     - Membaca secara murni (`AsNoTracking()`) baris `BilPrescriptionClearanceHandoff` aktif untuk `prescriptionId`, diurutkan menurun berdasarkan `FinancialVersion` (`OrderByDescending(x => x.FinancialVersion)`).
     - **Penegakan `BIL-AT-141`**: Jika ditemukan, mengembalikan data status clearance, hasil finansial, kode sebab, versi finansial, dan waktu efektif persis sesuai surat terakhir yang sah, dengan `IsKnown = true` dan `IsCleared = (ClearanceStatus == "CLEARED")`, walau surat tersebut belum diakui (`ACKNOWLEDGED`) oleh konsumen.
     - **Penegakan `BIL-AT-141-F` & `BIL-VAL-117` (Fail-Closed)**: Jika resep belum pernah memiliki surat sama sekali, mengembalikan respon `ClearanceStatus = "UNKNOWN"`, `IsKnown = false`, dan `IsCleared = false`. Sesuai mandat `PHA-DEC-067`, pemanggilan ini **tidak melempar galat** dan **tidak mengizinkan penyerahan obat**.
     - **Konsistensi Arsitektur (`BKC-DES-040`)**: Dibangun sebagai pemanggilan langsung dalam proses (in-process) pada assembly yang sama (`BIL-INT-010`–`012`), tanpa membuka endpoint HTTP baru. Operasi bersifat baca murni (idempoten) dan aman dipanggil berulang kali tanpa efek samping pada data.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk `BillingConsumerHandoffDtos.cs`; `TOUCHED LEGACY` untuk `BilPrescriptionClearanceHandoff.cs` dan `BilConsumerHandoffService.cs`.
  - QBE Compliance: Mematuhi `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-SVC-001`, dan `QBE-DB-001`.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: **Nol endpoint HTTP baru** untuk keperluan ini (memenuhi klausul Definition of Done: *"Method tersedia dan dipakai; nol endpoint HTTP baru untuk keperluan ini"*).
- DATABASE IMPACT: Nol perubahan skema database dan nol penulisan database (read-only query `AsNoTracking`).
- SECURITY IMPACT: Menjamin prinsip keselamatan klinis dan kepatuhan finansial fail-closed (`BIL-VAL-117`): resep yang tidak dikenal tidak pernah dinyatakan sebagai `CLEARED`.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` (3 berkas) | `PASS` | VERIFIED | `Files evaluated: 3`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none` |
  | `dotnet build` | **Menunggu eksekusi manual pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"biarkan saya build manual"* |
  | Review Diff dan Scope | Selesai | VERIFIED | Nol endpoint HTTP; implementasi baca murni sesuai dengan kontrak `BIL-INT-014` dan `BKC-DES-040` |
  | `BIL-AT-141` (surat belum diakui tetap terbaca) | Terpenuhi pada source | VERIFIED (Logic) | `ReadPrescriptionClearanceAsync` membaca surat terakhir tanpa menyaring kolom `Status == ACKNOWLEDGED` |
  | `BIL-AT-141-F` (resep belum punya surat dijawab UNKNOWN) | Terpenuhi pada source | VERIFIED (Logic) | Resep tanpa baris clearance mengembalikan `ClearanceStatus = UNKNOWN`, `IsKnown = false`, `IsCleared = false` tanpa melempar exception |
  | `BIL-VAL-117` (fail-closed) | Terpenuhi pada source | VERIFIED (Logic) | Status `UNKNOWN` tidak mengizinkan penyerahan obat (`IsCleared = false`) |
- WARNINGS: Tidak ada.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS: `BE-BKC-069` (Permukaan operasional surat yang menggantung `GET /operational/handoffs/pending`, `POST /operational/handoffs/pending/{id}/ack`).
