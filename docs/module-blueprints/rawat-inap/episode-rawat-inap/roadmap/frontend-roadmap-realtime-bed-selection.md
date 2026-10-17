# Roadmap Frontend Notifikasi Realtime Konflik Pemilihan Bed

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `RTB-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; `RWI-DEC-337`. Eksekusi memakai `build-module-frontend`, tepat satu task per pemanggilan |
| kontrak terkunci | `0.16.0`; [Frontend 20](../03-frontend-architecture.md), [API 17](../contracts/api-contract.md), [PRD 30](../04-prd-to-mvp.md), [Flow 21](../flowcharts/21-notifikasi-realtime-pemilihan-bed.md) |
| source snapshot | Frontend `HamzahV2@0accf387138a3fcc1ff40fbb5bdeb960076b4d59`; backend `MHamzah@d420c2fbe094a2d70776c44955233bce9c87bda8` |
| worktree guard | File frontend yang sudah modified milik pengguna wajib dipertahankan dan digabung secara sadar; roadmap bukan bukti implementasi |
| hash lock | [realtime-bed-selection-delivery-lock.json](realtime-bed-selection-delivery-lock.json) |
| scope / ID | 3 task: `FE-RWI-265` s.d. `FE-RWI-267`; berikutnya `FE-RWI-268` |
| roadmap pasangan | [Backend](backend-roadmap-realtime-bed-selection.md) · [Traceability](requirement-traceability-realtime-bed-selection.md) |
| production gate | `RWI-OQ-151` wajib CLOSED sebelum nama lengkap diaktifkan di produksi; bukan dependency coding nonproduksi |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai dan seluruh acceptance/DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` |

## Grafik Urutan Dependency

`[BE]` adalah cermin baca-saja dari roadmap backend. Garis berarti prasyarat → task.

```text
FE-RWI-265 ──> FE-RWI-266 ──┬──> FE-RWI-267
                             │
BE-RWI-251 [BE] ─────────────┘
```

| Gelombang | Boleh mulai setelah | Task ID | Hasil gelombang |
|---:|---|---|---|
| 1 | — | `FE-RWI-265` | Client SignalR mendukung event dan membership unit selection |
| 2 | `FE-RWI-265` | `FE-RWI-266` | Hook lifecycle menangani scope unit, dedup, actor suppression, reconnect, dan cleanup |
| 3 | `FE-RWI-266`, `BE-RWI-251` [BE] | `FE-RWI-267` | Alert pada selection terintegrasi dan terbukti lintas dua sesi |

Pasangan dependency persis: `FE-RWI-265 → FE-RWI-266`, `FE-RWI-266 → FE-RWI-267`, dan
`BE-RWI-251 [BE] → FE-RWI-267`.

## Keputusan Reuse UI dan Client

| Elemen | Disposisi | Bukti / alasan |
|---|---|---|
| `@microsoft/signalr` | Reuse | Sudah menjadi dependency dan dipakai `queue-realtime-client.js`; tidak menambah package |
| `createQueueRealtimeConnection` | Extend | Sudah memiliki auth, reconnect, group sync, dan cleanup; hub tetap `/hubs/queues` |
| `useFloorPlan.refresh` | Reuse | Source of truth denah/eligibility; event tidak boleh memutasi status bed dari payload |
| stale-selection effect pada `FloorPlanSelection` | Reuse | Membatalkan `selectedBedId` melalui `onClearSelection` setelah hasil refresh tidak eligible |
| `useInpatientBedRealtime` | Baru | Boundary lifecycle khusus unit selection, dedup sesi, actor suppression, callback refetch |
| `ToastStack` | Reuse preferred | Base component existing sudah dismissible dan lebih sesuai alert nonblocking daripada popup kamar |
| `InformationAlert` | Reuse fallback | Boleh dipakai bila implementer membuktikan dismissible/live-region; tidak boleh memblokir scroll |
| popup kamar | Tidak menjadi owner | Koneksi/alert harus hidup ketika user scroll dan popup tertutup |

## Register Task

| Task ID / Status | Outcome | Requirement / keputusan | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / owner | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `FE-RWI-265` | Client queue realtime memahami event dan sync membership unit bed, termasuk rejoin | FR-RI-202, 210, 211; DEC-335 | 0.16.0; API 17.2 | `createQueueRealtimeConnection`, auth/reconnect existing | Event constant/handler, join/leave/sync unit IDs, rejoin, stop cleanup | — | AC-474–477; UAT-911, 913 | `V-FE`; review lifecycle/contract | Frontend Platform + Rawat Inap; regresi consumer hub existing | `D-FE`; laporan `task/report/frontend/FE-RWI-265.md` |
| `FE-RWI-266` | Hook selection mengelola connection, unit scope, dedup, actor suppression, refetch callback, dan cleanup | FR-RI-208, 210, 211 | 0.16.0; VAL-RTB-06–09 | client FE-RWI-265; selector `selectUserInfo` | Hook baru tanpa UI, bounded event IDs, defense-in-depth unit filter, reconnect refresh | `FE-RWI-265` | AC-472, 474–477; UAT-910, 911, 913–915 | `V-FE`; verifikasi lifecycle terisolasi | Rawat Inap; stale closure/duplicate handler | `D-FE`; laporan `task/report/frontend/FE-RWI-266.md` |
| `FE-RWI-267` | Floor plan selection menampilkan alert actor lain, lalu refresh dan membersihkan pilihan stale | FR-RI-208, 209 | 0.16.0; message contract Frontend 20.4 | `FloorPlanSelection`, `useFloorPlan.refresh`, stale effect, `ToastStack` | Integrasi hook, 3 pesan, dismiss/live region, error refetch, popup-independent | `FE-RWI-266`, `BE-RWI-251` [BE] | AC-469, 471, 472, 476; UAT-901–903, 905, 910, 912, 915, 916 | `V-FE`; browser dua sesi/unit sama dan negative lintas unit | Rawat Inap + UX/A11y; PHI dan worktree overlap | `D-FE`; laporan `task/report/frontend/FE-RWI-267.md` |

