# Roadmap Delivery — Modul Operasi

| Field | Nilai |
|---|---|
| Blueprint | `operations`, revision 2, `approved` |
| Roadmap revision/status | `1` / `FORWARD-TEST` |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| API/state/integration | `opr-api-v1`, `opr-state-v1`, `opr-integration-v1` |
| Approval roadmap | Belum ada; setiap task memerlukan approval implementasi tersendiri |

## Input Roadmap yang Dikunci

| Input | Version/revision | SHA-256 |
|---|---|---|
| `blueprint-manifest.md` | revision 2 / `approved` | `DA3E3693DB0E143040DBF7EA6D1F123E98DA87949B7CDBDC43529BA45D4CB350` |
| `contracts/api-contract.md` | `opr-api-v1` / `approved` | `AC7D998C66D9274FB291A52CBBFEC5B575D1B40B4553D2C8B8D8CE0E9BC22176` |
| `contracts/state-transition-matrix.md` | `opr-state-v1` / `approved` | `A2401F00E89A8F88A7F8F8ECF056668C61ADBCFE570DD3237E2B3049F369BC37` |
| `contracts/integration-contract.md` | `opr-integration-v1` / `approved` | `61E9FEC2592135E780AD9294C8CF473451FFCF54BED3085AA6BA2A8FAEAC3058` |
| `contracts/validation-matrix.md` | `opr-validation-v1` / `approved` | `1A85F88C23B63E9F4F67576C865C31BDF84F8F41C5FE4B9B73B99D276433E311` |
| `contracts/permission-audit-matrix.md` | `opr-permission-v1` / `approved` | `CC3AD77CD8D5B24EE07DB46EA1CE08B535D1AF4205C204888FEEDF74C6FCA4DC` |

Urutan umum: backend foundation → case → schedule → preparation → execution → recovery/handover → integration/report → frontend per slice. Frontend selalu `BLOCKED BY` backend endpoint dan acceptance terkait.

Setiap task backend wajib menjalankan QBE preflight pada saat eksekusi berdasarkan `AGENTS.md`, `BACKEND_ENGINEERING_CONTRACT.md`, registry prefix, dan aturan task yang berlaku saat itu. Roadmap tidak memberi wewenang migration execution, database write, commit, push, atau deployment.

## Dependency Eksternal

| Dependency | Status | Dampak |
|---|---|---|
| Owner transaksi Billing | Belum tersedia lengkap | `BE-OPR-009` blocked untuk posting nyata |
| Owner item/implant dan mutasi stok | Belum terbukti tersedia | `BE-OPR-008/009` hanya dapat membangun ledger/adapter boundary |
| Resolver credential/privilege | Belum terbukti tersedia | Enforcement penuh pada `BE-OPR-004` blocked; validasi aktor aktif tetap dapat berjalan |
| Inpatient/ICU handover consumer | Belum tersedia | `BE-OPR-007` dapat menyimpan handover; integrasi consumer blocked |

Lihat `backend-roadmap.md`, `frontend-roadmap.md`, dan `requirement-traceability.md`.
