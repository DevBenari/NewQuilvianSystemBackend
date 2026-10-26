# MCU (Medical Check-Up) — Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `MCU-BP-001` |
| `revision` | `1` |
| `status` | `approved` |
| Modul | `mcu` — Medical Check-Up |
| Tanggal | 2026-10-20 (desain); 2026-10-21 (approval) |

## Owners

| Wewenang | Pemilik |
| --- | --- |
| Product/domain | Yoga Aji Pratama (`MCU-DEC-002`) |
| API | Yoga Aji Pratama; perubahan internal Registrasi/Lab/Rad/Billing oleh pemilik modul masing-masing |
| Security & privasi | Yoga Aji Pratama; bentuk bukti persetujuan pelepasan menunggu pemilik privasi/legal (`MCU-OQ-003`) |
| Frontend authority | Yoga Aji Pratama (`MCU-FE-001`..`005`, seluruhnya `approved`; `MCU-FE-002` lewat `05-screen-brief.md` 2026-10-21) |
| Billing (refund) | Yasmina, pemilik `billing-kasir` — perluasan refund disetujui 2026-10-21 (`MCU-COORD-003`) |
| Registry prefix | Muhammad Hamzah, pemilik registry — baris `Mcu` disetujui 2026-10-21 (`MCU-GOV-001`) |

## Approval

| Field | Nilai |
| --- | --- |
| `approved_by` | Yoga Aji Pratama |
| `approved_at` | 2026-10-21 |
| Catatan | Keputusan `MCU-DEC-001`..`043` `approved` (decision log r5). Blueprint dan kontrak `v1` disetujui bersama keputusan `MCU-DEC-041`..`043` dan persetujuan lintas modul yang tercatat di `MCU-EVD-002` |

## Snapshot source

| Field | Nilai |
| --- | --- |
| `backend_commit_sha` | `4d3e4f98aca0d9cbf6d5e0b62070d94d4760cfec` (branch `yoga`, working tree tidak bersih) — diverifikasi ulang 2026-10-21, tidak bergeser |
| `frontend_commit_sha` | `7fb1e1d3eb18df33a893cf2cb1a5cc7c6914acb9` (branch `YogaV2`, working tree tidak bersih) — diverifikasi ulang 2026-10-21, tidak bergeser |
| Referensi v1 | `7210f5f4db3a0c1f51446249e5c35a57eedb081c` |

## Contract versions

| Kontrak | Versi | Status |
| --- | --- | --- |
| `MCU-API-v1` — `contracts/api-contract.md` | `v1` | `approved` 2026-10-21 |
| `MCU-STATE-v1` — `contracts/state-transition-matrix.md` | `v1` | `approved` 2026-10-21 |
| `MCU-VAL-v1` — `contracts/validation-matrix.md` | `v1` | `approved` 2026-10-21 |
| `MCU-INT-v1` — `contracts/integration-contract.md` | `v1` | `approved` 2026-10-21; sisi Billing `INT-MCU-09` disetujui Yasmina 2026-10-21 |
| `MCU-PERM-v1` — `contracts/permission-audit-matrix.md` | `v1` | `approved` 2026-10-21 |
| PRD → MVP — `04-prd-to-mvp.md` | `v1` | `approved` 2026-10-21 |

## Input revisions

| Dokumen | Revision | Status | Hash (sha256, 12 karakter) |
| --- | --- | --- | --- |
| `evidence/2026-10-19-kebutuhan-mcu.md` (`MCU-EVD-001`) | — | Bukti mentah | `f58a5a45e8e8` |
| `evidence/2026-10-21-persetujuan-dan-pembuktian.md` (`MCU-EVD-002`) | — | Bukti persetujuan lintas modul dan pembuktian teknis | `3d24e89f6526` |
| `00-interview-decisions.md` | r5 | `approved` (`MCU-DEC-001`..`043`; `MCU-FE-002`) | `d235160129f5` |
| `01-existing-capability-map.md` | r1 | `draft` — dianotasi 2026-10-21 untuk `MCU-UNK-04` | `cd3ca704b088` |
| `02-requirement-completeness-assessment.md` (`MCU-RCG-001`) | r2 | `draft` — `PARTIALLY_READY` (S1–S8 siap, S9 tertahan) | `cc71647b69d7` |
| `03-domain-architecture.md` (`MCU-HDA-001`) | r1 | `draft` — `DOMAIN_ARCHITECTURE_READY` (S1–S8); dianotasi 2026-10-21 untuk `ARC-TECH-01` | `796752529840` |

## Artifact hashes

