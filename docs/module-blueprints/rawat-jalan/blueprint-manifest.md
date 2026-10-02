# Rawat Jalan — Blueprint Manifest

## Scope kepemilikan

Blueprint ini adalah umbrella end-to-end Rawat Jalan dengan **dua scope kepemilikan terpisah**.
Keduanya punya roadmap, progress, dan Definition of Done sendiri, dan **tidak boleh dijumlahkan**.

| Scope | Prefix | Roadmap | Status | Roadmap revision | Batas |
|---|---|---|---|---|---|
| Doctor / Clinical | `RJ-DOC` | [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md) | `PARTIAL` · roadmap **`OWNER_APPROVED`** · kontrak **`FROZEN`** | `3` — `CURRENT STATE` per `2026-08-31` | Berakhir pada `Selesai Konsultasi` yang authoritative, aman, idempotent, ter-audit, dengan durable producer handoff untuk setiap **eligible** clinical milestone |
| Billing / Revenue Cycle | `RJ-BIL` | [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md), [roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md) | `PARTIAL — NEEDS REVERIFICATION` | `1` — `HISTORICAL SNAPSHOT` per `2026-08-24`/`28` | Dimulai sebagai **consumer** clinical fact. `DOWNSTREAM — NOT PART OF DOCTOR DEFINITION OF DONE` |
| **Rawat Jalan → Invoice V2** | `RJ-E2E` (dokumen: `RJE`) | [roadmap/e2e-backend-roadmap.md](roadmap/e2e-backend-roadmap.md), [roadmap/e2e-frontend-roadmap.md](roadmap/e2e-frontend-roadmap.md) | **Desain `APPROVED` · roadmap `DRAFT`** | `1` | Jembatan fakta klinis → `BilInvoice`, obat dua tahap, jasa konsultasi, rekonsiliasi, Ringkasan Billing. Lihat bagian *Revisi 27* di bawah |

## Revisi 28 — Amendment DP: Daftar Pasien Rawat Jalan (`2026-10-02`)

| Field | Nilai |
|---|---|
| `revision` | `28` |
| `status` | **`approved`** — desain dan kontrak disetujui (`RJ-DOC-DEC-024`) |
| Scope | `RJ-DOC` (revisi roadmap `RJ-DOC-REV-*`). Layar daftar kunjungan RJ bercakupan, pembatalan kunjungan menggantung, dan pelonggaran pemblokir pendaftaran |
| `blueprint_shape` | `SINGLE` (`RJ-DOC-DEC-011`, `USER_CONFIRMED`) |
| `backend_commit_sha` / `frontend_commit_sha` | `245f0464` / `b7e9b7fd4` |
| `contract_versions` | **`RJ-DOC-ENCLIST-001@1.0.0 (approved)`** — bagian *Amendment DP* pada `api-contract`, `state-transition-matrix`, `validation-matrix`, `integration-contract`, `permission-audit-matrix`. Kontrak lain tidak berubah |
| `decision_revision` | `RJ-DOC-DEC-011`..`023`, `RJ-DOC-FE-005`..`009` |
| `requirement_readiness` | `GATE_NOT_RUN` — dilewati atas persetujuan pemilik (scope kecil, keputusan tertutup berbukti) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — tanpa bounded context, master, atau dampak billing baru |
| Capability | [01-capability-impact-scan-daftar-pasien-rj.md](01-capability-impact-scan-daftar-pasien-rj.md) |
| Migration | Tidak ada |
| `owners` | Product/Domain, API, Security/Privacy, Frontend authority: Sukma Giri |
| `approved_by`, `approved_at` | Sukma Giri, `2026-10-02` ("lanjutkan" atas pertanyaan "Setujui sekarang dan lanjut?") |
| `implementation_authority` | `NOT_GRANTED` — diberikan per task |
| Roadmap | [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md) bagian 11 — `RJ-DOC-REV-BE-008`..`010` (`MVP-0`), `RJ-DOC-REV-FE-010`..`011` (`MVP-1`) |
| Implementasi | ✅ `RJ-DOC-REV-BE-008`..`010`, `RJ-DOC-REV-FE-010`..`011` selesai `2026-10-02` (`RJ-DOC-DEC-025`); belum di-commit |
| Langkah berikutnya | Keputusan pemilik atas akun uji `UJI-RJDP` dan hak jabatan uji; commit oleh pemilik; opsional `verify-module-readiness` |

