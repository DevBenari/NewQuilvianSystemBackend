# Laporan Perubahan Backend — `BE-RWI-202`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-202` |
| Judul | Pelunasan Deposit dan peringatan jatuh tempo |
| Slice | Slice B; gelombang `RWA-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-202` |
| Trace | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263`; `RWI-AC-366`, `367`, `381`, `382`, `385`; G-45; validation `VAL-RWA-11`, `12`, `14`, `19`, `25`; data 20.10; flowchart `08` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-194` ✅, `BE-RWI-195` ✅ |
| Klasifikasi | `HEAVY` — angka uang, zona waktu, jatuh tempo |
| Task mode | `BACKEND` |
| Target tulis | `InpDepositDueDateCalculator.cs`, `InpAdmissionDocumentService*.cs`, `InpAdmissionPrintService.cs`, `InpAdmissionCompletenessEvaluator.cs`, `InpAdmissionWorkspaceQueryService.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi dengan Billing dan jam dipalsukan **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`; angka dibaca dari Billing lewat `IInpBillingDepositAdapter` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-DTO-001` |
| Wewenang | Source: ya. Tidak ada perubahan Billing |

---

## 1. Kebutuhan

Surat pelunasan deposit memakai angka Billing, tidak pernah diketik (`INV-RWA-06`). Jatuh temponya
berbatas kebijakan, angkanya dibekukan saat dikunci, dan petugas diperingatkan bila jatuh tempo
terlewati sementara kekurangan masih ada.

## 2. Proses bisnis

1. **Syarat buat dan kunci.** Hanya bila Billing mencatat kekurangan > 0 (`422 INP-ADM-DOC-011`).
   Billing gagal dibaca → `422 INP-ADM-DOC-012`.
2. **Angka.** Selama `Draft`, angka dibaca hidup dari Billing. Saat dikunci, minimum, diterima, dan
   kekurangan dibekukan beserta waktu bacanya; ringkasan header tetap hidup. Teks perhitungan:
   "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)".
3. **Jatuh tempo bawaan.** Hari kerja Senin–Jumat berikutnya sesudah tanggal surat, pukul 11.00 waktu
   rumah sakit, dibatasi tanggal surat + `FollowUpIntervalDays`. Contoh:
   - Jumat 9 Oktober 2026, interval 3 → Senin 12 Oktober 11.00 WIB; memilih 13 Oktober →
     `422 INP-ADM-DOC-019` "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit.";
   - interval 1 → Sabtu 10 Oktober;
   - Kamis 8 Oktober, interval 3 → Jumat 9 Oktober;
   - interval 0 → tanggal surat.
   Jatuh tempo sebelum tanggal surat ditolak.
4. **Rupiah dan cetak.** Rupiah hanya lewat `ViewAmount`: `GET /documents/{id}/amounts`, dan
   `GET /documents/{id}/amount-print`, yang juga memeriksa `Print` di service (`403 INP-ADM-PRT-003`).
   Cetak berupiah lewat jalur tanpa rupiah → `422 INP-ADM-PRT-002`.
5. **Peringatan.** Surat `Completed` yang jatuh temponya terlewati sementara Billing masih mencatat
   kekurangan:
   - `/summary/amounts` (berupiah) memuat `OverdueStatement`;
   - Detail Episode dan `/summary` memuat "Pelunasan deposit jatuh tempo *tgl jam* terlewati — lihat
     kasir" tanpa rupiah.
6. **Pihak.** Pihak bersumber relasi atau kontak darurat dibaca ulang dari service pemilik saat simpan
   dan kunci.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`IInpBillingDepositAdapter`/`InpEpisodeDepositSummaryDto`, validation `VAL-RWA-11`, `12`, `19`, `25`, flowchart `08`, keputusan `RWI-DEC-231`/`248`/`261`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpDepositDueDateCalculator.cs` | Baru — `NextWorkingDay`, `MaxDueDate`, `DefaultDueDate`, `ToDueAtUtc`, `Validate` |
| `Services/InpAdmissionDocumentService.cs` | `CheckDepositShortfall` saat buat dan kunci; pembekuan angka saat kunci; `GetAmountsAsync`; `BuildDepositAmounts` |
| `Services/InpAdmissionDocumentService.Content.cs` | `ApplyDeposit` (jatuh tempo bawaan dan validasi `019`); validasi kunci `025` |
| `Services/InpAdmissionPrintService.cs` | Jalur `amount-print` (`PRT-002`, `PRT-003`); rupiah hanya pada jalur berupiah |
| `Services/InpAdmissionCompletenessEvaluator.cs` | Jatuh tempo terlewati |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `OverdueStatement` pada `/summary/amounts` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `/documents/{id}/amounts`, `/documents/{id}/amount-print`, `/summary/amounts` |
| Database | `InpAdmissionDepositStatement` (`numeric(18,2)`) |
| Keamanan/Auth | Angka keuangan hanya `ViewAmount`; cetak berupiah butuh `ViewAmount` dan `Print` |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/documents/{documentId}/amounts` | Angka hidup (`Draft`) atau beku | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `…/admission-workspace/documents/{documentId}/amount-print` | Data cetak berupiah | `: ViewAmount` + `Print` di service |
| `GET` | `…/admission-workspace/summary/amounts` | Status deposit dan surat terlewati | `: ViewAmount` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Contoh jatuh tempo kartu (Jumat interval 3/1/0, Kamis interval 3) | Sesuai rumus `DefaultDueDate = min(NextWorkingDay, tanggal + interval)` | `PASS` (review rumus) | `InpDepositDueDateCalculator.cs` |
| Tidak ada angka di request | Request deposit hanya `DueDate?` | `PASS` | DTO |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Proses bisnis dengan Billing dan jam dipalsukan | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Rp 5.000.000 − Rp 2.000.000 → Rp 3.000.000 dengan teks perhitungan | Terpenuhi (source) | `BuildDepositAmounts` |
| 2. Kekurangan 0 → `011`; Billing gagal → `012` | Terpenuhi (source) | `CheckDepositShortfall` |
| 3. Contoh tanggal Jumat/Kamis dan interval 3/1/0; 13 Oktober → `019` | Terpenuhi (source) | `InpDepositDueDateCalculator` |
| 4. Angka beku saat kunci; ringkasan tetap hidup | Terpenuhi (source) | `LockAsync`; `GetAmountsAsync` |
| 5. Lewat jatuh tempo + kekurangan → peringatan; detail episode tanpa rupiah | Terpenuhi (source) | Evaluator; `BuildWarnings` |
| 6. Cetak berupiah butuh `ViewAmount` dan `Print` | Terpenuhi (source) | Atribut + `AccessPermissionService` (`PRT-003`) |
| 7. Pihak bersumber relasi dibaca ulang dari service pemilik | Terpenuhi (source) | `ResolvePartyAsync`, `RefreshPartyFromSourceAsync` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Jatuh tempo memakai zona waktu profil rumah sakit (bawaan `Asia/Jakarta`); disimpan UTC (11.00 WIB = 04.00Z) |
| Masalah yang diketahui | — |
| Risiko tersisa | Angka bergantung ketersediaan Billing; kegagalan selalu menolak, tidak menebak |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` `InpDepositDueDateCalculator.cs` dan berkas Workspace PPRI bersama |
| Langkah berikutnya | Uji pengguna dengan Billing; Yasmina meninjau angka (pemilik Billing) |
