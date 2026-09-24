# BE-BKC-072 — Adapter Room Stay & Konsolidasi Alihan IGD Non-Destruktif

## Ringkasan untuk Pembaca Umum

Pada alur pelayanan rumah sakit, pasien rawat inap melalui berbagai fase hunian tempat tidur (mulai dari pertama kali menempati kamar/bed, mutasi pindah kamar, hingga pemulangan). Selain itu, terdapat dua skenario operasional penting yang sering terjadi:
1. **Koreksi Penempatan Kamar (*Room Correction*):** Terkadang staf perawat di bangsal salah menginput data kamar atau kelas tempat tidur pada sistem (misalnya salah memilih nomor bed). Ketika bangsal melakukan koreksi penempatan kamar, tagihan sewa kamar lama yang keliru harus dibatalkan secara otomatis oleh kasir/billing tanpa menghapus rekam jejak (*audit trail*), dan tidak boleh menimbulkan tagihan ganda bila koreksi dikirim ulang berulang kali (*idempoten*).
2. **Alihan Pasien Gawat Darurat (IGD) ke Rawat Inap:** Pasien gawat darurat yang kondisinya memerlukan rawat inap dialihkan dari IGD ke bangsal. Biaya tindakan dan obat-obatan yang sudah diberikan selama di IGD digabungkan ke dalam invoice rawat inap agar keluarga pasien cukup membayar di satu loket kasir saat kepulangan (*one-stop billing*). Namun, sistem tidak boleh menghapus atau mengubah identitas asal pelayanan IGD tersebut menjadi rawat inap; penanda asal domain IGD (`EMERGENCY`) wajib dipertahankan secara utuh demi transparansi rincian kwitansi dan audit pertanggungjawaban unit.

Task ini menyelesaikan kedua kapabilitas tersebut pada pintu masuk pencatatan tagihan (*Charge Intake Adapter*):
- **Registrasi Domain Kamar Rawat Inap (`ROOM_STAY` & `INPATIENT`):** Adapter Billing kini secara resmi mengenali dan menerima event hunian kamar dari rawat inap dengan empat status sah: `OCCUPIED` (sedang menempati), `TRANSFERRED` (pindah kamar), `CORRECTED` (koreksi kamar), dan `RELEASED` (selesai hunian/kamar dirilis).
- **Pembatalan Idempoten saat Koreksi Kamar (`ROOM_CORRECTION`):** Ketika bangsal mengirimkan event koreksi kamar, sistem Billing secara otomatis menandai baris tagihan kamar lama sebagai dibatalkan (*voided*) dengan alasan audit `"Koreksi penempatan kamar (ROOM_CORRECTION)"`. Pemanggilan ulang dengan data yang sama tidak akan menduplikasi tagihan atau menimbulkan galat.
- **Konsolidasi Multi-Domain IGD Non-Destruktif (`EMERGENCY`):** Domain `EMERGENCY` didaftarkan pada adapter Billing sehingga rincian tagihan alihan IGD dapat masuk ke dalam invoice rawat inap dengan tetap mempertahankan identitas `SourceDomain = "EMERGENCY"`.

---

