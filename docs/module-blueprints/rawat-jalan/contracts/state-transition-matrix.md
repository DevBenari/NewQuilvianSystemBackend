# State Transition Matrix — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-STATE-001@1.0.0` |
| Status | `draft` |
| Source | Decision revision `10`, domain architecture revision `1` |

## Processing outcome

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Received` | Mulai proses | `InProgress` | Billing Integration | Identity/version valid | Tolak validation |
| `InProgress` | Semua komponen diterapkan | `Succeeded` | Billing Service | Rule/tariff valid | Masuk failure/review |
| `InProgress` | Komponen sebagian diterapkan | `PartialOutcome` | Billing Service | Komponen gagal terlihat | Jangan rollback komponen yang sudah applied |
| `InProgress` | Gagal sebelum efek | `FailedBeforeEffect` | Billing Integration | Error permanen sebelum mutation | Retry hanya sesuai policy |
| `InProgress` | Outcome tidak diketahui | `OutcomeUnknown` | Billing Integration | Timeout/response loss | Status query/reconciliation wajib |
| `OutcomeUnknown` | Verifikasi ulang | `Succeeded`/`PartialOutcome`/`PendingReconciliation` | Integration owner | Original identity dipakai | Tidak boleh membuat key baru |
| `Succeeded` | Replay sama | `Succeeded` | System | Fingerprint sama | Kembalikan hasil canonical |
| `Succeeded` | Versi lebih baru | `PendingReconciliation` | Billing Service | Newer fact supersedes prior | Jangan menimpa charge lama |

## Folio dan charge

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Open` | Charge/fact masuk | `ReviewRequired` atau `ReadyToClose` | Billing Service | Hasil kalkulasi valid | Tolak bila identity duplicate |
| `ReviewRequired` | Review selesai | `ReadyToClose` | Billing reviewer | Semua component resolved | Tetap review bila rule belum ada |
| `ReadyToClose` | Close | `Closed` | Billing + policy | Allocation, approval, reconciliation selesai | Tolak close |
| `Closed` | Reopen | `Open`/`ReviewRequired` | Authorized high-risk workflow | Approval valid dan histori dipertahankan | Tolak tanpa approval |
| `Recognized` | Correction | `Superseded` | Billing via approved action | New version dan reason | Original tetap immutable |
| `Recognized` | Void | `Voided` | Financial action executor | Approval/state valid | Tolak direct clinical cancel |
| `Recognized` | Reversal | `Reversed` | Financial action executor | Accounting consequence valid | Gunakan action baru, bukan overwrite |

Transisi lain yang tidak tercantum dianggap tidak sah. Status final target harus mempertahankan
makna ini walaupun nama enum hilir berubah.