| Artefak | Hash (sha256, 12 karakter) |
| --- | --- |
| `02-backend-architecture.md` | `898c58948d09` |
| `03-frontend-architecture.md` | `8ffd47f5df5f` |
| `04-prd-to-mvp.md` | `1f833b743653` |
| `05-screen-brief.md` (`MCU-FE-002`) | `578c82681386` |
| `erd/00-context-erd.md` | `0d35d782c933` |
| `erd/katalog-mcu.md` | `dccbb2fe0678` |
| `erd/jadwal-mcu.md` | `f6b8e458c815` |
| `erd/layanan-mcu.md` | `61252ae89d92` |
| `erd/data-dictionary.md` | `84d4d626af23` |
| `contracts/api-contract.md` | `7eb37e3bcc49` |
| `contracts/state-transition-matrix.md` | `2872c64a25bf` |
| `contracts/validation-matrix.md` | `9b673f72e14a` |
| `contracts/integration-contract.md` | `27aa3e165023` |
| `contracts/permission-audit-matrix.md` | `deaf5e2858b7` |
| `testing/acceptance-test-matrix.md` | `e712eab55af9` |

Seluruh hash dihitung 2026-10-21 sesudah perubahan putaran 5 dan penyusunan roadmap. Hash manifest ini
sendiri tidak dicatat.

## Roadmap

| Roadmap | Revision | Status | Hash (sha256, 12 karakter) |
| --- | --- | --- | --- |
| `roadmap/backend-roadmap.md` (`MCU-RM-BE-001`) | r1 | `approved` — Yoga Aji Pratama, 2026-10-21 | `3f408b1baedf` |
| `roadmap/frontend-roadmap.md` (`MCU-RM-FE-001`) | r1 | `approved` — Yoga Aji Pratama, 2026-10-21 | `5f41bf9d49f5` |
| `roadmap/requirement-traceability.md` | r1 | `approved` — Yoga Aji Pratama, 2026-10-21 | `36ded6c404c5` |

Roadmap memuat 26 task backend (`BE-MCU-000` selesai) dan 16 task frontend (`FE-MCU-000` selesai).
Sejak persetujuan Billing dan brief layar 2026-10-21, **tidak ada task yang tertahan** keputusan;
urutannya hanya mengikuti dependency antar-task.

## Gerbang

### Ditutup 2026-10-21 (`MCU-EVD-002`)

| ID | Hal | Hasil |
| --- | --- | --- |
| `MCU-GOV-001` | Baris registry `Mcu` | Disetujui Muhammad Hamzah; baris di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; checker tidak lagi melaporkan QBE-MOD-002 |
| `MCU-COORD-002`, `008` | Persetujuan Lab/Rad | Disetujui pemilik `laboratorium` dan `radiologi` |
| Perluasan `EncounterIntakeService` | Persetujuan Registrasi | Disetujui pemilik Registrasi |
| `ARC-TECH-01` | Batal satu pemeriksaan Lab | Terbukti didukung `LabExaminationService.CancelAsync` |
| `MCU-UNK-04` | Refund nominal sebagian | Terbukti **tidak** didukung → `MCU-DEC-043` |
| `DES-MCU-001` | Dokter MCU untuk kiosk | `MCU-DEC-042` — dokter jaga harian |
| Aturan DES | 18 aturan `validation-matrix.md` | `MCU-DEC-041` — disahkan |
| `MCU-COORD-003` | Perluasan refund Billing + penutupan celah refund ganda | Disetujui Yasmina, pemilik `billing-kasir` |
| `MCU-FE-002` | Menu, route, tab/modal, tata letak | `05-screen-brief.md` — disetujui Yoga Aji Pratama |
| Roadmap r1 | Backend, frontend, traceability | Disetujui Yoga Aji Pratama |

### Masih terbuka

| ID | Hal | Memblokir |
| --- | --- | --- |
| `MCU-OQ-002`, `MCU-OQ-003` | Isi master klinis, bukti persetujuan | Go-live |
| `MCU-COORD-006` | Perubahan modul kiosk | `EPIC MCU-08` (`POST-MVP`) |
| `DEC-MCU-005` | Definisi laporan | `EPIC MCU-11` |

## Pemicu impact scan

Perubahan pada `PatientEncounterController`/`EncounterIntakeService`, `LabOrder`/`LabSpecimenService`,
`LabExaminationService.CancelAsync`, `RadOrder`/`RadStudyService`, `LabPaymentClearanceRules`,
`BillingInvoiceService.AddCatalogChargeAsync`, `BillingRefundService`, `MstTariff`, `MstCompanyGuarantor`,
registry prefix — atau SHA kedua repository bergeser dari snapshot di atas sebelum roadmap disusun.
