# Blueprint Manifest — Rekam Medis

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `QV-RM-001` |
| `revision` | `4` |
| `status` | `approved` |
| `scope` | Existing-first clinical foundation: reuse/extend provider klinis yang sudah tersedia; menu dan aggregate RM baru ditunda |
| `product_domain_owner` | Unit Rekam Medis |
| `api_owner` | Clinical Management untuk API existing; owner API RM baru belum ditetapkan |
| `security_owner` | Nadia Kirana Adiwijaya, S.H., M.H. — Development/UAT |
| `frontend_authority` | Product Owner/Unit Rekam Medis untuk alur; developer hanya `DEV_DISCRETION` pada detail visual |
| `approved_by` | Raka Pradipta Wicaksana, S.Tr.RMIK; dr. Bima Aditya Mahendra, Sp.PD, M.Kes; Nadia Kirana Adiwijaya, S.H., M.H. |
| `approved_at` | 21 Agustus 2026, 14:49 WIB |
| `last_approval_evidence_review_at` | 21 Agustus 2026 |
| `approval_scope` | `DEVELOPMENT / UAT` — delivery planning dan implementasi; bukan production release |
| `development_approval_status` | `APPROVED_VERIFIED` |
| `production_approval_status` | `NOT_APPROVED` |
| `approval_target_manifest_sha256` | `6EF08E4A21435FB43811F848B8A92B7CA572EAF5C7473DFB0B6A9D12E8A6C8D3` |
| `approval_package_sha256` | `BB05D5697505ED6B809A0C8F16426C42F4B3F37A2937AEDD6414F0165C5751B3` |
| `backend_commit_sha` | `5103e68eec5529540d369673c8a4e2651be0344b` |
| `frontend_commit_sha` | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| `api_contract_version` | `rm-existing-clinical-api-evidence-v0.2-draft` |
| `integration_contract_version` | `rm-existing-clinical-integration-v0.2-draft` |
| `state_contract_version` | `rm-existing-clinical-state-v0.2-draft` |
| `compatibility_impact` | Tidak mengubah source; provider existing dipertahankan, repair finality/contextual access/audit tetap wajib sebelum klaim kesiapan RM |

## Input

| Artefak | Revision/status | SHA-256 |
| --- | --- | --- |
| `00-interview-decisions.md` | Revision `1`, `draft` | `CE28BB1126799FFC54C4515ED313AC470A22DD9D0F1126086864599A08B0F210` |
| `01-existing-capability-map.md` | Revision `1` | `E16740282974D0820742E62C862B1A3F7CEA6BCE3449268667E17586925694C6` |
| `evidence/02-requirement-completeness-gate.md` | Revision `4`, `READY_FOR_DOMAIN_DESIGN` | `4C828F9FEE69BE3FF1D7F74BF69300B83A546223E75076B0A26957DDCD641B94` |
| `evidence/03-hospital-domain-architecture.md` | Revision `2`, ownership/reference `DOMAIN_ARCHITECTURE_READY` | `75B4FAE4DDA16B429217334C0BC4257EE0E3BA13386B9A0024B1D79744F11E8C` |
| `evidence/04-hospital-domain-architecture-full.md` | Revision `1`, full module `DOMAIN_ARCHITECTURE_READY` untuk desain draft | `2BF5AEDB6ED5C0A101466052AB45617855E0E6BF86067CA0EA11BAA257364740` |

## Bukti Approval yang Diterima