| Artifact | SHA-256 | Keadaan |
|---|---|---|
| `00-interview-decisions.md` | `1D295036A86AE525A3A4918FF782FB6CE4FE6CEFF58926FB7952EF27C3F3560D` | `CHANGED` — Amendment Pass + Closure 2026-10-02 |
| `01-capability-impact-scan-daftar-pasien-rj.md` | `734E603A45F533330851002B3377DDA08189E6319615E12E0F7DFDD22D4C0E60` | `NEW` |
| `02-backend-architecture.md` | `1E507D7574153B71F207BF9868EC0179BB27D9713723118EA0D876F351B76328` | `CHANGED` — Amendment DP |
| `03-frontend-architecture.md` | `B3B41C2164A795FB49CBF6F0A135B0612F33588F773CDBCACD1EC81C3B0A2E3F` | `CHANGED` — Amendment DP |
| `04-prd-to-mvp.md` | `FC6BED21E07F3BCD6020B7BCA5B6621CB9DEDB35B4BD6E8D81F74E424026C2F6` | `CHANGED` — Amendment DP |
| `flowcharts/daftar-pasien-rawat-jalan.md` | `A9A6D137A860250E6026587B348127B8AB023AD362D4B1DDD25985372AE0FB8E` | `NEW` |
| `data/data-dictionary.md` | `3A9069C1FFF1C5CD19821F8A157C9AED940CE5C273FA737097EB81FC3E36A785` | `CHANGED` — kolom kunci saja |
| `contracts/api-contract.md` | `BD8AE89DABAE8C8847A2C83964CBAAD7BADDB2D75480C4A5E977474BD21D51DB` | `CHANGED` |
| `contracts/state-transition-matrix.md` | `7C355B329B6DC7F5D6BE8DAFA1E34557ABC43CC31481BD7B9C756D80665B8EBE` | `CHANGED` |
| `contracts/validation-matrix.md` | `60CCDD231E7AE1C1BDE3F2605C1D8D7BAE1E2EAE83DB68A56BE4C986DE80DB73` | `CHANGED` |
| `contracts/integration-contract.md` | `AE6EC521930E1C1FF427F6A86DB237BEBE501A576DF0848609D633D0C49B49ED` | `CHANGED` — satu paragraf "tidak berlaku" |
| `contracts/permission-audit-matrix.md` | `8CDCAD7C17F78C500200F9C9B9BE87B9A5A27AEFDF245A88760B87E2E8FFB0E9` | `CHANGED` |
| `testing/acceptance-test-matrix.md` | `A6326CD3BFF8F3D5E893D390DC6A1FBACB6A77CC942EFB7597CA1C8B2D2DD26F` | `CHANGED` |

## Revisi 27 — Amendment V2 (`2026-09-28`)

| Field | Nilai |
|---|---|
| `revision` | `27` |
| `status` | `approved` — desain V2 dan `RJ-E2E-CONTRACT-001@1.0.0` (`RJ-E2E-DEC-015`) |
| `blueprint_shape` | `SINGLE` |
| `shape_decided_by` | `USER_CONFIRMED` (`RJ-E2E-DEC-000`) |
| `backend_commit_sha` | `063d38bc306bb6b46bdf088513fa6cdcc80399d8` (`sukmagp`) |
| `frontend_commit_sha` | `83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6` (`sukmagpV2`) |
| `contract_versions` | **`RJ-E2E-CONTRACT-001@1.0.0 (approved)`** · **`RJ-E2E-CONTRACT-001@1.0.1 (draft)`** — tambahan aditif `BillingHandoffIssues` pada `POST /doctor-queues/{id}/finish-consultation` (lihat `api-contract.md` bagian V2.1) — satu set: `api-contract`, `state-transition-matrix`, `validation-matrix`, `integration-contract`, `permission-audit-matrix` bagian V2. Kontrak lama tetap: `RJ-BIL-CONTRACT-001@1.0.0`, `RJ-DOC-COMPLETION-001@1.0.0 (FROZEN)`, `RJ-DOC-HANDOFF-001@1.0.0 (FROZEN)` |
| `decision_revision` | `18` — `RJ-E2E-DEC-000`..`014` |
| `requirement_readiness` | Slice *clinical fact handoff* dan *billing folio* `READY_FOR_DOMAIN_DESIGN` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; slice V2 di core internal yang siap independen |
| `owners` | Product/Domain: Sukma Giri · Billing/Revenue Cycle · API authority · Security/Privacy · Frontend authority |
| `approved_by`, `approved_at` | Sukma Giri, `2026-09-28` |
| Capability map yang berlaku | [01-existing-capability-map-prd-v2.md](01-existing-capability-map-prd-v2.md). Berkas canonical `01-existing-capability-map.md` tetap sebagai riwayat revisi sebelumnya |
| Kamus data yang berlaku | [data/data-dictionary.md](data/data-dictionary.md) (lokasi canonical baru); `erd/` tetap sebagai riwayat |
| Verifikasi | Pola Bank Darah — **tanpa** project/folder test (`02-backend-architecture.md` bagian `V2.13`) |
| Roadmap V2 | [roadmap/e2e-backend-roadmap.md](roadmap/e2e-backend-roadmap.md) (14 task), [roadmap/e2e-frontend-roadmap.md](roadmap/e2e-frontend-roadmap.md) (3 task), [roadmap/e2e-requirement-traceability.md](roadmap/e2e-requirement-traceability.md) — revisi `1`, `DRAFT` |
| `implementation_authority` / `builder_execution` | `NOT_GRANTED` / `NOT_AUTHORIZED` — diberikan per task |
| Langkah berikutnya | Approval roadmap V2 dan kontrak `1.0.1`; lalu wewenang per task (mulai gelombang 1: `BE-RJE-001`, `BE-RJE-004`, `BE-RJE-006`) |

### Masukan

| Masukan | SHA-256 |
|---|---|
| `PRD-Rawat-Jalan.md` (`PRD-RJ-BIL-V2-001`, belum di-commit) | `39C60B262E6B38B4B6DF6F5B33AA4C013E8E0B6BC4CE715E721F7A5BEFBF59D2` |
| `00-interview-decisions.md` revisi `18` | `2520893E36039CD0CA6DA3AE45CE302C7C78A60113E7DCACB51FB0D1C117CA84` |
| `01-existing-capability-map-prd-v2.md` | `9ED01454F34A665787ED805A872C4DD77FE421CFEE59EC16AA2F6BA85B7F6615` |

### Artifact hashes revisi 27

