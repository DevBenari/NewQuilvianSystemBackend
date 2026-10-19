# Requirement Traceability Matrix — Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` (bentuk `COMPOSITE`) |
| planning_id / revision / tanggal | `TRF-DEL-20261019-01` / 1 / 2026-10-19 Asia/Jakarta |
| contract_version | `0.17.0` (`draft`) |
| status | `APPROVED_DESIGN_PLANNED` |
| coverage status | **Zero Coverage Gap (100% Tercakup)** |
| berkas roadmap pasangan | [Backend Roadmap](backend-roadmap-prioritas-antrean-member.md) · [Frontend Roadmap](frontend-roadmap-prioritas-antrean-member.md) |
| target demo | Demo Operasional Selasa, 20 Oktober 2026 (`IGD-REQ-004`) |

## 1. Matriks Keterlacakan Komprehensif (Requirement-to-Task Traceability)

| No | Keputusan Bisnis | Functional Requirement | Acceptance Criteria | Uji Kontrak / Kasus Uji | Skenario UAT | Task Backend | Task Frontend | Status Gap |
|:---:|---|---|---|---|---|---|---|:---:|
| 1 | `RWI-DEC-338` | `FR-TRF-008` | `RWI-AC-481` (DTO response field CITO & Membership) | `TRF-AT-01` | `UAT-TRF-01`, `02` | `BE-RWI-252` | — | Terpenuhi |
| 2 | `RWI-DEC-338` | `FR-TRF-008` | `RWI-AC-482` (Urutan deterministik: CITO > Member Prioritas > Reguler FIFO) | `TRF-AT-02`, `TRF-AT-03` | `UAT-TRF-01`, `02`, `03` | `BE-RWI-253` | `FE-RWI-268` | Terpenuhi |
| 3 | `RWI-DEC-339` | `FR-TRF-008` | `RWI-AC-483` (Evaluasi real-time membership aktif & priority queue) | `TRF-AT-04` | `UAT-TRF-02` | `BE-RWI-252` | — | Terpenuhi |
| 4 | `RWI-DEC-339` | `FR-TRF-008` | `RWI-AC-484` (Membership inaktif/expired/non-prioritas = reguler) | `TRF-AT-05`, `TRF-AT-06` | `UAT-TRF-04` | `BE-RWI-252` | — | Terpenuhi |
| 5 | `RWI-DEC-338` | `FR-TRF-008` | `RWI-AC-485` (Status CITO sah dari formulir transfer IGD yang Signed) | `TRF-AT-07`, `TRF-AT-08` | `UAT-TRF-01` | `BE-RWI-252` | — | Terpenuhi |
| 6 | `RWI-DEC-338`, `341` | `FR-TRF-009` | `RWI-AC-486` (Tabel Antrean Transfer render lencana CITO & Tier) | `TRF-AT-09` | `UAT-TRF-05` | — | `FE-RWI-268` | Terpenuhi |
| 7 | `RWI-DEC-341` | `FR-TRF-010` | `RWI-AC-487` (Header Langkah Admisi PPRI render lencana CITO & Tier) | `TRF-AT-10` | `UAT-TRF-05` | — | `FE-RWI-269` | Terpenuhi |
| 8 | `RWI-DEC-341` | `FR-TRF-011` | `RWI-AC-488` (Denah Bed Management bangsal umum bebas lencana tier) | `TRF-AT-11` | `UAT-TRF-06` | — | `FE-RWI-269` | Terpenuhi |
| 9 | `RWI-DEC-338` | `FR-TRF-008` | `RWI-AC-489` (Pemilihan kamar tetap manual, tanpa auto-booking server) | `TRF-AT-12` | `UAT-TRF-05` | `BE-RWI-253` | `FE-RWI-269` | Terpenuhi |
| 10 | `RWI-DEC-339` | `FR-TRF-008` | `RWI-AC-490` (Expired di bangsal tidak mencabut atau downgrade kamar) | `TRF-AT-13` | `UAT-TRF-04` | `BE-RWI-253` | — | Terpenuhi |
| 11 | `RWI-DEC-340` | `FR-TRF-008` | `RWI-AC-491` (Bebas diskriminasi asuhan klinis dan non-interference) | `TRF-AT-14` | — | `BE-RWI-253` | — | Terpenuhi |
| 12 | `RWI-DEC-338` | `FR-TRF-008` | `RWI-AC-492` (Endpoint detail /{id} membawa field CITO & Membership konsisten) | `TRF-AT-15` | — | `BE-RWI-252` | — | Terpenuhi |
| 13 | `RWI-DEC-338` | `FR-TRF-012` | `RWI-AC-493` (Fail-safe fallback bila data CRM/IGD null tanpa crash 500) | `TRF-AT-16` | `UAT-TRF-07` | `BE-RWI-252` | — | Terpenuhi |
| 14 | `RWI-DEC-342` | `FR-TRF-013` | `RWI-AC-494` (Lencana Tier pada Kartu Hasil Pencarian No. RM Pasien Lama) | `TRF-AT-17` | `UAT-TRF-08` | — | `FE-RWI-270` | Terpenuhi |
| 15 | `RWI-DEC-342` | `FR-TRF-013` | `RWI-AC-495` (Panggilan endpoint CRM aktif saat pasien lama dipilih) | `TRF-AT-18` | `UAT-TRF-08` | — | `FE-RWI-270` | Terpenuhi |
| 16 | `RWI-DEC-342` | `FR-TRF-013` | `RWI-AC-496` (Panel verifikasi render nomor member, tier, dan masa aktif CRM) | `TRF-AT-19` | `UAT-TRF-08` | — | `FE-RWI-270` | Terpenuhi |
| 17 | `RWI-DEC-342` | `FR-TRF-013` | `RWI-AC-497` (Ketahanan admisi fail-open jika CRM gagal/non-aktif) | `TRF-AT-20` | `UAT-TRF-09` | — | `FE-RWI-270` | Terpenuhi |