- TASK ID: BE-BKC-072
- TASK TYPE: Adapter Charge Intake & Domain Policy Extension (`ContractBillingChargeSourceAdapter` & `BillingInvoiceService`)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah 2 → 1 + logika status lifecycle, idempotensi, dan integritas transaksi non-destruktif → 2; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/Services/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (arsitektur revisi 1.5, `BKC-DES-042`, `BKC-DES-048`)
  - `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INT-015`, `BIL-INT-017`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-124`, `BIL-VAL-125`)
  - `docs/module-blueprints/billing-kasir/00-interview-decisions.md` (`BKC-DEC-112`, `BKC-DEC-117`, `BKC-DEC-118`)
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs`
    - Menambahkan method `IsRoomStayDomain(string? sourceDomain)` dan `IsRoomCorrection(string? sourceDomain, string? sourceStatus)` pada interface `IBillingChargeSourceAdapter` dan kelas `ContractBillingChargeSourceAdapter`.
    - Menambahkan konstanta `IntegrationContractVersion12 = "BIL-INTEGRATION-1.2"` serta mendukung kedua versi kontrak (`BIL-INTEGRATION-0.4` dan `BIL-INTEGRATION-1.2`) pada `ValidateAndNormalize` dan `ValidateVoid`.
    - Mendaftarkan kebijakan domain `ROOM_STAY` dan `INPATIENT` ke `SourcePolicies` dengan status billable: `OCCUPIED`, `TRANSFERRED`, `CORRECTED`, `ROOM_CORRECTION`, `RELEASED`; status normal void: `OCCUPIED`, `TRANSFERRED`, `CORRECTED`, `ROOM_CORRECTION`; status void: `CORRECTED`, `ROOM_CORRECTION`, `CANCELLED`, `VOIDED`.
    - Mendaftarkan kebijakan domain `EMERGENCY` ke `SourcePolicies` dengan status billable: `CONFIRMED`, `ACCEPTED`, `COMPLETED`, `PERFORMED`, `DISPENSED`, `ADDED`; status normal void: `CONFIRMED`, `ACCEPTED`, `ADDED`; status void: `CANCELLED`, `VOIDED`.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`
    - Pada `UpsertChargeAsync`: menambahkan logika otomatis pembatalan idempoten saat menerima event koreksi kamar (`_sourceAdapter.IsRoomCorrection(...)`), mengubah status item kamar lama menjadi `BillingInvoiceItemStatuses.Voided` dengan alasan audit `ROOM_CORRECTION`.
    - Pada `UpsertChargeAsync`: menambahkan penanganan item kamar yang sudah pernah dibatalkan sebelumnya (`priorVoidedItem`) agar replay event idempoten tidak membuat item baru duplikat.
    - Menjamin pemeliharaan `SourceDomain = "EMERGENCY"` tanpa tertimpa saat item IGD masuk ke invoice rawat inap gabungan.
- IMPLEMENTATION:
  1. **Registrasi Kebijakan Domain Kamar (`ROOM_STAY` & `INPATIENT`)**:
     Memetakan siklus hidup hunian kamar rawat inap: saat pasien masih berada di tempat tidur (`OCCUPIED`), kamar belum selesai sehingga `IsOrderComplete` bernilai `false`. Begitu kamar dirilis atau pasien pulang (`RELEASED`), status kamar tidak lagi di `NormalVoidFromStatuses` sehingga `IsOrderComplete` bernilai `true`.
  2. **Penanganan Event Koreksi Kamar Idempoten (`ROOM_CORRECTION` / `CORRECTED`)**:
     Jika bangsal rawat inap mengoreksi kesalahan penempatan kamar, event koreksi kamar yang masuk langsung membatalkan baris tagihan kamar lama secara idempoten (`Status = Voided`). Jika event yang sama dikirim ulang (jaringan retry atau crash outbox bangsal), sistem mengenalinya sebagai replay aman tanpa efek samping atau duplikasi data (`BIL-VAL-125`, `BIL-AT-150`).
  3. **Multi-Domain Non-Destruktif IGD (`EMERGENCY`)**:
     Pendaftaran domain `EMERGENCY` memastikan `ValidateAndNormalize` menerima item dari IGD tanpa exception `SourceDomain belum didukung`. Properti `SourceDomain` pada `BilInvoiceItem` tetap menyimpan nilai `"EMERGENCY"`, memenuhi invariant non-destruktif `BKC-DEC-117` dan `BIL-VAL-124`.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `TOUCHED LEGACY` untuk `BillingChargeSourceAdapter.cs` dan `BillingInvoiceService.cs`.
  - QBE Rules Evaluated: `QBE-ENT-001`, `QBE-NAM-001`, `QBE-CFG-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-MOD-002`, `QBE-SVC-001`.
  - QBE Conformance Check Result: **PASS** (Mode Strict, 2 berkas dievaluasi, 0 violations, 0 reviews, 0 info).
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Kontrak integrasi diperluas untuk menerima `SourceDomain`: `"ROOM_STAY"`, `"INPATIENT"`, dan `"EMERGENCY"`, serta kontrak versi `BIL-INTEGRATION-1.2` (`BIL-INT-015`, `BIL-INT-017`).
- DATABASE IMPACT: Nol perubahan skema database fisik. Seluruh perubahan memanfaatkan skema dan kolom yang sudah ada (`BilInvoiceItem.SourceDomain`, `BilInvoiceItem.Status`, `BilInvoiceItem.VoidReason`).
- SECURITY IMPACT: Pemetaan status billable yang ketat mencegah masuknya status tidak sah dari luar batas kewenangan producer.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` | **PASS** | VERIFIED | Strict mode, 2 berkas dievaluasi (`BillingChargeSourceAdapter.cs`, `BillingInvoiceService.cs`), 0 pelanggaran QBE |
  | `dotnet build` | **Menunggu eksekusi mandiri pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"Akan tetapi, jangan jalankan build secara automatis"* |
  | Registrasi Domain Kamar (`ROOM_STAY`, `INPATIENT`) | Selesai | VERIFIED (Code Inspection) | Status billable `OCCUPIED`, `TRANSFERRED`, `CORRECTED`, `ROOM_CORRECTION`, `RELEASED` terdaftar di `SourcePolicies` |
  | Registrasi Domain IGD (`EMERGENCY`) | Selesai | VERIFIED (Code Inspection) | Status billable `CONFIRMED`, `ACCEPTED`, `COMPLETED`, `PERFORMED`, `DISPENSED`, `ADDED` terdaftar di `SourcePolicies` |
  | Pembatalan Idempoten `ROOM_CORRECTION` | Selesai | VERIFIED (Code Inspection) | Logika `_sourceAdapter.IsRoomCorrection` membatalkan baris lama ke status `Voided` dan replay aman |
  | Konsolidasi IGD Non-Destruktif | Selesai | VERIFIED (Code Inspection) | `item.SourceDomain = source.SourceDomain` mempertahankan `"EMERGENCY"` tanpa ditimpa menjadi `"RANAP"` |
- WARNINGS:
  - Kompilasi proyek (`dotnet build`) belum dijalankan secara otomatis mengikuti instruksi pengguna; silakan jalankan `dotnet build` secara mandiri.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS:
  - `BE-BKC-073` (Mesin Kalkulasi Sewa Kamar Bertingkat & Pro-Rata Transfer Menit Riil)
  - `BE-BKC-074` (Layanan Biaya Administrasi Ranap 7% Cap Rp6jt & Penggantian Admin Rajal)
  - `BE-BKC-075` (Layanan Kelayakan Pemulangan Financial Clearance & Auto-Reblock Handoff)