| Artifact | SHA-256 | Keadaan |
|---|---|---|
| `02-backend-architecture.md` | `343E895FD6450D5C543DD3DDC6595323F7666D74DDDFC3FE7909556C60D95BFE` | `CHANGED` — amendment V2 |
| `03-frontend-architecture.md` | `91DFDA451773F1BDBC96F9D8231D229C32B385116DCA830760251F5CF722E2AC` | `CHANGED` — amendment V2 |
| `04-prd-to-mvp.md` | `1A59492EF66D5ADE41014597A62388389C9F0D622E5110EA97BB46F30F7F2B5E` | `NEW` |
| `data/data-dictionary.md` | `9C3C2AB22AEEC994EE402C2BE3050C0236E44FDB01156E2EB13A937AFBED9782` | `NEW` |
| `flowcharts/00-alur-utama.md` | `4F859D64908E18B917E8EE8C1F6D1943C1D1338DA9FE16D76839CFA29E3CC9EE` | `NEW` |
| `flowcharts/penagihan-otomatis.md` | `DAA041CB6E0F41286DE7277F9B99541A8438A8C7D73CE370F10B05D499FF8C33` | `NEW` |
| `flowcharts/obat-dua-tahap.md` | `CFAF927B8AAEC7C1A09F2A8B64F6ACED82ACE6E78F7EA06174214D848E09909E` | `NEW` |
| `flowcharts/pembatalan-dan-koreksi.md` | `6541BBCD98935ABF5EB9EF19CCCFAA1B1325F11B9F082B51CE955082A6D84B6C` | `NEW` |
| `flowcharts/rekonsiliasi.md` | `80CE649CD1987B244AFC14CB69B8549AE6B7688F90F82C12DCB7A9AC959EFD01` | `NEW` |
| `contracts/api-contract.md` | `704EA21583734C136EF5DFE573A5C7FDE921B12D6D4BFC9DAABF7C9F28AFC5C2` | `CHANGED` — V2 |
| `contracts/state-transition-matrix.md` | `B71EE027F85061086F05FF4CEDBA8E9A02F93FD4D29D58213B3078B03E6EBE6D` | `CHANGED` — V2 |
| `contracts/validation-matrix.md` | `0E83DD96B93945422BD52DD0F45DCE45128F7F14ECB43456B1A86CA345EDDD8D` | `CHANGED` — V2 |
| `contracts/integration-contract.md` | `67AB06BF27765F167BF886048F852FD3F2F275B28BA3EA2E6067AE233AA45831` | `CHANGED` — V2 |
| `contracts/permission-audit-matrix.md` | `E54D31079F65232421EA767DFC8888CBAEBC1D9CDA786213EFA77A91527C6EB0` | `CHANGED` — V2 |
| `testing/acceptance-test-matrix.md` | `E6966796EAA0F137AC477DC1A56B7E1E35F27B6F3237746D7720011346975629` | `CHANGED` — V2 |

Hash dihitung `2026-09-28` setelah seluruh artefak V2 selesai ditulis. Hitung ulang saat approval
untuk memastikan tidak ada perubahan di antaranya.

Metadata di bawah adalah metadata scope **Billing**; ia dipertahankan apa adanya. Metadata scope
Dokter ada di kepala roadmap-nya sendiri.

| Field | Value |
|---|---|
| `blueprint_id` | `RJ-BIL-BP-001` |
| `module_name` | Rawat Jalan (umbrella); scope Billing bernama Dokter / Rawat Jalan Billing |
| `module_slug` | `rawat-jalan` |
| `module_prefix` | `RJ-BIL` untuk scope Billing; `RJ-DOC` untuk scope Doctor/Clinical |
| `revision` | `26` — **dilanjutkan revisi `27` (amendment V2) di bagian atas**; baris ini metadata scope Billing lama |
| `status` | `PARTIAL` |
| `current_phase` | `RJ-BIL-PH-008` — Delivery Planning |
| `created_at` | `2026-08-20T15:06:30+07:00` |
| `updated_at` | `2026-08-28T14:02:11+07:00` |
| `last_verified_at` | `2026-08-28T14:02:11+07:00` |
| `backend_source_sha` | `6b25e6049e60e055593968abe463262b59842527` cabang `sukmagp`; working tree `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, `RJ-BIL-BE-007`, dan remediasi penamaan QBE belum di-commit |
| `frontend_source_sha` | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| `skill_suite_version` | `1.0.0-rc2` |
| `input_revision_hash` | `decisions:sha256:18D9B0CFEF4EDA1F22170FC8EA7FCDC84A317AC8B1C7B907F6BA8B4B4435F86D; capability:sha256:D1CB1D052474FA96F0BE801F7CEA277AEB0604A9969247B7323F82D23F5152B7` |
| `decision_revision` | `15` — ditambah amendment `RJ-DOC-DEC-001` s.d. `RJ-DOC-DEC-006` pada `2026-08-31` |
| `contract_versions` | `RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)`; `RJ-DOC-COMPLETION-001@1.0.0 (FROZEN)`; `RJ-DOC-HANDOFF-001@1.0.0 (FROZEN)` |
| `active_dependency_ids` | `RJ-BIL-DEP-001` s.d. `RJ-BIL-DEP-009` |
| `active_roadmap_revision` | `1` |
| `supersedes` | `null` |
| `domain_architecture_revision` | `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; core internal/manual siap independen |
| `owners` | Product/Domain owner; Billing/Revenue Cycle; API authority; Security/Privacy; Frontend authority |
| `approved_by` | `User-provided approval authority` |
| `approved_at` | `2026-08-21` |

## Artifact hashes

Hash artefak target dihitung pada `2026-08-20` untuk mendeteksi drift. Semua artefak desain masih
berstatus `draft` dan belum menjadi izin implementasi.

Hash dihitung ulang pada `2026-08-31` untuk revisi `26`. Kolom **Keadaan** memisahkan artefak yang
berubah dari yang diverifikasi ulang dan cocok. Tidak satu pun hash disalin dari revisi sebelumnya.

