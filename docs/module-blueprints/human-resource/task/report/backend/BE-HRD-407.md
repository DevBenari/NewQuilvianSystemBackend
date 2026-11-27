# Laporan Perubahan Backend — `BE-HRD-407`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-407` |
| Judul | Maker-checker dan guard scan SK |
| Slice | `S-FIN-M07` / Perpindahan Unit (alias `FIN-HRD-07-BE-001`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-407` |
| Trace | `HRD-DEC-031`, `HRD-DEC-056`; instruksi Menu 07 `FIN-HRD-07-BE-001` |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdPersonnelActionOrderService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Larangan pembuat/pengaju menyetujui SK dan kewajiban scan bertanda tangan sebelum pengajuan dan persetujuan sudah ada di `HrdPersonnelActionOrderService`. Celah: **dua persetujuan paralel** untuk SK yang sama sama-sama lolos pemeriksaan status `PendingApproval` (dibaca sebelum transaksi), sehingga penempatan baru dapat diterapkan dua kali dan jejak penyetuju tertimpa.

## 2. Proses bisnis

1. HR membuat draf SK dan mengunggah scan bertanda tangan.
2. Pengaju mengajukan; tanpa scan → ditolak, status tidak berubah.
3. Penyetuju (bukan pembuat, bukan pengaju) menyetujui. Di dalam transaksi diambil kunci per SK lalu status dibaca ulang; bila SK sudah diproses petugas lain → ditolak dengan pesan "muat ulang data".
4. SK berlaku hari ini → `Applied` dan penempatan diterapkan; berlaku nanti → `Scheduled`.

**Contoh.** Dua kepala SDM menekan Setujui pada detik yang sama: satu berhasil, satu menerima "SK sudah diproses oleh petugas lain".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdPersonnelActionOrderService.cs` (`SubmitOrderAsync`, `ApproveOrderAsync`, `ApplyOrderAssignmentsAsync`), controller SK.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` | Kunci `pg_advisory_xact_lock` per SK + baca ulang status di `ApproveOrderAsync`; helper `AcquireTransactionLockAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; persetujuan kedua yang bersamaan kini ditolak (`InvalidOperationException` → kode galat controller existing) |
| Database | Tanpa migration |
| Keamanan/Auth | Hak tidak berubah |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Personnel Action Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/approve` | Setujui SK tepat sekali | hak existing `HrdPersonnelActionOrder` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: pembuat menyetujui | Ditolak (SoD) | `PASS` | `ApproveOrderAsync` |
| Penelusuran: SK tanpa scan diajukan/disetujui | Ditolak, status tetap | `PASS` | guard `HRD-DEC-056` |
| Penelusuran: dua persetujuan serentak | Satu berhasil, satu ditolak | `PASS` | kunci per SK + baca ulang |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pembuat/pengaju ditolak saat approve | Terpenuhi | guard SoD |
| (2) Scan kosong tidak mengubah status | Terpenuhi | guard scan di submit dan approve |
| (3) Dua approval paralel hanya berlaku sekali | Terpenuhi | kunci per SK + baca ulang status |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-408`, `BE-HRD-409` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
