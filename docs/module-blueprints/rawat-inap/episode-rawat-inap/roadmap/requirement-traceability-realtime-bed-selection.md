# Requirement Traceability — Notifikasi Realtime Konflik Pemilihan Bed

| Metadata | Nilai |
|---|---|
| planning_id / revision | `RTB-DEL-20261017-01` / 1 |
| contract / approval | `0.16.0` / `RWI-DEC-337` / `APPROVED_DESIGN_PLANNED` |
| roadmap | [Backend](backend-roadmap-realtime-bed-selection.md) · [Frontend](frontend-roadmap-realtime-bed-selection.md) |
| sumber | PRD 30, Validation 19, Acceptance 24, Decision log revision 59 |
| coverage verdict | **ZERO COVERAGE GAP** untuk scope MVP; `RWI-OQ-151` adalah release gate produksi, bukan requirement tanpa task |

## Register Bukti Pelaksanaan

| Task | Status bukti | Laporan |
|---|---|---|
| `BE-RWI-250` | 🟡 Source/build terbukti; runtime allow/deny dan delivery belum dijalankan | [BE-RWI-250](../task/report/backend/BE-RWI-250.md) |
| `BE-RWI-251` | 🟡 Source/build terbukti; runtime transaksi/event, replay, race, dan failure injection belum dijalankan | [BE-RWI-251](../task/report/backend/BE-RWI-251.md) |
| `FE-RWI-265` | 🟡 Client event/group, rejoin, dan cleanup tersedia; runtime reconnect/unit change belum dijalankan | [FE-RWI-265](../task/report/frontend/FE-RWI-265.md) |
| `FE-RWI-266` | 🟡 Hook filter/dedup/actor/refetch tersedia dan test fokus lulus; runtime lifecycle belum dijalankan | [FE-RWI-266](../task/report/frontend/FE-RWI-266.md) |
| `FE-RWI-267` | 🟡 Alert/refetch/stale-clear tersedia dan test fokus lulus; runtime dua sesi belum dijalankan | [FE-RWI-267](../task/report/frontend/FE-RWI-267.md) |

## Matriks Functional Requirement

| FR | Ringkasan | Task backend | Task frontend | Bukti utama |
|---|---|---|---|---|
| `FR-RI-202` | Join/leave selection per unit | `BE-RWI-250` | `FE-RWI-265` | [FE-RWI-265](../task/report/frontend/FE-RWI-265.md); UAT-904, 913 runtime pending |
| `FR-RI-203` | Dua permission read + unit scope; no broad broadcast | `BE-RWI-250` | — | UAT-904, 907 |
| `FR-RI-204` | Event reservation post-commit | `BE-RWI-251` | `FE-RWI-267` | UAT-901, 908, 909 |
| `FR-RI-205` | Event placement post-commit | `BE-RWI-251` | `FE-RWI-267` | UAT-902, 908 |
| `FR-RI-206` | Event transfer hanya unit target | `BE-RWI-251` | `FE-RWI-267` | UAT-903, 908 |
| `FR-RI-207` | No event failure/replay; publish failure tidak mengubah mutation | `BE-RWI-251` | — | UAT-906, 908, 909 |
| `FR-RI-208` | Alert actor lain dan suppress actor sendiri | `BE-RWI-250` | `FE-RWI-266`, `FE-RWI-267` | [FE-RWI-266](../task/report/frontend/FE-RWI-266.md), [FE-RWI-267](../task/report/frontend/FE-RWI-267.md); runtime UAT-901, 910, 916 pending |
| `FR-RI-209` | Refetch dan clear stale selection | — | `FE-RWI-267` | [FE-RWI-267](../task/report/frontend/FE-RWI-267.md); runtime UAT-901, 912, 915 pending |
| `FR-RI-210` | Reconnect rejoin/refetch tanpa replay | — | `FE-RWI-265`, `FE-RWI-266` | [FE-RWI-265](../task/report/frontend/FE-RWI-265.md), [FE-RWI-266](../task/report/frontend/FE-RWI-266.md); runtime UAT-911 pending |
| `FR-RI-211` | Dedup, unit mismatch, lifecycle cleanup | — | `FE-RWI-265`, `FE-RWI-266` | [FE-RWI-265](../task/report/frontend/FE-RWI-265.md), [FE-RWI-266](../task/report/frontend/FE-RWI-266.md); runtime UAT-913, 914 pending |

## Matriks Acceptance Criteria

| Acceptance | Task pemilik | Bukti |
|---|---|---|
| `RWI-AC-469` reservasi memicu alert/refetch/stale-clear | `BE-RWI-251`, `FE-RWI-267` | UAT-901, 912 |
| `RWI-AC-470` mutation gagal tidak publish | `BE-RWI-251` | UAT-908 |
| `RWI-AC-471` placement/transfer post-commit; 409 tetap authoritative | `BE-RWI-251`, `FE-RWI-267` | UAT-902, 903, 905 |
| `RWI-AC-472` isi alert dan actor suppression | `BE-RWI-250`, `FE-RWI-266`, `FE-RWI-267` | UAT-901, 910, 916 |
| `RWI-AC-473` penerima terbatas permission dan unit | `BE-RWI-250` | UAT-903, 904, 907 |
| `RWI-AC-474` keluar layar/pindah unit menghentikan audience lama | `BE-RWI-250`, `FE-RWI-265`, `FE-RWI-266` | UAT-913 |
| `RWI-AC-475` reconnect memuat snapshot terbaru tanpa alert lama | `FE-RWI-265`, `FE-RWI-266` | UAT-911 |
| `RWI-AC-476` API guard tetap melindungi saat SignalR putus/race | `BE-RWI-251`, `FE-RWI-267` | UAT-905, 911, 912 |
| `RWI-AC-477` reconnect berulang tanpa tumpukan alert/history | `FE-RWI-265`, `FE-RWI-266` | UAT-911, 914 |
| `RWI-AC-478` publish failure sesudah reserve tidak rollback | `BE-RWI-251` | UAT-906 |
| `RWI-AC-479` publish failure placement/transfer tidak meloloskan stale submit | `BE-RWI-251` | UAT-905, 906 |
| `RWI-AC-480` recovery tanpa outbox/replay | `BE-RWI-251`, `FE-RWI-266` | UAT-906, 911 |