| Artifact | SHA-256 | Keadaan |
|---|---|---|
| `02-backend-architecture.md` | `3F57EE136C11F6971272D9FDC6E8E308CF8339F14FE1CF5D599EAB82A4D11D92` | `CHANGED` — banner ownership boundary |
| `03-frontend-architecture.md` | `E9CDEA62FEB995787DE940CEC4D0FBBAFB90E4A8D064C8C36D0CCDD8F757E3A9` | `CHANGED` — banner ownership boundary |
| `hospital-domain-architecture.md` | `E7B0B0F08DB9CFB2B7FEF6727F705FA09ED0720CC6FEE32F7EE0C1DD8EADF96E` | `VERIFIED — MATCH` |
| `contracts/api-contract.md` | `9508986836A537DD88A664F052003AE1A8509EE0555DD6DC2F3F83AB6B871FD6` | `VERIFIED — MATCH` |
| `contracts/state-transition-matrix.md` | `1688209BFEEAFA3F5A48A70376FCE4CA291239F189343FFF615D05C2C39B5807` | `VERIFIED — MATCH` |
| `contracts/validation-matrix.md` | `8C3FF0A302BAB10BC6DC904630F3FE0BD2DA675F1DE2491586F08CF596209E53` | `VERIFIED — MATCH` |
| `contracts/integration-contract.md` | `2B3162454CD88086F587EAB880281D43076D1BF95F7B2EB39BAA9BB17A8DBB87` | `CHANGED` — sisi producer dibekukan; ringkasan kewajiban consumer |
| `contracts/permission-audit-matrix.md` | `1424ADE6BB5084C8A77105477C65B347959C6C17570A88E2E9663C38C23FF093` | `VERIFIED — MATCH` |
| `testing/acceptance-test-matrix.md` | `FEC2E3816EF086540FB85EAB9242A559906BF65154B1DC311068A5C999840EF6` | `VERIFIED — MATCH` |
| `owner-review-checklist.md` | `review artifact; hash dihitung setelah owner mengisi record approval` | tidak di-hash |
| `roadmap/backend-roadmap.md` | `B4713CB2BD0FB626115574233E473D4E4DC536E3BCAC632682F3D8B743EF7CB1` | `CHANGED` — label `DOWNSTREAM` dan `HISTORICAL SNAPSHOT` |
| `roadmap/frontend-roadmap.md` | `2B8A06A76E32DE9F4C472E06B4FE14BA74E4B8D2CE7601CDB0E74E8899E40963` | `CHANGED` — label `DOWNSTREAM`, `HISTORICAL SNAPSHOT`, perbaikan tiga kontradiksi internal |
| `roadmap/requirement-traceability.md` | `74248D5F1C08AA2B8FB1174178E4C30DDB3C6B8405C85303F02C94A4283E6DDA` | `CHANGED` — `RJ-DOC-BE-002` selesai; `CAP-014` tertutup |
| `roadmap/doctor-consultation-roadmap.md` | `02FD9B63995AB19F48321210D50BE6B8FB3AF47BBD5798CE2DE14AA52B7E8FD4` | `CHANGED` — revision `5`; `RJ-DOC-BE-002` ✅ `COMPLETE` |
| `MODULE-STATUS.md` | `5BBF56E434910A7B99091AF6B3BDA221D2AF1E1A46BBD5F0778261FC70E4848E` | `CHANGED` — capability `MANDATORY` `19/7/2` sesudah `RJ-DOC-BE-002` |
| `contracts/doctor-consultation-contracts.md` | `E7F22D785913BB148FC31AA450D77F13E74385FA84348DC5AEE08710ED8BCAE9` | **`NEW / FROZEN`** — `RJ-DOC-COMPLETION-001@1.0.0` dan `RJ-DOC-HANDOFF-001@1.0.0` |
| `00-interview-decisions.md` | `C5AD4F3B1D7F5B760FE1F7EB41392997754CF9A0F5BD9C6609AB03BBB5C82D8C` | `CHANGED` — amendment `RJ-DOC-DEC-001` s.d. `RJ-DOC-DEC-006`; kini ikut di-hash |
| `task/report/backend/RJ-DOC-BE-001.md` | `1CB1C91056E3C19615797D38463F7CD6A90A75EA6A5EE9920F511851438BF83A` | **`NEW`** — laporan task tracked `RJ-DOC-BE-001` |
| `task/report/backend/RJ-DOC-BE-002.md` | `7A71452E1AE8A0B5D69AFA164C53CF4642AF4DF72F5C51BD8922DC5FE78DD139` | **`NEW`** — laporan task tracked `RJ-DOC-BE-002` |

Enam artefak `VERIFIED — MATCH` cocok persis dengan hash revisi `21`, sehingga tidak ada drift
tersembunyi pada artefak desain dan kontrak yang tidak disentuh.

Catatan `2026-08-31` (revision `25`) — **`RJ-DOC-BE-001` selesai.**

Task implementasi pertama scope Dokter dikerjakan di bawah wewenang owner yang dibatasi pada
`RJ-DOC-BE-001` saja. Jalur `POST /doctor-queues/{id}/finish-consultation` berhenti memiliki
logika penyelesaian klinis sendiri dan menjadi lapisan orkestrasi di atas
`ConsultationFinalizationService`; `CompleteImmediately=true` ditutup untuk pembuatan konsultasi
berantrean; dan kunjungan berhenti di `ConsultationCompleted` alih-alih melompat ke `Completed`.

Bukti: build solution `0 error`; `141` uji lulus `0` gagal, termasuk `9` uji acceptance baru pada
`Tests/QuilvianSystemBackend.Tests/ClinicalManagement/DoctorConsultationCompletionTests.cs`.
Laporan tracked: [task/report/backend/RJ-DOC-BE-001.md](task/report/backend/RJ-DOC-BE-001.md).

