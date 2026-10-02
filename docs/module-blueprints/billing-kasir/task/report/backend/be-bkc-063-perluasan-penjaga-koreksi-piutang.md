# BE-BKC-063 — Perluasan penjaga koreksi piutang

## Ringkasan untuk pembaca umum

Ini task terkecil dari seluruh rangkaian perbaikan gap `FINAL`→`CLOSED`, tapi menutup lubang yang
paling mahal: sebelum task ini, kalau ada salah tagih yang ditemukan **sesudah** pasien membayar
lunas, koreksinya tercatat di tagihan tapi **tidak pernah** memperbaiki catatan piutang rumah sakit
— tanpa galat, tanpa peringatan, tanpa jejak apa pun bahwa sesuatu terlewat. `BE-BKC-060`–`062`
membuat tagihan lunas sekarang benar-benar berpindah ke status "Closed"; task ini memastikan
koreksi yang diposting sesudahnya tetap tercatat sebagai koreksi piutang, bukan cuma dilewatkan.

---

- TASK ID: BE-BKC-063
- TASK TYPE: Perbaikan penjaga (satu kondisi, memperluas cakupan status yang diterima)
- COMPLEXITY: LIGHT
- CLASSIFICATION SCORE: 1 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah 1 → 0 + logika bisnis sedang, berdampak finansial → 1 + kontrak API tidak ada → 0 + database tidak ada → 0 + keamanan tidak ada → 0 + UI tidak ada → 0)
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED: `Services/BillingArApHandoffService.cs` (dibaca penuh — `RecordCorrectionIfLinkedAsync` dan pemanggilnya)
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs`
- IMPLEMENTATION: Baris 150 (sebelum task ini) — `if (invoice is null || invoice.Status != BillingInvoiceStatuses.Final) return;` — diperluas menjadi menerima `FINAL` **maupun** `CLOSED`; tetap menolak `OPEN` dan `SETTLED_BY_WRITE_OFF`. Doc-comment method diperbarui menjelaskan alasannya (`BKC-DES-035`/`BKC-DEC-101`). **Tidak ada baris lain yang disentuh** — badan method (pencarian `BilArHandoff`, idempotency per `sourceKey`, penulisan `BilHandoffAdjustment`) persis sama seperti sebelumnya.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: **`TOUCHED LEGACY`**. Tidak ada model, migration, maupun perubahan skema/API. Tidak ada QBE ID baru yang berlaku di luar yang sudah dipatuhi source aslinya.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Nol — `RecordCorrectionIfLinkedAsync` bukan endpoint, dipanggil internal dari empat titik `BillingFinancialExceptionService` (`BE-BKC-062`).
- DATABASE IMPACT: Nol perubahan skema. Efek tidak langsung: baris `BilHandoffAdjustment` kini juga dapat lahir untuk invoice `CLOSED`, tidak hanya `FINAL` — bentuk tabelnya tidak berubah.
- SECURITY IMPACT: Nol. Method ini tidak punya pintu masuk pengguna sendiri; otorisasi tetap diperiksa di controller pemanggil peristiwa aslinya (persetujuan/pembalikan penyesuaian atau write-off).
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | `dotnet build QuilvianSystemBackend.csproj` | **Belum dijalankan** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna yang berlaku sepanjang sesi |
  | Review diff dan scope | Dilakukan | Manual | Diff dikonfirmasi satu kondisi saja; badan method dan seluruh pemanggilnya (empat titik `BE-BKC-062`) tidak berubah |
  | `BIL-AT-129` (koreksi lahir untuk invoice `CLOSED` — skenario Tn. Budi), `BIL-AT-130` (tetap ditolak untuk `SETTLED_BY_WRITE_OFF`), `BIL-AT-131` (tetap ditolak untuk `OPEN`) | **Belum dijalankan** | NOT VERIFIED | Menuntut data invoice nyata dan database berjalan |
- WARNINGS: NONE
- KNOWN ISSUES:
  1. Belum dibangun/dijalankan — sama seperti `BE-BKC-060`–`062`.
  2. Task ini **hanya** memperbaiki penjaga di `RecordCorrectionIfLinkedAsync`. Ia tidak bisa diverifikasi bermakna sampai `BE-BKC-060`–`062` benar-benar membuat invoice berpindah ke `CLOSED` di database — keempat task karena itu **MUST** diverifikasi sebagai satu paket, bukan satu-satu terpisah.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS:
  ```text
   M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs
  ?? Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs
  ```
- NEXT RECOMMENDED STEP: Seluruh gelombang `MVP-24` (`BE-BKC-060`–`063`) kini source-complete. (1) **Pengguna menjalankan `dotnet build`** — titik verifikasi pertama untuk keenam berkas sekaligus. (2) Setelah build hijau, jalankan verifikasi proses bisnis end-to-end pada environment ter-autentikasi: bayar tagihan lunas → cek status `Closed` → posting koreksi → cek baris `BilHandoffAdjustment` benar-benar lahir. (3) Putuskan `KNOWN ISSUES` `BE-BKC-062` butir 2 (wiring alokasi deposit yang tidak pernah aktif). (4) `MVP-25` (`BE-BKC-064` dry-run, `BE-BKC-065` migration backfill) menyusul setelah `MVP-24` terverifikasi, sesuai urutan pada `roadmap/backend-roadmap.md`.
