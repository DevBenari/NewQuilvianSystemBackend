# Nilai Aset — Roadmap Backend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` — satu task ⛔ menunggu gerbang lintas modul |
| Blueprint / contract | `AST-BP-001` rev 1, `nilai`, `AST-NLI-1.0.0` `approved` 2026-11-12 |
| Backend SHA | `1f54a175` (`rizkiG`) |
| Task | `BE-AST-014`, `BE-AST-015` (gelombang `MVP-0`, dibutuhkan `register`) dan `BE-AST-025` s.d. `BE-AST-032` (`MVP-4`) |
| Handoff | `build-module-backend`, satu task per pemanggilan, wewenang `TASK MODE: BACKEND` eksplisit. QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend dan `docs/engineering/` |

**Legenda tanda status:** ✅ selesai dan terbukti · 🟡 sebagian · ⛔ terblokir (blocker disebut) · tanpa tanda = belum dikerjakan.

`V-BE` dan `DoD-BE` sama dengan `../../register/roadmap/backend-roadmap.md`; skenario proses bisnis
`AT-NLI-*`.

**Gerbang `GATE-AST-ACC`** (pemilik: Rizki selaku pemilik Akuntansi) terpenuhi bila keempatnya ada:
`DEP-AST-01` kejadian keuangan membawa pusat biaya (task modul Akuntansi); `DEP-AST-02` enam jenis kejadian
dan aturan posting terdaftar (`AST-OQ-009`); `DEP-AST-03` template jurnal berulang penyusutan dinonaktifkan
mulai bulan penyalaan (`AST-UNK-002`); `DEP-AST-04` akun layanan pengirim berizin `AccountingEvent : Receive`.
Rincian: `../contracts/integration-contract.md` bagian 5. Gerbang ini **tidak** menahan pembangunan task lain;
kejadian menumpuk `Held` sampai dinyalakan.

## Grafik Urutan Dependency

Dipotong menjadi dua blok karena lebar; `BE-AST-027` muncul lagi sebagai sambungan blok kedua.

```text
BE-AST-002 [REG] ─> BE-AST-014 ─┬─> BE-AST-015 ─> BE-AST-025 ─> BE-AST-026 ─> BE-AST-027
                                │
BE-AST-005 [REG] ───────────────┘
```

```text
BE-AST-027 ─┬─> BE-AST-031
            │
            ├─> BE-AST-029 ─────┐
            │                   │
            ├─> BE-AST-030 ───┐ │
            │                 │ │
            └─> BE-AST-028 ───┴─┴─┬─> BE-AST-032 ⛔
                                  │
                {GATE-AST-ACC ⛔} ┘
```

`[REG]` = task backend sub-modul `register` pada `../../register/roadmap/backend-roadmap.md`, cermin baca-saja.