Kontrak `RJ-DOC-COMPLETION-001@1.0.0` **tidak diubah** dan tetap `FROZEN`; hash-nya tidak berubah.
Tidak ada migration dibuat maupun diterapkan, dan tidak ada basis data yang dimutasi.
`IMPLEMENTATION_AUTHORITY` untuk `RJ-DOC-BE-002` dan seterusnya tetap `NOT_GRANTED`.

Catatan `2026-08-31` (revision `26`) — **`RJ-DOC-BE-002` selesai.**

Validasi finalisasi kini mengikat pada kedua permukaan penyelesaian, dan ditambah tiga pemeriksaan
keutuhan pesanan klinis sesuai kontrak bagian `1.6` — seluruhnya memakai state yang sudah ada,
tanpa status baru dan tanpa query tambahan: `INCONSISTENT_PROCEDURE_STATUS`,
`PROCEDURE_ENCOUNTER_MISMATCH`, dan `PRESCRIPTION_ENCOUNTER_MISMATCH`. Yang terakhir mencegah fakta
klinis mendarat pada kunjungan pasien yang salah.

Batas `RJ-DOC-DEC-004` terbukti: pesanan penunjang yang sudah tersimpan tetapi belum dikerjakan
**tidak** menahan penyelesaian konsultasi, dan ketiadaan pesanan penunjang juga tidak.

Bukti: build solution `0 error`; `155` uji lulus `0` gagal, termasuk `14` uji acceptance baru pada
`Tests/QuilvianSystemBackend.Tests/ClinicalManagement/DoctorConsultationValidationTests.cs`.
Laporan tracked: [task/report/backend/RJ-DOC-BE-002.md](task/report/backend/RJ-DOC-BE-002.md).

Kontrak `RJ-DOC-COMPLETION-001@1.0.0` **tidak diubah** dan tetap `FROZEN`; hash-nya
`E7F22D78…` diverifikasi ulang dan cocok. Tidak ada migration dibuat maupun diterapkan.
`IMPLEMENTATION_AUTHORITY` untuk `RJ-DOC-BE-003` dan seterusnya tetap `NOT_GRANTED`.

Catatan `2026-08-31` (revision `24`) — **owner approval dan contract freeze scope Dokter.**

Roadmap `RJ-DOC` dinaikkan dari `READY_FOR_OWNER_APPROVAL` menjadi **`OWNER_APPROVED`**
(`RJ-DOC-DEC-001`, Sukma Giri), dan kedua contract gate `P0` ditutup:

| Gate | Kontrak | Status |
|---|---|---|
| `RJ-DOC-INT-001` | `RJ-DOC-COMPLETION-001@1.0.0` | **`COMPLETE / FROZEN`** |
| `RJ-DOC-INT-002` | `RJ-DOC-HANDOFF-001@1.0.0` | **`COMPLETE / FROZEN`** |

Empat open question ditutup keputusan owner: `RJ-DOC-DEC-002` menempatkan Lab dan Radiologi sebagai
`CONDITIONAL`; `RJ-DOC-DEC-003` melarang reopen generik; `RJ-DOC-DEC-004` memisahkan *doctor order
creation* dari *ancillary execution*; `RJ-DOC-DEC-005` membatasi `CompleteImmediately` beserta tiga
compatibility requirement. **Tidak ada open question tersisa** pada scope Dokter.

Yang **tidak** diberikan revisi ini: `IMPLEMENTATION_AUTHORITY` tetap `NOT_GRANTED` dan
`BUILDER_EXECUTION` tetap `NOT_AUTHORIZED`. `RJ-DOC-BE-001`, `BE-002`, `BE-003`, dan `BE-005`
menjadi `ELIGIBLE` — dependency kontraknya terpenuhi — tetapi **belum boleh dikerjakan**.

Scope Billing **tidak disentuh** pada revisi ini selain rujukan silang; tidak ada task `RJ-BIL`
yang diubah, ditambah, atau dinilai ulang.

Catatan koreksi `2026-08-31` (revision `23`) — **scope hardening menjelang owner approval**.

Revisi ini menutup delapan kontradiksi lintas dokumen yang tersisa setelah revisi `22`, dan
menaikkan roadmap Dokter ke `roadmap_revision: 2` berstatus `READY_FOR_OWNER_APPROVAL`.

| # | Kontradiksi | Penyelesaian |
|---|---|---|
| `1` | `CAP-015` disebut *"satu-satunya butir yang menahan kedua DoD"*, padahal `CAP-023` juga bertanda `YES` di kedua kolom | Terminologi dipertegas. Tidak ada butir Dokter yang `Blocks Billing DoD`; yang ada adalah **downstream readiness dependency** milik `CAP-015`, `CAP-023`, dan `CAP-030` |
| `2` | Radiologi *"belum ada"* vs source `17` berkas | Diklasifikasikan `SOURCE EXISTS — ROADMAP TASK STATUS NEEDS REVERIFICATION`. **Tidak** ditandai `COMPLETE`; keberadaan folder bukan bukti acceptance |
| `3` | Backend Billing `3 dari 9` vs `5 dari 9` | Keduanya dilabeli `HISTORICAL SNAPSHOT` dengan tanggal observasi masing-masing. Ditambah temuan bahwa source `BE-006` dan `BE-007` **tidak ada** pada `HEAD` |
| `4` | SHA frontend `ab4bd836` vs `HEAD` `baca965`; klaim `88` test | Roadmap frontend dilabeli `HISTORICAL SNAPSHOT`; berkas test yang dirujuk tidak ditemukan, layarnya ada |
| `5` | `CAP-008` `COMPLETE` di roadmap vs `PARTIAL` di traceability | Dipecah menjadi `CAP-008A` pembuatan (`COMPLETE`) dan `CAP-008B` finalisasi (`PARTIAL`). Satu capability, satu makna |
| `6` | `CAP-024` dihitung sebagai implementation capability padahal ia invariant | Dipindahkan menjadi `RJ-DOC-INV-001`; ID `CAP-024` dipensiunkan. Registry kini punya empat kelas dan denominator hanya diambil dari `MANDATORY` |
| `7` | `BE-005` durable handoff `P1` padahal `END OF DOCTOR SCOPE` | Dinaikkan `P0`, ownership dipersempit ke sisi producer, dan acceptance-nya dibuat *eligibility-aware*: nol eligible milestone berarti nol fakta, dan itu sah |
| `8` | `INT-001` `P1` dan bergantung hasil implementasi | Dinaikkan `P0` dan dipecah menjadi `INT-001` Completion Contract dan `INT-002` Producer Handoff Contract. Keduanya **memblokir** implementasi |

