# Laporan Perubahan Backend — `BE-ACC-P2-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-016` |
| Judul | Migration master aturan posting (GATED) |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-016` (revisi 3) |
| Trace | `02-backend-architecture.md` bagian 18, rencana migration urutan 1 — sisa sesudah `BE-ACC-P2-004`; digabung dengan perubahan index `BE-ACC-P2-031` |
| Contract version | Kamus data bagian 11, 12, 12b |
| Dependency | `BE-ACC-P2-015` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 0 (agent tidak mengubah berkas), logika 0, kontrak API 0, database 2, keamanan 0, UI 0; ditambah risiko koordinasi migration lintas modul |
| Task mode | `BACKEND` — **pembuatan dan penerapan migration oleh Rizki**; agent hanya memverifikasi |
| Target tulis agent | Laporan ini, baris status roadmap, traceability. Nol berkas `Migrations/` disentuh agent |
| Model | Claude Opus 5 |
| Commit backend | Migration di `12b8af63` (Rizki, 14 Sep 2026 12:34 WIB, induk `53d8eedf`); integration di-merge sesudahnya lewat `ba124bbb` |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI** — 3 dari 3 acceptance terbukti dari berkas migration dan snapshot; migration dibuat **dan** diterapkan Rizki |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `AccountingManagement` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 |
| Keberlakuan | `NEW CODE` — tabel baru; plus perubahan index pada tabel `Acc*` yang sudah ada |
| QBE yang berlaku | `QBE-CFG-001` (skema berasal dari configuration), `QBE-CODE-004` (kode unik ber-index database) |
| Wewenang | Pembuatan dan eksekusi migration — **Rizki** (`AGENTS.md`, `CLAUDE.md` backend). Agent nol perintah `dotnet ef`, nol eksekusi database |

---

## 1. Masalah yang diperbaiki

`BE-ACC-P2-015` menambahkan tiga entity ke model EF, dan `BE-ACC-P2-031` mengubah satu index, tetapi
database belum punya tabelnya. Akibatnya endpoint Jenis Kejadian dan Aturan Posting menjawab `500`
(`relation does not exist`), dan sejak EF Core 9 `dotnet ef database update` menolak berjalan sama
sekali (`PendingModelChangesWarning`) — termasuk untuk migration modul lain yang tertunda.

---

## 2. Proses yang terjadi

| Langkah | Pelaku | Hasil |
| ---: | --- | --- |
| 1 | Rizki | Merge `rizkiG` ← integration (`53d8eedf`). Agent memeriksa snapshot: selisih terhadap integration +510 baris, nol deletion |
| 2 | Rizki | `dotnet ef database update` pertama **ditolak** `PendingModelChangesWarning` — model sudah memuat entity `015`, snapshot belum. Database tidak berubah |
| 3 | Rizki | `dotnet ef migrations add AddAccountingPostingRuleMaster` → `20260914044507`. Agent memeriksa isinya — bersih, lihat bagian 3 |
| 4 | Rizki | `database update` berhenti di `20260910153119_AddBbkBloodOrder` (`42P01: relation "public.TrxPatientEncounter" does not exist`) — cacat migration **Blood Bank**, bukan Accounting. Lihat bagian 7 |
| 5 | Integration | `b3361d07` (Rivenjxv, 14 Sep 2026) membetulkan FK `AddBbkBloodOrder` ke `RegPatientEncounter` |
| 6 | Rizki | Commit migration (`12b8af63`), merge integration lagi (`ba124bbb`), `database update` berhasil; `dotnet ef migrations list` tanpa satu pun `(Pending)`, termasuk `20260913070556_AddBbkBloodUnitAllocation` dan `20260914044507_AddAccountingPostingRuleMaster` |

---

## 3. Isi migration yang diperiksa

`Migrations/20260914044507_AddAccountingPostingRuleMaster.cs` — 245 baris.

### 3.1 `Up`

| Operasi | Objek |
| --- | --- |
| `DropIndex` | `IX_AccJournal_ReversalOfJournalId` (index biasa lama) |
| `CreateTable` | `AccEventType` — PK; 4 kolom domain + kolom `IdentityModel`; `IsActive` bawaan `true` |
| `CreateTable` | `AccPostingRule` — FK `Restrict` ke `AccEventType`, `AccJournalType`, `MstLegalEntity`; `Treatment` bawaan `2` |
| `CreateTable` | `AccPostingRuleLine` — FK **`Cascade`** ke `AccPostingRule`; FK `Restrict` ke `AccChartOfAccount`, `MstCostCenter`; `ComponentCode` bawaan `"TOTAL"` |
| `CreateIndex` unique | `IX_AccJournal_ReversalOfJournalId` — filter `"ReversalOfJournalId" IS NOT NULL AND "IsDelete" = false` |
| `CreateIndex` unique | `IX_AccEventType_EventTypeCode` (`IsDelete = false`); `IX_AccPostingRule_LegalEntityId_EventTypeId` (`IsActive = true AND IsDelete = false`); `IX_AccPostingRuleLine_PostingRuleId_LineNumber` (`IsDelete = false`) |
| `CreateIndex` biasa | `AccEventType`: `EventTypeName`, `(IsActive, IsDelete)`; `AccPostingRule`: `EventTypeId`, `JournalTypeId`; `AccPostingRuleLine`: `AccountId`, `ComponentCode`, `CostCenterId` |

### 3.2 `Down`

`DropTable` `AccPostingRuleLine`, `AccPostingRule`, `AccEventType`; `DropIndex` lalu `CreateIndex`
`IX_AccJournal_ReversalOfJournalId` sebagai index biasa tanpa filter — keadaan sebelum `031`.

### 3.3 Snapshot

| Pemeriksaan | Hasil |
| --- | --- |
| Diff snapshot pada `12b8af63` | **+294 / −1** baris. Satu-satunya baris yang dihapus: `b.HasIndex("ReversalOfJournalId");` — digantikan versi unique berfilter |
| Blok entity baru | Hanya `AccEventType`, `AccPostingRule`, `AccPostingRuleLine` (beserta blok relasinya) |
| `b.ToTable(` | 615 → 618 pada `12b8af63`; **619** sesudah merge `ba124bbb` (+`BbkBloodUnitAllocation` dari integration) |
| Tabel `Acc*` di snapshot | 15 — 12 lama + 3 baru |
| Konsistensi model ↔ snapshot sesudah merge kedua | **Terbukti tidak langsung:** EF Core 9.0.18 menolak `database update` bila model berbeda dari snapshot; update sesudah `ba124bbb` berhasil sampai migration terakhir |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task migration. Endpoint yang kini dapat dipanggil: grup Event Type
(`BE-ACC-P2-017`) dan Posting Rule (`BE-ACC-P2-018`).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pembacaan `20260914044507_AddAccountingPostingRuleMaster.cs` | Tepat 3 `CreateTable` `Acc*`, 1 pasang `DropIndex`/`CreateIndex` `AccJournal`, 10 `CreateIndex` lain; nol operasi pada tabel non-`Acc*` | `PASS` | Bagian 3.1 |
| `git diff` snapshot | +294/−1, deletion satu-satunya milik `AccJournal` | `PASS` | Bagian 3.3 |
| `dotnet ef database update` oleh Rizki | Berhasil sesudah perbaikan Blood Bank | `PASS` | Laporan Rizki di sesi |
| `dotnet ef migrations list --no-build` oleh Rizki | Nol `(Pending)`; `20260914044507_AddAccountingPostingRuleMaster` tercatat diterapkan | `PASS` | Tangkapan layar Rizki, 14 Sep 2026 |
| `dotnet build … -p:RunAnalyzers=false` oleh Rizki | `0 error`, 192 warning; satu-satunya warning Accounting yang terlihat (`AccJournalService.cs(381)`) berasal dari `0d4ad3adf`, 3 Sep 2026 | `PASS` | Terminal Rizki |
| Query langsung `information_schema` atas ketiga tabel | Tidak dijalankan agent — eksekusi database di luar wewenang | `NOT RUN` | — |

### Migration Coordination Gate — diperiksa sesudah pembuatan, dari bukti git

| # | Pertanyaan gate | Jawaban |
| ---: | --- | --- |
| 1 | Modul paralel sudah membuat migration? | **Ya** — Blood Bank (`Bbk*`), Radiology (`AddRad*`), Billing (`AddTableDepositPolicy`, `AddTableMstPaymentMethodAcc…`), penomoran (`AddNumNumberSeries`), rename `Reg`/`Phm` |
| 2 | Nama migration-nya | `20260909015603` s.d. `20260913070556` — seluruhnya ada di `Migrations/` |
| 3 | Sudah commit dan push? | Ya — masuk lewat PR integration #137 dan #140 |
| 4 | Sudah merge ke integration baseline? | Ya — `53d8eedf` (sebelum migration Accounting dibuat), `ba124bbb` (sesudahnya) |
| 5 | Sudah diterapkan ke database pengembangan? | Ya — `QuilvianNewDevRizki`, oleh Rizki, 14 Sep 2026 |
| 6 | Snapshot lokal berasal dari baseline terbaru? | Ya pada saat pembuatan — selisih terhadap integration hanya blok `Acc*` Phase 2, nol deletion |
| 7 | SHA sumber migration | `53d8eedf` |

Catatan jujur: gate ini semestinya dijalankan **sebelum** `migrations add`. Di sini jawabannya disusun
sesudahnya dari bukti yang sama, dan tidak ada jawaban yang berubah karenanya.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Snapshot bertambah **tanpa satu pun deletion** blok modul lain | **Terpenuhi** | +294/−1; deletion satu-satunya adalah index `AccJournal` milik `031` |
| 2 | `Down` menghapus ketiga tabel | **Terpenuhi** | Bagian 3.2 |
| 3 | `CONTAMINATION GUARD` `CLEAN` — nol `DropTable`, nol perubahan tabel non-`Acc*` | **Terpenuhi** | Bagian 3.1 |

| Butir DoD | Hasil |
| --- | --- |
| Migration dibuat **dan** diterapkan Rizki | **Ya** — `12b8af63`; `migrations list` tanpa `(Pending)` |
| Snapshot nol deletion | **Ya** |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 192 warning build, tidak ada yang berasal dari berkas batch Accounting yang terlihat di keluaran |
| Masalah yang diketahui | `Designer.cs` migration ini (108.098 baris) tidak memuat `BbkBloodUnitAllocation` karena migration Blood Bank itu masuk sesudahnya; normal untuk migration paralel dan tidak mempengaruhi eksekusi |
| Risiko tersisa | Migration ini **belum ada di integration**. Selama belum di-merge ke sana, modul lain yang membuat migration dari baseline integration akan membangun snapshot tanpa tiga tabel ini — pola `ACC-DEP-001`. Segera ajukan PR |
| Temuan lintas modul | `20260910153119_AddBbkBloodOrder` (Blood Bank) semula merujuk `TrxPatientEncounter` padahal migration rename `20260910031500` yang ber-ID lebih awal sudah menggantinya — database yang menerapkan urutan ID gagal `42P01`. **Sudah diperbaiki di integration** `b3361d07` (14 Sep 2026). Tidak diubah agent |
| Perubahan sampingan | `NONE` |
| Interupsi | `database update` sempat gagal di migration Blood Bank; dipulihkan lewat perbaikan integration, bukan lewat sunting lokal |
| Status Git | Working tree bersih pada `ba124bbb` sebelum laporan ini ditulis. **Agent nol commit, push, merge, dan migration** |
| Langkah berikutnya | (1) PR `rizkiG` → integration supaya migration ini masuk baseline. (2) Jalankan backend, lalu uji panggil `GET /event-types` dan `GET /posting-rules` untuk menutup `BE-ACC-P2-017`. (3) Beri hak `EventType`/`PostingRule` di Akses Role |
