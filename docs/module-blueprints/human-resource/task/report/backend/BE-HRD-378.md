# Laporan Perubahan Backend — `BE-HRD-378`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-378` |
| Judul | Gerbang uang muka dan settlement untuk SPPD disetujui |
| Slice | `S-D5` amandemen / `MVP-70` — gelombang 4 · `EPIC HRD-121` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-378` |
| Trace | `FR-TRV-31`, `FR-TRV-32`; `HRD-DEC-226`, `HRD-DEC-227`; `HRD-TF-074`; API 34.2–34.3, 34.5; Validation `VR-TRV-30`, `VR-TRV-31` (bersama `VR-TRV-02` yang ada); AT `AT-HRD-TRV-30`, `31`, `34`, `35` |
| Contract version | `v28` — approved 19 November 2026; tidak diubah |
| Dependency | `BE-HRD-376` ✅ |
| Klasifikasi | `LOW–MEDIUM` — satu repository, 4 berkas source diubah sedikit, perubahan perilaku dua endpoint yang ada, tanpa schema |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjutkan") |
| Target tulis | `Services/TravelAdvanceRequestService.cs`, `Services/TravelSettlementService.cs`, `Controllers/BusinessTravelAdvancesController.cs`, `Controllers/BusinessTravelSettlementsController.cs`; uji di `TravelWorkflowLifecycleTests.cs` (5 baru) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `43f66daf` (branch `yoga`); `BE-HRD-376`/`377` belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti: harness 5 uji baru (42/42 uji perjalanan dinas; kontrol negatif 3 mutasi tertangkap), proyek uji HR 369/369, QBE Strict tanpa temuan, API devYoga dengan akun Dewi/dr. Bima (AT-35 hanya harness — devYoga tidak punya SPPD lama) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Tanpa entity baru; `QBE-MOD-002` tidak berlaku |
| Keberlakuan | `TOUCHED LEGACY` (service dan action pengajuan uang muka/settlement) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001` (`409` kontrak; `[ProducesResponseType]`), `QBE-VAL-001` (`VR-TRV-30`, `VR-TRV-31` sebelum perhitungan/penulisan) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-PERM-001` (atribut tidak berubah), `QBE-DB-*` |
| Utang lama yang tidak disentuh | Penomoran `ADV/…` dan `STL/…` memakai `CountAsync() + 1` (`QBE-CODE-003` untuk `NEW CODE`) — sama seperti SPPD, diusulkan task penomoran |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Kondisi as-is terhadap kontrak

| Hal | As-is | Kontrak `v28` |
| --- | --- | --- |
| Uang muka | Hanya menolak SPPD `Cancelled`/`Rejected` (`HRD-TF-074`); plafon 80% dari `ApprovedBudgetAmount` atau estimasi bila 0 | `409` bila SPPD belum disetujui (`VR-TRV-30`); plafon dari anggaran disetujui |
| Settlement | Tanpa gerbang | `409` bila belum disetujui **dan** tanpa uang muka `Paid` (`VR-TRV-31`) |
| Uang muka lama | — | Persetujuan/pencairan/settlement-nya tetap berjalan (`HRD-DEC-227`) |

---

## 2. Perubahan yang dikerjakan

| Berkas | Isi |
| --- | --- |
| `Services/TravelAdvanceRequestService.cs` | Sesudah penolakan `Cancelled`/`Rejected` (aturan lama): bila `ApprovedAt` kosong → `BusinessTravelRequestException(409, "SPPD belum disetujui. Ajukan SPPD dan tunggu persetujuan sebelum mengajukan uang muka.")`. Plafon 80% kini dari `ApprovedBudgetAmount` saja (fallback ke estimasi dihapus; SPPD disetujui selalu punya anggaran = estimasi) |
| `Services/TravelSettlementService.cs` | Sesudah mencari uang muka `Paid`: bila `ApprovedAt` kosong dan tidak ada uang muka `Paid` → `409` "SPPD belum disetujui sehingga belum dapat dipertanggungjawabkan." |
| `Controllers/BusinessTravelAdvancesController.cs`, `BusinessTravelSettlementsController.cs` | `BusinessTravelRequestException` → kode statusnya (sebelum `catch` umum `400`); `[ProducesResponseType]` 400/409 bertipe; `<summary>` XML action yang disentuh dibuang |

Alur persetujuan uang muka 2-pintu, pencairan, verifikasi, dan penutupan settlement tidak disentuh.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat | `PASS` | Keluaran build |
| `dotnet test --filter TravelWorkflowLifecycleTests\|TravelSubmitTests` | 42/42 lulus (5 baru) | `PASS` | Keluaran `dotnet test` |
| — Uang muka: Draf, menunggu, disetujui | `409` pesan kontrak (dua kali); sesudah disetujui 4.000.001 → `400` `VR-TRV-02`, 4.000.000 (80% dari 5.000.000) → dibuat | `PASS` (AT-30) | idem |
| — Uang muka SPPD disetujui lalu dibatalkan | Ditolak (aturan lama) | `PASS` | idem |
| — Settlement: menunggu tanpa uang muka; disetujui | `409` pesan kontrak, tidak ada baris settlement; disetujui → dibuat | `PASS` (AT-31) | idem |
| — SPPD lama `Submitted` tanpa workflow (anggaran disalin perilaku lama) | Uang muka `409`; sesudah Ajukan + disetujui → dibuat | `PASS` (AT-34) | idem |
| — SPPD lama dengan uang muka `Paid` | Settlement tetap dibuat walau SPPD belum disetujui | `PASS` (AT-35) | idem |
| Kontrol negatif (3 mutasi; berkas dipulihkan, `cmp` identik) | G1 gerbang uang muka, G3 gerbang settlement, G4 pengecualian uang muka lama — semuanya tertangkap | `PASS` | Keluaran per putaran |
| QBE `Strict` empat berkas | 0 temuan | `PASS` | Keluaran checker |
| Proyek uji HR penuh | 369/369 lulus (364 + 5 baru) | `PASS` | Keluaran `dotnet test` |
| devYoga (hak sementara Dewi: `BusinessTravelRequest` Read/Create, `BusinessTravelAdvance : Request`, `BusinessTravelSettlement : Submit`; Bima: `WorkflowInstance` Read/Approve) | — | — | `at-378.out`, `at-378b.out` |
| — Uang muka SPPD Draf `0008`; SPPD `0009` menunggu | `409` "SPPD belum disetujui. Ajukan SPPD dan tunggu persetujuan sebelum mengajukan uang muka." (keduanya) | `PASS` (AT-30) | idem |
| — Settlement SPPD `0009` | `409` "SPPD belum disetujui sehingga belum dapat dipertanggungjawabkan." | `PASS` (AT-31) | idem |
| — Uang muka SPPD `0003` disetujui (anggaran 4.700.000) | 3.760.001 → `400` `VR-TRV-02` (batas 3.760.000); 3.760.000 → `201` `ADV/RSQ/2026/11/0001` | `PASS` (AT-30) | idem |
| — SPPD `0010` disetujui Bima, lalu settlement | `201` `STL/RSQ/2026/11/0001` | `PASS` (AT-31) | `at-378b.out` |
| — Catatan proses | Putaran pertama SPPD H gagal dibuat (`400`) karena tanggalnya bentrok dengan SPPD G milik Dewi (validasi bentrok jadwal yang ada) — galat skrip; diulang dengan tanggal lain | Catatan | idem |
| — Pemulihan hak | `identik: true` — Dewi 11, Bima 45 (kedua putaran) | `PASS` | idem |
| AT-35 di devYoga | Tidak dapat diuji: devYoga tidak memiliki SPPD lama (tabel kosong sebelum uji ini) | `NOT RUN` (harness saja) | — |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Uang muka Draf/menunggu `409`; disetujui lolos dengan plafon 80% | Terpenuhi | Harness + devYoga |
| 2. Settlement tanpa persetujuan dan tanpa uang muka `Paid` `409`; SPPD lama dengan uang muka `Paid` tetap dapat di-settle | Terpenuhi (bagian kedua harness) | Harness + devYoga |
| 3. SPPD lama: uang muka ditolak sebelum diajukan ulang, lolos sesudah disetujui | Terpenuhi (harness) | AT-34 |
| 4. Uang muka lama `Submitted`/`Paid` tetap dapat diproses dan di-settle | Terpenuhi: alur persetujuan/pencairan tidak diubah; settlement uang muka `Paid` dibuktikan harness | AT-35 + diff |
| Migration | Tidak ada | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data uji yang tertinggal di devYoga | Uang muka `ADV/RSQ/2026/11/0001` (3.760.000, `Submitted`) pada SPPD `0003`; SPPD `0009` menunggu dr. Bima; SPPD `0010` disetujui dengan settlement `STL/RSQ/2026/11/0001` — disimpan sesuai pilihan pengguna |
| Teks pesan plafon | Pesan `VR-TRV-02` yang ada masih berbunyi "pagu estimasi biaya disetujui"; tidak diubah (di luar cakupan) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `BE-HRD-379` (jejak audit 12 event `HRD_TRV_*`), lalu `BE-HRD-380` (verifikasi amandemen) |