Dua open question tertutup oleh bukti source, bukan oleh asumsi: canonical endpoint
(`RJ-DOC-OQ-001`) dan `EncounterStatus` setelah dokter selesai (`RJ-DOC-OQ-002`). Dasarnya adalah
urutan enum `InConsultation(6) → ConsultationCompleted(7) → Billing(8) → Completed(9)` beserta dua
consumer Medical Record yang mengunci catatan pada `Completed`. Empat open question baru dan lanjutan
tetap terbuka: `OQ-003` sampai `OQ-006`.

Satu permukaan penyelesaian ketiga ditemukan pada revisi ini:
`POST /doctor-consultations` dengan `CompleteImmediately=true`, yang menghasilkan konsultasi
`Completed` tanpa validasi dan tanpa handoff. Tidak ada call site frontend yang memakainya —
terverifikasi. Perlakuannya menjadi `RJ-DOC-OQ-006`.

Seluruh hash artefak yang berubah **dihitung ulang**, tidak disalin dari revisi sebelumnya. Enam
artefak desain dan kontrak yang tidak disentuh diverifikasi ulang dan **cocok persis**.

---

Catatan koreksi `2026-08-31` (revision `22`) — **pemisahan batas kepemilikan Dokter dan Billing**.

Sampai revisi `21` blueprint ini hanya memiliki roadmap `RJ-BIL`, sehingga pertanyaan *"apakah
pekerjaan developer Dokter / Rawat Jalan sudah selesai"* hanya dapat dijawab dengan angka Billing.
Revisi ini menambahkan roadmap klinis tersendiri dan memberi label `DOWNSTREAM` pada roadmap
Billing. **Tidak satu pun task Billing dihapus atau diturunkan statusnya.**

Audit read-only dilakukan terhadap SHA yang **benar-benar ada di working copy**, bukan SHA yang
tertulis pada manifest:

| Repository | Branch | `HEAD` yang diaudit | Working tree |
|---|---|---|---|
| `NewQuilvianSystemBackend` | `sukmagp` | `801a4f52459e1251ec9bb03c1abfe5e17dd3639c` | `DIRTY` — `1` berkas test termodifikasi, `7` berkas `agents/rules` terhapus; tidak satu pun menyentuh jalur konsultasi |
| `QuilvianSystemFrontendDev` | `QuilvianDevV2` | `baca9650848ded164538ab85405190fafe8785a3` | `CLEAN` |

`backend_source_sha` dan `frontend_source_sha` di tabel metadata **sengaja tidak disamakan**.
Menyatakan snapshot mana yang menjadi bukti resmi scope Billing adalah wewenang pemilik Billing,
dan menebaknya dari task ini akan mengklaim verifikasi yang tidak dilakukan. Yang perlu diketahui:
`6b25e60` tertinggal `144` commit dari `HEAD`.

Dua klaim revisi `21` terbukti **usang** terhadap `HEAD` dan dicatat sebagai koreksi fakta, bukan
sebagai penilaian ulang status task:

| Klaim revisi `21` | Keadaan pada `HEAD` yang diaudit |
|---|---|
| *"area `RadiologyManagement` belum ada sama sekali pada source"* | **Sudah ada** — `17` berkas, termasuk `RadOrderController`, `RadStudyService`, `RadSafetyGateEvaluator`, dan emisi fakta pada `RadStudyService.cs:958`; ditambah `Tests/.../Radiology/RadiologyStudyLifecycleTests.cs` |
| *"`88` test unit lulus; `29` milik `RJ-BIL-FE-001` dan `21` milik `RJ-BIL-FE-002`"* | Berkas test yang dirujuk **tidak ditemukan** pada frontend `HEAD` `baca965`; `tests/unit` memuat `22` berkas dan tidak satu pun bernama billing/folio/clinical-boundary. Layarnya sendiri **ada**. Kemungkinan besar hilang saat integrasi — **milik pemilik Billing untuk diverifikasi ulang** |

Artefak yang berubah pada revisi ini: `MODULE-STATUS.md`, `blueprint-manifest.md`,
`roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`,
`roadmap/requirement-traceability.md`, `contracts/integration-contract.md`,
`02-backend-architecture.md`, `03-frontend-architecture.md`, dan artefak baru
`roadmap/doctor-consultation-roadmap.md`. Hash sepuluh artefak lain **tidak dihitung ulang** pada
task ini; perubahannya bersifat penambahan label kepemilikan dan tidak mengubah cakupan,
acceptance criteria, dependency, maupun kontrak task Billing mana pun. `active_roadmap_revision`
karena itu tetap `1`.

---

Catatan verifikasi `2026-08-28` (revision `21`) — **`RJ-BIL-FE-002` selesai untuk Resep,
Tindakan, dan Laboratorium**. `next build` lulus exit `0` dengan kedua route Billing terbukti
dihasilkan, `88` test unit lulus tanpa satu pun gagal, dan lint berkas Billing bersih.