## Matriks Validation

| Validation | Task | Bukti minimum |
|---|---|---|
| `VAL-RTB-01` GUID unit nonempty | `BE-RWI-250` | Join ID kosong ditolak |
| `VAL-RTB-02` permission + unit scope | `BE-RWI-250` | UAT-904 |
| `VAL-RTB-03` committed baru dan non-replay | `BE-RWI-251` | UAT-908, 909 |
| `VAL-RTB-04` target group dari bed committed | `BE-RWI-250`, `BE-RWI-251` | UAT-903, 907 |
| `VAL-RTB-05` payload wajib dan minimum | `BE-RWI-250`, `BE-RWI-251` | UAT-907 |
| `VAL-RTB-06` actor sendiri tidak alert | `FE-RWI-266`, `FE-RWI-267` | UAT-910 |
| `VAL-RTB-07` unit di luar aktif diabaikan | `FE-RWI-266` | UAT-913 |
| `VAL-RTB-08` event duplicate sekali | `FE-RWI-266` | UAT-914 |
| `VAL-RTB-09` event/reconnect refetch | `FE-RWI-266`, `FE-RWI-267` | UAT-911, 912, 915 |
| `VAL-RTB-10` publish error tidak mengubah HTTP/DB | `BE-RWI-251` | UAT-906 |
| `VAL-RTB-11` race tetap 409 + refresh | `BE-RWI-251`, `FE-RWI-267` | UAT-905, 912 |
| `VAL-RTB-12` nama lengkap memiliki release gate | `BE-RWI-250`, langkah release `RWI-OQ-151` | Bukti sign-off sebelum production activation |

## Matriks UAT ke Task

| UAT | Task yang harus terbukti | Status awal |
|---|---|---|
| `UAT-901` reservation two-session | `BE-RWI-251`, `FE-RWI-267` | NOT RUN |
| `UAT-902` placement post-commit | `BE-RWI-251`, `FE-RWI-267` | NOT RUN |
| `UAT-903` transfer target unit | `BE-RWI-251`, `FE-RWI-267` | NOT RUN |
| `UAT-904` unauthorized join | `BE-RWI-250` | NOT RUN |
| `UAT-905` race satu pemenang | `BE-RWI-251`, `FE-RWI-267` | NOT RUN |
| `UAT-906` publish failure setelah commit | `BE-RWI-251` | NOT RUN |
| `UAT-907` payload/log minimization | `BE-RWI-250`, `BE-RWI-251` | NOT RUN |
| `UAT-908` failure/rollback no event | `BE-RWI-251` | NOT RUN |
| `UAT-909` replay no second event | `BE-RWI-251` | NOT RUN |
| `UAT-910` actor suppression | `FE-RWI-266`, `FE-RWI-267` | NOT RUN |
| `UAT-911` reconnect rejoin/refetch/no replay | `FE-RWI-265`, `FE-RWI-266` | NOT RUN |
| `UAT-912` stale selection clear | `FE-RWI-267` | NOT RUN |
| `UAT-913` unit change lifecycle | `FE-RWI-265`, `FE-RWI-266` | NOT RUN |
| `UAT-914` duplicate event | `FE-RWI-266` | NOT RUN |
| `UAT-915` refresh failure | `FE-RWI-266`, `FE-RWI-267` | NOT RUN |
| `UAT-916` accessibility saat scroll/popup tertutup | `FE-RWI-267` | NOT RUN |

## Gate, Gap, dan Handoff

| Item | Klasifikasi | Dampak |
|---|---|---|
| `RWI-OQ-151` owner security/privacy belum memberi sign-off nama lengkap | `PRODUCTION_RELEASE_GATE` | Implementasi dan verifikasi nonproduksi boleh berjalan; deployment/activation produksi payload nama lengkap dilarang sampai CLOSED |
| Runtime backend/frontend, identity dua user, dan data bed uji | `VERIFICATION_DEPENDENCY` | Bila tidak tersedia saat task final, status maksimal 🟡; bukan alasan menghapus skenario |
| Automated backend test baru | `NOT_APPLICABLE` | Tidak diwajibkan; bukti kontrak/build/runtime mengikuti backend TEST_POLICY |
| Coverage requirement | `ZERO_GAP` | Semua 10 FR, 12 AC, 12 VAL, dan 16 UAT memiliki task atau release gate eksplisit |

Urutan handoff implementasi: `BE-RWI-250` dan `FE-RWI-265` boleh mulai paralel; lanjut
`BE-RWI-251` dan `FE-RWI-266`; tutup dengan `FE-RWI-267` sesudah event backend tersedia.
