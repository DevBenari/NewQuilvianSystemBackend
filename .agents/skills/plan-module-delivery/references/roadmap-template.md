# Template Roadmap Delivery

## Metadata

```yaml
module_id: <id>
roadmap_revision: <revision>
status: DRAFT | APPROVED | SUPERSEDED
owners: []
approved_by: []
input_revisions: {}
artifact_hashes: {}
contract_versions: []
source_commits:
  backend: <sha>
  frontend: <sha>
```

## Task

| Field | Isi |
| --- | --- |
| Task ID | `BE-...` atau `FE-...` |
| Outcome | Hasil bisnis/pengguna |
| Trace | Requirement, decision, contract version |
| Reuse | Capability existing yang digunakan |
| Scope | Lokasi dan batas perubahan |
| Dependency | Task/sistem/keputusan pendahulu |
| Acceptance criteria | Pernyataan yang dapat diuji |
| Verification | Unit/integration/contract/E2E/manual evidence |
| Risk/blocker | Risiko, owner, resolusi |
| DoD | Kondisi penyelesaian lengkap |

## Urutan milestone yang disarankan

1. `B0/F0` — kontrak, permission, dan fondasi yang disetujui.
2. `B1/F1` — vertical slice minimum yang dapat dipakai.
3. `B2/F2` — variasi alur dan validasi bisnis.
4. `B3/F3` — integrasi lintas modul.
5. `B4/F4` — audit, observability, security, dan failure handling.
6. `B5/F5` — acceptance, regression, dan readiness.

Fase bukan status otomatis. Sebuah task hanya selesai bila seluruh acceptance criteria dan bukti verifikasinya tersedia.

## Traceability

| Requirement ID | Decision ID | Design/ERD | Contract version | Backend task | Frontend task | Test/evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `<REQ-ID>` | `<DEC-ID>` | `<path#section>` | `<version>` | `<BE-ID>` | `<FE-ID>` | `<test/path>` | Planned |