**Bagian Radiologi sengaja tidak dikerjakan** dan tetap ⛔. Radiologi belum terdaftar pada
`BillingSourceContract` backend, sehingga faktanya tidak akan pernah sampai ke layar. Ketiadaannya
**diumumkan di layar** alih-alih didiamkan: baris radiologi yang tidak muncul berarti *belum
diketahui*, bukan *tidak ada pemeriksaan*. Membiarkannya senyap akan membuat layar berbohong
secara pasif.

Dua temuan baru dicatat pada `roadmap/requirement-traceability.md` bagian 3, dan **tidak satu pun
diperbaiki dari task ini** karena keduanya milik modul lain:

| Temuan | Mengapa penting |
|---|---|
| `PrescriptionResponse.paymentStatus` masih mengirim nilai `Lunas` | `RJ-BIL-BE-002` sudah mencabut kewenangan finansial modul klinis dan menghapus endpoint yang **menulis** kolom itu — yang **membaca** belum. Siapa pun yang mengikat kolom itu ke badge status akan menampilkan pesanan klinis sebagai lunas, tanpa satu pun galat yang memberi tahu bahwa itu salah. Layar `FE-002` menjinakkannya untuk dirinya sendiri, bukan untuk layar lain |
| `GET /lab-orders` tanpa satu pun parameter penyaring | Tidak ada `encounterId`, tidak ada paginasi; seluruh pesanan laboratorium rumah sakit terkirim sekaligus. `FE-002` terpaksa menyaring per kunjungan di sisi klien. Berjalan sekarang; berhenti berjalan seiring pertumbuhan data. Yang harus berubah adalah endpoint-nya |

Satu butir Definition of Done `RJ-BIL-FE-002` **sengaja dibiarkan terbuka**: *"Batas klinis–finansial
ditinjau domain owner."* Implementasinya selesai dan teruji, tetapi tinjauan domain owner hanya
dapat dilakukan manusia dan tidak boleh ditandai selesai dari sini.

Dua hash berubah: `roadmap/frontend-roadmap.md` dan `roadmap/requirement-traceability.md`.
Sepuluh artefak lain diverifikasi ulang dan **cocok**.

Laporan task: [fe-rj-bil-002-batas-klinis-dan-finansial.md](task/report/frontend/fe-rj-bil-002-batas-klinis-dan-finansial.md).

---

Catatan verifikasi `2026-08-28` (revision `20`) — **`RJ-BIL-FE-001` selesai**. Task frontend
pertama modul ini dikerjakan di bawah `RJ-BIL-DEC-013` dan terbukti: `next build` lulus dengan
exit `0`, route `[encounterId]` terbukti dihasilkan pada `app-path-routes-manifest.json`, `67` test
unit lulus tanpa satu pun gagal, dan lint berkas Billing bersih.

Dua hash berubah: `roadmap/frontend-roadmap.md` dan `roadmap/requirement-traceability.md`.
Sepuluh artefak lain diverifikasi ulang dan **cocok**.

Tiga hal yang perlu diketahui pembaca berikutnya, dan **sengaja tidak dirapikan** dari task itu:

| Hal | Mengapa dibiarkan |
|---|---|
| `source_commits.frontend` masih `ab4bd836`, tertinggal **11 commit** dari `HEAD` `bd31dc99` | Menebak snapshot mana yang benar adalah wewenang pemilik frontend. Klaim terpenting yang bergantung padanya — `RJ-BIL-CAP-020` *Missing* — sudah diverifikasi ulang langsung ke pohon saat ini dan **masih benar** |
| Baris **Dependency** `RJ-BIL-FE-001` kurang menyebut `RJ-BIL-BE-007` | Scope task menuntut `processing outcome`, yang hanya tersedia pada endpoint milik `BE-007`. Dicatat sebagai delta pada roadmap dan laporan task; melengkapinya adalah wewenang pemilik roadmap |
| `Component render test` belum ada | Harness project memakai `node --test` tanpa `@testing-library`, sehingga render test memang **belum mungkin** ditulis. Ini batas alat, bukan pilihan; penutupannya adalah cakupan `RJ-BIL-FE-007` |

`RJ-BIL-CAP-020` kini **tertutup sebagian**: layar folio baca sudah ada, sedangkan payer split dan
correction status masih tidak punya satu pun consumer. `RJ-BIL-CAP-021` juga bergerak dari nihil
menjadi `29` test unit, dengan render test tetap kosong.

Laporan task: [fe-rj-bil-001-baca-tagihan-satu-kunjungan.md](task/report/frontend/fe-rj-bil-001-baca-tagihan-satu-kunjungan.md).

---

Catatan verifikasi `2026-08-28` (revision `19`) — **wewenang tulis frontend diberikan**.
`RJ-BIL-DEC-013` menaikkan `IMPLEMENTATION_AUTHORITY` roadmap frontend dari `NOT_GRANTED` menjadi
`GRANTED`, dan `BUILDER_EXECUTION` menjadi `AUTHORIZED` untuk **empat** task saja —
`RJ-BIL-FE-001`, `RJ-BIL-FE-002` bagian Lab, `RJ-BIL-FE-004`, dan `RJ-BIL-FE-005`.

Tiga task sisanya sengaja **tidak** ikut dinaikkan. `RJ-BIL-FE-003`, `RJ-BIL-FE-006`, dan
`RJ-BIL-FE-007` tetap `NOT_AUTHORIZED` karena endpoint pasangannya belum ada sama sekali;
menaikkannya bersama yang lain hanya akan membuat task tanpa backend tampak siap dikerjakan.
Bagian Radiologi pada `RJ-BIL-FE-002` juga dikecualikan selama `RJ-BIL-BE-004` terblokir.