| Artefak | SHA-256 file | Hasil review |
| --- | --- | --- |
| `Approval-1-Unit-Rekam-Medis.pdf` | `30B566ED16BC01E5B66327219C22AD74B4D837A359ED473FDDC4B05F7C5314D3` | `RECEIVED_NOT_VALID_APPROVAL` |
| `Approval-2-Komite-Medis-Direktur-Pelayanan-Medis.pdf` | `DBF1CD5ADB751017ED4DE76B7EE7212EE2859321ABFD7935DD052B94C71F4BF6` | `RECEIVED_NOT_VALID_APPROVAL` |
| `Approval-3-Privasi-Hukum-Release-Informasi.pdf` | `D42B544F89998309C0A7A770607007926090018875697E8409D82EC06E4DB298` | `RECEIVED_NOT_VALID_APPROVAL` |
| `Approval-1-Unit-Rekam-Medis-Lengkap.pdf` | `3567B4A99E48FA1782AB47C8D4C06347F9366F701FE1DE195688A5198CC4DC7F` | `SIMULATION_ONLY_NOT_APPROVAL` |
| `Approval-2-Komite-Medis-Direktur-Pelayanan-Medis-Lengkap.pdf` | `B06F0069CA69AC0C78B508EC98D6F4177D630B8F54C9446564074459B4E10B55` | `SIMULATION_ONLY_NOT_APPROVAL` |
| `Approval-3-Privasi-Hukum-Release-Informasi-Lengkap.pdf` | `B4A0621999BAD81B10B1CB351D0AAA05B87BA7A4A9831B2EF04EDBC1A0E8395B` | `SIMULATION_ONLY_NOT_APPROVAL` |
| `Approval-1-Unit-Rekam-Medis-APPROVED-Development.pdf` | `E850FD0B2EE078C4D47CC0EC40F3E59A080BEB04F971C3189C2637EDE31C9047` | `PENDING_ARTIFACT_AND_SIGNATURE_VERIFICATION` |
| `Approval-2-Klinis-APPROVED-Development.pdf` | `6E2C6302890B99A59DCB35FC9FB9933FB6268D54739529103F04909012AE0F91` | `PENDING_ARTIFACT_AND_SIGNATURE_VERIFICATION` |
| `Approval-3-Privasi-Hukum-APPROVED-Development.pdf` | `08856E38A1AB2140E8AB16CC1DAF7D99EBD71D8B4BD1C706FC975EEEB3C4D750` | `PENDING_ARTIFACT_AND_SIGNATURE_VERIFICATION` |
| `3-Dokumen-Approval-SIMRS-APPROVED-Verified.zip` | `BB05D5697505ED6B809A0C8F16426C42F4B3F37A2937AEDD6414F0165C5751B3` | `APPROVED_VERIFIED` untuk Development/UAT |

Submission keempat mengikat 17 artefak canonical melalui approval target manifest, memiliki tiga
signature PDF yang valid dan trusted terhadap root CA paket, serta menyetujui delivery planning dan
implementasi khusus Development/UAT. Detail pemeriksaan ada di
`evidence/05-approval-evidence-review.md`.

## Artifact Hashes

| Artefak | SHA-256 |
| --- | --- |
| `02-backend-architecture.md` | `75B98099F5E8E59E95A1A72657144A8CA4434D1B629B9A461E151AC3CB040838` |
| `03-frontend-architecture.md` | `896275C7FDFCB08D794BE133A6EFFA1CA5A404548E98A33E4B63A4C120341B6B` |
| `erd/00-context-erd.md` | `3F31A6964C872164E74A4C99C6C1ECCF3FFFE414538754F896F59E847C8E2DCB` |
| `erd/ownership-reference.md` | `D4F50AB20DEAFAC1FDCCDF1EABF9E5F474EAB8FA4EB9E041198AB6430F3FB7A2` |
| `erd/existing-clinical-foundation.md` | `E08A11F0B257022849378878A93CBFCE8E6D4E12BB0B4E4FA8DA9523F39BF2D2` |
| `erd/data-dictionary.md` | `A359A8B74F1882B5B7052F582B831385C3B0FD62B819C51FD5EF875B510CB7E2` |
| `contracts/api-contract.md` | `C30C05960BE6B3165EAEDD1901166EE84140363C430574B67668CF0842829D88` |
| `contracts/state-transition-matrix.md` | `B78104870F555374E37AED3AE4A077C7E2A48F2BBACED027A61AA4033B2018C2` |
| `contracts/validation-matrix.md` | `70D91723D6422F474D135888FA43EABE5612EB38194E4ADC296AF991C74A81B9` |
| `contracts/integration-contract.md` | `C74337CA4223091782F20F0EE5B8968F0D7EF411249B0326378BF61C28D63B87` |
| `contracts/permission-audit-matrix.md` | `1D1510BD2A68FEB6A8C49C0EE633332A8FC81313785FC60C2C7AF428EA0C135E` |
| `testing/acceptance-test-matrix.md` | `4E20EB8AFAB82D2F602C9F054F012C2B5F655E32EBAEB45B337750193D9AB06F` |
| `evidence/05-approval-evidence-review.md` | `766209F0E0B049120BB7D4D892465D5008A051DDCE462A34019FDED380ECCF3E` |

Perubahan isi setelah hash dicatat harus menaikkan revision atau memperbarui manifest sebelum
approval.

## Batas Approval

`RM-APR-002` ditutup hanya untuk delivery planning dan implementasi pada Development/UAT. Approval
operasional, klinis, serta privacy/legal terikat pada 17 artefak canonical revision `4` melalui
target manifest SHA-256 `6EF08E...C8D3`.

Production release, production activation, deployment produksi, dan sign-off organisasi belum
disetujui. Break-glass, release, retention deletion, serta policy berisiko hanya boleh tersedia pada
Development/UAT sesuai guardrail fail-closed dan acceptance test; fitur tersebut tidak boleh
diaktifkan di production berdasarkan approval ini. Perubahan material terhadap 17 artefak target
memerlukan impact review dan approval baru.
