# Insurance Management — Business Overview

## Batas yang terverifikasi

| ID | Type | Statement | Owner | Status | Evidence / approval |
| --- | --- | --- | --- | --- | --- |
| `INS-REQ-001` | Fact | Master provider, asuransi pasien, tarif asuransi, aturan coverage, dan sumber pembayaran encounter sudah ada. | Existing backend domains | Verified | `NewQuilvianSystemBackend/Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs#MstInsuranceProvider@cd6b7cfd34f79448445db5018a07040abead35a6`; `NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs#TrxPatientEncounterGuarantor@cd6b7cfd34f79448445db5018a07040abead35a6` |
| `INS-REQ-002` | Fact | UI master-data terkait dapat dijangkau dari App Router dan Redux frontend. | Existing frontend domains | Verified | `QuilvianSystemFrontendDev/src/app/administrator/master-data/insurance-provider/page.jsx#default@91df72cf05224c25c681f6f86b176c83e9610240`; `QuilvianSystemFrontendDev/src/lib/state/slice/health-services/master-data/master-data-insurance-coverage-rule-slice.jsx#masterDataInsuranceCoverageRuleSlice@91df72cf05224c25c681f6f86b176c83e9610240` |
| `INS-DEC-001` | Open decision | Definisikan outcome dan batas Insurance Management: administrasi master saja atau lifecycle operasi sampai klaim/collection. | Product/business owner | OPEN | Tidak disediakan pada tugas atau source. |
| `INS-DEC-002` | Open decision | Tetapkan source of truth dan retensi untuk eligibility, referral, Guarantee Letter, pre-authorization, dan claim document. | Insurance operations owner | OPEN | Tidak ada model lifecycle/kontrak external yang terbukti. |
| `INS-DEC-003` | Open decision | Tetapkan actor, permission, segregation of duties, approval, dan SLA per transisi. | Security + insurance operations owner | OPEN | Existing authorization master-data tidak menentukan operasi claim. |
| `INS-DEC-004` | Open decision | Tetapkan prioritas penjamin, co-pay/excess, multiple coverage, dan penanganan coverage tidak cukup. | Finance + insurance operations owner | OPEN | Provider memiliki flag excess; aturan lintas-transaksi belum disetujui. |
| `INS-DEC-005` | Open decision | Tetapkan provider eksternal yang masuk scope beserta API/file contract, retry, idempotency, dan rekonsiliasi. | Integration owner | OPEN | `IntegrationCode` adalah field master, bukan bukti integration contract. |
| `INS-DEC-006` | Open decision | Putuskan apakah modul HR insurance/benefit merupakan scope yang sama. | Product owner | OPEN | `WfpInsurance` berada di HR Payroll dan tidak boleh diambil alih tanpa keputusan. |

## Exclusions sampai ada keputusan

Tidak ada bukti yang mengesahkan claim submission, claim adjudication, invoice/collection, external eligibility lookup, GL document generation, atau automatic provider integration sebagai scope modul ini. Semua tetap di luar desain final.