Yang **tidak** diberikan keputusan ini: commit, push, merge, deployment, perubahan backend, dan
aktivasi `RJ-BIL-DEP-009`. `Frontend authority` atas route, menu, dan bentuk komponen pada
`03-frontend-architecture.md` bagian `7` tetap `OPEN`, begitu pula `Security/Privacy` — keduanya
sengaja dibiarkan terbuka dan tetap menjadi syarat sebelum aktivasi production.

Tiga hash berubah karena keputusan ini: `00-interview-decisions.md` (`decision_revision` `13` →
`14`), `roadmap/frontend-roadmap.md`, dan `roadmap/requirement-traceability.md` — yang blocker
*"wewenang tulis frontend belum diberikan"*-nya ditandai tertutup, bukan dihapus, supaya jejak
bahwa blocker itu pernah ada tidak hilang. Sembilan artefak lain diverifikasi ulang dan **cocok**.
`MODULE-STATUS.md` ikut dimutakhirkan tetapi tidak masuk tabel hash ini.

Satu hal yang perlu diketahui pembaca berikutnya dan **sengaja tidak saya sentuh**:
`MODULE-STATUS.md` mencatat `Frontend source SHA` `32db4acb…` sedangkan manifest dan kedua roadmap
memakai `ab4bd836…`. Selisih itu sudah ada sebelum keputusan ini dan tidak diciptakan olehnya.
Menyamakannya secara sepihak berarti menebak snapshot mana yang benar; yang berwenang menyatakan
itu adalah pemilik frontend.

---

Catatan verifikasi `2026-08-28` (revision `18`) — **penyelarasan artefak governance**. Seluruh
tiga belas hash dihitung ulang. Sembilan artefak desain dan kontrak cocok tanpa perubahan; ketiga
artefak roadmap berubah dan hash-nya diperbarui di sini, begitu pula `input_revision_hash` untuk
decisions.

Pemicunya: setelah `RJ-BIL-BE-006` selesai `2026-08-27`, sebagian artefak dimutakhirkan dan
sebagian tertinggal, sehingga beberapa dokumen bertentangan satu sama lain. Yang diselaraskan:

| Artefak | Yang melenceng | Perbaikan |
|---|---|---|
| `00-interview-decisions.md` | Header masih `Revision 10` padahal `RJ-BIL-DEC-011` dan `RJ-BIL-DEC-012` sudah tercatat dan `approved` di dalamnya | Header disamakan dengan `decision_revision` yang sudah dicatat manifest, yaitu `13`. Isi keputusan **tidak disentuh** |
| `MODULE-STATUS.md` | `IMPLEMENTATION_AUTHORITY` dan `BUILDER_EXECUTION` belum memuat `RJ-BIL-BE-006`; `Evidence state` masih menyebut `22` test, `3` migration, dan `88` migration terdaftar; **`Next recommended task` masih memakai urutan dependency yang sudah dikoreksi `RJ-BIL-DEC-008`** | Ketiganya dimutakhirkan. Urutan lama diberi catatan koreksi eksplisit, dan langkah berikutnya disusun ulang menurut apa yang benar-benar menahan modul |
| `roadmap/requirement-traceability.md` | Masih menyatakan `IMPLEMENTATION_AUTHORITY NOT_GRANTED`, `BUILDER_EXECUTION NOT_AUTHORIZED`, dan *"tidak ada test project pada snapshot"* | Ditulis ulang mengikuti bentuk kedua roadmap; kolom **Keadaan** dan **Governance** dipisahkan supaya ✅ tidak terbaca sebagai izin production |
| `roadmap/backend-roadmap.md` | Ringkasan bagian 0 tertinggal — `4 dari 9`, `111` test, `RJ-BIL-BE-006` terblokir — bertentangan dengan isi dokumennya sendiri | Disamakan menjadi `5 dari 9` dan `157` test |
| `roadmap/frontend-roadmap.md` | `RJ-BIL-FE-004` masih tercatat terblokir padahal `RJ-BIL-BE-006` sudah selesai | Enam tempat disamakan |
| `testing/readiness-report.md` | Catatan penyeliaan masih menyebut `111` test dan `4` dari `9` task | Catatan dimutakhirkan. **Badan laporan sengaja tidak ditulis ulang** — ia potret audit `2026-08-24`, dan verdict `NOT_READY` tetap berlaku |

Kedua roadmap juga memakai bentuk penyajian baru sejak `2026-08-27`: dari satu tabel sebelas
kolom menjadi struktur bagian bernomor dengan tabel `Field / Isi` vertikal per task, mengikuti
bentuk `rawat-inap`. Cakupan, acceptance criteria, dependency, dan kontrak **tidak berubah**;
`active_roadmap_revision` karena itu tetap `1`.

`current_phase` dinaikkan `RJ-BIL-PH-008` → `RJ-BIL-PH-009` agar cocok dengan `MODULE-STATUS.md`,
yang sejak revision `18` sudah mencatat `PH-008` sebagai fase selesai. `backend_source_sha`
dilengkapi: working tree yang belum di-commit mencakup `RJ-BIL-BE-002`, `003`, `006`, `007`, dan
remediasi penamaan QBE — bukan hanya `RJ-BIL-BE-003`.

Baris `owner-review-checklist.md` sengaja tidak memuat hash; isinya catatan, dan tetap demikian
sampai owner mengisi record approval.

Arsitektur domain revision `1` sudah tersedia. Fase berikutnya adalah `design-business-module`
untuk slice core internal/manual yang siap. Aktivasi adapter eksternal tetap terblokir oleh
`RJ-BIL-DEP-009`.
