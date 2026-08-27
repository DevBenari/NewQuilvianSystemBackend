# Billing dan Kasir — Permission & Audit Matrix

`contract_version: BIL-PERMISSION-0.4` · status **approved** · owner Security dan process owner · approved 20 Agustus 2026. String berikut adalah target exact string `[AccessPermission(...)]`.

| Endpoint/aksi | Resource/action dan string | Logger | Audit fact wajib |
| --- | --- | :---: | --- |
| `GET invoices` | `[AccessPermission("BillingInvoice", "Read")]` | Tidak | access log standar saja |
| `POST from-source` | `[AccessPermission("BillingInvoice", "Create")]` | Ya | source tuple, result ID, correlation |
| recalculate/void | `[AccessPermission("BillingInvoice", "Update")]` | Ya | version, reason, before/after total |
| apply discount | `[AccessPermission("BillingDiscount", "Create")]` | Ya | policy, target, amount, actor |
| doctor approve | `[AccessPermission("BillingDoctorDiscount", "Approve")]` | Ya | doctor actor, own-share evidence |
| deposit read | `[AccessPermission("BillingDeposit", "Read")]` | Tidak | access standar |
| top-up | `[AccessPermission("BillingDeposit", "Create")]` | Ya | amount/method/shift/correlation |
| allocation | `[AccessPermission("BillingDeposit", "Allocate")]` | Ya | balance before/after, target |
| payment create/tender | `[AccessPermission("BillingPayment", "Create")]` | Ya | amount/method/status, no provider payload |
| adjustment create/approve | `[AccessPermission("BillingAdjustment", "Create")]` / `[AccessPermission("BillingAdjustment", "Approve")]` | Ya | maker/approver/reason/direction |
| refund create/approve | `[AccessPermission("BillingRefund", "Create")]` / `[AccessPermission("BillingRefund", "Approve")]` | Ya | original tender, proportional result |
| write-off create/approve | `[AccessPermission("BillingWriteOff", "Create")]` / `[AccessPermission("BillingWriteOff", "Approve")]` | Ya | outstanding before/after |
| reverse exception | `[AccessPermission("BillingFinancialException", "Reverse")]` | Ya | original/new entry correlation |
| shift open | `[AccessPermission("CashierShift", "Create")]` | Ya | register/opening cash |
| shift read | `[AccessPermission("CashierShift", "Read")]` | Tidak | access standar |
| handover/close | `[AccessPermission("CashierShift", "Handover")]` / `[AccessPermission("CashierShift", "Close")]` | Ya | both actors, system/physical/variance |
| variance/reopen | `[AccessPermission("CashierShift", "Review")]` / `[AccessPermission("CashierShift", "Reopen")]` | Ya | authority, reason, outcome |
| finalization read | `[AccessPermission("BillingFinalization", "Read")]` | Tidak | access standar |
| finalization create | `[AccessPermission("BillingFinalization", "Create")]` | Ya | calculation version, outcome, AR/AP keys |
| administration fee policy read/create/update | `[AccessPermission("AdministrationFeePolicy", "Read")]` / `[AccessPermission("AdministrationFeePolicy", "Create")]` / `[AccessPermission("AdministrationFeePolicy", "Update")]` | GET Tidak; command Ya | effective period, nominal, actor |
| discount policy read/create/update | `[AccessPermission("DiscountPolicy", "Read")]` / `[AccessPermission("DiscountPolicy", "Create")]` / `[AccessPermission("DiscountPolicy", "Update")]` | GET Tidak; command Ya | target, value/limit, approval rule |
| tax rule read/create/update | `[AccessPermission("TaxRule", "Read")]` / `[AccessPermission("TaxRule", "Create")]` / `[AccessPermission("TaxRule", "Update")]` | GET Tidak; command Ya | rate, rounding, allocation |
| room charge policy read/create/update | `[AccessPermission("RoomChargePolicy", "Read")]` / `[AccessPermission("RoomChargePolicy", "Create")]` / `[AccessPermission("RoomChargePolicy", "Update")]` | GET Tidak; command Ya | period, rounding, tariff moment |

Audit disimpan append-only dengan actor, role, time, reason, correlation, entity/version, hasil, dan perubahan nominal. GET tidak memakai custom logger sesuai pola project. Custom log **dilarang** memuat nama/nomor identitas pasien, EncounterId mentah bila tidak perlu, debtor evidence, description klinis, provider reference lengkap, token, credential, nomor kartu, atau payload callback. Maker-checker diperiksa backend, bukan hanya permission. Tests `BIL-AT-012`,`014`,`016`,`022`,`024`.