Pasangan prasyarat→task: **14** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-AST-002` [REG] | `BE-AST-014` |
| 2 | `BE-AST-014`, `BE-AST-005` [REG] | `BE-AST-015` |
| 3 | `BE-AST-015` | `BE-AST-025` |
| 4 | `BE-AST-025` | `BE-AST-026` |
| 5 | `BE-AST-026` | `BE-AST-027` |
| 6 | `BE-AST-027` | `BE-AST-028`, `BE-AST-029`, `BE-AST-030`, `BE-AST-031` — boleh paralel |
| — | ⛔ menunggu `GATE-AST-ACC` dan `BE-AST-028`, `029`, `030` | `BE-AST-032` |

## Slice

| Slice | Gelombang MVP | Task | Status |
|---|---|---|---|
| `NLI-S1` Tabel dasar, parameter, buku nilai, kotak keluar | `MVP-0` | `BE-AST-014`, `BE-AST-015` | belum dikerjakan |
| `NLI-S2` Penyusutan, perubahan nilai, kiriman, rekonsiliasi, laporan | `MVP-4` | `BE-AST-025` s.d. `BE-AST-031` | belum dikerjakan |
| `NLI-S3` Penyalaan integrasi Akuntansi | `MVP-4` | `BE-AST-032` | ⛔ `GATE-AST-ACC` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-AST-014` | Lima tabel dasar nilai terdefinisi | `AST-DES-003` | data dict §1–3, §4.3 | `IdentityModel` | `Valuation/{Models,Enums}`, configuration, DbSet | `BE-AST-002` | Kartu | `V-BE` | Masuk migration `BE-AST-004` [REG] | `DoD-BE` |
| `BE-AST-015` | Parameter, buku nilai, dan penulis kotak keluar siap dipakai register | `CAP-EXT-02`, `CAP-EXT-04`, `AST-DEC-027`, `040`; `AST-DES-032`, `033` | API 2.1–2.2 | — | 3 service + 1 controller | `BE-AST-014`, `BE-AST-005` | Kartu | `V-BE`, `AT-NLI-07`, `08`, `13`, `33`, `34` | Angka parameter `AST-OQ-001..004` (data) | `DoD-BE` |
| `BE-AST-025` | Lima tabel proses + migration kedua | `AST-DES-035`, `011` | data dict §2.3, §4 | — | 5 model, configuration, `AddAssetValuationProcessing` | `BE-AST-015` | Kartu | `V-BE` | — | `DoD-BE` |
| `BE-AST-026` | Akun kontrol dan pemetaan pusat biaya dapat ditetapkan | `AST-DEC-036`; `AST-DES-011`, `035` | API 2.3–2.4 | `MstCostCenter`, daftar akun Akuntansi (baca) | 1 controller + service | `BE-AST-025` | Kartu | `V-BE`, `AT-NLI-32` | — | `DoD-BE` |
| `BE-AST-027` | Penyusutan bulanan diproses sekali, berurutan, per pusat biaya | `CAP-09.4`, `AST-DEC-013`, `014`, `039`, `042`; `AST-DES-009`, `010` | API 2.6, state §2 | — | `DepreciationCalculator`, `AssetDepreciationRunService`, controller | `BE-AST-026` | Kartu | `V-BE`, `AT-NLI-01..05`, `09`, `30..32`, `37`, `41` | — | `DoD-BE` |
| `BE-AST-028` | Perubahan nilai empat jenis | `CAP-09.3`, `AST-DEC-016`, `030`, `041`, `042` | API 2.7, state §3–4 | Pola maker-checker Finance | `AssetValueChangeService`, controller | `BE-AST-027` | Kartu | `V-BE`, `AT-NLI-05`, `06`, `14`, `15`, `35`, `36` | — | `DoD-BE` |
| `BE-AST-029` | Pengirim kejadian ke Akuntansi, bawaan mati | `AST-DEC-015`, `038`; `AST-DES-034` | API 2.8, state §5 | Pola `FinanceAccountingDispatchWorker` | Worker, controller kotak keluar, opsi | `BE-AST-027` | Kartu | `V-BE`, `AT-NLI-11`, `12`, `38`, `39` | Kredensial dari konfigurasi | `DoD-BE` |
| `BE-AST-030` | Saldo subledger per akun kontrol terkirim | `CAP-09.5`, `AST-DES-035` | API 2.9 | Rekonsiliasi control account Akuntansi | `AssetSubledgerSnapshotService`, controller | `BE-AST-027` | Kartu | `V-BE`, `AT-NLI-10` | — | `DoD-BE` |
| `BE-AST-031` | Nilai buku, harga perolehan, laporan nilai | `CAP-09.1`, `09.2`, `11.6` | API 2.5, 2.10 | — | `AssetValuationQueryService`, 2 controller | `BE-AST-027` | Kartu | `V-BE`, `AT-NLI-40` | — | `DoD-BE` |
| `BE-AST-032` ⛔ | Integrasi Akuntansi terbukti dan dinyalakan per kode | `AST-DEC-038`, `047`; `DEP-AST-01..04` | integration §3–5 | — | Konfigurasi penyalaan, daftar periksa, bukti jurnal | `BE-AST-028`, `BE-AST-029`, `BE-AST-030`, `{GATE-AST-ACC}` | Kartu | `AT-NLI-16`; `UAT-NLI-05`, `08` | Rizki (Akuntansi) | `DoD-BE` |

## Kartu task