## Batas Handoff dan Definition of Done

### V-FE — bukti wajib setiap task frontend

1. Re-read source dan `git diff` sebelum edit; jangan menimpa perubahan pengguna yang sudah ada.
2. Jalankan `npm run lint:errors`, `npm run test:unit`, dan `npm run build`.
3. Penulisan test baru opsional; **Jest dilarang**. Test existing yang berubah perilakunya harus tetap
   lulus.
4. Verifikasi network/SignalR dan UI sesuai scope task. Task final membutuhkan dua sesi authenticated,
   same-unit delivery, cross-unit/no-permission denial, actor suppression, reconnect, dan stale clear.
5. Bukti visual hanya ditulis dalam laporan task; nama pasien uji harus fiktif dan tidak bocor ke log.

### D-FE — selesai berarti

- Outcome dan acceptance task terbukti, ketiga command `V-FE` lulus, serta tidak ada package,
  route, menu, permission, atau kontrak backend yang diubah sepihak.
- Alert dapat ditutup, diumumkan screen reader, tidak memblokir scrolling, dan tetap aktif saat popup
  tertutup.
- Laporan tracked ada pada `task/report/frontend/<TASK-ID>.md`.
- `FE-RWI-267` maksimal 🟡 bila backend/runtime dua sesi belum tersedia, walaupun source/lint/build
  sudah selesai.

## Kartu Task

### FE-RWI-265 — Extend queue realtime client untuk unit selection

| Field | Isi |
|---|---|
| Status | `NOT_STARTED` |
| Outcome | Satu client existing dapat mendaftarkan handler `InpatientBedSelectionChanged`, sync daftar `serviceUnitId`, rejoin sesudah reconnect, dan leave/stop bersih |
| Trace | FR-RI-202, 210, 211; AC-474–477; VAL-RTB-07–09; UAT-911, 913 |
| Scope source | `src/lib/realtime/queue-realtime-client.js` |
| Cakupan | Tambah constant/callback event; state unit IDs; `join/leave/syncInpatientBedSelectionUnits`; rejoin pada `onreconnected`; reset saat `stop`; pertahankan seluruh consumer queue/emergency existing |
| Larangan | Tidak membuat koneksi hub kedua, tidak menambah package, tidak membaca/mencatat patient name pada logger, tidak mengubah API consumer existing secara breaking |
| Dependency | — |
| Verifikasi wajib | `V-FE`; contract name; unit dedup; unit change; reconnect; stop; regresi group queue existing |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-265.md` |

### FE-RWI-266 — Hook lifecycle realtime pemilihan bed

| Field | Isi |
|---|---|
| Status | `NOT_STARTED` |
| Outcome | Hook baru memberikan satu boundary aman untuk subscription unit, dedup event sesi, actor suppression, callback event, refresh reconnect, dan cleanup |
| Trace | FR-RI-208, 210, 211; AC-472, 474–477; VAL-RTB-06–09; UAT-910, 911, 913–915 |
| Scope source | `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-realtime.js` [baru] dan import minimum yang diperlukan |
| Input | `enabled`, `currentUserId`, distinct `serviceUnitIds`, `onChanged(payload)`, callback reconnect refresh |
| Cakupan | Normalize GUID; defense-in-depth unit filter; bounded in-memory set `eventId`; suppress alert actor sendiri; cleanup handler/group/connection; event actor lain diteruskan sekali; reconnect hanya refetch tanpa replay |
| Dependency | `FE-RWI-265` |
| Verifikasi wajib | `V-FE`; event unit salah, duplicate, actor sendiri, unit berganti, reconnect berulang, callback error, unmount |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-266.md` |

### FE-RWI-267 — Integrasi alert, refresh, dan stale clear pada FloorPlanSelection

| Field | Isi |
|---|---|
| Status | `NOT_STARTED` |
| Outcome | User lain pada unit sama melihat pesan bed/pasien/petugas segera sesudah commit, lalu denah refresh dan pilihan stale hilang |
| Trace | FR-RI-208, 209; AC-469, 471, 472, 476; VAL-RTB-06, 09, 11; UAT-901–903, 905, 910, 912, 915, 916 |
| Scope source | `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx`; hook `use-floor-plan.js` hanya bila bukti implementasi memerlukan adapter minimum; base `ToastStack`/style existing |
| Reuse | `selectUserInfo`, `useInpatientBedRealtime`, `state.refresh`, `onClearSelection`, `ToastStack` preferred |
| Cakupan | Bentuk tiga pesan sesuai `eventType`; fallback `bedName` lalu `bedCode`; alert sebelum await refresh; tetap tampil bila refresh gagal; dismissible/live region; tidak bergantung popup; no alert actor sendiri |
| Worktree guard | File ini sudah modified oleh pengguna pada snapshot planning. Implementer wajib membaca diff dan menggabungkan perubahan; dilarang mengganti/reset file secara keseluruhan |
| Dependency | `FE-RWI-266`, `BE-RWI-251` [BE] |
| Verifikasi wajib | `V-FE`; dua browser/session; reserve/place/transfer; unit sama/lain; actor sendiri; scroll + popup tertutup; refresh gagal; race 409; reconnect tanpa replay |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-267.md`; bukti runtime dua sesi untuk status ✅ |