## 2. Pemetaan Cakupan per Task

### 2.1 Backend Tasks

- **`BE-RWI-252`**:
  - Tanggung Jawab: DTO Response Extension & Relational Projection Query.
  - FR: `FR-TRF-008`, `FR-TRF-012`.
  - Acceptance Criteria: `RWI-AC-481`, `RWI-AC-483`, `RWI-AC-484`, `RWI-AC-485`, `RWI-AC-492`, `RWI-AC-493`.
  - Kasus Uji: `TRF-AT-01`, `TRF-AT-04`, `TRF-AT-05`, `TRF-AT-06`, `TRF-AT-07`, `TRF-AT-08`, `TRF-AT-14`, `TRF-AT-16`.
- **`BE-RWI-253`**:
  - Tanggung Jawab: Multi-Criteria Sorting Engine & Boundary Protection.
  - FR: `FR-TRF-008`.
  - Acceptance Criteria: `RWI-AC-482`, `RWI-AC-489`, `RWI-AC-490`, `RWI-AC-491`.
  - Kasus Uji: `TRF-AT-02`, `TRF-AT-03`, `TRF-AT-11`, `TRF-AT-12`, `TRF-AT-13`.

### 2.2 Frontend Tasks

- **`FE-RWI-268`**:
  - Tanggung Jawab: Lencana CITO & Tier Membership pada Tabel Antrean Transfer.
  - FR: `FR-TRF-009`.
  - Acceptance Criteria: `RWI-AC-482`, `RWI-AC-486`.
  - Kasus Uji / UAT: `TRF-AT-09`, `UAT-TRF-01`, `UAT-TRF-02`, `UAT-TRF-03`, `UAT-TRF-05`.
- **`FE-RWI-269`**:
  - Tanggung Jawab: Lencana pada Header Langkah Admisi PPRI & Isolasi Privasi Denah Bangsal.
  - FR: `FR-TRF-010`, `FR-TRF-011`.
  - Acceptance Criteria: `RWI-AC-487`, `RWI-AC-488`, `RWI-AC-489`.
  - Kasus Uji / UAT: `TRF-AT-10`, `TRF-AT-11`, `TRF-AT-12`, `UAT-TRF-05`, `UAT-TRF-06`.
- **`FE-RWI-270`**:
  - Tanggung Jawab: Lencana Tier pada Pencarian No. RM dan Validasi Keanggotaan CRM Pasien Lama.
  - FR: `FR-TRF-013`.
  - Acceptance Criteria: `RWI-AC-494`, `RWI-AC-495`, `RWI-AC-496`, `RWI-AC-497`.
  - Kasus Uji / UAT: `TRF-AT-17`, `TRF-AT-18`, `TRF-AT-19`, `TRF-AT-20`, `UAT-TRF-08`, `UAT-TRF-09`.

## 3. Pernyataan Penutupan Gap (Coverage Assurance)

- Total Keputusan Terpetakan: **5 Keputusan** (`RWI-DEC-338` s.d. `342`).
- Total Functional Requirements: **6 FR** (`FR-TRF-008` s.d. `013`).
- Total Acceptance Criteria: **17 AC** (`RWI-AC-481` s.d. `497`).
- Total Kasus Uji Kontrak: **20 Butir Uji** (`TRF-AT-01` s.d. `20`).
- Total Skenario UAT: **9 Skenario** (`UAT-TRF-01` s.d. `09`).
- Status Kesenjangan: **Nol Kesenjangan (Zero Gap)**.