### `BE-AST-014` — Tabel dasar nilai

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AstValuationParameter`, `AstDepreciationPolicy`, `AstAssetBook`, `AstAssetValueEntry`, `AstAccountingOutbox`; enum `AssetBookStatus`, `ValueEntryType`, `AccountingOutboxStatus`; configuration `Repositories/Configurations/Corporate/AssetManagement/Valuation/` |
| Acceptance criteria | 1. Kolom sesuai data dict. 2. Unik `(LegalEntityId, EffectiveFromPeriod)`, `(AssetGroupId, EffectiveFromPeriod)`, `AssetId` pada buku, `(SourceTransactionId, SourceVersion)`. 3. Check constraint nilai buku ≥ residu. 4. Tidak membuat migration sendiri — masuk `BE-AST-004` [REG] |

### `BE-AST-015` — Parameter, kebijakan, buku nilai, penulis kotak keluar

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetValuationParameterService`/`QueryService`, `AssetBookService` (`CreateForRegisteredAsset`, `CreateOpeningBalance`, `GetNetBookValue`), `AssetAccountingEventService` (penulisan baris + status `Held` per kode), `AssetValuationParametersController` bagian 2.1–2.2 |
| Acceptance criteria | 1. Baris pertama parameter boleh bulan berjalan, berikutnya bulan depan. 2. Kebijakan kelompok berlaku ke depan bagi aset tanpa estimasi sendiri. 3. Buku: dikapitalisasi bila harga ≥ batas **dan** kepemilikan dapat dikapitalisasi; mulai penyusutan bulan sesudah siap pakai; saldo awal menghitung bulan yang sudah disusutkan. 4. Kejadian hibah ditulis `Held`/`Pending` sesuai konfigurasi kode. 5. Persetujuan aset tanpa parameter → `AST-VAL-NLI-008` |

### `BE-AST-025` — Tabel proses dan migration `AddAssetValuationProcessing`

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AstDepreciationRun`, `AstDepreciationRunLine`, `AstValueChangeRequest`, `AstSubledgerAccountSetup`, `AstUnitCostCenter`; enum `ValueChangeType`, `ValueChangeStatus`, `DepreciationAdjustmentReason`; migration urutan 4 |
| Acceptance criteria | 1. Migration hanya menambah lima tabel. 2. Unik `(LegalEntityId, Period)` pada proses. 3. Unik parsial satu pengajuan terbuka per aset |

### `BE-AST-026` — Akun kontrol dan pusat biaya

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Endpoint 2.3–2.4. 2. Kode akun divalidasi terhadap daftar akun Akuntansi (baca saja). 3. Daftar unit "belum jelas" = unit tanpa pemetaan dengan 0 atau > 1 pusat biaya aktif |

### `BE-AST-027` — Pratinjau dan proses penyusutan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Rumus `AST-DES-009`; contoh monitor menghasilkan Rp 1.416.667 dan bulan ke-60 menutup tepat. 2. Penyesuaian terlambat dicatat, pemulihan, koreksi harga. 3. Pusat biaya unit pada akhir bulan. 4. Sekali per periode, berurutan, satu transaksi. 5. Kotak keluar satu baris per (kelompok, pusat biaya). 6. Aset yang dihapus pada periode itu tidak disusutkan |

### `BE-AST-028` — Perubahan nilai

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Empat jenis dengan akibat per jenis (state §4). 2. Hanya penurunan nilai menulis kejadian. 3. Penyetuju Kepala Akuntansi bukan pengaju. 4. Linimasa `NilaiBerubah` lewat `AssetHistoryService` |

### `BE-AST-029` — Pengirim kejadian

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. `AssetManagement:AccountingDispatch:Enabled=false` bawaan; nol HTTP saat mati. 2. Kode di luar `EnabledEventCodes` tetap `Held` tanpa menambah percobaan. 3. 200/201 → `Acknowledged`; gagal ke-`MaxAttempts` → `Failed`; coba ulang manual. 4. `LastError` tanpa kredensial dan tanpa payload |

### `BE-AST-030` — Saldo subledger dan rekonsiliasi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Satu kejadian per akun kontrol per periode sesudah proses. 2. Kirim ulang membuat versi baru. 3. Kelompok tanpa akun kontrol tercatat sebagai peringatan |

### `BE-AST-031` — Buku nilai dan laporan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Endpoint 2.5, 2.10 dengan saringan periode. 2. Hanya peran `AssetBook : Read`. 3. Ekspor sama dengan tampilan |

### ⛔ `BE-AST-032` — Verifikasi integrasi Akuntansi dan penyalaan per kode

| Field | Isi |
|---|---|
| **Status** | ⛔ **TERBLOKIR** — menunggu `GATE-AST-ACC` (`DEP-AST-01..04`), pemilik Rizki selaku pemilik Akuntansi |
| Cakupan | Daftar periksa penyalaan per kode; pengaturan `EnabledEventCodes`; bukti jurnal di Akuntansi (pusat biaya benar, tidak dobel); rekonsiliasi satu periode |
| Acceptance criteria | 1. Kejadian `AST-PENYUSUTAN` menghasilkan jurnal beban dengan pusat biaya dari kejadian. 2. Tidak ada jurnal berulang penyusutan aktif pada periode penyalaan. 3. Saldo subledger tampil di rekonsiliasi Akuntansi. 4. Kode lain dinyalakan satu per satu dengan bukti |
