# Billing dan Kasir — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Revision | `0.4` |
| Blueprint status | `APPROVED` |
| Roadmap revision/status | `1` / `DRAFT_FORWARD_TEST` (dokumen roadmap sendiri belum diperbarui — evidence per task sudah ditambahkan di `roadmap/frontend-roadmap.md` dan `roadmap/requirement-traceability.md`) |
| Current phase | `BKC-PH-005/006 — IMPLEMENTATION_IN_PROGRESS` (lihat rekonsiliasi di bawah) |
| Approved at | 20 Agustus 2026, 13:41 WIB |
| Approval evidence | Product/Domain Owner menyatakan “saya aprove” pada percakapan |
| Backend snapshot | `8e48237` (`Yasmina`) — mencakup part 1 (`1d61a5b`) dan part 2 (`22bf9cf`) |
| Frontend snapshot | `fac1b49c8` (`yasmina`) — bagian "part 1"; delapan task `FE-BKC-003`–`008` + navigasi menu ada di source lokal branch yang sama tapi **belum di-commit** |

## Rekonsiliasi 25 Agustus 2026 (`ISSUE-FE-003`)

Dokumen ini sebelumnya menyatakan roadmap `DRAFT_FORWARD_TEST` dan implementasi `NOT_STARTED`,
padahal source sudah jauh lebih maju. Rekonsiliasi berikut berdasarkan pembacaan langsung
controller/service terkait (bukan git-log commit message saja):

- **Backend**: Controller berikut sudah ada dan dibaca langsung di source (`Areas/HealthServices/BillingManagement/`):
  `BillingInvoicesController`, `BillingPatientFundsController`, `BillingSettlementsController`,
  `BillingFinancialExceptionsController`, `CashierShiftsController`, `BillingFinalizationsController`,
  plus 6 controller Master Data (`PaymentMethod`, `BillingItemCategory`, `AdministrationFeePolicies`,
  `DiscountPolicies`, `TaxRules`, `RoomChargePolicies`). Ini mencakup **mayoritas besar** `BE-BKC-001`–`016`
  dari `roadmap/backend-roadmap.md` berdasarkan cakupan endpoint yang cocok dengan `contracts/api-contract.md`
  (lihat rekonsiliasi di file itu). **Belum dilakukan**: pemetaan formal satu-per-satu commit ↔ `BE-BKC-NNN`
  (masih direkomendasikan sebagai task terpisah), dan cakupan `BE-BKC-017` (security/privacy/concurrency
  hardening) belum diverifikasi eksplisit.
- **Frontend**: `FE-BKC-001`/`002` (bagian "part 1", commit `fac1b49c8`) plus `FE-BKC-003`–`008` dan navigasi
  menu (source lokal, belum commit, lulus lint/build/`test:unit` — lihat laporan masing-masing di
  `task/report/frontend/`). **Belum dibangun**: `FE-BKC-009` (finalisasi invoice — meski backend-nya,
  `BillingFinalizationsController`, sudah ada) dan `FE-BKC-010` (security/privacy/concurrency hardening).
- **Belum diverifikasi sama sekali untuk seluruh slice**: klik-coba ter-autentikasi, migration Postgres
  dieksekusi ke database bersama, dan integrasi payment provider nyata (`BKC-BLK-PROV-001` masih berlaku —
  `DeferredBillingPaymentProviderAdapter` masih stub).
- **Tambahan 25 Agustus 2026 (`ISSUE-FE-006`, `ISSUE-FE-007`, `ISSUE-FE-008`)**: endpoint `GET` baru pada
  Cashier Shifts (`{id}`) dan Financial Exceptions (list per invoice, refundable-credits, by-id per tipe),
  serta domain Master Data baru "Register" (model `MstRegister`, service, controller CRUD) — source ada,
  **migration BELUM digenerate/dieksekusi** (memerlukan otorisasi terpisah sesuai `AGENTS.md`).

## Phase state

| Phase | Status | Evidence |
| --- | --- | --- |
| Discovery/capability/gate/domain | `DONE` | Decision `0.2`, gate/domain `0.3` |
| Business module design | `DONE` | Blueprint dan enam kontrak `0.4` approved |
| Design approval | `DONE` | `BIL-APR-001` ditutup 20 Agustus 2026 |
| Delivery planning | `DONE` | Roadmap revision `1`: 17 BE + 10 FE task |
| Task approval | `SUPERSEDED_BY_EVIDENCE` | Source sudah jauh melampaui rencana task-by-task; lihat rekonsiliasi di atas |
| Implementation/verification | `IN_PROGRESS` | Backend BE-BKC-001–016 dan Frontend FE-BKC-001–008 ada di source; BE-BKC-017 belum diverifikasi cakupannya; FE-BKC-009/010 belum dibangun; verifikasi manual ter-autentikasi dan migration Register masih tertunda |

## Delivery dependency

| ID | Dependency | Dampak |
| --- | --- | --- |
| `BKC-BLK-FE-001` | Governance root frontend — **RESOLVED**, ditemukan dan dipakai sejak sesi 25 Agustus 2026 | Frontend write sudah berjalan (`FE-BKC-003`–`008`) |
| `BKC-BLK-INT-001` | Consumer contract AR/AP belum dibuktikan | Menahan final `BE-BKC-016`/verifikasi handoff nyata |
| `BKC-BLK-PROV-001` | Provider sandbox/callback belum dipilih | Menahan provider E2E; `DeferredBillingPaymentProviderAdapter` masih stub, tender non-tunai selalu Pending |
| `BKC-BLK-DATA-001` | Nilai seed Finance/Inpatient belum diserahkan | Menahan aktivasi master, tidak menahan model/API |

## Next recommended task

Implementasi source untuk hampir seluruh backend dan sebagian besar frontend sudah ada — fokus
berikutnya BUKAN lagi "task pertama" melainkan: (1) verifikasi manual ter-autentikasi untuk seluruh
slice yang sudah source-complete, (2) commit tujuh task frontend yang menumpuk, (3) migration
`MstRegister` (butuh otorisasi eksplisit terpisah untuk generate dan `Update-Database`), (4)
`FE-BKC-009` (finalisasi invoice — backend `BillingFinalizationsController` sudah tersedia) dan
`FE-BKC-010` (security/privacy/concurrency), (5) audit cakupan `BE-BKC-017` yang belum diverifikasi.
Builder tidak boleh menjalankan migration ke database tanpa izin terpisah.
